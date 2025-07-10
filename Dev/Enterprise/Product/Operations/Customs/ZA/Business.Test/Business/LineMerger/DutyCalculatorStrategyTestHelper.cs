using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	class DutyCalculatorStrategyTestHelper
	{
		public DutyCalculatorStrategyTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
			Helper = new ZAUniversalReferenceTestDataHelper(factory);
		}

		public ZDateTime StartDate { get; set; } = ZDateTime.Today;
		public ZDateTime EndDate { get; set; } = ZDateTime.Today;
		public string Tariff1P1RateFormula { get; set; } = "0.10 * VFD";
		public string Tariff12ARateFormula { get; set; } = "(VFD+1P1)*0.1";
		public string Tariff12BRateFormula { get; set; } = "VFD*0.1";
		public string Tariff13ARateFormula { get; set; } = "(VFD+1P1+12A+12B)*0.1";
		public string Tariff2P1RateFormula { get; set; } = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B)*0.1";
		public string TariffRebateRateFormula { get; set; } = "VFD";
		public string DutyRateCode { get; set; } = "D";
		public string ExciseRateCode { get; set; } = "12A";
		public string LevyRateCode { get; set; } = "13A";
		public string AdValoremExciseRateCode { get; set; } = "12B";
		public string AntiDumpingRateCode { get; set; } = "2P1";
		public string RebateRateCode { get; set; } = "3P1";
		public string TariffTypeCodeForRebate { get; set; } = "3P1";
		public string TariffTypeCodeForAntiDumping { get; set; } = "2P1";
		public string TariffTypeCodeForAdValoremExcise { get; set; } = "12B";

		public RefCusTariffType TariffType1P1 { get; private set; }
		public RefCusTariffType TariffType12A { get; private set; }
		public RefCusTariffType TariffType12B { get; private set; }
		public RefCusTariffType TariffType13A { get; private set; }
		public RefCusTariffType TariffType2P1 { get; private set; }
		public RefCusTariffType TariffTypeRebate { get; private set; }
		public RefCusRateType RateType_ZA_DTY { get; private set; }
		public RefCusRateType RateType_ZA_LVY { get; private set; }
		public RefCusRateType RateType_ZA_EXC { get; private set; }
		public RefCusRateType RateType_ZA_REB { get; private set; }
		public RefCusRateType RateType_ZA_EX1 { get; private set; }
		public RefCusRateType RateType_ZA_ADD { get; private set; }
		public RefCusRateType RateType_ZA_REF { get; private set; }
		public CusRefRateCodeView RateCode_ZA_DTY_D { get; private set; }
		public CusRefRateCodeView RateCode_ZA_LVY_D { get; private set; }
		public CusRefRateCodeView RateCode_ZA_EXC_D { get; private set; }
		public CusRefRateCodeView RateCode_ZA_REB_D { get; private set; }
		public CusRefRateCodeView RateCode_ZA_EX1_D { get; private set; }
		public CusRefRateCodeView RateCode_ZA_ADD_D { get; private set; }
		public CusRefRateCodeView RateCode_ZA_REF_D { get; private set; }
		public CusRefPreferenceView Preference { get; set; }
		public Func<ZAUniversalReferenceTestDataHelper, CusRefPreferenceView> CreatePreferenceFunc { get; set; }

		public TariffView Tariff1 { get; private set; }
		public TariffView Tariff2 { get; private set; }
		public TariffView Tariff3 { get; private set; }
		public TariffView Tariff4 { get; private set; }
		public TariffView Tariff10 { get; private set; }
		public TariffView Tariff13 { get; private set; }

		public RateView Tariff1Rate { get; private set; }
		public RateView Tariff2Rate { get; private set; }
		public RateView Tariff3Rate { get; private set; }
		public RateView Tariff4Rate { get; private set; }
		public RateView Tariff10Rate { get; private set; }
		public RateView Tariff13Rate { get; private set; }

		public CusRefTradeGroupView EUTradeGroup { get; private set; }
		public CusRefTradeGroupView StandardTradeGroup { get; private set; }

		public ZAUniversalReferenceTestDataHelper Helper { get; }

		public DutyCalculatorStrategyTestHelper CreateTariffTypesAndRateCodes()
		{
			TariffType1P1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			TariffType12A = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			TariffType12B = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, TariffTypeCodeForAdValoremExcise);
			TariffType13A = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			TariffType2P1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, TariffTypeCodeForAntiDumping);
			TariffTypeRebate = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, TariffTypeCodeForRebate);
			RateType_ZA_DTY = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			RateCode_ZA_DTY_D = Helper.LoadOrCreateNewCusRateCode(factory, DutyRateCode, RateType_ZA_DTY.PK);
			RateType_ZA_LVY = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Levy);
			RateCode_ZA_LVY_D = Helper.LoadOrCreateNewCusRateCode(factory, LevyRateCode, RateType_ZA_LVY.PK);
			RateType_ZA_EXC = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Excise);
			RateCode_ZA_EXC_D = Helper.LoadOrCreateNewCusRateCode(factory, ExciseRateCode, RateType_ZA_EXC.PK);
			RateType_ZA_REB = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			RateCode_ZA_REB_D = Helper.LoadOrCreateNewCusRateCode(factory, RebateRateCode, RateType_ZA_REB.PK);
			RateType_ZA_EX1 = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AdValoremExcise);
			RateCode_ZA_EX1_D = Helper.LoadOrCreateNewCusRateCode(factory, AdValoremExciseRateCode, RateType_ZA_EX1.PK);
			RateType_ZA_ADD = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AntiDumping);
			RateCode_ZA_ADD_D = Helper.LoadOrCreateNewCusRateCode(factory, AntiDumpingRateCode, RateType_ZA_ADD.PK);
			RateType_ZA_REF = Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			RateCode_ZA_REF_D = Helper.LoadOrCreateNewCusRateCode(factory, "6P4", RateType_ZA_REF.PK);
			Preference = CreatePreferenceFunc?.Invoke(Helper);
			factory.Save();
			return this;
		}

		public DutyCalculatorStrategyTestHelper CreateTariff1RateOnly()
		{
			Tariff1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType1P1.PK, "1010101011", StartDate, EndDate, taxOrFeeCode: "VAT");
			Tariff1Rate = Helper.CreateRate(Tariff1, RateCode_ZA_DTY_D.PK, StartDate, EndDate, Tariff1P1RateFormula, Preference?.PK);
			return this;
		}

		public DutyCalculatorStrategyTestHelper CreateTaxOrFeesForVAT()
		{
			Helper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Exempt");
			Helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, StartDate, EndDate, "VAT Normal");
			return this;
		}

		public DutyCalculatorStrategyTestHelper CreateAllTariffsAndRates()
		{
			CreateTariff1RateOnly();
			Tariff2 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType12A.PK, "1010101012", StartDate, EndDate);
			Tariff3 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType12B.PK, "1010101013", StartDate, EndDate);
			Tariff4 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType13A.PK, "1010101014", StartDate, EndDate);
			Tariff10 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType2P1.PK, "1010101020", StartDate, EndDate);
			Tariff13 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffTypeRebate.PK, "1010101023", StartDate, EndDate);

			Helper.CreateTariffRelationship(Tariff2.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);
			Helper.CreateTariffRelationship(Tariff3.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);
			Helper.CreateTariffRelationship(Tariff4.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);
			Helper.CreateTariffRelationship(Tariff10.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);
			Helper.CreateTariffRelationship(Tariff13.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);

			CreateTaxOrFeesForVAT();

			Tariff2Rate = Helper.CreateRate(Tariff2, RateCode_ZA_EXC_D.PK, StartDate, EndDate, Tariff12ARateFormula, Preference?.PK);
			Tariff4Rate = Helper.CreateRate(Tariff4, RateCode_ZA_LVY_D.PK, StartDate, EndDate, Tariff13ARateFormula, Preference?.PK);
			Tariff10Rate = Helper.CreateRate(Tariff10, RateCode_ZA_ADD_D.PK, StartDate, EndDate, Tariff2P1RateFormula);
			Tariff3Rate = Helper.CreateRate(Tariff3, RateCode_ZA_EX1_D.PK, StartDate, EndDate, Tariff12BRateFormula);
			Tariff13Rate = Helper.CreateRate(Tariff13, RateCode_ZA_REB_D.PK, StartDate, EndDate, TariffRebateRateFormula);
			factory.Save();

			return this;
		}

		public DutyCalculatorStrategyTestHelper CreateTariffsAndRatesFor13DTest()
		{
			Tariff1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType1P1.PK, "1010101011", StartDate, EndDate, taxOrFeeCode: "VAT");
			Tariff3 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType12B.PK, "1010101013", StartDate, EndDate);
			Tariff4 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType13A.PK, "1010101014", StartDate, EndDate);
			Tariff10 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType2P1.PK, "1010101020", StartDate, EndDate);
			Helper.CreateTariffRelationship(Tariff3.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);
			Helper.CreateTariffRelationship(Tariff4.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);
			Helper.CreateTariffRelationship(Tariff10.PK, Tariff1.ZZ1_ZZI_TariffType, Tariff1.ZZ1_TariffCode);
			Tariff4Rate = Helper.CreateRate(Tariff4, RateCode_ZA_DTY_D.PK, StartDate, EndDate, Tariff13ARateFormula);
			Tariff10Rate = Helper.CreateRate(Tariff10, RateCode_ZA_ADD_D.PK, StartDate, EndDate, Tariff2P1RateFormula);
			Tariff3Rate = Helper.CreateRate(Tariff3, RateCode_ZA_EX1_D.PK, StartDate, EndDate, Tariff12BRateFormula);
			return this;
		}

		public void CreateEUTradeGroup(params string[] additionalCountryCodes)
		{
			EUTradeGroup = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "EUTRADE", StartDate, EndDate);
			Helper.AddCountry(EUTradeGroup, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Helper.AddCountry(EUTradeGroup, Core.Constants.CountryCodes.Italy, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			foreach (var code in additionalCountryCodes)
			{
				Helper.AddCountry(EUTradeGroup, code, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			}
			if (Tariff1Rate != null)
			{
				Helper.CreateCusApplicability(Tariff1Rate, EUTradeGroup, new ZDate(1980, 01, 01), new ZDate(2070, 06, 06));
			}
			if (Tariff2Rate != null)
			{
				Helper.CreateCusApplicability(Tariff2Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff3Rate != null)
			{
				Helper.CreateCusApplicability(Tariff3Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff4Rate != null)
			{
				Helper.CreateCusApplicability(Tariff4Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff10Rate != null)
			{
				Helper.CreateCusApplicability(Tariff10Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff13Rate != null)
			{
				Helper.CreateCusApplicability(Tariff13Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			factory.Save();
		}

		public void CreateStandardTradeGroup(params string[] additionalCountryCodes)
		{
			StandardTradeGroup = Helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", StartDate, EndDate);
			Helper.AddCountry(StandardTradeGroup, Core.Constants.CountryCodes.China, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			Helper.AddCountry(StandardTradeGroup, Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			foreach (var code in additionalCountryCodes)
			{
				Helper.AddCountry(StandardTradeGroup, code, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			}
			if (Tariff1Rate != null)
			{
				Helper.CreateCusApplicability(Tariff1Rate, StandardTradeGroup, new ZDate(1980, 01, 01), new ZDate(2070, 06, 06));
			}
			if (Tariff2Rate != null)
			{
				Helper.CreateCusApplicability(Tariff2Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff3Rate != null)
			{
				Helper.CreateCusApplicability(Tariff3Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff4Rate != null)
			{
				Helper.CreateCusApplicability(Tariff4Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff10Rate != null)
			{
				Helper.CreateCusApplicability(Tariff10Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			if (Tariff13Rate != null)
			{
				Helper.CreateCusApplicability(Tariff13Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			}
			factory.Save();
		}

		public JobDeclaration CreateTestJobDeclaration()
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		public CusEntryInstruction AddEntryInstruction(JobDeclaration declaration, string code)
		{
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = code;
			return entryInstruction;
		}

		public JobComInvoiceHeader AddInvoiceHeader(JobDeclaration declaration, decimal invoiceAmountZAR)
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = invoiceAmountZAR;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			return invoice;
		}

		public JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, decimal linePrice, string countryOfOrigin,
			CusEntryInstruction entryInstruction, string previousProcedure = ProcedureCodes._20)
		{
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = linePrice;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + previousProcedure;
			invoiceLine.JI_Tariff = Tariff1.ZZ1_TariffCode.Left(10);
			invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
			invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;
			return invoiceLine;
		}

		public static JobDeclaration SetupForDutyAndTaxCalculationBLNSTests(BusinessObjectFactory factory)
		{
			var helper = new DutyCalculatorStrategyTestHelper(factory)
			{
				StartDate = new ZDateTime(1990, 1, 1),
				EndDate = new ZDateTime(1991, 1, 1),
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
				Tariff12ARateFormula = "(VFD+1P1)*0.1",
				Tariff12BRateFormula = "VFD*0.1",
				Tariff13ARateFormula = "(VFD+1P1+12A+12B)*0.1",
				ExciseRateCode = "D",
				LevyRateCode = "D",
				AdValoremExciseRateCode = "D"
			}.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			var procedure = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", "4", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Swaziland, Core.Constants.CountryCodes.Botswana, Core.Constants.CountryCodes.Lesotho);

			helper.Helper.CreateTaxOrFee("VZR", 0, Core.Constants.CountryCodes.SouthAfrica, helper.StartDate, helper.EndDate, "VAT Zero Rated");
			helper.Helper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.SouthAfrica, helper.StartDate, helper.EndDate, "VAT Exempt");

			var tariffType13B = helper.Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13B");
			var tariff5 = helper.Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType13B.PK, "1010101015", helper.StartDate, helper.EndDate);
			var tariff5Rate = helper.Helper.CreateRate(tariff5, helper.RateCode_ZA_LVY_D.PK, helper.StartDate, helper.EndDate, "(VFD+1P1+12A+12B+13A)*0.1", helper.Preference.PK);
			helper.Helper.CreateTariffRelationship(tariff5.PK, helper.Tariff1.ZZ1_ZZI_TariffType, helper.Tariff1.ZZ1_TariffCode);

			factory.Save();

			var testApplicability5 = helper.Helper.CreateCusApplicability(tariff5Rate, null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			factory.Save();

			var declaration = helper.CreateTestJobDeclaration();
			declaration.JE_RL_NKOrigin = "AUSYD";
			var entryInstruction = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var invoice = helper.AddInvoiceHeader(declaration, 1000m);
			var invoiceLine = helper.AddInvoiceLine(invoice, 1000m, Core.Constants.CountryCodes.Italy, entryInstruction);
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff2.ZZ1_ZZI_TariffTypeCode, helper.Tariff2.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff3.ZZ1_ZZI_TariffTypeCode, helper.Tariff3.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(helper.Tariff4.ZZ1_ZZI_TariffTypeCode, helper.Tariff4.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff5.ZZ1_ZZI_TariffTypeCode, tariff5.ZZ1_TariffCode);

			return declaration;
		}

		public static JobDeclaration CreateSpecifiedMotorVehicleTestData(BusinessObjectFactory factory, bool specifiedMotorVehicle)
		{
			var helper = new DutyCalculatorStrategyTestHelper(factory)
			{
				EndDate = ZDateTime.Today.AddYears(1),
				CreatePreferenceFunc = h => h.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA"),
				Tariff1P1RateFormula = "0.3 * VFD",
				Tariff12BRateFormula = "VFD*0.1",
			}.CreateTariffTypesAndRateCodes().CreateAllTariffsAndRates();
			var procedure1 = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			var procedure2 = helper.Helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "13", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			if (specifiedMotorVehicle)
			{
				helper.Helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.SpecifiedMotorVehicle, "true", helper.Tariff1);
			}
			helper.CreateStandardTradeGroup(Core.Constants.CountryCodes.SouthAfrica);

			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var declaration = helper.CreateTestJobDeclaration();
			declaration.JE_OH_Importer = orgHeader.PK;
			var instruction1 = helper.AddEntryInstruction(declaration, ProcedureCodes._11);
			var instruction2 = helper.AddEntryInstruction(declaration, ProcedureCodes._13);
			var invHeader = helper.AddInvoiceHeader(declaration, 200m);
			var invLine1 = helper.AddInvoiceLine(invHeader, 75m, Core.Constants.CountryCodes.SouthAfrica, instruction1, ProcedureCodes._00);
			invLine1.CusLineTariffDetails.RemoveAndDeleteAll();
			invLine1.CusLineTariffDetails.AddNew(helper.Tariff3.ZZ1_ZZI_TariffTypeCode, helper.Tariff3.ZZ1_TariffCode);
			invLine1.JI_ZZF_NKTaxType = "VAT";

			helper.SetupPermit(declaration.Importer, 100, "12345", PermitTypeList.Codes.VALA, PermitSubTypeList.Codes.MHV);
			helper.SetupPermit(declaration.Importer, 200, "67890", PermitTypeList.Codes.VALA, PermitSubTypeList.Codes.LVE);
			helper.SetupPermit(declaration.Importer, 2.5, "99999", PermitTypeList.Codes.PRC, PermitSubTypeList.Codes.MHV);

			CreateRCCCertificate(instruction1, "12345", 1);
			CreateRCCCertificate(instruction1, "67890", 2);
			CreateDutyRebateCertificate(instruction1, "99999", 3);

			factory.Save();
			return declaration;
		}

		static void CreateRCCCertificate(CusEntryInstruction entryInstruction, ZString code, ZShort order)
		{
			var certificate1 = entryInstruction.RCCCertificates.AddNew();
			certificate1.CY_Code = code;
			certificate1.CY_Order = order;
		}

		static void CreateDutyRebateCertificate(CusEntryInstruction entryInstruction, ZString code, ZShort order)
		{
			var certificate1 = entryInstruction.DutyRebateCertificates.AddNew();
			certificate1.CY_Code = code;
			certificate1.CY_Order = order;
		}

		void SetupPermit(OrgHeader orgHeader, ZDecimal value, ZString number, ZString type, ZString subType)
		{
			var permit1 = factory.New<CusPermitHeader>();
			permit1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			permit1.CPH_Type = type;
			permit1.CPH_SubType = subType;
			permit1.CPH_Number = number;
			permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			permit1.CPH_OH_PermitHolder = orgHeader.PK;
			permit1.CPH_StartDate = ZDate.Today.AddDays(-10);
			permit1.CPH_EndDate = ZDate.Today.AddDays(10);
			var tran1 = permit1.CusPermitLineTransactions.AddNew();
			tran1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			tran1.CPL_TranValue = value;
			tran1.CPL_Reference = "Opening Balance";
		}

		readonly BusinessObjectFactory factory;
	}
}
