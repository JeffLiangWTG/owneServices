using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.LicenseManager;

namespace Enterprise.Customs.US.Business
{
	public class ExportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ExportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent.Parent; }
		}

		protected override void CheckUS_JurisdictionNumber()
		{
			base.CheckUS_JurisdictionNumber();
			CheckUS_JurisdictionNumberCore(Parent.US_JurisdictionNumberInfo, Parent.US_JurisdictionNumber, Parent.US_DDTCUSMLCategoryCode);
		}

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

			Parent.Validation.ValidateJI_Tariff();
			Parent.Validation.ValidateJI_CustomsQuantity();
			Parent.Validation.ValidateJI_CustomsSecondQuantity();
		}

		protected override void CheckUS_LicenseType()
		{
			var parent = Parent;
			var licenseType = parent.US_LicenseType;
			if (licenseType.IsEmpty)
			{
				parent.US_LicenseTypeInfo.AddMessageError(LicenseTypeRequired);
			}
			else
			{
				var invoiceHeader = parent.InvoiceHeader;
				var declaration = invoiceHeader?.JobDeclaration;
				var factory = parent.Factory;
				var exportDate = parent.ExportDateForLicenseType;
				var licenseTypeBO = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, parent.US_LicenseType, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, exportDate);
				if (licenseTypeBO == null)
				{
					parent.US_LicenseTypeInfo.AddMessageError(ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
				}
				else
				{
					if (declaration != null)
					{
						var transportModeError = LicenseValidationHelper.GetTransportModeError(licenseType, declaration.JE_TransportMode, factory, exportDate);
						if (!transportModeError.IsEmpty)
						{
							parent.US_LicenseTypeInfo.AddMessageError(transportModeError);
						}

						var errorMessage = LicenseValidationHelper.AddLicenseTypeValidationErrorMessage(declaration.US_RN_NKCountryOfDestination, licenseType);
						if (!string.IsNullOrEmpty(errorMessage))
						{
							parent.US_LicenseTypeInfo.AddMessageError(errorMessage);
						}
					}
				}

				ValidateUS_LicenseNo();
				ValidateUS_LicenseValue();
				ValidateUS_ECCN();
				ValidateUS_ExportCode();
				ValidateUS_DDTCITARExemptionNo();
				ValidateUS_DDTCMilitaryEquipmentIndicator();
				ValidateUS_DDTCPartyCertificationIndicator();
				ValidateUS_DDTCRegistrationNo();
				ValidateUS_DDTCUnit();
				ValidateUS_DDTCQuantity();
				ValidateUS_DDTCUnit();
				Parent.Validation.ValidateJI_Description();
			}
		}
		internal const string LicenseTypeRequired = "License Type is required, please enter it either on the Invoice Line, Invoice Header or Declaration.";

		protected override void CheckUS_IsUsedVehicle()
		{
			base.CheckUS_IsUsedVehicle();

			var parent = Parent;
			if (parent.US_IsUsedVehicle)
			{
				if (parent.US_ExportCode == ExportInformationCodeList.Codes.HH)
				{
					parent.US_IsUsedVehicleInfo.AddMessageError(VehicleInfoNotAllowed);
				}

				if (parent.Declaration.US_CommodityFilingOption == AESCommodityFilingOptionList.Codes._4Postdeparture && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(parent.Declaration.US_RN_NKCountryOfDestination) != Core.Constants.CountryCodes.UnitedStates)
				{
					parent.US_IsUsedVehicleInfo.AddMessageError(VehicleInfoNotAllowedForPostDepartureNotUSOrPR);
				}
			}
			else 
			{
				if (parent.US_ExportCode == ExportInformationCodeList.Codes.HV)
				{
					parent.US_IsUsedVehicleInfo.AddMessageError(VehicleInfoRequired);
				}

				if (parent.TariffRequiresVehicleReporting)
				{
					parent.US_IsUsedVehicleInfo.AddMessageError(TariffNumberRequiresUsedVehicleReporting);
				}
			}

			ValidateAllUsedVehicleDataAreEntered();
			Parent.Validation.ValidateJI_CustomsQuantity();
		}
		internal const string VehicleInfoNotAllowed = "A used vehicle cannot be reported as household goods under Export Information Code HH. Personal used vehicles may be reported under Export Information Code HV.";
		internal const string VehicleInfoRequired = "Used vehicle information is required if Export Information Code is HV.";
		internal const string VehicleInfoNotAllowedForPostDepartureNotUSOrPR = "Used vehicle information cannot be reported on a Post-Departure lodgement if the Country of Ultimate Destination is not the United States or Puerto Rico.";
		internal const string TariffNumberRequiresUsedVehicleReporting = "Tariff number requires used vehicle reporting.";

		void ValidateAllUsedVehicleDataAreEntered()
		{
			ValidateUS_VehicleIDType();
			ValidateUS_VehicleID();
			ValidateUS_VehicleTitleNo();
			ValidateUS_VehicleTitleState();
		}

		protected override void CheckUS_VehicleID()
		{
			base.CheckUS_VehicleID();
			if (Parent.US_IsUsedVehicle)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VehicleIDInfo, "Vehicle ID for Used Vehicle");
			}
		}

		protected override void CheckUS_VehicleIDType()
		{
			base.CheckUS_VehicleIDType();
			if (Parent.US_IsUsedVehicle)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VehicleIDTypeInfo, "Vehicle ID Type for Used Vehicle");
				ListValidation.MessageErrorIfInvalidCode(Parent.US_VehicleIDTypeInfo, Lookups.US_VehicleIDType_List);
			}

			ValidateUS_VehicleTitleNo();
			ValidateUS_VehicleTitleState();
		}

		protected override void CheckUS_VehicleTitleState()
		{
			base.CheckUS_VehicleTitleState();
			if (Parent.US_IsUsedVehicle && Parent.US_VehicleIDType == VehicleIDTypeList.Codes.VIN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VehicleTitleStateInfo, "Vehicle Title State for Used Vehicle");
				ListValidation.MessageErrorIfInvalidCode(Parent.US_VehicleTitleStateInfo, Lookups.USStateList);
			}

			ValidateUS_VehicleTitleNo();
		}

		protected override void CheckUS_VehicleTitleNo()
		{
			base.CheckUS_VehicleTitleNo();
			if (Parent.US_IsUsedVehicle && Parent.US_VehicleIDType == VehicleIDTypeList.Codes.VIN && Parent.US_VehicleTitleState != "US")
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VehicleTitleNoInfo, "Vehicle Title No for Used Vehicle");
			}
		}

		protected override void CheckUS_AESOriginIndicator()
		{
			base.CheckUS_AESOriginIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_AESOriginIndicatorInfo, Lookups.US_AESOriginIndicator_List);
			if (Parent.US_ExportCode == ExportInformationCodeList.Codes.HH)
			{
				if (!Parent.US_AESOriginIndicator.IsEmpty)
				{
					Parent.US_AESOriginIndicatorInfo.AddMessageError(AESOriginIndicatorMustBeBlank);
				}
			}
			else if (Parent.US_AESOriginIndicator.IsEmpty)
			{
				if (Parent.InvoiceHeader?.AllowOriginIndicatorToBeEntered ?? false)
				{
					Parent.US_AESOriginIndicatorInfo.AddMessageError(AESOriginIndicatorRequired);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_AESOriginIndicatorInfo);
				}
			}
		}
		internal const string AESOriginIndicatorRequired = "Origin Indicator is required, please enter it either on the Invoice Line or Invoice Header.";
		internal const string AESOriginIndicatorMustBeBlank = "The Domestic/Foreign Indicator is not allowed to be reported when the Export Information Code is HH for household goods.";

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

		protected override void CheckUS_DDTCUnit()
		{
			base.CheckUS_DDTCUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DDTCUnitInfo, Lookups.US_DDTCUnitOfMeasureList);
			var licenseType = Parent.US_LicenseType;
			if (USAESLicenseCode.IsDDTCDataRequired(licenseType))
			{
				if (Parent.US_DDTCUnit.IsEmpty)
				{
					Parent.US_DDTCUnitInfo.AddMessageError(DDTCUnitIsRequiredMessage);
				}
			}
			else
			{
				if (!Parent.US_DDTCUnit.IsEmpty)
				{
					Parent.US_DDTCUnitInfo.AddMessageError(DDTCUnitShouldNotBeEntered);
				}
			}
		}
		internal const string DDTCUnitIsRequiredMessage = "DDTC Unit of Measure is required when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
		internal const string DDTCUnitShouldNotBeEntered = "DDTC Unit of Measure should only be entered when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";

		protected override void CheckUS_DDTCQuantity()
		{
			base.CheckUS_DDTCQuantity();
			ZString licenseType = Parent.US_LicenseType;
			if (USAESLicenseCode.IsDDTCDataRequired(licenseType))
			{
				if (Parent.US_DDTCQuantity.IsEmpty)
				{
					Parent.US_DDTCQuantityInfo.AddMessageError(DDTCQuantityIsRequiredMessage);
				}
			}
			else
			{
				if (!Parent.US_DDTCQuantity.IsEmpty)
				{
					Parent.US_DDTCQuantityInfo.AddMessageError(DDTCQuantityShouldNotBeEntered);
				}
			}
		}
		internal const string DDTCQuantityIsRequiredMessage = "DDTC Quantity is required when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";
		internal const string DDTCQuantityShouldNotBeEntered = "DDTC Quantity should only be entered when License Type is 'SAG', 'SAU' 'SCA', 'SGB', 'S00', 'S05', 'S61', 'S73', 'S85', 'S94' or 'VDS'.";

		protected override void CheckUS_LicenseNo()
		{
			base.CheckUS_LicenseNo();
			var parent = Parent;
			if (!parent.US_LicenseType.IsEmpty || !parent.US_LicenseNo.IsEmpty)
			{
				var licenseNoError = LicenseValidationHelper.GetLicenseNumberError(parent.US_LicenseType, parent.US_LicenseNo, parent.Factory, parent.ExportDateForLicenseType);

				if (!licenseNoError.IsEmpty)
				{
					parent.US_LicenseNoInfo.AddMessageError(licenseNoError);
				}
			}

			ValidateUS_LicenseValue();
		}

		protected override void CheckUS_LicenseValue()
		{
			base.CheckUS_LicenseValue();

			var parent = Parent;
			MandatoryValidation.CheckNotNegative(parent.US_LicenseValueInfo);

			if (USAESLicenseCode.IsLicenseValueRequired(parent.US_LicenseType, parent.Factory, parent.ExportDateForLicenseType) &&
				!parent.US_LicenseNo.IsEmpty && parent.US_LicenseNo != LicenseExemptionTypeList.Codes.NLR &&
				parent.US_LicenseValue.IsEmpty)
			{
				parent.US_LicenseValueInfo.AddMessageError(MissingLicenseValue);
			}

			if (parent.US_LicenseValue > parent.JI_LinePrice && !(parent.UseScheduleB && parent.JI_Tariff.Equals("9801100000")))
			{
				parent.US_LicenseValueInfo.AddMessageError(GreaterThanLineValue);
			}
		}
		internal const string MissingLicenseValue = "License Value is required if License No is entered for certain License Types.";
		internal const string GreaterThanLineValue = "License Value cannot exceed Line Value.";

		protected override void CheckUS_ECCN()
		{
			base.CheckUS_ECCN();

			var parent = Parent;
			var invoiceHeader = parent.InvoiceHeader;
			var declaration = parent.Declaration;
			var factory = parent.Factory;
			var exportDate = parent.ExportDateForLicenseType;
			var licenseType = parent.US_LicenseType;
			var eccn = parent.US_ECCN;

			var countryCodeForUC = invoiceHeader?.UltimateConsigneeDocAddress.E2_RN_NKCountryCode ?? ZString.Empty;
			var countryCodeForIC = invoiceHeader?.IntermediateConsigneeDocAddress.E2_RN_NKCountryCode ?? ZString.Empty;
			var countryCodeForUD = invoiceHeader?.US_UltimateDestinationCountry ?? ZString.Empty;

			var errorForUC = LicenseValidationHelper.GetECCNError(licenseType, eccn, factory, exportDate, countryCodeForUC);
			var errorForIC = LicenseValidationHelper.GetECCNError(licenseType, eccn, factory, exportDate, countryCodeForIC);
			var errorForUD = LicenseValidationHelper.GetECCNError(licenseType, eccn, factory, exportDate, countryCodeForUD, countryCodeForUD);

			if (!errorForUC.IsEmpty)
			{
				parent.US_ECCNInfo.AddMessageError(errorForUC);
			}
			else if (!errorForIC.IsEmpty)
			{
				parent.US_ECCNInfo.AddMessageError(errorForIC);
			}
			else if (!errorForUD.IsEmpty)
			{
				parent.US_ECCNInfo.AddMessageError(errorForUD);
			}

			Parent.Validation.ValidateJI_Description();
		}

		protected override void CheckUS_HazWasteTrackingNo()
		{
			base.CheckUS_HazWasteTrackingNo();

			if (!Parent.US_HazWasteTrackingNo.IsEmpty && !Regex.IsMatch(Parent.US_HazWasteTrackingNo, "[0-9]{9}[a-z]{3}", RegexOptions.IgnoreCase))
			{
				Parent.US_HazWasteTrackingNoInfo.AddMessageError(InvalidHazWasteTrackingNoFormat);
			}

			if (Parent.IsExportEPADeclared && Parent.US_HazWasteTrackingNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_HazWasteTrackingNoInfo);
			}

			ValidateUS_ExportCertificateNo();
		}
		internal const string InvalidHazWasteTrackingNoFormat = "The RCRA HW Manifest Tracking Number must be specified in the required format: first 9 characters must be numeric and the last 3 characters must be alphabetic.";

		protected override void CheckUS_EPAConsentNumber()
		{
			base.CheckUS_EPAConsentNumber();

			if (Parent.IsExportEPADeclared && Parent.US_EPAConsentNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EPAConsentNumberInfo);
			}

			ValidateUS_ExportCertificateNo();
		}

		protected override void CheckUS_EPANetQty()
		{
			base.CheckUS_EPANetQty();

			if (Parent.IsExportEPADeclared)
			{
				if (Parent.US_EPANetQty.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EPANetQtyInfo);
				}
				else if (Parent.US_EPANetQty < 0m)
				{
					Parent.US_EPANetQtyInfo.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
				}
			}

			ValidateUS_ExportCertificateNo();
		}

		protected override void CheckUS_EPANetQtyUQ()
		{
			base.CheckUS_EPANetQtyUQ();

			if (Parent.IsExportEPADeclared)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_EPANetQtyUQInfo, Lookups.EPANetQtyUQList);

				if (Parent.US_EPANetQtyUQ.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EPANetQtyUQInfo);
				}
			}

			ValidateUS_ExportCertificateNo();
		}

		protected override void CheckUS_ExportCertificateNo()
		{
			base.CheckUS_ExportCertificateNo();

			if (Parent.IsAMSDeclared && Parent.US_ExportCertificateNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ExportCertificateNoInfo);
			}

			ValidateUS_EPAConsentNumber();
			ValidateUS_HazWasteTrackingNo();
			ValidateUS_EPANetQty();
			ValidateUS_EPANetQtyUQ();
		}

		protected override void CheckUS_AMSInd()
		{
			base.CheckUS_AMSInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_AMSIndInfo, Parent.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList);

			AgencyRequirementsValidator.ValidatePGA(Parent.US_AMSIndInfo, GovernmentAgencyProgramCodeList.Codes.AMS, true, Parent.ExportPGARequirementIndicator.RequireAMS);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.AMS, Parent.ExportPGAAgencyRequirements);
		}

		protected override void CheckUS_PSTIndicator()
		{
			base.CheckUS_PSTIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PSTIndicatorInfo, Parent.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList);
			AgencyRequirementsValidator.ValidatePGA(Parent.US_PSTIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.EPA, true, Parent.ExportPGARequirementIndicator.HasEPARequirement);
			AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.US_PSTIndicatorInfo, GovernmentAgencyProgramCodeList.Codes.EPA, Parent.PSTLines.Count, Parent.ExportPGARequirementIndicator.RequireEPA);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.EPA, Parent.ExportPGAAgencyRequirements);
		}

		protected override void CheckUS_NMFSHMSInd()
		{
			base.CheckUS_NMFSHMSInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NMFSHMSIndInfo, Parent.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList);

			AgencyRequirementsValidator.ValidatePGA(Parent.US_NMFSHMSIndInfo, GovernmentAgencyProgramCodeList.NMFS, true, Parent.ExportPGARequirementIndicator.RequireNMFS);
			AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.US_NMFSHMSIndInfo, GovernmentAgencyProgramCodeList.NMFS, Parent.NMFSLines.Count);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.NMFS, Parent.ExportPGAAgencyRequirements);
		}

		protected override void CheckUS_ATFInd()
		{
			base.CheckUS_ATFInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ATFIndInfo, Parent.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList);

			AgencyRequirementsValidator.ValidatePGA(Parent.US_ATFIndInfo, GovernmentAgencyProgramCodeList.Codes.ATF, true, Parent.ExportPGARequirementIndicator.RequireATF);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.ATF, Parent.ExportPGAAgencyRequirements);
		}

		protected override void CheckUS_DEAInd()
		{
			base.CheckUS_DEAInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DEAIndInfo, Parent.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList);

			AgencyRequirementsValidator.ValidatePGA(Parent.US_DEAIndInfo, GovernmentAgencyProgramCodeList.Codes.DEA, true, Parent.ExportPGARequirementIndicator.MayRequireDEA);
			AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.US_DEAIndInfo, GovernmentAgencyProgramCodeList.Codes.DEA, Parent.DEAHeaders.Count, Parent.ExportPGARequirementIndicator.RequireDEA);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.DEA, Parent.ExportPGAAgencyRequirements);
		}

		protected override void CheckUS_FWSInd()
		{
			base.CheckUS_FWSInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FWSIndInfo, Parent.AddInfoLookups.US_OGAIndicatorWithoutDisclaimerList);

			AgencyRequirementsValidator.ValidatePGA(Parent.US_FWSIndInfo, GovernmentAgencyProgramCodeList.Codes.FWS, true, Parent.ExportPGARequirementIndicator.RequireFWS);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.FWS, Parent.ExportPGAAgencyRequirements);
		}

		protected override void CheckUS_TTBInd()
		{
			base.CheckUS_TTBInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TTBIndInfo, Parent.AddInfoLookups.US_OGAIndicatorListWithDisclaimerList);

			AgencyRequirementsValidator.ValidatePGA(Parent.US_TTBIndInfo, GovernmentAgencyProgramCodeList.Codes.TTB, true, Parent.ExportPGARequirementIndicator.RequireTTB);
			AgencyRequirementsValidator.ValidatePGAIndicatorAndDataForExport(Parent.US_TTBIndInfo, GovernmentAgencyProgramCodeList.Codes.TTB, Parent.TTBLines.Count, Parent.ExportPGARequirementIndicator.RequireTTB);
			AgencyRequirementsValidator.ValidateOGAAgencyRequirements(GovernmentAgencyProgramCodeList.Codes.TTB, Parent.ExportPGAAgencyRequirements);
		}
	}
}
