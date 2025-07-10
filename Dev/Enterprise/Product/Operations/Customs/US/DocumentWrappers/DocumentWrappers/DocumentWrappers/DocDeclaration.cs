using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocDeclaration : DocBaseJobDeclaration
	{
		protected DocDeclaration(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap) : base(jobDeclaration, factoryToWrap)
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

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader
		{
			get { return (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal; }
		}

		public DocJobComInvoiceHeader InvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		public DocCusContainerCollection Containers
		{
			get { return (DocCusContainerCollection)ContainersInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceHeaderCollection InvoiceHeaders
		{
			get { return (DocJobComInvoiceHeaderCollection)InvoiceHeadersInternal; }
		}

		public DocCusEntryHeaderCollection EntryHeaders
		{
			get { return (DocCusEntryHeaderCollection)RateEntryHeaders; }
		}

		public DocJobComInvoiceGroupHeaderCollection InvoiceGroupHeaders
		{
			get { return (DocJobComInvoiceGroupHeaderCollection)InvoiceGroupHeadersInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal; }
		}

		public DocUNLOCO PortOfExport
		{
			get { return DocUNLOCO.New(Declaration.PortOfExport, Factory); }
		}

		protected override Enterprise.DocumentWrappers.DocOrganisation GetImporter()
		{
			if (IsReconciliation)
			{
				return Enterprise.DocumentWrappers.DocOrganisation.New(ReconDeclaration.Importer, Factory);
			}
			return base.GetImporter();
		}

		public override ZString CustomsEntryNumber
		{
			get { return ReconDeclaration != null ? ReconDeclaration.ReconEntryNumberWithEntryFilerCode : base.CustomsEntryNumber; }
		}

		public override ZString EntryPortName
		{
			get { return ReconDeclaration != null ? ReconDeclaration.FilingPortDescription : ZString.Empty; }
		}

		public override ZString EntryPortCode
		{
			get { return ReconDeclaration != null ? ReconDeclaration.US_SchDEntry : ZString.Empty; }
		}

		public override ZString PaymentType
		{
			get { return ReconDeclaration != null ? ReconDeclaration.US_PaymentType : ZString.Empty; }
		}

		public override ZString PaymentTypeDescription
		{
			get { return ReconDeclaration != null ? new PaymentTypeList().GetDescriptionFromCode(ReconDeclaration.US_PaymentType) : ""; }
		}

		#region Overrides

		public override ZString TransportModeDescription
		{
			get
			{
				ZString result = base.TransportModeDescription;

				int index = result.IndexOf(' ');
				if (index > -1)
				{
					result = result.SubstringSafe(0, index);
				}

				return result;
			}
		}

		public override bool IsReconciliation
		{
			get { return MessageType == US.Business.JobMessageTypeList.Codes.Recon; }
		}

		public override ZDateTime ExportDate
		{
			get { return Declaration.US_DateOfExport; }
		}

		protected override OrgAddress ImporterOfRecordAddressCore
		{
			get
			{
				var ior = IsReconciliation ? ReconDeclaration.ImporterOfRecord : Declaration.IOR;
				var organization = ior ?? Declaration.Importer;
				return organization.GetCustomsAddressDetailsFallingBackToMainAddress();
			}
		}

		protected override OrgAddress UltimateConsigneeAddressCore
		{
			get { return Declaration.ConsigneeAddress; }
		}

		protected override ZString SchDEntryWithDescCore
		{
			get { return Declaration.SchDEntryWithDesc; }
		}

		protected override ZString ReleaseStatusCore
		{
			get { return Declaration.ReleaseStatus; }
		}

		protected override ZString ReleaseStatusDescriptionCore
		{
			get { return Declaration.ReleaseStatusDesc; }
		}

		protected override IBusinessObjectCollection<IErrorsRecord> CreateLatestDispositionsCore()
		{
			var result = new StatusErrorsDataViewCollection(Factory);
			var latestDispositionCodes = Declaration.DispositionCodesView.OfType<ErrorsRecord>().GroupBy(x => x.StatusDate).OrderByDescending(x => x.Key).FirstOrDefault();
			if (latestDispositionCodes != null)
			{
				result.AddRange(Declaration.DispositionCodesView.OfType<ErrorsRecord>().Where(x => x.StatusDate == latestDispositionCodes.Key));
			}

			return result;
		}

		#endregion

		#region Implementation
		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)WrappedObject; }
		}

		protected ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null && IsReconciliation)
				{
					reconDeclaration = Declaration.ReconDeclaration ?? new ReconDeclaration(Declaration);
				}
				return reconDeclaration;
			}
		}
		ReconDeclaration reconDeclaration;

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Enterprise.Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Enterprise.Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((US.Business.InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<Enterprise.Customs.Business.CusEntryHeader> collectionToWrap)
		{
			return new DocCusEntryHeaderCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseBillOfLadingCollection CreateNewBillOfLadingsCollection()
		{
			return new DocBillOfLadingCollection(Declaration);
		}

		#endregion
	}
}
