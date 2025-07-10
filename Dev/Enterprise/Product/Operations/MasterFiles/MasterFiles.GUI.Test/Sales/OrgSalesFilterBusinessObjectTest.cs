using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgSalesFilterBusinessObject))]
	public class OrgSalesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestOrigin()
		{
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var gb = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");

			var salesAuAu = Factory.New<OrgSales>();
			salesAuAu.OW_OriginID = au.PK;
			salesAuAu.OW_OriginTableCode = au.TablePrefix;
			salesAuAu.OW_DestinationID = au.PK;
			salesAuAu.OW_DestinationTableCode = au.TablePrefix;
			var salesAuUs = Factory.New<OrgSales>();
			salesAuUs.OW_OriginID = au.PK;
			salesAuUs.OW_OriginTableCode = au.TablePrefix;
			salesAuUs.OW_DestinationID = us.PK;
			salesAuUs.OW_DestinationTableCode = us.TablePrefix;
			var salesUsAu = Factory.New<OrgSales>();
			salesUsAu.OW_OriginID = us.PK;
			salesUsAu.OW_OriginTableCode = us.TablePrefix;
			salesUsAu.OW_DestinationID = au.PK;
			salesUsAu.OW_DestinationTableCode = au.TablePrefix;
			var salesUsUs = Factory.New<OrgSales>();
			salesUsUs.OW_OriginID = us.PK;
			salesUsUs.OW_OriginTableCode = us.TablePrefix;
			salesUsUs.OW_DestinationID = us.PK;
			salesUsUs.OW_DestinationTableCode = us.TablePrefix;

			var filterBizObj = new OrgSalesFilterBusinessObject();
			var originFilter = (ModuleGuidFilter)filterBizObj[OrgSalesFilterBusinessObject.FilterDescription.Origin];
			AssertEquals("Origin", originFilter.MultilingualDescription);

			originFilter.IsActive = true;

			originFilter.Property = au.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesAuAu,
					salesAuUs
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			originFilter.Property = us.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesUsAu,
					salesUsUs
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			originFilter.Property = gb.PK;
			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<OrgSales>(),
				Factory.Load<OrgSales>(filterBizObj.Filter));
		}

		public void TestDestination()
		{
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var gb = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");

			var salesAuAu = Factory.New<OrgSales>();
			salesAuAu.OW_OriginID = au.PK;
			salesAuAu.OW_OriginTableCode = au.TablePrefix;
			salesAuAu.OW_DestinationID = au.PK;
			salesAuAu.OW_DestinationTableCode = au.TablePrefix;
			var salesAuUs = Factory.New<OrgSales>();
			salesAuUs.OW_OriginID = au.PK;
			salesAuUs.OW_OriginTableCode = au.TablePrefix;
			salesAuUs.OW_DestinationID = us.PK;
			salesAuUs.OW_DestinationTableCode = us.TablePrefix;
			var salesUsAu = Factory.New<OrgSales>();
			salesUsAu.OW_OriginID = us.PK;
			salesUsAu.OW_OriginTableCode = us.TablePrefix;
			salesUsAu.OW_DestinationID = au.PK;
			salesUsAu.OW_DestinationTableCode = au.TablePrefix;
			var salesUsUs = Factory.New<OrgSales>();
			salesUsUs.OW_OriginID = us.PK;
			salesUsUs.OW_OriginTableCode = us.TablePrefix;
			salesUsUs.OW_DestinationID = us.PK;
			salesUsUs.OW_DestinationTableCode = us.TablePrefix;

			var filterBizObj = new OrgSalesFilterBusinessObject();
			var destinationFilter = (ModuleGuidFilter)filterBizObj[OrgSalesFilterBusinessObject.FilterDescription.Destination];
			AssertEquals("Destination", destinationFilter.MultilingualDescription);

			destinationFilter.IsActive = true;

			destinationFilter.Property = au.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesAuAu,
					salesUsAu
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			destinationFilter.Property = us.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesAuUs,
					salesUsUs
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			destinationFilter.Property = gb.PK;
			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<OrgSales>(),
				Factory.Load<OrgSales>(filterBizObj.Filter));
		}

		public void TestBuyer()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var salesOrg1A = Factory.New<OrgSales>();
			salesOrg1A.OW_OH_Buyer = org1.PK;
			var salesOrg1B = Factory.New<OrgSales>();
			salesOrg1B.OW_OH_Buyer = org1.PK;
			var salesOrg2A = Factory.New<OrgSales>();
			salesOrg2A.OW_OH_Buyer = org2.PK;
			var salesOrg2B = Factory.New<OrgSales>();
			salesOrg2B.OW_OH_Buyer = org2.PK;

			var filterBizObj = new OrgSalesFilterBusinessObject();
			var buyerFilter = (ModuleGuidFilter)filterBizObj[OrgSalesFilterBusinessObject.FilterDescription.Buyer];
			AssertEquals("Buyer", buyerFilter.MultilingualDescription);

			buyerFilter.IsActive = true;

			buyerFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesOrg1A,
					salesOrg1B
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			buyerFilter.Property = org2.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesOrg2A,
					salesOrg2B
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			buyerFilter.Property = org3.PK;
			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<OrgSales>(),
				Factory.Load<OrgSales>(filterBizObj.Filter));
		}

		public void TestSupplier()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();

			var salesOrg1A = Factory.New<OrgSales>();
			salesOrg1A.OW_OH_Supplier = org1.PK;
			var salesOrg1B = Factory.New<OrgSales>();
			salesOrg1B.OW_OH_Supplier = org1.PK;
			var salesOrg2A = Factory.New<OrgSales>();
			salesOrg2A.OW_OH_Supplier = org2.PK;
			var salesOrg2B = Factory.New<OrgSales>();
			salesOrg2B.OW_OH_Supplier = org2.PK;

			var filterBizObj = new OrgSalesFilterBusinessObject();
			var supplierFilter = (ModuleGuidFilter)filterBizObj[OrgSalesFilterBusinessObject.FilterDescription.Supplier];
			AssertEquals("Supplier", supplierFilter.MultilingualDescription);

			supplierFilter.IsActive = true;

			supplierFilter.Property = org1.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesOrg1A,
					salesOrg1B
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			supplierFilter.Property = org2.PK;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					salesOrg2A,
					salesOrg2B
				},
				Factory.Load<OrgSales>(filterBizObj.Filter));

			supplierFilter.Property = org3.PK;
			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<OrgSales>(),
				Factory.Load<OrgSales>(filterBizObj.Filter));
		}

		#endregion

		#region Overrides

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgSalesFilterBusinessObject();
		}

		#endregion
	}
}
