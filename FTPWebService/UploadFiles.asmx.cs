using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;

namespace FTPWebService
{
    /// <summary>
    /// Summary description for UploadFiles
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class UploadFiles : System.Web.Services.WebService
    {

        [WebMethod]
        public string SaveAndUpload(string ftpSite, string ftpUsername, string ftpPassword, string reportLocation)
        {
            var ftp = new Ftp();
            return ftp.UploadFileToServer(new Uri(ftpSite), ftpUsername, ftpPassword, reportLocation);
        }

        public class Ftp
        {
            public string UploadFileToServer(Uri serverUri, string username, string password, string fileName)
            {
                var message = string.Empty;
                if (serverUri.Scheme != Uri.UriSchemeFtp) return "Incorrect FTP URL";

                try
                {
                    var fileArray = Directory.GetFiles(fileName, "*.xl*");
                    if (!fileArray.Any()) return "No Attachments found";

                    foreach (var file in fileArray)
                    {
                        // Get the object used to communicate with the server.
                        var request = (FtpWebRequest) WebRequest.Create(serverUri + @"/" + Path.GetFileName(file));
                        request.Credentials
                            = new NetworkCredential(username, password);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        // Don't set a time limit for the operation to complete.
                        request.Timeout = System.Threading.Timeout.Infinite;

                        // Copy the file contents to the request stream.
                        const int bufferLength = 2048;
                        var buffer = new byte[bufferLength];

                        var count = 0;
                        int readBytes;
                        var stream = File.OpenRead(file);
                        var requestStream = request.GetRequestStream();
                        do
                        {
                            readBytes = stream.Read(buffer, 0, bufferLength);
                            requestStream.Write(buffer, 0, bufferLength);
                            count += readBytes;
                        } while (readBytes != 0);

                        //Console.WriteLine("Writing {0} bytes to the stream.", count);
                        // IMPORTANT: Close the request stream before sending the request.
                        requestStream.Close();

                        var response = (FtpWebResponse) request.GetResponse();
                        if (response.StatusCode.ToString() == "226")
                        {
                            message = "226";
                        }
                        message = $"Upload status: {response.StatusCode} {response.StatusDescription}";
                        response.Close();
                    }

                    var deleteFileArray = Directory.GetFiles(fileName);
                    if (deleteFileArray.Length > 0)
                    {
                        foreach (var file in deleteFileArray)
                        {
                            File.Delete(file);
                        }
                    }

                }
                catch (Exception e)
                {
                    message = e.InnerException?.ToString();
                }
                return message;
            }
        }
    }
}
