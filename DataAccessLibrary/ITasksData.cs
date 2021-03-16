using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
namespace DataAccessLibrary
{
    public interface ITasksData
    {
        Task<List<TaskModel>> GetTasksTest();
        Task InsertTaskTest(TaskModel task);
    }
}