using System.Text.RegularExpressions;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondAirWayBillValidator : AirWayBillValidator
	{
		public CusInBondAirWayBillValidator(CusInBondBill bill)
		{
			this.bill = bill;
		}
		readonly CusInBondBill bill;

		protected override bool IsMatchingBillFormat(string masterBillNumber)
		{
			return !bill.MasterBillCarrierHasNumericBillPrefix ? Regex.IsMatch(masterBillNumber, @"^[A-Z0-9]{3}\d{8}$") : base.IsMatchingBillFormat(masterBillNumber);
		}

		protected override string MAWBLengthWarningMessage
		{
			get
			{
				var warningMessage = base.MAWBLengthWarningMessage;
				if (!bill.MasterBillCarrierHasNumericBillPrefix)
				{
					warningMessage = MasterBillLengthWarningMessage;
				}
				return warningMessage;
			}
		}

		internal const string MasterBillLengthWarningMessage = "Check the masterbill number format. The first 3 characters should be the Airline Code, followed by 8 numeric characters (including check digit).";

		protected override string InvalidMAWBFormatWarningMessage
		{
			get
			{
				var warningMessage = base.InvalidMAWBFormatWarningMessage;
				if (!bill.MasterBillCarrierHasNumericBillPrefix)
				{
					warningMessage = MasterBillFormatWarningMessage;
				}
				return warningMessage;
			}
		}
		internal const string MasterBillFormatWarningMessage = "The masterbill number must contain numeric characters in positions 4-11.";
	}
}
