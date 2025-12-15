using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace EgeGenerator
{
    public static class StorageService
    {
        // Метод для выбора папки
        public static string VybratPapku(Window owner, string opisanie)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = opisanie,
                Filter = "Папки|*.folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Выберите эту папку"
            };

            if (dialog.ShowDialog(owner) == true)
            {
                string vybrannyPut = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(vybrannyPut) && Directory.Exists(vybrannyPut))
                {
                    return vybrannyPut;
                }
            }

            return "";
        }

        // Метод для подготовки хранилища
        public static string PodgotovitKhranilishcheDlyaDobavleniya(Window owner)
        {
            string putKhranilishcha = VybratPapku(owner, "Выберите папку хранилища с заданиями");
            if (string.IsNullOrEmpty(putKhranilishcha))
                return "";

            if (Directory.Exists(putKhranilishcha))
            {
                bool imeetLyubyePapkiZadaniy = false;

                // Проверяем обычные задания
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

                // Проверяем задания 19-21
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

        // Метод для создания всех папок заданий
        public static void SozdatVsePapkiZadaniy(string putKhranilishcha)
        {
            List<string> sozdannyePapki = new List<string>();

            // Папки для заданий 1-18
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

            // Папка для заданий 19-21
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

            // Папки для заданий 22-27
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

        // Метод для получения путей к папкам заданий
        public static string PoluchitPutPapkiZadaniya(string putKhranilishcha, int nomerZadaniya)
        {
            if (nomerZadaniya >= 19 && nomerZadaniya <= 21)
            {
                return Path.Combine(putKhranilishcha, "19-21");
            }
            else
            {
                return Path.Combine(putKhranilishcha, nomerZadaniya.ToString());
            }
        }

        // Метод для получения всех существующих номеров вариантов
        public static HashSet<int> PoluchitSushchestvuyushchieNomeraVariantov(string putPapkiZadaniya)
        {
            var sushchestvuyushchieNomera = new HashSet<int>();

            if (!Directory.Exists(putPapkiZadaniya))
            {
                return sushchestvuyushchieNomera;
            }

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

        // Метод для получения всех доступных вариантов для задания
        public static string[] PoluchitDostupnyeVarianty(string putKhranilishcha, int nomerZadaniya)
        {
            string putPapkiZadaniya = PoluchitPutPapkiZadaniya(putKhranilishcha, nomerZadaniya);

            if (!Directory.Exists(putPapkiZadaniya))
            {
                return new string[0];
            }

            return Directory.GetDirectories(putPapkiZadaniya);
        }

        // Метод для проверки существования задания
        public static bool ZadanieSuschestvuet(string putKhranilishcha, int nomerZadaniya, int nomerVarianta)
        {
            string putPapkiZadaniya = PoluchitPutPapkiZadaniya(putKhranilishcha, nomerZadaniya);
            string putPapkiVarianta = Path.Combine(putPapkiZadaniya, nomerVarianta.ToString());

            return Directory.Exists(putPapkiVarianta);
        }

        // Метод для получения информации о варианте
        public static Dictionary<string, string> PoluchitInformatsiyuOVariante(string putPapkiVarianta)
        {
            var informatsiya = new Dictionary<string, string>();

            if (!Directory.Exists(putPapkiVarianta))
            {
                return informatsiya;
            }

            // Получаем все файлы в папке варианта
            string[] faily = Directory.GetFiles(putPapkiVarianta, "*.*", SearchOption.AllDirectories);

            foreach (string fail in faily)
            {
                string otnositelnyPut = fail.Substring(putPapkiVarianta.Length + 1);
                string razmer = new FileInfo(fail).Length.ToString();
                string dataSozdaniya = File.GetCreationTime(fail).ToString("yyyy-MM-dd HH:mm:ss");

                informatsiya[otnositelnyPut] = $"{razmer} байт, создан: {dataSozdaniya}";
            }

            return informatsiya;
        }

        // Метод для удаления варианта
        public static bool UdalitVariant(string putKhranilishcha, int nomerZadaniya, int nomerVarianta)
        {
            try
            {
                string putPapkiZadaniya = PoluchitPutPapkiZadaniya(putKhranilishcha, nomerZadaniya);
                string putPapkiVarianta = Path.Combine(putPapkiZadaniya, nomerVarianta.ToString());

                if (Directory.Exists(putPapkiVarianta))
                {
                    Directory.Delete(putPapkiVarianta, true);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Метод для копирования варианта
        public static bool SkopirovatVariant(string putKhranilishcha, int nomerZadaniya,
                                            int iskhodnyNomerVarianta, int novyNomerVarianta)
        {
            try
            {
                string putPapkiZadaniya = PoluchitPutPapkiZadaniya(putKhranilishcha, nomerZadaniya);
                string iskhodnayaPapka = Path.Combine(putPapkiZadaniya, iskhodnyNomerVarianta.ToString());
                string novayaPapka = Path.Combine(putPapkiZadaniya, novyNomerVarianta.ToString());

                if (!Directory.Exists(iskhodnayaPapka))
                {
                    return false;
                }

                // Копируем всю папку
                KopirovatPapku(iskhodnayaPapka, novayaPapka);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Вспомогательный метод для копирования папки
        private static void KopirovatPapku(string iskhodnayaPapka, string tselevayaPapka)
        {
            Directory.CreateDirectory(tselevayaPapka);

            foreach (string fail in Directory.GetFiles(iskhodnayaPapka))
            {
                string imyaFaila = Path.GetFileName(fail);
                string tselevoyFail = Path.Combine(tselevayaPapka, imyaFaila);
                File.Copy(fail, tselevoyFail, true);
            }

            foreach (string podpapka in Directory.GetDirectories(iskhodnayaPapka))
            {
                string imyaPodpapki = Path.GetFileName(podpapka);
                string tselevayaPodpapka = Path.Combine(tselevayaPapka, imyaPodpapki);
                KopirovatPapku(podpapka, tselevayaPodpapka);
            }
        }
    }
}