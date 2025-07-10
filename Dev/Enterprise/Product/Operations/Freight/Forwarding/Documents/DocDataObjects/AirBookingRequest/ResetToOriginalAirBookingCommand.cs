using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ResetToOriginalAirBookingCommand : AirBookingCommand
	{
		bool isEnabled = true;

		public override string Id => CommandIds.ResetToOriginal;
		public override string Caption => CommandResources.Captions.ResetToOriginal;
		public override bool IsEnabled => isEnabled;
		public override bool IsVisible => true;

		protected override bool OnInvokeCommand()
		{
			var errorMessage = string.Empty;
			var check = CheckAllowSendMessage();

			if (!check)
			{
				ShowMessage(errorMessage);
				return false;
			}

			var factory = GetFactory();

			var waring = Res.GetString("aed7885b-695c-4bba-b615-e392c7246cb4", @"WARNING: Using this option without checking with the Carrier first might result in duplicate messages being processed by the Carrier.
	Resetting to Original should only be required when there is a serious messaging failure at the Carrier’s end.
	In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
	Before using this option, you should always check with the Carrier to make sure they have not already processed the message.");

			var confirmation = Res.GetString("19fbb95a-02c6-4cc4-9174-3eb2674ca455", "I have confirmed with the Carrier that they did not process the Original message already sent.");

			if (ShowConfirmation(waring, confirmation)
				&& documentInfo.DocumentData is IStmALogParent logParent)
			{
				logParent.CreateStatusUpdateLog(Document.Name);

				var airBookingRequest = GetAirBookingRequest();

				FlightBookingStatusManager.UpdateBookingStatus(factory, airBookingRequest, Core.Constants.TransportStatus.Planned);

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);

				Notify(new ResetToOriginalEvent(documentInfo.Document));
				isEnabled = false;

				return true;
			}

			return false;
		}
	}
}
