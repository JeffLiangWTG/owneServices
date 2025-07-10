using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitsRatingAdaptersProvider<TParent, TRatingUnits>(TParent parent, TRatingUnits unitsForRating) : RatingAdaptersProvider<TParent>(parent)
		where TParent : BusinessObject, IJobHeaderParent, IJobNumber, IJobInvoicingPlugIn, IRatingSupporter, IPeriodicInvoicing
		where TRatingUnits : ICYDYardUnitsForRating
	{
		protected override List<IAutoRating> GetAdapters(TParent parent, IAutoRatingInteractor interactor, AutoRateOptions options)
		{
			var adapters = new List<IAutoRating>();
			var yardUnits = unitsForRating.YardUnits;
			foreach (var yardUnit in yardUnits)
			{
				adapters.Add(GetRatingAdpater(parent, unitsForRating, yardUnit));
			}

			return adapters;
		}

		protected virtual CYDYardUnitsRatingAdapter<TParent, TRatingUnits> GetRatingAdpater(TParent parent, TRatingUnits unitsForRating, CYDYardUnitState yardUnit)
		{
			return new CYDYardUnitsRatingAdapter<TParent, TRatingUnits>(parent, unitsForRating, yardUnit);
		}
	}
}
