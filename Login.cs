using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace EMP_WPF_FR
{
    class Login
    {
        public string login = "";
        public string password = "";
        public TextBox BoxLogin;
        public PasswordBox BoxPassword;  
        public Info info_w = new Info();


        ApplicationContext db = new ApplicationContext();


        public Employee Empl = null;
        public SuperUser SupUser = null;
        
        public SeniorSalesman Sensale = null;
        public JuniorSalesman JunSale = null;
        public SeniorManager Senmanage = null;
        public JuniorManager Junmanage = null;

        public MainWindow LogPass = new MainWindow();

        SetdateWindow SetDate;

        public Login(TextBox BoxLogin, PasswordBox BoxPassword, SetdateWindow SetDate, DateTime? SelectedDate)
        {

            this.BoxLogin = BoxLogin;
            this.BoxPassword = BoxPassword;
            this.SetDate = SetDate;
            login = BoxLogin.Text.Trim();
            password = BoxPassword.Password.Trim();
            Empl = db.Employees.Where(log => log.Login == login && log.Password == password).FirstOrDefault();
            SupUser = db.SuperUsers.Where(log => log.Login == login && log.Password == password).FirstOrDefault();
            Sensale = db.SeniorSalesmans.Where(log => log.Login == login && log.Password == password).FirstOrDefault();
            JunSale = db.JuniorSalesmans.Where(log => log.Login == login && log.Password == password).FirstOrDefault();
            Senmanage = db.SeniorManagers.Where(log => log.Login == login && log.Password == password).FirstOrDefault();
            Junmanage = db.JuniorManagers.Where(log => log.Login == login && log.Password == password).FirstOrDefault();



            Check_input();
      
        }
        public void Check_input()
        {

            if (login.Length < 5 || password.Length < 5) { MarkInvalid();}
            else { SetData(); }

        }

        public void MarkInvalid()
        {
            MessageBox.Show("Логин и пароль должен содержать более 5-ти символов");
            LogPass.Show();
        }

        public string queryForSuperUser(string table)
        {
            return $@"SELECT FIO AS [ФИО], 
                    Login AS [Логин],
                    Password AS [Пароль], 
                    Salary AS [Оклад] 
              FROM [{table}]";  // Квадратные скобки для экранирования
        }
        public void SetData()
        {

            List<object> Users_all = new List<object> { Empl, Senmanage, Sensale, JunSale, Junmanage, SupUser };
            List<object> Users_sin = new List<object> { Senmanage, Sensale, Junmanage }; 

            bool verification = false;


            foreach (var el in Users_all)
            {
                if (el is User el_user)
                {

                    if (el_user != null)
                    {
                        verification = true;
                        info_w.Show();
                        
                        info_w.Hello.Content = "Добро пожаловать, " + el_user.FIO;
                        el_user.Salary = FinalSalaryData(el_user);
                        info_w.Salary.Content = Math.Round(el_user.Salary, 2);
                        info_w.FIO.Content = el_user.FIO;
                        info_w.Main.Children.Remove(info_w.AddUser);

                        SetDate.Close();




                        if (el_user == Senmanage || el_user == Sensale || el_user == Junmanage)
                        {
                            string[] Column_table = SetColumnForQuery(el_user);
                            queryRorSen(Column_table);

                        }
                        else
                        {
                            AdaptiveInterface();
                        }
                        //Адаптация размера 
                        double GridLen = 0;
                        for (int i = 0; i < info_w.GridForData.RowDefinitions.Count - 1; i++)
                        {
                            GridLen += info_w.GridForData.RowDefinitions[i].ActualHeight;
                        }

                        double rowCount = info_w.JuniorDataGrid.Items.Count; // количество элементов в таблице
                        
                        if(rowCount == 0) { rowCount = 100; }
                      

                        double rowHeight = info_w.JuniorDataGrid.ActualHeight / rowCount; //Делим на размер всего контеёнера

                        info_w.BorderForData.Height = rowHeight + GridLen + 30;
                        info_w.Height = rowHeight + GridLen + 230;
                       


                    }

                }

                else if (el is SuperUser el_user1)
                {

                    if (el_user1 != null)
                    {
                        verification = true;
                        info_w.Show();
                       
                        info_w.Hello.Content = "Добро пожаловать, " + el_user1.FIO;
                        info_w.FIO.Content = el_user1.FIO;
                        info_w.AddUser.Visibility = Visibility.Visible;


                        string query = $@"{queryForSuperUser("Employees")} UNION ALL {queryForSuperUser("JuniorManagers")}
                                           UNION ALL {queryForSuperUser("JuniorSalesmans")} UNION ALL {queryForSuperUser("SeniorManagers")}
                                            UNION ALL {queryForSuperUser("SeniorSalesmans")}";



                        SetDate.Close();
                        //Изменения параметры формы
                        info_w.GridForData.Children.Remove(info_w.HeadingSalary);

                        Grid.SetColumn(info_w.FIO, 2);
                        Grid.SetColumn(info_w.HeadingFIO, 2);
                        
                        Grid.SetColumnSpan(info_w.JuniorDataGrid,2);
                        info_w.JuniorDataGrid.Width = 530;

                        SetDataTable(query);

                    }

                }

            }
            if (!verification) { MessageBox.Show("Пароль или логин введены неверно"); LogPass.Show(); }

        }


        public double FinalSalaryData(User el_user)
        {
            try
            {
                double FinalSalaryvalue = el_user.FinalSalary(el_user.Salary, el_user.DateWork);
                return FinalSalaryvalue;

            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Пользователь не найден");
                return 0.0;
            }


        }


        public void SetDataTable(string query)
        {
            string dbPath = "Users.db";
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader()) // чтение данных
                    {
                        // DataTable для хранения данных
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        // Привязываем DataTable к DataGrid
                        info_w.JuniorDataGrid.ItemsSource = dataTable.DefaultView;

                        info_w.GridForData.RowDefinitions[2].Height = GridLength.Auto;


                    }
                    connection.Close();


                }

            }


        }

        public void queryRorSen(string[] Column_table)
        {
            string query = $@"SELECT DISTINCT jun.FIO AS ФИО, jun.Salary AS Оклад, {Column_table[4]} AS [Конечная зарплата]
                                   FROM {Column_table[2]} AS jun
                                   INNER JOIN {Column_table[1]} sen ON jun.{Column_table[0]} = {Column_table[3]}";

            SetDataTable(query);

        }
        public string[] SetColumnForQuery(User el)
        {
            //список для хранения имен первых столбцов
            List<string> firstColumnNamesAndReq = new List<string>();
            var sen_and_jun = new Dictionary<string, string>()
            {
                ["JuniorManagers"] = "Employees",
                ["SeniorManagers"] = "JuniorManagers",
                ["SeniorSalesmans"] = "JuniorSalesmans"

            };

            var type = el.GetType();


            var firstColumnName = type.GetProperties().First().Name;
            string tableName_sen = type.Name + "s";
            var ID = GetIDValue(el);


            firstColumnNamesAndReq.Add(firstColumnName);
            firstColumnNamesAndReq.Add(tableName_sen);


            foreach (var person in sen_and_jun)
            {
                if (person.Key == tableName_sen)
                {
                    string tableName_jun = person.Value;
                    firstColumnNamesAndReq.Add(tableName_jun);
                    firstColumnNamesAndReq.Add(Convert.ToString(ID));
                }
            }

                firstColumnNamesAndReq.Add(el.FinalSalaryJun());
            // массив имен первых столбцов
            return firstColumnNamesAndReq.ToArray();
        }



        public static object GetIDValue(object obj)
        {
            var properties = obj.GetType().GetProperties();
            var secondProperty = properties[0];
            return secondProperty.GetValue(obj);

        }


        public void AdaptiveInterface()
        {

            info_w.GridForData.Children.Remove(info_w.JuniorDataGrid);
            info_w.GridForData.Children.Remove(info_w.Subordinates);
            info_w.Main.Children.Remove(info_w.AddUser);

            Grid.SetColumn(info_w.FIO, 0);
            Grid.SetColumn(info_w.Salary, 1);

            Grid.SetRow(info_w.HeadingFIO, 0);
            Grid.SetColumn(info_w.HeadingFIO, 0);

            Grid.SetRow(info_w.HeadingSalary, 0);
            Grid.SetColumn(info_w.HeadingSalary, 1);

            info_w.BorderForData.Height = 150;

            info_w.Width = 800;
            info_w.Height = 400;

        }



       


    }

}

        
        
        




    

