using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentCustomsEntryNumber : CommonShipmentCustomsEntryNumber
	{
		public ForwardingShipmentCustomsEntryNumber(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public new ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)base.Shipment; }
		}

		protected override void SetEntryNumber(ZString value)
		{
			ZString oldValue = EntryNumber;

			base.SetEntryNumber(value);

			if (EntryType == CusEntryNumberTypes.Standard.MovementReferenceNumber && value != oldValue)
			{
				var portMessagingHelper = ObjectFactory.Get<Integration.Forwarding.IPortMessagingForwardingHelper>();
				if (portMessagingHelper != null)
				{
					portMessagingHelper.SyncroniseMRN(Shipment.Factory, Shipment.PK, oldValue, value);
				}
			}
		}

		protected override bool EntryNumber_ReadOnly
		{
			get
			{
				return base.EntryNumber_ReadOnly
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland;
			}
		}

		protected override bool EntryType_ReadOnly
		{
			get
			{
				return base.EntryType_ReadOnly
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland;
			}
		}

		protected override ShipmentCustomsEntryNumberValidation GetNewValidation()
		{
			return new ForwardingShipmentCustomsEntryNumberValidation(this);
		}
	}
}
