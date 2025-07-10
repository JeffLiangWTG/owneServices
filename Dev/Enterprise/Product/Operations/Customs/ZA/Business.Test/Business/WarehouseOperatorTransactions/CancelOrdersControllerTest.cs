using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CancelOrdersController))]
	class CancelOrdersControllerTest : TransactionSelectionControllerTest
	{
		readonly string ownerReference = "RefForVAL";

		protected override BusinessObject GetNewBusinessObject() => new CancelOrdersController(Factory, WarehousePK, ProductOwnerPK, ownerReference);

		public override void TestProcessSelectedRecords()
		{
			var controller = new CancelOrdersController(Factory, WarehousePK, ProductOwnerPK, ownerReference);
			AssertEquals("Can proceed with matching owner reference", true, controller.CanProceed);
			AssertEquals(2, controller.Records.Count);
			controller.Records.Sort(OperatorTransactionSelection.Schema.ExportType, ListSortDirection.Descending);
			controller.Records[1].Select = true;
			controller.ProcessSelectedRecords();

			var cancelledTransactions = LoadCancelledTransactions();
			AssertEquals(1, cancelledTransactions.Length);
			AssertEquals(WarehouseOperatorTransactionStatusList.Codes.CAN, cancelledTransactions[0].WOT_Status);
			AssertEquals(ownerReference, cancelledTransactions[0].WOT_OwnerReference);
			AssertEquals(WarehouseOperatorTransactionExportTypeList.Codes.BLN, cancelledTransactions[0].WOT_ExportType);
			AssertEquals(Events.EditedARecord, cancelledTransactions[0].Logs.MostRecentLog.Event);
			AssertEquals(WarehouseOperatorTransactionStatusList.Codes.CAN, cancelledTransactions[0].Logs.MostRecentLog.ReferenceFreeText);
		}

		public void TestNoMatchingTransactions()
		{
			var newWarehouse = Factory.New<IWhsWarehouse>();
			var newProductOwner = Factory.New<OrgHeader>();

			var controller = new CancelOrdersController(Factory, WarehousePK, ProductOwnerPK, "INVALID REF");
			var cancelledTransactions = LoadCancelledTransactions();
			AssertEquals("Cannot proceed with no matching owner reference", false, controller.CanProceed);
			AssertEquals(0, cancelledTransactions.Length);

			controller = new CancelOrdersController(Factory, newWarehouse.PK, ProductOwnerPK, ownerReference);
			cancelledTransactions = LoadCancelledTransactions();
			AssertEquals("Cannot proceed with no matching warehouse", false, controller.CanProceed);
			AssertEquals(0, cancelledTransactions.Length);

			controller = new CancelOrdersController(Factory, WarehousePK, newProductOwner.PK, ownerReference);
			cancelledTransactions = LoadCancelledTransactions();
			AssertEquals("Cannot proceed with no matching product owner", false, controller.CanProceed);
			AssertEquals(0, cancelledTransactions.Length);
		}

		CusWHSOperatorTransaction[] LoadCancelledTransactions()
		{
			var cancelledTransactionQuery = new ZQuery(CusWHSOperatorTransactionSchema.WOT_Status, WarehouseOperatorTransactionStatusList.Codes.CAN);
			return Factory.Load<CusWHSOperatorTransaction>(cancelledTransactionQuery);
		}
	}
}
