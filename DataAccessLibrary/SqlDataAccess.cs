using Dapper;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;
        //Go through and make sure public and private are used consistently
        public string ConnectionStringName { get; set; } = "Default";


        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        public List<TaskModel> Tasks_GetAll()
        {
            List<TaskModel> output;
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                output = connection.Query<TaskModel>("dbo.spTasks_GetAll").ToList();

                foreach (TaskModel t in output)
                {
                    var p = new DynamicParameters();
                    p.Add("@ParentTaskId", t.Id);

                    t.Subtasks = connection.Query<SubtaskModel>("dbo.spSubtasks_GetByTask", p, commandType: CommandType.StoredProcedure).ToList();

                    t.Tags = connection.Query<TagModel>("dbo.spTags_GetByTask", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }

        public List<TagModel> Tags_GetAll()
        {
            List<TagModel> output;

            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                output = connection.Query<TagModel>("dbo.spTags_GetAll").ToList();
            }

            return output;
        }

        public void CreateTask(TaskModel model)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                //Save the task itself
                SaveTask(connection, model);
                //Save the subtasks
                SaveSubtasks(connection, model);
                //Save the tags OR associate pre-existing tag IDs to the current one.
                SaveTags(connection, model);
                //Link up tags with the task
                LinkTags(connection, model);
            }
        }

        private void SaveTask(IDbConnection connection, TaskModel model)
        {
            var p = new DynamicParameters();
            p.Add("@Description", model.Description);
            p.Add("@Priority", model.Priority);
            p.Add("@DueDate", model.DueDate);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTasks_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

        private void SaveSubtasks(IDbConnection connection, TaskModel model)
        {
            //Nullcheck & empty list check
            if (model.Subtasks is null || model.Subtasks.Count == 0)
            {
                return;
            }
            
            foreach (SubtaskModel stm in model.Subtasks)
            {
                var p = new DynamicParameters();
                p.Add("@Description", stm.Description);
                p.Add("@ParentTaskId", model.Id);

                connection.Execute("dbo.spSubtasks_Insert", p, commandType:CommandType.StoredProcedure);
            }
        }

        private void SaveTags(IDbConnection connection, TaskModel model)
        {
            //Nullcheck & empty list check
            if (model.Tags is null || model.Tags.Count == 0)
            {
                return;
            }
            // Gets the already existing list of tags
            List<TagModel> existingTags = Tags_GetAll();

            foreach (TagModel tm in model.Tags)
            {
                //Checks for duplicate tag and if they already exist, assign id from preexisting tag
                int idFinder = existingTags.FindIndex(x => x.Name == tm.Name);
                if (idFinder != -1)
                {
                    tm.Id = existingTags[idFinder].Id;
                }
                else
                {
                    var p = new DynamicParameters();
                    p.Add("@Name", tm.Name);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spTags_Insert", p, commandType: CommandType.StoredProcedure);

                    tm.Id = p.Get<int>("@id");
                    existingTags.Add(tm);
                }
            }
        }
        /// <summary>
        /// Associates the Tag IDs with the Task ID within the SQL database for display purposes.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="model"></param>
        private void LinkTags(IDbConnection connection, TaskModel model)
        {
            //Nullcheck & empty list check
            if (model.Tags is null || model.Tags.Count == 0)
            {
                return;
            }

            foreach (TagModel tm in model.Tags)
            {
                var p = new DynamicParameters();
                p.Add("@TaskId", model.Id);
                p.Add("@TagId", tm.Id);

                connection.Execute("dbo.spTaskTags_InsertLink", p, commandType:CommandType.StoredProcedure);
            }
        }

        public void DeleteTask(int Id)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                var p = new DynamicParameters();
                p.Add("@Id", Id);

                connection.Execute("dbo.spTasks_DeleteById", p, commandType: CommandType.StoredProcedure);

                DeleteUnusedTags(connection);
            }
        }

        public void UpdateTask(TaskModel model)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                //Update the task table itself
                UpdateTaskTable(connection, model);
                //Delete subtasks and create the new ones. (This is the simplest way to do it with little to no cost in efficiency.)
                UpdateSubtasks(connection, model);
                //Delink the tags from the task and create any new tags added
                UpdateTags(connection, model);
                //Link the newly updated tagset to the task
                LinkTags(connection, model);
                //Delete Tags that have no links
                DeleteUnusedTags(connection);
            }
        }
        private void UpdateTaskTable(IDbConnection connection, TaskModel model)
        {
            var p = new DynamicParameters();
            p.Add("@Id", model.Id);
            p.Add("@Description", model.Description);
            p.Add("@Priority", model.Priority);
            p.Add("@DueDate", model.DueDate);

            connection.Execute("dbo.spTasks_Update", p, commandType: CommandType.StoredProcedure);
        }
        private void UpdateSubtasks(IDbConnection connection, TaskModel model)
        {
            var p = new DynamicParameters();
            p.Add("@ParentTaskId", model.Id);
            connection.Execute("dbo.spSubtasks_DeleteByTask", p, commandType: CommandType.StoredProcedure);

            SaveSubtasks(connection, model);
        }
        private void UpdateTags(IDbConnection connection, TaskModel model)
        {
            var p = new DynamicParameters();
            p.Add("@id_Task", model.Id);

            connection.Execute("dbo.spTaskTags_DeleteByTask", p, commandType: CommandType.StoredProcedure);

            SaveTags(connection, model);

        }
        private void DeleteUnusedTags(IDbConnection connection)
        {
            connection.Execute("dbo.sp_Tags_DeleteUnused", commandType: CommandType.StoredProcedure);
        }

        public void UpdateTaskCompletion(TaskModel model)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                var p = new DynamicParameters();
                p.Add("@Id", model.Id);
                p.Add("@CompletionFlag", model.CompletionFlag);
                p.Add("@DateCompleted", model.DateCompleted);
                connection.Execute("dbo.spTasks_ChangeFlag", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void UpdateSubtaskCompletion(SubtaskModel model)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                var p = new DynamicParameters();
                p.Add("@Id", model.Id);
                p.Add("@CompletionFlag", model.CompletionFlag);

                connection.Execute("dbo.spSubtasks_ChangeFlag", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void DeleteAllCompleted()
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                connection.Execute("dbo.spTasks_DeleteAllCompleted", commandType: CommandType.StoredProcedure);
            }
        }




        //public TaskModel TaskById(int Id)
        //{
        //    string connectionString = _config.GetConnectionString(ConnectionStringName);

        //    TaskModel output = new TaskModel();

        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
        //    {
        //        //This might not work??? If not, will need to set up an intermediary value to accept the one-item long list and then convert it
        //        output = (TaskModel)connection.Query<TagModel>("dbo.spTags_GetAll");
        //    }
        //}

    }
}
