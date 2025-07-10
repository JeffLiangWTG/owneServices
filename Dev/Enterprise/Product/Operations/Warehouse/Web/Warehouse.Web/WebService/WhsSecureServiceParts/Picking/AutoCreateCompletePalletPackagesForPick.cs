using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region AutoCreateCompletePalletPackagesForPick

		[WebMethod(Description = "Auto Create Complete Pallet Packages For Pick")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse AutoCreateCompletePalletPackagesForPick(Guid pickPK, string[] palletIDs)
			=> HandleWebServiceRequest_WithValidateWarehouseAndStaff<WebServiceResponse>(r => AutoCreateCompletePalletPackagesForPickCore(r, pickPK, palletIDs));

		void AutoCreateCompletePalletPackagesForPickCore(WebServiceResponse response, Guid pickPK, string[] palletIDs)
		{
			if (pickPK == Guid.Empty)
			{
				response.LogBusinessValidationError(Res.GetString("133f84a2-4d7f-4b02-8c5e-c0db1c36a92f", "Auto-packing failed. Pick PK was empty."));
			}
			else if (palletIDs == null)
			{
				response.LogBusinessValidationError(Res.GetString("0584374b-8e03-4c8b-af82-89ea6a436869", "Auto-packing failed. Pallet IDs was null."));
			}
			else if (palletIDs.Length == 0)
			{
				response.LogBusinessValidationError(Res.GetString("5cf11cd4-82af-4106-96eb-15f2510ae96f", "Auto-packing failed. Pallet IDs was empty."));
			}
			else
			{
				var shouldSave = true;
				var (palletInfo, orderLines, orderPKs, clientPKs, pickLinePKs, originalInventoryLinePKs, originalInventoryDocketPKs) = LoadPalletInfo(pickPK, palletIDs);
				AddFetchHints(orderPKs, clientPKs, pickLinePKs, originalInventoryLinePKs, originalInventoryDocketPKs);

				var allReleaseLines
					= new Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>>(() => PackageHelper.GetReleaseLinesByKey(orderLines));
				var packageTypePerPartPKAndQty = new Dictionary<(ZGuid, decimal), string>();
				foreach (var palletID in palletIDs)
				{
					if (palletInfo.TryGetValue(palletID, out var rows))
					{
						var isAutoPkgCreationEnabled = rows.Order.ClientPickingParams?.WPP_EnableAutoPackageCreationOnPicking ?? false;
						shouldSave |= isAutoPkgCreationEnabled && PackPallet(response, palletID, allReleaseLines, packageTypePerPartPKAndQty, rows.Order, rows.Part, rows.PickLines);
					}

					if (response.Error != ErrorTypes.None)
					{
						shouldSave = false;
						break;
					}
				}

				if (shouldSave)
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
						});
				}
			}
		}

		(Dictionary<ZString, (WhsOrder Order, OrgSupplierPart Part, List<WhsPickLine> PickLines)>, WhsOrderLine[] OrderLines, IEnumerable<ZGuid> OrderPKs, IEnumerable<ZGuid> ClientPKs, IEnumerable<ZGuid> PickLinePKs, IEnumerable<ZGuid> OriginalInventoryLinePKs, IEnumerable<ZGuid> OriginalInventoryDocketPKs)
			LoadPalletInfo(Guid pickPK, string[] palletIDs)
		{
			var rawQuery = $@"
SELECT
	WI_PK,
	WI_WD,
	WI_PalletID,
	WZ_PK,
	orderDocketLine.WE_PK,
	orderDocketLine.WE_OP,
	orderDocketLine.WE_WD,
	WD_OH_Client
FROM
	dbo.WhsInventoryView
	JOIN dbo.WhsPickLine ON WI_PK = ISNULL(WZ_WE_OriginalPickedInventoryLine, WZ_WE_InventoryLine)
	JOIN dbo.WhsDocketLine orderDocketLine ON WZ_WE_TransactionLine = orderDocketLine.WE_PK
	JOIN dbo.WhsDocket ON orderDocketLine.WE_WD = WD_PK
WHERE (1=1)
	AND WD_DocketType = 'ORD'
	AND WI_PalletID IN (SELECT Value FROM @PalletIDs)
	AND WI_PalletID <> ''
	AND WD_WP = @PickPK
	AND orderDocketLine.WE_WE_ParentDocketLine IS NULL";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@PalletIDs", Array.ConvertAll(palletIDs, p => (ZString)p), WhsInventoryViewSchema.WI_PalletID, isTableValued: true),
				ZSqlParameter.New("@PickPK", pickPK, WhsDocketSchema.WD_WP)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(rawQuery, sqlParams);

			var pickLinesPKs = new ZGuid[collection.Count];
			var orderPKs = new HashSet<ZGuid>();
			var orderLinePKs = new HashSet<ZGuid>();
			var clientPKs = new HashSet<ZGuid>();
			var originalInventoryLinePKs = new HashSet<ZGuid>();
			var originalInventoryDocketPKs = new HashSet<ZGuid>();

			var infos = new (ZString PalletID, ZGuid PickLinePK, ZGuid PartPK, ZGuid OrderPK)[collection.Count];
			for (var index = 0; index < collection.Count; index++)
			{
				var item = collection[index];
				var pickLinePK = (ZGuid)item[WhsPickLineSchema.PK];
				pickLinesPKs[index] = pickLinePK;

				var orderPK = (ZGuid)item[WhsDocketLineSchema.WE_WD];
				orderPKs.Add(orderPK);

				orderLinePKs.Add((ZGuid)item[WhsDocketLineSchema.PK]);
				clientPKs.Add((ZGuid)item[WhsDocketSchema.WD_OH_Client]);
				originalInventoryLinePKs.Add((ZGuid)item[WhsInventoryViewSchema.PK]);
				originalInventoryDocketPKs.Add((ZGuid)item[WhsInventoryViewSchema.WI_WD]);
				infos[index] =
					(
						(ZString)item[WhsInventoryViewSchema.WI_PalletID],
						pickLinePK,
						(ZGuid)item[WhsDocketLineSchema.WE_OP],
						orderPK
					);
			}

			var ordersQuery = new ZQuery(WhsDocketSchema.PK, orderPKs);
			var orders = Factory.Load<WhsOrder>(ordersQuery);
			var ordersLookup = orders.ToDictionary(ol => ol.PK);

			var pickLineQuery = new ZQuery(WhsPickLineSchema.PK, pickLinesPKs);
			var pickLinesLookup = Factory.Load<WhsPickLine>(pickLineQuery).ToDictionary(pl => pl.PK);

			var partQuery = new ZQuery(OrgSupplierPartSchema.PK, infos.Select(r => r.PartPK).Distinct());
			var partsLookup = Factory.Load<OrgSupplierPart>(partQuery).ToDictionary(op => op.PK);

			var palletIDsToRemove = new HashSet<ZString>();
			var result = new Dictionary<ZString, (WhsOrder Order, OrgSupplierPart Part, List<WhsPickLine> PickLines)>();
			foreach (var (palletID, pickLinePK, partPK, orderPK) in infos)
			{
				if (result.TryGetValue(palletID, out var foundValues))
				{
					if (foundValues.Order.PK != orderPK || foundValues.Part.PK != partPK)
					{
						palletIDsToRemove.Add(palletID);
					}
					else
					{
						result[palletID].PickLines.Add(pickLinesLookup[pickLinePK]);
					}
				}
				else
				{
					result[palletID] = (ordersLookup[orderPK], partsLookup[partPK], new List<WhsPickLine>() { pickLinesLookup[pickLinePK] });
				}
			}

			foreach (var key in palletIDsToRemove)
			{
				result.Remove(key);
			}

			var orderLines = Factory.Load<WhsOrderLine>(new ZQuery(WhsDocketLineSchema.PK, orderLinePKs));
			return (result, orderLines, orderPKs, clientPKs, pickLinesPKs, originalInventoryLinePKs, originalInventoryDocketPKs);
		}

		void AddFetchHints(IEnumerable<ZGuid> orderPKs, IEnumerable<ZGuid> clientPKs, IEnumerable<ZGuid> pickLinePKs, IEnumerable<ZGuid> originalInventoryLinePKs, IEnumerable<ZGuid> originalInventoryDocketPKs)
		{
			Factory.AddFetchHint(PkgPackageItemDivotSchema.Instance, new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, pickLinePKs));

			Factory.AddFetchHint(WhsClientPickPackParamsByWhsSchema.Instance, new ZQuery(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, clientPKs));

			Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, orderPKs));
			Factory.AddFetchHint(StmALogSchema.Instance, new ZQuery(StmALogSchema.SL_Parent, orderPKs));

			Factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.PK, originalInventoryLinePKs));
			Factory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.PK, originalInventoryDocketPKs));
		}

		bool PackPallet(
			WebServiceResponse response,
			string palletID,
			Lazy<IReadOnlyDictionary<GroupingKey, WhsReleaseLine>> allReleaseLines,
			Dictionary<(ZGuid, decimal), string> packageTypeForPartPK,
			WhsOrder order,
			OrgSupplierPart part,
			List<WhsPickLine> pickLines)
		{
			var isPackingSuccessful = false;
			var package = CreatePackage(order, part, pickLines.Sum(r => r.WZ_Units), packageTypeForPartPK);
			package.KP_PackageID = palletID;

			foreach (var pickLine in pickLines)
			{
				isPackingSuccessful = PackPickLine(response, pickLine, allReleaseLines.Value[((IPackableItem)pickLine).Key], package);

				if (!isPackingSuccessful)
				{
					break;
				}
			}

			if (isPackingSuccessful)
			{
				package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			}

			return isPackingSuccessful;
		}

		PkgPackage CreatePackage(WhsOrder order, OrgSupplierPart part, decimal packQty, Dictionary<(ZGuid, decimal), string> packageTypeForPartPK)
		{
			var packType = GetPackageType(part, packQty, packageTypeForPartPK);
			var package = order.PackageJob.Packages.AddNew(packType);
			AllocatePackageLabelsHelper.FillPackagePropertiesFromProduct(package, part, packType);

			return package;
		}

		string GetPackageType(OrgSupplierPart part, decimal packQty, Dictionary<(ZGuid, decimal), string> packageTypeForPartPK)
		{
			if (!packageTypeForPartPK.TryGetValue((part.PK, packQty), out var packType))
			{
				if (packQty == part.UnitConverter.Convert(1m, PkgUnit.Pallet, PkgUnit.Unit))
				{
					packType = PkgUnit.Pallet;
				}
				else if (part.PartUnits.Count > 0)
				{
					foreach (var partUnit in part.PartUnits.Cast<OrgPartUnit>())
					{
						if (packQty == part.UnitConverter.Convert(1m, partUnit.ParentUnit, PkgUnit.Unit))
						{
							if (packType == null)
							{
								packType = partUnit.ParentUnit;
							}
							else
							{
								packType = PkgUnit.Package;
								break;
							}
						}
					}
				}
				else
				{
					packType = PkgUnit.Package;
				}

				packType ??= PkgUnit.Package;
				packageTypeForPartPK[(part.PK, packQty)] = packType;
			}

			return packType;
		}

		static bool PackPickLine(WebServiceResponse response, WhsPickLine pickLine, WhsReleaseLine releaseLine, PkgPackage packageToPackInto)
		{
			var isPackingSuccessful = false;
			if (!pickLine.IsUnpacked(pickLine.Factory))
			{
				response.LogBusinessValidationError(
						Res.GetString(
							"cdae6e69-c149-482a-b89b-de534ec97636",
							"Auto-packing failed. Pick Line to Pack should be unpacked."));
			}
			else
			{
				packageToPackInto.Pack(pickLine, releaseLine);
				isPackingSuccessful = true;
			}

			return isPackingSuccessful;
		}

		#endregion
	}
}
