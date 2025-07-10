using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USMIDQuery))]
	public class USMIDQueryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckUS_MID()
		{
			var data = new USMIDQuery(Factory);
			data.US_MID = "MID242";
			AssertHasError(data.US_MIDInfo, "Manufacturer ID must be a minimum of 7, maximum of 15 characters, containing only alpha and numeric characters.");
			data.US_MID = "US123456";
			AssertNoError(data.US_MIDInfo, "Manufacturer ID must be a minimum of 7, maximum of 15 characters, containing only alpha and numeric characters.");

			data.US_MID = " U ~S!1@2#3$4%5^6& ";
			AssertEquals("US123456", data.US_MID);
			AssertNoError(data.US_MIDInfo, "Manufacturer ID must be a minimum of 7, maximum of 15 characters, containing only alpha and numeric characters.");
		}

		public void TestCheckUS_MIDDuplicates()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			var cusCode1 = org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "CNSHANGH1106SHA");

			var data = new USMIDQuery(Factory);
			data.US_AutoCreateOrganization = true;
			data.US_MID = "CNSHANGH1106SHA";
			AssertHasErrorContaining(data.US_MIDInfo, "This Manufacturer ID already exists on ");

			data.US_MID = "XXXYYYUUUI";
			AssertNoErrorContaining(data.US_MIDInfo, "This Manufacturer ID already exists on ");
		}

		public void TestCheckValidateUS_AutoCreateOrganization()
		{
			var data = new USMIDQuery(Factory);
			data.US_AutoCreateOrganization = false;
			AssertNoErrorContaining(data.US_AutoCreateOrganizationInfo, USMIDQuery.UserCreateNewOrganizationError);

			Env.Security.OrganisationNew.IsAllowed = false;
			data.US_AutoCreateOrganization = true;
			AssertHasErrorContaining(data.US_AutoCreateOrganizationInfo, USMIDQuery.UserCreateNewOrganizationError);

			Env.Security.OrganisationNew.IsAllowed = true;
			data.US_AutoCreateOrganization = true;
			AssertNoErrorContaining(data.US_AutoCreateOrganizationInfo, USMIDQuery.UserCreateNewOrganizationError);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USMIDQuery(Factory);
		}
	}
}
