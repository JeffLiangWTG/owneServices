using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class NewsAnnouncementCollection : ActiveBusinessObjectCollection<NewsAnnouncement>
	{
		public NewsAnnouncementCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(GlbReleaseNoteSchema.GF_Section, SQLComparisonOperator.NotEqual, ExcludedNewsSectionTypes))
		{
		}

		public static string[] ExcludedNewsSectionTypes => new[] {
			NewsSectionTypeList.Codes.ProductUpdates,
			NewsSectionTypeList.Codes.WiseLearningUpdates,
			NewsSectionTypeList.Codes.WiseNews,
			NewsSectionTypeList.Codes.TechnicalAdvisoryNotes,
			NewsSectionTypeList.Codes.BorderWise,
			NewsSectionTypeList.Codes.WiseTechAcademy
		};
	}
}
