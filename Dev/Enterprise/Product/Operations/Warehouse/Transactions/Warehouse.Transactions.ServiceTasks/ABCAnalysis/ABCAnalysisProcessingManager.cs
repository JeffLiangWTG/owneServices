using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class ABCAnalysisProcessingManager
	{
		public ABCAnalysisProcessingManager(ILogger logger)
		{
			this.logger = new LoggerWrapper(logger);
		}

		readonly LoggerWrapper logger;

		#region LoggerWrapper

		class LoggerWrapper
		{
			internal LoggerWrapper(ILogger logger)
			{
				this.logger = logger;
			}

			readonly ILogger logger;

			internal bool HasErrors { get; private set; }

			internal void Log(LogType logtype, string message)
			{
				logger.Log(logtype, message);

				if (logtype == LogType.Error)
				{
					HasErrors = true;
				}
			}
		}

		#endregion

		#region DoABCAnalysis

		public void DoABCAnalysis()
		{
			var factory = new BusinessObjectFactory();
			var warehouseQuery = new ZQuery(WhsWarehouseSchema.WW_IsActive, true);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_ABCAnalysisEnabled, true);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone });
			warehouseQuery.OrderBy = WhsWarehouseSchema.WW_WarehouseCode.Name + OrderByClause.Descending;
			var warehouses = factory.Load<WhsWarehouse>(warehouseQuery);
			var warehousesGroupedByBranch = warehouses.GroupBy(warehouse => warehouse.WW_GB_RelatedCompanyBranch);

			foreach (var grouping in warehousesGroupedByBranch)
			{
				using (WarehouseUserContextHelper.SetUserContextForWarehouse(grouping.First()))
				{
					DoABCAnalysisCore(grouping.ToArray());
				}
			}

			if (logger.HasErrors)
			{
				logger.Log(LogType.Information, Res.GetString("7832f948-d04f-49cb-b8f6-5c60a75c2ac1", "ABC Analysis failed."));
			}
			else
			{
				LogSuccessfulMessage(warehouses);
			}
		}

		void DoABCAnalysisCore(WhsWarehouse[] warehouses)
		{
			var sql = GetSQLQuery();

			try
			{
				Db.Connection.ExecuteNonQuery(sql, (cmd) =>
				{
					cmd.AddParameterBasedOnDbColumn("@Today", ZDateTimeOffset.Now, WhsABCCategorySchema.WJ_AnalysisDateTo);
					cmd.AddParameterBasedOnDbColumn("@TodayUtc", ZDateTime.UtcNow, WhsABCCategorySchema.WJ_SystemCreateTimeUtc);
					cmd.AddTableValuedParameter("@WarehousePKs", WhsDocketSchema.WD_WW_Whs, warehouses.Select(warehouse => warehouse.PK));
				});
			}
			catch (SqlException exception)
			{
				logger.Log(LogType.Error, exception.Message);
			}
		}

		#region GetSQLQuery

		string GetSQLQuery()
		{
			#region SuppressResourceStringsCheckRegion

			var statement = string.Join("\r\nUNION ",
				new WhsABCAnalysisPeriodCodeList()
				.Cast<ICodeDescription>()
				.Select(abcPeriod =>
				string.Format(CultureInfo.InvariantCulture,
@"SELECT '{0}' as ABCPeriod, *
FROM csfn_ABCAnalysisDateRange(@Today, '{1}')",
				abcPeriod.Code,
				abcPeriod.Code == WhsABCAnalysisPeriodCodeList.Codes.Default ? RegistryABCAnalysisPeriod : abcPeriod.Code)));

			var result = new ZStringBuilder();
			result.Append("CREATE TABLE #Categories (Category varchar(3) collate SQL_Latin1_General_CP1_CI_AS, RangeFrom decimal(10,3), RangeTo decimal(10, 3))");

			var categories = RegistryABCAnalysisCategories.Cast<ABCAnalysisCategory>().OrderByDescending(c => c.PercentageOfTotal);
			int rangeTo = 0;
			foreach (ABCAnalysisCategory abcCategory in categories)
			{
				var rangeFrom = rangeTo == 0 ? 0 : rangeTo + 0.001;
				rangeTo = rangeTo + abcCategory.PercentageOfTotal;
				if (rangeTo >= 100)
				{
					rangeTo = 100;
				}

				if (rangeFrom <= 100)
				{
					result.Append(string.Format(CultureInfo.InvariantCulture, "INSERT INTO #Categories values ('{0}', {1}, {2})", abcCategory.CategoryName, rangeFrom, rangeTo));
				}
			}

			result.Append(string.Format(CultureInfo.InvariantCulture, @"
--Gets all products that are to be used for this ABC analysis
SELECT WD_OH_Client, WD_WW_Whs, WE_OP, WD_PK, WE_TransactionQuantity
INTO #ListForProducts
FROM dbo.WhsDocket
JOIN dbo.WhsDocketLine ON WE_WD = WD_PK
JOIN dbo.WhsWarehouse ON WW_PK = WD_WW_Whs
JOIN
(
	SELECT OH_PK, DateFrom, DateTo
	FROM dbo.OrgHeader
	JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
	JOIN ({0}) as DateRanges ON OM_WhsABCAnalysisPeriod = ABCPeriod
	WHERE OH_IsActive = 1 AND OH_IsWarehouseClient = 1 AND OM_WhsABCAnalysisEnabled = 1
) as Clients ON WD_OH_Client = OH_PK AND WD_FinalisedDate >= DateFrom AND WD_FinalisedDate < DateTo
WHERE WD_DocketType = 'ORD' AND WD_WW_Whs IN (SELECT value FROM @WarehousePKs)

--Gets total sum of orders & stock units for each product
SELECT WD_OH_Client, WD_WW_Whs, WE_OP, SUM(Velocity) as Velocity, SUM(QuantityConsumed) as QuantityConsumed
INTO #TotalsForProducts
FROM
(
	SELECT WD_OH_Client, WD_WW_Whs, WE_OP, CAST(COUNT(DISTINCT WD_PK) as decimal(10, 3)) as Velocity, SUM(WE_TransactionQuantity) as QuantityConsumed 
	FROM #ListForProducts
	GROUP BY WD_OH_Client, WD_WW_Whs, WE_OP
	UNION
	SELECT WJ_OH_Client, WJ_WW_Warehouse, WJ_OP_Product, 0, 0
	FROM dbo.WhsABCCategory
	WHERE WJ_WW_Warehouse IN (SELECT value FROM @WarehousePKs)
	GROUP BY WJ_OH_Client, WJ_WW_Warehouse, WJ_OP_Product
) as Totals
GROUP BY WD_OH_Client, WD_WW_Whs, WE_OP

--Gets Total StockUnits
SELECT WD_OH_Client, WD_WW_Whs, SUM(QuantityConsumed) as TotalQuantityConsumed
INTO #QuantityTotals
FROM #TotalsForProducts
GROUP BY WD_OH_Client, WD_WW_Whs

--Gets Total Orders
SELECT WD_OH_Client, WD_WW_Whs, COUNT(DISTINCT WD_PK) as OrderCount
INTO #VelocityAmount
FROM #ListForProducts
GROUP BY WD_OH_Client, WD_WW_Whs

--Groups Total StockUnits & Orders together
SELECT #QuantityTotals.WD_OH_Client, #QuantityTotals.WD_WW_Whs, OrderCount, TotalQuantityConsumed
INTO #Summary
FROM #QuantityTotals
JOIN #VelocityAmount ON #QuantityTotals.WD_OH_Client = #VelocityAmount.WD_OH_Client
	AND #QuantityTotals.WD_WW_Whs = #VelocityAmount.WD_WW_Whs

--Finds Percentage of Orders & Stock Units for each Product
-- QuantityConsumedRunningTotalStart & VelocityRunningTotalStart need to add 1
--		for example
--			Total:		100
--									Running Total		Start
--			product1:	80			80					1 = 0 + 1
--			product2:	14			94					81 = 80 + 1
--			product3:	6			100					95 = 94 + 1
--
SELECT
	WD_OH_Client,
	WD_WW_Whs,
	WE_OP,
	QuantityConsumedRatio,
	VelocityRatio,
	QuantityConsumedRunningTotalStart = SUM(QuantityConsumedRatio) OVER (Partition by WD_OH_Client, WD_WW_Whs ORDER BY QuantityConsumedRatio DESC) - SUM(QuantityConsumedRatio) OVER (PARTITION BY WD_OH_Client, WD_WW_Whs, QuantityConsumedRatio) + 1,
	VelocityRunningTotalStart = SUM(VelocityRatio) OVER (Partition by WD_OH_Client, WD_WW_Whs ORDER BY VelocityRatio DESC) - SUM(VelocityRatio) OVER (PARTITION BY WD_OH_Client, WD_WW_Whs, VelocityRatio) + 1
INTO #ProductRatios
FROM
(
	SELECT 
		T1.WD_OH_Client, 
		T1.WD_WW_Whs, 
		WE_OP, 
		CASE WHEN OrderCount = 0 THEN 0 ELSE CAST(Velocity / OrderCount * 100 as decimal(10, 3)) END as VelocityRatio,
		CASE WHEN TotalQuantityConsumed = 0 THEN 0 ELSE CAST(QuantityConsumed / TotalQuantityConsumed * 100 as decimal(10,3)) END as QuantityConsumedRatio
	FROM #TotalsForProducts as T1
	JOIN #Summary as T2 ON T1.WD_OH_Client = T2.WD_OH_Client AND T1.WD_WW_Whs = T2.WD_WW_Whs
) AS ProductRatio
", statement));

			result.Append(FormattableString.Invariant($@"
--Categorises the products based on ABC analysis categories
SELECT 
	WD_OH_Client, 
	WD_WW_Whs, 
	WE_OP, 
    NewCategory = CASE WHEN ABCAnalysisMethod = 'VLC' THEN CategoryForVLC.Category WHEN ABCAnalysisMethod = 'QTC' THEN CategoryForQTC.Category ELSE CategoryForVLC.Category + ' ' + CategoryForQTC.Category END,
	ABCKey = CASE WHEN ABCAnalysisMethod = 'VLC' THEN CAST(CAST(CategoryForVLC.RangeFrom as int) as varchar(2)) + ' ' + CAST(CAST(CategoryForVLC.RangeTo as int) as varchar(3)) + ' ' + ABCAnalysisMethod
                  WHEN ABCAnalysisMethod = 'QTC' THEN CAST(CAST(CategoryForQTC.RangeFrom as int) as varchar(2)) + ' ' + CAST(CAST(CategoryForQTC.RangeTo as int) as varchar(3)) + ' ' + ABCAnalysisMethod
                  ELSE CAST(CAST(CategoryForVLC.RangeFrom as int) as varchar(2)) + ' ' + CAST(CAST(CategoryForVLC.RangeTo as int) as varchar(3)) + ' ' +
                       CAST(CAST(CategoryForQTC.RangeFrom as int) as varchar(2)) + ' ' + CAST(CAST(CategoryForQTC.RangeTo as int) as varchar(3)) + ' ' +
                       ABCAnalysisMethod
                  END,
	Period = CASE WHEN OM_WhsABCAnalysisPeriod = 'DEF' THEN '{RegistryABCAnalysisPeriod}' ELSE OM_WhsABCAnalysisPeriod END
INTO #ActualResult
FROM #ProductRatios
JOIN dbo.OrgHeader ON WD_OH_Client = OH_PK
JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
CROSS APPLY 
(
	SELECT 
    CASE   
        WHEN OM_WhsABCAnalysisMethod = 'DEF' THEN '{RegistryABCAnalysisMethod}'   
        ELSE OM_WhsABCAnalysisMethod 
    END AS ABCAnalysisMethod 
) AS AnalysisMethod
JOIN #Categories CategoryForVLC ON VelocityRunningTotalStart >= CategoryForVLC.RangeFrom AND (VelocityRunningTotalStart <= CategoryForVLC.RangeTo OR (VelocityRunningTotalStart >= 100 AND CategoryForVLC.RangeTo = 100))
JOIN #Categories CategoryForQTC ON QuantityConsumedRunningTotalStart >= CategoryForQTC.RangeFrom AND (QuantityConsumedRunningTotalStart <= CategoryForQTC.RangeTo OR (QuantityConsumedRunningTotalStart >= 100 AND CategoryForQTC.RangeTo = 100))"
			));

			result.Append(FormattableString.Invariant($@"
--Finds the current ABC category for each product
SELECT WJ_PK, WJ_OH_Client, WJ_WW_Warehouse, WJ_OP_Product, WJ_Key, WJ_AnalysisPeriod, WJ_Category as LastCategoryRecorded
INTO #MostRecentCategoryResult
FROM
(
	SELECT WJ_PK, WJ_OH_Client, WJ_WW_Warehouse, WJ_OP_Product, WJ_Key, WJ_AnalysisPeriod, WJ_Category,
	ROW_NUMBER() OVER (PARTITION BY WJ_OH_Client, WJ_WW_Warehouse, WJ_OP_Product ORDER BY WJ_AnalysisDateTo DESC) as RowNumber
	FROM dbo.WhsABCCategory
	WHERE WJ_WW_Warehouse in (SELECT value from @WarehousePKs)
) as Ranks
WHERE RowNumber = 1

--Finds existing & non-existing ABC categories by product, client & warehouse
SELECT WJ_PK, NewCategory, Period, ABCKey, WJ_AnalysisPeriod, WJ_Key, LastCategoryRecorded,
ISNULL(WJ_OP_Product, WE_OP) as Product, ISNULL(WJ_OH_Client, WD_OH_Client) as Client, ISNULL(WJ_WW_Warehouse, WD_WW_Whs) as Warehouse
INTO #LastResult
FROM #ActualResult
FULL OUTER JOIN #MostRecentCategoryResult ON WD_OH_Client = WJ_OH_Client AND WD_WW_Whs = WJ_WW_Warehouse AND WE_OP = WJ_OP_Product

--Prevent duplicate records being created, can happen if service task is run within the same minute
UPDATE
	dbo.WhsABCCategory
SET
	WJ_AnalysisDateFrom = DateAdd(minute, -1, WJ_AnalysisDateFrom),
	WJ_AnalysisDateTo = DateAdd(minute, -1, WJ_AnalysisDateTo),
	WJ_SystemLastEditTimeUtc = @TodayUtc,
	WJ_SystemLastEditUser = '~BP'
FROM
	dbo.WhsABCCategory as ABC
JOIN
	#MostRecentCategoryResult ON ABC.WJ_PK = #MostRecentCategoryResult.WJ_PK
WHERE
	WJ_AnalysisDateTo = @Today

--Inserts a new ABCCategory row if the category | key has changed
INSERT INTO dbo.WhsABCCategory (WJ_PK, WJ_AnalysisDateFrom, WJ_AnalysisDateTo, WJ_AnalysisPeriod, WJ_Key, WJ_Category, WJ_OP_Product, WJ_OH_Client, WJ_WW_Warehouse, WJ_SystemCreateUser, WJ_SystemLastEditUser, WJ_SystemCreateTimeUtc, WJ_SystemLastEditTimeUtc)
SELECT NewID(), @Today, @Today, Period, ABCKey, NewCategory, Product, Client, Warehouse, '~BP', '~BP', @TodayUtc, @TodayUtc
FROM #LastResult
WHERE LastCategoryRecorded IS NULL OR ((LastCategoryRecorded <> NewCategory OR Period <> WJ_AnalysisPeriod OR ABCKey <> WJ_Key) AND NewCategory IS NOT NULL)

--Updates the existing ABCCategory row if category & key remains the same
UPDATE dbo.WhsABCCategory
SET WJ_AnalysisDateTo = @Today,
WJ_SystemLastEditTimeUtc = @TodayUtc,
WJ_SystemLastEditUser = '~BP'
FROM dbo.WhsABCCategory as ABC
JOIN #LastResult ON ABC.WJ_PK = #LastResult.WJ_PK
WHERE LastCategoryRecorded IS NOT NULL AND ((LastCategoryRecorded = NewCategory AND Period = ABC.WJ_AnalysisPeriod AND ABCKey = ABC.WJ_Key) OR NewCategory IS NULL)

--Inserts a new WhsProductParamsByWhsAndClient for that product if it does not exist
INSERT INTO dbo.WhsProductParamsByWhsAndClient (W3_PK, W3_OP, W3_OH, W3_WW, W3_SystemCreateTimeUtc, W3_SystemCreateUser, W3_SystemLastEditTimeUtc, W3_SystemLastEditUser)
SELECT NewID(), Product, Client, Warehouse, @TodayUtc, '~BP', @TodayUtc, '~BP'
FROM #LastResult
WHERE NOT EXISTS
(
	SELECT W3_PK
	FROM dbo.WhsProductParamsByWhsAndClient
	WHERE W3_OP = Product AND W3_OH = Client AND W3_WW = Warehouse
)

IF OBJECT_ID('tempdb.dbo.#Categories') IS NOT NULL DROP TABLE #Categories;
IF OBJECT_ID('tempdb.dbo.#ListForProducts') IS NOT NULL DROP TABLE #ListForProducts;
IF OBJECT_ID('tempdb.dbo.#TotalsForProducts') IS NOT NULL DROP TABLE #TotalsForProducts;
IF OBJECT_ID('tempdb.dbo.#QuantityTotals') IS NOT NULL DROP TABLE #QuantityTotals;
IF OBJECT_ID('tempdb.dbo.#VelocityAmount') IS NOT NULL DROP TABLE #VelocityAmount;
IF OBJECT_ID('tempdb.dbo.#Summary') IS NOT NULL DROP TABLE #Summary;
IF OBJECT_ID('tempdb.dbo.#ProductRatios') IS NOT NULL DROP TABLE #ProductRatios;
IF OBJECT_ID('tempdb.dbo.#ActualResult') IS NOT NULL DROP TABLE #ActualResult;
IF OBJECT_ID('tempdb.dbo.#MostRecentCategoryResult') IS NOT NULL DROP TABLE #MostRecentCategoryResult;
IF OBJECT_ID('tempdb.dbo.#LastResult') IS NOT NULL DROP TABLE #LastResult;"));
			return result.ToStringWithNewLineBetweenAppends();

			#endregion
		}

		#endregion

		#region LogSuccessfulMessage

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Service Task log, not user specific")]
		void LogSuccessfulMessage(WhsWarehouse[] warehouses)
		{
			logger.Log(LogType.Information, Res.GetString("51fe42bc-03ef-4f21-8342-dff5c3afcbc5", "ABC Analysis completed successfully."));

			var result = new ZStringBuilder();
			var clientQuery = @"
SELECT OH_FullName, OH_Code
FROM dbo.OrgHeader
JOIN dbo.OrgMiscServ ON OM_OH = OH_PK
WHERE OH_IsActive = 1 AND OH_IsWarehouseClient = 1 AND OM_WhsABCAnalysisEnabled = 1
ORDER BY OH_CODE";

			var clients = new ZStringBuilder();
			using (var reader = Db.Connection.Command(clientQuery).ExecuteReader()) // ServiceTask deals with lots of data
			{
				while (reader.Read())
				{
					clients.Append(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", reader.GetString(0), reader.GetString(1)));
				}
			}

			result.AppendIfNotEmpty(Res.GetString("47fdadba-1fde-44c5-b9ad-b68e4f748ff9", "Clients:") + "\r\n", clients.ToStringWithNewLineBetweenAppends());

			var warehousesInfo = new ZStringBuilder();
			foreach (var warehouse in warehouses.OrderBy(whs => whs.WW_WarehouseCode))
			{
				warehousesInfo.Append(string.Format(CultureInfo.InvariantCulture, $"{warehouse.WW_WarehouseName} ({warehouse.WW_WarehouseCode})"));
			}

			result.AppendIfNotEmpty(Res.GetString("1751adeb-a945-465a-a1a5-a2772aa005d9", "Warehouses:") + "\r\n", warehousesInfo.ToStringWithNewLineBetweenAppends());
			if (result.IsEmpty)
			{
				result.Append(Res.GetString("4afc326d-97b2-4fa6-b336-0ad21606080f", "None"));
			}

			result.Prepend(Res.GetString("04e93776-0378-4a01-b2f4-bccb90c50c10", "Clients and warehouses included in this Analysis were:"));
			logger.Log(LogType.Information, result.ToStringWithNewLineBetweenAppends());
		}

		#endregion

		#endregion

		#region RegistryABCAnalysisPeriod

		string RegistryABCAnalysisPeriod
		{
			get { return WarehouseDataRegistry.Instance.ABCAnalysisPeriod.Value; }
		}

		#endregion

		#region RegistryABCAnalysisMethod

		string RegistryABCAnalysisMethod
		{
			get { return WarehouseDataRegistry.Instance.ABCAnalysisMethod.Value; }
		}

		#endregion

		#region RegistryABCAnalysisCategories

		ABCAnalysisCategoryCollection RegistryABCAnalysisCategories
		{
			get { return WarehouseDataRegistry.Instance.ABCAnalysisCategories.Value; }
		}

		#endregion
	}
}
