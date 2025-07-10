using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickableDocket : IWhsDocket
	{
		ZString TransportZoneName { get; }
		ZByte WD_PickPriority { get; }
		ZString SalesChannelCode { get; }
	}
}
