using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AddressCodeDescriptionHelperTest : TestCaseWithFactory
	{
		public void TestGetAddressDescriptionFromCode()
		{
			var helper = new AddressCodeDescriptionHelper(Factory);
			var orgAddressTypeCode = "OrgAddress";
			var unKnowTypeCode = "unKnowTypeCode";
			var consignorDocumentaryAddressCode = "CRD";
			var noneTypeCode = "None";

			CombineAssertions(() =>
			{
				AssertEquals("Organization Address", helper.GetAddressDescriptionFromCode(orgAddressTypeCode));
				AssertEquals(DocAddressTypes.Unspecified.ToString(), helper.GetAddressDescriptionFromCode(unKnowTypeCode));
				AssertEquals("Consignor Documentary Address", helper.GetAddressDescriptionFromCode(consignorDocumentaryAddressCode));
				AssertEquals("None", helper.GetAddressDescriptionFromCode(noneTypeCode));
			});
		}
	}
}
