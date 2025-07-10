using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefServiceLevel
	{
		ZGuid PK { get; }
		ZString RS_Code { get; }
		ZString RS_Description { get; }
	}
}
