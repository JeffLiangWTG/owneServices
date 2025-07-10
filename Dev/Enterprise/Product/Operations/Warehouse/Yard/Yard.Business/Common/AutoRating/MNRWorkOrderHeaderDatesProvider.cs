using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderDatesProvider<T> : JobDatesProvider<T>
		where T : BusinessObject, IYardWorkOrderForRating
	{
		ZDateTime? today;
		ZDateTime Today
		{
			get
			{
				if (today == null)
				{
					today = ZDateTime.Today;
				}
				return today.Value;
			}
		}

		public MNRWorkOrderHeaderDatesProvider(T parent) : base(parent)
		{
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return Today;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return Today.AddDays(2);
		}
	}
}
