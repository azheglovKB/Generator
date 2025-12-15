using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace EgeGenerator
{
    public partial class AddVariantDialog : Window
    {
        private readonly int _nomerZadaniya;
        private readonly string _putKhranilishcha;
        private string _putFailaZadaniya;
        private string _putFailaOtvet;
        private string _putDopMaterialA;
        private string _putDopMaterialB;
        private readonly Dictionary<int, string> _failiZadaniy1921 = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _tekstiOtvetov1921 = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _failiOtvetov1921 = new Dictionary<int, string>();
        private static readonly HashSet<int> _zadaniyaSOdnimDop = new HashSet<int> { 3, 9, 10, 17, 18, 22, 24, 26 };
        private static readonly HashSet<int> _zadaniyaSDvumyaDop = new HashSet<int> { 27 };
        private static readonly string[] _razreshennyeRasshireniyaDop = { ".txt", ".ods", ".xlsx", ".xls", ".pdf", ".doc", ".docx" };
        private const string PAPKA_1921 = "19-21";
        public int NomerVarianta { get; private set; }

        public AddVariantDialog(int nomerZadaniya, string putKhranilishcha)
        {
            InitializeComponent();
            _nomerZadaniya = nomerZadaniya;
            _putKhranilishcha = putKhranilishcha;
            InitsializirovatPolya();
            InitsializirovatUI();
            ObnovitNomerVarianta();
        }

        private void InitsializirovatPolya()
        {
            _putFailaZadaniya = "";
            _putFailaOtvet = "";
            _putDopMaterialA = "";
            _putDopMaterialB = "";
            NomerVarianta = 0;

            if (_nomerZadaniya >= 19 && _nomerZadaniya <= 21)
            {
                for (int i = 19; i <= 21; i++)
                {
                    _failiZadaniy1921[i] = "";
                    _tekstiOtvetov1921[i] = "";
                    _failiOtvetov1921[i] = "";
                }
            }
        }

        private void InitsializirovatUI()
        {
            if (_nomerZadaniya >= 19 && _nomerZadaniya <= 21)
            {
                InitsializirovatUIDlya1921();
            }
            else
            {
                InitsializirovatUIDlyaObychnogoZadaniya();
            }
        }

        private void InitsializirovatUIDlya1921()
        {
            txtTitle.Text = "Добавление заданий 19-21";
            borderTask.Visibility = Visibility.Collapsed;
            borderAnswer.Visibility = Visibility.Collapsed;
            borderExtraA.Visibility = Visibility.Collapsed;
            borderExtraB.Visibility = Visibility.Collapsed;
            border1921.Visibility = Visibility.Visible;
            ObnovitStatus1921();
        }

        private void InitsializirovatUIDlyaObychnogoZadaniya()
        {
            txtTitle.Text = $"Добавление варианта задания {_nomerZadaniya}";
            if (_zadaniyaSOdnimDop.Contains(_nomerZadaniya))
            {
                borderExtraA.Visibility = Visibility.Visible;
                borderExtraB.Visibility = Visibility.Collapsed;
            }
            else if (_zadaniyaSDvumyaDop.Contains(_nomerZadaniya))
            {
                borderExtraA.Visibility = Visibility.Visible;
                borderExtraB.Visibility = Visibility.Visible;
            }
            else
            {
                borderExtraA.Visibility = Visibility.Collapsed;
                borderExtraB.Visibility = Visibility.Collapsed;
            }
        }

        private void ObnovitNomerVarianta()
        {
            string putPapkiZadaniya = PoluchitPutPapkiZadaniya();
            if (!Directory.Exists(putPapkiZadaniya))
            {
                NomerVarianta = 1;
                txtVariantInfo.Text = $"Будет создан вариант № {NomerVarianta} (новая папка)";
                return;
            }

            HashSet<int> sushchestvuyushchieNomera = PoluchitSushchestvuyushchieNomeraVariantov(putPapkiZadaniya);
            NomerVarianta = NaytiDostupnyNomerVarianta(sushchestvuyushchieNomera);
            txtVariantInfo.Text = $"Будет создан вариант № {NomerVarianta}";
        }

        private string PoluchitPutPapkiZadaniya()
        {
            return _nomerZadaniya >= 19 && _nomerZadaniya <= 21
                ? Path.Combine(_putKhranilishcha, PAPKA_1921)
                : Path.Combine(_putKhranilishcha, _nomerZadaniya.ToString());
        }

        private static HashSet<int> PoluchitSushchestvuyushchieNomeraVariantov(string putPapkiZadaniya)
        {
            var sushchestvuyushchieNomera = new HashSet<int>();
            foreach (string variant in Directory.GetDirectories(putPapkiZadaniya))
            {
                string imyaPapki = Path.GetFileName(variant);
                if (int.TryParse(imyaPapki, out int nomer))
                {
                    sushchestvuyushchieNomera.Add(nomer);
                }
            }
            return sushchestvuyushchieNomera;
        }

        private static int NaytiDostupnyNomerVarianta(HashSet<int> sushchestvuyushchieNomera)
        {
            for (int i = 1; i <= 1000; i++)
            {
                if (!sushchestvuyushchieNomera.Contains(i))
                {
                    return i;
                }
            }
            return sushchestvuyushchieNomera.Max() + 1;
        }

        private void ObnovitStatus1921()
        {
            ObnovitStatusOdnogo1921(19);
            ObnovitStatusOdnogo1921(20);
            ObnovitStatusOdnogo1921(21);
        }

        private void ObnovitStatusOdnogo1921(int nomerZadaniya)
        {
            var kontroly = PoluchitKontrolyDlyaZadaniya(nomerZadaniya);
            if (kontroly == null) return;

            ObnovitStatusZadaniya(kontroly.Item1, kontroly.Item2, _failiZadaniy1921[nomerZadaniya]);
            ObnovitStatusOtvet(kontroly.Item3, kontroly.Item4, kontroly.Item5, nomerZadaniya);
        }

        private Tuple<TextBlock, TextBlock, TextBlock, TextBlock, TextBox> PoluchitKontrolyDlyaZadaniya(int nomerZadaniya)
        {
            switch (nomerZadaniya)
            {
                case 19:
                    return Tuple.Create<TextBlock, TextBlock, TextBlock, TextBlock, TextBox>(
                        txtTask19Status, txtTask19Check, txtAnswer19Status, txtAnswer19Check, txtAnswer19);
                case 20:
                    return Tuple.Create<TextBlock, TextBlock, TextBlock, TextBlock, TextBox>(
                        txtTask20Status, txtTask20Check, txtAnswer20Status, txtAnswer20Check, txtAnswer20);
                case 21:
                    return Tuple.Create<TextBlock, TextBlock, TextBlock, TextBlock, TextBox>(
                        txtTask21Status, txtTask21Check, txtAnswer21Status, txtAnswer21Check, txtAnswer21);
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

        private void ObnovitStatusOtvet(TextBlock statusTekst, TextBlock checkTekst, TextBox tekstovoePoleOtvet, int nomerZadaniya)
        {
            bool imeetOtvetIzFaila = !string.IsNullOrEmpty(_failiOtvetov1921[nomerZadaniya]) && File.Exists(_failiOtvetov1921[nomerZadaniya]);
            bool imeetOtvetIzTeksta = !string.IsNullOrEmpty(_tekstiOtvetov1921[nomerZadaniya]) && _tekstiOtvetov1921[nomerZadaniya].Trim().Length > 0;

            if (imeetOtvetIzFaila)
            {
                statusTekst.Text = "Ответ из файла";
                checkTekst.Text = "✓";
                checkTekst.Foreground = System.Windows.Media.Brushes.Green;
                checkTekst.Visibility = Visibility.Visible;
                if (tekstovoePoleOtvet != null)
                {
                    string soderzhimoeFaila = File.ReadAllText(_failiOtvetov1921[nomerZadaniya]).Trim();
                    tekstovoePoleOtvet.Text = soderzhimoeFaila;
                    _tekstiOtvetov1921[nomerZadaniya] = soderzhimoeFaila;
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
                tekstovoePoleOtvet.Text = _tekstiOtvetov1921[nomerZadaniya];
            }
        }

        private void BtnBrowseTask19_Click(object sender, RoutedEventArgs e) => ZagruzitZadanieDlya1921(19);
        private void BtnBrowseTask20_Click(object sender, RoutedEventArgs e) => ZagruzitZadanieDlya1921(20);
        private void BtnBrowseTask21_Click(object sender, RoutedEventArgs e) => ZagruzitZadanieDlya1921(21);
        private void BtnBrowseAnswer19_Click(object sender, RoutedEventArgs e) => ZagruzitOtvetDlya1921(19);
        private void BtnBrowseAnswer20_Click(object sender, RoutedEventArgs e) => ZagruzitOtvetDlya1921(20);
        private void BtnBrowseAnswer21_Click(object sender, RoutedEventArgs e) => ZagruzitOtvetDlya1921(21);

        private void TxtAnswer19_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tekstiOtvetov1921[19] = txtAnswer19.Text;
            ObnovitStatusOdnogo1921(19);
        }

        private void TxtAnswer20_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tekstiOtvetov1921[20] = txtAnswer20.Text;
            ObnovitStatusOdnogo1921(20);
        }

        private void TxtAnswer21_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tekstiOtvetov1921[21] = txtAnswer21.Text;
            ObnovitStatusOdnogo1921(21);
        }

        private void ZagruzitZadanieDlya1921(int nomerZadaniya)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg",
                Title = $"Выберите файл задания {nomerZadaniya}"
            };

            if (dialog.ShowDialog() == true)
            {
                string rasshirenieZadaniya = Path.GetExtension(dialog.FileName).ToLower();
                if (rasshirenieZadaniya != ".png" && rasshirenieZadaniya != ".jpg" && rasshirenieZadaniya != ".jpeg")
                {
                    MessageBox.Show($"Файл задания {nomerZadaniya} должен быть в формате png или jpg",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _failiZadaniy1921[nomerZadaniya] = dialog.FileName;
                ObnovitStatusOdnogo1921(nomerZadaniya);
            }
        }

        private void ZagruzitOtvetDlya1921(int nomerZadaniya)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы|*.txt",
                Title = $"Выберите файл с ответом для задания {nomerZadaniya}"
            };

            if (dialog.ShowDialog() == true)
            {
                string soderzhimoe = File.ReadAllText(dialog.FileName).Trim();
                if (string.IsNullOrEmpty(soderzhimoe))
                {
                    MessageBox.Show($"Файл ответа для задания {nomerZadaniya} пустой.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _failiOtvetov1921[nomerZadaniya] = dialog.FileName;
                _tekstiOtvetov1921[nomerZadaniya] = soderzhimoe;
                ObnovitStatusOdnogo1921(nomerZadaniya);
            }
        }

        private void BtnBrowseTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg",
                Title = "Выберите файл задания"
            };

            if (dialog.ShowDialog() == true)
            {
                _putFailaZadaniya = dialog.FileName;
                ObnovitStatusFaila(txtTaskFile, txtTaskCheck, _putFailaZadaniya, true);
            }
        }

        private void BtnBrowseAnswer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы|*.txt",
                Title = "Выберите файл с ответом"
            };

            if (dialog.ShowDialog() == true)
            {
                _putFailaOtvet = dialog.FileName;
                string soderzhimoe = File.ReadAllText(_putFailaOtvet).Trim();
                if (string.IsNullOrEmpty(soderzhimoe))
                {
                    MessageBox.Show("Файл ответа пустой. Введите ответ вручную или выберите другой файл.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _putFailaOtvet = "";
                }
                else
                {
                    txtAnswer.Text = soderzhimoe;
                }
            }
        }

        private void BtnBrowseExtraA_Click(object sender, RoutedEventArgs e)
        {
            ZagruzitDopMaterial("Выберите дополнительный материал A", ref _putDopMaterialA,
                txtExtraAFile, txtExtraACheck,
                _zadaniyaSOdnimDop.Contains(_nomerZadaniya) || _zadaniyaSDvumyaDop.Contains(_nomerZadaniya));
        }

        private void BtnBrowseExtraB_Click(object sender, RoutedEventArgs e)
        {
            ZagruzitDopMaterial("Выберите дополнительный материал B", ref _putDopMaterialB,
                txtExtraBFile, txtExtraBCheck,
                _zadaniyaSDvumyaDop.Contains(_nomerZadaniya));
        }

        private void ZagruzitDopMaterial(string zagolovok, ref string putKFailu, TextBlock statusTekst, TextBlock checkTekst, bool obyazatelny)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Дополнительные файлы|*.txt;*.ods;*.xlsx;*.xls;*.pdf;*.doc;*.docx",
                Title = zagolovok
            };

            if (dialog.ShowDialog() == true)
            {
                string rasshirenie = Path.GetExtension(dialog.FileName).ToLower();
                if (_razreshennyeRasshireniyaDop.Contains(rasshirenie))
                {
                    putKFailu = dialog.FileName;
                    ObnovitStatusFaila(statusTekst, checkTekst, putKFailu, obyazatelny, _razreshennyeRasshireniyaDop);
                }
                else
                {
                    MessageBox.Show($"Недопустимый формат файла. Разрешенные форматы: {string.Join(", ", _razreshennyeRasshireniyaDop)}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ProveritVvod()
        {
            return _nomerZadaniya >= 19 && _nomerZadaniya <= 21
                ? ProveritVvodDlya1921()
                : ProveritVvodDlyaObychnogoZadaniya();
        }

        private bool ProveritVvodDlya1921()
        {
            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                if (string.IsNullOrEmpty(_failiZadaniy1921[nomerZadaniya]) || !File.Exists(_failiZadaniy1921[nomerZadaniya]))
                {
                    MessageBox.Show($"Загрузите файл задания {nomerZadaniya} (png или jpg)",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string rasshirenieZadaniya = Path.GetExtension(_failiZadaniy1921[nomerZadaniya]).ToLower();
                if (rasshirenieZadaniya != ".png" && rasshirenieZadaniya != ".jpg" && rasshirenieZadaniya != ".jpeg")
                {
                    MessageBox.Show($"Файл задания {nomerZadaniya} должен быть в формате png или jpg",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                bool imeetOtvetIzFaila = !string.IsNullOrEmpty(_failiOtvetov1921[nomerZadaniya]) && File.Exists(_failiOtvetov1921[nomerZadaniya]);
                bool imeetOtvetIzTeksta = !string.IsNullOrEmpty(_tekstiOtvetov1921[nomerZadaniya]) && _tekstiOtvetov1921[nomerZadaniya].Trim().Length > 0;

                if (!imeetOtvetIzFaila && !imeetOtvetIzTeksta)
                {
                    MessageBox.Show($"Введите ответ для задания {nomerZadaniya} вручную или загрузите файл с ответом",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (imeetOtvetIzFaila)
                {
                    string soderzhimoe = File.ReadAllText(_failiOtvetov1921[nomerZadaniya]).Trim();
                    if (string.IsNullOrEmpty(soderzhimoe))
                    {
                        MessageBox.Show($"Файл ответа для задания {nomerZadaniya} пустой. Введите ответ вручную или выберите другой файл.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ProveritVvodDlyaObychnogoZadaniya()
        {
            if (string.IsNullOrEmpty(_putFailaZadaniya))
            {
                MessageBox.Show("Загрузите файл задания (png или jpg)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string rasshirenieZadaniya = Path.GetExtension(_putFailaZadaniya).ToLower();
            if (rasshirenieZadaniya != ".png" && rasshirenieZadaniya != ".jpg" && rasshirenieZadaniya != ".jpeg")
            {
                MessageBox.Show("Файл задания должен быть в формате png или jpg",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string tekstOtvet = txtAnswer.Text.Trim();
            if (string.IsNullOrEmpty(tekstOtvet))
            {
                MessageBox.Show("Введите ответ вручную или загрузите файл с ответом",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((_zadaniyaSOdnimDop.Contains(_nomerZadaniya) || _zadaniyaSDvumyaDop.Contains(_nomerZadaniya))
                && string.IsNullOrEmpty(_putDopMaterialA))
            {
                MessageBox.Show("Загрузите дополнительный материал A",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (_zadaniyaSDvumyaDop.Contains(_nomerZadaniya) && string.IsNullOrEmpty(_putDopMaterialB))
            {
                MessageBox.Show("Загрузите дополнительный материал B",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ProveritVvod())
                return;

            try
            {
                if (_nomerZadaniya >= 19 && _nomerZadaniya <= 21)
                {
                    SohranitZadaniya1921();
                }
                else
                {
                    SohranitObychnoeZadanie();
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}\n\nПовторите попытку.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SohranitZadaniya1921()
        {
            string putPapki1921 = Path.Combine(_putKhranilishcha, PAPKA_1921);
            Directory.CreateDirectory(putPapki1921);

            string putPapkiVarianta = Path.Combine(putPapki1921, NomerVarianta.ToString());
            Directory.CreateDirectory(putPapkiVarianta);

            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                string putPapkiZadaniya = Path.Combine(putPapkiVarianta, nomerZadaniya.ToString());
                Directory.CreateDirectory(putPapkiZadaniya);

                string putKopiiZadaniya = Path.Combine(putPapkiZadaniya, "task" + Path.GetExtension(_failiZadaniy1921[nomerZadaniya]));
                File.Copy(_failiZadaniy1921[nomerZadaniya], putKopiiZadaniya, true);

                string tekstOtvet = _tekstiOtvetov1921[nomerZadaniya];
                if (string.IsNullOrEmpty(tekstOtvet) && !string.IsNullOrEmpty(_failiOtvetov1921[nomerZadaniya]))
                {
                    tekstOtvet = File.ReadAllText(_failiOtvetov1921[nomerZadaniya]).Trim();
                }

                string putKopiiOtvet = Path.Combine(putPapkiZadaniya, "answer.txt");
                File.WriteAllText(putKopiiOtvet, tekstOtvet);
            }

            MessageBox.Show($"Все 3 задания (19, 20, 21) успешно добавлены в вариант {NomerVarianta}!",
                           "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SohranitObychnoeZadanie()
        {
            string putPapkiZadaniya = Path.Combine(_putKhranilishcha, _nomerZadaniya.ToString());
            Directory.CreateDirectory(putPapkiZadaniya);

            string putPapkiVarianta = Path.Combine(putPapkiZadaniya, NomerVarianta.ToString());
            Directory.CreateDirectory(putPapkiVarianta);

            string putKopiiZadaniya = Path.Combine(putPapkiVarianta, "task" + Path.GetExtension(_putFailaZadaniya));
            File.Copy(_putFailaZadaniya, putKopiiZadaniya, true);

            string putKopiiOtvet = Path.Combine(putPapkiVarianta, "answer.txt");
            File.WriteAllText(putKopiiOtvet, txtAnswer.Text.Trim());

            if (!string.IsNullOrEmpty(_putDopMaterialA))
            {
                string putKopiiDopA = Path.Combine(putPapkiVarianta, "A" + Path.GetExtension(_putDopMaterialA));
                File.Copy(_putDopMaterialA, putKopiiDopA, true);
            }

            if (!string.IsNullOrEmpty(_putDopMaterialB))
            {
                string putKopiiDopB = Path.Combine(putPapkiVarianta, "B" + Path.GetExtension(_putDopMaterialB));
                File.Copy(_putDopMaterialB, putKopiiDopB, true);
            }
        }

        private void ObnovitStatusFaila(TextBlock statusTekst, TextBlock checkTekst, string putKFailu,
            bool obyazatelny, string[] razreshennyeRasshireniya = null)
        {
            if (!string.IsNullOrEmpty(putKFailu) && File.Exists(putKFailu))
            {
                statusTekst.Text = Path.GetFileName(putKFailu);
                if (razreshennyeRasshireniya != null)
                {
                    string rasshirenie = Path.GetExtension(putKFailu).ToLower();
                    if (razreshennyeRasshireniya.Contains(rasshirenie))
                    {
                        checkTekst.Text = "✓";
                        checkTekst.Foreground = System.Windows.Media.Brushes.Green;
                        checkTekst.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        checkTekst.Text = "✗";
                        checkTekst.Foreground = System.Windows.Media.Brushes.Red;
                        checkTekst.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    checkTekst.Text = "✓";
                    checkTekst.Foreground = System.Windows.Media.Brushes.Green;
                    checkTekst.Visibility = Visibility.Visible;
                }
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