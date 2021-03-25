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
        public string Name { get; set; }
    }

    public class TagComparer : IEqualityComparer<TagModel>
    {
        public bool Equals(TagModel x, TagModel y)
        {
            if (x.Id == y.Id && x.Name.ToLower() == y.Name.ToLower())
                return true;
            return false;


        }
        public int GetHashCode(TagModel obj)
        {
            return obj.GetHashCode();
        }
    }
}
