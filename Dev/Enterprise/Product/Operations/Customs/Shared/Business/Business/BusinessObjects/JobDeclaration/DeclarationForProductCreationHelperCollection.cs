using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class DeclarationForProductCreationHelperCollection
	{
		public DeclarationForProductCreationHelperCollection(BaseJobDeclaration declaration)
		{
			Argument.NotNull(declaration, nameof(declaration));

			this.Declaration = declaration;
			decidedProductCreationHelpers = new List<DeclarationForProductCreationHelper> { };
			undecidedProductCreationHelpers = new List<DeclarationForProductCreationHelper> { };

			CreateProductCreationHelpers();
		}

		public List<string> Create(ProductRelationDefaultOption manualOption)
		{
			var duplicateProducts = new List<string> { };

			foreach (var helper in decidedProductCreationHelpers)
			{
				duplicateProducts.AddRange(Declaration.SaveNewProductsorActivateInactiveOnes(helper.CalculateDefaultOptionForCreateProduct(), helper));
			}

			foreach (var helper in undecidedProductCreationHelpers)
			{
				if (IsOptionAvailableForInvoice(manualOption, helper.InvoiceHeader))
				{
					duplicateProducts.AddRange(Declaration.SaveNewProductsorActivateInactiveOnes(manualOption, helper));
				}
			}

			return duplicateProducts;
		}

		void CreateProductCreationHelpers()
		{
			foreach (var invoice in Declaration.Invoices)
			{
				var helper = invoice.GetProductCreationHelper();
				helper.CalculateDefaultOptionForCreateProduct();

				if (helper.CanChangeTheDefaultOption)
				{
					undecidedProductCreationHelpers.Add(helper);
				}
				else
				{
					decidedProductCreationHelpers.Add(helper);
				}
			}
		}

		bool IsOptionAvailableForInvoice(ProductRelationDefaultOption option, IInvoiceHeaderForProductCreation invoice)
		{
			return (option == ProductRelationDefaultOption.OptionForImporter && invoice.Importer_Effective != null)
				|| (option == ProductRelationDefaultOption.OptionForSupplier && invoice.Supplier_Effective != null)
				|| (option == ProductRelationDefaultOption.None);
		}

		#region OrganizationsInvolved

		public static OrgHeader[] ImportersInvolvedWithDeclaration(BaseJobDeclaration declaration)
		{
			var importerForDeclaration = declaration.Importer != null ? new List<OrgHeader>() { declaration.Importer } : new List<OrgHeader>();
			return IEnumerableExtensions.DistinctBy(
						declaration.Invoices.Where(x => x.Importer_Effective != null).Select(x => x.Importer_Effective).Concat(importerForDeclaration),
						orgHeader => orgHeader.PK)
					.ToArray();
		}

		OrgHeader[] ImportersInvolved => importersInvolved ?? (importersInvolved = ImportersInvolvedWithDeclaration(Declaration));
		OrgHeader[] importersInvolved;

		public static OrgHeader[] SuppliersInvolvedWithDeclaration(BaseJobDeclaration declaration)
		{
			var supplierForDeclaration = declaration.Supplier != null ? new List<OrgHeader>() { declaration.Supplier } : new List<OrgHeader>();
			return IEnumerableExtensions.DistinctBy(
						declaration.Invoices.Where(x => x.Supplier_Effective != null).Select(x => x.Supplier_Effective).Concat(supplierForDeclaration),
						orgHeader => orgHeader.PK)
					.ToArray();
		}

		OrgHeader[] SuppliersInvolved => suppliersInvolved ?? (suppliersInvolved = SuppliersInvolvedWithDeclaration(Declaration));
		OrgHeader[] suppliersInvolved;

		#endregion

		#region Properties

		readonly List<DeclarationForProductCreationHelper> decidedProductCreationHelpers;
		readonly List<DeclarationForProductCreationHelper> undecidedProductCreationHelpers;

		public DeclarationForProductCreationHelper PrimaryProductCreationHelper => undecidedProductCreationHelpers.Any()
																						? undecidedProductCreationHelpers.OrderBy(x => x.InvoiceHeader.InvoiceNumber).FirstOrDefault()
																						: decidedProductCreationHelpers.OrderBy(x => x.InvoiceHeader.InvoiceNumber).FirstOrDefault();

		BaseJobDeclaration Declaration { get; }

		bool CreateRelatedOrganisationAsBothImporterAndExporter => CustomsDataRegistry.Instance.AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.Value;
		public bool ShouldShowWarning => decidedProductCreationHelpers.Any(x => x.ShouldShowWarningLabel) || undecidedProductCreationHelpers.Any(x => x.ShouldShowWarningLabel);
		public static bool ShouldShowProductCreationConfirmation(BaseJobDeclaration declaration) => ImportersInvolvedWithDeclaration(declaration).Any() || SuppliersInvolvedWithDeclaration(declaration).Any();

		public string RelationshipTypeForImporters => CreateRelatedOrganisationAsBothImporterAndExporter && Declaration.IsImport ? OrgPartRelation.RelationshipTypes.Both : OrgPartRelation.RelationshipTypes.Owner;
		public string RelationshipTypeForSuppliers => CreateRelatedOrganisationAsBothImporterAndExporter && Declaration.IsExport ? OrgPartRelation.RelationshipTypes.Both : OrgPartRelation.RelationshipTypes.Supplier;

		public static bool ShouldShowAutomaticProductCreationConfirmation(BaseJobDeclaration declaration)
		{
			if (declaration.Importer == null || declaration.Importer.MiscServ.OM_IMEnablePromptToCreateProducts == EnablePromptToCreateProductsList.Codes.DEF)
			{
				return CustomsDataRegistry.Instance.EnablePromptToCreateProducts.Value;
			}
			return declaration.Importer.MiscServ.OM_IMEnablePromptToCreateProducts == EnablePromptToCreateProductsList.Codes.YES;
		}

		#region Text

		public string ProductRelationOptionForSupplierText
		{
			get
			{
				var result = ZString.Empty;
				if (SuppliersInvolved.Length > 1)
				{
					result = Res.GetString("03D0B57A-258A-438A-ACDB-89A958987466", "Relationship: {0}, Multiple", RelationshipTypeForSuppliers);
				}
				else
				{
					var supplier = SuppliersInvolved.FirstOrDefault();
					if (supplier != null)
					{
						result = Res.GetString("9455A44E-3691-4DEB-B26D-C90D7CF5AA1B", "Relationship: {0}, Code: {1}", RelationshipTypeForSuppliers, supplier.OH_Code);
					}
				}
				return result;
			}
		}

		public string ProductRelationOptionForImporterText
		{
			get
			{
				var result = ZString.Empty;
				if (ImportersInvolved.Length > 1)
				{
					result = Res.GetString("C31A0C3D-573A-48FC-B880-EE228DCCDCFF", "Relationship: {0}, Multiple", RelationshipTypeForImporters);
				}
				else
				{
					var importer = ImportersInvolved.FirstOrDefault();
					if (importer != null)
					{
						result = Res.GetString("B891D8DA-4AD8-4CDD-91FC-0FFAB08B252B", "Relationship: {0}, Code: {1}", RelationshipTypeForImporters, importer.OH_Code);
					}
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
