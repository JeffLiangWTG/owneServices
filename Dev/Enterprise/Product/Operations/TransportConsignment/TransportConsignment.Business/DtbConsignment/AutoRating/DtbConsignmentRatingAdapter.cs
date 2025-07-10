using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentRatingAdapter : RatingAdapter<DtbConsignment>,
		IJobNumber,
		IAutoRatingDescriptionMacroExpander,
		IAutoRatingFreightConditionsSupportable
	{
		/// <summary>
		/// Consignments will only rate loose for now
		/// </summary>
		public DtbConsignmentRatingAdapter(DtbConsignment consignment, FreightMode freightMode)
			: base(consignment)
		{
			FreightMode = freightMode;
		}

		#region Related Objects

		#region Consignment

		DtbConsignment Consignment
		{
			get { return Parent; }
		}

		#endregion

		#region FromAddress

		JobDocAddress FromAddress
		{
			get
			{
				return FromConsignmentAddress?.Address;
			}
		}

		DtbConsignmentAddress FromConsignmentAddress
		{
			get { return Consignment.Addresses.FirstOrDefault(i => i.LTS_InstructionType == ConsignmentAddressTypes.Codes.PickUp); }
		}

		#endregion

		#region ToAddress

		JobDocAddress ToAddress
		{
			get
			{
				return ToConsignmentAddress?.Address;
			}
		}

		DtbConsignmentAddress ToConsignmentAddress
		{
			get { return Consignment.Addresses.FirstOrDefault(i => i.LTS_InstructionType == ConsignmentAddressTypes.Codes.Delivery); }
		}

		#endregion

		#region JobServices

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = new JobServicesCollection();
				result.AddRange(GetServiceInfosFromJobServices(Consignment.Services, ChargeCodeGroupList.Codes.TransportBooking));

				return result;
			}
		}

		#endregion

		#endregion

		#region IAutoRating Members

		#region IAutoRating_AdapterType

		public override AdapterType AdapterType => AdapterType.TransportConsignment;

		#endregion

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return ((IJobInvoicingPlugIn)Parent).InvoicingSupporter; }
		}

		#endregion

		#region IAutoRating_JobDatesProvider

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new DtbConsignmentJobDatesProvider(Consignment, FromConsignmentAddress, ToConsignmentAddress); }
		}

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
		bool HasConsolCosts
		{
			get
			{
				var result = false;

				var runSheets = Consignment.Addresses.SelectMany(a => a.Actions).Where(a => a.RunSheetInstruction != null).Select(a => a.RunSheetInstruction).Select(i => i.RunSheet);
				if (runSheets.Any())
				{
					var costQuery = new ZQuery(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
					costQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, runSheets.Select(r => r.PK));
					costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, runSheets.First().TablePrefix);
					result = Consignment.Factory.LoadTop1<IJobConsolCost>(costQuery) != null;
				}

				return result;
			}
		}

		#endregion

		#region IAutoRating_ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.TransportConsignment; }
		}

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

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				return FromConsignmentAddress != null && ToConsignmentAddress != null
					? new AutoRatingStatusInfo(true, "")
					: new AutoRatingStatusInfo(false, Res.GetString("08c36178-5634-4951-9bf8-ef09aeeaf94e", "{0} doesn't have at least 2 Addresses. Cannot continue.", Parent.HumanReadableName));
			}
		}

		#endregion

		#endregion

		#region IAutoRatingLocations Members

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
				var fromAddress = FromConsignmentAddress;
				return fromAddress != null ? fromAddress.LTS_DropMode : ZString.Empty;
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
				var toAddress = ToConsignmentAddress;
				return toAddress != null ? toAddress.LTS_DropMode : ZString.Empty;
			}
		}

		#endregion

		#region IAutoRatingOrganisations

		public override OrgHeader Carrier
		{
			get { return GlbCompany.CurrentCompany.OrgProxy; }
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(GetTransportProviders()); }
		}

		IEnumerable<OrgWithSource> GetTransportProviders()
		{
			foreach (var action in Consignment.AllActions)
			{
				var runSheetInstruction = action.RunSheetInstruction;
				if (runSheetInstruction != null)
				{
					var transportCompany = OrgWithSource.NewFrom<OrgHeader>(runSheetInstruction.RunSheet.KG_OH_TransportCoInfo);
					if (transportCompany != null)
					{
						yield return transportCompany;
					}
				}
			}
		}

		#endregion

		#region IAutoRatingOrganisations.DebtorOrgs

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();

				if (FromAddress != null)
				{
					result[RatingDebtorOrgTypes.CNR] = FromAddress.Organisation;
				}

				if (ToAddress != null)
				{
					result[RatingDebtorOrgTypes.CNE] = ToAddress.Organisation;
				}

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

		#region FreightMode

		public override FreightMode FreightMode { get; }

		#endregion

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
			measures.CreateWeightAndVolumeListWithCommodityAndPackageType();

			foreach (var package in CommonPackages)
			{
				AddPackage(measures, package, includeUnit: true, includeVolume: true);
			}

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
			var containerNumber = package.IsContainer ? package.KP_PackageID : ZString.Empty;

			var info = new MeasureInfo.ContainerInfo(
				package.KP_Weight, package.KP_WeightUQ,
				package.KP_Volume, package.KP_VolumeUQ,
				innerPackageQuantity,
				package.Container?.ContainerType?.RC_TEU ?? 0,
				containerNumber,
				containerCount: packageQuantity);

			return info;
		}

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

		#region GetLowestCommonDivotQuantityRatio

		ZDecimal GetLowestCommonDivotQuantityRatio(PkgPackage package)
		{
			return (package.KP_PackageQty != 0) ? GetLowestCommonDivotQuantity(package) / package.KP_PackageQty : 0m;
		}

		#endregion

		#region GetLowestCommonDivotQuantity

		ZInt GetLowestCommonDivotQuantity(PkgPackage package)
		{
			return package.KP_PackageQty;
		}

		#endregion

		#region GetAutoRatingDeliveryDistanceMeasure

		public Tuple<ZDecimal, ZString> GetAutoRatingDistanceMeasure()
		{
			return (Consignment != null) ? Tuple.Create(Consignment.LTC_Distance, Consignment.LTC_DistanceUnit) : null;
		}

		#endregion

		#region CommonPackages

		public IEnumerable<PkgPackage> CommonPackages
		{
			get
			{
				return Consignment.PackageJob.Packages.Where(p => !p.IsContainer && p.KP_KP_ParentPackage == ZGuid.Empty);
			}
		}

		#endregion

		#region CommonContainers

		public IEnumerable<PkgPackage> CommonContainers
		{
			get
			{
				return Consignment.PackageJob.Packages.Where(p => p.IsContainer && p.KP_KP_ParentPackage == ZGuid.Empty);
			}
		}

		#endregion

		#endregion

		#region MonetaryValues

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				result.Add(MoneyType.ValueType.GoodsValue, new Money(Parent.LTC_GoodsValue, Parent.GoodsValueCurrency));
				result.Add(MoneyType.ValueType.InsuranceValue, new Money(Parent.LTC_InsuranceValue, Parent.InsuranceValueCurrency));

				return result;
			}
		}

		#endregion

		#region ServiceLevel

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				return new ServiceLevelRatingInformation(
					new ServiceLevelInfo(Parent.LTC_RS_NKServiceLevel, ServiceLevelType.Client), // carrier service level?
					new ServiceLevelInfo(Parent.LTC_RS_NKServiceLevel, ServiceLevelType.Carrier));
			}
		}

		#endregion

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return Parent.LTC_JobID; }
		}

		#endregion

		#region IAutoRatingDescriptionMacroExpander Members

		// required to be UPPERCASE!
		static class Keys
		{
			public const string FromName = "FROMNAME"; // Lookup Key.
			public const string FromCity = "FROMCITY"; // Lookup Key.
			public const string FromPostcode = "FROMPOSTCODE"; // Lookup Key.
			public const string FromState = "FROMSTATE"; // Lookup Key.

			public const string ToName = "TONAME"; // Lookup Key.
			public const string ToCity = "TOCITY"; // Lookup Key.
			public const string ToPostcode = "TOPOSTCODE"; // Lookup Key.
			public const string ToState = "TOSTATE"; // Lookup Key.
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

		#region IAutoRatingFreightConditionsSupportable

		RateLineConditionsSupporter IAutoRatingFreightConditionsSupportable.ConditionsSupporter
		{
			get { return conditionsSupporter ?? (conditionsSupporter = new DtbConsignmentRateLineConditionsSupporter(Consignment)); }
		}
		RateLineConditionsSupporter conditionsSupporter;

		#endregion
	}
}
