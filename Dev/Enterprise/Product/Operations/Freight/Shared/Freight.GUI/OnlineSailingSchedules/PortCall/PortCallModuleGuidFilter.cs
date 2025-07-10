using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI
{
	public class PortCallModuleGuidFilter : ModuleGuidFilter
	{
		public PortCallModuleGuidFilter(ZString description, ModuleIdentifier iD, GetGuidQuery queryDelegate, IBusinessObjectCollection list)
			: base(description, iD, queryDelegate, list)
		{
		}

		public PortCallModuleGuidFilter(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, IBusinessObjectCollection list)
			: base(description, id, filterColumn, list)
		{
		}

		public override bool HasComparisonOperator => false;
	}
}
