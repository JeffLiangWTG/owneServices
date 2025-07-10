using Enterprise.TransportCommon.Module;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.Module
{
	public partial class DtbBookingConsignmentFilterControl : TransportFilterControl
	{
		// for the designer
		public DtbBookingConsignmentFilterControl()
			: this(null, null)
		{
		}

		public DtbBookingConsignmentFilterControl(DtbBookingConsignmentCollection gridCollection, DtbBookingConsignmentFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}

	// Inheriting directly from a generic class crashes designer, therfore a seperate class is created to extend the generic class
	public partial class TransportFilterControl : DtbTransportFilterControl<DtbBookingConsignment>
	{
		public TransportFilterControl()
		{
		}

		public TransportFilterControl(DtbBookingConsignmentCollection gridCollection, DtbBookingConsignmentFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
