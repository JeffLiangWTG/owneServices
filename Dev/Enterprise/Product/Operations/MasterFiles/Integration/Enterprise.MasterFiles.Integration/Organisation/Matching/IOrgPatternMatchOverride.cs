using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgPatternMatchOverride
	{
		ZGuid PK { get; }
		ZString OO_ForeignCode { get; set; }
		ZString OO_LocalCode { get; set; }
		ZGuid OO_LocalGuid { get; set; }
		ZGuid OO_OH { get; set; }
		ZString OO_Relationship { get; set; }
	}
}
