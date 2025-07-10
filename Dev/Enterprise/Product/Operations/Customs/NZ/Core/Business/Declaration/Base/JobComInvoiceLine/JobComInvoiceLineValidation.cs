using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobComInvoiceLineValidation : AutoNZJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return base.Parent as JobComInvoiceLine; }
		}

		protected bool IsTSWDeclaration
		{
			get
			{
				bool result = false;
				if (Parent != null && Parent.Declaration != null)
				{
					result = Parent.Declaration.IsTSWDeclaration;
				}

				return result;
			}
		}

		#region ValidateJI_ParentLineNo
		public void ValidateJI_ParentLineNo()
		{
			ValidateCalculatedProperty(Parent.JI_ParentLineNoInfo);
		}
		#endregion

		protected override void CheckJI_OH_TreatmentProvider()
		{
			base.CheckJI_OH_TreatmentProvider();
			if (Parent.Declaration?.IsTSWDeclaration ?? false)
			{
				var treatmentProvider = Parent.TreatmentProvider;
				if (treatmentProvider != null && treatmentProvider.LocalCustomsClientCode.IsEmpty)
				{
					Parent.JI_OH_TreatmentProviderInfo.AddMessageError(TreatmentProviderMissingCCD);
				}
			}
		}
		public const string TreatmentProviderMissingCCD = "Treatment Provider Organization does not have a TSW Premise Code. (NZ CCD should be added on the Config tab of the selected Organization).";

		protected override void CheckJI_LineNo()
		{
			base.CheckJI_LineNo();
			if (Parent.Declaration != null && Parent.Declaration.IsTSWEmptyContainerWriteOff)
			{
				Parent.JI_LineNoInfo.AddError(EmptyContainerEntryCannotHaveInvoiceLines);
			}
		}
		public const string EmptyContainerEntryCannotHaveInvoiceLines = "Invoice Lines are not allowed for empty container(s) write-off declaration.";

		#region CheckJI_ParentLineNo
		protected void CheckJI_ParentLineNo()
		{
			bool hasFound = false; // ListValidation uses database columns
			if (!Parent.JI_ParentLineNo.IsEmpty)
			{
				foreach (JobComInvoiceLine invoiceLine in Parent.ParentLines)
				{
					if (invoiceLine.JI_LineNoString == Parent.JI_ParentLineNo)
					{
						hasFound = true;
						break;
					}
				}

				if (!hasFound)
				{
					Parent.JI_ParentLineNoInfo.AddError("Please enter a valid parent line no.");
				}
			}

			if (Parent.Declaration != null && Parent.Declaration.IsImport)
			{
				if (Parent.JI_LinePriceInLocalCurrency > 24)
				{
					Parent.JI_ParentLineNoInfo.AddMessageError("Parent line can be hooked up if the line price is less than or equal to 24 NZD");
				}
			}
		}
		#endregion

		#region CheckJI_CountryOfOrigin
		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			if (Parent.EffectiveCountryOfOriginRefCountry == null)
			{
				Parent.JI_CountryOfOriginInfo.AddMessageError(MessageErrorEnterAValidCountryOfOrigin);
			}
			if (Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.Russia && Parent.JI_ConcessionCode.IsEmpty)
			{
				Parent.JI_CountryOfOriginInfo.AddWarning(ImportFromCountryUnderSanctionWarning);
			}
			ValidateJI_PreferentialCountryGroup();
		}
		public const string MessageErrorEnterAValidCountryOfOrigin = "Please enter a valid Country/Region of Origin.";
		public string ImportFromCountryUnderSanctionWarning => $"There are currently sanctions in place for goods exported from {Parent.CountryOfOrigin.RN_Desc}, please enter the applicable concession number";
		#endregion

		#region CheckJI_Description
		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);

			if (IsTSWDeclaration)
			{
				if (Parent.JI_Description.Length > 250)
				{
					Parent.JI_DescriptionInfo.AddWarning(GoodsDescTooLong);
				}
			}
		}
		public const string GoodsDescTooLong = "The Goods Description must not exceed 250 characters for an Entry.\r\nThe Goods Description will be truncated to 250 character in the message sent to TSW.";
		#endregion

		#region CheckJI_Tariff
		protected override void CheckJI_Tariff()
		{
			// Do not want to call base on this one.... Please don't add it back in.
			new TariffValidation.TariffValidator(Parent).CheckMainTariff();
			if (ClassificationIsRequiredForAutoCreationOfProduct && !Parent.JI_PartNo.IsEmpty && Parent.Part == null && Parent.JI_CC.IsEmpty && Parent.JI_Tariff.IsEmpty)
			{
				Parent.JI_TariffInfo.AddWarning(MandatoryCCOrTariffForAutoCreateProduct);
			}
		}
		#endregion

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			if (Parent.IsNonPeriodicTSWDeclaration && Parent.JI_InvoiceQuantity.IsEmpty)
			{
				Parent.JI_InvoiceQuantityInfo.AddWarning(NoOfPackagesMissingWarning);
			}
			if (Parent.ShouldSetPackagingDefaultQty && Parent.JI_InvoiceQuantity > int.MaxValue)
			{
				Parent.JI_InvoiceQuantityInfo.AddError(InvoiceQuantityOversizeError);
			}
		}
		public const string NoOfPackagesMissingWarning = "Leaving Number of Packages blank may produce reporting errors.";
		public string InvoiceQuantityOversizeError = $"Number of Packages could not be updated automatically because this value is too large. It cannot be greater than {int.MaxValue}";

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			if (Parent.IsNonPeriodicTSWDeclaration)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_InvoiceUQInfo, Parent.Lookups.InvoiceUQList);
				if (Parent.IsTSWCRE && Parent.JI_InvoiceUQ.IsEmpty)
				{
					Parent.JI_InvoiceUQInfo.AddWarning(TypeOfPackagesMissingWarning);
				}
			}
			if (Parent.IsIMPWriteOffDeclaration)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_InvoiceUQInfo, Parent.Lookups.InvoiceUQList);
			}
		}
		public const string TypeOfPackagesMissingWarning = "Leaving Unit of Packages blank may produce reporting errors.";

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			ValidateJI_CustomsQuantity();
		}

		protected override void CheckJI_CustomsQuantity()
		{
			//Don't call base
			if (Parent.JI_CustomsUnitQty.IsEmpty)
			{
				if (!Parent.JI_CustomsQuantity.IsEmpty)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError(MessageErrorRemoveStatQtyAsTariffDoesntRequireIt);
				}
			}
			else
			{
				bool hasPartsOtherInfoCode = Parent.OtherInfos.GetElementWithThisCode(LineOtherInfoList.Codes.Parts) != null;
				if (Parent.JI_CustomsQuantity.IsEmpty)
				{
					if (!hasPartsOtherInfoCode)
					{
						Parent.JI_CustomsQuantityInfo.AddMessageError(MessageErrorMustHaveStatQty);
					}
				}
				else
				{
					if (hasPartsOtherInfoCode)
					{
						Parent.JI_CustomsQuantityInfo.AddMessageError(MessageErrorRemoveStatQtyWhenUsingPTSOtherInfo);
					}
				}
			}
		}
		public const string MessageErrorMustHaveStatQty = "You must have a Customs Statistical Quantity when using this Tariff Code unless you are using a 'PTS' Other Info code. Please enter a Customs Statistical Quantity.";
		public const string MessageErrorRemoveStatQtyWhenUsingPTSOtherInfo = "You cannot have a Customs Statistical Quantity when you are using the 'PTS' Other Info code. Please remove the Quantity.";
		public const string MessageErrorRemoveStatQtyAsTariffDoesntRequireIt = "You cannot have a Customs Statistical Quantity when using this Tariff Code. Please remove the Quantity.";

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			if (IsTSWDeclaration && Parent.JI_Weight.IsEmpty)
			{
				Parent.JI_WeightInfo.AddWarning(GrossWeightRequired);
			}
		}
		public const string GrossWeightRequired = "Must be transmitted to state the Gross weight of the line item.";

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			if (Parent.Declaration != null && Parent.JI_NetWeight.IsEmpty)
			{
				if (IsTSWDeclaration && !Parent.IsTSWCRE)
				{
					Parent.JI_NetWeightInfo.AddWarning(NetWeightRequired);
				}
			}
		}
		public const string NetWeightRequired = "Must be transmitted to state the Net weight of the line item.";

		public void ValidateItemPackaging()
		{
			if (Parent.RequiresPackingLine)
			{
				JobComInvoiceLine invoiceLine = Parent;
				invoiceLine.ClearRowNotifications();
				using (((ISingleElementListInternal)invoiceLine).SuspendListChanged())
				{
					if (invoiceLine.ItemPackages.Count == 0 || invoiceLine.ItemPackages[0].NZ_NumberOfPackages == 0)
					{
						invoiceLine.AddRowMessageError(AtLeast1PackagingLineRequired);
					}
				}
			}
		}
		public const string AtLeast1PackagingLineRequired = "Item Packaging information must be transmitted to state the number, type, volume and shipping marks of the packaging.\r\nEnter this information in the Packaging grid.";

		#region PackageDetailsLine1

		public void ValidateNumberOfPackages1()
		{
			ValidateCalculatedProperty(Parent.NumberOfPackages1Info);
		}

		public void ValidatePackages1UQ()
		{
			ValidateCalculatedProperty(Parent.Packages1UQInfo);
		}

		public void ValidatePackagesVolume1()
		{
			ValidateCalculatedProperty(Parent.PackagesVolume1Info);
		}

		public void ValidatePackagingMarks1()
		{
			ValidateCalculatedProperty(Parent.PackagingMarks1Info);
		}

		protected void CheckNumberOfPackages1()
		{
			if (Parent.RequiresPackingLine)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.NumberOfPackages1Info);
			}
		}

		protected void CheckPackages1UQ()
		{
			if (Parent.RequiresPackingLine)
			{
				if (Parent.Packages1UQ.IsEmpty)
				{
					Parent.Packages1UQInfo.AddWarning(PackageUQRequired);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.Packages1UQInfo, Parent.Lookups.PackageUQList);
				}
			}
		}
		public const string PackageUQRequired = "Must be transmitted to state the Packaging unit of quantity.";

		protected void CheckPackagingMarks1()
		{
			if (Parent.RequiresPackingLine)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.PackagingMarks1Info);
			}
		}

		#endregion

		#region AddInfo Validations
		protected bool IsTSWImport => Parent?.Declaration?.IsTSWImportDeclaration ?? false;
		bool IntendedUseEntered => !Parent.JI_IntendedUseCode.IsEmpty || !Parent.JI_IntendedUse.IsEmpty;

		public static bool IsTariffForChapter2to22(ZString tariff)
		{
			ZString tariffPrefix = tariff.SubstringSafe(0, 2);
			if (!tariffPrefix.IsEmpty && tariffPrefix.IsNumbersOnlyOrEmpty)
			{
				int tariffChapter = Convert.ToInt16(tariffPrefix, CultureInfo.InvariantCulture);
				if (tariffChapter >= 02 && tariffChapter <= 22)
				{
					return true;
				}
			}

			return false;
		}

		protected override void CheckJI_LevyForExportCode()
		{
			base.CheckJI_LevyForExportCode();
			if (Parent.IsExportDrawbackOrCompletion && !Parent.JI_LevyCreditAmountCodeInfo.ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_LevyForExportCodeInfo, Parent.Lookups.LevyTypeCodeList);
			}
		}

		protected override void CheckJI_RN_NKCountryOfExport()
		{
			base.CheckJI_RN_NKCountryOfExport();
			if (Parent != null && Parent.Declaration != null && Parent.Declaration.IsImport && Parent.EffectiveCountryOfExport == null && !Parent.Declaration.IsTSWWriteOff)
			{
				Parent.JI_RN_NKCountryOfExportInfo.AddMessageError(MessageErrorEnterAValidCountryOfExport);
			}
		}
		public const string MessageErrorEnterAValidCountryOfExport = "Please enter a valid Country/Region of Export.";

		protected override void CheckJI_PartsOfClassification()
		{
			base.CheckJI_PartsOfClassification();

			var isIPIDeclaration = Parent?.Declaration?.IsPrimaryIndustriesImportDeclaration;
			if (!isIPIDeclaration.HasValue || !isIPIDeclaration.Value)
			{
				new TariffValidation.TariffValidator(Parent).CheckPartsOfTariff();
			}
		}

		protected override void CheckJI_ConcessionCode()
		{
			base.CheckJI_ConcessionCode();
			if (!Parent.JI_ConcessionCode.IsEmpty)
			{
				if (UniversalTariffHelper.UseRefDatabaseData)
				{
					ListValidation.WarnIfInvalidCode(Parent.JI_ConcessionCodeInfo, Parent.Lookups.ConcessionList as ICodeDescriptionPairList, UniversalTariffHelper.GetInvalidConcessionCodeWarningMessage(Parent.JI_ConcessionCode));
				}
				else
				{
					var dateForDutyRate = Parent.Declaration?.DateForDutyRate ?? ZDateTime.Now;
					ZString concessionCodeWarning = IsConcessionCodeValid(Parent.JI_ConcessionCode, dateForDutyRate);
					if (!concessionCodeWarning.IsEmpty)
					{
						Parent.JI_ConcessionCodeInfo.AddWarning(concessionCodeWarning.Replace("~", Parent.JI_ConcessionCode) + ConcessionCodeWarningMessage);
					}
				}
			}
			else if (Parent.JI_CountryOfOrigin == Core.Constants.CountryCodes.Russia)
			{
				Parent.JI_ConcessionCodeInfo.AddWarning(ImportFromCountryUnderSanctionWarning);
			}

			if (Parent.JI_ConcessionCode == "100001C" && AnyDutyRatePerUnitDutyRate(Parent.Tariff))
			{
				Parent.JI_ConcessionCodeInfo.AddWarning(Res.GetString("36688F5F-A35B-4212-B16A-C909E66EE1FF",
					"The use of this concession on excisable goods needs to be manually assessed with New Zealand Customs or it may be rejected. When submitting the entry, please use the ‘Manual Processing Request (Override)’ option."));
			}

			static bool AnyDutyRatePerUnitDutyRate(ITariff tariff)
			{
				if (UniversalTariffHelper.UseRefDatabaseData)
				{
					return (tariff as TariffView)?.Rates.Any(x => x.ZZ2_RateFormula.Contains('[')) ?? false; // It should has an excise rate when formula contains '[]', e.g. (5.000000*(VFD/100))+(0.700240*[LMS])
				}
				else
				{
					return (tariff as NZCClassification)?.DutyRates.Cast<NZCClassificationDutyRate>().Any(x => x.U1_DutyRatePerUnit1 > 0) ?? false;
				}
			}
		}

		public const string ConcessionCodeWarningMessage = " - This either means you're using an invalid concession code, the concession code is unpublished, or your Tariff Data is out of date.";
		const string ConcessionCodeWarningNotRecognisedMessage = "Concession Code [~] not recognised";
		const string ConcessionCodeWarningNotActiveMessage = "Concession Code [~] not active yet";
		const string ConcessionCodeErrorExpired = "Concession Code [~] has expired";

		ZString IsConcessionCodeValid(ZString concessionCode, ZDateTime validDate)
		{
			ZString result = ZString.Empty;
			ZQuery concessionFilter = new ZQuery(NZCConcessionSchema.U2_Code, concessionCode);
			NonDependentNZCConcessionCollection concessions = new NonDependentNZCConcessionCollection(Parent.Factory);
			concessions.Load(concessionFilter);
			if (concessions.Count == 0)
			{
				result = ConcessionCodeWarningNotRecognisedMessage;
			}

			foreach (NZCConcession concession in concessions)
			{
				if (concession.U2_DateActiveFrom > validDate)
				{
					result = ConcessionCodeWarningNotActiveMessage;
					break;
				}
				else if (concession.U2_DateActiveTo.IsValid && concession.U2_DateActiveTo < validDate)
				{
					result = ConcessionCodeErrorExpired;
					break;
				}
			}
			return result;
		}

		protected override void CheckJI_QualifiesForPreferentialDuty()
		{
			base.CheckJI_QualifiesForPreferentialDuty();
			if (Parent.Declaration != null && Parent.Declaration.IsImport && !Parent.Declaration.IsTSWWriteOff)
			{
				QualifiesForPreferentialDutyList list = new QualifiesForPreferentialDutyList();
				if (!list.ContainsCode(Parent.JI_EffectiveQualifiesForPreferentialDuty))
				{
					Parent.JI_QualifiesForPreferentialDutyInfo.AddMessageError(MessageErrorEnterValidQualForPrefDutyFlag);
				}
			}

			ValidateJI_PreferentialCountryGroup();
		}
		public const string MessageErrorEnterValidQualForPrefDutyFlag = "You must indicate if this Invoice Line Qualifies for Preferential Duty Rates or not.";

		protected override void CheckJI_PreferentialCountryGroup()
		{
			base.CheckJI_PreferentialCountryGroup();
			if (Parent.Declaration?.IsImport ?? false)
			{
				if (Parent.JI_EffectivePreferentialCountryGroup.IsEmpty)
				{
					if (Parent.JI_EffectiveQualifiesForPreferentialDuty == QualifiesForPreferentialDutyList.Codes.Qualifies
						&& !NZCustomsDataRegistry.Instance.PreferentialCountryGroupCodeDefaulting.Value
						&& Parent.Lookups.PreferentialCountryGroupCodeList.Count > 1)
					{
						Parent.JI_PreferentialCountryGroupInfo.AddMessageError(MessageErrorPreferentialCountryGroupRequired);
					}
				}
				else if (!Parent.Lookups.PreferentialCountryGroupCodeList.ContainsCode(Parent.JI_EffectivePreferentialCountryGroup))
				{
					Parent.JI_PreferentialCountryGroupInfo.AddMessageError(MessageErrorPreferentialCountryGroupNotInList);
				}
			}
		}

		internal const string MessageErrorPreferentialCountryGroupRequired = "You must select a Preference Code for lines that Qualify for Preferential Rates of Duty where multiple Preference Codes could apply.\r\n(nb: If you turn on [Preferential Country/Region Group Code Defaulting] in the Registry, a Preference Code will be automatically defaulted for you when you don't enter one.)";
		internal const string MessageErrorPreferentialCountryGroupNotInList = "The Preference Code you have entered is not valid for this Country/Region of Origin.";

		protected override void CheckJI_IntendedUseCode()
		{
			base.CheckJI_IntendedUseCode();
			if (IsTSWImport)
			{
				if (!IntendedUseEntered && IsTariffForChapter2to22(Parent.JI_Tariff))
				{
					Parent.JI_IntendedUseCodeInfo.AddMessageError(IntendedUseCodeRequired);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.JI_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodeList);
				}

				Parent.Validation.ValidateJI_IntendedUse();
			}
		}
		public const string IntendedUseCodeRequired = "The Intended Use details, (Code or Text), must be entered if the Tariff Code is within chapters 02 – 22";

		protected override void CheckJI_IntendedUse()
		{
			base.CheckJI_IntendedUse();
			if (IsTSWImport)
			{
				if (!IntendedUseEntered && IsTariffForChapter2to22(Parent.JI_Tariff))
				{
					Parent.JI_IntendedUseInfo.AddMessageError(IntendedUseCodeRequired);
				}

				Parent.Validation.ValidateJI_IntendedUseCode();
			}
		}

		protected override void CheckJI_SupplementaryQty()
		{
			if (!Parent.JI_SupplementaryUQ.IsEmpty && Parent.JI_SupplementaryQty <= 0.00m)
			{
				Parent.JI_SupplementaryQtyInfo.AddMessageError(MessageErrorMustHaveSupplementaryQuantity);
			}
		}
		public const string MessageErrorMustHaveSupplementaryQuantity = "Supplementary Quantity must be greater than 0.00 when using this Tariff Code.";

		protected override void CheckJI_IsZeroRatedDuty()
		{
			base.CheckJI_IsZeroRatedDuty();
			if (Parent.Declaration != null && Parent.Declaration.IsImport && !Parent.Declaration.IsTSWWriteOff)
			{
				if (!Parent.JI_IsZeroRatedDuty.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JI_IsZeroRatedDuty))
				{
					Parent.JI_IsZeroRatedDutyInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedDutyFlag);
				}
			}
		}
		public const string MessageErrorMustHaveValidZeroRatedDutyFlag = "'Zero Rated Duty' must be either blank, 'Yes' or 'No'.";

		protected override void CheckJI_IsZeroRatedExcise()
		{
			base.CheckJI_IsZeroRatedExcise();
			if (Parent.Declaration != null && Parent.Declaration.IsImport)
			{
				if (!Parent.JI_IsZeroRatedExcise.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JI_IsZeroRatedExcise))
				{
					Parent.JI_IsZeroRatedExciseInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedExciseFlag);
				}
			}
		}
		public const string MessageErrorMustHaveValidZeroRatedExciseFlag = "'Zero Rated Excise' must be either blank, 'Yes' or 'No'.";

		protected override void CheckJI_IsZeroRatedGST()
		{
			base.CheckJI_IsZeroRatedGST();
			if (Parent != null && Parent.Declaration != null && Parent.Declaration.IsImport)
			{
				if (!Parent.JI_IsZeroRatedGST.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JI_IsZeroRatedGST))
				{
					Parent.JI_IsZeroRatedGSTInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedGSTFlag);
				}
			}
		}
		public const string MessageErrorMustHaveValidZeroRatedGSTFlag = "'Zero Rated GST' must be either blank, 'Yes' or 'No'.";

		protected override void CheckJI_IsZeroRatedLevies()
		{
			base.CheckJI_IsZeroRatedLevies();
			if (Parent != null && Parent.Declaration != null && Parent.Declaration.IsImport)
			{
				if (!Parent.JI_IsZeroRatedLevies.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JI_IsZeroRatedLevies))
				{
					Parent.JI_IsZeroRatedLeviesInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedLeviesFlag);
				}
			}
		}
		public const string MessageErrorMustHaveValidZeroRatedLeviesFlag = "'Zero Rated Levies' must be either blank, 'Yes' or 'No'.";

		protected override void CheckJI_OriginRegion()
		{
			base.CheckJI_OriginRegion();
			if (Parent != null && Parent.Declaration != null && Parent.Declaration.IsTSWDeclaration)
			{
				if ((!Parent.JI_OriginRegion.IsEmpty) && !IsTariffForChapter2to22(Parent.JI_Tariff))
				{
					Parent.JI_OriginRegionInfo.AddMessageError(OriginRegionNotRequired);
				}
			}
		}
		public const string OriginRegionNotRequired = "The Origin Region should only be entered if the Tariff Code is for scheduled products of Tariff chapter 02 – 22";
		#endregion
	}
}
