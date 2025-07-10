using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public sealed class SGEDIMessageInternalTest : TestCaseWithFactory
	{
		public void TestTradeNetMessage()
		{
			var d05bMessage = Factory.New<SGEDIMessage>();
			d05bMessage.EM_MessageText = "UNH+1+CUSPMT:0:1:RT:040+OUTPMT'EQD+CN+111:1+913:::FCL13'EQD+CN+222:2+517:::LCL77'UNT+2+1'";
			var d05bTradeNetMessage = d05bMessage.TradeNetMessage;
			AssertEquals("Enterprise.Edifact.D05B.dll", d05bTradeNetMessage.GetType().Module.Name);
			var d09bMessage = Factory.New<SGEDIMessage>();
			d09bMessage.EM_MessageText = "UNH+1+CUSPMT:0:1:RT:041+OUTPMT'EQD+CN+111:1+913:::FCL13'EQD+CN+222:2+517:::LCL77'UNT+2+1'";
			var d09bTradeNetMessage = d09bMessage.TradeNetMessage;
			AssertEquals("Enterprise.Edifact.D09B.dll", d09bTradeNetMessage.GetType().Module.Name);
		}
	}
}
