using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class JobDocAddressExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestLoadJobDocAddressQuickly

		public void TestLoadJobDocAddressQuickly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			AssertNull(orderInOtherFactory.LoadJobDocAddressQuickly(DocAddressType.PickUpAddress));

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedHits, otherFactory);

			orderInOtherFactory.LoadJobDocAddressQuickly(DocAddressType.ConsigneeAddress);
			// no new hits should occur since the Collection should be loaded
			AssertDbHits(expectedHits, otherFactory);

			var pickupDocAddress = orderInOtherFactory.DocAddresses.CreateWithAddressType(DocAddressType.PickUpAddress);
			AssertEquals("Should return correct Doc Address.", pickupDocAddress,
				orderInOtherFactory.LoadJobDocAddressQuickly(DocAddressType.PickUpAddress));
			// no new hits should occur since the Collection should be loaded
			AssertDbHits(expectedHits, otherFactory);
		}

		#endregion

		#region TestGetOrganisation

		public void TestGetOrganisation()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TRANSPORT CO";

			jobDocAddress.OrganisationPK = org.PK;
			AssertEquals(org, jobDocAddress.GetOrganisation());

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "OVERRIDEN TRANSPORT CO";
			AssertNull(jobDocAddress.GetOrganisation());
		}

		#endregion
	}
}
