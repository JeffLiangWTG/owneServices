using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGCusClassPartPivotAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsUQs()
		{
			CusClassPartPivot cusClassPartPivot = Factory.New<CusClassPartPivot>();
			AutoSGCusClassPartPivotAddInfo addinfo = new SGCusClassPartPivotAddInfo(cusClassPartPivot.CI_AddInfoInfo);
			SGCusClassPartPivotAddInfoLookups lookups = new SGCusClassPartPivotAddInfoLookups(addinfo);
			Assert(lookups.CustomsUQs is UnitOfQuantityCodeList);
		}
	}
}
