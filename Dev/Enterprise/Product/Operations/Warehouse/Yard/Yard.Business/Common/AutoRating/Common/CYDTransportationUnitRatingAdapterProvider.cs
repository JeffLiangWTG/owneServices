using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitRatingAdapterProvider<T>(T transportationUnit) : RatingAdaptersProvider<T>(transportationUnit)
		where T : CYDTransportationUnit, IRatingSupporter, IJobInvoicingPlugIn
	{
		protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor interactor, AutoRateOptions options)
		{
			var adapters = new List<IAutoRating>();

			if (!parent.YTU_GateInTime.IsEmpty)
			{
				adapters.Add(new CYDTransportationUnitRatingAdapter<CYDTransportationUnit, CYDReceiveTransportationUnit>(
					parent, new CYDReceiveTransportationUnit(parent)
				));
			}

			if (!parent.YTU_GateOutTime.IsEmpty)
			{
				adapters.Add(new CYDTransportationUnitRatingAdapter<CYDTransportationUnit, CYDReleaseTransportationUnit>(
					parent, new CYDReleaseTransportationUnit(parent)
				));
			}

			return adapters;
		}
	}
}
