using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class CYDYardUnitStateFilterControl : ZFilterStripControl
	{
		public CYDYardUnitStateFilterControl()
			: this(null, null)
		{
		}

		public CYDYardUnitStateFilterControl(IBusinessObjectCollection gridCollection, CYDYardUnitStateFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CYDYardUnitStateFilterControl")]
		readonly CYDYardUnitStateFilterBusinessObject filterBusinessObject;
	}
}
