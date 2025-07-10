using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed partial class JobComInvoiceLineTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public void TestCustomsUnitDefaultingStrategy()
		{
			AssertType<UniversalTariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(typeof(BaseJobComInvoiceLine).GetProperty("CustomsUnitDefaultingStrategy", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(invoiceLine));
		}

		public void TestCustomsCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("CustomsCountryCodeCore should be ZA", Core.Constants.CountryCodes.SouthAfrica, invoiceLine.CustomsCountryCode);
			}
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IInvoiceLinePartDetails partDetails = Factory.NewMoq<JobComInvoiceLine>().Object;
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, partDetails.CustomsCountryCode);
				AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
			}
		}

		public void TestRatesSelectionCriteria()
		{
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 12, 10, 0, 0, 0);
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = "1234";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_SecondaryPreference = "01";
			AssertEquals("", invoiceLine.JI_ConcessionOrder);
			var rateSelectionCriteria = invoiceLine.AllApplicableRatesSelectionCriteria;
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", Core.Constants.CountryCodes.UnitedStates, rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("DataGrouping", GlbCompany.CurrentCompany.Country.Code, rateSelectionCriteria.DataGrouping);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCodes", rateSelectionCriteria.AdditionalCodes.Contains(""));
			AssertEquals("ConcessionOrder", "", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", "", rateSelectionCriteria.RateType);
			AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);
			invoiceLine.JI_PrimaryPreference = "400";
			AssertEquals("QUOTA", invoiceLine.JI_ConcessionOrder);
			rateSelectionCriteria = invoiceLine.AllApplicableRatesSelectionCriteria;
			AssertRatesSelectionCriteria(rateSelectionCriteria, rateSelectionCriteria.RateType);
			rateSelectionCriteria = invoiceLine.DutyRateSelectionCriteria;
			AssertRatesSelectionCriteria(rateSelectionCriteria, Constants.RateTypes.Duty);
			rateSelectionCriteria = invoiceLine.RebateRateSelectionCriteria;
			AssertRatesSelectionCriteria(rateSelectionCriteria, Constants.RateTypes.Rebate);
			rateSelectionCriteria = invoiceLine.ExciseRateSelectionCriteria;
			AssertRatesSelectionCriteria(rateSelectionCriteria, Constants.RateTypes.Excise);
			rateSelectionCriteria = invoiceLine.AdValoremExciseRateSelectionCriteria;
			AssertRatesSelectionCriteria(rateSelectionCriteria, Constants.RateTypes.AdValoremExcise);
			rateSelectionCriteria = invoiceLine.AntiDumpingRateSelectionCriteria;
			AssertRatesSelectionCriteria(rateSelectionCriteria, Constants.RateTypes.AntiDumping);
			rateSelectionCriteria = invoiceLine.GetSpecificRateSelectionCriteria("2P1");
			AssertRatesSelectionCriteria(rateSelectionCriteria, rateSelectionCriteria.RateType, "2P1");
		}

		void AssertRatesSelectionCriteria(IZZRateSelectionCriteria criteria, ZString rateType, string rateCode = "")
		{
			AssertEquals(nameof(criteria.RateType), rateType, criteria.RateType);
			AssertEquals(nameof(criteria.PrimaryPreference), "400", criteria.PrimaryPreference);
			AssertEquals(nameof(criteria.ConcessionOrder), ZString.Empty, criteria.ConcessionOrder);
			if (!rateCode.IsNullOrEmpty())
			{
				AssertEquals(nameof(criteria.RateCode), rateCode, criteria.RateCode);
			}
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var provider = invoiceLine as ICusCodeDataTypeSupporter;
			AssertEquals(typeof(DA63AdditionalDuty), provider.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.DA63AdditionalDuty]);
			AssertType<Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy>(invoiceLine.GetFetchStrategies().Single());
		}

		public void TestGetGSTVATAmountIncludingLCOnly()
		{
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			AssertEquals(ZDecimal.Zero, invoiceLine.GetGSTVATAmountIncludingLCOnly());
			SetupInvoiceLineFeesForGSTVATAmountAndDutySch1P2BAmount(invoiceLine);
			AssertEquals(40m, invoiceLine.GetGSTVATAmountIncludingLCOnly());
		}

		public void TestGetDutySch1P2BAmountIncludingLCOnly()
		{
			AssertEquals(ZDecimal.Zero, invoiceLine.GetDutySch1P2BAmountIncludingLCOnly());
			SetupInvoiceLineFeesForGSTVATAmountAndDutySch1P2BAmount(invoiceLine);
			AssertEquals(20m, invoiceLine.GetDutySch1P2BAmountIncludingLCOnly());
		}

		void SetupInvoiceLineFeesForGSTVATAmountAndDutySch1P2BAmount(JobComInvoiceLine invoiceLine)
		{
			var instruction = Factory.NewMoq<CusEntryInstruction>().Object;
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._40;
			var cusEntryLine = Factory.NewMoq<CusEntryLine>().Object;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = cusEntryLine.PK;
			AddFee("DUT", 10m, isLandedCostOnly: true);
			AddFee("12B", 20m, isLandedCostOnly: true);
			AddFee("VAT", 40m, isLandedCostOnly: true);
		}

		public void TestASNRefresh()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);
			var config1 = collection.AddNew();
			config1.FieldType = DefaultOptions.Codes.Classification;
			var config2 = collection.AddNew();
			config2.FieldType = DefaultOptions.Codes.CountryOfOrigin;
			CustomsDataRegistry.Instance.ASNRefreshOptions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var invoice = Factory.NewMoq<JobComInvoiceHeader>().Object;
			var importer = Factory.NewMoq<OrgHeader>().Object;
			importer.OH_Code = "IMPTEST";
			invoice.JZ_OH_Buyer = importer.PK;
			var supplier = Factory.NewMoq<OrgHeader>().Object;
			supplier.OH_Code = "SUPTEST";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			var line = invoice.InvoiceLines.AddNew();
			var classification = Factory.NewMoq<CusClassification>().Object;
			classification.FillWithValidTestData();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "AMYTEST";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_RN_NKCountryOfOrigin = "CA";
			var relatedOrg1 = product.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = importer.PK;
			relatedOrg1.OU_Relationship = "OWN";
			var relatedOrg2 = product.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = supplier.PK;
			relatedOrg2.OU_Relationship = "SUP";
			line.JI_PartNo = product.OP_PartNum;
			line.JI_OP = product.PK;
			AssertEquals(pivot, line.Pivot);

			var declaration = Factory.NewMoq<JobDeclaration>().Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the registry", pivot.CI_CC, line.JI_CC);
			AssertEquals("Refreshed by the registry", pivot.CI_RN_NKCountryOfOrigin, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.JI_Tariff);
			var companyData = importer.CompanyData;
			companyData.ImporterOverride = true;
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,PREFF";
			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			line.JI_CC = ZGuid.Empty;
			line.JI_CountryOfOrigin = ZString.Empty;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the consignee config", ZGuid.Empty, line.JI_CC);
			AssertEquals("Refreshed by the consignee config", ZString.Empty, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the consignee config", pivot.CI_TariffNum, line.JI_Tariff);
		}

		void AssertWipeNKTaxType(ZBool shouldWipeNKTaxType, ZString expectedNKTaxType)
		{
			AssertEquals("Should Wipe NKTaxType", shouldWipeNKTaxType, invoiceLine.ShouldWipeNKTaxType);
			AssertEquals("Expected NKtaxType", expectedNKTaxType, invoiceLine.JI_ZZF_NKTaxType);
		}

		[ExpectNoExceptions]
		public void TestJI_ProcedureMaxLength()
		{
			invoiceLine.JI_Procedure = "1234567";
			AssertEquals(4, invoiceLine.JI_ProcedureInfo.MaxLength);
			AssertEquals("1234", invoiceLine.JI_Procedure);
		}

		public void TestJI_CEI_ReadOnly()
		{
			var standaloneInv = Factory.New<BaseJobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(standaloneInv);
			Assert("Prereq: Fake declaration, i.e. Commercial Invoice", !standaloneInv.IsAttachedToPersistentDeclaration);
			var standaloneInvLine = standaloneInv.InvoiceLines.AddNew();
			standaloneInvLine.JI_CEI = ZGuid.BrettsGuid;
			AssertEquals(expected: true, standaloneInvLine.JI_CEIInfo.ReadOnly);
		}

		public void TestJI_PreviousEntryNumberDefaultUsingEntryInstruction()
		{
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			instruction.CEI_PreviousMRN = "PREMRN";
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals("Previous Entry Number when empty on invoice line", instruction.CEI_PreviousMRN, invoiceLine.JI_PreviousEntryNumber);
			invoiceLine.JI_PreviousEntryNumber = "PREMRNLIN";
			AssertNotEquals("Previous Entry Number when filled on invoice line", instruction.CEI_PreviousMRN, invoiceLine.JI_PreviousEntryNumber);
		}

		[TestDate(2016, 8, 4)]
		public void TestJI_Description()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			tariffType1P1.ZZI_ZZ9_NKNomenclatureGroupType = UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
			var startDate = new ZDateTime(1900, 01, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var query = new ZQuery().AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica).AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, "STANDARD");
			var tradeGroup = Factory.LoadTop1<CusRefTradeGroupView>(query);
			if (tradeGroup == null)
			{
				universalReferenceDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			}
			Factory.Save();

			var longTariffDesc = "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.";
			var longResultDesc1 = "Spark Ignition reciprocating or rotary internal combustion piston engine Reciprocating piston engines of a kind used for the propulsion of vehicles of Chapter 87 The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog.";
			var longResultDesc2 = "Spark Ignition reciprocating or rotary internal combustion piston engine Reciprocating piston engines of a kind used for the propulsion of vehicles of Chapter 87 The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog. " + "The qui VIN; T1 Engine No.; T2 Make; T3 Model; 4 Veh. Format; OTH Veh. Type; 6 Colour; 7 YoM; 2016";
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "840734", startDate, endDate, description: longTariffDesc, compositeKey: "16.84..07.3");
			universalReferenceDataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "84073", startDate, endDate, "Reciprocating piston engines of a kind used for the propulsion of vehicles of Chapter 87", compositeKey: "16.84..07.3", nomenclatureGroupType: UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "84072", ZDateTime.BrettsBirthday, ZDateTime.Today, "Marine propulsion engines", "16.84..07.2", nomenclatureGroupType: UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "8407", ZDateTime.BrettsBirthday, ZDateTime.Today, "Spark Ignition reciprocating or rotary internal combustion piston engine", "16.84..07", nomenclatureGroupType: UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			universalReferenceDataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "", startDate, endDate, "Nuclear reactors, boilers, machinery and mechanical applicances; Parts thereof", compositeKey: "16.84", nomenclatureGroupType: UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = "840734";
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			Factory.Save();
			AssertEquals("Nomenclature Data Added", longResultDesc1, invoiceLine.JI_Description);
			invoiceLine.JI_VIN = "T1";
			invoiceLine.JI_EngineNumber = "T2";
			invoiceLine.JI_Make = "T3";
			invoiceLine.JI_Model = "4";
			invoiceLine.JI_VehicleFormat = "OTH";
			invoiceLine.JI_VehicleType = "6";
			invoiceLine.JI_Colour = "7";
			invoiceLine.JI_YearOfManufacture = "2016";
			Factory.Save();
			DoMerge(declaration);
			AssertEquals("Nomenclature Data Added", longResultDesc2, invoiceLine.CusEntryLine.CL_Description);
		}

		[TestDate(2016, 01, 01)]
		public void TestJI_CustomsValue()
		{
			AssertJICustomsValueFOB();
			AssertJICustomsValueCIFNotInLine();
			AssertJICustomsValueCIFInLine();
		}

		void AssertJICustomsValueFOB()
		{
			CombineAssertions("FOB", () =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				invoiceHeader.JZ_IncoTerm = "FOB";
				invoiceHeader.JZ_InvoiceAmount = 400;
				invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				var testInvoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				testInvoiceLine1.JI_LinePrice = 200m;
				var testInvoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				testInvoiceLine2.JI_LinePrice = 200m;
				var testCharge2 = testInvoiceLine2.Charges.AddNew("INT", 20m);
				var testInvoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				testInvoiceLine3.JI_LinePrice = 200m;
				var testCharge3 = testInvoiceLine3.Charges.AddNew("DIS", 20m);
				var testInvoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
				testInvoiceLine4.JI_LinePrice = 200m;
				testInvoiceLine4.JI_ValuationMarkup = 10;
				var testInvoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
				testInvoiceLine5.JI_LinePrice = 200m;
				testInvoiceLine5.JI_CustomsValueOverride = 100m;
				testInvoiceLine5.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.SouthAfrica;
				declaration.ResumeApportionment();
				AssertEquals(200m, testInvoiceLine1.JI_CustomsValue);
				AssertEquals(180m, testInvoiceLine2.JI_CustomsValue);
				AssertEquals(180m, testInvoiceLine3.JI_CustomsValue);
				AssertEquals(220m, testInvoiceLine4.JI_CustomsValue);
				AssertEquals(100m, testInvoiceLine5.JI_CustomsValue);
			});
		}

		void AssertJICustomsValueCIFNotInLine()
		{
			CombineAssertions("CIF not in LINE", () =>
			{
				var testInvoice = declaration.Invoices.AddNew();
				testInvoice.JZ_IncoTerm = "CIF";
				testInvoice.JZ_InvoiceAmount = 400;
				testInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				testInvoice.Charges.RemoveAndDeleteAll();
				var headerCharge = testInvoice.Charges.AddNew("ONS", 100);
				var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine1.JI_LinePrice = 200m;
				var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine2.JI_LinePrice = 200m;
				var testCharge2 = testInvoiceLine2.Charges.AddNew("INT", 20m);
				var testInvoiceLine3 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine3.JI_LinePrice = 200m;
				var testCharge3 = testInvoiceLine3.Charges.AddNew("DIS", 20m);
				var testInvoiceLine4 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine4.JI_LinePrice = 100m;
				testInvoiceLine4.JI_ValuationMarkup = 10;
				var testInvoiceLine5 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine5.JI_LinePrice = 100m;
				testInvoiceLine5.JI_CustomsValueOverride = 50m;
				testInvoiceLine5.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.SouthAfrica;
				declaration.ResumeApportionment();
				AssertEquals(300m, testInvoice.JZ_Calc_FOBAmount);
				AssertEquals(400m, testInvoice.JZ_Calc_CIFAmount);
				AssertEquals(1m, testInvoice.JZ_Calc_ConversionFactor);
				AssertEquals(200m, testInvoiceLine1.JI_CustomsValue);
				AssertEquals(180m, testInvoiceLine2.JI_CustomsValue);
				AssertEquals(180m, testInvoiceLine3.JI_CustomsValue);
				AssertEquals(110m, testInvoiceLine4.JI_CustomsValue);
				AssertEquals(50m, testInvoiceLine5.JI_CustomsValue);
				AssertEquals(200m, testInvoiceLine1.JI_Calc_ActualPrice);
				AssertEquals(200m, testInvoiceLine2.JI_Calc_ActualPrice);
				AssertEquals(180m, testInvoiceLine3.JI_Calc_ActualPrice);
				AssertEquals(100m, testInvoiceLine4.JI_Calc_ActualPrice);
				AssertEquals(100m, testInvoiceLine5.JI_Calc_ActualPrice);
			});
		}

		void AssertJICustomsValueCIFInLine()
		{
			CombineAssertions("CIF in LINE", () =>
			{
				var testInvoice = declaration.Invoices.AddNew();
				testInvoice.JZ_IncoTerm = "CIF";
				testInvoice.JZ_InvoiceAmount = 400;
				testInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				testInvoice.Charges.RemoveAndDeleteAll();
				var headerCharge = testInvoice.Charges.AddNew("ONS", 100);
				headerCharge.J7_IsIncludedInITOT = true;
				var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine1.JI_LinePrice = 200m;
				var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine2.JI_LinePrice = 200m;
				var testCharge2 = testInvoiceLine2.Charges.AddNew("INT", 20m);
				var testInvoiceLine3 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine3.JI_LinePrice = 200m;
				var testCharge3 = testInvoiceLine3.Charges.AddNew("DIS", 20m);
				var testInvoiceLine4 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine4.JI_LinePrice = 100m;
				testInvoiceLine4.JI_ValuationMarkup = 10;
				var testInvoiceLine5 = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine5.JI_LinePrice = 100m;
				testInvoiceLine5.JI_CustomsValueOverride = 50m;
				testInvoiceLine5.JI_RX_NKCustomsValueCurrencyOverride = Core.Constants.CurrencyCodes.SouthAfrica;
				declaration.ResumeApportionment();
				AssertEquals(300m, testInvoice.JZ_Calc_FOBAmount);
				AssertEquals(400m, testInvoice.JZ_Calc_CIFAmount);
				AssertEquals(0.87179487m, testInvoice.JZ_Calc_ConversionFactor);
				AssertEquals(174.358974m, testInvoiceLine1.JI_CustomsValue);
				AssertEquals(156.9230766m, testInvoiceLine2.JI_CustomsValue);
				AssertEquals(156.9230766m, testInvoiceLine3.JI_CustomsValue);
				AssertEquals(95.89743570m, testInvoiceLine4.JI_CustomsValue);
				AssertEquals(50m, testInvoiceLine5.JI_CustomsValue);
				AssertEquals(174.358974m, testInvoiceLine1.JI_Calc_ActualPrice);
				AssertEquals(174.358974m, testInvoiceLine2.JI_Calc_ActualPrice);
				AssertEquals(156.9230766m, testInvoiceLine3.JI_Calc_ActualPrice);
				AssertEquals(87.179487m, testInvoiceLine4.JI_Calc_ActualPrice);
				AssertEquals(87.179487m, testInvoiceLine5.JI_Calc_ActualPrice);
			});
		}

		public void TestImportBOEntry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var basDeclaration = Factory.NewMoq<JobDeclaration>().Object;
				basDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var basInvoice = basDeclaration.Invoices.AddNew();
				var basInvoiceLine = basInvoice.InvoiceLines.AddNew();

				var imxDeclaration = Factory.NewMoq<JobDeclaration>().Object;
				imxDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
				var imxEntryInstruction = imxDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var imxInvoice = imxDeclaration.Invoices.AddNew();
				var imxInvoiceLine = imxInvoice.InvoiceLines.AddNew();
				imxInvoiceLine.JI_CEI = imxEntryInstruction.PK;
				var imxEntryHeader = imxDeclaration.ActiveEntryHeaders.AddNew();
				var entryNum = Factory.NewMoq<CusEntryNumber>().Object;
				entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				entryNum.CE_EntryNum = "TST201707111234567";
				entryNum.CE_ParentID = imxEntryHeader.PK;
				entryNum.CE_ParentTable = imxEntryHeader.TableName;
				Factory.Save();

				var zaCompany = Factory.NewMoq<GlbCompany>().Object;
				zaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Cambodia;
				var zaBranch = zaCompany.Branches.AddNew();
				zaBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica)).RL_Code;
				imxDeclaration.JE_GB = zaBranch.PK;
				Factory.Save();
				basInvoiceLine.JI_PreviousEntryNumber = "TST201707111234567";
				AssertNull(basInvoiceLine.ImportBOEntry);

				zaCompany = GlbCompany.CurrentCompany;
				zaBranch = zaCompany.Branches[0];
				imxDeclaration.JE_GB = zaBranch.PK;
				Factory.Save();
				AssertNotNull(basInvoiceLine.ImportBOEntry);
				AssertEquals(imxEntryHeader.PK, basInvoiceLine.ImportBOEntry.PK);

				entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Albania;
				Factory.Save();
				AssertNull(basInvoiceLine.ImportBOEntry);

				entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;

				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryNum.CE_ParentID = entryHeader.PK;
				Factory.Save();
				invoiceLine = imxInvoice.InvoiceLines.AddNew();
				invoiceLine.JI_PreviousEntryNumber = "TST201707111234567";
				AssertNotNull(basInvoiceLine.ImportBOEntry);
				AssertEquals(entryHeader.PK, basInvoiceLine.ImportBOEntry.PK);
				imxDeclaration.Delete();
				Factory.Save();
				AssertNotNull(basInvoiceLine.ImportBOEntry);
				AssertEquals(entryHeader.PK, basInvoiceLine.ImportBOEntry.PK);
				declaration.Delete();
				Factory.Save();
				AssertNull(basInvoiceLine.ImportBOEntry);
			}
		}

		public void TestImportBOEntryEXB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				var origEntryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = origEntryInstruction.PK;
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				var entryNum = Factory.NewMoq<CusEntryNumber>().Object;
				entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				entryNum.CE_EntryNum = "TST201910150000001";
				entryNum.CE_ParentID = entryHeader.PK;
				entryNum.CE_ParentTable = entryHeader.TableName;
				Factory.Save();
				var da63Dec = Factory.NewMoq<JobDeclaration>().Object;
				da63Dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				var da63Invoice = da63Dec.Invoices.AddNew();
				var da63InvoiceLine = da63Invoice.InvoiceLines.AddNew();
				AssertNull("ImportBOEntry should be null", da63InvoiceLine.ImportBOEntry);
				da63InvoiceLine.JI_PreviousEntryNumber = "TST201910150000001";
				AssertNotNull("ImportBOEntry should not be null", da63InvoiceLine.ImportBOEntry);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				entryNum.CE_EntryNum = "TST201910150000002";
				Factory.Save();
				AssertNull("ImportBOEntry should be null", da63InvoiceLine.ImportBOEntry);
				da63InvoiceLine.JI_PreviousEntryNumber = "TST201910150000002";
				AssertNull("ImportBOEntry should still be null", da63InvoiceLine.ImportBOEntry);
			}
		}

		[TestDate(1999, 9, 9, 9, 9, 0)]
		public void TestEffectiveAssessmentDate()
		{
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertEquals("invoiceLine.EffectiveAssessmentDate default to Now", ZDateTime.Now, invoiceLine.EffectiveAssessmentDate);
			entryInstruction.CEI_DateForDuty = new ZDateTime(1990, 1, 1);
			AssertEquals("invoiceLine.EffectiveAssessmentDate use the entry instruction date", invoiceLine.EffectiveAssessmentDate, new ZDateTime(1990, 1, 1));
		}

		public void TestCopyingDataToOwnerProduct()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			helper.SetupPartAttributesForOrganisation(helper.Importer, "VIN1", PartAttributeTypeList.Codes.VIN, "SERIAL", PartAttributeTypeList.Codes.Mandatory, "BATCH", PartAttributeTypeList.Codes.BatchNumber);
			helper.SetupPartAttributesForOrganisation(helper.Owner, "SERIAL1", PartAttributeTypeList.Codes.Mandatory, "Vin1", PartAttributeTypeList.Codes.VIN, "BATCH", PartAttributeTypeList.Codes.JulianBatchNumber);

			var importerPart = helper.Part;
			var importerPartRelation = importerPart.RelatedOrganisations.FindByOrganisationAndRelationship(helper.Importer, OrgPartRelation.RelationshipTypes.Owner);
			importerPartRelation.OU_UsePartAttrib1 = true;
			importerPartRelation.OU_UsePartAttrib2 = true;
			importerPartRelation.OU_UsePartAttrib3 = true;

			var ownerPart = helper.OwnerPart;
			var ownerPartRelation = ownerPart.RelatedOrganisations.FindByOrganisationAndRelationship(helper.Owner, OrgPartRelation.RelationshipTypes.Owner);
			ownerPartRelation.OU_UsePartAttrib1 = true;
			ownerPartRelation.OU_UsePartAttrib2 = true;
			ownerPartRelation.OU_UsePartAttrib3 = true;

			declaration.JE_OH_Importer = helper.Importer.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_OH_Owner = helper.Owner.PK;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_PartNo = importerPart.OP_PartNum;
			invoiceLine.JI_PartAttrib1 = "VIM32";
			invoiceLine.JI_PartAttrib2 = "SE324";
			invoiceLine.JI_PartAttrib3 = "BN323";
			AssertCopyingDataToOwnerProduct(importerPart, ZString.Empty, "VIM32", ZString.Empty);
			invoiceLine.JI_PartAttrib1 = "VIM54";
			AssertCopyingDataToOwnerProduct(importerPart, ZString.Empty, "VIM54", ZString.Empty);
			helper.SetupPartAttributesForOrganisation(helper.Owner, "SERIAL", PartAttributeTypeList.Codes.Mandatory);
			invoiceLine.JI_PartAttrib2 = "SE325";
			AssertCopyingDataToOwnerProduct(importerPart, "SE325", "VIM54", ZString.Empty);
			helper.SetupPartAttributesForOrganisation(helper.Owner, attrib3Name: "BATCH", attrib3Type: PartAttributeTypeList.Codes.BatchNumber);
			invoiceLine.JI_PartAttrib3 = "BN325";
			AssertCopyingDataToOwnerProduct(importerPart, "SE325", "VIM54", "BN325");
		}

		void AssertCopyingDataToOwnerProduct(OrgSupplierPart importerPart, ZString expectedPartAttrib1, ZString expectedPartAttrib2, ZString expectedPartAttrib3)
		{
			AssertEquals("JI_NewOwnerPartNo", importerPart.OP_PartNum, invoiceLine.JI_NewOwnerPartNo);
			AssertEquals("JI_NewOwnerPartAttrib1", expectedPartAttrib1, invoiceLine.JI_NewOwnerPartAttrib1);
			AssertEquals("JI_NewOwnerPartAttrib2", expectedPartAttrib2, invoiceLine.JI_NewOwnerPartAttrib2);
			AssertEquals("JI_NewOwnerPartAttrib3", expectedPartAttrib3, invoiceLine.JI_NewOwnerPartAttrib3);
		}

		public void TestIChangeOfOwnershipLineDetailsMembers()
		{
			IChangeOfOwnershipLineDetails changeOfOwnershipLineDetails = invoiceLine;
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerProductCode", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerProductCodeInfo.Value);
			invoiceLine.JI_NewOwnerPartNo = "PART3234";
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerProductCode", "PART3234", changeOfOwnershipLineDetails.NewOwnerProductCodeInfo.Value);
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute1", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerPartAttribute1Info.Value);
			invoiceLine.JI_NewOwnerPartAttrib1 = "ATT1";
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute1", "ATT1", changeOfOwnershipLineDetails.NewOwnerPartAttribute1Info.Value);
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute2", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerPartAttribute2Info.Value);
			invoiceLine.JI_NewOwnerPartAttrib2 = "ATT2";
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute2", "ATT2", changeOfOwnershipLineDetails.NewOwnerPartAttribute2Info.Value);
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute3", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerPartAttribute3Info.Value);
			invoiceLine.JI_NewOwnerPartAttrib3 = "ATT3";
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute3", "ATT3", changeOfOwnershipLineDetails.NewOwnerPartAttribute3Info.Value);
			AssertEquals("changeOfOwnershipLineDetails.NewOwnSerialNumber", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerSerialNumberInfo.Value);
			invoiceLine.JI_NewOwnerSerialNum = "SN";
			AssertEquals("changeOfOwnershipLineDetails.NewOwnSerialNumber", "SN", changeOfOwnershipLineDetails.NewOwnerSerialNumberInfo.Value);
		}

		public void TestIChangeOfOwnershipLineDetailsMembers_ClearedOnClearedOwnerPartNo()
		{
			IChangeOfOwnershipLineDetails changeOfOwnershipLineDetails = invoiceLine;
			invoiceLine.JI_NewOwnerPartNo = "PART3234";
			invoiceLine.JI_NewOwnerPartAttrib1 = "ATT1";
			invoiceLine.JI_NewOwnerPartAttrib2 = "ATT2";
			invoiceLine.JI_NewOwnerPartAttrib3 = "ATT3";
			invoiceLine.JI_NewOwnerSerialNum = "SN";
			invoiceLine.JI_NewOwnerPartNo = ZString.Empty;
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute1", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerPartAttribute1Info.Value);
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute2", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerPartAttribute2Info.Value);
			AssertEquals("changeOfOwnershipLineDetails.NewOwnerPartAttribute3", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerPartAttribute3Info.Value);
			AssertEquals("changeOfOwnershipLineDetails.NewOwnSerialNumber", ZString.Empty, changeOfOwnershipLineDetails.NewOwnerSerialNumberInfo.Value);
		}

		public void TestNewOwnerProduct()
		{
			var owner1 = CreateOrganisation("OWNER1", isConsignee: true, isConsignor: false);
			var owner2 = CreateOrganisation("OWNER2", isConsignee: true, isConsignor: false);
			var owner3 = CreateOrganisation("OWNER3", isConsignee: true, isConsignor: false);
			var supplier1 = CreateOrganisation("SUPPLIER1", isConsignee: false, isConsignor: true);
			var supplier2 = CreateOrganisation("SUPPLIER2", isConsignee: false, isConsignor: true);
			var part1 = CreatePart("PART123", owner1, supplier1);
			part1.OP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			var part2 = CreatePart("PART123", owner1, supplier2);
			part2.OP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			var part3 = CreatePart("PART123", owner2, supplier1);
			part3.OP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-8);
			var part4 = CreatePart("PART123", owner2, supplier2);
			part4.OP_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-1);
			var part5 = CreatePart("PART1", owner2, supplier2);
			part5.OP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-11);
			var part6 = CreatePart("PART123", owner2, null);
			part6.OP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-12);
			declaration.JE_OH_Importer = owner1.PK;
			declaration.JE_OH_Supplier = supplier1.PK;

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OH_Owner = owner2.PK;
			invoiceHeader.JZ_OH_Buyer = owner3.PK;
			invoiceHeader.JZ_OH_Supplier = supplier2.PK;
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNewOwnerPartAttribInfoReadOnly(expected: true);
			invoiceLine.JI_NewOwnerPartAttrib1 = "A";
			invoiceLine.JI_NewOwnerPartAttrib2 = "B";
			invoiceLine.JI_NewOwnerPartAttrib3 = "C";
			invoiceLine.JI_NewOwnerPartNo = "PART123";
			AssertNewOwnerPartAttribInfoReadOnly(expected: false);
			AssertNewOwnerPartAttribValues("A", "B", "C");
			AssertEquals("InvoiceLine.JI_OP_NewOwnerProduct", part4.PK, invoiceLine.JI_OP_NewOwnerProduct);
			invoiceLine.JI_NewOwnerPartNo = ZString.Empty;
			AssertNewOwnerPartAttribValues(ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals("invoiceLine.JI_OP_NewOwnerProduct", ZGuid.Empty, invoiceLine.JI_OP_NewOwnerProduct);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			invoiceLine.JI_NewOwnerPartNo = "PART123";
			AssertEquals("invoiceLine.JI_OP_NewOwnerProduct", part4.PK, invoiceLine.JI_OP_NewOwnerProduct);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			invoiceLine.JI_NewOwnerPartNo = ZString.Empty;
			invoiceLine.JI_NewOwnerPartNo = "PART123";
			AssertEquals("invoiceLine.JI_OP_NewOwnerProduct", part4.PK, invoiceLine.JI_OP_NewOwnerProduct);
		}

		void AssertNewOwnerPartAttribInfoReadOnly(ZBool expected)
		{
			AssertEquals("invoiceLine.JI_NewOwnerPartAttrib1Info.ReadOnly", expected, invoiceLine.JI_NewOwnerPartAttrib1Info.ReadOnly);
			AssertEquals("invoiceLine.JI_NewOwnerPartAttrib2Info.ReadOnly", expected, invoiceLine.JI_NewOwnerPartAttrib2Info.ReadOnly);
			AssertEquals("invoiceLine.JI_NewOwnerPartAttrib3Info.ReadOnly", expected, invoiceLine.JI_NewOwnerPartAttrib3Info.ReadOnly);
		}

		void AssertNewOwnerPartAttribValues(params string[] attribs)
		{
			AssertEquals("invoiceLine.JI_NewOwnerPartAttrib1", attribs[0], invoiceLine.JI_NewOwnerPartAttrib1);
			AssertEquals("invoiceLine.JI_NewOwnerPartAttrib2", attribs[1], invoiceLine.JI_NewOwnerPartAttrib2);
			AssertEquals("invoiceLine.JI_NewOwnerPartAttrib3", attribs[2], invoiceLine.JI_NewOwnerPartAttrib3);
		}

		public void TestLandedCostData()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var entryLineDuty = entryLine.Fees.AddOrUpdate("1P1", 100m);
			var entryLineDutySch1P2B = entryLine.Fees.AddOrUpdate("12B", 120m);
			var entryLineVAT = entryLine.Fees.AddOrUpdate("VAT", 200m);
			entryLine.CL_DutyPercent = 80m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			AssertLandedCostData(220m, 200m, 80m, 220m, 0m);
			entryLineDuty.CF_IsLandedCostOnly = true;
			entryLineDutySch1P2B.CF_IsLandedCostOnly = true;
			entryLineVAT.CF_IsLandedCostOnly = true;
			AssertLandedCostData(0m, 0m, 80m, 220m, 0m);
		}

		void AssertLandedCostData(ZDecimal expectedDutyAmount, ZDecimal expectedGSTVATAmount, ZDecimal expectedDutyPercent, ZDecimal expectedDuty, ZDecimal expectedSpecialTax)
		{
			AssertEquals(nameof(invoiceLine.JI_Calc_DutyAmount), expectedDutyAmount, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals(nameof(invoiceLine.JI_Calc_GSTVATAmount), expectedGSTVATAmount, invoiceLine.JI_Calc_GSTVATAmount);
			AssertEquals("Duty Percentage", expectedDutyPercent, ((IUltimateDistributee)invoiceLine).DutyPercent);
			var lineDutyTaxEntryFeeItems = ((IUltimateDistributee)invoiceLine).LineDutyTaxEntryFeeItems;
			AssertEquals("Duty", expectedDuty, lineDutyTaxEntryFeeItems["TDT"]);
			AssertEquals("SpecialTax1", expectedSpecialTax, lineDutyTaxEntryFeeItems["ST1"]);
		}

		public void TestValuationMarkup()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 400;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_ValuationMarkup = 10;
			declaration.ResumeApportionment();
			AssertEquals(20m, invoiceLine.JI_Calc_ValuationMarkup);
		}

		public void TestIntellectualValue()
		{
			AssertIntellectualValue(20m, assertCore: false);
		}

		public void TestIntellectualValue_Core()
		{
			AssertIntellectualValue(20m, assertCore: true);
		}

		void AssertIntellectualValue(ZDecimal expected, ZBool assertCore)
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 400;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.Charges.AddNew("INT", 20m);
			declaration.ResumeApportionment();
			AssertEquals(expected, assertCore ? invoiceLine.JI_Calc_IntellectualValue_Core : invoiceLine.JI_Calc_IntellectualValue);
		}

		public void TestLookupsIsNotCached()
		{
			var lookups = invoiceLine.Lookups;
			AssertNotEquals("Lookups should not be cached", lookups, invoiceLine.Lookups);
		}

		[TestDate(2013, 03, 13, 13, 13, 00)]
		public void TestIUniversalRateCalcDataMembers()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 1m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_PrimaryPreference = "SKD";
			invoiceLine.JI_LinePrice = 1500m;
			invoiceLine.JI_CustomsQuantity = 11.00499m;
			invoiceLine.JI_CustomsUnitQty = "U1";
			invoiceLine.JI_CustomsSecondQuantity = 22.004m;
			invoiceLine.JI_CustomsSecondUnitQty = "U2";
			invoiceLine.JI_CustomsThirdQuantity = 33.001m;
			invoiceLine.JI_CustomsThirdUnitQty = "U2";
			IUniversalRateCalcData calcData = invoiceLine;
			AssertEquals("DateOfValuation", ZDateTime.Now, calcData.DateOfValuation);
			AssertEquals("ValueForDuty", 1500m, calcData.ValueForDuty);
			var countrySpecificValueList = calcData.CountrySpecificValueList;
			AssertEquals("CountrySpecificValueList.Cout", 0, countrySpecificValueList.Count);
			var unitOfMeasureValueList = calcData.UnitOfMeasureValueList;
			AssertEquals("UnitOfMeasureValueList.Cout", 2, unitOfMeasureValueList.Count);
			AssertEquals("UnitOfMeasureValueList.U1", 11m, unitOfMeasureValueList["U1"]);
			AssertEquals("UnitOfMeasureValueList.U2", 55.01m, unitOfMeasureValueList["U2"]);
		}

		public void TestRulesOfOriginCertificateEffectiveDefaulting()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertRulesOfOriginCertificateEffectiveDefaulting(ZString.Empty, ZString.Empty, ZString.Empty);
			declaration.JE_ROOCert = "CT1";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT1", "CT1");
			invoiceHeader.JZ_ROOCert = "CT2";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT2", "CT2");
			invoiceLine.JI_ROOCert = "CT3";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT2", "CT3");
			declaration.JE_ROOCert = "CT4";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT4", "CT2", "CT3");
			invoiceHeader.JZ_ROOCert = "CT4";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT4", "CT4", "CT3");
			declaration.JE_ROOCert = "CT3";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT3", "CT3", "CT3");
			declaration.JE_ROOCert = "CT1";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT1", "CT1");

			invoiceLine.JI_ROOCert = "CT3";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT1", ZString.Empty);
			AssertEquals("AddInfo of invoiceLine.JI_ROOCert", ZString.Empty, invoiceLine.JI_ROOCert);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT1", "CT3");
			AssertEquals("AddInfo of invoiceLine.JI_ROOCert", "CT3", invoiceLine.JI_ROOCert);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT1", ZString.Empty);
			AssertEquals("AddInfo of invoiceLine.JI_ROOCert", ZString.Empty, invoiceLine.JI_ROOCert);
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.PreferentialRate;
			invoiceLine.JI_ROOCert = "CT3";
			AssertRulesOfOriginCertificateEffectiveDefaulting("CT1", "CT1", "CT3");
		}

		void AssertRulesOfOriginCertificateEffectiveDefaulting(ZString rootCert, ZString invoiceRootCert, ZString invoiceLineRootCert)
		{
			AssertEquals("declaration.JE_ROOCert", rootCert, declaration.JE_ROOCert);
			AssertEquals("invoice.JZ_ROOCert", invoiceRootCert, declaration.Invoices[0].JZ_ROOCert);
			AssertEquals("invoiceLine.JI_ROOCert", invoiceLineRootCert, declaration.InvoiceLines[0].JI_ROOCert);
		}

		public void TestPartType()
		{
			var factory2 = new BusinessObjectFactory();
			var importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (MasterFiles.Business.OrgSupplierPart)factory2.New<AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();
			var zaCompany = Factory.New<GlbCompany>();
			zaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var zaBranch = zaCompany.Branches.AddNew();
			zaBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica)).RL_Code;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GB = zaBranch.PK;
			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("Product type gets changed depending on who is requesting", typeof(OrgSupplierPart), invoiceLine.Part.GetType());
			Factory.Save();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var factory3 = new BusinessObjectFactory();
			var declarationLoaded = factory3.Load<JobDeclaration>(declaration.PK);
			AssertEquals("product type still the right type type as it is hooked to ZA invoice line", typeof(OrgSupplierPart), declarationLoaded.InvoiceLines[0].Part.GetType());
		}

		public void TestIBondedWarehouseTransactionLineProvider()
		{
			AssertEquals(typeof(BondedWarehouseTransactionLine), ((IBondedWarehouseTransactionLineProvider)invoiceLine).TransactionLine.GetType());
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertEquals("There is no tariff and CustomsQuantity should be readonly", expected: true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			invoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", expected: true, invoiceLine.JI_CustomsUnitQty.IsEmpty);
			invoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", expected: false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertEquals(expected: true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(expected: false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestApportionedCustomsValue()
		{
			SetupInvoiceHeaderWithThreeLines(200.0m, 150.0m, 50.0m);
			AssertEquals(200.0m, invoiceHeader.JobComInvoiceLines[0].ApportionedCustomsValue);
			AssertEquals(150.0m, invoiceHeader.JobComInvoiceLines[1].ApportionedCustomsValue);
			AssertEquals(50.0m, invoiceHeader.JobComInvoiceLines[2].ApportionedCustomsValue);
		}

		public void TestLinkedEntryLineNumber()
		{
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "21";
			testInst.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 9999;
			entryHeader.CH_CEI_Instruction = testInst.PK;
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals((short)9999, invoiceLine.LinkedEntryLineNumber);
		}

		public void TestCustomsQuantityIsGreaterThenZeroAfterApportionByWeight()
		{
			SetupInvoiceHeaderWithThreeLines(399m, 100m, 0.1m);
			AssertEquals("InvoiceLine1 Customs Quantity", 3.997m, invoiceHeader.JobComInvoiceLines[0].JI_CustomsQuantity);
			AssertEquals("InvoiceLine2 Customs Quantity", 1.002m, invoiceHeader.JobComInvoiceLines[1].JI_CustomsQuantity);
			AssertEquals("InvoiceLine3 Customs Quantity", 0.01m, invoiceHeader.JobComInvoiceLines[2].JI_CustomsQuantity);
		}

		public void TestWeightIsGreaterThenZeroAfterApportionByWeight()
		{
			SetupInvoiceHeaderWithThreeLines(399m, 100m, 0.1m);
			AssertEquals("InvoiceLine1 Customs Quantity", 3.997m, invoiceHeader.JobComInvoiceLines[0].JI_Weight);
			AssertEquals("InvoiceLine2 Customs Quantity", 1.002m, invoiceHeader.JobComInvoiceLines[1].JI_Weight);
			AssertEquals("InvoiceLine3 Customs Quantity", 0.001m, invoiceHeader.JobComInvoiceLines[2].JI_Weight);
		}

		void SetupInvoiceHeaderWithThreeLines(ZDecimal linePrice1, ZDecimal linePrice2, ZDecimal linePrice3)
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalNoOfPacks = 16;
			declaration.JE_TotalWeight = 5m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "60";
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_Weight = 5m;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_NetWeight = 5m;
			invoiceHeader.JZ_NetWeightUQ = "KG";
			invoiceHeader.InvoiceLines.RemoveAndDeleteAll();

			for (var i = 0; i < 3; i++)
			{
				var line = invoiceHeader.JobComInvoiceLines.AddNew();
				line.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				line.JI_CEI = instruction1.PK;
				line.JI_Tariff = "3304.99.00 3";
				line.JI_Procedure = "6000";
				line.JI_CustomsUnitQty = "KG";
				switch (i)
				{
					case 0:
						line.JI_LinePrice = linePrice1;
						break;
					case 1:
						line.JI_LinePrice = linePrice2;
						break;
					case 2:
						line.JI_LinePrice = linePrice3;
						break;
				}
			}
			DoMerge(declaration);
		}

		public override void TestEffectiveGrossWeightReturnsApportionedWeightWhenSomeWeightKnown()
		{
			SetupEffectiveGrossWeight();
			BaseJobComInvoiceLine line1 = invoiceLine;
			line1.JI_Weight = 500m;
			line1.JI_WeightUQ = "KG";
			line1.JI_LinePrice = 100m;
			BaseJobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Weight = 0m;
			line2.JI_WeightUQ = "KG";
			line2.JI_LinePrice = 120m;
			BaseJobComInvoiceLine line3 = invoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_Weight = 0m;
			line3.JI_WeightUQ = "KG";
			line3.JI_LinePrice = 80m;
			AssertEquals("ZA customs does not use the apportioned weight for Gross Weight", new ZWeight(500, "KG"), line1.EffectiveGrossWeight);
			AssertEquals("ZA customs does not use the apportioned weight for Gross Weight", new ZWeight(0, "KG"), line2.EffectiveGrossWeight);
			AssertEquals("ZA customs does not use the apportioned weight for Gross Weight", new ZWeight(0, "KG"), line3.EffectiveGrossWeight);
		}

		public override void TestEffectiveGrossWeightReturnsApportionedWeightWhenNoWeightKnown()
		{
			SetupEffectiveGrossWeight();
			BaseJobComInvoiceLine line1 = invoiceLine;
			line1.JI_Weight = 0m;
			line1.JI_WeightUQ = "KG";
			line1.JI_LinePrice = 100m;
			BaseJobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Weight = 0m;
			line2.JI_WeightUQ = "KG";
			line2.JI_LinePrice = 120m;
			BaseJobComInvoiceLine line3 = invoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_Weight = 0m;
			line3.JI_WeightUQ = "KG";
			line3.JI_LinePrice = 80m;
			AssertEquals("ZA customs doesnot use the apportioned weight for Gross Weight", new ZWeight(0, "KG"), line1.EffectiveGrossWeight);
			AssertEquals("ZA customs doesnot use the apportioned weight for Gross Weight", new ZWeight(0, "KG"), line2.EffectiveGrossWeight);
			AssertEquals("ZA customs doesnot use the apportioned weight for Gross Weight", new ZWeight(0, "KG"), line3.EffectiveGrossWeight);
		}

		void SetupEffectiveGrossWeight()
		{
			invoiceHeader.JZ_InvoiceAmount = 300m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_Weight = 1000m;
			invoiceHeader.JZ_WeightUQ = "KG";
		}

		public void TestSetDefaultValues()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals("JI_TakeUpInTradeStatistics", expected: true, invoiceLine.JI_TakeUpInTradeStatistics);
			AssertEquals("JI_PrimaryPreference", string.Empty, invoiceLine.JI_PrimaryPreference);
		}

		public void TestTariffFormatterAttached()
		{
			invoiceLine.JI_Tariff = "2917.19.35";
			AssertEquals("Formatter BuiltIn - Tariff", "29171935", invoiceLine.JI_Tariff);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			invoiceLine.JI_Tariff = "29171935";
			AssertEquals("Formatter Interfaced - Tariff", "29171935", invoiceLine.JI_Tariff);
		}

		public void TestSettingPartNoSetsDescriptionFromPart()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			CreateProduct(supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			Assert("Line Description", invoiceLine.JI_Description.IsEmpty);
			invoiceLine.JI_PartNo = "NEWPROD10";
			Assert("Line Description should contain <PRODUCT10>", invoiceLine.JI_Description.Contains("PRODUCT10"));
		}

		public void TestSettingProductCodeSetsBrandNameFromOrgSupplierPartBrand()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var owner1 = CreateOrganisation("OWNER1", isConsignee: true, isConsignor: false);
				var supplier1 = CreateOrganisation("SUPPLIER1", isConsignee: false, isConsignor: true);
				var product1 = CreateProduct(supplier1, "PART111", "PART 111 DESC");
				product1.OP_Brand = "BRANDO";

				var declaration = Factory.NewMoq<JobDeclaration>().Object;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_OH_Supplier = supplier1.PK;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				Factory.Save();

				Assert("Line Brand Name should be empty", invoiceLine.JI_BrandName.IsEmpty);
				invoiceLine.JI_PartNo = "PART111";
				Assert("Line Brand Name should now contain BRANDO", invoiceLine.JI_BrandName.Contains("BRANDO"));
				invoiceLine.JI_PartNo = ZString.Empty;
				Assert("Line Brand Name should not change as it contains data", invoiceLine.JI_BrandName.Contains("BRANDO"));
			}
		}

		public new void TestEffectiveCountryOfOrigin()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals("Blank Country of Origin for export", "", invoiceLine.EffectiveCountryOfOrigin);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("Blank Country of Origin for import", "ZN", invoiceLine.EffectiveCountryOfOrigin);
		}

		public void TestEffectiveCountryOfOriginUsesHeaderIfBlank()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;
			invoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("Country when filled on invoice line", "ES", invoiceLine.EffectiveCountryOfOrigin);
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Country when empty on invoice line", Enterprise.Core.Constants.CountryCodes.SouthAfrica, invoiceLine.EffectiveCountryOfOrigin);
		}

		public void TestWhenInvoiceUQIsNXCustomsQtyReturnsInvoiceQty()
		{
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "NX";
			AssertEquals(100m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestWhenInvoiceUQIsNXCustomsUQReturnsInvoiceUQ()
		{
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "NX";
			AssertEquals("NX", invoiceLine.JI_CustomsUnitQty);
		}

		public override void TestChargeTypeList()
		{
			var testInvoiceLine = invoiceLine as Customs.Business.ICommonInvoice;
			CombineAssertions("EXP", () =>
			{
				AssertEquals(1, testInvoiceLine.ChargeTypeList.Count);
				Assert(testInvoiceLine.ChargeTypeList.ContainsCode("DIS"));
			});
			CombineAssertions("IMP", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				AssertEquals(2, testInvoiceLine.ChargeTypeList.Count);
				Assert(testInvoiceLine.ChargeTypeList.ContainsCode("DIS"));
				Assert(testInvoiceLine.ChargeTypeList.ContainsCode("INT"));
			});
		}

		public void TestSetChargeCurrency()
		{
			var testCharge = invoiceLine.Charges.AddNew("DIS", 1m);
			AssertEquals(string.Empty, testCharge.J7_RX_NKCurrency);
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, testCharge.J7_RX_NKCurrency);
			invoiceHeader.JZ_RX_NKInvoice_Currency = "XXX";
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, testCharge.J7_RX_NKCurrency);
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, testCharge.J7_RX_NKCurrency);
		}

		[TestDate(2016, 01, 01)]
		public void TestDefaultingValuationMarkup()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_ValuationMarkup = 10;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			AssertEquals(10m, invoiceLine.JI_ValuationMarkup);
			AssertEquals(0m, invoiceLine2.JI_ValuationMarkup);
			invoiceLine.JI_JZ = invoiceHeader2.PK;
			invoiceLine2.JI_JZ = invoiceHeader.PK;
			AssertEquals(0m, invoiceLine.JI_ValuationMarkup);
			AssertEquals(10m, invoiceLine2.JI_ValuationMarkup);
			invoiceHeader.JZ_ValuationMarkup = 0;
			invoiceHeader2.JZ_ValuationMarkup = 5;
			AssertEquals(5m, invoiceLine.JI_ValuationMarkup);
			AssertEquals(0m, invoiceLine2.JI_ValuationMarkup);
		}

		public void TestActualPriceAndCustomsValue()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			invoiceHeader.JZ_InvoiceAmount = 500;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";
			invoiceLine.JI_LinePrice = 500;
			AssertEquals("JI_ActualPrice: None", 0m, invoiceLine.JI_ActualPrice);
			SetupDataEligibleForMerging(declaration);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("fob: none", 500m, invoiceLine.JI_CustomsValue);
				AssertEquals("ActualPrice: None", 500m, invoiceLine.JI_Calc_ActualPrice);
				AssertEquals("CustomsValue: none", 500m, invoiceLine.JI_CustomsValue);
				AssertEquals("JI_ActualPrice: None", 500m, invoiceLine.JI_ActualPrice);
				invoiceLine.Charges.AddNew("INT", 50m);
				AssertEquals("fob: INT", 450m, invoiceLine.JI_CustomsValue);
				AssertEquals("ActualPrice: INT", 500m, invoiceLine.JI_Calc_ActualPrice);
				AssertEquals("CustomsValue: INT", 450m, invoiceLine.JI_CustomsValue);
			});
		}

		[TestDate(2016, 12, 1)]
		public void TestApportionedDutiableChargesInDifferentCurrencies()
		{
			var company = Factory.NewMoq<GlbCompany>().Object;
			company.GC_Code = "ZA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			company.GC_OH_OrgProxy = org.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AAA";
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				declaration = Factory.NewMoq<JobDeclaration>().Object;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
				invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				invoiceHeader.JZ_IncoTerm = "FOB";
				invoiceHeader.JZ_InvoiceAmount = 10600;
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.Charges.RemoveAll();
				invoiceLine.ApportionedCharges.RemoveAll();
				var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedKingdom);
				SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 0.5m, currency);
				var lch_excluded = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 10m, Core.Constants.CurrencyCodes.UnitedKingdom);
				lch_excluded.J7_IsDutiable = false;
				var oft = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20m, Core.Constants.CurrencyCodes.UnitedKingdom);
				oft.J7_IsDutiable = false;
				var ons = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 30m, Core.Constants.CurrencyCodes.UnitedKingdom);
				ons.J7_IsDutiable = false;
				var oth = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 40m, Core.Constants.CurrencyCodes.UnitedKingdom);
				oth.J7_IsDutiable = true;
				var oth_excluded = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 50m, Core.Constants.CurrencyCodes.UnitedKingdom);
				oth_excluded.J7_IsDutiable = false;
				var com = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 60m, Core.Constants.CurrencyCodes.UnitedKingdom);
				com.J7_IsDutiable = false;
				CombineAssertions("apportionaed charges in same currency", () =>
				{
					AssertEquals("total freight charges", 20m, invoiceLine.OverseasFreightNotIncludedInLines.Amount);
					AssertEquals("total freight charges", Core.Constants.CurrencyCodes.UnitedKingdom, invoiceLine.OverseasFreightNotIncludedInLines.Currency.Code);
					AssertEquals("total insurance charges", 30m, invoiceLine.OverseasInsuranceNotIncludedInLines.Amount);
					AssertEquals("total insurance charges", Core.Constants.CurrencyCodes.UnitedKingdom, invoiceLine.OverseasInsuranceNotIncludedInLines.Currency.Code);
					AssertEquals("total dutiable charges", 40m, invoiceLine.DutiableChargesNotIncludedInLines.Amount);
					AssertEquals("total dutiable charges", Core.Constants.CurrencyCodes.UnitedKingdom, invoiceLine.DutiableChargesNotIncludedInLines.Currency.Code);
					AssertEquals("total non dutiable charges", 120m, invoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount);
					AssertEquals("total non dutiable charges", Core.Constants.CurrencyCodes.UnitedKingdom, invoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Currency.Code);
				});
				var oth_zar = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 70, Core.Constants.CurrencyCodes.SouthAfrica);
				oth_zar.J7_IsDutiable = true;
				CombineAssertions("apportionaed charges in differing currencies - display in invoice currency", () =>
				{
					AssertEquals("total freight charges", 40m, invoiceLine.OverseasFreightNotIncludedInLines.Amount);
					AssertEquals("total freight charges", Core.Constants.CurrencyCodes.SouthAfrica, invoiceLine.OverseasFreightNotIncludedInLines.Currency.Code);
					AssertEquals("total insurance charges", 60m, invoiceLine.OverseasInsuranceNotIncludedInLines.Amount);
					AssertEquals("total insurance charges", Core.Constants.CurrencyCodes.SouthAfrica, invoiceLine.OverseasInsuranceNotIncludedInLines.Currency.Code);
					AssertEquals("total dutiable charges", 150m, invoiceLine.DutiableChargesNotIncludedInLines.Amount);
					AssertEquals("total dutiable charges", Core.Constants.CurrencyCodes.SouthAfrica, invoiceLine.DutiableChargesNotIncludedInLines.Currency.Code);
					AssertEquals("total non dutiable charges", 240m, invoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount);
					AssertEquals("total non dutiable charges", Core.Constants.CurrencyCodes.SouthAfrica, invoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Currency.Code);
				});
			}
		}

		[TestDate(2016, 12, 1)]
		public void TestApportionedDutiableChargesInRoundedCurrency()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ZA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			company.GC_OH_OrgProxy = org.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AAA";
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				invoiceLine.Charges.RemoveAll();
				invoiceLine.ApportionedCharges.RemoveAll();
				var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
				var jpyCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);
				SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 199m, usdCurrency);
				invoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
				invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoiceHeader.JZ_IncoTerm = "FOB";
				invoiceHeader.JZ_InvoiceAmount = 8096.15;
				var lch_excluded = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 10.11m, "JPY");
				lch_excluded.J7_IsDutiable = false;
				var oft = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 20.22m, "JPY");
				oft.J7_IsDutiable = false;
				var ons = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 30.33m, "JPY");
				ons.J7_IsDutiable = false;
				var oth = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 40.44m, "JPY");
				oth.J7_IsDutiable = true;
				var oth_excluded = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 50.55m, "JPY");
				oth_excluded.J7_IsDutiable = false;
				var com = invoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.Commission, 60.66m, "JPY");
				com.J7_IsDutiable = false;
				CombineAssertions("apportionaed charges in JPY currency shouldn't be rounded", () =>
				{
					AssertEquals("total freight charges", 20.22m, invoiceLine.OverseasFreightNotIncludedInLines.Amount);
					AssertEquals("total freight charges", "JPY", invoiceLine.OverseasFreightNotIncludedInLines.Currency.Code);
					AssertEquals("total insurance charges", 30.33m, invoiceLine.OverseasInsuranceNotIncludedInLines.Amount);
					AssertEquals("total insurance charges", "JPY", invoiceLine.OverseasInsuranceNotIncludedInLines.Currency.Code);
					AssertEquals("total dutiable charges", 40.44m, invoiceLine.DutiableChargesNotIncludedInLines.Amount);
					AssertEquals("total dutiable charges", "JPY", invoiceLine.DutiableChargesNotIncludedInLines.Currency.Code);
					AssertEquals("total non dutiable charges", 121.32m, invoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount);
					AssertEquals("total non dutiable charges", "JPY", invoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Currency.Code);
				});
			}
		}

		public void TestDutiableChargesIncludedInLines()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			SetupDummyCharges(declaration.LocalCurrencyCode);
			AssertEquals("DutiableChargesIncludedInLines: 10m", 10m, invoiceLine.DutiableChargesIncludedInLines.Amount);
		}

		public void TestDutiableChargesNotIncludedInLines()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			SetupDummyCharges(declaration.LocalCurrencyCode);
			AssertEquals("DutiableChargesNotIncludedInLines: 1m + 1000m", 1001m, invoiceLine.DutiableChargesNotIncludedInLines.Amount);
		}

		public void TestNonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			SetupDummyCharges(declaration.LocalCurrencyCode);
			AssertEquals("NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance: (100m)", 100m, invoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount);
		}

		public void TestOverseasFreightIncludedInLines()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			SetupDummyCharges(declaration.LocalCurrencyCode);
			AssertEquals("OverseasFreightIncludedInLines: (100000m)", 100000m, invoiceLine.OverseasFreightIncludedInLines.Amount);
		}

		public void TestOverseasFreightNotIncludedInLines()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			SetupDummyCharges(declaration.LocalCurrencyCode);
			AssertEquals("OverseasFreightIncludedInLines: (10000m)", 10000m, invoiceLine.OverseasFreightNotIncludedInLines.Amount);
		}

		public void TestOverseasInsuranceIncludedInLines()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			SetupDummyCharges(declaration.LocalCurrencyCode);
			AssertEquals("OverseasInsuranceIncludedInLines: (10000000m)", 10000000m, invoiceLine.OverseasInsuranceIncludedInLines.Amount);
		}

		public void TestOverseasInsuranceNotIncludedInLines()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			SetupDummyCharges(declaration.LocalCurrencyCode);
			AssertEquals("OverseasInsuranceNotIncludedInLines: (1000000m)", 1000000m, invoiceLine.OverseasInsuranceNotIncludedInLines.Amount);
		}

		void SetupDummyCharges(ZString currency)
		{
			AddCharge(CustomsChargeTypeList.Codes.OtherCharges, 1m, currency, isDutiable: true, isIncludedInITOT: false);
			AddCharge(CustomsChargeTypeList.Codes.OtherCharges, 10m, currency, isDutiable: true, isIncludedInITOT: true);
			AddCharge(CustomsChargeTypeList.Codes.OtherCharges, 100m, currency, isDutiable: false, isIncludedInITOT: false);
			AddCharge(CustomsChargeTypeList.Codes.OverseasFreight, 10000m, currency, isDutiable: false, isIncludedInITOT: false);
			AddCharge(CustomsChargeTypeList.Codes.OverseasFreight, 100000m, currency, isDutiable: false, isIncludedInITOT: true);
			AddCharge(CustomsChargeTypeList.Codes.OverseasInsurance, 1000000m, currency, isDutiable: false, isIncludedInITOT: false);
			AddCharge(CustomsChargeTypeList.Codes.OverseasInsurance, 10000000m, currency, isDutiable: false, isIncludedInITOT: true);
			AddCharge(CustomsChargeTypeList.Codes.OtherCharges, 1000m, currency, isDutiable: true, isIncludedInITOT: false);
		}

		public void TestDiscount()
		{
			invoiceLine.Charges.RemoveAll();
			invoiceLine.ApportionedCharges.RemoveAll();
			AssertEquals("Discount", 0m, invoiceLine.Discount.Amount);
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 20m, declaration.LocalCurrencyCode);
			invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 30m, declaration.LocalCurrencyCode);
			AssertEquals("Discount", 50m, invoiceLine.Discount.Amount);
		}

		public void TestCopyCusLineTariffDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "TestDescription";
			invoiceLine.CusLineTariffDetails.AddNew("12A", "111#");
			var clonedDeclaration = (JobDeclaration)new ZAJobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			var clonedTariffDetail = clonedDeclaration.InvoiceLines[0].CusLineTariffDetails;
			AssertEquals("CusLineTariffDetails Copy - Count", 1, clonedTariffDetail.Count);
			AssertEquals("CusLineTariffDetails Copy - AdditionalDuty Copied", "12A", clonedTariffDetail[0].BZ_Type);
			AssertEquals("CusLineTariffDetails Copy - AdditionalDuty Copied", "111#", clonedTariffDetail[0].BZ_Tariff);
		}

		public void TestCanConvertFromNetWeightToCustomsUnit()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_NetWeight = 1m;
			invoiceLine.JI_NetWeightUQ = "KG";
			AssertEquals("KG", expected: true, invoiceLine.CanConvertFromNetWeightToCustomsUnit("KG"));
			AssertEquals("AA", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("AA"));
			AssertEquals("SM", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("SM"));
			AssertEquals("BW", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("BW"));
			AssertEquals("BS", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("BS"));
			AssertEquals("CT", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("CT"));
			AssertEquals("CR", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("CR"));
			AssertEquals("GW", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("GW"));
			AssertEquals("GE", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("GE"));
			AssertEquals("GK", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("GK"));
			AssertEquals("GJ", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("GJ"));
			AssertEquals("IU", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("IU"));
			AssertEquals("KW", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("KW"));
			AssertEquals("CM", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("CM"));
			AssertEquals("ME", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("ME"));
			AssertEquals("MM", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("MM"));
			AssertEquals("GS", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("GS"));
			AssertEquals("KN", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("KN"));
			AssertEquals("GN", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("GN"));
			AssertEquals("MU", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("MU"));
			AssertEquals("PR", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("PR"));
			AssertEquals("NX", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("NX"));
			AssertEquals("LC", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("LC"));
			AssertEquals("PA", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("PA"));
			AssertEquals("RD", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("RD"));
			AssertEquals("NO", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("NO"));
			AssertEquals("LA", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("LA"));
			AssertEquals("LI", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("LI"));
			AssertEquals("ML", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("ML"));
			AssertEquals("MW", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("MW"));
			AssertEquals("KU", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("KU"));
			AssertEquals("MC", expected: false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("MC"));
		}

		public void TestCustomsQuantityUnitConversion()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_NetWeight = 1m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("1KG = 1KG", 1m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_CustomsUnitQty = "MC";
			AssertEquals("CustomsUnitQty should not be changed for MC (1)", 1m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_NetWeight = 2m;
			AssertEquals("CustomsUnitQty should not be changed for MC (2)", 1m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestSettingEntryInstructionWillCleanTargetEntryLineNumber()
		{
			var inst1 = declaration.CustomsEntryInstructions.AddNew();
			inst1.CEI_Style = "11";
			var inst2 = declaration.CustomsEntryInstructions.AddNew();
			inst2.CEI_Style = "11";
			invoiceLine.JI_CEI = inst1.PK;
			invoiceLine.JI_TargetEntryLineNumber = 1;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = inst2.PK;
			invoiceLine2.JI_TargetEntryLineNumber = 2;
			CombineAssertions(() =>
			{
				AssertEquals("Pre 1", new ZShort(1), invoiceLine.JI_TargetEntryLineNumber);
				AssertEquals("Pre 2", new ZShort(2), invoiceLine2.JI_TargetEntryLineNumber);
				invoiceLine.JI_CEI = inst2.PK;
				invoiceLine2.JI_CEI = inst2.PK;
				AssertEquals("After 1", new ZShort(0), invoiceLine.JI_TargetEntryLineNumber);
				AssertEquals("After 2", new ZShort(2), invoiceLine2.JI_TargetEntryLineNumber);
			});
		}

		public void TestRefreshPartSyncManagerWhenPartAttribChanged()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var ownRelation = part.RelatedOrganisations.AddNew();
			ownRelation.OU_OH = Factory.New<OrgHeader>().PK;
			ownRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var supRelation = part.RelatedOrganisations.AddNew();
			supRelation.OU_OH = Factory.New<OrgHeader>().PK;
			supRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var pivot1 = AddPivotWithAttributes(part, ClassificationTypeList.Codes.HTI, ownRelation.OU_OH, "1234", "A", "B", "C");
			var pivot2 = AddPivotWithAttributes(part, ClassificationTypeList.Codes.HTI, ownRelation.OU_OH, "2345", "D", "E", "F");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ownRelation.OU_OH;
			declaration.JE_OH_Supplier = supRelation.OU_OH;
			invoiceLine.JI_PartNo = "PARTNUM";
			AssertNull("Pivot cannot be found without setting attributes", invoiceLine.Pivot);
			invoiceLine.JI_PartAttrib1 = "a";
			invoiceLine.JI_PartAttrib2 = "b";
			invoiceLine.JI_PartAttrib3 = "c";
			AssertEquals("Pivot can be found with setting attributes", pivot1.PK, invoiceLine.Pivot.PK);
			AssertEquals("JI_Tariff", "1234", invoiceLine.JI_Tariff);
			invoiceLine.JI_PartAttrib1 = "d";
			invoiceLine.JI_PartAttrib2 = "e";
			invoiceLine.JI_PartAttrib3 = "f";
			AssertEquals("Pivot should be found with setting attributes", pivot2.PK, invoiceLine.Pivot.PK);
			AssertEquals("JI_Tariff", "2345", invoiceLine.JI_Tariff);
		}

		public void TestSynchroniseVINNumberAndVINPartAttribute()
		{
			for (var i = 1; i <= 3; i++)
			{
				var owner = Factory.New<OrgHeader>();
				owner.OH_Code = $"OH{i}";
				owner.MiscServ[$"OM_IMPartAttrib{i}Type"] = PartAttributeTypeList.Codes.VIN;
				owner.MiscServ[$"OM_IMPartAttrib{i}Name"] = "VIN number attribute";
				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "PARTNUM";
				var ownRelation = part.RelatedOrganisations.AddNew();
				ownRelation.OU_OH = owner.PK;
				ownRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
				Factory.Save();
				var declaration = new BusinessObjectFactory().New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = ownRelation.OU_OH;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine.Factory, invoiceLine.PartSyncManagerActiveDeciderPK);
				invoiceLine.JI_PartNo = "PARTNUM";
				var partAttribName = $"JI_PartAttrib{i}";
				invoiceLine.JI_VIN = "VIN1234";
				AssertEquals("VIN number attribute should NOT be populated from VIN field", "", invoiceLine[partAttribName]);
				invoiceLine[partAttribName] = "VIN3456";
				AssertEquals("VIN field should be populated from VIN number attribute", "VIN1234", invoiceLine.JI_VIN);
				ownRelation[$"OU_UsePartAttrib{i}"] = true;
				Factory.Save();
				AssertEquals("VIN number attribute should be populated from VIN field", "VIN1234", invoiceLine[partAttribName]);
				invoiceLine[partAttribName] = "VIN3456";
				AssertEquals("VIN field should be populated from VIN number attribute", "VIN3456", invoiceLine.JI_VIN);
				invoiceLine.JI_VIN = "VIN1234";
				AssertEquals("VIN number attribute should be populated from VIN field", "VIN1234", invoiceLine[partAttribName]);
			}
		}

		public void TestUniversalCopyAttributes()
		{
			var componentType = typeof(JobComInvoiceLine);
			AssertEquals("JobComInvoiceLine should have UniversalCopyWithExtendedEntitiesAttribute.", expected: true, componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), inherit: true).Length > 0);
			var cusLineTariffDetailsInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "CusLineTariffDetails");
			AssertEquals("CusLineTariffDetails collection should have UniversalCopyCollectionEntityAttribute.", expected: true, cusLineTariffDetailsInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), inherit: true)[0] != null);
		}

		public void TestJI_PreviousEntryLineNumberMaxValue()
		{
			invoiceLine.JI_PreviousEntryLineNumber = 12345;
			AssertEquals((ZShort)0, invoiceLine.JI_PreviousEntryLineNumber);
			invoiceLine.JI_PreviousEntryLineNumber = 9999;
			AssertEquals((ZShort)9999, invoiceLine.JI_PreviousEntryLineNumber);
		}

		public void TestJI_PreviousEntryNumber_Defaulting()
		{
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "10";
			invoiceLine.JI_CEI = instruction.PK;
			var mrn1 = "PTA201604198464646";
			var mrn2 = "PTA201604198464647";
			CombineAssertions("JI_PreviousEntryNumber Defaulting", () =>
			{
				AssertEquals("Test 01", invoiceLine.JI_PreviousEntryNumber, ZString.Empty);
				instruction.CEI_PreviousMRN = mrn1;
				AssertEquals("Test 02", instruction.CEI_PreviousMRN, mrn1);
				AssertEquals("Test 03", invoiceLine.JI_PreviousEntryNumber, mrn1);
				invoiceLine.JI_PreviousEntryNumber = mrn2;
				AssertEquals("Test 04", instruction.CEI_PreviousMRN, mrn1);
				AssertEquals("Test 05", invoiceLine.JI_PreviousEntryNumber, mrn2);
				instruction.CEI_PreviousMRN = mrn2;
				AssertEquals("Test 06", instruction.CEI_PreviousMRN, mrn2);
				AssertEquals("Test 07", invoiceLine.JI_PreviousEntryNumber, mrn2);
				instruction.CEI_PreviousMRN = mrn1;
				AssertEquals("Test 08", instruction.CEI_PreviousMRN, mrn1);
				AssertEquals("Test 09", invoiceLine.JI_PreviousEntryNumber, mrn1);
				invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
				AssertEquals("Test 10", instruction.CEI_PreviousMRN, mrn1);
				AssertEquals("Test 11", invoiceLine.JI_PreviousEntryNumber, mrn1);
				instruction.CEI_PreviousMRN = ZString.Empty;
				AssertEquals("Test 12", instruction.CEI_PreviousMRN, ZString.Empty);
				AssertEquals("Test 13", invoiceLine.JI_PreviousEntryNumber, ZString.Empty);
			});
		}

		public void TestOnMergedCopiesJI_Calc_ActualPriceToActualPrice()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			invoiceHeader.JZ_InvoiceAmount = 500;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}00";
			invoiceLine.JI_LinePrice = 500;
			AssertNotEquals(500m, invoiceLine.JI_ActualPrice);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			AssertEquals(500m, invoiceLine.JI_ActualPrice);
		}

		public void TestGetJI_Calc_CIF()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeader.JobDeclaration.LocalCurrencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			invoiceLine.JI_LinePrice = 1000.00m;
			invoiceLine.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, 500.00m);
			AssertEquals("Line CIF", 1000.00m, invoiceLine.JI_Calc_CIF, 0.01m);
		}

		public void TestGetJI_Calc_CIFUsingFetchHint()
		{
			AssertCalculateUsingFetchHint(nameof(JobComInvoiceLine.JI_Calc_CIF));
		}

		public void TestGetJI_Calc_FOBUsingFetchHint()
		{
			AssertCalculateUsingFetchHint(nameof(JobComInvoiceLine.JI_Calc_FOB));
		}

		void AssertCalculateUsingFetchHint(string propertyName)
		{
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			var line2 = invoiceHeader.InvoiceLines.AddNew();
			var line3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeader.JobDeclaration.LocalCurrencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			line1.JI_LinePrice = 1000.00m;
			line2.JI_LinePrice = 2000.00m;
			line1.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue);
			line2.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue);
			line3.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue);
			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.Load<JobComInvoiceLine>(line1.PK).GetPropertyValue<ZDecimal>(propertyName);
			var dbHitsBefore = anotherFactory.GetTableHitCount(InvoiceLineCharge.Schema.TableName);
			anotherFactory.Load<JobComInvoiceLine>(line2.PK).GetPropertyValue<ZDecimal>(propertyName);
			anotherFactory.Load<JobComInvoiceLine>(line3.PK).GetPropertyValue<ZDecimal>(propertyName);
			var dbHitsAfter = anotherFactory.GetTableHitCount(InvoiceLineCharge.Schema.TableName);
			AssertEquals("Invoice charge should be cached using fetch hints", 0, dbHitsAfter - dbHitsBefore);
		}

		public void TestGetJI_Calc_FOB()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeader.JobDeclaration.LocalCurrencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			invoiceLine.JI_LinePrice = 1000.00m;
			invoiceLine.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, 500.00m);
			AssertEquals("Line FOB", 1000.00m, invoiceLine.JI_Calc_FOB, 0.01m);
		}

		public void TestGetJI_Calc_ATV()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateTaxOrFee("VZR", 0, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Zero Rated");
			testHelper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Normal");
			Factory.Save();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			var line2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeader.JobDeclaration.LocalCurrencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			line1.JI_LinePrice = 200m;
			line1.JI_ZZF_NKTaxType = "VAT";
			line2.JI_LinePrice = 300m;
			line2.JI_ZZF_NKTaxType = "VAT";
			var intellectual = line1.Charges.AddNew(InvoiceLineCustomsChargeTypeList.Codes.IntellectualValue, 20.00m);
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate("VAT", 51.23m);
			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine.PK;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertGetJICalcATV(line1, 20.492m, 146.00m);
				AssertGetJICalcATV(line2, 30.738m, 220.00m);
			});
			line1.JI_ZZF_NKTaxType = "VEX";
			line2.JI_ZZF_NKTaxType = "VEX";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertGetJICalcATV(line1, 0.0m, 0.0m);
				AssertGetJICalcATV(line2, 0.0m, 0.0m);
			});
		}

		void AssertGetJICalcATV(JobComInvoiceLine invoiceLine, ZDecimal vat, ZDecimal atv)
		{
			AssertEquals("VAT", vat, invoiceLine.JI_Calc_GSTVATAmount);
			AssertEquals("ATV", atv, invoiceLine.JI_Calc_ATV);
		}

		public void TestDefaultPreference()
		{
			SetupDefaultPreferenceAndTradeAgreement();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "991001";
			var tariff = invoiceLine.UniversalTariff;
			AssertNull(tariff);
			invoiceLine.JI_Tariff = "992002";
			tariff = invoiceLine.UniversalTariff;
			AssertNotEquals(null, tariff);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNullOrEmpty("TestDefaultPreference is null/empty", invoiceLine.JI_PrimaryPreference);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertNullOrEmpty("TestDefaultPreference is null/empty", invoiceLine.JI_PrimaryPreference);
			invoiceLine.JI_Tariff = "992002";
			AssertNullOrEmpty("TestDefaultPreference is null/empty", invoiceLine.JI_PrimaryPreference);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("TestDefaultPreference is correct", invoiceLine.JI_PrimaryPreference, "100");

			invoiceHeader.JZ_ROOType = "APE";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("Preference is defaulted to the value from the invoice for export declaration originating in ZA", "APE", invoiceLine2.JI_PrimaryPreference);
		}

		public void TestJI_ROOCertDefaultsIfNotBeenSet()
		{
			invoiceHeader.JZ_ROOType = "APE";
			invoiceHeader.JZ_ROOCert = "12345";

			invoiceLine.JI_PrimaryPreference = "APE";
			AssertEquals("JI_ROOCert defaulted when not previously been set", "12345", invoiceLine.JI_ROOCert);

			invoiceHeader.JZ_ROOType = "ABC";
			invoiceHeader.JZ_ROOCert = "99999";
			invoiceLine.JI_PrimaryPreference = "ABC";
			AssertEquals("JI_ROOCert NOT defaulted when has already been set", "12345", invoiceLine.JI_ROOCert);

			invoiceHeader.JZ_ROOType = "XYZ";
			invoiceHeader.JZ_ROOCert = "11111";
			invoiceLine.JI_ROOCert = ZString.Empty;
			invoiceLine.JI_PrimaryPreference = "XYZ";
			AssertEquals("JI_ROOCert defaulted when not previously been set", "11111", invoiceLine.JI_ROOCert);
		}

		public void TestCanRemoveValueFromJI_ROOCert()
		{
			invoiceHeader.JZ_ROOType = "APE";
			invoiceHeader.JZ_ROOCert = "12345";
			invoiceLine.JI_PrimaryPreference = "APE";
			AssertEquals("JI_ROOCert defaulted when not previously been set", "12345", invoiceLine.JI_ROOCert);

			invoiceLine.JI_ROOCert = ZString.Empty;
			AssertEquals("Can remove value from JI_ROOCert", ZString.Empty, invoiceLine.JI_ROOCert);
		}

		public void TestTradeAgreement()
		{
			SetupDefaultPreferenceAndTradeAgreement();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.JI_Tariff = "992002";
			AssertEquals("TestTradeAgreement Key is correct", invoiceLine.TradeAgreement.Key, "TG1");
			AssertEquals("TestTradeAgreement Value is correct", invoiceLine.TradeAgreement.Value, "TG1 DEC");
		}

		void SetupDefaultPreferenceAndTradeAgreement()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var preference = universalReferenceDataHelper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var rateType_ZA_REB = universalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = universalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var tariff = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "992002", new ZDateTime(2010, 08, 02), new ZDateTime(2079, 06, 06));
			var rate1 = universalReferenceDataHelper.CreateRate(tariff, rateCode_ZA_REB_D.PK, new ZDateTime(2016, 1, 1), new ZDateTime(2060, 12, 1), preferencePk: preference.PK);

			var testTradeGroup1 = universalReferenceDataHelper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "TG1", new ZDateTime(2001, 01, 01), new ZDateTime(2079, 06, 06), "TG1 DEC");
			universalReferenceDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2001, 01, 01), new ZDate(2079, 06, 06));
			universalReferenceDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.Germany, new ZDate(2001, 01, 01), new ZDate(2079, 06, 06));
			universalReferenceDataHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2001, 01, 01), new ZDate(2079, 06, 06));
			universalReferenceDataHelper.CreateCusApplicability(rate1, testTradeGroup1, new ZDateTime(2004, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
		}

		[TestDate(2016, 01, 01)]
		public void TestDefaultingINTCharge()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariff2 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "88888", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.IntellectualValue, UniversalReferenceConstants.TariffAttributes.IntellectualValue, tariff2);
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				AssertEquals("99999 - N", 0, invoiceLine.Charges.GetCharge("INT").Length);
				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertEquals("88888 - Y", 1, invoiceLine.Charges.GetCharge("INT").Length);
			});
		}

		public void TestClearDiamondProcessingFields()
		{
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991001", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			universalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.Diamond, "X", tariff1);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991002", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			Factory.Save();
			invoiceLine.JI_Tariff = "991001";
			invoiceLine.JI_DiamondBeneficiaryLicense = "1";
			invoiceLine.JI_DiamondDealerLicense = "2";
			invoiceLine.JI_TemporaryExportExemption = "3";
			invoiceLine.JI_DiamondLevyValue = 4;
			invoiceLine.JI_DiamondProducerRegistration = "5";
			invoiceLine.JI_DiamondProducerExemption = "6";
			invoiceLine.JI_ElectionsExemptionsLevy = "7";
			invoiceLine.JI_KimberleyCertificate = "8";
			invoiceLine.JI_TemporaryBuyersPermit = "9";
			CombineAssertions(() =>
			{
				AssertEquals("1", invoiceLine.JI_DiamondBeneficiaryLicense);
				AssertEquals("2", invoiceLine.JI_DiamondDealerLicense);
				AssertEquals("3", invoiceLine.JI_TemporaryExportExemption);
				AssertEquals(new ZDecimal(4), invoiceLine.JI_DiamondLevyValue);
				AssertEquals("5", invoiceLine.JI_DiamondProducerRegistration);
				AssertEquals("6", invoiceLine.JI_DiamondProducerExemption);
				AssertEquals("7", invoiceLine.JI_ElectionsExemptionsLevy);
				AssertEquals("8", invoiceLine.JI_KimberleyCertificate);
				AssertEquals("9", invoiceLine.JI_TemporaryBuyersPermit);
				invoiceLine.JI_Tariff = "991002";
				AssertEquals("", invoiceLine.JI_DiamondBeneficiaryLicense);
				AssertEquals("", invoiceLine.JI_DiamondDealerLicense);
				AssertEquals("", invoiceLine.JI_TemporaryExportExemption);
				AssertEquals(new ZDecimal(0), invoiceLine.JI_DiamondLevyValue);
				AssertEquals("", invoiceLine.JI_DiamondProducerRegistration);
				AssertEquals("", invoiceLine.JI_DiamondProducerExemption);
				AssertEquals("", invoiceLine.JI_ElectionsExemptionsLevy);
				AssertEquals("", invoiceLine.JI_KimberleyCertificate);
				AssertEquals("", invoiceLine.JI_TemporaryBuyersPermit);
			});
		}

		public void TestCpcAndPpc()
		{
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "10", "10", "", "", ZAJobMessageTypeList.Codes.Import);
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "10";
			invoiceLine.JI_CEI = instruction.PK;
			AssertStartsWith("A 10-series procedure has been selected from the CEI style", "10", invoiceLine.JI_Procedure);
		}

		public void TestPreviousProcedureCodes()
		{
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "83", "", "", "", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "", "", "", ZAJobMessageTypeList.Codes.Export);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "83", "80", "", "", ZAJobMessageTypeList.Codes.Export);
			universalReferenceDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "40", "", "", ZAJobMessageTypeList.Codes.ExBond);
			Factory.Save();

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "83";
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = "XX";
			CombineAssertions("Test PPC Export", () =>
			{
				invoiceLine.JI_CEI = testInstruction1.PK;
				AssertEquals("TEST 1", ZString.Empty, invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 1 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = testInstruction2.PK;
				AssertEquals("TEST 2", "80", invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 2 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = testInstruction3.PK;
				AssertEquals("TEST 3", ZString.Empty, invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 3 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = ZGuid.Empty;
				AssertEquals("TEST 4", "", invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 4 Readonly", invoiceLine.JI_Procedure_ReadOnly);
			});
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			CombineAssertions("Test PPC Import", () =>
			{
				invoiceLine.JI_CEI = testInstruction1.PK;
				AssertEquals("TEST 1", "00", invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 1 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = testInstruction2.PK;
				AssertEquals("TEST 2", ZString.Empty, invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 2 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = testInstruction3.PK;
				AssertEquals("TEST 3", ZString.Empty, invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 3 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = ZGuid.Empty;
				AssertEquals("TEST 4", "", invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 4 Readonly", invoiceLine.JI_Procedure_ReadOnly);
			});
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			CombineAssertions("Test PPC ExBond", () =>
			{
				invoiceLine.JI_CEI = testInstruction1.PK;
				AssertEquals("TEST 1", "40", invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 1 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = testInstruction2.PK;
				AssertEquals("TEST 2", ZString.Empty, invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 2 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = testInstruction3.PK;
				AssertEquals("TEST 3", ZString.Empty, invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 3 Readonly", !invoiceLine.JI_Procedure_ReadOnly);
				invoiceLine.JI_CEI = ZGuid.Empty;
				AssertEquals("TEST 4", "", invoiceLine.JI_Calc_PreviousProcedure);
				Assert("TEST 4 Readonly", invoiceLine.JI_Procedure_ReadOnly);
			});
		}

		public void TestIsDA63()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			Assert(!invoiceLine.IsDA63);
			invoiceLine.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			Assert(!invoiceLine.IsDA63);
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			invoiceLine.JI_CEI = testInstruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}XX";
			Assert(!invoiceLine.IsDA63);
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}YY";
			Assert(invoiceLine.IsDA63);
		}

		public void TestCustomsQtyFromRelatedImportBOE()
		{
			SetupCustomsQtyFromRelatedImportBOE();
			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}XX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, testLine.CustomsQtyFromRelatedImportBOE);
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, testLine.CustomsQtyFromRelatedImportBOE);
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			AssertEquals(200m, testLine.CustomsQtyFromRelatedImportBOE);
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, testLine.CustomsQtyFromRelatedImportBOE);
			testLine.JI_PreviousEntryNumber = "TestMRN1";
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, testLine.CustomsQtyFromRelatedImportBOE);
		}

		public void TestCustomsQtyUQFromRelatedImportBOE()
		{
			SetupCustomsQtyFromRelatedImportBOE();
			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}XX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals("", testLine.CustomsQtyUQFromRelatedImportBOE);
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals("", testLine.CustomsQtyUQFromRelatedImportBOE);
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			AssertEquals("KG", testLine.CustomsQtyUQFromRelatedImportBOE);
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals("", testLine.CustomsQtyUQFromRelatedImportBOE);
			testLine.JI_PreviousEntryNumber = "TestMRN1";
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals("", testLine.CustomsQtyUQFromRelatedImportBOE);
		}

		void SetupCustomsQtyFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "YY";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "KG";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = "TESTTRF1";
			entryLine.CL_LineNumber = 2;
			Factory.Save();
		}

		public void TestCustomsValueFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_AdValoremTariff = "TESTTRF1";
			entryLine.CL_LineNumber = 2;
			entryLine.CL_CustomsValue = 2000m;
			Factory.Save();

			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			invoiceLine.JI_CEI = testInstruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}XX";
			invoiceLine.JI_PreviousEntryNumber = "TestMRN";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, invoiceLine.CustomsValueFromRelatedImportBOE);
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, invoiceLine.CustomsValueFromRelatedImportBOE);
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}YY";
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(2000m, invoiceLine.CustomsValueFromRelatedImportBOE);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, invoiceLine.CustomsValueFromRelatedImportBOE);
			invoiceLine.JI_PreviousEntryNumber = "TestMRN1";
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, invoiceLine.CustomsValueFromRelatedImportBOE);
		}

		public void TestCustomsDutyFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_AdValoremTariff = "TESTTRF1";
			entryLine.CL_LineNumber = 2;
			entryLine.Fees.AddOrUpdate("1P1", 21, isLandedCostOnly: false);
			entryLine.Fees.AddOrUpdate("12B", 22, isLandedCostOnly: false);
			entryLine.Fees.AddOrUpdate("12A", 23, isLandedCostOnly: false);
			entryLine.Fees.AddOrUpdate("2P2", 24, isLandedCostOnly: false);
			Factory.Save();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "XX";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}XX";
			invoiceLine.JI_PreviousEntryNumber = "TestMRN";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, invoiceLine.CustomsDutyFromRelatedImportBOE);
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, invoiceLine.CustomsDutyFromRelatedImportBOE);
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}YY";
			AssertEquals(45m, invoiceLine.CustomsDutyFromRelatedImportBOE);
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, invoiceLine.CustomsDutyFromRelatedImportBOE);
			invoiceLine.JI_PreviousEntryNumber = "TestMRN1";
			invoiceLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, invoiceLine.CustomsDutyFromRelatedImportBOE);
		}

		public void TestDutySch1P2BFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "YY";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "KG";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_AdValoremTariff = "TESTTRF1";
			entryLine.CL_LineNumber = 2;
			entryLine.Fees.AddOrUpdate("1P1", 21);
			entryLine.Fees.AddOrUpdate("12B", 22);
			Factory.Save();

			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}XX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.S1P2BDuty?.OriginValue ?? 0m);
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.S1P2BDuty?.OriginValue ?? 0m);
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(22m, testLine.DA63AdditionalDuties.S1P2BDuty?.OriginValue ?? 0m);
			testLine.JI_PreviousEntryLineNumber = 1;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.S1P2BDuty?.OriginValue ?? 0m);
			testLine.JI_PreviousEntryNumber = "TestMRN1";
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.S1P2BDuty?.OriginValue ?? 0m);
		}

		public void TestVATFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			invoiceLine.JI_CL = testEntryLine.PK;
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			testEntryLine.CL_LineNumber = 2;
			Factory.Save();

			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}XX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, testLine.VATFromRelatedImportBOE);
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, testLine.VATFromRelatedImportBOE);
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			AssertEquals(23m, testLine.VATFromRelatedImportBOE);
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(0m, testLine.VATFromRelatedImportBOE);
			testLine.JI_PreviousEntryNumber = "TestMRN1";
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(0m, testLine.VATFromRelatedImportBOE);
		}

		public void TestProvisionalPaymentFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "KG";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testEntryLine.CL_AdValoremTariff = "TESTTRF1";
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			Factory.Save();
			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}XX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.OriginValue));
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.OriginValue));
			testLine.JI_Procedure = $"{testLine.EntryInstruction.CEI_Style}YY";
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.OriginValue));
			testLine.JI_PreviousEntryLineNumber = 1;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.OriginValue));
			testLine.JI_PreviousEntryNumber = "TestMRN1";
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.ProvisionalPayments.Sum(x => x.OriginValue));
		}

		public void TestPenaltyFromRelatedImportBOE()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			invoiceLine.JI_CustomsQuantity = 200;
			invoiceLine.JI_CustomsUnitQty = "KG";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testEntryLine.CL_AdValoremTariff = "TESTTRF1";
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			invoiceLine.JI_CL = testEntryLine.PK;
			Factory.Save();

			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = "XXXX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.OriginValue));
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.OriginValue));
			testLine.JI_Procedure = "XXYY";
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.OriginValue));
			testLine.JI_PreviousEntryLineNumber = 1;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.OriginValue));
			testLine.JI_PreviousEntryNumber = "TestMRN1";
			testLine.JI_PreviousEntryLineNumber = 2;
			testLine.RepopulateDA63AdditionalDuties();
			AssertEquals(0m, testLine.DA63AdditionalDuties.Penalties.Sum(x => x.OriginValue));
		}

		public void TestRelatedImportBOENumberString()
		{
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = declaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			invoiceLine.JI_CustomsQuantity = 150;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInvLine2 = invoiceHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testEntryLine.CL_LineNumber = 2;
			invoiceLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			Factory.Save();

			var expected = "Original Entry not found, all DA63 values must be manually entered";
			var testLine = invoiceHeader.InvoiceLines.AddNew();
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = "00XX";
			testLine.JI_PreviousEntryNumber = "TestMRN";
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(expected, testLine.RelatedImportBOENumberString);
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(ZString.Empty, testLine.RelatedImportBOENumberString);
			testLine.JI_Procedure = "00YY";
			AssertEquals(ZString.Empty, testLine.RelatedImportBOENumberString);
			testLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(expected, testLine.RelatedImportBOENumberString);
			testLine.JI_PreviousEntryNumber = "TestMRN1";
			testLine.JI_PreviousEntryLineNumber = 2;
			AssertEquals(expected, testLine.RelatedImportBOENumberString);
		}

		public void TestConcurrencyExceptionHandlingForAddInfo()
		{
			invoiceLine.JI_ROOCert = "Cert 1";
			Factory.RefreshEnabled = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceReloaded = newFactory.Load<JobComInvoiceHeader>(invoiceHeader.PK);
			invoiceReloaded.InvoiceLines[0].JI_ROOCert = "Cert 2";
			newFactory.RefreshEnabled = false;
			newFactory.Save();

			invoiceLine.Delete();
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());
			ZExceptionReporting.HandleSaveException(ex);
		}

		public void TestCopyProductDetailsToCusVehicle()
		{
			var supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_IsConsignor = true;
			var part = CreateProduct(supplier, "NEWPROD10", "PRODUCT10");
			Factory.Save();
			var pivot = AddPivot(part, "1010101012", Core.Constants.CountryCodes.SouthAfrica);
			pivot.CI_EngineCapacity = 5000;
			pivot.CI_VehicleFormat = "OTH";
			pivot.CI_VehicleType = "Truck";
			invoiceLine.SetTariffEtcDataFromProductsPivot(pivot);
			AssertEquals(5000, invoiceLine.JI_EngineCapacity);
			pivot.CI_EngineCapacity = 9999999;
			invoiceLine.SetTariffEtcDataFromProductsPivot(pivot);
			AssertEquals(ZShort.Zero, invoiceLine.JI_EngineCapacity);
		}

		public void TestListOfMigratedAddInfoProperties()
		{
			AssertEquals(JobComInvoiceLine.Schema.JI_Colour, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_Colour));
			AssertEquals(JobComInvoiceLine.Schema.JI_EngineCapacity, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_EngineCapacity));
			AssertEquals(JobComInvoiceLine.Schema.JI_EngineNumber, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_EngineNumber));
			AssertEquals(JobComInvoiceLine.Schema.JI_Make, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_Make));
			AssertEquals(JobComInvoiceLine.Schema.JI_VehicleFormat, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_VehicleFormat));
			AssertEquals(JobComInvoiceLine.Schema.JI_VehicleType, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_VehicleType));
			AssertEquals(JobComInvoiceLine.Schema.JI_VIN, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_VIN));
			AssertEquals(JobComInvoiceLine.Schema.JI_YearOfManufacture, true, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_YearOfManufacture));
			AssertEquals(JobComInvoiceLine.Schema.JI_Model, false, JobComInvoiceLine.IsMigratedAddInfoProperties(JobComInvoiceLine.Schema.JI_Model));
		}

		public void TestVehicle()
		{
			AssertType<CusVehicle>(invoiceLine.FirstVehicle);
			AssertEquals("VehicleRelationshipType should be One", VehicleRelationshipType.One, invoiceLine.VehicleRelationship);
		}

		public void TestColor()
		{
			invoiceLine.JI_Colour = "RED";
			AssertEquals("RED", invoiceLine.FirstVehicle.CVH_Color);
			invoiceLine.FirstVehicle.CVH_Color = "BLUE";
			AssertEquals("BLUE", invoiceLine.JI_Colour);
			AssertEquals(35, invoiceLine.JI_ColourInfo.MaxLength);
		}

		public void TestEngineCapacity()
		{
			invoiceLine.JI_EngineCapacity = 2000;
			AssertEquals((short)2000, invoiceLine.FirstVehicle.CVH_EngineCapacity);
			AssertEquals("CC", invoiceLine.FirstVehicle.CVH_EngineCapacityUQ);
			invoiceLine.FirstVehicle.CVH_EngineCapacity = 3000;
			AssertEquals(3000, invoiceLine.JI_EngineCapacity);
		}

		public void TestEngineNumber()
		{
			invoiceLine.JI_EngineNumber = "A10000001";
			AssertEquals("A10000001", invoiceLine.FirstVehicle.CVH_SerialNumber);
			invoiceLine.FirstVehicle.CVH_SerialNumber = "B10000001";
			AssertEquals("B10000001", invoiceLine.JI_EngineNumber);
			AssertEquals(20, invoiceLine.JI_EngineNumberInfo.MaxLength);
		}

		public void TestMake()
		{
			invoiceLine.JI_Make = "BMW Z4";
			AssertEquals("BMW Z4", invoiceLine.FirstVehicle.CVH_ModelName);
			invoiceLine.FirstVehicle.CVH_ModelName = "Ferrari Testarossa";
			AssertEquals("Ferrari Testarossa", invoiceLine.JI_Make);
			AssertEquals(35, invoiceLine.JI_MakeInfo.MaxLength);
		}

		public void TestVehicleFormat()
		{
			invoiceLine.JI_VehicleFormat = VehicleFormatList.Codes.OTH;
			AssertEquals(VehicleFormatList.Codes.OTH, invoiceLine.FirstVehicle.CVH_SupplyMethod);
			invoiceLine.FirstVehicle.CVH_SupplyMethod = VehicleFormatList.Codes.FBU;
			AssertEquals(VehicleFormatList.Codes.FBU, invoiceLine.JI_VehicleFormat);
			AssertEquals(3, invoiceLine.JI_VehicleFormatInfo.MaxLength);
		}

		public void TestVehicleType()
		{
			invoiceLine.JI_VehicleType = VehicleTypeList.Codes.Truck;
			AssertEquals(VehicleTypeList.Codes.Truck, invoiceLine.FirstVehicle.CVH_CarType);
			invoiceLine.FirstVehicle.CVH_CarType = VehicleTypeList.Codes.Bus;
			AssertEquals(VehicleTypeList.Codes.Bus, invoiceLine.JI_VehicleType);
			AssertEquals(17, invoiceLine.JI_VehicleTypeInfo.MaxLength);
		}

		public void TestVIN()
		{
			invoiceLine.JI_VIN = "12345678901234567";
			AssertEquals("12345678901234567", invoiceLine.FirstVehicle.CVH_VehicleIdentificationNumber);
			invoiceLine.FirstVehicle.CVH_VehicleIdentificationNumber = "ABCDEFGHIJKLMNOPQ";
			AssertEquals("ABCDEFGHIJKLMNOPQ", invoiceLine.JI_VIN);
			AssertEquals(17, invoiceLine.JI_VINInfo.MaxLength);
		}

		public void TestYearOfManufacture()
		{
			invoiceLine.JI_YearOfManufacture = "2025";
			AssertEquals(new ZDateTime(2025, 1, 1), invoiceLine.FirstVehicle.CVH_ManufacturedDate);
			invoiceLine.FirstVehicle.CVH_ManufacturedDate = new ZDate(2026, 1, 1);
			AssertEquals("2026", invoiceLine.JI_YearOfManufacture);
			invoiceLine.JI_YearOfManufacture = "XXXX";
			AssertEquals(ZString.Empty, invoiceLine.JI_YearOfManufacture);
			AssertEquals(4, invoiceLine.JI_YearOfManufactureInfo.MaxLength);
		}

		OrgHeader CreateOrganisation(ZString code, bool isConsignee, bool isConsignor)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = $"{code} NAME";
			org.OH_IsConsignee = isConsignee;
			org.OH_IsConsignor = isConsignor;
			org.MainAddress.Address1 = $"{code} ADDRESS 1";
			return org;
		}

		OrgSupplierPart CreateProduct(OrgHeader supplier, ZString productNo, ZString productDesc)
		{
			var product = Factory.NewMoq<OrgSupplierPart>().Object;
			product.OP_StockKeepingUnit = "BAG";
			product.OP_PartNum = productNo;
			product.OP_Desc = productDesc;
			var relatedOrganization = product.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = supplier.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			return product;
		}

		OrgSupplierPart CreatePart(ZString partNo, OrgHeader owner, OrgHeader supplier)
		{
			var part = Factory.NewMoq<OrgSupplierPart>().Object;
			part.OP_PartNum = partNo;
			part.OP_Desc = $"{partNo} DESC";
			part.OP_StockKeepingUnit = "NO";

			if (owner != null)
			{
				part.RelatedOrganisations.AddOwner(owner);
			}

			if (supplier != null)
			{
				part.RelatedOrganisations.AddSupplier(supplier);
			}

			return part;
		}

		void AddCharge(ZString chargeCode, ZDecimal amount, ZString currency, ZBool isDutiable, ZBool isIncludedInITOT)
		{
			var charge = invoiceLine.Charges.AddNew(chargeCode, amount, currency);
			charge.J7_IsDutiable = isDutiable;
			charge.J7_IsIncludedInITOT = isIncludedInITOT;
		}

		void AddFee(ZString chargeType, ZDecimal amount, ZBool isLandedCostOnly)
		{
			var fee = invoiceLine.CusEntryLine.Fees.AddNew();
			fee.CF_ChargeType = chargeType;
			fee.CF_ChargeAmount = amount;
			fee.CF_IsLandedCostOnly = isLandedCostOnly;
		}

		CusClassPartPivot AddPivot(OrgSupplierPart part, ZString tariffNum, ZString countryOrigin)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_OH = part.RelatedOrganisations.OfType<OrgPartRelation>().FirstOrDefault()?.OU_OH ?? ZGuid.Empty;
			pivot.CI_TariffNum = tariffNum;
			pivot.CI_RN_NKCountryOfOrigin = countryOrigin;
			pivot.CI_PrimaryPreference = "EUTRADE";
			pivot.CI_ROOCert = "CERT";
			pivot.CI_NewUsed = "U";
			pivot.CI_EngineCapacity = 3000;
			pivot.CI_VehicleFormat = VehicleFormatList.Codes.FBU;
			pivot.CI_VehicleType = VehicleTypeList.Codes.Passenger;
			pivot.CI_Colour = "WHITE";
			return pivot;
		}

		CusClassPartPivot AddPivotWithAttributes(OrgSupplierPart part, ZString childType, ZGuid ownRelationPK, ZString tariffNum, ZString attrib1Value, ZString attrib2Value, ZString attrib3Value)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = childType;
			pivot.CI_OH = ownRelationPK;
			pivot.CI_TariffNum = tariffNum;
			var attrib1 = pivot.Attributes1.AddNew();
			attrib1.BG_AttributeName = nameof(CusAttributeFilter.AttributeFilterName.AT1);
			attrib1.BG_AttributeValue1 = attrib1Value;
			var attrib2 = pivot.Attributes2.AddNew();
			attrib2.BG_AttributeName = nameof(CusAttributeFilter.AttributeFilterName.AT2);
			attrib2.BG_AttributeValue1 = attrib2Value;
			var attrib3 = pivot.Attributes3.AddNew();
			attrib3.BG_AttributeName = nameof(CusAttributeFilter.AttributeFilterName.AT3);
			attrib3.BG_AttributeValue1 = attrib3Value;
			return pivot;
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			foreach (var line in declaration.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				line.JI_CEI = instruction.PK;
			}
		}

		protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
		protected override ZString UniversalTariffTypeForDefaultTaxOrFeeCode => UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
		protected override ZString UniversalTariffType => UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => invoiceLine;

		protected override BusinessObject GetNewBusinessObject()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			return invoiceLine;
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			new LineMerger((JobDeclaration)declaration).DoMerge();
		}

		protected override void SetUp()
		{
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();

			declaration = Factory.NewMoq<JobDeclaration>().Object;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}

	sealed class InvoiceLineForUOMSuspendDefaultingForTest : JobComInvoiceLine
	{
		public InvoiceLineForUOMSuspendDefaultingForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		internal int AdditionalUnitSetCount;
		public override ZString JI_CustomsSecondUnitQty
		{
			get
			{
				return base.JI_CustomsSecondUnitQty;
			}

			set
			{
				base.JI_CustomsSecondUnitQty = value;
				AdditionalUnitSetCount++;
			}
		}
	}

	sealed class JobComInvoiceLineForTest : JobComInvoiceLine
	{
		public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString[] ConcessionTariffTypes_Exposed => base.ConcessionTariffTypes;
		public ZString[] ImportShipmentTariffTypes_Exposed => base.ImportShipmentTariffTypes;
		public ZString[] ExportShipmentTariffTypes_Exposed => base.ExportShipmentTariffTypes;
	}
}
