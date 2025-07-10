using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ContractManagement.Business
{
	public class ViewRatingContractQuantity : AutoViewRatingContractQuantity
	{
		public ViewRatingContractQuantity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
