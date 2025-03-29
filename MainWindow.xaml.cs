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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EMP_WPF_FR
{

    public partial class MainWindow : Window
    {
       
        private SetdateWindow newWind;
        SuperUser super = null;
        ApplicationContext db = new ApplicationContext();
        public string login = "";
        public string password = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Next(object sender, RoutedEventArgs e)
        {

            newWind = new SetdateWindow(this.Login, this.Password, this);
            login = Login.Text.Trim();
            password = Password.Password.Trim();
            super = db.SuperUsers.Where(log => log.Login == login && log.Password == password).FirstOrDefault();

           
            if (super!=null)
            {
                DateTime Today = DateTime.Today;
                Login login = new Login(Login, Password, newWind, Today);

            }
            else { newWind.Show(); }
              


            this.Close();
        }
    }

}

