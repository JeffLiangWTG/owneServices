using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class ShipmentWrapper : IShipment
	{
		public ShipmentWrapper(ShipmentAction shipmentAction)
		{
			this.shipmentAction = shipmentAction;
			shipment = shipmentAction.Shipment;
		}

		internal void UpdateReleaseStatus(ZString releaseStatus, ZDateTime releaseStatusDate)
		{
			if (!shipment.B0_ReleaseStatusDate.IsValid
					|| (releaseStatusDate.IsValid
							&& shipment.B0_ReleaseStatusDate <= releaseStatusDate))
			{
				if (!releaseStatus.IsEmpty)
				{
					shipment.B0_ReleaseStatus = releaseStatus;
				}

				shipment.B0_ReleaseStatusDate = releaseStatusDate;
			}
		}

		internal bool IsLinked
		{
			get { return shipment.IsLinked; }
		}

		#region Implementation of IShipment

		public ZString ShipmentActionCode
		{
			get { return shipmentAction.B0_ActionCode; }
		}

		public ZString ShipmentType
		{
			get { return shipment.B0_ShipmentType; }
		}

		public ZString ShipmentControlNumber
		{
			get { return shipment.ShipmentControlNumber; }
		}

		public ZString ShipmentIdentifier
		{
			get { return shipment.B0_ReferenceID; }
		}

		public ZString PortOrPointOfLoading
		{
			get { return shipment.B0_PortOfLadingKCode; }
		}

		public ZString PortOrPointOfLoadingCodeType
		{
			get { return PortCodeTypes.Codes.ScheduleK; }
		}

		public ZString PlaceOfReceipt
		{
			get { return shipment.B0_PlaceOfReceipt; }
		}

		public ZString ServiceType
		{
			get { return shipment.B0_ServiceType; }
		}

		public ZString TransferDestinationFIRMSCode
		{
			get { return shipment.B0_Firms; }
		}

		public ZInt BoardedQuantity
		{
			get { return shipment.B0_BoardedQuantity; }
		}

		public ZString ShipmentAmendmentReasonCode
		{
			get
			{
				var amendmentReason = shipmentAction.B0_AmendmentReason;
				if (amendmentReason.IsEmpty && (ShipmentActionCode == MessageActionCodes.Codes.Change || shipment.IsSplit))
				{
					amendmentReason = ShipmentAmendmentCodes.Codes.C03;
				}
				return amendmentReason;
			}
		}

		public ZBool FDAFreightIndicator
		{
			get { return shipment.B0_IsFDAFreight; }
		}

		public ZBool ShipmentWasOutOfUSFor45DaysOrLessIndicator
		{
			get { return shipment.B0_WasOutOfUSFor45DaysOrLess; }
		}

		public ZDate ExportDate
		{
			get { return shipment.B0_DateOfExport.Date; }
		}

		public IEnumerable<ICommodity> Commodities
		{
			get { return from commodity in shipment.Commodities select (ICommodity)new CommodityWrapper(commodity); }
		}

		public IEnumerable<IParty> Parties
		{
			get
			{
				yield return new PartyWrapper(shipment.Shipper, shipment.IsSplit);
				yield return new PartyWrapper(shipment.Consignee, shipment.IsSplit);
				foreach (var party in shipment.Parties)
				{
					yield return new PartyWrapper(party);
				}
			}
		}

		public IInBond InBond
		{
			get { return shipment.B0_ShipmentType == ShipmentTypes.Codes.Inbond ? new InBondWrapper(shipment.InBond) : null; }
		}

		public bool IsLodged
		{
			get { return shipment.B0_IsLodged; }
		}

		public bool IsSplit
		{
			get { return shipment.IsSplit; }
		}

		#endregion

		readonly ShipmentAction shipmentAction;
		readonly Shipment shipment;
	}
}
