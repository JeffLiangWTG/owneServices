using System.Collections.Generic;
using System.Linq;
using Enterprise.eManifest.Integration;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eManifest.Business.Rating
{
	public class HLSShipmentRatingAdaptersProvider : RatingAdaptersProvider<CommonShipment>, IHLSShipmentRatingAdaptersProvider
	{
		public HLSShipmentRatingAdaptersProvider(CommonShipment parent) : base(parent)
		{
		}

		protected override List<IAutoRating> GetAdapters(CommonShipment shipment, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = new List<IAutoRating> { new HLSShipmentRatingAdapter(shipment) };

			SupplierBookingLinesManager manager = null;
			try
			{
				manager = new SupplierBookingLinesManager(shipment);
				var lineAdapters = manager.Lines.Cast<SupplierBookingLine>().Select(x => new HVLVLineRatingAdapter(x)).ToList();
				result.AddRange(lineAdapters);
			}
			finally
			{
				manager.Dispose();
			}

			return result;
		}

		public override AdaptersProviderOptions AdaptersProviderOptions
		{
			get { return AdaptersProviderOptions.HLSShipment; }
		}
	}
}
