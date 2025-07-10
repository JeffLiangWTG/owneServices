using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class FreightRatingHelper : IFreightRatingHelper
	{
		#region Calculate Freight Mode

		public static FreightMode CalculateFreightMode(ZString transportMode, ZString containerMode, bool isContainerized)
			=> CalculateFreightMode(transportMode, containerMode, () => isContainerized ? FreightMode.Containerised : FreightMode.NonContainerised);

		public static FreightMode CalculateFreightMode(ZString transportMode, ZString containerMode, RateableMeasureSet measures)
			=> CalculateFreightMode(transportMode, containerMode, () => CalculateContainerFreightMode(measures));

		public static FreightMode CalculateFreightMode(ZString transportMode, ZString containerMode)
			=> CalculateFreightMode(transportMode, containerMode, (RateableMeasureSet)null);

		public static FreightMode CalculateFreightModeFromTransportMode(ZString transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Air:
				case Constants.TransportModes.AirSea:
					return FreightMode.AIR;

				case Constants.TransportModes.Sea:
				case Constants.TransportModes.SeaAir:
					return FreightMode.SEA;

				case Constants.TransportModes.Rail:
					return FreightMode.RAI;

				case Constants.TransportModes.Road:
					return FreightMode.ROA;

				case Constants.TransportModes.Mail:
					return FreightMode.MAI;

				case Constants.TransportModes.Courier:
					return FreightMode.COU;

				default:
					return FreightMode.UKN;
			}
		}

		static FreightMode CalculateFreightMode(ZString transportMode, ZString containerMode, Func<FreightMode> fallbackContainerFreightModeGetter)
		{
			var result = CalculateFreightModeFromTransportMode(transportMode);

			if (transportMode == Constants.TransportModes.Mail)
			{
				// Preserving an existing logic which doesn't add additional freight modes for Mail transport modes.
				return result;
			}

			if (transportMode == Constants.TransportModes.Courier)
			{
				switch (containerMode)
				{
					case "":
					case Constants.ContainerModes.OnBoardCourier:
						return FreightMode.OBC;
					case Constants.ContainerModes.Unaccompanied:
						return FreightMode.UNA;
				}

				return result;
			}

			if (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.Value)
			{
				switch (containerMode)
				{
					case Constants.ContainerModes.RollOnRollOff:
						return FreightMode.ROR;
					case Constants.ContainerModes.BreakBulk:
						return FreightMode.BBK;
					case Constants.ContainerModes.Bulk:
						return FreightMode.BLK;
					case Constants.ContainerModes.FCLMixedShipper:
						return FreightMode.BCN;
					case Constants.ContainerModes.BuyersConsol:
						result |= FreightMode.BCN;
						break;
				}
			}

			if (containerMode == Constants.ContainerModes.ShippersConsol)
			{
				result |= FreightMode.SCN;
			}

			switch (containerMode)
			{
				case Constants.ContainerModes.LCL:
				case Constants.ContainerModes.Loose:
				case Constants.ContainerModes.RollOnRollOff:
				case Constants.ContainerModes.Liquid:
				case Constants.ContainerModes.LTL:
				case Constants.ContainerModes.BreakBulk:
				case Constants.ContainerModes.Bulk:
					result |= FreightMode.NonContainerised;
					break;

				case Constants.ContainerModes.ULD:
				case Constants.ContainerModes.FCL:
				case Constants.ContainerModes.Groupage:
					result |= FreightMode.Containerised;
					break;

				case Constants.ContainerModes.FTL:
					result |= FreightMode.FullLoad | FreightMode.NonContainerised;
					break;

				default:
					result |= fallbackContainerFreightModeGetter();
					break;
			}

			return result;
		}

		static FreightMode CalculateContainerFreightMode(RateableMeasureSet measures)
		{
			var isContainerized = measures?.IsContainerized();
			return isContainerized.HasValue && isContainerized.Value
				? FreightMode.Containerised
				: FreightMode.NonContainerised;
		}

		FreightMode IFreightRatingHelper.CalculateFreightMode(ZString transportMode, ZString containerMode, Func<FreightMode> fallbackContainerFrieghtModeGetter)
		{
			return CalculateFreightMode(transportMode, containerMode, fallbackContainerFrieghtModeGetter);
		}

		#endregion

		#region Measures

		/// <summary>
		/// Combine shipment measures. Only used for consols. They can contain multiple shipments.
		/// Takes all the values of a measure from each shipment and combines them into one big measure (doesn't actually summarize).
		/// All values are converted to the same unit as the first shipment.
		/// E.g., if the first shipment weights are in KG, all other shipment weights are converted to KG.
		///
		/// For standard consol, MeasureTypes are Weight, Volume, Package, LoadingMeters.
		/// A standard consol has its own chargeable fields and doesn't need to summarize shipment chargeables.
		/// Package and LoadingMeters do not have units (well LoadingMeters always has an implied unit of meters) and do not get any conversions.
		///
		/// For a buyers consol, MeasureTypes are Weight, Volume, Package, Chargeable.
		/// Why doesn't it summarize LoadingMeters? Good question. It's a trucking measure and maybe customers have never needed it.
		/// Maybe buyers consols are only used for air/sea freight.
		/// </summary>
		public static void SetCombinedMeasures(RateableMeasureSet measures, List<IAutoRating> shipmentAdapters, params MeasureType[] measureTypes)
		{
			if (shipmentAdapters.Count == 0)
			{
				return;
			}

			var shipmentMeasures = shipmentAdapters.Select(x => (RateableMeasureSet)x.RateableMeasures).ToList();
			measures.SetCombinedMeasureFrom(shipmentMeasures, measureTypes);
		}

		#region Convert

		public static ZDecimal Convert(ZDecimal sourceValue, ZString sourceUnit, ZString targetUnit)
			=> RateableMeasureSet.Convert(sourceValue, sourceUnit, targetUnit);
		
		#endregion

		#endregion

		#region Containers

		#region SetContainers

		/// <summary>
		/// Creates container measures for pack line containers.
		/// A shipment may not be packed into every container in a consol. Only packed containers are rated.
		/// Also, a shipment attached to more than one consol indicates all the goods start off in the first consol (vessel 1), go part of the way and then
		/// transfer to the next consol (vessel 2), etc.
		/// Only one consol must be used to count containers to avoid double counting.
		/// </summary>
		/// <param name="consol">consol to use. If null then only packlines with a single container are added.</param>
		public static void SetContainersFromPackLines(RateableMeasureSet measures, CommonConsol consol, CommonShipment shipment, IEnumerable<PackLine> packLines)
		{
			// A grouping from a container to all the packlines of the shipment
			// that are contained in that container.
			foreach (var containerPackLines in packLines
				.GroupBy(x => GetBestContainer(x, consol))
				.Where(x => x.Key != null && x.Key.JC_RC.IsValid))
			{
				var container = containerPackLines.Key;

				if (container.JC_ContainerMode != Constants.ContainerModes.LCL && container.JC_ContainerMode != Constants.ContainerModes.Groupage)
				{
					var commodity = container.JC_RH_NKContainerCommodityCode;

					if (commodity.IsEmpty)
					{
						commodity = containerPackLines
							.Where(p => !p.JL_RH_NKCommodityCode.IsEmpty)
							.SameOrDefault(p => p.JL_RH_NKCommodityCode);
					}

					var containerInfo = CreateContainerInfo(container, shipment);
					measures.AddContainerGroup(
						containerOwnership: container.GetOwnership(),
						containerNumber: container.JC_ContainerNum,
						containerTypePK: container.JC_RC,
						containerInfos: new[] { containerInfo },
						commodity: commodity,
						weight: container.JC_Calc_TotalWeightInKgs,
						volume: container.JC_Calc_TotalVolumeInM3);
				}
			}
		}

		/// <summary>
		/// Get the container for the pack line and consol.
		/// If there is none and the pack line only has one container, use that instead.
		/// Handles the case where the packline container doesn't belong to the shipment consol, but belongs to a child consol instead.
		/// </summary>
		static CommonContainer GetBestContainer(PackLine packLine, CommonConsol consol)
		{
			var count = packLine.Containers.Count;
			if (count == 1)
			{
				return packLine.Containers[0];
			}
			else if (count > 1 && consol != null)
			{
				return packLine.GetContainer(consol);
			}
			else
			{
				return null;
			}
		}

		public static MeasureInfo.ContainerInfo CreateContainerInfo(CommonContainer container, CommonShipment shipment)
		{
			var containerCount = container.JC_ContainerCount > 0 ? container.JC_ContainerCount : 1;
			var shipmentShare = shipment == null ? 1 : GetShipmentShareInContainer(shipment, container.PK);

			var info = new MeasureInfo.ContainerInfo(
				weight: container.JC_Calc_TotalWeightInKgs,
				grossWeight: new Quantity(container.JC_Calc_ActualGrossWeightInKgs, Constants.Weight.Kilograms),
				weightUnit: container.JC_GrossWeightUQ,
				volume: container.JC_Calc_TotalVolumeInM3,
				volumeUnit: container.JC_GrossVolumeUQ,
				packages: container.JC_Calc_TotalPackages,
				teu: container.JC_Calc_TEUCount,
				containerNumber: container.JC_ContainerNum,
				shipmentShare: shipmentShare,
				refNumber: "",
				containerCount: containerCount,
				containerPK: container.PK,
				containerQuality: container.JC_ContainerQuality,
				container: container.RefContainer,
				isNonOperatedReefer: container.JC_IsNonOperativeReefer);

			SetSpotRatesOnContainerInfo(info, container, shipment);

			return info;
		}

		#endregion

		#region GetShipmentContainers

		public static void SetShippingContainers(RateableMeasureSet measures, IEnumerable<CommonContainer> containers, CommonShipment shipment)
		{
			SetContainers(measures, GetShippingContainerGroups(containers, shipment));
		}

		static void SetContainers(RateableMeasureSet measures, Dictionary<ContainerKey, List<MeasureInfo.ContainerInfo>> containerInfosByContainerKeys)
		{
			measures.CreateContainerList(includeOwnership: true, includeContainerNumber: true);

			foreach (var kv in containerInfosByContainerKeys)
			{
				measures.AddContainerGroup(containerTypePK: kv.Key.ContainerType,
					commodity: kv.Key.CommodityCode,
					containerOwnership: kv.Key.ContainerOwnership,
					containerNumber: kv.Key.ContainerNumber,
					weight: kv.Key.ContainerCalculatedWeight,
					volume: kv.Key.ContainerCalculatedVolume,
					containerInfos: kv.Value);
			}
		}

		static Dictionary<ContainerKey, List<MeasureInfo.ContainerInfo>> GetShippingContainerGroups(IEnumerable<CommonContainer> containers, CommonShipment shipment)
		{
			var containerInfosByContainerKeys = new Dictionary<ContainerKey, List<MeasureInfo.ContainerInfo>>();

			foreach (CommonContainer container in containers)
			{
				var key = new ContainerKey(container, shipment.OuterPackLines.Cast<PackLine>());
				var containerInfos = containerInfosByContainerKeys.GetOrAdd(key, () => new List<MeasureInfo.ContainerInfo>());

				ZInt containerCount = container.JC_ContainerCount > 0 ? container.JC_ContainerCount : 1;

				var containerInfo = new MeasureInfo.ContainerInfo(
					weight: container.JC_GrossWeight,
					weightUnit: container.JC_GrossWeightUQ,
					volume: container.JC_GrossVolume,
					volumeUnit: container.JC_GrossVolumeUQ,
					packages: container.JC_Calc_TotalPackages,
					teu: container.JC_Calc_TEUCount,
					containerNumber: container.JC_ContainerNum,
					shipmentShare: 1,
					refNumber: "",
					containerCount: containerCount,
					containerPK: container.PK,
					containerQuality: container.JC_ContainerQuality,
					container: container.RefContainer,
					isNonOperatedReefer: container.JC_IsNonOperativeReefer);

				SetSpotRatesOnContainerInfo(containerInfo, container, shipment);
				containerInfos.Add(containerInfo);
			}

			return containerInfosByContainerKeys;
		}

		#endregion

		#region GetContainers

		public static void SetContainers(RateableMeasureSet measures, CommonShipment shipment, IEnumerable<ContainerAndMassAndVolumeHelper> containersHelpers, MeasureInfo.ContainerInfo lclInfo)
		{
			SetContainers(measures, GetContainerGroups(shipment, containersHelpers, lclInfo));
		}

		static Dictionary<ContainerKey, List<MeasureInfo.ContainerInfo>> GetContainerGroups(CommonShipment shipment, IEnumerable<ContainerAndMassAndVolumeHelper> containersHelpers, MeasureInfo.ContainerInfo lclInfo)
		{
			var containerMeasuresByContainerKeys = new Dictionary<ContainerKey, List<MeasureInfo.ContainerInfo>>();

			if (lclInfo != null)
			{
				var containersInfoMeasures = new List<MeasureInfo.ContainerInfo> { lclInfo };
				containerMeasuresByContainerKeys.Add(new ContainerKey(lclInfo), containersInfoMeasures);
			}

			foreach (var containerHelper in containersHelpers)
			{
				var container = containerHelper.Container;
				if (!container.JC_RC.IsValid)
				{
					continue;
				}

				var packLines = containerHelper.Shipment != null
					? containerHelper.Shipment.OuterPackLines.Cast<PackLine>().ToArray()
					: Array.Empty<PackLine>();

				var key = new ContainerKey(container, packLines);
				var containerInfos = containerMeasuresByContainerKeys.GetOrAdd(key, () => new List<MeasureInfo.ContainerInfo>());

				var shipmentShare = shipment == null ? 1 : GetShipmentShareInContainer(shipment, container.PK);

				var containerInfo = new MeasureInfo.ContainerInfo(
					weight: containerHelper.Weight,
					grossWeight: containerHelper.GrossWeight,
					weightUnit: container.JC_GrossWeightUQ,
					volume: containerHelper.Volume,
					volumeUnit: container.JC_GrossVolumeUQ,
					packages: container.JC_Calc_TotalPackages,
					teu: container.JC_Calc_TEUCount,
					containerNumber: container.JC_ContainerNum,
					shipmentShare: shipmentShare,
					refNumber: "",
					containerCount: container.JC_ContainerCount > 0 ? container.JC_ContainerCount : 1,
					containerPK: container.PK,
					containerQuality: container.JC_ContainerQuality,
					container: container.RefContainer,
					pivotBreak: container.JC_PivotBreak,
					isNonOperatedReefer: container.JC_IsNonOperativeReefer);

				SetSpotRatesOnContainerInfo(containerInfo, container, shipment);
				containerInfos.Add(containerInfo);
			}

			return containerMeasuresByContainerKeys;
		}

		public static ZDecimal GetShipmentShareInContainer(CommonShipment shipment, ZGuid containerPK)
		{
			var containerShare = 1m;

			var container = shipment.Containers?.FirstOrDefault(x => x.PK == containerPK);

			if (container != null && container.RefContainer != null)
			{
				var totalOccupiedVolume = container.PackLines.Cast<PackLine>().Sum(p => p.JL_ActualVolume);
				var totalOccupiedWeight = container.PackLines.Cast<PackLine>().Sum(p => p.JL_ActualWeight);

				var containerVolumeCapacity = container.RefContainer.RC_CubicCapacity * container.JC_ContainerCount;
				var containerWeightCapacity = container.RefContainer.RC_NetWeight * container.JC_ContainerCount;

				bool useVolume = containerWeightCapacity <= 0m
					|| (containerVolumeCapacity > 0m
						&& totalOccupiedVolume / containerVolumeCapacity > totalOccupiedWeight / containerWeightCapacity);

				if (useVolume)
				{
					var shipmentVolume = shipment.OuterPackLines
						.Cast<PackLine>()
						.Where(p => p.JL_JC == containerPK)
						.Sum(p => p.JL_ActualVolume);

					containerShare = (totalOccupiedVolume > 0m)
					? shipmentVolume / totalOccupiedVolume
					: 0m;
				}
				else
				{
					var shipmentWeight = shipment.OuterPackLines
						.Cast<PackLine>()
						.Where(p => p.JL_JC == containerPK)
						.Sum((p) => p.JL_ActualWeight);

					containerShare = (totalOccupiedWeight > 0m)
					? shipmentWeight / totalOccupiedWeight
					: 0m;
				}
			}

			return containerShare;
		}

		static void SetSpotRatesOnContainerInfo(MeasureInfo.ContainerInfo containerInfo, CommonContainer container, CommonShipment shipment)
		{
			if (container.JC_SellSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate
							|| container.JC_CostSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate
							|| container.JC_GatewaySellSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate)
			{
				containerInfo.SetSpotRates(GetContainerCostSpotRate(container, shipment), GetContainerSellSpotRate(container, shipment), container.PK, container.JC_RC);
			}
		}

		static SpotRateInfo GetContainerCostSpotRate(CommonContainer container, CommonShipment shipment)
		{
			if (!container.Validation.ContainerSpotRateIsValid(AutoratedValueType.NegotiatedCost))
			{
				return new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.NegotiatedCost);
			}

			OrgHeader creditor = null;
			var parentJob = shipment != null ? shipment.Job : container.Consol != null ? container.Consol.Job : null;

			if (IsGatewaySpotRateApplicableForContainer(parentJob, shipment, container, true))
			{
				var money = new Money(container.JC_GatewaySellSpotRate, container.GatewaySellSpotRateCurrency);
				var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var gatewayConsols = new List<CommonConsol>();

				if (shipment != null)
				{
					gatewayConsols.AddRange(shipment.Consols
					.Cast<CommonConsol>()
					.Where(x => x.LoadPort != null && x.LoadPort.RL_RN_NKCountryCode == countryCode && IsConsolSendingAgentActingAsGatewayInAnyCompany(x))
					.ToList());
				}
				else if (container.Consol != null
						&& container.Consol.LoadPort != null
						&& container.Consol.LoadPort.RL_RN_NKCountryCode == countryCode
						&& IsConsolSendingAgentActingAsGatewayInAnyCompany(container.Consol))
				{
					gatewayConsols.Add(container.Consol);
				}

				if (gatewayConsols.Count == 1)
				{
					creditor = gatewayConsols[0].SendingForwarder;
				}

				return new SpotRateInfo(money, container.JC_GatewaySellSpotRateMode, AutoratedValueType.GatewaySell) { Creditor = creditor };
			}
			else if (container.JC_CostSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate)
			{
				var money = new Money(container.JC_CostSpotRate, container.CostSpotRateCurrency);
				if (container.Consol != null)
				{
					creditor = container.Consol.Creditor;
				}

				return new SpotRateInfo(money, container.JC_CostSpotRateMode, AutoratedValueType.NegotiatedCost) { Creditor = creditor };
			}

			return null;
		}

		static SpotRateInfo GetContainerSellSpotRate(CommonContainer container, CommonShipment shipment)
		{
			var parentJob = shipment != null ? shipment.Job : container.Consol != null ? container.Consol.Job : null;

			if (!container.Validation.ContainerSpotRateIsValid(AutoratedValueType.SpotRate))
			{
				return new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.SpotRate);
			}

			if (IsGatewaySpotRateApplicableForContainer(parentJob, shipment, container, false))
			{
				var money = new Money(container.JC_GatewaySellSpotRate, container.GatewaySellSpotRateCurrency);
				return new SpotRateInfo(money, container.JC_GatewaySellSpotRateMode, AutoratedValueType.GatewaySell);
			}
			else if (container.JC_SellSpotRateMode != Constants.FreightRateAutoratingModes.Code.StandardRate)
			{
				var money = new Money(container.JC_SellSpotRate, container.SellSpotRateCurrency);
				return new SpotRateInfo(money, container.JC_SellSpotRateMode, AutoratedValueType.SpotRate);
			}

			return null;
		}

		static bool IsGatewaySpotRateApplicableForContainer(JobHeader parentJob, CommonShipment parentShipment, CommonContainer container, bool isCost)
		{
			if (container.JC_GatewaySellSpotRateMode == Constants.FreightRateAutoratingModes.Code.StandardRate
			 || parentJob == null
			 || parentJob.Department == null
			 || string.IsNullOrEmpty(parentJob.Department.GE_Code)
			 || !container.Validation.ContainerSpotRateIsValid(AutoratedValueType.GatewaySell))
			{
				return false;
			}

			if ((isCost && !parentJob.Department.IsGatewayDepartment) || (!isCost && parentJob.Department.IsGatewayDepartment))
			{
				return (container.Consol != null && IsConsolSendingAgentActingAsGatewayInAnyCompany(container.Consol)) || HasConsolSendingAgentActingAsGatewayInAnyCompany(parentShipment);
			}

			return false;
		}

		public static bool HasConsolSendingAgentActingAsGatewayInAnyCompany(CommonShipment shipment)
		{
			return shipment != null && shipment.Consols.Cast<CommonConsol>().Any(IsConsolSendingAgentActingAsGatewayInAnyCompany);
		}

		public static bool IsConsolSendingAgentActingAsGatewayInAnyCompany(CommonConsol consol)
		{
			var sendingForwarder = consol?.SendingForwarder;
			if (sendingForwarder != null)
			{
				var result = AllGlbCompanies(consol.Factory).Any(company =>
				{
					var gatewayAgent = consol.GatewayAgent(company);
					return gatewayAgent.sendingAgent?.PK == sendingForwarder.PK ||
						   gatewayAgent.receivingAgent?.PK == sendingForwarder.PK;
				});

				return result;
			}

			return false;
		}

		static IEnumerable<GlbCompany> AllGlbCompanies(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GlbCompany", () => factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true)));
		}

		struct ContainerKey : IEquatable<ContainerKey>
		{
			// This code is defective if there are two or more containers with no container number and the same container type, commodity and ownership.
			// They will have the same key, but not necessarily the same ContainerCalculatedWeight, ContainerCalculatedVolume.
			// Should remove weight and volume from key, or add them to key comparer, or something...
			public ContainerKey(CommonContainer container, IEnumerable<PackLine> packLines)
				: this(container.JC_RC, container.CommodityCodeForRating, container.GetOwnership(), container.JC_ContainerNum)
			{
				if (CommodityCode.IsEmpty)
				{
					CommodityCode = packLines
						.Where(p => !p.JL_RH_NKCommodityCode.IsEmpty)
						.SameOrDefault(p => p.JL_RH_NKCommodityCode);
				}

				ContainerCalculatedWeight = container.JC_Calc_TotalWeightInKgs;
				ContainerCalculatedVolume = container.JC_Calc_TotalVolumeInM3;
			}

			public ContainerKey(MeasureInfo.ContainerInfo lclInfo)
				: this(MeasureInfo.ContainerInfo.LCL, string.Empty, string.Empty, string.Empty)
			{
				if (lclInfo != null)
				{
					ContainerCalculatedWeight = lclInfo.Weight;
					ContainerCalculatedVolume = lclInfo.Volume;
				}
			}

			ContainerKey(ZGuid containerType, ZString commodityCode, ZString containerOwnership, ZString containerNumber)
			{
				ContainerType = containerType;
				CommodityCode = commodityCode;
				ContainerOwnership = containerOwnership;
				ContainerNumber = containerNumber;
				ContainerCalculatedWeight = 0;
				ContainerCalculatedVolume = 0;
			}

			public readonly ZGuid ContainerType;
			public readonly ZString CommodityCode;
			public readonly ZString ContainerOwnership;
			public readonly ZString ContainerNumber;
			public readonly ZDecimal ContainerCalculatedWeight;
			public readonly ZDecimal ContainerCalculatedVolume;

			public bool Equals(ContainerKey other)
			{
				return ContainerType.Equals(other.ContainerType)
					&& CommodityCode.Equals(other.CommodityCode)
					&& ContainerOwnership.Equals(other.ContainerOwnership)
					&& ContainerNumber.Equals(other.ContainerNumber);
			}

			public override bool Equals(object obj)
			{
				return Equals((ContainerKey)obj);
			}

			public static bool operator ==(ContainerKey key1, ContainerKey key2)
			{
				return key1.Equals(key2);
			}

			public static bool operator !=(ContainerKey key1, ContainerKey key2)
			{
				return !key1.Equals(key2);
			}

			public override int GetHashCode()
			{
				return ContainerType.GetHashCode() ^ CommodityCode.GetHashCode() ^ ContainerOwnership.GetHashCode();
			}
		}

		#endregion

		#region GetContainersServiceInfo

		public static IEnumerable<JobServiceInfo> GetServiceInfosFromContainers(IEnumerable<CommonContainer> containers, ZString serviceCode, GetContainerServices getServiceInfos)
		{
			var containerServiceInfos = new List<JobServiceInfo>();
			foreach (var container in containers)
			{
				foreach (var serviceInfo in getServiceInfos(container, serviceCode))
				{
					if (serviceInfo.IsEnabled)
					{
						PopulateServiceContainerInfo(serviceInfo, container);
						containerServiceInfos.Add(serviceInfo);
					}
				}
			}

			if (containerServiceInfos.Count == 0)
			{
				return new List<JobServiceInfo> { JobServiceInfo.Empty("", serviceCode) };
			}

			bool allowMultipleContractors = containerServiceInfos.First().IsHiddenService;
			if (!allowMultipleContractors && containerServiceInfos.GroupBy(si => si.Contractor).Count() > 1)
			{
				var faultMessage = Res.GetString("ef8d9934-d5e3-441b-a94a-8d486ac63fad", "{0} service has been performed on multiple containers by multiple contractors which is not supported.", serviceCode);
				return new List<JobServiceInfo> { JobServiceInfo.Faulty("", serviceCode, faultMessage) };
			}

			return containerServiceInfos;
		}

		public static CodeDescriptionPairList HiddenContainerServices
		{
			get
			{
				if (forwardingContainerServices == null)
				{
					forwardingContainerServices = new CodeDescriptionPairList();
					forwardingContainerServices.AddPair(ChargeCodeSubGroupList.CartageDemurrageTotal, ResString.GetMultilingualString("cc52fdef-792a-4e0c-8d9b-cac51d4f0c01", "Port Transport Demurrage"));
					forwardingContainerServices.AddPair(ChargeCodeSubGroupList.ContainerDetention, ResString.GetMultilingualString("75ba8868-8d8c-4508-8e53-5c021e1d8d1a", "Container Detention"));
					forwardingContainerServices.AddPair(ChargeCodeSubGroupList.Storage, ResString.GetMultilingualString("e74c15a9-cd83-4650-bb6e-dc83e05df47f", "Storage"));
					forwardingContainerServices.AddPair(ChargeCodeSubGroupList.CarrierStorage, ResString.GetMultilingualString("79871883-0375-49c6-8ba1-4c3d7919e521", "Carrier Storage/Demurrage"));
					forwardingContainerServices.AddPair(ChargeCodeSubGroupList.MergedDemurrageDetention, ResString.GetMultilingualString("D24F749D-39DE-41D1-8854-62EBF35DA5B1", "Merged Demurrage/Detention"));
				}

				return forwardingContainerServices;
			}
		}

		[ThreadStatic]
		static CodeDescriptionPairList forwardingContainerServices;

		public static IEnumerable<JobServiceInfo> GetServiceInfoDefault(CommonContainer container, ZString serviceCode)
		{
			return HiddenContainerServices.ContainsCode(serviceCode)
				? GetHiddenContainerServices(container, serviceCode)
				: container.Services.GetServiceInfos(serviceCode);
		}

		public delegate IEnumerable<JobServiceInfo> GetContainerServices(CommonContainer container, ZString serviceCode);

		internal static IEnumerable<JobServiceInfo> GetHiddenContainerServices(CommonContainer container, ZString serviceCode)
		{
			var consol = container.Consol;
			if (consol != null && consol.JK_IsForwarding)
			{
				return GetServiceInfoFromContainerPenalties(container, serviceCode);
			}
			else
			{
				return Enumerable.Empty<JobServiceInfo>();
			}
		}

		static IEnumerable<JobServiceInfo> GetServiceInfoFromContainerPenalties(CommonContainer container, ZString serviceCode)
		{
			var result = new List<JobServiceInfo>();
			var penaltyTypeCode = ConvertServiceCodeToContainerPenaltyTypeCode(serviceCode);
			if (string.IsNullOrEmpty(penaltyTypeCode))
			{
				return result;
			}

			AddServiceInfoFromContainerPenalties(false, result, container, serviceCode, penaltyTypeCode);
			AddServiceInfoFromContainerPenalties(true, result, container, serviceCode, penaltyTypeCode);
			return result;
		}

		static void AddServiceInfoFromContainerPenalties(bool isImport, List<JobServiceInfo> services, CommonContainer container, string serviceCode, string penaltyTypeCode)
		{
			var penalties = isImport ? container.ImportPenalties : (ContainerPenaltyCollection)container.ExportPenalties;
			var filteredPenalties = penalties.Where(x => x.CPY_PenaltyType == penaltyTypeCode);

			var requiredCreditorType = GetRequiredCreditorType(serviceCode);
			if (!requiredCreditorType.IsEmpty)
			{
				filteredPenalties = filteredPenalties.Where(x => x.CPY_CreditorType == requiredCreditorType);
			}

			foreach (var penalty in filteredPenalties)
			{
				var service = GetServiceInfoFromContainerPenalty(container, penalty, isImport, serviceCode);
				if (service != null)
				{
					services.Add(service);
				}
			}
		}

		static ZString GetRequiredCreditorType(string serviceCode)
		{
			if (serviceCode == ChargeCodeSubGroupList.CarrierStorage)
			{
				return Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			}
			else if (serviceCode == ChargeCodeSubGroupList.Storage)
			{
				return Constants.ContainerPenaltyCreditorType.Codes.CTO;
			}

			return ZString.Empty;
		}

		static List<OrgHeader> GetFallbackContainerPenaltyCreditors(CommonConsol consol, bool isImport, string containerPenaltyCreditorType)
		{
			var result = new List<OrgHeader>();
			if (consol == null)
			{
				return result;
			}

			switch (containerPenaltyCreditorType)
			{
				case Constants.ContainerPenaltyCreditorType.Codes.Transport:
					{
						var org = isImport
							? consol.ArrivalUnpackCFSTransportAddress?.Header
							: consol.DeparturePackCFSTransportAddress?.Header;
						AddIfNotNull(result, org);
						break;
					}
				case Constants.ContainerPenaltyCreditorType.Codes.Carrier:
					{
						AddIfNotNull(result, consol.Creditor);
						AddIfNotNull(result, consol.ShippingLine);
						break;
					}
				case Constants.ContainerPenaltyCreditorType.Codes.CTO:
					{
						var org = isImport
							? consol.ArrivalCTOAddress?.Header
							: consol.DepartureCTOAddress?.Header;
						AddIfNotNull(result, org);
						break;
					}
				default:
					throw new InvalidOperationException("Unknown ContainerPenaltyCreditorType " + containerPenaltyCreditorType);
			}

			return result;
		}

		static void AddIfNotNull(List<OrgHeader> orgs, OrgHeader orgToAdd)
		{
			if (orgToAdd != null && !orgs.Contains(orgToAdd))
			{
				orgs.Add(orgToAdd);
			}
		}

		static string ConvertServiceCodeToContainerPenaltyTypeCode(string serviceCode)
		{
			switch (serviceCode)
			{
				case ChargeCodeSubGroupList.CartageDemurrageTotal:
					return Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
				case ChargeCodeSubGroupList.ContainerDetention:
					return Constants.ContainerPenaltyPenaltyType.Codes.Detention;
				case ChargeCodeSubGroupList.Storage:
				case ChargeCodeSubGroupList.CarrierStorage:
					return Constants.ContainerPenaltyPenaltyType.Codes.Storage;
				case ChargeCodeSubGroupList.MergedDemurrageDetention:
					return Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
				default:
					return string.Empty;
			}
		}

		static JobServiceInfo GetServiceInfoFromContainerPenalty(CommonContainer container, ContainerPenalty penalty, bool isImport, string serviceCode)
		{
			var timeSpan = penalty.CPY_Duration.IsValid ? penalty.CPY_Duration.ToTimeSpan() : TimeSpan.Zero;
			if (timeSpan == TimeSpan.Zero && penalty.CPY_TotalCost == 0)
			{
				return null;
			}

			var fallbackCreditors = GetFallbackContainerPenaltyCreditors(container.Consol, isImport, penalty.CPY_CreditorType);
			var penaltyCreditor = penalty.Creditor;

			var service = new JobServiceInfo(true,
					isImport ? ChargeCodeGroupList.Codes.Destination : ChargeCodeGroupList.Codes.Origin,
					serviceCode,
					HiddenContainerServices.GetDescriptionFromCode(serviceCode),
					serviceCount: 1m,
					duration: timeSpan,
					contractor: penaltyCreditor ?? fallbackCreditors.FirstOrDefault(),
					rate: penalty.CPY_PerUnitCost,
					unit: ConvertPenaltyTimeUnitToServiceTimeUnit(penalty.CPY_TimeUnit),
					isHiddenService: true,
					totalCost: penalty.CPY_TotalCost,
					currencyCode: penalty.CPY_RX_NKCurrency,
					locationCode: penalty.CPY_RL_NKLocation);
			service.IsCostForSpotRate = true;
			if (penaltyCreditor != null)
			{
				service.IsContractorCreditor = true;
			}
			fallbackCreditors.Remove(penaltyCreditor);
			service.FallbackContractors = fallbackCreditors.ToArray();

			PopulateServiceContainerInfo(service, container);
			return service;
		}

		static string ConvertPenaltyTimeUnitToServiceTimeUnit(string penaltyUnit)
		{
			switch (penaltyUnit)
			{
				case Constants.ContainerPenaltyTimeUnit.Codes.Days:
					return JobServiceInfo.Constants.Codes.Day;
				case Constants.ContainerPenaltyTimeUnit.Codes.Hours:
					return JobServiceInfo.Constants.Codes.Hour;
				default:
					throw new InvalidOperationException("Unknown penalty unit " + penaltyUnit);
			}
		}

		internal static void PopulateServiceContainerInfo(JobServiceInfo serviceInfo, CommonContainer container)
		{
			serviceInfo.Container = container.PK;
			serviceInfo.ContainerCount = container.JC_ContainerCount;
			serviceInfo.ContainerNumber = container.JC_ContainerNum;
			if (!container.JC_RC.IsEmpty && container.JC_RC.IsValid)
			{
				serviceInfo.ContainerType = container.JC_RC;
			}
		}

		#endregion

		#endregion
	}

	public struct ContainerAndMassAndVolumeHelper
	{
		public ContainerAndMassAndVolumeHelper(
			CommonContainer container,
			CommonShipment shipmnent = null,
			ZDecimal? weight = null,
			ZDecimal? volume = null,
			Quantity? grossWeight = null,
			ZBool? isNonOperatedReefer = null)
		{
			Container = container;
			Shipment = shipmnent;

			Weight = weight ?? (container.IsGrossWeightOverrideActive
					? container.JC_GrossWeight
					: container.JC_Calc_TotalWeightInKgs);

			GrossWeight = grossWeight ?? (container.IsGrossWeightOverrideActive
					? new Quantity(container.JC_GrossWeight, container.JC_GrossWeightUQ)
					: new Quantity(container.JC_Calc_ActualGrossWeightInKgs, Constants.Weight.Kilograms));

			Volume = volume ?? (container.IsGrossWeightOverrideActive
					? 0
					: container.JC_Calc_TotalVolumeInM3);

			IsNonOperatedReefer = isNonOperatedReefer ?? container.JC_IsNonOperativeReefer;
		}

		public CommonContainer Container { get; }
		public CommonShipment Shipment { get; }
		public ZDecimal Weight { get; }
		public Quantity GrossWeight { get; }
		public ZDecimal Volume { get; }
		public ZBool IsNonOperatedReefer { get; }

		public override bool Equals(object obj)
		{
			if (obj is ContainerAndMassAndVolumeHelper helper)
			{
				return helper.Container.PK == Container.PK;
			}

			return false;
		}

		public static bool operator ==(ContainerAndMassAndVolumeHelper helper1, ContainerAndMassAndVolumeHelper helper2)
		{
			return helper1.Equals(helper2);
		}

		public static bool operator !=(ContainerAndMassAndVolumeHelper helper1, ContainerAndMassAndVolumeHelper helper2)
		{
			return !helper1.Equals(helper2);
		}

		public override int GetHashCode()
		{
			return Container.GetHashCode();
		}
	}
}
