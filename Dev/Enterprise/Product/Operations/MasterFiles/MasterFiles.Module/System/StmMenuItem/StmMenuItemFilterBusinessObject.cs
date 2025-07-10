using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class StmMenuItemFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddFiltersForTranslatableText("Document Name", StmMenuItemSchema.SU_MenuName, typeof(StmMenuItem), ResString.GetMultilingualString("MasterFiles|StmTemplateFilter|DocumentName", "Document Name"));
			filters.AddTextFilter("Business Context", StmMenuItemSchema.SU_BusinessContext).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|StmTemplateFilter|BusinessContext", "Business Context");
		}

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter contactTypeFilter = filters.AddTextFilter("Document Group", StmMenuItemSchema.SU_ContactType, ContactTypes);
			contactTypeFilter.Category = FilterCategories.Other;
			contactTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|StmTemplateFilter|DocumentGroup", "Document Group");
		}

		public CodeDescriptionPairList ContactTypes
		{
			get { return OrgCodeLists.ContactType_List; }
		}
	}
}
