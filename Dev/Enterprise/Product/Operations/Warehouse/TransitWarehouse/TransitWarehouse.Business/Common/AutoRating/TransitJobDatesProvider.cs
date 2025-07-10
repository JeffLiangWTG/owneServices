using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitJobDatesProvider<T> : JobDatesProvider<T>
		where T : BusinessObject, ITransitJobForRating
	{
		public TransitJobDatesProvider(T parent)
			: base(parent)
		{
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.ExpectedArrivalDate;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.ExpectedDepartureDate;
		}
	}
}
