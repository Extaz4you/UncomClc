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
using UncomClc.Models.Insulation;

namespace UncomClc.Views.InsulationsView
{
    /// <summary>
    /// Логика взаимодействия для Add_Insulation.xaml
    /// </summary>
    public partial class Add_Insulation : Window
    {
        public Insulation NewInsulation { get; private set; }
        public Add_Insulation()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            decimal parsed;
            var isParsed = decimal.TryParse(Koef.Text, out parsed);
            NewInsulation = new Insulation
            {
                Name = Name.Text,
                Koef = isParsed ? parsed : 0,
            };
            try
            {
                UploadedData.Instance.Insulations.Add(NewInsulation);
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
