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
        private readonly int _taskNumber;
        private readonly string _storagePath;
        private string _taskFilePath;
        private string _answerFilePath;
        private string _extraAFilePath;
        private string _extraBFilePath;
        private readonly Dictionary<int, string> _taskFiles1921 = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _answerTexts1921 = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _answerFiles1921 = new Dictionary<int, string>();
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
            InitializeFields();
            InitializeUI();
            UpdateVariantNumber();
        }

        private void InitializeFields()
        {
            _taskFilePath = "";
            _answerFilePath = "";
            _extraAFilePath = "";
            _extraBFilePath = "";
            VariantNumber = 0;

            if (_taskNumber >= 19 && _taskNumber <= 21)
            {
                for (int i = 19; i <= 21; i++)
                {
                    _taskFiles1921[i] = "";
                    _answerTexts1921[i] = "";
                    _answerFiles1921[i] = "";
                }
            }
        }

        private void InitializeUI()
        {
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
            txtTitle.Text = "Добавление заданий 19-21";
            borderTask.Visibility = Visibility.Collapsed;
            borderAnswer.Visibility = Visibility.Collapsed;
            borderExtraA.Visibility = Visibility.Collapsed;
            borderExtraB.Visibility = Visibility.Collapsed;
            border1921.Visibility = Visibility.Visible;
            Update1921Status();
        }

        private void InitializeUIForRegularTask()
        {
            txtTitle.Text = $"Добавление варианта задания {_taskNumber}";
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
            string taskFolderPath = GetTaskFolderPath();
            if (!Directory.Exists(taskFolderPath))
            {
                VariantNumber = 1;
                txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber} (новая папка)";
                return;
            }

            HashSet<int> existingNumbers = GetExistingVariantNumbers(taskFolderPath);
            VariantNumber = FindAvailableVariantNumber(existingNumbers);
            txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber}";
        }

        private string GetTaskFolderPath()
        {
            return _taskNumber >= 19 && _taskNumber <= 21
                ? Path.Combine(_storagePath, PAPKA_1921)
                : Path.Combine(_storagePath, _taskNumber.ToString());
        }

        private static HashSet<int> GetExistingVariantNumbers(string taskFolderPath)
        {
            var existingNumbers = new HashSet<int>();
            foreach (string variant in Directory.GetDirectories(taskFolderPath))
            {
                string folderName = Path.GetFileName(variant);
                if (int.TryParse(folderName, out int num))
                {
                    existingNumbers.Add(num);
                }
            }
            return existingNumbers;
        }

        private static int FindAvailableVariantNumber(HashSet<int> existingNumbers)
        {
            for (int i = 1; i <= 1000; i++)
            {
                if (!existingNumbers.Contains(i))
                {
                    return i;
                }
            }
            return existingNumbers.Max() + 1;
        }

        private void Update1921Status()
        {
            UpdateSingle1921Status(19);
            UpdateSingle1921Status(20);
            UpdateSingle1921Status(21);
        }

        private void UpdateSingle1921Status(int taskNum)
        {
            var controls = GetControlsForTask(taskNum);
            if (controls == null) return;

            UpdateTaskStatus(controls.Item1, controls.Item2, _taskFiles1921[taskNum]);
            UpdateAnswerStatus(controls.Item3, controls.Item4, controls.Item5, taskNum);
        }

        private Tuple<TextBlock, TextBlock, TextBlock, TextBlock, TextBox> GetControlsForTask(int taskNum)
        {
            switch (taskNum)
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

        private void UpdateTaskStatus(TextBlock statusText, TextBlock checkText, string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                statusText.Text = Path.GetFileName(filePath);
                checkText.Text = "✓";
                checkText.Foreground = System.Windows.Media.Brushes.Green;
                checkText.Visibility = Visibility.Visible;
            }
            else
            {
                statusText.Text = "Задание не загружено";
                checkText.Text = "✗";
                checkText.Foreground = System.Windows.Media.Brushes.Red;
                checkText.Visibility = Visibility.Visible;
            }
        }

        private void UpdateAnswerStatus(TextBlock statusText, TextBlock checkText, TextBox answerTextBox, int taskNum)
        {
            bool hasAnswerFromFile = !string.IsNullOrEmpty(_answerFiles1921[taskNum]) && File.Exists(_answerFiles1921[taskNum]);
            bool hasAnswerFromText = !string.IsNullOrEmpty(_answerTexts1921[taskNum]) && _answerTexts1921[taskNum].Trim().Length > 0;

            if (hasAnswerFromFile)
            {
                statusText.Text = "Ответ из файла";
                checkText.Text = "✓";
                checkText.Foreground = System.Windows.Media.Brushes.Green;
                checkText.Visibility = Visibility.Visible;
                if (answerTextBox != null)
                {
                    string fileContent = File.ReadAllText(_answerFiles1921[taskNum]).Trim();
                    answerTextBox.Text = fileContent;
                    _answerTexts1921[taskNum] = fileContent;
                }
            }
            else if (hasAnswerFromText)
            {
                statusText.Text = "Ответ введен вручную";
                checkText.Text = "✓";
                checkText.Foreground = System.Windows.Media.Brushes.Green;
                checkText.Visibility = Visibility.Visible;
            }
            else
            {
                statusText.Text = "Ответ не загружен";
                checkText.Text = "✗";
                checkText.Foreground = System.Windows.Media.Brushes.Red;
                checkText.Visibility = Visibility.Visible;
            }

            if (answerTextBox != null && hasAnswerFromText)
            {
                answerTextBox.Text = _answerTexts1921[taskNum];
            }
        }

        private void BtnBrowseTask19_Click(object sender, RoutedEventArgs e) => LoadTaskFor1921(19);
        private void BtnBrowseTask20_Click(object sender, RoutedEventArgs e) => LoadTaskFor1921(20);
        private void BtnBrowseTask21_Click(object sender, RoutedEventArgs e) => LoadTaskFor1921(21);
        private void BtnBrowseAnswer19_Click(object sender, RoutedEventArgs e) => LoadAnswerFor1921(19);
        private void BtnBrowseAnswer20_Click(object sender, RoutedEventArgs e) => LoadAnswerFor1921(20);
        private void BtnBrowseAnswer21_Click(object sender, RoutedEventArgs e) => LoadAnswerFor1921(21);

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
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg",
                Title = $"Выберите файл задания {taskNum}"
            };

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
            var dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы|*.txt",
                Title = $"Выберите файл с ответом для задания {taskNum}"
            };

            if (dialog.ShowDialog() == true)
            {
                string content = File.ReadAllText(dialog.FileName).Trim();
                if (string.IsNullOrEmpty(content))
                {
                    MessageBox.Show($"Файл ответа для задания {taskNum} пустой.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _answerFiles1921[taskNum] = dialog.FileName;
                _answerTexts1921[taskNum] = content;
                UpdateSingle1921Status(taskNum);
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
                _taskFilePath = dialog.FileName;
                UpdateFileStatus(txtTaskFile, txtTaskCheck, _taskFilePath, true);
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
                _answerFilePath = dialog.FileName;
                string content = File.ReadAllText(_answerFilePath).Trim();
                if (string.IsNullOrEmpty(content))
                {
                    MessageBox.Show("Файл ответа пустой. Введите ответ вручную или выберите другой файл.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _answerFilePath = "";
                }
                else
                {
                    txtAnswer.Text = content;
                }
            }
        }

        private void BtnBrowseExtraA_Click(object sender, RoutedEventArgs e)
        {
            LoadExtraMaterial("Выберите дополнительный материал A", ref _extraAFilePath,
                txtExtraAFile, txtExtraACheck,
                _tasksWithOneExtra.Contains(_taskNumber) || _tasksWithTwoExtra.Contains(_taskNumber));
        }

        private void BtnBrowseExtraB_Click(object sender, RoutedEventArgs e)
        {
            LoadExtraMaterial("Выберите дополнительный материал B", ref _extraBFilePath,
                txtExtraBFile, txtExtraBCheck,
                _tasksWithTwoExtra.Contains(_taskNumber));
        }

        private void LoadExtraMaterial(string title, ref string filePath, TextBlock statusText, TextBlock checkText, bool required)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Дополнительные файлы|*.txt;*.ods;*.xlsx;*.xls;*.pdf;*.doc;*.docx",
                Title = title
            };

            if (dialog.ShowDialog() == true)
            {
                string extension = Path.GetExtension(dialog.FileName).ToLower();
                if (_allowedExtraExtensions.Contains(extension))
                {
                    filePath = dialog.FileName;
                    UpdateFileStatus(statusText, checkText, filePath, required, _allowedExtraExtensions);
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
            return _taskNumber >= 19 && _taskNumber <= 21
                ? ValidateInputFor1921()
                : ValidateInputForRegularTask();
        }

        private bool ValidateInputFor1921()
        {
            for (int taskNum = 19; taskNum <= 21; taskNum++)
            {
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

                bool hasAnswerFromFile = !string.IsNullOrEmpty(_answerFiles1921[taskNum]) && File.Exists(_answerFiles1921[taskNum]);
                bool hasAnswerFromText = !string.IsNullOrEmpty(_answerTexts1921[taskNum]) && _answerTexts1921[taskNum].Trim().Length > 0;

                if (!hasAnswerFromFile && !hasAnswerFromText)
                {
                    MessageBox.Show($"Введите ответ для задания {taskNum} вручную или загрузите файл с ответом",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

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

            string answerText = txtAnswer.Text.Trim();
            if (string.IsNullOrEmpty(answerText))
            {
                MessageBox.Show("Введите ответ вручную или загрузите файл с ответом",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((_tasksWithOneExtra.Contains(_taskNumber) || _tasksWithTwoExtra.Contains(_taskNumber))
                && string.IsNullOrEmpty(_extraAFilePath))
            {
                MessageBox.Show("Загрузите дополнительный материал A",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

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
            string task1921FolderPath = Path.Combine(_storagePath, PAPKA_1921);
            Directory.CreateDirectory(task1921FolderPath);

            string variantFolderPath = Path.Combine(task1921FolderPath, VariantNumber.ToString());
            Directory.CreateDirectory(variantFolderPath);

            for (int taskNum = 19; taskNum <= 21; taskNum++)
            {
                string taskFolderPath = Path.Combine(variantFolderPath, taskNum.ToString());
                Directory.CreateDirectory(taskFolderPath);

                string taskDestPath = Path.Combine(taskFolderPath, "task" + Path.GetExtension(_taskFiles1921[taskNum]));
                File.Copy(_taskFiles1921[taskNum], taskDestPath, true);

                string answerText = _answerTexts1921[taskNum];
                if (string.IsNullOrEmpty(answerText) && !string.IsNullOrEmpty(_answerFiles1921[taskNum]))
                {
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
            string taskFolderPath = Path.Combine(_storagePath, _taskNumber.ToString());
            Directory.CreateDirectory(taskFolderPath);

            string variantFolderPath = Path.Combine(taskFolderPath, VariantNumber.ToString());
            Directory.CreateDirectory(variantFolderPath);

            string taskDestPath = Path.Combine(variantFolderPath, "task" + Path.GetExtension(_taskFilePath));
            File.Copy(_taskFilePath, taskDestPath, true);

            string answerDestPath = Path.Combine(variantFolderPath, "answer.txt");
            File.WriteAllText(answerDestPath, txtAnswer.Text.Trim());

            if (!string.IsNullOrEmpty(_extraAFilePath))
            {
                string extraADestPath = Path.Combine(variantFolderPath, "A" + Path.GetExtension(_extraAFilePath));
                File.Copy(_extraAFilePath, extraADestPath, true);
            }

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
                statusText.Text = Path.GetFileName(filePath);
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