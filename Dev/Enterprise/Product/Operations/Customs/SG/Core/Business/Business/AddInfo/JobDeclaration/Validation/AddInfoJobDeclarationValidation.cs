using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclarationValidation : SGAddInfoValidation
	{
		public AddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		#region Overrides

		protected override void CheckSG_ClaimantCode()
		{
			base.CheckSG_ClaimantCode();

			if (!Parent.SG_ClaimantName.IsEmpty || !Declaration.JE_OH_Claimant.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_ClaimantCodeInfo, "Claimant Code");
			}

			if (!Parent.SG_ClaimantCode.IsEmpty)
			{
				if (!ClaimantCodeOK(Parent.SG_ClaimantCode))
				{
					Parent.SG_ClaimantCodeInfo.AddWarning("Claimant Code should start with 'S', T', 'M' or 'P'");
				}
			}
		}

		bool ClaimantCodeOK(string claimantCodeOK)
		{
			return claimantCodeOK.StartsWith("S") || claimantCodeOK.StartsWith("T") || claimantCodeOK.StartsWith("M") || claimantCodeOK.StartsWith("P");
		}

		protected override void CheckSG_ClaimantName()
		{
			base.CheckSG_ClaimantName();

			if (!Parent.SG_ClaimantCode.IsEmpty || !Declaration.JE_OH_Claimant.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_ClaimantNameInfo, "Claimant Name");
			}
		}

		protected override void CheckSG_DutyExempt()
		{
			base.CheckSG_DutyExempt();
			if (Parent.Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.GST ||
				Parent.Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT)
			{
				if (Parent.SG_ClaimantCode.IsEmpty)
				{
					if (Parent.SG_DutyExempt)
					{
						Parent.SG_DutyExemptInfo.AddMessageError("Duty Exemption is not valid unless a Claimant is entered");
					}
				}
			}
			else
			{
				if (Parent.SG_DutyExempt)
				{
					Parent.SG_DutyExemptInfo.AddMessageError("Duty Exemption is not valid for this type of declaration");
				}
			}
		}

		protected override void CheckSG_OutwardVoyageFlightNo()
		{
			base.CheckSG_OutwardVoyageFlightNo();
			ValidateSG_OutwardMAWB();
		}

		protected override void CheckSG_OutwardMAWB()
		{
			base.CheckSG_OutwardMAWB();

			if (!Parent.Declaration.IsStandAloneCertificateOfOrigin)
			{
				if (!Parent.SG_IsOutwardHandCarried && (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA || Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air))
				{
					var billType = Parent.Declaration.IsOutwardTransportModeSea ? "Outward Ocean Bill Number" : "Outward Master Bill Number";
					MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardMAWBInfo, billType);
				}

				if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
				{
					if (Declaration != null && !Declaration.IsExWarehouse)
					{
						if (Parent.SG_OutwardVoyageFlightNo.Length >= 2)
						{
							RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Parent.Factory, Parent.SG_OutwardVoyageFlightNo.Left(2));

							if (airline != null && !Parent.SG_IsOutwardHandCarried && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != Parent.SG_OutwardMAWB.Left(3))
							{
								Parent.SG_OutwardMAWBInfo.AddWarning("The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.");
							}
						}
					}
				}
			}
		}

		protected override void CheckSG_OutwardTransportMode()
		{
			base.CheckSG_OutwardTransportMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_OutwardTransportModeInfo, Parent.Lookups.TransportTypeList);
		}

		protected override void CheckSG_US_NKInwardVesselBerth()
		{
			if (!Declaration.IsTradeNet4Point1)
			{
				base.CheckSG_US_NKInwardVesselBerth();
				ListValidation.MessageErrorIfInvalidCode(Parent.SG_US_NKInwardVesselBerthInfo, Parent.Lookups.SGCPlacesList, (NoResString)"Please select a valid Inward Vessel Berth");
			}
		}

		protected override void CheckSG_US_NKOutwardVesselBerth()
		{
			if (!Declaration.IsTradeNet4Point1)
			{
				base.CheckSG_US_NKOutwardVesselBerth();
				ListValidation.MessageErrorIfInvalidCode(Parent.SG_US_NKOutwardVesselBerthInfo, Parent.Lookups.SGCPlacesList, (NoResString)"Please select a valid Outward Vessel Berth");
			}
		}

		protected override void CheckSG_RN_NKFinalDestination()
		{
			base.CheckSG_RN_NKFinalDestination();

			if (Parent.SG_RN_NKFinalDestination != Core.Constants.CountryCodes.NetherlandsAntilles)
			{
				if (Parent.SG_RN_NKFinalDestination == Core.Constants.CountryCodes.Curacao)
				{
					Parent.SG_RN_NKFinalDestinationInfo.AddWarning(CuracaoNotUsedBySG);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.SG_RN_NKFinalDestinationInfo, Parent.Lookups.FinalDestinations, (NoResString)"Please select a valid Country/Region");
					if (!Declaration.HasBeenCleared)
					{
						if (Parent.SG_RN_NKFinalDestination.StartsWith(Core.Constants.CountryCodes.KoreaNorth) ||
							Parent.SG_RN_NKFinalDestination.StartsWith(Core.Constants.CountryCodes.Iran))
						{
							Parent.SG_RN_NKFinalDestinationInfo.AddWarning(JobDeclarationValidation.Circular18_2010);
						}
					}
				}
			}
		}
		internal const string CuracaoNotUsedBySG = "The country code of CW for Curacao is not yet used by Singapore Customs. You still need to use the old code reference of 'AN' for the Netherlands Antilles.";   // Note error message is for a specific country.

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return base.ShouldValidateFKToCancelledRecord(info) && info.Name != SGAddInfoSchema.Constants.SG_RN_NKFinalDestination;
		}

		protected override void CheckSG_US_NKPlaceOfCargoRelease()
		{
			base.CheckSG_US_NKPlaceOfCargoRelease();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_US_NKPlaceOfCargoReleaseInfo, Parent.Lookups.SGCPlacesList, (NoResString)"Please select a valid Place of Release");
			var placeOfRelease = Declaration.PlaceOfRelease;
			if (placeOfRelease != null)
			{
				if (!placeOfRelease.IsValidForGUIUse())
				{
					Parent.SG_US_NKPlaceOfCargoReleaseInfo.AddMessageError("This is not a valid Place. A valid Place is a Place other than 'O', 'SC', 'SY', 'BW', 'BWCY', 'LW'");
				}
				else if (placeOfRelease.IsMajorExporterSchemeExemptionCode())
				{
					Parent.SG_US_NKPlaceOfCargoReleaseInfo.AddMessageError("Major Exporter Scheme exemption code is not valid for Place of Release");
				}
				else if (placeOfRelease.ShouldUseFTZ())
				{
					Parent.SG_US_NKPlaceOfCargoReleaseInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
				}
				else if (placeOfRelease.IsApprovedImportGSTSuspensionSchemeExemptionCode())
				{
					Parent.SG_US_NKPlaceOfCargoReleaseInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.AISSNotValidForRelease);
				}
				else if (placeOfRelease.IsImportGSTDefermentSchemeExemptionCode())
				{
					Parent.SG_US_NKPlaceOfCargoReleaseInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.IGDSNotValidForRelease);
				}
			}
		}

		protected override void CheckSG_US_NKPlaceOfReceipt()
		{
			base.CheckSG_US_NKPlaceOfReceipt();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_US_NKPlaceOfReceiptInfo, Parent.Lookups.SGCPlacesList, (NoResString)"Please select a valid Place of Receipt");
			var placeOfReceipt = Declaration.PlaceOfReceipt;
			if (placeOfReceipt != null)
			{
				if (!placeOfReceipt.IsValidForGUIUse())
				{
					Parent.SG_US_NKPlaceOfReceiptInfo.AddMessageError("This is not a valid Place. A valid Place is a Place other than 'O', 'SC', 'SY', 'BW', 'BWCY', 'LW'");
				}
				else if (placeOfReceipt.IsMajorExporterSchemeExemptionCode() && Parent.Declaration.IsExport)
				{
					Parent.SG_US_NKPlaceOfReceiptInfo.AddMessageError("Major Exporter Scheme exemption code is not valid for this type of Declaration");
				}
				else if (placeOfReceipt.ShouldUseFTZ())
				{
					Parent.SG_US_NKPlaceOfReceiptInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.UseFTZPlaceOfReceiptRelease);
				}
				else if (placeOfReceipt.IsApprovedImportGSTSuspensionSchemeExemptionCode())
				{
					if (Parent.Declaration.JE_MessageType != MessageTypeCodeList.Codes.INP || Parent.Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.APS)
					{
						Parent.SG_US_NKPlaceOfReceiptInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.AISSInvalidUseForReceipt);
					}
				}
				else if (placeOfReceipt.IsImportGSTDefermentSchemeExemptionCode())
				{
					if (Parent.Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP)
					{
						if (Parent.Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.APS && Parent.Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.GTR)
						{
							Parent.SG_US_NKPlaceOfReceiptInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.IGDSCorrectUsageForINP);
						}
					}
					else if (Parent.Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT)
					{
						if (Parent.Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.DUT)
						{
							Parent.SG_US_NKPlaceOfReceiptInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.IGDSCorrectUsageForIPT);
						}
					}
					else
					{
						Parent.SG_US_NKPlaceOfReceiptInfo.AddMessageError(SGCPlaces.Constants.ReceiptRelease.IGDSInvalidUseForReceipt);
					}
				}
			}
		}

		protected override void CheckSG_US_NKPlaceOfStorage()
		{
			base.CheckSG_US_NKPlaceOfStorage();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_US_NKPlaceOfStorageInfo, Parent.Lookups.SGCPlacesList, (NoResString)"Please select a valid Place of Storage");
			var placeOfStorage = Declaration.PlaceOfStorage;
			if (placeOfStorage != null)
			{
				if (!placeOfStorage.IsValidForGUIUse())
				{
					Parent.SG_US_NKPlaceOfStorageInfo.AddMessageError("This is not a valid Place. A valid Place is a Place other than 'O', 'SC', 'SY', 'BW', 'BWCY', 'LW'");
				}
				else if (placeOfStorage.IsMajorExporterSchemeExemptionCode())
				{
					Parent.SG_US_NKPlaceOfStorageInfo.AddMessageError("Major Exporter Scheme exemption code is not valid for Place of Storage");
				}
			}
		}

		protected override void CheckSG_OutwardVesselName()
		{
			base.CheckSG_OutwardVesselName();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_OutwardVesselNameInfo, Declaration.Lookups.Vessels);
			if (Parent.OutwardVessel != null)
			{
				if (Parent.OutwardVessel.RV_VesselType.IsEmpty)
				{
					Parent.SG_OutwardVesselNameInfo.AddMessageError("Please enter Vessel Type for the outward vessel.");
				}

				if (Parent.Declaration.IsSeaStore && Parent.Declaration.HasLiquorOrTobacco)
				{
					if (Parent.OutwardVessel.RV_RN_NKCountryOfReg.IsEmpty)
					{
						Parent.SG_OutwardVesselNameInfo.AddMessageError("Please enter the Outward Vessel nationality. Specify nationality of Outward Vessel for seastore permits application if goods are liquor/tobacco product.");
					}
				}

				if (Parent.OutwardVesselHasDuplicates)
				{
					// This validation only becomes relevant (active) when the unique key constraint on RV_Code is removed
					Parent.SG_OutwardVesselNameInfo.AddWarning("Outward Vessel entered has duplicate entries in the Vessel Reference file.\r\nPlease use the <F4> module search functionality to select the appropriate vessel.");
				}
			}
		}

		protected override void CheckSG_TowingVesselName()
		{
			base.CheckSG_TowingVesselName();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_TowingVesselNameInfo, Declaration.Lookups.Vessels);
		}

		protected override void CheckSG_SupplyIndicator()
		{
			base.CheckSG_SupplyIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_SupplyIndicatorInfo, Parent.Declaration.Lookups.SupplyIndicators);
		}

		protected override void CheckSG_ApplicationProductType()
		{
			base.CheckSG_ApplicationProductType();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_ApplicationProductTypeInfo, Parent.Declaration.Lookups.ApplicationProductTypes);
		}

		protected override void CheckSG_RN_NKDonorCountry()
		{
			base.CheckSG_RN_NKDonorCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_RN_NKDonorCountryInfo, Parent.Declaration.Lookups.CountryCodeList);
		}

		protected override void CheckSG_EntryYear()
		{
			base.CheckSG_EntryYear();

			CompareValidation.CheckNumberNotNegative(Parent.SG_EntryYearInfo);

			if (Parent.SG_EntryYear > 0 && Parent.SG_EntryYear < 1900)
			{
				Parent.SG_EntryYearInfo.AddWarning("Entry Year should be greater than 1900");
			}

			CompareValidation.WarnIfGreaterThanValue(Parent.SG_EntryYearInfo, 2100m);
		}

		protected override void CheckSG_Cert1Type()
		{
			base.CheckSG_Cert1Type();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_Cert1TypeInfo, Parent.Declaration.Lookups.CertificateTypes);
		}

		protected override void CheckSG_Cert2Type()
		{
			base.CheckSG_Cert2Type();
			ListValidation.MessageErrorIfInvalidCode(Parent.SG_Cert2TypeInfo, Parent.Declaration.Lookups.CertificateTypes);
		}

		protected override void CheckSG_RX_NKCertReferenceCurrency()
		{
			base.CheckSG_RX_NKCertReferenceCurrency();
			if (!Parent.SG_ApplicationProductType.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.SG_RX_NKCertReferenceCurrencyInfo, Parent.Declaration.Lookups.CurrencyList);
			}
		}

		protected override void CheckSG_Cert1PercCommContent()
		{
			base.CheckSG_Cert1PercCommContent();
			CompareValidation.CheckNumberNotNegative(Parent.SG_Cert1PercCommContentInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_Cert1PercCommContentInfo, 100m);
		}

		protected override void CheckSG_Cert1CopiesNo()
		{
			base.CheckSG_Cert1CopiesNo();
			CompareValidation.CheckNumberNotNegative(Parent.SG_Cert1CopiesNoInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_Cert1CopiesNoInfo, 10m);
		}

		protected override void CheckSG_Cert2CopiesNo()
		{
			base.CheckSG_Cert2CopiesNo();
			CompareValidation.CheckNumberNotNegative(Parent.SG_Cert2CopiesNoInfo);
			CompareValidation.WarnIfGreaterThanValue(Parent.SG_Cert2CopiesNoInfo, 10m);
		}

		protected override void CheckSG_DutyUnitRate()
		{
			base.CheckSG_DutyUnitRate();
			CompareValidation.CheckNumberNotNegative(Parent.SG_DutyUnitRateInfo);
		}

		protected override void CheckSG_ExciseUnitRate()
		{
			base.CheckSG_ExciseUnitRate();
			CompareValidation.CheckNumberNotNegative(Parent.SG_ExciseUnitRateInfo);
		}

		protected override void CheckSG_IsOutwardHandCarried()
		{
			base.CheckSG_IsOutwardHandCarried();
			Parent.Validation.ValidateSG_OutwardMAWB();
		}

		protected override void CheckSG_IsInwardHandCarried()
		{
			base.CheckSG_IsInwardHandCarried();
			Declaration.Validation.ValidateJE_MasterBill();
		}

		#endregion

		public bool IsStrategic
		{
			get
			{
				foreach (JobComInvoiceHeader invoiceHeader in Parent.Declaration.Invoices)
				{
					foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
					{
						if (invoiceLine.SG_IsStrategic)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		protected JobDeclaration Declaration => Parent.Declaration;

		protected new AddInfoJobDeclaration Parent => (AddInfoJobDeclaration)base.Parent;

		protected AddInfoJobDeclarationLookups Lookups => Parent.Lookups;

		protected ValidationHelper ValidationHelper => validationHelper ?? (validationHelper = new ValidationHelper());
		ValidationHelper validationHelper;
	}
}
