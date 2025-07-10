using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SendACASShipmentMethodApplicator))]
	public class SendACASShipmentMethodApplicatorTest : DocDataObjectSendingMessageMethodApplicatorTest
	{
		public void TestSkip()
		{
			using (Factory.AddDisposableService())
			{
				var shipmentWithoutErrors = CreateShipmentWithoutErrors("12345678916", "S00000001", "C00001001", "AUSYD", "USLAX");
				var newShipmentWithoutErrors = CreateShipmentWithoutErrors("12345678912", "S00000002", "C00001002", "AUSYD", "USLAX");
				var shipmentWithFilterErrors = Factory.New<ForwardingShipment>();
				PopulateShipment(shipmentWithFilterErrors, "12345678916", "AUSYD", "GBLON", "S00000003");
				var shipmentWithSendErrors = Factory.New<ForwardingShipment>();
				PopulateShipment(shipmentWithSendErrors, "", "AUSYD", "USLAX", "S00000004");
				var consolWithoutErrors = CreateConsolWithoutErrors("12345678917", "C00000001", "AUSYD", "USLAX", "AdvancedCargoReportUS", Core.Constants.CountryCodes.UnitedStates);

				Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Skip;

				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);

				var expectedLog =
					"INFO: [HL S00000001] processed successfully.\n" +
					"WARNING: [HL S00000003] The shipment is not arriving in US.\n" +
					"WARNING: [HL S00000004] " +
					"Message Error - HAWB: Either HAWB Number or Consol Number is required.\n" +
					"Message Error - ConsolNumber: Either HAWB Number or Consol Number is required.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors, shipmentWithFilterErrors, shipmentWithSendErrors }, expectedLog);

				var expectedReSendLog =
					"WARNING: [HL S00000001] A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors }, expectedReSendLog);

				var expectedNotValidLog =
					"WARNING:  Can not load Shipment correctly.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedNotValidLog);

				var expectedLogWithWarning =
					"INFO: [HL S00000002] processed successfully.\n" +
					"WARNING: [HL S00000002] Warning - MAWB: Invalid check digit. The last digit should be '6'";
				ApplyApplicator(new BusinessObject[] { newShipmentWithoutErrors }, expectedLogWithWarning);
			}
		}

		public void TestAbort()
		{
			using (Factory.AddDisposableService())
			{
				var shipmentWithoutErrors = CreateShipmentWithoutErrors("12345678916", "S00000001", "C00001001", "AUSYD", "USLAX");
				var newShipmentWithoutErrors = CreateShipmentWithoutErrors("12345678912", "S00000002", "C00001002", "AUSYD", "USLAX");
				var shipmentWithFilterErrors = Factory.New<ForwardingShipment>();
				PopulateShipment(shipmentWithFilterErrors, "12345678916", "AUSYD", "GBLON", "S00000003");
				var shipmentWithSendErrors = Factory.New<ForwardingShipment>();
				PopulateShipment(shipmentWithSendErrors, "081001", "AUSYD", "USLAX", "S00000004");
				var consolWithoutErrors = CreateConsolWithoutErrors("12345678917", "C00000001", "AUSYD", "USLAX", "AdvancedCargoReportUS", Core.Constants.CountryCodes.UnitedStates);

				Settings.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Abort;

				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ACASOriginatorCode, "CCC", Core.Constants.CountryCodes.UnitedStates);

				var expectedLog =
					"INFO: [HL S00000001] processed successfully.\n" +
					"ERROR: [HL S00000003] The shipment is not arriving in US.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors, shipmentWithFilterErrors, shipmentWithSendErrors }, expectedLog);

				var expectedReSendLog =
					"ERROR: [HL S00000001] A message has previously been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
				ApplyApplicator(new BusinessObject[] { shipmentWithoutErrors }, expectedReSendLog);

				var expectedNotValidLog =
					"ERROR:  Can not load Shipment correctly.";
				ApplyApplicator(new BusinessObject[] { consolWithoutErrors }, expectedNotValidLog);

				var expectedLogWithWarning =
					"INFO: [HL S00000002] processed successfully.\n" +
					"WARNING: [HL S00000002] Warning - MAWB: Invalid check digit. The last digit should be '6'\n" +
					"ERROR: [HL S00000003] The shipment is not arriving in US.";
				ApplyApplicator(new BusinessObject[] { newShipmentWithoutErrors, shipmentWithFilterErrors }, expectedLogWithWarning);
			}
		}

		#region Implementation

		protected override string NoSelectedError => "ERROR: No shipments selected.";

		protected override string MessageSenderContext => "AirCargoAdvanceScreeningMessageSender";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SendACASShipmentMethodApplicator(Settings, Factory);
		}

		protected IStmMenuItem MenuItem => menuItem ?? (menuItem = Factory.Load<VisualizerMenuItem>(ShipmentSystemFormMenuItems.DocumentMenuACASShipmentReport));
		IStmMenuItem menuItem;

		protected override DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider()
		{
			return new ACASShipmentReportSendingProvider(Factory);
		}

		#endregion
	}
}
