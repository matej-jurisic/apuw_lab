namespace APUW.Model.DTOs.Boards
{
    public class BoardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
    }
}
