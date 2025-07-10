using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemTransportationUnitRatingAdaptersProvider<T> : RatingAdaptersProvider<T>
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitTransportationUnitForRating,
		IRatingSupporter,
		IJobInvoicingPlugIn,
		ITransitJobInvoicingPlugIn
	{
		public WhsItemTransportationUnitRatingAdaptersProvider(T transportationUnit)
			: base(transportationUnit)
		{
		}

		protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			if (parent.FreightMode == FreightMode.UKN)
			{
				uiInteractor.Error(LogMessages.RatingAdaptersCannotBeCreated($"{options.AutoratingProcess}: Could not determine Transport Mode."));   // Log messages are English only for now
				return new List<IAutoRating>();
			}
			else
			{
				return new List<IAutoRating> { new WhsItemTransportationUnitRatingAdapter<T>(parent) };
			}
		}
	}
}
