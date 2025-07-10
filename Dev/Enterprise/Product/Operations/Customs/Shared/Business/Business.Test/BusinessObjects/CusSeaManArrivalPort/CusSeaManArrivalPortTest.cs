using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManArrivalPort))]
	public class CusSeaManArrivalPortTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsAutoLogged()
		{
			Header.BT_VoyageNum = "9E9S";
			AssertEquals(0, Arrival.Logs.GetAllLogs().Count);
			Factory.Save();
			AssertEquals(1, Arrival.Logs.GetAllLogs().Count);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Arrival Port", Arrival.HumanReadableName);
			Arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("Arrival Port AUSYD", Arrival.HumanReadableName);
		}

		public void TestHeader()
		{
			AssertEquals("Header", Header, Arrival.Header);
		}

		public void TestMessages()
		{
			AssertNotNull(Arrival.Messages);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals(false, Arrival.StatusNeedsRecalculation);
			Arrival.Messages.AddNew().EM_MessageText = "12#";
			AssertEquals(true, Arrival.StatusNeedsRecalculation);
		}

		public void TestIsAnyCargoDischargingHere()
		{
			Arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("IsAnyCargoDischargingHere", false, Arrival.IsAnyCargoDischargingHere);
			CusSeaManOBLHeader oceanBill = Header.OceanBills.AddNew();
			AssertEquals("IsAnyCargoDischargingHere", false, Arrival.IsAnyCargoDischargingHere);
			oceanBill.BO_RL_NKDischargePort = "AUSYD";
			AssertEquals("IsAnyCargoDischargingHere", true, Arrival.IsAnyCargoDischargingHere);
		}

		public void TestLoadFromSendersReference()
		{
			CusSeaManArrivalPort arrival = Factory.New<CusSeaManArrivalPort>();
			arrival.BA_SendersMessageReference = "123";
			AssertEquals("Arrival", arrival, CusSeaManArrivalPort.LoadFromSendersReference(Factory, "123"));
		}

		CusSeaManTranHead header;
		CusSeaManTranHead Header
		{
			get
			{
				return header ?? (header = Factory.New<CusSeaManTranHead>());
			}
		}
		CusSeaManArrivalPort arrival;
		CusSeaManArrivalPort Arrival
		{
			get
			{
				return arrival ?? (arrival = Header.Arrivals.AddNew());
			}
		}
	}
}
