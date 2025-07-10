using System;
using System.Text;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Web
{
	public class InvoiceRequestHandler : DataRequestHandler<InvoiceRequestHelper>
	{
		protected override BusinessObject[] GetNewBusinessObjects()
		{
			return Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, PKs));
		}

		InvoicingBase[] Invoices
		{
			get { return BusinessObjects as InvoicingBase[]; }
		}

		public override string ContentType
		{
			get { return DataContentTypes.Pdf; }
		}

		protected override void PreProcessRequest()
		{
			base.PreProcessRequest();

			foreach (InvoicingBase invoice in Invoices)
			{
				var canReprint = invoice.CheckCanPrintPostedInvoicingBase();
				if (!canReprint.Result)
				{
					WriteError(HttpContext.Current, canReprint.ReasonForNotBeingAbleToPrint.Replace("\r\n", " "));
					HttpContext.Current.Response.End();

#if DEBUG
					if (Globals.IsTest)
					{
						throw new Exception("Will automatically throw ThreadAbortException when call HttpContext.Current.Response.End()");
					}
#endif
				}
			}
		}

		public override string FileName
		{
			get
			{
				var prefix = new StringBuilder();
				string prevTransactionName = "";

				foreach (InvoicingBase invoice in Invoices)
				{
					var transactionTypeList = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
					string transactionName = transactionTypeList.GetDescriptionFromCode(invoice.AH_TransactionType);
					var invoiceRefNumber = !invoice.AH_ConsolidatedInvoiceRef.IsEmpty ? invoice.AH_ConsolidatedInvoiceRef : invoice.AH_TransactionNum;
					if (prevTransactionName == transactionName)
					{
						prefix.Append(String.Format("{0},", invoiceRefNumber));
					}
					else
					{
						prefix.Append(String.Format("{0}-{1},", transactionTypeList.GetDescriptionFromCode(invoice.AH_TransactionType), invoiceRefNumber));
					}
					prevTransactionName = transactionName;
				}
				string fileName = prefix.ToString().TrimEnd(',') + ".pdf";
				return fileName;
			}
		}

		public override ZBlob GetBinaryData()
		{
			byte[] pdfData = null;

			if (Invoices.Length > 0)
			{
				lock (Invoices[0])
				{
#if DEBUG
					StopStopwatchForTesting();
#endif
					using (new WebLoginBranch(Invoices[0].Branch))
					using (var invoicePrintTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(Invoices)))
					{
						if (invoicePrintTask.TaskCount > 0)
						{
							pdfData = new DocumentUtility(Factory).GetDocument(GetDocumentPack(invoicePrintTask), (OrgContact)AppInstance.SiteUser.LoggedInUser, DataContentTypes.Pdf);
						}
						else
						{
							Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperException(new ApplicationException("No PrintTasks were found"));
						}

						if (pdfData != null && pdfData.Length > 0)
						{
							AddInvoiceRequestedEvents();
						}
					}
				}
			}
			return pdfData;
		}

		protected virtual DocumentPack GetDocumentPack(InvoicePrintTask invoicePrintTask)
		{
			return invoicePrintTask.GetFirstDocumentPack();
		}

		void AddInvoiceRequestedEvents()
		{
			foreach (InvoicingBase invoice in Invoices)
			{
				invoice.AddRequestedViaWebLog();
			}
#if DEBUG
			// To avoid deadlock with the TestCase transaction because WebFactory used in RequestHandler creates its own connection
			if (!Globals.IsTest)
#endif
			{
				Factory.Save();
			}
		}

		public override string NoDataErrorMessage
		{
			get { return Res.GetString("270478ff-a639-45db-9ce5-a51a4dc970a8", "The invoice was not found."); }
		}
	}
}
