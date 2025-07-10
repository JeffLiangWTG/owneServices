#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocDeclaration : DocBaseJobDeclaration, Integration.Customs._CustomsTemplate_.IDocDeclaration
	{
		protected DocDeclaration(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(jobDeclaration, factoryToWrap)
		{
		}

		public static DocDeclaration New(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			DocDeclaration result = null;

			if (jobDeclaration != null)
			{
				result = new DocDeclaration(jobDeclaration, factoryToWrap);
			}

			return result;
		}

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(Customs.Business.ICusContainerCollection<Customs.Business.BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(Customs.Business.BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(Customs.Business.IJobComInvoiceGroupHeaderCollection<Customs.Business.BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((Customs.Business.IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection((InvoiceLineCompleteCollection)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			Customs.Business.ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionToWrap)
		{
			return new DocCusEntryHeaderCollection(collectionToWrap, Factory);
		}

		#region Wrapper Fields

		public DocJobComInvoiceHeader InvoiceHeader => (DocJobComInvoiceHeader)InvoiceHeaderInternal;

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader => (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal;

		#endregion

		#region Collections

		public DocCusContainerCollection Containers => (DocCusContainerCollection)ContainersInternal;

		public DocJobComInvoiceLineCollection InvoiceLines => (DocJobComInvoiceLineCollection)InvoiceLinesInternal;

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo => (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal;

		#endregion
	}
}
#endif
