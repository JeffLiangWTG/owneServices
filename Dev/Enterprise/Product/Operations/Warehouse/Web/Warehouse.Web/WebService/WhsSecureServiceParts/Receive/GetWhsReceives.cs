using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetWhsReceives

		[WebMethod(Description = "Get Receive Data By Reference")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketsWebServiceResponse GetWhsReceives(string reference, bool isUnloadProcess)
		{
			return HandleWebServiceRequest<WhsDocketsWebServiceResponse>(r => GetWhsReceivesFromReference(r, reference, isUnloadProcess));
		}

		[WebMethod(Description = "Get Receive Data By PK")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsDocketsWebServiceResponse GetWhsReceive(Guid pk, bool isUnloadProcess)
		{
			return HandleWebServiceRequest<WhsDocketsWebServiceResponse>(r => GetWhsReceiveFromPK(r, pk, isUnloadProcess));
		}

		void GetWhsReceiveFromPK(WhsDocketsWebServiceResponse response, Guid pk, bool isUnloadProcess)
		{
			var additionalFilter = new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, SQLComparisonOperator.Equal, null);
			var dockets = Factory.Load<WhsReceive>(WebServiceHelper.GetUnfinalisedDocketByPKQuery(Factory, SecurityHeader.WarehouseCode, pk, additionalFilter))
				?? Array.Empty<WhsReceive>();

			if (!dockets.Any() && isUnloadProcess)
			{
				var package = Factory.Load<PkgPackage>(pk);
				if (package != null)
				{
					UpdateReturnReceive(response, package);
				}

				if (response.Dockets == null)
				{
					response.Dockets = Array.Empty<WhsDocketInfo>();
				}

				WhsReceiveHelper.SetSingleDockDoorLocationDetails(Factory, response, SecurityHeader.WarehouseCode);
			}
			else
			{
				GetWhsReceivesCore(response, dockets, isUnloadProcess);
			}
		}

		void GetWhsReceivesFromReference(WhsDocketsWebServiceResponse response, string reference, bool isUnloadProcess)
		{
			var receives = Array.Empty<WhsReceive>();
			if (reference.IsNullOrEmpty())
			{
				receives = HandleEmptyReference(response);
			}

			if(response.NoError())
			{
				if (receives.Length == 0 && reference.Length <= WhsDocketSchema.WD_ExternalReference.MaxLength)
				{
					receives = LoadWhsDockets<WhsReceive>(reference, Res.GetString("51b5481e-96b8-4145-b042-7b51f9fe0ac0", "Receive"), DocketType.Codes.Receive);
				}

				if (response.NoError())
				{
					if (receives.Length == 0 && isUnloadProcess)
					{
						GetWhsReceiveFromPackageReference(response, reference);
						WhsReceiveHelper.SetSingleDockDoorLocationDetails(Factory, response, SecurityHeader.WarehouseCode);
					}
					else
					{
						GetWhsReceivesCore(response, receives, isUnloadProcess);
					}
				}
			}
		}

		WhsReceive[] HandleEmptyReference(WhsDocketsWebServiceResponse response)
		{
			var receives = Array.Empty<WhsReceive>();
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (!warehouse.WW_GG_ReleaseGroup.IsValid)
			{
				response.LogBusinessValidationError(Res.GetString("DFB79914-16CD-4988-86D6-4EFB94D1F6D8", "Please provide Receive reference."));
			}
			else
			{
				receives = GetNextUnloadTaskReceives(response, warehouse);
			}

			return receives;
		}

		WhsReceive[] GetNextUnloadTaskReceives(WhsDocketsWebServiceResponse response, WhsWarehouse warehouse)
		{
			var receives = Array.Empty<WhsReceive>();
			var taskService = ObjectFactory.Get<IWhsTaskManagementService>();
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

			var result = taskService.GetNextTaskForWarehouseWeb(Factory, staff, warehouse.PK.ToGuid(), WarehouseTaskFormFlowTypes.UnloadJob, null);
			if (string.IsNullOrEmpty(result.ErrorMessage))
			{
				var task = Factory.Load<WhsReceiveProcessTasks>(result.TaskPK);
				receives = new[] { Factory.Load<WhsReceive>(task.P9_ParentID) };
			}
			else
			{
				response.LogBusinessValidationError(result.ErrorMessage);
			}
			return receives;
		}

		void GetWhsReceiveFromPackageReference(WhsDocketsWebServiceResponse response, string packageId)
		{
			var validPackageOrderInfosForReceiveCreation = PackageHelper.GetPackageParentOrdersWithNoFinalisedReturnReceive(Factory, packageId);

			if (validPackageOrderInfosForReceiveCreation.IsCountEqualTo(1))
			{
				var packageOrderInfoForReceiveCreation = validPackageOrderInfosForReceiveCreation.Single();
				var package = Factory.Load<PkgPackage>((ZGuid)packageOrderInfoForReceiveCreation[PkgPackageSchema.PK]);
				UpdateReturnReceive(response, package);

				if (response.Dockets == null)
				{
					response.Dockets = Array.Empty<WhsDocketInfo>();
				}
			}
			else
			{
				response.Dockets = GetPackageReceiveInfos(validPackageOrderInfosForReceiveCreation, packageId);
				response.HeldCodes = new CodeDescriptionPairInfo[] { new CodeDescriptionPairInfo { Code = "", Description = "" } };
			}
		}

		void UpdateReturnReceive(WhsDocketsWebServiceResponse response, PkgPackage returnedPackage)
		{
			if (returnedPackage?.PackageJob?.ParentJob is WhsOrder order)
			{
				var existingReceives = WhsReceive.GetExistingReturnReceivesForParentOrderWithOrderReference(order).ToArray();
				var returnReceive = GetReturnReceive(existingReceives, returnedPackage.KP_PackageID, order);
				if (returnReceive != null)
				{
					CreateReturnReceiveLines(returnReceive, existingReceives, returnedPackage);
					returnReceive.RunPreSaveValidation();
					if (returnReceive.HasErrors)
					{
						var errorMessage = new ZStringBuilder(Res.GetString("b5c12010-1b52-43ab-8476-40341cb8dc03", "Return Receive unload failed."));
						errorMessage.AppendLine(returnReceive.NotificationsIncludingChildren.ToUniqueMessageListString());
						response.LogError(ErrorTypes.BusinessValidationError, errorMessage.ToString());
					}
					else
					{
						GetWhsReceivesCore(response, [returnReceive], true);
					}
				}
			}
		}

		WhsReceive GetReturnReceive(WhsReceive[] existingReceives, string packageId, WhsOrder order)
		{
			WhsReceive returnReceive = null;
			var existingReceiveForPackage = GetExistingReceiveForPackage(existingReceives, packageId);
			var warehousePK = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).PK;

			if (existingReceiveForPackage != null)
			{
				if (IsReturnReceiveEligible(existingReceiveForPackage, warehousePK))
				{
					returnReceive = existingReceiveForPackage;
				}
			}
			else
			{
				var existingReceiveToUse = existingReceives.FirstOrDefault(receive => IsReturnReceiveEligible(receive, warehousePK));

				if (existingReceiveToUse != null)
				{
					returnReceive = existingReceiveToUse;
				}
				else
				{
					returnReceive = CreateReturnReceive(order, WhsReceive.GetNextMaxExternalReferenceSplitFromReceiveCollection(existingReceives));
				}
			}

			return returnReceive;

			bool IsReturnReceiveEligible(WhsReceive receive, ZGuid whsPK) => receive.WD_WW_Whs == whsPK && !receive.IsFinalised;
		}

		static WhsReceive GetExistingReceiveForPackage(IEnumerable<WhsReceive> existingReceives, string packageId)
		{
			WhsReceive existingReceiveForPackage = null;
			foreach (var receive in existingReceives)
			{
				if (receive.References.Cast<WhsDocketReference>()
					.Any(reference => reference.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.Other && reference.WX_Reference == PackageHelper.GetDocketPackageIdReference(packageId)))
				{
					existingReceiveForPackage = receive;
					break;
				}
			}

			return existingReceiveForPackage;
		}

		WhsReceive CreateReturnReceive(WhsOrder order, ZByte externalReferenceSplitNumber)
		{
			var returnReceive = Factory.New<WhsReceive>();
			returnReceive.WD_ExternalReference = order.WD_ExternalReference;
			returnReceive.WD_ExternalReferenceSplit = externalReferenceSplitNumber;
			returnReceive.WD_OH_Client = order.WD_OH_Client;
			returnReceive.WD_WW_Whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).PK;
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_WD_ParentDocket = order.PK;

			returnReceive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			returnReceive.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, "RF: Unloading job commenced.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			returnReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
			return returnReceive;
		}

		void CreateReturnReceiveLines(WhsReceive returnReceive, IEnumerable<WhsReceive> existingReceives, PkgPackage package)
		{
			foreach (var innerPackage in package.Packages)
			{
				CreateReturnReceiveLines(returnReceive, existingReceives, innerPackage);
			}

			if (GetExistingReceiveForPackage(existingReceives, package.KP_PackageID) == null)
			{
				var newReference = returnReceive.References.AddNew();
				newReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.Other;
				newReference.WX_Reference = PackageHelper.GetDocketPackageIdReference(package.KP_PackageID);

				var packageRelatedProductInfosWithAttributes = PackageHelper.GetPackageRelatedProductInfosWithAttributes(Factory, package.PK);
				if (packageRelatedProductInfosWithAttributes.Count > 0)
				{
					AddFetchHint(packageRelatedProductInfosWithAttributes, returnReceive.Factory);
				}

				foreach (DynamicBusinessObject packageRelatedProductInfoWithAttributes in packageRelatedProductInfosWithAttributes)
				{
					var line = returnReceive.Lines.AddNew();
					line.WE_OP = (ZGuid)packageRelatedProductInfoWithAttributes[WhsDocketLineSchema.WE_OP];
					line.WE_ClientOrderedUnits = (ZDecimal)packageRelatedProductInfoWithAttributes[PkgPackageItemDivotSchema.KI_PackedQty];
					line.WE_PartAttrib1 = (ZString)packageRelatedProductInfoWithAttributes[WhsDocketLineSchema.WE_PartAttrib1];
					line.WE_PartAttrib2 = (ZString)packageRelatedProductInfoWithAttributes[WhsDocketLineSchema.WE_PartAttrib2];
					line.WE_PartAttrib3 = (ZString)packageRelatedProductInfoWithAttributes[WhsDocketLineSchema.WE_PartAttrib3];
					line.WE_SerialNumber = (ZString)packageRelatedProductInfoWithAttributes[WhsDocketLineSchema.WE_SerialNumber];
					line.WE_PackingDate = ((ZDateTime)packageRelatedProductInfoWithAttributes[WhsDocketLineSchema.WE_PackingDate]).Date;
					line.WE_ExpiryDate = ((ZDateTime)packageRelatedProductInfoWithAttributes[WhsDocketLineSchema.WE_ExpiryDate]).Date;

					var workOrderInventoryPK = (ZGuid)packageRelatedProductInfoWithAttributes[WorkOrderInventoryPKKey];
					if (workOrderInventoryPK.IsValid)
					{
						var originalInventoryWithBOM = Factory.Load<WhsDocketLine>(workOrderInventoryPK);
						WhsRMAHelper.CopyBOMComponentLinks(originalInventoryWithBOM, line, false);
					}

					returnReceive.AsnLines.Add(line);
				}
			}
		}

		void AddFetchHint(DynamicBusinessObjectCollection dynamicBusinessObjectCollection, BusinessObjectFactory factory)
		{
			var inventoryPKs = dynamicBusinessObjectCollection.Select(bo => (ZGuid)bo[WorkOrderInventoryPKKey]).Where(pk => pk.IsValid).ToList();

			if (inventoryPKs.Count > 0)
			{
				var bOMInventoryPivotQuery = new ZQuery();
				bOMInventoryPivotQuery.AddToFilter(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, inventoryPKs);
				factory.AddFetchHint(WhsBOMInventoryPivotSchema.Instance, bOMInventoryPivotQuery);

				var inventoryQuery = new ZQuery();
				inventoryQuery.AddToFilter(WhsDocketLineSchema.PK, inventoryPKs);
				factory.AddFetchHint(WhsDocketLineSchema.Instance, inventoryQuery);
			}
		}

		const string WorkOrderInventoryPKKey = "WorkOrderInventoryPK";

		WhsDocketInfo[] GetPackageReceiveInfos(DynamicBusinessObjectCollection packageOrderInfos, string packageId)
		{
			return packageOrderInfos.Select(info => new WhsDocketInfo()
			{
				ExternalReference = packageId,
				DocketID = (ZString)info[WhsDocketSchema.WD_ExternalReference],
				ClientCode = (ZString)info[OrgHeaderSchema.OH_Code],
				PK = ((ZGuid)info[PkgPackageSchema.PK]).ToGuid()
			}).ToArray();
		}

		void GetWhsReceivesCore(WhsDocketsWebServiceResponse response, WhsReceive[] receives, bool isUnloadProcess)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var asnState = ASNState.NoPallets;
			var isEmptyAsnPalletMatchingEnabled = false;
			var receive = receives.FirstOrDefault();
			if (receive != null)
			{
				if (isUnloadProcess)
				{
					if (receive.WD_UnloadCompletedTime.IsValid)
					{
						response.LogBusinessValidationError(Res.GetString("48926852-6b15-4cb1-ad0d-fcdbe74a637d", "The Receive is already unloaded."));
					}
					else
					{
						CreateReceiptASNLinesInDb(receive);
						asnState = GetASNStateFromReceive(receive);
						isEmptyAsnPalletMatchingEnabled = receive.AsnLines.Any() && receive.AsnLines.Cast<WhsAsnLine>().All(asnLine => asnLine.WN_PalletId.IsEmpty);

						WhsReceiveHelper.SetSingleDockDoorLocationDetails(Factory, response, SecurityHeader.WarehouseCode);
					}
				}

				if (response.NoError())
				{
					UpdateArrivalDateOnReceive(receive);
					StartUnloadTask(response, staff, receive);

					if (response.NoError() && (!receive.IsInDatabase || receive.HasChanges))
					{
						var concurrencyErrorMessage = Res.GetString("d08b2de1-a9b3-4e9e-b4f1-f4543de3de85", "Another user has made changes while you have been loading this receive. Please restart the operation and try again.");
						WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
					}
				}
			}

			if (response.NoError())
			{
				PopulateReceiveInfos(response, receives, asnState, isEmptyAsnPalletMatchingEnabled, staff);
			}
		}

		void StartUnloadTask(WhsDocketsWebServiceResponse response, GlbStaff staff, WhsReceive receive)
		{
			if (receive.Warehouse.WW_GG_ReleaseGroup.IsValid)
			{
				var receiveTask = GetReceiveTask(receive, staff) ?? CreateUnloadTask(receive, staff);
				BeginRFTaskHelper.BeginRFTask(response, receiveTask, WarehouseTaskFormFlowTypes.UnloadJob, staff);
			}
		}

		ProcessTask GetReceiveTask(WhsReceive whsReceive, GlbStaff staff)
		{
			var staffQuery = new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff.GS_Code);
			staffQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Open);

			var taskQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, whsReceive.PK);
			taskQuery.AddToFilter(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.UnloadJob);
			taskQuery.AddToFilter(staffQuery);
			return Factory.LoadTop1<ProcessTask>(taskQuery);
		}

		void PopulateReceiveInfos(
			WhsDocketsWebServiceResponse response,
			WhsReceive[] receives,
			ASNState asnState,
			bool isEmptyAsnPalletMatchingEnabled,
			GlbStaff staff)
		{
			var receiveInfos = new WhsDocketInfo[receives.Length];

			for (var index = 0; index < receiveInfos.Length; index++)
			{
				var receive = receives[index];
				var docketInfo = new WhsDocketInfo(receive, shouldCreateDocketLines: false, staff: staff);

				docketInfo.ASNUnloadState = asnState;
				docketInfo.IsEmptyAsnPalletMatchingEnabledForUnload = isEmptyAsnPalletMatchingEnabled;
				SetMostRecentDockDoorLocation(receive, docketInfo);
				docketInfo.UsedSerialNumbers = GetUsedSerialNumbers(receive);
				docketInfo.ProductsWhichMayFulfillAsnLinesWithStockUnit = GetProductsWhichMayFulfillAsnLinesWithStockUnit(receive);
				receiveInfos[index] = docketInfo;
			}

			response.Dockets = receiveInfos;

			if (receives.Length == 1)
			{
				response.HeldCodes = GetHeldCodesOfClient(receives[0].Client.OH_Code.ToString());
			}
			else
			{
				response.HeldCodes = [];
			}

			response.ShowStockOnHandWarningOnPutaway = WarehouseDataRegistry.Instance.SOHLocationWarning.Value;
			response.CanDuplicatePreviousLine = Env.Security.WhsRFScanningUnloadDuplicatePreviousLine.IsAllowed;
		}

		Guid[] GetProductsWhichMayFulfillAsnLinesWithStockUnit(WhsReceive receive)
		{
			var result = new List<Guid>();
			var checkedProducts = new List<ZGuid>();
			var asnLines = receive.AsnLines.Cast<WhsAsnLine>();

			if (asnLines.Any())
			{
				foreach (var line in asnLines)
				{
					if (!checkedProducts.Contains(line.WN_OP))
					{
						var product = line.SupplierPart;

						var productMayFulfillAsnLinesWithStockUnit = asnLines.Where(l => l.WN_OP == product.PK).All(l => l.WN_QuantityUQ == product.OP_StockKeepingUnit);

						if (productMayFulfillAsnLinesWithStockUnit)
						{
							result.Add(product.PK.ToGuid());
						}

						checkedProducts.Add(line.WN_OP);
					}
				}
			}

			return result.ToArray();
		}

		#region GetASNStateFromReceive

		ASNState GetASNStateFromReceive(WhsReceive receive)
		{
			ASNState state;
			var asnLines = receive.AsnLines.Cast<WhsAsnLine>();
			if (!asnLines.Any(al => !string.IsNullOrEmpty(al.WN_PalletId)))
			{
				state = ASNState.NoPallets;
			}
			else if (AllAsnPalletsUnloaded(asnLines, receive.Lines.Cast<WhsReceiveLine>()))
			{
				state = ASNState.AllASNPalletsUnloaded;
			}
			else
			{
				state = ASNState.HasASNPalletsToUnload;
			}

			return state;
		}

		static bool AllAsnPalletsUnloaded(IEnumerable<WhsAsnLine> asnLines, IEnumerable<WhsReceiveLine> receiveLines)
		{
			return !asnLines
				.Where(al => !string.IsNullOrEmpty(al.WN_PalletId))
				.DistinctBy(al => al.WN_PalletId)
				.Select(al => al.WN_PalletId)
				.Except(receiveLines
						.Where(rl => !string.IsNullOrEmpty(rl.WE_PalletID) && rl.WE_ClientOrderedUnits > 0 && rl.WE_ClientOrderedUnits <= rl.WE_TransactionQuantity)
						.DistinctBy(rl => rl.WE_PalletID)
						.Select(rl => rl.WE_PalletID.ToUpper()))
				.Any();
		}

		#endregion

		#region SetMostRecentDockDoorLocation

		void SetMostRecentDockDoorLocation(WhsReceive receive, WhsDocketInfo docketInfo)
		{
			var lineAndAddedLog = from line in receive.Lines
								  where line.Inventory[0].IsReceivedIntoDockDoor
								  let mostRecentRfLog = GetMostRecentRFLog(line)
								  where mostRecentRfLog != null
								  orderby mostRecentRfLog.SL_EventTime descending
								  select line;

			var mostRecentlyAddedLineAndLog = lineAndAddedLog.FirstOrDefault();
			var location = mostRecentlyAddedLineAndLog?.Location;
			docketInfo.RecentlyUsedDockDoorLocation = location?.WLV_LocationString ?? ZString.Empty;
			docketInfo.RecentlyUsedDockDoorLocation_UserFriendly = location?.WLV_LocationString_UserFriendly ?? ZString.Empty;

			var transferFromPK = mostRecentlyAddedLineAndLog?.WE_WL ?? ZGuid.Empty;
			docketInfo.RecentlyUsedDockDoorLocationPK = mostRecentlyAddedLineAndLog != null && transferFromPK.IsValid
				? transferFromPK.ToGuid()
				: Guid.Empty;
		}

		StmALog GetMostRecentRFLog(WhsDocketLine line)
		{
			var recentRfLogs = new List<StmALog>();
			AddRfLogToList(recentRfLogs, line.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.AddedARecordToTheSystem, "RF"));
			AddRfLogToList(recentRfLogs, line.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.EditedARecord, "RF"));
			return recentRfLogs.OrderByDescending(log => log.SL_EventTime).FirstOrDefault();
		}

		void AddRfLogToList(List<StmALog> rfLogList, StmALog logToAdd)
		{
			if (logToAdd != null)
			{
				rfLogList.Add(logToAdd);
			}
		}

		#endregion

		#region GetUsedSerialNumbers

		string[] GetUsedSerialNumbers(WhsReceive receive)
		{
			var result = new HashSet<string>();

			foreach (var receiveLine in receive.Lines.Where(rl => rl.WE_TransactionQuantity > 0))
			{
				var product = receiveLine.Product;
				if (product.IsSerialNumberUsedAndNotReleaseCaptured(receive.Client))
				{
					result.Add(receiveLine.WE_SerialNumber + "|" + product.Parent.OP_PartNum);
				}
			}
			return result.ToArray();
		}

		#endregion

		#region LoadWhsDocket

		TDocket[] LoadWhsDockets<TDocket>(string reference, string docketDescription, string docketType)
			where TDocket : WhsDocket
		{
			var docketLoader = new DocketFromReferenceToArrayLoader<TDocket>(Factory, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode));
			var additionalFilter = new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, SQLComparisonOperator.Equal, null);
			return docketLoader.LoadWhsDockets(reference, docketDescription, docketType, additionalFilter);
		}

		#endregion

		#region CreateReceiptASNLinesInDb

		void CreateReceiptASNLinesInDb(WhsReceive receive)
		{
			if (!receive.StartedReceiving && IsFirstRFReceipt(receive))
			{
				receive.PopulateASNLines();
				receive.Lines.ForEach(rl => rl.WE_TransactionQuantity = 0);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				receive.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, "RF: Unloading job commenced.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		bool IsFirstRFReceipt(WhsReceive receive)
		{
			var result = true;

			if (receive.HasBeenLoadedInRF())
			{
				result = false;
			}
			else
			{
				foreach (WhsReceiveLine existingInventoryLine in receive.Lines)
				{
					if (WebServiceHelper.FindExistingLog(existingInventoryLine, ZArchitecture.Business.Events.AddedARecordToTheSystem, "RF") != null
						|| existingInventoryLine.HasPutawayTransfer)
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region UpdateArrivalDateOnReceive

		void UpdateArrivalDateOnReceive(WhsReceive receive)
		{
			if (receive.WD_ArrivalDate.IsEmpty)
			{
				receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			}
		}

		#endregion

		#region GetHeldCodesOfClient

		CodeDescriptionPairInfo[] GetHeldCodesOfClient(string clientCode)
		{
			var list = new List<CodeDescriptionPairInfo>();
			var heldCodesOfClient = GetHeldCodesCore().Where(hc => hc.ClientCode.IsNullOrEmpty() || hc.ClientCode.Equals(clientCode, StringComparison.OrdinalIgnoreCase));
			foreach (var heldCode in heldCodesOfClient)
			{
				list.Add(new CodeDescriptionPairInfo
				{
					Code = heldCode.Code,
					Description = heldCode.Description,
					CodeAndDescription = string.IsNullOrEmpty(heldCode.Code)
						? string.Empty
						: heldCode.Code + " - " + heldCode.Description
				});
			}

			return list.ToArray();
		}

		#endregion

		#endregion
	}
}
