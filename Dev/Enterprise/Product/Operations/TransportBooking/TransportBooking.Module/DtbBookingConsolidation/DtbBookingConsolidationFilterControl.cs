using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module
{
	public partial class DtbBookingConsolidationFilterControl : ZFilterStripControl
	{
		// for the designer
		public DtbBookingConsolidationFilterControl()
			: this(null, null)
		{
		}

		public DtbBookingConsolidationFilterControl(IBusinessObjectCollection gridCollection, DtbBookingConsolidationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new WorkflowFilterStrip();
		}
	}
}
