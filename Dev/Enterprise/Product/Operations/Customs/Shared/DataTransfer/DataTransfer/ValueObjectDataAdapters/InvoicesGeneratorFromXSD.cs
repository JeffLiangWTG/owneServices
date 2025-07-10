using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoicesGeneratorFromXSD
	{
		public InvoicesGeneratorFromXSD(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		protected BaseJobDeclaration declaration;

		public void ImportInvoicesDetails(Xsd.InvoiceHeaderCollection xmlInvoices, BaseJobDeclaration jobDec, IValueObjectImportContext context)
		{
			GenerateInvoiceGroupHeaders(xmlInvoices, jobDec, context);
			GenerateInvoiceHeaders(xmlInvoices, jobDec, context);
		}

		protected virtual void GenerateInvoiceHeaders(Xsd.InvoiceHeaderCollection xmlInvoices, BaseJobDeclaration jobDec, IValueObjectImportContext context)
		{
			foreach (Xsd.InvoiceHeader invoiceHeaderValue in xmlInvoices)
			{
				if (!invoiceHeaderValue.IsGroupInvoiceSpecified || (invoiceHeaderValue.IsGroupInvoice != Xsd.TrueFalse.@true))
				{
					BaseJobComInvoiceHeader invoiceHeader = GetInvoiceHeader(jobDec.Invoices, invoiceHeaderValue.InvoiceNumber);
					InvoiceAdapter.ImportFromValueObject(invoiceHeader, invoiceHeaderValue, context);
				}
			}
		}

		protected virtual void GenerateInvoiceGroupHeaders(Xsd.InvoiceHeaderCollection xmlInvoices, BaseJobDeclaration jobDec, IValueObjectImportContext context)
		{
			foreach (Xsd.InvoiceHeader invoiceHeaderValue in xmlInvoices)
			{
				if (invoiceHeaderValue.IsGroupInvoiceSpecified && (invoiceHeaderValue.IsGroupInvoice == Xsd.TrueFalse.@true))
				{
					BaseJobComInvoiceGroupHeader invoiceHeader = GetInvoiceGroupHeader(jobDec.JobComInvoiceGroupHeaders, invoiceHeaderValue.InvoiceNumber);
					GroupInvoiceAdapter.ImportFromValueObject(invoiceHeader, invoiceHeaderValue, context);
				}
			}
		}

		protected BaseJobComInvoiceGroupHeader GetInvoiceGroupHeader(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> jobComInvoiceGroupHeaders, string invoiceNumber)
		{
			return GetInvoiceGroupHeader(jobComInvoiceGroupHeaders, invoiceNumber, false);
		}

		protected BaseJobComInvoiceGroupHeader GetInvoiceGroupHeader(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> jobComInvoiceGroupHeaders, string invoiceNumber, bool addNewIfNotFound)
		{
			BaseJobComInvoiceGroupHeader result = null;
			foreach (BaseJobComInvoiceGroupHeader existingGroupInvoiceHeader in jobComInvoiceGroupHeaders)
			{
				if (existingGroupInvoiceHeader.JZ_InvoiceNumber == invoiceNumber)
				{
					result = existingGroupInvoiceHeader;
					break;
				}
			}

			if (result == null)
			{
				result = addNewIfNotFound ? jobComInvoiceGroupHeaders.AddNew() : jobComInvoiceGroupHeaders[0];
			}

			return result;
		}

		protected BaseJobComInvoiceHeader GetInvoiceHeader(InvoiceHeaderActiveCollection invoices, string invoiceNumber)
		{
			BaseJobComInvoiceHeader result = null;
			foreach (BaseJobComInvoiceHeader existingInvoiceHeader in invoices)
			{
				if (existingInvoiceHeader.JZ_InvoiceNumber == invoiceNumber)
				{
					result = existingInvoiceHeader;
					break;
				}
			}
			if (result == null)
			{
				result = invoices.AddNew();
			}

			return result;
		}

		public GroupInvoiceValueObjectDataAdapter GroupInvoiceAdapter
		{
			get { return groupInvoiceAdapter ?? (groupInvoiceAdapter = GetNewGroupInvoiceAdapter()); }
		}
		GroupInvoiceValueObjectDataAdapter groupInvoiceAdapter;

		protected virtual GroupInvoiceValueObjectDataAdapter GetNewGroupInvoiceAdapter()
		{
			return new GroupInvoiceValueObjectDataAdapter(declaration);
		}

		public InvoiceValueObjectDataAdapter InvoiceAdapter
		{
			get { return invoiceAdapter ?? (invoiceAdapter = GetInvoiceAdapter()); }
		}

		InvoiceValueObjectDataAdapter invoiceAdapter;

		protected virtual InvoiceValueObjectDataAdapter GetInvoiceAdapter()
		{
			return new InvoiceValueObjectDataAdapter(declaration);
		}
	}
}
