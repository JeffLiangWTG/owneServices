using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	class ACEImportJobComInvoiceLineValidation : FormalImportJobComInvoiceLineValidation
	{
		public ACEImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			base.CheckJI_TariffIsValidWhenItIsNotEmpty();

			if (Parent.IsEntrySummaryValidationMode)
			{
				new LicenceValidator().ValidateRequirementForACE(Parent.JI_TariffInfo, Parent, Tariff);
			}

			ValidateLaceyActRequirement();
		}

		void ValidateLaceyActRequirement()
		{
			var declaration = Declaration;
			if (declaration != null && !Parent.IsSetXLine && InvoiceLine.PGARequirementIndicator.MayRequireACSLacey && !declaration.IsACECargoCertificationMode)
			{
				Parent.JI_TariffInfo.AddWarning(LaceyActDataCertifyInACE);
			}
		}

		internal const string LaceyActDataCertifyInACE = "This tariff may require reporting of Lacey Act data. If this tariff does require reporting of Lacey Act data, you should change the Messaging Mode to ACS or certify in ACE.";

		bool IsACECargoReleaseValidationMode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsACECargoReleaseValidationMode;
			}
		}

		protected override void CheckJI_OA_ExporterAddress()
		{
			base.CheckJI_OA_ExporterAddress();
			var address = Parent.ExporterAddress;
			var info = Parent.JI_OA_ExporterAddressInfo;
			if (!Parent.US_ADDCaseNo.IsEmpty || !Parent.US_CVDCaseNo.IsEmpty)
			{
				if (address == null)
				{
					if (Parent.IsEntrySummaryValidationMode)
					{
						info.AddMessageError(ForeignExporterIsRequiredForADD_CVD);
					}
				}
				else
				{
					OrganisationValidation.ValidateManufacturerIDForAddress(address, info, false, CargoWise.EntityFramework.NotificationType.MessageError);
				}
			}
			CheckOrganisationAMSCodeWhenOR1(Parent, address, info);
		}
		internal const string ForeignExporterIsRequiredForADD_CVD = "Foreign Exporter MID is required with ADD/CVD details.";

		void CheckOrganisationAMSCodeWhenOR1(JobComInvoiceLine line, OrgAddress address, ZPropertyInfo addressInfo)
		{
			if (line.HasAMSOR1)
			{
				if (address == null)
				{
					MandatoryValidation.MessageErrorIfNotEntered(addressInfo);
				}
				else
				{
					var amsCode = address.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.AMSRegistrationNumber, Core.Constants.CountryCodes.UnitedStates);
					if (amsCode.IsEmpty)
					{
						addressInfo.AddMessageError(Res.GetString("C9C4295B-43C8-461F-A53C-108A6A6D5CE2", "Organization must have Registration Code of type AMS on file."));
					}
					else if (!Regex.IsMatch(amsCode, @"^[0-9]{10}$"))
					{
						addressInfo.AddMessageError(Res.GetString("22F78C92-B7C8-4064-9B80-3F4698956137", "AMS code must be 10 digits."));
					}
				}
			}
		}

		protected override void CheckJI_OA_Seller()
		{
			base.CheckJI_OA_Seller();
			if (IsACECargoReleaseValidationMode && Parent.JI_OA_Seller.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_OA_SellerInfo, "Seller");
			}
		}

		protected override void CheckJI_OA_ManufacturerAddress()
		{
			base.CheckJI_OA_ManufacturerAddress();

			var parent = Parent;
			if (IsACECargoReleaseValidationMode)
			{
				if (parent.ManufacturerAddress is OrgAddress address)
				{
					new ZipCodeValidation().ValidatePostalCodeForChinaManufacturer(parent.JI_OA_ManufacturerAddressInfo, address.OA_RN_NKCountryCode, address.OA_PostCode);
				}
				else if (!parent.Declaration.IsLowValue)
				{
					parent.JI_OA_ManufacturerAddressInfo.AddMessageError(ManufacturerRequiredForSE);
				}
			}
		}
		internal const string ManufacturerRequiredForSE = "Manufacturer is mandatory for ACE Cargo Release.";

		protected override bool ForceMIDToBeEntered => base.ForceMIDToBeEntered && Declaration != null && !Declaration.IsLowValue;

		protected override bool ShouldCheckJI_OA_ManufacturerAddress
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null && !EntryTypeList.IsInformal(declaration.US_EntryType);
			}
		}

		protected override void CheckJI_OA_SoldToPartyAddress()
		{
			base.CheckJI_OA_SoldToPartyAddress();
			if (!EntryTypeList.IsInformal(Parent.ImportEntryType) && Declaration != null && !Declaration.IsLowValue)
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JI_OA_SoldToPartyAddressInfo, OrgMatchedCustomsRegNoType.EIN, string.Format(CultureInfo.CurrentCulture, OrganisationValidation.EIN_SSN_CBNCodeRequired, "Sold To Party"), true, true);
			}
		}

		protected override bool IsTariffMandatory
		{
			get { return !Parent.IsCombinedLine() && base.IsTariffMandatory; }
		}

		void CheckTariffFromMX(JobComInvoiceLine parentLine)
		{
			if (parentLine.IsImport && !parentLine.JI_Tariff.IsEmpty)
			{
				var tradeGroupCountry = parentLine.US_UC_NKCountryOfOrigin;
				if (tradeGroupCountry == Core.Constants.CountryCodes.Mexico)
				{
					var universalTariff = parentLine.UniversalImportTariff;
					if (universalTariff != null)
					{
						var criteria = new ZZConditionSelectionCriteria(ZDateTime.Today, tradeGroupCountry,
							ZString.Empty, null, ZString.Empty, parentLine.GetDefaultDataGroupingCode(),
							ConditionChecker.ConditionDirection.Import, RefCusConditionTypes.ConditionClass.Control,
							TariffConditionTypes.Codes.IRCMF);
						foreach (var condition in
							ConditionChecker.GetApplicableConditions(parentLine.Factory, universalTariff, criteria))
						{
							parentLine.JI_TariffInfo.AddWarning(condition.ZX1_Comment);
						}
					}
				}
			}
		}

		internal static void CheckSmeltAndCastInformationRequired(ZPropertyInfo propertyInfo, JobComInvoiceLine invoiceLine, bool isSmeltCastInformationMandatory, bool hasValueEnteredWhenNotRequired, Action validateRelatedSmeltFieldsAction)
		{
			var isAluminumSmeltEffective = invoiceLine.IsAluminumSmeltEffective;
			var isTariffMatchesSmeltCondition = invoiceLine.TariffMatchesSmeltCondition;
			if (isAluminumSmeltEffective && isTariffMatchesSmeltCondition && isSmeltCastInformationMandatory)
			{
				propertyInfo.AddMessageError(SmeltAndCastInformationRequired);
			}
			else if ((!isAluminumSmeltEffective || !isTariffMatchesSmeltCondition) && hasValueEnteredWhenNotRequired)
			{
				propertyInfo.AddMessageError(SmeltAndCastInformationNotRequired);
			}

			if (validateRelatedSmeltFieldsAction != null)
			{
				validateRelatedSmeltFieldsAction();
			}
		}
		internal const string SmeltAndCastInformationRequired = "Smelt and Cast information required for this tariff number.";
		internal const string SmeltAndCastInformationNotRequired = "Smelt and Cast information not required for this tariff number.";

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			CheckTariffFromMX(Parent);
			CheckSmeltAndCastInformationRequired(Parent.JI_TariffInfo, InvoiceLine, !InvoiceLine.IsAluminumSmeltAndCastCountryClaimed, InvoiceLine.IsAluminumSmeltAndCastCountryClaimed, () =>
			{
				var validation = InvoiceLine.AddInfoValidation;
				validation.ValidateUS_Prim_NA();
				validation.ValidateUS_RN_NKPrimCtry();
				validation.ValidateUS_Sec_NA();
				validation.ValidateUS_RN_NKSecCtry();
				validation.ValidateUS_RN_NKCastCtry();
			});

			if (Parent.IsCombinedLine())
			{
				var dutyData = (IDutyData)Parent;
				var combineAllLines = dutyData.CombineAllLines;
				var tariffList = new List<ZString>();
				foreach (var idutydata in combineAllLines)
				{
					if (idutydata.SupTariffs.Count > 0)
					{
						tariffList.AddRange(idutydata.SupTariffs);
					}

					if (!idutydata.Tariff.IsEmpty)
					{
						tariffList.Add(idutydata.Tariff);
					}
				}
				var indexOf99 = tariffList.FindLastIndex(x => Chapter98Helper.Is99Tariff(x));
				var indexOf98 = tariffList.FindLastIndex(x => Chapter98Helper.Is98Tariff(x));
				var indexOfNormal = tariffList.FindLastIndex(x => !Chapter98Helper.Is99Tariff(x) && !Chapter98Helper.Is98Tariff(x));
				var has98Line = tariffList.Any(x => Chapter98Helper.Is98Tariff(x));

				if (Parent.IsTIBEntryType)
				{
					var correctOrder = indexOf98 < indexOf99 && indexOf99 < indexOfNormal;
					if (!correctOrder)
					{
						Parent.JI_TariffInfo.AddMessageError(TIBTariffOrder);
					}
				}
				else
				{
					if (has98Line)
					{
						var correctOrder = indexOf98 < indexOf99 && indexOf99 < indexOfNormal;
						if (!correctOrder)
						{
							Parent.JI_TariffInfo.AddMessageError(NormalCombieLineTariffOrder);
						}
					}
					else
					{
						var correctOrder = indexOfNormal > indexOf99;
						if (!correctOrder)
						{
							Parent.JI_TariffInfo.AddMessageError(NormalCombieLineTariffOrderNo98Line);
						}
					}
				}
			}
		}
		internal const string TIBTariffOrder = "For TIB combined lines, Tariff orders should be '9813' then '99', the classification tariff should be in the last.";
		internal const string NormalCombieLineTariffOrder = "For Combined Lines, Chapter 98 tariff numbers should be listed first, followed by Chapter 99 tariff numbers. The classification tariff should be listed under Tariff in the last combined line.";
		internal const string NormalCombieLineTariffOrderNo98Line = "For combined lines, Tariff orders should be '99' then classification tariff.";

		protected override void CheckJI_LinePrice()
		{
			if (Parent.IsCombinedLine() && Parent.JI_Tariff.IsEmpty)
			{
				if (!Parent.JI_LinePrice.IsEmpty)
				{
					Parent.JI_LinePriceInfo.AddMessageError(PriceShouldBeZeroForCombinedLine);
				}
			}
			else if (Parent.IsDerivedSetsPrentLine())
			{
				if (!Parent.JI_LinePrice.IsEmpty)
				{
					Parent.JI_LinePriceInfo.AddMessageError(PriceShouldBeZeroForDerived);
				}
			}
			else if (Parent.Declaration != null && Parent.Declaration.IsLowValue && Parent.JI_LinePrice.IsEmpty)
			{
				Parent.JI_LinePriceInfo.AddMessageError(PriceIsMandatoryForLowValueEntries);
			}
			else if (!Parent.JI_LinePrice.IsEmpty && Parent.IsEmbroideryChildTariffLine)
			{
				Parent.JI_LinePriceInfo.AddMessageError(ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, Parent.JI_LinePriceInfo.HumanReadableName));
			}
			else
			{
				base.CheckJI_LinePrice();
				var parent = Parent;
				if (!parent.IsChildLine
					&& (parent.IsFWSDeclared || (parent.IsParentLine && parent.ChildLines.Cast<JobComInvoiceLine>().Any(x => x.IsFWSDeclared)))
					&& parent.TotalLinePriceIncludingChildLines < parent.TotalFWSValueIncludingChildLines)
				{
					parent.JI_LinePriceInfo.AddMessageError(ZString.Format(TotalInvCurrValueGreaterThanLinePrice, parent.TotalFWSValueIncludingChildLines.ToStringTrimZeros(), parent.TotalLinePriceIncludingChildLines.ToStringTrimZeros()));
				}
			}
		}
		internal const string TotalInvCurrValueGreaterThanLinePrice = "The total of Inv. Curr. Value ({0}) from FWS data should be less than the line price ({1}) (including child lines).";
		internal const string PriceShouldBeZeroForCombinedLine = "The line price should be zero in a combined tariff line.";
		internal const string PriceShouldBeZeroForDerived = "The line price should be zero.";
		internal const string PriceIsMandatoryForLowValueEntries = "Line price is mandatory for entry type '86'.";

		protected override void CheckJI_OA_ConsigneeAddress()
		{
			base.CheckJI_OA_ConsigneeAddress();

			if (!IsACECargoReleaseValidationMode && !OrgHeaderWrapper.GetCustomsRelatedCode(Parent.ConsigneeOrgAddress, OrgMatchedCustomsRegNoType.EIN).IsEmpty)
			{
				if (Parent.Declaration.UltimateConsigneeCustomsClientNumber.IsEmpty)
				{
					Parent.JI_OA_ConsigneeAddressInfo.AddMessageError(NoLineConsigneeAllowedWhenHeaderHasNoConsigneeEntered);
				}
			}

			if (Parent.ConsigneeOrgAddress == null)
			{
				var hasNHTSARequireUltimateConsignee = Parent.HasNHTSADetails && Parent.NHTSALines.OfType<NHTSAHeader>().Any(x => x.NHTSADocuments.HasOrganization(NHTSAOrganizationTypeList.Codes.Consignee));
				if (IsEntrySummaryOrCargoReleaseValidationMode && hasNHTSARequireUltimateConsignee)
				{
					Parent.JI_OA_ConsigneeAddressInfo.AddMessageError(UltimateConsigneeIsRequiredFor("NHTSA"));
				}

				if (Parent.HasAPHISHeaders && Parent.IsACECargoReleaseValidationMode)
				{
					Parent.JI_OA_ConsigneeAddressInfo.AddMessageError(UltimateConsigneeIsRequiredFor("APHIS"));
				}
			}

			if (Parent.HasAMSMO4 && Parent.ConsigneeAddress != null)
			{
				var amsCodes = Parent.ConsigneeAddress.Header.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.USACodeTypes.AMSRegistrationNumber);
				if (amsCodes == null)
				{
					Parent.JI_OA_ConsigneeAddressInfo.AddMessageError(AMSIsRequiredForAMSMO4);
				}
			}

			var declaration = Declaration;
			if (declaration != null && declaration.IsLowValue && declaration.JE_OA_ConsigneeAddress != Parent.JI_OA_ConsigneeAddress)
			{
				Parent.JI_OA_ConsigneeAddressInfo.AddMessageError(ConsigneeNotMatchingMessageText);
			}
		}

		internal const string ConsigneeNotMatchingMessageText = "The Consignee must match the Consignee on the Declaration Header for Entry Type 86.";
		internal const string AMSIsRequiredForAMSMO4 = "AMS MO4 is selected in this invoice line, AMS ID number is required against this organization.";
		internal const string NoLineConsigneeAllowedWhenHeaderHasNoConsigneeEntered = "Line level consignee should never be allowed without Declaration level consignee.";
		internal static string UltimateConsigneeIsRequiredFor(string type)
		{
			return string.Format(CultureInfo.CurrentCulture, "Ultimate Consignee is required for {0} reporting.", type);
		}

		bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get { return Parent.Declaration != null && Parent.Declaration.IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();

			if (!Parent.JI_Weight.IsEmpty && Parent.IsEmbroideryChildTariffLine)
			{
				Parent.JI_WeightInfo.AddMessageError(ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, Parent.JI_WeightInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_Volume()
		{
			base.CheckJI_Volume();

			if (!Parent.JI_Volume.IsEmpty && Parent.IsEmbroideryChildTariffLine)
			{
				Parent.JI_VolumeInfo.AddMessageError(ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, Parent.JI_VolumeInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();

			if (!Parent.JI_NetWeight.IsEmpty && Parent.IsEmbroideryChildTariffLine)
			{
				Parent.JI_NetWeightInfo.AddMessageError(ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, Parent.JI_NetWeightInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			if (!Parent.JI_InvoiceQuantity.IsEmpty && Parent.IsEmbroideryChildTariffLine)
			{
				Parent.JI_InvoiceQuantityInfo.AddMessageError(ZString.Format(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredOnParent, Parent.JI_InvoiceQuantityInfo.HumanReadableName));
			}
		}

		protected override void CheckSupFormattedAdditionalTariff1()
		{
			base.CheckSupFormattedAdditionalTariff1();
			CheckSupFormattedAdditionalTariffs(Parent, Parent.ImportSupAdditionalTariff1, Parent.SupFormattedAdditionalTariff1Info);
		}

		protected override void CheckSupFormattedAdditionalTariff2()
		{
			base.CheckSupFormattedAdditionalTariff2();
			CheckSupFormattedAdditionalTariffs(Parent, Parent.ImportSupAdditionalTariff2, Parent.SupFormattedAdditionalTariff2Info);
		}

		protected override void CheckSupFormattedAdditionalTariff3()
		{
			base.CheckSupFormattedAdditionalTariff3();
			CheckSupFormattedAdditionalTariffs(Parent, Parent.ImportSupAdditionalTariff3, Parent.SupFormattedAdditionalTariff3Info);
		}

		protected override void CheckSupFormattedAdditionalTariff4()
		{
			base.CheckSupFormattedAdditionalTariff4();
			CheckSupFormattedAdditionalTariffs(Parent, Parent.ImportSupAdditionalTariff4, Parent.SupFormattedAdditionalTariff4Info);
		}

		protected override void CheckSupFormattedAdditionalTariff5()
		{
			base.CheckSupFormattedAdditionalTariff5();
			CheckSupFormattedAdditionalTariffs(Parent, Parent.ImportSupAdditionalTariff5, Parent.SupFormattedAdditionalTariff5Info);
		}

		void CheckSupFormattedAdditionalTariffs(JobComInvoiceLine invoiceLine, USCTariff tariff, ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				TariffValidator.Validate(invoiceLine, tariff, propertyInfo);

				var supTariffs = new ZString[] { invoiceLine.US_SupTariff, invoiceLine.US_SupAdditionalTariff1, invoiceLine.US_SupAdditionalTariff2, invoiceLine.US_SupAdditionalTariff3, invoiceLine.US_SupAdditionalTariff4, invoiceLine.US_SupAdditionalTariff5 };
				var numberOfNoneEmptyTariffs = supTariffs.Count(t => !t.IsEmpty);
				var validationError = numberOfNoneEmptyTariffs switch
				{
					1 when invoiceLine.US_SupTariff.IsEmpty
						=> "Only 'Prov/Prog. Tariff' should be filled when there is one supplementary tariff on invoice line.",
					2 when invoiceLine.US_SupTariff.IsEmpty || invoiceLine.US_SupAdditionalTariff1.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1' and 'Prov/Prog. Tariff' should be filled when there are two supplementary tariffs on invoice line.",
					3 when invoiceLine.US_SupTariff.IsEmpty || invoiceLine.US_SupAdditionalTariff1.IsEmpty || invoiceLine.US_SupAdditionalTariff2.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2' and 'Prov/Prog. Tariff' should be filled when there are three supplementary tariffs on invoice line.",
					4 when invoiceLine.US_SupTariff.IsEmpty || invoiceLine.US_SupAdditionalTariff1.IsEmpty || invoiceLine.US_SupAdditionalTariff2.IsEmpty || invoiceLine.US_SupAdditionalTariff3.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3' and 'Prov/Prog. Tariff' should be filled when there are four supplementary tariffs on invoice line.",
					5 when invoiceLine.US_SupTariff.IsEmpty || invoiceLine.US_SupAdditionalTariff1.IsEmpty || invoiceLine.US_SupAdditionalTariff2.IsEmpty || invoiceLine.US_SupAdditionalTariff3.IsEmpty || invoiceLine.US_SupAdditionalTariff4.IsEmpty
						=> "Only 'Prov/Prog. Additional Tariff 1', 'Prov/Prog. Additional Tariff 2', 'Prov/Prog. Additional Tariff 3', 'Prov/Prog. Additional Tariff 4' and 'Prov/Prog. Tariff' should be filled when there are five supplementary tariffs on invoice line.",
					_ => ""
				};

				if (!string.IsNullOrEmpty(validationError))
				{
					propertyInfo.AddMessageError(validationError);
				}

				TariffValidator.ValidateSupplementaryTariffIfEnteredOnChildLineForDerivedSets(invoiceLine, propertyInfo);
			}
		}
	}
}
