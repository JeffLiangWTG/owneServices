using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class RefundControlSheetDocumentWrapperTest : DA63DocumentWrapperTest
	{
		public void TestHasRefundLinesReturnsExpectedCount()
		{
			var wrapper = new RefundControlSheetDocumentWrapper(entryHeader);
			AssertEquals(false, wrapper.HasRefundLines);
			SetupDataForRefundControlSheetWrapper(Factory);
			wrapper = new RefundControlSheetDocumentWrapper(entryHeader);
			AssertEquals(4, entryHeader.MergedLines.Count);
			AssertEquals(true, wrapper.HasRefundLines);
			AssertEquals(2, wrapper.RefundEntryLines.Count);
		}

		public void TestRefundControlSheetRounding()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.GetCultureInfo("en-ZA")))
			{
				SetupDataForRefundControlSheetWrapper(Factory);
				invoiceLine1.JI_ImportDutyPaid = 1.2345;
				invoiceLine2.JI_ImportDutyPaid = 1.5678;
				invoiceLine1.JI_ImportVATPaid = 2.2345;
				invoiceLine2.JI_ImportVATPaid = 2.5678;
				var refundControlSheetWrapper = new RefundControlSheetDocumentWrapper(entryHeader);
				CombineAssertions(() =>
				{
					AssertEquals("CustomsDuty round down", "1.23", refundControlSheetWrapper.RefundEntryLines[0].CustomsDuty);
					AssertEquals("CustomsDuty round up", "1.57", refundControlSheetWrapper.RefundEntryLines[1].CustomsDuty);
					AssertEquals("VAT round down", "2.23", refundControlSheetWrapper.RefundEntryLines[0].Vat);
					AssertEquals("VAT round up", "2.57", refundControlSheetWrapper.RefundEntryLines[1].Vat);
				});
			}
		}

		[TestDate(2011, 01, 11)]
		public void TestRefundControlSheetWrapperData()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.GetCultureInfo("en-ZA")))
			{
				SetupDataForRefundControlSheetWrapper(Factory);
				var refundControlSheetWrapper = new RefundControlSheetDocumentWrapper(entryHeader);
				CombineAssertions("Refund Control Sheet wrapper should have 2 rebate lines", () =>
				{
					AssertEquals(2, refundControlSheetWrapper.RefundEntryLines.Count);
					declaration.JE_OH_AgentOverride = ZGuid.Missing;
					AssertEquals("ApplicantName - where the Misc tab Agent is null", "", refundControlSheetWrapper.ApplicantsName);
					OrgHeader agentOrg = Factory.New<OrgHeader>();
					agentOrg.OH_FullName = "AGENTNAME";
					declaration.JE_OH_AgentOverride = agentOrg.PK;
					AssertEquals("ApplicantName - where the Misc tab Agent is set", "AGENTNAME", refundControlSheetWrapper.ApplicantsName);
					AssertEquals("ApplicantsReferenceNumber", "Reference1", refundControlSheetWrapper.ApplicantsReferenceNumber);
					AssertEquals("", refundControlSheetWrapper.RefundEntryLines[0].ImportLineNo);
					AssertEquals("AAA", refundControlSheetWrapper.RefundEntryLines[0].DistrictOfficeCode);
					AssertEquals("1.01", refundControlSheetWrapper.RefundEntryLines[0].CustomsDuty);
					AssertEquals("1.03", refundControlSheetWrapper.RefundEntryLines[0].DutySchedule1Part2B);
					AssertEquals("1.04", refundControlSheetWrapper.RefundEntryLines[0].Vat);
					AssertEquals("3.13", refundControlSheetWrapper.RefundEntryLines[0].Other);
					AssertEquals("TESTMRN", refundControlSheetWrapper.RefundEntryLines[0].ExportDeclarationNumber);
					AssertEquals("11-Jan-11", refundControlSheetWrapper.RefundEntryLines[0].ExportDeclarationDate);
					AssertEquals("1", refundControlSheetWrapper.RefundEntryLines[0].ExportLineNo);
					AssertEquals("", refundControlSheetWrapper.RefundEntryLines[1].ImportLineNo);
					AssertEquals("BBB", refundControlSheetWrapper.RefundEntryLines[1].DistrictOfficeCode);
					AssertEquals("2.01", refundControlSheetWrapper.RefundEntryLines[1].CustomsDuty);
					AssertEquals("2.03", refundControlSheetWrapper.RefundEntryLines[1].DutySchedule1Part2B);
					AssertEquals("2.04", refundControlSheetWrapper.RefundEntryLines[1].Vat);
					AssertEquals("6.13", refundControlSheetWrapper.RefundEntryLines[1].Other);
					AssertEquals("TESTMRN", refundControlSheetWrapper.RefundEntryLines[1].ExportDeclarationNumber);
					AssertEquals("11-Jan-11", refundControlSheetWrapper.RefundEntryLines[1].ExportDeclarationDate);
					AssertEquals("2", refundControlSheetWrapper.RefundEntryLines[1].ExportLineNo);
				});
			}
		}

		public void SetupDataForRefundControlSheetWrapper(BusinessObjectFactory factory)
		{
			CreateInvoiceLinesToTestRefundControlSheetWrapper(factory);
			var broker = factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			declaration.JE_DeclarationReference = "Reference1";
			entryHeader.MovementReferenceNumberSetter("TESTMRN", ZDate.Today);
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.RefundRebateCode = "52203111";
			invoiceLine1.JI_PreviousEntryNumber = "AAAPREVIOUSMRN1";
			invoiceLine1.JI_ImportDutyPaid = 1.01;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12A", 1.02m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 1.03m);
			invoiceLine1.JI_ImportVATPaid = 1.04;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 1.05m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 1.06m);
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.RefundRebateCode = "53600222";
			invoiceLine2.JI_PreviousEntryNumber = "BBBPREVIOUSMRN2";
			invoiceLine2.JI_ImportDutyPaid = 2.01;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("12A", 2.02m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 2.03m);
			invoiceLine2.JI_ImportVATPaid = 2.04;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 2.05m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 2.06m);
			AssertEquals("pre-req", true, invoiceLine1.IsRefundRebateTariff);
			AssertEquals("pre-req", true, invoiceLine2.IsRefundRebateTariff);
			AssertEquals("pre-req", false, invoiceLine3.IsRefundRebateTariff);
			AssertEquals("pre-req", true, invoiceLine4.IsRefundRebateTariff);
			AssertEquals("pre-req", true, invoiceLine1.IsRefundRebateType5P);
			AssertEquals("pre-req", true, invoiceLine2.IsRefundRebateType5P);
			AssertEquals("pre-req", false, invoiceLine3.IsRefundRebateType5P);
			AssertEquals("pre-req", false, invoiceLine4.IsRefundRebateType5P);
		}

		void CreateInvoiceLinesToTestRefundControlSheetWrapper(BusinessObjectFactory factory)
		{
			var helper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType5P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P1");
			var tariffType5P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P2");
			var rateType_ZA_REB = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Enterprise.Customs.Universal.Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_REB.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			factory.Save();
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
			factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability1 = helper.CreateCusApplicability(tariff1Rate, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2070, 06, 06));
			factory.Save();
			declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "5#";
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Rebate line 1 - Type=5P1, Tariff= 5220311111";
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine1.JI_Tariff = "5220311111";
			invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine1.RefundRebateCode = "5220311111";
			invoiceLine1.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine1.CusLineTariffDetails.AddNew("5P1", "5220311111");
			invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Rebate line 2 - Type=5P2, Tariff= 5360022222";
			invoiceLine2.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine2.JI_Tariff = "5360022222";
			invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine2.RefundRebateCode = "5360022222";
			invoiceLine2.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine2.CusLineTariffDetails.AddNew("5P2", "5360022222");
			invoiceLine3 = invoice1.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "Non rebate line";
			invoiceLine3.JI_CEI = entryInstruction1.PK;
			invoiceLine3.JI_Procedure = "";
			invoiceLine4 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Description = "Wrong type for a rebate but tariff ok - Type=1P1, Tariff= 5360033333";
			invoiceLine4.JI_CEI = entryInstruction1.PK;
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine4.JI_Tariff = "5360033333";
			invoiceLine4.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine4.RefundRebateCode = "53600333333";
			invoiceLine4.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine4.CusLineTariffDetails.AddNew("1P1", "5360033333");
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			entryHeader = declaration.ActiveEntryHeaders[0];
			entryLine1 = invoiceLine1.CusEntryLine;
			entryLine2 = invoiceLine2.CusEntryLine;
			entryLine3 = invoiceLine3.CusEntryLine;
		}

		JobComInvoiceLine invoiceLine4;
	}
}
