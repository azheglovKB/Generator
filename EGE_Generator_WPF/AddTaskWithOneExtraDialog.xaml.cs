using System;
using System.Windows;
using System.IO;
using EgeGenerator;

namespace EgeGenerator
{
    public partial class AddTaskWithOneExtraDialog : Window
    {
        private readonly int _nomerZadaniya;
        private readonly string _putKhranilishcha;
        private string _putFailaZadaniya = "";
        private string _putDopMaterialA = "";

        public int NomerVarianta { get; private set; }

        public AddTaskWithOneExtraDialog(int nomerZadaniya, string putKhranilishcha)
        {
            InitializeComponent();

            _nomerZadaniya = nomerZadaniya;
            _putKhranilishcha = putKhranilishcha;

            Title = $"Добавление задания {nomerZadaniya} (с доп. материалом)";
            //txtZagolovok.Text = $"Добавление задания {nomerZadaniya} (с доп. материалом)";

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

        private void BtnZagruzitDopMaterialA_Click(object sender, RoutedEventArgs e)
        {
            _putDopMaterialA = FileService.ZagruzitDopMaterial(this, "Выберите дополнительный материал A");
            if (!string.IsNullOrEmpty(_putDopMaterialA))
            {
                ObnovitStatusFaila(txtStatusDopMaterialA, txtProverkaDopMaterialA, _putDopMaterialA, true);
            }
        }

        private void BtnSohranit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка ввода с помощью ValidationService
                ValidationService.ProveritZadanieSOdnimDop(_putFailaZadaniya, txtOtvet.Text.Trim(), _putDopMaterialA);

                // Сохранение с помощью FileService
                FileService.SohranitZadanieSOdnimDop(_putKhranilishcha, _nomerZadaniya,
                    NomerVarianta, _putFailaZadaniya, txtOtvet.Text.Trim(), _putDopMaterialA);

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