using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.db
{
    public class AppContext : DbContext
    { 


        public AppContext(string connectionString) : base(GetOptions(connectionString))
        {
        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseMySQL(connectionString);

            return builder.Options;
        }
    }
}
