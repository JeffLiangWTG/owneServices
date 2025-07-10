using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IVINOrEmptyContainer
	{
		ZString VIN { get; }
		ZString ForeignPort { get; }
		ZString FactoryCarOrderNumber { get; }
	}
}
