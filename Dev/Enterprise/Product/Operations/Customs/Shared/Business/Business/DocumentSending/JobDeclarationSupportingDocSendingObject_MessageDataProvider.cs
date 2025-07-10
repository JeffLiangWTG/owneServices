using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public partial class JobDeclarationSupportingDocSendingObject : ISupportingDocumentMessageDataProvider
	{
		public ForwardingShipment Shipment => ((BaseJobDeclaration)SupportingDocObject).Shipment;

		EnterpriseBusinessObject ISupportingDocumentMessageDataProvider.BusinessObject => (BaseJobDeclaration)SupportingDocObject;

		ZString ISupportingDocumentMessageDataProvider.CaseNumber => CaseNumber;

		ZString ISupportingDocumentMessageDataProvider.ContextReference => ((BaseJobDeclaration)SupportingDocObject).JobNumber;

		DataContextType ISupportingDocumentMessageDataProvider.ContextType => DataContextType.CustomsDeclaration;

		IeDoc ISupportingDocumentMessageDataProvider.Document => Document;

		ZString ISupportingDocumentMessageDataProvider.LocalReferenceNumber => LocalReferenceNumber;

		ZString ISupportingDocumentMessageDataProvider.DocumentType => DocumentType;

		ZString ISupportingDocumentMessageDataProvider.CountryCode => SupportingDocObject.CountryCode;
	}
}
