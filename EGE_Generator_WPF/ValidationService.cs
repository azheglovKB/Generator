using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EgeGenerator
{
    public static class ValidationService
    {
        // Определения типов заданий
        private static readonly HashSet<int> _prostyeZadaniya = new HashSet<int>
        {
            1, 2, 4, 5, 6, 7, 8, 11, 12, 13, 14, 15, 16, 23, 25
        };

        private static readonly HashSet<int> _zadaniyaSOdnimDop = new HashSet<int>
        {
            3, 9, 10, 17, 18, 22, 24, 26
        };

        private static readonly HashSet<int> _zadaniyaSDvumyaDop = new HashSet<int>
        {
            27
        };

        public static void ProveritKhranilishche(string putKhranilishcha)
        {
            if (!Directory.Exists(putKhranilishcha))
            {
                throw new Exception("Хранилище не существует");
            }

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
                    ProveritVariant(papkaVarianta, nomerZadaniya);
                }
            }
        }

        public static void ProveritKhranilishche1921(string putKhranilishcha)
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

        private static void ProveritVariant1921(string papkaVarianta)
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

        private static void ProveritVariant(string papkaVarianta, int nomerZadaniya)
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
            if (_zadaniyaSOdnimDop.Contains(nomerZadaniya))
                trebuemoeKolichestvoDopFailov = 1;
            else if (_zadaniyaSDvumyaDop.Contains(nomerZadaniya))
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

        // Методы для проверки ввода в диалогах
        public static bool ProveritProstoeZadanie(string putFailaZadaniya, string tekstOtvet)
        {
            if (string.IsNullOrEmpty(putFailaZadaniya))
            {
                throw new Exception("Загрузите файл задания (png или jpg)");
            }

            string rasshirenieZadaniya = Path.GetExtension(putFailaZadaniya).ToLower();
            if (rasshirenieZadaniya != ".png" && rasshirenieZadaniya != ".jpg" && rasshirenieZadaniya != ".jpeg")
            {
                throw new Exception("Файл задания должен быть в формате png или jpg");
            }

            if (string.IsNullOrEmpty(tekstOtvet))
            {
                throw new Exception("Введите ответ вручную или загрузите файл с ответом");
            }

            return true;
        }

        public static bool ProveritZadanieSOdnimDop(string putFailaZadaniya, string tekstOtvet, string putDopMaterialA)
        {
            ProveritProstoeZadanie(putFailaZadaniya, tekstOtvet);

            if (string.IsNullOrEmpty(putDopMaterialA))
            {
                throw new Exception("Загрузите дополнительный материал A");
            }

            string[] razreshennyeRasshireniya = { ".txt", ".ods", ".xlsx", ".xls", ".pdf", ".doc", ".docx" };
            string rasshirenieDop = Path.GetExtension(putDopMaterialA).ToLower();

            if (!razreshennyeRasshireniya.Contains(rasshirenieDop))
            {
                throw new Exception($"Недопустимый формат доп. материала. Разрешенные форматы: {string.Join(", ", razreshennyeRasshireniya)}");
            }

            return true;
        }

        public static bool ProveritZadanieSDvumyaDop(string putFailaZadaniya, string tekstOtvet,
                                                    string putDopMaterialA, string putDopMaterialB)
        {
            ProveritZadanieSOdnimDop(putFailaZadaniya, tekstOtvet, putDopMaterialA);

            if (string.IsNullOrEmpty(putDopMaterialB))
            {
                throw new Exception("Загрузите дополнительный материал B");
            }

            string[] razreshennyeRasshireniya = { ".txt", ".ods", ".xlsx", ".xls", ".pdf", ".doc", ".docx" };
            string rasshirenieDopB = Path.GetExtension(putDopMaterialB).ToLower();

            if (!razreshennyeRasshireniya.Contains(rasshirenieDopB))
            {
                throw new Exception($"Недопустимый формат доп. материала B. Разрешенные форматы: {string.Join(", ", razreshennyeRasshireniya)}");
            }

            return true;
        }

        public static bool ProveritZadanie1921(Dictionary<int, string> failiZadaniy,
                                              Dictionary<int, string> tekstiOtvetov,
                                              Dictionary<int, string> failiOtvetov)
        {
            for (int nomerZadaniya = 19; nomerZadaniya <= 21; nomerZadaniya++)
            {
                if (string.IsNullOrEmpty(failiZadaniy[nomerZadaniya]) || !File.Exists(failiZadaniy[nomerZadaniya]))
                {
                    throw new Exception($"Загрузите файл задания {nomerZadaniya} (png или jpg)");
                }

                string rasshirenieZadaniya = Path.GetExtension(failiZadaniy[nomerZadaniya]).ToLower();
                if (rasshirenieZadaniya != ".png" && rasshirenieZadaniya != ".jpg" && rasshirenieZadaniya != ".jpeg")
                {
                    throw new Exception($"Файл задания {nomerZadaniya} должен быть в формате png или jpg");
                }

                bool imeetOtvetIzFaila = !string.IsNullOrEmpty(failiOtvetov[nomerZadaniya]) &&
                                         File.Exists(failiOtvetov[nomerZadaniya]);
                bool imeetOtvetIzTeksta = !string.IsNullOrEmpty(tekstiOtvetov[nomerZadaniya]) &&
                                          tekstiOtvetov[nomerZadaniya].Trim().Length > 0;

                if (!imeetOtvetIzFaila && !imeetOtvetIzTeksta)
                {
                    throw new Exception($"Введите ответ для задания {nomerZadaniya} вручную или загрузите файл с ответом");
                }

                if (imeetOtvetIzFaila)
                {
                    string soderzhimoe = File.ReadAllText(failiOtvetov[nomerZadaniya]).Trim();
                    if (string.IsNullOrEmpty(soderzhimoe))
                    {
                        throw new Exception($"Файл ответа для задания {nomerZadaniya} пустой. Введите ответ вручную или выберите другой файл.");
                    }
                }
            }

            return true;
        }

        // Метод для определения типа задания
        public static string OpredelitTipZadaniya(int nomerZadaniya)
        {
            if (nomerZadaniya >= 19 && nomerZadaniya <= 21) return "1921";
            if (_prostyeZadaniya.Contains(nomerZadaniya)) return "prostoe";
            if (_zadaniyaSOdnimDop.Contains(nomerZadaniya)) return "s_odnim_dop";
            if (_zadaniyaSDvumyaDop.Contains(nomerZadaniya)) return "s_dvumya_dop";
            return "neizvestno";
        }
    }
}