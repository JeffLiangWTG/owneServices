using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceLineValidation
	{
		protected override void CheckJI_Compositions()
		{
			base.CheckJI_Compositions();
			var line = InvoiceLine;
			if (line.HasRegulationsCode581Or541 || line.IsForCMHeaderMessageTypeNX601)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CompositionsInfo);
			}
		}

		protected override void CheckJI_NewOwnerPartNo()
		{
			base.CheckJI_NewOwnerPartNo();
			var invoiceLine = InvoiceLine;
			if (Owner != null && !Parent.JI_NewOwnerPartNo.IsEmpty && invoiceLine.NewOwnerProductSyncManager.Enabled)
			{
				var part = invoiceLine.NewOwnerProduct;

				if (part == null)
				{
					if (invoiceLine.NewOwnerProductSyncManager.TotalNumberOfPartsCount == 1)
					{
						Parent.JI_NewOwnerPartNoInfo.AddWarning(ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
					}
					else if (invoiceLine.NewOwnerProductSyncManager.TotalNumberOfPartsCount > 1)
					{
						Parent.JI_NewOwnerPartNoInfo.AddWarning(ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
					}
					else
					{
						Parent.JI_NewOwnerPartNoInfo.AddWarning(JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
					}
				}
				else
				{
					if (invoiceLine.NewOwnerProductSyncManager.TotalMatchCount > 1)
					{
						Parent.JI_NewOwnerPartNoInfo.AddWarning(JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
					}
				}
			}
			ValidateJI_NewPartAttribute1();
			ValidateJI_NewPartAttribute2();
			ValidateJI_NewPartAttribute3();
		}

		protected override void CheckJI_NewPartAttribute1()
		{
			base.CheckJI_NewPartAttribute1();
			CheckPartAttribute(Parent.JI_NewPartAttribute1Info, 1);
		}

		protected override void CheckJI_NewPartAttribute2()
		{
			base.CheckJI_NewPartAttribute2();
			CheckPartAttribute(Parent.JI_NewPartAttribute2Info, 2);
		}

		protected override void CheckJI_NewPartAttribute3()
		{
			base.CheckJI_NewPartAttribute3();
			CheckPartAttribute(Parent.JI_NewPartAttribute3Info, 3);
		}

		protected override void CheckJI_NewSerialNumber()
		{
			base.CheckJI_NewSerialNumber();
			CheckSerialNumber();
		}

		void CheckSerialNumber()
		{
			var part = InvoiceLine.NewOwnerProduct;
			if (part != null)
			{
				var owner = Owner;
				if (owner != null)
				{
					PartAttributeValidation.CheckSerialNumber(owner, part, Parent.JI_NewSerialNumberInfo);
				}
			}
		}

		protected override void CheckJI_CustomsSupplierPartNo()
		{
			base.CheckJI_CustomsSupplierPartNo();
			if (Parent.JI_CustomsSupplierPartNo.IsEmpty)
			{
				if (IsCustomsSupplierPartNoRequired)
				{
					Parent.JI_CustomsSupplierPartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.CustomsSupplierPartNoShouldNotBeEmpty);
				}
				else if (IsCustomsSupplierPartNoRequiredForFTZ)
				{
					Parent.JI_CustomsSupplierPartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ);
				}
			}
		}

		protected override void CheckJI_CustomsOwnerPartNo()
		{
			base.CheckJI_CustomsOwnerPartNo();
			if (Parent.JI_CustomsOwnerPartNo.IsEmpty)
			{
				if (IsCustomsOwnerPartNoRequiredForDeclarationType)
				{
					Parent.JI_CustomsOwnerPartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForDeclarationType);
				}
				else if (IsCustomsOwnerPartNoRequiredForProcedure)
				{
					Parent.JI_CustomsOwnerPartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.CustomsOwnerPartNoShouldNotBeEmptyForProcedure);
				}
				else if (IsCustomsOwnerPartNoRequiredForFTZ)
				{
					Parent.JI_CustomsOwnerPartNoInfo.AddMessageError(ValidationConstants.InvoiceLine.CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ);
				}
			}
		}

		protected override void CheckJI_InnerPackType()
		{
			base.CheckJI_InnerPackType();
			var invoiceLine = InvoiceLine;
			var targetInfo = Parent.JI_InnerPackTypeInfo;
			if (invoiceLine.IsForCAHeaderDN || invoiceLine.IsForCAHeaderIF || invoiceLine.IsForCAHeaderDH)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			if (ValidationHelper.IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(invoiceLine, ControllingMessageTypeList.Codes.NX301_DN, i => i.JI_InnerPackType))
			{
				targetInfo.AddWarning(Res.GetString("F5E0174B-98EF-4E11-82A0-0333AEF5AD18", "Invoice lines of different Packaging Type cannot be linked to the same NX301_DN message."));
			}
		}

		protected override void CheckJI_InnerPackingMaterial()
		{
			base.CheckJI_InnerPackingMaterial();
			var invoiceLine = InvoiceLine;
			var targetInfo = Parent.JI_InnerPackingMaterialInfo;
			if (invoiceLine.IsForCAHeaderDN || invoiceLine.IsForCAHeaderIF || invoiceLine.IsForCAHeaderDH)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			if (ValidationHelper.IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(invoiceLine, ControllingMessageTypeList.Codes.NX301_DN, i => i.JI_InnerPackingMaterial))
			{
				targetInfo.AddWarning(Res.GetString("E0050503-4D23-401A-990D-A45C517F91CD", "Invoice lines of different Packaging Material cannot be linked to the same NX301_DN message."));
			}
		}

		void CheckPartAttribute(ZPropertyInfo info, int attribNumber)
		{
			var part = InvoiceLine.NewOwnerProduct;
			if (part != null)
			{
				var owner = Owner;
				if (owner != null)
				{
					PartAttributeValidation.CheckAttribute(owner, part, info, attribNumber);
				}
			}
		}

		protected JobComInvoiceLine InvoiceLine => Parent;

		CusEntryInstruction EntryInstruction => InvoiceLine.EntryInstruction;

		ZString DeclarationType => EntryInstruction?.CEI_Style ?? ZString.Empty;

		OrgHeader Owner => EntryInstruction?.Owner;

		ZBool IsCustomsSupplierPartNoRequired
		{
			get
			{
				var declarationType = DeclarationType;
				return Parent.JI_BondedGoodsCode != BondedGoodsCodeList.Codes.NB
					&& (declarationType == Constants.DeclarationTypes.Export.B2
					|| declarationType == Constants.DeclarationTypes.Export.B8
					|| declarationType == Constants.DeclarationTypes.Export.B9
					|| declarationType == Constants.DeclarationTypes.Import.D2
					|| declarationType == Constants.DeclarationTypes.Export.D5
					|| declarationType == Constants.DeclarationTypes.Import.D7
					|| declarationType == Constants.DeclarationTypes.Import.F2
					|| declarationType == Constants.DeclarationTypes.Import.F3
					|| declarationType == Constants.DeclarationTypes.Export.F5
					|| declarationType == Constants.DeclarationTypes.Import.G2);
			}
		}

		ZBool IsCustomsSupplierPartNoRequiredForFTZ
		{
			get
			{
				var declarationType = DeclarationType;
				return (declarationType == Constants.DeclarationTypes.Import.D8
					|| declarationType == Constants.DeclarationTypes.Import.B6
					|| declarationType == Constants.DeclarationTypes.Export.F4)
					&& InvoiceLine.Declaration.SupplierDocumentaryAddress.IsFreeTradeZone;
			}
		}

		ZBool IsCustomsOwnerPartNoRequiredForDeclarationType
		{
			get
			{
				var declarationType = DeclarationType;
				return declarationType == Constants.DeclarationTypes.Export.B1
					|| declarationType == Constants.DeclarationTypes.Export.B2
					|| declarationType == Constants.DeclarationTypes.Export.D1
					|| declarationType == Constants.DeclarationTypes.Import.D7
					|| declarationType == Constants.DeclarationTypes.Import.D8
					|| declarationType == Constants.DeclarationTypes.Import.F1
					|| declarationType == Constants.DeclarationTypes.Import.F3;
			}
		}

		bool IsCustomsOwnerPartNoRequiredForProcedure
		{
			get
			{
				var procedure = InvoiceLine?.JI_Procedure ?? ZString.Empty;
				return procedure == Constants.ProcedureCodes._56
					|| procedure == Constants.ProcedureCodes._5Y
					|| procedure == Constants.ProcedureCodes._58
					|| procedure == Constants.ProcedureCodes._5C
					|| procedure == Constants.ProcedureCodes._98
					|| procedure == Constants.ProcedureCodes._99;
			}
		}

		ZBool IsCustomsOwnerPartNoRequiredForFTZ
		{
			get
			{
				var declarationType = DeclarationType;
				return (declarationType == Constants.DeclarationTypes.Export.D5
					|| declarationType == Constants.DeclarationTypes.Export.B8
					|| declarationType == Constants.DeclarationTypes.Export.B9
					|| declarationType == Constants.DeclarationTypes.Export.F4)
					&& InvoiceLine.Declaration.ImporterDocumentaryAddress.IsFreeTradeZone;
			}
		}

		protected override void CheckJI_BottledDate()
		{
			CheckNotEnteredWhenAlcoholLessThan7(InvoiceLine.JI_BottledDateInfo, InvoiceLine.JI_ExpirationDate.IsEmpty);
		}

		protected override void CheckJI_ExpirationDate()
		{
			CheckNotEnteredWhenAlcoholLessThan7(InvoiceLine.JI_ExpirationDateInfo, InvoiceLine.JI_BottledDate.IsEmpty);
		}

		protected override void CheckJI_AlcoholEndOfShelfLife()
		{
			base.CheckJI_AlcoholEndOfShelfLife();
			var line = InvoiceLine;
			var propertyInfo = line.JI_AlcoholEndOfShelfLifeInfo;
			if (!propertyInfo.ReadOnly && !line.JI_BottledDate.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		protected override void CheckJI_EPTDigit1()
		{
			base.CheckJI_EPTDigit1();
			CheckAssignedJobComInvLineRefs();
		}

		protected override void CheckJI_EPTDigit2()
		{
			base.CheckJI_EPTDigit2();
			CheckAssignedJobComInvLineRefs();
		}

		protected override void CheckJI_EPTDigit3()
		{
			base.CheckJI_EPTDigit3();
			CheckAssignedJobComInvLineRefs();
		}

		void CheckNotEnteredWhenAlcoholLessThan7(ZPropertyInfo propertyInfo, bool otherDateIsEmpty)
		{
			var alcoholPercentage = InvoiceLine.JI_AlcoholPercentage;
			if (otherDateIsEmpty && !propertyInfo.ReadOnly && !alcoholPercentage.IsEmpty && alcoholPercentage < 7M)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
		}

		protected override void CheckJI_TextileWidth()
		{
			base.CheckJI_TextileWidth();
			if (Parent.JI_TextileWidth.IsEmpty)
			{
				if (!Parent.JI_TextileWidthUQ.IsEmpty)
				{
					Parent.JI_TextileWidthInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.JI_TextileWidthInfo.HumanReadableName));
				}
			}
			else if (InvoiceLine.UniversalTariff is TariffView tariff && tariff.GetSpecificUOM(UnitOfMeasureTypes.AdditionalUOMType) != Constants.CustomsUnitOfMeasureList.SquareMetre)
			{
				Parent.JI_TextileWidthInfo.AddMessageError(ValidationConstants.InvoiceLine.TextileWidthIsNotRequired);
			}
		}

		protected override void CheckJI_TextileWidthUQ()
		{
			base.CheckJI_TextileWidthUQ();
			if (Parent.JI_TextileWidthUQ.IsEmpty)
			{
				if (Parent.JI_TextileWidth > ZDecimal.Zero)
				{
					Parent.JI_TextileWidthUQInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.JI_TextileWidthUQInfo.HumanReadableName));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_TextileWidthUQInfo, Parent.Lookups.TextileWidthUQList);
			}
		}

		protected override void CheckJI_MicrochipID()
		{
			base.CheckJI_MicrochipID();
			var microchipID = Parent.JI_MicrochipID;
			if (!microchipID.IsEmpty && !microchipID.IsLettersAndNumbersOnlyOrEmpty)
			{
				var targetInfo = Parent.JI_MicrochipIDInfo;
				targetInfo.AddMessageError(ValidationConstants.InvoiceLine.InvalidValue(targetInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_AnimalMaleQty()
		{
			base.CheckJI_AnimalMaleQty();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_AnimalMaleQtyInfo, Parent.JI_AnimalMaleQty, 0, 999999);
		}

		protected override void CheckJI_AnimalFemaleQty()
		{
			base.CheckJI_AnimalFemaleQty();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_AnimalFemaleQtyInfo, Parent.JI_AnimalFemaleQty, 0, 999999);
		}

		protected override void CheckJI_AnimalAgeMonth()
		{
			base.CheckJI_AnimalAgeMonth();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_AnimalAgeMonthInfo, Parent.JI_AnimalAgeMonth, 0, 12);
		}

		protected override void CheckJI_AnimalAgeYear()
		{
			base.CheckJI_AnimalAgeYear();
			CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(Parent.JI_AnimalAgeYearInfo, Parent.JI_AnimalAgeYear, 0, 9999);
		}

		protected void CheckTheNumberBetweenMinValueAndMaxValueIfNeeded(ZPropertyInfo info, ZInt value, ZInt minValue, ZInt maxValue)
		{
			if (value < minValue || value > maxValue)
			{
				info.AddMessageError(ValidationConstants.InvoiceLine.InvalidValue(info.HumanReadableName));
			}
		}

		protected override void CheckJI_AlcoholPercentage()
		{
			base.CheckJI_AlcoholPercentage();
			if (Parent.JI_AlcoholPercentage == 0 && InvoiceLine.Taxes.Cast<JobComInvoiceLineTax>().Any(x => x.FormattedTariffRate.Contains(TWSpecificRateParameterList.Codes.AlcoholPercentage, System.StringComparison.OrdinalIgnoreCase)))
			{
				Parent.JI_AlcoholPercentageInfo.AddMessageError(Res.GetString("349F11F7-2FCF-4BA7-B557-19F254403CBD", "You have selected an alcohol tax rate that requires alcohol by volume information."));
			}
		}

		protected override void CheckJI_GoodsType()
		{
			base.CheckJI_GoodsType();
			var targetInfo = Parent.JI_GoodsTypeInfo;
			if (InvoiceLine.IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX601))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			if (ValidationHelper.IsDifferentInvoiceLineValueLinkedToSameControllingMsgHeader(InvoiceLine, ControllingMessageTypeList.Codes.NX301_DN, i => i.JI_GoodsType))
			{
				targetInfo.AddWarning(Res.GetString("91D336E4-63C0-41F5-A3C7-E25A43071F5F", "Invoice lines of different Goods Type cannot be linked to the same NX301_DN message."));
			}
		}

		protected override void CheckJI_Group()
		{
			base.CheckJI_Group();
			ValidationHelper.CheckGroupingPlusDeclarationGoodsDescriptionLength(InvoiceLine, Parent.JI_GroupInfo);
		}

		protected override void CheckJI_PermitQty()
		{
			base.CheckJI_PermitQty();
			if (InvoiceLine.IsForCMHeaderMessageTypeNX101)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JI_PermitQtyInfo);
			}
		}

		protected override void CheckJI_PermitUQ()
		{
			base.CheckJI_PermitUQ();

			var parent = Parent;
			if (InvoiceLine.IsForCMHeaderMessageTypeNX101 && parent.JI_PermitUQ.IsEmpty)
			{
				parent.JI_PermitUQInfo.AddWarning(Res.GetString("631D0C57-2E45-4839-A1E1-FDC9A11BE8D3", "You have not selected a Permit Quantity UQ."));
			}
		}

		protected override void CheckJI_CustomPermitUQ()
		{
			base.CheckJI_CustomPermitUQ();

			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsForCMHeaderMessageTypeNX101 && !invoiceLine.IsForCMHeaderMessageTypeNX101CertificateType15)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JI_CustomPermitUQInfo);
			}
		}

		protected override void CheckJI_OriginCriteria()
		{
			base.CheckJI_OriginCriteria();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsForCMHeaderMessageTypeNX101)
			{
				var info = Parent.JI_OriginCriteriaInfo;
				if (Parent.JI_OriginCriteria.IsEmpty)
				{
					var certificateTypes = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Where(x => x.IsLinkedCMHeader).Select(x => x.CertificateType);
					if (certificateTypes.Contains(CertificateTypeList.Codes.Code1) || certificateTypes.Contains(CertificateTypeList.Codes.Code7) || certificateTypes.Contains(CertificateTypeList.Codes.Code10))
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(info);
				}
			}
		}

		protected override void CheckJI_PTCriteria()
		{
			base.CheckJI_PTCriteria();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsForCMHeaderMessageTypeNX101)
			{
				var info = invoiceLine.JI_PTCriteriaInfo;
				if (invoiceLine.JI_PTCriteria.IsEmpty)
				{
					if (invoiceLine.IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code9, CertificateTypeList.Codes.Code11, CertificateTypeList.Codes.Code13, CertificateTypeList.Codes.Code14, CertificateTypeList.Codes.Code15, CertificateTypeList.Codes.Code19))
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(info);
				}
			}
		}

		protected override void CheckJI_PTCriteria2()
		{
			base.CheckJI_PTCriteria2();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsForCMHeaderMessageTypeNX101)
			{
				var info = invoiceLine.JI_PTCriteria2Info;
				if (invoiceLine.JI_PTCriteria2.IsEmpty)
				{
					if (invoiceLine.IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code9, CertificateTypeList.Codes.Code11, CertificateTypeList.Codes.Code13, CertificateTypeList.Codes.Code14, CertificateTypeList.Codes.Code19))
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(info);
				}
			}
		}

		protected override void CheckJI_ManufacturerRelationship()
		{
			base.CheckJI_ManufacturerRelationship();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsForCMHeaderMessageTypeNX101)
			{
				var info = invoiceLine.JI_ManufacturerRelationshipInfo;
				if (invoiceLine.JI_ManufacturerRelationship.IsEmpty)
				{
					if (invoiceLine.IsLinkedNX101WithCertificateType(CertificateTypeList.Codes.Code9, CertificateTypeList.Codes.Code11, CertificateTypeList.Codes.Code13, CertificateTypeList.Codes.Code14, CertificateTypeList.Codes.Code19))
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(info);
				}
			}
		}

		protected override void CheckJI_TariffPrintLength()
		{
			base.CheckJI_TariffPrintLength();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsForCMHeaderMessageTypeNX101)
			{
				var info = invoiceLine.JI_TariffPrintLengthInfo;
				var tariffPrintLength = invoiceLine.JI_TariffPrintLength;
				MandatoryValidation.MessageErrorIfNotEntered(info);
				ListValidation.MessageErrorIfInvalidCode(info);
				if (tariffPrintLength == PrintingTariffCodeList.Codes.N && invoiceLine.IsForCMHeaderMessageTypeNX101CertificateType15)
				{
					info.AddMessageError(Res.GetString("842F0FF7-686A-49F9-88F8-101FDCB1569D", "When Certificate Type is '15', Tariff Printing cannot be 'N'."));
				}
			}
		}

		protected override void CheckJI_IMPTariff()
		{
			base.CheckJI_IMPTariff();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsForCMHeaderMessageTypeNX101CertificateType15 && invoiceLine.JI_IMPTariff.Length != 8)
			{
				invoiceLine.JI_IMPTariffInfo.AddMessageError(Res.GetString("8C92C05D-B62F-4B38-A10C-B12B1E341525", "Import Country's Tariff should be 8 characters long."));
			}
		}
	}
}
