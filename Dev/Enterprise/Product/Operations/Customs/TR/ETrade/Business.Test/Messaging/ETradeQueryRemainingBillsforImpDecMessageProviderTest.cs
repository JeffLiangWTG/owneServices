using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing.Messaging
{
	public class ETradeQueryRemainingBillsforImpDecMessageProviderTest : TestCaseWithFactory
	{
		public void TestIETradeQueryRemainingBillsforImpDec()
		{
			var header = CreateHeader();
			IETradeQueryRemainingBillsforImpDec provider = new ETradeQueryRemainingBillsforImpDecMessageProvider(header);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("Null Arg Exception expected", () => new ETradeQueryRemainingBillsforImpDecMessageProvider(null));

				AssertEquals("IMessageSender.Parent", header, provider.Parent);
				AssertEquals("IMessageSender.Messages", header.Messages, provider.Messages);
				AssertEquals("IMessageSender.JobReference", "ULU-ETR0000001", provider.JobReference);

				AssertEquals("UserName", "20201224104", provider.UserName);
				AssertEquals("UserPassword", "25d55ad283aa400af464c76d713c07ad", provider.UserPassword);
				AssertEquals("RegistrationNo", "testRegNoforTest", provider.RegistrationNo);
			});
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";
			header.RegistrationNumber = "testRegNoforTest";

			var prevMsg = header.Messages.AddNew(typeof(ETradeEDIMessage));
			prevMsg.EM_MessageType = TRMessageTypes.Codes.TRE;
			prevMsg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			prevMsg.EM_MessageOwner = "PRV";

			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "PRV";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
		}
	}
}
