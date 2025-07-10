using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentAction : AutoShipmentAction
	{
		public ShipmentAction(Shipment shipment, string messageType)
			: base(shipment.Factory)
		{
			this.messageType = messageType;
			Argument.NotNull(shipment, "shipment");
			Shipment = shipment;
			base.SetPKAndDefaults();
			SetDefaultActionCode();
		}

		#region Properties

		#region B0_ShipmentControlNumber

		public override ZString B0_ShipmentControlNumber
		{
			get { return Shipment.B0_MasterBillNumber; }
		}

		public override ZPropertyInfo B0_ShipmentControlNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.B0_ShipmentControlNumber, x => Shipment.B0_MasterBillNumberInfo); }
		}

		#endregion

		#region B0_ActionCode

		[List(nameof(Lookups) + "." + nameof(ShipmentActionLookups.ActionCodes))]
		public override ZString B0_ActionCode
		{
			get { return base.B0_ActionCode; }
			set
			{
				var hasChanges = base.B0_ActionCode != value;
				base.B0_ActionCode = value;
				if (hasChanges && !B0_AmendmentReason.IsEmpty)
				{
					B0_AmendmentReason = ZString.Empty;
				}
			}
		}

		public ZString B0_ActionCodeDescription
		{
			get { return Lookups.ActionCodes.GetDescriptionFromCode(B0_ActionCode); }
		}

		#endregion

		#region B0_AmendmentReason

		[List(nameof(Lookups) + "." + nameof(ShipmentActionLookups.AmendmentReasonCodes))]
		public override ZString B0_AmendmentReason
		{
			get { return base.B0_AmendmentReason; }
			set { base.B0_AmendmentReason = value; }
		}

		protected bool B0_AmendmentReason_ReadOnly
		{
			get { return B0_ActionCode != MessageActionCodes.Codes.Change; }
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetPKAndDefaults()
		{
			//NOTE: Do nothing here, as it throws exception because Shipment is not set yet.
		}

		void SetDefaultActionCode()
		{
			if (LinkingOrDeLinkingShipment)
			{
				base.B0_ActionCode = (Shipment.B0_IsLodged || Shipment.IsSplit) && !Shipment.IsLinked ? MessageActionCodes.Codes.Link : MessageActionCodes.Codes.DoNotSend;
			}
			else
			{
				base.B0_ActionCode = Shipment.B0_IsLodged ? MessageActionCodes.Codes.Change : MessageActionCodes.Codes.Original;
			}
		}

		internal bool LinkingOrDeLinkingShipment
		{
			get { return messageType == MessageTypes.Codes.CompleteTrip || messageType == MessageTypes.Codes.PreliminaryTrip; }
		}

		public Shipment Shipment { get; private set; }

		public ShipmentActionLookups Lookups
		{
			get { return lookups ?? (lookups = new ShipmentActionLookups(this)); }
		}

		ShipmentActionLookups lookups;
		readonly string messageType;

		#endregion
	}
}
