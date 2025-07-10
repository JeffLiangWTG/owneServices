using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public partial class CycleCountVarianceProcessingManager
	{
		public CycleCountVarianceProcessingManager(ILogger logger)
		{
			Logger = logger;
		}

		readonly ILogger Logger;

		public void ProcessCycleCountVariances()
		{
			RejectVariances();
			AcceptVariances();
		}

		#region AcceptVariances

		void AcceptVariances()
		{
			var factory = new BusinessObjectFactory();
			var approvedCycleCountSQL = @"
SELECT 
	WCL_PK 
FROM
	dbo.WhsCycleCountLocation
WHERE
	WCL_Granularity NOT IN (@PalletCount, @ProductOnly) AND
	WCL_PK IN 
	(
		SELECT 
			WCC_WCL_CycleCountLocation 
		FROM 
			dbo.WhsCycleCountLocationVariance
		WHERE
			WCC_AuthorizedAction = @AuthorizedAction
		GROUP BY
			WCC_WCL_CycleCountLocation
	)
";
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@PalletCount", CycleCountGranularity.Codes.PalletCount, WhsCycleCountLocationSchema.WCL_Granularity);
			parameters.Add("@ProductOnly", CycleCountGranularity.Codes.ProductOnly, WhsCycleCountLocationSchema.WCL_Granularity);
			parameters.Add("@AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Approved, WhsCycleCountLocationVarianceSchema.WCC_AuthorizedAction);

			var cycleCountQuery = new ZDBOnlyQuery(typeof(WhsCycleCountLocation));
			cycleCountQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsCycleCountLocationSchema.PK.Name, approvedCycleCountSQL), parameters);

			var cycleCounts = factory.Load<WhsCycleCountLocation>(cycleCountQuery);
			if (cycleCounts.Length > 0)
			{
				var cycleCountsGroupedByWarehouse = cycleCounts.GroupBy(cycleCount => cycleCount.Location.WLV_WW_Whs);
				var warehouses = factory.Load<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, cycleCountsGroupedByWarehouse.Select(group => group.Key)));
				foreach (var cycleCountGroup in cycleCountsGroupedByWarehouse)
				{
					var referenceWarehouse = warehouses.Single(whs => whs.PK == cycleCountGroup.Key);
					using (WarehouseUserContextHelper.SetUserContextForWarehouse(referenceWarehouse))
					{
						AcceptVariancesCore(cycleCountGroup.ToArray());
					}
				}
			}
			else
			{
				Logger.Information(Res.GetString("6c0b47a8-baee-4f7b-a0f0-3aa8c3750e35", "Did not find any variances authorized for Approval."));
			}
		}

		void AcceptVariancesCore(WhsCycleCountLocation[] cycleCounts)
		{
			foreach (var cycleCount in cycleCounts)
			{
				var newFactory = new BusinessObjectFactory();
				CreateFactorySaveExceptionForTest(newFactory);

				var cycleCountInNewFactory = (WhsCycleCountLocation)newFactory.ImportFromAnotherFactory(cycleCount);
				cycleCountInNewFactory.AcceptVariances(new WhsDocketILoggerWrapper(Logger));

				ClearFactorySaveExceptionForTest();
			}
		}

		partial void CreateFactorySaveExceptionForTest(BusinessObjectFactory factory);
		partial void ClearFactorySaveExceptionForTest();

		#endregion

		#region RejectVariance

		public void RejectVariances()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rejectedCycleCountLocationSQL = @"
SELECT 
	DISTINCT WCC_WCL_CycleCountLocation, WLV_LocationString, WLV_WW_Whs
FROM 
	dbo.WhsCycleCountLocationVariance
JOIN	
	dbo.WhsCycleCountLocation ON WCC_WCL_CycleCountLocation = WCL_PK
JOIN 
	dbo.WhsLocationView ON WCL_WL_Location = WLV_PK
WHERE 
	WCC_AuthorizedAction = @RejectedAuthorizedAction
ORDER BY WLV_LocationString
";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@RejectedAuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Rejected, WhsCycleCountLocationVarianceSchema.WCC_AuthorizedAction);

			var cycleCounts = new DynamicBusinessObjectCollection(factory);
			cycleCounts.Load(rejectedCycleCountLocationSQL, parameters);

			if (cycleCounts.Count > 0)
			{
				var cycleCountsGroupedByWarehouse = cycleCounts.GroupBy(cycleCount => (ZGuid)cycleCount[WhsLocationViewSchema.Constants.WLV_WW_Whs]);
				var warehouses = factory.Load<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, cycleCountsGroupedByWarehouse.Select(group => group.Key)));
				foreach (var cycleCountGroup in cycleCountsGroupedByWarehouse)
				{
					var referenceWarehouse = warehouses.Single(whs => whs.PK == cycleCountGroup.Key);
					using (WarehouseUserContextHelper.SetUserContextForWarehouse(referenceWarehouse))
					{
						RejectVariancesCore(cycleCountGroup.ToArray());
					}
				}
			}
			else
			{
				Logger.Information(Res.GetString("fcde536f-172c-4ff0-ac91-1dc3c246d554", "Did not find any variances authorized for Rejection."));
			}
		}

		void RejectVariancesCore(DynamicBusinessObject[] cycleCounts)
		{
			foreach (var cycleCount in cycleCounts)
			{
				var factoryForCCL = new BusinessObjectFactory();
				var cycleCountLocationInNewFactory = factoryForCCL.Load<WhsCycleCountLocation>(((ZGuid)cycleCount[WhsCycleCountLocationVarianceSchema.Constants.WCC_WCL_CycleCountLocation]));
				cycleCountLocationInNewFactory.RejectVariances(new WhsDocketILoggerWrapper(Logger));
			}
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public partial class CycleCountVarianceProcessingManager
	{
		partial void CreateFactorySaveExceptionForTest(BusinessObjectFactory factory)
		{
			CreateSaveExceptionForTest?.Invoke(factory);
		}

		partial void ClearFactorySaveExceptionForTest()
		{
			CreateSaveExceptionForTest = null;
		}

		public Action<BusinessObjectFactory> CreateSaveExceptionForTest;
	}
}

#endif
#endregion
