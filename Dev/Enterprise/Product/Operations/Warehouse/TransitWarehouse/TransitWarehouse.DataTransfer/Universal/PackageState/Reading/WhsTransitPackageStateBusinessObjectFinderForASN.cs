using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitPackageStateBusinessObjectFinderForASN : IWhsTransitPackageStateBusinessObjectFinder
	{
		public enum FinderType
		{
			ByASN,
			Packages,
			ByRCN,
			ByGate,
			ForLinkToVehicle
		}

		public WhsTransitPackageStateBusinessObjectFinderForASN(FinderType type, UniversalObjectFactory factory)
		{
			this.type = type;
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		public WhsTransitPackageStateBusinessObjectFinderForASN(FinderType type, UniversalObjectFactory factory, WhsItemReceiveASN asn = null, WhsItemReceiveConsignment rcn = null, IEnumerable<ZGuid> packagePKs = null)
		{
			this.asn = asn;
			this.type = type;
			this.rcn = rcn;
			this.packagePKs = packagePKs;
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		public WhsTransitPackageStateBusinessObjectFinderForASN(UniversalShipment topLevelDO, UniversalObjectFactory factory, WhsItemPackageState[] packageStatesFromJob, List<ContainerByASNDTO> containerByASNDTOs, WhsItemReceiveASN vehicleASN, ZString? masterBill, ZGuid? latestASNPK = null, FinderType type = FinderType.ByGate)
		{
			this.type = type;
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.packageStatesFromJob = packageStatesFromJob;
			this.containerByASNDTOs = containerByASNDTOs;
			this.topLevelDO = topLevelDO;
			this.vehicleASN = vehicleASN;
			this.masterBill = masterBill;
			this.latestASNPK = latestASNPK;
		}

		public WhsTransitPackageStateBusinessObjectFinderForASN(UniversalShipment topLevelDO, UniversalObjectFactory factory, List<ContainerByASNDTO> containerByASNDTOs, FinderType type = FinderType.ForLinkToVehicle)
		{
			this.type = type;
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.topLevelDO = topLevelDO;
			this.containerByASNDTOs = containerByASNDTOs;
		}

		readonly UniversalObjectFactory factory;
		readonly FinderType type;

		readonly WhsItemReceiveConsignment rcn;
		readonly IEnumerable<ZGuid> packagePKs;
		readonly WhsItemReceiveASN asn;
		readonly UniversalShipment topLevelDO;
		readonly WhsItemPackageState[] packageStatesFromJob;
		readonly WhsItemReceiveASN vehicleASN;
		readonly ZString? masterBill;
		readonly ZGuid? latestASNPK;
		readonly List<ContainerByASNDTO> containerByASNDTOs;

		public IEnumerable<WhsItemPackageState> Find()
		{
			switch (type)
			{
				case FinderType.ByASN:
					return FindByASN();
				case FinderType.Packages:
					return FindByPackages();
				case FinderType.ByRCN:
					return FindByRCN();
				case FinderType.ByGate:
					return FindByGate();
				case FinderType.ForLinkToVehicle:
					return FindForLinkToVehicle();
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		IEnumerable<WhsItemPackageState> FindByASN()
		{
			var packageStates = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRP_ReceiveExpectedPacking, asn.PK));
			return packageStates;
		}

		IEnumerable<WhsItemPackageState> FindByPackages()
		{
			var packageStatesQuery = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packagePKs);
			var packageStates = factory.BOFactory.Load<WhsItemPackageState>(packageStatesQuery).ToList();
			return packageStates;
		}

		IEnumerable<WhsItemPackageState> FindByRCN()
		{
			var packageStates = factory.BOFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, rcn.PK)).ToList();
			return packageStates;
		}

		IEnumerable<WhsItemPackageState> FindByGate()
		{
			var result = new List<WhsItemPackageState>();
			var containerNumbersInDO = topLevelDO.GetContainerNumbers();

			var isLoosePackagesBooking = containerNumbersInDO.Count == 0;
			if (isLoosePackagesBooking)
			{
				result = packageStatesFromJob.ToList();

				var hasContainerRTUs = containerByASNDTOs != null && containerByASNDTOs.Count > 0;
				if (hasContainerRTUs)
				{
					var filteredcontainerByASNDTOs = containerByASNDTOs.Where(containerByASNDTO => !containerByASNDTO.Containers.Any() || containerByASNDTO.Containers.All(containerDTO => containerDTO.RTU.WRH_UnitType == TransportUnitTypes.Vehicle));
					if (!filteredcontainerByASNDTOs.Any())
					{
						throw new DataObjectReadFailureException(Res.GetString("0ba57dfb-7909-49d6-b1f9-8d7e57476792", "Gate Booking has no container and failed to find a matching ASN without container."));
					}

					var filteredASNPKs = filteredcontainerByASNDTOs.Select(containerByASNDTO => containerByASNDTO.ASNPK).ToHashSet();
					filteredASNPKs.Where(asnPK => asnPK != vehicleASN.PK).ForEach(asnPK => ObjectFactory.Get<TransitDataObjectReaderHandlerManager>().AddAffectedASNPK(asnPK));

					result = packageStatesFromJob.Where(ps => filteredASNPKs.Contains(ps.WPS_WRP_ReceiveExpectedPacking)).ToList();
				}
			}
			else
			{
				if (latestASNPK != null && masterBill.HasValue)
				{
					result = packageStatesFromJob.Where(ps => ps.WPS_WRP_ReceiveExpectedPacking == latestASNPK).ToList();
				}
				else
				{
					result = new List<WhsItemPackageState>(packageStatesFromJob);
				}
			}

			return result;
		}

		IEnumerable<WhsItemPackageState> FindForLinkToVehicle()
		{
			var result = new List<WhsItemPackageState>();
			var filteredASNPKs = new HashSet<ZGuid>();
			foreach (var containerByASNDTO in containerByASNDTOs)
			{
				var asnPK = containerByASNDTO.ASNPK;
				var containerDTOs = containerByASNDTO.Containers;

				var containerNumbersInDO = topLevelDO.GetContainerNumbers();
				var isAnyMatchingContainerRTU = false;
				foreach (var containerDTO in containerDTOs)
				{
					var unitType = containerDTO.RTU.WRH_UnitType;
					var containerNumber = containerDTO.RTU.WRH_VehicleReference;
					var isCurrentContainerRTUMatch = containerNumbersInDO.Contains(containerNumber) && (unitType == TransportUnitTypes.ULD || unitType == TransportUnitTypes.Container);
					if (isCurrentContainerRTUMatch)
					{
						var rtuPK = containerDTO.RTU.PK;
						result.Add(containerDTO.PackageState);
						if (!isAnyMatchingContainerRTU)
						{
							isAnyMatchingContainerRTU = isCurrentContainerRTUMatch;
						}
					}
				}
				if (isAnyMatchingContainerRTU && !filteredASNPKs.Contains(asnPK))
				{
					filteredASNPKs.Add(asnPK);
				}
			}

			if (!filteredASNPKs.Any())
			{
				throw new DataObjectReadFailureException(Res.GetString("c389e776-ddaa-4070-a652-6c11cc78b0c3", "Gate Booking has a container and failed to find a matching ASN with the same container."));
			}

			return result;
		}

		public WhsItemPackageStateDTO Find(PackingLine packingLine)
		{
			throw new NotImplementedException();
		}
	}
}
