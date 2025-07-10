using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Organisation.Registry;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using DefaultsOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;
using MetaData = CargoWise.ComponentModel.MetaData;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCompanyData))]
	public class OrgCompanyDataTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetDefaultEPaymentReason()
		{
			var org = Factory.New<OrgHeader>();
			var accountDetails = org.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var usdAccountDetail = accountDetails.AddNew();
			usdAccountDetail.A1_IsDefaultAccount = true;
			usdAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			usdAccountDetail.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			var eurAccountDetail = accountDetails.AddNew();
			eurAccountDetail.A1_IsDefaultAccount = true;
			eurAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			eurAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			eurAccountDetail.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.BusinessConsultancyAndPRSevices;

			AssertEquals(ZString.Empty, org.CompanyData.GetDefaultEPaymentReason(ZString.Empty, ZString.Empty));
			AssertEquals(ZString.Empty, org.CompanyData.GetDefaultEPaymentReason(EPaymentProviderCodes.Codes.OFX, Core.Constants.CurrencyCodes.Australia));
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, org.CompanyData.GetDefaultEPaymentReason(EPaymentProviderCodes.Codes.OFX, Core.Constants.CurrencyCodes.UnitedStates));
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.BusinessConsultancyAndPRSevices, org.CompanyData.GetDefaultEPaymentReason(EPaymentProviderCodes.Codes.OFX, Core.Constants.CurrencyCodes.EuropeanUnion));
			AssertExceptionThrown(typeof(NotImplementedException), "E-Payment Provider [AAA] is not mapped to its Payment Method. Please add the mapping here.", () => org.CompanyData.GetDefaultEPaymentReason("AAA", Core.Constants.CurrencyCodes.Australia));
		}

		public void TestOrgTaxConfigurationIsRegisteredAsEditableChild()
		{
			AssertEquals("AROrgTaxConfigurations", true, Data.IsRegisteredEditableChildObject(Data.AROrgTaxConfigurations));
			AssertEquals("APOrgTaxConfigurations", true, Data.IsRegisteredEditableChildObject(Data.APOrgTaxConfigurations));
		}

		public void TestIsAPPCDSetting()
		{
			Assert(Data.OB_APCreateVATComplianceDocumentOnPosting.Equals("NON"));
			Assert(!Data.IsAPPCDSetting);

			Data.OB_APCreateVATComplianceDocumentOnPosting = "PCD";
			Assert(Data.IsAPPCDSetting);
		}

		public void TestOB_APPrintContractorForm()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			Assert("default value is false", !companyData.OB_APPrintContractorForm);
			Assert(!companyData.IsIncludedInTparReport);
			AssertEquals("No logs about at the start", 0, companyData.Logs.Find(new ZQuery(StmALogSchema.SL_Table, OrgCompanyDataSchema.Constants.TableName)).Length);

			companyData.OB_APPrintContractorForm = true;
			Factory.Save();

			Assert(companyData.OB_APPrintContractorForm);
			Assert(companyData.IsIncludedInTparReport);
			AssertEquals("Still No logs", 0, companyData.Logs.Find(new ZQuery(StmALogSchema.SL_Table, OrgCompanyDataSchema.Constants.TableName)).Length);
		}

		public void TestIsARRCCSetting()
		{
			Assert(Data.OB_ARCreateVATComplianceDocumentOnPosting.Equals("NON"));
			Assert(!Data.IsARRCCSetting);

			Data.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";
			Assert(Data.IsARRCCSetting);
		}

		public void TestIsAPRCCSetting()
		{
			Assert(Data.OB_APCreateVATComplianceDocumentOnPosting.Equals("NON"));
			Assert(!Data.IsAPRCCSetting);

			Data.OB_APCreateVATComplianceDocumentOnPosting = "RCC";
			Assert(Data.IsAPRCCSetting);
		}

		public void TestImporterOverride()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,CLASS,COO,PREF";

			AssertEquals(true, companyData.ImporterOverride);

			companyData.ImporterOverride = false;
			AssertEquals("The collection should be removed when ImporterOverride is set to false", 0, companyData.DefaultOptions.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedData = newFactory.Load<OrgCompanyData>(companyData.PK);
			AssertEquals("", reloadedData.OB_IMProductValueDefaultOptions);
		}

		public void TestOverCreditLimitWhenCreditLimitIsUnlimited()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			var orgData = org.CompanyData;
			AssertEquals(0m, orgData.OB_ARCreditLimit);
			Assert(orgData.OB_ARCreditApproved);
			Assert(!org.MiscServ.OM_ARGlobalCreditApproved);
			Assert(!orgData.OverCreditLimit);

			var invoice = GetNewTransactionHeader(org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 99, GlbBranch.CurrentBranch);
			invoice.AH_InvoiceAmount = 99M;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadOrgData = newFactory.Load<OrgHeader>(org.PK).CompanyData;

			Assert("zero credit limit is considered as unlimited so it should not be over credit limit", !reloadOrgData.OverCreditLimit);
		}

		public void TestDefaultOptions()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,CLASS,COO";

			AssertEquals(3, companyData.DefaultOptions.Count);
			Assert(companyData.DefaultOptions.Cast<IMProductValueDefaultOption>().Any(x => x.FieldType == DefaultsOptions.Codes.Classification));
			Assert(companyData.DefaultOptions.Cast<IMProductValueDefaultOption>().Any(x => x.FieldType == DefaultsOptions.Codes.CountryOfOrigin));
			Assert(!companyData.DefaultOptions.Cast<IMProductValueDefaultOption>().Any(x => x.FieldType == DefaultsOptions.Codes.Preference));
			Assert(companyData.DefaultOptions.Cast<IMProductValueDefaultOption>().Any(x => x.FieldType == DefaultsOptions.Codes.Tariff));

			var option = new IMProductValueDefaultOption(DefaultsOptions.Codes.Preference, Factory);
			option.HasChanges = true;
			companyData.DefaultOptions.Add(option);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedData = newFactory.Load<OrgCompanyData>(companyData.PK);
			AssertEquals("OVR,TAR,CLASS,COO,PREFF", reloadedData.OB_IMProductValueDefaultOptions);
		}

		public void TestOrgCompanyDataShouldContainCorrectExRateConfig()
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var initilOrgConfigs = Factory.Load<AccExchangeRateConfiguration>(new ZQuery());
			AssertHasNonDefaultConfigs(0, initilOrgConfigs);

			var apCreditorConfig = Factory.New<AccExchangeRateConfiguration>();
			apCreditorConfig.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			apCreditorConfig.JCE_ParentID = newCompany.OrgProxy.PK;
			apCreditorConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(AccExRateConfigurationLevelEnum.Creditor, apCreditorConfig.Level);

			var arDebtorConfig = Factory.New<AccExchangeRateConfiguration>();
			arDebtorConfig.JCE_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			arDebtorConfig.JCE_ParentID = newCompany.OrgProxy.PK;
			arDebtorConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(AccExRateConfigurationLevelEnum.Debtor, arDebtorConfig.Level);

			var orgCreditorGroup = Factory.LoadTop1<OrgCreditorGroup>(new ZQuery());
			var creditorGroupConfig = Factory.New<AccExchangeRateConfiguration>();
			creditorGroupConfig.JCE_ParentTableCode = OrgCreditorGroupSchema.Constants.Prefix;
			creditorGroupConfig.JCE_ParentID = orgCreditorGroup.PK;
			creditorGroupConfig.JCE_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(AccExRateConfigurationLevelEnum.CreditorGroup, creditorGroupConfig.Level);
			newCompany.OrgProxy.CompanyData.OB_OG_APCreditorGroup = orgCreditorGroup.PK;

			var orgDebtorGroup = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery());
			var debtorGroupConfig = Factory.New<AccExchangeRateConfiguration>();
			debtorGroupConfig.JCE_ParentTableCode = OrgDebtorGroupSchema.Constants.Prefix;
			debtorGroupConfig.JCE_ParentID = orgDebtorGroup.PK;
			debtorGroupConfig.JCE_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(AccExRateConfigurationLevelEnum.DebtorGroup, debtorGroupConfig.Level);
			newCompany.OrgProxy.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup.PK;

			Factory.Save();

			var apCreditorConfigs = newCompany.OrgProxy.CompanyData.AccAPExchangeRateConfigurations;
			apCreditorConfigs.Load();
			AssertHasNonDefaultConfigs(2, apCreditorConfigs.Cast<AccExchangeRateConfiguration>());
			Assert(apCreditorConfigs.Contains(apCreditorConfig));
			Assert(apCreditorConfigs.Contains(creditorGroupConfig));
			Assert(!apCreditorConfigs.Contains(arDebtorConfig));
			Assert(!apCreditorConfigs.Contains(debtorGroupConfig));

			var arDebtorConfigs = newCompany.OrgProxy.CompanyData.AccARExchangeRateConfigurations;
			arDebtorConfigs.Load();
			AssertHasNonDefaultConfigs(2, arDebtorConfigs.Cast<AccExchangeRateConfiguration>());
			Assert(arDebtorConfigs.Contains(arDebtorConfig));
			Assert(arDebtorConfigs.Contains(debtorGroupConfig));
			Assert(!arDebtorConfigs.Contains(apCreditorConfig));
			Assert(!arDebtorConfigs.Contains(creditorGroupConfig));

			void AssertHasNonDefaultConfigs(int expectedCount, IEnumerable<AccExchangeRateConfiguration> collection)
			{
				var defaultConfigs = collection.Any(x => x.JCE_JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated) ? 2 : 1;
				AssertEquals(expectedCount, collection.Count() - defaultConfigs);
			}
		}

		public void TestAccountFeeSettings()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CompanyData.AccountFeeSettings.OverrideSettings = true;
			orgHeader.CompanyData.AccountFeeSettings.AAF_FeeAmount = -25m;
			orgHeader.RunPreSaveValidation();
			AssertHasErrorContaining(orgHeader.CompanyData.AccountFeeSettings.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesOrgCompanyData()
		{
			var org = Factory.New<OrgHeader>();
			var data = org.CompanyData;

			var localList = new List<string>
			{
				nameof(data.CreditOutStandingBalance),
				nameof(data.ARTemporaryCreditLimit),
				nameof(data.OB_ARCreditLimit)
			};

			var tester = new DecimalPlacesAttributeTester(data);
			tester.CheckLocalCurrency(localList, nameof(data.LocalDecimals));
		}

		public void TestARSettlementGroupPK()
		{
			var glbBranchAU = new BusinessObjectFactory().NewWithValidTestData<GlbBranch>();
			glbBranchAU.GB_Code = MasterFilesTestHelper.GetRandomString(glbBranchAU.GB_CodeInfo.MaxLength);
			glbBranchAU.SetCountry(Core.Constants.CountryCodes.Australia);
			var glbCompanyAU = glbBranchAU.Company;
			glbCompanyAU.GC_Code = MasterFilesTestHelper.GetRandomString(glbCompanyAU.GC_CodeInfo.MaxLength);
			glbCompanyAU.SetCountry(Core.Constants.CountryCodes.Australia);
			glbBranchAU.Factory.Save();

			var glbBranchJP = new BusinessObjectFactory().NewWithValidTestData<GlbBranch>();
			glbBranchJP.GB_Code = MasterFilesTestHelper.GetRandomString(glbBranchJP.GB_CodeInfo.MaxLength);
			glbBranchJP.SetCountry(Core.Constants.CountryCodes.Japan);
			var glbCompanyJP = glbBranchJP.Company;
			glbCompanyJP.GC_Code = MasterFilesTestHelper.GetRandomString(glbCompanyJP.GC_CodeInfo.MaxLength);
			glbCompanyJP.SetCountry(Core.Constants.CountryCodes.Japan);
			glbBranchJP.Factory.Save();

			var glbBranchFI = new BusinessObjectFactory().NewWithValidTestData<GlbBranch>();
			glbBranchFI.GB_Code = MasterFilesTestHelper.GetRandomString(glbBranchFI.GB_CodeInfo.MaxLength);
			glbBranchFI.SetCountry(Core.Constants.CountryCodes.Finland);
			var glbCompanyFI = glbBranchFI.Company;
			glbCompanyFI.GC_Code = MasterFilesTestHelper.GetRandomString(glbCompanyFI.GC_CodeInfo.MaxLength);
			glbCompanyFI.SetCountry(Core.Constants.CountryCodes.Finland);
			glbBranchFI.Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_ARCreditLimit = 400m;
			org.MiscServ.OM_ARGlobalCreditLimit = 500m;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			Factory.Save();
			var orgData = org.CompanyData;
			var settlementGroupAU = Factory.NewWithValidTestData<OrgHeader>();
			var settlementGroupJP = Factory.NewWithValidTestData<OrgHeader>();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranchAU.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				orgData = org.CompanyData;
				orgData.OB_IsDebtor = true;
				settlementGroupAU.CompanyData.OB_ARCreditLimit = 11M;
				orgData.ARSettlementGroupPK = settlementGroupAU.PK;
				orgData.OB_ARUseSettlementGroupCreditLimit = true;

				Factory.Save();
				AssertEquals(settlementGroupAU.PK, orgData.ARSettlementGroupPK);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranchJP.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				orgData = org.CompanyData;
				orgData.OB_IsDebtor = true;
				settlementGroupJP.CompanyData.OB_ARCreditLimit = 22M;
				orgData.ARSettlementGroupPK = settlementGroupJP.PK;
				orgData.OB_ARUseSettlementGroupCreditLimit = true;

				Factory.Save();
				AssertEquals(settlementGroupJP.PK, orgData.ARSettlementGroupPK);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranchFI.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				orgData = org.CompanyData;
				orgData.OB_IsDebtor = true;
				settlementGroupJP.CompanyData.OB_ARCreditLimit = 33M;
				orgData.OB_ARUseSettlementGroupCreditLimit = true;

				Factory.Save();
				AssertEquals(ZGuid.Empty, orgData.ARSettlementGroupPK);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbBranchAU.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				org.FillGlobalCreditGroupChilds();
				var childCompanyAU = org.GlobalCreditGroupChilds.First(x => ((OrgCompanyData)x).OB_GC == glbCompanyAU.PK) as OrgCompanyData;
				var childCompanyJP = org.GlobalCreditGroupChilds.First(x => ((OrgCompanyData)x).OB_GC == glbCompanyJP.PK) as OrgCompanyData;
				var childCompanyFI = org.GlobalCreditGroupChilds.First(x => ((OrgCompanyData)x).OB_GC == glbCompanyFI.PK) as OrgCompanyData;
				AssertEquals(settlementGroupAU.PK, childCompanyAU.ARSettlementGroupPK);
				AssertEquals(settlementGroupJP.PK, childCompanyJP.ARSettlementGroupPK);
				AssertEquals(ZGuid.Empty, childCompanyFI.ARSettlementGroupPK);
			}
		}

		public void TestGlobalCreditLimitIsValidatedWhenLocalCreditLimitisUpdated()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = true;
			org.MiscServ.OM_ARGlobalCreditLimit = 500m;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			Factory.Save();

			org.MiscServ.RunPreSaveValidation();
			Assert(!org.MiscServ.ShouldValidateOnSave);
			org.CompanyData.OB_ARCreditLimit = 400m;
			Assert(org.MiscServ.ShouldValidateOnSave);
		}

		public void TestGlobalCreditLimitIsValidatedWhenTemporaryLimitisUpdated()
		{
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(50, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(100, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(100, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_ARCreditLimit = 400m;
			org.MiscServ.OM_ARGlobalCreditLimit = 500m;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			Factory.Save();

			org.MiscServ.RunPreSaveValidation();
			Assert(!org.MiscServ.ShouldValidateOnSave);
			org.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.UtcNow.AddMinutes(1);
			org.CompanyData.OB_ARTemporaryCreditLimitIncrease = 200m;
			Assert(org.MiscServ.ShouldValidateOnSave);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadMethod()
		{
			var companyData = OrgCompanyData.Load(Factory, Data.OB_OH, Data.OB_GC);
			AssertEquals("Loaded OrgCompanyData should be the same as the Org.CompanyData", Data.PK, companyData.PK);

			companyData = OrgCompanyData.Load(Factory, ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Load method should simply return null if either ZGuids are invalid or empty", null, companyData);

			companyData = OrgCompanyData.Load(Factory, ZGuid.NewZGuid(), ZGuid.NewZGuid());
			AssertEquals("Load method should simply return null if a companydata does not exist", null, companyData);
			OrgCompanyData.Load(null, ZGuid.Empty, ZGuid.Empty);
		}

		public void TestOB_ARCreditAgreedPaymentMethod_List()
		{
			AssertEquals(OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList(), Org.CompanyData.OB_ARCreditAgreedPaymentMethod_List);
		}

		public void TestOB_APCreditAgreedPaymentMethod_List()
		{
			AssertEquals(Env.Registry.PayablesCreditAgreedPaymentMethodsList, Org.CompanyData.OB_APCreditAgreedPaymentMethod_List);
		}

		#region Invoice Types

		public void TestInvoiceTypes()
		{
			AssertNotNull(Org.CompanyData.InvoiceTypes);
			AssertEquals("Collection Empty", 0, Org.CompanyData.InvoiceTypes.Count);
			Org.CompanyData.InvoiceTypes.AddNew();
			AssertEquals("Collection has 1", 1, Org.CompanyData.InvoiceTypes.Count);
			AssertEquals("Dependent Parent Set", Org.CompanyData.PK, Org.CompanyData.InvoiceTypes[0].PI_OB);
			Factory.Save();

			OrgHeader header2 = Factory.Load<OrgHeader>(Org.PK);
			AssertEquals("Collection Loaded", 1, header2.CompanyData.InvoiceTypes.Count);
		}

		public void TestGetApplicableInvoiceTypes()
		{
			//Setup Invoicetype Config
			AddInvoiceTypeToCompanyData(Org.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "SHP", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "SHP", "SEA", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "SHP", "AIR", "EXP", "STD", "INV", "INV");
			Factory.Save();

			OrgHeader header2 = Factory.Load<OrgHeader>(Org.PK);
			AssertEquals("Collection Loaded", 4, header2.CompanyData.InvoiceTypes.Count);

			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "DOM", "STD", true, "SHP"), "SHP", "SHP", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("SEA", "IMP", "STD", true, "SHP"), "SHP", "SHP", "SEA", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "EXP", "STD", true, "SHP"), "SHP", "SHP", "AIR", "EXP", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "EXP", "STD", true, "CSH"), "CSH", "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");

			//Add few more
			AddInvoiceTypeToCompanyData(Org.CompanyData, "CSH", "SEA", "DOM", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "CSH", "AIR", "EXP", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "MSC", "ALL", "ALL", ZString.Empty, "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "BRK", "AIR", "ALL", "STD", "INV", "INV");
			Factory.Save();

			header2 = Factory.Load<OrgHeader>(Org.PK);
			AssertEquals("Collection Loaded", 8, header2.CompanyData.InvoiceTypes.Count);

			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "DOM", "STD", false, "CSH"), "CSH", "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "EXP", "STD", false, "CSH"), "CSH", "CSH", "AIR", "EXP", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", ZString.Empty, false, "MSC"), "MSC", "MSC", "ALL", "ALL", ZString.Empty, "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "ALL", "STD", false, "BRK"), "BRK", "BRK", "AIR", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("SEA", "DOM", "STD", false, "CSH"), "CSH", "CSH", "SEA", "DOM", "STD", "INV", "INV");
		}

		public void TestGetApplicableInvoiceTypesDoNotPickUpSpecificConfigurationForAll()
		{
			//Setup InvoiceType Configuration
			AddInvoiceTypeToCompanyData(Org.CompanyData, "CSH", "SEA", "DOM", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "CSH", "AIR", "IMP", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "BRK", "AIR", "IMP", "STD", "INV", "INV");
			Factory.Save();

			//Test number of configurations found
			OrgHeader header2 = Factory.Load<OrgHeader>(Org.PK);
			AssertEquals("Collection Loaded", 3, header2.CompanyData.InvoiceTypes.Count);

			//Billing Configuration for ALL should not retrieve specific Invoice Types
			AssertInvoiceTypeDoesNotExist(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "IMP", "STD", false, "CSH"), "CSH");
			AssertInvoiceTypeDoesNotExist(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "ALL", "STD", false, "CSH"), "CSH");
			AssertInvoiceTypeDoesNotExist(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", false, "CSH"), "CSH");

			//Only specific cases SHOULD pick up specific Invoice Types
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("AIR", "IMP", "STD", false, "CSH"), "CSH", "CSH", "AIR", "IMP", "STD", "INV", "INV");
		}

		public void TestGetApplicableInvoiceTypesDoNotReturnDeletedOrgInvoiceType()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceType1 = AddInvoiceTypeToCompanyData(organisation.CompanyData, "SHP", "SEA", "IMP", "STD", "INV", "INV");

			var cachedInvoiceTypes = organisation.CompanyData.GetApplicableInvoiceTypes("SEA", "IMP", "STD", true, new ZString[] { "SHP" });
			AssertEquals(1, cachedInvoiceTypes.Count);
			AssertEquals(invoiceType1.PI_Module, cachedInvoiceTypes.Values.First().PI_Module);

			var invoiceType2 = AddInvoiceTypeToCompanyData(organisation.CompanyData, "ALL", "SEA", "ALL", ZString.Empty, "INV", "INV");
			invoiceType1.Delete();
			Assert(invoiceType1.IsDeleted);

			cachedInvoiceTypes = organisation.CompanyData.GetApplicableInvoiceTypes("SEA", "IMP", "STD", true, new ZString[] { "SHP" });
			AssertEquals(1, cachedInvoiceTypes.Count);
			ErrorReporter.Clear();
			var resultModule = cachedInvoiceTypes.Values.First().PI_Module;
			AssertNull(ErrorReporter.LastExceptionReported);
			AssertEquals(invoiceType2.PI_Module, resultModule);

			invoiceType2.Delete();
			Assert(invoiceType2.IsDeleted);

			cachedInvoiceTypes = organisation.CompanyData.GetApplicableInvoiceTypes("SEA", "IMP", "STD", true, new ZString[] { "SHP" });
			AssertEquals(0, cachedInvoiceTypes.Count);
		}

		public void TestGetApplicableInvoiceTypesWithServiceLevel()
		{
			AddInvoiceTypeToCompanyData(Org.CompanyData, "AGB", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "AGS", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "SHP", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "CSH", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "BRK", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "LTC", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "QSH", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "TRN", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "WIN", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "WOU", "ALL", "ALL", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "WKI", "ALL", "ALL", ZString.Empty, "INV", "INV");
			Factory.Save();

			var header = Factory.Load<OrgHeader>(Org.PK);
			AssertEquals("Collection Loaded", 11, header.CompanyData.InvoiceTypes.Count);

			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "AGB"), "AGB", "AGB", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "AGS"), "AGS", "AGS", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "SHP"), "SHP", "SHP", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "CSH"), "CSH", "CSH", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "BRK"), "BRK", "BRK", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "LTC"), "LTC", "LTC", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "QSH"), "QSH", "QSH", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "TRN"), "TRN", "TRN", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "WIN"), "WIN", "WIN", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "STD", true, "WOU"), "WOU", "WOU", "ALL", "ALL", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", ZString.Empty, true, "WKI"), "WKI", "WKI", "ALL", "ALL", ZString.Empty, "INV", "INV");
		}

		public void TestGetApplicableInvoiceTypesWithoutCache()
		{
			AddInvoiceTypeToCompanyData(Org.CompanyData, "ACD", "SEA", "IMP", "STD", "INV", "INV");
			AddInvoiceTypeToCompanyData(Org.CompanyData, "ACD", "SEA", "IMP", "XXX", "CHG", "CHG");
			Factory.Save();

			var header = Factory.Load<OrgHeader>(Org.PK);
			AssertEquals("Collection Loaded", 2, header.CompanyData.InvoiceTypes.Count);

			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("SEA", "IMP", "STD", true, "ACD"), "ACD", "ACD", "SEA", "IMP", "STD", "INV", "INV");
			AssertInvoiceType(Org.CompanyData.GetApplicableInvoiceTypes("SEA", "IMP", "XXX", true, "ACD"), "ACD", "ACD", "SEA", "IMP", "XXX", "CHG", "CHG");
		}

		void AssertInvoiceType(Dictionary<ZString, OrgInvoiceType> invoiceTypes, ZString jobType, ZString expectedJobType, ZString expectedTransportMode, ZString expectedServiceDirection, ZString expectedServiceLevel, ZString expectedPI_Type, ZString expectedPI_SecondaryType)
		{
			AssertNotNull("Invoice Types", invoiceTypes);
			AssertEquals("Invoice Types", true, invoiceTypes.ContainsKey(jobType));
			AssertEquals("Invoice Types: PI_Module", expectedJobType, invoiceTypes[jobType].PI_Module);
			AssertEquals("Invoice Types: PI_ServiceDirection", expectedServiceDirection, invoiceTypes[jobType].PI_ServiceDirection);
			AssertEquals("Invoice Types: PI_TransportMode", expectedTransportMode, invoiceTypes[jobType].PI_TransportMode);
			AssertEquals("Invoice Types: PI_RS_NKServiceLevel", expectedServiceLevel, invoiceTypes[jobType].PI_RS_NKServiceLevel);
			AssertEquals("Invoice Types: PI_Type", expectedPI_Type, invoiceTypes[jobType].PI_Type);
			AssertEquals("Invoice Types: PI_SecondaryType", expectedPI_SecondaryType, invoiceTypes[jobType].PI_SecondaryType);
		}

		void AssertInvoiceTypeDoesNotExist(Dictionary<ZString, OrgInvoiceType> invoiceTypes, ZString jobType)
		{
			AssertNotNull("Invoice Types", invoiceTypes);
			AssertEquals("Invoice Types", false, invoiceTypes.ContainsKey(jobType));
		}

		#endregion

		#region AP Account Details

		public void TestAccountDetailsCollection()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			AccAPAccountDetails accDetails = Factory.New<AccAPAccountDetails>();
			accDetails.A1_OB = org.CompanyData.PK;

			AssertEquals("Should be one Account in the collection", 1, org.CompanyData.AccountDetailsCollection.Count);
			AccountDetailsDependentCollection collection = org.CompanyData.AccountDetailsCollection;
			AssertSame("Collection was not lazy loaded", collection, org.CompanyData.AccountDetailsCollection);
		}

		#endregion

		#region AR Account Details

		public void TestARAccountDetailsCollection()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			var accDetails = Factory.New<AccARAccountDetails>();
			accDetails.A1_OB = org.CompanyData.PK;

			AssertEquals("Should be one Account in the collection", 1, org.CompanyData.ARAccountDetailsCollection.Count);
			var collection = org.CompanyData.ARAccountDetailsCollection;
			AssertSame("Collection was not lazy loaded", collection, org.CompanyData.ARAccountDetailsCollection);
		}

		public void TestARAccountDetailsCollectionIsReadOnly()
		{
			bool oldAPValue = Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.ARAccountDetailsCollection.ReadOnly);

				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.CompanyData.ARAccountDetailsCollection.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = oldAPValue;
			}
		}

		#endregion

		public void TestAROrgTaxConfigurationTemplateIsReadOnly()
		{
			var isModificationAllowed = Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.OB_OCT_ARTaxTemplateInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.CompanyData.OB_OCT_ARTaxTemplateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed = isModificationAllowed;
			}
		}

		public void TestAPOrgTaxConfigurationTemplateIsReadOnly()
		{
			var isModificationAllowed = Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed;

			try
			{
				Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.OB_OCT_APTaxTemplateInfo.ReadOnly);

				Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.CompanyData.OB_OCT_APTaxTemplateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed = isModificationAllowed;
			}
		}

		#region CannotModifyAROrAPFlag

		void ResetARAndAPData()
		{
			Data.OB_IsCreditor = true;
			Data.OB_IsDebtor = true;
		}

		public void TestHasARTransaction_ExcludeARInvoiceBatch()
		{
			ResetARAndAPData();
			AccTransactionHeader invoiceBatch = GetNewTransactionHeader(Org, LedgerTypes.AccountsReceivable, TransactionTypes.InvoiceBatch, 99, GlbBranch.CurrentBranch);
			invoiceBatch.AH_InvoiceAmount = 99M;
			Factory.Save();

			Assert("Precondition", invoiceBatch.AH_Ledger == LedgerTypes.AccountsReceivable);
			Assert("Precondition", invoiceBatch.AH_TransactionType == TransactionTypes.InvoiceBatch);
			Assert("Precondition", invoiceBatch.AH_FullyPaidDate.IsEmpty);

			Assert("Not has AR Transaction", !Data.HasARTransaction);
		}

		public void TestCannotModifyARFlag()
		{
			ResetARAndAPData();
			ZString lastEventArgs = ZString.Empty;

			bool oldARSetting = Env.Security.OrgReceivablesModify.IsAllowed;
			bool oldARModifyFlagSetting = Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed;
			bool oldARModifyTempFlagSetting = Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed;
			bool oldARNewFlagSetting = Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed;
			bool oldARNewTempFlagSetting = Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed;

			Data.CannotModifyAROrAPFlag += delegate
				(object sender, OrgCompanyData.CannotModifyAROrAPFlagEventArgs e)
			{
				lastEventArgs = e.AROrAPCode;
			};

			Data.SecurityAccessDenied += delegate
				(object sender, OrgCompanyData.CannotModifyAROrAPFlagEventArgs e)
			{
				lastEventArgs = e.AROrAPCode;
			};

			try
			{
				Env.Security.OrgReceivablesModify.IsAllowed = true;
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = true;
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = true;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = true;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = true;

				Org.OH_IsTempAccount = ZBool.True;
				Data.OB_IsDebtor = ZBool.True;
				Assert("Org Should be Debtor", Data.OB_IsDebtor);
				AssertEquals("Last event args should be empty - no error", ZString.Empty, lastEventArgs);

				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = false;
				Data.OB_IsDebtor = ZBool.False;
				Assert("Org Should still be a Debtor", Data.OB_IsDebtor);
				AssertEquals("Last event args should be AR ", LedgerTypes.AccountsReceivable, lastEventArgs);

				lastEventArgs = ZString.Empty;
				Org.OH_IsTempAccount = ZBool.False;
				Data.OB_IsDebtor = ZBool.False;
				Assert("Change allowed. Org should NOT be debtor", !Data.OB_IsDebtor);
				AssertEquals("Last event args should be empty - no error", ZString.Empty, lastEventArgs);

				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = false;
				Data.OB_IsDebtor = ZBool.True;
				Assert("Org Should still NOT be a Debtor", !Data.OB_IsDebtor);
				AssertEquals("Last event args should be AR ", LedgerTypes.AccountsReceivable, lastEventArgs);

				Factory.Save();

				lastEventArgs = ZString.Empty;
				Org.OH_IsTempAccount = ZBool.True;
				Data.OB_IsDebtor = ZBool.True;
				Assert("Org Should be Debtor", Data.OB_IsDebtor);
				AssertEquals("Last event args should be empty - no error", ZString.Empty, lastEventArgs);

				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = false;
				Data.OB_IsDebtor = ZBool.False;
				Assert("Org Should still be a Debtor", Data.OB_IsDebtor);
				AssertEquals("Last event args should be AR ", LedgerTypes.AccountsReceivable, lastEventArgs);

				lastEventArgs = ZString.Empty;
				Org.OH_IsTempAccount = ZBool.False;
				Data.OB_IsDebtor = ZBool.False;
				Assert("Change allowed. Org should NOT be debtor", !Data.OB_IsDebtor);
				AssertEquals("Last event args should be empty - no error", ZString.Empty, lastEventArgs);

				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = false;
				Data.OB_IsDebtor = ZBool.True;
				Assert("Org Should still NOT be a Debtor", !Data.OB_IsDebtor);
				AssertEquals("Last event args should be AR ", LedgerTypes.AccountsReceivable, lastEventArgs);

				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = true;
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = true;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = true;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = true;

				lastEventArgs = ZString.Empty;
				Assert("Has No AR Transaction", !Data.HasARTransaction);
				Data.OB_IsDebtor = false;
				Assert("No transactions present, change allowed. Org should NOT be debtor", !Data.OB_IsDebtor);
				AssertEquals("Event not yet called - last event args should be empty", ZString.Empty, lastEventArgs);

				ResetARAndAPData();
				AccTransactionHeader aRInv = GetNewTransactionHeader(Org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 99, GlbBranch.CurrentBranch);
				aRInv.AH_InvoiceAmount = 99M;
				Factory.Save();

				Assert("Has AR Transaction", Data.HasARTransaction);
				Data.OB_IsDebtor = false;
				Assert("AR Transactions present, Org should still be debtor", Data.OB_IsDebtor);
				AssertEquals("last event args should show AR code", LedgerTypes.AccountsReceivable, lastEventArgs);

				aRInv.Delete();
				Factory.Save();

				Data.OB_IsDebtor = false;
				Assert("No transactions present, change allowed. Org should NOT be debtor", !Data.OB_IsDebtor);
				aRInv = GetNewTransactionHeader(Org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 99, GlbBranch.CurrentBranch);
				aRInv.AH_InvoiceAmount = 99M;
				Factory.Save();

				Assert("Org not debtor but still has AR transaction", Data.HasARTransaction);

				Data.OB_IsDebtor = true;
				Assert("Transactions present, but Org should still be allowed to change to debtor", Data.OB_IsDebtor);
				Assert("Changing debtor status should not lose transaction", Data.HasARTransaction);

				aRInv.Delete();
				Factory.Save();
				Assert("No transactions present", !Data.HasARTransaction);

				var reversedARInvoice = GetNewTransactionHeader(Org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 0m, GlbBranch.CurrentBranch);
				reversedARInvoice.AH_IsCancelled = true;
				Factory.Save();

				Assert("Org is still debtor", Data.OB_IsDebtor);
				Assert(!Data.HasARTransaction);
				Data.OB_IsDebtor = false;
				Assert("Reversed AR Transaction present, change allowed", !Data.OB_IsDebtor);

				reversedARInvoice.Delete();
				ResetARAndAPData();
				Factory.Save();
				Assert("No transactions present", !Data.HasARTransaction);

				var paidARInvoice = GetNewTransactionHeader(Org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 0m, GlbBranch.CurrentBranch);
				paidARInvoice.AH_FullyPaidDate = ZDateTime.Now;
				Factory.Save();

				Assert("Org is still debtor", Data.OB_IsDebtor);
				Assert(!Data.HasARTransaction);
				Data.OB_IsDebtor = false;
				Assert("Paid AR Transaction present, change allowed", !Data.OB_IsDebtor);

				paidARInvoice.Delete();
				ResetARAndAPData();
				Factory.Save();
				Assert("No transactions present", !Data.HasARTransaction);

				var unpaidARInvoice = GetNewTransactionHeader(Org, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 100m, GlbBranch.CurrentBranch);
				unpaidARInvoice.AH_InvoiceAmount = 100m;
				Factory.Save();

				Assert("Org is still debtor", Data.OB_IsDebtor);
				Assert(Data.HasARTransaction);
				Data.OB_IsDebtor = false;
				Assert("Unpaid AR Transaction present, change not allowed", Data.OB_IsDebtor);

				unpaidARInvoice.Delete();
				Factory.Save();
				Assert("No transactions present", !Data.HasARTransaction);

				var activeWIP = GetNewTransactionLine(Org, TransactionLineTypes.WIP, 20m, GlbBranch.CurrentBranch);
				Factory.Save();

				Assert("Org is still debtor", Data.OB_IsDebtor);
				Assert(Data.HasARTransaction);
				Data.OB_IsDebtor = false;
				Assert("Active WIP present, change not allowed", Data.OB_IsDebtor);
			}
			finally
			{
				Env.Security.OrgReceivablesModify.IsAllowed = oldARSetting;
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = oldARModifyFlagSetting;
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = oldARModifyTempFlagSetting;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = oldARNewFlagSetting;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = oldARNewTempFlagSetting;
			}
		}

		public void TestCannotModifyAPFlag()
		{
			ResetARAndAPData();
			Factory.Save();
			bool previousAPRegistrySetting = Env.Security.OrgPayablesModify.IsAllowed;
			ZString lastEventArgs = ZString.Empty;

			Data.CannotModifyAROrAPFlag += delegate
				(object sender, OrgCompanyData.CannotModifyAROrAPFlagEventArgs e)
			{
				lastEventArgs = e.AROrAPCode;
			};

			try
			{
				Env.Security.OrgPayablesModify.IsAllowed = true;

				Assert("Has No AP Transaction", !Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("No transactions present, change allowed. Org should NOT be Creditor", !Data.OB_IsCreditor);
				AssertEquals("Event not yet called - last event args should be empty", ZString.Empty, lastEventArgs);

				ResetARAndAPData();
				AccTransactionHeader aPInv = GetNewTransactionHeader(Org, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 99, GlbBranch.CurrentBranch);
				aPInv.AH_InvoiceAmount = 99M;
				Factory.Save();

				Assert("Has AP Transaction", Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("AP Transactions present, Org should still be Creditor", Data.OB_IsCreditor);
				AssertEquals("last event args should show AP code", LedgerTypes.AccountsPayable, lastEventArgs);

				aPInv.Delete();
				Factory.Save();

				Data.OB_IsCreditor = false;
				Assert("No transactions present, change allowed. Org should NOT be Creditor", !Data.OB_IsCreditor);
				aPInv = GetNewTransactionHeader(Org, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 99, GlbBranch.CurrentBranch);
				aPInv.AH_InvoiceAmount = 99M;
				Factory.Save();

				Assert("Org not creditor but still has AP transaction", Data.HasAPTransaction);

				Data.OB_IsCreditor = true;
				Assert("Transactions present, but Org should still be allowed to change to creditor", Data.OB_IsCreditor);
				Assert("Changing creditor status should not lose transaction", Data.HasAPTransaction);

				aPInv.Delete();
				Factory.Save();
				Assert("No transactions present", !Data.HasAPTransaction);

				var reversedAPInvoice = GetNewTransactionHeader(Org, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 0m, GlbBranch.CurrentBranch);
				reversedAPInvoice.AH_IsCancelled = true;
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(!Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Reversed AP Transaction present, change allowed", !Data.OB_IsCreditor);

				reversedAPInvoice.Delete();
				ResetARAndAPData();
				Factory.Save();
				Assert("No transactions present", !Data.HasAPTransaction);

				var paidAPInvoice = GetNewTransactionHeader(Org, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 0m, GlbBranch.CurrentBranch);
				paidAPInvoice.AH_FullyPaidDate = ZDateTime.Now;
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(!Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Paid AP Transaction present, change allowed", !Data.OB_IsCreditor);

				paidAPInvoice.Delete();
				ResetARAndAPData();
				Factory.Save();
				Assert("No transactions present", !Data.HasAPTransaction);

				var unpaidAPInvoice = GetNewTransactionHeader(Org, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 100m, GlbBranch.CurrentBranch);
				unpaidAPInvoice.AH_InvoiceAmount = 100m;
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Unpaid AP Transaction present, change not allowed", Data.OB_IsCreditor);

				unpaidAPInvoice.Delete();
				Factory.Save();
				Assert("No transactions present", !Data.HasAPTransaction);

				var activeACR = GetNewTransactionLine(Org, TransactionLineTypes.Accrual, 20m, GlbBranch.CurrentBranch);
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Active ACR present, change not allowed", Data.OB_IsCreditor);

				activeACR.AL_ReverseDate = ZDateTime.Today;
				Factory.Save();
				Assert("No transactions present", !Data.HasAPTransaction);

				var unapprovedAPInvoice = GetNewTransactionHeader(Org, LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice, 100m, GlbBranch.CurrentBranch);
				unapprovedAPInvoice.AH_InvoiceAmount = 100m;
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Unapproved AP Transaction present, change not allowed", Data.OB_IsCreditor);

				unapprovedAPInvoice.AH_IsCancelled = true;
				Factory.Save();
				Assert("No transactions present", !Data.HasAPTransaction);

				var unallocatedAPInvoice = GetNewTransactionHeader(Org, LedgerTypes.TransactionsPendingAllocation, TransactionTypes.InvoicePendingAllocation, 100m, GlbBranch.CurrentBranch);
				unallocatedAPInvoice.AH_InvoiceAmount = 100m;
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Unallocated AP Transaction present, change not allowed", Data.OB_IsCreditor);

				unallocatedAPInvoice.AH_IsCancelled = true;
				Factory.Save();
				Assert("No transactions present", !Data.HasAPTransaction);

				var incompleteAPInvoice = GetNewTransactionHeader(Org, LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteInvoice, 100m, GlbBranch.CurrentBranch);
				incompleteAPInvoice.AH_InvoiceAmount = 100m;
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Incomplete AP Transaction present, change not allowed", Data.OB_IsCreditor);

				incompleteAPInvoice.AH_IsCancelled = true;
				Factory.Save();

				Assert("Org is still creditor", Data.OB_IsCreditor);
				Assert(!Data.HasAPTransaction);
				Data.OB_IsCreditor = false;
				Assert("Incomplete AP Transaction is cancelled, change is allowed", !Data.OB_IsCreditor);
			}
			finally
			{
				Env.Security.OrgPayablesModify.IsAllowed = previousAPRegistrySetting;
			}
		}

		[SuspendCriticalValidation]
		public void TestGetActiveTransactionDetails()
		{
			var organisation = GlbCompany.CurrentCompany.OrgProxy;
			SetupTestData(organisation);
			Factory.Save();

			var expectedCollectionForDeactivatingOrg = new List<Tuple<string, string>>();
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("EDI", "ACR"));
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("QAA", "ACR"));
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("QAA", "AR"));
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("QAA", "IN"));
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("QAA", "UA"));
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("QBB", "AP"));
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("QBB", "PA"));
			expectedCollectionForDeactivatingOrg.Add(Tuple.Create("QBB", "WIP"));

			var actualCollectionForDeactivatingOrg = organisation.CompanyData.GetActiveTransactionDetails(true);
			AssertEquals(expectedCollectionForDeactivatingOrg.Count, actualCollectionForDeactivatingOrg.Count);
			foreach (DynamicBusinessObject transaction in actualCollectionForDeactivatingOrg)
			{
				var actualTransaction = Tuple.Create(transaction["CompanyCode"].ToString(), transaction["TransactionType"].ToString());
				if (expectedCollectionForDeactivatingOrg.Contains(actualTransaction))
				{
					expectedCollectionForDeactivatingOrg.Remove(actualTransaction);
				}
				else
				{
					Fail(ZString.Format("{0} should be in the collection", actualTransaction.ToString()));
				}
			}

			var actualCollectionForARAPFlagChange = organisation.CompanyData.GetActiveTransactionDetails(false);
			AssertEquals(1, actualCollectionForARAPFlagChange.Count);
			AssertEquals("EDI", actualCollectionForARAPFlagChange[0]["CompanyCode"].ToString());
			AssertEquals("ACR", actualCollectionForARAPFlagChange[0]["TransactionType"].ToString());
		}

		void SetupTestData(OrgHeader organisation)
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			companyA.GC_OH_OrgProxy = organisation.PK;
			companyA.GC_Code = "QAA";

			var companyBranchA = Factory.NewWithValidTestData<GlbBranch>();
			companyBranchA.GB_GC = companyA.PK;
			companyBranchA.GB_OH_OrgProxy = companyA.GC_OH_OrgProxy;

			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			companyB.GC_OH_OrgProxy = organisation.PK;
			companyB.GC_Code = "QBB";

			var companyBranchB = Factory.NewWithValidTestData<GlbBranch>();
			companyBranchB.GB_GC = companyB.PK;
			companyBranchB.GB_OH_OrgProxy = companyB.GC_OH_OrgProxy;

			var activeACR1 = GetNewTransactionLine(organisation, TransactionLineTypes.Accrual, 20m, companyBranchA);
			var unapprovedAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice, 100m, companyBranchA);
			unapprovedAPInvoice.AH_InvoiceAmount = 100m;
			var incompleteAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.IncompleteTransactions, TransactionTypes.IncompleteInvoice, 100m, companyBranchA);
			incompleteAPInvoice.AH_InvoiceAmount = 100m;
			var unpaidARInvoice = GetNewTransactionHeader(organisation, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 100m, companyBranchA);
			unpaidARInvoice.AH_InvoiceAmount = 100m;

			var activeWIP = GetNewTransactionLine(organisation, TransactionLineTypes.WIP, 20m, companyBranchB);
			var unallocatedAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.TransactionsPendingAllocation, TransactionTypes.InvoicePendingAllocation, 100m, companyBranchB);
			unallocatedAPInvoice.AH_InvoiceAmount = 100m;
			var unpaidAPInvoice = GetNewTransactionHeader(organisation, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, 100m, companyBranchB);
			unpaidAPInvoice.AH_InvoiceAmount = 100m;

			var activeACR2 = GetNewTransactionLine(organisation, TransactionLineTypes.Accrual, 20m, GlbBranch.CurrentBranch);
			var paidARInvoice = GetNewTransactionHeader(organisation, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 0m, GlbBranch.CurrentBranch);
			paidARInvoice.AH_FullyPaidDate = ZDateTime.Now;
		}

		public void TestConvertActiveTransactionCollectionToString()
		{
			var organisation = GlbCompany.CurrentCompany.OrgProxy;
			SetupTestData(organisation);
			Factory.Save();

			var collection = organisation.CompanyData.GetActiveTransactionDetails(true);
			var expectedString = @"	- Company Code: EDI: Accrual
	- Company Code: QAA: Accrual, Accounts Receivable, Incomplete, Unapproved
	- Company Code: QBB: Accounts Payable, Pending Allocation, WIP";

			AssertEquals(expectedString, organisation.CompanyData.ConvertActiveTransactionCollectionToString(collection));
		}

		public void TestShouldNotCreateAccAccountFeeAfterOrgCompanyDataDeleted()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			Factory.Save();

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = header.CompanyData;
			companyData.OB_IsDebtor = true;
			var accountFee = companyData.AccountFeeSettings;
			accountFee.OverrideSettings = true;
			accountFee.AAF_Rule = AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted;
			accountFee.AAF_FeeAmount = 250m;
			accountFee.AAF_AG_GLAccount = glHeader.PK;
			accountFee.AAF_RX_NKFeeCurrency = currency.RX_Code;

			companyData.Delete();
			AssertNoExceptionThrown("No exception expected", () => Factory.Save());

			var query = new ZQuery(AccAccountFeeSchema.AAF_OB_CompanyData, companyData.PK);
			Assert(!Factory.ExistsInDatabase("AccAccountFee", query));
		}

		public void TestConvertActiveTransactionCollectionToString_ThrowDeveloperNotificationExceptionWhenNewTransactionTypeIsDiscovered()
		{
			var organisation = GlbCompany.CurrentCompany.OrgProxy;
			var unpaidARInvoice = GetNewTransactionHeader(organisation, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 100m, GlbBranch.CurrentBranch);
			unpaidARInvoice.AH_InvoiceAmount = 100m;
			Factory.Save();

			DbConnection conn = ((IDbConnected)Factory).Connection;
			var selectQuery = @"SELECT 'ABC' AS CompanyCode, 'AA' as TransactionType 
		FROM dbo.AccTransactionHeader INNER JOIN dbo.GlbCompany
		ON AccTransactionHeader.AH_GC = GlbCompany.GC_PK";

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(selectQuery);

			AssertExceptionThrown(typeof(DeveloperNotificationException), "Found new transaction type: AA. Please update the TransactionTypeFullDescription dictionary.",
				() => organisation.CompanyData.ConvertActiveTransactionCollectionToString(collection));
		}

		#endregion

		#region AR/AP Transactions

		public void TestRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_DSB()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_DSB(false);
		}

		public void TestRequiredAuthorizationForGlobalCreditControlledDocumentDeliveryDueToCreditCheck_DSB()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_DSB(true);
		}

		void AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_DSB(bool isGlobal)
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);
			var companyPK = isGlobal ? singaporeCompany.PK : GlbCompany.CurrentCompany.PK;

			var dsb_none = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 3, 10m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired);
			var dsb_1st = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 5, 20m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var dsb_2nd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 10, 30m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var dsb_3rd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 10, 30m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			var all_none = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 5, 45m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired);
			var all_1st = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 12, 50m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var all_2nd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 22, 100m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var all_3rd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 22, 100m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { dsb_none, dsb_1st, dsb_2nd, dsb_3rd, all_none, all_1st, all_2nd, all_3rd };

			if (isGlobal)
			{
				accountingMock.Setup(m => m.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

				Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var tempFactory = new BusinessObjectFactory();
					var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
					tempOrg.CompanyData.OB_IsDebtor = true;

					var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
					var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
					tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
					tempExchangeRate.RE_SellRate = 2m;
					tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
					tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

					tempFactory.Save();
				}
			}
			else
			{
				accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);
			}

			ObjectFactory.Substitute(accountingMock.Object);

			//DSB
			CreateInvoiceForCreditControlledDocumentsCheck(3, Org, 2, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			var businessObject = Factory.New<CreditControlledBizo>();

			AssertEquals("No Approval required", 0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(2, Org, 4, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("First level authorization required", 1, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(10, Org, 6, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Second level authorization required", 2, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(15, Org, 11, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Third level authorization required", 3, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(20, Org, 11, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Third level authorization required", 3, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			//All
			CreateInvoiceForCreditControlledDocumentsCheck(20, Org, 23, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(30, Org, 23, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Third level authorization required from 'All' setting, total outstanding = 110", 3, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		public void TestRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB_Case2()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB_Case2(false);
		}

		public void TestRequiredAuthorizationForGlobalCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB_Case2()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB_Case2(true);
		}

		void AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB_Case2(bool isGlobal)
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);
			var companyPK = isGlobal ? singaporeCompany.PK : GlbCompany.CurrentCompany.PK;

			var nonDsb_2nd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code, 10, 1000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var all_1st = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 2, 10000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { nonDsb_2nd, all_1st };

			if (isGlobal)
			{
				accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

				Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var tempFactory = new BusinessObjectFactory();
					var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
					tempOrg.CompanyData.OB_IsDebtor = true;

					var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
					var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
					tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
					tempExchangeRate.RE_SellRate = 2m;
					tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
					tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

					tempFactory.Save();
				}
			}
			else
			{
				accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);
			}
			ObjectFactory.Substitute(accountingMock.Object);

			//Non DSB
			CreateInvoiceForCreditControlledDocumentsCheck(2000, Org, 12, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			var businessObject = Factory.New<CreditControlledBizo>();
			AssertEquals("Second level authorization required", 2, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		public void TestRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB(false);
		}

		public void TestRequiredAuthorizationForGlobalCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB(true);
		}

		void AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck_nonDSB(bool isGlobal)
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);
			var companyPK = isGlobal ? singaporeCompany.PK : GlbCompany.CurrentCompany.PK;

			var nonDsb_1st = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code, 5, 10m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var nonDsb_2nd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code, 7, 20m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var nonDsb_3rd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code, 7, 20m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			var all_1st = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 12, 50m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var all_2nd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 22, 100m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var all_3rd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 22, 100m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { nonDsb_1st, nonDsb_2nd, nonDsb_3rd, all_1st, all_2nd, all_3rd };

			if (isGlobal)
			{
				accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

				Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var tempFactory = new BusinessObjectFactory();
					var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
					tempOrg.CompanyData.OB_IsDebtor = true;

					var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
					var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
					tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
					tempExchangeRate.RE_SellRate = 2m;
					tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
					tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

					tempFactory.Save();
				}
			}
			else
			{
				accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);
			}
			ObjectFactory.Substitute(accountingMock.Object);

			//Non DSB
			CreateInvoiceForCreditControlledDocumentsCheck(3, Org, 3, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			var businessObject = Factory.New<CreditControlledBizo>();

			AssertEquals("First level authorization required", 1, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(2, Org, 6, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Second level authorization required", 2, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(15, Org, 8, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Third level authorization required", 3, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(20, Org, 8, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Third level authorization required", 3, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			//All
			CreateInvoiceForCreditControlledDocumentsCheck(30, Org, 23, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(30, Org, 23, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("Third level authorization required from 'All' setting", 3, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		public void TestIsRestrictedDueToCreditControlledDocumentsCheck_All()
		{
			AssertIsRestrictedDueToCreditControlledDocumentsCheck_All(false);
		}

		public void TestIsRestrictedDueToGlobalCreditControlledDocumentsCheck_All()
		{
			AssertIsRestrictedDueToCreditControlledDocumentsCheck_All(true);
		}

		void AssertIsRestrictedDueToCreditControlledDocumentsCheck_All(bool isGlobal)
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);
			var companyPK = isGlobal ? singaporeCompany.PK : GlbCompany.CurrentCompany.PK;

			var allCfg = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 7, 1000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { allCfg };

			if (isGlobal)
			{
				accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

				Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var tempFactory = new BusinessObjectFactory();
					var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
					tempOrg.CompanyData.OB_IsDebtor = true;

					var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
					var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
					tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
					tempExchangeRate.RE_SellRate = 2m;
					tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
					tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

					tempFactory.Save();
				}
			}
			else
			{
				accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);
			}
			ObjectFactory.Substitute(accountingMock.Object);

			var businessObject = Factory.New<CreditControlledBizo>();
			AssertEquals(0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(2000, Org, 4, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();
			AssertEquals("Ignores debt only 4 days overdue", 0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			CreateInvoiceForCreditControlledDocumentsCheck(1000, Org, 10, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();
			AssertEquals("Debt over 7 days overdue is at limit", 2, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			CreateInvoiceForCreditControlledDocumentsCheck(1, Org, 10, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();
			AssertEquals("Debt over 7 days overdue is over limit", 2, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		public void TestIsRestrictedDueToCreditControlledDocumentsCheck_NotDsb()
		{
			AssertIsRestrictedDueToCreditControlledDocumentsCheck_NotDsb(false);
		}

		public void TestIsRestrictedDueToGlobalCreditControlledDocumentsCheck_NotDsb()
		{
			AssertIsRestrictedDueToCreditControlledDocumentsCheck_NotDsb(true);
		}

		void AssertIsRestrictedDueToCreditControlledDocumentsCheck_NotDsb(bool isGlobal)
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);

			var companyPK = isGlobal ? singaporeCompany.PK : GlbCompany.CurrentCompany.PK;

			var dsbCfg = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 10, 1000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var notDsbCfg = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code, 20, 5000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { dsbCfg, notDsbCfg };

			if (isGlobal)
			{
				accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

				Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var tempFactory = new BusinessObjectFactory();
					var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
					tempOrg.CompanyData.OB_IsDebtor = true;

					var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
					var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
					tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
					tempExchangeRate.RE_SellRate = 2m;
					tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
					tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

					tempFactory.Save();
				}
			}
			else
			{
				accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);
			}
			ObjectFactory.Substitute(accountingMock.Object);

			var businessObject = Factory.New<CreditControlledBizo>();
			AssertEquals(0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(5000M, Org, 20, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(5000M, Org, 21, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(1000M, Org, 10, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(1000M, Org, 11, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();
			AssertEquals("Dsb and not dsb debt is within limit, no authorization required", 0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			CreateInvoiceForCreditControlledDocumentsCheck(1M, Org, 21, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();
			AssertEquals("Not dsb debt is over limit, no authorization required", 2, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		ICreditControlledDocumentsCheckConfiguration createCreditControlledDocumentsCheckConfigurationLine(string invoiceType, int numberOfDaysOverdue, decimal amount, string range, string authorisationRequirement)
		{
			var creditControlledDocumentsCheckConfigurationLine = new Mock<ICreditControlledDocumentsCheckConfiguration>();
			creditControlledDocumentsCheckConfigurationLine.Setup(c => c.InvoiceType).Returns(invoiceType);
			creditControlledDocumentsCheckConfigurationLine.Setup(c => c.NumberOfDaysOverdue).Returns(numberOfDaysOverdue);
			creditControlledDocumentsCheckConfigurationLine.Setup(c => c.Amount).Returns(amount);
			creditControlledDocumentsCheckConfigurationLine.Setup(c => c.Range).Returns(range);
			creditControlledDocumentsCheckConfigurationLine.Setup(c => c.AuthorisationRequirement).Returns(authorisationRequirement);

			return creditControlledDocumentsCheckConfigurationLine.Object;
		}

		public void TestIsRestrictedDueToCreditControlledDocumentsCheck_Dsb()
		{
			AssertIsRestrictedDueToCreditControlledDocumentsCheck_Dsb(false);
		}

		public void TestIsRestrictedDueToGlobalCreditControlledDocumentsCheck_Dsb()
		{
			AssertIsRestrictedDueToCreditControlledDocumentsCheck_Dsb(true);
		}

		void AssertIsRestrictedDueToCreditControlledDocumentsCheck_Dsb(bool isGlobal)
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);

			var companyPK = isGlobal ? singaporeCompany.PK : GlbCompany.CurrentCompany.PK;

			var dsbCfg = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 10, 1000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var notDsbCfg = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code, 20, 5000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { dsbCfg, notDsbCfg };

			if (isGlobal)
			{
				accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

				Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var tempFactory = new BusinessObjectFactory();
					var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
					tempOrg.CompanyData.OB_IsDebtor = true;

					var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
					var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
					tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
					tempExchangeRate.RE_SellRate = 2m;
					tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
					tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

					tempFactory.Save();
				}
			}
			else
			{
				accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);
			}
			ObjectFactory.Substitute(accountingMock.Object);

			var businessObject = Factory.New<CreditControlledBizo>();
			AssertEquals(0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(5000M, Org, 20, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(5000M, Org, 21, companyPK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(1000M, Org, 10, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoiceForCreditControlledDocumentsCheck(1000M, Org, 11, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();
			AssertEquals("Dsb and not dsb debt is within limit", 0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			CreateInvoiceForCreditControlledDocumentsCheck(1M, Org, 11, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();
			AssertEquals("Dsb debt is over limit", 3, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		public void TestIsRestrictedDueToGlobalCreditControlledDocumentsCheckWithInvoiceFromOtherCompany()
		{
			Org.CompanyData.OB_IsDebtor = true;
			var localFirstLevel = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 25, 100m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var localSecondLevel = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 25, 100m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			var globalFirstLevel = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 20, 200m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var globalSecondLevel = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code, 20, 200m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var localConfig = new ICreditControlledDocumentsCheckConfiguration[] { localFirstLevel, localSecondLevel };
			var globalConfig = new ICreditControlledDocumentsCheckConfiguration[] { globalFirstLevel, globalSecondLevel };

			Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

			accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(localConfig);
			accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(globalConfig);
			ObjectFactory.Substitute(accountingMock.Object);

			var businessObject = Factory.New<CreditControlledBizo>();
			AssertEquals(0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(30M, Org, 10, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();
			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("over local limit", 1, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var factory = new BusinessObjectFactory();
				var tempOrg = factory.Load<OrgHeader>(Org.PK);
				tempOrg.CompanyData.OB_IsDebtor = true;

				var tempCurrency = factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
				var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
				tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
				tempExchangeRate.RE_SellRate = 2m;
				tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
				tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

				CreateInvoiceForCreditControlledDocumentsCheck(30M, Org, 22, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice, factory);
				factory.Save();
			}
			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);
			AssertEquals("over global limit", 2, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		public void TestMissingExRateWhenCheckGlobalCreditControlledDocument()
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);

			var companyPK = singaporeCompany.PK;

			var dsbCfg = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 10, 1000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var notDsbCfg = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB.Code, 20, 5000m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { dsbCfg, notDsbCfg };
			accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

			Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var tempFactory = new BusinessObjectFactory();
				var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
				tempOrg.CompanyData.OB_IsDebtor = true;
				tempFactory.Save();
			}
			ObjectFactory.Substitute(accountingMock.Object);

			var businessObject = Factory.New<CreditControlledBizo>();
			AssertEquals(10, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		int transactionNumber;

		void CreateInvoiceForCreditControlledDocumentsCheck(ZDecimal amount, OrgHeader org, ZInt? overdueDays, ZGuid companyPK, ZString transactionCategory, BusinessObjectFactory factory = null)
		{
			factory = factory ?? Factory;

			var branch = factory.Load<GlbCompany>(companyPK).FirstActiveBranch;
			var invoice = factory.New<AccTransactionHeader>();
			invoice.AH_Desc = "Test Invoice";
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_ExchangeRate = 1;
			invoice.AH_OH = org.PK;
			invoice.AH_TransactionNum = (transactionNumber++).ToString();
			invoice.AH_Ledger = "AR";
			invoice.AH_InvoiceDate = DateTime.Now;
			invoice.AH_GB = branch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_OutstandingAmount = amount;
			invoice.AH_InvoiceAmount = amount;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_TransactionCategory = transactionCategory;
			invoice.AH_DueDate = overdueDays.HasValue ? ZDateTime.Now.AddDays(-overdueDays.Value) : ZDateTime.Empty;

			var line = factory.New<AccTransactionLines>();
			line.AL_AH = invoice.PK;
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_LineAmount = amount;
			line.AL_OSAmount = amount;
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_ExchangeRate = 1;
			line.AL_OSAmount = amount;
			line.AL_Desc = "tee he he";
			line.AL_GB = branch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;
		}

		public void TestIsAtOrOverCreditLimit()
		{
			foreach (string transactionType in new[]
					{
									TransactionTypes.Invoice,
									TransactionTypes.CreditNote,
									TransactionTypes.AdjustmentNote,
									TransactionTypes.Payment,
									TransactionTypes.Receipt,
									TransactionTypes.Journal,
									TransactionTypes.Contra,
									TransactionTypes.Transfer
								})
			{
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				AccTransactionHeader transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
				transaction.AH_OH = orgHeader.PK;
				transaction.AH_TransactionType = transactionType;
				transaction.AH_InvoiceAmount = transaction.AH_OutstandingAmount = 10m;
				if (transactionType == TransactionTypes.Invoice)
				{
					transaction.AH_GSTAmount = 1M;
					transaction.AH_OutstandingAmount = 11M;
				}
				transaction.AH_GC = GlbCompany.CurrentCompany.PK;

				transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
				transaction.AH_OH = orgHeader.PK;
				transaction.AH_TransactionType = TransactionTypes.InvoiceBatch;
				transaction.AH_InvoiceAmount = transaction.AH_OutstandingAmount = 3.3m;
				transaction.AH_GC = GlbCompany.CurrentCompany.PK;

				AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
				collection.Add(CreateNewSettings(0.00001M, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
				collection.Add(CreateNewSettings(0.00001M, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));

				AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgHeader.CompanyData, transactionType == TransactionTypes.Invoice ? 10.99M : 9.99M, 0M);
				Factory.Save();

				AssertEquals(transactionType, 3, orgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgHeader.CompanyData, 12M, 0M);
				Factory.Save();

				var newFactory = new BusinessObjectFactory(); // to avoid caching the Credit Info
				var reloadedOrgHeader = newFactory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(transactionType, 0, reloadedOrgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgHeader.CompanyData, 9M, 0M);
				Factory.Save();

				newFactory = new BusinessObjectFactory(); // to avoid caching the Credit Info
				reloadedOrgHeader = newFactory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(transactionType, 3, reloadedOrgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgHeader.CompanyData, 9M, 3M);
				Factory.Save();

				newFactory = new BusinessObjectFactory(); // to avoid caching the Credit Info
				reloadedOrgHeader = newFactory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(transactionType, 0, reloadedOrgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgHeader.CompanyData, 6M, 3M);
				Factory.Save();

				newFactory = new BusinessObjectFactory(); // to avoid caching the Credit Info
				reloadedOrgHeader = newFactory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(transactionType, 3, reloadedOrgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgHeader.CompanyData, transactionType == TransactionTypes.Invoice ? 11.01M : 10.01M, 0M);
				Factory.Save();

				newFactory = new BusinessObjectFactory(); // to avoid caching the Credit Info
				reloadedOrgHeader = newFactory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(transactionType, 0, reloadedOrgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

				CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgHeader.CompanyData, transactionType == TransactionTypes.Invoice ? 11M : 10M, 0M);
				Factory.Save();

				newFactory = new BusinessObjectFactory(); // to avoid caching the Credit Info
				reloadedOrgHeader = newFactory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(transactionType, 0, reloadedOrgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);
			}
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}

		public void TestIsAtOrOverCreditLimit_ZeroCreditLimit()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Default Value", false, ObjectFactory.Get<IAccounting>().UseWebServiceForCreditLimit);
			AssertEquals(0, orgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

			AccTransactionHeader transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_OH = orgHeader.PK;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_InvoiceAmount = transaction.AH_OutstandingAmount = 10m;
			transaction.AH_GSTAmount = 1M;
			transaction.AH_OutstandingAmount = 11M;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;

			orgHeader.CompanyData.OB_ARCreditLimit = 11M;

			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("Is At CreditLimit", 0, orgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

			orgHeader.CompanyData.OB_ARCreditLimit = 0M; // No Credit Limit
			var settlementGroup = Factory.NewWithValidTestData<OrgHeader>();
			settlementGroup.CompanyData.OB_ARCreditLimit = 11M;
			orgHeader.ARSettlementGroupPK = settlementGroup.PK;
			orgHeader.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			Factory.Save();
			orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("Is At CreditLimit should use Settlement Group Credit Limit", 0, orgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);

			orgHeader.CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
			Factory.Save();
			AssertEquals("Previous cached value should be used.", 0, orgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);
			orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("Credit Limit is not checked when it is 0 and we do not use Web Service for it", 0, orgHeader.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit);
		}

		[TestDate(2010, 11, 18, 9, 12, 55)]
		public void TestCreditControlEvents()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Assert("Credit is approved according to registry setting in registry", org.CompanyData.OB_ARCreditApproved);
			org.CompanyData.OB_ARCreditApproved = false;
			AssertEquals("Setting OB_ARCreditApproved back to False clears HasChanges on the property", false, org.CompanyData.OB_ARCreditApprovedInfo.HasChanges);
			Factory.Save();
			AssertEvents(org, 0);

			org.CompanyData.OB_ARCreditRating = "CR2";
			Factory.Save();
			AssertEvents(org, 0, "Credit Control and Settlement - Credit Rating: CR2");

			org.CompanyData.OB_ARCreditLimit = 10000m;
			Factory.Save();
			AssertEvents(org, 0, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00");

			org.CompanyData.OB_ARAccountAndCreditReviewDue = ZDateTime.Now.AddDays(90);
			Factory.Save();
			AssertEvents(org, 0, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00",
					"Credit Control and Settlement - Credit Review Due: 16/02/2011");

			org.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			Factory.Save();
			AssertEvents(org, 0, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00",
					"Credit Control and Settlement - Credit Review Due: 16/02/2011",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: Y");

			org.CompanyData.OB_ARCreditApproved = true;
			Factory.Save();
			AssertEvents(org, 1, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00",
					"Credit Control and Settlement - Credit Review Due: 16/02/2011",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: Y");

			org.CompanyData.OB_ARCreditApproved = false;
			Factory.Save();
			AssertEvents(org, 1, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00",
					"Credit Control and Settlement - Credit Review Due: 16/02/2011",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: Y",
					"Credit Control and Settlement - Credit Approved: N");

			org.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();
			AssertEvents(org, 1, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00",
					"Credit Control and Settlement - Credit Review Due: 16/02/2011",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: Y",
					"Credit Control and Settlement - Credit Approved: N",
					"Credit Control and Settlement - Credit On Hold: Y|TYPE=COH");

			org.CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
			org.CompanyData.OB_AROnCreditHold = false;
			org.CompanyData.OB_ARCreditApproved = true;
			Factory.Save();
			AssertEvents(org, 2, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00",
					"Credit Control and Settlement - Credit Review Due: 16/02/2011",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: Y",
					"Credit Control and Settlement - Credit Approved: N",
					"Credit Control and Settlement - Credit On Hold: Y|TYPE=COH",
					"Credit Control and Settlement - Credit On Hold: N|TYPE=COH",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: N");

			org.CompanyData.OB_ARCreditAgreedPaymentMethod = "CHK";
			Factory.Save();
			AssertEvents(org, 2, "Credit Control and Settlement - Credit Rating: CR2",
					"Credit Control and Settlement - Credit Limit: $10,000.00",
					"Credit Control and Settlement - Credit Review Due: 16/02/2011",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: Y",
					"Credit Control and Settlement - Credit Approved: N",
					"Credit Control and Settlement - Credit On Hold: Y|TYPE=COH",
					"Credit Control and Settlement - Credit On Hold: N|TYPE=COH",
					"Credit Control and Settlement - Use Settlement Group Credit Limit: N",
					"Credit Control and Settlement - Agreed Payment Method: CHK");
		}

		[TestDate(2010, 11, 18, 9, 12, 55)]
		public void TestCreditControlEditedDate()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Assert("Should be Empty", org.CompanyData.CreditControlEditedDate.IsEmpty);
			Factory.Save();
			AssertEvents(org, 0);
			Assert("Should be Empty", org.CompanyData.CreditControlEditedDate.IsEmpty);

			org.CompanyData.OB_ARCreditLimit = 10000m;
			Assert("Should be Empty", org.CompanyData.CreditControlEditedDate.IsEmpty);
			ZDateTime logTime = ZDateTime.Now;
			Factory.Save();
			AssertEvents(org, 0, "Credit Control and Settlement - Credit Limit: $10,000.00");
			Assert("Should be not Empty", !org.CompanyData.CreditControlEditedDate.IsEmpty);
			AssertEquals("Should be as expected", logTime, org.CompanyData.CreditControlEditedDate);
		}

		[TestDate(2010, 11, 18, 9, 12, 55)]
		public void TestCreditCreditApprovedDate()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Assert("Should be Empty", org.CompanyData.CreditApprovedDate.IsEmpty);
			org.CompanyData.OB_ARCreditApproved = false;
			Factory.Save();
			AssertEvents(org, 0);
			Assert("Should be Empty", org.CompanyData.CreditApprovedDate.IsEmpty);

			org.CompanyData.OB_ARCreditApproved = true;
			Assert("Should be Empty", org.CompanyData.CreditApprovedDate.IsEmpty);
			ZDateTime logTime = ZDateTime.Now;
			Factory.Save();
			AssertEvents(org, 1);
			Assert("Should be not Empty", !org.CompanyData.CreditApprovedDate.IsEmpty);
			AssertEquals("Should be as expected", logTime, org.CompanyData.CreditApprovedDate);
		}

		[TestDate(2010, 11, 18, 9, 12, 55)]
		void AssertEvents(OrgHeader org, int expectedCreditApprovalChangeEvents, params string[] expectedCreditControlChangeEventReferences)
		{
			StmALog[] controlEditedLogs = org.CompanyData.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));
			AssertEquals("Should be expected number of the CreditControlsEdited events", expectedCreditControlChangeEventReferences.Length, controlEditedLogs.Length);
			StmALog[] approvedLogs = org.CompanyData.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditApprovalGrantedCode));
			AssertEquals("Should be expected number of the CreditApproved events", expectedCreditApprovalChangeEvents, approvedLogs.Length);

			foreach (StmALog log in approvedLogs)
			{
				AssertEquals("Credit Control and Settlement - Credit Approval Granted By: E - CWSupport", log.SL_Reference);
			}

			foreach (StmALog log in controlEditedLogs)
			{
				Assert(string.Format("Event's Reference '{0}' should be among expected", log.SL_Reference), expectedCreditControlChangeEventReferences.Contains(log.SL_Reference.ToString()));
			}
		}

		public void TestSetDefaultPayablesAccount()
		{
			RefCurrency currency1 = Factory.New<RefCurrency>();
			currency1.RX_Code = "AUD";
			RefCurrency currency2 = Factory.New<RefCurrency>();
			currency2.RX_Code = "USA";
			RefCurrency currency3 = Factory.New<RefCurrency>();
			currency3.RX_Code = "HKD";
			RefCurrency currency4 = Factory.New<RefCurrency>();
			currency4.RX_Code = "JPY";

			GlbBranch branch1 = Factory.New<GlbBranch>();
			GlbBranch branch2 = Factory.New<GlbBranch>();
			GlbBranch branch3 = Factory.New<GlbBranch>();
			GlbBranch branch4 = Factory.New<GlbBranch>();

			MasterFilesTestHelper helper = new MasterFilesTestHelper(Factory);

			AccBankAccount account1 = helper.GetNewBankAccountWithBranch(currency1, branch1, true);
			AccBankAccount account2 = helper.GetNewBankAccountWithBranch(currency1, branch2, false);
			AccBankAccount account3 = helper.GetNewBankAccountWithBranch(currency2, null, true);
			AccBankAccount account4 = helper.GetNewBankAccountWithBranch(currency3, null, false);
			AccBankAccount account5 = helper.GetNewBankAccountWithBranch(currency4, branch3, false);
			AccBankAccount account6 = helper.GetNewBankAccountWithBranch(currency1, branch4, false);
			account5.AB_IsActive = false;

			Org.OH_IsCreditor = true;

			Org.CompanyData.OB_RX_NKAPDefltCurrency = currency1.RX_Code;
			Org.CompanyData.OB_GB_ControllingBranch = branch1.PK;
			AssertEquals("Default acct for this branch and currency exists", account1.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			Org.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			AssertEquals("No default acct for this branch and currency, but acct does exist", account2.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			Org.CompanyData.OB_RX_NKAPDefltCurrency = currency2.RX_Code;
			Org.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			AssertEquals("Default acct for this currency exists (but no branch)", account3.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			Org.CompanyData.OB_RX_NKAPDefltCurrency = currency3.RX_Code;
			Org.CompanyData.OB_GB_ControllingBranch = branch3.PK;
			AssertEquals("No default acct for this currency, but acct does exist", account4.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			Org.CompanyData.OB_RX_NKAPDefltCurrency = currency4.RX_Code;
			Org.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			Assert("Default bank account should not choose inactive bank account off the currency", Org.CompanyData.OB_AB_APDefaultBankAccount.IsEmpty);

			Org.CompanyData.OB_RX_NKAPDefltCurrency = null;
			Org.CompanyData.OB_GB_ControllingBranch = branch3.PK;
			Assert("Default bank account should not choose inactive bank account off the branch", Org.CompanyData.OB_AB_APDefaultBankAccount.IsEmpty);

			Org.CompanyData.OB_RX_NKAPDefltCurrency = null;
			Org.CompanyData.OB_GB_ControllingBranch = branch4.PK;
			AssertEquals("Default bank account should set to an account with a matching branch if possible", account6.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			Org.OH_IsCreditor = false;
			Org.CompanyData.OB_RX_NKAPDefltCurrency = currency1.RX_Code;
			Org.CompanyData.OB_GB_ControllingBranch = branch1.PK;
			AssertEquals("Default bank account should not be changed when the org payables flag is not set", account6.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);
		}

		public void TestSetDefaultAccountUsingLocalCurrency()
		{
			RefCurrency localCurrency = Factory.Load<RefCurrency>(GlbCompany.CurrentCompany.LocalCurrency.PK);
			RefCurrency currency1 = Factory.New<RefCurrency>();

			GlbBranch branch1 = Factory.New<GlbBranch>();
			GlbBranch branch2 = Factory.New<GlbBranch>();
			GlbBranch branch3 = Factory.New<GlbBranch>();

			MasterFilesTestHelper helper = new MasterFilesTestHelper(Factory);

			AccBankAccount account1 = helper.GetNewBankAccountWithBranch(localCurrency, branch1, true);
			AccBankAccount account2 = helper.GetNewBankAccountWithBranch(localCurrency, branch2, false);
			AccBankAccount account3 = helper.GetNewBankAccountWithBranch(localCurrency, null, true);

			Org.OH_IsCreditor = true;
			Org.CompanyData.OB_RX_NKAPDefltCurrency = currency1.RX_Code;
			Org.CompanyData.OB_GB_ControllingBranch = branch1.PK;
			AssertEquals("Default acct for this branch and local currency", account1.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			Org.CompanyData.OB_GB_ControllingBranch = branch2.PK;
			AssertEquals("No default acct for this branch and local currency, but acct does exist", account2.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			Org.CompanyData.OB_GB_ControllingBranch = branch3.PK;
			AssertEquals("Default acct for local currency exists (but no branch)", account3.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);

			account3.AB_IsDefaultReceiptBankAccount = false;
			Org.CompanyData.OB_GB_ControllingBranch = branch3.PK;
			AssertEquals("No default acct for local currency, but acct does exist", account3.PK, Org.CompanyData.OB_AB_APDefaultBankAccount);
		}

		public void TestValidateARGoodOwnership()
		{
			Org.CompanyData.OB_ARGoodsOwnership = "NVR";
			Assert("Valid value - AR Goods Ownership", !Org.CompanyData.OB_ARGoodsOwnershipInfo.HasErrors());

			Org.CompanyData.OB_ARGoodsOwnership = "XYZ";
			Assert("Invalid value - AR Goods Ownership", Org.CompanyData.OB_ARGoodsOwnershipInfo.HasErrors());
			Assert("Invalid error message - AR Goods Ownership", Org.CompanyData.OB_ARGoodsOwnershipInfo.HasError("Enter a valid selection."));
		}

		#endregion

		#region Credit On Hold

		[TestDate]
		public void TestGetLastUserSettingCreditOnHold()
		{
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			AssertEquals("Precondition", false, Data.OB_AROnCreditHold);

			SetCreditOnHoldViaUserLevel(Data, authorizingUser, 2);

			var actualResult = CreditOnHoldChecker.GetLastUserSettingLocalCreditOnHold(Data.Logs);

			AssertEquals(authorizingUser.GS_LoginName, actualResult.GS_LoginName);
			AssertEquals(authorizingUser.GS_FullName, actualResult.GS_FullName);
		}

		[TestDate]
		public void TestGetCreditOnHoldAuthorizationLevel()
		{
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			AssertEquals("Precondition", false, Data.OB_AROnCreditHold);

			var expectedDefaultAuthorizationLevel = 3;
			SetCreditOnHoldViaUserLevel(Data, authorizingUser, 0);
			AssertEquals("Default value for user without any rights", expectedDefaultAuthorizationLevel, Data.GetCreditOnHoldAuthorizationLevel());

			var expectedAuthorizationLevel = 2;
			SetCreditOnHoldViaUserLevel(Data, authorizingUser, expectedAuthorizationLevel);
			AssertEquals(expectedAuthorizationLevel, Data.GetCreditOnHoldAuthorizationLevel());

			expectedAuthorizationLevel = 3;
			SetCreditOnHoldViaUserLevel(Data, authorizingUser, expectedAuthorizationLevel);
			AssertEquals(expectedAuthorizationLevel, Data.GetCreditOnHoldAuthorizationLevel());

			expectedAuthorizationLevel = 1;
			SetCreditOnHoldViaUserLevel(Data, authorizingUser, expectedAuthorizationLevel);
			AssertEquals(expectedAuthorizationLevel, Data.GetCreditOnHoldAuthorizationLevel());

			var creditControlsModifiedEvents = Data.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));
			foreach (var creditControlsModifiedEvent in creditControlsModifiedEvents)
			{
				using (creditControlsModifiedEvent.LockForUpdatingKeyFieldsForTesting())
				{
					creditControlsModifiedEvent.SL_Reference = ZString.Empty;
				}
			}
			Factory.Save();
			AssertEquals("Default value when can't find user at all", expectedDefaultAuthorizationLevel, Data.GetCreditOnHoldAuthorizationLevel());
		}

		[TestDate]
		public void TestShouldCheckGroupAuthorisationLevelForOrganisation()
		{
			GlbStaff levelOneAuthorizingUser = levelOneAuthorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff authorizationUser = Factory.NewWithValidTestData<GlbStaff>();
			OrgHeader groupOrg = Factory.NewWithValidTestData<OrgHeader>();

			CreateUserAuthorisationLevelForOrganisationAndGroup(levelOneAuthorizingUser, authorizationUser, groupOrg);

			SetCreditOnHoldViaUserLevel(groupOrg.MiscServ, authorizationUser, 2, true);
			SetCreditOnHoldViaUserLevel(Data, levelOneAuthorizingUser, 1);

			AssertGreaterThan(Data.GetCreditOnHoldAuthorizationLevel(), 1);
		}

		[TestDate]
		public void TestGetGroupCreditOnHoldAuthorizationLevelForOrganisation()
		{
			GlbStaff levelOneAuthorizingUser = levelOneAuthorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff authorizationUser = Factory.NewWithValidTestData<GlbStaff>();
			OrgHeader groupOrg = Factory.NewWithValidTestData<OrgHeader>();

			CreateUserAuthorisationLevelForOrganisationAndGroup(levelOneAuthorizingUser, authorizationUser, groupOrg);

			var expectedAuthorizationLevel = 1;
			SetCreditOnHoldViaUserLevel(groupOrg.MiscServ, authorizationUser, expectedAuthorizationLevel, true);
			AssertEquals("Organisation Authorization User Level should equal to its Global Group's Level", expectedAuthorizationLevel, Data.GetCreditOnHoldAuthorizationLevel());

			SetCreditOnHoldViaUserLevel(groupOrg.MiscServ, authorizationUser, 2, true);
			SetCreditOnHoldViaUserLevel(Data, levelOneAuthorizingUser, 1);
			AssertEquals("Document delivery should be prevented and should be allowed to be overriden by L2 or above authority.", 2, Data.GetCreditOnHoldAuthorizationLevel());
		}

		void CreateUserAuthorisationLevelForOrganisationAndGroup(GlbStaff levelOneAuthorizingUser, GlbStaff authorizationUser, OrgHeader groupOrg)
		{
			levelOneAuthorizingUser.GS_LoginName = "Level1User";
			levelOneAuthorizingUser.GS_FullName = "Level 1 Authorization User";

			authorizationUser.GS_LoginName = "Uesr for Global Group";
			authorizationUser.GS_FullName = "Authorization User for Global Group";
			Factory.Save();

			Data.Header.MiscServ.OM_OH_ARGlobalCreditGroup = groupOrg.PK;
		}

		public void TestGetCreditOnHoldMaxLevel()
		{
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			AssertEquals("Default value for user without any rights", 0, Data.GetCreditOnHoldMaxLevel(new SecurityCore(null, authorizingUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK)));

			SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldFirstLevel, true);
			AssertEquals(1, Data.GetCreditOnHoldMaxLevel(new SecurityCore(null, authorizingUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK)));

			SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldSecondLevel, true);
			AssertEquals(2, Data.GetCreditOnHoldMaxLevel(new SecurityCore(null, authorizingUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK)));

			SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldThirdLevel, true);
			AssertEquals(3, Data.GetCreditOnHoldMaxLevel(new SecurityCore(null, authorizingUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK)));
		}

		internal static void SetCreditOnHoldViaUserLevel(EnterpriseBusinessObject orgData, GlbStaff user, int level, bool isGlobal = false)
		{
			Assert("Test requires the [TestDate] attribute", TestDateAttribute.IsActive);

			Func<StmALog[]> findLogs = () => orgData.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));

			var secondsIntervalForLogPosting = 10;
			var prevTestDate = TestDateAttribute.Date;
			var now = ZDateTime.Now.ToDateTime();
			using (new DisposableAction(() => TestDateAttribute.Date = prevTestDate))
			{
				if (TestDateAttribute.Date == DateTime.MinValue)
				{
					TestDateAttribute.Date = now;
				}

				var creditControlsModifiedEvents = findLogs();
				if (creditControlsModifiedEvents.Length != 0)
				{
					TestDateAttribute.Date = new[] { TestDateAttribute.Date, creditControlsModifiedEvents.Max(x => x.SL_PostedTimeUtc).ToDateTime() }.Max().AddSeconds(secondsIntervalForLogPosting);
				}

				SetSecurityForUser(user.PK, Env.Security.OrgCreditOnHoldFirstLevel, level == 1);
				SetSecurityForUser(user.PK, Env.Security.OrgCreditOnHoldSecondLevel, level == 2);
				SetSecurityForUser(user.PK, Env.Security.OrgCreditOnHoldThirdLevel, level == 3);

				if (isGlobal)
				{
					((OrgMiscServ)orgData).OM_ARGlobalOnCreditHold = false;
				}
				else
				{
					((OrgCompanyData)orgData).OB_AROnCreditHold = false;
				}

				orgData.Factory.Save();

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(secondsIntervalForLogPosting);

				var logsBefore = findLogs();
				using (Env.SetTemporaryUserContext(user.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					if (isGlobal)
					{
						((OrgMiscServ)orgData).OM_ARGlobalOnCreditHold = true;
						AssertNoErrors(((OrgMiscServ)orgData).Header.CompanyData);
					}
					else
					{
						((OrgCompanyData)orgData).OB_AROnCreditHold = true;
						AssertNoErrors(((OrgCompanyData)orgData).OB_AROnCreditHoldInfo);
					}

					orgData.Factory.Save();
				}
				var logsAfter = findLogs();
				AssertEquals("CreditControlsModified log must be added.", logsBefore.Length + 1, logsAfter.Length);
				var sortedLogsAfter = logsAfter.OrderBy(x => x.SL_PostedTimeUtc).Reverse().ToArray();
				if (logsBefore.Length > 0)
				{
					Assert("New log is the first one.", !logsBefore.Contains(sortedLogsAfter[0]));
					Assert("New log is posted with defined time shift.", (sortedLogsAfter[0].SL_PostedTimeUtc - sortedLogsAfter[1].SL_PostedTimeUtc).TotalSeconds >= secondsIntervalForLogPosting);
				}

				if (isGlobal)
				{
					Assert("New log is global credit on hold", sortedLogsAfter[0].SL_Reference.Contains("Global Credit On Hold"));
				}

				AssertEquals("SL_PostedTimeUtc has expected value.", TestDateAttribute.Date, sortedLogsAfter[0].SL_PostedTimeUtc);
			}
		}

		internal static void SetSecurityForUser(ZGuid userPK, SecurityCheckpoint checkPoint, bool isAllowed)
		{
			var securityFactory = new BusinessObjectFactory();
			var findQuery = new ZQuery(GlbSecuritySchema.GU_GB, GlbBranch.CurrentBranch.PK);
			findQuery.AddToFilter(GlbSecuritySchema.GU_GB, GlbBranch.CurrentBranch.PK);
			findQuery.AddToFilter(GlbSecuritySchema.GU_GE, GlbDepartment.CurrentDepartment.PK);
			findQuery.AddToFilter(GlbSecuritySchema.GU_GC, GlbCompany.CurrentCompany.PK);
			findQuery.AddToFilter(GlbSecuritySchema.GU_GS, userPK);
			findQuery.AddToFilter(GlbSecuritySchema.GU_SecurityRight, checkPoint.Code);

			var loginSecurity = securityFactory.LoadTop1<GlbSecurity>(findQuery);
			loginSecurity = loginSecurity ?? securityFactory.New<GlbSecurity>();
			loginSecurity.GU_GB = GlbBranch.CurrentBranch.PK;
			loginSecurity.GU_GE = GlbDepartment.CurrentDepartment.PK;
			loginSecurity.GU_GC = GlbCompany.CurrentCompany.PK;
			loginSecurity.GU_GS = userPK;
			loginSecurity.GU_SecurityRight = checkPoint.Code;
			loginSecurity.GU_SecurityItemIsAllowed = isAllowed;
			securityFactory.Save();
		}

		#endregion

		#region Default Values

		public void TestDefaults()
		{
			InvoiceRollupOrGroup defaults = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);

			defaults.GroupOrSubTotal = "SUB";
			defaults.GroupOrSubtotalStyle = "OFD";
			InvoiceRollupOrGroupCollection value = new InvoiceRollupOrGroupCollection();
			value.Add(defaults);
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCompanyData newCompanyData = org.CompanyData;
			AssertNotNull("Data not null", newCompanyData);

			AssertEquals("OB_GC", Env.CurrentCompany.PK, newCompanyData.OB_GC);

			AssertEquals("OB_RX_NKARDDefltCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, newCompanyData.OB_RX_NKARDDefltCurrency);

			AssertEquals("OB_OJ_ARDebtorGroup", ObjectFactory.Get<IAccounting>().ARAccountGroup, newCompanyData.OB_OJ_ARDebtorGroup);
			AssertEquals("OB_OG_ARCreditorGroup", ObjectFactory.Get<IAccounting>().APAccountGroup, newCompanyData.OB_OG_APCreditorGroup);
			AssertEquals("OB_ARCreditApproved", OrganisationsDataRegistry.Instance.DefaultARCreditApproved.Value, newCompanyData.OB_ARCreditApproved);

			AssertEquals("Company should have a new OrgInvoiceRollupOrGroup", 1, newCompanyData.InvoiceRollupOrGroups.Count);
			OrgInvoiceRollupOrGroup rollupOrGroup = newCompanyData.InvoiceRollupOrGroups[0];
			AssertEquals("Should be All JobTypes", OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code, rollupOrGroup.PG_JobType);
			AssertEquals("Should be All Modes", OrgConstants.ModesForGroupOrSubTotal.Codes.All, rollupOrGroup.PG_TransportMode);
			AssertEquals("Should be All Directions", OrgConstants.ServiceDirection.Code.All, rollupOrGroup.PG_ServiceDirection);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode, rollupOrGroup.PG_GroupOrSubTotal);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode, rollupOrGroup.PG_GroupOrSubtotalStyle);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode, rollupOrGroup.PG_InvoicePostingStyle);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode, rollupOrGroup.PG_InvoiceLineDisplayOption);

			newCompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals("Re-added on save", 1, newCompanyData.InvoiceRollupOrGroups.Count);
			rollupOrGroup = newCompanyData.InvoiceRollupOrGroups[0];
			AssertEquals("Should be All JobTypes", OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code, rollupOrGroup.PG_JobType);
			AssertEquals("Should be All Modes", OrgConstants.ModesForGroupOrSubTotal.Codes.All, rollupOrGroup.PG_TransportMode);
			AssertEquals("Should be All Directions", OrgConstants.ServiceDirection.Code.All, rollupOrGroup.PG_ServiceDirection);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode, rollupOrGroup.PG_GroupOrSubTotal);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode, rollupOrGroup.PG_GroupOrSubtotalStyle);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode, rollupOrGroup.PG_InvoicePostingStyle);
			AssertEquals("Should be DEF", OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode, rollupOrGroup.PG_InvoiceLineDisplayOption);

			// Test DefaultARCreditApproved and UseARSettlementGroupCreditLimit registry settings
			AssertEquals("DefaultARCreditApproved has default value", true, OrganisationsDataRegistry.Instance.DefaultARCreditApproved.Value);
			AssertEquals("UseARSettlementGroupCreditLimit has default value", false, OrganisationsDataRegistry.Instance.UseARSettlementGroupCreditLimit.Value);

			org = Factory.NewWithValidTestData<OrgHeader>();
			newCompanyData = org.CompanyData;
			AssertNotNull("Data not null", newCompanyData);

			AssertEquals("OB_ARCreditApproved", true, newCompanyData.OB_ARCreditApproved);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", false, newCompanyData.OB_ARUseSettlementGroupCreditLimit);

			AssertEquals("OB_ARUseSettlementGroupCreditLimit", "ALL", newCompanyData.ARTerms[0].PY_InvoiceClass);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", "COD", newCompanyData.ARTerms[0].PY_InvoiceTerm);

			OrganisationsDataRegistry.Instance.DefaultARCreditApproved.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			OrganisationsDataRegistry.Instance.UseARSettlementGroupCreditLimit.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			org = Factory.NewWithValidTestData<OrgHeader>();
			newCompanyData = org.CompanyData;
			AssertNotNull("Data not null", newCompanyData);

			AssertEquals("OB_ARCreditApproved", false, newCompanyData.OB_ARCreditApproved);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", true, newCompanyData.OB_ARUseSettlementGroupCreditLimit);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", "ALL", newCompanyData.ARTerms[0].PY_InvoiceClass);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", "DEF", newCompanyData.ARTerms[0].PY_InvoiceTerm);
		}

		#endregion

		#region OrgCompanyData Duplication

		public void TestMustNotDuplicateCompanyDataRecord()
		{
			var creationFactory = new BusinessObjectFactory();
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			creationFactory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var creationHeader = creationFactory.New<OrgHeader>();
			creationHeader.OH_Code = "XXXXXX";
			Assert("Pre-create address", creationHeader.Addresses.Count > 0);
			creationFactory.Save();

			var header1 = factory1.Load<OrgHeader>(creationHeader.PK);
			var header2 = factory2.Load<OrgHeader>(creationHeader.PK);

			header1.CompanyData.SetAPTaxApplicable(true);
			header2.CompanyData.SetAPTaxApplicable(true);

			using (GetFactoryIsolater(factory1))
			using (GetFactoryIsolater(factory2))
			{
				factory1.Save();
			}

			try
			{
				factory2.Save();
			}
			catch (Exception ex)
			{
				Assert("No notification before handler", UnitTestUserNotification.Instance.LastMessage.WasNone);
				ZExceptionReporting.HandleSaveException(ex);
				Assert("Notification shown after handler", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("While you were working, the organization XXXXXX had its AR/AP information updated. The system will now need to reload this information. Press OK to have this information loaded and then try saving again.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}

			AssertEquals("Company Data's should be the same", header1.CompanyData.PK, header2.CompanyData.PK);
		}

		#endregion

		#region TestDelete

		[ExpectNoExceptions]
		public void TestDelete()
		{
			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			var aRTaxCofing = Factory.NewWithValidTestData<AccTaxConfiguration>();
			aRTaxCofing.ETC_Ledger = "AR";
			Factory.Save();

			var aPTaxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			aPTaxConfig.ETC_Ledger = "AP";
			Factory.Save();

			foreach (GlbCompany company in companies)
			{
				OrgCompanyData companyData = Org.GetCompanyDataForGlbCompany(company);
				companyData.InvoiceTypes.AddNew();
				companyData.RateTariffLevels.AddNew();
				companyData.AccountDetailsCollection.AddNew();
				companyData.ARAccountDetailsCollection.AddNew();
				companyData.InvoiceRollupOrGroups.AddNew();

				var aROrgTaxConfig = companyData.AROrgTaxConfigurations.AddNew();
				aROrgTaxConfig.OTC_ETC = aRTaxCofing.PK;
				var aRorgTaxRate = aROrgTaxConfig.TaxRates.AddNew();
				aRorgTaxRate.OTR_Source = "MOV";
				aRorgTaxRate.OTR_StartDate = new ZDate(2023, 1, 1);
				aRorgTaxRate.OTR_EndDate = new ZDate(2023, 12, 1);

				var aPOrgTaxConfig = companyData.APOrgTaxConfigurations.AddNew();
				aPOrgTaxConfig.OTC_ETC = aPTaxConfig.PK;
				var aPOrgTaxRate = aPOrgTaxConfig.TaxRates.AddNew();
				aPOrgTaxRate.OTR_Source = "MOV";
				aPOrgTaxRate.OTR_StartDate = new ZDate(2023, 1, 1);
				aPOrgTaxRate.OTR_EndDate = new ZDate(2023, 12, 1);
			}
			Factory.Save();

			OrgHeader org2 = (new BusinessObjectFactory()).Load<OrgHeader>(Org.PK);
			org2.Delete();
			org2.Factory.RefreshEnabled = false;
			org2.Factory.Save();
		}

		public void TestDeleteIndependentCollectionsOnDeletingOrgCompanyData()
		{
			AssertEquals("Precondition: AccCFXConfigurations Collection is Empty", 0, Org.CompanyData.AccCFXConfigurations.Count);
			AssertEquals("Precondition: EInvoicingTemplateFileConfigurations Collection is Empty", 0, Org.CompanyData.EInvoicingTemplateFileConfigurations.Count);
			AssertEquals("Precondition: RateFeeChargeLevels Collection is Empty", 0, Org.CompanyData.RateFeeChargeLevels.Count);

			var templateFile = CreateNewTemplate("SHA");
			var configuration = CreateTESNewConfig("SHP", "AIR", "", templateFile.TFS_Code);
			Factory.Save();

			Org.CompanyData.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, Core.Constants.CurrencyCodes.Australia, 5m, 0.1m);
			Org.CompanyData.EInvoicingTemplateFileConfigurations.Load();
			Org.CompanyData.RateFeeChargeLevels.AddNew();
		
			AssertEquals("AccCFXConfigurations Collection has 1", 1, Org.CompanyData.AccCFXConfigurations.Count);
			AssertEquals("EInvoicingTemplateFileConfigurations Collection has 1", 1, Org.CompanyData.EInvoicingTemplateFileConfigurations.Count);
			AssertEquals("RateFeeChargeLevels Collection has 1", 1, Org.CompanyData.RateFeeChargeLevels.Count);
			Factory.Save();

			var companyData = Org.CompanyData;
			companyData.Delete();
			Factory.Save();
	
			AssertEquals(0, Factory.Load<AccCFXUpliftConfiguration>(new ZQuery
																		(new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_ParentTableCode, new ZString("OH")),
																		new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_ParentID, companyData.PK))).Length);

			AssertEquals(0, Factory.Load<AccEInvoicingTemplateFileView>(new ZQuery
																		(new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_ParentTableCode, new ZString("OH")),
																		 new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_ParentID, companyData.PK))).Length);

			AssertEquals(0, Factory.Load<OrgRateFeeChargeLevel>(new ZQuery(OrgRateFeeChargeLevelSchema.ORF_OH, companyData.PK)).Length);
		}

		#endregion

		#region Properties

		#region Payables (OB_IsCreditor)

		public void TestAirlineFieldsReadOnlyForNotAirline()
		{
			AssertEquals(true, Data.OB_APAirlineAccountNumberInfo.ReadOnly);

			Data.Header.OH_IsAirLine = true;
			AssertEquals(false, Data.OB_APAirlineAccountNumberInfo.ReadOnly);

			Data.Header.OH_IsAirLine = false;
			AssertEquals(true, Data.OB_APAirlineAccountNumberInfo.ReadOnly);
		}

		public void TestOB_APPaymentTerms()
		{
			Data.OB_AROnCreditHold = false;
			Data.OB_APPaymentTermDays = 5;
			Data.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals("OB_APPaymentTermDays", (short)5, Data.OB_APPaymentTermDays);
			Assert("OB_APPaymentTermDays not readonly", !Data.OB_APPaymentTermDaysInfo.ReadOnly);

			Data.OB_APPaymentTerms = Core.Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals("OB_APPaymentTermDays", (short)0, Data.OB_APPaymentTermDays);
			Assert("OB_APPaymentTermDays is readonly", Data.OB_APPaymentTermDaysInfo.ReadOnly);

			Data.OB_APPaymentTermDays = 15;
			Data.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromMonthEnd;
			AssertEquals("OB_APPaymentTermDays", (short)15, Data.OB_APPaymentTermDays);
			Assert("OB_APPaymentTermDays not readonly", !Data.OB_APPaymentTermDaysInfo.ReadOnly);

			Data.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			AssertEquals("OB_APPaymentTermDays", (short)0, Data.OB_APPaymentTermDays);
			Assert("OB_APPaymentTermDays is readonly", Data.OB_APPaymentTermDaysInfo.ReadOnly);
		}

		public void TestGetAPTerm()
		{
			Data.OB_AROnCreditHold = false;
			Data.OB_APPaymentTermDays = 5;
			Data.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromInvoiceDate;
			AssertEquals("GetAPTerm().Term", Core.Constants.InvoiceTerms.FromInvoiceDate, Data.GetAPTerm().Term);
			AssertEquals("GetAPTerm().Days", (short)5, Data.GetAPTerm().Days);
			AssertEquals("GetAPTermWithoutFallBack().Term", Core.Constants.InvoiceTerms.FromInvoiceDate, Data.GetAPTermWithoutFallback().Term);
			AssertEquals("GetAPTermWithoutFallBack().Days", (short)5, Data.GetAPTermWithoutFallback().Days);

			Data.OB_APPaymentTerms = Core.Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals("GetAPTerm().Term", Core.Constants.InvoiceTerms.CashOnDelivery, Data.GetAPTerm().Term);
			AssertEquals("GetAPTerm().Days", (short)0, Data.GetAPTerm().Days);
			AssertEquals("GetAPTermWithoutFallBack().Term", Core.Constants.InvoiceTerms.CashOnDelivery, Data.GetAPTermWithoutFallback().Term);
			AssertEquals("GetAPTermWithoutFallBack().Days", (short)0, Data.GetAPTermWithoutFallback().Days);

			Data.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			AssertEquals("GetAPTerm().Term", "", Data.GetAPTerm().Term);
			AssertEquals("GetAPTerm().Days", (short)0, Data.GetAPTerm().Days);
			AssertEquals("GetAPTermWithoutFallBack().Term", "DEF", Data.GetAPTermWithoutFallback().Term);
			AssertEquals("GetAPTermWithoutFallBack().Days", (short)0, Data.GetAPTermWithoutFallback().Days);

			Org.APSettlementGroupPK = Org.PK;
			AssertEquals("GetAPTerm().Term", "", Data.GetAPTerm().Term);
			AssertEquals("GetAPTerm().Days", (short)0, Data.GetAPTerm().Days);
			AssertEquals("GetAPTermWithoutFallBack().Term", "DEF", Data.GetAPTermWithoutFallback().Term);
			AssertEquals("GetAPTermWithoutFallBack().Days", (short)0, Data.GetAPTermWithoutFallback().Days);

			Org.APSettlementGroupPK = Factory.New<OrgHeader>().PK;
			Org.APSettlementGroup.APSettlementGroupPK = Org.PK;
			AssertEquals("Precondition: to exclude stack overflow cases.", Org.APSettlementGroup.APSettlementGroupPK, Org.PK);
			Org.APSettlementGroup.CompanyData.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromMonthEnd;
			Org.APSettlementGroup.CompanyData.OB_APPaymentTermDays = 7;
			AssertEquals("GetAPTerm().Term", Core.Constants.InvoiceTerms.FromMonthEnd, Data.GetAPTerm().Term);
			AssertEquals("GetAPTerm().Days", (short)7, Data.GetAPTerm().Days);
			AssertEquals("GetAPTermWithoutFallBack().Term", "DEF", Data.GetAPTermWithoutFallback().Term);
			AssertEquals("GetAPTermWithoutFallBack().Days", (short)0, Data.GetAPTermWithoutFallback().Days);

			Org.APSettlementGroup.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			AssertEquals("GetAPTerm().Term", "", Data.GetAPTerm().Term);
			AssertEquals("GetAPTerm().Days", (short)0, Data.GetAPTerm().Days);
			AssertEquals("GetAPTermWithoutFallBack().Term", "DEF", Data.GetAPTermWithoutFallback().Term);
			AssertEquals("GetAPTermWithoutFallBack().Days", (short)0, Data.GetAPTermWithoutFallback().Days);
		}

		public void TestOB_AROnCreditHold()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;
			onHoldTerms.TermDays = 7;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				OrgARTerms term1 = Data.ARTerms[0];
				term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
				term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
				OrgARTerms term2 = Data.ARTerms.AddNew();
				term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
				term2.PY_InvoiceDays = 4;
				Data.RunPreSaveValidation();
				Assert("ShouldValidateOnSave of OrgARTerms is false", !term1.ShouldValidateOnSave);
				Assert("ShouldValidateOnSave of OrgARTerms is false", !term2.ShouldValidateOnSave);
				Data.OB_AROnCreditHold = true;

				AssertEquals("PY_InvoiceTerm", "COD", term1.PY_InvoiceTerm);
				AssertEquals("PY_InvoiceDays", (short)0, term1.PY_InvoiceDays);
				AssertEquals("PY_InvoiceTerm", "INV", term2.PY_InvoiceTerm);
				AssertEquals("PY_InvoiceDays", (short)4, term2.PY_InvoiceDays);
				AssertEquals("ShouldValidateOnSave of OrgARTerms", false, term1.ShouldValidateOnSave);
				AssertEquals("ShouldValidateOnSave of OrgARTerms", false, term2.ShouldValidateOnSave);

				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), OrganisationRegistry.Instance.OnHoldTerms.Value.TermDays);
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestOB_ARCreditApproved()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				OrgARTerms term1 = Data.ARTerms[0];
				term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
				term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
				OrgARTerms term2 = Data.ARTerms.AddNew();
				term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
				term2.PY_InvoiceDays = 4;
				Data.RunPreSaveValidation();
				Assert("ShouldValidateOnSave of OrgARTerms is false", !term1.ShouldValidateOnSave);
				Assert("ShouldValidateOnSave of OrgARTerms is false", !term2.ShouldValidateOnSave);
				Data.OB_ARCreditApproved = false;

				AssertEquals("PY_InvoiceTerm", "COD", term1.PY_InvoiceTerm);
				AssertEquals("PY_InvoiceDays", (short)0, term1.PY_InvoiceDays);
				AssertEquals("PY_InvoiceTerm", "INV", term2.PY_InvoiceTerm);
				AssertEquals("PY_InvoiceDays", (short)4, term2.PY_InvoiceDays);
				AssertEquals("ShouldValidateOnSave of OrgARTerms", false, term1.ShouldValidateOnSave);
				AssertEquals("ShouldValidateOnSave of OrgARTerms", false, term2.ShouldValidateOnSave);

				InvoiceTerm mTHterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);
				AssertEquals(mTHterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestCreditPreApprovalAndOnHold()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				OrgARTerms term1 = Data.ARTerms[0];
				term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
				term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
				OrgARTerms term2 = Data.ARTerms.AddNew();
				term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
				term2.PY_InvoiceDays = 4;
				Data.RunPreSaveValidation();
				Assert("ShouldValidateOnSave of OrgARTerms is false", !term1.ShouldValidateOnSave);
				Assert("ShouldValidateOnSave of OrgARTerms is false", !term2.ShouldValidateOnSave);
				Data.OB_AROnCreditHold = true;
				Data.OB_ARCreditApproved = false;

				AssertEquals("PY_InvoiceTerm", "COD", term1.PY_InvoiceTerm);
				AssertEquals("PY_InvoiceDays", (short)0, term1.PY_InvoiceDays);
				AssertEquals("PY_InvoiceTerm", "INV", term2.PY_InvoiceTerm);
				AssertEquals("PY_InvoiceDays", (short)4, term2.PY_InvoiceDays);
				AssertEquals("ShouldValidateOnSave of OrgARTerms", false, term1.ShouldValidateOnSave);
				AssertEquals("ShouldValidateOnSave of OrgARTerms", false, term2.ShouldValidateOnSave);

				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), 0);
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestIsCreditApprovedAndNotOnHold()
		{
			Data.OB_ARCreditApproved = false;
			Data.OB_AROnCreditHold = false;
			AssertEquals("Not Approved, Not On Hold", false, Data.IsCreditApprovedAndNotOnHold);

			Data.OB_ARCreditApproved = true;
			Data.OB_AROnCreditHold = false;
			AssertEquals("Approved, Not On Hold", true, Data.IsCreditApprovedAndNotOnHold);

			Data.OB_ARCreditApproved = false;
			Data.OB_AROnCreditHold = false;
			AssertEquals("Not Approved, On Hold", false, Data.IsCreditApprovedAndNotOnHold);

			Data.OB_ARCreditApproved = true;
			Data.OB_AROnCreditHold = true;
			AssertEquals("Approved, On Hold", false, Data.IsCreditApprovedAndNotOnHold);
		}

		#endregion

		#region Receivables (OB_IsDebtor)

		public void TestOB_ARCreditCardExpire_Month_Year()
		{
			Data.OB_ARCreditCardExpire = "1209";
			AssertEquals("12", Data.OB_ARCreditCardExpire_Month);
			AssertEquals("09", Data.OB_ARCreditCardExpire_Year);

			Data.OB_ARCreditCardExpire_Month = "02";
			AssertEquals("0209", Data.OB_ARCreditCardExpire);
			AssertEquals("02", Data.OB_ARCreditCardExpire_Month);

			Data.OB_ARCreditCardExpire_Year = "10";
			AssertEquals("0210", Data.OB_ARCreditCardExpire);
			AssertEquals("10", Data.OB_ARCreditCardExpire_Year);
		}

		#region Cash Advance

		public void TestAccARCashAdvanceConfigurationsNotNullAndCorrectLevel()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			AssertNotNull(orgHeader.CompanyData.AccARCashAdvanceConfigurations);
			AssertEquals(AccCashAdvanceDefaultingLevel.Debtor, orgHeader.CompanyData.AccARCashAdvanceConfigurations.Level);
		}

		#endregion

		#region CFX

		public void TestAccCFXUpliftConfigurationsNotNullAndCorrectLevel()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			AssertNotNull(orgHeader.CompanyData.AccCFXConfigurations);
			AssertEquals(AccCFXConfigurationLevelEnum.Organisation, orgHeader.CompanyData.AccCFXConfigurations.Level);
		}

		public void TestAccARExRateConfigsNotNullAndCorrectLevel()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			AssertNotNull(orgHeader.CompanyData.AccARExchangeRateConfigurations);
			AssertEquals(AccExRateConfigurationLevelEnum.Debtor, orgHeader.CompanyData.AccARExchangeRateConfigurations.Level);
		}

		public void TestAccAPExRateConfigsNotNullAndCorrectLevel()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			AssertNotNull(orgHeader.CompanyData.AccAPExchangeRateConfigurations);
			AssertEquals(AccExRateConfigurationLevelEnum.Creditor, orgHeader.CompanyData.AccAPExchangeRateConfigurations.Level);
		}

		public void TestARExportSeaCollectUpliftReturnsProperValue()
		{
			TestUpliftPropertyReturnsCorrectValues("SEA", "EXP", () => GetCompanyDataUplifts("SEA", "EXP"));
		}

		public void TestARExportAirCollectUpliftReturnsProperValue()
		{
			TestUpliftPropertyReturnsCorrectValues("AIR", "EXP", () => GetCompanyDataUplifts("AIR", "EXP"));
		}

		public void TestARImportSeaCollectUpliftReturnsProperValue()
		{
			TestUpliftPropertyReturnsCorrectValues("SEA", "IMP", () => GetCompanyDataUplifts("SEA", "IMP"));
		}

		public void TestARImportAirCollectUpliftReturnsProperValue()
		{
			TestUpliftPropertyReturnsCorrectValues("AIR", "IMP", () => GetCompanyDataUplifts("AIR", "IMP"));
		}

		(ZDecimal percent, ZDecimal minimum) GetCompanyDataUplifts(ZString transportMode, ZString serviceDirection)
		{
			var uplift = Org.CompanyData.AccCFXConfigurations.GetRecord("ALL", serviceDirection, transportMode);

			return (uplift?.JCF_CFXPercentage ?? -1m, uplift?.JCF_CFXMinimum ?? -1m);
		}

		void TestUpliftPropertyReturnsCorrectValues(ZString transportMode, ZString serviceDirection, Func<(ZDecimal percent, ZDecimal minimum)> getCfxUpliftAndMin)
		{
			Func<AccCFXUpliftConfiguration, bool> cfxValuesAreFromTheExpectedRecord = (expectedRecord) =>
				getCfxUpliftAndMin().Equals((expectedRecord.JCF_CFXPercentage, expectedRecord.JCF_CFXMinimum));

			var accConfigGC = CreateNewConfig(transportMode, serviceDirection, "", 67, 43);
			Org.CompanyData.AccCFXConfigurations.Load();
			Factory.Save();
			Assert(cfxValuesAreFromTheExpectedRecord(accConfigGC));

			var accConfigGB = CreateNewConfig(transportMode, serviceDirection, "GB", 71, 34);
			Factory.Save();
			Org.CompanyData.AccCFXConfigurations.Load();
			Assert(cfxValuesAreFromTheExpectedRecord(accConfigGB));

			var accConfigOH = CreateNewConfig(transportMode, serviceDirection, "OH", 63, 11);
			Factory.Save();
			Org.CompanyData.AccCFXConfigurations.Load();
			Assert(cfxValuesAreFromTheExpectedRecord(accConfigOH));
		}

		AccCFXUpliftConfiguration CreateNewConfig(ZString transportMode, ZString serviceDirection, string level, ZDecimal percent, ZDecimal minimum)
		{
			var accCFXUpliftConfig = Factory.New<AccCFXUpliftConfiguration>();

			accCFXUpliftConfig.JCF_ServiceDirection = serviceDirection;
			accCFXUpliftConfig.JCF_TransportMode = transportMode;
			accCFXUpliftConfig.JCF_ConfigType = "CFX";
			accCFXUpliftConfig.JCF_JobType = "ALL";
			accCFXUpliftConfig.JCF_GC = Org.CompanyData.OB_GC;
			accCFXUpliftConfig.JCF_RX_NKCurrency = string.Empty;
			accCFXUpliftConfig.JCF_Ledger = "AR";
			accCFXUpliftConfig.JCF_CFXPercentage = percent;
			accCFXUpliftConfig.JCF_CFXMinimum = minimum;

			switch (level)
			{
				case OrgHeaderSchema.Constants.Prefix:
					{
						accCFXUpliftConfig.JCF_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
						accCFXUpliftConfig.JCF_ParentID = Org.CompanyData.OB_OH;
						break;
					}
				case GlbBranchSchema.Constants.Prefix:
					{
						accCFXUpliftConfig.JCF_ParentTableCode = GlbBranchSchema.Constants.Prefix;
						accCFXUpliftConfig.JCF_ParentID = Env.CurrentBranchPK;
						break;
					}
				case "":
					{
						accCFXUpliftConfig.JCF_ParentTableCode = string.Empty;
						accCFXUpliftConfig.JCF_ParentID = ZGuid.Empty;
						break;
					}
				default:
					throw new ArgumentException("Unsupported config level code", nameof(level));
			}

			return accCFXUpliftConfig;
		}

		public void TestEInvoiceTemplateCLLSEACollectUpliftReturnsProperValue()
		{
			Org.CompanyData.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);
			TestEInvoicingConfigurationPropertyReturnsCorrectValues("CLL", "SEA", "CLS", () => GetCompanyDataEInvoicingTemplateConfigs("CLL", "SEA", "CLS"));
		}

		public void TestEInvoicingConfigurationPropertyReturnsCorrectValues()
		{
			Org.CompanyData.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);
			TestEInvoicingConfigurationPropertyReturnsCorrectValues("CTO", "ROA", "CTR", () => GetCompanyDataEInvoicingTemplateConfigs("CTO", "ROA", "CTR"));
		}

		public void TestEInvoiceTemplateSHPAIRCollectUpliftReturnsProperValue()
		{
			Org.CompanyData.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);
			TestEInvoicingConfigurationPropertyReturnsCorrectValues("SHP", "AIR", "SHA", () => GetCompanyDataEInvoicingTemplateConfigs("SHP", "AIR", "SHA"));
		}

		public void TestEInvoiceTemplateSHPRAICollectUpliftReturnsProperValue()
		{
			Org.CompanyData.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);
			TestEInvoicingConfigurationPropertyReturnsCorrectValues("SHP", "RAI", "SHR", () => GetCompanyDataEInvoicingTemplateConfigs("SHP", "RAI", "SHR"));
		}

		public void TestEInvoicingTemplateFileConfigurationsNotNullAndCorrectLevel()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey);

			AssertNotNull(orgHeader.CompanyData.EInvoicingTemplateFileConfigurations);
			AssertEquals(AccEInvoicingTemplateFileLevelEnum.Organisation, orgHeader.CompanyData.EInvoicingTemplateFileConfigurations.Level);
		}

		ZString GetCompanyDataEInvoicingTemplateConfigs(ZString jobType, ZString transportMode, ZString templateCode)
		{
			var config = Org.CompanyData.EInvoicingTemplateFileConfigurations.GetRecord(jobType, "ALL", transportMode, templateCode);

			return config?.ETF_TemplateCode ?? ZString.Empty;
		}

		void TestEInvoicingConfigurationPropertyReturnsCorrectValues(ZString jobType, ZString transportMode, ZString templateCode, Func<ZString> getEInvoiceTemplateConfig)
		{
			Func<AccEInvoicingTemplateFileView, bool> templateCodeFromTheExpectedRecord = (expectedRecord) =>
				getEInvoiceTemplateConfig().Equals(expectedRecord.ETF_TemplateCode);

			var templateFile = CreateNewTemplate(templateCode);

			var accConfigGC = CreateTESNewConfig(jobType, transportMode, "", templateFile.TFS_Code);
			Org.CompanyData.EInvoicingTemplateFileConfigurations.Load();
			Factory.Save();
			Assert(templateCodeFromTheExpectedRecord(accConfigGC));

			var accConfigOH = CreateTESNewConfig(jobType, transportMode, "OH", templateFile.TFS_Code);
			Factory.Save();
			Org.CompanyData.EInvoicingTemplateFileConfigurations.Load();
			Assert(templateCodeFromTheExpectedRecord(accConfigOH));
		}

		AccTemplateFileStorage CreateNewTemplate(ZString templateCode)
		{
			var templateFile = Factory.New<AccTemplateFileStorage>();
			templateFile.TFS_Code = templateCode;
			templateFile.TFS_Description = "TEST";
			templateFile.TFS_FileData = new byte[] { 1, 2, 3, 4 };
			templateFile.TFS_FileName = "TestFile";
			templateFile.TFS_GC = Org.CompanyData.OB_GC;
			templateFile.TFS_IsActive = true;
			templateFile.TFS_Ledger = LedgerTypes.AccountsReceivable;

			return templateFile;
		}
		AccEInvoicingTemplateFileView CreateTESNewConfig(ZString jobType, ZString transportMode, string level, ZString templateCode)
		{
			var configuration = Factory.New<AccEInvoicingTemplateFileView>();

			configuration.ETF_GC = Org.CompanyData.OB_GC;
			configuration.ETF_JobType = jobType;
			configuration.ETF_ServiceDirection = "ALL";
			configuration.ETF_TransportMode = transportMode;
			configuration.ETF_ConfigType = "TES";
			configuration.ETF_Ledger = LedgerTypes.AccountsReceivable;
			configuration.ETF_TemplateCode = templateCode;

			switch (level)
			{
				case OrgHeaderSchema.Constants.Prefix:
					{
						configuration.ETF_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
						configuration.ETF_ParentID = Org.CompanyData.OB_OH;
						break;
					}
				case "":
					{
						configuration.ETF_ParentTableCode = string.Empty;
						configuration.ETF_ParentID = ZGuid.Empty;
						break;
					}
				default:
					throw new ArgumentException("Unsupported config level code", nameof(level));
			}

			return configuration;
		}

		#endregion

		#region SetPayToAccountUsingDebtorGroup

		public void TestSetPayToAccountUsingDebtorGroup()
		{
			AccBankAccount bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount1.AB_Code = "Bank1";
			AccBankAccount bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount2.AB_Code = "Bank2";
			Factory.Save();

			OrgDebtorGroup group = Factory.New<OrgDebtorGroup>();
			group.OJ_Code = "XXX";
			group.OJ_Desc = "XXX";
			group.DefaultBankAccountPK = bankAccount1.PK;
			Factory.Save();

			OrgHeader testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.OH_IsDebtor = true;

			AssertEquals("No Debtor Group Set", ZGuid.Empty, testHeader.CompanyData.OB_OJ_ARDebtorGroup);
			AssertEquals("Pay To Account Empty", ZGuid.Empty, testHeader.CompanyData.OB_AB_ARPayToAccount);

			// Assign Account Group
			testHeader.CompanyData.OB_OJ_ARDebtorGroup = group.PK;

			AssertEquals("Debtor Group Set", group.PK, testHeader.CompanyData.OB_OJ_ARDebtorGroup);
			AssertEquals("ARBankAccountToDisplay should be empty if OverrideBankAccountFromDebtorGroup false", ZGuid.Empty, testHeader.CompanyData.ARBankAccountToDisplay);

			// Tick Override Pay To Bank Account
			testHeader.CompanyData.OverrideBankAccountFromDebtorGroup = true;

			AssertEquals("ARBankAccountToDisplayInfo should not be readonly", false, testHeader.CompanyData.ARBankAccountToDisplayInfo.ReadOnly);
			AssertEquals("ARBankAccountToDisplay should be empty", ZGuid.Empty, testHeader.CompanyData.ARBankAccountToDisplay);

			// Set Pay To Bank Account for Org
			testHeader.CompanyData.OB_AB_ARPayToAccount = bankAccount2.PK;

			AssertEquals("ARBankAccountToDisplay should be bank 2", bankAccount2.PK, testHeader.CompanyData.ARBankAccountToDisplay);

			testHeader.CompanyData.OverrideBankAccountFromDebtorGroup = false;

			AssertEquals("AR Pay to account should be empty", ZGuid.Empty, testHeader.CompanyData.OB_AB_ARPayToAccount);
			AssertEquals("ARBankAccountToDisplay should be empty if OverrideBankAccountFromDebtorGroup false", ZGuid.Empty, testHeader.CompanyData.ARBankAccountToDisplay);
			AssertEquals("ARBankAccountToDisplayInfo should be read only", true, testHeader.CompanyData.ARBankAccountToDisplayInfo.ReadOnly);
		}

		public void TestLookupsForARBankAccountToDisplay()
		{
			var list = MetaData.GetListDataSource(Data, Data.ARBankAccountToDisplayInfo.PropertyDescriptor);
			AssertEquals("Lookup List Type", typeof(AccBankAccountCollection), list.GetType());
		}

		#endregion

		#region OverCreditLimit

		public void TestOverCreditLimit()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = header.CompanyData;
			companyData.OB_IsDebtor = true;
			companyData.OB_ARCreditLimit = 1000;
			Factory.Save();

			companyData.IncreaseOutstandingBalance(1000);
			Assert("Should check if outstanding balance is over temp credit limit", !companyData.OverCreditLimit);

			companyData.IncreaseOutstandingBalance(1);
			companyData.Header.GetType().GetField("globalCreditGroupChilds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(companyData.Header, null);
			Assert("Should check if outstanding balance is over temp credit limit", companyData.OverCreditLimit);
		}

		#endregion

		#endregion

		public void TestOB_APExternalCreditorCode()
		{
			Assert(Data.OB_APExternalCreditorCode.IsEmpty);

			Org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, "EDR AU");
			Org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, "ECR FR", RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.France));
			Org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, "ECR AU");

			Data.OB_APExternalCreditorCode = "My custom ECR";
			AssertEquals(Data.OB_APExternalCreditorCode, "My custom ECR");

			Data.OB_APExternalCreditorCode = "";
			AssertEquals(Data.OB_APExternalCreditorCode, "ECR AU");
		}

		public void TestOB_ARExternalDebtorCode()
		{
			Assert(Data.OB_ARExternalDebtorCode.IsEmpty);

			Org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, "ECR AU");
			Org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, "EDR FR", RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.France));
			Org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, "EDR AU");

			Data.OB_ARExternalDebtorCode = "My custom EDR";
			AssertEquals(Data.OB_ARExternalDebtorCode, "My custom EDR");

			Data.OB_ARExternalDebtorCode = "";
			AssertEquals(Data.OB_ARExternalDebtorCode, "EDR AU");
		}

		public void TestIsWhsSplitMonthBilling()
		{
			Data.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			AssertEquals(true, Data.IsWhsSplitMonthBilling);

			Data.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			AssertEquals(false, Data.IsWhsSplitMonthBilling);

			Data.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			AssertEquals(true, Data.IsWhsSplitMonthBilling);
		}

		public void TestOB_NoCreditLimitSetLabelIsVisible()
		{
			Data.OB_ARCreditLimit = 123.01;
			Assert("OB_NoCreditLimitSetLabelIsVisible should not be visible", !Data.OB_NoCreditLimitSetLabelIsVisible);
			Data.OB_ARCreditLimit = 0;
			Assert("OB_NoCreditLimitSetLabelIsVisible should be visible", Data.OB_NoCreditLimitSetLabelIsVisible);
		}

		public void TestOB_OnARGlobalCreditHoldLabelIsVisible()
		{
			AssertEquals("OB_OnARGlobalCreditHoldLabelIsVisible should not be visible", false, Data.OB_OnARGlobalCreditHoldLabelIsVisible);
			Data.Header.MiscServ.OM_ARGlobalOnCreditHold = true;
			AssertEquals("OB_OnARGlobalCreditHoldLabelIsVisible should be visible", true, Data.OB_OnARGlobalCreditHoldLabelIsVisible);

			Data.Header.MiscServ.OM_ARGlobalOnCreditHold = false;
			AssertEquals("OB_OnARGlobalCreditHoldLabelIsVisible should not be visible", false, Data.OB_OnARGlobalCreditHoldLabelIsVisible);

			var groupOrg = Factory.New<OrgHeader>();
			groupOrg.MiscServ.OM_ARGlobalOnCreditHold = true;
			Data.Organisation.MiscServ.OM_OH_ARGlobalCreditGroup = groupOrg.PK;

			AssertEquals("OB_OnARGlobalCreditHoldLabelIsVisible should be visible because its Global Group is On Hold", true, Data.OB_OnARGlobalCreditHoldLabelIsVisible);
		}

		public void TestOB_GlobalCreditApproved()
		{
			AssertEquals("OB_ARGlobalCreditApprovedLabelIsVisible should return true", true, Data.OB_ARGlobalCreditApprovedLabelIsVisible);
			Data.Header.MiscServ.OM_ARGlobalCreditApproved = true;
			AssertEquals("OB_ARGlobalCreditApprovedLabelIsVisible should return false", false, Data.OB_ARGlobalCreditApprovedLabelIsVisible);

			Data.Header.MiscServ.OM_ARGlobalCreditApproved = false;
			AssertEquals("OB_ARGlobalCreditApprovedLabelIsVisible should return true", true, Data.OB_ARGlobalCreditApprovedLabelIsVisible);

			var groupOrg = Factory.New<OrgHeader>();
			groupOrg.MiscServ.OM_ARGlobalCreditApproved = true;
			Data.Organisation.MiscServ.OM_OH_ARGlobalCreditGroup = groupOrg.PK;
			AssertEquals("OB_ARGlobalCreditApprovedLabelIsVisible should return false because its Global Group is Approved", false, Data.OB_ARGlobalCreditApprovedLabelIsVisible);
		}

		public void TestOB_ARCreditLimitNotApprovedLabelIsVisible()
		{
			Data.OB_ARCreditApproved = false;
			AssertEquals(true, Data.OB_ARCreditLimitNotApprovedLabelIsVisible);

			Data.OB_ARCreditApproved = true;
			AssertEquals(false, Data.OB_ARCreditLimitNotApprovedLabelIsVisible);
		}

		public void TestBuyersConsolInvoicingStyle()
		{
			string originalRegValue = OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.Value;
			try
			{
				OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "MAS");
				OrgHeader org = Factory.New<OrgHeader>();
				AssertEquals("DEF", org.CompanyData.OB_ARBuyersConsolInvoicingStyle);
				AssertEquals("MAS", org.CompanyData.EffectiveBuyersConsolInvoicingStyle);

				OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "APP");
				AssertEquals("DEF", org.CompanyData.OB_ARBuyersConsolInvoicingStyle);
				AssertEquals("APP", org.CompanyData.EffectiveBuyersConsolInvoicingStyle);

				org.CompanyData.OB_ARBuyersConsolInvoicingStyle = "MAS";
				AssertEquals("MAS", org.CompanyData.OB_ARBuyersConsolInvoicingStyle);
				AssertEquals("MAS", org.CompanyData.EffectiveBuyersConsolInvoicingStyle);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegValue);
			}
		}

		public void TestOrganisationAndCode()
		{
			OrgCompanyData testCompanyData = Factory.New<OrgCompanyData>();
			testCompanyData.OB_OH = Org.PK;
			AssertEquals("Should be the same organisation", Org, testCompanyData.Organisation);
			AssertEquals("Codes should be the same", Org.OH_Code, testCompanyData.Code);
		}

		[ExpectNoExceptions]
		public void TestSetIsDebtorWithoutHeader()
		{
			OrgCompanyDataForTest org = Factory.New<OrgCompanyDataForTest>();
			org.OB_IsDebtor = true;
			org.OB_IsDebtor = false;
			org.OB_IsDebtor = true;
		}

		public void TestSetARTaxApplicable()
		{
			Org.OH_Code = "ABCXYZ";
			Org.MainAddress.OA_Address1 = "Address 1";
			Org.OH_RL_NKClosestPort = "NZAKL";
			Org.CompanyData.OB_IsCreditor = true;
			Org.CompanyData.OB_IsDebtor = true;
			AssertEquals("DEF", Org.CompanyData.OB_ARVATConfig);

			Assert(Org.CompanyData.IsARTaxApplicable);
			Org.CompanyData.SetARTaxApplicable(false);
			AssertEquals("should always be NON when isApplicable is false", "NON", Org.CompanyData.OB_ARVATConfig);

			Assert(!Org.CompanyData.IsARTaxApplicable);
			Org.CompanyData.SetARTaxApplicable(true);
			AssertEquals("Should read from registry when isApplicable is true", "DEF", Org.CompanyData.OB_ARVATConfig);
		}

		public void TestSetAPTaxApplicable()
		{
			Org.ShowMessage += new OrgHeader.ShowMessageEventHandler(Org_ShowMessage);
			Org.OH_Code = "ABCXYZ";
			Org.MainAddress.OA_Address1 = "Address 1";
			Org.OH_RL_NKClosestPort = "NZAKL";
			Org.CompanyData.OB_IsCreditor = true;
			Org.CompanyData.OB_IsDebtor = true;
			AssertEquals("NON", Org.CompanyData.OB_APVATConfig);
			Org.CompanyData.OB_APVATConfig = "DEF";

			Assert(Org.CompanyData.IsAPTaxApplicable);
			Org.CompanyData.SetAPTaxApplicable(false);
			AssertEquals("should always be NON when isApplicable is false", "NON", Org.CompanyData.OB_APVATConfig);

			Assert(!Org.CompanyData.IsAPTaxApplicable);
			Org.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
			AssertEquals("should always be DEF when isApplicable is true and doNotUseRegistry is true", "DEF", Org.CompanyData.OB_APVATConfig);
			Org.CompanyData.OB_APVATConfig = "NON";
			Org.CompanyData.SetAPTaxApplicable(true);
			AssertEquals("Should read from registry when doNotUseRegistry is false", "NON", Org.CompanyData.OB_APVATConfig);
		}

		public void TestARAPTaxApplicable()
		{
			Org.ShowMessage += new OrgHeader.ShowMessageEventHandler(Org_ShowMessage);
			Org.OH_Code = "ABCXYZ";
			Org.MainAddress.OA_Address1 = "Address 1";
			Org.CompanyData.OB_IsCreditor = true;
			Org.CompanyData.OB_IsDebtor = true;

			Org.CompanyData.SetARTaxApplicable(true);
			AssertNull("No Message Shown - Org Not Saved", LastShowMessage);

			Org.CompanyData.SetAPTaxApplicable(true);
			AssertNull("No Message Shown - Org Not Saved", LastShowMessage);

			Factory.Save();

			Org.CompanyData.OB_IsCreditor = false;
			Org.CompanyData.OB_IsDebtor = false;

			Org.CompanyData.SetARTaxApplicable(true);
			AssertNull("No Message Shown - Not Debtor", LastShowMessage);

			Org.CompanyData.SetAPTaxApplicable(true);
			AssertNull("No Message Shown - Not Creditor", LastShowMessage);

			Org.CompanyData.OB_IsCreditor = true;
			Org.CompanyData.OB_IsDebtor = true;
			Org.CompanyData.SetARTaxApplicable(false);
			Org.CompanyData.SetAPTaxApplicable(false);
			Org.CompanyData.SetARTaxApplicable(true);
			AssertEquals("Changing Debtor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change.", LastShowMessage);
			Org.CompanyData.SetAPTaxApplicable(true);
			AssertEquals("Changing Creditor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Creditor will NOT change.", LastShowMessage);

			LastShowMessage = "";
			Org.CompanyData.SetARTaxApplicable(true);
			AssertEquals("", LastShowMessage);
			Org.CompanyData.SetAPTaxApplicable(true);
			AssertEquals("", LastShowMessage);

			Org.CompanyData.SetARTaxApplicable(false);
			AssertEquals("Changing Debtor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change.", LastShowMessage);
			Org.CompanyData.SetAPTaxApplicable(false);
			AssertEquals("Changing Creditor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Creditor will NOT change.", LastShowMessage);
		}

		void Org_ShowMessage(string caption, string message)
		{
			LastShowMessage = caption + "|" + message;
		}

		string LastShowMessage;

		public void TestGetWarehouseRatingPeriod()
		{
			Org.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			AssertEquals(Constants.StorageCalculationPeriods.Monthly, Org.CompanyData.GetWarehouseRatingPeriod());

			Org.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Default;
			AssertEquals(Constants.StorageCalculationPeriods.Weekly, Org.CompanyData.GetWarehouseRatingPeriod());

			Org.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Daily;
			AssertEquals(Constants.StorageCalculationPeriods.Daily, Org.CompanyData.GetWarehouseRatingPeriod());
		}

		public void TestOB_WhsClientFreeStorageDays()
		{
			int oldFreeDays = RatingDataRegistry.Instance.WarehouseClientFreeStorageDays.Value;
			try
			{
				RatingDataRegistry.Instance.WarehouseClientFreeStorageDays.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 10);
				AssertEquals(10, (int)Org.CompanyData.OB_WhsClientFreeStorageDays);
				Assert(Org.CompanyData.OB_WhsClientFreeStorageDaysInfo.ReadOnly);
				Assert(!Org.CompanyData.OB_WhsOverrideFreeStorage);

				Org.CompanyData.OB_WhsClientFreeStorageDays = 5;
				AssertEquals(5, (int)Org.CompanyData.OB_WhsClientFreeStorageDays);
				Assert(!Org.CompanyData.OB_WhsClientFreeStorageDaysInfo.ReadOnly);
				Assert(Org.CompanyData.OB_WhsOverrideFreeStorage);

				Org.CompanyData.OB_WhsOverrideFreeStorage = false;
				AssertEquals(10, (int)Org.CompanyData.OB_WhsClientFreeStorageDays);
				Assert(Org.CompanyData.OB_WhsClientFreeStorageDaysInfo.ReadOnly);
				Assert(!Org.CompanyData.OB_WhsOverrideFreeStorage);

				Org.CompanyData.OB_WhsOverrideFreeStorage = true;
				AssertEquals(5, (int)Org.CompanyData.OB_WhsClientFreeStorageDays);
				Assert(!Org.CompanyData.OB_WhsClientFreeStorageDaysInfo.ReadOnly);
				Assert(Org.CompanyData.OB_WhsOverrideFreeStorage);
			}
			finally
			{
				RatingDataRegistry.Instance.WarehouseClientFreeStorageDays.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldFreeDays);
			}
		}

		public void TestWhsAutoCreateAndRatePeriodicInvoice_AutoSetRelatedValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = org.CompanyData;

			AssertEquals("Precondition", false, companyData.OB_WhsAutoCreateAndRatePeriodicInvoice);
			AssertEquals("Precondition", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			AssertEquals("Precondition", false, companyData.OB_WhsAutoPostPeriodicInvoice);
			AssertEquals("Precondition", false, companyData.OB_WhsAutoDeliverPeriodicInvoice);
			AssertEquals("Precondition", false, companyData.OB_WhsAutoCreateAndRatePeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Precondition", true, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo.ReadOnly);
			AssertEquals("Precondition", true, companyData.OB_WhsAutoPostPeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Precondition", true, companyData.OB_WhsAutoDeliverPeriodicInvoiceInfo.ReadOnly);

			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			AssertEquals("Should set the value.", true, companyData.OB_WhsAutoCreateAndRatePeriodicInvoice);
			AssertEquals("Should not change this value)", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			AssertEquals("Should change this value", true, companyData.OB_WhsAutoPostPeriodicInvoice);
			AssertEquals("Should change this value", true, companyData.OB_WhsAutoDeliverPeriodicInvoice);
			AssertEquals("Always enable.", false, companyData.OB_WhsAutoCreateAndRatePeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Should make it not readonly", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo.ReadOnly);
			AssertEquals("Should make it not readonly", false, companyData.OB_WhsAutoPostPeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Should make it not readonly", false, companyData.OB_WhsAutoDeliverPeriodicInvoiceInfo.ReadOnly);
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = false;

			AssertEquals("Should set the value.", false, companyData.OB_WhsAutoCreateAndRatePeriodicInvoice);
			AssertEquals("Should change this value.", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			AssertEquals("Should change this value.", false, companyData.OB_WhsAutoPostPeriodicInvoice);
			AssertEquals("Should change this value.", false, companyData.OB_WhsAutoDeliverPeriodicInvoice);
			AssertEquals("Always enable.", false, companyData.OB_WhsAutoCreateAndRatePeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Should make it readonly.", true, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo.ReadOnly);
			AssertEquals("Should make it readonly.", true, companyData.OB_WhsAutoPostPeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Should make it readonly.", true, companyData.OB_WhsAutoDeliverPeriodicInvoiceInfo.ReadOnly);
		}

		public void TestWhsAutoPostPeriodicInvoice_AutoSetRelatedValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = org.CompanyData;
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			companyData.OB_WhsAutoPostPeriodicInvoice = false;

			AssertEquals("Precondition", false, companyData.OB_WhsAutoPostPeriodicInvoice);
			AssertEquals("Precondition", false, companyData.OB_WhsAutoDeliverPeriodicInvoice);
			AssertEquals("Precondition", false, companyData.OB_WhsAutoPostPeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Precondition", true, companyData.OB_WhsAutoDeliverPeriodicInvoiceInfo.ReadOnly);

			companyData.OB_WhsAutoPostPeriodicInvoice = true;

			AssertEquals("Should set the value.", true, companyData.OB_WhsAutoPostPeriodicInvoice);
			AssertEquals("Should change value", true, companyData.OB_WhsAutoDeliverPeriodicInvoice);
			AssertEquals("Should not change readonly", false, companyData.OB_WhsAutoPostPeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Should make it not readonly", false, companyData.OB_WhsAutoDeliverPeriodicInvoiceInfo.ReadOnly);

			companyData.OB_WhsAutoPostPeriodicInvoice = false;

			AssertEquals("Should set the value.", false, companyData.OB_WhsAutoPostPeriodicInvoice);
			AssertEquals("Should change value.", false, companyData.OB_WhsAutoDeliverPeriodicInvoice);
			AssertEquals("Should not change readonly", false, companyData.OB_WhsAutoPostPeriodicInvoiceInfo.ReadOnly);
			AssertEquals("Should make it readonly.", true, companyData.OB_WhsAutoDeliverPeriodicInvoiceInfo.ReadOnly);
		}

		public void TestWhsAllowCreateInvoiceWithNoTransactionsOrStock_AutoSetRelatedValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = org.CompanyData;
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = false;

			AssertEquals("Precondition", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			AssertEquals("Precondition", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo.ReadOnly);

			companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = true;

			AssertEquals("Should set the value.", true, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			AssertEquals("Should not change readonly.", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo.ReadOnly);

			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = false;

			AssertEquals("Should set the value.", false, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock);
			AssertEquals("Should not change readonly.", true, companyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStockInfo.ReadOnly);
		}

		public void TestOB_ARUseSettlementGroupCreditLimit()
		{
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(Data, 1000m, 1000m);
			Data.OB_ARUseSettlementGroupCreditLimit = true;
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", true, Data.OB_ARUseSettlementGroupCreditLimit);
			AssertEquals("OM_ARCreditLimit", 1000m, Data.Header.MiscServ.OM_ARCreditLimit);
			AssertEquals("OB_ARTemporaryCreditLimitIncrease", 1000m, Data.OB_ARTemporaryCreditLimitIncrease);

			Data.OB_ARUseSettlementGroupCreditLimit = false;
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", false, Data.OB_ARUseSettlementGroupCreditLimit);
			AssertEquals("OM_ARCreditLimit", 1000m, Data.Header.MiscServ.OM_ARCreditLimit);
			AssertEquals("OB_ARTemporaryCreditLimitIncrease", 1000m, Data.OB_ARTemporaryCreditLimitIncrease);

			Data.ARSettlementGroupPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Data.OB_ARUseSettlementGroupCreditLimit = true;
			AssertEquals("OB_ARUseSettlementGroupCreditLimit", true, Data.OB_ARUseSettlementGroupCreditLimit);
			AssertEquals("OM_ARCreditLimit", 0m, Data.Header.MiscServ.OM_ARCreditLimit);
			AssertEquals("OB_ARTemporaryCreditLimitIncrease", 0m, Data.OB_ARTemporaryCreditLimitIncrease);
		}

		public void TestOB_ARUseSettlementGroupCreditLimit_ReadOnly()
		{
			AssertEquals("OB_ARUseSettlementGroupCreditLimit_ReadOnly", false, Data.OB_ARUseSettlementGroupCreditLimitInfo.ReadOnly);
		}

		public void TestOB_ARUseSettlementGroupCreditLimit_Resets()
		{
			Data.ARSettlementGroupPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Data.OB_ARUseSettlementGroupCreditLimit = true;
			AssertEquals("OB_ARUseSettlementGroupCreditLimit_ReadOnly", false, Data.OB_ARUseSettlementGroupCreditLimitInfo.ReadOnly);
			AssertNotEquals("Data.ARSettlementGroupPK", ZGuid.Empty, Data.ARSettlementGroupPK);
			Assert("Data.OB_ARUseSettlementGroupCreditLimit Is True", Data.OB_ARUseSettlementGroupCreditLimit);

			Data.ARSettlementGroupPK = ZGuid.Empty;
			AssertEquals(false, Data.OB_ARUseSettlementGroupCreditLimit);
			AssertEquals("OB_ARUseSettlementGroupCreditLimit_ReadOnly", false, Data.OB_ARUseSettlementGroupCreditLimitInfo.ReadOnly);
			AssertEquals("Data.ARSettlementGroupPK", ZGuid.Empty, Data.ARSettlementGroupPK);
		}

		public void TestOB_ARCreditLimit_ReadOnly()
		{
			var originalOrgReceivablesModifyCreditControl = Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;
			var originalOrgReceivablesModifyPaymentTerms = Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Data.OB_ARCreditApproved = true;

				Data.OB_ARUseSettlementGroupCreditLimit = false;
				AssertEquals("Everything is allowed, UseSettlementGroup is not ticked, Credit is approved", false, Data.OB_ARCreditLimitInfo.ReadOnly);

				Data.OB_ARUseSettlementGroupCreditLimit = true;
				AssertEquals("Everything is allowed, UseSettlementGroup is ticked, Credit is approved", true, Data.OB_ARCreditLimitInfo.ReadOnly);

				Data.OB_ARUseSettlementGroupCreditLimit = false;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				AssertEquals("Modify Payment Terms is allowed, Modify Credit Control is not allowed, UseSettlementGroup is not ticked, Credit is approved", true, Data.OB_ARCreditLimitInfo.ReadOnly);

				Data.OB_ARCreditApproved = false;
				AssertEquals("Modify Payment Terms is allowed, Modify Credit Control is not allowed, UseSettlementGroup is not ticked, Credit is not approved", false, Data.OB_ARCreditLimitInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				AssertEquals("Modify Payment Terms is not allowed, Modify Credit Control is not allowed, UseSettlementGroup is not ticked, Credit is not approved", true, Data.OB_ARCreditLimitInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				AssertEquals("Modify Payment Terms is allowed, Modify Credit Control is not allowed, UseSettlementGroup is not ticked, Credit is not approved", false, Data.OB_ARCreditLimitInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = originalOrgReceivablesModifyCreditControl;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = originalOrgReceivablesModifyPaymentTerms;
			}
		}

		public void TestOB_ARTemporaryCreditLimitIncrease_ReadOnly()
		{
			var originalOrgReceivablesModifyCreditControl = Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Data.OB_ARUseSettlementGroupCreditLimit = true;
				AssertEquals(true, Data.OB_ARTemporaryCreditLimitIncreaseInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Data.OB_ARUseSettlementGroupCreditLimit = true;
				AssertEquals(true, Data.OB_ARTemporaryCreditLimitIncreaseInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Data.OB_ARUseSettlementGroupCreditLimit = false;
				AssertEquals(false, Data.OB_ARTemporaryCreditLimitIncreaseInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Data.OB_ARUseSettlementGroupCreditLimit = false;
				AssertEquals(true, Data.OB_ARTemporaryCreditLimitIncreaseInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = originalOrgReceivablesModifyCreditControl;
			}
		}

		public void TestOB_ARCreditAgreedPaymentMethod_ReadOnly()
		{
			var originalOrgReceivablesModifyCreditControl = Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				AssertEquals(true, Data.OB_ARCreditAgreedPaymentMethodInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				AssertEquals(false, Data.OB_ARCreditAgreedPaymentMethodInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = originalOrgReceivablesModifyCreditControl;
			}
		}

		#endregion

		#region IDocManagerSupport

		public void TestDocManagerCode()
		{
			AssertEquals("Code should be CSO. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "CSO", ((IDocManagerSupport)Org.CompanyData).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region IReadOnlySecurity

		public void TestRateFeeChargeLevelsIsReadOnly()
		{
			bool oldRatingAndTariffModify = Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed;
			bool oldDetailModify = Env.Security.OrgDetailsModify.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				Env.Security.OrgDetailsModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.RateFeeChargeLevels.ReadOnly);

				Env.Security.OrgDetailsModify.IsAllowed = false;
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				ResetOrgInDB();
				Assert("Acces Not Allowed - ReadOnly", OrgInDB.CompanyData.RateFeeChargeLevels.ReadOnly);

				Env.Security.OrgDetailsModify.IsAllowed = true;
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
				ResetOrgInDB();
				Assert("Acces Not Allowed - ReadOnly", OrgInDB.CompanyData.RateFeeChargeLevels.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModify.IsAllowed = oldDetailModify;
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = oldRatingAndTariffModify;
			}
		}

		public void TestInvoiceRollupGroupReadOnly()
		{
			bool oldChargeGroupingValue = Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed;
			OrgInDB.CompanyData.OB_IsCreditor = ZBool.True;
			OrgInDB.Factory.Save();
			Assert("CompanyData is saved", OrgInDB.CompanyData.IsInDatabase);

			try
			{
				Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = true;
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.InvoiceRollupOrGroups.ReadOnly);

				Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - Readonly", OrgInDB.CompanyData.InvoiceRollupOrGroups.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = oldChargeGroupingValue;
			}
		}

		public void TestARTermsReadOnly()
		{
			bool oldPaymentTermsSecurity = Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed;
			bool oldCreditControlSecurity = Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;
			OrgInDB.CompanyData.OB_IsDebtor = ZBool.True;
			OrgInDB.Factory.Save();
			Assert("CompanyData is saved", OrgInDB.CompanyData.IsInDatabase);

			OrgHeader newOrg = Factory.New<OrgHeader>();
			try
			{
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.ARTerms.ReadOnly);
				Assert("New Org Access Allowed - Not Readonly", !newOrg.CompanyData.ARTerms.ReadOnly);

				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				ResetOrgInDB();
				newOrg = Factory.New<OrgHeader>();
				Assert("Access Disallowed - Readonly", OrgInDB.CompanyData.ARTerms.ReadOnly);
				Assert("New Org Access Disallowed - Readonly", newOrg.CompanyData.ARTerms.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				ResetOrgInDB();
				newOrg = Factory.New<OrgHeader>();
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.ARTerms.ReadOnly);
				Assert("New Org Access Allowed - Not Readonly", !newOrg.CompanyData.ARTerms.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				ResetOrgInDB();
				newOrg = Factory.New<OrgHeader>();
				newOrg.CompanyData.OB_IsDebtor = ZBool.True;
				OrgInDB.CompanyData.OB_ARCreditApproved = true;
				Assert("Access Disallowed - Readonly", OrgInDB.CompanyData.ARTerms.ReadOnly);
				Assert("New Org Access Disallowed - Readonly", newOrg.CompanyData.ARTerms.ReadOnly);

				ResetOrgInDB();
				newOrg = Factory.New<OrgHeader>();
				OrgInDB.CompanyData.OB_ARCreditApproved = false;
				newOrg.CompanyData.OB_ARCreditApproved = false;
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.ARTerms.ReadOnly);
				Assert("New Org Access Allowed - Not Readonly", !newOrg.CompanyData.ARTerms.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = oldPaymentTermsSecurity;
			}
		}

		public void TestInvoiceTpesReadOnly()
		{
			bool oldInvoiceBatchingValue = Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = true;
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.InvoiceTypes.ReadOnly);

				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - Readonly", OrgInDB.CompanyData.InvoiceTypes.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = oldInvoiceBatchingValue;
			}
		}

		public void TestRateTariffLevelsCollectionReadOnly()
		{
			bool oldRateTariffValue = Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.RateTariffLevels.ReadOnly);

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - Readonly", OrgInDB.CompanyData.RateTariffLevels.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = oldRateTariffValue;
			}
		}

		public void TestRateSecurityGroupReadOnly()
		{
			bool oldRateTariffValue = Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.OB_RateSecurityGroupInfo.ReadOnly);

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - Readonly", OrgInDB.CompanyData.OB_RateSecurityGroupInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = oldRateTariffValue;
			}
		}

		public void TestAPAccountDetailsCollectionIsReadOnly()
		{
			bool oldAPValue = Env.Security.OrgPayablesAccountDetailsModify.IsAllowed;

			try
			{
				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.AccountDetailsCollection.ReadOnly);

				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.CompanyData.AccountDetailsCollection.ReadOnly);
			}
			finally
			{
				Env.Security.OrgPayablesAccountDetailsModify.IsAllowed = oldAPValue;
			}
		}

		public void TestAutoUpdateRatesReadOnly()
		{
			bool oldRateTariffValue = Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				Assert("Access Allowed - Not Readonly", !OrgInDB.CompanyData.OB_ARAutoUpdateRatesInfo.ReadOnly);

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - Readonly", OrgInDB.CompanyData.OB_ARAutoUpdateRatesInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = oldRateTariffValue;
			}
		}

		public void TestAPOrgTaxConfigurationTemplate_IsReadOnly_WhenOrgPayablesModifyIsNotAllowed()
		{
			Env.Security.OrgPayablesModify.IsAllowed = false;

			Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.OB_OCT_APTaxTemplateInfo.ReadOnly);

			Env.Security.OrgPayablesModifyTaxConfigurationTemplate.IsAllowed = false;
			ResetOrgInDB();
			Assert("Access Disallowed - ReadOnly", OrgInDB.CompanyData.OB_OCT_APTaxTemplateInfo.ReadOnly);
		}

		public void TestAROrgTaxConfigurationTemplate_IsReadOnly_WhenOrgReceivablesModifyIsNotAllowed()
		{
			Env.Security.OrgReceivablesModify.IsAllowed = false;

			Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.OB_OCT_ARTaxTemplateInfo.ReadOnly);

			Env.Security.OrgReceivablesModifyTaxConfigurationTemplate.IsAllowed = false;
			ResetOrgInDB();
			Assert("Access Disallowed - ReadOnly", OrgInDB.CompanyData.OB_OCT_ARTaxTemplateInfo.ReadOnly);
		}

		public void TestARAPReadOnlySecurityMembers()
		{
			bool oldAPValue = Env.Security.OrgPayablesModify.IsAllowed;
			bool oldARConfigValue = Env.Security.OrgReceivablesModifyConfig.IsAllowed;
			bool oldARInvoicingValue = Env.Security.OrgReceivablesModifyInvoicing.IsAllowed;
			bool oldCNRValue = Env.Security.OrgConsignorModifyDetails.IsAllowed;
			bool oldCNEValue = Env.Security.OrgConsigneeModifyDetails.IsAllowed;
			bool oldWHSValue = Env.Security.OrgWarehouseModify.IsAllowed;
			bool oldCarrierValue = Env.Security.OrgCarrierModify.IsAllowed;
			bool oldAccountDetailsValue = Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed;
			bool oldCreditControlValue = Env.Security.OrgReceivablesModifyCreditControl.IsAllowed;
			bool oldCurrencyUpliftValue = Env.Security.OrgReceivablesModifyCurrencyUplift.IsAllowed;
			bool oldTaxDetailsValue = Env.Security.OrgReceivablesModifyTaxDetails.IsAllowed;
			bool oldQualityAssurancValue = Env.Security.OrgReceivablesModifyQualityAssurance.IsAllowed;
			bool oldInvoicingValue = Env.Security.OrgReceivablesModifyInvoicing.IsAllowed;
			bool oldPaymentTermsValue = Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed;
			bool oldInvoiceDetailsValue = Env.Security.OrgReceivablesModifyInvoiceDetails.IsAllowed;
			bool oldBuyersConsolInvoicingValue = Env.Security.OrgReceivablesModifyBuyersConsolInvoicing.IsAllowed;
			bool oldExternalDebtorValue = Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed;
			bool oldExternalCreditorValue = Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed;

			OrgInDB.CompanyData.OverrideBankAccountFromDebtorGroup = ZBool.True;
			OrgInDB.OH_IsAirLine = true;
			OrgInDB.OH_IsDebtor = true;

			try
			{
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_OJ_ARDebtorGroupInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARCategoryInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARConsolidatedAccountingCategoryInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OverrideBankAccountFromDebtorGroupInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.ARBankAccountToDisplayInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_RX_NKARDDefltCurrencyInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARCreditRatingInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARCreditLimitInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARCreditApprovedInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_AROnCreditHoldInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARAccountAndCreditReviewDueInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARVATConfigInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARWHTApplicableInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARDontShowTaxOnDocsInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARVATSplitPaymentApplicableInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARQualityAssuredInfo.ReadOnly);
				Assert("Access Allowed AR Config - Not Readonly", !OrgInDB.CompanyData.OB_ARQualityAssuredCheckedDateInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARReceiptInvoiceAfterPostingDefaultInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARCombinedStatementInvoiceInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARWarehouseRatingPeriodInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARWhsStorageCalcMethodInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARBuyersConsolInvoicingStyleInfo.ReadOnly);
				Assert("Access Allowed AP - Not ReadOnly", !OrgInDB.CompanyData.OB_APAirlineAccountNumberInfo.ReadOnly);
				Assert("Access Allowed AP - Not ReadOnly", !OrgInDB.CompanyData.OB_AC_APDefaultChargeCodeInfo.ReadOnly);
				Assert("Access Allowed Warehouse - Not Readonly", !OrgInDB.CompanyData.OB_IMUsedBondedWhsInfo.ReadOnly);
				Assert("Access Allowed Carrier - Not ReadOnly", !OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);
				Assert("Access Allowed External Debtor - Not Readonly", !OrgInDB.CompanyData.OB_ARExternalDebtorCodeInfo.ReadOnly);
				Assert("Access Allowed External Creditor - Not ReadOnly", !OrgInDB.CompanyData.OB_APExternalCreditorCodeInfo.ReadOnly);
				Assert("Access Allowed AR Goods Ownership - Not ReadOnly", !OrgInDB.CompanyData.OB_ARGoodsOwnershipInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARExternalDebtorCodeInfo.ReadOnly);

				Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_APExternalCreditorCodeInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_OJ_ARDebtorGroupInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARCategoryInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_RX_NKARDDefltCurrencyInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OverrideBankAccountFromDebtorGroupInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARCreditRatingInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARCreditLimitInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARCreditApprovedInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_AROnCreditHoldInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARAccountAndCreditReviewDueInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyTaxDetails.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARVATConfigInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARWHTApplicableInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARDontShowTaxOnDocsInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARVATSplitPaymentApplicableInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARGoodsOwnershipInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyQualityAssurance.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARQualityAssuredInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARQualityAssuredCheckedDateInfo.ReadOnly);

				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARReceiptInvoiceAfterPostingDefaultInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARCombinedStatementInvoiceInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARWarehouseRatingPeriodInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARWhsStorageCalcMethodInfo.ReadOnly);
				Assert("Access Allowed AR Invoicing - Not Readonly", !OrgInDB.CompanyData.OB_ARBuyersConsolInvoicingStyleInfo.ReadOnly);
				Assert("Access Allowed AP - Not ReadOnly", !OrgInDB.CompanyData.OB_APAirlineAccountNumberInfo.ReadOnly);
				Assert("Access Allowed AP - Not ReadOnly", !OrgInDB.CompanyData.OB_AC_APDefaultChargeCodeInfo.ReadOnly);
				Assert("Access Allowed Warehouse - Not Readonly", !OrgInDB.CompanyData.OB_IMUsedBondedWhsInfo.ReadOnly);
				Assert("Access Allowed Carrier - Not ReadOnly", !OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);

				OrgHeader newOrg = Factory.New<OrgHeader>();
				newOrg.CompanyData.OB_ARCreditApproved = false;
				OrgInDB.CompanyData.OB_ARCreditApproved = false;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access NOT Allowed - ReadOnly", newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("Access Allowed - NOT ReadOnly", !OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access Allowed - NOT ReadOnly", !newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access NOT Allowed - ReadOnly", newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Assert("Access Allowed - NOT ReadOnly", !OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access Allowed - NOT ReadOnly", !newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("Access Allowed - NOT ReadOnly", !OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access Allowed - NOT ReadOnly", !newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);

				OrgInDB.CompanyData.OB_ARCreditApproved = true;
				newOrg.CompanyData.OB_ARCreditApproved = true;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access NOT Allowed - ReadOnly", newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = true;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access NOT Allowed - ReadOnly", newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = true;
				Assert("Access Allowed - NOT ReadOnly", !OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access Allowed - NOT ReadOnly", !newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = false;
				Assert("Access Allowed - NOT ReadOnly", !OrgInDB.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);
				Assert("New Org Access Allowed - NOT ReadOnly", !newOrg.CompanyData.OB_ARTreatDisbursementsAsStandardValueInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyInvoiceDetails.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARReceiptInvoiceAfterPostingDefaultInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARCombinedStatementInvoiceInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyBuyersConsolInvoicing.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARBuyersConsolInvoicingStyleInfo.ReadOnly);

				Assert("Access Allowed Warehouse - Not ReadOnly", !OrgInDB.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Allowed Warehouse - Not ReadOnly", !OrgInDB.CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Allowed Warehouse - Not ReadOnly", !OrgInDB.CompanyData.OB_ARWarehouseRatingPeriodInfo.ReadOnly);
				Assert("Access Allowed Warehouse - Not ReadOnly", !OrgInDB.CompanyData.OB_ARWhsStorageCalcMethodInfo.ReadOnly);
				Assert("Access Allowed AP - Not ReadOnly", !OrgInDB.CompanyData.OB_APAirlineAccountNumberInfo.ReadOnly);
				Assert("Access Allowed AP - Not ReadOnly", !OrgInDB.CompanyData.OB_AC_APDefaultChargeCodeInfo.ReadOnly);
				Assert("Access Allowed Warehouse - Not Readonly", !OrgInDB.CompanyData.OB_IMUsedBondedWhsInfo.ReadOnly);
				Assert("Access Allowed Carrier - Not ReadOnly", !OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);

				Env.Security.OrgPayablesModify.IsAllowed = false;
				Assert("Access Allowed Warehouse - Not Readonly", !OrgInDB.CompanyData.OB_IMUsedBondedWhsInfo.ReadOnly);
				Assert("Access Allowed Carrier - Not ReadOnly", !OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);

				Env.Security.OrgConsigneeModifyDetails.IsAllowed = false;
				Assert("Access Allowed Warehouse - Not Readonly", !OrgInDB.CompanyData.OB_IMUsedBondedWhsInfo.ReadOnly);
				Assert("Access Allowed Carrier - Not ReadOnly", !OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);

				Env.Security.OrgWarehouseModify.IsAllowed = false;
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_IMUsedBondedWhsInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoiceInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARWarehouseRatingPeriodInfo.ReadOnly);
				Assert("Access Denied - Readonly", OrgInDB.CompanyData.OB_ARWhsStorageCalcMethodInfo.ReadOnly);

				Env.Security.OrgCarrierModifySea.IsAllowed = false;
				Assert("Access Denied Carrier - ReadOnly", OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyConfig.IsAllowed = oldARConfigValue;
				Env.Security.OrgReceivablesModifyInvoicing.IsAllowed = oldARInvoicingValue;
				Env.Security.OrgPayablesModify.IsAllowed = oldAPValue;
				Env.Security.OrgConsignorModifyDetails.IsAllowed = oldCNRValue;
				Env.Security.OrgConsigneeModifyDetails.IsAllowed = oldCNEValue;
				Env.Security.OrgWarehouseModify.IsAllowed = oldWHSValue;
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierValue;
				Env.Security.OrgReceivablesModifyAccountDetails.IsAllowed = oldAccountDetailsValue;
				Env.Security.OrgReceivablesModifyCreditControl.IsAllowed = oldCreditControlValue;
				Env.Security.OrgReceivablesModifyCurrencyUplift.IsAllowed = oldCurrencyUpliftValue;
				Env.Security.OrgReceivablesModifyTaxDetails.IsAllowed = oldTaxDetailsValue;
				Env.Security.OrgReceivablesModifyQualityAssurance.IsAllowed = oldQualityAssurancValue;
				Env.Security.OrgReceivablesModifyInvoicing.IsAllowed = oldInvoicingValue;
				Env.Security.OrgReceivablesModifyPaymentTerms.IsAllowed = oldPaymentTermsValue;
				Env.Security.OrgReceivablesModifyInvoiceDetails.IsAllowed = oldInvoiceDetailsValue;
				Env.Security.OrgReceivablesModifyBuyersConsolInvoicing.IsAllowed = oldBuyersConsolInvoicingValue;
				Env.Security.OrgReceivablesModifyExternalDebtor.IsAllowed = oldExternalDebtorValue;
				Env.Security.OrgPayablesModifyExternalCreditor.IsAllowed = oldExternalCreditorValue;
			}
		}

		public void TestIsCreditorFlagForNewOrg()
		{
			bool oldAPValue = Env.Security.OrgPayablesModify.IsAllowed;
			bool oldOrgTypeAPValue = Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed;
			bool oldOrgTypeAPTempValue = Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed;

			try
			{
				Env.Security.OrgPayablesModify.IsAllowed = false;
				Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = true;
				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = true;

				Org.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsCreditorInfo.ReadOnly);

				Org.OH_IsTempAccount = ZBool.False;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsCreditorInfo.ReadOnly);

				Env.Security.OrgDetailsNewOrgTypeFlagAP.IsAllowed = false;
				Assert("Access NOT Allowed for non temp - ReadOnly", Org.CompanyData.OB_IsCreditorInfo.ReadOnly);

				Org = OrgHeader.New(Factory);
				Org.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed for temp - Not ReadOnly", !Org.CompanyData.OB_IsCreditorInfo.ReadOnly);

				Env.Security.OrgDetailsNewOrgTypeTempAPFlag.IsAllowed = false;
				Assert("Access NOT Allowed for temp - ReadOnly", Org.CompanyData.OB_IsCreditorInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgPayablesModify.IsAllowed = oldAPValue;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = oldOrgTypeAPValue;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = oldOrgTypeAPTempValue;
			}
		}

		public void TestIsCreditorFlagForExistingOrg()
		{
			bool oldAPValue = Env.Security.OrgPayablesModify.IsAllowed;
			bool oldOrgTypeAPValue = Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed;
			bool oldOrgTypeAPTempValue = Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed;

			try
			{
				Env.Security.OrgPayablesModify.IsAllowed = false;
				Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = true;
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = true;

				OrgInDB.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsCreditorInfo.ReadOnly);

				OrgInDB.OH_IsTempAccount = ZBool.False;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsCreditorInfo.ReadOnly);

				Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = false;
				Assert("Access NOT Allowed for non temp - ReadOnly", OrgInDB.CompanyData.OB_IsCreditorInfo.ReadOnly);

				ResetOrgInDB();
				OrgInDB.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed for temp - Not ReadOnly", !OrgInDB.CompanyData.OB_IsCreditorInfo.ReadOnly);

				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = false;
				Assert("Access NOT Allowed for temp - ReadOnly", OrgInDB.CompanyData.OB_IsCreditorInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgPayablesModify.IsAllowed = oldAPValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagAP.IsAllowed = oldOrgTypeAPValue;
				Env.Security.OrgDetailsModifyOrgTypeTempAPFlag.IsAllowed = oldOrgTypeAPTempValue;
			}
		}

		public void TestIsDebtorFlagForNewOrg()
		{
			bool oldARValue = Env.Security.OrgReceivablesModify.IsAllowed;
			bool oldOrgTypeARValue = Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed;
			bool oldOrgTypeARTempValue = Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModify.IsAllowed = false;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = true;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = true;

				Org.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsDebtorInfo.ReadOnly);

				Org.OH_IsTempAccount = ZBool.False;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsDebtorInfo.ReadOnly);

				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = false;
				Assert("Access NOT Allowed for non temp - ReadOnly", Org.CompanyData.OB_IsDebtorInfo.ReadOnly);

				Org = OrgHeader.New(Factory);
				Org.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed for temp - Not ReadOnly", !Org.CompanyData.OB_IsDebtorInfo.ReadOnly);

				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = false;
				Assert("Access NOT Allowed for temp - ReadOnly", Org.CompanyData.OB_IsDebtorInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModify.IsAllowed = oldARValue;
				Env.Security.OrgDetailsNewOrgTypeFlagAR.IsAllowed = oldOrgTypeARValue;
				Env.Security.OrgDetailsNewOrgTypeTempARFlag.IsAllowed = oldOrgTypeARTempValue;
			}
		}

		public void TestIsDebtorFlagForExistingOrg()
		{
			bool oldARValue = Env.Security.OrgReceivablesModify.IsAllowed;
			bool oldOrgTypeARValue = Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed;
			bool oldOrgTypeARTempValue = Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModify.IsAllowed = false;
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = true;
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = true;

				OrgInDB.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsDebtorInfo.ReadOnly);

				OrgInDB.OH_IsTempAccount = ZBool.False;
				Assert("Access Allowed - Not ReadOnly", !Org.CompanyData.OB_IsDebtorInfo.ReadOnly);

				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = false;
				Assert("Access NOT Allowed for non temp - ReadOnly", OrgInDB.CompanyData.OB_IsDebtorInfo.ReadOnly);

				ResetOrgInDB();
				OrgInDB.OH_IsTempAccount = ZBool.True;
				Assert("Access Allowed for temp - Not ReadOnly", !OrgInDB.CompanyData.OB_IsDebtorInfo.ReadOnly);

				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = false;
				Assert("Access NOT Allowed for temp - ReadOnly", OrgInDB.CompanyData.OB_IsDebtorInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModify.IsAllowed = oldARValue;
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = oldOrgTypeARValue;
				Env.Security.OrgDetailsModifyOrgTypeTempARFlag.IsAllowed = oldOrgTypeARTempValue;
			}
		}

		public void TestReadOnlyBranch()
		{
			bool oldARValue = Env.Security.OrgReceivablesModify.IsAllowed;
			bool oldAPValue = Env.Security.OrgPayablesModify.IsAllowed;
			bool oldDetailsModifyValue = Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed;

			try
			{
				Env.Security.OrgReceivablesModify.IsAllowed = false;
				Env.Security.OrgPayablesModify.IsAllowed = false;
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.CompanyData.OB_GB_ControllingBranchInfo.ReadOnly);

				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.CompanyData.OB_GB_ControllingBranchInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModify.IsAllowed = oldARValue;
				Env.Security.OrgPayablesModify.IsAllowed = oldAPValue;
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = oldDetailsModifyValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region InvoiceRollupOrGroups

		public void TestInvoiceRollupOrGroups()
		{
			OrgInvoiceRollupOrGroup invoiceRollupOrGroup = Factory.New<OrgInvoiceRollupOrGroup>();
			invoiceRollupOrGroup.PG_OB = Data.PK;

			AssertEquals("Should be one OrgInvoiceRollupOrGroup in the Collection", 1, Data.InvoiceRollupOrGroups.Count);
			OrgInvoiceRollupOrGroupCollection collection = Data.InvoiceRollupOrGroups;
			AssertSame("Collection was not lazy loaded", collection, Data.InvoiceRollupOrGroups);
		}

		#endregion

		#region Tax Details

		public void TestSetTaxApplicable_Defaults()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "YY";
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_RN_NKCountryCode = country.Code;
			uNLOCO.RL_Code = "LOCO1";

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			Assert("Precondition: OM_ARTaxApplicable is false", !Data.IsARTaxApplicable);
			Assert("Precondition: OM_ARWHTApplicable is false", !Data.OB_ARWHTApplicable);

			Data.OB_IsDebtor = true;
			Assert("OM_ARTaxApplicable should be true", Data.IsARTaxApplicable);
			Assert("OM_ARWHTApplicable should be true", !Data.OB_ARWHTApplicable);

			Assert("Precondition: OM_APTaxApplicable is false", !Data.IsAPTaxApplicable);
			Assert("Precondition: OM_APWHTApplicable is false", !Data.OB_APWHTApplicable);

			Data.Organisation.OH_RL_NKClosestPort = uNLOCO.RL_Code;
			uNLOCO.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Data.OB_IsCreditor = true;
			Assert("OM_APTaxApplicable should be true, same countries, Org GST registered", Data.IsAPTaxApplicable);
			Assert("OM_APWHTApplicable should be true, same countries, Org WHT registered", !Data.OB_APWHTApplicable);

			Data.OB_IsCreditor = false;
			GlbCompany.CurrentCompany.Country.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			country.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			uNLOCO.RL_RN_NKCountryCode = country.Code;
			Data.Organisation.OH_RL_NKClosestPort = ZString.Empty;
			Data.Organisation.OH_RL_NKClosestPort = uNLOCO.RL_Code;

			Assert("OM_APTaxApplicable should be true, different countries but both EU, Org GST registered", Data.IsAPTaxApplicable);
			Assert("OM_APWHTApplicable should be true, different countries but both EU, Org WHT registered", !Data.OB_APWHTApplicable);
		}

		#endregion

		#region Invoice terms testing

		public void TestARTermsHasOneRecordByDefault()
		{
			OrgCompanyData companyData = Factory.New<OrgHeader>().CompanyData;
			AssertEquals("One record must be in CompanyData.ARTerms by default.", 1, companyData.ARTerms.Count);
			AssertEquals("Invoice Type 'ALL' must be as default value for default record.", OrgARTermsLookups.InvoiceTypes.All.Code, companyData.ARTerms[0].PY_InvoiceClass);
		}

		public void TestGetARTerm()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				OrgARTerms term1 = Data.ARTerms[0];
				term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
				term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
				OrgARTerms term2 = Data.ARTerms.AddNew();
				term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
				term2.PY_InvoiceDays = 4;
				OrgARTerms term3 = Data.ARTerms.AddNew();
				term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
				term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
				OrgARTerms term4 = Data.ARTerms.AddNew();
				term4.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
				term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term1), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching));
				AssertEquals(new InvoiceTerm(term3), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching));
				AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice));
				AssertEquals(new InvoiceTerm(term3), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));

				Data.ARTerms.DeleteAll();
				AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				InvoiceTerm mTHterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);
				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), 0);

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				Data.OB_AROnCreditHold = false;

				Data.OB_ARCreditApproved = false;
				AssertEquals(mTHterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestGetARTerm_GlobalCreditGroupOnHold()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				var groupOrg = Factory.New<OrgHeader>();
				Data.OB_AROnCreditHold = false;
				Data.OB_ARCreditApproved = false;
				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), 0);
				Data.Organisation.MiscServ.OM_ARGlobalOnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				Data.Organisation.MiscServ.OM_ARGlobalOnCreditHold = false;
				groupOrg.MiscServ.OM_ARGlobalOnCreditHold = true;
				Data.Organisation.MiscServ.OM_OH_ARGlobalCreditGroup = groupOrg.PK;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestGetARTerm_InvoiceType()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
				var dept2 = Factory.NewWithValidTestData<GlbDepartment>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var term1 = Data.ARTerms[0];
				SetupTermsInfo(term1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 10);

				var term2 = Data.ARTerms.AddNew();
				SetupTermsInfo(term2, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromPeriodEnd.Code, 12);

				var term3 = Data.ARTerms.AddNew();
				SetupTermsInfo(term3, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromShipmentDate.Code, 13);

				var term7 = Data.ARTerms.AddNew();
				SetupTermsInfo(term7, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code, 20);

				var term4 = Data.ARTerms.AddNew();
				SetupTermsInfo(term4, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.PaymentInAdvance.Code, 14);

				var term5 = Data.ARTerms.AddNew();
				SetupTermsInfo(term5, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 15);

				var term6 = Data.ARTerms.AddNew();
				SetupTermsInfo(term6, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FreightInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FreightInvoice_Batching));
				AssertEquals(new InvoiceTerm(term7), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.InvoicePerTaxCode));
				AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch2.PK, dept1.PK, InvoiceTypesList.Codes.FreightInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept2.PK, InvoiceTypesList.Codes.FreightInvoice));
				AssertEquals(new InvoiceTerm(term5), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch2.PK, dept2.PK, InvoiceTypesList.Codes.FreightInvoice));
				AssertEquals(new InvoiceTerm(term6), Data.GetARTerm(JobInvoicingConsumerTypes.Brokerage, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch2.PK, dept2.PK, InvoiceTypesList.Codes.FreightInvoice));

				Data.ARTerms.DeleteAll();
				AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				InvoiceTerm mTHterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);
				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), 0);

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				Data.OB_AROnCreditHold = false;

				Data.OB_ARCreditApproved = false;
				AssertEquals(mTHterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestGetARTerm_TransportMode()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
				var dept2 = Factory.NewWithValidTestData<GlbDepartment>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var term1 = Data.ARTerms[0];
				SetupTermsInfo(term1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 10);

				var term2 = Data.ARTerms.AddNew();
				SetupTermsInfo(term2, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Sea, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromPeriodEnd.Code, 12);

				var term7 = Data.ARTerms.AddNew();
				SetupTermsInfo(term7, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.All, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code, 12);

				var term8 = Data.ARTerms.AddNew();
				SetupTermsInfo(term8, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.All, "ALL", InvoiceTermsList.FromCustomsClearanceDate.Code, 25);

				var term3 = Data.ARTerms.AddNew();
				SetupTermsInfo(term3, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.All, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromShipmentDate.Code, 13);

				var term4 = Data.ARTerms.AddNew();
				SetupTermsInfo(term4, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.All, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.PaymentInAdvance.Code, 14);

				var term5 = Data.ARTerms.AddNew();
				SetupTermsInfo(term5, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.All, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 15);

				var term6 = Data.ARTerms.AddNew();
				SetupTermsInfo(term6, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Sea, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term7), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.AirSea, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term8), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FreightInvoice));

				AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch2.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept2.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term5), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch2.PK, dept2.PK, InvoiceTypesList.Codes.FinalInvoice));

				AssertEquals(new InvoiceTerm(term6), Data.GetARTerm(JobInvoicingConsumerTypes.Brokerage, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch2.PK, dept2.PK, InvoiceTypesList.Codes.FreightInvoice));

				Data.ARTerms.DeleteAll();
				AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				InvoiceTerm mTHterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);
				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), 0);

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				Data.OB_AROnCreditHold = false;

				Data.OB_ARCreditApproved = false;
				AssertEquals(mTHterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestGetARTerm_Direction()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
				var dept2 = Factory.NewWithValidTestData<GlbDepartment>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var term1 = Data.ARTerms[0];
				SetupTermsInfo(term1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 10);

				var term2 = Data.ARTerms.AddNew();
				SetupTermsInfo(term2, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromPeriodEnd.Code, 12);

				var term3 = Data.ARTerms.AddNew();
				SetupTermsInfo(term3, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Other, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code, 12);

				var term4 = Data.ARTerms.AddNew();
				SetupTermsInfo(term4, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromCustomsClearanceDate.Code, 25);

				var term5 = Data.ARTerms.AddNew();
				SetupTermsInfo(term5, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Domestic, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromCustomsClearanceDate.Code, 25);

				var term6 = Data.ARTerms.AddNew();
				SetupTermsInfo(term6, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Sea, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.AirSea, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Sea, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Other, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Other, Core.Constants.TransportModes.Courier, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term5), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Domestic, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term6), Data.GetARTerm(JobInvoicingConsumerTypes.Brokerage, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch2.PK, dept2.PK, InvoiceTypesList.Codes.FinalInvoice));

				Data.ARTerms.DeleteAll();
				AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				InvoiceTerm mTHterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);
				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), 0);

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				Data.OB_AROnCreditHold = false;

				Data.OB_ARCreditApproved = false;
				AssertEquals(mTHterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestGetARTerm_JobType()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
				var dept2 = Factory.NewWithValidTestData<GlbDepartment>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var term1 = Data.ARTerms[0];
				SetupTermsInfo(term1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 10);

				var term2 = Data.ARTerms.AddNew();
				SetupTermsInfo(term2, JobInvoicingConsumerTypes.Brokerage.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromPeriodEnd.Code, 12);

				var term3 = Data.ARTerms.AddNew();
				SetupTermsInfo(term3, "ALL", branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code, 12);

				var term4 = Data.ARTerms.AddNew();
				SetupTermsInfo(term4, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, "ALL", Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(JobInvoicingConsumerTypes.CFSShipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(JobInvoicingConsumerTypes.CFSShipment, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(JobInvoicingConsumerTypes.Brokerage, Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));

				Data.ARTerms.DeleteAll();
				AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				InvoiceTerm mTHterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);
				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), 0);

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
				Data.OB_AROnCreditHold = false;

				Data.OB_ARCreditApproved = false;
				AssertEquals(mTHterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

				Data.OB_AROnCreditHold = true;
				AssertEquals(pERterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestGetARTermForDefaultTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
			AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

			Data.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals("GetARTerm().Term", "", Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTerm().Days", (short)0, Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			Org.ARSettlementGroupPK = Org.PK;
			AssertEquals("GetARTerm().Term", "", Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTerm().Days", (short)0, Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			Org.ARSettlementGroupPK = Factory.New<OrgHeader>().PK;
			Org.ARSettlementGroup.ARSettlementGroupPK = Org.PK;
			AssertEquals("Precondition: to exclude stack overflow cases.", Org.ARSettlementGroup.ARSettlementGroupPK, Org.PK);

			Org.ARSettlementGroup.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals("GetARTerm().Term", "", Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTerm().Days", (short)0, Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			OrgARTerms term1 = Org.ARSettlementGroup.CompanyData.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;
			OrgARTerms term3 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
			term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			OrgARTerms term4 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term4.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

			Data.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));

			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Days);

			Data.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));

			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Days);

			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));
			AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching));
			AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));

			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Days);

			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInForeignCurrency).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));
			AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching));
			AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));

			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInForeignCurrency).Days);
			AssertEquals("GetARTermWithoutFallBack().Term", "DEF", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Term);
			AssertEquals("GetARTermWithoutFallBack().Days", (short)0, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice).Days);

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestGetARTermWithFallbackWhenUseARInvoiceTermsAndTermDaysWhenCreditIsOnHoldIsOn()
		{
			Data.OB_AROnCreditHold = true;
			AssertEquals("Pre-condition: Credit should be on hold", true, Data.OB_AROnCreditHold);

			OrgARTerms term1 = Data.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;

			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromMonthEnd.Code;
			onHoldTerms.TermDays = 7;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			using (OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(new InvoiceTerm(term1), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code));
			}

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			using (OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				InvoiceTerm pERterm = new InvoiceTerm(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.OnHoldTerms.Value.Terms), OrganisationRegistry.Instance.OnHoldTerms.Value.TermDays);
				AssertEquals(pERterm, Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			}
		}

		public void TestGetARTermWithFallbackWhenOnCreditHoldIsTrueAndUseARInvoiceTermsAndTermDaysWhenCreditIsOnHoldIsTrue()
		{
			Data.OB_AROnCreditHold = true;
			AssertEquals("Pre-condition: Credit should be on hold", true, Data.OB_AROnCreditHold);

			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var term = Data.ARTerms[0];
			SetupTermsInfo(term, JobInvoicingConsumerTypes.Shipment.Code, branch.PK, dept.PK, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 10);

			var registryTerm = new OnHoldTerms();
			registryTerm.Terms = InvoiceTermsList.FromInvoiceDate.Code;
			registryTerm.TermDays = 7;

			using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryTerm))
			using (OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = Data.GetARTermWithoutFallback(JobInvoicingConsumerTypes.Shipment, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, branch.PK, dept.PK, InvoiceTypesList.Codes.FinalInvoice);
				AssertEquals("Should get a best matched ARTerm.", new InvoiceTerm(term), result);
			}
		}

		public void TestGetARTermWithFallbackWhenOnCreditHoldIsTrueAndUseARInvoiceTermsAndTermDaysWhenCreditIsOnHoldIsFalse()
		{
			Data.OB_AROnCreditHold = true;
			AssertEquals("Pre-condition: Credit should be on hold", true, Data.OB_AROnCreditHold);

			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var orgHeader = Data.Header;

			var term = Data.ARTerms[0];
			SetupTermsInfo(term, JobInvoicingConsumerTypes.Shipment.Code, branch.PK, dept.PK, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 10);

			var registryTerm = new OnHoldTerms();
			registryTerm.Terms = InvoiceTermsList.FromInvoiceDate.Code;
			registryTerm.TermDays = 7;

			using (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryTerm))
			using (OrganisationRegistry.Instance.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = Data.GetARTermWithoutFallback(JobInvoicingConsumerTypes.Shipment, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, branch.PK, dept.PK, InvoiceTypesList.Codes.FinalInvoice);

				CombineAssertions("Should get a ARTerm from the registry default configuration.", () =>
				{
					AssertEquals("Should get the term day from the registry default configuration.", registryTerm.TermDays, result.Days);
					AssertEquals("Should get the term from the registry default configuration.", registryTerm.Terms, result.Term);
				});
			}
		}

		public void TestGetARTermWithFallbackByInvoiceTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);

			OrgARTerms term1 = Data.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Data.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;
			OrgARTerms term3 = Data.ARTerms.AddNew();
			term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
			term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			OrgARTerms term4 = Data.ARTerms.AddNew();
			term4.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

			AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term1), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.PaymentInAdvance.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromMonthEnd.Code));
			AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromMonthEnd.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.CashOnDelivery.Code));

			AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term3), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.PaymentInAdvance.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromMonthEnd.Code));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromMonthEnd.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.CashOnDelivery.Code));

			AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term3), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.PaymentInAdvance.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromMonthEnd.Code));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromMonthEnd.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.CashOnDelivery.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.PaymentInAdvance.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromMonthEnd.Code));
			AssertEquals(new InvoiceTerm(), Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromMonthEnd.Code));

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;

			Data.Header.MiscServ.OM_ARGlobalOnCreditHold = true;
			AssertEquals("Global Credit On Hold should set ARTerm to COD", "COD", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			Data.Header.MiscServ.OM_ARGlobalOnCreditHold = false;

			var groupOrg = Factory.NewWithValidTestData<OrgHeader>();
			groupOrg.MiscServ.OM_ARGlobalOnCreditHold = true;
			Data.Header.MiscServ.OM_OH_ARGlobalCreditGroup = groupOrg.PK;
			AssertEquals("Global Credit On Hold should set ARTerm to COD", "COD", Data.GetARTermWithoutFallback(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
		}

		public void TestGetARTermForDefaultTermWithFallbackByInvoiceTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);

			Org.ARSettlementGroupPK = Factory.New<OrgHeader>().PK;

			OrgARTerms term1 = Org.ARSettlementGroup.CompanyData.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;
			OrgARTerms term3 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
			term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			OrgARTerms term4 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term4.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

			Data.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromMonthEnd.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			Data.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromMonthEnd.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromMonthEnd.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromMonthEnd.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(term3), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(term4), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromMonthEnd.Code));

			AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromInvoiceDate.Code));

			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestGetDisbursementARTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
			AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			OrgARTerms term1 = Data.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Data.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;
			OrgARTerms term3 = Data.ARTerms.AddNew();
			term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
			term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			OrgARTerms term4 = Data.ARTerms.AddNew();
			term4.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;

			Data.ARTerms.DeleteAll();
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
		}

		public void TestGetDisbursementARTermForDefaultTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
			AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", Data.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			Data.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals("GetDisbursementARTerm().Term", "", Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetDisbursementARTerm().Days", (short)0, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			Org.ARSettlementGroupPK = Org.PK;
			AssertEquals("GetDisbursementARTerm().Term", "", Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetDisbursementARTerm().Days", (short)0, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			Org.ARSettlementGroupPK = Factory.New<OrgHeader>().PK;
			Org.ARSettlementGroup.ARSettlementGroupPK = Org.PK;
			AssertEquals("Precondition: to exclude stack overflow cases.", Org.ARSettlementGroup.ARSettlementGroupPK, Org.PK);

			Org.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals("GetDisbursementARTerm().Term", "", Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals("GetDisbursementARTerm().Days", (short)0, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			OrgARTerms term1 = Org.ARSettlementGroup.CompanyData.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;
			OrgARTerms term3 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
			term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			OrgARTerms term4 = Org.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm();
			term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

			Data.CreateOrLoadDisbursementARTerm().Delete();
			Data.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term2), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			Data.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInForeignCurrency).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestGetDisbursementARTermWithFallbackByInvoiceTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);

			OrgARTerms term1 = Data.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Data.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;
			OrgARTerms term3 = Data.ARTerms.AddNew();
			term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
			term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			OrgARTerms term4 = Data.ARTerms.AddNew();
			term4.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromMonthEnd.Code));

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestGetDisbursementARTermForDefaultTermWithFallbackByInvoiceTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);

			Org.ARSettlementGroupPK = Factory.New<OrgHeader>().PK;

			OrgARTerms term1 = Org.ARSettlementGroup.CompanyData.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;
			OrgARTerms term3 = Org.ARSettlementGroup.CompanyData.ARTerms.AddNew();
			term3.PY_InvoiceClass = InvoiceTypesList.Codes.DisbursementInvoice;
			term3.PY_InvoiceTerm = InvoiceTermsList.PaymentInAdvance.Code;
			OrgARTerms term4 = Org.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm();
			term4.PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;

			Data.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromMonthEnd.Code));

			Data.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromMonthEnd.Code));

			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInvoice).PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.CashOnDelivery.Code));
			AssertEquals(new InvoiceTerm(term2), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromInvoiceDate.Code));
			AssertEquals(new InvoiceTerm(), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.PaymentInAdvance.Code));
			AssertEquals(new InvoiceTerm(term4), Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, InvoiceTermsList.FromMonthEnd.Code));

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestCreateOrLoadARTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);

			OrgARTerms term1 = Data.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Data.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;

			AssertEquals("Precondition: ARTerms.Count", 2, Data.ARTerms.Count);

			AssertNull("Returns null if type it empty.", Data.CreateOrLoadARTerm(""));
			AssertNull("Returns null if type it null.", Data.CreateOrLoadARTerm(null));

			OrgARTerms disbTerm = Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInvoice);
			AssertEquals(InvoiceTypesList.Codes.DisbursementInvoice, disbTerm.PY_InvoiceClass);
			AssertEquals("New term must be added to collection. ARTerms.Count: ", 3, Data.ARTerms.Count);
			AssertNotNull("New term must be in collection.", Data.ARTerms.FindByPK(disbTerm.PK));

			AssertEquals("Must return existing term.", term1, Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.FinalInvoice));
			AssertEquals("Any new terms must not be added to collection. ARTerms.Count: ", 3, Data.ARTerms.Count);

			AssertEquals("Must return just added term.", disbTerm, Data.CreateOrLoadARTerm(InvoiceTypesList.Codes.DisbursementInvoice));
			AssertEquals("Any new terms must not be added to collection. ARTerms.Count: ", 3, Data.ARTerms.Count);

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestCreateOrLoadDisbursementARTerm()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);

			OrgARTerms term1 = Data.ARTerms[0];
			term1.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
			term1.PY_InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			OrgARTerms term2 = Data.ARTerms.AddNew();
			term2.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			term2.PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			term2.PY_InvoiceDays = 4;

			AssertEquals("Precondition: ARTerms.Count", 2, Data.ARTerms.Count);

			OrgARTerms disbTerm = Data.CreateOrLoadDisbursementARTerm();
			AssertEquals(OrgARTermsLookups.InvoiceTypes.DSB.Code, disbTerm.PY_InvoiceClass);
			AssertEquals("New term must be added to collection. ARTerms.Count: ", 3, Data.ARTerms.Count);
			AssertNotNull("New term must be in collection.", Data.ARTerms.FindByPK(disbTerm.PK));

			AssertEquals("Must return just added term.", disbTerm, Data.CreateOrLoadDisbursementARTerm());
			AssertEquals("Any new terms must not be added to collection. ARTerms.Count: ", 3, Data.ARTerms.Count);

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestMarkARTermAsNeedValidationOnIsDebtorChange()
		{
			AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
			AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);

			Data.ARTerms[0].RunPreSaveValidation();
			AssertEquals("Precondition: LightValidationIsValid", true, Data.ARTerms[0].LightValidationIsValid);

			Data.OB_IsDebtor = true;
			AssertEquals("LightValidationIsValid", false, Data.ARTerms[0].LightValidationIsValid);

			InvoiceTerm cODterm = new InvoiceTerm(OrganisationRegistry.Instance.PreApprovalTerms.Value, new ARInvoiceTermsList().GetDescriptionFromCode(OrganisationRegistry.Instance.PreApprovalTerms.Value), 0);

			Data.OB_AROnCreditHold = true;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_AROnCreditHold = false;

			Data.OB_ARCreditApproved = false;
			AssertEquals(cODterm, Data.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty));
			Data.OB_ARCreditApproved = true;
		}

		public void TestGetOrgARTerms()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
				var dept2 = Factory.NewWithValidTestData<GlbDepartment>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var aRSettlementGroup = Factory.New<OrgHeader>();
				Org.ARSettlementGroupPK = aRSettlementGroup.PK;
				Org.ARSettlementGroup.ARSettlementGroupPK = Org.PK;
				AssertEquals("Precondition: to exclude stack overflow cases.", Org.ARSettlementGroup.ARSettlementGroupPK, Org.PK);

				Data.ARTerms.DeleteAll();

				var term1 = Data.ARTerms.AddNew();
				SetupTermsInfo(term1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 0);

				var term2 = Data.ARTerms.AddNew();
				SetupTermsInfo(term2, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromPeriodEnd.Code, 12);

				var term3 = Data.ARTerms.AddNew();
				SetupTermsInfo(term3, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.DisbursementInForeignCurrency, OrgARTermsLookups.DefaultInvoiceTerm.Code, 0);

				var term4 = Data.ARTerms.AddNew();
				SetupTermsInfo(term4, JobInvoicingConsumerTypes.CFSLoadList.Code, ZGuid.Empty, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, OrgARTermsLookups.DefaultInvoiceTerm.Code, 20);

				var term5 = Data.ARTerms.AddNew();
				SetupTermsInfo(term5, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", OrgARTermsLookups.InvoiceTypes.DSB.Code, InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 15);

				var term6 = Data.ARTerms.AddNew();
				SetupTermsInfo(term6, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				var sgCompanyData = aRSettlementGroup.CompanyData;
				var termSG1 = sgCompanyData.ARTerms[0];
				SetupTermsInfo(termSG1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromCustomsClearanceDate.Code, 12);

				var termSG2 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG2, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.DisbursementInForeignCurrency, InvoiceTermsList.FromPeriodEnd.Code, 95);

				var termSG3 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG3, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromMonthEnd.Code, 15);

				var termSG4 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG4, JobInvoicingConsumerTypes.CFSLoadList.Code, ZGuid.Empty, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, "ALL", InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromShipmentDate.Code, 45);

				var termSG5 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG5, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", OrgARTermsLookups.InvoiceTypes.DSB.Code, InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 15);

				var termSG6 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG6, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", Core.Constants.TransportModes.Air, "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FreightInvoice));
				AssertEquals(new InvoiceTerm(termSG2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, ZGuid.Empty, dept1.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
				AssertEquals(new InvoiceTerm(termSG4), Data.GetARTerm(JobInvoicingConsumerTypes.CFSLoadList, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));

				var expectedARTerms = new OrgARTerms[] { term1, term2, termSG2, termSG4, term5, term6 };
				var actualARTerms = Data.GetOrgARTerms();
				expectedARTerms.ForEach(x => AssertEquals("Should contain: " + x.ToString(), true, actualARTerms.Contains(x)));
				actualARTerms.ForEach(x => AssertEquals("Should contain: " + x.ToString(), true, expectedARTerms.Contains(x)));
			}
		}

		public void TestBothSQLAndCodeFunctionProduceTheSameCSVOrgARTermText()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
				var dept2 = Factory.NewWithValidTestData<GlbDepartment>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var aRSettlementGroup = Factory.NewWithValidTestData<OrgHeader>();
				Org.ARSettlementGroupPK = aRSettlementGroup.PK;
				Org.ARSettlementGroup.ARSettlementGroupPK = Org.PK;
				AssertEquals("Precondition: to exclude stack overflow cases.", Org.ARSettlementGroup.ARSettlementGroupPK, Org.PK);

				Factory.Save();

				Data.ARTerms.DeleteAll();

				var term1 = Data.ARTerms.AddNew();
				SetupTermsInfo(term1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.CashOnDelivery.Code, 0);

				var term2 = Data.ARTerms.AddNew();
				SetupTermsInfo(term2, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FreightInvoice, InvoiceTermsList.FromPeriodEnd.Code, 12);

				var term3 = Data.ARTerms.AddNew();
				SetupTermsInfo(term3, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.DisbursementInForeignCurrency, OrgARTermsLookups.DefaultInvoiceTerm.Code, 0);

				var term4 = Data.ARTerms.AddNew();
				SetupTermsInfo(term4, JobInvoicingConsumerTypes.CFSLoadList.Code, ZGuid.Empty, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, OrgARTermsLookups.DefaultInvoiceTerm.Code, 20);

				var term5 = Data.ARTerms.AddNew();
				SetupTermsInfo(term5, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", OrgARTermsLookups.InvoiceTypes.DSB.Code, InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 15);

				var term6 = Data.ARTerms.AddNew();
				SetupTermsInfo(term6, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				var sgCompanyData = aRSettlementGroup.CompanyData;

				sgCompanyData.ARTerms.DeleteAll();

				var termSG1 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG1, JobInvoicingConsumerTypes.Shipment.Code, branch1.PK, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromCustomsClearanceDate.Code, 12);

				var termSG2 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG2, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, dept1.PK, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, InvoiceTypesList.Codes.DisbursementInForeignCurrency, InvoiceTermsList.FromPeriodEnd.Code, 95);

				var termSG3 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG3, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTermsList.FromMonthEnd.Code, 15);

				var termSG4 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG4, JobInvoicingConsumerTypes.CFSLoadList.Code, ZGuid.Empty, ZGuid.Empty, Core.Constants.FreightShipmentDirection.Code.Export, "ALL", InvoiceTypesList.Codes.FinalInvoice, InvoiceTermsList.FromShipmentDate.Code, 45);

				var termSG5 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG5, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", OrgARTermsLookups.InvoiceTypes.DSB.Code, InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 15);

				var termSG6 = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG6, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				Factory.Save();

				AssertEquals(new InvoiceTerm(term1), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FinalInvoice));
				AssertEquals(new InvoiceTerm(term2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, branch1.PK, dept1.PK, InvoiceTypesList.Codes.FreightInvoice));
				AssertEquals(new InvoiceTerm(termSG2), Data.GetARTerm(JobInvoicingConsumerTypes.Shipment, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, ZGuid.Empty, dept1.PK, InvoiceTypesList.Codes.DisbursementInForeignCurrency));
				AssertEquals(new InvoiceTerm(termSG4), Data.GetARTerm(JobInvoicingConsumerTypes.CFSLoadList, Core.Constants.FreightShipmentDirection.Code.Export, Core.Constants.TransportModes.Air, ZGuid.Empty, ZGuid.Empty, InvoiceTypesList.Codes.FinalInvoice));

				var expectedDSBCSVString = term6.ToString() + ", " + termSG2.ToString() + ", " + term5.ToString();
				var actualDSBCSVString = Data.GetOrgARTermsAsCSV(", ", x => x.IsDisbursementTerm || x.IsDefaultTerm);
				AssertEquals("Org AR Term DSB CSV text", expectedDSBCSVString, actualDSBCSVString);

				var sql = string.Format("SELECT TOP 1 CSVTermsText from dbo.GetOrgARTermsInCSV('{0}', '{1}', 1)", Data.OB_GC, Data.OB_OH);
				var sqlCSV = Db.Connection.ExecuteScalar(sql);

				var arTermsreturnedBySQL = sqlCSV.ToString().Split(',').Select(x => x.Trim());
				var expectedARTerms = expectedDSBCSVString.Split(',').Select(x => x.Trim());
				expectedARTerms.ForEach(x => AssertEquals("Should contain: " + x, true, arTermsreturnedBySQL.Contains(x)));
				arTermsreturnedBySQL.ForEach(x => AssertEquals("Should contain: " + x, true, expectedARTerms.Contains(x)));

				var expectedStdCSVString = term6.ToString() + ", " + term2.ToString() + ", " + term1.ToString() + ", " + "(CLL-EXP-AIR-FIN)->45/SHP";
				var actualStdCSVString = Data.GetOrgARTermsAsCSV(", ", x => !x.IsDisbursementTerm || x.IsDefaultTerm);
				AssertEquals("Org AR Term standard CSV text", expectedStdCSVString, actualStdCSVString);

				sql = string.Format("SELECT TOP 1 CSVTermsText from dbo.GetOrgARTermsInCSV('{0}', '{1}', 0)", Data.OB_GC, Data.OB_OH);
				sqlCSV = Db.Connection.ExecuteScalar(sql);

				arTermsreturnedBySQL = sqlCSV.ToString().Split(',').Select(x => x.Trim());
				expectedARTerms = actualStdCSVString.Split(',').Select(x => x.Trim());
				expectedARTerms.ForEach(x => AssertEquals("Should contain: " + x, true, arTermsreturnedBySQL.Contains(x)));
				arTermsreturnedBySQL.ForEach(x => AssertEquals("Should contain: " + x, true, expectedARTerms.Contains(x)));
			}
		}

		public void TestCSVOrgARTermTextWhenJobTypeIsNULL()
		{
			var onHoldTerms = new OnHoldTerms();
			onHoldTerms.Terms = ARInvoiceTermsList.FromPeriodEnd.Code;

			using (OrganisationRegistry.Instance.PreApprovalTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MTH"))
			using (OrganisationRegistry.Instance.OnHoldTerms.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, onHoldTerms))
			{
				AssertEquals("Pre-condition: Credit should be approved", true, Data.OB_ARCreditApproved);
				AssertEquals("Pre-condition: Credit should not be on hold", false, Data.OB_AROnCreditHold);
				AssertEquals("By defaul terms for Invoice Type 'ALL' must be returned.", OrgARTermsLookups.InvoiceTypes.All.Code, Data.LoadARTermForAllInvoiceTypes().PY_InvoiceClass);

				var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
				var dept2 = Factory.NewWithValidTestData<GlbDepartment>();
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var aRSettlementGroup = Factory.NewWithValidTestData<OrgHeader>();
				Org.ARSettlementGroupPK = aRSettlementGroup.PK;
				Org.ARSettlementGroup.ARSettlementGroupPK = Org.PK;
				AssertEquals("Precondition: to exclude stack overflow cases.", Org.ARSettlementGroup.ARSettlementGroupPK, Org.PK);

				Factory.Save();

				Data.ARTerms.DeleteAll();

				var term1 = Data.ARTerms.AddNew();
				SetupTermsInfo(term1, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", OrgARTermsLookups.InvoiceTypes.DSB.Code, OrgARTermsLookups.DefaultInvoiceTerm.Code, 0);

				var term2 = Data.ARTerms.AddNew();
				SetupTermsInfo(term2, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				var sgCompanyData = aRSettlementGroup.CompanyData;

				sgCompanyData.ARTerms.DeleteAll();

				var termSG = sgCompanyData.ARTerms.AddNew();
				SetupTermsInfo(termSG, "ALL", ZGuid.Empty, ZGuid.Empty, "ALL", "ALL", "ALL", InvoiceTermsList.MonthsFromInvoiceCycleDate.Code, 16);

				Factory.Save();

				var expectedCSVString = term2.ToString() + ", " + "(DSB)->16/MIC";
				var actualCSVString = Data.GetOrgARTermsAsCSV(", ");
				AssertEquals("Org AR Term DSB CSV text", expectedCSVString, actualCSVString);
			}
		}

		#endregion

		public void TestGetOnlyOrgARTermWithNoElements()
		{
			var compData = Factory.New<OrgCompanyData>();
			compData.ARTerms.DeleteAll();
			InvoiceTerm disbursmentTerm;

			AssertEquals("No ARTerms results in empty disbursementTerm", false, compData.TryToGetTheOnlyOrgARTerm(out disbursmentTerm, true));
		}

		public void TestDefaultingOfPayablesTaxApplicable()
		{
			try
			{
				Org.OH_Code = "AAA";
				Org.CompanyData.SetAPTaxApplicable(false);
				Org.CompanyData.OB_IsCreditor = false;
				Org.CompanyData.OB_IsDebtor = false;
				Org.OH_IsForwarder = false;

				AssertEquals("IsAPTaxApplicable", false, Org.CompanyData.IsAPTaxApplicable);
				AssertEquals("OB_IsCreditor", false, Org.CompanyData.OB_IsCreditor);
				AssertEquals("OB_IsDebtor", false, Org.CompanyData.OB_IsDebtor);
				AssertEquals("OH_IsForwarder", false, Org.OH_IsForwarder);

				Org.CompanyData.OB_IsCreditor = true;
				Org.CompanyData.OB_IsDebtor = true;
				Org.CompanyData.SetARTaxApplicable(true);
				Org.OH_IsForwarder = true;

				AssertEquals("IsAPTaxApplicable", true, Org.CompanyData.IsAPTaxApplicable);

				Factory.Save();

				Org.ShowMessage += new OrgHeader.ShowMessageEventHandler(Org_ShowMessage);

				Org.CompanyData.SetARTaxApplicable(false);
				AssertEquals("Changing Debtor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change.", LastShowMessage);

				Org.CompanyData.SetAPTaxApplicable(false);
				AssertEquals("Changing Creditor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Creditor will NOT change.", LastShowMessage);

				Org.CompanyData.OB_IsCreditor = false;
				Org.CompanyData.OB_IsDebtor = false;
				Org.OH_IsForwarder = false;

				AssertEquals("IsARTaxApplicable", false, Org.CompanyData.IsARTaxApplicable);
				AssertEquals("IsAPTaxApplicable", false, Org.CompanyData.IsAPTaxApplicable);
				AssertEquals("OB_IsCreditor", false, Org.CompanyData.OB_IsCreditor);
				AssertEquals("OB_IsDebtor", false, Org.CompanyData.OB_IsDebtor);
				AssertEquals("OH_IsForwarder", false, Org.OH_IsForwarder);

				Org.OH_IsForwarder = true;
				Org.CompanyData.OB_IsDebtor = true;
				Org.CompanyData.SetARTaxApplicable(true);
				AssertEquals("Changing Debtor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change.", LastShowMessage);

				Org.CompanyData.OB_IsCreditor = true;

				AssertEquals("IsAPTaxApplicable", true, Org.CompanyData.IsAPTaxApplicable);
				AssertEquals("Changing Creditor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Creditor will NOT change.", LastShowMessage);
			}
			finally
			{
				Org.ShowMessage -= Org_ShowMessage;
			}
		}

		public void TestCreditControlledDocumentsCheckForDoNotCheckOverdueInvoicesStatus()
		{
			AssertCreditControlledDocumentsCheckForDoNotCheckOverdueInvoicesStatus(false);
		}

		public void TestCreditControlledDocumentsCheckForGlobalDoNotCheckOverdueInvoicesStatus()
		{
			AssertCreditControlledDocumentsCheckForDoNotCheckOverdueInvoicesStatus(true);
		}

		void AssertCreditControlledDocumentsCheckForDoNotCheckOverdueInvoicesStatus(bool isGlobal)
		{
			Org.CompanyData.OB_IsDebtor = true;
			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			AssertNotNull(singaporeCompany);
			var companyPK = isGlobal ? singaporeCompany.PK : GlbCompany.CurrentCompany.PK;

			var dsb_none = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 3, 10m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired);
			var dsb_1st = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 5, 20m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var dsb_2nd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 10, 30m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var dsb_3rd = createCreditControlledDocumentsCheckConfigurationLine(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB.Code, 10, 30m, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			var accountingMock = new Mock<IAccounting>();
			var configurationCollection = new ICreditControlledDocumentsCheckConfiguration[] { dsb_none, dsb_1st, dsb_2nd, dsb_3rd };

			if (isGlobal)
			{
				accountingMock.Setup(c => c.GetGlobalCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);

				Org.MiscServ.OM_ARGlobalCreditLimit = 15000m;
				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeCompany.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var tempFactory = new BusinessObjectFactory();
					var tempOrg = tempFactory.Load<OrgHeader>(Org.PK);
					tempOrg.CompanyData.OB_IsDebtor = true;

					var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
					var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
					tempExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;
					tempExchangeRate.RE_SellRate = 2m;
					tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
					tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

					tempFactory.Save();
				}
			}
			else
			{
				accountingMock.Setup(c => c.GetCreditControlledDocumentsCheckConfiguration()).Returns(configurationCollection);
			}

			ObjectFactory.Substitute(accountingMock.Object);

			CreateInvoiceForCreditControlledDocumentsCheck(3, Org, 2, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("No Approval required", 0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			CreateInvoiceForCreditControlledDocumentsCheck(2, Org, 4, companyPK, InvoiceTypesList.Codes.DisbursementInvoice);
			Factory.Save();

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			if (isGlobal)
			{
				AssertEquals("Precondition", false, Org.MiscServ.OM_GlobalDoNotCheckOverdueInvoicesStatus);
			}
			else
			{
				AssertEquals("Precondition", false, Org.CompanyData.OB_ARDoNotCheckOverdueInvoicesStatus);
			}

			AssertEquals("First level authorization required", 1, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());

			if (isGlobal)
			{
				Org.MiscServ.OM_GlobalDoNotCheckOverdueInvoicesStatus = true;
			}
			else
			{
				Org.CompanyData.OB_ARDoNotCheckOverdueInvoicesStatus = true;
			}

			Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + Org.CompanyData.PK);

			AssertEquals("No Approval required", 0, Org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck());
		}

		#region Org Type - Receivables/Payables

		public void TestReceivablesSelection()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.CompanyData.OB_IsDebtor = true;
			Assert(!orgHeader.CompanyData.OB_OJ_ARDebtorGroupInfo.ReadOnly);
			orgHeader.OH_IsDebtor = false;
			orgHeader.CompanyData.OB_IsDebtor = false;
			Assert(orgHeader.CompanyData.OB_OJ_ARDebtorGroupInfo.ReadOnly);
		}

		public void TestReceivablesReadOnlyState()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.CompanyData.OB_IsDebtor = true;

			PropertyInfo roProp = typeof(ZPropertyInfo).GetProperty("ReadOnly");
			roProp.SetValue(orgHeader.OH_IsDebtorInfo, true, null);

			roProp = typeof(ZPropertyInfo).GetProperty("ReadOnly");
			roProp.SetValue(orgHeader.CompanyData.OB_IsDebtorInfo, true, null);

			Assert(!orgHeader.CompanyData.OB_OJ_ARDebtorGroupInfo.ReadOnly);
			roProp = typeof(OrgCompanyData).GetProperty("OB_OJ_ARDebtorGroup_ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert(!(bool)roProp.GetValue(orgHeader.CompanyData, null));
		}

		public void TestPayablesSelection()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsCreditor = true;
			Assert(!orgHeader.CompanyData.OB_OG_APCreditorGroupInfo.ReadOnly);
			orgHeader.OH_IsCreditor = false;
			Assert(orgHeader.CompanyData.OB_OG_APCreditorGroupInfo.ReadOnly);
		}

		public void TestPayablesReadOnlyState()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsCreditor = true;

			PropertyInfo roProp = typeof(ZPropertyInfo).GetProperty("ReadOnly");
			roProp.SetValue(orgHeader.OH_IsCreditorInfo, true, null);

			roProp = typeof(ZPropertyInfo).GetProperty("ReadOnly");
			roProp.SetValue(orgHeader.CompanyData.OB_IsCreditorInfo, true, null);

			Assert(!orgHeader.CompanyData.OB_OG_APCreditorGroupInfo.ReadOnly);
			roProp = typeof(OrgCompanyData).GetProperty("OB_OG_APCreditorGroup_ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert(!(bool)roProp.GetValue(orgHeader.CompanyData, null));
		}

		#endregion

		#region Credit Limit Temporary Increase

		public void TestARTemporaryCreditLimitIncreaseExpiryLocalBranchTimeIsReadOnly()
		{
			AssertEquals("ARTemporaryCreditLimitIncreaseExpiryUTCInfo doesn't need a setter. User will not set this property business logic will decide the date", false, Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTimeInfo.HasSetter);
		}

		[TestDate(2014, 2, 4, 11, 0, 0)]
		[TestUtcOffset(1, 0, 0)]
		public void TestARTemporaryCreditLimitIncreaseExpiryLocalBranchTimeConvertsFromUTC()
		{
			using (new MasterFilesTestHelper(Factory).GetUtcPlus8UserContext())
			{
				var utcDate = new ZDateTime(2014, 2, 4, 20, 0, 0);
				var expectedLocalDate = utcDate.AddHours(8);
				Data.OB_ARTemporaryCreditLimitIncreaseExpiry = utcDate;
				AssertEquals("UTC value in database correctly converted to Local Branch Time for display", expectedLocalDate, Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			}
		}

		[TestDate(2014, 02, 07)]
		public void TestARTemporaryCreditLimitAndTemporaryCreditLimitInEffect()
		{
			Data.OB_ARCreditLimit = 0M;
			Data.OB_ARTemporaryCreditLimitIncrease = 0M;
			Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.UtcNow.AddMinutes(1);
			AssertEquals(0M, Data.ARTemporaryCreditLimit);
			AssertEquals(false, Data.TemporaryCreditLimitInEffect);
			Data.OB_ARCreditLimit = 100M;
			Data.OB_ARTemporaryCreditLimitIncrease = 0M;
			Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.UtcNow.AddMinutes(1);
			AssertEquals(100M, Data.ARTemporaryCreditLimit);
			AssertEquals(false, Data.TemporaryCreditLimitInEffect);
			Data.OB_ARCreditLimit = 100M;
			Data.OB_ARTemporaryCreditLimitIncrease = 20M;
			Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.UtcNow.AddMinutes(1);
			AssertEquals(120M, Data.ARTemporaryCreditLimit);
			AssertEquals(true, Data.TemporaryCreditLimitInEffect);
			Data.OB_ARCreditLimit = 100M;
			Data.OB_ARTemporaryCreditLimitIncrease = -20M;
			Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.UtcNow.AddMinutes(1);
			AssertEquals("Should ignore invalid negative value", 100M, Data.ARTemporaryCreditLimit);
			AssertEquals(false, Data.TemporaryCreditLimitInEffect);
			Data.OB_ARCreditLimit = 100M;
			Data.OB_ARTemporaryCreditLimitIncrease = 20M;
			Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.UtcNow.AddMinutes(-1);
			AssertEquals("Increase has expired", 100M, Data.ARTemporaryCreditLimit);
			AssertEquals(false, Data.TemporaryCreditLimitInEffect);
			Data.OB_ARCreditLimit = 100M;
			Data.OB_ARTemporaryCreditLimitIncrease = 20M;
			Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Empty;
			AssertEquals("Although empty date and actual increase can't be saved due to DB check constraint, lets check it would do the right thing anyway", 100M, Data.ARTemporaryCreditLimit);
			AssertEquals(false, Data.TemporaryCreditLimitInEffect);
			ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Data.OB_ARCreditLimit = 100M;
			Data.OB_ARTemporaryCreditLimitIncrease = 20M;
			Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.UtcNow.AddMinutes(1);
			AssertEquals("Ignore increase if using credit web service", 100M, Data.ARTemporaryCreditLimit);
			AssertEquals(false, Data.TemporaryCreditLimitInEffect);
		}

		public void TestSettingTempCreditLimitCausesExpiryDateToBeSetCorrectly_NoRegistry()
		{
			Factory.Save();
			Assert("Expiry not set due by default", Data.OB_ARTemporaryCreditLimitIncreaseExpiry.IsEmpty);
			Data.OB_ARTemporaryCreditLimitIncrease = 100;
			Assert("Expiry not set due to no registry settings controlling it", Data.OB_ARTemporaryCreditLimitIncreaseExpiry.IsEmpty);
		}

		CreditTemporaryIncreaseAuthorisationSettingsRegistryItem CreditLimitCheckTemporaryCreditLimitIncreaseThreshold
		{
			get { return ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold as CreditTemporaryIncreaseAuthorisationSettingsRegistryItem; }
		}

		[TestDate(2014, 2, 4, 11, 0, 0)]
		public void TestSettingTempCreditLimitCausesExpiryDateToBeSetCorrectly_WithAmountsInRegistry()
		{
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(50, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(100, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(100, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			Assert("Expiry not set due by default", Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);
			Data.OB_ARTemporaryCreditLimitIncrease = 50;
			AssertEquals("Expiry set 5 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 9).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 100;
			AssertEquals("Expiry set 10 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 14).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 150;
			AssertEquals("Expiry set 15 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 19).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 0;
			Assert("Expiry is not set due to no increase set", Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);
		}

		[TestDate(2014, 2, 4, 11, 0, 0)]
		public void TestSettingTempCreditLimitCausesExpiryDateToBeSetCorrectly_WithPercentagesInRegistry()
		{
			Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
			Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(0, 10, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 20, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 20, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			Data.OB_ARCreditLimit = 1000;
			Assert("Expiry not set due by default", Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);
			Data.OB_ARTemporaryCreditLimitIncrease = 100;
			AssertEquals("Expiry set 5 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 9).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 200;
			AssertEquals("Expiry set 10 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 14).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 200.01;
			AssertEquals("Expiry set 15 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 19).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 0;
			Assert("Expiry is not set due to no increase set", Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);

			Data.OB_ARCreditLimit = 0.01;
			Assert("Expiry not set due by default", Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);
			Data.OB_ARTemporaryCreditLimitIncrease = 0.001;
			AssertEquals("Expiry set 5 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 9).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 0.002;
			AssertEquals("Expiry set 10 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 14).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 0.0021;
			AssertEquals("Expiry set 15 days in future from the beginning of today (2014-02-04)", new ZDateTime(2014, 2, 19).AddMinutes(-1), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			Data.OB_ARTemporaryCreditLimitIncrease = 0;
			Assert("Expiry is not set due to no increase set", Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);

			Data.OB_ARCreditLimit = 0;
			Data.OB_ARTemporaryCreditLimitIncrease = 0.01;
			Assert("Expiry is set", !Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);
			Data.OB_ARTemporaryCreditLimitIncrease = 0;
			Assert("Expiry is not set due to no increase set", Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime.IsEmpty);
		}

		[TestDate(2014, 2, 4, 11, 0, 0)]
		public void TestSavingDataWithTempCreditIncreaseCausesLogEvent()
		{
			using (new MasterFilesTestHelper(Factory).GetUtcPlus8UserContext())
			{
				Data.OB_ARCreditLimit = 50000;
				Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
				Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
				Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

				var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
				collection.CreateCreditTemporaryIncreaseRequirement(20000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, 5);
				collection.CreateCreditTemporaryIncreaseRequirement(20000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 365);
				collection.RunPreSaveValidation();
				AssertEquals(false, collection.HasNotifications());
				CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				Factory.Save();
				AssertEquals(0, Data.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModified.Code)).Length);
				Data.OB_ARTemporaryCreditLimitIncrease = 20000;
				Factory.Save();
				var log = Data.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModified.Code)).Cast<StmALog>().Single();
				AssertEquals("Credit Limit Adjustment of: 20000.00 to: 70000.00, Expiry: 2014-02-08 15:59 UTC|TYPE=TCLA", log.SL_Reference);
			}
		}

		/// <remarks>
		/// This is required for the scenario where the expiry has not been set due to validation error (e.g. regsitry) and the form is kept open
		/// while the registry is fixed. When you now save the expiry needs to be set.
		/// </remarks>
		[TestDate(2014, 2, 4, 11, 0, 0)]
		public void TestSetsExpiryAgainOnSaving()
		{
			using (new MasterFilesTestHelper(Factory).GetUtcPlus8UserContext())
			{
				Data.OB_ARCreditLimit = 50000;
				Env.Security.CreditLimitAdjustmentFirstLevel.IsAllowed = true;
				Env.Security.CreditLimitAdjustmentSecondLevel.IsAllowed = true;
				Env.Security.CreditLimitAdjustmentThirdLevel.IsAllowed = true;

				var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
				collection.CreateCreditTemporaryIncreaseRequirement(20000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired, 5);
				collection.CreateCreditTemporaryIncreaseRequirement(20000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 365);
				collection.RunPreSaveValidation();
				AssertEquals(false, collection.HasNotifications());
				CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				Data.OB_ARTemporaryCreditLimitIncrease = 20000;
				Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Empty; // Simulate problem where date not set for whatever reason
				Factory.Save();
				AssertEquals("Date set during saving process for new", new ZDateTime(2014, 02, 08, 23, 59, 00), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);

				Data.OB_ARTemporaryCreditLimitIncrease = 10000;
				Data.OB_ARTemporaryCreditLimitIncreaseExpiry = ZDateTime.Empty; // Simulate problem where date not set for whatever reason
				Factory.Save();
				AssertEquals("Date set during saving process for edit", new ZDateTime(2014, 02, 08, 23, 59, 00), Data.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			}
		}

		#endregion

		#region OrgCompanyDataUniqueIndexFailureHandler

		public void TestUniqueIndexFailureHandlerShowInformationMessageWhenIsValidDifferent()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAABBB";
			org.CompanyDataCollection.RemoveAll();
			factory.Save();
			Assert("Shouldn't be in database", !org.CompanyData.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);
			orgInNewFactory.CompanyData.MarkLightValidationAsValidForTesting();
			newFactory.Save();
			Assert("Should be in database", orgInNewFactory.CompanyData.IsInDatabase);
			Assert("orgInNewFactory.IsValid should be true", (orgInNewFactory.CompanyData as ILightValidationInternals).IsValid);
			Assert("org.IsValid should be false", !(org.CompanyData as ILightValidationInternals).IsValid);
			Assert("Should be data equals", orgInNewFactory.CompanyData.DataEquals(org.CompanyData));

			var notification = new NotificationHandlerForTest();
			var failureHandler = new OrgCompanyData.OrgCompanyDataUniqueIndexFailureHandler(org.CompanyData);

			AssertExceptionThrown("ZSaveException should be thrown", typeof(ZSaveException), () =>
			{
				factory.Save();
			});

			failureHandler.NotifyUserAndAttemptToResolve(notification, failureHandler.HandledUniqueIndexNames.Single());
			Assert(notification.Message.Contains("While you were working, the organization AAABBB had its AR/AP information updated. The system will now need to reload this information. Press OK to have this information loaded and then try saving again."));
			Assert(notification.ReportErrorCount == 0 && notification.ReportInformationCount == 1);
			AssertEquals("CompanyData should be the same now", org.CompanyData.PK, orgInNewFactory.CompanyData.PK);
			Assert("IsValid should be true now", (org.CompanyData as ILightValidationInternals).IsValid);
			AssertNoExceptionThrown(() => factory.Save());
		}

		public void TestNotDeletingCompanyDataIndependentCollectionsOnUniqueIndexFailureHandler()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAAAAA";
			org.CompanyDataCollection.RemoveAll();
			factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);
			orgInNewFactory.CompanyData.MarkLightValidationAsValidForTesting();
			orgInNewFactory.CompanyData.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, Core.Constants.FreightShipmentDirection.Code.All, Core.Constants.TransportModes.All, Core.Constants.CurrencyCodes.Australia, 5m, 0.1m);
			CreateEInvoicingTemplateFileView(orgInNewFactory.CompanyData);
			orgInNewFactory.CompanyData.EInvoicingTemplateFileConfigurations.Load();
			orgInNewFactory.CompanyData.RateFeeChargeLevels.AddNew();
			newFactory.Save();

			using (AccountingMasterFilesRegistry.Instance.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ErrorReporter.Clear();
				factory.Save();

				var failureHandler = new OrgCompanyData.OrgCompanyDataUniqueIndexFailureHandler(org.CompanyData);
				failureHandler.NotifyUserAndAttemptToResolve(null, failureHandler.HandledUniqueIndexNames.Single());
				
				AssertEquals("Should not report the DeletingCFXConfiguration error when the deletion of AccCFXConfigurations is being ignored.", string.Empty, ErrorReporter.LastKeyReported);

				org.CompanyData.AccCFXConfigurations.Load();
				AssertEquals("The created CFXConfiguration should exist", 1, org.CompanyData.AccCFXConfigurations.Count);
			}

			org.CompanyData.EInvoicingTemplateFileConfigurations.Load();
			AssertEquals("The created AccEInvoicingTemplateFileView should exist", 1, org.CompanyData.EInvoicingTemplateFileConfigurations.Count);
			AssertEquals("The created OrgRateFeeChargeLevel should exist", 1, org.CompanyData.RateFeeChargeLevels.Count);

			AccEInvoicingTemplateFileView CreateEInvoicingTemplateFileView(OrgCompanyData companyData)
			{
				var configuration = Factory.New<AccEInvoicingTemplateFileView>();
				var templateFile = CreateNewTemplate("SHA");
				configuration.ETF_GC = companyData.OB_GC;
				configuration.ETF_JobType = "SHP";
				configuration.ETF_ServiceDirection = "ALL";
				configuration.ETF_TransportMode = "AIR";
				configuration.ETF_ConfigType = "TES";
				configuration.ETF_Ledger = LedgerTypes.AccountsReceivable;
				configuration.ETF_TemplateCode = templateFile.TFS_Code;
				configuration.ETF_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
				configuration.ETF_ParentID = companyData.OB_OH;
				Factory.Save();
				return configuration;
			}
		}
		public class NotificationHandlerForTest : INotificationHandler
		{
			public NotificationHandlerForTest()
			{
				Message = string.Empty;
			}

			public string Message
			{
				get;
				private set;
			}

			public int ReportErrorCount
			{
				get;
				private set;
			}

			public int ReportInformationCount
			{
				get;
				private set;
			}

			public void Reset()
			{
				Message = string.Empty;
				ReportErrorCount = 0;
				ReportInformationCount = 0;
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				ReportErrorCount++;
				Message += string.Format("ReportError={0}-{1}\r\n", message, caption);
			}
			public void ReportInformation(string message, string caption)
			{
				ReportInformationCount++;
				Message += string.Format("ReportInformation={0}-{1}\r\n", message, caption);
			}
		}

		#endregion

		#region AR Client Number

		public void TestARClientNumber()
		{
			Org.OH_IsDebtor = false;
			Factory.Save();

			AssertNullOrEmpty(Org.CompanyData.OB_ARClientNumber);

			Org.OH_IsDebtor = true;
			Factory.Save();

			AssertNotNullOrEmpty(Org.CompanyData.OB_ARClientNumber);
			AssertEquals("00001000", Org.CompanyData.OB_ARClientNumber);
		}

		public void TestARClientNumberReadOnly()
		{
			Assert("Client Number should always be readonly", Org.CompanyData.OB_ARClientNumberInfo.ReadOnly);
		}

		#endregion

		public void TestChangingAirCarrierFlag()
		{
			bool oldFlagValue = Env.Security.OrgCarrierModifyAir.IsAllowed;
			OrgInDB.OH_IsAirLine = false;

			try
			{
				Env.Security.OrgCarrierModifyAir.IsAllowed = false;
				OrgInDB.OH_IsAirLine = true;
				AssertEquals("OB_APAirlineAccountNumberInfo should be read only", ZBool.True, OrgInDB.CompanyData.OB_APAirlineAccountNumberInfo.ReadOnly);

				Env.Security.OrgCarrierModifyAir.IsAllowed = true;
				// Need to trigger change to force re-calc
				OrgInDB.OH_IsAirLine = false;
				OrgInDB.OH_IsAirLine = true;
				AssertEquals("OB_APAirlineAccountNumberInfo should not be read only", ZBool.False, OrgInDB.CompanyData.OB_APAirlineAccountNumberInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModifyAir.IsAllowed = oldFlagValue;
			}
		}

		public void TestChangingSeaCarrierFlag()
		{
			bool oldFlagValue = Env.Security.OrgCarrierModifySea.IsAllowed;

			try
			{
				Env.Security.OrgCarrierModifySea.IsAllowed = false;
				AssertEquals("OB_CRIsShipsAgencyPrincipal should be read only", ZBool.True, OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);

				Env.Security.OrgCarrierModifySea.IsAllowed = true;
				AssertEquals("OB_CRIsShipsAgencyPrincipal should not be read only", ZBool.False, OrgInDB.CompanyData.OB_CRIsShipsAgencyPrincipalInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModifySea.IsAllowed = oldFlagValue;
			}
		}

		public void TestTriggerCanBeAppliedOnOrgHeader_WhenHaveProperConditionValue()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.OrganisationCode;
			template.P0_GB = branch.PK;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"test\")";

			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = header.PK;
			Factory.Save();

			var logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));
			AssertEquals(0, logs.Length);

			orgCompanyData.Delete();
			Factory.Save();

			trigger.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"AR/AP Details\")";
			orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = header.PK;
			Factory.Save();

			logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));
			AssertEquals(0, logs.Length);
		}

		public void TestIFCTriggerActionToSetOB_APVATConfig_DoesNotShowMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = org.PK;
			orgCompanyData.OB_IsCreditor = true;
			Factory.Save();

			var messageShown = string.Empty;
			org.ShowMessage += new OrgHeader.ShowMessageEventHandler((string caption, string message) => messageShown = caption + "|" + message);

			org.CompanyData.OB_APVATConfig = "ACR";
			AssertEquals("Pre-condition: message shown", "Changing Creditor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Creditor will NOT change.", messageShown);

			var trigger = org.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<CompanyData.OB_APVATConfig>";
			action.PQ_FieldValue = "CSH";

			messageShown = string.Empty;

			AssertEquals("Trigger has not fired yet", true, trigger.LastFiredTime.IsEmpty);

			Factory.Save();

			AssertEquals("message shown", string.Empty, messageShown);
			AssertEquals("Trigger still won't fire", true, trigger.LastFiredTime.IsEmpty);
		}

		public void TestIFCTriggerActionToSetOB_ARVATConfig_DoesNotShowMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = org.PK;
			orgCompanyData.OB_IsDebtor = true;
			Factory.Save();

			var messageShown = string.Empty;
			org.ShowMessage += new OrgHeader.ShowMessageEventHandler((string caption, string message) => messageShown = caption + "|" + message);

			org.CompanyData.OB_ARVATConfig = "ACR";
			AssertEquals("Pre-condition: message shown", "Changing Debtor Tax Configuration|Note: Changing Tax Configuration will only affect NEW transactions. Transactions ALREADY POSTED for this Debtor will NOT change.", messageShown);

			var trigger = org.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<CompanyData.OB_ARVATConfig>";
			action.PQ_FieldValue = "CSH";

			messageShown = string.Empty;

			AssertEquals("Trigger has not fired yet", true, trigger.LastFiredTime.IsEmpty);

			Factory.Save();

			AssertEquals("Expected no message because ShowMessageEventHandler should not have been run", string.Empty, messageShown);
			AssertEquals("Trigger still won't fire", true, trigger.LastFiredTime.IsEmpty);
		}

		#region TestNoAuditLogForOrgCompanyData

		public void TestNoAuditLogForOrgCompanyData()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_IsDebtor = false;
			Factory.Save();

			companyData.OB_IsDebtor = true;
			Factory.Save();

			companyData.Delete();
			Factory.Save();

			var auditLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent,companyData.PK));
			AssertEquals(0, auditLogs.Length);
		}

		#endregion

		#region Implementation

		OrgHeader Org;
		OrgCompanyData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Data = Org.CompanyData;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return org.CompanyData;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			return org.CompanyData;
		}

		protected OrgInvoiceType AddInvoiceTypeToCompanyData(OrgCompanyData companyData, ZString jobTypeCode, ZString transportMode, ZString serviceDirection, ZString serviceLevel, ZString type, ZString secondaryType)
		{
			var invoiceType = companyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = jobTypeCode;
			invoiceType.PI_ServiceDirection = serviceDirection;
			invoiceType.PI_TransportMode = transportMode;
			invoiceType.PI_RS_NKServiceLevel = serviceLevel;
			invoiceType.PI_Type = type;
			invoiceType.PI_SecondaryType = secondaryType;
			return invoiceType;
		}

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string ledger, string transactionType, decimal outstandingAmount, GlbBranch branch)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_OutstandingAmount = outstandingAmount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = branch.PK;
			invoice.AH_TransactionType = transactionType;
			return invoice;
		}

		AccTransactionLines GetNewTransactionLine(OrgHeader org, string lineType, decimal lineAmount, GlbBranch branch)
		{
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = lineType;
			line.AL_OH = org.PK;
			line.AL_GB = branch.PK;
			line.AL_LineAmount = lineAmount;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			return line;
		}

		void SetupTermsInfo(OrgARTerms term, string jobType, ZGuid branchPK, ZGuid deptPK, string direction, string transportMode, string invoiceType, string invoiceTerm, int termDays)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
				term.PY_InvoiceTerm = invoiceTerm;
				term.PY_InvoiceDays = (ZByte)termDays;
			}
		}

		#endregion

	}
}
