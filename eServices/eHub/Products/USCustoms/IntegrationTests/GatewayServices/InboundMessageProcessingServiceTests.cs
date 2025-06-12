using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Snapshot;
using CargoWise.eServices.USCustoms.Common;
using CargoWise.eServices.USCustoms.Services;
using eServices.eHubDatabase.Tests.Common;
using NUnit.Framework;
using ServiceBroker.Common;
using ServiceBroker.Interface;

namespace CargoWise.eServices.USCustoms.IntegrationTests.GatewayServices
{
	[TestFixture]
	[UseSnapshotProtection("eHubTransactions")]
	public class InboundMessageProcessingServiceTests : GenericTestBase
	{
		protected override string CommonTestDataLocation => ".GatewayServices.Data.";
		protected override string TestDataSchemaLocation => ".TestBases.Schemas.";
		private Func<SqlConnection, SqlTransaction, SqlConnection> localConnPlaceHolder;
		
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			localConnPlaceHolder = DatabaseHelper.CreateLocalConnection;
			DatabaseHelper.CreateLocalConnection = (con, tran) => GetEHubTransactionsConnection();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			DatabaseHelper.CreateLocalConnection = localConnPlaceHolder;
		}

		[Test]
		[RestoreSnapshot]
		public void TestProcessInboundMessage_SameSystemID_AMA_Success()
		{
			using (var connection = GetEHubTransactionsConnection())
			{
				var service = new InboundMessageProcessingService(connection);

				using (var transaction = connection.BeginTransaction())
				{
					var mqResponse = @"
QK WTGTAAF
.WASUSCR 04041408
FSC
LAXZI
439-21030424
FSC/00
";
					var body = $"<Message IsProduction=\"true\" MessageType=\"AMA\"><![CDATA[{ new MemoryStream(Encoding.UTF8.GetBytes(mqResponse)).CompressAndEncode().ReadToEnd()}]]></Message>";
					using (var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
					{
						var message = new Message("AMA", bodyStream);
						service.ProcessInboundMessage(message, connection, transaction);
					}

					using (var command = new SqlCommand(@"
SELECT COUNT(*) AS Total FROM [dbo].[eHubInboxMessage] WHERE EI_ApplicationCode = 'AMA'
SELECT * FROM eHubInboxMessage
SELECT * FROM eHubOutboxMessage
SELECT * FROM eHubError
					", connection, transaction))
					{
						SQLExecution.AssertExecuteSQLResultMatchingXmlData(command, GetEmbeddedResource("GatewayServices.ExpectedData.TestProcessInboundMessage_SameSystemID_AMA_Success.xml"),
						tableNames : new string[] { "AMAInboxCount", "eHubInboxMessage", "eHubOutboxMessage", "eHubError" },
						ignoredComparingColumns : new string[] { "EI_PK", "OI_PK", "EI_MessageTrackingID", "EI_InsertUTC", "EI_LastUpdateUTC", "OI_MessageTrackingID", "OI_InsertUTC", "OI_EI_InboxPK" } );
					}

					transaction.Rollback();
				}
			}
		}

		[Test]
		[RestoreSnapshot]
		public void TestProcessInboundMessage_UEM_Success()
		{
			using (var connection = GetEHubTransactionsConnection())
			{
				var service = new InboundMessageProcessingService(connection);

				using (var transaction = connection.BeginTransaction())
				{
					var mqResponse = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<CBPManifestMessage xmlns=""http://manifest.cbp.dhs.gov/shared/model"">
    <Version></Version>
    <Filing>
        <SenderId>
            <Value>CUSTOMS</Value>
        </SenderId>
        <ReceiverId>
            <Value>APLU</Value>
        </ReceiverId>
        <ApplicationIdentifier>
            <Value>MMM</Value>
        </ApplicationIdentifier>
        <MessageDateTime>
            <Value>09/01/2023 02:10:00</Value>
        </MessageDateTime>
        <MessageType>
            <Value>2</Value>
        </MessageType>
        <MessageControlNumber>
            <Value>MANPHL3W0001109</Value>
        </MessageControlNumber>
        <MessageReferenceNumber>
            <Value>1</Value>
        </MessageReferenceNumber>
    </Filing>
    <Conveyance>
        <CarrierCode>
            <Value>8CAR</Value>
        </CarrierCode>
        <ConveyanceName>
            <Value>TITANIC</Value>
        </ConveyanceName>
        <FlightTripVoyageNumber>
            <Value>1W</Value>
        </FlightTripVoyageNumber>
        <ConveyanceCountryCode>
            <Value>US</Value>
        </ConveyanceCountryCode>
        <ModeOfTransportationCode>
            <Value>11</Value>
        </ModeOfTransportationCode>
        <ScheduledDepartureDate>
            <Value>20230915</Value>
        </ScheduledDepartureDate>
        <BOLInfoList>
            <BOLIssuerCode>
                <Value>APLU</Value>
            </BOLIssuerCode>
            <BOLNumber>
                <Value>123456789012</Value>
            </BOLNumber>
            <BOLClassificationCode>
                <Value>BOL</Value>
            </BOLClassificationCode>
            <ResponseMessage>
                <ResponseCode>970</ResponseCode>
                <SeverityIndicator>F</SeverityIndicator>
                <NarrativeText>MESSAGE REJECTED</NarrativeText>
            </ResponseMessage>
        </BOLInfoList>
        <BOLInfoList>
            <BOLIssuerCode>
                <Value>OTT1</Value>
            </BOLIssuerCode>
            <BOLNumber>
                <Value>PMP03132310</Value>
            </BOLNumber>
            <BOLClassificationCode>
                <Value>STD</Value>
            </BOLClassificationCode>
            <ResponseMessage>
                <ResponseCode>970</ResponseCode>
                <SeverityIndicator>F</SeverityIndicator>
                <NarrativeText>MESSAGE REJECTED</NarrativeText>
            </ResponseMessage>
        </BOLInfoList>
    </Conveyance>
</CBPManifestMessage>";
					var body = $"<Message IsProduction=\"false\" MessageType=\"UEM\"><![CDATA[{ new MemoryStream(Encoding.UTF8.GetBytes(mqResponse)).CompressAndEncode().ReadToEnd()}]]></Message>";
					using (var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
					{
						var message = new Message("USCustoms Export Manifest", bodyStream);
						service.ProcessInboundMessage(message, connection, transaction);
					}

					using (var command = new SqlCommand(@"
SELECT COUNT(*) AS Total FROM [dbo].[eHubInboxMessage] WHERE EI_ApplicationCode = 'UEM'
SELECT * FROM eHubInboxMessage
SELECT * FROM eHubOutboxMessage
SELECT * FROM eHubError
					", connection, transaction))
					{
						SQLExecution.AssertExecuteSQLResultMatchingXmlData(command, GetEmbeddedResource("GatewayServices.ExpectedData.TestProcessInboundMessage_UEM_Success.xml"),
							tableNames: new string[] { "UEMInboxCount", "eHubInboxMessage", "eHubOutboxMessage", "eHubError" },
							ignoredComparingColumns: new string[] { "EI_PK", "OI_PK", "EI_MessageTrackingID", "EI_InsertUTC", "EI_LastUpdateUTC", "OI_MessageTrackingID", "OI_InsertUTC", "OI_EI_InboxPK" });
					}

					transaction.Rollback();
				}
			}
		}

		static Stream GetEmbeddedResource(string location, bool requireXmlExist = true)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var fullName = $"{assembly.GetName().Name}.{location}";
			var resource = assembly.GetManifestResourceStream(fullName);
			if (requireXmlExist && resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullName));
			}
			return resource;
		}


		[Test]
		[RestoreSnapshot]
		public void TestDuplicateMessage()
		{
			using (var connection = GetEHubTransactionsConnection())
			{
				var service = new InboundMessageProcessingService(connection);

				using (var transaction = connection.BeginTransaction())
				{
					#region DB setup

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
                    USC                  DFO*                    *                 USE                   T          DFOUS0TST_AES
                    USC                  DFO___PRO               *                 USE                   F          DFOUS0PRO
                    USC                  DFOUS0PRO               USCustoms Export  USE                   T          DFOUS0PRO_AES
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
SELECT N'41A32845-E41D-4F81-92E2-F9570E255C8A', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 2, N'USC', N'DFO___PRO', N'%', N'USE', NULL UNION ALL
SELECT N'692691B5-4174-4948-8970-A9B994BBA96B', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 1, N'USC', N'DFO%', N'%', N'USE', NULL UNION ALL
SELECT N'A91EA7AD-7EF1-42C1-891E-69A005FFE5C2', N'14EE5190-E8D7-4736-A865-E079F66A35DF', 3, N'USC', N'DFOUS0PRO', N'USCustoms Export', N'USE', NULL UNION ALL
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

					var body = "B  13147927000E          ROCKYS PET EMPORIUM                                    SC1N11AUPAAPLUB0017316801      ATITANIC                2 60267110120180817 N    SC270                                   N                                       SC3                             APLUMST081418                                   SC3MAEU78998990                                                                 SC3MAUEURIEUIUO                                                                 N0113147927000EEROCKYS PET EMPORIUM           MAIN         CONTACT              N022300 W CHICAGO AVE                                              3125551212   N03CHICAGO                  ILUS60622                                           N0156999999900EFCARGOWISE INC                 TEST         FWR                  N021699 WALL ST                                                    8475551212   N03MOUNT PROSPECT           ILUS60056                                           N01            CABC EXPORTERS PTY LTD                                          NN02200 BOURKE ROAD                                                              N03ALEXANDRIA                 AU2015               R                            CL1OS 0001PLATES, FOIL, NONCELLULAR, NOT REINFORCED, LA0000000000 AC33D         CL23920990000KG 00000002500000005000   00000000000000000500     NLR             Y  13147927000E          ROCKYS PET EMPORIUM";
					using (var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
					{
						service.DuplicateMessage(connection, transaction, "DFOUS0PRO", "USCustoms Export", "USE", bodyStream);
					}

					using (var command = new SqlCommand(@"SELECT COUNT(*) FROM [dbo].[eHubInboxMessage] WHERE EI_ApplicationCode = 'XMS'", connection, transaction))
					{
						Assert.AreEqual(1, Convert.ToInt32(command.ExecuteScalar()));
					}

					transaction.Rollback();
				}
			}
		}

		[Test]
		[RestoreSnapshot]
		public void TestProcessInboundMessage_CacheReferenceFiles_SendNewTariffFileRequestsOnReceivingSuccessfulResponse()
		{
			var currentYear = DateTime.Now.ToString("yy");
			var referenceId = $"USC_ATF{currentYear}04";
			var referenceQuery = $"F110{currentYear}04";
			var updatedReferenceId = $"USC_ATF{currentYear}05";
			var updatedReferenceQuery = $"F110{currentYear}05";

			using (var connection = GetEHubTransactionsConnection())
			{
				using (var transaction = connection.BeginTransaction())
				{
					using (var command = new SqlCommand($@"
						UPDATE eHubReferenceFileQuery
						SET RF_ReferenceID = '{referenceId}',
							RF_Query = '{referenceQuery}'
						WHERE RF_PK = '04CA8F94-FEB6-404A-A527-F729BC04889C'", connection, transaction))
					{
						command.ExecuteNonQuery();
						transaction.Commit();
					}
				}

				var service = new InboundMessageProcessingService(connection);
				using (var transaction = connection.BeginTransaction())
				{
					// Unsuccessful response
					var header = $"A3910SV9      09102101   091021022148                                {referenceId}";
					var body = $"B013910SV9HZ                                               USC_REF              {referenceQuery}                    NOT ON FILE OR EXPIRED                              "
						+ "Y  3910SV9HZ00001                                                               ";
					var footer = "Z3910SV9      09102101   091021022148                                           ";
					var inboundMessage = $"<Message IsProduction=\"true\" MessageType=\"ABI\"><![CDATA[{new MemoryStream(Encoding.UTF8.GetBytes(header + body + footer)).CompressAndEncode().ReadToEnd()}]]></Message>";
					using (var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(inboundMessage)))
					{
						var message = new Message("ABI", bodyStream);
						service.ProcessInboundMessage(message, connection, transaction);
					}

					using (var command = new SqlCommand("SELECT COUNT(*) FROM eHubInboxMessage", connection, transaction))
					{
						Assert.AreEqual(0, Convert.ToInt32(command.ExecuteScalar()));

						command.CommandText = "SELECT RF_ReferenceID, RC_Data FROM eHubReferenceFileQuery LEFT JOIN eHubReferenceFileCache ON RF_PK = RC_RF ORDER BY RF_ReferenceID DESC, RC_ReceivedUTC DESC";
						using (var reader = command.ExecuteReader())
						{
							Assert.IsTrue(reader.Read());
							Assert.AreEqual(referenceId, reader.GetString(0));
							Assert.AreEqual(body, reader.GetString(1));
							Assert.IsFalse(reader.Read());
						}
					}

					// Successful response
					body = "B  3910SV9HZ                                               USC_REF                                          UPDATES FOUND IN THIS RANGE                         "
						+ "Y  3910SV9HZ00001                                                               ";
					inboundMessage = $"<Message IsProduction=\"true\" MessageType=\"ABI\"><![CDATA[{new MemoryStream(Encoding.UTF8.GetBytes(header + body + footer)).CompressAndEncode().ReadToEnd()}]]></Message>";
					using (var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(inboundMessage)))
					{
						var message = new Message("ABI", bodyStream);
						service.ProcessInboundMessage(message, connection, transaction);
					}

					using (var command = new SqlCommand("SELECT dbo.DecodeAndDecompress(EI_Content) FROM eHubInboxMessage", connection, transaction))
					using (var reader = command.ExecuteReader())
					{
						Assert.IsTrue(reader.Read());
						Assert.IsTrue(reader.GetString(0).Contains(updatedReferenceQuery));
						Assert.IsFalse(reader.Read());
					}

					using (var command = new SqlCommand(@"SELECT RF_ReferenceID, RC_Data FROM eHubReferenceFileQuery LEFT JOIN eHubReferenceFileCache ON RF_PK = RC_RF
						ORDER BY RF_ReferenceID DESC, RC_ReceivedUTC DESC", connection, transaction))
					using (var reader = command.ExecuteReader())
					{
						Assert.IsTrue(reader.Read());
						Assert.AreEqual(updatedReferenceId, reader.GetString(0));
						Assert.IsTrue(reader.IsDBNull(1));
						Assert.IsTrue(reader.Read());
						Assert.AreEqual(referenceId, reader.GetString(0));
						Assert.AreEqual(body, reader.GetString(1));
						Assert.IsTrue(reader.Read());
						Assert.IsFalse(reader.Read());
					}
				}
			}
		}

		[Test]
		[RestoreSnapshot]
		public void TestProcessInboundMessage_EmptyMessageShouldBeInsertedToInboxAndError()
		{
			using (var connection = GetEHubTransactionsConnection())
			{
				var service = new InboundMessageProcessingService(connection);

				using (var transaction = connection.BeginTransaction())
				{
					var body = "<Message IsProduction=\"true\" MessageType=\"AMS\"><![CDATA[H4sIAAAAAAAEAHN0DlJAAkYGBpaGhgZGRpbGhgrkgChU8ygGAEE3ylmgAAAA]]></Message>";
					using (var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
					{
						var message = new Message("AMS", bodyStream);
						service.ProcessInboundMessage(message, connection, transaction);
					}

					using (var command = new SqlCommand("SELECT COUNT(*) FROM [dbo].[eHubInboxMessage] WHERE EI_ApplicationCode = 'AMS'", connection, transaction))
					{
						Assert.AreEqual(1, Convert.ToInt32(command.ExecuteScalar()));
					}

					using (var command = new SqlCommand("SELECT COUNT(*) FROM [dbo].[eHubError] WHERE EE_Description LIKE 'Reason: Message has no content.% Header: ''ACR% Footer: ''ZCR%'", connection, transaction))
					{
						Assert.AreEqual(1, Convert.ToInt32(command.ExecuteScalar()));
					}

					transaction.Rollback();
				}
			}
		}

		[TestCase("EDIDUSCM2")]
		[TestCase("AMATS1AM2")]
		[RestoreSnapshot]
		public void TestProcessInboundMessage_InsertError(string clientId)
		{
			using (var connection = GetEHubTransactionsConnection())
			{
				var service = new InboundMessageProcessingService(connection);

				using (var transaction = connection.BeginTransaction())
				{
					var mqHeader = "A3910EEE      11021101   110711005537                                00004009430";
					var mqBody = $"B                                                                              BX0 BLOCK       1 REF ID: 5501 125    JC {clientId}_662946                        "
						+ "Y3910EEE BLABLA                                             BLABLABLA           ";
					var mqFooter = "Z3910EEE BLABLA                                             BLABLABLA           ";

					var compressedMessage = new MemoryStream(Encoding.UTF8.GetBytes(mqHeader + mqBody + mqFooter)).CompressAndEncode().ReadToEnd();
					var body = $"<Message IsProduction=\"true\" MessageType=\"ABI\"><![CDATA[{compressedMessage}]]></Message>";

					using (var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
					{
						var message = new Message("ABI", bodyStream);
						service.ProcessInboundMessage(message, connection, transaction);
					}

					using (var command = new SqlCommand("SELECT dbo.DecodeAndDecompress(EI_Content) FROM [dbo].[eHubInboxMessage] WHERE EI_ApplicationCode = 'USI'", connection, transaction))
					using (var reader = command.ExecuteReader())
					{
						Assert.IsTrue(reader.Read());
						Assert.AreEqual(mqBody, reader.GetString(0));
						Assert.IsFalse(reader.Read());
					}

					var errorDescription = "% Header: ''A3910EEE% Footer: ''Z3910EEE%RecipientNotFoundException: Message recipient could not be found. ReferenceId: NOREFERID%";

					using (var command = new SqlCommand($"SELECT COUNT(*) FROM [dbo].[eHubError] WHERE EE_Description LIKE '{errorDescription}'", connection, transaction))
					{
						Assert.AreEqual(1, Convert.ToInt32(command.ExecuteScalar()));
					}

					transaction.Rollback();
				}
			}
		}

		[Test]
		[RestoreSnapshot]
		public void TestProcessInboundMessage_MessageBodyCutUnexpectedly()
		{
			using (var connection = GetEHubTransactionsConnection())
			{
				var service = new InboundMessageProcessingService(connection);

				using (var transaction = connection.BeginTransaction())
				{
					// special character with Â
					var body1 = "<Message IsProduction=\"true\" MessageType=\"ABI\"><![CDATA[H4sIAAAAAAAEAK2RMQ7CMAxFr+IDdPhOS0rGkpKkUltXTYsEC3fhJJwFLgbpgBgYKOIN1pOlb9lylRtGPBhagIHhDXhRRtIcBdP37IhUicJAO1kRS9RO5ohhlDPrkg1UajqGtu0QbBepFd/EqbGR/CjzQFaonepXnqF4e7/crmHfe3KpjHVGnLuMRpHuaVg7kSmGqvehaggKpLRGyWT7t7WPv1/8kdOfP/IAV+qKkeIBAAA=]]></Message>";
					// special character with ¯
					var body2 = "<Message IsProduction=\"true\" MessageType=\"ABI\"><![CDATA[H4sIAAAAAAAEAK2RwQrCMBBEf2UP/YDZJjHm2NItCkpkYwW99BP7D36ZJgcPItWA7zQszDDsdCYw0iVQAQGBHbhIRpYGlul3enDrW7TeNanClhnGOCWcNM7sN9agHBvzxTSrjnIYRCmdtbsvQrxdMzS2slYm9qIyie5kf3zPczVBwTus1qMr0euBeFIV/4Hbnwd+APOz4P4xAgAA]]></Message>";

					using (var bodyStream1 = new MemoryStream(Encoding.Default.GetBytes(body1)))
					using (var bodyStream2 = new MemoryStream(Encoding.Default.GetBytes(body2)))
					{
						var message1 = new Message("ABI", bodyStream1);
						var message2 = new Message("ABI", bodyStream2);
						service.ProcessInboundMessage(message1, connection, transaction);
						service.ProcessInboundMessage(message2, connection, transaction);
					}

					using (var command = new SqlCommand("SELECT COUNT(*) FROM [dbo].[eHubInboxMessage] WHERE EI_ApplicationCode = 'USI' AND EI_Status = 2", connection, transaction))
					{
						Assert.AreEqual(2, Convert.ToInt32(command.ExecuteScalar()));
					}
					transaction.Rollback();
				}
			}
		}

		private static SqlConnection GetEHubTransactionsConnection()
		{
			return SqlServerHelper.OpenAdminSqlConnection("CargoWise.eServices.USCustoms.IntegrationTests.Properties.Settings.eHubTransactions");
		}
	}
}
