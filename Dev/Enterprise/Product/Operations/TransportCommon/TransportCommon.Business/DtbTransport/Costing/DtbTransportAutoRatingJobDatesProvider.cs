using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportJobDatesProvider<T> : JobDatesProvider<T>
		where T : DtbTransport
	{
		protected DtbTransportJobDatesProvider(T transport)
			: base(transport)
		{
		}

		protected sealed override ZDateTime GetArrivalDateCore()
		{
			var dates = JobArrivalDates.ToList();
			return dates.Any() ? new ZDateTime(dates.Max(d => d.Ticks)) : ZDateTime.Empty;
		}

		protected sealed override ZDateTime GetDepartureDateCore()
		{
			var dates = JobDepartureDates.ToList();
			return dates.Any() ? new ZDateTime(dates.Min(d => d.Ticks)) : ZDateTime.Empty;
		}

		protected abstract IEnumerable<ZDateTime> JobArrivalDates { get; }
		protected abstract IEnumerable<ZDateTime> JobDepartureDates { get; }
	}
}
