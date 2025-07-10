using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingRatingAdapter : RatingAdapter<DtbBooking>,
		IJobNumber,
		IAutoRatingDescriptionMacroExpander,
		IAutoRatingFreightConditionsSupportable
	{
		public DtbBookingRatingAdapter(DtbBooking booking, FreightMode freightMode, DtbBookingInstruction fromInstruction, DtbBookingInstruction toInstruction)
			: base(booking)
		{
			this.freightMode = freightMode;
			this.fromInstruction = fromInstruction;
			this.toInstruction = toInstruction;
		}

		readonly DtbBookingInstruction fromInstruction;
		readonly DtbBookingInstruction toInstruction;
		readonly FreightMode freightMode;

		ZString RatingFreightMode => Parent.KM_RatingFreightMode;

		JobDocAddress FromAddress
		{
			get
			{
				var fromInstruction = FromInstruction;
				return fromInstruction != null ? fromInstruction.Address : null;
			}
		}

		JobDocAddress ToAddress
		{
			get
			{
				var toInstruction = ToInstruction;
				return toInstruction != null ? toInstruction.Address : null;
			}
		}

		DtbBooking Booking
		{
			get { return Parent; }
		}

		JobDocAddress TransportCo
		{
			get { return Booking.Address; }
		}

		DtbBookingInstruction FromInstruction
		{
			get { return fromInstruction; }
		}

		DtbBookingInstruction ToInstruction
		{
			get { return toInstruction; }
		}

		/// <summary>
		/// Return chargeable quantity as amount and unit tuple.
		/// </summary>
		public Tuple<ZDecimal, ZString> GetChargableMeasure()
		{
			return (Booking != null) ? Tuple.Create(Booking.KM_Chargeable, Booking.ChargeableUnits) : null;
		}

		public override AdapterType AdapterType => AdapterType.TransportBooking;

		string GetUnknownStatusInformation()
		{
			var result = string.Empty;

			if (Booking.KM_RatingFreightMode.IsEmpty)
			{
				result = Res.GetString("80F8CA89-ADA5-4BBF-B3D9-EFE941EB0D7A", "Freight Mode for {0} is empty. Cannot continue.", Parent.HumanReadableName);
			}
			else if (fromInstruction != null && toInstruction != null)
			{
				result = Res.GetString("8D1AF057-56CB-4FA1-A28E-AC8C879C1225", "Cannot find related rates, please check if packages exist and if they've been assigned.");
			}

			return result;
		}

		#region IAutoRatingOrganisations Member
		[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1124:DoNotUseRegions", Justification = "Requested in Aspect Review by Ratings.")]

		public override OrgHeader Carrier
		{
			get
			{
				var address = TransportCo;
				return address != null ? address.Organisation : null;
			}
		}

		public override Creditors Creditors
		{
			get
			{
				var carrier = OrgWithSource.NewFrom<OrgHeader>(TransportCo.OrganisationPKInfo);
				return carrier != null ? Creditors.New(new[] { carrier }) : new Creditors();
			}
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();

				if (InvoicingSupporter != null && InvoicingSupporter.Job != null)
				{
					if (InvoicingSupporter.Job.LocalCharges != null)
					{
						result[RatingDebtorOrgTypes.LC] = InvoicingSupporter.Job.LocalCharges;
					}

					if (InvoicingSupporter.Job.AgentCollect != null)
					{
						result[RatingDebtorOrgTypes.AG] = InvoicingSupporter.Job.AgentCollect;
					}
				}

				if (FromInstruction?.Address != null)
				{
					result[Registry.Business.RatingDebtorOrgTypes.CNR] = FromInstruction.Address.Organisation;
				}

				if (ToInstruction?.Address != null)
				{
					result[Registry.Business.RatingDebtorOrgTypes.CNE] = ToInstruction.Address.Organisation;
				}

				return result;
			}
		}

		#endregion

		#region IAutoRatingFreightInfo Members
		[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1124:DoNotUseRegions", Justification = "Requested in Aspect Review by Ratings.")]

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				return new ServiceLevelRatingInformation(
					new ServiceLevelInfo(Booking.KM_RS_NKServiceLevel, ServiceLevelType.Client),
					new ServiceLevelInfo(Booking.KM_PL_NKCarrierServiceLevel, ServiceLevelType.Carrier)
					);
			}
		}

		#endregion

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return ((IJobInvoicingPlugIn)Parent).InvoicingSupporter; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return GetJobDatesProviderCore(); }
		}

		IJobDatesProvider GetJobDatesProviderCore()
		{
			return new DtbBookingJobDatesProvider(Booking);
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var chargeCodeGroups = new ChargeCodeGroupCollection();

				chargeCodeGroups.Add(ChargeCodeGroupList.Codes.TransportBooking);
				if (HasConsolCosts)
				{
					chargeCodeGroups.CostChargesFilter = ChargeCodeFilter.AutorateNonConsolLevelOnly;
				}

				return chargeCodeGroups;
			}
		}

		bool HasConsolCosts
		{
			get { return false; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return ConsumerTypeCore; }
		}

		JobInvoicingConsumerType ConsumerTypeCore
		{
			get { return JobInvoicingConsumerTypes.TransportBooking; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.TransportBookings; }
		}

		public override AutoRatingStatusInfo StatusInformation => GetStatusInformation();

		AutoRatingStatusInfo GetStatusInformation()
		{
			if (FromInstruction == null || ToInstruction == null)
			{
				return new AutoRatingStatusInfo(false, Res.GetString("04D25BCB-D057-4DD7-BD19-14DD59A980B2", "{0} doesn't have at least 2 Instructions. Cannot continue.", Parent.HumanReadableName));
			}

			if (HasInvalidDate(FromInstruction.Confirmations, true) || HasInvalidDate(ToInstruction.Confirmations, true))
			{
				return new AutoRatingStatusInfo(false, Res.GetString("CA120F88-9B80-4F0D-B545-E294F860CFD0", "Please enter valid Actual date."));
			}

			if (HasInvalidDate(FromInstruction.Confirmations, false) || HasInvalidDate(ToInstruction.Confirmations, false))
			{
				return new AutoRatingStatusInfo(false, Res.GetString("855ACEBD-2600-4C70-A5AD-7F94F9342B8E", "Please enter valid Estimated date."));
			}

			var volumeAndWeightUnitStatusInformation = GetVolumeAndWeightUnitStatusInformation();
			return volumeAndWeightUnitStatusInformation ?? GetAdditionalStatusInformation();
		}

		bool HasInvalidDate(DtbBookingConfirmationCollection items, bool actualElseEstimated)
		{
			return items.Cast<DtbBookingConfirmation>().Any(x =>
			{
				var d = actualElseEstimated ? x.KK_Actual : x.KK_Estimated;
				return !d.IsEmpty && !d.IsValid;
			});
		}

		AutoRatingStatusInfo GetAdditionalStatusInformation()
		{
			return (FreightMode == FreightMode.UKN)
				? new AutoRatingStatusInfo(false, GetUnknownStatusInformation())
				: new AutoRatingStatusInfo(true, "");
		}

		AutoRatingStatusInfo GetVolumeAndWeightUnitStatusInformation()
		{
			AutoRatingStatusInfo result = null;

			var invalidPackage = CommonPackages.Select(p => new
			{
				IsWeightValid = Constants.Weight.ContainsCode(p.KP_WeightUQ),
				IsVolumeValid = Constants.Volume.ContainsCode(p.KP_VolumeUQ),
				WeightUnit = p.KP_WeightUQ,
				VolumeUnit = p.KP_VolumeUQ
			}).FirstOrDefault(o => !o.IsWeightValid || !o.IsVolumeValid);

			if (invalidPackage != null)
			{
				var message = !invalidPackage.IsWeightValid
					? Res.GetString("66D49798-02DF-4DD2-8F6E-94CF1B9228D3", "Invalid unit of weight: '{0}'.", invalidPackage.WeightUnit)
					: Res.GetString("5E0F2EC3-7738-4979-9973-2C9393D18593", "Invalid unit of volume: '{0}'.", invalidPackage.VolumeUnit);

				result = new AutoRatingStatusInfo(false, message);
			}

			return result;
		}

		/// <summary>
		/// Used as Location
		/// </summary>
		public override ILocation Origin
		{
			get
			{
				ILocation result = null;
				var fromAddress = FromAddress;
				if (fromAddress != null)
				{
					if (fromAddress.E2_AddressOverride)
					{
						result = fromAddress.Country;
					}
					else
					{
						var address = fromAddress.Address;
						if (address != null)
						{
							var relatedPort = address.EffectiveRelatedPortCode;
							if (relatedPort != null)
							{
								result = relatedPort;
							}
						}
					}
				}

				return result;
			}
		}

		public override IDocAddress PickupAddress
		{
			get { return FromAddress; }
		}

		public override ZString PickupCartageEquipment
		{
			get
			{
				var fromInstruction = FromInstruction;
				return fromInstruction != null ? fromInstruction.KN_DropMode : ZString.Empty;
			}
		}

		public override IDocAddress DeliveryAddress
		{
			get { return ToAddress; }
		}

		public override ZString DeliveryCartageEquipment
		{
			get
			{
				var toInstruction = ToInstruction;
				return toInstruction != null ? toInstruction.KN_DropMode : ZString.Empty;
			}
		}

		public override FreightMode FreightMode
		{
			get { return freightMode; }
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				if ((FreightMode & FreightMode.LSE) != 0)
				{
					SetLooseMeasures(result);
				}
				else if ((FreightMode & FreightMode.FRO) != 0)
				{
					SetContainerizedMeasures(result);
				}

				if (((FreightMode & FreightMode.ROA) != 0) || ((FreightMode & FreightMode.RAI) != 0))
				{
					var chargableMeasure = GetChargableMeasure();
					if (chargableMeasure != null)
					{
						result.SetQuantity(MeasureType.Chargeable, chargableMeasure.Item1, chargableMeasure.Item2);
					}
				}

				SetAutoRatingContainers(result);

				return result;
			}
		}

		void SetLooseMeasures(RateableMeasureSet measures)
		{
			measures.CreatePackageUnitList(includeCommodity: true);
			// Note, PackageType doesn't seem to be used in rating for weight and volume. So probably only need commodity here.
			measures.CreateWeightAndVolumeListWithCommodityAndPackageType();

			foreach (var package in CommonPackages)
			{
				AddPackage(measures, package, includeUnit: true, includeVolume: true);
			}

			// Make MeasureType.Package a copy of MeasureType.Unit
			measures.CopyUnitMeasureToPackage();
		}

		void SetContainerizedMeasures(RateableMeasureSet measures)
		{
			measures.CreateWeightAndVolumeListWithCommodityAndPackageType();
			foreach (var package in CommonContainers)
			{
				AddPackage(measures, package, includeUnit: false, includeVolume: true);
			}
			foreach (var package in CommonPackages)
			{
				AddPackage(measures, package, includeUnit: false, includeVolume: false);
			}

			// This is hack for Containerised Mode (FRO) to calculate distance
			// because Autorate only calculate distance using MeasureType.Unit RateableMeasure
			// For Loose Mode (LRO), MeasureType.Unit is added on RateableMeasure in CreatePackageUnitList
			// For Both Mode (BTH), there will be 2 DtbTransportRatingAdapter (LRO, FRO) and the PackageUnit will only be applied to LRO
			if (RatingFreightMode == RatingFreightModes.Codes.Containerised
				&& !measures.HasMeasureType(MeasureType.Unit))
			{
				measures.AddPackageUnit(package: null, packType: PkgUnit.Container);
			}
		}

		void AddPackage(RateableMeasureSet measures, PkgPackage package, bool includeUnit, bool includeVolume)
		{
			if (includeUnit)
			{
				var containerInfo = GetPackageAsContainerInfo(package);
				measures.AddPackageUnitWithCommodity(containerInfo, packType: package.KP_F3_NKPackType, commodity: package.KP_RH_NKCommodityCode);
			}

			var qtyRatio = GetLowestCommonDivotQuantityRatio(package);
			var weightInKG = Constants.Weight.Convert(qtyRatio * package.KP_Weight, package.KP_WeightUQ, Constants.Weight.Kilograms);
			var volumeInM3 = includeVolume ? Constants.Volume.Convert(qtyRatio * package.KP_Volume, package.KP_VolumeUQ, Constants.Volume.CubicMetres) : (decimal?)null;
			measures.AddWeightAndVolumeWithCommodityAndPackageType(weightInKG, volumeInM3, packType: package.KP_F3_NKPackType, commodity: package.KP_RH_NKCommodityCode);
		}

		MeasureInfo.ContainerInfo GetPackageAsContainerInfo(PkgPackage package)
		{
			var packageQuantity = GetLowestCommonDivotQuantity(package);
			if (packageQuantity.IsEmpty)
			{
				return null;
			}

			var innerPackageQuantity = package.Packages.Sum(p => p.KP_PackageQty);
			var teu = package.Container?.ContainerType?.RC_TEU ?? 0;
			var containerNumber = package.IsContainer ? package.KP_PackageID : ZString.Empty;

			var containerInfo = new MeasureInfo.ContainerInfo(
				package.KP_Weight, package.KP_WeightUQ,
				package.KP_Volume, package.KP_VolumeUQ,
				innerPackageQuantity,
				teu,
				containerNumber,
				containerCount: packageQuantity);

			return containerInfo;
		}

		void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			var commonContainers = CommonContainers;

			if (commonContainers.Any() && (FreightMode & FreightMode.Containerised) != 0)
			{
				measures.CreateContainerList(includeContainerNumber: true);

				foreach (var container in commonContainers)
				{
					var ratio = GetLowestCommonDivotQuantityRatio(container);
					var teu = container.Container == null || container.Container.ContainerType == null
						? 0 : container.Container.ContainerType.RC_TEU * container.KP_PackageQty;
					var containerInfo = new MeasureInfo.ContainerInfo(ratio * container.KP_Weight, container.KP_WeightUQ, 0m, container.KP_VolumeUQ, 0, teu, container.KP_PackageID);
					measures.AddContainerWithCommodityAndNumber(container.Container.K0_RC_ContainerType, container.KP_RH_NKCommodityCode, container.KP_PackageID, containerInfo);
				}
			}
		}

		public IEnumerable<PkgPackage> CommonPackages
		{
			get
			{
				var pickupPackages = FromInstruction.DivotsWithPackages.Packages.Where(p => !p.IsContainer);
				var deliveryPackages = ToInstruction.DivotsWithPackages.Packages.Where(p => !p.IsContainer);
				var commonPackages = pickupPackages.Intersect(deliveryPackages);
				return commonPackages;
			}
		}

		public IEnumerable<PkgPackage> CommonContainers
		{
			get
			{
				var pickupContainers = FromInstruction.DivotsWithPackages.Packages.Where(p => p.IsContainer);
				var deliveryContainers = ToInstruction.DivotsWithPackages.Packages.Where(p => p.IsContainer);
				var commonContainers = pickupContainers.Intersect(deliveryContainers);
				return commonContainers;
			}
		}

		ZDecimal GetLowestCommonDivotQuantityRatio(PkgPackage package)
		{
			return (package.KP_PackageQty != 0) ? GetLowestCommonDivotQuantity(package) / package.KP_PackageQty : 0m;
		}

		ZInt GetLowestCommonDivotQuantity(PkgPackage package)
		{
			var pickupDivot = FromInstruction.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().First(d => d.KD_KP_Package == package.PK);
			var deliveryDivot = FromInstruction.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().First(d => d.KD_KP_Package == package.PK);

			return Math.Min(package.KP_PackageQty, Math.Min(pickupDivot.KD_Quantity, deliveryDivot.KD_Quantity)); // 11/10 should return 10 not 11
		}

		// required to be UPPERCASE!
		static class Keys
		{
			public const string FromName = "FROMNAME";
			public const string FromCity = "FROMCITY";
			public const string FromPostcode = "FROMPOSTCODE";
			public const string FromState = "FROMSTATE";

			public const string ToName = "TONAME";
			public const string ToCity = "TOCITY";
			public const string ToPostcode = "TOPOSTCODE";
			public const string ToState = "TOSTATE";
		}

		bool IAutoRatingDescriptionMacroExpander.CanExpandMacros
		{
			get { return true; }
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		string IAutoRatingDescriptionMacroExpander.ExpandMacro(string macro)
		{
			switch (macro.ToUpperInvariant())
			{
				case Keys.FromName:
					return FromAddress != null ? FromAddress.E2_CompanyNameTruncated : ZString.Empty;
				case Keys.FromCity:
					return FromAddress != null ? FromAddress.E2_City : ZString.Empty;
				case Keys.FromPostcode:
					return FromAddress != null ? FromAddress.E2_Postcode : ZString.Empty;
				case Keys.FromState:
					return FromAddress != null ? FromAddress.E2_State : ZString.Empty;
				case Keys.ToName:
					return ToAddress != null ? ToAddress.E2_CompanyNameTruncated : ZString.Empty;
				case Keys.ToCity:
					return ToAddress != null ? ToAddress.E2_City : ZString.Empty;
				case Keys.ToPostcode:
					return ToAddress != null ? ToAddress.E2_Postcode : ZString.Empty;
				case Keys.ToState:
					return ToAddress != null ? ToAddress.E2_State : ZString.Empty;
				default:
					return null;
			}
		}

		string IJobNumber.JobNumber
		{
			get { return Parent.KM_JobID; }
		}

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = GetConditionsSupporterCore()); }
		}
		RateLineConditionsSupporter conditionsSupporter;

		DtbBookingRateLineConditionsSupporter GetConditionsSupporterCore()
		{
			return new DtbBookingRateLineConditionsSupporter(Booking);
		}
	}
}
