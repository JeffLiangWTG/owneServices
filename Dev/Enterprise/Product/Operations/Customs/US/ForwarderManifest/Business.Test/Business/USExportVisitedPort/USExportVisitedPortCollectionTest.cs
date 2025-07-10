using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportVisitedPortCollection))]
	public class USExportVisitedPortCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestVisitedPortList()
		{
			var job = Factory.New<USExportAsycudaManifestHeader>();
			var bill = job.Bills.AddNew();
			var port = bill.VisitedPorts.AddNew();
			port.CY_Data = "CATOR";
			AssertEquals(typeof(USExportVisitedPortCollection), bill.VisitedPorts.GetType());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<USExportAsycudaBill>();
			bill.Packs.AddNew();
			return new USExportVisitedPortCollection(bill);
		}
	}
}
