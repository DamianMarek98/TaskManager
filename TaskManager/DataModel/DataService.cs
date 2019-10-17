using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//class which is responsible for editing data in database form app level

namespace TaskManager.DataModel
{
    class DataService
    {
        public static void RemoveTask(int id)
        {
            var dbContext = new TaskContext();
            var task = dbContext.Tasks.FirstOrDefault(x => x.Id == id);
            dbContext.Tasks.Remove(task);
            dbContext.SaveChanges();
        }

        public static void EditTask(TTask editedTask)
        {
            var dbContext = new TaskContext();
            var taskToEdit = dbContext.Tasks.FirstOrDefault(x => x.Id == editedTask.Id);
            taskToEdit.TaskName = editedTask.TaskName;
            taskToEdit.TaskContent = editedTask.TaskContent;
            if (editedTask.TaskDate != String.Empty) taskToEdit.TaskDate = Convert.ToDateTime(editedTask.TaskDate);
            else taskToEdit.TaskDate = null;
            taskToEdit.PriorityId = getPriorityId(editedTask.TaskPriority);
            taskToEdit.StatusId = getStatusId(editedTask.TaskStatus);
            dbContext.SaveChanges();
        }

        public static void AddTask(TTask newTask)
        {
            var dbContext = new TaskContext();
            var maxID = dbContext.Tasks.Max(x => newTask.Id);
            Task taskToStore = new Task();
            taskToStore.Id = maxID + 1;
            taskToStore.PriorityId = getPriorityId(newTask.TaskPriority);
            taskToStore.StatusId = getStatusId(newTask.TaskStatus);
            taskToStore.TaskContent = newTask.TaskContent;
            taskToStore.TaskName = newTask.TaskName;
            taskToStore.TaskDate = Convert.ToDateTime(newTask.TaskDate);

            dbContext.Tasks.Add(taskToStore);
            dbContext.SaveChanges();

        }

        public static int getPriorityId(string priority)
        {
            var dbContext = new TaskContext();
            return dbContext.TaskPriorities.FirstOrDefault(x => x.PriorityName == priority).Id;
        }
        public static int getStatusId(string status)
        {
            var dbContext = new TaskContext();
            return dbContext.TaskStatuses.FirstOrDefault(x => x.StatusName == status).Id;
        }
    }
}
