using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class AgencyShipmentRatingAdapter : ShipmentRatingAdapter<AgencyShipment>, IJobDataUpdater
	{
		protected internal AgencyShipmentRatingAdapter(AgencyShipment parent) : base(parent) { }

		public override AdapterType AdapterType => AdapterType.BillOfLading;

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				var volumeAndWeightUnitStatusInformation = GetVolumeAndWeightUnitStatusInformation();
				if (volumeAndWeightUnitStatusInformation != null)
				{
					return volumeAndWeightUnitStatusInformation;
				}

				if (Parent.IsTopLevelPacksMode && Parent.TopLevelPacks.Cast<AgencyShipmentContainer>().Any(c => c.JC_GrossWeightUQ.IsEmpty || c.JC_GrossVolumeUQ.IsEmpty))
				{
					return new AutoRatingStatusInfo(false, RequireWeightAndVolumeOnPackLinesError);
				}

				return parent.JS_PackingMode == Constants.ContainerModes.FCL
						? ContainerisedStatusInfo()
						: NonContainerisedStatusInfo();
			}
		}

		AutoRatingStatusInfo ContainerisedStatusInfo()
		{
			return parent.ShippingContainers.Count == 0
					   ? new AutoRatingStatusInfo(false, Res.GetString("0635440c-60c2-4b1f-acf7-8e33db09945b", "This FCL shipment has no containers defined."))
					   : new AutoRatingStatusInfo(true, null);
		}

		AutoRatingStatusInfo NonContainerisedStatusInfo()
		{
			if (parent.IsRollOnRollOff)
			{
				if (parent.ShippingContainers.Count == 0)
				{
					return new AutoRatingStatusInfo(false, Res.GetString("6da7163e-f35e-4a9c-8366-db6aef872157", "This RORO shipment has no vehicles defined."));
				}
			}
			else if (parent.JS_ActualWeight == 0)
			{
				return new AutoRatingStatusInfo(false, Res.GetString("e5a43273-5e35-432f-a99f-d4621a3291c4", "Shipment weight has not been entered."));
			}
			else if (parent.JS_ActualVolume == 0)
			{
				return new AutoRatingStatusInfo(false, Res.GetString("4f6be4ad-193e-4f1f-978b-591141773c71", "Shipment volume has not been entered."));
			}

			return new AutoRatingStatusInfo(true, null);
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Shipping; }
		}

		public override ILocation GetVia(CostSell costOrSell)
		{
			var loadVia = parent.CalcLoadPort != Origin ? parent.CalcLoadPort : null;
			var dischargeVia = parent.CalcDischargePort != Destination ? parent.CalcDischargePort : null;

			if (loadVia != null && dischargeVia != null)
			{
				if (parent.IsImport())
				{
					return dischargeVia;
				}

				if (parent.IsExport())
				{
					return loadVia;
				}
			}
			else
			{
				return loadVia ?? dischargeVia;
			}

			return null;
		}

		protected override CommonContainer GetContainerFromPackLineForAutoRatingCore(PackLine packLine, CommonConsol consol)
		{
			return ((AgencyShipmentPackLine)packLine).Container;
		}

		protected override void SetContainersCore(RateableMeasureSet measures)
		{
			FreightRatingHelper.SetShippingContainers(measures, parent.ShippingContainers.Cast<CommonContainer>(), parent);
		}

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				var value = parent.JS_INCO == Constants.DomesticPaymentTerms.Prepaid
					? Constants.PaymentType.Prepaid
					: parent.JS_INCO == Constants.DomesticPaymentTerms.Collect
						? Constants.PaymentType.Collect
						: string.Empty;

				if (!string.IsNullOrEmpty(value))
				{
					var paymentTerm = new PaymentTermInfos();
					paymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Revenue, value));

					return paymentTerm;
				}

				return null;
			}
		}

		public override RateablePartList GetPackages(CommonConsol consol = null, bool fallbackToShipmentMeasures = true)
		{
			if (!parent.IsTopLevelPacksMode)
			{
				return base.GetPackages(consol);
			}

			var packages = new RateablePartList();
			packages.HasContainerType = true;
			packages.HasCommodity = true;
			packages.WeightUnit = WeightUnit;
			packages.VolumeUnit = VolumeUnit;

			foreach (AgencyShipmentContainer topLevelPack in parent.TopLevelPacks)
			{
				var part = new RateablePart();
				part.PackageCount = topLevelPack.JC_ContainerCount;
				part.CommodityCode = topLevelPack.JC_RH_NKContainerCommodityCode;
				part.PackageType = topLevelPack.JC_F3_NKPackType;
				part.ContainerNumber = topLevelPack.JC_ContainerNum;
				part.Weight = FreightRatingHelper.Convert(topLevelPack.JC_GrossWeight, topLevelPack.JC_GrossWeightUQ, packages.WeightUnit);
				part.Volume = FreightRatingHelper.Convert(topLevelPack.JC_GrossVolume, topLevelPack.JC_GrossVolumeUQ, packages.VolumeUnit);

				if (!topLevelPack.JC_RC.IsEmpty)
				{
					part.ContainerTypePk = topLevelPack.JC_RC.ToGuid();
				}

				packages.AddPart(part);
			}

			return packages;
		}

		public override RateablePartList GetUnits(CommonConsol consol = null)
		{
			if (!parent.IsTopLevelPacksMode)
			{
				return base.GetUnits(consol);
			}

			var units = new RateablePartList();
			units.HasContainerType = true;
			units.HasPackageType = true;

			foreach (AgencyShipmentContainer topLevelPack in Parent.TopLevelPacks)
			{
				var part = GetUnitPartFromTopLevelPack(topLevelPack);
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

		static RateableContainer GetUnitPartFromTopLevelPack(AgencyShipmentContainer topLevelPack)
		{
			var part = new RateableContainer();
			part.ContainerWeightInKG = FreightRatingHelper.Convert(topLevelPack.JC_GrossWeight, topLevelPack.JC_GrossWeightUQ, Constants.Weight.Kilograms);
			part.ContainerVolumeInM3 = FreightRatingHelper.Convert(topLevelPack.JC_GrossVolume, topLevelPack.JC_GrossVolumeUQ, Constants.Volume.CubicMetres);
			part.UnitCount = topLevelPack.JC_ContainerCount;
			part.PackageType = topLevelPack.JC_F3_NKPackType;
			part.RefNumber = topLevelPack.JC_ContainerNum;

			part.TEU = topLevelPack.RefContainer?.RC_TEU ?? 0;
			part.ContainerNumber = topLevelPack.JC_ContainerNum;
			part.ContainerTypePk = topLevelPack.JC_RC.IsEmpty ? Guid.Empty : topLevelPack.JC_RC.ToGuid();

			// For CMB calculator. When calculating weight/volume per top pack, it uses IRateableContainer and
			// ContainerCount rather than UnitCount as packages in this context are historically treated as containers.
			// Something to refactor I guess.
			part.ContainerCount = topLevelPack.JC_ContainerCount;
			part.ContainerPackages = topLevelPack.JC_ContainerCount;

			return part;
		}

		public override MasterFiles.Integration.IDocAddress PickupAddress
		{
			get { return AgencyShipmentInvoicingSupporter.GetConsignorPickupAddressForAccounting(parent); }
		}

		public override OrgHeader Carrier
		{
			get { return parent.Principal; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				if (PickupAddress != null && PickupAddress.Organisation != null)
				{
					result[RatingDebtorOrgTypes.CNR] = (OrgHeader)PickupAddress.Organisation;
				}

				return result;
			}
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(GetCTOOrganisations()); }
		}

		IEnumerable<OrgWithSource> GetCTOOrganisations()
		{
			var sailing = parent.Sailing;
			if (sailing != null)
			{
				if (sailing.Origin != null && sailing.Origin.DepartureCTOAddress != null)
				{
					yield return OrgWithSource.NewFrom<OrgAddress>(sailing.Origin.JA_OA_DepartureCTOAddressInfo);
				}

				if (sailing.Destination != null && sailing.Destination.ArrivalCTOAddress != null)
				{
					yield return OrgWithSource.NewFrom<OrgAddress>(sailing.Destination.JB_OA_ArrivalCTOAddressInfo);
				}
			}
		}

		protected override void SetDistanceMeasuresCore(RateableMeasureSet measures)
		{
			var bookingParent = (IDtbBookingParent)parent;
			if (bookingParent.GetSupportedDirections().Contains(DtbBookingDirection.PIC))
			{
				var pickupTransport = PickupRoadLeg;
				if (pickupTransport != null)
				{
					measures.SetPickupDistance(pickupTransport.JW_Distance, pickupTransport.JW_DistanceUnit);
				}
			}

			if (bookingParent.GetSupportedDirections().Contains(DtbBookingDirection.DLV))
			{
				var deliveryTransport = DeliveryRoadLeg;
				if (deliveryTransport != null)
				{
					measures.SetDeliveryDistance(deliveryTransport.JW_Distance, deliveryTransport.JW_DistanceUnit);
				}
			}
		}

		Transport PickupRoadLeg
		{
			get
			{
				return parent.TransportsIncludingRelated.FirstLegMatching(l =>
					l.IsRoad &&
					l.JW_RL_NKLoadPort == parent.JS_RL_NKOrigin &&
					l.JW_TransportType != Constants.TransportPlanningType.MainVessel &&
					!l.JW_OA_DepartureLocation.IsEmpty);
			}
		}

		Transport DeliveryRoadLeg
		{
			get
			{
				return parent.TransportsIncludingRelated.LastLegMatching(l =>
					l.IsRoad &&
					l.JW_RL_NKDiscPort == parent.JS_RL_NKDestination &&
					l.JW_TransportType != Constants.TransportPlanningType.MainVessel &&
					!l.JW_OA_ArrivalLocation.IsEmpty);
			}
		}

		#region Contract Numbers

		// This is awkward to use CONs as CLCs but let's keep it this way until we do enhancement.
		public override IEnumerable<ZString> ClientContractNumbers => CarrierContractNumbers;

		public override IEnumerable<ZString> CarrierContractNumbers
		{
			get
			{
				if (parent.Numbers is Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection contractNumbers)
				{
					return contractNumbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON);
				}

				return Enumerable.Empty<ZString>();
			}
		}

		public override IContractNumberConfiguration GetContractNumberConfiguration(CostSell costOrSell)
		{
			return new ContractNumberConfiguration(costOrSell);
		}

		class ContractNumberConfiguration : IContractNumberConfiguration
		{
			public ContractNumberConfiguration(CostSell costOrSell)
			{
				this.costOrSell = costOrSell;
			}

			readonly CostSell costOrSell;

			public bool ShouldAddContractNumberQueryFilter
			{
				get
				{
					if (costOrSell == CostSell.Revenue)
					{
						return !AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers.Value;
					}

					return true;
				}
			}

			public bool ShouldApplySpecificAdapterContractNumberFilter => costOrSell == CostSell.Revenue;

			public bool ShouldIgnoreJobClientContractNumbers => AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers.Value;

			public bool ShouldIgnoreJobCarrierContractNumbers => false;

			public bool ShouldMatchJobBlankContractNumber => true;

			public bool ShouldUseCarrierContractDateFilter => false;
		}

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return false;
		}

		#endregion

		#region IJobDataUpdater

		bool IJobDataUpdater.CanUpdateDate => false;

		void IJobDataUpdater.UpdateCarrier(OrgHeader newCarrier) { }
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

		public void UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExistingDuplicates) { }
		public bool UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
		{
			confirmationMessage = null;
			return false;
		}

		void IJobDataUpdater.UpdateServiceLevel(ZString newServiceLevel) { }
		void IJobDataUpdater.UpdatePaymentTerms(ZString newPaymentTerms) { }
		void IJobDataUpdater.UpdateNamedAccount(ZString namedAccount) { }
		void IJobDataUpdater.UpdateCarrierQuoteNumber(ZString carrierQuoteNumber) { }
		void IJobDataUpdater.UpdateContainersCarrierQuoteNumber(ZGuid containerRefPK, ZString carrierQuoteNumber) { }
		void IJobDataUpdater.UpdateSpotBookingTerms(ZString termsAsText) { }
		DataUpdateResult IJobDataUpdater.UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token) => DataUpdateResult.NoAction;
		void IJobDataUpdater.UpdateTransports(IEnumerable<ITransport> transports) { }

		bool IJobDataUpdater.IsMultipleClientContractNumberSupported => true;

		DataUpdateResult IJobDataUpdater.UpdateClientContractNumber(IEnumerable<string> newNumbers)
		{
			var existingCusEntryNumbers = Parent.Numbers
				.Cast<CusEntryNumber>()
				.Where(x => !x.IsDeleted && x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
				.ToArray();

			var populateNumbers = AgencyRegistry.Instance.PopulateContractNumbersFromRevenueRates.Value;
			var replaceNumbers = AgencyRegistry.Instance.ReplaceExistingContractNumbersWithNewNumbers.Value;

			if (replaceNumbers)
			{
				foreach (var cusEntryNumber in existingCusEntryNumbers)
				{
					Parent.Numbers.RemoveAndDelete(cusEntryNumber);
				}
			}

			if (replaceNumbers || populateNumbers)
			{
				foreach (var newNumber in newNumbers)
				{
					parent.Numbers.AddOrSkipContractNumber(newNumber, isEmptyAllowed: true);
				}
			}

			return DataUpdateResult.Updated;
		}

		void IJobDataUpdater.SendBookingInformationToCarrier() { }

		#endregion
	}
}
