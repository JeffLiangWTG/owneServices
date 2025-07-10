using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.TransitDataObjectReaderHandlerManager;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.TransitStoredProcedureHandler;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveConsignmentDataObjectReader : WhsTransitConsignmentDataObjectReader<WhsItemReceiveConsignment>
	{
		#region Constructor

		public WhsTransitReceiveConsignmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer warehouseFromConsol = null, Dictionary<ZInt, List<IColumnIndexer>> packagesByContainerLink = null, HashSet<ZGuid> affectedASNPKs = null, bool shouldReadSubShipments = true, ZString? masterShipmentId = null)
			: base(shipment, logger, factory, warehouseFromConsol, masterShipmentId)
		{
			ShouldReadSubshipments = shouldReadSubShipments;
		}

		ITransitDataObjectReaderHandler ConsolHandler => HandlerManager.GetHandler<TransitReceiveConsolHandler>();

		protected override string GetBusinessObjectHumanReadableName(WhsItemReceiveConsignment rcn) => rcn == null ? Res.GetString("af0b6d88-9eb9-4fa5-964c-b0aee0353d52", "Receive Consignment") : rcn.HumanReadableName.ToString();

		readonly bool ShouldReadSubshipments;
		const string CustomsClearedCode = "CLR";
		WhsItemReceiveConsignment rcnForPostSavedAction;

		UniversalShipment topLeveDO;
		UniversalShipment TopLeveDO => topLeveDO ??= TransitUniversalExtensions.GetTopLevelDataObject(logger);
		bool IsArrivalTransitWarehouse => TopLeveDO.IsArrivalTransitWarehouse();

		public bool IsRecalculatePackageStateSecurityStatus { get; private set; }

		#endregion

		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitReceive; }
		}

		#endregion

		#region Parameterised Schema Columns

		protected override SchemaStringColumn ConsignmentIDSchemaColumn => ReceiveConsignmentIDColumn;

		protected override SchemaStringColumn ConsignmentHouseBillColumn => WhsItemReceiveConsignmentSchema.WRC_HouseBillNumber;

		protected override SchemaStringColumn ServiceLevelSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_RS_NKServiceLevel;

		protected override SchemaGuidColumn WarehouseSchemaColumn => ReceiveConsignmentWarehouseColumn;

		static SchemaStringColumn ReceiveConsignmentIDColumn => WhsItemReceiveConsignmentSchema.WRC_ConsignmentID;

		static SchemaGuidColumn ReceiveConsignmentWarehouseColumn => WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse;

		protected override SchemaStringColumn JobIDSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_JobID;

		protected override SchemaGuidColumn ParentIDSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_ParentID;

		protected override SchemaStringColumn ParentTableCodeSchemaColumn => WhsItemReceiveConsignmentSchema.WRC_ParentTableCode;

		protected override SchemaGuidColumn PKSchemaColumn => WhsItemReceiveConsignmentSchema.PK;

		protected override string TablePrefix => WhsItemReceiveConsignmentSchema.Constants.Prefix;

		#endregion

		#region Number Fountain ID

		protected override ZString NumberFountainID => NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseReceiveConsignmentID);

		#endregion

		#region Matching

		#region AttemptToGetConsignmentWithMatchingAddress

		public static WhsItemReceiveConsignment TryGetReceiveConsignmentInWarehouse(IColumnIndexer warehouse, IOrgHeader bookingParty, ZString consignmentID, string houseBill, UniversalShipment sourceDO, UniversalObjectFactory factory, bool isTWX, IXmlImportLogger logger)
		{
			var helper = new WhsTransitReceiveConsignmentMatchingHelper(factory, bookingParty, warehouse, logger);
			return helper.MatchConsignment(houseBill, consignmentID, sourceDO, excludeClosedConsignments: false);
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override WhsTransitConsignmentMatchingHelper<WhsItemReceiveConsignment> GetMatchingHelperCore(UniversalShipment sourceDO, IColumnIndexer warehouseFromConsol)
		{
			return new WhsTransitReceiveConsignmentMatchingHelper(factory, BookingParty, warehouseFromConsol, logger);
		}

		#endregion

		#endregion

		#region Create / Update

		#region GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(WhsItemReceiveConsignment targetBO)
		{
			var reason = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);

			if (reason.IsEmpty && targetBO != null)
			{
				var sourceDOType = GetSourceDataObject()?.FirstDataSource()?.Type;
				if (sourceDOType.HasValue && Enum.TryParse(sourceDOType.Value, out DataContextType dataContextType))
				{
					var links = UniversalJobLinkHelper.GetMatchingJobLinkEntities(targetBO);
					if (links.Any())
					{
						var highestPriorityEarliestLinkEntity = GetHighestPriorityEarliestLink(links, targetBO);
						if (highestPriorityEarliestLinkEntity != null)
						{
							Enum.TryParse(highestPriorityEarliestLinkEntity.UCL_SourceType, out DataContextType highestPriorityEarliestLinkContext);
							var currentDataContextPriority = GetDataContextPriority(dataContextType);
							var highestPriority = GetDataContextPriority(highestPriorityEarliestLinkContext);

							if (highestPriority > currentDataContextPriority)
							{
								reason = Res.GetString("e4a99ff5-a5ed-4bc7-a021-bb87ff65eaec", "Job information is not overridden since it has been previously received from {0} source.", highestPriorityEarliestLinkContext);
								PopulateNecessaryInformationAfterRejectedByPriority(targetBO);
							}
							else if (highestPriority == currentDataContextPriority && !HasSameSenderWithUniversalLink(highestPriorityEarliestLinkEntity))
							{
								reason = Res.GetString("de7f7791-4d2f-4333-87cd-26c98e197046", "Job information is not overridden since it has been previously received from {0} source by another sender.", highestPriorityEarliestLinkContext);
								PopulateNecessaryInformationAfterRejectedByPriority(targetBO);
							}
						}
					}
				}

				if (reason.IsEmpty)
				{
					var packageStateQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, targetBO.PK);
					var packages = factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, packageStateQuery).Select(GetColumnIndexerFromRow).ToArray();

					if (dataObject.PackingLineCollection != null)
					{
						var packagesWithIDs = dataObject.PackingLineCollection.Where(p => !p.ReferenceNumber.GetValueOrDefault().IsEmpty).ToArray();
						var hasLoosePackageIds = dataObject.PackingLineCollection.Any(p => p.IsLoosePackageIDDataObject());
						if (!hasLoosePackageIds && packagesWithIDs.Any() && packagesWithIDs.Length != dataObject.PackingLineCollection.Count)
						{
							reason = Res.GetString("25e6a891-3f89-4177-8950-98242538ac87",
	@"A mix of Packages with and without Package IDs was provided for Receive Consignment {0}. If any Package ID is provided then all Packages must have IDs.
Either add or remove all Package IDs and send again.", GetCodeDescriptionForLogs(targetBO));
						}
					}
				}
			}

			return reason;
		}

		bool HasSameSenderWithUniversalLink(IStmUniversalJobLink link)
		{
			return logger.TopLevelDataContext.GetEnterpriseCode() == link.UCL_EnterpriseCode
				&& logger.TopLevelDataContext.GetServerCode() == link.UCL_ServerCode
				&& logger.TopLevelDataContext.GetCompanyCode() == link.UCL_CompanyCode;
		}

		void PopulateNecessaryInformationAfterRejectedByPriority(WhsItemReceiveConsignment targetBO)
		{
			PopulateLinks(targetBO);
			var sourceDataObject = GetSourceDataObject();
			PopulateConsignmentAdditionalReferences(targetBO, sourceDataObject, true);
		}

		IStmUniversalJobLink GetHighestPriorityEarliestLink(IEnumerable<IStmUniversalJobLink> links, WhsItemReceiveConsignment targetBO)
		{
			if (links.Any())
			{
				var linksAndLevel = links.Select(link =>
				{
					Enum.TryParse(link.UCL_SourceType, out DataContextType context);
					return (Link: link, Level: GetDataContextPriority(context));
				});
				return linksAndLevel.OrderByDescending(l => l.Level).ThenBy(l => l.Link.UCL_SystemCreateTimeUtc).First().Link;
			}
			return null;
		}

		int GetDataContextPriority(DataContextType contextType)
		{
			var priorityDictionary = TransitConstants.TransitDataContextPriority.Value;
			if (priorityDictionary.ContainsKey(contextType.ToString()))
			{
				return priorityDictionary[contextType.ToString()];
			}
			return 0;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObjectCore(UniversalShipment sourceDO, WhsItemReceiveConsignment consignment, IColumnIndexer consignmentRow)
		{
			CheckIfBlindPackageAttached(sourceDO, consignment);
			RegisterHandler();
			PopulateTransportLegs(sourceDO, consignment);
			PopulateOrderReferences(sourceDO, consignment);
			var isAirManifest = sourceDO.GetMatchingDataSource(DataContextType.AirManifestLine) != null;
			consignment.WRC_TransportMode = isAirManifest ? new ZString(TransportModes.Air) : sourceDO.GetTransportModeForShipmentLevel();
			consignment.WRC_ShippersReference = sourceDO.BookingConfirmationReference ?? ZString.Empty;

			var finderForRCN = new WhsTransitPackageStateBusinessObjectFinderForRCN(consignment, factory);
			var packageStates = finderForRCN.Find();

			var existingPackageStates = IsNewBO ? Enumerable.Empty<WhsItemPackageState>() : packageStates;
			var packlines = sourceDO.PackingLineCollection;
			var isFromConsol = logger.GetConsolDataObject(factory) != null;
			var shouldDeletePackagesBeforeImporting = packlines != null && packlines.Content == CollectionContent.Complete && !existingPackageStates.Any(x => x.WPS_Status != TransitWarehouseStatuses.Codes.Booked);
			var shouldDetachASN = !isFromConsol && !shouldDeletePackagesBeforeImporting;

			if (shouldDeletePackagesBeforeImporting || shouldDetachASN)
			{
				var existingPackageStatesHavingASNs = existingPackageStates.Where(p => p.WPS_WRP_ReceiveExpectedPacking != ZGuid.Empty);

				foreach (var packageState in existingPackageStatesHavingASNs)
				{
					HandlerManager.AddAffectedASNPK(packageState.WPS_WRP_ReceiveExpectedPacking);
				}
			}

			// detach ASN from RCN
			new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), sourceDO, logger, factory, finderForRCN, new WhsItemPackageStateCollection(consignment), consignment,WhsTransitPackageStatePopulateStrategy.Detach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.DetachASNFromRCN).ReadIntoCollectionRetainingUnmatchedElements();

			if (shouldDeletePackagesBeforeImporting)
			{
				ClearUnloadTasks(consignmentRow);
				// Use the reader to delete all existing booked processedPackages on the consignment before importing
				new PkgPackageJobDataObjectReader(new PackingSourceDataObject(null, new DataObjectList<PackingLine> { }, new Shipment()), logger, factory, consignment, ImportOption.Default).ReadIntoBusinessObject();
				logger.Log(LogType.Information, ResString.GetMultilingualString("d65a6bb9-8177-49d3-a140-4c6a8bcab053", "Deleting existing booked Packages from previous Universal Import for Receive Consignment '{0}'.", consignment.WRC_ConsignmentID));
			}

			PopulatePackageStates(sourceDO, consignment);

			if (!logger.TopLevelDataContext.DataTargetCollection.IsConsolDataTarget())
			{
				ConsolHandler.Execute(factory, dataObject, logger);
			}

			var rcnIndexers = WhsTransitUniversalEventHelper.GetBizoColumnIndexers(factory, new[] { consignment });

			consignment.HasChanges = true;

			rcnForPostSavedAction = consignment;
		}

		#region ClearUnloadTasks

		void ClearUnloadTasks(IColumnIndexer consignmentRow)
		{
			var packageStatesQuery = new ZQuery(WhsItemUnloadTaskSchema.WUT_WPS_ActivePackline, SQLComparisonOperator.NotEqual, null);
			packageStatesQuery.AddToFilter(new ZQuery(WhsItemUnloadTaskSchema.WUT_WPS_LastScannedPackage, SQLComparisonOperator.NotEqual, null), JoinCondition.Or);
			packageStatesQuery.AddToFilter(new ZQuery(WhsItemUnloadTaskSchema.WUT_WPS_LockedPackage, SQLComparisonOperator.NotEqual, null), JoinCondition.Or);

			var unloadTaskQuery = new ZQuery(WhsItemUnloadTaskSchema.WUT_WRC_ActiveReceiveConsignment, consignmentRow.GetValue(WhsItemReceiveConsignmentSchema.PK));
			unloadTaskQuery.AddToFilter(packageStatesQuery, JoinCondition.And);

			var unloadTasksWithCurrentConsignment = factory.BOFactory.Load<WhsItemUnloadTask>(unloadTaskQuery);
			unloadTasksWithCurrentConsignment.ForEach(unloadTask =>
			{
				unloadTask.SetValue(WhsItemUnloadTaskSchema.WUT_WPS_ActivePackline, ZGuid.Empty);
				unloadTask.SetValue(WhsItemUnloadTaskSchema.WUT_WPS_LastScannedPackage, ZGuid.Empty);
				unloadTask.SetValue(WhsItemUnloadTaskSchema.WUT_WPS_LockedPackage, ZGuid.Empty);
			});
		}

		#endregion

		#region PopulatePackageStates

		void PopulatePackageStates(UniversalShipment sourceDO, WhsItemReceiveConsignment consignment)
		{
			var finderForCreatePackageState = new WhsTransitPackageStateBusinessObjectFinderForRCN(consignment, factory, logger, sourceDO, dataObject);
			var packingLines = GetPackingLinesFromSource(sourceDO.PackingLineCollection);
			var collectionReader = new WhsItemPackageStateDataObjectCollectionReader(packingLines, sourceDO, logger, factory, finderForCreatePackageState, new WhsItemPackageStateCollection(consignment), consignment, WhsTransitPackageStatePopulateStrategy.New, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.RCN);
			collectionReader.ReadIntoCollection();
			var packageStatesAfterReader = collectionReader.GetPackageStates().ToArray();

			IsRecalculatePackageStateSecurityStatus = packageStatesAfterReader.Any(p => p.WPS_IsHighRisk);
		}

		DataObjectList<PackingLine> GetPackingLinesFromSource(DataObjectList<PackingLine> packingLineCollection)
		{
			var result = new DataObjectList<PackingLine>();

			if (packingLineCollection != null)
			{
				foreach (var packingLine in packingLineCollection)
				{
					result.Add(packingLine);
					if (packingLine.PackingLineCollection != null)
					{
						result.AddRange(packingLine.PackingLineCollection.Select(p => p));
					}
				}
			}
			return result;
		}

		void CheckIfBlindPackageAttached(UniversalShipment sourceDO, WhsItemReceiveConsignment consignment)
		{
			var packingLines = GetPackingLinesFromSource(sourceDO.PackingLineCollection);
			if (packingLines != null && packingLines.Any(p => p.ReferenceNumber.HasValue))
			{
				var packageIds = packingLines.Where(p => p.ReferenceNumber.HasValue).Select(p => p.ReferenceNumber.Value);
				var packageStateFromAttachQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));

				var packageIdSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
				packageIdSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, packageIds);
				var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), WhsItemPackageStateSchema.WPS_KP_Package);
				packageSubQuery.AddSubQuery(packageIdSubQuery, JoinCondition.And);

				packageStateFromAttachQuery.AddSubQuery(packageSubQuery, JoinCondition.And);
				packageStateFromAttachQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, SQLComparisonOperator.IsNotBlank, null);
				packageStateFromAttachQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WW_Warehouse, SQLComparisonOperator.Equal, consignment.Warehouse.PK);
				packageStateFromAttachQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, SQLComparisonOperator.NotEqual, consignment.PK);

				var notInWarehouseStatuses = new List<string>() { TransitWarehouseStatuses.Codes.Booked, TransitWarehouseStatuses.Codes.FreightLoaded, TransitWarehouseStatuses.Codes.Departed, TransitWarehouseStatuses.Codes.Finalized, TransitWarehouseStatuses.Codes.AdjustedOut };

				packageStateFromAttachQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, notInWarehouseStatuses);
				var packageStatesFromAttach = factory.BOFactory.Load<WhsItemPackageState>(packageStateFromAttachQuery);

				if (packageStatesFromAttach.Any())
				{
					throw new DataObjectReadFailureException(Res.GetString("7147f7ca-469b-40b2-bd2e-828d2569adc2", "Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction."));
				}
			}
		}

		#endregion

		protected override void PopulateConsignmentDirection(WhsItemReceiveConsignment consignment, UniversalShipment sourceDO)
		{
			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var inboundTransportLeg = TransitUniversalHelper.GetInboundTransportLeg(factory, consignment.Warehouse, consolDO, sourceDO);
			var outboundTransportLeg = TransitUniversalHelper.GetOutboundTransportLeg(factory, consignment.Warehouse, consolDO, sourceDO);

			var twhCountryCode = consignment?.Warehouse?.RelatedCompanyBranch?.HomePort?.RL_RN_NKCountryCode ?? ZString.Empty;

			if (twhCountryCode.IsEmpty)
			{
				ConsignmentDirection = ZString.Empty;
			}

			var inboundLoadingCountryCode = inboundTransportLeg?.PortOfLoading?.Code?.Substring(0, 2) ?? ZString.Empty;
			var outboundDischargeCountryCode = outboundTransportLeg?.PortOfDischarge?.Code?.Substring(0, 2) ?? ZString.Empty;

			string splCountryCode = ZString.Empty;
			string spdCountryCode = ZString.Empty;

			if (consolDO != null)
			{
				splCountryCode = consolDO?.PortOfLoading?.Code?.Substring(0, 2) ?? ZString.Empty;
				spdCountryCode = consolDO?.PortOfDischarge?.Code?.Substring(0, 2) ?? ZString.Empty;
			}
			else if (sourceDO != null)
			{
				splCountryCode = sourceDO?.PortOfLoading?.Code?.Substring(0, 2) ?? ZString.Empty;
				spdCountryCode = sourceDO?.PortOfDischarge?.Code?.Substring(0, 2) ?? ZString.Empty;
			}

			var destination = consolDO?.PortOfDischarge?.Code ?? sourceDO?.PortOfDischarge?.Code ?? sourceDO?.PortOfDestination?.Code;
			var origin = consolDO?.PortOfLoading?.Code ?? sourceDO?.PortOfLoading?.Code ?? sourceDO?.PortOfOrigin?.Code;

			var destinationCountryCode = destination?.Substring(0, 2) ?? ZString.Empty;
			var originCountryCode = origin?.Substring(0, 2) ?? ZString.Empty;

			if (!inboundLoadingCountryCode.IsEmpty && inboundLoadingCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Import;
			}
			else if (!outboundDischargeCountryCode.IsEmpty && outboundDischargeCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Export;
			}
			else if (inboundTransportLeg is null && spdCountryCode == twhCountryCode && originCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Import;
			}
			else if ((inboundLoadingCountryCode == twhCountryCode && destinationCountryCode == twhCountryCode)
					 || (outboundDischargeCountryCode == twhCountryCode && originCountryCode == twhCountryCode)
					 || (outboundTransportLeg is null && splCountryCode == twhCountryCode && spdCountryCode == twhCountryCode))
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Domestic;
			}
			else if (outboundTransportLeg is null && splCountryCode == twhCountryCode && spdCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Export;
			}

			consignment.WRC_Direction = ConsignmentDirection;
			consignment.WRC_ExpectedArrivalTime = GetExpectedArrivalTime(inboundTransportLeg, outboundTransportLeg, sourceDO, consignment.Warehouse);
			consignment.WRC_RL_NKNextDischargePort = outboundTransportLeg?.PortOfDischarge?.GetUNLOCOAsUpperCase(factory.BOFactory) ?? sourceDO?.PortOfDestination?.Code.GetValueOrDefault() ?? "";
			consignment.WRC_ExpectedDispatchTime = GetExpectedDispatchTime(outboundTransportLeg, sourceDO, consignment.Warehouse);
		}

		protected override void PopulateDestination(WhsItemReceiveConsignment consignment, UniversalShipment sourceDO) => consignment.WRC_RL_NKDestination = sourceDO.PortOfDestination?.Code ?? string.Empty;

		protected override void ReadSubShipment(UniversalShipment shipment, ZString? masterShipmentId = null)
		{
			if (ShouldReadSubshipments)
			{
				var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipment, logger, factory, WarehouseFromSource, shouldReadSubShipments: false, masterShipmentId: masterShipmentId);
				reader.ReadIntoBusinessObject();
			}
		}

		void PopulateTransportLegs(UniversalShipment sourceDO, WhsItemReceiveConsignment consignment)
		{
			TransportLegImportHelper.DeleteExistingTransportLegs(factory, consignment.PK);

			Shipment dataObject = sourceDO;
			if (dataObject.TransportLegCollection == null || dataObject.TransportLegCollection.Count == 0)
			{
				var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
				dataObject = consolDO;
			}
			TransportLegImportHelper.ReadTransportLegs(logger, dataObject, factory, consignment, consignment.Warehouse);
		}

		void PopulateOrderReferences(UniversalShipment sourceDO, WhsItemReceiveConsignment consignment)
		{
			consignment.OrderReferences.DeleteAll();

			var orderReferencesUXML = sourceDO.LocalProcessing?.OrderNumberCollection ?? Enumerable.Empty<OrderNumber>();
			new TransitConsignmentOrderReferenceCollectionReader(orderReferencesUXML.ToArray(), consignment, logger, factory).ReadIntoCollection();
		}

		ZDateTime GetExpectedArrivalTime(TransportLeg inboundTransportLeg, TransportLeg outboundTransportLeg, UniversalShipment sourceDO, WhsWarehouse warehouse)
		{
			var expectedArrivalTime = inboundTransportLeg?.EstimatedArrival.GetValueOrDefault() ?? ZDateTime.Empty;

			if (!expectedArrivalTime.IsValid)
			{
				var isOriginWarehouse = sourceDO.PortOfOrigin != null &&
					TransitUniversalHelper.IsGivenPortARelatedOrExtraPort(warehouse, sourceDO.PortOfOrigin.Code);

				if (isOriginWarehouse && sourceDO.LocalProcessing != null)
				{
					var estimatedPickup = sourceDO.LocalProcessing.EstimatedPickup.GetValueOrDefault();
					expectedArrivalTime = estimatedPickup.IsValid ? estimatedPickup : sourceDO.LocalProcessing.PickupRequiredFrom.GetValueOrDefault();
				}
			}

			if (!expectedArrivalTime.IsValid)
			{
				expectedArrivalTime = outboundTransportLeg?.LCLReceivalCommences.GetValueOrDefault() ?? ZDateTime.Empty;
			}

			return expectedArrivalTime;
		}

		ZDateTime GetExpectedDispatchTime(TransportLeg outboundTransportLeg, UniversalShipment sourceDO, WhsWarehouse warehouse)
		{
			var expectedDispatchTime = ZDateTime.Empty;
			var isDestinationWarehouse = sourceDO.PortOfDestination != null &&
				TransitUniversalHelper.IsGivenPortARelatedOrExtraPort(warehouse, sourceDO.PortOfDestination.Code);

			if (isDestinationWarehouse && sourceDO.LocalProcessing != null)
			{
				var estimatedDelivery = sourceDO.LocalProcessing.EstimatedDelivery.GetValueOrDefault();
				expectedDispatchTime = estimatedDelivery.IsValid ? estimatedDelivery : sourceDO.LocalProcessing.DeliveryRequiredBy.GetValueOrDefault();
			}

			if (!expectedDispatchTime.IsValid && outboundTransportLeg != null)
			{
				var ctoCutOff = outboundTransportLeg.FCLCutOff.GetValueOrDefault();
				expectedDispatchTime = ctoCutOff.IsValid ? ctoCutOff : outboundTransportLeg.EstimatedDeparture.GetValueOrDefault();
			}

			return expectedDispatchTime;
		}

		protected override void AfterSuccessfullyPopulatingBizOCore()
		{
			if (Manager.IsTWX)
			{
				factory.SaveAtEndOfImport(logger);

				WhsTransitLogHelper.LogStartOfDispatchInstruction(logger);
				var dispatchConsignmentReader = new WhsTransitDispatchConsignmentDataObjectReader(Manager.Master ?? Manager.StandardOrFirstSubShipment, logger, factory);
				dispatchConsignmentReader.ReadIntoBusinessObject();
			}

			if (rcnForPostSavedAction != null && (Manager.IsTWX || TopLeveDO.IsTransitWarehouseReceive()))
			{
				var packageStates = rcnForPostSavedAction.PackageStates.Where(p => !p.IsDeleted).ToArray();
				if (packageStates.Length > 0)
				{
					StoredProcedureHandler.AddCachedPackageState([.. packageStates.Select(p => p.PK)], ProcedureType.CustomStatus);

					if (IsRecalculatePackageStateSecurityStatus)
					{
						StoredProcedureHandler.AddCachedPackageState([.. packageStates.Select(p => p.PK)], ProcedureType.SecurityStatus);
					}
				}
			}
		}

		#endregion

		#region PopulateAdditionalReferences

		protected override void PopulateConsignmentAdditionalReferencesCore(WhsItemReceiveConsignment consignment, UniversalShipment sourceDO, WhsTransitAdditionalReferencesHelper referenceHelper, List<TransitAdditionalReferenceInfo> additionalReferences)
		{
			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var consignmentPK = consignment.GetValue(WhsItemReceiveConsignmentSchema.PK);
			var outboundTransportLeg = TransitUniversalHelper.GetOutboundTransportLeg(factory, consignment.Warehouse, consolDO, sourceDO);
			var forwardingConsolDS = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol);
			var landTransportConsignmentDS = sourceDO.GetMatchingDataSource(DataContextType.LandTransportConsignment);
			var landTransportRunSheetDS = consolDO?.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheet);
			var landTransportRunSheetInstructionDS = consolDO?.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheetInstruction);
			var consignmentTableName = WhsItemReceiveConsignmentSchema.Constants.TableName;
			ZString? masterShipmentReference = null;

			if (Manager.Master != null && TransitUniversalHelper.IsMasterShipmentRepresentingAllChildShipments(Manager.Master))
			{
				masterShipmentReference = Manager.Master.WayBillNumber;
			}
			else if (!string.IsNullOrEmpty(MasterShipmentId))
			{
				masterShipmentReference = MasterShipmentId;
			}

			if (masterShipmentReference is not null)
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, masterShipmentReference);
			}

			// forwarding references
			if (IsArrivalTransitWarehouse)
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, AdditionalReferenceTypes.Codes.MasterBill, consolDO?.WayBillNumber ?? "");
			}
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, forwardingConsolDS?.Key ?? "");

			// land transport references
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, landTransportRunSheetDS?.Key ?? "");
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.RunSheetInstructionReference, landTransportRunSheetInstructionDS?.Key ?? "");
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ConsignmentNumber, landTransportConsignmentDS?.Key ?? "");

			// outbound routing references
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.Vessel, outboundTransportLeg?.VesselName);
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, outboundTransportLeg?.VoyageFlightNo);
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.CutOffDate, GetDateAsString(outboundTransportLeg?.LCLCutOff ?? ZDateTime.Empty));
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ETDDate, GetDateAsString(outboundTransportLeg?.EstimatedDeparture ?? ZDateTime.Empty));

			PopulateCustomsAdditionalReferences(consignmentPK, consignment.WRC_ConsignmentID, consignmentTableName, sourceDO, referenceHelper, additionalReferences);

			PopulateReferencesUsingMapper(consignment, referenceHelper, consignmentPK, consignmentTableName, additionalReferences);
		}

		void PopulateReferencesUsingMapper(WhsItemReceiveConsignment consignment, WhsTransitAdditionalReferencesHelper referenceHelper, ZGuid consignmentPK, string consignmentTableName, List<TransitAdditionalReferenceInfo> additionalReferences)
		{
			var warehouseBranchPK = WarehouseFromSource?.GetValue(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch) ?? Guid.Empty;
			var warehouseBranch = factory.Load<GlbBranch>(warehouseBranchPK);
			var countryCode = warehouseBranch?.GB_RN_NKCountryCode ?? "";
			var companyPK = warehouseBranch?.Company.PK;
			var referenceConverter = new ReferenceConverter(dataObject.PortReferenceCollection, dataObject.AdditionalReferenceCollection, dataObject.EntryNumberCollection);

			var mappings = WarehouseDataRegistry.Instance.TransitReferenceMapping.GetFallBackValueAtAllLevels(companyPK?.ToGuid() ?? Guid.Empty, warehouseBranch?.PK.ToGuid() ?? Guid.Empty, Guid.Empty).TransitReferenceMappingCollection.Cast<TransitReferenceMapping>();
			var mappingInfo = mappings.Select(m => new ReferenceTypeMappingInfo
			{
				FromReferenceCategory = m.SourceCategory,
				FromReferenceType = m.SourceType,
				ToReferenceCategory = m.TargetCategory,
				ToReferenceType = m.TargetType,
				Direction = m.Direction
			});

			var mapper = referenceConverter.ConvertReferences(mappingInfo, ConsignmentDirection);
			var validPortReferences = mapper.portReferences.Where(p => p.Type != null && p.Type.Code.GetValueOrDefault() == TransitWarehousePortReferenceTypes.Codes.PortAuthority
																		&& (p.Country == null || p.Country.Code.GetValueOrDefault().IsEmpty || p.Country.Code.GetValueOrDefault() == countryCode));
			if (validPortReferences.Any())
			{
				new PortReferenceCollectionReader(validPortReferences.ToArray(), consignment, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
			}

			var additionalReferenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.GetFallBackValueAtAllLevels(companyPK?.ToGuid() ?? Guid.Empty, warehouseBranch?.PK.ToGuid() ?? Guid.Empty, Guid.Empty).GetCodeDescriptionPairList().ToArray();
			var validReferenceCodes = additionalReferenceTypes.Select(c => c.Code);
			var validAdditionalReferences = mapper.additionalReferences.Where(a => a.Type != null && validReferenceCodes.Contains(a.Type.Code.GetValueOrDefault().ToString())
																			&& (a.CountryOfIssue == null || a.CountryOfIssue.Code.GetValueOrDefault().IsEmpty || a.CountryOfIssue.Code.GetValueOrDefault() == countryCode));
			var invalidAdditionalReferences = mapper.additionalReferences.Where(r => !validAdditionalReferences.Contains(r));

			if (invalidAdditionalReferences.Any())
			{
				var logHelper = new CusEntryNumberTransitLogHelper();
				logger.Log(LogType.Warning, logHelper.GetTable(Res.GetString("d66d7236-e321-41a0-afa5-0c03fd6a4f41", "The following Additional References are invalid for this Transit Warehouse and could not be imported:"), invalidAdditionalReferences, TransitLogColumnIDs.CusEntryNumberColumn.Type, TransitLogColumnIDs.CusEntryNumberColumn.Number, TransitLogColumnIDs.CusEntryNumberColumn.CountryCode));

				var noteHelper = new WhsTransitStmNoteHelper(logger, factory);
				noteHelper.AddOrUpdateUnrecognisedAdditionalReferenceNote(consignmentPK, WhsItemReceiveConsignmentSchema.Constants.TableName, invalidAdditionalReferences);
			}

			if (validAdditionalReferences.Any())
			{
				foreach (var additionalReference in validAdditionalReferences)
				{
					referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, additionalReference.Type.Code.Value, additionalReference.ReferenceNumber, countryCode: additionalReference.CountryOfIssue?.Code ?? ZString.Empty);
				}
			}

			var validEntryNumbers = mapper.entryNumbers.Where(c => c.EntryNumber.Type != null && (c.EntryNumber.Type.Code.GetValueOrDefault() == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber
																	|| c.EntryNumber.Type.Code.GetValueOrDefault() == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber)
																	&& (c.EntryNumber.CountryOfIssue == null || c.EntryNumber.CountryOfIssue.Code.GetValueOrDefault().IsEmpty || c.EntryNumber.CountryOfIssue.Code.GetValueOrDefault() == countryCode));
			if (validEntryNumbers.Any())
			{
				foreach (var additionalReference in validEntryNumbers)
				{
					referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, additionalReference.EntryNumber.Type.Code.Value, additionalReference.EntryNumber.Number, additionalReference.SourceType, TransitWarehouseReferenceCategories.Codes.CustomsReference);
				}
			}
		}

		void PopulateCustomsAdditionalReferences(ZGuid consignmentPK, ZString consignmentID, string tableName, UniversalShipment sourceDO, WhsTransitAdditionalReferencesHelper referenceHelper, List<TransitAdditionalReferenceInfo> additionalReferences)
		{
			var needPopulate = sourceDO.DataContext.GetMatchingDataSource(DataContextType.Outturn) != null
								|| sourceDO.DataContext.GetMatchingDataSource(DataContextType.AirManifestLine) != null
								|| sourceDO.DataContext.GetMatchingDataSource(DataContextType.UnderBond) != null;
			if (needPopulate)
			{
				var customsHeld = Res.GetString("11070916-dae6-4918-8d15-c09afc364efe", "Customs Held");
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, customsHeld, category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
				logger.Log(LogType.Information, ResString.GetMultilingualString("286babe4-c4e8-4891-9f04-0a3adca9dd7b", "Customs Entry Number '{0}' has been added to Receive Consignment '{1}'. It is now Held pending Customs Release.", customsHeld, consignmentID));

				if (sourceDO.ConsolidatedCargoStatus != null && sourceDO.ConsolidatedCargoStatus.GetCodeAsUpperCase().Equals(CustomsClearedCode))
				{
					var customsCleared = Res.GetString("184448ba-6665-4f95-bc17-deef1b6e5055", "Customs Cleared");
					referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, customsCleared, category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
					logger.Log(LogType.Information, ResString.GetMultilingualString("d3b28cf1-54ee-433c-bdf0-b25dfba711aa", "Customs Release Number '{0}' has been added to Receive Consignment '{1}'. It is now Cleared for Release.", customsCleared, consignmentID));
				}
			}
		}

		public static ZString GetDateAsString(ZDateTime date)
		{
			return !date.IsEmpty ? date.FormatDateTime() : "";
		}

		#endregion

		#region GetPackageJobReader

		#endregion

		#endregion

		#region GetConsigneeDocAddress

		protected override JobDocAddress GetConsigneeDocAddress(WhsItemReceiveConsignment consignment) => consignment.ConsigneeDocAddress;

		#endregion

		#region TransitJobType

		protected override string TransitJobType => TransitConstants.TransitReceiveJobType; // Event reference parameters

		#endregion

		#region PopulateAdditionalServices

		protected override IEnumerable<WhsJobService> GetRCNServices(UniversalShipment sourceDataObject, WhsItemReceiveConsignment consignment, IColumnIndexer consignmentRow) => consignment.Services.Cast<WhsJobService>();

		protected override IEnumerable<WhsJobService> GetDCNServices(WhsItemReceiveConsignment consignment) => Enumerable.Empty<WhsJobService>();

		#endregion

		#region ResStrings

		protected override ResourceString GetLogForMatching(IDataSourceDataObject dataSource)
		{
			if (!string.IsNullOrEmpty(dataSource?.Key))
			{
				return ResString.GetMultilingualString("8f0b0f66-dd52-438a-bd26-ad1c891e754a", "Searching for Receive Consignment for '{0} - {1}'.", dataSource?.Type, dataSource?.Key);
			}
			else
			{
				return ResString.GetMultilingualString("e31bb223-3492-4917-bc95-7c2bd2ce8b7e", "Searching for Receive Consignment.", dataSource?.Type, dataSource?.Key);
			}
		}

		protected override ResourceString GetJobTypeForLogs()
		{
			return ResString.GetMultilingualString("3218592b-8133-49b2-8fb3-943436c9198c", "Receive Consignment");
		}

		#endregion

		#region RegisterHandler

		protected override void RegisterHandler()
		{
			var warehouse = WarehouseFromSource == null ? null : factory.Load<WhsWarehouse>(WarehouseFromSource.GetValue(WhsWarehouseSchema.PK));
			HandlerManager.Init(logger, factory, this, warehouse);
			if (ConsolHandler == null)
			{
				var handler = HandlerManager.BuildHandler(HandlerType.Consol, TopLeveDO);
				HandlerManager.RegisterHandler(handler);
			}

			base.RegisterHandler();
		}

		#endregion
	}
}

// Sorry Mario, Your tests are in another castle!
