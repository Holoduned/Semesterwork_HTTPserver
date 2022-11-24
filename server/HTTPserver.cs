using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Http_Server;

public enum ServerStatus{Start,Stop};
public class HttpServer : IDisposable
{
    public ServerStatus Status = ServerStatus.Stop;
    private ServerSettings _serverSetting=new ServerSettings();
    private HttpListener _listener;
  
    public HttpServer() 
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:"+ _serverSetting.Port + "/");
    }
    public void Start()
    {
        _listener.Start();
        Status = ServerStatus.Start;
        Listening();
    }

    public void Stop()
    {
        _listener.Stop();
        Status = ServerStatus.Stop;
    }
    
    private async void Listening()
    {
            
        while (_listener.IsListening)
        {
            if (Status == ServerStatus.Stop)
                return;
               
            try
            {
                var _httpContext = await _listener.GetContextAsync();

                if (MethodHandler(_httpContext)) return;

                StaticFiles(_httpContext.Request, _httpContext.Response);
            }
            catch (System.Net.HttpListenerException) { }
        }
    }
    private bool MethodHandler(HttpListenerContext _httpContext)
    {
        HttpListenerRequest request = _httpContext.Request;

        HttpListenerResponse response = _httpContext.Response;

        if (_httpContext.Request.Url.Segments.Length < 2) return false;

        string controllerName = _httpContext.Request.Url.Segments[1].Replace("/", "");

        string[] strParams = _httpContext.Request.Url
            .Segments
            .Skip(2)
            .Select(s => s.Replace("/", ""))
            .ToArray();

        var assembly = Assembly.GetExecutingAssembly();

        var controller = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(HttpController))).FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());

        if (controller == null) return false;

        var test = typeof(HttpController).Name;
        var method = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name == $"Http{_httpContext.Request.HttpMethod}"))
            .FirstOrDefault();

        if (method == null) return false;

        object[] queryParams = method.GetParameters()
            .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
            .ToArray();

        var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

        response.ContentType = "Application/json";

        byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
        response.ContentLength64 = buffer.Length;

        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);

        output.Close();

        return true;
    }
    private void StaticFiles(HttpListenerRequest request, HttpListenerResponse response)
    {
        byte[] buffer;
        if (Directory.Exists(_serverSetting.Path))
        {
            buffer = FileManager.getFile(request.RawUrl.Replace("%20", " "), _serverSetting);
                

            if (buffer == null)
            {
                response.Headers.Set("Content-Type", "text/plain");
                response.StatusCode = (int)HttpStatusCode.NotFound;
                string err = "not found - 404";
                buffer = Encoding.UTF8.GetBytes(err);
            }

        }
        else
        {
            string err = $"Directory '{_serverSetting.Path}' not found";
            buffer = Encoding.UTF8.GetBytes(err);
        }
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }
    /*private void Receive()
    {
        _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
    }

    private void ListenerCallback(IAsyncResult result)
    {
        if(_listener.IsListening)
        {
            var _httpContext = _listener.EndGetContext(result);
            HttpListenerRequest request = _httpContext.Request;
            HttpListenerResponse response = _httpContext.Response;
            byte[] buffer = new byte[] { };
            string st;
            if(Directory.Exists(_serverSetting.Path))
            {
                buffer = FileManager.getFile(_httpContext.Request.RawUrl.Replace("%20", " "), _serverSetting);

                if (buffer==null)
                {
                    response.Headers.Set("Content-Type", "text/plain");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    string err = "404 - not found";
                    buffer = Encoding.UTF8.GetBytes(err);
                }
            }
            else
            {
                string err = $"Directory "+ _serverSetting.Path + " not found";
                buffer = Encoding.UTF8.GetBytes(err);
            }
            Stream output = response.OutputStream;
            output.Write(buffer,0,buffer.Length);
            output.Close();
            Receive();
        }
    }*/

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
