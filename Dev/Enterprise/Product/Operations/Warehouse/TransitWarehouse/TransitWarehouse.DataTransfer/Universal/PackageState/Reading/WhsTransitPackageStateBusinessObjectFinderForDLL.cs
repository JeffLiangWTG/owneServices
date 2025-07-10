using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitPackageStateBusinessObjectFinderForDLL : IWhsTransitPackageStateBusinessObjectFinder
	{
		public enum FindOption
		{
			ByDLLs,
			ByDCNs,
			ForVehicle,
			ForContainer,
			ForGate,
			ForLinkToVehicle
		}

		public WhsTransitPackageStateBusinessObjectFinderForDLL(UniversalObjectFactory factory, IReadOnlyCollection<WhsItemDispatchConsignment> dcns, FindOption findOption = FindOption.ByDCNs) : this(factory, findOption)
		{
			this.dcns = Argument.NotNull(dcns, nameof(dcns));
		}

		public WhsTransitPackageStateBusinessObjectFinderForDLL(UniversalObjectFactory factory, IReadOnlyCollection<WhsItemDispatchLoadList> dlls, FindOption findOption = FindOption.ByDLLs) : this(factory, findOption)
		{
			this.dlls = Argument.NotNull(dlls, nameof(dlls));
		}

		public WhsTransitPackageStateBusinessObjectFinderForDLL(UniversalObjectFactory factory, IReadOnlyCollection<WhsItemDispatchConsignment> dcns, PackingLine[] packingLines, FindOption findOption = FindOption.ForContainer) : this(factory, dcns, findOption)
		{
			this.packingLines = Argument.NotNull(packingLines, nameof(packingLines));
		}

		public WhsTransitPackageStateBusinessObjectFinderForDLL(UniversalShipment dataObject, UniversalObjectFactory factory, WhsItemPackageState[] pkgStatesToLoad, List<ContainerByDLLDTO> containerByDLLDTOs, ZString? masterBill, ZGuid? latestDllPK = null, FindOption findOption = FindOption.ForGate) : this(factory, findOption)
		{
			this.dataObject = Argument.NotNull(dataObject, nameof(dataObject));
			this.pkgStatesToLoad = pkgStatesToLoad;
			this.containerByDLLDTOs = containerByDLLDTOs;
			this.masterBill = masterBill;
			this.latestDllPK = latestDllPK;
		}

		public WhsTransitPackageStateBusinessObjectFinderForDLL(UniversalShipment dataObject, UniversalObjectFactory factory, List<ContainerByDLLDTO> containerByDLLDTOs, HashSet<ZGuid> filteredDLLPKs, FindOption findOption = FindOption.ForLinkToVehicle) : this(factory, findOption)
		{
			this.dataObject = Argument.NotNull(dataObject, nameof(dataObject));
			this.containerByDLLDTOs = containerByDLLDTOs;
			this.filteredDLLPKs = filteredDLLPKs;
		}

		WhsTransitPackageStateBusinessObjectFinderForDLL(UniversalObjectFactory factory, FindOption findOption = FindOption.ByDCNs)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.findOption = findOption;
		}

		readonly UniversalObjectFactory factory;
		readonly IReadOnlyCollection<WhsItemDispatchLoadList> dlls;
		readonly PackingLine[] packingLines;
		readonly IReadOnlyCollection<WhsItemDispatchConsignment> dcns;
		readonly FindOption findOption;
		readonly UniversalShipment dataObject;
		readonly WhsItemPackageState[] pkgStatesToLoad;
		readonly List<ContainerByDLLDTO> containerByDLLDTOs;
		readonly ZString? masterBill;
		readonly ZGuid? latestDllPK;
		readonly HashSet<ZGuid> filteredDLLPKs;

		IEnumerable<WhsItemPackageState> packageStatesCache;

		public IEnumerable<WhsItemPackageState> Find()
		{
			switch (findOption)
			{
				case FindOption.ByDLLs:
					return packageStatesCache ?? FindByDLLs();
				case FindOption.ByDCNs:
					return packageStatesCache ?? FindByDCNs();
				case FindOption.ForContainer:
					return packageStatesCache ?? FindPackageStatesForContainers();
				case FindOption.ForGate:
					return packageStatesCache ?? FindPackageStatesForForGate();
				case FindOption.ForLinkToVehicle:
					return packageStatesCache ?? FindPackageStatesForLinkToVehicle();
				default:
					throw new ArgumentOutOfRangeException(nameof(findOption), findOption, null);
			}
		}

		IEnumerable<WhsItemPackageState> FindByDLLs()
		{
			var dllPKs = dlls.Select(c => c.PK).ToArray();
			packageStatesCache = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WDL_LoadList, dllPKs));

			return packageStatesCache;
		}

		IEnumerable<WhsItemPackageState> FindByDCNs()
		{
			var consignmentPKs = dcns.Select(c => c.PK).ToArray();
			packageStatesCache = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, consignmentPKs));

			return packageStatesCache;
		}

		IEnumerable<WhsItemPackageState> FindPackageStatesForContainers()
		{
			var pkgStates = new List<WhsItemPackageState>();

			var packageIDs = GetChildPackageIDIfHandlingUnit(packingLines.Select(p => p.ReferenceNumber.Value).ToArray());
			foreach (var packageID in packageIDs)
			{
				var packageState = FindByDCNs().FirstOrDefault(p => p.Package.KP_PackageID == packageID);
				pkgStates.Add(packageState);
			}

			packageStatesCache = pkgStates;

			return pkgStates;
		}

		IEnumerable<WhsItemPackageState> FindPackageStatesForForGate()
		{
			var result = pkgStatesToLoad;
			var containerNumbersInDO = dataObject.GetContainerNumbers();

			var isLoosePackagesBooking = containerNumbersInDO.Count == 0;
			if (isLoosePackagesBooking)
			{
				var pkgStateHasDLL = containerByDLLDTOs != null && containerByDLLDTOs.Count > 0;

				if (pkgStateHasDLL)
				{
					var filteredContainerByDLLDTO = containerByDLLDTOs.Where(c => !c.containerDTOs.Any() || c.containerDTOs.All(container => container.DTU.WDH_UnitType == TransportUnitTypes.Vehicle));
					if (!filteredContainerByDLLDTO.Any())
					{
						throw new DataObjectReadFailureException(Res.GetString("14357c2c-edd5-4e5c-9fcf-bb1dc38b09cb", "Gate Booking has no container and failed to find a matching Dispatch Load List without a container."));
					}

					var filteredDllPks = filteredContainerByDLLDTO.Select(fc => fc.dllPK).ToHashSet();
					var filteredPkgStates = pkgStatesToLoad.Where(ps => filteredDllPks.Contains(ps.WPS_WDL_LoadList)).ToArray();
					result = filteredPkgStates;
				}
			}
			else
			{
				if (latestDllPK != null && masterBill.HasValue)
				{
					result = pkgStatesToLoad.Where(ps => ps.GetValue(WhsItemPackageStateSchema.WPS_WDL_LoadList) == latestDllPK).ToArray();
				}
			}

			packageStatesCache = result;
			return result;
		}

		IEnumerable<WhsItemPackageState> FindPackageStatesForLinkToVehicle()
		{
			var result = new List<WhsItemPackageState>();
			foreach (var containerByDLLDTO in containerByDLLDTOs)
			{
				var dllPK = containerByDLLDTO.dllPK;
				var containerDTOs = containerByDLLDTO.containerDTOs;
				var containerNumbersInDO = dataObject.GetContainerNumbers();
				var isAnyMatchingContainerDTU = false;
				foreach (var containerDTO in containerDTOs)
				{
					var isCurrentContainerDTUMatch = containerDTO.DTU != null 
						&& containerNumbersInDO.Contains(containerDTO.DTU.WDH_VehicleReference) 
						&& (containerDTO.DTU.WDH_UnitType == TransportUnitTypes.ULD || containerDTO.DTU.WDH_UnitType == TransportUnitTypes.Container);
					if (isCurrentContainerDTUMatch)
					{
						var dtuPK = containerDTO.DTU.PK;
						result.Add(containerDTO.PackageState);
						if (!isAnyMatchingContainerDTU)
						{
							isAnyMatchingContainerDTU = isCurrentContainerDTUMatch;
						}
					}
				}
				if (isAnyMatchingContainerDTU && !filteredDLLPKs.Contains(dllPK))
				{
					filteredDLLPKs.Add(dllPK);
				}
			}

			if (!filteredDLLPKs.Any())
			{
				throw new DataObjectReadFailureException(Res.GetString("e401c998-46f5-42c9-8095-3b2a81986f2c", "Gate Booking has a container and failed to find a matching Dispatch Load List with the same container."));
			}

			packageStatesCache = result;
			return result;
		}

		string[] GetChildPackageIDIfHandlingUnit(IReadOnlyCollection<ZString> referenceNumbers)
		{
			var sqlParams = new ZSqlParameterCollection();
			int packageCounter = 0;
			var packageParamsTextBuilder = new ZStringBuilder();

			foreach (var packageID in referenceNumbers.Where(r => !r.IsEmpty))
			{
				var packageIDParam = FormattableString.Invariant($"@PackageID{packageCounter}"); // SQL Parameters - not localisable
				sqlParams.Add(packageIDParam, packageID, PkgPackageHeaderSchema.KPH_PackageID);
				packageParamsTextBuilder.Append(packageIDParam);
				packageCounter++;
			}

			var packageParams = packageParamsTextBuilder.ToStringWithDelimiterBetweenAppends(", ");

			var sqlText = FormattableString.Invariant(
$@"
SELECT 
	CASE WHEN ChildPackageHeader.KPH_PackageID IS NULL 
	THEN ParentPackageHeader.KPH_PackageID
	ELSE ChildPackageHeader.KPH_PackageID
	END AS KPH_PackageID
FROM
	dbo.PkgPackageHeader ParentPackageHeader
	JOIN dbo.PkgPackage ParentPackage ON ParentPackage.KP_KPH_PackageHeader = ParentPackageHeader.KPH_PK
	LEFT JOIN dbo.PkgPackageHandlingUnitDivot ON ParentPackage.KP_PK = KPD_KP_HandlingUnit
	LEFT JOIN dbo.PkgPackage ChildPackage ON ChildPackage.KP_PK = KPD_KP_Package
	LEFT JOIN dbo.PkgPackageHeader ChildPackageHeader ON ChildPackage.KP_KPH_PackageHeader = ChildPackageHeader.KPH_PK
WHERE
	ParentPackageHeader.KPH_PackageID IN ({packageParams})
	AND KPD_UnpackedTime IS NULL
");

			var childPackageIDs = new DynamicBusinessObjectCollection(factory.BOFactory);
			childPackageIDs.Load(sqlText, sqlParams);

			return childPackageIDs.Select(p => p[PkgPackageHeaderSchema.KPH_PackageID].ToString()).Distinct().ToArray();
		}

		public WhsItemPackageStateDTO Find(PackingLine packingLine)
		{
			return null;
		}
	}
}
