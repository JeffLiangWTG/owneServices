using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NCTSReleaseNotificationTest : NctsCodeListDetailsAbstractTest
	{

		[Test]
		public void CodeType()
		{
			Assert.That(releaseNotification.CodeType, Is.EqualTo("CL164"));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(releaseNotification.CodeListType, Is.EqualTo("ReleaseNotification"));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(releaseNotification.DataSource, Is.EqualTo("EUN Release Notification"));
		}

		[SetUp]
		public void Setup()
		{
			releaseNotification = GetNctsCodeListDetails() as NCTSReleaseNotification;
		}
		NCTSReleaseNotification releaseNotification;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NCTSReleaseNotification();
	}
}
