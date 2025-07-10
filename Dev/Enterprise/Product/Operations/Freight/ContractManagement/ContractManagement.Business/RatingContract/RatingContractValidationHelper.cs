using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class RatingContractValidationHelper : IRatingContractValidationHelper
	{
		public bool DoesClientContractNumberExist(BusinessObjectFactory factory, string clientContractNumber)
		{
			var query = new ZQuery(RatingContractSchema.RCT_ContractNumber, clientContractNumber);
			query.AddToFilter(RatingContractSchema.RCT_ContractType, Core.Constants.RatingContractTypes.Client);
			query.AddToFilter(RatingContractSchema.RCT_IsActive, true);
			return factory.Exists(typeof(RatingContract), query);
		}
	}
}
