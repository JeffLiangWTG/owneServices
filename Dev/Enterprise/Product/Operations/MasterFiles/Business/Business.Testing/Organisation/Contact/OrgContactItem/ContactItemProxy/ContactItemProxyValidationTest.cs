using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ContactItemProxyValidationTest : BusinessObjectValidationTestCase
	{
		#region CheckOI_OC

		public void TestNoErrorWhenContactIsInactive()
		{
			var inactiveContact = Factory.New<OrgContact>();
			inactiveContact.OC_IsActive = false;

			var contactItem = new EmailContactItem(inactiveContact);

			AssertNoErrors(contactItem.OI_OCInfo);
		}

		#endregion
	}
}
