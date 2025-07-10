#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocJobComInvoiceLineCollection : DocBaseJobComInvoiceLineCollection
	{
		public DocJobComInvoiceLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceLineCollection(Customs.Business.InvoiceLineCompleteCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public DocJobComInvoiceLineCollection(Customs.Business.BaseJobComInvoiceLineViewCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceLine this[int index] => (DocJobComInvoiceLine)Elements[index];
	}
}
#endif
