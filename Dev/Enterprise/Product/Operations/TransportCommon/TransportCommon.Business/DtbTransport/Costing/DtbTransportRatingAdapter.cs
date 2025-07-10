using System;
using System.Collections.Generic;
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
using static Enterprise.Core.Constants;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportRatingAdapter<T> : RatingAdapter<T>,
													  IJobNumber,
													  IAutoRatingDescriptionMacroExpander,
													  IAutoRatingFreightConditionsSupportable
		where T : DtbTransport
	{
		protected DtbTransportRatingAdapter(T transport, FreightMode freightMode)
			: base(transport)
		{
			this.freightMode = freightMode;
		}

		readonly FreightMode freightMode;

		#region Related Objects

		protected abstract DtbTransportInstruction FromInstruction { get; }
		protected abstract DtbTransportInstruction ToInstruction { get; }

		#region FromAddress

		JobDocAddress FromAddress
		{
			get
			{
				var fromInstruction = FromInstruction;
				return fromInstruction != null ? fromInstruction.Address : null;
			}
		}

		#endregion

		#region ToAddress

		JobDocAddress ToAddress
		{
			get
			{
				var toInstruction = ToInstruction;
				return toInstruction != null ? toInstruction.Address : null;
			}
		}

		#endregion

		#endregion

		#region IAutoRating Members

		#region IAutoRating Members

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return ((IJobInvoicingPlugIn)Parent).InvoicingSupporter; }
		}

		#endregion

		#region IAutoRating_JobDatesProvider

		public override IJobDatesProvider JobDatesProvider
		{
			get { return GetJobDatesProviderCore(); }
		}

		protected abstract IJobDatesProvider GetJobDatesProviderCore();

		#endregion

		#region IAutoRating_ChargeCodeGroups

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

		protected abstract bool HasConsolCosts { get; }

		#endregion

		#region IAutoRating_ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return ConsumerTypeCore; }
		}

		protected abstract JobInvoicingConsumerType ConsumerTypeCore { get; }

		#endregion

		#region IAutoRating_MergeCharges

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		#endregion

		#region IAutoRating_RateTypeToUse

		public override RateType RateTypeToUse
		{
			get { return RateType.TransportBookings; }
		}

		#endregion

		#region IAutoRating.StatusInformation

		public override AutoRatingStatusInfo StatusInformation => GetStatusInformation();

		AutoRatingStatusInfo GetStatusInformation()
		{
			if (FromInstruction == null || ToInstruction == null)
			{
				return new AutoRatingStatusInfo(false, Res.GetString("344A68AC-BC21-435B-AEEA-C8F9E726E97D", "{0} doesn't have at least 2 Instructions. Cannot continue.", Parent.HumanReadableName));
			}

			if (HasInvalidDate(FromInstruction.Confirmations, true) || HasInvalidDate(ToInstruction.Confirmations, true))
			{
				return new AutoRatingStatusInfo(false, Res.GetString("3bfecc64-17f5-48c9-961c-a5ba9a7bf755", "Please enter valid Actual date."));
			}

			if (HasInvalidDate(FromInstruction.Confirmations, false) || HasInvalidDate(ToInstruction.Confirmations, false))
			{
				return new AutoRatingStatusInfo(false, Res.GetString("17224f49-a3a8-4f47-9e28-6320db219eb3", "Please enter valid Estimated date."));
			}

			var volumeAndWeightUnitStatusInformation = GetVolumeAndWeightUnitStatusInformation();
			return volumeAndWeightUnitStatusInformation ?? GetAdditionalStatusInformation();
		}

		bool HasInvalidDate(IDtbTransportConfirmationCollection items, bool actualElseEstimated)
		{
			return items.Cast<DtbTransportConfirmation>().Any(x =>
			{
				var d = actualElseEstimated ? x.KK_Actual : x.KK_Estimated;
				return !d.IsEmpty && !d.IsValid;
			});
		}

		protected virtual AutoRatingStatusInfo GetAdditionalStatusInformation()
		{
			return new AutoRatingStatusInfo(true, "");
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
					? Res.GetString("4f61bf93-f723-4e1a-9420-9b5bff73df78", "Invalid unit of weight: '{0}'.", invalidPackage.WeightUnit)
					: Res.GetString("066df783-66bd-4959-ba8c-b8b0a73eebb3", "Invalid unit of volume: '{0}'.", invalidPackage.VolumeUnit);

				result = new AutoRatingStatusInfo(false, message);
			}

			return result;
		}

		#endregion

		#endregion

		#region IAutoRatingLocations Members

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

		#endregion

		#region IAutoRatingOrganisations Members

		#region IAutoRatingOrganisations_Consignor

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

		#endregion

		#region IAutoRatingOrganisations_Consignee

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

		#endregion

		#region IAutoRatingOrganisations.DebtorOrgs

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

				return result;
			}
		}

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region IAutoRatingFreightInfo_FreightMode

		public override FreightMode FreightMode
		{
			get { return freightMode; }
		}

		#endregion

		#region IAutoRatingFreightInfo_Measures

		#region Measures

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

				if ((FreightMode & FreightMode.ROA) != 0)
				{
					var chargableMeasure = GetChargableMeasure();
					if (chargableMeasure != null)
					{
						result.SetQuantity(MeasureType.Chargeable, chargableMeasure.Item1, chargableMeasure.Item2);
					}
				}

				SetAutoRatingContainers(result);

				var distance = GetAutoRatingDistanceMeasure();
				if (distance != null)
				{
					result.SetPickupDistance(distance.Item1, distance.Item2);
				}

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
			if (RatingFreightMode == Shared.RatingFreightModes.Codes.Containerised
				&& !measures.HasMeasureType(MeasureType.Unit))
			{
				measures.AddPackageUnit(package: null, packType: PkgUnit.Container);
			}
		}

		protected virtual ZString RatingFreightMode => ZString.Empty;

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

		#endregion

		#region SetAutoRatingContainers

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

		#endregion

		#region GetAutoRatingDeliveryDistanceMeasure

		/// <summary>
		/// Return distance as amount and unit tuple.
		/// Could return a Enterprise.ZArchitecture.Quantity instead if it's certain unit is not empty.
		/// </summary>
		public virtual Tuple<ZDecimal, ZString> GetAutoRatingDistanceMeasure()
		{
			return null;
		}

		#endregion

		#region GetChargableMeasure

		/// <summary>
		/// Return chargeable quantity as amount and unit tuple.
		/// </summary>
		public virtual Tuple<ZDecimal, ZString> GetChargableMeasure()
		{
			return null;
		}

		#endregion

		#region CommonPackages

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

		#endregion

		#region CommonContainers

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

		#endregion

		#region GetLowestCommonDivotQuantityRatio

		ZDecimal GetLowestCommonDivotQuantityRatio(PkgPackage package)
		{
			return (package.KP_PackageQty != 0) ? GetLowestCommonDivotQuantity(package) / package.KP_PackageQty : 0m;
		}

		#endregion

		#region GetLowestCommonDivotQuantity

		ZInt GetLowestCommonDivotQuantity(PkgPackage package)
		{
			var pickupDivot = FromInstruction.PackageDivots.Cast<DtbTransportInstructionPkgDivot>().First(d => d.KD_KP_Package == package.PK);
			var deliveryDivot = FromInstruction.PackageDivots.Cast<DtbTransportInstructionPkgDivot>().First(d => d.KD_KP_Package == package.PK);

			return Math.Min(package.KP_PackageQty, Math.Min(pickupDivot.KD_Quantity, deliveryDivot.KD_Quantity)); // 11/10 should return 10 not 11
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region IAutoRatingDescriptionMacroExpander Members

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return Parent.KM_JobID; }
		}

		#endregion

		#region IAutoRatingFreightConditionsSupportable

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = GetConditionsSupporterCore()); }
		}
		RateLineConditionsSupporter conditionsSupporter;

		protected abstract TransportRateLineConditionsSupporter GetConditionsSupporterCore();

		#endregion
	}
}
