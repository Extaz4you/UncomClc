using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace UncomClc.Views
{
    /// <summary>
    /// Логика взаимодействия для EditPipes.xaml
    /// </summary>
    public partial class EditPipes : Window
    {
        public event Action MainEditPipe;
        public EditPipes()
        {
            InitializeComponent();
            LoadPipes();
        }
        private void LoadPipes()
        {
            PipesDataGrid.ItemsSource = Data.UploadedData.Instance.Pipes;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteSelectedRow(Pipe pipe)
        {

        }
    }
}
