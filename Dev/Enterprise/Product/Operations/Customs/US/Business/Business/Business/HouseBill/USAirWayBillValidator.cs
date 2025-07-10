using System.Text.RegularExpressions;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.Business
{
	public class USAirWayBillValidator : AirWayBillValidator
	{
		public USAirWayBillValidator(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		protected override string MAWBLengthWarningMessage
		{
			get
			{
				var warningMessage = base.MAWBLengthWarningMessage;
				if (IsImportDeclarationWithNonNumericMasterBillPrefix)
				{
					warningMessage = "Check the MAWB format. The first 3 characters should be the Airline Code, followed by 8 numeric characters (including check digit).";
				}

				return warningMessage;
			}
		}

		protected override string InvalidMAWBFormatWarningMessage
		{
			get
			{
				var warningMessage = base.InvalidMAWBFormatWarningMessage;
				if (IsImportDeclarationWithNonNumericMasterBillPrefix)
				{
					warningMessage = "The MAWB must contain numeric characters in positions 4-11.";
				}
				return warningMessage;
			}
		}

		protected override bool IsMatchingBillFormat(string masterBillNumber)
		{
			var result = base.IsMatchingBillFormat(masterBillNumber);
			if (IsImportDeclarationWithNonNumericMasterBillPrefix)
			{
				result = Regex.IsMatch(masterBillNumber, @"^[A-Z0-9]{3}[0-9]{8}$");
			}
			return result;
		}

		protected virtual bool IsImportDeclarationWithNonNumericMasterBillPrefix
		{
			get { return declaration.IsImport && !declaration.MasterBillCarrierHasNumericBillPrefix; }
		}
	}
}
