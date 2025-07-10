using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(USAMSBillFilterStrip))]
	sealed class USAMSBillFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestDateFilters()
		{
			AssertHeaderDateFilter(USAMSBillFilterStrip.FilterConstants.OriginalEsitmatedTime, CusInBondHeaderSchema.BH_ETA);
			AssertHeaderDateFilter(USAMSBillFilterStrip.FilterConstants.EstimatedDateofDeparture, CusInBondHeaderSchema.BH_FirstExportDate);
		}

		public void TestTextFilters()
		{
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.JobReference, CusInBondHeaderSchema.BH_JobReference);
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.CarrierSCAC, CusInBondHeaderSchema.BH_CarrierSCAC);
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.TransportMode, CusInBondHeaderSchema.BH_ImportTransportMode);
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.ImportingConveyanceName, CusInBondHeaderSchema.BH_ImportConveyanceName);
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.VoyageNumber, CusInBondHeaderSchema.BH_VoyageNumber);
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.PortOfUnladingSchD, CusInBondHeaderSchema.BH_PortUnladingDCode);
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.PortOfUnladingUNLOCO, CusInBondHeaderSchema.BH_RL_NKPortUnlading);
			AssertHeaderTextFilter(USAMSBillFilterStrip.FilterConstants.LoadPortUNLOCO, CusInBondHeaderSchema.BH_RL_NKImportLoadPort);
			AssertBillTextFilter(USAMSBillFilterStrip.FilterConstants.PortOfLadingSchK, CusInBondBillSchema.B0_PortOfLadingKCode);
			AssertBillTextFilter(USAMSBillFilterStrip.FilterConstants.PortOfLadingUNLOCO, CusInBondBillSchema.B0_RL_NKPortOfLading);
			AssertBillTextFilter(USAMSBillFilterStrip.FilterConstants.IssuerCode, CusInBondBillSchema.B0_IssuerCode);
			AssertBillTextFilter(USAMSBillFilterStrip.FilterConstants.BillStatus, CusInBondBillSchema.B0_BillStatus);
			AssertMoveDetailTextFilter(USAMSBillFilterStrip.FilterConstants.FilingStatus, CusInBondMoveDetailSchema.B9_CustomsStatus);
			AssertMoveDetailTextFilter(USAMSBillFilterStrip.FilterConstants.ManifestMessageStatus, CusInBondMoveDetailSchema.B9_MessageStatus);
		}

		void AssertHeaderDateFilter(ZString filterName, SchemaDateTimeColumn schemaCol)
		{
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader[schemaCol] = ZDateTime.Today.AddDays(1);
			var inbondHeaderBill = inbondHeader.Bills.AddNew();
			inbondHeader.BH_ApplicationCode = "INB";
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1 = Factory.New<CusInBondHeader>();
			header1[schemaCol] = ZDateTime.Today.AddDays(1);
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header2[schemaCol] = ZDateTime.Today.AddDays(100);
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleDateFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", filterName, schemaCol.Name), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", filterName, schemaCol.Name), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", filterName, schemaCol.Name), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
		}

		void AssertHeaderTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader[schemaCol] = "1";
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var inbondHeaderBill = inbondHeader.Bills.AddNew();
			inbondHeader.BH_ApplicationCode = "INB";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header1[schemaCol] = "1";
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header2[schemaCol] = "2";
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", filterName, schemaCol.Name), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", filterName, schemaCol.Name), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", filterName, schemaCol.Name), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
		}

		void AssertBillTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var header1 = Factory.New<CusInBondHeader>();
			var header1Bill = header1.Bills.AddNew();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header1Bill[schemaCol] = "1";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header2Bill = header2.Bills.AddNew();
			header2Bill[schemaCol] = "2";
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", filterName, schemaCol.Name), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", filterName, schemaCol.Name), !header2Bill.MatchesFilter(filterObj.Filter));
		}

		void AssertMoveDetailTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var inbondHeaderBill = inbondHeader.Bills.AddNew();
			var inbondHeaderMoveDetail = inbondHeaderBill.MovementDetail;
			inbondHeaderMoveDetail[schemaCol] = "1";
			inbondHeader.BH_ApplicationCode = "INB";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1Bill = header1.Bills.AddNew();
			var header1MoveDetail = header1Bill.MovementDetail;
			header1MoveDetail[schemaCol] = "1";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header2Bill = header2.Bills.AddNew();
			var header2MoveDetail = header2Bill.MovementDetail;
			header2MoveDetail[schemaCol] = "2";
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", filterName, schemaCol.Name), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", filterName, schemaCol.Name), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", filterName, schemaCol.Name), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
		}

		public void TestLatestAMSDispositionFilter()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3U", "3U DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "3Z DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1W", "1W DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "83", "83 DESC", startDate, endDate);
			Factory.Save();
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var inbondHeaderBill = inbondHeader.Bills.AddNew();
			var inbondHeaderBillDisposition = inbondHeaderBill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			inbondHeader.BH_ApplicationCode = "INB";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1Bill = header1.Bills.AddNew();
			var header1BillDisposition = header1Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header2Bill = header2.Bills.AddNew();
			var header2BillDisposition1 = header2Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header2BillDisposition2 = header2Bill.DispositionCodes.AddNewIfNotExist("1W", new ZDateTime(2012, 4, 10));
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header3Bill = header3.Bills.AddNew();
			var header3BillDisposition1 = header3Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header3BillDisposition2 = header3Bill.DispositionCodes.AddNewIfNotExist("3Z", new ZDateTime(2012, 4, 10));
			var header3BillDisposition3 = header3Bill.DispositionCodes.AddNewIfNotExist("83", new ZDateTime(2012, 4, 20));
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleNkFilter)filterObj[USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition];
			filter.IsActive = true;
			filter.Property = "3U";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3U"), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3U"), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3U"), !header3Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3U"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			filter.Property = "1W";
			Assert(string.Format("Should not match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "1W"), !header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "1W"), header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "1W"), !header3Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "1W"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			filter.Property = "83";
			Assert(string.Format("Should not match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "83"), !header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "83"), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "83"), header3Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "83"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			filter.Property = "3Z";
			Assert(string.Format("Should not match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3Z"), !header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3Z"), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3Z"), !header3Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestAMSDisposition, "3Z"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
		}

		public void TestISFStatusFilter()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3U", "3U DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "3Z DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1W", "1W DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "83", "83 DESC", startDate, endDate);
			Factory.Save();
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var inbondHeaderBill = inbondHeader.Bills.AddNew();
			var inbondHeaderBillDisposition = inbondHeaderBill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var inbondHeaderBillDispositionNoneISF = inbondHeaderBill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 5, 1));
			inbondHeader.BH_ApplicationCode = "INB";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1Bill = header1.Bills.AddNew();
			var header1BillDisposition = header1Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header1BillDispositionNoneISF = header1Bill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 5, 1));
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header2Bill = header2.Bills.AddNew();
			var header2BillDisposition = header2Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header2BillDispositionNoneISF = header2Bill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 5, 1));
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header3Bill = header3.Bills.AddNew();
			var header3BillDisposition1 = header3Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header3BillDispositionNoneISF = header3Bill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 4, 15));
			var header3BillDisposition2 = header3Bill.DispositionCodes.AddNewIfNotExist("3Z", new ZDateTime(2012, 4, 10));
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[USAMSBillFilterStrip.FilterConstants.LatestISFDisposition];
			filter.IsActive = true;
			filter.Property = "3U";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header3Bill for '{0}' and '{1}' as the lastest ISF is different", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), !header3Bill.MatchesFilter(filterObj.Filter));
			filter.Property = "3Z";
			Assert(string.Format("Should not match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), !header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestISFDisposition, "3U"), header3Bill.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondStatusFilter()
		{
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var inbondHeaderBill = inbondHeader.Bills.AddNew();
			var inbondHeaderMoveHeader = inbondHeader.InBondMovementHeaders.AddNew();
			inbondHeaderMoveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			var inbondHeaderMoveDetail = inbondHeaderMoveHeader.MovementDetails.AddNew(inbondHeaderBill.PK);
			inbondHeaderMoveDetail.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearArrival;
			inbondHeader.BH_ApplicationCode = "INB";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1Bill = header1.Bills.AddNew();
			var header1MoveHeader = header1.InBondMovementHeaders.AddNew();
			header1MoveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			var header1MoveDetail = header1MoveHeader.MovementDetails.AddNew(header1Bill.PK);
			header1MoveDetail.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearArrival;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header2Bill = header2.Bills.AddNew();
			var header2MoveHeader = header2.InBondMovementHeaders.AddNew();
			header2MoveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			var header2MoveDetail = header2MoveHeader.MovementDetails.AddNew(header2Bill.PK);
			header2MoveDetail.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearDeparture;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header3Bill = header3.Bills.AddNew();
			var header3MoveHeader1 = header3.InBondMovementHeaders.AddNew();
			header3MoveHeader1.BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
			var header3MoveDetail1 = header3MoveHeader1.MovementDetails.AddNew(header3Bill.PK);
			header3MoveDetail1.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearDeparture;
			var header3MoveHeader2 = header3.InBondMovementHeaders.AddNew();
			header3MoveHeader2.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			var header3MoveDetail2 = header3MoveHeader2.MovementDetails.AddNew(header3Bill.PK);
			header3MoveDetail2.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearArrival;
			var header4 = Factory.New<CusInBondHeader>();
			header4.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header4Bill = header4.Bills.AddNew();
			var header4PTTMoveHeader = header4.PTTMovements.AddNew();
			header4PTTMoveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.PermitToTransfer;
			var header4PTTMoveDetail = header4PTTMoveHeader.MovementDetails.AddNew(header4Bill.PK);
			header4PTTMoveDetail.B9_CustomsStatus = AMSBillMessageStatusList.Codes.ClearArrival;
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[USAMSBillFilterStrip.FilterConstants.InBondStatus];
			filter.IsActive = true;
			filter.Property = AMSBillMessageStatusList.Codes.ClearArrival;
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearArrival), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearArrival), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearArrival), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearArrival), header3Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header4Bill for '{0}' and '{1}' as it's not inbond movement", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearArrival), !header4Bill.MatchesFilter(filterObj.Filter));
			filter.Property = AMSBillMessageStatusList.Codes.ClearDeparture;
			Assert(string.Format("Should not match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearDeparture), !header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearDeparture), header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearDeparture), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearDeparture), header3Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header4Bill for '{0}' and '{1}' as it's not inbond movement", USAMSBillFilterStrip.FilterConstants.InBondStatus, AMSBillMessageStatusList.Codes.ClearDeparture), !header4Bill.MatchesFilter(filterObj.Filter));
		}

		public void TestPTTStatusFilter()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3U", "3U DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "3Z DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1W", "1W DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "83", "83 DESC", startDate, endDate);
			Factory.Save();
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var inbondHeaderBill = inbondHeader.Bills.AddNew();
			var inbondHeaderBillDisposition = inbondHeaderBill.DispositionCodes.AddNewIfNotExist("1W", new ZDateTime(2012, 4, 1));
			var inbondHeaderBillDispositionNonePTT = inbondHeaderBill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 5, 1));
			inbondHeader.BH_ApplicationCode = "INB";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1Bill = header1.Bills.AddNew();
			var header1BillDisposition = header1Bill.DispositionCodes.AddNewIfNotExist("1W", new ZDateTime(2012, 4, 1));
			var header1BillDispositionNonePTT = header1Bill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 5, 1));
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header2Bill = header2.Bills.AddNew();
			var header2BillDisposition = header2Bill.DispositionCodes.AddNewIfNotExist("1W", new ZDateTime(2012, 4, 1));
			var header2BillDispositionNonePTT = header2Bill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 5, 1));
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header3Bill = header3.Bills.AddNew();
			var header3BillDisposition1 = header3Bill.DispositionCodes.AddNewIfNotExist("1W", new ZDateTime(2012, 4, 1));
			var header3BillDispositionNonePTT = header3Bill.DispositionCodes.AddNewIfNotExist("02", new ZDateTime(2012, 4, 5));
			var header3BillDisposition2 = header3Bill.DispositionCodes.AddNewIfNotExist("83", new ZDateTime(2012, 4, 10));
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition];
			filter.IsActive = true;
			filter.Property = "1W";
			Assert(string.Format("Should match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header3Bill for '{0}' and '{1}' as the lastest PTT is different", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), !header3Bill.MatchesFilter(filterObj.Filter));
			filter.Property = "83";
			Assert(string.Format("Should not match header1Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), !header1Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match header2Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), !header2Bill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should not match inbondHeaderBill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), !inbondHeaderBill.MatchesFilter(filterObj.Filter));
			Assert(string.Format("Should match header3Bill for '{0}' and '{1}'", USAMSBillFilterStrip.FilterConstants.LatestPTTDisposition, "1W"), header3Bill.MatchesFilter(filterObj.Filter));
		}

		[TestDate(2016, 07, 14)]
		public void TestActualArrivalDateFilter()
		{
			var header0 = Factory.New<CusInBondHeader>();
			header0.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var bill0 = header0.Bills.AddNew();
			bill0.B0_A_ARV = ZDateTime.Today.AddDays(-1);
			var bill1 = header0.Bills.AddNew();
			bill1.B0_A_ARV = ZDateTime.Today.AddDays(-10);
			var bill2 = header0.Bills.AddNew();
			bill2.B0_A_ARV = ZDateTime.Today.AddDays(10);
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleDateFilter)filterObj[USAMSBillFilterStrip.FilterConstants.ActualArrivalDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			AssertEquals(true, bill0.MatchesFilter(filterObj.Filter));
			AssertEquals(true, bill2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill1.MatchesFilter(filterObj.Filter));
			filter.Property1 = ZDateTime.Today.AddDays(2);
			AssertEquals(true, bill2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill0.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill1.MatchesFilter(filterObj.Filter));
			filter.Property2 = ZDateTime.Today.AddDays(4);
			AssertEquals(false, bill2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill0.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill1.MatchesFilter(filterObj.Filter));
		}

		public void TestMasterAndHouseBillNumberQuery()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.OceanBill.B0_MasterBillNumber = "OCU111111";
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "HSB1111111";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "HSB2222222";
			Factory.Save();
			var filterObj = new USAMSBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[USAMSBillFilterStrip.FilterConstants.MasterBillNumber];
			filter.IsActive = true;
			filter.Property = "OCU1";
			AssertEquals(true, header.OceanBill.MatchesFilter(filterObj.Filter));
			AssertEquals(true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, bill2.MatchesFilter(filterObj.Filter));
			filter.Property = "OCU2";
			AssertEquals(false, header.OceanBill.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill2.MatchesFilter(filterObj.Filter));
			filterObj = new USAMSBillFilterStrip();
			filter = (ModuleTextFilter)filterObj[USAMSBillFilterStrip.FilterConstants.HouseBillNumber];
			filter.IsActive = true;
			filter.Property = "HSB";
			AssertEquals(false, header.OceanBill.MatchesFilter(filterObj.Filter));
			AssertEquals(true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, bill2.MatchesFilter(filterObj.Filter));
			filter.Property = "HSB1";
			AssertEquals(false, header.OceanBill.MatchesFilter(filterObj.Filter));
			AssertEquals(true, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill2.MatchesFilter(filterObj.Filter));
			filter.Property = "HSB2";
			AssertEquals(false, header.OceanBill.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, bill2.MatchesFilter(filterObj.Filter));
			filter.Property = "HSB3";
			AssertEquals(false, header.OceanBill.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, bill2.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new USAMSBillFilterStrip();
		}
	}
}
