﻿using Dapper;
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

        public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var data = await connection.QueryAsync<T>(sql, parameters);

                return data.ToList();
            }
        }

        public async Task SaveData<T>(string sql, T parameters)
        {
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(sql, parameters);
            }
        }
    }
}
