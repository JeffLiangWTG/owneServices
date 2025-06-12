using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using CargoWise.eServices.USCustoms.Common;
using CargoWise.eServices.USCustoms.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.USCustoms.Tests.GatewayServices
{
    [TestClass]
    public class eHubOutboxServiceTests
    {
        [TestMethod]
        [Ignore]
        public void TestDuplicates()
        {
            using (var connection = new SqlConnection("Data Source=localhost;Initial Catalog=eHubTransactions;Integrated Security=True;"))
            {
                connection.Open();

                var service = new eHubOutboxService(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    #region DB setup

                    using (var command = new SqlCommand("[dbo].[InsertClient]", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PK", Guid.NewGuid());
                        command.Parameters.AddWithValue("@ClientID", ServiceBrokerConstants.USCustoms.USCustomsClientId);
                        command.Parameters.AddWithValue("@FreindlyName", "US Customs");
                        command.Parameters.AddWithValue("@Odyssey_OH", Guid.Empty);
                        command.Parameters.AddWithValue("@Email", "hello@cbp.com");
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand("[dbo].[InsertClient]", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PK", Guid.NewGuid());
                        command.Parameters.AddWithValue("@ClientID", "DFOUS0PRO");
                        command.Parameters.AddWithValue("@FreindlyName", "DHL");
                        command.Parameters.AddWithValue("@Odyssey_OH", Guid.Empty);
                        command.Parameters.AddWithValue("@Email", "hello@dhl.com");
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand("[dbo].[InsertClient]", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PK", Guid.NewGuid());
                        command.Parameters.AddWithValue("@ClientID", "DFOUS0PRO_AES");
                        command.Parameters.AddWithValue("@FreindlyName", "AES PRD");
                        command.Parameters.AddWithValue("@Odyssey_OH", Guid.Empty);
                        command.Parameters.AddWithValue("@Email", "hello@dhl.com");
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand("[dbo].[InsertClient]", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PK", Guid.NewGuid());
                        command.Parameters.AddWithValue("@ClientID", "DFOUS0TST_AES");
                        command.Parameters.AddWithValue("@FreindlyName", "AES TST");
                        command.Parameters.AddWithValue("@Odyssey_OH", Guid.Empty);
                        command.Parameters.AddWithValue("@Email", "hello@dhl.com");
                        command.ExecuteNonQuery();
                    }

                    var inboxPk = Guid.NewGuid();
                    using (var command = new SqlCommand("[dbo].[InsertInbox]", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@InboxPK", inboxPk);
                        command.Parameters.AddWithValue("@MessageTrackingID", Guid.NewGuid());
                        command.Parameters.AddWithValue("@EnvelopeTrackingID", Guid.Empty);
                        command.Parameters.AddWithValue("@SenderID", "DFOUS0PRO");
                        command.Parameters.AddWithValue("@RecipientID", ServiceBrokerConstants.USCustoms.USCustomsClientId);
                        command.Parameters.AddWithValue("@MessageType", "XP");
                        command.Parameters.AddWithValue("@IsFlatFile", 0);
                        command.Parameters.AddWithValue("@EmailSubject", "");
                        command.Parameters.AddWithValue("@FileName", "");
                        command.Parameters.AddWithValue("@ApplicationCode", "USE");
                        command.Parameters.AddWithValue("@Status", 0);
                        command.Parameters.AddWithValue("@CurrentDateTimeUTC", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        command.Parameters.AddWithValue("@Content", "H4sIAAAAAAAEAJVUW27bMBC8yvYE2aVeFGAYoCi6ZiuRAknFToN8FEg+CwNN+tGz9aNH6hVKyXGs2I5hjyABIneH3Fly/v35O+u9/PX8svnxPJ8tn74/Pv2czz7dy1oEcS8gosiSFHOWIIxQ644hlcgYSVe9C3h4mM9udhzV5vH3nikGZpxYkmZ5wUsFbxBSQVA+gG4764JyQHABvCRDKPpOiK7pK0QqUsqzN9aggzBaHqYxyJHlBRHSWAQhA7PlYwVesK65ZHMjX3J2fth16wNFFYlfsi7SVD91Xja5FK7Rryp/sUvjrTnkYxRbqWQf9K2C2gEzNSyaaQm9DgoYfiRL1DFlVHJejnyJXKqm9Qvr6oPAVvQeibP0gjqn9eblFohqIYX7bFfaK9DmqKtbJXZYrNwpPkaRD1aiaWASfAV4WmRZRozYyJe0tjcBOmd9p+SUUTe9zxGz/AryWO/0V4pKDjdtbK2HLtxBEw51PUcX6x06V9nefVXgrLgi+fT+EtGotTC10+JoVvTxNmUn0j6EbMh6wHhvbVjG0xucML7V3ut4Ujvhgj+ZhgNo+ETjSHg94WM85UmJPD7r18A9tilHw+M41fEN/v2hubvCr0bb29rdYrN5mRrotyHlSgPdcdzsjfk/BXlM2acFAAA=");
                        command.ExecuteNonQuery();
                    }

                    Guid transformationSetPK = Guid.Empty;
                    using (var command = new SqlCommand(@"[dbo].[SelectTransformsByPartiesMessage]", connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@SenderID", ServiceBrokerConstants.USCustoms.USCustomsClientId));
                        command.Parameters.Add(new SqlParameter("@RecipientID", ServiceBrokerConstants.USCustoms.USCustomsClientId));
                        command.Parameters.Add(new SqlParameter("@SourceType", string.Empty));
                        command.Parameters.Add(new SqlParameter("@TransformationSet", SqlDbType.UniqueIdentifier)).Direction = ParameterDirection.Output;

                        command.ExecuteNonQuery();
                        transformationSetPK = command.Parameters["@TransformationSet"].Value as Guid? ?? Guid.Empty;
                    }

                    Guid clientPk = Guid.Empty;
                    using (var command = new SqlCommand(@"SELECT CC_PK FROM [dbo].[eHubClient] where CC_ID = @CC_ID", connection, transaction))
                    {
                        command.Parameters.Add(new SqlParameter("@CC_ID", ServiceBrokerConstants.USCustoms.USCustomsClientId));
                        clientPk = command.ExecuteScalar() as Guid? ?? Guid.Empty;
                    }

                    if (transformationSetPK.Equals(Guid.Empty))
                    {
                        transformationSetPK = Guid.NewGuid();
                        using (var command = new SqlCommand(@"
INSERT INTO [dbo].[eHubTransformationSet]([TS_PK], [TS_Name], [TS_CC_Sender], [TS_CC_Recipient], [TS_DT_Source], [TS_XPathPredicate], [TS_BillingInterfaceName], [TS_BillingElement], [TS_BillingXPathSource], [TS_BillingXPathTarget], [TS_BillSender], [TS_BillRecipient], [TS_CC_BillOther], [TS_BillingNumMessagesIncluded], [TS_BillingFee], [TS_Direction])
SELECT @TS_PK, N'US Customs Config', @ClientID, @ClientID, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL", connection, transaction))
                        {
                            command.Parameters.Add(new SqlParameter("@TS_PK", transformationSetPK));
                            command.Parameters.Add(new SqlParameter("@ClientID", clientPk));
                            command.ExecuteNonQuery();
                        }
                    }

                    /**********************
                    Original Sender      Original Recipient      Message Type      Application Code      Enable     Recipient ID
                    DFO*                 USC                     *                 USE                   T          DFOUS0TST_AES
                    DFO___PRO            USC                     *                 USE                   F          DFOUS0PRO
                    DFOUS0PRO            USC                     XP                USE                   T          DFOUS0PRO_AES
                    *                    *                       *                 *                     #BLANK#    #BLANK#
                    **********************/
                    using (var command = new SqlCommand(@"
DELETE [dbo].[eHubCodeMapValue]
WHERE CV_CK in (select CK_PK from [dbo].[eHubCodeMapKey] 
WHERE CK_CS in (select CS_PK from [dbo].[eHubCodeSet] where CS_TS = @TS_PK))

DELETE [dbo].[eHubCodeMapKey]
WHERE CK_CS in (select CS_PK from eHubTransactions..eHubCodeSet where CS_TS = @TS_PK)

DELETE [dbo].[eHubCodeSetResult]
where CR_CS in (select CS_PK from eHubTransactions..eHubCodeSet where CS_TS = @TS_PK)

DELETE [dbo].[eHubCodeSet]
WHERE CS_TS = @TS_PK

INSERT INTO [dbo].[eHubCodeSet]([CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name])
SELECT N'14EE5190-E8D7-4736-A865-E079F66A35DF', N'Copy of Message', @TS_PK, @ClientID, @ClientID, N'Original Sender', N'Original Recipient', N'Message Type', N'Application Code', NULL

INSERT INTO [dbo].[eHubCodeSetResult]([CR_PK], [CR_CS], [CR_Order], [CR_Name])
SELECT N'4AB47CDF-AF66-4D6F-B360-AB362E30345F', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 1, N'Enable' UNION ALL
SELECT N'B79527E0-3AF3-44F1-84D8-21F59E73BE7F', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 2, N'Recipient ID'

INSERT INTO [dbo].[eHubCodeMapKey]([CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value])
SELECT N'41A32845-E41D-4F81-92E2-F9570E255C8A', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 2, N'DFO___PRO', N'USC', N'%', N'USE', NULL UNION ALL
SELECT N'692691B5-4174-4948-8970-A9B994BBA96B', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 1, N'DFO%', N'USC', N'%', N'USE', NULL UNION ALL
SELECT N'A91EA7AD-7EF1-42C1-891E-69A005FFE5C2', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 3, N'DFOUS0PRO', N'USC', N'XP', N'USE', NULL UNION ALL
SELECT N'7E00732B-5058-462C-8468-0012DA1DBF48', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 4, N'%', N'%', N'%', N'%', NULL

INSERT INTO [dbo].[eHubCodeMapValue]([CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey])
SELECT N'692691B5-4174-4948-8970-A9B994BBA96B', N'B79527E0-3AF3-44F1-84D8-21F59E73BE7F', N'DFOUS0TST_AES', NULL UNION ALL
SELECT N'692691B5-4174-4948-8970-A9B994BBA96B', N'4AB47CDF-AF66-4D6F-B360-AB362E30345F', N'T', NULL UNION ALL
SELECT N'41A32845-E41D-4F81-92E2-F9570E255C8A', N'B79527E0-3AF3-44F1-84D8-21F59E73BE7F', N'Not_Me', NULL UNION ALL
SELECT N'41A32845-E41D-4F81-92E2-F9570E255C8A', N'4AB47CDF-AF66-4D6F-B360-AB362E30345F', N'F', NULL UNION ALL
SELECT N'A91EA7AD-7EF1-42C1-891E-69A005FFE5C2', N'B79527E0-3AF3-44F1-84D8-21F59E73BE7F', N'DFOUS0PRO_AES', NULL UNION ALL
SELECT N'A91EA7AD-7EF1-42C1-891E-69A005FFE5C2', N'4AB47CDF-AF66-4D6F-B360-AB362E30345F', N'T', NULL UNION ALL
SELECT N'7E00732B-5058-462C-8468-0012DA1DBF48', N'B79527E0-3AF3-44F1-84D8-21F59E73BE7F', N'', NULL UNION ALL
SELECT N'7E00732B-5058-462C-8468-0012DA1DBF48', N'4AB47CDF-AF66-4D6F-B360-AB362E30345F', N'', NULL", connection, transaction))
                    {
                        command.Parameters.Add(new SqlParameter("@TS_PK", transformationSetPK));
                        command.Parameters.Add(new SqlParameter("@ClientID", clientPk));
                        command.ExecuteNonQuery();
                    }

                    #endregion

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("Dummy")))
                    {
                        service.DuplicateMessage(connection, transaction, inboxPk.ToString(), stream);
                    }

                    using (var command = new SqlCommand(@"SELECT COUNT(*) FROM [dbo].[eHubInboxMessage] WHERE EI_ApplicationCode = 'XMS'", connection, transaction))
                    {
                        Assert.AreEqual(2, Convert.ToInt32(command.ExecuteScalar()));
                    }

                    transaction.Rollback();
                }

            }

        }
    }
}
