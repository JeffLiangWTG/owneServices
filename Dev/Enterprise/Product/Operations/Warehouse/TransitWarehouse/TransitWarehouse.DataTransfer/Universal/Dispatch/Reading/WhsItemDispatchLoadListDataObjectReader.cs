using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using MasterExtensions = Enterprise.MasterFiles.Business.Extensions;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemDispatchLoadListDataObjectReader : ShipmentDataObjectReader<WhsItemDispatchLoadList>
	{
		public WhsItemDispatchLoadListDataObjectReader(IColumnIndexer warehouseFromConsol, Shipment dataObject,
			IReadOnlyCollection<Container> containers,
			IReadOnlyCollection<PackingLine> packingLineDOs,
			IOrgHeader bookingParty,
			IXmlImportLogger logger,
			UniversalObjectFactory factory,
			IReadOnlyCollection<WhsItemDispatchLoadList> alreadyMatchedLoadLists,
			WhsTransitPackageStateBusinessObjectFinderForDLL finderForDLL = null,
			string dcnWarningNote = null,
			ZString? masterBillNumber = null,
			IEnumerable<IColumnIndexer> existingAdditionalReferences = null,
			EventHandler<CollectionChangedEventArgs> handler = null)
			: base(dataObject, logger, factory)
		{
			WarehouseFromConsol = Argument.NotNull(warehouseFromConsol, nameof(warehouseFromConsol));
			ContainerDOs = containers;
			PackingLineDOs = packingLineDOs;
			BookingParty = bookingParty;
			AlreadyMatchedLoadLists = alreadyMatchedLoadLists;
			this.finderForDLL = finderForDLL;
			DCNWarningNote = dcnWarningNote;
			masterBill = masterBillNumber ?? "";
			this.existingAdditionalReferences = existingAdditionalReferences ?? Enumerable.Empty<IColumnIndexer>();
			this.handler = handler;
		}

		protected override string GetBusinessObjectHumanReadableName(WhsItemDispatchLoadList dll) => dll == null ? Res.GetString("0ab8cdf9-bd97-4932-adba-a3cb6a21882b", "Dispatch Load List") : dll.HumanReadableName.ToString();

		readonly IColumnIndexer WarehouseFromConsol;
		readonly IReadOnlyCollection<Container> ContainerDOs;
		readonly IReadOnlyCollection<PackingLine> PackingLineDOs;
		readonly IOrgHeader BookingParty;
		readonly IReadOnlyCollection<WhsItemDispatchLoadList> AlreadyMatchedLoadLists;
		readonly string DCNWarningNote;
		readonly IEnumerable<IColumnIndexer> existingAdditionalReferences;
		readonly WhsTransitPackageStateBusinessObjectFinderForDLL finderForDLL;
		readonly EventHandler<CollectionChangedEventArgs> handler;

		ZString? vehicleRegistrationNumber;
		ZString? VehicleRegistrationNumber
		{
			get
			{
				if (!vehicleRegistrationNumber.HasValue)
				{
					vehicleRegistrationNumber = dataObject.GetVehicle()?.Registration?.Number;
				}
				return vehicleRegistrationNumber;
			}
		}

		ZString masterBill;
		ZString MasterBill
		{
			get
			{
				if (masterBill.IsEmpty)
				{
					var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
					masterBill = consolDO?.WayBillNumber ?? "";
				}
				return masterBill;
			}
		}

		#region CombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<WhsItemDispatchLoadList> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region LoadListMatching

		protected override WhsItemDispatchLoadList GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var consolNumber = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key ?? ZString.Empty;
			var matchingLoadLists = FindMatchingLoadListsByConsolNumberOrMasterBill(consolNumber, MasterBill);

			var isForVehicleMatching = VehicleRegistrationNumber.HasValue && !VehicleRegistrationNumber.Value.IsEmpty;
			if (isForVehicleMatching)
			{
				var filteredLoadLists = Enumerable.Empty<WhsItemDispatchLoadList>();
				if (consolNumber.IsEmpty && MasterBill.IsEmpty)
				{
					filteredLoadLists = GetMatchingLoadLists_ByVehicle();
				}
				else
				{
					filteredLoadLists = FilterMatchingLoadLists_ByVehicle(matchingLoadLists.ToArray());
				}

				if (!filteredLoadLists.Any())
				{
					var isLoosePackagesGateBooking = dataObject.GetContainerNumbers().Count == 0;
					if (isLoosePackagesGateBooking && matchingLoadLists.Any() && PackingLineDOs != null)
					{
						filteredLoadLists = FilterMatchingLoadLists_ByPacklineDOs(PackingLineDOs, matchingLoadLists.ToArray(), shouldMatchAllPackingLines: true);
					}
				}
				return filteredLoadLists.Any() ? GetMostRecentLoadlist(filteredLoadLists) : null;
			}
			else
			{
				var branchPK = WarehouseFromConsol.GetValue(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);
				return matchingLoadLists.Any() ? GetMostRecentLoadListFromMatching(matchingLoadLists.ToArray(), branchPK) : null;
			}
		}

		IEnumerable<WhsItemDispatchLoadList> FindMatchingLoadListsByConsolNumberOrMasterBill(ZString consolNumber, ZString masterBill)
		{
			var matchingLoadLists = Enumerable.Empty<WhsItemDispatchLoadList>();
			matchingLoadLists = TransitUniversalHelper.FindMatchingLoadLists(factory.BOFactory, consolNumber, MasterBill, new[] { WarehouseFromConsol.GetValue(WhsWarehouseSchema.PK) }, AlreadyMatchedLoadLists.Select(l => l.PK).ToArray(), logger);

			return matchingLoadLists;
		}

		WhsItemDispatchLoadList GetMostRecentLoadListFromMatching(WhsItemDispatchLoadList[] consolLoadLists, ZGuid branchPK)
		{
			var isCreateSingleDLLForAllContainers = WarehouseDataRegistry.Instance.CreateSingleDLLForAllContainers.GetFallBackValueAtAllLevels(Guid.Empty, branchPK.ToGuid(), Guid.Empty);
			if (isCreateSingleDLLForAllContainers)
			{
				return GetMostRecentLoadlist(consolLoadLists);
			}

			if (ContainerDOs.Any())
			{
				var hasContainerIds = ContainerDOs.All(c => c.ContainerNumber.HasValue && !string.IsNullOrEmpty(c.ContainerNumber.Value));

				IEnumerable<WhsItemDispatchLoadList> filteredLoadLists = null;
				if (hasContainerIds)
				{
					filteredLoadLists = FilterLoadListsByContainerNumber(consolLoadLists, ContainerDOs);
				}
				else
				{
					filteredLoadLists = FilterLoadListsByType(consolLoadLists, ContainerDOs, PackingLineDOs);
					if (!filteredLoadLists.Any())
					{
						filteredLoadLists = consolLoadLists;
					}
				}

				var loadListToReturn = GetMostRecentLoadlist(filteredLoadLists);

				if (loadListToReturn != null && loadListToReturn.WDL_CompleteTime.IsValid)
				{
					if (!IsLoadListPackageSetSameAsPlannedPackageSet(loadListToReturn.PackageStates.Select(p => p.PK)))
					{
						// if planned package set is same we have found exact match but this doesn't mean it will be overridden. We will not override any data
						// but we do allow other load lists / DTU pairs to extra packages created.
						throw new DataObjectReadFailureException(Res.GetString("05070c58-f65c-4463-9823-8007b658035c", "Matching Load List {0} is already completed and its packages cannot be modified.", loadListToReturn.WDL_JobID));
					}
				}

				return loadListToReturn;
			}
			else
			{
				var matchingLoadListsWithoutContainers = GetLoadListsWithNoContainers(consolLoadLists);
				return matchingLoadListsWithoutContainers.Any() ? GetMostRecentLoadlist(matchingLoadListsWithoutContainers) : GetMostRecentLoadlist(consolLoadLists);
			}
		}

		IEnumerable<WhsItemDispatchLoadList> GetMatchingLoadLists_ByVehicle()
		{
			var dllQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchLoadList));
			dllQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse, WarehouseFromConsol.GetValue(WhsWarehouseSchema.PK));

			var dtuQuery = new ZDBOnlySubQuery(typeof(WhsItemDispatchTransportationUnit), WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, WhsItemDispatchTransportationUnitSchema.PK);
			dtuQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_UnitType, TransportUnitTypes.Vehicle);
			dtuQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference, VehicleRegistrationNumber.Value);

			var pivotSubQuery = new ZDBOnlySubQuery(typeof(WhsItemDispatchLoadListDTUPivot), WhsItemDispatchLoadListSchema.PK, WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList);
			pivotSubQuery.AddSubQuery(dtuQuery, JoinCondition.And);

			dllQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			var loadLists = factory.Load<WhsItemDispatchLoadList>(dllQuery);

			var filteredLoadLists = loadLists.Where(l => l.DispatchTransportationUnits.Any(d => !d.HasContainerEquipmentDetails));

			return filteredLoadLists;
		}

		IEnumerable<WhsItemDispatchLoadList> FilterMatchingLoadLists_ByVehicle(WhsItemDispatchLoadList[] matchingLoadLists)
		{
			if (matchingLoadLists == null || matchingLoadLists.Length == 0)
			{
				return matchingLoadLists;
			}

			var filteredLoadLists = new List<WhsItemDispatchLoadList>();
			foreach (var loadList in matchingLoadLists)
			{
				var vehicleDTUs = loadList.DispatchTransportationUnits.Where(d => d.WDH_UnitType == TransportUnitTypes.Vehicle && !d.HasContainerEquipmentDetails);
				if (vehicleDTUs.Any())
				{
					var vehicleRegistrationNumbers = vehicleDTUs.Where(d => !string.IsNullOrEmpty(d.WDH_VehicleReference)).Select(d => d.WDH_VehicleReference).ToHashSet();
					if (vehicleRegistrationNumbers.Contains(VehicleRegistrationNumber.Value))
					{
						filteredLoadLists.Add(loadList);
					}
				}
			}
			return filteredLoadLists;
		}

		static IEnumerable<WhsItemDispatchLoadList> FilterLoadListsByType(WhsItemDispatchLoadList[] loadLists, IReadOnlyCollection<Container> containerDOs, IReadOnlyCollection<PackingLine> packingLineDOs)
		{
			var listOfMatchingLoadLists = new List<WhsItemDispatchLoadList>();
			foreach (var loadList in loadLists)
			{
				var loadListDTUs = loadList.DispatchTransportationUnits;
				var loadListDTUsGroupByContainerType = loadListDTUs.Where(d => d.HasContainerEquipmentDetails).Select(d => d.Container.ContainerType.RC_Code).Distinct();
				var containerDOsGroupByContainerType = containerDOs.Where(c => c.ContainerType != null && c.ContainerType.Code.HasValue && !string.IsNullOrEmpty(c.ContainerType.Code.Value)).Select(c => c.ContainerType.Code.Value).Distinct();

				if (loadListDTUsGroupByContainerType.ContainsSameElementsInAnyOrder(containerDOsGroupByContainerType))
				{
					listOfMatchingLoadLists.Add(loadList);
				}
			}

			if (listOfMatchingLoadLists.Count > 1)
			{
				return FilterMatchingLoadLists_ByPacklineDOs(packingLineDOs, listOfMatchingLoadLists);
			}

			return listOfMatchingLoadLists;
		}

		static IEnumerable<WhsItemDispatchLoadList> FilterMatchingLoadLists_ByPacklineDOs(IReadOnlyCollection<PackingLine> packingLineDOs, IEnumerable<WhsItemDispatchLoadList> matchingLoadLists, bool shouldMatchAllPackingLines = false)
		{
			var filteredLoadLists = new List<WhsItemDispatchLoadList>();

			var loadLists_WithPackingLineMatchingCount = GetLoadLists_WithPackingLineMatchingCount(packingLineDOs, matchingLoadLists);
			if (loadLists_WithPackingLineMatchingCount.Count > 0)
			{
				var maxMatchingItemCount = loadLists_WithPackingLineMatchingCount.Max(i => i.matchingItemCount);
				if (maxMatchingItemCount > 0)
				{
					filteredLoadLists = loadLists_WithPackingLineMatchingCount.Where(i => i.matchingItemCount == maxMatchingItemCount).Select(i => i.loadList).ToList();
					if (shouldMatchAllPackingLines)
					{
						filteredLoadLists = filteredLoadLists.Where(l => l.PackageStates != null && l.PackageStates.Count == maxMatchingItemCount).ToList();
					}
				}
			}

			return filteredLoadLists;
		}

		static List<(WhsItemDispatchLoadList loadList, int matchingItemCount)> GetLoadLists_WithPackingLineMatchingCount(IReadOnlyCollection<PackingLine> packingLineDOs, IEnumerable<WhsItemDispatchLoadList> matchingLoadLists)
		{
			var matchingItemCount = new List<(WhsItemDispatchLoadList loadList, int matchingItemsCount)>();
			foreach (var matchingLoadList in matchingLoadLists)
			{
				if (matchingLoadList.PackageStates.Any(p => p.Package != null && !string.IsNullOrEmpty(p.Package.KP_ExternalReference)))
				{
					// Forwarding sends receive and dispatch
					var packingLineIDsFromBO = matchingLoadList.PackageStates.Where(p => p.Package != null && !string.IsNullOrEmpty(p.Package.KP_ExternalReference))
						.Select(p => p.Package.KP_ExternalReference).Distinct().ToHashSet();

					var packingLineIDsFromDO = packingLineDOs.Where(p => p.PackingLineID.HasValue && !string.IsNullOrEmpty(p.PackingLineID.Value))
						.Select(p => p.PackingLineID.Value).ToHashSet();

					matchingItemCount.Add((matchingLoadList, packingLineIDsFromBO.Intersect(packingLineIDsFromDO).Count()));
				}
				else
				{
					// blind only
					var packingLineIDsFromBO = matchingLoadList.PackageStates.Where(p => p.Package != null && !string.IsNullOrEmpty(p.Package.KP_PackageID))
						.Select(p => p.Package.KP_PackageID).Distinct().ToHashSet();

					var packingLineIDsFromDO = packingLineDOs.Where(p => p.ReferenceNumber.HasValue && !string.IsNullOrEmpty(p.ReferenceNumber.Value))
						.Select(p => p.ReferenceNumber.Value).ToHashSet();

					matchingItemCount.Add((matchingLoadList, packingLineIDsFromBO.Intersect(packingLineIDsFromDO).Count()));
				}
			}

			return matchingItemCount;
		}

		static IEnumerable<WhsItemDispatchLoadList> FilterLoadListsByContainerNumber(WhsItemDispatchLoadList[] loadListsMatchingByConsolRefs, IReadOnlyCollection<Container> containerDOs)
		{
			var matchingLoadLists = new List<WhsItemDispatchLoadList>();

			foreach (var loadList in loadListsMatchingByConsolRefs)
			{
				var loadListDTUs = loadList.DispatchTransportationUnits.Where(d => !string.IsNullOrEmpty(d.ContainerNumber));

				foreach (var dtu in loadListDTUs)
				{
					var containerNumber = dtu.ContainerNumber;
					if (containerDOs.Any(c => c.ContainerNumber.HasValue && c.ContainerNumber.Value == containerNumber))
					{
						matchingLoadLists.Add(loadList);
						break;
					}
				}
			}

			FilterLoadListWithBlankContainer(
				loadListsMatchingByConsolRefs,
				containerDOs,
				matchingLoadLists);

			FilterLoadListWithNoDTUs(loadListsMatchingByConsolRefs, matchingLoadLists);

			return matchingLoadLists;
		}

		static void FilterLoadListWithBlankContainer(WhsItemDispatchLoadList[] loadListsMatchingByConsolRefs, IReadOnlyCollection<Container> containerDOs, List<WhsItemDispatchLoadList> matchingLoadLists)
		{
			if (!matchingLoadLists.Any() && containerDOs.Count == 1)
			{
				var loadListsWithMatchingContainerCount = loadListsMatchingByConsolRefs.Where(l => l.DispatchTransportationUnits.Count == 1 && string.IsNullOrEmpty(l.DispatchTransportationUnits.Single().ContainerNumber));
				matchingLoadLists.AddRange(loadListsWithMatchingContainerCount);
			}
		}

		static void FilterLoadListWithNoDTUs(WhsItemDispatchLoadList[] loadListsMatchingByConsolRefs, List<WhsItemDispatchLoadList> matchingLoadLists)
		{
			if (!matchingLoadLists.Any())
			{
				matchingLoadLists.AddRange(loadListsMatchingByConsolRefs.Where(l => !l.DispatchTransportationUnits.Any()));
			}
		}

		static IEnumerable<WhsItemDispatchLoadList> GetLoadListsWithNoContainers(IEnumerable<WhsItemDispatchLoadList> loadLists) => loadLists.Where(l => l.DispatchTransportationUnits.All(dtu => !dtu.HasContainerEquipmentDetails));

		static WhsItemDispatchLoadList GetMostRecentLoadlist(IEnumerable<WhsItemDispatchLoadList> loadLists) => loadLists.OrderByDescending(l => l.WDL_SystemCreateTimeUtc).FirstOrDefault();

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsItemDispatchLoadList targetBO)
		{
			var loadListRow = GetColumnIndexerFromRow(factory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, targetBO.PK));

			if (loadListRow.GetValue(WhsItemDispatchLoadListSchema.WDL_IsAwaitingForwardingChanges))
			{
				SetValue(loadListRow, WhsItemDispatchLoadListSchema.WDL_IsAwaitingForwardingChanges, false);

				var reference = WhsTransitLogHelper.GetEventReferenceString(
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, (NoResString)"CANCEL STOP LOAD"), // event reference parameters
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Warehouse, targetBO.Warehouse.WW_WarehouseCode),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, targetBO.WDL_JobID),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, targetBO.WDL_ReferenceNumber));
				WhsTransitLogHelper.AddStmALog(factory, logger, targetBO.PK.ToGuid(), targetBO.TableName, reference, Events.StatusUpdatedCode);
			}

			var loadListPackages = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WDL_LoadList, targetBO.PK));
			var isAllPackagesDeparted = loadListPackages.Any() && loadListPackages.All(p => p.WPS_Status.ToString().IsPackageDeparted());

			if (!isAllPackagesDeparted)
			{
				// Only allow modifications if load list is not load complete.
				// This doesn't mean rejection instead we just ignore import of this load list / DTU in the UXML file.
				var warehousePK = WarehouseFromConsol.GetValue(WhsWarehouseSchema.PK);

				if (IsNewBO)
				{
					SetValue(loadListRow, WhsItemDispatchLoadListSchema.WDL_WW_Warehouse, warehousePK);
					SetValue(loadListRow, WhsItemDispatchLoadListSchema.WDL_JobID, NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseDispatchLoadListReferenceNumber));
				}

				var warehouse = targetBO.Warehouse;
				var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
				var shipments = consolDO?.SubShipmentCollection;

				// Normally we don't create DLLs from standalone shipments but some UTs use standalone shipments as setup so we need it here
				var outboundTransportLeg = TransitUniversalHelper.GetOutboundTransportLeg(factory, warehouse, consolDO, sourceDO: dataObject);

				if (shipments != null)
				{
					PopulateExpectedDispatchIfValid(shipments, warehouse, outboundTransportLeg, targetBO);
				}

				if (outboundTransportLeg != null)
				{
					PopulateCTOCutOffIfValid(targetBO, warehouse, outboundTransportLeg);
				}

				SetValue(loadListRow, WhsItemDispatchLoadListSchema.WDL_TransportMode, dataObject.GetTransportModeForConsolLevel());
				PopulateLoadListAdditionalReferences(targetBO, outboundTransportLeg, consolDO);

				if (!string.IsNullOrEmpty(DCNWarningNote))
				{
					var noteHelper = new WhsTransitStmNoteHelper(logger, factory);
					noteHelper.AddOrUpdateLoadListWarningNote(loadListRow.GetValue(WhsItemDispatchLoadListSchema.PK), DCNWarningNote);
				}

				PopulatePackageStates(targetBO);

				var stagingLocationPK = loadListRow.GetValue(WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation);
				if (!stagingLocationPK.IsValid)
				{
					PopulateLoadListStagingLocation(warehousePK, loadListRow);
				}

				LinkParentConsol(targetBO);
				PopulateUniversalJobLinks(targetBO, consolDO);
				PopulateTransportLegs(targetBO, consolDO);
				PopulateNotes(targetBO);
				PopulateReference(targetBO, consolDO);
				PopulateLastDischarge(targetBO, consolDO);
				PopulateDocAddresses(targetBO, consolDO);

				// updating rows where the BizO is already loaded in the same factory causes issues
				// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
				targetBO.HasChanges = true;
			}
		}

		void PopulateDocAddresses(WhsItemDispatchLoadList targetBO, Shipment consolDO)
		{
			var creditorAddress = consolDO?.OrganizationAddressCollection?.FirstOrDefault(a => a.AddressType.GetValueOrDefault() == nameof(DocAddressType.Creditor));
			if (creditorAddress != null)
			{
				new OrganisationDataObjectReader(creditorAddress, logger, factory).GetMatchedOrNew(targetBO);
			}
		}

		void PopulateLastDischarge(WhsItemDispatchLoadList targetBO, Shipment consolDO)
		{
			SetValue(targetBO, WhsItemDispatchLoadListSchema.WDL_RL_NKLastDischargePort, consolDO?.PortOfDischarge?.Code ?? string.Empty);
		}

		void PopulateReference(WhsItemDispatchLoadList targetBo, Shipment consolDO)
		{
			var referenceNumber = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key ?? targetBo.WDL_JobID;
			if (referenceNumber.IsEmpty)
			{
				referenceNumber = targetBo.WDL_JobID;
			}
			SetValue(targetBo, WhsItemDispatchLoadListSchema.WDL_ReferenceNumber, referenceNumber);
		}

		void LinkParentConsol(WhsItemDispatchLoadList targetBO)
		{
			if (logger.IsInternalImport())
			{
				var consolDataSource = dataObject.GetMatchingDataSource(DataContextType.ForwardingConsol);
				var parent = consolDataSource?.GetLoadedJobFromDataContextType(dataObject, factory.BOFactory);

				if (parent != null && parent is IForwardingConsol)
				{
					SetValue(targetBO, WhsItemDispatchLoadListSchema.WDL_ParentID, parent.PK);
					SetValue(targetBO, WhsItemDispatchLoadListSchema.WDL_ParentTableCode, JobConsolSchema.Constants.Prefix);

					logger.Log(Enterprise.Integration.LogType.Information, ResString.GetMultilingualString("3458ecbd-f586-492f-96ef-df9a18ef909c", "Dispatch Load list {0} linked to Consol {1}.", targetBO.WDL_JobID, consolDataSource.Key));
				}
			}
		}

		void PopulateUniversalJobLinks(WhsItemDispatchLoadList targetBO, Shipment consolDO)
		{
			if (consolDO != null && BookingParty != null)
			{
				var sourceDataContext = consolDO.DataContext;
				var linkCreator = new UniversalJobLinkCreator(targetBO.Factory, targetBO, BookingParty, sourceDataContext, logger);
				linkCreator.TryCreateJobLink(DataContextType.ForwardingConsol);
			}
		}

		void PopulateTransportLegs(WhsItemDispatchLoadList loadList, Shipment consolDO)
		{
			TransportLegImportHelper.ReadTransportLegs(logger, consolDO, factory, loadList, loadList.Warehouse, onlyIncludeOutbound: true);
		}

		#region Populate Expected Dates

		void PopulateExpectedDispatchIfValid(DataObjectList<Shipment> shipments, WhsWarehouse warehouse, TransportLeg outboundTransportLeg, WhsItemDispatchLoadList loadList)
		{
			var expectedDispatch = GetExpectedDispatchDateTimeOffset(shipments, warehouse, outboundTransportLeg);
			if (expectedDispatch.IsValid)
			{
				loadList.WDL_ExpectedDispatchTime = expectedDispatch;
			}
		}

		ZDateTimeOffset GetExpectedDispatchDateTimeOffset(DataObjectList<Shipment> shipments, WhsWarehouse warehouse, TransportLeg outboundTransportLeg)
		{
			var shipmentsWithDeliveryDetails = shipments.Where(shipment => ShipmentHasDeliveryDetails(shipment, warehouse));
			var expectedDispatchDateTimeOffset = GetEtdFromShipmentDeliveryDetails(shipmentsWithDeliveryDetails, warehouse.RelatedCompanyBranch.HomePort);

			if (!expectedDispatchDateTimeOffset.IsValid && outboundTransportLeg != null)
			{
				var ctoCutOff = outboundTransportLeg.FCLCutOff.GetValueOrDefault();
				var localExpectedDispatch = ctoCutOff.IsValid ? ctoCutOff : outboundTransportLeg.EstimatedDeparture.GetValueOrDefault();
				if (localExpectedDispatch.IsValid)
				{
					expectedDispatchDateTimeOffset = MasterExtensions.ToDateTimeOffset(localExpectedDispatch, warehouse.RelatedCompanyBranch.HomePort);
				}
			}

			return expectedDispatchDateTimeOffset;
		}

		bool ShipmentHasDeliveryDetails(Shipment shipment, WhsWarehouse warehouse)
		{
			var isDestinationWarehouse = shipment.PortOfDestination != null && TransitUniversalHelper.IsGivenPortARelatedOrExtraPort(warehouse, shipment.PortOfDestination.Code);
			return isDestinationWarehouse && shipment.LocalProcessing != null;
		}

		ZDateTimeOffset GetEtdFromShipmentDeliveryDetails(IEnumerable<Shipment> shipmentWithDeliveryDetails, RefUNLOCO warehouseUNLOCO)
		{
			var expectedDispatchDateTimeOffset = ZDateTimeOffset.Empty;
			var earliestEstimatedDelivery = ZDateTime.Empty;
			var earliestDeliveryRequiredBy = ZDateTime.Empty;

			foreach (var shipment in shipmentWithDeliveryDetails)
			{
				var estimatedDelivery = shipment.LocalProcessing.EstimatedDelivery.GetValueOrDefault();
				if (estimatedDelivery.IsValid && (earliestEstimatedDelivery.IsEmpty || estimatedDelivery < earliestEstimatedDelivery))
				{
					earliestEstimatedDelivery = estimatedDelivery;
				}

				var deliveryRequiredBy = shipment.LocalProcessing.DeliveryRequiredBy.GetValueOrDefault();
				if (deliveryRequiredBy.IsValid && (earliestDeliveryRequiredBy.IsEmpty || deliveryRequiredBy < earliestDeliveryRequiredBy))
				{
					earliestDeliveryRequiredBy = deliveryRequiredBy;
				}
			}

			var localExpectedDispatch = earliestEstimatedDelivery.IsValid ? earliestEstimatedDelivery : earliestDeliveryRequiredBy;
			if (localExpectedDispatch.IsValid)
			{
				expectedDispatchDateTimeOffset = MasterExtensions.ToDateTimeOffset(localExpectedDispatch, warehouseUNLOCO);
			}

			return expectedDispatchDateTimeOffset;
		}

		void PopulateCTOCutOffIfValid(WhsItemDispatchLoadList loadList, WhsWarehouse warehouse, TransportLeg outboundTransportLeg)
		{
			ZDateTime ctoCutOff = outboundTransportLeg.FCLCutOff.GetValueOrDefault();
			var ctoCutOffDateTimeOffset = MasterExtensions.ToDateTimeOffset(ctoCutOff, warehouse.RelatedCompanyBranch.HomePort);
			if (ctoCutOffDateTimeOffset.IsValid)
			{
				loadList.WDL_CTOCutOffTime = ctoCutOffDateTimeOffset;
			}
		}

		#endregion

		// Note: We assmue all packages are from the same Receive Consignment for now.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL - not localisable")]
		void PopulateLoadListStagingLocation(ZGuid warehousePK, IColumnIndexer loadListRow)
		{
			var rcnPK = finderForDLL?.Find().Select(p => p.WPS_WRC_TransitReceiveConsignment)?.Distinct().ToArray().FirstOrDefault(x => x.IsValid);
			if (rcnPK != null)
			{
				var receiveConsignment = factory.RowFactory.Load(WhsItemReceiveConsignmentSchema.Constants.TableName, new ZQuery(WhsItemReceiveConsignmentSchema.PK, rcnPK)).FirstOrDefault();

				if (receiveConsignment != null)
				{
					var serviceLevel = receiveConsignment[WhsItemReceiveConsignmentSchema.WRC_RS_NKServiceLevel.Name];
					var dischargePort = receiveConsignment[WhsItemReceiveConsignmentSchema.WRC_RL_NKNextDischargePort.Name];

					var connection = ((IDbConnected)factory.BOFactory).Connection;
					Guid locationPK;
					const string cmdText = "EXEC WhsLocationForMatchingServiceLevelDischargePortAndWarehouseSP @ServiceLevel, @DischargePort, @WarehousePK";
					using (var cmd = connection.Command(cmdText))
					{
						cmd.AddParameter("@ServiceLevel", SqlDbType.VarChar, 3, serviceLevel);
						cmd.AddParameter("@DischargePort", SqlDbType.VarChar, 5, dischargePort);
						cmd.AddParameter("@WarehousePK", SqlDbType.UniqueIdentifier, warehousePK.ToGuid());
						var value = cmd.ExecuteScalar();
						locationPK = (value == null || value == DBNull.Value) ? Guid.Empty : (Guid)value;
					}

					if (locationPK != Guid.Empty)
					{
						SetValue(loadListRow, WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation, locationPK);
					}
				}
			}
		}

		void PopulateLoadListAdditionalReferences(WhsItemDispatchLoadList targetBO, TransportLeg outboundTransportLeg, Shipment consolDO)
		{
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(logger, factory);
			var additionalReferences = new List<TransitAdditionalReferenceInfo>();

			if (outboundTransportLeg != null)
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.Vessel, outboundTransportLeg.VesselName);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, outboundTransportLeg.VoyageFlightNo);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.DestinationPort, outboundTransportLeg.PortOfDischarge?.Code?.ToString());
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ETDDate, WhsTransitReceiveConsignmentDataObjectReader.GetDateAsString(outboundTransportLeg.EstimatedDeparture ?? ZDateTime.Empty));
			}

			// forwarding references
			var forwardingConsolDS = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol);
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, forwardingConsolDS?.Key ?? "");
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, AdditionalReferenceTypes.Codes.MasterBill, MasterBill.IsEmpty ? (dataObject.WayBillNumber ?? "") : MasterBill);

			// land transport references
			var runSheetDS = consolDO?.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheet);
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, runSheetDS?.Key ?? "");

			// if UXML is from gate booking, BookingConfirmationReference could be different meanings, it is not a CarrierBookingReference
			if (!dataObject.IsFromDataSource(DataContextType.GateBooking))
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, consolDO?.BookingConfirmationReference ?? "");
			}

			foreach (var existingAdditionalReference in existingAdditionalReferences)
			{
				if (!additionalReferences.Any(a => (ZString)a.Type == existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryType) && (ZString)a.Value == existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryNum) && (ZString)a.Category == existingAdditionalReference.GetValue(CusEntryNumSchema.CE_Category)))
				{
					referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryType), existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryNum), existingAdditionalReference.GetValue(CusEntryNumSchema.CE_Category));
				}
			}

			new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), targetBO, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
			targetBO.RefreshAdditionalReferenceNumbers();
		}

		#endregion

		#region PopulatePackageStates

		void PopulatePackageStates(WhsItemDispatchLoadList targetBO)
		{
			if (finderForDLL != null)
			{
				var collectionReader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderForDLL, targetBO, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.DLL);

				if (handler != null)
				{
					collectionReader.SubscribeToCollectionChanges(handler);
				}

				collectionReader.ReadIntoCollectionRetainingUnmatchedElements();
			}
		}

		#endregion

		#region PopulateNotes

		void PopulateNotes(WhsItemDispatchLoadList dll)
		{
			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			if (consolDO?.NoteCollection != null)
			{
				var notesToImport = WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(dll, consolDO.NoteCollection, StmNoteDescription.Pub, StmNoteDescription.Agv, StmNoteDescription.Prv, StmNoteDescription.Int);

				if (notesToImport.Any())
				{
					new NotesCollectionReader(notesToImport, logger, factory, dll).ReadIntoCollection();
				}
			}
		}

		#endregion

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitDispatch; }
		}

		#endregion

		#region Implementation

		bool IsLoadListPackageSetSameAsPlannedPackageSet(IEnumerable<ZGuid> loadListPackageStatePKs)
		{
			var packageStatePKsForDLL = finderForDLL?.Find().Select(p => p.PK)?.Distinct().ToArray();
			return packageStatePKsForDLL != null && loadListPackageStatePKs.ContainsSameElementsInAnyOrder(packageStatePKsForDLL);
		}

		#endregion
	}
}
