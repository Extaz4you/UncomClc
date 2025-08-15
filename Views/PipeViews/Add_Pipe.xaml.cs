using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using UncomClc.Data;
using UncomClc.Models;

namespace UncomClc.Views.PipeViews
{
    /// <summary>
    /// Логика взаимодействия для Add_Pipe.xaml
    /// </summary>
    public partial class Add_Pipe : Window
    {
        public Models.Pipe NewPipe { get; private set; }
        private const string DataPath = @"Data\data.json";
        public Add_Pipe()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            float parsed;
            var isParsed = float.TryParse(Koef.Text, out parsed);
            NewPipe = new Models.Pipe
            {
                Name = Name.Text,
                Koef = isParsed ? parsed : 0,
            };
            try
            {
                UploadedData.Instance.Pipes.Add(NewPipe);
                UploadedData.Instance.Save(); 

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
