using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddressCapabilityWrapper))]
	sealed class OrgAddressCapabilityWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetMainSilentlyWhenBaseOA_RL_NKRelatedPortCodeIsEmpty()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var address = org.MainAddress;
			AssertEquals(true, address.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));

			var newMainAddress = org.Addresses.AddNew();
			newMainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			newMainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address.OA_RL_NKRelatedPortCode = string.Empty;

			CombineAssertions("Precondition: Main address has been switched and value of OA_RL_NKRelatedPortCode has been cleared", () =>
			{
				AssertEquals(false, address.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
				AssertEquals(true, newMainAddress.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
				AssertEquals(string.Empty, address.OA_RL_NKRelatedPortCode);
				AssertEquals(string.Empty, address.GetBaseOA_RL_NKRelatedPortCode());
			});

			address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			CombineAssertions("Value of OA_RL_NKRelatedPortCode has been set", () =>
			{
				AssertEquals(true, address.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
				AssertEquals("AUSYD", address.OA_RL_NKRelatedPortCode);
				AssertEquals("AUSYD", address.GetBaseOA_RL_NKRelatedPortCode());
			});
		}

		public void TestSetMainWhenModifyCapabilitiesMainNotAllowed()
		{
			var originalOrgAddressCapabilitiesMainValue = Env.Security.OrgAddressCapabilitiesMain.IsAllowed;
			try
			{
				Env.Security.OrgAddressCapabilitiesMain.IsAllowed = false;
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var address = organisation.MainAddress;
				var capability = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);
				AssertEquals(true, capability.Main_ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressCapabilitiesMain.IsAllowed = originalOrgAddressCapabilitiesMainValue;
			}
		}

		public void TestValidationMainPassesWhenMultipleMainOFCSolved()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = organisation.MainAddress;
			var capability1 = address1.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			capability1.Main = capability1.Enabled = true;

			var address2 = organisation.Addresses.AddNew();
			var capability2 = address2.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			capability2.Enabled = true;
			capability2.SetMain(true);

			AssertNoErrors(capability1.MainInfo);
			capability1.ValidateMain();
			AssertHasError(capability1.MainInfo, "Main address of this type already exists for this organization.");

			capability2.Main = false;

			capability1.MainInfo.ClearAllNotifications();
			AssertNoErrors(capability1.MainInfo);
			capability1.ValidateMain();
			AssertNoErrors(capability1.MainInfo);
		}

		public void TestProperty_ReadOnlyDoesntCauseNullReferences()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = organisation.MainAddress;
			var capability1 = address1.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			Factory.Save();
			organisation.Delete();

			Assert("When the org has been deleted the Enabled_ReadOnly should be true", capability1.Enabled_ReadOnly);
			Assert("When the org has been deleted the Main_ReadOnly should be true", capability1.Main_ReadOnly);
		}

		public void TestMultipleMainAddressesAllowedInDifferentCountries()
		{
			var organisation = Factory.New<OrgHeader>();
			var address1 = organisation.MainAddress;
			var capability1 = address1.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			AssertNoErrors("Precondition: No errors when adding one AU payable Address.", capability1.MainInfo);

			var address2 = AddMainAddress(organisation, OrgAddressType.Payables, "CH", Constants.Languages.English);
			var capability2 = address2.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			AssertNoErrors("capability1.MainInfo only address in AU therefore no errors", capability1.MainInfo);

			address2.RunPreSaveValidation();
			AssertNoErrors("capability2.MainInfo only address in CH therefore no errors", capability2.MainInfo);
		}

		public void TestOnlyOneMainAddressPerCountry()
		{
			var expectedError = "Main address of this type already exists for this organization.";
			var organisation = Factory.New<OrgHeader>();
			var address1 = organisation.MainAddress;
			var capability1 = address1.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			AssertNoErrors("Precondition: No errors when adding one AU payable Address.", capability1.MainInfo);

			var address2 = AddMainAddress(organisation, OrgAddressType.Payables, "CH", Constants.Languages.English);
			var capability2 = address2.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			AssertNoErrors("Precondition: No errors when adding one CH payable Address.", capability2.MainInfo);

			var address3 = AddMainAddress(organisation, OrgAddressType.Payables, "CH", Constants.Languages.English);
			var capability3 = address3.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			organisation.RunPreSaveValidation();
			AssertNoErrors("capability1.MainInfo only address in AU therefore no errors.", capability1.MainInfo);
			AssertHasError("capability2.MainInfo in error because capability3 is in same country", capability2.MainInfo, expectedError);
			AssertHasError("capability3.MainInfo in error because capability2 is in same country", capability3.MainInfo, expectedError);
		}

		public void TestMultipleMainAddressesNotAllowedInSameCountry_EvenIfDifferentLanguage()
		{
			var expectedWarning = "A different language Main address of this type already exists for this organization.";
			var organisation = Factory.New<OrgHeader>();
			var address1 = organisation.MainAddress;
			var capability1 = address1.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			AssertNoErrors("Precondition: No errors when adding one English AP Address.", capability1.MainInfo);

			var address2 = AddMainAddress(organisation, OrgAddressType.Payables, "CH", Constants.Languages.German);
			var capability2 = address2.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			AssertNoErrors("Precondition: capability2.MainInfo first main address in CH so no error expected.", capability2.MainInfo);

			var address3 = AddMainAddress(organisation, OrgAddressType.Payables, "CH", Constants.Languages.Italian);
			var capability3 = address3.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.Code);

			organisation.RunPreSaveValidation();
			AssertNoErrors(capability1.MainInfo);
			AssertNoErrors(capability2.MainInfo);
			AssertNoErrors(capability3.MainInfo);

			AssertHasWarning("Warning expected for capability2 address duplication with capability3.", capability2.MainInfo, expectedWarning);
			AssertHasWarning("Warning expected for capability3 address duplication with capability2.", capability3.MainInfo, expectedWarning);
		}

		static OrgAddress AddMainAddress(OrgHeader organisation, OrgAddressType addressType, string countryCode, string languageCode)
		{
			var address = organisation.Addresses.AddNew();
			address.OA_Language = languageCode;
			address.OA_RN_NKCountryCode = countryCode;
			address.OA_Address1 = addressType.ToString() + languageCode;
			address.AddressCapability.SetCapabilityEnabled(addressType.Code);
			address.AddressCapability.SetIsMainAddress(addressType.Code);

			return address;
		}

		public void TestMainAddressReadOnlySecurityMembers()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = header.MainAddress;
			AssertEquals(1, address.CapabilitiesCollection.Count);
			address.AddAddressType(OrgAddressType.Pickup);
			AssertEquals(2, address.CapabilitiesCollection.Count);
			OrgAddressCapabilityWrapper testCapability = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());

			bool oldvalue = Env.Security.OrgAddressCapabilitiesModify.IsAllowed;
			try
			{
				Env.Security.OrgAddressCapabilitiesNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = true;
				Assert("not in database and not allowed - read only", testCapability.EnabledInfo.ReadOnly);
				Assert("not in database and not allowed - read only", testCapability.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = false;
				Assert("not in database and allowed - not read only", !testCapability.EnabledInfo.ReadOnly);
				Assert("not in database and allowed - not read only", !testCapability.MainInfo.ReadOnly);

				Factory.Save();

				Env.Security.OrgAddressCapabilitiesNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = true;
				Assert("in database and allowed - not read only", !testCapability.EnabledInfo.ReadOnly);
				Assert("in database and allowed - not read only", !testCapability.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = false;
				Assert("in database and not allowed - read only", testCapability.EnabledInfo.ReadOnly);
				Assert("in database and not allowed - read only", testCapability.MainInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = oldvalue;
				Env.Security.OrgAddressCapabilitiesNew.IsAllowed = oldvalue;
			}
		}

		public void TestSecurityCapabilitiesARAPAndNonARAP()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = header.MainAddress;
			AssertEquals(1, address.CapabilitiesCollection.Count);
			address.AddAddressType(OrgAddressType.Pickup);
			AssertEquals(2, address.CapabilitiesCollection.Count);
			OrgAddressCapabilityWrapper testCapability1 = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Payables.ToString());
			OrgAddressCapabilityWrapper testCapability2 = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Receivables.ToString());
			OrgAddressCapabilityWrapper testCapability3 = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Miscellaneous.ToString());

			bool oldvalue = Env.Security.OrgAddressCapabilitiesModify.IsAllowed;
			try
			{
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = false;

				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = true;
				Assert(!testCapability1.EnabledInfo.ReadOnly);
				Assert(!testCapability1.MainInfo.ReadOnly);
				Assert(!testCapability2.EnabledInfo.ReadOnly);
				Assert(!testCapability2.MainInfo.ReadOnly);
				Assert(!testCapability3.EnabledInfo.ReadOnly);
				Assert(!testCapability3.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = true;
				Assert(testCapability1.EnabledInfo.ReadOnly);
				Assert(testCapability1.MainInfo.ReadOnly);
				Assert(testCapability2.EnabledInfo.ReadOnly);
				Assert(testCapability2.MainInfo.ReadOnly);
				Assert(!testCapability3.EnabledInfo.ReadOnly);
				Assert(!testCapability3.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = false;
				Assert(!testCapability1.EnabledInfo.ReadOnly);
				Assert(!testCapability1.MainInfo.ReadOnly);
				Assert(!testCapability2.EnabledInfo.ReadOnly);
				Assert(!testCapability2.MainInfo.ReadOnly);
				Assert(testCapability3.EnabledInfo.ReadOnly);
				Assert(testCapability3.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = false;
				Assert(testCapability1.EnabledInfo.ReadOnly);
				Assert(testCapability1.MainInfo.ReadOnly);
				Assert(testCapability2.EnabledInfo.ReadOnly);
				Assert(testCapability2.MainInfo.ReadOnly);
				Assert(testCapability3.EnabledInfo.ReadOnly);
				Assert(testCapability3.MainInfo.ReadOnly);

				Factory.Save();

				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = true;
				Assert(!testCapability1.EnabledInfo.ReadOnly);
				Assert(!testCapability1.MainInfo.ReadOnly);
				Assert(!testCapability2.EnabledInfo.ReadOnly);
				Assert(!testCapability2.MainInfo.ReadOnly);
				Assert(!testCapability3.EnabledInfo.ReadOnly);
				Assert(!testCapability3.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = true;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = false;
				Assert(!testCapability1.EnabledInfo.ReadOnly);
				Assert(!testCapability1.MainInfo.ReadOnly);
				Assert(!testCapability2.EnabledInfo.ReadOnly);
				Assert(!testCapability2.MainInfo.ReadOnly);
				Assert(testCapability3.EnabledInfo.ReadOnly);
				Assert(testCapability3.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = true;
				Assert(testCapability1.EnabledInfo.ReadOnly);
				Assert(testCapability1.MainInfo.ReadOnly);
				Assert(testCapability2.EnabledInfo.ReadOnly);
				Assert(testCapability2.MainInfo.ReadOnly);
				Assert(!testCapability3.EnabledInfo.ReadOnly);
				Assert(!testCapability3.MainInfo.ReadOnly);

				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = false;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = false;
				Assert(testCapability1.EnabledInfo.ReadOnly);
				Assert(testCapability1.MainInfo.ReadOnly);
				Assert(testCapability2.EnabledInfo.ReadOnly);
				Assert(testCapability2.MainInfo.ReadOnly);
				Assert(testCapability3.EnabledInfo.ReadOnly);
				Assert(testCapability3.MainInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = oldvalue;
				Env.Security.OrgAddressCapabilitiesARAP.IsAllowed = oldvalue;
				Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = oldvalue;
				Env.Security.OrgAddressCapabilitiesNew.IsAllowed = oldvalue;
				Env.Security.OrgAddressCapabilitiesARAPNew.IsAllowed = oldvalue;
				Env.Security.OrgAddressCapabilitiesNonARAPNew.IsAllowed = oldvalue;
			}
		}

		public void TestIfNotOnlyOneMainOfficeAddressModifyingMainIsAllowed()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress mainAddress1 = header.MainAddress;
			mainAddress1.OA_Address1 = "main address 1";
			OrgAddressCapabilityWrapper capability1 = mainAddress1.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			capability1.SetMain(true);
			capability1.Enabled = true;
			bool oldvalue = Env.Security.OrgAddressModify.IsAllowed;
			try
			{
				Factory.Save();

				Assert("in database but not allowed - read only", capability1.EnabledInfo.ReadOnly);
				Assert("in database but not allowed - read only", capability1.MainInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressModify.IsAllowed = oldvalue;
			}

			OrgAddress mainAddress2 = header.Addresses.AddNew();
			mainAddress2.OA_Address1 = "main address 2";
			OrgAddressCapability capability2 = Factory.New<OrgAddressCapability>();
			capability2.PZ_AddressType = OrgAddressType.Office.Code;
			capability2.PZ_IsMainAddress = true;
			capability2.PZ_OA = mainAddress2.PK;

			oldvalue = Env.Security.OrgAddressModify.IsAllowed;
			try
			{
				Factory.Save();

				OrgAddressCapabilityWrapper wrapper2 = mainAddress2.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);

				Assert("allowed - two main address in database", !capability1.EnabledInfo.ReadOnly);
				Assert("allowed - two main address in database", !capability1.MainInfo.ReadOnly);
				Assert("allowed - two main address in database", !wrapper2.EnabledInfo.ReadOnly);
				Assert("allowed - two main address in database", !wrapper2.MainInfo.ReadOnly);

				wrapper2.Main = false;

				Assert("not allowed", capability1.EnabledInfo.ReadOnly);
				Assert("not allowed", capability1.MainInfo.ReadOnly);
				Assert("allowed - not main", !wrapper2.EnabledInfo.ReadOnly);
				Assert("allowed - not main", !wrapper2.MainInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressModify.IsAllowed = oldvalue;
			}
		}

		public void TestMainAddressReadOnlySecurityMembersAllowed()
		{
			var oldvalue = Env.Security.OrgAddressModify.IsAllowed;
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			address.AddAddressType(OrgAddressType.Pickup);

			Env.Security.OrgAddressModify.IsAllowed = true;
			Factory.Save();
			var testCapability = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			try
			{
				Assert("in database and allowed - not read only", !testCapability.EnabledInfo.ReadOnly);
				Assert("in database and allowed - not read only", !testCapability.MainInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgAddressModify.IsAllowed = oldvalue;
			}
		}

		public void TestEnabled()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.Addresses.MainAddress;
			var testCapabilityPickup = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			var testCapabilityOffice = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.ToString());
			Assert("should be enabled for office", testCapabilityOffice.Enabled);
			Assert("should not be enabled for pickup", !testCapabilityPickup.Enabled);

			AssertNotNull("Capability should not be null for office", testCapabilityOffice.Capability);
			AssertNull("Capability should be null for pickup", testCapabilityPickup.Capability);

			address.AddAddressType(OrgAddressType.Pickup);
			testCapabilityPickup = address.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			Assert("should be enabled for pickup", testCapabilityPickup.Enabled);
			AssertNotNull("Capability should not be null for Pickup", testCapabilityPickup.Capability);
		}

		public void TestIsDefaultOfficeAddress()
		{
			var header = Factory.New<OrgHeader>();
			var address = header.Addresses.MainAddress;
			var testCapability = header.MainAddress.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			Assert("should be office default", testCapability.IsDefaultOfficeAddress);

			header.Addresses[0].AddressCapability.SetIsNotMainAddress(OrgAddressType.Office.Code);
			Assert("still should be office default as MainAddress is readonly", testCapability.IsDefaultOfficeAddress);

			testCapability.AddressCapabilityType = OrgAddressType.Miscellaneous.Code;
			Assert("should not be office default", !testCapability.IsDefaultOfficeAddress);
		}

		public void TestCodeDescription()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddressCapabilityWrapper testCapability = header.MainAddress.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			AssertEquals("AddressTypeDescription should be Office Address", OrgCodeLists.AddressType_List(Factory).GetDescriptionFromCode(OrgAddressType.Office.Code), testCapability.AddressTypeDescription);
		}

		public void TestIsMainAddressReadonlyInfo()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			header.Addresses[0].AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			OrgAddressCapabilityWrapper testCapability = header.Addresses[0].AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);

			Assert("should be readonly", testCapability.MainInfo.ReadOnly);

			testCapability.AddressCapabilityType = OrgAddressType.Miscellaneous.Code;
			testCapability.Main = ZBool.False;
			Assert("should not be readonly", !testCapability.MainInfo.ReadOnly);
		}

		public void TestSetMainAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			header.Addresses[0].AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			OrgAddressCapabilityWrapper testCapability = header.Addresses[0].AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			Assert("should be office default", testCapability.IsDefaultOfficeAddress);

			testCapability.Main = ZBool.False;
			Assert("still should be office default as MainAddress is readonly", testCapability.IsDefaultOfficeAddress);
		}

		public void TestValidation()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsGlobalAccount = ZBool.False;
			OrgAddress address = header.MainAddress;
			OrgAddress address2 = header.Addresses.AddNew();

			address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);
			OrgAddressCapabilityWrapper oAC = address2.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Payables);
			Assert("This type of address is ticked", oAC.Enabled);
			Assert("This type of address is ticked", !oAC.MainInfo.HasErrors());

			OrgAddress address3 = header.Addresses.AddNew();
			address3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			OrgAddressCapabilityWrapper oAC3 = address3.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Payables);
			Assert("This type of address can be ticked", !oAC3.MainInfo.HasErrors());
			address3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);
			Assert("Main address of this type already exists for this organisation", oAC3.MainInfo.HasErrors());

			OrgAddress addressWithEmptyCapabilities = header.Addresses.AddNew();
			addressWithEmptyCapabilities.AddressCapability.RunPreSaveValidation();
			OrgAddressCapabilityWrapper fortest = addressWithEmptyCapabilities.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.ToString());
			Assert("All capabilities should have errors cause nothing is enabled", fortest.EnabledInfo.HasErrors());

			addressWithEmptyCapabilities.AddressCapability.RunPreSaveValidation();
			fortest = addressWithEmptyCapabilities.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			Assert("All capabilities should have errors cause nothing is enabled", fortest.EnabledInfo.HasErrors());

			addressWithEmptyCapabilities.OA_IsActive = false;
			addressWithEmptyCapabilities.AddressCapability.RunPreSaveValidation();
			fortest = addressWithEmptyCapabilities.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Pickup.ToString());
			Assert("All capabilities should have no errors cause address is inactive", !fortest.EnabledInfo.HasErrors());

			addressWithEmptyCapabilities.OA_IsActive = true;
			addressWithEmptyCapabilities.AddAddressType(OrgAddressType.Miscellaneous);
			addressWithEmptyCapabilities.AddressCapability.RunPreSaveValidation();
			fortest = addressWithEmptyCapabilities.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Miscellaneous.ToString());
			Assert("All capabilities should have no errors cause MISC is enabled", !fortest.EnabledInfo.HasErrors());
		}

		public void TestRefreshValidationWhenEnablingCapability()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.Addresses.AddNew();

			address.RunPreSaveValidation();
			AssertCapabilityHasValidationError(address, true);

			address.AddressCapability[0].Enabled = true;
			AssertCapabilityHasValidationError(address, false);
		}

		void AssertCapabilityHasValidationError(OrgAddress address, bool hasValidationError)
		{
			foreach (var capability in address.AddressCapability)
			{
				AssertEquals(hasValidationError, capability.HasErrors);
				if (hasValidationError)
				{
					AssertEquals(1, capability.Notifications.Count());
					AssertEquals("Error - Enabled: All addresses should have at least one capability selected.", capability.Notifications.First().Message);
				}
			}
		}

		public void TestIsCustomsAddressWithoutSecurity()
		{
			OrgAddress parentAddress = Factory.New<OrgAddress>();
			var wrapper = new OrgAddressCapabilityWrapper(parentAddress);
			bool l;
			AssertNoExceptionThrown(() => l = wrapper.IsCustomsAddressWithoutSecurityOrHeaderIsNull);
		}

		public void TestValidationForCustomsAddress()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsGlobalAccount = ZBool.False;
			var address = header.MainAddress;
			var address2 = header.Addresses.AddNew();

			address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.CustomsAddressOfRecord);
			var capability = address2.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.CustomsAddressOfRecord);
			Assert("This type of address is ticked", capability.Enabled);
			Assert("No Errors", !capability.EnabledInfo.HasErrors());

			var address3 = header.Addresses.AddNew();
			address3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			capability = address3.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.CustomsAddressOfRecord);
			capability.RunPreSaveValidation();
			Assert("Another CustomsAddress selected - should be an error", capability.EnabledInfo.HasErrors());
			capability.Enabled = false;
			capability.RunPreSaveValidation();
			Assert("Only one Customs Address selected for organization - no errors", !capability.EnabledInfo.HasError("You have selected more than one Customs Address Of Record."));
		}

		public void TestValidationForGlobalAccount()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "ZX";

			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "ZXZZZ";
			uNLOCO.RL_RN_NKCountryCode = country.Code;

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();

			header.OH_FullName = "Test Org";
			header.OH_RL_NKClosestPort = "ZXZZZ";

			OrgAddress address = header.MainAddress;
			address.OA_RL_NKRelatedPortCode = "NZAKL";
			header.OH_IsGlobalAccount = ZBool.True;

			OrgAddress address2 = header.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "AUABG";
			address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

			OrgAddressCapabilityWrapper oAC = address2.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Payables);
			Assert("This type of address is ticked", oAC.Enabled);
			Assert("This type of address is ticked", !oAC.MainInfo.HasErrors());

			address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			oAC = address2.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Office);
			oAC.RunPreSaveValidation();
			Assert("This type of address is ticked", oAC.Enabled);
			Assert("This type of address is ticked as it has different port", !oAC.MainInfo.HasErrors());

			OrgAddress address3 = header.Addresses.AddNew();
			address3.OA_RL_NKRelatedPortCode = "AUABG";
			address3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			OrgAddressCapabilityWrapper oAC3 = address3.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Payables);
			Assert("This type of address can be ticked", !oAC3.MainInfo.HasErrors());
			address3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);
			Assert("Main address of this type already exists for this organisation", oAC3.MainInfo.HasErrors());
		}

		public void TestSetMainAddressForGlobal()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUBNE";
			header.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			header.Addresses[0].AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			OrgAddressCapabilityWrapper testCapability = header.Addresses[0].AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			Assert("should be office default", testCapability.IsDefaultOfficeAddress);

			testCapability.Main = ZBool.False;
			Assert("still should be office default as MainAddress is readonly", testCapability.IsDefaultOfficeAddress);

			header.OH_IsGlobalAccount = ZBool.True;
			OrgAddress anotherAddress = Factory.New<OrgAddress>();
			header.Addresses.Add(anotherAddress);
			anotherAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			anotherAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			Assert("still should be office default as global account and diferent countries", testCapability.IsDefaultOfficeAddress);
			testCapability = anotherAddress.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
			Assert("still should be office default as global account and diferent countries", testCapability.IsDefaultOfficeAddress);
		}

		public void TestValidateMainWhenLanguageChanged()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = organisation.MainAddress;
			address1.OA_Language = Constants.Languages.English;
			var capability1 = address1.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Receivables.Code);
			capability1.Main = capability1.Enabled = true;

			var address2 = organisation.Addresses.AddNew();
			address2.OA_Language = Constants.Languages.ChineseSimplified;
			var capability2 = address2.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Receivables.Code);
			capability2.Enabled = true;
			capability2.SetMain(true);

			capability2.ValidateMain();
			AssertNoErrors(capability2.MainInfo);

			address2.OA_Language = Constants.Languages.English;
			capability2.ValidateMain();
			AssertHasError(capability2.MainInfo, "Main address of this type already exists for this organization.");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress parentHeader = header.MainAddress;
			return new OrgAddressCapabilityWrapper(parentHeader);
		}

		#endregion
	}
}
