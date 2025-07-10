using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region GetPackagesToPutawayToConsolidationLocations

		[WebMethod(Description = "Get Packages To Putaway To Consolidation Locations")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPackageToPutawayToConsolidationLocationsResponse GetPackagesToPutawayToConsolidationLocations(Guid jobPk, PickJobType pickJobType)
		{
			return HandleWebServiceRequest<WhsPackageToPutawayToConsolidationLocationsResponse>(response => GetPackagesToPutawayToConsolidationLocations(response, jobPk, pickJobType));
		}

		void GetPackagesToPutawayToConsolidationLocations(WhsPackageToPutawayToConsolidationLocationsResponse response, Guid jobPk, PickJobType pickJobType)
		{
			var packagesToPutawayGroupedByOrders = GetPackagesToConsolidateGroupedByOrder(response, jobPk, pickJobType);
			AllocateConsolidationLocationForPackages(response, packagesToPutawayGroupedByOrders, GetPackagesToPutawayToConsolidationLocationsConcurrencyErrorMessage);
		}

		void AllocateConsolidationLocationForPackages(WhsPackageToPutawayToConsolidationLocationsResponse response, Dictionary<WhsOrder, (List<WhsTransferLine>, List<PkgPackage>)> packagesToPutawayGroupedByOrders, string concurrencyErrorMessage)
		{
			if (response.NoError())
			{
				response.PackagesToConsolidationLocationGroupingInfos = AllocateLocationsForPackages(packagesToPutawayGroupedByOrders);
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		static string GetPackagesToPutawayToConsolidationLocationsConcurrencyErrorMessage
			=> Res.GetString("2f73e8dd-de32-4088-81eb-a27c1ed36b5f", "Another user has changed the pick job while you have been working on it. Please restart the operation and try again.");

		#region GetPackagesToConsolidateGroupedByOrder

		Dictionary<WhsOrder, (List<WhsTransferLine>, List<PkgPackage>)> GetPackagesToConsolidateGroupedByOrder(WhsPackageToPutawayToConsolidationLocationsResponse response, Guid jobPK, PickJobType pickJobType)
		{
			var couldLoadJob = false;

			Dictionary<WhsOrder, (List<WhsTransferLine>, List<PkgPackage>)> packagesToConsolidate = null;

			switch (pickJobType)
			{
				case PickJobType.Pick:

					var pick = Factory.Load<WhsPick>(jobPK);
					if (pick != null)
					{
						couldLoadJob = true;

						var orderLookup = pick.Orders.Cast<WhsOrder>().ToDictionary(o => o.PK);
						Factory.AddFetchHint(typeof(JobDocAddress), new ZQuery(JobDocAddressSchema.E2_ParentID, orderLookup.Keys));

						var packageJobs = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, orderLookup.Keys));
						var packageJobLookup = packageJobs.ToDictionary(pkgJob => pkgJob.PK);
						var outerPackages = PutawayStockInDockDoorOrPackingStationHelper.LoadOuterPackagesFromPackageJobs(Factory, packageJobLookup.Keys);
						packagesToConsolidate = AddPackagesAndTransferLinesToConsiderForConsolidation(response, outerPackages, pickJobType, (package) =>
						{
							var orderPK = packageJobLookup[package.KP_KJ_ParentPackageJob].KJ_ParentID;
							return orderLookup[orderPK];
						});

						if (!response.RequiresDockDoorPutaway)
						{
							var packagePicklinePKs = outerPackages.SelectMany(p => p.PackedItemDivots).Select(d => d.KI_ParentID);
							CheckIfAnyLooseStockRequiresPutaway(response, orderLookup.Keys, packagePicklinePKs);
						}
					}

					break;

				case PickJobType.TrolleyJob:
					var trolleyJobPackages = PutawayStockInDockDoorOrPackingStationHelper.LoadPackagesForTrolleyJob(Factory, jobPK);
					if (trolleyJobPackages.Length > 0)
					{
						couldLoadJob = true;

						packagesToConsolidate = AddPackagesAndTransferLinesToConsiderForConsolidation(response, trolleyJobPackages, pickJobType, (pkg) => (WhsOrder)pkg.PackageJob.ParentJob);
					}

					break;

				case PickJobType.PickByLabelJob:
					var pickByLabelPackages = PutawayStockInDockDoorOrPackingStationHelper.LoadPackagesForPickByLabelJob(Factory, jobPK);
					if (pickByLabelPackages.Length > 0)
					{
						couldLoadJob = true;

						packagesToConsolidate = AddPackagesAndTransferLinesToConsiderForConsolidation(response, pickByLabelPackages, pickJobType, (pkg) => (WhsOrder)pkg.PackageJob.ParentJob);
					}

					break;

				default:
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid PickJobType {0}", pickJobType));
			}

			if (!couldLoadJob)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("0ae8432f-411b-454f-848a-7c0caef640b8", "Error loading job.");
			}

			return packagesToConsolidate;
		}

		Dictionary<WhsOrder, (List<WhsTransferLine>, List<PkgPackage>)> AddPackagesAndTransferLinesToConsiderForConsolidation(
			WhsPackageToPutawayToConsolidationLocationsResponse response,
			PkgPackage[] packages,
			PickJobType pickJobType,
			Func<PkgPackage, WhsOrder> getOrderFromPackageJob)
		{
			var packagesToConsolidate = new Dictionary<WhsOrder, (List<WhsTransferLine> TransferLines, List<PkgPackage> Packages)>();
			if (pickJobType == PickJobType.TrolleyJob && packages[0].GetIsTote())
			{
				response.RequiresDockDoorPutaway = true;
			}
			else
			{
				var (pickLineLookup, _) = PutawayStockInDockDoorOrPackingStationHelper.GetPickLineLookup(Factory, packages, pickJobType);
				var user = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

				var orderAllowsConsolidation = new Dictionary<ZGuid, bool>();

				foreach (var package in packages)
				{
					if (package.KP_KPH_PackageHeader.IsEmpty)
					{
						response.RequiresDockDoorPutaway = true;
					}
					else
					{
						var packagePickLinePKs = package.PackedItemDivots.Select(d => d.KI_ParentID);
						var packagePickLines = packagePickLinePKs.Select(pk => pickLineLookup[pk]);

						var inTransitTransferLines = PutawayStockInDockDoorOrPackingStationHelper.GetInTransitTransferLinesFromPickLines(Factory, packagePickLines, user.GS_Code).ToArray();

						if (inTransitTransferLines.Length > 0)
						{
							var order = getOrderFromPackageJob(package);

							if (order.WD_UseDirectedPackingConsolidation && PickAllowsConsolidationPutaway(orderAllowsConsolidation, pickJobType, order.WD_WP))
							{
								if (!packagesToConsolidate.TryGetValue(order, out var transferLinesAndPackages))
								{
									packagesToConsolidate[order] = transferLinesAndPackages = (new List<WhsTransferLine>(), new List<PkgPackage>());
								}

								transferLinesAndPackages.TransferLines.AddRange(inTransitTransferLines);
								transferLinesAndPackages.Packages.Add(package);
							}
							else
							{
								response.RequiresDockDoorPutaway = true;
							}
						}
					}
				}
			}

			return packagesToConsolidate;

			bool PickAllowsConsolidationPutaway(Dictionary<ZGuid, bool> orderAllowsConsolidation, PickJobType pickJobType, ZGuid pickPK)
			{
				if (!orderAllowsConsolidation.TryGetValue(pickPK, out var allowsConsolidation))
				{
					if (pickJobType == PickJobType.Pick && WebServiceHelper.IsPickAndPackEnabled(Factory.Load<WhsPick>(pickPK)))
					{
						allowsConsolidation = true;
					}
					else
					{
						var queryResults = PutawayStockInDockDoorOrPackingStationHelper.BuildAndRunQueryForPicksWithLooseInventory(Factory, new[] { pickPK }, topOneOnly: true);
						allowsConsolidation = queryResults.Count <= 0;
					}

					orderAllowsConsolidation[pickPK] = allowsConsolidation;
				}
				return allowsConsolidation;
			}
		}

		void CheckIfAnyLooseStockRequiresPutaway(WhsPackageToPutawayToConsolidationLocationsResponse response, IEnumerable<ZGuid> orderPKs, IEnumerable<ZGuid> packagePickLinePKs)
		{
			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_InventoryLine, WhsDocketLineSchema.PK);
			transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketLineStatus.Codes.Finalised);

			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsDocketLineSchema.PK, WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, SQLComparisonOperator.NotEqual, null);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.PK, SQLComparisonOperator.NotEqual, packagePickLinePKs);
			pickLineSubQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);

			var orderLineQuery = new ZDBOnlyQuery(typeof(WhsDocketLine));
			orderLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, SQLComparisonOperator.Equal, orderPKs);
			orderLineQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var orderLine = Factory.LoadTop1<WhsOrderLine>(orderLineQuery);

			response.RequiresDockDoorPutaway = orderLine != null;
		}

		#endregion

		#region AllocateLocationsForPackages

		ConsolidationLocationToPackagesGroupingInfo[] AllocateLocationsForPackages(
			Dictionary<WhsOrder, (List<WhsTransferLine> TransferLines, List<PkgPackage> Packages)> packagesToPutawayGroupedByOrders)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var packingConsolidationAllocationHelper = new PackingConsolidationAllocationHelper(Factory, warehouse);

			AddFetchHintsForTransferLines(packagesToPutawayGroupedByOrders.SelectMany(v => v.Value.TransferLines));
			var groupedPackageAllocations = new Dictionary<ZGuid, List<ConsolidationPackageInfo>>();

			var updatedTransfers = new HashSet<ZGuid>();
			foreach (var groupedPackages in packagesToPutawayGroupedByOrders.OrderByDescending(o => o.Key.WD_RequiredDate))
			{
				var order = groupedPackages.Key;
				var cannotOverrideConsolidationLocation = packingConsolidationAllocationHelper.HasExistingFinalisedConsolidationLocationAllocation(order.PK);
				var consolidationLocationPK = packingConsolidationAllocationHelper.AllocationConsolidationLocationForOrder(order);

				var transferLines = groupedPackages.Value.TransferLines;
				transferLines.ForEach(tl =>
				{
					tl.WE_WL = consolidationLocationPK;
					updatedTransfers.Add(tl.WE_WD);
				});

				var packages = groupedPackages.Value.Packages;

				if (!consolidationLocationPK.IsEmpty)
				{
					if (!groupedPackageAllocations.TryGetValue(consolidationLocationPK, out var allocations))
					{
						groupedPackageAllocations[consolidationLocationPK] = allocations = new List<ConsolidationPackageInfo>();
					}

					allocations.AddRange(packages.Select(pkg => new ConsolidationPackageInfo(new PackageInfo(pkg), order.PK.ToGuid(), cannotOverrideConsolidationLocation)));
				}
			}

			AddFetchHintsForUpdatedTransfers(updatedTransfers);
			return PrepareInfosForResponse(groupedPackageAllocations, packagesToPutawayGroupedByOrders, packingConsolidationAllocationHelper);
		}

		ConsolidationLocationToPackagesGroupingInfo[] PrepareInfosForResponse(
			Dictionary<ZGuid, List<ConsolidationPackageInfo>> groupedPackageAllocations,
			Dictionary<WhsOrder, (List<WhsTransferLine> TransferLines, List<PkgPackage> Packages)> packagesToPutawayGroupedByOrders,
			PackingConsolidationAllocationHelper packingConsolidationAllocationHelper)
		{
			var resultConsolidationLocationToPackagesInfos = new List<ConsolidationLocationToPackagesGroupingInfo>();
			foreach (var packageAllocation in groupedPackageAllocations)
			{
				var consolidationLocation = packingConsolidationAllocationHelper.RetrieveConsolidationLocationInfoForLocationPK(packageAllocation.Key);
				var info = new ConsolidationLocationToPackagesGroupingInfo(consolidationLocation, SortPackageInfos(packageAllocation.Value));

				resultConsolidationLocationToPackagesInfos.Add(info);
			}

			foreach (var orderGrouping in packingConsolidationAllocationHelper.RetrieveUnallocatedOrderGroupings())
			{
				var consolidationPackageInfos = new List<ConsolidationPackageInfo>();
				foreach (var order in orderGrouping)
				{
					consolidationPackageInfos.AddRange(packagesToPutawayGroupedByOrders[order].Packages.Select(pkg => new ConsolidationPackageInfo(new PackageInfo(pkg), order.PK.ToGuid(), false)));
				}

				var info = new ConsolidationLocationToPackagesGroupingInfo(new WhsLocationInfo(), SortPackageInfos(consolidationPackageInfos));
				resultConsolidationLocationToPackagesInfos.Add(info);
			}

			resultConsolidationLocationToPackagesInfos.Sort(new SortConsolidationLocations(Factory));
			return resultConsolidationLocationToPackagesInfos.ToArray();

			ConsolidationPackageInfo[] SortPackageInfos(IEnumerable<ConsolidationPackageInfo> packages)
			{
				return packages.OrderBy(pkg => pkg.Package.PackageID).ToArray();
			}
		}

		class SortConsolidationLocations : SortByStandardPickingOrPutawayFields<ConsolidationLocationToPackagesGroupingInfo>
		{
			public SortConsolidationLocations(BusinessObjectFactory factory)
			{
				Factory = Argument.NotNull(factory, nameof(factory));
			}

			BusinessObjectFactory Factory { get; }

			protected override IEnumerable<IComparer<ConsolidationLocationToPackagesGroupingInfo>> GetElementaryComparers()
				=> GetComparersForSortByLocationAndThenByProduct(l => GetLocation(l.ConsolidationLocation.LocationPK), l => null, isPickSort: false);

			WhsLocation GetLocation(ZGuid locationPK)
			{
				if (!LocationCache.TryGetValue(locationPK, out var location))
				{
					LocationCache[locationPK] = location = Factory.Load<WhsLocation>(locationPK);
				}

				return location;
			}

			Dictionary<ZGuid, WhsLocation> LocationCache => locationCache ?? (locationCache = new Dictionary<ZGuid, WhsLocation>());
			Dictionary<ZGuid, WhsLocation> locationCache;
		}

		#endregion

		void AddFetchHintsForUpdatedTransfers(IEnumerable<ZGuid> updatedTransfers)
		{
			// Factory Save will perform hits to WhsVASOrder per updated Transfer
			Factory.AddFetchHint(WhsVASOrderSchema.Instance, new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, updatedTransfers));
			Factory.AddFetchHint(WhsVASOrderSchema.Instance, new ZQuery(WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, updatedTransfers));
		}

		void AddFetchHintsForTransferLines(IEnumerable<WhsTransferLine> transferlines)
		{
			transferlines.ForEach(t => Factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, t.WE_WE_OriginalDocketLineForRating));
			var receiveQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			var receiveLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.PK, transferlines.Select(t => t.WE_WE_OriginalDocketLineForRating));
			receiveQuery.AddSubQuery(receiveLineQuery, JoinCondition.And);
			Factory.AddFetchHint(WhsDocketSchema.Instance, receiveQuery);
		}

		#endregion
	}
}
