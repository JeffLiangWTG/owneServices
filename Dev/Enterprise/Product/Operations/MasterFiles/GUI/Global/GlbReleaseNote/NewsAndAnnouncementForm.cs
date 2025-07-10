using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class NewsAndAnnouncementForm : ZTemplateForm
	{
		public NewsAndAnnouncementForm(NewsAnnouncement newsItem)
			: base(newsItem)
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return BusinessEntity.HumanReadableName; }
		}

		protected override bool SupportsEDocs => false;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var newsItem = BusinessEntity as NewsAnnouncement;
			if (newsItem != null && newsItem.GF_RN_NKCountryForReleaseNote.IsEmpty)
			{
				zDropEdit2.OnItemSelected(newsItem.Lookups.Countries[""], false);
			}
		}
	}
}
