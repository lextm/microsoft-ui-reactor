namespace ApiDash.Models;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
