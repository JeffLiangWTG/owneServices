using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Billing.Integration;

namespace Enterprise.Customs.Business.TransactionalBillingReporting
{
	public class ReportDataAndName
	{
		public ReportDataAndName(ZString reportData, ZString fileName, int reportRowCount, List<BillingTransaction> transactionRows)
		{
			TransactionRows = transactionRows;
			ReportRowCount = (transactionRows != null && reportRowCount == 0) ? transactionRows.Count : reportRowCount;
			ReportData = reportData;
			FileName = fileName;
		}
		public int ReportRowCount { get; private set; }
		public ZString ReportData { get; private set; }
		public ZString FileName { get; private set; }
		public List<BillingTransaction> TransactionRows { get; private set; }
	}
}
