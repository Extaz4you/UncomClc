using System;
using System.Collections.Generic;
using System.IO.Pipelines;
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
using UncomClc.Models;

namespace UncomClc.Views.PipeViews
{
    /// <summary>
    /// Логика взаимодействия для Change_Pipe.xaml
    /// </summary>
    public partial class Change_Pipe : Window
    {
        private readonly Models.Pipe _originalPipe;
        public bool IsSaved { get; private set; }

        public Change_Pipe(Models.Pipe pipe)
        {
            InitializeComponent();
            _originalPipe = pipe ?? throw new ArgumentNullException(nameof(pipe));
            LoadParamForEditWindow();
        }

        private void LoadParamForEditWindow()
        {
            Name.Text = _originalPipe.Name;
            Koef.Text = _originalPipe.Koef.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _originalPipe.Name = Name.Text;

            if (float.TryParse(Koef.Text, out float koef))
            {
                _originalPipe.Koef = koef;
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
