#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocJobComInvoiceGroupHeaderCollection : DocBaseJobComInvoiceGroupHeaderCollection
	{
		public DocJobComInvoiceGroupHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobComInvoiceGroupHeaderCollection(Customs.Business.IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobComInvoiceGroupHeader this[int index] => (DocJobComInvoiceGroupHeader)Elements[index];
	}
}
#endif
