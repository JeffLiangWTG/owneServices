using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class PortMessagingValidationHelperTest : TestCaseWithFactory
	{
		[TestDate(2023, 3, 1)]
		public static void AssertEntryTypeValidation(ZPropertyInfoString entryTypeInfo)
		{
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				var expectedError = "Enter a valid Entry Type.";
				var expectedWarning = "Entry Type AE1: from <2023-01-01> it will be mandatory to provide EORI and LRN Local Reference Number as part of the new ATLAS Release 3.0 one-stage AES Procedure.";

				AssertNoWarning("Expected no Warning", entryTypeInfo, expectedWarning);

				entryTypeInfo.Value = "YYY";
				AssertHasError("Expected to have an error as 'YYY' is not a valid Entry Type", entryTypeInfo, expectedError);

				entryTypeInfo.Value = EntryTypeList.Codes.OtherExemptions;
				AssertNoError("Expected no errors as Entry type is valid", entryTypeInfo, expectedError);

				entryTypeInfo.Value = ZString.Empty;
				AssertNoError("Expected no errors with an empty Entry Type", entryTypeInfo, expectedError);

				entryTypeInfo.Value = EntryTypeList.Codes.AE1ExportDeclaration;
				AssertHasWarning("Expected to have an Warning", entryTypeInfo, expectedWarning);
			}

			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 3, 2)))
			{
				var expectedError = "Entry Type AE1: the new ATLAS Release 3.0 one-stage AES Procedure can be sent from <2023-03-02>.";

				entryTypeInfo.Value = EntryTypeList.Codes.OtherExemptions;
				AssertNoError("Expected no errors", entryTypeInfo, expectedError);

				entryTypeInfo.Value = EntryTypeList.Codes.AE1ExportDeclaration;
				AssertHasError("Expected to have error", entryTypeInfo, expectedError);
			}
		}

		public static void AssertEntryTypeForPortsValidation(ZPropertyInfoString entryTypeInfo, ForwardingShipment shipment, Action validationAction)
		{
			var consol = shipment.Consols[0];
			var factory = consol.Factory;
			var expectedError = "Entry Type EUB can only be used for movements of union products for on-carriage transports from Hamburg to other EU ports.";

			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = new DateTime(2021, 1, 1);
			entryTypeInfo.Value = EntryTypeList.Codes.EUPortOfDestination;
			consol.JK_RL_NKLoadPort = "DEHAM";
			shipment.JS_RL_NKOrigin = "DEHAM";

			var london = new RefUNLOCO.Loader(factory).Load("GBLON");
			if (london.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion)
			{
				london.Country.RN_EconomicGrouping = ZString.Empty;
			}

			consol.JK_RL_NKDischargePort = "GBLON";
			shipment.JS_RL_NKDestination = "GBLON";
			validationAction();
			AssertHasError("Expected to have an error as 'EUB' is not a valid Entry Type for movements from DEHAM to GB (except Northern Ireland)", entryTypeInfo, expectedError);

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_ETD = new DateTime(2021, 1, 1);
			validationAction();
			AssertHasError("Expected to have an error as 'EUB' is not a valid Entry Type for movements from DEHAM to GB (except Northern Ireland)", entryTypeInfo, expectedError);

			transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			validationAction();
			AssertHasError("Expected to have an error as 'EUB' is not a valid Entry Type for movements from DEHAM to GB (except Northern Ireland)", entryTypeInfo, expectedError);

			entryTypeInfo.Value = EntryTypeList.Codes.ConsolidatedContainer;
			validationAction();
			AssertNoErrors("Expected no error for 'SAC'", entryTypeInfo);

			var belfast = new RefUNLOCO.Loader(factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nOrThErN IRelAnd";
			}
			belfast.Country.RN_EconomicGrouping = ZString.Empty;

			consol.JK_RL_NKDischargePort = belfast.Code;
			shipment.JS_RL_NKDestination = belfast.Code;
			entryTypeInfo.Value = EntryTypeList.Codes.EUPortOfDestination;
			validationAction();
			AssertNoErrors("Expected no error for 'EUB' as movement from DEHAM to Northern Ireland is exempted", entryTypeInfo);

			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = new DateTime(2022, 1, 1);
			AssertEquals("Precondition: Shipment ETD", new DateTime(2021, 1, 1), shipment.JS_E_DEP);
			consol.JK_RL_NKDischargePort = "GBLON";
			shipment.JS_RL_NKDestination = "GBLON";
			validationAction();
			AssertHasError("Expected to have an error as 'EUB' is not a valid Entry Type for movements from DEHAM to GB (except Northern Ireland) when shipment ETD is on or after 2021", entryTypeInfo, expectedError);

			shipment.JS_E_DEP = new DateTime(2022, 1, 1);
			validationAction();
			AssertHasError("Expected to have an error as 'EUB' is not a valid Entry Type for movements from DEHAM to GB (except Northern Ireland) when shipment ETD is on or after 2021", entryTypeInfo, expectedError);

			consol.JK_RL_NKLoadPort = "DEHAM";
			shipment.JS_RL_NKOrigin = "DEHAM";
			consol.JK_RL_NKDischargePort = "AUSYD";
			validationAction();
			AssertHasErrors("Expected error for 'EUB' as destination / discharge port is not in EU", entryTypeInfo);

			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "GBLON";
			validationAction();
			AssertHasError("Expected to have an error as 'EUB' is not a valid Entry Type for movements from DEHAM to GB (except Northern Ireland)", entryTypeInfo, expectedError);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			validationAction();
			AssertNoErrors("Expected no error for 'EUB' as shipment and consol Transport Mode is not SEA", entryTypeInfo);

			consol.JK_RL_NKLoadPort = "DKVAL";
			shipment.JS_RL_NKOrigin = "DKVAL";
			AssertEquals("Precondition: Shipment is not valid for Dakosy", false, ShipmentPortMessagingManager.IsValidShipmentForDakosyPortMessaging(shipment));
		}

		public static void AssertEntryTypeForConsignorValidation(ZPropertyInfoString entryTypeInfo, ForwardingShipment shipment, Action validationAction)
		{
			var consol = shipment.Consols[0];
			var expectedError = "A United Kingdom EORI may no longer be used for export via Hamburg.";

			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = new DateTime(2021, 1, 1);
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var consignor = consol.Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			var cusCode = consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "4941561651");
			cusCode.OK_RN_NKCodeCountry = "GB";

			entryTypeInfo.Value = EntryTypeList.Codes.EmergencyConcept;
			AssertHasError("Expected to have an error as 'AUS' is not a valid Entry Type when consignor EORI or TRN is issued by GB (except for XI)", entryTypeInfo, expectedError);

			entryTypeInfo.Value = EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities;
			AssertHasError("Expected to have an error as 'AEM' is not a valid Entry Type when consignor EORI or TRN is issued by GB (except for XI)", entryTypeInfo, expectedError);

			entryTypeInfo.Value = EntryTypeList.Codes.OtherExemptions;
			AssertNoErrors("Expected no error as validation only applies to Entry Type 'AUS' and 'AEM'", entryTypeInfo);

			cusCode.OK_CustomsRegNo = "XI123456789000";
			validationAction();
			AssertNoErrors("Expected no error as consignor EORI or TRN starts with XI", entryTypeInfo);

			cusCode.OK_CustomsRegNo = "4941561651";
			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = new DateTime(2023, 1, 1);

			entryTypeInfo.Value = EntryTypeList.Codes.EmergencyConcept;
			AssertHasError("Expected to have an error as 'AUS' is not a valid Entry Type when consignor EORI or TRN is issued by GB (except for XI)", entryTypeInfo, expectedError);

			entryTypeInfo.Value = EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities;
			AssertHasError("Expected to have an error as 'AEM' is not a valid Entry Type when consignor EORI or TRN is issued by GB (except for XI)", entryTypeInfo, expectedError);

			cusCode.OK_RN_NKCodeCountry = "DE";
			validationAction();
			AssertNoErrors("Expected no error as consignor EORI or TRN is issued by country code other than GB", entryTypeInfo);
		}

		public static void AssertMovementReferenceNumberValidation(ZPropertyInfoString entryTypeInfo, ZPropertyInfoString mrnInfo, ZPropertyInfoString annex30ATypeInfo)
		{
			mrnInfo.Value = "blah blah";
			entryTypeInfo.Value = EntryTypeList.Codes.AESExportDeclaration;
			mrnInfo.Value = ZString.Empty;
			AssertHasErrors("Expected error as MRN is mandatory for this entry type", mrnInfo);

			mrnInfo.Value = "219307502";
			AssertNoErrors("Expected no error as MRN has been entered", mrnInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.Message;
			mrnInfo.Value = ZString.Empty;
			AssertNoErrors("Expected no error as MRN is not required for this entry type without a valid Annex 30 type", mrnInfo);

			mrnInfo.Value = "219307502";
			annex30ATypeInfo.Value = Annex30ATypeList.Codes.AlreadyCompleted;
			mrnInfo.Value = ZString.Empty;
			AssertHasErrors("Expected error as MRN is required for this entry type when parent has valid Annex 30 type", mrnInfo);

			mrnInfo.Value = "10LTKR1000IS0199F0";
			AssertNoErrors(mrnInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.OtherExemptions;
			mrnInfo.Value = ZString.Empty;
			AssertNoErrors("Expected no error as MRN is not required for this entry type", mrnInfo);
		}

		public static void AssertMovementReferenceNumberFormatValidation(ZPropertyInfoString mrnInfo)
		{
			ZString invalidCheckDigitError = "MRN does not have a valid check (last) digit. The check digit should be ";
			ZString invalidCountryError = "Please enter a valid country/region code.";
			ZString invalidMRNFormatError = @"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit";

			mrnInfo.Value = "ENFORCEVALIDATION";
			mrnInfo.Value = "";

			AssertNoErrors("Pre-condition: expected mrn to have no errors", mrnInfo);
			AssertNoMessageErrors("Pre-condition: expected mrn to have no message errors", mrnInfo);

			#region General Format of the MRN

			mrnInfo.Value = "219307502";
			AssertHasMessageError("Expected a message error as MRN does not contain both letters and numbers", mrnInfo, invalidMRNFormatError);

			mrnInfo.Value = "DEBLAHBLAH";
			AssertHasMessageError("Expected a message error as MRN does not contain both letters and numbers", mrnInfo, invalidMRNFormatError);

			mrnInfo.Value = "!-0*&^)w*($&";
			AssertHasMessageError("Expected a message error as MRN does not contain only letters and numbers", mrnInfo, invalidMRNFormatError);

			mrnInfo.Value = "15DE-219307502";
			AssertHasMessageError("Expected a message error as MRN does not contain only letters and numbers", mrnInfo, invalidMRNFormatError);

			mrnInfo.Value = "DE219307502";
			AssertHasMessageError("Expected a message error as MRN does not begin with two numbers for the year", mrnInfo, invalidMRNFormatError);

			mrnInfo.Value = "15DE";
			AssertHasMessageError("Expected a message error as MRN is too short", mrnInfo, invalidMRNFormatError);

			mrnInfo.Value = "13DE485101472466XX";
			AssertHasMessageError("Expected a message error as MRN has the wrong check digit", mrnInfo, invalidMRNFormatError);

			mrnInfo.Value = "15dE33344blahblah2";
			AssertNoMessageError("Expected a message error as MRN has lower case letters", mrnInfo, invalidCountryError);

			#endregion

			#region Country Code Is Valid ISO COde

			mrnInfo.Value = "15XX333444455555E0";
			AssertHasMessageError("Expected a message error as MRN does not have a valid country/region code", mrnInfo, invalidCountryError);

			mrnInfo.Value = "15JI333444455555E0";
			AssertHasMessageError("Expected no message error as MRN has a valid country/region code", mrnInfo, invalidCountryError);

			mrnInfo.Value = "15DE333444455555E2";
			AssertNoMessageError("Expected no message error as MRN has a valid country/region code", mrnInfo, invalidCountryError);

			#endregion

			#region Check Digit for Transit (MRN for NCTS)

			mrnInfo.Value = "10LTKA100011789E50";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10LU704000103AF901";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10LV00020610B97B83";
			AssertHasMessageError("Expected a message error as MRN has the wrong check digit", mrnInfo, invalidCheckDigitError + "4");

			mrnInfo.Value = "10LV00020610B97B84";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10MT0001181004F449";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10NL5631291B1A3361";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10NO01011A10583AE9";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10PL301010116885B4";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10PT0001151011BA25";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10ROBU140000026445";
			AssertHasMessageError("Expected message error as MRN has the wrong check digit", mrnInfo, invalidCheckDigitError + "6");

			mrnInfo.Value = "10ROBU140000026446";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10SE00005010C124F5";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10SI00102615009349";
			AssertHasMessageError("Expected message error as MRN has the wrong check digit", mrnInfo, invalidCheckDigitError + "6");

			mrnInfo.Value = "10SI00102615009346";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10SK5161TR00000391";   //this example was wrong in the dakosy document
			AssertHasMessageError("Expected message error as MRN has the wrong check digit", mrnInfo, invalidCheckDigitError + "4");

			mrnInfo.Value = "10SK5161TR00000394";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10SMQ02010004408T1";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "13TR34130000025717";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			mrnInfo.Value = "10GB00002910B75BE5";
			AssertNoMessageErrors("Expected no message error as MRN is valid", mrnInfo);

			#endregion
		}

		public static void AssertExemptionReasonValidation(ZPropertyInfoString entryTypeInfo, ZPropertyInfoString exemptionReasonInfo, ForwardingShipment shipment = null)
		{
			entryTypeInfo.Value = EntryTypeList.Codes.OtherExemptions;
			exemptionReasonInfo.Value = "X";
			AssertHasErrors("Expected to have an error as 'X' is not a valid Exemption code", exemptionReasonInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.Message;
			exemptionReasonInfo.Value = "X";
			AssertHasErrors("Expected to have an error as 'X' is not a valid Exemption code", exemptionReasonInfo);

			exemptionReasonInfo.Value = new ExemptionReasonList(EntryTypeList.Codes.Message)[0].Code;
			AssertNoErrors("Expected no errors if Exemption code is from the Exemption code list", exemptionReasonInfo);

			exemptionReasonInfo.Value = ZString.Empty;
			AssertHasErrors("Expected to have an error as Exemption code cannot be blank if Customs type is MIT", exemptionReasonInfo);

			exemptionReasonInfo.Value = new ExemptionReasonList(EntryTypeList.Codes.Message)[0].Code;
			AssertNoErrors("Expected no errors if Exemption code is from the Exemption code list", exemptionReasonInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.ConsolidatedContainer;
			AssertEquals("Expected an empty Exemption code if Customs Type is not MIT or SBF", ZString.Empty, exemptionReasonInfo.Value);
			AssertNoErrors("Expected no errors with an empty Exemption code if Customs Type is not MIT or SBF", exemptionReasonInfo);

			if (shipment != null)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
				{
					var exchangeRate = shipment.Factory.New<RefExchangeRate>();
					exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
					exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
					exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
					exchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
					exchangeRate.RE_RX_NKExCurrency = "USD";
					exchangeRate.RE_SellRate = 0.5;

					shipment.JS_GoodsValue = 750;
					shipment.JS_RX_NKGoodsValueCurr = "USD";

					entryTypeInfo.Value = EntryTypeList.Codes.OtherExemptions;
					exemptionReasonInfo.Value = ExemptionReasonList.Codes.OtherExemptions;
					AssertHasWarnings("750 USD should be 1500 EUR with configured exchange rates", exemptionReasonInfo);

					exemptionReasonInfo.Value = ExemptionReasonList.Codes.ValueOver1000Euro;
					AssertNoWarnings("750 USD should be 1500 EUR with configured exchange rates", exemptionReasonInfo);

					shipment.JS_GoodsValue = 300;

					exemptionReasonInfo.Value = ExemptionReasonList.Codes.OtherExemptions;
					AssertNoWarnings("300 USD should be 600 EUR with configured exchange rates", exemptionReasonInfo);

					exemptionReasonInfo.Value = ExemptionReasonList.Codes.ValueOver1000Euro;
					AssertHasWarnings("300 USD should be 600 EUR with configured exchange rates", exemptionReasonInfo);

					exemptionReasonInfo.Value = ZString.Empty;
					AssertHasErrors("Mandatory field", exemptionReasonInfo);
				}
			}
		}

		public static void AssertATBNumberValidation(ZPropertyInfoString entryTypeInfo, ZPropertyInfoString atbNumberInfo, ForwardingShipment parentShipment)
		{
			entryTypeInfo.Value = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
			atbNumberInfo.Value = "abc";
			AssertHasError(atbNumberInfo, "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.)");

			entryTypeInfo.Value = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
			atbNumberInfo.Value = "ATB123456789123451111";
			AssertHasError(atbNumberInfo, "ATLAS Registration must be issued by customs office '4851' (digit 18-21)");

			entryTypeInfo.Value = EntryTypeList.Codes.Message;
			atbNumberInfo.Value = "ATB123456789123452222";
			AssertHasError(atbNumberInfo, "ATLAS Registration must be issued by customs office '4851' (digit 18-21)");

			atbNumberInfo.Value = "ATB45678912345678912*";
			AssertHasError(atbNumberInfo, "ATB Number should contain letters and numbers only.");

			atbNumberInfo.Value = ZString.Empty;
			AssertHasError(atbNumberInfo, "ATB (former customs registration number) is mandatory in case of declaration type MIT. Enter the ATB number of the summary declaration (goods Sum A) here.");

			atbNumberInfo.Value = "ATB123456789123454851";
			AssertNoErrors(atbNumberInfo);

			atbNumberInfo.Value = "1234abcd1234567891";
			AssertHasError(atbNumberInfo, "MRN registration must be issued by customs office '4851' (digit 5-8)");

			atbNumberInfo.Value = "11DE48511111111111";
			AssertHasError(atbNumberInfo, "MRN does not have a valid last digit. The last digit should be 5");

			atbNumberInfo.Value = "24DE4851I00001H7U4";
			AssertNoErrors(atbNumberInfo);

			var consol = parentShipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			parentShipment.JS_RL_NKOrigin = "USLAX";
			Assert("Prerequisite", !parentShipment.Origin.IsInEU);

			entryTypeInfo.Value = EntryTypeList.Codes.EUPortOfDestination;
			atbNumberInfo.Value = ZString.Empty;
			AssertHasErrors("ATB is mandatory for EUB entry type when shipment origin not in EU and consol load is DEHAM", atbNumberInfo);

			parentShipment.JS_RL_NKOrigin = "NLAMS";
			Assert("Prerequisite", parentShipment.Origin.IsInEU);

			atbNumberInfo.Value = ZString.Empty;
			AssertNoErrors(atbNumberInfo);
		}

		public static void AssertAnnex30ATypeValidation(ZPropertyInfoString annex30ATypeInfo)
		{
			annex30ATypeInfo.Value = Annex30ATypeList.Codes.AlreadyCompleted;
			AssertNoErrors(annex30ATypeInfo);

			annex30ATypeInfo.Value = "*";
			AssertHasErrors(annex30ATypeInfo);

			annex30ATypeInfo.Value = "";
			AssertNoErrors(annex30ATypeInfo);

			annex30ATypeInfo.Value = Annex30ATypeList.Codes.AlreadyCompleted;
			AssertNoErrors(annex30ATypeInfo);
		}

		public static void AssertCustomsReleaseDateValidation(ZPropertyInfoString entryTypeInfo, ZPropertyInfoDateTime customsReleaseDateInfo, Action customsReleaseDateValidation, ForwardingShipment parentShipment)
		{
			var now = ZDateTime.Now;

			entryTypeInfo.Value = EntryTypeList.Codes.ExitSummaryDeclaration;
			customsReleaseDateInfo.Value = ZDateTime.Empty;
			AssertHasErrors("Customs release date field is mandatory when entry type DUX is selected.", customsReleaseDateInfo);

			customsReleaseDateInfo.Value = now;
			AssertNoErrors("Custom release date field has valid data when entry type DUX is selected", customsReleaseDateInfo);

			customsReleaseDateInfo.Value = now.AddHours(2).AddDays(4);
			AssertNoErrors("Custom release date field has valid data when entry type DUX is selected", customsReleaseDateInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.ConsolidatedContainer;
			customsReleaseDateInfo.Value = ZDateTime.Empty;
			AssertNoErrors("Custom release date field is not mandatory when entry type is not DUX", customsReleaseDateInfo);

			customsReleaseDateInfo.Value = now;
			AssertNoErrors("Custom release date field is not mandatory when entry type is not DUX", customsReleaseDateInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.ExitSummaryDeclaration;
			customsReleaseDateInfo.Value = ZDateTime.Empty;
			customsReleaseDateInfo.BizObj.Factory.Save();
			customsReleaseDateValidation();

			AssertEquals("prerequisite", true, customsReleaseDateInfo.BizObj.IsInDatabase);
			AssertEquals("prerequisite", false, customsReleaseDateInfo.BizObj.HasChanges);
			AssertNoErrors("Portmessaging record has been saved in database without a requirement to enter customs release date (a previously saved portmessaging record)", customsReleaseDateInfo);

			customsReleaseDateInfo.BizObj.HasChanges = true;
			customsReleaseDateValidation();

			AssertEquals("prerequisite", true, customsReleaseDateInfo.BizObj.IsInDatabase);
			AssertEquals("prerequisite", true, customsReleaseDateInfo.BizObj.HasChanges);
			AssertHasErrors("Previously saved portmessaging record has been changed and a valid customs release date is required", customsReleaseDateInfo);

			customsReleaseDateInfo.Value = now;
			customsReleaseDateValidation();

			AssertNoErrors("Previously saved portmessaging record has been changed and a valid customs release date has been entered", customsReleaseDateInfo);
		}

		public static void AssertExemptionReasonObsoleteCodesValidation(ZPropertyInfoString exemptionReasonInfo, Action exemptionReasonValidation, ForwardingShipment parentShipment)
		{
			AssertObsoleteCodesImports(exemptionReasonInfo, exemptionReasonValidation, parentShipment);
			AssertObsoleteCodesExports(exemptionReasonInfo, exemptionReasonValidation, parentShipment);
			AssertObsoleteCodesNonApplicableLegs(exemptionReasonInfo, exemptionReasonValidation, parentShipment);
		}

		static void AssertObsoleteCodesImports(ZPropertyInfoString exemptionReasonInfo, Action exemptionReasonValidation, ForwardingShipment parentShipment)
		{
			parentShipment.Consols.RemoveAll();
			ForwardingConsol consol = parentShipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.Transports[0].JW_RL_NKLoadPort = "AUMEL";
			consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			consol.Transports[0].JW_ETA = new ZDateTime(2016, 12, 3);
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Sea;

			consol.Transports.AddNew();

			consol.Transports[1].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[1].JW_RL_NKDiscPort = "DEHAM";
			consol.Transports[1].JW_ETA = new ZDateTime(2016, 12, 31);
			consol.Transports[1].JW_TransportMode = Constants.TransportModes.Sea;

			exemptionReasonInfo.Value = "6";
			consol.Factory.Save();
			exemptionReasonValidation();
			AssertHasMessageError("Should display a message for dates before 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");

			exemptionReasonInfo.Value = "4";
			exemptionReasonValidation();
			AssertHasError("Should display a message for dates before 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");

			consol.Transports[1].JW_ATA = new ZDateTime(2017, 1, 1);

			exemptionReasonInfo.Value = "4";
			exemptionReasonValidation();
			AssertHasError("Should display an error for dates after 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");

			exemptionReasonInfo.Value = "6";
			exemptionReasonValidation();
			AssertHasError("Should display an error for dates after 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");
		}

		static void AssertObsoleteCodesExports(ZPropertyInfoString exemptionReasonInfo, Action exemptionReasonValidation, ForwardingShipment parentShipment)
		{
			parentShipment.Consols.RemoveAll();
			ForwardingConsol consol = parentShipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.Transports[0].JW_RL_NKLoadPort = "DEHAM";
			consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			consol.Transports[0].JW_ETD = new ZDateTime(2016, 12, 31);
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Sea;

			consol.Transports.AddNew();

			consol.Transports[1].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[1].JW_RL_NKDiscPort = "AUMEL";
			consol.Transports[1].JW_ETD = new ZDateTime(2017, 1, 5);
			consol.Transports[1].JW_TransportMode = Constants.TransportModes.Sea;

			exemptionReasonInfo.Value = "4";
			consol.Factory.Save();
			exemptionReasonValidation();
			AssertHasMessageError("Should display a message for dates before 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");

			exemptionReasonInfo.Value = "6";
			exemptionReasonValidation();
			AssertHasError("Should display a message for dates before 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");

			consol.Transports[0].JW_ATD = new ZDateTime(2017, 1, 1);

			exemptionReasonInfo.Value = "4";
			exemptionReasonValidation();
			AssertHasError("Should display an error for dates after 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");

			exemptionReasonInfo.Value = "6";
			exemptionReasonValidation();
			AssertHasError("Should display an error for dates after 1/1/17", exemptionReasonInfo, "This Exemption Reason is now obsolete.");
		}

		static void AssertObsoleteCodesNonApplicableLegs(ZPropertyInfoString exemptionReasonInfo, Action exemptionReasonValidation, ForwardingShipment parentShipment)
		{
			parentShipment.Consols.RemoveAll();
			ForwardingConsol consol = parentShipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.Transports[0].JW_RL_NKLoadPort = "AUMEL";
			consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			consol.Transports[0].JW_ETA = new ZDateTime(2016, 12, 3);
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Sea;

			consol.Transports.AddNew();

			consol.Transports[1].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[1].JW_RL_NKDiscPort = "DEHAM";
			consol.Transports[1].JW_ETA = new ZDateTime(2017, 1, 5);
			consol.Transports[1].JW_TransportMode = Constants.TransportModes.Air;

			exemptionReasonInfo.Value = "6";
			consol.Factory.Save();
			exemptionReasonValidation();
			AssertHasMessageError("Should ignore non-SEA legs", exemptionReasonInfo, "This Exemption Reason is now obsolete.");

			consol.Transports[1].JW_TransportMode = Constants.TransportModes.Sea;

			exemptionReasonValidation();
			AssertHasError("Should recognise all SEA legs", exemptionReasonInfo, "This Exemption Reason is now obsolete.");
		}

		public static void AssertExportDeclarationReferenceValidation(ZPropertyInfoString entryTypeInfo, ZPropertyInfoString exportDeclarationReferenceInfo)
		{
			entryTypeInfo.Value = EntryTypeList.Codes.AESExportDeclaration;
			exportDeclarationReferenceInfo.Value = ZString.Empty;
			AssertNoErrors(exportDeclarationReferenceInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.EmergencyConcept;
			AssertHasErrors(exportDeclarationReferenceInfo);

			exportDeclarationReferenceInfo.Value = "Blah blah";
			AssertNoErrors(exportDeclarationReferenceInfo);
		}

		public static void AssertLocalReferenceNumberValidation(ZPropertyInfoString entryTypeInfo, ZPropertyInfoString lrnInfo)
		{
			entryTypeInfo.Value = EntryTypeList.Codes.AE1ExportDeclaration;
			lrnInfo.Value = ZString.Empty;
			AssertHasError("Expected error as LRN is mandatory for this entry type", lrnInfo, "Entry Type AE1 requires LRN Local Reference Number to be entered.");

			lrnInfo.Value = "123456789012345678901234567890";
			AssertNoErrors("Expected no error as MRN has been entered", lrnInfo);

			entryTypeInfo.Value = EntryTypeList.Codes.Message;
			lrnInfo.Value = ZString.Empty;
			AssertNoError("Expected no error as LRN is not required for this entry type", lrnInfo, "Entry Type AE1 requires LRN Local Reference Number to be entered.");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				entryTypeInfo.Value = EntryTypeList.Codes.AE1ExportDeclaration;
				lrnInfo.Value = "123456789012345678901234567890";
				AssertHasError("Expected error as LRN has a max length for Germany for this entry type", lrnInfo, "Local Reference Number has a maximum limit of 22 characters.");
			}
		}

		public void TestGetValueInEuroNullChargeCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.OtherExemptions;

			const string message = "No null exception when trying to convert an empty charge code/value to Euro converted value.";

			AssertNoExceptionThrown(message, () => shipmentPortMessaging.Validation.ValidateJSM_ExemptionReason());
		}
	}
}
