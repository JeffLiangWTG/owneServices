#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
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

		#region Overrides

		protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		#endregion
	}
}
#endif
