using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManTranHead))]
	public class CusSeaManTranHeadTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsAutoLogged()
		{
			AssertEquals(0, Header.Logs.GetAllLogs().Count);
			Factory.Save();
			AssertEquals(1, Header.Logs.GetAllLogs().Count);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Import Manifest", Header.HumanReadableName);
			Header.BT_SendersMessageReference = "foosh";
			AssertEquals("Import Manifest foosh", Header.HumanReadableName);
		}

		public virtual void TestBusinessObjectsWithRelatedLogs()
		{
			AssertEquals(0, Header.BusinessObjectsWithRelatedEvents.Length);

			CusSeaManOBLHeader oceanBill = header.OceanBills.AddNew();
			AssertEquals(1, Header.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains(oceanBill, Header.BusinessObjectsWithRelatedEvents);

			CusSeaManArrivalPort port = header.Arrivals.AddNew();
			AssertEquals(2, Header.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains(port, Header.BusinessObjectsWithRelatedEvents);
		}

		public virtual void TestArrivals()
		{
			AssertNotNull(Header.Arrivals);
			AssertEquals(true, Header.IsRegisteredEditableChildObject(Header.Arrivals));
		}

		public virtual void TestOceanBills()
		{
			AssertNotNull(Header.OceanBills);
			AssertEquals(true, Header.IsRegisteredEditableChildObject(Header.OceanBills));
		}

		public void TestOceanBillsView()
		{
			AssertNotNull(Header.OceanBillsView);
		}

		public virtual void TestSlotCharterers()
		{
			AssertNotNull(Header.SlotCharterers);
			AssertEquals(true, Header.IsRegisteredEditableChildObject(Header.SlotCharterers));
		}

		public void TestFirstPortOfArrival()
		{
			AssertEquals("FirstPortOfArrival", ZString.Empty, Header.FirstPortOfArrival);
			CusSeaManArrivalPort arrival = Header.Arrivals.AddNew();
			arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("FirstPortOfArrival", ZString.Empty, Header.FirstPortOfArrival);
			arrival.BA_IsFirstArrival = true;
			AssertEquals("FirstPortOfArrival", "AUSYD", Header.FirstPortOfArrival);
		}

		public void TestMessages()
		{
			AssertNotNull("Messages", Header.Messages);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals("StatusNeedsRecalculation", false, Header.StatusNeedsRecalculation);
			Header.Messages.AddNew().EM_Status = "ABC";
			AssertEquals("StatusNeedsRecalculation", true, Header.StatusNeedsRecalculation);
		}

		public void TestLoadFromSendersReference()
		{
			Header.BT_SendersMessageReference = "123";
			AssertEquals("Header", Header, CusSeaManTranHead.LoadFromSendersReference(Factory, "123"));
		}

		public void TestDischargePortToShowSetsViewDischargePort()
		{
			Header.DischargePortToShow = "AUSYD";
			AssertEquals("OceanBillsView.DischargePort", "AUSYD", Header.OceanBillsView.DischargePort);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			if (GetType() == typeof(CusSeaManTranHeadTest))
			{
				Assert($"Covered by {nameof(FetchStrategies.Testing.CusSeaManTranHeadFetchStrategyTest)}.", true);
				return;
			}

			base.TestCalcPropertiesWithDbHitsUseFetchHints();
		}

		#region Implementation

		CusSeaManTranHead header;
		public CusSeaManTranHead Header
		{
			get { return header ?? (header = Factory.New<CusSeaManTranHead>()); }
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusSeaManTranHead header = (CusSeaManTranHead)base.GetNewBusinessObjectForDeleteTest(factory);
			header.Arrivals.AddNew();
			header.OceanBills.AddNew();
			//Header.SlotCharterers.AddNew();
			return header;
		}

		#endregion
	}
}
