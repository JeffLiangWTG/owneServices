using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IShipmentConsolAttachRequest
	{
		ZString Errors { get; }
		ZString Warnings { get; }
	}
}
