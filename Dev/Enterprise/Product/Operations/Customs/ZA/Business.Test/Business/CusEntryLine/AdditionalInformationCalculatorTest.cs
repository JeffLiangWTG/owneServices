using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AdditionalInformationCalculatorTest : TestCaseWithFactory
	{
		[TestDate(1990, 6, 1)]
		public void TestRCCCertificates()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("NUI");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("RCC");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("RCV");
			Factory.Save();
			var tariffType1P1 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType4P1 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			var rateType_ZA_DTY = UniversalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty);
			var rateCode_ZA_DTY_D = UniversalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			Factory.Save();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", startDate, endDate);
			var tariff4P1 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4100101010", startDate, endDate);
			UniversalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.PRCC, "true", tariff4P1);
			var relationship01 = UniversalReferenceDataHelper.CreateTariffRelationship(tariff4P1.PK, tariff.CusTariffType.PK, "1010102030");
			var tariff1P1Rate = UniversalReferenceDataHelper.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.1 * VFD");
			var helper = new PermitTestDataHelper(Factory);
			var permit1 = helper.CreatePermitHeader(importer.PK, "PERMIT1", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.ACO, 0m, 1000m);
			var permit2 = helper.CreatePermitHeader(importer.PK, "PERMIT2", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.ACO, 0m, 1000m);
			var permit3 = helper.CreatePermitHeader(importer.PK, "PERMIT3", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.ATO, 0m, 1000m);
			var permit4 = helper.CreatePermitHeader(importer.PK, "PERMIT4", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.LEG, 0m, 1000m);
			var permit5 = helper.CreatePermitHeader(importer.PK, "PERMIT5", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.MHV, 0m, 2000m);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_OH_Importer = importer.PK;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var rcc1 = instruction1.RCCCertificates.AddNew();
			rcc1.CY_Code = "PERMIT1";
			rcc1.CY_Order = 1;
			var rcc2 = instruction1.RCCCertificates.AddNew();
			rcc2.CY_Code = "PERMIT2";
			rcc2.CY_Order = 2;
			var rcc3 = instruction1.RCCCertificates.AddNew();
			rcc3.CY_Code = "PERMIT3";
			rcc3.CY_Order = 3;
			var rcc4 = instruction1.RCCCertificates.AddNew();
			rcc4.CY_Code = "PERMIT4";
			rcc4.CY_Order = 4;
			var rcc5 = instruction1.RCCCertificates.AddNew();
			rcc5.CY_Code = "PERMIT5";
			rcc5.CY_Order = 5;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 4234.52m;
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine1.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine1.JI_PrimaryPreference = "STANDARD";
			invoiceLine1.CusLineTariffDetails.AddNew(tariff4P1.ZZ1_ZZI_TariffTypeCode, tariff4P1.ZZ1_TariffCode);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
			var entryLine1 = invoiceLine1.CusEntryLine;
			AssertXMLEquals(@"NUI=N
RCC=PERMIT1
RCC=PERMIT2
RCC=PERMIT3
RCC=PERMIT4
RCC=PERMIT5
RCV=1000
RCV=1000
RCV=1000
RCV=1000
RCV=235
", new ZStringBuilder(entryLine1.AdditionalInformationCodes.Cast<AdditionalInformation>().Select(x => x.CY_Code + "=" + x.CY_Data)).ToStringWithNewLineBetweenAppends().TrimEnd());
			UniversalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.SpecifiedMotorVehicle, "true", tariff);
			invoiceLine1.UniversalTariff?.Attributes.RefreshFromDb();
			AssertEquals(true, invoiceLine1.UniversalTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributes.SpecifiedMotorVehicle) ?? false);
			declaration.DoMerge();
			entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
			entryLine1 = invoiceLine1.CusEntryLine;
			AssertXMLEquals(@"NUI=N
RCC=PERMIT1
RCC=PERMIT2
RCC=PERMIT3
RCC=PERMIT4
RCC=PERMIT5
RCV=1000
RCV=1000
RCV=1000
RCV=1000
RCV=1294
", new ZStringBuilder(entryLine1.AdditionalInformationCodes.Cast<AdditionalInformation>().Select(x => x.CY_Code + "=" + x.CY_Data)).ToStringWithNewLineBetweenAppends().TrimEnd());
		}

		[TestDate(1990, 6, 1)]
		public void TestCalculate()
		{
			var universalReferenceTestDataHelper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var tariffType1P1 = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType2P1 = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1");
			var tariffType2P2 = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P2");
			var tariffType2P3 = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P3");
			var rateType_ZA_DTY = universalReferenceTestDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = universalReferenceTestDataHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("ADI");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("APE");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("BND");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("CVI");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("NUI");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("ROO");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("SGI");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("VDN");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("VIN");
			universalReferenceTestDataHelper.CreateAdditionalInformationCusCodeEntry("VTE");
			UniversalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.ProcedureCategoryCodes._A, UniversalReferenceConstants.ProcedureCodes._11, UniversalReferenceConstants.ProcedureCodes._41, "", "", "IMP");
			UniversalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.ProcedureCategoryCodes._E, UniversalReferenceConstants.ProcedureCodes._52, "", "", "", "EXP");
			var preference = UniversalReferenceDataHelper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", startDate, endDate);
			var tariff2P1 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P1.PK, "2100101010", startDate, endDate);
			universalReferenceTestDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "11", tariff2P1);
			var tariff2P2 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P2.PK, "2200101010", startDate, endDate);
			universalReferenceTestDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "22", tariff2P2);
			var tariff2P3 = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P3.PK, "2300101010", startDate, endDate);
			universalReferenceTestDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "11", tariff2P3);
			universalReferenceTestDataHelper.CreateTariffRelationship(tariff2P1.PK, tariff.ZZ1_ZZI_TariffType, "1010102030");
			universalReferenceTestDataHelper.CreateTariffRelationship(tariff2P2.PK, tariff.ZZ1_ZZI_TariffType, "1010102030");
			universalReferenceTestDataHelper.CreateTariffRelationship(tariff2P3.PK, tariff.ZZ1_ZZI_TariffType, "1010102030");
			var tariff1P1Rate = universalReferenceTestDataHelper.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.1 * VFD", preference.PK);
			Factory.Save();
			var testTradeGroup1 = universalReferenceTestDataHelper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			universalReferenceTestDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability1 = universalReferenceTestDataHelper.CreateCusApplicability(tariff1P1Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var supplier = GetNewOrgWithOK_CodeType(OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ROOCert = "ROO3242";
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_VDN = "VDN3234";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine1.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine1.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine1.CusLineTariffDetails.AddNew(tariff2P1.ZZ1_ZZI_TariffTypeCode, tariff2P1.ZZ1_TariffCode);
			invoiceLine1.CusLineTariffDetails.AddNew(tariff2P2.ZZ1_ZZI_TariffTypeCode, tariff2P2.ZZ1_TariffCode);
			invoiceLine1.CusLineTariffDetails.AddNew(tariff2P3.ZZ1_ZZI_TariffTypeCode, tariff2P3.ZZ1_TariffCode);
			invoiceLine1.JI_VIN = "VIN323423";
			invoiceLine1.JI_NewUsed = GoodsTypeList.Codes.S;
			invoiceLine1.JI_ElectionsExemptionsLevy = "EEL364m";
			invoiceLine1.JI_KimberleyCertificate = "KC532";
			invoiceLine1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._11 + UniversalReferenceConstants.ProcedureCodes._41;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VEX;
			invoiceLine2.JI_PrimaryPreference = "ROO";
			invoiceLine2.JI_ROOCert = "ROO3242";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			CombineAssertions(() =>
			{
				var entryLine1 = invoiceLine1.CusEntryLine;
				AssertXMLEquals(@"ADI=210010101011
CVI=220010101022
NUI=S
SGI=230010101011
VDN=VDN3234
VIN=VIN323423", new ZStringBuilder(entryLine1.AdditionalInformationCodes.Cast<AdditionalInformation>().Select(x => x.CY_Code + "=" + x.CY_Data)).ToStringWithNewLineBetweenAppends().TrimEnd());
				var entryLine2 = invoiceLine2.CusEntryLine;
				AssertXMLEquals(@"NUI=N
ROO=ROO3242
VDN=VDN3234
VTE=", new ZStringBuilder(entryLine2.AdditionalInformationCodes.Cast<AdditionalInformation>().Select(x => x.CY_Code + "=" + x.CY_Data)).ToStringWithNewLineBetweenAppends().TrimEnd());
			});
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine1.JI_PrimaryPreference = OrgCusCode.SouthAfricaCodeTypes.CustomsApprovedExporter;
			invoiceLine1.JI_ROOCert = "12345";
			declaration.DoMerge();
			entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			CombineAssertions(() =>
			{
				var entryLine1 = invoiceLine1.CusEntryLine;
				AssertXMLEquals(@"APE=12345
NUI=S
VDN=VDN3234
VIN=VIN323423", new ZStringBuilder(entryLine1.AdditionalInformationCodes.Cast<AdditionalInformation>().Select(x => x.CY_Code + "=" + x.CY_Data)).ToStringWithNewLineBetweenAppends().TrimEnd());
				var entryLine2 = invoiceLine2.CusEntryLine;
				AssertXMLEquals(@"NUI=N
VDN=VDN3234", new ZStringBuilder(entryLine2.AdditionalInformationCodes.Cast<AdditionalInformation>().Select(x => x.CY_Code + "=" + x.CY_Data)).ToStringWithNewLineBetweenAppends().TrimEnd());
			});
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			instruction1.CEI_Style = UniversalReferenceConstants.ProcedureCodes._52;
			instruction1.CEI_OH_Carrier = supplier.PK;
			invoice1.JZ_InvoiceAmount = 1000m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine1.JI_ROOCert = "";
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._52;
			declaration.DoMerge();
			entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			CombineAssertions(() =>
			{
				var entryLine1 = invoiceLine1.CusEntryLine;
				AssertXMLEquals(@"BND=100
NUI=S
VDN=VDN3234
VIN=VIN323423", new ZStringBuilder(entryLine1.AdditionalInformationCodes.Cast<AdditionalInformation>().Select(x => x.CY_Code + "=" + x.CY_Data)).ToStringWithNewLineBetweenAppends().TrimEnd());
			});
		}

		public void TestBHR()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("BHR");
			UniversalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.ProcedureCategoryCodes._B, UniversalReferenceConstants.ProcedureCodes._20, UniversalReferenceConstants.ProcedureCodes._41, "", "", "IMP");
			Factory.Save();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.CustomsCodes.AddNew("BHR", "TestBHR", "ZA");
			CombineAssertions("SEA", () =>
			{
				var declaration = GetNewJobDeclaration();
				declaration.JE_TransportMode = "SEA";
				var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
				var header = declaration.Invoices.AddNew();
				var line = header.JobComInvoiceLines.AddNew();
				line.JI_CEI = inst.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.InvoiceLines.Add(line);
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "BND", "BHR" });
			});
			CombineAssertions("ROA", () =>
			{
				var declaration = GetNewJobDeclaration();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
				declaration.JE_RL_NKFinalDestination = "ZAJNB";
				var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._20;
				inst.CEI_OH_BondHolder = testOrg.PK;
				inst.CEI_OH_Carrier = testOrg.PK;
				var header = declaration.Invoices.AddNew();
				var line = header.JobComInvoiceLines.AddNew();
				line.JI_CEI = inst.PK;
				line.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._20 + UniversalReferenceConstants.ProcedureCodes._41;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				entryLine.InvoiceLines.Add(line);
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "BHR", "TestBHR");
			});
			CombineAssertions("BHR Line 1 only", () =>
			{
				var declaration = GetNewJobDeclaration();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
				declaration.JE_RL_NKFinalDestination = "ZAJNB";
				var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._20;
				inst.CEI_OH_BondHolder = testOrg.PK;
				inst.CEI_OH_Carrier = testOrg.PK;
				var header = declaration.Invoices.AddNew();
				var line = header.JobComInvoiceLines.AddNew();
				var line2 = header.JobComInvoiceLines.AddNew();
				line.JI_CEI = inst.PK;
				line.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._20 + UniversalReferenceConstants.ProcedureCodes._41;
				line2.JI_CEI = inst.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				entryLine1.InvoiceLines.Add(line);
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				entryLine2.InvoiceLines.Add(line2);
				AdditionalInformationCalculator.Calculate(entryLine1);
				AdditionalInformationCalculator.Calculate(entryLine2);
				AssertHasCustomsCode(entryLine1.AdditionalInformationCodes, "BHR", "TestBHR");
				AssertHasNoCustomsCode(entryLine2.AdditionalInformationCodes, new ZString[] { "BHR" });
			});
		}

		public void TestBND()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("BND");
			UniversalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.ProcedureCategoryCodes._E, UniversalReferenceConstants.ProcedureCodes._40, UniversalReferenceConstants.ProcedureCodes._41, "", "", "IMP");
			Factory.Save();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.CustomsCodes.AddNew("BND", "TestBND", "ZA");
			CombineAssertions("BND", () =>
			{
				var declaration = GetNewJobDeclaration();
				declaration.JE_RemovalTransportCode = Enterprise.Core.Constants.TransportModes.Road;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_RL_NKFinalDestination = "ZAJNB";
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._40;
				instruction.CEI_OH_Carrier = testOrg.PK;
				var header = declaration.Invoices.AddNew();
				var line = header.JobComInvoiceLines.AddNew();
				line.JI_ZZF_NKTaxType = "VAT";
				var line2 = header.JobComInvoiceLines.AddNew();
				line2.JI_ZZF_NKTaxType = "VAT";
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				line.JI_CL = entryLine.PK;
				line.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._40 + UniversalReferenceConstants.ProcedureCodes._41;
				entryLine.AdditionalInformationCodes.AddNew("BND", "0");
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "BND", "0");
				var entryLine2 = entryHeader.MergedLines.AddNew();
				line2.JI_CL = entryLine2.PK;
				line2.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._40 + UniversalReferenceConstants.ProcedureCodes._41;
				entryLine2.AdditionalInformationCodes.AddNew("BND", "6006");
				AdditionalInformationCalculator.Calculate(entryLine2);
				AssertHasCustomsCode(entryLine2.AdditionalInformationCodes, "BND", "6006");
			});
		}

		public void TestProvisionalPaymentSuretyAmount()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety);
			UniversalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.ProcedureCategoryCodes._E, UniversalReferenceConstants.ProcedureCodes._40, UniversalReferenceConstants.ProcedureCodes._41, "", "", "IMP");
			Factory.Save();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew("BHR", "TestBHR", "ZA");
			var declaration = GetNewJobDeclaration();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_RL_NKFinalDestination = "ZAJNB";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._40;
			instruction.CEI_OH_Carrier = carrier.PK;
			var header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			var line2 = header.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			line1.JI_CL = entryLine1.PK;
			line1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._40 + UniversalReferenceConstants.ProcedureCodes._41;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			line2.JI_CL = entryLine2.PK;
			line2.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._40 + UniversalReferenceConstants.ProcedureCodes._41;

			CombineAssertions(() =>
			{
				instruction.CEI_ProvisionalPaymentSuretyAmount = 0m;
				AdditionalInformationCalculator.Calculate(entryLine1);
				Assert(!entryLine1.AdditionalInformationCodes.ContainsCode(UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety));

				instruction.CEI_ProvisionalPaymentSuretyAmount = 123456m;
				AdditionalInformationCalculator.Calculate(entryLine1);
				AssertHasCustomsCode(entryLine1.AdditionalInformationCodes, UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety, "123456");

				AdditionalInformationCalculator.Calculate(entryLine2);
				Assert(!entryLine2.AdditionalInformationCodes.ContainsCode(UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety));
			});
		}

		public void TestROOTypeAndNumber()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("EUR");
			Factory.Save();
			var declaration = GetNewJobDeclaration();
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.SouthAfrica;
			declaration.JE_ROOType = "EUR";
			var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = "36";
			declaration.JE_ROOCert = "12345678901234567890123456789012345";
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "EUR", "12345678901234567890123456789012");
		}

		public void TestROOForIntoWarehouseEntry()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("ROO");
			Factory.Save();
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure1.ZZ6_ProcedureCode = ProcedureCodes._10;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure2.ZZ6_ProcedureCode = ProcedureCodes._41;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ROOType = "EUR";
			var inst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst1.CEI_Style = ProcedureCodes._10;
			var inst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst2.CEI_Style = ProcedureCodes._41;
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_ROOCert = "1234";
			line.JI_CEI = inst1.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "ROO", "1234");
			entryLine.AdditionalInformationCodes.RemoveAndDeleteAll();
			line.JI_CEI = inst2.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			Assert(!entryLine.AdditionalInformationCodes.ContainsCode("ROO"));
		}

		public void TestVIN()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("VIN");
			Factory.Save();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.CustomsCodes.AddNew("VIN", "TestVIN", "ZA");
			CombineAssertions("Has VIN", () =>
			{
				var declaration = GetNewJobDeclaration();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inst.CEI_Style = "11";
				var header = declaration.Invoices.AddNew();
				var line = header.JobComInvoiceLines.AddNew();
				line.JI_VIN = "TestVIN";
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				line.JI_CL = entryLine.PK;
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "VIN", "TestVIN");
			});
			CombineAssertions("No VIN", () =>
			{
				var declaration = GetNewJobDeclaration();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var header = declaration.Invoices.AddNew();
				var line = header.JobComInvoiceLines.AddNew();
				line.JI_VIN = ZString.Empty;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				line.JI_CL = entryLine.PK;
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "VIN" });
			});
		}

		public void TestOLI()
		{
			var entryLine = CreateNewCusEntryLine(ZAJobMessageTypeList.Codes.Import, "10", "00", "870310");
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCodeWithValue(entryLine.AdditionalInformationCodes, "OLI", "19620");
			entryLine = CreateNewCusEntryLine(ZAJobMessageTypeList.Codes.Import, "10", "20", "010121");
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCodeWithValue(entryLine.AdditionalInformationCodes, "OLI", "19610");
			entryLine = CreateNewCusEntryLine(ZAJobMessageTypeList.Codes.Export, "10", "20", "010121");
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "OLI" });
		}

		public void TestIPC_EPC()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("EPC");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("IPC");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ProcedureCodes._10;
			var header = dec.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { UniversalReferenceConstants.AdditionalInformation.ImportPermitControl });
			line.JI_PermitNumber = "ABC001";
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, UniversalReferenceConstants.AdditionalInformation.ImportPermitControl, "ABC001");
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { UniversalReferenceConstants.AdditionalInformation.ExportPermitControl });
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			line.JI_PermitNumber = ZString.Empty;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { UniversalReferenceConstants.AdditionalInformation.ImportPermitControl });
			line.JI_PermitNumber = "GHI001";
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, UniversalReferenceConstants.AdditionalInformation.ImportPermitControl, "GHI001");
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { UniversalReferenceConstants.AdditionalInformation.ExportPermitControl });
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			line.JI_PermitNumber = ZString.Empty;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { UniversalReferenceConstants.AdditionalInformation.ExportPermitControl });
			line.JI_PermitNumber = "DEF001";
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, UniversalReferenceConstants.AdditionalInformation.ExportPermitControl, "DEF001");
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { UniversalReferenceConstants.AdditionalInformation.ImportPermitControl });
		}

		public void TestVDN()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("VDN");
			Factory.Save();
			var line = CreateCusEntryLine("IMP", "");
			AdditionalInformationCalculator.Calculate(line);
			AssertHasNoCustomsCode(line.AdditionalInformationCodes, new ZString[] { "VDN" });
			line = CreateCusEntryLine("IMP", "VDN01");
			AdditionalInformationCalculator.Calculate(line);
			AssertHasCustomsCode(line.AdditionalInformationCodes, "VDN", "VDN01");
			line = CreateCusEntryLine("EXP", "VDN01");
			AdditionalInformationCalculator.Calculate(line);
			AssertHasCustomsCode(line.AdditionalInformationCodes, "VDN", "VDN01");
		}

		public void TestVTE()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("VTE");
			Factory.Save();
			var dec = GetNewJobDeclaration();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "10";
			var header = dec.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VEX;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "VTE", "");
			line.JI_CL = ZGuid.Empty;
			entryHeader.MergedLines.RemoveAll();
			entryLine = entryHeader.MergedLines.AddNew();
			line.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "VTE" });
		}

		public void TestAPN()
		{
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("APN");
			Factory.Save();
			var dec = GetNewJobDeclaration();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "10";
			var header = dec.Invoices.AddNew();
			header.JZ_PaymentNo = "001";
			var line = header.JobComInvoiceLines.AddNew();
			AssertEquals("Pre assertion - JI_AdvancePaymentNo should be 001", "001", line.JI_AdvancePaymentNo);
			line.JI_CEI = instruction.PK;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "APN", "001");
			line.JI_CL = ZGuid.Empty;
			entryHeader.MergedLines.RemoveAll();
			entryLine = entryHeader.MergedLines.AddNew();
			line.JI_AdvancePaymentNo = ZString.Empty;
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "APN" });
		}

		[TestDate(2016, 09, 16)]
		public void TestADIWithCheckDigit()
		{
			var tariffType1P1 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var tariffType2P1 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1");
			Factory.Save();
			var tariff1P1 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariff1 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P1.PK, "201020407", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariffAtt = UniversalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "75", tariff1);
			UniversalReferenceDataHelper.CreateTariffRelationship(tariff1.PK, tariff1P1.ZZ1_ZZI_TariffType, "1010102030");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("ADI");
			Factory.Save();
			var dec = GetNewJobDeclaration();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "10";
			var header = dec.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_CEI = instruction.PK;
			line.JI_Tariff = "1010102030";
			line.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VEX;
			var lineTariffDetail = line.CusLineTariffDetails.AddNew();
			lineTariffDetail.BZ_Type = "2P1";
			lineTariffDetail.BZ_Tariff = "201020407";
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			AdditionalInformationCalculator.Calculate(entryLine);
			AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "ADI", "20102040775");
		}

		public void TestADISelectCorrectTariffCheckDigit()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", "IMP");
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", "IMP");
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			var adiCodeList = testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "ADDIN", "ADI", startDate, endDate);
			testHelper.CreateCusCodeListAttribute(adiCodeList.PK, "Schedule", "2P1");
			var dtyRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "DTY");
			var addRateType = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "ADD");
			var rateCode_ZA_ADD_D = testHelper.LoadOrCreateNewCusRateCode(Factory, "D", addRateType.PK);
			var dtyTariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DTY");
			var addTariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1", "ADD");
			Factory.Save();
			testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, dtyTariffType.PK, "99999111", startDate, endDate, "DESC 1P1 for CN", 0, "");
			var cnAddTariff = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, addTariffType.PK, "99999211", startDate.AddDays(-1), endDate, "DESC 2P1 CN", 0, "", "99999111");
			testHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "01", cnAddTariff);
			testHelper.CreateRate(cnAddTariff, rateCode_ZA_ADD_D.PK, startDate, endDate, "1");
			testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, dtyTariffType.PK, "99999112", startDate, endDate, "DESC 1P1 for IN", 0, "");
			var inAddTariff = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, addTariffType.PK, "99999211", startDate, endDate, "DESC 2P1 IN", 1, "", "99999112");
			testHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "02", inAddTariff);
			testHelper.CreateRate(inAddTariff, rateCode_ZA_ADD_D.PK, startDate, endDate, "1");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInst = declaration.CustomsEntryInstructions.AddNew();
			entryInst.CEI_Style = "XX";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInst.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "YY";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			invoiceLine.JI_Tariff = "99999111";
			CombineAssertions(() =>
			{
				AssertEquals("Pre: Defaulting 2P1", 0, invoiceLine.CusLineTariffDetails.Count);
				var lineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
				lineTariffDetail.BZ_Tariff = "99999211";
				AssertEquals(cnAddTariff.PK, lineTariffDetail.UniversalTariff.PK);
				var lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				AssertEquals(1, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
				var entryLine = declaration.CustomsEntryHeaders[0].AllEntryLines[0];
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "ADI", "9999921101");
			});
		}

		public void TestIsDiamondProcessingRequired()
		{
			var tariffType1P1 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			//1P1 - Diamond Processing Required
			var tariff1 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991001", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariffAttribute = UniversalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.Diamond, "X", tariff1);
			//1P1 - Diamond Processing Not Required
			var tariff2 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991002", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("DBL");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("DDL");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("DDX");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("DLV");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("DPR");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("DPX");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("ELX");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("KBC");
			UniversalReferenceDataHelper.CreateAdditionalInformationCusCodeEntry("TBP");
			Factory.Save();
			CombineAssertions("Imports", () =>
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var invoice = dec.Invoices.AddNew();
				var invLine = invoice.JobComInvoiceLines.AddNew();
				var instruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "10";
				invLine.JI_CEI = instruction.PK;
				invLine.JI_DiamondBeneficiaryLicense = "1";
				invLine.JI_DiamondDealerLicense = "2";
				invLine.JI_TemporaryExportExemption = "3";
				invLine.JI_DiamondLevyValue = 4;
				invLine.JI_DiamondProducerRegistration = "5";
				invLine.JI_DiamondProducerExemption = "6";
				invLine.JI_ElectionsExemptionsLevy = "7";
				invLine.JI_KimberleyCertificate = "8";
				invLine.JI_TemporaryBuyersPermit = "9";
				invLine.JI_Tariff = "991001";
				var entryHeader = dec.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invLine.JI_CL = entryLine.PK;
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DBL", "1");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DDL", "2");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DPR", "5");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "ELX", "7");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "KBC", "8");
				AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "DLV", "DPX", "TBP", "DDX" });
				invLine.JI_CL = ZGuid.Empty;
				invLine.JI_Tariff = "991002";
				entryHeader.MergedLines.RemoveAll();
				entryLine = entryHeader.MergedLines.AddNew();
				invLine.JI_CL = entryLine.PK;
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "DBL", "DDL", "DLV", "DPR", "DPX", "ELX", "KBC", "TBP", "DDX" });
			});
			CombineAssertions("Exports", () =>
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				var invoice = dec.Invoices.AddNew();
				var invLine = invoice.JobComInvoiceLines.AddNew();
				var instruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "36";
				invLine.JI_CEI = instruction.PK;
				invLine.JI_DiamondBeneficiaryLicense = "1";
				invLine.JI_DiamondDealerLicense = "2";
				invLine.JI_TemporaryExportExemption = "3";
				invLine.JI_DiamondLevyValue = 4;
				invLine.JI_DiamondProducerRegistration = "5";
				invLine.JI_DiamondProducerExemption = "6";
				invLine.JI_ElectionsExemptionsLevy = "7";
				invLine.JI_KimberleyCertificate = "8";
				invLine.JI_TemporaryBuyersPermit = "9";
				invLine.JI_Tariff = "991001";
				var entryHeader = dec.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invLine.JI_CL = entryLine.PK;
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DBL", "1");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DDL", "2");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DLV", "4");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DPR", "5");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DPX", "6");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "ELX", "7");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "KBC", "8");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "TBP", "9");
				AssertHasCustomsCode(entryLine.AdditionalInformationCodes, "DDX", "3");
				invLine.JI_CL = ZGuid.Empty;
				invLine.JI_Tariff = "991002";
				entryHeader.MergedLines.RemoveAll();
				entryLine = entryHeader.MergedLines.AddNew();
				invLine.JI_CL = entryLine.PK;
				AdditionalInformationCalculator.Calculate(entryLine);
				AssertHasNoCustomsCode(entryLine.AdditionalInformationCodes, new ZString[] { "DBL", "DDL", "DLV", "DPR", "DPX", "ELX", "KBC", "TBP", "DDX" });
			});
		}

		ZAUniversalReferenceTestDataHelper UniversalReferenceDataHelper
		{
			get
			{
				if (universalReferenceDataHelper == null)
				{
					universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
				}

				return universalReferenceDataHelper;
			}
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
		CusEntryLine CreateCusEntryLine(ZString messageType, ZString basisDeterminationNum)
		{
			var declaration = GetNewJobDeclaration();
			declaration.JE_RemovalTransportCode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._10;
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			header.JZ_VDN = basisDeterminationNum;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			return entryLine;
		}

		CusEntryLine CreateNewCusEntryLine(ZString messageType, ZString cpc, ZString ppc, ZString tariffCode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "10";
			var testLookups = line.Lookups;
			var codes = testLookups.CustomsProcedureCodes;
			foreach (ICodeDescription item in codes)
			{
				line.JI_CEI = (ZGuid)item.PK;
			}

			line.JI_Procedure = line.EntryInstruction.CEI_Style + ppc;
			line.JI_Tariff = tariffCode;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			return entryLine;
		}

		JobDeclaration GetNewJobDeclaration()
		{
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			return exportDeclaration;
		}

		void AssertHasNoCustomsCode(AdditionalInformationCollection collection, IEnumerable<ZString> cusCodesToCheck)
		{
			if (collection.Count > 0)
			{
				foreach (AdditionalInformation item in collection)
				{
					Assert("Should not have " + item.CY_Code, !cusCodesToCheck.Contains(item.CY_Code));
				}
			}
			else
			{
				Assert("Empty Collection", true);
			}
		}

		void AssertHasNoCustomsCodeWithValue(AdditionalInformationCollection collection, ZString cusCodeToCheck, ZString value)
		{
			if (collection.Count > 0)
			{
				bool found = false;
				foreach (AdditionalInformation item in collection)
				{
					if (item.CY_Code == cusCodeToCheck && item.CY_Data == value)
					{
						found = true;
					}
				}

				if (!found)
				{
					Assert("Code With Value Not Found", true);
				}
			}
			else
			{
				Assert("Empty Collection", true);
			}
		}

		void AssertHasCustomsCode(AdditionalInformationCollection addInfoArr, string cusCode, string value)
		{
			bool cusCodeFound = false;
			foreach (AdditionalInformation addInfo in addInfoArr)
			{
				cusCodeFound = (addInfo.CY_Code.ToUpper() == cusCode.ToUpper());
				if (cusCodeFound)
				{
					AssertEquals(cusCode + "Value:", value, addInfo.CY_Data);
					return;
				}
			}

			Assert(cusCode + " does not Exists", cusCodeFound);
		}

		OrgHeader GetNewOrgWithOK_CodeType(ZString oK_CodeType)
		{
			var orgHWithOCC = OrgHeader.New(Factory);
			var oCC = orgHWithOCC.CustomsCodes.AddNew();
			oCC.OK_CodeType = oK_CodeType;
			oCC.OK_CustomsRegNo = "123";
			return orgHWithOCC;
		}
	}
}
