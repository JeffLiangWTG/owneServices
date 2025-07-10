using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageJobDatesProvider : JobDatesProvider<CartageRatingAdapter>
	{
		public CartageJobDatesProvider(CartageRatingAdapter cartageRatingAdapter)
			: base(cartageRatingAdapter)
		{
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.LastCartageLeg != null
				? Parent.LastCartageLeg.JU_DeliverTimeOut.IsValid
					? Parent.LastCartageLeg.JU_DeliverTimeOut
					: Parent.LastCartageLeg.QuickEstimatedDeliveryTime
				: ZDateTime.Empty;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.FirstCartageLeg != null
				? Parent.FirstCartageLeg.JU_PickupTimeIn.IsValid
					? Parent.FirstCartageLeg.JU_PickupTimeIn
					: Parent.FirstCartageLeg.QuickPlannedPickupTime
				: ZDateTime.Empty;
		}

		protected override ZDateTime GetJobOpenDateCore()
		{
			return Parent.InvoicingSupporter?.Job?.JH_A_JOP ?? ZDateTime.Empty;
		}
	}
}
