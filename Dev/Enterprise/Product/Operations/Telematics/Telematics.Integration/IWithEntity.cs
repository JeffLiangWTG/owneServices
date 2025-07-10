using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IWithEntity
	{
		ZString EntityTableCode { get; }
		ZGuid EntityId { get; }
	}
}
