using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrgSupplierFilterBusinessObject))]
	sealed class OrgSupplierFilterBusinessObjectTest : OrganisationFilterBusinessObjectTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "unused private member after test extraction, better take a look")]
		OrgSupplierFilterBusinessObject OrgSupplierFilter
		{
			get { return (OrgSupplierFilterBusinessObject)FilterObject; }
		}

		public void TestNotLoadingInactiveOrganization()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "BBBB";

			OrgHeader relatedConsign = Factory.NewWithValidTestData<OrgHeader>();
			relatedConsign.OH_FullName = "AAAA";

			org1.OH_IsConsignor = true;
			relatedConsign.OH_IsConsignor = true;
			org1.BuyerLinks.AddNew(relatedConsign);

			Factory.Save();
			OrgHeaderCollection matchedOrgs = new OrgHeaderCollection(Factory);

			OrgSupplierFilterBusinessObject filterBizO = FilterBusinessObjectFactory.New<OrgSupplierFilterBusinessObject>();
			filterBizO.OH_RelatedConsign = relatedConsign.PK;
			matchedOrgs.Load(filterBizO.Filter);

			AssertCollectionContains("Organization should be included if it matches search criteria", org1, matchedOrgs);
			AssertCollectionNotContains("RelatedConsign should not be included if it does not match search criteria", relatedConsign, matchedOrgs);
			AssertEquals("OH_IsActive should be true", true, org1.OH_IsActive);

			org1.OH_IsActive = false;
			Factory.Save();
			matchedOrgs.Load(filterBizO.Filter);
			AssertCollectionNotContains("Organization should not be included if it is inactive", org1, matchedOrgs);
		}

		protected override void TestFilterInternal(OrgHeaderCollection matchedOrgs, OrgHeader relatedConsign,
			OrgHeader linkedBuyingConsignee, OrgHeader linkedSupplyingConsignor,
			OrgHeader linkedBuyer, OrgHeader linkedSupplier,
			OrgHeader consignee, OrgHeader consignor)
		{
			AssertEquals("Collection", 1, matchedOrgs.Count);
			AssertCollectionNotContains("RelatedConsign should not be included", relatedConsign, matchedOrgs);
			AssertCollectionNotContains("LinkedBuyingConsignee not should be included - Not a Supplier", linkedBuyingConsignee, matchedOrgs);
			AssertCollectionContains("LinkedSupplyingConsignor should be included - Consignor which Supplies to RelatedConsign", linkedSupplyingConsignor, matchedOrgs);
			AssertCollectionNotContains("LinkedSupplier should not be included - Not a Consignee/Consignor", linkedSupplier, matchedOrgs);
			AssertCollectionNotContains("LinkedBuyer should not be included - Not  a Consignee/Consignor", linkedBuyer, matchedOrgs);
			AssertCollectionNotContains("Consignee should not be included - No Supplier/Buyer link", consignee, matchedOrgs);
			AssertCollectionNotContains("Consignor should not be included - No Supplier/Buyer link", consignor, matchedOrgs);
		}
	}
}
