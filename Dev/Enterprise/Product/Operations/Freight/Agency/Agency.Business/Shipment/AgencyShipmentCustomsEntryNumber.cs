using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentCustomsEntryNumber : CommonShipmentCustomsEntryNumber
	{
		public AgencyShipmentCustomsEntryNumber(AgencyShipment shipment)
			: base(shipment)
		{
		}

		public new AgencyShipment Shipment { get { return (AgencyShipment)base.Shipment; } }

		#region EntryType

		protected override ZString GetEntryType()
		{
			return Shipment.CusEntryNumbers.Count > 0 ? base.GetEntryType() : ZString.Empty;
		}

		protected override void SetEntryType(ZString value)
		{
			if (EntryType != value)
			{
				if (value.IsEmpty)
				{
					if (Shipment.CusEntryNumbers.Count > 0)
					{
						base.SetEntryType(value);
						SetEntryNumber(ZString.Empty);
					}
				}
				else
				{
					base.SetEntryType(value);
				}
			}
		}

		protected override ZString DefaultEntryType
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region EntryNumber

		protected override void SetEntryNumber(ZString value)
		{
			if (value != EntryNumber)
			{
				base.SetEntryNumber(value);

				if (value.IsEmpty)
				{
					if (Shipment.CusEntryNumbers.Count > 0)
					{
						base.SetEntryType(ZString.Empty);
					}
				}
			}
		}

		#endregion

		protected override bool DeleteCustomsEntriesIfNumberIsEmpty
		{
			get { return false; }
		}

		protected override ShipmentCustomsEntryNumberValidation GetNewValidation()
		{
			return new AgencyShipmentCustomsEntryNumberValidation(this);
		}
	}
}
