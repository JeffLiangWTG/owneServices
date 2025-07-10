using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemConsignmentRatingAdaptersProvider<T> : RatingAdaptersProvider<T>
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitConsignmentForRating,
		IHaveServices,
		IRatingSupporter,
		IJobInvoicingPlugIn,
		ITransitJobInvoicingPlugIn,
		ITransitJobForConsolCosting
	{
		public WhsItemConsignmentRatingAdaptersProvider(T consignment)
			: base(consignment)
		{
		}

		protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new WhsItemConsignmentRatingAdapter<T>(parent) };
		}
	}
}
