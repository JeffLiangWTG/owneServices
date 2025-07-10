using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Document
{
	public sealed class DtbBookingConfirmationVisualizableDocumentSupporter : TransportBookingsVisualizableDocumentSupporter<DtbBookingConfirmation>
	{
		public DtbBookingConfirmationVisualizableDocumentSupporter(DtbBookingConfirmation confirmation)
			: base(confirmation)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => null;

		public override Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => null;

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters) => documentName;

		public override string GetMessageBroker() => EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

		public override bool ShouldUseDraftWatermark(IDocument document) => false;
	}
}
