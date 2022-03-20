namespace Server.Models
{
    public class Duplicate
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public int NumRepetitions { get; set; }
        public int ElementId { get; set; }
        public virtual Element Element { get; set; }
    }
}
