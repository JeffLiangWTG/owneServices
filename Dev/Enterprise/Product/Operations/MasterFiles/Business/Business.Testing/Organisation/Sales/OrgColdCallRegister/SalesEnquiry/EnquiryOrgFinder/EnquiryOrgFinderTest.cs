using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EnquiryOrgFinder))]
	sealed class EnquiryOrgFinderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClientIntelligenceActionIsUnique()
		{
			var finder = new EnquiryOrgFinder(Factory, null, false, false);

			AssertEquals("Precondtion", false, finder.ShouldLinkToExistingClientIntelligence);
			AssertEquals("Precondtion", false, finder.ShouldCreateNewClientIntelligence);

			finder.ShouldLinkToExistingClientIntelligence = true;
			AssertEquals(true, finder.ShouldLinkToExistingClientIntelligence);
			AssertEquals(false, finder.ShouldCreateNewClientIntelligence);

			finder.ShouldCreateNewClientIntelligence = true;
			AssertEquals(false, finder.ShouldLinkToExistingClientIntelligence);
			AssertEquals(true, finder.ShouldCreateNewClientIntelligence);

			finder.ShouldCreateNewClientIntelligence = false;
			AssertEquals(false, finder.ShouldLinkToExistingClientIntelligence);
			AssertEquals(false, finder.ShouldCreateNewClientIntelligence);
		}

		public void TestSingleOrgAddresses()
		{
			var finder = new EnquiryOrgFinder(Factory, null, false, false);

			var org1 = Factory.New<OrgHeader>();
			org1.Addresses.AddNew();
			org1.Addresses.AddNew();
			finder.SingleOrgPk = org1.PK;
			AssertAllAddressesMatch(org1.Addresses, finder.SingleOrgAddresses);

			var org2 = Factory.New<OrgHeader>();
			finder.SingleOrgPk = org2.PK;
			AssertEquals(0, finder.SingleOrgAddresses.Count);

			finder.SingleOrgPk = ZGuid.Invalid;
			AssertEquals(0, finder.SingleOrgAddresses.Count);
		}

		public void TestSimilarOrgMatches()
		{
			var tempFactory = new BusinessObjectFactory();
			var org = tempFactory.NewWithValidTestData<OrgHeaderForEnquiryMatching>();
			org.OH_IsSalesLead = true;
			org.OH_FullName = "Test Organization";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_Address2 = "Address 2";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_PostCode = "2222";
			org.MainAddress.OA_State = "NSW";
			org.OH_RL_NKClosestPort = "AUBNE";

			var similarOrg1 = CreateSimilarOrg(org, Factory);
			var similarOrg2 = CreateSimilarOrg(org, Factory);
			Factory.Save();

			var finder = new EnquiryOrgFinder(tempFactory, org, false, false);
			AssertOrgPatternMatches(new OrgHeader[] { similarOrg1, similarOrg2 }, finder.SimilarOrgMatches);

			finder = new EnquiryOrgFinder(tempFactory, tempFactory.NewWithValidTestData<OrgHeaderForEnquiryMatching>(), false, false);
			AssertEquals(0, finder.SimilarOrgMatches.Count);
		}

		public void TestDefaultValues()
		{
			var orgForMatching = Factory.New<OrgHeaderForEnquiryMatching>();
			var finder = new EnquiryOrgFinder(Factory, orgForMatching, true, false);

			AssertEquals(false, finder.ShouldLinkToExistingClientIntelligence);
			AssertEquals(true, finder.ShouldLinkOrganizationAddressToInquiry);
			AssertEquals(false, finder.ShouldAddInquiryAddressToOrganization);

			AssertEquals(false, finder.ShouldCreateNewClientIntelligence);
			AssertEquals(false, finder.ShouldAllowWebAccess);
			AssertEquals(true, finder.ShouldNotAllowWebAccess);
		}

		#region Validation

		public void TestValidateShouldLinkOrganizationAddressToInquiry()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderForEnquiryMatching>();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Code = "TEST ADDRESS";
			orgAddress.OA_Address1 = "TEST ADDRESS 1";
			var finder = new EnquiryOrgFinder(Factory, org, false, false);

			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldLinkOrganizationAddressToInquiry = true;
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldLinkOrganizationAddressToInquiryInfo, "Please select a valid organization address to link to inquiry.");

			finder.SelectedAddressPk = ZGuid.NewZGuid();
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldLinkOrganizationAddressToInquiryInfo, "Please select a valid organization address to link to inquiry.");

			finder.SelectedAddressPk = orgAddress.PK;
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldLinkOrganizationAddressToInquiryInfo, "Please select a valid organization address to link to inquiry.");

			Factory.Save();

			finder.SelectedAddressPk = orgAddress.PK;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldLinkOrganizationAddressToInquiryInfo);
		}

		public void TestValidateShouldLinkOrganizationAddressToInquiry_NotLinking()
		{
			var org = Factory.New<OrgHeaderForEnquiryMatching>();
			var finder = new EnquiryOrgFinder(Factory, org, false, false);
			finder.SelectedAddressPk = ZGuid.Invalid;

			finder.ShouldLinkToExistingClientIntelligence = false;
			finder.ShouldLinkOrganizationAddressToInquiry = true;
			AssertNoErrors(finder.ShouldLinkOrganizationAddressToInquiryInfo);

			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldLinkOrganizationAddressToInquiry = false;
			AssertNoErrors(finder.ShouldLinkOrganizationAddressToInquiryInfo);
		}

		public void TestValidateSingleOrgPk()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderForEnquiryMatching>();
			org.MainAddress.OA_Address1 = "Address 1";
			var finder = new EnquiryOrgFinder(Factory, org, false, false);

			finder.ShouldLinkToExistingClientIntelligence = true;

			finder.SingleOrgPk = ZGuid.Invalid;
			finder.RunPreSaveValidation();
			AssertHasError(finder.SingleOrgPkInfo, "Enter a valid selection.");

			finder.SingleOrgPk = ZGuid.NewZGuid();
			finder.RunPreSaveValidation();
			AssertHasError(finder.SingleOrgPkInfo, "The entered organization has either been deleted or not yet added to the database.");

			finder.SingleOrgPk = org.PK;
			finder.RunPreSaveValidation();
			AssertHasError(finder.SingleOrgPkInfo, "The entered organization has either been deleted or not yet added to the database.");

			Factory.Save();

			finder.SingleOrgPk = org.PK;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.SingleOrgPkInfo);

			finder.SingleOrgPk = ZGuid.Empty;
			finder.SelectedAddressPk = org.MainAddress.PK;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.SingleOrgPkInfo);
		}

		public void TestValidateSingleOrgPk_NotLinking()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderForEnquiryMatching>();
			org.MainAddress.OA_Address1 = "Address 1";
			var finder = new EnquiryOrgFinder(Factory, org, false, false);

			finder.ShouldLinkToExistingClientIntelligence = false;

			finder.SingleOrgPk = ZGuid.Empty;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.SingleOrgPkInfo);

			finder.SingleOrgPk = ZGuid.Invalid;
			finder.RunPreSaveValidation();
			AssertHasError(finder.SingleOrgPkInfo, "Enter a valid selection.");

			finder.SingleOrgPk = org.PK;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.SingleOrgPkInfo);
		}

		public void TestValidateShouldAddInquiryAddressToOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderForEnquiryMatching>();
			org.MainAddress.OA_Address1 = "Address 1";
			var finder = new EnquiryOrgFinder(Factory, org, false, true);

			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldAddInquiryAddressToOrganization = true;
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldAddInquiryAddressToOrganizationInfo, "Please enter a valid organization or select a similarly matched organization to add inquiry address.");

			finder.SingleOrgPk = org.PK;
			finder.SelectedAddressPk = ZGuid.Empty;
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldAddInquiryAddressToOrganizationInfo, "Please enter a valid organization or select a similarly matched organization to add inquiry address.");

			finder.SingleOrgPk = ZGuid.Empty;
			finder.SelectedAddressPk = org.MainAddress.PK;
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldAddInquiryAddressToOrganizationInfo, "Please enter a valid organization or select a similarly matched organization to add inquiry address.");

			Factory.Save();

			finder.SingleOrgPk = org.PK;
			finder.SelectedAddressPk = ZGuid.Empty;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAddInquiryAddressToOrganizationInfo);

			finder.SingleOrgPk = ZGuid.Empty;
			finder.SelectedAddressPk = org.MainAddress.PK;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAddInquiryAddressToOrganizationInfo);
		}

		public void TestValidateShouldAddInquiryAddressToOrganization_NotAdding()
		{
			var org = Factory.New<OrgHeaderForEnquiryMatching>();
			var finder = new EnquiryOrgFinder(Factory, org, false, true);
			finder.SingleOrgPk = ZGuid.Invalid;

			finder.ShouldLinkToExistingClientIntelligence = false;
			finder.ShouldAddInquiryAddressToOrganization = true;
			AssertNoErrors(finder.ShouldAddInquiryAddressToOrganizationInfo);

			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldAddInquiryAddressToOrganization = false;
			AssertNoErrors(finder.ShouldAddInquiryAddressToOrganizationInfo);
		}

		public void TestValidateShouldAddInquiryAddressToOrganization_NoAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderForEnquiryMatching>();
			var finder = new EnquiryOrgFinder(Factory, org, false, false);

			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldAddInquiryAddressToOrganization = true;
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldAddInquiryAddressToOrganizationInfo, "Cannot add inquiry address to an organization because Address is empty for inquiry.");

			finder.ShouldLinkToExistingClientIntelligence = false;
			finder.ShouldAddInquiryAddressToOrganization = false;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAddInquiryAddressToOrganizationInfo);
		}

		public void TestValidateShouldAllowWebAccess()
		{
			var orgForMatching = Factory.New<OrgHeaderForEnquiryMatching>();

			var finder = new EnquiryOrgFinder(Factory, orgForMatching, false, false);
			finder.ShouldCreateNewClientIntelligence = true;

			finder.ShouldAllowWebAccess = false;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAllowWebAccessInfo);

			finder.ShouldAllowWebAccess = true;
			finder.RunPreSaveValidation();
			AssertHasError(finder.ShouldAllowWebAccessInfo, "Cannot approve web access because contact email is empty.");

			finder = new EnquiryOrgFinder(Factory, orgForMatching, true, false);
			finder.ShouldCreateNewClientIntelligence = true;

			finder.ShouldAllowWebAccess = false;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAllowWebAccessInfo);

			finder.ShouldAllowWebAccess = true;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAllowWebAccessInfo);
		}

		public void TestValidateShouldAllowWebAccess_NotCreatingNewClientIntelligence()
		{
			var org = Factory.New<OrgHeaderForEnquiryMatching>();

			var finder = new EnquiryOrgFinder(Factory, org, false, false);
			finder.ShouldCreateNewClientIntelligence = false;

			finder.ShouldAllowWebAccess = false;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAllowWebAccessInfo);

			finder.ShouldAllowWebAccess = true;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAllowWebAccessInfo);

			finder = new EnquiryOrgFinder(Factory, org, true, false);
			finder.ShouldCreateNewClientIntelligence = false;

			finder.ShouldAllowWebAccess = false;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAllowWebAccessInfo);

			finder.ShouldAllowWebAccess = true;
			finder.RunPreSaveValidation();
			AssertNoErrors(finder.ShouldAllowWebAccessInfo);
		}

		#endregion

		#region Implementation

		void AssertAllAddressesMatch(OrgAddressDependentCollection expectedAddresses, OrgAddressCollection actualAddressCollection)
		{
			CombineAssertions(() =>
			{
				AssertEquals("count", expectedAddresses.Count, actualAddressCollection.Count);
				foreach (var address in expectedAddresses)
				{
					Assert("contains", actualAddressCollection.Contains(address));
				}
			});
		}

		OrgHeader CreateSimilarOrg(OrgHeader org, BusinessObjectFactory factory)
		{
			var similarOrg = factory.NewWithValidTestData<OrgHeader>();
			similarOrg.OH_IsSalesLead = org.OH_IsSalesLead;
			similarOrg.OH_FullName = org.OH_FullName;
			similarOrg.MainAddress.OA_Address1 = org.MainAddress.OA_Address1;
			similarOrg.MainAddress.OA_Address2 = org.MainAddress.OA_Address2;
			similarOrg.MainAddress.OA_City = org.MainAddress.OA_City;
			similarOrg.MainAddress.OA_PostCode = org.MainAddress.OA_PostCode;
			similarOrg.MainAddress.OA_State = org.MainAddress.OA_State;
			similarOrg.OH_RL_NKClosestPort = org.OH_RL_NKClosestPort;

			return similarOrg;
		}

		void AssertOrgPatternMatches(IEnumerable<OrgHeader> expectedOrgsToBeMatched, OrgPatternMatchCollection actualMatchCollection)
		{
			CombineAssertions(() =>
				{
					AssertEquals("count", expectedOrgsToBeMatched.Count(), actualMatchCollection.Count);
					foreach (var org in expectedOrgsToBeMatched)
					{
						Assert("org is matched", actualMatchCollection.Cast<OrgPatternMatch>().Any(match => match.OS_OH == org.PK));
					}
				});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgForMatching = Factory.New<OrgHeaderForEnquiryMatching>();
			return new EnquiryOrgFinder(new BusinessObjectFactory(), orgForMatching, true, true);
		}

		#endregion
	}
}
