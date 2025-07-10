using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class GroupInvoiceValueObjectDataAdapter : InvoiceChargeValueObjectDataAdapter<BaseJobComInvoiceGroupHeader, Xsd.InvoiceHeader>
	{
		public GroupInvoiceValueObjectDataAdapter(BaseJobDeclaration jobDec) : base()// : base (JobDec)
		{
			this.jobDec = jobDec;
		}

		readonly BaseJobDeclaration jobDec;

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

		protected override BaseJobComInvoiceGroupHeader FindBusinessObject(Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			return null;
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(BaseJobComInvoiceGroupHeader bizObj, Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			SetGroupInvoiceDetails(bizObj, value, context);
			AddImportEvent(bizObj);
		}

		protected override BaseJobComInvoiceGroupHeader NewBusinessObject(Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			return jobDec.JobComInvoiceGroupHeaders[0];
		}

		protected void SetGroupInvoiceDetails(BaseJobComInvoiceGroupHeader invoiceGroupHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			invoiceGroupHeader.JZ_GroupInvoice = true;
			if (xmlInvoiceHeader.InvoiceDate.IsValid)
			{
				invoiceGroupHeader.JZ_InvoiceDate = (ZDateTime)xmlInvoiceHeader.InvoiceDate;
			}
			ImportInvoiceChargesDetails(xmlInvoiceHeader.InvoiceCharges, invoiceGroupHeader.Charges, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(BaseJobComInvoiceGroupHeader bizObj, Xsd.InvoiceHeader constructedValueObject, IValueObjectExportContext context)
		{
			SetXmlGroupInvoiceHeaderValues(bizObj, constructedValueObject, context);
			AddExportEvent(constructedValueObject, bizObj, context);
		}

		protected void SetXmlGroupInvoiceHeaderValues(BaseJobComInvoiceGroupHeader invoiceHeader, Xsd.InvoiceHeader xmlInvoiceHeader, IValueObjectExportContext context)
		{
			xmlInvoiceHeader.InvoiceNumber = invoiceHeader.JZ_InvoiceNumber;
			xmlInvoiceHeader.IsGroupInvoice = Xsd.TrueFalse.@true;
			xmlInvoiceHeader.IsGroupInvoiceSpecified = true;
			if (invoiceHeader.JZ_InvoiceDate.IsValid)
			{
				xmlInvoiceHeader.InvoiceDate = (ZDate)invoiceHeader.JZ_InvoiceDate;
			}
			xmlInvoiceHeader.Incoterm = invoiceHeader.JZ_IncoTerm;
			xmlInvoiceHeader.InvoiceCharges = ExportInvoiceCharges(invoiceHeader.Charges);

			xmlInvoiceHeader.Weight.IsSpecified = true;
			xmlInvoiceHeader.Volume.IsSpecified = true;
		}

		#endregion
	}
}
