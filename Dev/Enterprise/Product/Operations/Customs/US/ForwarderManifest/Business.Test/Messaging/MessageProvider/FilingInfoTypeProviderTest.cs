using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class FilingInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestSenderID()
		{
			var org = GlbCompany.CurrentCompany.OrgProxy;
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCC", "123", "US");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCP", "456", "US");

			manifestHeader.AMA_TransportMode = "AIR";
			AssertEquals(FilingInfoTypeProvider.AirSenderID, provider.SenderId.Value);

			manifestHeader.AMA_TransportMode = "SEA";
			AssertEquals(FilingInfoTypeProvider.SeaSenderID, provider.SenderId.Value);
		}

		public void TestMessageControlNumber()
		{
			manifestHeader.AMA_JobReference = "123456";
			AssertEquals("123456", provider.MessageControlNumber.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			provider = new FilingInfoTypeProvider(bill, "A");
		}
		IFilingInfoType provider;
		USExportAsycudaManifestHeader manifestHeader;
	}
}
