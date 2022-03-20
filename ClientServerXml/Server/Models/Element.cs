using System.Collections.Generic;

namespace Server.Models
{
    public class Element
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
        public string Content { get; set; }
        public virtual ICollection<Duplicate> Duplicates { get; set; }
    }
}
