using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.TextStandardizer.JobCategorizer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class JobCategoryHelperTest : TestCaseWithFactory
	{
		public void TestGetJobCategories()
		{
			AssertJobCategory((Level.Leadership, Area.Undefined), (OrgContactJobCategories.Codes.LEA, OrgContactJobCategories.Descriptions.LEA));

			AssertJobCategory((Level.SeniorManagement, Area.Administration), (OrgContactJobCategories.Codes.SMA, OrgContactJobCategories.Descriptions.SMA));
			AssertJobCategory((Level.SeniorManagement, Area.Finance), (OrgContactJobCategories.Codes.SMF, OrgContactJobCategories.Descriptions.SMF));
			AssertJobCategory((Level.SeniorManagement, Area.Operations), (OrgContactJobCategories.Codes.SMO, OrgContactJobCategories.Descriptions.SMO));
			AssertJobCategory((Level.SeniorManagement, Area.SalesAndMarketing), (OrgContactJobCategories.Codes.SMS, OrgContactJobCategories.Descriptions.SMS));
			AssertJobCategory((Level.SeniorManagement, Area.Undefined), (OrgContactJobCategories.Codes.SMU, OrgContactJobCategories.Descriptions.SMU));

			AssertJobCategory((Level.Management, Area.Administration), (OrgContactJobCategories.Codes.MAA, OrgContactJobCategories.Descriptions.MAA));
			AssertJobCategory((Level.Management, Area.Finance), (OrgContactJobCategories.Codes.MAF, OrgContactJobCategories.Descriptions.MAF));
			AssertJobCategory((Level.Management, Area.Operations), (OrgContactJobCategories.Codes.MAO, OrgContactJobCategories.Descriptions.MAO));
			AssertJobCategory((Level.Management, Area.SalesAndMarketing), (OrgContactJobCategories.Codes.MAS, OrgContactJobCategories.Descriptions.MAS));
			AssertJobCategory((Level.Management, Area.Undefined), (OrgContactJobCategories.Codes.MAU, OrgContactJobCategories.Descriptions.MAU));

			AssertJobCategory((Level.Employee, Area.Administration), (OrgContactJobCategories.Codes.EMA, OrgContactJobCategories.Descriptions.EMA));
			AssertJobCategory((Level.Employee, Area.Finance), (OrgContactJobCategories.Codes.EMF, OrgContactJobCategories.Descriptions.EMF));
			AssertJobCategory((Level.Employee, Area.Operations), (OrgContactJobCategories.Codes.EMO, OrgContactJobCategories.Descriptions.EMO));
			AssertJobCategory((Level.Employee, Area.SalesAndMarketing), (OrgContactJobCategories.Codes.EMS, OrgContactJobCategories.Descriptions.EMS));
			AssertJobCategory((Level.Employee, Area.Undefined), (OrgContactJobCategories.Codes.EMU, OrgContactJobCategories.Descriptions.EMU));
		}

		void AssertJobCategory((Level level, Area area) key, (string code, string decription) value)
		{
			JobCategoryHelper.JobCategoriesMap.TryGetValue((key.level, key.area), out var category);
			AssertNotNull(category);
			AssertEquals(value.code, category.Item1);
			AssertEquals(value.decription, category.Item2);
		}

		public void TestIsUserDefinedCategoryOrEmpty()
		{
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.LEA));

			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.SMA));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.SMF));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.SMO));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.SMS));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.SMU));

			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.MAA));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.MAF));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.MAO));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.MAS));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.MAU));

			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.EMA));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.EMF));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.EMO));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.EMS));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.EMU));
			AssertEquals(true, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(string.Empty));

			AssertEquals(false, JobCategoryHelper.IsUserDefinedCategoryOrEmpty(OrgContactJobCategories.Codes.SEM));
		}

		public void TestFindSuggestedJobCategories_NoSuggestions_WhenTitleIsEmpty()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			contact.OC_Title = "MANAGER";
			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(1, contact.SuggestedJobCategories.Count);

			contact.OC_Title = string.Empty;
			contact.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(0, contact.SuggestedJobCategories.Count);
		}

		public void TestFindSuggestedJobCategories_ShouldOverrideJobCategoryWhenJobCategoryIsEMUAndThereIsOnlyOneSuggestion()
		{
			// Preconditon
			var title = "MANAGER";
			var suggestions = JobCategoryHelper.GetJobCategories(title);
			AssertEquals(1, suggestions.Count);
			AssertNotEquals(OrgContactJobCategories.Codes.EMU, suggestions[0].Item1);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Title = title;
			contact.OC_JobCategory = OrgContactJobCategories.Codes.EMU;

			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(OrgContactJobCategories.Codes.MAU, contact.OC_JobCategory);
		}

		public void TestFindSuggestedJobCategories_SuggestionsFound_WhenJobCategoryIsNotEMUAndThereIsOnlyOneSuggestion()
		{
			// Preconditon
			var title = "MANAGER";
			var suggestions = JobCategoryHelper.GetJobCategories(title);
			AssertEquals(1, suggestions.Count);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Title = title;
			contact.OC_JobCategory = OrgContactJobCategories.Codes.LEA;

			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(1, contact.SuggestedJobCategories.Count);
			AssertEquals(OrgContactJobCategories.Codes.MAU, contact.SuggestedJobCategories[0].Item1);
		}

		public void TestFindSuggestedJobCategories_SuggestionsFound_AfterSelectSuggestions()
		{
			// Preconditon
			var title = "OFFICER";
			var suggestions = JobCategoryHelper.GetJobCategories(title);
			AssertEquals(OrgContactJobCategories.Codes.SMA, suggestions[0].Item1);
			AssertEquals(OrgContactJobCategories.Codes.EMA, suggestions[1].Item1);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Title = title;
			contact.OC_JobCategory = OrgContactJobCategories.Codes.LEA;

			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(2, contact.SuggestedJobCategories.Count);
			AssertEquals(OrgContactJobCategories.Codes.SMA, suggestions[0].Item1);
			AssertEquals(OrgContactJobCategories.Codes.EMA, suggestions[1].Item1);
		}

		public void TestFindSuggestedJobCategories_WhenJobCategoryIsEmpty()
		{
			// Preconditon
			var title = "OFFICER";
			var suggestions = JobCategoryHelper.GetJobCategories(title);
			AssertEquals(2, suggestions.Count);
			AssertEquals(OrgContactJobCategories.Codes.SMA, suggestions[0].Item1);
			AssertEquals(OrgContactJobCategories.Codes.EMA, suggestions[1].Item1);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Title = title;
			contact.OC_JobCategory = string.Empty;

			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(2, contact.SuggestedJobCategories.Count);
		}

		public void TestFindSuggestedJobCategories_ShouldClearSuggestionsBeforeFind()
		{
			// Preconditon
			var title = "OFFICER";
			var suggestions = JobCategoryHelper.GetJobCategories(title);
			AssertEquals(2, suggestions.Count);
			AssertEquals(OrgContactJobCategories.Codes.SMA, suggestions[0].Item1);
			AssertEquals(OrgContactJobCategories.Codes.EMA, suggestions[1].Item1);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Title = title;
			contact.OC_JobCategory = string.Empty;

			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(2, contact.SuggestedJobCategories.Count);

			contact.OC_JobCategory = OrgContactJobCategories.Codes.SEM;
			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(0, contact.SuggestedJobCategories.Count);
		}

		public void TestShouldNotOverwriteJobCategory()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Title = "Manager";
			contact.OC_JobCategory = OrgContactJobCategories.Codes.LEA;

			JobCategoryHelper.FindSuggestedJobCategories(contact);
			AssertEquals(1, contact.SuggestedJobCategories.Count);
			AssertEquals(OrgContactJobCategories.Codes.MAU, contact.SuggestedJobCategories[0].Item1);

			JobCategoryHelper.OverwriteJobCategory(contact);
			AssertNotEquals("Should not overwrite.", OrgContactJobCategories.Codes.MAU, contact.OC_JobCategory);

			contact.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
			JobCategoryHelper.OverwriteJobCategory(contact);
			AssertEquals("Should overwrite.", OrgContactJobCategories.Codes.MAU, contact.OC_JobCategory);

			contact.OC_JobCategory = string.Empty;
			JobCategoryHelper.OverwriteJobCategory(contact);
			AssertEquals("Should overwrite.", OrgContactJobCategories.Codes.MAU, contact.OC_JobCategory);
		}
	}
}
