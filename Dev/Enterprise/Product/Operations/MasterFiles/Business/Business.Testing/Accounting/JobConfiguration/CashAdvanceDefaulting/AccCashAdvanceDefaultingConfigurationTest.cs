using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCashAdvanceDefaultingConfiguration))]
	sealed class AccCashAdvanceDefailtingConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var companyConfig = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			companyConfig.CAC_ParentTableCode = ZString.Empty;

			var branchConfig = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			branchConfig.CAC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			branchConfig.CAC_ParentId = GlbBranch.CurrentBranch.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var organizationConfig = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			organizationConfig.CAC_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			organizationConfig.CAC_ParentId = orgHeader.PK;
			organizationConfig.CAC_Ledger = LedgerTypes.AccountsReceivable;

			Assert("Config outside of collection is always editable", !companyConfig.ReadOnly);

			var companyCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK);
			companyCollection.Add(companyConfig);
			Assert("Company level config in company collection is editable", !companyConfig.ReadOnly);
			companyCollection.RemoveAll();

			var branchCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK);
			branchCollection.Add(companyConfig);
			branchCollection.Add(branchConfig);
			Assert("Company level config in branch collection is read only", companyConfig.ReadOnly);
			Assert("Branch level config in branch collection is editable", !branchConfig.ReadOnly);
			branchCollection.RemoveAll();

			var organizationCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, orgHeader.PK, LedgerTypes.AccountsReceivable);
			organizationCollection.Add(companyConfig);
			organizationCollection.Add(branchConfig);
			organizationCollection.Add(organizationConfig);
			Assert("Company level config in organization collection is read only", companyConfig.ReadOnly);
			Assert("Branch level config in organization collection is read only", branchConfig.ReadOnly);
			Assert("Organization level config in organization collection is editable", !organizationConfig.ReadOnly);
			organizationCollection.RemoveAll();
		}

		public void TestLedgerIsReadOnlyForDebtorAndCreditorLevels()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();

			config.CAC_ParentTableCode = ZString.Empty;
			AssertEquals(AccCashAdvanceDefaultingLevel.Company, config.Level);
			Assert(!config.CAC_LedgerInfo.ReadOnly);

			config.CAC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			AssertEquals(AccCashAdvanceDefaultingLevel.Branch, config.Level);
			Assert(!config.CAC_LedgerInfo.ReadOnly);

			config.CAC_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			config.CAC_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(AccCashAdvanceDefaultingLevel.Debtor, config.Level);
			Assert(config.CAC_LedgerInfo.ReadOnly);

			config.CAC_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(AccCashAdvanceDefaultingLevel.Creditor, config.Level);
			Assert(config.CAC_LedgerInfo.ReadOnly);
		}

		public void TestSyncChargeCodes()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TST1";
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TST2";
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_Code = "TST3";

			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			var chargeCodePivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			chargeCodePivot.JCT_JCF_JobConfig = config.PK;
			chargeCodePivot.JCT_ParentId = chargeCode1.PK;
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedConfig = reloadFactory.Load<AccCashAdvanceDefaultingConfiguration>(config.PK);

			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK }, reloadedConfig.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));

			reloadedConfig.SyncChargeCodes(new[] { chargeCode2.PK, chargeCode3.PK });
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK, chargeCode3.PK }, reloadedConfig.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));
		}

		public void TestUnsyncChargeCodes()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TST1";
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TST2";
			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_Code = "TST3";

			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			var chargeCodePivot1 = Factory.New<CashAdvanceDefaultingChargeCode>();
			chargeCodePivot1.JCT_JCF_JobConfig = config.PK;
			chargeCodePivot1.JCT_ParentId = chargeCode1.PK;
			var chargeCodePivot2 = Factory.New<CashAdvanceDefaultingChargeCode>();
			chargeCodePivot2.JCT_JCF_JobConfig = config.PK;
			chargeCodePivot2.JCT_ParentId = chargeCode2.PK;
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedConfig = reloadFactory.Load<AccCashAdvanceDefaultingConfiguration>(config.PK);

			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK }, reloadedConfig.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));

			AssertNoExceptionThrown(() => config.UnsyncChargeCodes(new[] { chargeCode3.PK }));
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK }, reloadedConfig.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));

			reloadedConfig.UnsyncChargeCodes(new[] { chargeCode2.PK });
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK }, reloadedConfig.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.JCT_ParentId));
		}

		public void TestChargeGroupPivotsAndChargeCodePivotsAreDeletedWhenConfigurationIsDeleted()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			var chargeCodePivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			chargeCodePivot.JCT_JCF_JobConfig = config.PK;
			chargeCodePivot.JCT_ParentId = chargeCode.PK;
			var chargeGroupPivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			chargeGroupPivot.JCT_JCF_JobConfig = config.PK;
			chargeGroupPivot.JCT_Code = "FRT";
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedConfig = reloadFactory.Load<AccCashAdvanceDefaultingConfiguration>(config.PK);
			var reloadedChargeCodePivot = reloadFactory.Load<CashAdvanceDefaultingChargeCode>(chargeCodePivot.PK);
			var reloadedChargeGroupPivot = reloadFactory.Load<CashAdvanceDefaultingChargeGroup>(chargeGroupPivot.PK);
			var reloadedChargeCode = reloadFactory.Load<AccChargeCode>(chargeCode.PK);

			AssertContainsExactElementsInAnyOrder(new[] { reloadedChargeCodePivot.PK }, reloadedConfig.ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { reloadedChargeCode.PK }, reloadedConfig.LinkedChargeCodes.Cast<AccChargeCode>().Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { reloadedChargeGroupPivot.PK }, reloadedConfig.ChargeGroups.Cast<CashAdvanceDefaultingChargeGroup>().Select(x => x.PK));

			reloadedConfig.Delete();

			AssertEquals(0, reloadedConfig.ChargeCodes.Count);
			AssertEquals(0, reloadedConfig.LinkedChargeCodes.Count);
			AssertEquals(0, reloadedConfig.ChargeGroups.Count);

			AssertNoExceptionThrown(() => Factory.Save());

			Assert(reloadedChargeCodePivot.IsDeleted);
			Assert(!reloadedChargeCode.IsDeleted);
			Assert(reloadedChargeGroupPivot.IsDeleted);
			Assert(reloadedConfig.IsDeleted);
		}

		#region DefaultingOptionValueChange

		public void TestWhenDefaultingOptionIsSetToALLChargeCodesAndChargeGroupsAreRemovedAndDeleted()
		{
			AssertDefaultingOptionValueChange(CashAdvanceDefaultingOption.All);
		}

		public void TestWhenDefaultingOptionIsSetToNONChargeCodesAndChargeGroupsAreRemovedAndDeleted()
		{
			AssertDefaultingOptionValueChange(CashAdvanceDefaultingOption.None);
		}

		public void TestWhenDefaultingOptionIsSetToINCChargeCodesAndChargeGroupsAreNotRemovedAndDeleted()
		{
			AssertDefaultingOptionValueChange(CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups);
		}

		void AssertDefaultingOptionValueChange(ZString optionToTest)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TESTCODE";

			var configuration = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			configuration.CAC_ConfigType = JobConfiguration.TypeCodes.CashAdvanceDefaulting;
			configuration.CAC_GC = Env.CurrentCompanyPK;
			configuration.CAC_Ledger = LedgerTypes.AccountsReceivable;
			configuration.CAC_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			configuration.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			configuration.CAC_TransportMode = Core.Constants.TransportModes.Sea;
			configuration.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			var chargeCodePivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			chargeCodePivot.JCT_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			chargeCodePivot.JCT_ParentId = chargeCode.PK;
			chargeCodePivot.JCT_JCF_JobConfig = configuration.PK;
			configuration.ChargeCodes.Add(chargeCodePivot);

			var chargeGroupPivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			chargeGroupPivot.JCT_Code = ChargeCodeGroupList.Codes.Freight;
			chargeGroupPivot.JCT_JCF_JobConfig = configuration.PK;
			configuration.ChargeGroups.Add(chargeGroupPivot);

			configuration.LinkedChargeCodes.Load();

			AssertEquals(1, configuration.ChargeCodes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCodePivot }, configuration.ChargeCodes);
			AssertEquals(1, configuration.LinkedChargeCodes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode }, configuration.LinkedChargeCodes);
			AssertEquals(1, configuration.ChargeGroups.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeGroupPivot }, configuration.ChargeGroups);

			configuration.CAC_DefaultingOption = optionToTest;

			if (optionToTest == CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups)
			{
				AssertEquals(1, configuration.ChargeCodes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { chargeCodePivot }, configuration.ChargeCodes);
				AssertEquals(1, configuration.LinkedChargeCodes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { chargeCode }, configuration.LinkedChargeCodes);
				AssertEquals(1, configuration.ChargeGroups.Count);
				AssertContainsExactElementsInAnyOrder(new[] { chargeGroupPivot }, configuration.ChargeGroups);
			}
			else
			{
				AssertEquals(0, configuration.ChargeCodes.Count);
				Assert(chargeCodePivot.IsDeleted);
				AssertEquals(0, configuration.LinkedChargeCodes.Count);
				Assert(!chargeCode.IsDeleted);
				AssertEquals(0, configuration.ChargeGroups.Count);
				Assert(chargeGroupPivot.IsDeleted);
			}
		}

		#endregion

		public void TestHumanReadableName()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			AssertEquals("Advance Payment Configuration", config.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			AssertEquals(JobConfiguration.TypeCodes.CashAdvanceDefaulting, config.CAC_ConfigType);
			AssertEquals(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, config.CAC_JobType);
			AssertEquals(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, config.CAC_TransportMode);
			AssertEquals(Core.Constants.FreightShipmentDirection.Code.All, config.CAC_ServiceDirection);
			AssertEquals(GlbCompany.CurrentCompany.PK, config.CAC_GC);
			AssertEquals(ZString.Empty, config.CAC_Ledger);
			AssertEquals(CashAdvanceDefaultingOption.All, config.CAC_DefaultingOption);
		}

		public void TestLevelAndLevelName()
		{
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();

			config.CAC_ParentTableCode = ZString.Empty;
			AssertEquals(AccCashAdvanceDefaultingLevel.Company, config.Level);
			AssertEquals("Company", config.LevelName);

			config.CAC_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			AssertEquals(AccCashAdvanceDefaultingLevel.Branch, config.Level);
			AssertEquals("Branch", config.LevelName);

			config.CAC_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			config.CAC_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(AccCashAdvanceDefaultingLevel.Debtor, config.Level);
			AssertEquals("Debtor", config.LevelName);

			config.CAC_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(AccCashAdvanceDefaultingLevel.Creditor, config.Level);
			AssertEquals("Creditor", config.LevelName);

			config.CAC_ParentTableCode = "123";
			AssertEquals(AccCashAdvanceDefaultingLevel.Null, config.Level);
			AssertEquals("", config.LevelName);
		}

		public void TestIsDuplicate()
		{
			var bizo = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			bizo.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;
			var duplicate = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			duplicate.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;
			AssertNotEquals("Precondition: two different bizos", bizo.PK, duplicate.PK);

			Assert("Bizo should not be a duplicate of itself", !duplicate.IsDuplicateOf(duplicate));

			duplicate.CAC_GC = ZGuid.NewZGuid();
			Assert("Duplicate should consider GC", !duplicate.IsDuplicateOf(bizo));
			duplicate.CAC_GC = bizo.CAC_GC;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.CAC_ParentTableCode = "GB";
			Assert("Duplicate should consider ParentTableCode", !duplicate.IsDuplicateOf(bizo));
			duplicate.CAC_ParentTableCode = bizo.CAC_ParentTableCode;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.CAC_ParentId = ZGuid.NewZGuid();
			Assert("Duplicate should consider ParentId", !duplicate.IsDuplicateOf(bizo));
			duplicate.CAC_ParentId = bizo.CAC_ParentId;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.CAC_Ledger = LedgerTypes.AccountsPayable;
			Assert("Duplicate should consider Ledger", !duplicate.IsDuplicateOf(bizo));
			duplicate.CAC_Ledger = bizo.CAC_Ledger;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.CAC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Assert("Duplicate should consider JopbType", !duplicate.IsDuplicateOf(bizo));
			duplicate.CAC_JobType = bizo.CAC_JobType;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Domestic;
			Assert("Duplicate should consider Direction", !duplicate.IsDuplicateOf(bizo));
			duplicate.CAC_ServiceDirection = bizo.CAC_ServiceDirection;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.CAC_TransportMode = Core.Constants.TransportModes.Road;
			Assert("Duplicate should consider Transport Mode", !duplicate.IsDuplicateOf(bizo));
			duplicate.CAC_TransportMode = bizo.CAC_TransportMode;
			Assert(duplicate.IsDuplicateOf(bizo));
		}

		public void TestIJobConfigurationMemebers()
		{
			var configuration = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			configuration.CAC_ConfigType = JobConfiguration.TypeCodes.CashAdvanceDefaulting;
			configuration.CAC_GC = Env.CurrentCompanyPK;
			configuration.CAC_Ledger = LedgerTypes.AccountsReceivable;
			configuration.CAC_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			configuration.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			configuration.CAC_TransportMode = Core.Constants.TransportModes.Sea;
			configuration.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;

			var jobConfig = configuration as IJobConfiguration;
			AssertEquals(JobInvoicingConsumerTypes.Shipment.Code, jobConfig.JobType);
			AssertEquals(Core.Constants.FreightShipmentDirection.Code.Import, jobConfig.ServiceDirection);
			AssertEquals(Core.Constants.TransportModes.Sea, jobConfig.TransportMode);
			AssertEquals(true, jobConfig.IncludeOptionsForAllJobTypes);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var configuration = (AccCashAdvanceDefaultingConfiguration)base.GetNewBusinessObject();
			configuration.CAC_ConfigType = JobConfiguration.TypeCodes.CashAdvanceDefaulting;
			configuration.CAC_GC = Env.CurrentCompanyPK;
			configuration.CAC_Ledger = LedgerTypes.AccountsReceivable;
			configuration.CAC_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			configuration.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			configuration.CAC_TransportMode = Core.Constants.TransportModes.Sea;
			configuration.CAC_DefaultingOption = CashAdvanceDefaultingOption.All;
			return configuration;
		}

		#endregion
	}
}
