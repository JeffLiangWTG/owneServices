using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateMultiOrderPickActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public GenerateMultiOrderPickActionMethodApplicator(IFactoryService factoryService)
			: base(Res.GetString("f1a48cb2-8e49-4b46-b31d-903765115501", "Generate Multi-Order Pick")) // text used for logging
		{
			FactoryService = Argument.NotNull(factoryService, nameof(factoryService));
		}

		IFactoryService FactoryService { get; }

		#region Generate Picks

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] dockets)
		{
			log.SetSectionProgressMax(dockets.Length + 2);

			var orders = dockets.Cast<WhsOrder>();
			var validOrders = LogInvalidOrdersAndReturnValidOnes(orders, log).ToArray();

			var filteredOrdersByWhsAndPickType = validOrders.GroupBy(o => (o.WD_WW_Whs, WhsPick.GetPickTypeToMatchDockets([o])));

			log.BumpSectionProgress();

			if (filteredOrdersByWhsAndPickType.Any())
			{
				CreatePickPerWarehouseAndPickType(log, filteredOrdersByWhsAndPickType);
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("6c906dd9-a703-49f2-8800-48f43645d482", "No Valid Orders to Create Picks for."));
			}

			log.BumpSectionProgress();
		}

		static IEnumerable<WhsOrder> LogInvalidOrdersAndReturnValidOnes(IEnumerable<WhsOrder> allOrders, IOperationalActionSectionLog log)
		{
			LogUnpickableOrderDueToPickingStatus(allOrders.Where(o => o.WD_DocketStatus == DocketStatus.Codes.Picking), log);
			foreach (var order in allOrders.Where(o => o.WD_DocketStatus != DocketStatus.Codes.Picking))
			{
				if (order.WD_DocketStatus != DocketStatus.Codes.Entered)
				{
					LogUnpickableOrderDueToNonEnteredStatus(order, log);
				}
				else if (order.WD_PickOption != WhsPickOption.Codes.Auto)
				{
					LogUnpickableOrderDueToNonAutoPickOption(order, log);
				}
				else if (order.AllLines.Count == 0)
				{
					LogUnpickableOrderDueToNoLines(order, log);
				}
				else if (order.HasErrors)
				{
					LogUnpickableDueToErrorOnOrder(order, log);
				}
				else
				{
					yield return order;
					continue;
				}

				log.BumpSectionProgress();
			}
		}

		void CreatePickPerWarehouseAndPickType(IOperationalActionSectionLog log, IEnumerable<IGrouping<(ZGuid WarehousePK, string PickType), WhsOrder>> filteredOrdersByWhsAndPickType)
		{
			foreach (var ordersForWarehouseAndPickType in filteredOrdersByWhsAndPickType)
			{
				var factoryForWhs = FactoryService.GetFactory<Func<BusinessObjectFactory>>().Invoke();
				var warehouse = factoryForWhs.Load<WhsWarehouse>(ordersForWarehouseAndPickType.Key.WarehousePK);
				var pickType = ordersForWarehouseAndPickType.Key.PickType;

				var pick = CreatePickAndAttachValidOrders(log, ordersForWarehouseAndPickType, warehouse, pickType);
				if (pick.Orders.Count > 0)
				{
					AddFetchHintsForSave(pick.Orders.Cast<WhsOrder>(), factoryForWhs);

					pick.PickOrdersWithoutAutoAllocate();
					if (pick.IsInDatabase && !pick.WP_IsAwaitingReplenishment)
					{
						LogPickCreated(pick, warehouse, log);
					}
				}
				else
				{
					log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("2a925ffc-de04-4062-a3b9-7012bea55372", "No Pick was created for Warehouse {0}.", warehouse.WW_WarehouseNameMultilingual));
				}
			}
		}

		static WhsPick CreatePickAndAttachValidOrders(IOperationalActionSectionLog log, IEnumerable<WhsOrder> ordersForWarehouse, WhsWarehouse warehouse, string pickType)
		{
			var factory = warehouse.Factory;
			var pick = factory.New<WhsPick>();
			pick.WP_PickType = pickType;

			var ordersInFactoryForWhs = factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, ordersForWarehouse.Select(o => o.PK)));
			log.Notify(OperationalActionLogErrorLevel.Informational,
				Res.GetString("9777e220-4651-45c3-9b85-34fba527271d", "Attempting to Create Pick with {0}x Order(s) for Warehouse {1}", ordersInFactoryForWhs.Length, warehouse.WW_WarehouseNameMultilingual));

			AddFetchHints(ordersInFactoryForWhs, warehouse, factory);

			pick.SaveFailureEvent += (sender, e) => HandleSaveFailure(pick, log, e);
			pick.AutoPickAttempt += (sender, e) => OnAutoPickAttempt(warehouse, log, e);

			var ordersToAttach = CheckPickabilityOfOrders(log, pick, ordersInFactoryForWhs);
			if (ordersToAttach.Count > 0)
			{
				AttachOrdersAndRemoveAllOrdersWithNoStock(log, pick, ordersToAttach);
			}

			return pick;
		}

		static void HandleSaveFailure(WhsPick pick, IOperationalActionSectionLog log, WhsPick.SaveFailureEventArgs args)
		{
			log.NotifyFormat(OperationalActionLogErrorLevel.Error, args.Message);
			UndoPick(pick);
		}

		static void UndoPick(WhsPick pick)
		{
			pick.CancelPick();
			pick.Delete();
		}

		static void OnAutoPickAttempt(WhsWarehouse warehouse, IOperationalActionSectionLog log, WhsPick.AutoPickEventArgs e)
		{
			if (!e.StockWasAllocated && !e.IsPickWaitingReplenishment)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("2a925ffc-de04-4062-a3b9-7012bea55372", "No Pick was created for Warehouse {0}.", warehouse.WW_WarehouseNameMultilingual));
			}
			else if (e.IsPickWaitingReplenishment)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, e.Message);
			}
		}

		static IReadOnlyCollection<WhsOrder> CheckPickabilityOfOrders(IOperationalActionSectionLog log, WhsPick pick, WhsOrder[] ordersInFactoryForWhs)
		{
			var ordersToAttach = new List<WhsOrder>(ordersInFactoryForWhs.Length);

			foreach (var order in ordersInFactoryForWhs)
			{
				var pickability = order.GetPickability(pick);
				if (pickability.IsDocketPickable)
				{
					ordersToAttach.Add(order);
				}
				else
				{
					LogUnpickableOrderDueToPickability(order, pickability, log);
					log.BumpSectionProgress();
				}
			}

			return ordersToAttach;
		}

		static void AttachOrdersAndRemoveAllOrdersWithNoStock(IOperationalActionSectionLog log, WhsPick pick, IReadOnlyCollection<WhsOrder> ordersToAttach)
		{
			pick.AddOrders(ordersToAttach);

			var notifications = new NotificationBuffer();
			pick.AutoAllocateItems(notifications);

			if (notifications.Events.Length > 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("1b2e1791-379c-4f70-8da5-ab84be47ac1f", "Issue occurred during allocation: {0}", notifications.AsString));
			}

			var productsAwaitingReplishment = new Dictionary<(ZGuid Client, ZGuid Product), bool>();
			var ordersToRemove = new List<WhsOrder>(ordersToAttach.Count);

			foreach (var order in ordersToAttach)
			{
				if (HasNoStockAndIsNotAwaitingReplenishment(productsAwaitingReplishment, order, pick))
				{
					LogUnpickableOrderDueToStockAllocation(order, log);
					ordersToRemove.Add(order);
				}

				log.BumpSectionProgress();
			}

			pick.RemoveOrders(ordersToRemove);
		}

		static bool HasNoStockAndIsNotAwaitingReplenishment(Dictionary<(ZGuid Client, ZGuid Product), bool> productsAwaitingReplenishment, WhsOrder order, WhsPick pick)
		{
			foreach (WhsOrderLine line in order.GetLinesToPick())
			{
				if (line.PickLineQuantity > 0)
				{
					return false;
				}
				else
				{
					var productClient = (order.WD_OH_Client, line.WE_OP);
					if (!productsAwaitingReplenishment.TryGetValue(productClient, out var isAwaitingReplenishment))
					{
						productsAwaitingReplenishment[productClient] = isAwaitingReplenishment = pick.IsLineInventoryAwaitingReplenishment(line);
					}

					if (isAwaitingReplenishment)
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region Log Functions

		static void LogUnpickableOrderDueToPickingStatus(IEnumerable<WhsOrder> orders, IOperationalActionSectionLog log)
		{
			if (orders.Any())
			{
				var query = new ZQuery(WhsOrderStatusViewSchema.PK, orders.Select(o => o.PK).ToArray());
				var whsOrderStatusViews = orders.First().Factory.Load<WhsOrderStatusView>(query);

				foreach (var dBO in whsOrderStatusViews)
				{
					var order = orders.Single(o => o.PK == dBO.PK);
					var orderStatusCode = dBO.WOS_OrderStatus;
					var message = Res.GetString("c660eadb-2428-4d52-a4dd-b8bb34d1a63a",
						"{0} {1} is {2} ({3}).",
						order.Description,
						"{0}",
						WhsOrderHelper.OrderStatuses.GetDescriptionFromCode(orderStatusCode),
						orderStatusCode);
					LogCore(order, log, message);
					log.BumpSectionProgress();
				}
			}
		}

		static void LogUnpickableOrderDueToNonEnteredStatus(WhsOrder order, IOperationalActionSectionLog log)
		{
			var message = Res.GetString("c660eadb-2428-4d52-a4dd-b8bb34d1a63a",
				"{0} {1} is {2} ({3}).",
				order.Description,
				"{0}",
				order.WD_DocketStatusDescription,
				order.WD_DocketStatus);
			LogCore(order, log, message);
		}

		static void LogUnpickableOrderDueToNonAutoPickOption(WhsOrder order, IOperationalActionSectionLog log)
		{
			var message = Res.GetString("81e3065d-bb52-4f09-baac-ed9da62b97c7",
				"{0} {1} does not have Pick Option AUT.",
				order.Description, "{0}");
			LogCore(order, log, message);
		}

		static void LogUnpickableDueToErrorOnOrder(WhsOrder order, IOperationalActionSectionLog log)
		{
			var message = Res.GetString("5d97eae5-b4a8-4dbe-b173-a7472b568981",
				"{0} {1} has errors. Resolve these on the order before attempting to pick.",
				order.Description, "{0}");
			LogCore(order, log, message);
		}

		static void LogUnpickableOrderDueToNoLines(WhsOrder order, IOperationalActionSectionLog log)
		{
			var message = Res.GetString("6fc3675c-9070-4a3b-bc03-5113799e288d",
				"{0} {1} has no order lines.",
				order.Description, "{0}");
			LogCore(order, log, message);
		}

		static void LogUnpickableOrderDueToPickability(WhsOrder order, WhsPick.DocketPickabilityEventArgs pickability, IOperationalActionSectionLog log)
		{
			var message = Res.GetString("8607e640-f53b-4438-a970-92bd1de7ee2d",
				"{0} {1} could not be attached to Pick:\r\n{2}",
				order.Description,
				"{0}",
				pickability.Message);
			LogCore(order, log, message, OperationalActionLogErrorLevel.Warning);
		}

		static void LogUnpickableOrderDueToStockAllocation(WhsOrder order, IOperationalActionSectionLog log)
		{
			var message = Res.GetString("29269de8-bc7f-44ed-b500-297952876714",
				"{0} {1} could not be allocated stock.",
				order.Description,
				"{0}");
			LogCore(order, log, message, OperationalActionLogErrorLevel.Warning);
		}

		static void LogPickCreated(WhsPick pick, WhsWarehouse warehouse, IOperationalActionSectionLog log)
		{
			var message = Res.GetString("fd5db26d-3b44-4232-884c-3d1585f24cac",
				"Created pick {0} with {1}x Order(s) for Warehouse {2}.");
			var pickLink = GetPickNoLink(pick);
			log.NotifyFormat(OperationalActionLogErrorLevel.Informational, message, pickLink, pick.Orders.Count, warehouse.WW_WarehouseNameMultilingual);
		}

		static void LogCore(WhsOrder docket, IOperationalActionSectionLog log, string message, OperationalActionLogErrorLevel errorLevel = OperationalActionLogErrorLevel.Error)
		{
			var docketLink = GetDocketIdLink(docket);
			log.NotifyFormat(errorLevel, message, docketLink);
		}

		#endregion

		#region Fetch Hints

		static void AddFetchHints(IEnumerable<WhsOrder> orders, WhsWarehouse warehouse, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(OrgAddressSchema.Constants.TableName, warehouse.WW_OA_WarehouseAddress);

			foreach (var order in orders)
			{
				AddOrderFetchHints(order, factory);
			}

			JobDocAddress[] consigneeDocAddresses;

			// Loading Job Doc Addresses tries to load Validation object which for Orders will Load Warehouse Address for Country Code
			// This screws up the Fetch Hints, so prevent this occuring by temporarily suspending Validation.
			factory.SuspendValidation();
			try
			{
				consigneeDocAddresses = orders.Select(o => o.LoadJobDocAddressQuickly(o.ConsigneeDocAddressRequirement.DefaultDocAddressType)).WhereNotNull().ToArray();
			}
			finally
			{
				factory.ResumeValidation();
			}

			foreach (var consigneeDocAddress in consigneeDocAddresses)
			{
				AddJobDocAddressFetchHints(consigneeDocAddress, factory);
			}

			foreach (var address in consigneeDocAddresses.Where(d => d.HasRealAddress).Select(d => d.Address))
			{
				AddOrgAddressFetchHints(factory, address);
			}

			foreach (var wfItem in orders.SelectMany(o => o.WorkflowItems))
			{
				AddProcessTasksFetchHints(wfItem, factory);
			}

			foreach (var orderLine in orders.SelectMany(o => o.GetLinesToPick()))
			{
				AddOrderLineFetchHints(orderLine, factory);
			}
		}

		static void AddFetchHintsForSave(IEnumerable<WhsOrder> orders, BusinessObjectFactory factory)
		{
			foreach (var notification in orders.SelectMany(o => o.WorkflowItems.Cast<ProcessTask>().SelectMany(p => p.ProcessTaskNotifications)))
			{
				factory.AddFetchHint(ProcessTaskNotificationSchema.PK, notification.PQ_SourceTemplateNotification);
				factory.AddFetchHint(ProcessTasksSchema.PK, notification.PQ_P9);
			}
		}

		static void AddOrderFetchHints(WhsOrder order, BusinessObjectFactory factory)
		{
			var jobHeaderQuery = new ZQuery(JobHeaderSchema.JH_ParentID, order.PK);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			factory.AddFetchHint(JobHeaderSchema.Instance, jobHeaderQuery);
			factory.AddFetchHint(WhsDocketLineSchema.WE_WD, order.PK);
			factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, order.PK);
			factory.AddFetchHint(WhsDocketSchema.PK, order.PK);
			factory.AddFetchHint(StmALogSchema.SL_Parent, order.PK);
			factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
			factory.AddFetchHint(OrgHeaderSchema.Constants.TableName, order.WD_OH_Client);
		}

		static void AddJobDocAddressFetchHints(JobDocAddress consigneeDocAddress, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(OrgAddressSchema.Constants.TableName, consigneeDocAddress.E2_OA_Address);
			factory.AddFetchHint(OrgAddressCapabilitySchema.PZ_OA, consigneeDocAddress.E2_OA_Address);
		}

		static void AddOrgAddressFetchHints(BusinessObjectFactory factory, OrgAddress address)
		{
			factory.AddFetchHint(OrgHeaderSchema.Constants.TableName, address.OA_OH);
			factory.AddFetchHint(OrgAddressSchema.OA_OH, address.OA_OH);
			factory.AddFetchHint(OrgContactSchema.OC_OH, address.OA_OH);
			factory.AddFetchHint(OrgCusCodeSchema.OK_OH, address.OA_OH);
		}

		static void AddProcessTasksFetchHints(BusinessObject processTask, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, processTask.PK);
			factory.AddFetchHint(StmALogSchema.SL_Parent, processTask.PK);
		}

		static void AddOrderLineFetchHints(WhsDocketLine line, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, line.PK);
		}

		#endregion
	}
}
