using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.Business.Testing;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class VOCLineDetailWrapperTest : TestCaseWithFactory
	{
		public void TestTariffCodes()
		{
			var dtyTariff = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "00001000", Universal.Constants.RateTypes.AntiDumping);
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "4", dtyTariff);
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "2000000", Universal.Constants.RateTypes.Excise);
			var tariff2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12B", "DTY", "3000000", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff3 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "2P1", "DTY", "3100000", Universal.Constants.RateTypes.AntiDumping);
			var tariff4 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "3P1", "DTY", "4000000", Universal.Constants.RateTypes.AntiDumping);
			var tariff5 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "4P1", "DTY", "5000000", Universal.Constants.RateTypes.AntiDumping);
			var tariff6 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "5P1", "DTY", "6000000", Universal.Constants.RateTypes.AntiDumping);
			var tariff7 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "6P1", "DTY", "7000000", Universal.Constants.RateTypes.AntiDumping);
			universalReferenceDataHelper.CreateTariffRelationship(tariff1.PK, dtyTariff.ZZ1_ZZI_TariffType, dtyTariff.ZZ1_TariffCode);
			universalReferenceDataHelper.CreateTariffRelationship(tariff2.PK, dtyTariff.ZZ1_ZZI_TariffType, dtyTariff.ZZ1_TariffCode);
			universalReferenceDataHelper.CreateTariffRelationship(tariff3.PK, dtyTariff.ZZ1_ZZI_TariffType, dtyTariff.ZZ1_TariffCode);
			universalReferenceDataHelper.CreateTariffRelationship(tariff4.PK, dtyTariff.ZZ1_ZZI_TariffType, dtyTariff.ZZ1_TariffCode);
			universalReferenceDataHelper.CreateTariffRelationship(tariff5.PK, dtyTariff.ZZ1_ZZI_TariffType, dtyTariff.ZZ1_TariffCode);
			universalReferenceDataHelper.CreateTariffRelationship(tariff6.PK, dtyTariff.ZZ1_ZZI_TariffType, dtyTariff.ZZ1_TariffCode);
			universalReferenceDataHelper.CreateTariffRelationship(tariff7.PK, dtyTariff.ZZ1_ZZI_TariffType, dtyTariff.ZZ1_TariffCode);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "40";
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			invLine.JI_Tariff = "00001000";
			invLine.CusLineTariffDetails.AddNew("12A", "2000000");
			invLine.CusLineTariffDetails.AddNew("12B", "3000000");
			invLine.CusLineTariffDetails.AddNew("2P1", "3100000");
			invLine.CusLineTariffDetails.AddNew("3P1", "4000000");
			invLine.CusLineTariffDetails.AddNew("4P1", "5000000");
			invLine.CusLineTariffDetails.AddNew("5P1", "6000000");
			invLine.CusLineTariffDetails.AddNew("6P1", "7000000");
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testLine = testHeader.MergedLines.AddNew();
			testLine.InvoiceLines.Add(invLine);
			var tester = new VOCLineDetailWrapper(testLine, testHeader, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("0000.10.00 (4)", tester.TariffCode);
				AssertEquals("310.00.00", tester.AntiDumpingTariffCode);
				AssertEquals("300.00.00", tester.AdValoremExciseTariffCode);
			});
		}

		public void TestValues_FromCusEntryLine()
		{
			CombineAssertions("EXP", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "40";
				var invHeader = testDeclaration.Invoices.AddNew();
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_CEI = testInst.PK;
				invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
				invLine.JI_Tariff = "00001000";
				invLine.JI_ZZF_NKTaxType = "VAT";
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				var testLine = testHeader.MergedLines.AddNew();
				testLine.CL_CustomsValue = 20m;
				testLine.Fees.AddOrUpdate("1P1", 500m);
				testLine.Fees.AddOrUpdate("12A", 400m);
				testLine.Fees.AddOrUpdate("12B", 300m);
				testLine.Fees.AddOrUpdate("VAT", 200m);
				testLine.ProvisionalPayments.AddNew("PPA", 50m);
				testLine.ProvisionalPayments.AddNew("PPR", 40m);
				testLine.ProvisionalPayments.AddNew("PEN", 30m);
				testLine.InvoiceLines.Add(invLine);
				var tester = new VOCLineDetailWrapper(testLine, testHeader, Factory);
				AssertEquals(20m, tester.CustomsValue);
				AssertEquals(900m, tester.CustomsDutyExcluding12B);
				AssertEquals(300m, tester.S1P2BDuty);
				AssertEquals(200m, tester.ValueAddedTax);
				AssertEquals(0m, tester.ProvisionalPaymentsAndPenalties);
			});
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "40";
				var invHeader = testDeclaration.Invoices.AddNew();
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_CEI = testInst.PK;
				invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
				invLine.JI_Tariff = "00001000";
				invLine.JI_ZZF_NKTaxType = "VAT";
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				var testLine = testHeader.MergedLines.AddNew();
				testLine.CL_CustomsValue = 20m;
				testLine.Fees.AddOrUpdate("1P1", 500m);
				testLine.Fees.AddOrUpdate("12A", 400m);
				testLine.Fees.AddOrUpdate("12B", 300m);
				testLine.Fees.AddOrUpdate("VAT", 200m);
				testLine.ProvisionalPayments.AddNew("PPA", 50m);
				testLine.ProvisionalPayments.AddNew("PPR", 40m);
				testLine.ProvisionalPayments.AddNew("PEN", 30m);
				testLine.InvoiceLines.Add(invLine);
				var tester = new VOCLineDetailWrapper(testLine, testHeader, Factory);
				AssertEquals(20m, tester.CustomsValue);
				AssertEquals(900m, tester.CustomsDutyExcluding12B);
				AssertEquals(300m, tester.S1P2BDuty);
				AssertEquals(200m, tester.ValueAddedTax);
				AssertEquals(120m, tester.ProvisionalPaymentsAndPenalties);
			});
		}

		public void TestValues_FromCUSDECMessageSG30Segment()
		{
			var testMessage = Factory.New<CUSDECEDIMessage>();
			testMessage.EM_MessageText = @"UNH+3242+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+00505655JSA20161020003240::00001+9'
CST++D:117:ZZZ'
LOC+14+A9::ZZZ'
LOC+35+DE::5'
LOC+36+ZA::5'
LOC+96+JSA::ZZZ'
LOC+9+DEHAM::5'
DTM+178:20161021:102'
GIS+D:134:ZZZ'
MEA+AAE+AAD+KGM:10.00'
FTX+LIN+++1::Y'
RFF+AAS:082-12345675'
DTM+137:20161020:102'
RFF+ABI:8120067395'
RFF+ACD:3242'
PAC+1'
TDT+20+SA132+4'
DOC+380+INVTEMP'
DTM+3:20161020:102'
NAD+IM+00549578++TIM IMPORT CO+88 STREETNAME DBN DURBAN 4362+DURBAN++4362'
RFF+VA:4320190434'
NAD+AG+00505655'
NAD+MS+TST'
UNS+D'
CST+0001+820110059:108:ZZZ+100'
FTX+AAA+++BASE METALS AND ARTICLES OF BASE METAL TOOLS, IMPLEMENTS, CUTLERY, SPO:ONS AND FORKS, OF BASE METAL; PARTS THEREOF OF BASE METAL HAND TOOLS, :THE FOLLOWING  SPADES, SHOVELS, MATTOCKS, PICKS, HOES, FORKS AND RAKES:; AXES, BILL HOOKSAND SIMILAR HEWING TOOLS; SECATEURS AND PRUNERS OF A:NY KIND; SCYTHES,SICKLES, HAY KNIVES, HEDGE SHEA REBATE AMOUNT; 1000.0'
FTX+ACB+++NUIN'
FTX+CCI+++35:00:48010010002'
LOC+27+JP'
MEA+AAR++KG:100.00'
NAD+WH+00549578'
MOA+38:5000'
MOA+40:5000'
TAX+1+1P1:107:ZZZ'
MOA+161:500.00'
TAX+1+12A:107:ZZZ'
MOA+161:400.00'
TAX+1+12B:107:ZZZ'
MOA+161:300.00'
TAX+1+VAT:107:ZZZ'
MOA+161:200.00'
TAX+1+PPT:107:ZZZ'
MOA+161:50.00'
TAX+1+PPR:107:ZZZ'
MOA+161:40.00'
TAX+1+PEN:107:ZZZ'
MOA+161:30.00'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:5000'
TAX+3+TDD:107:ZZZ'
MOA+161:500.00'
TAX+3+TVD:107:ZZZ'
MOA+161:770.00'
TAX+3+CUS:107:ZZZ'
MOA+161:5000'
UNT+48+3242'".Replace("\r\n", "");
			var tester = new VOCLineDetailWrapper(CUSDECMessageHelper.New(testMessage).LineLevelInformations[0], null, Factory);
			CombineAssertions(() =>
			{
				AssertEquals(5000m, tester.CustomsValue);
				AssertEquals(900m, tester.CustomsDutyExcluding12B);
				AssertEquals(300m, tester.S1P2BDuty);
				AssertEquals(200m, tester.ValueAddedTax);
				AssertEquals(120m, tester.ProvisionalPaymentsAndPenalties);
			});
		}

		public void TestAdValoremExciseTariffCodeFallbackToBO()
		{
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "10000000", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff1A = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "1000001", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff1B = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12B", "DTY", "1000002", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "20000000", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff2A = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "2000001", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff2B = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12B", "DTY", "2000002", Universal.Constants.RateTypes.AdValoremExcise);
			universalReferenceDataHelper.CreateTariffRelationship(tariff1A.PK, tariff1.ZZ1_ZZI_TariffType, "10000000");
			universalReferenceDataHelper.CreateTariffRelationship(tariff1B.PK, tariff1.ZZ1_ZZI_TariffType, "10000000");
			universalReferenceDataHelper.CreateTariffRelationship(tariff2A.PK, tariff2.ZZ1_ZZI_TariffType, "20000000");
			universalReferenceDataHelper.CreateTariffRelationship(tariff2B.PK, tariff2.ZZ1_ZZI_TariffType, "20000000");
			Factory.Save();
			CUSDECMessageDataProviderForTest messageDataProvider = new CUSDECMessageDataProviderForTest();
			messageDataProvider.LineLevelDetails = new LineLevelInformationForTest[]
			{
				new LineLevelInformationForTest
				{
					LineNumber = "0001",
					TariffCode = "10000000",
					AdditionalInformations = System.Array.Empty<AdditionalInformationForTest>(),
					DutiesAndFees = System.Array.Empty<DutyFeeInformationForTest>(),
					ProvisionalPayments = System.Array.Empty<DutyFeeInformationForTest>()
				},
				new LineLevelInformationForTest
				{
					LineNumber = "0002",
					TariffCode = "20000000",
					AdditionalInformations = System.Array.Empty<AdditionalInformationForTest>(),
					DutiesAndFees = System.Array.Empty<DutyFeeInformationForTest>(),
					ProvisionalPayments = System.Array.Empty<DutyFeeInformationForTest>()
				},
				new LineLevelInformationForTest
				{
					LineNumber = "0003",
					TariffCode = "20000000",
					AdditionalInformations = System.Array.Empty<AdditionalInformationForTest>(),
					DutiesAndFees = System.Array.Empty<DutyFeeInformationForTest>(),
					ProvisionalPayments = System.Array.Empty<DutyFeeInformationForTest>()
				},
				new LineLevelInformationForTest
				{
					LineNumber = "0004",
					TariffCode = "20000000",
					AdditionalInformations = System.Array.Empty<AdditionalInformationForTest>(),
					DutiesAndFees = System.Array.Empty<DutyFeeInformationForTest>(),
					ProvisionalPayments = System.Array.Empty<DutyFeeInformationForTest>()
				}
			};
			var textBuilder = new CUSDECMessageTextBuilderForTest(messageDataProvider);
			var message = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			message.EM_MessageText = textBuilder.GenerateMessageBody();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "10000000";
			invoiceLine1.CusLineTariffDetails.AddNew("12B", "1000002");
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "20000000";
			invoiceLine2.CusLineTariffDetails.AddNew("12B", "2000002");
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "10000000";
			invoiceLine3.CusLineTariffDetails.AddNew("12B", "1000002");
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "20000000";
			invoiceLine4.CusLineTariffDetails.AddNew("12A", "2000001");
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.Add(message);
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine3.InvoiceLines.Add(invoiceLine3);
			entryLine3.CL_LineNumber = 3;
			var entryLine4 = entryHeader.MergedLines.AddNew();
			entryLine4.InvoiceLines.Add(invoiceLine4);
			entryLine4.CL_LineNumber = 4;
			ICUSDECMessageDataProvider parsedMessageDataProvider = CUSDECMessageHelper.New(message);
			List<VOCLineDetailWrapper> lineWrappers = parsedMessageDataProvider.LineLevelDetails.Select(l => new VOCLineDetailWrapper(l, entryHeader, entryHeader.Factory)).ToList();
			AssertEquals(4, lineWrappers.Count);
			AssertEquals("100.00.02", lineWrappers[0].AdValoremExciseTariffCode);
			AssertEquals("200.00.02", lineWrappers[1].AdValoremExciseTariffCode);
			// value is empty because tariff in 3rd line does not match
			AssertEquals("", lineWrappers[2].AdValoremExciseTariffCode);
			// value is empty because tariff in 4th is not 12B
			AssertEquals("", lineWrappers[2].AdValoremExciseTariffCode);
		}

		protected override void SetUp()
		{
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}
}
