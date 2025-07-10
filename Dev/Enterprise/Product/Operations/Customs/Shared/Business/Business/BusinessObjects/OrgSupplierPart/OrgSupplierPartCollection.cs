using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class OrgSupplierPartCollection : MasterFiles.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine, bool isExport)
			: this(factory, invoiceLine, invoiceLine.Supplier_Effective, invoiceLine.Importer_Effective, isExport)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport)
			: base(factory, supplier, owner, false, ZString.Empty, ZString.Empty, isExport)
		{
			this.fInvoiceLine = null;
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine, OrgHeader supplier, OrgHeader owner, bool isExport)
				: base(factory, supplier, owner, invoiceLine.JI_PartNoInfo.HasWarnings(), invoiceLine.JI_Description, invoiceLine.JI_InvoiceUQ, isExport)
		{
			fInvoiceLine = invoiceLine;
		}

		readonly BaseJobComInvoiceLine fInvoiceLine;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var part = (OrgSupplierPart)child;
			if (fInvoiceLine != null)
			{
				AddAdditionalLineDetails(part, fInvoiceLine);
				AddPivotWithAdditionalLineDetails(part, fInvoiceLine);
				AddCustomFields(part, fInvoiceLine);
			}
			hasAdditionalOrgHeader = false;
		}

		public void AddPivotWithAdditionalLineDetails(OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine) => AddPivotWithAdditionalLineDetailsCore(part, invoiceLine);

		protected virtual void AddPivotWithAdditionalLineDetailsCore(OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			if (part != null && invoiceLine != null && (invoiceLine.JI_CC.IsValid || !invoiceLine.JI_Tariff.IsEmpty))
			{
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_ChildType = invoiceLine.GetPartPivotType();
				if (invoiceLine.JI_CC.IsValid)
				{
					pivot.CI_CC = invoiceLine.JI_CC;
				}
				else
				{
					pivot.CI_TariffNum = invoiceLine.JI_Tariff.Left(BaseCusClassPartPivot.Schema.CI_TariffNumMaxLength);
				}
			}
		}

		protected virtual void AddAdditionalLineDetails(OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.Commodity_Code != null)
			{
				part.OP_RH_NKCommodityCode = invoiceLine.Commodity_Code.RH_Code;
			}

			foreach (UNDGDataItem invoiceLineDGItem in invoiceLine.UNDGs)
			{
				if (invoiceLineDGItem.Substance != null)
				{
					UNDGDataItem partDGItem = part.UNDGs.AddNew();
					partDGItem.DI_DG = invoiceLineDGItem.DI_DG;
					partDGItem.DI_DGFlashPoint = invoiceLineDGItem.DI_DGFlashPoint;
					partDGItem.DI_OC_DGContact = invoiceLineDGItem.DI_OC_DGContact;
				}
			}
		}

		void AddCustomFields(OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			if (!CopyCustomFieldsFromInvoiceLineToProduct)
			{
				return;
			}

			if (part.RelatedOrganisations.Any())
			{
				part.RelatedOrganisations[0].OU_FormLayoutController = true;
			}

			var lineCustomBizo = invoiceLine.GetCustomBusinessObject();

			if (lineCustomBizo != null)
			{
				var productCustomBizo = part.GetCustomBusinessObject(true);
				var matchedCustomFields = productCustomBizo.GetOrderedCustomProperties().Intersect(lineCustomBizo.GetOrderedCustomProperties()).Where(x => !x.EndsWith((NoResString)"Info"));

				foreach (var field in matchedCustomFields)
				{
					if (lineCustomBizo[field] is IZType fieldValue && !fieldValue.IsEmpty)
					{
						productCustomBizo[field] = lineCustomBizo[field];
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected override (OrgHeader Org, string Relationship)? GetNewRelationship()
		{
			(OrgHeader Org, string Relationship)? result = null;

			if (hasAdditionalOrgHeader) // When auto creating the product after the on save selection dialog.
			{
				result = base.GetNewRelationship();
			}
			else // When auto creating product using F3.
			{
				var invoiceHeader = fInvoiceLine?.InvoiceHeader;
				if (invoiceHeader != null)
				{
					var helper = new DeclarationForProductCreationHelper(invoiceHeader);
					var option = helper.CalculateDefaultOptionForCreateProduct();
					if (option == ProductRelationDefaultOption.OptionForImporter)
					{
						result = (EffectiveOwner(fOwner), OrgPartRelation.RelationshipTypes.Owner);
					}
					else if (option == ProductRelationDefaultOption.OptionForSupplier)
					{
						result = (EffectiveSupplier(fSupplier), OrgPartRelation.RelationshipTypes.Supplier);
					}
				}
			}

			return result;
		}

		protected override bool ShouldCreateRelationshipAsBoth(OrgHeader orgForRelationship, string relationship)
		{
			return base.ShouldCreateRelationshipAsBoth(orgForRelationship, relationship)
				|| (AlwaysAssumeLocalOrganisationIsBothImporterAndExporter && (hasImportParentAndRelationshipIsOwner() || hasExportParentAndRelationshipIsSupplier()));

			bool hasImportParentAndRelationshipIsOwner() => HasImportParent && relationship == OrgPartRelation.RelationshipTypes.Owner;
			bool hasExportParentAndRelationshipIsSupplier() => HasExportParent && relationship == OrgPartRelation.RelationshipTypes.Supplier;
		}

		protected bool HasImportParent => fInvoiceLine?.IsImport ?? false;

		protected bool HasExportParent => fInvoiceLine?.IsExport ?? false;

		public bool AlwaysAssumeLocalOrganisationIsBothImporterAndExporter =>
			CustomsDataRegistry.Instance.AlwaysAssumeLocalOrganisationIsBothImporterAndExporter.Value;

		public bool CopyCustomFieldsFromInvoiceLineToProduct =>
			DataRegistry.Business.CustomsDataRegistry.Instance.CopyCustomFieldsFromInvoiceLineToProduct.Value;

		protected override OrgHeader EffectiveSupplier(OrgHeader supplier)
		{
			if (hasAdditionalOrgHeader)
			{
				return additionalSupplier;
			}
			else
			{
				return base.EffectiveSupplier(supplier);
			}
		}

		protected override OrgHeader EffectiveOwner(OrgHeader owner)
		{
			if (hasAdditionalOrgHeader)
			{
				return additionalImporter;
			}
			else
			{
				return base.EffectiveOwner(owner);
			}
		}

		public MasterFiles.Business.OrgSupplierPart AdditionalAddNewByOrgHeader(OrgHeader supplier, OrgHeader importer, bool hasAdditional)
		{
			try
			{
				this.hasAdditionalOrgHeader = hasAdditional;
				additionalSupplier = supplier;
				additionalImporter = importer;
				return this.AddNew();
			}
			finally
			{
				hasAdditionalOrgHeader = false;
				additionalSupplier = null;
				additionalImporter = null;
			}
		}

		public void UpdatePart(OrgSupplierPart part, OrgHeader supplier, OrgHeader importer, bool hasAdditional)
		{
			try
			{
				this.hasAdditionalOrgHeader = hasAdditional;
				additionalSupplier = supplier;
				additionalImporter = importer;
				SetupNewElementButDoNotAddIt(part, true);
				part.HasChanges = true;
			}
			finally
			{
				hasAdditionalOrgHeader = false;
				additionalSupplier = null;
				additionalImporter = null;
			}
		}

		bool hasAdditionalOrgHeader;
		OrgHeader additionalSupplier, additionalImporter;
	}
}
