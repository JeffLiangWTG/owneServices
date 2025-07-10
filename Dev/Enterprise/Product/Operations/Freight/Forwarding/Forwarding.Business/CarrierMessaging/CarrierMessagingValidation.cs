using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CarrierMessagingValidation : ICarrierMessagingValidation
	{
		public CarrierMessagingValidation(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, "consol");
		}

		protected readonly ForwardingConsol consol;

		public void Validate(INotifications notifications)
		{
			Argument.NotNull(notifications, "notifications");

			if (!consol.IsInDatabase || consol.HasChanges)
			{
				notifications.AddError(Res.GetString("5853c110-2aea-49a6-b18a-c920c7feeed4", "{0} must be saved before sending message to a carrier.", consol.HumanReadableName));
			}
			else
			{
				ValidateCore(notifications);
			}
		}

		protected virtual void ValidateCore(INotifications notifications)
		{ }

		public void ValidateMasterBillNumber(INotifications notifications)
		{
			ValidateMasterBillNumberCore(notifications);
		}

		protected virtual void ValidateMasterBillNumberCore(INotifications notifications)
		{ }

		public bool IsForwardAir()
		{
			return consol.IsSuitableForForwardAirMessage();
		}
	}
}
