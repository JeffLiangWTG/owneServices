using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class CusSeaManTranHeadLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAllArrivalPorts()
		{
			AssertEquals("Count", 0, TranHead.Lookups.AllArrivalPorts.Count);
			CusSeaManArrivalPort arrival = TranHead.Arrivals.AddNew();
			arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("Count", 1, TranHead.Lookups.AllArrivalPorts.Count);
			AssertEquals("AllArrivalPorts[0].Code", "AUSYD", TranHead.Lookups.AllArrivalPorts[0].Code);
		}

		CusSeaManTranHead fTranHead;
		CusSeaManTranHead TranHead
		{
			get
			{
				if (fTranHead == null)
				{
					fTranHead = Factory.New<CusSeaManTranHead>();
				}
				return fTranHead;
			}
		}
	}
}
