using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Reflection.Emit;


namespace EMP_WPF_FR
{
    class ApplicationContext : DbContext
    {


        public DbSet<Employee> Employees { get; set; }
        public DbSet<SuperUser> SuperUsers { get; set; }

        public DbSet<SeniorSalesman> SeniorSalesmans { get; set; }

        public DbSet<JuniorSalesman> JuniorSalesmans { get; set; }

        public DbSet<SeniorManager> SeniorManagers { get; set; }

        public DbSet<JuniorManager> JuniorManagers { get; set; }



        public ApplicationContext() : base("DefaultConnection") { }
    }
}
