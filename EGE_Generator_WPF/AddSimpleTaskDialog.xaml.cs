using System;
using System.IO;
using System.Windows;
using EgeGenerator;

namespace EgeGenerator
{
    public partial class AddSimpleTaskDialog : Window
    {
        private readonly int _nomerZadaniya;
        private readonly string _putKhranilishcha;
        private string _putFailaZadaniya = "";

        public int NomerVarianta { get; private set; }

        public AddSimpleTaskDialog(int nomerZadaniya, string putKhranilishcha)
        {
            InitializeComponent();

            _nomerZadaniya = nomerZadaniya;
            _putKhranilishcha = putKhranilishcha;

            Title = $"Добавление задания {nomerZadaniya}";
            //txtZagolovok.Text = $"Добавление задания {nomerZadaniya}";

            ObnovitNomerVarianta();
        }

        private void ObnovitNomerVarianta()
        {
            string putPapkiZadaniya = StorageService.PoluchitPutPapkiZadaniya(_putKhranilishcha, _nomerZadaniya);
            NomerVarianta = FileService.NaytiDostupnyNomerVarianta(putPapkiZadaniya);
            txtInformatsiyaOVariante.Text = $"Будет создан вариант № {NomerVarianta}";

            if (!System.IO.Directory.Exists(putPapkiZadaniya))
            {
                txtInformatsiyaOVariante.Text += " (новая папка)";
            }
        }

        private void BtnZagruzitZadanie_Click(object sender, RoutedEventArgs e)
        {
            _putFailaZadaniya = FileService.ZagruzitFailZadaniya(this);
            if (!string.IsNullOrEmpty(_putFailaZadaniya))
            {
                ObnovitStatusFaila(txtStatusZadaniya, txtProverkaZadaniya, _putFailaZadaniya, true);
            }
        }

        private void BtnZagruzitOtvet_Click(object sender, RoutedEventArgs e)
        {
            string putFailaOtvet = FileService.ZagruzitFailOtvet(this);
            if (!string.IsNullOrEmpty(putFailaOtvet))
            {
                txtOtvet.Text = System.IO.File.ReadAllText(putFailaOtvet).Trim();
            }
        }

        private void BtnSohranit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка ввода с помощью ValidationService
                ValidationService.ProveritProstoeZadanie(_putFailaZadaniya, txtOtvet.Text.Trim());

                // Сохранение с помощью FileService
                FileService.SohranitProstoeZadanie(_putKhranilishcha, _nomerZadaniya,
                    NomerVarianta, _putFailaZadaniya, txtOtvet.Text.Trim());

                MessageBox.Show($"Успешно добавлен вариант {NomerVarianta} для задания {_nomerZadaniya}!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ObnovitStatusFaila(System.Windows.Controls.TextBlock statusTekst,
                                       System.Windows.Controls.TextBlock checkTekst,
                                       string putKFailu, bool obyazatelny)
        {
            if (!string.IsNullOrEmpty(putKFailu) && System.IO.File.Exists(putKFailu))
            {
                statusTekst.Text = System.IO.Path.GetFileName(putKFailu);
                checkTekst.Text = "✓";
                checkTekst.Foreground = System.Windows.Media.Brushes.Green;
                checkTekst.Visibility = Visibility.Visible;
            }
            else
            {
                statusTekst.Text = "Не загружено";
                if (obyazatelny)
                {
                    checkTekst.Text = "✗";
                    checkTekst.Foreground = System.Windows.Media.Brushes.Red;
                    checkTekst.Visibility = Visibility.Visible;
                }
                else
                {
                    checkTekst.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}