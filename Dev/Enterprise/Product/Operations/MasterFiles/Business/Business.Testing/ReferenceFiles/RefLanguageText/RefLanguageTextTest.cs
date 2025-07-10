using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefLanguageText))]
	sealed class RefLanguageTextTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetCustomLanguage()
		{
			var parentPkToSearch = Guid.NewGuid();
			var lang1 = Factory.New<RefLanguageText>();
			lang1.RLT_ColumnName = "ANY";
			lang1.RLT_ParentTableCode = "RN";
			lang1.RLT_Text = "ANY";
			lang1.RLT_IsSystem = true;
			lang1.RLT_IsClientOverridden = true;
			lang1.RLT_Language = "EN";
			lang1.RLT_ParentId = Guid.NewGuid();
			Factory.Save();

			var lang2 = Factory.New<RefLanguageText>();
			lang2.RLT_ColumnName = "ANY2";
			lang2.RLT_ParentTableCode = "RN";
			lang2.RLT_Text = "ANY2";
			lang2.RLT_IsSystem = true;
			lang2.RLT_IsClientOverridden = false;
			lang2.RLT_Language = "EN";
			lang2.RLT_ParentId = Guid.NewGuid();
			Factory.Save();

			var lang3 = Factory.New<RefLanguageText>();
			lang3.RLT_ColumnName = "ANY3";
			lang3.RLT_ParentTableCode = "RN";
			lang3.RLT_ParentId = parentPkToSearch;
			lang3.RLT_Text = "ANY3";
			lang3.RLT_IsSystem = false;
			lang3.RLT_Language = "EN";
			lang3.RLT_IsClientOverridden = false;
			Factory.Save();

			var languageText = RefLanguageText.GetByParentPKAndLanguage(Factory, lang1.RLT_ParentId.ToString(), "EN", "RN", "ANY");
			AssertEquals(languageText.RLT_ColumnName, lang1.RLT_ColumnName);
			AssertEquals(languageText.RLT_Text, lang1.RLT_Text);

			var translation = RefLanguageText.GetTranslation(Factory, "EN", lang1.RLT_ColumnName, lang1.RLT_ParentTableCode, lang1.RLT_ParentId.ToString());
			AssertEquals(translation, lang1.RLT_Text);

			var translation2 = RefLanguageText.GetTranslation(Factory, "EN", lang1.RLT_ColumnName, lang1.RLT_ParentTableCode, lang1.RLT_ParentId);
			AssertEquals(translation, translation2);
		}
		public void TestGetCustomLanguageForMultipleFields()
		{
			var parentPkToSearch = Guid.NewGuid();
			var lang1 = Factory.New<RefLanguageText>();
			lang1.RLT_ColumnName = "ANY";
			lang1.RLT_ParentTableCode = "RN";
			lang1.RLT_Text = "ANY";
			lang1.RLT_IsSystem = true;
			lang1.RLT_IsClientOverridden = true;
			lang1.RLT_Language = "EN";
			lang1.RLT_ParentId = parentPkToSearch;
			Factory.Save();

			var lang2 = Factory.New<RefLanguageText>();
			lang2.RLT_ColumnName = "ANY2";
			lang2.RLT_ParentTableCode = "RN";
			lang2.RLT_Text = "ANY2";
			lang2.RLT_IsSystem = true;
			lang2.RLT_IsClientOverridden = false;
			lang2.RLT_Language = "EN";
			lang2.RLT_ParentId = parentPkToSearch;
			Factory.Save();

			var lang3 = Factory.New<RefLanguageText>();
			lang3.RLT_ColumnName = "ANY3";
			lang3.RLT_ParentTableCode = "RN";
			lang3.RLT_ParentId = parentPkToSearch;
			lang3.RLT_Text = "ANY3";
			lang3.RLT_IsSystem = false;
			lang3.RLT_Language = "EN";
			lang3.RLT_IsClientOverridden = false;
			Factory.Save();

			var languageText = RefLanguageText.GetByParentPKAndLanguage(Factory, lang1.RLT_ParentId.ToString(), "EN", "RN", "ANY");
			AssertEquals(languageText.RLT_ColumnName, lang1.RLT_ColumnName);
			AssertEquals(languageText.RLT_Text, lang1.RLT_Text);

			var translation = RefLanguageText.GetTranslation(Factory, "EN", lang1.RLT_ColumnName, lang1.RLT_ParentTableCode, lang1.RLT_ParentId.ToString());
			AssertEquals(translation, lang1.RLT_Text);
			translation = RefLanguageText.GetTranslation(Factory, "EN", lang2.RLT_ColumnName, lang1.RLT_ParentTableCode, lang1.RLT_ParentId.ToString());
			AssertEquals(translation, lang2.RLT_Text);
			translation = RefLanguageText.GetTranslation(Factory, "EN", lang3.RLT_ColumnName, lang1.RLT_ParentTableCode, lang1.RLT_ParentId.ToString());
			AssertEquals(translation, lang3.RLT_Text);
		}

		public void TestGetRuntimeCaptionLanguage()
		{
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_RN_NKCountryCode = "AU";
			state.RW_Description = "State 1";

			var state2 = Factory.NewWithValidTestData<RefCountryStates>();
			state2.RW_RN_NKCountryCode = "AU";
			state2.RW_Description = "State 2";

			var state3 = Factory.NewWithValidTestData<RefCountryStates>();
			state3.RW_RN_NKCountryCode = "BR";
			state3.RW_Description = "State 3";

			var languageTextToState1 = Factory.New<RefLanguageText>();
			languageTextToState1.RLT_ColumnName = "RW_Description";
			languageTextToState1.RLT_ParentTableCode = "RW";
			languageTextToState1.RLT_ParentId = state.PK;
			languageTextToState1.RLT_Text = "Translation of state 1";
			languageTextToState1.RLT_IsSystem = false;
			languageTextToState1.RLT_Language = "EN";
			languageTextToState1.RLT_IsClientOverridden = false;

			var languageTextToState3 = Factory.New<RefLanguageText>();
			languageTextToState3.RLT_ColumnName = "RW_Description";
			languageTextToState3.RLT_ParentTableCode = "RW";
			languageTextToState3.RLT_ParentId = state3.PK;
			languageTextToState3.RLT_Text = "Translation of state 3";
			languageTextToState3.RLT_IsSystem = false;
			languageTextToState3.RLT_Language = "EN";
			languageTextToState3.RLT_IsClientOverridden = false;
			Factory.Save();

			var result = RefLanguageText.GetRuntimeLanguageCaptions(Factory, "RW_Description", "RW", state, new string[] { "RW_RN_NKCountryCode" });
			Assert(result.Any(x => x.EnglishText == languageTextToState1.RLT_Text));
			Assert(result.Any(x => x.EnglishText == state2.RW_Description));
			Assert(!result.Any(x => x.EnglishText == languageTextToState3.RLT_Text));

			result = RefLanguageText.GetRuntimeLanguageCaptions(Factory, "RW_Description", "RW", state3, new string[] { "RW_RN_NKCountryCode" });
			Assert(!result.Any(x => x.EnglishText == languageTextToState1.RLT_Text));
			Assert(!result.Any(x => x.EnglishText == state2.RW_Description));
			Assert(result.Any(x => x.EnglishText == languageTextToState3.RLT_Text));
		}

		RefLanguageText GetValidRefLanguageTextToTest()
		{
			var languageText = Factory.NewWithValidTestData<RefLanguageText>();
			languageText.RLT_Text = "AA";
			languageText.RLT_ParentTableCode = "RN";
			return languageText;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetValidRefLanguageTextToTest();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetValidRefLanguageTextToTest();
		}
	}
}
