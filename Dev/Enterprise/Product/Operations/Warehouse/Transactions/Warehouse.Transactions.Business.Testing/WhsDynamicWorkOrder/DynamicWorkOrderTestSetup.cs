using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class DynamicWorkOrderTestSetup
	{
		DynamicWorkOrderTestSetup(WhsTestHelperFunctions helper, OrgHeader client, WhsWarehouse warehouse)
		{
			Helper = helper;

			Juice_MainProduct = helper.CreateProduct("JUICE", client);
			Peel_SecondaryProduct = helper.CreateProduct("PEEL", client);
			Orange_ComponentProduct = helper.CreateProduct("ORANGE", client);
			Water_ComponentProduct = helper.CreateProduct("WATER", client);
			PotassiumBenzoate_ComponentProduct = helper.CreateProduct("POTASSIUMBENZOATE", client);
			Frogurt_InvalidProduct = helper.CreateProduct("FROGURT", client);
			helper.SetProductWeightAndVolume(Juice_MainProduct, 19m, "KG", 2m, "M3");
			helper.SetProductWeightAndVolume(Peel_SecondaryProduct, 8m, "KG", 3m, "M3");
			helper.SetProductWeightAndVolume(Orange_ComponentProduct, 23m, "KG", 0.5, "M3");
			helper.SetProductWeightAndVolume(Water_ComponentProduct, 17m, "KG", 0.7, "M3");
			helper.SetProductWeightAndVolume(PotassiumBenzoate_ComponentProduct, 9m, "KG", 0.9, "M3");

			WorkOrder = helper.CreateWhsDynamicWorkOrder(client, warehouse, "JUICE LOOSENER");
			WorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			MainProductLine_Juice = helper.CreateWhsDynamicWorkOrderLine(WorkOrder, Juice_MainProduct, 1m);
			MainProductLine_Juice.IsMainInwardProcessedItem = true;

			MainComponentLine_Orange = helper.CreateWhsDynamicWorkOrderLine(WorkOrder, Orange_ComponentProduct, 1m);
			MainComponentLine_Orange.WE_WE_ParentDocketLine = MainProductLine_Juice.PK;

			MainComponentLine_Water = helper.CreateWhsDynamicWorkOrderLine(WorkOrder, Water_ComponentProduct, 2m);
			MainComponentLine_Water.WE_WE_ParentDocketLine = MainProductLine_Juice.PK;

			MainComponentLine_PotassiumBenzoate = helper.CreateWhsDynamicWorkOrderLine(WorkOrder, PotassiumBenzoate_ComponentProduct, 10m);
			MainComponentLine_PotassiumBenzoate.WE_WE_ParentDocketLine = MainProductLine_Juice.PK;

			SecondaryProductLine_Peel = helper.CreateWhsDynamicWorkOrderLine(WorkOrder, Peel_SecondaryProduct, 1m);
			SecondaryProductLine_Peel.IsSecondaryInwardProcessedItem = true;

			SecondaryComponentLine_Orange = helper.CreateWhsDynamicWorkOrderLine(WorkOrder, Orange_ComponentProduct, 0.1m);
			SecondaryComponentLine_Orange.WE_WE_ParentDocketLine = SecondaryProductLine_Peel.PK;

			SecondaryComponentLine_PotassiumBenzoate = helper.CreateWhsDynamicWorkOrderLine(WorkOrder, PotassiumBenzoate_ComponentProduct, 1m);
			SecondaryComponentLine_PotassiumBenzoate.WE_WE_ParentDocketLine = SecondaryProductLine_Peel.PK;
		}

		public static DynamicWorkOrderTestSetup CreateDynamicWorkOrderForTest(WhsTestHelperFunctions helper, OrgHeader client, WhsWarehouse warehouse)
		{
			Argument.NotNull(helper, nameof(helper));
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(warehouse, nameof(warehouse));
			return new DynamicWorkOrderTestSetup(helper, client, warehouse);
		}

		public void ReceiveStockForWorkOrder()
		{
			var warehouse = WorkOrder.Warehouse;
			var receive = Helper.CreateWhsReceive(WorkOrder.Client, warehouse, "R1");
			receive.WD_DocketSubType = "CUS";
			receive.WD_IsInwardsProcessingJob = true;
			var line1 = Helper.CreateWhsReceiveLine(receive, Orange_ComponentProduct, 100m, warehouse.DefaultLocationInInwardProcessingArea);
			var line2 = Helper.CreateWhsReceiveLine(receive, Water_ComponentProduct, 100m, warehouse.DefaultLocationInInwardProcessingArea);
			var line3 = Helper.CreateWhsReceiveLine(receive, PotassiumBenzoate_ComponentProduct, 100m, warehouse.DefaultLocationInInwardProcessingArea);
			line1.CustomsData.WB_EntryKey = "ABC";
			line2.CustomsData.WB_EntryKey = "ABC";
			line3.CustomsData.WB_EntryKey = "ABC";
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
		}

		public OrgSupplierPart Juice_MainProduct { get; }
		public OrgSupplierPart Peel_SecondaryProduct { get; }
		public OrgSupplierPart Orange_ComponentProduct { get; }
		public OrgSupplierPart Water_ComponentProduct { get; }
		public OrgSupplierPart PotassiumBenzoate_ComponentProduct { get; }
		public OrgSupplierPart Frogurt_InvalidProduct { get; }

		public WhsDynamicWorkOrder WorkOrder { get; }

		public WhsDynamicWorkOrderLine MainProductLine_Juice { get; }
		public WhsDynamicWorkOrderLine MainComponentLine_Orange { get; }
		public WhsDynamicWorkOrderLine MainComponentLine_Water { get; }
		public WhsDynamicWorkOrderLine MainComponentLine_PotassiumBenzoate { get; }

		public WhsDynamicWorkOrderLine SecondaryProductLine_Peel { get; }
		public WhsDynamicWorkOrderLine SecondaryComponentLine_Orange { get; }
		public WhsDynamicWorkOrderLine SecondaryComponentLine_PotassiumBenzoate { get; }

		WhsTestHelperFunctions Helper { get; }
	}
}
