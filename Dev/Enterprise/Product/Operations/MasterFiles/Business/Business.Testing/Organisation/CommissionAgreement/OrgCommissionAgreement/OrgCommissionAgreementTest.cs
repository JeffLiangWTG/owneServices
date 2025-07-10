using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreement))]
	class OrgCommissionAgreementTest : EnterpriseBusinessObjectTestCase
	{
		#region Find Duplicate

		public void TestFindDuplicate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var agreement1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement1Item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement1, "SHP", "XXX", "XXX", "XXX");

			var agreement2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement2Item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement2, "SHP", "XXX", "XXX", "XXX");

			AssertEquals(agreement1Item1, agreement1.FindDuplicate(agreement2Item1));

			var agreement3 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement3Item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement3, "SHP", "XXX", "XXX", "YYY");

			AssertEquals(null, agreement1.FindDuplicate(agreement3Item1));
			AssertEquals(null, agreement2.FindDuplicate(agreement3Item1));

			var agreement4 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement4Item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement4, "AGS", "XXX", "XXX", "XXX");

			AssertEquals(null, agreement1.FindDuplicate(agreement4Item1));
			AssertEquals(null, agreement2.FindDuplicate(agreement4Item1));
			AssertEquals(null, agreement3.FindDuplicate(agreement4Item1));

			var agreement5 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement5Item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement5, "SHP", "AAA", "BBB", "YYY");
			var agreement5Item2 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement5, "AGS", "UUU", "UUU", "YYY");
			var cond2 = agreement5Item2.ConditionCollection.AddNew();
			cond2.CIC_Mode = "XXX";
			cond2.CIC_RL_NKOrigin = "XXX";
			cond2.CIC_RL_NKDestination = "XXX";

			AssertEquals(null, agreement1.FindDuplicate(agreement5Item1));
			AssertEquals(null, agreement2.FindDuplicate(agreement5Item1));
			AssertEquals(null, agreement3.FindDuplicate(agreement5Item1));
			AssertEquals(null, agreement4.FindDuplicate(agreement5Item1));

			AssertEquals(null, agreement1.FindDuplicate(agreement5Item2));
			AssertEquals(null, agreement2.FindDuplicate(agreement5Item2));
			AssertEquals(null, agreement3.FindDuplicate(agreement5Item2));
			AssertEquals(agreement4Item1, agreement4.FindDuplicate(agreement5Item2));
		}

		#endregion

		#region Properties

		#region Categorization Properties

		public void TestCA0_OH_Customer_AddSalesPersonAsRecipientOnceCustomerIsEntered()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";
			var agreement = opportunity.CommissionAgreements.AddNew();
			AssertEquals("Precondition", ZGuid.Empty, agreement.CA0_OH_Customer);
			AssertEquals("Should not add sales person recipient until Customer is entered", 0, agreement.Recipients.Count);

			agreement.CA0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertContainsExactElementsInAnyOrder("Should add sales person recipient",
				new ZString[] { "ADL" },
				agreement.Recipients.Select(x => x.CAR_GS_NKStaff));

			agreement.CA0_OH_Customer = ZGuid.Empty;
			agreement.CA0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertContainsExactElementsInAnyOrder("Should not add sales person as recipient again",
				new ZString[] { "ADL" },
				agreement.Recipients.Select(x => x.CAR_GS_NKStaff));
		}

		public void TestCA0_OH_Customer_OrgInactive()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = customer.PK;
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1,
				JobInvoicingConsumerTypes.Shipment.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);
			agreement1.CA0_OH_Customer = customer.PK;
			agreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement1.CA0_CommissionBasis = CommissionBasisType.Codes.REV;

			Factory.Save();
			customer.OH_IsActive = false;

			Factory.Save();

			agreement1.Validation.ValidateCA0_OH_Customer();
			AssertHasWarning(agreement1.CA0_OH_CustomerInfo, "This Customer is inactive.");

			Factory.ClearCachedValue<OrgHeaderCollection>("OrgCommissionAgreementLookups.Customers" + opportunity.P8_OH + "-" + agreement1.CA0_Name);

			var agreement2 = agreement1.CreateDraft(true);

			agreement2.Validation.ValidateCA0_OH_Customer();
			AssertHasWarning(agreement2.CA0_OH_CustomerInfo, "This Customer is inactive.");

			var agreement3 = opportunity.CommissionAgreements.AddNew();

			agreement3.CA0_OH_Customer = customer.PK;
			agreement3.Validation.ValidateCA0_OH_Customer();

			AssertHasError(agreement3.CA0_OH_CustomerInfo, "This Customer is inactive - it may not be used.");
		}

		public void TestDefaultTriggerTypeAndCommissionBasisFromSalesPersonCommissionRules()
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
			auRuleA.ACM_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			auRuleA.ACM_CommissionBasis = CommissionBasisType.Codes.PRF;
			auRuleA.FillWithValidTestData();
			var auRuleARate = auRuleA.Rates.AddNew();
			auRuleARate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			auRuleARate.FillWithValidTestData();

			var auRuleB = salesTeamAu.CommissionRules.AddNew();
			auRuleB.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			auRuleB.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			auRuleB.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			auRuleB.ACM_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			auRuleB.ACM_CommissionBasis = CommissionBasisType.Codes.REV;
			auRuleB.FillWithValidTestData();
			var auRuleBRate = auRuleB.Rates.AddNew();
			auRuleBRate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			auRuleBRate.FillWithValidTestData();

			var usRule = salesTeamUs.CommissionRules.AddNew();
			usRule.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			usRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			usRule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			usRule.ACM_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			usRule.ACM_CommissionBasis = CommissionBasisType.Codes.REV;
			usRule.FillWithValidTestData();

			Factory.Save();

			var auOrg = Factory.New<OrgHeader>();
			auOrg.OH_RL_NKClosestPort = "AUSYD";
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = auOrg.PK;
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";

			var agreement1 = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1,
				JobInvoicingConsumerTypes.Shipment.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);
			agreement1.CA0_OH_Customer = auOrg.PK;

			CombineAssertions("Should have defaulted from auRuleB", () =>
			{
				AssertEquals("CA0_CommissionTriggerType", CommissionTriggerTypes.Codes.FirstArInvoice, agreement1.CA0_CommissionTriggerType);
				AssertEquals("CA0_CommissionBasis", CommissionBasisType.Codes.REV, agreement1.CA0_CommissionBasis);

				AssertEquals("Recipients.Count", 1, agreement1.Recipients.Count);
				if (agreement1.Recipients.Count == 1)
				{
					AssertEquals("CAR_CommissionType", CommissionTypes.Codes.PCT, agreement1.Recipients[0].CAR_CommissionType);
				}
			});

			var agreement2 = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement2,
				JobInvoicingConsumerTypes.Brokerage.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);
			agreement2.CA0_OH_Customer = auOrg.PK;

			CombineAssertions("Should have defaulted from auRuleA", () =>
			{
				AssertEquals("CA0_CommissionTriggerType", CommissionTriggerTypes.Codes.ClientCommencedDate, agreement2.CA0_CommissionTriggerType);
				AssertEquals("CA0_CommissionBasis", CommissionBasisType.Codes.PRF, agreement2.CA0_CommissionBasis);

				AssertEquals("Recipients.Count", 1, agreement2.Recipients.Count);
				if (agreement2.Recipients.Count == 1)
				{
					AssertEquals("CAR_CommissionType", CommissionTypes.Codes.FIX, agreement2.Recipients[0].CAR_CommissionType);
				}
			});
		}

		public void TestSettingProductServiceSubModuleOrCustomer_ShouldRefreshCommissionRuleDefaultsOfAllNonSavedStaffRecipients()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			var shipmentRule = staff.CommissionRules.AddNew();
			shipmentRule.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			shipmentRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			shipmentRule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			shipmentRule.FillWithValidTestData();
			var shipmentRate = shipmentRule.Rates.AddNew();
			shipmentRate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			shipmentRate.ACT_CommissionPeriod = "NEW";

			var brokerageRule = staff.CommissionRules.AddNew();
			brokerageRule.ACM_Product = JobInvoicingConsumerTypes.Brokerage.Code;
			brokerageRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			brokerageRule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			brokerageRule.FillWithValidTestData();
			var brokerageRate = brokerageRule.Rates.AddNew();
			brokerageRate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			brokerageRate.ACT_CommissionPeriod = "EXS";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";
			var agreement = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement,
				JobInvoicingConsumerTypes.Shipment.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);
			agreement.CA0_OH_Customer = customer.PK;

			AssertContainsExactElementsInAnyOrder("Should have added primary sales person as a recipient", new[] { "ADL" }, agreement.Recipients.Select(x => x.CAR_GS_NKStaff.ToString()));
			CombineAssertions("Should have defaulted rules from shipmentRule", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.FIX, agreement.Recipients[0].CAR_CommissionType);
				AssertEquals("Rates.Count", 1, agreement.Recipients[0].Rates.Count);
				if (agreement.Recipients[0].Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionPeriod", "NEW", agreement.Recipients[0].Rates[0].CAT_CommissionPeriod);
				}
			});

			agreement.ProductItems[0].CAI_Code = JobInvoicingConsumerTypes.Brokerage.Code;
			CombineAssertions("Should have defaulted rules from brokerageRule", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.PCT, agreement.Recipients[0].CAR_CommissionType);
				AssertEquals("Rates.Count", 1, agreement.Recipients[0].Rates.Count);
				if (agreement.Recipients[0].Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionPeriod", "EXS", agreement.Recipients[0].Rates[0].CAT_CommissionPeriod);
				}
			});
		}

		public void TestChangingProductOriginDestinationOrMode_ShouldRevalidateCommissionAgreementAgainstEntitlementRules()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";

			var ruleProduct = staff.CommissionRules.AddNew();
			ruleProduct.ACM_Product = JobInvoicingConsumerTypes.Brokerage.Code;
			ruleProduct.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			ruleProduct.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			ruleProduct.ACM_NKOrigin = "AUSYD";
			ruleProduct.ACM_NKDestination = "ADCAN";
			ruleProduct.ACM_Mode = "COU";
			ruleProduct.FillWithValidTestData();
			var ruleProductRate = ruleProduct.Rates.AddNew();
			ruleProductRate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleProductRate.ACT_CommissionPeriod = "NEW";

			var ruleOrigin = staff.CommissionRules.AddNew();
			ruleOrigin.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			ruleOrigin.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			ruleOrigin.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			ruleOrigin.ACM_NKOrigin = "ADCAN";
			ruleOrigin.ACM_NKDestination = "ADCAN";
			ruleOrigin.ACM_Mode = "COU";
			ruleOrigin.FillWithValidTestData();
			var ruleOriginRate = ruleOrigin.Rates.AddNew();
			ruleOriginRate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			ruleOriginRate.ACT_CommissionPeriod = "EXS";

			var ruleDestination = staff.CommissionRules.AddNew();
			ruleDestination.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			ruleDestination.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			ruleDestination.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			ruleDestination.ACM_NKOrigin = "AUSYD";
			ruleDestination.ACM_NKDestination = "AUBNE";
			ruleDestination.ACM_Mode = "COU";
			ruleDestination.FillWithValidTestData();
			var ruleDestinationRate = ruleDestination.Rates.AddNew();
			ruleDestinationRate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			ruleDestinationRate.ACT_CommissionPeriod = "NEW";

			var ruleMode = staff.CommissionRules.AddNew();
			ruleMode.ACM_Product = JobInvoicingConsumerTypes.Shipment.Code;
			ruleMode.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			ruleMode.ACM_NKOrigin = "AUSYD";
			ruleMode.ACM_NKDestination = "ADCAN";
			ruleMode.ACM_Mode = "RAI";
			ruleMode.FillWithValidTestData();
			var ruleModeRate = ruleMode.Rates.AddNew();
			ruleModeRate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			ruleModeRate.ACT_CommissionPeriod = "EXS";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";
			var agreement = opportunity.CommissionAgreements.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement,
				JobInvoicingConsumerTypes.Shipment.Code,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);
			agreement.CA0_OH_Customer = customer.PK;

			var agreementCondition = agreement.ProductItems[0].ConditionCollection[0];

			AssertContainsExactElementsInAnyOrder("Should have added primary sales person as a recipient", new[] { "ADL" }, agreement.Recipients.Select(x => x.CAR_GS_NKStaff.ToString()));

			agreementCondition.CIC_RL_NKOrigin = "ADCAN";
			agreementCondition.CIC_RL_NKDestination = "ADCAN";

			AssertEquals("Rates.Count", 1, agreement.Recipients[0].Rates.Count);

			CombineAssertions("Should have defaulted rules from ruleOrigin", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.FIX, agreement.Recipients[0].CAR_CommissionType);
				AssertEquals("Rates.Count", 1, agreement.Recipients[0].Rates.Count);
				if (agreement.Recipients[0].Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionPeriod", "EXS", agreement.Recipients[0].Rates[0].CAT_CommissionPeriod);
				}
			});

			agreementCondition.CIC_RL_NKOrigin = "ABCDE";
			AssertEquals("Rates.Count", 0, agreement.Recipients[0].Rates.Count);

			agreementCondition.CIC_RL_NKOrigin = "AUSYD";
			agreementCondition.CIC_RL_NKDestination = "AUBNE";

			CombineAssertions("Should have defaulted rules from ruleDestination", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.PCT, agreement.Recipients[0].CAR_CommissionType);
				AssertEquals("Rates.Count", 1, agreement.Recipients[0].Rates.Count);
				if (agreement.Recipients[0].Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionPeriod", "NEW", agreement.Recipients[0].Rates[0].CAT_CommissionPeriod);
				}
			});

			agreementCondition.CIC_RL_NKDestination = "ABCDE";
			AssertEquals("Rates.Count", 0, agreement.Recipients[0].Rates.Count);

			agreementCondition.CIC_RL_NKDestination = "ADCAN";
			agreementCondition.CIC_Mode = "RAI";

			CombineAssertions("Should have defaulted rules from ruleMode", () =>
			{
				AssertEquals("CAR_CommissionType", CommissionTypes.Codes.PCT, agreement.Recipients[0].CAR_CommissionType);
				AssertEquals("Rates.Count", 1, agreement.Recipients[0].Rates.Count);
				if (agreement.Recipients[0].Rates.Count == 1)
				{
					AssertEquals("CAT_CommissionPeriod", "EXS", agreement.Recipients[0].Rates[0].CAT_CommissionPeriod);
				}
			});
		}

		#endregion

		#region CA0_LastApprovedDateUtc

		public void TestCA0_LastApprovedDateUtc_Concurrency()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var agreementInFactory1 = factory1.NewWithValidTestData<OrgCommissionAgreement>();
			agreementInFactory1.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			factory1.Save();

			var agreementInFactory2 = factory2.Load<OrgCommissionAgreement>(agreementInFactory1.PK);
			agreementInFactory2.ApproveDraft();
			factory2.Save();

			agreementInFactory1.ApproveDraft();
			AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
		}

		#endregion

		#region CA0_RequireApprovalFromDate

		public void TestNotifyHasChangedPreventingAutoApproval()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();

			var draft = agreement.CreateDraft();
			draft.NotifyHasChangedPreventingAutoApproval();
			Factory.Save();

			AssertEquals("Should not have auto-approved", true, draft.IsDraft);
		}

		#endregion

		#region CA0_ExpiredDate

		public void TestCA0_ExpiredDate_ReadOnly()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			AssertEquals(true, agreement.CA0_ExpiredDateInfo.ReadOnly);
		}

		#endregion

		#region CA0_Name

		public void TestCA0_Name_ReadOnly()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			AssertEquals(true, agreement.CA0_NameInfo.ReadOnly);
		}

		#endregion

		#region CA0_ReversedDateUtc

		public void TestReverse()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			AssertEquals(false, agreement.IsReversed);

			agreement.Reverse();

			AssertEquals(true, agreement.IsReversed);
		}

		#endregion

		#region IsEffective

		public void TestIsEffective()
		{
			var opportunityStatuses = new OpportunityStatusCollection();
			opportunityStatuses.Add("CRT", (NoResString)"Current", false, false, true, "");
			opportunityStatuses.Add("LOS", (NoResString)"Lost", false, true, true, "");
			opportunityStatuses.Add("WON", (NoResString)"Won", true, true, true, "");
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, opportunityStatuses);

			var opportunity = Factory.New<OrgOpportunity>();
			var commissionAgreement = opportunity.ApprovedCommissionAgreements.AddNew();

			opportunity.P8_Status = "CRT";
			AssertEquals("Should not be effective as its status is not flagged as effective agreement", false, commissionAgreement.OpportunityIsEffective(ZGuid.Empty));

			opportunity.P8_Status = "WON";
			AssertEquals("Should be effective as its status is flagged as effective agreement", true, commissionAgreement.OpportunityIsEffective(ZGuid.Empty));
		}

		#endregion

		#region EffectiveDate

		public void TestEffectiveDate()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TS1";
			var opportunity = org.SalesOpportunities.AddNew();
			var commissionAgreement = opportunity.CommissionAgreements.AddNew();
			commissionAgreement.CA0_CommissionBasis = "AAA";
			commissionAgreement.CA0_OH_Customer = org.PK;
			AssertEquals(true, commissionAgreement.EffectiveDateInfo.ReadOnly);

			commissionAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			AssertEquals(true, commissionAgreement.EffectiveDateInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, commissionAgreement.EffectiveDate);

			org.MiscServ.OM_CMClientCommenced = new ZDateTime(2000, 1, 1);
			AssertEquals(new ZDateTime(2000, 1, 1), commissionAgreement.EffectiveDate);

			commissionAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			AssertEquals(true, commissionAgreement.EffectiveDateInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, commissionAgreement.EffectiveDate);

			commissionAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.EarliestRevRecog;
			AssertEquals(true, commissionAgreement.EffectiveDateInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, commissionAgreement.EffectiveDate);

			var arInvoice = CreateNewARInvoice(org);
			arInvoice.AH_PostDate = new ZDateTime(2000, 10, 10);
			var apInvoice = CreateNewAPInvoice(org);
			apInvoice.AH_PostDate = new ZDateTime(2000, 5, 5);

			var arInvoiceLine = CreateNewInvoiceLine(arInvoice, new ZDateTime(2000, 4, 4));
			var arInvoiceLine2 = CreateNewInvoiceLine(arInvoice, new ZDateTime(2000, 6, 6));

			commissionAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			AssertEquals(new ZDateTime(2000, 10, 10), commissionAgreement.EffectiveDate);

			Factory.Save();

			commissionAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.EarliestRevRecog;
			AssertEquals(new ZDateTime(2000, 4, 4), commissionAgreement.EffectiveDate);

			var arInvoice2 = CreateNewARInvoice(org);
			arInvoice2.AH_PostDate = new ZDateTime(1999, 9, 9);

			commissionAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			AssertEquals(new ZDateTime(1999, 9, 9), commissionAgreement.EffectiveDate);

			commissionAgreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			AssertEquals(false, commissionAgreement.EffectiveDateInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, commissionAgreement.EffectiveDate);

			commissionAgreement.EffectiveDate = new ZDate(2001, 1, 1);
			AssertEquals(new ZDateTime(2001, 1, 1), commissionAgreement.EffectiveDate);
		}

		public void TestEffectiveDateWithDifferentCompanies()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company1.GC_Code = "C1";
			company2.GC_Code = "C2";
			branch1.GB_Code = "B1";
			branch2.GB_Code = "B2";
			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;

			Factory.Save();

			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var commissionAgreement = opportunity.CommissionAgreements.AddNew();
			commissionAgreement.CA0_OH_Customer = org.PK;

			commissionAgreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			AssertEquals(true, commissionAgreement.EffectiveDateInfo.ReadOnly);
			AssertEquals(ZDateTime.Empty, commissionAgreement.EffectiveDate);

			var arInvoice = CreateNewARInvoice(org);
			arInvoice.AH_PostDate = new ZDateTime(2000, 10, 10);
			var apInvoice = CreateNewAPInvoice(org);
			apInvoice.AH_PostDate = new ZDateTime(2000, 5, 5);
			AssertEquals(new ZDateTime(2000, 10, 10), commissionAgreement.EffectiveDate);

			var arInvoice2 = CreateNewARInvoice(org);
			arInvoice2.AH_PostDate = new ZDateTime(1999, 9, 9);
			AssertEquals(new ZDateTime(1999, 9, 9), commissionAgreement.EffectiveDate);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals(new ZDateTime(1999, 9, 9), commissionAgreement.EffectiveDate);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals(new ZDateTime(1999, 9, 9), commissionAgreement.EffectiveDate);
			}
		}

		public void TestCA0_CommissionTriggerType_ClearedWhenTriggerChanged()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = new ZDate(2000, 10, 10);

			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			AssertEquals(ZDateTime.Empty, agreement.CA0_EffectiveDate);
		}

		#endregion

		#region Status

		[TestDate(2001, 1, 1)]
		public void TestStatus()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = ZDate.Empty;
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Inactive, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Inactive, agreement.StatusDescription);

			bool statusPropertyInfoChangedEventRaised;
			agreement.StatusInfo.ValueChanged += (sender, e) =>
			{
				statusPropertyInfoChangedEventRaised = true;
			};

			statusPropertyInfoChangedEventRaised = false;
			agreement.CA0_EffectiveDate = new ZDate(2001, 1, 1);
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Active, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Active, agreement.StatusDescription);
			Assert("statusPropertyInfoChangedEventRaised", statusPropertyInfoChangedEventRaised);

			statusPropertyInfoChangedEventRaised = false;
			agreement.Expire(new ZDate(2002, 2, 2));
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Active, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Active, agreement.StatusDescription);

			statusPropertyInfoChangedEventRaised = false;
			agreement.Expire(new ZDate(2001, 1, 1));
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Expired, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Expired, agreement.StatusDescription);
			Assert("statusPropertyInfoChangedEventRaised", statusPropertyInfoChangedEventRaised);

			statusPropertyInfoChangedEventRaised = false;
			agreement.Reverse();
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Reversed, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Reversed, agreement.StatusDescription);
			Assert("statusPropertyInfoChangedEventRaised", statusPropertyInfoChangedEventRaised);
		}

		[TestDate(2001, 1, 1)]
		public void TestStatusQueued()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = new ZDate(2001, 1, 1);
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Active, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Active, agreement.StatusDescription);

			var queue = Factory.NewWithValidTestData<OrgCommissionCalculationQueue>();
			queue.CAQ_CA0 = agreement.PK;

			AssertEquals(OrgCommissionAgreementStatusList.Codes.Active, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Active + ", Queued", agreement.StatusDescription);

			agreement.Expire(new ZDate(2001, 1, 1));
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Expired, agreement.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Expired + ", Queued", agreement.StatusDescription);
		}

		[TestDate(2001, 1, 1)]
		public void TestStatusQueued_Draft()
		{
			var parent = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var draft = parent.CreateDraft();
			draft.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			draft.CA0_EffectiveDate = ZDate.Empty;

			Factory.Save();

			draft.CA0_EffectiveDate = new ZDate(2001, 1, 1);
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Active, draft.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Active, draft.StatusDescription);

			var queue = Factory.NewWithValidTestData<OrgCommissionCalculationQueue>();
			queue.CAQ_CA0 = parent.PK;

			Factory.Save();

			draft = new BusinessObjectFactory().Load<OrgCommissionAgreement>(draft.PK);

			AssertEquals(OrgCommissionAgreementStatusList.Codes.Active, draft.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Active + ", Queued", draft.StatusDescription);

			draft.Expire(new ZDate(2001, 1, 1));
			AssertEquals(OrgCommissionAgreementStatusList.Codes.Expired, draft.Status);
			AssertEquals(OrgCommissionAgreementStatusList.Descriptions.Expired + ", Queued", draft.StatusDescription);
		}

		#endregion

		#endregion

		#region Draft

		public void TestCreateDraft_Uncommitted()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();
			OrgCommissionAgreementTestHelper.AddExclusionCommissionAgreementOverallItem(agreement, "XXX", "XXX", "XXX");
			OrgCommissionAgreementTestHelper.AddExclusionCommissionAgreementOverallItem(agreement, "YYY", "YYY", "YYY");
			agreement.Recipients.AddNew().FillWithValidTestData();
			agreement.Recipients.AddNew().FillWithValidTestData();

			Factory.Save();

			var draft = agreement.CreateDraft(true);

			CombineAssertions(() =>
			{
				AssertEquals("draft.IsDraft", true, draft.IsDraft);
				AssertEquals("draft.ProductItems.Count", 2, draft.ProductItems.Count);
				AssertEquals("draft.Recipients.Count", 2, draft.Recipients.Count);
				AssertEquals("draft.UncommittedParentVersion", agreement, draft.UncommittedParentVersion);
				AssertEquals("draft.CA0_CA0_ParentVersion", ZGuid.Empty, draft.CA0_CA0_ParentVersion);
				AssertEquals("draft.HasChanges", false, draft.HasChanges);

				AssertEquals("agreement.HasChanges", false, agreement.HasChanges);
			});
		}

		public void TestCreateDraft_Committed()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();
			OrgCommissionAgreementTestHelper.AddExclusionCommissionAgreementOverallItem(agreement, "XXX", "XXX", "XXX");
			OrgCommissionAgreementTestHelper.AddExclusionCommissionAgreementOverallItem(agreement, "YYY", "YYY", "YYY");
			agreement.Recipients.AddNew().FillWithValidTestData();
			agreement.Recipients.AddNew().FillWithValidTestData();

			Factory.Save();

			var draft = agreement.CreateDraft();

			CombineAssertions(() =>
			{
				AssertEquals("draft.IsDraft", true, draft.IsDraft);
				AssertEquals("draft.ProductItems.Count", 2, draft.ProductItems.Count);
				AssertEquals("draft.Recipients.Count", 2, draft.Recipients.Count);
				AssertEquals("draft.IsUncommittedDraft", false, draft.IsUncommittedDraft);
				AssertEquals("draft.CA0_CA0_ParentVersion", agreement.PK, draft.CA0_CA0_ParentVersion);
				AssertEquals("draft.HasChanges", false, draft.HasChanges);
			});
		}

		#region Approve

		[TestDate(2002, 2, 2)]
		public void TestApproveDraft_WithPreviouslyApprovedAgreement()
		{
			var originalAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			originalAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			var xxxItem = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(originalAgreement, "XXX", "XXX", "XXX");
			var adlRecipient = originalAgreement.Recipients.AddNew();
			adlRecipient.CAR_GS_NKStaff = "ADL";
			adlRecipient.CAR_Comment = "Original Comment";

			Factory.Save();

			var draftAgreement = originalAgreement.CreateDraft();
			draftAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			draftAgreement.Recipients.First(x => x.CAR_GS_NKStaff == "ADL").CAR_Comment = "New Comment";
			var yyyItem = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(draftAgreement, "YYY", "YYY", "YYY");
			var risRecipient = draftAgreement.Recipients.AddNew();
			risRecipient.CAR_GS_NKStaff = "RIS";

			draftAgreement.ApproveDraft();

			CombineAssertions(() =>
			{
				AssertEquals("originalAgreement.IsDraft", false, originalAgreement.IsDraft);
				AssertEquals("originalAgreement.CA0_LastApprovedDateUtc", new ZDateTime(2002, 2, 2), originalAgreement.CA0_LastApprovedDateUtc);
				AssertEquals("originalAgreement.IsDeleted", false, originalAgreement.IsDeleted);
				AssertEquals("draftAgreement.IsDeleted", true, draftAgreement.IsDeleted);

				AssertEquals("originalAgreement.CA0_CommissionBasis", CommissionBasisType.Codes.REV, originalAgreement.CA0_CommissionBasis);
				AssertEquals("adlRecipient.CAR_Comment", "New Comment", adlRecipient.CAR_Comment);
				AssertEquals("originalAgreement.Recipients.Count", 2, originalAgreement.Recipients.Count);
				AssertEquals("originalAgreement.ProductItems.Count", 2, originalAgreement.ProductItems.Count);
			});

			AssertNotNull(originalAgreement.Logs.MostRecentLog);
			AssertEquals("Approved:", originalAgreement.Logs.MostRecentLog.SL_Reference);
		}

		public void TestMergeDraft_DeleteRecipientWithCommisions()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			var rate = recipient.Rates.AddNew();

			var commissionLine = Factory.New<IAccCommissionLine>();
			commissionLine.CL0_CAT = rate.PK;
			((BusinessObject)commissionLine).FillWithValidTestData();
			Factory.Save();

			AssertEquals("precondition (recipient)", 1, agreement.Recipients.Count);
			AssertEquals("precondition (rate)", 1, recipient.Rates.Count);
			AssertNotNullOrEmpty("precondition (agreementId)", agreement.AgreementId);

			var agreementDraft = agreement.CreateDraft();
			agreementDraft.Recipients.DeleteAll();

			var ex = AssertExceptionThrown<CommissionAgreementApprovalException>(() => agreementDraft.ApproveDraft());

			AssertContains("cannot be removed", ex.Message);
			AssertContains(recipient.CommissionAgreement.AgreementId, ex.Message);
			AssertContains(recipient.PK.ToString(), ex.Message);

			AssertContains("Commission transactions lines have been processed under the previously approved agreement conditions.", ex.UserFriendlyMessage);
			AssertContains(recipient.CommissionAgreement.AgreementId, ex.UserFriendlyMessage);
		}

		[TestDate(2002, 2, 2)]
		public void TestApproveDraft_WithNoPreviouslyApprovedAgreement()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			agreement.ApproveDraft();

			CombineAssertions(() =>
			{
				AssertEquals("IsDraft", false, agreement.IsDraft);
				AssertEquals("CA0_LastApprovedDateUtc", new ZDateTime(2002, 2, 2), agreement.CA0_LastApprovedDateUtc);
			});

			AssertNotNull(agreement.Logs.MostRecentLog);
			AssertEquals("Approved:", agreement.Logs.MostRecentLog.SL_Reference);
		}

		#endregion

		#region Disapprove

		[TestDate(2002, 2, 2)]
		public void TestDisapproveDraft_WithPreviouslyApprovedAgreement()
		{
			var originalAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			originalAgreement.CA0_LastApprovedDateUtc = new ZDateTime(2001, 1, 1);

			Factory.Save();

			var draftAgreement = originalAgreement.CreateDraft();
			draftAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;

			Factory.Save();

			draftAgreement.DisapproveDraft();

			CombineAssertions(() =>
			{
				AssertEquals("originalAgreement.CA0_LastApprovedDateUtc", new ZDateTime(2001, 1, 1), originalAgreement.CA0_LastApprovedDateUtc);
				AssertEquals("draftAgreement.IsDeleted", true, draftAgreement.IsDeleted);
			});

			AssertEquals("Disapproved", originalAgreement.Logs.LogsNotInDB[0].SL_Reference);
			AssertEquals(1, originalAgreement.Logs.LogsNotInDB.Length);
		}

		public void TestDisapproveDraft_WithNoPreviouslyApprovedAgreement()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			agreement.DisapproveDraft();

			AssertEquals(true, agreement.IsDeleted);
		}

		#endregion

		#endregion

		#region Security

		public void TestIsRecipientViewAdditionalInformationAllowed()
		{
			Env.Security.CommissionAgreementViewAny.IsAllowed = false;

			var primarySalesStaff = Factory.NewWithValidTestData<GlbStaff>();
			primarySalesStaff.GS_Code = "ADL";
			var agreementRecipientStaff = Factory.NewWithValidTestData<GlbStaff>();
			agreementRecipientStaff.GS_Code = "SCW";
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = "ADL";
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			agreement.FillWithValidTestData();
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "SCW";

			AssertEquals(true, agreement.IsViewRecipientsAdditionalInformationAllowed);

			Factory.Save();

			AssertEquals(false, agreement.IsViewRecipientsAdditionalInformationAllowed);

			using (Env.SetTemporaryUserContext(primarySalesStaff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals(true, agreement.IsViewRecipientsAdditionalInformationAllowed);
			}

			using (Env.SetTemporaryUserContext(agreementRecipientStaff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals(true, agreement.IsViewRecipientsAdditionalInformationAllowed);
			}

			Env.Security.CommissionAgreementViewAny.IsAllowed = true;
			AssertEquals(true, agreement.IsViewRecipientsAdditionalInformationAllowed);
		}

		public void TestExpireAgreementSecurityCheckpoint()
		{
			AssertEquals(Env.Security.CommissionAgreementOverrideAny, OrgCommissionAgreement.ExpireAgreementSecurityCheckpoint);
		}

		public void TestReverseAgreementSecurityCheckpoint()
		{
			AssertEquals(Env.Security.CommissionAgreementOverrideAny, OrgCommissionAgreement.ReverseAgreementSecurityCheckpoint);
		}

		#endregion

		#region Save

		[TestDate(2002, 2, 2)]
		public void TestAutoApproveNewAgreement()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertAutoApproveNewAgreement("Should auto-approve agreements without an effective date", ZDate.Empty, true);
			AssertAutoApproveNewAgreement("Should never auto-approve agreements that are effective in the past", new ZDate(2001, 1, 1), false);
			AssertAutoApproveNewAgreement("Should auto-approve agreements in the future", new ZDate(2003, 3, 3), true);

			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertAutoApproveNewAgreement("Should not auto-approve agreement because registry set to false", ZDate.Empty, false);
			AssertAutoApproveNewAgreement("Should never auto-approve agreements that are effective in the past", new ZDate(2001, 1, 1), false);
			AssertAutoApproveNewAgreement("Should not auto-approve agreement because registry set to false", new ZDate(2003, 3, 3), false);
		}

		void AssertAutoApproveNewAgreement(string message, ZDate effectiveDate, bool expectedShouldAutoApprove)
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = effectiveDate;
			Factory.Save();

			AssertEquals(message, !expectedShouldAutoApprove, agreement.IsDraft);
		}

		[TestDate(2002, 2, 2)]
		public void TestAutoApproveExistingAgreement()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestDateAttribute.Date = new DateTime(2003, 1, 1);
			AssertAutoApproveExistingAgreement("Should always auto-approve name changes", (x) => x.CA0_Name = "New", true);
			AssertAutoApproveExistingAgreement("Should not auto-approve when effective date in the past", (x) => x.CA0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK, false);
			AssertAutoApproveExistingAgreement("Should not auto-approve when effective date in the past", (x) => x.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice, false);
			AssertAutoApproveExistingAgreement("Should not auto-approve when effective date in the past", (x) => x.CA0_CommissionBasis = CommissionBasisType.Codes.PRF, false);
			AssertAutoApproveExistingAgreement("Should not auto-approve when effective date in the past", (x) => x.CA0_EffectiveDate = new ZDate(2004, 1, 1), false);
			AssertAutoApproveExistingAgreement("Should auto-approve since expired date in the future", (x) => x.CA0_ExpiredDate = new ZDate(2004, 1, 1), true);
			AssertAutoApproveExistingAgreement("Should not auto-approve since expired date in the past", (x) => x.CA0_ExpiredDate = new ZDate(2001, 1, 1), false);
			AssertAutoApproveExistingAgreement("Should never auto-approve agreement reversals", (x) => x.CA0_ReversedDateUtc = new ZDateTime(2002, 2, 2), false);

			AssertAutoApproveExistingAgreement("Should not auto-approve when effective date in the past", (x) => x.ProductItems.AddNew(true, "ALL"), false);
			AssertAutoApproveExistingAgreement("Should not auto-approve when effective date in the past", (x) => x.Recipients.AddNew(Factory.NewWithValidTestData<GlbStaff>()), false);

			TestDateAttribute.Date = new DateTime(2001, 1, 1);
			AssertAutoApproveExistingAgreement("Should always auto-approve name changes", (x) => x.CA0_Name = "New", true);
			AssertAutoApproveExistingAgreement("Should auto-approve when effective date in the future", (x) => x.CA0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK, true);
			AssertAutoApproveExistingAgreement("Should auto-approve when effective date in the future", (x) => x.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice, true);
			AssertAutoApproveExistingAgreement("Should auto-approve when effective date in the future", (x) => x.CA0_CommissionBasis = CommissionBasisType.Codes.PRF, true);
			AssertAutoApproveExistingAgreement("Should auto-approve when effective date in the future", (x) => x.CA0_EffectiveDate = new ZDate(2004, 1, 1), true);
			AssertAutoApproveExistingAgreement("Should auto-approve since expired date in the future", (x) => x.CA0_ExpiredDate = new ZDate(2004, 1, 1), true);
			AssertAutoApproveExistingAgreement("Should not auto-approve since expired date in the past", (x) => x.CA0_ExpiredDate = new ZDate(2000, 1, 1), false);
			AssertAutoApproveExistingAgreement("Should never auto-approve agreement reversals", (x) => x.CA0_ReversedDateUtc = new ZDateTime(2002, 2, 2), false);

			AssertAutoApproveExistingAgreement("Should auto-approve when effective date in the future", (x) => x.ProductItems.AddNew(true, "ALL"), true);
			AssertAutoApproveExistingAgreement("Should auto-approve when effective date in the future", (x) => x.Recipients.AddNew(Factory.NewWithValidTestData<GlbStaff>()), true);
		}

		void AssertAutoApproveExistingAgreement(string message, Action<OrgCommissionAgreement> modifyAgreementAction, bool expectedShouldAutoApprove)
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.CA0_LastApprovedDateUtc = new ZDate(2002, 2, 2);
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			agreement.CA0_EffectiveDate = new ZDate(2002, 2, 2);
			Factory.Save();

			var agreementDraft = agreement.CreateDraft(true);
			modifyAgreementAction(agreementDraft);
			Factory.Save();

			AssertEquals(message, expectedShouldAutoApprove, agreementDraft.IsDeleted);
			AssertEquals(message, expectedShouldAutoApprove ? null : agreementDraft, agreement.Draft);
		}

		#endregion

		#region Delete

		public void TestDelete_DeletesParentAgreement()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			var agreementDraft = agreement.CreateDraft();

			agreementDraft.Delete();

			AssertEquals("agreementDraft.IsDeleted", true, agreementDraft.IsDeleted);
			AssertEquals("agreement.IsDeleted", true, agreement.IsDeleted);
		}

		public void TestCanDelete()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.CA0_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			agreement.FillWithValidTestData();

			Factory.Save();

			AssertEquals(true, agreement.CanDelete);

			agreement.CA0_FirstUsageDateUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();

			var reloadedAgreement = new BusinessObjectFactory().Load<OrgCommissionAgreement>(agreement.PK);
			AssertEquals(false, reloadedAgreement.CanDelete);
			AssertEquals(string.Format("You can not delete {0} because it has already been used for a commission pay out. If you wish to disable the agreement, select the \"Disable\" option from the context menu. If you wish to revert all commission payments, select the \"Reverse\" option from the context menu.", agreement.HumanReadableName), reloadedAgreement.ReasonForNotAbleToDelete.ToString());

			reloadedAgreement.Expire(new ZDate(2000, 1, 1));
			AssertEquals(false, reloadedAgreement.CanDelete);
			AssertEquals(string.Format("You can not delete {0} because it has already been used for a commission pay out. If you wish to revert all commission payments, select the \"Reverse\" option from the context menu.", agreement.HumanReadableName), reloadedAgreement.ReasonForNotAbleToDelete.ToString());

			reloadedAgreement.Reverse();
			AssertEquals(false, reloadedAgreement.CanDelete);
			AssertEquals(string.Format("You can not delete {0} because it has already been used for a commission pay out.", agreement.HumanReadableName), reloadedAgreement.ReasonForNotAbleToDelete.ToString());

			reloadedAgreement.CA0_ExpiredDate = ZDate.Empty;
			AssertEquals(false, reloadedAgreement.CanDelete);
			AssertEquals(string.Format("You can not delete {0} because it has already been used for a commission pay out.", agreement.HumanReadableName), reloadedAgreement.ReasonForNotAbleToDelete.ToString());

			var draftAgreement = agreement.CreateDraft();
			AssertEquals(false, draftAgreement.CanDelete);
			AssertEquals(string.Format("You can not delete {0} because it has already been used for a commission pay out.", agreement.HumanReadableName), reloadedAgreement.ReasonForNotAbleToDelete.ToString());
		}

		#endregion

		#region Logs

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();
			agreement.Recipients.DeleteAll();
			AssertEquals(0, agreement.BusinessObjectsWithRelatedEvents.Length);

			var productItem = agreement.ProductItems.AddNew();
			productItem.ChildServiceItems.DeleteAll();
			AssertEquals(1, agreement.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals(true, agreement.BusinessObjectsWithRelatedEvents.Contains(productItem));

			var serviceItem = productItem.ChildServiceItems.AddNew();
			serviceItem.ChildSubModuleItems.DeleteAll();
			AssertEquals(2, agreement.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals(true, agreement.BusinessObjectsWithRelatedEvents.Contains(serviceItem));

			var recipient = agreement.Recipients.AddNew();
			recipient.FillWithValidTestData();
			AssertEquals(3, agreement.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals(true, agreement.BusinessObjectsWithRelatedEvents.Contains(recipient));

			var rate = recipient.Rates.AddNew();
			rate.FillWithValidTestData();
			AssertEquals(4, agreement.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals(true, agreement.BusinessObjectsWithRelatedEvents.Contains(rate));

			var agreementDraft = agreement.CreateDraft();
			AssertEquals("Should not include draft nor its child objects", 4, agreement.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals("Should not include draft nor its child objects", false, agreement.BusinessObjectsWithRelatedEvents.Contains(agreementDraft));

			var newDraftProductItem = agreementDraft.ProductItems.AddNew();
			var newDraftServiceItem = newDraftProductItem.ChildServiceItems.Single();
			var newDraftSubModuleItem = newDraftServiceItem.ChildSubModuleItems.Single();
			AssertEquals("Should include *NEW* draft child objects", 7, agreement.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals("Should include *NEW* draft child objects", true, agreement.BusinessObjectsWithRelatedEvents.Contains(newDraftProductItem));
			AssertEquals("Should include children of new draft child objects", true, agreement.BusinessObjectsWithRelatedEvents.Contains(newDraftServiceItem));
			AssertEquals("Should include children of new draft child objects", true, agreement.BusinessObjectsWithRelatedEvents.Contains(newDraftSubModuleItem));

			var newDraftRateForExistingRecipient = agreementDraft.Recipients.First().Rates.AddNew();
			AssertEquals("Should include *NEW* draft objects of sub-children", 8, agreement.BusinessObjectsWithRelatedEvents.Length);
			AssertEquals("Should include *NEW* draft objects of sub-children", true, agreement.BusinessObjectsWithRelatedEvents.Contains(newDraftRateForExistingRecipient));
		}

		public void TestModifiedLogs_Properties()
		{
			var orgAAA = Factory.NewWithValidTestData<OrgHeader>();
			orgAAA.OH_Code = "AAA";
			var orgBBB = Factory.NewWithValidTestData<OrgHeader>();
			orgBBB.OH_Code = "BBB";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_Name = "#1";
			agreement.CA0_OH_Customer = orgAAA.PK;
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			agreement.CA0_EffectiveDate = ZDate.Empty;
			agreement.CA0_ReversedDateUtc = ZDate.Empty;

			Factory.Save();
			var modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals("Should not create modified logs on first save of agreement", 0, modifiedLogs.Length);

			agreement.CA0_OH_Customer = orgBBB.PK;
			Factory.Save();
			modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("Customer: AAA > BBB", modifiedLogs[0].SL_Reference);

			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.REV;
			Factory.Save();
			modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("Commission Basis: PRF > REV", modifiedLogs[1].SL_Reference);

			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.EffectiveDate = new ZDate(2000, 1, 1);
			Factory.Save();
			modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals(4, modifiedLogs.Length);
			AssertEquals("Effective Trigger: 1AR > MAN", modifiedLogs[2].SL_Reference);
			AssertEquals("Effective Date:  > 01-Jan-00", modifiedLogs[3].SL_Reference);

			agreement.CA0_ReversedDateUtc = new ZDateTime(2003, 3, 3);
			Factory.Save();
			modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals(5, modifiedLogs.Length);
			AssertEquals("Reversed", modifiedLogs[4].SL_Reference);
		}

		public void TestModifiedLogs_AgreementItems()
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
				var agreement = opportunity.CommissionAgreements.AddNew();
				agreement.FillWithValidTestData();
				OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement, "SHP", "ALL", "ALL");

				Factory.Save();

				var inclusionItem = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "CLL", "ALL", "ALL");
				Factory.Save();
				var modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
				AssertEquals("Item Inclusion Added: CLL > ALL > ALL", modifiedLogs[0].SL_Reference);

				var exclusionItem = agreement.ProductItems.First(x => x.CAI_Code == "CLL").ChildServiceItems.AddNew(false, "XXX");
				Factory.Save();
				modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
				AssertEquals(2, modifiedLogs.Length);
				AssertEquals("Item Exclusion Added: CLL > XXX", modifiedLogs[1].SL_Reference);

				inclusionItem.Delete();
				Factory.Save();
				modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
				AssertEquals(3, modifiedLogs.Length);
				AssertEquals("Item Inclusion Deleted: CLL > ALL > ALL", modifiedLogs[2].SL_Reference);

				exclusionItem.Delete();
				Factory.Save();
				modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
				AssertEquals(4, modifiedLogs.Length);
				AssertEquals("Item Exclusion Deleted: CLL > XXX", modifiedLogs[3].SL_Reference);
			}
		}

		public void TestIsEditable()
		{
			var approved = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var unapproved = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			approved.Approve();

			void AssertIsEditable(bool approvedCommissionAgreementEditAllowed, bool unapprovedCommissionAgreementEditAllowed)
			{
				AssertEquals(approvedCommissionAgreementEditAllowed, approved.IsEditable);
				AssertEquals(unapprovedCommissionAgreementEditAllowed, unapproved.IsEditable);
			}

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = false;
			AssertIsEditable(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = true;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = false;
			AssertIsEditable(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = true;
			AssertIsEditable(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = true;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = true;
			AssertIsEditable(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);
		}

		public void TestApprovedLogs_AutoApprovedOnSave()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var orgAAA = Factory.NewWithValidTestData<OrgHeader>();
			orgAAA.OH_Code = "AAA";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_Name = "#1";
			agreement.CA0_OH_Customer = orgAAA.PK;
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			agreement.CA0_EffectiveDate = ZDate.Empty;
			agreement.CA0_ReversedDateUtc = ZDate.Empty;

			Factory.Save();

			Assert("Auto approved on save log should exist.", agreement.Logs.Find(x => x.SL_Reference == "Approved: Auto Approved On Save").Any());
		}

		public void TestApprovedLogs_ApprovedByApprovalWizard()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var orgAAA = Factory.NewWithValidTestData<OrgHeader>();
			orgAAA.OH_Code = "AAA";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.CA0_Name = "#1";
			agreement.CA0_OH_Customer = orgAAA.PK;
			agreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			agreement.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			agreement.CA0_EffectiveDate = ZDate.Empty;
			agreement.CA0_ReversedDateUtc = ZDate.Empty;
			agreement.NotifyHasChangedPreventingAutoApproval();

			Factory.Save();

			agreement.Approve("Test Log Message");

			Assert("Custom approval information should show in logs.", agreement.Logs.Find(x => x.SL_Reference == "Approved: Test Log Message").Any());
		}

		#endregion

		#region IReadOnlySecurity Members

		public void TestReadOnlyWhenReversed()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			AssertEquals(false, agreement.CA0_OH_CustomerInfo.ReadOnly);
			AssertEquals(false, agreement.CA0_CommissionBasisInfo.ReadOnly);
			AssertEquals(false, agreement.CA0_CommissionTriggerTypeInfo.ReadOnly);
			AssertEquals(false, agreement.ProductItems.ReadOnly);
			AssertEquals(false, agreement.Recipients.ReadOnly);

			agreement.Reverse();

			AssertEquals(true, agreement.CA0_OH_CustomerInfo.ReadOnly);
			AssertEquals(true, agreement.CA0_CommissionBasisInfo.ReadOnly);
			AssertEquals(true, agreement.CA0_CommissionTriggerTypeInfo.ReadOnly);
			AssertEquals(true, agreement.ProductItems.ReadOnly);
			AssertEquals(true, agreement.Recipients.ReadOnly);
		}

		#endregion

		#region Implementation

		AccTransactionHeader CreateNewAPInvoice(OrgHeader org)
		{
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = org.PK;
			return invoice;
		}

		AccTransactionHeader CreateNewARInvoice(OrgHeader org)
		{
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = org.PK;
			return invoice;
		}

		AccTransactionLines CreateNewInvoiceLine(AccTransactionHeader header, ZDateTime reverseDate)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AC = chargeCode.PK;
			transactionLine.AL_AH = header.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_ReverseDate = reverseDate;
			transactionLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			return transactionLine;
		}

		#endregion
	}
}
