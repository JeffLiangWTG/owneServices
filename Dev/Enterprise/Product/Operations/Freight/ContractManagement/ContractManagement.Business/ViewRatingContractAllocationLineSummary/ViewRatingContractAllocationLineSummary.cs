using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ContractManagement.Business
{
	public class ViewRatingContractAllocationLineSummary : AutoViewRatingContractAllocationLineSummary
	{
		public ViewRatingContractAllocationLineSummary(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
