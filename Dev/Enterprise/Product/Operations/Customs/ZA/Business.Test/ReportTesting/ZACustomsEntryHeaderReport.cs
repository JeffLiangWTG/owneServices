using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class ZACustomsEntryHeaderReportTests : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "Report_ZACustomsEntryHeader";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => ZACustomsEntryHeaderReportTestHelper.ExpectedColumnsInAnyOrder;

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected override List<string> ParametersValuesList => ZACustomsEntryHeaderReportTestHelper.ParametersValuesList;

		protected override void AssertTestResults(DataTable resultsOrderedByExpectedColumnNames)
		{
			AssertEquals(1, resultsOrderedByExpectedColumnNames.Rows.Count);
			var row = FormatRowsValues(resultsOrderedByExpectedColumnNames.Rows[0], resultsOrderedByExpectedColumnNames);
			var expectedData = $"[JobNumber]='AR1TEST'; [EntryNumber]='AAAANGWMT'; [LRN]='00505655JSA20170214004008'; [EntryStatus]='1'; [ProcedureCode]='00'; [EntryInstructionDescription]='Entry Instruction Description'; [ImporterCode]='IMP00'; [ImporterName]='IMP00_FullName'; [SupplierCode]='SUP01'; [SupplierName]='SUP01_FullName'; [TotalDuty]='123.0000'; [TotalS1p2b]='456.0000'; [TotalPpPen]='789.0000'; [TotalVat]='600.0000'; [NoOfPkgs]='5'; [BondAcquittalValue]='0.0000'; [NoOfLines]='4'; [ShipmentType]='IMP'; [CustomsOffice]='JHB'; [MasterBill]='OOCL12345678'; [CountryOfExit]='CATOR'; [CountryOfDestination]='ZAJNB'; [OfficeOfExit]='CTN'; [Transport]='AIR'; [VoyFlight]='SA666'; [BondAcquittedDate]='2023-09-05T00:00:00'; [BondValidToDate]='2023-09-06T00:00:00'; [EntryReleaseDate]='2023-09-03T00:12:00'; [EntrySubmittedDate]='2023-09-02T00:11:00'; [WarehouseReleaseDate]='2023-09-04T00:14:00'; [WarehouseTransactionStatus]='IUP'; [PreviousMRN]='DFM201609225000601'; [UcrNo]='UCRFROMCUSENTRYNUMBER00001'; [AssessmentDate]='2016-09-22T00:00:00'; [FromWarehouseCode]='WHS00_2WC'; [FromWarehouseName]='WHS00_FullName'; [ToWarehouseCode]='WHS01_2WC'; [ToWarehouseName]='WHS01_FullName'; [BondHolderCode]='CEIBH03'; [BondHolderName]='CEIBH03_FullName'; [RemoverCode]='CEICAR02'; [RemoverName]='CEICAR02_FullName'; [NewWhOwnerCode]='CEIOH04'; [NewWhOwnerName]='CEIOH04_FullName'; [HouseBill]='S00049567'; [JobRegisteredDate]='2023-09-01T00:00:00'; [BranchPK]='27a55065-ac88-4ec3-8bed-e575e79172cb'; [ImporterPk]='{importerPK}'; [SupplierPk]='{supplierPK}'; [RelPrintInd]='Y'";
			AssertContainsMoreHelpfully(expectedData, row);
		}

		protected override void PrepareTestData()
		{
			var clusterKey = 1;
			var orgFactory = new OrganizationFactory(Factory);
			importerPK = orgFactory.CreateOrganization("IMP").PK;
			supplierPK = orgFactory.CreateOrganization("SUP").PK;
			var whsFactory = new WarehouseFactory(Factory);
			var warehouse = whsFactory.CreateWareHouse();
			var warehouse2 = whsFactory.CreateWareHouse();
			var carrier = orgFactory.CreateOrganization("CEICAR");
			var bondHolder = orgFactory.CreateOrganization("CEIBH");
			var owner = orgFactory.CreateOrganization("CEIOH");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refDatagrouping = helper.CreateNewOrGetExistingDataGrouping("ZIP", "ZZZ Data Group 1");
			var duty = helper.CreateNewOrGetExistingRateType("ZIP", Universal.Constants.RateTypes.Duty, "Cus Rate Type 1");
			var s1p2b = helper.CreateNewOrGetExistingRateType("ZIP", Universal.Constants.RateTypes.AdValoremExcise, "Cus Rate Type 2");
			var pp = helper.CreateNewOrGetExistingRateType("ZIP", Universal.Constants.RateTypes.ProvisionalPayment, "Cus Rate Type 3");
			var tariff = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, Core.Constants.CountryCodes.SouthAfrica);
			var groupPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica, "South Africa");
			var type1 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			type1.ZZR_IsPayable = false;
			type1.ZZR_CustomsValueFormula = string.Empty;
			var type2 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Universal.Constants.RateTypes.AdValoremExcise);
			type2.ZZR_IsPayable = false;
			type2.ZZR_CustomsValueFormula = string.Empty;
			var typeReb = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Universal.Constants.RateTypes.Rebate);
			typeReb.ZZR_IsPayable = false;
			typeReb.ZZR_CustomsValueFormula = string.Empty;
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, type1.PK, isSystem: true, internalUse: false);
			var rateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, Universal.Constants.RateTypes.AdValoremExcise, type2.PK, isSystem: true, internalUse: false);

			var dec = ZACustomsEntryHeaderReportTestHelper.GetJobDeclaration(Factory, "AR1TEST", importerPK, supplierPK);
			var instruction = ZACustomsEntryHeaderReportTestHelper.GetEntryInstruction(Factory, dec.PK, bondHolder.PK, carrier.PK, owner.PK, warehouse.MainAddress.PK, warehouse2.MainAddress.PK);
			var entry = ZACustomsEntryHeaderReportTestHelper.GetCusEntryHeader(Factory, dec.PK, instruction.PK);

			var entryLineFactory = new EntryLineFactory(Factory);
			var l1 = entryLineFactory.CreateEntryLine(entry, clusterKey);
			var l2 = entryLineFactory.CreateEntryLine(entry, clusterKey);
			var l3 = entryLineFactory.CreateEntryLine(entry, clusterKey);
			var l4 = entryLineFactory.CreateEntryLine(entry, clusterKey);

			var lineFeeFactory = new EntryLineFeeFactory(Factory);
			var fee1Pk = lineFeeFactory.CreateEntryLineFee(l1, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, 123.00f, clusterKey);
			var fee2Pk = lineFeeFactory.CreateEntryLineFee(l2, Universal.Constants.RateTypes.AdValoremExcise, 456.00f, clusterKey);
			var fee3Pk = lineFeeFactory.CreateEntryLineFee(l3, Universal.Constants.RateTypes.ProvisionalPayment, 789.00f, clusterKey);
			var fee4Pk = lineFeeFactory.CreateEntryLineFee(l4, "VAT", 600.00f, clusterKey);

			var entryNumFactory = new EntryNumFactory(Factory);
			var ce1Pk = entryNumFactory.CreateEntryNum(entry.PK, nameof(CusEntryHeader), "AAAANGWMT", "MRN", "CUS", Core.Constants.CountryCodes.SouthAfrica);
			var ce2Pk = entryNumFactory.CreateEntryNum(entry.PK, nameof(CusEntryHeader), "UCRFROMCUSENTRYNUMBER00001", "UCR", "CUS", Core.Constants.CountryCodes.SouthAfrica);

			Factory.Save();
		}

		ZGuid importerPK;
		ZGuid supplierPK;
	}
}
