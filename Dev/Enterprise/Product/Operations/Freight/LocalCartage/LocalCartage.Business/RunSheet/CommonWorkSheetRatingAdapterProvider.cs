using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetRatingAdapterProvider : RatingOrCostingAdaptersProvider<CommonWorkSheet>
	{
		public CommonWorkSheetRatingAdapterProvider(CommonWorkSheet workSheet)
			: base(workSheet, null)
		{
		}

		protected override List<IAutoRating> GetAdapters(CommonWorkSheet workSheet, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var adapters = base.GetAdapters(workSheet, uiInteractor, options.With(billingType: BillingType.Default));

			if (options.AutoratingProcess == CostSell.Cost)
			{
				adapters.Add(new CommonWorkSheetRatingAdapter(workSheet));

				// leg adapters (no services)
				var cartageLegs = workSheet.CartageLegs;
				var legAdapters = cartageLegs.Select(leg => new CartageLegRatingAdapter(leg, forWorkSheet: true, shouldAutorateServices: false));

				adapters.AddRange(legAdapters);

				// services only adapters
				adapters.AddRange(cartageLegs
					.Select(leg => leg.BookedCtgMove)
					.Distinct()
					.Select(move => new CartageMoveRatingAdapter(move, move.PickupFromDocAddress, move.DeliverToDocAddress, true)));
			}
			else
			{
				throw new NotSupportedException("Rating for Revenue on a runsheet is not supported");
			}

			return adapters;
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(CommonWorkSheet workSheet)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(workSheet));
			result.AddRange(workSheet.CartageLegs.Select(leg => leg.Cartage).Distinct());

			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}
	}
}
