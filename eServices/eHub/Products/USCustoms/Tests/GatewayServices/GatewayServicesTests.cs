//using System;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Text;
//using System.Web;
//using CargoWise.eServices.USCustoms.Services;
//using Microsoft.Samples.SqlServer;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace CargoWise.eServices.USCustoms.Tests
//{
//    [TestClass]
//    public abstract class GatewayServicesTests
//    {
//        protected readonly Guid usCustomsClientPK = new Guid("8750AD2B-B89A-4700-AB87-02AD4C32A29B");

//        protected void InserteHubOutboxEnvelope(SqlTransaction transaction, Guid trackingId, string clientId)
//        {
//            using (var cmd = new SqlCommand())
//            {
//                cmd.Connection = transaction.Connection;
//                cmd.Transaction = transaction;
//                cmd.CommandType = CommandType.StoredProcedure;
//                cmd.CommandText = "InsertOutboxEnvelope";

//                var senderIdParam = cmd.Parameters.Add("@SenderID", SqlDbType.VarChar);
//                senderIdParam.Direction = ParameterDirection.Input;
//                senderIdParam.Value = clientId;

//                var recipientIdParam = cmd.Parameters.Add("@RecipientID", SqlDbType.VarChar);
//                recipientIdParam.Direction = ParameterDirection.Input;
//                recipientIdParam.Value = eHubOutboxService.USCustomsClientId;

//                var messageTrackingIDParam = cmd.Parameters.Add("@MessageTrackingID", SqlDbType.VarChar);
//                messageTrackingIDParam.Direction = ParameterDirection.Input;
//                messageTrackingIDParam.Value = trackingId.ToString();

//                var internalTrackingIDParam = cmd.Parameters.Add("@InternalTrackingID", SqlDbType.VarChar);
//                internalTrackingIDParam.Direction = ParameterDirection.Input;
//                internalTrackingIDParam.Value = trackingId.ToString();

//                var envelopeTrackingIDParam = cmd.Parameters.Add("@EnvelopeTrackingID", SqlDbType.VarChar);
//                envelopeTrackingIDParam.Direction = ParameterDirection.Input;
//                envelopeTrackingIDParam.Value = trackingId.ToString();

//                var isBatchedParam = cmd.Parameters.Add("@IsBatch", SqlDbType.Bit);
//                isBatchedParam.Direction = ParameterDirection.Input;
//                isBatchedParam.Value = false;

//                var statusParam = cmd.Parameters.Add("@Status", SqlDbType.SmallInt);
//                statusParam.Direction = ParameterDirection.Input;
//                statusParam.Value = 1;

//                cmd.ExecuteNonQuery();
//            }
//        }

//        protected void InserteHubTestClients(Guid clientPK, string clientId)
//        {
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();
//                var transaction = connection.BeginTransaction();

//                try
//                {
//                    InserteHubTestClients(clientPK, clientId, transaction);

//                    transaction.Commit();
//                }
//                catch
//                {
//                    transaction.Rollback();
//                    throw;
//                }
//            }
//        }

//        protected static void InserteHubTestClients(Guid clientPK, string clientId, SqlTransaction transaction)
//        {
//            using (SqlCommand cmd = new SqlCommand())
//            {
//                cmd.Connection = transaction.Connection;
//                cmd.Transaction = transaction;
//                cmd.CommandText =
//@"
//delete from eHubOutboxEnvelope where OI_CC_Sender in (select CC_PK from eHubClient where CC_ID = @clientId);
//delete from eHubOutboxEnvelope where OI_CC_Recipient in (select CC_PK from eHubClient where CC_ID = @clientId);
//delete from eHubClient where CC_ID = @clientId;
//
//insert into eHubClient (CC_PK, CC_ID, CC_Password, CC_Odyssey_OH, CC_EmailAddress)
//values (@clientPK, @clientId, '', @clientPK, '')
//";
//                cmd.Parameters.Add("@clientPK", SqlDbType.UniqueIdentifier).Value = clientPK;
//                cmd.Parameters.Add("@clientId", SqlDbType.VarChar).Value = clientId;
//                cmd.ExecuteNonQuery();
//            }
//        }

//        protected Stream GetUSMessageData(string header, string body, string footer)
//        {
//            return new MemoryStream(Encoding.ASCII.GetBytes(string.Format("<Data><{0}>{1}</{0}><{2}>{3}</{2}><{4}>{5}</{4}></Data>", MessageAttributes.MessageHeader, HttpUtility.HtmlEncode(header), MessageAttributes.MessageBody, HttpUtility.HtmlEncode(body), MessageAttributes.MessageFooter, HttpUtility.HtmlEncode(footer))));
//        }

//        protected static SqlConnection GetSqlConnectionToeHub()
//        {
//            return new SqlConnection("Initial Catalog=eHubTransactions;Data Source=localhost;Integrated Security=SSPI;");
//        }

//        protected static IServiceLogger loggerOverride;
//        protected static IMessageSender messageSenderOverride; 

//        protected static void RunOutboundMessageProcessingService()
//        {
//            RunOutboundMessageProcessingService(messageSenderOverride);
//        }

//        protected static void RunOutboundMessageProcessingService(IMessageSender messageSender)
//        {
//            using (var connection = GetSqlConnectionToeHub())
//            {
//                connection.Open();

//                var logger = loggerOverride ?? new DummyLogger();
//                messageSender = messageSender ?? new DummyMessageSender();
//                var waitTimeOut = TimeSpan.FromSeconds(10);

//                try
//                {
//                    var service = new OutboundMessageProcessingService(connection, waitTimeOut, logger, messageSender);
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

//        protected MQTest GetNewMQTest(IMQConfiguration config)
//        {
//            var mqTest = new MQTest(config.Hostname, config.Port, config.Channel);
//            string connectResult = mqTest.ConnectMQ();
//            Assert.AreEqual<string>("Connected Successfully", connectResult);
//            mqTest.ClearQueue(config.QueueName);
//            return mqTest;
//        }
//    }
//}