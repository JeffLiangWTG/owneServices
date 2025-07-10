using CargoWise.ComponentModel;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface ICarrierMessagingValidation
	{
		void Validate(INotifications notifications);
		void ValidateMasterBillNumber(INotifications notifications);

		bool IsForwardAir();
	}
}
