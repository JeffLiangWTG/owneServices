using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgDocumentCopyRecipientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestODR_EmailAddress()
		{
			// Arrange
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_OH = org.PK;
			OrgDocument orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument.OD_OC = orgContact.PK;
			Factory.Save();
			// Act
			OrgDocumentCopyRecipient orgDocumentCarbonCopyRecipient = orgDocument.CarbonCopyRecipients.AddNew();
			orgDocumentCarbonCopyRecipient.ODR_EmailAddress = "test1@test.com";
			OrgDocumentCopyRecipient orgDocumentBlindCopyRecipient = orgDocument.BlindCarbonCopyRecipients.AddNew();
			orgDocumentBlindCopyRecipient.ODR_EmailAddress = "123456";
			// Assert
			AssertNoErrors(orgDocumentCarbonCopyRecipient.ODR_EmailAddressInfo);
			AssertHasErrors(orgDocumentBlindCopyRecipient.ODR_EmailAddressInfo);
		}
	}
}
