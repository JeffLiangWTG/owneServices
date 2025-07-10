using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class BillOfLadingVisualizableDocumentSupporter : AgencyShipmentVisualizableDocumentSupporter<BillOfLading>
	{
		public BillOfLadingVisualizableDocumentSupporter(BillOfLading parent) : base(parent)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.None;

		public override Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem)
		{
			return (object)null;
		}

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document)
		{
			return null;
		}

		public override IMessageLogCreator GetMessageLogCreator(IDocument document)
		{
			return null;
		}

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			return null;
		}

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters)
		{
			switch (documentName)
			{
				case BillOfLadingDocumentNames.BillOfLading:
					return BillOfLadingDocumentDataStoreNames.AgencyBillOfLading;

				default:
					return documentName;
			}
		}
	}
}
