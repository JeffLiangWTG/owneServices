using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCustomsNumberViewStmNumsCompanyProvider))]
	sealed class USCustomsNumberViewStmNumsCompanyProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetProvider()
		{
			var provider = (CustomsNumberViewStmNumsCompanyProvider)CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Company.GC_RN_NKCountryCode, Company.PK);
			AssertEquals(typeof(USCustomsNumberViewStmNumsCompanyProvider), provider.GetType());
			AssertEquals(true, object.ReferenceEquals(provider, Company.CustomsNumberProvider));
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, provider.CountryCode);
		}

		public void TestEnableBranchLevel()
		{
			AssertEquals("EnableBranchLevel", true, ((CustomsNumberViewStmNumsCompanyProvider)Company.CustomsNumberProvider).EnableBranchLevel);
		}

		public void TestEnableCompanyLevel()
		{
			AssertEquals("EnableCompanyLevel", true, ((CustomsNumberViewStmNumsCompanyProvider)Company.CustomsNumberProvider).EnableCompanyLevel);
		}

		public void TestGetSetting()
		{
			var provider = Company.CustomsNumberProvider;
			var setting = provider.GetSetting(NumberRangeTypeList.Codes.CustomsEntry);
			AssertEquals(true, object.ReferenceEquals(setting, provider.GetSetting(NumberRangeTypeList.Codes.CustomsEntry)));
			AssertEquals(typeof(USCustomsNumberViewStmNumsSetting), setting.GetType());
			AssertEquals(NumberRangeTypeList.Codes.CustomsEntry, setting.RangeType);
		}

		public void TestGetEditorForm()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = provider.CustomsNumbers.AddNew();
			var wrapper = provider.CustomsNumberWrappers[0];
			using (var form = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetEditorForm(wrapper))
			{
				AssertEquals(typeof(USCustomsNumberViewStmNumsEditorForm), form.GetType());
			}
		}

		public void TestCustomsNumberWrappers()
		{
			var provider = Company.CustomsNumberProvider;
			AssertEquals(typeof(USCustomsNumberViewStmNumsWrapperCollection), provider.CustomsNumberWrappers.GetType());
			AssertEquals(0, provider.CustomsNumberWrappers.Count);
			var stmNum = provider.CustomsNumbers.AddNew();
			AssertEquals(1, provider.CustomsNumberWrappers.Count);
			var wrapper = provider.CustomsNumberWrappers[0];
			AssertEquals(typeof(USCustomsNumberViewStmNumsWrapper), wrapper.GetType());
		}

		public void TestGetUserControl()
		{
			var provider = Company.CustomsNumberProvider;
			using (var userControl = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetUserControl())
			{
				AssertEquals(typeof(USCustomsNumberViewStmNumsUserControl), userControl.GetType());
			}
		}

		public void TestGetOrCreateWrapper()
		{
			var provider = Company.CustomsNumberProvider;
			AssertNull(provider.GetOrCreateWrapper(null));
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var wrapper = provider.GetOrCreateWrapper(stmNum);
			AssertEquals(typeof(USCustomsNumberViewStmNumsWrapper), wrapper.GetType());
			AssertEquals(true, object.ReferenceEquals(wrapper, provider.GetOrCreateWrapper(stmNum)));
		}

		public void TestGetNewLookups()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var lookups = stmNum.Lookups;
			AssertEquals(typeof(CustomsNumberViewStmNumsLookups), lookups.GetType());
			stmNum.Provider = provider;
			lookups = stmNum.Lookups;
			AssertEquals(typeof(USCustomsNumberViewStmNumsLookups), lookups.GetType());
		}

		[UseSnapshotProtection]
		public void TestGetNumberRangeDetail()
		{
			var provider = Company.CustomsNumberProvider;
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.Provider = provider;
			stmNum.SN_Owner = Company.PK;
			stmNum.SN_Type = NumberRangeTypeList.Codes.CustomsEntry;
			stmNum.SN_FountainName = "XJ5:1";
			stmNum.SN_MinimumValue = 100000;
			stmNum.SN_Count = 1000;
			Factory.Save();
			var range = stmNum.GetNumberRanges()[0];
			AssertEquals("Owner: Company - EDI - Eagle Datamation International, Range Type: ENS, Check Digit Addition: 1, Applies To: XJ5", range.Detail);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject() => new USCustomsNumberViewStmNumsCompanyProvider(Factory, Company.PK);

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
	}
}
