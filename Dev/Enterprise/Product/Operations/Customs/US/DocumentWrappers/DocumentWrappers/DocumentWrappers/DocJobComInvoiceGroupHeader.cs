using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.US.DocumentWrappers
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

		protected override DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(Enterprise.Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		#endregion

		#region Implementation

		protected JobComInvoiceGroupHeader JobComInvoiceGroupHeader
		{
			get { return (JobComInvoiceGroupHeader)WrappedObject; }
		}

		#endregion
	}
}
