using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ReferenceFileUrlProviderTest : TestCaseWithFactory
	{
		[TestDate(2016, 08, 16)]
		public void TestGetLegacyCustomsReferenceFileURL()
		{
			var provider = new ReferenceFileUrlProvider(Factory);
			AssertEquals(string.Empty, provider.GetCustomsReferenceFileURL());
		}

		[TestDate(2016, 08, 17)]
		public void TestGetCurrentCustomsReferenceFileURL()
		{
			var provider = new ReferenceFileUrlProvider(Factory);
			AssertEquals("https://www.ccf.border.gov.au/reference", provider.GetCustomsReferenceFileURL());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var configType = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfigType>();
			configType.ZRT_ConfigCode = "AURefURL";
			configType.ZRT_Description = "AU Customs Reference data URL";
			configType.ZRT_LongDescription = "URL root address to source CMR reference files for Australian Customs.";

			var config = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode;
			config.ZRC_StringValue = "https://www.ccf.border.gov.au/reference";
			config.ZRC_StartDate = new ZDateTime(2016, 08, 17);
			config.ZRC_EndDate = new ZDateTime(2079, 06, 06);

			Factory.Save();
		}
	}
}
