using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.OceanCarrier.Business
{
	sealed class CarrierShipmentRatingAdaptersProvider : RatingAdaptersProvider<CarrierShipmentHeader>
	{
		public CarrierShipmentRatingAdaptersProvider(CarrierShipmentHeader parent)
			: base(parent)
		{
		}

		protected override List<IAutoRating> GetAdapters(CarrierShipmentHeader parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating>
			{
				new CarrierShipmentRatingAdapter(parent)
			};
		}
	}
}
