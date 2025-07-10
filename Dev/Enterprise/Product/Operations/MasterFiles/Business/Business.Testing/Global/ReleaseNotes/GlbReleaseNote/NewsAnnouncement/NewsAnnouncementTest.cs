using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NewsAnnouncement))]
	sealed class NewsAnnouncementTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2021, 01, 17, 9, 32, 0)]
		public void TestDefaultValues()
		{
			var note = Factory.New<NewsAnnouncement>();
			AssertEquals(ZString.Empty, note.GF_Section);
			AssertEquals(ZDateTime.UtcNow, note.GF_ReleaseNoteDate);
		}

		public void TestReleaseNoteDateLocal()
		{
			var releaseNoteDateUtc = ZDateTime.UtcNow.AddDays(-1);
			var note = Factory.New<NewsAnnouncement>();
			note.GF_ReleaseNoteDate = releaseNoteDateUtc;
			AssertEquals(releaseNoteDateUtc.ToLocalBranchTime(), note.ReleaseNoteDateLocal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}
	}
}
