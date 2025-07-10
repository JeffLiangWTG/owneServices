using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryNumberGeneratorTest : TestCaseWithFactory
	{
		public void TestTryGetNextEntryNumber()
		{
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XXX");
			var wholeCompanyFountain = companyStmNums.TryGetNumberFountain();
			wholeCompanyFountain.SetNext(Factory, 100212);

			ZString entryNumber;
			EntryNumberGenerator.TryGetNextEntryNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), "XXX", out entryNumber);
			AssertEquals("01002121", entryNumber);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XXX";

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.Parent = declaration;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			cusEntryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			cusEntryNumber.CE_EntryNum = "01002139";
			Factory.Save();

			EntryNumberGenerator.TryGetNextEntryNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), "XXX", out entryNumber);
			AssertEquals("01002147", entryNumber);
		}

		public void TestFormalEntryNumber_SetFromPreAllocatedNumber()
		{
			const string entryFilerCode = "XJ5";
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber(entryFilerCode);
			var wholeCompanyFountain = companyStmNums.TryGetNumberFountain();
			wholeCompanyFountain.SetNext(Factory, 100212);
			ZString entryNumber;

			EntryNumberGenerator.TryGetNextEntryNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), entryFilerCode, out entryNumber);
			AssertEquals("01002126", entryNumber);

			EntryNumberGenerator.TryGetNextEntryNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), entryFilerCode, out entryNumber);
			AssertEquals("01002134", entryNumber);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.Parent = declaration;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			cusEntryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			cusEntryNumber.CE_EntryNum = "01002142";
			Factory.Save();

			EntryNumberGenerator.TryGetNextEntryNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), entryFilerCode, out entryNumber);
			AssertEquals("01002159", entryNumber);
		}

		public void TestFormalEntryNumberWhenSameNumberExistsForOtherEntryFilerCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "ABC";
			declaration.ImportEntryNumber = "01002126";
			Factory.Save();

			const string entryFilerCode = "XJ5";
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber(entryFilerCode);
			var wholeCompanyFountain = companyStmNums.TryGetNumberFountain();
			wholeCompanyFountain.SetNext(Factory, 100212);
			ZString entryNumber;

			EntryNumberGenerator.TryGetNextEntryNumber(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), entryFilerCode, out entryNumber);
			AssertEquals("01002126", entryNumber);
		}
	}
}
