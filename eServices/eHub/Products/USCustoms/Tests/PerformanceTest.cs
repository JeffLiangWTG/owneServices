//using System;
//using System.Data.SqlClient;
//using System.IO;
//using CargoWise.eServices.USCustoms.Integration.Extensions;
//using CargoWise.eServices.USCustoms.Services;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Data;

//namespace CargoWise.eServices.USCustoms.Tests
//{
//    [TestClass]
//    public class PerformanceTest
//    {
//        [TestMethod]
//        public void TestOutbound()
//        {
//            for (int i = 0; i < 1000; i++)
//            {
//                using (var connection = new SqlConnection("Data Source=localhost;Initial Catalog=eHubTransactions;Integrated Security=SSPI;"))
//                {
//                    connection.Open();
//                    var transaction = connection.BeginTransaction();

//                    Stream stream = new MemoryStream();
//                    var writer = new StreamWriter(stream);
//                    writer.Write(Body);
//                    writer.Flush();
//                    stream.SeekBegin();
//                    stream = stream.CompressAndEncode();
//                    var queuer = new OutboundMessageQueuer();
//                    try
//                    {
//                        stream.SeekBegin();
//                        queuer.EnqueueMessage(transaction, "HYEDUSPAV", Guid.NewGuid(), "USI", "EI", stream);
//                        transaction.Commit();
//                    }
//                    catch
//                    {
//                        transaction.Rollback();
//                    }
//                }
//            }
//        }

//        const string Body = "<USCustoms><Header><![CDATA[A    SV9      06031101                                               00000000017]]></Header><Body><![CDATA[B01    XJ5EI                                               17                   10A390113-14792700013-147927000                 8         XJ5 0000003001037  TX 20                         403901053111B00001003            001  053111I317     22            00112225533                         00000001PC                    30                                  0               2061011             AA      40001GT00000050000000000100                    0000000100                       50P4202921500                         000000010000KG                GT053111N   51                  369                                                         60                                        GTGUAEXP1CHI                          40002GT00000000000000000000                                                     50P9003110000                                                       GT053111N   51                                                                              60                                        GTGUAEXP1CHI                          90                      0                                  00000005000          Y      XJ5EI00013                                                               ]]></Body><Footer><![CDATA[Z    SV9      06031101                                               00000000017]]></Footer></USCustoms>";
//            //"A    SV9      06031101                                               00000000017B01    XJ5EI                                               17                   10A390113-14792700013-147927000                 8         XJ5 0000003001037  TX 20                         403901053111B00001003            001  053111I317     22            00112225533                         00000001PC                    30                                  0               2061011             AA      40001GT00000050000000000100                    0000000100                       50P4202921500                         000000010000KG                GT053111N   51                  369                                                         60                                        GTGUAEXP1CHI                          40002GT00000000000000000000                                                     50P9003110000                                                       GT053111N   51                                                                              60                                        GTGUAEXP1CHI                          90                      0                                  00000005000          Y      XJ5EI00013                                                               Z    SV9      06031101                                               00000000017";
//    }
//}
