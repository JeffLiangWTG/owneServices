using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class RestrictedCodeValidation : Customs.Business.CusCodeDataValidation
	{
		public RestrictedCodeValidation(RestrictedCode parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.CY_Code == RestrictedCodeTypeList.Codes.RestrictedSPI || Parent.CY_Code == RestrictedCodeTypeList.Codes.RestrictedEntryType)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CY_DataInfo);
			}
			else if (Parent.CY_Code == RestrictedCodeTypeList.Codes.RestrictedTariff)
			{
				if (!Regex.IsMatch(Parent.CY_Data, "^[0-9]{2,4}$"))
				{
					Parent.CY_DataInfo.AddMessageError(InvalidRestrictedTariffCode);
				}
			}
			else if (Parent.CY_Code == RestrictedCodeTypeList.Codes.FTZAllowsFDA)
			{
				var messageError = FTZJobDeclarationValidation.GetFTZZoneIDFormatMessageErrorIfInvalid(FTZJobDeclarationValidation.FieldType.ZoneID, Parent.CY_Data);
				if (!messageError.IsEmpty)
				{
					Parent.CY_DataInfo.AddMessageError(messageError);
				}
			}
		}
		internal const string InvalidRestrictedTariffCode = "Code must contain only numbers and between 2 and 4 in length.";
	}
}
