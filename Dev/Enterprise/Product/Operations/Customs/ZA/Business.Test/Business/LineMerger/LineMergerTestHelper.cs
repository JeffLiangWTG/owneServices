using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.DataRegistry.Business;
using static Enterprise.Customs.Universal.Constants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	class LineMergerTestHelper
	{
		public LineMergerTestHelper(BusinessObjectFactory factory, ZAUniversalReferenceTestDataHelper helper)
		{
			this.factory = factory;
			this.helper = helper;
		}

		static public FinancialAccountNumberPortMap AddMapping(FinancialAccountNumberPortMapCollection collection, ZGuid orgPk, ZString customsOfficeCode, ZString financialAccountNumber, ZBool importerPays, ZInt accountStartDay)
		{
			var mapping = collection.AddNew();
			mapping.OrganizationPK = orgPk;
			mapping.CreditorPK = orgPk;
			mapping.CustomsOfficeCode = customsOfficeCode;
			mapping.FinancialAccountNumber = financialAccountNumber;
			mapping.AccountStartDay = accountStartDay;
			mapping.Cash = false;
			mapping.ImporterPays = importerPays;
			return mapping;
		}

		public CusRefRateCodeView RateCode_ZA_DTY_D { get; private set; }
		public CusRefPreferenceView Preference { get; private set; }
		public RefCusTariffType TariffType1P1 { get; private set; }
		public TariffView Tariff1P1 { get; private set; }
		public TariffView Tariff1P1_2 { get; private set; }
		public TariffView Tariff12A { get; private set; }
		public TariffView Tariff15A { get; private set; }
		public TariffView Tariff15B { get; private set; }
		public RateView Tariff1P1Rate { get; private set; }
		public RateView Tariff12ARate { get; private set; }
		public RateView Tariff15ARate { get; private set; }
		public RateView Tariff15BRate { get; private set; }
		public RefCusProcedure Procedure2 { get; private set; }
		public CusRefTradeGroupView TradeGroup { get; private set; }

		public void CreateBasicTariff()
		{
			TariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
			RateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_DTY.PK);
			Preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			factory.Save();
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			var startDate = ZDateTime.Today;
			var endDate = ZDateTime.Today.AddYears(1);
			helper.CreateTaxOrFee(TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			TradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.AddCountry(TradeGroup, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType1P1.PK, "99999", startDate, endDate, taxOrFeeCode: "VAT");
			Tariff1P1Rate = helper.CreateRate(Tariff1P1, RateCode_ZA_DTY_D.PK, startDate, endDate, "0.3 * VFD", Preference.PK);
			helper.CreateCusApplicability(Tariff1P1Rate, TradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			factory.Save();
		}

		public void CreateTariffWithTwoDates(ZDateTime rate1End, ZDateTime rate2Start, ZDateTime rate2End)
		{
			TariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
			RateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_DTY.PK);
			factory.Save();
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			Tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType1P1.PK, "27101203", startDate, endDate, taxOrFeeCode: "VAT");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			var tariff1P1Rate1 = helper.CreateRate(Tariff1P1, RateCode_ZA_DTY_D.PK, startDate, rate1End, "0.01 * VFD");
			var tariff1P1Rate2 = helper.CreateRate(Tariff1P1, RateCode_ZA_DTY_D.PK, rate2Start, rate2End, "0.02 * VFD");
			helper.CreateTaxOrFee(TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			factory.Save();
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.China, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(tariff1P1Rate1, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff1P1Rate2, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			factory.Save();
		}

		public void SetupTariffsForBNDTests()
		{
			TariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var tariffType15A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "15A");
			var tariffType15B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "15B");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_DTY.PK);
			var rateType_ZA_LVY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Levy);
			var rateCode_ZA_LVY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_LVY.PK);
			var rateType_ZA_EXC = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Excise);
			var rateCode_ZA_EXC_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_EXC.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			factory.Save();
			var startDate = ZDateTime.Today;
			var endDate = ZDateTime.Today.AddYears(1);
			Tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType1P1.PK, "1111111", startDate, endDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(Tariff1P1, "CU1", "LI");
			Tariff12A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1111112", startDate, endDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(Tariff12A, "CU1", "LI");
			Tariff15A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType15A.PK, "1111151", startDate, endDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(Tariff15A, "CU1", "LI");
			Tariff15B = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType15B.PK, "1111152", startDate, endDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(Tariff15B, "CU1", "LI");
			helper.CreateTariffRelationship(Tariff12A.PK, Tariff1P1.ZZ1_ZZI_TariffType, Tariff1P1.ZZ1_TariffCode);
			helper.CreateTariffRelationship(Tariff15A.PK, Tariff1P1.ZZ1_ZZI_TariffType, Tariff1P1.ZZ1_TariffCode);
			helper.CreateTariffRelationship(Tariff15B.PK, Tariff1P1.ZZ1_ZZI_TariffType, Tariff1P1.ZZ1_TariffCode);
			helper.CreateTaxOrFee(TaxOrFeeTypeCode.VAT, 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			Tariff1P1Rate = helper.CreateRate(Tariff1P1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.00035 * [LI]", preference.PK);
			Tariff12ARate = helper.CreateRate(Tariff12A, rateCode_ZA_EXC_D.PK, startDate, endDate, "0.03909 * [LI]", preference.PK);
			Tariff15ARate = helper.CreateRate(Tariff15A, rateCode_ZA_LVY_D.PK, startDate, endDate, "2.55 * [LI]", preference.PK);
			Tariff15BRate = helper.CreateRate(Tariff15B, rateCode_ZA_LVY_D.PK, startDate, endDate, "1.54 * [LI]", preference.PK);
			helper.CreateCustomsStatusCusCodeEntry("1");
			helper.CreateAdditionalInformationCusCodeEntry("BND");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, ProcedureCategoryCodes._E, ProcedureCodes._40, "", "", "", ZAJobMessageTypeList.Codes.Import, false, true);
			Procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, ProcedureCategoryCodes._E, ProcedureCodes._40, ProcedureCodes._41, "", "", ZAJobMessageTypeList.Codes.Import, false, true);
			factory.Save();
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.China, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(Tariff1P1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(Tariff12ARate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(Tariff15ARate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(Tariff15BRate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			factory.Save();
		}

		public void SetupTariffsForBNDCalculationForExportsWithoutTaxField()
		{
			TariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var preference2 = helper.CreatePreferenceForCountryAndGrouping("200", "Special", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			factory.Save();
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			Tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType1P1.PK, "1111111", startDate, endDate, taxOrFeeCode: "VAT");
			helper.CreateTariffUOM(Tariff1P1, "CU1", "LI");
			Tariff1P1_2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType1P1.PK, "2222222", startDate, endDate, taxOrFeeCode: "VEX");
			helper.CreateTariffUOM(Tariff1P1_2, "CU1", "LI");
			helper.CreateTaxOrFee(TaxOrFeeTypeCode.VAT, 0.15, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			Tariff1P1Rate = helper.CreateRate(Tariff1P1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.1 * [LI]", preference.PK);
			var tariff1P1Rate2 = helper.CreateRate(Tariff1P1_2, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.1 * [LI]", preference.PK);
			var tariff1P1Rate200 = helper.CreateRate(Tariff1P1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.05 * [LI]", preference2.PK);
			var tariff1P1Rate2002 = helper.CreateRate(Tariff1P1_2, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.05 * [LI]", preference2.PK);
			helper.CreateCustomsStatusCusCodeEntry("1");
			helper.CreateAdditionalInformationCusCodeEntry("BND");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, ProcedureCategoryCodes._E, ProcedureCodes._67, "", "", "", ZAJobMessageTypeList.Codes.Export, false, true);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, ProcedureCategoryCodes._E, ProcedureCodes._67, ProcedureCodes._40, "", "", ZAJobMessageTypeList.Codes.Export, false, true);
			factory.Save();
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.China, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(Tariff1P1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff1P1Rate2, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff1P1Rate200, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff1P1Rate2002, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			factory.Save();
		}

		public JobDeclaration GetTestDeclaration()
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		public CusEntryInstruction AddEntryInstruction(JobDeclaration declaration, string procedureCode)
		{
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = procedureCode;
			return entryInstruction;
		}

		public JobComInvoiceLine AddInvoiceAndInvoiceLine(JobDeclaration declaration, CusEntryInstruction entryInstruction, decimal linePrice, decimal customsQuantity, string customsUnitQuantity)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = linePrice;
			var invoiceLine = AddInvoiceLine(invoice, entryInstruction, Tariff1P1.ZZ1_TariffCode, linePrice);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._41;
			invoiceLine.JI_CustomsQuantity = customsQuantity;
			invoiceLine.JI_CustomsUnitQty = customsUnitQuantity;
			return invoiceLine;
		}

		public JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, CusEntryInstruction entryInstruction, string tariff, decimal linePrice)
		{
			var line = invoice.InvoiceLines.AddNew();
			line.JI_CEI = entryInstruction.PK;
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			line.JI_Procedure = entryInstruction.CEI_Style + ProcedureCodes._00;
			line.JI_PrimaryPreference = PrimaryPreference.Standard;
			line.JI_Tariff = tariff;
			line.JI_LinePrice = linePrice;
			return line;
		}

		public JobDeclaration GetTestDeclarationWithInvoiceLine()
		{
			CreateBasicTariff();
			var declaration = GetTestDeclaration();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_CustomsOffice = "DFM";
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invHeader.JZ_InvoiceAmount = 1m;
			invHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invHeader.JZ_InvoiceNumber = "INV1";
			var invLine1 = AddInvoiceLine(invHeader, instruction1, "99999", 75m);
			invLine1.JI_ZZF_NKTaxType = TaxOrFeeTypeCode.VAT;
			return declaration;
		}

		readonly BusinessObjectFactory factory;
		readonly ZAUniversalReferenceTestDataHelper helper;
	}
}
