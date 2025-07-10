using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestPremisesAddresses

		public void TestPremisesAddresses_OnlyContainsActiveAddresses_WhenCusCodeDoesNotHaveAnAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_IsActive = true;

			var activeAddress = org.Addresses.AddNew();
			activeAddress.OA_IsActive = true;
			var inactiveAddress = org.Addresses.AddNew();
			inactiveAddress.OA_IsActive = false;

			AssertEquals("Pre-Condition: Number of all addresses", 3, org.Addresses.Count);
			AssertEquals("Pre-Condition: Addresses List contains inactive address set above", 1, org.Addresses.Cast<OrgAddress>().Count(o => !o.OA_IsActive));

			var orgCusCode = org.CustomsCodes.AddNew();
			AssertEquals("Pre-Condition: Custom Code does not have an address", true, orgCusCode.OK_OA_PremisesAddress.IsEmpty);

			var lookups = new OrgCusCodeLookups(orgCusCode);
			var result = lookups.PremisesAddresses;

			AssertEquals("Inactive address has been removed from Premises Addresses List", 0, result.Cast<OrgAddress>().Count(o => !o.OA_IsActive));
			AssertEquals("Active addresses count", 2, result.Cast<OrgAddress>().Count(o => o.OA_IsActive));
		}

		public void TestPremisesAddresses_ContainsActiveAndCusCodeAddresses_EventhoughCusCodeHasInactiveAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_IsActive = true;

			var activeAddress = org.Addresses.AddNew();
			activeAddress.OA_IsActive = true;
			var inactiveAddress = org.Addresses.AddNew();
			inactiveAddress.OA_IsActive = false;

			AssertEquals("Pre-Condition: Number of all addresses", 3, org.Addresses.Count);
			AssertEquals("Pre-Condition: Addresses List contains inactive address set above", 1, org.Addresses.Cast<OrgAddress>().Count(o => !o.OA_IsActive));

			var orgCusCode = org.CustomsCodes.AddNew();
			orgCusCode.OK_OA_PremisesAddress = inactiveAddress.PK;

			AssertEquals("Pre-Condition: Addresses List contains inactive address and contains custom code's address", 1, org.Addresses.Cast<OrgAddress>().Count(o => !o.OA_IsActive && o.PK == orgCusCode.OK_OA_PremisesAddress));

			var lookups = new OrgCusCodeLookups(orgCusCode);
			var result = lookups.PremisesAddresses;

			AssertEquals("Inactive address has not been removed from Premises Addresses List", 1, result.Cast<OrgAddress>().Count(o => !o.OA_IsActive));
			AssertEquals("Active addresses count", 2, result.Cast<OrgAddress>().Count(o => o.OA_IsActive));
		}

		#endregion

		[TestDate(2021, 11, 18)]
		public void TestNZCSupplier_List()
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			var supplierList = cusCode.Lookups.NZCSupplier_List;
			AssertEquals("Should get the supplier from ZZ Ref Database.", "ZZRefCusCodeListCombinedCollection", supplierList.GetType().Name);
		}

		public void TestSGPartyStatusTypeList()
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			var statusTypeList = cusCode.Lookups.SGPartyStatusTypeList;
			AssertEquals("statusTypeList.GetType()", "Enterprise.Customs.SG.Access.Business.SGPartyStatusList", statusTypeList.GetType().FullName);
		}

		public void TestSGDirectDeliveryList()
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			var dirList = cusCode.Lookups.SGDirectDeliveryList;
			AssertEquals("dirList[0]", "", dirList[0].Code);
			AssertEquals("dirList[1]", "Y", dirList[1].Code);
			AssertEquals("SGDirectDeliveryList should contain 2 elements", 2, dirList.Count);
		}

		public void TestNMFCParticipantList()
		{
			var cusCode = Factory.New<OrgCusCode>();
			var list1 = cusCode.Lookups.NMFCParticipantList;
			var cusCode2 = Factory.New<OrgCusCode>();
			var list2 = cusCode2.Lookups.NMFCParticipantList;
			AssertEquals("Should be cached", list1, list2);
			AssertEquals("NMFCParticipantList should contain 2 elements", 2, list1.Count);
			AssertEquals(OrgConstants.NMFCParticipantCodes.Description.Yes, list1.GetDescriptionFromCode(OrgConstants.NMFCParticipantCodes.Code.Yes));
			AssertEquals(OrgConstants.NMFCParticipantCodes.Description.No, list1.GetDescriptionFromCode(OrgConstants.NMFCParticipantCodes.Code.No));
		}

		public void TestCommunicationAgreementID()
		{
			OrgCusCode orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			Assert(!orgCusCode.Lookups.OK_CodeType_List.ContainsCode(OrgCusCode.CodeTypes.AgentCode));
			orgCusCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			Assert(orgCusCode.Lookups.OK_CodeType_List.ContainsCode(OrgCusCode.CodeTypes.AgentCode));
		}

		public void TestOrgPremiseGateCode_List()
		{
			OrgCusCode orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = "ZZZ";
			RefPremisesGateCodeCollection refPremise = orgCusCode.Lookups.OrgPremiseGateCode_List;
			Assert("List should be emplty", refPremise.Count == 0);
		}

		public void TestCustomsOfficeOfExitList()
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			var customsOfficeOfExitList = cusCode.Lookups.CustomsOfficeOfExitList;
			AssertEquals("Should get the supplier from ZZ Ref Database.", "ZZRefCusCodeListCombinedCollection", customsOfficeOfExitList.GetType().Name);
		}

		[ExpectNoExceptions]
		public void TestPremisesAddresses()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode orgCusCode = org.CustomsCodes.AddNew();
			OrgCusCodeLookups lookups = new OrgCusCodeLookups(orgCusCode);
			orgCusCode.Delete();

			OrgAddressDependentCollection collection;
			collection = lookups.PremisesAddresses;
		}

		public void TestOK_CodeType_List()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			var codeTypeList = orgCusCode.Lookups.OK_CodeType_List;
			var cacheKey = "OrgCusCodeLookups.OK_CodeType_List.AU." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertSame("OK_CodeType_List cached", codeTypeList, Factory.GetCachedValue<CodeDescriptionPairList>(cacheKey, () => null));

			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			AssertNotSame("OK_CodeType_List cache refreshed by OK_RN_NKCodeCountry", codeTypeList, orgCusCode.Lookups.OK_CodeType_List);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				AssertNotSame("OK_CodeType_List cache refreshed by CurrentCompany", codeTypeList, orgCusCode.Lookups.OK_CodeType_List);
			}
		}
	}
}
