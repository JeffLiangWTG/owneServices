using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRInterchangeProviderTest : InterchangeProviderTestCase
	{
		public void TestMessagesPopulateWithBinaryDataOfMessageBody()
		{
			var messageData = new byte[] { 0x00, 0x01, 0xFF, 0x01 };

			var blobValue = new ZBlob(messageData);
			var byteValue = (byte[])blobValue;
			AssertEquals("Pre-condition, type convert should not change data", messageData, byteValue);

			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = CreateEdiMessage();
			message.EM_MessageData = new ZBlob(messageData);
			messages.Add(message);

			Factory.Save();

			var factory2 = NewFactory();
			var messageReloaded = factory2.Load<EDIMessage>(message.PK);
			AssertEquals(messageData, messageReloaded.EM_MessageData);

			var logger = new LoggingInformation();
			var provider = new TRInterchangeProvider(logger, messages);
			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfInterchanges", 1, interchanges.Count);

				var interchange = interchanges.FirstOrDefault(x => x.PK == message.EM_EI) as EDIInterchange;
				AssertEquals("EI_ApplicationCode", "TRC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "TRO", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "TROCustomsTest", interchange.EI_To);
				AssertEquals("EI_From ", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("EI_BodyData", messageData, interchange.EI_BodyData);
				AssertEquals("EI_FooterText", "", interchange.EI_FooterText);
				AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
				AssertEquals("EI_HeaderText", ZString.Empty, interchange.EI_HeaderText);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals("EI_SessionGUID", true, interchange.EI_SessionGUID.IsValid);
			});
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message = CreateEdiMessage();
			message.EM_MessageText = "<Message><OtherInfo><Code>AAA+111+BBB+222</Code></OtherInfo></Message>";
			messages.Add(message);

			var logger = new LoggingInformation();
			var provider = new TRInterchangeProvider(logger, messages);
			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfInterchanges", 1, interchanges.Count);

				var interchange = interchanges.FirstOrDefault(x => x.PK == message.EM_EI) as EDIInterchange;
				AssertEquals("EI_ApplicationCode", "TRC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "TRO", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "TROCustomsTest", interchange.EI_To);
				AssertEquals("EI_From ", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("EI_BodyText", "<Message><OtherInfo><Code>AAA+111+BBB+222</Code></OtherInfo></Message>", interchange.EI_BodyText);
				AssertEquals("EI_FooterText", "", interchange.EI_FooterText);
				AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
				AssertEquals("EI_HeaderText", ZString.Empty, interchange.EI_HeaderText);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals("EI_SessionGUID", true, interchange.EI_SessionGUID.IsValid);
			});
		}

		public void TestDoNotSendInterchangeWithEmptyBody()
		{
			var message = CreateEdiMessage();
			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);
			var logger = new LoggingInformation();
			var provider = new TRInterchangeProvider(logger, messageCollection);

			ErrorReporter.Clear();

			try
			{
				AssertEquals(0, provider.Interchanges.Length);

				string expectedMessage = "The Interchange Body is empty even though there are 1 messages.\r\nMessage Text : \r\n";
				AssertContains(expectedMessage, logger.UserLogStrings[0]);
				AssertEquals(expectedMessage, ErrorReporter.LastMessageReported);

				message.EM_MessageText = "message text";
				provider = new TRInterchangeProvider(logger, messageCollection);
				AssertEquals(1, provider.Interchanges.Length);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestEUTInterchangeHeaderTextShouldIncludeFilename()
		{
			var declaration = Factory.New<JobDeclaration>();
			var message = Factory.New<ExportUnionMessage>();
			message.EM_LinkUniqueID = declaration.PK;
			message.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message.EM_MessageType = TRMessageTypes.Codes.EUT;
			Factory.Save();

			var messages = new NonDependentEDIMessageCollection(Factory);
			message.EM_MessageText = "<Message><OtherInfo><Code>AAA+111+BBB+222</Code></OtherInfo></Message>";
			messages.Add(message);

			var logger = new LoggingInformation();
			var provider = new TRInterchangeProvider(logger, messages);
			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfInterchanges", 1, interchanges.Count);

				var interchange = interchanges.FirstOrDefault(x => x.PK == message.EM_EI) as EDIInterchange;

				AssertEquals($"{{\"custom.TR.EUT.FileName\":\"{interchange.EI_SessionGUID}.txt\"}}", interchange.EI_HeaderText);

				AssertEquals("EI_ApplicationCode", "TRC", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "EUT", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "EUTUnion", interchange.EI_To);
				AssertEquals("EI_From ", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("EI_BodyText", "<Message><OtherInfo><Code>AAA+111+BBB+222</Code></OtherInfo></Message>", interchange.EI_BodyText);
				AssertEquals("EI_FooterText", "", interchange.EI_FooterText);
				AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals("EI_SessionGUID", true, interchange.EI_SessionGUID.IsValid);
			});
		}

		public void TestAssignsEMGPToEIGPForNCTSTR5Message() 
		{
			AssertAssignsEMGPToEIGPForMessage(TRMessageTypes.Codes.TR5);
		}

		public void TestAssignsEMGPToEIGPForNCTST15Message()
		{
			AssertAssignsEMGPToEIGPForMessage(TRMessageTypes.Codes.T15);
		}

		void AssertAssignsEMGPToEIGPForMessage(string messageType)
		{
			var message = Factory.New<NCTSMessage>();
			message.EM_MessageType = messageType;
			Factory.Save();

			var messages = new NonDependentEDIMessageCollection(Factory);
			message.EM_MessageText = "<Message><OtherInfo><Code>AAA+111+BBB+222</Code></OtherInfo></Message>";
			message.EM_GP = ZGuid.NewZGuid();
			messages.Add(message);

			var logger = new LoggingInformation();
			var provider = new TRInterchangeProvider(logger, messages);
			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("NumberOfInterchanges", 1, interchanges.Count);
			var interchange = interchanges.FirstOrDefault(x => x.PK == message.EM_EI) as EDIInterchange;
			AssertEquals(message.EM_GP, interchange.EI_GP);
		}

		EDIMessage CreateEdiMessage()
		{
			var manifest = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var message = Factory.New<TRManifestMessage>();
			message.EM_LinkedObject = (BusinessObject)manifest;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRO;
			message.EM_MessageOwner = "CZH";
			message.EM_IsTestMessage = true;
			message.EM_ApplicationReference = "RefID1";
			return message;
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new TRInterchangeProvider(new LoggingInformation(), collection);
	}
}
