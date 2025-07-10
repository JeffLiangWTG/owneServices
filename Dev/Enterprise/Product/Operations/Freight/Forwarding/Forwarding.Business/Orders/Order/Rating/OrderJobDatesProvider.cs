using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderJobDatesProvider : JobDatesProvider<Order>
	{
		public OrderJobDatesProvider(Order order)
			: base(order)
		{ }

		protected override ZDateTime GetDepartureDateCore()
		{
			var actualDate = Parent.GetMilestoneActualDate(Events.Departure);
			return (actualDate.IsValid
				? actualDate
				: Parent.GetMilestoneEstimatedDate(Events.Departure)).ToZDateTime();
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			var actualDate = Parent.GetMilestoneActualDate(Events.Arrival);
			return (actualDate.IsValid
				? actualDate
				: Parent.GetMilestoneEstimatedDate(Events.Arrival)).ToZDateTime();
		}
	}
}
