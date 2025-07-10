using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class PackageStateStatusHelper
	{
		public static void UpdatePackageStatesStatus(IEnumerable<WhsItemPackageState> packageStates, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var handlingUnitsRequireRecalculateStatus = new HashSet<IColumnIndexer>();
			foreach (var packageState in packageStates)
			{
				var isPackageStatusUpdated = UpdatePackageStateStatus(packageState, factory, logger);

				if (isPackageStatusUpdated)
				{
					var handlingUnits = PackageStateColumnIndexerHelper.GetAllHandlingUnits(packageState, factory);
					foreach (var handlingUnit in handlingUnits)
					{
						handlingUnitsRequireRecalculateStatus.Add(handlingUnit);
					}
				}
			}

			foreach (var handlingUnit in handlingUnitsRequireRecalculateStatus)
			{
				UpdateHandlingUnitPackageStatus(handlingUnit, factory, logger);
			}
		}

		#region UpdatePackageStateStatus

		// Only consider packages status should be affected by stopping the load list
		static bool UpdatePackageStateStatus(WhsItemPackageState packageState, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var originalPackageStatus = packageState.WPS_Status;
			var packageStatus = GetUpdatedPackageStatus(packageState, factory, originalPackageStatus);

			packageState.SetValue(WhsItemPackageStateSchema.WPS_Status, packageStatus, logger);
			packageState.HasChanges = true;
			return packageStatus != originalPackageStatus;
		}

		static string GetUpdatedPackageStatus(WhsItemPackageState packageState, UniversalObjectFactory factory, string originalPackageStatus)
		{
			string packageStatus;
			if (PackageStateColumnIndexerHelper.GetIncompleteTransferLine(packageState, factory) != null)
			{
				packageStatus = originalPackageStatus;
			}
			else if (IsPackageInArrivedStatus(packageState, factory))
			{
				packageStatus = TransitWarehouseStatuses.Codes.Arrived;
			}
			else
			{
				packageStatus = TransitWarehouseStatuses.Codes.Putaway;
			}

			return packageStatus;
		}

		static bool IsPackageInArrivedStatus(WhsItemPackageState packageState, UniversalObjectFactory factory)
		{
			bool result = false;
			if (packageState.WPS_WRH_TransitReceiveHeader != ZGuid.Empty && packageState.WPS_WL_LastLocation != ZGuid.Empty)
			{
				var rtu = PackageStateColumnIndexerHelper.GetReceiveTransportationUnit(packageState, factory);
				result = packageState.WPS_WL_LastLocation == rtu.WRH_WL_StagingLocation;
			}

			return result;
		}

		#endregion

		#region UpdateHandlingUnitPackageStatus

		static void UpdateHandlingUnitPackageStatus(IColumnIndexer handlingUnit, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var handlingUnitBO = factory.BOFactory.Load<WhsItemPackageState>(handlingUnit.GetValue(WhsItemPackageStateSchema.PK));
			var childPackagesExcludeHandlingUnits = PackageStateColumnIndexerHelper.GetAllHandlingUnitChildPackages(handlingUnitBO, factory)
				.Where(h => !h.WPS_IsHandlingUnit).ToArray();

			string handlingUnitStatus;
			if (IsHandlingUnitInPickedOrCommittedToTransferStatus(childPackagesExcludeHandlingUnits))
			{
				handlingUnitStatus = handlingUnitBO.WPS_Status;
			}
			else if (IsHandlingUnitInArrivedStatus(childPackagesExcludeHandlingUnits))
			{
				handlingUnitStatus = TransitWarehouseStatuses.Codes.Arrived;
			}
			else
			{
				handlingUnitStatus = TransitWarehouseStatuses.Codes.Putaway;
			}

			handlingUnitBO.SetValue(WhsItemPackageStateSchema.WPS_Status, handlingUnitStatus, logger);
			handlingUnitBO.HasChanges = true;
		}

		static bool IsHandlingUnitInArrivedStatus(WhsItemPackageState[] childPackagesExcludeHandlingUnits)
		{
			return childPackagesExcludeHandlingUnits.All(c => c.WPS_Status == TransitWarehouseStatuses.Codes.Arrived);
		}

		static bool IsHandlingUnitInPickedOrCommittedToTransferStatus(WhsItemPackageState[] childPackagesExcludeHandlingUnits)
		{
			return childPackagesExcludeHandlingUnits.Any(c => c.WPS_Status == TransitWarehouseStatuses.Codes.Picked || c.WPS_Status == TransitWarehouseStatuses.Codes.Committed);
		}

		#endregion
	}
}
