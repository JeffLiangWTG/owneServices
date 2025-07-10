using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGenShapeGeography
	{
		ZGuid PK { get; }
		ZString SHG_Name { get; set; }
		ZString SHG_Description { get; set; }
		ZBool SHG_IsActive { get; set; }
		ZBool SHG_IsSystem { get; set; }
		ZGeography SHG_Shape { get; set; }
		ZString SHG_Type { get; set; }
	}
}
