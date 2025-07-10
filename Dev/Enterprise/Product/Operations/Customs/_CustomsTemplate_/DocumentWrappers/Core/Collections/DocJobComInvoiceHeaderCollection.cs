#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocJobComInvoiceHeaderCollection : DocBaseJobComInvoiceHeaderCollection
	{
		public DocJobComInvoiceHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceHeader this[int index] => (DocJobComInvoiceHeader)Elements[index];
	}
}
#endif
