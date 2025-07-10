using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartLocation))]
	sealed class OrgPartLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestStockTakeCountValidation()
		{
			OrgPartLocation bO = (OrgPartLocation)GetNewBusinessObject();
			AssertEquals(new ZDecimal(1), bO.OR_StockTakeCount);
			AssertEquals(false, bO.OR_StockTakeCountInfo.HasErrors());
			bO.OR_StockTakeCount = 0;
			AssertEquals(false, bO.OR_StockTakeCountInfo.HasErrors());
			bO.OR_StockTakeCount = -1;
			AssertEquals(true, bO.OR_StockTakeCountInfo.HasErrors());
		}
	}
}
