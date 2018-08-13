using System;
using System.IO;
using System.Net;
using System.Text;

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

   
    }
}