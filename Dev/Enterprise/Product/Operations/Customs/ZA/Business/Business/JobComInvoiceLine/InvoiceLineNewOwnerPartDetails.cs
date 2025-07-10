using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	class InvoiceLineNewOwnerPartDetails : IInvoiceLinePartDetails
	{
		public InvoiceLineNewOwnerPartDetails(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}
		readonly JobComInvoiceLine invoiceLine;

		ZString IInvoiceLinePartDetails.CustomsCountryCode => ((IInvoiceLinePartDetails)invoiceLine).CustomsCountryCode;
		OrgHeader IInvoiceLinePartDetails.Importer => invoiceLine.EntryInstruction?.Owner;
		OrgHeader IInvoiceLinePartDetails.Supplier => null;
		BaseJobComInvoiceHeader IInvoiceLinePartDetails.Header => invoiceLine.InvoiceHeader;
		bool IInvoiceLinePartDetails.IsForImportSectionOfDrawback => invoiceLine.IsForImportSectionOfDrawback;
		bool IInvoiceLinePartDetails.IsForExportSectionOfDrawback => invoiceLine.IsForExportSectionOfDrawback;
		bool IInvoiceLinePartDetails.IsDrawback => invoiceLine.Declaration?.IsDrawback ?? false;
		bool IInvoiceLinePartDetails.IsDeleted => invoiceLine.IsDeleted;
		bool IInvoiceLinePartDetails.Enabled => true;
		ZGuid IInvoiceLinePartDetails.PartPK
		{
			get { return invoiceLine.JI_OP_NewOwnerProduct; }
			set { invoiceLine.JI_OP_NewOwnerProduct = value; }
		}
		ZString IInvoiceLinePartDetails.PartNo => invoiceLine.JI_NewOwnerPartNo;
		RefCountry IInvoiceLinePartDetails.InvoiceCountry => invoiceLine.InvoiceHeader?.InvoiceCountry;
		bool IInvoiceLinePartDetails.JustUpdatedByDataRefresh => false;
		Type IInvoiceLinePartDetails.TypeOfPartUsed => invoiceLine.TypeOfPartUsed;
		BusinessObjectFactory IInvoiceLinePartDetails.Factory => invoiceLine.Factory;

		void IInvoiceLinePartDetails.UpdateDetailsOnPartChange()
		{
			if (!invoiceLine.IsCopyingInternal && !invoiceLine.IsDeleted && !invoiceLine.IsNull)
			{
				var product = invoiceLine.NewOwnerProduct;
				invoiceLine.JI_OP_NewOwnerProduct = (product == null || product.IsDeleted) ? ZGuid.Empty : product.PK;
			}
		}

		ZGuid IInvoiceLinePartDetails.PartSyncManagerActiveDeciderPK => invoiceLine.PartSyncManagerActiveDeciderPK;
	}
}
