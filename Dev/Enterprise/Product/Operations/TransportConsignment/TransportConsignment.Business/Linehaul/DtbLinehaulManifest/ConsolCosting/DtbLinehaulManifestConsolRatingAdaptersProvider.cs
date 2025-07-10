using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestConsolRatingAdaptersProvider : RatingOrCostingAdaptersProvider<DtbLinehaulManifest>
	{
		public DtbLinehaulManifestConsolRatingAdaptersProvider(DtbLinehaulManifest manifest)
			: base(manifest, null)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		protected override List<IAutoRating> GetAdapters(DtbLinehaulManifest parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = base.GetAdapters(parent, uiInteractor, options);
			result.Add(new DtbLinehaulManifestRatingAdapter(parent));
			return result;
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(DtbLinehaulManifest manifest)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(manifest));
			var consignments = manifest.Consignments.Cast<IJobInvoicingPlugIn>().ToArray();
			result.AddRange(consignments);

			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}
	}
}
