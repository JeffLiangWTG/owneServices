using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RegistryPenaltyMatcherTest : TestCaseWithFactory
	{
		GlbCompany SetupTestCompany()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "M#@";
			company.GC_Name = "TEST COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "S$#";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			return company;
		}

		public void TestMatchStorage()
		{
			var testCompany = SetupTestCompany();

			var match = new RegistryPenaltyMatcher();
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 7 }))
			{
				AssertEquals((ZByte)7, match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Import, ProcessType = Core.Constants.ContainerPenaltyProcessType.Import }).FreeDays);
				AssertEquals(ContainerDetentionFreeDayType.CTOAvailable, match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Import, ProcessType = Core.Constants.ContainerPenaltyProcessType.Import }).FreeDayType);

				AssertEquals((ZByte)7, match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Import, ProcessType = Core.Constants.ContainerPenaltyProcessType.Delivery }).FreeDays);
				AssertNull(match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Import, CreditorType = ContainerPenaltyCreditorType.Codes.Carrier }));
			}

			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 7 }))
			{
				AssertEquals((ZByte)7, match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Export, ProcessType = Core.Constants.ContainerPenaltyProcessType.Export }).FreeDays);
				AssertEquals(ZString.Empty, match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Export, ProcessType = Core.Constants.ContainerPenaltyProcessType.Export }).FreeDayType);

				AssertEquals((ZByte)7, match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Export, ProcessType = Core.Constants.ContainerPenaltyProcessType.Pickup }).FreeDays);
				AssertNull(match.MatchStorage(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Export, CreditorType = ContainerPenaltyCreditorType.Codes.Carrier }));
			}
		}

		public void TestMatchDetention()
		{
			var testCompany = SetupTestCompany();

			var match = new RegistryPenaltyMatcher();
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 7 }))
			{
				AssertEquals((ZByte)7, match.MatchDetention(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Import, ProcessType = Core.Constants.ContainerPenaltyProcessType.Import }).FreeDays);
				AssertEquals(ContainerDetentionFreeDayType.CTOAvailable, match.MatchDetention(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Import, ProcessType = Core.Constants.ContainerPenaltyProcessType.Import }).FreeDayType);

				AssertEquals((ZByte)7, match.MatchDetention(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Import, ProcessType = Core.Constants.ContainerPenaltyProcessType.Delivery }).FreeDays);
			}

			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 7 }))
			{
				AssertEquals((ZByte)7, match.MatchDetention(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Export, ProcessType = Core.Constants.ContainerPenaltyProcessType.Export }).FreeDays);
				AssertEquals(ZString.Empty, match.MatchDetention(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Export, ProcessType = Core.Constants.ContainerPenaltyProcessType.Export }).FreeDayType);

				AssertEquals((ZByte)7, match.MatchDetention(new ContainerPenaltyMatchFilter { Company = testCompany, Direction = ContainerDetentionDirection.Export, ProcessType = Core.Constants.ContainerPenaltyProcessType.Pickup }).FreeDays);
			}
		}
	}
}
