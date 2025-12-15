using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgeGenerator
{
    public static class FileService
    {
        // Разрешенные форматы доп. материалов
        private static readonly string[] _razreshennyeRasshireniyaDop =
        {
            ".txt", ".ods", ".xlsx", ".xls", ".pdf", ".doc", ".docx"
        };

        // Методы для загрузки файлов
        public static string ZagruzitFailZadaniya(System.Windows.Window owner)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg",
                Title = "Выберите файл задания"
            };

            if (dialog.ShowDialog(owner) == true)
            {
                return dialog.FileName;
            }

            return "";
        }

        public static string ZagruzitFailOtvet(System.Windows.Window owner)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Текстовые файлы|*.txt",
                Title = "Выберите файл с ответом"
            };

            if (dialog.ShowDialog(owner) == true)
            {
                string soderzhimoe = File.ReadAllText(dialog.FileName).Trim();
                if (string.IsNullOrEmpty(soderzhimoe))
                {
                    System.Windows.MessageBox.Show("Файл ответа пустой. Введите ответ вручную или выберите другой файл.",
                        "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return "";
                }
                return dialog.FileName;
            }

            return "";
        }

        public static string ZagruzitDopMaterial(System.Windows.Window owner, string zagolovok)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Дополнительные файлы|*.txt;*.ods;*.xlsx;*.xls;*.pdf;*.doc;*.docx",
                Title = zagolovok
            };

            if (dialog.ShowDialog(owner) == true)
            {
                string rasshirenie = Path.GetExtension(dialog.FileName).ToLower();
                if (_razreshennyeRasshireniyaDop.Contains(rasshirenie))
                {
                    return dialog.FileName;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Недопустимый формат файла. Разрешенные форматы: {string.Join(", ", _razreshennyeRasshireniyaDop)}",
                        "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

            return "";
        }

        // Методы для сохранения заданий
        public static void SohranitProstoeZadanie(string putKhranilishcha, int nomerZadaniya,
                                                  int nomerVarianta, string putFailaZadaniya,
                                                  string tekstOtvet)
        {
            // Создание папок
            string putPapkiZadaniya = Path.Combine(putKhranilishcha, nomerZadaniya.ToString());
            Directory.CreateDirectory(putPapkiZadaniya);

            string putPapkiVarianta = Path.Combine(putPapkiZadaniya, nomerVarianta.ToString());
            Directory.CreateDirectory(putPapkiVarianta);

            // Копирование задания
            string putKopiiZadaniya = Path.Combine(putPapkiVarianta,
                "task" + Path.GetExtension(putFailaZadaniya));
            File.Copy(putFailaZadaniya, putKopiiZadaniya, true);

            // Сохранение ответа
            string putKopiiOtvet = Path.Combine(putPapkiVarianta, "answer.txt");
            File.WriteAllText(putKopiiOtvet, tekstOtvet.Trim());
        }

        public static void SohranitZadanieSOdnimDop(string putKhranilishcha, int nomerZadaniya,
                                                   int nomerVarianta, string putFailaZadaniya,
                                                   string tekstOtvet, string putDopMaterialA)
        {
            SohranitProstoeZadanie(putKhranilishcha, nomerZadaniya, nomerVarianta,
                                  putFailaZadaniya, tekstOtvet);

            string putPapkiVarianta = Path.Combine(putKhranilishcha, nomerZadaniya.ToString(),
                                                  nomerVarianta.ToString());

            // Копирование доп. материала A
            if (!string.IsNullOrEmpty(putDopMaterialA))
            {
                string putKopiiDopA = Path.Combine(putPapkiVarianta,
                    "A" + Path.GetExtension(putDopMaterialA));
                File.Copy(putDopMaterialA, putKopiiDopA, true);
            }
        }

        public static void SohranitZadanieSDvumyaDop(string putKhranilishcha, int nomerZadaniya,
                                                    int nomerVarianta, string putFailaZadaniya,
                                                    string tekstOtvet, string putDopMaterialA,
                                                    string putDopMaterialB)
        {
            SohranitZadanieSOdnimDop(putKhranilishcha, nomerZadaniya, nomerVarianta,
                                    putFailaZadaniya, tekstOtvet, putDopMaterialA);

            string putPapkiVarianta = Path.Combine(putKhranilishcha, nomerZadaniya.ToString(),
                                                  nomerVarianta.ToString());

            // Копирование доп. материала B
            if (!string.IsNullOrEmpty(putDopMaterialB))
            {
                string putKopiiDopB = Path.Combine(putPapkiVarianta,
                    "B" + Path.GetExtension(putDopMaterialB));
                File.Copy(putDopMaterialB, putKopiiDopB, true);
            }
        }

        public static void SohranitZadaniya1921(string putKhranilishcha, int nomerVarianta,
                                               Dictionary<int, string> failiZadaniy,
                                               Dictionary<int, string> tekstiOtvetov,
                                               Dictionary<int, string> failiOtvetov)
        {
            // Создание папок
            string putPapki1921 = Path.Combine(putKhranilishcha, "19-21");
            Directory.CreateDirectory(putPapki1921);

            string putPapkiVarianta = Path.Combine(putPapki1921, nomerVarianta.ToString());
            Directory.CreateDirectory(putPapkiVarianta);

            // Сохранение всех трех заданий
            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                string putPapkiZadaniya = Path.Combine(putPapkiVarianta, nomerZadaniya.ToString());
                Directory.CreateDirectory(putPapkiZadaniya);

                // Копирование задания
                string putKopiiZadaniya = Path.Combine(putPapkiZadaniya,
                    "task" + Path.GetExtension(failiZadaniy[nomerZadaniya]));
                File.Copy(failiZadaniy[nomerZadaniya], putKopiiZadaniya, true);

                // Получение текста ответа
                string tekstOtvet = tekstiOtvetov[nomerZadaniya];
                if (string.IsNullOrEmpty(tekstOtvet) && !string.IsNullOrEmpty(failiOtvetov[nomerZadaniya]))
                {
                    tekstOtvet = File.ReadAllText(failiOtvetov[nomerZadaniya]).Trim();
                }

                // Сохранение ответа
                string putKopiiOtvet = Path.Combine(putPapkiZadaniya, "answer.txt");
                File.WriteAllText(putKopiiOtvet, tekstOtvet);
            }
        }

        // Методы для генерации вариантов
        public static void GenerirovatVarianty(string putKhranilishcha, string putVykhoda,
                                              int kolichestvoVariantov, Random random)
        {
            string papkaVariantov = Path.Combine(putVykhoda, $"Варианты_{DateTime.Now:yyyy-MM-dd_HH-mm}");
            Directory.CreateDirectory(papkaVariantov);

            for (int nomerVarianta = 1; nomerVarianta <= kolichestvoVariantov; nomerVarianta++)
            {
                GenerirovatOdinVariant(putKhranilishcha, papkaVariantov, nomerVarianta, random);
            }
        }

        private static void GenerirovatOdinVariant(string putKhranilishcha, string papkaVariantov,
                                                  int nomerVarianta, Random random)
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

        private static void ObratitZadaniya1921(string putKhranilishcha, string papkaZadaniy,
                                               List<string> vseOtvety, Random random)
        {
            string putKhranilishcha1921 = Path.Combine(putKhranilishcha, "19-21");
            if (!Directory.Exists(putKhranilishcha1921)) return;

            string[] dostupnyeVarianty = Directory.GetDirectories(putKhranilishcha1921);
            if (dostupnyeVarianty.Length == 0) return;

            string vybrannyVariant = dostupnyeVarianty[random.Next(dostupnyeVarianty.Length)];

            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                string papkaVariantaZadaniya = Path.Combine(vybrannyVariant, nomerZadaniya.ToString());
                if (!Directory.Exists(papkaVariantaZadaniya)) continue;

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

        private static void ObratitObychnoeZadanie(string putKhranilishcha, int nomerZadaniya,
                                                  string papkaZadaniy, List<string> vseOtvety,
                                                  Random random)
        {
            string putKhranilishchaZadaniya = Path.Combine(putKhranilishcha, nomerZadaniya.ToString());
            if (!Directory.Exists(putKhranilishchaZadaniya)) return;

            string[] dostupnyeVarianty = Directory.GetDirectories(putKhranilishchaZadaniya);
            if (dostupnyeVarianty.Length == 0) return;

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

        // Метод для поиска доступного номера варианта
        public static int NaytiDostupnyNomerVarianta(string putPapkiZadaniya)
        {
            if (!Directory.Exists(putPapkiZadaniya))
            {
                return 1;
            }

            var sushchestvuyushchieNomera = new HashSet<int>();
            foreach (string variant in Directory.GetDirectories(putPapkiZadaniya))
            {
                string imyaPapki = Path.GetFileName(variant);
                if (int.TryParse(imyaPapki, out int nomer))
                {
                    sushchestvuyushchieNomera.Add(nomer);
                }
            }

            // Находим доступный номер
            for (int i = 1; i <= 1000; i++)
            {
                if (!sushchestvuyushchieNomera.Contains(i))
                {
                    return i;
                }
            }

            return sushchestvuyushchieNomera.Max() + 1;
        }
    }
}