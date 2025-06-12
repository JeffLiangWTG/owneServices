//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Text;
//using System.Threading;
//using CargoWise.eServices.USCustoms.Services;
//using DotNetMock.Dynamic;
//using IBM.WMQ;
//using Microsoft.Samples.SqlServer;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using CargoWise.eServices.USCustoms.Inbound;

//namespace CargoWise.eServices.USCustoms.Tests
//{
//    [TestClass]
//    public class PerformanceTests : GatewayServicesTests
//    {
//        [TestMethod]
//        public void TestProcessingLotsOfInbounds()
//        {
//            string client1Id = "client 1";
//            var builder = new StringBuilder();
//            var isFailure = AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfInbounds(client1Id, 100, new InboundConfigurationForTesting1(client1Id, "ZZD", "DZ")), new TimeSpan(0, 0, 0, 2, 500));
//            isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfInbounds(client1Id, 100, new InboundConfigurationForTestingSYD_VHMY_1(client1Id, "ZZD", "DZ")), new TimeSpan(0, 0, 0, 0, 20));
//            Assert.AreEqual<bool>(false, isFailure, builder.ToString());
//        }

//        bool AssertPerformanceResult(StringBuilder builder, string hostName, PerformanceResult result, TimeSpan expectedAverageTimePerMsg)
//        {
//            bool isFailure = result.AverageTimePerMsg > expectedAverageTimePerMsg;
//            builder.AppendLine(string.Format("{0}: {1}, {2}, Expected Avg/Msg {3}", isFailure ? "FAILED" : "PASSED", hostName, result.Message, expectedAverageTimePerMsg.ToString()));
//            return isFailure;
//        }

//        struct PerformanceResult
//        {
//            public string Message;
//            public TimeSpan AverageTimePerMsg;
//        }

//        PerformanceResult AssertProcessingLotsOfInbounds(string client1Id, int expectedNoOfMessages, IMQInboundConfiguration config)
//        {
//            PerformanceResult result;
//            var mqTest = GetNewMQTest(config);
//            mqTest.ClearQueue(config.QueueName);

//            var logger = new WindowsEventLogger();
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();

//                using (var transaction = connection.BeginTransaction())
//                {
//                    var inboundService = new eHubInboundService(connection, transaction, logger);
//                    inboundService.Run(true, connection, transaction);
//                }
//            }

//            Guid client1PK = Guid.NewGuid();
//            InserteHubTestClients(client1PK, client1Id);

//            InserteHubTestClients(usCustomsClientPK, eHubOutboxService.USCustomsClientId);
//            string message = "A3910SV9      10311001   110110001415                                00000054262" +
//                "B018888XJ5ER                                               54468                " +
//                "10A888891-01319900091-013199000            1    8         XJ5 7003469502891  IL " +
//                "40001HT00000000000000000400000000000000000000000000000100                       " +
//                "50 98201103                                                         HT103110N   " +
//                "51        0HT125472    00000000096DPC                                           " +
//                "E518888XJ5 700346950014KX02   INVALID HAITI HOPE VISA NBR              B00154438" +
//                "9000000000000000000000000 00000000000000000000000000000000000000005282          " +
//                "E908888XJ5 70034695   52402   TRANSACTION DATA REJECTED                B00154438" +
//                "9000000000000000000000000 00000000000000000000000000000000000000005282          " +
//                "E908888XJ5 70034695   52402   TRANSACTION DATA REJECTED                B00154438" +
//                "Y  8888XJ5ER00007".PadRight(80) +
//                "Z3910SV9      10311001   110110001417                                00000054262";

//            var queueManager = mqTest.QueueManager;
//            var queue = queueManager.AccessQueue(config.QueueName, MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
//            var options = new MQPutMessageOptions();
//            options.Options = MQC.MQPMO_SYNCPOINT;
//            for (int i = 0; i < expectedNoOfMessages; i++)
//            {
//                var queueMessage = new MQMessage();
//                queueMessage.Encoding = 546;
//                queueMessage.Format = MQC.MQFMT_STRING;
//                queueMessage.Persistence = MQC.MQPER_PERSISTENT;
//                queueMessage.CharacterSet = 437;
//                queueMessage.MessageType = MQC.MQMT_APPL_LAST;
//                queueMessage.WriteString(message);
//                queue.Put(queueMessage, options);
//            }
//            queue.Close();
//            mqTest.CommitAndDisconnectMQ();

//            long startCheckTime = 0;
//            long checkTime = 0;
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var loader = new InboundMessageLoader(connection, logger, new MQMessageRetriever(config, logger) { ShouldLog = false });
//                startCheckTime = DateTime.Now.Ticks;
//                loader.Run();
//                checkTime = DateTime.Now.Ticks;
//            }

//            int noOfMessagesReceived = 0;
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                using (SqlCommand cmd = new SqlCommand())
//                {
//                    cmd.Connection = connection;
//                    cmd.CommandText =
//    @"
//SELECT COUNT(*)
//FROM USCustomseHubInboundProcessingQueue WITH(NOLOCK)
//";
//                    noOfMessagesReceived = (int)cmd.ExecuteScalar();
//                }

//                var totalTime = checkTime - startCheckTime;
//                result.AverageTimePerMsg = new TimeSpan(totalTime / expectedNoOfMessages);
//                result.Message = string.Format("{0} out of {1} messages were received in {2}, Avg/Msg {3}", noOfMessagesReceived, expectedNoOfMessages, new TimeSpan(totalTime).ToString(), result.AverageTimePerMsg.ToString());
//            }
//            return result;
//        }

//        [TestMethod]
//        public void TestProcessingLotsOfOutbounds()
//        {
//            var builder = new StringBuilder();
//            IMQOutboundConfiguration config = new OutboundConfigurationForTesting1();
//            var expectedAverageTimePerMsg = new TimeSpan(0, 0, 0, 0, 100);
//            var isFailure = false;
//            //isFailure = AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 10, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 10, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 10, 25, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 25, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 25, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 25, 25, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 50, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 50, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 50, 25, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 75, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 75, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 75, 25, config), expectedAverageTimePerMsg);
//            isFailure = AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 100, 25, config), expectedAverageTimePerMsg);
//            isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 100, 50, config), expectedAverageTimePerMsg);
//            isFailure |= AssertPerformanceResult(builder, "ORD-VCMR-1", AssertProcessingLotsOfOutbounds(10, 100, 75, config), expectedAverageTimePerMsg);
//            builder.AppendLine();
//            config = new OutboundConfigurationForTestingSYD_VHMY_1();
//            expectedAverageTimePerMsg = new TimeSpan(0, 0, 0, 0, 5);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 10, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 10, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 10, 25, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 25, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 25, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 25, 25, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 50, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 50, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 50, 25, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 75, 75, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 75, 50, config), expectedAverageTimePerMsg);
//            //isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 75, 25, config), expectedAverageTimePerMsg);
//            isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 100, 25, config), expectedAverageTimePerMsg);
//            isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 100, 50, config), expectedAverageTimePerMsg);
//            isFailure |= AssertPerformanceResult(builder, "SYD-VHMY-1", AssertProcessingLotsOfOutbounds(10, 100, 75, config), expectedAverageTimePerMsg);
//            Assert.AreEqual<bool>(false, isFailure, builder.ToString());
//        }

//        PerformanceResult AssertProcessingLotsOfOutbounds(int noOfMessagesPerClient, int noOfClients, int noOfThreads, IMQOutboundConfiguration config)
//        {
//            PerformanceResult result;
//            CleanUpData();
//            try
//            {
//                var mqTest = GetNewMQTest(config);
//                InserteHubTestClients(usCustomsClientPK, eHubOutboxService.USCustomsClientId);

//                string messageBody = "B018888XJ5EI                                               29511                " +
//                            "10A888891-01319900091-013199000            1    8         XJ5 7000662801891  DC " +
//                            "20                         212904021909B00151194                 021909C001     " +
//                            "22            OB9384                              00000010PK         QF         " +
//                            "30                                  0               2030209             QF  808 " +
//                            "40001OM00006750000000000000                    0000060000                       " +
//                            "50 4906000000          000000012300KG                               OM021909N   " +
//                            "51                                                                              " +
//                            "60                                        XOAGRPRO5001TOR                       " +
//                            "62          49900141750                                                         " +
//                            "8949900000048500                                                                " +
//                            "90                      0                       0000004850000000675000          " +
//                            "Y  8888XJ5EI00011".PadRight(80);

//                var logger = new WindowsEventLogger();
//                using (var connection = GetSqlConnectionToeHub())
//                {
//                    connection.Open();
//                    var transaction = connection.BeginTransaction();

//                    try
//                    {
//                        OutboundMessageQueuer queuer = new OutboundMessageQueuer();
//                        for (int j = 1; j <= noOfClients; j++)
//                        {
//                            string clientId = "client " + j.ToString();
//                            Guid clientPK = Guid.NewGuid();
//                            InserteHubTestClients(clientPK, clientId, transaction);
//                            Guid envelopeTrackingId = Guid.NewGuid();
//                            string applicationCode = "US" + j.ToString();

//                            for (int i = 1; i <= noOfMessagesPerClient; i++)
//                            {
//                                var number = i.ToString().PadLeft(11, '0');

//                                InserteHubOutboxEnvelope(transaction, envelopeTrackingId, clientId);
//                                queuer.EnqueueMessage(transaction, clientId, envelopeTrackingId, applicationCode, i.ToString(),
//                                    GetUSMessageData(("A3902SV9CAREDI02190901" + applicationCode).PadRight(69) + number,
//                                    messageBody,
//                                    ("Z3902SV9CAREDI02190901" + applicationCode).PadRight(69) + number));
//                            }
//                        }
//                        transaction.Commit();
//                    }
//                    catch
//                    {
//                        transaction.Rollback();
//                        throw;
//                    }
//                }

//                var queue = mqTest.QueueManager.AccessQueue(config.QueueName, MQC.MQOO_INPUT_EXCLUSIVE + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE + MQC.MQGMO_CONVERT);
//                List<Thread> threads = new List<Thread>(noOfClients);
//                for (int i = 0; i < noOfThreads; i++)
//                {
//                    threads.Add(GetNewRunOutboundMessageProcessingServiceThread(config, logger));
//                }

//                NoOfThreadFinished = 0;
//                int expectedNoOfMessages = noOfMessagesPerClient * noOfClients;
//                var startCheckTime = DateTime.Now.Ticks;
//                var expectedTime = startCheckTime + new TimeSpan(0, 0, expectedNoOfMessages + 60).Ticks;
//                int noOfMessagesReceived = 0;
//                long checkTime = 0;
//                foreach (var thread in threads)
//                {
//                    thread.Start();
//                }
//                while (true)
//                {
//                    noOfMessagesReceived = queue.CurrentDepth;
//                    checkTime = DateTime.Now.Ticks;
//                    if (noOfMessagesReceived >= expectedNoOfMessages || checkTime > expectedTime || NoOfThreadFinished == noOfThreads)
//                    {
//                        break;
//                    }
//                }
//                queue.Close();
//                queue = null;
//                foreach (var thread in threads)
//                {
//                    if (thread.IsAlive)
//                    {
//                        thread.Join();
//                    }
//                }
//                mqTest.ClearQueue(config.QueueName);
//                mqTest.CommitAndDisconnectMQ();

//                var totalTime = checkTime - startCheckTime;
//                result.AverageTimePerMsg = new TimeSpan(totalTime / expectedNoOfMessages);
//                result.Message = string.Format("{0} Threads, {1} Clients, {2} out of {3} messages were received in {4}, Avg/Msg {5}", noOfThreads, noOfClients, noOfMessagesReceived, expectedNoOfMessages, new TimeSpan(totalTime).ToString(), result.AverageTimePerMsg.ToString());
//            }
//            finally
//            {
//                CleanUpData();
//            }
//            return result;
//        }

//        void CleanUpData()
//        {
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();

//                try
//                {
//                    using (SqlCommand cmd = new SqlCommand())
//                    {
//                        cmd.Connection = transaction.Connection;
//                        cmd.Transaction = transaction;
//                        cmd.CommandText =
//        @"
//if ((SELECT OBJECT_ID('tempdb..#TempPK')) IS NOT NULL)
//begin
//	drop table #TempPK
//end
//create table #TempPK (PK uniqueidentifier);
//insert into #TempPK (PK) select CC_PK from eHubClient with (nolock) where CC_ID like 'client %';
//delete from eHubOutboxEnvelope where OI_CC_Sender in (select PK from #TempPK);
//delete from eHubOutboxEnvelope where OI_CC_Recipient in (select PK from #TempPK);
//delete from eHubClient where CC_PK in (select PK from #TempPK);
//
//declare @conversationHandle uniqueidentifier
//select top 1 @conversationHandle = D1_DialogueHandle from eHubClientDialogue where D1_ClientPK not in (select CC_PK FROM eHubClient)
//while @@rowcount = 1
//begin
//	end conversation @conversationHandle with cleanup
//	select top 1 @conversationHandle = conversation_handle from sys.conversation_endpoints
//end
//
//delete from eHubClientDialogue where D1_ClientPK not in (select CC_PK FROM eHubClient)
//
//";
//                        cmd.ExecuteNonQuery();
//                    }

//                    transaction.Commit();
//                }
//                catch
//                {
//                    transaction.Rollback();
//                    throw;
//                }
//            }

//        }

//        static int NoOfThreadFinished = 0;

//        static Thread GetNewRunOutboundMessageProcessingServiceThread(IMQOutboundConfiguration config, WindowsEventLogger logger)
//        {
//            var processingThread = new Thread(new ThreadStart(delegate
//            {
//                using (var connection = GetSqlConnectionToeHub())
//                {
//                    var queueManagerProvider = new MQQueueManagerProvider();
//                    try
//                    {
//                        connection.Open();

//                        var mock = new DynamicMock<MQMessageSender>(queueManagerProvider, logger);
//                        mock.ExpectAndReturnAlways("GetNewConfiguration", config);
//                        //mock.Object.ShouldLog = true;

//                        var messageSender = mock.Object;
//                        var waitTimeOut = TimeSpan.FromSeconds(10);
//                        var service = new OutboundMessageProcessingService(connection, waitTimeOut, logger, messageSender);
//                        service.FetchSize = 1;

//                        // Run the message loop of the service
//                        service.Run(true, connection, null);
//                    }
//                    catch (ServiceException ex)
//                    {
//                        if (ex.Transaction != null)
//                        {
//                            ex.Transaction.Rollback();
//                        }

//                        throw ex;
//                    }
//                    finally
//                    {
//                        connection.Close();
//                        queueManagerProvider.ClossAllInactiveQueueManager();
//                    }
//                }
//                NoOfThreadFinished++;
//            }));
//            return processingThread;
//        }
//    }
//}
