using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentJobDatesProvider : JobDatesProvider<DtbConsignment>
	{
		public DtbConsignmentJobDatesProvider(DtbConsignment consignment, DtbConsignmentAddress fromAddress, DtbConsignmentAddress toAddress)
			: base(consignment)
		{
			this.fromAddress = fromAddress;
			this.toAddress = toAddress;
		}

		readonly DtbConsignmentAddress fromAddress;
		readonly DtbConsignmentAddress toAddress;

		#region GetArrivalDateCore

		protected sealed override ZDateTime GetArrivalDateCore()
		{
			var dates = GetDates(toAddress);
			return dates.Any() ? new ZDateTime(dates.Max(d => d.Ticks)) : ZDateTime.Empty;
		}

		#endregion

		#region GetDepartureDateCore

		protected sealed override ZDateTime GetDepartureDateCore()
		{
			var dates = GetDates(fromAddress);
			return dates.Any() ? new ZDateTime(dates.Min(d => d.Ticks)) : ZDateTime.Empty;
		}

		#endregion

		#region GetDates

		ZDateTimeOffset[] GetDates(DtbConsignmentAddress address)
		{
			var result = new List<ZDateTimeOffset>();
			if (address != null)
			{
				result.AddRange(address.Actions.Where(c => !c.LTA_ActualTime.IsEmpty).Select(c => c.LTA_ActualTime));
				if (!result.Any())
				{
					result.AddRange(address.Actions.Where(c => !c.LTA_EstimatedTime.IsEmpty).Select(c => c.LTA_EstimatedTime));
				}
			}
			return result.ToArray();
		}

		#endregion
	}
}
