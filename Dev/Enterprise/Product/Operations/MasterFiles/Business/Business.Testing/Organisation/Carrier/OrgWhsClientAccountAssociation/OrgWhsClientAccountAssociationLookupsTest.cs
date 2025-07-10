using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgWhsClientAccountAssociationLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestOrgCarrierAccountCollection

		public void TestCarrierAccounts()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrierAcc11 = carrier1.CarrierAccounts.AddNew();
			var carrierAcc12 = carrier1.CarrierAccounts.AddNew();

			var carrier2 = Factory.New<OrgHeader>();
			var carrierAcc21 = carrier2.CarrierAccounts.AddNew();
			var carrierAcc22 = carrier2.CarrierAccounts.AddNew();

			var client = Factory.New<OrgHeader>();

			var cas1 = Factory.New<OrgWhsClientAccountAssociation>();
			cas1.OWC_OH_Client = client.PK;
			cas1.OWC_OAN_CarrierAccount = carrierAcc12.PK;

			var cas2 = Factory.New<OrgWhsClientAccountAssociation>();
			cas2.OWC_OH_Client = client.PK;
			cas2.OWC_OAN_CarrierAccount = carrierAcc21.PK;

			var lookups = new OrgWhsClientAccountAssociationLookups(cas1);
			AssertContainsExactElementsInAnyOrder(
				new[] { carrierAcc11, carrierAcc12, carrierAcc21, carrierAcc22 }, lookups.CarrierAccounts);
		}

		public void TestBillToCarrierAccounts()
		{
			TestBillToCarrierAccountCollectionCore((lookups) => lookups.BillToCarrierAccounts);
		}

		public void TestDutyBillToCarrierAccounts()
		{
			TestBillToCarrierAccountCollectionCore((lookups) => lookups.DutyBillToCarrierAccounts);
		}

		void TestBillToCarrierAccountCollectionCore(Func<OrgWhsClientAccountAssociationLookups, OrgCarrierAccountCollection> getOrgCarrierAccountCollection)
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrierAcc11 = carrier1.CarrierAccounts.AddNew();
			var carrierAcc12 = carrier1.CarrierAccounts.AddNew();

			var carrier2 = Factory.New<OrgHeader>();
			var carrierAcc21 = carrier2.CarrierAccounts.AddNew();
			var carrierAcc22 = carrier2.CarrierAccounts.AddNew();

			var client = Factory.New<OrgHeader>();

			var cas = Factory.New<OrgWhsClientAccountAssociation>();
			cas.OWC_OH_Client = client.PK;

			var lookups = new OrgWhsClientAccountAssociationLookups(cas);
			Assert("Collection should be empty", !getOrgCarrierAccountCollection(lookups).Any());

			cas.OWC_OAN_CarrierAccount = carrierAcc12.PK;
			AssertContainsExactElementsInAnyOrder(new[] { carrierAcc11, carrierAcc12 }, getOrgCarrierAccountCollection(lookups));

			cas.OWC_OAN_CarrierAccount = carrierAcc21.PK;
			AssertContainsExactElementsInAnyOrder(new[] { carrierAcc21, carrierAcc22 }, getOrgCarrierAccountCollection(lookups));
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			var cas1 = Factory.New<OrgWhsClientAccountAssociation>();
			var lookups1 = new OrgWhsClientAccountAssociationLookups(cas1);
			var warehouses1 = lookups1.Warehouses;

			var cas2 = Factory.New<OrgWhsClientAccountAssociation>();
			var lookups2 = new OrgWhsClientAccountAssociationLookups(cas2);
			var warehouses2 = lookups2.Warehouses;

			AssertSame("Must return the same object from cache", warehouses1, warehouses2);
		}

		#endregion

		#region TestSalesChannels

		public void TestSalesChannels()
		{
			var cas1 = Factory.New<OrgWhsClientAccountAssociation>();
			var lookups1 = new OrgWhsClientAccountAssociationLookups(cas1);
			var salesChannels1 = lookups1.SalesChannels;

			var cas2 = Factory.New<OrgWhsClientAccountAssociation>();
			var lookups2 = new OrgWhsClientAccountAssociationLookups(cas2);
			var salesChannels2 = lookups2.SalesChannels;

			AssertSame("Must return the same object from cache", salesChannels1, salesChannels2);
		}

		#endregion

		#region TestBillingTypesList

		public void TestBillingTypesList()
		{
			var cas = Factory.New<OrgWhsClientAccountAssociation>();
			var lookups = new OrgWhsClientAccountAssociationLookups(cas);
			AssertEquals(3, lookups.BillingTypesList.Count);
			AssertEquals(true, lookups.BillingTypesList.ContainsCode(CarrierBillingType.BillReceiver));
			AssertEquals(true, lookups.BillingTypesList.ContainsCode(CarrierBillingType.BillSender));
			AssertEquals(true, lookups.BillingTypesList.ContainsCode(CarrierBillingType.BillThirdParty));
		}

		#endregion
	}
}
