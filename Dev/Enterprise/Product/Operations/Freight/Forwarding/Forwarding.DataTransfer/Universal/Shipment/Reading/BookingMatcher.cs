using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class BookingMatcher : BaseShipmentMatcher<ForwardingShipment>
	{
		public BookingMatcher(BusinessObjectFactory factory, ShipmentReferences bookingReferences, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, bookingReferences, logger, helper)
		{
		}

		protected override void AddJobShipmentTypeFilter(ZQuery query)
		{
			query.AddToFilter(JobShipmentSchema.JS_IsBooking, ZBool.True);
			query.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.False);
		}

		protected override bool ShouldMatchCfsReference
		{
			get { return true; }
		}
	}
}
