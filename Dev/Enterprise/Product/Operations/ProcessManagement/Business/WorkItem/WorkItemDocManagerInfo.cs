using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemDocManagerInfo : DocManagerInfo
	{
		public WorkItemDocManagerInfo(WorkItem parent, string docManagerCode = Constants.DocManagerCodes.WorkItem)
			: base(parent, docManagerCode)
		{ }

		protected override BusinessObject[] GetRelatedObjects()
		{
			return Transactions.ToArray();
		}

		AccTransactionHeaderCollection Transactions
		{
			get
			{
				var workItem = (WorkItem)BusinessEntity;
				return new InvoiceLoader(BusinessEntity.Factory).GetInvoicesForUniqueRef(workItem.Number);
			}
		}
	}
}
