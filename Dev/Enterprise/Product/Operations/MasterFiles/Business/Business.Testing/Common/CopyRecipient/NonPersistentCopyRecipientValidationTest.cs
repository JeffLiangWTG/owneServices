using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class NonPersistentCopyRecipientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEmailAddress()
		{
			// Arrange
			NonPersistentCopyRecipient carbonCopyRecipient = new NonPersistentCopyRecipient(Organization, Constants.CopyRecipientType.CarbonCopyRecipient);
			NonPersistentCopyRecipient blindCarbonCopyRecipient = new NonPersistentCopyRecipient(Organization, Constants.CopyRecipientType.CarbonCopyRecipient);
			// Act
			carbonCopyRecipient.EmailAddress = "test1@test.com";
			blindCarbonCopyRecipient.EmailAddress = "123456";
			// Assert
			AssertNoErrors(carbonCopyRecipient.EmailAddressInfo);
			AssertHasErrors(blindCarbonCopyRecipient.EmailAddressInfo);
		}

		OrgHeader Organization
		{
			get { return organization ?? (organization = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader organization;
	}
}
