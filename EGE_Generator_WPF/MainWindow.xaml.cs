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
                // 1. Выбираем или создаём папку хранилища
                string storagePath = GetOrCreateStorageForAdding();
                if (string.IsNullOrEmpty(storagePath))
                    return;

                // 2. Открываем окно ввода номера задания
                var taskNumberDialog = new TaskNumberDialog();
                if (taskNumberDialog.ShowDialog() == true)
                {
                    int taskNumber = taskNumberDialog.TaskNumber;

                    // 3. Открываем окно добавления варианта
                    var addVariantDialog = new AddVariantDialog(taskNumber, storagePath);
                    if (addVariantDialog.ShowDialog() == true)
                    {
                        MessageBox.Show($"Успешно добавлен вариант {addVariantDialog.VariantNumber} для задания {taskNumber}!",
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

        private string GetOrCreateStorageForAdding()
        {
            // Сначала пробуем выбрать существующую папку хранилища
            string storagePath = SelectFolder("Выберите папку хранилища с заданиями");

            if (string.IsNullOrEmpty(storagePath))
                return "";

            // Если папка существует, проверяем структуру
            if (Directory.Exists(storagePath))
            {
                // Проверяем есть ли хоть какие-то папки заданий
                bool hasAnyTaskFolders = false;

                // Проверяем папки 1-18, 19-21, 22-27
                for (int i = 1; i <= 27; i++)
                {
                    if (i >= 19 && i <= 21) continue; // Пропускаем 19-21, будем проверять отдельно

                    string taskFolder = Path.Combine(storagePath, i.ToString());
                    if (Directory.Exists(taskFolder))
                    {
                        hasAnyTaskFolders = true;
                        break;
                    }
                }

                // Проверяем папку 19-21
                string task1921Folder = Path.Combine(storagePath, "19-21");
                if (Directory.Exists(task1921Folder))
                {
                    hasAnyTaskFolders = true;
                }

                // Если нет ни одной папки задания, спрашиваем создать ли
                if (!hasAnyTaskFolders)
                {
                    var result = MessageBox.Show("В выбранной папке нет заданий. Создать папки для заданий?\n\n(26 папок: 1-18, 19-21, 22-27)",
                        "Создание структуры", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        CreateAllTaskFolders(storagePath);
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
                // Если папки не существует, спрашиваем создать ли
                var result = MessageBox.Show($"Папка не существует. Создать папку хранилища и папки для заданий?\n\n{storagePath}",
                    "Создание хранилища", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(storagePath);
                        CreateAllTaskFolders(storagePath);

                        MessageBox.Show($"Создано хранилище и папки для заданий.\n\n{storagePath}",
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

            return storagePath;
        }

        private void CreateAllTaskFolders(string storagePath)
        {
            List<string> createdFolders = new List<string>();

            // Создаем папки 1-18
            for (int taskNum = 1; taskNum <= 18; taskNum++)
            {
                string taskFolderPath = Path.Combine(storagePath, taskNum.ToString());

                if (!Directory.Exists(taskFolderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(taskFolderPath);
                        createdFolders.Add(taskNum.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось создать папку для задания {taskNum}: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            // Создаем папку 19-21
            string task1921FolderPath = Path.Combine(storagePath, "19-21");
            if (!Directory.Exists(task1921FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(task1921FolderPath);
                    createdFolders.Add("19-21");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось создать папку для заданий 19-21: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Создаем папки 22-27
            for (int taskNum = 22; taskNum <= 27; taskNum++)
            {
                string taskFolderPath = Path.Combine(storagePath, taskNum.ToString());

                if (!Directory.Exists(taskFolderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(taskFolderPath);
                        createdFolders.Add(taskNum.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось создать папку для задания {taskNum}: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            if (createdFolders.Count > 0)
            {
                Console.WriteLine($"Созданы папки: {string.Join(", ", createdFolders)}");
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Выбираем папку хранилища
                string storagePath = SelectFolder("Выберите папку хранилища с заданиями");
                if (string.IsNullOrEmpty(storagePath))
                    return;

                // 2. Проверяем структуру данных
                ValidateStorage(storagePath);

                // 3. Запрашиваем количество вариантов
                int variantCount = GetVariantCount();
                if (variantCount <= 0)
                    return;

                // 4. Выбираем папку для сохранения
                string outputPath = SelectFolder("Выберите папку для сохранения вариантов");
                if (string.IsNullOrEmpty(outputPath))
                    return;

                // 5. Генерируем варианты
                GenerateVariants(storagePath, outputPath, variantCount);

                // 6. Уведомляем об успехе
                MessageBox.Show($"Успешно создано {variantCount} вариантов!\n\nПапка: {outputPath}",
                    "Генерация завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\nИсправьте ошибку и попробуйте снова.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string SelectFolder(string description)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = description;
            dialog.Filter = "Папки|*.folder";
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Выберите эту папку";

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = Path.GetDirectoryName(dialog.FileName);

                if (!string.IsNullOrEmpty(selectedPath) && Directory.Exists(selectedPath))
                {
                    return selectedPath;
                }
            }

            return "";
        }

        private void ValidateStorage(string storagePath)
        {
            // Задания с доп. материалами
            var tasksWithOneExtra = new HashSet<int> { 3, 9, 10, 17, 18, 22, 24, 26 }; // 26 - 1 доп.
            var tasksWithTwoExtra = new HashSet<int> { 27 }; // 27 - 2 доп.

            // Проверяем все папки заданий
            for (int taskNum = 1; taskNum <= 27; taskNum++)
            {
                // Для заданий 19-21 - отдельная проверка
                if (taskNum >= 19 && taskNum <= 21)
                {
                    // Проверяем только когда дошли до 19
                    if (taskNum == 19)
                    {
                        ValidateStorage1921(storagePath);
                    }
                    continue;
                }

                string taskFolderPath = Path.Combine(storagePath, taskNum.ToString());

                // 1. Проверяем наличие папки задания
                if (!Directory.Exists(taskFolderPath))
                {
                    throw new Exception($"Нет папки задания {taskNum}");
                }

                // 2. Проверяем наличие вариантов в папке задания
                string[] variantFolders = Directory.GetDirectories(taskFolderPath);
                if (variantFolders.Length == 0)
                {
                    throw new Exception($"Нет вариантов в задании {taskNum}");
                }

                // 3. Проверяем каждый вариант
                foreach (string variantFolder in variantFolders)
                {
                    ValidateVariant(variantFolder, taskNum, tasksWithOneExtra, tasksWithTwoExtra);
                }
            }
        }

        private void ValidateStorage1921(string storagePath)
        {
            string task1921FolderPath = Path.Combine(storagePath, "19-21");

            // 1. Проверяем наличие папки 19-21
            if (!Directory.Exists(task1921FolderPath))
            {
                throw new Exception($"Нет папки заданий 19-21");
            }

            // 2. Проверяем наличие вариантов в папке 19-21
            string[] variantFolders = Directory.GetDirectories(task1921FolderPath);
            if (variantFolders.Length == 0)
            {
                throw new Exception($"Нет вариантов в заданиях 19-21");
            }

            // 3. Проверяем каждый вариант
            foreach (string variantFolder in variantFolders)
            {
                ValidateVariant1921(variantFolder);
            }
        }

        private void ValidateVariant1921(string variantFolder)
        {
            string variantName = Path.GetFileName(variantFolder);

            // Проверяем наличие папок 19, 20, 21 внутри варианта
            for (int taskNum = 19; taskNum <= 21; taskNum++)
            {
                string taskFolder = Path.Combine(variantFolder, taskNum.ToString());

                if (!Directory.Exists(taskFolder))
                {
                    throw new Exception($"В варианте {variantName} заданий 19-21 нет папки {taskNum}");
                }

                // Проверяем файлы в папке задания
                string[] files = Directory.GetFiles(taskFolder);

                // Ищем обязательные файлы
                string taskFile = files.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("task", StringComparison.OrdinalIgnoreCase) &&
                    (f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                     f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)));

                string answerFile = files.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("answer", StringComparison.OrdinalIgnoreCase) &&
                    f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

                // Проверяем наличие задания
                if (taskFile == null)
                {
                    throw new Exception($"В задании {taskNum}, варианте {variantName} заданий 19-21 нет задания (task.png или task.jpg)");
                }

                // Проверяем наличие ответа
                if (answerFile == null)
                {
                    throw new Exception($"В задании {taskNum}, варианте {variantName} заданий 19-21 нет ответа (answer.txt)");
                }

                // Проверяем что ответ не пустой
                string answerContent = File.ReadAllText(answerFile).Trim();
                if (string.IsNullOrEmpty(answerContent))
                {
                    throw new Exception($"В задании {taskNum}, варианте {variantName} заданий 19-21 пустой файл ответа");
                }

                // Для заданий 19-21 доп. материалов не должно быть
                if (files.Length != 2) // Только task и answer
                {
                    throw new Exception($"В задании {taskNum}, варианте {variantName} заданий 19-21 должно быть ровно 2 файла (task и answer), найдено {files.Length}");
                }
            }
        }

        private void ValidateVariant(string variantFolder, int taskNum,
            HashSet<int> tasksWithOneExtra, HashSet<int> tasksWithTwoExtra)
        {
            string variantName = Path.GetFileName(variantFolder);
            string[] files = Directory.GetFiles(variantFolder);

            // Ищем обязательные файлы
            string taskFile = files.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Equals("task", StringComparison.OrdinalIgnoreCase) &&
                (f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                 f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)));

            string answerFile = files.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Equals("answer", StringComparison.OrdinalIgnoreCase) &&
                f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

            // Проверяем наличие задания
            if (taskFile == null)
            {
                throw new Exception($"В задании {taskNum}, варианте {variantName} нет задания (task.png или task.jpg)");
            }

            // Проверяем наличие ответа
            if (answerFile == null)
            {
                throw new Exception($"В задании {taskNum}, варианте {variantName} нет ответа (answer.txt)");
            }

            // Проверяем что ответ не пустой
            string answerContent = File.ReadAllText(answerFile).Trim();
            if (string.IsNullOrEmpty(answerContent))
            {
                throw new Exception($"В задании {taskNum}, варианте {variantName} пустой файл ответа");
            }

            // Определяем сколько должно быть дополнительных файлов
            int requiredExtraFiles = 0;
            if (tasksWithOneExtra.Contains(taskNum))
                requiredExtraFiles = 1;
            else if (tasksWithTwoExtra.Contains(taskNum))
                requiredExtraFiles = 2;

            // Проверяем доп. файлы для заданий с одним доп. материалом
            if (requiredExtraFiles == 1)
            {
                bool hasExtraFile = files.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("A", StringComparison.OrdinalIgnoreCase));

                if (!hasExtraFile)
                {
                    throw new Exception($"В задании {taskNum}, варианте {variantName} нет доп. материала A");
                }
            }

            // Проверяем доп. файлы для заданий с двумя доп. материалами
            if (requiredExtraFiles == 2)
            {
                bool hasFileA = files.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("A", StringComparison.OrdinalIgnoreCase));

                bool hasFileB = files.Any(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("B", StringComparison.OrdinalIgnoreCase));

                if (!hasFileA)
                {
                    throw new Exception($"В задании {taskNum}, варианте {variantName} нет доп. материала A");
                }

                if (!hasFileB)
                {
                    throw new Exception($"В задании {taskNum}, варианте {variantName} нет доп. материала B");
                }
            }

            // Проверяем общее количество файлов
            int expectedFileCount = 2 + requiredExtraFiles; // task + answer + доп. файлы
            if (files.Length != expectedFileCount)
            {
                throw new Exception($"В задании {taskNum}, варианте {variantName} должно быть {expectedFileCount} файлов, найдено {files.Length}");
            }
        }

        private int GetVariantCount()
        {
            var inputDialog = new InputDialog("Количество вариантов");
            if (inputDialog.ShowDialog() == true)
            {
                if (int.TryParse(inputDialog.Answer, out int count) && count > 0)
                {
                    return count;
                }
                else
                {
                    MessageBox.Show("Введите корректное положительное число",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return 0;
        }

        private void GenerateVariants(string storagePath, string outputPath, int variantCount)
        {
            Random random = new Random();

            // Создаем основную папку для вариантов
            string variantsFolder = Path.Combine(outputPath, $"Варианты_{DateTime.Now:yyyy-MM-dd_HH-mm}");
            Directory.CreateDirectory(variantsFolder);

            for (int variantNum = 1; variantNum <= variantCount; variantNum++)
            {
                GenerateSingleVariant(storagePath, variantsFolder, variantNum, random);
            }
        }

        private void GenerateSingleVariant(string storagePath, string variantsFolder, int variantNum, Random random)
        {
            // Создаем папку для варианта
            string variantFolder = Path.Combine(variantsFolder, $"Вариант_{variantNum:000}");
            string tasksFolder = Path.Combine(variantFolder, "Задания");
            string answersFolder = Path.Combine(variantFolder, "Ответы");

            Directory.CreateDirectory(variantFolder);
            Directory.CreateDirectory(tasksFolder);
            Directory.CreateDirectory(answersFolder);

            List<string> allAnswers = new List<string>();

            // Для каждого задания (1-27)
            for (int taskNum = 1; taskNum <= 27; taskNum++)
            {
                // Обработка заданий 19-21 особая
                if (taskNum >= 19 && taskNum <= 21)
                {
                    if (taskNum == 19) // Обрабатываем 19-21 один раз
                    {
                        ProcessTask1921(storagePath, tasksFolder, allAnswers, random);
                    }
                    continue;
                }

                ProcessRegularTask(storagePath, taskNum, tasksFolder, allAnswers, random);
            }

            // Сохраняем все ответы в один файл в папке "Ответы"
            if (allAnswers.Count > 0)
            {
                string answersFile = Path.Combine(answersFolder, "answers.txt");
                File.WriteAllLines(answersFile, allAnswers);
            }
        }

        private void ProcessTask1921(string storagePath, string tasksFolder, List<string> allAnswers, Random random)
        {
            string task1921StoragePath = Path.Combine(storagePath, "19-21");

            // Получаем все варианты для заданий 19-21
            string[] availableVariants = Directory.GetDirectories(task1921StoragePath);
            if (availableVariants.Length == 0)
                return;

            // Выбираем случайный вариант
            string selectedVariant = availableVariants[random.Next(availableVariants.Length)];

            // Для каждого задания 19, 20, 21
            for (int taskNum = 19; taskNum <= 21; taskNum++)
            {
                string taskVariantFolder = Path.Combine(selectedVariant, taskNum.ToString());
                string[] files = Directory.GetFiles(taskVariantFolder);

                // Создаем папку для задания в конечном варианте
                string taskOutputFolder = Path.Combine(tasksFolder, taskNum.ToString());
                Directory.CreateDirectory(taskOutputFolder);

                // Копируем файлы с переименованием
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string extension = Path.GetExtension(file);

                    if (fileName.Equals("task", StringComparison.OrdinalIgnoreCase))
                    {
                        string newFileName = $"{taskNum}.png";
                        string destinationFile = Path.Combine(taskOutputFolder, newFileName);
                        File.Copy(file, destinationFile, true);
                    }
                    else if (fileName.Equals("answer", StringComparison.OrdinalIgnoreCase))
                    {
                        string answerText = File.ReadAllText(file).Trim();
                        allAnswers.Add($"{taskNum} - {answerText}");
                    }
                }
            }
        }

        private void ProcessRegularTask(string storagePath, int taskNum, string tasksFolder, List<string> allAnswers, Random random)
        {
            string taskStoragePath = Path.Combine(storagePath, taskNum.ToString());

            // Получаем все варианты для этого задания
            string[] availableVariants = Directory.GetDirectories(taskStoragePath);
            if (availableVariants.Length == 0)
                return;

            // Выбираем случайный вариант
            string selectedVariant = availableVariants[random.Next(availableVariants.Length)];

            // Получаем все файлы выбранного варианта
            string[] files = Directory.GetFiles(selectedVariant);

            // Создаем папку для задания в конечном варианте
            string taskOutputFolder = Path.Combine(tasksFolder, taskNum.ToString());
            Directory.CreateDirectory(taskOutputFolder);

            // Копируем файлы с переименованием
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);

                if (fileName.Equals("task", StringComparison.OrdinalIgnoreCase))
                {
                    string newFileName = $"{taskNum}.png";
                    string destinationFile = Path.Combine(taskOutputFolder, newFileName);
                    File.Copy(file, destinationFile, true);
                }
                else if (fileName.Equals("answer", StringComparison.OrdinalIgnoreCase))
                {
                    string answerText = File.ReadAllText(file).Trim();
                    allAnswers.Add($"{taskNum} - {answerText}");
                }
                else if (fileName.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    string newFileName = $"{taskNum}A{extension}";
                    string destinationFile = Path.Combine(taskOutputFolder, newFileName);
                    File.Copy(file, destinationFile, true);
                }
                else if (fileName.Equals("B", StringComparison.OrdinalIgnoreCase))
                {
                    string newFileName = $"{taskNum}B{extension}";
                    string destinationFile = Path.Combine(taskOutputFolder, newFileName);
                    File.Copy(file, destinationFile, true);
                }
            }
        }
    }
}