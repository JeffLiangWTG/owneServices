using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Module
{
	public class ReviewProcessNodeFilterStripBusinessObject : FilterStripBusinessObject
	{
		public ReviewProcessNodeFilterStripBusinessObject()
			: this(new BusinessObjectFactory { NameForDebugging = nameof(ReviewProcessNodeFilterStripBusinessObject) })
		{
		}

		public ReviewProcessNodeFilterStripBusinessObject(BusinessObjectFactory factory)
			: base(factory) => LayoutContext = ModuleIDs.ReviewProcessNode.Name;

		protected override ModuleFilterCollection GetModuleFiltersCore()
			=> new ModuleFilterCollection();
	}
}
