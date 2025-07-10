using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomsNumberViewStmNumsValidationTest : BusinessObjectValidationTestCase
	{
		[UseSnapshotProtection]
		public void TestCheckSN_MaximumValue()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNums1 = Factory.New<CustomsNumberViewStmNums>();
			stmNums1.Provider = provider;
			stmNums1.SN_Type = "CEN";
			stmNums1.SN_FountainName = "BOB NUMBER";
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums1.Wrapper).IsBranchLevel = false;
			stmNums1.SN_MinimumValue = 100L;
			stmNums1.SN_MaximumValue = 1200L;
			stmNums1.SN_Owner = provider.Parent.PK;
			var stmNums2 = Factory.New<CustomsNumberViewStmNums>();
			stmNums2.Provider = provider;
			stmNums2.SN_Type = "CEN";
			stmNums2.SN_FountainName = "BOB NUMBER";
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums2.Wrapper).IsBranchLevel = false;
			stmNums2.SN_MinimumValue = 2210L;
			stmNums2.SN_MaximumValue = 3200L;
			stmNums2.SN_Owner = provider.Parent.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(newFactory);
			var query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNums1.SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNums1.SN_Name);
			stmNums1 = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums1.Provider = provider;
			query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNums2.SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNums2.SN_Name);
			stmNums2 = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums2.Provider = provider;

			stmNums1.SN_Value = 800L;
			stmNums1.Validation.ValidateSN_MaximumValue();
			AssertNoError(stmNums1.SN_MaximumValueInfo, CustomsNumberViewStmNumsValidation.EndNumberCannotBeLessThanNextNumber);
			stmNums1.SN_MaximumValue = 700L;
			AssertHasError(stmNums1.SN_MaximumValueInfo, CustomsNumberViewStmNumsValidation.EndNumberCannotBeLessThanNextNumber);
			stmNums1.SN_MaximumValue = 2400L;
			AssertNoErrors(stmNums1.SN_MaximumValueInfo);
			AssertHasError(stmNums1.SN_MinimumValueInfo, CustomsNumberViewStmNumsValidation.ThisRangeOverlapAnotherRange(stmNums1.SN_OwnerForDisplay, 100, 2400, stmNums2.SN_OwnerForDisplay, 2210, 3200));
			stmNums1.SN_MaximumValue = 900L;
			AssertNoErrors(stmNums1.SN_MaximumValueInfo);
			AssertNoErrors(stmNums1.SN_MinimumValueInfo);

			var stmNums3 = newFactory.New<CustomsNumberViewStmNums>();
			stmNums3.Provider = provider;
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums3.Wrapper).IsBranchLevel = true;
			stmNums3.SN_Owner = GlbBranch.CurrentBranch.PK;
			stmNums3.SN_Type = "CEN";
			stmNums3.SN_FountainName = "BOB NUMBER";
			stmNums3.SN_MinimumValue = 1L;
			stmNums3.SN_MaximumValue = 1200L;
			AssertNoErrors(stmNums3.SN_MaximumValueInfo);
			AssertHasError(stmNums3.SN_MinimumValueInfo, CustomsNumberViewStmNumsValidation.ThisRangeOverlapAnotherRange(stmNums3.SN_OwnerForDisplay, 1, 1200, stmNums1.SN_OwnerForDisplay, 100, 900));

			stmNums3.SN_MaximumValue = 99L;
			AssertNoErrors(stmNums3.SN_MaximumValueInfo);
			AssertNoErrors(stmNums3.SN_MinimumValueInfo);
		}

		public void TestCheckSN_MinimumValue()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNums1 = provider.NewCustomsNumber();
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums1.Wrapper).IsBranchLevel = false;
			stmNums1.SN_Owner = provider.Parent.PK;
			stmNums1.SN_MinimumValue = 200L;
			stmNums1.SN_MaximumValue = 320L;
			var stmNums2 = provider.NewCustomsNumber();
			stmNums2.SN_MinimumValue = 321L;
			stmNums2.SN_MaximumValue = 420L;
			stmNums2.SN_Owner = provider.Parent.PK;
			AssertNoErrors(stmNums2.SN_MinimumValueInfo);
			stmNums2.SN_MinimumValue = 290L;
			stmNums2.SN_MaximumValue = 420L;
			AssertHasError(stmNums2.SN_MinimumValueInfo, CustomsNumberViewStmNumsValidation.ThisRangeOverlapAnotherRange(stmNums2.SN_OwnerForDisplay, 290, 420, stmNums1.SN_OwnerForDisplay, 200, 320));
			stmNums2.SN_MinimumValue = 190L;
			stmNums2.SN_MaximumValue = 420L;
			AssertHasError(stmNums2.SN_MinimumValueInfo, CustomsNumberViewStmNumsValidation.ThisRangeOverlapAnotherRange(stmNums2.SN_OwnerForDisplay, 190, 420, stmNums1.SN_OwnerForDisplay, 200, 320));
			stmNums2.SN_MinimumValue = 400L;
			stmNums2.SN_MaximumValue = 420L;
			AssertNoErrors(stmNums2.SN_MinimumValueInfo);

			var stmNums3 = provider.NewCustomsNumber();
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums3.Wrapper).IsBranchLevel = true;
			stmNums3.SN_Owner = GlbBranch.CurrentBranch.PK;
			stmNums3.SN_MinimumValue = 200L;
			stmNums3.SN_MaximumValue = 320L;
			AssertHasError(stmNums3.SN_MinimumValueInfo, CustomsNumberViewStmNumsValidation.ThisRangeOverlapAnotherRange(stmNums3.SN_OwnerForDisplay, 200, 320, stmNums1.SN_OwnerForDisplay, 200, 320));
			stmNums3.SN_MinimumValue = 421L;
			stmNums3.SN_MaximumValue = 520L;
			AssertNoErrors(stmNums3.SN_MinimumValueInfo);
		}

		[UseSnapshotProtection]
		public void TestSN_Type()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNums1 = Factory.New<CustomsNumberViewStmNums>();
			stmNums1.Provider = provider;
			stmNums1.SN_Type = "CEN";
			stmNums1.SN_FountainName = "BOB NUMBER";
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums1.Wrapper).IsBranchLevel = false;
			stmNums1.SN_MinimumValue = 10L;
			stmNums1.SN_MaximumValue = 1200L;
			stmNums1.SN_Owner = provider.Parent.PK;

			var stmNums2 = Factory.New<CustomsNumberViewStmNums>();
			stmNums2.Provider = provider;
			stmNums2.SN_Type = "DEF";
			stmNums2.SN_FountainName = "BOB NUMBER";
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums2.Wrapper).IsBranchLevel = false;
			stmNums2.SN_MinimumValue = 2210L;
			stmNums2.SN_MaximumValue = 3200L;
			stmNums2.SN_Owner = provider.Parent.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(newFactory);
			provider.AllowDuplicatedTypeForTesting = false;
			var query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNums2.SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNums2.SN_Name);
			stmNums2 = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums2.Provider = provider;
			stmNums2.SN_Type = "CEN";
			stmNums2.Validation.ValidateSN_Type();
			var error = "This range (Owner: 'EDI - Eagle Datamation International', Type: CEN, Name: BOB NUMBER) already exists.";
			AssertHasErrorContaining(stmNums2.SN_TypeInfo, error);

			stmNums2.SN_Type = "DEF";
			stmNums2.Validation.ValidateSN_Type();
			AssertNoErrorContaining(stmNums2.SN_TypeInfo, error);

			var stmNums3 = newFactory.New<CustomsNumberViewStmNums>();
			stmNums3.Provider = provider;
			stmNums3.SN_Owner = GlbBranch.CurrentBranch.PK;
			stmNums3.SN_Type = "CEN";
			stmNums3.SN_FountainName = "BOB NUMBER";
			stmNums3.SN_MinimumValue = 10L;
			stmNums3.SN_MaximumValue = 1200L;
			stmNums3.Validation.ValidateSN_MaximumValue();
			AssertNoErrors(stmNums3.SN_MaximumValueInfo);
			var stmNums4 = newFactory.New<CustomsNumberViewStmNums>();
			stmNums4.Provider = provider;
			stmNums4.SN_Owner = GlbBranch.CurrentBranch.PK;
			stmNums4.SN_Type = "DEF";
			stmNums4.SN_FountainName = "BOB NUMBER";
			stmNums4.SN_MinimumValue = 2210L;
			stmNums4.SN_MaximumValue = 3200L;
			stmNums4.Validation.ValidateSN_MaximumValue();
			AssertNoErrors(stmNums4.SN_MaximumValueInfo);

			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(newFactory);
			provider.AllowDuplicatedTypeForTesting = false;
			query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNums4.SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNums4.SN_Name);
			stmNums4 = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
			stmNums4.Provider = provider;
			stmNums4.SN_Type = "CEN";
			((CustomsNumberViewStmNumsCompanyWrapper)stmNums4.Wrapper).IsBranchLevel = true;
			stmNums4.SN_Owner = GlbBranch.CurrentBranch.PK;
			stmNums4.Validation.ValidateSN_Type();
			error = $"This range (Owner: '{stmNums4.SN_OwnerForDisplay}', Type: CEN, Name: BOB NUMBER) already exists.";
			AssertHasErrorContaining(stmNums4.SN_TypeInfo, error);

			stmNums4.SN_Type = "DEF";
			stmNums4.Validation.ValidateSN_Type();
			AssertNoErrorContaining(stmNums4.SN_TypeInfo, error);
		}

		public void TestCheckSN_Owner()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "US$";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "US%";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNum = provider.NewCustomsNumber();
			var wrapper = stmNum.Wrapper;
			wrapper.SN_Owner = ZGuid.Empty;
			AssertHasErrorContaining(wrapper.SN_OwnerInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(wrapper.SN_OwnerInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(stmNum.SN_OwnerInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(stmNum.SN_OwnerInfo, ListValidation.InvalidCodeError);
			wrapper.SN_Owner = usCompany.PK;
			stmNum.Validation.ValidateSN_Owner();
			AssertNoErrorContaining("If the owner is company, you can select any company.", wrapper.SN_OwnerInfo, ListValidation.InvalidCodeError);
			AssertNoErrorContaining(stmNum.SN_OwnerInfo, ListValidation.InvalidCodeError);
			wrapper.SN_Owner = GlbCompany.CurrentCompany.PK;
			AssertNoErrors(wrapper.SN_OwnerInfo);
			AssertNoErrors(stmNum.SN_OwnerInfo);
			wrapper.SN_Owner = usBranch.PK;
			stmNum.Validation.ValidateSN_Owner();
			AssertHasErrorContaining("If the owner is branch, you must select the branch belongs to the provider's company.", wrapper.SN_OwnerInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(stmNum.SN_OwnerInfo, ListValidation.InvalidCodeError);
			wrapper.SN_Owner = GlbBranch.CurrentBranch.PK;
			AssertNoErrors(wrapper.SN_OwnerInfo);
			AssertNoErrors(stmNum.SN_OwnerInfo);
		}

		public void TestCheckSN_Name()
		{
			var provider = CustomsNumberViewStmNumsCompanyProviderForTest.New(Factory);
			var stmNum = provider.NewCustomsNumber();
			stmNum.SN_Name = "TestWithNon-EnglishCharacter⑧";
			stmNum.Validation.ValidateSN_Name();
			AssertNoErrors(stmNum.SN_NameInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea);
			providerSetup = new CustomsNumberViewStmNumsCompanyProviderForTestSetUp();
		}
		IDisposable providerSetup;
		IDisposable countrySetter;

		protected override void TearDown()
		{
			if (providerSetup != null)
			{
				providerSetup.Dispose();
				providerSetup = null;
			}
			if (countrySetter != null)
			{
				countrySetter.Dispose();
				countrySetter = null;
			}
			base.TearDown();
		}
	}
}
