using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFBondActivityCodeListTest : TestCase
	{
		public void TestGetEquivalentDeclarationActiveCodeList()
		{
			var declarationActiveCodeList = ISFBondActivityCodeList.GetEquivalentDeclarationActiveCodeList();
			AssertEquals(6, declarationActiveCodeList.Count);
			AssertEquals(ActivityCodeList.Codes._1, declarationActiveCodeList[0]);
			AssertEquals(ActivityCodeList.Codes._1a1, declarationActiveCodeList[1]);
			AssertEquals(ActivityCodeList.Codes._2, declarationActiveCodeList[2]);
			AssertEquals(ActivityCodeList.Codes._3, declarationActiveCodeList[3]);
			AssertEquals(ActivityCodeList.Codes._3a3, declarationActiveCodeList[4]);
			AssertEquals(ActivityCodeList.Codes._4, declarationActiveCodeList[5]);
		}

		public void TestGetCode()
		{
			AssertEquals("", ISFBondActivityCodeList.GetCode("ZSD"));
			AssertEquals(ISFBondActivityCodeList.Codes.ImporterOrBroker, ISFBondActivityCodeList.GetCode(ActivityCodeList.Codes._1));
			AssertEquals(ISFBondActivityCodeList.Codes.ImporterOrBroker, ISFBondActivityCodeList.GetCode(ActivityCodeList.Codes._1a1));
			AssertEquals(ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise, ISFBondActivityCodeList.GetCode(ActivityCodeList.Codes._2));
			AssertEquals(ISFBondActivityCodeList.Codes.InternationalCarrier, ISFBondActivityCodeList.GetCode(ActivityCodeList.Codes._3));
			AssertEquals(ISFBondActivityCodeList.Codes.InternationalCarrier, ISFBondActivityCodeList.GetCode(ActivityCodeList.Codes._3a3));
			AssertEquals(ISFBondActivityCodeList.Codes.ForeignTradeZoneOperator, ISFBondActivityCodeList.GetCode(ActivityCodeList.Codes._4));
			AssertEquals(ISFBondActivityCodeList.Codes.ISFBond16, ISFBondActivityCodeList.GetCode(ActivityCodeList.Codes._16));
		}

		public void TestICodeDescriptionPairListProviderMembers()
		{
			var list = new ISFBondActivityCodeList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
