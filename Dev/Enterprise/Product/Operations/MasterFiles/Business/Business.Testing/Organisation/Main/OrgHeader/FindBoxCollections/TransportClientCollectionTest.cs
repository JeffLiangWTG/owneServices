using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TransportClientCollection))]
	sealed class TransportClientCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new TransportClientCollection(Factory, orgDefaults);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = TransportClients.AddNew();
			AssertEquals("TransportClient is selected", true, org1.OH_IsTransportClient);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagTC.IsAllowed = false;

			OrgHeader org2 = TransportClients.AddNew();
			AssertEquals("TransportClient is not selected", false, org2.OH_IsTransportClient);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = TransportClients.AddNew();
			organisation.OH_IsTransportClient = false;
			TransportClients.ValidateEntityOnSaving(organisation);
			Assert("Error - TransportClient not selected", organisation.OH_IsTransportClientInfo.HasErrors());

			organisation.OH_IsTransportClient = true;
			TransportClients.ValidateEntityOnSaving(organisation);
			Assert("No error - TransportClient selected", !organisation.OH_IsTransportClientInfo.HasErrors());
		}

		#region Implementation

		TransportClientCollection TransportClients;

		protected override void SetUp()
		{
			base.SetUp();
			TransportClients = new TransportClientCollection(Factory);
		}

		#endregion
	}
}
