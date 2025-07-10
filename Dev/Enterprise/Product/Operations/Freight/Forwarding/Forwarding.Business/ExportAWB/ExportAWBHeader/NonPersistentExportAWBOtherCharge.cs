using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DebuggerDisplay("ChargeCode: {ChargeCode}, EntitlementCode: {EntitlementCode}, PPDCLT: {PPDCLT}, Amount: {Amount}")]
	public class NonPersistentExportAWBOtherCharge
	{
		public NonPersistentExportAWBOtherCharge(ExportAWBHeader awbHeader)
		{
			Argument.NotNull(awbHeader, "awbHeader");
			this.awbHeader = awbHeader;
		}

		readonly ExportAWBHeader awbHeader;

		public ZString ChargeCode
		{
			get { return chargeCode; }
			set
			{
				if (chargeCode != value)
				{
					chargeCode = value;

					if (chargeCode != ZString.Empty)
					{
						ChargeDescription = IATAChargeCodesList.GetDescriptionFromCode(chargeCode);
					}
				}
			}
		}
		ZString chargeCode;

		public ZString ChargeDescription { get; set; }
		public ZString EntitlementCode { get; set; }
		public ZString PPDCLT { get; set; }
		public ZDecimal Amount { get; set; }
		public ZString Currency { get; set; }

		public CodeDescriptionPairList IATAChargeCodesList
		{
			get { return iataChargeCodesList ?? (iataChargeCodesList = awbHeader.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AWBChargeCodes)); }
		}
		CodeDescriptionPairList iataChargeCodesList;

		public ExportAWBOtherCharges ToPersistentCharge()
		{
			ExportAWBOtherCharges charge = (ExportAWBOtherCharges)awbHeader.Factory.New(awbHeader.AWBOtherCharges.TypeOfElements);
			using (charge.SuspendSettingHasChanges())
			{
				charge.EO_ChargeCode = ChargeCode;
				charge.SetEO_ChargeDescriptionSafely(ChargeDescription);
				charge.EO_PPDCLT = PPDCLT;
				charge.EO_EntitlementCode = EntitlementCode;
				charge.EO_Amount = Amount;
				charge.Currency = Currency;
			}

			return charge;
		}

		public bool Equals(ExportAWBOtherCharges persistentCharge)
		{
			return persistentCharge != null &&
				persistentCharge.EO_ChargeCode == ChargeCode &&
				persistentCharge.EO_EntitlementCode == EntitlementCode &&
				persistentCharge.EO_PPDCLT == PPDCLT &&
				persistentCharge.EO_Amount == Amount;
		}
	}
}
