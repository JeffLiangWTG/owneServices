using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Module;
using Enterprise.TransportCommon.Module.Testing;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.Module.Testing
{
	class DtbBookingConsignmentFilterControlTest : DtbTransportFilterControlTest<DtbBookingConsignment, DtbBookingConsignmentFilterBusinessObject>
	{
		#region Implementation

		protected override DtbTransportFilterControl<DtbBookingConsignment> GetFilterControl(DtbTransportCollection<DtbBookingConsignment> gridCollection, DtbBookingConsignmentFilterBusinessObject filterBizO)
		{
			return new DtbBookingConsignmentFilterControl((DtbBookingConsignmentCollection)gridCollection, filterBizO);
		}

		protected override DtbTransportCollection<DtbBookingConsignment> GetTransportCollection()
		{
			return new DtbBookingConsignmentCollection(Factory);
		}

		#endregion
	}
}
