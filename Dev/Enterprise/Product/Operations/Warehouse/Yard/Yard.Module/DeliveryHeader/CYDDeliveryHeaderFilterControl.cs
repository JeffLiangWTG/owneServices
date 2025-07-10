using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class CYDDeliveryHeaderFilterControl : ZFilterStripControl
	{
		public CYDDeliveryHeaderFilterControl()
			: this(null, null)
		{
		}

		public CYDDeliveryHeaderFilterControl(IBusinessObjectCollection gridCollection, CYDDeliveryHeaderFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CYDDeliveryHeaderFilterControl")]
		readonly CYDDeliveryHeaderFilterBusinessObject filterBusinessObject;
	}
}
