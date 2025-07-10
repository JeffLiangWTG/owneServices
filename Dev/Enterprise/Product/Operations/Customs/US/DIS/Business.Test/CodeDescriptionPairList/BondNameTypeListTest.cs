using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class BondNameTypeListTest : TestCaseWithFactory
	{
		public void TestGetBondNameType()
		{
			AssertEquals(BondNameType.Single, BondNameTypeList.GetBondNameType(BondNameTypeList.Codes.SingleBond));
			AssertEquals(BondNameType.ISFBond, BondNameTypeList.GetBondNameType(BondNameTypeList.Codes.ISFBond));
			AssertEquals(BondNameType.Other, BondNameTypeList.GetBondNameType(BondNameTypeList.Codes.Other));
			AssertEquals(BondNameType.None, BondNameTypeList.GetBondNameType(""));
		}

		public void TestGetCodeFrom()
		{
			AssertEquals(BondNameTypeList.Codes.SingleBond, BondNameTypeList.GetCodeFrom(BondNameType.Single));
			AssertEquals(BondNameTypeList.Codes.ISFBond, BondNameTypeList.GetCodeFrom(BondNameType.ISFBond));
			AssertEquals(BondNameTypeList.Codes.Other, BondNameTypeList.GetCodeFrom(BondNameType.Other));
			AssertEquals("", BondNameTypeList.GetCodeFrom(BondNameType.None));
		}
	}
}
