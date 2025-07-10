using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class USExportVisitedPortLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefUNLOCOCodeList()
		{
			var job = Factory.New<USExportAsycudaManifestHeader>();
			var bill = job.Bills.AddNew();
			var port = bill.VisitedPorts.AddNew();
			port.CY_Data = "CATOR";
			AssertEquals(typeof(USExportVisitedPortCollection), bill.VisitedPorts.GetType());
			AssertType<RefUNLOCOCollection>(port.Lookups.UNLOCOCodeList);
		}

		public void TestScheduleKCodeList()
		{
			RefUNLOCOTestDataHelper.CreateScheduleKPort(Factory, true);

			var header = Factory.New<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var port = bill.VisitedPorts.AddNew();
			port.CY_Data = "TEST1";
			var list = port.Lookups.ScheduleKCodeList;
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60002"));

			header = Factory.New<USExportAsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			port = bill.VisitedPorts.AddNew();
			port.CY_Data = "TEST2";
			AssertEquals(0, port.Lookups.ScheduleKCodeList.Count);
		}
	}
}
