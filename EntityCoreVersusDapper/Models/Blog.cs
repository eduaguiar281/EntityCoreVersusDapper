using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EntityCoreVersusDapper.Models
{
    public class Blog
    {
        public Blog()
        {
            BlogCategories = new List<BlogCategory>(1);
        }
        public virtual int Id { get; set; }

        public virtual string Title { get; set; }

        public virtual string Article { get; set; }

        public virtual DateTime? Publish { get; set; }

        public virtual string Slug { get; set; }
        public ICollection<BlogCategory> BlogCategories { get; set; }


        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>().HasKey(entity => entity.Id);

        }
    }
}
