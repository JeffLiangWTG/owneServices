using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	public sealed class AssertionHelper : TestCaseWithFactory
	{
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

		public static void AssertAddressToOrder(Address address)
		{
			CombineAssertions(() =>
			{
				AssertSameAsOtherAddressSupport(address, "To Order");
				AssertSameAsOtherAddressSupport(address, "To Order Of");
				AssertSameAsOtherAddressSupport(address, "To The Order");
				AssertSameAsOtherAddressSupport(address, "To The Order of");
			});
		}

		public static void AssertAddressSameAsConsignee(Address address)
		{
			AssertSameAsOtherAddressSupport(address, "Same As Consignee");
		}

		static void AssertSameAsOtherAddressSupport(Address address, string identifier)
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
	}
}
