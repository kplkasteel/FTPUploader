using System;
using System.IO;
using System.Linq;
using System.Net;
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
    public class UploadFiles :WebService
    {
        private string _message;
        [WebMethod]
        public string SaveAndUpload()
        {
            UploadFileToServer("ftp://cloudplatform.iconnix.co.za", "Mobile1", "Mobile1",@"C:\NashuaReports");

            DeleteFiles(@"C:\NashuaReports");
            return _message;
        }

        public void UploadFileToServer(string url, string username, string password, string fileName)
        {   
            var serverUri = new Uri(url);
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                _message = "Incorrect FTP URL";
                return;
            }
              

            try
            {
                var fileArray = Directory.GetFiles(fileName, "*.xl*");
                if (!fileArray.Any())
                {
                   _message = "No Attachments found";
                    return;
                }

                foreach (var file in fileArray)
                {
                    // Get the object used to communicate with the server.
                    var request = (FtpWebRequest)WebRequest.Create(serverUri + @"/" + Path.GetFileName(file));
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
                    Console.WriteLine("Writing {0} bytes to the stream.", count);
                    // IMPORTANT: Close the request stream before sending the request.
                    requestStream.Close();

                    var response = (FtpWebResponse)request.GetResponse();
                    var code = response.StatusCode;
                    if (response.StatusCode == FtpStatusCode.ClosingData)
                    {
                        _message = "226";
                    }
                    else
                    {
                        _message = $"Upload status: {response.StatusCode} {response.StatusDescription}";
                    }
                    response.Close();
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                _message = e.ToString();
            }

        }

        private static void DeleteFiles(string filePatch)
        {
            var deleteFileArray = Directory.GetFiles(filePatch);
            if (deleteFileArray.Length <= 0) return;
            foreach (var file in deleteFileArray)
            {
                var fileInfo = new FileInfo(file);
                fileInfo.Delete();
            }
        }
    }

}
