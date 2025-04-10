﻿using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections;


namespace EMP_WPF_FR
{

    public partial class AddUser : Window
    {
         public Dictionary<string, string> ValueComboBoxPost = new Dictionary<string, string>()
        {
            ["Работник"] = "Employees",
            ["Младший менеджер"] = "JuniorManagers",
            ["Старший менеджер"] = "SeniorManagers",
            ["Младший продавец"] = "JuniorSalesmans",
            ["Старший продавец"] = "SeniorSalesmans"
        };

        public Dictionary<string, string> jun_and_sen = new Dictionary<string, string>()
        {
            ["Employees"] = "JuniorManagers",
            ["JuniorManagers"] = "SeniorManagers",
            ["JuniorSalesmans"] = "SeniorSalesmans"
        };


        public AddUser()
        {
            InitializeComponent();
        }
        public void Adduser(object sender, RoutedEventArgs e)
        {
            Info info_win = new Info();
            AddUser addUser_win = new AddUser();
            info_win.Close();
            addUser_win.Show();

        }

        public void ErrtextBox(Border Border)
        {
            Border.Background = Brushes.Pink;
            Border.Background = Brushes.Pink;

        }

        public void ExecuteQuery(string query, Dictionary<string, object> parameters)
        {
            try
            {
                string connectionString = "Data Source=Users.db";
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqliteCommand(query, connection))
                    {
                        foreach (var param in parameters)
                        {

                            command.Parameters.AddWithValue(param.Key, param.Value); // добавление параметров динамически из словаря
                        }
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                MessageBox.Show("Пользователь зарегистрирован");
                var color = (Color)ColorConverter.ConvertFromString("#AFB2B8");
                BorderSalary.Background = new SolidColorBrush(color);
                BorderPassword.Background = new SolidColorBrush(color);




            }

            catch(InvalidOperationException)
            {
                MessageBox.Show("Не все поля заполнены !");
            }

            catch (SqliteException ex ) when (ex.SqliteErrorCode == 19)
            {
                if (Convert.ToInt32(Salary.Text)<0) { MessageBox.Show("Поле оклад не может быть отрицательным");  ErrtextBox(BorderSalary); }
                else
                {
                    MessageBox.Show("Пароль уже используется!");
                    ErrtextBox(BorderPassword);
                }

            }
        }

        public string GetLastColimnName(string TableName)
        {

            List<string> columnNames = new List<string>();
            string connectionString = $"Data Source=Users.db";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string queryCloumn = $"PRAGMA table_info({TableName});";

                using (SQLiteCommand command = new SQLiteCommand(queryCloumn, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string columnName = reader.GetString(1);
                            columnNames.Add(columnName);
                        }
                    }
                }
                connection.Close();
            }
            string LastColumnName = columnNames[columnNames.Count - 1];

            return LastColumnName;

        }

        public int? GetIdValue(string TableName)
        {
            string selectedValue = "";
            if (СhoiceComboBoxDirector.SelectedItem != null)
            {
                selectedValue = СhoiceComboBoxDirector.SelectedItem.ToString();
            }
            var jun_and_sen = new Dictionary<string, string>()
            {
                ["Employees"] = "JuniorManagers",
                ["JuniorManagers"] = "SeniorManagers",
                ["JuniorSalesmans"] = "SeniorSalesmans"
            };

            string connectionString = $"Data Source=Users.db";
            string queryValuColumn = $@"SELECT {GetLastColimnName(TableName)} FROM {jun_and_sen[TableName]} WHERE FIO = '{selectedValue}'"; 

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(queryValuColumn, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int IdValue = reader.GetInt32(0);
                            connection.Close();
                            return IdValue;

                        }
                        connection.Close();

                        return null;
                    }
                }
                
            }

        }

        public void AddUserInit(string TableName)
        {
            string FIOText = FIO.Text.Trim() == "" ? null : FIO.Text.Trim();
            string LoginText = Login.Text.Trim() == "" ? null : Login.Text.Trim();
            string PasswordText = Password.Text.Trim() == "" ? null : Password.Text.Trim();
            string DateWorkText = Date.Text.Trim() == "" ? null : Date.Text.Trim();
            try
            {
                int SalaryText = Convert.ToInt32(Salary.Text.Trim());

                string ColumNameIdSen = GetLastColimnName(TableName); 

                bool isBoss = TableName == "SeniorManagers" || TableName == "SeniorSalesmans";

                string query;
                if (isBoss)
                {
                    query = $@"INSERT INTO {TableName} (FIO, Login, Password, DateWork, Salary) VALUES (@FIO, @Login, @Password, @DateWork, @Salary)";
                }
                else
                {
                    query = $@"INSERT INTO {TableName} (FIO, Login, Password, DateWork, Salary, {ColumNameIdSen}) VALUES (@FIO, @Login, @Password, @DateWork, @Salary, @{ColumNameIdSen})";
                }
                var parameters = new Dictionary<string, object>
    {
        { "@FIO", FIOText },
        { "@Login", LoginText },
        { "@Password", PasswordText },
        { "@DateWork", DateWorkText },
        { "@Salary", SalaryText }
    };

                if (!isBoss)
                {
                    parameters.Add($"@{ColumNameIdSen}", GetIdValue(TableName));
                }

                ExecuteQuery(query, parameters);
            }
            catch (FormatException)
            {
                MessageBox.Show("Поле оклад должно быть числовым ");
            }
        }

        public void SetValue()
        {

            // Получаем выбранную таблицу из ComboBox
            if (СhoiceComboBoxPost.SelectedItem is TextBlock selectedTextBlock)
            {
                string selectedText = selectedTextBlock.Text;

                if (selectedText != "Старший менеджер" && selectedText != "Старший продавец")
                {
                    string query = $@"Select FIO FROM {jun_and_sen[ValueComboBoxPost[selectedText]]}";
                    ComboBoxDirector(СhoiceComboBoxDirector, query);
                   

                }

            }

           

        }
        public void AddUserMain()
        {   

            // Получаем выбранную таблицу из ComboBox
            if (СhoiceComboBoxPost.SelectedItem is TextBlock selectedTextBlock)
            {
                string selectedText = selectedTextBlock.Text;

                if (selectedText != "Старший менеджер" && selectedText != "Старший продавец")
                {
                    string query = $@"Select FIO FROM {jun_and_sen[ValueComboBoxPost[selectedText]]}";
                    ComboBoxDirector(СhoiceComboBoxDirector, query);

                }
                    // Проверяем, есть ли выбранное значение в словаре
                    if (ValueComboBoxPost.ContainsKey(selectedText))
                    {
                        AddUserInit(ValueComboBoxPost[selectedText]);

                   
                    }
                    else
                    {
                        
                        MessageBox.Show("Выбранная должность не найдена!");
                    }
            }

        }

        public void Regr(object sender, RoutedEventArgs e)
        {
            AddUserMain();
            

        }
        
        private void SetValueComboBoxDirector(object sender, SelectionChangedEventArgs e)
        {
            СhoiceComboBoxDirector.Items.Clear();
            SetValue();
        }

        private void ComboBoxDirector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СhoiceComboBoxDirector.SelectedItem != null)
            {
                string selectedValue = СhoiceComboBoxDirector.SelectedItem.ToString();   
                MessageBox.Show($"Выбранный руководитель: {selectedValue}");
            }
        }

        // добавления данных в ComboBox
        public void ComboBoxDirector(ComboBox comboBox, string query) 
        {
            string connectionString = "Data Source=Users.db";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox.Items.Add(reader[0].ToString());
                        }
                    }
                }

            }

        }

    }
}
