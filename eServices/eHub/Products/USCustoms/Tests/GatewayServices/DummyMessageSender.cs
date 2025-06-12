//using System;
//using System.Data;
//using System.Data.SqlClient;
//using System.Diagnostics;
//using System.IO;
//using System.Text;
//using System.Threading;
//using System.Xml;

//namespace CargoWise.eServices.USCustoms.Services
//{
//    class DummyMessageSender : IMessageSender
//    {
//        #region IMessageSender Members

//        public void Send(Stream messageBody, SqlConnection connection, SqlTransaction transaction, out Stream responseMessageBody)
//        {
//            int threadId = Thread.CurrentThread.ManagedThreadId;
//            Random rand = new Random(threadId);
//            rand.Next(3);
//            Thread.Sleep(3 * 1000); // Sleep random seconds to test when messages are processed out of sync concurrently.

//            messageBody.Position = 0;

//            string internalTrackingId = string.Empty;
//            string clientId = string.Empty;

//            using (XmlTextReader reader = new XmlTextReader(messageBody))
//            {
//                if (reader.Read() && reader.Read())
//                {
//                    internalTrackingId = reader.GetAttribute("InternalTrackingId");
//                    clientId = reader.GetAttribute("ClientId");


//                    reader.Read();
//                    string message = reader.ReadInnerXml();

//                    EventLog.WriteEntry("CargoWise eHub", string.Format("Dummy Message Sender processing message on thread {0}", threadId), EventLogEntryType.Information);

//                    string[] messageSplit = message.Split(',');
//                    if (messageSplit.Length == 4)
//                    {
//                        string splitMessage = clientId + "," + messageSplit[0] + "," + messageSplit[1] + ",USI,EI";
//                        Send(new MemoryStream(ASCIIEncoding.ASCII.GetBytes(splitMessage)), connection, transaction);
//                        Thread.Sleep(100);

//                        splitMessage = clientId + "," + messageSplit[0] + "," + messageSplit[2] + ",USI,EI";
//                        Send(new MemoryStream(ASCIIEncoding.ASCII.GetBytes(splitMessage)), connection, transaction);
//                        Thread.Sleep(100);

//                        splitMessage = clientId + "," + messageSplit[0] + "," + messageSplit[3] + ",USI,EI";
//                        Send(new MemoryStream(ASCIIEncoding.ASCII.GetBytes(splitMessage)), connection, transaction);
//                    }
//                }
//            }

//            responseMessageBody = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(string.Format("<SendResponse ClientId=\"{0}\" InternalTrackingId=\"{1}\" ResponseStatus=\"2\"></SendResponse>", clientId, internalTrackingId)));
//        }

//        #endregion

//        void Send(Stream messageBody, SqlConnection connection, SqlTransaction transaction)
//        {
//            StreamReader reader = new StreamReader(messageBody);
//            string message = reader.ReadToEnd();

//            string[] messageSplit = message.Split(',');
//            if (messageSplit.Length == 5)
//            {
//                SendMessageToMQ(messageSplit[0], messageSplit[1], messageSplit[2], messageSplit[3], messageSplit[4], connection, transaction);
//            }
//        }

//        void SendMessageToMQ(string client, string message, string part, string applicationCode, string interchangeType, SqlConnection connection, SqlTransaction transaction)
//        {
//            using (SqlCommand cmd = new SqlCommand())
//            {
//                cmd.Connection = connection;
//                cmd.Transaction = transaction;
//                cmd.CommandText =
//@"insert into DummyUSCustomsMQ (DD_PK, DD_Client, DD_Message, DD_Part, DD_CreateTime, DD_ApplicationCode, DD_InterchangeType)
//values (newid(), @client, @message, @part, getdate(), @applicationCode, @interchangeType)
//";
//                cmd.Parameters.Add("@client", SqlDbType.VarChar).Value = client;
//                cmd.Parameters.Add("@message", SqlDbType.VarChar).Value = message;
//                cmd.Parameters.Add("@part", SqlDbType.VarChar).Value = part;
//                cmd.Parameters.Add("@applicationCode", SqlDbType.Char).Value = applicationCode;
//                cmd.Parameters.Add("@interchangeType", SqlDbType.Char).Value = interchangeType;

//                cmd.ExecuteNonQuery();
//            }
//        }
//    }
//}
