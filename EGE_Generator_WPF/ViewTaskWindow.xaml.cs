using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EgeGenerator
{
    public partial class ViewTaskWindow : Window
    {
        private readonly string _putPapkiVarianta;
        private List<InformatsiyaOZadanii> _zadaniya = new List<InformatsiyaOZadanii>();
        private int _tekushchiyIndexZadaniya = 0;
        private string _putKFailuOtvetov;

        private class InformatsiyaOZadanii
        {
            public int NomerZadaniya { get; set; }
            public string PapkaZadaniya { get; set; }
            public string PutKIzobrazheniyuZadaniya { get; set; }
            public string TekstOtvet { get; set; }
            public List<string> DopolnitelnyeFaily { get; set; } = new List<string>();
            public string OtobrazhaemoeImyaZadaniya { get; set; }
        }

        public ViewTaskWindow(string putPapkiVarianta)
        {
            InitializeComponent();
            _putPapkiVarianta = putPapkiVarianta;
            ZagruzitZadaniya();
        }

        private void ZagruzitZadaniya()
        {
            try
            {
                string imyaVarianta = Path.GetFileName(_putPapkiVarianta);
                txtVariantInfo.Text = $"Вариант: {imyaVarianta}";
                string papkaZadaniy = Path.Combine(_putPapkiVarianta, "Задания");

                if (!Directory.Exists(papkaZadaniy))
                {
                    MessageBox.Show("Папка с заданиями не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                ZagruzitOtvetyIzPapkiOtvetov();

                for (int nomerZadaniya = 1; nomerZadaniya <= 27; nomerZadaniya++)
                {
                    if (nomerZadaniya >= 19 && nomerZadaniya <= 21)
                    {
                        if (nomerZadaniya == 19)
                        {
                            ZagruzitZadaniya1921(papkaZadaniy);
                        }
                        continue;
                    }

                    string papkaZadaniya = Path.Combine(papkaZadaniy, nomerZadaniya.ToString());
                    if (Directory.Exists(papkaZadaniya))
                    {
                        var informatsiyaOZadanii = ZagruzitInformatsiyuOZadanii(papkaZadaniya, nomerZadaniya);
                        if (informatsiyaOZadanii != null)
                        {
                            _zadaniya.Add(informatsiyaOZadanii);
                        }
                    }
                }

                _zadaniya = _zadaniya.OrderBy(z => z.NomerZadaniya).ToList();

                if (_zadaniya.Count == 0)
                {
                    MessageBox.Show("Задания не найдены", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                    return;
                }

                PokazatZadanie(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заданий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void ZagruzitOtvetyIzPapkiOtvetov()
        {
            try
            {
                string papkaOtvetov = Path.Combine(_putPapkiVarianta, "Ответы");
                if (Directory.Exists(papkaOtvetov))
                {
                    string[] failyOtvetov = Directory.GetFiles(papkaOtvetov, "*.txt");
                    if (failyOtvetov.Length > 0)
                    {
                        _putKFailuOtvetov = failyOtvetov[0];
                    }
                }
            }
            catch
            {
            }
        }

        private void ZagruzitZadaniya1921(string papkaZadaniy)
        {
            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                string papkaZadaniya = Path.Combine(papkaZadaniy, nomerZadaniya.ToString());
                if (Directory.Exists(papkaZadaniya))
                {
                    var informatsiyaOZadanii = ZagruzitInformatsiyuOZadanii(papkaZadaniya, nomerZadaniya);
                    if (informatsiyaOZadanii != null)
                    {
                        informatsiyaOZadanii.OtobrazhaemoeImyaZadaniya = $"Задания 19-21 ({nomerZadaniya})";
                        _zadaniya.Add(informatsiyaOZadanii);
                    }
                }
            }
        }

        private InformatsiyaOZadanii ZagruzitInformatsiyuOZadanii(string papkaZadaniya, int nomerZadaniya)
        {
            try
            {
                string[] faily = Directory.GetFiles(papkaZadaniya);
                if (faily.Length == 0)
                    return null;

                var informatsiyaOZadanii = new InformatsiyaOZadanii
                {
                    NomerZadaniya = nomerZadaniya,
                    PapkaZadaniya = papkaZadaniya,
                    OtobrazhaemoeImyaZadaniya = $"Задание {nomerZadaniya}"
                };

                foreach (string fail in faily)
                {
                    string imyaFaila = Path.GetFileName(fail).ToLower();

                    if (imyaFaila.StartsWith($"{nomerZadaniya}.png") ||
                        imyaFaila.StartsWith($"{nomerZadaniya}.jpg") ||
                        imyaFaila.StartsWith($"{nomerZadaniya}.jpeg") ||
                        imyaFaila == "task.png" ||
                        imyaFaila == "task.jpg" ||
                        imyaFaila == "task.jpeg" ||
                        Path.GetFileNameWithoutExtension(imyaFaila).ToLower() == "task")
                    {
                        informatsiyaOZadanii.PutKIzobrazheniyuZadaniya = fail;
                    }
                    else if (imyaFaila.Contains("answer") && imyaFaila.EndsWith(".txt"))
                    {
                        informatsiyaOZadanii.TekstOtvet = File.ReadAllText(fail).Trim();
                    }
                    else if (!imyaFaila.EndsWith(".png") &&
                             !imyaFaila.EndsWith(".jpg") &&
                             !imyaFaila.EndsWith(".jpeg"))
                    {
                        informatsiyaOZadanii.DopolnitelnyeFaily.Add(fail);
                    }
                }

                if (string.IsNullOrEmpty(informatsiyaOZadanii.TekstOtvet) && !string.IsNullOrEmpty(_putKFailuOtvetov))
                {
                    informatsiyaOZadanii.TekstOtvet = NaytiOtvetVFaileOtvetov(nomerZadaniya);
                }

                return informatsiyaOZadanii;
            }
            catch
            {
                return null;
            }
        }

        private string NaytiOtvetVFaileOtvetov(int nomerZadaniya)
        {
            try
            {
                if (string.IsNullOrEmpty(_putKFailuOtvetov) || !File.Exists(_putKFailuOtvetov))
                    return null;

                string vseOtvety = File.ReadAllText(_putKFailuOtvetov);
                string[] stroki = vseOtvety.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string stroka in stroki)
                {
                    string obrezannayaStroka = stroka.Trim();
                    bool etoStrokaZadaniya = false;
                    string chastOtvet = "";

                    if (obrezannayaStroka.StartsWith($"{nomerZadaniya}. "))
                    {
                        etoStrokaZadaniya = true;
                        chastOtvet = obrezannayaStroka.Substring($"{nomerZadaniya}. ".Length);
                    }
                    else if (obrezannayaStroka.StartsWith($"{nomerZadaniya}) "))
                    {
                        etoStrokaZadaniya = true;
                        chastOtvet = obrezannayaStroka.Substring($"{nomerZadaniya}) ".Length);
                    }
                    else if (obrezannayaStroka.StartsWith($"{nomerZadaniya} - "))
                    {
                        etoStrokaZadaniya = true;
                        chastOtvet = obrezannayaStroka.Substring($"{nomerZadaniya} - ".Length);
                    }
                    else if (obrezannayaStroka.StartsWith($"{nomerZadaniya} – "))
                    {
                        etoStrokaZadaniya = true;
                        chastOtvet = obrezannayaStroka.Substring($"{nomerZadaniya} – ".Length);
                    }
                    else if (obrezannayaStroka.StartsWith($"{nomerZadaniya} ") &&
                             obrezannayaStroka.Length > nomerZadaniya.ToString().Length + 1)
                    {
                        string posleNomera = obrezannayaStroka.Substring(nomerZadaniya.ToString().Length).TrimStart();
                        if (!string.IsNullOrEmpty(posleNomera))
                        {
                            etoStrokaZadaniya = true;
                            chastOtvet = posleNomera;
                        }
                    }
                    else if (obrezannayaStroka.StartsWith($"{nomerZadaniya}: "))
                    {
                        etoStrokaZadaniya = true;
                        chastOtvet = obrezannayaStroka.Substring($"{nomerZadaniya}: ".Length);
                    }

                    if (etoStrokaZadaniya && !string.IsNullOrEmpty(chastOtvet))
                    {
                        string[] strokiOtvet = chastOtvet.Split(
                            new[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.RemoveEmptyEntries);

                        if (strokiOtvet.Length > 1)
                        {
                            return string.Join(" ", strokiOtvet.Select(l => l.Trim()));
                        }
                        else
                        {
                            return chastOtvet.Trim();
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private void PokazatZadanie(int index)
        {
            if (index < 0 || index >= _zadaniya.Count)
                return;

            _tekushchiyIndexZadaniya = index;
            var zadanie = _zadaniya[index];
            txtTaskTitle.Text = zadanie.OtobrazhaemoeImyaZadaniya;

            if (!string.IsNullOrEmpty(zadanie.PutKIzobrazheniyuZadaniya) && File.Exists(zadanie.PutKIzobrazheniyuZadaniya))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(zadanie.PutKIzobrazheniyuZadaniya);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    taskImage.Source = bitmap;
                }
                catch
                {
                    taskImage.Source = null;
                }
            }
            else
            {
                taskImage.Source = null;
            }

            string tekstOtvet = zadanie.TekstOtvet;
            if (string.IsNullOrEmpty(tekstOtvet))
            {
                tekstOtvet = "Ответ не найден";
                txtAnswer.Foreground = Brushes.Red;
            }
            else
            {
                txtAnswer.Foreground = Brushes.Black;
                if (tekstOtvet.Contains("\r") || tekstOtvet.Contains("\n"))
                {
                    string[] strokiOtvet = tekstOtvet.Split(
                        new[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries);

                    tekstOtvet = string.Join(" ", strokiOtvet.Select(l => l.Trim()));
                }
            }
            txtAnswer.Text = tekstOtvet;

            extraFilesStackPanel.Children.Clear();
            if (zadanie.DopolnitelnyeFaily.Count > 0)
            {
                extraMaterialsPanel.Visibility = Visibility.Visible;
                foreach (string fail in zadanie.DopolnitelnyeFaily)
                {
                    string imyaFaila = Path.GetFileName(fail);
                    var knopka = new Button
                    {
                        Content = imyaFaila,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 0, 0, 5),
                        Padding = new Thickness(10, 5, 10, 5),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Background = Brushes.LightGray
                    };

                    knopka.Click += (s, e) => OtkrytDopolnitelnyFail(fail);
                    extraFilesStackPanel.Children.Add(knopka);
                }
            }
            else
            {
                extraMaterialsPanel.Visibility = Visibility.Collapsed;
            }

            btnPrev.IsEnabled = _tekushchiyIndexZadaniya > 0;
            btnNext.IsEnabled = _tekushchiyIndexZadaniya < _zadaniya.Count - 1;
        }

        private void OtkrytDopolnitelnyFail(string putKFailu)
        {
            try
            {
                if (File.Exists(putKFailu))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = putKFailu,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Файл не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_tekushchiyIndexZadaniya > 0)
            {
                PokazatZadanie(_tekushchiyIndexZadaniya - 1);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_tekushchiyIndexZadaniya < _zadaniya.Count - 1)
            {
                PokazatZadanie(_tekushchiyIndexZadaniya + 1);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}