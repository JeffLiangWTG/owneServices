using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class AttachInvoiceCollection : InvoiceHeaderWithNoDeclarationCollection
	{
		public AttachInvoiceCollection(BaseJobDeclaration declaration)
			: base(declaration.Factory)
		{
			fDeclaration = declaration;

			if (declaration.JE_OH_Importer.IsValid)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(JobDeclarationLookups.ImporterSupplierFilterName, "Property1", declaration.JE_OH_Importer));
			}

			if (declaration.JE_OH_Supplier.IsValid)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(JobDeclarationLookups.ImporterSupplierFilterName, "Property2", declaration.JE_OH_Supplier));
			}
		}

		protected override void OnLoadedIntoCollectionCore(BaseJobComInvoiceHeader invoice)
		{
			base.OnLoadedIntoCollectionCore(invoice);

			if (invoice.JZ_MessageType != fDeclaration.JE_MessageType && !(fDeclaration.IsImport && invoice.IsAdvanceShippingNotice))
			{
				invoice.AddRowWarning(Res.GetString("1984c6aa-0bf5-4000-8c22-9623496a01de", "This invoice has a different message type to the declaration. Invoice message type is '{0}' but declaration message type is '{1}'.", invoice.JZ_MessageType, fDeclaration.JE_MessageType));
			}

			if (invoice.JZ_OH_Supplier != fDeclaration.JE_OH_Supplier)
			{
				ZString invoiceSupplierCode = invoice.Supplier == null ? ZString.Empty : invoice.Supplier.OH_Code;
				ZString declarationSupplierCode = fDeclaration.Supplier == null ? ZString.Empty : fDeclaration.Supplier.OH_Code;
				invoice.AddRowWarning(Res.GetString("19401e11-7c57-4e33-a6be-62574fc479de", "This invoice has a different supplier to the Declaration. Invoice supplier is '{0}' but declaration supplier is '{1}'.", invoiceSupplierCode, declarationSupplierCode));
			}

			if (invoice.JZ_OH_Buyer != fDeclaration.JE_OH_Importer)
			{
				OrgHeader buyer = Factory.Load<OrgHeader>(invoice.JZ_OH_Buyer);
				ZString invoiceBuyerCode = buyer == null ? ZString.Empty : buyer.OH_Code;
				ZString declarationImporterCode = fDeclaration.Importer == null ? ZString.Empty : fDeclaration.Importer.OH_Code;
				invoice.AddRowWarning(Res.GetString("8ba47d6f-82cb-4185-869e-f104a23934a0", "This invoice has a different importer to the Declaration. Invoice importer is '{0}' but declaration importer code is '{1}'.", invoiceBuyerCode, declarationImporterCode));
			}

			if (((ICustomsFileParent)invoice).IsLocked)
			{
				invoice.AddRowError(Res.GetString("6692C986-EEFD-44E9-888C-130FEE4CBEAE", "This invoice is Locked and cannot be added to a Customs Declaration until it has been Unlocked."));
			}
		}

		protected readonly BaseJobDeclaration fDeclaration;
	}
}
