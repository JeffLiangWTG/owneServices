using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BIRDOrganisationMatchingTest : TestCaseWithFactory
	{
		public void TestSetOrganisationAddress()
		{
			OrgAddress organisation = CreateOrganisationAddress(OrgCusCode.USACodeTypes.ManufacturerID, "AUABCEXP6390ALE");
			OrgAddress organisation2 = CreateOrganisationAddress(OrgCusCode.USACodeTypes.ManufacturerID, "AUABCEXP6390ALD");
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();

			NotificationCollection notifications = new NotificationCollection();

			declaration.JE_OA_ManufacturerAddress = BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "AUABCEXP6390ALE", "Manufacturer", notifications).PK;

			AssertEquals("Manufacturer should have been set to the organisation", organisation.PK, declaration.JE_OA_ManufacturerAddress);
			AssertEquals("No warning or error", false, notifications.HasErrors());
			AssertEquals("No warning or error", false, notifications.HasWarnings());
		}

		public void TestNotificationsBeingNull()
		{
			AssertNoExceptionThrown(() => BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "AUABCEXP6390ALE", "Manufacturer", null));
		}

		public void TestSetOrganisationWhenThereIsOnlyOneOrganisation()
		{
			OrgHeader organisation = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			OrgHeader organisation2 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-3456789XY");
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();

			NotificationCollection notifications = new NotificationCollection();

			declaration.JE_OH_Importer = BIRDOrganisationMatching.GetOrganisation(Factory, OrgMatchedCustomsRegNoType.EIN, "12-3456789XY", "Importer", notifications).PK;

			AssertEquals("Importer should have been set to the organisation", organisation.PK, declaration.JE_OH_Importer);
			AssertEquals("No warning or error", false, notifications.HasErrors());
			AssertEquals("No warning or error", false, notifications.HasWarnings());
		}

		public void TestSetOrganisationWhenThereIsNoOrganisation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			NotificationCollection notifications = new NotificationCollection();

			declaration.JE_OH_Importer = BIRDOrganisationMatching.GetOrganisation(Factory, OrgMatchedCustomsRegNoType.EIN, "12-3456789XY", "Importer", notifications)?.PK ?? ZGuid.Empty;

			AssertEquals("Importer could not be set as there is no matching organisation", ZGuid.Empty, declaration.JE_OH_Importer);
			AssertEquals("No error", false, notifications.HasErrors());
			AssertEquals("No warning", true, notifications.HasWarnings());
			AssertEquals("Contains a warning for no organisation found", true, notifications.ContainsNotificationContaining("There is no organization found with this number, "));
		}

		public void TestSetOrganisationWhenThereAreMultipleOrganisations()
		{
			var organisation = CreateOrganisationAddress(OrgCusCode.USACodeTypes.ManufacturerID, "123456789XY");
			var organisation2 = CreateOrganisationAddress(OrgCusCode.USACodeTypes.ManufacturerID, "123456789XY");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();

			var notifications = new NotificationCollection();

			declaration.JE_OH_Importer = BIRDOrganisationMatching.GetOrganisation(Factory, OrgMatchedCustomsRegNoType.MID, "123456789XY", "Importer", notifications).PK;

			AssertEquals("Importer could be set even if there are more than one matching organisations. Set to first active organization.", organisation.OA_OH, declaration.JE_OH_Importer);
			AssertEquals("No error", false, notifications.HasErrors());
			AssertEquals("But warning", true, notifications.HasWarnings());
			AssertEquals("Contains a warning for multiple organisations found", true, notifications.ContainsNotificationContaining("There is more than one organization found with this number, "));
		}

		public void TestGetOrganisationAddressForMultipleOrgs()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var notifications = new NotificationCollection();
			var result = BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "PHEVEAPP80SAB", ZString.Empty, notifications);
			AssertEquals("Only one org found for this Customs Code", orgAddress1, result);
			AssertEquals("No notifications provided", 0, notifications.Count);

			orgAddress1.Header.OH_IsActive = false;
			notifications = new NotificationCollection();
			result = BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "PHEVEAPP80SAB", ZString.Empty, notifications);
			AssertNull("Existing organization is inactive, so should return empty guid", result);

			orgAddress1.Header.OH_IsActive = true;
			orgAddress1.OA_IsActive = false;
			notifications = new NotificationCollection();
			result = BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "PHEVEAPP80SAB", ZString.Empty, notifications);
			AssertNull("Existing organization is active, but address is inactive, so should return empty guid", result);

			orgAddress1.OA_IsActive = true;
			var orgAddress2 = CreateOrganisationAddress(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB");
			notifications = new NotificationCollection();
			result = BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "PHEVEAPP80SAB", ZString.Empty, notifications);
			AssertEquals("Two orgs exist with the same Customs Code, first should be returned if active", orgAddress1, result);
			AssertEquals("Warning shown", ": There is more than one organization found with this number, PHEVEAPP80SAB. System will match first active organization, please review data.", notifications[0].Message);

			orgAddress1.Header.OH_IsActive = false;
			notifications = new NotificationCollection();
			result = BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "PHEVEAPP80SAB", ZString.Empty, notifications);
			AssertEquals("Two orgs exist with the same Customs Code, first is inactive, so second should be returned if active", orgAddress2, result);

			orgAddress2.Header.OH_IsActive = false;
			notifications = new NotificationCollection();
			result = BIRDOrganisationMatching.GetOrganisationAddress(Factory, OrgMatchedCustomsRegNoType.MID, "PHEVEAPP80SAB", ZString.Empty, notifications);
			AssertNull("Two orgs exist with the same Customs Code, both orgs inactive, empty guid returned", result);
		}

		public void TestGetOrganisationWithMIDNoAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			var cusCode = orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			cusCode.PremisesAddress.Delete();
			AssertNull(cusCode.PremisesAddress);

			var notifications = new NotificationCollection();

			BusinessObject result = null;
			AssertNoExceptionThrown(() => BIRDOrganisationMatching.GetOrganisation(Factory, OrgMatchedCustomsRegNoType.MID, "PHEVEAPP80SAB", ZString.Empty, notifications));
			AssertNull(result);
		}

		public void TestGetCustomsNumberTypeEntityIdentifierQualifier()
		{
			AssertEquals(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier("EI"));
			AssertEquals(OrgCusCode.USACodeTypes.CBPAssignedNumber, BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier("ANI"));
			AssertEquals(OrgCusCode.USACodeTypes.SocialSecurityNumber, BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier("34"));
			AssertEquals(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier("CIN"));
			AssertEquals(OrgCusCode.USACodeTypes.FIRMSCode, BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier("FR"));
			AssertEquals(OrgCusCode.USACodeTypes.ManufacturerID, BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier("MID"));
			AssertEquals(ZString.Empty, BIRDOrganisationMatching.GetCustomsNumberTypeEntityIdentifierQualifier("ABC"));
		}

		public void TestFindMatchedOrgAddressPKWithCustomsNumber()
		{
			var notifications = new NotificationCollection();
			var address1 = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", "", "", "", "", "", "", "", "", "", notifications);
			AssertNotNull(address1);

			notifications.Clear();
			var address2 = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", "", "CUSNUM", "", "", "", "", "", "", "", notifications);
			AssertNotNull(address2);
			AssertNotEquals(address1, address2);

			notifications.Clear();
			var address3 = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", "", "CUSNUM", "", "", "", "", "", "", "", notifications);
			AssertNotNull(address3);
			AssertNotEquals(address1, address3);
			AssertEquals(address2, address3);
		}

		public void TestFindMatchedOrgAddressPK()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var notifications = new NotificationCollection();
			var result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", "", "", "", "", "", "", "", notifications);
			AssertEquals("Matched address found", orgAddress1, result);

			notifications.Clear();
			notifications = new NotificationCollection();
			result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", OrgCusCode.USACodeTypes.ManufacturerID, "  PHEVEAPP80SAB", "", "", "", "", "", "", "", notifications);
			AssertEquals("No matched address found and no address is created because MID has invalid characters", null, result);
			Assert(notifications.GetWarnings().Any(x => x.Message.Contains(string.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, "  PHEVEAPP80SAB"))));

			notifications.Clear();
			var orgAddress2 = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SBC", "", "", "", "", "", "", "", notifications);
			AssertNotEquals("New address should be created", orgAddress1, orgAddress2);
			Assert(notifications.GetWarnings().Any(x => x.Message.Contains(string.Format(OrganisationCreator.ManufacturerCreated, "PHEVEAPP80SBC"))));

			notifications.Clear();
			result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", OrgCusCode.USACodeTypes.ManufacturerID, "", "", "", "", "", "", "", "", notifications);
			AssertNull("No new address should be created", result);

			notifications.Clear();
			result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY", "", "", "", "", "", "", "", notifications);
			AssertEquals("Main address should be matched", orgHeader.MainAddress, result);

			notifications.Clear();
			var orgAddress3 = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789YZ", "", "", "", "", "", "", "ABC", notifications);
			AssertNotNull("New address should be created", orgAddress3);
			AssertNotEquals("New address should be created", orgAddress1, orgAddress3);
			AssertNotEquals("New address should be created", orgAddress2, orgAddress3);
			Assert(notifications.GetWarnings().Any(x => x.Message.Contains("There is no organization matching this EIN, 12-3456789YZ when import ABC data.")));

			notifications.Clear();
			var orgAddress4 = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", "", "", "TESTORGA", "", "", "", "", "", "", notifications);
			AssertNotNull("New address should be created", orgAddress4);
			AssertNotEquals("New address should be created", orgAddress1, orgAddress4);
			AssertNotEquals("New address should be created", orgAddress2, orgAddress4);
			AssertNotEquals("New address should be created", orgAddress3, orgAddress4);

			notifications.Clear();
			var orgAddress5 = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "", "TESTORGA", "", "", "", "", "", "", notifications);
			AssertNotNull("New address should be created", orgAddress4);
			AssertNotEquals("New address should be created", orgAddress1, orgAddress5);
			AssertNotEquals("New address should be created", orgAddress2, orgAddress5);
			AssertNotEquals("New address should be created", orgAddress3, orgAddress5);
			AssertNotEquals("New address should be created", orgAddress4, orgAddress5);
		}

		public void TestFindMatchedOrgAddress_CreateNewOrg_Unloco()
		{
			var notifications = new NotificationCollection();
			var result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "ORG", ZString.Empty, ZString.Empty, "IAN TEST ORG", "TEST ADDRESS 1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "PGA", notifications);
			var header = result.Header;
			AssertEquals("IA", header.OH_RL_NKClosestPort);
			result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "ORG", ZString.Empty, ZString.Empty, "IAN TEST ORG 2", "TEST ADDRESS 1", ZString.Empty, "AU", ZString.Empty, ZString.Empty, "PGA", notifications);
			header = result.Header;
			AssertEquals("AU", header.OH_RL_NKClosestPort);
		}

		public void TestFindMatchedOrganizationByCompanyNameAndAddress()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_FullName = "IAN TEST ORG";
			orgHeader1.MainAddress.OA_Address1 = "TEST ADDRESS 1";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "IAN TEST ORG";
			orgHeader2.MainAddress.OA_Address1 = "TEST ADDRESS 1";
			Factory.Save();

			var addressCollection = new OrgAddressCollection(Factory);
			addressCollection.Add(orgHeader1.MainAddress);
			addressCollection.Add(orgHeader2.MainAddress);

			var warningMessage = "ORG: There is more than one organization address found with company name [IAN TEST ORG] and address [TEST ADDRESS 1] when import PGA data. System will match first active organization address, please review data.";
			var notifications = new NotificationCollection();
			var result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "ORG", ZString.Empty, ZString.Empty, "IAN TEST ORG", "TEST ADDRESS 1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "PGA", notifications);
			AssertEquals(true, notifications.ContainsNotificationContaining(warningMessage));
			Assert(addressCollection.Contains(result));

			orgHeader1.OH_IsActive = false;
			Factory.Save();

			notifications = new NotificationCollection();
			result = BIRDOrganisationMatching.FindMatchedOrgAddress(Factory, "ORG", ZString.Empty, ZString.Empty, "IAN TEST ORG", "TEST ADDRESS 1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "PGA", notifications);
			AssertEquals(false, notifications.ContainsNotificationContaining(warningMessage));
			AssertEquals(orgHeader2.MainAddress, result);
		}

		OrgHeader CreateOrganisation(ZString customsRegNoType, ZString number)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();

			result.CustomsCodes.AddNew(customsRegNoType, number, GlbCompany.CurrentCompany.Country);

			return result;
		}

		OrgAddress CreateOrganisationAddress(ZString customsRegNoType, ZString number)
		{
			var result = Factory.NewWithValidTestData<OrgHeader>();

			result.MainAddress.CustomsCodes.AddNew(customsRegNoType, number, GlbCompany.CurrentCompany.Country);

			return result.MainAddress;
		}
	}
}
