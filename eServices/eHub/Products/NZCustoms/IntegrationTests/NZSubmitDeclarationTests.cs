using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using CargoWise.eHub2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.NZCustoms.IntegrationTests
{
	[TestClass]
	public class NZSubmitDeclarationTests : BaseNZCustomsTest
	{
		const string outputFolder = @"C:\Temp\NZCustoms\GatewayTest\";

		/// <summary>
		/// Send message with unique CustomerReference number to eHub2Gateway. 
		/// Check that message was logged in eHubInboxMessage.
		/// Validate output in folder : "C:\Temp\NZCustoms\GatewayTest\"
		/// Test port should be created.
		/// </summary>
		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SendTestNZMessageToEHub2Gateway()
		{
			var messageTrackingID = Guid.NewGuid();
			var dateTimeNow = DateTime.Now;

			var messageContent = SendMessageToEHub2Gateway(MessageModeType.TST, messageTrackingID);
			ValidateMessageInEHubInboxTable(messageTrackingID);
			Thread.Sleep(10000);
			ValidateGeneratedOutput(dateTimeNow, messageContent);
		}

		/// <summary>
		/// Send Test Message To Production System
		/// </summary>
		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SendProductionNZMessageToEHub2Gateway()
		{
			var messageContent = SendMessageToEHub2Gateway(MessageModeType.PROD, Guid.NewGuid());
		}
		#region Implementation

		string SendMessageToEHub2Gateway(MessageModeType messageMode, Guid messageTrackingID)
		{
			int interchangeNumber = GenerateInterchangeNumber();
			int messageNumber = GenerateMessageNumber();
			string messageReference = GenerateMessageReference();

			var originalMessage = String.Format("UNA:+.? 'UNB+UNOA:2+00009908C:ZZZ+CUSSWT:ZZZ+121130:1315+{0}'UNH+{1}+CUSDEC:D:96B:UN'BGM+929+{2}+9'CST++10:105:143'LOC+9+AUSYD'LOC+11+NZAKL'LOC+41+NZAKL'DTM+151:20121130:102'MEA+WT+AAD+KGM:250'RFF+MB:08133333333'RFF+HWB:ILYAKB123456'PAC+54++PK'TDT+20++4+++++:::QF253'NAD+AL+00782903F:ZZZ:143'NAD+CB+:ZZZ:143'UNS+D'DMS+CI21398712+935'TOD+++FOB:106:143'CST+1+5601210000G:169:143'FTX+AAA+++WADDING ETC OF COTTON'LOC+27+US'LOC+35+AU'NAD+SU+00710841Y:ZZZ:143'MOA+14:1200.00:NZD'CUX+2++1.00'MOA+40:1200'MOA+64:150'MOA+70:12'GIS+N:109:143'TAX+1+GST'MOA+161:204.30'UNS+S'CNT+4:1'CNT+5:1'CNT+11:54'TAX+3+CUD++1200'MOA+161:0.00'TAX+3+GST'MOA+161:204.30'TAX+4+TOT'MOA+161:204.30'GIS+B:134:143'AUT+EHBMCAOKNJMHO@OG+40006206E'UNT+43+{1}'UNZ+1+{0}'", interchangeNumber, messageNumber, messageReference);
			var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(originalMessage));
			string messageContent = string.Format(@"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>C498F602-48E1-4839-9900-4BEC610BB604</Reference><Type>NZC</Type><Content>{0}</Content></NZCustoms>", encodedMessage);

			using (var message = new eHub2GatewayMessage())
			{
				message.ApplicationCode = "NZC";
				message.ClientID = "NZCustoms";
				message.MessageTrackingID = messageTrackingID;
				message.FileName = messageReference;
				message.SenderID = "HYEDAUIKB";
				message.MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(messageContent)); ;
				message.SchemaName = "http://cargowise.com/ehub/products/#NZCustoms";
				message.SchemaType = MessageSchemaType.Xml;
				message.MessageMode = messageMode;

				using (var channelFactory = new ChannelFactory<IEHub2Reciever>("eHub2GatewayService"))
				{
					channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
					var channel = channelFactory.CreateChannel();
					channel.SendMessage(message);
				}
			}
			return encodedMessage;
		}

		void ValidateGeneratedOutput(DateTime dateTimeNow, string messageContent)
		{
			var outputDirectory = new DirectoryInfo(outputFolder);
			var fileInfos = outputDirectory.GetFiles().Where(f => f.CreationTime > dateTimeNow);
			Assert.IsTrue(fileInfos.Any(), "File wasn't created");
			var fileInfo = fileInfos.First();
			using (var reader = fileInfo.OpenText())
			{
				string actualText = reader.ReadToEnd();
				var expectedText = String.Format(@"<ns0:SendLodgement xmlns:ns0=""http://cargowise.com/ehub/products/""><ns0:reference>C498F602-48E1-4839-9900-4BEC610BB604</ns0:reference><ns0:messageType>NZC</ns0:messageType><ns0:message>{0}</ns0:message></ns0:SendLodgement>", messageContent);
				Assert.AreEqual(expectedText, actualText, "Message are not the same");
			}
		}

		#endregion
	}
}

