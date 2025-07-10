using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// THIS CLASS SHOULD NOT BE INHERITED FOR A COUNTRY SPECIFIC IMPLEMENTATION OF PRODUCT DEFAULTING, IF YOU BELIEVE OTHERWISE SPEAK TO GLENN LAWSON (GRL)
	/// </summary>
	public class DeclarationForProductCreationHelper
	{
		public DeclarationForProductCreationHelper(IInvoiceHeaderForProductCreation invoiceHeader)
		{
			this.InvoiceHeader = invoiceHeader;
			defaultOption = null;
		}

		#region Organizations

		public OrgHeader Importer => fImporter ?? (fImporter = InvoiceHeader.Importer_Effective);
		OrgHeader fImporter;

		ZBool IsImporterInvolved => Importer != null;

		public OrgHeader Supplier => fSupplier ?? (fSupplier = InvoiceHeader.Supplier_Effective);
		OrgHeader fSupplier;

		ZBool IsSupplierInvolved => Supplier != null;

		#endregion

		#region GetDefaultOption

		public ProductRelationDefaultOption CalculateDefaultOptionForCreateProduct()
		{
			if (!defaultOption.HasValue)
			{
				var orderOfPriority = new Action[]
				{
					SetDefaultOptionForCreateProduct_Elimination,
					SetDefaultOptionForCreateProduct_IntegratedBondedWarehouse,
					SetDefaultOptionForCreateProduct_SupplierImporterLink,
					SetDefaultOptionForCreateProduct_ClientOrganization,
					SetDefaultOptionForCreateProduct_Registry
				};

				foreach (var method in orderOfPriority)
				{
					method.Invoke();
					if (EffectiveOrgProperties.HasBeenSet)
					{
						break;
					}
				}
				defaultOption = EffectiveOrgProperties.Organization;
			}

			return defaultOption.Value;
		}
		ProductRelationDefaultOption? defaultOption;

		#region DefaultByElimination

		void SetDefaultOptionForCreateProduct_Elimination()
		{
			EffectiveOrgProperties.Reset();

			if ((!HasImportDeclaration && !HasExportDeclaration) || (!IsImporterInvolved && !IsSupplierInvolved))
			{
				EffectiveOrgProperties.Set(ProductRelationDefaultOption.None, false, ZString.Empty);
			}
			else if (IsImporterInvolved && !IsSupplierInvolved)
			{
				EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForImporter, true, ZString.Empty);
			}
			else if (IsSupplierInvolved && !IsImporterInvolved)
			{
				EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForSupplier, true, ZString.Empty);
			}
		}

		#endregion

		#region DefaultByIntegratedBondedWarehouseEntry

		public void SetDefaultOptionForCreateProduct_IntegratedBondedWarehouse()
		{
			EffectiveOrgProperties.Reset();

			if (InvoiceHeader.IsInwardBondedWarehousingEnabled)
			{
				EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForImporter, true, behaviourFromWarehouse);
			}
		}

		#endregion

		#region DefaultByBuyerSupplierLink

		void SetDefaultOptionForCreateProduct_SupplierImporterLink()
		{
			EffectiveOrgProperties.Reset();

			var supplierImporterLink = GetSupplierImporterLink(Importer, Supplier);
			switch (supplierImporterLink?.OL_ProductRelation ?? ZString.Empty)
			{
				case OrgRelationTypeList.Codes.Supplier:
					EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForSupplier, true, behaviourFromSupplierAndImporter);
					break;
				case OrgRelationTypeList.Codes.Importer:
					EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForImporter, true, behaviourFromSupplierAndImporter);
					break;
			}
		}

		OrgSupplierBuyerLink GetSupplierImporterLink(OrgHeader importer, OrgHeader supplier)
		{
			OrgSupplierBuyerLink result = null;
			if (importer != null && supplier != null)
			{
				result = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, importer, InvoiceHeader.FinalDestinationCountryCode);
				if (result == null)
				{
					result = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, importer, InvoiceHeader.BranchCompanyCountryCode);
				}
			}
			return result;
		}

		#endregion

		#region DefaultByClientOrg

		void SetDefaultOptionForCreateProduct_ClientOrganization()
		{
			EffectiveOrgProperties.Reset();

			var clientOrganization = GetClientOrganization(Importer, Supplier);
			if (clientOrganization != null)
			{
				var clientData = clientOrganization.MiscServ;
				if (HasImportDeclaration && clientData.OM_IMOwnsProducts)
				{
					EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForImporter, true, behaviourFromImporter);
				}
				else if (HasExportDeclaration && clientData.OM_EXOwnsProducts)
				{
					EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForSupplier, true, behaviourFromSupplier);
				}
			}
		}

		OrgHeader GetClientOrganization(OrgHeader importer, OrgHeader supplier)
		{
			return HasImportDeclaration ? importer : supplier;
		}

		#endregion

		#region DefaultByRegistry

		void SetDefaultOptionForCreateProduct_Registry()
		{
			EffectiveOrgProperties.Reset();

			if (HasImportDeclaration || HasExportDeclaration)
			{
				switch (CustomsDataRegistry.Instance.AssumeCreatedProductBelongsToClient.Value)
				{
					case ClientProductCreationTypeList.Codes.Never:
						EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForSupplier, true, behaviourFromRegistry);
						break;
					case ClientProductCreationTypeList.Codes.No:
						EffectiveOrgProperties.Set(ProductRelationDefaultOption.OptionForSupplier, false, behaviourFromRegistry);
						break;
					case ClientProductCreationTypeList.Codes.Yes:
						EffectiveOrgProperties.Set(GetClientOrganizationOption(), false, behaviourFromRegistry);
						break;
					case ClientProductCreationTypeList.Codes.Always:
						EffectiveOrgProperties.Set(GetClientOrganizationOption(), true, behaviourFromRegistry);
						break;
				}
			}
		}

		ProductRelationDefaultOption GetClientOrganizationOption()
		{
			return HasImportDeclaration ? ProductRelationDefaultOption.OptionForImporter : ProductRelationDefaultOption.OptionForSupplier;
		}

		#endregion

		#endregion

		#region Properties

		public bool ShouldShowWarningLabel => InvoiceHeader.InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ;

		protected string behaviourFromRegistry = Res.GetString("1cc3dcb1-2ae7-4849-adff-1c76cd796c2f", "Behavior defaulted from: Registry");
		protected string behaviourFromImporter = Res.GetString("c67bec5e-f40a-4d34-9da1-1a66b2fb620b", "Behavior defaulted from: Importer");
		protected string behaviourFromSupplier = Res.GetString("710400b4-9cea-481e-b433-9d70d0866fb3", "Behavior defaulted from: Supplier");
		protected string behaviourFromSupplierAndImporter = Res.GetString("F85D7389-9AA6-4EBB-80FD-68B89A39E84D", "Behavior defaulted from: Importer/Supplier Relationship");
		protected string behaviourFromWarehouse = Res.GetString("4A204DBF-09F8-4FC8-B360-9B7B9DEAC356", "Behavior defaulted from: Warehousing Defaults");

		public IInvoiceHeaderForProductCreation InvoiceHeader { get; }

		bool HasImportDeclaration => (hasImportDeclaration ?? (hasImportDeclaration = InvoiceHeader.HasImportDeclaration)).Value;
		bool? hasImportDeclaration;

		bool HasExportDeclaration => (hasExportDeclaration ?? (hasExportDeclaration = InvoiceHeader.HasExportDeclration)).Value;
		bool? hasExportDeclaration;

		EffectiveOrganizationProperties EffectiveOrgProperties => effectiveOrgProperties ?? (effectiveOrgProperties = new EffectiveOrganizationProperties());
		EffectiveOrganizationProperties effectiveOrgProperties;

		public bool CanChangeTheDefaultOption => !EffectiveOrgProperties.IsStrict;
		public ZString BehaviorDefaultedFromText => EffectiveOrgProperties.BehaviorDefaultedFrom;

		#endregion
	}

	#region Helpers

	class EffectiveOrganizationProperties
	{
		public EffectiveOrganizationProperties()
		{
			Reset();
		}

		public ProductRelationDefaultOption Organization { get; private set; }
		public bool IsStrict { get; private set; }
		public ZString BehaviorDefaultedFrom { get; private set; }
		public bool HasBeenSet { get; private set; }

		public void Set(ProductRelationDefaultOption organization, bool isStrict, ZString behaviorDefaultedFrom)
		{
			Organization = organization;
			IsStrict = isStrict;
			BehaviorDefaultedFrom = behaviorDefaultedFrom;
			HasBeenSet = true;
		}

		public void Reset()
		{
			Organization = ProductRelationDefaultOption.None;
			IsStrict = false;
			BehaviorDefaultedFrom = null;
			HasBeenSet = false;
		}
	}

	public enum ProductRelationDefaultOption
	{
		None,
		OptionForImporter,
		OptionForSupplier
	}

	#endregion
}
