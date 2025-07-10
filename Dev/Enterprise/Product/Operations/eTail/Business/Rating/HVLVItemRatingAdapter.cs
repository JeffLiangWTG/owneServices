using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.eTail.Business.Rating
{
	class HVLVItemRatingAdapter : RatingAdapter<HVLVItem>, IAutoRatingFreightConditionsSupportable
	{
		public HVLVItemRatingAdapter(HVLVItem item, HVLVConsignment consignment, ForwardingShipment shipment)
			: base(item)
		{
			Consignment = Argument.NotNull(consignment, nameof(consignment));
			Shipment = Argument.NotNull(shipment, nameof(shipment));
		}

		public readonly HVLVConsignment Consignment;
		public readonly ForwardingShipment Shipment;

		#region IAutoRating

		public override AdapterType AdapterType => AdapterType.HVLVShipment;

		public override IJobInvoicingSupporter InvoicingSupporter => Shipment.RatingAdapter.InvoicingSupporter;

		public override IJobDatesProvider JobDatesProvider => Shipment.RatingAdapter.JobDatesProvider;

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = Shipment.RatingAdapter.ChargeCodeGroups;
				result.Add(ChargeCodeGroupList.Codes.Brokerage);
				result.Add(ChargeCodeGroupList.Codes.Origin);

				return result;
			}
		}

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.eManifest;

		public override MergeChargeOptions MergeCharges => MergeChargeOptions.HLSMerge;

		public override RateType RateTypeToUse => RateType.Forwarding;

		public override ILocation Destination
		{
			get
			{
				if (!string.IsNullOrEmpty(Shipment.JS_RL_NKDestination))
				{
					return Shipment.Destination;
				}

				var consol = ShipmentRatingExtensions.GetFirstOrCorrectConsol(Shipment);
				return consol?.DischargePort;
			}
		}

		public override ILocation Origin
		{
			get
			{
				if (!string.IsNullOrEmpty(Shipment.JS_RL_NKOrigin))
				{
					return Shipment.Origin;
				}

				var consol = ShipmentRatingExtensions.GetFirstOrCorrectConsol(Shipment);
				return consol?.LoadPort;
			}
		}

		public override Creditors Creditors
		{
			get
			{
				var result = new Creditors();
				if (Consignment.HVC_OH_LastMileCarrier.IsValid)
				{
					result.Add(ChargeCodeGroupList.Codes.Destination, new OrgPrioritizedList(OrgWithSource.NewFrom<OrgHeader>(Consignment.HVC_OH_LastMileCarrierInfo)));
				}

				result.Merge(Shipment.GetCreditors());
				return result;
			}
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection
				{
					[Registry.Business.RatingDebtorOrgTypes.CNR] = Shipment.Consignor,
					[Registry.Business.RatingDebtorOrgTypes.CNE] = Shipment.Consignee
				};

				if (InvoicingSupporter != null && InvoicingSupporter.Job != null)
				{
					if (InvoicingSupporter.Job.LocalCharges != null)
					{
						result[Registry.Business.RatingDebtorOrgTypes.LC] = InvoicingSupporter.Job.LocalCharges;
					}

					if (InvoicingSupporter.Job.AgentCollect != null)
					{
						result[Registry.Business.RatingDebtorOrgTypes.AG] = InvoicingSupporter.Job.AgentCollect;
					}
				}

				return result;
			}
		}

		public override ZString DeliveryCartageEquipment => Shipment.IsDeleted ? ZString.Empty : Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded;

		public override IDocAddress DeliveryAddress => consigneeDocAddress ?? (consigneeDocAddress = new ConsigneeDocAddressProxy(Consignment));

		public override ZString PickupCartageEquipment => Shipment.IsDeleted ? ZString.Empty : Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded;

		public override IDocAddress PickupAddress => Shipment.ConsignorPickupAddress;

		public override FreightMode FreightMode => FreightRatingHelper.CalculateFreightMode(Shipment.JS_TransportMode, ContainerMode);

		public override ZString ContainerMode => Shipment.JS_PackingMode;

		public override ZString HousebillReleaseType => Shipment.RatingAdapter.HousebillReleaseType;

		public override PaymentTermInfos PaymentTerm => Shipment.RatingAdapter.PaymentTerm;

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell) =>
			Parent.Factory.GetCachedValue("HVLVItemRatingAdapter|PaymentTerm|" + chargeCodeGroup + "|" + costOrSell, () => Shipment.RatingAdapter.IsApplicableToPaymentTermFiltering(chargeCodeGroup, costOrSell));

		public override ServiceLevelRatingInformation ServiceLevel => new ServiceLevelRatingInformation(new ServiceLevelInfo(Parent.Consignment.HVC_RS_NKServiceLevel, ServiceLevelType.Client));

		public override OrgAddress WharfCTOAddress => Shipment.RatingAdapter.WharfCTOAddress;

		public override Directions JobDirection => Shipment.RatingAdapter.JobDirection;

		public override IJobExRateCurrencyConverter CurrencyConverter => Shipment.RatingAdapter.CurrencyConverter;

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				result.SetQuantity(MeasureType.Weight, GetActualWeight(Parent), Consignment.HVC_WeightUQ);
				result.SetQuantity(MeasureType.Volume, GetActualVolume(Parent), Consignment.HVC_VolumeUQ);

				// HVLV Items are autorated as separate rating adapters rather than as RateableContainers collection on shipment level
				// as it would be with forwarding shipment pack lines.
				//
				// So, historically, instead of creating a a Unit/Package measures with a collection of RateableContainers on the shipment level
				// (a container per item), we have to create Unit/Package measures on the item rating adapter level with 1 RateableContainer each.
				var part = new RateableContainer
				{
					ContainerCount = 1,
					PackageCount = 1,
					UnitCount = 1,
					ContainerNumber = Parent.HVI_ContainerNumber,
					PackageType = Parent.HVI_F3_NKPackType,
				};

				part.SetContainerWeight(GetActualWeight(Parent), Consignment.HVC_WeightUQ);
				part.SetContainerVolume(GetActualVolume(Parent), Consignment.HVC_VolumeUQ);

				var packages = new RateablePartList { HasPackageType = true, HasContainerNumber = true };
				packages.AddPart(part);

				result.AddPartList(MeasureType.Unit, packages);
				result.AddPartList(MeasureType.Package, packages);

				return result;
			}
		}

		#region IAutoRatingFreightConditionsSupportable

		ConsigneeDocAddressProxy consigneeDocAddress;

		public RateLineConditionsSupporter ConditionsSupporter => new HVLVItemRateLineConditionsSupporter(Parent);

		#endregion

		#endregion

		ZDecimal GetActualWeight(HVLVItem item)
		{
			return item.HVI_ActualWeight.IsEmpty ? item.HVI_ManifestedWeight : item.HVI_ActualWeight;
		}

		ZDecimal GetActualVolume(HVLVItem item)
		{
			return item.HVI_ActualVolume.IsEmpty ? item.HVI_ManifestedVolume : item.HVI_ActualVolume;
		}
	}
}
