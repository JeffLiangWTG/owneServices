using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentJobDatesProvider : DtbTransportJobDatesProvider<DtbBookingConsignment>
	{
		readonly DtbTransportInstruction fromInstruction;
		readonly DtbTransportInstruction toInstruction;

		public DtbBookingConsignmentJobDatesProvider(DtbBookingConsignment consignment, DtbTransportInstruction fromInstruction, DtbTransportInstruction toInstruction)
			: base(consignment)
		{
			this.fromInstruction = fromInstruction;
			this.toInstruction = toInstruction;
		}

		protected override IEnumerable<ZDateTime> JobArrivalDates
		{
			get { return GetDates((DtbConsignmentInstruction)toInstruction); }
		}

		protected override IEnumerable<ZDateTime> JobDepartureDates
		{
			get { return GetDates((DtbConsignmentInstruction)fromInstruction); }
		}

		IEnumerable<ZDateTime> GetDates(DtbConsignmentInstruction instruction)
		{
			var result = new List<ZDateTime>();
			if (instruction != null)
			{
				result.AddRange(instruction.Confirmations.Where(c => !c.KK_Actual.IsEmpty).Select(c => c.KK_Actual));
				if (!result.Any())
				{
					result.AddRange(instruction.Confirmations.Where(c => !c.KK_Estimated.IsEmpty).Select(c => c.KK_Estimated));
				}
			}
			return result;
		}
	}
}
