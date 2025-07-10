using CargoWise.ComponentModel;

namespace Enterprise.Freight.Forwarding.Business
{
	public class NotImplementedCarrierMessagingValidation : CarrierMessagingValidation
	{
		public NotImplementedCarrierMessagingValidation(ForwardingConsol consol)
			: base(consol)
		{
		}

		protected override void ValidateCore(INotifications notifications)
		{
			notifications.AddError(Res.GetString("073734ad-cb0f-4c5f-89ba-ff3b1e493208", "Carrier messaging is currently available for Road Consolidations only."));
		}
	}
}
