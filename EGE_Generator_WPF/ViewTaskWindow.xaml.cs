using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EgeGenerator
{
    public partial class ViewTaskWindow : Window
    {
        private string _variantFolderPath;
        private List<TaskInfo> _tasks = new List<TaskInfo>();
        private int _currentTaskIndex = 0;
        private string _answersFilePath;

        private class TaskInfo
        {
            public int TaskNumber { get; set; }
            public string TaskFolder { get; set; }
            public string TaskImagePath { get; set; }
            public string AnswerText { get; set; }
            public List<string> ExtraFiles { get; set; } = new List<string>();
            public string TaskDisplayName { get; set; }
        }

        public ViewTaskWindow(string variantFolderPath)
        {
            InitializeComponent();
            _variantFolderPath = variantFolderPath;
            LoadTasks();
        }

        private void LoadTasks()
        {
            try
            {
                string variantName = Path.GetFileName(_variantFolderPath);
                txtVariantInfo.Text = $"Вариант: {variantName}";

                string tasksFolder = Path.Combine(_variantFolderPath, "Задания");

                if (!Directory.Exists(tasksFolder))
                {
                    MessageBox.Show("Папка с заданиями не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Загружаем все ответы из папки "Ответы" если есть
                LoadAnswersFromAnswersFolder();

                // Загружаем все задания
                for (int taskNum = 1; taskNum <= 27; taskNum++)
                {
                    string taskFolder = Path.Combine(tasksFolder, taskNum.ToString());

                    // Для заданий 19-21 особая обработка
                    if (taskNum >= 19 && taskNum <= 21)
                    {
                        if (taskNum == 19)
                        {
                            LoadTasks1921(tasksFolder);
                        }
                        continue;
                    }

                    if (Directory.Exists(taskFolder))
                    {
                        var taskInfo = LoadTaskInfo(taskFolder, taskNum);
                        if (taskInfo != null)
                        {
                            _tasks.Add(taskInfo);
                        }
                    }
                }

                // Сортируем по номеру задания
                _tasks = _tasks.OrderBy(t => t.TaskNumber).ToList();

                if (_tasks.Count == 0)
                {
                    MessageBox.Show("Задания не найдены", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                    return;
                }

                ShowTask(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заданий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadAnswersFromAnswersFolder()
        {
            try
            {
                string answersFolder = Path.Combine(_variantFolderPath, "Ответы");
                if (Directory.Exists(answersFolder))
                {
                    string[] answerFiles = Directory.GetFiles(answersFolder, "*.txt");
                    if (answerFiles.Length > 0)
                    {
                        // Берем первый файл с ответами
                        _answersFilePath = answerFiles[0];
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при загрузке ответов
            }
        }

        private void LoadTasks1921(string tasksFolder)
        {
            // Проверяем, есть ли папки 19, 20, 21
            for (int taskNum = 19; taskNum <= 21; taskNum++)
            {
                string taskFolder = Path.Combine(tasksFolder, taskNum.ToString());
                if (Directory.Exists(taskFolder))
                {
                    var taskInfo = LoadTaskInfo(taskFolder, taskNum);
                    if (taskInfo != null)
                    {
                        taskInfo.TaskDisplayName = $"Задания 19-21 ({taskNum})";
                        _tasks.Add(taskInfo);
                    }
                }
            }
        }

        private TaskInfo LoadTaskInfo(string taskFolder, int taskNum)
        {
            try
            {
                string[] files = Directory.GetFiles(taskFolder);
                if (files.Length == 0)
                    return null;

                var taskInfo = new TaskInfo
                {
                    TaskNumber = taskNum,
                    TaskFolder = taskFolder,
                    TaskDisplayName = $"Задание {taskNum}"
                };

                // Ищем файлы заданий
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file).ToLower();

                    // Ищем изображение задания
                    if (fileName.StartsWith($"{taskNum}.png") ||
                        fileName.StartsWith($"{taskNum}.jpg") ||
                        fileName.StartsWith($"{taskNum}.jpeg") ||
                        fileName == "task.png" ||
                        fileName == "task.jpg" ||
                        fileName == "task.jpeg" ||
                        Path.GetFileNameWithoutExtension(fileName).ToLower() == "task")
                    {
                        taskInfo.TaskImagePath = file;
                    }
                    // Ищем локальный файл с ответом
                    else if (fileName.Contains("answer") && fileName.EndsWith(".txt"))
                    {
                        taskInfo.AnswerText = File.ReadAllText(file).Trim();
                    }
                    // Дополнительные материалы
                    else if (!fileName.EndsWith(".png") &&
                             !fileName.EndsWith(".jpg") &&
                             !fileName.EndsWith(".jpeg"))
                    {
                        taskInfo.ExtraFiles.Add(file);
                    }
                }

                // Если ответ не найден в папке задания, ищем в общем файле ответов
                if (string.IsNullOrEmpty(taskInfo.AnswerText) && !string.IsNullOrEmpty(_answersFilePath))
                {
                    taskInfo.AnswerText = FindAnswerInAnswersFile(taskNum);
                }

                return taskInfo;
            }
            catch
            {
                return null;
            }
        }

        private string FindAnswerInAnswersFile(int taskNum)
        {
            try
            {
                if (string.IsNullOrEmpty(_answersFilePath) || !File.Exists(_answersFilePath))
                    return null;

                // Читаем весь файл ответов
                string allAnswersText = File.ReadAllText(_answersFilePath);

                // Разбиваем на строки
                string[] lines = allAnswersText.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    // Ищем строку, которая начинается с номера задания
                    // Поддерживаем разные форматы:
                    // "1. ответ"
                    // "1) ответ" 
                    // "1 - ответ"
                    // "1 ответ"
                    // "1: ответ"

                    string trimmedLine = line.Trim();

                    // Проверяем разные форматы начала строки
                    bool isTaskLine = false;
                    string answerPart = "";

                    // Формат "1. ответ"
                    if (trimmedLine.StartsWith($"{taskNum}. "))
                    {
                        isTaskLine = true;
                        answerPart = trimmedLine.Substring($"{taskNum}. ".Length);
                    }
                    // Формат "1) ответ"
                    else if (trimmedLine.StartsWith($"{taskNum}) "))
                    {
                        isTaskLine = true;
                        answerPart = trimmedLine.Substring($"{taskNum}) ".Length);
                    }
                    // Формат "1 - ответ"
                    else if (trimmedLine.StartsWith($"{taskNum} - "))
                    {
                        isTaskLine = true;
                        answerPart = trimmedLine.Substring($"{taskNum} - ".Length);
                    }
                    // Формат "1 – ответ" (длинное тире)
                    else if (trimmedLine.StartsWith($"{taskNum} – "))
                    {
                        isTaskLine = true;
                        answerPart = trimmedLine.Substring($"{taskNum} – ".Length);
                    }
                    // Формат "1 ответ" (просто пробел)
                    else if (trimmedLine.StartsWith($"{taskNum} ") &&
                             trimmedLine.Length > taskNum.ToString().Length + 1)
                    {
                        // Проверяем следующий символ после номера и пробела
                        string afterNumber = trimmedLine.Substring(taskNum.ToString().Length).TrimStart();
                        if (!string.IsNullOrEmpty(afterNumber))
                        {
                            isTaskLine = true;
                            answerPart = afterNumber;
                        }
                    }
                    // Формат "1: ответ"
                    else if (trimmedLine.StartsWith($"{taskNum}: "))
                    {
                        isTaskLine = true;
                        answerPart = trimmedLine.Substring($"{taskNum}: ".Length);
                    }

                    if (isTaskLine && !string.IsNullOrEmpty(answerPart))
                    {
                        // Если ответ состоит из нескольких строк (например, несколько вариантов через запятую)
                        // Очищаем от лишних пробелов и объединяем в одну строку
                        string[] answerLines = answerPart.Split(
                            new[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.RemoveEmptyEntries);

                        if (answerLines.Length > 1)
                        {
                            // Если ответ разбит на несколько строк, соединяем их через пробел
                            return string.Join(" ", answerLines.Select(l => l.Trim()));
                        }
                        else
                        {
                            // Одна строка - просто возвращаем её
                            return answerPart.Trim();
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

        private void ShowTask(int index)
        {
            if (index < 0 || index >= _tasks.Count)
                return;

            _currentTaskIndex = index;
            var task = _tasks[index];

            // Заголовок
            txtTaskTitle.Text = task.TaskDisplayName;

            // Изображение задания
            if (!string.IsNullOrEmpty(task.TaskImagePath) && File.Exists(task.TaskImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(task.TaskImagePath);
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

            // Ответ
            string answerText = task.AnswerText;
            if (string.IsNullOrEmpty(answerText))
            {
                answerText = "Ответ не найден";
                txtAnswer.Foreground = Brushes.Red;
            }
            else
            {
                txtAnswer.Foreground = Brushes.Black;

                // Если ответ содержит несколько строк, объединяем их через пробел
                if (answerText.Contains("\r") || answerText.Contains("\n"))
                {
                    string[] answerLines = answerText.Split(
                        new[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries);

                    answerText = string.Join(" ", answerLines.Select(l => l.Trim()));
                }
            }
            txtAnswer.Text = answerText;

            // Дополнительные материалы
            extraFilesStackPanel.Children.Clear();
            if (task.ExtraFiles.Count > 0)
            {
                extraMaterialsPanel.Visibility = Visibility.Visible;
                foreach (string file in task.ExtraFiles)
                {
                    string fileName = Path.GetFileName(file);

                    var button = new Button
                    {
                        Content = fileName,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 0, 0, 5),
                        Padding = new Thickness(10, 5, 10, 5),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Background = Brushes.LightGray
                    };

                    button.Click += (s, e) => OpenExtraFile(file);

                    extraFilesStackPanel.Children.Add(button);
                }
            }
            else
            {
                extraMaterialsPanel.Visibility = Visibility.Collapsed;
            }

            // Обновляем состояние кнопок навигации
            btnPrev.IsEnabled = _currentTaskIndex > 0;
            btnNext.IsEnabled = _currentTaskIndex < _tasks.Count - 1;
        }

        private void OpenExtraFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
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
            if (_currentTaskIndex > 0)
            {
                ShowTask(_currentTaskIndex - 1);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTaskIndex < _tasks.Count - 1)
            {
                ShowTask(_currentTaskIndex + 1);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}