using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	public sealed class DtbConsignmentVisualizableDocumentSupporter : ConsignmentVisualizableDocumentSupporter<DtbConsignment>
	{
		public DtbConsignmentVisualizableDocumentSupporter(DtbConsignment dtbConsignment)
			: base(dtbConsignment)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.DtbConsignmentCustomiseForms;

		public override Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => new Either<string, object>((object)null);

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters) => documentName;

		public override string GetMessageBroker() => EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

		public override bool ShouldUseDraftWatermark(IDocument document) => false;
	}
}
