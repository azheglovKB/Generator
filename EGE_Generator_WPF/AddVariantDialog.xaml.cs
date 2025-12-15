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
        private int _taskNumber;
        private string _storagePath;

        // Для обычных заданий
        private string _taskFilePath;
        private string _answerFilePath;
        private string _extraAFilePath;
        private string _extraBFilePath;

        // Для заданий 19-21
        private Dictionary<int, string> _taskFiles1921 = new Dictionary<int, string>(); // taskNum -> filePath
        private Dictionary<int, string> _answerTexts1921 = new Dictionary<int, string>(); // taskNum -> answerText (ручной ввод)
        private Dictionary<int, string> _answerFiles1921 = new Dictionary<int, string>(); // taskNum -> filePath (загрузка файла)

        private static readonly HashSet<int> _tasksWithOneExtra = new HashSet<int> { 3, 9, 10, 17, 18, 22, 24, 26 };
        private static readonly HashSet<int> _tasksWithTwoExtra = new HashSet<int> { 27 };
        private static readonly string[] _allowedExtraExtensions = { ".txt", ".ods", ".xlsx", ".xls", ".pdf", ".doc", ".docx" };

        private const string PAPKA_1921 = "19-21";

        public int VariantNumber { get; private set; }

        public AddVariantDialog(int taskNumber, string storagePath)
        {
            InitializeComponent();

            _taskNumber = taskNumber;
            _storagePath = storagePath;

            _taskFilePath = "";
            _answerFilePath = "";
            _extraAFilePath = "";
            _extraBFilePath = "";
            VariantNumber = 0;

            // Инициализация для 19-21
            if (_taskNumber >= 19 && _taskNumber <= 21)
            {
                for (int i = 19; i <= 21; i++)
                {
                    _taskFiles1921[i] = "";
                    _answerTexts1921[i] = "";
                    _answerFiles1921[i] = "";
                }
            }

            InitializeUI();
            UpdateVariantNumber();
        }

        private void InitializeUI()
        {
            // Для заданий 19-21 - особый случай
            if (_taskNumber >= 19 && _taskNumber <= 21)
            {
                InitializeUIFor1921();
            }
            else
            {
                InitializeUIForRegularTask();
            }
        }

        private void InitializeUIFor1921()
        {
            txtTitle.Text = $"Добавление заданий 19-21";

            // Скрываем стандартные поля
            borderTask.Visibility = Visibility.Collapsed;
            borderAnswer.Visibility = Visibility.Collapsed;
            borderExtraA.Visibility = Visibility.Collapsed;
            borderExtraB.Visibility = Visibility.Collapsed;

            // Показываем поля для 19-21
            border1921.Visibility = Visibility.Visible;

            // Обновляем статусы для каждого задания
            Update1921Status();
        }

        private void InitializeUIForRegularTask()
        {
            txtTitle.Text = $"Добавление варианта задания {_taskNumber}";

            // Показываем нужные поля в зависимости от типа задания
            if (_tasksWithOneExtra.Contains(_taskNumber))
            {
                borderExtraA.Visibility = Visibility.Visible;
                borderExtraB.Visibility = Visibility.Collapsed;
            }
            else if (_tasksWithTwoExtra.Contains(_taskNumber))
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

        private void UpdateVariantNumber()
        {
            string taskFolderPath;

            // Для заданий 19-21 используем папку 19-21
            if (_taskNumber >= 19 && _taskNumber <= 21)
            {
                taskFolderPath = Path.Combine(_storagePath, PAPKA_1921);
            }
            else
            {
                taskFolderPath = Path.Combine(_storagePath, _taskNumber.ToString());
            }

            if (!Directory.Exists(taskFolderPath))
            {
                VariantNumber = 1;
                txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber} (новая папка)";
                return;
            }

            // Находим минимальный пропущенный номер
            string[] existingVariants = Directory.GetDirectories(taskFolderPath);
            HashSet<int> existingNumbers = new HashSet<int>();

            foreach (string variant in existingVariants)
            {
                string folderName = Path.GetFileName(variant);
                if (int.TryParse(folderName, out int num))
                {
                    existingNumbers.Add(num);
                }
            }

            // Ищем минимальный пропущенный номер
            for (int i = 1; i <= 1000; i++)
            {
                if (!existingNumbers.Contains(i))
                {
                    VariantNumber = i;
                    txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber}";
                    return;
                }
            }

            VariantNumber = existingNumbers.Max() + 1;
            txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber}";
        }

        private void Update1921Status()
        {
            // Обновляем статусы для заданий 19, 20, 21
            UpdateSingle1921Status(19);
            UpdateSingle1921Status(20);
            UpdateSingle1921Status(21);
        }

        private void UpdateSingle1921Status(int taskNum)
        {
            TextBlock taskStatus = null;
            TextBlock taskCheck = null;
            TextBlock answerStatus = null;
            TextBlock answerCheck = null;
            TextBox answerTextBox = null;

            // Находим контролы для текущего задания
            switch (taskNum)
            {
                case 19:
                    taskStatus = txtTask19Status;
                    taskCheck = txtTask19Check;
                    answerStatus = txtAnswer19Status;
                    answerCheck = txtAnswer19Check;
                    answerTextBox = txtAnswer19;
                    break;
                case 20:
                    taskStatus = txtTask20Status;
                    taskCheck = txtTask20Check;
                    answerStatus = txtAnswer20Status;
                    answerCheck = txtAnswer20Check;
                    answerTextBox = txtAnswer20;
                    break;
                case 21:
                    taskStatus = txtTask21Status;
                    taskCheck = txtTask21Check;
                    answerStatus = txtAnswer21Status;
                    answerCheck = txtAnswer21Check;
                    answerTextBox = txtAnswer21;
                    break;
            }

            if (taskStatus == null) return;

            // Обновляем статус задания
            if (!string.IsNullOrEmpty(_taskFiles1921[taskNum]) && File.Exists(_taskFiles1921[taskNum]))
            {
                string fileName = Path.GetFileName(_taskFiles1921[taskNum]);
                taskStatus.Text = fileName;
                taskCheck.Text = "✓";
                taskCheck.Foreground = System.Windows.Media.Brushes.Green;
                taskCheck.Visibility = Visibility.Visible;
            }
            else
            {
                taskStatus.Text = $"Задание {taskNum} не загружено";
                taskCheck.Text = "✗";
                taskCheck.Foreground = System.Windows.Media.Brushes.Red;
                taskCheck.Visibility = Visibility.Visible;
            }

            // Обновляем статус ответа (есть ли ответ вручную или из файла)
            bool hasAnswerFromFile = !string.IsNullOrEmpty(_answerFiles1921[taskNum]) && File.Exists(_answerFiles1921[taskNum]);
            bool hasAnswerFromText = !string.IsNullOrEmpty(_answerTexts1921[taskNum]) && _answerTexts1921[taskNum].Trim().Length > 0;

            if (hasAnswerFromFile)
            {
                answerStatus.Text = "Ответ из файла";
                answerCheck.Text = "✓";
                answerCheck.Foreground = System.Windows.Media.Brushes.Green;
                answerCheck.Visibility = Visibility.Visible;

                // Загружаем текст из файла в TextBox
                if (answerTextBox != null)
                {
                    string fileContent = File.ReadAllText(_answerFiles1921[taskNum]).Trim();
                    answerTextBox.Text = fileContent;
                    _answerTexts1921[taskNum] = fileContent;
                }
            }
            else if (hasAnswerFromText)
            {
                answerStatus.Text = "Ответ введен вручную";
                answerCheck.Text = "✓";
                answerCheck.Foreground = System.Windows.Media.Brushes.Green;
                answerCheck.Visibility = Visibility.Visible;
            }
            else
            {
                answerStatus.Text = $"Ответ {taskNum} не загружен";
                answerCheck.Text = "✗";
                answerCheck.Foreground = System.Windows.Media.Brushes.Red;
                answerCheck.Visibility = Visibility.Visible;
            }

            // Обновляем TextBox если нужно
            if (answerTextBox != null && hasAnswerFromText)
            {
                answerTextBox.Text = _answerTexts1921[taskNum];
            }
        }

        // Обработчики для заданий 19-21
        private void BtnBrowseTask19_Click(object sender, RoutedEventArgs e)
        {
            LoadTaskFor1921(19);
        }

        private void BtnBrowseTask20_Click(object sender, RoutedEventArgs e)
        {
            LoadTaskFor1921(20);
        }

        private void BtnBrowseTask21_Click(object sender, RoutedEventArgs e)
        {
            LoadTaskFor1921(21);
        }

        private void BtnBrowseAnswer19_Click(object sender, RoutedEventArgs e)
        {
            LoadAnswerFor1921(19);
        }

        private void BtnBrowseAnswer20_Click(object sender, RoutedEventArgs e)
        {
            LoadAnswerFor1921(20);
        }

        private void BtnBrowseAnswer21_Click(object sender, RoutedEventArgs e)
        {
            LoadAnswerFor1921(21);
        }

        // Обработчики изменения текста в TextBox
        private void TxtAnswer19_TextChanged(object sender, TextChangedEventArgs e)
        {
            _answerTexts1921[19] = txtAnswer19.Text;
            UpdateSingle1921Status(19);
        }

        private void TxtAnswer20_TextChanged(object sender, TextChangedEventArgs e)
        {
            _answerTexts1921[20] = txtAnswer20.Text;
            UpdateSingle1921Status(20);
        }

        private void TxtAnswer21_TextChanged(object sender, TextChangedEventArgs e)
        {
            _answerTexts1921[21] = txtAnswer21.Text;
            UpdateSingle1921Status(21);
        }

        private void LoadTaskFor1921(int taskNum)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.png;*.jpg;*.jpeg";
            dialog.Title = $"Выберите файл задания {taskNum}";

            if (dialog.ShowDialog() == true)
            {
                string taskExtension = Path.GetExtension(dialog.FileName).ToLower();
                if (taskExtension != ".png" && taskExtension != ".jpg" && taskExtension != ".jpeg")
                {
                    MessageBox.Show($"Файл задания {taskNum} должен быть в формате png или jpg",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _taskFiles1921[taskNum] = dialog.FileName;
                UpdateSingle1921Status(taskNum);
            }
        }

        private void LoadAnswerFor1921(int taskNum)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Текстовые файлы|*.txt";
            dialog.Title = $"Выберите файл с ответом для задания {taskNum}";

            if (dialog.ShowDialog() == true)
            {
                // Проверяем что файл не пустой
                string content = File.ReadAllText(dialog.FileName).Trim();
                if (string.IsNullOrEmpty(content))
                {
                    MessageBox.Show($"Файл ответа для задания {taskNum} пустой.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _answerFiles1921[taskNum] = dialog.FileName;
                _answerTexts1921[taskNum] = content; // Сохраняем текст для отображения
                UpdateSingle1921Status(taskNum);
            }
        }

        // Старые методы для обычных заданий
        private void BtnBrowseTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.png;*.jpg;*.jpeg";
            dialog.Title = "Выберите файл задания";

            if (dialog.ShowDialog() == true)
            {
                _taskFilePath = dialog.FileName;
                UpdateFileStatus(txtTaskFile, txtTaskCheck, _taskFilePath, true);
            }
        }

        private void BtnBrowseAnswer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Текстовые файлы|*.txt";
            dialog.Title = "Выберите файл с ответом";

            if (dialog.ShowDialog() == true)
            {
                _answerFilePath = dialog.FileName;
                // Проверяем что файл не пустой
                string content = File.ReadAllText(_answerFilePath).Trim();
                if (string.IsNullOrEmpty(content))
                {
                    MessageBox.Show("Файл ответа пустой. Введите ответ вручную или выберите другой файл.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _answerFilePath = "";
                }
                else
                {
                    // Копируем содержимое в текстовое поле
                    txtAnswer.Text = content;
                }
            }
        }

        private void BtnBrowseExtraA_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Дополнительные файлы|*.txt;*.ods;*.xlsx;*.xls;*.pdf;*.doc;*.docx";
            dialog.Title = "Выберите дополнительный материал A";

            if (dialog.ShowDialog() == true)
            {
                string extension = Path.GetExtension(dialog.FileName).ToLower();
                if (_allowedExtraExtensions.Contains(extension))
                {
                    _extraAFilePath = dialog.FileName;
                    UpdateFileStatus(txtExtraAFile, txtExtraACheck, _extraAFilePath,
                        _tasksWithOneExtra.Contains(_taskNumber) || _tasksWithTwoExtra.Contains(_taskNumber),
                        _allowedExtraExtensions);
                }
                else
                {
                    MessageBox.Show($"Недопустимый формат файла. Разрешенные форматы: {string.Join(", ", _allowedExtraExtensions)}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnBrowseExtraB_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Дополнительные файлы|*.txt;*.ods;*.xlsx;*.xls;*.pdf;*.doc;*.docx";
            dialog.Title = "Выберите дополнительный материал B";

            if (dialog.ShowDialog() == true)
            {
                string extension = Path.GetExtension(dialog.FileName).ToLower();
                if (_allowedExtraExtensions.Contains(extension))
                {
                    _extraBFilePath = dialog.FileName;
                    UpdateFileStatus(txtExtraBFile, txtExtraBCheck, _extraBFilePath,
                        _tasksWithTwoExtra.Contains(_taskNumber),
                        _allowedExtraExtensions);
                }
                else
                {
                    MessageBox.Show($"Недопустимый формат файла. Разрешенные форматы: {string.Join(", ", _allowedExtraExtensions)}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            // Для заданий 19-21 особая проверка
            if (_taskNumber >= 19 && _taskNumber <= 21)
            {
                return ValidateInputFor1921();
            }
            else
            {
                return ValidateInputForRegularTask();
            }
        }

        private bool ValidateInputFor1921()
        {
            // Проверяем все 3 задания
            for (int taskNum = 19; taskNum <= 21; taskNum++)
            {
                // Проверяем задание
                if (string.IsNullOrEmpty(_taskFiles1921[taskNum]) || !File.Exists(_taskFiles1921[taskNum]))
                {
                    MessageBox.Show($"Загрузите файл задания {taskNum} (png или jpg)",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string taskExtension = Path.GetExtension(_taskFiles1921[taskNum]).ToLower();
                if (taskExtension != ".png" && taskExtension != ".jpg" && taskExtension != ".jpeg")
                {
                    MessageBox.Show($"Файл задания {taskNum} должен быть в формате png или jpg",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Проверяем ответ (может быть из файла или вручную)
                bool hasAnswerFromFile = !string.IsNullOrEmpty(_answerFiles1921[taskNum]) && File.Exists(_answerFiles1921[taskNum]);
                bool hasAnswerFromText = !string.IsNullOrEmpty(_answerTexts1921[taskNum]) && _answerTexts1921[taskNum].Trim().Length > 0;

                if (!hasAnswerFromFile && !hasAnswerFromText)
                {
                    MessageBox.Show($"Введите ответ для задания {taskNum} вручную или загрузите файл с ответом",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Если ответ из файла, проверяем что он не пустой
                if (hasAnswerFromFile)
                {
                    string content = File.ReadAllText(_answerFiles1921[taskNum]).Trim();
                    if (string.IsNullOrEmpty(content))
                    {
                        MessageBox.Show($"Файл ответа для задания {taskNum} пустой. Введите ответ вручную или выберите другой файл.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateInputForRegularTask()
        {
            // Старая проверка
            if (string.IsNullOrEmpty(_taskFilePath))
            {
                MessageBox.Show("Загрузите файл задания (png или jpg)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string taskExtension = Path.GetExtension(_taskFilePath).ToLower();
            if (taskExtension != ".png" && taskExtension != ".jpg" && taskExtension != ".jpeg")
            {
                MessageBox.Show("Файл задания должен быть в формате png или jpg",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Проверяем ответ
            string answerText = txtAnswer.Text.Trim();
            if (string.IsNullOrEmpty(answerText))
            {
                MessageBox.Show("Введите ответ вручную или загрузите файл с ответом",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Проверяем доп. материал A если требуется
            if ((_tasksWithOneExtra.Contains(_taskNumber) || _tasksWithTwoExtra.Contains(_taskNumber))
                && string.IsNullOrEmpty(_extraAFilePath))
            {
                MessageBox.Show("Загрузите дополнительный материал A",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Проверяем доп. материал B если требуется
            if (_tasksWithTwoExtra.Contains(_taskNumber) && string.IsNullOrEmpty(_extraBFilePath))
            {
                MessageBox.Show("Загрузите дополнительный материал B",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // Для заданий 19-21 - особый случай
                if (_taskNumber >= 19 && _taskNumber <= 21)
                {
                    SaveTask1921();
                }
                else
                {
                    SaveRegularTask();
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}\n\nПовторите попытку.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveTask1921()
        {
            // Создаем папку 19-21 если её нет
            string task1921FolderPath = Path.Combine(_storagePath, PAPKA_1921);
            if (!Directory.Exists(task1921FolderPath))
            {
                Directory.CreateDirectory(task1921FolderPath);
            }

            // Создаем папку варианта
            string variantFolderPath = Path.Combine(task1921FolderPath, VariantNumber.ToString());
            Directory.CreateDirectory(variantFolderPath);

            // Создаем все 3 папки заданий внутри варианта и сохраняем файлы
            for (int taskNum = 19; taskNum <= 21; taskNum++)
            {
                string taskFolderPath = Path.Combine(variantFolderPath, taskNum.ToString());
                Directory.CreateDirectory(taskFolderPath);

                // Копируем файл задания с переименованием
                string taskDestPath = Path.Combine(taskFolderPath, "task" + Path.GetExtension(_taskFiles1921[taskNum]));
                File.Copy(_taskFiles1921[taskNum], taskDestPath, true);

                // Сохраняем ответ (из файла или ручного ввода)
                string answerText = _answerTexts1921[taskNum];
                if (string.IsNullOrEmpty(answerText) && !string.IsNullOrEmpty(_answerFiles1921[taskNum]))
                {
                    // Если текст пустой, но есть файл, читаем из файла
                    answerText = File.ReadAllText(_answerFiles1921[taskNum]).Trim();
                }

                string answerDestPath = Path.Combine(taskFolderPath, "answer.txt");
                File.WriteAllText(answerDestPath, answerText);
            }

            MessageBox.Show($"Все 3 задания (19, 20, 21) успешно добавлены в вариант {VariantNumber}!",
                           "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveRegularTask()
        {
            // Создаем папку задания если её нет
            string taskFolderPath = Path.Combine(_storagePath, _taskNumber.ToString());
            if (!Directory.Exists(taskFolderPath))
            {
                Directory.CreateDirectory(taskFolderPath);
            }

            // Создаем папку варианта
            string variantFolderPath = Path.Combine(taskFolderPath, VariantNumber.ToString());
            Directory.CreateDirectory(variantFolderPath);

            // Копируем файл задания с переименованием
            string taskDestPath = Path.Combine(variantFolderPath, "task" + Path.GetExtension(_taskFilePath));
            File.Copy(_taskFilePath, taskDestPath, true);

            // Сохраняем ответ
            string answerDestPath = Path.Combine(variantFolderPath, "answer.txt");
            File.WriteAllText(answerDestPath, txtAnswer.Text.Trim());

            // Копируем доп. материал A если есть
            if (!string.IsNullOrEmpty(_extraAFilePath))
            {
                string extraADestPath = Path.Combine(variantFolderPath, "A" + Path.GetExtension(_extraAFilePath));
                File.Copy(_extraAFilePath, extraADestPath, true);
            }

            // Копируем доп. материал B если есть
            if (!string.IsNullOrEmpty(_extraBFilePath))
            {
                string extraBDestPath = Path.Combine(variantFolderPath, "B" + Path.GetExtension(_extraBFilePath));
                File.Copy(_extraBFilePath, extraBDestPath, true);
            }
        }

        private void UpdateFileStatus(TextBlock statusText, TextBlock checkText, string filePath,
            bool required, string[] allowedExtensions = null)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                statusText.Text = fileName;

                // Проверяем расширение если нужно
                if (allowedExtensions != null)
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (allowedExtensions.Contains(extension))
                    {
                        checkText.Text = "✓";
                        checkText.Foreground = System.Windows.Media.Brushes.Green;
                        checkText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        checkText.Text = "✗";
                        checkText.Foreground = System.Windows.Media.Brushes.Red;
                        checkText.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    checkText.Text = "✓";
                    checkText.Foreground = System.Windows.Media.Brushes.Green;
                    checkText.Visibility = Visibility.Visible;
                }
            }
            else
            {
                statusText.Text = "Не загружено";

                if (required)
                {
                    checkText.Text = "✗";
                    checkText.Foreground = System.Windows.Media.Brushes.Red;
                    checkText.Visibility = Visibility.Visible;
                }
                else
                {
                    checkText.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}