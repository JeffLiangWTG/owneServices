using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business.Testing
{
	class MessageStatusCalculatorHelperTest : TestCaseWithFactory
	{
		public void TestCalculateMessageStatusAndMode()
		{
			AssertEquals((CustomsStatusList.Codes.TRS, TRMessageTypes.Codes.TRQ), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Accepted, true));
			AssertEquals((CustomsStatusList.Codes.TRR, TRMessageTypes.Codes.TRE), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Error, true));
			AssertEquals((CustomsStatusList.Codes.QRS, TRMessageTypes.Codes.TRI), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRQ, TRMessageStatusCodeList.Codes.Accepted, true));
			AssertEquals((CustomsStatusList.Codes.QRR, TRMessageTypes.Codes.TRQ), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRQ, TRMessageStatusCodeList.Codes.Error, true));
			AssertEquals((CustomsStatusList.Codes.QIS, TRMessageTypes.Codes.TRL), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Accepted, true));
			AssertEquals((CustomsStatusList.Codes.QIR, TRMessageTypes.Codes.TRI), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Error, true));
			AssertEquals((CustomsStatusList.Codes.QLS, TRMessageTypes.Codes.TRB), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Accepted, true));
			AssertEquals((CustomsStatusList.Codes.QLR, TRMessageTypes.Codes.TRL), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Error, true));
			AssertEquals((CustomsStatusList.Codes.QBS, TRMessageTypes.Codes.TRD), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRB, TRMessageStatusCodeList.Codes.Accepted, true));
			AssertEquals((CustomsStatusList.Codes.QBR, TRMessageTypes.Codes.TRB), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRB, TRMessageStatusCodeList.Codes.Error, true));
			AssertEquals((CustomsStatusList.Codes.DLS, TRMessageTypes.Codes.TCD), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRD, TRMessageStatusCodeList.Codes.Accepted, true));
			AssertEquals((CustomsStatusList.Codes.DLR, TRMessageTypes.Codes.TRD), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRD, TRMessageStatusCodeList.Codes.Error, true));
			AssertEquals((CustomsStatusList.Codes.CDS, TRMessageTypes.Codes.CPL), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TCD, TRMessageStatusCodeList.Codes.Accepted, true));
			AssertEquals((CustomsStatusList.Codes.CDR, TRMessageTypes.Codes.TCD), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TCD, TRMessageStatusCodeList.Codes.Error, true));

			AssertEquals((CustomsStatusList.Codes.TRS, TRMessageTypes.Codes.TRS), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Accepted, false));
			AssertEquals((CustomsStatusList.Codes.TRR, TRMessageTypes.Codes.TRE), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Error, false));
			AssertEquals((CustomsStatusList.Codes.RNS, TRMessageTypes.Codes.TRI), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRS, TRMessageStatusCodeList.Codes.Accepted, false));
			AssertEquals((CustomsStatusList.Codes.RNR, TRMessageTypes.Codes.TRS), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRS, TRMessageStatusCodeList.Codes.Error, false));
			AssertEquals((CustomsStatusList.Codes.QIS, TRMessageTypes.Codes.TRL), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Accepted, false));
			AssertEquals((CustomsStatusList.Codes.QIR, TRMessageTypes.Codes.TRI), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Error, false));
			AssertEquals((CustomsStatusList.Codes.QLS, TRMessageTypes.Codes.CPL), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Accepted, false));
			AssertEquals((CustomsStatusList.Codes.QLR, TRMessageTypes.Codes.TRL), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Error, false));

			AssertEquals(((string)null, (string)null), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(string.Empty, string.Empty, true));
			AssertEquals(((string)null, (string)null), MessageStatusCalculatorHelper.CalculateMessageStatusAndMode(string.Empty, string.Empty, false));
		}
	}
}
