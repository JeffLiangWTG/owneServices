using System;
using System.Threading;
using FluentFTP;
using Hawking.CSI.Plugins.FTP;

namespace TestFtp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TestFtp();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Done");
        }

        private static void TestFtp()
        {
            var ftp = new Hawking.CSI.Plugins.FTP.FtpClient();
            var ftpOptions = new FtpOptions
            {
                Server = "localhost",
                User = "sydco-wbln-2\\hawking",
                Password = "3hubRock$"
            };
            ftp.OpenAsync(ftpOptions, CancellationToken.None).Wait();
            var list = ftp.ListFilesAsync("YYYYYY\\IN", "", CancellationToken.None).Result;
            foreach (var file in list)
            {
                System.Console.WriteLine(file.Name);
            }
        }
    }
}
