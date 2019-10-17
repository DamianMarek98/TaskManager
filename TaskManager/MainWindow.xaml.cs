using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskManager.DataModel;

namespace TaskManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int SelectedTaskId; //wchih task is selected
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            SelectedTaskId = 0; //at the initialization there is no selected task
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        
        }

        void LoadData()
        {
            using (var db = new TaskContext()) //using database tasks context to get data
            {
                List<Task> Tasks = db.Tasks.AsNoTracking().ToList();
                lvTasks.ItemsSource = GetTTasks(Tasks);
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvTasks.ItemsSource);            
                PropertyGroupDescription groupDescription = new PropertyGroupDescription(getTaskSortStyle()); //selecting sorting style
                view.GroupDescriptions.Add(groupDescription);
            }
            clearTextAreas();
        }

        string getTaskSortStyle()
        {
            if (SortByStatusCheckBox.IsChecked == true) return "TaskPriority";
            else return "TaskStatus";
        }

        List<TTask> GetTTasks(List<Task> tasks)
        {
            List<TTask> TTask = new List<TTask>();
            using (var db = new TaskContext())
            {
                foreach (Task task in tasks)
                {
                    TaskStatus status = db.TaskStatuses.Where(x => x.Id == task.StatusId).FirstOrDefault();
                    TaskPriority priority = db.TaskPriorities.AsNoTracking().Where(x => x.Id == task.PriorityId).FirstOrDefault();
                    string date = getDate(task);
                    TTask.Add(new DataModel.TTask { Id = task.Id, PriorityId = priority.Id, StatusId = status.Id, TaskContent = task.TaskContent, TaskDate = date, TaskName = task.TaskName, TaskPriority = priority.PriorityName, TaskStatus = status.StatusName });
                }
            }
            return TTask;
        }

        string getDate(Task task)
        {
            if (task.TaskDate == null) return String.Empty;
            else return task.TaskDate.ToString().Substring(0, 10);
        }

        private void lvTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView dataList = (ListView)sender;
            if(dataList.SelectedItem != null) FillControlsWithDataFormSelectedTask((TTask)dataList.SelectedItem);       
            else SelectedTaskContent.Text = String.Empty;
        }

        void FillControlsWithDataFormSelectedTask(TTask selectedTask)
        {
            TaskNameEditTextBox.Text = selectedTask.TaskName;
            SelectedTaskContent.Text = selectedTask.TaskContent;
            PriorityEditComboBox.SelectedIndex = GetTaskPriorityComboBoxIndex(selectedTask);
            StatusEditComboBox.SelectedIndex = GetTaskStatusComboBoxIndex(selectedTask);
            if(selectedTask.TaskDate != String.Empty) TaskDatePicker.SelectedDate = Convert.ToDateTime(selectedTask.TaskDate);
            NumberOfCharactersInContentTextBox.Text = "Number of characters: " + SelectedTaskContent.Text.Length.ToString();
            SaveChangesButton.IsEnabled = true;
            NewOrEditTaskInfoTExtBox.Text = "Selected task content:";
            SelectedTaskId = selectedTask.Id;
        }

        int GetTaskPriorityComboBoxIndex(TTask task)
        {
            if (task.TaskPriority == "low") return 0;
            else if (task.TaskPriority == "high") return 2;
            else return 1;
        }

        int GetTaskStatusComboBoxIndex(TTask task)
        {
            if (task.TaskStatus == "new") return 0;
            else if (task.TaskStatus == "finished") return 2;
            else return 1;
        }

        private void DeleteSelectedTask_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTaskId != 0)
            {
                DataService.RemoveTask(SelectedTaskId);
                clearTextAreas();
                LoadData();
            }
        }

        private void SelectedTaskContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumberOfCharactersInContentTextBox.Text = "Number of characters: " + SelectedTaskContent.Text.Length.ToString();
            if (checkIfNumberOfCharactersInContentIsLowerThanMaxAndGreaterThanMin(SelectedTaskContent.Text.Length))
            {
                NumberOfCharactersInContentTextBox.Foreground = new SolidColorBrush(Colors.Red);
                SaveChangesButton.IsEnabled = false;
            }
            else
            {
                NumberOfCharactersInContentTextBox.Foreground = new SolidColorBrush(Colors.Black);
                if(!checkIfNumberOfCharactersInNameIsLowerThanMaxOrGreaterThanMin(TaskNameEditTextBox.Text.Length)) SaveChangesButton.IsEnabled = true;
            }
        }

        Boolean checkIfNumberOfCharactersInContentIsLowerThanMaxAndGreaterThanMin(int number) 
        {
            if (number > 1000) return true;
            else return false;
        }

        private void TaskNameEditTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (checkIfNumberOfCharactersInNameIsLowerThanMaxOrGreaterThanMin(TaskNameEditTextBox.Text.Length))
            {
                NumberOfCharactersInNameTextBox.Text = "Not valid!";
                SaveChangesButton.IsEnabled = false;
            }
            else
            {
                NumberOfCharactersInNameTextBox.Text = String.Empty;
                if (!checkIfNumberOfCharactersInContentIsLowerThanMaxAndGreaterThanMin(SelectedTaskContent.Text.Length)) SaveChangesButton.IsEnabled = true;
            }
        }

        Boolean checkIfNumberOfCharactersInNameIsLowerThanMaxOrGreaterThanMin(int number)
        {
            if (number > 30 || number < 1) return true;
            else return false;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e) //event leads to save or update task in database depends on SelectedTaskId
        {
            TTask editedTask = new TTask();
            editedTask.TaskName = TaskNameEditTextBox.Text;
            editedTask.TaskContent = SelectedTaskContent.Text;
            editedTask.TaskStatus = getTaskStatusById(StatusEditComboBox.SelectedIndex);
            editedTask.TaskPriority = getTaskPriorityById(PriorityEditComboBox.SelectedIndex);
            editedTask.TaskDate = TaskDatePicker.ToString();

            if (SelectedTaskId == 0) SaveNewTaskInDatabase(editedTask);
            else EditTaskInDatabase(editedTask);

            LoadData();
        }

        void SaveNewTaskInDatabase(TTask newTask)
        {
            DataService.AddTask(newTask);
        }

        void EditTaskInDatabase(TTask editedTask)
        {
            editedTask.Id = SelectedTaskId;
            DataService.EditTask(editedTask);
        }

        string getTaskStatusById(int id)
        {
            if (id == 0) return "new";
            else if (id == 1) return "in progress";
            else return "finished";
        }

        string getTaskPriorityById(int id)
        {
            if (id == 0) return "low";
            else if (id == 1) return "normal";
            else return "high";
        }

        private void AddNewTask_Click(object sender, RoutedEventArgs e)
        {
            clearTextAreas();
        }

        void clearTextAreas()
        {
            SelectedTaskId = 0;
            TaskNameEditTextBox.Text = String.Empty;
            SelectedTaskContent.Text= String.Empty;
            StatusEditComboBox.SelectedIndex = 1;
            PriorityEditComboBox.SelectedIndex = 1;
            TaskDatePicker.SelectedDate = DateTime.Today;
            NewOrEditTaskInfoTExtBox.Text = "New task data:";
            NumberOfCharactersInNameTextBox.Text = "Not valid!";
        }

        private void SortByStatusCheckBox_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
