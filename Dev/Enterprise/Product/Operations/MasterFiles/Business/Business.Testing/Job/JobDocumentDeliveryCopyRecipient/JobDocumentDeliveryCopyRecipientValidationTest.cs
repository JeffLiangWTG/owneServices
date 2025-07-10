using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocumentDeliveryCopyRecipientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJDR_EmailAddress()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			var emailToRecipient = jobDocumentDelivery.EmailToRecipients.AddNew();
			emailToRecipient.JDR_EmailAddress = "dexter@morgan.com";
			var carbonCopyRecipient = jobDocumentDelivery.CarbonCopyRecipients.AddNew();
			carbonCopyRecipient.JDR_EmailAddress = "123456";
			var blindCarbonCopyRecipient = jobDocumentDelivery.BlindCarbonCopyRecipients.AddNew();
			blindCarbonCopyRecipient.JDR_EmailAddress = ZString.Empty;

			AssertNoErrors(emailToRecipient.JDR_EmailAddressInfo);
			AssertHasErrors(carbonCopyRecipient.JDR_EmailAddressInfo);
			AssertHasErrors(blindCarbonCopyRecipient.JDR_EmailAddressInfo);
		}
	}
}