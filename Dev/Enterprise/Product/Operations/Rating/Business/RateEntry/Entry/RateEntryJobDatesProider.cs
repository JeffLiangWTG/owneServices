using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class RateEntryJobDatesProider : JobDatesProvider<RateEntry>
	{
		public RateEntryJobDatesProider(RateEntry entry)
			: base(entry) { }

		protected override ZDateTime GetDepartureDateCore()
		{
			return GetDate();
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return GetDate();
		}

		ZDateTime GetDate()
		{
			var startDate = Parent.IsQuote() ? Parent.Parent.TH_QuoteDate : Parent.TI_RateStartDate;
			var endDate = Parent.IsQuote() ? Parent.Parent.TH_QuoteEndDate : Parent.TI_RateEndDate;

			if (startDate > ZDateTime.Today)
			{
				return startDate;
			}

			if (!endDate.IsEmpty && endDate < ZDateTime.Today)
			{
				return endDate;
			}

			return ZDateTime.Today;
		}
	}
}
