using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Global.Testing
{
	[TestedType(typeof(NewsAndAnnouncementForm))]
	sealed class NewsAndAnnouncementFormForm : ZFormBasherTest
	{
		public void TestCountryDescriptionIsSetForEmptyCode()
		{
			var newsItem = Factory.New<NewsAnnouncement>();
			AssertEquals("Precondition", ZString.Empty, newsItem.GF_RN_NKCountryForReleaseNote);

			using (var form = new NewsAndAnnouncementForm(newsItem))
			{
				form.Show();
				var dropEdit = ControlTestHelper.FindControls<ZDropEdit>(form).SingleOrDefault(x => x.Name == "zDropEdit2");
				AssertEquals("All Countries/Regions", dropEdit.DescriptionBox.Text);
			}
		}

		public void TestSupportsEDocsIsFalse()
		{
			var newsItem = Factory.New<NewsAnnouncement>();
			using (var form = new NewsAndAnnouncementFormForTest(newsItem))
			{
				AssertEquals(expected: false, form.TestSupportsEDocs);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new NewsAndAnnouncementForm(Factory.New<NewsAnnouncement>());
		}

		protected override bool AllowFormSizeFixed => true;

		class NewsAndAnnouncementFormForTest : NewsAndAnnouncementForm
		{
			public NewsAndAnnouncementFormForTest(NewsAnnouncement newsItem)
				: base(newsItem)
			{ }

			public bool TestSupportsEDocs => SupportsEDocs;
		}
	}
}
