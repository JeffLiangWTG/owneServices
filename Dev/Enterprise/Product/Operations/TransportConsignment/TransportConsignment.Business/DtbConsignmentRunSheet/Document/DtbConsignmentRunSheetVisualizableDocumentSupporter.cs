using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	public sealed class DtbConsignmentRunSheetVisualizableDocumentSupporter : ConsignmentVisualizableDocumentSupporter<DtbConsignmentRunSheet>
	{
		public DtbConsignmentRunSheetVisualizableDocumentSupporter(DtbConsignmentRunSheet dtbRunsheet)
			: base(dtbRunsheet)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.DtbConsignmentRunSheetCustomiseForms;

		public override Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem)
		{
			var contextTypes = GetContextTypes(menuItem);

			if (contextTypes == null)
			{
				return (object)null;
			}

			if (contextTypes.Contains(DataContext.CMRConsignmentNote))
			{
				return GetCMRConsignmentNoteAdditionalData();
			}

			return (object)null;
		}

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters) => documentName;

		public override string GetMessageBroker() => EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

		public override bool ShouldUseDraftWatermark(IDocument document) => false;

		ZString[] GetContextTypes(IStmMenuItem menuItem)
		{
			return menuItem
						?.Documents
						.OfType<IStmMenuTemplatePivot>()
						.Select(p => p.Template.SO_DataContext)
						.ToArray();
		}

		Either<string, object> GetCMRConsignmentNoteAdditionalData()
		{
			var consignments = ConsignmentRunSheetHelper.GetConsignmentsFromRunSheet(parent);

			if (!consignments.Any())
			{
				return Res.GetString("db4d2e39-2fbe-11f0-b96e-4dceb86731c8", "Run Sheet does not contain any Consignments.");
			}

			return (object)null;
		}
	}
}
