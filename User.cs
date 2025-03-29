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
        

        public Info info = new Info();

       

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


        [NotMapped]
        public static DateTime CurrentSelectedDate { get; protected set; } = DateTime.Today;

        public static void SetCurrentSelectedDate(DateTime date)
        {
            CurrentSelectedDate = date;
        }

        public void LimitDate(string DateWork)
        {
            DateTime DateWorkInit = DateTime.ParseExact(DateWork, "dd.MM.yyyy", null);

            if (CurrentSelectedDate < DateWorkInit)
            {
                
                MessageBox.Show("Введённая вами дата некорректна по отношению к дате начала работы, дата установится в минимальное допустимое значение");
                CurrentSelectedDate = DateWorkInit;


            }
        }
        public int years(string DateWork)
        {
            try
            {
                DateTime DateWorkInit = DateTime.ParseExact(DateWork, "dd.MM.yyyy", null);
                //int years = CurrentSelectedDate.Year - DateWorkInit.Year;

                LimitDate(DateWork);
                int years = CurrentSelectedDate.Year - DateWorkInit.Year;
               


                if (DateWorkInit > CurrentSelectedDate.AddYears(-years))
                    years--;
               
                return years;
            }
            catch
            {
                return 0;
            }
            

        }





        // Расчёт финальной зарплаты пользователя
        public double resSalary(double Salary, string query, string DateWork, double ExperiencePrecent, double LimExperiencePrecent, double SubordinatesSalariesPrecent)
        {
            string dbPath = "Users.db";
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


        public string SalaryrequestForJun(string ExperiencePrecent, string LimExperiencePrecent, string limDate)
        {
            DateTime DateWorkInit = DateTime.ParseExact(limDate, "dd.MM.yyyy", null);

            if (CurrentSelectedDate < DateWorkInit)
            {
                //CurrentSelectedDate = DateWorkInit;
                MessageBox.Show("Введённая вами дата некорректна по отношению к дате начала работы, дата установится в минимальное допустимое значение");
                CurrentSelectedDate = DateWorkInit;
            }

            string currentDateFormatted = CurrentSelectedDate.ToString("yyyy-MM-dd");

            string query1 = $@"CAST((julianday('{currentDateFormatted}') - julianday(
                    substr(jun.DateWork, 7, 4) || '-' || substr(jun.DateWork, 4, 2) || '-' || substr(jun.DateWork, 1, 2))) / 365.25 AS INTEGER)";

            string query2 = $@"CASE
                      WHEN julianday(substr(jun.DateWork, 7, 4) || '-' || 
                           substr(jun.DateWork, 4, 2) || '-' || 
                           substr(jun.DateWork, 1, 2)) > julianday('{currentDateFormatted}')
                      THEN 0  
                      ELSE (
                          CASE
                          WHEN (jun.Salary + jun.Salary * {LimExperiencePrecent}) > 
                               (jun.Salary + jun.Salary * ({ExperiencePrecent}*{query1}))  
                          THEN (jun.Salary + jun.Salary * ({ExperiencePrecent}*{query1}))
                          ELSE (jun.Salary + jun.Salary * {LimExperiencePrecent})
                          END
                      )
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

            string query = $"SELECT DISTINCT {SalaryrequestForJun("0.01", "0.35", DateWork)} FROM JuniorSalesmans as jun WHERE jun.SeniorSalesmanID = {SeniorSalesmanID}";
            double Result = base.resSalary(Salary, query, DateWork, 0.01, 0.35, 0.003);
            return Result;

        }

        public override string FinalSalaryJun()
        {
            string Result = base.SalaryrequestForJun("0.01", "0.35", DateWork);

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
            string currentDateFormatted = CurrentSelectedDate.ToString("yyyy-MM-dd");
            return $@"CAST((julianday('{currentDateFormatted}') - julianday(
                            substr({post}.DateWork, 7, 4) || '-' || substr({post}.DateWork, 4, 2) || '-' || substr({post}.DateWork, 1, 2))) / 365.25 AS INTEGER)";


        }

        public override double FinalSalary(double Salary, string DateWork)
        {
            DateTime DateWorkInit = DateTime.ParseExact(DateWork, "dd.MM.yyyy", null);
            string currentDateFormatted = CurrentSelectedDate.ToString("yyyy-MM-dd");

            string experienceCalc = $@"CAST((julianday('{currentDateFormatted}') - julianday(
                            substr(jun.DateWork, 7, 4) || '-' || 
                            substr(jun.DateWork, 4, 2) || '-' || 
                            substr(jun.DateWork, 1, 2))) / 365.25 AS INTEGER)";

            string query = $@"
    SELECT 
        CASE
            WHEN julianday(substr(jun.DateWork, 7, 4) || '-' || 
                 substr(jun.DateWork, 4, 2) || '-' || 
                 substr(jun.DateWork, 1, 2)) > julianday('{currentDateFormatted}')
            THEN 0
            ELSE (
                CASE
                    WHEN (jun.Salary + jun.Salary * 0.40) > (jun.Salary + jun.Salary * (0.05 * {experienceCalc}))
                    THEN (jun.Salary + jun.Salary * (0.05 * {experienceCalc}))
                    ELSE (jun.Salary + jun.Salary * 0.40)
                END
            )
        END +
        (SELECT COALESCE(SUM(
            CASE
                WHEN julianday(substr(emp.DateWork, 7, 4) || '-' || 
                     substr(emp.DateWork, 4, 2) || '-' || 
                     substr(emp.DateWork, 1, 2)) > julianday('{currentDateFormatted}')
                THEN 0
                ELSE (
                    CASE
                        WHEN (emp.Salary + emp.Salary * 0.30) > (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                        THEN (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                        ELSE (emp.Salary + emp.Salary * 0.30)
                    END * 0.005
                )
            END), 0)
         FROM Employees emp 
         WHERE emp.JuniorManagerID = jun.JuniorManagerID)
    FROM JuniorManagers jun 
    WHERE jun.SeniorManagerID = {SeniorManagerID}";

            double Result = base.resSalary(Salary, query, DateWork, 0.05, 0.40, 0.005);
            return Result;
        }
        public override string FinalSalaryJun()
        {
            DateTime DateWorkInit = DateTime.ParseExact(DateWork, "dd.MM.yyyy", null);

            if (CurrentSelectedDate < DateWorkInit)
            {
                User.SetCurrentSelectedDate(DateWorkInit);
                MessageBox.Show("Введённая вами дата некорректна по отношению к дате начала работы, дата установится в минимальное допустимое значение");
            }

            string currentDateFormatted = CurrentSelectedDate.ToString("yyyy-MM-dd");

            string experienceCalc = $@"CAST((julianday('{currentDateFormatted}') - julianday(
                        substr(jun.DateWork, 7, 4) || '-' || 
                        substr(jun.DateWork, 4, 2) || '-' || 
                        substr(jun.DateWork, 1, 2))) / 365.25 AS INTEGER)";

            return $@"
CASE
    WHEN julianday(substr(jun.DateWork, 7, 4) || '-' || 
         substr(jun.DateWork, 4, 2) || '-' || 
         substr(jun.DateWork, 1, 2)) > julianday('{currentDateFormatted}')
    THEN 0
    ELSE (
        CASE
            WHEN (jun.Salary + jun.Salary * 0.40) > (jun.Salary + jun.Salary * (0.05 * {experienceCalc}))
            THEN (jun.Salary + jun.Salary * (0.05 * {experienceCalc}))
            ELSE (jun.Salary + jun.Salary * 0.40)
        END
    )
END +
(SELECT COALESCE(SUM(
    CASE
        WHEN julianday(substr(emp.DateWork, 7, 4) || '-' || 
             substr(emp.DateWork, 4, 2) || '-' || 
             substr(emp.DateWork, 1, 2)) > julianday('{currentDateFormatted}')
        THEN 0
        ELSE (
            CASE
                WHEN (emp.Salary + emp.Salary * 0.30) > (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                THEN (emp.Salary + emp.Salary * (0.03 * {JualyDay("emp")}))
                ELSE (emp.Salary + emp.Salary * 0.30)
            END * 0.005
        )
    END), 0)
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

            string query = $@"SELECT {base.SalaryrequestForJun("0.03", "0.30", DateWork)} FROM Employees as jun Where jun.JuniorManagerID = {JuniorManagerID}";

            double Result = base.resSalary(Salary, query, DateWork, 0.05, 0.4, 0.005);
            return Result;
        }
        public override string FinalSalaryJun()
        {
            string query = $@"(SELECT {base.SalaryrequestForJun("0.03", "0.30", DateWork)} FROM Employees)";
            return base.SalaryrequestForJun("0.03", "0.30", DateWork);
        }

    }

}
