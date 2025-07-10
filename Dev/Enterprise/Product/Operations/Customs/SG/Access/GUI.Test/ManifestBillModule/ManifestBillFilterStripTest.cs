using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ManifestBillFilterStrip))]
	class ManifestBillFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestSGFields()
		{
			AssertNull(new ManifestBillFilterStrip()[ManifestBillFilterStrip.FilterConstants.Country]);
			AssertNotNull(new ManifestBillFilterStrip()[ManifestBillFilterStrip.SGFilterConstants.PartyIdentifier]);
			AssertNotNull(new ManifestBillFilterStrip()[ManifestBillFilterStrip.SGFilterConstants.PartyStatus]);
			AssertNotNull(new ManifestBillFilterStrip()[ManifestBillFilterStrip.SGFilterConstants.PayeeIndicator]);
			AssertNotNull(new ManifestBillFilterStrip()[ManifestBillFilterStrip.SGFilterConstants.TotalGST]);
			AssertNotNull(new ManifestBillFilterStrip()[ManifestBillFilterStrip.SGFilterConstants.TotalDuty]);
		}

		public void TestBillCycleAndBatchFilters()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_Nature = "IMP";
			var bill11 = header1.Bills.AddNew();
			bill11.ABL_AMA = header1.PK;
			var country111 = bill11;
			country111.CycleDate = new ZDateTime(2018, 4, 1);
			country111.CycleNumber = "1000001";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "C123457";
			header2.AMA_Nature = "EXP";
			var bill21 = header2.Bills.AddNew();
			bill21.ABL_AMA = header2.PK;
			var country211 = bill21;
			country211.BatchDate = new ZDateTime(2018, 4, 2);
			country211.BatchNumber = "1000002";
			Factory.Save();
			var filterStrip = new ManifestBillFilterStrip();
			var filterCycleNumber = (ModuleTextFilter)filterStrip[ManifestBillFilterStrip.SGFilterConstants.BillCycleNumber];
			filterCycleNumber.IsActive = true;
			filterCycleNumber.Property = "1000001";
			Assert(bill11.MatchesFilter(filterStrip.Filter));
			Assert(!bill21.MatchesFilter(filterStrip.Filter));
			filterCycleNumber.IsActive = false;
			var filterCycleDate = (ModuleDateFilter)filterStrip[ManifestBillFilterStrip.SGFilterConstants.BillCycleDate];
			filterCycleDate.IsActive = true;
			filterCycleDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			filterCycleDate.Property1 = new ZDateTime(2018, 4, 1);
			Assert(bill11.MatchesFilter(filterStrip.Filter));
			Assert(!bill21.MatchesFilter(filterStrip.Filter));
			filterCycleDate.IsActive = false;
			var filterBatchNumber = (ModuleTextFilter)filterStrip[ManifestBillFilterStrip.SGFilterConstants.BillBatchNumber];
			filterBatchNumber.IsActive = true;
			filterBatchNumber.Property = "1000002";
			Assert(!bill11.MatchesFilter(filterStrip.Filter));
			Assert(bill21.MatchesFilter(filterStrip.Filter));
			filterBatchNumber.IsActive = false;
			var filterBatchDate = (ModuleDateFilter)filterStrip[ManifestBillFilterStrip.SGFilterConstants.BillBatchDate];
			filterBatchDate.IsActive = true;
			filterBatchDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			filterBatchDate.Property1 = new ZDateTime(2018, 4, 2);
			Assert(!bill11.MatchesFilter(filterStrip.Filter));
			Assert(bill21.MatchesFilter(filterStrip.Filter));
		}

		public void TestPartyIDFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.SG_PartyID = "1";
			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.SG_PartyID = "2";
			Factory.Save();
			var filterObj = new ManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[ManifestBillFilterStrip.SGFilterConstants.PartyIdentifier];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestPartyStatusFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.SG_PartyStatus = "1";
			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.SG_PartyStatus = "2";
			Factory.Save();
			var filterObj = new ManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[ManifestBillFilterStrip.SGFilterConstants.PartyStatus];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestPayeeIndicatorFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.SG_PayeeIndicator = "1";
			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.SG_PayeeIndicator = "2";
			Factory.Save();
			var filterObj = new ManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[ManifestBillFilterStrip.SGFilterConstants.PayeeIndicator];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestTotalGSTFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.TaxAmount = 2.0;
			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.TaxAmount = 5.0;
			Factory.Save();
			var filterObj = new ManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[ManifestBillFilterStrip.SGFilterConstants.TotalGST];
			filter.IsActive = true;
			filter.Property = "2.0";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		public void TestTotalDutyFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			bill1.DutyAmount = 2.0;
			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			bill2.DutyAmount = 5.0;
			Factory.Save();
			var filterObj = new ManifestBillFilterStrip();
			var filter = (ASYCUDA.Module.GenAddOnTextNumericFilter)filterObj[ManifestBillFilterStrip.SGFilterConstants.TotalDuty];
			filter.IsActive = true;
			filter.Property = "2.0";
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
			dbOnlyQuery.AddToFilter(filterObj.Filter);
			var headers = Factory.Load<AsycudaBill>(dbOnlyQuery);
			AssertEquals(true, headers.Any());
			AssertEquals(true, headers.Any(x => x.PK == bill1.PK));
			AssertEquals(false, headers.Any(x => x.PK == bill2.PK));
		}

		public void TestGoodsTypeFilter()
		{
			var asyheader1 = Factory.New<AsycudaManifestHeader>();
			asyheader1.AMA_JobReference = "MAN000001";
			asyheader1.AMA_MasterBill = "123";
			asyheader1.AMA_ManifestType = "MGI";
			asyheader1.AMA_MessageStatus = "QUE";
			var bill1 = asyheader1.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			var packLine1 = pack1.PackedItem;
			packLine1.GoodsType = "1";
			bill1.DutyAmount = 2.0;
			var asyheader2 = Factory.New<AsycudaManifestHeader>();
			asyheader2.AMA_JobReference = "MAN000002";
			asyheader2.AMA_MasterBill = "456";
			var bill2 = asyheader2.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packLine2 = pack2.PackedItem;
			packLine2.GoodsType = "2";
			bill2.DutyAmount = 5.0;
			Factory.Save();
			var filterObj = new ManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[ManifestBillFilterStrip.FilterConstants.PackCustomsGoodsType];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(bill1.MatchesFilter(filterObj.Filter));
			Assert(!bill2.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ManifestBillFilterStrip();
		}
	}
}
