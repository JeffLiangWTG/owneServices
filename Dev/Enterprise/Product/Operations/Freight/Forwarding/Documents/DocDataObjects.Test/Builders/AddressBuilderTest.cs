using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using EnterpriseConstants = Enterprise.Core.Constants;
using UniversalCountry = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalRegistrationNumberType = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumberType;
using UniversalShipmentAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalUNLOCO = Enterprise.UniversalDataBuss.DataObjects.Universal.UNLOCO;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class AddressTest : TestCaseWithFactory
	{
		#region TestPopulate

		public void TestPopulate_FromObject()
		{
			var org = CreateOrganizationForAddressTesting();
			var orgAddress = org.MainAddress;
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_AdditionalAddressInformation = "add info";

			var address = AddressBuilder.Create(Context, (object)docAddress);

			AssertAddress(address, orgAddress: orgAddress);
			AssertEquals("AdditionalAddressInformation", "add info", address.AdditionalAddressInformation);
		}

		public void TestPopulate_FromJobDocAddress()
		{
			var org = CreateOrganizationForAddressTesting();
			var orgAddress = org.MainAddress;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_AdditionalAddressInformation = "add info";

			var address = AddressBuilder.Create(Context, docAddress);

			AssertAddress(address, orgAddress: orgAddress);
			AssertEquals("AdditionalAddressInformation", "add info", address.AdditionalAddressInformation);
		}

		public void TestPopulate_FromJobDocAddress_CompanyName()
		{
			var org = CreateOrganizationForAddressTesting();
			var orgAddress = org.MainAddress;
			orgAddress.OA_CompanyNameOverride = "Address override";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;

			var address = AddressBuilder.Create(Context, docAddress);

			AssertEquals("Company Name shoulld be from OrgAddress' OA_CompanyNameOverride", "Address override", address.CompanyName);

			orgAddress.OA_CompanyNameOverride = string.Empty;
			address = AddressBuilder.Create(Context, docAddress);

			AssertEquals("Company Name shoulld be from OH_FullName as OA_CompanyNameOverride is empty", "Fudge burners", address.CompanyName);
		}

		public void TestPopulate_FromOrgAddress_CompanyName()
		{
			var org = CreateOrganizationForAddressTesting();
			org.OH_FullName = "OH Address";
			var orgAddress = org.MainAddress;
			orgAddress.OA_CompanyNameOverride = "Address override";

			var address = AddressBuilder.Create(Context, orgAddress);

			AssertEquals("Company Name shoulld be from OrgAddress' OA_CompanyNameOverride", "Address override", address.CompanyName);

			orgAddress.OA_CompanyNameOverride = string.Empty;
			address = AddressBuilder.Create(Context, orgAddress);

			AssertEquals("Company Name shoulld be from OH_FullName as OA_CompanyNameOverride is empty", "OH Address", address.CompanyName);
		}

		public void TestPopulate_FromJobDocAddress_IncludeContactDetails()
		{
			var org = CreateOrganizationForAddressTesting();
			var orgAddress = org.MainAddress;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_Fax = "1111";
			orgAddress.OA_Phone = "2222";
			orgAddress.OA_Email = "john@test.com";
			docAddress.E2_Contact = "John";

			var address = AddressBuilder.Create(Context, docAddress, false);

			CombineAssertions(() =>
			{
				AssertEquals("Fax", string.Empty, address.Fax);
				AssertEquals("Phone", string.Empty, address.Phone);
				AssertEquals("Email", string.Empty, address.Email);
				AssertEquals("Contact", string.Empty, address.Contact);
			});

			address = AddressBuilder.Create(Context, docAddress);

			CombineAssertions(() =>
			{
				AssertEquals("Fax", "1111", address.Fax);
				AssertEquals("Phone", "2222", address.Phone);
				AssertEquals("Email", "john@test.com", address.Email);
				AssertEquals("Contact", "John", address.Contact);
				AssertEquals("Contact", "John", address.Contact);
			});
		}

		public void TestPopulate_FromZAddressWithContact()
		{
			var consol = Factory.New<ForwardingConsol>();
			var org = CreateOrganizationForAddressTesting();
			org.MainAddress.OA_Fax = "696969";
			org.MainAddress.OA_Phone = "420420";
			org.MainAddress.OA_Email = "fudge@burners.boop";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;

			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			consol.JK_OC_SendingForwarderContact = contact.PK;

			var address = AddressBuilder.Create(Context, consol.SendingForwarderWithContact, false);

			AssertAddress(address, orgAddress: org.MainAddress);

			CombineAssertions(
				"When includeContactInfo is false, contact details should not be populated.",
				() =>
				{
					AssertEquals("Fax", string.Empty, address.Fax);
					AssertEquals("Phone", string.Empty, address.Phone);
					AssertEquals("Email", string.Empty, address.Email);
					AssertEquals("Contact", string.Empty, address.Contact);
				}
			);

			address = AddressBuilder.Create(Context, consol.SendingForwarderWithContact);

			AssertAddress(address, orgAddress: org.MainAddress);

			CombineAssertions(
				"When contact details empty, address should fall back on org details for population.",
				() =>
				{
					AssertEquals("Fax", "696969", address.Fax);
					AssertEquals("Phone", "420420", address.Phone);
					AssertEquals("Email", "fudge@burners.boop", address.Email);
					AssertEquals("Contact", string.Empty, address.Contact);
				}
			);

			contact = CreateContactForAddressTesting(org.PK);
			consol.JK_OC_SendingForwarderContact = contact.PK;
			address = AddressBuilder.Create(Context, consol.SendingForwarderWithContact);

			AssertAddress(address, orgAddress: org.MainAddress);

			CombineAssertions(
				"When contact details not empty, address should use them for population.",
				() =>
				{
					AssertEquals("Fax", "67890", address.Fax);
					AssertEquals("Phone", "12345", address.Phone);
					AssertEquals("Email", "x@x.com", address.Email);
					AssertEquals("Contact", "Mr X", address.Contact);
				}
			);

			var orgAddress = org.Addresses.AddNew();
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			address = AddressBuilder.Create(Context, consol.SendingForwarderWithContact, true, true);

			CombineAssertions(
				"When contact details not empty, address should use them for population.",
				() =>
				{
					AssertEquals("Fax", "696969", address.Fax);
					AssertEquals("Phone", "420420", address.Phone);
					AssertEquals("Email", "fudge@burners.boop", address.Email);
					AssertEquals("Contact", string.Empty, address.Contact);
				}
			);

			orgAddress.OA_Fax = "000000";
			orgAddress.OA_Phone = "11111";
			orgAddress.OA_Email = "fudge@wtc.boop";
			address = AddressBuilder.Create(Context, consol.SendingForwarderWithContact, true, true);

			CombineAssertions(
				"When contact details not empty, address should use them for population.",
				() =>
				{
					AssertEquals("Fax", "000000", address.Fax);
					AssertEquals("Phone", "11111", address.Phone);
					AssertEquals("Email", "fudge@wtc.boop", address.Email);
					AssertEquals("Contact", string.Empty, address.Contact);
				}
			);
		}

		public void TestPopulate_FromOrgAddress()
		{
			var org = CreateOrganizationForAddressTesting();

			AssertAddress(AddressBuilder.Create(Context, org.MainAddress), orgAddress: org.MainAddress);
		}

		public void TestPopulate_FromIAddress()
		{
			var org = CreateOrganizationForAddressTesting();
			var orgAddress = org.MainAddress;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_AdditionalAddressInformation = "add info";

			var address = AddressBuilder.Create(Context, docAddress);

			AssertAddress(address, orgAddress: orgAddress);
			AssertEquals("AdditionalAddressInformation", "add info", address.AdditionalAddressInformation);

			var addressFromIAddress = AddressBuilder.Create(Context, address);

			AssertEquals("AddressIdentifier", address.AddressIdentifier, addressFromIAddress.AddressIdentifier);
			AssertEquals("HeaderIdentifier", address.HeaderIdentifier, addressFromIAddress.HeaderIdentifier);
			AssertEquals("CompanyName", address.CompanyName, addressFromIAddress.CompanyName);
			AssertEquals("AddressLine1", address.AddressLine1, addressFromIAddress.AddressLine1);
			AssertEquals("AddressLine2", address.AddressLine2, addressFromIAddress.AddressLine2);
			AssertEquals("City", address.City, addressFromIAddress.City);
			AssertEquals("State", address.State, addressFromIAddress.State);
			AssertEquals("Postcode", address.Postcode, addressFromIAddress.Postcode);
			AssertEquals("Country.Code", address.Country.Code, addressFromIAddress.Country.Code);
			AssertEquals("Country.Name", address.Country.Name, addressFromIAddress.Country.Name);
			AssertEquals("Unloco.Code", address.Unloco.Code, addressFromIAddress.Unloco.Code);
			AssertEquals("Unloco.Name", address.Unloco.Name, addressFromIAddress.Unloco.Name);
			AssertEquals("Unloco.Country.Code", address.Unloco.Country.Code, addressFromIAddress.Unloco.Country.Code);
			AssertEquals("Unloco.Country.Name", address.Unloco.Country.Name, addressFromIAddress.Unloco.Country.Name);
			AssertEquals("AdditionalAddressInformation", address.AdditionalAddressInformation, addressFromIAddress.AdditionalAddressInformation);
		}

		public void TestPopulate_FromOrgHeader()
		{
			var org = CreateOrganizationForAddressTesting();
			var contact = CreateContactForAddressTesting(org.PK);

			var address = AddressBuilder.Create(Context, org, contact);
			AssertAddress(address, orgAddress: org.MainAddress);
			AssertEquals("Contact", "Mr X", address.Contact);
			AssertEquals("Phone", "12345", address.Phone);
			AssertEquals("Fax", "67890", address.Fax);
			AssertEquals("Email", "x@x.com", address.Email);
		}

		public void TestPopulate_ForCurrentUser()
		{
			var address = AddressBuilder.CreateForCurrentUser(Context);

			AssertCurrentUserAddressData(address);
			AssertEquals("AddressIdentifier", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, address.AddressIdentifier);
			AssertEquals("HeaderIdentifier", GlbBranch.CurrentBranch.OrgProxy.MainAddress.Header.PK, address.HeaderIdentifier);
		}

		public void TestPopulateUnlocoFromOrgAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_RL_NKRelatedPortCode = "CNSHA";

			var address = AddressBuilder.Create(Context, orgAddress);
			AssertEquals("Unloco.Code", "CNSHA", address.Unloco.Code);
			AssertEquals("Unloco.Name", "Shanghai Hongqiao International Apt", address.Unloco.Name);
		}

		public void TestPopulateUnlocoFromJobDocAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_RL_NKRelatedPortCode = "CNSHA";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;

			var address = AddressBuilder.Create(Context, docAddress);
			AssertEquals("Unloco.Code", "CNSHA", address.Unloco.Code);
			AssertEquals("Unloco.Name", "Shanghai Hongqiao International Apt", address.Unloco.Name);

			docAddress.E2_AddressOverride = true;
			address = AddressBuilder.Create(Context, docAddress);
			AssertEquals("Unloco.Code", string.Empty, address.Unloco.Code);
			AssertEquals("Unloco.Name", string.Empty, address.Unloco.Name);
		}

		public void TestCreateFromUniversalShipmentAddress()
		{
			var universalAddress = new UniversalShipmentAddress(DefaultDataObjectWriterStrategy.Instance)
			{
				CompanyName = "Fudge burners",
				Address1 = "line 1",
				Address2 = "line 2",
				City = "Sydney",
				State = "NSW",
				Postcode = "2000",
				Country = new UniversalCountry
				{
					Code = "AU",
					Name = "Australia"
				},
				Port = new UniversalUNLOCO
				{
					Code = "AUSYD",
					Name = "Sydney"
				},
				Contact = "Jos Vermeulen",
				Phone = "+33457787896",
				Fax = "+334587521457",
				Email = "jos.vermeulen@company.com"
			};

			List<UniversalRegistrationNumber> CreateRegistrationNumbers()
			{
				return new List<UniversalRegistrationNumber>
				{
					new UniversalRegistrationNumber
					{
						Type = new UniversalRegistrationNumberType
						{
							Code = "AAA"
						},
						Value = "12345"
					},
					new UniversalRegistrationNumber
					{
						Type = new UniversalRegistrationNumberType
						{
							Code = "BBB"
						},
						Value = "67890"
					}
				};
			}

			universalAddress.SetRegistrationNumberCollection(CreateRegistrationNumbers);

			var address = AddressBuilder.Create(Context, universalAddress);

			AssertAddress(address, true);
		}

		public void TestCreateFromUniversalShipmentAddress_Null()
		{
			UniversalShipmentAddress universalAddress = null;
			var address = AddressBuilder.Create(Context, universalAddress);
			AssertNotNull(address);
		}

		void AssertAddress(IAddress address, bool assertContactInfo = false, OrgAddress orgAddress = null)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", "Fudge burners", address.CompanyName);
				AssertEquals("AddressLine1", "line 1", address.AddressLine1);
				AssertEquals("AddressLine2", "line 2", address.AddressLine2);
				AssertEquals("City", "Sydney", address.City);
				AssertEquals("State", "NSW", address.State);
				AssertEquals("Postcode", "2000", address.Postcode);

				AssertEquals("Country.Code", "AU", address.Country.Code);
				AssertEquals("Country.Name", "Australia", address.Country.Name);

				AssertEquals("Unloco.Code", "AUSYD", address.Unloco.Code);
				AssertEquals("Unloco.Name", "Sydney", address.Unloco.Name);

				AssertEquals("Unloco.Country.Code", "AU", address.Unloco.Country.Code);
				AssertEquals("Unloco.Country.Name", "Australia", address.Unloco.Country.Name);

				AssertMultilineASCIIEquals("RegistrationNumbers",
@"AAA|12345
BBB|67890",
					string.Join("\r\n", address.RegistrationNumbers.Select(rn => string.Concat(rn.Type.Code, "|", rn.Value))));

				if (assertContactInfo)
				{
					AssertEquals("Contact", "Jos Vermeulen", address.Contact);
					AssertEquals("Phone", "+33457787896", address.Phone);
					AssertEquals("Fax", "+334587521457", address.Fax);
					AssertEquals("Email", "jos.vermeulen@company.com", address.Email);
				}

				if (orgAddress != null)
				{
					AssertEquals("AddressIdentifier", orgAddress.PK, address.AddressIdentifier);
					AssertEquals("HeaderIdentifier", orgAddress.Header.PK, address.HeaderIdentifier);
				}
			});
		}

		OrgHeader CreateOrganizationForAddressTesting()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Fudge burners";
			org.OH_RL_NKClosestPort = "AUSYD";

			org.MainAddress.Address1 = "line 1";
			org.MainAddress.Address2 = "line 2";
			org.MainAddress.City = "Sydney";
			org.MainAddress.Postcode = "2000";

			org.CustomsCodes.AddNew("AAA", "12345");
			org.CustomsCodes.AddNew("BBB", "67890");

			return org;
		}

		OrgContact CreateContactForAddressTesting(ZGuid orgPK)
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgPK;
			contact.OC_ContactName = "Mr X";
			contact.OC_Email = "x@x.com";
			contact.OC_Phone = "12345";
			contact.OC_Fax = "67890";

			return contact;
		}

		#endregion

		#region TestPopulate_FromEmpty

		public void TestPopulate_FromEmpty()
		{
			var docAddress = Factory.New<JobDocAddress>();
			Assert("prerequisite: address is empty", docAddress.IsEmpty);

			var dataObject = AddressBuilder.Create(Context, docAddress);
			AssertAddressEmptyDataObject(dataObject);

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "";

			dataObject = AddressBuilder.Create(Context, orgAddress);
			AssertAddressEmptyDataObject(dataObject);
		}

		public void TestPopulate_FromNull()
		{
			var dataObject = AddressBuilder.Create(Context, (JobDocAddress)null);
			AssertAddressEmptyDataObject(dataObject);

			dataObject = AddressBuilder.Create(Context, (OrgAddress)null);
			AssertAddressEmptyDataObject(dataObject);
		}

		void AssertAddressEmptyDataObject(IAddress dataObject)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName", ZString.Empty, dataObject.CompanyName);
				AssertEquals("AddressLine1", ZString.Empty, dataObject.AddressLine1);
				AssertEquals("AddressLine2", ZString.Empty, dataObject.AddressLine2);
				AssertEquals("AdditionalAddressInformation", ZString.Empty, dataObject.AdditionalAddressInformation);
				AssertEquals("City", ZString.Empty, dataObject.City);
				AssertEquals("State", ZString.Empty, dataObject.State);
				AssertEquals("Postcode", ZString.Empty, dataObject.Postcode);

				AssertEquals("Country.Code", ZString.Empty, dataObject.Country.Code);
				AssertEquals("Country.Code", ZString.Empty, dataObject.Country.Name);
				AssertEquals("Unloco.Code", ZString.Empty, dataObject.Unloco.Code);
				AssertEquals("Unloco.Name", ZString.Empty, dataObject.Unloco.Name);
				AssertEquals("Unloco.Country.Code", ZString.Empty, dataObject.Unloco.Country.Code);
				AssertEquals("Unloco.Country.Name", ZString.Empty, dataObject.Unloco.Country.Name);
			});
		}

		#endregion

		#region TestToStringReturnsFormattedAddress

		public void TestToStringReturnsFormattedAddress()
		{
			var org = CreateOrganizationForAddressTesting();

			var address = AddressBuilder.Create(Context, org.MainAddress);

			AssertMultilineASCIIEquals("formatted address",
@"FUDGE BURNERS
LINE 1
LINE 2
SYDNEY NSW 2000
AUSTRALIA", address.ToString());
		}

		#endregion

		#region TestAddressFormatted

		public void TestAddressFormatted()
		{
			var address = CreateAddressForTest();

			AssertMultilineASCIIEquals("AddressFormatted",
@"ADDRESS LINE 1
ADDRESS LINE 2
SYDNEY NSW 2015
AUSTRALIA", address.AddressFormatted);
		}

		public void TestAddressFormatted_Update()
		{
			var address = CreateAddressForTest();

			address.AddressLine1 = "update 1";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
ADDRESS LINE 2
SYDNEY NSW 2015
AUSTRALIA", address.AddressFormatted);

			address.AddressLine2 = "update 2";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
SYDNEY NSW 2015
AUSTRALIA", address.AddressFormatted);

			address.State = "QLD";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
SYDNEY QLD 2015
AUSTRALIA", address.AddressFormatted);

			address.Postcode = "4000";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
SYDNEY QLD 4000
AUSTRALIA", address.AddressFormatted);

			address.City = "Brisbane";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
BRISBANE QLD 4000
AUSTRALIA", address.AddressFormatted);

			address.Country.Code = "DE";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
4000 BRISBANE
GERMANY", address.AddressFormatted);

			address.Country.Name = "Norway";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
4000 BRISBANE
NORWAY", address.AddressFormatted);

			address.Country = new Country(context.Factory, context.Countries)
			{
				Code = ZString.Empty,
				Name = ZString.Empty
			};

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
BRISBANE QLD 4000", address.AddressFormatted);

			address.Country.Name = "Finland";

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
BRISBANE QLD 4000
FINLAND", address.AddressFormatted);

			address.Country = null;

			AssertMultilineASCIIEquals("AddressFormatted",
@"UPDATE 1
UPDATE 2
BRISBANE QLD 4000", address.AddressFormatted);
		}

		public void TestAddressFormatted_WithFormattingRule()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityPostcodeCountry;

			var address = CreateAddressForTest();

			AssertMultilineASCIIEquals("AddressFormatted",
@"ADDRESS LINE 1
ADDRESS LINE 2
SYDNEY
2015
AUSTRALIA", address.AddressFormatted);
		}

		Address CreateAddressForTest()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "Address Line 1";
			orgAddress.OA_Address2 = "Address Line 2";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_PostCode = "2015";
			orgAddress.OA_City = "Sydney";
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddress.OA_Email = "test@test.com";

			return AddressBuilder.Create(Context, orgAddress);
		}

		#endregion

		#region TestAddressIsNull

		public void TestIsPickUp_ExportReceivingDepotIsNull_ReturnFalseWithoutException()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "SEA";
			consol.JK_ConsolMode = EnterpriseConstants.ContainerModes.FCL;
			var shipment = consol.Shipments.AddNew();
			var exportDepot = Factory.New<OrgHeader>();
			shipment.JS_OA_ExportReceivingDepot = exportDepot.MainAddress.PK;
			consol.Shipments.AddNew();

			var container = Factory.New<ForwardingContainer>();
			consol.Containers.Add(container);
			container.JC_DeliveryMode = EnterpriseConstants.DeliveryModes.Codes.CFS_CFS;

			AssertNoExceptionThrown(() => consol.IsPickup(true));
			AssertNoExceptionThrown(() => consol.IsPickup(false));
		}

		#endregion

		#region TestCreateRegistrationNumbers

		public void TestCreateRegistrationNumbers_Eori()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DE12345a", Core.Constants.CountryCodes.Germany);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "fr67890a", Core.Constants.CountryCodes.France);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI67890a", Core.Constants.CountryCodes.Spain);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "xI67890a", Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "XI67890a", Core.Constants.CountryCodes.Poland);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Empty, Core.Constants.CountryCodes.Portugal);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "abc123", ZString.Empty);
			orgHeader.CustomsCodes.AddNew(ZString.Empty, "cn123", Core.Constants.CountryCodes.China);

			var address = AddressBuilder.Create(Context, orgHeader.MainAddress);

			AssertEquals("DE12345A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Germany).Value);
			AssertEquals("FR67890A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.France).Value);
			AssertEquals("ESXI67890A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Spain).Value);
			AssertEquals("XI67890A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.UnitedKingdom).Value);
			AssertEquals("XI67890a", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Poland).Value);
			AssertEquals(ZString.Empty, address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Portugal).Value);
			AssertEquals("ABC123", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == ZString.Empty).Value);
			AssertEquals("cn123", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.China).Value);

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgHeader.MainAddress.PK;

			address = AddressBuilder.Create(Context, docAddress);

			AssertEquals("DE12345A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Germany).Value);
			AssertEquals("FR67890A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.France).Value);
			AssertEquals("ESXI67890A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Spain).Value);
			AssertEquals("XI67890A", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.UnitedKingdom).Value);
			AssertEquals("XI67890a", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Poland).Value);
			AssertEquals(ZString.Empty, address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.Portugal).Value);
			AssertEquals("ABC123", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == ZString.Empty).Value);
			AssertEquals("cn123", address.RegistrationNumbers.First(x => x.CountryOfIssue.Code == Core.Constants.CountryCodes.China).Value);
		}

		#endregion

		#region Implementation

		IContext Context => context ?? (context = new CommonContext(Factory));
		IContext context;

		#endregion
	}
}
