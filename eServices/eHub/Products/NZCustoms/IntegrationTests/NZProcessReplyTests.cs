using System;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Threading;
using CargoWise.eHub.Products.NZCustoms.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eServices.TestHelpers.Database.Common;

namespace CargoWise.eHub.Products.NZCustoms.IntegrationTests
{
	[TestClass]
	public class NZProcessReplyTests : BaseNZCustomsTest
	{
		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ProcessReplyFromNetTcpLocation()
		{
			string messageReference = GenerateMessageReference();
			InsertMessageReferenceToDatabase("HYEDAUIKB", "NZC", messageReference);
			SendMessageToNZCustomsReply(messageReference);
			Thread.Sleep(10000);
			ValidateMessageInEHubInboxTable(messageReference);
			ValidateResultInOutbox(messageReference);
		}

		#region Implementation

		void SendMessageToNZCustomsReply(string reference)
		{
			var message = new NZCustomsReply();
			message.Reference = reference;
			message.Content = @"UNA:+.? 'UNB+UNOA:2+EXT.TSW.GOVT.NZ:ZZZ+00009908C:ZZZ+121212:1246+11'UNH+1+CUSRES:D:96B:UN+B00159089'BGM+963+58033310:1'GIS+801:120:143'ERP+3::2'ERC+460::143'ERP+1::44'ERC+487::143'UNT+8+1'UNZ+1+11'";

			using (var channelFactory = new ChannelFactory<INZCustomsReply>("NZCustomsReplyService"))
			{
				channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
				var channel = channelFactory.CreateChannel();
				channel.SendMessage(message);
			}
		}

		void ValidateResultInOutbox(string reference)
		{
			using (var connection = SqlServerHelper.OpenAdminSqlConnection("CargoWise.eHub.DataAccess.Sql.eHubTransactions"))
			{
				var command = connection.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = "select top 1 dbo.DecodeAndDecompress(OI_Content) as Content, OI_PK, OI_CC_Sender, OI_CC_Recipient, OI_EI_InboxPK, OI_EnvelopeTrackingID, OI_MessageTrackingID, OI_BatchEnvelopeTrackingID, OI_OverrideFilename, OI_OverrideEmailSubject, OI_Status, OI_Content, OI_XmlContent, OI_DT_Target, OI_InsertUTC, OI_LastUpdateUTC, OI_UncompressedLength from eHubOutboxMessage order by OI_InsertUTC desc";

				var reader = command.ExecuteReader();

				if (reader.Read())
				{
					string actualText = reader["Content"].ToString();
					string expectedText = @"<ns0:NZCustomsReply xmlns:ns0=""http://cargowise.com/ehub/products/""><ns0:Reference>B15105300</ns0:Reference><ns0:Content>UNA:+.? 'UNB+UNOA:2+EXT.TSW.GOVT.NZ:ZZZ+00009908C:ZZZ+121212:1246+11'UNH+1+CUSRES:D:96B:UN+B00159089'BGM+963+58033310:1'GIS+801:120:143'ERP+3::2'ERC+460::143'ERP+1::44'ERC+487::143'UNT+8+1'UNZ+1+11'</ns0:Content></ns0:NZCustomsReply>".Replace("B15105300", reference);
					Assert.AreEqual(expectedText, actualText);
				}
			}
		}

		#endregion
	}
}
