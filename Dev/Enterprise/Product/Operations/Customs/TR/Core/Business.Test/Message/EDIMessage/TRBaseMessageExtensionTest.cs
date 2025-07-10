using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRBaseMessageExtensionTest : TestCase
	{
		public void TestNeedToSignMessage()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Non-specific type", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(EDIMessage), ""));
				AssertEquals("TRBaseMessage", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(TRBaseMessage), ""));
				AssertEquals("SPTSMessage", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(SPTSMessage), ""));

				AssertEquals("TRManifestMessage", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(TRManifestMessage), ""));
				AssertEquals("TRManifestMessage", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(TRManifestMessage), TRMessageTypes.Codes.T1O));
				AssertEquals("TRManifestMessage", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(TRManifestMessage), TRMessageTypes.Codes.T3O));
				AssertEquals("TRManifestMessage", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(TRManifestMessage), TRMessageTypes.Codes.TRM));

				AssertEquals("SPTSMessage", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(SPTSMessage), TRMessageTypes.Codes.T1P));

				AssertEquals("ETradeEDIMessage - Blank", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ETradeEDIMessage), ""));
				AssertEquals("ETradeEDIMessage - TRE", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ETradeEDIMessage), TRMessageTypes.Codes.TRE));
				AssertEquals("ETradeEDIMessage - TRS", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ETradeEDIMessage), TRMessageTypes.Codes.TRS));
				AssertEquals("ETradeEDIMessage - TRD", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ETradeEDIMessage), TRMessageTypes.Codes.TRD));
				AssertEquals("ETradeEDIMessage - TCD", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ETradeEDIMessage), TRMessageTypes.Codes.TCD));
				AssertEquals("ETradeEDIMessage - Other", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ETradeEDIMessage), TRMessageTypes.Codes.TSP));
				AssertEquals("ETradeEDIMessage - TRQ", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ETradeEDIMessage), TRMessageTypes.Codes.TRQ));

				AssertEquals("NCTSEDIMessage - Blank", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(NCTSMessage), ""));
				AssertEquals("NCTSEDIMessage - TRN", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(NCTSMessage), TRMessageTypes.Codes.TRN));
				AssertEquals("NCTSEDIMessage - T1N", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(NCTSMessage), TRMessageTypes.Codes.T1N));

				AssertEquals("Import-Export EDIMessage - DKO", false, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(TRImportExportMessage), TRMessageTypes.Codes.DKO));
				AssertEquals("Import-Export EDIMessage - DTE", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(TRImportExportMessage), TRMessageTypes.Codes.DTE));

				AssertEquals("Export Union EDIMessage - EUT", true, TRBaseMessageExtensions.IsMessageSigningRequired(typeof(ExportUnionMessage), TRMessageTypes.Codes.EUT));
			});
		}
	}
}
