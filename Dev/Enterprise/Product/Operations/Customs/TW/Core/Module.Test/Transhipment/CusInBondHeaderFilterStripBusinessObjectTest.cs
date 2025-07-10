using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Transhipment.Module.Test
{
	[TestedType(typeof(CusInBondHeaderFilterStripBusinessObject))]
	sealed class CusInBondHeaderFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CusInBondHeaderFilterStripBusinessObject();
		}

		public void TestETAFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ETA = ZDateTime.Today.AddDays(-20);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_ETA = ZDateTime.Today.AddDays(-10);
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_ETA = ZDateTime.Today;
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var etaFilter = (ModuleDateFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ETA];
			etaFilter.IsActive = true;
			etaFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			etaFilter.Property2 = ZDateTime.Today.AddDays(-30); //to-date
			AssertEquals("Should NOT have found all created test Inbonds", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header3.MatchesFilter(filterObj.Filter));
			etaFilter.Property1 = ZDateTime.Today.AddDays(-20);
			etaFilter.Property2 = ZDateTime.Today; //to-date
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			etaFilter.Property1 = ZDateTime.Today.AddDays(-12);
			etaFilter.Property2 = ZDateTime.Today.AddDays(-5); //to-date
			AssertEquals("Should NOT have found all created test Inbonds", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header3.MatchesFilter(filterObj.Filter));
		}

		public void TestImporterFilter()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMPCHI";
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "IMPLAX";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_OA_Importer = importer1.MainAddress.PK;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_OA_Importer = importer2.MainAddress.PK;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_OA_Importer = importer1.MainAddress.PK;
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var importerFilter = (ModuleGuidFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.Importer];
			importerFilter.IsActive = true;
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			importerFilter.Property = importer1.PK;
			AssertEquals("Filtering for Importer 1 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 1 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 1 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			importerFilter.Property = importer2.PK;
			AssertEquals("Filtering for Importer 2 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 2 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 2 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
		}

		public void TestCarrier()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMPCHI";
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "IMPLAX";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_OH_Carrier = importer1.PK;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_OH_Carrier = importer2.PK;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_OH_Carrier = importer1.PK;
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var importerFilter = (ModuleGuidFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.Carrier];
			importerFilter.IsActive = true;
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			importerFilter.Property = importer1.PK;
			AssertEquals("Filtering for Importer 1 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 1 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 1 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			importerFilter.Property = importer2.PK;
			AssertEquals("Filtering for Importer 2 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 2 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 2 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
		}

		public void TestLoadingPortFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_RL_NKImportLoadPort = "60243";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_RL_NKImportLoadPort = "52120";
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_RL_NKImportLoadPort = "60243";
			var header4 = Factory.New<CusInBondHeader>();
			header4.BH_RL_NKImportLoadPort = "79325";
			var header5 = Factory.New<CusInBondHeader>();
			header5.BH_RL_NKImportLoadPort = "22590";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var loadingPortFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.LoadingPort];
			loadingPortFilter.IsActive = true;
			loadingPortFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header5.MatchesFilter(filterObj.Filter));
			loadingPortFilter.Property = "60243";
			AssertEquals("Filtering for Loading Port 60243 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			loadingPortFilter.Property = "52120";
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			loadingPortFilter.Property = "22590";
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should find Inbond (header5).", true, header5.MatchesFilter(filterObj.Filter));
		}

		public void TestVoyageNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusInBondHeader>();
			header1.BH_VoyageNumber = "123456";
			var header2 = Factory.NewWithValidTestData<CusInBondHeader>();
			header2.BH_VoyageNumber = "111111";
			var header3 = Factory.NewWithValidTestData<CusInBondHeader>();
			header3.BH_VoyageNumber = "666666";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var vesselREGFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.VesselREG];
			vesselREGFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			vesselREGFilter.IsActive = true;
			vesselREGFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			vesselREGFilter.Property = "1";
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header3.MatchesFilter(filterObj.Filter));
			vesselREGFilter.Property = "2";
			AssertEquals("Should NOT have found all created test Inbonds", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header3.MatchesFilter(filterObj.Filter));
			vesselREGFilter.Property = "6";
			AssertEquals("Should NOT have found all created test Inbonds", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
		}

		public void TestEntryNumberFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusInBondHeader>();
			header1.ReceiptOffice = "1A";
			header1.UnladingOffice = "BB";
			header1.BH_ReleaseStatus = "A28";
			header1.TW_BoxNumber = "123";
			var header2 = Factory.NewWithValidTestData<CusInBondHeader>();
			header2.ReceiptOffice = "1A";
			header2.UnladingOffice = "CC";
			header2.BH_ReleaseStatus = "A28";
			header2.TW_BoxNumber = "123";
			var header3 = Factory.NewWithValidTestData<CusInBondHeader>();
			header3.ReceiptOffice = "AA";
			header3.UnladingOffice = "BB";
			header3.BH_ReleaseStatus = "A28";
			header3.TW_BoxNumber = "123";
			header1.AllocateEntryNumber();
			header2.AllocateEntryNumber();
			header3.AllocateEntryNumber();
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var entryNumberFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.EntryNumber];
			entryNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			entryNumberFilter.IsActive = true;
			entryNumberFilter.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header3.MatchesFilter(filterObj.Filter));
			entryNumberFilter.Property = "1";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header3.MatchesFilter(filterObj.Filter));
			entryNumberFilter.Property = "2";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header3.MatchesFilter(filterObj.Filter));
			entryNumberFilter.Property = "A";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header3.MatchesFilter(filterObj.Filter));
			var header4 = Factory.NewWithValidTestData<CusInBondHeader>();
			Factory.Save();
			entryNumberFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header4.MatchesFilter(filterObj.Filter));
			entryNumberFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header4.MatchesFilter(filterObj.Filter));
		}

		public void TestImportMasterBillFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusInBondHeader>();
			header1.ArrivalBill.B0_MasterBillNumber = "123456789";
			var header2 = Factory.NewWithValidTestData<CusInBondHeader>();
			header2.MovementBill.B0_MasterBillNumber = "123456789";
			var header3 = Factory.NewWithValidTestData<CusInBondHeader>();
			header3.ArrivalBill.B0_MasterBillNumber = "12345";
			var header4 = Factory.NewWithValidTestData<CusInBondHeader>();
			header4.ArrivalBill.B0_MasterBillNumber = "ABCD";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var importMasterBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ImportMasterBill];
			importMasterBill.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			importMasterBill.IsActive = true;
			importMasterBill.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header4.MatchesFilter(filterObj.Filter));
			importMasterBill.Property = "1";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header4.MatchesFilter(filterObj.Filter));
			importMasterBill.Property = "2";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header4.MatchesFilter(filterObj.Filter));
			importMasterBill.Property = "A";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header4.MatchesFilter(filterObj.Filter));
		}

		public void TestImportHouseBillFilter()
		{
			var header1 = Factory.NewWithValidTestData<CusInBondHeader>();
			header1.ArrivalBill.B0_HouseBillNumber = "123456789";
			var header2 = Factory.NewWithValidTestData<CusInBondHeader>();
			header2.MovementBill.B0_HouseBillNumber = "123456789";
			var header3 = Factory.NewWithValidTestData<CusInBondHeader>();
			header3.ArrivalBill.B0_HouseBillNumber = "12345";
			var header4 = Factory.NewWithValidTestData<CusInBondHeader>();
			header4.ArrivalBill.B0_HouseBillNumber = "ABCD";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var importHouseBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ImportHouseBill];
			importHouseBill.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			importHouseBill.IsActive = true;
			importHouseBill.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header4.MatchesFilter(filterObj.Filter));
			importHouseBill.Property = "1";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header4.MatchesFilter(filterObj.Filter));
			importHouseBill.Property = "2";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header4.MatchesFilter(filterObj.Filter));
			importHouseBill.Property = "A";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header4.MatchesFilter(filterObj.Filter));
		}

		public void TestJobReferenceFilter()
		{
			var header60243 = Factory.New<CusInBondHeader>();
			header60243.BH_JobReference = "60243";
			var header52120 = Factory.New<CusInBondHeader>();
			header52120.BH_JobReference = "52120";
			var header602432 = Factory.New<CusInBondHeader>();
			header602432.BH_JobReference = "60243";
			var header79325 = Factory.New<CusInBondHeader>();
			header79325.BH_JobReference = "79325";
			var header22590 = Factory.New<CusInBondHeader>();
			header22590.BH_JobReference = "22590";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var jobReferenceFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.BH_JobReference];
			jobReferenceFilter.IsActive = true;
			jobReferenceFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, header60243.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header52120.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header602432.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header79325.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header22590.MatchesFilter(filterObj.Filter));
			jobReferenceFilter.Property = "60243";
			AssertEquals("Filtering for Job Reference 60243 should find Inbond (header1).", true, header60243.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 60243 should NOT find Inbond (header2).", false, header52120.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 60243 should find Inbond (header3).", true, header602432.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 60243 should NOT find Inbond (header4).", false, header79325.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 60243 should NOT find Inbond (header5).", false, header22590.MatchesFilter(filterObj.Filter));
			jobReferenceFilter.Property = "52120";
			AssertEquals("Filtering for Job Reference 52120 should NOT find Inbond (header1).", false, header60243.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 52120 should find Inbond (header2).", true, header52120.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 52120 should NOT find Inbond (header3).", false, header602432.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 52120 should NOT find Inbond (header4).", false, header79325.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 52120 should NOT find Inbond (header5).", false, header22590.MatchesFilter(filterObj.Filter));
			jobReferenceFilter.Property = "22590";
			AssertEquals("Filtering for Job Reference 22590 should NOT find Inbond (header1).", false, header60243.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 22590 should NOT find Inbond (header2).", false, header52120.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 22590 should NOT find Inbond (header3).", false, header602432.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 22590 should NOT find Inbond (header4).", false, header79325.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference 22590 should find Inbond (header5).", true, header22590.MatchesFilter(filterObj.Filter));
		}
	}
}
