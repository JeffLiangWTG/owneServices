using CargoWise.EntityFramework;
namespace Enterprise.Freight.Integration
{
	public interface IRatingContractValidationHelper
	{
		bool DoesClientContractNumberExist(BusinessObjectFactory factory, string clientContractNumber);
	}
}
