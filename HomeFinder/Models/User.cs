namespace HomeFinder.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int MinCost { get; set; }
        public int MaxCost { get; set; }
        public int MinRoomCount { get; set; }
        public int MaxRoomCount { get; set; }
    }
}
