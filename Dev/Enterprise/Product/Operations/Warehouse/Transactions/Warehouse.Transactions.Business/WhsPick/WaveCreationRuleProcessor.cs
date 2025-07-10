using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseWaveCreation;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WaveCreationRuleProcessor : IScheduledRuleProcessor
	{
		public WaveCreationRuleProcessor(IPartiallyReplenishedPickSplitter pickSplitter)
		{
			PartiallyReplenishedPickSplitter = Argument.NotNull(pickSplitter, nameof(pickSplitter));
		}

		IPartiallyReplenishedPickSplitter PartiallyReplenishedPickSplitter { get; }

		public ZGuid GetBranchToRunRulesAgainst(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(ruleSet, nameof(ruleSet));

			var warehouse = factory.Load<WhsWarehouse>(ruleSet.PRS_WW_Warehouse);
			return warehouse.WW_GB_RelatedCompanyBranch;
		}

		public IEnumerable<IInputFact> LoadInputFacts(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet, CancellationToken cancellationToken)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(ruleSet, nameof(ruleSet));

			var orders = GetWhsOrders(factory, ruleSet.PRS_WW_Warehouse);
			return orders.Length > 0
				? GetOrderFacts(factory, orders, cancellationToken).ToArray()
				: Enumerable.Empty<IInputFact>();
		}

		static WhsOrder[] GetWhsOrders(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			var orderLineSubQuery = new ZDBOnlySubQuery(typeof(WhsOrderLine), WhsDocketLineSchema.WE_WD);
			orderLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_TransactionQuantity, SQLComparisonOperator.GreaterThan, 0m);

			var whsOrderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			whsOrderQuery.AddSubQuery(orderLineSubQuery, JoinCondition.And);
			whsOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			whsOrderQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehousePK);
			whsOrderQuery.AddToFilter(WhsDocketSchema.WD_WP, null);
			whsOrderQuery.AddToFilter(WhsDocketSchema.WD_RequiredDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			whsOrderQuery.AddToFilter(WhsDocketSchema.WD_PickOption, WhsPickOption.Codes.Auto);
			whsOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, new[] { DocketStatus.Codes.Held, DocketStatus.Codes.Cancelled });

			return factory.Load<WhsOrder>(whsOrderQuery);
		}

		static IEnumerable<IOrderFact> GetOrderFacts(ReadOnlyBusinessObjectFactory factory, IEnumerable<WhsOrder> orders, CancellationToken cancellationToken)
		{
			AddFetchHints(factory, orders);

			var organisationFacts = new Dictionary<ZGuid, OrganisationFact>();
			var docAddressFacts = new Dictionary<ZGuid, DocAddressFact>();
			var currencyConverter = CurrencyConverter.New(factory, ZDateTime.Now, ExchangeRateType.All, 7);
			var nonPickableOrderPKs = new List<ZGuid>();
			foreach (var order in orders)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (order.GetPickabilityWithoutPick().IsDocketPickable)
				{
					var orderFact = new OrderFact(
						order,
						WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, order.WD_OH_Client, () => order.Client),
						WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, order.ConsigneePK, () => order.Consignee),
						WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, order.TransportCoPK, () => order.TransportCo),
						WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, order.CarrierBookingAgentPK, () => order.CarrierBookingAgent),
						order.ConsigneeAddress?.OA_DeliveryRoute,
						currencyConverter,
						WarehouseFactsHelper.GetOrCreateDocAddressFact(docAddressFacts, organisationFacts, order.ConsigneeDocAddress),
						GetOrCreateDistributionCentreDocAddressFact(docAddressFacts, organisationFacts, order.DistributionCentreDocAddress));

					yield return orderFact;
				}
				else
				{
					nonPickableOrderPKs.Add(order.PK);
				}
			}

			if (nonPickableOrderPKs.Count > 0)
			{
				SaveNotPickableEvents(nonPickableOrderPKs);
			}
		}

		static IDocAddressFact GetOrCreateDistributionCentreDocAddressFact(Dictionary<ZGuid, DocAddressFact> docAddressFacts, Dictionary<ZGuid, OrganisationFact> organisationFacts, JobDocAddress distributionCentreDocAddress)
		{
			return distributionCentreDocAddress.IsEmpty
				? null
				: WarehouseFactsHelper.GetOrCreateDocAddressFact(docAddressFacts, organisationFacts, distributionCentreDocAddress);
		}

		#region AddFetchHints for GetOrderFacts

		static void AddFetchHints(BusinessObjectFactory factory, IEnumerable<WhsOrder> orders)
		{
			foreach (var order in orders)
			{
				factory.AddFetchHint(WhsDocketLineSchema.WE_WD, order.PK);
				factory.AddFetchHint(JobDocAddressSchema.Instance, FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, order.PK));
			}

			var productPKs = orders.SelectMany(o => o.Lines).Select(line => line.WE_OP).Distinct();
			foreach (var productPK in productPKs)
			{
				factory.AddFetchHint(OrgSupplierPartSchema.PK, productPK);
				factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, productPK);
			}

			AddOrganizationRelatedFetchHints(factory, orders);
		}

		static void AddOrganizationRelatedFetchHints(BusinessObjectFactory factory, IEnumerable<WhsOrder> orders)
		{
			var consigneeAndDistributionCentreAddresses = (GetAddresses(orders, DocAddressType.ConsigneeAddress).Concat(GetAddresses(orders, DocAddressType.DistributionCentreAddress))).ToArray();
			var transportCoAddresses = GetAddresses(orders, DocAddressType.TransportCompanyDocumentaryAddress).ToArray();
			var carrierBookingAgentAddresses = GetAddresses(orders, DocAddressType.CarrierBookingAgent).ToArray();

			AddOrgAddressFetchHints(factory,
				consigneeAndDistributionCentreAddresses.Select(address => address.E2_OA_Address)
				.Union(transportCoAddresses.Select(address => address.E2_OA_Address))
				.Union(carrierBookingAgentAddresses.Select(address => address.E2_OA_Address)));

			var clientPKs = orders.Select(order => order.WD_OH_Client);
			var orgPKs = clientPKs
				.Union(consigneeAndDistributionCentreAddresses.Select(address => address.OrganisationPK))
				.Union(transportCoAddresses.Select(address => address.OrganisationPK))
				.Union(carrierBookingAgentAddresses.Select(address => address.OrganisationPK));

			foreach (var orgPK in orgPKs)
			{
				factory.AddFetchHint(OrgHeaderSchema.PK, orgPK);
			}

			foreach (var address in IEnumerableExtensions.DistinctBy(consigneeAndDistributionCentreAddresses.Where(d => d.HasRealAddress), a => a.E2_OA_Address).Select(d => d.Address))
			{
				factory.AddFetchHint(OrgAddressSchema.OA_OH, address.OA_OH);
				factory.AddFetchHint(OrgContactSchema.OC_OH, address.OA_OH);
				factory.AddFetchHint(OrgCusCodeSchema.OK_OH, address.OA_OH);
			}

			FetchHintsHelper.AddFetchHintsForGlbCompanyOrgProxy(factory, orgPKs);
			FetchHintsHelper.AddFetchHintsForGlbBranchOrgProxy(factory, orgPKs);
		}

		static IEnumerable<JobDocAddress> GetAddresses(IEnumerable<WhsPickableDocket> orders, DocAddressType addressType)
		{
			return orders
				.Select(order => order.LoadJobDocAddressQuickly(addressType))
				.Where(address => address != null);
		}

		static void AddOrgAddressFetchHints(BusinessObjectFactory factory, IEnumerable<ZGuid> addressPKs)
		{
			foreach (var addressPK in addressPKs)
			{
				factory.AddFetchHint(OrgAddressSchema.PK, addressPK);
			}
		}

		static void CreateErrorLog(WhsOrder order, string reason)
		{
			var typeParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.WaveCreation);
			var reasonParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason);
			order.Logs.CreateOrRecreateEventLog(Events.ErrorReport, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, typeParameter, reasonParameter);
		}

		#endregion

		#region Save Not Pickable Events

		static void SaveNotPickableEvents(List<ZGuid> nonPickableOrderPKs)
		{
			var newFactory = new BusinessObjectFactory();
			var orders = newFactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, nonPickableOrderPKs));
			foreach (var order in orders)
			{
				CreateErrorLog(order, Constants.EventReferenceParameterReasons.NotPickable);
			}

			newFactory.Save();
		}

		#endregion

		public void ProcessResults(BusinessObjectFactory factory, ProductionRulesEngineResult result, INotifications notifications, CancellationToken cancellationToken)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(result, nameof(ProductionRulesEngineResult));
			Argument.NotNull(notifications, nameof(notifications));

			var decoratedNotifications = new NotificationsDecorator(notifications);

			var wavedOrderFacts = new Dictionary<Guid, List<IOrderFact>>();
			var waveFacts = new List<WaveFact>();
			foreach (var fact in result.Facts)
			{
				if (fact is IOrderFact orderFact && orderFact.WaveFactPK != Guid.Empty)
				{
					if (wavedOrderFacts.TryGetValue(orderFact.WaveFactPK, out var orderFactsList))
					{
						orderFactsList.Add(orderFact);
					}
					else
					{
						wavedOrderFacts[orderFact.WaveFactPK] = new List<IOrderFact> { orderFact };
					}
				}
				else if (fact is WaveFact waveFact)
				{
					waveFacts.Add(waveFact);
				}
			}

			var allOrders = LoadOrders(factory, wavedOrderFacts.SelectMany(wof => wof.Value));
			var orderLookup = allOrders.ToDictionary(o => o.PK);

			foreach (var waveFact in waveFacts)
			{
				var orderIDs = string.Empty;
				WhsPick pickFromSplit = null;
				var pick = PrepareNewPick(factory, waveFact);
				pick.PickOrdersFailed += (s, args) => decoratedNotifications.AddError(args.Message.TrimEnd());
				pick.AutoPickAttempt += HandleAutoPickAttempt;
				pick.NotificationManager.Push(decoratedNotifications);

				var orderFacts = wavedOrderFacts[waveFact.PK];
				var orders = orderFacts.Select(of => orderLookup[of.PK]).ToArray();

				pick.PickOrders(orders, saveFactory: false);

				if (decoratedNotifications.ReportedFatalError)
				{
					break;
				}
				else
				{
					if (AttemptToDetachOrdersInShortfall(pick))
					{
						pick.Delete();
						notifications.AddWarning(Res.GetString("103a5c8d-bd0e-4bc3-95d6-a23edbf48885", "After detaching Orders in shortfall, no Orders are attached to the Pick."));
					}
					else
					{
						AddSplitFetchHints(pick);
						orderIDs = string.Join(", ", pick.Orders.Select(o => o.WD_DocketID).OrderBy(id => id));
						pickFromSplit = PartiallyReplenishedPickSplitter.SplitOrdersFromPartiallyReplenishedPick(pick);
						factory.Saved += LogCreatedPick;
					}
				}

				cancellationToken.ThrowIfCancellationRequested();

				void LogCreatedPick(BusinessObjectFactory f, bool savedSuccessfully)
				{
					if (savedSuccessfully)
					{
						var awaitingReplenishment = pick.WP_IsAwaitingReplenishment ? (NoResString)" (awaiting replenishment) " : " ";  // Service Task Logging
						notifications.Add(CargoWise.ComponentModel.NotificationType.Information, $"Created {pick.HumanReadableName}{awaitingReplenishment}with Order(s): {orderIDs}."); // Service Task Logging

						if (pickFromSplit != null)
						{
							notifications.Add(CargoWise.ComponentModel.NotificationType.Information, $"Created {pickFromSplit.HumanReadableName} by splitting the awaiting replenishment {pick.HumanReadableName}. This pick contains Order(s): {string.Join(", ", pickFromSplit.Orders.Select(o => o.WD_DocketID).OrderBy(id => id))}."); // Service Task Logging
						}
					}

					f.Saved -= LogCreatedPick;
				}
			}

			if (wavedOrderFacts.Count == 0 || decoratedNotifications.ReportedFatalError)
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, InformationMessageForNothingProcessed);
			}

			void HandleAutoPickAttempt(object s, WhsPick.AutoPickEventArgs args)
			{
				if (!args.StockWasAllocated && !args.IsPickWaitingReplenishment)
				{
					decoratedNotifications.AddWarning(Res.GetString("ef615825-d470-4d09-a2f3-8178763ba73b", "No Stock was allocated to pick."));
				}
				else
				{
					// This branch should never get hit with the current implementation
					// As AutoPickAttempt event only occurs if no stock was allocated, or while attempting to perform save
					ErrorReporter.ReportOnce(nameof(WaveCreationRuleProcessor), "HandleAutoPickAttempt event was thrown for a picking orders where stock was allocated.");
					decoratedNotifications.AddWarning(args.Message);
				}
			}
		}

		IEnumerable<WhsOrder> LoadOrders(BusinessObjectFactory factory, IEnumerable<IOrderFact> orderFacts)
		{
			var orderPKs = orderFacts.Select(o => o.PK).ToArray();

			AddFetchHints(factory, orderPKs);

			var ordersQuery = new ZQuery(WhsDocketSchema.PK, orderPKs);
			ordersQuery.AddToFilter(WhsDocketSchema.WD_WP, null);

			var orders = factory.Load<WhsOrder>(ordersQuery);
			AddPickLineFetchHints(factory, orders);

			return orders;
		}

		#region Add FetchHints for ProcessResults

		void AddFetchHints(BusinessObjectFactory factory, IEnumerable<Guid> orderPKs)
		{
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WD, orderPKs));
			factory.AddFetchHint(PkgPackageJobSchema.Instance, new ZQuery(PkgPackageJobSchema.KJ_ParentID, orderPKs));
			factory.AddFetchHint(JobDocAddressSchema.Instance, new ZQuery(JobDocAddressSchema.E2_ParentID, orderPKs));
		}

		void AddPickLineFetchHints(BusinessObjectFactory factory, IEnumerable<WhsOrder> orders)
		{
			var linePKs = orders.SelectMany(o => o.Lines.Select(l => l.PK));
			factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, linePKs));
		}

		void AddSplitFetchHints(WhsPick pick)
		{
			var factory = pick.Factory;
			foreach (var order in pick.Orders.Cast<WhsPickableDocket>())
			{
				factory.AddFetchHint(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, order.WD_OH_Client);
			}
		}

		#endregion

		WhsPick PrepareNewPick(BusinessObjectFactory factory, WaveFact waveFact)
		{
			var pick = factory.New<WhsPick>();
			pick.WP_CartoniseSplitCases = waveFact.CartonizeSplitCases;
			pick.WP_PickPalletsByLabel = waveFact.PickPalletsByLabel;
			pick.WP_PickCasesByLabel = waveFact.PickCasesByLabel;
			pick.WP_ForcePickByCaseUOMTypeAllocation = waveFact.ForcePickByCaseUOMTypeAllocation;
			pick.WP_ForceSplitCaseUOMTypeAllocation = waveFact.ForceSplitCaseUOMTypeAllocation;

			return pick;
		}

		bool AttemptToDetachOrdersInShortfall(WhsPick pick)
		{
			while (pick.Orders.Count > 0 && pick.WP_PickStatus == PickStatus.Codes.Building)
			{
				var unfulfilledOrder = pick.Orders.Where(order => (((WhsOrder)order).ClientPickingParams?.WPP_DetachWavedOrdersWithBlockingShortfall ?? true)
															&& !order.IsFulfillmentRuleMet).MinBySafe(p => p.WD_PickPriority, new PickPriorityComparer());

				if (unfulfilledOrder == null)
				{
					break;
				}

				pick.RemoveOrders(new[] { unfulfilledOrder });
				CreateErrorLog((WhsOrder)unfulfilledOrder, Constants.EventReferenceParameterReasons.Shortfall);

				if (pick.Orders.Count > 0)
				{
					var orders = pick.Orders.ToArray<WhsPickableDocket>();
					pick.RemoveOrders(orders); // Detach and reattach to handle reinstating cross dock allocations, pick by BOM etc.
					pick.PickOrders(orders, saveFactory: false);
				}
			}

			return pick.Orders.Count == 0;
		}

		public string InformationMessageForNothingProcessed => (NoResString)"No picks were created from this run."; // Service Task Logging

		public GuidRegistryItem ErrorContactGroupRegistryItem => WarehouseDataRegistry.Instance.WaveCreationRulesFailureNotificationGroup;
	}
}
