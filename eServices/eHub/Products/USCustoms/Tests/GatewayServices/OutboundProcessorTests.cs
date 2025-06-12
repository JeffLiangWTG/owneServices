//using System;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Text;
//using System.Threading;
//using CargoWise.eServices.USCustoms.Services;
//using DotNetMock.Dynamic;
//using Microsoft.Samples.SqlServer;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace CargoWise.eServices.USCustoms.Tests
//{
//    /// <summary>
//    /// Summary description for OutboundProcessorTests
//    /// </summary>
//    [TestClass]
//    public class OutboundProcessorTests : GatewayServicesTests
//    {
//        public OutboundProcessorTests()
//        {
//            //
//            // TODO: Add constructor logic here
//            //
//        }

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get;
//            set;
//        }

//        #region Additional test attributes
//        //
//        // You can use the following additional attributes as you write your tests:
//        //
//        // Use ClassInitialize to run code before running the first test in the class
//        // [ClassInitialize()]
//        // public static void MyClassInitialize(TestContext testContext) { }
//        //
//        // Use ClassCleanup to run code after all tests in a class have run
//        // [ClassCleanup()]
//        // public static void MyClassCleanup() { }
//        //
//        // Use TestInitialize to run code before running each test 
//        // [TestInitialize()]
//        // public void MyTestInitialize() { }
//        //
//        // Use TestCleanup to run code after each test has run
//        // [TestCleanup()]
//        // public void MyTestCleanup() { }
//        //
//        #endregion

//        [TestMethod]
//        public void TestMQ()
//        {
//            var config = new OutboundConfigurationForTesting1();
//            var mqTest = GetNewMQTest(config);

//            string sentResult = mqTest.WriteMsg("USLocal", "Testing 123");
//            Assert.AreEqual<string>("Message sent to the queue successfully", sentResult);

//            string readResult = mqTest.ReadMsg(config.QueueName, true);
//            Assert.AreEqual<string>("Testing 123", readResult);
//        }

//        [TestMethod]
//        public void TestGetClientDialogueUsingClientId()
//        {
//            ClearTestTables();

//            const string client1Id = "client 1";
//            InserteHubTestClients(Guid.NewGuid(), client1Id);

//            const string client2Id = "client 2";
//            InserteHubTestClients(Guid.NewGuid(), client2Id);

//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();
//                var manager = new DialogueManager();

//                var service = new eHubOutboxService(connection, transaction);

//                Conversation client1dialogue1 = manager.GetClientDialogue(client1Id, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);
//                Conversation client2dialogue1 = manager.GetClientDialogue(client2Id, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);
//                Conversation client1dialogue2 = manager.GetClientDialogue(client1Id, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);
//                Conversation client2dialogue2 = manager.GetClientDialogue(client2Id, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);

//                var service2 = new OutboundMessageProcessingService(connection, transaction, TimeSpan.FromSeconds(20), new DummyLogger(), new DummyMessageSender());
//                Conversation service2client1dialogue1 = manager.GetClientDialogue(client1Id, service2, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);

//                try
//                {
//                    Assert.AreEqual<Guid>(client1dialogue1.Handle, client1dialogue2.Handle);
//                    Assert.AreEqual<Guid>(client2dialogue1.Handle, client2dialogue2.Handle);
//                    Assert.AreNotEqual<Guid>(client1dialogue1.Handle, client2dialogue1.Handle);
//                    Assert.AreNotEqual<Guid>(client1dialogue1.Handle, service2client1dialogue1.Handle);
//                }
//                finally
//                {
//                    transaction.Rollback();
//                }
//            }
//        }

//        [TestMethod]
//        public void TestGetClientDialogue()
//        {
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();
//                var manager = new DialogueManager();

//                Guid client1PK = Guid.NewGuid();
//                Guid client2PK = Guid.NewGuid();

//                var service = new eHubOutboxService(connection, transaction);

//                Conversation client1dialogue1 = manager.GetClientDialogue(client1PK, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);
//                Conversation client2dialogue1 = manager.GetClientDialogue(client2PK, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);
//                Conversation client1dialogue2 = manager.GetClientDialogue(client1PK, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);
//                Conversation client2dialogue2 = manager.GetClientDialogue(client2PK, service, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);

//                var service2 = new OutboundMessageProcessingService(connection, transaction, TimeSpan.FromSeconds(20), new DummyLogger(), new DummyMessageSender());
//                Conversation service2client1dialogue1 = manager.GetClientDialogue(client1PK, service2, OutboundMessageProcessingService.ServiceName, OutboundMessageProcessingService.ContractName, connection, transaction);

//                try
//                {
//                    Assert.AreEqual<Guid>(client1dialogue1.Handle, client1dialogue2.Handle);
//                    Assert.AreEqual<Guid>(client2dialogue1.Handle, client2dialogue2.Handle);
//                    Assert.AreNotEqual<Guid>(client1dialogue1.Handle, client2dialogue1.Handle);
//                    Assert.AreNotEqual<Guid>(client1dialogue1.Handle, service2client1dialogue1.Handle);
//                }
//                finally
//                {
//                    transaction.Rollback();
//                }
//            }
//        }

//        [TestMethod]
//        public void TestOutboundService()
//        {
//            var config1 = new OutboundConfigurationForTesting1();
//            var config2 = new OutboundConfigurationForTesting2();
//            var config3 = new OutboundConfigurationForTesting3();
//            var mqTest1 = GetNewMQTest(config1);
//            var mqTest2 = GetNewMQTest(config2);
//            var mqTest3 = GetNewMQTest(config3);

//            string client1Id = "client 1";
//            Guid client1PK = Guid.NewGuid();
//            InserteHubTestClients(client1PK, client1Id);

//            string client2Id = "client 2";
//            Guid client2PK = Guid.NewGuid();
//            InserteHubTestClients(client2PK, client2Id);

//            string client3Id = "client 3";
//            Guid client3PK = Guid.NewGuid();
//            InserteHubTestClients(client3PK, client3Id);

//            InserteHubTestClients(usCustomsClientPK, eHubOutboxService.USCustomsClientId);

//            Guid envelopeTrackingId1 = Guid.NewGuid();
//            Guid envelopeTrackingId2 = Guid.NewGuid();
//            Guid envelopeTrackingId3 = Guid.NewGuid();
//            string messageBody = "B018888XJ5EI                                               29511                " +
//                        "10A888891-01319900091-013199000            1    8         XJ5 7000662801891  DC " +
//                        "20                         212904021909B00151194                 021909C001     " +
//                        "22            OB9384                              00000010PK         QF         " +
//                        "30                                  0               2030209             QF  808 " +
//                        "40001OM00006750000000000000                    0000060000                       " +
//                        "50 4906000000          000000012300KG                               OM021909N   " +
//                        "51                                                                              " +
//                        "60                                        XOAGRPRO5001TOR                       " +
//                        "62          49900141750                                                         " +
//                        "8949900000048500                                                                " +
//                        "90                      0                       0000004850000000675000          " +
//                        "Y  8888XJ5EI00011".PadRight(80);

//            var logger = new DummyLogger();
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();

//                try
//                {
//                    OutboundMessageQueuer queuer = new OutboundMessageQueuer();

//                    InserteHubOutboxEnvelope(transaction, envelopeTrackingId1, client1Id);
//                    queuer.EnqueueMessage(transaction, client1Id, envelopeTrackingId1, "US1", "E1",
//                        GetUSMessageData("A3902SV9CAREDI02190901                                               00000029517",
//                        messageBody,
//                        "Z3902SV9CAREDI02190901                                               00000029517"));

//                    InserteHubOutboxEnvelope(transaction, envelopeTrackingId2, client2Id);
//                    queuer.EnqueueMessage(transaction, client2Id, envelopeTrackingId2, "US2", "E2",
//                        GetUSMessageData("A3902SV9CAREDI02190901                                               00000029518",
//                        messageBody,
//                        "Z3902SV9CAREDI02190901                                               00000029518"));

//                    InserteHubOutboxEnvelope(transaction, envelopeTrackingId3, client3Id);
//                    queuer.EnqueueMessage(transaction, client3Id, envelopeTrackingId3, "US3", "E3",
//                        GetUSMessageData("A3902SV9CAREDI02190901                                               00000029519",
//                        messageBody,
//                        "Z3902SV9CAREDI02190901                                               00000029519"));

//                    transaction.Commit();
//                }
//                catch
//                {
//                    transaction.Rollback();
//                    throw;
//                }
//            }

//            var queueManagerProvider = new MQQueueManagerProvider();
//            var mock = new DynamicMock<MQMessageSender>(queueManagerProvider, logger);
//            mock.ExpectAndReturn("GetNewConfiguration", config1);
//            mock.ExpectAndReturn("GetNewConfiguration", config2);
//            mock.ExpectAndReturn("GetNewConfiguration", config3);
//            try
//            {
//                messageSenderOverride = mock.Object;
//                var processingThread = new Thread(new ThreadStart(RunOutboundMessageProcessingService));
//                processingThread.Start();

//                processingThread.Join();
//            }
//            finally
//            {
//                messageSenderOverride = null;
//            }

//            Thread.Sleep(1000);

//            string readResult = mqTest1.ReadMsg(config1.QueueName, false);
//            Assert.AreEqual<string>("A8888DKDPASSSD02190901                                               00000029517" +
//                messageBody +
//                "Z8888DKDPASSSD02190901                                               00000029517", readResult);

//            readResult = mqTest2.ReadMsg(config2.QueueName, false);
//            Assert.AreEqual<string>("A7777DEEDGWSOE02190901                                               00000029518" +
//                messageBody +
//                "Z7777DEEDGWSOE02190901                                               00000029518", readResult);

//            readResult = mqTest3.ReadMsg(config3.QueueName, false);
//            Assert.AreEqual<string>("A6666ABCDEFGHI02190901                                               00000029519" +
//                messageBody +
//                "Z6666ABCDEFGHI02190901                                               00000029519", readResult);

//            RuneHubOutboxService();

//            AssertEnvelopStatus(envelopeTrackingId1, MQMessageSender.ResponseStatus.Pass);
//            AssertEnvelopStatus(envelopeTrackingId2, MQMessageSender.ResponseStatus.Pass);
//            AssertEnvelopStatus(envelopeTrackingId3, MQMessageSender.ResponseStatus.Pass);
//        }

//        [TestMethod]
//        public void TestOutboundService_GreaterThan4MBMessage()
//        {
//            var config = new OutboundConfigurationForTesting1();
//            var mqTest = GetNewMQTest(config);

//            string client1Id = "client 1";
//            Guid client1PK = Guid.NewGuid();
//            InserteHubTestClients(client1PK, client1Id);
//            InserteHubTestClients(usCustomsClientPK, eHubOutboxService.USCustomsClientId);

//            Guid envelopeTrackingId1 = Guid.NewGuid();
//            string messageBody = "B018888XJ5EI                                               29511                " +
//                        "10A888891-01319900091-013199000            1    8         XJ5 7000662801891  DC " +
//                        "20                         212904021909B00151194                 021909C001     " +
//                        "22            OB9384                              00000010PK         QF         " +
//                        "30                                  0               2030209             QF  808 " +
//                        "40001OM00006750000000000000                    0000060000                       " +
//                        "50 4906000000          000000012300KG                               OM021909N   " +
//                        "51                                                                              " +
//                        "60                                        XOAGRPRO5001TOR                       " +
//                        "62          49900141750                                                         " +
//                        "8949900000048500                                                                " +
//                        "90                      0                       0000004850000000675000          " +
//                        "Y  8888XJ5EI00011".PadRight(80);
//            int remainder;
//            int multiple = Math.DivRem(4194240 - 80, messageBody.Length, out remainder);
//            string messageBodyPart1 = GetCreateMessageData(messageBody, multiple, messageBody.Substring(0, remainder)); // the length of messageBodyPart1 plus A-Block's length should equal = 4194240;
//            string messageBodyPart2 = messageBody.Substring(remainder) + GetCreateMessageData(messageBody, 40, "");
//            int numberOfBlocks = (multiple + 41) * (messageBody.Length / 80);
//            string userData = numberOfBlocks.ToString().PadLeft(11, '0');
//            var logger = new DummyLogger();
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();

//                try
//                {
//                    OutboundMessageQueuer queuer = new OutboundMessageQueuer();

//                    InserteHubOutboxEnvelope(transaction, envelopeTrackingId1, client1Id);
//                    queuer.EnqueueMessage(transaction, client1Id, envelopeTrackingId1, "USI", "EI",
//                        GetUSMessageData("A3902SV9CAREDI02190901                                               " + userData,
//                        messageBodyPart1 + messageBodyPart2,
//                        "Z3902SV9CAREDI02190901                                               " + userData));
//                    transaction.Commit();
//                }
//                catch
//                {
//                    transaction.Rollback();
//                    throw;
//                }
//            }

//            var queueManagerProvider = new MQQueueManagerProvider();
//            var mock = new DynamicMock<MQMessageSender>(queueManagerProvider, logger);
//            mock.ExpectAndReturnAlways("GetNewConfiguration", config);
//            try
//            {
//                messageSenderOverride = mock.Object;
//                var processingThread = new Thread(new ThreadStart(RunOutboundMessageProcessingService));
//                processingThread.Start();

//                processingThread.Join();
//            }
//            finally
//            {
//                messageSenderOverride = null;
//            }

//            Thread.Sleep(1000);

//            string readResult = mqTest.ReadMsg(config.QueueName, false);
//            Assert.AreEqual<int>(4194240, readResult.Length);
//            Assert.AreEqual<string>("A8888DKDPASSSD02190901                                               " + userData +
//                messageBodyPart1, readResult);
//            readResult = mqTest.ReadMsg(config.QueueName, false);
//            Assert.AreEqual<string>(messageBodyPart2 + "Z8888DKDPASSSD02190901                                               " + userData, readResult);

//            RuneHubOutboxService();

//            AssertEnvelopStatus(envelopeTrackingId1, MQMessageSender.ResponseStatus.Pass);
//        }

//        string GetCreateMessageData(string data, int multiple, string extraData)
//        {
//            var builder = new StringBuilder();
//            for (int i = 0; i < multiple; i++)
//            {
//                builder.Append(data);
//            }
//            builder.Append(extraData);
//            return builder.ToString();
//        }

//        [TestMethod]
//        public void TestOutboundService_InvalidFormat()
//        {
//            var config = new OutboundConfigurationForTesting1();
//            var mqTest = GetNewMQTest(config);

//            string client1Id = "client 1";
//            Guid client1PK = Guid.NewGuid();
//            InserteHubTestClients(client1PK, client1Id);
//            InserteHubTestClients(usCustomsClientPK, eHubOutboxService.USCustomsClientId);

//            Guid envelopeTrackingId1 = Guid.NewGuid();
//            string messageBody = "B018888XJ5EI                                               29511                " +
//                        "10A888891-01319900091-013199000            1    8         XJ5 7000662801891  DC " +
//                        "20                         212904021909B00151194                 021909C001     " +
//                        "22            OB9384                              00000010PK         QF         " +
//                        "30                                  0               2030209             QF  808 " +
//                        "40001OM00006750000000000000                    0000060000                       " +
//                        "50 4906000000          000000012300KG                               OM021909N   " +
//                        "51                                                                              " +
//                        "60                                        XOAGRPRO5001TOR                       " +
//                        "62          49900141750                                                         " +
//                        "8949900000048500                                                                " +
//                        "90                      0                       0000004850000000675000          " +
//                        "Y  8888XJ5EI00011".PadRight(80);

//            var logger = new DummyLogger();
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();

//                try
//                {
//                    OutboundMessageQueuer queuer = new OutboundMessageQueuer();

//                    InserteHubOutboxEnvelope(transaction, envelopeTrackingId1, client1Id);
//                    queuer.EnqueueMessage(transaction, client1Id, envelopeTrackingId1, "USI", "EI",
//                        GetUSMessageData("",
//                        messageBody,
//                        "Z3902SV9CAREDI02190901                                               00000029517".PadRight(80)));

//                    transaction.Commit();
//                }
//                catch
//                {
//                    transaction.Rollback();
//                    throw;
//                }
//            }

//            var queueManagerProvider = new MQQueueManagerProvider();
//            var mock = new DynamicMock<MQMessageSender>(queueManagerProvider, logger);
//            mock.ExpectAndReturnAlways("GetNewConfiguration", config);
//            try
//            {
//                messageSenderOverride = mock.Object;
//                var processingThread = new Thread(new ThreadStart(RunOutboundMessageProcessingService));
//                processingThread.Start();

//                processingThread.Join();
//            }
//            finally
//            {
//                messageSenderOverride = null;
//            }

//            Thread.Sleep(1000);

//            string readResult = mqTest.ReadMsg(config.QueueName, false);
//            Assert.AreEqual<string>("", readResult);

//            RuneHubOutboxService();

//            AssertEnvelopStatus(envelopeTrackingId1, MQMessageSender.ResponseStatus.Fail);
//        }

//        [TestMethod]
//        public void TestOutboundServiceSendOrder_UsingDummyMessageSender()
//        {
//            ClearTestTables();

//            try
//            {
//                const string client1Id = "client 1";
//                Guid client1PK = Guid.NewGuid();
//                InserteHubTestClients(client1PK, client1Id);

//                const string client2Id = "client 2";
//                Guid client2PK = Guid.NewGuid();
//                InserteHubTestClients(client2PK, client2Id);

//                const string client3Id = "client 3";
//                Guid client3PK = Guid.NewGuid();
//                InserteHubTestClients(client3PK, client3Id);

//                InserteHubTestClients(usCustomsClientPK, eHubOutboxService.USCustomsClientId);

//                Guid envelopeTrackingId1 = Guid.NewGuid();
//                Guid envelopeTrackingId2 = Guid.NewGuid();
//                Guid envelopeTrackingId3 = Guid.NewGuid();
//                Guid envelopeTrackingId4 = Guid.NewGuid();
//                Guid envelopeTrackingId5 = Guid.NewGuid();
//                Guid envelopeTrackingId6 = Guid.NewGuid();
//                Guid envelopeTrackingId7 = Guid.NewGuid();
//                Guid envelopeTrackingId8 = Guid.NewGuid();
//                Guid envelopeTrackingId9 = Guid.NewGuid();

//                //Sending in order for each client, but interlaced.

//                using (var connection = GetSqlConnectionToeHub())
//                {
//                    connection.Open();
//                    var transaction = connection.BeginTransaction();

//                    try
//                    {
//                        OutboundMessageQueuer queuer = new OutboundMessageQueuer();

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId1, client1Id);
//                        queuer.EnqueueMessage(transaction, client1Id, envelopeTrackingId1, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 1, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId2, client1Id);
//                        queuer.EnqueueMessage(transaction, client1Id, envelopeTrackingId2, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 2, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId3, client2Id);
//                        queuer.EnqueueMessage(transaction, client2Id, envelopeTrackingId3, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 1, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId4, client3Id);
//                        queuer.EnqueueMessage(transaction, client3Id, envelopeTrackingId4, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 1, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId5, client3Id);
//                        queuer.EnqueueMessage(transaction, client3Id, envelopeTrackingId5, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 2, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId6, client2Id);
//                        queuer.EnqueueMessage(transaction, client2Id, envelopeTrackingId6, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 2, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId7, client2Id);
//                        queuer.EnqueueMessage(transaction, client2Id, envelopeTrackingId7, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 3, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId8, client1Id);
//                        queuer.EnqueueMessage(transaction, client1Id, envelopeTrackingId8, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 3, part 1, part 2, part 3</Data>")));

//                        InserteHubOutboxEnvelope(transaction, envelopeTrackingId9, client3Id);
//                        queuer.EnqueueMessage(transaction, client3Id, envelopeTrackingId9, "", "", new MemoryStream(Encoding.ASCII.GetBytes("<Data>message 3, part 1, part 2, part 3</Data>")));

//                        transaction.Commit();
//                    }
//                    catch
//                    {
//                        transaction.Rollback();
//                        throw;
//                    }
//                }

//                // Run 2 services in parrallel to pretend multiple activations in database.
//                Thread processingThread1 = new Thread(new ThreadStart(RunOutboundMessageProcessingService));
//                processingThread1.Start();

//                Thread processingThread2 = new Thread(new ThreadStart(RunOutboundMessageProcessingService));
//                processingThread2.Start();

//                processingThread1.Join();
//                processingThread2.Join();

//                DateTime startTime = DateTime.Now;

//                while (true)
//                {
//                    Thread.Sleep(1000);
//                    int rowCount = GetDummyMQRowCount();

//                    if (rowCount == 27)
//                    {
//                        break;
//                    }
//                    else if ((DateTime.Now - startTime).TotalSeconds > 15)
//                    {
//                        Assert.Fail("time out while waiting for DummyMQRows, current count is: " + rowCount);
//                    }
//                }

//                //expecting message order per client is preserved.
//                string expectedMQContent = @"message 1  part 1
//message 1  part 2
//message 1  part 3
//message 2  part 1
//message 2  part 2
//message 2  part 3
//message 3  part 1
//message 3  part 2
//message 3  part 3
//";
//                Assert.AreEqual<string>(expectedMQContent, GetDummyMQContent("client 1"));
//                Assert.AreEqual<string>(expectedMQContent, GetDummyMQContent("client 2"));
//                Assert.AreEqual<string>(expectedMQContent, GetDummyMQContent("client 3"));


//                RuneHubOutboxService();

//                AssertEnvelopStatus(envelopeTrackingId1, MQMessageSender.ResponseStatus.Pass);

//            }
//            finally
//            {
//                ClearTestTables();
//            }
//        }

//        void AssertEnvelopStatus(Guid envelopeTrackingId, short expectedStatus)
//        {
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();

//                using (var cmd = new SqlCommand())
//                {
//                    cmd.Connection = connection;
//                    cmd.CommandText = "SELECT TOP 1 OI_Status FROM eHubOutboxEnvelope WHERE OI_InternalTrackingID = @InternalTrackingID";

//                    var internalTrackingIDParam = cmd.Parameters.Add("@InternalTrackingID", SqlDbType.VarChar);
//                    internalTrackingIDParam.Direction = ParameterDirection.Input;
//                    internalTrackingIDParam.Value = envelopeTrackingId.ToString();

//                    short actualStatus = (short)cmd.ExecuteScalar();
//                    Assert.AreEqual<string>(expectedStatus.ToString(), actualStatus.ToString(), "OI_Status");
//                }
//            }
//        }

//        static void RuneHubOutboxService()
//        {
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();

//                var logger = loggerOverride ?? new DummyLogger();
//                var waitTimeOut = TimeSpan.FromSeconds(10);

//                try
//                {
//                    Service service = new eHubOutboxService(connection);
//                    service.FetchSize = 1;

//                    // Run the message loop of the service
//                    service.Run(true, connection, null);
//                }
//                catch (ServiceException ex)
//                {
//                    if (ex.Transaction != null)
//                    {
//                        ex.Transaction.Rollback();
//                    }

//                    throw ex;
//                }
//                finally
//                {
//                    if (connection != null)
//                    {
//                        connection.Close();
//                    }
//                }
//            }
//        }

//        int GetDummyMQRowCount()
//        {
//            int result = 0;

//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();

//                using (SqlCommand cmd = new SqlCommand())
//                {
//                    cmd.Connection = connection;
//                    cmd.CommandText = @"select count(*) from DummyUSCustomsMQ";
//                    result = (int)cmd.ExecuteScalar();
//                }
//            }

//            return result;
//        }

//        string GetDummyMQContent(string clientId)
//        {
//            StringBuilder builder = new StringBuilder();

//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();

//                using (SqlCommand cmd = new SqlCommand())
//                {
//                    cmd.Connection = connection;
//                    cmd.CommandText = @"select DD_Message, DD_Part from DummyUSCustomsMQ where DD_Client = @clientId order by DD_CreateTime ASC";
//                    cmd.Parameters.Add("@clientId", SqlDbType.VarChar).Value = clientId;

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            builder.Append(reader.GetString(0));
//                            builder.Append(" ");
//                            builder.AppendLine(reader.GetString(1));
//                        }
//                    }
//                }
//            }

//            return builder.ToString();
//        }

//        void ClearTestTables()
//        {
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();

//                try
//                {
//                    using (SqlCommand cmd = new SqlCommand())
//                    {
//                        cmd.Connection = connection;
//                        cmd.Transaction = transaction;
//                        cmd.CommandText =
//@"delete eHubOutboxEnvelope;
//delete eHubMessage;
//
//IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DummyUSCustomsMQ]') AND type in (N'U'))
//BEGIN
//	DELETE DummyUSCustomsMQ
//END
//ELSE BEGIN
//	CREATE TABLE [dbo].[DummyUSCustomsMQ](
//		[DD_PK] [uniqueidentifier] NOT NULL,
//		[DD_Client] [varchar](50) NOT NULL,
//		[DD_ApplicationCode] [char](3) NOT NULL,
//		[DD_InterchangeType] [char](3) NOT NULL,
//		[DD_Message] [varchar](50) NOT NULL,
//		[DD_Part] [varchar](50) NOT NULL,
//		[DD_CreateTime] [datetime] NOT NULL,
//	 CONSTRAINT [PK_DummyUSCustomsMQ] PRIMARY KEY CLUSTERED 
//	(
//		[DD_PK] ASC
//	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
//	) ON [PRIMARY];
//END
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
//    }
//}