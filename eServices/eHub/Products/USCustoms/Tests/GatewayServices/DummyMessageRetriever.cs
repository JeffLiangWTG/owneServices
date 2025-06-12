//using System;
//using System.Data.SqlClient;
//using System.IO;
//using System.Text;

//namespace CargoWise.eServices.USCustoms.Tests
//{
//    class DummyMessageRetriever : IMessageRetriever
//    {
//        public DummyMessageRetriever(SqlConnection connection)
//        {
//            this.connection = connection;
//        }

//        #region IMessageRetriever Members

//        public void Retrieve(out Stream messageBody, out string destinationId, out string applicationCode, out string interchangeType)
//        {
//            Guid? messagePK = null;

//            using (var cmd = new SqlCommand())
//            {
//                cmd.Connection = connection;
//                cmd.Transaction = currentTransaction;

//                cmd.CommandText = "SELECT TOP 1 DD_Client, DD_Message, DD_Part, DD_PK, DD_ApplicationCode, DD_InterchangeType FROM DummyUSCustomsMQ ORDER BY DD_CreateTime";

//                using (var reader = cmd.ExecuteReader())
//                {
//                    if (reader.Read())
//                    {
//                        destinationId = reader.GetString(0);
//                        applicationCode = reader.GetString(4);
//                        interchangeType = reader.GetString(5);
//                        string message = string.Format("<Data>{0},{1},{2},{3},{4}</Data>", destinationId, reader.GetString(1), reader.GetString(2), applicationCode, interchangeType);

//                        messageBody = new MemoryStream(Encoding.ASCII.GetBytes(message));
//                        messagePK = reader.GetGuid(3);

//                    }
//                    else
//                    {
//                        destinationId = null;
//                        messageBody = null;
//                        applicationCode = null;
//                        interchangeType = null;
//                    }
//                }
//            }

//            if (messagePK.HasValue)
//            {
//                using (var cmd = new SqlCommand())
//                {
//                    cmd.Connection = connection;
//                    cmd.Transaction = currentTransaction;
//                    cmd.CommandText = "DELETE DummyUSCustomsMQ WHERE DD_PK = @pk";
//                    cmd.Parameters.Add(new SqlParameter("pk", messagePK.Value));
//                    cmd.ExecuteNonQuery();
//                }
//            }
//        }

//        public void BeginTransaction()
//        {
//            if (currentTransaction != null)
//            {
//                throw new InvalidOperationException("There is already a transaction open");
//            }

//            currentTransaction = connection.BeginTransaction();
//        }

//        public void CommitTransaction()
//        {
//            CheckTransactionIsPending();
//            currentTransaction.Commit();
//            currentTransaction = null;
//        }

//        public void RollbackTransaction()
//        {
//            CheckTransactionIsPending();
//            currentTransaction.Rollback();
//            currentTransaction = null;
//        }

//        #endregion

//        readonly SqlConnection connection;
//        SqlTransaction currentTransaction;

//        void CheckTransactionIsPending()
//        {
//            if (currentTransaction == null)
//            {
//                throw new InvalidOperationException("There is no current transaction pending");
//            }
//        }
//    }
//}