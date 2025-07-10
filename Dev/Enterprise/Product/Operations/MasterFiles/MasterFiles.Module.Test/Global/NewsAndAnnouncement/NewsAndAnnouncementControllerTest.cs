using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(NewsAndAnnouncementController))]
	sealed class NewsAndAnnouncementControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.NewsAndAnnouncement;
		}

		public void TestUrlsCanBeOpenedByAnyCompany()
		{
			Assert("News And Announcement hyperlinks should not be restricted to the current company", !Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
