using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public partial class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			var parent = Parent;
			if (parent.JI_Procedure == Constants.ProcedureCodes._50
				&& (!parent.FormattedSpecificDutyRate.IsEmpty || parent.FormattedAdValoremDutyRate != CommonHelper.GetFormattedStringForRateFormulaDerivedFrom("0")))
			{
				parent.JI_ProcedureInfo.AddMessageError(ValidationConstants.InvoiceLine.IncorrectDutyTreatmentTypesForNonDutyFreeTariff);
			}
		}

		protected override void CheckJI_Model()
		{
			base.CheckJI_Model();
			var parent = Parent;
			if (parent.JI_Model.IsEmpty)
			{
				var targetInfo = parent.JI_ModelInfo;
				if (parent.IsL1Declaration)
				{
					targetInfo.AddWarning(ValidationConstants.Declaration.AutomaticallyDeclareNIL(targetInfo.HumanReadableName));
				}
				if (parent.HasRegulationsCodeF01OrF02)
				{
					targetInfo.AddWarning(Res.GetString("56E4A338-9A50-4362-A7DB-951877844C35", "'Model' might be required when the goods is subject to F01 or F02 importer regulation."));
				}
			}
		}

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();
			var parent = Parent;
			if (parent.JI_BrandName.IsEmpty)
			{
				var targetInfo = parent.JI_BrandNameInfo;
				if (parent.IsL1Declaration)
				{
					targetInfo.AddWarning(ValidationConstants.Declaration.AutomaticallyDeclareNIL(targetInfo.HumanReadableName));
				}
				if (parent.HasRegulationsCodeF01OrF02)
				{
					targetInfo.AddMessageError(Res.GetString("FCA4FABB-E3EA-456C-B4FC-75CA1BADE301", "'Brand' cannot be empty when the goods is subject to F01 or F02 import regulation. For further details, please hit F3 on the 'Tariff' field and go to Details > Attribute."));
				}
			}
		}

		public override void ValidatePreviousPermitNo()
		{
			ValidateCalculatedProperty(Parent.PreviousPermitNoInfo);
		}

		protected override void CheckPreviousPermitNo()
		{
			var previousPermitNo = Parent.PreviousPermitNo;
			if (!previousPermitNo.IsEmpty)
			{
				var targetInfo = Parent.PreviousPermitNoInfo;
				if (previousPermitNo.Length != 14)
				{
					targetInfo.AddMessageError(Res.GetString("0fa5e68a-40a4-43d0-938c-2e81d579f030", "The length of Previous Permit Number must be 14 characters."));
				}

				EnglishCharactersValidation.ErrorIfNotWesternEuropean(targetInfo);
			}
		}

		void ValidateFoodDataMaximumRows()
		{
			if (!Parent.IsValidationSuspended)
			{
				if (Parent.FoodDataCollection.Count > 99)
				{
					Parent.AddRowMessageError(ValidationConstants.InvoiceLine.FoodContentMaximumRows);
				}
			}
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidatePreviousPermitNo();
				ValidateFoodDataMaximumRows();
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo);
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			var invoiceLine = Parent;
			if (!invoiceLine.JI_Tariff.IsEmpty)
			{
				var targetInfo = invoiceLine.JI_TariffInfo;
				CustomsRequirements(invoiceLine, Constants.UniversalReferenceConstants.CusTariffAttributeValue.T, Constants.UniversalReferenceConstants.RefCusRateTypes.CT, targetInfo, NotificationType.MessageError, ValidationConstants.InvoiceLine.CustomsRequirementT);
				CustomsRequirements(invoiceLine, Constants.UniversalReferenceConstants.CusTariffAttributeValue.B, Constants.UniversalReferenceConstants.RefCusRateTypes.AT, targetInfo, NotificationType.MessageError, ValidationConstants.InvoiceLine.CustomsRequirementB);
				CustomsRequirements(invoiceLine, Constants.UniversalReferenceConstants.CusTariffAttributeValue.C, Constants.UniversalReferenceConstants.RefCusRateTypes.TT, targetInfo, NotificationType.MessageError, ValidationConstants.InvoiceLine.CustomsRequirementC);
				CheckJI_TariffAfterGeneratingEntry(invoiceLine, targetInfo);
			}
		}

		void CheckJI_TariffAfterGeneratingEntry(JobComInvoiceLine invoiceLine, ZPropertyInfo targetInfo)
		{
			if (Declaration?.EntryHeader is CusEntryHeader entryHeader)
			{
				if (!entryHeader.HasB10OrB19Charges)
				{
					if (invoiceLine.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.T))
					{
						targetInfo.AddMessageError(Res.GetString("8683bf33-372b-4257-970a-b86c3c0d9a4a", "The Invoice Line is subject to Commodity Tax when the customs requirement of tariff is 'T'."));
					}
					else if (invoiceLine.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.TPartially))
					{
						targetInfo.AddWarning(Res.GetString("0767f45f-9128-4f2b-b0f4-457fca105c90", "The Invoice Line might be subject to Commodity Tax when the customs requirement of tariff is 'T*'."));
					}
				}

				if (!entryHeader.HasB31OrB69Charges)
				{
					if (invoiceLine.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.B)
						|| invoiceLine.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.C))
					{
						targetInfo.AddMessageError(Res.GetString("fdb036f8-4022-4b33-8f8f-35cfa609c0ca", "The Invoice Line is subject to Tobacco and Alcohol Tax when the customs requirement of tariff is 'B' or 'C'."));
					}
					else if (invoiceLine.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.BPartially))
					{
						targetInfo.AddWarning(Res.GetString("27f84159-d281-4e1f-b2be-52781fc2ea72", "The Invoice Line might be subject to Tobacco and Alcohol Tax when the customs requirement of tariff is 'B*'."));
					}
				}

				if (!entryHeader.HasB60OrB89Charges)
				{
					if (invoiceLine.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.LPartially))
					{
						targetInfo.AddWarning(Res.GetString("1baeff80-7217-452a-ae78-84077db8d12b", "The Invoice Line might be subject to Specifically Selected Goods and Services Tax when the customs requirement of tariff is 'L*'."));
					}
				}
			}
		}

		void CustomsRequirements(JobComInvoiceLine line, ZString attributeCode, ZString type, ZPropertyInfo info, CargoWise.ComponentModel.INotificationType notificationType, ZString message)
		{
			if (line.HasTariffCustomsRequirementsAttribute(attributeCode) && !line.Taxes.HasType(type))
			{
				info.AddNotification(notificationType, message);
			}
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (Parent.JI_CustomsSecondQuantity.IsEmpty)
			{
				if (Parent.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.Z))
				{
					Parent.JI_CustomsSecondQuantityInfo.AddWarning(ValidationConstants.InvoiceLine.CustomsSecondQuantityWarningMessage);
				}
				else if (!Parent.JI_CustomsSecondUnitQty.IsEmpty)
				{
					CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_CustomsSecondQuantityInfo);
				}
			}
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();

			var declarationType = Parent.EntryInstruction?.CEI_Style ?? ZString.Empty;
			if (declarationType == Constants.DeclarationTypes.Import.G7)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryNumberInfo);
			}
		}

		protected override void CheckJI_DeclarationGoodsDescription()
		{
			base.CheckJI_DeclarationGoodsDescription();

			var parent = Parent;
			ValidationHelper.CheckDeclarationGoodsDescriptionLengthWhenReportingAircraftParts(parent, parent.JI_DeclarationGoodsDescriptionInfo);
		}

		protected override void CheckJI_DtyPymntMthd()
		{
			base.CheckJI_DtyPymntMthd();
			var parent = Parent;
			CheckPaymentMethod(parent.JI_DtyPymntMthdInfo);
		}

		protected override void CheckJI_VatPymntMthd()
		{
			base.CheckJI_VatPymntMthd();
			var parent = Parent;
			CheckPaymentMethod(parent.JI_VatPymntMthdInfo);
		}

		protected override void CheckJI_TpfPymntMthd()
		{
			base.CheckJI_TpfPymntMthd();
			var parent = Parent;
			CheckPaymentMethod(parent.JI_TpfPymntMthdInfo);
		}

		protected override List<ZString> GetImportExportRegulationCodesCore(JobComInvoiceLine invoiceLine)
		{
			var importOrExportRegulationCodes = base.GetImportExportRegulationCodesCore(invoiceLine);
			if (invoiceLine.JI_CountryOfOrigin != Core.Constants.CountryCodes.China)
			{
				importOrExportRegulationCodes.RemoveAll(code => code == Constants.ImportExportRegulationCodes.RegulationsCodeMP1 || code == Constants.ImportExportRegulationCodes.RegulationsCodeMW0);
			}
			return importOrExportRegulationCodes;
		}

		void CheckPaymentMethod(ZPropertyInfo targetInfo)
		{
			var parent = Parent;
			if (parent.IsROR && (ZString)targetInfo.Value != parent.RORPaymentMethod)
			{
				targetInfo.AddWarning(Res.GetString("F62D113C-2356-4F97-A122-54AACC9E9F7F", "The selected {0} is different from the ROR Payment Method under Declaration > Entry Details.", targetInfo.HumanReadableName));
			}
		}
	}
}
