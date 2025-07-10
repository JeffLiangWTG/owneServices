namespace Enterprise.Freight.Forwarding.Business
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using Enterprise.Freight.Forwarding.Orders.Business;
	using Enterprise.Integration.Accounting;
	using Enterprise.Integration.Rating;
	using Enterprise.MasterFiles.Business;

	public class OrderRatingAdaptersProvider : RatingAdaptersProvider
	{
		public OrderRatingAdaptersProvider(Order order)
		{
			this.order = order;
		}

		protected override List<IAutoRating> GetAdaptersCore(IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new OrderRatingAdapter(order) };
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobsCore()
		{
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(new List<IJobInvoicingPlugIn>());
		}

		readonly Order order;
	}
}
