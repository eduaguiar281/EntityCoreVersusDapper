using EntityCoreVersusDapper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityCoreVersusDapper.Database.EntityTypeConfiguration
{
    public class BlogEntityTypeConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder.ToTable($"{nameof(Blog)}s");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Title).HasMaxLength(100);
            builder.Property(b => b.Article);
            builder.Property(b => b.Slug).HasMaxLength(150);
        }
    }
}
