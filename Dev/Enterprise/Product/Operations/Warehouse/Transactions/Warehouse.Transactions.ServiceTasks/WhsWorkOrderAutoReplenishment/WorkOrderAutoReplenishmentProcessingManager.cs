using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public partial class WorkOrderAutoReplenishmentProcessingManager
	{
		public WorkOrderAutoReplenishmentProcessingManager(BusinessObjectFactory factory, ILogger logger)
		{
			Logger = logger;
			Factory = factory;
		}

		readonly ILogger Logger;
		readonly BusinessObjectFactory Factory;

		#region CreateWorkOrderForReplenishment

		public void CreateWorkOrderForReplenishment()
		{
			ReplenishmentWorkOrderInfo[] replenishmentInfos = null;

			try
			{
				replenishmentInfos = GetReplenishmentInfos();
			}
			catch (SqlException ex)
			{
				Logger.Error(ex.Message);
				Logger.Information(AutoCreatingFailedMessage);
			}

			if (replenishmentInfos != null)
			{
				CreateWorkOrders(replenishmentInfos);
			}
		}

		static string AutoCreatingFailedMessage
		{
			get { return Res.GetString("7e7a1163-34bb-47fe-bd93-1226c3952db6", "Auto-creation of Replenishment Work Orders failed."); }
		}

		#region GetReplenishmentInfos

		ReplenishmentWorkOrderInfo[] GetReplenishmentInfos()
		{
			var replenishmentInfos = new List<ReplenishmentWorkOrderInfo>();
			var summaryList = LoadStockAndWorkOrderInfos();
			foreach (DynamicBusinessObject replenishmentInfo in summaryList)
			{
				var replenishmentWorkOrderInfo = new ReplenishmentWorkOrderInfo();
				replenishmentWorkOrderInfo.ClientPK = (ZGuid)replenishmentInfo["Client"];
				replenishmentWorkOrderInfo.WarehousePK = (ZGuid)replenishmentInfo["Warehouse"];
				replenishmentWorkOrderInfo.ProductPK = (ZGuid)replenishmentInfo["Product"];
				replenishmentWorkOrderInfo.Quantity = (ZDecimal)replenishmentInfo["ReplenishmentQuanity"];

				replenishmentInfos.Add(replenishmentWorkOrderInfo);
			}

			return replenishmentInfos.ToArray();
		}

		DynamicBusinessObjectCollection LoadStockAndWorkOrderInfos()
		{
			#region SQL

			string sql = @"
;		
WITH 
		BOMPart AS (
			SELECT 
				OE_OP_MainProduct AS BOM_PK,
				OP_PartNum AS ProductCode
			FROM 
				dbo.OrgSupplierPart  
				JOIN dbo.OrgPartBOM ON OE_OP_MainProduct = OP_PK
			WHERE OP_IsActive = 1 AND OP_KitIsAutoReplenished = 1
			GROUP BY OE_OP_MainProduct, OP_PartNum
		),
		BOMParam AS (
			SELECT 
				W3_OH AS Client, 
				W3_WW AS Warehouse, 
				W3_OP AS Product,
				ProductCode,			
				W3_ReplenishmentMinimum AS ReplenishmentMinimum, 
				W3_EconomicQuantity AS EconomicQuantity, 
				W3_ReplenishmentMultiple AS ReplenishmentMultiple
			FROM 
				dbo.WhsProductParamsByWhsAndClient  
				JOIN BOMPart ON BOM_PK = W3_OP
			WHERE W3_EconomicQuantity > 0 and W3_ReplenishmentMultiple > 0
		),
		BOMInventroy AS (
			SELECT
				WD_OH_Client AS InventoryClient,
				WD_WW_Whs AS InventoryWhs,
				WE_OP AS InventoryProduct,
				SUM(WE_StockOnHand) AS CurrentInventory
			FROM
				dbo.WhsDocketLine  
				JOIN dbo.WhsDocket ON WE_WD = WD_PK
				JOIN BOMParam ON Product = WE_OP AND Client = WD_OH_Client AND Warehouse = WD_WW_Whs
			WHERE
				WE_StockOnHand > 0
			GROUP BY
				WD_OH_Client,
				WD_WW_Whs,
				WE_OP
		),
		UnfinalisedWorkOrder AS (
			SELECT 			
				WD_OH_Client AS UnfinalisedClient,
				WD_WW_Whs AS UnfinalisedWhs,
				WE_OP AS UnfinalisedProduct,
				SUM(WE_TransactionQuantity) AS UnfinalisedUnits
			FROM dbo.WhsDocketLine  
			JOIN dbo.WhsDocket ON WE_WD = WD_PK
			JOIN BOMParam ON Product = WE_OP AND Client = WD_OH_Client AND Warehouse = WD_WW_Whs
			WHERE WD_DocketType = 'WOR' AND WD_DocketSubType = 'ASS' AND WD_DocketStatus <> 'FIN'
			GROUP BY
				WD_OH_Client,
				WD_WW_Whs,
				WE_OP
		),
		CalculateReplenishment AS (
			SELECT 
				Client, OH_CODE as ClientCode, WW_WarehouseCode as WarehouseCode, ProductCode, Warehouse, Product,
				ceiling((EconomicQuantity - ISNULL(CurrentInventory, 0) - ISNULL(UnfinalisedUnits, 0)) / ReplenishmentMultiple) * ReplenishmentMultiple AS ReplenishmentQuanity
			FROM BOMParam 
			JOIN dbo.OrgHeader ON Client = OH_PK
			JOIN dbo.WhsWarehouse ON Warehouse = WW_PK
			LEFT JOIN UnfinalisedWorkOrder ON Client = UnfinalisedClient AND Warehouse = UnfinalisedWhs AND Product = UnfinalisedProduct
			LEFT JOIN BOMInventroy ON Client = InventoryClient AND Warehouse = InventoryWhs AND Product = InventoryProduct 
			WHERE ((ISNULL(CurrentInventory, 0) + ISNULL(UnfinalisedUnits, 0)) < ReplenishmentMinimum) OR (ReplenishmentMinimum = 0 AND (ISNULL(CurrentInventory, 0) + ISNULL(UnfinalisedUnits, 0)) = 0)
		)
		SELECT * FROM CalculateReplenishment
		ORDER BY ClientCode, WarehouseCode, ProductCode;
	";

			#endregion

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WorkOrderDocketType", DocketType.Codes.WorkOrder, WhsDocketSchema.WD_DocketType);
			sqlParams.Add("@WorkOrderDocketSubType", WorkOrderType.Codes.Assemble, WhsDocketSchema.WD_DocketSubType);
			sqlParams.Add("@FinalisedDocketStatus", DocketLineStatus.Codes.Finalised, WhsDocketSchema.WD_DocketStatus);

			var summaryList = new DynamicBusinessObjectCollection(Factory);
			summaryList.Load(sql, sqlParams);
			return summaryList;
		}

		#endregion

		#endregion

		#region CreateWorkOrders

		void CreateWorkOrders(IEnumerable<ReplenishmentWorkOrderInfo> replenishmentInfos)
		{
			if (replenishmentInfos.Any())
			{
				var successLogs = new ZStringBuilder();
				var failureLogs = new ZStringBuilder();

				var replenishmentInfosGroupedByWarehouse = replenishmentInfos.GroupBy(info => info.WarehousePK);
				var warehouses = Factory.Load<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, replenishmentInfosGroupedByWarehouse.Select(group => group.Key)));
				foreach (var replenishmentInfosGroup in replenishmentInfosGroupedByWarehouse)
				{
					var referenceWarehouse = warehouses.Single(whs => whs.PK == replenishmentInfosGroup.Key);
					using (WarehouseUserContextHelper.SetUserContextForWarehouse(referenceWarehouse))
					{
						CreateAndSaveWorkOrders(replenishmentInfosGroup.ToArray(), successLogs, failureLogs);
					}
				}

				if (!successLogs.IsEmpty)
				{
					Logger.Information(successLogs.ToStringWithNewLineBetweenAppends());
				}
				if (!failureLogs.IsEmpty)
				{
					Logger.Error(failureLogs.ToStringWithNewLineBetweenAppends());
				}
			}
			else
			{
				Logger.Information(Res.GetString("51738939-12fe-4ab3-8a37-8782e4365edb", "Did not find any Products that needed replenishing."));
			}
		}

		#region CreateWorkOrderWithLines

		void CreateAndSaveWorkOrders(IEnumerable<ReplenishmentWorkOrderInfo> replenishmentInfos, ZStringBuilder successLogs, ZStringBuilder failureLogs)
		{
			foreach (var replenishmentInfo in replenishmentInfos)
			{
				var newFactory = new BusinessObjectFactory();
				var workOrder = CreateWorkOrderWithLines(newFactory, replenishmentInfo);
				workOrder.RunPreSaveValidation();

				#region Test
				AddErrorForTest(workOrder);
				#endregion

				if (workOrder.HasErrors)
				{
					var errorToAppend = Res.GetString("90d3e128-b7de-4b24-ad6a-d75d03adbea5", "Validation errors: {0}.", workOrder.GetErrors().ToUniqueMessageListString());
					LogErrors(workOrder, errorToAppend, failureLogs);
				}
				else
				{
					#region Test
					CreateFactorySavingErrorForTest(workOrder);
					#endregion

					SaveWorkOrdersAndHandleErrors(newFactory, workOrder, successLogs, failureLogs);
				}
			}
		}

		WhsWorkOrder CreateWorkOrderWithLines(BusinessObjectFactory factory, ReplenishmentWorkOrderInfo repenlishmentInfo)
		{
			var workOrder = factory.New<WhsWorkOrder>();
			workOrder.WD_OH_Client = repenlishmentInfo.ClientPK;
			workOrder.WD_WW_Whs = repenlishmentInfo.WarehousePK;

			var newLine = workOrder.Lines.AddNew();
			newLine.WE_OP = repenlishmentInfo.ProductPK;
			newLine.WE_TransactionQuantity = repenlishmentInfo.Quantity;

			return workOrder;
		}

		void SaveWorkOrdersAndHandleErrors(BusinessObjectFactory newFactory, WhsWorkOrder workOrder, ZStringBuilder successLogs, ZStringBuilder failureLogs)
		{
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
			{
				try
				{
					newFactory.Save();
					LogSucessfullyCreatedWorkOrderInformation(successLogs, workOrder);
				}
				catch (ZSaveException ex)
				{
					var errorMessage = ex.Message;

					#region Test
					SetErrorMessageForTest(ref errorMessage);
					#endregion

					LogErrors(workOrder, Res.GetString("bdaa9d7a-e29d-44de-939d-2028150055f3", "Save errors: {0}", errorMessage), failureLogs);
				}
			}, null);
		}

		#endregion

		#region LogSucessfullyCreatedWorkOrderInformation

		static void LogSucessfullyCreatedWorkOrderInformation(ZStringBuilder successLogs, WhsWorkOrder workOrder)
		{
			successLogs.Append(Res.GetString("704add51-3654-4b65-a63c-714337f21746", "Work Order {0}: was created successfully for Client {1}, Warehouse {2}.",
				workOrder.WD_DocketID, workOrder.ClientName, workOrder.Warehouse.WW_WarehouseNameMultilingual));

			var firstLine = workOrder.Lines.Single();
			successLogs.Append(Res.GetString("f9f8f50c-28be-46cc-8417-848026fe4223", "Products and Units For Replenishment: {0} : {1}", firstLine.ProductCode, firstLine.WE_TransactionQuantity));
		}

		#endregion

		partial void AddErrorForTest(WhsWorkOrder workOrder);
		partial void CreateFactorySavingErrorForTest(WhsWorkOrder workOrder);
		partial void SetErrorMessageForTest(ref string errorMessage);

		#endregion

		#region LogErrors

		static void LogErrors(WhsWorkOrder workOrder, string errorToAppend, ZStringBuilder failureLogs)
		{
			var clientName = workOrder.ClientName;
			var warehouseName = workOrder.Warehouse.WW_WarehouseNameMultilingual;
			if (workOrder != null)
			{
				failureLogs.Append(Res.GetString("c54a0b93-7345-4c7e-9cd8-8f1c746ab6d2",
					"Creating a Work Order for Client: {0}, Warehouse: {1} failed.\r\n{2}",
					clientName, warehouseName, errorToAppend));
			}
		}

		#endregion

		#region ReplenishmentWorkOrderInfo

		class ReplenishmentWorkOrderInfo
		{
			public ZGuid WarehousePK { get; set; }
			public ZGuid ClientPK { get; set; }
			public ZGuid ProductPK { get; set; }
			public ZDecimal Quantity { get; set; }
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	partial class WorkOrderAutoReplenishmentProcessingManager
	{
		partial void AddErrorForTest(WhsWorkOrder workOrder)
		{
			if (IsAddErrorForTest)
			{
				workOrder.AddRowError("Test Error1");
			}
		}

		partial void CreateFactorySavingErrorForTest(WhsWorkOrder workOrder)
		{
			if (IsAddFactorySavingErrorForTest)
			{
				workOrder.WD_OH_Client = ZGuid.Empty;
			}
		}

		partial void SetErrorMessageForTest(ref string errorMessage)
		{
			if (IsAddFactorySavingErrorForTest)
			{
				errorMessage = "Save Error";
			}
		}

		public bool IsAddErrorForTest;

		public bool IsAddFactorySavingErrorForTest;
	}
}

#endif
#endregion
