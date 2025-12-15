using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace EgeGenerator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string putKhranilishcha = PoluchitIliSozdatKhranilishcheDlyaDobavleniya();
                if (string.IsNullOrEmpty(putKhranilishcha))
                    return;

                var dialogNomerZadaniya = new TaskNumberDialog();
                if (dialogNomerZadaniya.ShowDialog() == true)
                {
                    int nomerZadaniya = dialogNomerZadaniya.NomerZadaniya;
                    var dialogDobavleniyaVarianta = new AddVariantDialog(nomerZadaniya, putKhranilishcha);
                    if (dialogDobavleniyaVarianta.ShowDialog() == true)
                    {
                        MessageBox.Show($"Успешно добавлен вариант {dialogDobavleniyaVarianta.NomerVarianta} для задания {nomerZadaniya}!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\nПовторите попытку.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string PoluchitIliSozdatKhranilishcheDlyaDobavleniya()
        {
            string putKhranilishcha = VybratPapku("Выберите папку хранилища с заданиями");
            if (string.IsNullOrEmpty(putKhranilishcha))
                return "";

            if (Directory.Exists(putKhranilishcha))
            {
                bool imeetLyubyePapkiZadaniy = false;
                for (int i = 1; i <= 27; i++)
                {
                    if (i >= 19 && i <= 21) continue;
                    string papkaZadaniya = Path.Combine(putKhranilishcha, i.ToString());
                    if (Directory.Exists(papkaZadaniya))
                    {
                        imeetLyubyePapkiZadaniy = true;
                        break;
                    }
                }

                string papka1921 = Path.Combine(putKhranilishcha, "19-21");
                if (Directory.Exists(papka1921))
                {
                    imeetLyubyePapkiZadaniy = true;
                }

                if (!imeetLyubyePapkiZadaniy)
                {
                    var rezultat = MessageBox.Show("В выбранной папке нет заданий. Создать папки для заданий?\n\n(26 папок: 1-18, 19-21, 22-27)",
                        "Создание структуры", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (rezultat == MessageBoxResult.Yes)
                    {
                        SozdatVsePapkiZadaniy(putKhranilishcha);
                        MessageBox.Show("Созданы папки для заданий. Теперь можно добавлять варианты.",
                            "Структура создана", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            else
            {
                var rezultat = MessageBox.Show($"Папка не существует. Создать папку хранилища и папки для заданий?\n\n{putKhranilishcha}",
                    "Создание хранилища", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (rezultat == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(putKhranilishcha);
                        SozdatVsePapkiZadaniy(putKhranilishcha);
                        MessageBox.Show($"Создано хранилище и папки для заданий.\n\n{putKhranilishcha}",
                            "Хранилище создано", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось создать папку: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }

            return putKhranilishcha;
        }

        private void SozdatVsePapkiZadaniy(string putKhranilishcha)
        {
            List<string> sozdannyePapki = new List<string>();
            for (int nomerZadaniya = 1; nomerZadaniya <= 18; nomerZadaniya++)
            {
                string putPapkiZadaniya = Path.Combine(putKhranilishcha, nomerZadaniya.ToString());
                if (!Directory.Exists(putPapkiZadaniya))
                {
                    try
                    {
                        Directory.CreateDirectory(putPapkiZadaniya);
                        sozdannyePapki.Add(nomerZadaniya.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось создать папку для задания {nomerZadaniya}: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            string putPapki1921 = Path.Combine(putKhranilishcha, "19-21");
            if (!Directory.Exists(putPapki1921))
            {
                try
                {
                    Directory.CreateDirectory(putPapki1921);
                    sozdannyePapki.Add("19-21");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось создать папку для заданий 19-21: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            for (int nomerZadaniya = 22; nomerZadaniya <= 27; nomerZadaniya++)
            {
                string putPapkiZadaniya = Path.Combine(putKhranilishcha, nomerZadaniya.ToString());
                if (!Directory.Exists(putPapkiZadaniya))
                {
                    try
                    {
                        Directory.CreateDirectory(putPapkiZadaniya);
                        sozdannyePapki.Add(nomerZadaniya.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось создать папку для задания {nomerZadaniya}: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            if (sozdannyePapki.Count > 0)
            {
                Console.WriteLine($"Созданы папки: {string.Join(", ", sozdannyePapki)}");
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string putKhranilishcha = VybratPapku("Выберите папку хранилища с заданиями");
                if (string.IsNullOrEmpty(putKhranilishcha))
                    return;

                ProveritKhranilishche(putKhranilishcha);
                int kolichestvoVariantov = PoluchitKolichestvoVariantov();
                if (kolichestvoVariantov <= 0)
                    return;

                string putVykhoda = VybratPapku("Выберите папку для сохранения вариантов");
                if (string.IsNullOrEmpty(putVykhoda))
                    return;

                GenerirovatVarianty(putKhranilishcha, putVykhoda, kolichestvoVariantov);
                MessageBox.Show($"Успешно создано {kolichestvoVariantov} вариантов!\n\nПапка: {putVykhoda}",
                    "Генерация завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\nИсправьте ошибку и попробуйте снова.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string VybratPapku(string opisanie)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = opisanie,
                Filter = "Папки|*.folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Выберите эту папку"
            };

            if (dialog.ShowDialog() == true)
            {
                string vybrannyPut = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(vybrannyPut) && Directory.Exists(vybrannyPut))
                {
                    return vybrannyPut;
                }
            }

            return "";
        }

        private void ProveritKhranilishche(string putKhranilishcha)
        {
            var zadaniyaSOdnimDop = new HashSet<int> { 3, 9, 10, 17, 18, 22, 24, 26 };
            var zadaniyaSDvumyaDop = new HashSet<int> { 27 };

            for (int nomerZadaniya = 1; nomerZadaniya <= 27; nomerZadaniya++)
            {
                if (nomerZadaniya >= 19 && nomerZadaniya <= 21)
                {
                    if (nomerZadaniya == 19)
                    {
                        ProveritKhranilishche1921(putKhranilishcha);
                    }
                    continue;
                }

                string putPapkiZadaniya = Path.Combine(putKhranilishcha, nomerZadaniya.ToString());
                if (!Directory.Exists(putPapkiZadaniya))
                {
                    throw new Exception($"Нет папки задания {nomerZadaniya}");
                }

                string[] papkiVariantov = Directory.GetDirectories(putPapkiZadaniya);
                if (papkiVariantov.Length == 0)
                {
                    throw new Exception($"Нет вариантов в задании {nomerZadaniya}");
                }

                foreach (string papkaVarianta in papkiVariantov)
                {
                    ProveritVariant(papkaVarianta, nomerZadaniya, zadaniyaSOdnimDop, zadaniyaSDvumyaDop);
                }
            }
        }

        private void ProveritKhranilishche1921(string putKhranilishcha)
        {
            string putPapki1921 = Path.Combine(putKhranilishcha, "19-21");
            if (!Directory.Exists(putPapki1921))
            {
                throw new Exception($"Нет папки заданий 19-21");
            }

            string[] papkiVariantov = Directory.GetDirectories(putPapki1921);
            if (papkiVariantov.Length == 0)
            {
                throw new Exception($"Нет вариантов в заданиях 19-21");
            }

            foreach (string papkaVarianta in papkiVariantov)
            {
                ProveritVariant1921(papkaVarianta);
            }
        }

        private void ProveritVariant1921(string papkaVarianta)
        {
            string imyaVarianta = Path.GetFileName(papkaVarianta);
            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                string papkaZadaniya = Path.Combine(papkaVarianta, nomerZadaniya.ToString());
                if (!Directory.Exists(papkaZadaniya))
                {
                    throw new Exception($"В варианте {imyaVarianta} заданий 19-21 нет папки {nomerZadaniya}");
                }

                string[] faily = Directory.GetFiles(papkaZadaniya);
                string failZadaniya = faily.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("task", StringComparison.OrdinalIgnoreCase) &&
                    (f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)));

                string failOtvet = faily.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("answer", StringComparison.OrdinalIgnoreCase) &&
                    f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

                if (failZadaniya == null)
                {
                    throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} заданий 19-21 нет задания (task.png или task.jpg)");
                }

                if (failOtvet == null)
                {
                    throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} заданий 19-21 нет ответа (answer.txt)");
                }

                string soderzhimoeOtvet = File.ReadAllText(failOtvet).Trim();
                if (string.IsNullOrEmpty(soderzhimoeOtvet))
                {
                    throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} заданий 19-21 пустой файл ответа");
                }

                if (faily.Length != 2)
                {
                    throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} заданий 19-21 должно быть ровно 2 файла (task и answer), найдено {faily.Length}");
                }
            }
        }

        private void ProveritVariant(string papkaVarianta, int nomerZadaniya,
            HashSet<int> zadaniyaSOdnimDop, HashSet<int> zadaniyaSDvumyaDop)
        {
            string imyaVarianta = Path.GetFileName(papkaVarianta);
            string[] faily = Directory.GetFiles(papkaVarianta);
            string failZadaniya = faily.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f).Equals("task", StringComparison.OrdinalIgnoreCase) &&
            (f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
             f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)));

            string failOtvet = faily.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f).Equals("answer", StringComparison.OrdinalIgnoreCase) &&
            f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

            if (failZadaniya == null)
            {
                throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} нет задания (task.png или task.jpg)");
            }

            if (failOtvet == null)
            {
                throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} нет ответа (answer.txt)");
            }

            string soderzhimoeOtvet = File.ReadAllText(failOtvet).Trim();
            if (string.IsNullOrEmpty(soderzhimoeOtvet))
            {
                throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} пустой файл ответа");
            }

            int trebuemoeKolichestvoDopFailov = 0;
            if (zadaniyaSOdnimDop.Contains(nomerZadaniya))
                trebuemoeKolichestvoDopFailov = 1;
            else if (zadaniyaSDvumyaDop.Contains(nomerZadaniya))
                trebuemoeKolichestvoDopFailov = 2;

            if (trebuemoeKolichestvoDopFailov == 1)
            {
                bool imeetDopFail = faily.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("A", StringComparison.OrdinalIgnoreCase));

                if (!imeetDopFail)
                {
                    throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} нет доп. материала A");
                }
            }

            if (trebuemoeKolichestvoDopFailov == 2)
            {
                bool imeetFailA = faily.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("A", StringComparison.OrdinalIgnoreCase));

                bool imeetFailB = faily.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("B", StringComparison.OrdinalIgnoreCase));

                if (!imeetFailA)
                {
                    throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} нет доп. материала A");
                }

                if (!imeetFailB)
                {
                    throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} нет доп. материала B");
                }
            }

            int ozhidaemoeKolichestvoFailov = 2 + trebuemoeKolichestvoDopFailov;
            if (faily.Length != ozhidaemoeKolichestvoFailov)
            {
                throw new Exception($"В задании {nomerZadaniya}, варианте {imyaVarianta} должно быть {ozhidaemoeKolichestvoFailov} файлов, найдено {faily.Length}");
            }
        }

        private int PoluchitKolichestvoVariantov()
        {
            var dialogVvoda = new InputDialog("Количество вариантов");
            if (dialogVvoda.ShowDialog() == true)
            {
                if (int.TryParse(dialogVvoda.Otvet, out int kolichestvo) && kolichestvo > 0)
                {
                    return kolichestvo;
                }
                else
                {
                    MessageBox.Show("Введите корректное положительное число",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return 0;
        }

        private void GenerirovatVarianty(string putKhranilishcha, string putVykhoda, int kolichestvoVariantov)
        {
            Random random = new Random();
            string papkaVariantov = Path.Combine(putVykhoda, $"Варианты_{DateTime.Now:yyyy-MM-dd_HH-mm}");
            Directory.CreateDirectory(papkaVariantov);

            for (int nomerVarianta = 1; nomerVarianta <= kolichestvoVariantov; nomerVarianta++)
            {
                GenerirovatOdinVariant(putKhranilishcha, papkaVariantov, nomerVarianta, random);
            }
        }

        private void GenerirovatOdinVariant(string putKhranilishcha, string papkaVariantov, int nomerVarianta, Random random)
        {
            string papkaVarianta = Path.Combine(papkaVariantov, $"Вариант_{nomerVarianta:000}");
            string papkaZadaniy = Path.Combine(papkaVarianta, "Задания");
            string papkaOtvetov = Path.Combine(papkaVarianta, "Ответы");

            Directory.CreateDirectory(papkaVarianta);
            Directory.CreateDirectory(papkaZadaniy);
            Directory.CreateDirectory(papkaOtvetov);

            List<string> vseOtvety = new List<string>();

            for (int nomerZadaniya = 1; nomerZadaniya <= 27; nomerZadaniya++)
            {
                if (nomerZadaniya >= 19 && nomerZadaniya <= 21)
                {
                    if (nomerZadaniya == 19)
                    {
                        ObratitZadaniya1921(putKhranilishcha, papkaZadaniy, vseOtvety, random);
                    }
                    continue;
                }

                ObratitObychnoeZadanie(putKhranilishcha, nomerZadaniya, papkaZadaniy, vseOtvety, random);
            }

            if (vseOtvety.Count > 0)
            {
                string failOtvetov = Path.Combine(papkaOtvetov, "answers.txt");
                File.WriteAllLines(failOtvetov, vseOtvety);
            }
        }

        private void ObratitZadaniya1921(string putKhranilishcha, string papkaZadaniy, List<string> vseOtvety, Random random)
        {
            string putKhranilishcha1921 = Path.Combine(putKhranilishcha, "19-21");
            string[] dostupnyeVarianty = Directory.GetDirectories(putKhranilishcha1921);
            if (dostupnyeVarianty.Length == 0)
                return;

            string vybrannyVariant = dostupnyeVarianty[random.Next(dostupnyeVarianty.Length)];
            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                string papkaVariantaZadaniya = Path.Combine(vybrannyVariant, nomerZadaniya.ToString());
                string[] faily = Directory.GetFiles(papkaVariantaZadaniya);
                string vykhodnayaPapkaZadaniya = Path.Combine(papkaZadaniy, nomerZadaniya.ToString());
                Directory.CreateDirectory(vykhodnayaPapkaZadaniya);

                foreach (string fail in faily)
                {
                    string imyaFaila = Path.GetFileNameWithoutExtension(fail);
                    string rasshirenie = Path.GetExtension(fail);

                    if (imyaFaila.Equals("task", StringComparison.OrdinalIgnoreCase))
                    {
                        string novoeImyaFaila = $"{nomerZadaniya}.png";
                        string putKopii = Path.Combine(vykhodnayaPapkaZadaniya, novoeImyaFaila);
                        File.Copy(fail, putKopii, true);
                    }
                    else if (imyaFaila.Equals("answer", StringComparison.OrdinalIgnoreCase))
                    {
                        string tekstOtvet = File.ReadAllText(fail).Trim();
                        vseOtvety.Add($"{nomerZadaniya} - {tekstOtvet}");
                    }
                }
            }
        }

        private void ObratitObychnoeZadanie(string putKhranilishcha, int nomerZadaniya, string papkaZadaniy, List<string> vseOtvety, Random random)
        {
            string putKhranilishchaZadaniya = Path.Combine(putKhranilishcha, nomerZadaniya.ToString());
            string[] dostupnyeVarianty = Directory.GetDirectories(putKhranilishchaZadaniya);
            if (dostupnyeVarianty.Length == 0)
                return;

            string vybrannyVariant = dostupnyeVarianty[random.Next(dostupnyeVarianty.Length)];
            string[] faily = Directory.GetFiles(vybrannyVariant);
            string vykhodnayaPapkaZadaniya = Path.Combine(papkaZadaniy, nomerZadaniya.ToString());
            Directory.CreateDirectory(vykhodnayaPapkaZadaniya);

            foreach (string fail in faily)
            {
                string imyaFaila = Path.GetFileNameWithoutExtension(fail);
                string rasshirenie = Path.GetExtension(fail);

                if (imyaFaila.Equals("task", StringComparison.OrdinalIgnoreCase))
                {
                    string novoeImyaFaila = $"{nomerZadaniya}.png";
                    string putKopii = Path.Combine(vykhodnayaPapkaZadaniya, novoeImyaFaila);
                    File.Copy(fail, putKopii, true);
                }
                else if (imyaFaila.Equals("answer", StringComparison.OrdinalIgnoreCase))
                {
                    string tekstOtvet = File.ReadAllText(fail).Trim();
                    vseOtvety.Add($"{nomerZadaniya} - {tekstOtvet}");
                }
                else if (imyaFaila.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    string novoeImyaFaila = $"{nomerZadaniya}A{rasshirenie}";
                    string putKopii = Path.Combine(vykhodnayaPapkaZadaniya, novoeImyaFaila);
                    File.Copy(fail, putKopii, true);
                }
                else if (imyaFaila.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    string novoeImyaFaila = $"{nomerZadaniya}B{rasshirenie}";
                    string putKopii = Path.Combine(vykhodnayaPapkaZadaniya, novoeImyaFaila);
                    File.Copy(fail, putKopii, true);
                }
            }
        }

        private void BtnViewVariant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Выберите папку с вариантом для просмотра",
                    Filter = "Папки|*.folder",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "Выберите эту папку"
                };

                if (dialog.ShowDialog() == true)
                {
                    string vybrannyPut = Path.GetDirectoryName(dialog.FileName);
                    if (!string.IsNullOrEmpty(vybrannyPut) && Directory.Exists(vybrannyPut))
                    {
                        string papkaZadaniy = Path.Combine(vybrannyPut, "Задания");
                        if (Directory.Exists(papkaZadaniy))
                        {
                            var oknoProsmotra = new ViewTaskWindow(vybrannyPut);
                            oknoProsmotra.Owner = this;
                            oknoProsmotra.ShowDialog();
                        }
                        else
                        {
                            var vlozhennyePapki = Directory.GetDirectories(vybrannyPut);
                            foreach (string papka in vlozhennyePapki)
                            {
                                string vlozhennayaPapkaZadaniy = Path.Combine(papka, "Задания");
                                if (Directory.Exists(vlozhennayaPapkaZadaniy))
                                {
                                    var oknoProsmotra = new ViewTaskWindow(papka);
                                    oknoProsmotra.Owner = this;
                                    oknoProsmotra.ShowDialog();
                                    return;
                                }
                            }

                            MessageBox.Show("В выбранной папке не найдены варианты с заданиями",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}