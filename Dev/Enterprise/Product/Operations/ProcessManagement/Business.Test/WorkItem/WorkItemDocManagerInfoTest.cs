using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemDocManagerInfo))]
	public class WorkItemDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestGetRelatedObjects()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();
			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_GC = Env.CurrentCompanyPK;
			transaction.AH_ConsolidatedInvoiceRef = workItem.Number;
			Factory.Save();

			Assert(workItem.DocManagerInfo.RelatedObjects.Contains(transaction));
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WorkItem>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<WorkItem>();
		}
	}
}
