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
        ApplicationContext db;
        private SetdateWindow newWind;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Next(object sender, RoutedEventArgs e)
        {
            // Предполагаем, что Login и Password — это свойства или поля этого окна,
            // содержащие учетные данные пользователя
            newWind = new SetdateWindow(this.Login, this.Password, this);
            newWind.Show();
            this.Close();
        }
    }

}

