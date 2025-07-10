using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgCompanyData : IBusiness
	{
		ZGuid PK { get; }

		ZBool OB_IMUsedBondedWhs { get; set; }
	}
}
