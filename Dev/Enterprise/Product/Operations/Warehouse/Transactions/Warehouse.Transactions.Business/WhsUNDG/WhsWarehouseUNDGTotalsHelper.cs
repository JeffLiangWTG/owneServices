using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsWarehouseUNDGTotalsHelper
	{
		#region LoadWhsWarehouseUNDGTotalsBySubstance

		public static IReadOnlyCollection<WhsWarehouseUNDGWeightAndVolume> LoadWhsWarehouseUNDGTotalsBySubstance(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid undgSubstance)
		{
			var sql = "SELECT TotalWeight, TotalWeightUQ, TotalVolume, TotalVolumeUQ FROM dbo.WhsWarehouseUNDGTotalsBySubstance(@WarehousePK, @UNDGSubstance)";
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@WarehousePK", warehousePK, WhsLocationViewSchema.WLV_WW_Whs),
				ZSqlParameter.New("@UNDGSubstance", undgSubstance, UNDGDataItemSchema.DI_DG),
			});

			return ReadWhsWarehouseUNDGWeightAndVolumeFromCollection(collection);
		}

		#endregion

		#region LoadWhsWarehouseUNDGTotalsByCountryReference

		public static IReadOnlyCollection<WhsWarehouseUNDGWeightAndVolume> LoadWhsWarehouseUNDGTotalsByCountryReference(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid undgCountryReference)
		{
			var sql = "SELECT TotalWeight, TotalWeightUQ, TotalVolume, TotalVolumeUQ FROM dbo.WhsWarehouseUNDGTotalsByCountryReference(@WarehousePK, @UNDGCountryReference)";
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@WarehousePK", warehousePK, WhsLocationViewSchema.WLV_WW_Whs),
				ZSqlParameter.New("@UNDGCountryReference", undgCountryReference, UNDGCountryReferencePivotSchema.DCP_DCR),
			});

			return ReadWhsWarehouseUNDGWeightAndVolumeFromCollection(collection);
		}

		#endregion

		#region LoadWhsWarehouseUNDGTotalsByClass

		public static IReadOnlyCollection<WhsWarehouseUNDGWeightAndVolume> LoadWhsWarehouseUNDGTotalsByClass(BusinessObjectFactory factory, ZGuid warehousePK, ZString undgClass)
		{
			var sql = "SELECT TotalWeight, TotalWeightUQ, TotalVolume, TotalVolumeUQ FROM dbo.WhsWarehouseUNDGTotalsByClass(@WarehousePK, @UNDGClass)";
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@WarehousePK", warehousePK, WhsLocationViewSchema.WLV_WW_Whs),
				ZSqlParameter.New("@UNDGClass", undgClass, WhsUNDGLimitSchema.WWD_UNDGClass),
			});

			return ReadWhsWarehouseUNDGWeightAndVolumeFromCollection(collection);
		}

		#endregion

		#region ReadWhsWarehouseUNDGWeightAndVolumeFromCollection

		static IReadOnlyCollection<WhsWarehouseUNDGWeightAndVolume> ReadWhsWarehouseUNDGWeightAndVolumeFromCollection(DynamicBusinessObjectCollection collection) =>
			collection.Select(result => new WhsWarehouseUNDGWeightAndVolume(
				(ZDecimal)result["TotalWeight"],
				(ZString)result["TotalWeightUQ"],
				(ZDecimal)result["TotalVolume"],
				(ZString)result["TotalVolumeUQ"]
			)).ToArray();

		#endregion

		#region LoadWhsWarehouseUNDGTotals

		public static IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> LoadUNDGTotalsForWarehouseInventory(BusinessObjectFactory factory, ZGuid warehousePK, ZGuid? docketToExcludePK = null)
		{
			var sql = @"SELECT UNDGSubstance, UNDGCountryReference, UNDGClass,
TotalWeight, TotalWeightLimit, TotalWeightLimitUQ,
TotalVolume, TotalVolumeLimit, TotalVolumeLimitUQ
FROM dbo.WhsWarehouseUNDGTotals(@WarehousePK, @DocketPK)";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@WarehousePK", warehousePK, WhsLocationViewSchema.WLV_WW_Whs),
				ZSqlParameter.New("@DocketPK", docketToExcludePK, docketToExcludePK.HasValue ? WhsDocketLineSchema.WE_WD : WhsDocketSchema.WD_WD_ParentDocket)
			});

			return ReadWhsWarehouseUNDGTotalsFromCollection(collection);
		}

		#endregion

		#region ReadWhsWarehouseUNDGTotalsFromCollection

		static IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> ReadWhsWarehouseUNDGTotalsFromCollection(DynamicBusinessObjectCollection collection) =>
			collection.Select(result => new WhsWarehouseUNDGTotalsInfo(
				(ZGuid)result["UNDGSubStance"],
				(ZGuid)result["UNDGCountryReference"],
				(ZString)result["UNDGClass"],
				(ZDecimal)result["TotalWeight"],
				(ZDecimal)result["TotalWeightLimit"],
				(ZString)result["TotalWeightLimitUQ"],
				(ZDecimal)result["TotalVolume"],
				(ZDecimal)result["TotalVolumeLimit"],
				(ZString)result["TotalVolumeLimitUQ"]
			)).ToArray();

		#endregion

		#region LoadProductsUNDGInfo

		public static IReadOnlyCollection<WhsProductUNDGInfo> LoadProductsUNDGInfo(BusinessObjectFactory factory, IReadOnlyCollection<ZGuid> productPKs, string weightUQ = "KG", string volumeUQ = "M3")
		{
			var sql = @"SELECT
	DI_ParentID AS ProductPK,
	DG_PK,
	DG_Code,
	DCR_PK,
	DCR_Code,
	CASE WHEN SUBSTRING(DI_IMOClass, 1, 1) IN ('1', '2', '3', '4', '5', '6', '7', '8', '9') THEN SUBSTRING(DI_IMOClass, 1, 1) ELSE DI_IMOClass END AS UNDGClass,
	ConvertedWeight.Value AS DGWeight,
	@WeightUQ AS DGWeightUQ,
	ConvertedVolume.Value AS DGVolume,
	@VolumeUQ AS DGVolumeUQ
FROM
	dbo.UNDGDataItem DI
	JOIN dbo.UNDGSubstance DG ON DI_DG = DG_PK
	LEFT JOIN dbo.UNDGCountryReferencePivot DCP ON DCP_UNNO = DG_UNNO AND DCP_Variant = DG_Variant AND DCP_Standard = DG_Standard
	LEFT JOIN dbo.UNDGCountryReference DCR ON DCP_DCR = DCR_PK
	CROSS APPLY(SELECT Value FROM dbo.ConvertWeight(DI_DGWeight, DI_UnitOfWeight, @WeightUQ)) AS ConvertedWeight
	CROSS APPLY(SELECT Value FROM dbo.ConvertVolume(DI_DGVolume, DI_UnitOfVolume, @VolumeUQ)) AS ConvertedVolume
WHERE 1 = 1
	AND DI_ParentID IN (SELECT Value FROM @ProductPKs) AND DI_ParentTableCode = 'OP'
	AND (DI_DGVolume > 0 OR DI_DGWeight > 0)";

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@ProductPKs", productPKs, UNDGDataItemSchema.DI_ParentID, isTableValued: true),
				ZSqlParameter.New("@WeightUQ", weightUQ, WhsUNDGLimitSchema.WWD_TotalWeightLimitUQ),
				ZSqlParameter.New("@VolumeUQ", volumeUQ, WhsUNDGLimitSchema.WWD_TotalVolumeLimitUQ),
			});

			return collection.Select(result => new WhsProductUNDGInfo(
				(ZGuid)result["ProductPK"],
				(ZGuid)result["DG_PK"],
				(ZString)result["DG_Code"],
				(ZGuid)result["DCR_PK"],
				(ZString)result["DCR_Code"],
				(ZString)result["UNDGClass"],
				(ZDecimal)result["DGWeight"],
				(ZString)result["DGWeightUQ"],
				(ZDecimal)result["DGVolume"],
				(ZString)result["DGVolumeUQ"]
			)).ToArray();
		}

		#endregion

		#region LoadUNDGSubstanceCodes

		public static Dictionary<ZGuid, ZString> LoadUNDGSubstanceCodes(BusinessObjectFactory factory, IReadOnlyCollection<ZGuid> undgSubstancePKs)
		{
			var sql = "SELECT DG_PK, DG_Code FROM dbo.UNDGSubstance WHERE DG_PK IN (SELECT Value FROM @UNDGSubstancePKs) ORDER BY DG_Code";
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@UNDGSubstancePKs", undgSubstancePKs, UNDGSubstanceSchema.PK, isTableValued: true)
			});

			return collection.ToDictionary(result => (ZGuid)result["DG_PK"], result => (ZString)result["DG_Code"]);
		}

		#endregion

		#region LoadUNDGCountryReferenceCodes

		public static Dictionary<ZGuid, ZString> LoadUNDGCountryReferenceCodes(BusinessObjectFactory factory, IReadOnlyCollection<ZGuid> undgCountryReferencePKs)
		{
			var sql = "SELECT DCR_PK, DCR_Code FROM dbo.UNDGCountryReference WHERE DCR_PK IN (SELECT Value FROM @UNDGCountryReferencePKs) ORDER BY DCR_Code";
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sql, new[]
			{
				ZSqlParameter.New("@UNDGCountryReferencePKs", undgCountryReferencePKs, UNDGCountryReferenceSchema.PK, isTableValued: true)
			});

			return collection.ToDictionary(result => (ZGuid)result["DCR_PK"], result => (ZString)result["DCR_Code"]);
		}

		#endregion
	}
}
