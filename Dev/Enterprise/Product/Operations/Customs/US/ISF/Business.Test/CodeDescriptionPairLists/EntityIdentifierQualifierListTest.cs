using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class EntityIdentifierQualifierListTest : TestCase
	{
		public void TestGetCodeFromCusCodeType()
		{
			AssertEquals(EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
			AssertEquals(EntityIdentifierQualifierList.Codes.CBPAssignedNumber, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.USACodeTypes.CBPAssignedNumber));
			AssertEquals(EntityIdentifierQualifierList.Codes.CBPEncryptedConsigneeNumber, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber));
			AssertEquals(EntityIdentifierQualifierList.Codes.SocialSecurityNumber, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.USACodeTypes.SocialSecurityNumber));
			AssertEquals(EntityIdentifierQualifierList.Codes.DUNSNumber, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.CodeTypes.DataUniversalNumberingSystem));
			AssertEquals(EntityIdentifierQualifierList.Codes.DUNSPlust4Number, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4));
			AssertEquals(EntityIdentifierQualifierList.Codes.FIRMSCode, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.USACodeTypes.FIRMSCode));
			AssertEquals(EntityIdentifierQualifierList.Codes.PassportNumber, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.CodeTypes.PassportID));
			AssertEquals(EntityIdentifierQualifierList.Codes.SCAC, EntityIdentifierQualifierList.GetCodeFromCusCodeType(OrgCusCode.CodeTypes.CarrierCode));
		}
	}
}
