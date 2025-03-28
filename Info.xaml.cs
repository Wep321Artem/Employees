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
    public partial class Info : Window
    {
        public Info()
        {   
            InitializeComponent(); 
        }


        public void Adduser(object sender, RoutedEventArgs e)
        {

            AddUser AddUser = new AddUser();
            AddUser.Show();
            this.Close();

        }


    }
}
