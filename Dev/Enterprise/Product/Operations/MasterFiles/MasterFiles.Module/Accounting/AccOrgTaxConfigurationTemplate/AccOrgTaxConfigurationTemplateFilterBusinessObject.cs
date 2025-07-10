using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccOrgTaxConfigurationTemplateFilterBusinessObject : FilterStripBusinessObject
	{
		public AccOrgTaxConfigurationTemplateFilterBusinessObject()
		{
		}

		#region Filter
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddTemplateType(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Template Code", AccOrgTaxConfigurationTemplateSchema.OCT_Code).MultilingualDescription = ResString.GetMultilingualString("ab93e6fd-a09f-4bc5-a4cb-808771f2935e", "Template Code");
			filters.AddTextFilter("Template Description", AccOrgTaxConfigurationTemplateSchema.OCT_Description).MultilingualDescription = ResString.GetMultilingualString("28b324ca-a8c0-4038-9417-cba910497f5f", "Template Description");
		}

		void AddTemplateType(ModuleFilterCollection filters)
		{
			var templateTypeFilter = filters.AddTextFilter("Template Type", GetTemplateTypeQuery, CancelledTemplateTypeList);
			templateTypeFilter.MultilingualDescription = ResString.GetMultilingualString("48632b6c-fa96-4652-a535-9b5fea7b88c5", "Template Type");
		}

		public static string TemplateTypeAll
		{
			get { return Res.GetString("53b47b20-4e9e-4b02-b50b-1f963b78df28", "All"); }
		}
		public static string TemplateTypeReceivable
		{
			get { return Res.GetString("9a8334e7-bc43-4841-8d1c-89b65c54307a", "A/R"); }
		}
		public static string TemplateTypePayable
		{
			get { return Res.GetString("454c5c97-53cd-4005-88c4-e4534a927ab9", "A/P"); }
		}

		ZQuery GetTemplateTypeQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value.Equals(TemplateTypeReceivable))
			{
				query.AddToFilter(AccOrgTaxConfigurationTemplateSchema.OCT_IsReceivable, true);
			}
			else if (value.Equals(TemplateTypePayable))
			{
				query.AddToFilter(AccOrgTaxConfigurationTemplateSchema.OCT_IsReceivable, false);
			}
			return query;
		}

		public static CodeDescriptionPairList CancelledTemplateTypeList
		{
			get
			{
				var codes = new CodeDescriptionPairList();
				codes.AddPair(TemplateTypeAll, ResString.GetMultilingualString("cc534dbb-0b13-4280-8579-8b1061d93a18", "All Templates"));
				codes.AddPair(TemplateTypeReceivable, ResString.GetMultilingualString("{8dbe575d-7f86-4291-99b9-4cab344c2cf5", "Receivables Templates Only"));
				codes.AddPair(TemplateTypePayable, ResString.GetMultilingualString("9150dad5-2b27-4d5b-8c64-de92a435dfa2", "Payables Templates Only"));

				return codes;
			}
		}

		#endregion Filter
	}
}
