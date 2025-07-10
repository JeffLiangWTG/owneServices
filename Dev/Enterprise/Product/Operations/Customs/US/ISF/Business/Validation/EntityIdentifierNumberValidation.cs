using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public static class EntityIdentifierNumberValidation
	{
		public static void Validate(ZPropertyInfo info, ZString codeType)
		{
			ZString number = info.Value.ToString();
			if (!number.IsEmpty)
			{
				ZString errorMessage = ZString.Empty;
				switch (codeType)
				{
					case CodeTypeList.Codes.CBPAssignedNumber:
						errorMessage = CBPAssignedNumberValidator.Validate(number);
						break;
					case CodeTypeList.Codes.IRS:
						errorMessage = EmployerIdentificationNumberValidator.Validate(number, true);
						break;
					case CodeTypeList.Codes.SocialSecurity:
						errorMessage = SocialSecurityNumberValidator.Validate(number);
						break;
					case CodeTypeList.Codes.EncryptedConsigneeNumber:
						errorMessage = EncryptedConsigneeNumberValidator.Validate(number);
						break;
					case CodeTypeList.Codes.FIRMS:
						errorMessage = FIRMSCodeValidator.Validate(number);
						break;
					case CodeTypeList.Codes.DUNS:
					case CodeTypeList.Codes.DUNSPlus4:
						if (!number.IsEmpty)
						{
							info.AddWarning("DUNS number is no longer supported by CBP");
						}
						break;
				}
				if (!errorMessage.IsEmpty)
				{
					info.AddMessageError(errorMessage);
				}
			}
		}
	}
}
