using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CreateLoadsFromOrdersMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public CreateLoadsFromOrdersMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("956503a2-c515-4750-87e1-3a50bc7c55c7", "Create Loads from Orders"), factory)
		{
		}

		protected override bool SupportsSummaryCore => true;

		const string OutputTextFormat = "{0} {1} - {2}";

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] orders)
		{
			log.SetSectionProgressMax(orders.Length);

			var factory = orders.FirstOrDefault()?.Factory;
			var orderPKs = AddFetchHints(orders, factory);
			InitialiseConsolidatedOrdersCache(orderPKs, factory);

			foreach (WhsOrder order in orders)
			{
				CreateLoadIfValid(order, log);
				log.BumpSectionProgress();
			}
		}

		protected override void InitialiseBeforeAllBatchesRunCore()
		{
			base.InitialiseBeforeAllBatchesRunCore();
			CreatedLoadHolder = new Dictionary<(ZGuid TransportCoPK, ZGuid DockDoorLocationPK, ZString CarrierServiceLevel), WhsLoad>();
			LogMessageActions = new List<Action<IOperationalActionSectionLog>>();
			ProcessedConsolidatedOrders = new HashSet<ZGuid>();
		}

		Guid[] AddFetchHints(BusinessObject[] orders, BusinessObjectFactory factory)
		{
			var orderPKs = new Guid[orders.Length];
			for (var i = 0; i < orders.Length; i++)
			{
				var orderPK = orders[i].PK;
				factory.AddFetchHint(JobDocAddressSchema.Instance, FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, orderPK));
				factory.AddFetchHint(WhsLoadOrderSchema.WOV_WD_Docket, orderPK);
				orderPKs[i] = orderPK.ToGuid();
			}
			return orderPKs;
		}

		void InitialiseConsolidatedOrdersCache(Guid[] orderPKs, BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				var allPackageJobs = new DynamicBusinessObjectCollection(factory);
				var sql = @"
SELECT
	sourcePackageJob.KJ_ParentID AS SourceOrderPK,
	allPackageJobsOnHU.KJ_ParentID AS ConsolidatedOrderPK
FROM
	dbo.PkgPackageJob sourcePackageJob
	JOIN dbo.PkgPackage sourcePackage ON sourcePackage.KP_KJ_ParentPackageJob = sourcePackageJob.KJ_PK
	JOIN dbo.PkgPackage allPackagesOnHU ON allPackagesOnHU.KP_KP_TopHandlingUnitPackage = sourcePackage.KP_KP_TopHandlingUnitPackage
	JOIN dbo.PkgPackageJob allPackageJobsOnHU ON allPackageJobsOnHU.KJ_PK = allPackagesOnHU.KP_KJ_ParentPackageJob
WHERE
	sourcePackageJob.KJ_ParentID IN (SELECT VALUE FROM @OrderPKs)
	AND allPackageJobsOnHU.KJ_ParentID NOT IN (SELECT VALUE FROM @OrderPKs)
	AND sourcePackage.KP_KP_TopHandlingUnitPackage IS NOT NULL
";

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add(ZSqlParameter.New("@OrderPKs", orderPKs, PkgPackageJobSchema.KJ_ParentID, isTableValued: true));

				allPackageJobs.Load(sql, sqlParams);

				ConsolidatedOrdersCache = new Dictionary<ZGuid, List<WhsOrder>>();
				var consolidatedOrderPKs = allPackageJobs.Select(p => (ZGuid)p["ConsolidatedOrderPK"]).ToArray();
				var consolidatedOrders = factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, consolidatedOrderPKs)).ToDictionary(o => o.PK);

				foreach (var packageJob in allPackageJobs)
				{
					var sourceOrderPK = (ZGuid)packageJob["SourceOrderPK"];
					var consolidatedOrderPK = (ZGuid)packageJob["ConsolidatedOrderPK"];
					if (!ConsolidatedOrdersCache.TryGetValue(sourceOrderPK, out var orders))
					{
						ConsolidatedOrdersCache[sourceOrderPK] = orders = new List<WhsOrder>();
					}
					orders.Add(consolidatedOrders[consolidatedOrderPK]);
				}
			}
		}

		#region CreateLoadIfValid

		void CreateLoadIfValid(WhsOrder order, IOperationalActionSectionLog log)
		{
			if (CanCreateLoad(order, log))
			{
				CreateLoadIfNotExistsAndAttachOrderToLoad(order);
			}
		}

		void CreateLoadIfNotExistsAndAttachOrderToLoad(WhsOrder order)
		{
			var transportCoPK = order.TransportCoPK;
			var warehouse = order.Warehouse;
			var dockDoorLocationPK = warehouse.WW_DefaultOutboundDockDoor;
			var carrierServiceLevel = order.WD_PL_NKCarrierServiceLevel;
			var messageIfCreated = "";

			if (!CreatedLoadHolder.TryGetValue((transportCoPK, dockDoorLocationPK, carrierServiceLevel), out var load_PossiblyInOtherFactory))
			{
				load_PossiblyInOtherFactory = CreateWhsLoad(order.Factory, transportCoPK, dockDoorLocationPK, carrierServiceLevel);
				CreatedLoadHolder.Add((transportCoPK, dockDoorLocationPK, carrierServiceLevel), load_PossiblyInOtherFactory);
				messageIfCreated = (NoResString)" (load created)";
			}
			// else, load may be from a previous batch's factory

			LogMessageActions.Add((log) =>
			{
				log.NotifyFormat(
					OperationalActionLogErrorLevel.Informational,
					Res.GetString(
						"2f54e992-3749-4af3-8ba8-7c23dc1bd88a",
						"{0} {1} - Attached to load {2} for the Transport Company '{3}' Carrier Service Level '{4}' for the Dock Door Location '{5}' Warehouse '{6}'{7}.",
						order.HumanReadableName,
						"{0}",
						"{1}",
						order.TransportCoName,
						order.WD_PL_NKCarrierServiceLevel,
						warehouse.DefaultOutboundDockDoorLocation.WLV_RowName,
						warehouse.HumanReadableName,
						messageIfCreated),
					GetDocketIdLink(order),
					GetLoadIdLink(load_PossiblyInOtherFactory));
			});

			order.WD_WLO_PlannedLoad = load_PossiblyInOtherFactory.PK;
			AttachConsolidatedOrders(order, load_PossiblyInOtherFactory.PK);
		}

		void AttachConsolidatedOrders(WhsOrder order, ZGuid loadPK)
		{
			if (ConsolidatedOrdersCache.TryGetValue(order.PK, out var consolidatedOrders))
			{
				foreach (var consolidatedOrder in consolidatedOrders)
				{
					if (!ProcessedConsolidatedOrders.Contains(consolidatedOrder.PK))
					{
						consolidatedOrder.WD_WLO_PlannedLoad = loadPK;
						LogMessageActions.Add((log) =>
						{
							log.NotifyFormat(
								OperationalActionLogErrorLevel.Warning,
								Res.GetString(
									"0f9ba6a4-9003-4830-acc1-15010a6f85f2",
									"{0} {1} - was automatically assigned to Load as it was consolidated with Order {2}.",
									consolidatedOrder.HumanReadableName,
									"{0}",
									"{1}")
								,
								GetDocketIdLink(consolidatedOrder),
								GetDocketIdLink(order));
						});
						ProcessedConsolidatedOrders.Add(consolidatedOrder.PK);
					}
				}
			}
		}

		Dictionary<(ZGuid TransportCoPK, ZGuid DockDoorLocationPK, ZString CarrierServiceLevel), WhsLoad> CreatedLoadHolder;
		Dictionary<ZGuid, List<WhsOrder>> ConsolidatedOrdersCache;
		HashSet<ZGuid> ProcessedConsolidatedOrders;
		List<Action<IOperationalActionSectionLog>> LogMessageActions;

		#region CreateWhsLoad

		static WhsLoad CreateWhsLoad(BusinessObjectFactory factory, ZGuid transportCompanyPK, ZGuid dockDoorPK, ZString carrierServiceLevel)
		{
			var load = factory.New<WhsLoad>();
			load.WLO_OH_TransportCompany = transportCompanyPK;
			load.WLO_WL_PlannedDockDoor = dockDoorPK;
			load.WLO_PL_NKCarrierServiceLevel = carrierServiceLevel;

			return load;
		}

		#endregion

		#endregion

		#region CanCreateLoad

		bool CanCreateLoad(WhsOrder order, IOperationalActionSectionLog log)
		{
			var result = false;

			if (ProcessedConsolidatedOrders.Contains(order.PK))
			{
				LogMessageActions.Add((log) =>
				{
					log.NotifyFormat(
						OperationalActionLogErrorLevel.Informational,
						Res.GetString(
							"dbe372fc-06ee-4ff6-8796-7be8882007d3",
							"{0} {1} - was already processed.",
							order.HumanReadableName,
							"{0}")
						,
						GetDocketIdLink(order));
				});
			}
			else if (order.WD_PL_NKCarrierServiceLevel.IsEmpty)
			{
				WriteInLogWarning(Res.GetString("c9ecf6c0-4de0-4a26-8935-2fc420c70f95", "does not have a Carrier Service Level."));
			}
			else if (order.IsOrderAssignedToLoad)
			{
				WriteInLogWarning(Res.GetString("3a62d7c1-c2d8-41b5-bf86-75a09283214c", "is already assigned to a load. If you are trying to assign a load to a partially loaded order, please do it on the Load desktop module."));
			}
			else if (order.IsOrderHeld)
			{
				WriteInLogWarning(Res.GetString("0265ea6f-006a-4007-8663-08ea3ddf3755", "has been held and cannot be loaded."));
			}
			else if (!order.WD_FinalisedDate.IsEmpty)
			{
				WriteInLogWarning(Res.GetString("391e008e-c1b1-4f02-b44f-262c2b730f7c", "is finalized and cannot be loaded."));
			}
			else if (order.IsCancelled)
			{
				WriteInLogWarning(Res.GetString("35cbe91b-1cc0-4c16-a45b-38a392012829", "is canceled and cannot be loaded."));
			}
			else if (order.Warehouse.WW_DefaultOutboundDockDoor.IsEmpty)
			{
				WriteInLogWarning(Res.GetString("c3506726-1053-44ad-950e-9a69f78f4661", "does not have a Default Outbound Dock Door on the Warehouse."));
			}
			else if (!order.TransportCoPK.IsValid)
			{
				WriteInLogWarning(Res.GetString("c53e2377-4c7b-4bbf-9948-5b8bd2e05da6", "does not have a Transport Company."));
			}
			else
			{
				result = true;
			}

			return result;

			void WriteInLogWarning(string msg)
				=> log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, GetDocketIdLink(order), msg);
		}

		#endregion

		#region Logging

		protected override void SummaryLogCore(IOperationalActionSectionLog log)
		{
			base.SummaryLogCore(log);

			foreach (var logMessage in LogMessageActions)
			{
				logMessage(log);
			}

			LogMessageActions.Clear();
		}

		#endregion
	}
}
