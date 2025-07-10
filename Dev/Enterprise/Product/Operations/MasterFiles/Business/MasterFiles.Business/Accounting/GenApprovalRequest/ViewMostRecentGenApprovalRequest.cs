using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ViewMostRecentGenApprovalRequest : AutoViewMostRecentGenApprovalRequest
	{
		public ViewMostRecentGenApprovalRequest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
