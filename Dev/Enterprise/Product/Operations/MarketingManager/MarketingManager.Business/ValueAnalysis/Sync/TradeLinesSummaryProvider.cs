using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public abstract class TradeLinesSummaryProviderCommon : Disposable
	{
		protected TradeLinesSummaryProviderCommon(DbConnection connection, bool includeActuals, bool includeProspect)
		{
			Argument.NotNull(connection, "connection");

			this.Connection = connection;
			this.includeActuals = includeActuals;
			this.includeProspect = includeProspect;
		}

		#region Properties

		public readonly DbConnection Connection;

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory(Connection)); }
		}
		protected BusinessObjectFactory factory;

		public ZBool IncludeActuals
		{
			get { return includeActuals; }
		}
		readonly ZBool includeActuals;

		public ZBool IncludeProspect
		{
			get { return includeProspect; }
		}
		readonly ZBool includeProspect;

		#endregion

		#region GetForOrg

		public TradeLinesSummary GetForOrg(IOrgHeader org, ZDate from, ZDate to)
		{
			var syncRange = new TradeLinesSynchronizationRange(from, to, IncludeActuals, IncludeProspect);
			var products = Factory.Load<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_IsSystemDefined, true)).ToDictionary(x => x.MP_Code);
			var summary = new TradeLinesSummary(syncRange);
			var rawCollection = GetRawCollection(org, syncRange);
			foreach (DynamicBusinessObject bizo in rawCollection)
			{
				var line = new TradeLine(bizo, products);
				if (line.IsValid)
				{
					summary.AddData(org.PK, line);
				}
			}

			return summary;
		}

		protected abstract DynamicBusinessObjectCollection GetRawCollection(IOrgHeader org, TradeLinesSynchronizationRange syncRange);

		#endregion

		#region SQL Query

		#region SuppressResourceStringsCheckRegion

		public const string TradeLineCacheTableName = "#TradeLinesSummaryCache";

		public static Tuple<string, string>[] CreatePopulateTradeLinesSummaryCacheQuery(ZBool includeActuals, ZBool includeProspect, OrgParameterType orgParamType)
		{
			var cacheQueryList = new List<Tuple<string, string>>();
			if (includeActuals)
			{
				cacheQueryList.Add("Customs Brokerage", CreateTradeLinesSummaryCacheQuery_Variables + CreateTradeLinesSummaryCacheQuery_CustomsBrokerage(orgParamType));
				cacheQueryList.Add("Port Transport", CreateTradeLinesSummaryCacheQuery_Variables + CreateTradeLinesSummaryCacheQuery_PortTransport(orgParamType));
				cacheQueryList.Add("Shipments", CreateTradeLinesSummaryCacheQuery_Variables + CreateTradeLinesSummaryCacheQuery_Shipments(orgParamType));
				cacheQueryList.Add("Warehouse", CreateTradeLinesSummaryCacheQuery_Variables + CreateTradeLinesSummaryCacheQuery_Warehouse(orgParamType));
			}

			if (includeProspect)
			{
				cacheQueryList.Add("Quotations", CreateTradeLinesSummaryCacheQuery_Quotations(orgParamType));
			}

			return cacheQueryList.ToArray();
		}

		public enum OrgParameterType
		{
			None,
			SingleValue,
			TableValue
		}

		static string GenerateUnionedSelectForOrgFilter(string insertPart, string selectPart, OrgParameterType orgParamType, params string[] columnsToCompare)
		{
			if (orgParamType == OrgParameterType.None)
			{
				return insertPart + System.Environment.NewLine + selectPart;
			}
			else
			{
				var orgWhereClauses =
					orgParamType == OrgParameterType.SingleValue
						? columnsToCompare.Select(col => string.Format(CultureInfo.InvariantCulture, "{0} = {1}", col, OrgPkParamName)).ToArray()
						: columnsToCompare.Select(col => string.Format(CultureInfo.InvariantCulture, "{0} IN (SELECT Value FROM {1})", col, OrgPkParamName)).ToArray();

				if (orgWhereClauses.Length == 1)
				{
					return insertPart + System.Environment.NewLine + selectPart + " WHERE " + orgWhereClauses[0];
				}
				else
				{
					return @"
WITH Cte AS
(
	" + selectPart + @"
)
" + insertPart
+ System.Environment.NewLine +
string.Join(System.Environment.NewLine + "UNION" + System.Environment.NewLine, orgWhereClauses.Select(x => @"
SELECT *
FROM Cte
WHERE " + x));
				}
			}
		}

		static string GetOrgSqlWhereClause(OrgParameterType orgParamType, params string[] columnsToCompare)
		{
			if (orgParamType == OrgParameterType.None)
			{
				return string.Empty;
			}
			else if (orgParamType == OrgParameterType.SingleValue)
			{
				if (columnsToCompare.Length == 1)
				{
					return string.Format(CultureInfo.InvariantCulture, "AND {0} = {1}", OrgPkParamName, columnsToCompare[0]);
				}
				else
				{
					return string.Format(CultureInfo.InvariantCulture, "AND {0} IN ({1})",
						OrgPkParamName,
						string.Join(", ", columnsToCompare));
				}
			}
			else
			{
				return
					"AND ("
					+ string.Join(" OR ",
						columnsToCompare.Select(orgColumn => string.Format(CultureInfo.InvariantCulture, "{0} IN (SELECT Value FROM {1})", orgColumn, OrgPkParamName)))
					+ ")";
			}
		}

		public const string CreateTradeLinesSummaryCacheQuery_TableDeclaration =
@"CREATE TABLE #TradeLinesSummaryCache
(
	MainOrg uniqueidentifier,
	Supplier uniqueidentifier,
	Buyer uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(3),
	Destination uniqueidentifier,
	DestinationTableCode varchar(3),
	Warehouse uniqueidentifier,
	Status tinyint,
	Product varchar(20),
	Service varchar(3),
	Mode varchar(3),
	TradeLaneType varchar(3),
	PeriodStart date,
	PeriodLastTrade smalldatetime,
	JobCount int,
	PalletCount int,
	LineCount int,
	WeightVolume decimal(12, 3),
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3),
	ChargeableAmount decimal(12, 3),
	ChargeableUnits varchar(2),
	TEU decimal(10, 2),
	SupplierPart uniqueidentifier,
	JobCompany uniqueidentifier,
	Currency varchar(3),
	JobRevenue decimal(19, 4),
	JobCost decimal(19, 4),
	MainOrgRevenue decimal(19, 4),
	MainOrgCost decimal(19, 4),
	ChargeCompany uniqueidentifier,
	RelatedJobID uniqueidentifier
)";

		const string CreateTradeLinesSummaryCacheQuery_Variables =
@"
DECLARE @CountMaxValue int = 2147483647
DECLARE @WeightVolumeMaxValue decimal(12, 3) = 999999999.000
DECLARE @RevenueCostMaxValue money = 900000000000000.0000
DECLARE @SmallDateTimeMinValue smalldatetime = '1900-01-01'
DECLARE @SmallDateTimeMaxValue smalldatetime = '2079-06-06'
";

		#region Shipments
		static string CreateTradeLinesSummaryCacheQuery_Shipments(OrgParameterType orgParamType)
		{
			return
@"
--------------
-- Shipments
--------------

CREATE TABLE #Shipment
(
	JS_PK uniqueidentifier,
	JH_PK uniqueidentifier,
	JH_GC uniqueidentifier,
	JS_E2_OA_OH_Consignor uniqueidentifier,
	JS_E2_OA_OH_Consignee uniqueidentifier,
	OA_OH_LocalClient uniqueidentifier,
	OA_OH_OverseasClient uniqueidentifier
);

" + GenerateUnionedSelectForOrgFilter("INSERT INTO #Shipment", @"
SELECT
	JobShipment.JS_PK,
	JobHeader.JH_PK,
	JobHeader.JH_GC,
	JS_E2_OA_OH_Consignor,
	JS_E2_OA_OH_Consignee,
	OA_OH_LocalClient = OA_LocalClient.OA_OH,
	OA_OH_OverseasClient = OA_OverseasClient.OA_OH
FROM
	dbo.JobShipment
	OUTER APPLY
	(
		SELECT OA_OH [JS_E2_OA_OH_Consignor]
		FROM
			dbo.JobDocAddress
			JOIN dbo.OrgAddress ON E2_OA_Address = OA_PK
		WHERE
			E2_ParentID = JS_PK
			AND E2_AddressType = 'CRD'
	) ConsignorAddress
	OUTER APPLY
	(
		SELECT OA_OH [JS_E2_OA_OH_Consignee]
		FROM
			dbo.JobDocAddress
			JOIN dbo.OrgAddress ON E2_OA_Address = OA_PK
		WHERE
			E2_ParentID = JS_PK
			AND E2_AddressType = 'CED'
	) ConsigneeAddress
	LEFT JOIN dbo.JobHeader ON JH_ParentID = JobShipment.JS_PK AND JH_ParentTableCode = 'JS'
	LEFT JOIN dbo.OrgAddress OA_LocalClient ON JH_OA_LocalChargesAddr = OA_LocalClient.OA_PK
	LEFT JOIN dbo.OrgAddress OA_OverseasClient ON JH_OA_AgentCollectAddr = OA_OverseasClient.OA_PK
WHERE
	JS_IsCancelled = 0
	AND (JS_IsForwardRegistered = 1 OR JS_IsShipping = 1)
	AND JS_ShipmentType <> 'CLD'
	AND JS_ShipmentType <> 'CLB'
	AND COALESCE(JS_E_DEP, JS_SystemCreateTimeUtc) >= @From
	AND COALESCE(JS_E_DEP, JS_SystemCreateTimeUtc) < @To
	", orgParamType, "OA_OH_LocalClient", "OA_OH_OverseasClient", "JS_E2_OA_OH_Consignor", "JS_E2_OA_OH_Consignee") +
@"

CREATE TABLE #ShipmentDetail
(
	JS_PK uniqueidentifier,
	Consignor uniqueidentifier,
	Consignee uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	Product varchar(3),
	Mode varchar(3),
	TradeLaneType varchar(3),
	PeriodStart date,
	TradeTime smalldatetime,
	JobCount int,
	WeightVolume decimal(12, 3),
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3),
	ChargeableWeight decimal(12, 3),
	ChargeableVolume decimal(12, 3),
	TEU decimal(10, 2)
);

INSERT INTO #ShipmentDetail
SELECT
	[JS_PK] = JobShipment.JS_PK,
	[Consignor] = JS_E2_OA_OH_Consignor,
	[Consignee] = JS_E2_OA_OH_Consignee,
	[Origin] = OriginUNLOCO.RL_PK,
	[OriginTableCode] = CASE WHEN OriginUNLOCO.RL_PK IS NOT NULL THEN 'RL' ELSE '' END,
	[Destination] = DestinationUNLOCO.RL_PK,
	[DestinationTableCode] = CASE WHEN DestinationUNLOCO.RL_PK IS NOT NULL THEN 'RL' ELSE '' END,
	[Product] = 
		CASE 
			WHEN JS_IsShipping = 0 THEN 'SHP'
			ELSE 'LGY'
		END,
	[Mode] =
		CASE 
			WHEN JS_IsShipping = 0 THEN JS_TransportMode
			WHEN JS_ShipmentStatus IN ('WFI', 'CNF', 'ESI') THEN 'BOL'
			WHEN JS_ShipmentStatus IN ('WEB', 'BKD', 'WTL', 'EBK')  THEN 'BRK'
			ELSE JS_TransportMode
		END,
	[TradeLaneType] = JS_PackingMode,
	[PeriodStart] = DATEADD(m, DATEDIFF(m, 0, COALESCE(JS_E_DEP, JS_SystemCreateTimeUtc)), 0),
	[TradeTime] = COALESCE(JS_E_DEP, JS_SystemCreateTimeUtc),
	[JobCount] =
		CASE
			WHEN JS_TransportMode IN ('AIR', 'SEA', 'FSA', 'FAS', 'ROA', 'RAI', 'COU') OR JS_IsShipping = 1 THEN 1
			ELSE 0 
		END,
	[WeightVolume] =
		CASE
			WHEN JS_TransportMode = 'AIR' THEN ConvertedActualWeight.Value
			WHEN JS_TransportMode = 'SEA' AND JS_PackingMode = 'FCL' THEN COALESCE(ShippingContainers.TEUs, PackLines.Shipment_TEU, 0)
			WHEN JS_TransportMode = 'SEA' AND (JS_PackingMode = 'LCL' OR JS_IsShipping = 1) THEN ConvertedActualVolume.Value
			ELSE 0
		END,
	[WeightAmount] = ConvertedActualWeight.Value,
	[VolumeM3] = ConvertedActualVolume.Value,
	[ChargeableWeight] = ConvertedChargeableWeight.Value,
	[ChargeableVolume] = ConvertedChargeableVolume.Value,
	[TEU] =
		CASE
			WHEN JS_TransportMode = 'SEA' AND JS_PackingMode = 'FCL' THEN COALESCE(ShippingContainers.TEUs, PackLines.Shipment_TEU, 0)
			ELSE ISNULL(PackLines.Shipment_TEU, 0)
		END
FROM
	(	
		SELECT DISTINCT JS_PK, JS_E2_OA_OH_Consignor, JS_E2_OA_OH_Consignee
		FROM #Shipment
	) Shipment
	JOIN dbo.JobShipment ON Shipment.JS_PK = JobShipment.JS_PK
	LEFT JOIN 
	( 
		SELECT 
			JS_PK, 
			Shipment_TEU = SUM(JC_ContainerCount * RC_TEU) 
		FROM 
			dbo.JobContainer
			JOIN dbo.RefContainer ON RC_PK = JC_RC 
			JOIN 
			( 
				SELECT 
					DISTINCT JS_PK, J6_JC
				FROM 
					dbo.JobShipment
					JOIN dbo.JobPackLines ON JL_JS = JS_PK 
					JOIN dbo.JobContainerPackPivot ON J6_JL = JL_PK 
				WHERE 
					JS_TransportMode = 'SEA'
					AND JS_PackingMode = 'FCL'
					AND JL_FreightMode = 'OUT'
			) Containers ON J6_JC = JC_PK
		GROUP BY 
			JS_PK
	) PackLines ON JobShipment.JS_PK = PackLines.JS_PK	
	LEFT JOIN 
	( 
		SELECT 
			JC_JS_FCLBookingOnlyLink, 
			TEUs = SUM(JC_ContainerCount * RC_TEU)
		FROM 
			dbo.JobContainer
			JOIN dbo.RefContainer ON RC_PK = JC_RC 
			JOIN dbo.JobShipment ON JS_PK = JC_JS_FCLBookingOnlyLink 
		WHERE 
			JS_IsShipping = 1
			AND JC_Purpose = CASE WHEN JS_ShipmentStatus in ('CNF', 'WFI') THEN 'REL' ELSE 'BKD' END
		GROUP BY 
			JC_JS_FCLBookingOnlyLink
	) AS ShippingContainers ON ShippingContainers.JC_JS_FCLBookingOnlyLink = JobShipment.JS_PK

	LEFT JOIN dbo.RefUNLOCO OriginUNLOCO ON JS_RL_NKOrigin = OriginUNLOCO.RL_Code
	LEFT JOIN dbo.RefUNLOCO DestinationUNLOCO ON JS_RL_NKDestination = DestinationUNLOCO.RL_Code

	CROSS APPLY dbo.ConvertWeight(JS_ActualWeight, JS_UnitOfWeight, 'T') AS ConvertedActualWeight
	CROSS APPLY dbo.ConvertVolume(JS_ActualVolume, JS_UnitOfVolume, 'M3') AS ConvertedActualVolume
	CROSS APPLY dbo.IsUnitMetric(JS_UnitOfWeight, JS_UnitOfVolume) AS IsUnitMetric
	CROSS APPLY dbo.GetChargeableUnitsForTransportMode(JS_TransportMode, IsUnitMetric.Value) ChargeableUnit
	CROSS APPLY dbo.ConvertWeight(JS_ActualChargeable, ChargeableUnit.Value, 'T') AS ConvertedChargeableWeight
	CROSS APPLY dbo.ConvertVolume(JS_ActualChargeable, ChargeableUnit.Value, 'M3') AS ConvertedChargeableVolume

CREATE TABLE #ShipmentOrg
(
	MainOrg uniqueidentifier,
	JS_PK uniqueidentifier,
	JH_PK uniqueidentifier
)

INSERT INTO #ShipmentOrg
SELECT [MainOrg] = JS_E2_OA_OH_Consignor, JS_PK, JH_PK FROM #Shipment WHERE JS_E2_OA_OH_Consignor IS NOT NULL
UNION ALL
SELECT [MainOrg] = JS_E2_OA_OH_Consignee, JS_PK, JH_PK FROM #Shipment WHERE JS_E2_OA_OH_Consignee IS NOT NULL 
	AND (JS_E2_OA_OH_Consignee != JS_E2_OA_OH_Consignor OR JS_E2_OA_OH_Consignor IS NULL)
UNION ALL
SELECT [MainOrg] = OA_OH_LocalClient, JS_PK, JH_PK FROM #Shipment WHERE OA_OH_LocalClient IS NOT NULL 
	AND (OA_OH_LocalClient != JS_E2_OA_OH_Consignor OR JS_E2_OA_OH_Consignor IS NULL) 
	AND (OA_OH_LocalClient != JS_E2_OA_OH_Consignee OR JS_E2_OA_OH_Consignee IS NULL)
UNION ALL
SELECT [MainOrg] = OA_OH_OverseasClient, JS_PK, JH_PK FROM #Shipment WHERE OA_OH_OverseasClient IS NOT NULL 
	AND (OA_OH_OverseasClient != JS_E2_OA_OH_Consignor OR JS_E2_OA_OH_Consignor IS NULL) 
	AND (OA_OH_OverseasClient != JS_E2_OA_OH_Consignee OR JS_E2_OA_OH_Consignee IS NULL) 
	AND (OA_OH_OverseasClient != OA_OH_LocalClient OR OA_OH_LocalClient IS NULL)

CREATE TABLE #ShipmentPeriod
(
	MainOrg uniqueidentifier,
	Consignor uniqueidentifier,
	Consignee uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	Product varchar(3),
	Mode varchar(3),
	TradeLaneType varchar(3),
	PeriodStart date,
	PeriodLastTrade smalldatetime,
	JobCount int,
	WeightVolume decimal(12, 3),
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3),
	ChargeableAmount decimal(12, 3),
	ChargeableUnits varchar(2),
	TEU decimal(10, 2)
);

INSERT INTO #ShipmentPeriod
SELECT 
	[MainOrg] = MainOrg,
	[Consignor] = Consignor,
	[Consignee] = Consignee,
	[Origin] = Origin,
	[OriginTableCode] = OriginTableCode,
	[Destination] = Destination,
	[DestinationTableCode] = DestinationTableCode,
	[Product] = Product,
	[Mode] = Mode,
	[TradeLaneType] = TradeLaneType,
	[PeriodStart] = PeriodStart,
	[PeriodLastTrade] = MAX(PeriodLastTrade),
	[JobCount] = CASE WHEN SUM(JobCount) <= @CountMaxValue THEN SUM(JobCount) ELSE @CountMaxValue END,
	[WeightVolume] = CASE WHEN SUM(WeightVolume) <= @WeightVolumeMaxValue THEN SUM(WeightVolume) ELSE @WeightVolumeMaxValue END,
	[WeightAmount] = CASE WHEN SUM(WeightAmount) <= @WeightVolumeMaxValue THEN SUM(WeightAmount) ELSE @WeightVolumeMaxValue END,
	[VolumeM3] = CASE WHEN SUM(VolumeM3) <= @WeightVolumeMaxValue THEN SUM(VolumeM3) ELSE @WeightVolumeMaxValue END,
	[ChargeableAmount] = CASE WHEN SUM(ChargeableWeight + ChargeableVolume) <= @WeightVolumeMaxValue THEN SUM(ChargeableWeight + ChargeableVolume) ELSE @WeightVolumeMaxValue END, -- Note: Each grouping will only have ChargeableWeight OR ChargeableVolume (not both)
	[ChargeableUnits] = 
		CASE
			WHEN SUM(ChargeableWeight) > 0 THEN 'T'
			WHEN SUM(ChargeableVolume) > 0 THEN 'M3'
			ELSE ''
		END,
	 [TEU] = SUM(TEU)
FROM
	(
		SELECT
			MainOrg,
			Detail.JS_PK,
			Consignor,
			Consignee,
			Origin,
			OriginTableCode,
			Destination,
			DestinationTableCode,
			Product,
			Mode,
			TradeLaneType,
			PeriodStart,
			PeriodLastTrade = MAX(TradeTime),
			JobCount = MAX(JobCount),
			WeightVolume = MAX(WeightVolume),
			WeightAmount = MAX(WeightAmount),
			VolumeM3 = MAX(VolumeM3),
			ChargeableWeight = MAX(ChargeableWeight),
			ChargeableVolume = MAX(ChargeableVolume),
			TEU = MAX(TEU)
		FROM
			#ShipmentDetail Detail
			JOIN #ShipmentOrg Org ON Detail.JS_PK = Org.JS_PK
		GROUP BY
			Detail.JS_PK,
			MainOrg,
			Consignor,
			Consignee,
			Origin,
			OriginTableCode,
			Destination,
			DestinationTableCode,
			Product,
			Mode,
			TradeLaneType,
			PeriodStart
	) a
GROUP BY
	MainOrg,
	Consignor,
	Consignee,
	Origin,
	OriginTableCode,
	Destination,
	DestinationTableCode,
	Product,
	Mode,
	TradeLaneType,
	PeriodStart

CREATE TABLE #ShipmentCharge
(
	MainOrg uniqueidentifier,
	JH_PK uniqueidentifier,
	Currency varchar(3),
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4)
)

	INSERT INTO #ShipmentCharge
	SELECT
		[MainOrg] = MainOrg,
		[JH_PK]	= JCA_JH,
		[Currency] = GC_RX_NKLocalCurrency,
		[JobRevenue]	 = CASE WHEN SUM(JCA_Revenue) <= @RevenueCostMaxValue THEN SUM(JCA_Revenue) ELSE @RevenueCostMaxValue END,
		[JobCost]		 = CASE WHEN ABS(SUM(JCA_Cost)) <= @RevenueCostMaxValue THEN SUM(JCA_Cost) WHEN SUM(JCA_Cost) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END,
		[MainOrgRevenue] = CASE WHEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Revenue ELSE 0 END) <= @RevenueCostMaxValue THEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Revenue ELSE 0 END) ELSE @RevenueCostMaxValue END,
		[MainOrgCost]	 = CASE WHEN ABS(SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END)) <= @RevenueCostMaxValue THEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END) WHEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END
	FROM
			dbo.RptDtJobCostingDataAmountByJob
			JOIN #ShipmentOrg ShipmentOrg on ShipmentOrg.JH_PK = JCA_JH
			JOIN dbo.GlbCompany on JCA_GC = GC_PK
	GROUP BY
		JCA_JH,
		MainOrg,
		GC_RX_NKLocalCurrency

CREATE TABLE #ShipmentPeriodCharge
(
	MainOrg uniqueidentifier,
	Consignor uniqueidentifier,
	Consignee uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	Product varchar(3),
	Mode varchar(3),
	TradeLaneType varchar(3),
	PeriodStart date,
	Currency varchar(3),
	ChargeCompany uniqueidentifier,
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4)
)

INSERT INTO #ShipmentPeriodCharge
SELECT
	MainOrg,
	Consignor,
	Consignee,
	Origin,
	OriginTableCode,
	Destination,
	DestinationTableCode,
	Product,
	Mode,
	TradeLaneType,
	PeriodStart,
	Currency,
	[ChargeCompany] = JH_GC,
	[JobRevenue] = CASE WHEN SUM(ISNULL([JobRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[JobCost] = CASE WHEN ABS(SUM(ISNULL([JobCost], 0))) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobCost], 0)) WHEN SUM(ISNULL([JobCost], 0)) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END,
	[MainOrgRevenue] = CASE WHEN SUM(ISNULL([MainOrgRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[MainOrgCost] = CASE WHEN ABS(SUM(ISNULL([MainOrgCost], 0))) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgCost], 0)) WHEN SUM(ISNULL([MainOrgCost], 0)) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END
FROM
	#ShipmentCharge Charge
	JOIN #Shipment Shipment on Charge.JH_PK = Shipment.JH_PK
	JOIN #ShipmentDetail Detail on Shipment.JS_PK = Detail.JS_PK
GROUP BY
	MainOrg,
	Consignor,
	Consignee,
	Origin,
	OriginTableCode,
	Destination,
	DestinationTableCode,
	Product,
	Mode,
	TradeLaneType,
	PeriodStart,
	currency,
	JH_GC

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = tl.MainOrg,
	[Supplier] = tl.Consignor,
	[Buyer] = tl.Consignee,
	[Origin] = tl.Origin,
	[OriginTableCode] = tl.OriginTableCode,
	[Destination] = tl.Destination,
	[DestinationTableCode] = tl.DestinationTableCode,
	[Warehouse] = NULL,
	[Status] = 3,
	[Product] = tl.Product,
	[Service] = '',
	[Mode] = tl.Mode,
	[TradeLaneType] = tl.TradeLaneType,
	[PeriodStart] = tl.PeriodStart,
	[PeriodLastTrade] = tl.PeriodLastTrade,
	[JobCount] = tl.JobCount,
	[PalletCount] = 0,
	[LineCount] = 0,
	[WeightVolume] = tl.WeightVolume,
	[WeightAmount] = tl.WeightAmount,
	[VolumeM3] = tl.VolumeM3,
	[ChargeableAmount] = tl.ChargeableAmount,
	[ChargeableUnits] = tl.ChargeableUnits,
	[TEU] = tl.TEU,
	[SupplierPart] = NULL,
	[JobCompany] = NULL,
	[Currency] = ch.Currency,
	[JobRevenue] = ch.JobRevenue,
	[JobCost] = ch.JobCost,
	[MainOrgRevenue] = ch.MainOrgRevenue,
	[MainOrgCost] = ch.MainOrgCost,
	[ChargeCompany] = ch.ChargeCompany,
	[RelatedJobID] = NULL
FROM
	#ShipmentPeriod tl
	LEFT JOIN #ShipmentPeriodCharge ch ON
		tl.MainOrg = ch.MainOrg
		AND tl.Product = ch.Product
		AND tl.Mode = ch.Mode
		AND tl.TradeLaneType = ch.TradeLaneType
		AND tl.PeriodStart = ch.PeriodStart
		AND (tl.Consignor = ch.Consignor OR (tl.Consignor IS NULL AND ch.Consignor IS NULL))
		AND (tl.Consignee = ch.Consignee OR (tl.Consignee IS NULL AND ch.Consignee IS NULL))
		AND (tl.Origin = ch.Origin OR (tl.Origin IS NULL AND ch.Origin IS NULL))
		AND (tl.OriginTableCode = ch.OriginTableCode OR (tl.OriginTableCode IS NULL AND ch.OriginTableCode IS NULL))
		AND (tl.Destination = ch.Destination OR (tl.Destination IS NULL AND ch.Destination IS NULL))
		AND (tl.DestinationTableCode = ch.DestinationTableCode OR (tl.DestinationTableCode IS NULL AND ch.DestinationTableCode IS NULL))

IF OBJECT_ID('tempdb.dbo.#Shipment') IS NOT NULL DROP TABLE #Shipment
IF OBJECT_ID('tempdb.dbo.#ShipmentOrg') IS NOT NULL DROP TABLE #ShipmentOrg
IF OBJECT_ID('tempdb.dbo.#ShipmentDetail') IS NOT NULL DROP TABLE #ShipmentDetail
IF OBJECT_ID('tempdb.dbo.#ShipmentPeriod') IS NOT NULL DROP TABLE #ShipmentPeriod
IF OBJECT_ID('tempdb.dbo.#ShipmentCharge') IS NOT NULL DROP TABLE #ShipmentCharge
IF OBJECT_ID('tempdb.dbo.#ShipmentPeriodCharge') IS NOT NULL DROP TABLE #ShipmentPeriodCharge
";
		}

		#endregion

		#region Quotations

		static string CreateTradeLinesSummaryCacheQuery_Quotations(OrgParameterType orgParamType)
		{
			return
@"
--------------
-- Quotation
--------------
CREATE TABLE #RateLine
(
	TI_PK uniqueidentifier,
	TH_RateType char(3),
	Org1 uniqueidentifier,
	Org2 uniqueidentifier,
	Org3 uniqueidentifier,
	TH_OH uniqueidentifier,
	OA_OH uniqueidentifier
);

" + GenerateUnionedSelectForOrgFilter("INSERT INTO #RateLine", @"
SELECT
	TI_PK,
	TH_RateType,
	COALESCE(TH_OH, OA_OH) AS [Org1],
	TI_OH_Consignor AS [Org2],
	TI_OH_Consignee AS [Org3],
	TH_OH,
	OA_OH
FROM 
	dbo.RatingHeader
	JOIN dbo.RateEntry ON TH_PK = TI_TH
	LEFT OUTER JOIN dbo.JobDocAddress ON E2_ParentID = TH_PK AND E2_ParentTableCode = 'TH'	AND E2_AddressType = 'LCA' 
	LEFT OUTER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address

WHERE
	TH_RateType IN ('QTE', 'SAL')
	AND TI_RateCategory IN ('AIR', 'FCL', 'LCL', 'SCO', 'SNC')
	AND TH_IsCancelled = 0
	AND TI_SystemLastEditTimeUtc >= @From
	AND TI_SystemLastEditTimeUtc < @To
	AND (TI_RateEndDate IS NULL OR TI_RateEndDate >= @From)
	", orgParamType, RatingHeaderSchema.Constants.TH_OH, OrgAddressSchema.Constants.OA_OH, "Org2", "Org3") + @"

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = Org1,
	[Supplier] = Org2,
	[Buyer] = Org3,
	[Origin] = OriginLocation.VLO_PK,
	[OriginTableCode] = OriginLocation.VLO_TableCode,
	[Destination] = DestinationLocation.VLO_PK,
	[DestinationTableCode] = DestinationLocation.VLO_TableCode,
	[Warehouse] = NULL,
	[Status] = CASE TH_RateType WHEN 'QTE' THEN 1 ELSE 2 END,
	[Product] =
		CASE 
			WHEN TI_RateCategory IN ('AIR', 'FCL', 'LCL') THEN 'SHP'
			WHEN TI_RateCategory IN ('SCO', 'SNC') THEN 'LGY'
			ELSE ''
		END,
	[Service] = '',
	[Mode] = 
		CASE 
			TI_RateCategory 
			WHEN 'AIR' THEN 'AIR' 
			WHEN 'FCL' THEN 'SEA'
			WHEN 'LCL' THEN
				CASE TI_Mode
					WHEN 'LCL' THEN 'SEA'
					WHEN 'LRO' THEN 'ROA'
					WHEN 'FTL' THEN 'ROA'
					WHEN 'LRA' THEN 'RAI'
					WHEN 'FWL' THEN 'RAI'
					WHEN 'BLK' THEN 'SEA'
					WHEN 'BBK' THEN 'SEA'
					WHEN 'ROR' THEN 'SEA'
					WHEN 'BCN' THEN 'SEA'
					WHEN 'SCN' THEN 'SEA'
					WHEN 'UNA' THEN 'COU'
					WHEN 'OBC' THEN 'COU'
					WHEN 'COU' THEN 'COU'
				END
			WHEN 'SCO' THEN 'BOL'
			WHEN 'SNC' THEN 'BOL'
			ELSE ''
		END,
	[TradeLaneType] = 
		CASE
			TI_RateCategory
			WHEN 'AIR' THEN
				CASE TI_Mode
					WHEN 'BCN' THEN 'ULD'
					WHEN 'SCN' THEN 'ULD'
					ELSE TI_Mode
				END
			WHEN 'FCL' THEN 'FCL'
			WHEN 'LCL' THEN 
				CASE TI_Mode
					WHEN 'LRO' THEN 'LTL'
					WHEN 'FTL' THEN 'FTL'
					WHEN 'UNA' THEN 'UNA'
					WHEN 'OBC' THEN 'OBC'
					ELSE 'LCL'
				END
			WHEN 'SCO' THEN 'FCL'
			WHEN 'SNC' THEN 'BBK'
			ELSE 'BBK'
		END,
	[PeriodStart] = NULL,
	[PeriodLastTrade] = NULL,
	[JobCount] = 0,
	[PalletCount] = 0,
	[LineCount] = 0,
	[WeightVolume] = 0,
	[WeightAmount] = 0,
	[VolumeM3] = 0,
	[ChargeableAmount] = 0,
	[ChargeableUnits] = '',
	[TEU] = 0,
	[SupplierPart] = NULL,
	[JobCompany] = NULL,
	[Currency] = TI_RX_NKCurrency,
	[JobOrgRevenue] = 0,
	[JobOrgCost] = 0,
	[MainOrgRevenue] = 0,
	[MainOrgCost] = 0,
	[ChargeCompany] = NULL,
	[RelatedJobID] = RateEntry.TI_PK

FROM 
	#RateLine RateLine
	JOIN dbo.RateEntry ON RateLine.TI_PK = RateEntry.TI_PK
	LEFT JOIN dbo.ViewLocation OriginLocation ON TI_OriginLRC = OriginLocation.VLO_Code AND OriginLocation.VLO_TableCode IN ('RL', 'RN', 'FZ')
	LEFT JOIN dbo.ViewLocation DestinationLocation ON TI_DestinationLRC = DestinationLocation.VLO_Code AND DestinationLocation.VLO_TableCode IN ('RL', 'RN', 'FZ')
GROUP BY
	Org1,
	OriginLocation.VLO_PK,
	OriginLocation.VLO_TableCode,
	DestinationLocation.VLO_PK,
	DestinationLocation.VLO_TableCode,
	TI_RateCategory,
	TI_Mode,
	TH_RateType,
	TI_RX_NKCurrency,
	Org2,
	Org3,
	RateEntry.TI_PK

IF OBJECT_ID('tempdb.dbo.#RateLine') IS NOT NULL DROP TABLE #RateLine

--------------
-- Quotation (Origin & Destination)
--------------
CREATE TABLE #RateLineOrgAndDst
(
	TI_PK uniqueidentifier,
	TH_RateType char(3),
	Org1 uniqueidentifier,
	Org2 uniqueidentifier,
	Org3 uniqueidentifier,
	TH_OH uniqueidentifier,
	OA_OH uniqueidentifier,
	IsCustomsBrokerage bit
);

" + GenerateUnionedSelectForOrgFilter("INSERT INTO #RateLineOrgAndDst", @"
SELECT
	TI_PK,
	TH_RateType,
	COALESCE(TH_OH, OA_OH) AS [Org1],
	TI_OH_Consignor AS [Org2],
	TI_OH_Consignee AS [Org3],
	TH_OH,
	OA_OH,
	CASE WHEN AC_ChargeGroup IN ('BRK', 'BON', 'OBR', 'OBO') THEN 1 ELSE 0 END [IsCustomsBrokerage]
FROM 
	dbo.RatingHeader
	JOIN dbo.RateEntry ON TH_PK = TI_TH
	LEFT JOIN dbo.RateLines ON TL_TI = TI_PK
	LEFT JOIN dbo.AccChargeCode ON TL_AC = AC_PK AND AC_GC = TH_GC
	LEFT OUTER JOIN dbo.JobDocAddress ON E2_ParentID = TH_PK AND E2_ParentTableCode = 'TH'	AND E2_AddressType = 'LCA' 
	LEFT OUTER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address

WHERE
	TH_RateType IN ('QTE', 'SAL')
	AND TI_RateCategory IN ('ORG', 'DST')
	AND TH_IsCancelled = 0
	AND TI_SystemLastEditTimeUtc >= @From
	AND TI_SystemLastEditTimeUtc < @To
	AND (TI_RateEndDate IS NULL OR TI_RateEndDate >= @From)
	", orgParamType, RatingHeaderSchema.Constants.TH_OH, OrgAddressSchema.Constants.OA_OH, "Org2", "Org3") + @"

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = Org1,
	[Supplier] = Org2,
	[Buyer] = Org3,
	[Origin] = CASE WHEN IsCustomsBrokerage = 1 AND TI_RateCategory = 'DST' THEN DestinationLocation.VLO_PK ELSE OriginLocation.VLO_PK END,
	[OriginTableCode] = CASE WHEN IsCustomsBrokerage = 1 AND TI_RateCategory = 'DST' THEN DestinationLocation.VLO_TableCode ELSE OriginLocation.VLO_TableCode END,
	[Destination] = CASE WHEN IsCustomsBrokerage = 1 THEN NULL ELSE DestinationLocation.VLO_PK END,
	[DestinationTableCode] = CASE WHEN IsCustomsBrokerage = 1 THEN '' ELSE DestinationLocation.VLO_TableCode END,
	[Warehouse] = NULL,
	[Status] = CASE TH_RateType WHEN 'QTE' THEN 1 ELSE 2 END,
	[Product] = 
		CASE
			WHEN IsCustomsBrokerage = 1 THEN 'BRK'
			ELSE 'SHP'
		END,
	[Service] = '',
	[Mode] = 
		CASE
			WHEN TI_Mode IN ('AIR', 'ULD', 'LSE') THEN 'AIR'
			WHEN TI_Mode IN ('SEA', 'LCL', 'FCL', 'BLK', 'BBK', 'ROR', 'BCN', 'SCN') THEN 'SEA'
			WHEN TI_Mode IN ('ROA', 'LRO', 'FRO', 'FTL') THEN 'ROA'
			WHEN TI_Mode IN ('RAI', 'LRA', 'FRA', 'FWL') THEN 'RAI'
			WHEN TI_Mode IN ('MAI', 'UNA', 'OBC', 'COU') THEN
				CASE WHEN IsCustomsBrokerage = 1 THEN 'MAI' ELSE 'COU' END
			ELSE ''
		END,
	[TradeLaneType] = 
		CASE WHEN IsCustomsBrokerage = 1 THEN
			CASE WHEN TI_RateCategory = 'ORG' THEN 'EXP' ELSE 'IMP' END
		ELSE
			CASE TI_Mode
				WHEN 'ULD' THEN 'ULD'
				WHEN 'LSE' THEN 'LSE'
				WHEN 'LCL' THEN 'LCL'
				WHEN 'FCL' THEN 'FCL'
				WHEN 'LRO' THEN 'LTL'
				WHEN 'FRO' THEN 'FTL'
				WHEN 'FTL' THEN 'FTL'
				WHEN 'LRA' THEN 'LCL'
				WHEN 'FRA' THEN 'FCL'
				WHEN 'FWL' THEN 'FCL'
				WHEN 'MAI' THEN ''
			ELSE ''
			END
		END,
	[PeriodStart] = NULL,
	[PeriodLastTrade] = NULL,
	[JobCount] = 0,
	[PalletCount] = 0,
	[LineCount] = 0,
	[WeightVolume] = 0,
	[WeightAmount] = 0,
	[VolumeM3] = 0,
	[ChargeableAmount] = 0,
	[ChargeableUnits] = '',
	[TEU] = 0,
	[SupplierPart] = NULL,
	[JobCompany] = NULL,
	[Currency] = TI_RX_NKCurrency,
	[JobRevenue] = 0,
	[JobCost] = 0,
	[MainOrgRevenue] = 0,
	[MainOrgCost] = 0,
	[ChargeCompany] = NULL,
	[RelatedJobID] = RateEntry.TI_PK

FROM 
	#RateLineOrgAndDst RateLine
	JOIN dbo.RateEntry ON RateLine.TI_PK = RateEntry.TI_PK
	LEFT JOIN dbo.ViewLocation OriginLocation ON TI_OriginLRC = OriginLocation.VLO_Code AND OriginLocation.VLO_TableCode IN ('RL', 'RN', 'FZ')
	LEFT JOIN dbo.ViewLocation DestinationLocation ON TI_DestinationLRC = DestinationLocation.VLO_Code AND DestinationLocation.VLO_TableCode IN ('RL', 'RN', 'FZ')
GROUP BY
	Org1,
	OriginLocation.VLO_PK,
	OriginLocation.VLO_TableCode,
	DestinationLocation.VLO_PK,
	DestinationLocation.VLO_TableCode,
	TI_RateCategory,
	TI_Mode,
	TH_RateType,
	TI_RX_NKCurrency,
	Org2,
	Org3,
	RateEntry.TI_PK,
	IsCustomsBrokerage

IF OBJECT_ID('tempdb.dbo.#RateLineOrgAndDst') IS NOT NULL DROP TABLE #RateLineOrgAndDst

--------------
-- Quotation (Warehouse)
--------------
CREATE TABLE #RateLineWarehouse
(
	TI_PK uniqueidentifier,
	TL_PK uniqueidentifier,
	TH_RateType char(3),
	Org1 uniqueidentifier,
	Org2 uniqueidentifier,
	Org3 uniqueidentifier,
	TH_OH uniqueidentifier,
	OA_OH uniqueidentifier,
	AC_ChargeGroup varchar(3)
);

" + GenerateUnionedSelectForOrgFilter("INSERT INTO #RateLineWarehouse", @"
SELECT
	TI_PK,
	TL_PK,
	TH_RateType,
	COALESCE(TH_OH, OA_OH) AS [Org1],
	TI_OH_Consignor AS [Org2],
	TI_OH_Consignee AS [Org3],
	TH_OH,
	OA_OH,
	AC_ChargeGroup
FROM 
	dbo.RateEntry
	JOIN dbo.RatingHeader ON TI_TH = TH_PK
	JOIN dbo.RateLines ON TL_TI = TI_PK
	JOIN dbo.AccChargeCode ON TL_AC = AC_PK AND AC_GC = TH_GC
	LEFT OUTER JOIN dbo.JobDocAddress ON E2_ParentID = TH_PK AND E2_ParentTableCode = 'TH'	AND E2_AddressType = 'LCA' 
	LEFT OUTER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address

WHERE
	TH_RateType IN ('QTE', 'SAL')
	AND TH_IsCancelled = 0
	AND TI_RateCategory = 'WHS'
	AND AC_ChargeGroup IN ('WOU', 'WIN', 'WST')
	AND TI_SystemLastEditTimeUtc >= @From
	AND TI_SystemLastEditTimeUtc < @To
	AND (TI_RateEndDate IS NULL OR TI_RateEndDate >= @From)
", orgParamType, RatingHeaderSchema.Constants.TH_OH, OrgAddressSchema.Constants.OA_OH, "Org2", "Org3") + @"

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = Org1,
	[Supplier] = Org2,
	[Buyer] = Org3,
	[Origin] = RL_PK,
	[OriginTableCode] = CASE WHEN RL_PK IS NOT NULL THEN 'RL' ELSE '' END,
	[Destination] = NULL,
	[DestinationTableCode] = '',
	[Warehouse] = TI_ParentID,
	[Status] = CASE TH_RateType WHEN 'QTE' THEN 1 ELSE 2 END,
	[Product] = 'WHS',
	[Service] = 
		CASE AC_ChargeGroup
			WHEN 'WOU' THEN 'ORD'
			WHEN 'WIN' THEN 'REC'
			WHEN 'WST' THEN 'STG'
			ELSE ''
		END,
	[Mode] = '',
	[TradeLaneType] = '',
	[PeriodStart] = NULL,
	[PeriodLastTrade] = NULL,
	[JobCount] = 0,
	[PalletCount] = 0,
	[LineCount] = 0,
	[WeightVolume] = 0,
	[WeightAmount] = 0,
	[VolumeM3] = 0,
	[ChargeableAmount] = 0,
	[ChargeableUnits] = '',
	[TEU] = 0,

	[SupplierPart] = TL_ParentID,
	[JobCompany] = NULL,
	[Currency] = TL_RX_NKCurrency,
	[JobRevenue] = 0,
	[JobCost] = 0,
	[MainOrgRevenue] = 0,
	[MainOrgCost] = 0,
	[ChargeCompany] = NULL,
	[RelatedJobID] = #RateLineWarehouse.TI_PK
FROM 
	#RateLineWarehouse
	JOIN dbo.RateEntry ON #RateLineWarehouse.TI_PK = RateEntry.TI_PK
	JOIN dbo.RateLines ON #RateLineWarehouse.TL_PK = RateLines.TL_PK
	JOIN dbo.WhsWarehouse ON TI_ParentID = WW_PK
	LEFT JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	LEFT JOIN dbo.RefUnloco ON GB_RL_NKHomePort = RL_Code

GROUP BY
	Org1,
	TI_ParentID,
	RL_PK,
	TH_RateType,
	TL_RX_NKCurrency,
	Org2,
	Org3,
	#RateLineWarehouse.TI_PK,
	TL_ParentID,
	AC_ChargeGroup

IF OBJECT_ID('tempdb.dbo.#RateLineWarehouse') IS NOT NULL DROP TABLE #RateLineWarehouse
";
		}

		#endregion

		#region Port Transport

		static string CreateTradeLinesSummaryCacheQuery_PortTransport(OrgParameterType orgParamType)
		{
			return
@"
------------------
-- Port Transport
------------------

CREATE TABLE #PortTransport
(
	JJ_PK uniqueidentifier,
	JH_PK uniqueidentifier,
	JH_GC uniqueidentifier,
	LocalClient_OA_OH uniqueidentifier,
	OverseasClient_OA_OH uniqueidentifier,
	From_E2_PK uniqueidentifier,
	FromOrg_OA_PK uniqueidentifier,
	FromOrg_OA_OH uniqueidentifier,
	To_E2_PK uniqueidentifier,
	ToOrg_OA_PK uniqueidentifier,
	ToOrg_OA_OH uniqueidentifier
);

INSERT INTO #PortTransport
SELECT
	JJ_PK,
	JH_PK,
	JH_GC,
	LocalClientAddress.OA_OH,
	OverseasClientAddress.OA_OH,
	FromAddress.E2_PK,
	FromOrgAddress.OA_PK,
	FromOrgAddress.OA_OH,
	ToAddress.E2_PK,
	ToOrgAddress.OA_PK,
	ToOrgAddress.OA_OH
FROM
	dbo.JobCartage
	LEFT JOIN dbo.JobHeader ON JJ_PK = JH_ParentID AND JH_ParentTableCode = 'JJ'
	LEFT JOIN dbo.OrgAddress LocalClientAddress ON JH_OA_LocalChargesAddr = LocalClientAddress.OA_PK
	LEFT JOIN dbo.OrgAddress OverseasClientAddress ON JH_OA_AgentCollectAddr = OverseasClientAddress.OA_PK
	JOIN dbo.LocalCartageJobType ON JJ_E3_NKJobType = E3_JobType
	CROSS APPLY (SELECT TOP 1 E4_E5_FromOrg, E4_E5_WaitPointOrg FROM dbo.LocalCartageJobLegType WHERE E4_E3 = E3_PK ORDER BY E4_DisplayOrder) LocalCartageJobTypeBooking
	JOIN dbo.LocalCartageJobOrg LocalCartageJobTypeFromOrg ON LocalCartageJobTypeBooking.E4_E5_FromOrg = LocalCartageJobTypeFromOrg.E5_PK
	JOIN dbo.LocalCartageJobOrg LocalCartageJobTypeToOrg ON LocalCartageJobTypeBooking.E4_E5_WaitPointOrg = LocalCartageJobTypeToOrg.E5_PK
	LEFT JOIN dbo.JobDocAddress FromAddress ON FromAddress.E2_ParentID = JobCartage.JJ_PK AND FromAddress.E2_ParentTableCode = 'JJ'
		AND FromAddress.E2_AddressSequence = 0
		AND FromAddress.E2_AddressType = 
		CASE LocalCartageJobTypeFromOrg.E5_OrgType
			WHEN 'CNR' THEN 'LCE'
			WHEN 'CNE' THEN 'LCI'
			WHEN 'CFS' THEN 'LCF'
			WHEN 'CTO' THEN 'LCT'
			WHEN 'CYD' THEN 'LCY'
			WHEN 'SRV' THEN 'LCS'
			WHEN 'WHS' THEN 'LCW'
			ELSE 'LCM'
		END	
	LEFT JOIN dbo.JobDocAddress ToAddress ON ToAddress.E2_ParentID = JobCartage.JJ_PK AND ToAddress.E2_ParentTableCode = 'JJ'
		AND ToAddress.E2_AddressSequence = 0
		AND ToAddress.E2_AddressType = 
		CASE LocalCartageJobTypeToOrg.E5_OrgType
			WHEN 'CNR' THEN 'LCE'
			WHEN 'CNE' THEN 'LCI'
			WHEN 'CFS' THEN 'LCF'
			WHEN 'CTO' THEN 'LCT'
			WHEN 'CYD' THEN 'LCY'
			WHEN 'SRV' THEN 'LCS'
			WHEN 'WHS' THEN 'LCW'
			ELSE 'LCM'
		END		
	LEFT JOIN dbo.OrgAddress FromOrgAddress ON FromAddress.E2_OA_Address = FromOrgAddress.OA_PK AND FromAddress.E2_AddressOverride = 0
	LEFT JOIN dbo.OrgAddress ToOrgAddress ON ToAddress.E2_OA_Address = ToOrgAddress.OA_PK AND ToAddress.E2_AddressOverride = 0
WHERE
	COALESCE(JJ_EstimatedPickup, JJ_SystemCreateTimeUtc) >= @From
	AND COALESCE(JJ_EstimatedPickup, JJ_SystemCreateTimeUtc) < @To
	AND JJ_IsCancelled = 0
" + GetOrgSqlWhereClause(orgParamType, "LocalClientAddress.OA_OH", "OverseasClientAddress.OA_OH", "FromOrgAddress.OA_OH", "ToOrgAddress.OA_OH") + @"

CREATE TABLE #PortTransportDetail
(
	JJ_PK uniqueidentifier,
	FromOrg uniqueidentifier,
	ToOrg uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	TradeType varchar(3),
	PeriodStart date,
	TradeTime smalldatetime,
	JobCount int,
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3)
);

INSERT INTO #PortTransportDetail
SELECT
	[JJ_PK] = JobCartage.JJ_PK,
	[FromOrg] = PortTransport.FromOrg_OA_OH,
	[ToOrg] = PortTransport.ToOrg_OA_OH,
	[Origin] = COALESCE(FromCity.R9_PK, FromUNLOCO.RL_PK, FromState.RW_PK, FromCountry.RN_PK),
	[OriginTableCode] = COALESCE(FromCity.TableCode, FromUNLOCO.TableCode, FromState.TableCode, FromCountry.TableCode),
	[Destination] = COALESCE(ToCity.R9_PK, ToUNLOCO.RL_PK, ToState.RW_PK, ToCountry.RN_PK),
	[DestinationTableCode] = COALESCE(ToCity.TableCode, ToUNLOCO.TableCode, ToState.TableCode, ToCountry.TableCode),
	[TradeLaneType] = JJ_ContainerMode,
	[PeriodStart] = DATEADD(m, DATEDIFF(m, 0, COALESCE(JJ_EstimatedPickup, JJ_SystemCreateTimeUtc)), 0),
	[PeriodLastTrade] = COALESCE(JJ_EstimatedPickup, JJ_SystemCreateTimeUtc),
	[JobCount] = 1,
	[WeightAmount] = ISNULL(ConvertedActualWeight.Value, 0),
	[VolumeM3] = ISNULL(ConvertedActualVolume.Value, 0)
FROM
	(
		SELECT DISTINCT JJ_PK, From_E2_PK, FromOrg_OA_PK, FromOrg_OA_OH, To_E2_PK, ToOrg_OA_PK, ToOrg_OA_OH
		FROM #PortTransport 
	) PortTransport 
	JOIN dbo.JobCartage ON JobCartage.JJ_PK = PortTransport.JJ_PK
	LEFT JOIN dbo.JobDocAddress FromAddress ON FromAddress.E2_PK = PortTransport.From_E2_PK
	LEFT JOIN dbo.JobDocAddress ToAddress ON ToAddress.E2_PK = PortTransport.To_E2_PK
	LEFT JOIN dbo.OrgAddress FromOrgAddress ON FromOrgAddress.OA_PK = PortTransport.FromOrg_OA_PK
	LEFT JOIN dbo.OrgAddress ToOrgAddress ON ToOrgAddress.OA_PK = PortTransport.ToOrg_OA_PK

	OUTER APPLY 
		(
			SELECT TOP 1 R9_PK, 'R9' AS TableCode 
			FROM dbo.RefCityTown 
			WHERE 
				R9_InternationalName = COALESCE(FromOrgAddress.OA_City, FromAddress.E2_City)
				AND 
				(COALESCE(FromOrgAddress.OA_State, FromAddress.E2_State, '') = '' OR R9_RW_NKState = COALESCE(FromOrgAddress.OA_State, FromAddress.E2_State))
		) FromCity  
	OUTER APPLY 
		(
			SELECT TOP 1 RL_PK, 'RL' AS TableCode FROM dbo.RefUNLOCO WHERE RL_Code = COALESCE(FromOrgAddress.OA_RL_NKRelatedPortCode, '')
		) FromUNLOCO
	OUTER APPLY 
		(
			SELECT TOP 1 RW_PK, 'RW' AS TableCode 
			FROM dbo.RefCountryStates JOIN dbo.RefCountry ON RW_RN_NKCountryCode = RN_Code
			WHERE 
				RW_Code = COALESCE(FromOrgAddress.OA_State, FromAddress.E2_State) OR RW_Description = COALESCE(FromOrgAddress.OA_State, FromAddress.E2_State)
				AND
				((COALESCE(LEFT(FromOrgAddress.OA_RL_NKRelatedPortCode, 2), FromAddress.E2_RN_NKCountryCode, '') = '') OR RN_Code = COALESCE(LEFT(FromOrgAddress.OA_RL_NKRelatedPortCode, 2), FromAddress.E2_RN_NKCountryCode))
		) FromState
	OUTER APPLY 
		(
			SELECT TOP 1 RN_PK, 'RN' AS TableCode FROM dbo.RefCountry WHERE RN_Code = COALESCE(LEFT(FromOrgAddress.OA_RL_NKRelatedPortCode, 2), FromAddress.E2_RN_NKCountryCode)
		) FromCountry

	OUTER APPLY 
		(
			SELECT TOP 1 R9_PK, 'R9' AS TableCode 
			FROM dbo.RefCityTown 
			WHERE 
				R9_InternationalName = COALESCE(ToOrgAddress.OA_City, ToAddress.E2_City)
				AND 
				(COALESCE(ToOrgAddress.OA_State, ToAddress.E2_State, '') = '' OR R9_RW_NKState = COALESCE(ToOrgAddress.OA_State, ToAddress.E2_State))
		) ToCity  
	OUTER APPLY 
		(
			SELECT TOP 1 RL_PK, 'RL' AS TableCode FROM dbo.RefUNLOCO WHERE RL_Code = COALESCE(ToOrgAddress.OA_RL_NKRelatedPortCode, '')
		) ToUNLOCO
	OUTER APPLY 
		(
			SELECT TOP 1 RW_PK, 'RW' AS TableCode 
			FROM dbo.RefCountryStates JOIN dbo.RefCountry ON RW_RN_NKCountryCode = RN_Code
			WHERE 
				RW_Code = COALESCE(ToOrgAddress.OA_State, ToAddress.E2_State) OR RW_Description = COALESCE(ToOrgAddress.OA_State, ToAddress.E2_State)
				AND
				((COALESCE(LEFT(ToOrgAddress.OA_RL_NKRelatedPortCode, 2), ToAddress.E2_RN_NKCountryCode, '') = '') OR RN_Code = COALESCE(LEFT(ToOrgAddress.OA_RL_NKRelatedPortCode, 2), ToAddress.E2_RN_NKCountryCode))
		) ToState
	OUTER APPLY 
		(
			SELECT TOP 1 RN_PK, 'RN' AS TableCode 
			FROM dbo.RefCountry 
			WHERE RN_Code = COALESCE(LEFT(ToOrgAddress.OA_RL_NKRelatedPortCode, 2), ToAddress.E2_RN_NKCountryCode)
		) ToCountry

	CROSS APPLY dbo.ConvertWeight(JJ_Weight, JJ_WeightUQ, 'T') AS ConvertedActualWeight
	CROSS APPLY dbo.ConvertVolume(JJ_Volume, JJ_VolumeUQ, 'M3') AS ConvertedActualVolume

CREATE TABLE #PortTransportPeriod
(
	MainOrg uniqueidentifier,
	FromOrg uniqueidentifier,
	ToOrg uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	TradeType varchar(3),
	PeriodStart date,
	PeriodLastTrade smalldatetime,
	JobCount int,
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3)
);

CREATE TABLE #PortTransportOrg
(
	MainOrg uniqueidentifier,
	JJ_PK uniqueidentifier,
	JH_PK uniqueidentifier
)

INSERT INTO #PortTransportOrg
SELECT [MainOrg] = FromOrg_OA_OH, JJ_PK, JH_PK FROM #PortTransport WHERE FromOrg_OA_OH IS NOT NULL
UNION ALL
SELECT [MainOrg] = ToOrg_OA_OH, JJ_PK, JH_PK FROM #PortTransport WHERE ToOrg_OA_OH IS NOT NULL 
	AND (ToOrg_OA_OH != FromOrg_OA_OH OR FromOrg_OA_OH IS NULL)
UNION ALL
SELECT [MainOrg] = LocalClient_OA_OH, JJ_PK, JH_PK FROM #PortTransport WHERE LocalClient_OA_OH IS NOT NULL 
	AND (LocalClient_OA_OH != FromOrg_OA_OH OR FromOrg_OA_OH IS NULL)
	AND (LocalClient_OA_OH != ToOrg_OA_OH OR ToOrg_OA_OH IS NULL)
UNION ALL
SELECT [MainOrg] = OverseasClient_OA_OH, JJ_PK, JH_PK FROM #PortTransport WHERE OverseasClient_OA_OH IS NOT NULL 
	AND (OverseasClient_OA_OH != FromOrg_OA_OH OR FromOrg_OA_OH IS NULL)
	AND (OverseasClient_OA_OH != ToOrg_OA_OH OR ToOrg_OA_OH IS NULL)
	AND (OverseasClient_OA_OH != LocalClient_OA_OH OR LocalClient_OA_OH IS NULL)

INSERT INTO #PortTransportPeriod
SELECT 
	[MainOrg] = MainOrg,
	[FromOrg] = FromOrg,
	[ToOrg] = ToOrg,
	[Origin] = Origin,
	[OriginTableCode] = OriginTableCode,
	[Destination] = Destination,
	[DestinationTableCode] = DestinationTableCode,
	[TradeType] = TradeType,
	[PeriodStart] = PeriodStart,
	[PeriodLastTrade] = MAX(PeriodLastTrade),
	[JobCount] = CASE WHEN SUM(JobCount) <= @CountMaxValue THEN SUM(JobCount) ELSE @CountMaxValue END,
	[WeightAmount] = CASE WHEN SUM(WeightAmount) <= @WeightVolumeMaxValue THEN SUM(WeightAmount) ELSE @WeightVolumeMaxValue END,
	[VolumeM3] = CASE WHEN SUM(VolumeM3) <= @WeightVolumeMaxValue THEN SUM(VolumeM3) ELSE @WeightVolumeMaxValue END
FROM
	(
		SELECT 
			MainOrg,
			Detail.JJ_PK,
			FromOrg,
			ToOrg,
			Origin,
			OriginTableCode,
			Destination,
			DestinationTableCode,
			TradeType,
			PeriodStart,
			PeriodLastTrade = MAX(TradeTime),
			JobCount = MAX(JobCount),
			WeightAmount = MAX(WeightAmount),
			VolumeM3 = MAX(VolumeM3)
		FROM
			#PortTransportDetail Detail
			JOIN #PortTransportOrg Org ON Detail.JJ_PK = Org.JJ_PK
		GROUP BY
			MainOrg,
			Detail.JJ_PK,
			FromOrg,
			ToOrg,
			Origin,
			OriginTableCode,
			Destination,
			DestinationTableCode,
			TradeType,
			PeriodStart
	) a
GROUP BY
	MainOrg,
	FromOrg,
	ToOrg,
	Origin,
	OriginTableCode,
	Destination,
	DestinationTableCode,
	TradeType,
	PeriodStart

CREATE TABLE #PortTransportCharge
(
	MainOrg uniqueidentifier,
	JH_PK uniqueidentifier,
	Currency varchar(3),
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4),
)

INSERT INTO #PortTransportCharge
SELECT
	[MainOrg] = MainOrg,
	[JH_PK]	= JCA_JH,
	[Currency] = GC_RX_NKLocalCurrency,
	[JobRevenue]	 = CASE WHEN SUM(JCA_Revenue) <= @RevenueCostMaxValue THEN SUM(JCA_Revenue) ELSE @RevenueCostMaxValue END,
	[JobCost]		 = CASE WHEN ABS(SUM(JCA_Cost)) <= @RevenueCostMaxValue THEN SUM(JCA_Cost) WHEN SUM(JCA_Cost) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END,
	[MainOrgRevenue] = CASE WHEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Revenue ELSE 0 END) <= @RevenueCostMaxValue THEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Revenue ELSE 0 END) ELSE @RevenueCostMaxValue END,
	[MainOrgCost]	 = CASE WHEN ABS(SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END)) <= @RevenueCostMaxValue THEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END) WHEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END
FROM
	dbo.RptDtJobCostingDataAmountByJob
	JOIN #PortTransportOrg PortTransportOrg ON PortTransportOrg.JH_PK = JCA_JH
	JOIN dbo.GlbCompany ON JCA_GC = GC_PK
GROUP BY
	JCA_JH,
	MainOrg,
	GC_RX_NKLocalCurrency

CREATE TABLE #PortTransportPeriodCharge
(
	MainOrg uniqueidentifier,
	FromOrg uniqueidentifier,
	ToOrg uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	TradeType varchar(3),
	PeriodStart date,
	Currency varchar(3),
	ChargeCompany uniqueidentifier,
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4),
)

INSERT INTO #PortTransportPeriodCharge
SELECT 
	[MainOrg] = MainOrg,
	[FromOrg] = FromOrg,
	[ToOrg] = ToOrg,
	[Origin] = Origin,
	[OriginTableCode] = OriginTableCode,
	[Destination] = Destination,
	[DestinationTableCode] = DestinationTableCode,
	[TradeType] = TradeType,
	[PeriodStart] = PeriodStart,
	[Currency] = ISNULL([Currency], ''),
	[ChargeCompany] = JH_GC,
	[JobRevenue] = CASE WHEN SUM(ISNULL([JobRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[JobCost] = CASE WHEN ABS(SUM(ISNULL([JobCost], 0))) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobCost], 0)) WHEN SUM(ISNULL([JobCost], 0)) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END,
	[MainOrgRevenue] = CASE WHEN SUM(ISNULL([MainOrgRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[MainOrgCost] = CASE WHEN ABS(SUM(ISNULL([MainOrgCost], 0))) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgCost], 0)) WHEN SUM(ISNULL([MainOrgCost], 0)) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END
FROM
	#PortTransportCharge Charge
	JOIN #PortTransport PortTransport ON Charge.JH_PK = PortTransport.JH_PK
	JOIN #PortTransportDetail Detail ON PortTransport.JJ_PK = Detail.JJ_PK
GROUP BY
	MainOrg,
	FromOrg,
	ToOrg,
	Origin,
	OriginTableCode,
	Destination,
	DestinationTableCode,
	TradeType,
	PeriodStart,
	[Currency],
	JH_GC

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = tl.MainOrg,
	[Supplier] = tl.FromOrg,
	[Buyer] = tl.ToOrg,
	[Origin] = tl.Origin,
	[OriginTableCode] = tl.OriginTableCode,
	[Destination] = tl.Destination,
	[DestinationTableCode] = tl.DestinationTableCode,
	[Warehouse] = NULL,
	[Status] = 3,
	[Product] = 'TRN',
	[Service] = '',
	[Mode] = 'PTR',
	[TradeLaneType] = tl.TradeType,
	[PeriodStart] = tl.PeriodStart,
	[PeriodLastTrade] = tl.PeriodLastTrade,
	[JobCount] = tl.JobCount,
	[PalletCount] = 0,
	[LineCount] = 0,
	[WeightVolume] = 0,
	[WeightAmount] = tl.WeightAmount,
	[VolumeM3] = tl.VolumeM3,
	[ChargeableAmount] = 0,
	[ChargeableUnits] = '',
	[TEU] = 0,
	[SupplierPart] = NULL,
	[JobCompany] = NULL,
	[Currency] = ch.Currency,
	[JobRevenue] = ch.JobRevenue,
	[JobCost] = ch.JobCost,
	[MainOrgRevenue] = ch.MainOrgRevenue,
	[MainOrgCost] = ch.MainOrgCost,
	[ChargeCompany] = ch.ChargeCompany,
	[RelatedJobID] = NULL
FROM
	#PortTransportPeriod tl
	LEFT JOIN #PortTransportPeriodCharge ch ON
		tl.MainOrg = ch.MainOrg
		AND (tl.PeriodStart = ch.PeriodStart)
		AND (tl.FromOrg = ch.FromOrg OR (tl.FromOrg IS NULL AND ch.FromOrg IS NULL))
		AND (tl.ToOrg = ch.ToOrg OR (tl.ToOrg IS NULL AND ch.ToOrg IS NULL))
		AND (tl.Origin = ch.Origin OR (tl.Origin IS NULL AND ch.Origin IS NULL))
		AND (tl.OriginTableCode = ch.OriginTableCode OR (tl.OriginTableCode IS NULL AND ch.OriginTableCode IS NULL))
		AND (tl.Destination = ch.Destination OR (tl.Destination IS NULL AND ch.Destination IS NULL))
		AND (tl.DestinationTableCode = ch.DestinationTableCode OR (tl.DestinationTableCode IS NULL AND ch.DestinationTableCode IS NULL))
		AND (tl.TradeType = ch.TradeType)

IF OBJECT_ID('tempdb.dbo.#PortTransport') IS NOT NULL DROP TABLE #PortTransport
IF OBJECT_ID('tempdb.dbo.#PortTransportOrg') IS NOT NULL DROP TABLE #PortTransportOrg
IF OBJECT_ID('tempdb.dbo.#PortTransportDetail') IS NOT NULL DROP TABLE #PortTransportDetail
IF OBJECT_ID('tempdb.dbo.#PortTransportPeriod') IS NOT NULL DROP TABLE #PortTransportPeriod
IF OBJECT_ID('tempdb.dbo.#PortTransportCharge') IS NOT NULL DROP TABLE #PortTransportCharge
IF OBJECT_ID('tempdb.dbo.#PortTransportPeriodCharge') IS NOT NULL DROP TABLE #PortTransportPeriodCharge
";
		}

		#endregion

		#region Customs Brokerage

		static string CreateTradeLinesSummaryCacheQuery_CustomsBrokerage(OrgParameterType orgParamType)
		{
			return
@"
---------------------
-- Customs Brokerage
---------------------

CREATE TABLE #Declaration
(
	JE_PK uniqueidentifier,
	JH_PK uniqueidentifier,
	JH_GC uniqueidentifier,
	OA_OH_LocalClient uniqueidentifier,
	OA_OH_OverseasClient uniqueidentifier,
	JE_OH_Supplier uniqueidentifier,
	JE_OH_Importer uniqueidentifier
);

" + GenerateUnionedSelectForOrgFilter("INSERT INTO #Declaration", @"
SELECT
	JE_PK,
	JH_PK,
	JH_GC,
	OA_OH_LocalClient = LocalClientAddress.OA_OH,
	OA_OH_OverseasClient = OverseasClientAddress.OA_OH,
	JE_OH_Supplier,
	JE_OH_Importer
FROM
	dbo.JobDeclaration
	LEFT JOIN dbo.JobHeader ON JE_PK = JH_ParentID AND JH_ParentTableCode = 'JE'
	LEFT JOIN dbo.OrgAddress LocalClientAddress ON JH_OA_LocalChargesAddr = LocalClientAddress.OA_PK
	LEFT JOIN dbo.OrgAddress OverseasClientAddress ON JH_OA_AgentCollectAddr = OverseasClientAddress.OA_PK
WHERE
	COALESCE(JE_ExportDate, JE_DateAtOrigin, JE_SystemCreateTimeUtc) >= @From
	AND COALESCE(JE_ExportDate, JE_DateAtOrigin, JE_SystemCreateTimeUtc) < @To
", orgParamType, "OA_OH_LocalClient", "OA_OH_OverseasClient", JobDeclarationSchema.Constants.JE_OH_Supplier, JobDeclarationSchema.Constants.JE_OH_Importer) + @"

CREATE TABLE #DeclarationDetail
(
	JE_PK uniqueidentifier,
	Supplier uniqueidentifier,
	Importer uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	Mode varchar(3),
	TradeLaneType varchar(3),
	JobCompany uniqueidentifier,
	PeriodStart date,
	TradeTime smalldatetime,
	JobCount int,
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3)
);

INSERT INTO #DeclarationDetail
SELECT
	[JE_PK] = Declaration.JE_PK,
	[Supplier] = Declaration.JE_OH_Supplier,
	[Importer] = Declaration.JE_OH_Importer,
	[Origin] = CASE JE_MessageType WHEN 'DRW' THEN BranchUNLOCO.RL_PK ELSE OriginUNLOCO.RL_PK END,
	[OriginTableCode] =
		CASE 
			WHEN (CASE JE_MessageType WHEN 'DRW' THEN BranchUNLOCO.RL_PK ELSE OriginUNLOCO.RL_PK END) IS NOT NULL THEN 'RL'
			ELSE ''
		END,
	[Destination] = DestinationUNLOCO.RL_PK,
	[DestinationTableCode] = CASE WHEN DestinationUNLOCO.RL_PK IS NOT NULL THEN 'RL' ELSE '' END,
	[Mode] = JE_TransportMode,
	[TradeLaneType] = JE_MessageType,
	[JobCompany] = GB_GC,
	[PeriodStart] = DATEADD(m, DATEDIFF(m, 0, COALESCE(JE_ExportDate, JE_DateAtOrigin, JE_SystemCreateTimeUtc)), 0),
	[TradeTime] = COALESCE(JE_ExportDate, JE_DateAtOrigin, JE_SystemCreateTimeUtc),
	[JobCount] = 1,
	[WeightAmount] = ISNULL(ConvertedActualWeight.Value, 0),
	[VolumeM3] = ISNULL(ConvertedActualVolume.Value, 0)
FROM
	(
		SELECT DISTINCT JE_PK, JE_OH_Supplier, JE_OH_Importer
		FROM #Declaration 
	) Declaration
	JOIN dbo.JobDeclaration ON Declaration.JE_PK = JobDeclaration.JE_PK
	LEFT JOIN dbo.RefUNLOCO OriginUNLOCO ON JE_RL_NKOrigin = OriginUNLOCO.RL_Code
	LEFT JOIN dbo.RefUNLOCO DestinationUNLOCO ON JE_RL_NKPortOfArrival = DestinationUNLOCO.RL_Code
	JOIN dbo.GlbBranch ON JE_GB = GB_PK
	LEFT JOIN dbo.RefUNLOCO BranchUNLOCO ON GB_RL_NKHomePort = BranchUNLOCO.RL_Code
	CROSS APPLY dbo.ConvertWeight(JE_TotalWeight, JE_TotalWeightUnit, 'T') AS ConvertedActualWeight
	CROSS APPLY dbo.ConvertVolume(JE_TotalVolume, JE_TotalVolumeUnit, 'M3') AS ConvertedActualVolume

CREATE TABLE #DeclarationOrg
(
	MainOrg uniqueidentifier,
	JE_PK uniqueidentifier,
	JH_PK uniqueidentifier
)

INSERT INTO #DeclarationOrg
SELECT [MainOrg] = JE_OH_Supplier, JE_PK, JH_PK FROM #Declaration WHERE JE_OH_Supplier IS NOT NULL
UNION ALL
SELECT [MainOrg] = JE_OH_Importer, JE_PK, JH_PK FROM #Declaration WHERE JE_OH_Importer IS NOT NULL 
	AND (JE_OH_Importer != JE_OH_Supplier OR JE_OH_Supplier IS NULL)
UNION ALL
SELECT [MainOrg] = OA_OH_LocalClient, JE_PK, JH_PK FROM #Declaration WHERE OA_OH_LocalClient IS NOT NULL 
	AND (OA_OH_LocalClient != JE_OH_Supplier OR JE_OH_Supplier IS NULL)
	AND (OA_OH_LocalClient != JE_OH_Importer OR JE_OH_Importer IS NULL)
UNION ALL
SELECT [MainOrg] = OA_OH_OverseasClient, JE_PK, JH_PK FROM #Declaration WHERE OA_OH_OverseasClient IS NOT NULL 
	AND (OA_OH_OverseasClient != JE_OH_Supplier OR JE_OH_Supplier IS NULL)
	AND (OA_OH_OverseasClient != JE_OH_Importer OR JE_OH_Importer IS NULL)
	AND (OA_OH_OverseasClient != OA_OH_LocalClient OR OA_OH_LocalClient IS NULL)

CREATE TABLE #DeclarationPeriod
(
	MainOrg uniqueidentifier,
	Supplier uniqueidentifier,
	Importer uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	Mode varchar(3),
	TradeLaneType varchar(3),
	JobCompany uniqueidentifier,
	PeriodStart date,
	PeriodLastTrade smalldatetime,
	JobCount int,
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3)
);

INSERT INTO #DeclarationPeriod
SELECT 
	[MainOrg] = MainOrg,
	[Supplier] = Supplier,
	[Importer] = Importer,
	[Origin] = Origin,
	[OriginTableCode] = OriginTableCode,
	[Destination] = Destination,
	[DestinationTableCode] = DestinationTableCode,
	[Mode] = Mode,
	[TradeLaneType] = TradeLaneType,
	[JobCompany] = JobCompany,
	[PeriodStart] = PeriodStart,
	[PeriodLastTrade] = MAX(PeriodLastTrade),
	[JobCount] = CASE WHEN SUM(JobCount) <= @CountMaxValue THEN SUM(JobCount) ELSE @CountMaxValue END,
	[WeightAmount] = CASE WHEN SUM(WeightAmount) <= @WeightVolumeMaxValue THEN SUM(WeightAmount) ELSE @WeightVolumeMaxValue END,
	[VolumeM3] = CASE WHEN SUM(VolumeM3) <= @WeightVolumeMaxValue THEN SUM(VolumeM3) ELSE @WeightVolumeMaxValue END
FROM
	(
		SELECT 
			MainOrg,
			Detail.JE_PK,
			Supplier,
			Importer,
			Origin,
			OriginTableCode,
			Destination,
			DestinationTableCode,
			Mode,
			TradeLaneType,
			JobCompany,
			PeriodStart,
			PeriodLastTrade = MAX(TradeTime),
			JobCount = MAX(JobCount),
			WeightAmount = MAX(WeightAmount),
			VolumeM3 = MAX(VolumeM3)
		FROM
			#DeclarationDetail Detail
			JOIN #DeclarationOrg Org ON Detail.JE_PK = Org.JE_PK
		GROUP BY
			MainOrg,
			Detail.JE_PK,
			Supplier,
			Importer,
			Origin,
			OriginTableCode,
			Destination,
			DestinationTableCode,
			Mode,
			TradeLaneType,
			JobCompany,
			PeriodStart
	) a
GROUP BY
	MainOrg,
	Supplier,
	Importer,
	Origin,
	OriginTableCode,
	Destination,
	DestinationTableCode,
	Mode,
	TradeLaneType,
	JobCompany,
	PeriodStart

CREATE TABLE #DeclarationCharge
(
	MainOrg uniqueidentifier,
	JH_PK uniqueidentifier,
	Currency varchar(3),
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4),
)

	INSERT INTO #DeclarationCharge
	SELECT
		[MainOrg] = MainOrg,
		[JH_PK]	= JCA_JH,
		[Currency] = GC_RX_NKLocalCurrency,
		[JobRevenue]	 = CASE WHEN SUM(JCA_Revenue) <= @RevenueCostMaxValue THEN SUM(JCA_Revenue) ELSE @RevenueCostMaxValue END,
		[JobCost]		 = CASE WHEN ABS(SUM(JCA_Cost)) <= @RevenueCostMaxValue THEN SUM(JCA_Cost) WHEN SUM(JCA_Cost) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END,
		[MainOrgRevenue] = CASE WHEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Revenue ELSE 0 END) <= @RevenueCostMaxValue THEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Revenue ELSE 0 END) ELSE @RevenueCostMaxValue END,
		[MainOrgCost]	 = CASE WHEN ABS(SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END)) <= @RevenueCostMaxValue THEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END) WHEN SUM(CASE WHEN JCA_OH_DebtorOrCreditor = MainOrg THEN JCA_Cost ELSE 0 END) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END
	FROM
		dbo.RptDtJobCostingDataAmountByJob
		JOIN #DeclarationOrg DeclarationOrg ON DeclarationOrg.JH_PK = JCA_JH
		JOIN dbo.GlbCompany ON JCA_GC = GC_PK
	GROUP BY
		JCA_JH,
		MainOrg,
		GC_RX_NKLocalCurrency

CREATE TABLE #DeclarationPeriodCharge
(
	MainOrg uniqueidentifier,
	Supplier uniqueidentifier,
	Importer uniqueidentifier,
	Origin uniqueidentifier,
	OriginTableCode varchar(2),
	Destination uniqueidentifier,
	DestinationTableCode varchar(2),
	Mode varchar(3),
	TradeLaneType varchar(3),
	JobCompany uniqueidentifier,
	PeriodStart date,
	Currency varchar(3),
	ChargeCompany uniqueidentifier,
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4),
)

INSERT INTO #DeclarationPeriodCharge
SELECT 
	[MainOrg] = MainOrg,
	[Supplier] = Supplier,
	[Importer] = Importer,
	[Origin] = Origin,
	[OriginTableCode] = OriginTableCode,
	[Destination] = Destination,
	[DestinationTableCode] = DestinationTableCode,
	[Mode] = Mode,
	[TradeLaneType] = TradeLaneType,
	[JobCompany] = JobCompany,
	[PeriodStart] = PeriodStart,
	[Currency] = ISNULL([Currency], ''),
	[ChargeCompany] = JH_GC,
	[JobRevenue] = CASE WHEN SUM(ISNULL([JobRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[JobCost] = CASE WHEN ABS(SUM(ISNULL([JobCost], 0))) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobCost], 0)) WHEN SUM(ISNULL([JobCost], 0)) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END,
	[MainOrgRevenue] = CASE WHEN SUM(ISNULL([MainOrgRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[MainOrgCost] = CASE WHEN ABS(SUM(ISNULL([MainOrgCost], 0))) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgCost], 0)) WHEN SUM(ISNULL([MainOrgCost], 0)) < 0 THEN -@RevenueCostMaxValue ELSE @RevenueCostMaxValue END
FROM
	#DeclarationCharge Charge
	JOIN #Declaration Declaration ON Charge.JH_PK = Declaration.JH_PK
	JOIN #DeclarationDetail Detail ON Declaration.JE_PK = Detail.JE_PK
GROUP BY
	MainOrg,
	Supplier,
	Importer,
	Origin,
	OriginTableCode,
	Destination,
	DestinationTableCode,
	Mode,
	TradeLaneType,
	JobCompany,
	PeriodStart,
	[Currency],
	JH_GC

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = tl.MainOrg,
	[Supplier] = tl.Supplier,
	[Buyer] = tl.Importer,
	[Origin] = tl.Origin,
	[OriginTableCode] = tl.OriginTableCode,
	[Destination] = tl.Destination,
	[DestinationTableCode] = tl.DestinationTableCode,
	[Warehouse] = NULL,
	[Status] = 3,
	[Product] = 'BRK',
	[Service] = '',
	[Mode] = tl.Mode,
	[TradeLaneType] = tl.TradeLaneType,
	[PeriodStart] = tl.PeriodStart,
	[PeriodLastTrade] = tl.PeriodLastTrade,
	[JobCount] = tl.JobCount,
	[PalletCount] = 0,
	[LineCount] = 0,
	[WeightVolume] = 0,
	[WeightAmount] = tl.WeightAmount,
	[VolumeM3] = tl.VolumeM3,
	[ChargeableAmount] = 0,
	[ChargeableUnits] = 0,
	[TEU] = 0,
	[SupplierPart] = NULL,
	[JobCompany] = tl.JobCompany,
	[Currency] = ch.Currency,
	[JobRevenue] = ch.JobRevenue,
	[JobCost] = ch.JobCost,
	[MainOrgRevenue] = ch.MainOrgRevenue,
	[MainOrgCost] = ch.MainOrgCost,
	[ChargeCompany] = ch.ChargeCompany,
	[RelatedJobID] = NULL
FROM
	#DeclarationPeriod tl
	LEFT JOIN #DeclarationPeriodCharge ch ON
		tl.MainOrg = ch.MainOrg
		AND tl.Mode = ch.Mode
		AND tl.TradeLaneType = ch.TradeLaneType
		AND tl.PeriodStart = ch.PeriodStart
		AND (tl.Supplier = ch.Supplier OR (tl.Supplier IS NULL AND ch.Supplier IS NULL))
		AND (tl.Importer = ch.Importer OR (tl.Importer IS NULL AND ch.Importer IS NULL))
		AND (tl.Origin = ch.Origin OR (tl.Origin IS NULL AND ch.Origin IS NULL))
		AND (tl.OriginTableCode = ch.OriginTableCode OR (tl.OriginTableCode IS NULL AND ch.OriginTableCode IS NULL))
		AND (tl.Destination = ch.Destination OR (tl.Destination IS NULL AND ch.Destination IS NULL))
		AND (tl.DestinationTableCode = ch.DestinationTableCode OR (tl.DestinationTableCode IS NULL AND ch.DestinationTableCode IS NULL))
		AND (tl.JobCompany = ch.JobCompany OR (tl.JobCompany IS NULL AND ch.JobCompany IS NULL))

IF OBJECT_ID('tempdb.dbo.#Declaration') IS NOT NULL DROP TABLE #Declaration
IF OBJECT_ID('tempdb.dbo.#DeclarationOrg') IS NOT NULL DROP TABLE #DeclarationOrg
IF OBJECT_ID('tempdb.dbo.#DeclarationDetail') IS NOT NULL DROP TABLE #DeclarationDetail
IF OBJECT_ID('tempdb.dbo.#DeclarationPeriod') IS NOT NULL DROP TABLE #DeclarationPeriod
IF OBJECT_ID('tempdb.dbo.#DeclarationCharge') IS NOT NULL DROP TABLE #DeclarationCharge
IF OBJECT_ID('tempdb.dbo.#DeclarationPeriodCharge') IS NOT NULL DROP TABLE #DeclarationPeriodCharge
";
		}

		#endregion

		#region Warehouse

		static string CreateTradeLinesSummaryCacheQuery_Warehouse(OrgParameterType orgParamType)
		{
			return
@"
---------------------
-- Warehouse Order & Receipts
---------------------

CREATE TABLE #Docket
(
	WD_PK uniqueidentifier,
	WD_ExternalReference varchar(35) COLLATE SQL_Latin1_General_CP1_CI_AS,
	WD_OH_Client uniqueidentifier,
	Consignor_OH uniqueidentifier,
	Consignee_OH uniqueidentifier,
	CNROrgAddressOA_OH uniqueidentifier,
	CNEOrgAddressOA_OH uniqueidentifier,
	DocketDate smalldatetime,
	JH_GC uniqueidentifier,
);

" + GenerateUnionedSelectForOrgFilter("INSERT INTO #Docket", @"
SELECT
	[WD_PK] = WD_PK,
	[WD_ExternalReference] = WD_ExternalReference,
	[WD_OH_Client] = WD_OH_Client,
	[Consignor_OH] = CASE WHEN WD_DocketType = 'ORD' THEN WD_OH_Client ELSE CNROrgAddress.OA_OH END,
	[Consignee_OH] = CASE WHEN WD_DocketType = 'INW' THEN WD_OH_Client ELSE CNEOrgAddress.OA_OH END ,
	[CNROrgAddressOA_OH] = CNROrgAddress.OA_OH,
	[CNEOrgAddressOA_OH] = CNEOrgAddress.OA_OH,
	[DocketDate] = COALESCE(WD_FinalisedDate AT TIME ZONE 'UTC', WD_SystemCreateTimeUtc),
	[JH_GC] = JH_GC
FROM
	dbo.WhsDocket
	LEFT JOIN dbo.JobDocAddress CNRDocAddress ON CNRDocAddress.E2_ParentID = WD_PK AND CNRDocAddress.E2_ParentTableCode = 'WD' AND CNRDocAddress.E2_AddressType = 'SUD'
	LEFT JOIN dbo.JobDocAddress CNEDocAddress ON CNEDocAddress.E2_ParentID = WD_PK AND CNEDocAddress.E2_ParentTableCode = 'WD' AND CNEDocAddress.E2_AddressType = 'CEA'
	LEFT JOIN dbo.OrgAddress CNROrgAddress ON CNRDocAddress.E2_OA_Address = CNROrgAddress.OA_PK AND CNRDocAddress.E2_AddressOverride = 0
	LEFT JOIN dbo.OrgAddress CNEOrgAddress ON CNEDocAddress.E2_OA_Address = CNEOrgAddress.OA_PK AND CNEDocAddress.E2_AddressOverride = 0
	LEFT JOIN dbo.JobHeader ON WD_PK = JH_ParentID AND JH_ParentTableCode = 'WD'
WHERE
	WD_DocketType IN ('ORD', 'INW')
	AND COALESCE(WD_FinalisedDate, WD_SystemCreateTimeUtc) >= @From
	AND COALESCE(WD_FinalisedDate, WD_SystemCreateTimeUtc) < @To
", orgParamType, WhsDocketSchema.Constants.WD_OH_Client, "CNROrgAddressOA_OH", "CNEOrgAddressOA_OH") + @"

CREATE TABLE #DocketOrg
(
	MainOrg uniqueidentifier,
	WD_PK uniqueidentifier,
)

INSERT INTO #DocketOrg
SELECT [MainOrg] = Consignor_OH, WD_PK FROM #Docket WHERE Consignor_OH IS NOT NULL
UNION ALL
SELECT [MainOrg] = Consignee_OH, WD_PK FROM #Docket WHERE Consignee_OH IS NOT NULL 
	AND (Consignee_OH != Consignor_OH OR Consignor_OH IS NULL)
UNION ALL
SELECT [MainOrg] = WD_OH_Client, WD_PK FROM #Docket WHERE WD_OH_Client IS NOT NULL 
	AND (WD_OH_Client != Consignor_OH OR Consignor_OH IS NULL) 
	AND (WD_OH_Client != Consignee_OH OR Consignee_OH IS NULL)

CREATE TABLE #DocketCharge
(
	MainOrg uniqueidentifier,
	WD_PK uniqueidentifier,
	ProductPk uniqueidentifier,
	JH_GC uniqueidentifier,
	Currency varchar(3),
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4),
)

CREATE TABLE #DocketChargeStorage
(
	JR_AL_ARLine uniqueidentifier,
	ET_PK uniqueidentifier,
	WD_PK uniqueidentifier,
)

INSERT INTO #DocketChargeStorage
SELECT
	JR_AL_ARLine,
	ET_PK,
	WD_PK
FROM dbo.JobCharge
JOIN dbo.JobChargeAttrib
	ON EC_JR = JR_PK AND EC_Name = 'DRE'
JOIN #Docket ON WD_ExternalReference = EC_Value
JOIN dbo.JobStorage ON ET_OH_Client = WD_OH_Client

DECLARE @SelectedCompany table (CompanyPk uniqueidentifier)
INSERT INTO @SelectedCompany
SELECT distinct JH_GC
FROM #Docket
WHERE JH_GC IS NOT NULL

DECLARE @DocketChargePeriodKey table(PeriodKey char(42))

INSERT INTO @DocketChargePeriodKey
SELECT t.PeriodKey
FROM (SELECT CompanyPk FROM @SelectedCompany) as companyPKs
	CROSS APPLY dbo.RptDt_GetPeriodKeys(@SmallDateTimeMinValue, @SmallDateTimeMaxValue, CompanyPk) as t

	INSERT INTO #DocketCharge
	SELECT
		[MainOrg] = MainOrg,
		[WD_PK] = WD_PK,
		[ProductPk] = OP_PK,
		[JH_GC] = JH_GC,
		[Currency] = Currency,
		[JobRevenue] = SUM(JobRevenue),
		[JobCost] = SUM(JobCost),
		[MainOrgRevenue] = SUM(MainOrgRevenue),
		[MainOrgCost] = SUM(MainOrgCost)
	FROM
		(
		SELECT
			[MainOrg] = MainOrg,
			[WD_PK] = Charges.WD_PK,
			[OP_PK] = OP_PK,
			[JH_GC] = JH_GC,
			[Currency] = GC_RX_NKLocalCurrency,
			[JobRevenue]	 = SUM(Revenue),
			[JobCost]		 = SUM(Cost),
			[MainOrgRevenue] = SUM(CASE WHEN LineOrgPk = MainOrg THEN Revenue ELSE 0 END),
			[MainOrgCost]	 = SUM(CASE WHEN LineOrgPk = MainOrg THEN Cost ELSE 0 END)
		FROM
			(
				SELECT
					WD_PK,
					OP_PK,
					JH_GC,
					GC_RX_NKLocalCurrency,
					LineOrgPk = JCD_OH,
					Revenue = CASE WHEN JCD_LineType IN ('REV', 'WIP') THEN JCD_LineAmount ELSE 0 END,
					Cost = CASE WHEN JCD_LineType IN ('CST', 'ACR')  THEN JCD_LineAmount ELSE 0 END
				FROM
					(
						SELECT 
							WD_PK, OP_PK, JH_GC, JCD_LineType, JCD_OH, JCD_LineAmount
						FROM
							(
								SELECT WD_PK, WD_OH_Client, EC_Value, JH_GC = Docket.JH_GC, JCD_LineType, JCD_OH, JCD_LineAmount
								FROM 
									dbo.RptDtJobCostingData
									JOIN @DocketChargePeriodKey p on p.PeriodKey = JCD_PeriodCompanyKey
									JOIN #Docket Docket ON WD_PK = JCD_ParentID
									JOIN dbo.JobCharge ON JCD_AL = JR_AL_ARLine
									JOIN dbo.JobChargeAttrib ON EC_JR = JR_PK AND EC_Name = 'PRD'
								WHERE
									JCD_LineType IN ('REV', 'CST', 'WIP', 'ACR')
									AND JCD_ParentTableCode = 'WD'
							) a 
							JOIN
							(
								SELECT DISTINCT 
									WD_OH_Client, OP_PK, OP_PartNum 
								FROM 
									#Docket Docket
									JOIN dbo.OrgPartRelation ON WD_OH_Client = OU_OH 
									JOIN dbo.OrgSupplierPart ON OU_OP = OP_PK
							) b
							ON a.WD_OH_Client = b.WD_OH_Client AND a.EC_Value = b.OP_PartNum
					) c 
					JOIN dbo.GlbCompany ON JH_GC = GC_PK
			) Charges
			JOIN #DocketOrg DocketOrg ON DocketOrg.WD_PK = Charges.WD_PK
		GROUP BY
			MainOrg,
			Charges.WD_PK,
			OP_PK,
			JH_GC,
			GC_RX_NKLocalCurrency

		UNION ALL

		SELECT
			[MainOrg] = MainOrg,
			[WD_PK] = Charges.WD_PK,
			[OP_PK] = NULL,
			[JH_GC] = JH_GC,
			[Currency] = GC_RX_NKLocalCurrency,
			[JobRevenue]	 = SUM(Revenue),
			[JobCost]		 = SUM(Cost),
			[MainOrgRevenue] = SUM(CASE WHEN LineOrgPk = MainOrg THEN Revenue ELSE 0 END),
			[MainOrgCost]	 = SUM(CASE WHEN LineOrgPk = MainOrg THEN Cost ELSE 0 END)
		FROM
			(
				SELECT
					WD_PK,
					JH_GC = Docket.JH_GC,
					GC_RX_NKLocalCurrency,
					LineOrgPk = JCD_OH,
					Revenue = CASE WHEN JCD_LineType IN ('REV', 'WIP') THEN JCD_LineAmount ELSE 0 END,
					Cost = CASE WHEN JCD_LineType IN ('CST', 'ACR')  THEN JCD_LineAmount ELSE 0 END
				FROM
					dbo.RptDtJobCostingData
					JOIN @DocketChargePeriodKey p on p.PeriodKey = JCD_PeriodCompanyKey
					JOIN #Docket Docket ON JCD_ParentID = WD_PK
					JOIN dbo.GlbCompany ON Docket.JH_GC = GC_PK
					LEFT JOIN dbo.JobCharge ON JCD_AL = JR_AL_ARLine
					LEFT JOIN dbo.JobChargeAttrib ON EC_JR = JR_PK AND EC_Name = 'PRD'
				WHERE
					EC_Value IS NULL 
					AND JCD_LineType IN ('REV', 'CST', 'WIP', 'ACR')
					AND JCD_ParentTableCode = 'WD'
			) Charges
			JOIN #DocketOrg DocketOrg ON DocketOrg.WD_PK = Charges.WD_PK
		GROUP BY
			MainOrg,
			Charges.WD_PK,
			JH_GC,
			GC_RX_NKLocalCurrency

		UNION ALL

		-- lines created on periodic invoice
		SELECT
			[MainOrg] = MainOrg,
			[WD_PK] = Charges.WD_PK,
			[OP_PK] = NULL,
			[JH_GC] = JH_GC,
			[Currency] = GC_RX_NKLocalCurrency,
			[JobRevenue]	 = SUM(Revenue),
			[JobCost]		 = SUM(Cost),
			[MainOrgRevenue] = SUM(CASE WHEN LineOrgPk = MainOrg THEN Revenue ELSE 0 END),
			[MainOrgCost]	 = SUM(CASE WHEN LineOrgPk = MainOrg THEN Cost ELSE 0 END)
		FROM
			(
				SELECT
					WD_PK = DocketChargeStorage.WD_PK,
					JH_GC = JCD_GC,
					GC_RX_NKLocalCurrency,
					LineOrgPk = JCD_OH,
					Revenue = CASE WHEN JCD_LineType IN ('REV', 'WIP') THEN JCD_LineAmount ELSE 0 END,
					Cost = CASE WHEN JCD_LineType IN ('CST', 'ACR')  THEN JCD_LineAmount ELSE 0 END
				FROM
					dbo.RptDtJobCostingData
					JOIN @DocketChargePeriodKey p on p.PeriodKey = JCD_PeriodCompanyKey
					JOIN dbo.GlbCompany ON JCD_GC = GC_PK
					JOIN #DocketChargeStorage DocketChargeStorage
						ON JCD_AL = DocketChargeStorage.JR_AL_ARLine AND JCD_ParentID = DocketChargeStorage.ET_PK
				WHERE
					JCD_LineType IN ('REV', 'CST', 'WIP', 'ACR')
					AND JCD_ParentTableCode = 'ET'
			) Charges
			JOIN #DocketOrg DocketOrg ON DocketOrg.WD_PK = Charges.WD_PK
			GROUP BY
				MainOrg,
				Charges.WD_PK,
				JH_GC,
				GC_RX_NKLocalCurrency
		) x
	GROUP BY
		MainOrg,
		WD_PK,
		OP_PK,
		JH_GC,
		Currency

CREATE TABLE #DocketPeriod
(
	MainOrg uniqueidentifier,
	Consignor uniqueidentifier,
	Consignee uniqueidentifier,
	SupplierPart uniqueidentifier,
	Warehouse uniqueidentifier,
	Service varchar(3),
	PeriodStart date,
	PeriodLastTrade smalldatetime,
	JobCount int,
	PalletCount int,
	LineCount int,
	WeightVolume decimal(12, 3),
	WeightAmount decimal(12, 3),
	VolumeM3 decimal(12, 3)
)

-- per docket line
INSERT INTO #DocketPeriod
SELECT
	[MainOrg] = MainOrg,
	[Consignor] = Consignor_OH,
	[Consignee] = Consignee_OH,
	[SupplierPart] = DocketProductLine.WE_OP,
	[Warehouse] = WD_WW_Whs,
	[Service] = 
		CASE WD_DocketType
			WHEN 'ORD' THEN 'ORD'
			WHEN 'INW' THEN 'REC'
			ELSE ''
		END,
	[PeriodStart] = DATEADD(m, DATEDIFF(m, 0, Docket.DocketDate), 0),
	[PeriodLastTrade] = MAX(COALESCE(WD_FinalisedDate AT TIME ZONE 'UTC', WD_SystemCreateTimeUtc)),
	[JobCount] = CASE WHEN COUNT(*) <= @CountMaxValue THEN COUNT(*) ELSE @CountMaxValue END,
	[PalletCount] = 0,
	[LineCount] = CASE WHEN SUM(LineCount) <= @CountMaxValue THEN SUM(LineCount) ELSE @CountMaxValue END,
	[WeightVolume] = CASE WHEN SUM(UnitCount) <= @WeightVolumeMaxValue THEN SUM(UnitCount) ELSE @WeightVolumeMaxValue END,
	[WeightAmount] = 0,
	[VolumeM3] = 0
FROM
	#Docket Docket
	JOIN dbo.WhsDocket ON Docket.WD_PK = WhsDocket.WD_PK
	JOIN
	(
		SELECT
			WE_WD,
			WE_OP,
			COUNT(*) [LineCount],
			SUM(WE_TransactionQuantity) [UnitCount]
		FROM dbo.WhsDocketLine line
		GROUP BY
			WE_WD,
			WE_OP
	) DocketProductLine ON DocketProductLine.WE_WD = Docket.WD_PK
	JOIN #DocketOrg Org ON Docket.WD_PK = Org.WD_PK
GROUP BY
	MainOrg,
	Consignor_OH,
	Consignee_OH,
	DATEADD(m, DATEDIFF(m, 0, Docket.DocketDate), 0),
	WD_WW_Whs,
	DocketProductLine.WE_OP,
	WD_DocketType

-- per docket
INSERT INTO #DocketPeriod
SELECT
	[MainOrg] = MainOrg,
	[Consignor] = Consignor_OH,
	[Consignee] = Consignee_OH,
	[SupplierPart] = NULL,
	[Warehouse] = WD_WW_Whs,
	[Service] = 
		CASE WD_DocketType
			WHEN 'ORD' THEN 'ORD'
			WHEN 'INW' THEN 'REC'
			ELSE ''
		END,
	[PeriodStart] = DATEADD(m, DATEDIFF(m, 0, Docket.DocketDate), 0),
	[PeriodLastTrade] = MAX(COALESCE(WD_FinalisedDate AT TIME ZONE 'UTC', WD_SystemCreateTimeUtc)),
	[JobCount] = CASE WHEN COUNT(*) <= @CountMaxValue THEN COUNT(*) ELSE @CountMaxValue END,
	[PalletCount] =
		CASE WD_DocketType
			WHEN 'ORD' THEN CASE WHEN SUM(WD_PalletsSent) <= @CountMaxValue THEN SUM(WD_PalletsSent) ELSE @CountMaxValue END
			WHEN 'INW' THEN CASE WHEN SUM(WD_TotalPallets) <= @CountMaxValue THEN SUM(WD_TotalPallets) ELSE @CountMaxValue END
			ELSE 0
		END,
	[LineCount] = 0,
	[WeightVolume] = 0,
	[WeightAmount] = CASE WHEN SUM(ISNULL(ConvertedActualWeight.Value, 0)) <= @WeightVolumeMaxValue THEN SUM(ISNULL(ConvertedActualWeight.Value, 0)) ELSE @WeightVolumeMaxValue END,
	[VolumeM3] = CASE WHEN SUM(ISNULL(ConvertedActualVolume.Value, 0)) <= @WeightVolumeMaxValue THEN SUM(ISNULL(ConvertedActualVolume.Value, 0)) ELSE @WeightVolumeMaxValue END
FROM
	#Docket Docket
	JOIN dbo.WhsDocket ON Docket.WD_PK = WhsDocket.WD_PK
	JOIN #DocketOrg Org ON Docket.WD_PK = Org.WD_PK
	CROSS APPLY dbo.ConvertWeight(WD_TotalWeight, WD_TotalWeightUnit, 'T') AS ConvertedActualWeight
	CROSS APPLY dbo.ConvertVolume(WD_TotalCubic, WD_TotalCubicUnit, 'M3') AS ConvertedActualVolume
GROUP BY
	MainOrg,
	Consignor_OH,
	Consignee_OH,
	DATEADD(m, DATEDIFF(m, 0, Docket.DocketDate), 0),
	WD_WW_Whs,
	WD_DocketType

CREATE TABLE #DocketPeriodCharge
(
	MainOrg uniqueidentifier,
	Consignor uniqueidentifier,
	Consignee uniqueidentifier,
	SupplierPart uniqueidentifier,
	PeriodStart date,
	Currency varchar(3),
	ChargeCompany uniqueidentifier,
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	MainOrgRevenue decimal(19,4),
	MainOrgCost decimal(19,4),
)

INSERT INTO #DocketPeriodCharge
SELECT
	[MainOrg] = MainOrg,
	[Consignor] = Consignor_OH,
	[Consignee] = Consignee_OH,
	[SupplierPart] = ProductPk,
	[PeriodStart] = DATEADD(m, DATEDIFF(m, 0, Docket.DocketDate), 0),
	[Currency] = ISNULL([Currency], ''),
	[ChargeCompany] = Charge.JH_GC,
	[JobRevenue] = CASE WHEN SUM(ISNULL([JobRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[JobCost] = CASE WHEN SUM(ISNULL([JobCost], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([JobCost], 0)) ELSE @RevenueCostMaxValue END,
	[MainOrgRevenue] = CASE WHEN SUM(ISNULL([MainOrgRevenue], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgRevenue], 0)) ELSE @RevenueCostMaxValue END,
	[MainOrgCost] = CASE WHEN SUM(ISNULL([MainOrgCost], 0)) <= @RevenueCostMaxValue THEN SUM(ISNULL([MainOrgCost], 0)) ELSE @RevenueCostMaxValue END
FROM
	#DocketCharge Charge
	JOIN #Docket Docket ON Charge.WD_PK = Docket.WD_PK
GROUP BY
	MainOrg,
	Consignor_OH,
	Consignee_OH,
	ProductPk,
	DATEADD(m, DATEDIFF(m, 0, Docket.DocketDate), 0),
	[Currency],
	Charge.JH_GC

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = tl.MainOrg,
	[Supplier] = tl.Consignor,
	[Buyer] = tl.Consignee,
	[Origin] = NULL,
	[OriginTableCode] = '',
	[Destination] = NULL,
	[DestinationTableCode] = '',
	[Warehouse] = tl.Warehouse,
	[Status] = 3,
	[Product] = 'WHS',
	[Service] = tl.Service,
	[Mode] = '',
	[TradeLaneType] = '',
	[PeriodStart] = tl.PeriodStart,
	[PeriodLastTrade] = tl.PeriodLastTrade,
	[JobCount] = tl.JobCount,
	[PalletCount] = tl.PalletCount,
	[LineCount] = tl.LineCount,
	[WeightVolume] = tl.WeightVolume,
	[WeightAmount] = tl.WeightAmount,
	[VolumeM3] = tl.VolumeM3,
	[ChargeableAmount] = 0,
	[ChargeableUnits] = '',
	[TEU] = 0,
	[SupplierPart] = ch.SupplierPart,
	[JobCompany] = NULL,
	[Currency] = ch.Currency,
	[Revenue] = ch.JobRevenue,
	[Cost] = ch.JobCost,
	[MainOrgRevenue] = ch.MainOrgRevenue,
	[MainOrgCost] = ch.MainOrgCost,
	[ChargeCompany] = ch.ChargeCompany,
	[RelatedJobID] = NULL
FROM
	#DocketPeriod tl
	LEFT JOIN #DocketPeriodCharge ch ON
		tl.MainOrg = ch.MainOrg
		AND tl.PeriodStart = ch.PeriodStart
		AND (tl.SupplierPart = ch.SupplierPart OR (tl.SupplierPart IS NULL AND ch.SupplierPart IS NULL))
		AND (tl.Consignor = ch.Consignor OR (tl.Consignor IS NULL AND ch.Consignor IS NULL))
		AND (tl.Consignee = ch.Consignee OR (tl.Consignee IS NULL AND ch.Consignee IS NULL))

IF OBJECT_ID('tempdb.dbo.#Docket') IS NOT NULL DROP TABLE #Docket
IF OBJECT_ID('tempdb.dbo.#DocketOrg') IS NOT NULL DROP TABLE #DocketOrg
IF OBJECT_ID('tempdb.dbo.#DocketCharge') IS NOT NULL DROP TABLE #DocketCharge
IF OBJECT_ID('tempdb.dbo.#DocketChargeStorage') IS NOT NULL DROP TABLE #DocketChargeStorage
IF OBJECT_ID('tempdb.dbo.#DocketPeriod') IS NOT NULL DROP TABLE #DocketPeriod
IF OBJECT_ID('tempdb.dbo.#DocketPeriodCharge') IS NOT NULL DROP TABLE #DocketPeriodCharge

---------------------
-- Storage
---------------------

CREATE TABLE #Storage
(
	ET_PK uniqueidentifier,
	ET_WW uniqueidentifier,
	ET_StorageFromDate date,
	ET_OH_Client uniqueidentifier,
	JH_PK uniqueidentifier,
	JH_GC uniqueidentifier,
	JH_ParentTableCode varchar(3),
	JH_ParentID uniqueidentifier
)

INSERT INTO #Storage
SELECT
	ET_PK,
	ET_WW,
	ET_StorageFromDate,
	ET_OH_Client,
	JH_PK,
	JH_GC,
	JH_ParentTableCode,
	JH_ParentID
FROM
	dbo.JobStorage
	JOIN dbo.JobHeader ON JH_ParentID = ET_PK AND JH_ParentTableCode = 'ET'

CREATE TABLE #StorageCharge
(
	ClientPk uniqueidentifier,
	WarehousePK uniqueidentifier,
	ProductPk uniqueidentifier,
	Period date,
	CompanyPk uniqueidentifier,
	Currency varchar(3),
	JobRevenue decimal(19,4),
	JobCost decimal(19,4),
	LocalClientRevenue decimal(19,4),
	LocalClientCost decimal(19,4)
)

DECLARE @SelectedCompanyStorage table (CompanyPk uniqueidentifier)
INSERT INTO @SelectedCompanyStorage
SELECT distinct JH_GC
FROM #Storage
WHERE JH_GC IS NOT NULL

DECLARE @StorageChargePeriodKey table(PeriodKey char(42))
DECLARE @CompanyPk uniqueidentifier

INSERT INTO @StorageChargePeriodKey
SELECT t.PeriodKey
FROM (SELECT CompanyPk FROM @SelectedCompanyStorage) as companyPKs
	CROSS APPLY dbo.RptDt_GetPeriodKeys(@SmallDateTimeMinValue, @SmallDateTimeMaxValue, CompanyPk) as t

INSERT INTO #StorageCharge
SELECT
	[ClientPk] = ET_OH_Client,
	[WarehousePK] = ET_WW,
	[ProductPk] = OP_PK,
	[Period] = DATEADD(month, DATEDIFF(month, 0, ET_StorageFromDate), 0),
	[CompanyPk] = GC_PK,
	[Currency] = GC_RX_NKLocalCurrency,
	[JobRevenue]	 = SUM(Revenue),
	[JobCost]		 = SUM(Cost),
	[LocalClientRevenue] = SUM(CASE WHEN LineOrgPk = ET_OH_Client THEN Revenue ELSE 0 END),
	[LocalClientCost]	 = SUM(CASE WHEN LineOrgPk = ET_OH_Client THEN Cost ELSE 0 END)
FROM
	(
		SELECT
			ET_WW,
			OP_PK,
			ET_StorageFromDate,
			JCD_GC,
			LineOrgPk = JCD_OH,
			ET_OH_Client,
			Revenue = CASE WHEN JCD_LineType IN ('REV', 'WIP') THEN JCD_LineAmount ELSE 0 END,
			Cost = CASE WHEN JCD_LineType IN ('CST', 'ACR')  THEN JCD_LineAmount ELSE 0 END
		FROM
			dbo.RptDtJobCostingData
			JOIN @StorageChargePeriodKey p on p.PeriodKey = JCD_PeriodCompanyKey
			JOIN #Storage ON JCD_JH = JH_PK AND JH_GC = JCD_GC
			JOIN dbo.JobCharge ON JCD_AL = JR_AL_ARLine
			CROSS APPLY
			(
				SELECT OP_PK
				FROM
					dbo.OrgSupplierPart
					JOIN dbo.JobChargeAttrib ON EC_Value = OP_PartNum
				WHERE
					EC_JR = JR_PK
					AND EC_Name = 'PRD'
					AND EXISTS (SELECT 1 FROM dbo.OrgPartRelation WHERE OU_OH = ET_OH_Client AND OU_OP = OP_PK)
			) SupplierPart
		WHERE
			JCD_LineType IN ('REV', 'CST', 'WIP', 'ACR')
			AND ET_StorageFromDate >= @From
			AND ET_StorageFromDate < @To
			" + GetOrgSqlWhereClause(orgParamType, "ET_OH_Client") + @"
	) a
	JOIN dbo.GlbCompany ON JCD_GC = GC_PK
GROUP BY
	ET_OH_Client,
	ET_WW,
	OP_PK,
	DATEADD(month, DATEDIFF(month, 0, ET_StorageFromDate), 0),
	GC_PK,
	GC_RX_NKLocalCurrency

CREATE TABLE #InventoryArrivalSummary
(
	ClientPk uniqueidentifier,
	WarehousePk uniqueidentifier,
	ProductPk uniqueidentifier,
	Count int,
	Period date,
	LastArrivalDate date
)

INSERT INTO #InventoryArrivalSummary
SELECT
	[ClientPk] = WI_OH_Client,
	[WarehousePk] = WA_WW_Whs,
	[ProductPk] = WI_OP,
	[Count] = CASE WHEN COUNT(*) <= @CountMaxValue THEN COUNT(*) ELSE @CountMaxValue END,
	[Period] = DATEADD(month, DATEDIFF(month, 0, WI_ArrivalDate AT TIME ZONE 'UTC'), 0),
	[LastArrivalDate] = MAX(WI_ArrivalDate AT TIME ZONE 'UTC')
FROM
	dbo.WhsInventoryView
	JOIN dbo.WhsLocation ON WI_WL = WL_PK
	JOIN dbo.WhsArea ON WL_WA_PickingArea = WA_PK
WHERE
	WI_ArrivalDate >= @From
	AND WI_ArrivalDate < @To
	" + GetOrgSqlWhereClause(orgParamType, "WI_OH_Client") +
@"
GROUP BY
	WI_OH_Client,
	WA_WW_Whs,
	WI_OP,
	DATEADD(month, DATEDIFF(month, 0, WI_ArrivalDate AT TIME ZONE 'UTC'), 0)

INSERT INTO #TradeLinesSummaryCache
SELECT
	[MainOrg] = COALESCE(invArrival.ClientPk, storageCharge.ClientPk),
	[Supplier] = NULL,
	[Buyer] = NULL,
	[Origin] = NULL,
	[OriginTableCode] = '',
	[Destination] = NULL,
	[DestinationTableCode] = '',
	[Warehouse] = COALESCE(invArrival.WarehousePk, storageCharge.WarehousePk),
	[Status] = 3,
	[Product] = 'WHS',
	[Service] = 'STG',
	[Mode] = '',
	[TradeLaneType] = '',
	[PeriodStart] = COALESCE(invArrival.Period, storageCharge.Period),
	[PeriodLastTrade] = COALESCE(invArrival.LastArrivalDate, storageCharge.Period),
	[JobCount] = ISNULL(invArrival.Count, 0),
	[PalletCount] = 0,
	[LineCount] = 0,
	[WeightVolume] = 0,
	[WeightAmount] = 0,
	[VolumeM3] = 0,
	[ChargeableAmount] = 0,
	[ChargeableUnits] = '',
	[TEU] = 0,
	[SupplierPart] = COALESCE(invArrival.ProductPk, storageCharge.ProductPk),
	[JobCompany] = NULL,
	[Currency] = ISNULL([Currency], ''),
	[JobRevenue] = CASE WHEN ISNULL([JobRevenue], 0) <= @RevenueCostMaxValue THEN ISNULL([JobRevenue], 0) ELSE @RevenueCostMaxValue END,
	[JobCost] = CASE WHEN ISNULL([JobCost], 0) <= @RevenueCostMaxValue THEN ISNULL([JobCost], 0) ELSE @RevenueCostMaxValue END,
	[MainOrgRevenue] = CASE WHEN ISNULL([LocalClientRevenue], 0) <= @RevenueCostMaxValue THEN ISNULL([LocalClientRevenue], 0) ELSE @RevenueCostMaxValue END,
	[MainOrgCost] = CASE WHEN ISNULL([LocalClientCost], 0) <= @RevenueCostMaxValue THEN ISNULL([LocalClientCost], 0) ELSE @RevenueCostMaxValue END,
	[ChargeCompany] = CompanyPk,
	[RelatedJobID] = NULL
FROM
	#StorageCharge storageCharge
	FULL OUTER JOIN #InventoryArrivalSummary invArrival
		ON storageCharge.ClientPk = invArrival.ClientPk
		AND storageCharge.WarehousePk = invArrival.WarehousePk
		AND storageCharge.ProductPk = invArrival.ProductPk
		AND storageCharge.Period = invArrival.Period

IF OBJECT_ID('tempdb.dbo.#Storage') IS NOT NULL DROP TABLE #Storage
IF OBJECT_ID('tempdb.dbo.#StorageCharge') IS NOT NULL DROP TABLE #StorageCharge
IF OBJECT_ID('tempdb.dbo.#InventoryArrivalSummary') IS NOT NULL DROP TABLE #InventoryArrivalSummary

";
		}

		#endregion

		protected const string FromParamName = "@From";
		protected const string ToParamName = "@To";
		protected const string OrgPkParamName = "@OrgPk";

		#endregion

		#endregion

		#region Dispose
		protected override void Dispose(bool isDisposing)
		{
		}
		#endregion
	}
}
