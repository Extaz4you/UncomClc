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
using UncomClc.Data;

namespace UncomClc.Views.Line
{
    /// <summary>
    /// Логика взаимодействия для LineView.xaml
    /// </summary>
    public partial class LineView : Window
    {
        public string NameLine { get; private set; }
        public LineView(string currentName)
        {
            InitializeComponent();
            Load(currentName);
            Name.Text = currentName;
            Name.Focus();
            Name.SelectAll();
        }
        private void Load(string name)
        {
            Name.Text = name;
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show("Название не может быть пустым", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NameLine = Name.Text.Trim();
            DialogResult = true;
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
