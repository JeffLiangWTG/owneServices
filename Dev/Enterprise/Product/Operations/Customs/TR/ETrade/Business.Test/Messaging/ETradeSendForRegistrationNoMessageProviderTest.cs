using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing.Messaging
{
	class ETradeSendForRegistrationNoMessageProviderTest : TestCaseWithFactory
	{
		public void TestIETradeSendForRegistrationNo()
		{
			var header = CreateHeader();
			IETradeSendForRegistrationNo provider = new ETradeSendForRegistrationNoMessageProvider(header);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("Null Arg Exception expected", () => new ETradeSendForRegistrationNoMessageProvider(null));

				AssertEquals("IMessageSender.Parent", header, provider.Parent);
				AssertEquals("IMessageSender.Messages", header.Messages, provider.Messages);
				AssertEquals("IMessageSender.JobReference", "ULU-ETR0000001", provider.JobReference);

				AssertEquals("DeclarantNameAndTitle", "Test Company Name", provider.DeclarantNameAndTitle);
				AssertEquals("DeclarantIDTaxNo", "BUS1234567", provider.DeclarantIDTaxNo);
				AssertEquals("CustomsOffice", "12345", provider.CustomsOffice);
				AssertEquals("TemporaryRegistrationNo", "testRegNoforTest", provider.TemporaryRegistrationNo);
			});
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";
			header.TempRegNo = "testRegNoforTest";
			header.AMA_CustomsOffice = "TR12345";

			var prevMsg = header.Messages.AddNew(typeof(ETradeEDIMessage));
			prevMsg.EM_MessageType = TRMessageTypes.Codes.TRE;
			prevMsg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			prevMsg.EM_MessageOwner = "PRV";

			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_Name = "Test Company Name";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "BUS1234567";
		}
	}
}
