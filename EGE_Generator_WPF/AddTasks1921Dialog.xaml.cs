using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using EgeGenerator;

namespace EgeGenerator
{
    public partial class AddTasks1921Dialog : Window
    {
        private readonly string _putKhranilishcha;
        
        // Словари для хранения данных по заданиям
        private readonly Dictionary<int, string> _failiZadaniy = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _tekstiOtvetov = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _failiOtvetov = new Dictionary<int, string>();
        
        public int NomerVarianta { get; private set; }

        public AddTasks1921Dialog(string putKhranilishcha)
        {
            InitializeComponent();
            _putKhranilishcha = putKhranilishcha;
            
            Title = "Добавление заданий 19-21";
            //txtZagolovok.Text = "Добавление заданий 19-21";
            
            InitsializirovatPolya();
            ObnovitNomerVarianta();
            ObnovitStatusVsehZadaniy();
        }

        private void InitsializirovatPolya()
        {
            // Инициализируем словари для трех заданий
            for (int i = 19; i <= 21; i++)
            {
                _failiZadaniy[i] = "";
                _tekstiOtvetov[i] = "";
                _failiOtvetov[i] = "";
            }
            
            NomerVarianta = 0;
        }

        private void ObnovitNomerVarianta()
        {
            string putPapki1921 = StorageService.PoluchitPutPapkiZadaniya(_putKhranilishcha, 19); // 19 для получения папки 19-21
            NomerVarianta = FileService.NaytiDostupnyNomerVarianta(putPapki1921);
            txtInformatsiyaOVariante.Text = $"Будет создан вариант № {NomerVarianta}";
            
            if (!Directory.Exists(putPapki1921))
            {
                txtInformatsiyaOVariante.Text += " (новая папка)";
            }
        }

        private void ObnovitStatusVsehZadaniy()
        {
            ObnovitStatusOdnogoZadaniya(19);
            ObnovitStatusOdnogoZadaniya(20);
            ObnovitStatusOdnogoZadaniya(21);
        }

        private void ObnovitStatusOdnogoZadaniya(int nomerZadaniya)
        {
            // Получаем контролы для задания
            var kontroly = PoluchitKontrolyDlyaZadaniya(nomerZadaniya);
            if (kontroly == null) return;

            // Обновляем статус задания
            ObnovitStatusZadaniya(kontroly.Item1, kontroly.Item2, _failiZadaniy[nomerZadaniya]);
            
            // Обновляем статус ответа
            ObnovitStatusOtvet(kontroly.Item3, kontroly.Item4, kontroly.Item5, nomerZadaniya);
        }

        private Tuple<TextBlock, TextBlock, TextBlock, TextBlock, TextBox> PoluchitKontrolyDlyaZadaniya(int nomerZadaniya)
        {
            switch (nomerZadaniya)
            {
                case 19:
                    return Tuple.Create(txtStatusZadaniya19, txtProverkaZadaniya19, 
                        txtStatusOtvet19, txtProverkaOtvet19, txtOtvet19);
                case 20:
                    return Tuple.Create(txtStatusZadaniya20, txtProverkaZadaniya20, 
                        txtStatusOtvet20, txtProverkaOtvet20, txtOtvet20);
                case 21:
                    return Tuple.Create(txtStatusZadaniya21, txtProverkaZadaniya21, 
                        txtStatusOtvet21, txtProverkaOtvet21, txtOtvet21);
                default:
                    return null;
            }
        }

        private void ObnovitStatusZadaniya(TextBlock statusTekst, TextBlock checkTekst, string putKFailu)
        {
            if (!string.IsNullOrEmpty(putKFailu) && File.Exists(putKFailu))
            {
                statusTekst.Text = Path.GetFileName(putKFailu);
                checkTekst.Text = "✓";
                checkTekst.Foreground = System.Windows.Media.Brushes.Green;
                checkTekst.Visibility = Visibility.Visible;
            }
            else
            {
                statusTekst.Text = "Задание не загружено";
                checkTekst.Text = "✗";
                checkTekst.Foreground = System.Windows.Media.Brushes.Red;
                checkTekst.Visibility = Visibility.Visible;
            }
        }

        private void ObnovitStatusOtvet(TextBlock statusTekst, TextBlock checkTekst, 
                                       TextBox tekstovoePoleOtvet, int nomerZadaniya)
        {
            bool imeetOtvetIzFaila = !string.IsNullOrEmpty(_failiOtvetov[nomerZadaniya]) && 
                                     File.Exists(_failiOtvetov[nomerZadaniya]);
            bool imeetOtvetIzTeksta = !string.IsNullOrEmpty(_tekstiOtvetov[nomerZadaniya]) && 
                                      _tekstiOtvetov[nomerZadaniya].Trim().Length > 0;

            if (imeetOtvetIzFaila)
            {
                statusTekst.Text = "Ответ из файла";
                checkTekst.Text = "✓";
                checkTekst.Foreground = System.Windows.Media.Brushes.Green;
                checkTekst.Visibility = Visibility.Visible;
                
                if (tekstovoePoleOtvet != null)
                {
                    string soderzhimoeFaila = File.ReadAllText(_failiOtvetov[nomerZadaniya]).Trim();
                    tekstovoePoleOtvet.Text = soderzhimoeFaila;
                    _tekstiOtvetov[nomerZadaniya] = soderzhimoeFaila;
                }
            }
            else if (imeetOtvetIzTeksta)
            {
                statusTekst.Text = "Ответ введен вручную";
                checkTekst.Text = "✓";
                checkTekst.Foreground = System.Windows.Media.Brushes.Green;
                checkTekst.Visibility = Visibility.Visible;
            }
            else
            {
                statusTekst.Text = "Ответ не загружен";
                checkTekst.Text = "✗";
                checkTekst.Foreground = System.Windows.Media.Brushes.Red;
                checkTekst.Visibility = Visibility.Visible;
            }

            if (tekstovoePoleOtvet != null && imeetOtvetIzTeksta)
            {
                tekstovoePoleOtvet.Text = _tekstiOtvetov[nomerZadaniya];
            }
        }

        // Обработчики загрузки заданий
        private void BtnZagruzitZadanie19_Click(object sender, RoutedEventArgs e) => ZagruzitZadanie(19);
        private void BtnZagruzitZadanie20_Click(object sender, RoutedEventArgs e) => ZagruzitZadanie(20);
        private void BtnZagruzitZadanie21_Click(object sender, RoutedEventArgs e) => ZagruzitZadanie(21);

        private void ZagruzitZadanie(int nomerZadaniya)
        {
            _failiZadaniy[nomerZadaniya] = FileService.ZagruzitFailZadaniya(this);
            if (!string.IsNullOrEmpty(_failiZadaniy[nomerZadaniya]))
            {
                ObnovitStatusOdnogoZadaniya(nomerZadaniya);
            }
        }

        // Обработчики загрузки ответов
        private void BtnZagruzitOtvet19_Click(object sender, RoutedEventArgs e) => ZagruzitOtvet(19);
        private void BtnZagruzitOtvet20_Click(object sender, RoutedEventArgs e) => ZagruzitOtvet(20);
        private void BtnZagruzitOtvet21_Click(object sender, RoutedEventArgs e) => ZagruzitOtvet(21);

        private void ZagruzitOtvet(int nomerZadaniya)
        {
            string putFailaOtvet = FileService.ZagruzitFailOtvet(this);
            if (!string.IsNullOrEmpty(putFailaOtvet))
            {
                _failiOtvetov[nomerZadaniya] = putFailaOtvet;
                _tekstiOtvetov[nomerZadaniya] = File.ReadAllText(putFailaOtvet).Trim();
                ObnovitStatusOdnogoZadaniya(nomerZadaniya);
            }
        }

        // Обработчики изменения текста ответов
        private void TxtOtvet19_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tekstiOtvetov[19] = txtOtvet19.Text;
            ObnovitStatusOdnogoZadaniya(19);
        }

        private void TxtOtvet20_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tekstiOtvetov[20] = txtOtvet20.Text;
            ObnovitStatusOdnogoZadaniya(20);
        }

        private void TxtOtvet21_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tekstiOtvetov[21] = txtOtvet21.Text;
            ObnovitStatusOdnogoZadaniya(21);
        }

        private void BtnSohranit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка ввода с помощью ValidationService
                ValidationService.ProveritZadanie1921(_failiZadaniy, _tekstiOtvetov, _failiOtvetov);
                
                // Сохранение с помощью FileService
                FileService.SohranitZadaniya1921(_putKhranilishcha, NomerVarianta, 
                    _failiZadaniy, _tekstiOtvetov, _failiOtvetov);
                
                MessageBox.Show($"Все 3 задания (19, 20, 21) успешно добавлены в вариант {NomerVarianta}!",
                               "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}