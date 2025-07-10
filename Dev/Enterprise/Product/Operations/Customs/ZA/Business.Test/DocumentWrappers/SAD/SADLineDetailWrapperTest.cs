using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(SADLineDetailWrapper))]
	sealed class SADLineDetailWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalcDutiesAndFees_NoException()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var wrapper = new SADLineDetailWrapper(null, testHeader);
			AssertNoExceptionThrown(() =>
			{
				_ = wrapper.CalcDutiesAndFees;
			});
		}

		public void TestIsEmpty()
		{
			var wrapper = new SADLineDetailWrapper(null, null);
			Assert(wrapper.IsEmpty);
			var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			testInvoiceLine.JI_ZZF_NKTaxType = "VAT";
			var testEntryLine = Factory.NewWithValidTestData<CusEntryLine>();
			testEntryLine.InvoiceLines.Add(testInvoiceLine);
			wrapper = new SADLineDetailWrapper(testEntryLine, null);
			Assert(!wrapper.IsEmpty);
		}

		public void TestSADLineDetailWrapper()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("BHR");
			testHelper.CreateAdditionalInformationCusCodeEntry("BND");
			testHelper.CreateAdditionalInformationCusCodeEntry("PGR");
			testHelper.CreateAdditionalInformationCusCodeEntry("PPS");
			testHelper.CreateAdditionalInformationCusCodeEntry("RCC");
			testHelper.CreateAdditionalInformationCusCodeEntry("RCV");
			testHelper.CreateAdditionalInformationCusCodeEntry("VDN");
			testHelper.CreateAdditionalInformationCusCodeEntry("UK");
			testHelper.CreateTaxOrFee(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddYears(0), ZDateTime.Today.AddYears(1), "VAT Normal");
			Factory.Save();

			CombineAssertions(() =>
			{
				var tester = new SADLineDetailWrapper(null, null);
				AssertEquals(string.Empty, tester.LicenceNumberAddInfo.Value);
				AssertEquals(string.Empty, tester.BondHolderCodeAddInfo.Value);
				AssertEquals(string.Empty, tester.ProvisionalPaymentSuretyAddInfo.Value);
				AssertEquals(null, tester.BondHolder);
				AssertEquals(0m, tester.ATVAmount);
				AssertEquals(0m, tester.VPBAmount);
				AssertEquals(0, tester.GeneralAddInfos.Count);
			});

			CombineAssertions(() =>
			{
				var testInvoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
				testInvoiceLine.JI_ZZF_NKTaxType = "VAT";
				var testEntryLine = Factory.NewWithValidTestData<CusEntryLine>();
				testEntryLine.InvoiceLines.Add(testInvoiceLine);
				testEntryLine.Fees.AddOrUpdate("VAT", 14);
				testEntryLine.AdditionalInformationCodes.AddNew("PGR", "valuets1");
				testEntryLine.AdditionalInformationCodes.AddNew("BHR", "123321");
				testEntryLine.AdditionalInformationCodes.AddNew("VDN", "valuets3");
				testEntryLine.AdditionalInformationCodes.AddNew("BND", "111222");
				testEntryLine.AdditionalInformationCodes.AddNew("RCC", "valuets5");
				testEntryLine.AdditionalInformationCodes.AddNew("PPS", "valuets6");
				testEntryLine.AdditionalInformationCodes.AddNew("RCV", "333444");
				testEntryLine.AdditionalInformationCodes.AddNew("UK", "123");
				testEntryLine.CL_VPBAmount = 125.13m;
				var testHeader = Factory.NewWithValidTestData<CusEntryHeader>();
				var line1 = testHeader.MergedLines.AddNew();
				line1.CL_LineNumber = 1;
				line1.CL_VPBAmount = 1m;
				var line2 = testHeader.MergedLines.AddNew();
				line2.CL_LineNumber = 2;
				line2.CL_VPBAmount = 3m;
				var newFactory = new BusinessObjectFactory();
				var testMessage = newFactory.NewWithValidTestData<ZAMessageForTest>();
				testMessage.EM_MessageType = "DEC";
				newFactory.Save();
				var testSourceEDIMessage = Factory.Load<CUSDECEDIMessage>(testMessage.PK);
				testSourceEDIMessage.VPBAmounts.AddNew("Line1", 105m.ToString());
				testSourceEDIMessage.VPBAmounts.AddNew("Line2", 115m.ToString());
				var tester = new SADLineDetailWrapper(testEntryLine, testHeader, testSourceEDIMessage);
				var org = tester.Factory.NewWithValidTestData<OrgHeader>();
				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.BondHolderCode;
				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
				orgCusCode.OK_CustomsRegNo = "123321";
				tester.Factory.Save();
				AssertEquals("valuets1", tester.LicenceNumberAddInfo.Value);
				AssertEquals("123321", tester.BondHolderCodeAddInfo.Value);
				AssertEquals("valuets6", tester.ProvisionalPaymentSuretyAddInfo.Value);
				AssertEquals(org.PK, tester.BondHolder?.PK);
				AssertEquals(100m, tester.ATVAmount);
				AssertEquals(125m, tester.VPBAmount);
				AssertEquals(7, tester.GeneralAddInfos.Count);
				AssertEquals("valuets3", tester.GeneralAddInfos[0].Value);
				AssertEquals("valuets5", tester.GeneralAddInfos[1].Value);
				AssertEquals("333444", tester.GeneralAddInfos[2].Value);
				AssertEquals("123321", tester.GeneralAddInfos[3].Value);
				AssertEquals("111222", tester.GeneralAddInfos[4].Value);
				AssertEquals("valuets6", tester.GeneralAddInfos[5].Value);
				AssertEquals("123", tester.GeneralAddInfos[6].Value);
				AssertEquals("UK ", tester.GeneralAddInfos[6].Code);
				AssertEquals("", tester.AdditionalCommodityCodeFormatted);
			});
		}

		public void TestGetVPBAmount_FromMessage()
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
CST+0002+820110059:108:ZZZ+100'
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
			testMessage.VPBAmounts.AddNew("Line1", 105m.ToString());
			testMessage.VPBAmounts.AddNew("Line2", 115m.ToString());
			var tester = new SADLineDetailWrapper(CUSDECMessageHelper.New(testMessage).LineLevelInformations[0], Factory.New<CusEntryHeader>(), testMessage);
			AssertEquals(115m, tester.VPBAmount);
		}

		[TestDate(2016, 01, 01)]
		public void TestAdditionaCommodityCodeFormatted()
		{
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "99999999", Universal.Constants.RateTypes.AntiDumping);
			var tariff2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "2P1", "DTY", "99999999", Universal.Constants.RateTypes.AntiDumping);
			var tariff3 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "8888888", Universal.Constants.RateTypes.AntiDumping);
			var tariff4 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12B", "DTY", "8888888", Universal.Constants.RateTypes.Excise);
			var relationship1 = universalReferenceDataHelper.CreateTariffRelationship(tariff3.PK, tariff1.ZZ1_ZZI_TariffType, "99999999");
			var relationship2 = universalReferenceDataHelper.CreateTariffRelationship(tariff4.PK, tariff2.ZZ1_ZZI_TariffType, "99999999");
			Factory.Save();
			CombineAssertions(() =>
			{
				var dec = Factory.New<JobDeclaration>();
				var invoice = dec.Invoices.AddNew();
				var testInvoiceLine = invoice.InvoiceLines.AddNew();
				testInvoiceLine.JI_Tariff = "99999999";
				var testLineTariff = testInvoiceLine.CusLineTariffDetails.AddNew();
				testLineTariff.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
				testLineTariff.BZ_Tariff = "8888888";
				var testLine = Factory.NewWithValidTestData<CusEntryLine>();
				testLine.InvoiceLines.Add(testInvoiceLine);
				var tester = new SADLineDetailWrapper(testLine, null);
				AssertEquals("888.88.88/9999.99.99", tester.AdditionalCommodityCodeFormatted);
				testLineTariff.BZ_Type = "12B";
				testLineTariff.BZ_Tariff = "8888888";
				AssertEquals("888.88.88", tester.AdditionalCommodityCodeFormatted);
				testLineTariff.BZ_Type = "12C";
				testLineTariff.BZ_Tariff = "8888888";
				AssertEquals("", tester.AdditionalCommodityCodeFormatted);
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionaCommodityCodeFormatted_NoException()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			Factory.Save();
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999999", startDate, endDate);
			var tariff2 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "8888888", startDate, endDate);
			var relationship1 = universalReferenceDataHelper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "99999999");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var testInvoiceLine = invoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_Tariff = "99999999";
			var testLineTariff = testInvoiceLine.CusLineTariffDetails.AddNew();
			testLineTariff.BZ_Type = "11A";
			testLineTariff.BZ_Tariff = "8888888";
			var testLine = Factory.NewWithValidTestData<CusEntryLine>();
			testLine.InvoiceLines.Add(testInvoiceLine);
			var tester = new SADLineDetailWrapper(testLine, null);
			AssertEquals("", tester.AdditionalCommodityCodeFormatted);
		}

		public void TestDutiesAndFees_FromEntry_MoreThan2Duties_EXP()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "", "40", "43", "", "", "", false, false, true, false);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "40";
			testInst.CEI_ProvisionalPaymentAmount = ZDecimal.Zero;
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			invLine.JI_Tariff = "00001000";
			invLine.JI_ZZF_NKTaxType = "VAT";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var cusEntryPayInfo = testHeader.EntryPayInfos.AddNew();
			cusEntryPayInfo.C9_TransactionType = "PPE";
			cusEntryPayInfo.C9_RemAdvReceived = false;
			cusEntryPayInfo.C9_PaymentAmount = 5000.00m;
			var testLine = testHeader.MergedLines.AddNew();
			testLine.CL_CustomsValue = 20m;
			testLine.Fees.AddOrUpdate("1P1", 500m);
			testLine.Fees.AddOrUpdate("12A", 400m);
			testLine.Fees.AddOrUpdate("13A", 400m);
			testLine.Fees.AddOrUpdate("12B", 300m);
			testLine.Fees.AddOrUpdate("VAT", 200m);
			var pp1 = testLine.ProvisionalPayments.AddNew("PEN", 0);
			var pp2 = testLine.ProvisionalPayments.AddNew("PEN", 11.11);
			var pp3 = testLine.ProvisionalPayments.AddNew("XXT", 22.11);
			var pp4 = testLine.ProvisionalPayments.AddNew("", 33.11);
			var pp5 = testLine.ProvisionalPayments.AddNew("PPA", 44.11);
			var pp6 = testLine.ProvisionalPayments.AddNew("PPA", 55.11);
			var pp7 = testLine.ProvisionalPayments.AddNew("FOR", 66.11);
			testLine.InvoiceLines.Add(invLine);
			CombineAssertions("EXP", () =>
			{
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				var tester = new SADLineDetailWrapper(testLine, testHeader);
				AssertEquals(5, tester.CalcDutiesAndFees.Count);
				AssertEquals("1P1", tester.CalcDutiesAndFees[0].Code);
				AssertEquals("12A", tester.CalcDutiesAndFees[1].Code);
				AssertEquals("13A", tester.CalcDutiesAndFees[2].Code);
				AssertEquals("VAT", tester.CalcDutiesAndFees[3].Code);
				AssertEquals("12B", tester.CalcDutiesAndFees[4].Code);
				AssertEquals(500m, tester.CalcDutiesAndFees[0].Value);
				AssertEquals(400m, tester.CalcDutiesAndFees[1].Value);
				AssertEquals(400m, tester.CalcDutiesAndFees[2].Value);
				AssertEquals(200m, tester.CalcDutiesAndFees[3].Value);
				AssertEquals(300m, tester.CalcDutiesAndFees[4].Value);
			});
		}

		public void TestDutiesAndFees_FromEntry_MoreThan2Duties_IMP()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "", "40", "43", "", "", "", false, false, true, false);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "40";
			testInst.CEI_ProvisionalPaymentAmount = ZDecimal.Zero;
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			invLine.JI_Tariff = "00001000";
			invLine.JI_ZZF_NKTaxType = "VAT";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var cusEntryPayInfo = testHeader.EntryPayInfos.AddNew();
			cusEntryPayInfo.C9_TransactionType = "PPE";
			cusEntryPayInfo.C9_RemAdvReceived = false;
			cusEntryPayInfo.C9_PaymentAmount = 5000.00m;
			var testLine = testHeader.MergedLines.AddNew();
			testLine.CL_CustomsValue = 20m;
			testLine.Fees.AddOrUpdate("1P1", 500m);
			testLine.Fees.AddOrUpdate("12A", 400m);
			testLine.Fees.AddOrUpdate("13A", 400m);
			testLine.Fees.AddOrUpdate("12B", 300m);
			testLine.Fees.AddOrUpdate("VAT", 200m);
			var pp1 = testLine.ProvisionalPayments.AddNew("PEN", 0);
			var pp2 = testLine.ProvisionalPayments.AddNew("PEN", 11.11);
			var pp3 = testLine.ProvisionalPayments.AddNew("XXT", 22.11);
			var pp4 = testLine.ProvisionalPayments.AddNew("", 33.11);
			var pp5 = testLine.ProvisionalPayments.AddNew("PPA", 44.11);
			var pp6 = testLine.ProvisionalPayments.AddNew("PPA", 55.11);
			var pp7 = testLine.ProvisionalPayments.AddNew("FOR", 66.11);
			testLine.InvoiceLines.Add(invLine);
			CombineAssertions("IMP", () =>
			{
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var tester = new SADLineDetailWrapper(testLine, testHeader);
				AssertEquals(4, tester.CalcDutiesAndFees.Count);
				AssertEquals("DTY", tester.CalcDutiesAndFees[0].Code);
				AssertEquals("VAT", tester.CalcDutiesAndFees[1].Code);
				AssertEquals("12B", tester.CalcDutiesAndFees[2].Code);
				AssertEquals("PP's", tester.CalcDutiesAndFees[3].Code);
				AssertEquals(1300m, tester.CalcDutiesAndFees[0].Value);
				AssertEquals(200m, tester.CalcDutiesAndFees[1].Value);
				AssertEquals(300m, tester.CalcDutiesAndFees[2].Value);
				AssertEquals(176.44m, tester.CalcDutiesAndFees[3].Value);
			});
		}

		public void TestDutiesAndFees_FromEntry()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "", "40", "43", "", "", "", false, false, true, false);
			Factory.Save();
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
				var tester = new SADLineDetailWrapper(testLine, testHeader);
				AssertEquals(4, tester.CalcDutiesAndFees.Count);
				AssertEquals("1P1", tester.CalcDutiesAndFees[0].Code);
				AssertEquals("12A", tester.CalcDutiesAndFees[1].Code);
				AssertEquals("VAT", tester.CalcDutiesAndFees[2].Code);
				AssertEquals("12B", tester.CalcDutiesAndFees[3].Code);
				AssertEquals(500m, tester.CalcDutiesAndFees[0].Value);
				AssertEquals(400m, tester.CalcDutiesAndFees[1].Value);
				AssertEquals(200m, tester.CalcDutiesAndFees[2].Value);
				AssertEquals(300m, tester.CalcDutiesAndFees[3].Value);
				testHeader.CH_CEI_Instruction = testInst.PK;
				tester = new SADLineDetailWrapper(testLine, testHeader);
				AssertEquals(1, tester.CalcDutiesAndFees.Count);
				AssertEquals("12B", tester.CalcDutiesAndFees[0].Code);
				AssertEquals(300m, tester.CalcDutiesAndFees[0].Value);
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
				var tester = new SADLineDetailWrapper(testLine, testHeader);
				AssertEquals(5, tester.CalcDutiesAndFees.Count);
				AssertEquals("1P1", tester.CalcDutiesAndFees[0].Code);
				AssertEquals("12A", tester.CalcDutiesAndFees[1].Code);
				AssertEquals("VAT", tester.CalcDutiesAndFees[2].Code);
				AssertEquals("12B", tester.CalcDutiesAndFees[3].Code);
				AssertEquals("PP's", tester.CalcDutiesAndFees[4].Code);
				AssertEquals(500m, tester.CalcDutiesAndFees[0].Value);
				AssertEquals(400m, tester.CalcDutiesAndFees[1].Value);
				AssertEquals(200m, tester.CalcDutiesAndFees[2].Value);
				AssertEquals(300m, tester.CalcDutiesAndFees[3].Value);
				AssertEquals(120m, tester.CalcDutiesAndFees[4].Value);
				testHeader.CH_CEI_Instruction = testInst.PK;
				tester = new SADLineDetailWrapper(testLine, testHeader);
				AssertEquals(2, tester.CalcDutiesAndFees.Count);
				AssertEquals("12B", tester.CalcDutiesAndFees[0].Code);
				AssertEquals("PP's", tester.CalcDutiesAndFees[1].Code);
				AssertEquals(300m, tester.CalcDutiesAndFees[0].Value);
				AssertEquals(120m, tester.CalcDutiesAndFees[1].Value);
			});
		}

		public void TestDutiesAndFees_FromMessage()
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
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "", "40", "43", "", "", "", false, false, true, false);
			Factory.Save();
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "40";
				var invHeader = declaration.Invoices.AddNew();
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_CEI = testInst.PK;
				invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
				invLine.JI_Tariff = "00001000";
				invLine.JI_ZZF_NKTaxType = "VAT";
				var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				var tester = new SADLineDetailWrapper(CUSDECMessageHelper.New(testMessage).LineLevelInformations[0], cusEntryHeader);
				AssertEquals(5, tester.CalcDutiesAndFees.Count);
				AssertEquals("1P1", tester.CalcDutiesAndFees[0].Code);
				AssertEquals("12A", tester.CalcDutiesAndFees[1].Code);
				AssertEquals("VAT", tester.CalcDutiesAndFees[2].Code);
				AssertEquals("12B", tester.CalcDutiesAndFees[3].Code);
				AssertEquals("PP's", tester.CalcDutiesAndFees[4].Code);
				AssertEquals(500m, tester.CalcDutiesAndFees[0].Value);
				AssertEquals(400m, tester.CalcDutiesAndFees[1].Value);
				AssertEquals(200m, tester.CalcDutiesAndFees[2].Value);
				AssertEquals(300m, tester.CalcDutiesAndFees[3].Value);
				AssertEquals(120m, tester.CalcDutiesAndFees[4].Value);
				cusEntryHeader.CH_CEI_Instruction = testInst.PK;
				tester = new SADLineDetailWrapper(CUSDECMessageHelper.New(testMessage).LineLevelInformations[0], cusEntryHeader);
				AssertEquals(2, tester.CalcDutiesAndFees.Count);
				AssertEquals("12B", tester.CalcDutiesAndFees[0].Code);
				AssertEquals("PP's", tester.CalcDutiesAndFees[1].Code);
				AssertEquals(300m, tester.CalcDutiesAndFees[0].Value);
				AssertEquals(120m, tester.CalcDutiesAndFees[1].Value);
			});
		}

		public void TestAdditionalCommodityCodeFormattedFallbackToBO()
		{
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "10000000", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff1A = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "1000001", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff1B = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12B", "DTY", "1000002", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff2 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "20000000", Universal.Constants.RateTypes.AdValoremExcise);
			var tariff2A = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12A", "DTY", "2000001", Universal.Constants.RateTypes.AdValoremExcise);
			universalReferenceDataHelper.CreateTariffRelationship(tariff1A.PK, tariff1.ZZ1_ZZI_TariffType, "10000000");
			universalReferenceDataHelper.CreateTariffRelationship(tariff1B.PK, tariff1.ZZ1_ZZI_TariffType, "10000000");
			universalReferenceDataHelper.CreateTariffRelationship(tariff2A.PK, tariff2.ZZ1_ZZI_TariffType, "20000000");
			Factory.Save();
			CUSDECMessageDataProviderForTest messageDataProvider = new CUSDECMessageDataProviderForTest();
			messageDataProvider.LineLevelDetails = new LineLevelInformationForTest[]
			{
				new LineLevelInformationForTest {
					LineNumber = "0001",
					TariffCode = "10000000",
					AdditionalInformations = System.Array.Empty<AdditionalInformationForTest>(),
					DutiesAndFees = System.Array.Empty<DutyFeeInformationForTest>(),
					ProvisionalPayments = System.Array.Empty<DutyFeeInformationForTest>()
				},
				new LineLevelInformationForTest {
					LineNumber = "0002",
					TariffCode = "20000000",
					AdditionalInformations = System.Array.Empty<AdditionalInformationForTest>(),
					DutiesAndFees = System.Array.Empty<DutyFeeInformationForTest>(),
					ProvisionalPayments = System.Array.Empty<DutyFeeInformationForTest>()
				},
				new LineLevelInformationForTest {
					LineNumber = "0003",
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
			invoiceLine1.CusLineTariffDetails.AddNew("12A", "1000001");
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "20000000";
			invoiceLine2.CusLineTariffDetails.AddNew("12A", "2000001");
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "10000000";
			invoiceLine3.CusLineTariffDetails.AddNew("12B", "1000002");
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
			ICUSDECMessageDataProvider parsedMessageDataProvider = CUSDECMessageHelper.New(message);
			List<SADLineDetailWrapper> lineWrappers = parsedMessageDataProvider.LineLevelDetails.Select(l => new SADLineDetailWrapper(l, entryHeader, message)).ToList();
			AssertEquals(3, lineWrappers.Count);
			AssertEquals("100.00.01/1000.00.00", lineWrappers[0].AdditionalCommodityCodeFormatted);
			AssertEquals("200.00.01/2000.00.00", lineWrappers[1].AdditionalCommodityCodeFormatted);
			// value is empty because tariff in 3rd line does not match
			AssertEquals("", lineWrappers[2].AdditionalCommodityCodeFormatted);
		}

		protected override BusinessObject GetNewBusinessObject() => new SADLineDetailWrapper(null, null);

		protected override void SetUp()
		{
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}
}
