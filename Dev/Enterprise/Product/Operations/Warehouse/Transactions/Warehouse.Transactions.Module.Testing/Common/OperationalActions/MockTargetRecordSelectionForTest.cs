using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class MockTargetRecordSelectionForTest : IFilterRecordsSelection
	{
		public MockTargetRecordSelectionForTest(ISelectedRecords records)
		{
			Records = records;
		}
		readonly ISelectedRecords Records;

		public IList<string> ExclusionReasons => throw new NotImplementedException();
		public int FilterRowCount => Records.PrimaryKeys.Length;
		public ISelectedRecords GetSelectedRecords() => Records;

		public IEnumerable<ISelectedRecords> GetAllFilterRecords(Type bizObjType)
		{
			var batchSize = RawDataRegistry.Instance.OperationalActionsRecordBatchSize.Value;
			int i = 0;
			return Records.PrimaryKeys.GroupBy(s => i++ / batchSize).Select(s => new SelectedRecords(s.ToArray())).ToArray();
		}
	}
}
