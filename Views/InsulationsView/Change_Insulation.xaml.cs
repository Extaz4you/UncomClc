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
using UncomClc.Models.Insulations;

namespace UncomClc.Views.InsulationsView
{
    /// <summary>
    /// Логика взаимодействия для Change_Insulation.xaml
    /// </summary>
    public partial class Change_Insulation : Window
    {
        private readonly Insulation _originalInsulation;
        public bool IsSaved { get; private set; }

        public Change_Insulation(Insulation insulation)
        {
            InitializeComponent();
            _originalInsulation = insulation ?? throw new ArgumentNullException(nameof(insulation));
            LoadParamForEditWindow();
        }
        private void LoadParamForEditWindow()
        {
            Name.Text = _originalInsulation.Name;
            Koef.Text = _originalInsulation.Koef.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _originalInsulation.Name = Name.Text;

            if (decimal.TryParse(Koef.Text, out decimal koef))
            {
                _originalInsulation.Koef = koef;
            }
            else
            {
                MessageBox.Show("Введите корректное значение коэффициента");
                return;
            }

            UploadedData.Instance.Save();
            IsSaved = true;
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
