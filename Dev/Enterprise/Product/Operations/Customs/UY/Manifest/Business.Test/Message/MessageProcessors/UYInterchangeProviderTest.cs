using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYInterchangeProviderTest : InterchangeProviderTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestMessagesPopulateNewInterchange()
		{
			SetCertificate(true);
			var message1 = CreateAndPopulateMessage() as TestEdiMessage;
			var message2 = CreateAndPopulateMessage() as TestEdiMessage;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new TestEdiMessage[] { message1, message2 });

			var logger = new LoggingInformation();
			var provider = new UYInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();

			message1.Reload();
			message2.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 2, interchanges.Length);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);

				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				var interchange2 = interchanges.FirstOrDefault(x => x.PK == message2.EM_EI);

				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);

				AssertNotNull("Interchange 2 is linked to message 2", interchange2);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);

				interchanges.ToList().ForEach(interchange =>
				{
					AssertEquals("To", UYCInterchange.UYCustomsForTest, interchange.EI_To);
					AssertEquals("From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
					AssertEquals("Interchange Status", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("Interchange Type ", "UYC", interchange.EI_InterchangeType);
					AssertEquals("Application Code ", EDIInterchange.ApplicationCodes.UYCustoms, interchange.EI_ApplicationCode);
					AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
					AssertEquals("EI_IsActive", ZBool.True, interchange.EI_IsActive);
					AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
					AssertEquals("EI_GP", message1.EM_GP, interchange.EI_GP);
				});
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNewInterchangeWithEnvelope()
		{
			SetCertificate(true);
			var message1 = CreateAndPopulateMessage() as TestEdiMessage;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new TestEdiMessage[] { message1 });

			var logger = new LoggingInformation();
			var provider = new UYInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();

			message1.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 1, interchanges.Length);

				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);

				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);

				Assert("EI_BodyText with Envelope", interchange1.EI_BodyText.Contains("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\""));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoNotSendInterchangeWithEmptyBody()
		{
			SetCertificate(true);
			var message = CreateAndPopulateMessage() as TestEdiMessage;
			message.EM_MessageText = "";

			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var logger = new LoggingInformation();
			var provider = new UYInterchangeProvider(logger, messageCollection);

			AssertEquals(0, provider.Interchanges.Length);
			AssertContains("The Interchange Body is empty even though there are 1 messages.", logger.UserLogStrings[0]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoNotSendInterchangeWithInvalidCertificate()
		{
			SetCertificate(false);

			var message = CreateAndPopulateMessage() as TestEdiMessage;
			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var logger = new LoggingInformation();
			var provider = new UYInterchangeProvider(logger, messageCollection);

			AssertEquals(0, provider.Interchanges.Length);
			AssertContains("The Certificate is not valid.", logger.UserLogStrings[0]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInterchangeType()
		{
			SetCertificate(true);
			var message = CreateAndPopulateMessage() as TestEdiMessage;

			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var logger = new LoggingInformation();
			var provider = new UYInterchangeProvider(logger, messageCollection);
			Factory.Save();

			AssertType(typeof(UYCInterchange), provider.Interchanges.Single());
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new UYInterchangeProvider(new LoggingInformation(), collection);

		EDIMessage CreateAndPopulateMessage()
		{
			var message = Factory.New<TestEdiMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message.EM_ApplicationReference = "REF1";
			message.EM_EI = new ZGuid();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageOwner = "";
			message.EM_MessageText = File.ReadAllText(bodyTextWithOutSign);
			message.EM_MessageType = MessageTypes.Codes.UYC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GP = new ZGuid();

			return message;
		}

		void SetCertificate(bool valid)
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			if (valid)
			{
				credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
				credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			}
			else
			{
				var invalidCertificate = new byte[] { 1, 2, 3, 4 };
				credential.GP_Certificate = invalidCertificate;
				credential.CurrentDecryptedCertificatePassphrase = "TEST";
			}
		}

		readonly ZString bodyTextWithOutSign = Path.Combine(BaseSourcePath, DAETestingConstants.DAEManifestWithOutSign);
	}
}
