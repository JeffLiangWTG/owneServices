using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Hawking.CSI.Apps.XXXXXX.Models;
using Hawking.CSI.Apps.YYYYYY.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hawking.Testing.TaskApps
{
    class Program
    {
        static void Main(string[] args)
        {
            // int limit = 10000;
            // int interval = 1;
            // int batch = 10;
            // Task.WaitAll(
            //     //Task.Run(() => TestSendToX("AAAAAA", interval, limit, 100)),
            //     //Task.Run(() => TestSendToX("BBBBBB", interval, limit, 100)),
            //     Task.Run(() => TestSendToY("AAAAAA", interval, batch, limit, 20000))//,
            //     // Task.Run(() => TestSendToY("BBBBBB", interval, limit, 100)),
            //     //Task.Run(() => TestSendToY("CCCCCC", interval, limit, 100))
            //     //Task.Run(() => TestReceive("XXXXXX", 100)),
            //     //Task.Run(() => TestReceive("ZZZZZZ", 100))
            // );                

            //TestSendToY("AAAAAA", 2000, 100, 10, 100);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 100);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 200);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 500);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 1000);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 2000);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 5000);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 10000);
            // TestSendToY("AAAAAA", 2000, 100, 30000, 20000);
            // TestSendToY("AAAAAA", 2000, 100, 20000, 50000);
            // TestSendToY("AAAAAA", 5000, 100, 20000, 100000);
            // TestSendToY("AAAAAA", 5000, 100, 20000, 200000);
            // TestSendToY("AAAAAA", 5000, 100, 15000, 500000);
            // TestSendToY("AAAAAA", 5000, 100, 10000, 1000000);

            // CreateSendToYFile("AAAAAA", 100);
            // CreateSendToYFile("AAAAAA", 200);
            // CreateSendToYFile("AAAAAA", 500);
            // CreateSendToYFile("AAAAAA", 1024);
            // CreateSendToYFile("AAAAAA", 2 * 1024);
            // CreateSendToYFile("AAAAAA", 5 * 1024);
            // CreateSendToYFile("AAAAAA", 10 * 1024);
            // CreateSendToYFile("AAAAAA", 20 * 1024);
            // CreateSendToYFile("AAAAAA", 50 * 1024);
            // CreateSendToYFile("AAAAAA", 100 * 1024);
            // CreateSendToYFile("AAAAAA", 200 * 1024);
            // CreateSendToYFile("AAAAAA", 500 * 1024);
            // CreateSendToYFile("AAAAAA", 1024 * 1024);
            // CreateSendToYFile("AAAAAA", 2 * 1024 * 1024);
            // CreateSendToYFile("AAAAAA", 5 * 1024 * 1024);
            // CreateSendToYFile("AAAAAA", 10 * 1024 * 1024);
            // CreateSendToYFile("AAAAAA", 20 * 1024 * 1024);
            // CreateSendToYFile("AAAAAA", 50 * 1024 * 1024);

            CreateSendToXFile("AAAAAA", 100, 100);
            CreateSendToXFile("AAAAAA", 200, 200);
            CreateSendToXFile("AAAAAA", 500, 500);
            CreateSendToXFile("AAAAAA", 1024, 1000);
            CreateSendToXFile("AAAAAA", 2 * 1024, 2000);
            CreateSendToXFile("AAAAAA", 5 * 1024, 5000);
            CreateSendToXFile("AAAAAA", 10 * 1024, 10000);
            CreateSendToXFile("AAAAAA", 20 * 1024, 20000);
            CreateSendToXFile("AAAAAA", 50 * 1024, 50000);
            CreateSendToXFile("AAAAAA", 100 * 1024, 100000);
            CreateSendToXFile("AAAAAA", 200 * 1024, 200000);
            CreateSendToXFile("AAAAAA", 500 * 1024, 500000);
            CreateSendToXFile("AAAAAA", 1024 * 1024, 1000000);
            CreateSendToXFile("AAAAAA", 2 * 1024 * 1024, 2000000);
            CreateSendToXFile("AAAAAA", 5 * 1024 * 1024, 5000000);
            CreateSendToXFile("AAAAAA", 10 * 1024 * 1024, 10000000);
            CreateSendToXFile("AAAAAA", 20 * 1024 * 1024, 20000000);
            CreateSendToXFile("AAAAAA", 50 * 1024 * 1024, 50000000);
        }

        private static void TestReceive(string recipient, int wait)
        {
            int counter = 0;
            using (var httpClient = new HttpClient())
            {
                while (true)
                {
                    var response = httpClient.GetAsync($"http://localhost:6001/v1/message/{recipient}").Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var msg = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        var result = httpClient.DeleteAsync($"http://localhost:6001/v1/message/{msg["id"]}").Result;
                        System.Console.WriteLine($"{recipient}<={msg["message"]["headers"]["Sender"]} {counter++,-5} {result.StatusCode}");
                    }
                    else
                    {
                        Task.Delay(wait).Wait();
                    }
                }
            }
        }

        private static void TestSendToX(string sender, int interval, int limit, int size)
        {

            var bodyXml1 = "<MessageX1><TrackingId>";
            var bodyXml2 = "</TrackingId><CreatedUtc>";
            var bodyXml3 = "</CreatedUtc><Message>" + String.Concat(Enumerable.Repeat($"<string>{sender}</string>", size)) + "</Message></MessageX1>";

            var message1 = @"{ ""Version"": 1, ""TrackingId"": """;
            var message2 = @""", ""PreceedingTrackingIds"": [], ""Headers"": { ""Sender"": """ + sender 
                            + @""", ""Recipient"": ""XXXXXX"", ""Schema"": ""MessageX1"" }, ""BodyHash"": """", ""Body"": """;
            var message3 = @""" }";

            using (var httpClient = new HttpClient())
            {
                for (int counter = 0; counter < limit; counter++)
                {
                    var msgId = Guid.NewGuid();

                    var bodyXml = new StringBuilder(bodyXml1)
                        .Append(msgId)
                        .Append(bodyXml2)
                        .Append(DateTime.UtcNow.ToString("s"))
                        .Append(bodyXml3).ToString();
                    var message = new StringBuilder(message1)
                        .Append(msgId)
                        .Append(message2)
                        .Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(bodyXml)))
                        .Append(message3).ToString();

                    try
                    {
                        var result = httpClient.PostAsync("http://localhost:6001/v1/message", new StringContent(message, Encoding.UTF8, "application/json")).Result;
                        System.Console.WriteLine($"{sender}=>XXXXXX {counter,-5} {result.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.ToString());
                    }

                    Task.Delay(TimeSpan.FromMilliseconds(interval)).Wait();
                }
            }
        }

        private static void TestSendToY(string sender, int interval, int batch, int limit, int size)
        {
            var mongoUri = $"mongodb://localhost:27017";
            var client = new MongoDB.Driver.MongoClient(mongoUri);
            var database = client.GetDatabase("hawking-csi");
            var collection = database.GetCollection<BsonDocument>("forward-queue");

            var wait = TimeSpan.FromMilliseconds(interval);
            var count = (int)((size - 100)/9);
            var message = new MessageY1
            {
                TrackingId = Guid.Empty,
                CreatedUtc = DateTime.UtcNow,
                Message = Enumerable.Repeat(sender, count).ToList()
            };
            var msgBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            var msgStream = new MemoryStream(msgBytes);

            using (var httpClient = new HttpClient())
            {
                for (int counter = 0; counter < limit; counter++)
                {
                    msgStream.Position = 0;
                    var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:7004/v1/messages/");
                    request.Headers.Add("X-Sender", sender);
                    request.Headers.Add("X-Recipient", "YYYYYY");
                    request.Headers.Add("X-TrackingId", Guid.NewGuid().ToString());
                    request.Content = new StreamContent(msgStream);

                    try
                    {
                        var result = httpClient.SendAsync(request).Result;
                        System.Console.WriteLine($"{sender}=>YYYYYY {counter,-5} {msgStream.Length,10} {result.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.ToString());
                    }

                    if (counter % batch == 0)
                    {
                        while(collection.Count(Builders<BsonDocument>.Filter.Empty) > 1000)
                            Task.Delay(wait).Wait();
                    }
                }
            }
        }

        private static void CreateSendToXFile(string sender, int size, int label)
        {
            var count = size < 148 ? 0 : (int)((size - 148)/23);
            var message = new MessageX1
            {
                TrackingId = Guid.Empty,
                CreatedUtc = DateTime.UtcNow,
                Message = Enumerable.Repeat(sender, count).ToList()
            };
            var serializer = new XmlSerializer(typeof(MessageX1));
            var ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var fs = File.Open($@"D:\eServices\HawkingPOC\CSI\Testing\TestFiles\MessageX1_{label}_{sender}.xml", FileMode.Create);
            var xw = XmlWriter.Create(fs, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true });
            serializer.Serialize(xw, message, ns);
            fs.Close();

            count = size < 162 ? 0 : (int)((size - 162)/29);
            message = new MessageX1
            {
                TrackingId = Guid.Empty,
                CreatedUtc = DateTime.UtcNow,
                Message = Enumerable.Repeat(sender, count).ToList()
            };
            fs = File.Open($@"D:\eServices\HawkingPOC\CSI\Testing\TestFiles\MessageX1_Indented_{label}_{sender}.xml", FileMode.Create);
            xw = XmlWriter.Create(fs, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true });
            serializer.Serialize(xw, message, ns);
            fs.Close();
        }

        private static void CreateSendToYFile(string sender, int size)
        {
            var count = (int)((size - 100)/9);
            var message = new MessageY1
            {
                TrackingId = Guid.Empty,
                CreatedUtc = DateTime.UtcNow,
                Message = Enumerable.Repeat(sender, count).ToList()
            };
            File.WriteAllText($@"C:\eServices\HawkingPOC\CSI\Testing\TestFiles\MessageY1_{size}_{sender}.json", JsonConvert.SerializeObject(message));

            count = size < 131 ? 0 : (int)((size - 115)/15);
            message = new MessageY1
            {
                TrackingId = Guid.Empty,
                CreatedUtc = DateTime.UtcNow,
                Message = Enumerable.Repeat(sender, count).ToList()
            };
            File.WriteAllText($@"C:\eServices\HawkingPOC\CSI\Testing\TestFiles\MessageY1_Indented_{size}_{sender}.json", JsonConvert.SerializeObject(message, Newtonsoft.Json.Formatting.Indented));
        }
    }
}
