using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingJobInvoicesExporter
	{
		public ForwardingJobInvoicesExporter(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public Xsd.TxnHeaderCollection PopulateInvoicesToXSD(ZString jobNumber, IValueObjectExportContext context, bool isConsol)
		{
			object filterProvider = Activator.CreateInstance(ObjectFactory.GetType<Accounting.Integration.ITransactionExportFilterProvider>(), new object[] { Factory });
			IConsolOrShipmentTransExportFilter invoiceFilter = (IConsolOrShipmentTransExportFilter)Activator.CreateInstance(
				ObjectFactory.GetType<Accounting.Integration.IJobRelatedARInvoicesExportFilter>(),
				new object[] { Factory, filterProvider, jobNumber, isConsol });

			Xsd.TxnHeaderCollection invoicesValue = new Xsd.TxnHeaderCollection();
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(invoiceFilter.Filter, invoiceFilter.BusinessObjectType);

			foreach (BusinessObject invoice in reader)
			{
				Xsd.TxnHeader invoiceValue = new Xsd.TxnHeader();
				FinancialInvoiceDataAdapter.ExportToValueObject(invoice, invoiceValue, context);
				invoicesValue.Add(invoiceValue);
			}

			return invoicesValue;
		}

		IValueObjectDataAdapter FinancialInvoiceDataAdapter
		{
			get
			{
				if (financialInvoiceDataAdapter == null)
				{
					financialInvoiceDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<Accounting.Integration.IFinancialInvoiceDataAdapter>(), new object[] { false });
				}

				return financialInvoiceDataAdapter;
			}
		}
		IValueObjectDataAdapter financialInvoiceDataAdapter;

		readonly BusinessObjectFactory Factory;
	}
}
