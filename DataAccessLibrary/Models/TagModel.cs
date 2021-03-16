using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class TagModel
    {
        /// <summary>
        /// Unique identifier for the tag.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the tag. Uniqueness enforced by the creation function.
        /// </summary>
        public int Name { get; set; }
    }
}
