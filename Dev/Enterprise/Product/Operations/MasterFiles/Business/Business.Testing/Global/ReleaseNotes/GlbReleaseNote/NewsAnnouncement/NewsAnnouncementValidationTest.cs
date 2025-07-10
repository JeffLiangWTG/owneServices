using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class NewsAnnouncementValidationTest : GlbReleaseNoteValidationTest
	{
		public override void TestCheckGF_Section()
		{
			Note.GF_Section = "";
			AssertHasError(Note.GF_SectionInfo, "Please enter a " + Note.GF_SectionInfo.Description + ".");

			Note.GF_Section = "!@#";
			AssertHasError(Note.GF_SectionInfo, "Enter a valid " + Note.GF_SectionInfo.Description + ".");

			Note.GF_Section = NewsSectionTypeList.Codes.ClientAnnouncements;
			AssertNoErrors("Pre-condition", Note.GF_SectionInfo);

			foreach (var section in NewsAnnouncementCollection.ExcludedNewsSectionTypes)
			{
				Note.GF_Section = section;
				AssertHasErrors($"NewsAnnouncement.GF_Section should have an error for: {section}", Note.GF_SectionInfo);
			}

			foreach (var section in NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.CustomerOnly).GetAllCodes())
			{
				Note.GF_Section = section;
				AssertNoErrors($"NewsAnnouncement.GF_Section should not have an error for: {section}", Note.GF_SectionInfo);
			}
		}

		protected override GlbReleaseNote GetNewNote() => Factory.NewWithValidTestData<NewsAnnouncement>();
	}
}
