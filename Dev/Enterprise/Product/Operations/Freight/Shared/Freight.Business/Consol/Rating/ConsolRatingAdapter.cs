using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Consol;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Base class rating adapter for consols derived from CommonConsol.
	/// Includes ForwardingConsol and CFSLoadListConsol.
	/// </summary>
	public class ConsolRatingAdapter<T> : RatingRouteAdapter<T>,
											IAutoRatingWeightBreakOverrideProvider,
											IAutoRatingFreightConditionsSupportable
		where T : CommonConsol
	{
		public ConsolRatingAdapter(IRatingRoute<IRoutingSupport> ratingRoute)
			: base(ratingRoute)
		{
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return chargeCodeGroups ?? (chargeCodeGroups = GetChargeCodeGroups()); }
		}

		ChargeCodeGroupCollection chargeCodeGroups;

		ChargeCodeGroupCollection GetChargeCodeGroups()
		{
			var result = new ChargeCodeGroupCollection();
			result.AddRange(Env.Registry.Rating.FreightRatedCodes);
			result.SellChargesFilter = ChargeCodeFilter.AutorateNothing;
			result.CostChargesFilter = ChargeCodeFilter.AutorateConsolLevelOnly;
			return result;
		}

		public override AdapterType AdapterType => AdapterType.Consolidation;

		public override ZString OperationalJobCode => Invariant($"{Parent.JK_UniqueConsignRef} Route {RouteSetNumber}");  // autorating long info

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.ForwardingConsol; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override IEnumerable<ZString> CarrierContractNumbers =>
			Parent.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON);

		public override JobServicesCollection JobServices
		{
			get
			{
				var jobServices = new JobServicesCollection();
				var servicesChargeGroup = this.IsImport() ? ChargeCodeGroupList.Codes.Destination : ChargeCodeGroupList.Codes.Origin;
				var containers = Parent.Containers.Cast<CommonContainer>().ToArray();

				var containerSpecialServiceTypes = new FreightServiceTypes();
				containerSpecialServiceTypes.AddRangeOverwriteIfExists(FreightRatingHelper.HiddenContainerServices);

				foreach (CodeDescriptionPair subGroup in containerSpecialServiceTypes)
				{
					var serviceInfos = FreightRatingHelper.GetServiceInfosFromContainers(containers, subGroup.Code, FreightRatingHelper.GetServiceInfoDefault);

					foreach (var serviceInfo in serviceInfos.Where(x => x.ChargeCodeGroup.IsEmpty))
					{
						var substitutedChargeGroup = GetServiceChargeGroup(serviceInfo);
						var currentServicesChargeGroup = string.IsNullOrEmpty(substitutedChargeGroup) ? servicesChargeGroup : substitutedChargeGroup;

						serviceInfo.ChargeCodeGroup = currentServicesChargeGroup;
						if (serviceInfo.ServiceDescription.IsEmpty)
						{
							serviceInfo.ServiceDescription = subGroup.Description;
						}
					}

					jobServices.AddRange(serviceInfos);
				}

				return jobServices;
			}
		}

		string GetServiceChargeGroup(JobServiceInfo info)
		{
			string servicesChargeGroup = null;
			if (info.IsEnabled && !info.LocationCountryCode.IsEmpty)
			{
				if (Parent.LoadPort != null && info.LocationCountryCode == Parent.LoadPort.RL_RN_NKCountryCode)
				{
					servicesChargeGroup = ChargeCodeGroupList.Codes.Origin;
				}
				else if (Parent.DischargePort != null && info.LocationCountryCode == Parent.DischargePort.RL_RN_NKCountryCode)
				{
					servicesChargeGroup = ChargeCodeGroupList.Codes.Destination;
				}
			}

			return servicesChargeGroup;
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				var autoRatingInfo = AutoRatingAdapters.FirstOrDefault(shipment => !shipment.StatusInformation.CanExecute);

				return autoRatingInfo != null
						? autoRatingInfo.StatusInformation
						: new AutoRatingStatusInfo(true);
			}
		}

		public List<IAutoRating> AutoRatingAdapters
		{
			get
			{
				return autoRatingAdapters ?? (autoRatingAdapters = AllShipments.Select(shipment => shipment.RatingAdapter).ToList());
			}
		}
		List<IAutoRating> autoRatingAdapters;

		public override IDocAddress DeliveryAddress
		{
			get
			{
				var consignee = GetConsignee();
				return consignee != null ? consignee.MainAddress : null;
			}
		}

		public override IDocAddress PickupAddress
		{
			get
			{
				var consignor = GetConsignor();
				return consignor != null ? consignor.MainAddress : null;
			}
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				result[RatingDebtorOrgTypes.CNE] = GetConsignee();
				result[RatingDebtorOrgTypes.CNR] = GetConsignor();
				result[RatingDebtorOrgTypes.CCUS] = GetControllingCustomer();

				return result;
			}
		}

		OrgHeader GetConsignee()
		{
			return GetValueIfSameForAllShipments(x => x.RatingAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNE], BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer);
		}

		OrgHeader GetConsignor()
		{
			return GetValueIfSameForAllShipments(x => x.RatingAdapter.DebtorOrgs[RatingDebtorOrgTypes.CNR], BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer);
		}

		OrgHeader GetControllingCustomer()
		{
			var result = AllShipments
				.Select(x => x.RatingAdapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS])
				.Where(x => x != null)
				.SameOrDefault(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer);

			return result;
		}

		TX GetValueIfSameForAllShipments<TX>(Func<CommonShipment, TX> selector, IEqualityComparer<TX> comparer = null)
		{
			comparer = comparer ?? EqualityComparer<TX>.Default;

			var values = AllShipments.Select(selector);
			var first = values.FirstOrDefault();

			return Equals(first, default(TX)) || values.Skip(1).Any(x => Equals(x, default(TX)) || !comparer.Equals(x, first))
				? default(TX)
				: first;
		}

		public override FreightMode FreightMode
		{
			get
			{
				if (!isContainerized.HasValue)
				{
					isContainerized = ((RateableMeasureSet)RateableMeasures)?.IsContainerized() ?? false;
				}

				return FreightRatingHelper.CalculateFreightMode(TransportMode, ContainerMode, isContainerized.Value);
			}
		}

		public override ZString ContainerMode
		{
			get { return Parent.JK_ConsolMode; }
		}

		protected virtual void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			var containerList = new List<ContainerAndMassAndVolumeHelper>();
			var addLCL = false;
			ZDecimal lclWeight = 0m;
			ZDecimal lclVolume = 0m;
			foreach (var commonContainer in Parent.Containers.Cast<CommonContainer>())
			{
				if (commonContainer.JC_ContainerMode == Constants.ContainerModes.LCL)
				{
					addLCL = true;
					lclWeight += commonContainer.JC_Calc_TotalWeightInKgs;
					lclVolume += commonContainer.JC_Calc_TotalVolumeInM3;
				}
				else if (containerList.All(x => x.Container != commonContainer))
				{
					containerList.Add(new ContainerAndMassAndVolumeHelper(commonContainer));
				}
			}

			var lclInfo = addLCL
				? new MeasureInfo.ContainerInfo(lclWeight, Constants.Weight.Kilograms, lclVolume, Constants.Volume.CubicMetres, 0, 0, ZString.Empty, containerCount: 0)
				: null;

			FreightRatingHelper.SetContainers(measures, null, containerList, lclInfo);
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				var packs = GetPackages();
				var units = GetUnits();

				var topLevelShipments = Parent.TopLevelShipments;

				result.AddPartList(MeasureType.Weight, packs);
				result.AddPartList(MeasureType.Volume, packs);
				result.AddPartList(MeasureType.Package, packs);
				result.AddPartList(MeasureType.Unit, units);
				result.AddPartList(MeasureType.LoadingMeters, packs);
				result.SetQuantity(MeasureType.Chargeable, Parent.JK_ConsolChargeable, Parent.JK_ConsolChargeableUnit);
				result.SetQuantity(MeasureType.JobWeight, Parent.JK_CorrectedConsolWeight, Parent.JK_CorrectedConsolWeightUnit);
				result.SetQuantity(MeasureType.JobVolume, Parent.JK_CorrectedConsolVolume, Parent.JK_CorrectedConsolVolumeUnit);

				result.Shipments = topLevelShipments.Count;
				result.LowestBill = AllShipments.Except(topLevelShipments).Count();
				SetAutoRatingContainers(result);

				OverrideCommoditiesIfNecessary(result);

				isContainerized = result.IsContainerized();

				return result;
			}
		}

		bool? isContainerized;

		void OverrideCommoditiesIfNecessary(RateableMeasureSet result)
		{
			if (!Parent.JK_RH_NKConsolCommodity.IsEmpty && Parent.JK_RH_NKConsolCommodity.IsValid)
			{
				// If the Consol has the ConsolCommodity set then replace all
				// commodities in the measures with that one. This will overwrite
				// all shipment related commodities (weight/volume commodities),
				// and all container commodities.
				result.UpdateCommodities(Parent.JK_RH_NKConsolCommodity);
			}
		}

		protected virtual RateablePartList GetPackages()
		{
			return GetPackages(Parent, true, true, true);
		}

		protected static RateablePartList GetPackages(CommonConsol consol, bool includePacked, bool includeUnpacked, bool fallbackToShipmentMeasures)
		{
			var containers = consol.Containers.Cast<CommonContainer>();
			var (weightUnit, volumeUnit) = GetWeightVolumeUnits(containers, consol.TopLevelShipments.Cast<CommonShipment>());

			var packages = new RateablePartList();
			packages.HasContainerNumber = true;
			packages.HasContainerType = true;
			packages.HasCommodity = true;
			packages.WeightUnit = weightUnit;
			packages.VolumeUnit = volumeUnit;

			var shipmentParts = consol.ShipmentsForTotalling
				.Cast<CommonShipment>()
				.Where(shipment => shipment.RatingAdapter.StatusInformation.CanExecute)
				.Select(x => GetPartsFromShipment(x, consol, weightUnit, volumeUnit, fallbackToShipmentMeasures))
				.ToList();

			// Add packed lines
			if (includePacked)
			{
				foreach (var container in containers)
				{
					var containerPackages = shipmentParts
						.SelectMany(p => p)
						.Where(p => p.ContainerPK == container.PK)
						.ToList();

					if (container.IsGrossWeightOverrideActive)
					{
						packages.AddPart(new RateableContainer
						{
							Weight = FreightRatingHelper.Convert(container.JC_GrossWeight, container.JC_GrossWeightUQ, weightUnit),
							Volume = 0,
							PackageCount = containerPackages.Sum(p => p.PackageCount),
							LoadingMeterMeasure = new ClientProviderValues(
								containerPackages.Sum(p => p.LoadingMeterMeasure.Actual),
								containerPackages.Sum(p => p.LoadingMeterMeasure.ForClient),
								containerPackages.Sum(p => p.LoadingMeterMeasure.ForProvider)),
							CommodityCode = container.CommodityCodeForRating,
							ContainerPK = NullableHelper.ToNullable(container.PK),
							ContainerTypePk = NullableHelper.ToNullable(container.JC_RC),
							ContainerNumber = container.JC_ContainerNum,
							PivotBreak = container.JC_PivotBreak,
							IsWeightWithoutVolume = true
						});
					}
					else
					{
						packages.AddParts(containerPackages);
					}
				}
			}

			// Add unpacked lines
			if (includeUnpacked)
			{
				packages.AddParts(shipmentParts.SelectMany(p => p).Where(p => !p.ContainerTypePk.HasValue));
			}

			return packages.Any() ? packages : null;
		}

		static RateablePartList GetPartsFromShipment(CommonShipment shipment, CommonConsol consol, string requestedWeightUnit, string requestedVolumeUnit, bool fallbackToShipmentMeasures)
		{
			var adapter = shipment.RatingAdapter as IShipmentRatingAdapter;
			if (adapter == null)
			{
				return null;
			}

			var packs = adapter.GetPackages(consol, fallbackToShipmentMeasures);

			if (packs.WeightUnit == requestedWeightUnit && packs.VolumeUnit == requestedVolumeUnit)
			{
				return packs;
			}

			foreach (var pack in packs.Cast<RateablePart>())
			{
				if (packs.WeightUnit != requestedWeightUnit)
				{
					pack.WeightMeasure = new ClientProviderValues(
						FreightRatingHelper.Convert(pack.WeightMeasure.Actual, packs.WeightUnit, requestedWeightUnit),
						FreightRatingHelper.Convert(pack.WeightMeasure.ForClient, packs.WeightUnit, requestedWeightUnit),
						FreightRatingHelper.Convert(pack.WeightMeasure.ForProvider, packs.WeightUnit, requestedWeightUnit));
				}

				if (packs.VolumeUnit != requestedVolumeUnit)
				{
					pack.VolumeMeasure = new ClientProviderValues(
						FreightRatingHelper.Convert(pack.VolumeMeasure.Actual, packs.VolumeUnit, requestedVolumeUnit),
						FreightRatingHelper.Convert(pack.VolumeMeasure.ForClient, packs.VolumeUnit, requestedVolumeUnit),
						FreightRatingHelper.Convert(pack.VolumeMeasure.ForProvider, packs.VolumeUnit, requestedVolumeUnit));
				}
			}

			packs.WeightUnit = requestedWeightUnit;
			packs.VolumeUnit = requestedVolumeUnit;
			return packs;
		}

		RateablePartList GetUnits()
		{
			var units = new RateablePartList();
			units.HasContainerType = true;
			units.HasPackageType = true;

			var shipmentUnits = Parent.TopLevelShipments
				.Cast<CommonShipment>()
				.Select(x => x.RatingAdapter as IShipmentRatingAdapter)
				.Select(a => a.GetUnits(Parent))
				.ToList();

			foreach (var parts in shipmentUnits)
			{
				units.AddParts(parts);
			}

			return units.Any() ? units : null;
		}

		/// <summary>
		///		Gets weight and volume units based on provided <paramref name="containers"/> and <paramref name="shipments"/>
		///		configuration of the current consol.
		///
		///		Measures must contain weight and volume details in the same unit, so, we are trying to find the most common
		///		units across all pack lines from all <paramref name="shipments"/> packed in <paramref name="containers"/>.
		/// </summary>
		/// <returns></returns>
		static (string weight, string volume) GetWeightVolumeUnits(IEnumerable<CommonContainer> containers, IEnumerable<CommonShipment> shipments)
		{
			string getUnit(
				Func<CommonContainer, ZString> containerUnitGetter,
				Func<PackLine, ZString> packLineUnitGetter)
			{
				var units = containers.Select(containerUnitGetter).Where(u => !u.IsEmpty).ToList();
				if (units.Count == 0)
				{
					var packLines = containers.SelectMany(c => c.PackLines.Cast<PackLine>()).ToList();
					packLines.AddRange(shipments.SelectMany(s => s.OuterPackLines.Cast<PackLine>()).Where(p => p.JL_JC.IsEmpty));

					units = packLines.Select(packLineUnitGetter).Where(u => !u.IsEmpty).ToList();
				}

				if (units.Count > 0)
				{
					return units.GroupBy(u => u).OrderByDescending(u => u.Count()).First().Key;
				}

				return null;
			}

			var weightUnit = getUnit(c => c.JC_GrossWeightUQ, l => l.PackLineWeightUnit);
			var volumeUnit = getUnit(c => c.JC_GrossVolumeUQ, l => l.PackLineVolumeUnit);

			return (weightUnit ?? Env.Registry.FreightWeightUnit, volumeUnit ?? Env.Registry.FreightVolumeUnit);
		}

		public IEnumerable<CommonShipment> AllShipments
		{
			get { return GetAllShipmentsCore(); }
		}

		protected virtual IEnumerable<CommonShipment> GetAllShipmentsCore()
		{
			return Parent.Shipments.Cast<CommonShipment>();
		}

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				for (var i = 0; i < Parent.TopLevelShipments.Count; i++)
				{
					result.Add(MoneyType.ValueType.GoodsValue, new Money(Parent.TopLevelShipments[i].JS_GoodsValue, Parent.TopLevelShipments[i].GoodsValueCurr));
					result.Add(MoneyType.ValueType.InsuranceValue, new Money(Parent.TopLevelShipments[i].JS_InsuranceValue, Parent.TopLevelShipments[i].InsuranceCurrency));
				}

				return result;
			}
		}

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				if (overridenPaymentTerm != null)
				{
					return overridenPaymentTerm;
				}

				var infos = new PaymentTermInfos();
				if (!string.IsNullOrWhiteSpace(Parent.JK_PrepaidCollect))
				{
					infos.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Cost, Parent.JK_PrepaidCollect));
					infos.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Revenue, Parent.JK_PrepaidCollect));
				}

				return infos;
			}

			set
			{
				overridenPaymentTerm = value;
			}
		}

		PaymentTermInfos overridenPaymentTerm;

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			var costPrepaidCollect = PaymentTerm.GetPrepaidCollect(CostSell.Cost, chargeCodeGroup);
			if (string.IsNullOrEmpty(costPrepaidCollect))
			{
				return false;
			}

			var revenuePrepaidCollect = GetValueIfSameForAllShipments(x => x.RatingAdapter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, chargeCodeGroup));
			if (string.IsNullOrEmpty(revenuePrepaidCollect))
			{
				return false;
			}

			return costPrepaidCollect == revenuePrepaidCollect;
		}

		public override ServiceLevelRatingInformation ServiceLevel =>
			new ServiceLevelRatingInformation
			(
				new ServiceLevelInfo(Parent.JK_AWBServiceLevel, ServiceLevelType.Carrier),
				new ServiceLevelInfo(Parent.JK_RS_NKGatewayServiceLevel, ServiceLevelType.Gateway)
			);

		public override OrgAddress WharfCTOAddress
		{
			get
			{
				OrgAddress result;

				if (this.IsExport())
				{
					result = Parent.IsRoad ? Parent.PackDepotAddress : Parent.DepartureCTOAddress;
				}
				else
				{
					result = Parent.IsRoad ? Parent.UnpackDepotAddress : Parent.ArrivalCTOAddress;
				}
				return result;
			}
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		public decimal? WeightBreakOverride => ApplyWeightBreakOverrideForGateway ? Parent.JK_ConsolChargeable : null;

		public string WeightBreakOverrideUnit => ApplyWeightBreakOverrideForGateway ? Parent.JK_ConsolChargeableUnit : null;

		bool ApplyWeightBreakOverrideForGateway
		{
			get { return Parent.IsAir && RatingDataRegistry.Instance.GatewayBillingUseTotalWeightForWeightBreak.Value && IsGateway; }
		}

		public RateLineConditionsSupporter ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = new ConsolRateLineConditionsSupporter(Parent)); }
		}

		RateLineConditionsSupporter conditionsSupporter;

		bool IsGateway => Parent.IsGatewayBillingEnabled();

		public override ZString NamedAccount => Parent?.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)?.CE_EntryNum ?? base.NamedAccount;
	}
}
