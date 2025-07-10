using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManSlotOrg))]
	public class CusSeaManSlotOrgTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			CusSeaManSlotOrg slotOrg = tranHead.SlotCharterers.AddNew();
			AssertEquals("same as parent", tranHead, slotOrg.Header);
		}

		#region TestCase

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusSeaManTranHead head = factory.New<CusSeaManTranHead>();
			return head.SlotCharterers.AddNew();
		}

		#endregion
	}
}
