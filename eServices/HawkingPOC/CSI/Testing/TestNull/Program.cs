using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Hawking.CSI.Testing.TestNull
{
    class Program
    {
        private const string EXCHANGEURI = "http://10.61.163.74:7004/v1/messages/";
        //private const string EXCHANGEURI = "http://sydco-wbln-2.wtg.zone:7004/v1/messages/";
        private const string TESTFILEDIR = @"D:\eServices\HawkingPOC\CSI\Testing\TestFiles";

        static void Main(string[] args)
        {
            int scale = 4;

            SendToXMulti("MessageX1_100.xml", scale * 3000);
            SendToXMulti("MessageX1_200.xml", scale * 3000);
            SendToXMulti("MessageX1_500.xml", scale * 3000);
            SendToXMulti("MessageX1_1000.xml", scale * 3000);
            SendToXMulti("MessageX1_2000.xml", scale * 3000);
            SendToXMulti("MessageX1_5000.xml", scale * 3000);
            SendToXMulti("MessageX1_10000.xml", scale * 3000);
            SendToXMulti("MessageX1_20000.xml", scale * 3000);
            SendToXMulti("MessageX1_50000.xml", scale * 3000);
            SendToXMulti("MessageX1_100000.xml", scale * 3000);
            SendToXMulti("MessageX1_200000.xml", scale * 2000);
            SendToXMulti("MessageX1_500000.xml", scale * 1500);
            SendToXMulti("MessageX1_1000000.xml", scale * 1000);
            SendToXMulti("MessageX1_2000000.xml", scale * 500);
            SendToXMulti("MessageX1_5000000.xml", scale * 250);
            SendToXMulti("MessageX1_10000000.xml", scale * 150);
            SendToXMulti("MessageX1_20000000.xml", scale * 100);
            SendToXMulti("MessageX1_50000000.xml", scale * 50);
        }

        static void SendToXMulti(string fileName, int count)
        {
            Task.WaitAll(
                SendToX(fileName, "AAAAAA", count)
                ,SendToX(fileName, "BBBBBB", count)
                ,SendToX(fileName, "CCCCCC", count)
            );
        }

        static async Task SendToX(string fileName, string sender, int count)
        {
            var filePath = Path.Combine(TESTFILEDIR, fileName);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Sender", sender);
                httpClient.DefaultRequestHeaders.Add("X-Recipient", "XXXXXX");

                var mongoUri = $"mongodb://localhost:27017";
                var client = new MongoDB.Driver.MongoClient(mongoUri);
                var database = client.GetDatabase("hawking-csi");
                var collection = database.GetCollection<BsonDocument>("forward-queue");
                var filter = Builders<BsonDocument>.Filter.Eq("X-Sender", sender);
                var sw = Stopwatch.StartNew();

                for (int i = 0; i < count; i++)
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, EXCHANGEURI);
                    var id = new Guid(i, 0, 0, BitConverter.GetBytes(DateTime.Now.ToBinary())).ToString();
                    request.Headers.Add("X-TrackingId", id);
                    using (var fs = File.OpenRead(filePath))
                    {
                        request.Content = new StreamContent(fs);
                        try
                        {

                            var result = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                            System.Console.WriteLine($"{sender}=>XXXXXX {fileName} {id} {i,-5} {result.StatusCode}");
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex.ToString());
                        }
                    }

                    if (sw.ElapsedMilliseconds > 1000)
                    {
                        while ((await collection.CountDocumentsAsync(filter)) > 1000)
                            Task.Delay(1000).Wait();
                        sw.Restart();
                    }
                }
            }
        }

        static async Task SendToY(string fileName, string sender, int count)
        {
            var filePath = Path.Combine(TESTFILEDIR, fileName);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Sender", sender);
                httpClient.DefaultRequestHeaders.Add("X-Recipient", "XXXXXX");

                var mongoUri = $"mongodb://localhost:27017";
                var client = new MongoDB.Driver.MongoClient(mongoUri);
                var database = client.GetDatabase("hawking-csi");
                var collection = database.GetCollection<BsonDocument>("forward-queue");
                var filter = Builders<BsonDocument>.Filter.Eq("X-Sender", sender);
                var sw = Stopwatch.StartNew();

                for (int i = 0; i < count; i++)
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, EXCHANGEURI);
                    var id = new Guid(i, 0, 0, BitConverter.GetBytes(DateTime.Now.ToBinary())).ToString();
                    request.Headers.Add("X-TrackingId", id);
                    using (var fs = File.OpenRead(filePath))
                    {
                        request.Content = new StreamContent(fs);
                        try
                        {

                            var result = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                            System.Console.WriteLine($"{sender}=>XXXXXX {fileName} {id} {i,-5} {result.StatusCode}");
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex.ToString());
                        }
                    }

                    if (sw.ElapsedMilliseconds > 1000)
                    {
                        while ((await collection.CountDocumentsAsync(filter)) > 1000)
                            Task.Delay(1000).Wait();
                        sw.Restart();
                    }
                }
            }
        }
    }
}
