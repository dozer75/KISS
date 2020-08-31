using System;
using Microsoft.EntityFrameworkCore;

namespace Foralla.KISS.Repository
{
    public class EFOptions
    {
        public Action<DbContextOptionsBuilder> OnConfiguring { get; set; }

        public Action<ModelBuilder> OnModelCreating { get; set; }
    }
}
