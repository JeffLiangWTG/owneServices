using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGenericJobCostPlugInBase
	{
		BusinessObjectFactory Factory { get; }
	}
}
