using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class LoadListController
	{
		public static void StopLoadList(UniversalObjectFactory factory, IXmlImportLogger logger, WhsItemDispatchLoadList loadList, UniversalShipment dataObject, bool isStopLoad = false)
		{
			var isLoadListStarted = loadList.WDL_IsReadyToStage;
			if (isLoadListStarted)
			{
				var loadListPK = loadList.PK.ToGuid();
				var loadListNumber = loadList.WDL_JobID;

				var isAllPackagesDeparted = factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName,
				new ZQuery(WhsItemPackageStateSchema.WPS_WDL_LoadList, loadListPK)).All(p => p[WhsItemPackageStateSchema.Constants.WPS_Status].ToString().IsPackageDeparted());

				if (isAllPackagesDeparted && !isStopLoad)
				{
					logger.Log(LogType.Warning, Res.GetString("c8aea4d0-809f-4da1-8194-8f7f2ba4f388", "Load List {0} cannot be stopped as it has already departed.", loadListNumber));
				}
				else
				{
					var incompleteTransferLines = GetIncompleteTransferLines(factory, loadListPK);

					foreach (var transferLine in incompleteTransferLines)
					{
						var isPicked = transferLine[WhsItemTransferLineSchema.Constants.WTF_PickTime] != DBNull.Value;
						if (!isPicked)
						{
							var packageStatePK = (Guid)transferLine[WhsItemTransferLineSchema.Constants.WTF_WPS_PackageState];
							var packageState = factory.RowFactory.LoadFromPK(WhsItemPackageStateSchema.Constants.TableName, packageStatePK);
							transferLine.Delete();
						}
					}

					FinaliseCompletedTransfers(loadListPK, loadListNumber, factory, logger);

					loadList.SetValue(WhsItemDispatchLoadListSchema.WDL_IsReadyToStage, false, logger);
					logger.Log(LogType.Information, Res.GetString("9611ec77-0804-4af9-bdbf-316caaa59131", "Load List {0} has been stopped.", loadListNumber));

					UpdateLoadListPackagesStatus(loadListPK, factory, logger);

					if (!isStopLoad)
					{
						AddLoadListSuspendEvent(factory, logger, loadList, dataObject);
					}
				}
			}
		}

		static void UpdateLoadListPackagesStatus(ZGuid loadListPK, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var packageStatesQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			packageStatesQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDL_LoadList, loadListPK);
			packageStatesQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader, null);
			packageStatesQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader, SQLComparisonOperator.NotEqual, null);
			var packageStates = factory.BOFactory.Load<WhsItemPackageState>(packageStatesQuery);

			PackageStateStatusHelper.UpdatePackageStatesStatus(packageStates, factory, logger);
		}

		static void FinaliseCompletedTransfers(ZGuid loadListPK, string loadListNumber, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var transfers = GetTransfersForLoadList(loadListPK, factory);

			foreach (var transfer in transfers)
			{
				var transferLineQuery = new ZQuery(WhsItemTransferLineSchema.WTF_WTH_TransitTransferHeader, transfer[WhsItemTransferHeaderSchema.Constants.PK]);
				var allTransferLinesCompleted = factory.RowFactory.Load(WhsItemTransferLineSchema.Constants.TableName, transferLineQuery).All(l => l[WhsItemTransferLineSchema.Constants.WTF_PutTime] != DBNull.Value);

				if (allTransferLinesCompleted)
				{
					var transferReference = transfer[WhsItemTransferHeaderSchema.Constants.WTH_ReferenceNumber];
					var columnIndexerForTransfer = DataObjectReader.GetColumnIndexerFromRow(transfer);
					columnIndexerForTransfer.SetValue(WhsItemTransferHeaderSchema.WTH_IsFinalised, true, logger);
					logger.Log(LogType.Information, Res.GetString("c0101b58-1f5f-417a-8779-8bc92397d912", "Finalized transfer {0} when stopping Load List {1}.", transferReference, loadListNumber));
				}
			}
		}

		static DataRow[] GetTransfersForLoadList(ZGuid loadListPK, UniversalObjectFactory factory)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(WhsItemTransferHeader));
			headerQuery.AddToFilter(JoinCondition.And, WhsItemTransferHeaderSchema.WTH_IsFinalised, false);
			headerQuery.AddToFilter(JoinCondition.And, WhsItemTransferHeaderSchema.WTH_TransferType, new[] { TransferTypes.Codes.PIC, TransferTypes.Codes.XDK });
			var transferLinesSubQuery = new ZDBOnlySubQuery(typeof(WhsItemTransferLine), WhsItemTransferLineSchema.WTF_WTH_TransitTransferHeader, WhsItemTransferHeaderSchema.PK);

			var packageStateQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.PK, WhsItemTransferLineSchema.WTF_WPS_PackageState);
			packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDL_LoadList, loadListPK);
			transferLinesSubQuery.AddSubQuery(packageStateQuery, JoinCondition.And);
			headerQuery.AddSubQuery(transferLinesSubQuery, JoinCondition.And);

			var transfers = factory.RowFactory.Load(WhsItemTransferHeaderSchema.Constants.TableName, headerQuery);
			return transfers;
		}

		static DataRow[] GetIncompleteTransferLines(UniversalObjectFactory factory, Guid loadListPK)
		{
			var incompleteTransferLineQuery = new ZDBOnlyQuery(typeof(WhsItemTransferLine));

			var headerSubQuery = new ZDBOnlySubQuery(typeof(WhsItemTransferHeader), WhsItemTransferHeaderSchema.PK, WhsItemTransferLineSchema.WTF_WTH_TransitTransferHeader);
			headerSubQuery.AddToFilter(JoinCondition.And, WhsItemTransferHeaderSchema.WTH_IsFinalised, false);
			headerSubQuery.AddToFilter(JoinCondition.And, WhsItemTransferHeaderSchema.WTH_TransferType, new[] { TransferTypes.Codes.PIC, TransferTypes.Codes.XDK });

			var packageStateQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.PK, WhsItemTransferLineSchema.WTF_WPS_PackageState);
			packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDL_LoadList, loadListPK);

			incompleteTransferLineQuery.AddSubQuery(headerSubQuery, JoinCondition.And);
			incompleteTransferLineQuery.AddSubQuery(packageStateQuery, JoinCondition.And);
			incompleteTransferLineQuery.AddToFilter(WhsItemTransferLineSchema.WTF_PutTime, null);

			var transferLinesNotYetPutaway = factory.RowFactory.Load(WhsItemTransferLineSchema.Constants.TableName, incompleteTransferLineQuery);
			return transferLinesNotYetPutaway;
		}

		static void AddLoadListSuspendEvent(UniversalObjectFactory factory, IXmlImportLogger logger, WhsItemDispatchLoadList loadList, UniversalShipment dataObject)
		{
			var suspendEventTYP = ResString.GetMultilingualString("729dcdf6-76c9-4b00-aaca-69c94ff36d9c", "Load List");
			var suspendEventRES = ResString.GetMultilingualString("8e3bb1ea-7798-4d60-bd6f-c6d7015fcb69", "Modifying via UXML");
			var suspendEventDEP_Forwarding = ResString.GetMultilingualString("e881b817-013d-4304-abaa-d346c93187e3", "Forwarder");
			var suspendEventREP_Runsheet = ResString.GetMultilingualString("1f88e77d-25c4-4ab3-9958-53f6d3d60f95", "Transport Company");

			var dataSourceCollection = dataObject?.DataContext?.DataSourceCollection;
			var isFromForwarding = dataSourceCollection.IsFromForwardingConsol() || dataSourceCollection.IsFromForwardingShipment();
			var isFromLocalRunSheet = dataSourceCollection.IsFromLocalTransportRunSheet();

			var department = (isFromForwarding ? suspendEventDEP_Forwarding : null) ?? (isFromLocalRunSheet ? suspendEventREP_Runsheet : null) ?? "";
			var warehouseFromSource = DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(WhsWarehouseSchema.Constants.TableName, GetWarehouseQuery(loadList.WDL_WW_Warehouse.ToGuid())).SingleOrDefault());
			var warehouseUnloco = GetWarehouseUnlocoByPK(factory, warehouseFromSource);

			var parameters = WhsTransitLogHelper.GetEventReferenceString(
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, suspendEventTYP),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Location, warehouseUnloco),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Facility, Facilities.Code.Depot),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, suspendEventRES),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, department),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Warehouse, warehouseFromSource?.GetValue(WhsWarehouseSchema.WW_WarehouseCode) ?? string.Empty));

			var reference = loadList[WhsItemDispatchLoadListSchema.Constants.WDL_JobID] + parameters;
			var loadListPK = loadList.PK.ToGuid();

			WhsTransitLogHelper.AddStmALog(factory, logger, loadListPK, WhsItemDispatchLoadListSchema.Constants.TableName, reference, Events.ServiceSuspendedCode);
		}

		static ZString GetWarehouseUnlocoByPK(UniversalObjectFactory factory, IColumnIndexer warehouseFromSource)
		{
			var addressCode = "";

			var warehouseAddressPK = warehouseFromSource?.GetValue(WhsWarehouseSchema.WW_OA_WarehouseAddress);
			if (warehouseAddressPK != null)
			{
				var columnIndexerQuery = new ZQuery(OrgAddressSchema.PK, warehouseAddressPK);
				var address = Array.ConvertAll(factory.RowFactory.Load(OrgAddressSchema.Constants.TableName, columnIndexerQuery), DataObjectReader.GetColumnIndexerFromRow).FirstOrDefault();
				addressCode = address?.GetValue(OrgAddressSchema.OA_RL_NKRelatedPortCode) ?? "";
			}

			return addressCode;
		}

		static ZQuery GetWarehouseQuery(Guid warehousePK)
		{
			var warehouseQuery = new ZQuery(WhsWarehouseSchema.PK, warehousePK);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);

			return warehouseQuery;
		}
	}
}
