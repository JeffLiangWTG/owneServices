using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.AutoratingViaPortHelper;

namespace Enterprise.Freight.Business
{
	public class ShipmentRatingAdapter<T> : RatingAdapter<T>,
		IAutoRatingPackageTypeInfo,
		IAutoRatingFreightConditionsSupportable,
		IAutoRatingShipmentConsolidationStatus,
		IJobDataUpdater,
		ISpotRate,
		IShipmentRatingAdapter
		where T : CommonShipment
	{
		#region Constants

		public static string RequireWeightAndVolumeOnPackLinesError =>
			Res.GetString("48397f0d-c6db-4004-a52a-63ed54fd47ea", "Both Volume Unit and Weight Unit for all packing lines are required to proceed with Autorating.");

		#endregion

		protected internal ShipmentRatingAdapter(T parent)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		protected readonly T parent;

		protected virtual JobHeader ParentJob
		{
			get { return parent.Job; }
		}

		#region RatingAdapter

		public override AdapterType AdapterType => AdapterType.Shipment;

		public override IEnumerable<ZString> CarrierContractNumbers =>
			parent.GetFirstOrCorrectConsol()?.RatingAdapter?.CarrierContractNumbers ?? Enumerable.Empty<ZString>();

		public override IEnumerable<ZString> ClientContractNumbers =>
			parent.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CLC);

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return parent.InvoicingSupporter; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new CommonShipmentJobDatesProvider(parent); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return GetChargeCodeGroups(); }
		}

		ChargeCodeGroupCollection GetChargeCodeGroups()
		{
			var result = new ChargeCodeGroupCollection();
			result.AddRange(Env.Registry.Rating.FreightRatedCodes);

			ApplyChargeCodeFilterOnChargeCodeGroups(result);

			if (parent.DeclarationForDocuments != null)
			{
				var codesToRemove = new[]
					{
						ChargeCodeGroupList.Codes.Brokerage,
						ChargeCodeGroupList.Codes.BrokerageOnly,
						ChargeCodeGroupList.Codes.CustomsDuty,
						ChargeCodeGroupList.Codes.OriginBrokerage,
						ChargeCodeGroupList.Codes.OriginBrokerageOnly
					};

				foreach (string codeToRemove in codesToRemove)
				{
					if (result.Contains(codeToRemove))
					{
						result.Remove(codeToRemove);
					}
				}
			}

			return result;
		}

		protected virtual void ApplyChargeCodeFilterOnChargeCodeGroups(ChargeCodeGroupCollection groups)
		{
			var consol = parent.GetFirstOrCorrectConsol();
			if (consol != null)
			{
				//	Product requirements for setup where a shipment is the only shipment on a consol:
				//
				//							Shipment FCL			Shipment LCL
				//			Consol FCL		all charges				non consol charges
				//			Consol LCL		non consol charges		all charges
				//

				var autorateConsolLevel = consol.TopLevelShipments.Count == 1
					&& !consol.HasConsolCosts(GlbCompany.CurrentCompany)
					&& Constants.ContainerModes.IsFCLType(consol.JK_ConsolMode) == Constants.ContainerModes.IsFCLType(parent.PackingMode);

				if (!autorateConsolLevel)
				{
					groups.CostChargesFilter = ChargeCodeFilter.AutorateNonConsolLevelOnly;
				}
			}
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Shipment; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = parent.GetJobServices();
				var allShipmentEnabledServices = result
					.Where(x => x.IsEnabled)
					.ToList();

				var containers = Parent.Containers
					.Where(c => c.JC_ContainerMode != Constants.ContainerModes.LCL)
					.Where(c => c.JC_ContainerMode != Constants.ContainerModes.BuyersConsol)
					.ToArray();

				var docsAndCartageServices = parent.DocsAndCartage.Services;
				var serviceInfos = GetServiceInfosFromJobServices(docsAndCartageServices);

				var hiddenServiceCodes = new HashSet<string>(
					FreightRatingHelper.HiddenContainerServices
						.Cast<CodeDescriptionPair>()
						.Select(service => service.Code));
				var possibleContainerServiceCodes = new HashSet<string>(hiddenServiceCodes);

				foreach (var service in serviceInfos)
				{
					if (service.IsEnabled)
					{
						possibleContainerServiceCodes.Remove(service.ServiceCode);
						service.ChargeCodeGroup = parent.GetServiceChargeGroup(service);
						result.Add(service);
					}
					else
					{
						possibleContainerServiceCodes.Add(service.ServiceCode);
					}
				}

				foreach (var serviceCode in possibleContainerServiceCodes)
				{
					bool isHidden = hiddenServiceCodes.Contains(serviceCode);
					var containerServices = FreightRatingHelper.GetServiceInfosFromContainers(containers, serviceCode, FreightRatingHelper.GetServiceInfoDefault);
					if (isHidden)
					{
						AddContainerServices(result, serviceCode, ChargeCodeGroupList.Codes.Origin, isHidden, allShipmentEnabledServices, containerServices);
						AddContainerServices(result, serviceCode, ChargeCodeGroupList.Codes.Destination, isHidden, allShipmentEnabledServices, containerServices);
					}
					else
					{
						// Non-hidden container services don't have a chargecode group set, so we just match on service code alone
						AddContainerServices(result, serviceCode, string.Empty, isHidden, allShipmentEnabledServices, containerServices);
					}
				}

				return result;
			}
		}

		void AddContainerServices(
			JobServicesCollection services,
			string serviceCode,
			string chargeCodeGroup,
			bool isHidden,
			IEnumerable<JobServiceInfo> allShipmentEnabledServices,
			IEnumerable<JobServiceInfo> containerServices)
		{
			// Some services can be defined on both the shipment and on the containers.
			// If both types exist:
			//	- for hidden services, they are all added to the result, but each type can be rated as either cost or revenue, but not both.
			//  - for non-hidden services, then only the shipment ones are added to the result, and can be rated both cost and revenue.
			// If only one type exists then they are added to the result and can be rated both cost and revenue.
			var containerServiceToMatch = containerServices.Where(x => chargeCodeGroup.Length == 0 || x.ChargeCodeGroup == chargeCodeGroup);
			if (containerServiceToMatch.Any())
			{
				var shipmentServicesToMatch = allShipmentEnabledServices
					.Where(x => x.ServiceCode == serviceCode && (chargeCodeGroup.Length == 0 || x.ChargeCodeGroup == chargeCodeGroup));
				bool isEnabledOnShipment = shipmentServicesToMatch.Any();
				if (!isEnabledOnShipment || isHidden)
				{
					if (isEnabledOnShipment && isHidden)
					{
						shipmentServicesToMatch.ForEach(x => x.IsCostOrSellForRateSearch = JobServiceInfo.CostOrSell.Sell);
						containerServiceToMatch.ForEach(x => x.IsCostOrSellForRateSearch = JobServiceInfo.CostOrSell.Cost);
					}
					services.AddRange(containerServiceToMatch);
				}
			}
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get { return GetStatusInformation(); }
		}

		AutoRatingStatusInfo GetStatusInformation()
		{
			var volumeAndWeightUnitStatusInformation = GetVolumeAndWeightUnitStatusInformation();
			if (volumeAndWeightUnitStatusInformation != null)
			{
				return volumeAndWeightUnitStatusInformation;
			}

			return parent.GetStatusInformation();
		}

		protected AutoRatingStatusInfo GetVolumeAndWeightUnitStatusInformation()
		{
			var packs = Parent.OuterPackLines.Concat(Parent.InnerPackLines).Cast<PackLine>().ToList();

			if (packs.Any(packLine => packLine.JL_ActualWeightUQ.IsEmpty || packLine.JL_ActualVolumeUQ.IsEmpty))
			{
				return new AutoRatingStatusInfo(false, RequireWeightAndVolumeOnPackLinesError);
			}

			if (!Constants.Weight.ContainsCode(parent.JS_UnitOfWeight))
			{
				string message = Res.GetString("4f61bf93-f723-4e1a-9420-9b5bff56df76", "Invalid unit of weight: '{0}'.", parent.JS_UnitOfWeight);

				return new AutoRatingStatusInfo(false, message);
			}

			if (!Constants.Volume.ContainsCode(parent.JS_UnitOfVolume))
			{
				string message = Res.GetString("066df783-66bd-4959-ba8c-b8b0a07eebb3", "Invalid unit of volume: '{0}'.", parent.JS_UnitOfVolume);

				return new AutoRatingStatusInfo(false, message);
			}

			return null;
		}

		public override ILocation Origin
		{
			get { return parent.Origin; }
		}

		public override ILocation Destination
		{
			get { return parent.Destination; }
		}

		public override ILocation GetVia(CostSell costOrSell)
		{
			var firstOrCorrectConsol = parent.GetFirstOrCorrectConsol();

			var viaCode = RatingDataRegistry.Instance.AutoratingViaPort.GetViaForShipment(
				transportMode: parent.TransportMode,
				direction:
					parent.IsExport() ? DirectionOption.Export.Code :
					parent.IsImport() ? DirectionOption.Import.Code :
					DirectionOption.All.Code,
				origin: parent.JS_RL_NKOrigin,
				destination: parent.JS_RL_NKDestination,
				firstLoad: firstOrCorrectConsol?.JK_RL_NKLoadPort,
				lastDischarge: firstOrCorrectConsol?.JK_RL_NKDischargePort,
				lastModeRouteSetDischarge: GetLastRouteSetDischarge(firstOrCorrectConsol, costOrSell)?.Code);

			return LocationHelper.GetCachedLocationFromString(viaCode, parent.Factory);
		}

		public override ILocation GetFirstLoad(CostSell costOrSell) =>
			SortedConsolsWithSameTransportMode().FirstOrDefault()?.LoadPort;

		public override ILocation GetLastDischarge(CostSell costOrSell) =>
			SortedConsolsWithSameTransportMode().LastOrDefault()?.DischargePort;

		public override ILocation GetFirstRouteSetLoad(CostSell costOrSell) =>
			GetFirstRouteSetLoad(SortedConsolsWithSameTransportMode().FirstOrDefault(), costOrSell);

		public override ILocation GetLastRouteSetDischarge(CostSell costOrSell) =>
			GetLastRouteSetDischarge(SortedConsolsWithSameTransportMode().LastOrDefault(), costOrSell);

		IEnumerable<CommonConsol> SortedConsolsWithSameTransportMode()
		{
			var legs = parent.Consols.Cast<CommonConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(legs);
			return legs.Where(c => c.TransportMode == parent.TransportMode);
		}

		ILocation GetFirstRouteSetLoad(CommonConsol consol, CostSell costOrSell)
		{
			if (consol == null)
			{
				return null;
			}

			var routes = consol.GetRatingRoutes(costOrSell);
			if (routes.IsNullOrEmpty())
			{
				return consol.Transports.FirstTransportWithTransportMode(parent.TransportMode)?.LoadPort;
			}

			return routes.FirstOrDefault(r => r.TransportMode == consol.TransportMode)?.Origin;
		}

		ILocation GetLastRouteSetDischarge(CommonConsol consol, CostSell costOrSell)
		{
			if (consol == null)
			{
				return null;
			}

			var routes = consol.GetRatingRoutes(costOrSell);
			if (routes.IsNullOrEmpty())
			{
				return consol.Transports.LastTransportWithTransportMode(parent.TransportMode)?.DiscPort;
			}

			return routes.LastOrDefault(r => r.TransportMode == consol.TransportMode)?.Destination;
		}

		public override Creditors Creditors
		{
			get { return parent.GetCreditors(); }
		}

		public override OrgHeader Carrier
		{
			get
			{
				var firstOrCorrectConsol = parent.GetFirstOrCorrectConsol();
				return firstOrCorrectConsol != null ? firstOrCorrectConsol.ShippingLine : null;
			}
		}

		public override OrgHeader ImportBroker
		{
			get { return parent.ImportBroker; }
		}

		public override OrgHeader ExportBroker
		{
			get { return parent.ExportBroker; }
		}

		public override ZString DeliveryCartageEquipment
		{
			get { return parent.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
		}

		public override ZString PickupCartageEquipment
		{
			get { return parent.DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
		}

		public override IDocAddress DeliveryAddress
		{
			get
			{
				var deliveryAddress = parent.ConsigneeDeliveryAddress;
				return !deliveryAddress.IsEmpty
					? deliveryAddress
					: parent.ConsigneeDocumentaryAddress;
			}
		}

		public override IDocAddress ConsigneeDocumentaryAddress => parent.ConsigneeDocumentaryAddress;

		public override IDocAddress PickupAddress
		{
			get
			{
				var pickupAddress = parent.ConsignorPickupAddress;
				return !pickupAddress.IsEmpty
					? pickupAddress
					: parent.ConsignorDocumentaryAddress;
			}
		}

		public override IDocAddress ConsignorDocumentaryAddress => parent.ConsignorDocumentaryAddress;

		public override ZString HBLDeliveryMode => parent.JS_HBLContainerPackModeOverride;

		public override FreightMode FreightMode
		{
			get
			{
				if (!isContainerized.HasValue)
				{
					isContainerized = ((RateableMeasureSet)RateableMeasures)?.IsContainerized() ?? false;
				}

				return FreightRatingHelper.CalculateFreightMode(parent.JS_TransportMode, ContainerMode, isContainerized.Value);
			}
		}

		public override ZString ContainerMode
		{
			get { return parent.JS_PackingMode; }
		}

		public override ZString HousebillReleaseType
		{
			get { return parent.JS_ReleaseType; }
		}

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				if (overridenPaymentTerm != null)
				{
					return overridenPaymentTerm;
				}

				var paymentTerm = new PaymentTermInfos();

				if (!string.IsNullOrWhiteSpace(parent.JS_INCO))
				{
					var type = this.IsDomestic() ? PaymentTermType.DomesticPaymentTerm : PaymentTermType.Incoterm;
					paymentTerm.AddOrReplace(new PaymentTermInfo(type, CostSell.Cost, parent.JS_INCO));
					paymentTerm.AddOrReplace(new PaymentTermInfo(type, CostSell.Revenue, parent.JS_INCO));
				}

				return paymentTerm;
			}

			set
			{
				overridenPaymentTerm = value;
			}
		}

		protected PaymentTermInfos overridenPaymentTerm;

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			var consolPaymentTerm = parent.GetFirstOrCorrectConsol()?.RatingAdapter?.PaymentTerm;
			var consolPrepaidCollect = consolPaymentTerm.GetPrepaidCollect(costOrSell, chargeCodeGroup);
			if (string.IsNullOrEmpty(consolPrepaidCollect))
			{
				return false;
			}

			var shipmentPrepaidCollect = PaymentTerm.GetPrepaidCollect(CostSell.Revenue, chargeCodeGroup);
			if (string.IsNullOrEmpty(shipmentPrepaidCollect))
			{
				return false;
			}

			return consolPrepaidCollect == shipmentPrepaidCollect;
		}

		public override ZString FMCTariffID
		{
			get
			{
				return parent.JS_FMCTariffID;
			}
		}

		public override IEnumerable<RefCommodityCode> OverriddenCommodity
		{
			get
			{
				if (parent.RateCommodity != null)
				{
					return new[] { parent.RateCommodity };
				}
				return Enumerable.Empty<RefCommodityCode>();
			}
		}

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				var packs = GetPackages(null, fallbackToShipmentMeasures: true);
				var units = GetUnits();
				var chargeable = GetChargeable();
				var innerPacks = GetInnerPackages();
				var innerUnits = GetInnerUnits();
				var shipments = new JobLevelPart { ShipmentCount = parent.JS_JS_ColoadMasterShipment.IsEmpty ? 1 : 0 };
				var lowestBills = new JobLevelPart { LowestBillCount = parent.CoLoadShipments.Count };

				result.AddPartList(MeasureType.Weight, packs);
				result.AddPartList(MeasureType.Volume, packs);
				result.AddPartList(MeasureType.Package, packs);
				result.AddPartList(MeasureType.LoadingMeters, packs);
				result.AddPartList(MeasureType.Unit, units);
				result.AddPartList(MeasureType.InnerPacksWeight, innerPacks);
				result.AddPartList(MeasureType.InnerPacksVolume, innerPacks);
				result.AddPartList(MeasureType.InnerPacksPackage, innerPacks);
				result.AddPartList(MeasureType.InnerPacksUnit, innerUnits);
				result.AddPartList(MeasureType.Chargeable, chargeable);
				result.AddPartList(MeasureType.Shipment, shipments);
				result.AddPartList(MeasureType.LowestBill, lowestBills);

				// TODO: Refactor Distance and Container measures to follow the same approach as measures above.
				// I.e. it must be clear what measure types are added, with what what attributes and have they are
				// populated. Currently, the only way to figure out is to reverse engineer dozens of helper methods.
				SetDistanceMeasuresCore(result);
				SetContainersCore(result);

				OverrideCommodityIfNeeded(result);

				isContainerized = result.IsContainerized();
				return result;
			}
		}

		void OverrideCommodityIfNeeded(RateableMeasureSet result)
		{
			var overriddenCommodity = OverriddenCommodity.FirstOrDefault();
			if (overriddenCommodity != null)
			{
				result.UpdateCommodities(overriddenCommodity.RH_Code);
			}
		}

		bool? isContainerized;

		#region Packages, Weight, Volume

		/// <summary>
		///		Returns allocated and unallocated packages in <paramref name="consol"/>.
		///		If the consol is not specified, the first or correct consol will be used.
		/// </summary>
		/// <param name="consol">
		///		A pack line may be packed in multiple containers in multiple consols.
		///		For shipment rating adapter it doesn't matter, but for consol rating adapter which calls
		///		this method as well to get packates packed in its containers, we want packages to
		///		contain attributes of the container from those consol, not form another one.
		///
		///		Also, if a the packline is packed in another container on another consol, for the consol
		///		calling this method, the pack line remains unallocated, so, we want a container attributes
		///		to be empty in this case rather than containing details of another container from another consol.
		/// </param>
		/// <param name="fallbackToShipmentMeasures">
		///		Identifies if we should create a fake package part using shipment measures if the shipment doesn't have pack lines at all.
		/// </param>
		public virtual RateablePartList GetPackages(CommonConsol consol = null, bool fallbackToShipmentMeasures = true)
		{
			var packages = new RateablePartList();
			packages.HasContainerType = true;
			packages.HasCommodity = true;
			packages.WeightUnit = WeightUnit;
			packages.VolumeUnit = VolumeUnit;

			if (Parent.OuterPackLines.Count > 0)
			{
				if (Parent.OuterPackLines.Count > 1)
				{
					foreach (PackLine packLine in Parent.OuterPackLines)
					{
						var part = GetPackagePartFromPackLine(packLine, consol);
						packages.AddPart(part);
					}
				}
				else
				{
					// We have 1 pack line. In this case we take measures from shipment itself (why?).
					var packLine = Parent.OuterPackLines.Cast<PackLine>().Single();

					var part = GetPackagePartFromShipmentMeasures(Parent, consol, packLine);
					packages.AddPart(part);
				}
			}
			else if (fallbackToShipmentMeasures)
			{
				var part = GetPackagePartFromShipmentMeasures(Parent, null, null);
				packages.AddPart(part);
			}

			return packages;
		}

		RateablePartList GetInnerPackages(CommonConsol consol = null)
		{
			var packages = new RateablePartList();
			packages.WeightUnit = WeightUnit;
			packages.VolumeUnit = VolumeUnit;

			// Inner packs don't have their own commodity. But if they are linked to outer packs, they inherit commodity from the inner packs.
			// So, only linked inner packs can have commodity.
			var isLinked = Parent.InnerPackLines.Cast<PackLine>().Any(p => !p.JL_JL_OuterPackLine.IsEmpty);
			packages.HasCommodity = isLinked;

			foreach (PackLine packLine in Parent.InnerPackLines)
			{
				var part = GetPackagePartFromPackLine(packLine, consol);
				packages.AddPart(part);
			}

			return packages;
		}

		protected RateablePart GetPackagePartFromPackLine(PackLine packLine, CommonConsol consol)
		{
			var part = new RateablePart();
			part.Weight = FreightRatingHelper.Convert(packLine.JL_ActualWeight, packLine.PackLineWeightUnit, WeightUnit);
			part.Volume = FreightRatingHelper.Convert(packLine.JL_ActualVolume, packLine.PackLineVolumeUnit, VolumeUnit);
			part.LoadingMeter = packLine.JL_LoadingMeters;
			part.PackageCount = packLine.JL_PackageCount;

			PopulatePackagePartAttributes(part, packLine, consol);

			return part;
		}

		RateablePart GetPackagePartFromShipmentMeasures(CommonShipment shipment, CommonConsol consol, PackLine singlePackLine)
		{
			var part = new RateablePart();
			part.WeightMeasure = new ClientProviderValues(shipment.JS_ActualWeight, shipment.JS_DocumentedWeight, shipment.JS_ManifestedWeight);
			part.VolumeMeasure = new ClientProviderValues(shipment.JS_ActualVolume, shipment.JS_DocumentedVolume, shipment.JS_ManifestedVolume);
			part.LoadingMeterMeasure = new ClientProviderValues(shipment.JS_LoadingMeters, shipment.JS_DocumentedLoadingMeters, shipment.JS_ManifestedLoadingMeters);
			part.PackageCount = shipment.JS_OuterPacks;

			if (singlePackLine != null)
			{
				PopulatePackagePartAttributes(part, singlePackLine, consol);
			}

			return part;
		}

		void PopulatePackagePartAttributes(RateablePart part, PackLine packLine, CommonConsol consol)
		{
			var container = GetContainerFromPackLineForAutoRatingCore(packLine, consol);
			if (container != null)
			{
				part.ContainerPK = container.PK.ToGuid();
				part.ContainerTypePk = container.JC_RC.IsEmpty ? Guid.Empty : container.JC_RC.ToGuid();
				part.ContainerNumber = container.JC_ContainerNum;
				part.PivotBreak = container.JC_PivotBreak;
			}

			// Inner pack lines don't have their commodity, but they can be linked to
			// outer pack lines and thus can inherit commodity from them.
			var outerPackLine = packLine.IsInnerPackType && !packLine.JL_JL_OuterPackLine.IsEmpty
				? packLine.OuterPackLine
				: packLine;

			SetPacklineCommodity(part, outerPackLine.JL_RH_NKCommodityCode, container?.CommodityCodeForRating, consol != null);

			if (!string.IsNullOrEmpty(part.ContainerNumber) || !string.IsNullOrEmpty(packLine.JL_RefNumber))
			{
				part.PackageCountReference = FormattableString.Invariant($"{part.ContainerNumber}/{packLine.JL_RefNumber}");
			}
		}

		void SetPacklineCommodity(RateablePart part, string outerPackCommdoity, string containerCommodity, bool isAutoratingFromConsol)
		{
			part.CommodityCode = isAutoratingFromConsol && !containerCommodity.IsNullOrEmpty()
								? containerCommodity
								: outerPackCommdoity;
		}

		#endregion

		#region Units (i.e. Box, Pellets, etc)

		public virtual RateablePartList GetUnits(CommonConsol consol = null)
		{
			var units = new RateablePartList();
			units.HasContainerType = true;
			units.HasPackageType = true;

			foreach (var packLine in Parent.OuterPackLines.Where(l => l.JL_PackageCount > 0))
			{
				var part = GetUnitPartFromPackLine(packLine, consol);
				units.AddPart(part);
			}

			// This is a hack!
			//
			// Distance units also use Unit measure type for calculation. So, a job must have at least 1 part of Unit measure type.
			// Otherwise, the autorating will filter out the per distance line as it won't find any Unit measure on the job.
			// The distance calculator doesn't care about Unit measure at all, it will calculate the distance from the Criteria
			// by itself, but we still need to add a fake one so that the line is not filtered out.
			//
			// Why distance units use Unit measure? God knows, it is a very old logic. It is something to refactor.
			if (!units.Any())
			{
				units.HasPackageType = false;
				units.HasContainerType = false;
				units.AddPart(new RateableContainer());
			}

			return units;
		}

		RateablePartList GetInnerUnits(CommonConsol consol = null)
		{
			var units = new RateablePartList();
			units.HasPackageType = true;

			foreach (var packLine in Parent.InnerPackLines.Where(l => l.JL_PackageCount > 0))
			{
				var part = GetUnitPartFromPackLine(packLine, consol);
				units.AddPart(part);
			}

			return units;
		}

		protected RateableContainer GetUnitPartFromPackLine(PackLine packLine, CommonConsol consol)
		{
			var part = new RateableContainer();
			part.ContainerWeightInKG = FreightRatingHelper.Convert(packLine.JL_ActualWeight, packLine.PackLineWeightUnit, Constants.Weight.Kilograms);
			part.ContainerVolumeInM3 = FreightRatingHelper.Convert(packLine.JL_ActualVolume, packLine.PackLineVolumeUnit, Constants.Volume.CubicMetres);
			part.UnitCount = packLine.JL_PackageCount;
			part.PackageType = packLine.JL_F3_NKPackType;
			part.RefNumber = packLine.JL_RefNumber;

			var container = GetContainerFromPackLineForAutoRatingCore(packLine, consol);
			if (container != null)
			{
				part.TEU = container.RefContainer?.RC_TEU ?? 0;
				part.ContainerNumber = container.JC_ContainerNum;
				part.ContainerTypePk = container.JC_RC.IsEmpty ? Guid.Empty : container.JC_RC.ToGuid();
			}

			// For CMB calculator. When calculating weight/volume per top pack, it uses IRateableContainer and
			// ContainerCount rather than UnitCount as packages in this context are historically treated as containers.
			// Something to refactor I guess.
			part.ContainerCount = packLine.JL_PackageCount;
			part.ContainerPackages = packLine.JL_PackageCount;

			return part;
		}

		#endregion

		#region Chargeable

		JobLevelPart GetChargeable()
		{
			var commodity = Parent.OuterPackLines.Cast<PackLine>().SameOrDefault(p => p.JL_RH_NKCommodityCode);
			var chargeable = new ClientProviderValues(
				parent.JS_ActualChargeable,
				parent.JS_DocumentedChargeable,
				parent.JS_ManifestedChargeable);

			return new JobLevelPart
			{
				CommodityCode = commodity,
				ChargeableUnit = parent.JS_ChargeableUnit,
				ChargeableMeasure = chargeable
			};
		}

		#endregion

		protected string WeightUnit => Parent.JS_UnitOfWeight.IsEmpty
			? Env.Registry.FreightWeightUnit
			: Parent.JS_UnitOfWeight.ToString();

		protected string VolumeUnit => Parent.JS_UnitOfVolume.IsEmpty
			? Env.Registry.FreightVolumeUnit
			: Parent.JS_UnitOfVolume.ToString();

		#endregion

		protected virtual void SetDistanceMeasuresCore(RateableMeasureSet measures)
		{
			var serviceLevel = parent.ServiceLevel;
			if (serviceLevel != null && serviceLevel.RS_IsDoorToDoor)
			{
				foreach (Transport transport in parent.Transports)
				{
					if (transport.JW_TransportType == Constants.TransportPlanningType.MainVessel)
					{
						measures.SetPickupDistance(transport.JW_Distance, transport.JW_DistanceUnit);
						measures.SetDeliveryDistance(transport.JW_Distance, transport.JW_DistanceUnit);
						break;
					}
				}
			}
			else
			{
				SetOriginConfirmationDistanceMeasures(measures);
				SetDestinationConfirmationDistanceMeasures(measures);
			}
		}

		void SetOriginConfirmationDistanceMeasures(RateableMeasureSet measures)
		{
			decimal largestDistance = 0m;
			string largestDistanceUnit = Constants.Length.Kilometres;

			if (parent.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup))
			{
				foreach (CommonContainer container in parent.Containers)
				{
					SetLargestDistance(ref largestDistance, ref largestDistanceUnit, container.OriginGetConfirm);
				}
			}
			else
			{
				foreach (CommonPickupDeliveryConfirm confirm in parent.PickupConfirms)
				{
					SetLargestDistance(ref largestDistance, ref largestDistanceUnit, confirm);
				}
			}

			measures.SetPickupDistance(largestDistance, largestDistanceUnit);
		}

		void SetDestinationConfirmationDistanceMeasures(RateableMeasureSet measures)
		{
			decimal largestDistance = 0m;
			string largestDistanceUnit = Constants.Length.Kilometres;

			if (parent.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery))
			{
				foreach (CommonContainer container in parent.Containers)
				{
					SetLargestDistance(ref largestDistance, ref largestDistanceUnit, container.DestinationGetConfirm);
				}
			}
			else
			{
				foreach (CommonPickupDeliveryConfirm confirm in parent.DeliveryConfirms)
				{
					SetLargestDistance(ref largestDistance, ref largestDistanceUnit, confirm);
				}
			}

			measures.SetDeliveryDistance(largestDistance, largestDistanceUnit);
		}

		static void SetLargestDistance(ref decimal largestDistance, ref string largestDistanceUnit, CommonPickupDeliveryConfirm confirm)
		{
			if (confirm != null && Constants.Length.ContainsCode(confirm.EU_DistanceUnit))
			{
				decimal distanceInResultUnit = Constants.Length.Convert(confirm.EU_Distance, confirm.EU_DistanceUnit, largestDistanceUnit);

				if (distanceInResultUnit > largestDistance)
				{
					largestDistance = confirm.EU_Distance;
					largestDistanceUnit = confirm.EU_DistanceUnit;
				}
			}
		}

		protected virtual CommonContainer GetContainerFromPackLineForAutoRatingCore(PackLine packLine, CommonConsol consol)
		{
			if (packLine.IsInnerPackType)
			{
				return null;
			}

			if (consol != null)
			{
				return packLine.GetContainer(consol);
			}

			var containers = packLine.Containers.Cast<CommonContainer>().ToArray();
			return containers.Length == 1 ? containers.First() : packLine.GetContainer(parent.GetFirstOrCorrectConsol());
		}

		protected virtual void SetContainersCore(RateableMeasureSet measures)
		{
			var consol = Parent.GetFirstOrCorrectConsol();

			if (Parent.IsOnlyShipmentInConsol() && Parent.UnallocatedContainers().Count > 0)
			{
				var containers = consol.Containers.Cast<CommonContainer>().ToList();

				foreach (var container in containers)
				{
					var containerInfo = FreightRatingHelper.CreateContainerInfo(container, Parent);

					measures.AddContainerGroup(
						containerTypePK: container.JC_RC,
						commodity: container.JC_RH_NKContainerCommodityCode,
						containerOwnership: container.GetOwnership(),
						containerNumber: container.JC_ContainerNum,
						weight: container.JC_Calc_TotalWeightInKgs,
						volume: container.JC_Calc_TotalVolumeInM3,
						containerInfos: new[] { containerInfo });
				}
			}
			else
			{
				var packLines = parent.OuterPackLines.Cast<PackLine>().ToList();

				FreightRatingHelper.SetContainersFromPackLines(measures, consol, parent, packLines);
			}
		}

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				result.Add(MoneyType.ValueType.GoodsValue, new Money(parent.JS_GoodsValue, parent.GoodsValueCurr));
				result.Add(MoneyType.ValueType.InsuranceValue, new Money(parent.JS_InsuranceValue, parent.InsuranceCurrency));
				return result;
			}
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var firstOrCorrectConsol = parent.GetFirstOrCorrectConsol();

				ZString carrierServiceLevel = firstOrCorrectConsol != null
												? firstOrCorrectConsol.JK_AWBServiceLevel
												: parent.JS_RS_NKServiceLevel;

				return new ServiceLevelRatingInformation(
					new ServiceLevelInfo(parent.JS_RS_NKServiceLevel, ServiceLevelType.Client),
					new ServiceLevelInfo(carrierServiceLevel, ServiceLevelType.Carrier));
			}
		}

		public override OrgAddress WharfCTOAddress
		{
			get
			{
				OrgAddress result = null;

				var firstOrCorrectConsol = parent.GetFirstOrCorrectConsol();

				if (firstOrCorrectConsol != null)
				{
					var isConsolDischargedLocally = ImportExportHelper.IsBranchCountry(firstOrCorrectConsol.JK_RL_NKDischargePort);

					if (parent.IsRoad)
					{
						result = isConsolDischargedLocally
									? parent.Factory.Load<OrgAddress>(parent.CartageDeliveryDepotAddress)
									: parent.Factory.Load<OrgAddress>(parent.CartagePickupDepotAddress);
					}
					else
					{
						result = isConsolDischargedLocally
									? firstOrCorrectConsol.ArrivalCTOAddress
									: firstOrCorrectConsol.DepartureCTOAddress;
					}
				}

				return result;
			}
		}

		public override Directions JobDirection
		{
			get { return parent.JobDirection; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				result[Registry.Business.RatingDebtorOrgTypes.CNR] = parent.Consignor;
				result[Registry.Business.RatingDebtorOrgTypes.CNE] = parent.Consignee;
				result[Registry.Business.RatingDebtorOrgTypes.CCUS] = parent.ControllingCustomer;
				return result;
			}
		}

		#endregion

		#region ISpotRate

		public SpotRateInfo SellSpotRateInfo
		{
			get { return GetSellSpotRateInfoCore(); }
		}

		protected virtual SpotRateInfo GetSellSpotRateInfoCore()
		{
			if (ParentJob == null || ParentJob.Department == null || string.IsNullOrEmpty(ParentJob.Department.GE_Code))
			{
				return new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.ClientRate);
			}

			if (ParentJob.Department.IsGatewayDepartment && FreightRatingHelper.HasConsolSendingAgentActingAsGatewayInAnyCompany(parent))
			{
				var money = new Money(parent.JS_GatewayFreightSellRate, parent.GatewayFreightSellRateCurrency);
				return new SpotRateInfo(money, parent.JS_FreightGatewaySellRateAutoratingMode, AutoratedValueType.GatewaySell);
			}
			else
			{
				var money = new Money(parent.JS_UnitFreightRate, parent.FrtRateCurrency);
				return new SpotRateInfo(money, parent.JS_FreightSpotRateAutoratingMode, AutoratedValueType.SpotRate);
			}
		}

		public SpotRateInfo CostSpotRateInfo
		{
			get { return GetCostSpotRateInfoCore(); }
		}

		protected virtual SpotRateInfo GetCostSpotRateInfoCore()
		{
			SpotRateInfo result;
			OrgHeader creditor = null;

			if (ParentJob == null || ParentJob.Department == null || string.IsNullOrEmpty(ParentJob.Department.GE_Code))
			{
				return new SpotRateInfo(Money.Invalid, Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
			}

			var consols = parent.Consols.Cast<CommonConsol>().Where(FreightRatingHelper.IsConsolSendingAgentActingAsGatewayInAnyCompany);
			if (!ParentJob.Department.IsGatewayDepartment && consols.Any())
			{
				var money = new Money(parent.JS_GatewayFreightSellRate, parent.GatewayFreightSellRateCurrency);
				var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				var gatewayConsols = consols.Where(x => x.LoadPort != null && x.LoadPort.RL_RN_NKCountryCode == countryCode).ToList();
				if (gatewayConsols.Count == 1)
				{
					creditor = gatewayConsols[0].SendingForwarder;
				}

				result = new SpotRateInfo(money, parent.JS_FreightGatewaySellRateAutoratingMode, AutoratedValueType.GatewaySell) { Creditor = creditor };
			}
			else
			{
				var money = new Money(parent.JS_FreightCostRate, parent.FreightCostRateCurrency);
				var consol = parent.GetFirstOrCorrectConsol();

				if (consol != null)
				{
					creditor = consol.Creditor;
				}

				result = new SpotRateInfo(money, parent.JS_FreightCostRateAutoratingMode, AutoratedValueType.NegotiatedCost) { Creditor = creditor };
			}

			return result;
		}

		#endregion

		#region IAutoRatingPackageTypeInfo Members

		public List<PackageInformation> PackageInformation
		{
			get
			{
				var result = new List<PackageInformation>();
				foreach (PackLine line in parent.OuterPackLines)
				{
					string description = line.JL_Description.IsEmpty ? PackTypeList.GetDescriptionFromCode(line.JL_F3_NKPackType) : line.JL_Description.ToString();
					result.Add(new PackageInformation(line.PK.ToGuid(), description, line.JL_PackageCount, line.JL_ActualVolume, line.JL_ActualVolumeUQ, line.JL_RH_NKCommodityCode));
				}

				return result;
			}
		}

		RefPackTypeCollection PackTypeList
		{
			get { return fPackTypeList ?? (fPackTypeList = new RefPackTypeCollection(parent.Factory)); }
		}

		RefPackTypeCollection fPackTypeList;

		#endregion

		#region IAutoRatingFreightConditionsSupportable Members

		public RateLineConditionsSupporter ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = GetConditionsSupporterCore()); }
		}

		protected virtual ShipmentRateLineConditionsSupporter GetConditionsSupporterCore()
		{
			return new ShipmentRateLineConditionsSupporter(parent);
		}

		RateLineConditionsSupporter conditionsSupporter;

		#endregion

		#region IAutoRatingShipmentConsolidationStatus Members

		public virtual ZString ShipmentConsolidationStatus =>
			Parent.Consols.Any()
				? Constants.ShipmentConsolidationStatus.Codes.ConsolidatedShipment
				: Constants.ShipmentConsolidationStatus.Codes.StandaloneShipment;

		#endregion

		#region IJobDataUpdater

		bool IJobDataUpdater.CanUpdateDate => false;

		void IJobDataUpdater.UpdateServiceLevel(ZString newServiceLevel) { }

		void IJobDataUpdater.UpdateCarrier(OrgHeader newCarrier) { }

		void IJobDataUpdater.UpdateRateCommodityCodeAndFMCTariffID(ZString newRateCommodityCode, ZString newFMCTariffID)
		{
			parent.JS_RH_NKRateCommodity = newRateCommodityCode;
			parent.JS_FMCTariffID = newFMCTariffID;
		}

		void IJobDataUpdater.UpdateDetailedGoodsDescription(ZString newDescription, bool append)
		{
			if (append && !parent.DetailedGoodsDescriptionNoteText.IsEmpty)
			{
				parent.DetailedGoodsDescriptionNoteText += System.Environment.NewLine + newDescription;
			}
			else
			{
				parent.DetailedGoodsDescriptionNoteText = newDescription;
			}
		}

		bool IJobDataUpdater.UpdateCarrierConfirmationIsNeeded(string newServiceProvider, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateOrigin(ZString newOrigin) { }

		bool IJobDataUpdater.UpdateOriginConfirmationIsNeeded(string newOrigin, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateDestination(ZString newDestination) { }

		bool IJobDataUpdater.UpdateDestinationConfirmationIsNeeded(string newDestination, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExistingDuplicates) { }

		bool IJobDataUpdater.UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdatePaymentTerms(ZString newPaymentTerms) { }

		void IJobDataUpdater.UpdateNamedAccount(ZString namedAccount) { }

		void IJobDataUpdater.UpdateCarrierQuoteNumber(ZString carrierQuoteNumber) { }

		void IJobDataUpdater.UpdateContainersCarrierQuoteNumber(ZGuid containerRefPK, ZString carrierQuoteNumber) { }

		void IJobDataUpdater.UpdateSpotBookingTerms(ZString termsAsText) { }

		public virtual void UpdateAutoratingDate(ZDate autoratingDate, bool isCosting) { }

		public virtual CanUpdateCarrierContractNumberResult CanUpdateCarrierContractNumber(IEnumerable<string> contractNumbers, IDialogService dialogService = null, bool isManualCostSelected = false)
		{
			return new CanUpdateCarrierContractNumberResult(contractNumbers);
		}

		public virtual bool IsMultipleCarrierContractNumberSupported => true;

		public virtual DataUpdateResult UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token) => DataUpdateResult.NoAction;

		public virtual DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers)
		{
			return DataUpdateResult.NoAction;
		}

		void IJobDataUpdater.UpdateTransports(IEnumerable<ITransport> transports) { }

		public virtual void UpdateChargeable(ZDecimal newChargeable)
		{
			parent.JS_ActualChargeable = newChargeable;
		}

		public void SendBookingInformationToCarrier() { }

		bool IJobDataUpdater.IsMultipleClientContractNumberSupported => false;

		#endregion
	}

	public interface IShipmentRatingAdapter
	{
		/// <summary>
		///		Returns shipment package parts included in the consol. Those parts which are packed in the consol, will have
		///		corresponding container attributes. If a packline is packed in another container on another consol, it will
		///		not have container attributes and for this <paramref name="consol"/> will be counted as unpacked.
		/// </summary>
		/// <param name="consol">
		///		A consol for which to return packs.
		/// </param>
		/// <param name="fallbackToShipmentMeasures">
		///		Identifies if we should create a fake package part using shipment measures if the shipment doesn't have pack lines at all.
		/// </param>
		/// <returns></returns>
		RateablePartList GetPackages(CommonConsol consol, bool fallbackToShipmentMeasures);
		RateablePartList GetUnits(CommonConsol conol);
	}
}
