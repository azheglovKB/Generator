using System.Collections.Generic;
using System.IO;

namespace EgeGenerator.Models
{
    public enum TaskType
    {
        Regular,
        WithOneExtra,
        WithTwoExtras,
        Grouped19_21
    }

    public class EgeTask
    {
        public int Number { get; set; }
        public TaskType Type { get; set; }
        public string ImagePath { get; set; }
        public string Answer { get; set; }
        public string ExtraFileA { get; set; }
        public string ExtraFileB { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsSelected { get; set; }

        public EgeTask()
        {
            ImagePath = "";
            Answer = "";
            ExtraFileA = "";
            ExtraFileB = "";
        }

        public static TaskType GetTaskType(int taskNumber)
        {
            var tasksWithOneExtra = new HashSet<int> { 3, 9, 10, 17, 18, 22, 24, 26 };
            var tasksWithTwoExtras = new HashSet<int> { 27 };

            if (taskNumber >= 19 && taskNumber <= 21)
                return TaskType.Grouped19_21;
            if (tasksWithTwoExtras.Contains(taskNumber))
                return TaskType.WithTwoExtras;
            if (tasksWithOneExtra.Contains(taskNumber))
                return TaskType.WithOneExtra;

            return TaskType.Regular;
        }

        public string GetExtraFileAExtension()
        {
            if (string.IsNullOrEmpty(ExtraFileA))
                return "";

            return Path.GetExtension(ExtraFileA);
        }

        public string GetExtraFileBExtension()
        {
            if (string.IsNullOrEmpty(ExtraFileB))
                return "";

            return Path.GetExtension(ExtraFileB);
        }
    }
}