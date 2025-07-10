using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccPaymentApprovalItem : AutoAccPaymentApprovalItem
	{
		public AccPaymentApprovalItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
