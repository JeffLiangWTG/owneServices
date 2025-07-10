using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	[TestedType(typeof(ExWarehouseApplicator))]
	sealed class ExWarehouseApplicatorTest : LockedOperationalActionMethodApplicatorTest<ExWarehouseApplicator>
	{
		protected override BusinessObject GetNewBusinessObject() => new ExWarehouseApplicator(Factory, () => false, (_) => { });

		protected override OperationalActionMethodApplicator GetOtherApplicator() => new ExportBelnApplicator(Factory, () => false);

		public void TestProcessAllOrdersWhenGridIsEmpty()
		{
			CreateOperatorTransaction(GlbCompany.CurrentCompany, 1);
			CreateOperatorTransaction(GlbCompany.CurrentCompany, 2);
			Factory.Save();

			var applicator = new ExWarehouseApplicatorForTesting(Factory);
			var selections = applicator.GetTransactionSelectionsExposed(Array.Empty<CusWHSOperatorTransaction>());
			AssertEquals(0, selections.Count);
		}

		public void TestGetTransactionSelections()
		{
			var transaction1 = CreateOperatorTransaction(GlbCompany.CurrentCompany, 1);
			var transaction2 = CreateOperatorTransaction(GlbCompany.CurrentCompany, 2);
			Factory.Save();

			var applicator = new ExWarehouseApplicatorForTesting(Factory);
			var selections = applicator.GetTransactionSelectionsExposed(new CusWHSOperatorTransaction[] { transaction1, transaction2 });
			AssertEquals(1, selections.Count);
			AssertEquals(transaction1.Batch.WOB_OA_Warehouse, selections[0].WarehouseAddress);
			AssertEquals(transaction1.WOT_OH_ProductOwner, selections[0].ProductOwner);
		}

		CusWHSOperatorTransaction CreateOperatorTransaction(GlbCompany company, int quantity)
		{
			var result = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			result.Batch.WOB_GC_Company = company.PK;
			result.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			result.WOT_ExportType = ZString.Empty;
			result.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			result.WOT_Quantity = quantity;
			return result;
		}

		class ExWarehouseApplicatorForTesting : ExWarehouseApplicator
		{
			public ExWarehouseApplicatorForTesting(BusinessObjectFactory factory)
				: base(factory, () => true, (_) => { })
			{
			}

			public ExWarehouseApplicatorForTesting(BusinessObjectFactory factory, Func<bool> showConfirmationDialogMethod, Action<ZString> showProcessLockedInfo)
				: base(factory, showConfirmationDialogMethod, showProcessLockedInfo)
			{
			}

			public OperatorTransactionSelectionCollection GetTransactionSelectionsExposed(CusWHSOperatorTransaction[] targets) => GetTransactionSelections(targets);
		}
	}
}
