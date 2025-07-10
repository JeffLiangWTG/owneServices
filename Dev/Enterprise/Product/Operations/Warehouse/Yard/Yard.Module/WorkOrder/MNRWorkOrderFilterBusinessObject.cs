using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class MNRWorkOrderFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => MNRWorkOrderHeaderSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => MNRWorkOrderHeaderSchema.MWO_WW_Facility;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddTypeFilter(filters);

			return filters;
		}

		#region TypeList

		CodeDescriptionPairList typeList;

		public CodeDescriptionPairList TypeList
		{
			get
			{
				if (typeList == null)
				{
					typeList = new CodeDescriptionPairList();
					typeList.AddPair("MAC", "MAC");
					typeList.AddPair("STL", "STL");
				}
				return typeList;
			}
		}

		#endregion

		#region AddTypeFilter

		void AddTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Type", GetTypeQuery, TypeList);
			filter.Category = FilterCategories.TextSearch;
			filter.MultilingualDescription = ResString.GetMultilingualString("MNRWorkOrder|MNRWorkOrderFilterBusinessObject|Type", "Type");
		}

		ZQuery GetTypeQuery(ZString type)
		{
			var query = new ZQuery();
			if (!type.IsEmpty)
			{
				query.AddToFilter(MNRWorkOrderHeaderSchema.MWO_Type, type);
			}
			return query;
		}

		#endregion
	}
}
