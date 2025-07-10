using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class RefundControlSheetDocumentWrapper : DA63DocumentWrapper, DocumentEngineCore.DocWrappers.IBODocDataProvider, IDocumentWrapper
	{
		public RefundControlSheetDocumentWrapper(CusEntryHeader cusEntryHeader)
			: base(cusEntryHeader)
		{
			Argument.NotNull(cusEntryHeader, "cusEntryHeader");
			refundControlSheetEntryLineWrapper = null;
			CUSDECSource = new MessageSendingObject(cusEntryHeader);
		}

		public BusinessObjectCollectionWrapper<RefundControlSheetEntryLineWrapper> RefundEntryLines
		{
			get
			{
				if (refundControlSheetEntryLineWrapper == null)
				{
					refundControlSheetEntryLineWrapper = new BusinessObjectCollectionWrapper<RefundControlSheetEntryLineWrapper>(System.Array.Empty<RefundControlSheetEntryLineWrapper>());

					var cusEntryLinesWithRefunds = EntryHeader.InvoiceLines.OfType<JobComInvoiceLine>()
							.Where(x => x.CusEntryLine != null && x.IsRefundRebateType5P && x.IsRefundRebateTariff)
							.Select(c => c.CusEntryLine).Cast<CusEntryLine>();

					if (cusEntryLinesWithRefunds != null && cusEntryLinesWithRefunds.Any())
					{
						var refundControlSheetEntryLineWrappers = cusEntryLinesWithRefunds.Select(x => new RefundControlSheetEntryLineWrapper(x));
						refundControlSheetEntryLineWrapper = new BusinessObjectCollectionWrapper<RefundControlSheetEntryLineWrapper>(refundControlSheetEntryLineWrappers);
					}
				}
				return refundControlSheetEntryLineWrapper;
			}
		}

		BusinessObjectCollectionWrapper<RefundControlSheetEntryLineWrapper> refundControlSheetEntryLineWrapper;

		public ZBool HasRefundLines => RefundEntryLines != null && RefundEntryLines.Count > 0;

		#region Entry Header Data

		public ZString ApplicantsName => Declaration == null ? ZString.Empty : Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, Declaration.JE_OH_AgentOverride))?.OH_FullName ?? ZString.Empty;
		public ZString ApplicantsReferenceNumber => Declaration == null ? ZString.Empty : Declaration.JE_DeclarationReference;
		public ZString BranchOfficeName => CustomsOfficeCode;

		#endregion
	}
}
