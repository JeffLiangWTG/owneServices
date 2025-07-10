using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDevice
	{
		ZGuid PK { get; }
		ZString V3_Model { get; set; }
		ZDateTime V3_SystemCreateTimeUtc { get; }
		ZString V3_SystemCreateUser { get; set; }
		ZBool V3_IsActive { get; }
		ZString V3_HumanReadableIdentifier { get; set; }
		ZBlob V3_MobileServicesIdentifier { get; set; }
	}
}
