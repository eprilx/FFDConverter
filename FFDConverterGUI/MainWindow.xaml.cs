using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using FFDConverter;
using Microsoft.Win32;

namespace FFDConverterGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunFFDConverterConsole(List<string> args)
        {
            string exePath = System.AppDomain.CurrentDomain.BaseDirectory;
            exePath += "FFDConverter.exe";
            string strCmdText = exePath + " ";
            foreach(string str in args)
            {
                strCmdText += " \"" + str + "\"";
            }
            //MessageBox.Show(strCmdText);
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = strCmdText;
            process.Start();

        }

        private void rbtn2_Checked(object sender, RoutedEventArgs e)
        {
            label1.Visibility = Visibility.Hidden;
            txb1.Visibility = Visibility.Hidden;
            btn1.Visibility = Visibility.Hidden;
            if (txb3.Text != "(Optional)")
                txb3.Text = txb2.Text.ToString() + ".fnt";
        }

        private void rbtn1_Checked(object sender, RoutedEventArgs e)
        {
            if (label1 != null)
            {
                label1.Visibility = Visibility.Visible;
                txb1.Visibility = Visibility.Visible;
                btn1.Visibility = Visibility.Visible;
                if (txb3.Text != "(Optional)")
                    txb3.Text = txb2.Text.ToString() + ".ffd";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string inputFFD = null;
            string output = null;
            string fntInput = null;
            List<string> args;
            fntInput = txb1.Text.ToString();
            inputFFD = txb2.Text.ToString();
            output = txb3.Text.ToString();
            if (rbtn1.IsChecked == true)
            {
                if (!File.Exists(fntInput))
                    { MessageBox.Show("Missing char desc file"); return; }
                if (!File.Exists(inputFFD))
                    { MessageBox.Show("Missing original ffd file"); return; }
                args = new List<string>{ "-fnt2ffd", "-v", cbx.SelectedItem.ToString(), "-f",inputFFD, "-b", fntInput,"-o",output};

            }
            else
            {
                if (!File.Exists(inputFFD))
                    { MessageBox.Show("Missing original ffd file"); return; }
                args = new List<string> {"-ffd2fnt", "-v", cbx.SelectedItem.ToString(), "-f", inputFFD, "-o", output };
            }

            RunFFDConverterConsole(args);
        }


        private void ComboBox_Initialized(object sender, EventArgs e)
        {
            List<string> versionGames = FFDConverter.DefaultConfig.GetSupportedList();
            foreach (string game in versionGames)
            {
                cbx.Items.Add(game);
            }
            cbx.SelectedItem = versionGames[0];
        }

        private void txb2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txb3 == null)
            {
                return;
            }
            if (rbtn1.IsChecked == true)
            {
                txb3.Text = txb2.Text.ToString() + ".new";
            }
            else
            {
                txb3.Text = txb2.Text.ToString() + ".fnt";
            }
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FNT file (*.fnt)|*.fnt";
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                txb1.Text = openFileDialog.FileName;
            }
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FFD file (*.ffd;*.Fire_Font_Descriptor)|*.ffd;*.Fire_Font_Descriptor";
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                txb2.Text = openFileDialog.FileName;
            }
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (rbtn1.IsChecked == true)
                saveFileDialog.Filter = "New FFD file|*.ffd";
            else
                saveFileDialog.Filter = "FNT file|*.fnt";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                txb3.Text = saveFileDialog.FileName;
            }
        }
    }
}
