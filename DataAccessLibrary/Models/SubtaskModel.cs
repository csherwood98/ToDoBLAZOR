using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class SubtaskModel
    {
        /// <summary>
        /// Unique identifier of the subtask
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Full description of what the subtask is. Limit 300 char.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Set to true if subtask was completed.
        /// </summary>
        public bool CompletionFlag { get; set; }
    }
}
