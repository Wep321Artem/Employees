using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.IO;
using System.Windows.Shapes;
using System.Reflection;
using System.Windows.Media.Media3D;
using System.Windows.Controls.Primitives;

namespace EMP_WPF_FR
{

    [Table("SuperUsers")]
    class SuperUser
    {
        public int SuperUserID { get; set; }
        public string FIO { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public SuperUser() { }

        public SuperUser(string FIO, string Login, string Password)
        {
            this.FIO = FIO;
            this.Login = Login;
            this.Password = Password;
        }
    }

    class User
    {
        public string FIO { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string DateWork { get; set; }

        public double Salary { get; set;}


        public User() { }

        public User(string FIO, string Login, string Password)
        {

            this.FIO = FIO;
            this.Login = Login;
            this.Password = Password;

        }

        public User(string FIO, string Login, string Password, string DateWork, double Salary, int ID_sen)
        {
            this.FIO = FIO;
            this.Login = Login;
            this.Password = Password;
            this.DateWork = DateWork;
            this.Salary = Salary;

        }

        public User(string FIO, string Login, string Password, string DateWork, double Salary)
        {
            this.FIO = FIO;
            this.Login = Login;
            this.Password = Password;
            this.DateWork = DateWork;
            this.Salary = Salary;

        }

        public int years(string DateWork)
        {
            DateTime DateWorkInit = DateTime.ParseExact(DateWork, "dd.MM.yyyy", null);
            DateTime today = DateTime.Today;
            int years = today.Year - DateWorkInit.Year;

            if (DateWorkInit > today.AddYears(-years))
            {
                years--;
            }

            return years;
        }

        // Расчёт финальной зарплаты пользователя
        public double resSalary(double Salary, string query, string DateWork, double ExperiencePrecent, double LimExperiencePrecent, double SubordinatesSalariesPrecent)
        {
            string dbPath = "Users.db";///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            string connectionString = $"Data Source = {dbPath};";
            double salary_jun = 0;


            
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                   
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salary_jun += reader.GetDouble(0);
                        }


                    }

                }

                connection.Close();

            }

            int years = this.years(DateWork);
            double SalaryLim = Salary + Salary * LimExperiencePrecent;
            Salary = Salary + Salary * (ExperiencePrecent * years); 
            if (SalaryLim > Salary) { return Salary + (salary_jun * SubordinatesSalariesPrecent); }
            return SalaryLim + (salary_jun * SubordinatesSalariesPrecent);
        }

        public string SalaryrequestForJun(string ExperiencePrecent, string LimExperiencePrecent, string SubordinatesSalariesPrecent = "0")
        {

            string query1 = $@"CAST((julianday('now') - julianday(
                            substr(jun.DateWork, 7, 4) || '-' || substr(jun.DateWork, 4, 2) || '-' || substr(jun.DateWork, 1, 2))) / 365.25 AS INTEGER)";


            string query2 = $@"CASE
                               WHEN(jun.Salary + jun.Salary * {LimExperiencePrecent}) > ( jun.Salary + jun.Salary * ({ExperiencePrecent}*{query1}))  
                               THEN (jun.Salary + jun.Salary * ({ExperiencePrecent}*{query1}))
                               ELSE (jun.Salary + jun.Salary * {LimExperiencePrecent})
                               END";

            return query2;
        }


        public virtual double FinalSalary(double Salary, string DateWork)
        {
            return Salary;
        }

        public virtual string FinalSalaryJun()
        {
            return "";
        }
    }

    [Table("Employees")]
    class Employee : User
    {
        public int EmployeeID { get; set; }

        public int JuniorManagerID { get; set; }

        public Employee() { }


        public Employee(int EmployeeID, string FIO, string Login, string Password, string DateWork, double Salary, int JuniorManagerID) : base(FIO, Login, Password, DateWork, Salary)
        {
            this.JuniorManagerID = JuniorManagerID;
            this.EmployeeID = EmployeeID;

        }
        public override double FinalSalary(double Salary, string DateWork)
        {
            int years = base.years(DateWork);
            double SalaryLim = Salary + Salary * 0.30;
            Salary = Salary + Salary * (0.03 * years);
            if (SalaryLim > Salary) { return Salary; }
            return SalaryLim;

        }
    }

    [Table("SeniorSalesmans")]
    class SeniorSalesman : User
    {
        public int SeniorSalesmanID { get; set; }

        public SeniorSalesman() { }

        public SeniorSalesman(int SeniorSalesmanID, string FIO, string Login, string Password, string DateWork, double Salary) : base(FIO, Login, Password, DateWork, Salary)
        {
            this.SeniorSalesmanID = SeniorSalesmanID;
        }

        public override double FinalSalary(double Salary, string DateWork) 
        {

            string query = $"SELECT DISTINCT {SalaryrequestForJun("0.01", "0.35")} FROM JuniorSalesmans as jun WHERE jun.SeniorSalesmanID = {SeniorSalesmanID}";
            double Result = base.resSalary(Salary, query, DateWork, 0.01, 0.35, 0.003);
            return Result;

        }

        public override string FinalSalaryJun()
        {
            string Result = base.SalaryrequestForJun("0.01", "0.35");

            return Result;
        }
    }

    [Table("JuniorSalesmans")]
    class JuniorSalesman : User
    {
        public int JuniorSalesmanID { get; set; }
        public int SeniorSalesmanID { get; set; }
        public JuniorSalesman() { }

        public JuniorSalesman(int JuniorSalesmanID, string FIO, string Login, string Password, double Salary, string DateWork, int SeniorSalesmanID) : base(FIO, Login, Password, DateWork, Salary)
        {
            this.SeniorSalesmanID = SeniorSalesmanID;
            this.JuniorSalesmanID = JuniorSalesmanID;

        }

        public override double FinalSalary(double Salary, string DateWork)
        {
            int years = base.years(DateWork);
            double SalaryLim = Salary + Salary * 0.35;
            Salary = Salary + Salary * (0.01 * years);
            if (SalaryLim > Salary) { return Salary; }
            return SalaryLim;
        }
    }

    [Table("SeniorManagers")]
    class SeniorManager : User
    {
        public int SeniorManagerID { get; set; }

        public SeniorManager() { }

        public SeniorManager(int SeniorManagerID, string FIO, string Login, string Password, double Salary, string DateWork)
            : base(FIO, Login, Password, DateWork, Salary)
        {
            this.SeniorManagerID = SeniorManagerID;
        }

        public string JualyDay(string post)
        {
            return $@"CAST((julianday('now') - julianday(
                            substr({post}.DateWork, 7, 4) || '-' || substr({post}.DateWork, 4, 2) || '-' || substr({post}.DateWork, 1, 2))) / 365.25 AS INTEGER)";


        }

        public override double FinalSalary(double Salary, string DateWork)
        {
            // Запрос для получения конечных зарплат всех подчиненных JuniorManagers
            string query = $@"
            SELECT 
                CASE
                    WHEN (jun.Salary + jun.Salary * 0.40) > (jun.Salary + jun.Salary * (0.05 * {JualyDay("jun")}))
                    THEN (jun.Salary + jun.Salary * (0.05 * {JualyDay("jun")}))
                    ELSE (jun.Salary + jun.Salary * 0.40)
                END +
                (SELECT COALESCE(SUM(
                    CASE
                        WHEN (emp.Salary + emp.Salary * 0.30) > (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                        THEN (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                        ELSE (emp.Salary + emp.Salary * 0.30)
                    END * 0.005), 0)
                 FROM Employees emp 
                 WHERE emp.JuniorManagerID = jun.JuniorManagerID)
            FROM JuniorManagers jun 
            WHERE jun.SeniorManagerID = {SeniorManagerID}";

            double Result = base.resSalary(Salary, query, DateWork, 0.05, 0.40, 0.005);
            return Result;
        }

        public override string FinalSalaryJun()
        {
            // Формула (Запрос) расчета конечной зарплаты для JuniorManagers (с учетом их подчиненных)
            return $@"
            CASE
                WHEN (jun.Salary + jun.Salary * 0.40) > (jun.Salary + jun.Salary * (0.05 * {JualyDay("jun")}))
                THEN (jun.Salary + jun.Salary * (0.05 * {JualyDay("jun")}))
                ELSE (jun.Salary + jun.Salary * 0.40)
            END +
            (SELECT COALESCE(SUM(
                CASE
                    WHEN (emp.Salary + emp.Salary * 0.30) > (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                    THEN (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                    ELSE (emp.Salary + emp.Salary * 0.30)
                END * 0.005), 0)
             FROM Employees emp 
             WHERE emp.JuniorManagerID = jun.JuniorManagerID)";
        }
    }



    [Table("JuniorManagers")]
    class JuniorManager : User
    {
        public int JuniorManagerID { get; set; }
        public int SeniorManagerID { get; set; }


        public JuniorManager() { }

        public JuniorManager(int JuniorManagerID, string FIO, string Login, string Password, double Salary, string DateWork, int SeniorManagerID) : base(FIO, Login, Password, DateWork, Salary)
        {
            this.JuniorManagerID = JuniorManagerID;
            this.SeniorManagerID = SeniorManagerID;


        }

        public override double FinalSalary(double Salary, string DateWork) 
        {

            string query = $@"SELECT {base.SalaryrequestForJun("0.03", "0.30")} FROM Employees as jun Where jun.JuniorManagerID = {JuniorManagerID}";

            double Result = base.resSalary(Salary, query, DateWork, 0.05, 0.4, 0.005);
            return Result;
        }
        public override string FinalSalaryJun()
        {
            string query = $@"(SELECT {base.SalaryrequestForJun("0.03", "0.30")} FROM Employees)";
            return base.SalaryrequestForJun("0.03", "0.30");
        }

    }

}
