using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business.MessageBuilders
{
	public interface ISupportingDocumentMessageDataProvider
	{
		ZString CaseNumber { get; }
		ZString ContextReference { get; }
		DataContextType ContextType { get; }
		IeDoc Document { get; }
		ZString LocalReferenceNumber { get; }
		ZString DocumentType { get; }
		EnterpriseBusinessObject BusinessObject { get; }
		ZString CountryCode { get; }

		SupportingDocUniversalEventBuilder GetSupportingDocUniversalEventBuilder();

		ZBool ShouldSend { get; set; }

		CusEntryHeader Header { get; }

		BusinessObjectFactory Factory { get; }

		ForwardingShipment Shipment { get; }
	}
}
