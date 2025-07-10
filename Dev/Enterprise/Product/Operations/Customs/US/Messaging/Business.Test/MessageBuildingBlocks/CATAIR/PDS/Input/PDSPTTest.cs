using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PDSPTTest : TestCaseWithFactory
	{
		public void TestBillNumberJustification()
		{
			PDSPT pt = new PDSPT();
			pt.PayersUnitNumber = "000778";
			pt.PaymentType = "01";
			pt.StatementFiler = "385";
			pt.StatementNumber = "2710288B73";
			pt.PaymentAmount = 192.30m;
			pt.NegationCode = "Y";
			pt.NegationDate = new ZDate(2015, 01, 01);
			AssertContains("PT00077801385 2710288B730000019230Y010115", pt.Serialise());
		}
	}
}
