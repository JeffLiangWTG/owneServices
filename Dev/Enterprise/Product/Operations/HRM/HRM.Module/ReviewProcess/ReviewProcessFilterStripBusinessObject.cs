using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Module
{
	public class ReviewProcessFilterStripBusinessObject : FilterStripBusinessObject
	{
		public ReviewProcessFilterStripBusinessObject()
			: this(new BusinessObjectFactory { NameForDebugging = nameof(ReviewProcessFilterStripBusinessObject) })
		{
		}

		public ReviewProcessFilterStripBusinessObject(BusinessObjectFactory factory)
			: base(factory) => LayoutContext = ModuleIDs.ReviewProcess.Name;

		protected override ModuleFilterCollection GetModuleFiltersCore()
			=> new ModuleFilterCollection();
	}
}
