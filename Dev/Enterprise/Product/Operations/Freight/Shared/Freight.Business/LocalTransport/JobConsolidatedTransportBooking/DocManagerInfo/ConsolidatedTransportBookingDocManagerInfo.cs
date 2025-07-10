using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	internal class ConsolidatedTransportBookingDocManagerInfo : DocManagerInfo
	{
		public ConsolidatedTransportBookingDocManagerInfo(CommonConsolidatedTransportBooking parent, ZString docManagerCode)
			: base(parent, docManagerCode)
		{
		}
	}
}
