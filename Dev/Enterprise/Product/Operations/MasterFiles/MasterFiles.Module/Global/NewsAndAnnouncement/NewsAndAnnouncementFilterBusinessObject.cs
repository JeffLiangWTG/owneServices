using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class NewsAndAnnouncementFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddFiltersForTranslatableText("Summary", GlbReleaseNoteSchema.GF_Summary, typeof(GlbReleaseNote), ResString.GetMultilingualString("MasterFiles|NewsAndAnnouncementFilter|Summary", "Summary"));
			result.AddTextFilter("Section", GlbReleaseNoteSchema.GF_Section, () => NewsAnnouncementSectionListRetriever.GetList(NewsAnnouncementSectionListMode.CustomerOnly)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|NewsAndAnnouncementFilter|Section", "Section");
			result.AddDateFilter("Published Time", GlbReleaseNoteSchema.GF_ReleaseNoteDate, true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|NewsAndAnnouncementFilter|PublishedTime", "Published Time");

			return result;
		}
	}
}
