using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	public class OnlineSchedulesModuleNumberFilter : ModuleNumberFilter
	{
		public OnlineSchedulesModuleNumberFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public OnlineSchedulesModuleNumberFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public OnlineSchedulesModuleNumberFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public override bool HasComparisonOperator => false;
	}
}
