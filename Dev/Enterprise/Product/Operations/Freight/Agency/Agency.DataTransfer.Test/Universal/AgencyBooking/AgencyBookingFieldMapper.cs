using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyBookingFieldMapper : CommonShipmentFieldMapper<AgencyBooking>
	{
		public AgencyBookingFieldMapper(AgencyBooking agencyBooking) : base(agencyBooking)
		{
			this.agencyBooking = agencyBooking;
		}

		readonly AgencyBooking agencyBooking;
		protected override IEnumerable<string> MapContainers()
		{
			return agencyBooking.BookedContainers.Cast<AgencyBookingContainer>().Select(ContainerToString);
		}
	}
}
