using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.AWB;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public abstract class FBase : CargoIMP
	{
		public const int OCISectionMaxLength = 1600;
		readonly struct MessagingConstants
		{
			public const string MessagingUnit = "MU";
			public const string MessagingDestination = "MD";
		}

		protected FBase(IFWBMessageDetailsProvider fwbDetailsProvider)
			: base()
		{
			FWBDetailsProvider = fwbDetailsProvider;
		}

		public readonly IFWBMessageDetailsProvider FWBDetailsProvider;

		protected abstract CharType OCIMUFormatType { get; }

		protected abstract CharType OCIMDFormatType { get; }

		#region AWB Consignment Details

		protected ElementList AWBIdentification
		{
			get
			{
				ElementList result = new ElementList();

				//Airline Identification
				//Airline Prefix
				result.AddValue(new Format(3, CharType.Numeric), FWBDetailsProvider.AirlinePrefix);
				result.AddHyphen();
				//Serial Number
				result.AddValue(new Format(8, 0, CharType.Numeric), FWBDetailsProvider.AWBSerialNo);

				return result;
			}
		}

		protected ElementList AWBOriginAndDestination
		{
			get
			{
				ElementList result = new ElementList();
				//Origin			
				result.AddValue(new Format(3, CharType.Alpha), FWBDetailsProvider.AWBOriginCode);
				//Destination
				result.AddValue(new Format(3, CharType.Alpha), FWBDetailsProvider.AirportOfDestinationCode);

				return result;
			}
		}

		protected ElementList AWBQuantityDetail
		{
			get
			{
				ElementList result = new ElementList();
				result.AddSlant();
				result.AddColumnIdentifier("T");
				//Number of Pieces //slac
				result.AddValue(new Format(4, 0, CharType.Numeric), FWBDetailsProvider.TotalNoOfPieces);
				string weightCode = "";
				if (FWBDetailsProvider.AWBRateLines.Any())
				{
					weightCode = FWBDetailsProvider.AWBRateLines.First().WeightInLBsOrKGs;
				}
				//Weight Code
				result.AddValue(new Format(1, CharType.Alpha), weightCode);
				//Weight
				result.AddValue(new Format(7, 0, CharType.NumericWithDecimal), FWBDetailsProvider.TotalGrossWeight);

				return result;
			}
		}

		#endregion

		#region Party Elements

		protected ElementList GetPartyElements(bool includeAccount, ZString lineIdentifier, ZString account, ZString name, ZString address, ZString address2, ZString place, ZString state, ZString countryCode, ZString postCode, ZString contactCode, ZString contactDetail)
		{
			ElementList result = new ElementList();
			result.AddLineIdentifier(lineIdentifier);

			ElementList valueElements;

			//Account			
			if (includeAccount && !account.IsCargoIMPEmpty())
			{
				valueElements = new ElementList();
				valueElements.AddSlant();
				valueElements.AddValue(new Format(14, 0, CharType.Text), account);
				result.AddOptionalHeader(valueElements);
			}

			if (includeAccount)
			{
				result.AddCRLF();
			}

			//Name
			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(35, 0, CharType.Text), name);
			valueElements.AddCRLF();
			result.AddHeader(valueElements);

			//Address
			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(35, 0, CharType.Text), (address + " " + address2).Trim());
			valueElements.AddCRLF();
			result.AddHeader(valueElements);

			//Location
			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(17, 0, CharType.Text), place);
			valueElements.AddSlant(StatusType.Conditional);
			valueElements.AddOptionalValue(new Format(9, 0, CharType.Text), state);
			valueElements.AddCRLF();
			result.AddHeader(valueElements);

			//Contact Detail (Can be repeated, but we dont support repeats at this point)
			ElementList contactDetailElements = new ElementList();
			contactDetailElements.AddSlant();
			if (!contactCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric) &&
				!contactDetail.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric))
			{
				contactDetailElements.AddValue(new Format(3, 0, CharType.AlphaNumeric), contactCode);
				contactDetailElements.AddSlant();
				contactDetailElements.AddValue(new Format(25, 0, CharType.AlphaNumeric), contactDetail);
			}
			else
			{
				contactDetailElements.AddSlant();
			}

			//Coded Location
			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(2, CharType.Alpha), countryCode);

			bool includePostcode = !postCode.IsCargoIMPEmpty(CIMPFieldFormats.AlphaNumeric);
			bool includeContact = contactDetailElements.ToStringValueTypes().Length != 0;

			if (includePostcode || includeContact)
			{
				valueElements.AddSlant();
			}

			if (includePostcode)
			{
				valueElements.AddOptionalValue(new Format(9, 0, CharType.Text), postCode.Trim());
			}

			result.AddHeader(valueElements);
			result.AddOptionalHeader(contactDetailElements);

			result.AddCRLF();
			return result;
		}

		#endregion

		#region Charges

		protected void AddChargeDeclarations(IFBaseMessageDetailsProvider awbDetailsProvider, bool includeChargeCode)
		{
			ElementList chargeDeclarations = new ElementList();
			chargeDeclarations.AddLineIdentifier("CVD");

			ElementList valueElements = new ElementList();
			//ISO Currency Code
			valueElements.AddSlant();
			valueElements.AddValue(new Format(3, CharType.Alpha), awbDetailsProvider.Currency);
			valueElements.AddSlant();
			//Charge Code
			if (includeChargeCode)
			{
				valueElements.AddValue(new Format(2, CharType.Alpha), awbDetailsProvider.ChargesCode);
				valueElements.AddSlant();
			}
			chargeDeclarations.AddHeader(valueElements);

			valueElements = new ElementList();
			//Weight PPD or COL
			string weightPPDorCOL = awbDetailsProvider.WeightVPPDCOL.Left(1) == ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid ? ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid : ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			valueElements.AddValue(new Format(1, CharType.Alpha), weightPPDorCOL);
			//Other PPD or COL
			string otherPPDorCOL = awbDetailsProvider.OtherPPDCOL.Left(1) == ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid ? ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid : ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			valueElements.AddValue(new Format(1, CharType.Alpha), otherPPDorCOL);
			chargeDeclarations.AddConditionalHeader(valueElements);

			//Value For Carriage
			valueElements = new ElementList();
			valueElements.AddSlant();
			if (awbDetailsProvider.DeclaredValue > 0M)
			{
				valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), awbDetailsProvider.DeclaredValue);
			}
			else
			{
				valueElements.AddValue(new Format(3, CharType.Alpha), "NVD");
			}
			chargeDeclarations.AddHeader(valueElements);

			chargeDeclarations.AddSlant();

			//Value For Customs
			valueElements = new ElementList();
			if (awbDetailsProvider.CustomsValue > 0M)
			{
				valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), awbDetailsProvider.CustomsValue);
			}
			else
			{
				valueElements.AddValue(new Format(3, CharType.Alpha), "NCV");
			}
			chargeDeclarations.AddHeader(valueElements);

			//Value For Insurance
			valueElements = new ElementList();
			valueElements.AddSlant();
			if (awbDetailsProvider.InsuranceValue > 0M)
			{
				valueElements.AddValue(new Format(11, 0, CharType.NumericWithDecimal), awbDetailsProvider.InsuranceValue);
			}
			else
			{
				valueElements.AddValue(new Format(3, CharType.Alpha), "XXX");
			}
			chargeDeclarations.AddHeader(valueElements);

			chargeDeclarations.AddCRLF();

			Elements.AddHeader(chargeDeclarations);
		}

		#endregion

		protected void AddDGCodes(IEnumerable<ZString> dgCodes)
		{
			foreach (var dgCode in dgCodes.Where(x => !x.IsEmpty))
			{
				AddOCILine(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "DNR", "D", dgCode); // DNR = DGD Item Number, D = Dangerous Goods
			}
		}

		protected ElementList OCISection
		{
			get
			{
				if (ociSection == null)
				{
					ociSection = new ElementList();
					ociSection.AddLineIdentifier("OCI");
				}

				return ociSection;
			}
		}

		ElementList ociSection;

		protected virtual void AddVATAndContactNumbersToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			if (!awbDetailsProvider.ShipperTraderNo.IsEmpty)
			{
				AddOCILine(
					awbDetailsProvider.ShipperCountryCode,
					"SHP",
					ExplicitNumberPrefix,
					CompanyIdAndNumberUtils.GetShipperTraderCode(awbDetailsProvider));
			}

			var shipperContactHandler = awbDetailsProvider.ContactNumberCountryHandlers.First(h => h.ShipperApplicable());
			AddContactNumbersToOCI(
				awbDetailsProvider.AirlineContactNameOCIIdentifier,
				awbDetailsProvider.AirlineContactPhoneOCIIdentifier,
				shipperContactHandler.ShipperCountryCodeForContactNumber(),
				"SHP",
				awbDetailsProvider.ShipperContactName,
				awbDetailsProvider.ShipperContactCode,
				awbDetailsProvider.ShipperContactDetail,
				shipperContactHandler.UseAirlineIdentifierForShipper);

			// ACAS line, shipper contact email
			AddContactEmailToOCI(awbDetailsProvider, "SHP");

			AddOCILineConsigneeTraderNo(awbDetailsProvider);

			var consigneeContactHandler = awbDetailsProvider.ContactNumberCountryHandlers.First(h => h.ConsigneeApplicable());
			AddContactNumbersToOCI(
				awbDetailsProvider.AirlineContactNameOCIIdentifier,
				awbDetailsProvider.AirlineContactPhoneOCIIdentifier,
				consigneeContactHandler.ConsigneeCountryCodeForContactNumber(),
				"CNE",
				awbDetailsProvider.ConsigneeContactName,
				awbDetailsProvider.ConsigneeContactCode,
				awbDetailsProvider.ConsigneeContactDetail,
				consigneeContactHandler.UseAirlineIdentifierForConsignee);

			// ACAS line, consignee contact email
			AddContactEmailToOCI(awbDetailsProvider, "CNE");

			AddOCILineAlsoNotifyTraderNo(awbDetailsProvider);

			var alsoNotifyContactHandler = awbDetailsProvider.ContactNumberCountryHandlers.First(h => h.AlsoNotifyApplicable());
			AddContactNumbersToOCI(
				awbDetailsProvider.AirlineContactNameOCIIdentifier,
				awbDetailsProvider.AirlineContactPhoneOCIIdentifier,
				alsoNotifyContactHandler.AlsoNotifyCountryCodeForContactNumber(),
				"NFY",
				awbDetailsProvider.AlsoNotifyContactName,
				awbDetailsProvider.AlsoNotifyContactCode,
				awbDetailsProvider.AlsoNotifyContactDetail,
				alsoNotifyContactHandler.UseAirlineIdentifierForAlsoNotify);

			// ACAS lines
			if (awbDetailsProvider.ACASCountryHandler?.ShouldApplyACAS() == true)
			{
				AddVerifiedKnownConsignorToOCI(awbDetailsProvider);
				AddCustomerAccountHolderAndNameToOCI(awbDetailsProvider);
				AddCustomerAccountIssuerAndNumberToOCI(awbDetailsProvider);
				AddAccountEstablishmentDateToOCI(awbDetailsProvider);
				AddAccountBillingTypeToOCI(awbDetailsProvider);
				AddCustomerAccountShippingFrequencyToOCI(awbDetailsProvider);
				AddIPAddressesToOCI(awbDetailsProvider);
				AddBiographicDataToOCI(awbDetailsProvider);
			}
		}

		protected void AddCustomerAccountIssuerAndNumberToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			/*
				/US/CUS/AI/LH
				/US/CUS/AR/436123L
			 */
			if (awbDetailsProvider.ACASCountryHandler?.GetCustomerAccountIssuerAndNumber(out var accountIssuer, out var accountNumber) == true)
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "AI", accountIssuer);
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "AR", accountNumber);
			}
		}

		protected void AddCustomerAccountHolderAndNameToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			/*
				/US/CUS/AH/S
				/US/CUS/AN/CUSTOMER NAME
			 */
			if (awbDetailsProvider.ACASCountryHandler?.GetCustomerAccountHolderAndName(out var accountHolder, out var accountName) == true)
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "AH", accountHolder);
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "AN", accountName);
			}
		}

		protected void AddCustomerAccountShippingFrequencyToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			var shippingFrequency = awbDetailsProvider.ACASCountryHandler?.GetCustomerAccountShippingFrequency();
			if (!string.IsNullOrEmpty(shippingFrequency))
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "AF", shippingFrequency);
			}
		}

		protected void AddAccountEstablishmentDateToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			/*
				/US/CUS/AE/22FEB16
			*/
			if (awbDetailsProvider.ACASCountryHandler?.GetCustomerAccountEstablishmentDate(out var establishmentDate) == true)
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "AE", establishmentDate);
			}
		}

		protected void AddAccountBillingTypeToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			/*
				/US/CUS/BT/EFT
			*/
			if (awbDetailsProvider.ACASCountryHandler?.GetCustomerAccountBillingType(out var billingType) == true)
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "BT", billingType);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constants")]
		protected void AddContactEmailToOCI(IFBaseMessageDetailsProvider awbDetailsProvider, ZString prefix)
		{
			if (awbDetailsProvider.ACASCountryHandler == null || !awbDetailsProvider.ACASCountryHandler.ShouldApplyACAS())
			{
				return;
			}

			// IATA mapping, if the OCI email line appears, the country code must be US but not actual country code of the email holder
			var countryCode = Constants.CountryCodes.UnitedStates;
			var contactEmail = prefix == "SHP" ? awbDetailsProvider.ShipperContactEmail : awbDetailsProvider.ConsigneeContactEmail;
			if (contactEmail.IsValid && contactEmail.Contains('@'))
			{
				if (contactEmail.StartsWith("Em:"))
				{
					contactEmail = contactEmail.TrimStart("Em:".ToCharArray()).TrimStart();
				}

				// The email address someone@domain.com splits to 2 lines for FWB/FHL:
				// US/SHP/MU/SOMEONE
				// US/SHP/MD/DOMAIN.COM
				var emailParts = contactEmail.Split('@');
				if (emailParts.Length > 1)
				{
					AddOCILine(countryCode, prefix, MessagingConstants.MessagingUnit, emailParts[0]);
					AddOCILine(countryCode, prefix, MessagingConstants.MessagingDestination, emailParts[1]);
				}
			}
		}

		protected void AddVerifiedKnownConsignorToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			if (awbDetailsProvider.ACASCountryHandler == null || !awbDetailsProvider.ACASCountryHandler.ShouldApplyACAS())
			{
				return;
			}

			var countryCode = Constants.CountryCodes.UnitedStates;
			if (awbDetailsProvider.ACASCountryHandler.IsVerifiedKnownConsignor())
			{
				AddOCILine(countryCode, "CUS", "KP", "Y");
			}
			else
			{
				AddOCILine(countryCode, "CUS", "KP", "N");
			}
		}

		protected void AddIPAddressesToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			var ipOfCustomerAccountCreation = "127.0.0.1";
			if (!string.IsNullOrEmpty(ipOfCustomerAccountCreation))
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "IA", ipOfCustomerAccountCreation);
			}

			var ipOfAWBCreation = "127.0.0.1";
			if (!string.IsNullOrEmpty(ipOfAWBCreation))
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "IR", ipOfAWBCreation);
			}
		}

		protected void AddBiographicDataToOCI(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			var bioData = awbDetailsProvider.ACASCountryHandler?.GetBiographicData();
			if (bioData != null && bioData.Value.isNaturalPersonOrg && !string.IsNullOrEmpty(bioData.Value.idNumber))
			{
				AddOCILine(Constants.CountryCodes.UnitedStates, "CUS", "PI",
					$"{bioData.Value.idType}-{bioData.Value.idIssuer}-{bioData.Value.idNumber}");
			}
		}

		protected string ExplicitNumberPrefix => "T";

		void AddContactNumbersToOCI(ZString? airlineContactNameOCIIdentifier, ZString? airlineContactPhoneOCIIdentifier, ZString countryCode, ZString prefix, ZString contactName, ZString contactCode, ZString contactDetail, ZBool useAirlineIdentifier)
		{
			var contactNameId = useAirlineIdentifier ? airlineContactNameOCIIdentifier ?? ZString.Empty : (ZString)"CP";
			if (!contactNameId.IsEmpty && !contactName.IsEmpty)
			{
				AddOCILine(countryCode, prefix, contactNameId, contactName);
			}

			var contactPhoneId = useAirlineIdentifier ? airlineContactPhoneOCIIdentifier ?? ZString.Empty : (ZString)"CT";
			if (!contactPhoneId.IsEmpty && !contactDetail.IsEmpty &&
				(contactCode.IsEmpty || contactCode == Constants.AWB.ContactCodes.TELEPHONE))
			{
				var numericalContactDetail = contactDetail.TrimStart('+');
				AddOCILine(countryCode, prefix, contactPhoneId, numericalContactDetail);
			}
		}

		protected void AddOCILineConsigneeTraderNo(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			var consigneeTraderCode = CompanyIdAndNumberUtils.GetConsigneeTraderCode(awbDetailsProvider);

			if (string.IsNullOrEmpty(consigneeTraderCode))
			{
				return;
			}

			if (awbDetailsProvider.IsDeclarantForAdvancedCargoReporting)
			{
				AddOCILine(
					awbDetailsProvider.ConsigneeCountryCode,
					"DCL",
					ExplicitNumberPrefix,
					consigneeTraderCode);
			}
			else
			{
				AddOCILine(
					awbDetailsProvider.ConsigneeCountryCode,
					"CNE",
					ExplicitNumberPrefix,
					consigneeTraderCode);
			}
		}

		protected void AddOCILineAlsoNotifyTraderNo(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			var alsoNotifyTraderCode = CompanyIdAndNumberUtils.GetAlsoNotifyTraderCode(awbDetailsProvider);

			if (string.IsNullOrEmpty(alsoNotifyTraderCode))
			{
				return;
			}

			AddOCILine(
					awbDetailsProvider.AlsoNotifyCountryCode,
					"NFY",
					ExplicitNumberPrefix,
					alsoNotifyTraderCode);
		}

		protected void AddOCILine(ZString identifier, ZString info)
		{
			AddOCILine(ZString.Empty, ZString.Empty, identifier, info);
		}

		protected void AddOCILine(ZString countryCode, ZString informationIdentifier, ZString securityInfoIdentifier, ZString supplementaryInfo)
		{
			var ociLine = CreateOCILine(countryCode, informationIdentifier, securityInfoIdentifier, supplementaryInfo);
			var excessLineLength = ociSectionLength + ociLine.ToString().Length - OCISectionMaxLength;
			var maxLineLength = supplementaryInfo.Length - excessLineLength;

			if (maxLineLength > 0)
			{
				if (excessLineLength > 0)
				{
					supplementaryInfo = supplementaryInfo.Substring(0, maxLineLength - 1);
					ociLine = CreateOCILine(countryCode, informationIdentifier, securityInfoIdentifier, supplementaryInfo);
				}

				OCISection.AddOptionalHeader(ociLine);
				ociSectionLength += ociLine.ToString().Length;
			}
		}

		ElementList CreateOCILine(ZString countryCode, ZString informationIdentifier, ZString securityInfoIdentifier, ZString supplementaryInfo)
		{
			var ociLine = new ElementList();
			ociLine.AddSlant();
			ociLine.AddOptionalValue(new Format(2, CharType.Alpha), countryCode);
			ociLine.AddSlant();
			ociLine.AddOptionalValue(new Format(3, CharType.Alpha), informationIdentifier);
			ociLine.AddSlant();
			ociLine.AddOptionalValue(new Format(2, 1, CharType.Alpha), securityInfoIdentifier);
			ociLine.AddSlant();
			if(securityInfoIdentifier == MessagingConstants.MessagingUnit)
			{
				ociLine.AddValue(new Format(35, 1, OCIMUFormatType), supplementaryInfo);
			} else if(securityInfoIdentifier == MessagingConstants.MessagingDestination)
			{
				ociLine.AddValue(new Format(35, 1, OCIMDFormatType), supplementaryInfo);
			}
			else
			{
				ociLine.AddValue(new Format(35, 1, CharType.Text), supplementaryInfo);
			}
			
			ociLine.AddCRLF();
			return ociLine;
		}

		protected void AddOCILines(ZString identifier, IEnumerable<ZString> infos)
		{
			AddOCILines(ZString.Empty, ZString.Empty, identifier, infos);
		}

		protected void AddOCILines(ZString countryCode, ZString informationIdentifier, ZString securityInfoIdentifier, IEnumerable<ZString> infos)
		{
			foreach (var info in infos)
			{
				AddOCILine(countryCode, informationIdentifier, securityInfoIdentifier, info);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constants")]
		protected void AddOCIExportStatements(IFBaseMessageDetailsProvider awbDetailsProvider)
		{
			var ociExportStatementCodeTranslations = new Dictionary<string, string>
			{
				{ "PRF", "AES" },
				{ "PDU", "PDF" },
				{ "DWN", "AED" },
				{ "LOW", "AES NOEEI EXC" },
			};

			var isCurrentCountryInUsaAndTerritories = Constants.CountryCodes.UsaAndTerritoriesList.Any(_ => _ == GlbBranch.CurrentBranch.Country.Code);

			if (isCurrentCountryInUsaAndTerritories)
			{
				var exportStatements = awbDetailsProvider.ExportStatements.DistinctBy(_ => new { _.Code, _.Statement }).ToList();

				foreach (var exportStatementSetting in exportStatements)
				{
					if (ociExportStatementCodeTranslations.ContainsKey(exportStatementSetting.Code))
					{
						AddOCILine(GlbBranch.CurrentBranch.Country.Code, "EXP", "M",
							$"{ociExportStatementCodeTranslations[exportStatementSetting.Code]} {exportStatementSetting.Statement.Trim()}");
					}
				}
			}
		}

		protected IRequiredTaxNumberHelper RequiredTaxNumberHelper => requiredTaxNumberHelper ?? (requiredTaxNumberHelper = ObjectFactory.Get<IRequiredTaxNumberHelper>());
		IRequiredTaxNumberHelper requiredTaxNumberHelper;
		int ociSectionLength;

		protected void AddOCIForMRNs(IReadOnlyCollection<IAWBMovementReferenceNumberMessageDetailsProvider> movementReferenceNumberMessageDetailsProviders)
		{
			foreach (var movementReferenceNumbers in movementReferenceNumberMessageDetailsProviders)
			{
				if (movementReferenceNumbers == null || movementReferenceNumbers.Numbers == null || !movementReferenceNumbers.Numbers.Any())
				{
					continue;
				}

				foreach (var movementReferenceNumber in movementReferenceNumbers.Numbers)
				{
					if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Italy)
					{
						AddOCILineWithCommunityTransitStatusCode(movementReferenceNumbers.CountryOfIssue, movementReferenceNumbers.MovementCode, "M", movementReferenceNumber.Trim(), movementReferenceNumbers.CommunityTransitStatusCode);
					}
					else
					{
						AddOCILine(movementReferenceNumbers.CountryOfIssue, movementReferenceNumbers.MovementCode, "M", movementReferenceNumber.Trim());
					}
				}

				if (movementReferenceNumbers.RelatedNumbers != null)
				{
					foreach (var relatedNumber in movementReferenceNumbers.RelatedNumbers)
					{
						AddOCILine(ZString.Empty, relatedNumber.Type, "I", relatedNumber.Number);
					}
				}
			}
		}

		protected void AddOCIForGDRNs(IReadOnlyCollection<IAWBGoodsDeclarationReferenceNumberMessageDetailsProvider> goodsDeclarationReferenceNumberMessageDetailsProviders)
		{
			foreach (var goodsDeclarationReferenceNumber in goodsDeclarationReferenceNumberMessageDetailsProviders)
			{
				if (goodsDeclarationReferenceNumber == null || goodsDeclarationReferenceNumber.Numbers == null || goodsDeclarationReferenceNumber.Numbers.Count == 0)
				{
					continue;
				}

				goodsDeclarationReferenceNumber.Numbers.ForEach(n => AddOCILine(goodsDeclarationReferenceNumber.CountryOfIssue, goodsDeclarationReferenceNumber.MovementCode, "M", n));
			}
		}

		protected void AddOCIForAcidNumbers(ZString handlingInformation)
		{
			var handlingInformationLines = handlingInformation.Split(System.Environment.NewLine);

			foreach (var handlingInformationLine in handlingInformationLines)
			{
				if (handlingInformationLine.Contains((NoResString)"ACID Number:"))
				{
					var acidNumbersText = handlingInformationLine.Replace("ACID Number:", "");

					var acidNumbers = acidNumbersText.Split(",");
					foreach (var acidNumber in acidNumbers)
					{
						AddOCILine(Core.Constants.CountryCodes.Egypt, "IMP", "M", acidNumber);
					}
				}
			}
		}

		protected void AddOCILineWithCommunityTransitStatusCode(ZString countryCode, ZString informationIdentifier, ZString securityInfoIdentifier, ZString movementReferenceNumber, ZString communityTransitStatusCode)
		{
			var ociLine = CreateOCILine(countryCode, informationIdentifier, securityInfoIdentifier, movementReferenceNumber);
			var ctStatusLine = new ElementList();
			if (!communityTransitStatusCode.IsEmpty)
			{
				ctStatusLine = CreateCommunityTransitStatusLine(communityTransitStatusCode);
			}

			var excessLineLength = ociSectionLength + ociLine.ToString().Length + ctStatusLine.ToString().Length - OCISectionMaxLength;

			if (excessLineLength < 0)
			{
				OCISection.AddOptionalHeader(ociLine);
				ociSectionLength += ociLine.ToString().Length;
				if (!communityTransitStatusCode.IsEmpty)
				{
					OCISection.AddOptionalHeader(ctStatusLine);
					ociSectionLength += ctStatusLine.ToString().Length;
				}
			}
		}

		ElementList CreateCommunityTransitStatusLine(ZString communityTransitStatusCode)
		{
			var ctStatusLine = new ElementList();
			ctStatusLine.AddSlant();
			ctStatusLine.AddSlant();
			ctStatusLine.AddOptionalValue(new Format(3, CharType.Alpha), "COR");
			ctStatusLine.AddSlant();
			ctStatusLine.AddSlant();
			ctStatusLine.AddOptionalValue(new Format(5, CharType.AlphaNumeric), communityTransitStatusCode);
			ctStatusLine.AddCRLF();
			return ctStatusLine;
		}
	}
}
