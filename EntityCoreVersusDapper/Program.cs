using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EntityCoreVersusDapper.Database;
using EntityCoreVersusDapper.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityCoreVersusDapper
{
    class Program
    {
        private const int TOTAL_TESTES = 6;
        private static List<string> resultadosPrimeirosDezEFCore = new List<string>();
        private static List<string> resultadosTodosBlogsEFCore = new List<string>();
        private static List<string> resultadosJoinCategoryEFCore = new List<string>();

        private static List<string> resultadosPrimeirosDezDapper = new List<string>();
        private static List<string> resultadosTodosBlogsDapper = new List<string>();
        private static List<string> resultadosJoinCategoryDapper = new List<string>();

        static void Main(string[] args)
        {
            using var databaseHelper = new DatabaseHelper();
            InicializarBancoDeDados(databaseHelper);
            EfCore(databaseHelper);
            Dapper(databaseHelper);
            MostrarTodosOsResultados();

        }

        static void Cabecalho(string mensagem)
        {
            Console.Clear();
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

        static void ImprimeResultados(List<string> resultados)
        {
            int i = 2;
            foreach (var item in resultados)
            {
                Console.WriteLine($"| {i:D2} | {item} |");
                i++;
            }
            Console.WriteLine();
            PressioneQualquerTeclaParaContinuar();
        }

        static void MostrarTodosOsResultados()
        {
            Cabecalho("Comparando os Resultados");
            Console.WriteLine("Primeiro Passo: Obtendo apenas 10 Registros primeiros registros.");
            Console.WriteLine();
            Console.WriteLine("| Teste | EF Core  | Dapper   |");
            int j = 2;
            for (int i = 1; i < TOTAL_TESTES; i++)
            {
                string itemEfCore = resultadosPrimeirosDezEFCore[i - 1];
                string itemDapper = resultadosPrimeirosDezDapper[i - 1];
                Console.WriteLine($"| {j:D2}    | {itemEfCore}    | {itemDapper}    |");
                j++;
            }
            Console.WriteLine();

            Console.WriteLine("Segundo Passo: Obtendo todos os Registros da tabela blogs.");
            Console.WriteLine();
            Console.WriteLine("| Teste | EF Core   | Dapper   |");
            j = 2;
            for (int i = 1; i < TOTAL_TESTES; i++)
            {
                string itemEfCore = resultadosTodosBlogsEFCore[i - 1];
                string itemDapper = resultadosTodosBlogsDapper[i - 1];
                Console.WriteLine($"| {j:D2}    | {itemEfCore}    | {itemDapper}    |");
                j++;
            }
            Console.WriteLine();

            Console.WriteLine("Terceiro Passo: Obtendo todos os Registros da tabela blogs juntando com as tabelas BlogCategory e Category.");
            Console.WriteLine();
            Console.WriteLine("| Teste | EF Core   | Dapper   |");
            j = 2;
            for (int i = 1; i < TOTAL_TESTES; i++)
            {
                string itemEfCore = resultadosJoinCategoryEFCore[i - 1];
                string itemDapper = resultadosJoinCategoryDapper[i - 1];
                Console.WriteLine($"| {j:D2}    | {itemEfCore}    | {itemDapper}    |");
                j++;
            }
            Console.WriteLine();
            PressioneQualquerTeclaParaContinuar();
        }

        #region Dapper
        static void Dapper(DatabaseHelper databaseHelper)
        {
            Cabecalho("Testando o Dapper");
            DapperPrimeiroPasso(databaseHelper);
            DapperSegundoPasso(databaseHelper);
            DapperTerceiroPasso(databaseHelper);
        }
        static void DapperPrimeiroPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine("Primeiro Passo: Obtendo apenas 10 Registros primeiros registros. Anote os resultados");
            for (var i = 1; i <= TOTAL_TESTES; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var blogs = databaseHelper.Connection.Query<Blog>("SELECT TOP 10 [b].[Id], [b].[Article], [b].[Publish], [b].[Slug], [b].[Title] FROM [dbo].[Blogs] [b]");
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    resultadosPrimeirosDezDapper.Add(stopwatch.Elapsed.TotalSeconds.ToString("0.000"));
                }
                Console.WriteLine($"Dapper - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultados(resultadosPrimeirosDezDapper);
        }
        static void DapperSegundoPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine("Segundo Passo: Obtendo todos os Registros da tabela blogs. Anote os resultados");
            for (var i = 1; i <= TOTAL_TESTES; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var blogs = databaseHelper.Connection.Query<Blog>("SELECT [b].[Id], [b].[Article], [b].[Publish], [b].[Slug], [b].[Title] FROM [dbo].[Blogs] [b]");
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    resultadosTodosBlogsDapper.Add(stopwatch.Elapsed.TotalSeconds.ToString("0.000"));
                }
                Console.WriteLine($"Dapper - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultados(resultadosTodosBlogsDapper);
        }
        static void DapperTerceiroPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine("Terceiro Passo: Obtendo todos os Registros da tabela blogs juntando com as tabelas BlogCategory e Category. Anote os resultados");
            for (var i = 1; i <= TOTAL_TESTES; i++)
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
                        blog.BlogCategories = blog.BlogCategories ?? new List<BlogCategory>();
                        blog.BlogCategories.Add(new BlogCategory { BlogId = blog.Id, CategoryId = category.Id, Category = category });
                        return blog;
                    }, splitOn: "CategoryId, Id"
                ).ToList();
                blogArticles = blogArticles.GroupBy(p => p.Id).Select(g =>
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
                    resultadosJoinCategoryDapper.Add(stopwatch.Elapsed.TotalSeconds.ToString("0.000"));
                }
                Console.WriteLine($"Dapper - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }
            ImprimeResultados(resultadosJoinCategoryDapper);
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
            Cabecalho("Testando Entity Framework Core");
            Console.WriteLine("Primeiro Passo: Obtendo apenas 10 Registros primeiros registros. Anote os resultados");
            for (var i = 1; i <= TOTAL_TESTES; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var blogs = databaseHelper.Context.Blogs.Take(10).ToList();
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    resultadosPrimeirosDezEFCore.Add(stopwatch.Elapsed.TotalSeconds.ToString("0.000"));
                }
                Console.WriteLine($"EF Core - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }

            ImprimeResultados(resultadosPrimeirosDezEFCore);

        }

        static void EfCoreSegundoPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine("Segundo passo: Obtendo todos os registros da tabela blog. Anote os resultados");
            Console.WriteLine();

            for (var i = 1; i <= TOTAL_TESTES; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var todosBlogs = databaseHelper.Context.Blogs.ToList();
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    resultadosTodosBlogsEFCore.Add(stopwatch.Elapsed.TotalSeconds.ToString("0.000"));
                }
                Console.WriteLine($"EF Core - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }

            ImprimeResultados(resultadosTodosBlogsEFCore);
        }

        static void EfCoreTerceiroPasso(DatabaseHelper databaseHelper)
        {
            Console.WriteLine("Terceiro passo: Obtendo todos os registros da tabela blog juntando com as tabelas BlogCategory e Category. Anote os resultados");
            Console.WriteLine();

            for (var i = 1; i <= TOTAL_TESTES; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var blogsComCategorias = databaseHelper.Context.Blogs.Include("BlogCategories.Category").ToList();
                stopwatch.Stop();

                if (i == 1)
                {
                    Console.WriteLine("Este primeiro resultado descartamos.");
                }
                else
                {
                    resultadosJoinCategoryEFCore.Add(stopwatch.Elapsed.TotalSeconds.ToString("0.000"));
                }
                Console.WriteLine($"EF Core - A query executou em {stopwatch.Elapsed.TotalSeconds:N3}");
            }

            ImprimeResultados(resultadosJoinCategoryEFCore);
        }

        #endregion
        
        static void InicializarBancoDeDados(DatabaseHelper databaseHelper)
        {
            Cabecalho("Criando Banco de Dados...");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            databaseHelper.InitializeAsync().Wait();
            stopwatch.Stop();
            Console.WriteLine("Criado 300.000 registros com vínculos");
            Console.WriteLine("Tempo de Criação com Entity " + stopwatch.Elapsed.TotalMinutes.ToString("0.00"));
            Console.WriteLine();
            PressioneQualquerTeclaParaContinuar();
        }
    }
}
