public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public byte[] PasswordHash { get; set; }
    
    // For simplicity, we use one Role per user.
    public int RoleId { get; set; }
    public Role Role { get; set; }
}