using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IJobHeaderParentCore : IJobNumber
	{
		ZGuid PK { get; }
		string TableName { get; }
		bool IsInDatabase { get; }
		BusinessObjectFactory Factory { get; }
	}
}
