#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocJobComInvoiceHeader : DocBaseJobComInvoiceHeader
	{
		DocJobComInvoiceHeader(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceHeader New(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			if (jobComInvoiceHeader == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceHeader(jobComInvoiceHeader, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseJobDeclaration CreateJobDeclaration(Customs.Business.BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New((JobDeclaration)declarationToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Customs.Business.BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		#endregion

		#region Collections

		public DocJobComInvoiceLineCollection InvoiceLines => (DocJobComInvoiceLineCollection)InvoiceLinesInternal;
		public DocJobComInvoiceLineCollection UnclassifiedInvoiceLines => (DocJobComInvoiceLineCollection)UnclassifiedInvoiceLinesInternal;

		#endregion

		#region Wrapper Fields

		public DocDeclaration Declaration => (DocDeclaration)DeclarationInternal;

		#endregion
	}
}
#endif
