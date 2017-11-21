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
using Microsoft.Win32;
namespace Compiler
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        String InputFileName = "";
        String OutputFileName = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //button for 'run'
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //button for 'selecting input file'
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.RestoreDirectory = true;
            dialog.Filter = "*.txt|*.*";
            dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == true)
            {
                ((Button)sender).Content = "(点击可以再次选择)";
                InputFileName = dialog.FileName;
                this.InputFileDest.Text = InputFileName;
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //button for 'selecting output file'
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.RestoreDirectory = true;
            dialog.Filter = "*.txt|*.*";
            dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == true)
            {
                ((Button)sender).Content = "(点击可以再次选择)";
                OutputFileName = dialog.FileName;
                this.OutputFileDest.Text = OutputFileName;
            }
        }
    }
}
