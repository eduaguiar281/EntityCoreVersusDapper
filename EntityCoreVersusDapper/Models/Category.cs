using Microsoft.EntityFrameworkCore;

namespace EntityCoreVersusDapper.Models
{
    public class Category
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Slug { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasKey(entity => entity.Id);

            modelBuilder.Entity<Category>().Property(category => category.Name).HasMaxLength(200);
        }
    }
}