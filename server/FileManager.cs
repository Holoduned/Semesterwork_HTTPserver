namespace Http_Server;

public class FileManager
{
    public static byte[] getFile(string rawUrl, ServerSettings _serverSetting) 
    {
        byte[] buffer = null;
        var filePath = _serverSetting.Path + rawUrl;
            
        if (Directory.Exists(filePath))
        {
                
            filePath = filePath + "main/index.html";

            if (File.Exists(filePath))
            {
                Console.WriteLine(filePath);
                buffer = File.ReadAllBytes(filePath);
            }

        }
        else if (File.Exists(filePath))
            buffer = File.ReadAllBytes(filePath);
        return buffer; 
    }
}