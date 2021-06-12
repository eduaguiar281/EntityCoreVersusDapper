using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityCoreVersusDapper.Database.EntityTypeConfiguration;
using EntityCoreVersusDapper.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityCoreVersusDapper.Database
{
    public class EfDbContext : DbContext
    {
        private const int QUANTIDADE_CATEGORIAS = 10; //20
        private const int QUANTIDADE_BLOGS_POR_CATEGORIA = 1000; //15000

        private readonly string _connectionString;
        public EfDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<Category> Categories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BlogEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BlogCategoryEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryEntityTypeConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public async Task CreateDatabase()
        {
            await Database.EnsureCreatedAsync();
            await SeedData();
        }

        public async Task SeedData()
        {

            for (int i = 0; i < QUANTIDADE_CATEGORIAS; i++)
            {
                Category category = new()
                {
                    Name = $"Categoria {i:D3}",
                    Slug = $"categoria-{i:D3}"
                };
                Categories.Add(category);
                for (int j = 0; j < QUANTIDADE_BLOGS_POR_CATEGORIA; j++)
                {
                    Blogs.Add(new Blog()
                    {
                        Title = $"Tema do Blog {j:D5}",
                        Publish = DateTime.UtcNow,
                        Article = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
                                    Phasellus aliquam, libero quis sodales placerat, ex purus pulvinar elit, quis 
                                    vehicula urna eros ut urna. 
                                    Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. 
                                    Sed hendrerit turpis ex, eu semper urna auctor sit amet. Vestibulum viverra lacus vitae 
                                    placerat porta. Quisque dignissim quis ipsum faucibus semper. Curabitur ut consectetur erat. 
                                    Praesent eget diam gravida, lobortis erat eget, venenatis ante. Quisque convallis venenatis eros 
                                    sit amet consectetur. Donec at justo vitae enim dapibus tincidunt. Sed in efficitur orci. 
                                    Ut commodo congue nibh eu tempor. Maecenas massa risus, tincidunt eget nisl auctor, porta 
                                    accumsan orci. Aliquam pretium leo ut pellentesque accumsan. Duis ut libero malesuada, 
                                    facilisis tortor cursus, tristique ipsum.
                                    Phasellus sed urna ut nisi consectetur gravida dapibus mollis lorem. Donec ante nunc, 
                                    lacinia gravida ex in, pharetra condimentum leo. Mauris odio enim, pretium nec molestie id, 
                                    dictum eget felis. Pellentesque quis mi pulvinar, pulvinar ipsum eget, tempor neque. 
                                    Pellentesque lacinia suscipit tortor, placerat vestibulum erat dapibus a. 
                                    Curabitur imperdiet turpis in tellus scelerisque vestibulum. In non eros porta, auctor lorem at, 
                                    vulputate nibh. Curabitur pulvinar purus ut lacus lobortis malesuada.",
                        Slug = $"{category.Slug}-tema-do-blog-{j:D5}",
                        BlogCategories = new List<BlogCategory>()
                        {
                            new ()
                            {
                                Category = category
                            }
                        }
                    });
                }
            }

            await SaveChangesAsync();
        }


        public async Task CleanupTestsAndDropDatabaseAsync()
        {
            await Database.EnsureDeletedAsync();
        }

    }
}
