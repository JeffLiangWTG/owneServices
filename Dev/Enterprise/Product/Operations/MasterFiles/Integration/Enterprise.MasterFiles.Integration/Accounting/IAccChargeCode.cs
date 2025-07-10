using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAccChargeCode
	{
		ZGuid PK { get; }
		ZString AC_Code { get; set; }
	}
}
