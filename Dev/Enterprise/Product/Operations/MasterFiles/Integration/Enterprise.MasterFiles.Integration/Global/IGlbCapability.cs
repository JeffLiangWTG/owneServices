using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCapability
	{
		ZString G4_Code { get; set; }
		ZString G4_Description { get; set; }
		ZBool G4_IsActive { get; set; }
	}
}
