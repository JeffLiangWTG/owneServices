using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGenCustomColumnDefinition
	{
		ZGuid PK { get; }
		ZGuid XC_XR { get; }
		ZString XC_Type { get; }
		ZString XC_Name { get; }
		ZInt XC_DisplaySequence { get; }
		ZGuid XC_ParentID { get; }
		ZString XC_ParentTableCode { get; }
	}
}
