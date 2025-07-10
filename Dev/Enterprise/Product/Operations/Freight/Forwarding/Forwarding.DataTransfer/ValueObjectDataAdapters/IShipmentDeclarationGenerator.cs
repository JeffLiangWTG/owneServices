using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public interface IShipmentDeclarationGenerator
	{
		void CreateDeclarationForShipment(ForwardingShipment shipment, Xsd.InvoiceHeaderCollection invoicesValue, IValueObjectImportContext context);
	}
}
