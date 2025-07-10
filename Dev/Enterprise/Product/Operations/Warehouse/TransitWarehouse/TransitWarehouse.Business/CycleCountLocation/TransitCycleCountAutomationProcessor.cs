using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Facts;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.TransitWarehouseCycleCountAutomation;
using WTG.ProductionRules.Core;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitCycleCountAutomationProcessor : IScheduledRuleProcessor
	{
		public TransitCycleCountAutomationProcessor(IWhsItemCycleCountLocationCreator cycleCountLocationCreator)
		{
			CycleCountLocationCreator = Argument.NotNull(cycleCountLocationCreator, nameof(cycleCountLocationCreator));
		}

		IWhsItemCycleCountLocationCreator CycleCountLocationCreator { get; }
		public string InformationMessageForNothingProcessed => (NoResString)"No cycle count tasks were created in this run."; // Service Task Logging

		public GuidRegistryItem ErrorContactGroupRegistryItem => WarehouseDataRegistry.Instance.CycleCountingAutomationFailureNotificationGroup;

		public ZGuid GetBranchToRunRulesAgainst(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(ruleSet, nameof(ruleSet));

			var warehouse = factory.Load<WhsWarehouse>(ruleSet.PRS_WW_Warehouse);
			return warehouse.WW_GB_RelatedCompanyBranch;
		}

		public IEnumerable<IInputFact> LoadInputFacts(ReadOnlyBusinessObjectFactory factory, IProductionRuleSet ruleSet, CancellationToken cancellationToken)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(ruleSet, nameof(ruleSet));

			var locations = GetLocations(factory, ruleSet.PRS_WW_Warehouse);
			var inventories = GetInventories(factory, ruleSet.PRS_WW_Warehouse);
			return GetCycleCountLocationFacts(factory, locations, inventories, cancellationToken);
		}

		public void ProcessResults(BusinessObjectFactory factory, ProductionRulesEngineResult result, INotifications notifications, CancellationToken cancellationToken)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(result, nameof(result));
			Argument.NotNull(notifications, nameof(notifications));

			var cycleCountTaskFacts = new List<TransitCycleCountTaskFact>();
			foreach (var fact in result.Facts)
			{
				if (fact is TransitCycleCountTaskFact cycleCountTaskFact)
				{
					cycleCountTaskFacts.Add(cycleCountTaskFact);
				}
			}

			if (cycleCountTaskFacts.Count > 0)
			{
				var cycleCountsByLocPK = cycleCountTaskFacts.ToDictionary(c => c.LocationPK);
				var cycleCountTasks =
					CycleCountLocationCreator.CreateCycleCountLocations(factory, cycleCountTaskFacts.Select(cy => new WhsItemCycleCountLocationInfo(cy.LocationPK, cy.Priority)));
				var createdForLocPKs = cycleCountTasks.Select(c => c.WIC_WL_Location).ToHashSet();

				foreach (var cycleCountTask in cycleCountTasks)
				{
					notifications.AddInformation($"Created Cycle Count Task for Location: {cycleCountsByLocPK[cycleCountTask.WIC_WL_Location.ToGuid()].LocationString}.");  // Service Task Logging
				}

				foreach (var cycleCount in cycleCountTaskFacts.Where(t => !createdForLocPKs.Contains(t.LocationPK)).ToArray())
				{
					notifications.AddWarning($"Skipped creating Cycle Count Task for Location: {cycleCountsByLocPK[cycleCount.LocationPK].LocationString} as one already exists.");  // Service Task Logging
				}
			}
			else
			{
				notifications.AddInformation(InformationMessageForNothingProcessed);
			}
		}

		DataRow[] GetLocations(ReadOnlyBusinessObjectFactory factory, ZGuid warehousePK)
		{
			var whsParamName = $"@WhsPK_{ParameterSuffixer.Instance.GetParameterSuffix(ZDateTime.Now.ToDateTime(), WhsLocationViewSchema.WLV_WW_Whs, warehousePK.ToGuid())}";
			var whsParameter = ZSqlParameter.New(whsParamName, warehousePK, WhsLocationViewSchema.WLV_WW_Whs);

			var sql = Invariant($@"
SELECT
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
	WLV_LocationTypeCode,
	LocationIndexForSort.LocationIndex as LocationIndexForSort
FROM
	dbo.WhsLocationView
	JOIN dbo.WhsArea ON WA_PK = WLV_WA_PickingArea
	JOIN dbo.WhsRow ON WLV_WR = WR_PK
	CROSS APPLY dbo.WhsLocationIndexForSort(WLV_Column, WLV_Level, WLV_Tray, WR_Levels, WR_Trays) as LocationIndexForSort
WHERE
	WLV_WW_Whs = {whsParamName}
	AND WLV_LocationStatus <> 'VOI'
	AND WLV_LocationClass NOT IN ('DDL', 'PST', 'CON')
	AND NOT EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsItemCycleCountLocation
		WHERE
			WIC_WL_Location = WLV_PK
			AND WIC_EndTime IS NULL
	)
	AND NOT EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.WhsItemCycleCountLocation
			JOIN dbo.WhsItemCycleCountLocationVariance ON WIV_WIC_CycleCountLocation = WIC_PK
		WHERE
			WIC_WL_Location = WLV_PK
			AND WIV_Status = 'OPN'
	)
");

			void addParameters(DbCommand command)
			{
				command.AddParameter(whsParameter);
			}

			return DataRowLoader.Load(factory, sql, addParameters);
		}

		DataRow[] GetInventories(ReadOnlyBusinessObjectFactory factory, ZGuid warehousePK)
		{
			var whsParameter = ZSqlParameter.New("@WhsPK", warehousePK, WhsInventoryViewSchema.WI_WW_Whs);

			var paramNameFactory = new ParameterNameFactory();
			whsParameter.Rename(paramNameFactory.GetParameterName(whsParameter));

			var sql = $@"
SELECT
	DISTINCT
	WPS_WL_LastLocation,
	CASE
		WHEN RCN_Consignor.OH_PK IS NOT NULL THEN RCN_Consignor.OH_PK
		ELSE DCN_Consignor.OH_PK
	END AS ConsignorPK,
	CASE
		WHEN RCN_Consignor.OH_Code IS NOT NULL THEN RCN_Consignor.OH_Code
		ELSE DCN_Consignor.OH_Code
	END AS ConsignorCode,
	CASE
		WHEN RCN_Consignee.OH_PK IS NOT NULL THEN RCN_Consignee.OH_PK
		ELSE DCN_Consignee.OH_PK
	END AS ConsigneePK,
	CASE
		WHEN RCN_Consignee.OH_Code IS NOT NULL THEN RCN_Consignee.OH_Code
		ELSE DCN_Consignee.OH_Code
	END AS ConsigneeCode,
	CASE
		WHEN RCN_BookingParty.OH_PK IS NOT NULL THEN RCN_BookingParty.OH_PK
		ELSE DCN_BookingParty.OH_PK
	END AS BookingPartyPK,
	CASE
		WHEN RCN_BookingParty.OH_Code IS NOT NULL THEN RCN_BookingParty.OH_Code
		ELSE DCN_BookingParty.OH_Code
	END AS BookingPartyCode,
	CASE
		WHEN RCN_BillToParty.OH_PK IS NOT NULL THEN RCN_BillToParty.OH_PK
		ELSE DCN_BillToParty.OH_PK
	END AS BillToPartyPK,
	CASE
		WHEN RCN_BillToParty.OH_Code IS NOT NULL THEN RCN_BillToParty.OH_Code
		ELSE DCN_BillToParty.OH_Code
	END AS BillToPartyCode,
	ConsignorPickup.OH_PK AS ConsignorPickupPK,
	ConsignorPickup.OH_Code AS ConsignorPickupCode,
	RL_PK,
	RL_Code
FROM 
	WhsItemPackageState
	LEFT JOIN WhsItemReceiveConsignment ON WRC_PK = WPS_WRC_TransitReceiveConsignment
	LEFT JOIN WhsItemDispatchConsignment ON WDC_PK = WPS_WDC_TransitDispatchConsignment
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress on OA_PK = E2_OA_Address
			JOIN OrgHeader on OH_PK = OA_OH
		WHERE E2_ParentID = WRC_PK AND E2_AddressType = 'LCE'
	) RCN_Consignor
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WDC_PK and E2_AddressType = 'LCE'
	) DCN_Consignor
	OUTER APPLY
	(
		SELECT 
			top 1 OH_PK, OH_Code
		from 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WRC_PK AND E2_AddressType = 'CED'
	) RCN_Consignee
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WDC_PK AND E2_AddressType = 'CED'
	) DCN_Consignee
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WRC_PK AND E2_AddressType = 'BKD'
	) RCN_BookingParty
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WDC_PK AND E2_AddressType = 'BKD'
	) DCN_BookingParty
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WRC_PK AND E2_AddressType = 'CRB'
	) RCN_BillToParty
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WDC_PK AND E2_AddressType = 'CRB'
	) DCN_BillToParty
	OUTER APPLY
	(
		SELECT 
			TOP 1 OH_PK, OH_Code
		FROM 
			JobDocAddress
			JOIN OrgAddress ON OA_PK = E2_OA_Address
			JOIN OrgHeader ON OH_PK = OA_OH
		WHERE E2_ParentID = WRC_PK AND E2_AddressType = 'CRG'
	) ConsignorPickup
	OUTER APPLY
	(
		SELECT
			TOP 1 RL_PK, RL_Code
		FROM
			RefUNLOCO
		WHERE RL_Code = WRC_RL_NKDestination
	) UNLOCO
WHERE WPS_WL_LastLocation IS NOT NULL AND WPS_LoadedTime IS NULL AND WPS_AdjustedOut = '' AND WPS_WW_Warehouse = {whsParameter.ParameterName}";

			void addParameters(DbCommand command)
			{
				command.AddParameter(whsParameter);
			}

			return DataRowLoader.Load(factory, sql, addParameters);
		}

		class Schema
		{
			public const string ConsignorPK = nameof(ConsignorPK);
			public const string ConsignorCode = nameof(ConsignorCode);
			public const string ConsigneePK = nameof(ConsigneePK);
			public const string ConsigneeCode = nameof(ConsigneeCode);
			public const string BookingPartyPK = nameof(BookingPartyPK);
			public const string BookingPartyCode = nameof(BookingPartyCode);
			public const string BillToPartyPK = nameof(BillToPartyPK);
			public const string BillToPartyCode = nameof(BillToPartyCode);
			public const string ConsignorPickupPK = nameof(ConsignorPickupPK);
			public const string ConsignorPickupCode = nameof(ConsignorPickupCode);
			public const string LocationIndexForSort = nameof(LocationIndexForSort);
		}

		IEnumerable<TransitCycleCountLocationFact> GetCycleCountLocationFacts(BusinessObjectFactory factory, DataRow[] locations, DataRow[] inventories, CancellationToken cancellationToken)
		{
			var cycleCountLocationCoreFacts = new Dictionary<ZGuid, TransitCycleCountLocationCoreFact>(locations.Length);
			var locationsWithoutInventory = new HashSet<ZGuid>(locations.Length);
			var organisationFacts = new Dictionary<ZGuid, OrganisationFact>();
			var unlocoFacts = new Dictionary<ZGuid, UNLOCOFact>();
			var countryFacts = new Dictionary<ZGuid, CountryFact>();

			if (locations.Length > 0)
			{
				var columns = locations[0].Table.Columns;
				var pkIndex = columns.IndexOf(WhsLocationViewSchema.PK.Name);
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
				var locationTypeCodeIndex = columns.IndexOf(WhsLocationViewSchema.WLV_LocationTypeCode.Name);
				var locationIndexForSortIndex = columns.IndexOf(Schema.LocationIndexForSort);

				foreach (var location in locations)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var locationPK = (Guid)location[pkIndex];
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

					var cycleCountLocation = new TransitCycleCountLocationCoreFact(
						locationPK,
						locationTypeCode,
						locationClass,
						areaName,
						rowName,
						column,
						level,
						tray,
						locationString,
						locationStatus,
						cycleCountTaskExists: false,
						cycleCountPathSequence,
						rowPathSequence,
						locationStringSortIndex,
						inventoryLastChangedDate,
						cycleCountLastPerformedDate);

					cycleCountLocationCoreFacts.Add(locationPK, cycleCountLocation);
					locationsWithoutInventory.Add(locationPK);
				}
			}

			if (inventories.Length > 0)
			{
				var columns = inventories[0].Table.Columns;
				var locationPKIndex = columns.IndexOf(WhsItemPackageStateSchema.WPS_WL_LastLocation.Name);

				var consignorPKIndex = columns.IndexOf(Schema.ConsignorPK);
				var consignorCodeIndex = columns.IndexOf(Schema.ConsignorCode);
				var consigneePKIndex = columns.IndexOf(Schema.ConsigneePK);
				var consigneeCodeIndex = columns.IndexOf(Schema.ConsigneeCode);
				var bookingPartyPKIndex = columns.IndexOf(Schema.BookingPartyPK);
				var bookingPartyCodeIndex = columns.IndexOf(Schema.BookingPartyCode);
				var billToPartyPKIndex = columns.IndexOf(Schema.BillToPartyPK);
				var billToPartyCodeIndex = columns.IndexOf(Schema.BillToPartyCode);
				var consignorPickupPKIndex = columns.IndexOf(Schema.ConsignorPickupPK);
				var consignorPickupCodeIndex = columns.IndexOf(Schema.ConsignorPickupCode);
				var unlocoPKIndex = columns.IndexOf(RefUNLOCOSchema.PK.Name);
				var unlocoCodeIndex = columns.IndexOf(RefUNLOCOSchema.RL_Code.Name);

				foreach (var inventory in inventories)
				{
					cancellationToken.ThrowIfCancellationRequested();

					OrganisationFact consignorFact = null;
					OrganisationFact consigneeFact = null;
					OrganisationFact bookingPartyFact = null;
					OrganisationFact billToPartyFact = null;
					OrganisationFact consignorPickupFact = null;
					CountryFact countryFact = null;
					UNLOCOFact unlocoFact = null;

					var consignorPK = DataRowLoader.GetNullableGuid(inventory[consignorPKIndex]);
					var consigneePK = DataRowLoader.GetNullableGuid(inventory[consigneePKIndex]);
					var bookingPartyPK = DataRowLoader.GetNullableGuid(inventory[bookingPartyPKIndex]);
					var billToPartyPK = DataRowLoader.GetNullableGuid(inventory[billToPartyPKIndex]);
					var consignorPickupPK = DataRowLoader.GetNullableGuid(inventory[consignorPickupPKIndex]);
					var unlocoPK = DataRowLoader.GetNullableGuid(inventory[unlocoPKIndex]);

					if (consignorPK != null)
					{
						if (!organisationFacts.TryGetValue((Guid)consignorPK, out consignorFact))
						{
							var consignorCode = (string)inventory[consignorCodeIndex];

							consignorFact = new OrganisationFact((Guid)consignorPK, consignorCode, false, false);
							organisationFacts.Add((Guid)consignorPK, consignorFact);
						}
					}

					if (consigneePK != null)
					{
						if (!organisationFacts.TryGetValue((Guid)consigneePK, out consigneeFact))
						{
							var consigneeCode = (string)inventory[consigneeCodeIndex];

							consigneeFact = new OrganisationFact((Guid)consigneePK, consigneeCode, false, false);
							organisationFacts.Add((Guid)consigneePK, consigneeFact);
						}
					}

					if (bookingPartyPK != null)
					{
						if (!organisationFacts.TryGetValue((Guid)bookingPartyPK, out bookingPartyFact))
						{
							var bookingPartyCode = (string)inventory[bookingPartyCodeIndex];

							bookingPartyFact = new OrganisationFact((Guid)bookingPartyPK, bookingPartyCode, false, false);
							organisationFacts.Add((Guid)bookingPartyPK, bookingPartyFact);
						}
					}

					if (billToPartyPK != null)
					{
						if (!organisationFacts.TryGetValue((Guid)billToPartyPK, out billToPartyFact))
						{
							var billToPartyCode = (string)inventory[billToPartyCodeIndex];

							billToPartyFact = new OrganisationFact((Guid)billToPartyPK, billToPartyCode, false, false);
							organisationFacts.Add((Guid)billToPartyPK, billToPartyFact);
						}
					}

					if (consignorPickupPK != null)
					{
						if (!organisationFacts.TryGetValue((Guid)consignorPickupPK, out consignorPickupFact))
						{
							var consignorPickupCode = (string)inventory[consignorPickupCodeIndex];

							consignorPickupFact = new OrganisationFact((Guid)consignorPickupPK, consignorPickupCode, false, false);
							organisationFacts.Add((Guid)consignorPickupPK, consignorPickupFact);
						}
					}

					if (unlocoPK != null)
					{
						if (!unlocoFacts.TryGetValue((Guid)unlocoPK, out unlocoFact))
						{
							var unlocoCode = (string)inventory[unlocoCodeIndex];

							var refUNLOCO = factory.New<RefUNLOCO>();
							refUNLOCO.RL_Code = unlocoCode;

							var countryPK = refUNLOCO.Country.PK;
							if (!countryFacts.TryGetValue(countryPK, out countryFact))
							{
								countryFact = new CountryFact(refUNLOCO.Country);
								unlocoFact = new UNLOCOFact(refUNLOCO, countryFact);
								countryFacts.Add(countryPK, countryFact);
								unlocoFacts.Add((Guid)unlocoPK, unlocoFact);
							}
							else
							{
								unlocoFact = new UNLOCOFact(refUNLOCO, countryFact);
								unlocoFacts.Add((Guid)unlocoPK, unlocoFact);
							}
						}
					}

					var locationPK = (Guid)inventory[locationPKIndex];

					if (cycleCountLocationCoreFacts.TryGetValue(locationPK, out var relatedLocationFact))
					{
						var cycleCountLocationFact = new TransitCycleCountLocationFact(relatedLocationFact, consignorFact, consigneeFact, bookingPartyFact, billToPartyFact, unlocoFact, consignorPickupFact);

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
					var cycleCountLocationFact = new TransitCycleCountLocationFact(locationFact, null, null, null, null, null, null);

					yield return cycleCountLocationFact;
				}
			}
		}
	}
}
