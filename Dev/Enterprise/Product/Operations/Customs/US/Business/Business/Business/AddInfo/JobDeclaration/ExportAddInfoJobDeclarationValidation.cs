using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.LicenseManager;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ExportAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public ExportAddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		public new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent.Parent; }
		}

		protected override void CheckUS_DateOfExport()
		{
			base.CheckUS_DateOfExport();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DateOfExportInfo, "Date Of Export");

			if (Declaration.US_CommodityFilingOption == AESCommodityFilingOptionList.Codes._2Predeparture &&
				Declaration.US_DateOfExport < ZDateTime.Today &&
				Declaration.JE_EntryStatus.IsEmpty)
			{
				Parent.US_DateOfExportInfo.AddWarning(LateDepartureFiling);
			}

			if (Parent.US_DateOfExport > ZDateTime.Today.AddDays(120))
			{
				Parent.US_DateOfExportInfo.AddMessageError(string.Format(ExportDateTooFar, ZDateTime.Today.AddDays(120).ToShortDateString()));
			}
		}
		internal const string LateDepartureFiling = "Pre-Departure filing will be considered late if AES receives this data after the Export Date.";
		internal const string ExportDateTooFar = "AES cannot accept an Export Date more than 120 days in the future. ('{0}')";

		protected override void CheckUS_TransportReference()
		{
			base.CheckUS_TransportReference();
			var declaration = Declaration;

			if (Parent.US_TransportReference.IsEmpty)
			{
				if (declaration.IsTransportReferenceNumberRequired())
				{
					Parent.US_TransportReferenceInfo.AddMessageError(TransportRefRequired);
				}
			}
			else
			{
				if (declaration.IsTransportReferenceNumberForbidden())
				{
					Parent.US_TransportReferenceInfo.AddMessageError(TransportRefMustBeBlank);
				}
				else if (AESTransportationReferenceNumberValidator.HasLeadingOrEmbeddedSpaces(Parent.US_TransportReference))
				{
					Parent.US_TransportReferenceInfo.AddMessageError(TransportRefCannotContainSpaces);
				}

				if (declaration.IsAir)
				{
					var validationMessage = new AESTransportationReferenceNumberValidator().GetWarningMessage(Parent.US_TransportReference);
					if (!string.IsNullOrEmpty(validationMessage))
					{
						Parent.US_TransportReferenceInfo.AddWarning(validationMessage);
					}

					if (!Regex.IsMatch(Parent.US_TransportReference, @"^[0-9]{3}-[0-9]{8}$"))
					{
						Parent.US_TransportReferenceInfo.AddMessageError(TransportRefMustHaveThisFormat);
					}
				}
			}
		}
		internal const string TransportRefRequired = "Transport Reference Number is required for this Transport Type.";
		internal const string TransportRefMustBeBlank = "Transport Reference Number must be left blank for this Transport Type.";
		internal const string TransportRefMustHaveThisFormat = "For Air Shipments, Transportation Reference must be in NNN-NNNNNNNN format if entered.";
		internal const string TransportRefCannotContainSpaces = "Transport Reference Number cannot contain leading spaces or embedded spaces.";

		protected override void CheckUS_LicenseType()
		{
			base.CheckUS_LicenseType();
			var parent = Parent;
			var licenseType = parent.US_LicenseType;
			if (!licenseType.IsEmpty)
			{
				var factory = parent.Factory;
				var exportDate = parent.GetEffectiveDateForECR();

				var licenseTypeBO = exportDate.IsValid ? ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, licenseType, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, exportDate) : null;
				if (licenseTypeBO == null)
				{
					parent.US_LicenseTypeInfo.AddMessageError(CodeIsInvalid);
				}
				else
				{
					if (exportDate.IsValid)
					{
						var transportModeError = LicenseValidationHelper.GetTransportModeError(licenseType, parent.JE_TransportMode, factory, exportDate);
						if (!transportModeError.IsEmpty)
						{
							parent.US_LicenseTypeInfo.AddMessageError(transportModeError);
						}
					}

					var errorMessage = LicenseValidationHelper.AddLicenseTypeValidationErrorMessage(parent.US_RN_NKCountryOfDestination, licenseType);
					if (!string.IsNullOrEmpty(errorMessage))
					{
						parent.US_LicenseTypeInfo.AddMessageError(errorMessage);
					}
				}

				ValidateUS_LicenseNo();
				ValidateUS_ECCN();
				ValidateUS_ExportCode();
				ValidateUS_DDTCITARExemptionNo();
				ValidateUS_DDTCMilitaryEquipmentIndicator();
				ValidateUS_DDTCPartyCertificationIndicator();
				ValidateUS_DDTCRegistrationNo();
				ValidateUS_DDTCUSMLCategoryCode();
			}
		}
		internal const string CodeIsInvalid = "The code you have selected is invalid.";

		protected override void CheckUS_TariffType()
		{
			base.CheckUS_TariffType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TariffTypeInfo, Lookups.US_TariffTypeList);
		}

		protected override void CheckUS_RL_NKPortOfExport()
		{
			base.CheckUS_RL_NKPortOfExport();

			if (Parent.IsUSTerritoryTreatedAsDomesticState)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_RL_NKPortOfExportInfo, Declaration.Lookups.Origins);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_RL_NKPortOfExportInfo, Lookups.PortOfExports);
			}

			ValidateUS_SchDExport();
		}

		protected override void CheckUS_SchDExport()
		{
			base.CheckUS_SchDExport();

			var schDExportInfo = Parent.US_SchDExportInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(schDExportInfo, Lookups.ExportRegionDistrictPorts);

			var schDExport = Parent.US_SchDExport;
			var transportMode = Parent.JE_TransportMode;
			if (!schDExport.IsEmpty)
			{
				if (transportMode == RefTransportModeList.Codes.AIR || transportMode == RefTransportModeList.Codes.SEA
					|| transportMode == RefTransportModeList.Codes.ROA || transportMode == RefTransportModeList.Codes.RAI || transportMode == RefTransportModeList.Codes.FIX)
				{
					var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, schDExport);
					query.AddToFilter(Lookups.ExportRegionDistrictPorts.CompleteFilter, JoinCondition.And);
					var port = Parent.Factory.LoadTop1<ZZRefCusCodeListCombined>(query);
					if (port != null && !port.IsTransportModeApplied(transportMode))
					{
						schDExportInfo.AddMessageError(ValidationConstants.Declaration.InvalidPortForTransportMode(transportMode));
					}
				}
			}

			if (schDExport.IsEmpty && Declaration.PortOfExportRefLocoMappings.Count > 1)
			{
				schDExportInfo.AddMessageError(ExportMultipleMatches);
			}
		}

		protected override void CheckUS_SchDArrival()
		{
			base.CheckUS_SchDArrival();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SchDArrivalInfo, Lookups.DischargeSchDList);

			if (Parent.US_SchDArrival.IsEmpty)
			{
				var dec = Declaration;
				if (dec.IsSea)
				{
					Parent.US_SchDArrivalInfo.AddMessageError(EmptyPortOfDischardSea);
				}
				else if (dec.IsAir && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(dec.US_RN_NKCountryOfDestination) == Core.Constants.CountryCodes.UnitedStates)
				{
					Parent.US_SchDArrivalInfo.AddMessageError(EmptyPortOfDischardAirForBetweenUSAndPuertoRico);
				}
			}
			else if (!Declaration.IsSchDArrivalAllowed)
			{
				Parent.US_SchDArrivalInfo.AddMessageError(PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico);
			}
			else
			{
				CheckPortMatchesTransportMode(Parent.US_SchDArrivalInfo);
			}
		}
		internal const string EmptyPortOfDischardSea = "Port Of Discharge for Sea cannot be empty.";
		internal const string EmptyPortOfDischardAirForBetweenUSAndPuertoRico = "Port Of Discharge for Air shipments between U.S. and Puerto Rico cannot be empty.";
		internal const string PortOfDischardIsOnlyAllowedForSeaOrForAirBetweenUSAndPuertoRico = "Port Of Discharge is only allowed for Sea Shipments or for Air shipments between U.S. and Puerto Rico.";

		protected override void CheckUS_RN_NKCountryOfDestination()
		{
			base.CheckUS_RN_NKCountryOfDestination();

			var destCountry = Parent.US_RN_NKCountryOfDestination;
			var propertyInfo = Parent.US_RN_NKCountryOfDestinationInfo;
			if (Declaration.IsUSTerritoryTreatedAsDomesticState && !(Declaration.IsSea || Declaration.IsAir || Declaration.IsPost) && destCountry == Core.Constants.CountryCodes.UnitedStates)
			{
				propertyInfo.AddMessageError(InvalidMOTForPRShipmentToUS);
			}

			if (Parent.US_LicenseType == USAESLicenseCode.Codes.SCA && destCountry != Core.Constants.CountryCodes.Canada)
			{
				propertyInfo.AddMessageError(CountryForSCA);
			}

			if (Parent.US_LicenseType == USAESLicenseCode.Codes.C62 && destCountry != Core.Constants.CountryCodes.Cuba)
			{
				propertyInfo.AddMessageError(CountryForC62);
			}

			if (IsUSTerritoryNotValidForExport(destCountry))
			{
				propertyInfo.AddWarning(DestinationIsUSTerritory);
			}

			AESCountryCodeValidator.ValidateCountryForMyanmar(propertyInfo, destCountry, Parent.US_DateOfExport);
			ListValidation.MessageErrorIfInvalidCode(propertyInfo);

			ValidateUS_SchDArrival();
			ValidateUS_LicenseType();
			Parent.Invoices.OfType<JobComInvoiceHeader>().ToList().ForEach(invoice => invoice.AddInfoValidation.ValidateUS_LicenseType());
			Parent.InvoiceLines.OfType<JobComInvoiceLine>().ToList().ForEach(invLine => invLine.AddInfoValidation.ValidateUS_LicenseType());
		}
		internal const string InvalidMOTForPRShipmentToUS = "Only Mode of Transportation Codes for vessel, air or mail shipments are allowed for a shipment from Puerto Rico to the United States. Verify the method of transportation or Country of Ultimate Destination";
		internal const string CountryForSCA = "The reporting of a Canadian ITAR Exemption (License Type 'SCA') requires that the Country Of Destination be reported as Canada.";
		internal const string CountryForC62 = "The reporting of Support for the Cuban People (License Type 'C62') requires that the Country Of Destination be reported as Cuba.";
		internal const string DestinationIsUSTerritory = "The reporting of Goods to US territories (Guam, American Samoa, Northern Mariana Islands, Wake Island or Midway Island) is not required.";

		internal static bool IsUSTerritoryNotValidForExport(ZString country)
		{
			return !country.IsEmpty && (country == Core.Constants.CountryCodes.Guam ||
				country == Core.Constants.CountryCodes.UnitedStatesMinorIslands ||
				country == Core.Constants.CountryCodes.AmericanSamoa ||
				country == Core.Constants.CountryCodes.NorthernMarianaIslands);
		}
		protected override void CheckUS_StateOfOrigin()
		{
			base.CheckUS_StateOfOrigin();

			if (Declaration.IsUSTerritoryTreatedAsDomesticState &&
				Parent.US_StateOfOrigin != Core.Constants.CountryCodes.PuertoRico &&
				Parent.US_StateOfOrigin != Core.Constants.CountryCodes.VirginIslands)
			{
				Parent.US_StateOfOriginInfo.AddMessageError(StateOfOriginMustBePuertoRicoOrVirginIslands);
			}

			if (Parent.US_RN_NKCountryOfDestination == Core.Constants.CountryCodes.VirginIslands && !IsOriginAUSState(Parent.US_StateOfOrigin))
			{
				Parent.US_StateOfOriginInfo.AddMessageError(StateOfOriginMustBeAUSState);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.US_StateOfOriginInfo, Parent.AddInfoLookups.USStateList, (NoResString)ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}
		internal const string StateOfOriginMustBePuertoRicoOrVirginIslands = "Only the U.S. State of Origin Code for Puerto Rico (PR) or Virgin Island (VI) can be reported as the state code for a shipment with ultimate country of destination of the United States.";
		internal const string StateOfOriginMustBeAUSState = "When the Country of Ultimate Destination is the Virgin Islands (VI), the US state of origin code must be any valid two-character US state code or Puerto Rico (PR).";

		bool IsOriginAUSState(string stateOfOrigin)
		{
			return Parent.AddInfoLookups.USStateList.ContainsCode(stateOfOrigin);
		}

		protected override bool IsCarrierSCACRequired
		{
			get
			{
				return base.IsCarrierSCACRequired || Declaration.IsTruck;
			}
		}

		protected override void CheckUS_DDTCITARExemptionNo()
		{
			base.CheckUS_DDTCITARExemptionNo();
			CheckUS_DDTCITARExemptionNoCore(Parent.US_DDTCITARExemptionNoInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCRegistrationNo()
		{
			base.CheckUS_DDTCRegistrationNo();
			CheckUS_DDTCRegistrationNoCore(Parent.US_DDTCRegistrationNoInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCMilitaryEquipmentIndicator()
		{
			base.CheckUS_DDTCMilitaryEquipmentIndicator();
			CheckUS_DDTCMilitaryEquipmentIndicatorCore(Parent.US_DDTCMilitaryEquipmentIndicatorInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCPartyCertificationIndicator()
		{
			base.CheckUS_DDTCPartyCertificationIndicator();
			CheckUS_DDTCPartyCertificationIndicatorCore(Parent.US_DDTCPartyCertificationIndicatorInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCUSMLCategoryCode()
		{
			base.CheckUS_DDTCUSMLCategoryCode();
			CheckUS_DDTCUSMLCategoryCodeCore(Parent.US_DDTCUSMLCategoryCodeInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_JurisdictionNumber()
		{
			base.CheckUS_JurisdictionNumber();
			CheckUS_JurisdictionNumberCore(Parent.US_JurisdictionNumberInfo, Parent.US_JurisdictionNumber, Parent.US_DDTCUSMLCategoryCode);
		}

		protected override void CheckUS_HazardousCargo()
		{
			base.CheckUS_HazardousCargo();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_HazardousCargoInfo, Lookups.US_YesNoList);
		}

		protected override void CheckUS_TransactionsRelated()
		{
			base.CheckUS_TransactionsRelated();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TransactionsRelatedInfo, Lookups.US_YesNoList);
		}

		protected override void CheckUS_RoutedTransaction()
		{
			base.CheckUS_RoutedTransaction();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_RoutedTransactionInfo, Lookups.US_YesNoList);

			Parent.Validation.ValidateJE_OH_Supplier();
		}

		protected override void CheckUS_InbondType()
		{
			base.CheckUS_InbondType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InbondTypeInfo, Lookups.US_InbondType_List, (NoResString)InbondTypeShouldBeInList);
		}
		internal const string InbondTypeShouldBeInList = "Please enter a valid IT Type. The code you have selected is not in the In-Bond Types List.";

		protected override void CheckUS_ExportCode()
		{
			base.CheckUS_ExportCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ExportCodeInfo, Lookups.US_ExportCode_List);
			if (!Parent.US_LicenseType.IsEmpty || !Parent.US_ExportCode.IsEmpty)
			{
				var exportCodeError = LicenseValidationHelper.GetExportCodeError(Parent.US_LicenseType, Parent.US_ExportCode);
				if (!exportCodeError.IsEmpty)
				{
					Parent.US_ExportCodeInfo.AddMessageError(exportCodeError);
				}
			}
		}

		protected override void CheckUS_LicenseNo()
		{
			base.CheckUS_LicenseNo();

			var parent = Parent;
			if (!parent.US_LicenseType.IsEmpty || !parent.US_LicenseNo.IsEmpty)
			{
				var licenseNoError = LicenseValidationHelper.GetLicenseNumberError(parent.US_LicenseType, parent.US_LicenseNo, parent.Factory, parent.GetEffectiveDateForECR());
				if (!licenseNoError.IsEmpty)
				{
					parent.US_LicenseNoInfo.AddMessageError(licenseNoError);
				}

				if (parent.US_LicenseType == USAESLicenseCode.Codes.C30 || parent.US_LicenseType == USAESLicenseCode.Codes.C31 || parent.US_LicenseType == USAESLicenseCode.Codes.C51)
				{
					if (!parent.US_LicenseNoInfo.HasNotifications())
					{
						parent.US_LicenseNoInfo.AddWarning(BISLicenseWarning);
					}
				}
			}
		}

		internal const string BISLicenseWarning = "BIS license numbers or notice confirmation numbers reported under License Types C30, C31 and C51 must be valid and must not be expired.";

		protected override void CheckUS_ECCN()
		{
			base.CheckUS_ECCN();
			var parent = Parent;
			var factory = parent.Factory;
			var exportDate = parent.GetEffectiveDateForECR();
			var licenseType = parent.US_LicenseType;
			var eccnError = LicenseValidationHelper.GetECCNError(licenseType, parent.US_ECCN, factory, exportDate, destinationCountry: parent.US_RN_NKCountryOfDestination);
			if (!eccnError.IsEmpty)
			{
				parent.US_ECCNInfo.AddMessageError(eccnError);
			}
		}

		protected override void CheckUS_OriginalITNNumber()
		{
			base.CheckUS_OriginalITNNumber();
			if (!Parent.US_OriginalITNNumber.IsEmpty)
			{
				ExportFieldsValidator.ValidateOriginalTINFormat(Parent.US_OriginalITNNumberInfo);
			}
		}

		protected override void CheckUS_CommodityFilingOption()
		{
			base.CheckUS_CommodityFilingOption();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CommodityFilingOptionInfo, Lookups.US_CommodityFilingOptions);

			if (Declaration.IsUSMLEntry && Parent.US_CommodityFilingOption == AESCommodityFilingOptionList.Codes._4Postdeparture)
			{
				Parent.US_CommodityFilingOptionInfo.AddError(USMLCannotBePostDepartureFiling); // error used (as distinct from message error) due to specific request in AES certification test cases to build into software edit to prevent post-departure filing for USML shipments
			}

			ValidateUS_DateOfExport();
		}
		internal const string USMLCannotBePostDepartureFiling = "USML shipments are not allowed to be filed on a post-departure basis.";

		protected override void CheckUS_SoldEnRouteIndicator()
		{
			base.CheckUS_SoldEnRouteIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SoldEnRouteIndicatorInfo, Lookups.US_YesNoList);
		}

		protected override void CheckUS_RN_NKFirstPortOfCallCountry()
		{
			base.CheckUS_RN_NKFirstPortOfCallCountry();

			if (Parent.IsSoldEnRoute)
			{
				if (Parent.US_RN_NKFirstPortOfCallCountry.IsEmpty)
				{
					Parent.US_RN_NKFirstPortOfCallCountryInfo.AddMessageError(FirstPortOfCallCountryRequired);
				}
			}
		}
		internal const string FirstPortOfCallCountryRequired = "If goods are Sold En Route, the Country of First Port of Call is required.";

		protected override void CheckUS_FirstPortOfCallCity()
		{
			base.CheckUS_FirstPortOfCallCity();

			if (Parent.IsSoldEnRoute)
			{
				if (Parent.US_FirstPortOfCallCity.IsEmpty)
				{
					Parent.US_FirstPortOfCallCityInfo.AddMessageError(FirstPortOfCallCityRequired);
				}
			}
		}
		internal const string FirstPortOfCallCityRequired = "If goods are Sold En Route, the City of First Port of Call is required.";

		protected override void CheckUS_ForeignTradeZone()
		{
			base.CheckUS_ForeignTradeZone();
			if (!Parent.US_ForeignTradeZone.IsEmpty)
			{
				ExportFieldsValidator.ValidateFTZIndicatorFormat(Parent.US_ForeignTradeZoneInfo);
			}
		}

		protected override void CheckUS_CarrierName()
		{
			base.CheckUS_CarrierName();
			if (Parent.IsUnknownCarrierSCACForExport && Parent.US_CarrierName.IsEmpty)
			{
				Parent.US_CarrierNameInfo.AddMessageError(ValidationConstants.Declaration.CarrierNameRequired);
			}
		}
	}
}
