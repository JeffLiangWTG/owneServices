using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region AutoCreatePackagesForPick

		[WebMethod(Description = "Auto Create Attribute Neutral Packages For Pick")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse AutoCreateAttributeNeutralPackagesForPick(AttributeNeutralPackageInfo[] packageToCreateInfos)
			=> HandleWebServiceRequest_WithValidateWarehouseAndStaff<WebServiceResponse>(r => AutoCreateAttributeNeutralPackagesForPickCore(r, packageToCreateInfos));

		void AutoCreateAttributeNeutralPackagesForPickCore(WebServiceResponse response, AttributeNeutralPackageInfo[] packageToCreateInfos)
		{
			if (packageToCreateInfos != null && packageToCreateInfos.Length > 0)
			{
				var packingAttempted = false;
				var packageToCreateInfosList = new List<AttributeNeutralPackageInfo>(packageToCreateInfos.Length);
				foreach (var info in packageToCreateInfos.OrderBy(i => i.ExpectedQuantityInPackage))
				{
					info.SerialNumbers = info.SerialNumbers.Where(s => !string.IsNullOrEmpty(s)).Select(s => s.ToUpper()).ToArray();
					packageToCreateInfosList.Add(info);
				}

				var orderProductSerialNumInfo = LoadOrderInfo(packageToCreateInfosList.Select(p => (p.ProductPK, p.SerialNumbers)).ToArray());
				if (orderProductSerialNumInfo.Count > 0)
				{
					var pick = orderProductSerialNumInfo.First().Value.Order.Pick;
					var totalPalletQuantities = LoadInventoryQuantitiesPerPallet(pick.Factory, packageToCreateInfos.Select(i => i.PalletID).Distinct(), pick.WP_WW_Whs);
					if (packageToCreateInfosList.Any(i => !IsSingleOrderPackage(orderProductSerialNumInfo, i)))
					{
						var reallocatedOrderInfos = ReallocateSerialNumberInventories(packageToCreateInfosList, orderProductSerialNumInfo);
						if (reallocatedOrderInfos.Count > 0)
						{
							orderProductSerialNumInfo = reallocatedOrderInfos;
						}
					}

					AddFetchHints(orderProductSerialNumInfo);
					foreach (var packageToCreateInfo in packageToCreateInfosList)
					{
						if (orderProductSerialNumInfo.TryGetValue((packageToCreateInfo.ProductPK, packageToCreateInfo.SerialNumbers[0]), out var row))
						{
							var client = row.Order.Client;
							var isAutoPkgCreationEnabled = row.Order.ClientPickingParams?.WPP_EnableAutoPackageCreationOnPicking ?? false;
							if (isAutoPkgCreationEnabled)
							{
								packingAttempted = true;
								PackSerialNumbers(
									response,
									packageToCreateInfo,
									row.Order,
									orderProductSerialNumInfo,
									totalPalletQuantities);

								if (response.Error != ErrorTypes.None)
								{
									break;
								}
							}
						}
					}
				}

				if (packingAttempted && response.Error == ErrorTypes.None)
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(
						Factory,
						ex =>
						{
							var errorMessage =
								Res.GetString(
									"a7dada4a-436e-49f7-869d-ba8f7dbeb26d",
									"Auto-packing failed. A saving error occurred whilst attempting to auto-pack:\r\n{0}",
									string.Join("\r\n", ex.Message, ex.InnerException?.Message ?? string.Empty));

							response.LogBusinessValidationError(errorMessage);
							ErrorReporter.ReportOnce(errorMessage);
						});
				}
			}
		}

		void AddFetchHints(Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)> orderProductSerialNumInfo)
		{
			var pickLinePKs = orderProductSerialNumInfo.Values.Select(v => v.PickLine.PK);
			Factory.AddFetchHint(PkgPackageItemDivotSchema.Instance, new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, pickLinePKs));

			var clientPKs = orderProductSerialNumInfo.Values.Select(v => v.Order.WD_OH_Client);
			Factory.AddFetchHint(WhsClientPickPackParamsByWhsSchema.Instance, new ZQuery(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, clientPKs));

			var orderPKs = orderProductSerialNumInfo.Values.Select(v => v.Order.PK);
			Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, orderPKs));
			Factory.AddFetchHint(StmALogSchema.Instance, new ZQuery(StmALogSchema.SL_Parent, orderPKs));

			var pickPKs = orderProductSerialNumInfo.Values.Select(v => v.Order.WD_WP).Distinct();
			Factory.AddFetchHint(StmALogSchema.Instance, new ZQuery(StmALogSchema.SL_Parent, pickPKs));

			WhsPickByBOMHelper.AddFetchHintsForIsPickByBOMKitPickLine(Factory, orderProductSerialNumInfo.Values.Select(v => v.PickLine));
		}

		Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)> ReallocateSerialNumberInventories(
			List<AttributeNeutralPackageInfo> packageToCreateInfos,
			Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)> orderProductSerialNumInfo)
		{
			var (orderInfoKeysViaSerial, orderInfoDictionary) = PopulateInformation();
			var (reallocatedOrderProductSerialNumInfo, unallocatedPkgs) = AllocateSatisfiedPkgsFromOrders(packageToCreateInfos);
			AllocateUnpackedPkgs(unallocatedPkgs);

			return reallocatedOrderProductSerialNumInfo;

			(Dictionary<(ZGuid, string), (ZGuid, ZGuid, ZGuid)>, Dictionary<(ZGuid, ZGuid, ZGuid), Dictionary<ZGuid, Queue<(WhsOrderLine, WhsOrder)>>>) PopulateInformation()
			{
				var orderInfoKeysViaPartSerial = new Dictionary<(ZGuid, string), (ZGuid, ZGuid, ZGuid)>();
				var queueByPartOrgLocDictionary = new Dictionary<(ZGuid, ZGuid, ZGuid), Dictionary<ZGuid, Queue<(WhsOrderLine, WhsOrder)>>>();
				foreach (var orderInfo in orderProductSerialNumInfo)
				{
					var infoKey = (orderInfo.Key.PartPK, orderInfo.Value.Order.WD_OH_Client, orderInfo.Value.LocationPK);
					orderInfoKeysViaPartSerial[orderInfo.Key] = infoKey;

					if (!queueByPartOrgLocDictionary.TryGetValue(infoKey, out var orderQueueDictionary))
					{
						queueByPartOrgLocDictionary[infoKey] = orderQueueDictionary = new Dictionary<ZGuid, Queue<(WhsOrderLine, WhsOrder)>>();
					}

					if (!orderQueueDictionary.TryGetValue(orderInfo.Value.Order.PK, out var orderlineQueue))
					{
						orderQueueDictionary[orderInfo.Value.Order.PK] = orderlineQueue = new Queue<(WhsOrderLine, WhsOrder)>();
					}

					var orderLine = orderInfo.Value.OrderLine;
					orderLine.ClearReleaseLines();
					orderlineQueue.Enqueue((orderLine, orderInfo.Value.Order));
				}

				return (orderInfoKeysViaPartSerial, queueByPartOrgLocDictionary);
			}

			(Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)>, List<AttributeNeutralPackageInfo>)
				AllocateSatisfiedPkgsFromOrders(List<AttributeNeutralPackageInfo> pkgInfos)
			{
				var reallocatedInfos = new Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)>();
				var unallocatedPkgInfos = new List<AttributeNeutralPackageInfo>();
				for (var index = pkgInfos.Count - 1; index >= 0; index--)
				{
					var pkgInfo = pkgInfos[index];
					var swapped = false;
					var productClientLocationKey = orderInfoKeysViaSerial[(pkgInfo.ProductPK, pkgInfo.SerialNumbers[0])];
					if (orderInfoDictionary.TryGetValue(productClientLocationKey, out var orderLineQueues))
					{
						swapped = SwapValidPickLinesInPackages(pkgInfo, orderLineQueues, reallocatedInfos);
					}

					if (!swapped)
					{
						pkgInfos.RemoveAt(index);
						unallocatedPkgInfos.Add(pkgInfo);
					}
				}
				return (reallocatedInfos, unallocatedPkgInfos);
			}

			bool SwapValidPickLinesInPackages(
				AttributeNeutralPackageInfo pkgInfo,
				Dictionary<ZGuid, Queue<(WhsOrderLine, WhsOrder)>> orderLineQueues,
				Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)> reallocatedInfos)
			{
				foreach (var orderLineQueue in orderLineQueues)
				{
					if (orderLineQueue.Value.Count >= pkgInfo.SerialNumbers.Length)
					{
						var serialsPacked = false;
						foreach (var serial in pkgInfo.SerialNumbers)
						{
							if (orderProductSerialNumInfo.TryGetValue((pkgInfo.ProductPK, serial), out var result))
							{
								var (queueOrderLine, queuedOrder) = orderLineQueue.Value.Dequeue();
								result.PickLine.WZ_WE_TransactionLine = queueOrderLine.PK;
								reallocatedInfos[(pkgInfo.ProductPK, serial)] = (result.PickLine, queueOrderLine, queuedOrder, result.LocationPK);
								serialsPacked = true;
							}
						}
						if (serialsPacked)
						{
							return serialsPacked;
						}
					}
				}
				return false;
			}

			void AllocateUnpackedPkgs(List<AttributeNeutralPackageInfo> unallocatedPkgs)
			{
				foreach (var pkgInfo in unallocatedPkgs)
				{
					var productClientLocationKey = orderInfoKeysViaSerial[(pkgInfo.ProductPK, pkgInfo.SerialNumbers[0])];
					if (orderInfoDictionary.TryGetValue(productClientLocationKey, out var queues))
					{
						foreach (var serial in pkgInfo.SerialNumbers)
						{
							if (orderProductSerialNumInfo.TryGetValue((pkgInfo.ProductPK, serial), out var result))
							{
								var (line, _) = queues.Values.First(q => q.Count > 0).Dequeue();
								result.PickLine.WZ_WE_TransactionLine = line.PK;
							}
						}
					}
				}
			}
		}

		static Dictionary<ZString, decimal> LoadInventoryQuantitiesPerPallet(BusinessObjectFactory factory, IEnumerable<string> palletIDs, ZGuid whsPK)
		{
			var rawQuery = $@"
SELECT
	IIF(WE_PalletID <> '', WE_PalletID, WE_TransferFromPalletId) AS WE_PalletID,
	WE_StockOnHand
FROM
	dbo.WhsDocketLine
	JOIN dbo.WhsDocket ON WE_WD = WD_PK
WHERE (1=1)
	AND
	(
		(WE_TransferFromPalletId IN (SELECT Value FROM @PalletIDs) AND WE_TransferFromPalletId <> '')
		OR
		(WE_PalletID IN (SELECT Value FROM @PalletIDs) AND WE_PalletID <> '')
	)
	AND WD_WW_Whs = @Whs
	AND WE_StockOnHand > 0";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@PalletIDs", palletIDs.ToArray(), WhsDocketLineSchema.WE_TransferFromPalletId, isTableValued: true),
				ZSqlParameter.New("@Whs", whsPK, WhsDocketSchema.WD_WW_Whs)
			};

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(rawQuery, sqlParams);

			return collection
				.GroupBy(i => ((ZString)i[WhsDocketLineSchema.WE_PalletID]).ToUpper())
				.ToDictionary(i => i.Key, i => i.Sum(l => ((ZDecimal)l[WhsDocketLineSchema.WE_StockOnHand])));
		}

		static bool IsSingleOrderPackage(Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)> orderProductSerialNumInfo, AttributeNeutralPackageInfo packageToCreateInfo)
		{
			var orderPK = ZGuid.Empty;
			var isSingleOrderPackage = true;

			foreach (var serialNumber in packageToCreateInfo.SerialNumbers)
			{
				if (orderProductSerialNumInfo.TryGetValue((packageToCreateInfo.ProductPK, serialNumber), out var info))
				{
					if (orderPK == ZGuid.Empty)
					{
						orderPK = info.Order.PK;
					}
					else if (orderPK != info.Order.PK)
					{
						isSingleOrderPackage = false;
						break;
					}
				}
			}

			return isSingleOrderPackage;
		}

		Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)> LoadOrderInfo((Guid, string[])[] serialsByProductPK)
		{
			var serialsByProductLength = serialsByProductPK.Length;
			var productPKs = new HashSet<Guid>(serialsByProductLength);
			var serialNumbers = new List<string>(serialsByProductLength);
			var partSerialSet = new HashSet<(ZGuid PartPK, string SerialNumber)>(serialsByProductLength);
			foreach (var (productPK, serials) in serialsByProductPK)
			{
				productPKs.Add(productPK);
				serialNumbers.AddRange(serials);
				serials.ForEach(s => partSerialSet.Add((productPK, s)));
			}

			// This query currently collects all the information for all the combinations of the provided ProductPKs and serial numbers.
			// This makes the query more performant and prevents excess sql plan creation and churn.
			var rawQuery = $@"
SELECT
	orderLine.WE_WD,
	orderLine.WE_PK,
	WZ_PK,
	WI_SerialNumber,
	WI_OP,
	ISNULL(inventoryLine.WE_WL, WI_WL) AS WI_WL
FROM
	dbo.WhsInventoryView
	JOIN dbo.WhsPickLine ON WZ_WE_InventoryLine = WI_WE_InDocketLine
	JOIN dbo.WhsDocketLine orderLine ON WE_PK = WZ_WE_TransactionLine
	LEFT JOIN dbo.WhsDocketLine inventoryLine ON WZ_WE_OriginalPickedInventoryLine = inventoryLine.WE_PK
WHERE (1=1)
	AND WI_TotalUnits > 0
	AND WI_OP IN (SELECT Value FROM @ProductPKs)
	AND WI_SerialNumber <> ''
	AND WI_SerialNumber IN (SELECT Value FROM @SerialNumbers)
	AND (WZ_PickedDateTime IS NOT NULL OR WZ_WE_OriginalPickedInventoryLine IS NOT NULL)
	AND orderLine.WE_WE_ParentDocketLine IS NULL";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@ProductPKs", productPKs.ToArray(), WhsInventoryViewSchema.WI_OP, isTableValued: true),
				ZSqlParameter.New("@SerialNumbers", serialNumbers, WhsInventoryViewSchema.WI_SerialNumber, isTableValued: true)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(rawQuery, sqlParams);

			// Filter the results to the orders, orderlines and inventories that match the productPK and serial number pairs scanned by the user.
			var filteredCollection
				= collection.Where(c => partSerialSet.Contains(((ZGuid)c[WhsInventoryViewSchema.WI_OP], ((ZString)c[WhsInventoryViewSchema.WI_SerialNumber]).ToUpper()))).ToArray();

			var orderPKs
				= filteredCollection
					.Select(c => (ZGuid)c[WhsDocketLineSchema.WE_WD])
					.Distinct().ToArray();

			var ordersQuery = new ZQuery(WhsDocketSchema.PK, orderPKs);
			var orders = Factory.Load<WhsOrder>(ordersQuery).ToDictionary(ol => ol.PK);

			var orderLineQuery = new ZQuery(WhsDocketLineSchema.PK, filteredCollection.Select(c => (ZGuid)c[WhsDocketLineSchema.PK]).Distinct());
			var orderLines = Factory.Load<WhsOrderLine>(orderLineQuery).ToDictionary(ol => ol.PK);

			var pickLineQuery = new ZQuery(WhsPickLineSchema.PK, filteredCollection.Select(c => (ZGuid)c[WhsPickLineSchema.PK]).Distinct());
			var pickLines = Factory.Load<WhsPickLine>(pickLineQuery).ToDictionary(pl => pl.PK);

			var result = new Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)>();
			foreach (var dBO in filteredCollection)
			{
				var partPK = (ZGuid)dBO[WhsInventoryViewSchema.WI_OP];
				var serialNumber = ((ZString)dBO[WhsInventoryViewSchema.WI_SerialNumber]).ToUpper();
				var pickLine = pickLines[(ZGuid)dBO[WhsPickLineSchema.PK]];
				var orderLine = orderLines[(ZGuid)dBO[WhsDocketLineSchema.PK]];
				var order = orders[(ZGuid)dBO[WhsDocketLineSchema.WE_WD]];
				result[(partPK, serialNumber)] = (pickLine, orderLine, order, (ZGuid)dBO[WhsInventoryViewSchema.WI_WL]);
			}

			return result;
		}

		void PackSerialNumbers(
			WebServiceResponse response,
			AttributeNeutralPackageInfo packageToCreateInfo,
			WhsOrder order,
			Dictionary<(ZGuid PartPK, string SerialNumber), (WhsPickLine PickLine, WhsOrderLine OrderLine, WhsOrder Order, ZGuid LocationPK)> orderProductSerialNumInfo,
			Dictionary<ZString, decimal> totalPalletQuantities)
		{
			var package = CreatePackage(packageToCreateInfo, order, totalPalletQuantities);
			var orderLineWithReleaseLinesBySerialNumber
				= orderProductSerialNumInfo
					.DistinctBy(j => j.Value.OrderLine.PK)
					.ToDictionary(
						i => i.Value.OrderLine.PK,
						i => i.Value.OrderLine.ReleaseLines.Cast<WhsReleaseLine>().ToDictionary(r => r.SerialNumber.ToUpper()));

			foreach (var serialNumber in packageToCreateInfo.SerialNumbers)
			{
				if (orderProductSerialNumInfo.TryGetValue((packageToCreateInfo.ProductPK, serialNumber), out var value))
				{
					try
					{
						var releaseLine = orderLineWithReleaseLinesBySerialNumber[value.OrderLine.PK][serialNumber];
						PickLineUpdater.PackPickLineForAttributeNeutral(value.PickLine, releaseLine, package);
					}
					catch (Exception exception)
					{
						response.LogBusinessValidationError(
							Res.GetString(
								"3e25b615-e264-443c-814b-a64d917b4f85",
								"Auto-packing failed. '{0}'",
								exception.Message));
					}
				}
				else
				{
					// This branch should never happen but error report added incase it occurs
					ErrorReporter.ReportOnce($"Attempting to pack serial number '{serialNumber}' and productPK '{packageToCreateInfo.ProductPK}' but orderLine dictionary does not contain values.");
				}
			}

			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
		}

		static PkgPackage CreatePackage(AttributeNeutralPackageInfo packageInfo, WhsOrder order, Dictionary<ZString, decimal> totalPalletQuantities)
		{
			var package = order.PackageJob.Packages.AddNew(packageInfo.PackType);
			AllocatePackageLabelsHelper.FillPackagePropertiesFromProduct(package, order.Factory.Load<OrgSupplierPart>(packageInfo.ProductPK), packageInfo.PackType);

			var pickedPalletID = packageInfo.PalletID.Trim().ToUpper();
			if (!string.IsNullOrWhiteSpace(pickedPalletID)
				&& totalPalletQuantities.TryGetValue(pickedPalletID, out var totalPalletQty)
				&& packageInfo.ExpectedQuantityInPackage == totalPalletQty)
			{
				package.KP_PackageID = pickedPalletID;
			}
			return package;
		}

		#endregion
	}
}
