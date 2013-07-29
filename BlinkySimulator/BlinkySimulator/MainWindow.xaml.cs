using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlinkySimulator
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Demo _demo;
        public MainWindow()
        {
            InitializeComponent();

            SPI spi = new SPI(this, virtualScreen, 32, 5);

            _demo = new Demo(spi);
            
        }

        private void updateMode(object sender, KeyEventArgs e)
        {
            _demo.ChangeMode(0,0,System.DateTime.Now);
        }
    }
}
