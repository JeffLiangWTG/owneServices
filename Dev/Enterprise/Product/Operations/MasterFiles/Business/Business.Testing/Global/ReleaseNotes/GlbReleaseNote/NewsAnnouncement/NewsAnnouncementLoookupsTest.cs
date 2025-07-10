using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NewsAnnouncementLoookupsTest : GlbReleaseNoteLookupsTest
	{
		#region Tests
		public override void TestSectionList()
		{
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ProductUpdates));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.WiseLearningUpdates));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.WiseNews));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.BorderWise));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes));
			AssertEquals(false, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.WiseTechAcademy));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ClientStaffNews));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ClientAnnouncements));
			AssertEquals(true, Lookups.SectionList.ContainsCode(NewsSectionTypeList.Codes.ClientNews));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			parent = Factory.New<NewsAnnouncement>();
		}

		#endregion
	}
}
