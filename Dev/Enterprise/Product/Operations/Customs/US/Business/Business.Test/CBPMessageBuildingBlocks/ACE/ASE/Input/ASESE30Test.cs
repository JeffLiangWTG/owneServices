using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE30Test : TestCaseWithFactory
	{
		public void TestCompanyDetail()
		{
			var asese30 = new ASESE30()
			{
				EntityCode = "CN",
				EntityIdentifierQualifier = EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber,
				EntityIdentifier = "12345678",
				EntityName = "TEST"
			};

			AssertEquals("CN", ((IACEBIRDOrgCompanyRecord)asese30).OrganizationType);
			AssertEquals(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ((IACEBIRDOrgCompanyRecord)asese30).CustomsNoType);
			AssertEquals("12345678", ((IACEBIRDOrgCompanyRecord)asese30).CustomsNumber);
			AssertEquals("TEST", ((IACEBIRDOrgCompanyRecord)asese30).CompanyName);
		}

		public void TestManufacturerWithNumber()
		{
			var asese30 = new ASESE30()
			{
				EntityCode = "MF",
				EntityIdentifierQualifier = "MID",
				EntityIdentifier = "USFTZ234BLE",
				EntityName = "USFTZ234BLE"
			};

			AssertEquals("MF", ((IACEBIRDOrgCompanyRecord)asese30).OrganizationType);
			AssertEquals(OrgCusCode.USACodeTypes.ManufacturerID, ((IACEBIRDOrgCompanyRecord)asese30).CustomsNoType);
			AssertEquals("USFTZ234BLE", ((IACEBIRDOrgCompanyRecord)asese30).CustomsNumber);
			AssertEquals("USFTZ234BLE", ((IACEBIRDOrgCompanyRecord)asese30).CompanyName);
		}
	}
}
