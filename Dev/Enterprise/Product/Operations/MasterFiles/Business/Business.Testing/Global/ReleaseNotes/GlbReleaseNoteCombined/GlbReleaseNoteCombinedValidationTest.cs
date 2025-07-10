using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing;

public sealed class GlbReleaseNoteCombinedValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckGF_Category()
	{
		Note.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;

		AssertNoErrors("Precondition: GF_Category should not have errors.", Note.GF_CategoryInfo);
		AssertEquals("Precondition: Categories[1].Code should not be empty.", false, string.IsNullOrEmpty(Note.Lookups.Categories[1].Code));

		Note.GF_Category = "";
		AssertHasError(Note.GF_CategoryInfo, "Please enter a " + Note.GF_CategoryInfo.Description + ".");

		Note.GF_Category = "!@#";
		AssertHasError(Note.GF_CategoryInfo, "Enter a valid " + Note.GF_CategoryInfo.Description + ".");

		Note.GF_Category = Note.Lookups.Categories[1].Code;
		AssertNoErrors(Note.GF_CategoryInfo);

		Note.GF_Section = NewsSectionTypeList.Codes.WiseLearningUpdates;
		Note.GF_Category = "!@#";
		AssertHasErrors(Note.GF_CategoryInfo);

		Note.GF_Category = "";
		AssertNoErrors(Note.GF_CategoryInfo);
	}

	public void TestCheckGF_Section()
	{
		Note.GF_Section = "";
		AssertHasError(Note.GF_SectionInfo, "Please enter a " + Note.GF_SectionInfo.Description + ".");

		Note.GF_Section = "!@#";
		AssertHasError(Note.GF_SectionInfo, "Enter a valid " + Note.GF_SectionInfo.Description + ".");

		Note.GF_Section = NewsSectionTypeList.Codes.WiseNews;
		AssertNoErrors("Precondition: GF_Section should not have errors.", Note.GF_SectionInfo);

		foreach (var section in NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.WiseTechOnly).GetAllCodes())
		{
			Note.GF_Section = section;
			AssertNoErrors($"GlbReleaseNoteCombined.GF_Section should not have an error for: {section}", Note.GF_SectionInfo);
		}

		foreach (var section in NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.CustomerOnly).GetAllCodes())
		{
			Note.GF_Section = section;
			AssertHasErrors($"GlbReleaseNoteCombined.GF_Section should have an error for: {section}", Note.GF_SectionInfo);
		}
	}
	public void TestCheckGF_Summary()
	{
		AssertNoErrors("Precondition: GF_Summary should not have errors.", Note.GF_SummaryInfo);

		Note.GF_Summary = "";
		AssertHasError(Note.GF_SummaryInfo, "Please enter a Summary.");

		Note.GF_Summary = "Summary";
		AssertNoErrors(Note.GF_SummaryInfo);
	}

	public void TestCheckGF_URL()
	{
		AssertNoErrors("Precondition: GF_URL should not have errors.", Note.GF_URLInfo);

		Note.GF_URL = "";
		AssertHasError(Note.GF_URLInfo, "Please enter a Link.");

		Note.GF_URL = "Link";
		AssertNoErrors(Note.GF_URLInfo);
	}

	public void TestCheckGF_RN_NKCountryForReleaseNote()
	{
		AssertNoErrors("Precondition: GF_RN_NKCountryForReleaseNote should not have errors.", Note.GF_RN_NKCountryForReleaseNoteInfo);
		AssertEquals("Precondition: Countries[1].Code should not be empty.", false, string.IsNullOrEmpty(Note.Lookups.Countries[1].Code));

		Note.GF_RN_NKCountryForReleaseNote = "";
		AssertNoErrors(Note.GF_RN_NKCountryForReleaseNoteInfo);

		Note.GF_RN_NKCountryForReleaseNote = "!#";
		AssertHasError(Note.GF_RN_NKCountryForReleaseNoteInfo, "Enter a valid " + Note.GF_RN_NKCountryForReleaseNoteInfo.Description + ".");

		Note.GF_RN_NKCountryForReleaseNote = Note.Lookups.Countries[1].Code;
		AssertNoErrors(Note.GF_RN_NKCountryForReleaseNoteInfo);
	}

	#region Implementation

	protected override void SetUp()
	{
		base.SetUp();
		Note = GetNewNote();
	}

	GlbReleaseNoteCombined GetNewNote() => Factory.New<GlbReleaseNoteCombined>();

	GlbReleaseNoteCombined Note { get; set; }

	#endregion
}
