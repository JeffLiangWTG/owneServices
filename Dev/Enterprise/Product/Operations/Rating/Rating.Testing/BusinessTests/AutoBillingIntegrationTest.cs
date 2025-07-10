using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using CustomsCommon = Enterprise.Customs.Common;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AUCustomsAutoBillingIntegrationTest : AutoBillingIntegrationTest
	{
		protected override string CountryCode => CountryCodes.Australia;

		protected override void SetUpAutoBillingAndAutoRatingEnvironment()
		{
			CreateRefCusRateCode(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount);

			Env.Registry.Rating.SetBrokerageRatedCodes("BRK,CDS");
			Helper.ChargeCodes["CUSDSB"].AC_DepartmentFilterList = "ALL";
			Helper.ChargeCodes.New("BRKCHG", "Rated Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.CustomsDuty);

			Factory.Save();

			Helper.NewClientRateWithSingleRateLine(NewClient, RatingConstants.RateCategory.DST, RateMode.ALL, "", "AU", "BRKCHG", 100m);
		}

		protected override BaseJobDeclaration CreateDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.LCL;
			declaration.JE_EntryStatus = CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code;
			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			declaration.JE_OH_Supplier = NewClient.PK;
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Factory.Save();

			var job = CreateJob(declaration, declaration.JE_DeclarationReference);
			job.JH_OA_LocalChargesAddr = NewClient.MainAddress.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "AAACW3RYN";
			cusEntryHeader.CH_BGMReference = "S00046074/1";
			cusEntryHeader.CH_TotalPaid = 30m;

			var customsCharge = cusEntryHeader.Charges.AddNew();
			customsCharge.C1_ChargeAmount = 45m;
			customsCharge.C1_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			Factory.Save();

			return declaration;
		}

		protected override BusinessObjectFactory ChangeStatusToEnableAutoBilling(BaseJobDeclaration declaration)
		{
			var cusEntryHeader = Helper.LoadInNewFactory(declaration.CustomsEntryHeaders[0]);
			cusEntryHeader.CH_Status = "PAY";
			cusEntryHeader.CH_EntryStatus = CustomsCommon.AU.CMR.CMRImportEntryAdvice.ATDReceived.Code;

			return cusEntryHeader.Factory;
		}
	}

	//public class USCustomsAutoBillingIntegrationTest : AutoBillingIntegrationTest
	//{
	//	protected override string CountryCode => CountryCodes.UnitedStates;

	//	protected override void SetUpAutoBillingAndAutoRatingEnvironment()
	//	{
	//		CustomsUS.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");
	//		CustomsUS.Business.Testing.DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

	//		Helper.ChargeCodes["CUSDSB"].AC_DepartmentFilterList = "ALL";
	//		Helper.ChargeCodes.New("BRKCHG", "Charge from Rating", FlatCalculator.Code, ChargeCodeGroupList.Codes.CustomsDuty);
	//		Factory.Save();

	//		Helper.NewClientRateWithSingleRateLine(NewClient, RatingConstants.RateCategory.DST, RateMode.ALL, "", "US", "BRKCHG", 100m);
	//	}

	//	protected override BaseJobDeclaration CreateDeclaration()
	//	{
	//		var declaration = Factory.New<BaseJobDeclaration>();
	//		declaration.JE_TransportMode = TransportModes.Sea;
	//		declaration.JE_ContainerMode = ContainerModes.LCL;
	//		declaration.JE_ShipmentIncoTerm = "";
	//		declaration.JE_OH_Importer = Consignee.PK;
	//		declaration.JE_OH_Supplier = Consignor.PK;
	//		declaration.JE_RL_NKOrigin = "SGSIN";
	//		declaration.JE_RL_NKFinalDestination = "USBOS";
	//		declaration.JE_ApplicationCode = CustomsUS.Business.JobApplicationCodeList.Codes.ACE;
	//		declaration["US_EntryType"] = CustomsUS.Business.EntryTypeList.Codes.ConsumptionFreeDutiable;
	//		declaration["US_EnableENS"] = true;
	//		declaration["US_PaymentType"] = CustomsUS.Business.PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
	//		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

	//		var job = CreateJob(declaration, declaration.JE_DeclarationReference);
	//		job.JH_OA_LocalChargesAddr = NewClient.MainAddress.PK;
	//		job.JH_OA_AgentCollectAddr = NewClient2.MainAddress.PK;

	//		CreateInvoice("00001001", job.PK, Consignee);
	//		CreateInvoice("00001002", job.PK, Consignor);
	//		CreateInvoice("00001003", job.PK, NewClient);
	//		CreateInvoice("00001004", job.PK, NewClient2);

	//		var invoice = declaration.Invoices.AddNew();
	//		invoice.JZ_InvoiceAmount = 3000m;
	//		invoice.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;

	//		var invoiceLine = declaration.InvoiceLines.AddNew();
	//		invoiceLine.JI_Tariff = "3201.90.1000";
	//		invoiceLine.JI_CustomsQuantity = 50m;
	//		invoiceLine.JI_LinePrice = 3000m;

	//		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

	//		Factory.Save();

	//		return declaration;
	//	}

	//	void CreateInvoice(ZString invoiceNumber, ZGuid jobPK, OrgHeader orgHeader)
	//	{
	//		var result = Factory.NewWithValidTestData<AccTransactionHeader>();
	//		result.AH_Ledger = LedgerTypes.AccountsReceivable;
	//		result.AH_TransactionType = TransactionTypes.Invoice;
	//		result.AH_OH = orgHeader.PK;
	//		result.AH_TransactionNum = invoiceNumber;
	//		result.AH_RX_NKTransactionCurrency = CurrencyCodes.UnitedStates;
	//		result.AH_GB = GlbBranch.CurrentBranch.PK;
	//		result.AH_ConsolidatedInvoiceRef = "B11092008";
	//		result.AH_TransactionReference = invoiceNumber;
	//		result.AH_JH = jobPK;
	//	}

	//	protected override BusinessObjectFactory ChangeStatusToEnableAutoBilling(BaseJobDeclaration declaration)
	//	{
	//		var cusEntryHeaderToReload = declaration.InvoiceLines[0].CusEntryLine.Header;
	//		var cusEntryHeader = Helper.LoadInNewFactory(cusEntryHeaderToReload);

	//		cusEntryHeader.CH_MessageType = CustomsUS.Business.CusEntryHeaderMessageTypeList.Codes.EntrySummary;
	//		cusEntryHeader.CH_Status = CustomsCommon.US.ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

	//		return cusEntryHeader.Factory;
	//	}
	//}

	public abstract class AutoBillingIntegrationTest : BaseRatingIntegrationTest
	{
		[TestDate(2018, 02, 10)]
		public void TestAddDutyCustomsCharge_ByAutoBilling_ThenByAutoRating()
		{
			var declaration = CreateDeclaration();
			var autoBillingFactory = ChangeStatusToEnableAutoBilling(declaration);      //1. SETUP Customs AutoBilling (to be triggered by saving this factory)
			autoBillingFactory.Save();                                                  //2. SAVE AutoBilling Factory to CREATE Customs CUSDSB Job Charge
			var autoRatingFactory = AutoRateDeclaration(declaration);                   //3. AUTORATE Revenue including Customs CUSDSB Charge in a new Factory
			autoRatingFactory.Save();                                                   //4. SAVE AutoRated Job Charges

			AssertAutoRatingAndAutoBillingResults(declaration);
		}

		[TestDate(2018, 02, 10)]
		public void TestAddDutyCustomsCharge_ByAutoBilling_WhileAutoRating()
		{
			var declaration = CreateDeclaration();
			var autoBillingFactory = ChangeStatusToEnableAutoBilling(declaration);      //1. SETUP Customs AutoBilling (to be triggered by saving this factory)
			var autoRatingFactory = AutoRateDeclaration(declaration);                   //2. AUTORATE Revenue including Customs CUSDSB Charge in a new Factory
			autoRatingFactory.Save();                                                   //3. SAVE AutoRated Job Charges
			autoBillingFactory.Save();                                                  //4. SAVE AutoBilling Factory to CREATE Customs CUSDSB Job Charge

			AssertAutoRatingAndAutoBillingResults(declaration);
		}

		//[TestDate(2018, 02, 10)]
		//public void TestAddDutyCustomsCharge_ByAutoBilling_WhileAutoRating2()
		//{
		//	var declaration = CreateDeclaration();
		//	var autoBillingFactory = ChangeStatusToEnableAutoBilling(declaration);      //1. SETUP Customs AutoBilling (to be triggered by saving this factory)
		//	var autoRatingFactory = AutoRateDeclaration(declaration);                   //2. AUTORATE Revenue including Customs CUSDSB Charge in a new Factory
		//	autoBillingFactory.Save();                                                  //3. SAVE AutoRated Job Charges
		//	autoRatingFactory.Save();                                                   //4. SAVE AutoBilling Factory to CREATE Customs CUSDSB Job Charge

		//	AssertAutoRatingAndAutoBillingResults(declaration);
		//}

		[TestDate(2017, 02, 10)]
		public void TestAddDutyCustomsCharge_ByAutoRating_ThenByAutoBilling()
		{
			var declaration = CreateDeclaration();
			var autoRatingFactory = AutoRateDeclaration(declaration);                   //1. AUTORATE Revenue including Customs CUSDSB Charge in a new Factory
			autoRatingFactory.Save();                                                   //2. SAVE AutoRated Job Charges
			var autoBillingFactory = ChangeStatusToEnableAutoBilling(declaration);      //3. SETUP Customs AutoBilling (to be triggered by saving this factory)
			autoBillingFactory.Save();                                                  //4. SAVE AutoBilling Factory to CREATE Customs CUSDSB Job Charge

			AssertAutoRatingAndAutoBillingResults(declaration);
		}

		[TestDate(2020, 11, 18)]
		public void TestCustomsJobs_WhenUnitIsNotValid_ShouldHaveAutorateException()
		{
			Helper.ChargeCodes.New("SSACHG", "Rated Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.CustomsDuty);

			var client = Helper.NewOrgHeader();
			client.OH_Code = "NTC3";
			client.OH_FullName = "New Test Client";
			client.OH_RL_NKClosestPort = "AUSYD";

			client.OH_IsDebtor = true;
			client.OH_IsConsignor = true;
			client.OH_IsConsignee = true;

			var address = client.MainAddress;
			address.OA_Address1 = "123 Fake Street";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "2000";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = CustomsCommon.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.LCL;
			declaration.JE_EntryStatus = CustomsCommon.AU.CustomsEntryStatus.DeclarationWorkComplete.Code;
			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			declaration.JE_OH_Supplier = client.PK;
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_TotalWeight = 20;
			declaration.JE_TotalWeightUnit = "0.";

			Factory.Save();

			var job = CreateJob(declaration, declaration.JE_DeclarationReference);
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "AAACW3RYN";
			cusEntryHeader.CH_BGMReference = "S00046074/1";
			cusEntryHeader.CH_TotalPaid = 30m;

			var customsCharge = cusEntryHeader.Charges.AddNew();
			customsCharge.C1_ChargeAmount = 45m;
			customsCharge.C1_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			Factory.Save();

			var clientRate = Helper.NewClientRate(client);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "SGSIN", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("SSACHG", UnitCalculator.Code, Weight.Kilograms, CurrencyCodes.Australia);
			var calc = rateLine.GetCalculator<UnitCalculator>();
			calc.PerUnit = 25;

			var rateLine1 = rateEntry.AddRateLine("BRKCHG", FlatCalculator.Code, "", CurrencyCodes.Australia);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 100;

			Factory.Save();

			var message1 = "Expected both a BRKCHG Charge from the Client Rate and the CUSDSB Charge from Customs Entry";
			AutorateAndAssert(message1, Array.Empty<AssertionCharge>(), declaration, client, autorateCosts: false, expectedErrors: new[] { @"Error Autorating has encountered an error:
Cannot convert 0. to KG, The conversion logic for unit 0. is not implemented" });
		}

		//[TestDate(2018, 02, 10)]
		//public void TestAddDutyCustomsCharge_ByAutoRating_WhileAutoBilling()
		//{
		//	var declaration = CreateDeclaration();
		//	var autoRatingFactory = AutoRateDeclaration(declaration);                   //1. AUTORATE Revenue including Customs CUSDSB Charge in a new Factory
		//	var autoBillingFactory = ChangeStatusToEnableAutoBilling(declaration);      //2. SETUP Customs AutoBilling (to be triggered by saving this factory)
		//	autoBillingFactory.Save();                                                  //3. SAVE AutoRated Job Charges
		//	autoRatingFactory.Save();                                                   //4. SAVE AutoBilling Factory to CREATE Customs CUSDSB Job Charge

		//	AssertAutoRatingAndAutoBillingResults(declaration);
		//}

		#region Implementation

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCode);
			var integrationOption = new AccountingIntegrationOptions { EnableAccountingIntegration = true };
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, integrationOption);
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Helper.CreateCreditor().PK.ToGuid());

			SetUpAutoBillingAndAutoRatingEnvironment();
		}

		protected abstract string CountryCode { get; }
		protected abstract void SetUpAutoBillingAndAutoRatingEnvironment();
		protected abstract BaseJobDeclaration CreateDeclaration();
		protected abstract BusinessObjectFactory ChangeStatusToEnableAutoBilling(BaseJobDeclaration declaration);

		protected BusinessObjectFactory AutoRateDeclaration(BaseJobDeclaration declarationPK)
		{
			var declaration = Helper.LoadInNewFactory(declarationPK);

			var message = "Expected both a BRKCHG Charge from the Client Rate and the CUSDSB Charge from Customs Entry";
			AutorateAndAssert(message, ExpectedCharges, declaration, NewClient, autorateCosts: false);

			return declaration.Factory;
		}

		protected void AssertAutoRatingAndAutoBillingResults(BaseJobDeclaration declaration)
		{
			AssertCharges(ExpectedCharges, (Job)declaration.Job);

			var reloadedJob = Helper.LoadInNewFactory(declaration.Job);
			AssertCharges("When reload the job, we expect no duplicates", ExpectedCharges, (Job)reloadedJob);
		}

		AssertionCharge[] expectedCustomsAndRatingResults;
		AssertionCharge[] ExpectedCharges => expectedCustomsAndRatingResults ?? (expectedCustomsAndRatingResults = new[]
		{
			new AssertionCharge
			{
				ChargeCode = "CUSDSB",
				JR_OSSellAmt = 45m,
				JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency,
				CostCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.",
				RevenueCalculationDescription = "This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message."
			},
			new AssertionCharge
			{
				ChargeCode = "BRKCHG",
				JR_OSSellAmt = 100m
			}
		});

		#endregion
	}
}
