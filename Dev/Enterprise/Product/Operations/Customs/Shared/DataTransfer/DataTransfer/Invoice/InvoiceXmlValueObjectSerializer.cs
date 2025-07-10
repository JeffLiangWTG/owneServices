using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoiceXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public InvoiceXmlValueObjectSerializer()
			: base(typeof(Xsd.InvoiceHeader))
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter iDataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			StandAloneInvoiceValueObjectDataAdapter dataAdapter = (StandAloneInvoiceValueObjectDataAdapter)iDataAdapter;
			Xsd.InvoiceHeader invoiceValueObject = (Xsd.InvoiceHeader)valueObject;

			BaseJobComInvoiceHeader[] existingInvoices = GetExistingInvoices(invoiceValueObject, collection.Factory, context);
			BaseJobComInvoiceHeader invoice = null;

			if (existingInvoices.Length > 0)
			{
				if (existingInvoices.Length == 1)
				{
					invoice = existingInvoices[0];
				}
				else
				{
					context.Notify(new InfoNotification(Res.GetString("80bdcd2e-0574-40d0-bea2-df4988dcdd8a", "There is more than one invoice that matches the invoice number and the supplier.")));
				}
			}
			else
			{
				invoice = context.Factory.New<BaseJobComInvoiceHeader>();
			}

			if (invoice != null)
			{
				new FakeDeclarationCreatorForInvoice(invoice);
				dataAdapter.ImportFromValueObject(invoice, invoiceValueObject, context);

				if (!invoice.IsDeleted && !collection.Contains(invoice.PK))
				{
					collection.Add(invoice);
				}
			}
			return invoice;
		}

		BaseJobComInvoiceHeader[] GetExistingInvoices(Xsd.InvoiceHeader invoice, BusinessObjectFactory factory, IValueObjectImportContext context)
		{
			ZQuery query = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoice.InvoiceNumber);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, DBNull.Value);

			ZQuery branchQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			query.AddToFilter(branchQuery);

			if (invoice.Consignor.IsSpecified)
			{
				query.AddToFilter(JobComInvoiceHeaderSchema.JZ_OH_Supplier, GetMatchedOrganisation(invoice.Consignor, context));
			}

			return factory.Load<BaseJobComInvoiceHeader>(query);
		}

		protected ZGuid GetMatchedOrganisation(Xsd.Organisation organisation, IValueObjectImportContext context)
		{
			return context.FindOrCreateTempOrganisationPK(organisation, null, OrganisationTypes.None);
		}
	}
}
