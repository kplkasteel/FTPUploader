using System;
using System.IO;
using System.Net;

namespace FTPUploader
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 4)
            {
                var message = new Messages();
                message.ShowMessage();
                return 0;
            }

            var ftp = new Ftp();
            return ftp.UploadFileToServer(new Uri(args[0]), args[1], args[2], args[3]) ? 1 : 0;
        }

        public class Messages
        {
            public void ShowMessage()
            {
                Console.WriteLine(
                    @"4 arguments required:
1.) FTP Url
2.) UserName
3.) Password
4.) File-path + filename
Press any Key to close....
                    ");
                Console.ReadLine();
            }
        }
    }

    public class Ftp
    {
        public bool UploadFileToServer(Uri serverUri, string username, string password, string fileName)
        {
            if (serverUri.Scheme != Uri.UriSchemeFtp) return false;

            try
            { 
                var fileArray = Directory.GetFiles(fileName, "*.xl*");
                foreach (var file in fileArray)
                {
                    // Get the object used to communicate with the server.
                    var request = (FtpWebRequest)WebRequest.Create(serverUri + @"/" +  Path.GetFileName(file));
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
                    Console.WriteLine("Upload status: {0}, {1}", response.StatusCode, response.StatusDescription);

                    response.Close();
                    
                }
                var deleteFileArray = Directory.GetFiles(fileName);
                if (deleteFileArray.Length <= 0) return false;
                foreach (var file in deleteFileArray)
                {
                    File.Delete(fileName);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}