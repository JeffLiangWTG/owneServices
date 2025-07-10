using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;

namespace Enterprise.eManifest.Business.Rating
{
	public class HVLVLineRatingAdapter : RatingAdapter<SupplierBookingLine>
	{
		public HVLVLineRatingAdapter(SupplierBookingLine bookingLine)
			: base(bookingLine)
		{
		}

		public CommonShipment Shipment
		{
			get { return (CommonShipment)Parent.Shipment; }
		}

		#region IAutoRating

		public override AdapterType AdapterType => AdapterType.HVLVShipment;

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Shipment.RatingAdapter.InvoicingSupporter; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return Shipment.RatingAdapter.JobDatesProvider; }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return Shipment.RatingAdapter.ChargeCodeGroups; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.eManifest; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.HLSMerge; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override ILocation Destination
		{
			get
			{
				if (!string.IsNullOrEmpty(Shipment.JS_RL_NKDestination))
				{
					return Shipment.Destination;
				}

				var consol = Shipment.Consols.Count == 1 ? Shipment.Consols[0] : Shipment.FindCorrectConsol();
				if (consol != null)
				{
					return consol.DischargePort;
				}

				return null;
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

				var consol = Shipment.Consols.Count == 1 ? Shipment.Consols[0] : Shipment.FindCorrectConsol();
				if (consol != null)
				{
					return consol.LoadPort;
				}

				return null;
			}
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();

				result[RatingDebtorOrgTypes.CNR] = Parent.BookingHeader?.Consignor == null ? Shipment.Consignor : Parent.BookingHeader.Consignor.Header;
				result[RatingDebtorOrgTypes.CNE] = Shipment.Consignee;

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

		public override ZString DeliveryCartageEquipment
		{
			get { return Shipment.IsDeleted ? ZString.Empty : Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
		}

		public override IDocAddress DeliveryAddress
		{
			get { return Shipment.ConsigneeDeliveryAddress; }
		}

		public override ZString PickupCartageEquipment
		{
			get { return Shipment.IsDeleted ? ZString.Empty : Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
		}

		public override IDocAddress PickupAddress
		{
			get { return (IDocAddress)Parent.BookingHeader.DispatchAddress ?? Shipment.ConsignorPickupAddress; }
		}

		public override FreightMode FreightMode
		{
			get { return FreightRatingHelper.CalculateFreightMode(Shipment.JS_TransportMode, ContainerMode); }
		}

		public override ZString ContainerMode
		{
			get { return Shipment.JS_PackingMode; }
		}

		public override ZString HousebillReleaseType
		{
			get { return Shipment.RatingAdapter.HousebillReleaseType; }
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				result.SetQuantity(MeasureType.Weight, Parent.DL_GrossWeight, Parent.DL_GrossWeightUQ);
				result.SetQuantity(MeasureType.Volume, Parent.DL_Cubic, Parent.DL_CubicUQ);

				var packages = GetPackagesAsContainerInfo();
				result.AddPackageUnit(packages, Parent.DL_F3_NKPackType);

				result.SetPackageCountWithEmptyContainerType((ZDecimal)Parent.DL_PiecesManifested);

				return result;
			}
		}

		MeasureInfo.ContainerInfo GetPackagesAsContainerInfo()
		{
			var containerInfo = new MeasureInfo.ContainerInfo(
				Parent.DL_GrossWeight, Parent.DL_GrossWeightUQ,
				Parent.DL_Cubic, Parent.DL_CubicUQ,
				Parent.DL_PiecesManifested,
				0,
				string.Empty,
				containerCount: Parent.DL_PiecesManifested);

			return containerInfo;
		}

		public override PaymentTermInfos PaymentTerm
		{
			get { return Shipment.RatingAdapter.PaymentTerm; }
		}

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return Shipment.RatingAdapter.IsApplicableToPaymentTermFiltering(chargeCodeGroup, costOrSell);
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				ZString clientServieLevel = string.IsNullOrEmpty(Parent.DL_RS_NKServiceLevel)
					? Shipment.JS_RS_NKServiceLevel
					: Parent.DL_RS_NKServiceLevel;

				return new ServiceLevelRatingInformation(new ServiceLevelInfo(clientServieLevel, ServiceLevelType.Client));
			}
		}

		public override OrgAddress WharfCTOAddress
		{
			get { return Shipment.RatingAdapter.WharfCTOAddress; }
		}

		public override Directions JobDirection
		{
			get { return Shipment.RatingAdapter.JobDirection; }
		}

		public override IJobExRateCurrencyConverter CurrencyConverter =>
			Shipment.RatingAdapter.CurrencyConverter;

		#endregion
	}
}
