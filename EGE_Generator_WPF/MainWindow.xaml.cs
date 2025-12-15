using System;
using System.IO;
using System.Windows;
using EgeGenerator;

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
                string putKhranilishcha = StorageService.PodgotovitKhranilishcheDlyaDobavleniya(this);
                if (string.IsNullOrEmpty(putKhranilishcha))
                    return;

                var dialogNomerZadaniya = new TaskNumberDialog();
                if (dialogNomerZadaniya.ShowDialog() == true)
                {
                    int nomerZadaniya = dialogNomerZadaniya.NomerZadaniya;

                    // Определяем тип задания
                    string tipZadaniya = ValidationService.OpredelitTipZadaniya(nomerZadaniya);

                    // Выбираем правильное окно (старый синтаксис для C# 7.3)
                    Window dialogDobavleniya = null;

                    if (tipZadaniya == "1921")
                    {
                        dialogDobavleniya = new AddTasks1921Dialog(putKhranilishcha);
                    }
                    else if (tipZadaniya == "prostoe")
                    {
                        dialogDobavleniya = new AddSimpleTaskDialog(nomerZadaniya, putKhranilishcha);
                    }
                    else if (tipZadaniya == "s_odnim_dop")
                    {
                        dialogDobavleniya = new AddTaskWithOneExtraDialog(nomerZadaniya, putKhranilishcha);
                    }
                    else if (tipZadaniya == "s_dvumya_dop")
                    {
                        dialogDobavleniya = new AddTaskWithTwoExtrasDialog(nomerZadaniya, putKhranilishcha);
                    }
                    else
                    {
                        throw new Exception($"Неизвестный тип задания {nomerZadaniya}");
                    }

                    if (dialogDobavleniya.ShowDialog() == true)
                    {
                        // Получаем номер варианта (старый синтаксис)
                        int nomerVarianta = 0;

                        if (dialogDobavleniya is AddSimpleTaskDialog simpleDialog)
                        {
                            nomerVarianta = simpleDialog.NomerVarianta;
                        }
                        else if (dialogDobavleniya is AddTaskWithOneExtraDialog oneExtraDialog)
                        {
                            nomerVarianta = oneExtraDialog.NomerVarianta;
                        }
                        else if (dialogDobavleniya is AddTaskWithTwoExtrasDialog twoExtrasDialog)
                        {
                            nomerVarianta = twoExtrasDialog.NomerVarianta;
                        }
                        else if (dialogDobavleniya is AddTasks1921Dialog zadaniya1921Dialog)
                        {
                            nomerVarianta = zadaniya1921Dialog.NomerVarianta;
                        }

                        MessageBox.Show($"Успешно добавлен вариант {nomerVarianta} для задания {nomerZadaniya}!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string putKhranilishcha = StorageService.VybratPapku(this, "Выберите папку хранилища с заданиями");
                if (string.IsNullOrEmpty(putKhranilishcha))
                    return;

                // Проверяем хранилище
                ValidationService.ProveritKhranilishche(putKhranilishcha);

                // Получаем количество вариантов
                int kolichestvoVariantov = PoluchitKolichestvoVariantov();
                if (kolichestvoVariantov <= 0)
                    return;

                string putVykhoda = StorageService.VybratPapku(this, "Выберите папку для сохранения вариантов");
                if (string.IsNullOrEmpty(putVykhoda))
                    return;

                // Генерируем варианты
                Random random = new Random();
                FileService.GenerirovatVarianty(putKhranilishcha, putVykhoda, kolichestvoVariantov, random);

                MessageBox.Show($"Успешно создано {kolichestvoVariantov} вариантов!\n\nПапка: {putVykhoda}",
                    "Генерация завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int PoluchitKolichestvoVariantov()
        {
            var dialogVvoda = new InputDialog("Количество вариантов");
            if (dialogVvoda.ShowDialog() == true)
            {
                if (int.TryParse(dialogVvoda.Otvet, out int kolichestvo) && kolichestvo > 0 && kolichestvo <= 100)
                {
                    return kolichestvo;
                }
                else
                {
                    MessageBox.Show("Введите корректное положительное число меньше 100",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return 0;
        }

        private void BtnViewVariant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string vybrannyPut = StorageService.VybratPapku(this, "Выберите папку с вариантом для просмотра");
                if (string.IsNullOrEmpty(vybrannyPut))
                    return;

                string putKZadaniyam = System.IO.Path.Combine(vybrannyPut, "Задания");
                if (System.IO.Directory.Exists(putKZadaniyam))
                {
                    var oknoProsmotra = new ViewTaskWindow(vybrannyPut);
                    oknoProsmotra.Owner = this;
                    oknoProsmotra.ShowDialog();
                }
                else
                {
                    // Ищем вложенные папки
                    foreach (string papka in System.IO.Directory.GetDirectories(vybrannyPut))
                    {
                        string vlozhennayaPapka = System.IO.Path.Combine(papka, "Задания");
                        if (System.IO.Directory.Exists(vlozhennayaPapka))
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}