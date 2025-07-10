using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	partial class VASOrderCreateInitialTransferManager
	{
		public VASOrderCreateInitialTransferManager(ILogger logger)
		{
			Logger = logger;
		}

		readonly ILogger Logger;

		#region CreateTransfersForVASOrder

		public void CreateTransfersForVASOrdersWithNoInitialTransfer()
		{
			var whsVASOrderPKs = GetWhsVASOrderPKs();
			if (whsVASOrderPKs.Any())
			{
				var factory = new BusinessObjectFactory();
				var vasOrders = factory.Load<WhsVASOrder>(new ZQuery(WhsVASOrderSchema.PK, whsVASOrderPKs));
				var vasOrdersGroupedByWarehouse = vasOrders.GroupBy(vasOrder => vasOrder.WarehousePK);

				foreach (var vasOrderGroup in vasOrdersGroupedByWarehouse)
				{
					using (WarehouseUserContextHelper.SetUserContextForWarehouse(vasOrderGroup.First().Warehouse))
					{
						CreateTransfers(vasOrderGroup.ToArray(), factory);
					}
				}
			}
			else
			{
				Logger.Information(Res.GetString("4d261a80-596f-42f0-bcf3-badca804a6ed", "Did not find any VAS Order with no initial transfer."));
			}
		}

		#endregion

		#region GetWhsVASOrders

		static IEnumerable<ZGuid> GetWhsVASOrderPKs()
		{
			var rawSqlQuery =
$@" SELECT
	WVO_PK
FROM
	dbo.WhsVASOrder
WHERE
	WVO_WD_TransferIntoServiceArea IS NULL
	AND WVO_CancelledTimeUtc IS NULL
ORDER BY
	WVO_CustomerReferenceNo";

			var vasOrderPKCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			vasOrderPKCollection.Load(rawSqlQuery, new ZSqlParameterCollection());

			return vasOrderPKCollection.Select(row => (ZGuid)row[WhsVASOrderSchema.PK]).ToArray();
		}

		#endregion

		#region CreateTransfers

		void CreateTransfers(IEnumerable<WhsVASOrder> vasOrders, BusinessObjectFactory factory)
		{
			foreach (var vasOrder in vasOrders)
			{
				var buffer = new NotificationBuffer();
				CreateInitialTransfer(vasOrder, buffer, factory);

				if (buffer.HasErrors)
				{
					LogErrors(vasOrder, buffer);
				}
				else
				{
					LogSucessfullyCreatedTransferInformation(vasOrder);
				}
			}
		}

		#endregion

		#region CreateInitialTransfer

		WhsVASOrder CreateInitialTransfer(WhsVASOrder vasOrder, NotificationBuffer buffer, BusinessObjectFactory factory)
		{
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(buffer);

			CreateInitialTransferForTest(initialTransfer);

			if (initialTransfer != null && !initialTransfer.IsInDatabase)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
				{
					try
					{
						factory.Save();
					}
					catch (ZSaveConcurrencyException)
					{
						AddConcurrencyError(buffer);
					}
					catch (ZConcurrencyCheckFailureException)
					{
						AddConcurrencyError(buffer);
					}
					catch (ZCannotSaveException ex)
					{
						buffer.Notify(new Notification(CargoWise.ComponentModel.NotificationType.Error, ex.Message));
					}
				}, null);
			}

			return vasOrder;
		}

		static void AddConcurrencyError(NotificationBuffer buffer)
		{
			buffer.Notify(new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("eb89ad27-864c-4075-8de0-a7c00d9d53bd", "Another user has modified the VAS Order or created the initial transfer. The transfer cannot be saved because it may conflict with the other user's changes. Please try again.")));
		}

		#endregion

		#region LogErrors

		void LogErrors(WhsVASOrder vasOrder, NotificationBuffer buffer)
		{
			var errorsFromNotification = string.Join(", ", buffer.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(m => m.Message));
			var errorMessage = Res.GetString("52abbbc1-06eb-41dc-be1f-d229b1c80202",
											"Creating a transfer for VAS Order {0} with Client {1} and Warehouse {2} failed. Error below:\r\n{3}", vasOrder.WVO_CustomerReferenceNo, vasOrder.Client.OH_FullName, vasOrder.Warehouse.WW_WarehouseNameMultilingual, errorsFromNotification);

			Logger.Error(errorMessage);
		}

		#endregion

		#region LogSucessfullyCreatedTransferInformation

		void LogSucessfullyCreatedTransferInformation(WhsVASOrder vasOrder)
		{
			var sucessMessage = Res.GetString("e893c08e-96c7-4581-a3aa-182901eb4322", "Initial transfer was created successfully for VAS Order {0} with Client {1} Warehouse {2}.",
											vasOrder.WVO_CustomerReferenceNo, vasOrder.Client.OH_FullName, vasOrder.Warehouse.WW_WarehouseNameMultilingual);

			Logger.Information(sucessMessage);
		}

		#endregion

		partial void CreateInitialTransferForTest(WhsTransfer transfer);
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public partial class VASOrderCreateInitialTransferManager
	{
		partial void CreateInitialTransferForTest(WhsTransfer transfer)
		{
			CreateTransferForTest?.Invoke(transfer);
		}

		public Action<WhsTransfer> CreateTransferForTest;
	}
}

#endif
#endregion
