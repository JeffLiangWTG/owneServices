using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eTail.Business.Rating
{
	[CodeAlive("We don't want to delete it yet")]
	class HVLVConsignmentRatingAdapter : RatingAdapter<HVLVConsignment>, IAutoRatingFreightConditionsSupportable
	{
		public HVLVConsignmentRatingAdapter(HVLVConsignment consignment)
				: base(consignment)
		{
		}

		public ForwardingShipment Shipment => Parent.ManifestedOnShipment;

		#region IAutoRating

		public override AdapterType AdapterType => AdapterType.HVLVShipment;

		public override IJobInvoicingSupporter InvoicingSupporter => Shipment.RatingAdapter.InvoicingSupporter;

		public override IJobDatesProvider JobDatesProvider => Shipment.RatingAdapter.JobDatesProvider;

		public override ChargeCodeGroupCollection ChargeCodeGroups => Shipment.RatingAdapter.ChargeCodeGroups;

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

		public override IDocAddress DeliveryAddress => consigneeDocAddress ?? (consigneeDocAddress = new ConsigneeDocAddressProxy(Parent));

		public override ZString PickupCartageEquipment => Shipment.IsDeleted ? ZString.Empty : Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded;

		public override IDocAddress PickupAddress => (IDocAddress)Parent.BookingHeader?.DispatchAddress ?? Shipment.ConsignorPickupAddress;

		public override FreightMode FreightMode => FreightRatingHelper.CalculateFreightMode(Shipment.JS_TransportMode, ContainerMode);

		public override ZString ContainerMode => Shipment.JS_PackingMode;

		public override ZString HousebillReleaseType => Shipment.RatingAdapter.HousebillReleaseType;

		public override PaymentTermInfos PaymentTerm => Shipment.RatingAdapter.PaymentTerm;

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell) => Shipment.RatingAdapter.IsApplicableToPaymentTermFiltering(chargeCodeGroup, costOrSell);

		public override ServiceLevelRatingInformation ServiceLevel => new ServiceLevelRatingInformation(new ServiceLevelInfo(Shipment.JS_RS_NKServiceLevel, ServiceLevelType.Client));

		public override OrgAddress WharfCTOAddress => Shipment.RatingAdapter.WharfCTOAddress;

		public override Directions JobDirection => Shipment.RatingAdapter.JobDirection;

		public override IJobExRateCurrencyConverter CurrencyConverter => Shipment.RatingAdapter.CurrencyConverter;

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				var items = Parent.Items.Cast<HVLVItem>();

				var sumOfItemWeightWithManifestedFallback = items.Sum(item => GetActualWeight(item));
				result.SetQuantity(MeasureType.Weight, sumOfItemWeightWithManifestedFallback, Parent.HVC_WeightUQ);

				var sumOfItemVolumeWithManifestedFallback = items.Sum(item => GetActualVolume(item));
				result.SetQuantity(MeasureType.Volume, sumOfItemVolumeWithManifestedFallback, Parent.HVC_VolumeUQ);

				AddUnitMeasureForEachItemPackType(items, result);

				result.SetPackageCountWithEmptyContainerType(items.Count());

				return result;
			}
		}

		#region IAutoRatingFreightConditionsSupportable

		public RateLineConditionsSupporter ConditionsSupporter => new ShipmentRateLineConditionsSupporter(Shipment);

		#endregion

		#endregion

		ConsigneeDocAddressProxy consigneeDocAddress;

		ZDecimal GetActualWeight(HVLVItem item)
		{
			return item.HVI_ActualWeight.IsEmpty ? item.HVI_ManifestedWeight : item.HVI_ActualWeight;
		}

		ZDecimal GetActualVolume(HVLVItem item)
		{
			return item.HVI_ActualVolume.IsEmpty ? item.HVI_ManifestedVolume : item.HVI_ActualVolume;
		}

		void AddUnitMeasureForEachItemPackType(IEnumerable<HVLVItem> items, RateableMeasureSet measures)
		{
			measures.CreatePackageUnitList();

			foreach (var item in items)
			{
				var containerInfo = new MeasureInfo.ContainerInfo(
					GetActualWeight(item),
					Parent.HVC_WeightUQ,
					GetActualVolume(item),
					Parent.HVC_VolumeUQ,
					1,
					0,
					item.HVI_ContainerNumber,
					containerCount: 1);

				measures.AddPackageUnit(containerInfo, item.HVI_F3_NKPackType);
			}
		}
	}
}
