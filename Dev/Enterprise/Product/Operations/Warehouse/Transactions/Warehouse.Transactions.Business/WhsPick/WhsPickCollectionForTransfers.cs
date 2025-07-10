using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.WhsPicking)]
	public class WhsPickCollectionForTransfers : ActiveBusinessObjectCollection<WhsPick>
	{
		public WhsPickCollectionForTransfers(BusinessObjectFactory factory, ZGuid whsPK)
			: base(factory)
		{
			AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults, whsPK);
		}

		void AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults filterDefaults, ZGuid whsPK)
		{
			AdditionalFilter = new ZQuery(WhsPickSchema.WP_WW_Whs, whsPK);
			filterDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", whsPK, false));
			filterDefaults.Add(new FilterBusinessObjectDefault("Status", "Property", (ZString)"NEW", false));
		}

		protected override bool AllowNew => false;
	}
}
