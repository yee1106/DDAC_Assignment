using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DDAC_Assignment.Models;

namespace DDAC_Assignment.Data
{
    public class DDAC_AssignmentNewsDatabase : DbContext
    {
        public DDAC_AssignmentNewsDatabase (DbContextOptions<DDAC_AssignmentNewsDatabase> options)
            : base(options)
        {
        }

        public DbSet<DDAC_Assignment.Models.News> News { get; set; }

        public DbSet<DDAC_Assignment.Models.Category> Category { get; set; }

        public DbSet<DDAC_Assignment.Models.Advertisement> Advertisement { get; set; }

        public DbSet<DDAC_Assignment.Models.Comment> Comment { get; set; }
    }
}
