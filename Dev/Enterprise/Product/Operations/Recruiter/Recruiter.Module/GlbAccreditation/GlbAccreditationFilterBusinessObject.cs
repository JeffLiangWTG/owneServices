using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", GlbAccreditationSchema.HAC_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbAccreditationFilter|Code", "Code");
			filters.AddTextFilter("Description", GlbAccreditationSchema.HAC_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbAccreditationFilter|Description", "Description");
			var certificateCodeFilter = filters.AddTextFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.CertificateCode, GlbAccreditationSchema.HAC_CertificateCode, Certificates);
			certificateCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbAccreditationFilter|CertificateCode", "Certificate Code");
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var webPublishedFilter = filters.AddFlagsFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.WebPublished, new string[] { ResString.GetMultilingualString("Recruiter|GlbAccreditationFilter|WebPublished", "Web Published") }, new GetFlagsQuery[] { GetWebPublishedFilter });
			webPublishedFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbAccreditationFilter|WebPublished", "Web Published");
			var isRefresherFilter = filters.AddFlagsFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.IsRefresher, new string[] { ResString.GetMultilingualString("Recruiter|GlbAccreditationFilter|IsRefresher", "Is Refresher") }, new GetFlagsQuery[] { GetIsRefresherFilter });
			isRefresherFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbAccreditationFilter|IsRefresher", "Is Refresher");
		}

		ZQuery GetIsRefresherFilter(ZBool value)
		{
			return new ZQuery(GlbAccreditationSchema.HAC_IsRefresher, value);
		}

		ZQuery GetWebPublishedFilter(ZBool value)
		{
			return new ZQuery(GlbAccreditationSchema.HAC_IsWebPublished, value);
		}

		public ICodeDescriptionPairList Certificates
		{
			get { return RecruiterDataRegistry.Instance.CertificateTypesExtra.Value.GetActiveCodeDescriptionPairList(); }
		}
	}
}
