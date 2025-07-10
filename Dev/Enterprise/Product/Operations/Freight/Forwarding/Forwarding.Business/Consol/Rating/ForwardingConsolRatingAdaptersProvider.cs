using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	internal class ForwardingConsolRatingAdaptersProvider : ConsolRatingAdaptersProvider<ForwardingConsol>
	{
		public ForwardingConsolRatingAdaptersProvider(ForwardingConsol parent) : base(parent)
		{
		}

		public override bool IncludeChildrenProviders(CostSell costOrSell, BillingType billingType) => costOrSell != CostSell.Cost || ParentRatingSupporter.ContinueAutorateCosting(billingType);

		protected override List<IAutoRating> GetAdapters(ForwardingConsol parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			if (options.AutoratingProcess == CostSell.Cost && !ParentRatingSupporter.ContinueAutorateCosting(options.BillingType))
			{
				return new List<IAutoRating>();
			}

			var ratingRoutes = parent.GetRatingRoutes(options.AutoratingProcess);
			List<IAutoRating> result;

			if (ratingRoutes.Any())
			{
				result = ratingRoutes.SelectMany(x => CreateAdapters(x, options.AutoratingProcess, true)).ToList();

				// In case of multi-route mode, we still want to autorate origin/destination charges based on first load and last discharge port and consol
				// creditors. That's why additionally to routing adapters (created per route), we create a consol adapter for the whole route configured to
				// autorate specific charges code groups.
				//
				// The consol adapter is created only if we have more than 1 route. The thing is, if we have 1 route, it will have the same origin and destination
				// as the consol and therefore will autorate consol charges as well. So, no need to create a separate consol adapter to autorate consol charges.
				// But this is a bit of hack. I would enhance it so that route adapter autorates only route charges (even if there are only 1 route) and consol adapter
				// is created always to autorate consol charges.
				if (ratingRoutes.Count > 1)
				{
					result.AddRange(CreateConsolAdaptersForMultiRoutingMode(parent, options.AutoratingProcess));
				}

				result.Add(new ForwardingConsolJobServicesAdapter(new ConsolRatingRoute(parent)));
			}
			else
			{
				result = CreateAdapters(new ConsolRatingRoute(parent), options.AutoratingProcess).ToList();
			}

			var includeGatewayShipmentAdapters = (options.AutoratingProcess == CostSell.Revenue || options.BillingType == BillingType.Invoicing) && parent.IsGateway();

			if (includeGatewayShipmentAdapters)
			{
				result.AddRange(
					((ForwardingConsolRatingAdapter)parent.RatingAdapter)
					.AllShipments
					.Cast<ForwardingShipment>()
					.Select(x => new GatewayShipmentRatingAdapter(parent, x)));
			}

			return result;
		}

		/// <summary>
		///		As per description below, we want to do this only when we autorate in multi route mode.
		/// </summary>
		public override bool NeedsHandleResult => RatingDataRegistry.Instance.MultiModalRatingCost.Value;

		/// <summary>
		///		If Autorating routes is enabled in the registry, some charges we autorate on a route level and some on a consol level.
		///		If the same charge found on both route and consol - we need to decide which one to pick based on creditor priorities.
		///		Since we don't have this logic across multiple adapters this is a trick of achieving it. I.e. at the end of autorating,
		///		the autorating engine gives us right to check results per adapter edit them if required.
		///
		///		So, we delete charges from consol if they were autorated on routes.
		/// </summary>
		public override bool HandleResult(IReadOnlyDictionary<IRatingAdapter, List<IAutoRatedCharge>> charges)
		{
			var updated = false;

			// We skip results from all other adapter types, and ONLY update/modify standard (not service) results for consol or routes
			var consolResult = charges.Where(a => a.Key is ForwardingConsolRatingAdapter { RouteSet: null }).ToList();
			var routeResults = charges.Where(a => a.Key is ForwardingConsolRatingAdapter { RouteSet: not null }).ToList();

			if (consolResult.Count == 0 || routeResults.Count == 0)
			{
				return false;
			}

			// There supposed to be 1 consol anyway
			var consolCharges = consolResult.First().Value;

			foreach (var routeResult in routeResults)
			{
				var routeAdapter = (ForwardingConsolRatingAdapter)routeResult.Key;
				var routeCharges = routeResult.Value;
				var routeChargesToRemove = new List<IAutoRatedCharge>();

				foreach (var routeCharge in routeCharges)
				{
					var consolChargesToRemove = new List<IAutoRatedCharge>();
					var matchChargeList = consolCharges.Where(consolCharge => consolCharge.ChargeCode.AC_Code == routeCharge.ChargeCode.AC_Code).ToList();
					foreach (var consolCharge in matchChargeList)
					{
						var preferredCharge = GetPreferredCharge(routeAdapter, consolCharge, routeCharge);
						if (preferredCharge == routeCharge)
						{
							consolChargesToRemove.Add(consolCharge);
						}
						else if (preferredCharge == consolCharge)
						{
							routeChargesToRemove.Add(routeCharge);
						}
					}

					consolChargesToRemove.ForEach(c =>
					{
						consolCharges.Remove(c);
						updated = true;
					});
				}

				routeChargesToRemove.ForEach(c =>
				{
					routeCharges.Remove(c);
					updated = true;
				});
			}

			return updated;
		}

		/// <summary>
		///		Determines which charge from which adapter is preferred based on their attributes. Currently we compare only by service provider
		///		using the predefined creditors ranks, but later it may be extended.
		///
		///		Returns null if can't determine the preferred one, in this case both should come through.
		/// </summary>
		IAutoRatedCharge GetPreferredCharge(ForwardingConsolRatingAdapter routeAdapter, IAutoRatedCharge consolCharge, IAutoRatedCharge routeCharge)
		{
			var allowDifferentProvider = DataRegistryRating.Instance.AllowChargesWithSameChargeCodeForDifferentProvider
				.GetIsEnabled(routeAdapter.ConsumerType?.Code ?? string.Empty, routeAdapter.FreightMode.ToTransportMode(), routeAdapter.JobDirection);

			if (allowDifferentProvider)
			{
				return null;
			}

			// Creditors ranks are defined in creditors collection by an adapter. But, in a multi-route mode when we create an adapter per route
			// and then an adapter for consol for first load and last discharge, the route adapters include only route creditors. In this case,
			// we can't identify the rank of service provider of the consol charge.
			//
			// This flag (a hack) forces the route to include consol creditors as well with ranks based on predefined creditor priorities.
			routeAdapter.RouteSet.ForceConsolCreditors = true;

			// Since route adapter now has both - route creditors and consol creditors, we can determine ranks for both
			var routeCostProvider = routeAdapter.Creditors.AllOrgsWithSource.FirstOrDefault(c => c.Org.PK == routeCharge.ProviderPK);
			var consolCostProvider = routeAdapter.Creditors.AllOrgsWithSource.FirstOrDefault(c => c.Org.PK == consolCharge.ProviderPK);

			if (consolCostProvider == null)
			{
				// Should not be null, just in case
				return routeCharge;
			}

			if (routeCostProvider == null)
			{
				// Should not be null, just in case
				return consolCharge;
			}

			var tp1Ranking = routeAdapter.Creditors.GetRank(((AccChargeCode)consolCharge.ChargeCode).AC_ChargeGroup, consolCostProvider);
			var tp2Ranking = routeAdapter.Creditors.GetRank(((AccChargeCode)routeCharge.ChargeCode).AC_ChargeGroup, routeCostProvider);

			if (tp1Ranking.Intersect(tp2Ranking).Any())
			{
				return routeCharge;
			}

			var tp1Rank = tp1Ranking.Any() ? tp1Ranking.Min() : int.MaxValue;
			var tp2Rank = tp2Ranking.Any() ? tp2Ranking.Min() : int.MaxValue;

			return tp2Rank.CompareTo(tp1Rank) <= 0 ? routeCharge : consolCharge;
		}

		IEnumerable<IAutoRating> CreateConsolAdaptersForMultiRoutingMode(ForwardingConsol parent, CostSell costSell)
		{
			// As per HLD for DHL, we want to autorate these charge code groups for these consol orgs additionally to routes in multi route mode (when we
			// autorate routes rather than a consol). By default, a consol adapter will autorate all freight charge code groups (not only origin and destination ones)
			// and will fallback to best matching route creditors which we don't want.
			var chargeCodeGroupCollection = new ChargeCodeGroupCollection();
			chargeCodeGroupCollection.AddRange(new[]
			{
				ChargeCodeGroupList.Codes.Origin,
				ChargeCodeGroupList.Codes.Destination,
				ChargeCodeGroupList.Codes.Unloading,
				ChargeCodeGroupList.Codes.Loading,
			});
			chargeCodeGroupCollection.SellChargesFilter = ChargeCodeFilter.AutorateNothing;
			chargeCodeGroupCollection.CostChargesFilter = ChargeCodeFilter.AutorateConsolLevelOnly;

			var creditors = new Creditors();

			foreach (var group in chargeCodeGroupCollection)
			{
				switch (group)
				{
					case ChargeCodeGroupList.Codes.Loading:
					case ChargeCodeGroupList.Codes.Origin:
						creditors.Add(group, new OrgPrioritizedList(
							OrgWithSource.NewFrom<OrgAddress>(parent.JK_OA_DepartureCTOAddressInfo),
							OrgWithSource.NewFrom<OrgAddress>(parent.JK_OA_PackDepotAddressInfo),
							OrgWithSource.NewFrom<OrgAddress>(parent.JK_OA_DeparturePackCFSTransportAddressInfo)));
						break;

					case ChargeCodeGroupList.Codes.Unloading:
					case ChargeCodeGroupList.Codes.Destination:
						creditors.Add(group, new OrgPrioritizedList(
							OrgWithSource.NewFrom<OrgAddress>(parent.JK_OA_ArrivalCTOAddressInfo),
							OrgWithSource.NewFrom<OrgAddress>(parent.JK_OA_UnpackDepotAddressInfo),
							OrgWithSource.New(parent.ArrivalUnpackCFSTransport, new List<string>
							{
								parent.HumanReadableName,
								parent.JK_OH_ArrivalUnpackCFSTransportInfo.HumanReadableName
							})));
						break;
				}
			}

			var consolRoute = new ConsolRatingRoute(parent) { Creditors = creditors };
			consolRoute.SupportsManualRateSelection = false;
			var adapters = CreateAdapters(consolRoute, costSell, true, chargeCodeGroupCollection).ToList();
			return adapters;
		}

		IEnumerable<IAutoRating> CreateAdapters(IRatingRoute<IRoutingSupport> ratingRoute, CostSell costSell, bool dontAutorateServices = false, ChargeCodeGroupCollection chargeCodeGroupsOverride = null)
		{
			var consol = (CommonConsol)ratingRoute.Parent;
			var adapters = new List<IAutoRating>();

			if (consol.JK_ConsolMode == Constants.ContainerModes.ULD && costSell == CostSell.Cost)
			{
				// In case of autorating costs for ULD Consol, we want to autorate both ULD and LSE measures.
				//
				// This it to achieve client scenario when you have cargoes more than you can fit into the ULD reserved
				// but those additional cargoes could not consume full ULD, you may want to ship the additional cargoes
				// on the same Consol per agreement with the relevant carrier.
				//
				// So, we have the following responsibilities:
				//
				// UldForwardingConsolRatingAdapter autorates:
				// - packed lines using rates with ULD, AIR, ALL mode
				// - ULD containers
				// - job level charges (like job weight/volume, shipment count, etc.)
				//
				// LseInUldForwardingConsolRatingAdapter autorates:
				// - unpacked lines using rates with LSE, AIR, ALL mode
				adapters.Add(new UldForwardingConsolRatingAdapter(ratingRoute, dontAutorateServices));

				if (LseInUldForwardingConsolRatingAdapter.CanAutoRate(ratingRoute))
				{
					adapters.Add(new LseInUldForwardingConsolRatingAdapter(ratingRoute, dontAutorateServices));
				}
			}
			else
			{
				adapters.Add(new ForwardingConsolRatingAdapter(ratingRoute, dontAutorateServices));
			}

			foreach (var adapter in adapters)
			{
				if (chargeCodeGroupsOverride != null
					&& adapter is ForwardingConsolRatingAdapter chargeCodeAdapter)
				{
					chargeCodeAdapter.ChargeCodeGroupsOverride = chargeCodeGroupsOverride;
				}
			}

			return adapters;
		}
	}
}
