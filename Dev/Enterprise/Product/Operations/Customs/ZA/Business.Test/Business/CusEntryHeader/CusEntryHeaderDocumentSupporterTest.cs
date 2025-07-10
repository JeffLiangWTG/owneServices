using System;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	sealed class CusEntryHeaderDocumentSupporterTest : Customs.Business.Testing.CusEntryHeaderDocumentSupportTest
	{
		public void TestGetBODocDataProviders()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure("ZA", "T", "XX", "YY", "5", "XXYY5", "EXP");
			testHelper.CreateRefCusProcedure("ZA", "T", "XX", "ZZ", "", "XXZZ", "EXP");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			var providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null);
			AssertEquals("Provider for CUSDECCUSRESMessagePair", 1, providers.Length);
			providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null);
			AssertEquals("Provider for SADDocumentPack", 1, providers.Length);
			providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null);
			AssertEquals("Provider for VOCDocumentPack", 1, providers.Length);
			providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document), null);
			AssertEquals("Provider for DA63Document", 0, providers.Length);
			providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.RefundControlSheetDocument), null);
			AssertEquals("Provider for RefundControlSheet not found because we have no data to print", 0, providers.Length);
			providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.DA74Document), null);
			AssertEquals("Provider for DA74Document", 1, providers.Length);
			providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument), null);
			AssertEquals("Provider for VOCRefundDocumentPack", 1, providers.Length);
			header = GetEntryHeaderForRefundControlSheetWrapper();
			providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.RefundControlSheetDocument), null);
			AssertEquals("Provider for RefundControlSheet has data to print", 1, providers.Length);
		}

		CusEntryHeader GetEntryHeaderForRefundControlSheetWrapper()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType5P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P1");
			var tariffType5P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P2");
			var rateType_ZA_REB = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Customs.Universal.Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "", "5", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "5#", "0", "5", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101011", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P1.PK, "5220311111", startDate, endDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P2.PK, "5360022222", startDate, endDate);
			var relationship1 = helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			var rate1 = helper.CreateRate(tariff2, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,3):""Question For Testing""\}");
			Factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability1 = helper.CreateCusApplicability(tariff1Rate, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "5#";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Rebate line 1 - Type=5P1, Tariff= 5220311111";
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine1.JI_Tariff = "5220311111";
			invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine1.RefundRebateCode = "5220311111";
			invoiceLine1.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine1.CusLineTariffDetails.AddNew("5P1", "5220311111");
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Rebate line 2 - Type=5P2, Tariff= 5360022222";
			invoiceLine2.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine2.JI_Tariff = "5360022222";
			invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine2.RefundRebateCode = "5360022222";
			invoiceLine2.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine2.CusLineTariffDetails.AddNew("5P2", "5360022222");
			var invoiceLine3 = invoice1.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "Non rebate line";
			invoiceLine3.JI_CEI = entryInstruction1.PK;
			invoiceLine3.JI_Procedure = "";
			var invoiceLine4 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Description = "Wrong type for a rebate but tariff ok - Type=1P1, Tariff= 5360033333";
			invoiceLine4.JI_CEI = entryInstruction1.PK;
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine4.JI_Tariff = "5360033333";
			invoiceLine4.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine4.RefundRebateCode = "53600333333";
			invoiceLine4.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine4.CusLineTariffDetails.AddNew("1P1", "5360033333");
			AssertEquals("pre-req", true, invoiceLine1.IsRefundRebateTariff);
			AssertEquals("pre-req", true, invoiceLine2.IsRefundRebateTariff);
			AssertEquals("pre-req", false, invoiceLine3.IsRefundRebateTariff);
			AssertEquals("pre-req", true, invoiceLine4.IsRefundRebateTariff);
			AssertEquals("pre-req", true, invoiceLine1.IsRefundRebateType5P);
			AssertEquals("pre-req", true, invoiceLine2.IsRefundRebateType5P);
			AssertEquals("pre-req", false, invoiceLine3.IsRefundRebateType5P);
			AssertEquals("pre-req", false, invoiceLine4.IsRefundRebateType5P);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			return declaration.ActiveEntryHeaders[0];
		}

		public void TestCustomWatermarkText()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var collection = new SADDocumentWatermarkCollection();
			var watermark = collection.AddNew();
			watermark.EntryStatusCode = "1";
			ZACustomsRegistry.Instance.SADDocumentPackWatermarks.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, collection);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "1";
			var supporter = entryHeader.DocumentSupporter;
			var context = new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack);
			var docCommand = Factory.New<DocumentCommand>();
			docCommand.SU_MenuName = "Customs Declaration Response";
			AssertNull("Custom Watermark should be null", supporter.GetCustomWatermarkText(docCommand, supporter.GetBODocDataProviders(context, docCommand)[0]));
			docCommand.SU_MenuName = "SAD Document Pack";
			AssertEquals("Custom Watermark should be set", "Release", supporter.GetCustomWatermarkText(docCommand, supporter.GetBODocDataProviders(context, docCommand)[0]));
		}

		public override void TestGetBODocDataProvidersNotFoundMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_BGMReference = "BGM1";
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "BGM2";
			var testingContext = new DataContextValue(CusEntryHeaderDocumentSupporter.DA63Document);
			AssertEquals("Entry Header BGM1 doesn't contain any DA63 Entry Line", entryHeader1.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
			AssertEquals("Entry Header BGM2 doesn't contain any DA63 Entry Line", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
			testingContext = new DataContextValue(CusEntryHeaderDocumentSupporter.RefundControlSheetDocument);
			AssertEquals("Entry Header BGM1 doesn't contain any Refund Entry Lines", entryHeader1.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
			AssertEquals("Entry Header BGM2 doesn't contain any Refund Entry Lines", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(testingContext, null));
			AssertEquals("Entry Header cannot be found.", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair), null));
			AssertEquals("Entry Header cannot be found.", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack), null));
			AssertEquals("Entry Header cannot be found.", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack), null));
			AssertEquals("Entry Header cannot be found.", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.DA74Document), null));
			AssertEquals("Entry Header cannot be found.", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument), null));
			AssertEquals("Entry Header cannot be found.", entryHeader2.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(CusEntryHeaderDocumentSupporter.RefundWorkSheet), null));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => GetEntryHeaderForRefundControlSheetWrapper();

		protected override ZString GetMainNameSpace() => "Enterprise.Customs.ZA.Business.DocumentWrappers.";
	}
}
