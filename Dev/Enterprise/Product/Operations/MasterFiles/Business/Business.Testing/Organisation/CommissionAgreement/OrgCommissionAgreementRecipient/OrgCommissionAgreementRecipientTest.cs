using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreementRecipient))]
	sealed class OrgCommissionAgreementRecipientTest : EnterpriseBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			AssertEquals(true, recipient.CAR_IsCommissionRateOverriden);
			AssertEquals((ZByte)1, recipient.CAR_Share);
		}

		#endregion

		#region Properties

		#region CAR_GS_NKStaff

		public void TestCAR_GS_NKStaff_SetDefaultsFromStaffCommissionRule()
		{
			var salesTeamAu = Factory.NewWithValidTestData<SalesTeam>();
			salesTeamAu.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));
			var salesTeamUs = Factory.NewWithValidTestData<SalesTeam>();
			salesTeamUs.CoveredCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US"));
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			salesTeamAu.Staff.Add(staff);
			salesTeamUs.Staff.Add(staff);

			var auRuleA = salesTeamAu.CommissionRules.AddNew();
			auRuleA.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			auRuleA.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			auRuleA.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			auRuleA.FillWithValidTestData();
			var auRuleARate1 = auRuleA.Rates.AddNew();
			auRuleARate1.ACT_CommissionType = CommissionTypes.Codes.FIX;
			auRuleARate1.ACT_CommissionAmount = 100d;
			auRuleARate1.ACT_RX_NKCommissionCurrency = "AUD";
			auRuleARate1.ACT_CommissionPeriod = "EXS";

			var auRuleB = salesTeamAu.CommissionRules.AddNew();
			auRuleB.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			auRuleB.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			auRuleB.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			auRuleB.FillWithValidTestData();
			var auRuleBRate1 = auRuleB.Rates.AddNew();
			auRuleBRate1.ACT_CommissionType = CommissionTypes.Codes.PCT;
			auRuleBRate1.ACT_CommissionPercentage = 5d;
			auRuleBRate1.ACT_CommissionPeriod = "NEW";

			var usRule = salesTeamUs.CommissionRules.AddNew();
			usRule.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			usRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			usRule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			usRule.FillWithValidTestData();
			var usRuleRate1 = usRule.Rates.AddNew();
			usRuleRate1.ACT_CommissionType = CommissionTypes.Codes.FIX;
			usRuleRate1.ACT_CommissionAmount = 50d;
			usRuleRate1.ACT_RX_NKCommissionCurrency = "USD";
			usRuleRate1.ACT_CommissionPeriod = "NEW";

			Factory.Save();

			var auOrg = Factory.New<OrgHeader>();
			auOrg.OH_RL_NKClosestPort = "AUSYD";
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = auOrg.PK;

			var agreement1 = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1,
				JobInvoicingConsumerTypes.Shipment.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var agreement1Recipient = agreement1.Recipients.AddNew();
			agreement1Recipient.CAR_GS_NKStaff = "ADL";

			CombineAssertions("Should have defaulted from auRuleB", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.PCT, agreement1Recipient.CAR_CommissionType);
				AssertEquals("Count", 1, agreement1Recipient.Rates.Count);
				if (agreement1Recipient.Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionAmount", (ZDecimal)0d, agreement1Recipient.Rates[0].CAT_CommissionAmount);
					AssertEquals("CAT_RX_NKCommissionCurrency", "", agreement1Recipient.Rates[0].CAT_RX_NKCommissionCurrency);
					AssertEquals("CAT_CommissionPercentage", (ZDecimal)5d, agreement1Recipient.Rates[0].CAT_CommissionPercentage);
					AssertEquals("CAT_CommissionPeriod", "NEW", agreement1Recipient.Rates[0].CAT_CommissionPeriod);
				}
			});

			var agreement2 = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement2,
				JobInvoicingConsumerTypes.Brokerage.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var agreement2Recipient = agreement2.Recipients.AddNew();
			agreement2Recipient.CAR_GS_NKStaff = "ADL";

			CombineAssertions("Should have defaulted from auRuleA", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.FIX, agreement2Recipient.CAR_CommissionType);
				AssertEquals("Count", 1, agreement2Recipient.Rates.Count);
				AssertEquals("CAT_CommissionAmount", (ZDecimal)100d, agreement2Recipient.Rates[0].CAT_CommissionAmount);
				AssertEquals("CAT_RX_NKCommissionCurrency", "AUD", agreement2Recipient.Rates[0].CAT_RX_NKCommissionCurrency);
				AssertEquals("CAT_CommissionPercentage", (ZDecimal)0d, agreement2Recipient.Rates[0].CAT_CommissionPercentage);
				AssertEquals("CAT_CommissionPeriod", "EXS", agreement2Recipient.Rates[0].CAT_CommissionPeriod);
			});
		}

		public void TestCAR_GS_NKStaff_ShouldRedefaultRatesWhenStaffIsChanged()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SR1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SR2";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "SR3";

			var rule1 = staff1.CommissionRules.AddNew();
			rule1.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			rule1.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			rule1.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			rule1.FillWithValidTestData();
			var rate1 = rule1.Rates.AddNew();
			rate1.ACT_CommissionType = CommissionTypes.Codes.FIX;
			rate1.ACT_CommissionAmount = 10d;
			rate1.ACT_RX_NKCommissionCurrency = "AUD";
			rate1.ACT_CommissionPeriod = "NEW";

			var rule2 = staff2.CommissionRules.AddNew();
			rule2.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			rule2.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			rule2.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			rule2.FillWithValidTestData();
			var rate2 = rule2.Rates.AddNew();
			rate2.ACT_CommissionType = CommissionTypes.Codes.PCT;
			rate2.ACT_CommissionPercentage = 50d;
			rate2.ACT_CommissionPeriod = "EXS";

			Factory.Save();

			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement,
				OrgCommissionAgreementItemLookups.AllProductsCode,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "SR1";
			recipient.CAR_IsCommissionRateOverriden = false;

			CombineAssertions("Should have defaulted rules from SR1", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.FIX, recipient.CAR_CommissionType);
				AssertEquals("Rates.Count", 1, recipient.Rates.Count);
				if (recipient.Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionAmount", (ZDecimal)10d, recipient.Rates[0].CAT_CommissionAmount);
					AssertEquals("CAT_RX_NKCommissionCurrency", "AUD", recipient.Rates[0].CAT_RX_NKCommissionCurrency);
					AssertEquals("CAT_CommissionPeriod", "NEW", recipient.Rates[0].CAT_CommissionPeriod);
				}
			});

			recipient.CAR_GS_NKStaff = "SR2";
			CombineAssertions("Should have defaulted rules from SR2", () =>
			{
				AssertEquals("CAR_IsCommissionRateOverriden", false, recipient.CAR_IsCommissionRateOverriden);
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.PCT, recipient.CAR_CommissionType);
				AssertEquals("Rates.Count", 1, recipient.Rates.Count);
				if (recipient.Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionPercentage", (ZDecimal)50d, recipient.Rates[0].CAT_CommissionPercentage);
					AssertEquals("CAT_CommissionPeriod", "EXS", recipient.Rates[0].CAT_CommissionPeriod);
				}
			});

			recipient.CAR_GS_NKStaff = "SR3";
			CombineAssertions("Should have cleared rates since no rules available for SR3", () =>
			{
				AssertEquals("CAR_CommissionType", "", recipient.CAR_CommissionType);
				AssertEquals("Rates.Count", 0, recipient.Rates.Count);
			});
		}

		public void TestCAR_GS_NKStaff_ReadOnly()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			recipient.CAR_GS_NKStaff = "";
			recipient.CAR_OH_Party = ZGuid.Empty;
			AssertEquals(false, recipient.CAR_GS_NKStaffInfo.ReadOnly);

			recipient.CAR_OH_Party = Factory.New<OrgHeader>().PK;
			AssertEquals(true, recipient.CAR_GS_NKStaffInfo.ReadOnly);

			recipient.CAR_OH_Party = ZGuid.Empty;
			recipient.CAR_GS_NKStaff = "ADL";
			AssertEquals(false, recipient.CAR_GS_NKStaffInfo.ReadOnly);

			var rate = recipient.Rates.AddNew();
			var commissionLine = Factory.New<IAccCommissionLine>();
			commissionLine.CL0_CAT = rate.PK;
			AssertEquals(true, recipient.CAR_GS_NKStaffInfo.ReadOnly);
		}

		public void TestHasResponsibleStaffCommissionRule()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			var staffCommissionRule = staff.CommissionRules.AddNew();
			staffCommissionRule.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			staffCommissionRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			staffCommissionRule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			staffCommissionRule.FillWithValidTestData();

			Factory.Save();

			var opportunity = Factory.New<OrgOpportunity>();
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1,
				JobInvoicingConsumerTypes.Shipment.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var recipient1 = agreement1.Recipients.AddNew();
			recipient1.CAR_GS_NKStaff = "ADL";
			AssertEquals(true, recipient1.HasResponsibleStaffCommissionRule);

			var agreement2 = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement2,
				JobInvoicingConsumerTypes.Brokerage.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var recipient2 = agreement2.Recipients.AddNew();
			recipient2.CAR_GS_NKStaff = "ADL";
			AssertEquals(false, recipient2.HasResponsibleStaffCommissionRule);
		}

		#endregion

		#region CAR_OH_Party

		public void TestCAR_OH_Party()
		{
			var orgProxy = Factory.New<OrgHeader>();
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = orgProxy.PK;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_IsSalesRep = true;
			salesRep.GS_Code = "ADL";
			salesRep.GS_GB_HomeBranch = branch.PK;

			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			recipient.CAR_OH_Party = Factory.New<OrgHeader>().PK;

			recipient.CAR_GS_NKStaff = "ADL";
			AssertEquals("Should inherit party from staff", orgProxy.PK, recipient.CAR_OH_Party);
		}

		public void TestCAR_OH_Party_ReadOnly()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			recipient.CAR_GS_NKStaff = "";
			recipient.CAR_OH_Party = ZGuid.Empty;
			AssertEquals(false, recipient.CAR_OH_PartyInfo.ReadOnly);

			recipient.CAR_GS_NKStaff = "ADL";
			AssertEquals(true, recipient.CAR_OH_PartyInfo.ReadOnly);

			recipient.CAR_GS_NKStaff = "";
			recipient.CAR_OH_Party = Factory.New<OrgHeader>().PK;
			AssertEquals(false, recipient.CAR_OH_PartyInfo.ReadOnly);

			var rate = recipient.Rates.AddNew();
			var commissionLine = Factory.New<IAccCommissionLine>();
			commissionLine.CL0_CAT = rate.PK;
			AssertEquals(true, recipient.CAR_OH_PartyInfo.ReadOnly);
		}

		#endregion

		#region CAR_CommissionType

		public void TestCAR_CommissionType_ClearingTypeAlsoClearsShareAmount()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			recipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			recipient.CAR_Share = 10;

			recipient.CAR_CommissionType = "";
			AssertEquals((byte)0, recipient.CAR_Share);
		}

		public void TestCAR_CommissionType_CallsRefreshBindingForSharePercentageForAllRecipients()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var commissionAgreement = opportunity.CommissionAgreements.AddNew();
			var recipientA = commissionAgreement.Recipients.AddNew();
			var recipientB = commissionAgreement.Recipients.AddNew();
			var recipientC = commissionAgreement.Recipients.AddNew();

			var recipientASharePercentageValueChangedCalled = false;
			var recipientBSharePercentageValueChangedCalled = false;
			var recipientCSharePercentageValueChangedCalled = false;
			recipientA.SharePercentageInfo.ValueChanged += (sender, e) => recipientASharePercentageValueChangedCalled = true;
			recipientB.SharePercentageInfo.ValueChanged += (sender, e) => recipientBSharePercentageValueChangedCalled = true;
			recipientC.SharePercentageInfo.ValueChanged += (sender, e) => recipientCSharePercentageValueChangedCalled = true;

			recipientB.CAR_CommissionType = CommissionTypes.Codes.FIX;

			CombineAssertions("Should have called SharePercentage's Refresh Binding for all recipients", () =>
			{
				AssertEquals("Recipient A", true, recipientASharePercentageValueChangedCalled);
				AssertEquals("Recipient B", true, recipientBSharePercentageValueChangedCalled);
				AssertEquals("Recipient C", true, recipientCSharePercentageValueChangedCalled);
			});
		}

		#endregion

		#region CAR_Share

		public void TestCAR_Share_IsZeroAndReadOnly_WhenNoResponsibleRuleExists_AndRatesNotOverriden()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			AssertEquals("Precondition", 0, staff.OverallCommissionRules.Count);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();

			Factory.Save();

			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "";
			recipient.CAR_OH_Party = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Should be 1 since it is org recipient", (ZByte)1, recipient.CAR_Share);
			AssertEquals("Should be not be read only since it is org recipient", false, recipient.CAR_ShareInfo.ReadOnly);

			recipient.CAR_GS_NKStaff = "ADL";
			AssertEquals("Should be 0 when no rules exists", ZByte.Zero, recipient.CAR_Share);
			AssertEquals("Should be read only when no rules exists and rates not overriden", true, recipient.CAR_ShareInfo.ReadOnly);
			recipient.Delete();

			recipient = Factory.New<OrgCommissionAgreementRecipient>();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_IsCommissionRateOverriden = true;
			AssertEquals("Should be 0 when no rules exists", (ZByte)0, recipient.CAR_Share);
			AssertEquals("Should be not be read only since rates overriden", false, recipient.CAR_ShareInfo.ReadOnly);
		}

		public void TestCAR_Share_CallsRefreshBindingForSharePercentageForAllRecipients()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var commissionAgreement = opportunity.CommissionAgreements.AddNew();
			var recipientA = commissionAgreement.Recipients.AddNew();
			var recipientB = commissionAgreement.Recipients.AddNew();
			var recipientC = commissionAgreement.Recipients.AddNew();

			var recipientASharePercentageValueChangedCalled = false;
			var recipientBSharePercentageValueChangedCalled = false;
			var recipientCSharePercentageValueChangedCalled = false;
			recipientA.SharePercentageInfo.ValueChanged += (sender, e) => recipientASharePercentageValueChangedCalled = true;
			recipientB.SharePercentageInfo.ValueChanged += (sender, e) => recipientBSharePercentageValueChangedCalled = true;
			recipientC.SharePercentageInfo.ValueChanged += (sender, e) => recipientCSharePercentageValueChangedCalled = true;

			recipientB.CAR_Share = 10;

			CombineAssertions("Should have called SharePercentage's Refresh Binding for all recipients", () =>
			{
				AssertEquals("Recipient A", true, recipientASharePercentageValueChangedCalled);
				AssertEquals("Recipient B", true, recipientBSharePercentageValueChangedCalled);
				AssertEquals("Recipient C", true, recipientCSharePercentageValueChangedCalled);
			});
		}

		#endregion

		#region Name

		public void TestName()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_FullName = "Andrew";
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org";

			var recipient1 = Factory.New<OrgCommissionAgreementRecipient>();
			recipient1.CAR_GS_NKStaff = "ADL";
			AssertEquals("Andrew", recipient1.Name);

			var recipient2 = Factory.New<OrgCommissionAgreementRecipient>();
			recipient2.CAR_OH_Party = org.PK;
			AssertEquals("Test Org", recipient2.Name);
		}

		#endregion

		#region Share Percentage

		public void TestSharePercentage()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var commissionAgreement = opportunity.CommissionAgreements.AddNew();
			var recipientFixA = commissionAgreement.Recipients.AddNew();
			recipientFixA.CAR_CommissionType = CommissionTypes.Codes.FIX;
			var recipientFixB = commissionAgreement.Recipients.AddNew();
			recipientFixB.CAR_CommissionType = CommissionTypes.Codes.FIX;

			var recipientPctA = commissionAgreement.Recipients.AddNew();
			recipientPctA.CAR_CommissionType = CommissionTypes.Codes.PCT;
			var recipientPctB = commissionAgreement.Recipients.AddNew();
			recipientPctB.CAR_CommissionType = CommissionTypes.Codes.PCT;
			var recipientPctC = commissionAgreement.Recipients.AddNew();
			recipientPctC.CAR_CommissionType = CommissionTypes.Codes.PCT;

			recipientFixA.CAR_Share = 1;
			recipientFixB.CAR_Share = 1;

			recipientPctA.CAR_Share = 1;
			recipientPctB.CAR_Share = 4;
			recipientPctC.CAR_Share = 0;

			AssertEquals((ZDecimal)50d, recipientFixA.SharePercentage);
			AssertEquals((ZDecimal)50d, recipientFixB.SharePercentage);

			AssertEquals((ZDecimal)20d, recipientPctA.SharePercentage);
			AssertEquals((ZDecimal)80d, recipientPctB.SharePercentage);
			AssertEquals((ZDecimal)0d, recipientPctC.SharePercentage);
		}

		#endregion

		#endregion

		#region Draft

		public void TestCreateAndMergeDraft()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			recipient.CAR_GS_NKStaff = "ADL";
			recipient.CAR_Comment = "Original Comment";
			var rate1 = recipient.Rates.AddNew();

			var recipientDraft = recipient.CreateDraft();
			recipientDraft.CAR_Comment = "New Comment";
			var rate2 = recipientDraft.Rates.AddNew();

			recipientDraft.MergeDraft();

			AssertEquals("New Comment", recipient.CAR_Comment);
			AssertContainsExactElementsInAnyOrder(new[] { rate1, rate2 }, recipient.Rates);
		}

		public void TestCreateAndMergeDraft_ParentPartyOrg_DraftStaff()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_OH_Party = org.PK;
			var originalRate = recipient.Rates.AddNew();

			Factory.Save();

			var agreementDraft = agreement.CreateDraft();
			var recipientDraft = agreementDraft.Recipients[0];

			var primarySalesStaff = Factory.NewWithValidTestData<GlbStaff>();
			primarySalesStaff.GS_Code = "ADL";

			recipientDraft.CAR_GS_NKStaff = "ADL";

			Factory.Save();

			agreementDraft.ApproveDraft();

			Factory.Save();

			AssertEquals(agreement.Recipients[0].CAR_GS_NKStaff, "ADL");
		}

		public void TestMergeDraft_DeleteRateWithCommisions()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			var originalRate = recipient.Rates.AddNew();

			var commissionLine = Factory.New<IAccCommissionLine>();
			commissionLine.CL0_CAT = originalRate.PK;
			((BusinessObject)commissionLine).FillWithValidTestData();
			Factory.Save();

			AssertEquals("precondition (recipient)", 1, agreement.Recipients.Count);
			AssertEquals("precondition (rate)", 1, recipient.Rates.Count);
			AssertNotNullOrEmpty("precondition (agreementId)", agreement.AgreementId);

			var agreementDraft = agreement.CreateDraft();
			var recipientDraft = agreementDraft.Recipients[0];
			recipientDraft.Rates.DeleteAll();
			recipientDraft.Rates.AddNew();

			var ex = AssertExceptionThrown<CommissionAgreementApprovalException>(() => recipientDraft.MergeDraft());

			AssertContains("cannot be removed", ex.Message);
			AssertContains(recipient.CommissionAgreement.AgreementId, ex.Message);
			AssertContains(originalRate.PK.ToString(), ex.Message);

			AssertContains("Commission transactions lines have been processed under the previously approved agreement conditions.", ex.UserFriendlyMessage);
			AssertContains(recipient.CommissionAgreement.AgreementId, ex.UserFriendlyMessage);
		}

		#endregion

		#region Emails

		public void TestNotificationEmailAddresses()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";
			var org = Factory.New<OrgHeader>();
			var orgContact1 = org.Contacts.AddNew();
			orgContact1.OC_Email = "contact1@email.com";
			var orgContact2 = org.Contacts.AddNew();
			orgContact2.OC_Email = "contact2@email.com";
			var orgContact3 = org.Contacts.AddNew();
			orgContact3.OC_Email = "contact3@email.com";
			orgContact1.Documents.AddNew().OD_DocumentGroup = ContactType.CommissionAgreementRecipient.Code;
			orgContact2.Documents.AddNew().OD_DocumentGroup = ContactType.CommissionAgreementRecipient.Code;

			var recipient1 = Factory.New<OrgCommissionAgreementRecipient>();
			recipient1.CAR_GS_NKStaff = "ADL";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "andrew.luong@wisetechglobal.com" }, recipient1.NotificationEmailAddresses);

			var recipient2 = Factory.New<OrgCommissionAgreementRecipient>();
			recipient2.CAR_OH_Party = org.PK;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "contact1@email.com", "contact2@email.com" }, recipient2.NotificationEmailAddresses);
		}

		#endregion

		#region Security

		public void TestIsViewRatesAllowed()
		{
			Env.Security.CommissionAgreementViewAny.IsAllowed = false;

			var primarySalesStaff = Factory.NewWithValidTestData<GlbStaff>();
			primarySalesStaff.GS_Code = "ADL";
			var agreementRecipientStaff = Factory.NewWithValidTestData<GlbStaff>();
			agreementRecipientStaff.GS_Code = "SCW";
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "SCW";

			Factory.Save();

			AssertEquals("Should not be able to view other recipient rates", false, recipient.IsViewRatesAllowed);

			using (Env.SetTemporaryUserContext(agreementRecipientStaff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Should be able to view own recipient rates", true, recipient.IsViewRatesAllowed);
			}

			using (Env.SetTemporaryUserContext(primarySalesStaff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Should not be able to view other recipient rates", false, recipient.IsViewRatesAllowed);

				recipient.CAR_GS_NKStaff = "";
				AssertEquals("Primary Sales Person should be able to view org rates", true, recipient.IsViewRatesAllowed);
			}

			AssertEquals("Only Primary Sales People should be able to view org rates", false, recipient.IsViewRatesAllowed);

			Env.Security.CommissionAgreementViewAny.IsAllowed = true;
			AssertEquals("Should be allowed to view any rates with view any security permission", true, recipient.IsViewRatesAllowed);
		}

		public void TestIsEditRatesAllowed()
		{
			Env.Security.CommissionAgreementOverrideAny.IsAllowed = false;

			var primarySalesStaff = Factory.NewWithValidTestData<GlbStaff>();
			primarySalesStaff.GS_Code = "ADL";
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";
			recipient.CAR_GS_NKStaff = "";
			recipient.CAR_OH_Party = agreement.CA0_OH_Customer;

			Factory.Save();

			AssertEquals(false, recipient.IsEditRatesAllowed);

			using (Env.SetTemporaryUserContext(primarySalesStaff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Primary Sales Person should be allowed to edit rates for orgs", true, recipient.IsEditRatesAllowed);

				recipient.CAR_OH_Party = ZGuid.Empty;
				recipient.CAR_GS_NKStaff = "ADL";
				AssertEquals("Primary Sales Person should not be allowed to edit staff rates without override security permission", false, recipient.IsEditRatesAllowed);
			}

			Env.Security.CommissionAgreementOverrideAny.IsAllowed = true;
			AssertEquals("Should be allowed to edit staff rates with override security permission", true, recipient.IsEditRatesAllowed);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			var rate = recipient.Rates.AddNew();
			recipient.Delete();

			AssertEquals(true, rate.IsDeleted);
		}

		public void TestCanDelete()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			var rate = recipient.Rates.AddNew();

			AssertEquals(true, recipient.CanDelete);

			Factory.Save();

			var commissionLine = Factory.New<IAccCommissionLine>();
			commissionLine.CL0_CAT = rate.PK;
			((BusinessObject)commissionLine).FillWithValidTestData();
			Factory.Save();

			AssertEquals(false, recipient.CanDelete);

			var reloadedRecipient = new BusinessObjectFactory().Load<OrgCommissionAgreementRecipient>(recipient.PK);
			AssertEquals(false, reloadedRecipient.CanDelete);
			AssertEquals("Wolf Pack members cannot be deleted once commissions have been generated. To exclude this Wolf Pack Member from this agreement, set its share to 0, and then re-approve the agreement.", reloadedRecipient.ReasonForNotAbleToDelete);

			var reloadedAgreement = new BusinessObjectFactory().Load<OrgCommissionAgreement>(agreement.PK);
			var draft = reloadedAgreement.CreateDraft();
			var draftRecipient = draft.Recipients.First(x => x.CAR_GS_NKStaff == "ADL");
			AssertEquals(false, draftRecipient.CanDelete);
			AssertEquals("Wolf Pack members cannot be deleted once commissions have been generated. To exclude this Wolf Pack Member from this agreement, set its share to 0, and then re-approve the agreement.", draftRecipient.ReasonForNotAbleToDelete);
		}

		#endregion

		#region Log

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var recipient = Factory.New<OrgCommissionAgreementRecipient>();
			AssertEquals(0, recipient.BusinessObjectsWithRelatedEvents.Length);

			recipient.Rates.AddNew();
			AssertEquals(1, recipient.BusinessObjectsWithRelatedEvents.Length);

			recipient.Rates.AddNew();
			AssertEquals(2, recipient.BusinessObjectsWithRelatedEvents.Length);
		}

		#endregion
	}
}
