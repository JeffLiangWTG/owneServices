using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class OrganisationCreatorTest : OrganizationAddressTestHelper
	{
		#region TestGetNewOrgHeader

		public void TestGetNewOrgHeader()
		{
			var originalValue = RawDataRegistry.Instance.CanUserEditOrganisationCode.Value;
			RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			try
			{
				var orgAddressDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.CustomsWarehouseAddress);
				var org = OrganisationCreator.GetNewOrgHeader(orgAddressDataObject, Factory, Logger);

				// org
				AssertEquals("INTHEMSYD", org.OH_Code);
				AssertEquals("In The Moment", org.OH_FullName);
				AssertEquals("AU", org.OH_RL_NKClosestPort);

				// 1 contact
				var orgContact = (OrgContact)org.Contacts.Single();
				AssertEquals("Starshine Moonbeam", orgContact.OC_ContactName);
				AssertEquals("s.m@moment.com.au", orgContact.OC_Email);
				AssertEquals("234098234", orgContact.OC_Fax);
				AssertEquals("234098293", orgContact.OC_Mobile);
				AssertEquals("1239813209", orgContact.OC_Phone);

				// 4 cuscodes
				var cusCodes = org.CustomsCodes.Cast<OrgCusCode>();
				AssertEquals(4, cusCodes.Count());
				AssertEquals(1, (from o in cusCodes where o.OK_CodeType == "ATF" && o.OK_CustomsRegNo == "1234F" && o.OK_RN_NKCodeCountry == "NZ" && o.OK_OA_PremisesAddress == org.MainAddress.PK select o).Count());
				AssertEquals(1, (from o in cusCodes where o.OK_CodeType == "UNC" && o.OK_CustomsRegNo == "545" && o.OK_RN_NKCodeCountry == "AU" && o.OK_OA_PremisesAddress.IsEmpty select o).Count());
				AssertEquals(1, (from o in cusCodes where o.OK_CodeType == "UOC" && o.OK_CustomsRegNo == "454" && o.OK_RN_NKCodeCountry == "AU" && o.OK_OA_PremisesAddress.IsEmpty select o).Count());
				AssertEquals(1, (from o in cusCodes where o.OK_CodeType == "GST" && o.OK_CustomsRegNo == "55555" && o.OK_RN_NKCodeCountry == "AU" && o.OK_OA_PremisesAddress.IsEmpty select o).Count());

				// 1 address
				AssertAddressContentMatches_INTHEMSYD(org.MainAddress);

				// no errors
				AssertEquals(0, Logger.Logs.Length);

				// ensure we can actually save - catches any new DB constraints
				Factory.SaveForTesting();
			}
			finally
			{
				RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		#endregion

		#region TestGetNewOrgHeader_OrgCodeOverrideDisabled

		public void TestGetNewOrgHeader_OrgCodeOverrideDisabled()
		{
			var originalValue = RawDataRegistry.Instance.CanUserEditOrganisationCode.Value;
			RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			try
			{
				var orgAddressDataObject = GetNewAddressData_INTHEMSYD(DocAddressType.CustomsWarehouseAddress);
				var org = OrganisationCreator.GetNewOrgHeader(orgAddressDataObject, Factory, Logger);
				AssertEquals("INMOME", org.OH_Code);
			}
			finally
			{
				RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		#endregion

		#region TestGetNewOrgHeader_BadAddressDataObject

		public void TestGetNewOrgHeader_BadAddressDataObject()
		{
			var orgAddressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressDataObject.AddressType = nameof(DocAddressType.CustomsWarehouseAddress);

			// no address lines
			AssertNull(OrganisationCreator.GetNewOrgHeader(orgAddressDataObject, Factory, Logger));
			AssertMultilineASCIIEquals("", @"
Error - <Address1> is missing - Organization could not be created for <OrganizationAddress> with <AddressType> [CustomsWarehouseAddress].
Error - <Address2> is missing - Organization could not be created for <OrganizationAddress> with <AddressType> [CustomsWarehouseAddress].".Trim(), Logger.Logs);
			Logger.ClearLogs(); // cleanup

			var originalValue = RawDataRegistry.Instance.CanUserEditOrganisationCode.Value;
			try
			{
				// no org code & unable to generate
				RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				orgAddressDataObject.Address2 = "moo";
				AssertNull(OrganisationCreator.GetNewOrgHeader(orgAddressDataObject, Factory, Logger));
				AssertEquals("Error - <OrganizationCode> is missing and could not be generated - Organization could not be created for <OrganizationAddress> with <AddressType> [CustomsWarehouseAddress].", Logger.Logs);
				Logger.ClearLogs(); // cleanup

				// address line 2 only
				RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				orgAddressDataObject.OrganizationCode = "oink";
				var org = OrganisationCreator.GetNewOrgHeader(orgAddressDataObject, Factory, Logger);
				AssertEquals("moo", org.MainAddress.OA_Address1);
				AssertEquals("", org.MainAddress.OA_Address2);
				AssertEquals(0, Logger.Logs.Length);
			}
			finally
			{
				RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}

			// ensure we can actually save - catches any new DB constraints
			Factory.SaveForTesting();
		}

		#endregion
	}
}
