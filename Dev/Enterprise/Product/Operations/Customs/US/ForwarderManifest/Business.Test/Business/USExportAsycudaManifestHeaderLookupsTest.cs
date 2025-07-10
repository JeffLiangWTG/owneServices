using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test.Business
{
	sealed class USExportAsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportModeList()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var list = header.Lookups.TransportModeList;

			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(TransportTypeList.Codes.Air));
			Assert(list.ContainsCode(TransportTypeList.Codes.Sea));
			Assert(list.ContainsCode(TransportTypeList.Codes.Rail));
		}

		public void TestCustomsFirstArrivalPortList()
		{
			RefUNLOCOTestDataHelper.CreateScheduleKPort(Factory, true);
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();

			header.AMA_RL_NKPortOfFirstArrival = "TEST1";
			AssertEquals(2, header.Lookups.CustomsFirstArrivalPortList.Count);
			Assert(header.Lookups.CustomsFirstArrivalPortList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60001"));
			Assert(header.Lookups.CustomsFirstArrivalPortList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60002"));

			header.AMA_RL_NKPortOfFirstArrival = "TEST2";
			AssertEquals(0, header.Lookups.CustomsFirstArrivalPortList.Count);
		}

		public void TestCustomsFinalDeparturePortList()
		{
			RefUNLOCOTestDataHelper.CreateScheduleDPort(Factory, true);
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_RL_NKPortOfFinalDeparture = "USTES";

			var list = header.Lookups.CustomsFinalDeparturePortList;
			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4001"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4002"));

			header.AMA_TransportMode = "AIR";
			AssertEquals(0, header.Lookups.CustomsFinalDeparturePortList.Count);
		}
	}
}
