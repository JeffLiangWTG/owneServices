using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(InBondNumberRange))]
	sealed class InBondNumberRangeTest : RegistryBusinessObjectTemplateTestCase<InBondNumberRange>
	{
		public void TestStartNumberForDisplay()
		{
			var (newCompany, branch) = SetupCompanyAndBranch();

			var range = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);

			range.StartNumber = 111111111;
			AssertEquals("StartNumberForDisplay", "111111111", range.StartNumberForDisplay);

			range.StartNumber = 111;
			AssertEquals("StartNumberForDisplay", "00000111", range.StartNumberForDisplay);

			range.StartNumberForDisplay = "111";
			AssertEquals("StartNumberForDisplay", 111m, range.StartNumber);

			range.StartNumberForDisplay = "A2B2C2.3";
			AssertEquals("StartNumberForDisplay", 2223m, range.StartNumber);
		}

		public void TestLastNumberForDisplay()
		{
			var (newCompany, branch) = SetupCompanyAndBranch();

			var range = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);

			range.LastNumber = 111111111;
			AssertEquals("EndNumberForDisplay", "111111111", range.LastNumberForDisplay);

			range.LastNumber = 111;
			AssertEquals("EndNumberForDisplay", "00000111", range.LastNumberForDisplay);

			range.LastNumberForDisplay = "111";
			AssertEquals("LastNumberForDisplay", 111m, range.LastNumber);

			range.LastNumberForDisplay = "A2B2C2.3";
			AssertEquals("LastNumberForDisplay", 2223m, range.LastNumber);
		}

		#region Test Validations

		public void TestValidateStartNumber()
		{
			var (newCompany, branch) = SetupCompanyAndBranch();

			InBondNumberRange range = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			range.RunOutWarningLimitNumber = 300;
			range.LastNumber = MaximumNumberRange - 659;
			range.StartNumber = 0;
			const string expectedOtherBranchError = "This Start Number overlaps Dummy Branch's Start Number (1000001) and Last Number (2000000).";
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberInfo, expectedOtherBranchError);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberForDisplayInfo, expectedOtherBranchError);

			range.StartNumber = -1;
			AssertHasErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberInfo, expectedOtherBranchError);
			AssertHasErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberForDisplayInfo, expectedOtherBranchError);

			range.StartNumber = MaximumNumberRange;
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertHasErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberInfo, expectedOtherBranchError);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertHasErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberForDisplayInfo, expectedOtherBranchError);

			range.StartNumber = MaximumNumberRange - 1;
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberInfo, expectedOtherBranchError);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberForDisplayInfo, expectedOtherBranchError);

			range.StartNumber = MinimumNumberRange;
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberInfo, expectedOtherBranchError);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertNoError(range.StartNumberForDisplayInfo, expectedOtherBranchError);

			BusinessObject newBranch = (BusinessObject)Factory.New<IGlbBranch>();
			newBranch[GlbBranchSchema.GB_BranchName] = "Dummy Branch";
			newBranch[GlbBranchSchema.GB_Code] = "Z@Z";
			newBranch[GlbBranchSchema.GB_GC] = newCompany.PK;
			newBranch[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;
			Factory.Save();
			InBondNumberRange otherRange = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty);
			otherRange.RunOutWarningLimitNumber = 300;
			otherRange.StartNumber = 1000001;
			otherRange.LastNumber = 2000000;
			NumberRangeRegistryItem.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, otherRange);

			range.StartNumber = 1500000;
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertHasError(range.StartNumberInfo, expectedOtherBranchError);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeGreaterThanOrEqualMinimum);
			AssertNoErrorContaining(range.StartNumberForDisplayInfo, InBondNumberRange.StartNumberMustBeLessOrEqualToMaximumMinusOne);
			AssertHasError(range.StartNumberForDisplayInfo, expectedOtherBranchError);
		}

		public void TestValidateLastNumber()
		{
			var (newCompany, branch) = SetupCompanyAndBranch();

			InBondNumberRange range = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			range.RunOutWarningLimitNumber = 300;
			range.StartNumber = 1000;
			range.LastNumber = 4;
			const string expectedOtherBranchError = "This Last Number overlaps Dummy Branch's Start Number (1000001) and Last Number (2000000).";
			AssertHasError(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertNoErrorContaining(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertNoError(range.LastNumberInfo, expectedOtherBranchError);
			AssertHasError(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertNoErrorContaining(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertNoError(range.LastNumberForDisplayInfo, expectedOtherBranchError);

			range.LastNumber = MaximumNumberRange + 1;
			AssertNoError(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertHasErrorContaining(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertNoError(range.LastNumberInfo, expectedOtherBranchError);
			AssertNoError(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertHasErrorContaining(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertNoError(range.LastNumberForDisplayInfo, expectedOtherBranchError);

			range.LastNumber = MaximumNumberRange;
			AssertNoError(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertNoErrorContaining(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertNoError(range.LastNumberInfo, expectedOtherBranchError);
			AssertNoError(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertNoErrorContaining(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertNoError(range.LastNumberForDisplayInfo, expectedOtherBranchError);

			BusinessObject newBranch = (BusinessObject)Factory.New<IGlbBranch>();
			newBranch[GlbBranchSchema.GB_BranchName] = "Dummy Branch";
			newBranch[GlbBranchSchema.GB_Code] = "Z@Z";
			newBranch[GlbBranchSchema.GB_GC] = newCompany.PK;
			newBranch[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;
			Factory.Save();

			InBondNumberRange otherRange = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty);
			otherRange.RunOutWarningLimitNumber = 300;
			otherRange.StartNumber = 1000001;
			otherRange.LastNumber = 2000000;
			NumberRangeRegistryItem.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, otherRange);

			range.LastNumber = 1500000;
			AssertNoError(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertNoErrorContaining(range.LastNumberInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertHasError(range.LastNumberInfo, expectedOtherBranchError);
			AssertNoError(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeGreaterThanStartNumber);
			AssertNoErrorContaining(range.LastNumberForDisplayInfo, InBondNumberRange.LastNumberMustBeLessOrEqualToMaximum);
			AssertHasError(range.LastNumberForDisplayInfo, expectedOtherBranchError);
		}

		public void TestValidateStartNumberForDisplay()
		{
			var (newCompany, branch) = SetupCompanyAndBranch();

			InBondNumberRange range = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			range.RunOutWarningLimitNumber = 300;
			range.LastNumberForDisplay = (MaximumNumberRange - 500).ToString();
			range.StartNumberForDisplay = "1";
			const string expectedZeroError = "Please enter a Start Number.";
			AssertNoError(range.StartNumberForDisplayInfo, expectedZeroError);

			range.StartNumberForDisplay = "AAA";
			AssertHasError(range.StartNumberForDisplayInfo, expectedZeroError);
			Assert(range.StartNumber.ToZInt().Equals(0));
		}

		public void TestValidateLastNumberForDisplay()
		{
			var (newCompany, branch) = SetupCompanyAndBranch();

			InBondNumberRange range = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
			range.RunOutWarningLimitNumber = 300;
			range.StartNumberForDisplay = "1000";
			range.LastNumberForDisplay = "4";

			range.LastNumberForDisplay = "BBB";
			Assert(range.LastNumber.ToZInt().Equals(0));
		}

		public void TestIsRangeValidForNumberFountain()
		{
			InBondNumberRange range = GetBusinessObjectToClone();
			range.StartNumber = 0;
			range.LastNumber = 0;
			AssertEquals("IsRangeValidForNumberFountain", false, range.IsRangeValidForNumberFountain);
			range.LastNumber = 10;
			AssertEquals("IsRangeValidForNumberFountain", false, range.IsRangeValidForNumberFountain);
			range.StartNumber = 11;
			AssertEquals("IsRangeValidForNumberFountain", false, range.IsRangeValidForNumberFountain);
			range.StartNumber = 1;
			AssertEquals("IsRangeValidForNumberFountain", true, range.IsRangeValidForNumberFountain);
		}

		public void TestValidateRunOutWarningLimitNumber()
		{
			BusinessObject newCompany = (BusinessObject)Factory.New<IGlbCompany>();
			newCompany[GlbCompanySchema.GC_Name] = "Dummy Company";
			newCompany[GlbCompanySchema.GC_Code] = "Z@Z";
			newCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;
			Factory.Save();

			InBondNumberRange range = NumberRangeRegistryItem.GetValueWithoutFallback(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			range.StartNumber = 1000;
			range.LastNumber = 4000;
			range.ValidateRunOutWarningLimitNumber();
			AssertHasErrorContaining(range.RunOutWarningLimitNumberInfo, "Please enter a Run-out warning limit number");

			range.RunOutWarningLimitNumber = 3500;
			range.ValidateRunOutWarningLimitNumber();
			AssertNoErrorContaining(range.RunOutWarningLimitNumberInfo, "Please enter a Run-out warning limit number");
			AssertHasWarning(range.RunOutWarningLimitNumberInfo, InBondNumberRange.RunOutWarningLimitGreaterThanCurrentRange + "3000).");

			range.RunOutWarningLimitNumber = 300;
			range.ValidateRunOutWarningLimitNumber();
			AssertNoWarning(range.RunOutWarningLimitNumberInfo, InBondNumberRange.RunOutWarningLimitGreaterThanCurrentRange + "3000).");
		}

		public void TestOnlyCompanyOrBranchRegistryCanBeSetup()
		{
			var newCompany = (BusinessObject)Factory.New<IGlbCompany>();
			newCompany[GlbCompanySchema.GC_Name] = "US Company";
			newCompany[GlbCompanySchema.GC_Code] = "Z@Z";
			newCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;

			var branchCHI = (BusinessObject)Factory.New<IGlbBranch>();
			branchCHI[GlbBranchSchema.GB_BranchName] = "Chicago Branch";
			branchCHI[GlbBranchSchema.GB_Code] = "Z@1";
			branchCHI[GlbBranchSchema.GB_GC] = newCompany.PK;
			branchCHI[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;

			var branchLAX = (BusinessObject)Factory.New<IGlbBranch>();
			branchLAX[GlbBranchSchema.GB_BranchName] = "Los Angeles Branch";
			branchLAX[GlbBranchSchema.GB_Code] = "Z@2";
			branchLAX[GlbBranchSchema.GB_GC] = newCompany.PK;
			branchLAX[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;
			Factory.Save();

			var currentCompanyRange = NumberRangeRegistryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			currentCompanyRange.StartNumber = 1;
			currentCompanyRange.LastNumber = 5000;
			NumberRangeRegistryItem.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, currentCompanyRange);

			var companyRange = NumberRangeRegistryItem.GetValueWithoutFallback(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			companyRange.StartNumber = 1;
			companyRange.LastNumber = 5000;
			NumberRangeRegistryItem.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyRange);
			Factory.Save();

			var cHIrange = NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, branchCHI.PK.ToGuid(), Guid.Empty);
			cHIrange.StartNumber = 1;
			AssertHasError(cHIrange.StartNumberInfo, cHIrange.ConflictingCompanyAndBranchRanges);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override InBondNumberRange GetBusinessObjectToClone()
		{
			InBondNumberRange result = new InBondNumberRange();

			result.StartNumber = MinimumNumberRange;
			result.LastNumber = MaximumNumberRange;

			return result;
		}

		protected override InBondNumberRange GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		InBondNumberRangeRegistryItem NumberRangeRegistryItem
		{
			get { return USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange; }
		}

		long MinimumNumberRange
		{
			get { return NumberFountains.USMinimumInBondNumber; }
		}

		long MaximumNumberRange
		{
			get { return NumberFountains.USMaximumInBondNumber; }
		}

		(BusinessObject newCompany, BusinessObject branch) SetupCompanyAndBranch()
		{
			BusinessObject newCompany = (BusinessObject)Factory.New<IGlbCompany>();
			newCompany[GlbCompanySchema.GC_Name] = "Dummy Compay";
			newCompany[GlbCompanySchema.GC_Code] = "Z@Z";
			newCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;

			BusinessObject branch = (BusinessObject)Factory.New<IGlbBranch>();
			branch[GlbBranchSchema.GB_BranchName] = "Dummy Branch 1";
			branch[GlbBranchSchema.GB_Code] = "Z@1";
			branch[GlbBranchSchema.GB_GC] = newCompany.PK;
			branch[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;
			Factory.Save();

			return (newCompany, branch);
		}

		#endregion
	}
}
