using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitReceiveVehicleHandler : ITransitDataObjectReaderHandler
	{
		readonly UniversalShipment dataObject;

		public TransitReceiveVehicleHandler(UniversalShipment dataObject)
		{
			this.dataObject = dataObject;
		}

		TransitDataObjectReaderHandlerManager HandlerManager
		{
			get
			{
				handlerManager ??= ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();
				return handlerManager;
			}
		}
		TransitDataObjectReaderHandlerManager handlerManager;

		WhsWarehouse Warehouse => HandlerManager.GetWarehouse();

		public void Execute(UniversalObjectFactory factory, UniversalShipment dataObject, IXmlImportLogger logger)
		{
			HandleVehicle(factory, logger);
		}

		void HandleVehicle(UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var vehicle = dataObject.GetVehicle();
			if (vehicle != null)
			{
				var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
				var isFromSeaCargo = consolDO?.GetMatchingDataSource(DataContextType.SeaCargoOutturn) != null;
				var bookingParty = TransitUniversalHelper.GetBookingParty(factory, logger, isRequired: !isFromSeaCargo);

				var bookingConfirmationReference = dataObject.GetBookingConfirmationReference();
				if (!bookingConfirmationReference.HasValue || bookingConfirmationReference.Value.IsEmpty)
				{
					throw new DataObjectReadFailureException(Res.GetString("c0d7b940-b87b-4ee6-9f85-5ee79a551760", "Booking Confirmation Reference is not provided in UXML."));
				}
				var firstSubShpmentFromGateBooking = dataObject.GetFirstSubShipmentFromGateBooking() ?? throw new DataObjectReadFailureException(Res.GetString("d877c210-2795-4859-93c2-026cda142945", "Gate Movement Booking is not provided in UXML."));

				var packageStatesFromJob = GetPackageStatesByBookingConfirmationRef(bookingConfirmationReference.Value, factory, logger, out ZString? masterBill);
				if (packageStatesFromJob == null || packageStatesFromJob.Length == 0)
				{
					var packages = HandlerManager.GetPackagesByContainerLink().SelectMany(kvp => kvp.Value).ToArray();
					var packagePKs = packages.Select(p => p.PK);
					var packageStatesQuery = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packagePKs);
					packageStatesFromJob = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(packageStatesQuery));
				}
				if (packageStatesFromJob.Length == 0)
				{
					throw new DataObjectReadFailureException(Res.GetString("8da9cab4-40dd-43b2-a383-78ed80454769", "Could not find any package state using Container Number, House Bill and Master Bill."));
				}

				var asnPKs = packageStatesFromJob.Select(ps => ps.WPS_WRP_ReceiveExpectedPacking).Where(p => !p.IsEmpty).Distinct().ToList();
				var containerByASNDTOs = RTUColumnIndexerHelper.GetExistingRTUs_ByAsnPK(factory, asnPKs);
				var containerNumbersInDO = dataObject.GetContainerNumbers();

				var isLoosePackagesBooking = containerNumbersInDO.Count == 0;
				if (isLoosePackagesBooking)
				{
					var additionalReferences = containerByASNDTOs.Select(c => c.ASNPK).SelectMany(pk => AdditionalReferenceColumnIndexerHelper.GetAdditionalReferencesByParent(factory, pk));
					var vehicleASN = new WhsTransitReceiveASNDataObjectReader(dataObject, null, null, bookingParty, Warehouse, logger, factory, false, vehicle, masterBill, additionalReferences).ReadIntoBusinessObject();
					var finderForASN = new WhsTransitPackageStateBusinessObjectFinderForASN(dataObject, factory, packageStatesFromJob, containerByASNDTOs, vehicleASN, masterBill);

					var collectionReader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderForASN, vehicleASN, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.ASN);
					collectionReader.ReadIntoCollectionRetainingUnmatchedElements();
				}
				else
				{
					var latestASNPK = GetLatestASNPK(factory, asnPKs);

					var finderForASN = new WhsTransitPackageStateBusinessObjectFinderForASN(dataObject, factory, packageStatesFromJob, containerByASNDTOs, null, masterBill, latestASNPK);
					var pkgStates = finderForASN.Find();

					var shouldHandleBlindContainers = false;
					if (!asnPKs.Any())
					{
						shouldHandleBlindContainers = true;
					}
					else
					{
						var pivotQuery = new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPKs);
						var asnRtuPivots = Array.ConvertAll(factory.RowFactory.Load(WhsItemReceiveASNRTUPivotSchema.Constants.TableName, pivotQuery), DataObjectReader.GetColumnIndexerFromRow);
						shouldHandleBlindContainers = asnRtuPivots.Length == 0;
					}

					var isBlindContainerPackagesAndPackageStatesToArriveCountMatch = false;
					if (shouldHandleBlindContainers)
					{
						var packagePKs = pkgStates.Select(p => p.WPS_KP_Package);
						var packagesQuery = new ZQuery(PkgPackageSchema.PK, packagePKs);
						var packages = factory.BOFactory.Load<PkgPackage>(packagesQuery);
						var totalASNPackageQty = packages.Sum(p => p.KP_PackageQty);

						var container = dataObject.GetContainers().FirstOrDefault();
						var containerLink = container.Link;
						var containerPackageQuantity = dataObject.GetPackingLines().Where(p => p.ContainerLink == containerLink).Sum(p => p.PackQty);
						isBlindContainerPackagesAndPackageStatesToArriveCountMatch = totalASNPackageQty == containerPackageQuantity;

						if (isBlindContainerPackagesAndPackageStatesToArriveCountMatch)
						{
							var asnReaderForBlindContainer = new WhsTransitReceiveASNDataObjectReader(dataObject, new Container[] { container }, null, bookingParty, Warehouse, logger, factory, false, null, masterBill, finder: finderForASN);
							var asnForBlindContainer = asnReaderForBlindContainer.ReadIntoBusinessObject();
							asnPKs.Add(asnForBlindContainer.PK);
						}
						else
						{
							var vehicleASN = new WhsTransitReceiveASNDataObjectReader(dataObject, null, null, bookingParty, Warehouse, logger, factory, false, vehicle, masterBill).ReadIntoBusinessObject();
							var collectionReader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderForASN, vehicleASN, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.ASN);
							collectionReader.ReadIntoCollectionRetainingUnmatchedElements();

							var containerRTUReader = new WhsTransitReceiveTransportationUnitDataObjectReader(Warehouse, container, null, vehicleASN, logger, factory, bookingParty, masterBill, false, false);
							var containerRTU = containerRTUReader.ReadIntoBusinessObject();
							var filteredContainerNumbers = new HashSet<(ZString containerNumber, ZGuid rtuPK)>
							{
								(container.ContainerNumber ?? "", containerRTU.PK)
							};
							CreateContainerUniversalLinkToGateMovementBooking(dataObject, factory, logger, filteredContainerNumbers);

							if (latestASNPK != null)
							{
								HandlerManager.AddAffectedASNPK((ZGuid)latestASNPK);
							}
						}
					}

					#region Link to Vehicle

					if (!shouldHandleBlindContainers || isBlindContainerPackagesAndPackageStatesToArriveCountMatch)
					{
						var filteredContainerNumbers = new HashSet<(ZString containerNumber, ZGuid rtuPK)>();
						var containerByASNDTOsUpdated = RTUColumnIndexerHelper.GetExistingRTUs_ByAsnPK(factory, asnPKs);
						var finderForLinkToVehicle = new WhsTransitPackageStateBusinessObjectFinderForASN(dataObject, factory, containerByASNDTOsUpdated);
						var pacakgeStatesFromFinder = finderForLinkToVehicle.Find();

						pacakgeStatesFromFinder.Where(ps => !ps.WPS_WRP_ReceiveExpectedPacking.IsEmpty).ForEach(ps => HandlerManager.AddAffectedASNPK(ps.WPS_WRP_ReceiveExpectedPacking));

						var containerDTOs = containerByASNDTOsUpdated.SelectMany(c => c.Containers).ToArray();
						var pacakgeStatePKsFromFinder = pacakgeStatesFromFinder.Select(p => p.PK).ToArray();
						foreach (var containerDTO in containerDTOs)
						{
							if (pacakgeStatePKsFromFinder.Contains(containerDTO.PackageState.PK))
							{
								var containerNumber = containerDTO.RTU.WRH_VehicleReference;
								var rtuPK = containerDTO.RTU.PK;
								filteredContainerNumbers.Add((containerNumber, rtuPK));
							}
						}

						if (!filteredContainerNumbers.Any())
						{
							throw new DataObjectReadFailureException(Res.GetString("c389e776-ddaa-4070-a652-6c11cc78b0c3", "Gate Booking has a container and failed to find a matching ASN with the same container."));
						}

						CreateContainerUniversalLinkToGateMovementBooking(dataObject, factory, logger, filteredContainerNumbers);

						var vehicleASN = new WhsTransitReceiveASNDataObjectReader(dataObject, null, null, bookingParty, Warehouse, logger, factory, false, vehicle, masterBill, finder: finderForLinkToVehicle).ReadIntoBusinessObject();

						// Exclude the vehicle ASN
						containerByASNDTOsUpdated
							.Where(rtus_ByAsnPK => rtus_ByAsnPK.ASNPK != vehicleASN.PK)
							.ForEach(rtus_ByAsnPK => HandlerManager.AddAffectedASNPK(rtus_ByAsnPK.ASNPK));
					}

					#endregion
				}
			}
		}

		WhsItemPackageState[] GetPackageStatesByBookingConfirmationRef(ZString bookingConfirmationReference, UniversalObjectFactory factory, IXmlImportLogger logger, out ZString? masterBill)
		{
			masterBill = null;
			var readonlyBOFactory = factory.BOFactory.GetCachedReadOnlyFactory();
			var warehouseBO = HandlerManager.GetWarehouse();
			var rtuMatcher = new TWHRTUJobMatcher(readonlyBOFactory, warehouseBO, bookingConfirmationReference, ReferenceNumberTypes.Unknown);
			var rcnMatcher = new TWHRCNJobMatcher(readonlyBOFactory, warehouseBO, bookingConfirmationReference, ReferenceNumberTypes.Unknown);
			var asnMatcher = new TWHASNJobMatcher(readonlyBOFactory, warehouseBO, bookingConfirmationReference, ReferenceNumberTypes.Unknown);

			rtuMatcher.SetNextJobMatcher(rcnMatcher);
			rcnMatcher.SetNextJobMatcher(asnMatcher);

			var jobMatcherResult = rtuMatcher.Process();

			if (jobMatcherResult.ErrorCode != null)
			{
				return null;
			}

			if (jobMatcherResult.ReferenceNumberType == ReferenceNumberTypes.MasterBill)
			{
				masterBill = bookingConfirmationReference;
			}

			return factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, jobMatcherResult.PackageStates?.Select(ps => ps.PK).ToArray()));
		}

		void CreateContainerUniversalLinkToGateMovementBooking(UniversalShipment topLevelDO, UniversalObjectFactory factory, IXmlImportLogger logger, HashSet<(ZString containerNumber, ZGuid rtuPK)> filteredContainerNumbers)
		{
			var firstContainerNumberAndRtuPK = filteredContainerNumbers.ToList()[0];
			var containerRTU = factory.Load<WhsItemReceiveTransportationUnit>(firstContainerNumberAndRtuPK.rtuPK);

			var firstSubShipmentFromGateBooking = topLevelDO.GetFirstSubShipmentFromGateBooking();
			if (containerRTU != null && firstSubShipmentFromGateBooking != null)
			{
				var sourceDataContext = UniversalShipment.GetSourceDataObject(firstSubShipmentFromGateBooking).DataContext;
				var linkCreator = new UniversalJobLinkCreator(containerRTU.Factory, containerRTU, null, sourceDataContext, logger, true);
				linkCreator.TryCreateJobLink(DataContextType.GateMovementBooking);

				// Log Booking Confirmed
				var gateMovementBookingNumber = firstSubShipmentFromGateBooking.GetMatchingDataSourceValue(DataContextType.GateMovementBooking) ?? ZString.Empty;
				WhsTransitLogHelper.LogBookingConfirmed(containerRTU, nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
			}
		}

		ZGuid? GetLatestASNPK(UniversalObjectFactory factory, List<ZGuid> asnPKs)
		{
			if (!asnPKs.Any())
			{
				return null;
			}

			var asnQuery = new ZQuery(WhsItemReceiveASNSchema.PK, asnPKs);
			asnQuery.OrderBy = WhsItemReceiveASNSchema.Constants.WRP_SystemCreateTimeUtc + OrderByClause.Descending;
			var latestASN = Array.ConvertAll(factory.RowFactory.Load(WhsItemReceiveASNSchema.Constants.TableName, asnQuery), DataObjectReader.GetColumnIndexerFromRow).First();
			return latestASN.GetValue(WhsItemReceiveASNSchema.PK);
		}
	}
}
