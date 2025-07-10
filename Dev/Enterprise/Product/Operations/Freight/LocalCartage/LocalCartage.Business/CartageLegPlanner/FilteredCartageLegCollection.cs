using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Business
{
	[ModuleID(ModuleId.CartageLeg)]
	public class FilteredCartageLegCollection : BusinessObjectCollection<CommonCartageLeg>
	{
		public FilteredCartageLegCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
