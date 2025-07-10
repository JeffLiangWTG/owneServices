using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IDeviceAssignmentDivot
	{
		ZGuid PK { get; }
		ZGuid V7_V3_Device { get; set; }
		ZGuid V7_ParentID { get; set; }
		ZString V7_ParentTableCode { get; set; }
		ZDateTime V7_StartTimeUtc { get; set; }
		ZDateTime V7_EndTimeUtc { get; set; }
	}
}
