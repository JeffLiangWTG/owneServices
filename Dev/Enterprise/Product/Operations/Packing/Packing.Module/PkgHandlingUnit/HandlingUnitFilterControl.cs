using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.Module
{
	public abstract partial class HandlingUnitFilterControl : ZFilterStripControl
	{
		// for the designer
		protected HandlingUnitFilterControl()
			: this(null, null)
		{
		}

		protected HandlingUnitFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
