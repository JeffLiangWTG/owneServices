using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingJobDatesProvider : JobDatesProvider<DtbBooking>
	{
		public DtbBookingJobDatesProvider(DtbBooking dtbBooking)
			: base(dtbBooking) { }

		IEnumerable<ZDateTime> JobArrivalDates
		{
			get { return GetActualDtbBookingDates(); }
		}

		IEnumerable<ZDateTime> JobDepartureDates
		{
			get { return GetActualDtbBookingDates(); }
		}

		IEnumerable<ZDateTime> GetActualDtbBookingDates()
		{
			var dates = new List<ZDateTime>();
			foreach (var instruction in Parent.Instructions)
			{
				var confirmations = instruction.Confirmations;
				dates.AddRange(confirmations.Where(c => !c.KK_Actual.IsEmpty).Select(c => c.KK_Actual));
			}

			if (!dates.Any())
			{
				foreach (var instruction in Parent.Instructions)
				{
					var confirmations = instruction.Confirmations;
					dates.AddRange(confirmations.Where(c => !c.KK_Estimated.IsEmpty).Select(c => c.KK_Estimated));
				}
			}

			return dates;
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			var dates = JobArrivalDates.ToList();
			return dates.Any() ? new ZDateTime(dates.Max(d => d.Ticks)) : ZDateTime.Empty;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			var dates = JobDepartureDates.ToList();
			return dates.Any() ? new ZDateTime(dates.Min(d => d.Ticks)) : ZDateTime.Empty;
		}
	}
}
