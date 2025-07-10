using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class GlbReleaseNoteLookupsTest : BusinessObjectLookupsTestCase
	{
		public virtual void TestSectionList()
		{
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ProductUpdates));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.WiseLearningUpdates));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.WiseNews));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ClientStaffNews));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ClientAnnouncements));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ClientNews));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.BorderWise));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.WiseTechAcademy));
		}

		public void TestCategories()
		{
			AssertEquals("Count = length of licences + blank item", Array.FindAll(Env.Licence.GetAllCheckpoints(), checkpoint => !(checkpoint is LanguageLicenceChildCheckpoint)).Length + 1, Lookups.Categories.Count);
			AssertEquals("GetDescriptionFromCode(Env.Licence.Accountant.Name)", Env.Licence.Accountant.DisplayName, Lookups.Categories.GetDescriptionFromCode(Env.Licence.Accountant.Name));

			Assert(Lookups.Categories.ContainsCode(Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.French].Name));
			Assert(!Lookups.Categories.ContainsCode(Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.French].DocBuilderLanguageCheckpoint.Name));
			Assert(!Lookups.Categories.ContainsCode(Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.French].GUILanguageCheckpoint.Name));
			Assert(!Lookups.Categories.ContainsCode(Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.French].WebTrackerLanguageCheckpoint.Name));

			parent.GF_Section = NewsSectionTypeList.Codes.BorderWise;
			AssertEquals("BorderWise: Count = 1, blank item", 1, Lookups.Categories.Count);
			Assert("BorderWise: blank item", Lookups.Categories.ContainsCode(""));

			parent.GF_Section = NewsSectionTypeList.Codes.WiseTechAcademy;
			AssertEquals("WiseTechAcademy: Count = 1, blank item", 1, Lookups.Categories.Count);
			Assert("WiseTechAcademy: blank item", Lookups.Categories.ContainsCode(""));
		}

		public void TestCountries()
		{
			AssertEquals("ContainsCode(CountryCodes.Australia)", true, Lookups.Countries.ContainsCode(Core.Constants.CountryCodes.Australia));
			AssertEquals("ContainsCode(CountryCodes.NewZealand)", true, Lookups.Countries.ContainsCode(Core.Constants.CountryCodes.NewZealand));
			AssertEquals("GetDescriptionFromCode(\"\")", "All Countries/Regions", Lookups.Countries.GetDescriptionFromCode(""));
			AssertEquals("All Countries is first item in list", "", Lookups.Countries[0].Code);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			parent = Factory.New<GlbReleaseNote>();
		}

		internal GlbReleaseNoteLookups Lookups
		{
			get { return parent.Lookups; }
		}

		internal GlbReleaseNote parent;

		#endregion
	}
}
