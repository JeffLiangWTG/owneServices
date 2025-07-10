using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BrokerLiquidationTypeCodeDescriptionPairListTest : TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			LiquidationTypeCodeList list = new LiquidationTypeCodeList();
			ReadOnlyCodeDescriptionPairList iList = ((DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider)list).GetCodeDescriptionPairList();

			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code01));
			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code02));
			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code03));
			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code04));
			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code05));
			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code06));
			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code07));
			AssertEquals(true, iList.ContainsCode(LiquidationTypeCodeList.Codes.Code08));
		}
	}
}
