using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class SupportingDocSendingObject : ISupportingDocumentMessageDataProvider
	{
		CusEntryHeader ISupportingDocumentMessageDataProvider.Header => null;

		ForwardingShipment ISupportingDocumentMessageDataProvider.Shipment => null;

		EnterpriseBusinessObject ISupportingDocumentMessageDataProvider.BusinessObject => SupportingDocObject as EnterpriseBusinessObject;

		ZString ISupportingDocumentMessageDataProvider.CaseNumber => CaseNumber;

		ZString ISupportingDocumentMessageDataProvider.ContextReference => SupportingDocObject.Reference;

		DataContextType ISupportingDocumentMessageDataProvider.ContextType => DataContextType.AsycudaManifest;

		IeDoc ISupportingDocumentMessageDataProvider.Document => Document;

		ZString ISupportingDocumentMessageDataProvider.LocalReferenceNumber => ZString.Empty;

		ZString ISupportingDocumentMessageDataProvider.DocumentType => DocumentType;

		ZString ISupportingDocumentMessageDataProvider.CountryCode => SupportingDocObject.CountryCode;
	}
}
