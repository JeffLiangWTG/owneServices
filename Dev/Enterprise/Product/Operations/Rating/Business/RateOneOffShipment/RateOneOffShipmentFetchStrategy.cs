using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateOneOffShipmentFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public RateOneOffShipmentFetchStrategy(RateOneOffShipment rateOneOffShipment)
			: base(rateOneOffShipment)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(RateOneOffContainersSchema.TC_TT, BusinessObject.PK);
			Factory.AddFetchHint(RateOneOffPackLineSchema.TPL_TT_RateOneOffShipment, BusinessObject.PK);

			var rateOneOffShipment = (RateOneOffShipment)BusinessObject;
			var mode = rateOneOffShipment.Mode;
			if (mode == Core.Constants.RateMode.LSE || mode == Core.Constants.RateMode.LCL || mode == Core.Constants.RateMode.FCL)
			{
				Factory.AddFetchHint(typeof(RatingHeader), rateOneOffShipment.TT_TH);
			}
		}
	}
}

