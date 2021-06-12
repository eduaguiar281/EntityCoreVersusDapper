using EntityCoreVersusDapper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityCoreVersusDapper.Database.EntityTypeConfiguration
{
    public class BlogCategoryEntityTypeConfiguration : IEntityTypeConfiguration<BlogCategory>
    {
        public void Configure(EntityTypeBuilder<BlogCategory> builder)
        {
            builder.ToTable("BlogCategories");
            builder.HasKey(bc => bc.Id);
            builder.HasOne(bc => bc.Blog)
                .WithMany(b => b.BlogCategories)
                .HasPrincipalKey(b => b.Id)
                .HasForeignKey(bc=> bc.BlogId);

            builder.HasOne(bc => bc.Category)
                .WithMany()
                .HasPrincipalKey(c => c.Id)
                .HasForeignKey(bc => bc.CategoryId);

        }
    }
}
