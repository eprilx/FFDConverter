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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FFDConverter;
namespace FFDConverterGUI
{
    /// <summary>
    /// Interaction logic for PageConfig.xaml
    /// </summary>
    public partial class PageConfig : Window
    {
        public PageConfig()
        {
            InitializeComponent();
        }

        private Config config = new();
        private string gameName = ((MainWindow) Application.Current.MainWindow).cbx.SelectedItem.ToString();
        private void MyGrid_Initialized(object sender, EventArgs e)
        {
            config = DefaultConfig.Get(gameName);
            scaleW.Text = config.scaleWidth.ToString();
            scaleH.Text = config.scaleHeight.ToString();
            scaleXadv.Text = config.scaleXadvance.ToString();
            addY.Text = config.addCustomYoffset.ToString();
            scaleXoff.Text = config.scaleXoffset.ToString();
            scaleYoff.Text = config.scaleYoffset.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                float.TryParse(scaleW.Text, out config.scaleWidth);
                float.TryParse(scaleH.Text, out config.scaleHeight);
                float.TryParse(scaleXadv.Text, out config.scaleXadvance);
                int.TryParse(addY.Text, out config.addCustomYoffset);
                float.TryParse(scaleXoff.Text, out config.scaleXoffset);
                float.TryParse(scaleYoff.Text, out config.scaleYoffset);
                DefaultConfig.Set(gameName, config);
                MessageBox.Show("Saved !!!");
            }
            catch
            {
                MessageBox.Show("Can't save config");
            }
            this.Close();
        }

        private void PreviewTextInput_(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
