using Dapper;
using EntityCoreVersusDapper.Database;
using EntityCoreVersusDapper.Models;
using HowToDevelop.Utils.Docker.Artifacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EntityCoreVersusDapper
{
    internal class Program
    {
        protected Program()
        {

        }

        private static TesteEtapa _resultadosEtapaPrimeirosRegistros;
        private static TesteEtapa _resultadosEtapaTodosRegistros;
        private static TesteEtapa _resultadosEtapaTodosRegistrosComJoin;

        private static TesteSettings _testeSettings;
        private static SqlServerDockerSettings _dockerSettings;

        static void Main(string[] args)
        {
            LerConfiguracoes();
            using var databaseHelper = new DatabaseHelper(_dockerSettings, _testeSettings);
            InicializarBancoDeDados(databaseHelper);
            
            do
            {
                ConfigurarEtapas();
                Dapper(databaseHelper);
                EfCore(databaseHelper);
                MostrarResultadosPorEtapa();
                Console.WriteLine("Continua? (S/N)");
            } while (Console.ReadLine().Equals("S", StringComparison.CurrentCultureIgnoreCase));
        }

        #region Configuração
        static void LerConfiguracoes()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _dockerSettings = configuration.GetSection("SqlServerDockerSettings").Get<SqlServerDockerSettings>()
                              ?? SqlServerDockerSettings.Default;
            _testeSettings = configuration.GetSection(nameof(TesteSettings)).Get<TesteSettings>()
                             ?? TesteSettings.Default;
        }
        static void ConfigurarEtapas()
        {
            long totalRegistros = _testeSettings.QuantidadeDeCategorias * _testeSettings.QuantidadeDeBlogsPorCategoria;

            _resultadosEtapaPrimeirosRegistros = new TesteEtapa(
                $"Primeira Etapa: Obtendo apenas {_testeSettings.QuantidadeDePrimeirosRegistros} Registros primeiros registros.");
            _resultadosEtapaTodosRegistros = new TesteEtapa(
                $"Segunda Etapa: Obtendo todos os Registros da tabela blogs. {totalRegistros} Registros");
            _resultadosEtapaTodosRegistrosComJoin = new TesteEtapa(
                $"Terceira Etapa: Obtendo todos os Registros da tabela blogs juntando com as tabelas BlogCategory e Category. {totalRegistros} Registros");
        }
        static void InicializarBancoDeDados(DatabaseHelper databaseHelper)
        {
            Console.Clear();
            Cabecalho("Criando Banco de Dados...");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            databaseHelper.InitializeAsync().Wait();
            stopwatch.Stop();
            long totalRegistros = _testeSettings.QuantidadeDeCategorias * _testeSettings.QuantidadeDeBlogsPorCategoria;
            Console.WriteLine($"Criado {totalRegistros} registros com vínculos");
            Console.WriteLine("Tempo de Criação com Entity " + stopwatch.Elapsed.TotalMinutes.ToString("0.00"));
            Console.WriteLine();
            PressioneQualquerTeclaParaContinuar();
        }
        #endregion

        #region Visualização
        static void Cabecalho(string mensagem)
        {
            Console.WriteLine("Entity Core Versus Dapper Fight ... ");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("========================================================================");
            Console.WriteLine($" {mensagem}");
            Console.WriteLine("========================================================================");
            Console.WriteLine();
        }
        static void PressioneQualquerTeclaParaContinuar()
        {
            Console.WriteLine("Pressione Qualquer Tecla Para Continuar...");
            Console.ReadLine();
        }
        static void ImprimeResultadosEntity(TesteEtapa testeEtapa)
        {
            foreach (var item in testeEtapa.ResultadosEntityCore)
            {
                Console.WriteLine($"| {item.Key:D2} | {item.Value.ConverterStringDuasCasasDecimais()} |");
            }
            Console.WriteLine();
            PressioneQualquerTeclaParaContinuar();
        }
        static void ImprimeResultadosDapper(TesteEtapa testeEtapa)
        {
            foreach (var item in testeEtapa.ResultadosDapper)
            {
                Console.WriteLine($"| {item.Key:D2} | {item.Value.ConverterStringDuasCasasDecimais()} |");
            }
            Console.WriteLine();
            PressioneQualquerTeclaParaContinuar();
        }

        static void MostrarResultados(TesteEtapa etapa)
        {
            Console.WriteLine(etapa.Descricao);
            Console.WriteLine();
            Console.WriteLine("| Teste | EF Core  | Dapper   | Diferença | Resultado       |");
            int j = 2;
            for (int i = 1; i < _testeSettings.QuantidadeDeTestes; i++)
            {
                string resultadoEf = etapa.ResultadosEntityCore[j]
                    .ConverterStringDuasCasasDecimais();
                string resultadoDapper = etapa.ResultadosDapper[j]
                    .ConverterStringDuasCasasDecimais();
                string diferenca = (etapa.ResultadosEntityCore[j] - etapa.ResultadosDapper[j])
                    .ConverterStringDuasCasasDecimais();
                string resultado = "Empate";
                if (Math.Round(etapa.ResultadosEntityCore[j],3) > Math.Round(etapa.ResultadosDapper[j],3))
                {
                    resultado = "Dapper Wins";
                } 
                else if (Math.Round(etapa.ResultadosEntityCore[j], 3) < Math.Round(etapa.ResultadosDapper[j], 3))
                {
                    resultado = "EF Core Wins";
                }

                Console.WriteLine($"| {j:D2}    | {resultadoEf,-8} | {resultadoDapper,-8} | {diferenca, -9} | {resultado,-15} |");
                j++;
            }
            Console.WriteLine();

        }

        static void MostrarResultadosPorEtapa()
        {
            Console.Clear();
            Cabecalho("Comparando os Resultados");
            MostrarResultados(_resultadosEtapaPrimeirosRegistros);
            MostrarResultados(_resultadosEtapaTodosRegistros);
            MostrarResultados(_resultadosEtapaTodosRegistrosComJoin);
            PressioneQualquerTeclaParaContinuar();
        }
        #endregion


        #region Dapper
        static void Dapper(DatabaseHelper databaseHelper)
        {
            Console.Clear();
            Cabecalho("Testando o Dapper");
            DapperPrimeiroPasso(databaseHelper);
            DapperSegundoPasso(databaseHelper);
            DapperTerceiroPasso(databaseHelper);
        }
        static void DapperPrimeiroPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine(_resultadosEtapaPrimeirosRegistros.Descricao);
            for (var i = 1; i <= _testeSettings.QuantidadeDeTestes; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _ = databaseHelper.Connection.Query<Blog>("SELECT TOP 10 [b].[Id], [b].[Article], [b].[Publish], [b].[Slug], [b].[Title] FROM [dbo].[Blogs] [b]");
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    _resultadosEtapaPrimeirosRegistros.ResultadosDapper.Add(i, stopwatch.Elapsed.TotalSeconds);
                }
                Console.WriteLine($"Dapper - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultadosDapper(_resultadosEtapaPrimeirosRegistros);
        }
        static void DapperSegundoPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine(_resultadosEtapaTodosRegistros.Descricao);
            for (var i = 1; i <= _testeSettings.QuantidadeDeTestes; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _ = databaseHelper.Connection.Query<Blog>("SELECT [b].[Id], [b].[Article], [b].[Publish], [b].[Slug], [b].[Title] FROM [dbo].[Blogs] [b]");
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    _resultadosEtapaTodosRegistros.ResultadosDapper.Add(i, stopwatch.Elapsed.TotalSeconds);
                }
                Console.WriteLine($"Dapper - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultadosDapper(_resultadosEtapaTodosRegistros);
        }
        static void DapperTerceiroPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine(_resultadosEtapaTodosRegistrosComJoin.Descricao);
            for (var i = 1; i <= _testeSettings.QuantidadeDeTestes; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var blogArticles = databaseHelper.Connection.Query<Blog, Category, Blog>(
                    @"SELECT [b].[Id], [b].[Article], [b].[Publish], [b].[Slug], [b].[Title], [c].[Id], [c].[Name], [c].[Slug]
                           FROM [dbo].[Blogs] AS[b]
                           LEFT JOIN(SELECT [bc].BlogId, [c].Id, c.[Name], c.[Slug]
                                       FROM [dbo].[BlogCategories] bc
                                       INNER JOIN [dbo].[Categories] c ON bc.CategoryId = c.Id
                            ) AS c ON c.BlogId = b.Id",
                    (blog, category) =>
                    {
                        blog.BlogCategories ??= new List<BlogCategory>();
                        blog.BlogCategories.Add(new BlogCategory { BlogId = blog.Id, CategoryId = category.Id, Category = category });
                        return blog;
                    }, splitOn: "CategoryId, Id"
                ).ToList();
                _ = blogArticles.GroupBy(p => p.Id).Select(g =>
                {
                    var groupedBlogArticle = g.First();
                    groupedBlogArticle.BlogCategories = g.Select(p => p.BlogCategories.Single()).ToList();
                    return groupedBlogArticle;
                }).ToList();
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    _resultadosEtapaTodosRegistrosComJoin.ResultadosDapper.Add(i, stopwatch.Elapsed.TotalSeconds);
                }
                Console.WriteLine($"Dapper - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultadosDapper(_resultadosEtapaTodosRegistrosComJoin);
        }

        #endregion

        #region Entity Framework Core
        static void EfCore(DatabaseHelper databaseHelper)
        {
            EfCorePrimeiroPasso(databaseHelper);
            EfCoreSegundoPasso(databaseHelper);
            EfCoreTerceiroPasso(databaseHelper);
        }
        static void EfCorePrimeiroPasso(DatabaseHelper databaseHelper)
        {
            Console.Clear();
            Cabecalho("Testando Entity Framework Core");
            Console.WriteLine(_resultadosEtapaPrimeirosRegistros.Descricao);
            for (var i = 1; i <= _testeSettings.QuantidadeDeTestes; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _ = databaseHelper.Context.Blogs.Take(10).ToList();
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    _resultadosEtapaPrimeirosRegistros.ResultadosEntityCore.Add(i, stopwatch.Elapsed.TotalSeconds);
                }
                Console.WriteLine($"EF Core - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultadosEntity(_resultadosEtapaPrimeirosRegistros);
        }
        static void EfCoreSegundoPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine(_resultadosEtapaTodosRegistros.Descricao);
            Console.WriteLine();
            for (var i = 1; i <= _testeSettings.QuantidadeDeTestes; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _ = databaseHelper.Context.Blogs.ToList();
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    _resultadosEtapaTodosRegistros.ResultadosEntityCore.Add(i, stopwatch.Elapsed.TotalSeconds);
                }
                Console.WriteLine($"EF Core - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultadosEntity(_resultadosEtapaTodosRegistros);
        }
        static void EfCoreTerceiroPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine(_resultadosEtapaTodosRegistrosComJoin.Descricao);
            Console.WriteLine();

            for (var i = 1; i <= _testeSettings.QuantidadeDeTestes; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _ = databaseHelper.Context.Blogs.Include("BlogCategories.Category").ToList();
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    _resultadosEtapaTodosRegistrosComJoin.ResultadosEntityCore.Add(i, stopwatch.Elapsed.TotalSeconds);
                }
                Console.WriteLine($"EF Core - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultadosEntity(_resultadosEtapaTodosRegistrosComJoin);
        }

        #endregion

    }
}
