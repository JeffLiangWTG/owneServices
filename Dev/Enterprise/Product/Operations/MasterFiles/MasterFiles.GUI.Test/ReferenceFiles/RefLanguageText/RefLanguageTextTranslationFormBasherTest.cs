using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefLanguageTextTranslationForm))]
	sealed class RefLanguageTextTranslationFormBasherTest : ZFormBasherTest
	{
		const string TranslationColumn = "Translation";

		RefLanguageTextPage GetPageInstance(ZString caption)
		{
			return new RefLanguageTextPage(testHelper.ParentTableCode, testHelper.ColumnToTest, testHelper.GetMultilingualLanguage(caption), testHelper.GetContextObject(Factory, caption), "Test", testHelper.GetFilterColumns());
		}

		protected override Form GetFormToBashCore()
		{
			return new RefLanguageTextTranslationForm(GetPageInstance("one"));
		}

		public void TestFormBehaviour()
		{
			using (var form = new RefLanguageTextTranslationForm(GetPageInstance("two")))
			{
				form.Show();

				var currentTopGrid = (RefLanguageTextPageEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
				AssertEquals("two", currentTopGrid.English);
				foreach (RefLanguageTextPageEntry entry in form.translationsOfCurrentValueGrid.ListManager.List)
				{
					AssertEquals("two", entry.English);
				}
				var currentBottomGrid = (RefLanguageTextPageEntry)form.allValuesGrid.ListManager.GetCurrent();
				AssertEquals("two", currentBottomGrid.English);
				AssertNotEquals("Precondition: first language should not be ZH-CN", Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
				AssertEquals(currentTopGrid.Language, currentBottomGrid.Language);
				AssertEquals(currentTopGrid.Language, new CodeDescriptionPairList(OLookUpEditType.Language).GetCodeFromDescription(form.allValuesGrid.Columns[TranslationColumn].ColumnStyle.HeaderText));

				for (int i = 0; i < form.translationsOfCurrentValueGrid.ListManager.List.Count; i++)
				{
					if (((RefLanguageTextPageEntry)form.translationsOfCurrentValueGrid.ListManager.List[i]).Language == Core.SharedConstants.Languages.ChineseSimplified)
					{
						form.translationsOfCurrentValueGrid.ListManager.Position = i;
						break;
					}
				}

				currentTopGrid = (RefLanguageTextPageEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
				AssertEquals("two", currentTopGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentTopGrid.Language);
				AssertEquals(testHelper.ChineseNumbers.ElementAt(2), currentTopGrid.Translation);
				currentBottomGrid = (RefLanguageTextPageEntry)form.allValuesGrid.ListManager.GetCurrent();
				AssertEquals("two", currentBottomGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
				AssertEquals(testHelper.ChineseNumbers.ElementAt(2), currentBottomGrid.Translation);
				AssertEquals("Chinese - Simplified", form.allValuesGrid.Columns[TranslationColumn].ColumnStyle.HeaderText);
				foreach (RefLanguageTextPageEntry entry in form.allValuesGrid.ListManager.List)
				{
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, entry.Language);
				}

				for (int i = 0; i < form.allValuesGrid.ListManager.List.Count; i++)
				{
					if (((RefLanguageTextPageEntry)form.allValuesGrid.ListManager.List[i]).English == "seven")
					{
						form.allValuesGrid.ListManager.Position = i;
						break;
					}
				}

				currentTopGrid = (RefLanguageTextPageEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
				AssertEquals("seven", currentTopGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentTopGrid.Language);
				AssertEquals(testHelper.ChineseNumbers.ElementAt(7), currentTopGrid.Translation);
				currentBottomGrid = (RefLanguageTextPageEntry)form.allValuesGrid.ListManager.GetCurrent();
				AssertEquals("seven", currentBottomGrid.English);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
				AssertEquals(testHelper.ChineseNumbers.ElementAt(7), currentBottomGrid.Translation);
			}
		}

		public void TestDefaultLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				using (var form = new RefLanguageTextTranslationForm(GetPageInstance("two")))
				{
					form.Show();

					var currentTopGrid = (RefLanguageTextPageEntry)form.translationsOfCurrentValueGrid.ListManager.GetCurrent();
					AssertEquals("two", currentTopGrid.English);
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentTopGrid.Language);
					AssertEquals(testHelper.ChineseNumbers.ElementAt(2), currentTopGrid.Translation);
					var currentBottomGrid = (RefLanguageTextPageEntry)form.allValuesGrid.ListManager.GetCurrent();
					AssertEquals("two", currentBottomGrid.English);
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, currentBottomGrid.Language);
					AssertEquals(testHelper.ChineseNumbers.ElementAt(2), currentBottomGrid.Translation);
				}
			}
		}

		protected override void SetUp()
		{
			testHelper = new RefLanguageTextTranslationTestHelper(Factory);
			base.SetUp();
		}

		RefLanguageTextTranslationTestHelper testHelper;
	}
}
