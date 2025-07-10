using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ContractManagement.Business
{
	public class ViewRatingContractSummary : AutoViewRatingContractSummary
	{
		public ViewRatingContractSummary(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
