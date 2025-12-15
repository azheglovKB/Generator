using System;
using System.Windows;
using System.IO;
using EgeGenerator;

namespace EgeGenerator
{
    public partial class AddTaskWithTwoExtrasDialog : Window
    {
        private readonly int _nomerZadaniya;
        private readonly string _putKhranilishcha;
        private string _putFailaZadaniya = "";
        private string _putDopMaterialA = "";
        private string _putDopMaterialB = "";

        public int NomerVarianta { get; private set; }

        public AddTaskWithTwoExtrasDialog(int nomerZadaniya, string putKhranilishcha)
        {
            InitializeComponent();

            _nomerZadaniya = nomerZadaniya;
            _putKhranilishcha = putKhranilishcha;

            Title = $"Добавление задания {nomerZadaniya} (с двумя доп. материалами)";
            //txtZagolovok.Text = $"Добавление задания {nomerZadaniya} (с двумя доп. материалами)";

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

        private void BtnZagruzitDopMaterialB_Click(object sender, RoutedEventArgs e)
        {
            _putDopMaterialB = FileService.ZagruzitDopMaterial(this, "Выберите дополнительный материал B");
            if (!string.IsNullOrEmpty(_putDopMaterialB))
            {
                ObnovitStatusFaila(txtStatusDopMaterialB, txtProverkaDopMaterialB, _putDopMaterialB, true);
            }
        }

        private void BtnSohranit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка ввода с помощью ValidationService
                ValidationService.ProveritZadanieSDvumyaDop(_putFailaZadaniya, txtOtvet.Text.Trim(),
                    _putDopMaterialA, _putDopMaterialB);

                // Сохранение с помощью FileService
                FileService.SohranitZadanieSDvumyaDop(_putKhranilishcha, _nomerZadaniya,
                    NomerVarianta, _putFailaZadaniya, txtOtvet.Text.Trim(),
                    _putDopMaterialA, _putDopMaterialB);

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