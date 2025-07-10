using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeader))]
	sealed class CusSeaManOBLHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Ocean Bill", Header.HumanReadableName);
			Header.BO_OceanBill = "OBL100";
			AssertEquals("Ocean Bill OBL100", Header.HumanReadableName);
		}

		public void TestIsAutoLogged()
		{
			AssertEquals(0, Header.Logs.GetAllLogs().Count);
			Factory.Save();
			AssertEquals(1, Header.Logs.GetAllLogs().Count);
		}

		public void TestDetails()
		{
			AssertNotNull("Header.Details should be created", Header.Details);
			AssertEquals("Detail should be an editable child", true, Header.IsRegisteredEditableChildObject(Header.Details));
		}

		public void TestTransportHeader()
		{
			TranHead.OceanBills.AddNew();
			AssertEquals("TranHeader", TranHead, TranHead.OceanBills[0].TransportHeader);
		}

		public void TestMessages()
		{
			AssertNotNull("Messages", Header.Messages);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals("StatusNeedsRecalculation", false, Header.StatusNeedsRecalculation);
			Header.Messages.AddNew().EM_MessageText = "123";
			AssertEquals("StatusNeedsRecalculation", true, Header.StatusNeedsRecalculation);
		}

		public void TestLoadFromSendersReference()
		{
			Header.BO_SendersMessageReference = "123";
			AssertEquals("Header", Header, CusSeaManOBLHeader.LoadFromSendersReference(Factory, "123"));
		}

		[ExpectNoExceptions()]
		public void TestSupportsClone()
		{
			Header.Clone();
		}

		#region Implementation

		CusSeaManTranHead tranHead;
		public CusSeaManTranHead TranHead
		{
			get { return tranHead ?? (tranHead = Factory.New<CusSeaManTranHead>()); }
		}

		CusSeaManOBLHeader header;
		public CusSeaManOBLHeader Header
		{
			get { return header ?? (header = TranHead.OceanBills.AddNew()); }
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusSeaManOBLHeader header = (CusSeaManOBLHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			header.Details.AddNew();
			return header;
		}

		#endregion
	}
}
