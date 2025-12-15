using System.Windows;

namespace EgeGenerator
{
    public partial class InputDialog : Window
    {
        public string Otvet { get; private set; }

        public InputDialog(string zagolovok)
        {
            InitializeComponent();
            Title = zagolovok;
            Otvet = "";
            txtInput.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Otvet = txtInput.Text;
            DialogResult = true;
        }
    }
}