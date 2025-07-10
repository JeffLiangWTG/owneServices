using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class USExportVisitedPortValidationTest : TestCaseWithFactory
	{
		public void TestPortCY_Code()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var port = bill.VisitedPorts.AddNew();
			port.CY_Code = "XXX";
			port.Validation.ValidateCY_Code();
			AssertNoNotifications(port.CY_CodeInfo);
		}
	}
}
