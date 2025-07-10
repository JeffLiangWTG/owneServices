using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForETradeEDIMessage()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRE;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T1E;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.TRQ;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.TRL;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.TRI;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.TRS;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.TRB;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.TCD;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T1D;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T2D;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T1S;
			AssertEquals(typeof(ETradeEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForTRManifestMessage()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRO;
			AssertEquals(typeof(TRManifestMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T1O;
			AssertEquals(typeof(TRManifestMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T2O;
			AssertEquals(typeof(TRManifestMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.TRM;
			AssertEquals(typeof(TRManifestMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForDefault()
		{
			message.EM_MessageType = "ZZZ";
			AssertEquals(typeof(TRBaseMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForSPTS()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TSP;
			AssertEquals(typeof(SPTSMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T1P;
			AssertEquals(typeof(SPTSMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForNCTS()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TRN;
			AssertEquals(typeof(NCTSMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T1N;
			AssertEquals(typeof(NCTSMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T2N;
			AssertEquals(typeof(NCTSMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForPhase5NCTS()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.TR5;
			AssertEquals(typeof(NCTSMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.T15;
			AssertEquals(typeof(NCTSMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForImportExport()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.DKO;
			AssertEquals(typeof(TRImportExportMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.DK1;
			AssertEquals(typeof(TRImportExportMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.DT1;
			AssertEquals(typeof(TRImportExportMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.DT2;
			AssertEquals(typeof(TRImportExportMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.DT3;
			AssertEquals(typeof(TRImportExportMessage), typeDecider.GetTypeForLoad(row, Factory));
			message.EM_MessageType = TRMessageTypes.Codes.DTE;
			AssertEquals(typeof(TRImportExportMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForExporUnion()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = TRMessageTypes.Codes.EUT;
			AssertEquals(typeof(ExportUnionMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new EDIMessageTypeDecider();
			message = Factory.New<TRBaseMessage>();
			row = ((INeedRow)message).Row;
		}

		EDIMessageTypeDecider typeDecider;
		EDIMessage message;
		DataRow row;
	}
}
