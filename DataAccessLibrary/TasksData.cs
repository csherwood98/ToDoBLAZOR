using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
namespace DataAccessLibrary
{
    public class TasksData : ITasksData
    {
        private readonly ISqlDataAccess _db;

        public TasksData(ISqlDataAccess db)
        {
            _db = db;
        }
        public Task<List<TaskModel>> GetTasksTest()
        {
            string sql = "select * from dbo.Tasks";

            return _db.LoadData<TaskModel, dynamic>(sql, new { });
        }

        public Task InsertTaskTest(TaskModel task)
        {
            string sql = @"insert into dbo.Tasks (Description, Priority, DueDate, DateCompleted, CompletionFlag, TaskTagsId) 
                            values (@Description, @Priority, @DueDate, @DateCompleted, @CompletionFlag, @TaskTagsId)";

            return _db.SaveData(sql, task);
        }

    }
}
