using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeFinder.Models
{
    public class HomeContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public HomeContext(DbContextOptions<HomeContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
