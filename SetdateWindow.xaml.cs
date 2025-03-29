using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EMP_WPF_FR
{

    public partial class SetdateWindow : Window
    {
        public DateTime? SelectedDate => DateSalary.SelectedDate;
        public MainWindow main;
        public SetdateWindow()
        {
            InitializeComponent();


        }

        public void NewInfo(object sender, RoutedEventArgs e)
        {
            if (!SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату!");
                return;
            }

            // Передаем дату в User через статическое свойство или метод
            User.SetCurrentSelectedDate(SelectedDate.Value);




            Login login = new Login(main.Login, main.Password, this, this.SelectedDate.Value);

            this.Close();
        }

        public SetdateWindow(TextBox Login, PasswordBox Password, MainWindow mainWindow)
        {
            InitializeComponent();
            this.main = mainWindow;
        }
    }



}
