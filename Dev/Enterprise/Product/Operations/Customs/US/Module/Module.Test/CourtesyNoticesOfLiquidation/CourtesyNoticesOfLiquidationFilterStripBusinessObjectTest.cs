using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(CourtesyNoticesOfLiquidationFilterStripBusinessObject))]
	sealed class CourtesyNoticesOfLiquidationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new CourtesyNoticesOfLiquidationFilterStripBusinessObject();
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.BrokerReferenceNo]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.ChangeLiquidationReasonCode]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.CustomsDocumentFilingLocation]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.EntryDate]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.EntryFiler]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.EntryNumberFilterName]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.EntryType]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.ExtensionSuspensionCode]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.ExtensionSuspensionDate]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.ImportOfRecordNo]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.LiquidationType]);
			AssertNotNull(filter[CourtesyNoticesOfLiquidationFilterStripBusinessObject.Constants.LiquidationDate]);
		}

		public void TestLiquidationNoticeEntryTypes()
		{
			var filter = new CourtesyNoticesOfLiquidationFilterStripBusinessObject();
			AssertEquals(29, filter.EntryTypeList.Count);
			filter.EntryTypeList.ContainsCode(EntryTypeList.Codes.ReconciliationSummary);
			filter.EntryTypeList.ContainsCode(EntryTypeList.Codes.Drawback);
		}

		public void TestCurrentCompanyFilter()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var liq1 = Factory.New<CusLiquidation>();
			liq1.B8_GC = currentCompanyPK;
			var liq2 = Factory.New<CusLiquidation>();
			liq2.B8_GC = currentCompanyPK;
			var liq3 = Factory.New<CusLiquidation>();
			liq3.B8_GC = company2.PK;
			Factory.Save();
			var filterObj = new CourtesyNoticesOfLiquidationFilterStripBusinessObject();
			Assert("liq1 matches filter", liq1.MatchesFilter(filterObj.Filter));
			Assert("liq2 matches filter", liq2.MatchesFilter(filterObj.Filter));
			Assert("liq3 does not match filter", !liq3.MatchesFilter(filterObj.Filter));
		}

		public void TestEntryTypeList()
		{
			var filter = new CourtesyNoticesOfLiquidationFilterStripBusinessObject();
			var result = filter.EntryTypeList;
			AssertEquals("ExWarehouse type entry type should be there", true, result.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals("Other type is available", true, result.ContainsCode(EntryTypeList.Codes.ConsumptionFreeDutiable));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.ImmediateExportation));
			AssertEquals(false, result.ContainsCode(EntryTypeList.Codes.ImmediateTransportation));
			AssertEquals("Drawback Summary entry types should be removed", false, result.ContainsCode(EntryTypeList.Codes.DirectIdentificationManufacturingDrawback));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CourtesyNoticesOfLiquidationFilterStripBusinessObject();
	}
}
