using System;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoiceValueObjectDataAdapter : InvoiceChargeValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader>
	{
		public InvoiceValueObjectDataAdapter(BaseJobDeclaration jobDec)
		{
			if (jobDec == null)
			{
				throw new ArgumentNullException(nameof(jobDec), "jobDec argument is null");
			}
			this.JobDec = jobDec;
		}

		#region Overrides

		public override string RootCollectionElementName
		{
			get { throw new NotSupportedException(); }
		}

		public override string RootElementName
		{
			get { return "InvoiceHeader"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return CustomsXmlSchemaDefinitions.Instance.SingleInvoiceSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.ConsolsSchema; }
		}

		protected override BaseJobComInvoiceHeader FindBusinessObject(Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			return null;
		}

		#endregion

		#region Import

		protected readonly BaseJobDeclaration JobDec;

		protected override void ImportFromValueObjectCore(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xsdInvoiceHeader, IValueObjectImportContext context)
		{
			InvoiceDataTransferTool.ImportInvoiceDetails(invoiceHeader, xsdInvoiceHeader, context);

			ImportInvoiceChargesDetails(xsdInvoiceHeader.InvoiceCharges, invoiceHeader.Charges, context);

			AddImportEvent(invoiceHeader);
		}

		protected override BaseJobComInvoiceHeader NewBusinessObject(Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			return JobDec.Invoices.AddNew();
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(BaseJobComInvoiceHeader invoiceHeader, Xsd.InvoiceHeader xsdInvoiceHeader, IValueObjectExportContext context)
		{
			InvoiceDataTransferTool.ExportInvoiceHeaderValues(invoiceHeader, xsdInvoiceHeader, context);
			xsdInvoiceHeader.InvoiceCharges = ExportInvoiceCharges(invoiceHeader.Charges);
			AddExportEvent(xsdInvoiceHeader, invoiceHeader, context);
		}

		#endregion

		#region Implementation

		protected InvoiceDataTransferTool InvoiceDataTransferTool
		{
			get { return fInvoiceDataTransferTool ?? (fInvoiceDataTransferTool = GetInvoiceDataTransferTool()); }
		}
		InvoiceDataTransferTool fInvoiceDataTransferTool;

		protected virtual InvoiceDataTransferTool GetInvoiceDataTransferTool()
		{
			return new InvoiceDataTransferTool(false);
		}

		#endregion

	}
}
