using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE50Test : TestCaseWithFactory
	{
		public void TestCompanyDetail()
		{
			var asese50 = new ASESE50()
			{
				EntityCode = "CN",
				EntityIdentifierQualifier = EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber,
				EntityIdentifier = "12345678",
				EntityName = "TEST"
			};

			AssertEquals("CN", ((IACEBIRDOrgCompanyRecord)asese50).OrganizationType);
			AssertEquals(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, ((IACEBIRDOrgCompanyRecord)asese50).CustomsNoType);
			AssertEquals("12345678", ((IACEBIRDOrgCompanyRecord)asese50).CustomsNumber);
			AssertEquals("TEST", ((IACEBIRDOrgCompanyRecord)asese50).CompanyName);
		}

		public void TestManufacturerWithNumber()
		{
			var asese50 = new ASESE50()
			{
				EntityCode = "MF",
				EntityIdentifierQualifier = "MID",
				EntityIdentifier = "USFTZ234BLE",
				EntityName = "USFTZ234BLE"
			};

			AssertEquals("MF", ((IACEBIRDOrgCompanyRecord)asese50).OrganizationType);
			AssertEquals(OrgCusCode.USACodeTypes.ManufacturerID, ((IACEBIRDOrgCompanyRecord)asese50).CustomsNoType);
			AssertEquals("USFTZ234BLE", ((IACEBIRDOrgCompanyRecord)asese50).CustomsNumber);
			AssertEquals("USFTZ234BLE", ((IACEBIRDOrgCompanyRecord)asese50).CompanyName);
		}
	}
}
