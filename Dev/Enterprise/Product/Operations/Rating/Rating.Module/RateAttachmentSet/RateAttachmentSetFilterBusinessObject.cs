using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	public class RateAttachmentSetFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddFiltersForTranslatableText("Document Name", RateAttachmentSetSchema.TS_AttachmentName, typeof(RateAttachmentSet), ResString.GetMultilingualString("Rating|RateAttachmentSetFilterBusinessObject|DocumentName", "Document Name"));

			filters.AddGuidFilter("Company Name", ModuleIDs.GlbCompany, RateAttachmentSetSchema.TS_GC, TS_GCList).MultilingualDescription = ResString.GetMultilingualString("Rating|RateAttachmentSetFilterBusinessObject|CompanyName", "Company Name");

			ModuleTextFilter typeFilter = filters.AddTextFilter("Type", RateAttachmentSetSchema.TS_TemplateType, TS_TemplateType_List);
			typeFilter.Category = FilterCategories.StatusAndFlags;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("Rating|RateAttachmentSetFilterBusinessObject|Type", "Type");

			filters.AddFlagsFilter(
					"Configuration",
					new string[] { Res.GetString("Rating|RateAttachmentSetFilterBusinessObject|DefaultPage", "Default Page"), Res.GetString("Rating|RateAttachmentSetFilterBusinessObject|MandatoryPage", "Mandatory Page") },
					new SchemaBoolColumn[] { RateAttachmentSetSchema.TS_IsDefault, RateAttachmentSetSchema.TS_IsMandatory }
				).MultilingualDescription = ResString.GetMultilingualString("Rating|RateAttachmentSetFilterBusinessObject|Configuration", "Configuration");

			return filters;
		}

		#endregion

		#region Lists

		CodeDescriptionPairList TS_TemplateType_List
		{
			get
			{
				if (fTS_TemplateType_List == null)
				{
					fTS_TemplateType_List = new RateAttachmentSetLookups(null).TemplateTypes;
				}

				return fTS_TemplateType_List;
			}
		}

		CodeDescriptionPairList fTS_TemplateType_List;

		#endregion

		#region TS_GC_List

		GlbCompanyCollection TS_GCList
		{
			get { return fTS_GCList ?? (fTS_GCList = new GlbCompanyCollection(Factory)); }
		}

		GlbCompanyCollection fTS_GCList;

		#endregion
	}
}

