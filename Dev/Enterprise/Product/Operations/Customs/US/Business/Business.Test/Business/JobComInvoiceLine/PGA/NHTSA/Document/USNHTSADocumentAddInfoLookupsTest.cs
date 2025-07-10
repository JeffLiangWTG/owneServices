using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USNHTSADocumentAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentTypes()
		{
			var documentTypes = lookups.DocumentTypes;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "165, 871, 872, 873, 874, 875, 946, 958", documentTypes.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<NHTSADocumentTypeList>(), documentTypes);
			});
		}

		public void TestOrganizationTypes()
		{
			var organizationTypes = lookups.OrganizationTypes;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "CI, CN, FM, IM, MF, OVM, DFP, RD", organizationTypes.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<NHTSAOrganizationTypeList>(), organizationTypes);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var document = Factory.New<NHTSADocument>();
			var addInfo = new USNHTSADocumentAddInfo(document.B7_AddInfoDataInfo);
			lookups = new USNHTSADocumentAddInfoLookups(addInfo);
		}
		USNHTSADocumentAddInfoLookups lookups;
	}
}
