using DataAccessLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public interface ISqlDataAccess
    {
        string ConnectionStringName { get; set; }

        void CreateTask(TaskModel model);
        Task<List<T>> LoadData<T, U>(string sql, U parameters);
        Task SaveData<T>(string sql, T parameters);
        List<TagModel> Tags_GetAll();
        List<TaskModel> Tasks_GetAll();
        //TaskModel TaskById(int id);
        void DeleteTask(int Id);
        void UpdateTask(TaskModel model);
        void UpdateTaskCompletion(TaskModel model);
        void UpdateSubtaskCompletion(SubtaskModel model);
    }
}