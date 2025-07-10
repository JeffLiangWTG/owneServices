using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestIsDisclaimSanction()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0000000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType1.PK, tariff1.PK, "Fishing Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "0000000000";
			uscTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff.UE_DateTo = ZDateTime.Today;
			uscTariff.UE_PGACodes = "TB1";

			var uscTariff1 = Factory.New<USCTariff>();
			uscTariff1.UE_Tariff = "0000000001";
			uscTariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff1.UE_DateTo = ZDateTime.Today;
			uscTariff1.UE_PGACodes = "TB2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = uscTariff.UE_Tariff;
			invoiceLine.US_UC_NKCountryOfOrigin = "RU";
			invoiceLine.US_DisclaimSanctions = true;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = uscTariff1.UE_Tariff;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine1 = (ICusEntryLine)invoiceLine.CusEntryLine;
			AssertEquals(true, entryLine1.IsDisclaimSanction);

			var entryLine2 = (ICusEntryLine)invoiceLine2.CusEntryLine;
			AssertEquals(false, entryLine2.IsDisclaimSanction);
		}

		public void TestSanctionsAdditionalInfos()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "TB1";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PGACodes = "TB2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var fishing1 = invoiceLine.FishingInformations.AddNew();
			fishing1.US_MethodOfHarvest = "VESSEL";
			fishing1.US_VesselName = "VESSEL NAME";
			fishing1.US_VesselCountry = "GB";
			fishing1.US_VesselIMO = "321546";
			fishing1.US_HarvestedCountry = "AU";
			var fishing2 = invoiceLine.FishingInformations.AddNew();
			fishing2.US_MethodOfHarvest = "HCF";
			fishing2.US_HarvestedCountry = "TH";
			var mining1 = invoiceLine.MiningInformations.AddNew();
			mining1.CountryOfMining = "US";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			var mining2 = invoiceLine2.MiningInformations.AddNew();
			mining2.CountryOfMining = "DE";
			var mining3 = invoiceLine2.MiningInformations.AddNew();
			mining3.CountryOfMining = "FR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine1 = (ICusEntryLine)invoiceLine.CusEntryLine;
			var sanctionsAdditionalInfos1 = entryLine1.SanctionsAdditionalInfos;
			AssertEquals(8, sanctionsAdditionalInfos1.Count());
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "01", CusEntryLine.FshngInfo, CusEntryLine.MethodOfHarvest, "VESSEL");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "01", CusEntryLine.FshngInfo, CusEntryLine.VesselName, "VESSEL NAME");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "01", CusEntryLine.FshngInfo, CusEntryLine.VesselFlag, "GB");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "01", CusEntryLine.FshngInfo, CusEntryLine.VesselIMO, "321546");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "01", CusEntryLine.FshngInfo, CusEntryLine.CountryOfHarvest, "AU");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "02", CusEntryLine.FshngInfo, CusEntryLine.MethodOfHarvest, "HCF");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "02", CusEntryLine.FshngInfo, CusEntryLine.CountryOfHarvest, "TH");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos1, "01", CusEntryLine.MineInfo, CusEntryLine.CountryOfMining, "US");

			var entryLine2 = (ICusEntryLine)invoiceLine2.CusEntryLine;
			var sanctionsAdditionalInfos2 = entryLine2.SanctionsAdditionalInfos;
			AssertEquals(2, sanctionsAdditionalInfos2.Count());
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos2, "01", CusEntryLine.MineInfo, CusEntryLine.CountryOfMining, "DE");
			AssertContainSanctionsAdditionalInfo(sanctionsAdditionalInfos2, "02", CusEntryLine.MineInfo, CusEntryLine.CountryOfMining, "FR");
		}

		void AssertContainSanctionsAdditionalInfo(IEnumerable<ISanctionsAdditionalInfo> sanctions, ZString recordID, ZString recordType, ZString fieldNmae, ZString fieldValue)
		{
			AssertNotNull(sanctions.FirstOrDefault(x => x.RecordID == recordID && x.RecordType == recordType && x.FieldName == fieldNmae && x.FieldValue == fieldValue));
		}

		public void TestQuotaDispositions()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertEquals(typeof(QuotaCusDispositionCollection), entryLine.QuotaDispositions.GetType());
		}

		public void TestAESCusDispositions()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var disposition = entryLine.AESCusDispositions.AddNew();

			AssertEquals(typeof(AESCusDispositionCollection), entryLine.AESCusDispositions.GetType());
			AssertEquals(1, entryLine.AESCusDispositions.Count);
			AssertEquals(false, disposition.IsDeleted);

			entryLine.Delete();
			AssertEquals(0, entryLine.AESCusDispositions.Count);
			AssertEquals(true, disposition.IsDeleted);
		}

		public void TestIAESCusDispositionParentMembers()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var iAESCusDispositionParent = entryLine as Customs.Business.ICusDispositionParent;
			AssertEquals("ParentTableCode", CusEntryLineSchema.Constants.Prefix, iAESCusDispositionParent.ParentTableCode);
			AssertEquals("CollectionMaster", entryLine.PK, iAESCusDispositionParent.CollectionMaster.PK);
		}

		public void TestADDCVDNonReimbursementStatement()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000000";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "0000000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff1.UE_Tariff;
			invoiceLine.US_IsParent = true;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine.PK;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = tariff3.UE_Tariff;
			invoiceLine3.JI_ParentID = invoiceLine.PK;
			invoiceLine3.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var iCusEntryLine = invoiceLine.CusEntryLine as IACECusEntryLine;

			AssertEquals(ADDCVDNonReimbursementList.Codes.OnceOff, iCusEntryLine.ADDCVDNonReimbursementStatement);

			invoiceLine.US_SetInd = "X";
			invoiceLine2.US_SetInd = "V";
			invoiceLine3.US_SetInd = "V";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			iCusEntryLine = invoiceLine.CusEntryLine;

			AssertNotEquals(ADDCVDNonReimbursementList.Codes.OnceOff, iCusEntryLine.ADDCVDNonReimbursementStatement);
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(cusEntryLine,
				"USCusEntryLine",
				schemaTypeName: nameof(AutoCusEntryLine.Schema));
		}

		public void TestICusEntryLineImportFTZNumber()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.US_FTZNo = "1234567";
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var cusEntryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = cusEntryLine.PK;
			AssertEquals(dec.US_FTZNo, ((ICusEntryLine)cusEntryLine).ImportFTZNumber);
		}

		public void TestSpecialProgramsIndicatorCountry()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0811202025";
			importTariff.UE_Unit1 = "KG";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.Today;
			importTariff.UE_SPICode = "A B ";

			var dutyRate = importTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Raspberry;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.022m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0811202025";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals(SpecialProgramList.Codes.AU, entryLine.SpecialProgramsIndicatorCountry);

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.C;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZString.Empty, entryLine.SpecialProgramsIndicatorCountry);
		}

		public void TestIsRandomLineSPINotCAAndS()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0811202025";
			importTariff.UE_Unit1 = "KG";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.Today;

			var dutyRate = importTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Raspberry;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.022m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0811202025";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			Assert(entryLine.IsRandomLineSPINotCAAndS);

			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(!entryLine.IsRandomLineSPINotCAAndS);

			invoiceLine.US_SPI = SpecialProgramList.Codes.SPlus;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(!entryLine.IsRandomLineSPINotCAAndS);

			invoiceLine.US_SPI = SpecialProgramList.Codes.CA;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(!entryLine.IsRandomLineSPINotCAAndS);
		}

		public void TestGettingFromXVVSets()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 4001251m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine1 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.US_DateOfExportFromCountryOfOrigin = ZDateTime.BrettsBirthday;
			invoiceLine2.US_VisaNo = "123456";
			invoiceLine2.US_VisaQty = 10m;
			invoiceLine2.US_VisaUQ = "KG";
			invoiceLine2.US_AgricultureLicNo = "7890";
			invoiceLine2.US_CAExportCertificate = "3428";
			invoiceLine2.US_WoolLicenceNo = "342980";
			invoiceLine2.US_CBTPACertificateNo = "032492";
			invoiceLine2.US_MiscPermitNo = "392038";
			invoiceLine2.US_LumberExportPrice = 2900m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = (ICusEntryLine)invoiceLine1.CusEntryLine;
			//"Parent looking at its 'secondary' line"
			AssertEquals(ZDate.BrettsBirthday, entryLine.DateOfExportationFromCountryOfOrigin);
			AssertEquals("123456", entryLine.VisaNumber);
			AssertEquals(10m, entryLine.VisaQuantity);
			AssertEquals("KG", entryLine.VisaUnitOfMeasure);
			AssertEquals("7890", entryLine.AgricultureLicenseNumber);
			AssertEquals("3428", entryLine.CanadianExportCertificateSugar);
			AssertEquals("342980", entryLine.WoolLicense);
			AssertEquals(2900m, entryLine.SoftwoodLumberExportPrice);
		}

		public void TestVChildEntryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLineParent1 = declaration.InvoiceLines.AddNew();
			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 27m, Core.Constants.CurrencyCodes.UnitedStates);

			invoiceLineParent1.US_SetInd = "X";
			invoiceLineParent1.US_SupTariff = "98130020";
			invoiceLineParent1.JI_Tariff = "9101118010";
			invoiceLineParent1.JI_LinePrice = 2158m;

			var invoiceLine2 = invoiceLineParent1.AddSecondaryInvoiceLine();
			invoiceLine2.US_SetInd = "V";
			invoiceLine2.JI_Tariff = "9101118020";
			invoiceLine2.US_SupTariff = "";
			invoiceLine2.JI_LinePrice = 10062m;

			var invoiceLine3 = invoiceLine2.AddSecondaryInvoiceLine();
			invoiceLine3.US_SetInd = "V";
			invoiceLine3.JI_ParentID = invoiceLine2.PK;
			invoiceLine3.JI_Tariff = "9101118030";
			invoiceLine3.US_SupTariff = "98130020";
			invoiceLine3.JI_LinePrice = 292m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var cusLine0 = invoiceLineParent1.CusEntryLine;
			Assert(!cusLine0.IsVParentLine);
			Assert(!cusLine0.IsVChildLine);

			var cusLine1 = invoiceLine2.CusEntryLine;
			Assert(cusLine1.IsVParentLine);

			var cusLine2 = invoiceLine3.CusEntryLine;
			Assert(cusLine2.IsVChildLine);

			Assert(cusLine1.ChildVLines.Contains(invoiceLine3.GetMatchingSupEntryLine(invoiceLine3.US_SupTariff)));
		}

		public void TestHasProvTariffExemptedAttribute()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1901101600";
			tariff1.UE_DateFrom = startDate;
			tariff1.UE_DateTo = endDate;

			var uscTariff2 = Factory.New<USCTariff>();
			uscTariff2.UE_Tariff = "99031919";
			uscTariff2.UE_DateFrom = startDate;
			uscTariff2.UE_DateTo = endDate;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping.ZZZ_DataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff2 = helper.LoadOrCreateNewTariff(dataGrouping.ZZZ_DataGrouping, tariffType.PK, uscTariff2.UE_Tariff, startDate, endDate);

			helper.CreateNewOrGetExistingTariffAttribute(TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariff2);
			helper.CreateNewOrGetExistingTariffAttribute(TariffAttributeTypes.Codes.TYPE, TariffAttributeTypes.Values.BabyFomula, tariff2);

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "AB5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_Tariff = "1901101600";
			invoiceLine.US_SupTariff = "99031919";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Switzerland;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Switzerland;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("BFF", true, invoiceLine.CusEntryLine.TariffHasBabyFomulaAttribute(invoiceLine.EffectiveDateForDutyRate));
			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			var fees = new List<IFee>(entryLine.Fees);
			AssertEquals(0, fees.Count);

			invoiceLine.US_SupTariff = "N/A";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("BFF", false, invoiceLine.CusEntryLine.TariffHasBabyFomulaAttribute(invoiceLine.EffectiveDateForDutyRate));
			ICusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			var fees1 = new List<IFee>(entryLine1.Fees);
			AssertEquals(1, fees1.Count);
		}

		public void TestChargesAndWeightDoNotDoubleUp()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 8744.69m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_Weight = 54m;
			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

			var charge = invoiceHeader.Charges.AddNew();
			charge.J7_Amount = 265.60m;
			charge.J7_RX_NKCurrency = "USD";
			charge.J7_ChargeType = "OFT";

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9808.00.3000";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_Weight = 0m;
			invoiceLine1.JI_WeightUQ = "KG";

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "8805.21.0000";
			invoiceLine2.JI_LinePrice = 8744.69m;
			invoiceLine2.JI_Weight = 54m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.US_SupTariff = "9802.00.4040";
			invoiceLine2.US_98GoodsValue = 155910.00m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entrySummaryEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(3, entrySummaryEntry.MergedLines.Count);
			AssertEquals(true, invoiceLine1.CusEntryLine.IsSupLineOrNormalLine);
			AssertEquals(true, invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).IsSupLineOrNormalLine);
			AssertEquals(false, invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false).IsSupLineOrNormalLine);

			AssertEquals(266m, invoiceLine1.CusEntryLine.Charges);
			AssertEquals(54m, ((ICusEntryLine)invoiceLine1.CusEntryLine).GrossWeightInKilograms);

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions[0].US_SendMessage = true;

			var entrySummaryMessage = new ACEEntrySummaryMessageBuilder(entrySummaryEntry, ACEEntrySummaryMessageSendingOption.New((EntryHeaderMessageSendingAction)actions[0]), UpdateActionCode.Add).PopulateMessage();
			var ens40s = entrySummaryMessage.MessageBlock.MessageBlocks.FindAll(x => x is Messaging.Business.MessageBuildingBlocks.ACE.Common.AENS40).Cast<Messaging.Business.MessageBuildingBlocks.ACE.Common.AENS40>();

			AssertEquals(1, ens40s.Count());
			AssertEquals(266m, ens40s.First().ChargesAmount);
			AssertEquals(54m, ens40s.First().GrossShippingWeight);
		}

		public void TestCoffeeFeeMandatory()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_ShortDescription = "Coffee Fee Test";
			tariff.UE_Unit1 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			Factory.Save();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			invoiceLine1.JI_CustomsQuantity = 840m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_Weight = 3285.59m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 3285.59m;
			declaration.US_SchDEntry = "1234";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine = entry.MergedLines[0];
			Assert(entryLine.IsPRCoffeeFeeMandatory);

			declaration.US_SchDEntry = "5678";
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5678", "TEST NAME", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeNameState.ZXE_Name, USStateList.Codes.Alabama);
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryLine = entry.MergedLines[0];
			Assert(!entryLine.IsPRCoffeeFeeMandatory);

			declaration.US_SchDEntry = "1234";
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Beef;
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryLine = entry.MergedLines[0];
			Assert(!entryLine.IsPRCoffeeFeeMandatory);
		}

		public void TestRapsberryFeeGetRequiredFeeCodesWhenOrganicExemptionCodeIsEntered()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0811202025";
			importTariff.UE_Unit1 = "KG";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.Today;

			var dutyRate = importTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Raspberry;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.022m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0811202025";
			invoiceLine.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals("Raspberry Fee calculated", 22m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));

			invoiceLine.US_CottonCertificateNo = "123456789";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Raspberry Fee calculated exempt", 0m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));

			Assert("057 should not be included as it is exempt", !entryLine.GetRequiredFees().Contains(Core.Constants.USCustoms.FeeCodes.Raspberry));
		}

		[TestDate(2020, 01, 03)]
		public void TestSecondFTZFT51HasValues()
		{
			SetupTariff99038842();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.IOROrgPK = TestOrg.PK;
			declaration.FTZControlNumber = "00000001";
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;
			declaration.JE_VoyageFlightNo = "12";
			declaration.US_SchDEntry = "8888";

			var bill = declaration.Bills.AddNew();
			bill.US_SESplitShip = false;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6307909870";
			invoiceLine.US_SPI = "N/A";

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "D";
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.US_SupTariff = "99038842";
			invoiceLine.US_SupQty1 = 100m;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Weight = 200m;
			invoiceLine.JI_LinePrice = 2002m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee.CY_FeeAmount = 99m;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_Description = "a description";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("The sconde 51Blocks should have values Weight, Value, Charges, ZoneStatus, HarbourMaintenanceFee",
				"B  8888XJ5FT                                               <<MSGNO PLACEHOLDER>>" +
				"10A         2000000001N8888NSV9         887766554433    123456789012            " +
				"20                              12                                              " +
				"40                                                                              " +
				"500000199038842     HAU000000010000KG                123                        " +
				"5100000000000000000000000000000000000000000                                     " +
				"60A DESCRIPTION                                MID9876543210987654321012        " +
				"50000016307909870   HAU000000010000KG 000000001000D  123                        " +
				"5100000002000000000000010000000000Z00009900                                     " +
				"60A DESCRIPTION                                MID9876543210987654321012        " +
				"Y  8888XJ5FT", message.EM_MessageText);
		}

		[TestDate(2011, 11, 02)]
		public void TestWithLongSPINA()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "SV9";
			declaration.IOROrgPK = TestOrg.PK;
			declaration.FTZControlNumber = "00000001";
			declaration.WarehouseDocAddress.OrganisationPK = WarehouseOrg.PK;
			declaration.JE_VoyageFlightNo = "12";
			declaration.US_SchDEntry = "8888";

			var bill = declaration.Bills.AddNew();
			bill.US_SESplitShip = false;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.US_SPI = "N/A";

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsSecondUnitQty = "D";
			invoiceLine.JI_CustomsSecondQuantity = 10m;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_LinePrice = 1001m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee.CY_FeeAmount = 99m;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_Description = "a description";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var ftzMessageSendingObject = new FTZMessageSendingObject(declaration, UpdateActionCode.Add);
			var builder = new FTZMessageBuilder(ftzMessageSendingObject, ftzMessageSendingObject.ActionCode);
			var message = builder.PopulateMessage();
			AssertEquals("Blocks 61 should be excluded",
				@"B  8888XJ5FT                                               <<MSGNO PLACEHOLDER>>" +
				"10A         1100000001N8888NSV9         887766554433    123456789012            " +
				"20                              0012                                            " +
				"40                                                                              " +
				"50000011234567890   HAU000000010000KG 000000001000D  123                        " +
				"5100000001000000000010010000000000Z00009900                                     " +
				"60A DESCRIPTION                                MID9876543210987654321012        " +
				"Y  8888XJ5FT", message.EM_MessageText);
		}

		public override void TestDutyRateDescription()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0811202025";
			importTariff.UE_Unit1 = "KG";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.Today;

			var dutyRate = importTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Raspberry;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.022m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0811202025";
			invoiceLine.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			entryLine.CL_DutyPercent = 2.7m;
			entryLine.CL_FlatAmount = 0.004m;
			entryLine.CL_FlatAmountUQ = "NO";
			entryLine.US_DutyRateDesc = "3.1c/L + 22.1c/PFL";
			AssertEquals("3.1c/L + 22.1c/PFL", entryLine.DutyRateDescription);
		}

		public void TestDEADetails()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "DE1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_DEADisclaimReason = PGADisclaimReasonList.Codes.A;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff.UE_Tariff;
			invoiceLine2.US_DEAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DEADisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine2.DEAHeaders.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var tariffLine1 = (IGovernmentAgencies)invoiceLine.CusEntryLine;
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.DEAIndicator);
			AssertEquals(0, tariffLine1.DEAHeaders.Count());
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine2.DEAIndicator);
			AssertEquals(1, tariffLine2.DEAHeaders.Count());
			Assert(tariffLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestTTBDetails()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "TB1";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_PGACodes = "TB2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.US_TTBInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine2.TTBLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine1 = (IGovernmentAgencies)invoiceLine.GetEntryLineFor("ENS", true);
			var tariffLine1 = (IGovernmentAgencies)invoiceLine.CusEntryLine;

			var supLine2 = (IGovernmentAgencies)invoiceLine2.GetEntryLineFor("ENS", true);
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;

			AssertEquals(ZString.Empty, supLine1.TTBIndicator);
			AssertEquals(ZString.Empty, supLine1.TTBDisclaimReason);
			AssertEquals(0, supLine1.TTBLines.Count());
			AssertEquals(ZString.Empty, supLine2.TTBIndicator);
			AssertEquals(ZString.Empty, supLine2.TTBDisclaimReason);
			AssertEquals(0, supLine1.TTBLines.Count());

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.TTBIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, tariffLine1.TTBDisclaimReason);
			AssertEquals(0, tariffLine1.TTBLines.Count());
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine2.TTBIndicator);
			AssertEquals(ZString.Empty, tariffLine2.TTBDisclaimReason);
			AssertEquals(1, tariffLine2.TTBLines.Count());

			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());

			Assert(tariffLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestVNEDataWith98()
		{
			var tariffWithEP3 = Factory.New<USCTariff>();
			tariffWithEP3.UE_Tariff = "1000000001";
			tariffWithEP3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithEP3.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithEP3.UE_PGACodes = "FW1EP3";//may be required

			var tariffWithEP4 = Factory.New<USCTariff>();
			tariffWithEP4.UE_Tariff = "1000000001";
			tariffWithEP4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithEP4.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithEP4.UE_PGACodes = "FW1EP4";//required

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = tariffWithEP3.UE_Tariff;
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = tariffWithEP4.UE_Tariff;
			invoiceLine2.US_VNEInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.VehicleLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine1 = (IGovernmentAgencies)invoiceLine.GetEntryLineFor("ENS", true);
			var tariffLine1 = (IGovernmentAgencies)invoiceLine.CusEntryLine;

			var supLine2 = (IGovernmentAgencies)invoiceLine2.GetEntryLineFor("ENS", true);
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;

			AssertEquals(ZString.Empty, supLine1.VNEIndicator);
			AssertEquals(0, supLine1.EPA_VNELines.Count());
			AssertEquals(ZString.Empty, supLine2.VNEIndicator);
			AssertEquals(0, supLine1.EPA_VNELines.Count());

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.VNEIndicator);
			AssertEquals(0, tariffLine1.EPA_VNELines.Count());
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine2.VNEIndicator);
			AssertEquals(1, tariffLine2.EPA_VNELines.Count());

			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());

			Assert(tariffLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestPSTDataWith98()
		{
			var tariffWithEP5 = Factory.New<USCTariff>();
			tariffWithEP5.UE_Tariff = "1000000001";
			tariffWithEP5.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithEP5.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithEP5.UE_PGACodes = "FW1EP5";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();

			JobComInvoiceLine[] invoiceLines = { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4 };
			foreach (var invoiceLine in invoiceLines)
			{
				invoiceLine.US_SupTariff = "9802005060";
				invoiceLine.JI_Tariff = tariffWithEP5.UE_Tariff;
			}

			invoiceLine1.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine1.US_PSTDisclaimProgram = ZString.Empty;

			invoiceLine2.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_PSTDisclaimProgram = PSTProductTypeList.Codes.PS1;

			invoiceLine3.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine3.US_PSTDisclaimProgram = PSTProductTypeList.Codes.PS2;

			invoiceLine4.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine4.PSTLines.AddNew();

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var tariffLine1 = (IGovernmentAgencies)invoiceLine1.CusEntryLine;
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;
			var tariffLine3 = (IGovernmentAgencies)invoiceLine3.CusEntryLine;
			var tariffLine4 = (IGovernmentAgencies)invoiceLine4.CusEntryLine;

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.PSTIndicator);
			AssertEquals(ZString.Empty, tariffLine1.PSTDisclaimProgram);
			AssertEquals(0, tariffLine1.EPA_PSTLines.Count());

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine2.PSTIndicator);
			AssertEquals(PSTProductTypeList.Codes.PS1, tariffLine2.PSTDisclaimProgram);
			AssertEquals(0, tariffLine2.EPA_PSTLines.Count());

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine3.PSTIndicator);
			AssertEquals(PSTProductTypeList.Codes.PS2, tariffLine3.PSTDisclaimProgram);
			AssertEquals(0, tariffLine3.EPA_PSTLines.Count());

			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine4.PSTIndicator);
			AssertEquals(ZString.Empty, tariffLine4.PSTDisclaimProgram);
			AssertEquals(1, tariffLine4.EPA_PSTLines.Count());

			Assert(tariffLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine3.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine4.HasAnyPGADataToBeDeclaredOrDisclaimed());

			var supLine1 = (IGovernmentAgencies)invoiceLine1.GetEntryLineFor("ENS", true);
			var supLine2 = (IGovernmentAgencies)invoiceLine2.GetEntryLineFor("ENS", true);
			var supLine3 = (IGovernmentAgencies)invoiceLine3.GetEntryLineFor("ENS", true);
			var supLine4 = (IGovernmentAgencies)invoiceLine4.GetEntryLineFor("ENS", true);

			AssertEquals(ZString.Empty, supLine1.PSTIndicator);
			AssertEquals(ZString.Empty, supLine1.PSTDisclaimProgram);
			AssertEquals(0, supLine1.EPA_PSTLines.Count());

			AssertEquals(ZString.Empty, supLine2.PSTIndicator);
			AssertEquals(ZString.Empty, supLine2.PSTDisclaimProgram);
			AssertEquals(0, supLine2.EPA_PSTLines.Count());

			AssertEquals(ZString.Empty, supLine3.PSTIndicator);
			AssertEquals(ZString.Empty, supLine3.PSTDisclaimProgram);
			AssertEquals(0, supLine3.EPA_PSTLines.Count());

			AssertEquals(ZString.Empty, supLine4.PSTIndicator);
			AssertEquals(ZString.Empty, supLine4.PSTDisclaimProgram);
			AssertEquals(0, supLine4.EPA_PSTLines.Count());

			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine3.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine4.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestODSDataWith98()
		{
			var tariffWithEP1 = Factory.New<USCTariff>();
			tariffWithEP1.UE_Tariff = "1000000001";
			tariffWithEP1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithEP1.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithEP1.UE_PGACodes = "FW1EP1";//may be required

			var tariffWithEP2 = Factory.New<USCTariff>();
			tariffWithEP2.UE_Tariff = "1000000002";
			tariffWithEP2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithEP2.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithEP2.UE_PGACodes = "FW1EP2";//required

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = tariffWithEP1.UE_Tariff;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = tariffWithEP2.UE_Tariff;
			invoiceLine2.US_ODSInd = OGAIndicatorList.Codes.Declared;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine1 = (IGovernmentAgencies)invoiceLine.GetEntryLineFor("ENS", true);
			var tariffLine1 = (IGovernmentAgencies)invoiceLine.CusEntryLine;

			var supLine2 = (IGovernmentAgencies)invoiceLine2.GetEntryLineFor("ENS", true);
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;

			AssertEquals(ZString.Empty, supLine1.ODSIndicator);
			AssertEquals(ZString.Empty, supLine2.ODSIndicator);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.ODSIndicator);
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine2.ODSIndicator);

			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());

			Assert(tariffLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestFSISDataWith98()
		{
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);

			var tariffWithFS3 = Factory.New<USCTariff>();
			tariffWithFS3.UE_Tariff = "1000000001";
			tariffWithFS3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFS3.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFS3.UE_PGACodes = "FW1EP5FS3";//may be required

			var tariffWithFS4 = Factory.New<USCTariff>();
			tariffWithFS4.UE_Tariff = "1000000002";
			tariffWithFS4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFS4.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFS4.UE_PGACodes = "FW1EP5FS4";//required

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = tariffWithFS3.UE_Tariff;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = tariffWithFS4.UE_Tariff;
			invoiceLine2.US_FSISInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.FSISLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine1 = (IGovernmentAgencies)invoiceLine.GetEntryLineFor("ENS", true);
			var tariffLine1 = (IGovernmentAgencies)invoiceLine.CusEntryLine;

			var supLine2 = (IGovernmentAgencies)invoiceLine2.GetEntryLineFor("ENS", true);
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;

			AssertEquals(ZString.Empty, supLine1.FSISIndicator);
			AssertEquals(0, supLine1.FSISLines.Count());
			AssertEquals(ZString.Empty, supLine2.FSISIndicator);
			AssertEquals(0, supLine1.FSISLines.Count());

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.FSISIndicator);
			AssertEquals(0, tariffLine1.FSISLines.Count());
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine2.FSISIndicator);
			AssertEquals(1, tariffLine2.FSISLines.Count());

			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());

			Assert(tariffLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestACEFDADataWith98()
		{
			var tariffWithFD1 = Factory.New<USCTariff>();
			tariffWithFD1.UE_Tariff = "1000000001";
			tariffWithFD1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD1.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD1.UE_PGACodes = "FW1FD1";//may be required

			var tariffWithFD2 = Factory.New<USCTariff>();
			tariffWithFD2.UE_Tariff = "1000000002";
			tariffWithFD2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD2.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD2.UE_PGACodes = "FW1FD2";//required

			var tariffWithFD3 = Factory.New<USCTariff>();
			tariffWithFD3.UE_Tariff = "1000000003";
			tariffWithFD3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD3.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD3.UE_PGACodes = "FW1FD3";//may be required

			var tariffWithFD4 = Factory.New<USCTariff>();
			tariffWithFD4.UE_Tariff = "1000000004";
			tariffWithFD4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD4.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD4.UE_PGACodes = "FW1FD4";//required

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = tariffWithFD1.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = tariffWithFD2.UE_Tariff;
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine2.ACE_FDALines.AddNew();

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "9802005060";
			invoiceLine3.JI_Tariff = tariffWithFD3.UE_Tariff;
			invoiceLine3.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.US_SupTariff = "9802005060";
			invoiceLine4.JI_Tariff = tariffWithFD4.UE_Tariff;
			invoiceLine4.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine4.ACE_FDALines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine1 = (IGovernmentAgencies)invoiceLine.GetEntryLineFor("ENS", true);
			var tariffLine1 = (IGovernmentAgencies)invoiceLine.CusEntryLine;

			var supLine2 = (IGovernmentAgencies)invoiceLine2.GetEntryLineFor("ENS", true);
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;

			var supLine3 = (IGovernmentAgencies)invoiceLine3.GetEntryLineFor("ENS", true);
			var tariffLine3 = (IGovernmentAgencies)invoiceLine3.CusEntryLine;

			var supLine4 = (IGovernmentAgencies)invoiceLine4.GetEntryLineFor("ENS", true);
			var tariffLine4 = (IGovernmentAgencies)invoiceLine4.CusEntryLine;

			AssertEquals(ZString.Empty, supLine1.ACEFDAIndicator);
			AssertEquals(0, supLine1.FDALines.Count());
			AssertEquals(ZString.Empty, supLine2.ACEFDAIndicator);
			AssertEquals(0, supLine1.FDALines.Count());
			AssertEquals(ZString.Empty, supLine3.ACEFDAIndicator);
			AssertEquals(0, supLine3.FDALines.Count());
			AssertEquals(ZString.Empty, supLine4.ACEFDAIndicator);
			AssertEquals(0, supLine4.FDALines.Count());

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.ACEFDAIndicator);
			AssertEquals(0, tariffLine1.FDALines.Count());
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine2.ACEFDAIndicator);
			AssertEquals(1, tariffLine2.FDALines.Count());
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine3.ACEFDAIndicator);
			AssertEquals(0, tariffLine3.FDALines.Count());
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine4.ACEFDAIndicator);
			AssertEquals(1, tariffLine4.FDALines.Count());

			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine3.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert("OI construction uses this logic and should not return true both for sup line and tariff line", !supLine4.HasAnyPGADataToBeDeclaredOrDisclaimed());

			Assert(tariffLine1.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine2.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine3.HasAnyPGADataToBeDeclaredOrDisclaimed());
			Assert(tariffLine4.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestRapsberryFeeForACE()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0811202025";
			importTariff.UE_Unit1 = "KG";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.Today;

			var dutyRate = importTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Raspberry;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.022m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0811202025";
			invoiceLine.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals("Raspberry Fee calculated", 22m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));

			var feeExmpt = invoiceLine.LicenceAndPermits.AddNew();
			feeExmpt.CY_Code = LicencePermitTypeList.Codes._22;
			feeExmpt.CY_Data = "RASP0012";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Raspberry Fee calculated exempt", 0m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));

			feeExmpt.CY_Code = LicencePermitTypeList.Codes._23;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Raspberry Fee calculated exempt", 0m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));

			Assert("057 should not be included as it is exempt", !entryLine.GetRequiredFees().Contains(Core.Constants.USCustoms.FeeCodes.Raspberry));
		}

		public void TestRequiredFeeForCottonFeeExempt()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = "6117.80.9510";//Cotton Fee - Flagged as Required

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			Assert(!entryLine.Fees.Any(x => x.Code == Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine.US_SupTariff = "";
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
			Assert(!entryLine.Fees.Any(x => x.Code == Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		public void TestInvoiceLinesIsSortedAfterMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = ZBool.True;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV2";
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_Tariff = "1010101010";
			invoice1Line1.JI_Description = "INV 1 LINE 2";
			invoice1Line1.JI_InvoiceQuantity = 3m;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_LineNo = 1;
			invoice1Line2.JI_Tariff = "1010101010";
			invoice1Line2.JI_Description = "INV 1 LINE 1";
			invoice1Line2.JI_InvoiceQuantity = 1m;
			var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line3.JI_Tariff = "1010101010";
			invoice1Line3.JI_Description = "INV 1 LINE 3";
			invoice1Line3.JI_InvoiceQuantity = 0.5m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV1";
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_Tariff = "1010101010";
			invoice2Line1.JI_Description = "INV 2 LINE 1";
			invoice2Line1.JI_InvoiceQuantity = 4m;
			var invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_Tariff = "1010101010";
			invoice2Line2.JI_Description = "INV 2 LINE 2";
			invoice2Line2.JI_InvoiceQuantity = 2m;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			var entryLine1 = entry.MergedLines[1];
			var entryLine2 = entry.MergedLines[0];
			AssertEquals(2, entryLine1.InvoiceLines.Count);
			AssertEquals(invoice2Line1, entryLine1.InvoiceLines[0]);
			AssertEquals(invoice2Line2, entryLine1.InvoiceLines[1]);
			AssertEquals(3, entryLine2.InvoiceLines.Count);
			AssertEquals(invoice1Line2, entryLine2.InvoiceLines[0]);
			AssertEquals(invoice1Line1, entryLine2.InvoiceLines[1]);
			AssertEquals(invoice1Line3, entryLine2.InvoiceLines[2]);

			entryLine1.InvoiceLines.Sort(JobComInvoiceLine.Schema.JI_InvoiceQuantity);
			AssertEquals(invoice2Line2, entryLine1.InvoiceLines[0]);
			AssertEquals(invoice2Line1, entryLine1.InvoiceLines[1]);
			entryLine2.InvoiceLines.Sort(JobComInvoiceLine.Schema.JI_InvoiceQuantity);
			AssertEquals(invoice1Line3, entryLine2.InvoiceLines[0]);
			AssertEquals(invoice1Line2, entryLine2.InvoiceLines[1]);
			AssertEquals(invoice1Line1, entryLine2.InvoiceLines[2]);

			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			entryLine1 = entry.MergedLines[1];
			entryLine2 = entry.MergedLines[0];
			AssertEquals(2, entryLine1.InvoiceLines.Count);
			AssertEquals(invoice2Line1, entryLine1.InvoiceLines[0]);
			AssertEquals(invoice2Line2, entryLine1.InvoiceLines[1]);
			AssertEquals(3, entryLine2.InvoiceLines.Count);
			AssertEquals(invoice1Line2, entryLine2.InvoiceLines[0]);
			AssertEquals(invoice1Line1, entryLine2.InvoiceLines[1]);
			AssertEquals(invoice1Line3, entryLine2.InvoiceLines[2]);
		}

		public void TestCalculateADDOnCL_CustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 47269.19m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var otherCharge = invoice.Charges.AddNew();
			otherCharge.J7_ChargeType = "OTH";
			otherCharge.J7_Amount = 72m;
			otherCharge.J7_RX_NKCurrency = "USD";
			otherCharge.J7_IsIncludedInITOT = false;
			otherCharge.J7_IsNotIncludedInInvoice = false;

			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_LinePrice = 3067.38m;
			line1.JI_Tariff = "4011.63.0000";
			line1.US_CVDCaseNo = "C570913000";

			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_LinePrice = 7363.60m;
			line2.JI_Tariff = "4011.92.0000";
			line2.US_CVDCaseNo = "C570913000";

			var line3 = invoice.InvoiceLines.AddNew();
			line3.JI_LinePrice = 4690.63m;
			line3.JI_Tariff = "4011.61.0000";
			line3.US_CVDCaseNo = "C570913000";

			var line4 = invoice.InvoiceLines.AddNew();
			line4.JI_LinePrice = 32075.58m;
			line4.JI_Tariff = "4011.62.0000";
			line4.US_CVDCaseNo = "C570913000";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Value for ADD", 0m, line4.CusEntryLine.ValueForADD);
			AssertEquals("Value for CVD", 32124m, line4.CusEntryLine.ValueForCVD);
		}

		public void TestCalculateADDOnCL_CustomsValueWithSup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_LinePrice = 3000m;
			line1.JI_Tariff = "4011.63.0000";
			line1.US_CVDCaseNo = "C570913000";
			line1.US_SupTariff = "9802004040";
			line1.US_98GoodsValue = 100m;

			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_LinePrice = 7000m;
			line2.JI_Tariff = "4011.63.0000";
			line2.US_CVDCaseNo = "C570913000";
			line2.US_SupTariff = "9802004040";
			line2.US_98GoodsValue = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotEquals("PreCondition:Line 1 and 2 do not get merged due to Prov. Tariff entered.", line1.CusEntryLine, line2.CusEntryLine);
			AssertEquals("ValueForCVD only adds up a value from a randomLine and it is not incorrect as two invoice lines do not get merged", 3100m, line1.GetEntryLineFor("ENS", true).ValueForCVD);
		}

		public void TestExportDate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.US_EntryFilerCode = "XXX";
			dec.US_EnableENS = true;
			dec.US_DateOfExport = ZDateTime.BrettsBirthday;

			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZDateTime.BrettsBirthday, invoiceLine.CusEntryLine.ExportDate);

			invoiceLine.US_DateOfExport = ZDateTime.BrettsBirthday.AddDays(1);
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), invoiceLine.CusEntryLine.ExportDate);
		}

		public void TestTaxRateQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_LinePrice = 3000m;
			line1.JI_Tariff = "3303.00.3000";
			line1.US_TaxApply = TaxApplyList.Codes.Override;
			line1.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_1;
			line1.US_TaxQty = 100m;
			line1.US_SupTariff = "9802004040";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = line1.CusEntryLine;
			var supLine = line1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);

			AssertEquals(100m, ((IFeeCalculationDataProvider)entryLine).TaxRateQuantity);
			AssertEquals("For sup line, it should not return quantity for tax as we should not calculate tax twice", 0m, ((IFeeCalculationDataProvider)supLine).TaxRateQuantity);
		}

		public void TestTaxRateQuantityWithSup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_LinePrice = 3000m;
			line1.JI_Tariff = "3303.00.3000";
			line1.US_TaxApply = TaxApplyList.Codes.Override;
			line1.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_1;
			line1.US_TaxQty = 100m;

			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_LinePrice = 7000m;
			line2.JI_Tariff = "3303.00.3000";
			line2.US_TaxApply = TaxApplyList.Codes.Override;
			line2.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_1;
			line2.US_TaxQty = 200m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Precondition", line1.CusEntryLine, line2.CusEntryLine);
			AssertEquals(300m, ((IFeeCalculationDataProvider)line1.CusEntryLine).TaxRateQuantity);
		}

		public void TestIDrawbackEntryLineMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-10);
			declaration.US_SchDEntry = "8888";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceNumber = "Inv-001";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_LinePrice = 5000m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entry = declaration.FormalEntry;
			AssertEquals(2, entry.MergedLines.Count);

			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing].C1_ChargeAmount = 25m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.HMF].C1_ChargeAmount = 30m;

			var entryLine = entry.MergedLines[1];
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 10m);
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Watermelon, 5m);

			var drawbackEntryLine = (IDrawbackEntryLine)entryLine;
			AssertEquals(declaration.JE_DateOfArrival, drawbackEntryLine.EntryDate);
			AssertEquals("8888", drawbackEntryLine.EntryPort);
			AssertEquals(false, drawbackEntryLine.IsFTZAdmission);
			AssertEquals(25m, drawbackEntryLine.MPFAmountForEntry);
			AssertEquals(30m, drawbackEntryLine.HMFAmountForEntry);
			AssertEquals(15000m, drawbackEntryLine.TotalEnteredValueForEntry);
			AssertEquals(entry.MergedLines[0].PK, drawbackEntryLine.ParentLine.PK);
			AssertEquals(0, drawbackEntryLine.ChildSecondaryEntryLines.Count());
			AssertEquals(1, drawbackEntryLine.ParentLine.ChildSecondaryEntryLines.Count());
			AssertEquals(5m, drawbackEntryLine.GetFeeAmount("104"));
			AssertEquals(2, drawbackEntryLine.Fees.Count());
		}

		public void TestXAndVLineForFTZAndHMF()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			dec.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = dec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var line1 = dec.InvoiceLines.AddNew();
			line1.JI_LinePrice = 0m;
			line1.US_SecondarySPI = "X";

			var line2 = dec.InvoiceLines.AddNew();
			line2.JI_LinePrice = 5000m;
			line2.US_SecondarySPI = "V";
			line2.JI_ParentID = line1.PK;

			var line3 = dec.InvoiceLines.AddNew();
			line3.JI_LinePrice = 2000m;
			line3.US_SecondarySPI = "V";
			line3.JI_ParentID = line1.PK;

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(dec.ActiveEntryHeaders.FTZEntry);
			AssertEquals("X - CustomsValue", 7000m, line1.CusEntryLine.CL_CustomsValue);
			AssertNotEquals("Some HMF payable on X", 0m, line1.CusEntryLine.HMFAmount);
		}

		public void TestIFTZLineMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EntryFilerCode = "XJ5";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123910-00001");
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789");

			var orgIOR = Factory.NewWithValidTestData<OrgHeader>();
			orgIOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "33-8887743");
			declaration.IOROrgPK = orgIOR.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.Charges.AddNew("OFT", 10m, "USD");

			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;

			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_CustomsSecondUnitQty = "D";
			invoiceLine1.JI_CustomsSecondQuantity = 10m;
			invoiceLine1.US_TextileCategoryNo = "123";
			invoiceLine1.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine1.JI_Weight = 100m;
			invoiceLine1.JI_LinePrice = 1001m;
			invoiceLine1.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee = invoiceLine1.FeeCusCodes.AddNew();
			fee.CY_IsOverridden = true;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee.CY_FeeAmount = 99m;

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";

			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "9876543210987654321012");
			invoiceLine1.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine1.JI_Description = "a description";

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1234567890";
			invoiceLine2.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsQuantity = 100m;
			invoiceLine2.JI_CustomsSecondUnitQty = "D";
			invoiceLine2.JI_CustomsSecondQuantity = 10m;
			invoiceLine2.US_TextileCategoryNo = "123";
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine2.JI_Weight = 100m;
			invoiceLine2.JI_LinePrice = 1001m;
			invoiceLine2.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;
			var fee1 = invoiceLine2.FeeCusCodes.AddNew();
			fee1.CY_IsOverridden = true;
			fee1.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee1.CY_FeeAmount = 99m;

			invoiceLine2.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine2.JI_Description = "a description";
			invoiceLine1.US_F_PNDisclaimer = YesNoDefaultList.Codes.Yes;

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IFTZLine ftzLine = declaration.ActiveEntryHeaders.FTZEntry.EntryLines.FirstOrDefault(x => x.RandomLine == invoiceLine1);
			AssertEquals("Tariff", "1234567890", ftzLine.Tariff);
			AssertEquals("SPI Indicator", PrimarySpecProgramIndicatorList.Codes.B, ftzLine.SpecialProgramsIndicatorPrimary);
			AssertEquals("Long SPI Code", ZString.Empty, ftzLine.LongSPICode);
			AssertEquals("Secondary SPI", SecondarySpecProgIndicatorList.Codes.H, ftzLine.SecondarySPI);
			AssertEquals("Country of Origin", "AU", ftzLine.CountryOfOrigin);
			AssertEquals("Quantity 1", 100m, ftzLine.Quantity1);
			AssertEquals("Unit of measure 1", "KG", ftzLine.UQ1);
			AssertEquals("Quantity 2", 10m, ftzLine.Quantity2);
			AssertEquals("Unit of measure 2", "D", ftzLine.UQ2);
			AssertEquals("Quota category", "123", ftzLine.QuotaCategory);
			AssertEquals("PN Disclaimer Flag", "Y", ftzLine.PNDisclaimer);
			AssertEquals("Weight", 100m, ftzLine.Weight);
			AssertEquals("Value", 1001m, ftzLine.Value);
			AssertEquals("Zone status", ZoneStatusList.Codes.ZoneRestricted, ftzLine.ZoneStatus);
			AssertEquals("Charges", 5m, ftzLine.Charges);
			AssertEquals("Harbour Maintenance Fee", 99m, ftzLine.HMF);
			AssertEquals("IOR EIN Number", "33-8887743", ftzLine.ImporterReferenceID);
			AssertEquals("Manufacturer ID", "9876543210987654321012", ftzLine.ManufacturerReferenceID);
			AssertEquals("Description", "a description", ftzLine.Remarks);

			invoiceLine1.US_SPI = SpecialProgramList.Codes.AU;
			invoiceLine2.US_SPI = SpecialProgramList.Codes.AU;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("SPI Country", SpecialProgramList.Codes.AU, ftzLine.SpecialProgramsIndicatorCountry);
			AssertEquals("SPI Indicator", ZString.Empty, ftzLine.SpecialProgramsIndicatorPrimary);
			AssertEquals("Long SPI Code", "AU", ftzLine.LongSPICode);

			invoiceLine1.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.NoUnitRequired;
			invoiceLine1.US_ManifestQty = 13;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Quantity 1", 13m, ftzLine.Quantity1);
			AssertEquals("Unit of measure 1", ABIUnitOfMeasureList.Codes.NoUnitRequired, ftzLine.UQ1);
		}

		public void TestFTZWeightAndUnit_DecimalTooLargeOverflow()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			var reallyheavyobject = Factory.NewMoq<Customs.Business.BaseJobComInvoiceLine>();
			reallyheavyobject.Object.JI_CL = entryLine.PK;
			reallyheavyobject.Setup(m => m.EffectiveGrossWeight).Returns(new ZWeight(2147483648m, "KG"));
			string value = "";
			AssertNoExceptionThrown(() => value = entryLine.FTZWeightAndUnit);
			AssertEquals(value, "2147483648 KG");
		}

		public void TestChargesAs1WhenMandatoryAndFreightEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Assert(declaration.IsChargeMandatory);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 4001251m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 4000000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1251m;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 276.00m;
			invoiceLine3.US_SupTariff = "9802005060";
			invoiceLine3.US_98GoodsValue = 975.00m;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 1251.00m;
			invoiceLine4.US_SupTariff = "9802005060";
			invoiceLine4.US_98GoodsValue = 975.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.CusEntryLine.Charges);
			AssertEquals("Mandatory. But no freight entered", 0m, invoiceLine2.CusEntryLine.Charges);

			invoice.Charges.AddNew("OFT", 10m, "USD");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(10m, invoiceLine.CusEntryLine.Charges);
			AssertEquals("Mandatory. Should be sent $1", 1m, invoiceLine2.CusEntryLine.Charges);
			AssertEquals("Both lines are <1250", 0m, invoiceLine3.CusEntryLine.ParentLine.Charges);
			AssertEquals("One of the lines is >1251", 1m, invoiceLine4.CusEntryLine.ParentLine.Charges);
		}

		public void TestChargeWhenNotMandatoryAndFreightEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Assert(!declaration.IsChargeMandatory);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 4001251m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.Charges.AddNew("OFT", 10m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 4000000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1251m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(10m, invoiceLine.CusEntryLine.Charges);
			AssertEquals("Not mandatory", 0m, invoiceLine2.CusEntryLine.Charges);
		}

		public void TestWhenParentAndChildHaveCATNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 4001251m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104291030";
			invoiceLine.US_TextileCategoryNo = "123";

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6104692040";
			invoiceLine2.US_TextileCategoryNo = "456";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6104291040";
			invoiceLine3.US_TextileCategoryNo = "123";

			var invoiceLine4 = invoiceLine3.AddSecondaryInvoiceLine();
			invoiceLine4.JI_Tariff = "6104692030";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("TextileCategoryNumber", "456", ((ICusEntryLine)invoiceLine.CusEntryLine).TextileCategoryNumber);
			AssertEquals("TextileCategoryNumber", "123", ((ICusEntryLine)invoiceLine3.CusEntryLine).TextileCategoryNumber);
		}

		public void TestChargesCeilingAs1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 4001251m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.Charges.AddNew("OFT", 0.4m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 4000000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Ceiling as 1", 1m, invoiceLine.CusEntryLine.Charges);
		}

		public void TestGettingFromSecondaryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 4001251m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.US_DateOfExportFromCountryOfOrigin = ZDateTime.BrettsBirthday;
			invoiceLine2.US_VisaNo = "123456";
			invoiceLine2.US_VisaQty = 10m;
			invoiceLine2.US_VisaUQ = "KG";
			invoiceLine2.US_AgricultureLicNo = "7890";
			invoiceLine2.US_CAExportCertificate = "3428";
			invoiceLine2.US_WoolLicenceNo = "342980";
			invoiceLine2.US_CBTPACertificateNo = "032492";
			invoiceLine2.US_MiscPermitNo = "392038";
			invoiceLine2.US_LumberExportPrice = 2900m;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine4.US_DateOfExportFromCountryOfOrigin = ZDateTime.BrettsBirthday;
			AssertEquals("PreCondition", invoiceLine3, invoiceLine4.ParentTariffLine);
			invoiceLine4.US_DateOfExportFromCountryOfOrigin = ZDateTime.BrettsBirthday;
			invoiceLine4.US_VisaNo = "123456";
			invoiceLine4.US_VisaQty = 10m;
			invoiceLine4.US_VisaUQ = "KG";
			invoiceLine4.US_AgricultureLicNo = "7890";
			invoiceLine4.US_CAExportCertificate = "3428";
			invoiceLine4.US_WoolLicenceNo = "342980";
			invoiceLine4.US_CBTPACertificateNo = "032492";
			invoiceLine4.US_MiscPermitNo = "392038";
			invoiceLine4.US_LumberExportPrice = 2900m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = (ICusEntryLine)invoiceLine.CusEntryLine;
			//"Parent looking at its 'secondary' line"
			AssertEquals(ZDate.BrettsBirthday, entryLine.DateOfExportationFromCountryOfOrigin);
			AssertEquals("123456", entryLine.VisaNumber);
			AssertEquals(10m, entryLine.VisaQuantity);
			AssertEquals("KG", entryLine.VisaUnitOfMeasure);
			AssertEquals("7890", entryLine.AgricultureLicenseNumber);
			AssertEquals("3428", entryLine.CanadianExportCertificateSugar);
			AssertEquals("342980", entryLine.WoolLicense);
			AssertEquals(2900m, entryLine.SoftwoodLumberExportPrice);

			//However X line should not look at V line
			var entryLine3 = (ICusEntryLine)invoiceLine3.CusEntryLine;
			AssertEquals(ZDate.Empty, entryLine3.DateOfExportationFromCountryOfOrigin);
			AssertEquals("", entryLine3.VisaNumber);
			AssertEquals(0m, entryLine3.VisaQuantity);
			AssertEquals("", entryLine3.VisaUnitOfMeasure);
			AssertEquals("", entryLine3.AgricultureLicenseNumber);
			AssertEquals("", entryLine3.CanadianExportCertificateSugar);
			AssertEquals("", entryLine3.WoolLicense);
			AssertEquals(0m, entryLine3.SoftwoodLumberExportPrice);
		}

		public void TestEntryLineCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var entryNumber = CusEntryNumber.New(declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Core.Constants.CountryCodes.UnitedStates);
			entryNumber.CE_EntryNum = "43245323";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801001049";
			invoiceLine.JI_Tariff = "2403.10.2080";
			invoiceLine.JI_LinePrice = 1.23m;
			invoiceLine.US_SPI = "CL";
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			AssertNotNull("precondition", invoiceLine.ImportTariff);

			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var normalLine = invoiceLine.CusEntryLine;

			AssertEquals("EntryLineCode", "Line 1, Tariff 2", normalLine.EntryLineCode);
			AssertEquals("EntryLineDescription", "2403.10.2080 from CA valued at $1(1 KG)", normalLine.EntryLineDescription);
			normalLine.IsInGlobalCusEntryLineCollection = true;
			AssertEquals("EntryLineCode", "XJ543245323-1", normalLine.EntryLineCode);
			AssertEquals("EntryLineDescription", "XJ543245323-1", normalLine.EntryLineDescription);
		}

		public void TestTaxCalculationWithSupLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801001049";
			invoiceLine.JI_Tariff = "2403.10.2080";
			invoiceLine.US_SPI = "CL";
			AssertNotNull("precondition", invoiceLine.ImportTariff);

			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine supLine = invoiceLine.GetEntryLineFor("ENS", true);
			CusEntryLine normalLine = invoiceLine.CusEntryLine;
			AssertEquals(0m, supLine.Fees.GetTotal());
			AssertEquals(2.42m, normalLine.Fees.GetTotal());
			AssertEquals(2.42m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid);
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateDairyFee()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.DairyFeeApplicable;
			AssertNotNull(invoiceLine.ImportTariff);
			invoiceLine.ImportTariff.SetUpTestDataForDairyFeeWithXComputationCode();
			invoiceLine.JI_CustomsThirdUnitQty = "CKG";
			invoiceLine.JI_CustomsThirdQuantity = 1000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = USCTariff.DairyFeeApplicable;
			invoiceLine2.JI_CustomsThirdUnitQty = "CKG";
			invoiceLine2.JI_CustomsThirdQuantity = 500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition", invoiceLine.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(1500m, invoiceLine.CusEntryLine.TotalDairyQty);
			AssertEquals(19.91m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateDairyFeeWhenSupLineIsInvolved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99040201";
			invoiceLine.JI_Tariff = USCTariff.DairyFeeApplicable;
			AssertNotNull(invoiceLine.ImportTariff);
			invoiceLine.ImportTariff.SetUpTestDataForDairyFeeWithXComputationCode();
			invoiceLine.JI_CustomsThirdUnitQty = "CKG";
			invoiceLine.JI_CustomsThirdQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1000m, invoiceLine.CusEntryLine.TotalDairyQty);
			AssertEquals(13.27m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
		}

		public void TestCensusWarningOverridesForParentLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801001049";
			invoiceLine.JI_Tariff = "2403.10.2080";
			var one = invoiceLine.CensusWarningOverrides.AddNew();
			one.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			one.CY_Data = CensusOverrideCodeList.Codes._01;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine = invoiceLine.GetEntryLineFor("ENS", true);
			var normalLine = invoiceLine.CusEntryLine;

			AssertEquals(1, ((IACECusEntryLine)supLine).CensusWarningOverrideCodes.Count());
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, ((IACECusEntryLine)supLine).CensusWarningOverrideCodes.ElementAt(0).ConditionCode);
			AssertEquals(CensusOverrideCodeList.Codes._01, ((IACECusEntryLine)supLine).CensusWarningOverrideCodes.ElementAt(0).OverrideCode);

			AssertEquals(1, ((IACECusEntryLine)normalLine).CensusWarningOverrideCodes.Count());
			AssertEquals(CensusWarningCodeList.Codes.GrossWeightVessel, ((IACECusEntryLine)normalLine).CensusWarningOverrideCodes.ElementAt(0).ConditionCode);
			AssertEquals(CensusOverrideCodeList.Codes._01, ((IACECusEntryLine)normalLine).CensusWarningOverrideCodes.ElementAt(0).OverrideCode);

			invoiceLine.US_SupTariff = ZString.Empty;
			var secondary = invoiceLine.AddSecondaryInvoiceLine();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			normalLine = invoiceLine.CusEntryLine;
			var secondaryLine = secondary.CusEntryLine;

			var two = invoiceLine.CensusWarningOverrides.AddNew();
			two.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			two.CY_Data = CensusOverrideCodeList.Codes._02;

			var three = secondary.CensusWarningOverrides.AddNew();
			three.CY_Code = CensusWarningCodeList.Codes.GrossWeightVessel;
			three.CY_Data = CensusOverrideCodeList.Codes._03;

			AssertEquals(3, ((IACECusEntryLine)normalLine).CensusWarningOverrideCodes.Count());
			AssertEquals(1, ((IACECusEntryLine)secondaryLine).CensusWarningOverrideCodes.Count());
		}

		public void TestIOGAWithSupLineForFDA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801001049";//no fda requirement
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "8512300030";// radar detector, no FDA requirement
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			FDA fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAForcePN = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IOGA classificationLine = invoiceLine.CusEntryLine;
			IOGA supplementaryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(OGAIndicatorList.Codes.Declared, classificationLine.FDAIndicator);
			AssertEquals(ZString.Empty, supplementaryLine.FDAIndicator);
			AssertEquals(ZString.Empty, classificationLine.DOTIndicator);
			AssertEquals(ZString.Empty, supplementaryLine.DOTIndicator);

			AssertEquals(1, classificationLine.FDA.Count);
			AssertEquals(1, supplementaryLine.FDA.Count);
			AssertEquals(false, classificationLine.DOT.Any());
			AssertEquals(false, supplementaryLine.DOT.Any());
		}

		public void TestCustomsValueForXLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_LinePrice = 0m;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("PreCondition", SecondarySpecProgIndicatorList.Codes.V, invoiceLine2.US_SecondarySPI);
			invoiceLine2.JI_LinePrice = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine ensXLine = invoiceLine.CusEntryLine;
			CusEntryLine crlXLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false);
			AssertEquals("ENS X line's value", 100m, ensXLine.CL_CustomsValue);
			AssertEquals("CRL X line's value", 0m, crlXLine.CL_CustomsValue);
		}

		public void TestIOGAWithParentAndChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801001049";//no fda requirement
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FCCs.AddNew();

			invoiceLine.JI_Tariff = "8512300030";// radar detector, no FDA requirement
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs.AddNew();

			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.DOTs.AddNew();
			invoiceLine.DOTs.AddNew();
			invoiceLine.DOTs.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine childLine = invoiceLine.CusEntryLine;
			AssertEquals(1, ((IOGA)childLine).FDA.Count);
			AssertEquals(3, ((IOGA)childLine).DOT.Count());

			CusEntryLine parentLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals(1, ((IOGA)parentLine).FDA.Count);
			AssertEquals(3, ((IOGA)parentLine).DOT.Count());
		}

		public void Test9802WithXAndVLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Tariff = "6307906800";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.US_SupTariff = "9802008068";
			invoiceLine2.JI_Tariff = "6307906800";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "4818200020";
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			AssertEquals("MergedLines", 5, entry.MergedLines.Count);

			var supLine1 = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			Assert(!supLine1.IsSecondaryTariffLine);

			var normalLine1 = invoiceLine.CusEntryLine;
			Assert(normalLine1.IsSecondaryTariffLine);

			var supLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			Assert(!supLine2.IsSecondaryTariffLine);
			var normalLine2 = invoiceLine2.CusEntryLine;
			Assert(normalLine2.IsSecondaryTariffLine);

			var normalLine3 = invoiceLine3.CusEntryLine;
			Assert(!normalLine3.IsSecondaryTariffLine);

			var entryLinesDeclaredOn40s = ((ICusEntryHeader)entry).EntryLines;

			AssertEquals("EntyLines declared on 40s", 3, entryLinesDeclaredOn40s.Count());
			Assert("Should have supLine1", entryLinesDeclaredOn40s.Contains(supLine1));
			Assert("Should have supLine2", entryLinesDeclaredOn40s.Contains(supLine2));
			Assert("Should have normalLine3", entryLinesDeclaredOn40s.Contains(normalLine3));
			Assert("Should NOT have normalLine1", !entryLinesDeclaredOn40s.Contains(normalLine1));
			Assert("Should NOT have normalLine2", !entryLinesDeclaredOn40s.Contains(normalLine2));

			AssertEquals(1, supLine1.ChildSecondaryEntryLines.Count);
			AssertEquals(1, supLine2.ChildSecondaryEntryLines.Count);

			AssertEquals("Should have normalLine1 as secondary line", 1, ((ICusEntryLine)supLine1).SecondaryTariffLines.Count());
			AssertEquals("Should have normalLine1 as secondary line", 1, ((ICusEntryLine)supLine2).SecondaryTariffLines.Count());

			AssertEquals("supLine1.SecondarySPI", SecondarySpecProgIndicatorList.Codes.X, ((ICusEntryLine)supLine1).SpecialProgramsIndicatorSecondary);
			AssertEquals("normalLine1.SecondarySPI", ZString.Empty, ((ICusEntryLine)normalLine1).SpecialProgramsIndicatorSecondary);
			AssertEquals("supLine2.SecondarySPI", SecondarySpecProgIndicatorList.Codes.V, ((ICusEntryLine)supLine2).SpecialProgramsIndicatorSecondary);
			AssertEquals("normalLine2.SecondarySPI", ZString.Empty, ((ICusEntryLine)normalLine2).SpecialProgramsIndicatorSecondary);
			AssertEquals("normalLine3.SecondarySPI", SecondarySpecProgIndicatorList.Codes.V, ((ICusEntryLine)normalLine3).SpecialProgramsIndicatorSecondary);
		}

		public void TestIOGAWithSupLineForFDA2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801001010";//FD3
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "8512300030";// radar detector, no FDA requirement
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			FDA fda = invoiceLine.FDAs.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IOGA classificationLine = invoiceLine.CusEntryLine;
			IOGA supplementaryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(ZString.Empty, classificationLine.FDAIndicator);
			AssertEquals(OGAIndicatorList.Codes.Declared, supplementaryLine.FDAIndicator);
			AssertEquals(ZString.Empty, classificationLine.DOTIndicator);
			AssertEquals(ZString.Empty, supplementaryLine.DOTIndicator);

			AssertEquals(1, classificationLine.FDA.Count);
			AssertEquals(1, supplementaryLine.FDA.Count);
			Assert(!classificationLine.DOT.Any());
			Assert(!supplementaryLine.DOT.Any());
		}

		public void TestIOGAWithSupLineForDOT()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801008000";//no FDA requirement
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "7007110010";//DT1
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";

			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			DOT dot = invoiceLine.DOTs.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IOGA classificationLine = invoiceLine.CusEntryLine;
			IOGA supplementaryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(ZString.Empty, classificationLine.FDAIndicator);
			AssertEquals(ZString.Empty, supplementaryLine.FDAIndicator);
			AssertEquals(OGAIndicatorList.Codes.Declared, classificationLine.DOTIndicator);
			AssertEquals(ZString.Empty, supplementaryLine.DOTIndicator);

			AssertEquals(0, classificationLine.FDA.Count);
			AssertEquals(0, supplementaryLine.FDA.Count);
			AssertEquals(1, classificationLine.DOT.Count());
			AssertEquals(1, supplementaryLine.DOT.Count());
		}

		public void TestLaceyActDetailsWith98()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var rule = Factory.LoadTop1<USCRule>(new ZQuery(USCRuleSchema.U0_Code, TariffRuleList.Codes.LaceyAct));
			if (rule == null)
			{
				rule = Factory.New<USCRule>();
				rule.U0_Code = TariffRuleList.Codes.LaceyAct;
			}

			var tariff_rule = Factory.New<USCTariffRule>();
			tariff_rule.U1_RuleCode = TariffRuleList.Codes.LaceyAct;
			tariff_rule.U1_Tariff = "4412947000";
			tariff_rule.U1_TariffTo = "44129951";
			tariff_rule.U1_DateFrom = new ZDate(2000, 1, 1);

			var tariff = new USCTariff.Loader(Factory).LoadBestMatch("4412947000", ZDateTime.Today);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "4412947000";
				tariff.UE_DateFrom = new ZDate(2000, 1, 1);
			}
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.LaceyActLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var tariffLine1 = (IGovernmentAgencies)invoiceLine.CusEntryLine;
			AssertEquals(1, tariffLine1.LaceyActData.Count());

			invoiceLine.US_SupTariff = "9801008000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IGovernmentAgencies classificationLine = invoiceLine.CusEntryLine;
			IGovernmentAgencies supplementaryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(1, classificationLine.LaceyActData.Count());
			AssertEquals(0, supplementaryLine.LaceyActData.Count());

			//ACE Declaration
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var tariffWithLacyActMayRequired = Factory.New<USCTariff>();
			tariffWithLacyActMayRequired.UE_Tariff = "1000000001";
			tariffWithLacyActMayRequired.UE_DateFrom = ZDateTime.Today.AddYears(-2);
			tariffWithLacyActMayRequired.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariffWithLacyActMayRequired.UE_PGACodes = "AL1";//may be required

			var tariffWithLacyActRequired = Factory.New<USCTariff>();
			tariffWithLacyActRequired.UE_Tariff = "1000000002";
			tariffWithLacyActRequired.UE_DateFrom = ZDateTime.Today.AddYears(-2);
			tariffWithLacyActRequired.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariffWithLacyActRequired.UE_PGACodes = "AL2";//required

			invoiceLine.JI_Tariff = tariffWithLacyActMayRequired.UE_Tariff;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			tariffLine1 = invoiceLine.CusEntryLine;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.LaceyActIndicator);

			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			tariffLine1 = invoiceLine.CusEntryLine;
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine1.LaceyActIndicator);
			AssertEquals(1, tariffLine1.LaceyActData.Count());

			// with Sup Tariff
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_Tariff = tariffWithLacyActMayRequired.UE_Tariff;
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802005060";
			invoiceLine2.JI_Tariff = tariffWithLacyActRequired.UE_Tariff;
			invoiceLine2.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine2.LaceyActLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine1 = (IGovernmentAgencies)invoiceLine.GetEntryLineFor("ENS", true);
			tariffLine1 = invoiceLine.CusEntryLine;

			var supLine2 = (IGovernmentAgencies)invoiceLine2.GetEntryLineFor("ENS", true);
			var tariffLine2 = (IGovernmentAgencies)invoiceLine2.CusEntryLine;

			AssertEquals(ZString.Empty, supLine1.LaceyActIndicator);
			AssertEquals(0, supLine1.LaceyActData.Count());
			AssertEquals(ZString.Empty, supLine2.LaceyActIndicator);
			AssertEquals(0, supLine1.LaceyActData.Count());

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, tariffLine1.LaceyActIndicator);
			AssertEquals(OGAIndicatorList.Codes.Declared, tariffLine2.LaceyActIndicator);
			AssertEquals(1, tariffLine2.LaceyActData.Count());
		}

		[TestDate(2007, 1, 1)]
		public void TestIOGAWithSupLineForFCC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801008000";//FD3
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "8528123228";//FC4FD2
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine.FCCs.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IOGA classificationLine = invoiceLine.CusEntryLine;
			IOGA supplementaryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, classificationLine.FDAIndicator);
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, supplementaryLine.FDAIndicator);
			AssertEquals(ZString.Empty, classificationLine.DOTIndicator);
			AssertEquals(ZString.Empty, supplementaryLine.DOTIndicator);

			AssertEquals(0, classificationLine.FDA.Count);
			AssertEquals(0, supplementaryLine.FDA.Count);
			AssertEquals(false, classificationLine.DOT.Any());
			AssertEquals(false, supplementaryLine.DOT.Any());
		}

		[TestDate(2006, 1, 1)]
		public void TestFDAReportingForSupLineWhenNoneInvolved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9904.17.45";// no FDA requirement
			invoiceLine.JI_Tariff = "1901.90.5400";// FD4

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			ICusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals("If this is declared again, Customs rejects FDA value should be less than Line Customs Value", ZString.Empty, supplementaryTariff.FDAIndicator);
			AssertEquals(OGAIndicatorList.Codes.Declared, classificationTariff.FDAIndicator);

			invoiceLine.US_SupTariff = "9810.00.8500";// FD2
			invoiceLine.JI_Tariff = "3912.12.0000";// No FDA requirement

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(OGAIndicatorList.Codes.Declared, supplementaryTariff.FDAIndicator);
			AssertEquals("If this is declared again, Customs rejects FDA value should be less than Line Customs Value", ZString.Empty, classificationTariff.FDAIndicator);
		}

		[TestDate(2006, 1, 1)]
		public void TestFDAReportingForSupLineWhenNoneInvolvedWithDisclaim()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9904.17.45";// no FDA requirement
			invoiceLine.JI_Tariff = "1901.90.5400";// FD4

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			ICusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(ZString.Empty, supplementaryTariff.FDAIndicator);
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, classificationTariff.FDAIndicator);

			invoiceLine.US_SupTariff = "9810.00.8500";// FD2
			invoiceLine.JI_Tariff = "3912.12.0000";// No FDA requirement
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, supplementaryTariff.FDAIndicator);
			AssertEquals(ZString.Empty, classificationTariff.FDAIndicator);
		}

		public void TestFTZWarehousePackageQtyAndUnit()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "O@11";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Tes1@#";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";
			product.OP_StockKeepingUnit = "CS";
			product.OP_NetWeight = 0.1m;
			product.OP_WeightUQ = "T";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.FTZAdmissionNumber = "2140000|14|00000002";
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.US_SchDEntry = "3901";
			declaration.JE_ExportDate = new ZDateTime(2021, 2, 28);
			declaration.JE_DateOfArrival = new ZDateTime(2021, 3, 1);
			declaration.US_EntryDate = new ZDateTime(2021, 3, 2);
			declaration.JE_PrimaryITNumber = "IT1";
			declaration.US_ITDate = new ZDateTime(2021, 3, 14);
			declaration.JE_MasterBillIssuerSCAC = "SC1";
			declaration.JE_MasterBill = "MB2";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2021, 3, 15);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OP = product.PK;
			invoiceLine.JI_BondedWhsQuantity = 2m;
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_InvoiceUQ = "NO";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("Loaded from Product.OP_StockKeepingUnit", "CS", invoiceLine.PartStockTakeUnit);
			AssertEquals("FTZWarehousePackageQtyAndUnit", "2 NO", invoiceLine.CusEntryLine.FTZWarehousePackageQtyAndUnit);
			invoiceLine.JI_BondedWhsQuantity = 0m;
			Factory.Save();
			AssertEquals("FTZWarehousePackageQtyAndUnit is empty when Qty is 0.", "", invoiceLine.CusEntryLine.FTZWarehousePackageQtyAndUnit);
		}

		[TestDate(2008, 1, 1)]
		public void TestSetCustomsValueForSup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6204624021";
			invoiceLine.US_SupTariff = "98191124";// Value should be declared at its classification tariff level
			invoiceLine.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.US_VisaNo = "8ZA121221";
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_CustomsSecondQuantity = 2500m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(0m, supplementaryTariff.CL_CustomsValue);
			AssertEquals(0m, supplementaryTariff.CustomsQuantity);
			AssertEquals("", supplementaryTariff.CustomsUnitQty);

			AssertEquals(10000m, classificationTariff.CL_CustomsValue);
			AssertEquals(200m, classificationTariff.CustomsQuantity);
			AssertEquals("DOZ", classificationTariff.CustomsUnitQty);

			AssertEquals(2500m, classificationTariff.SecondCustomsQuantity);
			AssertEquals("KG", classificationTariff.SecondCustomsUnitQty);
		}

		public void TestCS00137507_HasMPFIsSetCorrectlyEvenIfCustomsValueIsZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20.51m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 0m;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 50.61m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("invoiceLine", 20m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals("invoiceLine2", 0m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals("invoiceLine3", 51m, invoiceLine3.CusEntryLine.CL_CustomsValue);

			Assert(invoiceLine.CusEntryLine.US_HasMPF);
			Assert(invoiceLine2.CusEntryLine.US_HasMPF);
			Assert(invoiceLine3.CusEntryLine.US_HasMPF);
		}

		public void TestInvoiceLinesCollectionForSup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6204624021";
			invoiceLine.US_SupTariff = "98191124";// Value should be declared at its classification tariff level
			invoiceLine.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.US_VisaNo = "8ZA121221";
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_CustomsSecondQuantity = 2500m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(1, entry.InvoiceLines.Count());
		}

		[TestDate(2008, 1, 1)]
		public void TestSetCustomsValueForSup9802()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 2300m;

			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_Weight = 2;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(2300m, supplementaryTariff.CL_CustomsValue);
			AssertEquals(20000m, classificationTariff.CL_CustomsValue);
		}

		[TestDate(2025, 4, 24)]
		public void TestCustomsValueForSup9802WithAdditionalSupTariff()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9802004040", "X", 0, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030125", "7", 0.1m, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296010", "1", 0m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296020", "7", 0.048m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296030", "7", 0.022m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030125 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030125", new ZDateTime(2025, 04, 05), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF ANY COUNTRY", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030125);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.JI_FormattedTariff = "9102.29.6010";
			parentLine.SupTariffFormatted = "9903.01.25";
			parentLine.SupFormattedAdditionalTariff1 = "9802.00.4040";
			parentLine.JI_LinePrice = 5000m;
			parentLine.JI_CustomsQuantity = 100m;
			parentLine.US_98GoodsValue = 3000m;

			var childLine1 = parentLine.ChildLines.ElementAt(0);
			childLine1.JI_FormattedTariff = "9102.29.6020";
			childLine1.JI_LinePrice = 2000m;
			var childLine2 = parentLine.ChildLines.ElementAt(1);
			childLine2.JI_FormattedTariff = "9102.29.6030";
			childLine2.JI_LinePrice = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions("Customs Value for each tariff", () =>
			{
				var entry = declaration.FormalEntry;
				AssertEquals("Customs Value for tariff 9802004040", 3000m, entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9802004040").FirstOrDefault().CL_CustomsValue);
				AssertEquals("Customs Value for tariff 99030125", 0m, entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99030125").FirstOrDefault().CL_CustomsValue);
				AssertEquals("Customs Value for tariff 9102296010", 5000m, entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9102296010").FirstOrDefault().CL_CustomsValue);
				AssertEquals("Customs Value for tariff 9102296020", 2000m, entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9102296020").FirstOrDefault().CL_CustomsValue);
				AssertEquals("Customs Value for tariff 9102296030", 1000m, entry.MergedLines.Find(x => x.CL_AdValoremTariff == "9102296030").FirstOrDefault().CL_CustomsValue);
			});
		}

		public void TestCustomsValueForMPFWithSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 0.01m;

			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 0.25m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_Weight = 2;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			Assert(!supplementaryTariff.US_HasMPF);
			Assert(classificationTariff.US_HasMPF);

			AssertEquals(2m, ((ICusEntryLine)supplementaryTariff).GrossWeightInKilograms);
			AssertEquals(0m, ((ICusEntryLine)classificationTariff).GrossWeightInKilograms);
		}

		[TestDate(2008, 1, 1)]
		public void TestSetCustomsValueForSup982205()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.US_SupTariff = "98220510";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 2300m;

			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_Weight = 2;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(2300m, supplementaryTariff.CL_CustomsValue);
			AssertEquals(20000m, classificationTariff.CL_CustomsValue);
		}

		[TestDate(2008, 12, 1)]
		public void TestSupplementaryTariffQuantityFields()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99010052"; // 5.99 cents/litre extra

			AssertEquals("99010052", invoiceLine.US_SupTariff);
			AssertEquals("L", invoiceLine.US_SupUQ1);

			invoiceLine.US_SupQty1 = 5000m;
			AssertEquals(5000m, invoiceLine.US_SupQty1);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, invoiceLine.CusEntryLine.ParentLine.InvoiceLines.Count);

			CusEntryLine entryLine = invoiceLine.CusEntryLine.ParentLine;
			AssertEquals(5000m, entryLine.CustomsQuantity);
			AssertEquals("L", entryLine.CustomsUnitQty);
		}

		public void TestChargesWithSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4201.00.60 00";
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_98GoodsValue = 2300m;

			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_Weight = 2;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(50m, supplementaryTariff.Charges);
			AssertEquals(0m, classificationTariff.Charges);
		}

		[TestDate(2009, 12, 12)]
		public void TestGetRelevantTariffForFeeType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98191124";
			invoiceLine.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.US_VisaNo = "8ZA121221";
			invoiceLine.JI_Tariff = "6204624021";
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_CustomsSecondQuantity = 2500m;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			AssertEquals("6204624021", entryLine.GetRelevantTariffForFee(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		public void TestGetRelevantTariffForEnsemble()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			invoiceLine.JI_CustomsSecondQuantity = 2500m;
			Assert("PreCondition", invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6104622028";
			secondaryLine.JI_CustomsQuantity = 200m;
			secondaryLine.JI_CustomsSecondQuantity = 2500m;
			Assert("PreCondition", secondaryLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			AssertEquals("6104220040", entryLine.GetRelevantTariffForFee(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		public void TestAccessInvoiceLineFromCusEntryLineWhenItIsDeletedWhenValidating()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = invoiceLine.CusEntryLine;

			invoiceLine.Delete();

			AssertNoExceptionThrown(delegate
			{ var accessed = entryLine.RandomLine.JI_LinePrice; });
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "0804.40.0010";//AVOCADOS - Required

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			List<ZString> requiredFees = new List<ZString>(invoiceLine.CusEntryLine.GetRequiredFees());
			AssertEquals(1, requiredFees.Count);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Avocado, requiredFees[0]);
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodesDoesNotReturnFeesWhenTIBEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "0804.40.0010"; //Avocado fee tariff

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.US_SupTariff = "98130030";
			invoiceLine2.JI_InvoiceQuantity = 1000m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 24055m;
			invoiceLine2.JI_Tariff = "0201300600"; // Beef fee tariff
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			List<ZString> requiredFees = new List<ZString>(invoiceLine.CusEntryLine.GetRequiredFees());
			AssertEquals("Should be no Required Fees on either line for this TIB job", 0, requiredFees.Count);
			requiredFees = new List<ZString>(invoiceLine2.CusEntryLine.GetRequiredFees());
			AssertEquals("Should be no Required Fees on either line for this TIB job", 0, requiredFees.Count);
		}

		public void TestIAESTIRCommodityLineItemMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			invoiceLine1.JI_Description = "DESCRIPTION 1";
			invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine1.US_LicenseNo = "LCN1234";
			invoiceLine1.JI_Weight = 145.25m;
			invoiceLine1.JI_CustomsQuantity = 24m;
			invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.JI_CustomsSecondQuantity = 46.3m;
			invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
			invoiceLine1.US_ECCN = "EC12";
			invoiceLine1.JI_LinePrice = 1500.40m;
			invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			invoiceLine1.US_DDTCITARExemptionNo = "123.11B";
			invoiceLine1.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCQuantity = 142m;
			invoiceLine1.US_DDTCRegistrationNo = "REG123";
			invoiceLine1.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.BulletsRounds;
			invoiceLine1.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.ClassifiedArticlesTechnicalDataAndDefenseServicesNotOtherwiseEnumerated;
			invoiceLine1.US_LicenseValue = 99.99m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2020202020";
			invoiceLine2.JI_Description = "DESCRIPTION 2";
			invoiceLine2.US_ExportCode = ExportInformationCodeList.Codes.CH;
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine2.US_LicenseNo = "LCN5678";
			invoiceLine2.JI_Weight = 250m;
			invoiceLine2.JI_CustomsQuantity = 30m;
			invoiceLine2.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Packs;
			invoiceLine2.JI_CustomsSecondQuantity = 60.75m;
			invoiceLine2.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pieces;
			invoiceLine2.US_ECCN = "EC34";
			invoiceLine2.JI_LinePrice = 2600m;
			invoiceLine2.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;
			invoiceLine2.US_IsUsedVehicle = true;
			invoiceLine2.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			invoiceLine2.US_VehicleID = "VID12";
			invoiceLine2.US_VehicleTitleNo = "VTITLENO";
			invoiceLine2.US_VehicleTitleState = "MA";
			invoiceLine2.US_LicenseValue = 100.01;
			invoiceLine2.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_EPAConsentNumber = "100900056";
			invoiceLine2.US_HazWasteTrackingNo = "975123456";
			invoiceLine2.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.US_ExportCertificateNo = "123456780";
			invoiceLine2.US_JurisdictionNumber = "CJ1234567";

			invoiceLine2.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			var exportNMFSLine = invoiceLine2.NMFSLines.AddNew();
			exportNMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			exportNMFSLine.US_IFTPPermitNumber = "SE52021";

			invoiceLine2.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.ExportATF.US_FFLNumber = "1111";
			invoiceLine2.ExportATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._1;

			invoiceLine2.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var exportDEALine = invoiceLine2.DEAHeaders.AddNew();
			exportDEALine.US_DrugCode = "DRUG";
			exportDEALine.US_PermitNumber = "1234567";

			invoiceLine2.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine2.ExportFWS.US_ConfirmationNum = "TEST ABC";

			invoiceLine2.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var exportTTBLine = invoiceLine2.TTBLines.AddNew();
			exportTTBLine.US_NumberForIRC = "12-345-67-12345";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.MergedLines.Count);
			AssertCommodityLine(invoiceLine1, entry.MergedLines[0]);
			AssertCommodityLine(invoiceLine2, entry.MergedLines[1]);
			AssertUsedVehicle(invoiceLine1, entry.MergedLines[0]);
			AssertUsedVehicle(invoiceLine2, entry.MergedLines[1]);

			var entryLine2 = entry.MergedLines[1];
			var commodityLineItem = (IAESTIRCommodityLineItem)entryLine2;
			var exportEPA = commodityLineItem.ExportEPA;
			AssertEquals(OGAIndicatorList.Codes.Declared, commodityLineItem.EPAIndicator);
			AssertEquals("100900056", exportEPA.EPAConsentNumber);
			AssertEquals("975123456", exportEPA.HazWasteManifestTrackingNumber);

			var exportAMS = commodityLineItem.ExportAMS;
			AssertEquals(OGAIndicatorList.Codes.Declared, commodityLineItem.AMSIndicator);
			AssertEquals("123456780", exportAMS.ExportCertificateNo);

			var exportNMFS = commodityLineItem.ExportNMFSLines.FirstOrDefault();
			AssertEquals(OGAIndicatorList.Codes.Declared, commodityLineItem.NMFSIndicator);
			AssertEquals("HMS", exportNMFS.ProgramCode);
			AssertEquals("SE52021", exportNMFS.PermitNumber);

			var exportATF = commodityLineItem.ExportATF;
			AssertEquals(OGAIndicatorList.Codes.Declared, commodityLineItem.ATFIndicator);
			AssertEquals("1111", exportATF.FFLNumber);
			AssertEquals(ExemptionCodesCodeList.Codes._1, exportATF.PermitExemptionCode);

			var exportDEA = commodityLineItem.ExportDEALines.FirstOrDefault();
			AssertEquals(OGAIndicatorList.Codes.Declared, commodityLineItem.DEAIndicator);
			AssertEquals("DRUG", exportDEA.DrugCode);
			AssertEquals("1234567", exportDEA.PermitNumber);

			var exportFWS = commodityLineItem.ExportFWS;
			AssertEquals(OGAIndicatorList.Codes.Declared, commodityLineItem.FWSIndicator);
			AssertEquals("TEST ABC", exportFWS.EDecsConfirmation);

			var exportTTB = commodityLineItem.ExportTTBLines.FirstOrDefault();
			AssertEquals(OGAIndicatorList.Codes.Declared, commodityLineItem.TTBIndicator);
			AssertEquals("12-345-67-12345", exportTTB.IRCNumber);
		}

		public void TestIAESTIRCommodityLineItemMembersRounding()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			invoiceLine1.JI_Description = "DESCRIPTION 1";
			invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine1.US_LicenseNo = "LCN1234";
			invoiceLine1.JI_Weight = 0.25m;
			invoiceLine1.JI_CustomsQuantity = 24m;
			invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.JI_CustomsSecondQuantity = 0.3m;
			invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
			invoiceLine1.US_ECCN = "EC12";
			invoiceLine1.JI_LinePrice = 0.40m;
			invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			invoiceLine1.US_DDTCITARExemptionNo = "123.11B";
			invoiceLine1.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCQuantity = .74m;
			invoiceLine1.US_DDTCRegistrationNo = "REG123";
			invoiceLine1.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.BulletsRounds;
			invoiceLine1.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.ClassifiedArticlesTechnicalDataAndDefenseServicesNotOtherwiseEnumerated;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entry.MergedLines.Count);
			AssertCommodityLine(invoiceLine1, entry.MergedLines[0]);
			AssertUsedVehicle(invoiceLine1, entry.MergedLines[0]);
		}

		public void TestNumbersAreRoundedForAESTIR()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine1.US_LicenseNo = "LCN1234";
			invoiceLine1.JI_Weight = 145.25m;
			invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.JI_CustomsQuantity = 24.9m;
			invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
			invoiceLine1.JI_CustomsSecondQuantity = 46.3m;
			invoiceLine1.US_ECCN = "EC12";
			invoiceLine1.JI_LinePrice = 1498.90m;
			invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];

			IAESTIRCommodityLineItem lineItem = entry.MergedLines[0];
			AssertEquals("Quantity1 - ensure value is rounded", 145m, lineItem.ShippingWeight);
			AssertEquals("Quantity1 - ensure value is rounded", 1499m, lineItem.ValueOfGoods);
			AssertEquals("Quantity1 - ensure value is rounded", 25m, lineItem.Quantity1);
			AssertEquals("Quantity2 - ensure value is rounded", 46m, lineItem.Quantity2);
		}

		public void TestDDTCQuantityMergedLineValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();

			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "8544420000";
			line1.US_UC_NKCountryOfOrigin = "TW";
			line1.US_UC_NKCountryOfExport = "TW";
			line1.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			line1.US_DDTCRegistrationNo = "MI0096";
			line1.US_DDTCQuantity = 142m;
			line1.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.Items;
			line1.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.MilitaryElectronics;

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "8544420000";
			line2.US_UC_NKCountryOfOrigin = "TW";
			line2.US_UC_NKCountryOfExport = "TW";
			line2.US_DDTCMilitaryEquipmentIndicator = line1.US_DDTCMilitaryEquipmentIndicator;
			line2.US_DDTCRegistrationNo = line1.US_DDTCRegistrationNo;
			line2.US_DDTCQuantity = 35m;
			line2.US_DDTCUnit = line1.US_DDTCUnit;
			line2.US_DDTCUSMLCategoryCode = line1.US_DDTCUSMLCategoryCode;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("Pre-condition - lines should have merged", 1, entry.MergedLines.Count);

			var mergedLine = entry.MergedLines[0];
			AssertEquals("DDTCRegistrationNumber", "MI0096", mergedLine.DDTCRegistrationNumber);
			AssertEquals("DDTCUSMLCategoryCode", USMLCategoryCodes.Codes.MilitaryElectronics, mergedLine.DDTCUSMLCategoryCode);
			AssertEquals("DDTCUnitOfMeasureCode", DDTCUnitOfMeasureList.Codes.Items, mergedLine.DDTCUnitOfMeasure);
			AssertEquals("DDTCQuantity should be accumulation of both line quantities", 177m, mergedLine.DDTCQuantity);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), entryLine.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), entryLine.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), entryLine.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), entryLine.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), entryLine.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), entryLine.RegistryBranchPK);

			entry.CH_JE = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), entryLine.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), entryLine.RegistryBranchPK);
		}

		public void TestValueForADD_CVDForDerivedDutyCalc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211929045"; // .4c per unit + 6.1% = 20000 *.004 + 24000 * .061 = $1544
			line2.JI_CustomsQuantity = 16000m;
			line2.JI_LinePrice = 16000m;

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (20000 *.03) + (24000 * .054) = $1896
			line3.JI_CustomsQuantity = 4000m;
			line3.JI_LinePrice = 8000m;
			line3.US_ADDCaseNo = "A7439";
			line3.US_ADDDepositValue = 8000.50m;
			line3.US_CVDCaseNo = "C7439";
			line3.US_CVDDepositValue = 8002.40m;
			line3.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition", line2.CusEntryLine, line3.CusEntryLine);
			AssertEquals(8001m, line3.CusEntryLine.ValueForADD);
			AssertEquals(8002m, line3.CusEntryLine.ValueForCVD);

			IDutyData iDutyData = line3.CusEntryLine;
			AssertEquals(8001m, iDutyData.ValueForADD);
			AssertEquals(8002m, iDutyData.ValueForCVD);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			line3.US_ADDQty = 451m;
			line3.US_CVDQty = 12m;
			line3.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			line3.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			iDutyData = line3.CusEntryLine;
			AssertEquals("ValueForADD", 8001m, iDutyData.ValueForADD);
			AssertEquals("ValueForCVD", 8002m, iDutyData.ValueForCVD);
			AssertEquals("ADDQuantity", 451m, iDutyData.ADDQuantity);
			AssertEquals("If the US_CVDDepositRateIndicator is not 'Specific' CVDQuantity should set 0", 0m, iDutyData.CVDQuantity);
			AssertEquals("ADDCaseRateTypeQualifier", DepositRateIndicatorList.Codes.AdValorem, iDutyData.ADDCaseRateTypeQualifier);
			AssertEquals("CVDCaseRateTypeQualifier", DepositRateIndicatorList.Codes.AdValorem, iDutyData.CVDCaseRateTypeQualifier);
		}

		public void TestADD_CVDManualForDerivedDutyCalc()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;

			var line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211929045"; // .4c per unit + 6.1% = 20000 *.004 + 24000 * .061 = $1544
			line2.JI_CustomsQuantity = 16000m;
			line2.JI_LinePrice = 16000m;

			var line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (20000 *.03) + (24000 * .054) = $1896
			line3.JI_CustomsQuantity = 4000m;
			line3.JI_LinePrice = 8000m;
			line3.US_ADDCaseNo = "A7439";
			line3.US_CVDCaseNo = "C7439";
			line3.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			line3.US_ADDuty = 100m;
			line3.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			line3.US_CVDuty = 200m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition", line2.CusEntryLine, line3.CusEntryLine);

			var iDutyData = (IDutyData)line3.CusEntryLine;
			AssertEquals("AD Duty", 100m, iDutyData.ADDutyManual);
			AssertEquals("CV Duty", 200m, iDutyData.CVDutyManual);
		}

		public void TestValueForADD_CVDFor99Or98Parent()
		{
			USCACCase addCase1 = Factory.New<USCACCase>();
			addCase1.U5_CaseNumber = "A462105011";
			addCase1.U5_ISOCountryCode = "RU";
			addCase1.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			var rate1 = addCase1.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.01m;
			rate1.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802004040";
			AssertNotNull(invoiceLine.ImportSupTariff);

			invoiceLine.US_98GoodsValue = 250m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_ADDCaseNo = "A462105011";
			invoiceLine.US_CVDCaseNo = "";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1000m, invoiceLine.CusEntryLine.CL_CustomsValue);
			AssertEquals(250m, invoiceLine.CusEntryLine.ParentLine.CL_CustomsValue);

			AssertEquals("ValueForADD", 0m, invoiceLine.CusEntryLine.ValueForADD);
			AssertEquals("ValueForCVD", 0m, invoiceLine.CusEntryLine.ValueForCVD);

			IDutyData supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			IDutyData normalLine = invoiceLine.CusEntryLine;

			AssertEquals("ValueForADD", 1250m, supLine.ValueForADD);
			AssertEquals("ValueForCVD", 0m, supLine.ValueForCVD);
			AssertEquals("ValueForADD", 0m, normalLine.ValueForADD);
			AssertEquals("ValueForCVD", 0m, normalLine.ValueForCVD);

			AssertEquals("ADDDepositRate", 1.01m, supLine.ADDDepositRate);
			AssertEquals("ADDDepositRate", 0m, normalLine.ADDDepositRate);
		}

		public void TestADD_CVDManualFor99Or98Parent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9817009040";
			invoiceLine.US_98GoodsValue = 250m;
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_ADDCaseNo = "A462105011";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDuty = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IDutyData supLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			IDutyData normalLine = invoiceLine.CusEntryLine;

			AssertEquals(100m, supLine.ADDutyManual);
			AssertNull(normalLine.ADDutyManual);

			AssertEquals("Total AD duty", 100m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty);
		}

		public void TestValueForADD_CVDForWatchRepair()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceHeader.JZ_InvoiceAmount = 3406m;
			JobComInvoiceLine movementTariff = invoiceHeader.JobComInvoiceLines.AddNew();
			movementTariff.US_SupTariff = "9802004040"; // repairs
			movementTariff.US_98GoodsValue = 3406m;
			movementTariff.JI_Tariff = "9102111010";
			movementTariff.JI_LinePrice = 5258m;
			movementTariff.JI_CustomsQuantity = 1000m;
			movementTariff.US_ADDCaseNo = "A123456";
			movementTariff.US_CVDCaseNo = "C123456";

			JobComInvoiceLine casesTariff = movementTariff.AddSecondaryInvoiceLine();
			casesTariff.US_SupTariff = "9802004040";    // repairs
			casesTariff.JI_Tariff = "9102111020";
			casesTariff.JI_LinePrice = 2619m;
			casesTariff.JI_CustomsQuantity = 1000m;

			JobComInvoiceLine braceletsTariff = movementTariff.AddSecondaryInvoiceLine();
			braceletsTariff.US_SupTariff = "9802004040";    // repairs
			braceletsTariff.JI_Tariff = "9102111030";
			braceletsTariff.JI_LinePrice = 1344m;
			braceletsTariff.JI_CustomsQuantity = 1000m;
			braceletsTariff.US_ADDCaseNo = "A123456";
			braceletsTariff.US_CVDCaseNo = "C123456";

			JobComInvoiceLine batteriesTariff = movementTariff.AddSecondaryInvoiceLine();
			batteriesTariff.US_SupTariff = "9802004040";    // repairs
			batteriesTariff.JI_Tariff = "9102111040";
			batteriesTariff.JI_LinePrice = 204m;
			batteriesTariff.JI_CustomsQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("ValueForADD for first repair", 0m, movementTariff.CusEntryLine.ValueForADD);
			AssertEquals("ValueForCVD for first repair", 0m, movementTariff.CusEntryLine.ValueForCVD);

			CusEntryLine movementTariffSupLine = movementTariff.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals("ValueForADD for first repair", 8664m, movementTariffSupLine.ValueForADD);
			AssertEquals("ValueForCVD for first repair", 8664m, movementTariffSupLine.ValueForCVD);

			AssertEquals("ValueForADD for third repair", 0m, braceletsTariff.CusEntryLine.ValueForADD);
			AssertEquals("ValueForCVD for third repair", 0m, braceletsTariff.CusEntryLine.ValueForCVD);

			CusEntryLine braceletsTariffSupLine = braceletsTariff.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals("ValueForADD for third repair", 1344m, braceletsTariffSupLine.ValueForADD);
			AssertEquals("ValueForCVD for third repair", 1344m, braceletsTariffSupLine.ValueForCVD);
		}

		public void TestValueForADD_CVDForWatchRepairWithSup()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceHeader.JZ_InvoiceAmount = 3406m;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802004040";    // repairs
			invoiceLine.US_98GoodsValue = 3406m;
			invoiceLine.JI_Tariff = "9102111010";
			invoiceLine.JI_LinePrice = 5258m;
			invoiceLine.JI_CustomsQuantity = 1000m;

			JobComInvoiceLine secondRepairLine = invoiceLine.AddSecondaryInvoiceLine();
			secondRepairLine.US_SupTariff = "9802004040";   // repairs
			secondRepairLine.JI_Tariff = "9102111020";
			secondRepairLine.JI_LinePrice = 2619m;
			secondRepairLine.JI_CustomsQuantity = 1000m;

			JobComInvoiceLine thirdRepairLine = invoiceLine.AddSecondaryInvoiceLine();
			thirdRepairLine.US_SupTariff = "9802004040";    // repairs
			thirdRepairLine.JI_Tariff = "9102111030";
			thirdRepairLine.JI_LinePrice = 1344m;
			thirdRepairLine.JI_CustomsQuantity = 1000m;

			JobComInvoiceLine fourthRepairLine = invoiceLine.AddSecondaryInvoiceLine();
			fourthRepairLine.US_SupTariff = "9802004040";   // repairs
			fourthRepairLine.JI_Tariff = "9102111040";
			fourthRepairLine.JI_LinePrice = 204m;
			fourthRepairLine.JI_CustomsQuantity = 1000m;

			invoiceLine.US_ADDCaseNo = "A123456";
			invoiceLine.US_CVDCaseNo = "C123456";
			thirdRepairLine.US_ADDCaseNo = "A123456";
			thirdRepairLine.US_CVDCaseNo = "C123456";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("ValueForADD for first repair", 0m, invoiceLine.CusEntryLine.ValueForADD);
			AssertEquals("ValueForCVD for first repair", 0m, invoiceLine.CusEntryLine.ValueForCVD);
			AssertNull(invoiceLine.CusEntryLine.GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_ADDCaseNo));
			AssertNull(invoiceLine.CusEntryLine.GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_CVDCaseNo));

			AssertEquals("ValueForADD for first repair", 8664m, invoiceLine.CusEntryLine.ParentLine.ValueForADD);
			AssertEquals("ValueForCVD for first repair", 8664m, invoiceLine.CusEntryLine.ParentLine.ValueForCVD);
			AssertNotNull(invoiceLine.CusEntryLine.ParentLine.GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_ADDCaseNo));
			AssertNotNull(invoiceLine.CusEntryLine.ParentLine.GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_CVDCaseNo));

			AssertEquals("ValueForADD for third repair", 0m, thirdRepairLine.CusEntryLine.ValueForADD);
			AssertEquals("ValueForCVD for third repair", 0m, thirdRepairLine.CusEntryLine.ValueForCVD);
			AssertNull(thirdRepairLine.CusEntryLine.GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_ADDCaseNo));
			AssertNull(thirdRepairLine.CusEntryLine.GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_CVDCaseNo));

			AssertEquals("ValueForADD for third repair", 1344m, thirdRepairLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).ValueForADD);
			AssertEquals("ValueForCVD for third repair", 1344m, thirdRepairLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).ValueForCVD);
			AssertNotNull(thirdRepairLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_ADDCaseNo));
			AssertNotNull(thirdRepairLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true).GetInvoiceLineWithADD_CVDDetails(JobComInvoiceLine.Schema.US_CVDCaseNo));
		}

		public void TestValueForADD_CVDForXAndV()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine xLine = declaration.InvoiceLines.AddNew();
			xLine.JI_LinePrice = 10000m;
			xLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine vLine = xLine.AddSecondaryInvoiceLine();
			vLine.JI_LinePrice = 5000m;
			vLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine.US_ADDCaseNo = "A123456";
			vLine.US_CVDCaseNo = "C123456";

			JobComInvoiceLine vLine2 = xLine.AddSecondaryInvoiceLine();
			vLine2.JI_LinePrice = 3000m;
			vLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine2.US_ADDCaseNo = "A123456";
			vLine2.US_CVDCaseNo = "C123456";

			JobComInvoiceLine vLine3 = xLine.AddSecondaryInvoiceLine();
			vLine3.JI_LinePrice = 2000m;
			vLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			vLine3.US_ADDCaseNo = "A123456";
			vLine3.US_CVDCaseNo = "C123456";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(5000m, vLine.CusEntryLine.ValueForADD);
			AssertEquals(5000m, vLine.CusEntryLine.ValueForCVD);

			AssertEquals(3000m, vLine2.CusEntryLine.ValueForADD);
			AssertEquals(3000m, vLine2.CusEntryLine.ValueForCVD);

			AssertEquals(2000m, vLine3.CusEntryLine.ValueForADD);
			AssertEquals(2000m, vLine3.CusEntryLine.ValueForCVD);
		}

		public void TestADDCVDDetailsWithSupLine()
		{
			USCACCase addCase1 = Factory.New<USCACCase>();
			addCase1.U5_CaseNumber = "A570832000";
			addCase1.U5_ISOCountryCode = "CN";
			addCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "81041100";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "81049000";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "81043000";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "38249019";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "98170090";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "81042000";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "81041900";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "38249011";
			var rate1 = addCase1.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.08m;
			rate1.U6_EffectiveDate = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8302.10.3000";
			invoiceLine.US_SupTariff = "9817.00.9080";
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_CustomsQuantity = 250m;

			invoiceLine.US_ADDCaseNo = "A570832000";
			invoiceLine.US_ADDQty = 4999m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("TotalAntidumpingDuty", 5400.00m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty);

			IDutyData normalLine = invoiceLine.CusEntryLine;
			IDutyData supLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals("ADD/CVD Value", 5000m, supLine.ValueForADD);
			AssertEquals("ADD/CVD Value", 0m, normalLine.ValueForADD);
			AssertEquals("ADD/CVD Value", 0m, supLine.ValueForCVD);
			AssertEquals("ADD/CVD Value", 0m, normalLine.ValueForCVD);

			AssertEquals("supLine.ADDQuantity", 4999m, supLine.ADDQuantity);
			AssertEquals("normalLine.ADDQuantity", 4999m, normalLine.ADDQuantity);
			AssertEquals("supLine.CVDQuantity", 0m, supLine.CVDQuantity);
			AssertEquals("normalLine.CVDQuantity", 0m, normalLine.CVDQuantity);

			AssertEquals("supLine.ADDDepositRate", 1.08m, supLine.ADDDepositRate);
			AssertEquals("normalLine.ADDDepositRate", 0m, normalLine.ADDDepositRate);
			AssertEquals("supLine.CVDDepositRate", 0m, supLine.CVDDepositRate);
			AssertEquals("normalLine.CVDDepositRate", 0m, normalLine.CVDDepositRate);
		}

		public void TestADDCVDDetailsForLineWithAdditionalSupTariffsOnNormalLine()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "4823690040", "7", 0m, "KG");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570164003";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "4823690040";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 2.6763m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C570165004";
			cvdCase.U5_ISOCountryCode = "CN";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "4823690040";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 3.1314m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "4823.69.0040";
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine.JI_LinePrice = 250m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_ADDCaseNo = "A570164003";
			invoiceLine.US_CVDCaseNo = "C570165004";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.FormalEntry;
			var entryLine4823690040 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "4823690040").FirstOrDefault();
			AssertEquals("ValueForADD for tariff 4823690040", 250m, entryLine4823690040.ValueForADD);
			AssertEquals("ValueForCVD for tariff 4823690040", 250m, entryLine4823690040.ValueForCVD);
			AssertEquals("Antidumping duty amount for tariff 4823690040", 669.08m, entryLine4823690040.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("Countervailing duty amount for tariff 4823690040", 782.85m, entryLine4823690040.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));

			var entryLine99030120 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99030120").FirstOrDefault();
			AssertEquals("ValueForADD for tariff 99030120", 0m, entryLine99030120.ValueForADD);
			AssertEquals("ValueForCVD for tariff 99030120", 0m, entryLine99030120.ValueForCVD);
			AssertEquals("Antidumping duty amount for tariff 99030120", 0m, entryLine99030120.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("Countervailing duty amount for tariff 99030120", 0m, entryLine99030120.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));

			var entryLine99038802 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038802").FirstOrDefault();
			AssertEquals("ValueForADD for tariff 99038802", 0m, entryLine99038802.ValueForADD);
			AssertEquals("ValueForCVD for tariff 99038802", 0m, entryLine99038802.ValueForCVD);
			AssertEquals("Antidumping duty amount for tariff 99038802", 0m, entryLine99038802.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("Countervailing duty amount for tariff 99038802", 0m, entryLine99038802.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));
		}

		public void TestADDCVDDetailsForDerivedSetsWithAdditionalTariffs()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, ZString.Empty);
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, ZString.Empty);
			var tariff8206000000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8206000000", "9", 1m, "PCS");
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8203204000", "7", 0.12m, "DOZ");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570164003";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "8206000000";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.8205m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C570165004";
			cvdCase.U5_ISOCountryCode = "CN";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "8206000000";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 3.1314m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_FormattedTariff = "8206.00.0000";
			invoiceLine1.SupTariffFormatted = "9903.01.20";
			invoiceLine1.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1.US_ADDCaseNo = "A570164003";
			invoiceLine1.US_CVDCaseNo = "C570165004";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_FormattedTariff = "8203.20.4000";
			invoiceLine2.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.FormalEntry;
			var entryLine8206000000 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "8206000000").FirstOrDefault();
			AssertEquals("ValueForADD for tariff 8206000000", 10000m, entryLine8206000000.ValueForADD);
			AssertEquals("ValueForCVD for tariff 8206000000", 10000m, entryLine8206000000.ValueForCVD);
			AssertEquals("Invoice Line for ADD details for tariff 8206000000", invoiceLine1.PK, entryLine8206000000.GetInvoiceLineWithADD_CVDDetails(USAddInfoSchema.Constants.US_ADDCaseNo).PK);
			AssertEquals("Invoice Line for CVD details for tariff 8206000000", invoiceLine1.PK, entryLine8206000000.GetInvoiceLineWithADD_CVDDetails(USAddInfoSchema.Constants.US_CVDCaseNo).PK);
			AssertEquals("Antidumping duty amount for tariff 8206000000", 8205m, entryLine8206000000.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("Countervailing duty amount for tariff 8206000000", 31314m, entryLine8206000000.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));

			var entryLine99030120 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99030120").FirstOrDefault();
			AssertEquals("Antidumping duty amount for tariff 99030120", 0m, entryLine99030120.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("Countervailing duty amount for tariff 99030120", 0m, entryLine99030120.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));

			var entryLine99038802 = (CusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038802").FirstOrDefault();
			AssertEquals("Antidumping duty amount for tariff 99038802", 0m, entryLine99038802.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty));
			AssertEquals("Countervailing duty amount for tariff 99038802", 0m, entryLine99038802.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty));
		}

		public void TestIsDutyOverriden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_OverrideDuty = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.US_OverrideDuty = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(false, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.CusEntryLine.US_SupLine = true;

			invoiceLine.CusEntryLine.US_SupAdditionalLine = true;
			invoiceLine.US_OverrideSupAdditionalTariff1Duty = true;
			AssertEquals(true, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.US_OverrideSupAdditionalTariff1Duty = false;
			AssertEquals(false, invoiceLine.CusEntryLine.IsDutyOverriden);
			invoiceLine.CusEntryLine.US_SupAdditionalLine = false;

			invoiceLine.CusEntryLine.US_SupAdditionalLine2 = true;
			invoiceLine.US_OverrideSupAdditionalTariff2Duty = true;
			AssertEquals(true, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.US_OverrideSupAdditionalTariff2Duty = false;
			AssertEquals(false, invoiceLine.CusEntryLine.IsDutyOverriden);
			invoiceLine.CusEntryLine.US_SupAdditionalLine2 = false;

			invoiceLine.CusEntryLine.US_SupAdditionalLine3 = true;
			invoiceLine.US_OverrideSupAdditionalTariff3Duty = true;
			AssertEquals(true, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.US_OverrideSupAdditionalTariff3Duty = false;
			AssertEquals(false, invoiceLine.CusEntryLine.IsDutyOverriden);
			invoiceLine.CusEntryLine.US_SupAdditionalLine3 = false;

			invoiceLine.CusEntryLine.US_SupAdditionalLine4 = true;
			invoiceLine.US_OverrideSupAdditionalTariff4Duty = true;
			AssertEquals(true, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.US_OverrideSupAdditionalTariff4Duty = false;
			AssertEquals(false, invoiceLine.CusEntryLine.IsDutyOverriden);
			invoiceLine.CusEntryLine.US_SupAdditionalLine4 = false;

			invoiceLine.CusEntryLine.US_SupAdditionalLine5 = true;
			invoiceLine.US_OverrideSupAdditionalTariff5Duty = true;
			AssertEquals(true, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.US_OverrideSupAdditionalTariff5Duty = false;
			AssertEquals(false, invoiceLine.CusEntryLine.IsDutyOverriden);
			invoiceLine.CusEntryLine.US_SupAdditionalLine5 = false;

			invoiceLine.US_OverrideSupDuty = true;
			AssertEquals(true, invoiceLine.CusEntryLine.IsDutyOverriden);

			invoiceLine.US_OverrideSupDuty = false;
			AssertEquals(false, invoiceLine.CusEntryLine.IsDutyOverriden);
		}

		[TestDate(2009, 6, 10)]
		public void TestADDIncludeSecondaryLines()
		{
			USCACCase addCase1 = Factory.New<USCACCase>();
			addCase1.U5_CaseNumber = "A570204006";
			addCase1.U5_ISOCountryCode = "CN";
			addCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "8205595510";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "82014060";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "8465960015";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "8205203000";
			var rate1 = addCase1.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.89m;
			rate1.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "8974238";
			secondaryLine.JI_LinePrice = 15000m;
			secondaryLine.US_ADDCaseNo = "A570204006";
			secondaryLine.US_IsBondedADD = true;
			secondaryLine.US_ADDDepositValue = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine parentEntryLine = invoiceLine.CusEntryLine;
			AssertEquals("ADD including secondary lines", 18900.00m, parentEntryLine.AntidumpingDuty);
			AssertEquals("ADD case number", "A570204006", parentEntryLine.AntidumpingCaseNumber);
			AssertEquals("ADD depositRate", 1.89m, parentEntryLine.ADDDepositRate);
			AssertEquals(true, ((ICusEntryLine)parentEntryLine).BondedAntidumpingDuty);
			AssertEquals("ADD deposit value", 10000m, parentEntryLine.ADDSpecificDepositValue);

			secondaryLine.US_ADDCaseNo = "";
			secondaryLine.US_IsBondedADD = false;
			secondaryLine.US_ADDDepositValue = 0m;
			invoiceLine.US_ADDCaseNo = "A570204006";
			invoiceLine.US_IsBondedADD = true;
			invoiceLine.US_ADDDepositValue = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			parentEntryLine = invoiceLine.CusEntryLine;
			AssertEquals("ADD including secondary lines", 18900.00m, parentEntryLine.AntidumpingDuty);
			AssertEquals("ADD case number", "A570204006", parentEntryLine.AntidumpingCaseNumber);
			AssertEquals("ADD depositRate", 1.89m, parentEntryLine.ADDDepositRate);
			AssertEquals(true, ((ICusEntryLine)parentEntryLine).BondedAntidumpingDuty);
			AssertEquals("ADD deposit value", 10000m, parentEntryLine.ADDSpecificDepositValue);
		}

		[TestDate(2009, 6, 10)]
		public void TestCVDIncludeSecondaryLines()
		{
			USCACCase addCase1 = Factory.New<USCACCase>();
			addCase1.U5_CaseNumber = "C475819004";
			addCase1.U5_ISOCountryCode = "IT";
			addCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "1901909095";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "19021920";
			var rate1 = addCase1.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 0.03m;
			rate1.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "897234";
			secondaryLine.JI_LinePrice = 15000m;
			secondaryLine.US_CVDCaseNo = "C475819004";
			secondaryLine.US_IsBondedCVD = true;
			secondaryLine.US_CVDDepositValue = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine parentEntryLine = invoiceLine.CusEntryLine;

			AssertEquals("CVD including secondary lines", 300.00m, parentEntryLine.CountervailingDuty);
			AssertEquals("CVD case number", "C475819004", parentEntryLine.CountervailingCaseNumber);
			AssertEquals("CVD depositRate", 0.03m, parentEntryLine.CVDDepositRate);
			AssertEquals(true, ((ICusEntryLine)parentEntryLine).BondedCountervailingDuty);
			AssertEquals("CVD deposit value", 10000m, parentEntryLine.CVDSpecificDepositValue);

			secondaryLine.US_CVDCaseNo = "";
			secondaryLine.US_IsBondedCVD = false;
			secondaryLine.US_CVDDepositValue = 0m;
			invoiceLine.US_CVDCaseNo = "C475819004";
			invoiceLine.US_IsBondedCVD = true;
			invoiceLine.US_CVDDepositValue = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			parentEntryLine = invoiceLine.CusEntryLine;

			AssertEquals("CVD including secondary lines", 300.00m, parentEntryLine.CountervailingDuty);
			AssertEquals("CVD case number", "C475819004", parentEntryLine.CountervailingCaseNumber);
			AssertEquals("CVD depositRate", 0.03m, parentEntryLine.CVDDepositRate);
			AssertEquals(true, ((ICusEntryLine)parentEntryLine).BondedCountervailingDuty);
			AssertEquals("CVD deposit value", 10000m, parentEntryLine.CVDSpecificDepositValue);
		}

		public void TestADDSpecificDepositValueRounding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.US_ADDCaseNo = "A570204006";
			secondaryLine.US_IsBondedADD = true;
			secondaryLine.US_ADDDepositValue = 0.49m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var parentEntryLine = invoiceLine.CusEntryLine;
			AssertEquals("less than 1, set to 1", 1m, parentEntryLine.ADDSpecificDepositValue);

			secondaryLine.US_ADDDepositValue = 1.55m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			parentEntryLine = invoiceLine.CusEntryLine;
			AssertEquals("greater than or equal to 1, round down", 2m, parentEntryLine.ADDSpecificDepositValue);
		}

		public void TestCVDSpecificDepositValueRounding()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.US_CVDCaseNo = "C475819004";
			secondaryLine.US_IsBondedCVD = true;
			secondaryLine.US_CVDDepositValue = 0.49m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var parentEntryLine = invoiceLine.CusEntryLine;
			AssertEquals("less than 1, set to 1", 1m, parentEntryLine.CVDSpecificDepositValue);

			secondaryLine.US_CVDDepositValue = 1.55m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			parentEntryLine = invoiceLine.CusEntryLine;
			AssertEquals("greater than or equal to 1, round down", 2m, parentEntryLine.CVDSpecificDepositValue);
		}

		public void TestInvoiceLinesWhen7501IsEnabledAfterOneMergeHappened()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull("merged", invoiceLine.CusEntryLine);
			AssertEquals("0000", invoiceLine.CusEntryLine.CL_AdValoremTariff);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			var accessed = decLoaded.InvoiceLines[0].CusEntryLine.InvoiceLines;
			AssertNotNull(accessed);

			decLoaded.US_EnableENS = true;
			decLoaded.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("For ENS entry line", "0000", decLoaded.InvoiceLines[0].CusEntryLine.CL_AdValoremTariff);
			AssertEquals("For CRL entry line", "0000", decLoaded.InvoiceLines[0].GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false).CL_AdValoremTariff);
		}

		public void TestNoDutyRateExist()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6104622006";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_SPICode = "MA";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_AdValoremSpecialRate = 9999.99990000m;//this actually exists in the reference file
			dutyRate.UD_ISOCountryCode = "MA";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104622006";
			invoiceLine.JI_LinePrice = 250m;
			invoiceLine.US_UC_NKCountryOfOrigin = "MA";
			invoiceLine.US_SPI = "MA";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, invoiceLine.CusEntryLine.NoDutyRateExists);

			invoiceLine.US_SPI = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(false, invoiceLine.CusEntryLine.NoDutyRateExists);
		}

		public void TestIFDAEntryLine_()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine1.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine1.FDAs.AddNew();
			invoiceLine1.FDAs.AddNew();

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IFDAEntryLine entryLine1 = invoiceLine1.CusEntryLine;

			AssertEquals("entry line number", (short)1, entryLine1.EntryLineNo);
			AssertEquals("tariff number", USCTariff.FDAAdmissibilityReviewRequiredTariff, entryLine1.TariffNo);
			AssertEquals(OGAIndicatorList.Codes.Declared, entryLine1.FDAIndicator);
			AssertEquals("BTA lines", 2, new List<IPriorNoticeLine>(entryLine1.BTALines).Count);

			IFDAEntryLine entryLine2 = invoiceLine2.CusEntryLine;

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, entryLine2.FDAIndicator);
		}

		public void TestDDTCFields()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010203010";
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4407100115";

			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.SCA;
			invoiceLine1.US_DDTCITARExemptionNo = "ß126.3Ã"; // invalid character should be stripped
			invoiceLine1.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.No;
			invoiceLine1.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCQuantity = 324m;
			invoiceLine1.US_DDTCRegistrationNo = "DÃS4ß4"; // invalid character should be stripped
			invoiceLine1.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.Magazines;
			invoiceLine1.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.TanksAndMilitaryVehicles;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryLine entryLine1 = invoiceLine1.CusEntryLine;
			AssertEquals("DDTCITARExemptionNumber", "126.3", entryLine1.DDTCITARExemptionNumber);
			AssertEquals("DDTCRegistrationNumber", "DS44", entryLine1.DDTCRegistrationNumber);
			AssertEquals("DDTCSignificantMilitaryEquipmentIndicator", YesNoDefaultList.Codes.No, entryLine1.DDTCSignificantMilitaryEquipmentIndicator);
			AssertEquals("DDTCEligiblePartyCertificationIndicator", YesNoDefaultList.Codes.Yes, entryLine1.DDTCEligiblePartyCertificationIndicator);
			AssertEquals("DDTCUSMLCategoryCode", USMLCategoryCodes.Codes.TanksAndMilitaryVehicles, entryLine1.DDTCUSMLCategoryCode);
			AssertEquals("DDTCUnitOfMeasure", DDTCUnitOfMeasureList.Codes.Magazines, entryLine1.DDTCUnitOfMeasure);
			AssertEquals("DDTCQuantity", 324m, entryLine1.DDTCQuantity);
			CusEntryLine entryLine2 = invoiceLine2.CusEntryLine;
			AssertEquals("DDTCITARExemptionNumber", ZString.Empty, entryLine2.DDTCITARExemptionNumber);
			AssertEquals("DDTCRegistrationNumber", ZString.Empty, entryLine2.DDTCRegistrationNumber);
			AssertEquals("DDTCSignificantMilitaryEquipmentIndicator", ZString.Empty, entryLine2.DDTCSignificantMilitaryEquipmentIndicator);
			AssertEquals("DDTCEligiblePartyCertificationIndicator", ZString.Empty, entryLine2.DDTCEligiblePartyCertificationIndicator);
			AssertEquals("DDTCUSMLCategoryCode", ZString.Empty, entryLine2.DDTCUSMLCategoryCode);
			AssertEquals("DDTCUnitOfMeasure", ZString.Empty, entryLine2.DDTCUnitOfMeasure);
			AssertEquals("DDTCQuantity", ZDecimal.Zero, entryLine2.DDTCQuantity);
		}

		public void TestIsSoftwoodLumberSection804FarmBillRequirement()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010203010";
			JobComInvoiceLine invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "4407100115";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryLine entryLine1 = invoiceLine1.CusEntryLine;
			AssertEquals(true, entryLine1.IsSoftwoodLumberSection804FarmBillRequirement);
			CusEntryLine entryLine2 = invoiceLine2.CusEntryLine;
			AssertEquals(true, entryLine2.IsSoftwoodLumberSection804FarmBillRequirement);
		}

		public void TestChildSecondaryEntryLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine secondary1 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceLine secondary2 = invoiceLine1.AddSecondaryInvoiceLine();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Secondary entry lines", 2, new List<CusEntryLine>(invoiceLine1.CusEntryLine.ChildSecondaryEntryLines).Count);

			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, secondary1.US_SecondarySPI);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.V, secondary2.US_SecondarySPI);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("No secondary entry lines as it is X line", 0, new List<CusEntryLine>(invoiceLine1.CusEntryLine.ChildSecondaryEntryLines).Count);

			invoiceLine1.US_SecondarySPI = ZString.Empty;
			secondary1.US_SecondarySPI = ZString.Empty;
			secondary2.US_SecondarySPI = ZString.Empty;
			secondary1.JI_ParentID = ZGuid.Empty;
			secondary2.JI_ParentID = ZGuid.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("No secondary entry lines", 0, new List<CusEntryLine>(invoiceLine1.CusEntryLine.ChildSecondaryEntryLines).Count);
		}

		//Can happen for derived duty calculation
		public void TestDeleteEntryLineRefreshesChildLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine secondary1 = invoiceLine1.AddSecondaryInvoiceLine();
			JobComInvoiceLine secondary2 = invoiceLine1.AddSecondaryInvoiceLine();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("One header", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Secondary entry lines", 2, new List<CusEntryLine>(invoiceLine1.CusEntryLine.ChildSecondaryEntryLines).Count);

			secondary2.CusEntryLine.Delete();
			AssertEquals("Secondary entry lines should not have a deleted entry line", false, new List<CusEntryLine>(invoiceLine1.CusEntryLine.ChildSecondaryEntryLines).Contains(secondary2.CusEntryLine));
		}

		public void TestAdditionalIDutyData()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IDutyData dutyData = invoiceLine.CusEntryLine;
			AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, dutyData.EntryType);

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(false, dutyData.IsAMSFeeExempt);
			AssertEquals(true, dutyData.IsCottonFeeExemptIndicated);

			invoiceLine.US_CottonCertificateNo = "ORGANICXX";
			AssertEquals(true, dutyData.IsAMSFeeExempt);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals(true, invoiceLine.IsSetVLine);

			AssertNull(invoiceLine.ParentTariffLine);

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(invoiceLine.CusEntryLine, ((IDutyData)invoiceLine2.CusEntryLine).ParentTariffLine);
		}

		public void TestSecondarySPIWithSup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_SupTariff = "9802004040";
			invoiceLine.JI_Tariff = "4407100115";
			invoiceLine.JI_LinePrice = 204m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2208700030";
			invoiceLine2.JI_LinePrice = 22m;
			invoiceLine2.JI_CustomsQuantity = 20m;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("MergedLines", 3, entry.MergedLines.Count);

			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			var entryLine3 = entry.MergedLines[2];

			AssertEquals(SecondarySpecProgIndicatorList.Codes.F, ((ICusEntryLine)entryLine1).SpecialProgramsIndicatorSecondary);
			AssertEquals(SecondarySpecProgIndicatorList.Codes.F, ((ICusEntryLine)entryLine2).SpecialProgramsIndicatorSecondary);

			AssertEquals(SecondarySpecProgIndicatorList.Codes.C, ((ICusEntryLine)entryLine3).SpecialProgramsIndicatorSecondary);
		}

		public void TestSecondarySPIForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1902.19.4000";
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_LinePrice = 5000m;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "0712.31.1000";
			invoiceLine2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_LinePrice = 1300m;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "2002.90.8020";
			invoiceLine3.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.JI_LinePrice = 1300m;

			var invoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine4.JI_Tariff = "1902.19.4000";
			invoiceLine4.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine4.JI_LinePrice = 2400m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("MergedLines", 4, entry.MergedLines.Count);

			var entryLine1 = entry.MergedLines[0];
			var entryLine2 = entry.MergedLines[1];
			var entryLine3 = entry.MergedLines[2];
			var entryLine4 = entry.MergedLines[3];

			AssertEquals("entry line 1, SecondarySPI", ZString.Empty, ((ICusEntryLine)entryLine1).SpecialProgramsIndicatorSecondary);
			AssertEquals("entry line 2, SecondarySPI", ZString.Empty, ((ICusEntryLine)entryLine2).SpecialProgramsIndicatorSecondary);
			AssertEquals("entry line 3, SecondarySPI", ZString.Empty, ((ICusEntryLine)entryLine3).SpecialProgramsIndicatorSecondary);
			AssertEquals("entry line 4, SecondarySPI", ZString.Empty, ((ICusEntryLine)entryLine4).SpecialProgramsIndicatorSecondary);

			AssertEquals("entry line 1, SecondarySPI", ZString.Empty, ((IACECusEntryLine)entryLine1).SpecialProgramsIndicatorSecondary);
			AssertEquals("entry line 2, SecondarySPI", ZString.Empty, ((IACECusEntryLine)entryLine2).SpecialProgramsIndicatorSecondary);
			AssertEquals("entry line 3, SecondarySPI", ZString.Empty, ((IACECusEntryLine)entryLine3).SpecialProgramsIndicatorSecondary);
			AssertEquals("entry line 4, SecondarySPI", ZString.Empty, ((IACECusEntryLine)entryLine4).SpecialProgramsIndicatorSecondary);

			AssertEquals("entry line 1, ArticleSetIndicator", SecondarySpecProgIndicatorList.Codes.X, ((IACECusEntryLine)entryLine1).ArticleSetIndicator);
			AssertEquals("entry line 2, ArticleSetIndicator", SecondarySpecProgIndicatorList.Codes.V, ((IACECusEntryLine)entryLine2).ArticleSetIndicator);
			AssertEquals("entry line 3, ArticleSetIndicator", SecondarySpecProgIndicatorList.Codes.V, ((IACECusEntryLine)entryLine3).ArticleSetIndicator);
			AssertEquals("entry line 4, ArticleSetIndicator", SecondarySpecProgIndicatorList.Codes.V, ((IACECusEntryLine)entryLine4).ArticleSetIndicator);
		}

		public void TestLicenceTypeAndNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine01 = invoiceHeader.InvoiceLines.AddNew();

			var invoiceLine02 = invoiceLine01.AddSecondaryInvoiceLine();
			invoiceLine02.JI_Tariff = "0712.31.1000";
			invoiceLine02.US_SupTariff = "9903.85.01";
			invoiceLine02.JI_LinePrice = 1300m;

			var invoiceLine03 = invoiceLine01.AddSecondaryInvoiceLine();
			invoiceLine03.JI_Tariff = "2002.90.8020";
			invoiceLine03.US_SupTariff = "9903.85.01";
			invoiceLine03.JI_LinePrice = 1300m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var parentLine = entry.EntryLines.First(x => x.IsParentLine);
			AssertEquals(4, parentLine.ChildSecondaryEntryLines.Count);
			AssertEquals(0, ((IACECusEntryLine)parentLine).LicenceTypeAndNumbers.Count());

			invoiceLine01.LicenceAndPermits.AddNew("001");
			AssertEquals(1, ((IACECusEntryLine)parentLine).LicenceTypeAndNumbers.Count());

			invoiceLine02.LicenceAndPermits.AddNew("002");
			invoiceLine03.LicenceAndPermits.AddNew("003");
			AssertEquals(3, ((IACECusEntryLine)parentLine).LicenceTypeAndNumbers.Count());

			invoiceLine03.LicenceAndPermits.AddNew("KR");
			AssertEquals(3, ((IACECusEntryLine)parentLine).LicenceTypeAndNumbers.Count());
		}

		public void TestLicenceTypeAndNumbers_XVV()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine01 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine01.US_SetInd = "X";

			var invoiceLine02 = invoiceLine01.AddSecondaryInvoiceLine();
			invoiceLine02.JI_Tariff = "0712.31.1000";
			invoiceLine02.US_SupTariff = "9903.85.01";
			invoiceLine02.JI_LinePrice = 1300m;
			invoiceLine02.US_SetInd = "V";
			invoiceLine02.JI_ParentID = invoiceLine01.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var parentLine = entry.EntryLines.First(x => x.IsSetXLine);
			var childLine = entry.EntryLines.First(x => x.IsSetVLine);
			AssertEquals(1, parentLine.ChildVLines.Count());
			AssertEquals(0, ((IACECusEntryLine)parentLine).LicenceTypeAndNumbers.Count());
			AssertEquals(0, ((IACECusEntryLine)childLine).LicenceTypeAndNumbers.Count());

			invoiceLine02.LicenceAndPermits.AddNew("002");
			AssertEquals(0, ((IACECusEntryLine)parentLine).LicenceTypeAndNumbers.Count());
			AssertEquals(1, ((IACECusEntryLine)childLine).LicenceTypeAndNumbers.Count());
		}

		public void TestSelectedRateTypeWithSup()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802004040";
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.JI_LinePrice = 204m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Secondary;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(ZString.Empty, ((IDutyData)invoiceLine.CusEntryLine.ParentLine).SelectedRateType);
			AssertEquals(RateTypeList.Codes.Secondary, ((IDutyData)invoiceLine.CusEntryLine).SelectedRateType);
		}

		public void TestIDutyData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ6";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "10000000";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine = invoiceLine.CusEntryLine;

			IDutyData dutyData = entryLine;

			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			AssertEquals("SelectedRateType", RateTypeList.Codes.Primary, dutyData.SelectedRateType);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertEquals("SpecialProgramsIndicatorSecondary", SecondarySpecProgIndicatorList.Codes.F, dutyData.SpecialProgramsIndicatorSecondary);

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.E;
			AssertEquals("SpecialProgramsIndicatorPrimary", PrimarySpecProgramIndicatorList.Codes.E, dutyData.SpecialProgramsIndicatorPrimary);
			AssertEquals("SpecialProgramsIndicatorCountry", ZString.Empty, dutyData.SpecialProgramsIndicatorCountry);

			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals("SpecialProgramsIndicatorCountry", SpecialProgramList.Codes.AU, dutyData.SpecialProgramsIndicatorCountry);
			AssertEquals("SpecialProgramsIndicatorPrimary", ZString.Empty, dutyData.SpecialProgramsIndicatorPrimary);

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			AssertEquals("CountryOfOrigin", "TT", dutyData.CountryOfOrigin);

			entryLine.CL_CustomsValue = 10000m;
			AssertEquals("CustomsValue", 10000m, dutyData.CustomsValue);

			invoiceLine.JI_CustomsQuantity = 2.50m;
			AssertEquals("Quantity1", 3m, dutyData.Quantity1);

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("UQ1", "KG", dutyData.UQ1);

			invoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.JI_CustomsQuantity = 2.50m;
			invoiceLine.JI_CustomsSecondUnitQty = "LT";
			invoiceLine.JI_CustomsSecondQuantity = 4.30m;
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 8.80m;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 1.44m;

			AssertEquals("Quantity1", 3m, dutyData.Quantity1);
			AssertEquals("Quantity2", 4m, dutyData.Quantity2);
			AssertEquals("UQ2", "LT", dutyData.UQ2);
			AssertEquals("Quantity3", 9m, dutyData.Quantity3);
			AssertEquals("UQ3", "NO", dutyData.UQ3);

			entryLine.CL_AdValoremTariff = USCTariff.AGOABenefitsApplicable;
			AssertEquals("Tariff", USCTariff.AGOABenefitsApplicable, dutyData.Tariff);
		}

		public void TestIDutyData_XVVSets()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ6";

			declaration.Invoices.AddNew();
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			line1.JI_Tariff = "7215905000";
			line1.US_SupTariff = "99038815";
			line1.US_SupUQ1 = "LT";
			line1.US_SupQty1 = 1.50m;
			line1.US_SupUQ2 = "NO";
			line1.US_SupQty2 = 1.30m;
			line1.US_SupUQ3 = "NO";
			line1.US_SupQty3 = 1.80m;

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.US_SupTariff = "99038815";
			line2.JI_ParentID = line1.PK;
			line2.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			line2.US_SupUQ1 = "LT";
			line2.US_SupQty1 = 2.50m;
			line2.US_SupUQ2 = "NO";
			line2.US_SupQty2 = 2.30m;
			line2.US_SupUQ3 = "NO";
			line2.US_SupQty3 = 2.80m;

			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			line3.JI_ParentID = line2.PK;
			line3.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			line3.JI_Tariff = "7215905000";
			line3.US_SupTariff = "99038001";
			line3.US_SupUQ1 = "LT";
			line3.US_SupQty1 = 3.50m;
			line3.US_SupUQ2 = "NO";
			line3.US_SupQty2 = 3.30m;
			line3.US_SupUQ3 = "NO";
			line3.US_SupQty3 = 3.80m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertResult(line1, new ZDecimal(1.5).Round(0), new ZDecimal(1.3).Round(0), new ZDecimal(1.8).Round(0));
			AssertResult(line2, new ZDecimal(2.5 + 3.5).Round(0), new ZDecimal(2.3 + 3.3).Round(0), new ZDecimal(2.8 + 3.8).Round(0));
			AssertResult(line3, new ZDecimal(3.50).Round(0), new ZDecimal(3.30).Round(0), new ZDecimal(3.80).Round(0));
		}

		public void TestUltimateConsigneeForCargoRelease()
		{
			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "7980345io");

			OrgHeader ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "08975234078");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false);
			CusEntryLine entryLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false);

			AssertNotEquals("InvoiceLine1 and 2 should be on separate entry lines", entryLine1, entryLine2);

			AssertEquals("ultimateConsigneeForCargoRelease for entryLine1", ultimateConsignee, entryLine1.UltimateConsigneeForCargoRelease);
			AssertEquals("ultimateConsigneeForCargoRelease for entryLine1", "7980345io", entryLine1.UltimateConsigneeNumberForCargoRelease);

			AssertEquals("ultimateConsigneeForCargoRelease for entryLine2", ultimateConsignee2, entryLine2.UltimateConsigneeForCargoRelease);
			AssertEquals("ultimateConsigneeForCargoRelease for entryLine2", "08975234078", entryLine2.UltimateConsigneeNumberForCargoRelease);
		}

		public void TestInvoiceSequenceForSingleInvoice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			new EntrySummaryMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, false);

			AssertEquals("No InvDelimter for a single entry", ZInt.Zero, ((ICusEntryLine)invoiceLine.CusEntryLine).InvDelimter);
			AssertEquals("Invoice Sequence", (short)1, invoiceLine.CusEntryLine.InvoiceSequence);
		}

		public void TestInvoiceSequenceForMultipleInvoices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "AAA";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "BBB";
			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4 = invoiceLine3.AddSecondaryInvoiceLine();
			JobComInvoiceLine invoiceLine5 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals("PreCondition", (short)1, invoice.JZ_InvoiceDisplaySequence);
			AssertEquals("PreCondition", (short)2, invoice2.JZ_InvoiceDisplaySequence);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			new EntrySummaryMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, false);

			AssertEquals("Sequence number for the first entry line", (short)1, ((ICusEntryLine)invoiceLine.CusEntryLine).InvDelimter);
			AssertEquals("Sequence number is returned from a parent line or normal line", (short)0, ((ICusEntryLine)invoiceLine2.CusEntryLine).InvDelimter);

			AssertEquals("Sequence number for invoice line 3 returns nothing as it is not the last line of an invoice", (short)0, ((ICusEntryLine)invoiceLine3.CusEntryLine).InvDelimter);
			AssertEquals("Sequence number for invoice line 5 should return something as it is the last line of an invoice", (short)2, ((ICusEntryLine)invoiceLine5.CusEntryLine).InvDelimter);

			AssertEquals("Invoice Sequence", (short)1, invoiceLine.CusEntryLine.InvoiceSequence);
			AssertEquals("Invoice Sequence", (short)1, invoiceLine2.CusEntryLine.InvoiceSequence);

			AssertEquals("Invoice Sequence", (short)2, invoiceLine3.CusEntryLine.InvoiceSequence);
			AssertEquals("Invoice Sequence", (short)2, invoiceLine4.CusEntryLine.InvoiceSequence);
			AssertEquals("Invoice Sequence", (short)2, invoiceLine5.CusEntryLine.InvoiceSequence);
		}

		[TestDate(2008, 3, 25)]
		public void TestExciseTax()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(invoiceLine.CusEntryLine);
			AssertEquals("ExciseTax", 2300.84m, invoiceLine.CusEntryLine.ExciseTax);
		}

		[TestDate(2008, 3, 25)]
		public void TestADD_CVDDutyCalculation()
		{
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
				declaration.US_EnableENS = true;

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
				invoice.US_UC_NKCountryOfExport = "KR";
				invoice.US_UC_NKCountryOfOrigin = "KR";

				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, invoice.JZ_RX_NKInvoice_Currency);
				var rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDateTime.Today, currency.RX_Code, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK) ?? invoice.Invoice_Currency.ExchangeRates.AddNew();
				rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				rate.RE_StartDate = ZDateTime.Today;
				rate.RE_ExpiryDate = ZDateTime.Today;
				rate.RE_SellRate = 0.78m;

				JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "7222.11.00 50";
				invoiceLine1.JI_CustomsQuantity = 70m;
				invoiceLine1.US_ADDCaseNo = "A580844004";
				invoiceLine1.US_ADDDepositValue = 5743.45m;
				AssertEquals("ADD Deposit value in local currency", 4479.89m, invoiceLine1.ADDDepositValueInLocalCurrency);

				JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "7222.11.00 50";
				invoiceLine2.JI_CustomsQuantity = 70m;
				invoiceLine2.US_ADDCaseNo = "A580844004";
				invoiceLine2.US_ADDDepositValue = 7789.90m;
				AssertEquals("ADD Deposit value in local currency", 6076.12m, invoiceLine2.ADDDepositValueInLocalCurrency);

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("Two entry lines:Lines are currently not merged together if there is a deposit value", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				AssertEquals("Deposit value in local currency", 4480m, invoiceLine1.CusEntryLine.ADDSpecificDepositValue);
				AssertEquals("Deposit value in local currency", 6076m, invoiceLine2.CusEntryLine.ADDSpecificDepositValue);
			}
		}

		public void TestTotalCustomsValueIncludingSecondaryLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_LinePrice = 6000m;
			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			line2.JI_ParentID = line1.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(6000m, line1.CusEntryLine.TotalRoundedCustomsValueIncludingSecondaryLines);
			AssertEquals(6000m, line2.CusEntryLine.TotalRoundedCustomsValueIncludingSecondaryLines);
			AssertEquals("DutyPercentAsString should be blank, not 'free' for v lines in sets", "Free", line2.CusEntryLine.CL_DutyPercentAsString);

			line1.US_SecondarySPI = "";
			line2.US_SecondarySPI = "";
			line2.JI_ParentID = line1.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(6000m, line1.CusEntryLine.TotalRoundedCustomsValueIncludingSecondaryLines);
			AssertEquals(6000m, line2.CusEntryLine.TotalRoundedCustomsValueIncludingSecondaryLines);
			AssertEquals("DutyPercentAsString should be 'free' for this line now", "Free", line2.CusEntryLine.CL_DutyPercentAsString);
		}

		public void TestIsSetVLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("IsSetXLine", true, line1.IsSetXLine);
			AssertEquals("IsSetXLine", true, line1.CusEntryLine.IsSetXLine);
			AssertEquals("IsSetVLine", false, line1.IsSetVLine);
			AssertEquals("IsSetVLine", false, line1.CusEntryLine.IsSetVLine);

			AssertEquals("IsSetXLine", false, line2.IsSetXLine);
			AssertEquals("IsSetXLine", false, line2.CusEntryLine.IsSetXLine);
			AssertEquals("IsSetVLine", true, line2.IsSetVLine);
			AssertEquals("IsSetVLine", true, line2.CusEntryLine.IsSetVLine);
		}

		public void TestLineLevelManufacturerIDWithLabel()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.FillWithValidTestData();
			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");

			OrgHeader manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.FillWithValidTestData();
			manufacturer2.OH_FullName = "Manufacturer2";
			manufacturer2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123EREQU6LON");

			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			CusEntryLine entryLine1 = invoiceLine1.CusEntryLine;
			CusEntryLine entryLine2 = invoiceLine2.CusEntryLine;
			AssertEquals("PreCondition:HasMultipleManufacturerID", false, entryHeader.HasMultipleManufacturerIDs);

			AssertEquals("LineLevelManufacturerIDWithLabel", "", entryLine1.LineLevelManufacturerIDWithLabel);
			AssertEquals("LineLevelManufacturerIDWithLabel", "", entryLine2.LineLevelManufacturerIDWithLabel);

			invoiceLine1.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine2.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition:HasMultipleManufacturerID", true, entryHeader.HasMultipleManufacturerIDs);

			AssertEquals("LineLevelManufacturerIDWithLabel", "MID XYBEREQU6LON", entryLine1.LineLevelManufacturerIDWithLabel);
			AssertEquals("LineLevelManufacturerIDWithLabel", "MID 123EREQU6LON", entryLine2.LineLevelManufacturerIDWithLabel);
		}

		public void TestSPIAndOrSecondarySPI()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "6206900040";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = entryHeader.MergedLines[0];
			AssertEquals("SPIAndOrSecondarySPI should be blank", "", entryLine.SPIAndOrSecondarySPI);

			line1.US_SPI = SpecialProgramList.Codes.CA;
			AssertEquals("SPIAndOrSecondarySPI should be Canadian Special Rate under NAFTA code", "CA", entryLine.SPIAndOrSecondarySPI);

			line1.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertEquals("SPIAndOrSecondarySPI should be Generalized System of Preferences(GSP) code", "A", entryLine.SPIAndOrSecondarySPI);

			line1.US_SPI = "";
			line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("SPIAndOrSecondarySPI should be code to identify sets and the tariff number used to calculate the rate of duty", "X", entryLine.SPIAndOrSecondarySPI);

			line1.US_SPI = PrimarySpecProgramIndicatorList.Codes.K;
			line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			AssertEquals("SPIAndOrSecondarySPI should be combined Primary & Secondary indicators", "K.M", entryLine.SPIAndOrSecondarySPI);

			line1.US_SPI = SpecialProgramList.Codes.JPlus;
			line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.S;
			AssertEquals("SPIAndOrSecondarySPI should be combined Country Special Rate & Secondary indicators", "J+.S", entryLine.SPIAndOrSecondarySPI);
		}

		public void TestCountryOfOriginForLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "6206900040";
			line1.US_UC_NKCountryOfOrigin = "TW";

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "6206900090";
			line2.US_UC_NKCountryOfOrigin = "TW";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine1 = entryHeader.MergedLines[0];
			CusEntryLine entryLine2 = entryHeader.MergedLines[1];

			AssertEquals("PreCondition: Should have single Country of Origin", "TW", entryHeader.UniqueCountryOfOrigin);
			AssertEquals("CountryOfOriginForLine1 should be blank", "", entryLine1.CountryOfOriginForLine);
			AssertEquals("CountryOfOriginForLine2 should be blank", "", entryLine2.CountryOfOriginForLine);

			line2.US_UC_NKCountryOfOrigin = "SG";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition: Has Multiple Country of Origins", USConstants.MultipleValueIndicator, entryHeader.UniqueCountryOfOrigin);
			AssertEquals("CountryOfOriginForLine1 should be Taiwan", "O,TW", entryLine1.CountryOfOriginForLine);
			AssertEquals("CountryOfOriginForLine2 should be Singapore", "O,SG", entryLine2.CountryOfOriginForLine);
		}

		public void TestCountryOfExportForLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Tariff = "6206900040";
			line1.US_UC_NKCountryOfExport = "TW";

			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = "6206900090";
			line2.US_UC_NKCountryOfExport = "TW";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine1 = entryHeader.MergedLines[0];
			CusEntryLine entryLine2 = entryHeader.MergedLines[1];

			AssertEquals("PreCondition: Should have single Country of Export", "TW", entryHeader.UniqueCountryOfExport);
			AssertEquals("CountryOfExportForLine1 should be blank", "", entryLine1.CountryOfExportForLine);
			AssertEquals("CountryOfExportForLine2 should be blank", "", entryLine2.CountryOfExportForLine);

			line2.US_UC_NKCountryOfExport = "SG";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition: Has Multiple Country of Exports", USConstants.MultipleValueIndicator, entryHeader.UniqueCountryOfExport);
			AssertEquals("CountryOfExportForLine1 should be Taiwan", "E,TW", entryLine1.CountryOfExportForLine);
			AssertEquals("CountryOfExportForLine2 should be Singapore", "E,SG", entryLine2.CountryOfExportForLine);
		}

		public void TestBindingRulingWithLabel()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.US_PIRPRulingType = PIRPRulingTypeList.Codes.BindingRulings;
			invoiceLine1.US_PIRPRulingNo = "68739X";

			AssertEquals("BindingRulingWithLabel", "RLNG 68739X", entryLine1.BindingRulingWithLabel);
		}

		public void TestADDNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			invoiceLine.US_ADDCaseNo = "A427818000";
			entryHeader.CH_JE = declaration.PK;
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("Formatted ADDNo", "A427-818-000", entryLine.ADDNo);

			invoiceLine.US_ADDCaseNo = "427818000";
			AssertEquals("Formatted ADDNo", "A427-818-000", entryLine.ADDNo);

			invoiceLine.US_ADDCaseNo = "427818";
			AssertEquals("Formatted ADDNo", "A427-818", entryLine.ADDNo);

			invoiceLine.US_ADDCaseNo = "";
			invoiceLine.US_CVDCaseNo = "GS4345";
			AssertEquals("ADDNo should be empty", "", entryLine.ADDNo);
		}

		public void TestADA_CVDNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			invoiceLine.US_CVDCaseNo = "GS4345";
			entryHeader.CH_JE = declaration.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.US_CVDCaseNo = "GS4345";
			AssertEquals("Formatted CVDNo", "CGS4-345", entryLine.CVDNo);

			invoiceLine.US_CVDCaseNo = "427818000";
			AssertEquals("Formatted CVDNo", "C427-818-000", entryLine.CVDNo);

			invoiceLine.US_CVDCaseNo = "C427818000";
			AssertEquals("Formatted CVDNo", "C427-818-000", entryLine.CVDNo);
		}

		public void TestPartNoAndCustomAttributes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			entryHeader.CH_JE = declaration.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_PartNo = "CJ-039847478X9";
			invoiceLine.JI_CustomAttrib1 = "Attribute Text 1";
			invoiceLine.JI_CustomAttrib2 = "Attribute Text 2";
			invoiceLine.JI_CustomAttrib3 = "Attribute Text 3";

			AssertEquals("PartNo", "CJ-039847478X9", entryLine.PartNo);
			AssertEquals("CustomAttrib1", "Attribute Text 1", entryLine.CustomAttrib1);
			AssertEquals("CustomAttrib2", "Attribute Text 2", entryLine.CustomAttrib2);
			AssertEquals("CustomAttrib3", "Attribute Text 3", entryLine.CustomAttrib3);
		}

		public void TestCL_LineNumberFormatted()
		{
			var line = Factory.New<CusEntryLine>();
			line.CL_LineNumber = 999;
			AssertEquals("999", line.CL_LineNumberFormatted);

			line = Factory.New<CusEntryLine>();
			line.CL_LineNumber = 1000;
			AssertEquals("A00", line.CL_LineNumberFormatted);

			line = Factory.New<CusEntryLine>();
			line.CL_LineNumber = 1117;
			AssertEquals("A39", line.CL_LineNumberFormatted);

			line = Factory.New<CusEntryLine>();
			line.CL_LineNumber = 3599;
			AssertEquals("C07", line.CL_LineNumberFormatted);

			line = Factory.New<CusEntryLine>();
			line.CL_LineNumber = 7760;
			AssertEquals("F7S", line.CL_LineNumberFormatted);

			for (short lineNumber = 1; lineNumber < short.MaxValue; lineNumber++)
			{
				line.CL_LineNumber = lineNumber;
				var formattedLineNumber = line.CL_LineNumberFormatted;
				AssertEquals(lineNumber, CusEntryLine.GetNumericLineNumber(formattedLineNumber));
			}
		}

		[TestDate(2008, 12, 31)]
		public void TestCL_DutyPercentAsString()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5203003000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_SPICode = "AU";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_AdValoremSpecialRate = 9999.99990000m;
			dutyRate.UD_ISOCountryCode = "AU";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "AU";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine6 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine7 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine8 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine9 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine10 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.JI_Tariff = "2001903800";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_CustomsQuantity = 15000m;

			invoiceLine2.JI_Tariff = "1105100000";
			invoiceLine2.JI_LinePrice = 23500m;
			invoiceLine2.JI_CustomsQuantity = 300m;

			invoiceLine3.JI_Tariff = "0901210060";
			invoiceLine3.JI_LinePrice = 500m;
			invoiceLine3.JI_CustomsQuantity = 100m;

			invoiceLine4.JI_Tariff = "0403105000";
			invoiceLine4.JI_LinePrice = 100m;
			invoiceLine4.JI_CustomsQuantity = 100m;

			invoiceLine5.JI_Tariff = "9615113000";
			invoiceLine5.JI_LinePrice = 100m;
			invoiceLine5.JI_CustomsQuantity = 100m;

			invoiceLine6.US_SupTariff = "99135220";
			invoiceLine6.JI_InvoiceQuantity = 0m;
			invoiceLine6.JI_LinePrice = 0m;
			invoiceLine6.JI_CustomsQuantity = 2000m;
			invoiceLine6.JI_Weight = 0m;
			invoiceLine6.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine6.US_SPI = "AU";

			invoiceLine7.JI_Tariff = "5203003000";
			invoiceLine7.JI_LinePrice = 5000m;
			invoiceLine7.JI_CustomsQuantity = 2000m;
			invoiceLine7.JI_Weight = 1250m;
			invoiceLine7.US_SPI = "AU";

			invoiceLine8.JI_Tariff = "2204300000";
			invoiceLine8.JI_LinePrice = 5000m;
			invoiceLine8.JI_CustomsQuantity = 2000m;
			invoiceLine8.JI_CustomsUnitQty = "L";
			invoiceLine8.JI_CustomsSecondUnitQty = "PFL";
			invoiceLine8.JI_Weight = 1250m;

			invoiceLine9.JI_Tariff = "6505900800";
			invoiceLine9.JI_LinePrice = 5000m;
			invoiceLine9.JI_CustomsQuantity = 2000m;
			invoiceLine9.JI_CustomsUnitQty = "NO";
			invoiceLine9.JI_CustomsSecondUnitQty = "KG";
			invoiceLine9.JI_Weight = 1250m;

			invoiceLine10.JI_Tariff = "1701115000";
			invoiceLine10.JI_LinePrice = 5000m;
			invoiceLine10.JI_CustomsQuantity = 2000m;
			invoiceLine10.JI_CustomsUnitQty = "KG";
			invoiceLine10.JI_Weight = 1250m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			CusEntryLine entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			CusEntryLine entryLine3 = declaration.ActiveEntryHeaders[0].MergedLines[2];
			CusEntryLine entryLine4 = declaration.ActiveEntryHeaders[0].MergedLines[3];
			CusEntryLine entryLine5 = declaration.ActiveEntryHeaders[0].MergedLines[4];
			CusEntryLine entryLine6 = declaration.ActiveEntryHeaders[0].MergedLines[5];
			CusEntryLine entryLine7 = declaration.ActiveEntryHeaders[0].MergedLines[7];
			CusEntryLine entryLine8 = declaration.ActiveEntryHeaders[0].MergedLines[8];
			CusEntryLine entryLine9 = declaration.ActiveEntryHeaders[0].MergedLines[9];
			CusEntryLine entryLine10 = declaration.ActiveEntryHeaders[0].MergedLines[10];

			AssertEquals("DutyPercentAsString", "9.6%", entryLine1.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "1.7c/KG", entryLine2.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "Free", entryLine3.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "$1.035/KG + 17%", entryLine4.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "28.8c/GR + 4.6%", entryLine5.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "24.3c/KG", entryLine6.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString should be blank not free in this case", "", entryLine7.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString for MultipleSpecific computation 3", "4.4c/L + 31.4c/PFL", entryLine8.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString for Specific+Compound computation 6", "1.9c/NO + 13.5c/KG + 6.3%", entryLine9.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString for SpecificSugar computation K", "33.87c/KG", entryLine10.CL_DutyPercentAsString);
		}

		[TestDate(2008, 12, 31)]
		public void TestCL_DutyPercentAsStringMore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "GB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.JI_Tariff = "6402917060";
			invoiceLine1.JI_LinePrice = 31500m;
			invoiceLine1.JI_CustomsQuantity = 9000m;
			invoiceLine1.JI_Weight = 5000m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "GB";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];

			AssertEquals("DutyPercentAsString", "90c/PRS + 37.5%", entryLine1.CL_DutyPercentAsString);
		}

		public void TestTotalLinePriceInLocalCurrencyRounded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			invoice.JZ_RX_NKInvoice_Currency = uSD.RX_Code;
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.JI_Tariff = "2001903800";
			invoiceLine1.JI_LinePrice = 200.35m;
			invoiceLine1.JI_CustomsQuantity = 15000m;

			invoiceLine2.JI_Tariff = "1105100000";
			invoiceLine2.JI_LinePrice = 200.50m;
			invoiceLine2.JI_CustomsQuantity = 300m;

			invoiceLine3.JI_Tariff = "0901210060";
			invoiceLine3.JI_LinePrice = .49m;
			invoiceLine3.JI_CustomsQuantity = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			CusEntryLine entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			CusEntryLine entryLine3 = declaration.ActiveEntryHeaders[0].MergedLines[2];

			AssertEquals("Should round down", 200m, entryLine1.CustomsValueRounded);
			AssertEquals("Should round up", 201m, entryLine2.CustomsValueRounded);
			AssertEquals("Should round to zero", 0m, entryLine3.CustomsValueRounded);

			AssertEquals("Should not show decimals", "200", entryLine1.CustomsValueRounded.ToString());
			AssertEquals("Should not show decimals", "201", entryLine2.CustomsValueRounded.ToString());
			AssertEquals("Should not show decimals", "0", entryLine3.CustomsValueRounded.ToString());
		}

		public void TestChargesRounded()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 215.35m, "USD");

				JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;
				declaration.ResumeApportionment();
				CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				CusEntryLine entryLine1 = entry.MergedLines.AddNew();
				entryLine1.CL_CustomsValue = invoiceLine.JI_CustomsValue;
				invoiceLine.JI_CL = entryLine1.PK;

				AssertEquals("Should round down & prefix charge", "C215", entryLine1.ChargesRounded);

				declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
				AssertEquals("Passenger Hand Carried does not require charges", "", entryLine1.ChargesRounded);
			}
		}

		public void TestCalculateChargesUnroundedForAdditionalSupplementaryTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = TermsOfDeliveryList.Codes.FOB;
			invoice.Charges.RemoveAndDeleteAll();
			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 200m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "8539.90.0000";
			invoiceLine.SupTariffFormatted = "9903.01.23";
			invoiceLine.JI_LinePrice = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine = (CusEntryLine)declaration.FormalEntry.MergedLines.Find(x => x.CL_AdValoremTariff == "99030123").FirstOrDefault();
			AssertEquals("Charges amount", 200m, entryLine.Charges);

			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.03";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = (CusEntryLine)declaration.FormalEntry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038803").FirstOrDefault();
			AssertEquals("Charges amount", 200m, entryLine.Charges);
		}

		public void TestChargesForTIBWatches()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLineParent1 = declaration.InvoiceLines.AddNew();
			var invoiceLineParent2 = declaration.InvoiceLines.AddNew();

			invoice.Charges.AddNew(USCustomsChargeTypeList.Codes.OverseasFreight, 27m, Core.Constants.CurrencyCodes.UnitedStates);

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLineParent1.US_SupTariff = "98130020";
				invoiceLineParent1.JI_Tariff = "9101118010";
				invoiceLineParent1.JI_LinePrice = 2158m;

				var invoiceLine2 = invoiceLineParent1.AddSecondaryInvoiceLine();
				invoiceLine2.JI_Tariff = "9101118020";
				invoiceLine2.US_SupTariff = "";
				invoiceLine2.JI_LinePrice = 10062m;

				var invoiceLine3 = invoiceLineParent1.AddSecondaryInvoiceLine();
				invoiceLine3.JI_Tariff = "9101118030";
				invoiceLine3.US_SupTariff = "";
				invoiceLine3.JI_LinePrice = 292m;

				var invoiceLine4 = invoiceLineParent1.AddSecondaryInvoiceLine();
				invoiceLine4.JI_Tariff = "9101118040";
				invoiceLine4.US_SupTariff = "";
				invoiceLine4.JI_LinePrice = 16m;

				invoiceLineParent2.US_SupTariff = "98130020";
				invoiceLineParent2.JI_Tariff = "9101.29.8000";
				invoiceLineParent2.JI_LinePrice = 487.00m;

				var invoiceLine2_2 = invoiceLineParent2.AddSecondaryInvoiceLine();
				invoiceLine2_2.JI_Tariff = "9101.29.9010";
				invoiceLine2_2.US_SupTariff = "";
				invoiceLine2_2.JI_LinePrice = 32457.00m;

				var invoiceLine3_2 = invoiceLineParent2.AddSecondaryInvoiceLine();
				invoiceLine3_2.JI_Tariff = "9101.29.9020";
				invoiceLine3_2.US_SupTariff = "";
				invoiceLine3_2.JI_LinePrice = 48729.00m;
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine1 = invoiceLineParent1.CusEntryLine.ParentLine;
			AssertNotNull(supLine1);
			AssertEquals("SupLine1.Charges", 4m, supLine1.Charges);

			var supLine2 = invoiceLineParent2.CusEntryLine.ParentLine;
			AssertNotNull(supLine2);
			AssertEquals("SupLine2.Charges", 23m, supLine2.Charges);
		}

		public void TestChargesRoundedForMultipleChildLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 102250.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 10m, "USD");

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9101.29.8000";
			invoiceLine1.US_SupTariff = "9813.00.20";
			invoiceLine1.JI_LinePrice = 39500m;

			var invoiceLine1_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_Tariff = "9101.29.9010";
			invoiceLine1_1.US_SupTariff = "9813.00.20";
			invoiceLine1_1.JI_LinePrice = 45040m;
			invoiceLine1_1.JI_ParentID = invoiceLine1.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9101.11.8010";
			invoiceLine2.US_SupTariff = "9813.00.20";
			invoiceLine2.JI_LinePrice = 810m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine1 = invoiceLine1_1.CusEntryLine.ParentLine;

			Assert(entryLine1.Charges > 0m);

			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			AssertEquals("Passenger Hand Carried does not require charges", "", entryLine1.ChargesRounded);
		}

		/// <summary>
		/// Some members licence/permit are expected to be there for parent, but the related tariff is entered at the secondary line
		/// </summary>
		public void TestICusEntryLineForParentLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_DateOfExportFromCountryOfOrigin = ZDateTime.BrettsBirthday;
			JobComInvoiceLine invoiceLineP = invoice.JobComInvoiceLines.AddNew();
			invoiceLineP.US_UC_NKCountryOfOrigin = "HK";

			JobComInvoiceLine invoiceLineC = invoiceLineP.AddSecondaryInvoiceLine();
			invoiceLineC.US_VisaNo = "123";
			invoiceLineC.US_WoolLicenceNo = "W123";
			invoiceLineC.US_TextileCategoryNo = "712";
			invoiceLineC.US_VisaQty = 100m;
			invoiceLineC.US_VisaUQ = "NO";
			invoiceLineC.US_AgricultureLicNo = "A123";
			invoiceLineC.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLineC.US_CottonCertificateNo = "C123";
			invoiceLineC.US_SWPMIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLineC.US_CAExportCertificate = "C213";
			invoiceLineC.US_CBTPACertificateNo = "C986";
			invoiceLineC.US_MiscPermitNo = "M123";
			invoiceLineC.US_UC_NKCountryOfOrigin = "HK";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("One ENS entry is created", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryLine entryLineP = invoiceLineP.CusEntryLine;
			ICusEntryLine iEntryLine = entryLineP;
			AssertEquals("ExportDate", ZDateTime.BrettsBirthday, iEntryLine.DateOfExportationFromCountryOfOrigin);
			AssertEquals("Visa Number", "123", iEntryLine.VisaNumber);
			AssertEquals("Wool Licence", "W123", iEntryLine.WoolLicense);
			AssertEquals("TextileCategoryNo", "712", iEntryLine.TextileCategoryNumber);
			AssertEquals("Visa Qty", 100m, iEntryLine.VisaQuantity);
			AssertEquals("Visa UQ", "NO", iEntryLine.VisaUnitOfMeasure);
			AssertEquals("AgricultureLicenseNumber", "A123", iEntryLine.AgricultureLicenseNumber);
			AssertEquals("CottonCertificateNumberOrganicExemptionCertificateNumber", "C123", iEntryLine.CottonCertificateNumberOrganicExemptionCertificateNumber);
			AssertEquals("SWMP indicator", YesNoDefaultList.Codes.Yes, iEntryLine.ChinaHongKongSWPMIndicator);
			AssertEquals("CanadianExportCertificateSugar", "C213", iEntryLine.CanadianExportCertificateSugar);
			AssertEquals("CBTPACertificationNumber", "C986", iEntryLine.CBTPACertificationNumber);
			AssertEquals("MiscellaneousPermitLicenseNumber", "M123", iEntryLine.MiscellaneousPermitLicenseNumber);
		}

		[TestDate(2008, 9, 11)]// B00153633 CMR
		public void TestCottonCertificateNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			Assert("PreCondition", invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6104622028";
			secondaryLine.JI_CustomsQuantity = 200m;
			Assert("PreCondition", secondaryLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZString.Empty, ((ICusEntryLine)invoiceLine.CusEntryLine).CottonCertificateNumberOrganicExemptionCertificateNumber);

			invoiceLine.JI_CustomsSecondQuantity = 50000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(CottonFeeCalculator.ExemptCottonFeeCertificate, ((ICusEntryLine)invoiceLine.CusEntryLine).CottonCertificateNumberOrganicExemptionCertificateNumber);

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_CustomsSecondQuantity = 10m;// not enough to make cotton fee exempt
			invoiceLine.US_CottonCertificateNo = "009146222";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ZString.Empty, ((ICusEntryLine)invoiceLine.CusEntryLine).CottonCertificateNumberOrganicExemptionCertificateNumber);

			invoiceLine.JI_CustomsSecondQuantity = 50000;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("009146222", ((ICusEntryLine)invoiceLine.CusEntryLine).CottonCertificateNumberOrganicExemptionCertificateNumber);
			AssertEquals(0m, invoiceLine.CusEntryLine.CottonAmount);
		}

		public void TestChildLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			JobComInvoiceLine secondary = invoiceLine.AddSecondaryInvoiceLine();
			secondary.JI_Tariff = "2";
			secondary = invoiceLine.AddSecondaryInvoiceLine();
			secondary.JI_Tariff = "3";
			secondary = invoiceLine.AddSecondaryInvoiceLine();
			secondary.JI_Tariff = "4";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("PreCondition:One entry", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("PreCondition:Four entry lines", 4, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			CusEntryLine entryLine = declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("Three child lines", 3, entryLine.ChildLines.Count);

			secondary.JI_ParentID = ZGuid.Empty;
			declaration.DoMerge();
			entryLine = declaration.ActiveEntryHeaders[0].MergedLines[0];
			List<CusEntryLine> childLines = new List<CusEntryLine>(entryLine.ChildLines);
			AssertEquals("two child lines", 2, entryLine.ChildLines.Count);
			AssertEquals("should not contain an entry line for the last invoice line", false, childLines.Contains(secondary.CusEntryLine));
		}

		public void TestCharges()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 500m, "USD");

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_LinePrice = 4000m;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 16000m;
			JobComInvoiceLine secondaryLine2 = invoiceLine2.AddSecondaryInvoiceLine();
			secondaryLine2.JI_LinePrice = 4000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			AssertEquals("Charges for entryline1 include entryLine2", 500m, invoiceLine.CusEntryLine.Charges);
			AssertEquals("Charges for entryline1 include entryLine2", 200m, secondaryLine.CusEntryLine.Charges);

			AssertEquals("Charges for entryline1 include entryLine2", 50m, invoiceLine2.CusEntryLine.Charges);
			AssertEquals("Charges for entryline1 include entryLine2", 10m, secondaryLine2.CusEntryLine.Charges);
		}

		public void TestFirstInvoiceLineAfterSortedOnInvoiceLineNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "2";

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "1";

			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("entry header", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("entry lines", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("FirstInvoiceLine", invoiceLine3, invoiceLine3.CusEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
			AssertEquals("FirstInvoiceLine", invoiceLine2, invoiceLine2.CusEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
			AssertEquals("FirstInvoiceLine", invoiceLine1, invoiceLine1.CusEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo);

			invoiceLine1.JI_Tariff = "2";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("FirstInvoiceLine", invoiceLine3, invoiceLine3.CusEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
			AssertEquals("FirstInvoiceLine", invoiceLine1, invoiceLine2.CusEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
			AssertEquals("FirstInvoiceLine", invoiceLine1, invoiceLine1.CusEntryLine.FirstInvoiceLineAfterSortedOnInvoiceLineNo);
		}

		public void TestSecondCustomsQuantity()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CustomsSecondQuantity = 10m;
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CustomsSecondQuantity = 30m;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine.PK;
			AssertEquals("SecondCustomsQuantity", 40m, entryLine.SecondCustomsQuantity);

			line2.JI_CustomsSecondQuantity = 40m;
			AssertEquals("SecondCustomsQuantity", 50m, entryLine.SecondCustomsQuantity);
		}

		public void TestGrossWeightInKilograms()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Weight = 10m;
			line1.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Weight = 30m;
			line2.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine.PK;
			AssertEquals("GrossWeightInKilograms", 40000m, entryLine.GrossWeightInKilograms);

			line1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			line2.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("GrossWeightInKilograms", 30010m, entryLine.GrossWeightInKilograms);
		}

		public void TestGetGrossWeightInKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var line01 = invoice.JobComInvoiceLines.AddNew();
			var line02 = invoice.JobComInvoiceLines.AddNew();
			var line03 = invoice.JobComInvoiceLines.AddNew();

			line01.JI_Weight = 10m;
			line02.JI_Weight = 20m;
			line03.JI_Weight = 30m;

			line02.JI_ParentID = line01.PK;
			line01.US_IsParent = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals(2, entry.EntryLines.Count());
			AssertEquals(30m, ((ICusEntryLine)line01.CusEntryLine).GrossWeightInKilograms);
			AssertEquals(30m, ((ICusEntryLine)line03.CusEntryLine).GrossWeightInKilograms);
		}

		public void TestGrossWeightInPounds()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Weight = 10m;
			line1.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Weight = 30m;
			line2.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine.PK;
			AssertEquals("GrossWeightInPounds", 88184.904874m, entryLine.GrossWeightInPounds);

			line1.JI_WeightUQ = Core.Constants.Weight.Pounds;
			line2.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("GrossWeightInPounds", 66148.678656m, entryLine.GrossWeightInPounds);
		}

		public void TestVolumeInCubicMeters()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Volume = 10m;
			line1.JI_VolumeUQ = Core.Constants.Volume.CubicFeet;
			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Volume = 30m;
			line2.JI_VolumeUQ = Core.Constants.Volume.CubicFeet;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine.PK;
			AssertEquals("GrossWeightInKilograms", 1.132673m, entryLine.VolumeInCubicMeters);

			line1.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;
			line2.JI_VolumeUQ = Core.Constants.Volume.CubicFeet;
			AssertEquals("GrossWeightInKilograms", 10.849505m, entryLine.VolumeInCubicMeters);
		}

		public void TestProperties()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_ExportCode = ExportInformationCodeList.Codes.TE;
			invoice.US_LicenseType = USAESLicenseCode.Codes.C50;
			invoice.US_LicenseNo = "Lic@231";
			invoice.US_ECCN = "EC";
			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			AssertEquals("ExportCode", "", entryLine.ExportCode);
			AssertEquals("SecondCustomsUnitQty", "", entryLine.SecondCustomsUnitQty);
			AssertEquals("LicenseType", "", entryLine.LicenseType);
			AssertEquals("LicenseNumber", "", entryLine.LicenseNumber);
			AssertEquals("MarksAndNumbers", "", entryLine.MarksAndNumbers);
			AssertEquals("IsUsedVehicle", false, entryLine.IsUsedVehicle);
			AssertEquals("VehicleIDType", "", entryLine.VehicleIDType);
			AssertEquals("VehicleID", "", entryLine.VehicleID);
			AssertEquals("VehicleTitleNumber", "", entryLine.VehicleTitleNumber);
			AssertEquals("VehicleTitleState", "", entryLine.VehicleTitleState);
			AssertEquals("ECCN", "", entryLine.ECCN);
			AssertEquals("AESOriginIndicator", "", entryLine.AESOriginIndicator);

			entryLine = entry.MergedLines.AddNew();
			line1.JI_CL = entryLine.PK;
			AssertEquals("ExportCode", ExportInformationCodeList.Codes.TE, entryLine.ExportCode);
			AssertEquals("SecondCustomsUnitQty", "", entryLine.SecondCustomsUnitQty);
			AssertEquals("LicenseType", USAESLicenseCode.Codes.C50, entryLine.LicenseType);
			AssertEquals("LicenseNumber has been changed to used special characters set - will send what user has entered", "LIC@231", entryLine.LicenseNumber);
			AssertEquals("MarksAndNumbers", "", entryLine.MarksAndNumbers);
			AssertEquals("IsUsedVehicle", false, entryLine.IsUsedVehicle);
			AssertEquals("VehicleIDType", "", entryLine.VehicleIDType);
			AssertEquals("VehicleID", "", entryLine.VehicleID);
			AssertEquals("VehicleTitleNumber", "", entryLine.VehicleTitleNumber);
			AssertEquals("VehicleTitleState", "", entryLine.VehicleTitleState);
			AssertEquals("ECCN", "EC", entryLine.ECCN);
			AssertEquals("AESOriginIndicator", line1.US_AESOriginIndicator, entryLine.AESOriginIndicator);

			line1.US_ExportCode = ExportInformationCodeList.Codes.HH;
			line1.US_LicenseType = USAESLicenseCode.Codes.C57;
			line1.US_LicenseNo = "F1-P-LBD";
			line1.US_ECCN = "CNãDS"; // invalid character should be stripped
			line1.JI_CustomsSecondUnitQty = Core.Constants.Weight.Kilograms;
			line1.US_MarksAndNumbers = "MaËrks&Nu§ms"; // invalid character should be stripped
			line1.US_IsUsedVehicle = true;
			line1.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			line1.US_VehicleID = "VI¢D1³2"; // invalid character should be stripped
			line1.US_VehicleTitleNo = "VTi§tleNo"; // invalid character should be stripped
			line1.US_VehicleTitleState = "MA";
			line1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;
			AssertEquals("ExportCode", ExportInformationCodeList.Codes.HH, entryLine.ExportCode);
			AssertEquals("SecondCustomsUnitQty", Core.Constants.Weight.Kilograms, entryLine.SecondCustomsUnitQty);
			AssertEquals("LicenseType", USAESLicenseCode.Codes.C57, entryLine.LicenseType);
			AssertEquals("LicenseNumber should validly retain dashes", "F1-P-LBD", entryLine.LicenseNumber);
			AssertEquals("MarksAndNumbers", "MARKS&NUMS", entryLine.MarksAndNumbers);
			AssertEquals("IsUsedVehicle", true, entryLine.IsUsedVehicle);
			AssertEquals("VehicleIDType", VehicleIDTypeList.Codes.VIN, entryLine.VehicleIDType);
			AssertEquals("VehicleID", "VI¢D12", entryLine.VehicleID);
			AssertEquals("VehicleTitleNumber", "VTITLENO", entryLine.VehicleTitleNumber);
			AssertEquals("VehicleTitleState", "MA", entryLine.VehicleTitleState);
			AssertEquals("ECCN", "CNDS", entryLine.ECCN);
			AssertEquals("AESOriginIndicator", AESOriginIndicatorList.Codes.Foreign, entryLine.AESOriginIndicator);
		}

		public void TestIsSetHeaderLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsSetHeaderLine", false, invoiceLine.CusEntryLine.IsSetXLine);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsSetHeaderLine", true, invoiceLine.CusEntryLine.IsSetXLine);
		}

		public void TestFDA()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.FDAs.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryLine entryLine = invoiceLine.CusEntryLine;

			ICusEntryLine iEntryLine = entryLine;
			AssertEquals(OGAIndicatorList.Codes.Declared, ((IOGA)entryLine).FDAIndicator);
			AssertEquals(1, iEntryLine.FDA.Count);
		}

		public void TestDOT()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.DOTs.AddNew();
			invoiceLine.DOTs.AddNew();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = USCTariff.DOTIsApplicable;

			ICusEntryLine iEntryLine = entryLine;
			AssertEquals(OGAIndicatorList.Codes.Declared, ((IOGA)entryLine).DOTIndicator);
			AssertEquals(2, iEntryLine.DOT.Count());
		}

		public void TestCalculatedEntryHeaderData()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateSeaExportDeclaration();

			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			AssertNull("entryLine.Header", entryLine.Header);
			AssertEquals("entryLine.CL_Calc_EntryNumber", "", entryLine.CL_Calc_EntryNumber);
			AssertEquals("entryLine.CL_Calc_XTN", "", entryLine.CL_Calc_XTN);

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "ITN1234";
			entryHeader.US_XTN = "XTN1234";

			entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals(entryHeader.PK, entryLine.Header.PK);
			AssertEquals("CL_Calc_EntryNumber", entryHeader.EntryNumber, entryLine.CL_Calc_EntryNumber);
			AssertEquals("CL_Calc_XTN", entryHeader.US_XTN, entryLine.CL_Calc_XTN);
		}

		public void TestMPFAndHMFAmountsForLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("EntryChargeTypeList", typeof(Registry.Business.Customs.US.EntryChargeTypeList), entry.EntryChargeTypeList.GetType());

			CusEntryLine entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.HMF).CF_ChargeAmount = 100m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CF_ChargeAmount = 25m;

			AssertEquals("HMF for line", 100m, entryLine.HMFAmount);
			AssertEquals("MPF for line", 25m, entryLine.MPFAmount);
		}

		public void TestOtherChargeAmounts()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Beef).CF_ChargeAmount = 1m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Blueberry).CF_ChargeAmount = 2m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Cotton).CF_ChargeAmount = 3m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.DutiableMail).CF_ChargeAmount = 4m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Honey).CF_ChargeAmount = 5m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal).CF_ChargeAmount = 6m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.FreshLimes).CF_ChargeAmount = 7m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Mango).CF_ChargeAmount = 8m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Mushroom).CF_ChargeAmount = 9m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Raspberry).CF_ChargeAmount = 10m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Pork).CF_ChargeAmount = 11m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Potato).CF_ChargeAmount = 12m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber).CF_ChargeAmount = 13m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Sorghum).CF_ChargeAmount = 14m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Sugar).CF_ChargeAmount = 15m;
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.Watermelon).CF_ChargeAmount = 16m;

			AssertEquals("Beef Fee", 1m, entryLine.BeefAmount);
			AssertEquals("Blueberry Fee", 2m, entryLine.BlueberryAmount);
			AssertEquals("Cotton Fee", 3m, entryLine.CottonAmount);
			AssertEquals("DutiableMail Fee", 4m, entryLine.DutiableMailAmount);
			AssertEquals("Honey Fee", 5m, entryLine.HoneyAmount);
			AssertEquals("Informal Fee", 6m, entryLine.InformalAmount);
			AssertEquals("Limes Fee", 7m, entryLine.LimesAmount);
			AssertEquals("Mango Fee", 8m, entryLine.MangoAmount);
			AssertEquals("Mushroom Fee", 9m, entryLine.MushroomAmount);
			AssertEquals("Raspberry Fee", 10m, entryLine.RaspberryAmount);
			AssertEquals("Pork Fee", 11m, entryLine.PorkAmount);
			AssertEquals("Potato Fee", 12m, entryLine.PotatoAmount);
			AssertEquals("Softwood Lumber Fee", 13m, entryLine.SoftwoodLumberAmount);
			AssertEquals("Sorghum Fee", 14m, entryLine.SorghumAmount);
			AssertEquals("Sugar Fee", 15m, entryLine.SugarAmount);
			AssertEquals("Watermelon Fee", 16m, entryLine.WatermelonAmount);
		}

		public void TestDestinationState()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine = invoiceLine.CusEntryLine;

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, "IL");
			newFactory.Save();

			declaration.US_DestinationState = "IL";
			AssertEquals("IL", entryLine.DestinationState);

			invoice.US_DestinationState = "BB";
			AssertEquals("BB", entryLine.DestinationState);

			invoiceLine.US_DestinationState = "CC";
			AssertEquals("CC", entryLine.DestinationState);
		}

		public void TestChargesForMultipleTariffLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine primaryLine = invoice.JobComInvoiceLines.AddNew();
			primaryLine.JI_LinePrice = 0m;

			JobComInvoiceLine secondaryLine = primaryLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_LinePrice = 2000m;

			JobComInvoiceLine setXLine = invoice.JobComInvoiceLines.AddNew();
			setXLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			setXLine.JI_LinePrice = 8000m;

			JobComInvoiceLine setVLine1 = setXLine.AddSecondaryInvoiceLine();
			setVLine1.JI_LinePrice = 2000m;

			JobComInvoiceLine setVLine2 = setXLine.AddSecondaryInvoiceLine();
			setVLine2.JI_LinePrice = 2000m;

			JobComInvoiceLine setVLine3 = setXLine.AddSecondaryInvoiceLine();
			setVLine3.JI_LinePrice = 4000m;

			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 1000m, "USD");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Charges for secondary line with component price", 200m, secondaryLine.CusEntryLine.Charges);

			AssertEquals("Charges for X line", 800m, setXLine.CusEntryLine.Charges);
			AssertEquals("Charges for V line", 200m, setVLine1.CusEntryLine.Charges);
			AssertEquals("Charges for V line", 200m, setVLine2.CusEntryLine.Charges);
			AssertEquals("Charges for V line", 400m, setVLine3.CusEntryLine.Charges);
		}

		public void TestUniqueKey()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entry.EntryNumber = "12345678";
			entryLine.CL_LineNumber = 1;
			AssertEquals("Should be filer code", "XJ512345678-1", entryLine.UniqueKey);
		}

		[TestDate(2008, 9, 11)]
		public void TestParentChildLineDuty()
		{
			JobDeclaration declaration = CreateTIBDeclaration();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Total duty calculation amount - line without parent", 0m, entry.MergedLines[0].ParentChildLineDuty);
			AssertEquals("Total duty calculation amount - parent/child combination", 697.6m, entry.MergedLines[1].ParentChildLineDuty);
		}

		public void TestDOTsForSupLineWithDT1_DT2()
		{
			USCTariff tariff1DT1 = Factory.New<USCTariff>();
			tariff1DT1.UE_Tariff = "9813123211";
			tariff1DT1.UE_Unit1 = "KG";
			tariff1DT1.UE_OGACodes = "DT1";
			tariff1DT1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1DT1.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariff1DT2 = Factory.New<USCTariff>();
			tariff1DT2.UE_Tariff = "8965123211";
			tariff1DT2.UE_Unit1 = "KG";
			tariff1DT2.UE_OGACodes = "DT2";
			tariff1DT2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1DT2.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariff2DT1 = Factory.New<USCTariff>();
			tariff2DT1.UE_Tariff = "9813123212";
			tariff2DT1.UE_Unit1 = "KG";
			tariff2DT1.UE_OGACodes = "DT1";
			tariff2DT1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2DT1.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariff2DT2 = Factory.New<USCTariff>();
			tariff2DT2.UE_Tariff = "8965123212";
			tariff2DT2.UE_Unit1 = "KG";
			tariff2DT2.UE_OGACodes = "DT2";
			tariff2DT2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2DT2.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariff3DT1 = Factory.New<USCTariff>();
			tariff3DT1.UE_Tariff = "9813123213";
			tariff3DT1.UE_Unit1 = "KG";
			tariff3DT1.UE_OGACodes = "DT1";
			tariff3DT1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3DT1.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariff3DT2 = Factory.New<USCTariff>();
			tariff3DT2.UE_Tariff = "8965123213";
			tariff3DT2.UE_Unit1 = "KG";
			tariff3DT2.UE_OGACodes = "DT2";
			tariff3DT2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3DT2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = tariff1DT1.UE_Tariff;
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			invoiceLine1.JI_Tariff = tariff1DT2.UE_Tariff;
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			invoiceLine1.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			var dot1 = invoiceLine1.DOTs.AddNew();
			dot1.US_DOTBoxNo = "A2";
			dot1.US_DOTClarCode = "V";
			var dot1Vin1 = dot1.DOTVINs.AddNew();
			dot1Vin1.US_DOTMake = "AUDI";
			dot1Vin1.US_DOTModel = "D2D3";
			dot1Vin1.US_DOTVIN = "VIN1";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = tariff2DT1.UE_Tariff;
			invoiceLine2.JI_InvoiceQuantity = 6100m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 24055m;

			invoiceLine2.JI_Tariff = tariff2DT2.UE_Tariff;
			invoiceLine2.JI_CustomsQuantity = 6100m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			invoiceLine2.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			var dot2 = invoiceLine2.DOTs.AddNew();
			dot2.US_DOTBoxNo = "A3";
			dot2.US_DOTClarCode = "V";
			var dot2Vin1 = dot2.DOTVINs.AddNew();
			dot2Vin1.US_DOTMake = "HOLDEN";
			dot2Vin1.US_DOTModel = "HD34";
			dot2Vin1.US_DOTVIN = "VIN2";
			var dot2Vin2 = dot2.DOTVINs.AddNew();
			dot2Vin2.US_DOTMake = "HOLDEN";
			dot2Vin2.US_DOTModel = "HD36";
			dot2Vin2.US_DOTVIN = "VIN3";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = tariff3DT1.UE_Tariff;
			invoiceLine3.JI_InvoiceQuantity = 6100m;
			invoiceLine3.JI_InvoiceUQ = "KG";
			invoiceLine3.JI_LinePrice = 24055m;

			invoiceLine3.JI_Tariff = tariff3DT2.UE_Tariff;
			invoiceLine3.JI_CustomsQuantity = 6100m;
			invoiceLine3.JI_CustomsUnitQty = "KG";

			invoiceLine3.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			var dot3 = invoiceLine3.DOTs.AddNew();
			dot3.US_DOTBoxNo = "A4";
			dot3.US_DOTClarCode = "P";
			var dot3Vin1 = dot3.DOTVINs.AddNew();
			dot3Vin1.US_DOTMake = "HONDA";
			dot3Vin1.US_DOTModel = "HDN32";
			dot3Vin1.US_DOTVIN = "VIN4";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLines = new List<CusEntryLine>(entry.EntryLines);
			AssertEquals(3, entryLines.Count);
			var entryLine = entryLines[0];
			IOGA oga = entryLine;
			AssertEquals(OGAIndicatorList.Codes.Declared, oga.DOTIndicator);
			AssertEquals(1, oga.DOT.Count());
			IDOT dot = oga.DOT.First();
			var vins = new List<IDOTVIN>(dot.VINs);
			AssertEquals(1, vins.Count);
			AssertEquals("98 Line should report Zeroes", "".PadLeft(17, '0'), vins[0].VehicleIdentificationNumber);

			AssertEquals(1, entryLine.ChildLines.Count);
			IOGA childEntryLine = entryLine.ChildLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, childEntryLine.DOTIndicator);
			AssertEquals(1, childEntryLine.DOT.Count());
			dot = childEntryLine.DOT.First();
			AssertEquals(1, dot.VINs.Count());
			AssertEquals("VIN1", dot.VINs.First().VehicleIdentificationNumber);

			entryLine = entryLines[1];
			oga = entryLine;
			AssertEquals(OGAIndicatorList.Codes.Declared, oga.DOTIndicator);
			AssertEquals(1, oga.DOT.Count());
			dot = oga.DOT.First();
			vins = new List<IDOTVIN>(dot.VINs);
			AssertEquals(1, vins.Count);
			AssertEquals("98 Line should report Zeroes", "1".PadLeft(17, '0'), vins[0].VehicleIdentificationNumber);

			AssertEquals(1, entryLine.ChildLines.Count);
			childEntryLine = entryLine.ChildLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, childEntryLine.DOTIndicator);
			AssertEquals(1, childEntryLine.DOT.Count());
			dot = childEntryLine.DOT.First();
			vins = new List<IDOTVIN>(dot.VINs);
			AssertEquals(2, vins.Count);
			var vin1 = vins[0];
			var vin2 = vins[1];
			if (vin2.VehicleEligibilityNumber == "VIN2")
			{
				AssertEquals("VIN3", vin1.VehicleIdentificationNumber);
			}
			else
			{
				AssertEquals("VIN2", vin1.VehicleIdentificationNumber);
				AssertEquals("VIN3", vin2.VehicleIdentificationNumber);
			}

			entryLine = entryLines[2];
			oga = entryLine;
			AssertEquals(OGAIndicatorList.Codes.Declared, oga.DOTIndicator);
			AssertEquals(1, oga.DOT.Count());
			dot = oga.DOT.First();
			vins = new List<IDOTVIN>(dot.VINs);
			AssertEquals("No VIN as it's Product ClarCode", 0, vins.Count);

			AssertEquals(1, entryLine.ChildLines.Count);
			childEntryLine = entryLine.ChildLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, childEntryLine.DOTIndicator);
			AssertEquals(1, childEntryLine.DOT.Count());
			dot = childEntryLine.DOT.First();
			AssertEquals(1, dot.VINs.Count());
			AssertEquals("VIN4", dot.VINs.First().VehicleIdentificationNumber);
		}

		public void TestDOTsForSupLineWithDT1_NoneWithDisclaim()
		{
			SetUpTariffsForDOT();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9813123211";
			invoiceLine.JI_InvoiceQuantity = 6100m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_LinePrice = 24055m;

			invoiceLine.JI_Tariff = "7632585211";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(2, entry.MergedLines.Count);
			ICusEntryLine entryLine = entry.MergedLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, entryLine.DOTIndicator);
			AssertEquals(1, entry.MergedLines[0].ChildLines.Count);
			ICusEntryLine childEntryLine = entry.MergedLines[0].ChildLines[0];
			AssertEquals("", childEntryLine.DOTIndicator);

			entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertEquals(2, entry.MergedLines.Count);
			entryLine = entry.MergedLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, entryLine.DOTIndicator);
			childEntryLine = entry.MergedLines[1];
			AssertEquals("", childEntryLine.DOTIndicator);
		}

		public void TestDOTsForSupLineWithDT2_None()
		{
			SetUpTariffsForDOT();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9813123211";//dt2
			invoiceLine.JI_InvoiceQuantity = 6100m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_LinePrice = 24055m;

			invoiceLine.JI_Tariff = "7632585211";//none
			invoiceLine.JI_CustomsQuantity = 6100m;
			invoiceLine.JI_CustomsUnitQty = "KG";

			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			var dot1 = invoiceLine.DOTs.AddNew();
			dot1.US_DOTBoxNo = "A2";
			dot1.US_DOTClarCode = "V";
			var dot1Vin1 = dot1.DOTVINs.AddNew();
			dot1Vin1.US_DOTMake = "AUDI";
			dot1Vin1.US_DOTModel = "D2D3";
			dot1Vin1.US_DOTVIN = "VIN1";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(2, entry.MergedLines.Count);
			IOGA entryLine = entry.MergedLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, entryLine.DOTIndicator);
			AssertEquals(1, entryLine.DOT.Count());
			IDOT dot = entryLine.DOT.First();
			AssertEquals(1, dot.VINs.Count());
			AssertEquals("VIN1", dot.VINs.First().VehicleIdentificationNumber);

			AssertEquals(1, entry.MergedLines[0].ChildLines.Count);
			IOGA childEntryLine = entry.MergedLines[0].ChildLines[0];
			AssertEquals(ZString.Empty, childEntryLine.DOTIndicator);

			entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertEquals(2, entry.MergedLines.Count);
			entryLine = entry.MergedLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, entryLine.DOTIndicator);
			AssertEquals(1, entryLine.DOT.Count());
			dot = entryLine.DOT.First();
			AssertEquals(1, dot.VINs.Count());
			AssertEquals("VIN1", dot.VINs.First().VehicleIdentificationNumber);

			childEntryLine = entry.MergedLines[1];
			AssertEquals(ZString.Empty, childEntryLine.DOTIndicator);
			AssertEquals(1, childEntryLine.DOT.Count());
		}

		public void TestDDTCMessageDoesntDuplicatePGABlock()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_CargoReleaseType = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			declaration.US_SchDEntry = "1101";
			declaration.US_SchDArrival = "1101";
			declaration.US_EnableENS = true;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			var header = declaration.Invoices.AddNew();

			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9013.80.9000";
			invoiceLine.US_SupTariff = "9813.00.0520";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

			invoiceLine.US_DDTCInd = "D";
			invoiceLine.US_DDTCLicenseType = "S73";
			invoiceLine.US_DDTCExemptionCode = "123.13";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.RunPreSaveValidation();
			declaration.DoMerge();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("The PGA block should not be duplicated: " + message.EM_FormattedMessageText, message.EM_FormattedMessageText.Contains(
@"509813000520 0000000000 0000000000                                              
509013809000 0000000000 0000000000 000000001000X                                
OI        COMMERCIAL DESCRIPTION                                                
PG01001DTCDTC                                                                   
PG02P                                                                           
PG14 S73                                                               123.13   
PG30A                                                                           "));
		}

		public void TestDDTCMessageDoesntRemoveOnlyPGABlock()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_CargoReleaseType = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			declaration.US_SchDEntry = "1101";
			declaration.US_SchDArrival = "1101";
			declaration.US_EnableENS = true;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			var header = declaration.Invoices.AddNew();

			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9013.80.9000";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

			invoiceLine.US_DDTCInd = "D";
			invoiceLine.US_DDTCLicenseType = "S73";
			invoiceLine.US_DDTCExemptionCode = "123.13";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.RunPreSaveValidation();
			declaration.DoMerge();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Assert("The PGA block should not be removed: " + message.EM_FormattedMessageText, message.EM_FormattedMessageText.Contains(
@"509013809000 0000000000 0000000000 000000001000X                                
OI        COMMERCIAL DESCRIPTION                                                
PG01001DTCDTC                                                                   
PG02P                                                                           
PG14 S73                                                               123.13   
PG30A                                                                           "));
		}

		public void TestPSCReasonCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			var pscReasonCodes = Factory.New<PSCReasonCusCodeData>();
			pscReasonCodes.CY_ParentID = entryLine.PK;
			pscReasonCodes.CY_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
			pscReasonCodes.CY_Data = "Some data";

			Factory.Save();
			AssertEquals("Object saved should be retrieve", pscReasonCodes, entryLine.PSCReasonCodes);
		}

		public void TestTSCAMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7007110010";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "";
			tariff.UE_PGACodes = "EP8";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "8703210000";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_OGACodes = "";
			tariff2.UE_PGACodes = "";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801008000";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION TEST 1";
			invoiceLine.US_SPI = "AU";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9801008000";
			invoiceLine2.US_UC_NKCountryOfExport = "AU";
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine2.US_SPI = "AU";
			invoiceLine2.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IGovernmentAgencies classificationLine = invoiceLine.CusEntryLine;
			AssertEquals("EP4", classificationLine.EPA_TSCAData.TSCACertificationCode);
			AssertEquals("D", classificationLine.TSCAIndicator);
			AssertEquals("", classificationLine.TSCADisclaimReason);
			AssertEquals("B", classificationLine.ODSDisclaimReason);

			IGovernmentAgencies classificationLine2 = invoiceLine2.CusEntryLine;
			AssertEquals("C", classificationLine2.TSCAIndicator);
		}

		public void TestPGALinesFor98_99Tariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7007110010";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_OGACodes = "DT1FS2";
			tariff.UE_PGACodes = "EP4NM1";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "8703210000";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_OGACodes = "FS2";
			tariff2.UE_PGACodes = "EP2NM4";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9801008000";
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION TEST 1";
			invoiceLine.US_SPI = "AU";
			invoiceLine.VehicleLines.AddNew();
			invoiceLine.FSISLines.AddNew();
			invoiceLine.PSTLines.AddNew();
			var nmfs370Line = invoiceLine.NMFSLines.AddNew();
			nmfs370Line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var nmfsAMRLine = invoiceLine.NMFSLines.AddNew();
			nmfsAMRLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			var nmfsHMSLine = invoiceLine.NMFSLines.AddNew();
			nmfsHMSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			var nmfsSIMLine = invoiceLine.NMFSLines.AddNew();
			nmfsSIMLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			var nmfsCOALine = invoiceLine.NMFSLines.AddNew();
			nmfsCOALine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.NHTSALines.AddNew();

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9801008000";
			invoiceLine2.US_UC_NKCountryOfExport = "AU";
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine2.US_SPI = "AU";
			invoiceLine2.FSISLines.AddNew();
			nmfs370Line = invoiceLine2.NMFSLines.AddNew();
			nmfs370Line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfsAMRLine = invoiceLine2.NMFSLines.AddNew();
			nmfsAMRLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsHMSLine = invoiceLine2.NMFSLines.AddNew();
			nmfsHMSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsSIMLine = invoiceLine2.NMFSLines.AddNew();
			nmfsSIMLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsCOALine = invoiceLine2.NMFSLines.AddNew();
			nmfsCOALine.US_ProgramType = NMFSProgramCodeList.Codes.COA;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IGovernmentAgencies classificationLine = invoiceLine.CusEntryLine;
			IGovernmentAgencies supplementaryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals("COMMERCIAL DESCRIPTION TEST 1", classificationLine.CommercialDescription);
			AssertEquals(1, classificationLine.EPA_VNELines.Count());
			AssertEquals(1, classificationLine.FSISLines.Count());
			AssertEquals(1, classificationLine.EPA_PSTLines.Count());
			AssertEquals(1, classificationLine.NMFS370Lines.Count());
			AssertEquals(1, classificationLine.NMFSAMRLines.Count());
			AssertEquals(1, classificationLine.NMFSHMSLines.Count());
			AssertEquals(1, classificationLine.NMFSSIMLines.Count());
			AssertEquals(1, classificationLine.NMFSCOALines.Count());
			AssertEquals(1, classificationLine.FDALines.Count());
			AssertEquals(1, classificationLine.NHTSALines.Count());

			AssertEquals(0, supplementaryLine.EPA_VNELines.Count());
			AssertEquals(0, supplementaryLine.FSISLines.Count());
			AssertEquals(0, supplementaryLine.EPA_PSTLines.Count());
			AssertEquals(0, supplementaryLine.NMFS370Lines.Count());
			AssertEquals(0, supplementaryLine.NMFSAMRLines.Count());
			AssertEquals(0, supplementaryLine.NMFSHMSLines.Count());
			AssertEquals(0, supplementaryLine.NMFSSIMLines.Count());
			AssertEquals(0, supplementaryLine.NMFSCOALines.Count());
			AssertEquals(0, supplementaryLine.FDALines.Count());
			AssertEquals(0, supplementaryLine.NHTSALines.Count());

			IGovernmentAgencies classificationLine2 = invoiceLine2.CusEntryLine;
			IGovernmentAgencies supplementaryLine2 = invoiceLine2.CusEntryLine.ParentLine;

			AssertEquals(1, classificationLine2.FSISLines.Count());
			AssertEquals(0, classificationLine2.EPA_PSTLines.Count());
			AssertEquals(1, classificationLine2.NMFS370Lines.Count());
			AssertEquals(1, classificationLine2.NMFSAMRLines.Count());
			AssertEquals(1, classificationLine2.NMFSHMSLines.Count());
			AssertEquals(1, classificationLine2.NMFSSIMLines.Count());
			AssertEquals(1, classificationLine2.NMFSCOALines.Count());
			AssertEquals(0, classificationLine2.FDALines.Count());
			AssertEquals(0, classificationLine2.NHTSALines.Count());

			AssertEquals(0, supplementaryLine2.FSISLines.Count());
			AssertEquals(0, supplementaryLine2.EPA_PSTLines.Count());
			AssertEquals(0, supplementaryLine2.NMFS370Lines.Count());
			AssertEquals(0, supplementaryLine2.NMFSAMRLines.Count());
			AssertEquals(0, supplementaryLine2.NMFSHMSLines.Count());
			AssertEquals(0, supplementaryLine2.NMFSSIMLines.Count());
			AssertEquals(0, supplementaryLine2.NMFSCOALines.Count());
			AssertEquals(0, supplementaryLine2.FDALines.Count());
			AssertEquals(0, supplementaryLine2.NHTSALines.Count());
		}

		public void TestPGAFor98_99TariffsWhenProgAndClassLinesRequireData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();

			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8703105060"));
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "8703105060";
			}
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "EP3";

			var tariff2 = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9801008000"));
			if (tariff2 == null)
			{
				tariff2 = Factory.New<USCTariff>();
				tariff2.UE_Tariff = "9801008000";
			}
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_OGACodes = "FD3";
			tariff2.UE_PGACodes = "EP3";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = tariff2.UE_Tariff;
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = "AU";
			var vne = invoiceLine.VehicleLines.AddNew();
			vne.US_VehicleModel = "Test";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			IGovernmentAgencies classificationLine = invoiceLine.CusEntryLine;
			IGovernmentAgencies supplementaryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(0, classificationLine.EPA_VNELines.Count());
			AssertEquals(1, supplementaryLine.EPA_VNELines.Count());
		}

		public void TestIRTaxCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9905903080";
			tariff.UE_Unit1 = "KG";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var dutyRate1 = Factory.New<USCTariffDutyRate>();
			dutyRate1.UD_UE = tariff.PK;
			var dutyRate2 = Factory.New<USCTariffDutyRate>();
			dutyRate2.UD_UE = tariff.PK;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = tariff.UE_Tariff;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine = invoiceLine.CusEntryLine as IACECusEntryLine;
			AssertEquals(ZString.Empty, entryLine.IRTaxCode);

			dutyRate1.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
			AssertEquals(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, entryLine.IRTaxCode);

			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
			AssertEquals(ZString.Empty, entryLine.IRTaxCode);

			invoiceLine.JI_FormattedTariff = ZString.Empty;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_2;
			invoiceLine.US_TaxQty = 567m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Wines, entryLine.IRTaxCode);
		}

		public void TestIPGALineNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			((IPGALineNumbers)entryLine).EPAStartLineNumber = 1;
			((IPGALineNumbers)entryLine).FSISStartLineNumber = 2;
			((IPGALineNumbers)entryLine).NMFSStartLineNumber = 3;
			((IPGALineNumbers)entryLine).FDAStartLineNumber = 4;
			((IPGALineNumbers)entryLine).TTBStartLineNumber = 5;
			((IPGALineNumbers)entryLine).NHTSAStartLineNumber = 6;
			((IPGALineNumbers)entryLine).AMSStartLineNumber = 7;
			((IPGALineNumbers)entryLine).APHStartLineNumber = 8;
			((IPGALineNumbers)entryLine).FWSStartLineNumber = 9;
			((IPGALineNumbers)entryLine).ATFStartLineNumber = 10;
			((IPGALineNumbers)entryLine).CPSCStartLineNumber = 11;
			((IPGALineNumbers)entryLine).OMCStartLineNumber = 12;
			((IPGALineNumbers)entryLine).DEAStartLineNumber = 13;
			AssertEquals(1, ((IPGALineNumbers)entryLine).EPAStartLineNumber);
			AssertEquals(2, ((IPGALineNumbers)entryLine).FSISStartLineNumber);
			AssertEquals(3, ((IPGALineNumbers)entryLine).NMFSStartLineNumber);
			AssertEquals(4, ((IPGALineNumbers)entryLine).FDAStartLineNumber);
			AssertEquals(5, ((IPGALineNumbers)entryLine).TTBStartLineNumber);
			AssertEquals(6, ((IPGALineNumbers)entryLine).NHTSAStartLineNumber);
			AssertEquals(7, ((IPGALineNumbers)entryLine).AMSStartLineNumber);
			AssertEquals(8, ((IPGALineNumbers)entryLine).APHStartLineNumber);
			AssertEquals(9, ((IPGALineNumbers)entryLine).FWSStartLineNumber);
			AssertEquals(10, ((IPGALineNumbers)entryLine).ATFStartLineNumber);
			AssertEquals(11, ((IPGALineNumbers)entryLine).CPSCStartLineNumber);
			AssertEquals(12, ((IPGALineNumbers)entryLine).OMCStartLineNumber);
			AssertEquals(13, ((IPGALineNumbers)entryLine).DEAStartLineNumber);

			((IPGALineNumbers)entryLine).ClearPGALineNumbers();
			AssertEquals(0, ((IPGALineNumbers)entryLine).EPAStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).FSISStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).NMFSStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).FDAStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).TTBStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).NHTSAStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).AMSStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).APHStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).FWSStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).ATFStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).CPSCStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).OMCStartLineNumber);
			AssertEquals(0, ((IPGALineNumbers)entryLine).DEAStartLineNumber);
		}

		public void TestNMFSCOAIndicator()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0000000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();

			var tariff = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0000000001")).LastOrDefault();
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "0000000001";
			}
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			AssertEquals("D", ((IGovernmentAgenciesIndicators)entryLine).NMFSCOAIndicator);

			entryLine = invoiceLine1.CusEntryLine;
			AssertEquals(ZString.Empty, ((IGovernmentAgenciesIndicators)entryLine).NMFSCOAIndicator);
		}

		public void TestHFCRelated()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-30);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "EH2";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine = invoiceLine.CusEntryLine;
			Assert(!entryLine.HasAnyPGADataToBeDeclaredOrDisclaimed());

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.USHFCHeaders.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
			AssertEquals(OGAIndicatorList.Codes.Declared, ((IGovernmentAgenciesIndicators)entryLine).HFCIndicator);
			AssertEquals(1, (((IGovernmentAgencies)entryLine).EPA_HFCHeaders).Count());
			Assert(entryLine.HasAnyPGADataToBeDeclaredOrDisclaimed());

			invoiceLine.USHFCHeaders.RemoveAndDeleteAll();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			entryLine = invoiceLine.CusEntryLine;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, ((IGovernmentAgenciesIndicators)entryLine).HFCIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, ((IGovernmentAgenciesIndicators)entryLine).HFCDisclaimReason);
			Assert(entryLine.HasAnyPGADataToBeDeclaredOrDisclaimed());
		}

		public void TestGrossWeightInKilogramsForSupAdditionalTariffs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "8215.99.3500";
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals("Gross Weight for tariff 8215.99.3500 is 100", 100m, entryLines.Find(x => x.CL_AdValoremTariff == "8215993500").OfType<ICusEntryLine>().FirstOrDefault().GrossWeightInKilograms);

			invoiceLine.SupTariffFormatted = "9903.01.20";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals("Gross Weight for tariff 9903.01.20 is 100", 100m, entryLines.Find(x => x.CL_AdValoremTariff == "99030120").OfType<ICusEntryLine>().FirstOrDefault().GrossWeightInKilograms);

			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.15";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals("Gross Weight for tariff 9903.88.15 is 100", 100m, entryLines.Find(x => x.CL_AdValoremTariff == "99038815").OfType<ICusEntryLine>().FirstOrDefault().GrossWeightInKilograms);
		}

		public void TestCalculateSupCustomsValue()
		{
			#region Setup Tariff

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8215993500", "7", 0.068m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038815", "7", 0.075m, "");

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "8215.99.3500";
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.15";
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals("US_SupCustomsValue is zero for tariff 8215.99.3500", 0m, entryLines.Find(x => x.CL_AdValoremTariff == "8215993500").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);
			AssertEquals("US_SupCustomsValue is zero for tariff 9903.01.20", 0m, entryLines.Find(x => x.CL_AdValoremTariff == "99030120").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);
			AssertEquals("US_SupCustomsValue is zero for tariff 9903.88.15", 0m, entryLines.Find(x => x.CL_AdValoremTariff == "99038815").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);

			invoiceLine.US_SupGoodsValue = 500m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals("US_SupCustomsValue is zero for tariff 8215.99.3500", 0m, entryLines.Find(x => x.CL_AdValoremTariff == "8215993500").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);
			AssertEquals("US_SupCustomsValue is zero for tariff 9903.01.20", 500m, entryLines.Find(x => x.CL_AdValoremTariff == "99030120").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);
			AssertEquals("US_SupCustomsValue is zero for tariff 9903.88.15", 0m, entryLines.Find(x => x.CL_AdValoremTariff == "99038815").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);

			invoiceLine.US_SupAdditionalTariff1GoodsValue = 1000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLines = declaration.FormalEntry.MergedLines;
			AssertEquals("US_SupCustomsValue is zero for tariff 8215.99.3500", 0m, entryLines.Find(x => x.CL_AdValoremTariff == "8215993500").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);
			AssertEquals("US_SupCustomsValue is zero for tariff 9903.01.20", 500m, entryLines.Find(x => x.CL_AdValoremTariff == "99030120").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);
			AssertEquals("US_SupCustomsValue is zero for tariff 9903.88.15", 1000m, entryLines.Find(x => x.CL_AdValoremTariff == "99038815").OfType<CusEntryLine>().FirstOrDefault().US_SupCustomsValue);
		}

		protected override Customs.Business.BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				JobDeclaration result = Factory.New<JobDeclaration>();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				result.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				result.US_EnableENS = true;
				return result;
			}
		}

		protected override bool RatesAreReciprocal => true;

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection);

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}

		JobDeclaration CreateTIBDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		void AssertResult(JobComInvoiceLine line, ZDecimal quantity1, ZDecimal quantity2, ZDecimal quantity3)
		{
			CusEntryLine entryLine = line.GetMatchingSupEntryLine(line.US_SupTariff);

			IDutyData dutyData = entryLine;
			AssertEquals("Quantity1", quantity1, dutyData.Quantity1);
			AssertEquals("Quantity2", quantity2, dutyData.Quantity2);
			AssertEquals("Quantity3", quantity3, dutyData.Quantity3);

			AssertEquals("JI_CustomsQuantity empty", ZDecimal.Zero, line.JI_CustomsQuantity);
			AssertEquals("JI_CustomsSecondQuantity empty", ZDecimal.Zero, line.JI_CustomsSecondQuantity);
			AssertEquals("JI_CustomsThirdQuantity empty", ZDecimal.Zero, line.JI_CustomsThirdQuantity);
		}

		void SetupTariff99038842()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffHelper = new USCTariffTestCase(Factory);
			var startDate = new ZDate(2016, 01, 01);
			var endDate = new ZDate(2079, 01, 01);

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();
			var tariff73 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "6307909870", startDate, endDate);
			var tariff9903 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038001", startDate, endDate);
			var tariff99038842 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99038842", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("6307909870", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038001", startDate, endDate, "KG", "", "", "");
			tariffHelper.CreateNewTariffIfNotExists("99038842", startDate, endDate, "KG", "", "", "");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			Factory.Save();
			var rate = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate, "0.25");
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.HongKong, "HK", startDate, endDate);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.HongKong, startDate, endDate);
			var applicability = helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate);
			var relationship = helper.CreateTariffRelationship(tariff9903.PK, hsnTariffType.PK, "73");
			var tariffAttribute = helper.CreateTariffAttribute("RULE", "A99", tariff9903);
			Factory.Save();
		}

		ISEAdditionalData GetAction(JobDeclaration declaration)
		{
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, Common.US.ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			return ACEEntrySummaryMessageSendingOption.New(action);
		}

		OrgHeader warehouseOrg;
		OrgHeader WarehouseOrg
		{
			get
			{
				if (warehouseOrg == null)
				{
					warehouseOrg = Factory.New<OrgHeader>();
					warehouseOrg.FillWithValidTestData();
					warehouseOrg.OH_FullName = "Warehouse Organisation";
					warehouseOrg.OH_IsWarehouseClient = true;
					warehouseOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "887766554433");
				}
				return warehouseOrg;
			}
		}

		OrgHeader testOrg;
		OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>();
					testOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123456789012");
				}
				return testOrg;
			}
		}

		void AssertCommodityLine(JobComInvoiceLine invoiceLine, IAESTIRCommodityLineItem lineItem)
		{
			AssertEquals("ExportInformationCode", invoiceLine.US_ExportCode, lineItem.ExportInformationCode);
			AssertEquals("LineNumber", invoiceLine.JI_LineNo, lineItem.LineNumber);
			AssertEquals("CommodityDescription", invoiceLine.JI_Description, lineItem.CommodityDescription);
			AssertEquals("LicenseCodeLicenseExemptionCode", invoiceLine.US_LicenseType, lineItem.LicenseCodeLicenseExemptionCode);
			AssertEquals("ForeignDomesticOriginIndicator", invoiceLine.US_AESOriginIndicator, lineItem.ForeignDomesticOriginIndicator);

			AssertEquals("ScheduleBHTSNumber", invoiceLine.JI_Tariff, lineItem.ScheduleBHTSNumber);
			AssertEquals("UnitOfMeasure1", invoiceLine.JI_CustomsUnitQty, lineItem.UnitOfMeasure1);
			ZDecimal expectedResult = ExpectedRoundedValue(invoiceLine.JI_CustomsQuantity);
			AssertEquals("Quantity1", expectedResult, lineItem.Quantity1);
			expectedResult = ExpectedRoundedValue(invoiceLine.JI_LinePriceInLocalCurrency);
			AssertEquals("ValueOfGoods", expectedResult, lineItem.ValueOfGoods);
			AssertEquals("UnitOfMeasure2", invoiceLine.JI_CustomsSecondUnitQty, lineItem.UnitOfMeasure2);
			expectedResult = ExpectedRoundedValue(invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("Quantity2", expectedResult, lineItem.Quantity2);
			expectedResult = ExpectedRoundedValue(invoiceLine.JI_Weight);
			AssertEquals("ShippingWeight", expectedResult, lineItem.ShippingWeight);
			AssertEquals("ExportControlClassificationNumberECCN", invoiceLine.US_ECCN, lineItem.ExportControlClassificationNumberECCN);
			AssertEquals("ExportLicenseNumberCFRCitationAuthorizationSymbolKCP", invoiceLine.US_LicenseNo, lineItem.ExportLicenseNumberCFRCitationAuthorizationSymbolKCP);
			AssertEquals(invoiceLine.US_LicenseValue.Round(0), lineItem.LicenseValue);

			AssertEquals("DDTCITARExemptionNumber", invoiceLine.US_DDTCITARExemptionNo, lineItem.DDTCITARExemptionNumber);
			AssertEquals("DDTCRegistrationNumber", invoiceLine.US_DDTCRegistrationNo, lineItem.DDTCRegistrationNumber);
			AssertEquals("DDTCSignificantMilitaryEquipmentSMEIndicator", invoiceLine.US_DDTCMilitaryEquipmentIndicator, lineItem.DDTCSignificantMilitaryEquipmentSMEIndicator);
			AssertEquals("DDTCEligiblePartyCertificationIndicator", invoiceLine.US_DDTCPartyCertificationIndicator, lineItem.DDTCEligiblePartyCertificationIndicator);
			AssertEquals("DDTCUSMLCategoryCode", invoiceLine.US_DDTCUSMLCategoryCode, lineItem.DDTCUSMLCategoryCode);
			AssertEquals("DDTCUnitOfMeasureCode", invoiceLine.US_DDTCUnit, lineItem.DDTCUnitOfMeasureCode);
			AssertEquals("DDTCUnitOfMeasureCode", invoiceLine.US_JurisdictionNumber, lineItem.DDTCCommodityJurisdictionNumber);
			expectedResult = ExpectedRoundedValue(invoiceLine.US_DDTCQuantity);
			AssertEquals("DDTCQuantity", expectedResult, lineItem.DDTCQuantity);
			List<IAESTIRUsedVehicle> usedVehicles = new List<IAESTIRUsedVehicle>(lineItem.UsedVehicles);
			if (invoiceLine.US_IsUsedVehicle)
			{
				AssertEquals(invoiceLine.US_IsUsedVehicle ? 1 : 0, usedVehicles.Count);
				AssertUsedVehicle(invoiceLine, usedVehicles[0]);
			}
			else
			{
				AssertEquals(0, usedVehicles.Count);
			}
		}

		ZDecimal ExpectedRoundedValue(ZDecimal field) => field > 0 && field < 0.5m ? 1 : field.Round(0);

		void AssertUsedVehicle(JobComInvoiceLine invoiceLine, IAESTIRUsedVehicle usedVehicle)
		{
			AssertEquals("VehicleIdentificationNumberVINProductID", invoiceLine.US_VehicleID, usedVehicle.VehicleIdentificationNumberVINProductID);
			AssertEquals("VehicleIDQualifier", invoiceLine.US_VehicleIDType, usedVehicle.VehicleIDQualifier);
			AssertEquals("VehicleTitleNumber", invoiceLine.US_VehicleTitleNo, usedVehicle.VehicleTitleNumber);
			AssertEquals("VehicleTitleStateCode", invoiceLine.US_VehicleTitleState, usedVehicle.VehicleTitleStateCode);
		}

		void SetUpTariffsForDOT()
		{
			USCTariff tariffDT1 = Factory.New<USCTariff>();
			tariffDT1.UE_Tariff = "9813123211";
			tariffDT1.UE_Unit1 = "KG";
			tariffDT1.UE_OGACodes = "DT1";
			tariffDT1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariffDT1.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariffNoDT = Factory.New<USCTariff>();
			tariffNoDT.UE_Tariff = "9813123212";
			tariffNoDT.UE_Unit1 = "KG";
			tariffNoDT.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariffNoDT.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariffDT2 = Factory.New<USCTariff>();
			tariffDT2.UE_Tariff = "8965123211";
			tariffDT2.UE_Unit1 = "KG";
			tariffDT2.UE_OGACodes = "DT2";
			tariffDT2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariffDT2.UE_DateTo = ZDateTime.Today.AddYears(1);

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "7632585211";
			tariff3.UE_Unit1 = "KG";
			tariff3.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);
		}
	}
}
