using System.Windows;

namespace EgeGenerator
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; }

        public InputDialog(string title, string question)
        {
            InitializeComponent();
            Title = title;
            txtInput.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Answer = txtInput.Text;
            DialogResult = true;
        }
    }
}