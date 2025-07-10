using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	sealed class BaseExportApplicatorTest : TestCaseWithFactory
	{
		public void TestQuerySql()
		{
			var fields = new List<string>
			{
				CusWHSOperatorTransaction.Schema.WOT_TransactionType,
				CusWHSOperatorTransaction.Schema.WOT_ExportType,
				CusWHSOperatorTransaction.Schema.WOT_TransactionDate,
				CusWHSOperatorTransaction.Schema.WOT_OwnerReference,
				CusWHSOperatorTransaction.Schema.WOT_WOB_CusWHSTransactionBatch,
				CusWHSOperatorTransaction.Schema.WOT_OH_ProductOwner,
				CusWHSOperatorTransactionBatch.Schema.WOB_OA_Warehouse,
				CusWHSOperatorTransactionBatch.Schema.PK,
			};

			var querySql = BaseExportApplicator.QuerySql;

			foreach (var fieldName in fields)
			{
				AssertContains(fieldName, querySql);
			}
		}

		public void TestSupportRunningOnAllMatchedRecords()
		{
			var applicator = new ExportNonBelnApplicator(Factory, () => false);
			AssertEquals("SupportRunningOnAllMatchedRecords should be false", expected: false, applicator.SupportRunningOnAllMatchedRecords);
		}

		public void TestSelectAll()
		{
			var instance = new BaseExportApplicatorForTesting(Factory);
			AssertEquals("SelectAll for empty collection", expected: false, instance.SelectAll);

			instance.Records.AddNew();
			instance.Records.AddNew();

			instance.SelectAll = true;
			AssertContainsExactElementsInExactOrder("After SelectAll set to true, all records should be true", new[] { true, true }, instance.Records.Select(x => (bool)x.Select));

			instance.Records[0].Select = false;
			AssertEquals("After selecting one record to not-selected, SelectAll should be false", expected: false, instance.SelectAll);

			instance.SelectAll = false;
			AssertContainsExactElementsInExactOrder("After selectAll set to false, all records should be false", new[] { false, false }, instance.Records.Select(x => (bool)x.Select));
		}

		class BaseExportApplicatorForTesting : BaseExportApplicator
		{
			public BaseExportApplicatorForTesting(BusinessObjectFactory factory) : base("BaseExportApplicatorForTesting", factory, ZString.Empty)
			{
			}

			protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
