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

        private string _taskFilePath;
        private string _answerFilePath;
        private string _extraAFilePath;
        private string _extraBFilePath;

        private static readonly HashSet<int> _tasksWithOneExtra = new HashSet<int> { 3, 9, 10, 17, 18, 22, 24, 26 };
        private static readonly HashSet<int> _tasksWithTwoExtra = new HashSet<int> { 27 };
        private static readonly string[] _allowedExtraExtensions = { ".txt", ".ods", ".xlsx", ".xls", ".pdf", ".doc", ".docx" };

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

            InitializeUI();
            UpdateVariantNumber();
        }

        private void InitializeUI()
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
            string taskFolderPath = Path.Combine(_storagePath, _taskNumber.ToString());

            if (!Directory.Exists(taskFolderPath))
            {
                VariantNumber = 1;
                txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber} (новая папка задания)";
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
            for (int i = 1; i <= 1000; i++) // Ограничим 1000 вариантов
            {
                if (!existingNumbers.Contains(i))
                {
                    VariantNumber = i;
                    txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber}";
                    return;
                }
            }

            // Если все номера до 1000 заняты, берём следующий
            VariantNumber = existingNumbers.Max() + 1;
            txtVariantInfo.Text = $"Будет создан вариант № {VariantNumber}";
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
            // Проверяем задание
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

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}\n\nПовторите попытку.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}