using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(SGCustomsNumberViewStmNumsCompanyProvider))]
	sealed class SGCustomsNumberViewStmNumsCompanyProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetProvider()
		{
			var provider = (CustomsNumberViewStmNumsCompanyProvider)CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, Company.GC_RN_NKCountryCode, Company.PK);
			AssertEquals(typeof(SGCustomsNumberViewStmNumsCompanyProvider), provider.GetType());
			AssertEquals(true, object.ReferenceEquals(provider, Company.CustomsNumberProvider));
			AssertEquals(Core.Constants.CountryCodes.Singapore, provider.CountryCode);
		}

		public void TestGetSetting()
		{
			var provider = Company.CustomsNumberProvider;
			var setting = provider.GetSetting(NumberRangeTypeList.Codes.SingaporeMessageNumber);
			AssertEquals(true, object.ReferenceEquals(setting, provider.GetSetting(NumberRangeTypeList.Codes.SingaporeMessageNumber)));
			AssertEquals(typeof(SGCustomsNumberViewStmNumsSetting), setting.GetType());
			AssertEquals(NumberRangeTypeList.Codes.SingaporeMessageNumber, setting.RangeType);
		}

		public void TestGetEditorForm()
		{
			var provider = Company.CustomsNumberProvider;
			provider.CustomsNumbers.AddNew();
			var wrapper = provider.CustomsNumberWrappers[0];
			using (var form = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetEditorForm(wrapper))
			{
				AssertEquals(typeof(CustomsNumberViewStmNumsEditorForm), form.GetType());
			}
		}

		public void TestCustomsNumberWrappers()
		{
			var provider = Company.CustomsNumberProvider;
			AssertEquals(typeof(SGCustomsNumberViewStmNumsWrapperCollection), provider.CustomsNumberWrappers.GetType());
			AssertEquals(0, provider.CustomsNumberWrappers.Count);
			provider.CustomsNumbers.AddNew();
			AssertEquals(1, provider.CustomsNumberWrappers.Count);
			var wrapper = provider.CustomsNumberWrappers[0];
			AssertEquals(typeof(SGCustomsNumberViewStmNumsWrapper), wrapper.GetType());
		}

		public void TestGetUserControl()
		{
			var provider = Company.CustomsNumberProvider;
			using (var userControl = ((ICustomsNumberViewStmNumsGuiProvider)provider).GetUserControl())
			{
				AssertEquals(typeof(CustomsNumberViewStmNumsUserControl), userControl.GetType());
			}
		}

		public void TestGetOrCreateWrapper()
		{
			var provider = Company.CustomsNumberProvider;
			AssertNull(provider.GetOrCreateWrapper(null));
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var wrapper = provider.GetOrCreateWrapper(stmNum);
			AssertEquals(typeof(SGCustomsNumberViewStmNumsWrapper), wrapper.GetType());
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
			AssertEquals(typeof(SGCustomsNumberViewStmNumsLookups), lookups.GetType());
		}

		public void TestAdditionalValidationForGC_CustomsRegistrationNo()
		{
			Company.GC_CustomsRegistrationNo = "SA0001";
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "AAA";
			glbCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			glbCompany.GC_CustomsRegistrationNo = "SA0001";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "CCC";
			branch.GB_GC = glbCompany.PK;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var staff = GlbStaff.New(Factory);
			staff.GS_Code = "EEE";
			staff.GS_LoginName = "Squanchy";
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertNotNull(glbCompany.CustomsNumberProvider);
				glbCompany.CustomsNumberProvider.SetupRelatedDataAndNotification();
				glbCompany.Validation.ValidateGC_CustomsRegistrationNo();
				AssertHasWarningContaining(glbCompany.GC_CustomsRegistrationNoInfo, "An SMN Number Range must be setup for this Company as its Customs Registration Number is shared with other Companies.");
				var customsNumber1 = glbCompany.CustomsNumberProvider.CustomsNumbers.AddNew();
				customsNumber1.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
				customsNumber1.SN_MinimumValue = 1111;
				customsNumber1.SN_MaximumValue = 2222;
				customsNumber1.SN_Value = 1111;
				customsNumber1.SN_FountainName = "SA0001";
				Factory.Save();
				glbCompany.Validation.ValidateGC_CustomsRegistrationNo();
				AssertNoWarningContaining(glbCompany.GC_CustomsRegistrationNoInfo, "An SMN Number Range must be setup for this Company as its Customs Registration Number is shared with other Companies.");
				AssertHasWarningContaining(glbCompany.GC_CustomsRegistrationNoInfo, $"The following Companies are sharing this Customs Registration Number, but does not have an SMN Number Range setup: ({Company.GC_Code} - {Company.GC_Name}). Please record an SMN Number Range for all other Companies which share this Customs Registration Number.");
				var customsNumber2 = Company.CustomsNumberProvider.CustomsNumbers.AddNew();
				customsNumber2.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
				customsNumber2.SN_MinimumValue = 3000;
				customsNumber2.SN_MaximumValue = 4000;
				customsNumber2.SN_Value = 3001;
				customsNumber2.SN_FountainName = "SA0001";
				customsNumber2.Factory.Save();
				glbCompany.Validation.ValidateGC_CustomsRegistrationNo();
				AssertNoWarningContaining(glbCompany.GC_CustomsRegistrationNoInfo, $"The following Companies are sharing this Customs Registration Number, but does not have an SMN Number Range setup: ({Company.GC_Code} - {Company.GC_Name}). Please record an SMN Number Range for all other Companies which share this Customs Registration Number.");
				glbCompany.GC_CustomsRegistrationNo = "XCV00669";
				glbCompany.Validation.ValidateGC_CustomsRegistrationNo();
				AssertHasWarningContaining(glbCompany.GC_CustomsRegistrationNoInfo, "There is an SMN Number Range setup that doesn't match this Customs Registration Number");
				glbCompany.GC_CustomsRegistrationNo = "SA0001";
				glbCompany.Validation.ValidateGC_CustomsRegistrationNo();
				AssertNoWarningContaining(glbCompany.GC_CustomsRegistrationNoInfo, "There is an SMN Number Range setup that doesn't match this Customs Registration Number");
			}
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new SGCustomsNumberViewStmNumsCompanyProvider(Factory, Company.PK);
		}
	}
}
