using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class EntityIdentifierNumberValidationTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ZPropertyInfoString info = (ZPropertyInfoString)header.BF_MasterBillInfo;
			info.Value = "Z!Z";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.CBPAssignedNumber);
			AssertHasMessageError(info, CBPAssignedNumberValidator.CBPAssignedNumberRightFormat);
			info.Value = "";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.CBPAssignedNumber);
			AssertNoMessageError(info, CBPAssignedNumberValidator.CBPAssignedNumberRightFormat);
			info.Value = "078888-12345";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.CBPAssignedNumber);
			AssertNoMessageError(info, CBPAssignedNumberValidator.CBPAssignedNumberRightFormat);
			info.Value = "078888123";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.DUNS);
			AssertHasWarning(info, "DUNS number is no longer supported by CBP");
			info.Value = "0788881231234";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.DUNSPlus4);
			AssertHasWarning(info, "DUNS number is no longer supported by CBP");
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.EncryptedConsigneeNumber);
			AssertHasMessageError(info, EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat);
			info.Value = "";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.EncryptedConsigneeNumber);
			AssertNoMessageError(info, EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat);
			info.Value = "-L8E9QX17S-P";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.EncryptedConsigneeNumber);
			AssertNoMessageError(info, EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat);
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.FIRMS);
			AssertHasMessageError(info, FIRMSCodeValidator.FIRMSCodeRightFormat);
			info.Value = "";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.FIRMS);
			AssertNoMessageError(info, FIRMSCodeValidator.FIRMSCodeRightFormat);
			info.Value = "A4A8";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.FIRMS);
			AssertNoMessageError(info, FIRMSCodeValidator.FIRMSCodeRightFormat);
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.IRS);
			AssertHasMessageError(info, EmployerIdentificationNumberValidator.EINNumberFullFormatOnly);
			info.Value = "";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.IRS);
			AssertNoMessageError(info, EmployerIdentificationNumberValidator.EINNumberFullFormatOnly);
			info.Value = "12-3456789XY";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.IRS);
			AssertNoMessageError(info, EmployerIdentificationNumberValidator.EINNumberFullFormatOnly);
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.SocialSecurity);
			AssertHasMessageError(info, SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
			info.Value = "";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.SocialSecurity);
			AssertNoMessageError(info, SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
			info.Value = "123-12-1234";
			EntityIdentifierNumberValidation.Validate(info, CodeTypeList.Codes.SocialSecurity);
			AssertNoMessageError(info, SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
			if (ErrorReporter.LastKeyReported == "Validation:BF_MasterBill")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
