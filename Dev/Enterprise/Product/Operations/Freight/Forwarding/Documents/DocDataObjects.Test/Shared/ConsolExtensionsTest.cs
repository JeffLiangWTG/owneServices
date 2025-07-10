using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class ConsolExtensionsTest : TestCaseWithFactory
	{
		public void TestGetCustomsBrokerAddress()
		{
			var context = new ContextWithCarrierUnlocoMapping(Factory, ZGuid.Empty);

			ForwardingConsol consol = null;

			var orgHeader1 = CreateOrgHeader("OrgHeader 1");
			var orgHeader2 = CreateOrgHeader("OrgHeader 2");
			var orgHeader3 = CreateOrgHeader("OrgHeader 3");
			var orgHeader4 = CreateOrgHeader("OrgHeader 4");

			Factory.Save();

			AssertAddressIsEmpty(consol.GetCustomsBrokerAddress(context));

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			AssertAddressIsEmpty(consol.GetCustomsBrokerAddress(context));

			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgHeader1.PK;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader2.PK;
			AssertAddressIsEmpty(consol.GetCustomsBrokerAddress(context));

			orgHeader1.OH_IsBroker = true;
			Factory.Save();
			AssertEquals("OrgHeader 1", consol.GetCustomsBrokerAddress(context).CompanyName);

			orgHeader2.OH_IsBroker = true;
			Factory.Save();
			AssertEquals("OrgHeader 2", consol.GetCustomsBrokerAddress(context).CompanyName);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_OH_ExportBroker = orgHeader3.PK;
			AssertEquals("OrgHeader 3", consol.GetCustomsBrokerAddress(context).CompanyName);

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "EXP";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_RL_NKFinalDestination = "ZAAAM";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_ExternalBroker = orgHeader4.PK;
			AssertEquals("OrgHeader 4", consol.GetCustomsBrokerAddress(context).CompanyName);
		}

		public void TestGetCustomsBrokerContactInfo()
		{
			var context = new ContextWithCarrierUnlocoMapping(Factory, ZGuid.Empty);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";

			var orgHeader = CreateOrgHeader("OrgHeader 1");
			orgHeader.MainAddress.OA_Fax = "Fax_Main";
			orgHeader.MainAddress.OA_Phone = "Phone_Main";
			orgHeader.MainAddress.OA_Email = "Email_Main";
			orgHeader.OH_IsBroker = true;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgHeader.PK;

			void AddOrgContact(string fax, string phone, string email, string contactName, string transportMode, string localPortFilter = "")
			{
				var contact = orgHeader.Contacts.AddNew();
				contact.OC_IsActive = true;
				contact.OC_Fax = fax;
				contact.OC_Phone = phone;
				contact.OC_Email = email;
				contact.OC_ContactName = contactName;
				var document = contact.Documents.AddNew();
				document.OD_DocumentGroup = ContactType.ExportBroker.Code;
				document.OD_FilterShipmentMode = transportMode;
				document.OD_FilterLocalPort = localPortFilter;
			}

			AddOrgContact("Fax_1", "Phone_1", "Email_1", "ContactName_1", TransportModes.Air);
			Factory.Save();
			var address = consol.GetCustomsBrokerAddress(context);
			AssertEquals("Fax_Main", address.Fax);
			AssertEquals("Phone_Main", address.Phone);
			AssertEquals("Email_Main", address.Email);
			AssertEquals(ZString.Empty, address.Contact);

			AddOrgContact("Fax_2", "Phone_2", "Email_2", "ContactName_2", TransportModes.All);
			Factory.Save();
			address = consol.GetCustomsBrokerAddress(context);
			AssertEquals("Fax_2", address.Fax);
			AssertEquals("Phone_2", address.Phone);
			AssertEquals("Email_2", address.Email);
			AssertEquals("ContactName_2", address.Contact);

			AddOrgContact("Fax_3", "Phone_3", "Email_3", "ContactName_3", TransportModes.Sea);
			Factory.Save();
			address = consol.GetCustomsBrokerAddress(context);
			AssertEquals("Fax_3", address.Fax);
			AssertEquals("Phone_3", address.Phone);
			AssertEquals("Email_3", address.Email);
			AssertEquals("ContactName_3", address.Contact);

			AddOrgContact("Fax_4", "Phone_4", "Email_4", "ContactName_4", TransportModes.Sea, "AU");
			Factory.Save();
			address = consol.GetCustomsBrokerAddress(context);
			AssertEquals("Fax_4", address.Fax);
			AssertEquals("Phone_4", address.Phone);
			AssertEquals("Email_4", address.Email);
			AssertEquals("ContactName_4", address.Contact);

			AddOrgContact("Fax_5", "Phone_5", "Email_5", "ContactName_5", TransportModes.Sea, "AUSYD");
			Factory.Save();
			address = consol.GetCustomsBrokerAddress(context);
			AssertEquals("Fax_5", address.Fax);
			AssertEquals("Phone_5", address.Phone);
			AssertEquals("Email_5", address.Email);
			AssertEquals("ContactName_5", address.Contact);
		}

		OrgHeader CreateOrgHeader(ZString companyNameOverride)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_CompanyNameOverride = companyNameOverride;

			return orgHeader;
		}

		void AssertAddressIsEmpty(Address address, bool checkEmptyInfo = true)
		{
			AssertNullOrEmpty(nameof(address.CompanyName), address.CompanyName);
			AssertNullOrEmpty(nameof(address.AddressLine1), address.AddressLine1);
			AssertNullOrEmpty(nameof(address.AddressLine2), address.AddressLine2);
			AssertNullOrEmpty(nameof(address.AdditionalAddressInformation), address.AdditionalAddressInformation);
			AssertNullOrEmpty(nameof(address.City), address.City);
			AssertNullOrEmpty(nameof(address.State), address.State);
			AssertNullOrEmpty(nameof(address.Postcode), address.Postcode);
			AssertNullOrEmpty(nameof(address.Fax), address.Fax);
			AssertNullOrEmpty(nameof(address.Phone), address.Phone);
			AssertNullOrEmpty(nameof(address.Email), address.Email);

			if (checkEmptyInfo)
			{
				AssertNotNull(nameof(address.Country), address.Country);
				AssertNullOrEmpty(nameof(address.Country.Code), address.Country.Code);
				AssertNullOrEmpty(nameof(address.Country.Name), address.Country.Name);

				AssertNotNull(nameof(address.Unloco), address.Unloco);
				AssertNullOrEmpty(nameof(address.Unloco.Code), address.Unloco.Code);
				AssertNullOrEmpty(nameof(address.Unloco.Name), address.Unloco.Name);

				AssertEquals(nameof(address.RegistrationNumbers), 0, address.RegistrationNumbers.Count);
			}
		}
	}
}
