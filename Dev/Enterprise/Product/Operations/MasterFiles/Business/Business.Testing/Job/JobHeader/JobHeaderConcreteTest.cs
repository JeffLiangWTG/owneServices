using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobHeaderConcreteTest : TestCaseWithFactory
	{
		public void TestRefreshParentWithInactiveJob()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();

			jobHeader.MarkAsInactive();

			jobHeader.Parent = (IJobHeaderParent)shipment;

			AssertEquals(false, jobHeader.IsDeleted);
			AssertEquals(false, jobHeader.JH_IsActive);
			Assert(!jobHeader.DefaultValuesHasBeenAssigned);
		}

		public void TestRefreshParentWithJobHeaderCompanyOtherThanTheLoginCompany()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			Factory.Save();

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = anotherCompany.PK;
			Factory.Save();

			JobHeader job = null;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, anotherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				Factory.Save();
			}

			var jobInAnotherCompany = Factory.Load<JobHeaderForTemporarilyLoginTest>(job.PK);
			jobInAnotherCompany.InitializeParentFromGenericJobWithSettingDefaults();

			AssertEquals(true, jobInAnotherCompany.HasInterCompanyJobOperationContextWhenSetParentCore);
			AssertEquals(false, jobInAnotherCompany.HasContext(BusinessContext.InterCompanyJobOperation));
			AssertEquals(anotherCompany.PK, jobInAnotherCompany.CurrentCompanyPKWhenSetParentCore);
		}

		public void TestRefreshParentWithJobHeaderCompanyBeTheLoginCompany()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			Factory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			var jobReload = Factory.Load<JobHeaderForTemporarilyLoginTest>(job.PK);
			jobReload.InitializeParentFromGenericJobWithSettingDefaults();

			AssertEquals(false, jobReload.HasInterCompanyJobOperationContextWhenSetParentCore);
			AssertEquals(false, jobReload.HasContext(BusinessContext.InterCompanyJobOperation));
			AssertEquals(GlbCompany.CurrentCompany.PK, jobReload.CurrentCompanyPKWhenSetParentCore);
		}

		[TestDate(2019, 12, 20, 11, 12, 13)]
		public void TestConstructorStackTrace()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			Factory.Save();

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
#if NETFRAMEWORK
			AssertContains("job's constructor stack trace is always collected", $"JobConstructorStackTrace:\r\nJob Created Time: 2019-12-20 11:12:13.000\r\n   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", jobHeader.ConstructorStackTrace);

			Factory.Save();
			AssertContains("We should still be able to get ConstructorStackTrace after Factory.Save()"
				, $"JobConstructorStackTrace:\r\nJob Created Time: 2019-12-20 11:12:13.000\r\n   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)"
				, jobHeader.ConstructorStackTrace);
#else
			AssertContains("job's constructor stack trace is always collected", $"JobConstructorStackTrace:\r\nJob Created Time: 2019-12-20 11:12:13.000\r\n   at System.Environment.get_StackTrace()", jobHeader.ConstructorStackTrace);

			Factory.Save();
			AssertContains("We should still be able to get ConstructorStackTrace after Factory.Save()"
				, $"JobConstructorStackTrace:\r\nJob Created Time: 2019-12-20 11:12:13.000\r\n   at System.Environment.get_StackTrace()"
				, jobHeader.ConstructorStackTrace);
#endif
		}

		public void TestGetGSTID_SupplyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
				orgAALSHI.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				orgAALSHI.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				var jobHeader = Factory.NewJobForTesting<JobHeader>();
				jobHeader.FillWithValidTestData();
				var jobCharge = Factory.NewWithValidTestData<JobCharge>();
				jobCharge.JR_JH = jobHeader.PK;
				jobCharge.JR_AC = chargeCode.PK;
				jobCharge.JR_OH_CostAccount = orgAALSHI.PK;
				jobCharge.JR_OH_SellAccount = orgAALSHI.PK;
				jobCharge.JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				jobCharge.JR_SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
				var testObjectCreator = new AccountingTestObjectCreator(Factory);
				var tax1 = testObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code, "Tax1");
				var tax2 = testObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code, "Tax2");

				Factory.Save();

				var override1 = chargeCode.TaxOverrides.AddNew();
				testObjectCreator.PopulateTaxOverride(override1, tax1.PK, costSellAll: AccChargeTaxOverrideLookups.Cost);
				override1.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				var override2 = chargeCode.TaxOverrides.AddNew();
				testObjectCreator.PopulateTaxOverride(override2, tax2.PK, costSellAll: AccChargeTaxOverrideLookups.Revenue);
				override2.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
				Factory.Save();

				ZGuid taxMsg;
				AssertEquals(ZGuid.Empty, jobHeader.GetGSTID(jobCharge, CostSell.Cost, out taxMsg));
				AssertEquals(ZGuid.Empty, taxMsg);
				AssertEquals(ZGuid.Empty, jobHeader.GetGSTID(jobCharge, CostSell.Revenue, out taxMsg));
				AssertEquals(ZGuid.Empty, taxMsg);
				chargeCode.ClearGSTRateCacheForTesting();
				AssertEquals(tax1.PK, jobHeader.GetGSTID(jobCharge, CostSell.Cost, out taxMsg));
				AssertEquals(tax1.AT_A9_DefaultVatClass, taxMsg);
				AssertEquals(tax2.PK, jobHeader.GetGSTID(jobCharge, CostSell.Revenue, out taxMsg));
				AssertEquals(tax2.AT_A9_DefaultVatClass, taxMsg);
			}
		}

		public void TestGetGSTID_Branch()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var testObjectCreator = new AccountingTestObjectCreator(Factory);

			var taxCurrentBranch = testObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code, "Tax1");
			var taxCostBranch = testObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code, "Tax2");
			var taxSellBranch = testObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code, "Tax3");
			var taxDefault = testObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code, "Tax4");

			var branchForCost = Factory.NewWithValidTestData<GlbBranch>();
			branchForCost.GB_Code = "CST";
			var branchForSell = Factory.NewWithValidTestData<GlbBranch>();
			branchForCost.GB_Code = "SEL";

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AT_GSTRate = ZGuid.Empty;
			chargeCode.TaxOverrides.RemoveAndDeleteAll();
			testObjectCreator.CreateTaxOverrides(chargeCode
				, taxOverride =>
				{
					taxOverride.AO_AT = taxCurrentBranch.PK;
					taxOverride.AO_GB = GlbBranch.CurrentBranch.PK;
				}
				, taxOverride =>
				{
					taxOverride.AO_AT = taxSellBranch.PK;
					taxOverride.AO_GB = branchForSell.PK;
				}
				, taxOverride =>
				{
					taxOverride.AO_AT = taxCostBranch.PK;
					taxOverride.AO_GB = branchForCost.PK;
				}
				, taxOverride =>
				{
					taxOverride.AO_AT = taxDefault.PK;
					taxOverride.AO_SupplyType = SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var orgAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			orgAALSHI.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			orgAALSHI.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.FillWithValidTestData();
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_OH_CostAccount = orgAALSHI.PK;
			jobCharge.JR_OH_SellAccount = orgAALSHI.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GB_CostTaxBranch = branchForCost.PK;
			jobCharge.JR_GB_SellTaxBranch = branchForSell.PK;
			jobCharge.JR_CostSupplyType = SupplyTypeClassificationCodes.LOC;
			jobCharge.JR_SellSupplyType = SupplyTypeClassificationCodes.LOC;

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			chargeCode.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled, GST override filter will not consider branch.", () =>
			{
				AssertEquals(taxDefault.PK, jobHeader.GetGSTID(jobCharge, CostSell.Cost, out _));
				AssertEquals(taxDefault.PK, jobHeader.GetGSTID(jobCharge, CostSell.Revenue, out _));
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			chargeCode.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is not enabled, GST override filter will not consider branch.", () =>
			{
				AssertEquals(taxDefault.PK, jobHeader.GetGSTID(jobCharge, CostSell.Cost, out _));
				AssertEquals(taxDefault.PK, jobHeader.GetGSTID(jobCharge, CostSell.Revenue, out _));
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			chargeCode.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations is enabled but EnableTaxBranchReporting is not enabled, GST override filter will consider JR_GB.", () =>
			{
				AssertEquals(taxCurrentBranch.PK, jobHeader.GetGSTID(jobCharge, CostSell.Cost, out _));
				AssertEquals(taxCurrentBranch.PK, jobHeader.GetGSTID(jobCharge, CostSell.Revenue, out _));
			});

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			chargeCode.ClearGSTRateCacheForTesting();
			CombineAssertions("When EnableBranchLevelTaxOverrideRuleConfigurations & EnableTaxBranchReporting are enabled, GST override filter will consider each own tax branch.", () =>
			{
				AssertEquals("JR_GB_CostTaxBranch", taxCostBranch.PK, jobHeader.GetGSTID(jobCharge, CostSell.Cost, out _));
				AssertEquals("JR_GB_SellTaxBranch", taxSellBranch.PK, jobHeader.GetGSTID(jobCharge, CostSell.Revenue, out _));
			});
		}

		public void TestResetParentScreeningStatus_AgentCollectAddrOrLocalChargesAddrChanged()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			jobHeader.Parent = (IJobHeaderParent)shipment;

			var parentProvider = shipment as IShouldUpdateScreeningStatus;
			var parentScreeningStatusProvider = shipment as IScreeningStatusProvider;
			AssertNotNull(parentProvider);
			AssertNotNull(parentScreeningStatusProvider);

			parentScreeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			parentProvider.ShouldUpdateScreeningStatus = false;
			var orgHeader = Factory.New<OrgHeader>();
			var address2 = orgHeader.Addresses.AddNew();
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			jobHeader.JH_OA_AgentCollectAddr = orgHeader.MainAddress.PK;
			AssertEquals(false, parentProvider.ShouldUpdateScreeningStatus);

			jobHeader.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;
			AssertEquals(false, parentProvider.ShouldUpdateScreeningStatus);

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			jobHeader.JH_OA_AgentCollectAddr = address2.PK;
			AssertEquals(true, parentProvider.ShouldUpdateScreeningStatus);

			parentScreeningStatusProvider.ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			parentProvider.ShouldUpdateScreeningStatus = false;
			jobHeader.JH_OA_LocalChargesAddr = address2.PK;
			AssertEquals(true, parentProvider.ShouldUpdateScreeningStatus);
		}

		public void TestNoExceptionThrowWhenParentIsNull()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			AssertNoExceptionThrown(() =>
			{
				jobHeader.JH_OA_AgentCollectAddr = orgHeader.MainAddress.PK;
			});
		}

		public void TestUniversalCopyIgnoreBusinessObjectAttribute()
		{
			var charge = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var componentType = charge.GetType();
			var ignoreBoAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreBusinessObjectAttribute), true);
			AssertNotNull("UniversalCopyIgnoreBusinessObject Attribute", ignoreBoAttributes[0] as UniversalCopyIgnoreBusinessObjectAttribute);
		}

		public void TestGetCreateOrActivateJobMutex()
		{
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = anotherCompany.PK;
			Factory.Save();

			AssertGetCreateOrActivateJobMutex(new ZGuid("1E6A3EC7-1E91-437B-8F8F-312943728CF8"), GlbBranch.CurrentBranch);
			AssertGetCreateOrActivateJobMutex(new ZGuid("2479D2F7-C874-4354-8B09-6084D670E3ED"), anotherBranch);

			void AssertGetCreateOrActivateJobMutex(ZGuid testingPK, GlbBranch branch)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var mutex = JobHeader.GetCreateOrActivateJobMutex(testingPK);
					AssertEquals(MutexIDs.JobBeingCreatedForShipment, mutex.MutexID);
					AssertEquals(testingPK + "_" + branch.Company.GC_Code, mutex.RecordIdentifier);
				}
			}
		}

		#region Implementation

		class JobHeaderForTemporarilyLoginTest : JobHeader
		{
			public JobHeaderForTemporarilyLoginTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsChargesCollectionLoaded => false;

			public override ZDecimal JH_TotalProfitRevenueMargin => throw new NotImplementedException();

			protected override void SetParentCore(IJobHeaderParent value)
			{
				HasInterCompanyJobOperationContextWhenSetParentCore = this.HasContext(BusinessContext.InterCompanyJobOperation);
				CurrentCompanyPKWhenSetParentCore = GlbCompany.CurrentCompany.PK;
			}

			public bool HasInterCompanyJobOperationContextWhenSetParentCore { get; set; }
			public ZGuid CurrentCompanyPKWhenSetParentCore { get; set; }
		}

		#endregion
	}
}
