using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.DocumentEngine.Web;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Business
{
	public class InvoicingService : IInvoicingService
	{
		public IWebTrackerPrintResult Print(Guid contactPK, Guid invoicePK)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(InvoicingService) };

			var invoice = factory.Load<InvoicingBase>(invoicePK);
			if (invoice == null)
			{
				return null;
			}

			var contact = factory.Load<OrgContact>(new ZGuid(contactPK));

			if (contact == null)
			{
				return null;
			}

			var relatedOrgs = factory.Load<OrgHeader>(OrgContactWebUser.GetRelatedOrgQuery(contact.OC_OH));
			if (!relatedOrgs.Any(x => x.PK == invoice.AH_OH))
			{
				return null;
			}

			var (canPrint, errorMessage) = invoice.CheckCanPrintPostedInvoicingBase();
			if (!canPrint)
			{
				return new InvoicingPrintResult
				{
					ErrorMessage = errorMessage,
				};
			}

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, invoice.Branch.PK.ToGuid(), Env.CurrentDepartment.PK)))
			using (var invoicePrintTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice)))
			{
				if (invoicePrintTask.TaskCount == 0)
				{
					return null;
				}

				var invoiceType = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(invoice.AH_TransactionType);
				var invoiceRefNumber = !invoice.AH_ConsolidatedInvoiceRef.IsEmpty ? invoice.AH_ConsolidatedInvoiceRef : invoice.AH_TransactionNum;
				var pdfData = new DocumentUtility(factory).GetDocument(invoicePrintTask.GetFirstDocumentPack(), contact, DataContentTypes.Pdf);

				return new InvoicingPrintResult
				{
					FileName = FormattableString.Invariant($"{invoiceType}-{invoiceRefNumber}.pdf"), // File name
					FileContents = new MemoryStream(pdfData),
				};
			}
		}
	}
}
