using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ForwarderCollection))]
	sealed class ForwarderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new ForwarderCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Forwarders.AddNew();
			AssertEquals("Forwarder is selected", true, org1.OH_IsForwarder);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagFA.IsAllowed = false;

			OrgHeader org2 = Forwarders.AddNew();
			AssertEquals("Forwarder is not selected", false, org2.OH_IsForwarder);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Forwarders.AddNew();
			organisation.OH_IsForwarder = false;
			Forwarders.ValidateEntityOnSaving(organisation);
			Assert("Error - Forwarder not selected", organisation.OH_IsForwarderInfo.HasErrors());

			organisation.OH_IsForwarder = true;
			Forwarders.ValidateEntityOnSaving(organisation);
			Assert("No error - Forwarder selected", !organisation.OH_IsForwarderInfo.HasErrors());

			Forwarders.AllowOtherOrgTypes = true;
			organisation.OH_IsForwarder = false;
			Forwarders.ValidateEntityOnSaving(organisation);
			Assert("No error - Forwarder not selected", !organisation.OH_IsForwarderInfo.HasErrors());
		}

		#region Implementation

		ForwarderCollection Forwarders;

		protected override void SetUp()
		{
			base.SetUp();
			Forwarders = new ForwarderCollection(Factory);
		}

		#endregion
	}
}
