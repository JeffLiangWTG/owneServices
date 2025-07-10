using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYCOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateNewInterchangeProvider()
		{
			var bodyTextWithOutSign = Path.Combine(BaseSourcePath, DAETestingConstants.DAEManifestWithOutSign);

			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var message1 = Factory.New<UYMessage>();

			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message1.EM_ApplicationReference = "REF1";
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_IsTestMessage = true;
			message1.EM_MessageOwner = "OWNER1";
			message1.EM_MessageText = File.ReadAllText(bodyTextWithOutSign);
			message1.EM_MessageType = MessageTypes.Codes.UYC;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_Status = EDIMessage.Status.Queued;

			var message2 = Factory.New<UYMessage>();

			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message2.EM_ApplicationReference = "REF2";
			message2.EM_GB = GlbBranch.CurrentBranch.PK;
			message2.EM_IsTestMessage = false;
			message2.EM_MessageOwner = "OWNER2";
			message2.EM_MessageText = File.ReadAllText(bodyTextWithOutSign);
			message2.EM_MessageType = MessageTypes.Codes.UYC;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new UYCOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 2, interchangesCreated.Length);

				message1.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message1.EM_Status);
				var interchange1 = interchangesCreated.Single(i => i.PK == message1.EM_EI);
				AssertEquals("EI_InterchangeNum", message1.EM_MessageNum, interchange1.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "1", interchange1.EI_InterchangeNum);

				message2.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message2.EM_Status);
				var interchange2 = interchangesCreated.Single(i => i.PK == message2.EM_EI);
				AssertEquals("EI_InterchangeNum", message2.EM_MessageNum, interchange2.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "2", interchange2.EI_InterchangeNum);
			});
		}
	}
}
