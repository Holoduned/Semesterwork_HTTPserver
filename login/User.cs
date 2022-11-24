using System.Net;
using System.Text;

namespace Http_Server;

public enum Role{Admin,Client};

public class Client
{
    public long Id { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }

    public string Name { get; set; }
    public string Surname { get; set; }
    public string Region { get; set; }

    public Role Role { get; set; }
}