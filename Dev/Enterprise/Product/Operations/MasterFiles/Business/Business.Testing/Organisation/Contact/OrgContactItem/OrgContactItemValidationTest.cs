using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactItemValidationTest : BusinessObjectValidationTestCase
	{
		#region CheckOI_OC

		public void TestNoErrorWhenContactIsInactive()
		{
			var inactiveContact = Factory.New<OrgContact>();
			inactiveContact.OC_IsActive = false;

			var contactItem = Factory.New<OrgContactItem>();
			contactItem.OI_OC = inactiveContact.PK;

			AssertNoErrors(contactItem.OI_OCInfo);
		}

		#endregion
	}
}
