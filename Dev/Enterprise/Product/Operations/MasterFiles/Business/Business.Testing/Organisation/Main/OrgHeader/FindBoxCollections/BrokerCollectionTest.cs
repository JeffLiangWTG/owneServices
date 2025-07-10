using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BrokerCollection))]
	sealed class BrokerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new BrokerCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = Brokers.AddNew();
			AssertEquals("Broker is selected", true, org1.OH_IsBroker);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagBR.IsAllowed = false;

			OrgHeader org2 = Brokers.AddNew();
			AssertEquals("Broker is not selected", false, org2.OH_IsBroker);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = Brokers.AddNew();
			organisation.OH_IsBroker = false;
			Brokers.ValidateEntityOnSaving(organisation);
			Assert("Error - Broker not selected", organisation.OH_IsBrokerInfo.HasErrors());

			organisation.OH_IsBroker = true;
			Brokers.ValidateEntityOnSaving(organisation);
			Assert("No error - Broker selected", !organisation.OH_IsBrokerInfo.HasErrors());
		}

		#region Implementation

		BrokerCollection Brokers;

		protected override void SetUp()
		{
			base.SetUp();
			Brokers = new BrokerCollection(Factory);
		}

		#endregion
	}
}
