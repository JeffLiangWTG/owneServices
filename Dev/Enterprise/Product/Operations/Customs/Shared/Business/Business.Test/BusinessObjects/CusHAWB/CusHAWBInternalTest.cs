using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusHAWBInternalTest : TestCaseWithFactory
	{
		public void TestDetachedCusHAWBApplicationCode()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_ApplicationCode = "XXX";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_CM = ZGuid.Empty;

			AssertEquals("XXX", hawb.CS_ApplicationCode);
		}
	}
}
