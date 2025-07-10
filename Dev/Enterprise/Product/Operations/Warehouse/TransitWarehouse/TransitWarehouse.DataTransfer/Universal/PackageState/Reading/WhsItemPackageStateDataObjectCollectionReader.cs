using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CollectionChangedEventArgs;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using AutoEvents = Enterprise.ZArchitecture.Business.AutoEvents;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemPackageStateDataObjectCollectionReader : DataObjectCollectionReader<PackingLine, WhsItemPackageState>
	{
		public WhsItemPackageStateDataObjectCollectionReader(DataObjectList<PackingLine> packingLines, UniversalShipment sourceDataObject, IXmlImportLogger logger,
			UniversalObjectFactory factory, IWhsTransitPackageStateBusinessObjectFinder finder, WhsItemPackageStateCollection packageStates,
			BusinessObject parent, WhsTransitPackageStatePopulateStrategy populateStrategy, WhsTransitPackageStateUpdateStrategy updateStrategy,
			ProcessType processType) : this(packingLines, sourceDataObject, logger, factory, finder, parent, populateStrategy, processType)
		{
			this.packageStates = Argument.NotNull(packageStates, "packageStates");
			this.updateStrategy = updateStrategy;
		}

		public WhsItemPackageStateDataObjectCollectionReader(DataObjectList<PackingLine> packingLines, UniversalShipment sourceDataObject, IXmlImportLogger logger,
			UniversalObjectFactory factory, IWhsTransitPackageStateBusinessObjectFinder finder, BusinessObject parent, WhsTransitPackageStatePopulateStrategy populateStrategy,
			ProcessType processType) : base(packingLines)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.finder = Argument.NotNull(finder, "finder");
			this.sourceDataObject = Argument.NotNull(sourceDataObject, "sourceDataObject");
			this.processType = processType;
			this.parent = Argument.NotNull(parent, "parent");
			this.populateStrategy = populateStrategy;
		}

		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly BusinessObject parent;
		readonly UniversalShipment sourceDataObject;
		readonly ProcessType processType;
		readonly IWhsTransitPackageStateBusinessObjectFinder finder;
		readonly WhsItemPackageStateCollection packageStates;
		readonly WhsTransitPackageStatePopulateStrategy populateStrategy;
		readonly WhsTransitPackageStateUpdateStrategy updateStrategy;

		WhsItemPackageStateDTO packageStateDTO = new WhsItemPackageStateDTO();

		public IEnumerable<WhsItemPackageState> GetPackageStates()
		{
			return packageStates;
		}

		protected override WhsItemPackageState[] BusinessObjects => finder.Find().ToArray();

		TransitDataObjectReaderHandlerManager HandlerManager
		{
			get
			{
				handlerManager ??= ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();
				return handlerManager;
			}
		}
		TransitDataObjectReaderHandlerManager handlerManager;

		bool IsSkipEntityForRCN
		{
			get
			{
				if (isSkipEntityForRCN == null)
				{
					var existingPackageStates = new UniversalObjectFactory().BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, parent.PK));
					isSkipEntityForRCN = existingPackageStates.Any(x => x.WPS_Status != TransitWarehouseStatuses.Codes.Booked);
					if ((bool)isSkipEntityForRCN)
					{
						logger.Log(LogType.Warning, Res.GetString("415835a0-be94-435a-9bbd-7b25b788cc30", "Receive Consignment has been partially received. Package level data will be ignored and not read in."));
					}
					else
					{
						logger.Log(LogType.Information, Res.GetString("04bde11e-e581-41da-a87f-0c4e3d3d7c78", "Populating Packages for {0}...", GetJobTypeForLogs()));
					}
				}
				return (bool)isSkipEntityForRCN;
			}
		}
		bool? isSkipEntityForRCN;

		protected override void AddToCollection(WhsItemPackageState businessObject)
		{
			packageStates.Add(businessObject);

			var rcn = (WhsItemReceiveConsignment)parent;
			rcn.PackageStates.Add(businessObject);
		}

		protected override WhsItemPackageState FindMatchingBusinessObject(PackingLine packingLine)
		{
			packageStateDTO = finder.Find(packingLine);
			var packageState = packageStateDTO == null ? null : GetPackageStateByPackage(packageStateDTO.WPS_KP_Package);
			return packageState;
		}

		protected override WhsItemPackageState ReadIntoBusinessObject(PackingLine packingLine, WhsItemPackageState packageState)
		{
			if (packageStateDTO != null && packageStateDTO.IsNew)
			{
				var afterReadPackageState = new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, packageState, packageStateDTO, populateStrategy, updateStrategy, parent).ReadIntoBusinessObject();
				return afterReadPackageState;
			}

			return null;
		}

		protected override void RemoveFromCollection(WhsItemPackageState businessObject)
		{
			var packlingLineContent = sourceDataObject.PackingLineCollection?.Content;
			var shouldDeletePackage = (packlingLineContent == null || packlingLineContent == CollectionContent.Complete) && !IsSkipEntityForRCN;

			if (shouldDeletePackage)
			{
				businessObject.Delete();
			}
		}

		protected override bool SkipEntity(PackingLine packingLine)
		{
			var result = false;
			switch (processType)
			{
				case ProcessType.RCN:
					result = IsSkipEntityForRCN;
					break;
				case ProcessType.DetachASNFromRCN:
					break;
				case ProcessType.ASN:
					break;
				case ProcessType.DLL:
					break;
				case ProcessType.DCN:
					break;
			}

			return result;
		}

		protected override void ReadIntoCollectionCore()
		{
			switch (processType)
			{
				case ProcessType.RCN:
					PopulatePackageStatesWhenNoPackline();
					break;
				case ProcessType.DetachASNFromRCN:
					DetachASNFromRCN();
					break;
				case ProcessType.ASN:
					ReadPackageStateForASN();
					break;
				case ProcessType.DLL:
					UpdatePackageStateForDLL();
					break;
				case ProcessType.DCN:
					UpdatePackageStateForDCN();
					break;
			}
		}

		#region ProcessPackagesForRCN

		void PopulatePackageStatesWhenNoPackline()
		{
			if (!IsSkipEntityForRCN && packageStates.IsNullOrEmpty() && (sourceDataObject.TotalNoOfPieces != null || sourceDataObject.OuterPacks != null || sourceDataObject.TotalNoOfPacks != null))
			{
				packageStateDTO = finder.Find(null);
				while (packageStateDTO != null)
				{
					var afterReadPackageState = new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, null, packageStateDTO, populateStrategy, updateStrategy, parent).ReadIntoBusinessObject();

					packageStates.Add(afterReadPackageState);

					packageStateDTO = finder.Find(null);
				}
			}

			// Skip importing data if no Packages be populated.
			if (packageStates.IsNullOrEmpty())
			{
				HandlerManager.AddPackagesByContainerLink(-1, new List<PkgPackage>());
			}
		}

		void DetachASNFromRCN()
		{
			var packlines = sourceDataObject.PackingLineCollection;
			var isFromConsol = logger.GetConsolDataObject(factory) != null;
			var shouldDeletePackages = packlines != null && packlines.Content == CollectionContent.Complete && BusinessObjects.Any();
			var shouldDetachASN = !isFromConsol && !shouldDeletePackages;

			if (shouldDetachASN)
			{
				var existingPackageStatesHavingASNs = BusinessObjects.Where(p => p.WPS_WRP_ReceiveExpectedPacking != ZGuid.Empty);

				var asnQuery = new ZQuery(WhsItemReceiveASNSchema.PK, existingPackageStatesHavingASNs.Select(p => p.WPS_WRP_ReceiveExpectedPacking));
				var asns = factory.BOFactory.Load<WhsItemReceiveASN>(asnQuery);

				var asnReferenceDic = asns.ToDictionary(asn => asn.PK, asn => asn.JobNumber);
				var packageStateBOsGroupByASN = PackageStateColumnIndexerHelper.GetPackageStateBOs(existingPackageStatesHavingASNs, factory).GroupBy(p => p.WPS_WRP_ReceiveExpectedPacking);

				foreach (var asnGroup in packageStateBOsGroupByASN)
				{
					var helper = new PackageStateTransitLogHelper();
					ZString message = ResString.GetMultilingualString("902c3486-0be4-4438-a347-127395da0c5b",
						"Detaching the following Packages from Advanced Shipping Notice {0}:", asnReferenceDic[asnGroup.Key]);
					var logMessage = helper.GetTable(message, asnGroup, TransitLogColumnIDs.PackageStateColumn.Package
						, TransitLogColumnIDs.PackageStateColumn.RCN);
					logger.Log(LogType.Information, logMessage);

					asnGroup.ForEach(packageStateInASN =>
					{
						new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, packageStateInASN, new WhsItemPackageStateDTO(), populateStrategy, updateStrategy, asns.FirstOrDefault()).ReadIntoBusinessObject();
					});
				}
			}
		}

		#endregion

		#region ProcessTypeDLL

		void UpdatePackageStateForDLL()
		{
			var dll = (WhsItemDispatchLoadList)parent;
			var loadListPK = dll.PK;
			var packagesRemovedFromThisLoadList = GetRemovedPackages(loadListPK, BusinessObjects);
			var packagesOnOtherLoadLists = BusinessObjects.Where(p => p.WPS_WDL_LoadList.IsValid && p.WPS_WDL_LoadList != loadListPK).ToArray();

			if (packagesOnOtherLoadLists.Any())
			{
				AddToCollection(packagesOnOtherLoadLists.Select(p => p.WPS_WDL_LoadList));
			}

			AddToCollection(packagesRemovedFromThisLoadList);

			var packagesToReassignFromLoadLists = packagesRemovedFromThisLoadList.Append(packagesOnOtherLoadLists);
			CheckPackagesCanBeDetached(packagesToReassignFromLoadLists);

			var logHelper = new PackageStateColumnIndexerTransitLogHelper(factory);
			var detachedPackageLogMessage = logHelper.GetTable(
				Res.GetString("d8090d6a-4650-4df1-b5e8-d7d9fdc4a232", "The following packages have been detached from their Load Lists:"),
				packagesToReassignFromLoadLists,
				TransitLogColumnIDs.PackageStateIndexerColumn.Package,
				TransitLogColumnIDs.PackageStateIndexerColumn.LoadList,
				TransitLogColumnIDs.PackageStateIndexerColumn.RCN,
				TransitLogColumnIDs.PackageStateIndexerColumn.DCN);

			TransitUniversalHelper.RemovePackagesFromLoadList(packagesRemovedFromThisLoadList, dll, sourceDataObject, factory, logger);

			var groupedPackageStateRows = packagesOnOtherLoadLists.ToKeyListDictionary(p => p.WPS_WDL_LoadList);
			var loadListsToDetachQuery = new ZQuery(WhsItemDispatchLoadListSchema.PK, groupedPackageStateRows.Keys);
			var loadListsToDetach = factory.BOFactory.Load<WhsItemDispatchLoadList>(loadListsToDetachQuery).ToArray();
			foreach (var loadList in loadListsToDetach)
			{
				var packageStateGrouping = groupedPackageStateRows[loadList.PK];
				TransitUniversalHelper.RemovePackagesFromLoadList(packageStateGrouping, loadList, sourceDataObject, factory, logger);
			}

			// updating rows where the BizO is already loaded in the same factory causes issues
			// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
			var loadLists = factory.BOFactory.Load<WhsItemDispatchLoadList>(
				new ZQuery(WhsItemDispatchLoadListSchema.PK, loadListsToDetach.Select(l => l.GetValue(WhsItemDispatchLoadListSchema.PK))));
			loadLists.ForEach(l =>
			{
				l.HasChanges = true;
			});

			if (!detachedPackageLogMessage.IsEmpty)
			{
				logger.Log(LogType.Information, detachedPackageLogMessage);
			}

			// Attach packages to Load List
			BusinessObjects.ForEach(packageState =>
			{
				new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, packageState, new WhsItemPackageStateDTO(), populateStrategy, updateStrategy, parent).ReadIntoBusinessObject();
			});

			var attachedPackageLogMessage = logHelper.GetTable(
					Res.GetString("1ae6c16f-f1d3-4714-aa93-f9dabbd87584", "The following packages have been attached to Load List {0}:", dll.WDL_JobID),
					BusinessObjects,
					TransitLogColumnIDs.PackageStateIndexerColumn.Package,
					TransitLogColumnIDs.PackageStateIndexerColumn.RCN,
					TransitLogColumnIDs.PackageStateIndexerColumn.DCN);
			if (!attachedPackageLogMessage.IsEmpty)
			{
				logger.Log(LogType.Information, attachedPackageLogMessage);
			}
		}

		void CheckPackagesCanBeDetached(IEnumerable<IColumnIndexer> packageStates)
		{
			var loadCompleteDTUs = TransitUniversalHelper.GetLoadCompleteDTUs(packageStates, factory).ToArray();
			if (loadCompleteDTUs.Any())
			{
				var loadCompleteDTUHashSet = loadCompleteDTUs.Select(d => d.PK).ToHashSet();
				var packagesOnLoadCompleteDTUs = packageStates.Where(p => loadCompleteDTUHashSet.Contains(p.GetValue(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader)));
				var packageStateBOsOnLoadCompleteDTUs = factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, packagesOnLoadCompleteDTUs.Select(p => p.GetValue(WhsItemPackageStateSchema.PK))));

				var tableMessage = Res.GetString("a3f2294f-e749-47a3-9444-2eb09ee02936",
@"Attempting to reassign Packages from Load Lists for Dispatch Transportation Units that have already been closed.
These Packages can only be removed from their current Load List via Transit Warehouse Desktop:");
				var logHelper = new PackageStateTransitLogHelper();
				var errorMessage = logHelper.GetTable(tableMessage, packageStateBOsOnLoadCompleteDTUs, TransitLogColumnIDs.PackageStateColumn.Package, TransitLogColumnIDs.PackageStateColumn.LoadList, TransitLogColumnIDs.PackageStateColumn.DTU);

				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		IEnumerable<WhsItemPackageState> GetRemovedPackages(ZGuid loadListPK, IReadOnlyCollection<IColumnIndexer> plannedPackageStates)
		{
			var packagesRemovedQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			packagesRemovedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDL_LoadList, loadListPK);
			packagesRemovedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_IsHandlingUnit, ZBool.False);

			if (plannedPackageStates.Any())
			{
				var notInQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.PK, notIn: true);
				notInQuery.AddToFilter(WhsItemPackageStateSchema.PK, plannedPackageStates.Select(p => p.GetValue(WhsItemPackageStateSchema.PK)));
				packagesRemovedQuery.AddSubQuery(notInQuery, JoinCondition.And);
			}

			var packagesShouldRemovedForPlanned = factory.BOFactory.Load<WhsItemPackageState>(packagesRemovedQuery);
			var dcnsNotAuthorized = TransitUniversalHelper.GetDCNsNotAuthorizedByPackagesOnLoadList(packagesShouldRemovedForPlanned, factory);

			if (dcnsNotAuthorized.Any())
			{
				var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
				query.AddToFilter(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, SQLComparisonOperator.NotEqual, dcnsNotAuthorized.Select(d => d.GetValue(WhsItemDispatchConsignmentSchema.PK)));
				query.AddToFilter(WhsItemPackageStateSchema.PK, packagesShouldRemovedForPlanned.Select(p => p.GetValue(WhsItemPackageStateSchema.PK)));

				var packagesRemoved = factory.BOFactory.Load<WhsItemPackageState>(query);
				return packagesRemoved;
			}

			return packagesShouldRemovedForPlanned;
		}

		#endregion

		#region ProcessTypeDCN

		void UpdatePackageStateForDCN()
		{
			if (populateStrategy == WhsTransitPackageStatePopulateStrategy.Attach)
			{
				AttachPackagesToDCN();
			}
			else if (populateStrategy == WhsTransitPackageStatePopulateStrategy.Update)
			{
				SetPackageStateIsHighRiskAndExternalReference();
			}
			else if (populateStrategy == WhsTransitPackageStatePopulateStrategy.Detach)
			{
				DelinkPackageStates();
			}
		}

		#region AttachPackagesToDCN

		void AttachPackagesToDCN()
		{
			var dispatchConsignment = (WhsItemDispatchConsignment)parent;
			var packageStatesToDispatch = finder.Find();
			var processedPackageStates = new HashSet<WhsItemPackageState>();
			if (packageStatesToDispatch != null && packageStatesToDispatch.Any())
			{
				var loadListPKs = new HashSet<ZGuid>();
				var packageStatePKsHaveNoRCN = new HashSet<ZGuid>();

				foreach (var packageState in packageStatesToDispatch)
				{
					if (packageState.WPS_IsHandlingUnit)
					{
						var childPackageStates = PackageStateColumnIndexerHelper.GetAllHandlingUnitChildPackages(packageState, factory);
						foreach (var childPackageState in childPackageStates)
						{
							CollectPackageStateInfoForValidation(loadListPKs, childPackageState);

							var childPackageStatePK = childPackageState.PK;
							if (childPackageState.WPS_WRC_TransitReceiveConsignment.IsValid)
							{
								if (AttachPackageStateToDCN(childPackageState, dispatchConsignment))
								{
									processedPackageStates.Add(childPackageState);
								}
							}
							else
							{
								packageStatePKsHaveNoRCN.Add(childPackageStatePK);
							}
						}
						if (packageState.WPS_UnitType == PackageStateUnitType.Codes.Overpack)
						{
							if (AttachPackageStateToDCN(packageState, dispatchConsignment))
							{
								processedPackageStates.Add(packageState);
							}
						}
					}
					else
					{
						CollectPackageStateInfoForValidation(loadListPKs, packageState);

						if (packageState.WPS_WRC_TransitReceiveConsignment.IsValid)
						{
							if (AttachPackageStateToDCN(packageState, dispatchConsignment))
							{
								processedPackageStates.Add(packageState);
							}
						}
						else
						{
							var packageStatePK = packageState.PK;
							packageStatePKsHaveNoRCN.Add(packageStatePK);
						}
					}
				}

				CheckForLoadLists(dispatchConsignment, loadListPKs);

				var logHelper = new PackageStateColumnIndexerTransitLogHelper(factory);

				if (packageStatePKsHaveNoRCN.Count > 0)
				{
					var packageStatesHaveNoRCN = PackageStateColumnIndexerHelper.GetPackageStateIndexers(packageStatePKsHaveNoRCN, factory);

					var errorMessageStringBuilder = new ZStringBuilder();
					errorMessageStringBuilder.Append(
						logHelper.GetTable(
							Res.GetString("9c9128c8-d130-41c4-b667-7653976c17c9", "The following Packages were matched but they were received blind into the Transit Warehouse and have yet to be attached to a Receive Consignment:"),
							packageStatesHaveNoRCN,
							TransitLogColumnIDs.PackageStateIndexerColumn.Package,
							TransitLogColumnIDs.PackageStateIndexerColumn.RCN,
							TransitLogColumnIDs.PackageStateIndexerColumn.DCN,
							TransitLogColumnIDs.PackageStateIndexerColumn.LoadList,
							TransitLogColumnIDs.PackageStateIndexerColumn.Status)
						);
					errorMessageStringBuilder.AppendLine(System.Environment.NewLine + Res.GetString("73a4e712-f859-4b60-8699-58f66564eacf", "Packages without a Receive Consignment cannot be dispatched from a Transit Warehouse. The Receive process must be completed via Transit Warehouse."));

					throw new DataObjectReadFailureException(errorMessageStringBuilder.ToString().TrimEnd());
				}

				(var packLinesWithoutIds, var packLinesWithoutIdsByType) = WhsTransitPackageStateBusinessObjectFinderForDCN.GetPackLinesWithoutIdsAndGroupedByType(sourceDataObject, HandlerManager);

				LogAttachedPackages(dispatchConsignment, processedPackageStates, packLinesWithoutIdsByType);
				processedPackageStates.ForEach(c => HandlerManager.AddPackagesAlreadyOnDCN(c.PK));
			}
		}

		void CollectPackageStateInfoForValidation(HashSet<ZGuid> loadListPks, WhsItemPackageState packageStates)
		{
			var loadListPK = packageStates.WPS_WDL_LoadList;
			if (loadListPK != ZGuid.Empty)
			{
				loadListPks.Add(loadListPK);
			}
		}

		void CheckForLoadLists(WhsItemDispatchConsignment dispatchConsignment, HashSet<ZGuid> loadListPKs)
		{
			if (loadListPKs.Any())
			{
				var dllQuery = new ZQuery(WhsItemDispatchLoadListSchema.PK, loadListPKs.ToArray());
				dllQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_IsReadyToStage, true);
				var dllsReadytoStage = factory.Load<WhsItemDispatchLoadList>(dllQuery);
				if (dllsReadytoStage.Any())
				{
					logger.Log(LogType.Warning, Res.GetString("e10f52ad-5cb7-4b6e-8d02-78f59bb85877", "Stopping Load Lists for Dispatch Consignment {0}.", WhsTransitLogHelper.FormatCodeDescriptionForLogging(WhsItemDispatchConsignmentSchema.WDC_JobID.ToString(), WhsItemDispatchConsignmentSchema.WDC_ConsignmentID.ToString())));
					foreach (var loadList in dllsReadytoStage)
					{
						LoadListController.StopLoadList(factory, logger, loadList, sourceDataObject);

						// updating rows where the BizO is already loaded in the same factory causes issues
						// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
						loadList.HasChanges = true;
					}
				}
			}
		}

		void LogAttachedPackages(IColumnIndexer dispatchConsignmentRow, HashSet<WhsItemPackageState> packageStatesToLog, IEnumerable<(ZString Type, long Qty)> typeAndQty = null)
		{
			var logHelper = new PackageStateColumnIndexerTransitLogHelper(factory);
			var tableMessage = Res.GetString("13a2bd8b-7512-4088-b317-daab0fa64f63", "The following packages have been attached to Dispatch Consignment {0}:", dispatchConsignmentRow.GetValue(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID));
			if (typeAndQty == null || !typeAndQty.Any())
			{
				tableMessage = logHelper.GetTable(tableMessage,
					packageStatesToLog,
					TransitLogColumnIDs.PackageStateIndexerColumn.Package,
					TransitLogColumnIDs.PackageStateIndexerColumn.RCN);
			}
			else
			{
				tableMessage = logHelper.GetTable(tableMessage,
					packageStatesToLog,
					typeAndQty,
					TransitLogColumnIDs.PackageStateIndexerColumn.UxmlPackage,
					TransitLogColumnIDs.PackageStateIndexerColumn.Package,
					TransitLogColumnIDs.PackageStateIndexerColumn.RCN);
			}
			logger.Log(LogType.Information, tableMessage);
		}

		#endregion

		void SetPackageStateIsHighRiskAndExternalReference()
		{
			foreach (var keyValuePair in HandlerManager.GetMatchedPackagePKAndPackingLine())
			{
				var packageStateBO = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, keyValuePair.Key)).FirstOrDefault();

				if (packageStateBO != null)
				{
					var packageStateDTO = new WhsItemPackageStateDTO();
					if (packageStateBO != null && keyValuePair.Value.IsHighRisk.HasValue && (packageStateBO.WPS_IsHighRisk != keyValuePair.Value.IsHighRisk))
					{
						packageStateDTO.WPS_IsHighRisk = keyValuePair.Value.IsHighRisk;
						packageStateBO.HasChanges = true;
					}

					var package = packageStateBO.Package;
					if (package != null && package.KP_ExternalReference.IsEmpty && keyValuePair.Value.PackingLineID.HasValue && !keyValuePair.Value.PackingLineID.Value.IsEmpty)
					{
						packageStateDTO.KP_ExternalReference = (ZString)keyValuePair.Value.PackingLineID;
						package.HasChanges = true;
					}

					new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, packageStateBO, packageStateDTO, WhsTransitPackageStatePopulateStrategy.Update, WhsTransitPackageStateUpdateStrategy.PartialUpdate, parent).ReadIntoBusinessObject();
				}
			}
		}

		#region DetachPackageStates

		void DelinkPackageStates()
		{
			var dcn = (WhsItemDispatchConsignment)parent;
			var existingPackageStates = finder.Find();
			var processedPackageStates = HandlerManager.GetPackagesAlreadyOnDCN();

			var delinkedPackageStatePKs = new HashSet<ZGuid>();
			var hasConsolDO = sourceDataObject.GetMatchingDataSource(DataContextType.ForwardingConsol) != null;

			// detach packages that were not updated
			foreach (var existingPackageStateBizo in existingPackageStates)
			{
				if (!processedPackageStates.Contains(existingPackageStateBizo.PK))
				{
					if (existingPackageStateBizo.WPS_Status.ToString().IsPackageDeparted())
					{
						throw new DataObjectReadFailureException(Res.GetString("bda17e49-ef0f-4a0c-b30b-2aa47f69cef2", "At least one package is departed already. Therefore cannot be removed from dispatch consignment."));
					}
					else
					{
						var loadCompleteDTUs = TransitUniversalHelper.GetLoadCompleteDTUs(existingPackageStates, factory).ToArray();
						if (!loadCompleteDTUs.Any(d => d.PackageStates.Contains(existingPackageStateBizo)))
						{
							new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, existingPackageStateBizo, null, WhsTransitPackageStatePopulateStrategy.Detach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, parent).ReadIntoBusinessObject();
							delinkedPackageStatePKs.Add(existingPackageStateBizo.PK);
						}
					}
				}
			}
			LogDetachedPackages(dcn, delinkedPackageStatePKs);
		}

		void LogDetachedPackages(IColumnIndexer consignmentRow, HashSet<ZGuid> linkedPackageStatePKs)
		{
			var linkedPackageStates = PackageStateColumnIndexerHelper.GetPackageStateIndexers(linkedPackageStatePKs, factory);
			if (linkedPackageStates.Any())
			{
				var consignmentId = consignmentRow.GetValue(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID);
				var logHelper = new PackageStateColumnIndexerTransitLogHelper(factory);
				var message = logHelper.GetTable(
					Res.GetString("4cdda101-f476-4339-9582-c0f1465e8973", "The following packages have been detached from their Dispatch Consignments and their Load Lists:"),
					linkedPackageStates,
					TransitLogColumnIDs.PackageStateIndexerColumn.Package,
					TransitLogColumnIDs.PackageStateIndexerColumn.RCN,
					TransitLogColumnIDs.PackageStateIndexerColumn.DCN,
					TransitLogColumnIDs.PackageStateIndexerColumn.LoadList,
					TransitLogColumnIDs.PackageStateIndexerColumn.Status);
				logger.Log(LogType.Information, message);
				AddPackageStateDetachedEvents(linkedPackageStatePKs, consignmentId);
			}
		}

		void AddPackageStateDetachedEvents(HashSet<ZGuid> packageStatePKs, ZString consignmentId)
		{
			var packageStateSubQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_KP_Package);
			packageStateSubQuery.AddToFilter(WhsItemPackageStateSchema.PK, packageStatePKs);

			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			packageQuery.AddSubQuery(packageStateSubQuery, JoinCondition.And);

			var packages = factory.BOFactory.Load<PkgPackage>(packageQuery);
			packages.ForEach(p => WhsTransitLogHelper.AddStmALogToBizoObject(
				p,
				AutoEvents.Detached,
				Res.GetString("bd3ecd67-4ff9-4727-9e95-f9fb62e77144", "Package {0} has been detached from dispatch consignment {1}.", p.KP_PackageID, consignmentId)));
		}

		bool AttachPackageStateToDCN(WhsItemPackageState packageState, WhsItemDispatchConsignment dcn)
		{
			if (packageState != null)
			{
				// updating rows where the BizO is already loaded in the same factory causes issues
				// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
				new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, packageState, null, WhsTransitPackageStatePopulateStrategy.Attach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, dcn).ReadIntoBusinessObject();
				packageState.HasChanges = true;
				return true;
			}
			return false;
		}

		#endregion

		#endregion

		#region ProcessTypeASN

		void ReadPackageStateForASN()
		{
			if (populateStrategy == WhsTransitPackageStatePopulateStrategy.Attach)
			{
				BusinessObjects.ForEach(packageState =>
				{
					new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, packageState, new WhsItemPackageStateDTO(), populateStrategy, updateStrategy, parent).ReadIntoBusinessObject();
				});

				var helper = new PackageStateTransitLogHelper();
				var asn = (WhsItemReceiveASN)parent;
				var message = ResString.GetMultilingualString("73488e04-8227-4519-8730-3852dd2b7302"
					, "Attaching the following packages to Advanced Shipping Notice {0}:", asn.JobNumber);
				var logMessage = helper.GetTable(message, BusinessObjects,
					TransitLogColumnIDs.PackageStateColumn.Package,
					TransitLogColumnIDs.PackageStateColumn.RCN);
				logger.Log(LogType.Information, logMessage);
			}
			else if (populateStrategy == WhsTransitPackageStatePopulateStrategy.Detach)
			{
				var arrivedPackageStates = BusinessObjects.Where(ps => !ps.WPS_WRH_TransitReceiveHeader.IsEmpty);
				if (arrivedPackageStates.Any())
				{
					var asn = (WhsItemReceiveASN)parent;
					var helper = new PackageStateTransitLogHelper();
					var message = ResString.GetMultilingualString("c861011e-7e56-4282-be20-1dbd53c6a790", "Cannot attach the following packages to Advanced Shipping Notice {0}. These Packages were already received into the Warehouse. System cannot create a Receive Instruction for this ASN.", asn.WRP_ReferenceNumber);
					var arrivedPackageStateBOs = PackageStateColumnIndexerHelper.GetPackageStateBOs(arrivedPackageStates, factory);
					var logMessage = helper.GetTable(message, arrivedPackageStateBOs, TransitLogColumnIDs.PackageStateColumn.Package, TransitLogColumnIDs.PackageStateColumn.RCN, TransitLogColumnIDs.PackageStateColumn.Status);
					throw new DataObjectReadFailureException(logMessage);
				}

				if (!sourceDataObject.IsFromDataSource(DataContextType.GateBooking))
				{
					BusinessObjects.ForEach(packageState =>
					{
						new WhsItemPackageStateDataObjectReader(sourceDataObject, logger, factory, packageState, new WhsItemPackageStateDTO(), populateStrategy, updateStrategy, parent).ReadIntoBusinessObject();
					});
				}
			}
		}

		#endregion

		protected ResourceString GetJobTypeForLogs()
		{
			return ResString.GetMultilingualString("3218592b-8133-49b2-8fb3-943436c9198c", "Receive Consignment");
		}

		WhsItemPackageState GetPackageStateByPackage(ZGuid packagePK)
		{
			var query = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packagePK);
			query.MaximumRows = 1;

			var packageStates = factory.BOFactory.Load<WhsItemPackageState>(query);
			return packageStates.FirstOrDefault();
		}

		#region CollectionChanged

		public event EventHandler<CollectionChangedEventArgs> CollectionChanged;

		public void AddToCollection(IEnumerable<ZGuid> collection)
		{
			OnCollectionChanged(collection, CollectionType.DetachedLoadList);
		}

		public void AddToCollection(IEnumerable<WhsItemPackageState> collection)
		{
			OnCollectionChanged(collection);
		}

		void OnCollectionChanged(IEnumerable<ZGuid> collection, CollectionType type)
		{
			CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(collection, type));
		}

		void OnCollectionChanged(IEnumerable<WhsItemPackageState> collection)
		{
			CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(collection));
		}

		public void SubscribeToCollectionChanges(EventHandler<CollectionChangedEventArgs> handler)
		{
			CollectionChanged += handler;
		}

		#endregion
	}
}