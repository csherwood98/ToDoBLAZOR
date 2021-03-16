using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class TaskModel
    {
        /// <summary>
        /// Unique identifier for the task.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Full description of what the task is, for display purposes. Limit 300 char in MySql database.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The priority ranking of the task
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// The assigned due date of the task, optional.
        /// </summary>
        public DateTimeOffset DueDate { get; set; }
        /// <summary>
        /// Set to true if task was completed.
        /// </summary>
        public bool CompletionFlag { get; set; }
        /// <summary>
        /// Stores the DateTime of when the task was set to complete, used for automatic data cleanup.
        /// </summary>
        public DateTimeOffset DateCompleted { get; set; }
        /// <summary>
        /// List of tags associated with the task, may be empty.
        /// </summary>
        public List<TagModel> Tags { get; set; }
        /// <summary>
        /// List of subtasks associated with the task, may be empty.
        /// </summary>
        public List<SubtaskModel> Subtasks { get; set; }
    }
}
