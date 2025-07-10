using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationWithConsolShipmentDetailExporter : IDeclarationWithConsolShipmentDetailExporter
	{
		public XmlInterchange ExportDeclarationWithRelatedConsolShipmentDetails(BaseJobDeclaration declaration, EventsWithSourceType eventsWithSourceType, ProcessTaskNotification action)
		{
			var declarationAdapter = DeclarationValueObjectDataAdapter.New(declaration.CountryCode, eventsWithSourceType);
			return ExportDeclarationWithRelatedConsolShipmentDetails(declaration, declarationAdapter, action);
		}

		public XmlInterchange ExportDeclarationWithRelatedConsolShipmentDetails(BaseJobDeclaration declaration, DeclarationValueObjectDataAdapter declarationAdapter, ProcessTaskNotification action)
		{
			var exportContext = new ValueObjectExportContext(new NotificationBuffer());
			if (action != null)
			{
				exportContext.SimplifiedXML = WorkflowTriggerActionTypeConstants.IsSimplifiedXml(action.PQ_TriggerType);
			}
			var consolValue = declarationAdapter.ExportConsolsValueObject(declaration, exportContext);
			var interchange = XmlInterchange.NewPopulatedInterchange(declaration.Factory, exportContext);
			InitializeInterchangePayloadFromConsolValueObject(interchange.Payload, consolValue);
			return interchange;
		}

		void InitializeInterchangePayloadFromConsolValueObject(Payload paylod, Consols consol)
		{
			paylod.Data = consol;
			paylod.ValueObjectSerializer = new XmlValueObjectSerializer(typeof(Consols));
		}

		#region IDeclarationWithConsolShipmentDetailExporter Members

		IValueObject IDeclarationWithConsolShipmentDetailExporter.ExportDeclarationWithRelatedConsolShipmentDetails(Integration.Customs.IBaseJobDeclaration declaration, EventsWithSourceType eventsWithSourceType, ProcessTaskNotification action)
		{
			return ExportDeclarationWithRelatedConsolShipmentDetails((BaseJobDeclaration)declaration, eventsWithSourceType, action);
		}

		#endregion
	}
}
