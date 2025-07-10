using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLDetail))]
	sealed class CusSeaManOBLDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			AssertEquals("Header", Header, Header.Details.AddNew().Header);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Container ()", Detail.HumanReadableName);

			Detail.BD_ContainerNumber = "AAAA1111113";
			AssertEquals("Container () AAAA1111113", Detail.HumanReadableName);

			Header.BO_OceanBill = "foosh";
			AssertEquals("Container (foosh) AAAA1111113", Detail.HumanReadableName);
		}

		public void TestIsAutoLogged()
		{
			AssertEquals(0, Detail.Logs.GetAllLogs().Count);
			Factory.Save();
			AssertEquals(1, Detail.Logs.GetAllLogs().Count);
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

		CusSeaManOBLDetail detail;
		public CusSeaManOBLDetail Detail
		{
			get { return detail ?? (detail = Header.Details.AddNew()); }
		}

		#endregion
	}
}
