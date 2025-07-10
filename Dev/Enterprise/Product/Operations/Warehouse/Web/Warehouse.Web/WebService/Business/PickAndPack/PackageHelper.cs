using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class PackageHelper
	{
		#region GetPackageRelatedProductInfos

		public static WhsPackageProductInfo[] GetPackageRelatedProductInfos(BusinessObjectFactory factory, PkgPackage package)
		{
			var result = new List<WhsPackageProductInfo>();
			const string rawQuery = @"
SELECT
	WE_OP,
	OP_PartNum,
	OP_Desc,
	OP_Weight,
	OP_WeightUQ,
	SUM(KI_PackedQty) AS KI_PackedQty
FROM
	dbo.PkgPackageItemDivot
	JOIN dbo.WhsPickLine ON WZ_PK = KI_ParentID AND KI_ParentTableCode = 'WZ'
	JOIN dbo.WhsDocketLine ON WE_PK = WZ_WE_TransactionLine
	JOIN dbo.OrgSupplierPart ON WE_OP = OP_PK
WHERE 
	KI_KP_Package = @PackagePK
GROUP BY 
	WE_OP, OP_PartNum, OP_Weight, OP_WeightUQ, OP_Desc";
			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@PackagePK", package.PK, PkgPackageItemDivotSchema.KI_KP_Package);

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(rawQuery, queryParams);

			foreach (var dynamicObject in dynamicCollection.Cast<DynamicBusinessObject>())
			{
				var packageInfo = new WhsPackageProductInfo();
				packageInfo.ProductPK = ((ZGuid)dynamicObject[WhsDocketLineSchema.WE_OP]).ToGuid();
				packageInfo.ProductCode = (ZString)dynamicObject[OrgSupplierPartSchema.OP_PartNum];
				packageInfo.ProductDescription = (ZString)dynamicObject[OrgSupplierPartSchema.OP_Desc];
				packageInfo.ProductWeight = (ZDecimal)dynamicObject[OrgSupplierPartSchema.OP_Weight];
				packageInfo.ProductWeightUQ = (ZString)dynamicObject[OrgSupplierPartSchema.OP_WeightUQ];
				packageInfo.ExpectedQty = (ZDecimal)dynamicObject[PkgPackageItemDivotSchema.KI_PackedQty];

				result.Add(packageInfo);
			}
			return result.ToArray();
		}

		public static DynamicBusinessObjectCollection GetPackageRelatedProductInfosWithAttributes(BusinessObjectFactory factory, ZGuid packagePK)
		{
			const string rawQuery = @"
SELECT
	WE_OP,
	WE_PartAttrib1,
	WE_PartAttrib2,
	WE_PartAttrib3,
	WE_SerialNumber,
	WE_ExpiryDate,
	WE_PackingDate,
	SUM(KI_PackedQty) AS KI_PackedQty,
	WorkOrderInventoryPK
FROM
(
	SELECT
		WE_OP,
		WE_PartAttrib1,
		WE_PartAttrib2,
		WE_PartAttrib3,
		WE_SerialNumber,
		WE_ExpiryDate,
		WE_PackingDate,
		KI_PackedQty,
		CASE
			WHEN HasBOMLinks = 1 AND WD_WP_ParentPickForReceive IS NULL THEN InventoryLine.WE_PK
			ELSE NULL
		END AS WorkOrderInventoryPK
	FROM
		dbo.PkgPackageItemDivot
		JOIN dbo.WhsPickLine ON WZ_PK = KI_ParentID AND KI_ParentTableCode = 'WZ'
		JOIN dbo.WhsDocketLine InventoryLine ON WE_PK = ISNULL(WZ_WE_OriginalPickedInventoryLine, WZ_WE_InventoryLine)
		JOIN dbo.WhsDocket InventoryDocket ON WD_PK = WE_WD
		OUTER APPLY
		(
			SELECT
				TOP 1 1 AS HasBOMLinks
			FROM
				dbo.WhsBOMInventoryPivot
			WHERE
				InventoryLine.WE_PK = WIP_WE_InventoryLine
		) BOMLinks
	WHERE
		KI_KP_Package = @PackagePK
) AS Divots
GROUP BY
	WE_OP, WE_PartAttrib1, WE_PartAttrib2, WE_PartAttrib3, WE_SerialNumber, WE_ExpiryDate, WE_PackingDate, WorkOrderInventoryPK";

			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@PackagePK", packagePK, PkgPackageItemDivotSchema.KI_KP_Package);

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(rawQuery, queryParams);
			return dynamicCollection;
		}

		#endregion

		#region ValidateAndGetPackage
		// Tested in ClosePackageTest.cs and ShortToteTest.cs
		public static PkgPackage GetPackageAndValidate(WebServiceResponse response, string packageId, ZGuid packagePk, BusinessObjectFactory factory)
		{
			PkgPackage result = null;

			var packageID = packageId.ToUpper(Culture.Invariant);
			var package = factory.Load<PkgPackage>(packagePk);
			if (package != null)
			{
				if (!package.IsClosed)
				{
					result = package;
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("24bcf5cf-c609-458f-bed9-3ef2fe6d7aa7", "Package with ID '{0}' is already closed.", packageID));
				}
			}
			else
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("e976fbaa-0933-4270-992b-67a9d9d804aa", "Package ID '{0}' does not exist.", packageID));
			}

			return result;
		}

		#endregion

		#region GetPackageAndValidateForClosing
		// Tested in ClosePackageTest.cs and ShortToteTest.cs
		public static PkgPackage GetPackageAndValidateForClosing(WebServiceResponse response, string packageId, ZGuid packagePk, BusinessObjectFactory factory)
		{
			PkgPackage result = null;
			var package = GetPackageAndValidate(response, packageId, packagePk, factory);

			if (package != null && string.IsNullOrEmpty(response.ErrorMessage) && !package.PackedItemDivots.Any(p => p.KI_PackedQty > 0m))
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("ecb1f5a1-c122-4b76-954d-09bb8b822ebf", "There is nothing packed in Package with ID '{0}'.", packageId.ToUpper(Culture.Invariant)));
			}
			else
			{
				result = package;
			}

			return result;
		}

		#endregion

		#region GetAllPackageDivotsForProduct

		public static IEnumerable<PkgPackageItemDivot> GetAllPackageDivotsForProduct(WhsOrderLine[] orderLines, PkgPackage package)
		{
			var pickLines = orderLines.SelectMany(orderLine => orderLine.PickLines);
			var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, pickLines.Select(pickLine => pickLine.PK).ToArray());
			var packageDivots = orderLines.First().Factory.Load<PkgPackageItemDivot>(query);

			return packageDivots.Intersect(package.PackedItemDivots).ToArray().OrderByDescending(divot => divot.KI_PackedQty);
		}

		#endregion

		#region GetPackageParentOrdersWithNoFinalisedReturnReceive

		public static DynamicBusinessObjectCollection GetPackageParentOrdersWithNoFinalisedReturnReceive(BusinessObjectFactory factory, string packageId)
		{
			const string rawQuery = @"
SELECT
		OH_Code,
		WD_ExternalReference,
		KP_PK
FROM
		dbo.WhsDocket WhsOrder
		JOIN dbo.OrgHeader on OH_PK = WD_OH_Client
		JOIN dbo.PkgPackageJob on KJ_ParentID = WD_PK
		JOIN dbo.PkgPackage on KP_KJ_ParentPackageJob = KJ_PK
		JOIN dbo.PkgPackageHeader on KP_KPH_PackageHeader = KPH_PK
		JOIN dbo.WhsPick on WD_WP = WP_PK
WHERE
		KPH_PackageID = @PackageId
		AND WP_PickStatus = @PickStatus
		AND @PackageDocketReference not in
		(
			SELECT
				WX_Reference
			FROM
				dbo.WhsDocket
				JOIN dbo.WhsDocketReference on WX_WD = WD_PK
			WHERE
				WD_DocketType = @ReceiveDocketType
				AND WD_DocketSubType = @ReceiveSubType
				AND WD_DocketStatus = @ReceiveDocketFinalisedStatus
				AND WX_RefType = @DocketReferenceType
				AND WD_ExternalReference = WhsOrder.WD_ExternalReference
				AND WD_WD_ParentDocket = WhsOrder.WD_PK
		)
";

			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@PackageId", packageId, PkgPackageHeaderSchema.KPH_PackageID);
			queryParams.Add("@PickStatus", PickStatus.Codes.Finalised, WhsPickSchema.WP_PickStatus);
			queryParams.Add("@ReceiveDocketFinalisedStatus", DocketStatus.Codes.Finalised, WhsDocketSchema.WD_DocketStatus);
			queryParams.Add("@DocketReferenceType", WarehouseAdditionalReferenceTypes.Codes.Other, WhsDocketReferenceSchema.WX_RefType);
			queryParams.Add("@ReceiveDocketType", DocketType.Codes.Receive, WhsDocketSchema.WD_DocketType);
			queryParams.Add("@ReceiveSubType", ReceiveType.Codes.Returns, WhsDocketSchema.WD_DocketSubType);
			queryParams.Add("@PackageDocketReference", GetDocketPackageIdReference(packageId), WhsDocketReferenceSchema.WX_Reference);

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(rawQuery, queryParams);
			return dynamicCollection;
		}

		public static string GetDocketPackageIdReference(string packageId)
		{
			return packageId.Length > WhsDocketReferenceSchema.WX_Reference.MaxLength
				? packageId.Substring(packageId.Length - WhsDocketReferenceSchema.WX_Reference.MaxLength)
				: packageId;
		}

		#endregion

		#region GetOrderUnpackedProductInfos

		public static WhsPackageProductInfo[] GetOrderUnpackedProductInfos(BusinessObjectFactory factory, ZGuid orderPK)
		{
			var result = new List<WhsPackageProductInfo>();
			const string rawQuery = @"
SELECT
	WE_OP,
	OP_PartNum,
	OP_Desc,
	OP_Weight,
	OP_WeightUQ,
	SUM(Quantity) AS ExpectedQuantity
FROM
	(
		SELECT
			WE_OP,
			WZ_Units AS Quantity
		FROM
			dbo.WhsDocketLine
			JOIN dbo.WhsPickLine ON WE_PK = WZ_WE_TransactionLine
		WHERE 
			WE_WD = @OrderPK
			AND WE_WE_ParentDocketLine IS NULL

		UNION ALL

		SELECT
			WE_OP,
			-KI_PackedQty AS Quantity
		FROM
			dbo.WhsDocketLine
			JOIN dbo.WhsPickLine ON WE_PK = WZ_WE_TransactionLine
			JOIN dbo.PkgPackageItemDivot ON WZ_PK = KI_ParentID AND KI_ParentTableCode = 'WZ'
		WHERE 
			WE_WD = @OrderPK
	) Lines
	JOIN dbo.OrgSupplierPart ON WE_OP = OP_PK
GROUP BY 
	WE_OP, OP_PartNum, OP_Weight, OP_WeightUQ, OP_Desc";

			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@OrderPK", orderPK, WhsDocketLineSchema.WE_WD);

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(rawQuery, queryParams);

			foreach (var dynamicObject in dynamicCollection.Cast<DynamicBusinessObject>().Where(o => (ZDecimal)o["ExpectedQuantity"] > 0))
			{
				var packageInfo = new WhsPackageProductInfo();
				packageInfo.ProductPK = ((ZGuid)dynamicObject[WhsDocketLineSchema.WE_OP]).ToGuid();
				packageInfo.ProductCode = (ZString)dynamicObject[OrgSupplierPartSchema.OP_PartNum];
				packageInfo.ProductDescription = (ZString)dynamicObject[OrgSupplierPartSchema.OP_Desc];
				packageInfo.ProductWeight = (ZDecimal)dynamicObject[OrgSupplierPartSchema.OP_Weight];
				packageInfo.ProductWeightUQ = (ZString)dynamicObject[OrgSupplierPartSchema.OP_WeightUQ];
				packageInfo.ExpectedQty = (ZDecimal)dynamicObject["ExpectedQuantity"];

				result.Add(packageInfo);
			}
			return result.ToArray();
		}

		#endregion

		#region GetReleaseLinesByKey

		// Tested in PickLineUpdaterTest.cs and AutoCreateCompletePalletPackagesForPickTest.cs
		public static IReadOnlyDictionary<GroupingKey, WhsReleaseLine> GetReleaseLinesByKey(WhsPickableDocketLine[] pickableDocketLines)
			=> pickableDocketLines.SelectMany(ol => ol.ReleaseLines.Cast<WhsReleaseLine>()).ToDictionary(r => r.KeyForPacking);

		#endregion
	}
}
