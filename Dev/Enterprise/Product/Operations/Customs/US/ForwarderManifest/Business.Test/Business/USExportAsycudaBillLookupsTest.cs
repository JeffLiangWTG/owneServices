using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test.Business
{
	public class USExportAsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInlandTransportMode()
		{
			var bill = Factory.New<USExportAsycudaBill>();
			var list = bill.Lookups.InlandTransportMode;

			AssertEquals(15, list.Count);
			AssertContainsExactElementsInAnyOrder(new PriorTransportationModeList(), list);
		}

		public void TestCarriersList()
		{
			var bill = Factory.New<USExportAsycudaBill>();
			var list = bill.Lookups.BillIssuers;

			AssertEquals("Carriers", typeof(USCarrierCombinedCollection), list.GetType());
		}

		public void TestBillOfLadingType()
		{
			var bill = Factory.New<USExportAsycudaBill>();
			var list = bill.Lookups.BillOfLadingType;

			AssertEquals(3, list.Count);
			AssertContainsExactElementsInAnyOrder(new BillOfLadingTypeList(), list);
		}

		public void TestCustomsLoadPortList()
		{
			AssertRegionDistrictPortList("ABL_RL_NKPortOfLoading", "CustomsLoadPortList");
		}

		public void TestCustomsOriginPortList()
		{
			AssertRegionDistrictPortList("ABL_RL_NKOrigin", "CustomsOriginPortList");
		}

		void AssertRegionDistrictPortList(string uNLOCOPropertyName, string codeListPropertyName)
		{
			RefUNLOCOTestDataHelper.CreateScheduleDPort(Factory, true);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			var bill = header.Bills.AddNew();

			bill.SetPropertyValue(uNLOCOPropertyName, new ZString("USTES"));
			var list = (ZZRefCusCodeListCombinedCollection)bill.Lookups.GetPropertyValue(codeListPropertyName);
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4002"));

			header.AMA_TransportMode = "AIR";
			list = (ZZRefCusCodeListCombinedCollection)bill.Lookups.GetPropertyValue(codeListPropertyName);
			AssertEquals(0, list.Count);
		}

		public void TestCustomsPortOfUnladingList()
		{
			AssertForeignPortList("ABL_RL_NKPortOfDischarge", "CustomsPortOfUnladingList");
		}

		public void TestCustomsFinalDestinationPortList()
		{
			AssertForeignPortList("ABL_RL_NKFinalDestination", "CustomsFinalDestinationPortList");
		}

		void AssertForeignPortList(string uNLOCOPropertyName, string codeListPropertyName)
		{
			RefUNLOCOTestDataHelper.CreateScheduleKPort(Factory, true);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.SetPropertyValue(uNLOCOPropertyName, new ZString("TEST1"));
			var list = (ZZRefCusCodeListCombinedCollection)bill.Lookups.GetPropertyValue(codeListPropertyName);
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60002"));

			bill.SetPropertyValue(uNLOCOPropertyName, new ZString("TEST2"));
			list = (ZZRefCusCodeListCombinedCollection)bill.Lookups.GetPropertyValue(codeListPropertyName);
			AssertEquals(0, list.Count);
		}
	}
}
