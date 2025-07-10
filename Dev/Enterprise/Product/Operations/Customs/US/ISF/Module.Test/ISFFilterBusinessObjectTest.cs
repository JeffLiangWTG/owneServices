using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Module.Testing
{
	[TestedType(typeof(ISFFilterBusinessObject))]
	sealed class ISFFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestThatAttribFieldsFilterFunctionsCorrectly()
		{
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.Exact, "one", "Custom Attribute 1", true, false, false);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.NotEqual, "one", "Custom Attribute 1", false, true, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.StartsWith, "o", "Custom Attribute 1", true, false, false);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.NotStartsWith, "o", "Custom Attribute 1", false, true, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.Contains, "i", "Custom Attribute 1", false, true, false);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.NotContain, "i", "Custom Attribute 1", true, false, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.IsBlank, "", "Custom Attribute 1", false, false, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.IsNotBlank, "", "Custom Attribute 1", true, true, false);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.Exact, "two", "Custom Attribute 2", true, false, false);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.NotEqual, "two", "Custom Attribute 2", false, true, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.StartsWith, "t", "Custom Attribute 2", true, false, false);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.NotStartsWith, "t", "Custom Attribute 2", false, true, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.Contains, "o", "Custom Attribute 2", true, true, false);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.NotContain, "o", "Custom Attribute 2", false, false, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.IsBlank, "", "Custom Attribute 2", false, false, true);
			AssertEqualsChecksThatFiltersProduceTheCorrectResults(ModuleTextFilter.ComparisonConstants.IsNotBlank, "", "Custom Attribute 2", true, true, false);
		}

		public void TestSellingPartyOrgList()
		{
			var filterObj = new ISFFilterBusinessObject();
			var sellingBuytingPartyFilter = (ModuleGuidsFilter)filterObj[ISFFilterBusinessObject.Constants.SellingBuyingParty];
			AssertEquals("Selling party should be type of shipper", typeof(ConsignorCollection), sellingBuytingPartyFilter.List1.GetType());
			AssertEquals("Buying party should be type of consigee", typeof(ConsigneeCollection), sellingBuytingPartyFilter.List2.GetType());
		}

		public void TestShipToPartyFilterSearchConsigneesOn2ndParameter()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "IMPORTERCONSIGNEE";
			organisation.OH_IsConsignor = false;
			organisation.OH_IsConsignee = true;
			Factory.Save();
			var filterObj = new ISFFilterBusinessObject();
			var shipToPartyFilter = (ModuleGuidsFilter)filterObj[ISFFilterBusinessObject.Constants.ManufacturerShipToParty];
			shipToPartyFilter.Property2 = organisation.PK;
			AssertEquals("No shipper error", false, shipToPartyFilter.Notifications.HasMessageErrors());
		}

		public void TestAcceptedDateFilters()
		{
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF3001001";
			header1.BF_FirstAcceptedDate = new ZDateTime(2009, 10, 11);
			header1.BF_LastAcceptedDate = new ZDateTime(2009, 10, 30);
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF3001002";
			header2.BF_FirstAcceptedDate = new ZDateTime(2009, 11, 11);
			header2.BF_LastAcceptedDate = new ZDateTime(2009, 11, 30);
			Factory.Save();
			var isfFilterObj = new ISFFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.FirstAccepted];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 10, 10);
			dateFilter.Property2 = new ZDateTime(2009, 11, 12);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 11, 10);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.LastAccepted];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 10, 29);
			dateFilter.Property2 = new ZDateTime(2009, 11, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 11, 10);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
		}

		public void TestOtherReferenceDataFilters()
		{
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF3001001";
			var bill1 = header1.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill1.BB_BillNum = "OB1";
			bill1.BB_MatchDate = new ZDateTime(2009, 6, 29);
			var bill2 = header1.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill2.BB_BillNum = "HB2";
			bill2.BB_MatchDate = new ZDateTime(2009, 7, 30);
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF3001002";
			var bill3 = header2.ReferenceDatas.AddNew();
			bill3.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill3.BB_BillNum = "OB3";
			bill3.BB_MatchDate = new ZDateTime(2009, 6, 29);
			var bill4 = header2.ReferenceDatas.AddNew();
			bill4.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill4.BB_BillNum = "HB4";
			bill4.BB_MatchDate = new ZDateTime(2009, 8, 5);
			var header3 = Factory.New<CusISFHeader>();
			header3.BF_JobReference = "ISF3001003";
			var bill5 = header3.ReferenceDatas.AddNew();
			bill5.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill5.BB_BillNum = "OB5";
			var bill6 = header3.ReferenceDatas.AddNew();
			bill6.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill6.BB_BillNum = "HB5";
			var header4 = Factory.New<CusISFHeader>();
			header4.BF_JobReference = "ISF3001004";
			Factory.Save();
			var isfFilterObj = new ISFFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.MatchedDate];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 8, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", false, header4.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 8, 1);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", false, header4.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 9, 1);
			dateFilter.Property2 = new ZDateTime(2009, 9, 30);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", false, header4.MatchesFilter(isfFilterObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", false, header4.MatchesFilter(isfFilterObj.Filter));
			isfFilterObj = new ISFFilterBusinessObject();
			var textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.OceanBill];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4.MatchesFilter(isfFilterObj.Filter));
			isfFilterObj = new ISFFilterBusinessObject();
			textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.HouseBill];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4.MatchesFilter(isfFilterObj.Filter));
			isfFilterObj = new ISFFilterBusinessObject();
			textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.MasterBill];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4.MatchesFilter(isfFilterObj.Filter));
			var header5 = Factory.New<CusISFHeader>();
			header5.BF_JobReference = "ISF3001005";
			var header5_bill1 = header5.ReferenceDatas.AddNew();
			header5_bill1.BB_BillType = BillTypeList.Codes.ISFBondNumber;
			header5_bill1.BB_BillNum = "123456";
			var header5_bill2 = header5.ReferenceDatas.AddNew();
			header5_bill2.BB_BillType = BillTypeList.Codes.USCBPEntryNumber;
			header5_bill2.BB_BillNum = "15454";
			var header5_bill3 = header5.ReferenceDatas.AddNew();
			header5_bill3.BB_BillType = BillTypeList.Codes.SuretyCode;
			header5_bill3.BB_BillNum = "891";
			header5_bill3.BB_FirstMatchedDate = ZDateTime.Now;
			Factory.Save();
			isfFilterObj = new ISFFilterBusinessObject();
			textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ISFBondNumber];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", false, header5.MatchesFilter(isfFilterObj.Filter));
			isfFilterObj = new ISFFilterBusinessObject();
			textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.CBPEntryNumber];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", false, header5.MatchesFilter(isfFilterObj.Filter));
			isfFilterObj = new ISFFilterBusinessObject();
			textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.BondSuretyCode];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", false, header5.MatchesFilter(isfFilterObj.Filter));
			isfFilterObj = new ISFFilterBusinessObject();
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.FirstMatchedDate];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 8, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", false, header4.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", false, header5.MatchesFilter(isfFilterObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header5.MatchesFilter(isfFilterObj.Filter));
		}

		public void TestBondReferenceFilter()
		{
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF3001001";
			header1.BF_BondReferenceNumber = "BOND123";
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF3001002";
			header2.BF_BondReferenceNumber = "BOND345";
			var header3 = Factory.New<CusISFHeader>();
			header3.BF_JobReference = "ISF3001003";
			Factory.Save();
			var isfFilterObj = new ISFFilterBusinessObject();
			var textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.BondReference];
			textFilter.IsActive = true;
			textFilter.Property = "BOND";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "BOND1";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "BOND3";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 not match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 not match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
		}

		public void TestCarnetReferenceFilter()
		{
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF3001001";
			var carnet1 = header1.ReferenceDatas.AddNew();
			carnet1.BB_BillType = BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber;
			carnet1.BB_BillNum = "AU1234";
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF3001002";
			var carnet2 = header2.ReferenceDatas.AddNew();
			carnet2.BB_BillType = BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber;
			carnet2.BB_BillNum = "AU5678";
			var header3 = Factory.New<CusISFHeader>();
			header3.BF_JobReference = "ISF3001003";
			Factory.Save();
			var isfFilterObj = new ISFFilterBusinessObject();
			var textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.CarnetReference];
			textFilter.IsActive = true;
			textFilter.Property = "AU";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "AU1";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "AU5";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("header1 not match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 not match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 not match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
		}

		public void TestRouteFilters()
		{
			CusISFHeader header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF3001001";
			Transport transport1 = header1.Transports.AddNew();
			transport1.JW_Vessel = "ABC VESSEL";
			transport1.JW_VoyageFlight = "V32D4";
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport1.JW_ETD = new ZDateTime(2009, 6, 29);
			transport1.JW_ETA = new ZDateTime(2009, 7, 29);
			transport1.JW_ATD = new ZDateTime(2009, 6, 30);
			transport1.JW_ATA = new ZDateTime(2009, 7, 30);
			Transport transport2 = header1.Transports.AddNew();
			transport2.JW_Vessel = "ABC VESSEL";
			transport2.JW_VoyageFlight = "V32D4";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "CATOR";
			transport2.JW_ETD = new ZDateTime(2009, 7, 30);
			transport2.JW_ETA = new ZDateTime(2009, 8, 1);
			transport2.JW_ATD = new ZDateTime(2009, 7, 31);
			transport2.JW_ATA = new ZDateTime(2009, 8, 2);
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF3001002";
			Transport transport3 = header2.Transports.AddNew();
			transport3.JW_Vessel = "ABC BOAT";
			transport3.JW_VoyageFlight = "V368D";
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "USCHI";
			transport3.JW_ETD = new ZDateTime(2009, 6, 29);
			transport3.JW_ETA = new ZDateTime(2009, 7, 29);
			transport3.JW_ATD = new ZDateTime(2009, 6, 30);
			transport3.JW_ATA = new ZDateTime(2009, 7, 30);
			Transport transport4 = header2.Transports.AddNew();
			transport4.JW_Vessel = "GDS VESSEL";
			transport4.JW_VoyageFlight = "G32D232";
			transport4.JW_RL_NKLoadPort = "USCHI";
			transport4.JW_RL_NKDiscPort = "CATOR";
			transport4.JW_ETD = new ZDateTime(2009, 8, 5);
			transport4.JW_ETA = new ZDateTime(2009, 8, 7);
			transport4.JW_ATD = new ZDateTime(2009, 8, 6);
			transport4.JW_ATA = new ZDateTime(2009, 8, 8);
			Factory.Save();
			ISFFilterBusinessObject isfFilterObj = new ISFFilterBusinessObject();
			ModuleTextFilter textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.Vessel];
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			textFilter.Property = "ABC V";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "ABC";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "ZZZ";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.IsActive = false;
			textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.VoyageFlight];
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			textFilter.Property = "V32D";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "V3";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "ZZZ";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.IsActive = false;
			ModuleLocationFilter locationFilter = (ModuleLocationFilter)isfFilterObj[ISFFilterBusinessObject.Constants.LoadDischarge];
			AssertEquals("Load", locationFilter.ItemDescription1.Caption);
			AssertEquals("Discharge", locationFilter.ItemDescription2.Caption);
			locationFilter.IsActive = true;
			locationFilter.Property1 = ZString.Empty;
			locationFilter.Property2 = "US";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			locationFilter.Property1 = "NZAKL";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			locationFilter.Property2 = "USNYK";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			locationFilter.IsActive = false;
			CusISFHeader header3 = Factory.New<CusISFHeader>();
			header3.BF_JobReference = "ISF3001003";
			Transport transport5linked = header3.Transports.AddNew();
			transport5linked.JW_Vessel = "ABC VESSEL";
			transport5linked.JW_VoyageFlight = "V32D5";
			transport5linked.JW_RL_NKLoadPort = "USLAX";
			transport5linked.JW_RL_NKDiscPort = "AEAUH";
			transport5linked.JW_IsLinked = true;
			transport5linked.Sailing.Destination.JB_A_ARV = new ZDateTime(2009, 10, 2);
			transport5linked.Sailing.Destination.JB_E_ARV = new ZDateTime(2009, 10, 1);
			transport5linked.Sailing.Origin.JA_E_DEP = new ZDateTime(2009, 9, 28);
			transport5linked.Sailing.Origin.JA_A_DEP = new ZDateTime(2009, 9, 28);
			Factory.Save();
			ModuleDateFilter dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ETD];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 8, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 8, 1);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 9, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ETA];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 8, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 8, 2);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 9, 1);
			dateFilter.Property2 = new ZDateTime(2009, 10, 2);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ATD];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 8, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 8, 1);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 9, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ATA];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = new ZDateTime(2009, 7, 30);
			dateFilter.Property2 = new ZDateTime(2009, 8, 30);
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 8, 3);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.Property1 = new ZDateTime(2009, 9, 1);
			dateFilter.Property2 = new ZDateTime(2009, 10, 3);
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			transport1.JW_ETD = new ZDateTime(2009, 6, 29);
			transport1.JW_ETA = ZDateTime.Empty;
			transport1.JW_ATD = ZDateTime.Empty;
			transport1.JW_ATA = new ZDateTime(2009, 7, 30);
			transport2.JW_ETD = new ZDateTime(2009, 7, 30);
			transport2.JW_ETA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;
			transport2.JW_ATA = new ZDateTime(2009, 8, 2);
			transport3.JW_ETD = ZDateTime.Empty;
			transport3.JW_ETA = new ZDateTime(2009, 7, 29);
			transport3.JW_ATD = new ZDateTime(2009, 6, 30);
			transport3.JW_ATA = ZDateTime.Empty;
			transport4.JW_ETD = ZDateTime.Empty;
			transport4.JW_ETA = new ZDateTime(2009, 8, 7);
			transport4.JW_ATD = new ZDateTime(2009, 8, 6);
			transport4.JW_ATA = ZDateTime.Empty;
			transport5linked.Sailing.Destination.JB_A_ARV = new ZDateTime(2009, 8, 2);
			transport5linked.Sailing.Destination.JB_E_ARV = ZDateTime.Empty;
			transport5linked.Sailing.Origin.JA_E_DEP = new ZDateTime(2009, 7, 30);
			transport5linked.Sailing.Origin.JA_A_DEP = ZDateTime.Empty;
			Factory.Save();
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ETD];
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			dateFilter.IsActive = true;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ETA];
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			dateFilter.IsActive = true;
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ATD];
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			dateFilter.IsActive = true;
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
			dateFilter = (ModuleDateFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ATA];
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			dateFilter.IsActive = true;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			dateFilter.IsActive = false;
		}

		public void TestJobNumberFilter()
		{
			var filters = new ISFFilterBusinessObject();
			var jobNumberFilter = (ModuleFountainFilter)filters[ISFFilterBusinessObject.Constants.JobNumber];
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.Property = "1";
			AssertEquals("ISF0000001", jobNumberFilter.Property);
		}

		public void TestFiltersCategory()
		{
			var filters = new ISFFilterBusinessObject();
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.JobNumber], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.OwnerReference], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.CustomsReference], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.DiscardedCustomsReference], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.Status], FilterCategories.StatusAndFlags);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.EntryType], FilterCategories.ModesAndTypes);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ShipmentType], FilterCategories.ModesAndTypes);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.SCAC], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ImporterCodeType], FilterCategories.ModesAndTypes);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ImporterCode], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ConsigneeCodeType], FilterCategories.ModesAndTypes);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ConsigneeCode], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ImporterDOB], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.UnloadPort], FilterCategories.Locations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.DeliveryPort], FilterCategories.Locations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.CountryOfIssue], FilterCategories.Locations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.BondHolder], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.BondSuretyCode], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ISFBondNumber], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.MasterBill], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.HouseBill], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.OceanBill], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.BillStatus], FilterCategories.StatusAndFlags);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.DISStatus], FilterCategories.StatusAndFlags);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.CBPEntryNumber], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.NoOfHTSDigits], FilterCategories.ModesAndTypes);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.SendEquipment], FilterCategories.StatusAndFlags);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.MergeStyle], FilterCategories.ModesAndTypes);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.Importer], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ManufacturerShipToParty], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.SellingBuyingParty], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ConsolidatorStuffingLocation], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.BookingParty], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.SendingAgent], FilterCategories.Organisations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ActionReasonCode], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.Vessel], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.VoyageFlight], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.LoadDischarge], FilterCategories.Locations);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ETD], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ETA], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ATD], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.ATA], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.FirstAccepted], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.LastAccepted], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.MatchedDate], FilterCategories.Dates);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.BondReference], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.CarnetReference], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.CustomAttribute1], FilterCategories.NumbersAndReferences);
			AssertFilterCategory(filters[ISFFilterBusinessObject.Constants.CustomAttribute2], FilterCategories.NumbersAndReferences);
		}

		public void TestSomeFiltersAreInvisibleForWeb()
		{
			Assert(!GetNewFilterStripBusinessObject().ModuleFilters[ISFFilterBusinessObject.Constants.Branch].IsPublishedOnWeb);
		}

		public void TestDiscardedCustomsReferenceFilter()
		{
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF3001001";
			var header1messgage1 = Factory.New<MQEDIMessage>();
			header1.Messages.Add(header1messgage1);
			header1messgage1.EM_ApplicationReference = "AU1234";
			header1messgage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			header1messgage1.EM_Status = EDIMessage.Status.Received;
			var header1messgage2 = Factory.New<MQEDIMessage>();
			header1.Messages.Add(header1messgage2);
			header1messgage2.EM_ApplicationReference = "UK5";
			header1messgage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			header1messgage2.EM_Status = EDIMessage.Status.Discarded;
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF3001002";
			var header2messgage1 = Factory.New<MQEDIMessage>();
			header2.Messages.Add(header2messgage1);
			header2messgage1.EM_ApplicationReference = "AU5678";
			header2messgage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			header2messgage1.EM_Status = EDIMessage.Status.Discarded;
			var header3 = Factory.New<CusISFHeader>();
			header3.BF_JobReference = "ISF3001003";
			var header3Messgage1 = Factory.New<MQEDIMessage>();
			header3.Messages.Add(header3Messgage1);
			header3Messgage1.EM_ApplicationReference = "UK5678";
			header3Messgage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			header3Messgage1.EM_Status = EDIMessage.Status.Discarded;
			Factory.Save();
			var isfFilterObj = new ISFFilterBusinessObject();
			var textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.DiscardedCustomsReference];
			AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
			AssertEquals("IsBlank", false, textFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals("IsNotBlank", false, textFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			AssertEquals("IsNotBlank", false, textFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			textFilter.IsActive = true;
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.Property = "AU";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header1 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "UK";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header1 match filter", true, header3.MatchesFilter(isfFilterObj.Filter));
			textFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			textFilter.Property = "UK5";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header1 match filter", false, header3.MatchesFilter(isfFilterObj.Filter));
		}

		public void TestContainerNumberFilter()
		{
			var header1 = Factory.New<CusISFHeader>();
			header1.BF_JobReference = "ISF3001001";
			var equipment1 = header1.Equipments.AddNew();
			equipment1.BE_ContainerNum = "AU1234";
			var header2 = Factory.New<CusISFHeader>();
			header2.BF_JobReference = "ISF3001002";
			var equipment2 = header2.Equipments.AddNew();
			equipment2.BE_ContainerNum = "AU5678";
			Factory.Save();
			var isfFilterObj = new ISFFilterBusinessObject();
			var textFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.ContainerNumber];
			AssertEquals(FilterCategories.NumbersAndReferences, textFilter.Category);
			textFilter.IsActive = true;
			textFilter.Property = "AU";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "AU1";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(isfFilterObj.Filter));
			textFilter.Property = "AU5";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(isfFilterObj.Filter));
		}

		public void TestBillStatus()
		{
			var header1_S1_S3 = Factory.New<CusISFHeader>();
			header1_S1_S3.BF_JobReference = "ISF3001001";
			var header1_bill1_S1 = header1_S1_S3.ReferenceDatas.AddNew();
			header1_bill1_S1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			header1_bill1_S1.BB_BillNum = "HB1232122";
			header1_bill1_S1.BB_CustomsStatus = DispositionCodeList.Codes.S1;
			var header1_bill2_S3 = header1_S1_S3.ReferenceDatas.AddNew();
			header1_bill2_S3.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			header1_bill2_S3.BB_BillNum = "HB5687985";
			header1_bill2_S3.BB_CustomsStatus = DispositionCodeList.Codes.S3;
			var header2_S1 = Factory.New<CusISFHeader>();
			header2_S1.BF_JobReference = "ISF3001002";
			var header2_bill1_S1 = header2_S1.ReferenceDatas.AddNew();
			header2_bill1_S1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			header2_bill1_S1.BB_BillNum = "HB6358799";
			header2_bill1_S1.BB_CustomsStatus = DispositionCodeList.Codes.S1;
			var header3_S5 = Factory.New<CusISFHeader>();
			header3_S5.BF_JobReference = "ISF3001003";
			var header3_bill1_S5 = header3_S5.ReferenceDatas.AddNew();
			header3_bill1_S5.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			header3_bill1_S5.BB_BillNum = "HB6358800";
			header3_bill1_S5.BB_CustomsStatus = DispositionCodeList.Codes.S5;
			var header3_bill2_MAWB = header3_S5.ReferenceDatas.AddNew();
			header3_bill2_MAWB.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			header3_bill2_MAWB.BB_BillNum = "HB6358801";
			var header4_NoStatus = Factory.New<CusISFHeader>();
			header4_NoStatus.BF_JobReference = "ISF3001004";
			var header4_bill1_NoStatus = header4_NoStatus.ReferenceDatas.AddNew();
			header4_bill1_NoStatus.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			header4_bill1_NoStatus.BB_BillNum = "HB6358803";
			var header5_S3_Blank = Factory.New<CusISFHeader>();
			header5_S3_Blank.BF_JobReference = "ISF3001005";
			var header5_bill1_S3 = header5_S3_Blank.ReferenceDatas.AddNew();
			header5_bill1_S3.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			header5_bill1_S3.BB_BillNum = "HB6358804";
			header5_bill1_S3.BB_CustomsStatus = DispositionCodeList.Codes.S3;
			var header5_bill2_NoStatus = header5_S3_Blank.ReferenceDatas.AddNew();
			header5_bill2_NoStatus.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			header5_bill2_NoStatus.BB_BillNum = "HB6358805";
			Factory.Save();
			var isfFilterObj = new ISFFilterBusinessObject();
			var billFilter = (ModuleTextFilter)isfFilterObj[ISFFilterBusinessObject.Constants.BillStatus];
			billFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			billFilter.IsActive = true;
			billFilter.Property = DispositionCodeList.Codes.S1;
			AssertEquals("header1 match filter", true, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 should not match filter", false, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 should not match filter", false, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 should not match filter", false, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.Property = DispositionCodeList.Codes.S3;
			AssertEquals("header1 match filter", true, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 should not match filter", false, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 should not match filter", false, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 should not match filter", false, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", true, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.Property = DispositionCodeList.Codes.S4;
			AssertNoWarning(billFilter.PropertyInfo, ListValidation.InvalidCodeMessage);
			AssertEquals("header1 should not match filter", false, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 should not match filter", false, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 should not match filter", false, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 should not match filter", false, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 should not match filter", false, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.Property = DispositionCodeList.Codes.S5;
			AssertEquals("header1 should not match filter", false, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 should not match filter", false, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 should not match filter", false, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 should not match filter", false, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.Property = ISFStatusHelper.Multiple;
			AssertEquals("header1 match filter", true, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 should not match filter", false, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 should not match filter", false, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 should not match filter", false, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 should not match filter", false, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			AssertEquals("header1 should not match filter", false, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", true, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.Property = DispositionCodeList.Codes.S5;
			AssertEquals("header1 should not match filter", true, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 should not match filter", true, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 should not match filter, because Master Bill ignored", false, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", true, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.Property = DispositionCodeList.Codes.S1;
			AssertEquals("header1 should not match filter", false, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 should not match filter", false, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", true, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals("header1 should not match filter", false, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 should not match filter", false, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 should not match filter", false, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 match filter", true, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 should not match filter", false, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals("header1 match filter", true, header1_S1_S3.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header2 match filter", true, header2_S1.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header3 match filter", true, header3_S5.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header4 should not match filter", false, header4_NoStatus.MatchesFilter(isfFilterObj.Filter));
			AssertEquals("header5 match filter", true, header5_S3_Blank.MatchesFilter(isfFilterObj.Filter));
			billFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			billFilter.Property = "Z!";
			AssertHasWarning(billFilter.PropertyInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestDISStatusFilter()
		{
			var header1 = Factory.New<CusISFHeader>();
			var doc1 = header1.RequiredDocuments.AddNew();
			var docAddInfo1 = doc1.AddInfos.AddNew();
			docAddInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo1.EX_Status = ZString.Empty;
			var header2 = Factory.New<CusISFHeader>();
			var doc2 = header2.RequiredDocuments.AddNew();
			var docAddInfo2 = doc2.AddInfos.AddNew();
			docAddInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo2.EX_Status = ZString.Empty;
			var doc21 = header2.RequiredDocuments.AddNew();
			var docAddInfo21 = doc21.AddInfos.AddNew();
			docAddInfo21.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo21.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			var header3 = Factory.New<CusISFHeader>();
			var doc3 = header3.RequiredDocuments.AddNew();
			var docAddInfo3 = doc3.AddInfos.AddNew();
			docAddInfo3.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo3.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			var doc31 = header3.RequiredDocuments.AddNew();
			var docAddInfo31 = doc31.AddInfos.AddNew();
			docAddInfo31.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo31.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			var header4 = Factory.New<CusISFHeader>();
			var doc4 = header4.RequiredDocuments.AddNew();
			var docAddInfo4 = doc4.AddInfos.AddNew();
			docAddInfo4.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo4.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			var doc41 = header4.RequiredDocuments.AddNew();
			var docAddInfo41 = doc41.AddInfos.AddNew();
			docAddInfo41.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			docAddInfo41.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.ARS;
			Factory.Save();
			var bizObj = new ISFFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[ISFFilterBusinessObject.Constants.DISStatus];
			filter.IsActive = true;
			filter.Property = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			Assert(!header1.MatchesFilter(bizObj.Filter));
			Assert(header2.MatchesFilter(bizObj.Filter));
			Assert(header3.MatchesFilter(bizObj.Filter));
			Assert(header4.MatchesFilter(bizObj.Filter));
			filter.Property = Enterprise.Customs.Common.US.DIS.StatusList.Codes.ARS;
			Assert(!header1.MatchesFilter(bizObj.Filter));
			Assert(!header2.MatchesFilter(bizObj.Filter));
			Assert(!header3.MatchesFilter(bizObj.Filter));
			Assert(header4.MatchesFilter(bizObj.Filter));
		}

		public void TestDISStatusList()
		{
			var obj = GetNewFilterStripBusinessObject() as ISFFilterBusinessObject;
			Assert(!obj.DISStatusList.ContainsCode(Enterprise.Customs.Common.US.DIS.StatusList.Codes.MUL));
		}

		public void TestShipmentNumberFilter()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_UniqueConsignRef = "S00015001";
			shipment2.JS_UniqueConsignRef = "S00015002";
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.Shipments.Add(shipment1);
			consol2.Shipments.Add(shipment2);
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment1.PK;
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment2.PK;
			Factory.Save();
			var creator1 = ISFFromShipmentCreator.GetCreatorForShipment(shipment1);
			var creator2 = ISFFromShipmentCreator.GetCreatorForShipment(shipment2);
			var header1 = creator1.Create(Factory);
			Factory.Save();
			var header2 = creator2.Create(Factory);
			Factory.Save();
			var bizObj = new ISFFilterBusinessObject();
			var filter = (ModuleTextFilter)bizObj[ISFFilterBusinessObject.Constants.ShipmentNumber];
			filter.IsActive = true;
			filter.Property = "S00015001";
			Assert("Expected ISF header to match the search filter", header1.MatchesFilter(bizObj.Filter));
			Assert("Expected ISF header not to match the search filter", !header2.MatchesFilter(bizObj.Filter));
		}

		public void TestCustomFieldFilter()
		{
			var filterCollection = new ISFFilterBusinessObject().ModuleFilters;
			CombineAssertions(() =>
			{
				AssertNull("stringField", filterCollection["stringField"]);
				AssertNull("intField", filterCollection["intField"]);
				AssertNull("dateTimeField", filterCollection["dateTimeField"]);
				AssertNull("boolField", filterCollection["boolField"]);
				var template = CreateWorkflowTemplate(WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode);
				AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
				AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
				AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
				AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
				Factory.Save();
				WorkflowCustomFieldsFilter.ClearCache();
				filterCollection = new ISFFilterBusinessObject().ModuleFilters;
				AssertEquals("stringField added", typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
				AssertEquals("intField added", typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
				AssertEquals("dateTimeField added", typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
				AssertEquals("Workflow Flags added", typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ISFFilterBusinessObject();

		void AssertEqualsChecksThatFiltersProduceTheCorrectResults(ZString comparisonOperator, ZString filterProperty, ZString customAttribute, bool isHeaderOneMatch, bool isHeaderTwoMatch, bool isHeaderThreeMatch)
		{
			var header = Factory.New<CusISFHeader>();
			header.CustomAttribute1 = "one";
			header.CustomAttribute2 = "two";
			var header2 = Factory.New<CusISFHeader>();
			header2.CustomAttribute1 = "first";
			header2.CustomAttribute2 = "second";
			var header3 = Factory.New<CusISFHeader>();
			header3.CustomAttribute1 = "";
			header3.CustomAttribute2 = "";
			Factory.Save();
			var filter = new ISFFilterBusinessObject();
			var textFilter = (ModuleTextFilter)filter[customAttribute];
			textFilter.IsActive = true;
			textFilter.ComparisonOperator = comparisonOperator;
			textFilter.Property = filterProperty;
			AssertEquals("header match filter", isHeaderOneMatch, header.MatchesFilter(filter.Filter));
			AssertEquals("header match filter", isHeaderTwoMatch, header2.MatchesFilter(filter.Filter));
			AssertEquals("header match filter", isHeaderThreeMatch, header3.MatchesFilter(filter.Filter));
		}

		void AssertFilterCategory(ModuleFilter moduleFilter, FilterCategory category)
		{
			AssertEquals(category, moduleFilter.Category);
		}

		ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}
	}
}
