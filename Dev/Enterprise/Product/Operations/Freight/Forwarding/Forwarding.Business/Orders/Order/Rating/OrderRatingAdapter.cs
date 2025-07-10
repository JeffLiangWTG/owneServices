using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class OrderRatingAdapter : RatingAdapter<Order>
	{
		public OrderRatingAdapter(Order order)
			: base(order)
		{
		}

		public override AdapterType AdapterType => AdapterType.Order;

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent?.Shipment?.InvoicingSupporter; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new OrderJobDatesProvider(Parent); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.AddRange(Env.Registry.Rating.FreightRatedCodes);
				result.AddRange(Env.Registry.Rating.BrokerageRatedCodes);

				return result;
			}
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Shipment; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.CrossAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override ILocation Destination
		{
			get { return Parent.GoodsDeliveredTo; }
		}

		public override ILocation Origin
		{
			get { return Parent.GoodsAvailableAt; }
		}

		public override OrgHeader Carrier
		{
			get { return Parent.Carrier; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;

				if (DeliveryAddress != null && DeliveryAddress.Organisation != null)
				{
					result[RatingDebtorOrgTypes.CNE] = (OrgHeader)DeliveryAddress.Organisation;
				}

				if (PickupAddress != null && PickupAddress.Organisation != null)
				{
					result[RatingDebtorOrgTypes.CNE] = (OrgHeader)PickupAddress.Organisation;
				}

				result[RatingDebtorOrgTypes.AG] = Parent.IsImport() ? Parent.ReceivingAgent : Parent.SendingAgent;
				result[RatingDebtorOrgTypes.LC] = Parent.Buyer;

				return result;
			}
		}

		public override IDocAddress DeliveryAddress
		{
			get
			{
				return Parent.GoodsDeliveredToAddress;
			}
		}

		public override IDocAddress PickupAddress
		{
			get
			{
				return Parent.GoodsAvailableAtAddress;
			}
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(OrgWithSource.NewFrom<OrgHeader>(Parent.JD_OH_ReceivingAgentInfo), OrgWithSource.NewFrom<OrgHeader>(Parent.JD_OH_SendingAgentInfo)); }
		}

		public override FreightMode FreightMode
		{
			get
			{
				if (!isContainerized.HasValue)
				{
					isContainerized = ((RateableMeasureSet)RateableMeasures)?.IsContainerized() ?? false;
				}

				return FreightRatingHelper.CalculateFreightMode(Parent.JD_TransportMode, ContainerMode, isContainerized.Value);
			}
		}

		public override ZString ContainerMode
		{
			get { return Parent.JD_ContainerMode; }
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				result.SetQuantity(MeasureType.Weight, Parent.JD_ActualWeight, Parent.JD_UnitOfWeight);
				result.SetQuantity(MeasureType.Volume, Parent.JD_ActualVolume, Parent.JD_UnitOfVolume);
				result.SetQuantity(MeasureType.Package, Parent.JD_Packs, Parent.JD_F3_NKPackType);
				SetAutoRatingContainers(result);

				isContainerized = result.IsContainerized();

				return result;
			}
		}

		bool? isContainerized;

		void SetAutoRatingContainers(RateableMeasureSet measures)
		{
			measures.CreateContainerList(includeCommodity: false);

			foreach (OrderContainer container in Parent.PlannedContainers)
			{
				var key = Parent.JD_ContainerMode == Core.Constants.ContainerModes.LCL
					? MeasureInfo.ContainerInfo.LCL
					: container.J1_RC;

				var containerInfo = new MeasureInfo.ContainerInfo(
					container: container.Container,
					containerCount: container.J1_ContainerCount);

				measures.AddContainerGroup(key, new[] { containerInfo });
			}
		}

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();
				result.Add(MoneyType.ValueType.GoodsValue, new Money(Parent.JD_Calc_TotalPrice, Parent.OrderCurrency));

				return result;
			}
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get { return new ServiceLevelRatingInformation(new ServiceLevelInfo(Parent.JD_RS_NKServiceLevel_NI, ServiceLevelType.Client)); }
		}
	}
}
