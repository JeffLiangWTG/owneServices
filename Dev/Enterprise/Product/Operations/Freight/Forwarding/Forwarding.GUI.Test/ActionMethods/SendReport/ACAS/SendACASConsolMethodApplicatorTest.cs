using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SendACASConsolMethodApplicator))]
	public class SendACASConsolMethodApplicatorTest : DocDataObjectSendingMessageMethodApplicatorTest
	{
		public void TestSkip()
		{
			using (Factory.AddDisposableService())
			{
				var consolWithoutErrors = CreateConsolWithoutErrors("12345678916", "C00000001", "AUSYD", "USLAX", "AdvancedCargoReportUS", Core.Constants.CountryCodes.UnitedStates);
				var consolWithFilterErrors = Factory.New<ForwardingConsol>();
				PopulateConsol(consolWithFilterErrors, "12345678917", "AUSYD", "GBLON", "C00000002");
				var consolWithSendErrors = Factory.New<ForwardingConsol>();
				PopulateConsol(consolWithSendErrors, "", "AUSYD", "USLAX", "C00000003");
				var shipmentWithoutErrors = CreateShipmentWithoutErrors("12345678918", "S00000001", "C00001001", "AUSYD", "USLAX");

				Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Skip;

				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);

				var expectedLog =
					"INFO: [HL C00000001] processed successfully.\n" +
					"WARNING: [HL C00000002] The consol is not arriving in US.\n" +
					"WARNING: [HL C00000003] " +
					"Message Error - WayBillNumber: MAWB must be entered to use Advance Air Cargo Report.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithFilterErrors, consolWithSendErrors }, expectedLog);

				var expectedReSendLog =
					"WARNING: [HL C00000001] A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedReSendLog);

				var expectedNotValidLog =
					"WARNING:  Can not load Consol correctly.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors }, expectedNotValidLog);
			}
		}

		public void TestAbort()
		{
			using (Factory.AddDisposableService())
			{
				var consolWithoutErrors = CreateConsolWithoutErrors("12345678916", "C00000001", "AUSYD", "USLAX", "AdvancedCargoReportUS", Core.Constants.CountryCodes.UnitedStates);
				var consolWithFilterErrors = Factory.New<ForwardingConsol>();
				PopulateConsol(consolWithFilterErrors, "12345678917", "AUSYD", "GBLON", "C00000002");
				var consolWithSendErrors = Factory.New<ForwardingConsol>();
				PopulateConsol(consolWithSendErrors, "", "AUSYD", "USLAX", "C00000003");
				var shipmentWithoutErrors = CreateShipmentWithoutErrors("12345678918", "S00000001", "C00001001", "AUSYD", "USLAX");

				Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Abort;

				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);

				var expectedLog =
					"INFO: [HL C00000001] processed successfully.\n" +
					"ERROR: [HL C00000002] The consol is not arriving in US.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors, consolWithFilterErrors, consolWithSendErrors }, expectedLog);

				var expectedReSendLog =
					"ERROR: [HL C00000001] A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedReSendLog);

				var expectedNotValidLog =
					"ERROR:  Can not load Consol correctly.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors }, expectedNotValidLog);
			}
		}

		#region Implementation

		protected override string NoSelectedError => "ERROR: No consols selected.";

		protected override string MessageSenderContext => "ACASHouseChecklistMessageSender";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SendACASConsolMethodApplicator(Settings, Factory);
		}

		protected override DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider()
		{
			return new ACASConsolReportSendingProvider(Factory);
		}

		#endregion
	}
}
