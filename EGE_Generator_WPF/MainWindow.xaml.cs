using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace EgeGenerator
{
    public partial class MainWindow : Window
    {
        private const int TOTAL_TASKS = 27;
        private static readonly int[] tasksWithOneExtra = { 3, 9, 10, 17, 18, 22, 24 };
        private static readonly int[] tasksWithTwoExtra = { 26, 27 };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Получаем или создаём папку Data
                string dataPath = GetOrCreateDataFolder();

                // 2. Проверяем структуру данных
                CheckDataStructure(dataPath);

                // 3. Запрашиваем количество вариантов
                int variantCount = GetVariantCount();
                if (variantCount <= 0)
                    return;

                // 4. Выбираем папку для сохранения
                string outputPath = GetOutputPath();
                if (string.IsNullOrEmpty(outputPath))
                    return;

                // 5. Генерируем варианты
                GenerateVariants(dataPath, outputPath, variantCount);

                // 6. Уведомляем об успехе
                System.Windows.MessageBox.Show($"Успешно создано {variantCount} вариантов!",
                    "Генерация завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}\n\nИсправьте ошибку и попробуйте снова.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetOrCreateDataFolder()
        {
            string dataPath = "Data";

            // Проверяем папку Data рядом с программой
            if (!Directory.Exists(dataPath))
            {
                dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            }

            // Если папки Data нет совсем
            if (!Directory.Exists(dataPath))
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Папка Data не найдена. Выберите или создайте папку для данных";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dataPath = dialog.SelectedPath;
                }
                else
                {
                    throw new Exception("Папка Data не выбрана. Операция отменена.");
                }
            }

            // Создаём все 27 папок заданий внутри Data
            CreateTaskFolders(dataPath);

            return dataPath;
        }

        private void CreateTaskFolders(string dataPath)
        {
            List<int> createdFolders = new List<int>();

            for (int taskNum = 1; taskNum <= TOTAL_TASKS; taskNum++)
            {
                string taskFolderPath = Path.Combine(dataPath, taskNum.ToString());

                if (!Directory.Exists(taskFolderPath))
                {
                    Directory.CreateDirectory(taskFolderPath);
                    createdFolders.Add(taskNum);
                }
            }

            // Если были созданы папки, сообщаем пользователю
            if (createdFolders.Count > 0)
            {
                string foldersList = string.Join(", ", createdFolders);
                throw new Exception($"Созданы папки для заданий: {foldersList}. Заполните их вариантами заданий.");
            }
        }

        private void CheckDataStructure(string dataPath)
        {
            txtStatus.Text = "Проверка структуры данных...";

            // Проверяем все 27 папок заданий
            for (int taskNum = 1; taskNum <= TOTAL_TASKS; taskNum++)
            {
                string taskFolderPath = Path.Combine(dataPath, taskNum.ToString());

                // Проверяем что папка существует (должна существовать после CreateTaskFolders)
                if (!Directory.Exists(taskFolderPath))
                {
                    throw new Exception($"Папка задания {taskNum} не найдена.");
                }

                // Проверяем что папка не пуста
                string[] variantFolders = Directory.GetDirectories(taskFolderPath);

                if (variantFolders.Length == 0)
                {
                    throw new Exception($"Папка задания {taskNum} пуста. Добавьте варианты заданий.");
                }

                // Проверяем каждый вариант в папке
                foreach (string variantFolder in variantFolders)
                {
                    CheckVariantFolder(variantFolder, taskNum);
                }
            }

            txtStatus.Text = "Структура данных проверена успешно!";
        }

        private void CheckVariantFolder(string variantFolder, int taskNum)
        {
            // Получаем номер варианта из имени папки
            string variantName = Path.GetFileName(variantFolder);

            // Получаем все файлы в папке варианта
            string[] allFiles = Directory.GetFiles(variantFolder);

            // Ищем обязательные файлы
            string taskFile = allFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Equals("task", StringComparison.OrdinalIgnoreCase) &&
                (f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                 f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)));

            string answerFile = allFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Equals("answer", StringComparison.OrdinalIgnoreCase) &&
                f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

            // Проверяем наличие обязательных файлов
            if (taskFile == null)
            {
                throw new Exception($"В варианте {variantName} задания {taskNum} отсутствует файл задания (task.png или task.jpg)");
            }

            if (answerFile == null)
            {
                throw new Exception($"В варианте {variantName} задания {taskNum} отсутствует файл ответа (answer.txt)");
            }

            // Определяем сколько должно быть дополнительных файлов
            int requiredExtraFiles = 0;
            if (tasksWithOneExtra.Contains(taskNum))
                requiredExtraFiles = 1;
            else if (tasksWithTwoExtra.Contains(taskNum))
                requiredExtraFiles = 2;

            // Определяем сколько у нас дополнительных файлов (все файлы кроме task и answer)
            int actualExtraFiles = allFiles.Length - 2;

            // Проверяем количество файлов
            int expectedTotalFiles = 2 + requiredExtraFiles;

            if (allFiles.Length != expectedTotalFiles)
            {
                throw new Exception($"В варианте {variantName} задания {taskNum} должно быть {expectedTotalFiles} файлов, но найдено {allFiles.Length}");
            }

            // Для заданий с двумя доп. файлами проверяем имена
            if (requiredExtraFiles == 2)
            {
                bool hasFileA = allFiles.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("A", StringComparison.OrdinalIgnoreCase));

                bool hasFileB = allFiles.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("B", StringComparison.OrdinalIgnoreCase));

                if (!hasFileA || !hasFileB)
                {
                    throw new Exception($"В варианте {variantName} задания {taskNum} должны быть файлы A и B");
                }
            }

            // Проверяем что нет нескольких task.* файлов
            int taskFilesCount = allFiles.Count(f =>
                Path.GetFileNameWithoutExtension(f).Equals("task", StringComparison.OrdinalIgnoreCase));

            if (taskFilesCount > 1)
            {
                throw new Exception($"В варианте {variantName} задания {taskNum} несколько файлов с именем task");
            }
        }

        private int GetVariantCount()
        {
            var inputDialog = new InputDialog("Введите количество вариантов",
                "Сколько вариантов хотите сгенерировать?");

            if (inputDialog.ShowDialog() == true)
            {
                if (int.TryParse(inputDialog.Answer, out int count) && count > 0)
                {
                    return count;
                }
                else
                {
                    System.Windows.MessageBox.Show("Введите корректное положительное число",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return 0;
        }

        private string GetOutputPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку для сохранения вариантов";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }

            return null;
        }

        private void GenerateVariants(string dataPath, string outputPath, int variantCount)
        {
            txtStatus.Text = "Генерация вариантов...";

            Random random = new Random();

            // Создаем папку для вариантов с timestamp
            string variantsFolder = Path.Combine(outputPath, $"Варианты_{DateTime.Now:yyyy-MM-dd_HH-mm}");
            Directory.CreateDirectory(variantsFolder);

            for (int variantNum = 1; variantNum <= variantCount; variantNum++)
            {
                GenerateSingleVariant(dataPath, variantsFolder, variantNum, random);
            }

            txtStatus.Text = $"Готово! Создано {variantCount} вариантов";
        }

        private void GenerateSingleVariant(string dataPath, string outputPath, int variantNum, Random random)
        {
            // Создаем структуру папок для варианта
            string variantFolder = Path.Combine(outputPath, $"Вариант_{variantNum:000}");
            string tasksFolder = Path.Combine(variantFolder, "Задания");
            string answersFolder = Path.Combine(variantFolder, "Ответы");

            Directory.CreateDirectory(variantFolder);
            Directory.CreateDirectory(tasksFolder);
            Directory.CreateDirectory(answersFolder);

            List<string> allAnswers = new List<string>();

            // Для каждого задания (1-27)
            for (int taskNum = 1; taskNum <= TOTAL_TASKS; taskNum++)
            {
                string taskDataFolder = Path.Combine(dataPath, taskNum.ToString());

                // Получаем все папки вариантов для этого задания
                string[] variantFolders = Directory.GetDirectories(taskDataFolder);

                if (variantFolders.Length == 0)
                    continue;

                // Выбираем случайный вариант
                string selectedVariantFolder = variantFolders[random.Next(variantFolders.Length)];

                // Получаем файлы из выбранного варианта
                string[] files = Directory.GetFiles(selectedVariantFolder);

                // Создаем папку для этого задания в конечном варианте
                string taskOutputFolder = Path.Combine(tasksFolder, taskNum.ToString());
                Directory.CreateDirectory(taskOutputFolder);

                // Копируем и переименовываем файлы
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string extension = Path.GetExtension(file);

                    string newFileName;

                    if (fileName.Equals("task", StringComparison.OrdinalIgnoreCase))
                    {
                        newFileName = $"{taskNum}.png";
                    }
                    else if (fileName.Equals("answer", StringComparison.OrdinalIgnoreCase))
                    {
                        newFileName = "answer.txt";

                        // Читаем ответ и добавляем в список
                        string answerText = File.ReadAllText(file).Trim();
                        allAnswers.Add($"{taskNum} - {answerText}");
                    }
                    else if (fileName.Equals("A", StringComparison.OrdinalIgnoreCase))
                    {
                        newFileName = $"{taskNum}A{extension}";
                    }
                    else if (fileName.Equals("B", StringComparison.OrdinalIgnoreCase))
                    {
                        newFileName = $"{taskNum}B{extension}";
                    }
                    else
                    {
                        // Для заданий с одним доп. файлом
                        newFileName = $"{taskNum}A{extension}";
                    }

                    string destFile = Path.Combine(taskOutputFolder, newFileName);
                    File.Copy(file, destFile, true);
                }
            }

            // Сохраняем все ответы в один файл
            if (allAnswers.Count > 0)
            {
                string answersFile = Path.Combine(answersFolder, "answers.txt");
                File.WriteAllLines(answersFile, allAnswers);
            }
        }
    }
}