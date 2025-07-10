using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module
{
	public partial class DtbBookingTmplFilterControl : ZFilterStripControl
	{
		// for the designer
		public DtbBookingTmplFilterControl()
			: this(null, null)
		{
		}

		public DtbBookingTmplFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
