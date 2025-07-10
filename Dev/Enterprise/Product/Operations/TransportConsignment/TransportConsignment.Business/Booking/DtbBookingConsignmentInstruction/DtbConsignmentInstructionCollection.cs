using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInstructionCollection : DtbTransportInstructionCollection<DtbConsignmentInstruction>
	{
		public DtbConsignmentInstructionCollection(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		public DtbConsignmentInstructionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IEnumerable<DtbConsignmentInstruction> Deliveries
		{
			get { return this.Where(i => i.IsDelivery); }
		}

		public IEnumerable<DtbConsignmentInstruction> PickUps
		{
			get { return this.Where(i => i.IsPickUp); }
		}
	}
}
