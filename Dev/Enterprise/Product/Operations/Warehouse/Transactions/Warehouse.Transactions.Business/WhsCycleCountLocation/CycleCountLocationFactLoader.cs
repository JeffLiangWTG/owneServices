using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class CycleCountLocationFactLoader<TFact> : ICycleCountLocationFactLoader
		where TFact : CycleCountLocationFact
	{
		class Schema
		{
			public const string LocationIndexForSort = nameof(LocationIndexForSort);
			public const string IsProxyOrgOfCurrentCompany = nameof(IsProxyOrgOfCurrentCompany);
			public const string IsProxyOrgOfAnyCompany = nameof(IsProxyOrgOfAnyCompany);
			public const string HasCommittedStock = nameof(HasCommittedStock);
			public const string EntityPK = nameof(EntityPK);
		}

		public IEnumerable<IInputFact> LoadInputFacts(ReadOnlyBusinessObjectFactory factory, ZGuid warehousePK, CancellationToken cancellationToken)
		{
			Argument.NotNull(factory, nameof(factory));

			var locations = GetLocations(factory, warehousePK);
			var inventories = GetInventories(factory, warehousePK);
			return GetCycleCountLocationFacts(locations, inventories, cancellationToken);
		}

		DataRow[] GetLocations(ReadOnlyBusinessObjectFactory factory, ZGuid warehousePK)
		{
			var whsParamName = $"@WhsPK_{ParameterSuffixer.Instance.GetParameterSuffix(ZDateTime.Now.ToDateTime(), WhsLocationViewSchema.WLV_WW_Whs, warehousePK.ToGuid())}";
			var whsParameter = ZSqlParameter.New(whsParamName, warehousePK, WhsLocationViewSchema.WLV_WW_Whs);

			var sql = Invariant($@"
SELECT
	{EntityPK},
	WLV_PK,
	WLV_LocationClass,
	WLV_LocationStatus,
	WA_Name,
	WR_PickPathSequence,
	WLV_RowName,
	WLV_Column,
	WLV_Level,
	WLV_Tray,
	WLV_LocationString,
	WLV_CycleCountPathSequence,
	WLV_CycleCountLastPerformed,
	WLV_LastInventoryChangeDate,
	WLV_PickMethod,
	WLV_LocationTypeCode,
	LocationIndexForSort.LocationIndex as LocationIndexForSort,{PriorityAndGranularityFields}
FROM
	dbo.WhsLocationView{AdditionalJoins}
	JOIN dbo.WhsArea ON WA_PK = WLV_WA_PickingArea
	JOIN dbo.WhsRow ON WLV_WR = WR_PK
	CROSS APPLY dbo.WhsLocationIndexForSort(WLV_Column, WLV_Level, WLV_Tray, WR_Levels, WR_Trays) as LocationIndexForSort
WHERE
	WLV_WW_Whs = {whsParamName}{AdditionalWhereClause}
");

			void addParameters(DbCommand command)
			{
				command.AddParameter(whsParameter);
			}

			return DataRowLoader.Load(factory, sql, addParameters);
		}

		protected virtual string EntityPK => $"WLV_PK AS {Schema.EntityPK}";
		protected virtual string AdditionalWhereClause => string.Empty;
		protected virtual string AdditionalJoins => string.Empty;
		protected abstract string PriorityAndGranularityFields { get; }

		DataRow[] GetInventories(ReadOnlyBusinessObjectFactory factory, ZGuid warehousePK)
		{
			var whsParameter = ZSqlParameter.New("@WhsPK", warehousePK, WhsInventoryViewSchema.WI_WW_Whs);
			var companyParameter = ZSqlParameter.New("@CompanyPK", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);

			var paramNameFactory = new ParameterNameFactory();
			whsParameter.Rename(paramNameFactory.GetParameterName(whsParameter));
			companyParameter.Rename(paramNameFactory.GetParameterName(companyParameter));

			var sql = $@"
SELECT
	WI_WL,
	WI_OH_Client,
	OH_Code,
	WI_OP,
	OP_PartNum,
	OP_RH_NKCommodityCode,
	OPC_CategoryCode,
	OU_PK,
	OU_UnitPrice,
	OU_RX_NKUnitPriceCurrency,
	WJ_Category,
	WI_TotalUnits,
	HasCommittedStock,
	CAST(CASE WHEN IsProxyOfCurrentCompany = 1 OR IsProxyOfCurrentBranch = 1 THEN 1 ELSE 0 END AS BIT) AS IsProxyOrgOfCurrentCompany,
	CAST(CASE WHEN IsProxyOfCurrentCompany = 1 OR IsProxyOfCurrentBranch = 1 OR IsProxyOfAnyCompany = 1 OR IsProxyOfAnyBranch = 1 THEN 1 ELSE 0 END AS BIT) AS IsProxyOrgOfAnyCompany
FROM
	(
		SELECT
			WI_WL,
			WI_OP,
			WI_OH_Client,
			SUM(WI_TotalUnits) AS WI_TotalUnits,
			CAST(CASE WHEN MAX(WZ_Units) IS NULL THEN 0 ELSE 1 END AS BIT) AS HasCommittedStock
		FROM
			dbo.WhsInventoryView
			OUTER APPLY
			(
				SELECT TOP 1
					WZ_Units
				FROM
					dbo.WhsCommittedStock
				WHERE
					WI_PK = WZ_WE_InventoryLine AND WZ_Units > 0
			) CommittedStock
		WHERE
			WI_WW_Whs = {whsParameter.ParameterName}
			AND WI_TotalUnits > 0
			AND WI_WL IS NOT NULL
		GROUP BY
			WI_WL, WI_OP, WI_OH_Client
	) Inventory
	JOIN dbo.OrgSupplierPart ON OP_PK = WI_OP
	JOIN dbo.OrgHeader ON OH_PK = WI_OH_Client
	JOIN dbo.OrgPartRelation ON OU_OP = WI_OP AND OU_OH = WI_OH_Client AND OU_Relationship IN ('OWN', 'BTH')
	LEFT JOIN dbo.OrgPartCategory ON OPC_PK = OU_OPC_Category
	OUTER APPLY
	(
		SELECT TOP 1
			WJ_Category
		FROM
			dbo.WhsABCCategory
		WHERE
			WJ_OP_Product = WI_OP AND WJ_OH_Client = WI_OH_Client AND WJ_WW_Warehouse = {whsParameter.ParameterName}
		ORDER BY WJ_AnalysisDateTo DESC
	) ABCCategory
	OUTER APPLY
	(
		SELECT TOP 1
			GC_IsActive AS IsProxyOfCurrentCompany
		FROM
			dbo.GlbCompany
		WHERE
			GC_PK = {companyParameter.ParameterName} AND GC_IsActive = 1 AND GC_OH_OrgProxy = WI_OH_Client
	) CurrentCompanyProxy
	OUTER APPLY
	(
		SELECT TOP 1
			GB_IsActive AS IsProxyOfCurrentBranch
		FROM
			dbo.GlbBranch
		WHERE
			GB_GC = {companyParameter.ParameterName} AND GB_IsActive = 1 AND GB_OH_OrgProxy = WI_OH_Client
	) CurrentBranchProxy
	OUTER APPLY
	(
		SELECT TOP 1
			GC_IsActive AS IsProxyOfAnyCompany
		FROM
			dbo.GlbCompany
		WHERE
			GC_IsActive = 1 AND GC_OH_OrgProxy = WI_OH_Client
	) AnyCompanyProxy
	OUTER APPLY
	(
		SELECT TOP 1
			GB_IsActive AS IsProxyOfAnyBranch
		FROM
			dbo.GlbBranch
		WHERE
			GB_IsActive = 1 AND GB_OH_OrgProxy = WI_OH_Client
	) AnyBranchProxy";

			void addParameters(DbCommand command)
			{
				command.AddParameter(whsParameter);
				command.AddParameter(companyParameter);
			}

			return DataRowLoader.Load(factory, sql, addParameters);
		}

		IEnumerable<CycleCountLocationFact> GetCycleCountLocationFacts(DataRow[] locations, DataRow[] inventories, CancellationToken cancellationToken)
		{
			var cycleCountLocationCoreFacts = new Dictionary<ZGuid, CycleCountLocationCoreFact>(locations.Length);
			var locationsWithoutInventory = new HashSet<ZGuid>(locations.Length);
			var productFacts = new Dictionary<ZGuid, CycleCountProductFact>();
			var clientFacts = new Dictionary<ZGuid, OrganisationFact>();

			if (locations.Length > 0)
			{
				var columns = locations[0].Table.Columns;
				var pkIndex = columns.IndexOf(WhsLocationViewSchema.PK.Name);
				var entityPkIndex = columns.IndexOf(Schema.EntityPK);
				var locationClassIndex = columns.IndexOf(WhsLocationViewSchema.WLV_LocationClass.Name);
				var locationStringIndex = columns.IndexOf(WhsLocationViewSchema.WLV_LocationString.Name);
				var locationStatusIndex = columns.IndexOf(WhsLocationViewSchema.WLV_LocationStatus.Name);
				var areaNameIndex = columns.IndexOf(WhsAreaSchema.WA_Name.Name);
				var rowPathSequenceIndex = columns.IndexOf(WhsRowSchema.WR_PickPathSequence.Name);
				var rowNameIndex = columns.IndexOf(WhsLocationViewSchema.WLV_RowName.Name);
				var columnIndex = columns.IndexOf(WhsLocationViewSchema.WLV_Column.Name);
				var levelIndex = columns.IndexOf(WhsLocationViewSchema.WLV_Level.Name);
				var trayIndex = columns.IndexOf(WhsLocationViewSchema.WLV_Tray.Name);
				var cycleCountPathSequenceIndex = columns.IndexOf(WhsLocationViewSchema.WLV_CycleCountPathSequence.Name);
				var cycleCountLastPerformedIndex = columns.IndexOf(WhsLocationViewSchema.WLV_CycleCountLastPerformed.Name);
				var lastInventoryChangeDateIndex = columns.IndexOf(WhsLocationViewSchema.WLV_LastInventoryChangeDate.Name);
				var pickMethodIndex = columns.IndexOf(WhsLocationViewSchema.WLV_PickMethod.Name);
				var locationTypeCodeIndex = columns.IndexOf(WhsLocationViewSchema.WLV_LocationTypeCode.Name);
				var locationIndexForSortIndex = columns.IndexOf(Schema.LocationIndexForSort);
				var granularityIndex = columns.IndexOf(WhsCycleCountLocationSchema.WCL_Granularity.Name);
				var priorityIndex = columns.IndexOf(WhsCycleCountLocationSchema.WCL_Priority.Name);

				foreach (var location in locations)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var locationPK = (Guid)location[pkIndex];
					var entityPK = (Guid)location[entityPkIndex];
					var locationTypeCode = (string)location[locationTypeCodeIndex];
					var locationClass = (string)location[locationClassIndex];
					var areaName = (string)location[areaNameIndex];
					var rowName = (string)location[rowNameIndex];
					var column = (short)location[columnIndex];
					var level = (short)location[levelIndex];
					var tray = (short)location[trayIndex];
					var locationString = (string)location[locationStringIndex];
					var locationStatus = (string)location[locationStatusIndex];
					var cycleCountPathSequence = (int)location[cycleCountPathSequenceIndex];
					var rowPathSequence = (short)location[rowPathSequenceIndex];
					var locationStringSortIndex = (int)location[locationIndexForSortIndex];
					var inventoryLastChangedDate = DataRowLoader.GetNullableDateTimeFromDateTimeOffset(location[lastInventoryChangeDateIndex]);
					var cycleCountLastPerformedDate = DataRowLoader.GetNullableDateTimeFromDateTimeOffset(location[cycleCountLastPerformedIndex]);
					var pickMethod = (string)location[pickMethodIndex];
					var granularity = (string)location[granularityIndex];
					var priority = (byte)location[priorityIndex];

					var cycleCountLocation = new CycleCountLocationCoreFact(
						locationPK,
						entityPK,
						locationTypeCode,
						locationClass,
						areaName,
						rowName,
						column,
						level,
						tray,
						locationString,
						locationStatus,
						pickMethod,
						cycleCountTaskExists: CycleCountTaskExists,
						cycleCountPathSequence,
						rowPathSequence,
						locationStringSortIndex,
						inventoryLastChangedDate,
						cycleCountLastPerformedDate,
						stockOnHand: 0m,
						hasCommittedStock: false,
						granularity,
						priority);

					cycleCountLocationCoreFacts.Add(locationPK, cycleCountLocation);
					locationsWithoutInventory.Add(locationPK);
				}
			}

			if (inventories.Length > 0)
			{
				var columns = inventories[0].Table.Columns;
				var locationPKIndex = columns.IndexOf(WhsInventoryViewSchema.WI_WL.Name);
				var partCodeIndex = columns.IndexOf(OrgSupplierPartSchema.OP_PartNum.Name);
				var partCommodityIndex = columns.IndexOf(OrgSupplierPartSchema.OP_RH_NKCommodityCode.Name);
				var partCategoryCodeIndex = columns.IndexOf(OrgPartCategorySchema.OPC_CategoryCode.Name);
				var partABCCategoryIndex = columns.IndexOf(WhsABCCategorySchema.WJ_Category.Name);
				var partRelationPKIndex = columns.IndexOf(OrgPartRelationSchema.PK.Name);
				var partUnitPriceIndex = columns.IndexOf(OrgPartRelationSchema.OU_UnitPrice.Name);
				var partUnitPriceCurrencyIndex = columns.IndexOf(OrgPartRelationSchema.OU_RX_NKUnitPriceCurrency.Name);
				var clientPKIndex = columns.IndexOf(WhsInventoryViewSchema.WI_OH_Client.Name);
				var clientCodeIndex = columns.IndexOf(OrgHeaderSchema.OH_Code.Name);
				var isProxyOrgOfCurrentCompanyIndex = columns.IndexOf(Schema.IsProxyOrgOfCurrentCompany);
				var isProxyOrgOfAnyCompanyIndex = columns.IndexOf(Schema.IsProxyOrgOfAnyCompany);
				var totalUnitsIndex = columns.IndexOf(WhsInventoryViewSchema.WI_TotalUnits.Name);
				var hasCommittedStockIndex = columns.IndexOf(Schema.HasCommittedStock);

				foreach (var inventory in inventories)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var partRelation = (Guid)inventory[partRelationPKIndex];
					if (!productFacts.TryGetValue(partRelation, out var productFact))
					{
						var partCode = (string)inventory[partCodeIndex];
						var partCommodity = (string)inventory[partCommodityIndex];
						var partCategoryCode = DataRowLoader.GetNullableString(inventory[partCategoryCodeIndex]);
						var partABCCategory = DataRowLoader.GetNullableString(inventory[partABCCategoryIndex]);
						var partUnitPrice = (decimal)inventory[partUnitPriceIndex];
						var partUnitPriceCurrency = DataRowLoader.GetNullableString(inventory[partUnitPriceCurrencyIndex]);

						productFact = new CycleCountProductFact(partRelation, partCode, partCategoryCode, partCommodity, partABCCategory, partUnitPrice, partUnitPriceCurrency);
						productFacts.Add(partRelation, productFact);
					}

					var clientPK = (Guid)inventory[clientPKIndex];
					if (!clientFacts.TryGetValue(clientPK, out var clientFact))
					{
						var clientCode = (string)inventory[clientCodeIndex];
						var clientIsProxyOrgOfCurrentCompany = (bool)inventory[isProxyOrgOfCurrentCompanyIndex];
						var clientIsProxyOrgOfAnyCompany = (bool)inventory[isProxyOrgOfAnyCompanyIndex];

						clientFact = new OrganisationFact(clientPK, clientCode, clientIsProxyOrgOfAnyCompany, clientIsProxyOrgOfCurrentCompany);
						clientFacts.Add(clientPK, clientFact);
					}

					var locationPK = (Guid)inventory[locationPKIndex];
					var totalUnits = (decimal)inventory[totalUnitsIndex];
					var hasCommittedStock = (bool)inventory[hasCommittedStockIndex];

					if (cycleCountLocationCoreFacts.TryGetValue(locationPK, out var relatedLocationFact))
					{
						relatedLocationFact.StockOnHand += totalUnits;
						relatedLocationFact.HasCommittedStock |= hasCommittedStock;
						var cycleCountLocationFact = CreateLocationFact(relatedLocationFact, clientFact, productFact, totalUnits);

						yield return cycleCountLocationFact;
					}

					locationsWithoutInventory.Remove(locationPK);
				}
			}

			foreach (var locationPK in locationsWithoutInventory)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (cycleCountLocationCoreFacts.TryGetValue(locationPK, out var locationFact))
				{
					var cycleCountLocationFact = CreateLocationFact(locationFact, client: null, product: null, stockOnHandForThisProduct: 0m);

					yield return cycleCountLocationFact;
				}
			}
		}

		protected virtual bool CycleCountTaskExists => false;

		protected abstract TFact CreateLocationFact(
			CycleCountLocationCoreFact location,
			IOrganisationFact client,
			ICycleCountProductFact product,
			decimal stockOnHandForThisProduct);
	}
}
