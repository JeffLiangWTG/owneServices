using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyShipmentTransportSupporter<T> : CommonShipmentTransportSupporter<T>
		where T : AgencyShipment
	{
		public AgencyShipmentTransportSupporter(T shipment)
			: base(shipment) { }

		public override ZGuid ShippingLine
		{
			get { return Parent.JS_OH_DeliveryAgent; }
			set { Parent.JS_OH_DeliveryAgent = value; }
		}

		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Sea; }
		}

		protected override void NotifySailingChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifySailingChangedCore(transport, previousValue);
			Parent.RefreshContainerMovements();
			Parent.DefaultSailing();
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceShipping; }
		}

		public override bool CreateSailingIfNotExistsForUniversalShipment
		{
			get { return false; }
		}

		#region PickupRoadOrRailLeg DepartureLocation / DeliveryRoadOrRailLeg ArrivalLocation Defaulting

		protected override void NotifyTransportModeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyTransportModeChangedCore(transport, previousValue);

			DefaultPickupRoadOrRailLegDepartureLocation(transport);
			DefaultDeliveryRoadOrRailLegArrivalLocation(transport);
		}

		protected override void NotifyTransportTypeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyTransportTypeChangedCore(transport, previousValue);

			DefaultPickupRoadOrRailLegDepartureLocation(transport);
			DefaultDeliveryRoadOrRailLegArrivalLocation(transport);
			Parent.DefaultSailing();
		}

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyLoadChangedCore(transport, previousValue);

			DefaultPickupRoadOrRailLegDepartureLocation(transport);
		}

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyDischargeChangedCore(transport, previousValue);

			DefaultDeliveryRoadOrRailLegArrivalLocation(transport);
		}

		void DefaultPickupRoadOrRailLegDepartureLocation(Transport transport)
		{
			if (transport != null && Parent.PickupRoadOrRailLeg == transport && transport.JW_OA_DepartureLocation.IsEmpty)
			{
				Parent.DefaultPickupRoadOrRailLegDepartureLocation();
			}
		}

		void DefaultDeliveryRoadOrRailLegArrivalLocation(Transport transport)
		{
			if (transport != null && Parent.DeliveryRoadOrRailLeg == transport && transport.JW_OA_ArrivalLocation.IsEmpty)
			{
				Parent.DefaultDeliveryRoadOrRailLegArrivalLocation();
			}
		}

		#endregion

		#region PickupRoadOrRailLeg / DeliveryRoadOrRailLeg Carrier Address Defaulting

		protected override void NotifyDepartureLocationChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifyDepartureLocationChangedCore(transport, previousValue);

			if (transport != null && Parent.PickupRoadOrRailLeg == transport && !transport.JW_OA_DepartureLocation.IsEmpty && transport.JW_OA_CarrierAddress.IsEmpty)
			{
				DefaultRoadOrRailLegCarrierAddress(transport, RelatedPartyDirectionList.Codes.Pickup, transport.JW_OA_DepartureLocation);
			}
		}

		protected override void NotifyArrivalLocationChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifyArrivalLocationChangedCore(transport, previousValue);

			if (transport != null && Parent.DeliveryRoadOrRailLeg == transport && !transport.JW_OA_ArrivalLocation.IsEmpty && transport.JW_OA_CarrierAddress.IsEmpty)
			{
				DefaultRoadOrRailLegCarrierAddress(transport, RelatedPartyDirectionList.Codes.Delivery, transport.JW_OA_ArrivalLocation);
			}
		}

		void DefaultRoadOrRailLegCarrierAddress(Transport transport, ZString direction, ZGuid orgAddressPK)
		{
			if (transport.JW_OA_CarrierAddress.IsEmpty)
			{
				var newAddress = transport.Factory.Load<OrgAddress>(orgAddressPK);
				if (newAddress != null && newAddress.Header != null && transport.TransportSupporterWithSchedule != null)
				{
					var transportMode = transport.TransportSupporterWithSchedule.TransportMode;
					var containerMode = transport.TransportSupporterWithSchedule.ContainerMode;

					var relatedParty = GetLocalTransportRelatedPartyFromBuyerSupplierLinks(transport, direction, transportMode, containerMode) ?? newAddress.Header.GetRelatedParty(orgAddressPK, RelatedPartyTypeList.Codes.LocalTransport, direction, transportMode, containerMode);

					if (relatedParty != null)
					{
						transport.JW_OA_CarrierAddress = relatedParty.MainAddress.PK;
					}
				}
			}
		}

		OrgHeader GetLocalTransportRelatedPartyFromBuyerSupplierLinks(Transport transport, ZString direction, ZString transportMode, ZString containerMode)
		{
			var destination = Parent.Destination;
			var link = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(Parent.Consignor, Parent.Consignee, destination != null ? destination.RL_RN_NKCountryCode : ZString.Empty);
			if (link != null)
			{
				var trnMode = link.OrgSupBuyLinkTrnModes.Find(transportMode, containerMode);
				if (trnMode != null)
				{
					if (direction == RelatedPartyDirectionList.Codes.Pickup && !trnMode.PF_OH_PickupCartageContractor.IsEmpty)
					{
						return transport.Factory.Load<OrgHeader>(trnMode.PF_OH_PickupCartageContractor);
					}
					else if (direction == RelatedPartyDirectionList.Codes.Delivery && !trnMode.PF_OH_DeliveryCartageContractor.IsEmpty)
					{
						return transport.Factory.Load<OrgHeader>(trnMode.PF_OH_DeliveryCartageContractor);
					}
				}
			}

			return null;
		}

		#endregion

		#region Validation

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			return new AgencyShipmentTransportValidation(transport);
		}

		#endregion
	}
}
