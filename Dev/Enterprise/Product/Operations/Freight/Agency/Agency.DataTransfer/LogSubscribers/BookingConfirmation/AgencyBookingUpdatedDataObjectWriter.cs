using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.DataTransfer
{
	class AgencyBookingUpdatedDataObjectWriter : AgencyBookingDataObjectWriter
	{
		public AgencyBookingUpdatedDataObjectWriter(IDataWritingManager manager, AgencyBookingContainer container = null) : base(manager, container)
		{
		}

		protected override void PopulateShipment(AgencyBooking sourceBO, Shipment dataObject)
		{
			base.PopulateShipment(sourceBO, dataObject);

			var purposeList = new CodeDescriptionPairList();
			purposeList.AddPair(Events.MessageAcceptedCode, Events.MessageAccepted.Description);

			dataObject.DataContext.SetDocumentaryOverride(DocumentNameBookingConfirmation, Events.MessageAcceptedCode, purposeList, ZBool.False, 1, 1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded document name.")]
		const string DocumentNameBookingConfirmation = "Booking Confirmation";
	}
}
