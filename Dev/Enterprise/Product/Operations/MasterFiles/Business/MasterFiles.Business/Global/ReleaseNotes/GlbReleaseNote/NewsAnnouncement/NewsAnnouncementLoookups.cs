using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class NewsAnnouncementLoookups : GlbReleaseNoteLookups
	{
		public NewsAnnouncementLoookups(NewsAnnouncement parent) : base(parent)
		{
		}

		public override ReadOnlyCodeDescriptionPairList SectionList => sectionList ?? (sectionList = NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.CustomerOnly));
		ReadOnlyCodeDescriptionPairList sectionList;
	}
}
