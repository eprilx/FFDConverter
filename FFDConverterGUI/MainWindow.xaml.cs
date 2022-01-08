/*
MIT License

Copyright (c) 2021 eprilx

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AutoUpdaterDotNET;

namespace FFDConverterGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Change current culture
            CultureInfo culture;
            culture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            
            string ToolVersion;
            try
            {
                ToolVersion = FileVersionInfo.GetVersionInfo("FFDConverter.dll").FileVersion;
                Hide();
                AutoUpdater.Synchronous = true;
                AutoUpdater.InstalledVersion = new Version(ToolVersion);
                AutoUpdater.UpdateFormSize = new System.Drawing.Size(1200, 800);
                AutoUpdater.Start("https://raw.githubusercontent.com/eprilx/FFDConverter/master/AutoUpdate.xml");
            }
            catch
            {
                ToolVersion = "1.0.0";
            }
            InitializeComponent();
            Title = "FFDConverter GUI v" + ToolVersion;
            Show();
        }

        private void RunFFDConverterConsole(List<string> args)
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            exePath += "FFDConverter.exe";
            if (!File.Exists(exePath))
                { MessageBox.Show("Missing " + exePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            string strCmdText = exePath + " ";
            foreach (string str in args)
            {
                strCmdText += " \"" + str + "\"";
            }
            //MessageBox.Show(strCmdText);
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = strCmdText;
            process.Start();
            process.WaitForExit();
        }

        private void rbtn2_Checked(object sender, RoutedEventArgs e)
        {
            if (txb3.Text != "(Optional)")
                txb3.Text = txb2.Text.ToString() + ".fnt";
        }

        private void rbtn1_Checked(object sender, RoutedEventArgs e)
        {
            if (txb3 != null)
                if (txb3.Text != "(Optional)")
                    txb3.Text = txb2.Text.ToString() + ".new";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> args = new List<string>();
            string fntInput = txb1.Text.ToString();
            string inputFFD = txb2.Text.ToString();
            string output = txb3.Text.ToString();
            if (!File.Exists(inputFFD))
            { MessageBox.Show("Missing original ffd file", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (rbtn1.IsChecked == true)
            {
                if (!File.Exists(fntInput))
                { MessageBox.Show("Missing char desc file", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                args = new List<string> { "-fnt2ffd", "-v", cbx.SelectedItem.ToString(), "-f", inputFFD, "-b", fntInput, "-o", output };
            }
            else if (rbtn2.IsChecked == true)
            {
                args = new List<string> { "-ffd2fnt", "-v", cbx.SelectedItem.ToString(), "-f", inputFFD, "-o", output };
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
            if (txb3 == null)
            {
                return;
            }
            if (rbtn1.IsChecked == true)
            {
                txb3.Text = txb2.Text.ToString() + ".new";
            }
            else if (rbtn2.IsChecked == true)
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
            else if (rbtn2.IsChecked == true)
                saveFileDialog.Filter = "FNT file|*.fnt";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                txb3.Text = saveFileDialog.FileName;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = e.Uri.AbsoluteUri;
            process.Start();
            e.Handled = true;
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            PageConfig pageConfig = new PageConfig();
            pageConfig.Title = cbx.SelectedItem.ToString();

            pageConfig.ShowDialog();
            Show();
        }
    }
}
