using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZAMessageTypeDeciderTestCase : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new ZAMessageTypeDecider();
			var message = Factory.New<ZAMessage>();
			var row = ((INeedRow)message).Row;
			CombineAssertions(() =>
			{
				message.EM_MessageType = "DEC";
				AssertEquals("MessageType For DEC", typeof(CUSDECEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "CAR";
				AssertEquals("MessageType For CAR", typeof(CUSCAREDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "GIO";
				AssertEquals("MessageType For GIO", typeof(GOVGIOEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "CTL";
				AssertEquals("MessageType For CTL", typeof(CONTRLEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "RES";
				AssertEquals("MessageType For RES", typeof(CUSRESEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "STA";
				AssertEquals("MessageType For STA", typeof(STATACEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "REQ";
				AssertEquals("MessageType For REQ", typeof(REQDOCEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "RSQ";
				AssertEquals("MessageType For RSQ", typeof(CUSRES_REQDOCEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "CTO";
				AssertEquals("MessageType For CTO", typeof(COSTCOEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "CAL";
				AssertEquals("MessageType For CAL", typeof(CALINFEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "EXP";
				AssertEquals("MessageType For EXP", typeof(ZAMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "XXX";
				AssertEquals("MessageType For XXX", typeof(ZAMessage), typeDecider.GetTypeForLoad(row, Factory));
				message.EM_MessageType = "";
				AssertEquals("MessageType For Empty", typeof(ZAMessage), typeDecider.GetTypeForLoad(row, Factory));
			});
		}
	}
}
