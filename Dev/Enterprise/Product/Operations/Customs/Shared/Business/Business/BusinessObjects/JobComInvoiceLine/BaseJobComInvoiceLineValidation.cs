using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public BaseJobComInvoiceLineValidation(AutoJobComInvoiceLine parent) : base(parent)
		{
		}

		#region Parent (BaseJobComInvoiceLine)

		public new BaseJobComInvoiceLine Parent
		{
			get { return (BaseJobComInvoiceLine)base.Parent; }
		}

		#endregion

		// OK to cache as Validation is created each time JobComInvoiceLine.Validation is touched
		public BaseJobDeclaration Declaration
		{
			get
			{
				if (!hasCalculateDeclaration)
				{
					hasCalculateDeclaration = true;
					declaration = Parent.Declaration;
				}
				return declaration;
			}
		}
		BaseJobDeclaration declaration;
		bool hasCalculateDeclaration;

		// OK to cache as Validation is created each time JobComInvoiceLine.Validation is touched
		public bool IsExport
		{
			get
			{
				if (!isExport.HasValue)
				{
					isExport = IsExportCore;
				}
				return isExport.Value;
			}
		}
		bool? isExport;

		protected virtual bool IsExportCore => Parent?.IsExport ?? false;

		// OK to cache as Validation is created each time JobComInvoiceLine.Validation is touched
		public bool IsImport
		{
			get
			{
				if (!isImport.HasValue)
				{
					isImport = IsImportCore;
				}
				return isImport.Value;
			}
		}
		bool? isImport;

		protected virtual bool IsImportCore => Parent?.IsImport ?? false;

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUnitPrice();
			ValidateJI_Calc_Invoice();
			ValidateJI_Calc_FreightInInvoiceCurr();
			ValidateJI_Calc_InsuranceInInvoiceCurr();
			ValidateExposedCusEntryLineErrorProperty();
			ValidateJI_IsClassUsageCommentRead();
		}

		#endregion

		#region Properties

		public static string ClassificationIsNotValid
		{
			get { return Res.GetString("DB62584C-B709-4da8-8D7A-7AD2E9171A9E", "The code you have selected is not in the list."); }
		}

		public static string ClassificationIsNotActive
		{
			get { return Res.GetString("36d3ea99-7b34-4e5e-a21a-9377b1e3ed53", "The selected classification is not active"); }
		}

		public static string MandatoryForAutoCreateProduct
		{
			get { return Res.GetString("b2995b9a-63ca-4a32-b567-76339c9ec214", "This field is required to auto-create a Product."); }
		}

		public static string MandatoryCCOrTariffForAutoCreateProduct
		{
			get { return Res.GetString("9365acd1-0ef5-4929-8214-514a610dcc5d", "Either Classification or Tariff is required to auto-create a Product."); }
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();

			if (!Parent.JI_PartNo.IsEmpty && Parent.Part == null && Parent.JI_Description.IsEmpty)
			{
				Parent.JI_DescriptionInfo.AddWarning(MandatoryForAutoCreateProduct);
			}
		}

		#endregion

		#region CheckJI_IsClassUsageCommentRead

		public void ValidateJI_IsClassUsageCommentRead()
		{
			((IValidationInternals)this).Validate(Parent.JI_IsClassUsageCommentReadInfo, CheckJI_IsClassUsageCommentRead);
		}

		public void CheckJI_IsClassUsageCommentRead()
		{
			if (!Parent.JI_ClassUsageComment.IsEmpty && !Parent.JI_IsClassUsageCommentRead)
			{
				var action = CustomsDataRegistry.Instance.SeverityLevelOfUsageCommentValidation.GetFallBackValueAtAllLevels(Parent.RegistryCompanyPK, Guid.Empty, Guid.Empty);

				var notificationType = action == ProductAuditActions.Codes.AddMessageErrorValidation
					? CargoWise.EntityFramework.NotificationType.MessageError
					: action == ProductAuditActions.Codes.AddWarningValidation
						? CargoWise.EntityFramework.NotificationType.Warning
						: null;

				if (notificationType != null)
				{
					Parent.JI_IsClassUsageCommentReadInfo.AddNotification(notificationType, Res.GetString("57B116C8-8116-4455-81C2-A0D93043054E", "Usage Comment exist, please confirm these have been read by ticking the 'Is Usage Comment Read?' field."));
				}
			}
		}

		#endregion

		public void ValidateExposedCusEntryLineErrorProperty()
		{
			((IValidationInternals)this).Validate(Parent.ExposedCusEntryLineErrorPropertyInfo, CheckExposedCusEntryLineErrorProperty);
		}

		protected virtual void CheckExposedCusEntryLineErrorProperty()
		{
			var entryLine = Parent.CusEntryLine;
			if (entryLine != null && !entryLine.DescriptionSource.IsEmpty && !entryLine.CL_Description.IsWesternEuropeanOrEmpty)
			{
				Parent.ExposedCusEntryLineErrorPropertyInfo.AddError(ResString.GetMultilingualString("A6E97FDF-0F33-4683-A70B-0953624F5C67", "Entry line description has non-Western European characters that are copied from {0}.", entryLine.DescriptionSource));
			}
		}

		#region CheckJI_CC

		protected override void CheckJI_CC()
		{
			base.CheckJI_CC();

			var classification = Parent.Classification;
			if (classification != null)
			{
				if (!classification.IsBoth && ((classification.IsExport && !Parent.UseExportClassification) || (classification.IsImport && !Parent.UseImportClassification)))
				{
					Parent.JI_CCInfo.AddMessageError(ClassificationIsNotValid);
				}
				else if (!classification.CC_IsActive)
				{
					Parent.JI_CCInfo.AddWarning(ClassificationIsNotActive);
				}

				if (!Parent.IsLookupAudited())
				{
					var invoiceHeader = Parent.InvoiceHeader;
					if (invoiceHeader != null)
					{
						var auditAction = invoiceHeader.ProductAuditAction();
						if (auditAction == ProductAuditActions.Codes.AddWarningValidation)
						{
							Parent.JI_CCInfo.AddWarning(ClassificationLookupHasNotBeenAudited);
						}
						else if (auditAction == ProductAuditActions.Codes.AddMessageErrorValidation)
						{
							Parent.JI_CCInfo.AddMessageError(ClassificationLookupHasNotBeenAudited);
						}
					}
				}
			}

			if (ClassificationIsRequiredForAutoCreationOfProduct && !Parent.JI_PartNo.IsEmpty && Parent.Part == null && Parent.JI_CC.IsEmpty && Parent.JI_Tariff.IsEmpty)
			{
				Parent.JI_CCInfo.AddWarning(MandatoryCCOrTariffForAutoCreateProduct);
			}
		}

		protected virtual bool ClassificationIsRequiredForAutoCreationOfProduct
		{
			get { return true; }
		}

		protected virtual bool EitherCCOrTariffRequiredForAutoCreationOfProduct => true;

		public static string ClassificationLookupHasNotBeenAudited
		{
			get
			{
				return Res.GetString("ec439a13-9e26-4efb-a9be-bb2c4568e24e", "This Classification Lookup has not been audited.");
			}
		}

		#endregion

		#region Attributes

		protected override void CheckJI_PartAttrib1()
		{
			base.CheckJI_PartAttrib1();
			CheckPartAttribute(Parent.JI_PartAttrib1Info, 1);
		}

		protected override void CheckJI_PartAttrib2()
		{
			base.CheckJI_PartAttrib2();
			CheckPartAttribute(Parent.JI_PartAttrib2Info, 2);
		}

		protected override void CheckJI_PartAttrib3()
		{
			base.CheckJI_PartAttrib3();
			CheckPartAttribute(Parent.JI_PartAttrib3Info, 3);
		}

		void CheckPartAttribute(ZPropertyInfo info, int attribNumber)
		{
			if (Parent.Declaration is BaseJobDeclaration dec && dec.Importer is OrgHeader importer && Parent.Part is OrgSupplierPart part && !importer.PartAttributeManager.IsPartAttributeReleaseCaptured(part, attribNumber))
			{
				PartAttributeValidation.CheckAttribute(importer, part, info, attribNumber);
			}
		}

		protected override void CheckJI_SerialNumber()
		{
			base.CheckJI_SerialNumber();
			if (Parent.Declaration is BaseJobDeclaration dec && dec.Importer is OrgHeader importer && Parent.Part is OrgSupplierPart part && !importer.PartAttributeManager.IsSerialNumberReleaseCaptured(part))
			{
				PartAttributeValidation.CheckSerialNumber(importer, part, Parent.JI_SerialNumberInfo);
			}
		}

		public PartAttributeValidation PartAttributeValidation => partAttributeValidation ?? (partAttributeValidation = GetPartAttributeValidation());
		PartAttributeValidation partAttributeValidation;

		protected virtual PartAttributeValidation GetPartAttributeValidation()
		{
			return new PartAttributeValidation();
		}

		#endregion

		bool IsOutwardBondedWarehousingEnabled(BaseJobDeclaration declaration)
		{
			return declaration.SupportMultipleWarehouseEntry ? (Parent.CusEntryLine?.Header?.IsOutwardBondedWarehousingEnabled ?? false) : declaration.IsOutwardBondedWarehousingEnabled;
		}

		bool IsInwardBondedWarehousingEnabled(BaseJobDeclaration declaration)
		{
			return declaration.SupportMultipleWarehouseEntry ? (Parent.CusEntryLine?.Header?.IsInwardBondedWarehousingEnabled ?? false) : declaration.IsInwardBondedWarehousingEnabled;
		}

		#region CheckJI_InvoiceQuantity
		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsInvoiceQuantityRequiredForBondedWarehouse && !Parent.IsBondedWarehousingDisabled && Parent.SupportsBondedWarehousing &&
				Parent.JI_InvoiceQuantity <= 0m)
			{
				if (IsOutwardBondedWarehousingEnabled(declaration))
				{
					if (declaration.IsWHSUniversalXMLActive && IsBondedWarehouseValidationMode &&
						Parent.Part != null && declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(Parent))
					{
						Parent.JI_InvoiceQuantityInfo.AddMessageError(InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
					}
				}
				else if (Parent.IsGoingIntoBondedWarehouse)
				{
					if (declaration.IsWarehousedByExternalAgent)
					{
						Parent.JI_InvoiceQuantityInfo.AddError(Res.GetString("67c09ab3-ad4f-428c-8408-90d1ab6067a2", "Please enter an invoice quantity for the bonded warehousing system."));
					}
					else if (declaration.IsWHSUniversalXMLActive && IsInwardBondedWarehousingEnabled(declaration))
					{
						if (Parent.Part != null && IsBondedWarehouseValidationMode)
						{
							Parent.JI_InvoiceQuantityInfo.AddMessageError(InvoiceQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
						}
					}
					else
					{
						Parent.JI_InvoiceQuantityInfo.AddWarning(Res.GetString("67c3879f-17de-4ba8-b3d1-76462cc04847", "Please enter an invoice quantity if you would like this line to be recorded in the bonded warehousing system."));
					}
				}
			}
		}

		public static string InvoiceQtyIsRequiredForWarehouse(string term)
		{
			return Res.GetString("8450E72E-B66C-4400-9430-486DE8142D5B", "Please enter an Invoice Quantity which is required for {0} integration.", term);
		}

		#endregion

		#region CheckJI_InvoiceUQ
		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ValidateJI_CustomsQuantity();
			if (Parent.JI_InvoiceUQ.IsEmpty)
			{
				var declaration = Parent.Declaration;
				if (declaration != null)
				{
					if (declaration.IsInvoiceQuantityRequiredForBondedWarehouse && Parent.SupportsBondedWarehousing)
					{
						if (IsOutwardBondedWarehousingEnabled(declaration))
						{
							if (declaration.IsWHSUniversalXMLActive && IsBondedWarehouseValidationMode &&
								Parent.Part != null && declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(Parent))
							{
								Parent.JI_InvoiceUQInfo.AddMessageError(InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
							}
						}
						else if (Parent.IsGoingIntoBondedWarehouse)
						{
							if (declaration.IsWarehousedByExternalAgent)
							{
								Parent.JI_InvoiceUQInfo.AddError(Res.GetString("05205c6a-62b6-471d-ba5f-0fe072eee710", "Please enter an invoice unit of quantity for the bonded warehousing system."));
							}
							else if (declaration.IsWHSUniversalXMLActive && IsInwardBondedWarehousingEnabled(declaration))
							{
								if (Parent.Part != null && IsBondedWarehouseValidationMode)
								{
									Parent.JI_InvoiceUQInfo.AddMessageError(InvoiceUQIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
								}
							}
							else
							{
								Parent.JI_InvoiceUQInfo.AddWarning(Res.GetString("9ad04b3e-fe4d-4073-abc0-22cb5202f0d9", "Please enter an invoice unit of quantity if you would like this line to be recorded in the bonded warehousing system."));
							}
						}
					}

					if (!Parent.JI_PartNo.IsEmpty && Parent.Part == null)
					{
						Parent.JI_InvoiceUQInfo.AddWarning(MandatoryForAutoCreateProduct);
					}
				}
			}
		}

		protected virtual bool IsBondedWarehouseValidationMode
		{
			get { return true; }
		}

		public static string InvoiceUQIsRequiredForWarehouse(string term)
		{
			return Res.GetString("ECC9C41D-AF3D-46D2-9CF9-39354C78C446", "Please enter an Invoice Unit of Quantity which is required for {0} integration.", term);
		}

		#endregion

		#region CheckJI_BondedWhsQuantity
		protected override void CheckJI_BondedWhsQuantity()
		{
			base.CheckJI_BondedWhsQuantity();

			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsWHSUniversalXMLActive && declaration.IsBondedWhsQuantityRequiredForBondedWarehouse && !Parent.IsBondedWarehousingDisabled && Parent.SupportsBondedWarehousing &&
				Parent.JI_BondedWhsQuantity <= 0m && Parent.ComponentInventoryCollection.Count == 0)
			{
				if (IsOutwardBondedWarehousingEnabled(declaration))
				{
					if (IsBondedWarehouseValidationMode &&
						Parent.Part != null && declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(Parent))
					{
						Parent.JI_BondedWhsQuantityInfo.AddMessageError(BondedWhsQuantityIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
					}
				}
				else if (Parent.IsGoingIntoBondedWarehouse && IsInwardBondedWarehousingEnabled(declaration) && Parent.Part != null && IsBondedWarehouseValidationMode)
				{
					Parent.JI_BondedWhsQuantityInfo.AddMessageError(BondedWhsQuantityIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				}
			}

			if (IsBondedWhsQuantityRequired && !Parent.JI_BondedWhsQuantityInfo.HasMessageErrors() && Parent.ComponentInventoryCollection.Count == 0)
			{
				var bondedWhsQuantity = Parent.JI_BondedWhsQuantity;

				if (bondedWhsQuantity.IsEmpty)
				{
					Parent.JI_BondedWhsQuantityInfo.AddMessageError(ResString.GetMultilingualString("D9367BAF-C59A-4994-ABDA-830333878AE7", "Please enter a Countable Quantity."));
				}

				var unitOfQty = Parent.JI_BondedWhsUnitQty;
				if (bondedWhsQuantity.IsEmpty && !unitOfQty.IsEmpty && !Parent.JI_BondedWhsQuantityInfo.HasMessageErrors())
				{
					Parent.JI_BondedWhsQuantityInfo.AddMessageError(ResString.GetMultilingualString("{96EEED7B-28D3-4536-919B-0250572CB2C5}", "Please enter a quantity in {0}.", unitOfQty));
				}
			}
		}

		protected virtual bool IsBondedWhsQuantityRequired => false;

		public static string BondedWhsQuantityIsRequiredForWarehouse(string term)
		{
			return Res.GetString("{35C4CDB8-AD3A-4228-B7AD-BD166EF321EA}", "Please enter a Countable Quantity which is required for {0} integration.", term);
		}

		#endregion

		#region CheckJI_BondedWhsUnitQty
		protected override void CheckJI_BondedWhsUnitQty()
		{
			base.CheckJI_BondedWhsUnitQty();

			var targetInfo = Parent.JI_BondedWhsUnitQtyInfo;
			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsWHSUniversalXMLActive && declaration.IsBondedWhsQuantityRequiredForBondedWarehouse && Parent.JI_BondedWhsUnitQty.IsEmpty && !Parent.IsBondedWarehousingDisabled && Parent.SupportsBondedWarehousing && Parent.ComponentInventoryCollection.Count == 0)
			{
				if (IsOutwardBondedWarehousingEnabled(declaration))
				{
					if (IsBondedWarehouseValidationMode &&
						Parent.Part != null && declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(Parent))
					{
						targetInfo.AddMessageError(BondedWhsUnitQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
					}
				}
				else if (Parent.IsGoingIntoBondedWarehouse && IsInwardBondedWarehousingEnabled(declaration) &&
					Parent.Part != null && IsBondedWarehouseValidationMode)
				{
					targetInfo.AddMessageError(BondedWhsUnitQtyIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
				}
			}
			if (IsBondedWhsQuantityRequired && !targetInfo.HasMessageErrors())
			{
				var sourceValue = Parent.JI_BondedWhsUnitQty;
				if (sourceValue.IsEmpty && !Parent.JI_BondedWhsQuantity.IsEmpty && Parent.ComponentInventoryCollection.Count == 0)
				{
					targetInfo.AddMessageError(UnitOfQuantityIsRequired);
				}
				else if (!sourceValue.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(targetInfo);
				}
			}
		}

		public static string UnitOfQuantityIsRequired
		{
			get { return ResString.GetMultilingualString("{19999055-A889-46BB-9CF3-18F314B9FBE0}", "You have not entered a unit of quantity."); }
		}

		public static string BondedWhsUnitQtyIsRequiredForWarehouse(string term)
		{
			return Res.GetString("{3BA5596A-8EFA-431B-8ABB-ED43437644EE}", "Please enter a Countable Unit of Quantity which is required for {0} integration.", term);
		}

		#endregion

		#region Custom Label Mandatory Validation

		protected override void CheckJI_CustomAttrib1()
		{
			base.CheckJI_CustomAttrib1();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomAttrib1Info);
		}

		protected override void CheckJI_CustomAttrib2()
		{
			base.CheckJI_CustomAttrib2();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomAttrib2Info);
		}

		protected override void CheckJI_CustomAttrib3()
		{
			base.CheckJI_CustomAttrib3();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomAttrib3Info);
		}

		protected override void CheckJI_CustomAttrib4()
		{
			base.CheckJI_CustomAttrib4();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomAttrib4Info);
		}

		protected override void CheckJI_CustomAttrib5()
		{
			base.CheckJI_CustomAttrib5();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomAttrib5Info);
		}

		protected override void CheckJI_CustomAttrib6()
		{
			base.CheckJI_CustomAttrib6();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomAttrib6Info);
		}

		protected override void CheckJI_CustomTextBlob1()
		{
			base.CheckJI_CustomTextBlob1();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomTextBlob1Info);
		}

		protected override void CheckJI_CustomDecimal1()
		{
			base.CheckJI_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomDecimal1Info);
		}

		protected override void CheckJI_CustomDecimal2()
		{
			base.CheckJI_CustomDecimal2();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomDecimal2Info);
		}

		protected override void CheckJI_CustomDecimal3()
		{
			base.CheckJI_CustomDecimal3();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomDecimal3Info);
		}

		protected override void CheckJI_CustomDate1()
		{
			base.CheckJI_CustomDate1();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomDate1Info);
		}

		protected override void CheckJI_CustomDate2()
		{
			base.CheckJI_CustomDate2();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomDate2Info);
		}

		protected override void CheckJI_CustomDate3()
		{
			base.CheckJI_CustomDate3();
			CustomLabelPropertyValidation.Validate(CustomLabelsProvider, Parent.JI_CustomDate3Info);
		}

		CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}

		CustomLabelPropertyValidation customLabelPropertyValidation;

		BaseJobComInvoiceLine.CustomLabelsProvider CustomLabelsProvider
		{
			get { return customLabelsProvider ?? (customLabelsProvider = new BaseJobComInvoiceLine.CustomLabelsProvider(Parent.Declaration)); }
		}
		BaseJobComInvoiceLine.CustomLabelsProvider customLabelsProvider;

		#endregion

		#region JI_LineNo
		protected override void CheckJI_LineNo()
		{
			base.CheckJI_AddInfo();
			if (Parent.JI_LineNo < 1)
			{
				Parent.JI_LineNoInfo.AddMessageError(Res.GetString("1d1c287e-a2ac-4666-b3b7-4596dbcdd539", "Please enter a valid Line Number; valid number should be greater than zero and no greater than 32767."));
			}
		}
		#endregion

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			var targetInfo = Parent.JI_ProcedureInfo;

			if (Parent.Lookups.Procedures.Count > 0)
			{
				if (IsJIProcedureMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(targetInfo, ProcedureCodeInvalid);
			}
			if (Parent.EntryInstruction != null && !Parent.AllowMyBrotherInvoiceLinesToBeOfMixedCategories && Parent.CusProcedure != null)
			{
				var mySeries = Parent.CusProcedure.ZZ6_Category;
				foreach (var brother in Parent.EntryInstruction.InvoiceLines.Where(b => b.PK != Parent.PK))  // Note - check only within CEI.  Brothers only.  Do not need to worry about cousins in other instructions.
				{
					var brotherProc = brother.CusProcedure;
					if (brotherProc != null && brotherProc.ZZ6_Category != mySeries)
					{
						targetInfo.AddMessageError(ResString.GetMultilingualString("12345678-0F33-4683-A70B-0953624F5C67", "Procedures of different {3} may not be mixed on the same entry instruction. \r\nThis line: {3} {0}. \r\nLine #{1}: {3} {2}.", mySeries, brother.JI_LineNo, brotherProc.ZZ6_Category, FriendlyNameForZz6Category));
					}
				}
			}
		}

		protected virtual bool IsJIProcedureMandatory => true;

		protected virtual IMultilingualString ProcedureCodeInvalid => ListValidation.InvalidCodeMessageError;

		protected virtual string FriendlyNameForZz6Category
		{
			get { return ResString.GetMultilingualString("87654321-0F33-4683-A70B-0953624F5C67", "category"); }
		}

		#region CheckJI_PartNo

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckJI_PartNo()
		{
			base.CheckJI_PartNo();
			ValidateJI_CustomsQuantity();
			ValidateJI_InvoiceUQ();
			ValidateJI_InvoiceQuantity();
			ValidateJI_Description();
			ValidateJI_CC();

			var part = Parent.Part;
			CheckBondedWarehousingForPart(part);
			CheckProductAuditForPart(part);
			CheckMatchPivotForPart(part);
		}

		void CheckBondedWarehousingForPart(OrgSupplierPart part)
		{
			var warehousingNotificationRequired = part is null &&
				!Parent.IsBondedWarehousingDisabled &&
				Parent.SupportsBondedWarehousing;

			if (!warehousingNotificationRequired || Declaration is null)
			{
				return;
			}

			if (IsOutwardBondedWarehousingEnabled(Declaration))
			{
				if (Declaration.IsWHSUniversalXMLActive && IsBondedWarehouseValidationMode && Declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(Parent))
				{
					Parent.JI_PartNoInfo.AddMessageError(ProductIsRequiredForWarehouse(Declaration.TermNameForBondedWarehouse));
				}
			}
			else if (Parent.IsGoingIntoBondedWarehouse)
			{
				if (Declaration.IsWarehousedByExternalAgent)
				{
					Parent.JI_PartNoInfo.AddError(Res.GetString("079ac5c3-7439-444f-a497-3956efc56de6", "Please choose or create a product so that this line can be recorded in the bonded warehousing system."));
				}
				else
				{
					if (Declaration.IsWHSUniversalXMLActive && IsPartNoRequiredForInwardBondedWarehousing(Declaration))
					{
						if (IsBondedWarehouseValidationMode)
						{
							Parent.JI_PartNoInfo.AddMessageError(ProductIsRequiredForWarehouse(Declaration.TermNameForBondedWarehouse));
						}
					}
					else if (!Declaration.IsWHSUniversalXMLActive || IsPartNoRequiredForInwardBondedWarehousing(Declaration))
					{
						Parent.JI_PartNoInfo.AddWarning(Res.GetString("e06217e2-8447-475d-a831-3e525a1adb0c", "Please choose or create a product if you would like this line to be recorded in the bonded warehousing system."));
					}
				}
			}
		}

		void CheckProductAuditForPart(OrgSupplierPart part)
		{
			if (Parent.JI_PartNo.IsEmpty || Parent.PartSyncManager is not { Enabled: true })
			{
				return;
			}

			InvoiceLineProductValidationHelper.ValidateProductCodeWhenPartSyncManagerEnabled(
				Parent.JI_PartNoInfo,
				part,
				Parent.Supplier_Effective,
				Parent.Importer_Effective,
				Parent.PartSyncManager,
				WarningPartCodeFoundButNotRelatedToSupplierImporterCombination,
				WarningPartCodesFoundButNotRelatedToSupplierImporterCombination,
				Parent.IsUNDGSupported);

			if (Parent.IsProductAuditedOrInvalid() || Parent.InvoiceHeader is not { } invoiceHeader)
			{
				return;
			}

			var auditAction = invoiceHeader.ProductAuditAction();
			if (auditAction == ProductAuditActions.Codes.AddWarningValidation)
			{
				Parent.JI_PartNoInfo.AddWarning(ProductHasNotBeenAudited);
			}
			else if (auditAction == ProductAuditActions.Codes.AddMessageErrorValidation)
			{
				Parent.JI_PartNoInfo.AddMessageError(ProductHasNotBeenAudited);
			}
		}

		void CheckMatchPivotForPart(OrgSupplierPart part)
		{
			if (part is null || Parent.Pivot is not null)
			{
				return;
			}

			var importer = Parent.Importer;
			var supplier = Parent.Supplier;

			if (Parent.IsImport)
			{
				var partAttrib1 = ZString.Empty;
				var partAttrib2 = ZString.Empty;
				var partAttrib3 = ZString.Empty;
				if (importer?.PartAttributeManager is { } partAttributeManager)
				{
					partAttrib1 = partAttributeManager.PartAttributeName1;
					partAttrib2 = partAttributeManager.PartAttributeName2;
					partAttrib3 = partAttributeManager.PartAttributeName3;
				}
				Parent.JI_PartNoInfo.AddWarning(CannotMatchClassificationForPart(part.OP_PartNum, importer?.OH_Code ?? ZString.Empty, supplier?.OH_Code ?? ZString.Empty, partAttrib1, Parent.JI_PartAttrib1, partAttrib2, Parent.JI_PartAttrib2, partAttrib3, Parent.JI_PartAttrib3, Parent.JI_SerialNumber, Parent.EffectiveDateForDutyRate));
			}
			else if (Parent.IsExport)
			{
				Parent.JI_PartNoInfo.AddWarning(CannotMatchClassificationForPart(part.OP_PartNum, importer?.OH_Code ?? ZString.Empty, supplier?.OH_Code ?? ZString.Empty, Parent.EffectiveDateForDutyRate));
			}
		}

		string CannotMatchClassificationForPart(string partNum, string importerCode, string supplierCode, string partAttrib1Name, string partAttrib1Value, string partAttrib2Name, string partAttrib2Value, string partAttrib3Name, string partAttrib3Value, string serialNumberValue, ZDateTime effectiveDate)
		{
			return Parent.HasMultiplePivotsMatchingProduct
				? Res.GetString("d3e0719c-3698-4fe3-b8a3-8d6757aec832", "Multiple classification matching for product ({0}) based on Importer ({1}), Supplier ({2}), {3} ({4}), {5} ({6}), {7} ({8}), Serial Number ({9}) and Effective Date ({10}).", partNum, importerCode, supplierCode, partAttrib1Name, partAttrib1Value, partAttrib2Name, partAttrib2Value, partAttrib3Name, partAttrib3Value, serialNumberValue, effectiveDate)
				: Res.GetString("abcde0bc-0b12-4212-87e3-820b6e0e2231", "Cannot match classification for product ({0}) based on Importer ({1}), Supplier ({2}), {3} ({4}), {5} ({6}), {7} ({8}), Serial Number ({9}) and Effective Date ({10}).", partNum, importerCode, supplierCode, partAttrib1Name, partAttrib1Value, partAttrib2Name, partAttrib2Value, partAttrib3Name, partAttrib3Value, serialNumberValue, effectiveDate);
		}

		string CannotMatchClassificationForPart(string partNum, string importerCode, string supplierCode, ZDateTime effectiveDate)
		{
			return Parent.HasMultiplePivotsMatchingProduct
				? Res.GetString("0ced5195-1785-4984-bc2a-15ba323d7d26", "Multiple classification matching for product ({0}) based on Importer ({1}), Supplier ({2}) and Effective Date ({3}).", partNum, importerCode, supplierCode, effectiveDate)
				: Res.GetString("{F0B302F7-4733-4576-9DAC-56F5CA7EA1B8}", "Cannot match classification for product ({0}) based on Importer ({1}), Supplier ({2}) and Effective Date ({3}).", partNum, importerCode, supplierCode, effectiveDate);
		}

		bool IsPartNoRequiredForInwardBondedWarehousing(BaseJobDeclaration declaration)
		{
			var result = false;
			if (declaration.SupportMultipleWarehouseEntry)
			{
				var entryInstruction = Parent.EntryInstruction;
				result = entryInstruction != null && (entryInstruction.IsChangeOfOwnershipWarehousing || entryInstruction.IsChangeOfRegimeWarehousing || (entryInstruction.IsIntoRegime && !entryInstruction.HasBothOutOfAndIntoRegimeProcedure));
			}
			else
			{
				result = declaration.IsInwardBondedWarehousingEnabled;
			}
			return result;
		}

		public static string ProductIsRequiredForWarehouse(string term)
		{
			return Res.GetString("45404B12-2146-4C0D-BA17-10147B6157C8", "Please enter a valid Product which is required for {0} integration.", term);
		}

		public static string WarningPartHasMultipleUNDGRecords
		{
			get => InvoiceLineProductValidationHelper.Warnings.PartHasMultipleUNDGRecords;
		}

		public static string WarningPartCannotBeFoundBeforeEnteringASupplierAndImporter
		{
			get => InvoiceLineProductValidationHelper.Warnings.PartCannotBeFoundBeforeEnteringASupplierAndImporter;
		}

		public static string WarningPartCodeFoundButNotRelatedToSupplierImporterCombination
		{
			get => Res.GetString("b9a0ba74-7a50-4845-995f-0784bf45d997", "A Product with this code exists but is inactive or the Supplier (Exporter)/Importer (Owner) relationship on that Product does not match this invoice. Either add a new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Supplier (Exporter)/Importer (Owner) relationship on the existing Product (F4 then edit a Product).");
		}

		public static string WarningPartCodesFoundButNotRelatedToSupplierImporterCombination
		{
			get => Res.GetString("7d1a4880-5fca-47b2-b2d6-92e6e72b249e", "Several Products with this code exist but are inactive or the Supplier (Exporter)/Importer (Owner) relationship on that Product does not match this invoice. Either add a new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Supplier (Exporter)/Importer (Owner) relationship on the existing Product (F4 then edit a Product).");
		}

		public static string WarningPartCodeNotFoundAtAll
		{
			get => InvoiceLineProductValidationHelper.Warnings.PartCodeNotFoundAtAll;
		}

		public static string WarningMoreThanOneProductMatchFound
		{
			get => InvoiceLineProductValidationHelper.Warnings.MoreThanOneProductMatchFound;
		}

		public static string ProductHasNotBeenAudited
		{
			get => Res.GetString("878d53e9-a623-4a40-b203-612e9a3f3ab4", "This Product has not been audited.");
		}

		#endregion

		#region CheckJI_Tariff

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			var targetInfo = Parent.JI_TariffInfo;
			var tariff = Parent.JI_Tariff;
			if (IsTariffMandatory && tariff.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("b59051d1-dfc6-4c15-a793-3c56253bb695", "Tariff may not be empty"));
			}

			MatchToProductValidation(Parent.JI_TariffInfo, () => Parent.Pivot?.CI_TariffNum ?? ZString.Empty);

			if (ClassificationIsRequiredForAutoCreationOfProduct && !Parent.JI_PartNo.IsEmpty && Parent.Part == null && Parent.JI_CC.IsEmpty && tariff.IsEmpty)
			{
				targetInfo.AddWarning(EitherCCOrTariffRequiredForAutoCreationOfProduct ? MandatoryCCOrTariffForAutoCreateProduct : MandatoryForAutoCreateProduct);
			}

			if (Parent.UseUniversalTariff)
			{
				if (!tariff.IsEmpty)
				{
					var cusTariff = Parent.UniversalTariff;
					if (cusTariff == null)
					{
						targetInfo.AddMessageError(UniversalTariffNotExistedError);
					}
				}

				if (Parent.IsImport && !Parent.EffectiveCountryOfOrigin.IsEmpty)
				{
					CheckApplicableRate();
				}
			}

			CheckApplicableConditionsForTariff(targetInfo);

			var tariffRuleMessageError = CustomsRuleHelper.ValidateWithTariffRule(tariff, Declaration);
			if (!tariffRuleMessageError.IsEmpty)
			{
				targetInfo.AddMessageError(tariffRuleMessageError);
			}
		}

		protected virtual ZString UniversalTariffNotExistedError => Res.GetString("7306ECF9-7621-4A44-B68E-9132838243FF", "The Tariff Code entered is not valid for the current context.");

		void CheckApplicableRate()
		{
			var tariff = Parent.JI_Tariff;
			var targetInfo = Parent.JI_TariffInfo;
			var universalTariff = Parent.UniversalTariff;
			if (universalTariff != null)
			{
				foreach (var criteria in RateSelectionCriteriaLists)
				{
					var rateTypeDescription = UniversalReferenceDataHelper.GetRateTypeDescription(Parent.Factory, criteria.DataGrouping, criteria.RateType);

					var applicableRates = universalTariff.GetApplicableRates(criteria);
					if (!applicableRates.Any())
					{
						var rateSelectionCriteriaInfos = UniversalReferenceDataHelper.GetRateSelectionCriteriaInfo(universalTariff, criteria);
						if (rateSelectionCriteriaInfos.Any())
						{
							int count = 0;
							var validCriteriaInfos = new ZStringBuilder();
							var split = rateSelectionCriteriaInfos.Split(x => x.Match(criteria));
							var allCriteriaInfos = (split.MatchingSet.Any() ? split.MatchingSet : split.NonMatchingSet)
								.Select(x => new { x.ZZS_Preference, x.ZZT_OrderNumber, x.ZZT_AdditionalCode, x.SecondTradeGroup })
								.Distinct()
								.OrderBy(x => x.ZZS_Preference)
								.ThenBy(x => x.ZZT_OrderNumber)
								.ThenBy(x => x.ZZT_AdditionalCode)
								.ThenBy(x => x.SecondTradeGroup);

							foreach (var criteriaInfo in allCriteriaInfos)
							{
								var strOrderNumber = criteriaInfo.ZZT_OrderNumber.IsEmpty ? ZString.Empty : new ZString(Res.GetString("FA7D6EE5-6E81-41D9-90B8-281C129CC61B", "{0} = {1}", OrderNumberCaption, criteriaInfo.ZZT_OrderNumber));
								var strAdditionalCode = criteriaInfo.ZZT_AdditionalCode.IsEmpty ? ZString.Empty : new ZString(Res.GetString("08EF9D57-F9F2-4186-AF1C-E3AAAD463DF2", "{0} = {1}", AdditionalCodeCaption, criteriaInfo.ZZT_AdditionalCode));
								var strPreference = criteriaInfo.ZZS_Preference.IsEmpty ? ZString.Empty : new ZString(Res.GetString("CD0B054B-27E3-4223-9E93-89319D72A912", "Preference = {0}", criteriaInfo.ZZS_Preference));
								var strSecondGroup = criteriaInfo.SecondTradeGroup.IsEmpty ? ZString.Empty : new ZString(Res.GetString("C5E9021A-4A46-44CB-8A2B-561BFE651196", "Application Territory = {0}", criteriaInfo.SecondTradeGroup));
								var joinedLine = string.Join(Res.GetString("BE2F6D5A-0E02-462C-A328-F2D057679636", " and "), new[] { strPreference, strOrderNumber, strAdditionalCode, strSecondGroup }.Where(x => !x.IsEmpty));
								validCriteriaInfos.AppendLine(Res.GetString("A40B3FC1-1733-4DDB-A097-2EFF0A390E49", "{0}: {1}", ++count, joinedLine));
							}
							targetInfo.AddNotification(NoApplicableRateNotificationSeverity, Res.GetString("1BED2721-A81B-4ADD-9E80-DF1F3BB50676", "There is no applicable {0} rate for the Tariff '{1}' and Country Of Origin '{2}' as at {3} in combination with other data entered on the form.", rateTypeDescription, tariff, criteria.TradeGroupCountry, criteria.EffectiveDate));
							if (ShouldValidateDutyRule)
							{
								targetInfo.AddNotification(ValidRatesNotificationSeverity, Res.GetString("5A4DB284-7B83-49FD-8704-AF34D4304281", "Valid {0} rates exist where\r\n{1}", rateTypeDescription, validCriteriaInfos.ToString()));
							}
						}
					}
					else
					{
						if (ShouldValidateUniversalTariffWithAmbiguousRateCode)
						{
							var groupsWithMoreThanOneRate = applicableRates.GroupBy(x => x.ZZ2_ZY1_RateCode).Where(x => x.Count() > 1);
							foreach (var group in groupsWithMoreThanOneRate)
							{
								targetInfo.AddMessageError(UniversalTariffWithAmbiguousRateCodeError(rateTypeDescription, group.First().RateCode, tariff, criteria));
							}
						}

						RunCountrySpecificRateValidation(targetInfo, criteria, applicableRates);
					}
				}
			}
		}

		protected virtual INotificationType ValidRatesNotificationSeverity => CargoWise.EntityFramework.NotificationType.MessageError;
		protected virtual INotificationType NoApplicableRateNotificationSeverity => CargoWise.EntityFramework.NotificationType.MessageError;

		protected virtual ZString UniversalTariffWithAmbiguousRateCodeError(ZString rateTypeDescription, ZString rateCode, ZString tariff, IZZRateSelectionCriteria criteria)
		{
			return Res.GetString("F214809C-1114-4699-A318-6A2AE93D55DB", "There is more than one applicable {0} rate with rate code {1} for the Tariff '{2}' and Country Of Origin '{3}' as at {4} in combination with other data entered on the form.", rateTypeDescription, rateCode, tariff, criteria.TradeGroupCountry, criteria.EffectiveDate);
		}

		protected virtual ZBool ShouldValidateUniversalTariffWithAmbiguousRateCode => true;

		protected virtual ZBool ShouldValidateDutyRule => true;

		protected virtual void RunCountrySpecificRateValidation(ZPropertyInfo targetInfo, IZZRateSelectionCriteria criteria, IEnumerable<RateView> applicableRates)
		{
		}

		protected virtual INotificationType NotificationTypeForConditionsCheck => CargoWise.EntityFramework.NotificationType.MessageError;

		void CheckApplicableConditionsForTariff(ZPropertyInfo targetInfo)
		{
			if (Parent?.Declaration != null && Parent.UseUniversalTariff && Parent.UseUniversalConditionCheck)
			{
				var (controlConditionCheck, informationConditionCheck) = Parent.UniversalTariff?.CheckConditionsAreMet(Parent.ConditionSelectionCriterias, Parent.EvaluateConditionValue, Parent.GetFriendlyConditionValue, Parent.CalcDataForConditionFormula) ?? (ZString.Empty, ZString.Empty);
				if (!controlConditionCheck.IsEmpty)
				{
					targetInfo.AddNotification(NotificationTypeForConditionsCheck, controlConditionCheck);
				}

				if (!informationConditionCheck.IsEmpty)
				{
					targetInfo.AddNotification(CargoWise.EntityFramework.NotificationType.Warning, informationConditionCheck);
				}
			}
		}

		protected virtual IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaLists => new List<IZZRateSelectionCriteria>() { Parent.DutyRateSelectionCriteria };

		protected virtual ZString OrderNumberCaption => Res.GetString("A5127705-7E07-4E49-B2E8-BF7934DCD4FF", "Order Number");

		protected virtual ZString AdditionalCodeCaption => Res.GetString("7AB5C148-99F8-480E-8198-F9803193E3E9", "Additional Code");
		#endregion

		#region IsTariffMandatory
		protected virtual bool IsTariffMandatory
		{
			get { return true; }
		}
		#endregion

		#region CheckJI_OrderNumber
		protected override void CheckJI_OrderNumber()
		{
			base.CheckJI_OrderNumber();

			ZString lineOrderNumber = Parent.JI_OrderNumber;
			if (!Parent.HasValidOrder)
			{
				OrderNumberValidation.ErrorIfOrderNumberNotValid(Parent.JI_OrderNumberInfo);

				if (!lineOrderNumber.IsEmpty)
				{
					bool found = false;
					BaseJobComInvoiceHeader invoiceHeader = Parent.InvoiceHeader;
					BaseJobDeclaration declaration = invoiceHeader == null ? null : invoiceHeader.JobDeclaration;
					if (declaration != null)
					{
						foreach (Order order in declaration.AttachedOrders)
						{
							if (order.JD_OrderNumberAndSplit == lineOrderNumber)
							{
								found = true;
							}
						}
						foreach (OrderItem item in declaration.DocsAndCartage.OrderItems)
						{
							if (item.JT_OrderReference == lineOrderNumber)
							{
								found = true;
							}
						}
						if (!found)
						{
							Parent.JI_OrderNumberInfo.AddWarning(Res.GetString("fb9d3e89-aa68-42de-930a-da01a02e89a9", "This order number is not defined on this declaration."));
						}
					}
				}
			}
		}
		#endregion

		#region ValidateUnitPrice
		public virtual void ValidateUnitPrice()
		{
			ValidateCalculatedProperty(Parent.UnitPriceInfo);
		}
		#endregion

		#region CheckUnitPrice
		protected virtual void CheckUnitPrice()
		{
			TypeValidation.CheckValidMoney(Parent.UnitPriceInfo, 19, 4);
		}
		#endregion

		#region ValidateJI_Calc_Invoice

		public virtual void ValidateJI_Calc_Invoice()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_InvoiceInfo);
		}

		#endregion

		#region CheckJI_Calc_Invoice

		protected virtual void CheckJI_Calc_Invoice()
		{
		}

		#endregion

		#region ValidateJI_Calc_FreightInInvoiceCurr

		public void ValidateJI_Calc_FreightInInvoiceCurr()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_FreightInInvoiceCurrInfo);
		}

		#endregion

		#region CheckJI_Calc_FreightInInvoiceCurr

		protected virtual void CheckJI_Calc_FreightInInvoiceCurr()
		{
		}

		#endregion

		#region ValidateJI_Calc_InsuranceInInvoiceCurr

		public void ValidateJI_Calc_InsuranceInInvoiceCurr()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_InsuranceInInvoiceCurrInfo);
		}

		#endregion

		#region CheckJI_Calc_InsuranceInInvoiceCurr

		protected virtual void CheckJI_Calc_InsuranceInInvoiceCurr()
		{
		}

		#endregion

		#region CheckJI_CustomsQuantity()

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (Parent.NeedsCustomsQuantity)
			{
				if (Parent.JI_CustomsQuantity == 0m && !Parent.JI_CustomsUnitQty.IsEmpty)
				{
					string messageError = "";
					if (Parent.UnitConverter.Convertible(Parent.JI_InvoiceUQ, Parent.JI_CustomsUnitQty))
					{
						messageError = Res.GetString("c283d9ad-f871-4501-94b9-8d3e0a697493", "Customs Qty should be greater than zero. Please enter an Invoice Qty and the value will be translated into a customs qty.");
					}
					else if (Parent.Part == null)
					{
						messageError = Res.GetString("EFEEA52D-9F35-446F-B5FC-A912277E1B2B", "Customs Qty should be greater than zero. Please enter a Customs Qty directly or enter an invoice UQ that can be convertible to Customs UQ '{0}'.", Parent.JI_CustomsUnitQty);
					}
					else if (!Parent.JI_InvoiceUQ.IsEmpty && !Parent.JI_InvoiceUQInfo.HasNotifications())
					{
						messageError = Res.GetString("5c17f7a0-4739-4ba4-b986-a6b0f76ca699", @"Customs Qty should be greater than zero. The invoice UQ '{0}' cannot be convertible to Customs UQ '{1}'. Please enter a Customs Qty directly or go to a main menu Config -> Customs Files -> Product and enter all convertible units from '{2}' to '{3}' in 'Unit Conversions' tab.", Parent.JI_InvoiceUQ, Parent.CustomsUQ, Parent.JI_InvoiceUQ, Parent.JI_CustomsUnitQty);
					}
					else
					{
						messageError = Res.GetString("c3b7ed2f-30e2-4625-85b2-9377d98ac212", "Customs Qty should be greater than zero.");
					}
					if (!string.IsNullOrEmpty(messageError))
					{
						Parent.JI_CustomsQuantityInfo.AddMessageError(messageError);
					}
				}
			}
			else if (Parent.JI_CustomsQuantity > 0 && Parent.JI_CustomsUnitQty.IsEmpty)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(NoCustomsQuantityRequired);
			}
		}

		protected virtual ZString NoCustomsQuantityRequired
		{
			get { return Res.GetString("85db4e03-4bb2-4f53-ae7e-96f93489a812", "There is no customs unit associated with the tariff."); }
		}

		#endregion

		#region CheckJI_WeightUQ

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			ValidateJI_NetWeight();
			if (Parent.JI_WeightUQ.IsEmpty && Parent.JI_Weight > 0)
			{
				MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.JI_WeightUQInfo, Parent.JI_WeightInfo);
			}
		}

		#endregion

		#region CheckJI_Weight

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			ValidateJI_NetWeight();
		}

		#endregion

		#region CheckJI_NetWeight

		public static string WarningNetWeightIsGreaterThanGrossWeight
		{
			get
			{
				return Res.GetString("B91A1CC7-B44A-4096-96CC-0357133B8B33", "Net Weight should be less than Gross Weight.");
			}
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			bool needCheckGrossAndNetWeight = !Parent.JI_NetWeight.IsEmpty && !Parent.JI_NetWeightUQ.IsEmpty && !Parent.JI_Weight.IsEmpty && !Parent.JI_WeightUQ.IsEmpty
				&& Core.Constants.Weight.ContainsCode(Parent.JI_WeightUQ) && Core.Constants.Weight.ContainsCode(Parent.JI_NetWeightUQ);
			if (needCheckGrossAndNetWeight)
			{
				var netWeight = new ZWeight(Parent.JI_NetWeight, Parent.JI_NetWeightUQ);
				var grossWeight = new ZWeight(Parent.JI_Weight, Parent.JI_WeightUQ);
				if (grossWeight.IsValid && netWeight.IsValid && grossWeight < netWeight)
				{
					Parent.JI_NetWeightInfo.AddWarning(WarningNetWeightIsGreaterThanGrossWeight);
				}
			}
		}

		#endregion

		#region CheckJI_NetWeightUQ

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();
			ValidateJI_NetWeight();
			if (Parent.JI_NetWeightUQ.IsEmpty && Parent.JI_NetWeight > 0)
			{
				MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.JI_NetWeightUQInfo, Parent.JI_NetWeightInfo);
			}
		}

		#endregion

		#region CheckJI_VolumeUQ()

		protected override void CheckJI_VolumeUQ()
		{
			base.CheckJI_VolumeUQ();
			ListValidation.ErrorIfInvalidCode(Parent.JI_VolumeUQInfo, Parent.Lookups.VolumeUQList);

			if (Parent.JI_VolumeUQ.IsEmpty && Parent.JI_Volume > 0)
			{
				MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.JI_VolumeUQInfo, Parent.JI_VolumeInfo);
			}
		}

		#endregion

		#region CheckJI_ContainerMode

		protected override void CheckJI_ContainerMode()
		{
			base.CheckJI_ContainerMode();
			if (!Parent.JI_ContainerMode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JI_ContainerModeInfo, Parent.Lookups.ContainerModeList);
			}

			if (!Parent.JI_ContainerModeInfo.HasErrors() && Parent.IsContainerLinkMandatory)
			{
				if (Parent.IsContainerisedMode && Parent.IsNonContainerised)
				{
					Parent.JI_ContainerModeInfo.AddNotification(Parent.Declaration?.ContainerNotLinkedSeverity ?? CargoWise.ComponentModel.NotificationType.Warning, InvoiceLineNotLinkedToContainerMessage);
				}
				else if (!Parent.IsContainerisedMode && !Parent.IsNonContainerised)
				{
					Parent.JI_ContainerModeInfo.AddWarning(Res.GetString("24A46132-14E5-4443-A361-26560B08B737", "Invoice Line is not in containerized mode but is linked to a container"));
				}
			}
		}

		protected virtual string InvoiceLineNotLinkedToContainerMessage => Res.GetString("82164847-9DD5-4361-BF16-AA54D95169EF", "Invoice Line is in containerized mode but is not linked to a container, a container can be associated to all invoice lines from the context menu on the Containers grid on the container sub tab");

		#endregion

		#region CheckJI_RH_NKCommodity_Code

		protected override void CheckJI_RH_NKCommodity_Code()
		{
			base.CheckJI_RH_NKCommodity_Code();
			ListValidation.ErrorIfInvalidCode(Parent.JI_RH_NKCommodity_CodeInfo);
		}

		#endregion

		#region CheckJI_MatchingKey

		protected override void CheckJI_MatchingKey()
		{
			base.CheckJI_MatchingKey();

			var parent = Parent;
			var matchingKey = parent.JI_MatchingKey;

			if (!matchingKey.IsEmpty)
			{
				var invoiceHeader = parent.InvoiceHeader;

				if (invoiceHeader != null)
				{
					var query = new ZQuery();
					query.AddToFilter(JobComInvoiceLineSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
					query.AddToFilter(JobComInvoiceLineSchema.JI_MatchingKey, parent.JI_MatchingKey);

					var duplicateInvoiceLines = invoiceHeader.JobComInvoiceLines.Find(query);
					if (duplicateInvoiceLines.Any())
					{
						parent.JI_MatchingKeyInfo.AddError(Res.GetString("9e2c0fb5-85f7-4d20-b1ed-f02d1c34fae1", "Duplicate matching key is entered on other line which is also associated with the same invoice header."));
					}
				}
			}
		}

		#endregion

		#region CheckJI_ZZF_NKTaxType

		protected override void CheckJI_ZZF_NKTaxType()
		{
			base.CheckJI_ZZF_NKTaxType();
			TaxTypeListValidationCore();

			var parent = Parent;
			if (ShouldWarnVAT && !parent.JI_ZZF_NKTaxType.IsEmpty)
			{
				var message = VATWarningMessageWhenTaxTypeIsNotEmpty;
				parent.JI_ZZF_NKTaxTypeInfo.AddWarning(message);
			}
		}

		protected virtual void TaxTypeListValidationCore()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ZZF_NKTaxTypeInfo);
		}

		protected virtual string VATWarningMessageWhenTaxTypeIsNotEmpty
		{
			get
			{
				var parent = Parent;
				return Res.GetString("33047D9A-1928-4E4E-BDCD-3006332983D1",
					"Procedure {0} indicates that VAT does not apply, but value {1} in this field means that VAT is calculated; To disable the calculation set this field's value to blank.",
					parent.ProcedureIndicatesVATNotApply,
					parent.JI_ZZF_NKTaxType);
			}
		}

		protected virtual bool ShouldWarnVAT => Parent.ShouldWipeNKTaxType;

		#endregion

		#region MatchToProductCodeValidation

		public virtual void MatchToProductValidation(ZPropertyInfo propertyInfo, Func<IZType> getPivotValue)
		{
			var invoiceLineValue = propertyInfo.Value;
			var pivotValue = getPivotValue();
			if (!pivotValue.IsDefault && !invoiceLineValue.Equals(pivotValue))
			{
				var msg = $"{propertyInfo.HumanReadableName}" + DoesNotMatchProductCodeFile;
				var notificationType = CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.Value;
				if (notificationType == ProductAuditActions.Codes.AddMessageErrorValidation)
				{
					propertyInfo.AddMessageError(msg);
				}
				else if (notificationType == ProductAuditActions.Codes.AddWarningValidation)
				{
					propertyInfo.AddWarning(msg);
				}
			}
		}

		protected virtual ZString DoesNotMatchProductCodeFile => Res.GetString("405A3E79-8AA4-4E38-A5A0-52C41B0B7D15", " does not match product code file.");

		#endregion

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();

			if (Parent.UseUniversalTariff)
			{
				ValidateJI_Tariff();
			}
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();

			if (Parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_PrimaryPreferenceInfo);
				ValidateJI_Tariff();
			}
		}

		protected override void CheckJI_AddInfoIsWesternEuropean()
		{
			if (!(Parent is INAddInfoSupporter))
			{
				base.CheckJI_AddInfoIsWesternEuropean();
			}
		}

		protected override void CheckJI_ConcessionOrder()
		{
			base.CheckJI_ConcessionOrder();
			if (Parent.UseUniversalTariff)
			{
				ValidateJI_Tariff();
			}
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();
			MandatoryValidation.CheckNotNegative(Parent.JI_CustomsThirdQuantityInfo);
		}

		protected override void CheckJI_CustomsFourthQuantity()
		{
			base.CheckJI_CustomsFourthQuantity();
			MandatoryValidation.CheckNotNegative(Parent.JI_CustomsFourthQuantityInfo);
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			MandatoryValidation.CheckNotNegative(Parent.JI_CustomsSecondQuantityInfo);
		}

		protected override void CheckJI_CustomsFifthQuantity()
		{
			base.CheckJI_CustomsFifthQuantity();
			MandatoryValidation.CheckNotNegative(Parent.JI_CustomsFifthQuantityInfo);
		}

		protected override void CheckJI_CEI()
		{
			base.CheckJI_CEI();
			if (Parent.JI_CEI.IsEmpty && Declaration is BaseJobDeclaration declaration && declaration.IsEntryInstructionRequired)
			{
				Parent.JI_CEIInfo.AddMessageError(Res.GetString("B9E49183-3BF8-4350-8B6A-CEF86DFEF547", "Entry Instruction should be selected on an Invoice Line"));
			}
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			if (!Parent.JI_PreviousEntryLineNumber.IsEmpty && Parent.ComponentInventoryCollection.Count > 0 && !Parent.JI_PreviousEntryLineNumberInfo.ReadOnly)
			{
				var messageError = Res.GetString("7e17896c-66de-493f-8d9b-dd7f5482f1ef", "Either an Allocation Key should be provided (in the case of processed goods) or a reference to the Previous Entry Line Number should be provided but not both");
				Parent.JI_PreviousEntryLineNumberInfo.AddMessageError(messageError);
			}
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			if (!Parent.JI_PreviousEntryNumber.IsEmpty && Parent.ComponentInventoryCollection.Count > 0 && !Parent.JI_PreviousEntryNumberInfo.ReadOnly)
			{
				var messageError = Res.GetString("57dcc4e2-4702-4b1b-9c52-1059f8c100f0", "Either an Allocation Key should be provided (in the case of processed goods) or a reference to the Previous Entry Number should be provided but not both");
				Parent.JI_PreviousEntryNumberInfo.AddMessageError(messageError);
			}
		}

		protected override void CheckJI_BondedWHSOrderLineNumber()
		{
			base.CheckJI_BondedWHSOrderLineNumber();
			if (Parent.IsWarehouseOrderEnabled)
			{
				var info = Parent.JI_BondedWHSOrderLineNumberInfo;
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(info, Parent.JI_BondedWHSOrderNumberInfo);

				var orderLineNumber = Parent.JI_BondedWHSOrderLineNumber;
				var orderNumber = Parent.JI_BondedWHSOrderNumber;
				var factory = Parent.Factory;

				if (!orderLineNumber.IsEmpty && !orderNumber.IsEmpty)
				{
					var docket = GetCachedWhsOrderPK(factory, orderNumber);

					if (!docket.IsEmpty)
					{
						var listOfAvailableWhsLineNumber = GetCachedListAvailableWhsLineNumber(factory, docket);

						if (!listOfAvailableWhsLineNumber.Contains(orderLineNumber))
						{
							info.AddMessageError(Res.GetString("25EF6289-5BBA-40AD-A212-5F710987F539", "Please enter a valid {0}; There is no matching Order Line No '{1}' on Warehouse Order '{2}'.", info.Description, orderLineNumber, orderNumber));
						}
					}
				}
			}
			ValidateJI_BondedWHSOrderNumber();
		}

		protected override void CheckJI_BondedWHSOrderNumber()
		{
			base.CheckJI_BondedWHSOrderNumber();

			if (Parent.IsWarehouseOrderEnabled)
			{
				var info = Parent.JI_BondedWHSOrderNumberInfo;
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(info, Parent.JI_BondedWHSOrderLineNumberInfo);
				var orderNumber = Parent.JI_BondedWHSOrderNumber;
				if (!orderNumber.IsEmpty && GetCachedWhsOrderPK(Parent.Factory, orderNumber).IsEmpty)
				{
					info.AddMessageError(Res.GetString("522A9A9D-DEAC-4BEF-8EE5-F7A56AD6D9F2", "Please enter a valid {0}; There is no Warehouse Order with Docket ID equals to '{1}'.", info.Description, orderNumber));
				}
			}

			ValidateJI_BondedWHSOrderLineNumber();
		}

		ZGuid GetCachedWhsOrderPK(BusinessObjectFactory factory, string orderId)
		{
			return factory.GetCachedValue("WhsDocket_Validation_" + orderId, () =>
			{
				var orderQuery = new ZQuery(WhsDocketSchema.WD_DocketID, orderId);
				orderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				return factory.LoadTop1<IWhsDocket>(orderQuery)?.PK ?? ZGuid.Empty;
			});
		}

		HashSet<ZShort> GetCachedListAvailableWhsLineNumber(BusinessObjectFactory factory, ZGuid orderPk)
		{
			return factory.GetCachedValue("WhsDocketLine_Validation_" + orderPk, () =>
			{
				var orderLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, orderPk);
				orderLineQuery.AddToFilter(WhsDocketLineSchema.WE_LineNo, SQLComparisonOperator.GreaterThan, new ZShort(0));
				var lines = factory.Load<IWhsDocketLine>(orderLineQuery);

				return lines.Select(x => x.WE_LineNo).ToHashSet();
			});
		}
	}
}
