using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class WebServiceHelper
	{
		#region GetOrgHeader

		public static OrgHeader GetOrgHeader(BusinessObjectFactory factory, string orgHeaderCode)
		{
			OrgHeader orgHeader = null;
			if (!string.IsNullOrEmpty(orgHeaderCode))
			{
				orgHeader = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgHeaderCode.Trim());
			}
			return orgHeader;
		}

		#endregion

		#region GetStaff

		public static GlbStaff GetStaff(BusinessObjectFactory factory, string userName)
		{
			var query = new ZQuery(GlbStaffSchema.GS_LoginName, userName);
			return factory.LoadTop1<GlbStaff>(query);
		}

		#endregion

		#region GetWarehouse

		public static WhsWarehouse GetWarehouse(BusinessObjectFactory factory, string code)
		{
			var query = new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, code);
			return factory.LoadTop1<WhsWarehouse>(query);
		}

		#endregion

		#region GetWarehouseSubQuery

		public static ZDBOnlySubQuery GetWarehouseSubQuery(WhsWarehouse warehouse)
		{
			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsInventoryViewSchema.WI_WD);
			docketSubQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehouse.PK);
			return docketSubQuery;
		}

		#endregion

		#region GetLocation

		public static WhsLocation GetLocationByLocationString(BusinessObjectFactory factory, string warehouseCode, WebServiceResponse response, string locationString)
		{
			WhsLocation location = null;
			var warehouse = GetWarehouse(factory, warehouseCode);
			if (response.ValidateShouldNotBeNull(warehouse, nameof(warehouse)))
			{
				location = GetLocationByLocationString(factory, warehouse, locationString);
			}
			return location;
		}

		public static WhsLocation GetLocationByLocationString(BusinessObjectFactory factory, WhsWarehouse warehouse, string locationString)
		{
			var locationQuery = new ZDBOnlyQuery(typeof(WhsLocation));
			AddLocationStringFilter(warehouse, locationQuery, locationString);

			return factory.LoadTop1<WhsLocation>(locationQuery);
		}

		public static void AddLocationStringFilter(WhsWarehouse warehouse, ZDBOnlyQuery locationQuery, string locationString)
		{
			var locationStringWithoutWarehouse = GetLocationStringWithoutWarehouse(warehouse, locationString);
			var locationStringQuery = new ZQuery(WhsLocationViewSchema.WLV_LocationString, locationStringWithoutWarehouse);
			var userFriendlyLocationQuery = new ZQuery(WhsLocationViewSchema.WLV_LocationString_UserFriendly, locationStringWithoutWarehouse);
			var locationStringAndUserFriendlyLocationQuery = new ZQuery();
			locationStringAndUserFriendlyLocationQuery.AddToFilter(locationStringQuery, JoinCondition.Or);
			locationStringAndUserFriendlyLocationQuery.AddToFilter(userFriendlyLocationQuery, JoinCondition.Or);

			locationQuery.AddToFilter(locationStringAndUserFriendlyLocationQuery, JoinCondition.And);
			locationQuery.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, warehouse.PK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307", Justification = "SQL query doesn't need StringComparison in this case")]
		static string GetLocationStringWithoutWarehouse(WhsWarehouse warehouse, string locationString)
		{
			var oldLocationBarcodeStart = (NoResString)"#W" + warehouse.WW_WarehouseCode;    // No need translation.
			return (locationString.StartsWith(oldLocationBarcodeStart))
				? locationString.Substring(oldLocationBarcodeStart.Length)
				: locationString;
		}

		#endregion

		#region GetDockDoorLocations

		public static IEnumerable<WhsLocation> GetDockDoorLocations(BusinessObjectFactory factory, string warehouse)
		{
			IEnumerable<WhsLocation> result = null;
			var whs = GetWarehouse(factory, warehouse);
			if (whs.IsUsingDockDoorLocation)
			{
				var locationQuery = new ZQuery(WhsLocationViewSchema.WLV_WW_Whs, whs.PK);
				locationQuery.AddToFilter(WhsLocationViewSchema.WLV_LocationClass, LocationClasses.Codes.DDL);
				result = factory.Load<WhsLocation>(locationQuery);
			}
			return result ?? Enumerable.Empty<WhsLocation>();
		}

		#endregion

		#region GetConsumedCapacityForLocation

		public static decimal GetConsumedCapacityForLocation(BusinessObjectFactory factory, ZGuid locationPK)
		{
			var rawSql = string.Format(CultureInfo.InvariantCulture, @"
;
WITH CurrentStock as 
(
	SELECT 
		SUM(inventoryLine.WE_StockOnHand) as TotalUnits
	FROM
		dbo.WhsDocketLine inventoryLine
	WHERE
		inventoryLine.WE_StockOnHand > 0
		AND inventoryLine.WE_DocketLineStatus = 'FIN'
		AND WE_WL = @WL_PK
),
PendingStock as 
(
	SELECT 
		SUM(transactions.WE_TransactionQuantity) as PendingUnits
	FROM
		dbo.WhsDocketLine transactions
	WHERE
		WE_TransactionQuantity > 0
		AND WE_DocketLineStatus NOT IN ('FIN', 'PFU', 'CAN')
		AND WE_DocketLineType <> 'ORD'
		AND WE_WL = @WL_PK
)
SELECT 
	ISNULL(TotalUnits, 0) + ISNULL(PendingUnits, 0) as TotalUnits
FROM 
	CurrentStock, PendingStock
");

			var result = 0m;
			var locationTotalUnit = new DynamicBusinessObjectCollection(factory);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@WL_PK", locationPK, WhsDocketLineSchema.WE_WL);

			locationTotalUnit.Load(rawSql, parameters);
			if (locationTotalUnit.Any())
			{
				result = Convert.ToDecimal(locationTotalUnit[0]["TotalUnits"].ToString(), CultureInfo.InvariantCulture);
			}

			return result;
		}

		#endregion

		#region GetPartByPartNumOrBarcode

		public static OrgSupplierPart GetPartByPartNumOrBarcode(BusinessObjectFactory factory, string warehouseCode, string partNumOrBarcode, string clientCode, bool clientCodeIsMandatory)
		{
			return GetPartByPartNumOrBarcode(factory, warehouseCode, partNumOrBarcode, clientCode, clientCodeIsMandatory, out _, out _, out _);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		public static OrgSupplierPart GetPartByPartNumOrBarcode(BusinessObjectFactory factory, string warehouseCode, string partNumOrBarcode, string clientCode, bool clientCodeIsMandatory, out OrgHeader client, out string barcode, out string errorMessage)
		{
			client = null;
			barcode = "";
			errorMessage = null;

			if (string.IsNullOrEmpty(partNumOrBarcode.Trim()))
			{
				errorMessage = Res.GetString("d21efa54-e6c6-45a1-954b-57f94ddc6d33", "Please provide a valid product code or barcode.");
			}
			else
			{
				client = GetOrgHeader(factory, clientCode);
				if (client == null && (clientCodeIsMandatory || !string.IsNullOrEmpty(clientCode)))
				{
					errorMessage = Res.GetString("c939b317-081f-4ba8-9afd-18bf06247f35", "Please provide a valid client code.");
				}
			}

			OrgSupplierPart result = null;

			if (string.IsNullOrEmpty(errorMessage))
			{
				var warehouse = GetWarehouse(factory, warehouseCode);
				result = GetPartByPartNumOrBarcode(factory, partNumOrBarcode, client, warehouse, out barcode, out errorMessage);
			}

			return result;
		}

		static OrgSupplierPart GetPartByPartNumOrBarcode(BusinessObjectFactory factory, string partNumOrBarcode, OrgHeader client, WhsWarehouse warehouse, out string barcode, out string errorMessage)
		{
			barcode = "";
			errorMessage = null;

			var part = GetOrderedProductsFromPartNumOrBarcode(partNumOrBarcode, client, factory).FirstOrDefault();
			if (part == null)
			{
				var message = client == null ? "" : Res.GetString("bc56de8e-b6be-49e1-986a-7f4de69ef9d1", "(Client: {0})", client.OH_Code);
				errorMessage = Res.GetString("e16b9662-e4ce-4adf-8700-7cdd41d4aacf", "Product could not be found.{0} Please provide a valid product code or barcode.", message);
			}
			else if (WhsProduct.GetWhsProduct(part).IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(client, warehouse))
			{
				errorMessage = Enterprise.Warehouse.Transactions.Business.PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage;
				part = null;
			}
			else if (!part.OP_PartNum.EqualsIgnoringCase(partNumOrBarcode))
			{
				barcode = partNumOrBarcode;
			}

			return part;
		}

		public static OrgSupplierPart[] GetPartsByBarcode(BusinessObjectFactory factory, string barcode)
		{
			var query = GetPartBarcodeFilterQuery(barcode);
			return factory.Load<OrgSupplierPart>(query);
		}

		public static OrgSupplierPart[] GetPartsByPartNum(BusinessObjectFactory factory, string productCodeOrBarcode)
		{
			var query = new ZQuery(OrgSupplierPartSchema.OP_PartNum, productCodeOrBarcode);
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);

			return factory.Load<OrgSupplierPart>(query);
		}

		internal static ZDBOnlyQuery GetPartBarcodeFilterQuery(string barcode)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);

			var subQueryBC = new ZDBOnlySubQuery(typeof(OrgSupplierPartBarcode), OrgSupplierPartBarcodeSchema.PH_OP);
			subQueryBC.AddToFilter(OrgSupplierPartBarcodeSchema.PH_Barcode, SQLComparisonOperator.Equal, barcode);
			query.AddSubQuery(subQueryBC, JoinCondition.And);

			return query;
		}

		#endregion

		#region GetProductQuery

		static ZDBOnlyQuery GetProductQuery(string partNumOrBarcode, OrgHeader client)
		{
			var fullQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			fullQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, ZBool.True);
			fullQuery.AddSubQuery(GetProductQueryCore(OrgSupplierPartSchema.PK, partNumOrBarcode), JoinCondition.And);

			if (client != null)
			{
				AddPartRelationSubQuery(fullQuery, client.PK);
			}

			return fullQuery;
		}

		internal static ZDBOnlySubQuery GetProductQueryCore(SchemaColumn parentColumn, string partNumOrBarcode)
		{
			var barcodePartQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPartBarcode), OrgSupplierPartBarcodeSchema.PH_OP);
			barcodePartQuery.AddToFilter(OrgSupplierPartBarcodeSchema.PH_Barcode, partNumOrBarcode);

			var partNumberQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), parentColumn);
			partNumberQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNumOrBarcode);
			partNumberQuery.AddAsUnionQuery(barcodePartQuery, addAsUnionAll: true);

			return partNumberQuery;
		}

		internal static IEnumerable<OrgSupplierPart> GetOrderedProductsFromPartNumOrBarcode(string partNumOrBarcode, OrgHeader client, BusinessObjectFactory factory)
		{
			return factory.Load<OrgSupplierPart>(GetProductQuery(partNumOrBarcode, client)).OrderByDescending(p => p.OP_PartNum == partNumOrBarcode);
		}

		#endregion

		#region LoadWhsDocket

		public static TDocket LoadWhsDocket<TDocket>(BusinessObjectFactory factory, string reference, string docketDescription, string docketType, string warehouseCode, ZQuery additionalFilter, Guid? clientPK = null)
			where TDocket : WhsDocket
		{
			var docketLoader = new DocketFromReferenceToDocketLoader<TDocket>(factory, WebServiceHelper.GetWarehouse(factory, warehouseCode), clientPK);
			return docketLoader.LoadWhsDockets(reference, docketDescription, docketType, additionalFilter);
		}

		#endregion

		#region LoadWhsInventoryByPalletID

		public static IEnumerable<WhsInventoryView> LoadWhsInventoryByPalletID(WebServiceResponse response, BusinessObjectFactory factory, string warehouseCode, string palletID, bool logErrorIfEmpty = false)
			=> LoadWhsInventoryByPalletIDs(response, factory, warehouseCode, new[] { palletID }, logErrorIfEmpty);

		public static IEnumerable<WhsInventoryView> LoadWhsInventoryByPalletIDs(WebServiceResponse response, BusinessObjectFactory factory, string warehouseCode, IEnumerable<string> palletIDs, bool logErrorIfEmpty = false)
		{
			var inventory = Array.Empty<WhsInventoryView>();
			if (!palletIDs.Any() || string.IsNullOrEmpty(palletIDs.First()))
			{
				response.LogBusinessValidationError(ProvidePalletIdErrorMessage);
			}
			else
			{
				var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
				query.AddFilterAndZSQLParameterCollection($"{WhsInventoryViewSchema.Constants.WI_PalletID} <> ''", new ZSqlParameterCollection()); // we needed this to force sql server to use NR_RX__WE_PalletID index
				query.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, GetWarehouse(factory, warehouseCode).PK);
				query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, palletIDs);
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
				query.OrderBy = "(SELECT OP_PartNum FROM dbo.OrgSupplierPart WHERE OP_PK = WI_OP)";
				query.ReLoadExistingRows = true;

				inventory = factory.Load<WhsInventoryView>(query);
				if (logErrorIfEmpty && inventory.Length == 0)
				{
					response.LogBusinessValidationError(PalletIdCanntBeFoundErrorMessage);
				}
			}
			return inventory;
		}

		#endregion

		#region LoadWhsInventoryByTransferFromPalletID

		public static IEnumerable<WhsInventoryView> LoadWhsInventoryByTransferFromPalletID(WebServiceResponse response, BusinessObjectFactory factory, string warehouseCode, string palletID, bool logErrorIfEmpty = false, bool includeFinalized = false)
			=> LoadWhsInventoryByTransferFromPalletIDs(response, factory, warehouseCode, new[] { palletID }, logErrorIfEmpty, includeFinalized);

		// Tested in AllocateMultiplePalletPutawayLocationsTest.cs TestAllocateMultiplePalletPutawayLocations_ConsolidatedPallets
		public static IEnumerable<WhsInventoryView> LoadWhsInventoryByTransferFromPalletIDs(WebServiceResponse response, BusinessObjectFactory factory, string warehouseCode, IEnumerable<string> palletIDs, bool logErrorIfEmpty, bool includeFinalized = false)
		{
			var docketLines = Array.Empty<WhsDocketLine>();
			if (!palletIDs.Any() || palletIDs.Any(id => id.IsNullOrEmpty()))
			{
				response.LogBusinessValidationError(ProvidePalletIdErrorMessage);
			}
			else
			{
				var subQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
				subQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, GetWarehouse(factory, warehouseCode).PK);

				var query = new ZDBOnlyQuery(typeof(WhsDocketLine));
				query.AddToFilter(WhsDocketLineSchema.WE_TransferFromPalletId, palletIDs);
				query.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
				if (!includeFinalized)
				{
					query.AddToFilter(WhsDocketLineSchema.WE_FinalisedDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
				}
				query.AddSubQuery(subQuery, JoinCondition.And);
				query.OrderBy = "(SELECT OP_PartNum FROM dbo.OrgSupplierPart WHERE OP_PK = WE_OP)";
				query.ReLoadExistingRows = true;

				docketLines = factory.Load<WhsDocketLine>(query);
				if (logErrorIfEmpty && docketLines.Length == 0)
				{
					response.LogBusinessValidationError(PalletIdCanntBeFoundErrorMessage);
				}
			}
			return docketLines.SelectMany(line => line.Inventory.Cast<WhsInventoryView>());
		}

		static string ProvidePalletIdErrorMessage => Res.GetString("19a1df4c-6155-44ea-9a1b-24b8294c2045", "Provide Pallet ID.");

		static string PalletIdCanntBeFoundErrorMessage => Res.GetString("07e7103d-1fe4-4a52-8f09-a851816fc8b6", "Pallet ID(s) cannot be found.");

		#endregion

		#region LoadAndValidateClientAndPartBeforeRunningAction

		public static void LoadAndValidateClientAndPartBeforeRunningAction<T>(T response, BusinessObjectFactory factory, Guid productPK, string clientCode,
			Action<T, OrgSupplierPart, OrgHeader> action) where T : WebServiceResponse
		{
			var client = GetOrgHeader(factory, clientCode);
			if (client == null)
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("c939b317-081f-4ba8-9afd-18bf06247f35", "Please provide a valid client code."));
			}
			else
			{
				var part = factory.Load<OrgSupplierPart>(productPK);
				if (part == null)
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("528fbd64-4a17-48d4-9689-cedf1e463b50", "Please provide a valid product code."));
				}
				else
				{
					action(response, part, client);
				}
			}
		}

		#endregion

		#region TrolleyPicking / PickByLabel Common Methods

		public static bool CheckTrolleyJobValidAndIsBuilding(WebServiceResponse response, WhsPickTrolleyJob trolleyJob)
		{
			if (trolleyJob == null)
			{
				response.ErrorMessage = Res.GetString("8c9f234e-bb80-4be8-bf29-a93571642528", "Trolley job was not found. Please start building Trolley again.");
			}
			else if (trolleyJob.WTJ_Status != PickTrolleyStatus.Codes.Building)
			{
				response.ErrorMessage = Res.GetString("8d734566-475b-4f5b-9e4e-69cf1a9b97a2", "This Trolley job is in {0} state. Cannot remove Package from Trolley.", trolleyJob.WTJ_Status);
			}
			return string.IsNullOrEmpty(response.ErrorMessage);
		}

		public static void CheckPickability<T>(WhsPickJobWebServiceResponse<T> response, GlbStaff rfUser, IEnumerable<WhsPickLine> notPickedPickLines, bool shouldOverrideOtherUsers)
			where T : WhsPickJobInfo
		{
			Argument.NotNull(rfUser, nameof(rfUser));
			Argument.NotNull(response, nameof(response));

			var otherUsers = notPickedPickLines.Where(l => !l.WZ_GS_NKAssignedTo.IsEmpty && l.WZ_GS_NKAssignedTo != rfUser.GS_Code);
			if (otherUsers.Any())
			{
				var otherUserPicking = otherUsers.Where(pl => pl.WZ_IsPicking).Select(l => l.WZ_GS_NKAssignedTo).Distinct().OrderBy(s => s);
				if (otherUserPicking.Any())
				{
					response.Job = null;
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("b8d81073-9b31-4589-bbb4-45411b044675", "Some of the items are in process of being picked by '{0}'. All items should be picked by the same person.",
							string.Join("', '", otherUserPicking));
				}
				else if (!shouldOverrideOtherUsers)
				{
					response.Job = null;
					response.Error = ErrorTypes.YesNoEnquiry;
					var otherUserCodes = otherUsers.Select(l => l.WZ_GS_NKAssignedTo).Distinct().OrderBy(s => s);
					response.ErrorMessage = Res.GetString("928bc828-d3e1-4d11-a843-c916cb79cf3d", "Some of the items are assigned to be picked by '{0}'. Would you like to assign all items to yourself?",
						string.Join("', '", otherUserCodes));
				}
			}
		}

		public static void AssignPickLinesToUserAndSave(WebServiceResponse response, BusinessObjectFactory factory, IEnumerable<WhsPickLine> notPickedPickLines, GlbStaff rfUser)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(rfUser, nameof(rfUser));

			foreach (var pickLine in notPickedPickLines)
			{
				pickLine.WZ_GS_NKAssignedTo = rfUser.GS_Code;
				pickLine.WZ_IsPicking = true;
				// WZ_PickedDateTime will be set when the user actually confirms they picked stuff
			}

			var concurrencyErrorMessage = Res.GetString("e1e0be0b-d451-4499-af9d-045664414e23", "Another user has been assigned to or modified this Job. Please restart the operation and try again.");
			SaveFactoryWithExceptionHandling(factory, response, (concurrencyException) => concurrencyErrorMessage);
		}

		internal static PkgPackage GetPackageOnTrolley(WebServiceResponse response, WhsPickTrolleyJob trolleyJob, string packageID)
		{
			var package = trolleyJob.Slots.FirstOrDefault(sl => sl.Package.KP_PackageID == packageID)?.Package;
			if (package == null)
			{
				response.ErrorMessage = Res.GetString("337d51c2-c743-4862-85cb-d256339f88b6", "Package '{0}' was not on this Trolley.", packageID);
			}
			return package;
		}

		internal static WhsPickTrolleyJob GetTrolleyJobUsingTrolleyJobPK(BusinessObjectFactory factory, WebServiceResponse response, Guid trolleyJobPK)
		{
			var trolleyJob = factory.Load<WhsPickTrolleyJob>(trolleyJobPK);
			if (trolleyJob == null)
			{
				response.ErrorMessage = Res.GetString("4c9b1afc-bba9-403f-84ae-4af05e69c009", "Trolley job was not found. Please start building Trolley again.");
			}
			return trolleyJob;
		}

		internal static WhsCartonGroup GetCartonGroupUsingCartonGroupCode(BusinessObjectFactory factory, WebServiceResponse response, string cartonGroupCode)
		{
			var cartonGroup = factory.LoadTop1<WhsCartonGroup>(new ZQuery(WhsCartonGroupSchema.WCG_Code, cartonGroupCode));
			if (cartonGroup == null)
			{
				response.ErrorMessage = Res.GetString("002cc472-d148-4d8a-b86b-45a2d1453f6c", "Carton Group was not found.");
			}
			return cartonGroup;
		}

		#endregion

		#region Pick and Pack

		public static WhsOrder GetOrderByDocketID(BusinessObjectFactory factory, WebServiceResponse response, string warehouseCode, string orderDocketID)
		{
			var query = DocketFromReferenceLoader<WhsOrder>.GetUnfinalisedDocketIDQuery(GetWarehouse(factory, warehouseCode), orderDocketID, DocketType.Codes.Order);
			var order = factory.LoadTop1<WhsOrder>(query);
			if (order == null)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("4b35943c-de1c-400e-9a9f-59d7b63c5360", "No Warehouse Order found with Docket ID: {0}.", orderDocketID);
			}

			return order;
		}

		public static PkgPackage GetPackageByPackagePK(WhsOrder order, Guid packagePK)
		{
			return (PkgPackage)order.PackageJob.Packages.FindByPK(packagePK);
		}

		public static PkgPackage GetPackageByPackageID(WhsOrder order, string packageID)
		{
			return order.PackageJob.Packages.SingleOrDefault(p => p.KP_PackageID.EqualsIgnoringCase(packageID));
		}

		public static WhsClientPickPackParamsByWhs GetPickPackParameter(WhsOrder order)
		{
			var param = order.ClientPickingParams;
			return param != null && param.WPP_IsPickAndPackEnabled ? param : null;
		}

		public static bool IsPickAndPackEnabled(WhsPick pick) =>
			pick != null &&
			pick.Orders.Count == 1 &&
			!pick.IsWorkOrderPick &&
			GetPickPackParameter((WhsOrder)pick.Orders[0]) != null;

		#endregion

		#region GetJobServiceSupporter

		public static IHaveServices GetJobServiceSupporter(BusinessObjectFactory factory, Guid jobPK, JobServiceSupporterStrategy jobServiceSupporterStrategy)
		{
			switch (jobServiceSupporterStrategy)
			{
				case JobServiceSupporterStrategy.WhsReceive:
					return factory.Load<WhsDocket>(jobPK);
				case JobServiceSupporterStrategy.WhsPickLine:
					IHaveServices serviceSupporter = null;
					var pickLine = factory.Load<WhsPickLine>(jobPK);
					if (pickLine != null)
					{
						var orderLine = factory.Load<WhsDocketLine>(pickLine.WZ_WE_TransactionLine);
						serviceSupporter = orderLine?.Docket;
					}
					return serviceSupporter;
				default:
					return null;
			}
		}

		#endregion

		#region SaveFactoryWithExceptionHandling

		public static void SaveFactoryWithExceptionHandling(BusinessObjectFactory factory, WebServiceResponse response, Func<ZSaveConcurrencyException, string> getErrorMessageForConcurrencyException)
		{
			Argument.NotNull(getErrorMessageForConcurrencyException, nameof(getErrorMessageForConcurrencyException));

			SaveFactoryWithExceptionHandling(factory, ex =>
			{
				var errorMessage = ex switch
				{
					ZSaveConcurrencyException concurrencyException => getErrorMessageForConcurrencyException(concurrencyException),
					ZSaveException e when e.IsInnermostLockTimeoutExpired() => Res.GetString("c45fa4f7-dec6-44fd-9094-f74a4ff2b724", "The save operation timed out\r\n{0}", GetFriendlyOrDefualtMessage(e)),
					_ => ex.Message
				};

				response.LogBusinessValidationError(errorMessage);

				static string GetFriendlyOrDefualtMessage(ZSaveException e)
					=> e.FriendlyMessage.IsNullOrEmpty() ? Res.GetString("10ac37ba-5a4f-491d-8daa-77c2b63553c4", "Please try again.") : e.FriendlyMessage;
			});
		}

		public static void SaveFactoryWithExceptionHandling(BusinessObjectFactory factory, Action<Exception> onExceptionThrown)
		{
			try
			{
				factory.Save();
			}
			catch (ZCannotSaveException ex)
			{
				onExceptionThrown(ex);
			}
			catch (ZSaveConcurrencyException ex)
			{
				onExceptionThrown(ex);
			}
			catch (ZSaveException ex) when (ex.IsInnermostLockTimeoutExpired())
			{
				onExceptionThrown(ex);
			}
		}

		#endregion

		#region FinaliseAndSaveDocket

		internal static void FinaliseAndSaveDocket<TDocket>(BusinessObjectFactory factory, TDocket docket, string docketDescription)
			where TDocket : WhsDocket
		{
			docket.FinaliseDocketWithoutUserConfirmation();

			if (!docket.IsFinalised)
			{
				SendNotificationEmail(factory, docket, docketDescription);
			}
			else
			{
				SaveFactoryWithExceptionHandling(factory, ex => SendNotificationEmail(factory, docket, docketDescription, ex.Message));
			}
		}

		#region SendNotificationEmail

		static void SendNotificationEmail(BusinessObjectFactory factory, WhsDocket docket, string docketDescription, string exceptionMessage = "")
		{
			ZGuid groupPK = WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value;
			if (!groupPK.IsEmpty)
			{
				var group = factory.Load<GlbGroup>(groupPK);
				if (group != null && HasAtLeastOneRecipient(group))
				{
					var possibleActiveTasksOnPlayNotification = docket.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned)
						? Res.GetString("67d78f29-9574-463e-a6a7-29956fc72e72", "If there are no errors, then there might be active tasks set to working assigned to other users for the {0}.", docketDescription)
						: string.Empty;
					var possibleActiveTasksOnPlayNotificationLine = possibleActiveTasksOnPlayNotification.IsNullOrEmpty() ? string.Empty : "\r\n" + possibleActiveTasksOnPlayNotification;
					var email = new EmailDef();
					email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupPK.ToGuid(), Enterprise.Registry.Business.Warehouse.WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup);
					email.Subject = Res.GetString("d2a9b376-b765-4df9-a1de-d40145e83258", "{0} Job {1} RF finalization Failure", docketDescription, docket.WD_DocketID);
					email.Body = Res.GetString("9e456fd8-c615-4254-b9e3-87ce9a41f38d", "Job ID: {0}", docket.WD_DocketID) + " \r\n" +
						Res.GetString("d2147ab0-cb1f-426b-8013-02124d36bbf4", "Reference: {0}", docket.WD_ExternalReference) + "\r\n\r\n" +
						Res.GetString("a042d15f-fbc8-4fc0-b7e2-291876e3f1d4", "{0} Job {1} (entered via RF) could not be finalized due to following errors:", docketDescription, docket.WD_DocketID) + "\r\n\r\n" +
						docket.NotificationsIncludingChildren.ToUniqueMessageListString() + exceptionMessage +
						possibleActiveTasksOnPlayNotificationLine;
					Env.OutgoingMailManager.CreateAndSave(email);
				}
			}
		}

		static bool HasAtLeastOneRecipient(GlbGroup group)
		{
			foreach (GlbStaff staff in group.Staff)
			{
				if (!staff.GS_EmailAddress.IsEmpty)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#endregion

		#region GetPartsByPartNum

		public static OrgSupplierPart GetPartByPartNum(BusinessObjectFactory factory, string partNum, ZGuid clientPK)
		{
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			AddPartNumFilter(query, partNum);
			AddPartRelationSubQuery(query, clientPK);

			return factory.LoadTop1<OrgSupplierPart>(query);
		}

		#endregion

		#region GenerateWarehouseConfirmedPutAwayEvent

		public static void GenerateWarehouseConfirmedPutAwayEvent(WhsDocketLine docketLine)
		{
			var receiveLine = docketLine as WhsReceiveLine;
			var locationPK = (receiveLine?.HasPutawayTransfer ?? false) ? receiveLine.PutawayTransferLine.WE_WL : docketLine.WE_WL;
			if (locationPK.IsValid)
			{
				var logType = ZArchitecture.Business.Events.WarehouseReceiptConfirmedPutaway;
				var oldLog = FindExistingLog(docketLine, logType, "RF");
				if (oldLog != null)
				{
					oldLog.Cancel();
				}

				var references = new ZStringBuilder();
				if (docketLine.IsInDatabase) // i.e. Line existed before this was run, has non non zero (correct) line number
				{
					references.Append(string.Format(CultureInfo.CurrentCulture, (NoResString)"Line {0}", docketLine.WE_LineNo));  // This is log information.
				}

				references.AppendIfNotEmpty((NoResString)"Pallet ID ", docketLine.WE_PalletID);   // This is log information.
				references.AppendIfNotEmpty((NoResString)"Location ", docketLine.LocationString); // This is log information.

				var newLog = docketLine.Logs.AddNew(logType,
					string.Format(CultureInfo.CurrentCulture, "RF: Putaway for {0}.",
					references.ToStringWithDelimiterBetweenAppends(", ")));
				newLog.SL_GS_NKUser = Enterprise.Environment.Env.CurrentUser.Initials;
			}
		}

		#endregion

		#region FindExistingLog

		internal static StmALog FindExistingLog(EnterpriseBusinessObject businessObject, Event logType, ZString logReference)
		{
			return businessObject.Logs.GetAllLogs().Cast<StmALog>()
				.FirstOrDefault(l => l.SL_SE_NKEvent == logType.Code && l.SL_Reference.StartsWith(logReference, StringComparison.CurrentCultureIgnoreCase));
		}

		#endregion

		#region GetUnfinalisedDocketByPKQuery

		public static ZQuery GetUnfinalisedDocketByPKQuery(BusinessObjectFactory factory, string warehouseCode, Guid pk, ZQuery additionalFilter = null)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsDocketSchema.PK, pk);
			query.AddToFilter(WhsDocketSchema.WD_WW_Whs, GetWarehouse(factory, warehouseCode).PK);
			query.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);

			if (additionalFilter != null)
			{
				query.AddToFilter(additionalFilter);
			}

			return query;
		}

		#endregion

		#region InventoryStatusAllowsTransfer

		public static bool InventoryAllowsTransfer(WhsInventoryView inventory, WhsLocation location)
		{
			return !location.IsDockDoorLocation && inventory.WI_InventoryStatus != InventoryStatus.Codes.Putaway;
		}

		#endregion

		#region WebServiceResponseExtentions

		public static void LogBusinessValidationError(this WebServiceResponse response, string errorMessage) => response.LogError(ErrorTypes.BusinessValidationError, errorMessage);

		public static bool ValidateShouldNotBeNull<T>(this WebServiceResponse response, T argument, string name)
		{
			var result = (argument == null);
			if (result)
			{
				response.LogBusinessValidationError(Res.GetString("8C2C1440-5314-4506-BD6B-7E71DBAF77F0", "{0} cannot be null.", name));
			}
			return !result;
		}

		public static bool NoError(this WebServiceResponse response) => response.Error == ErrorTypes.None || response.Error == ErrorTypes.Information;

		#endregion

		#region Implementation

		static void AddPartNumFilter(ZQuery query, string partNum)
		{
			query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNum);
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, ZBool.True);
		}

		static void AddPartRelationSubQuery(ZDBOnlyQuery query, ZGuid clientPK)
		{
			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.Equal, clientPK);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, new[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both });
			query.AddSubQuery(relationSubQuery, JoinCondition.And);
		}

		#endregion
	}
}
