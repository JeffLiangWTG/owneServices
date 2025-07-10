using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccChargeCodeFilterBusinessObject))]
	class AccChargeCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var excludedItems = new List<Tuple<string, string>>();
			excludedItems.Add(new Tuple<string, string>(AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, "Universal Charge Code"));
			return excludedItems;
		}

		public void TestDepartmentListFilter()
		{
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_DepartmentFilterList = "AAA";

			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_DepartmentFilterList = "BBB";

			AccChargeCode chargeCode3 = Factory.New<AccChargeCode>();
			chargeCode3.AC_DepartmentFilterList = "AAA, BBB, CCC";

			AccChargeCode chargeCode4 = Factory.New<AccChargeCode>();
			chargeCode4.AC_DepartmentFilterList = "ALL";

			ModuleTextFilter filter = GetModuleTextFilter("Dept Filter");
			filter.IsActive = true;

			var chargeCodes = new AccChargeCodeCollection(Factory);
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 4, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
			AssertEquals("Should contain ChargeCode2", true, chargeCodes.Contains(chargeCode2));
			AssertEquals("Should contain ChargeCode3", true, chargeCodes.Contains(chargeCode3));
			AssertEquals("Should contain ChargeCode4", true, chargeCodes.Contains(chargeCode4));

			filter.Property = "AAA";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 3, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
			AssertEquals("Should contain ChargeCode3", true, chargeCodes.Contains(chargeCode3));
			AssertEquals("Should contain ChargeCode4", true, chargeCodes.Contains(chargeCode4));

			filter.Property = "AAA, BBB";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 4, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
			AssertEquals("Should contain ChargeCode2", true, chargeCodes.Contains(chargeCode2));
			AssertEquals("Should contain ChargeCode3", true, chargeCodes.Contains(chargeCode3));
			AssertEquals("Should contain ChargeCode4", true, chargeCodes.Contains(chargeCode4));

			filter.Property = "AAA, CCC";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 3, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
			AssertEquals("Should contain ChargeCode3", true, chargeCodes.Contains(chargeCode3));
			AssertEquals("Should contain ChargeCode4", true, chargeCodes.Contains(chargeCode4));

			filter.Property = "CCC";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 2, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode3", true, chargeCodes.Contains(chargeCode3));
			AssertEquals("Should contain ChargeCode4", true, chargeCodes.Contains(chargeCode4));

			filter.Property = "DDD";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 1, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode4", true, chargeCodes.Contains(chargeCode4));
		}

		public void TestChargeCodeFilter()
		{
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "AB";

			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "BA";

			ModuleTextFilter filter = GetModuleTextFilter("Code");
			filter.IsActive = true;

			var chargeCodes = new AccChargeCodeCollection(Factory);
			filter.Property = "A";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 2, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
			AssertEquals("Should contain ChargeCode2", true, chargeCodes.Contains(chargeCode2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 1, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
			AssertEquals("Should not contain ChargeCode2", false, chargeCodes.Contains(chargeCode2));
		}

		public void TestUniversalChargeCodeFilter()
		{
			var globalChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode1.AC_Code = "CC1";
			globalChargeCode1.AC_Desc = "Global 1";
			globalChargeCode1.AC_GC = ZGuid.Empty;
			globalChargeCode1.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
			var globalChargeCode1Mapping = globalChargeCode1.UniversalChargeCodeMappingsCollection.AddNew();
			globalChargeCode1Mapping.AUP_Code = "UNI1";

			var globalChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode2.AC_Code = "CC2";
			globalChargeCode2.AC_Desc = "Global 2";
			globalChargeCode2.AC_GC = ZGuid.Empty;
			globalChargeCode2.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
			var globalChargeCode2Mapping = globalChargeCode2.UniversalChargeCodeMappingsCollection.AddNew();
			globalChargeCode2Mapping.AUP_Code = "UNI2";
			Factory.Save();

			var query = new ZQuery(AccChargeCodeSchema.AC_Code, globalChargeCode1.AC_Code);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			var chargeCode1_LinkedToGlobal = Factory.LoadTop1<AccChargeCode>(query);
			var chargeCode1_LinkedToGlobalMapping = chargeCode1_LinkedToGlobal.UniversalChargeCodeMappingsCollection.AddNew();
			chargeCode1_LinkedToGlobalMapping.AUP_Code = "UNI3";

			Factory.Save();

			query = new ZQuery(AccChargeCodeSchema.AC_Code, globalChargeCode2.AC_Code);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			var chargeCode2_LinkedToGlobal = Factory.LoadTop1<AccChargeCode>(query);

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_Code = "CC3";
			var chargeCode3Mapping = chargeCode3.UniversalChargeCodeMappingsCollection.AddNew();
			chargeCode3Mapping.AUP_Code = "UNI4";

			var chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_Code = "CC4";

			Factory.Save();

			ModuleTextFilter filter = GetModuleTextFilter("Universal Charge Code");
			filter.IsActive = true;

			var chargeCodes = new AccChargeCodeCollection(Factory);
			filter.Property = "UNI1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 0, chargeCodes.Count);

			filter.Property = "UNI2";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 0, chargeCodes.Count);

			filter.Property = "UNI3";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 1, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1_LinkedToGlobal));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 2, chargeCodes.Count);
			AssertEquals("Should contain ChargeCode2", true, chargeCodes.Contains(chargeCode2_LinkedToGlobal));
			AssertEquals("Should contain ChargeCode4", true, chargeCodes.Contains(chargeCode4));
		}

		public void TestChargeGroupFilter()
		{
			AccChargeCode freightCode = Factory.New<AccChargeCode>();
			freightCode.AC_Code = "FRT1";
			freightCode.AC_ChargeGroup = "FRT";

			AccChargeCode originCode = Factory.New<AccChargeCode>();
			originCode.AC_Code = "ORG1";
			originCode.AC_ChargeGroup = "ORG";

			AccChargeCode loadingCode = Factory.New<AccChargeCode>();
			loadingCode.AC_Code = "LOD1";
			loadingCode.AC_ChargeGroup = "LOD";

			ModuleTextFilter filter = GetModuleTextFilter("Charge Group");
			filter.IsActive = true;
			filter.Property = "FRT";

			AccChargeCodeCollection results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);

			AssertEquals("Only Freight charges present", 1, results.Count);
			AssertEquals("Correct charge code present", freightCode, results[0]);

			filter.Property = "ORG";

			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);

			AssertEquals("Only Origin charges present", 1, results.Count);
			AssertEquals("Correct charge code present", originCode, results[0]);

			filter.Property = "OLC";

			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);

			AssertEquals("Origin and Loading charges present", 2, results.Count);
			Assert("Origin charge code present", results.Contains(originCode));
			Assert("Loading charge code present", results.Contains(loadingCode));

			filter.Property = "";

			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);

			AssertEquals("All charges present", 3, results.Count);
		}

		public void TestChargeGroupFilter_TransitCharges()
		{
			var receiveChargeCode = CreateAccChargeCode("REC", ChargeCodeGroupList.Codes.TRWReceive);
			var dispatchChargeCode = CreateAccChargeCode("DEP", ChargeCodeGroupList.Codes.TRWDispatch);
			var rtuChargeCode = CreateAccChargeCode("RTU", ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var dtuChargeCode = CreateAccChargeCode("DTU", ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);

			var results = new AccChargeCodeCollection(Factory);
			results.Load();
			AssertEquals("All Transit charges must be returned.", 4, results.Count);

			var filter = GetModuleTextFilter("Charge Group");
			filter.IsActive = true;
			filter.Property = AccChargeCodeLookups.TRWGroupFilterCode;

			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);

			AssertContainsExactElementsInAnyOrder(new[] { receiveChargeCode, dispatchChargeCode }, results);
		}

		public void TestChargeGroupFilter_TransitTransportationUnitCharges()
		{
			var receiveChargeCode = CreateAccChargeCode("REC", ChargeCodeGroupList.Codes.TRWReceive);
			var dispatchChargeCode = CreateAccChargeCode("DEP", ChargeCodeGroupList.Codes.TRWDispatch);
			var rtuChargeCode = CreateAccChargeCode("RTU", ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var dtuChargeCode = CreateAccChargeCode("DTU", ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);

			var results = new AccChargeCodeCollection(Factory);
			results.Load();
			AssertEquals("All Transit charges must be returned.", 4, results.Count);

			var filter = GetModuleTextFilter("Charge Group");
			filter.IsActive = true;
			filter.Property = AccChargeCodeLookups.TWUGroupFilterCode;

			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);

			AssertContainsExactElementsInAnyOrder(new[] { rtuChargeCode, dtuChargeCode }, results);
		}

		public void TestChargeGroupFilter_ContainerYard()
		{
			var gateInChargeCode = CreateAccChargeCode("GIN", ChargeCodeGroupList.Codes.YardGateIn);
			var gateOutChargeCode = CreateAccChargeCode("GOT", ChargeCodeGroupList.Codes.YardGateOut);
			var yardStorageChargeCode = CreateAccChargeCode("YSC", ChargeCodeGroupList.Codes.YardStorage);
			var workOrderHeaderChargeCode = CreateAccChargeCode("MNR", ChargeCodeGroupList.Codes.MNRWorkOrderHeader);
			var labourHourChargeCode = CreateAccChargeCode("LRC", ChargeCodeGroupList.Codes.LabourHourRate);
			var receiveChargeCode = CreateAccChargeCode("REC", ChargeCodeGroupList.Codes.TRWReceive);
			var dispatchChargeCode = CreateAccChargeCode("DEP", ChargeCodeGroupList.Codes.TRWDispatch);

			var results = new AccChargeCodeCollection(Factory);
			results.Load();
			AssertEquals("All charges must be returned.", 7, results.Count);

			var filter = GetModuleTextFilter("Charge Group");
			filter.IsActive = true;
			filter.Property = AccChargeCodeLookups.CYDGroupFilterCode;

			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);

			AssertContainsExactElementsInAnyOrder(new[] { gateInChargeCode, gateOutChargeCode, yardStorageChargeCode, workOrderHeaderChargeCode, labourHourChargeCode }, results);
		}

		AccChargeCode CreateAccChargeCode(string code, string chargeGroup)
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_ChargeGroup = chargeGroup;

			return chargeCode;
		}

		public void TestConsolLeveFilter()
		{
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_IsGroupageCharge = true;

			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_IsGroupageCharge = false;

			ModuleTextFilter filter = GetModuleTextFilter("Consol Level Status");
			filter.IsActive = true;

			var chargeCodes = new AccChargeCodeCollection(Factory);
			filter.Property = "CON";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 1, chargeCodes.Count);
			AssertCollectionContains("Should contain ChargeCode1 for GroupageCharge", chargeCode1, chargeCodes);
			AssertCollectionNotContains("Should not contain ChargeCode2 for GroupageCharge", chargeCode2, chargeCodes);

			filter.Property = "NOT";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 1, chargeCodes.Count);
			AssertCollectionNotContains("Should not contain ChargeCode1 for non GroupageCharge", chargeCode1, chargeCodes);
			AssertCollectionContains("Should contain ChargeCode2 for non GroupageCharge", chargeCode2, chargeCodes);

			filter.Property = "ALL";
			chargeCodes.Load(filter.Query);
			AssertEquals("ChargeCodes.Count", 2, chargeCodes.Count);
			AssertCollectionContains("Should contain ChargeCode1", chargeCode1, chargeCodes);
			AssertCollectionContains("Should contain ChargeCode2", chargeCode2, chargeCodes);
		}

		public virtual void TestLinkedToGlobalListFilter()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var filter = GetModuleTextFilter("LinkedToGlobal");
			filter.IsActive = true;
			var chargeCodes = new AccChargeCodeCollection(Factory);

			filter.Property = AccChargeCodeFilterBusinessObject.LinkedToGlobalOption.Code.All;
			chargeCodes.Load(filter.Query);
			AssertEquals("Should show all charge codes", 2, chargeCodes.Count);

			filter.Property = AccChargeCodeFilterBusinessObject.LinkedToGlobalOption.Code.LinkedToGlobal;
			chargeCodes.Load(filter.Query);
			AssertEquals("Should show just the one charge code", 1, chargeCodes.Count);
			AssertEquals("The linked charge code is shown", normalChargeCodeLinked, chargeCodes[0]);

			filter.Property = AccChargeCodeFilterBusinessObject.LinkedToGlobalOption.Code.NotLinkedToGlobal;
			chargeCodes.Load(filter.Query);
			AssertEquals("Should show just the one charge code", 1, chargeCodes.Count);
			AssertEquals("The not-linked charge code is shown", normalChargeCode, chargeCodes[0]);
		}

		public virtual void TestGovernmentChargeCodeFilter()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				ModuleTextFilter filter = GetModuleTextFilter("Government Charge Code");
				AssertNull(filter);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var chargeCode1 = Factory.New<AccChargeCode>();
				chargeCode1.AC_GovtChargeCode = "CD1";

				var chargeCode2 = Factory.New<AccChargeCode>();
				chargeCode2.AC_GovtChargeCode = "CD2";

				ModuleTextFilter filter = GetModuleTextFilter("Government Charge Code");
				filter.IsActive = true;

				var chargeCodes = new AccChargeCodeCollection(Factory);
				filter.Property = "CD";
				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				chargeCodes.Load(filter.Query);
				AssertEquals("ChargeCodes.Count", 2, chargeCodes.Count);
				AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
				AssertEquals("Should contain ChargeCode2", true, chargeCodes.Contains(chargeCode2));

				filter.Property = "CD1";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				chargeCodes.Load(filter.Query);
				AssertEquals("ChargeCodes.Count", 1, chargeCodes.Count);
				AssertEquals("Should contain ChargeCode1", true, chargeCodes.Contains(chargeCode1));
				AssertEquals("Should not contain ChargeCode2", false, chargeCodes.Contains(chargeCode2));
			}
		}

		public void TestIATACodeFilter()
		{
			var freightCode = Factory.New<AccChargeCode>();
			freightCode.AC_Code = "FRT1";
			freightCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;

			var originCode = Factory.New<AccChargeCode>();
			originCode.AC_Code = "ORG1";
			originCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.DC;

			var filter = GetModuleTextFilter("IATA Code");
			filter.IsActive = true;
			filter.Property = Core.Constants.AWB.ChargeCodes.DB;
			var results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("Only 1 charge present", 1, results.Count);
			AssertEquals("Correct charge code present", freightCode, results[0]);

			filter.Property = Core.Constants.AWB.ChargeCodes.DC;
			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("Only 1 charge present", 1, results.Count);
			AssertEquals("Correct charge code present", originCode, results[0]);

			filter.Property = ZString.Empty;
			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("All charges present", 2, results.Count);
		}

		public void TestAirlineIATACodeFilter()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsAirLine = true;
			carrier1.OH_Code = "CARRIER1";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			carrier2.OH_Code = "CARRIER2";

			var freightCode = Factory.New<AccChargeCode>();
			freightCode.AC_Code = "FRT1";
			var airLineCode1 = freightCode.AccChargeCodeCarrierIataMappings.AddNew();
			airLineCode1.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
			airLineCode1.ACI_OH_Carrier = carrier1.PK;

			var originCode = Factory.New<AccChargeCode>();
			originCode.AC_Code = "ORG1";
			var airLineCode2 = originCode.AccChargeCodeCarrierIataMappings.AddNew();
			airLineCode2.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.DC;
			airLineCode2.ACI_OH_Carrier = carrier2.PK;

			Factory.Save();

			var filter = GetModuleTextFilter("Airline IATA Code");
			filter.IsActive = true;
			filter.Property = Core.Constants.AWB.ChargeCodes.DB;
			var results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("Only 1 charge present", 1, results.Count);
			AssertEquals("Correct charge code present", freightCode, results[0]);

			filter.Property = Core.Constants.AWB.ChargeCodes.DC;
			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("Only 1 charge present", 1, results.Count);
			AssertEquals("Correct charge code present", originCode, results[0]);

			filter.Property = ZString.Empty;
			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("All charges present", 2, results.Count);
		}

		public void TestIATACodeAirlineOrgFilter()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_IsAirLine = true;
			carrier1.OH_Code = "CARRIER1";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			carrier2.OH_Code = "CARRIER2";

			var freightCode = Factory.New<AccChargeCode>();
			freightCode.AC_Code = "FRT1";
			var airLineCode1 = freightCode.AccChargeCodeCarrierIataMappings.AddNew();
			airLineCode1.ACI_OH_Carrier = carrier1.PK;
			airLineCode1.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;

			var originCode = Factory.New<AccChargeCode>();
			originCode.AC_Code = "ORG1";
			var airLineCode2 = originCode.AccChargeCodeCarrierIataMappings.AddNew();
			airLineCode2.ACI_OH_Carrier = carrier2.PK;
			airLineCode2.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;

			Factory.Save();

			var filter = GetModuleGuidFilter("IATA Code Airline");
			filter.IsActive = true;
			filter.Property = carrier1.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("Only 1 charge present", 1, results.Count);
			AssertEquals("Correct charge code present", freightCode, results[0]);

			filter.Property = carrier2.PK;
			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("Only 1 charge present", 1, results.Count);
			AssertEquals("Correct charge code present", originCode, results[0]);

			filter.Property = carrier2.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("Only 1 charge present", 1, results.Count);
			AssertEquals("Correct charge code present", freightCode, results[0]);

			filter.Property = ZGuid.Empty;
			results = new AccChargeCodeCollection(Factory);
			results.Load(filter.Query);
			AssertEquals("All charges present", 2, results.Count);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccChargeCodeFilterBusinessObject();
		}

		internal ModuleTextFilter GetModuleTextFilter(ZString description)
		{
			return (ModuleTextFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		ModuleGuidFilter GetModuleGuidFilter(ZString description)
		{
			return (ModuleGuidFilter)GetNewFilterStripBusinessObject()[description];
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccChargeCodeCollection chargeCodes = new AccChargeCodeCollection(Factory);
			chargeCodes.Load();
			chargeCodes.RemoveAndDeleteAll();
		}

		#endregion
	}
}
