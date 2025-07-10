using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class ShipmentRatingAdaptersProvider<T> : RatingAdaptersProvider<T>
		where T : CommonShipment
	{
		public ShipmentRatingAdaptersProvider(T parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(T parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = new List<IAutoRating> { parent.RatingAdapter };
			return result;
		}
	}
}
