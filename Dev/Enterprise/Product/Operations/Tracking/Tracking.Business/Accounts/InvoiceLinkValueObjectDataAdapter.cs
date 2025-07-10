using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	public class InvoiceLinkValueObjectDataAdapter : ValueObjectDataAdapter<InvoicingBase, Xsd.InvoiceLink>
	{
		public override string RootCollectionElementName
		{
			get { return "InvoiceLinks"; }
		}

		public override string RootElementName
		{
			get { return "InvoiceLink"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return XmlSchemaDefinitions.Instance.InvoiceLinksSchema; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleInvoiceLinkSchema; }
		}

		protected override InvoicingBase FindBusinessObject(Xsd.InvoiceLink value, IValueObjectImportContext context)
		{
			return null;
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(InvoicingBase bizObj, Xsd.InvoiceLink value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("InvoiceLink import is not supported");
		}

		#endregion

		#region ExportToValueObjectCore

		protected override void ExportToValueObjectCore(InvoicingBase bizObj, Xsd.InvoiceLink invoiceLinkValue, IValueObjectExportContext context)
		{
			invoiceLinkValue.InvoiceNumber = bizObj.InvoiceNumber;

			if (bizObj.Company != null)
			{
				invoiceLinkValue.IssuerName = bizObj.Company.GC_Name;
			}

			invoiceLinkValue.TransactionType = bizObj.AH_TransactionType;

			invoiceLinkValue.InvoiceTerm = bizObj.AH_InvoiceTerm;

			invoiceLinkValue.InvoiceDate = bizObj.AH_InvoiceDate;

			invoiceLinkValue.DueDate = bizObj.AH_DueDate;

			if (bizObj.TransactionCurrency != null)
			{
				invoiceLinkValue.Currency = bizObj.AH_RX_NKTransactionCurrency;
			}

			ZDecimal ah_OSTotalAmountWithCurrencyDecimalPlaces = bizObj.TransactionCurrency != null ? bizObj.AH_OSTotalAmount.Round(bizObj.TransactionCurrency.Decimals) : bizObj.AH_OSTotalAmount;
			invoiceLinkValue.TotalAmount = ah_OSTotalAmountWithCurrencyDecimalPlaces;

			ZDecimal aH_OSOutstandingAmountCurrencyDecimalPlaces = bizObj.TransactionCurrency != null ? bizObj.AH_Calc_OSOutstandingAmount.Round(bizObj.TransactionCurrency.Decimals) : bizObj.AH_Calc_OSOutstandingAmount;
			invoiceLinkValue.OutstandingAmount = aH_OSOutstandingAmountCurrencyDecimalPlaces;

			if (!bizObj.AH_FullyPaidDate.IsEmpty)
			{
				invoiceLinkValue.FullyPaidDate = bizObj.AH_FullyPaidDate;
			}

			OrgContact webUser = WebEnv.CurrentUser as OrgContact;
			if (webUser != null)
			{
				invoiceLinkValue.Link = TrackingUrlCreator.Instance.CreateUrl(webUser.PK,
					TrackingConstants.BusinessContext.Transaction,
					bizObj.PK);
			}
		}

		#endregion

		public Xsd.InvoiceLinkCollection ExportToXmlValueObjectCollection(TrackingTransactionHeaderCollection invoices, IValueObjectExportContext context)
		{
			Xsd.InvoiceLinkCollection invoiceLinks = new Xsd.InvoiceLinkCollection();
			if (invoices.Count > 0)
			{
				foreach (InvoicingBase invoice in invoices)
				{
					ExportToValueObject(invoice, invoiceLinks.AddNew(), context);
				}
			}

			return invoiceLinks;
		}
	}
}
