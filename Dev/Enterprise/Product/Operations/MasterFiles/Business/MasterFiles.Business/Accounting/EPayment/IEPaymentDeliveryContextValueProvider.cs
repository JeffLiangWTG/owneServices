using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IEPaymentDeliveryContextValueProvider
	{
		ZString Purpose { get; }
		EntityInfo EntityInfo { get; }
	}
}
