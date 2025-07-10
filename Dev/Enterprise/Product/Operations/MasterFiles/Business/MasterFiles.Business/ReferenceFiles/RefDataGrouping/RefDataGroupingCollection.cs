using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefDataGrouping)]
	public class RefDataGroupingCollection : ActiveBusinessObjectCollection<RefDataGrouping>
	{
		public RefDataGroupingCollection(RefDataGrouping parentDataGrouping)
			: base(parentDataGrouping.Factory, parentDataGrouping, new ZQuery(), RefDataGroupingSchema.ZZZ_ZZZ_Grouping)
		{
		}

		public RefDataGroupingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefDataGroupingCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		protected override bool AllowNew => false;
	}
}
