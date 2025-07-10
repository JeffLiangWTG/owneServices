using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class TNPUPDTest : TestCaseWithFactory
	{
		public void TestUpdate()
		{
			DataProvider.AdditionalMessageInfo.UpdateIndicator = SGConstants.UpdateIndicators.AME;
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "Updating Flight details";
			DataProvider.AdditionalMessageInfo.ReasonForExtendingTemporaryImportPeriod = "TEST Extension Reason";
			DataProvider.PermitNoToUpdateOrCancel = "TT0E103190M";
			DataProvider.ReplacementPermitNumber = "TT2A241592P";
			DataProvider.NumberOfRequestsForUpdate = 1;
			Message.Build(TradenetDeclaration);
			var update = TradenetDeclaration.InboundMessage.TranshipmentMovementUpdate.Update;
			AssertEquals("UpdateIndicatorCode", "AME", update.UpdateIndicatorCode);
			AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
			AssertEquals("UpdatePermitNumber", "TT0E103190M", update.UpdatePermitNumber);
			AssertEquals("ReplacementPermitNumber", "TT2A241592P", update.ReplacementPermitNumber);
			AssertArrayEqualsByElements("AmendmentReason", new[] { "UPDATING FLIGHT DETAILS" }, update.Amendment.AmendmentReason);
			AssertNull("ExtensionReason should be not used in TNPUPD", update.Amendment.ExtensionReason);
		}

		public void TestMessageTypeAndSubType()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.TNPUPD, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Amendment, Message.MessageSubType);
		}

		TNPUPD Message => message ?? (message = new TNPUPD(DataProvider));
		TNPUPD message;
		ITNPUPDTestClass DataProvider => dataProvider ?? (dataProvider = new ITNPUPDTestClass());
		ITNPUPDTestClass dataProvider;
		TradenetDeclaration TradenetDeclaration => tradenetDeclaration ?? (tradenetDeclaration = new TradenetDeclaration());
		TradenetDeclaration tradenetDeclaration;
	}
}
