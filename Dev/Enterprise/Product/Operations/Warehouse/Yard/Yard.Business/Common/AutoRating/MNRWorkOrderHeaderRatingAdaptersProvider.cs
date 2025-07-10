using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderRatingAdaptersProvider<T> : RatingAdaptersProvider<T>
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		IRatingSupporter,
		IJobInvoicingPlugIn, IYardWorkOrderForRating, ICYDJobInvoicingSupporter
	{
		public MNRWorkOrderHeaderRatingAdaptersProvider(T workOrder)
			: base(workOrder)
		{
		}

		protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor interactor, AutoRateOptions options)
		{
			var filter = new ZQuery(MNRWorkOrderLineSchema.MWL_MWO_MNRWorkOrderHeader, parent.PK);
			var jobLines = parent.Factory.Load<MNRWorkOrderLine>(filter);

			var adapters = new List<IAutoRating>();
			adapters.AddRange(jobLines.Select(l => new MNRWorkOrderHeaderRatingAdapter<T>(parent, l)));

			return adapters;
		}
	}
}
