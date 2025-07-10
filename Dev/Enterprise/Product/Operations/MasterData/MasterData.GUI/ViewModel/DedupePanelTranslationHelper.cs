using System.Collections.Generic;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.GUI
{
	static class DedupePanelTranslationHelper
	{
		static DedupePanelTranslationHelper()
		{
			InitialiseTranslationDictionary();
		}

		public static string GetTranslation(string key)
		{
			var result = key;
			if (!string.IsNullOrWhiteSpace(key) && translationDictionary.TryGetValue(key, out var translation))
			{
				result = translation;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Dictionary keys")]
		static void InitialiseTranslationDictionary()
		{
			translationDictionary = new Dictionary<string, ResourceString>
			{
				[DeduplicationProvider.Constants.Name] = ResString.GetMultilingualString("ef0cc16a-6829-49ac-9866-94953a3251c8", "Name"),
				[DeduplicationProvider.Constants.Number] = ResString.GetMultilingualString("19077004-feef-48c2-a7ae-d83f126316cb", "Number"),
				[DeduplicationProvider.Constants.Website] = ResString.GetMultilingualString("53f4b06d-3f6b-4a2e-bcfa-08dbca081c2a", "Website"),
				[DeduplicationProvider.Constants.Domain] = ResString.GetMultilingualString("b776edb5-4c7a-4f89-ab26-a242afe9d44c", "Domain"),
				[DeduplicationProvider.Constants.Email] = ResString.GetMultilingualString("fce6e104-6cf3-466d-a879-ed32c8d8ee9a", "E-mail"),
				[DeduplicationProvider.Constants.Similarity] = ResString.GetMultilingualString("5fbb2bf7-06fd-4ab9-8da6-47da29dba41d", "Similarity"),
				[DeduplicationProvider.Constants.Source] = ResString.GetMultilingualString("b92e732a-65d9-41a5-ab74-0fdde4eeff28", "Source"),
				[DeduplicationProvider.Constants.Address] = ResString.GetMultilingualString("adac3536-455e-48fb-a7df-28fade3c7a67", "Address"),
				[DeduplicationProvider.Constants.Brand] = ResString.GetMultilingualString("8f1f95ec-b31a-4720-919d-29d0d50bca90", "Brand"),
				[DeduplicationProvider.Constants.ContactPhone] = ResString.GetMultilingualString("9f513252-5b54-434f-a029-89cdb9b650b4", "Contact Phone"),
				[DeduplicationProvider.Constants.AddressPhone] = ResString.GetMultilingualString("476af117-f69e-4845-9614-e5f686ae4dad", "Address Phone"),
				[DeduplicationProvider.Constants.ContactMobile] = ResString.GetMultilingualString("a3299f7b-c792-4979-85da-ae68ce093c23", "Contact Mobile"),
				[DeduplicationProvider.Constants.AddressMobile] = ResString.GetMultilingualString("c170e7bf-77fc-4e69-8a9c-544a967e6a92", "Address Mobile"),
				[DeduplicationProvider.Constants.Emails] = ResString.GetMultilingualString("2234f1b6-46e4-4f93-a939-a4b441c042cb", "E-mails"),
				[DeduplicationProvider.Constants.Birthday] = ResString.GetMultilingualString("e1dbadb5-6cbe-47c3-a732-fc9b8083bd89", "Birthday"),
				[DeduplicationProvider.Constants.Birthdays] = ResString.GetMultilingualString("2cceacd0-e83a-4cb3-a58e-5f9eb38c8bab", "Birthdays"),
				[DeduplicationProvider.Constants.OtherPhone] = ResString.GetMultilingualString("e6fb07c5-e323-4196-b4db-aa9665c8a474", "Other Ph"),
				[DeduplicationProvider.Constants.HomePhone] = ResString.GetMultilingualString("cf370cfc-a318-4254-81b7-ba4bfc3c7a94", "Home Ph"),
				[DeduplicationProvider.Constants.ContactFax] = ResString.GetMultilingualString("faa1bf4f-d445-4ce4-a2b8-d59f4bbba272", "Contact Fax"),
				[DeduplicationProvider.Constants.AddressFax] = ResString.GetMultilingualString("bf7f314a-ccee-4451-89fc-2b35d68f964c", "Address Fax"),
				[DeduplicationProvider.Constants.Type] = ResString.GetMultilingualString("61774c8d-88bd-48c3-8067-6db57454c540", "Type"),
				[DeduplicationProvider.Constants.Country] = ResString.GetMultilingualString("f10c4bf2-d915-4041-9612-bc3d41dbff26", "Country/Region"),
				[DeduplicationProvider.Constants.CusCodeCustomsRegNo] = ResString.GetMultilingualString("266b77c0-a021-484c-a7dd-9468d69ac4cf", "Reg No"),
				[DeduplicationProvider.Constants.Coordinates] = ResString.GetMultilingualString("7c577f05-0062-4274-ae71-495d7fabfec2", "Coordinates"),
				[DeduplicationProvider.Constants.Code] = ResString.GetMultilingualString("41a56bf2-62fa-4e5b-9c7f-0aaeaf46bbb4", "Code"),
				[DeduplicationProvider.Constants.Organisations] = ResString.GetMultilingualString("3036f37d-a733-4b75-8fac-3dd549b07c1f", "Organizations"),
				[DeduplicationProvider.Constants.Addresses] = ResString.GetMultilingualString("301eb2ab-5fe3-4d66-b388-45f2956cc65f", "Addresses"),
				[DeduplicationProvider.Constants.Contacts] = ResString.GetMultilingualString("5b88013a-ee76-4e1f-8717-ed287e968260", "Contacts"),
				[DeduplicationProvider.Constants.RegistrationCodes] = ResString.GetMultilingualString("7fcd6ebc-1bd8-4b0b-9f34-f63dfa75a307", "Registration Codes"),
				[DeduplicationProvider.Constants.Websites] = ResString.GetMultilingualString("8b9e49b1-01f7-401e-a113-4c646fa2ccd1", "Websites"),
				[DeduplicationProvider.Constants.Domains] = ResString.GetMultilingualString("400ba6b1-4003-4328-9a66-918306d4d271", "Domains"),
				[DeduplicationProvider.Constants.PhoneNumbers] = ResString.GetMultilingualString("80db641b-1ded-42a7-80ec-b1b7895b632d", "Phone Numbers"),
				[DeduplicationProvider.Constants.OrganisationNames] = ResString.GetMultilingualString("cfa96c98-15f9-4d8b-87f0-e2b78247b611", "Organization Names"),
				[DeduplicationProvider.Constants.PersonNames] = ResString.GetMultilingualString("56022bf1-3048-4c36-b2ba-ad0c41807570", "Person Names"),
				[DeduplicationProvider.Constants.Person] = ResString.GetMultilingualString("f94ebb2d-4655-42ff-ba58-444001ed7fe3", "Person"),
				[DeduplicationProvider.Constants.Staff] = ResString.GetMultilingualString("43fdcd98-e901-40dd-8f5b-afc27d416e32", "Staff"),
				[DeduplicationProvider.Constants.Applicant] = ResString.GetMultilingualString("2a0e06c4-5d6c-4989-86d4-6e7f33bac6ca", "Applicant"),
				[DeduplicationProvider.Constants.PersonPhone] = ResString.GetMultilingualString("44808808-eb37-423f-9a02-fca022965101", "Person Phone"),
				[DeduplicationProvider.Constants.PersonWorkPhone] = ResString.GetMultilingualString("22349ce6-442c-4e65-a937-9a21a3d18c0a", "Person Work Phone"),
				[DeduplicationProvider.Constants.PersonMobile] = ResString.GetMultilingualString("2d04ea07-7e58-4377-a569-c21022125a57", "Person Mobile"),
				[DeduplicationProvider.Constants.PersonFax] = ResString.GetMultilingualString("2f41956e-95fc-418f-b259-7dcc8ffd268d", "Person Fax"),
				[DeduplicationProvider.Constants.ActiveAssociations] = ResString.GetMultilingualString("4b9c7a15-f1e8-4962-aa0b-84d28b682a99", "Active Associations"),
				[DeduplicationProvider.Constants.PrimaryWorkplace] = ResString.GetMultilingualString("F2079788-CC65-4F57-9A7F-EB9ADBF612A0", "Primary Workplace"),
				[DeduplicationProvider.Constants.UNLOCO] = ResString.GetMultilingualString("57533D76-B7E8-45FD-BE69-0A0A3C0CB2C0", "UNLOCO"),
				[DeduplicationProvider.Constants.City] = ResString.GetMultilingualString("4D3A5420-4C52-4E4B-8184-14DB7B248A1B", "City"),
				[DeduplicationProvider.Constants.State] = ResString.GetMultilingualString("B6F6D459-C7C6-4830-A6F0-73935FB6C502", "State"),
				[DeduplicationProvider.Constants.Active] = ResString.GetMultilingualString("2738E34D-619A-4A1D-81AD-85C2903EB1B2", "Active"),
				["None"] = ResString.GetMultilingualString("774FD060-6C97-47C3-AF97-4303EF50374D", "None"),
				["Low"] = ResString.GetMultilingualString("5D900F25-67F8-4176-BCC7-28CF065A0079", "Low"),
				["Medium"] = ResString.GetMultilingualString("D3794F7A-AA11-4C8E-AF36-3597535A210F", "Medium"),
				["High"] = ResString.GetMultilingualString("AC266F46-BCB3-4F69-8D55-047D98E03DB3", "High"),
				["Exact"] = ResString.GetMultilingualString("B1A9D418-DA40-400C-A13A-6420F87A50AB", "Exact"),
				["Undefined"] = ResString.GetMultilingualString("229AB2A3-21AB-4135-A621-B90BFCAEE005", "Undefined"),
				["Merge"] = ResString.GetMultilingualString("C9A3FD1D-A76A-495D-B8F5-40825D3D090A", "Add"),
				["Add"] = ResString.GetMultilingualString("ECE054C9-F35A-442E-9B49-692323D1A6ED", "Merge")
			};
		}

		[ThreadSafe]
		internal static IReadOnlyDictionary<string, ResourceString> translationDictionary;
	}
}
