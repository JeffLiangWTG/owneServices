using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.Module
{
	public partial class CYDTransportationUnitFilterControl : ZFilterStripControl
	{
		public CYDTransportationUnitFilterControl()
			: this(null, null)
		{
		}

		public CYDTransportationUnitFilterControl(IBusinessObjectCollection gridCollection, CYDTransportationUnitFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			this.filterBusinessObject = filterBusinessObject;

			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CYDTransportationUnitFilterControl")]
		readonly CYDTransportationUnitFilterBusinessObject filterBusinessObject;
	}
}
