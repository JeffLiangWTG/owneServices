using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed partial class JobComInvoiceLineTest
	{
		public void TestIsExcise()
		{
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "45", "00", "", "", "EXW", group: UniversalReferenceConstants.RefCusProcedureGroup.Excise);
			Factory.Save();
			var instruction = Factory.NewMoq<CusEntryInstruction>().Object;
			instruction.CEI_Style = "45";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "4500";
			Assert(invoiceLine.IsExcise);
		}

		public override void TestWipeNKTaxType()
		{
			var procedure = universalReferenceDataHelper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			procedure.ZZ6_CalculateVAT = false;
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_ZZF_NKTaxType = "605";
			invoiceLine.JI_Procedure = "1111";
			AssertWipeNKTaxType(shouldWipeNKTaxType: true, ZString.Empty);
			invoiceLine.JI_ZZF_NKTaxType = "605";
			invoiceLine.JI_Procedure = "1112";
			AssertWipeNKTaxType(shouldWipeNKTaxType: false, "605");
			procedure.ZZ6_CalculateVAT = true;
			Factory.Save();
			invoiceLine.JI_Procedure = "1111";
			AssertWipeNKTaxType(shouldWipeNKTaxType: false, "605");
		}

		[TestDate(2016, 08, 02)]
		public void TestSchedule1Part1Tariff()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991001", new ZDateTime(2000, 01, 01), new ZDateTime(2016, 06, 01));
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "992002", new ZDateTime(2016, 08, 02), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "991001";
			var tariff = invoiceLine.UniversalTariff;
			AssertEquals(null, tariff);
			invoiceLine.JI_Tariff = "992002";
			tariff = invoiceLine.UniversalTariff;
			AssertNotEquals(null, tariff);
			instruction.CEI_DateForDuty = new ZDateTime(2016, 06, 01);
			invoiceLine.JI_Tariff = "991001";
			tariff = invoiceLine.UniversalTariff;
			AssertNotEquals(null, tariff);
			invoiceLine.JI_Tariff = "992002";
			tariff = invoiceLine.UniversalTariff;
			AssertEquals(null, tariff);
		}

		[TestDate(2016, 08, 02)]
		public void TestSchedule1Part2ATariff()
		{
			var tariffType12A = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A);
			var tariff = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "992002", new ZDateTime(2016, 08, 02), new ZDateTime(2079, 06, 06));
			Factory.Save();
			invoiceLine.CusLineTariffDetails.AddNew("12A", "992002");
			AssertEquals(tariff, invoiceLine.Schedule1Part2ATariff);
		}

		[TestDate(1990, 6, 1)]
		public void TestAdditionalScheduleIsDefaulted()
		{
			SetupAdditionalScheduleIsDefaulted();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "1#";
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "3#";
			var entryInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_Style = "4#";
			var entryInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction4.CEI_Style = "5#";

			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.JI_Procedure = "002$";
			invoiceLine.JI_Tariff = "2010101011";
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], "13A", "1010101011");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "2P2", "1020101012");
			invoiceLine.JI_Tariff = "3010101011";
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 0, invoiceLine.CusLineTariffDetails.Count);
			entryInstruction1.CEI_Style = "2#";
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 0, invoiceLine.CusLineTariffDetails.Count);
			invoiceLine.JI_Tariff = "2010101011";

			var tariffDetail1 = invoiceLine.CusLineTariffDetails[0];
			var tariffDetail2 = invoiceLine.CusLineTariffDetails[1];
			var tariffDetail3 = invoiceLine.CusLineTariffDetails[2];
			AssertAdditionalScheduleIsDefaultedTariffDetail(invoiceLine, "13A", "2P2", "3P2");

			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}3$";
			AssertAdditionalScheduleIsDefaultedTariffDetail(invoiceLine, "13A", "2P2", "3P2");

			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}4$";
			AssertAdditionalScheduleIsDefaultedTariffDetailIsDeleted(tariffDetail1, tariffDetail2, tariffDetail3);
			AssertAdditionalScheduleIsDefaultedTariffDetail(invoiceLine, "13A", "2P2", "4P4");

			invoiceLine.JI_CEI = entryInstruction2.PK;
			AssertAdditionalScheduleIsDefaultedTariffDetailIsDeleted(tariffDetail1, tariffDetail2, tariffDetail3);
			AssertAdditionalScheduleIsDefaultedTariffDetail(invoiceLine, "13A", "2P2", "4P4");

			invoiceLine.JI_CEI = entryInstruction3.PK;
			AssertAdditionalScheduleIsDefaultedTariffDetailIsDeleted(tariffDetail1, tariffDetail2, tariffDetail3);
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], "6P1", "1050101016");
			invoiceLine.JI_CEI = entryInstruction4.PK;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 0, invoiceLine.CusLineTariffDetails.Count);
			entryInstruction4.CEI_Style = "7#";
			AssertAdditionalScheduleIsDefaultedTariffDetail(invoiceLine, "13A", "2P2", "4P4");
		}

		void SetupAdditionalScheduleIsDefaulted()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P2");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P2");
			var tariffType4P4 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P4");
			var tariffType4P5 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P5");
			var tariffType5P5 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P5");
			var tariffType6P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "2#", "2$", "3", "WENDY's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "C", "3#", "2$", "4", "JOHN's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "D", "4#", "2$", "6", "JACK's PROCEDURE", ZAJobMessageTypeList.Codes.Import, calculateDuty: false);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "5#", "2$", "", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import, calculateDuty: false);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "2#", "3$", "3", "JAKE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "6#", "2$", "3", "MIKE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "2#", "4$", "4", "JANE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "7#", "2$", "4", "TOM's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "DTY", "1010101011", Universal.Constants.RateTypes.AntiDumping);
			tariff1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "2P2", "DTY", "1020101012", Universal.Constants.RateTypes.AdValoremExcise);
			tariff2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff3 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "3P2", "DTY", "1030101013", Universal.Constants.RateTypes.Duty);
			tariff3.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = false;
			var tariff4 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P4.PK, "1040101014", startDate, endDate);
			var tariff5 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P5.PK, "1050101015", startDate, endDate);
			var tariff6 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2010101011", startDate, endDate);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "3010101011", startDate, endDate);
			var tariff8 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "1050101016", startDate, endDate);
			var tariff9 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P5.PK, "1040101017", startDate, endDate);
			universalReferenceDataHelper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "1010101011");
			universalReferenceDataHelper.CreateTariffRelationship(tariff3.PK, tariff1.ZZ1_ZZI_TariffType, "1010101011");
			universalReferenceDataHelper.CreateTariffRelationship(tariff4.PK, tariff2.ZZ1_ZZI_TariffType, "102010");
			universalReferenceDataHelper.CreateTariffRelationship(tariff5.PK, tariff4.ZZ1_ZZI_TariffType, "104010");
			universalReferenceDataHelper.CreateTariffRelationship(tariff1.PK, tariff6.ZZ1_ZZI_TariffType, "201010");
			universalReferenceDataHelper.CreateTariffRelationship(tariff8.PK, tariff6.ZZ1_ZZI_TariffType, "201010");
			universalReferenceDataHelper.CreateTariffRelationship(tariff9.PK, tariff2.ZZ1_ZZI_TariffType, "102010");
			Factory.Save();
		}

		void AssertAdditionalScheduleIsDefaultedTariffDetailIsDeleted(CusLineTariffDetail tariffDetail1, CusLineTariffDetail tariffDetail2, CusLineTariffDetail tariffDetail3)
		{
			AssertEquals(expected: true, tariffDetail1.IsDeleted);
			AssertEquals(expected: true, tariffDetail2.IsDeleted);
			AssertEquals(expected: true, tariffDetail3.IsDeleted);
		}

		[TestDate(2021, 1, 22)]
		public void TestAdditionalScheduleDefaultedExcludes13DWhenUsed()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part3D);
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P2");
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var rootTariff = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1000000001", new ZDateTime(2021, 1, 1), new ZDateTime(2021, 1, 31));
			var tariff13A = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "DTY", "2000000002", Universal.Constants.RateTypes.AntiDumping);
			tariff13A.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff13D = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13D", "DTY", "3000000003", Universal.Constants.RateTypes.Duty);
			tariff13D.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff2P2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "2P2", "DTY", "4000000004", Universal.Constants.RateTypes.AdValoremExcise);
			tariff2P2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffRelationship(tariff13A.PK, rootTariff.ZZ1_ZZI_TariffType, "1000000001");
			universalReferenceDataHelper.CreateTariffRelationship(tariff13D.PK, rootTariff.ZZ1_ZZI_TariffType, "1000000001");
			universalReferenceDataHelper.CreateTariffRelationship(tariff2P2.PK, rootTariff.ZZ1_ZZI_TariffType, "1000000001");
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "1#";
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.N;
			invoiceLine.JI_Procedure = "002$";
			invoiceLine.JI_Tariff = rootTariff.ZZ1_TariffCode;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 3, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], "13A", "2000000002");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "13D", "3000000003");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[2], "2P2", "4000000004");
			invoiceLine.JI_Tariff = "";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.U;
			invoiceLine.JI_Tariff = rootTariff.ZZ1_TariffCode;
			AssertEquals("Detail count for Used", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], "13A", "2000000002");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "2P2", "4000000004");
		}

		[TestDate(2021, 1, 22)]
		public void TestAdditionalScheduleOnlyIncludes1P8ForProcedure1000()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part8);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", ProcedureCodes._10, ProcedureCodes._00, "", "PROCEDURE 1", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", ProcedureCodes._11, ProcedureCodes._00, "", "PROCEDURE 2", ZAJobMessageTypeList.Codes.Import);
			var rootTariff = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1000000001", new ZDateTime(2021, 1, 1), new ZDateTime(2021, 1, 31));
			var tariff13A = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "DTY", "2000000002", Constants.RateTypes.AntiDumping);
			tariff13A.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff1P8 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part8, "LVY", "3000000003", Constants.RateTypes.Levy);
			tariff1P8.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffRelationship(tariff13A.PK, rootTariff.ZZ1_ZZI_TariffType, "1000000001");
			universalReferenceDataHelper.CreateTariffRelationship(tariff1P8.PK, rootTariff.ZZ1_ZZI_TariffType, "1000000001");
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ProcedureCodes._10;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = ProcedureCodes._10 + ProcedureCodes._00;
			invoiceLine.JI_Tariff = rootTariff.ZZ1_TariffCode;

			AssertEquals("Should include 1P8 tariff for procedure 1000", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], "13A", "2000000002");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "1P8", "3000000003");

			entryInstruction.CEI_Style = ProcedureCodes._11;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.JI_Procedure = ProcedureCodes._11 + ProcedureCodes._00;
			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_Tariff = rootTariff.ZZ1_TariffCode;

			AssertEquals("Should exclude 1P8 tariff", 1, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], "13A", "2000000002");
		}

		[TestDate(1990, 6, 1)]
		public void TestSetTariffEtcDataFromProductsPivotCore()
		{
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "40", "00", "", "", "", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "14", "00", "3", "", "", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "C", "14", "01", "2", "", "", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "D", "14", "02", "3", "", "IMP,EXW", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "14", "03", "3", "", "EXP", "");
			var tariffType = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "1010101011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CreateProduct(supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var pivot = AddPivot(part, "1010101012", Core.Constants.CountryCodes.Germany);
			pivot.CusLineTariffDetails.AddNew("2P2", "1020101012");
			pivot.CusLineTariffDetails.AddNew("3P1", "1030101013");
			pivot.CusLineTariffDetails.AddNew("1P1", ZString.Empty);
			pivot.CusLineTariffDetails.AddNew("1P2", ZString.Empty);
			pivot.CusLineTariffDetails.AddNew("12B", "1260331");
			pivot.CusLineTariffDetails.AddNew("13E", "1530357");
			pivot.CusLineTariffDetails.AddNew("1P6", "169102");
			AssertSetTariffEtcDataFromProductsPivotCore(part.OP_PartNum);

			invoiceLine.JI_PartNo = ZString.Empty;
			instruction.CEI_Style = "40";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertSetTariffEtcDataFromProductsPivotCoreTariffDetails("1020101012", "1260331", "1530357", "169102");

			invoiceLine.JI_PartNo = ZString.Empty;
			instruction.CEI_Style = "14";
			invoiceLine.JI_Procedure = "14003";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertSetTariffEtcDataFromProductsPivotCoreTariffDetails("1020101012", "1030101013", "1260331", "1530357", "169102");

			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_Procedure = "14012";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertSetTariffEtcDataFromProductsPivotCoreTariffDetails("1020101012", "1260331", "1530357", "169102");

			pivot.CI_TariffNum = "1010101011";
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_Procedure = "1402";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertSetTariffEtcDataFromProductsPivotCoreTariffDetails("1020101012", "1030101013", "1260331", "1530357");

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			invoiceLine.JI_PartNo = ZString.Empty;
			invoiceLine.JI_Procedure = "1403";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertSetTariffEtcDataFromProductsPivotCoreTariffDetails("1030101013", "169102");

			AssertSetTariffEtcDataFromProductsPivotCoreChildType(part, string.Empty);
			AssertSetTariffEtcDataFromProductsPivotCoreChildType(part, null);
		}

		void AssertSetTariffEtcDataFromProductsPivotCore(ZString partNum)
		{
			Assert(invoiceLine.JI_CountryOfOrigin.IsEmpty);
			Assert(invoiceLine.JI_PrimaryPreference.IsEmpty);
			Assert(invoiceLine.JI_ROOCert.IsEmpty);
			AssertEquals("N", invoiceLine.JI_NewUsed);
			Assert(invoiceLine.JI_EngineCapacity.IsEmpty);
			Assert(invoiceLine.JI_VehicleFormat.IsEmpty);
			Assert(invoiceLine.JI_VehicleType.IsEmpty);
			Assert(invoiceLine.JI_Colour.IsEmpty);
			invoiceLine.JI_PartNo = partNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals(Core.Constants.CountryCodes.Germany, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("EUTRADE", invoiceLine.JI_PrimaryPreference);
			AssertEquals("CERT", invoiceLine.JI_ROOCert);
			AssertEquals("U", invoiceLine.JI_NewUsed);
			AssertEquals(3000, invoiceLine.JI_EngineCapacity);
			AssertEquals(VehicleFormatList.Codes.FBU, invoiceLine.JI_VehicleFormat);
			AssertEquals(VehicleTypeList.Codes.Passenger, invoiceLine.JI_VehicleType);
			AssertEquals("WHITE", invoiceLine.JI_Colour);
			AssertSetTariffEtcDataFromProductsPivotCoreTariffDetails("1020101012", "1030101013", "1260331", "1530357", "169102");
		}

		void AssertSetTariffEtcDataFromProductsPivotCoreTariffDetails(params string[] tariffCodes)
		{
			AssertContainsExactElementsInAnyOrder(tariffCodes, invoiceLine.CusLineTariffDetails.Select(d => d.BZ_Tariff));
		}

		void AssertSetTariffEtcDataFromProductsPivotCoreChildType(OrgSupplierPart part, string newUsed)
		{
			var line = invoiceHeader.InvoiceLines.AddNew();
			part.PivotsForBinding.RemoveAll();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_NewUsed = newUsed;
			line.JI_PartNo = part.OP_PartNum;
			AssertNotNull(line.Part);
			AssertEquals("N", line.JI_NewUsed);
		}

		public void TestAdditionalTariffsFromProduct_NoConcession_Import()
		{
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "40", "00", "", "", "IMP,EXW", "");
			SetupAdditionalTariffsFromProduct_NoConcession();
			AssertNotNull(invoiceLine.Part);
			AssertEquals(3, invoiceLine.CusLineTariffDetails.Count);
			AssertEquals("1020101012", invoiceLine.CusLineTariffDetails[0].BZ_Tariff);
			AssertEquals("1030101014", invoiceLine.CusLineTariffDetails[1].BZ_Tariff);
			AssertEquals("1030101015", invoiceLine.CusLineTariffDetails[2].BZ_Tariff);
		}

		public void TestAdditionalTariffsFromProduct_NoConcession_Export()
		{
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "40", "00", "", "", "EXP", "");
			SetupAdditionalTariffsFromProduct_NoConcession();
			AssertNotNull(invoiceLine.Part);
			AssertEquals(1, invoiceLine.CusLineTariffDetails.Count);
			AssertEquals("1030101016", invoiceLine.CusLineTariffDetails[0].BZ_Tariff);
		}

		void SetupAdditionalTariffsFromProduct_NoConcession()
		{
			var tariffType = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "1010101011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CreateProduct(supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var pivot = AddPivot(part, "1010101011", Core.Constants.CountryCodes.Germany);
			pivot.CusLineTariffDetails.AddNew("2P2", "1020101012");
			pivot.CusLineTariffDetails.AddNew("3P1", "1030101013");
			pivot.CusLineTariffDetails.AddNew("12A", "1030101014");
			pivot.CusLineTariffDetails.AddNew("13D", "1030101015");
			pivot.CusLineTariffDetails.AddNew("1P6", "1030101016");
			instruction.CEI_Style = "40";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.JI_PartNo = part.OP_PartNum;
		}

		public void TestAdditionalTariffsFromProductTariffTypes()
		{
			var line = Factory.New<JobComInvoiceLineForTest>();
			AssertContainsExactElementsInAnyOrder("Concession Types", new ZString[] { "3P1", "3P2", "4P1", "4P2", "4P3", "4P4", "4P5", "4P6", "5P1", "5P2", "5P3", "5P4", "5P5", "6P1", "6P2", "6P3", "6P4", "6P5" }, line.ConcessionTariffTypes_Exposed);
			AssertContainsExactElementsInAnyOrder("Import Types", new ZString[] { "12A", "12B", "13A", "13B", "13C", "13D", "13E", "15A", "15B", "17A", "2P1", "2P2", "2P3" }, line.ImportShipmentTariffTypes_Exposed);
			AssertContainsExactElementsInAnyOrder("Export Types", new ZString[] { "1P6" }, line.ExportShipmentTariffTypes_Exposed);
		}

		public void TestUseAdditionalTariffsFallBackToRelatedTariffs()
		{
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "40", "00", "4", "", "", "");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var tariffType4P2 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P2");
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff4P2 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P2.PK, "4200000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceDataHelper.CreateTariffRelationship(tariff4P2.PK, tariffType1P1.PK, "1010");
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part1 = CreateProduct(supplier, "NEWPROD-1", "PRODUCT-1");
			var part2 = CreateProduct(supplier, "NEWPROD-2", "PRODUCT-2");
			Factory.Save();

			var pivot1 = AddPivot(part1, "1010101011", Core.Constants.CountryCodes.Germany);
			pivot1.CusLineTariffDetails.AddNew("4P1", "4100000002");
			AddPivot(part2, "1010101011", Core.Constants.CountryCodes.Germany);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "40";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "4000";
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Procedure = "4000";
			invoiceLine.JI_Tariff = "1010101011";
			AssertNull(invoiceLine.Part);
			AssertEquals(1, invoiceLine.CusLineTariffDetails.Count);
			AssertEquals("4200000001", invoiceLine.CusLineTariffDetails[0].BZ_Tariff);
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_PartNo = part1.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals(1, invoiceLine.CusLineTariffDetails.Count);
			AssertEquals("4100000002", invoiceLine.CusLineTariffDetails[0].BZ_Tariff);
			invoiceLine2.JI_PartNo = part2.OP_PartNum;
			AssertNotNull(invoiceLine2.Part);
			AssertEquals(1, invoiceLine2.CusLineTariffDetails.Count);
			AssertEquals("4200000001", invoiceLine2.CusLineTariffDetails[0].BZ_Tariff);
		}

		public void TestAdditionalTariffsFromProduct_ConcessionFilter()
		{
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "40", "00", "3", "", "", "");
			var tariffType = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "1010101011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CreateProduct(supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var pivot = AddPivot(part, "1010101011", Core.Constants.CountryCodes.Germany);
			pivot.CusLineTariffDetails.AddNew("5P1", "1020101012");
			pivot.CusLineTariffDetails.AddNew("3P1", "1030101013");
			pivot.CusLineTariffDetails.AddNew("3P2", "1030101014");
			pivot.CusLineTariffDetails.AddNew("4P2", "1030101015");
			instruction.CEI_Style = "40";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertEquals(2, invoiceLine.CusLineTariffDetails.Count);
			AssertEquals("1030101013", invoiceLine.CusLineTariffDetails[0].BZ_Tariff);
			AssertEquals("1030101014", invoiceLine.CusLineTariffDetails[1].BZ_Tariff);
		}

		public void TestProcedureMeasure()
		{
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var rateType_ZA_REF = universalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "REF", "Refd");
			universalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "6P1", rateType_ZA_REF.PK);
			Factory.Save();
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "66666", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", ProcedureCodes._64, "00", "6", "", "EXP", calculateDuty: false, landedCostOnly: true);
			Factory.Save();

			var ceInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			ceInstruction.CEI_Style = "64";
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.JI_Description = "abcde";
			invoiceLine.CusLineTariffDetails.RemoveAll();
			invoiceLine.JI_Procedure = "6400";
			AssertNull(invoiceLine.ProcedureMeasureTariff);
			invoiceLine.CusLineTariffDetails.AddNew("6P1", "66666");
			var procedureMeasureTariff = invoiceLine.ProcedureMeasureTariff;
			AssertNotNull("procedureMeasureTariff", procedureMeasureTariff);
			AssertEquals("procedureMeasureTariff.ZZ1_ZZI_TariffTypeCode", "6P1", procedureMeasureTariff.ZZ1_ZZI_TariffTypeCode);
		}

		public void TestProcedureMeasureWithAdditionalTariff_Schedule6()
		{
			var tariffType6P5 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P5");
			var tariffType6P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			Factory.Save();
			universalReferenceDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "691010101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalReferenceDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P5.PK, "691010100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H",
				UniversalReferenceConstants.ProcedureCodes._68, UniversalReferenceConstants.ProcedureCodes._00, string.Empty,
				"TEST PROCEDURE 1", ZAJobMessageTypeList.Codes.Export, string.Empty);
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H",
				UniversalReferenceConstants.ProcedureCodes._52, UniversalReferenceConstants.ProcedureCodes._00, string.Empty,
				"TEST PROCEDURE 2", ZAJobMessageTypeList.Codes.Export, string.Empty);
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H",
				UniversalReferenceConstants.ProcedureCodes._40, UniversalReferenceConstants.ProcedureCodes._00, string.Empty,
				"TEST PROCEDURE 3", ZAJobMessageTypeList.Codes.Import, string.Empty);

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			CombineAssertions(() =>
			{
				AssertProcedureMeasureWithAdditionalTariff_Schedule6("6P5&Export&_68", hasProcedureMeasureTariff: true, "6P5", "691010100", ZAJobMessageTypeList.Codes.Export, UniversalReferenceConstants.ProcedureCodes._68);
				AssertProcedureMeasureWithAdditionalTariff_Schedule6("6P5&Import&_68", hasProcedureMeasureTariff: false, "6P5", "691010100", ZAJobMessageTypeList.Codes.Import, UniversalReferenceConstants.ProcedureCodes._68);
				AssertProcedureMeasureWithAdditionalTariff_Schedule6("6P1&Export&_52", hasProcedureMeasureTariff: true, "6P1", "691010101", ZAJobMessageTypeList.Codes.Export, UniversalReferenceConstants.ProcedureCodes._52);
				AssertProcedureMeasureWithAdditionalTariff_Schedule6("6P1&Import&_40", hasProcedureMeasureTariff: false, "6P1", "691010101", ZAJobMessageTypeList.Codes.Import, UniversalReferenceConstants.ProcedureCodes._40);
			});
		}

		void AssertProcedureMeasureWithAdditionalTariff_Schedule6(string message, bool hasProcedureMeasureTariff, string tariffType, string tariff, string messageType, string procedure)
		{
			declaration.JE_MessageType = messageType;
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			entryInstruction.CEI_Style = procedure;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = procedure + UniversalReferenceConstants.ProcedureCodes._00;
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Tariff = tariff;
			tariffDetail.BZ_Type = tariffType;

			if (hasProcedureMeasureTariff)
			{
				AssertEquals(message, tariff, invoiceLine.ProcedureMeasureTariff.ZZ1_TariffCode);
			}
			else
			{
				AssertNull(message, invoiceLine.ProcedureMeasureTariff);
			}
		}

		public void TestProcedureCategory_PreviousProcedureCategory()
		{
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "10", "", "", "A", "", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "F", "51", "", "", "F", "", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "D", "35", "", "", "D", "", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "20", "", "", "B", "", "");
			universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "48", "", "", "E", "", "");
			Factory.Save();
			var instruction = Factory.NewMoq<CusEntryInstruction>().Object;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = ProcedureCodes._00 + "00";
			AssertEquals("Empty", ZString.Empty, invoiceLine.ProcedureCategory);
			instruction.CEI_Style = ProcedureCodes._10;
			AssertEquals("Category A", UniversalReferenceConstants.ProcedureCategoryCodes._A, invoiceLine.ProcedureCategory);
			instruction.CEI_Style = ProcedureCodes._51;
			AssertEquals("Category F", UniversalReferenceConstants.ProcedureCategoryCodes._F, invoiceLine.ProcedureCategory);
			instruction.CEI_Style = ProcedureCodes._35;
			AssertEquals("Category D", UniversalReferenceConstants.ProcedureCategoryCodes._D, invoiceLine.ProcedureCategory);
			instruction.CEI_Style = ProcedureCodes._10;
			invoiceLine.JI_Procedure = "10" + ProcedureCodes._20;
			AssertEquals("Previous Category B", UniversalReferenceConstants.ProcedureCategoryCodes._B, invoiceLine.PreviousProcedureCategory);
			invoiceLine.JI_Procedure = "10" + ProcedureCodes._48;
			AssertEquals("Previous Category E", UniversalReferenceConstants.ProcedureCategoryCodes._E, invoiceLine.PreviousProcedureCategory);
		}

		public void TestSettingDefaultUOMFromTariffAndAdditionalTariffs()
		{
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "11", "00", "", "", "");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "15A");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";
			Factory.Save();
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs1(invoiceLine, tariffType1P1.PK);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs2(invoiceLine, tariffType1P1.PK);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs3(invoiceLine, tariffType1P1.PK);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs4(invoiceLine, tariffType1P1.PK);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs5(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs6(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs7(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs8(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs9(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs10(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs11(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs12(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs13(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs14(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs15(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs16(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs17(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs18(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs19(invoiceLine);
			AssertSettingDefaultUOMFromTariffAndAdditionalTariffs20(invoiceLine);
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs1(JobComInvoiceLine invLine, ZGuid tariffPK)
		{
			CombineAssertions("Tariff 1: CU1", () =>
			{
				var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffPK, "99991101", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
				universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				Factory.Save();
				invLine.JI_Tariff = "99991101";
				AssertUnitQuantities(invLine, "LI", "", "", isReadOnly: false, 0);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs2(JobComInvoiceLine invLine, ZGuid tariffPK)
		{
			CombineAssertions("Tariff 2: CU1 + CU2", () =>
			{
				var tariff2 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffPK, "99991102", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
				universalReferenceDataHelper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
				universalReferenceDataHelper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "LI");
				Factory.Save();
				invLine.JI_Tariff = "99991102";
				AssertUnitQuantities(invLine, "KG", "LI", "", isReadOnly: false, 0);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs3(JobComInvoiceLine invLine, ZGuid tariffPK)
		{
			CombineAssertions("Tariff 3: CU1 + RU1", () =>
			{
				var tariff3 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffPK, "99991103", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
				universalReferenceDataHelper.CreateTariffUOM(tariff3, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff3, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "KG");
				Factory.Save();
				invLine.JI_Tariff = "99991103";
				AssertUnitQuantities(invLine, "LI", "KG", "", isReadOnly: false, 0);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs4(JobComInvoiceLine invLine, ZGuid tariffPK)
		{
			CombineAssertions("Tariff 4: CU1 + CU2 + RU1", () =>
			{
				var tariff4 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffPK, "99991104", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
				universalReferenceDataHelper.CreateTariffUOM(tariff4, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff4, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "ML");
				universalReferenceDataHelper.CreateTariffUOM(tariff4, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "KG");
				Factory.Save();
				invLine.JI_Tariff = "99991104";
				AssertUnitQuantities(invLine, "LI", "ML", "KG", isReadOnly: false, 0);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs5(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 5: CU1, child 1: CU1", () =>
			{
				var tariff5 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991105", Universal.Constants.RateTypes.Rebate);
				tariff5.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff5, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				var tariff5_child = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "99991205", Universal.Constants.RateTypes.Duty);
				tariff5_child.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff5_child, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
				universalReferenceDataHelper.CreateTariffRelationship(tariff5_child.PK, tariff5.ZZ1_ZZI_TariffType, "99991105");
				Factory.Save();
				invLine.JI_Tariff = "99991105";
				AssertUnitQuantities(invLine, "LI", "KG", "", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs6(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 6: CU1 + CU2, child 1: CU1", () =>
			{
				var tariff6 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991106", Universal.Constants.RateTypes.Rebate);
				tariff6.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff6, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff6, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				var tariff6_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991206", Universal.Constants.RateTypes.Rebate);
				tariff6_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff6_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffRelationship(tariff6_child1.PK, tariff6.ZZ1_ZZI_TariffType, "99991106");
				Factory.Save();
				invLine.JI_Tariff = "99991106";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs7(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 7: CU1 + CU2 + RU1, child 1: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991107", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991207", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991107");
				Factory.Save();
				invLine.JI_Tariff = "99991107";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs8(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 8: CU1 + CU2 + RU1, child 1: CU1 + CU2", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991108", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991208", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991108");
				Factory.Save();
				invLine.JI_Tariff = "99991108";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs9(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 9: CU1, child 1: CU1 + CU2", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991109", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991209", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991109");
				Factory.Save();
				invLine.JI_Tariff = "99991109";
				AssertUnitQuantities(invLine, "LI", "KM", "GJ", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs10(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 10: CU1, child 1: CU1 + RU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991110", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991210", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991110");
				Factory.Save();
				invLine.JI_Tariff = "99991110";
				AssertUnitQuantities(invLine, "LI", "KM", "GJ", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs11(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 11: CU1 + RU1, child 1: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991111", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991211", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991111");
				Factory.Save();
				invLine.JI_Tariff = "99991111";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs12(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 12: CU1 + RU1, child 1: CU1 + CU2", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991112", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991212", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991112");
				Factory.Save();
				invLine.JI_Tariff = "99991112";
				AssertUnitQuantities(invLine, "LI", "KM", "ML", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs13(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 13: CU1 + RU1, child 1: CU1 + RU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991113", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991213", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991113");
				Factory.Save();
				invLine.JI_Tariff = "99991113";
				AssertUnitQuantities(invLine, "LI", "KM", "ML", isReadOnly: false, 1);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs14(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 14: CU1, child 1: CU1, child 2: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991114", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991214", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991114");
				var tariff_child2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "REB", "99991314", Universal.Constants.RateTypes.Rebate);
				tariff_child2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child2.PK, tariff.ZZ1_ZZI_TariffType, "99991114");
				Factory.Save();
				invLine.JI_Tariff = "99991114";
				AssertUnitQuantities(invLine, "LI", "KM", "GJ", isReadOnly: false, 2);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs15(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 15: CU1 + CU2, child 1: CU1, child 2: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991115", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991215", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991115");
				var tariff_child2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "REB", "99991315", Universal.Constants.RateTypes.Rebate);
				tariff_child2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child2.PK, tariff.ZZ1_ZZI_TariffType, "99991115");
				Factory.Save();
				invLine.JI_Tariff = "99991115";
				AssertUnitQuantities(invLine, "LI", "ML", "KM", isReadOnly: false, 2);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs16(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 16: CU1 + CU2, child 1: CU1 + CU2, child 2: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991116", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991216", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991116");
				var tariff_child2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "REB", "99991316", Universal.Constants.RateTypes.Rebate);
				tariff_child2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KB");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child2.PK, tariff.ZZ1_ZZI_TariffType, "99991116");
				Factory.Save();
				invLine.JI_Tariff = "99991116";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 2);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs17(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 17: CU1 + CU2 + RU1, child 1: CU1, child 2: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991117", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991217", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991117");
				var tariff_child2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "REB", "99991317", Universal.Constants.RateTypes.Rebate);
				tariff_child2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "GB");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child2.PK, tariff.ZZ1_ZZI_TariffType, "99991117");
				Factory.Save();
				invLine.JI_Tariff = "99991117";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 2);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs18(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 18: CU1 + CU2 + RU1, child 1: CU1 + CU2, child 2: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991118", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991218", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991118");
				var tariff_child2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "REB", "99991318", Universal.Constants.RateTypes.Rebate);
				tariff_child2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "GB");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child2.PK, tariff.ZZ1_ZZI_TariffType, "99991118");
				Factory.Save();
				invLine.JI_Tariff = "99991118";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 2);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs19(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 19: CU1 + RU1, child 1: CU1, child 2: CU1", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991119", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991219", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991119");
				var tariff_child2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "REB", "99991319", Universal.Constants.RateTypes.Rebate);
				tariff_child2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "GB");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child2.PK, tariff.ZZ1_ZZI_TariffType, "99991119");
				Factory.Save();
				invLine.JI_Tariff = "99991119";
				AssertUnitQuantities(invLine, "LI", "KN", "KM", isReadOnly: false, 2);
			});
		}

		void AssertSettingDefaultUOMFromTariffAndAdditionalTariffs20(JobComInvoiceLine invLine)
		{
			CombineAssertions("Tariff 20: CU1 + RU1, child 1: CU1 + CU2", () =>
			{
				var tariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "REB", "99991120", Universal.Constants.RateTypes.Rebate);
				tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
				universalReferenceDataHelper.CreateTariffUOM(tariff, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "ML");
				var tariff_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "99991220", Universal.Constants.RateTypes.Rebate);
				tariff_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KM");
				universalReferenceDataHelper.CreateTariffUOM(tariff_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "GJ");
				universalReferenceDataHelper.CreateTariffRelationship(tariff_child1.PK, tariff.ZZ1_ZZI_TariffType, "99991120");
				Factory.Save();
				invLine.JI_Tariff = "99991120";
				AssertUnitQuantities(invLine, "LI", "KM", "ML", isReadOnly: false, 1);
			});
		}

		void AssertUnitQuantities(JobComInvoiceLine invLine, ZString unit1, ZString unit2, ZString unit3, ZBool isReadOnly, int tariffCount)
		{
			AssertEquals(unit1, invLine.JI_CustomsUnitQty);
			AssertEquals(unit2, invLine.JI_CustomsSecondUnitQty);
			AssertEquals(unit3, invLine.JI_CustomsThirdUnitQty);
			AssertEquals(isReadOnly, invLine.JI_CustomsUnitQtyInfo.ReadOnly);
			AssertEquals(isReadOnly, invLine.JI_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals(isReadOnly, invLine.JI_CustomsThirdUnitQtyInfo.ReadOnly);
			AssertEquals(tariffCount, invLine.CusLineTariffDetails.Count);
			invLine.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			invLine.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
		}

		public override void TestJI_FormattedTariff()
		{
			var tariff = "12345678";
			invoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.78", invoiceLine.JI_FormattedTariff);
			tariff = "9876 .54 .32";
			invoiceLine.JI_FormattedTariff = tariff;
			AssertEquals("JI_FormattedTariff", "9876.54.32", invoiceLine.JI_FormattedTariff);
		}

		public void TestTariffFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var orgEntry = declaration.ActiveEntryHeaders.AddNew();
			orgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var entryLine = orgEntry.MergedLines.AddNew();
			entryLine.CL_AdValoremTariff = "TESTTRF1";
			entryLine.CL_LineNumber = 2;
			Factory.Save();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "XX";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}XX";
			invoiceLine.JI_PreviousEntryNumber = "TestMRN";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(ZString.Empty, invoiceLine.TariffFromRelatedImportBOE);
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(ZString.Empty, invoiceLine.TariffFromRelatedImportBOE);
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "YY";
			AssertEquals("TESTTRF1", invoiceLine.TariffFromRelatedImportBOE);
			invoiceLine.JI_Procedure = "YY";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(ZString.Empty, invoiceLine.TariffFromRelatedImportBOE);
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "YY";
			invoiceLine.JI_PreviousEntryNumber = "TestMRN1";
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(ZString.Empty, invoiceLine.TariffFromRelatedImportBOE);
		}

		public void TestRefreshDA63Values_Normal()
		{
			var testLine = SetupRefreshDA63Values_Normal();
			CombineAssertions(() =>
			{
				AssertEquals("", testLine.JI_ImportTariff);
				AssertEquals(0m, testLine.JI_ImportCustomsQty);
				AssertEquals("", testLine.JI_ImportCustomsQtyUQ);
				AssertEquals(0m, testLine.JI_ImportCustomsValue);
				AssertEquals(0m, testLine.JI_ImportDutyPaid);
				AssertEquals(0m, testLine.JI_ImportSch1P2BPaid);
				AssertEquals(0m, testLine.JI_ImportVATPaid);
				AssertEquals(0m, testLine.JI_ImportProvisionalPayment);
				AssertEquals(0m, testLine.JI_ImportPenalty);
				AssertEquals(0m, testLine.JI_ImportCustomsQty2);
				AssertEquals("", testLine.JI_ImportCustomsQty2UQ);
				AssertEquals(0m, testLine.JI_ImportCustomsQty3);
				AssertEquals("", testLine.JI_ImportCustomsQty3UQ);
				AssertEquals(0m, testLine.JI_ConversionFactor);
				AssertEquals(0m, testLine.DA63AdditionalDuties.S1P2BDuty?.CY_Value ?? 0m);
				AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.CustomsDutiesExcluding12B.Sum(x => x.CY_Value));
			});
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			testLine.RepopulateDA63AdditionalDuties();
			testLine.DA63NeedsRecalculation = true;
			testLine.RecalculateDA63Values();
			CombineAssertions(() =>
			{
				AssertEquals("99991", testLine.JI_ImportTariff);
				AssertEquals(100m, testLine.JI_ImportCustomsQty);
				AssertEquals("KG", testLine.JI_ImportCustomsQtyUQ);
				AssertEquals(1000m, testLine.JI_ImportCustomsValue);
				AssertEquals(23.5m, testLine.JI_ImportDutyPaid);
				AssertEquals(11m, testLine.JI_ImportSch1P2BPaid);
				AssertEquals(0m, testLine.JI_ImportVATPaid);
				AssertEquals(0m, testLine.JI_ImportProvisionalPayment);
				AssertEquals(0m, testLine.JI_ImportPenalty);
				AssertEquals(0m, testLine.JI_ImportCustomsQty2);
				AssertEquals("ML", testLine.JI_ImportCustomsQty2UQ);
				AssertEquals(0m, testLine.JI_ImportCustomsQty3);
				AssertEquals("", testLine.JI_ImportCustomsQty3UQ);
				AssertEquals(1m, testLine.JI_ConversionFactor);
				AssertEquals(11m, testLine.DA63AdditionalDuties.S1P2BDuty.CY_Value);
				AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.CY_Value));
				AssertEquals(20m, testLine.DA63AdditionalDuties.CustomsDutiesExcluding12B.Sum(x => x.CY_Value));
			});
		}

		public void TestRefreshDA63Values_NoOriginal()
		{
			var testLine = SetupRefreshDA63Values_NoOriginal();
			CombineAssertions(() =>
			{
				AssertEquals("99992", testLine.JI_ImportTariff);
				AssertEquals(100m, testLine.JI_ImportCustomsQty);
				AssertEquals("LI", testLine.JI_ImportCustomsQtyUQ);
				AssertEquals(0m, testLine.JI_ImportCustomsValue);
				AssertEquals(0m, testLine.JI_ImportDutyPaid);
				AssertEquals(0m, testLine.JI_ImportSch1P2BPaid);
				AssertEquals(0m, testLine.JI_ImportVATPaid);
				AssertEquals(0m, testLine.JI_ImportProvisionalPayment);
				AssertEquals(0m, testLine.JI_ImportPenalty);
				AssertEquals(0m, testLine.JI_ImportCustomsQty2);
				AssertEquals("", testLine.JI_ImportCustomsQty2UQ);
				AssertEquals(0m, testLine.JI_ImportCustomsQty3);
				AssertEquals("", testLine.JI_ImportCustomsQty3UQ);
				AssertEquals(0m, testLine.JI_ImportSch1P2BPaid);
				AssertEquals(0m, testLine.JI_ImportProvisionalPayment);
				AssertEquals(0m, testLine.JI_ImportPenalty);
				AssertEquals(0m, testLine.DA63AdditionalDuties.CustomsDutiesExcluding12B.Sum(x => x.CY_Value));
			});
		}

		JobComInvoiceLine SetupRefreshDA63Values_Normal()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
			var testTariff2 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "ML");
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "KG");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CustomsQuantity = 150;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			invoiceLine.JI_LinePrice = 42.42m;
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testOrgInvLine2 = invoiceHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			invoiceLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 30);
			testEntryLine.Fees.AddOrUpdate("12A", 23);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			testEntryLine.Fees.AddOrUpdate("2P2", 17);
			testEntryLine.Fees.AddOrUpdate("VAT", 27);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			Factory.Save();

			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_Tariff = "99991";
			testLine.JI_CustomsQuantity = 100m;
			testLine.JI_CustomsUnitQty = "KG";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = testLine.EntryInstruction.CEI_Style + "XX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RecalculateDA63Values();
			return testLine;
		}

		JobComInvoiceLine SetupRefreshDA63Values_NoOriginal()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			var testTariff2 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff2, "CU1", "LI");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CustomsQuantity = 150;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testOrgInvLine2 = invoiceHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			invoiceLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			Factory.Save();

			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_Tariff = "99992";
			testLine.JI_CustomsQuantity = 100m;
			testLine.JI_CustomsUnitQty = "LI";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			testLine.RecalculateDA63Values();
			return testLine;
		}

		public void TestRefreshDA63Values_UnitMismatch()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			var testTariff2 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff2, "CU1", "LI");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CustomsQuantity = 50;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			invoiceLine.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			Factory.Save();

			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_Tariff = "99992";
			testLine.JI_CustomsQuantity = 10m;
			testLine.JI_CustomsUnitQty = "LI";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = testLine.EntryInstruction.CEI_Style + "YY";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RecalculateDA63Values();
			CombineAssertions(() =>
			{
				AssertEquals("99991", testLine.JI_ImportTariff);
				AssertEquals(0m, testLine.JI_ImportCustomsQty);
				AssertEquals("KG", testLine.JI_ImportCustomsQtyUQ);
				AssertEquals(0m, testLine.JI_ImportCustomsValue);
				AssertEquals(0m, testLine.JI_ImportDutyPaid);
				AssertEquals(0m, testLine.JI_ImportSch1P2BPaid);
				AssertEquals(0m, testLine.JI_ImportVATPaid);
				AssertEquals(0m, testLine.JI_ImportProvisionalPayment);
				AssertEquals(0m, testLine.JI_ImportPenalty);
				AssertEquals(0m, testLine.JI_ImportCustomsQty2);
				AssertEquals("", testLine.JI_ImportCustomsQty2UQ);
				AssertEquals(0m, testLine.JI_ImportCustomsQty3);
				AssertEquals("", testLine.JI_ImportCustomsQty3UQ);
				AssertEquals(0m, testLine.DA63AdditionalDuties.S1P2BDuty.CY_Value);
				AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.CustomsDutiesExcluding12B.Sum(x => x.CY_Value));
			});
		}

		public void TestRefreshDA63Values_ZeroOriginalQty()
		{
			var testLine = SetupRefreshDA63ValuesZero(0);
			testLine.JI_CustomsQuantity = 100m;
			testLine.RecalculateDA63Values();
			CombineAssertions(() =>
			{
				AssertEquals("99991", testLine.JI_ImportTariff);
				AssertEquals(100m, testLine.JI_ImportCustomsQty);
				AssertEquals("KG", testLine.JI_ImportCustomsQtyUQ);
				AssertEquals(20000000m, testLine.JI_ImportCustomsValue);
				AssertEquals(210000m, testLine.JI_ImportDutyPaid);
				AssertEquals(220000m, testLine.JI_ImportSch1P2BPaid);
				AssertEquals(0m, testLine.JI_ImportVATPaid);
				AssertEquals(0m, testLine.JI_ImportProvisionalPayment);
				AssertEquals(0m, testLine.JI_ImportPenalty);
				AssertEquals(0m, testLine.JI_ImportCustomsQty2);
				AssertEquals("", testLine.JI_ImportCustomsQty2UQ);
				AssertEquals(0m, testLine.JI_ImportCustomsQty3);
				AssertEquals("", testLine.JI_ImportCustomsQty3UQ);
				AssertEquals(220000m, testLine.DA63AdditionalDuties.S1P2BDuty.CY_Value);
				AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.CustomsDutiesExcluding12B.Sum(x => x.CY_Value));
			});
		}

		public void TestRefreshDA63Values_ZeroCurrentQty()
		{
			var testLine = SetupRefreshDA63ValuesZero(50);
			testLine.JI_CustomsQuantity = 0m;
			testLine.RecalculateDA63Values();
			CombineAssertions(() =>
			{
				AssertEquals("99991", testLine.JI_ImportTariff);
				AssertEquals(0m, testLine.JI_ImportCustomsQty);
				AssertEquals("KG", testLine.JI_ImportCustomsQtyUQ);
				AssertEquals(0m, testLine.JI_ImportCustomsValue);
				AssertEquals(0m, testLine.JI_ImportDutyPaid);
				AssertEquals(0m, testLine.JI_ImportSch1P2BPaid);
				AssertEquals(0m, testLine.JI_ImportVATPaid);
				AssertEquals(0m, testLine.JI_ImportProvisionalPayment);
				AssertEquals(0m, testLine.JI_ImportPenalty);
				AssertEquals(0m, testLine.JI_ImportCustomsQty2);
				AssertEquals("", testLine.JI_ImportCustomsQty2UQ);
				AssertEquals(0m, testLine.JI_ImportCustomsQty3);
				AssertEquals("", testLine.JI_ImportCustomsQty3UQ);
				AssertEquals(0m, testLine.DA63AdditionalDuties.S1P2BDuty.CY_Value);
				AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.CY_Value));
				AssertEquals(0m, testLine.DA63AdditionalDuties.CustomsDutiesExcluding12B.Sum(x => x.CY_Value));
			});
		}

		JobComInvoiceLine SetupRefreshDA63ValuesZero(ZDecimal customsQty)
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			var testTariff2 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff2, "CU1", "LI");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CustomsQuantity = customsQty;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			invoiceLine.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			Factory.Save();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testLine = invoiceHeader.InvoiceLines.AddNew();
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Tariff = "99991";
			testLine.JI_CustomsUnitQty = "KG";
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 2;
			return testLine;
		}

		public void TestDefaultingJI_ImportCustomsQtyUQ()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			var testTariff1 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			var testTariff2 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff2, "CU1", "LI");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "XX";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}YY";
			invoiceLine.JI_ImportTariff = "99991";
			AssertDefaultingJI_ImportCustomsQtyUQ("KG");
			invoiceLine.JI_ImportTariff = "99992";
			AssertDefaultingJI_ImportCustomsQtyUQ("LI");
			invoiceLine.JI_ImportTariff = "99993";
			AssertDefaultingJI_ImportCustomsQtyUQ(ZString.Empty);
		}

		void AssertDefaultingJI_ImportCustomsQtyUQ(ZString importCustomsQtyUQ)
		{
			AssertEquals(importCustomsQtyUQ, invoiceLine.JI_ImportCustomsQtyUQ);
			AssertEquals(expected: false, invoiceLine.JI_ImportCustomsQtyUQInfo.ReadOnly);
			AssertEquals("", invoiceLine.JI_ImportCustomsQty2UQ);
			AssertEquals(expected: false, invoiceLine.JI_ImportCustomsQty2UQInfo.ReadOnly);
			AssertEquals("", invoiceLine.JI_ImportCustomsQty3UQ);
			AssertEquals(expected: false, invoiceLine.JI_ImportCustomsQty3UQInfo.ReadOnly);
		}

		public void TestCustomsUQ()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "NO");
			var tariff2 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "88888", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
			universalReferenceDataHelper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
			Factory.Save();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				invoiceLine.JI_Tariff = "99999";
				AssertEquals("Test1-Customs Unit", "NO", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Test1-Customs Unit2", "", invoiceLine.JI_CustomsSecondUnitQty);
				invoiceLine.JI_Tariff = "88888";
				AssertEquals("Test2-Customs Unit", "KG", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Test2-Customs Unit2", "KN", invoiceLine.JI_CustomsSecondUnitQty);
			});
		}

		public void TestEffectiveCustomsUOMDefaulting()
		{
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "991001", Universal.Constants.RateTypes.AntiDumping);
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "NO");
			var tariff2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "REB", "991012", Universal.Constants.RateTypes.Rebate);
			universalReferenceDataHelper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LA");
			tariff2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, tariff1.ZZ1_TariffCode);
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "991001";
			AssertEquals("Test1-Customs Unit", "LI", invoiceLine.JI_CustomsUnitQty);
			AssertEquals("Test2-Customs Additional Unit", "NO", invoiceLine.JI_CustomsSecondUnitQty);
			AssertNullOrEmpty(invoiceLine.JI_CustomsThirdUnitQty);
			invoiceLine.CusLineTariffDetails.RemoveAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = "991012";
			AssertEquals("Test3-Customs Additional Unit", "LA", invoiceLine.JI_CustomsSecondUnitQty);
		}

		public void TestSuspendDefaultUOMFromTariff()
		{
			var procedure = universalReferenceDataHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "11", "00", "", "", "", "");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var tariffType13A = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			var tariff4 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "99991104", Universal.Constants.RateTypes.AntiDumping);
			tariff4.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffUOM(tariff4, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			universalReferenceDataHelper.CreateTariffUOM(tariff4, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KG");
			universalReferenceDataHelper.CreateTariffUOM(tariff4, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "KG");
			var tariff5 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "99991105", Universal.Constants.RateTypes.AntiDumping);
			universalReferenceDataHelper.CreateTariffUOM(tariff5, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			universalReferenceDataHelper.CreateTariffUOM(tariff5, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
			universalReferenceDataHelper.CreateTariffUOM(tariff5, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "KN");
			tariff5.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff6 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "99991106", Universal.Constants.RateTypes.AntiDumping);
			universalReferenceDataHelper.CreateTariffUOM(tariff6, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			universalReferenceDataHelper.CreateTariffUOM(tariff6, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "KN");
			tariff6.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff6_child1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "99991216", Universal.Constants.RateTypes.Duty);
			tariff6_child1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffUOM(tariff6_child1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KN");
			var tariff6_child2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "DTY", "99991316", Universal.Constants.RateTypes.Duty);
			tariff6_child2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			universalReferenceDataHelper.CreateTariffUOM(tariff6_child2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LA");
			universalReferenceDataHelper.CreateTariffRelationship(tariff6_child1.PK, tariff6.ZZ1_ZZI_TariffType, "99991106");
			universalReferenceDataHelper.CreateTariffRelationship(tariff6_child2.PK, tariff6.ZZ1_ZZI_TariffType, "99991106");
			Factory.Save();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invLine = Factory.New<InvoiceLineForUOMSuspendDefaultingForTest>();
			invoiceHeader.InvoiceLines.Add(invLine);
			invLine.JI_JZ = invoiceHeader.PK;
			invLine.JI_CEI = instruction.PK;
			invLine.JI_Procedure = "1100";
			CombineAssertions(() =>
			{
				AssertEquals("", invLine.JI_CustomsSecondUnitQty);
				AssertEquals(0, invLine.AdditionalUnitSetCount);
				invLine.JI_Tariff = "99991104";
				AssertEquals("KG", invLine.JI_CustomsSecondUnitQty);
				AssertEquals(1, invLine.AdditionalUnitSetCount);
				using (invLine.SuspendUOMDefaulting())
				{
					invLine.JI_Tariff = "99991105";
				}

				AssertEquals("99991105", invLine.JI_Tariff);
				AssertEquals("KG", invLine.JI_CustomsSecondUnitQty);
				AssertEquals(1, invLine.AdditionalUnitSetCount);
				invLine.JI_Tariff = "99991106";
				AssertEquals("KN", invLine.JI_CustomsSecondUnitQty);
				AssertEquals("LA", invLine.JI_CustomsThirdUnitQty);
				AssertEquals(2, invLine.CusLineTariffDetails.Count);
				AssertEquals(2, invLine.AdditionalUnitSetCount);
			});
		}

		public void TestScheduleConcessionDefaulting()
		{
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", ProcedureCodes._64, "00", "6", "", "EXP");
			var tariffType6P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "101010", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var ceInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			ceInstruction.CEI_Style = "64";
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.JI_CEI = ceInstruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";
			invoiceLine.JI_Tariff = "101010";
			Assert("Count is 1", invoiceLine.CusLineTariffDetails.Count == 1);
			AssertEquals("6P1", invoiceLine.CusLineTariffDetails[0].BZ_Type);
		}

		[TestDate(2013, 03, 13, 13, 13, 33)]
		public void TestDutyFormulaDescriptionWhenEffectiveAssessmentDateIsChanged()
		{
			var tariff1P1 = SetupDutyFormula(true);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ProcedureCodes._11;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = tariff1P1.ZZ1_TariffCode;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}{ProcedureCodes._11}";
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("DutyFormulaDescription", "", invoiceLine.DutyFormulaDescription);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("EffectiveAssessmentDate is now", new ZDateTime(2013, 03, 13, 13, 13, 33), invoiceLine.EffectiveAssessmentDate);
			AssertEquals("DutyFormulaDescription", "20%", invoiceLine.DutyFormulaDescription);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 04, 14, 14, 14, 10);
			AssertEquals("EffectiveAssessmentDate is CEI_DateForDuty", new ZDateTime(2024, 04, 14, 14, 14, 10), invoiceLine.EffectiveAssessmentDate);
			AssertEquals("DutyFormulaDescription", "30%", invoiceLine.DutyFormulaDescription);
		}

		public void TestDutyFormulaDescription()
		{
			var tariff1P1 = SetupDutyFormula();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ProcedureCodes._11;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = tariff1P1.ZZ1_TariffCode;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._11;
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("DutyFormulaDescription", "", invoiceLine.DutyFormulaDescription);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("DutyFormulaDescription", "FREE", invoiceLine.DutyFormulaDescription);
		}

		TariffView SetupDutyFormula(bool whenEffectiveAssessment = false)
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = universalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = universalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(2075, 1, 1);
			var tariff1P1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101011", startDate, endDate);
			var testTradeGroup1 = universalReferenceDataHelper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			universalReferenceDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			if (whenEffectiveAssessment)
			{
				var tariff1P1Rate = universalReferenceDataHelper.CreateRate(tariff1P1, rateCode_ZA_DTY_D.PK, new ZDateTime(2010, 1, 1), new ZDateTime(2020, 1, 1), "0.2 * VFD", rateFormulaDeriveFrom: "20%");
				universalReferenceDataHelper.CreateCusApplicability(tariff1P1Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
				var tariff1P1Rate2 = universalReferenceDataHelper.CreateRate(tariff1P1, rateCode_ZA_DTY_D.PK, new ZDateTime(2020, 1, 2), new ZDateTime(2030, 1, 1), "0.3 * VFD", rateFormulaDeriveFrom: "30%");
				universalReferenceDataHelper.CreateCusApplicability(tariff1P1Rate2, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			else
			{
				var tariff1P1Rate = universalReferenceDataHelper.CreateRate(tariff1P1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.2 * VFD", rateFormulaDeriveFrom: "FREE");
				universalReferenceDataHelper.CreateCusApplicability(tariff1P1Rate, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2070, 06, 06));
			}
			Factory.Save();
			return tariff1P1;
		}

		public void TestProcedureMeasureWithRelatedTariff()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(2075, 1, 1);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", ProcedureCodes._85, "00", "3", "", "IMP");
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "5208##", startDate, endDate);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "5213##", startDate, endDate);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "5407##", startDate, endDate);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "3114201##", startDate, endDate, iamUnique: 0);
			universalReferenceDataHelper.CreateTariffRelationship(tariff1.PK, tariffType.PK, "5208");
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "42", tariff1);
			var tariff2 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "3114201##", startDate, endDate, iamUnique: 1);
			universalReferenceDataHelper.CreateTariffRelationship(tariff2.PK, tariffType.PK, "5213");
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "46", tariff2);
			var tariff3 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "3114201##", startDate, endDate, iamUnique: 2);
			universalReferenceDataHelper.CreateTariffRelationship(tariff3.PK, tariffType.PK, "5407");
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "43", tariff3);
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var importer = Factory.NewMoq<OrgHeader>().Object;
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "0110110", Core.Constants.CountryCodes.SouthAfrica);
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RebateUserCode, "0110111", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_Importer = importer.PK;
			var ceInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			ceInstruction.CEI_Style = ProcedureCodes._85;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.JI_CEI = ceInstruction.PK;
			invoiceLine.JI_Description = "ABCDEFG";
			invoiceLine.JI_Tariff = "5407##";
			invoiceLine.CusLineTariffDetails.RemoveAll();
			invoiceLine.CusLineTariffDetails.AddNew("3P1", "3114201##");

			new LineMerger(declaration).DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			MessageBuilders.ILineLevelInformation lineLevel = entryLine;
			AssertEquals("pre-condition", "3114201##43", lineLevel.ProcedureMeasure);
			invoiceLine.JI_Tariff = "5208##";
			invoiceLine.CusLineTariffDetails.RemoveAll();
			invoiceLine.CusLineTariffDetails.AddNew("3P1", "3114201##");

			new LineMerger(declaration).DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			lineLevel = entryLine;
			AssertEquals("pre-condition", "3114201##42", lineLevel.ProcedureMeasure);
			invoiceLine.JI_Tariff = "5213##";
			invoiceLine.CusLineTariffDetails.RemoveAll();
			invoiceLine.CusLineTariffDetails.AddNew("3P1", "3114201##");

			new LineMerger(declaration).DoMerge();
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			lineLevel = entryLine;
			AssertEquals("pre-condition", "3114201##46", lineLevel.ProcedureMeasure);
		}

		[TestDate(1990, 6, 1)]
		public void TestIsRefundRebateType5P()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.GetCultureInfo("en-ZA")))
			{
				PrepareRefundRebateLine("5P1", "5P2", "5", "5010101011", "5010101011");
				AssertEquals(expected: false, invoiceLine.IsRefundRebateType5P);
				invoiceLine.JI_Tariff = "1010101011";
				AssertEquals(expected: true, invoiceLine.IsRefundRebateType5P);
				invoiceLine.JI_Tariff = "5010101011";
				AssertEquals(expected: false, invoiceLine.IsRefundRebateType5P);
			}
		}

		[TestDate(1990, 6, 1)]
		public void TestRefundRebateCode()
		{
			PrepareRefundRebateLineRebateType6P();
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, invoiceLine.CusLineTariffDetails.Count);
			var tariffDetail = invoiceLine.CusLineTariffDetails[0];
			AssertCusLineTariffDetail(tariffDetail, "6P1", "6010101011", "Question For Testing", "", "{5,3}");
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			AssertEquals(0, invoiceLine.CusLineTariffDetails.Count);
			invoiceLine.RefundRebateCode = "6010101011";
			tariffDetail = invoiceLine.CusLineTariffDetails[0];
			AssertCusLineTariffDetail(tariffDetail, "6P1", "6010101011", "Question For Testing", "", "{5,3}");
		}

		[TestDate(1990, 6, 1)]
		public void TestRefundRebateValue()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				PrepareRefundRebateLineRebateType6P();
				AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, invoiceLine.CusLineTariffDetails.Count);
				var tariffDetail = invoiceLine.CusLineTariffDetails[0];
				AssertCusLineTariffDetail(tariffDetail, "6P1", "6010101011", "Question For Testing", "", "{5,3}");
				invoiceLine.RefundRebateValue = "23,3426";
				AssertCusLineTariffDetail(tariffDetail, "6P1", "6010101011", "Question For Testing", "23,343", "{5,3}23,343");
			}
		}

		[TestDate(1990, 6, 1)]
		public void TestIsRefundRebateTariff()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.GetCultureInfo("en-ZA")))
			{
				PrepareRefundRebateLineRebateType6P();
				var testInstruction = declaration.CustomsEntryInstructions.AddNew();
				testInstruction.CEI_Style = "XX";
				AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, invoiceLine.CusLineTariffDetails.Count);
				var tariffDetail = invoiceLine.CusLineTariffDetails[0];
				AssertEquals(expected: false, invoiceLine.IsRefundRebateTariff);
				tariffDetail.BZ_Tariff = "5220311111";
				AssertEquals(expected: true, invoiceLine.IsRefundRebateTariff);
				tariffDetail.BZ_Tariff = "536002222";
				AssertEquals(expected: true, invoiceLine.IsRefundRebateTariff);
				tariffDetail.BZ_Tariff = "1234567890";
				AssertEquals(expected: false, invoiceLine.IsRefundRebateTariff);
			}
		}

		void PrepareRefundRebateLineRebateType6P()
		{
			PrepareRefundRebateLine("6P1", "6P2", "6", "6010101011", "1010101011");
		}

		void PrepareRefundRebateLine(ZString typeCode1, ZString typeCode2, ZString refCusProcedureDescription, ZString tariffCode1, ZString tariffCode2)
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffTypeP1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, typeCode1);
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, typeCode2);
			var rateType_ZA_REB = universalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = universalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var preference = universalReferenceDataHelper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "", refCusProcedureDescription, "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "00", refCusProcedureDescription, "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101011", startDate, endDate);
			var tariff2 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffTypeP1.PK, tariffCode1, startDate, endDate);
			universalReferenceDataHelper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
			var tariff1Rate = universalReferenceDataHelper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			var rate1 = universalReferenceDataHelper.CreateRate(tariff2, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,3):""Question For Testing""\}");
			Factory.Save();
			var testTradeGroup1 = universalReferenceDataHelper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			universalReferenceDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			universalReferenceDataHelper.CreateCusApplicability(tariff1Rate, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2070, 06, 06));
			universalReferenceDataHelper.CreateCusApplicability(rate1, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "5#";
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.JI_Tariff = tariffCode2;
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
		}

		void AssertAdditionalScheduleIsDefaultedTariffDetail(JobComInvoiceLine invoiceLine, ZString tariffDetail1Type, ZString tariffDetail2Type, ZString tariffDetail3Type)
		{
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 3, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], tariffDetail1Type, "1010101011");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], tariffDetail2Type, "1020101012");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[2], tariffDetail3Type, tariffDetail3Type == "3P2" ? "1030101013" : "1040101014");
		}

		void AssertCusLineTariffDetail(CusLineTariffDetail tariffDetail, ZString type, ZString tariff)
		{
			AssertEquals("tariffDetail.BZ_Type", type, tariffDetail.BZ_Type);
			AssertEquals("tariffDetail.BZ_Tariff", tariff, tariffDetail.BZ_Tariff);
		}

		void AssertCusLineTariffDetail(CusLineTariffDetail tariffDetail, ZString type, ZString tariff, ZString question, ZString value, ZString storedValue)
		{
			AssertCusLineTariffDetail(tariffDetail, type, tariff);
			AssertCusLineTariffDetailFormulaSpecific(tariffDetail, question, value, storedValue);
		}

		void AssertCusLineTariffDetailFormulaSpecific(CusLineTariffDetail tariffDetail, ZString question, ZString value, ZString storedValue)
		{
			AssertEquals("tariffDetail.FormulaSpecificQuestion", question, tariffDetail.FormulaSpecificQuestion);
			AssertEquals("tariffDetail.FormulaSpecificValue", value, tariffDetail.FormulaSpecificValue);
			AssertEquals("storedValue", storedValue, tariffDetail.GetSystemDefinedValue<ZString>(CusLineTariffDetail.Schema.FormulaSpecificStoredValue));
		}
	}
}
