using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public abstract class CommonImportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		#region Constant

		public static class Constants
		{
			public static class PrimarySPI
			{
				public const string NotEligibleForSPI = "Tariff is not eligible for selected Special Programs Indicator";
				public const string InvalidSPIIndicatorForCountryOfExport = " is invalid for country of export";
				public const string InvalidSPIIndicatorForCountryOfOrigin = " is invalid for country of origin";
				public const string TPLClaimedAndNoSPINeeded = "The selected tariff already claims TPL benefits and you should not enter this SPI indicator.";
				public const string GSPCountryOfOrigin = "Country of origin is invalid for GSP";
				public const string GSPCountryOfExport = "Country of export is invalid for GSP";
				public const string OriginAndExportMustBeTheSameForGSP = "Country of Origin and Country of Export must be the same for GSP qualification unless both countries are members of the same Association of Countries";
				public const string IsraelFTAOriginError = "Israel free trade agreement not supported for selected country of origin - See Administrative Message 05-0147 ABI-EGYPT ELIGIBLE FOR SPI 'N' BENEFITS";
				public const string IsraelFTAExportError = "Israel free trade agreement not supported for selected country of export - See Administrative Message 05-0147 ABI-EGYPT ELIGIBLE FOR SPI 'N' BENEFITS";
				public const string CAFTAOriginatingClaim = "Country of Export not eligible for CAFTA (CAFTA countries are: Costa Rica, Dominican Republic, El Salvador, Guatemala, Honduras and Nicaragua)";
				public const string FASClaimRequiresSameOriginExport = "FAS claims require country of origin and country of export to be the same.";
				public const string CheckFASClaimExceptionOnChapter2To52AgriculturalProductWarning = "Please check whether the article is subject to duty free treatment or not. Freely Associated States duty-free treatment does not apply to any agricultural product of chapters 2 through 52 inclusive of the Harmonized Tariff Schedule, that is subject to a tariff rate quota, if entered in a quantity in excess of the in quota quantity for such product";
				public const string InsularPossessionRequiresSameCountry = "Insular possession claims require that country of origin and country of export to be the same.";
				public const string MayNotBeSpecifiedForDutyFree = "The SPI Indicator should not be entered if the Goods are Duty Free.";
				public const string TariffValidOnlyForColumn3Country = "The country of origin you have entered is not valid for this SPI. The tariff with this SPI is only valid for a column 3 country.";
				public const string ThereAreSPIsApplicableNonEntered = "There are SPIs that are valid for the selected tariff and/or country of origin. Please review and select a SPI from the list. Select N/A(Not Applicable) in the case that SPI does not apply to the invoice line.";
				public const string InvalidDutyRateWithEmptySPI = "The duty rate for this tariff indicates that SPI is required.";
				public const string CheckNAFTACertificate = "The NAFTA certificate on Importer Organization or Product -> eDocs Tab is not valid or expired.";
				public const string CheckCottonCertificate = "The Cotton Certificate on Importer Organization -> eDocs Tab is not valid or expired.";
				public const string MatchCottonCertificateOnImporter = "The Cotton Certificate does not macth Cotton Certificate on Importer Organization -> eDocs Tab.";
				public const string CheckCertificateOfOrigin = "The Certificate of Origin in eDocs of the Importer of Record Organization or Product Code is not valid or is expired.";
			}

			public static class CountryOfExport
			{
				public const string CountryOfExportAndOriginShouldBeSame = "For this tariff number, the country of origin and export should be the same.";
				public const string NoCAFTACountry = "The selected tariff indicates that the goods are eligible for CAFTA benefits, but this country is not a CAFTA country.";
				public const string NoCBTPACountry = "The selected tariff indicates that the goods are eligible for CBTPA textile benefits, but this country is not a CBTPA country.";
				public const string NoAGOACountry = "The selected tariff indicates that the goods are eligible for AGOA textile benefits, but this country is not an AGOA country.";
				public const string NoATPDEACountry = "The selected tariff indicates that the goods are eligible for ATPDEA textile or tuna benefits, but this country is not an ATPDEA country.";
				public const string CountryOfExportNeeded = "You should enter country of export. You can enter here or at Invoice Header or Declaration";
			}

			public static class OGA
			{
				public const string LaceyDataActIsNotNeededForXLine = "Lacey Act data is not allowed on X line and should be entered on V lines.";
				public const string DataIsNotNeededForXLine = "Please do not declare {0} data here, because this line is indicated as a set X line. {0} data should be declared against V lines only.";
				public const string TextileCLassificationDetails = "Textile Classification Details";
				public const string LicenseAndPermit = "Permit/License";
			}

			public static class FDA
			{
				public const string FDARequiredButBlank = "FDA Indicator is required when tariff is an FDA tariff (FD1, FD2, FD3, FD4)";
				public const string FDARequiredButDisclaimed = "The selected tariff requires the declaration of FDA data. Indicator must be 'Declared'";
				public const string FDAMustNotBeSubmitted = "FDA Indicator must be blank for this tariff. The tariff indicates that 'FDA is subject to FDA Admissibility Review - FDA notification is not required for this tariff' (FD0)";
				public const string FDALineRequired = "At least one FDA Line is required when FDA Indicator is 'Declared'";
				public const string FDALineNotAllowed = "FDA Lines may not be entered if FDA Indicator is blank or 'Disclaimed'";
				public const string FDANotRequired = "The tariff does not indicate that FDA reporting is required. If FDA Indicator is 'Declared', FDA data will be sent in the appropriate messages.";
				public const string FDADisclaimedInvalid = "It is invalid to Disclaim FDA when the entered tariff does not require FDA reporting.";
			}
		}

		#endregion

		public CommonImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine addInfo)
			: base(addInfo)
		{
		}

		#region US_FlavorContentCreditInd

		protected override void CheckUS_FlavorContentCreditInd()
		{
			base.CheckUS_FlavorContentCreditInd();
			if (Parent.US_FlavorContentCreditInd && Parent.US_TaxCode != Core.Constants.USCustoms.FeeCodes.DistilledSpirits)
			{
				Parent.US_FlavorContentCreditIndInfo.AddWarning(Res.GetString("USSpell|66969FCA-6275-4736-87E4-866F0536E345", "The Flavor Content Credit Indicator usually only applies to spirits."));
			}
		}

		#endregion

		#region US_DestinationState

		protected override void CheckUS_DestinationState()
		{
			base.CheckUS_DestinationState();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DestinationStateInfo, Parent.AddInfoLookups.USStateList, (NoResString)ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		#endregion

		protected override void CheckUS_AMMVPerUnit()
		{
			base.CheckUS_AMMVPerUnit();

			if (Parent.IsSetXLine && Parent.US_AMMVPerUnit > 0m)
			{
				Parent.US_AMMVPerUnitInfo.AddMessageError(AMMVNotApplicableForXLine);
			}
		}
		protected override void CheckUS_AMMVPercentage()
		{
			base.CheckUS_AMMVPercentage();

			if (Parent.IsSetXLine && Parent.US_AMMVPercentage > 0m)
			{
				Parent.US_AMMVPercentageInfo.AddMessageError(AMMVNotApplicableForXLine);
			}
		}
		internal const string AMMVNotApplicableForXLine = "You should not enter this value against X lines.";

		#region US_ManifestQty

		protected override void CheckUS_ManifestQty()
		{
			base.CheckUS_ManifestQty();

			if (IsManifestQtyRequired && Parent.US_ManifestQty.IsEmpty)
			{
				Parent.US_ManifestQtyInfo.AddMessageError(GetManifestQtyNotification());
			}
		}

		protected virtual string GetManifestQtyNotification()
		{
			return InnermostPacksIsMandatory;
		}
		internal const string InnermostPacksIsMandatory = "You should enter an Innermost Packs quantity for Invoice Line.";

		protected virtual bool IsManifestQtyRequired
		{
			get { return false; }
		}

		#endregion

		#region US_MiscPermitNo

		protected override void CheckUS_MiscPermitNo()
		{
			base.CheckUS_MiscPermitNo();

			if (ShouldValidateUS_MiscPermitNo)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(Constants.OGA.LicenseAndPermit, Parent.US_MiscPermitNoInfo);
				}
				else
				{
					var validator = new LicenceValidator();
					validator.ValidateMiscPermitTypeRequirement(Parent.US_MiscPermitNoInfo, InvoiceLine, !InvoiceLine.US_MiscPermitNo.IsEmpty);
					validator.ValidateMiscPermitTypeFormat(Parent.US_MiscPermitNoInfo, InvoiceLine, (x) => x.UE_PermitLicenseIndicator);
				}
			}
		}

		protected virtual bool ShouldValidateUS_MiscPermitNo
		{
			get { return false; }
		}

		#endregion

		#region US_UC_NKCountryOfExport

		protected override void CheckUS_UC_NKCountryOfExport()
		{
			base.CheckUS_UC_NKCountryOfExport();

			var declaration = Parent.Declaration;

			if (declaration != null && !EntryTypeList.IsWarehouseRelatedExceptFTZ(declaration.US_EntryType) && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Parent.US_UC_NKCountryOfExport) == Core.Constants.CountryCodes.UnitedStates)
			{
				Parent.US_UC_NKCountryOfExportInfo.AddMessageError(ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			}

			if (ShouldCheckUS_UC_NKCountryOfExport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_UC_NKCountryOfExportInfo, Lookups.USCountryList);

				string errorText = ExternalValidation.GetErrorTextForCanadaCountryOfExport(Parent.US_UC_NKCountryOfExport);
				if (!string.IsNullOrEmpty(errorText))
				{
					Parent.US_UC_NKCountryOfExportInfo.AddMessageError(errorText);
				}

				if (Parent.US_UC_NKCountryOfExport.IsEmpty && !(Parent.Declaration?.IsConsumptionFTZ ?? false))
				{
					Parent.US_UC_NKCountryOfExportInfo.AddMessageError(Constants.CountryOfExport.CountryOfExportNeeded);
				}
				else
				{
					USCTariff tariff = Parent.ImportSupTariff ?? Parent.ImportTariff;
					if (tariff != null)
					{
						USCCountry exportCountry = Parent.CountryOfExport_US;
						if (exportCountry != null)
						{
							ZDateTime dutyDate = Parent.EffectiveDateForDutyRate;
							string notificationText = GetNotificationTextOnSpecialTradeAgreementAndCountry(exportCountry, tariff, dutyDate);

							if (!string.IsNullOrEmpty(notificationText))
							{
								Parent.US_UC_NKCountryOfExportInfo.AddMessageError(notificationText);
							}

							if (tariff.CountryOfOriginAndExportShouldBeSame(dutyDate))
							{
								CheckCOAndCEAreTheSameAndAddMessageIfNot(Parent.US_UC_NKCountryOfExportInfo);
							}
						}
					}
				}

				ValidateUS_SPI();
				ValidateUS_UC_NKCountryOfOrigin();
			}
		}

		protected virtual bool ShouldCheckUS_UC_NKCountryOfExport
		{
			get { return true; }
		}

		string GetNotificationTextOnSpecialTradeAgreementAndCountry(USCCountry country, USCTariff tariff, ZDateTime dutyDate)
		{
			string result = "";

			if (country != null && tariff != null)
			{
				if (tariff.IsEligibleForAGOATextileBenefits(dutyDate) && !country.IsEligibleForAGOA(dutyDate))
				{
					result = Constants.CountryOfExport.NoAGOACountry;
				}
				else if (tariff.IsEligibleForCBTPATextileBenefits(dutyDate) && !country.IsEligibleForCBTPA(dutyDate))
				{
					result = Constants.CountryOfExport.NoCBTPACountry;
				}
				else if (tariff.IsEligibleForATPDEATextileAndTunaClaims(dutyDate) && !country.IsEligibleForATPDEA(dutyDate))
				{
					result = Constants.CountryOfExport.NoATPDEACountry;
				}
				else if (tariff.IsEligibleForCAFTAClaims(dutyDate) && !country.IsEligibleForCAFTA(dutyDate))
				{
					result = Constants.CountryOfExport.NoCAFTACountry;
				}
			}

			return result;
		}

		void CheckCOAndCEAreTheSameAndAddMessageIfNot(ZPropertyInfo infoToAddNotifications)
		{
			if (Parent.US_UC_NKCountryOfExport != Parent.US_UC_NKCountryOfOrigin)
			{
				infoToAddNotifications.AddMessageError(Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			}
		}

		#endregion

		#region US_UC_NKCountryOfOrigin

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			base.CheckUS_UC_NKCountryOfOrigin();

			if (ShouldCheckUS_UC_NKCountryOfOrigin)
			{
				//users have entered C/O at this level
				if (!InvoiceLine.US_UC_NKCountryOfOrigin.IsEmpty)
				{
					if (InvoiceLine.US_UC_NKCountryOfOrigin != USCCountry.Unknown)
					{
						ListValidation.MessageErrorIfInvalidCode(InvoiceLine.US_UC_NKCountryOfOriginInfo, InvoiceLine.Lookups.USCountryOfOrigins);

						string errorTextForOrigin = ExternalValidation.GetErrorTextForCanadaCountryOfOrigin(InvoiceLine.US_UC_NKCountryOfOrigin, InvoiceLine.US_UC_NKCountryOfExport);
						if (!string.IsNullOrEmpty(errorTextForOrigin))
						{
							InvoiceLine.US_UC_NKCountryOfOriginInfo.AddMessageError(errorTextForOrigin);
						}

						var mID = Parent.ManufacturerFallBackToSupplierNumber;
						var invoiceHeader = InvoiceLine.InvoiceHeader;

						if (mID != invoiceHeader.ManufacturerFallBackToSupplierNumber || Parent.US_UC_NKCountryOfOrigin != invoiceHeader.US_UC_NKCountryOfOrigin)
						{
							OrganisationValidation.ValidateMIDCountryAgainstCountryOfOriginForCanada(Parent.US_UC_NKCountryOfOriginInfo, mID, Parent.US_UC_NKCountryOfExport);
						}

						OrganisationValidation.ValidateMIDCountryAgainstCountryOfOriginForTextileTariff(Parent.US_UC_NKCountryOfOriginInfo, mID, InvoiceLine.ImportTariff, InvoiceLine.EffectiveDateForDutyRate);
					}
				}
				else if (!InvoiceLine.IsSecondaryTariffLine)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UC_NKCountryOfOriginInfo);
				}

				USCTariff tariff = Parent.ImportSupTariff ?? Parent.ImportTariff;
				if (tariff != null)
				{
					ZDateTime dutyDate = InvoiceLine.EffectiveDateForDutyRate;

					if (!tariff.HasValidCountryOfOrigin(InvoiceLine.US_UC_NKCountryOfOrigin))
					{
						InvoiceLine.US_UC_NKCountryOfOriginInfo.AddMessageError(string.Format(InvalidCountryOfOrigin, tariff.ValidCountryOfOrigin));
					}

					USCCountry originCountry = InvoiceLine.CountryOfOrigin_US;
					if (originCountry != null)
					{
						string notificationText = GetNotificationTextOnSpecialTradeAgreementAndCountry(originCountry, tariff, dutyDate);

						if (!string.IsNullOrEmpty(notificationText))
						{
							InvoiceLine.US_UC_NKCountryOfOriginInfo.AddMessageError(notificationText);
						}

						if (tariff.CountryOfOriginAndExportShouldBeSame(dutyDate))
						{
							CheckCOAndCEAreTheSameAndAddMessageIfNot(InvoiceLine.US_UC_NKCountryOfOriginInfo);
						}
					}
				}

				ValidateUS_SPI();
				ValidateUS_UC_NKCountryOfExport();
				InvoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
			}
		}
		internal const string InvalidCountryOfOrigin = "This tariff is valid only for goods where the country of origin is '{0}'";

		protected virtual bool ShouldCheckUS_UC_NKCountryOfOrigin
		{
			get { return true; }
		}

		#endregion

		#region US_SPI

		protected override void CheckUS_SPI()
		{
			base.CheckUS_SPI();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SPIInfo, Lookups.SPIList);

			if (ShouldCheckUS_SPI)
			{
				ZString sPIEffective = Parent.US_SPI;
				var tariffList = new List<USCTariff>() { InvoiceLine.ImportTariff, InvoiceLine.ImportSupTariff };
				if (Parent.IsCombinedLine() && Parent.IsParentLine)
				{
					foreach (var childLine in Parent.ChildLines)
					{
						tariffList.Add(childLine.ImportTariff);
						tariffList.Add(childLine.ImportSupTariff);
					}
				}

				if (!Parent.IsCombinedLine())
				{
					EnsurePrimarySPINotEntereredForDutyFreeTariff(sPIEffective, tariffList);
				}

				if (Parent.US_SPI.IsEmpty)
				{
					var spiList = InvoiceLine.AddInfoLookups.SPIList;
					if (spiList.Count > 0 && !(spiList.Count == 1 && spiList[0].Code == SPICompleteList.MoreCodes.NotApplicable))
					{
						IDutyData dutyData = InvoiceLine;
						dutyData = dutyData.ParentTariffLine ?? dutyData;

						if (Parent.IsCombinedLine())
						{
							var normalTraiffDutyData = Parent.ChildLines.FirstOrDefault(x => x.IsNormalTariffLine());
							if (normalTraiffDutyData != null)
							{
								dutyData = normalTraiffDutyData;
							}
						}

						IRateWrapper dutyRate = DutyRateWrapper.GetWrapper(dutyData);
						if (dutyRate.IsInvalidDutyRate())
						{
							Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.InvalidDutyRateWithEmptySPI);
						}
						else
						{
							if (USCustomsDataRegistry.Instance.SPIValidation.GetFallBackValueAtAllLevels(Parent.RegistryCompanyPK, Parent.RegistryBranchPK, Guid.Empty) == SPIValidationTypeList.Codes.WRN)
							{
								Parent.US_SPIInfo.AddWarning(Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
							}
							else
							{
								Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
							}
						}
					}
				}
				else if (Parent.US_SPI != SPICompleteList.MoreCodes.NotApplicable)
				{
					USCCountry countryOfOrigin = Parent.CountryOfOrigin_US;
					USCCountry countryOfExport = Parent.CountryOfExport_US;

					if (Parent.IsCombinedLine())
					{
						PrimarySPIBasedOnTariffValidationForCombinedLine(sPIEffective, countryOfOrigin, countryOfExport, tariffList);
					}
					else
					{
						PrimarySPIBasedOnTariffValidation(sPIEffective, countryOfOrigin, countryOfExport);
					}

					switch (sPIEffective)
					{
						case PrimarySpecProgramIndicatorList.Codes.A:
							DoCaseAprimarySPIValidation(countryOfOrigin, countryOfExport, tariffList);
							break;
						case PrimarySpecProgramIndicatorList.Codes.B:
							EnsureCountryOfOriginIsCanadaForPrimaySPI_B();
							break;
						case PrimarySpecProgramIndicatorList.Codes.N:
							DoCaseNprimarySPIValidation();
							break;
						case PrimarySpecProgramIndicatorList.Codes.P:
						case SpecialProgramList.Codes.PPlus:
							if (countryOfExport != null && !countryOfExport.IsEligibleForCAFTA(InvoiceLine.EffectiveDateForDutyRate))
							{
								Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.CAFTAOriginatingClaim);
							}
							break;
						case PrimarySpecProgramIndicatorList.Codes.Y:
							if (Parent.US_UC_NKCountryOfOrigin != Parent.US_UC_NKCountryOfExport)
							{
								Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.InsularPossessionRequiresSameCountry);
							}
							break;
						case PrimarySpecProgramIndicatorList.Codes.Z:
							if (Parent.US_UC_NKCountryOfExport != Parent.US_UC_NKCountryOfOrigin)
							{
								Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.FASClaimRequiresSameOriginExport);
							}

							if (countryOfOrigin != null)
							{
								if (countryOfOrigin.IsValidForSPI(Parent.US_SPI, Parent.EffectiveDateForDutyRate))
								{
									ZInt tariffChapter = ZInt.ParseSafe(Parent.JI_Tariff.Left(2), 0);

									if (tariffChapter >= 2 && tariffChapter <= 52)
									{
										Parent.US_SPIInfo.AddWarning(Constants.PrimarySPI.CheckFASClaimExceptionOnChapter2To52AgriculturalProductWarning);
									}
								}
								else
								{
									Parent.US_SPIInfo.AddMessageError(Parent.US_SPI + Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
								}
							}

							break;
						case PrimarySpecProgramIndicatorList.Codes.D:
						case PrimarySpecProgramIndicatorList.Codes.R:
						case PrimarySpecProgramIndicatorList.Codes.J:
							ZDateTime effectiveDutyDate = Parent.EffectiveDateForDutyRate;
							if (countryOfOrigin != null && !countryOfOrigin.IsValidForSPI(sPIEffective, effectiveDutyDate))
							{
								Parent.US_SPIInfo.AddMessageError(sPIEffective + Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
							}
							if (countryOfExport != null && !countryOfExport.IsValidForSPI(sPIEffective, effectiveDutyDate))
							{
								Parent.US_SPIInfo.AddMessageError(sPIEffective + Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
							}
							break;
						case SpecialProgramList.Codes.MX:
						case SpecialProgramList.Codes.CA:
							CheckNAFTACertificatesExpirationDate();
							break;
						case SpecialProgramList.Codes.S:
						case SpecialProgramList.Codes.SPlus:
							CheckForValidCertificateOfOrigin();
							break;
					}

					SpecialProgramIndicatorValidation specialProgramIndicatorValidation = new SpecialProgramIndicatorValidation(Parent.Factory);
					string countryOfOriginString = Parent.IsCountryOfOriginCanada ? new ZString(Core.Constants.CountryCodes.Canada) : Parent.US_UC_NKCountryOfOrigin;
					string countryOfExportString = Parent.US_UC_NKCountryOfExport;

					USCTariff tariff = Parent.ImportSupTariff ?? Parent.ImportTariff;
					string errorText = specialProgramIndicatorValidation.GetErrorText(countryOfOriginString, countryOfExportString, Parent.US_SPI, Parent.EffectiveDateForDutyRate, tariff);
					if (errorText.Length > 0)
					{
						Parent.US_SPIInfo.AddMessageError(errorText);
					}
				}
			}

			var iorWrapper = Parent.IORWrapper;
			if (iorWrapper != null)
			{
				iorWrapper.RestrictedSPIs.CheckRestrictedCode(Parent.US_SPIInfo);
			}
		}

		protected virtual bool ShouldCheckUS_SPI
		{
			get { return true; }
		}

		void EnsurePrimarySPINotEntereredForDutyFreeTariff(string spiCode, List<USCTariff> tariffs)
		{
			if (InvoiceLine.Factory.GetCachedValue<PrimarySpecProgramIndicatorList>().ContainsCode(spiCode)
				&& !SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(spiCode, InvoiceLine.CountryOfOrigin_US, InvoiceLine.EffectiveDateForDutyRate)
				&& (tariffs?.TrueForAll(tariff => tariff != null && tariff.IsDutyFree) ?? false))
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
			}
		}

		void EnsureCountryOfOriginIsCanadaForPrimaySPI_B()
		{
			ZString countryOfOriginCode = Parent.US_UC_NKCountryOfOrigin;

			if (!countryOfOriginCode.IsEmpty && !CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOriginCode))
			{
				Parent.US_SPIInfo.AddMessageError(CountryOfOriginShouldBeACanadianProvinceForPrimarySPI_B);
			}
		}
		internal const string CountryOfOriginShouldBeACanadianProvinceForPrimarySPI_B = "For this SPI, Country of Origin should be one of Canadian Province codes starting 'X'";

		void PrimarySPIBasedOnTariffValidationForCombinedLine(string primarySPIEffective, USCCountry countryOfOrigin, USCCountry countryOfExport, List<USCTariff> tariffs)
		{
			var normalTariffLine = Parent.ChildLines.FirstOrDefault(x => x.IsNormalTariffLine());
			var isEligibleForSPI = !IsNotEligibleForSPI(countryOfOrigin, Parent, Parent.ImportSupTariff, normalTariffLine != null ? normalTariffLine.ImportTariff : null);
			foreach (var childLine in Parent.ChildLines)
			{
				isEligibleForSPI |= !IsNotEligibleForSPI(countryOfOrigin, childLine, childLine.ImportSupTariff, null);
			}

			if (!isEligibleForSPI)
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.NotEligibleForSPI);
			}

			if (tariffs.Any(x => x != null && x.Applies(TariffRuleList.Codes.NoSPIRequired, Parent.EffectiveDateForDutyRate)))
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.TPLClaimedAndNoSPINeeded);
			}
			else if (primarySPIEffective == PrimarySpecProgramIndicatorList.Codes.A || primarySPIEffective == PrimarySpecProgramIndicatorList.Codes.B)
			{
				CheckGSPExcluded(countryOfOrigin, countryOfExport, tariffs.ToArray());
			}
		}

		bool IsNotEligibleForSPI(USCCountry countryOfOrigin, JobComInvoiceLine invoiceLine, USCTariff importSupTariff, USCTariff importTariff)
		{
			return countryOfOrigin != null && !countryOfOrigin.IsValidForSPI(invoiceLine.US_SPI, invoiceLine.EffectiveDateForDutyRate)
					&& !DoesAnyTariffHaveSpecialProgramsIndicator(importSupTariff, importTariff, invoiceLine.EffectiveDateForDutyRate, invoiceLine.US_SPI)
					&& !SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(invoiceLine.US_SPI, countryOfOrigin, InvoiceLine.EffectiveDateForDutyRate);
		}

		void PrimarySPIBasedOnTariffValidation(string primarySPIEffective, USCCountry countryOfOrigin, USCCountry countryOfExport)
		{
			USCTariff tariff = Parent.ImportSupTariff ?? Parent.ImportTariff;

			if (tariff != null)
			{
				if (IsNotEligibleForSPI(countryOfOrigin, Parent, Parent.ImportSupTariff, Parent.ImportTariff))
				{
					Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.NotEligibleForSPI);
				}

				if (tariff.Applies(TariffRuleList.Codes.NoSPIRequired, Parent.EffectiveDateForDutyRate))
				{
					Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.TPLClaimedAndNoSPINeeded);
				}
				else if (primarySPIEffective == PrimarySpecProgramIndicatorList.Codes.A || primarySPIEffective == PrimarySpecProgramIndicatorList.Codes.B)
				{
					CheckGSPExcluded(countryOfOrigin, countryOfExport, Parent.ImportSupTariff, Parent.ImportTariff);
				}
			}
		}

		static bool DoesAnyTariffHaveSpecialProgramsIndicator(USCTariff importSupTariff, USCTariff importTariff, ZDate dutyRateDate, ZString spi)
		{
			var tariff = importSupTariff ?? importTariff;
			var additionalTariff = importSupTariff != null && !importSupTariff.Applies(TariffRuleList.Codes.InLieuTariffs, dutyRateDate) ? importTariff : null;

			return tariff != null && tariff.HasSpecialProgramsIndicator(spi) || additionalTariff != null && additionalTariff.HasSpecialProgramsIndicator(spi);
		}

		void CheckGSPExcluded(USCCountry countryOfOrigin, USCCountry countryOfExport, params USCTariff[] tariffs)
		{
			foreach (USCTariff tariff in tariffs)
			{
				if (tariff != null)
				{
					if (countryOfOrigin != null && tariff.IsGSPExcluded(countryOfOrigin.UC_Code))
					{
						Parent.US_SPIInfo.AddMessageError(countryOfOrigin.UC_Name + " is excluded for GSP for the selected tariff.");
						break;
					}
					else if (countryOfExport != null && tariff.IsGSPExcluded(countryOfExport.UC_Code))
					{
						Parent.US_SPIInfo.AddMessageError(countryOfExport.UC_Name + " is excluded for GSP for the selected tariff.");
						break;
					}
				}
			}
		}

		void DoCaseAprimarySPIValidation(USCCountry countryOfOrigin, USCCountry countryOfExport, List<USCTariff> tariffs)
		{
			ZDateTime effectiveDutyDate = Parent.EffectiveDateForDutyRate;

			if (countryOfOrigin == null || !countryOfOrigin.IsValidForGSP(effectiveDutyDate))
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.GSPCountryOfOrigin);
			}

			if (countryOfExport == null || !countryOfExport.IsValidForGSP(effectiveDutyDate))
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.GSPCountryOfExport);
			}

			// Rule 386
			if (Parent.US_UC_NKCountryOfExport != Parent.US_UC_NKCountryOfOrigin)
			{
				AssociationsOfCountries.Agreement exportAgreement = AssociationsOfCountries.GetAgreement(Parent.US_UC_NKCountryOfExport);
				AssociationsOfCountries.Agreement originAgreement = AssociationsOfCountries.GetAgreement(Parent.US_UC_NKCountryOfOrigin);
				if (exportAgreement != originAgreement || exportAgreement == AssociationsOfCountries.Agreement.None)
				{
					Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.OriginAndExportMustBeTheSameForGSP);
				}
			}

			if (countryOfOrigin != null)
			{
				if (tariffs.Any(x => x != null && x.HasSpecialProgramsIndicator("A+")))
				{
					ZString rateIndicator = countryOfOrigin.GetRateIndicator(Parent.EffectiveDateForDutyRate);
					if (rateIndicator != "3")
					{
						Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.TariffValidOnlyForColumn3Country);
					}
				}
			}
		}

		void DoCaseNprimarySPIValidation()
		{
			if (!USCCountry.IsraelEligibleCountries.Contains(Parent.US_UC_NKCountryOfOrigin))
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.IsraelFTAOriginError);
			}

			if (!USCCountry.IsraelEligibleCountries.Contains(Parent.US_UC_NKCountryOfExport))
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.IsraelFTAExportError);
			}
		}

		void CheckNAFTACertificatesExpirationDate()
		{
			var importer = Parent.Declaration?.Importer;
			var part = Parent.Part;

			var naftaDocuments = Enumerable.Empty<JobRequiredDocument>();

			if (importer != null)
			{
				naftaDocuments = importer.RequiredDocuments.Cast<JobRequiredDocument>().Where(x => x.EQ_DocType == Core.Constants.RefDocTypes.Nafta);
			}

			if (part != null)
			{
				var partHasRequiredDocuments = (IHaveRequiredDocuments)part;
				naftaDocuments = naftaDocuments.Concat(partHasRequiredDocuments.RequiredDocuments.Cast<JobRequiredDocument>().Where(x => x.EQ_DocType == Core.Constants.RefDocTypes.Nafta));
			}

			if (naftaDocuments.Any() && HasExpiredNAFTACertificate(naftaDocuments) && !HasValidNAFTACertificate(naftaDocuments))
			{
				Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.CheckNAFTACertificate);
			}
		}

		bool HasExpiredNAFTACertificate(IEnumerable<BusinessObject> requiredDocuments)
		{
			return requiredDocuments.Cast<JobRequiredDocument>().Any(x => x.EQ_ValidToDate.IsValid && x.EQ_ValidToDate < Parent.EffectiveDateForDutyRate);
		}

		bool HasValidNAFTACertificate(IEnumerable<BusinessObject> requiredDocuments)
		{
			return requiredDocuments.Cast<JobRequiredDocument>().Any(x => x.EQ_ValidToDate.IsEmpty || Parent.EffectiveDateForDutyRate <= x.EQ_ValidToDate);
		}

		void CheckForValidCertificateOfOrigin()
		{
			var action = USCustomsDataRegistry.Instance.SeverityLevelForMissingUSMCACertificate.Value;

			if (ShouldCheckForValidCertificateOfOrigin(action))
			{
				var ior = Parent.Declaration?.IOR;
				var importer = Parent.Declaration?.Importer;
				var part = Parent.Part;

				if (ior != null && HasValidCertificateOfOrigin(ior.RequiredDocuments))
				{
					return;
				}
				else if (importer != null && HasValidCertificateOfOrigin(importer.RequiredDocuments))
				{
					return;
				}
				else if (part != null && HasValidCertificateOfOrigin(((IHaveRequiredDocuments)part).RequiredDocuments))
				{
					return;
				}

				if (action == ProductAuditActions.Codes.AddMessageErrorValidation)
				{
					Parent.US_SPIInfo.AddMessageError(Constants.PrimarySPI.CheckCertificateOfOrigin);
				}
				else if (action == ProductAuditActions.Codes.AddWarningValidation)
				{
					Parent.US_SPIInfo.AddWarning(Constants.PrimarySPI.CheckCertificateOfOrigin);
				}
			}
		}

		bool ShouldCheckForValidCertificateOfOrigin(string action)
		{
			return action == ProductAuditActions.Codes.AddMessageErrorValidation || action == ProductAuditActions.Codes.AddWarningValidation;
		}

		bool HasValidCertificateOfOrigin(JobRequiredDocumentDependentCollection requiredDocuments)
		{
			return requiredDocuments.OfType<JobRequiredDocument>().Any(x =>
				x.EQ_DocType == Core.Constants.RefDocTypes.CertificateOfOrigin &&
				x.Attributes[JobRequiredDocAttribTypeList.Codes.TradePreferenceCode, SpecialProgramList.Codes.S] != null &&
				(x.EQ_ValidToDate.IsEmpty || Parent.EffectiveDateForDutyRate <= x.EQ_ValidToDate));
		}

		#endregion

		#region US_SecondarySPI

		protected override void CheckUS_SecondarySPI()
		{
			base.CheckUS_SecondarySPI();

			if (ShouldCheckUS_SecondarySPI)
			{
				switch (Parent.US_SecondarySPI)
				{
					case SecondarySpecProgIndicatorList.Codes.F:
						{
							ZString entryType = Parent.ImportEntryType;
							if (entryType != EntryTypeList.Codes.ConsumptionQuotaVisa &&
								entryType != EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa &&
								entryType != EntryTypeList.Codes.WarehouseWithdrawalQuota &&
								entryType != EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa)
							{
								Parent.US_SecondarySPIInfo.AddMessageError(GetErrorMessageForFolkloreValidEntryTypes());
							}

							if (Parent.CountryOfOrigin_US != null && !Parent.CountryOfOrigin_US.IsFolkloreAgreementsCountry)
							{
								Parent.US_SecondarySPIInfo.AddMessageError(GetErrorMessageForNotFolkloreAgreementsCountry());
							}
						}
						break;

					case SecondarySpecProgIndicatorList.Codes.G:
						{
							if (Parent.US_UC_NKCountryOfOrigin != Core.Constants.CountryCodes.HongKong)
							{
								Parent.US_SecondarySPIInfo.AddMessageError(OnlyValidWhenCOisHK);
							}
							break;
						}
					case SecondarySpecProgIndicatorList.Codes.H:
						{
							if (!Parent.JI_Tariff.StartsWith("61") && !Parent.JI_Tariff.StartsWith("62"))
							{
								Parent.US_SecondarySPIInfo.AddMessageError(OnlyValidWhenTariffChapter6162);
							}
							break;
						}
				}
			}

			ValidateUS_TextileCategoryNo();
			JobComInvoiceLineValidation validationObject = Parent.Validation;
			validationObject.ValidateJI_LinePrice();

			var iorWrapper = Parent.IORWrapper;
			if (iorWrapper != null)
			{
				iorWrapper.RestrictedSPIs.CheckRestrictedCode(Parent.US_SecondarySPIInfo);
			}
		}

		protected override string GetHumanReadableNameForSecondarySPI()
		{
			var declaration = Parent?.Declaration;
			return declaration != null && declaration.IsACE ? "Product Claim" : base.GetHumanReadableNameForSecondarySPI();
		}

		string GetErrorMessageForFolkloreValidEntryTypes()
		{
			return string.Format(CultureInfo.InvariantCulture
				, "{0}, 'F' is valid only for entry types, 02/07/32/38."
				, GetHumanReadableNameForSecondarySPI());
		}

		string GetErrorMessageForNotFolkloreAgreementsCountry()
		{
			return string.Format(CultureInfo.InvariantCulture
				, "{0} of 'F' should only be selected where a folklore agreement exists for the Country of Origin. The U.S. has folklore agreements with the following countries: BD, CO, IN, JP, KR, MY, MX, PK, PE, PH, TW and TH."
				, GetHumanReadableNameForSecondarySPI());
		}

		protected virtual bool ShouldCheckUS_SecondarySPI
		{
			get { return true; }
		}

		internal const string OnlyValidWhenCOisHK = "'G' is only a valid selection when Country of Origin is Hong Kong.";
		internal const string OnlyValidWhenTariffChapter6162 = "'H' is only a valid selection when tariff is chapter 61 or 62.";

		#endregion

		#region XV set

		protected void ValidateSetXAndVAgainstRelationship(ZPropertyInfo info)
		{
			Func<JobComInvoiceLine, JobComInvoiceLine, ZBool> tariffDifferent = (line1, line2) => line1 != null && line2 != null && (line1.JI_Tariff != line2.JI_Tariff || line1.US_SupTariff != line2.US_SupTariff);
			if (InvoiceLine.IsSetXLine)
			{
				if (InvoiceLine.HasLaceyActData)
				{
					info.AddMessageError(Constants.OGA.LaceyDataActIsNotNeededForXLine);
				}

				if (Parent.ChildLines.IsNullOrEmpty())
				{
					info.AddMessageError(NoComponentLinesForSetHeaderLine);
				}
				else
				{
					var firstVLine = InvoiceLine.FirstVLine;
					if (tariffDifferent(firstVLine, Parent))
					{
						if (!firstVLine.IsVParentLine)
						{
							info.AddMessageError(SameTariffNumberForTheFirstVLine);
						}
						else if (tariffDifferent(firstVLine.FirstVLine, Parent))
						{
							info.AddMessageError(SameTariffNumberForTheFirstVLine);
						}
					}
				}
			}
			else if (InvoiceLine.IsSetVLine)
			{
				var parentLine = Parent.ParentTariffLine;
				if (parentLine == null)
				{
					info.AddMessageError(NoParentLineForAComponentLine);
				}
				else
				{
					if (parentLine.IsSetVLine && parentLine.FirstVLine == Parent)
					{
						var parentXLine = parentLine.ParentTariffLine;
						if (parentXLine != null && parentXLine.FirstVLine == parentLine && tariffDifferent(parentXLine, parentLine) && tariffDifferent(parentXLine, InvoiceLine))
						{
							info.AddMessageError(SameTariffNumberForTheFirstVLine);
						}
					}
					else if (parentLine.IsSetXLine && parentLine.FirstVLine == Parent && tariffDifferent(parentLine, InvoiceLine))
					{
						var childVLine = Parent.FirstVLine;
						if (childVLine == null)
						{
							info.AddMessageError(SameTariffNumberForTheFirstVLine);
						}
						else if (tariffDifferent(childVLine, parentLine))
						{
							info.AddMessageError(SameTariffNumberForTheFirstVLine);
						}
					}
				}
			}
		}
		internal const string SameTariffNumberForTheFirstVLine = "The first V line after X line has to have the same tariff number and prov/prog tariff number as the X line. Please change the tariff number and prov/prog tariff number of the first V line to match the X line's one OR change the Line Number (LNO) of an existing V line which has the same tariff number and prov/prog tariff number as the X line so it can become the first V line.";
		internal const string NoComponentLinesForSetHeaderLine = "This line is indicated as a set X line, but there is no corresponding V line with the same tariff number as this line. Please enter a line with its 'Parent' line pointing to this line with the same tariff number.";
		internal const string NoParentLineForAComponentLine = "This line is indicated as a set component line, but there is no corresponding set header Parent line entered. Please enter 'Parent' line";

		#endregion

		#region US_TextileCategoryNo

		protected override void CheckUS_TextileCategoryNo()
		{
			base.CheckUS_TextileCategoryNo();

			if (Parent.IsSetXLine)
			{
				ValidateDataNotNeededForXLine(Constants.OGA.TextileCLassificationDetails, Parent.US_TextileCategoryNoInfo);
			}
			else
			{
				IDutyData parentLine = ((IDutyData)Parent).ParentTariffLine;
				if (!Parent.US_TextileCategoryNo.IsEmpty)
				{
					CheckTariffMakesCategoryNumberExempt(Parent.ImportTariff, parentLine != null ? parentLine.ImportTariff : null);

					if ((Parent.ImportTariff != null && (Parent.US_SupTariff.StartsWith("98020050") || Parent.US_SupTariff.StartsWith("98020060"))) ||
						(Parent.ImportSupTariff != null && (Parent.US_SupTariff.StartsWith("98020050") || Parent.US_SupTariff.StartsWith("98020060"))))
					{
						Parent.US_TextileCategoryNoInfo.AddMessageError(TextileCategoryIsNotRequiredForThisTariff);
					}
				}

				if (Parent.US_TextileCategoryNo_Effective.IsEmpty)
				{
					USCTariff tariff = parentLine != null && parentLine.ImportTariff != null ? parentLine.ImportTariff : Parent.ImportTariff;

					if (tariff != null && tariff.Applies(TariffRuleList.Codes.HaitiTariffHope, Parent.EffectiveDateForDutyRate))
					{
						Parent.US_TextileCategoryNoInfo.AddMessageError(TextileCategoryIsMandatory);
					}
				}

				if (Parent.ImportTariff != null && !Parent.HasSupplementary)
				{
					if (!Parent.IsSecondaryTariffLine && !Parent.HasSecondaryTariffLines)
					{
						if (Parent.ImportTariff.UE_QuotaIndicator && !Parent.ImportTariff.UE_TextileCategoryNumber.IsEmpty)
						{
							if (Parent.US_TextileCategoryNo.IsEmpty)
							{
								Parent.US_TextileCategoryNoInfo.AddWarning(TariffSubjectToQuota);
							}
							else if (Parent.US_TextileCategoryNo != Parent.ImportTariff.UE_TextileCategoryNumber)
							{
								Parent.US_TextileCategoryNoInfo.AddMessageError(InvalidTextileCategory + Parent.ImportTariff.UE_TextileCategoryNumber.ToString());
							}
						}
						else if (!Parent.US_TextileCategoryNo.IsEmpty)
						{
							Parent.US_TextileCategoryNoInfo.AddMessageError(TariffNotSubjectToQuota);
						}

						if (Parent.US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.G)
						{
							if (!ValidCategoryCodeForG(Parent.US_TextileCategoryNo))
							{
								Parent.US_TextileCategoryNoInfo.AddMessageError(TextileCategoryForSPI);
							}
						}
					}
				}

				ValidateUS_SPI();
			}
		}

		void CheckTariffMakesCategoryNumberExempt(params USCTariff[] tariffs)
		{
			foreach (USCTariff tariff in tariffs)
			{
				if (tariff != null && tariff.NoCategoryNumberToBeEntered(Parent.EffectiveDateForDutyRate))
				{
					Parent.US_TextileCategoryNoInfo.AddMessageError(string.Format(ParentTariffNumberMakesCategoryNumberExempt, tariff.UE_Tariff));
					break;
				}
			}
		}

		bool ValidCategoryCodeForG(ZString sPI)
		{
			switch (sPI)
			{
				case "443":
				case "444":
				case "643":
				case "644":
				case "843":
				case "844":
					return true;

				default:
					return false;
			}
		}

		internal const string ParentTariffNumberMakesCategoryNumberExempt = "The tariff number, '{0}' makes Visa/Quota reporting exempt. You should not enter a category number.";
		internal const string InvalidTextileCategory = "The Textile Category for this Tariff should be ";
		internal const string TariffSubjectToQuota = "According to CBP reference files, this Tariff may be subject to Quota Reporting - if so, the Textile Category should be entered.";
		internal const string TariffNotSubjectToQuota = "This Tariff does not appear to be subject to Quota Reporting - Please do not enter the Textile Category.";
		internal const string TextileCategoryForSPI = "If Secondary SPI is 'G', Category No must be '443', '444', '643', '644', '843', OR '844'.";
		internal const string TextileCategoryIsMandatory = "Category Number is mandatory for the entered tariff number.";
		internal const string TextileCategoryIsNotRequiredForThisTariff = "Category Number is not required for 9802.00.50 and 9802.00.60 Tariffs";

		#endregion

		#region US_SupTariff

		protected override void CheckUS_SupTariff()
		{
			base.CheckUS_SupTariff();
			var parent = Parent;
			var supTariff = parent.US_SupTariff;
			var supTariffInfo = parent.US_SupTariffInfo;
			var iorWrapper = parent.IORWrapper;
			if (iorWrapper != null)
			{
				iorWrapper.RestrictedTariffs.CheckRestrictedCode(supTariffInfo);
			}
			var isSupTariffEmpty = supTariff.IsEmpty;
			var parentIsTariffWithGAEType = parent.IsTariffWithGAEType;
			if (parent.IsACENormalInvoiceLine || ((parent.IsParentLine && parent.ChildLines.Any()) || parent.IsChildLine))
			{
				var effectiveLine = GetEffectiveInvoiceLine();
				var applicableSupTariffs = effectiveLine.ApplicableSupTariffs;
				if (!parentIsTariffWithGAEType && !isSupTariffEmpty && parent.SupTariffHasAttributeA99 && (!applicableSupTariffs.Any(x => x.Tariff == supTariff) && !IsStnTariffRule))
				{
					supTariffInfo.AddMessageError(ProvTariffIsNotAllowed);
				}

				if (applicableSupTariffs.Length > 0)
				{
					if (isSupTariffEmpty)
					{
						var declaration = parent.Declaration;
						var forEUNSteel = parent.IsOriginFromEUN && parent.IsSteelOriginFromEUNForAnySupTariff && !parent.IsTariffWithGAEType;
						if (declaration != null && EntryTypeList.IsQuotaProductExclusionType(declaration.US_EntryType) && !parent.US_ProductExclusion.IsEmpty && !forEUNSteel)
						{
							supTariffInfo.AddMessageError(ProvTariffIsRequired);
						}
					}

					var combinedLines = parent.IsCombinedLine() ? parent.GetCombinedLines() : new List<JobComInvoiceLine>() { parent };

					ValidateUS_SupTariffForA99Rule(combinedLines, applicableSupTariffs);
					ValidateUS_SupTariffFor99038809(applicableSupTariffs);
					ValidateUS_SupTariffForR99Rule(applicableSupTariffs);
				}
			}
		}

		bool IsStnTariffRule
		{
			get
			{
				var parentLine = Parent.ParentTariffLine ?? Parent;
				var importTariff = parentLine.ImportTariff;
				return importTariff != null && importTariff.GetTariffRuleIfApplies(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, parentLine.EffectiveDateForDutyRate) != null;
			}
		}

		void ValidateUS_SupTariffForR99Rule(IEnumerable<TariffViewAsCodeDescription> applicableSupTariffs)
		{
			var parent = Parent;

			if (parent.US_SupTariff.IsEmpty)
			{
				var factory = parent.Factory;
				var effectiveDateForDutyRate = parent.EffectiveDateForDutyRate;
				if (applicableSupTariffs.Any(x => USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, x.Tariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs) != null
							&& USRefTariffDataLoader.TariffViewHasRuleWithAttribute(factory, x.Tariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula) != null))
				{
					parent.US_SupTariffInfo.AddMessageError(FormulaActTariffOrNARequired);
				}
			}
		}

		void ValidateUS_SupTariffForA99Rule(List<JobComInvoiceLine> combinedLines, IEnumerable<TariffViewAsCodeDescription> applicableSupTariffs)
		{
			var parent = Parent;
			var uS_SupTariff = parent.US_SupTariff;
			var uS_SupTariffInfo = parent.US_SupTariffInfo;
			if (combinedLines.Count == 1)
			{
				if (parent.US_ProductExclusion.IsEmpty)
				{
					if (uS_SupTariff.IsEmpty)
					{
						if (parent.IsEntrySummaryValidationMode)
						{
							var line = combinedLines[0];
							if ((!line.IsChildLine || line.IsSetVLine) && applicableSupTariffs.All(x => x.IsMandatory) && !parent.IsTariffWithGAEType && !parent.IsSteelOriginFromEUN && !parent.IsSteelOriginFromSpecificCountriesOtherThanEU)
							{
								uS_SupTariffInfo.AddMessageError(ChildLineTariffNotEntered);
							}
						}
					}
					else if (!applicableSupTariffs.Any(x => x.Tariff == uS_SupTariff) && !IsStnTariffRule)
					{
						uS_SupTariffInfo.AddMessageError(ChildLineTariffNotFoundInRuleTariffs);
					}
				}
			}
			else
			{
				if (!combinedLines.Any(line => (applicableSupTariffs.Any(x => x.Tariff == line.US_SupTariff))))
				{
					if (!Chapter98Helper.Is98Tariff(uS_SupTariff))
					{
						uS_SupTariffInfo.AddMessageError(ChildLineTariffNotFoundInRuleTariffs);
					}
				}
			}
		}

		void ValidateUS_SupTariffFor99038809(IEnumerable<TariffViewAsCodeDescription> applicableSupTariffs)
		{
			var parent = Parent;
			if (parent.US_SupTariff.EqualsIgnoringCase("99038809"))
			{
				var uS_DateOfExport = parent.IsChildLine ? parent.ParentTariffLine.US_DateOfExport : parent.US_DateOfExport;
				if (!uS_DateOfExport.IsEmpty
					&& uS_DateOfExport >= parent.DefaultExportDateFor99038809
					&& applicableSupTariffs.Take(2).Count() > 1
					&& applicableSupTariffs.Any(x => x.Tariff.EqualsIgnoringCase("99038809")))
				{
					parent.US_SupTariffInfo.AddMessageError(_99038809CannotBeSelected);
				}
			}
		}

		JobComInvoiceLine GetEffectiveInvoiceLine()
		{
			var parent = Parent;
			var invoiceLine = parent.IsChildLine && parent.JI_Tariff.IsEmpty ? parent.ParentTariffLine : parent;
			var childLines = invoiceLine.ChildLines;
			JobComInvoiceLine effectiveInvoiceLine = null;
			if (invoiceLine.IsParentLine && childLines.Any())
			{
				var firstChildWithTariff = childLines.FirstOrDefault(x => !x.JI_Tariff.IsEmpty);
				effectiveInvoiceLine = invoiceLine.JI_Tariff.IsEmpty && firstChildWithTariff != null ? firstChildWithTariff : invoiceLine;
			}
			else
			{
				effectiveInvoiceLine = invoiceLine;
			}
			return effectiveInvoiceLine;
		}

		internal const string ChildLineTariffNotFoundInRuleTariffs = "Only certain tariffs can be selected as secondary tariff numbers where their primary tariff numbers are applicable for a 'A99' rule.";
		internal const string ChildLineTariffNotEntered = "Tariff number is required";
		internal const string _99038809CannotBeSelected = "Tariff 9903.88.09 cannot be used when the export date is 10 May 2019 or later.";
		internal const string ProvTariffIsRequired = "This is a Quota entry, the Prov/Prog. Tariff is required when the Product Excusion is entered.";
		internal const string ProvTariffIsNotAllowed = "Prov/Prog Tariff number not allowed with the Tariff/Country combination.";
		internal const string FormulaActTariffOrNARequired = "The classification tariff may be eligible for Temporary Duty-Free Treatment under HR 8351, the Formula Act. You must either choose the Formula Act Tariff or N/A if the classification is not eligible.";

		#endregion

		#region Common

		protected void ValidateDataNotNeededForXLine(ZString propertyName, ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				propertyInfo.AddMessageError(string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, propertyName));
			}
		}

		#endregion

		#region Related Objects

		protected new JobComInvoiceLine Parent
		{
			//this is to ensure that the add info properties are not accessed - because they may not have values - Look at US_EntryType on InvoiceLine as an example as to why
			get { return base.Parent.InvoiceLine; }
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get { return Parent; }
		}

		#endregion

		protected override void CheckUS_DOTIndicator()
		{
			base.CheckUS_DOTIndicator();

			if (Parent.Declaration != null && Parent.Declaration.IsACECargoCertificationMode && !Parent.US_DOTIndicator.IsEmpty)
			{
				InvoiceLine.US_DOTIndicatorInfo.AddMessageError(FormalImportAddInfoJobDeclarationValidation.ACECertificationModeMessageError);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckUS_FDAIndicator()
		{
			base.CheckUS_FDAIndicator();

			var isACSFDA = !InvoiceLine.IsACEFDARelevant;
			var declaration = InvoiceLine.Declaration;
			var isFTZACEFDA = declaration != null && declaration.IsFTZAdmission && InvoiceLine.IsACEFDARelevant;

			if (isACSFDA || isFTZACEFDA)
			{
				if (InvoiceLine.IsSetXLine)
				{
					ValidateDataNotNeededForXLine("FDA", InvoiceLine.US_FDAIndicatorInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_FDAIndicatorInfo, Parent.AddInfoLookups.US_OGAIndicatorList);

					if (declaration != null && declaration.IsFDAValidationMode)
					{
						USCTariff supTariff = Parent.ImportSupTariff;
						USCTariff tariff = Parent.ImportTariff;

						bool fDAAdmissibilityReviewRequired = isACSFDA && HasACertainRequirement(tariff, supTariff, x => x.FDAAdmissibilityReviewRequired)
															|| isFTZACEFDA && HasACertainRequirement(tariff, supTariff, x => x.ACEFDAAdmissibilityReviewRequired);

						bool fDAPriorNoticeAndAdmissibilityReviewRequired = isACSFDA && HasACertainRequirement(tariff, supTariff, x => x.FDAPriorNoticeAndAdmissibilityReviewRequired)
																	|| isFTZACEFDA && HasACertainRequirement(tariff, supTariff, x => x.ACEFDAPriorNoticeAndAdmissibilityReviewRequired);

						bool fDAAdmissibilityReviewMayBeRequired = isACSFDA && HasACertainRequirement(tariff, supTariff, x => x.FDAAdmissibilityReviewMayBeRequired)
															|| isFTZACEFDA && HasACertainRequirement(tariff, supTariff, x => x.ACEFDAAdmissibilityReviewMayBeRequired);

						bool fDAPriorNoticeAndAdmissibilityReviewMayBeRequired = isACSFDA && HasACertainRequirement(tariff, supTariff, x => x.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired)
															|| isFTZACEFDA && HasACertainRequirement(tariff, supTariff, x => x.ACEFDAPriorNoticeAndAdmissibilityReviewMayBeRequired);

						bool fDAAdmissibilityReviewDONOTSUBMIT = isACSFDA && HasACertainRequirement(tariff, supTariff, x => x.FDAAdmissibilityReviewDONOTSUBMIT);

						CheckFDAIndicatorForTariff(fDAAdmissibilityReviewRequired, fDAPriorNoticeAndAdmissibilityReviewRequired, fDAAdmissibilityReviewMayBeRequired, fDAPriorNoticeAndAdmissibilityReviewMayBeRequired, fDAAdmissibilityReviewDONOTSUBMIT);
					}
				}
			}
		}

		static bool HasACertainRequirement(USCTariff tariff1, USCTariff tariff2, Func<USCTariff, bool> hasACertainRequirement)
		{
			return tariff1 != null && hasACertainRequirement(tariff1) || tariff2 != null && hasACertainRequirement(tariff2);
		}

		void CheckFDAIndicatorForTariff(bool fDAAdmissibilityReviewRequired,
										bool fDAPriorNoticeAndAdmissibilityReviewRequired,
										bool fDAAdmissibilityReviewMayBeRequired,
										bool fDAPriorNoticeAndAdmissibilityReviewMayBeRequired,
										bool fDAAdmissibilityReviewDONOTSUBMIT)
		{
			if (fDAAdmissibilityReviewRequired ||
				fDAPriorNoticeAndAdmissibilityReviewRequired ||
				fDAAdmissibilityReviewMayBeRequired ||
				fDAPriorNoticeAndAdmissibilityReviewMayBeRequired)
			{
				if (Parent.US_FDAIndicator.IsEmpty)
				{
					Parent.US_FDAIndicatorInfo.AddMessageError(Constants.FDA.FDARequiredButBlank);
				}
			}

			if (fDAAdmissibilityReviewRequired || fDAPriorNoticeAndAdmissibilityReviewRequired)
			{
				if (OGAIndicatorList.IsToBeDisclaimed(Parent.US_FDAIndicator))
				{
					Parent.US_FDAIndicatorInfo.AddMessageError(Constants.FDA.FDARequiredButDisclaimed);
				}
			}

			if (fDAAdmissibilityReviewDONOTSUBMIT)
			{
				if (OGAIndicatorList.IsToBeDisclaimed(Parent.US_FDAIndicator) || OGAIndicatorList.IsToBeDeclared(Parent.US_FDAIndicator))
				{
					Parent.US_FDAIndicatorInfo.AddMessageError(Constants.FDA.FDAMustNotBeSubmitted);
				}
			}

			if (!fDAAdmissibilityReviewRequired && !fDAPriorNoticeAndAdmissibilityReviewRequired && !fDAAdmissibilityReviewMayBeRequired && !fDAPriorNoticeAndAdmissibilityReviewMayBeRequired && !fDAAdmissibilityReviewDONOTSUBMIT)
			{
				if (OGAIndicatorList.IsToBeDeclared(Parent.US_FDAIndicator))
				{
					Parent.US_FDAIndicatorInfo.AddWarning(Constants.FDA.FDANotRequired);
				}
				else if (OGAIndicatorList.IsToBeDisclaimed(Parent.US_FDAIndicator))
				{
					Parent.US_FDAIndicatorInfo.AddMessageError(Constants.FDA.FDADisclaimedInvalid);
				}
			}

			if (!Parent.US_FDAIndicatorInfo.HasMessageErrors())
			{
				var fdacount = Parent.Declaration != null && Parent.Declaration.CanHavePGAFDA ? Parent.ACE_FDALines.Count : Parent.FDAs.Count;

				if (Parent.IsFDADeclared)
				{
					if (fdacount == 0)
					{
						Parent.US_FDAIndicatorInfo.AddMessageError(Constants.FDA.FDALineRequired);
					}
				}
				else
				{
					if (fdacount > 0)
					{
						Parent.US_FDAIndicatorInfo.AddMessageError(Constants.FDA.FDALineNotAllowed);
					}
				}
			}
		}

		protected override void CheckUS_TransactionsRelated()
		{
			base.CheckUS_TransactionsRelated();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TransactionsRelatedInfo, Lookups.US_RelatedOrgList);

			if (Parent.US_TransactionsRelated.IsEmpty && Parent.IsEntrySummaryValidationMode)
			{
				var declaration = Parent.Declaration;
				if (declaration.US_EntryType != EntryTypeList.Codes.InformalFreeDutiable && declaration.US_EntryType != EntryTypeList.Codes.InformalQuotaVisa)
				{
					Parent.US_TransactionsRelatedInfo.AddMessageError(TransactionRelatedRequiredForNonInformalEntry);
				}
			}
		}
		internal const string TransactionRelatedRequiredForNonInformalEntry = "Where there are non-informal entry types, Transaction Related Indicator should be entered. You can fill the information in for all invoice lines by entering the value on invoice header level.";

		#region Common ADD/CVD Case No Validation
		protected override void CheckUS_ADD_NA()
		{
			base.CheckUS_ADD_NA();

			if (Parent.US_ADD_NA && !Parent.US_ADDCaseNo.IsEmpty)
			{
				Parent.US_ADD_NAInfo.AddMessageError(NACannotBeTickedWithCaseNum);
			}
			ValidateUS_ADDCaseNo();
		}

		protected override void CheckUS_CVD_NA()
		{
			base.CheckUS_CVD_NA();

			if (Parent.US_CVD_NA && !Parent.US_CVDCaseNo.IsEmpty)
			{
				Parent.US_CVD_NAInfo.AddMessageError(NACannotBeTickedWithCaseNum);
			}
			ValidateUS_CVDCaseNo();
		}

		internal const string NACannotBeTickedWithCaseNum = @"N/A cannot be ticked if Case information is entered.";

		protected override void CheckUS_ADDCaseNo()
		{
			base.CheckUS_ADDCaseNo();

			if (Parent.US_ADDCaseNo.IsEmpty)
			{
				new ADD_CVDLiabilityChecker().ValidateLiability(Parent.US_ADDCaseNoInfo, Parent, ADD_CVDLiabilityChecker.ADD_CVD.ADD, Parent.ImportSupTariff, Parent.ImportTariff);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ADDCaseNoInfo, Lookups.ADDCaseNumberList);
				CheckAntiDumpingOrCountervailingAgainstEntryType(Parent.US_ADDCaseNoInfo);
				CheckAntiDumpingOrCountervailingAgainstTariff(Parent.US_ADDCaseNoInfo);
				CheckMultipleChildrenAntiDumpingOrCountervailingDutyDetails(Parent.US_ADDCaseNoInfo);

				if (Parent.IsSetXLine)
				{
					Parent.US_ADDCaseNoInfo.AddMessageError(ADDIsNotAllowedOnSetXLine);
				}

				if (Parent.IsDomesticMerchandise)
				{
					Parent.US_ADDCaseNoInfo.AddMessageError(NoADDOrCVDForDomesticMerchandise);
				}

				IACCase addCase = Parent.AntidumpingDutyCase;
				var validateCountryOfOrigin = Parent.US_UC_NKCountryOfOrigin;

				if (validateCountryOfOrigin.StartsWith("X", StringComparison.OrdinalIgnoreCase))
				{
					validateCountryOfOrigin = Core.Constants.CountryCodes.Canada;
				}

				if (addCase != null)
				{
					if (addCase.CountryCode != validateCountryOfOrigin)
					{
						Parent.US_ADDCaseNoInfo.AddMessageError(CaseNoAgainstCountryOfOriginPartialMessage + addCase.CountryCode);
					}

					if (!addCase.IsReportable(InvoiceLine.EffectiveDateForDutyRate))
					{
						Parent.US_ADDCaseNoInfo.AddWarning(ADD_CVDIsNotReportable);
					}
					else if (!addCase.IsEffective)
					{
						string statusDesc = addCase.CaseStatusList.GetDescriptionFromCode(addCase.CaseStatus) ?? addCase.CaseStatus;
						Parent.US_ADDCaseNoInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, ADD_CVDIsNotEffective, addCase.CaseStatus, statusDesc));
					}

					if (Parent.ManufacturerAddress != null && !addCase.ManufacturerIDCode.IsEmpty)
					{
						ZString manufIDCustomsCode = Parent.ManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);

						if (!manufIDCustomsCode.IsEmpty && addCase.ManufacturerIDCode != manufIDCustomsCode)
						{
							Parent.US_ADDCaseNoInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, CVDADDManufacturerIDDiffer, "Antidumping"));
						}
					}
				}
			}
		}

		protected override void CheckUS_CVDCaseNo()
		{
			base.CheckUS_CVDCaseNo();

			if (Parent.US_CVDCaseNo.IsEmpty)
			{
				new ADD_CVDLiabilityChecker().ValidateLiability(Parent.US_CVDCaseNoInfo, Parent, ADD_CVDLiabilityChecker.ADD_CVD.CVD, Parent.ImportSupTariff, Parent.ImportTariff);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_CVDCaseNoInfo, Lookups.CVDCaseNumberList);

				CheckAntiDumpingOrCountervailingAgainstEntryType(Parent.US_CVDCaseNoInfo);
				CheckAntiDumpingOrCountervailingAgainstTariff(Parent.US_CVDCaseNoInfo);
				CheckMultipleChildrenAntiDumpingOrCountervailingDutyDetails(Parent.US_CVDCaseNoInfo);

				if (Parent.IsSetXLine)
				{
					Parent.US_CVDCaseNoInfo.AddMessageError(CVDIsNotAllowedOnSetXLine);
				}

				if (Parent.IsDomesticMerchandise)
				{
					Parent.US_CVDCaseNoInfo.AddMessageError(NoADDOrCVDForDomesticMerchandise);
				}

				IACCase cvdCase = Parent.CountervailingDutyCase;
				var validateCountryOfOrigin = Parent.US_UC_NKCountryOfOrigin;

				if (validateCountryOfOrigin.StartsWith("X", StringComparison.OrdinalIgnoreCase))
				{
					validateCountryOfOrigin = Core.Constants.CountryCodes.Canada;
				}
				if (cvdCase != null)
				{
					if (cvdCase.CountryCode != validateCountryOfOrigin)
					{
						Parent.US_CVDCaseNoInfo.AddMessageError(CaseNoAgainstCountryOfOriginPartialMessage + cvdCase.CountryCode);
					}

					if (!cvdCase.IsReportable(InvoiceLine.EffectiveDateForDutyRate))
					{
						Parent.US_CVDCaseNoInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, ADD_CVDIsNotReportable));
					}
					else if (!cvdCase.IsEffective)
					{
						string statusDesc = cvdCase.CaseStatusList.GetDescriptionFromCode(cvdCase.CaseStatus) ?? cvdCase.CaseStatus;
						Parent.US_CVDCaseNoInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, ADD_CVDIsNotEffective, cvdCase.CaseStatus, statusDesc));
					}

					if (Parent.ManufacturerAddress != null && !cvdCase.ManufacturerIDCode.IsEmpty)
					{
						ZString manufIDCustomsCode = Parent.ManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);

						if (!manufIDCustomsCode.IsEmpty && cvdCase.ManufacturerIDCode != manufIDCustomsCode)
						{
							Parent.US_CVDCaseNoInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, CVDADDManufacturerIDDiffer, "Countervailing"));
						}
					}
				}
			}
		}

		protected bool HasValidEntryTypeForADD_CVD
		{
			get { return Parent.IsACE ? EntryTypeList.IsValidForADD_CVDForACE(Parent.ImportEntryType) : EntryTypeList.IsValidForADD_CVD(Parent.ImportEntryType); }
		}

		void CheckAntiDumpingOrCountervailingAgainstEntryType(ZPropertyInfo info)
		{
			if (InvoiceLine.IsEntrySummaryValidationMode && !info.Value.IsEmpty && !HasValidEntryTypeForADD_CVD)
			{
				info.AddMessageError(ADD_CVDEnteredForAnInvalidEntryType);
			}
		}

		void CheckMultipleChildrenAntiDumpingOrCountervailingDutyDetails(ZPropertyInfo info)
		{
			if (InvoiceLine.IsEntrySummaryValidationMode && !info.Value.IsEmpty && InvoiceLine.ParentTariffLine != null)
			{
				JobComInvoiceLine parentTariffLine = InvoiceLine.ParentTariffLine;

				IZType parentValue = (IZType)parentTariffLine[info.PropertyDescriptor.Name];

				if (!parentValue.IsEmpty)
				{
					info.AddMessageError(SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
				}

				foreach (JobComInvoiceLine secondaryLine in parentTariffLine.SecondaryTariffLines)
				{
					if (secondaryLine != InvoiceLine)
					{
						IZType diffSecondaryLineValue = (IZType)secondaryLine[info.PropertyDescriptor.Name];

						if (!diffSecondaryLineValue.IsEmpty)
						{
							info.AddMessageError(AnotherSecondaryHasADD_CVD);
							break;
						}
					}
				}

				CusEntryLine lineWithHighestDutyForDerived = GetEntryLineWithHighestDutyRateIfDerived();

				if (lineWithHighestDutyForDerived != null && lineWithHighestDutyForDerived.CL_AdValoremTariff != Parent.JI_Tariff)
				{
					info.AddMessageError(ThisTariffWillBeIgnoredDueToDerivedDutyCalculationAndADD_CVDDetailsNotValid);
				}
			}
		}
		void CheckAntiDumpingOrCountervailingAgainstTariff(ZPropertyInfo info)
		{
			if (InvoiceLine.IsEntrySummaryValidationMode && !info.Value.IsEmpty)
			{
				if (Parent.US_SupTariff.StartsWith("98020040", StringComparison.OrdinalIgnoreCase) || Parent.US_SupTariff.StartsWith("98020050", StringComparison.OrdinalIgnoreCase))
				{
					info.AddMessageError(ADD_CVDNotAllowedForRepairOrAlteration);
				}
			}
		}

		protected CusEntryLine GetEntryLineWithHighestDutyRateIfDerived()
		{
			CusEntryLine result = null;

			JobComInvoiceLine parentTariffLine = InvoiceLine.ParentTariffLine;

			if (parentTariffLine != null &&
				parentTariffLine.CusEntryLine != null && //merged
				parentTariffLine.ImportTariff != null && //derived duty calculation
				parentTariffLine.ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived &&
				parentTariffLine.CusEntryLine.ChildLines.Count == 1)
			{
				result = parentTariffLine.CusEntryLine.ChildLines[0];
			}

			return result;
		}

		public const string ADD_CVDNotAllowedForRepairOrAlteration = "ADD/CVD details may not be reported with the selected repair/alteration tariff.";
		public const string ADD_CVDEnteredForAnInvalidEntryType = "ADD/CVD details may not be applied to the selected entry type.  Change to an entry type that supports ADD/CVD.";
		public const string ThisTariffWillBeIgnoredDueToDerivedDutyCalculationAndADD_CVDDetailsNotValid = "This line has a lower duty rate and won't be sent in 7501 due to the derived duty calculation requirement. ADD/CVD details entered here will not match the tariff with the highest duty rate and won't be accepted by Customs. Please contact Customs to declare this entry manually.";
		public const string SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD = "You have entered ADD/CVD details here as well as against its Parent line. Due to Entry Summary message limitation, this cannot be sent. Please contact Customs to declare this entry manually.";
		public const string AnotherSecondaryHasADD_CVD = "You have entered ADD/CVD details here as well as against another secondary line. Due to Entry Summary message limitation, this cannot be sent. Please contact Customs to declare this entry manually.";
		internal const string CaseNoAgainstCountryOfOriginPartialMessage = "The selected case is only applicable for ";
		internal const string ADD_CVDIsNotEffective = "This case is not effective. Its current status is {0}({1}).";
		internal const string ADD_CVDIsNotReportable = "This case may not be reportable based on Liquidation Suspension information provided by CBP. Please verify";
		internal const string CVDADDManufacturerIDDiffer = "Manufacturer ID for the selected {0} case differs from the Manufacturer ID.";
		internal const string CVDIsNotAllowedOnSetXLine = "CVD is not allowed on set X line.";
		internal const string ADDIsNotAllowedOnSetXLine = "ADD is not allowed on set X line.";
		public const string NoADDOrCVDForDomesticMerchandise = "You have indicated that this is a domestic merchandise and it is subject to no duty or fees.";
		#endregion
	}
}
