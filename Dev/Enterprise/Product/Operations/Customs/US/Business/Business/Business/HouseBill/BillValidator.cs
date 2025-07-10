using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class BillValidator : Customs.Business.BillValidator
	{
		public static class Constants
		{
			public const int MaximumBillLength = 12;
			public const int MaximumFTZBillLength = 35;

			public const string BillInvalidCharacters = " contains non-alphanumeric characters. Messages will be sent without those characters.";
			public const string BillTooLong = " is too long. Messages will be sent using the first {0} characters only.";
			public const string BillPrefix = " If you have prefixed this Bill No. with the Issuer's SCAC code, you can remove the SCAC code component as this value is sent seperately in the entry.";
			public const string OnlyOneBillAllowedForLowValueEntries = "Only one {0} is allowed per entry type 86.";
		}

		public static ZString GetTruncatedBillNumber(ZString billNumber)
		{
			return billNumber.KeepAlphanumericCharacters().Left(Constants.MaximumBillLength);
		}

		public void CheckInvalidCharacters(ZPropertyInfo billInfo, string billType)
		{
			ZPropertyInfoString stringBillInfo = billInfo as ZPropertyInfoString;

			if (stringBillInfo != null)
			{
				billType = string.IsNullOrEmpty(billType) ? "Bill" : billType;

				ZString billNumber = stringBillInfo.Value;

				if (billNumber.KeepAlphanumericCharacters().Length != billNumber.Length)
				{
					billInfo.AddWarning(billType + Constants.BillInvalidCharacters);
				}
			}
		}

		public void CheckInvalidLength(ZPropertyInfo billInfo, string billType, int maxLength)
		{
			var stringBillInfo = billInfo as ZPropertyInfoString;

			if (stringBillInfo != null)
			{
				billType = string.IsNullOrEmpty(billType) ? "Bill" : billType;

				if (stringBillInfo.Value.Length > maxLength)
				{
					billInfo.AddWarning(billType + string.Format(Constants.BillTooLong, maxLength) + Constants.BillPrefix);
				}
			}
		}

		protected override string GetWarningMessage(ZString billValue, BaseJobDeclaration declaration)
		{
			var usDeclaration = (JobDeclaration)declaration;
			return new USAirWayBillValidator(usDeclaration).GetWarningMessage(billValue);
		}

		public ZBool CheckNumberOfBillsForLowValueEntries(JobDeclaration declaration, ZPropertyInfo propertyInfo, ZString billType)
		{
			if (declaration.IsACE && declaration.IsLowValue)
			{
				var numberOfBillsForType = declaration.Bills.FindByBillType(billType).Length;
				if (numberOfBillsForType > 1)
				{
					propertyInfo.AddMessageError(ZString.Format(Constants.OnlyOneBillAllowedForLowValueEntries, billType == BillTypeList.Codes.MasterBill ? BillTypeList.Descriptions.MasterBill : BillTypeList.Descriptions.HouseBill));
					return true;
				}
			}

			return false;
		}

		protected override (ZDBOnlyQuery DeclarationQuery, ZDBOnlySubQuery MasterBillQuery) GetDeclarationWithSameDirectMasterBillCore(BaseJobDeclaration declaration, ZString masterBillNum)
		{
			var declarationWithBillQuery = base.GetDeclarationWithSameDirectMasterBillCore(declaration, masterBillNum);
			var declarationQuery = declarationWithBillQuery.DeclarationQuery;
			var masterBillSubQuery = declarationWithBillQuery.MasterBillQuery;

			if (declaration is JobDeclaration usDeclaration && !usDeclaration.IsExport && (usDeclaration.IsSea || usDeclaration.IsRail || usDeclaration.IsTruck || usDeclaration.IsAir))
			{
				masterBillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_AddInfo, SQLComparisonOperator.Contains, $"UI_NKBillIssuerSCAC={usDeclaration.JE_MasterBillIssuerSCAC}"); // A query string param
			}

			return (declarationQuery, masterBillSubQuery);
		}
	}
}
