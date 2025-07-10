namespace Enterprise.Freight.Forwarding.Routing.S8.GUI
{
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;
	public class RealTimeRoutingModuleNumberFilter : ModuleNumberFilter
	{
		public RealTimeRoutingModuleNumberFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public RealTimeRoutingModuleNumberFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public RealTimeRoutingModuleNumberFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public override bool HasComparisonOperator => false;
	}
}
