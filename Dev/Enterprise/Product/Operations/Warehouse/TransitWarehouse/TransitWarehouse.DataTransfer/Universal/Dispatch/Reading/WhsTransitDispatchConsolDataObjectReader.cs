using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static CollectionChangedEventArgs;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateBusinessObjectFinderForDLL;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchConsolDataObjectReader : WhsTransitDataObjectReader<WhsTransitDispatchConsol>
	{
		public WhsTransitDispatchConsolDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger,
			UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			RemovedPackageStates = new HashSet<WhsItemPackageState>();
			DetachedLoadListPKs = new HashSet<ZGuid>();
			ReassignedPackageStatePKs = new HashSet<ZGuid>();
		}

		HashSet<WhsItemPackageState> RemovedPackageStates { get; }
		HashSet<ZGuid> DetachedLoadListPKs { get; }
		HashSet<ZGuid> ReassignedPackageStatePKs { get; }

		IColumnIndexer Warehouse
		{
			get
			{
				warehouse ??= WarehouseMatchingHelper.GetWarehouse(dataObject, factory, logger);
				if (warehouse == null)
				{
					WarehouseMatchingHelper.ThrowForNoMatchingWarehouse(dataObject, logger);
				}
				return warehouse;
			}
		}
		IColumnIndexer warehouse;

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitDispatchConsol; }
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsTransitDispatchConsol targetBO)
		{
			RegisterHandler();
			var subShipmentDOs = dataObject.SubShipmentCollection;
			if (subShipmentDOs == null || !subShipmentDOs.Any())
			{
				throw new DataObjectReadFailureException(Res.GetString("2631c2c9-898b-43c0-864f-3cb2c30d02c6", "No Dispatch Consignments (e.g. Forwarding Shipments) were included in the Dispatch Instruction."));
			}

			var runSheetDataSource = dataObject.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheet);
			if (runSheetDataSource != null && runSheetDataSource.Key.HasValue && !runSheetDataSource.Key.Value.IsEmpty && (!dataObject.VoyageFlightNo.HasValue || dataObject.VoyageFlightNo.Value.IsEmpty))
			{
				throw new DataObjectReadFailureException(Res.GetString("b3f2a430-4981-4188-a858-f2d041a88396", "Run Sheet vehicle number must be populated in UXML."));
			}

			WhsTransitLogHelper.LogRecipientRole(dataObject, logger);
			var branchPK = Warehouse.GetValue(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);
			var subShipmentsToCreateDCN = TransitUniversalHelper.GetShipmentsToCreateConsignments(subShipmentDOs, branchPK.ToGuid());

			var houseBills = subShipmentsToCreateDCN.Where(p => !string.IsNullOrEmpty(p.WayBillNumber)).ToLookup(p => p.WayBillNumber.GetValueOrDefault().ToUpper());
			var duplicatedHSBs = houseBills.Where(g => g.Count() > 1).Select(g => g.Key);

			if (duplicatedHSBs.Any())
			{
				throw new DataObjectReadFailureException(Res.GetString("3e20b9f0-e1a9-41ea-a9cf-9ab7e90cf3e1", "Duplicate Waybill Numbers found in the Dispatch Instruction: {0}.", string.Join(", ", duplicatedHSBs)));
			}

			var dcnWarningInfo = new Dictionary<PackingLine, DispatchInstructionWarningInfo>();
			var dispatchConsignments = PopulateDispatchConsignments(targetBO, Warehouse, subShipmentsToCreateDCN, dcnWarningInfo);
			var containerLoadLists = PopulateLoadListsForContainers(subShipmentsToCreateDCN, targetBO, dispatchConsignments, dcnWarningInfo, branchPK);
			PopulateLoadListsForVehicle(targetBO, dispatchConsignments);

			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var consolNumber = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key ?? ZString.Empty;
			var masterBill = consolDO?.WayBillNumber ?? ZString.Empty;
			ProcessUnusedLoadLists(containerLoadLists, consolNumber, masterBill);
			DeactivateEmptyLoadLists(consolNumber, masterBill);
			SetDCNNotAuthorizedByRemovedPackageStates();

			var dispatchConsignmentRows = dispatchConsignments.Select(d => GetColumnIndexer(d)).ToArray();
			TransitUniversalHelper.UpdateAllowPartialLoading(factory, dispatchConsignmentRows);
			ExecuteStoreProcedure();
		}

		IEnumerable<WhsItemDispatchLoadList> PopulateLoadListsForContainers(IEnumerable<UniversalShipment> subShipmentDOs, WhsTransitDispatchConsol targetBO, IReadOnlyCollection<WhsItemDispatchConsignment> dispatchConsignments, Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo, ZGuid branchPK)
		{
			var subShipmentsToCreateDCN = TransitUniversalHelper.GetShipmentsToCreateConsignments(subShipmentDOs, branchPK.ToGuid());
			var allPackLines = subShipmentsToCreateDCN.Where(s => s.PackingLineCollection != null).SelectMany(s => s.PackingLineCollection).ToArray();
			var allPackLinesHavePackageIds = allPackLines.All(p => p.ReferenceNumber.HasValue && !string.IsNullOrEmpty(p.ReferenceNumber.Value) && p.PackQty.HasValue && p.PackQty.Value == 1);

			var containerCollection = dataObject.ContainerCollection?.Where(c => c.ContainerNumber.HasValue).ToArray() ?? Array.Empty<Container>();

			var populatedLoadLists = new List<WhsItemDispatchLoadList>();

			if (allPackLinesHavePackageIds && containerCollection.Any() && !WarehouseDataRegistry.Instance.CreateSingleDLLForAllContainers.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), branchPK.ToGuid(), Guid.Empty))
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString("46a7de1a-63c7-4012-b8d4-1a315c0411dc", "Attempting to create Container Loading Plans as Containers have been specified."));

#if DEBUG
				targetBO.PopulatedDispatchTransportationUnitsForTesting = new List<WhsItemDispatchTransportationUnit>();
#endif

				var packLinesGroupByContainerLink = allPackLines.GroupBy(p => p.ContainerLink).ToArray();
				var containerGroupedByContainerLinks = packLinesGroupByContainerLink.Where(g => g.Key.HasValue).Distinct().ToArray();

				var noContainerPackLine = packLinesGroupByContainerLink.SingleOrDefault(g => !g.Key.HasValue);

				var canAllPackagesAssignedToContainers = containerGroupedByContainerLinks.Length < containerCollection.Length || noContainerPackLine == null;
				if (canAllPackagesAssignedToContainers)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("3069b363-49f2-4871-b45d-09d6c89c0d1d", "Creating {0} Load List(s) for Containers with a Loading Plan.", containerGroupedByContainerLinks.Length));

					foreach (var containerGroupedByContainerLink in containerGroupedByContainerLinks)
					{
						var container = containerCollection.Single(c => c.Link == containerGroupedByContainerLink.Key);
						var packLines = containerGroupedByContainerLink.ToArray();

						var finderForDLL = new WhsTransitPackageStateBusinessObjectFinderForDLL(factory, dispatchConsignments, packLines, FindOption.ForContainer);

						CreateDispatchInstruction(targetBO, packLines, new[] { container }, populatedLoadLists, finderForDLL);
					}

					var assignedContainerLinks = containerGroupedByContainerLinks.Select(c => c.Key).ToHashSet();
					var containerLinks = containerCollection.Select(c => c.Link).ToHashSet();

					var unAssignedContainerLinks = containerLinks.Except(assignedContainerLinks);

					if (noContainerPackLine != null)
					{
						logger.Log(LogType.Information, ResString.GetMultilingualString("8bb72730-b5a7-4d7f-8838-2494fd044392", "Creating a Load List for remaining Containers and unallocated Packages."));
						var unAssignedContainers = containerCollection.Where(c => unAssignedContainerLinks.Contains(c.Link)).ToList();
						var packLines = noContainerPackLine.ToArray();

						var finderForDLL = new WhsTransitPackageStateBusinessObjectFinderForDLL(factory, dispatchConsignments, packLines, FindOption.ForContainer);
						CreateDispatchInstruction(targetBO, packLines, unAssignedContainers, populatedLoadLists, finderForDLL);
					}
				}
				else
				{
					throw new DataObjectReadFailureException(Res.GetString("36596008-1250-44ce-a0e7-698f66e87de1",
			@"Cannot determine Loading Plan for Packages {0} as they have not been allocated to any Container.
If all Containers have a Loading Plan(allocated Packages), then all Packages are required to be allocated to a Container.", ZString.Join(", ", noContainerPackLine.Select(p => p.ReferenceNumber.Value).ToArray())));
				}
			}
			else if (subShipmentDOs != null && subShipmentDOs.Any())
			{
				var finderForDLL = new WhsTransitPackageStateBusinessObjectFinderForDLL(factory, dcns: dispatchConsignments);
				var packageStates = finderForDLL.Find().ToArray();

				if (packageStates.Any())
				{
					ReassignedPackageStatePKs.UnionWith(packageStates.Select(p => p.PK));
				}

				var loadList = MatchOrCreateDispatchLoadList(targetBO, containerCollection, allPackLines, populatedLoadLists, finderForDLL, dcnWarningInfo);

				CreateDispatchTransportationUnits(targetBO, loadList);
			}

			return populatedLoadLists;
		}

		void PopulateLoadListsForVehicle(WhsTransitDispatchConsol targetBO, IReadOnlyCollection<WhsItemDispatchConsignment> dispatchConsignments)
		{
			var vehicle = dataObject.GetVehicle();
			if (vehicle != null)
			{
				var bookingConfirmationReference = dataObject.GetBookingConfirmationReference();
				if (!bookingConfirmationReference.HasValue || bookingConfirmationReference.Value.IsEmpty)
				{
					throw new DataObjectReadFailureException(Res.GetString("a617410a-8b84-4efb-9402-2f5c613b307c", "Booking Confirmation Reference is missing from UXML."));
				}

				var firstSubShipmentFromGateBooking = dataObject.GetFirstSubShipmentFromGateBooking() ?? throw new DataObjectReadFailureException(Res.GetString("0dbb2b55-817b-40f9-884c-acecbf190ae5", "Gate Movement Booking is not provided in UXML."));

				var pkgStatesToLoad = GetPkgStatesFromBookingConfirmationReferenceMatching(bookingConfirmationReference.Value, dispatchConsignments, out ZString? masterBill);
				if (pkgStatesToLoad == null || pkgStatesToLoad.Length == 0)
				{
					var consignmentPKs = dispatchConsignments.Select(c => c.PK).ToArray();
					pkgStatesToLoad = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, consignmentPKs));
				}
				if (pkgStatesToLoad.Length == 0)
				{
					throw new DataObjectReadFailureException(Res.GetString("8da9cab4-40dd-43b2-a383-78ed80454769", "Could not find any package state using Container Number, House Bill and Master Bill."));
				}

				var originalDllPKs = pkgStatesToLoad.Select(ps => ps.WPS_WDL_LoadList).Where(pk => !pk.IsEmpty).Distinct().ToList();
				var containerNumbersInDO = dataObject.GetContainerNumbers();

				var isLoosePackagesBooking = containerNumbersInDO.Count == 0;
				if (isLoosePackagesBooking)
				{
					var containerByDLLDTOs = DTUColumnIndexerHelper.GetContainerByDLLDTOByLoadListPKs(factory, originalDllPKs);
					var packingLines = dataObject.GetPackingLines();

					var finderForDLL = new WhsTransitPackageStateBusinessObjectFinderForDLL(dataObject, factory, pkgStatesToLoad, containerByDLLDTOs, masterBill, findOption: FindOption.ForGate);
					var packageStatesForDLL = finderForDLL.Find().ToArray();

					var dllPKsFromContainer = containerByDLLDTOs.Where(c => !c.containerDTOs.Any() || c.containerDTOs.All(container => container.DTU.WDH_UnitType == TransportUnitTypes.Vehicle)).Select(c => c.dllPK).ToList();
					var additionalReferences = dllPKsFromContainer.SelectMany(pk => AdditionalReferenceColumnIndexerHelper.GetAdditionalReferencesByParent(factory, pk));

					var vehicleLoadList = MatchOrCreateDispatchLoadList(targetBO, new List<Container>(), packingLines, new List<WhsItemDispatchLoadList>(), finderForDLL, masterBill: masterBill, existingAdditionalReferences: additionalReferences);
					ImportVehicle(targetBO, vehicleLoadList, Warehouse, vehicle);

					if (containerByDLLDTOs != null && containerByDLLDTOs.Count > 0)
					{
						var filteredContainerByDLLDTO = containerByDLLDTOs.Where(c => !c.containerDTOs.Any() || c.containerDTOs.All(container => container.DTU.WDH_UnitType == TransportUnitTypes.Vehicle));
						if (packageStatesForDLL.Any())
						{
							ReassignedPackageStatePKs.UnionWith(packageStatesForDLL.Select(p => p.PK));
						}
						// Exclude the vehicle loadlist
						filteredContainerByDLLDTO = filteredContainerByDLLDTO.Where(dll => dll.dllPK != vehicleLoadList.PK).ToArray();
						DetachedLoadListPKs.UnionWith(filteredContainerByDLLDTO.Select(dll => dll.dllPK));
					}
				}
				else
				{
					HandleContainerBooking(targetBO, vehicle, masterBill, containerNumbersInDO, pkgStatesToLoad, originalDllPKs);
				}
			}
		}

		void HandleContainerBooking(WhsTransitDispatchConsol targetBO, Vehicle vehicle, ZString? masterBill, HashSet<ZString> containerNumbersInDO,
										WhsItemPackageState[] pkgStatesToLoad, List<ZGuid> originalDllPKs)
		{
			var shouldHandleBlindContainers = ShouldHandleBlindContainers(originalDllPKs, factory);
			var isBlindContainerPackagesAndPackageStatesToLoadCountMatch = false;

			if (shouldHandleBlindContainers)
			{
				var latestDllPK = GetLatestDllPK(factory, originalDllPKs);
				var finderForDLL = new WhsTransitPackageStateBusinessObjectFinderForDLL(dataObject, new UniversalObjectFactory(factory.BOFactory), pkgStatesToLoad, null, masterBill, latestDllPK);
				var packageStatesForDLL = finderForDLL.Find().ToArray();

				var packagePKs = packageStatesForDLL.Select(p => p.WPS_KP_Package);
				var packagesQuery = new ZQuery(PkgPackageSchema.PK, packagePKs);
				var packages = factory.BOFactory.Load<PkgPackage>(packagesQuery);
				var totalDLLPackageQty = packages.Sum(p => p.KP_PackageQty);

				var container = dataObject.GetContainers().FirstOrDefault();
				var containerLink = container.Link;
				var containerPackageQuantity = dataObject.GetPackingLines().Where(p => p.ContainerLink == containerLink).Sum(p => p.PackQty);

				isBlindContainerPackagesAndPackageStatesToLoadCountMatch = totalDLLPackageQty == containerPackageQuantity;

				if (isBlindContainerPackagesAndPackageStatesToLoadCountMatch)
				{
					var bookingParty = TransitUniversalHelper.GetBookingParty(factory, logger);

					var loadListReader = new WhsItemDispatchLoadListDataObjectReader(Warehouse, dataObject, new Container[] { container }, new List<PackingLine>(), bookingParty, logger, factory, new List<WhsItemDispatchLoadList>(), finderForDLL: finderForDLL, handler: HandleCollectionChanged);
					var loadList = loadListReader.ReadIntoBusinessObject();

					var dtuReader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(GetColumnIndexerFromRow(loadList), Warehouse, container, logger, factory, new List<WhsItemDispatchTransportationUnit>());
					dtuReader.ReadIntoBusinessObject();

					ReassignedPackageStatePKs.UnionWith(pkgStatesToLoad.Select(p => p.PK));
					originalDllPKs.Add(loadList.PK);
				}
				else
				{
					ReassignedPackageStatePKs.UnionWith(pkgStatesToLoad.Select(p => p.PK));

					var vehicleLoadList = MatchOrCreateDispatchLoadList(targetBO, new List<Container>(), new List<PackingLine>(), new List<WhsItemDispatchLoadList>(), finderForDLL, masterBill: masterBill);
					ImportVehicle(targetBO, vehicleLoadList, Warehouse, vehicle);

					var dtuReader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(GetColumnIndexerFromRow(vehicleLoadList), Warehouse, container, logger, factory, new List<WhsItemDispatchTransportationUnit>(), false);
					var containerDTU = dtuReader.ReadIntoBusinessObject();

					var filteredContainerNumbers = new HashSet<(ZString containerNumber, ZGuid rtuPK)>
					{
						(container.ContainerNumber ?? "", containerDTU.PK)
					};
					CreateContainerUniversalLinkToGateMovementBooking(dataObject, factory, logger, filteredContainerNumbers);

					if (latestDllPK != null)
					{
						DetachedLoadListPKs.UnionWith(originalDllPKs);
					}
				}
			}

			if (!shouldHandleBlindContainers || isBlindContainerPackagesAndPackageStatesToLoadCountMatch)
			{
				LinkContainerDTUAsPackageStateToVehicleDll(targetBO, vehicle, masterBill, containerNumbersInDO, originalDllPKs, isBlindContainerPackagesAndPackageStatesToLoadCountMatch);
			}
		}

		void LinkContainerDTUAsPackageStateToVehicleDll(
			WhsTransitDispatchConsol targetBO,
			Vehicle vehicle,
			ZString? masterBill,
			HashSet<ZString> containerNumbersInDO,
			List<ZGuid> originalDllPKs,
			bool isBlindContainerPackagesAndPackageStatesToArriveCountMatch)
		{
			var filteredLoadListPKs = new HashSet<ZGuid>();
			var containerByDLLDTOs = DTUColumnIndexerHelper.GetContainerByDLLDTOByLoadListPKs(factory, originalDllPKs);
			var finderForDLL = new WhsTransitPackageStateBusinessObjectFinderForDLL(dataObject, factory, containerByDLLDTOs, filteredLoadListPKs);
			var pacakgeStatesForDLL = finderForDLL.Find().ToArray();

			pacakgeStatesForDLL.ForEach(ps =>
			{
				var previousLoadListPK = ps.GetValue(WhsItemPackageStateSchema.WPS_WDL_LoadList);
				if (previousLoadListPK != ZGuid.Empty && !DetachedLoadListPKs.Contains(previousLoadListPK))
				{
					DetachedLoadListPKs.Add(previousLoadListPK);
				}
			});

			var containerDTOs = containerByDLLDTOs.SelectMany(c => c.containerDTOs).ToArray();
			var pacakgeStatePKsForDLL = pacakgeStatesForDLL.Select(p => p.PK).ToArray();
			var filteredContainerNumbers = new HashSet<(ZString containerNumber, ZGuid dtuPK)>();
			foreach (var containerDTO in containerDTOs)
			{
				if (pacakgeStatePKsForDLL.Contains(containerDTO.PackageState.PK))
				{
					var containerNumber = containerDTO.DTU.WDH_VehicleReference;
					var dtuPK = containerDTO.DTU.PK;
					filteredContainerNumbers.Add((containerNumber, dtuPK));
				}
			}

			ReassignedPackageStatePKs.UnionWith(pacakgeStatePKsForDLL);

			CreateContainerUniversalLinkToGateMovementBooking(dataObject, factory, logger, filteredContainerNumbers);

			var vehicleLoadList = MatchOrCreateDispatchLoadList(targetBO, new List<Container>(), new List<PackingLine>(), new List<WhsItemDispatchLoadList>(), finderForDLL: finderForDLL, masterBill: masterBill);

			if (!isBlindContainerPackagesAndPackageStatesToArriveCountMatch)
			{
				// Exclude the vehicle loadlist
				filteredLoadListPKs.Where(loadListPK => loadListPK != vehicleLoadList.PK).ForEach(loadListPK => DetachedLoadListPKs.Add(loadListPK));
			}

			ImportVehicle(targetBO, vehicleLoadList, Warehouse, vehicle);
		}

		bool ShouldHandleBlindContainers(List<ZGuid> dllPKs, UniversalObjectFactory factory)
		{
			if (!dllPKs.Any())
			{
				return true;
			}

			var pivotQuery = new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, dllPKs);
			var dllDtuPivots = Array.ConvertAll(factory.RowFactory.Load(WhsItemDispatchLoadListDTUPivotSchema.Constants.TableName, pivotQuery), DataObjectReader.GetColumnIndexerFromRow);

			return dllDtuPivots.Length == 0;
		}

		ZGuid? GetLatestDllPK(UniversalObjectFactory factory, List<ZGuid> dllPKs)
		{
			if (!dllPKs.Any())
			{
				return null;
			}

			var dllQuery = new ZQuery(WhsItemDispatchLoadListSchema.PK, dllPKs);
			dllQuery.OrderBy = WhsItemDispatchLoadListSchema.Constants.WDL_SystemCreateTimeUtc + OrderByClause.Descending;
			var latestDll = Array.ConvertAll(factory.RowFactory.Load(WhsItemDispatchLoadListSchema.Constants.TableName, dllQuery), DataObjectReader.GetColumnIndexerFromRow).First();
			return latestDll.GetValue(WhsItemDispatchLoadListSchema.PK);
		}

		void CreateContainerUniversalLinkToGateMovementBooking(UniversalShipment topLevelDO, UniversalObjectFactory factory, IXmlImportLogger logger, HashSet<(ZString containerNumber, ZGuid dtuPK)> filteredContainerNumbers)
		{
			var firstContainerNumberAndDtuPK = filteredContainerNumbers.ToList()[0];
			var query = new ZQuery(WhsItemDispatchTransportationUnitSchema.PK, firstContainerNumberAndDtuPK.dtuPK);
			var containerDTU = factory.LoadTop1<WhsItemDispatchTransportationUnit>(query);

			var firstSubShipmentFromGateBooking = topLevelDO.GetFirstSubShipmentFromGateBooking();
			if (containerDTU != null && firstSubShipmentFromGateBooking != null)
			{
				var sourceDataContext = UniversalShipment.GetSourceDataObject(firstSubShipmentFromGateBooking).DataContext;
				var linkCreator = new UniversalJobLinkCreator(containerDTU.Factory, containerDTU, null, sourceDataContext, logger, true);
				linkCreator.TryCreateJobLink(DataContextType.GateMovementBooking);

				// Log Booking Confirmed
				var gateMovementBookingNumber = firstSubShipmentFromGateBooking.GetMatchingDataSourceValue(DataContextType.GateMovementBooking) ?? ZString.Empty;
				WhsTransitLogHelper.LogBookingConfirmed(containerDTU, nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
			}
		}

		WhsItemPackageState[] GetPkgStatesFromBookingConfirmationReferenceMatching(ZString bookingConfirmationReference, IReadOnlyCollection<WhsItemDispatchConsignment> dispatchConsignments, out ZString? masterBill)
		{
			masterBill = null;
			var readonlyFactory = factory.BOFactory.GetCachedReadOnlyFactory();

			var warehouseBO = readonlyFactory.Load<WhsWarehouse>(Warehouse.GetValue(WhsWarehouseSchema.PK));
			var dtuMatcher = new TWHDTUJobMatcher(readonlyFactory, warehouseBO, bookingConfirmationReference, ReferenceNumberTypes.Unknown);
			var dcnMatcher = new TWHDCNJobMatcher(readonlyFactory, warehouseBO, bookingConfirmationReference, ReferenceNumberTypes.Unknown);
			var dllMatcher = new TWHDLLJobMatcher(readonlyFactory, warehouseBO, bookingConfirmationReference, ReferenceNumberTypes.Unknown);

			dtuMatcher.SetNextJobMatcher(dcnMatcher);
			dcnMatcher.SetNextJobMatcher(dllMatcher);

			var jobMatcherResult = dtuMatcher.Process();

			if (jobMatcherResult.ErrorCode != null)
			{
				return null;
			}

			if (jobMatcherResult.ReferenceNumberType == ReferenceNumberTypes.MasterBill)
			{
				masterBill = bookingConfirmationReference;
			}

			var pkgStateQuery = new ZQuery(WhsItemPackageStateSchema.PK, jobMatcherResult.PackageStates?.Select(ps => ps.PK).ToArray());
			return factory.BOFactory.Load<WhsItemPackageState>(pkgStateQuery).ToArray();
		}

		#region CreateDispatchLoadLists

		WhsItemDispatchLoadList MatchOrCreateDispatchLoadList(WhsTransitDispatchConsol targetBO,
			IReadOnlyCollection<Container> containers,
			IReadOnlyCollection<PackingLine> packingLines,
			List<WhsItemDispatchLoadList> alreadyMatchedLoadLists,
			WhsTransitPackageStateBusinessObjectFinderForDLL finderForDLL = null,
			Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo = null,
			ZString? masterBill = null, IEnumerable<IColumnIndexer> existingAdditionalReferences = null)
		{
			var bookingParty = TransitUniversalHelper.GetBookingParty(factory, logger);
			var dcnWarningNoteForLoadList = dcnWarningInfo == null ? string.Empty :
				GetDCNWarningNote(dcnWarningInfo.Where(p => packingLines.Contains(p.Key)).Select(d => d.Value));
			var loadListReader = new WhsItemDispatchLoadListDataObjectReader(Warehouse, dataObject, containers, packingLines, bookingParty, logger, factory, alreadyMatchedLoadLists, dcnWarningNote: dcnWarningNoteForLoadList, masterBillNumber: masterBill, existingAdditionalReferences: existingAdditionalReferences, finderForDLL: finderForDLL, handler: HandleCollectionChanged);
			var loadList = loadListReader.ReadIntoBusinessObject();
			alreadyMatchedLoadLists.Add(loadList);

#if DEBUG
			targetBO.PopulatedDispatchLoadListForTesting = loadList;
#endif

			return loadList;
		}

		string GetDCNWarningNote(IEnumerable<DispatchInstructionWarningInfo> dcnWarningInfo)
		{
			var warningFromDCNsWithPackageMatchingDiscrepancy = string.Empty;
			var warningFromDCNsCanNotBeCreated = string.Empty;

			var dcnIDsWithPackageMatchingDiscrepancy = dcnWarningInfo.SelectMany(info => info.DCNIDsWithPackageMatchingDiscrepancy).Distinct().OrderBy(id => id);
			if (dcnIDsWithPackageMatchingDiscrepancy.Any())
			{
				warningFromDCNsWithPackageMatchingDiscrepancy = Res.GetString("a26fdc26-c7c1-4e34-893a-076dc01aac03",
@"Warning :

The following DCNs were created but the Package Type and Qty is not fully matched with the RCN's packline when imported.
{0}", string.Join(System.Environment.NewLine, dcnIDsWithPackageMatchingDiscrepancy));
			}

			var dcnIDsCanNotBeCreated = dcnWarningInfo.SelectMany(info => info.DCNIDsCanNotBeCreated).Distinct().OrderBy(id => id);
			if (dcnIDsCanNotBeCreated.Any())
			{
				warningFromDCNsCanNotBeCreated = Res.GetString("7ef686c7-f142-4cb1-9935-c78585d69560",
@"Warning :

The following DCNs were not created because packages couldn't be found to attach.
{0}", string.Join(System.Environment.NewLine, dcnIDsCanNotBeCreated));
			}

			var messages = new List<string>() { warningFromDCNsWithPackageMatchingDiscrepancy, warningFromDCNsCanNotBeCreated };
			return string.Join(System.Environment.NewLine + System.Environment.NewLine, messages.Where(m => !string.IsNullOrEmpty(m)));
		}

		#endregion

		#region CreateDispatchTransportationUnits

		WhsItemDispatchLoadList CreateDispatchInstruction(WhsTransitDispatchConsol targetBO,
			IReadOnlyCollection<PackingLine> packLines,
			IReadOnlyCollection<Container> containers,
			List<WhsItemDispatchLoadList> alreadyMatchedLoadLists,
			WhsTransitPackageStateBusinessObjectFinderForDLL finderForDLL = null)
		{
			var packageStates = finderForDLL.Find().ToArray();
			if (packageStates.Any())
			{
				ReassignedPackageStatePKs.UnionWith(packageStates.Select(p => p.PK));
			}

			var dispatchLoadList = MatchOrCreateDispatchLoadList(targetBO, containers, packLines, alreadyMatchedLoadLists, finderForDLL);

			var warehouse = factory.RowFactory.LoadFromPK(WhsWarehouseSchema.Constants.TableName, dispatchLoadList.GetValue(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse));

			var loadListPackages = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WDL_LoadList, dispatchLoadList.GetValue(WhsItemDispatchLoadListSchema.PK))).ToArray();
			var loadCompleteDTUPKs = TransitUniversalHelper.GetLoadCompleteDTUs(loadListPackages, factory)
				.Select(d => GetColumnIndexerFromRow(d).GetValue(WhsItemDispatchTransportationUnitSchema.PK))
				.ToHashSet();
			var isAllPackagesDeparted = loadListPackages.Any() && loadListPackages.All(p => p.WPS_Status.ToString().IsPackageDeparted() || loadCompleteDTUPKs.Contains(p.GetValue(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader)));
			if (!isAllPackagesDeparted)
			{
				ImportContainers(targetBO, containers, dispatchLoadList, warehouse);
			}

			return dispatchLoadList;
		}

		void CreateDispatchTransportationUnits(WhsTransitDispatchConsol targetBO, WhsItemDispatchLoadList loadlist)
		{
			var containerCollection = dataObject.ContainerCollection?.Where(c => c.ContainerNumber.HasValue).ToArray() ?? Array.Empty<Container>();

			if (containerCollection.Any())
			{
				var warehouse = factory.RowFactory.LoadFromPK(WhsWarehouseSchema.Constants.TableName, loadlist.GetValue(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse));
#if DEBUG
				targetBO.PopulatedDispatchTransportationUnitsForTesting = new List<WhsItemDispatchTransportationUnit>();
#endif
				ImportContainers(targetBO, containerCollection, loadlist, warehouse);
			}
			else
			{
				var runSheetDataSource = dataObject.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheet);
				if (runSheetDataSource != null && runSheetDataSource.Key.HasValue && !runSheetDataSource.Key.Value.IsEmpty && dataObject.VoyageFlightNo.HasValue && !dataObject.VoyageFlightNo.Value.IsEmpty)
				{
					var warehouse = factory.RowFactory.LoadFromPK(WhsWarehouseSchema.Constants.TableName, loadlist.GetValue(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse));
					var dispatchLoadListRow = GetColumnIndexerFromRow(loadlist);

					var dispatchHeaderReader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(dispatchLoadListRow, GetColumnIndexerFromRow(warehouse), dataObject, logger, factory);
					dispatchHeaderReader.ReadIntoBusinessObject();
				}
			}
		}

		void ImportContainers(WhsTransitDispatchConsol targetBO, IReadOnlyCollection<Container> containers, WhsItemDispatchLoadList dispatchLoadList, System.Data.DataRow warehouse)
		{
			var listOfContainersAlreadyMatched = new List<WhsItemDispatchTransportationUnit>();

			foreach (var container in containers)
			{
				var numberOfContainersToCreate = container.ContainerCount ?? 1;
				container.ContainerCount = 1;
				for (int count = 0; count < numberOfContainersToCreate; count++)
				{
					var dtuReader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(GetColumnIndexerFromRow(dispatchLoadList), GetColumnIndexerFromRow(warehouse), container, logger, factory, listOfContainersAlreadyMatched);
					var dtu = dtuReader.ReadIntoBusinessObject();
					listOfContainersAlreadyMatched.Add(dtu);
#if DEBUG
					targetBO.PopulatedDispatchTransportationUnitsForTesting.Add(dtu);
#endif
				}
			}
		}

		void ImportVehicle(WhsTransitDispatchConsol targetBO, WhsItemDispatchLoadList dispatchLoadList, IColumnIndexer warehouse, Vehicle vehicle)
		{
			if (vehicle != null)
			{
				var vehicleReader = new WhsItemDispatchTransportationUnitDataObjectReaderForTruck(GetColumnIndexerFromRow(dispatchLoadList), warehouse, dataObject, logger, factory);
				vehicleReader.ReadIntoBusinessObject();
			}
		}

		#endregion

		#region CreateDispatchConsignments

		IReadOnlyCollection<WhsItemDispatchConsignment> PopulateDispatchConsignments(WhsTransitDispatchConsol targetBO, IColumnIndexer warehouseFromConsol, IEnumerable<UniversalShipment> subShipments, Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo)
		{
			var result = new List<WhsItemDispatchConsignment>();

			var shipmentExceptions = new List<(UniversalShipment, DataObjectReadFailureException)>();
			foreach (var shipment in subShipments)
			{
				try
				{
					var fromTWPConsol = dataObject.HasRecipientRoleAndService(RecipientRoleType.ATW, ServiceCodeType.TWP) ||
										dataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWP);
					var reader = new WhsTransitDispatchConsignmentDataObjectReader(shipment, logger, factory, warehouseFromConsol, false, dcnWarningInfo: dcnWarningInfo, fromTWPConsol: fromTWPConsol);
					result.Add(reader.ReadIntoBusinessObject());
				}
				catch (DataObjectReadFailureException ex)
				{
					shipmentExceptions.Add((shipment, ex));
					logger.Log(Enterprise.Integration.LogType.Error, ex.Message);
				}
			}
#if DEBUG
			targetBO.PopulatedConsignmentsForTesting = result;
#endif
			TransitUniversalHelper.ThrowIfShipmentsHaveExceptions(dataObject, shipmentExceptions);

			return result.ToArray();
		}

		#endregion

		#region ProcessUnusedLoadLists

		void ProcessUnusedLoadLists(IEnumerable<WhsItemDispatchLoadList> populatedLoadLists, ZString consolNumber, ZString masterBill)
		{
			var unusedLoadLists = Enumerable.Empty<WhsItemDispatchLoadList>();
			if (!consolNumber.IsEmpty)
			{
				unusedLoadLists = GetMatchedButUnusedLoadListsFromConsolNumber(populatedLoadLists, consolNumber);
			}
			else if (!masterBill.IsEmpty)
			{
				unusedLoadLists = GetMatchedButUnusedLoadListsFromMasterBill(populatedLoadLists, masterBill);
			}

			DetachLoadLists(unusedLoadLists);
		}

		IEnumerable<WhsItemDispatchLoadList> GetMatchedButUnusedLoadListsFromConsolNumber(IEnumerable<WhsItemDispatchLoadList> populatedLoadLists, string consolNumber)
		{
			return GetMatchedButUnusedLoadListsUsingAdditionalReference(consolNumber, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, populatedLoadLists);
		}

		IEnumerable<WhsItemDispatchLoadList> GetMatchedButUnusedLoadListsFromMasterBill(IEnumerable<WhsItemDispatchLoadList> populatedLoadLists, string masterBill)
		{
			return GetMatchedButUnusedLoadListsUsingAdditionalReference(masterBill, AdditionalReferenceTypes.Codes.MasterBill, populatedLoadLists);
		}

		IEnumerable<WhsItemDispatchLoadList> GetMatchedButUnusedLoadListsUsingAdditionalReference(string referenceNumber, string referenceType, IEnumerable<WhsItemDispatchLoadList> populatedLoadLists)
		{
			var loadListQuery = TransitUniversalHelper.GetLoadListsMatchingAdditionalReferenceQuery(referenceNumber, referenceType, new[] { Warehouse.GetValue(WhsWarehouseSchema.PK) }, populatedLoadLists.Select(l => l.PK).ToArray());
			var matchedLoadListRows = factory.BOFactory.Load<WhsItemDispatchLoadList>(loadListQuery).ToArray();
			return matchedLoadListRows;
		}

		void DetachLoadLists(IEnumerable<WhsItemDispatchLoadList> loadLists)
		{
			if (loadLists.Any())
			{
				var loadListPKs = loadLists.Select(l => l.PK).ToArray();
				DetachPackagesFromUnusedLoadLists(loadLists, loadListPKs);
				DetachedLoadListPKs.UnionWith(loadListPKs);
			}
		}

		void DetachPackagesFromUnusedLoadLists(IEnumerable<WhsItemDispatchLoadList> unusedLoadListRows, IEnumerable<ZGuid> unusedLoadListPKs)
		{
			var query = GetPackageStateToDetachQuery(unusedLoadListPKs, ReassignedPackageStatePKs);
			var packageStates = factory.BOFactory.Load<WhsItemPackageState>(query);
			ReassignedPackageStatePKs.UnionWith(packageStates.Select(p => p.PK).ToArray());

			var groupedPackageStateRows = packageStates.ToKeyListDictionary(p => p.GetValue(WhsItemPackageStateSchema.WPS_WDL_LoadList));
			foreach (var unusedLoadListRow in unusedLoadListRows)
			{
				if (groupedPackageStateRows.TryGetValue(unusedLoadListRow.GetValue(WhsItemDispatchLoadListSchema.PK), out var packageStatesToDetach))
				{
					TransitUniversalHelper.RemovePackagesFromLoadList(packageStatesToDetach, unusedLoadListRow, dataObject, factory, logger);
				}
			}

			// updating rows where the BizO is already loaded in the same factory causes issues
			// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
			packageStates.ForEach(p =>
			{
				p.HasChanges = true;
			});
		}

		void SetDCNNotAuthorizedByRemovedPackageStates()
		{
			var dcnsRemovedFromLoadListPKs = RemovedPackageStates.Select(p => p.WPS_WDC_TransitDispatchConsignment).Distinct();
			var dcnsRemovedFromLoadListQuery = new ZQuery(WhsItemDispatchConsignmentSchema.PK, dcnsRemovedFromLoadListPKs);
			var dcnsRemovedFromLoadList = factory.BOFactory.Load<WhsItemDispatchConsignment>(dcnsRemovedFromLoadListQuery);

			foreach (var dcnRemovedFromLoadList in dcnsRemovedFromLoadList)
			{
				var dcnPackagesQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, dcnRemovedFromLoadList.PK);
				var dcnPackages = factory.BOFactory.Load<WhsItemPackageState>(dcnPackagesQuery).ToArray();
				var allPackagesOnDCNRemovedFromLoadList = !dcnPackages.Except(RemovedPackageStates).Any();
				if (allPackagesOnDCNRemovedFromLoadList)
				{
					dcnRemovedFromLoadList.WDC_IsAuthorizedForDispatch = false;
				}
			}
		}

		static ZDBOnlyQuery GetPackageStateToDetachQuery(IEnumerable<ZGuid> unusedLoadListPKs, IEnumerable<ZGuid> importedPackageStatePKs)
		{
			// Exclude package states that are on a Load Complete DTU ('not in completeDTUSubQuery' will exclude packages with no DTU)
			var packageStateQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));

			var packageStateWithoutDTUSubQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.PK);
			packageStateWithoutDTUSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDL_LoadList, unusedLoadListPKs);
			packageStateWithoutDTUSubQuery.AddToFilter(WhsItemPackageStateSchema.PK, SQLComparisonOperator.NotEqual, importedPackageStatePKs);
			packageStateWithoutDTUSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader, null);

			var packageStateOnIncompleteDTUSubQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.PK);
			packageStateOnIncompleteDTUSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDL_LoadList, unusedLoadListPKs);
			packageStateOnIncompleteDTUSubQuery.AddToFilter(WhsItemPackageStateSchema.PK, SQLComparisonOperator.NotEqual, importedPackageStatePKs);

			var incompleteDTUSubQuery = new ZDBOnlySubQuery(typeof(WhsItemDispatchTransportationUnit), WhsItemDispatchTransportationUnitSchema.PK);
			incompleteDTUSubQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_LoadCompleteTime, ZDateTimeOffset.Empty);
			packageStateOnIncompleteDTUSubQuery.AddSubQuery(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader, incompleteDTUSubQuery, JoinCondition.And);

			packageStateQuery.AddSubQuery(WhsItemPackageStateSchema.PK, packageStateWithoutDTUSubQuery, JoinCondition.And);
			packageStateQuery.AddSubQuery(WhsItemPackageStateSchema.PK, packageStateOnIncompleteDTUSubQuery, JoinCondition.Or);

			return packageStateQuery;
		}

		void DeactivateEmptyLoadLists(ZString consolNumber, ZString masterBill)
		{
			if (DetachedLoadListPKs.Count > 0)
			{
				// The DB is queried for load lists with remaining packages, ignoring packages we've detached.
				var loadListRowsToDeactivate = GetLoadListsToDeactivate(DetachedLoadListPKs.ToArray(), ReassignedPackageStatePKs.ToArray(), factory);

				if (loadListRowsToDeactivate.Any())
				{
					logger.Log(LogType.Information, Res.GetString("4bf75b2a-2b47-47c3-90e7-f4910159cdd4",
						"Deactivating the following Load Lists as all their Packages were reassigned: {0}",
						ZString.Join(", ", loadListRowsToDeactivate.Select(l => l.GetValue(WhsItemDispatchLoadListSchema.WDL_JobID)).ToArray())));

					DeleteLoadListDTUPivotAndDTUByLoadLists(loadListRowsToDeactivate, consolNumber, masterBill);

					foreach (var loadListRow in loadListRowsToDeactivate)
					{
						SetValue(loadListRow, WhsItemDispatchLoadListSchema.WDL_IsActive, ZBool.False);
						SetValue(loadListRow, WhsItemDispatchLoadListSchema.WDL_CompleteTime, ZDateTimeOffset.Empty);
					}
				}

				// updating rows where the BizO is already loaded in the same factory causes issues
				// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
				var loadLists = factory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, DetachedLoadListPKs));
				loadLists.ForEach(l =>
				{
					l.HasChanges = true;
				});
			}
		}

		static IEnumerable<IColumnIndexer> GetLoadListsToDeactivate(ZGuid[] detachedLoadListPKs, ZGuid[] detachedPackageStatePKs, UniversalObjectFactory factory)
		{
			// Get load lists with packages excluding ones we've detached during the import.
			var sqlText =
@"SELECT DISTINCT WPS_WDL_LoadList
FROM dbo.WhsItemPackageState
WHERE WPS_WDL_LoadList IN (SELECT Value FROM @LoadListPKs) AND WPS_PK NOT IN (SELECT Value FROM @PackageStatesToIgnore)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@LoadListPKs", detachedLoadListPKs, WhsItemPackageStateSchema.WPS_WDL_LoadList, isTableValued: true));
			sqlParams.Add(ZSqlParameter.New("@PackageStatesToIgnore", detachedPackageStatePKs, WhsItemPackageStateSchema.PK, isTableValued: true));
			var nonEmptyLoadLists = new DynamicBusinessObjectCollection(factory.BOFactory);
			nonEmptyLoadLists.Load(sqlText, sqlParams);
			var nonEmptyLoadListPKs = nonEmptyLoadLists.Select(d => (ZGuid)d[WhsItemPackageStateSchema.Constants.WPS_WDL_LoadList]).ToArray();

			// Get the load lists with no remaining packages after the import
			var emptyLoadListPKs = detachedLoadListPKs.Except(nonEmptyLoadListPKs);

			return factory.RowFactory.Load(WhsItemDispatchLoadListSchema.Constants.TableName, new ZQuery(WhsItemDispatchLoadListSchema.PK, emptyLoadListPKs))
				.Select(l => GetColumnIndexerFromRow(l))
				.Where(l => l.GetValue(WhsItemDispatchLoadListSchema.WDL_IsActive))
				.ToArray();
		}

		void DeleteLoadListDTUPivotAndDTUByLoadLists(IEnumerable<IColumnIndexer> loadListRowsToDeactivate, ZString consolNumber, ZString masterBill)
		{
			var consolReference = consolNumber.IsEmpty ? masterBill : consolNumber;
			LogDeactivatedLoadList(loadListRowsToDeactivate, consolReference);

			var pivots = factory.RowFactory
				.Load(WhsItemDispatchLoadListDTUPivotSchema.Constants.TableName, new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, loadListRowsToDeactivate.Select(l => l.GetValue(WhsItemDispatchLoadListSchema.PK))))
				.Select(GetColumnIndexerFromRow)
				.ToArray();

			var dtuPKs = pivots.Select(l => l.GetValue(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit));
			var dtuPKsHasAPivot = factory.RowFactory.Load(WhsItemDispatchLoadListDTUPivotSchema.Constants.TableName, new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, dtuPKs))
				.Select(l => GetColumnIndexerFromRow(l).GetValue(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit))
				.GroupBy(l => l)
				.Select(l => new { l.Key, Count = l.Count() })
				.Where(l => l.Count == 1)
				.Select(l => l.Key)
				.ToArray();
			var dtuPKsHasAPivotAndPackages = factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader, dtuPKsHasAPivot))
				.Select(l => GetColumnIndexerFromRow(l).GetValue(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader))
				.ToArray();
			var dtuHasAPivotWithoutPackages = factory.RowFactory.Load(WhsItemDispatchTransportationUnitSchema.Constants.TableName, new ZQuery(WhsItemDispatchTransportationUnitSchema.PK, dtuPKsHasAPivot.Except(dtuPKsHasAPivotAndPackages)))
				.Select(GetColumnIndexerFromRow)
				.Where(l => l.GetValue(WhsItemDispatchTransportationUnitSchema.WDH_LoadStartTime).IsEmpty)
				.ToArray();
			LogRemovedDTU(dtuHasAPivotWithoutPackages, consolReference);

			var packageExtensions = factory.RowFactory.Load(PkgPackageExtensionSchema.Constants.TableName, new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, dtuHasAPivotWithoutPackages.Select(l => l.GetValue(WhsItemDispatchTransportationUnitSchema.PK))))
				.Select(GetColumnIndexerFromRow)
				.ToArray();

			var packages = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.PK, packageExtensions.Select(l => l.GetValue(PkgPackageExtensionSchema.KPN_KP_Package))))
				.Select(GetColumnIndexerFromRow)
				.ToArray();

			var packageContainers = factory.RowFactory.Load(PkgPackageContainerSchema.Constants.TableName, new ZQuery(PkgPackageContainerSchema.K0_KP_Package, packages.Select(l => l.GetValue(PkgPackageSchema.PK))))
				.Select(GetColumnIndexerFromRow)
				.ToArray();

			var packageStates = factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packages.Select(l => l.GetValue(PkgPackageSchema.PK))))
				.Select(GetColumnIndexerFromRow)
				.ToArray();

			var packageHeaders = factory.RowFactory.Load(PkgPackageHeaderSchema.Constants.TableName, new ZQuery(PkgPackageHeaderSchema.PK, packages.Select(l => l.GetValue(PkgPackageSchema.KP_KPH_PackageHeader))))
				.Select(GetColumnIndexerFromRow)
				.ToArray();

			pivots.ForEach(p => factory.DeleteRowAndSetHasChanges<WhsItemDispatchLoadListDTUPivot>(p, WhsItemDispatchLoadListDTUPivotSchema.PK));
			packageExtensions.ForEach(p => factory.DeleteRowAndSetHasChanges<PkgPackageExtension>(p, PkgPackageExtensionSchema.PK));
			dtuHasAPivotWithoutPackages.ForEach(d => factory.DeleteRowAndSetHasChanges<WhsItemDispatchTransportationUnit>(d, WhsItemDispatchTransportationUnitSchema.PK));
			packageContainers.ForEach(p => factory.DeleteRowAndSetHasChanges<PkgPackageContainer>(p, PkgPackageContainerSchema.PK));
			packageStates.ForEach(p => factory.DeleteRowAndSetHasChanges<WhsItemPackageState>(p, WhsItemPackageStateSchema.PK));
			packages.ForEach(p => factory.DeleteRowAndSetHasChanges<PkgPackage>(p, PkgPackageSchema.PK));
			packageHeaders.ForEach(p => factory.DeleteRowAndSetHasChanges<PkgPackageHeader>(p, PkgPackageHeaderSchema.PK));
		}

		void LogDeactivatedLoadList(IEnumerable<IColumnIndexer> dllRowsToDeactivate, ZString consolReference)
		{
			var matchingDlls = factory.BOFactory.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.PK, dllRowsToDeactivate.Select(l => l.GetValue(WhsItemDispatchLoadListSchema.PK))));
			var dllLogHelper = new DispatchLoadListTransitLogHelper();
			logger.Log(LogType.Information, dllLogHelper.GetTable(Res.GetString("e8c2e121-a9f0-4946-9681-fde4ca9f3042", "The following Load List(s) deactivated from Consol {0}:", consolReference), matchingDlls, TransitLogColumnIDs.LoadListColumn.LoadList));
		}

		void LogRemovedDTU(IEnumerable<IColumnIndexer> dtuRowsToDeactivate, string consolReference)
		{
			var matchingDtus = factory.BOFactory.Load<WhsItemDispatchTransportationUnit>(new ZQuery(WhsItemDispatchTransportationUnitSchema.PK, dtuRowsToDeactivate.Select(l => l.GetValue(WhsItemDispatchTransportationUnitSchema.PK))));
			var dtuLogHelper = new DispatchTransportationUnitTransitLogHelper();
			logger.Log(LogType.Information, dtuLogHelper.GetTable(Res.GetString("67472477-428a-43a3-b5f1-24fbc17f8495", "The following Dispatch Transportation Unit(s) removed from Consol {0}:", consolReference), matchingDtus, TransitLogColumnIDs.DTUColumn.DTU));
		}

		#endregion

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override WhsTransitDispatchConsol GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return new WhsTransitDispatchConsol();
		}

		#endregion

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<WhsTransitDispatchConsol> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region Log Message

		protected override void LogSuccessfullyLoadedMessage(string typeName)
		{
		}

		protected override void LogPopulatingMessage(string typeName)
		{
		}

		#endregion

		#region HookCollectionChangedEvent

		void HandleCollectionChanged(object sender, CollectionChangedEventArgs e)
		{
			switch (e.type)
			{
				case CollectionType.DetachedLoadList:
					DetachedLoadListPKs.UnionWith(e.DetachedLoadListPKs);
					break;
				case CollectionType.RemovedPackageState:
					RemovedPackageStates.UnionWith(e.RemovedPackageStates);
					break;
				case CollectionType.ReassignedPackageState:
					ReassignedPackageStatePKs.UnionWith(e.ReassignedPackageStatePKs);
					break;
				default:
					break;
			}
		}

		#endregion

		#region RegisterHandler

		protected override void RegisterHandler()
		{
			HandlerManager.Init(logger, factory, this, null);
			base.RegisterHandler();
		}

		#endregion
	}
}
