using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardRatingAdaptersProvider<T> : RatingAdaptersProvider<T>
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		IRatingSupporter,
		IJobInvoicingPlugIn,
		ICYDYardUnitsForRating
	{
		public CYDYardRatingAdaptersProvider(T transportationUnit)
			: base(transportationUnit)
		{
		}

		protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor interactor, AutoRateOptions options)
		{
			var adapters = new List<IAutoRating>();
			var yardUnits = parent.YardUnits;
			foreach (var yardUnit in yardUnits)
			{
				adapters.Add(new CYDYardRatingAdapter<T>(parent, yardUnit));
			}

			return adapters;
		}
	}
}
