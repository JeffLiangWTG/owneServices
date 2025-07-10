using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoiceXmlDataImporter : XmlDataImporter
	{
		public InvoiceXmlDataImporter(StandAloneInvoiceValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		public InvoiceXmlDataImporter(BusinessObjectFactoryProvider factoryProvider, StandAloneInvoiceValueObjectDataAdapter adapter)
			: base(factoryProvider, adapter)
		{
		}

		protected new StandAloneInvoiceValueObjectDataAdapter Adapter
		{
			get { return (StandAloneInvoiceValueObjectDataAdapter)base.Adapter; }
		}

		protected override XmlValueObjectSerializer GetSerializer()
		{
			return Serializer;
		}

		InvoiceXmlValueObjectSerializer Serializer
		{
			get { return (serializer) ?? (serializer = new InvoiceXmlValueObjectSerializer()); }
		}
		InvoiceXmlValueObjectSerializer serializer;
	}
}
