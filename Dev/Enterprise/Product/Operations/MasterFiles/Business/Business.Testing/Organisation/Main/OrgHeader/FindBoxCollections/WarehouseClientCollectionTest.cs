using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(WarehouseClientCollection))]
	sealed class WarehouseClientCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIsValidClient()
		{
			OrgHeader badOrg = Factory.New<OrgHeader>();
			AssertEquals(false, WarehouseClients.IsValidClient(badOrg));

			OrgHeader whsClient = Factory.New<OrgHeader>();
			whsClient.OH_IsActive = true;
			whsClient.OH_IsWarehouseClient = true;
			AssertEquals(true, WarehouseClients.IsValidClient(whsClient));
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			Assert(WarehouseClients.AllowNewTemporaryOrganisations);
		}

		public void TestCreateAdditionalFilter()
		{
			ZQuery expectedFilter = new ZQuery(OrgHeaderSchema.OH_IsWarehouseClient, true);
			Assert("Additional Filter", Organisations.AdditionalFilter.LiteralTextADO.Contains(expectedFilter.LiteralTextADO));
		}

		public void TestSetDefaultsForNewChild()
		{
			OrgHeader org1 = WarehouseClients.AddNew();
			AssertEquals("WarehouseClient is selected", true, org1.OH_IsWarehouseClient);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed = false;

			OrgHeader org2 = WarehouseClients.AddNew();
			AssertEquals("WarehouseClient is not selected", false, org2.OH_IsWarehouseClient);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			Assert(WarehouseClients.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property7"));
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_IsActive = true;
			org.OH_IsWarehouseClient = false;
			AssertEquals((NoResString)"An Organization selected from here must have an Organization type of Warehouse selected.", WarehouseClients.GetAllNotificationsWhenAdditionalFilterNotMet(org));
		}

		#region IValidateForController Members

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = WarehouseClients.AddNew();
			organisation.OH_IsWarehouseClient = false;
			WarehouseClients.ValidateEntityOnSaving(organisation);
			Assert("Error - Warehouse not selected", organisation.OH_IsWarehouseClientInfo.HasErrors());

			organisation.OH_IsWarehouseClient = true;
			WarehouseClients.ValidateEntityOnSaving(organisation);
			Assert("No error - Warehouse selected", !organisation.OH_IsWarehouseClientInfo.HasErrors());
		}

		#endregion

		#region Implementation

		#region OrganisationsForTest

		public class OrganisationsForTest : WarehouseClientCollection
		{
			public OrganisationsForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.CreateAdditionalFilter(); }
			}
		}

		#endregion

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new WarehouseClientCollection(Factory, orgDefaults);
		}

		WarehouseClientCollection WarehouseClients;
		OrganisationsForTest Organisations;

		protected override void SetUp()
		{
			base.SetUp();
			WarehouseClients = new WarehouseClientCollection(Factory);
			Organisations = new OrganisationsForTest(Factory);
		}

		#endregion
	}
}
