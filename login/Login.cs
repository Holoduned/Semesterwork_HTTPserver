using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Http_Server;

public class Login
{
    HttpClientHandler handler;
    CookieContainer cookie;
    private string login_name;

    public Login()
    {
        cookie= new CookieContainer();
        handler = new HttpClientHandler
        {
            AllowAutoRedirect = false, AutomaticDecompression = System.Net.DecompressionMethods.Deflate |
            System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.None,
            CookieContainer = cookie
        };
    }
    
    public void LoadCookies()
    {
        var formater = new BinaryFormatter();
        using (var stream = File.Create("cookies.dat"))
        {
            cookie = (CookieContainer)formater.Deserialize(stream);
        }
    }
    public void SaveCookies()
    {
        var formater = new BinaryFormatter();
        using (var stream = File.Create("cookies.dat"))
        {
            formater.Serialize(stream, cookie);
        }
    }
    public async Task<bool> GetLoginPage()
    {
        using(var page = new HttpClient(handler))
        {
            var response = await page.GetAsync("http://localhost:7700/login/login.html");
            if (response != null) 
            {
                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                var login_name = doc.DocumentNode.SelectSingleNode(".//input[@name='login_name'");
                if (login_name != null) 
                {
                    this.login_name = login_name.Attributes["name"].Value;
                }
            }
        }
        return false;
    }

    public async Task<bool> LoginValidation()
    {
        using (var page = new HttpClient(handler))
        {
            using (var content = new StringContent($""))
            {
                using(var response = await page.PostAsync("http://localhost:7700/login/login.html", content))
                {
                    if(response != null)
                    {
                        var resp = await response.Content.ReadAsStringAsync();
                        return resp == "Login-OK";
                    }
                }
            }
        }
        return false;
    }
    public async Task<bool> login()
    {
        using (var page = new HttpClient(handler, false))
        {
            using (var content = new StringContent($""))
            {
                using (var response = await page.PostAsync("http://localhost:7700/login/login.html", content))
                {
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public async void Start()
    {
        var login = new Login();
        login.LoadCookies();
        if(await login.GetLoginPage() && await login.LoginValidation()) 
        {
            if (await login.login())
            {
                login.SaveCookies();
            }
        }
    }
}
