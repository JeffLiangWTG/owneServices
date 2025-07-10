using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetConsolRatingAdaptersProvider : RatingOrCostingAdaptersProvider<DtbConsignmentRunSheet>
	{
		public DtbConsignmentRunSheetConsolRatingAdaptersProvider(DtbConsignmentRunSheet runSheet)
			: base(runSheet, null)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		protected override List<IAutoRating> GetAdapters(DtbConsignmentRunSheet parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = base.GetAdapters(parent, uiInteractor, options);
			result.Add(new DtbConsignmentRunSheetRatingAdapter(parent));
			return result;
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(DtbConsignmentRunSheet runSheet)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(runSheet));

			if (runSheet.HasNewConsignments)
			{
				var consignments = runSheet.RunSheetInstructions.SelectMany(i => i.Actions).Select(a => a.Consignment).Distinct();
				result.AddRange(consignments);
			}
			else
			{
				var bookingConsignments = runSheet.RunSheetInstructions.SelectMany(i => i.Confirmations).Select(c => c.Instruction.Booking).Distinct();
				result.AddRange(bookingConsignments);
			}

			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}
	}
}
