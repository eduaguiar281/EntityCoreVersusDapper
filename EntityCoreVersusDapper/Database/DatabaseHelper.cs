using System;
using System.Data;
using Docker.DotNet;
using HowToDevelop.Utils.Docker.Artifacts;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EntityCoreVersusDapper.Database
{
    public class DatabaseHelper : IDisposable
    {
        private readonly IDockerClient _dockerClient = DockerClientBuilder.Build();
        private readonly DockerRegistries _dockerRegistries;
        private readonly SqlServerDockerSettings _settings;
        private readonly TesteSettings _testeSettings;
        private EfDbContext _context;

        public DatabaseHelper(SqlServerDockerSettings sqlServerDockerSettings, TesteSettings testeSettings)
        {
            _dockerRegistries = new DockerRegistries();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _settings = sqlServerDockerSettings;
            _testeSettings = configuration.GetSection(nameof(TesteSettings)).Get<TesteSettings>();
            _dockerRegistries.RegisterSqlServer2019(_dockerClient, _settings);
        }

        public EfDbContext Context => _context;

        public IDbConnection Connection => _context.Database.GetDbConnection();

        public async Task InitializeAsync()
        {
            await _dockerRegistries.RunAsync();
            _context = new EfDbContext(_settings.GetDatabaseConnectionString());
            await _context.CreateDatabase(_testeSettings.QuantidadeDeCategorias, _testeSettings.QuantidadeDeBlogsPorCategoria);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.CleanupTestsAndDropDatabaseAsync().Wait();
                }

                if (_dockerRegistries != null)
                {
                    _dockerRegistries.CleanAsync().Wait();
                }
            }
        }
    }
}
