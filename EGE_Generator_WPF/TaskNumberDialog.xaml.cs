using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace EgeGenerator
{
    public partial class TaskNumberDialog : Window
    {
        public int TaskNumber { get; private set; }

        public TaskNumberDialog()
        {
            InitializeComponent();
            TaskNumber = 0;
            txtTaskNumber.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtTaskNumber.Text, out int number))
            {
                if (number >= 1 && number <= 27)
                {
                    TaskNumber = number;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Номер задания должен быть от 1 до 27",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTaskNumber.Focus();
                }
            }
            else
            {
                MessageBox.Show("Введите корректное число",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTaskNumber.Focus();
            }
        }

        private void TxtTaskNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}