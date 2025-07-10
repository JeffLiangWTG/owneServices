using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeBranchOverride))]
	sealed class AccChargeBranchOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetBranchUsesChargeCodeCompany()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = BranchOverride.ChargeCode.Company.PK;
			branch1.GB_OH_OrgProxy = organisation1.PK;

			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			branch2.GB_OH_OrgProxy = organisation2.PK;
			Factory.Save();

			Func<ZString, OrgHeader> getOrganisationByBranchDefaultingRule = rule => organisation1;

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ConsolArrivalLocalTransport;
			var resultBranch = BranchOverride.GetBranch(getOrganisationByBranchDefaultingRule);
			AssertEquals("Returns correct branch when branch uses ChargeCodeCompany", branch1.PK, resultBranch.PK);

			getOrganisationByBranchDefaultingRule = rule => organisation2;
			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ConsolArrivalLocalTransport;
			resultBranch = BranchOverride.GetBranch(getOrganisationByBranchDefaultingRule);
			AssertNull("Should return null when GlbCurrentCompany is used", resultBranch);
		}

		public void TestGetBranch()
		{
			var newFactory = new BusinessObjectFactory();
			var organisation1 = newFactory.NewWithValidTestData<OrgHeader>();
			var organisation2 = newFactory.NewWithValidTestData<OrgHeader>();
			var organisation3 = newFactory.NewWithValidTestData<OrgHeader>();
			var branch1 = newFactory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_OH_OrgProxy = organisation1.PK;
			var branch2 = newFactory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_OH_OrgProxy = organisation2.PK;
			var branch3 = newFactory.NewWithValidTestData<GlbBranch>();
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_OH_OrgProxy = organisation1.PK;
			var branch4 = newFactory.NewWithValidTestData<GlbBranch>();
			branch4.GB_GC = GlbCompany.CurrentCompany.PK;
			newFactory.Save();

			AssertEquals("Works with null deligate and empty rule without exception.", null, BranchOverride.GetBranch(null));

			Func<ZString, OrgHeader> getOrganisationByBranchDefaultingRule = rule =>
				{
					OrgHeader result = null;
					if (rule == Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO)
					{
						result = organisation1;
					}
					else if (rule == Constants.ChargeCodeBranchDefaultingRule.ConsolArrivalLocalTransport)
					{
						result = organisation2;
					}
					else if (rule == Constants.ChargeCodeBranchDefaultingRule.SendingAgent)
					{
						result = organisation3;
					}

					return result;
				};

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO;
			AssertEquals("Works with null delegate with rule that needs it without exception.", null, BranchOverride.GetBranch(null));

			BranchOverride.YA_GB_SpecificBranch = branch4.PK;
			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways;
			AssertEquals("Returns specific branch defined in the override.", BranchOverride.YA_GB_SpecificBranch, BranchOverride.GetBranch(getOrganisationByBranchDefaultingRule).PK);
			AssertEquals("Returns specific branch defined in the override without exceptions if delegate is null.", BranchOverride.YA_GB_SpecificBranch, BranchOverride.GetBranch(null).PK);

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO;
			AssertEquals("Organisation1 returned by delegate is OrgProxy for more then one branch, so result is null.", null, BranchOverride.GetBranch(getOrganisationByBranchDefaultingRule));

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ConsolArrivalLocalTransport;
			var resultBranch = BranchOverride.GetBranch(getOrganisationByBranchDefaultingRule);
			AssertEquals("Returns correct branch where Organisation2 is OrgProxy", branch2.PK, resultBranch.PK);
			AssertEquals("Resulting branch should be in the same Factory as BranchOverride.", BranchOverride.Factory, resultBranch.Factory);

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SendingAgent;
			AssertEquals("There are no branches with OrgProxy Organisation3, so result null", null, BranchOverride.GetBranch(getOrganisationByBranchDefaultingRule));

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ShipmentExportBroker;
			AssertEquals("Works without expeptions when delegate returns null for arganisation.", null, BranchOverride.GetBranch(getOrganisationByBranchDefaultingRule));
		}

		public void TestYA_DirectionReadOnly()
		{
			var dummyConsumerType = new DummyConsumerType("DMY", null);
			BranchOverride.Lookups.JobTypeList.Add(dummyConsumerType);

			BranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.Export;
			BranchOverride.YA_JobType = "";
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", true, BranchOverride.YA_DirectionInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", true, BranchOverride.YA_Direction.IsEmpty);

			BranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", false, BranchOverride.YA_Direction.IsEmpty);

			dummyConsumerType.SetIsDirectionSupported(true);
			BranchOverride.YA_JobType = "DMY";
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", false, BranchOverride.YA_DirectionInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", false, BranchOverride.YA_Direction.IsEmpty);

			dummyConsumerType.SetIsDirectionSupported(false);
			BranchOverride.YA_JobType = "";
			BranchOverride.YA_JobType = "DMY";
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", true, BranchOverride.YA_DirectionInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", true, BranchOverride.YA_Direction.IsEmpty);
		}

		public void TestYA_TransportModeReadOnly()
		{
			var dummyConsumerType = new DummyConsumerType("DMY", null);
			BranchOverride.Lookups.JobTypeList.Add(dummyConsumerType);

			BranchOverride.YA_TransportMode = Constants.FreightShipmentDirection.Code.Export;
			BranchOverride.YA_JobType = "";
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", true, BranchOverride.YA_TransportModeInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", true, BranchOverride.YA_TransportMode.IsEmpty);

			BranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", false, BranchOverride.YA_TransportModeInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", false, BranchOverride.YA_TransportMode.IsEmpty);

			dummyConsumerType.SetIsTransportModeSupported(true);
			BranchOverride.YA_JobType = "DMY";
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", false, BranchOverride.YA_TransportModeInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", false, BranchOverride.YA_TransportMode.IsEmpty);

			dummyConsumerType.SetIsTransportModeSupported(false);
			BranchOverride.YA_JobType = "";
			BranchOverride.YA_JobType = "DMY";
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", true, BranchOverride.YA_TransportModeInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", true, BranchOverride.YA_TransportMode.IsEmpty);
		}

		public void TestYA_GB_SpecificBranchReadOnly()
		{
			BranchOverride.YA_GB_SpecificBranch = ZGuid.NewZGuid();
			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO;
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", true, BranchOverride.YA_GB_SpecificBranchInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", true, BranchOverride.YA_GB_SpecificBranch.IsEmpty);

			BranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways;
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", false, BranchOverride.YA_GB_SpecificBranchInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", false, BranchOverride.YA_GB_SpecificBranch.IsEmpty);

			BranchOverride.YA_DefaultingRule = "";
			AssertEquals("YA_GB_SpecificBranchInfo.ReadOnly", true, BranchOverride.YA_GB_SpecificBranchInfo.ReadOnly);
			AssertEquals("YA_GB_SpecificBranch is Empty", true, BranchOverride.YA_GB_SpecificBranch.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			BranchOverride = chargeCode.BranchOverrides.AddNew();
			BranchOverride.YA_JobType = "ALL";
			BranchOverride.YA_DefaultingRule = "SDT";
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => BranchOverride;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => BranchOverride;

		AccChargeBranchOverride BranchOverride;
	}
}
