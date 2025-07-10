using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class NewsAnnouncement : GlbReleaseNote
	{
		public NewsAnnouncement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("NewsAnnouncement|ReleaseNoteDateLocal", Caption = "Published Time")]
		public ZDateTime ReleaseNoteDateLocal
		{
			get { return GF_ReleaseNoteDate.ToLocalBranchTime(); }
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.GF_Section = ZString.Empty;
			this.GF_ReleaseNoteDate = ZDateTime.UtcNow;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("f480ae12-890d-4d0f-a960-5c8733566534", "News & Announcement Item"); }
		}

		#endregion

		protected override GlbReleaseNoteLookups GetNewLookups()
		{
			return new NewsAnnouncementLoookups(this);
		}
	}
}
