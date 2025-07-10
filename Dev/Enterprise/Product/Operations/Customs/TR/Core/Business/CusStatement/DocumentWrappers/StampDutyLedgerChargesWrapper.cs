using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class StampDutyLedgerChargesWrapper : NonPersistentBusinessObject
	{
		public StampDutyLedgerChargesWrapper(CusStatementLineCharge lineCharge) : base(lineCharge.Factory)
		{
			this.lineCharge = lineCharge;
			this.statementLine = lineCharge.StatementLine;
		}

		readonly CusStatementLineCharge lineCharge;
		readonly CusStatementLine statementLine;

		public ZString ChargeReferenceNumber => lineCharge.B4_ReferenceNumber;

		public ZString ChargeKind => StampDutyTaxCodeList.GetChargeTypeCodeByDutyTaxCode(lineCharge.B4_ChargeType);

		public ZString ChargeType => new StampDutyChargeTypeList().GetDescriptionFromCode(ChargeKind);

		public ZString StampDutyBaseAmount => StampDutyTaxCodeList.IsFixValueOfCustomsFeesTotalByChargeType(lineCharge.B4_ChargeType) ? FixedTax : statementLine.B3_CustomsFeesTotal.ToString();

		public ZString StampDutyRate => GetStampDutyPercentage().ToString("N5", ObjectCache.CultureProvider.Culture.NumberFormat);

		ZDecimal GetStampDutyPercentage()
		{
			var result = ZDecimal.Zero;
			if (!statementLine.B3_CustomsFeesTotal.IsEmpty)
			{
				result = new ZDecimal(lineCharge.B4_ChargeAmount / statementLine.B3_CustomsFeesTotal * 100.0m).Round(5);
			}

			return result;
		}

		public ZDecimal StampDutyAmount => statementLine.B3_Status == StatementLineStatusList.Codes.INA ? ZDecimal.Zero : lineCharge.B4_ChargeAmount;

		public ZString LineEntryDate => statementLine.B3_EntryDate.ToString("dd/MM/yyyy");
		public ZString LineAssociatedEntry => statementLine.B3_AssociatedEntry;
		public ZString LineBrokerReference => statementLine.B3_BrokerReference + LineBrokerReferenceTurkishDescExtentionForDocument;
		public ZString LineStatus => statementLine.B3_Status;

		const string FixedTax = "MAKTU";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised string")]
		const string LineBrokerReferenceTurkishDescExtentionForDocument = " Nolu İş";
	}
}

