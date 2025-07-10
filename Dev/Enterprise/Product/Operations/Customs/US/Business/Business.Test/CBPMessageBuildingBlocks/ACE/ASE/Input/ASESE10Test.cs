using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE10Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			GlbBranch branch = company.Branches.AddNew();
			branch.GB_BranchName = "BBB";

			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			var iorWrapper = OrgHeaderWrapper.New(importerOfRecord);
			iorWrapper.ZO_GB = branch.PK;
			importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB", GlbCompany.CurrentCompany.Country);

			var asese10Block = new ASESE10()
			{
				EntryFilerCode = "SV9",
				EntryNumber = "71019383",
				BondTypeCode = "8",
				ModeOfTransportationMOTCode = "41",
				ImporterOfRecord = "12-1234567AB",
				PlannedPortOfEntry = "3901"
			};

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)asese10Block).Update(declaration, notifications);
			AssertEquals("3901", declaration.US_SchDEntry);
			AssertEquals("SV9", declaration.US_EntryFilerCode);
			AssertEquals("71019383", declaration.ImportEntryNumber);
			AssertEquals("Branch PK", declaration.JE_GB, branch.PK);
			AssertEquals("Branch Name BBB", declaration.Branch.GB_BranchName, branch.GB_BranchName);
			AssertEquals("8", declaration.US_BondType);
			AssertEquals(TransportTypeList.Codes.Air, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			AssertEquals(importerOfRecord.PK, declaration.IOROrgPK);
		}
	}
}
