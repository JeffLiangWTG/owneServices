using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	public sealed class AssertionHelper : TestCaseWithFactory
	{
		public static void AssertMessageErrorIfEmpty(ZPropertyInfo info, string expectedErrorMessage)
		{
			var originalValue = info.Value;
			info.Value = new ZString("something");
			AssertNoMessageError(info, expectedErrorMessage);

			info.Value = ZString.Empty;
			AssertHasMessageError(info, expectedErrorMessage);

			info.Value = originalValue;
		}

		public static void AssertMaximumLengthValidation(ZPropertyInfo info, int maxLength, string expectedErrorMessage)
		{
			if (maxLength <= 0)
			{
				Assert(true);
				return;
			}

			var originalValue = info.Value;
			info.Value = ZDataType.ObjectToZType(info.Value.GetType(), ZString.Replicate('1', maxLength));
			AssertNoMessageError(info, expectedErrorMessage);

			info.Value = ZDataType.ObjectToZType(info.Value.GetType(), ZString.Replicate('1', maxLength + 1));
			AssertHasMessageError(info, expectedErrorMessage);

			info.Value = originalValue;
		}

		public static void AssertUNLOCOIATAValidation(Unloco unloco, string portName)
		{
			if (unloco == null)
			{
				return;
			}

			unloco.IATACode = string.Empty;
			AssertMessageErrorIfEmpty(unloco.IATACodeInfo, $"{portName} IATA code is mandatory");

			unloco.IATACode = "AA";
			AssertHasMessageError(unloco.IATACodeInfo, $"{portName} IATA code must consist of 3 letters.");

			unloco.IATACode = "AA1";
			AssertHasMessageError(unloco.IATACodeInfo, $"{portName} IATA code must consist of 3 letters.");

			unloco.IATACode = "AAB";
			AssertNoMessageError(unloco.IATACodeInfo, $"{portName} IATA code must consist of 3 letters.");
		}

		public static void AssertAddressData(OrgHeader orgHeader, IAddress shipperData, bool includeContactDetail = true)
		{
			var address = orgHeader.MainAddress;

			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", orgHeader.OH_FullName, shipperData.CompanyName);
				AssertEquals("AddressLine1", address.Address1, shipperData.AddressLine1);
				AssertEquals("AddressLine2", address.Address2, shipperData.AddressLine2);
				AssertEquals("AdditionalAddressInformation", address.UnrestrictedAdditionalAddressInformation, shipperData.AdditionalAddressInformation);
				AssertEquals("City", address.City, shipperData.City);
				AssertEquals("State", address.StateCode, shipperData.State);
				AssertEquals("Postcode", address.Postcode, shipperData.Postcode);
				AssertEquals("Country.Code", address.Country?.Code, shipperData.Country?.Code);
				AssertEquals("Unloco.Code", address.Header?.ClosestPort?.Code, shipperData.Unloco.Code);
				if (includeContactDetail)
				{
					AssertEquals("Fax", address.OA_Fax, shipperData.Fax);
					AssertEquals("Phone", address.OA_Phone, shipperData.Phone);
					AssertEquals("Email", address.OA_Email, shipperData.Email);
				}
			});
		}

		public static void AssertAddressData(JobDocAddress docAddress, IAddress shipperData)
		{
			AssertNotNull(docAddress.Organisation);
			var address = docAddress.Organisation.MainAddress;

			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", docAddress.Organisation.OH_FullName, shipperData.CompanyName);
				AssertEquals("AddressLine1", address.Address1, shipperData.AddressLine1);
				AssertEquals("AddressLine2", address.Address2, shipperData.AddressLine2);
				AssertEquals("AdditionalAddressInformation", address.UnrestrictedAdditionalAddressInformation, shipperData.AdditionalAddressInformation);
				AssertEquals("City", address.City, shipperData.City);
				AssertEquals("State", address.StateCode, shipperData.State);
				AssertEquals("Postcode", address.Postcode, shipperData.Postcode);
				AssertEquals("Country.Code", address.Country?.Code ?? string.Empty, shipperData.Country?.Code);
				AssertEquals("Unloco.Code", docAddress.Organisation?.ClosestPort?.Code ?? string.Empty, shipperData.Unloco.Code);
				AssertEquals("Fax", docAddress.E2_Fax, shipperData.Fax);
				AssertEquals("Phone", docAddress.E2_Phone, shipperData.Phone);
				AssertEquals("Email", docAddress.E2_Email, shipperData.Email);
				AssertEquals("Contact", docAddress.E2_Contact, shipperData.Contact);
			});
		}

		public static void AssertAddressData(OrgAddress orgAddress, IAddress addressDataObject)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", orgAddress.Header.OH_FullName, addressDataObject.CompanyName);
				AssertEquals("AddressLine1", orgAddress.Address1, addressDataObject.AddressLine1);
				AssertEquals("AddressLine2", orgAddress.Address2, addressDataObject.AddressLine2);
				AssertEquals("AdditionalAddressInformation", orgAddress.UnrestrictedAdditionalAddressInformation, addressDataObject.AdditionalAddressInformation);
				AssertEquals("City", orgAddress.City, addressDataObject.City);
				AssertEquals("State", orgAddress.StateCode, addressDataObject.State);
				AssertEquals("Postcode", orgAddress.Postcode, addressDataObject.Postcode);
				AssertEquals("Country.Code", orgAddress.OA_RN_NKCountryCode, addressDataObject.Country.Code);
			});
		}

		internal static void AssertPopulateAPPlusCodes(Func<Cresa> getCRESAMessage, OrgAddress address, string sPropertyType, string sPropertyName, string ci5PropertyName, string formattedProviderIDPropertyInfoName = "")
		{
			var isAddProviderIDValidation = !string.IsNullOrEmpty(formattedProviderIDPropertyInfoName);
			var sonCode1 = address.Header.CustomsCodes.AddNew();
			sonCode1.OK_CodeType = sPropertyType;
			sonCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			sonCode1.OK_CustomsRegNo = "S001";

			var ci5Code1 = address.Header.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code1.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			ci5Code1.OK_CustomsRegNo = "C001";

			var cresa = getCRESAMessage();

			AssertEquals($"{sPropertyName} should be from org", "S001", ((RegistrationNumber)cresa[sPropertyName]).Value);
			if (!ci5PropertyName.IsNullOrEmpty())
			{
				AssertEquals($"{ci5PropertyName} should be from org", "C001", ((RegistrationNumber)cresa[ci5PropertyName]).Value);
			}

			if (isAddProviderIDValidation)
			{
				AssertNoMessageErrors((ZPropertyInfo)cresa[formattedProviderIDPropertyInfoName]);
			}

			var sonCode2 = address.CustomsCodes.AddNew();
			sonCode2.OK_CodeType = sPropertyType;
			sonCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			sonCode2.OK_CustomsRegNo = "S002";

			var ci5Code2 = address.CustomsCodes.AddNew();
			ci5Code2.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code2.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			ci5Code2.OK_CustomsRegNo = "C002";

			cresa = getCRESAMessage();

			AssertEquals($"{sPropertyName} should be from address", "S002", ((RegistrationNumber)cresa[sPropertyName]).Value);
			if (!ci5PropertyName.IsNullOrEmpty())
			{
				AssertEquals($"{ci5PropertyName} should be from address", "C002", ((RegistrationNumber)cresa[ci5PropertyName]).Value);
			}
			if (isAddProviderIDValidation)
			{
				AssertNoMessageErrors((ZPropertyInfo)cresa[formattedProviderIDPropertyInfoName]);
			}

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			cresa = getCRESAMessage();

			AssertEquals(string.Empty, ((RegistrationNumber)cresa[sPropertyName]).Value);
			if (!ci5PropertyName.IsNullOrEmpty())
			{
				AssertEquals(string.Empty, ((RegistrationNumber)cresa[ci5PropertyName]).Value);
			}

			if (isAddProviderIDValidation)
			{
				var partyName = "";
				var configPath = "";
				var isOrganization = false;
				if (sPropertyName.StartsWith("SendingParty"))
				{
					partyName = "Sending Party";
					configPath = "Shipment > Pickup > CFS";
					isOrganization = true;
				}
				if (sPropertyName.StartsWith("Agent"))
				{
					partyName = "Agent";
					configPath = "Shipment > Pickup > Pickup Agent";
				}
				if (sPropertyName.StartsWith("SendingForwarder"))
				{
					partyName = "Forwarder";
					configPath = "Org. Proxy";
				}

				AssertRequireAPPlusIDValidation((ZPropertyInfo)cresa[formattedProviderIDPropertyInfoName], cresa.PCS, sPropertyType, partyName, partyName, configPath, isOrganization);
			}
		}

		internal static void AssertRequireAPPlusIDValidation(ZPropertyInfo formattedProviderIDPropertyInfo, string pcs, string sPropertyType, string partyType = "", string partyCodeType = "", string configPath = "", bool isOrganization = false, string extraConfigPath = "")
		{
			switch (pcs)
			{
				case FrenchPortsConstants.PCS.MGI:
					if (!string.IsNullOrEmpty(extraConfigPath))
					{
						extraConfigPath = $"\r\nOR CI5 code in {extraConfigPath} > Config > Registration Numbers / Codes[Type = CI5],";
					}

					if (isOrganization)
					{
						AssertHasMessageError(formattedProviderIDPropertyInfo, $"{partyType} Port Community System (PCS) code missing from Organization {configPath} > Config > Registration Numbers/Codes [Type=CI5].");
					}
					else
					{
						AssertHasMessageError(formattedProviderIDPropertyInfo, $"{partyType} Port Community System (PCS) code is required.\r\nProvide {partyCodeType} Code of Operational Port in Registry > Freight > Port Messaging > France > Port Community System Code of Forwarder and Agent,{extraConfigPath}\r\nOR CI5 code in {configPath} > Config > Registration Numbers / Codes[Type = CI5].");
					}
					break;
				case FrenchPortsConstants.PCS.Soget:
					if (!string.IsNullOrEmpty(extraConfigPath))
					{
						extraConfigPath = $"\r\nOR {sPropertyType} code in {extraConfigPath} > Config > Registration Numbers / Codes[Type = {sPropertyType}],";
					}

					if (isOrganization)
					{
						AssertHasMessageError(formattedProviderIDPropertyInfo, $"{partyType} Port Community System (PCS) code missing from Organization {configPath} > Config > Registration Numbers/Codes [Type={sPropertyType}].");
					}
					else
					{
						AssertHasMessageError(formattedProviderIDPropertyInfo, $"{partyType} Port Community System (PCS) code is required.\r\nProvide {partyCodeType} Code of Operational Port in Registry > Freight > Port Messaging > France > Port Community System Code of Forwarder and Agent,{extraConfigPath}\r\nOR {sPropertyType} code in {configPath} > Config > Registration Numbers / Codes[Type = {sPropertyType}].");
					}
					break;
				default:
					AssertHasMessageError(formattedProviderIDPropertyInfo, $"{partyType} Port Community System (PCS) code cannot be defaulted due to error on PCS field. Verify error on PCS field.");
					break;
			}
		}

		public static void AssertCurrentUserAddressData(IAddress shipperData)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "EDI CUSTOMS BROKERS", shipperData.CompanyName);
				AssertEquals("AddressLine1", "10 HUTCHESON STREET", shipperData.AddressLine1);
				AssertEquals("AddressLine2", "ALBION  QLD", shipperData.AddressLine2);
				AssertEquals("AdditionalAddressInformation", "", shipperData.AdditionalAddressInformation);
				AssertEquals("City", "", shipperData.City);
				AssertEquals("State", "", shipperData.State);
				AssertEquals("Postcode", "4010", shipperData.Postcode);
				AssertEquals("Country.Code", "AU", shipperData.Country?.Code);
				AssertEquals("Unloco.Code", "AUBNE", shipperData.Unloco.Code);
				AssertEquals("Fax", "", shipperData.Fax);
				AssertEquals("Phone", "", shipperData.Phone);
				AssertEquals("Email", "", shipperData.Email);
				AssertEquals("Contact", "CargoWise Support", shipperData.Contact);
			});
		}

		public static void AssertAddressData(IAddress data1, IAddress data2)
		{
			AssertNotNull(data1);
			AssertNotNull(data2);

			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", data1.CompanyName, data2.CompanyName);
				AssertEquals("AddressLine1", data1.AddressLine1, data2.AddressLine1);
				AssertEquals("AddressLine2", data1.AddressLine2, data2.AddressLine2);
				AssertEquals("AdditionalAddressInformation", data1.AdditionalAddressInformation, data2.AdditionalAddressInformation);
				AssertEquals("City", data1.City, data2.City);
				AssertEquals("State", data1.State, data2.State);
				AssertEquals("Postcode", data1.Postcode, data2.Postcode);
				AssertEquals("Phone", data1.Phone, data2.Phone);
				AssertEquals("Fax", data1.Fax, data2.Fax);
				AssertEquals("Email", data1.Email, data2.Email);
				AssertEquals("Contact", data1.Contact, data2.Contact);
				AssertEquals("Contact", data1.TaxNumber, data2.TaxNumber);
				AssertEquals("Country.Code", data1.Country?.Code, data2.Country?.Code);
				AssertEquals("Unloco.Code", data1.Unloco.Code, data2.Unloco.Code);

				AssertEquals("RegistrationNumbers", data1.RegistrationNumbers.Count, data2.RegistrationNumbers.Count);
				foreach (var regNo in data1.RegistrationNumbers)
				{
					var regNoMatch = data2.RegistrationNumbers.FirstOrDefault(r => r.Type.Code == regNo.Type.Code
						&& r.CountryOfIssue.Code == regNo.CountryOfIssue.Code
						&& r.Value == regNo.Value);
					AssertNotNull("RegistrationNumbers element", regNoMatch);
				}
			});
		}

		public static void AssertContainerData(ForwardingContainer container, IContainer containerData)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number", container.JC_ContainerNum, containerData.Number);
				AssertEquals("ContainerCount", container.JC_ContainerCount, containerData.ContainerCount);
				AssertEquals("Type.Code", container.JC_F3_NKPackType, containerData.Type.Code);

				AssertEquals("Seal", container.JC_SealNum, containerData.Seal);
				AssertEquals("SealPartyType.Code", container.JC_SealParty, containerData.SealPartyType.Code);

				AssertEquals("SecondSeal", container.JC_AdditionalSealParty, containerData.SecondSeal);
				AssertEquals("SecondSealPartyType.Code", container.JC_AdditionalSealParty, containerData.SecondSealPartyType.Code);

				AssertEquals("ThirdSeal", container.JC_Additional2SealNum, containerData.ThirdSeal);
				AssertEquals("ThirdSealPartyType.Code", container.JC_Additional2SealParty, containerData.ThirdSealPartyType.Code);

				AssertEquals("HasControlledAtmosphere", container.JC_IsControlledAtmosphere, containerData.HasControlledAtmosphere);
				AssertEquals("TemperatureRecorderSerialNumber", container.JC_TempRecorderSerialNo, containerData.TemperatureRecorderSerialNumber);

				AssertEquals("SetTemperature.Value", container.JC_SetPointTemp, containerData.SetTemperature.Value);
				AssertEquals("SetTemperature.Unit.Code", container.JC_SetPointTempUnit, containerData.SetTemperature.Unit.Code);

				AssertEquals("Humidity.Value", container.JC_HumidityPercent, (byte)containerData.Humidity.Value);
				AssertEquals("Humidity.Unit.Code", "%", containerData.Humidity.Unit.Code);

				AssertEquals("AirVentFlow.Value", container.JC_AirVentFlow, containerData.AirVentFlow.Value);
				AssertEquals("AirVentFlow.Unit.Code", container.JC_AirVentFlowRateUnit, containerData.AirVentFlow.Unit.Code);
			});
		}

		public static void AssertAsciiCharactersValidation(string portName, Unloco port)
		{
			AssertAsciiCharactersValidation($"{portName}.Code", port.CodeInfo);
			AssertAsciiCharactersValidation($"{portName}.Name", port.NameInfo);
		}

		public static void AssertAsciiCharactersValidation(string propertyName, Vessel vessel)
		{
			AssertAsciiCharactersValidation($"{propertyName}.Name", vessel.NameInfo);
		}

		public static void AssertAsciiCharactersValidation(string transportName, Transport transport)
		{
			AssertAsciiCharactersValidation($"{transportName}.VoyageFlightNumber", transport.VoyageFlightNumberInfo);

			if (transport.Vessel != null)
			{
				AssertAsciiCharactersValidation($"{transportName}.Vessel.Name", transport.Vessel.NameInfo);
			}

			if (transport.Carrier != null)
			{
				AssertAsciiCharactersValidation($"{transportName}.Carrier", transport.Carrier);
			}
		}

		public static void AssertAsciiCharactersValidation(string addressName, Address address)
		{
			AssertAsciiCharactersValidation($"{addressName}.CompanyName", address.CompanyNameInfo);
			AssertAsciiCharactersValidation($"{addressName}.Contact", address.ContactInfo);
			AssertAsciiCharactersValidation($"{addressName}.Address1", address.AddressLine1Info);
			AssertAsciiCharactersValidation($"{addressName}.Address 2", address.AddressLine2Info);
			AssertAsciiCharactersValidation($"{addressName}.City", address.CityInfo);
			AssertAsciiCharactersValidation($"{addressName}.State", address.StateInfo);
			AssertAsciiCharactersValidation($"{addressName}.PostCode", address.PostcodeInfo);
		}

		public static void AssertAsciiCharactersValidation(string propertyName, ZPropertyInfo propertyInfo)
		{
			const string errorMessage = "Most messaging providers do not support non ASCII characters.";

			propertyInfo.Value = (ZString)"天";
			AssertHasMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"இ";
			AssertHasMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"æ";
			AssertNoMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"ß";
			AssertNoMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"1";
			AssertNoMessageError($"{propertyName}", propertyInfo, errorMessage);
		}

		public static void AssertSupportedCharactersValidationForUSCustoms(Address address)
		{
			AssertSupportedCharactersValidationForUSCustoms("Address.CompanyName", address.CompanyNameInfo);
			AssertSupportedCharactersValidationForUSCustoms("Address.Contact", address.ContactInfo);
			AssertSupportedCharactersValidationForUSCustoms("Address.Address1", address.AddressLine1Info);
			AssertSupportedCharactersValidationForUSCustoms("Address.Address2", address.AddressLine2Info);
			AssertSupportedCharactersValidationForUSCustoms("Address.City", address.CityInfo);
			AssertSupportedCharactersValidationForUSCustoms("Address.State", address.StateInfo);
			AssertSupportedCharactersValidationForUSCustoms("Address.PostCode", address.PostcodeInfo);
		}

		public static void AssertSupportedCharactersValidationForUSCustoms(Unloco port)
		{
			AssertSupportedCharactersValidationForUSCustoms("Port.Code", port.CodeInfo);
			AssertSupportedCharactersValidationForUSCustoms("Port.Name", port.NameInfo);
		}

		public static void AssertSupportedCharactersValidationForUSCustoms(string propertyName, ZPropertyInfo propertyInfo)
		{
			const string errorMessage = "This text contains characters not supported by the United States Customs (CBP).\r\n"
				+ "Only characters shown directly on a keyboard with US layout are acceptable for this message, not typed or special characters.";

			propertyInfo.Value = (ZString)"è";
			AssertHasMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"1";
			AssertNoMessageError($"{propertyName}", propertyInfo, errorMessage);
		}

		public static void AssertAddressCompanyNameLength(string party, Address address)
		{
			var warning = "Party name should not exceed 70 characters. Please note that any excess characters might be truncated by the message recipient.";

			address.ValidateAll();
			AssertNoWarning("No warning", address.CompanyNameInfo, warning);

			address.CompanyName = "Alibaba's name is from a story known across the world named 'Alibaba and the Forty Thieves'.";
			AssertHasWarning($"Should has warning for {party}", address.CompanyNameInfo, warning);

			address.CompanyName = "Alibaba";
			AssertNoWarning($"No warning for {party}", address.CompanyNameInfo, warning);
		}

		public static void AssertAddressContactNameValid(string party, Address address)
		{
			var messageError = "Please enter both contact name and at least one communication: phone, email or fax.";
			address.ValidateAll();

			address.Contact = "a";
			AssertHasMessageError($"Should have message error for {party}", address.ContactInfo, messageError);

			address.Contact = "aa";
			AssertHasMessageError($"Should have message error for {party}", address.ContactInfo, messageError);

			address.Contact = "1";
			AssertHasMessageError($"Should have message error for {party}", address.ContactInfo, messageError);

			address.Contact = "111";
			AssertHasMessageError($"Should have message error for {party}", address.ContactInfo, messageError);

			address.Contact = ".";
			AssertHasMessageError($"Should have message error for {party}", address.ContactInfo, messageError);

			address.Contact = "....";
			AssertHasMessageError($"Should have message error for {party}", address.ContactInfo, messageError);

			address.Contact = "Good Contact Name";
			address.Email = "PlaceHolder@AvoidOtherWarning.com";
			AssertNoMessageError("No message error", address.ContactInfo, messageError);
		}

		public static void AssertAddressToOrder(string party, Address address, bool requireCompanyName = true)
		{
			var messageError = $"{party} party name and address information is required.";

			void AssertToOrder(string identifier)
			{
				var originalAddress1 = address.AddressLine1;
				var originalAddress2 = address.AddressLine2;
				var originalCity = address.City;
				var originalState = address.State;
				var originalCountryCode = address.Country.Code;
				var originalCountryName = address.Country.Name;
				var originalPostcode = address.Postcode;
				var originalContact = address.Contact;
				var originalContactPhone = address.Phone;
				var originalContactFax = address.Fax;
				var originalContactEmail = address.Email;
				var originalTaxNumber = address.TaxNumber;
				var originalTaxNumberType = address.TaxNumberType?.Code;

				address.CompanyName = string.Empty;
				address.AddressLine1 = string.Empty;
				address.AddressLine2 = string.Empty;
				address.Country.Name = string.Empty;

				if (requireCompanyName)
				{
					AssertHasMessageErrorContaining("has message error for empty address", address.CompanyNameInfo, messageError);

					address.CompanyName = identifier;
					AssertNoMessageErrorContaining($"no message error when '{identifier}'", address.CompanyNameInfo, messageError);
				}

				address.CompanyName = identifier;
				AssertEquals("CompanyName", identifier, address.CompanyName);
				AssertEquals("AddressLine1", string.Empty, address.AddressLine1);
				AssertEquals("AddressLine2", string.Empty, address.AddressLine2);
				AssertEquals("City", string.Empty, address.City);
				AssertEquals("State", string.Empty, address.State);
				AssertEquals("Country.Code", string.Empty, address.Country.Code);
				AssertEquals("Country.Name", string.Empty, address.Country.Name);
				AssertEquals("Postcode", string.Empty, address.Postcode);
				AssertEquals("Contact", string.Empty, address.Contact);
				AssertEquals("Phone", string.Empty, address.Phone);
				AssertEquals("Fax", string.Empty, address.Fax);
				AssertEquals("Email", string.Empty, address.Email);
				AssertEquals("TaxNumber", string.Empty, address.TaxNumber);
				AssertEquals("TaxNumberType", originalTaxNumberType.HasValue ? string.Empty : null, address.TaxNumberType?.Code);

				address.CompanyName = "reset";

				AssertEquals("CompanyName", "reset", address.CompanyName);
				AssertEquals("AddressLine1", originalAddress1, address.AddressLine1);
				AssertEquals("AddressLine2", originalAddress2, address.AddressLine2);
				AssertEquals("City", originalCity, address.City);
				AssertEquals("State", originalState, address.State);
				AssertEquals("Country.Code", originalCountryCode, address.Country.Code);
				AssertEquals("Country.Name", originalCountryName, address.Country.Name);
				AssertEquals("Postcode", originalPostcode, address.Postcode);
				AssertEquals("Contact", originalContact, address.Contact);
				AssertEquals("Phone", originalContactPhone, address.Phone);
				AssertEquals("Fax", originalContactFax, address.Fax);
				AssertEquals("Email", originalContactEmail, address.Email);
				AssertEquals("TaxNumber", originalTaxNumber, address.TaxNumber);
				AssertEquals("TaxNumberType", originalTaxNumberType, address.TaxNumberType?.Code);
			}

			CombineAssertions(() =>
			{
				AssertToOrder("To Order");
				AssertToOrder("To Order Of");
				AssertToOrder("To The Order");
				AssertToOrder("To The Order of");
			});
		}

		public static void AssertInvalidCodeValidation(string portName, Unloco port, string errorMessage = "You have not entered a valid code.")
		{
			port.Code = "~";
			AssertHasMessageError($"{portName} has invalid code", port.CodeInfo, errorMessage);

			port.Code = "CNSHA";
			AssertNoMessageError($"{portName} has valid code", port.CodeInfo, errorMessage);
		}
	}
}
