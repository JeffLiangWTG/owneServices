using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemDispatchTransportationUnitDataObjectReaderForContainer : DataObjectReader<Container, WhsItemDispatchTransportationUnit>
	{
		public WhsItemDispatchTransportationUnitDataObjectReaderForContainer(IColumnIndexer loadList, IColumnIndexer warehouseFromConsol, Container dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, List<WhsItemDispatchTransportationUnit> listOfContainersAlreadyMatched = null, bool shouldCreateContainerPackageState = true)
			: base(dataObject, logger, factory)
		{
			LoadList = Argument.NotNull(loadList, nameof(loadList));
			WarehouseFromConsol = Argument.NotNull(warehouseFromConsol, nameof(warehouseFromConsol));
			ListOfContainersAlreadyMatched = listOfContainersAlreadyMatched ?? new List<WhsItemDispatchTransportationUnit>();
			ShouldCreateContainerPackageState = shouldCreateContainerPackageState;
		}

		protected override string GetBusinessObjectHumanReadableName(WhsItemDispatchTransportationUnit dtu) => dtu == null ? Res.GetString("3a2e8810-049b-4d1e-9a99-d1ccb9d80cbb", "Dispatch Transportation Unit") : dtu.HumanReadableName.ToString();

		readonly IColumnIndexer LoadList;
		readonly IColumnIndexer WarehouseFromConsol;
		readonly List<WhsItemDispatchTransportationUnit> ListOfContainersAlreadyMatched;
		readonly bool ShouldCreateContainerPackageState;

		#region GetExistingBusinessObject

		protected override WhsItemDispatchTransportationUnit GetExistingBusinessObject()
		{
			return WhsTransitDispatchTransportationUnitMatchingHelper.GetExistingContainerDTU(factory, dataObject, LoadList, ListOfContainersAlreadyMatched, WarehouseFromConsol);
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsItemDispatchTransportationUnit unit)
		{
			var headerRow = GetColumnIndexer(unit);
			if (!HasLoadList(unit))
			{
				var pivot = factory.New<WhsItemDispatchLoadListDTUPivot>();
				pivot.WLD_WDL_TransitDispatchLoadList = LoadList.GetValue(WhsItemDispatchLoadListSchema.PK);
				pivot.WLD_WDH_TransitDispatchTransportationUnit = unit.PK;
			}

			var warehousePK = WarehouseFromConsol.GetValue(WhsWarehouseSchema.PK);
			SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse, warehousePK);
			if (IsNewBO)
			{
				SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_ReferenceNumber, NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseDispatchID));
			}
			if (IsNewBO || dataObject.ContainerNumber.HasValue && !string.IsNullOrEmpty(dataObject.ContainerNumber.Value))
			{
				SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference, dataObject.ContainerNumber);
			}
			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var bookingParty = TransitUniversalHelper.GetBookingParty(factory, logger) as OrgHeader;
			OrgAddressImportHelper.PopulateTransportOrg(consolDO, null, bookingParty, unit, logger, factory);
			OrgAddressImportHelper.PopulateBillToPartyOrg(consolDO, unit, logger, factory);

			var unitType = TransitUniversalHelper.GetUnitTypeFromContainerDataObject(factory, dataObject);
			if (string.IsNullOrEmpty(unitType))
			{
				var containerNumber = dataObject.ContainerNumber;
				if (!string.IsNullOrEmpty(containerNumber))
				{
					throw new DataObjectReadFailureException(Res.GetString("31bbdae4-6304-4233-b52f-96b4a40ad0f5", "Container {0} does not have a supported Container Type.", containerNumber));
				}
				else
				{
					throw new DataObjectReadFailureException(Res.GetString("b30621ad-8406-475e-9eb0-6aa2ad42d71c", "Container does not have a supported Container Type."));
				}
			}

			var matchingContainer = FindMatchingULDWithContainerInTransitWarehouseInventory(unit, unitType, warehousePK);
			SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_UnitType, unitType);
			if (ShouldCreateContainerPackageState)
			{
				unit.CreateOrUpdatePackageExtension(matchingContainer.Package, logger);
				TransitUniversalHelper.CreatePackageStateForContainerizedTransportationUnit(factory, matchingContainer.Package, unit, warehousePK, unitType);
			}
		}

		PkgPackageContainer FindMatchingULDWithContainerInTransitWarehouseInventory(WhsItemDispatchTransportationUnit unit, string rtuUnitType, ZGuid warehousePK)
		{
			PkgPackageContainer matchingContainer = null;
			var containerNumber = dataObject.ContainerNumber.GetValueOrDefault();
			var sqlParams = new ZSqlParameterCollection();

			if (!string.IsNullOrEmpty(containerNumber))
			{
				var sqlText =
	@"
SELECT TOP 2 WRH_PK
FROM dbo.WhsItemReceiveTransportationUnit
JOIN dbo.PkgPackageExtension AS RTUExtension ON
	RTUExtension.KPN_ParentID = WRH_PK AND RTUExtension.KPN_IsActive = 1
JOIN dbo.WhsItemPackageState ON
	WPS_KP_Package = RTUExtension.KPN_KP_Package
LEFT JOIN dbo.PkgPackageExtension AS DTUExtension ON
	DTUExtension.KPN_KP_Package = RTUExtension.KPN_KP_Package AND DTUExtension.KPN_ParentTableCode = 'WDH' AND DTUExtension.KPN_IsActive = 1
WHERE WRH_WW_Warehouse = @WarehousePK AND (DTUExtension.KPN_ParentID = @DTUParentPK OR WPS_Status <> 'ADJ' AND WPS_UnloadedTime IS NOT NULL AND WPS_LoadedTime IS NULL AND DTUExtension.KPN_PK IS NULL AND WRH_VehicleReference = @ContainerNumber AND WRH_UnitType = @UnitType)
ORDER BY WRH_SystemCreateTimeUtc DESC
";
				sqlParams.Add("@WarehousePK", warehousePK, WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse);
				sqlParams.Add("@ContainerNumber", containerNumber, WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference);
				sqlParams.Add("@UnitType", rtuUnitType, WhsItemReceiveTransportationUnitSchema.WRH_UnitType);
				sqlParams.Add("@DTUParentPK", unit.PK, WhsItemDispatchTransportationUnitSchema.PK);

				var sqlResults = new DynamicBusinessObjectCollection(factory.BOFactory);
				sqlResults.Load(sqlText, sqlParams);
				if (sqlResults?.Count == 1)
				{
					var rtuPK = sqlResults?.First()?[WhsItemReceiveTransportationUnitSchema.PK];
					var matchingULDInWarehouse = factory.Load<WhsItemReceiveTransportationUnit>((ZGuid)rtuPK);
					matchingContainer = matchingULDInWarehouse?.Container;
				}
				else if (sqlResults?.Count > 1)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("9031a039-5c6a-4bd1-b83e-03454af5ac8b", "Multiple open containers found with reference {0} – could not match – created new TDU Ref {1}", containerNumber, unit.WDH_ReferenceNumber));
				}
			}

			if (matchingContainer == null)
			{
				var originalContainerType = unit.Container?.ContainerType;
				var packageContainerReader = new PkgPackageContainerDataObjectReader(dataObject, logger, factory, unit.PackageJob.Packages, unit.Container?.Package);
				matchingContainer = packageContainerReader.ReadIntoBusinessObject().Container;

				var containerType = matchingContainer?.ContainerType;
				if (originalContainerType != null && matchingContainer.K0_RC_ContainerType != originalContainerType.PK)
				{
					var links = UniversalJobLinkHelper.GetMatchingJobLinkEntities(unit);
					if (links.Any(l => l.UCL_SourceType == "GateMovementBooking"))
					{
						var dtuEventReference = WhsTransitLogHelper.GetEventReferenceString(
													new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "ContainerType"),
													new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, containerType.RC_Code),
													new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalContainerType.RC_Code));

						WhsTransitLogHelper.AddStmALogToBizoObject(unit, AutoEvents.ContainerTypeUpdated, dtuEventReference);

						logger.Log(LogType.Warning, $"DTU container type updated from: {originalContainerType.RC_Code} to {containerType.RC_Code}");
					}
				}
			}
			return matchingContainer;
		}

		bool HasLoadList(WhsItemDispatchTransportationUnit unit)
		{
			var query = new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, unit.PK);
			query.AddToFilter(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, LoadList.GetValue(WhsItemDispatchLoadListSchema.PK));

			return factory.LoadTop1<WhsItemDispatchLoadListDTUPivot>(query) != null;
		}

		#endregion
	}
}
