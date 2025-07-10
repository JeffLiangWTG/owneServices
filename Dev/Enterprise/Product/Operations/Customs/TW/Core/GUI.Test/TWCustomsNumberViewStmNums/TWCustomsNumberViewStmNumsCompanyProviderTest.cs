using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(TWCustomsNumberViewStmNumsCompanyProvider))]
	sealed class TWCustomsNumberViewStmNumsCompanyProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetProvider()
		{
			var provider = (TWCustomsNumberViewStmNumsCompanyProvider)CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Company.GC_RN_NKCountryCode, Company.PK);
			AssertEquals(typeof(TWCustomsNumberViewStmNumsCompanyProvider), provider.GetType());
			AssertEquals(true, object.ReferenceEquals(provider, Company.CustomsNumberProvider));
			AssertEquals(Core.Constants.CountryCodes.Taiwan, provider.CountryCode);
		}

		public void TestEnableBranchLevel()
		{
			AssertEquals("EnableBranchLevel", false, ((CustomsNumberViewStmNumsCompanyProvider)Company.CustomsNumberProvider).EnableBranchLevel);
		}

		public void TestEnableCompanyLevel()
		{
			AssertEquals("EnableCompanyLevel", true, ((CustomsNumberViewStmNumsCompanyProvider)Company.CustomsNumberProvider).EnableCompanyLevel);
		}

		public void TestGetSetting()
		{
			var provider = Company.CustomsNumberProvider;
			var setting = provider.GetSetting(RangeTypeList.Codes.A);
			AssertEquals(true, object.ReferenceEquals(setting, provider.GetSetting(RangeTypeList.Codes.A)));
			AssertEquals(typeof(TWCustomsNumberViewStmNumsSetting), setting.GetType());
			AssertEquals(RangeTypeList.Codes.A, setting.RangeType);
		}

		public void TestGetEditorForm()
		{
			var provider = Company.CustomsNumberProvider;
			provider.CustomsNumbers.AddNew();
			var wrapper = provider.CustomsNumberWrappers[0];
			using (var form = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetEditorForm(wrapper))
			{
				AssertEquals(typeof(TWCustomsNumberViewStmNumsEditorForm), form.GetType());
			}
		}

		public void TestCustomsNumberWrappers()
		{
			var provider = Company.CustomsNumberProvider;
			AssertEquals(typeof(TWCustomsNumberViewStmNumsWrapperCollection), provider.CustomsNumberWrappers.GetType());
			AssertEquals(0, provider.CustomsNumberWrappers.Count);
			provider.CustomsNumbers.AddNew();
			AssertEquals(1, provider.CustomsNumberWrappers.Count);
			var wrapper = provider.CustomsNumberWrappers[0];
			AssertEquals(typeof(TWCustomsNumberViewStmNumsWrapper), wrapper.GetType());
		}

		public void TestGetUserControl()
		{
			var provider = Company.CustomsNumberProvider;
			using (var userControl = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetUserControl())
			{
				AssertEquals(typeof(TWCustomsNumberViewStmNumsUserControl), userControl.GetType());
			}
		}

		public void TestGetOrCreateWrapper()
		{
			var provider = Company.CustomsNumberProvider;
			AssertNull(provider.GetOrCreateWrapper(null));
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var wrapper = provider.GetOrCreateWrapper(stmNum);
			AssertEquals(typeof(TWCustomsNumberViewStmNumsWrapper), wrapper.GetType());
			AssertEquals(true, object.ReferenceEquals(wrapper, provider.GetOrCreateWrapper(stmNum)));
		}

		public void TestGetNewLookupsAndValidation()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var lookups = stmNum.Lookups;
			AssertEquals(typeof(CustomsNumberViewStmNumsLookups), lookups.GetType());
			stmNum.Provider = provider;
			lookups = stmNum.Lookups;
			AssertEquals(typeof(TWCustomsNumberViewStmNumsLookups), lookups.GetType());
			AssertEquals(typeof(TWCustomsNumberViewStmNumsValidation), stmNum.Validation.GetType());
		}

		[UseSnapshotProtection]
		public void TestGetNumberRangeDetail()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = provider;
			stmNum.SN_Owner = Company.PK;
			stmNum.SN_Type = "A";
			stmNum.SN_FountainName = "TWEntryNum_IMP_A";
			stmNum.SN_MinimumValue = 100000;
			stmNum.SN_Count = 1000;
			Factory.Save();
			var range = stmNum.GetNumberRanges().First();
			AssertEquals("Owner: Company - EDI - Eagle Datamation International, Range Type: A, MessageType: IMP", range.Detail);
		}

		public void TestCheckRangeType()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = provider;
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			wrapper.MessageType = "IMP";
			wrapper.RangeType = ZString.Empty;
			AssertHasErrorContaining(wrapper.RangeTypeInfo, MandatoryValidation.MustBeEntered);
			wrapper.RangeType = "8";
			CombineAssertions(() =>
			{
				AssertNoErrorContaining(wrapper.RangeTypeInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining(wrapper.RangeTypeInfo, ListValidation.InvalidCodeError);
			});
			wrapper.RangeType = "A";
			AssertNoErrorContaining(wrapper.RangeTypeInfo, ListValidation.InvalidCodeError);
			stmNum.SN_Owner = Company.PK;
			Factory.Save();
			stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = provider;
			wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			wrapper.MessageType = "IMP";
			wrapper.RangeType = "A";
			CombineAssertions(() =>
			{
				AssertHasErrorContaining(wrapper.RangeTypeInfo, "This range (Owner: '', Start: 1, End: 6888105) overlaps range (Owner: 'EDI - Eagle Datamation International', Start: 1, End: 6888105).");
				AssertHasErrorContaining(wrapper.RangeTypeInfo, "This range (Owner: '', Type: CUS, Name: TWEntryNum_IMP_A) already exists.");
			});
			wrapper.RangeType = "B";
			CombineAssertions(() =>
			{
				AssertNoErrorContaining(wrapper.RangeTypeInfo, "This range (Owner: '', Start: 1, End: 6888105) overlaps range (Owner: 'EDI - Eagle Datamation International', Start: 1, End: 6888105).");
				AssertNoErrorContaining(wrapper.RangeTypeInfo, "This range (Owner: '', Type: CUS, Name: TWEntryNum_IMP_A) already exists.");
			});
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TWCustomsNumberViewStmNumsCompanyProvider(Factory, Company.PK);
		}
	}
}
