using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration;

public struct ValidationToolRequestHandleParameter
{
	public IBusiness BusinessEntity;
	public ZGuid ValidationRulePK;
	public ZGuid RequestTypePKOnFailure;
}
