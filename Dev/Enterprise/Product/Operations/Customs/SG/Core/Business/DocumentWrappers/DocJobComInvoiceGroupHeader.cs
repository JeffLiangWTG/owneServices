using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocJobComInvoiceGroupHeader : DocBaseJobComInvoiceGroupHeader
	{
		DocJobComInvoiceGroupHeader(JobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceGroupHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceGroupHeader New(JobComInvoiceGroupHeader jobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
		{
			if (jobComInvoiceGroupHeader == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceGroupHeader(jobComInvoiceGroupHeader, factoryToWrap);
			}
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}
	}
}
