using System.Collections.Generic;

namespace Server.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Element> Elements { get; set; }
    }
}
