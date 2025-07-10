using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceLineValidation : Customs.Business.BaseJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public void ValidateCertItemDescription()
		{
			ValidateCalculatedProperty(Parent.CertItemDescriptionInfo);
		}

		protected virtual void CheckCertItemDescription()
		{
		}

		protected override void CheckJI_Procedure()
		{
			//not used in SG
		}

		#region Check

		protected override void CheckJI_LineNo()
		{
			base.CheckJI_LineNo();
			if (Parent.Declaration != null && Parent.Declaration.InvoiceLines.Count > SGConstants.MaxEntryLines)
			{
				if (Parent.Declaration.JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMerge || Parent.Declaration.JE_MergeBy == OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription)
				{
					Parent.JI_LineNoInfo.AddMessageError(SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
				}
				else
				{
					if (Parent.Declaration.MergedLinesCount > SGConstants.MaxEntryLines)
					{
						var messageToShow = string.Format(CultureInfo.CurrentCulture, SGConstants.MaxLinesValidation.MaxEntryLimitForDeclaration + "\r\nThe merged line count is currently {0} entry lines.", Parent.Declaration.MergedLinesCount);
						Parent.JI_LineNoInfo.AddMessageError(messageToShow);
					}
					else if (CumulativeMergedLines > SGConstants.MaxEntryLines)
					{
						Parent.JI_LineNoInfo.AddWarning(SGConstants.MaxLinesValidation.CheckEntryLineLimitForDeclaration);
					}
				}
			}
		}

		int CumulativeMergedLines
		{
			get
			{
				var mergedEntryLines = Parent.Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.JI_CL != ZGuid.Empty).Select(x => x.JI_CL).Distinct().Count();
				var linesStillToBeMerged = Parent.Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Count(x => x.JI_CL == ZGuid.Empty);
				return mergedEntryLines + linesStillToBeMerged;
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo, Parent.Lookups.CountryOfOrigins as IBusinessObjectCollection);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CountryOfOriginInfo, "Country/Region of Origin");

			if (CertificateOfOriginDeclarationRequiringSingaporeOrigin && Parent.JI_CountryOfOrigin != Core.Constants.CountryCodes.Singapore)
			{
				Parent.JI_CountryOfOriginInfo.AddWarning("When declaring a Certificate of Origin, the Origin must be Singapore (SG), except for Certificate Types 3, 17, 22, 26, 28 & 30.");
			}

			if (!Parent.Declaration.HasBeenCleared)
			{
				if (Parent.JI_CountryOfOrigin.StartsWith(Core.Constants.CountryCodes.KoreaNorth) ||
					Parent.JI_CountryOfOrigin.StartsWith(Core.Constants.CountryCodes.Iran))
				{
					Parent.JI_CountryOfOriginInfo.AddWarning(JobDeclarationValidation.Circular18_2010);
				}
			}
		}

		bool CertificateOfOriginDeclarationRequiringSingaporeOrigin
		{
			get
			{
				return (Parent.Declaration.Certificate1Type.IsSGOriginRequired()) || (Parent.Declaration.Certificate2Type.IsSGOriginRequired());
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo, "Description");
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JI_DescriptionInfo);
		}

		protected override void CheckJI_Tariff()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_TariffInfo, "Tariff");

			foreach (CusLineTariffDetail product in Parent.ProductCodes)
			{
				product.Validation.ValidateBZ_Tariff();
			}

			var tariff = Parent.UniversalTariff;
			if (tariff == null)
			{
				// Check for tariff that may have expired or not yet activated.
				TariffValidator.ValidateTariff(Parent.Factory, tariff, Parent.JI_TariffInfo);
			}
			else
			{
				var effectiveAssessmentDate = Parent.EffectiveAssessmentDate;
				if (tariff.GetTariffCommodities(effectiveAssessmentDate).Any() && !Parent.ProductCodes.Cast<CusLineTariffDetail>().Any())
				{
					if (Parent.Declaration.IsTradeNet4Point1)
					{
						if (Parent.Declaration.IsExportOnly && tariff.IsUnderExportControl(effectiveAssessmentDate))
						{
							Parent.JI_TariffInfo.AddWarning(NoProductCodesOnHSE);
						}
						else if (Parent.Declaration.IsImportOnly && tariff.IsUnderImportControl(effectiveAssessmentDate))
						{
							Parent.JI_TariffInfo.AddWarning(NoProductCodesOnHSI);
						}
						else if (Parent.Declaration.IsTranshipment && tariff.IsUnderTranshipmentControl(effectiveAssessmentDate))
						{
							Parent.JI_TariffInfo.AddWarning(NoProductCodesOnMovement);
						}
					}
					else
					{
						Parent.JI_TariffInfo.AddWarning(NoProductCodes);
					}
				}
			}

			if (ClassificationIsRequiredForAutoCreationOfProduct && !Parent.JI_PartNo.IsEmpty && Parent.Part == null && Parent.JI_CC.IsEmpty && Parent.JI_Tariff.IsEmpty)
			{
				Parent.JI_TariffInfo.AddWarning(MandatoryCCOrTariffForAutoCreateProduct);
			}

			Parent.AddInfoValidation.ValidateSG_IsStrategic();
		}

		public static ResourceString NoProductCodesOnHSE => ResString.GetMultilingualString("1E2332F4-B932-4A95-9FF9-4275C26B8B9E", "This Tariff item appears to be a controlled HS Code for Export declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
		public static ResourceString NoProductCodesOnHSI => ResString.GetMultilingualString("304EB26A-669F-476B-9639-61076D169EEA", "This Tariff item appears to be a controlled HS Code for Import declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
		public static ResourceString NoProductCodesOnMovement => ResString.GetMultilingualString("7E96AA54-8B29-45B7-AB1B-31C6D8759523", "This Tariff item appears to be a controlled HS Code for Transhipment/Movement declarations. Consider entering a Product Code in the Product Code grid before sending to Customs.");
		public static ResourceString NoProductCodes => ResString.GetMultilingualString("8F730EE6-8A6D-403C-8E75-927CAF4172F2", "This item appears to be controlled. Consider entering a Product Code in the Product Code grid before sending to Customs.");

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			CompareValidation.CheckNumberNotNegative(Parent.JI_InvoiceQuantityInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_InvoiceUQInfo, Parent.Lookups.CustomsUQList);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			if (Parent.JI_CustomsQuantity <= 0m)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError("Please enter a valid Quantity");
			}
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsUnitQtyInfo, Parent.Lookups.CustomsUQList);
		}

		protected override void CheckJI_HazMatCodeQualifier()
		{
			base.CheckJI_HazMatCodeQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_HazMatCodeQualifierInfo, Parent.Lookups.HazardousMaterialCodeQualifierList);
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			CompareValidation.CheckNumberNotNegative(Parent.JI_LinePriceInfo);
		}

		protected override void CheckUnitPrice()
		{
			base.CheckUnitPrice();
			CompareValidation.CheckNumberNotNegative(Parent.UnitPriceInfo);
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_PrimaryPreferenceInfo);
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_WeightInfo);
		}

		#endregion

		#region Implementation

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCertItemDescription();
		}

		protected ValidationHelper ValidationHelper
		{
			get { return validationHelper ?? (validationHelper = new ValidationHelper()); }
		}
		ValidationHelper validationHelper;

		#endregion
	}
}
