using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgOpportunityValidationTest : BusinessObjectValidationTestCase
	{
		public void TestP8_OC()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1";
			for (int i = 2; i <= 10; ++i)
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "Contact" + i;
			}

			opportunity.P8_OC = contact1.PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var opportunityReload = factory2.Load<OrgOpportunity>(opportunity.PK);
			opportunityReload.Validation.ValidateP8_OC();
			AssertEquals("one contact loaded", 1, ((IBusinessObjectFactoryInternals)factory2).AllBusinessObjects.OfType<OrgContact>().Count());
		}

		public void TestEstimatedValue()
		{
			var org = Factory.New<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();

			opp.P8_EstimatedValue = 999999999999999.999m;
			AssertHasErrorContaining(opp.P8_EstimatedValueInfo, "The value you have entered here is too large. Please enter a value less than 100,000,000,000,000.");
		}

		public void TestDescription()
		{
			CodeDescriptionBoolCollection registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			CodeDescriptionBool registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_OpportunityDescription);
			registryItem.Bool = true;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();

			opp.P8_OpportunityDescription = "hello";
			AssertNoErrors(opp.P8_OpportunityDescriptionInfo);

			opp.P8_OpportunityDescription = "";
			AssertHasErrors(opp.P8_OpportunityDescriptionInfo);

			opp.P8_OpportunityDescription = "hello";
			AssertNoErrors(opp.P8_OpportunityDescriptionInfo);

			registryItem.Bool = false;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList);

			opp.P8_OpportunityDescription = "";
			AssertNoErrors("Registry says it's not mandatory", opp.P8_OpportunityDescriptionInfo);
		}

		public void TestExtraCategory()
		{
			var newList = new CodeDescriptionBoolCollection();
			newList.Add("ZUB", (NoResString)"ZUBIN");
			newList.Add("SAM", (NoResString)"SAMUEL", false);
			OrganisationsDataRegistry.Instance.ProductTypeList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newList);

			CodeDescriptionBoolCollection registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			CodeDescriptionBool registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_PackageType);
			registryItem.Bool = true;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();

			opportunity.P8_PackageType = "ZUB";
			AssertNoErrors(opportunity.P8_PackageTypeInfo);

			opportunity.P8_PackageType = "RAK";
			AssertHasErrors(opportunity.P8_PackageTypeInfo);

			opportunity.P8_PackageType = "";
			AssertHasErrors(opportunity.P8_PackageTypeInfo);

			registryItem.Bool = false;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);
			opportunity.P8_PackageType = "";
			AssertNoErrors(opportunity.P8_PackageTypeInfo);

			opportunity.P8_PackageType = "SAM";
			AssertHasErrors(opportunity.P8_PackageTypeInfo);

			Factory.Save();

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.Validation.ValidateP8_PackageType();
			AssertNoErrors(opportunity.P8_PackageTypeInfo);
			AssertHasWarnings(opportunity.P8_PackageTypeInfo);

			opportunity.P8_PackageType = "";
			opportunity.Factory.Save();

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.P8_PackageType = "SAM";
			AssertHasErrors(opportunity.P8_PackageTypeInfo);

			opportunity.Factory.Save();

			newList = new CodeDescriptionBoolCollection();
			newList.Add("ZUB", (NoResString)"ZUBIN");
			OrganisationsDataRegistry.Instance.ProductTypeList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newList);

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.Validation.ValidateP8_PackageType();
			AssertHasErrors(opportunity.P8_PackageTypeInfo);
		}

		[TestDate(2013, 11, 27)]
		public void TestClosedDateValidationBasedOnStatus()
		{
			var collection = new OpportunityStatusCollection();

			var noClosesItem = collection.AddNew();
			noClosesItem.Code = "ST1";
			noClosesItem.Description = (NoResString)"fdsfsdf";
			noClosesItem.Bool = false;

			var closesItem = collection.AddNew();
			closesItem.Code = "ST2";
			closesItem.Description = (NoResString)"fdsfsdf";
			closesItem.Bool = true;

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();

			AssertNoErrors(opp.P8_ClosedDateInfo);

			opp.P8_Status = "ST1";
			Assert(opp.P8_ClosedDate.IsEmpty);
			AssertNoErrors(opp.P8_ClosedDateInfo);

			opp.P8_Status = "ST2";
			AssertEquals(ZDateTime.UtcNow, opp.P8_ClosedDate);
			AssertNoErrors(opp.P8_ClosedDateInfo);

			opp.P8_ClosedDate = ZDateTime.Empty;
			AssertHasErrors(opp.P8_ClosedDateInfo);
		}

		public void TestAssignedOrgPKValidation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();

			AssertNoErrors(opp.AssignedOrgPKInfo);

			opp.AssignedOrgPK = org1.PK;
			AssertNoErrors(opp.AssignedOrgPKInfo);

			opp.AssignedOrgPK = ZGuid.NewZGuid();
			AssertHasErrors(opp.AssignedOrgPKInfo);
			AssertHasNotifications(opp.AssignedOrgPKInfo);
			AssertEquals("Enter a valid Assigned Organization.", opp.AssignedOrgPKInfo.Notifications.First().Message);
			AssertEquals("Assigned Organization", opp.AssignedOrgPKInfo.HumanReadableName);
		}

		public void TestEstimatedValueCurrency()
		{
			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();

			opportunity.P8_RX_NKEstimatedValueCurrency = "ZUB";
			AssertHasError(opportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Enter a valid Estimated Value Currency.");

			opportunity.P8_RX_NKEstimatedValueCurrency = "";
			AssertNoError(opportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Enter a valid Estimated Value Currency.");

			opportunity.P8_RX_NKEstimatedValueCurrency = "ZUB";
			opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";
			AssertNoError(opportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Enter a valid Estimated Value Currency.");
		}

		public void TestPrimarySalesPerson()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1 = org.SalesOpportunities.AddNew();
			var opportunity2 = org.SalesOpportunities.AddNew();

			var registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			var registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_GS_NKPrimarySalesPerson);

			registryItem.Bool = false;

			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunity1.P8_GS_NKPrimarySalesPerson = "ZUB";
				AssertHasError(opportunity1.P8_GS_NKPrimarySalesPersonInfo, "Enter a valid Opportunity Primary Sales Person.");

				opportunity2.P8_GS_NKPrimarySalesPerson = "";
				AssertNoErrors(opportunity2.P8_GS_NKPrimarySalesPersonInfo);

				opportunity1.P8_GS_NKPrimarySalesPerson = GlbStaff.CurrentUser.GS_Code;
				AssertNoErrors(opportunity1.P8_GS_NKPrimarySalesPersonInfo);
			}

			Factory.Save();

			registryItem.Bool = true;

			using (OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryList))
			{
				opportunity2.Validation.ValidateAll();
				AssertHasWarning(opportunity2.P8_GS_NKPrimarySalesPersonInfo, "You have not entered an Opportunity Primary Sales Person.");

				opportunity2.P8_GS_NKPrimarySalesPerson = GlbStaff.CurrentUser.GS_Code;
				AssertNoErrors(opportunity2.P8_GS_NKPrimarySalesPersonInfo);

				opportunity2.P8_GS_NKPrimarySalesPerson = "";
				AssertHasWarning(opportunity2.P8_GS_NKPrimarySalesPersonInfo, "You have not entered an Opportunity Primary Sales Person.");

				opportunity2.P8_GS_NKPrimarySalesPerson = GlbStaff.CurrentUser.GS_Code;
				AssertNoErrors(opportunity2.P8_GS_NKPrimarySalesPersonInfo);

				Factory.Save();

				opportunity2.P8_GS_NKPrimarySalesPerson = "";
				AssertHasError(opportunity2.P8_GS_NKPrimarySalesPersonInfo, "Please enter an Opportunity Primary Sales Person.");
			}
		}

		public void TestType()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("AAA", (NoResString)"Desc A", true);
			list.Add("BBB", (NoResString)"Desc B", true);
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			OrgOpportunity opportunity = Factory.NewWithValidTestData<OrgOpportunity>();

			opportunity.P8_OpportunityType = "ZUB";
			AssertHasErrors(opportunity.P8_OpportunityTypeInfo);

			opportunity.P8_OpportunityType = "AAA";
			AssertNoErrors(opportunity.P8_OpportunityTypeInfo);

			opportunity.P8_OpportunityType = "";
			AssertHasErrors(opportunity.P8_OpportunityTypeInfo);

			opportunity.P8_OpportunityType = "BBB";
			Factory.Save();

			list[1].Bool = false;
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.Validation.ValidateP8_OpportunityType();
			AssertNoErrors(opportunity.P8_OpportunityTypeInfo);
			AssertHasWarnings(opportunity.P8_OpportunityTypeInfo);
		}

		public void TestOutcome()
		{
			CodeDescriptionBoolCollection registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			CodeDescriptionBool registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_Outcome);
			registryItem.Bool = true;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);

			var opportunity = Factory.New<OrgOpportunity>();

			opportunity.P8_Outcome = "ZUB";
			AssertHasErrors(opportunity.P8_OutcomeInfo);

			opportunity.P8_Outcome = "";
			AssertHasErrors(opportunity.P8_OutcomeInfo);

			registryItem.Bool = false;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);
			opportunity.P8_Outcome = "";
			AssertNoErrors(opportunity.P8_OutcomeInfo);

			var list = new CodeDescriptionBoolCollection();
			list.Add("A1", (NoResString)"Outcome A1", true);
			list.Add("A2", (NoResString)"Outcome A2", true);
			OrganisationsDataRegistry.Instance.OpportunityOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var org = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			opportunity = org.SalesOpportunities.AddNew();

			opportunity.P8_Outcome = "B";
			AssertHasErrors(opportunity.P8_OutcomeInfo);

			opportunity.P8_Outcome = "A2";
			AssertNoErrors(opportunity.P8_OutcomeInfo);

			opportunity.P8_Outcome = "A1";
			opportunity.Factory.Save();

			list[0].Bool = false;
			OrganisationsDataRegistry.Instance.OpportunityOutcome.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.Validation.ValidateP8_Outcome();
			AssertNoErrors(opportunity.P8_OutcomeInfo);
			AssertHasWarnings(opportunity.P8_OutcomeInfo);
		}

		public void TestSource()
		{
			CodeDescriptionBoolCollection registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			CodeDescriptionBool registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_Source);
			registryItem.Bool = true;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);

			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();

			opportunity.P8_Source = "ZUB";
			AssertHasErrors(opportunity.P8_SourceInfo);

			opportunity.P8_Source = "";
			AssertHasErrors(opportunity.P8_SourceInfo);

			registryItem.Bool = false;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);

			opportunity.P8_Source = OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetCodeDescriptionPairList()[0].Code;
			AssertNoErrors(opportunity.P8_SourceInfo);

			opportunity.P8_Source = "";
			AssertNoErrors(opportunity.P8_SourceInfo);
		}

		public void TestReferringOrganisation()
		{
			var referringOrg = Factory.New<OrgHeader>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH_ReferringOrganisation = referringOrg.PK;
			AssertNoErrors("Existing org as referral is valid", opportunity.P8_OH_ReferringOrganisationInfo);

			opportunity.P8_OH_ReferringOrganisation = ZGuid.Invalid;
			AssertHasErrors("Invalid org as referral is invalid", opportunity.P8_OH_ReferringOrganisationInfo);
		}

		public void TestReferringContact()
		{
			var referringOrg = Factory.NewWithValidTestData<OrgHeader>();
			var activeContact = referringOrg.Contacts.AddNew();
			activeContact.OC_ContactName = "Jenny";
			var inactiveContact = referringOrg.Contacts.AddNew();
			inactiveContact.OC_ContactName = "Jenny's twin";
			inactiveContact.OC_IsActive = false;

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH_ReferringOrganisation = ZGuid.Empty;
			opportunity.P8_OC_ReferringContact = activeContact.PK;
			AssertHasErrors("Opportunity has no referring org, so cannot add referring contact. This scenario can happen via Operational Actions", opportunity.P8_OC_ReferringContactInfo);

			opportunity.P8_OH_ReferringOrganisation = ZGuid.Invalid;
			opportunity.P8_OC_ReferringContact = activeContact.PK;
			AssertHasErrors("Opportunity has invalid referring org, so cannot add referring contact", opportunity.P8_OC_ReferringContactInfo);

			opportunity.P8_OH_ReferringOrganisation = referringOrg.PK;
			opportunity.P8_OC_ReferringContact = activeContact.PK;
			AssertNoErrors("Opportunity has referring org, so can add referring contact", opportunity.P8_OC_ReferringContactInfo);

			opportunity.P8_OC_ReferringContact = inactiveContact.PK;
			AssertHasErrors("Opportunity has referring org, but referring contact is inactive", opportunity.P8_OC_ReferringContactInfo);

			#region Allow existing Opportunities and Inquiries to be modified with inactive referring contacts.  

			inactiveContact.OC_IsActive = true;
			Factory.Save();

			inactiveContact.OC_IsActive = false;
			Factory.Save();

			opportunity.RunPreSaveValidation();
			AssertHasWarning(opportunity.P8_OC_ReferringContactInfo, "Select an active contact for Referring Contact.");
			Assert(!opportunity.HasChanges);
			opportunity.P8_OpportunityDescription = "opportunity ABC-123";
			Assert(opportunity.HasChanges);
			Factory.Save();

			#endregion
		}

		public void TestStage()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("A1", (NoResString)"Stage A1", true);
			list.Add("A2", (NoResString)"Stage A2", true);
			OrganisationsDataRegistry.Instance.OpportunityStages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			OrgOpportunity opportunity = Factory.NewWithValidTestData<OrgOpportunity>();

			opportunity.P8_Stage = "ZUB";
			AssertHasErrors(opportunity.P8_StageInfo);

			opportunity.P8_Stage = "A2";
			AssertNoErrors(opportunity.P8_StageInfo);

			opportunity.P8_Stage = "";
			AssertHasErrors(opportunity.P8_StageInfo);

			opportunity.P8_Stage = "A1";
			Factory.Save();

			list[0].Bool = false;
			OrganisationsDataRegistry.Instance.OpportunityStages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.Validation.ValidateP8_Stage();
			AssertNoErrors(opportunity.P8_StageInfo);
			AssertHasWarnings(opportunity.P8_StageInfo);
		}

		public void TestStatus()
		{
			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();

			opportunity.P8_Status = "ZUB";
			AssertHasErrors(opportunity.P8_StatusInfo);

			opportunity.P8_Status = OrganisationsDataRegistry.Instance.OpportunityStatus.Value[0].Code;
			AssertNoErrors(opportunity.P8_StatusInfo);

			opportunity.P8_Status = "";
			AssertHasErrors(opportunity.P8_StatusInfo);
		}

		public void TestEnabledStatus()
		{
			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();

			opportunity.P8_Status = "TES";
			AssertHasErrors(opportunity.P8_StatusInfo);

			opportunity.P8_Status = OrganisationsDataRegistry.Instance.OpportunityStatus.Value[0].Code;
			AssertNoErrors(opportunity.P8_StatusInfo);

			opportunity.P8_Status = "";
			AssertHasErrors(opportunity.P8_StatusInfo);
		}

		public void TestCloseReason()
		{
			var statuses = new OpportunityStatusCollection();
			var statusWON = statuses.AddNew();
			statusWON.Code = "WON";
			statusWON.Description = (NoResString)"WON";
			statusWON.Bool = true;
			statusWON.Enabled = true;
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, statuses);

			var list = new OpportunityClosedReasonsCollection();
			list.Add("A1", (NoResString)"Reason A1").StatusRules.AddNew().Code = "WON";
			list.Add("A2", (NoResString)"Reason A2").StatusRules.AddNew().Code = "WON";
			OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_Status = "WON";

			opportunity.P8_LostReason = "B";
			AssertHasErrors(opportunity.P8_LostReasonInfo);

			opportunity.P8_LostReason = "A2";
			AssertNoErrors(opportunity.P8_LostReasonInfo);

			opportunity.P8_LostReason = "A1";
			Factory.Save();

			list[0].Bool = false;
			OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.Validation.ValidateP8_LostReason();
			AssertNoErrors(opportunity.P8_LostReasonInfo);
			AssertHasWarnings(opportunity.P8_LostReasonInfo);
		}

		public void TestCloseReason_NotAssociatedToStatus()
		{
			var statuses = new OpportunityStatusCollection();
			var statusWON = statuses.AddNew();
			statusWON.Code = "WON";
			statusWON.Description = (NoResString)"WON";
			statusWON.Bool = true;
			statusWON.Enabled = true;
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, statuses);

			var list = new OpportunityClosedReasonsCollection();
			list.Add("A1", (NoResString)"Reason A1").StatusRules.AddNew().Code = "WON";
			list.Add("A2", (NoResString)"Reason A2").StatusRules.AddNew().Code = "WON";
			OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_Status = "WON";

			opportunity.P8_LostReason = "B";
			AssertHasErrors(opportunity.P8_LostReasonInfo);

			opportunity.P8_LostReason = "A2";
			AssertNoErrors(opportunity.P8_LostReasonInfo);

			opportunity.P8_LostReason = "A1";
			Factory.Save();

			list[0].Bool = false;
			OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			opportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			opportunity.Validation.ValidateP8_LostReason();
			AssertNoErrors(opportunity.P8_LostReasonInfo);
			AssertHasWarning(opportunity.P8_LostReasonInfo, "The entered reason is not associated to the Opportunity Status.");
		}

		public void TestCheckP8_CloseCertainty()
		{
			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_CloseCertainty = 200;
			AssertHasError(opportunity.P8_CloseCertaintyInfo, "Invalid percentage for close certainty. Must be between 0 and 100.");

			opportunity.P8_CloseCertainty = 101;
			AssertHasError(opportunity.P8_CloseCertaintyInfo, "Invalid percentage for close certainty. Must be between 0 and 100.");

			opportunity.P8_CloseCertainty = 100;
			AssertNoError(opportunity.P8_CloseCertaintyInfo, "Invalid percentage for close certainty. Must be between 0 and 100.");

			opportunity.P8_CloseCertainty = 0;
			AssertNoError(opportunity.P8_CloseCertaintyInfo, "Invalid percentage for close certainty. Must be between 0 and 100.");

			opportunity.P8_CloseCertainty = 67;
			AssertNoError(opportunity.P8_CloseCertaintyInfo, "Invalid percentage for close certainty. Must be between 0 and 100.");
		}

		public void TestEstimatedValueCurrencyIsMandatory()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			OrgOpportunity orgOpportunity = orgHeader.SalesOpportunities.AddNew();

			orgOpportunity.P8_RX_NKEstimatedValueCurrency = "AUD";
			orgOpportunity.P8_EstimatedValue = 100;
			orgOpportunity.Validation.ValidateP8_RX_NKEstimatedValueCurrency();
			AssertNoErrors(orgOpportunity.P8_RX_NKEstimatedValueCurrencyInfo);

			orgOpportunity.P8_RX_NKEstimatedValueCurrency = "";
			AssertHasError(orgOpportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Please enter an Estimated Value Currency.");

			orgOpportunity.P8_EstimatedValue = ZDecimal.Zero;
			orgOpportunity.Validation.ValidateP8_RX_NKEstimatedValueCurrency();
			AssertNoError(orgOpportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Please enter an Estimated Value Currency.");

			CodeDescriptionBoolCollection registryList = OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.Value;
			CodeDescriptionBool registryItem = (CodeDescriptionBool)registryList.FindByCode(OrgOpportunitySchema.Constants.P8_RX_NKEstimatedValueCurrency);
			registryItem.Bool = false;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);

			orgOpportunity.P8_RX_NKEstimatedValueCurrency = "";
			orgOpportunity.P8_EstimatedValue = 100;
			orgOpportunity.Validation.ValidateP8_RX_NKEstimatedValueCurrency();
			AssertNoError(orgOpportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Please enter an Estimated Value Currency.");

			registryItem.Bool = true;
			OrganisationsDataRegistry.Instance.OpportunityManagementFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryList);

			orgOpportunity.P8_RX_NKEstimatedValueCurrency = "";
			orgOpportunity.Validation.ValidateP8_RX_NKEstimatedValueCurrency();
			AssertHasError(orgOpportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Please enter an Estimated Value Currency.");

			orgOpportunity.P8_EstimatedValue = ZDecimal.Zero;
			orgOpportunity.Validation.ValidateP8_RX_NKEstimatedValueCurrency();
			AssertNoError(orgOpportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Please enter an Estimated Value Currency.");

			orgOpportunity.P8_RX_NKEstimatedValueCurrency = "AUD";
			AssertNoError(orgOpportunity.P8_RX_NKEstimatedValueCurrencyInfo, "Please enter an Estimated Value Currency.");
		}

		public void TestP8_DiscountAmount()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();

			AssertNoErrors(opp.P8_DiscountAmountInfo);

			opp.P8_DiscountAmount = 999999999999;
			AssertNoErrors(opp.P8_DiscountAmountInfo);

			opp.P8_DiscountAmount = 9999999999999 + 1;
			AssertHasErrors(opp.P8_DiscountAmountInfo);
		}

		public void TestP8_RentalMultiplier()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();

			AssertNoErrors(opp.P8_RentalMultiplierInfo);
			opp.P8_RentalMultiplier = -1;
			AssertHasErrors(opp.P8_RentalMultiplierInfo);

			opp.P8_RentalMultiplier = 0;
			AssertNoErrors(opp.P8_RentalMultiplierInfo);

			opp.P8_RentalMultiplier = 999999999999;
			AssertNoErrors(opp.P8_RentalMultiplierInfo);

			opp.P8_RentalMultiplier = 9999999999999 + 1;
			AssertHasErrors(opp.P8_RentalMultiplierInfo);
		}

		public void TestNewSalesOpportunityValidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var salesOpportunity = header.SalesOpportunities.AddNew();
			salesOpportunity.P8_OpportunityDescription = string.Empty;

			salesOpportunity.Validation.ValidateAll();
			AssertHasErrors(salesOpportunity.P8_OpportunityDescriptionInfo);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OpportunityDescription = string.Empty;

			opportunity.Validation.ValidateAll();
			AssertHasErrors(opportunity.P8_OpportunityDescriptionInfo);
		}
	}
}
