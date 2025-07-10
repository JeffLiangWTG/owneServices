using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	partial class EntityIdentifierQualifierList
	{
		public static ZString GetCodeFromCusCodeType(ZString codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.USACodeTypes.EmployerIdentificationNumber:
					return Codes.EmployerIdentificationNumber;
				case OrgCusCode.USACodeTypes.CBPAssignedNumber:
					return Codes.CBPAssignedNumber;
				case OrgCusCode.USACodeTypes.EncryptedConsigneeNumber:
					return Codes.CBPEncryptedConsigneeNumber;
				case OrgCusCode.USACodeTypes.SocialSecurityNumber:
					return Codes.SocialSecurityNumber;
				case OrgCusCode.CodeTypes.DataUniversalNumberingSystem:
					return Codes.DUNSNumber;
				case OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4:
					return Codes.DUNSPlust4Number;
				case OrgCusCode.USACodeTypes.FIRMSCode:
					return Codes.FIRMSCode;
				case OrgCusCode.CodeTypes.PassportID:
					return Codes.PassportNumber;
				case OrgCusCode.CodeTypes.CarrierCode:
					return Codes.SCAC;
			}
			return ZString.Empty;
		}
	}
}
