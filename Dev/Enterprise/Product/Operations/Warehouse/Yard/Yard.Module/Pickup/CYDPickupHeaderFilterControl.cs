using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class CYDPickupHeaderFilterControl : ZFilterStripControl
	{
		public CYDPickupHeaderFilterControl()
			: this(null, null)
		{
		}

		public CYDPickupHeaderFilterControl(IBusinessObjectCollection gridCollection, CYDPickupHeaderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in MNRWorkOrderFilterControl")]
		readonly CYDPickupHeaderFilterBusinessObject filterBusinessObject;
	}
}
