using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentCustomsEntryNumberValidation : ShipmentCustomsEntryNumberValidation
	{
		public AgencyShipmentCustomsEntryNumberValidation(AgencyShipmentCustomsEntryNumber parent)
			: base(parent)
		{
		}

		protected override void CheckEntryType()
		{
			base.CheckEntryType();

			if (Parent.EntryType_List.Count == 0 && !Parent.EntryType.IsEmpty)
			{
				Parent.EntryTypeInfo.AddError(Res.GetString("1e459ca0-5c77-4897-b597-73112550c0e0", "Entry Type should be Empty"));
			}
		}

		protected override void CheckEntryNumber()
		{
			base.CheckEntryNumber();

			if (Parent.Shipment.CusEntryNumbers.Count > 0 && !Parent.EntryNumberInfo.ReadOnly)
			{
				ZString entryType = Parent.EntryType;
				ZString entryNumber = Parent.EntryNumber;

				if (entryNumber.IsEmpty)
				{
					Parent.EntryNumberInfo.AddWarning(Res.GetString("AFC7FCF4-8897-48e7-AD12-AEFFA670A2C1", "Entry Number is not specified"));
				}
				else if (entryNumber.Length > ExportCustomsManifestLinesSchema.EL_CAN.MaxLength)
				{
					Parent.EntryNumberInfo.AddWarning(Res.GetString("DBC133E9-ED32-43CC-BA51-15AC5F48E24D", "Entry Number is too long, only the first {0} characters will be transferred to the manifest.", ExportCustomsManifestLinesSchema.EL_CAN.MaxLength));
				}
				else if (entryType == CANType.CustomsAuthorityNumber.Code)
				{
					ZString invalidReason = new CANValidation().GetInvalidReason(entryNumber);
					if (!invalidReason.IsEmpty)
					{
						Parent.EntryNumberInfo.AddWarning(invalidReason);
					}
				}
			}
		}
	}
}
