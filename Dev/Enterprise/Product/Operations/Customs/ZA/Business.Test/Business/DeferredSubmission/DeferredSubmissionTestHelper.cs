using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public static class DeferredSubmissionTestHelper
	{
		public static JobDeclaration CreateDeferableJobDeclaration(BusinessObjectFactory factory, OrgHeader agent1 = null, OrgHeader agent2 = null)
		{
			var helper = new ZAUniversalReferenceTestDataHelper(factory);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			factory.Save();

			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "00", "", "", ZAJobMessageTypeList.Codes.Import);
			var startDate = ZDateTime.Today.AddYears(0);
			var endDate = ZDateTime.Today.AddYears(1);
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, startDate, endDate, "VAT Normal");
			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99999", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.3 * VFD", preference.PK);
			var testApplicability1 = helper.CreateCusApplicability(tariff1Rate, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));

			helper.CreateCustomsOfficeCusCodeEntry("DFM");
			factory.Save();

			if (agent1 == null)
			{
				agent1 = factory.NewWithValidTestData<OrgHeader>();
			}

			if (agent2 == null)
			{
				agent2 = factory.NewWithValidTestData<OrgHeader>();
			}

			agent1.CompanyData.OB_IsCreditor = true;
			agent1.OH_FullName = "AGENT1";
			agent1.OH_Code = "AG1";
			agent1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			agent2.CompanyData.OB_IsCreditor = true;
			agent2.OH_FullName = "AGENT2";
			agent2.OH_Code = "AG2";
			agent2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "22334455", Core.Constants.CountryCodes.SouthAfrica);
			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "IMPORTER1";
			importer.OH_Code = "IMP000000001";
			importer.CompanyData.OB_IsCreditor = true;
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "99999999", Core.Constants.CountryCodes.SouthAfrica);
			factory.Save();

			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), factory);
			var mapping1 = maps.AddNew();
			mapping1.AccountStartDay = 5;
			mapping1.OrganizationPK = agent1.PK;
			mapping1.CreditorPK = agent1.PK;
			mapping1.CustomsOfficeCode = "DFM";
			mapping1.Cash = false;
			mapping1.ImporterPays = false;
			mapping1.FinancialAccountNumber = "1111111111";
			mapping1.DutyDefermentAmount = 100;
			var mapping2 = maps.AddNew();
			mapping2.AccountStartDay = 5;
			mapping2.OrganizationPK = agent2.PK;
			mapping2.CreditorPK = agent2.PK;
			mapping2.CustomsOfficeCode = "DFM";
			mapping2.Cash = false;
			mapping2.ImporterPays = false;
			mapping2.FinancialAccountNumber = "2222222222";
			mapping2.DutyDefermentAmount = 100;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);

			var declaration = factory.New<JobDeclarationForTesting>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_CustomsOffice = "DFM";
			declaration.JE_DateOfArrival = ZDate.Today.AddDays(2);
			declaration.JE_OH_AgentOverride = agent1.PK;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 75m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
			invoiceLine.JI_Tariff = "99999";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.JI_LinePrice = 75m;
			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			invoiceLine.JI_InvoiceQuantity = 1.0m;
			invoiceLine.JI_InvoiceUQ = "KG";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			return declaration;
		}
	}
}
