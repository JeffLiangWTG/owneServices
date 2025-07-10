using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.Business
{
	public class AESTransportationReferenceNumberValidator : AirWayBillValidator
	{
		protected override string MAWBLengthWarningMessage
		{
			get { return ""; }
		}

		protected override string InvalidMAWBFormatWarningMessage
		{
			get { return ""; }
		}

		protected override int MAWBLength
		{
			get { return 12; }
		}

		protected override bool IsMatchingBillFormat(string masterBillNumber)
		{
			return Regex.IsMatch(masterBillNumber, @"^[0-9]{3}-[0-9]{8}$");
		}

		protected override string GetNumberForCheckDigit(ZString masterBillNumber)
		{
			return masterBillNumber.Left(3) + masterBillNumber.SubstringSafe(4);
		}

		public static bool HasLeadingOrEmbeddedSpaces(ZString masterBillNumber)
		{
			return masterBillNumber.Contains(" ", System.StringComparison.CurrentCulture);
		}
	}
}
