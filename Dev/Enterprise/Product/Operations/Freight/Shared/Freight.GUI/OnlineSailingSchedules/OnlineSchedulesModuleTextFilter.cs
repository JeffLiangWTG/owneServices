using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	public class OnlineSchedulesModuleTextFilter : ModuleTextFilter
	{
		public OnlineSchedulesModuleTextFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public OnlineSchedulesModuleTextFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}
		public OnlineSchedulesModuleTextFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public override bool HasComparisonOperator => false;
	}
}
