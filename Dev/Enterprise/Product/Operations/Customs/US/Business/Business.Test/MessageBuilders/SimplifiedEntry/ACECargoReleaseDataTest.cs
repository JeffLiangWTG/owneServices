using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACECargoReleaseDataTest : TestCaseWithFactory
	{
		public void TestGetCustomsNumberType()
		{
			var numberType = ACECargoReleaseData.GetCustomsNumberType("12-34567");
			AssertEquals(ZString.Empty, numberType);

			numberType = ACECargoReleaseData.GetCustomsNumberType("12-3456789XY");
			AssertEquals(EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber, numberType);

			numberType = ACECargoReleaseData.GetCustomsNumberType("061234-1");
			AssertEquals(ZString.Empty, numberType);

			numberType = ACECargoReleaseData.GetCustomsNumberType("061234-12345");
			AssertEquals(EntityIdentifierQualifierList.Codes.CBPAssignedNumber, numberType);

			numberType = ACECargoReleaseData.GetCustomsNumberType("123-12-1234");
			AssertEquals(EntityIdentifierQualifierList.Codes.SocialSecurityNumber, numberType);
		}

		public void TestGetEntityCustomsNumberType()
		{
			var numberType = ACECargoReleaseData.GetEntityCustomsNumberType("-123-12 1233");
			AssertEquals(ZString.Empty, numberType);

			numberType = ACECargoReleaseData.GetEntityCustomsNumberType("-12A12CD34-1");
			AssertEquals(EntityIdentifierQualifierList.Codes.CBPEncryptedConsigneeID, numberType);

			numberType = ACECargoReleaseData.GetEntityCustomsNumberType("1 34");
			AssertEquals(ZString.Empty, numberType);

			numberType = ACECargoReleaseData.GetEntityCustomsNumberType("A002");
			AssertEquals(EntityIdentifierQualifierList.Codes.FIRMS, numberType);
		}
	}
}
