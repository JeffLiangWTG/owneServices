using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunity))]
	public class OrgOpportunityTest : EnterpriseBusinessObjectTestCase
	{
		public virtual void TestUpdateEstimatedValue()
		{
			var opportunity = CreateOpportunity(false);

			var mockProspectiveSalesHeaderCollection = new Mock<ISalesHeaderCollection>();
			mockProspectiveSalesHeaderCollection.Setup(m => m.TotalValue).Returns(1000);
			var mockBuilderMock = new Mock<ISalesHeaderCollectionBuilder>();
			mockBuilderMock.Setup(m => m.New(opportunity, false)).Returns(mockProspectiveSalesHeaderCollection.Object);

			using (ObjectFactory.Substitute(mockBuilderMock.Object))
			{
				opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";

				opportunity.ValueItems.AddNew().PV_Value = 10;
				opportunity.UpdateEstimatedValue();

				AssertEquals(10 * 12 + 1000m, opportunity.P8_EstimatedValue);
			}
		}

		public void TestDocumentSupporter()
		{
			var opportunity = CreateOpportunity(false);
			AssertEquals(typeof(OrgOpportunityDocumentSupporter), opportunity.DocumentSupporter.GetType());
		}

		#region IJobNumber

		public void TestJobNumber()
		{
			var opportunity = CreateOpportunity(false);
			opportunity.P8_OpportunityID = "O00001001";
			AssertEquals("O00001001", ((IJobNumber)opportunity).JobNumber);
		}

		#endregion

		#region CreateCommissionAgreementsAllowed

		public void TestCommissionAgreementForEdit_IsRegisteredEditableChildObject()
		{
			var opportunity = CreateOpportunity(true);
			AssertEquals(true, opportunity.HasChanges);
			Factory.Save();
			AssertEquals(false, opportunity.HasChanges);

			var agreements = opportunity.CommissionAgreementsForEdit.AddNew();
			AssertEquals(true, opportunity.IsRegisteredEditableChildObject(opportunity.CommissionAgreementsForEdit));
			AssertEquals(true, opportunity.HasChanges);
			agreements.FillWithValidTestData();
			Factory.Save();
			AssertEquals(false, opportunity.HasChanges);

			opportunity.CommissionAgreementsForEdit[0].CA0_CommissionStream = "AAA";
			AssertEquals(true, opportunity.HasChanges);
		}

		public void TestCreateCommissionAgreementsAllowed()
		{
			var opportunity = CreateOpportunity(true);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.CommissionAgreementOverrideAny.IsAllowed = false;

			AssertEquals(false, opportunity.CreateCommissionAgreementsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = true;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = true;

			AssertEquals("CreateCommissionAgreementsAllowed should be true since the opportunity has not been saved yet.", true, opportunity.CreateCommissionAgreementsAllowed);

			Factory.Save();

			AssertEquals("CreateCommissionAgreementsAllowed should be false since the opportunit was saved.", false, opportunity.CreateCommissionAgreementsAllowed);

			opportunity.P8_GS_NKPrimarySalesPerson = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("CreateCommissionAgreementsAllowed should be true since the PrimarySalesPerson is the current user.", true, opportunity.CreateCommissionAgreementsAllowed);

			opportunity.P8_GS_NKPrimarySalesPerson = "XXX";
			AssertEquals("CreateCommissionAgreementsAllowed should be true since the PrimarySalesPerson is not the current user.", false, opportunity.CreateCommissionAgreementsAllowed);

			Env.Security.CommissionAgreementOverrideAny.IsAllowed = true;
			AssertEquals("CreateCommissionAgreementsAllowed should be true since the current user has the CommissionAgreementOverrideAny security option.", true, opportunity.CreateCommissionAgreementsAllowed);
		}

		#endregion

		[TestDate(2015, 5, 5, 10, 10, 0)]
		public virtual void TestDefaultValues()
		{
			var opportunity = CreateOpportunity(false);
			AssertEquals(GlbCompany.CurrentCompany.PK, opportunity.P8_GC);
			AssertEquals(new ZDateTime(2015, 5, 5), opportunity.P8_DateForExchangeRate);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, opportunity.P8_RX_NKEstimatedValueCurrency);
		}

		public void TestP8_GC_ReadOnly()
		{
			var opportunity = CreateOpportunity(false);
			AssertEquals(true, opportunity.P8_GCInfo.ReadOnly);
		}

		public void TestP8_DateForExchangeRate_ReadOnly()
		{
			var opportunity = CreateOpportunity(false);
			AssertEquals(true, opportunity.P8_DateForExchangeRateInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public virtual void TestP8_RX_NKEstimatedValueCurrency_ShouldValidateSalesHeaderCollectionCurrenciesOnChange()
		{
			var opportunity = CreateOpportunity(false);
			opportunity.P8_RX_NKEstimatedValueCurrency = "AUD";

			var mockSalesHeaderCollection = new Mock<ISalesHeaderCollection>();
			var mockBuilderMock = new Mock<ISalesHeaderCollectionBuilder>();
			mockBuilderMock.Setup(m => m.New(opportunity, false)).Returns(mockSalesHeaderCollection.Object);

			using (ObjectFactory.Substitute(mockBuilderMock.Object))
			{
				opportunity.P8_RX_NKEstimatedValueCurrency = "USD";
				mockSalesHeaderCollection.Verify(m => m.ValidateAllCurrencies());
			}
		}

		public void TestP8_OH_ShouldRefreshCommissionRuleDefaultsOfAllNonSavedStaffRecipients()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			var staffGlobalRule = staff.CommissionRules.AddNew();
			staffGlobalRule.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			staffGlobalRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			staffGlobalRule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			staffGlobalRule.FillWithValidTestData();
			var staffGlobalRate = staffGlobalRule.Rates.AddNew();
			staffGlobalRate.ACT_CommissionType = CommissionTypes.Codes.PCT;
			staffGlobalRate.FillWithValidTestData();

			var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var auTeam = Factory.NewWithValidTestData<SalesTeam>();
			auTeam.CoveredCountries.Add(auCountry);
			var auRule = auTeam.CommissionRules.AddNew();
			auRule.ACM_Product = CommissionRuleLookups.AnyProductsCode;
			auRule.ACM_Service = CommissionRuleLookups.AnyServicesCode;
			auRule.ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
			auRule.FillWithValidTestData();
			var auRate = auRule.Rates.AddNew();
			auRate.ACT_CommissionType = CommissionTypes.Codes.FIX;
			auRate.FillWithValidTestData();
			auTeam.Staff.Add(staff);

			Factory.Save();

			var opportunity = CreateOpportunity(true);
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement,
				OrgCommissionAgreementItemLookups.AllProductsCode,
				OrgCommissionAgreementItemLookups.AllServicesCode,
				OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";

			AssertEquals("Should have set default recipient rates from staffGlobalRate", CommissionTypes.Codes.PCT, recipient.CAR_CommissionType);

			var auOrg = Factory.NewWithValidTestData<OrgHeader>();
			auOrg.OH_RL_NKClosestPort = "AUSYD";
			opportunity.P8_OH = auOrg.PK;

			AssertEquals("Should have updated recipient default rates from auRate", CommissionTypes.Codes.FIX, recipient.CAR_CommissionType);
		}

		public void TestP8_GS_NKPrimarySalesPerson_ReadOnlyIfNotSameAsCurrentLoginAndNoOverridingSecurityRight()
		{
			Env.Security.CommissionAgreementOverrideAny.IsAllowed = false;

			var primarySalesRep = Factory.NewWithValidTestData<GlbStaff>();
			primarySalesRep.GS_Code = "PSR";
			var anotherSalesRep = Factory.NewWithValidTestData<GlbStaff>();
			anotherSalesRep.GS_Code = "ASR";
			var opportunity = CreateOpportunity(true);
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();

			AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

			opportunity.P8_GS_NKPrimarySalesPerson = "PSR";
			AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

			Factory.Save();
			AssertEquals(true, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

			using (Env.SetTemporaryUserContext(primarySalesRep.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.OpportunityManagementEditModifyStaffAssignment.IsAllowed = true;

				AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

				opportunity.P8_GS_NKPrimarySalesPerson = "ASR";
				AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

				Factory.Save();
				AssertEquals(true, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);
			}

			Env.Security.CommissionAgreementOverrideAny.IsAllowed = true;
			AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);
		}

		public void TestP8_GS_NKPrimarySalesPerson_ReadOnly()
		{
			Env.Security.OpportunityManagementEditModifyStaffAssignment.IsAllowed = true;
			var opportunity = CreateOpportunity(true);
			AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

			Env.Security.OpportunityManagementEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

			Factory.Save();

			Env.Security.OpportunityManagementEditModifyStaffAssignment.IsAllowed = true;
			AssertEquals(false, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);

			Env.Security.OpportunityManagementEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(true, opportunity.P8_GS_NKPrimarySalesPersonInfo.ReadOnly);
		}

		public void TestRecallDateUpdatedLog()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "DEF";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "GHI";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "President Alex";
			Factory.Save();

			var opportunity = CreateOpportunity(true);
			opportunity.P8_RecallDate = new ZDateTime(2020, 10, 13, 13, 30, 0);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Factory.Save();
			}

			var eventCodeQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.RecallDateUpdated.Code);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Parent, opportunity.PK);

			var auditedLogs = opportunity.Logs.Find(eventCodeQuery);

			AssertEquals("Should be one log entry", 1, auditedLogs.Length);
			AssertEquals("Logged in User", "ABC", auditedLogs[0].SL_GS_NKUser);
			AssertEquals("Logged in Branch", "DEF", auditedLogs[0].SL_GB_NKBranch);
			AssertEquals("Logged in Department", "GHI", auditedLogs[0].SL_GE_NKDepartment);
			AssertEquals("Log reference", $"Opportunity {opportunity.P8_OpportunityID} Recall Date Updated|NEW=20201013T133000Z", auditedLogs[0].SL_Reference);

			opportunity.P8_EstimatedValue = 2000;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Factory.Save();
			}

			auditedLogs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("Should be one log entry", 1, auditedLogs.Length);

			opportunity.P8_RecallDate = new ZDateTime(2021, 11, 12, 14, 45, 0);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Factory.Save();
			}

			auditedLogs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("Should be two log entries", 2, auditedLogs.Length);

			AssertEquals("Logged in User", "ABC", auditedLogs[0].SL_GS_NKUser);
			AssertEquals("Logged in Branch", "DEF", auditedLogs[0].SL_GB_NKBranch);
			AssertEquals("Logged in Department", "GHI", auditedLogs[0].SL_GE_NKDepartment);
			AssertEquals("Log reference", $"Opportunity {opportunity.P8_OpportunityID} Recall Date Updated|NEW=20201013T133000Z", auditedLogs[0].SL_Reference);

			AssertEquals("Logged in User", "ABC", auditedLogs[1].SL_GS_NKUser);
			AssertEquals("Logged in Branch", "DEF", auditedLogs[1].SL_GB_NKBranch);
			AssertEquals("Logged in Department", "GHI", auditedLogs[1].SL_GE_NKDepartment);
			AssertEquals("Log reference", $"Opportunity {opportunity.P8_OpportunityID} Recall Date Updated|NEW=20211112T144500Z|OLD=20201013T133000Z", auditedLogs[1].SL_Reference);

			opportunity.P8_RecallDate = ZDateTime.Empty;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Factory.Save();
			}

			auditedLogs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("Should be three log entries", 3, auditedLogs.Length);

			AssertEquals("Logged in User", "ABC", auditedLogs[0].SL_GS_NKUser);
			AssertEquals("Logged in Branch", "DEF", auditedLogs[0].SL_GB_NKBranch);
			AssertEquals("Logged in Department", "GHI", auditedLogs[0].SL_GE_NKDepartment);
			AssertEquals("Log reference", $"Opportunity {opportunity.P8_OpportunityID} Recall Date Updated|NEW=20201013T133000Z", auditedLogs[0].SL_Reference);

			AssertEquals("Logged in User", "ABC", auditedLogs[1].SL_GS_NKUser);
			AssertEquals("Logged in Branch", "DEF", auditedLogs[1].SL_GB_NKBranch);
			AssertEquals("Logged in Department", "GHI", auditedLogs[1].SL_GE_NKDepartment);
			AssertEquals("Log reference", $"Opportunity {opportunity.P8_OpportunityID} Recall Date Updated|NEW=20211112T144500Z|OLD=20201013T133000Z", auditedLogs[1].SL_Reference);

			AssertEquals("Logged in User", "ABC", auditedLogs[2].SL_GS_NKUser);
			AssertEquals("Logged in Branch", "DEF", auditedLogs[2].SL_GB_NKBranch);
			AssertEquals("Logged in Department", "GHI", auditedLogs[2].SL_GE_NKDepartment);
			AssertEquals("Log reference", $"Opportunity {opportunity.P8_OpportunityID} Recall Date Updated|OLD=20211112T144500Z", auditedLogs[2].SL_Reference);
		}

		public void TestOpportunityStatusUpdatedLog()
		{
			var statusCollection = new OpportunityStatusCollection();
			statusCollection.Add("CS1", (NoResString)"Lost", effectiveAgreement: false, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
			statusCollection.Add("CS2", (NoResString)"Abandoned", effectiveAgreement: false, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
			statusCollection.Add("OP1", (NoResString)"Current", effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
			statusCollection.Add("OP2", (NoResString)"Suspended", effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statusCollection);

			var opportunity = CreateOpportunity(true);
			opportunity.P8_Status = "OP1";
			Factory.Save();

			var eventCodeQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Parent, opportunity.PK);

			var logs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("There should be no edited record yet.", 0, logs.Length);

			opportunity.P8_Status = "CS1";
			Factory.Save();

			logs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("Opportunity status change log should be created", 1, logs.Where(x => x.referenceFreeText.Equals("Opportunity Status updated from OP1 (Open) to CS1 (Closed)")).Count());

			opportunity.P8_Status = "CS2";
			Factory.Save();

			logs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("Opportunity status change log should be created", 1, logs.Where(x => x.referenceFreeText.Equals("Opportunity Status updated from CS1 (Closed) to CS2 (Closed)")).Count());

			opportunity.P8_Status = "OP2";
			Factory.Save();

			logs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("Opportunity status change log should be created", 1, logs.Where(x => x.referenceFreeText.Equals("Opportunity Status updated from CS2 (Closed) to OP2 (Open)")).Count());

			opportunity.P8_Status = "OP1";
			Factory.Save();

			logs = opportunity.Logs.Find(eventCodeQuery);
			AssertEquals("Opportunity status change log should be created", 1, logs.Where(x => x.referenceFreeText.Equals("Opportunity Status updated from OP2 (Open) to OP1 (Open)")).Count());
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestP8_ClosedDateLocal()
		{
			var opportunity = CreateOpportunity(false);
			opportunity.P8_ClosedDate = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), opportunity.P8_ClosedDateLocal);

			opportunity.P8_ClosedDateLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), opportunity.P8_ClosedDate);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestP8_EstimatedCloseDateLocal()
		{
			var opportunity = CreateOpportunity(false);
			opportunity.P8_EstimatedCloseDate = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), opportunity.P8_EstimatedCloseDateLocal);

			opportunity.P8_EstimatedCloseDateLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), opportunity.P8_EstimatedCloseDate);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestP8_RecallDateLocal()
		{
			var opportunity = CreateOpportunity(false);
			opportunity.P8_RecallDate = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), opportunity.P8_RecallDateLocal);

			opportunity.P8_RecallDateLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), opportunity.P8_RecallDate);
		}

		public void TestIsClosedAndOverallDisposition()
		{
			var statusCollection = new OpportunityStatusCollection();
			statusCollection.Add("XXX", (NoResString)"XXX Description", false);
			statusCollection.Add("YYY", (NoResString)"YYY Description", true);
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statusCollection);

			var opp = CreateOpportunity(false);
			opp.P8_LostReason = "L01";
			opp.P8_Status = "XXX";
			AssertEquals(false, opp.IsClosed);
			AssertEquals(OrgOpportunityOverallDispositionList.Codes.Open, opp.OverallDisposition);
			AssertEquals(true, opp.P8_LostReasonInfo.ReadOnly);
			AssertEquals("", opp.P8_LostReason);

			opp.P8_LostReason = "L01";
			opp.P8_Status = "YYY";
			AssertEquals(true, opp.IsClosed);
			AssertEquals(OrgOpportunityOverallDispositionList.Codes.Closed, opp.OverallDisposition);
			AssertEquals(false, opp.P8_LostReasonInfo.ReadOnly);
			AssertEquals("L01", opp.P8_LostReason);

			opp.P8_LostReason = "L01";
			opp.P8_Status = "ZZZ";
			AssertEquals(false, opp.IsClosed);
			AssertEquals(OrgOpportunityOverallDispositionList.Codes.Open, opp.OverallDisposition);
			AssertEquals(true, opp.P8_LostReasonInfo.ReadOnly);
			AssertEquals("", opp.P8_LostReason);

			opp.P8_LostReason = "L01";
			opp.P8_Status = "";
			AssertEquals(false, opp.IsClosed);
			AssertEquals(OrgOpportunityOverallDispositionList.Codes.Open, opp.OverallDisposition);
			AssertEquals(true, opp.P8_LostReasonInfo.ReadOnly);
			AssertEquals("", opp.P8_LostReason);
		}

		public void TestStaffSalesRepresentativeIsDefaultedCorrectly()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var currentCompany = GlbCompany.CurrentCompany;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var staffForOtherCompany = AddNewSalesStaff(organisation, otherCompany.PK);
			var opportunity1 = CreateOpportunity(false);
			opportunity1.P8_OH = organisation.PK;
			AssertEquals("Should be left blank if there is no sales rep assignment for organisation specific to current company", "", opportunity1.P8_GS_NKPrimarySalesPerson);

			var globalStaff = AddNewSalesStaff(organisation, ZGuid.Empty);
			var opportunity2 = CreateOpportunity(false);
			opportunity2.P8_OH = organisation.PK;
			AssertEquals("Should be defaulted to global sales rep for organisation if there is none assigned for current company", globalStaff.GS_Code, opportunity2.P8_GS_NKPrimarySalesPerson);

			var staffForCurrentCompany = AddNewSalesStaff(organisation, currentCompany.PK);
			var opportunity3 = CreateOpportunity(false);
			opportunity3.P8_OH = organisation.PK;
			AssertEquals("Should be defaulted to current company's sales rep for the organisation", staffForCurrentCompany.GS_Code, opportunity3.P8_GS_NKPrimarySalesPerson);
		}

		public void TestNewOpportunityLog()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "Code111";
			OrgOpportunity opportunity = CreateOpportunity(true);
			opportunity.P8_OH = org.PK;
			Factory.Save();

			ZQuery eventCodeQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Parent, org.PK);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Reference, "Opportunity " + opportunity.P8_OpportunityID + " attached");

			StmALog[] auditedLogs1 = org.Logs.Find(eventCodeQuery);
			AssertEquals(1, auditedLogs1.Length);
			AssertEquals("Opportunity " + opportunity.P8_OpportunityID + " attached", auditedLogs1[0].SL_Reference);
		}

		public void TestMoveOpportunityLog()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "Code111";
			OrgOpportunity opportunity = CreateOpportunity(true);
			opportunity.P8_OH = org.PK;
			Factory.Save();

			ZQuery eventCodeQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Parent, org.PK);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Reference, "Opportunity " + opportunity.P8_OpportunityID + " attached");

			StmALog[] auditedLogs1 = org.Logs.Find(eventCodeQuery);
			AssertEquals(1, auditedLogs1.Length);
			AssertEquals("Opportunity " + opportunity.P8_OpportunityID + " attached", auditedLogs1[0].SL_Reference);

			OrgHeader org2 = OrgHeader.New(Factory);
			org2.OH_Code = "Code222";
			opportunity.P8_OH = org2.PK;
			Factory.Save();

			ZQuery eventCodeQuery2 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			eventCodeQuery2.AddToFilter(StmALogSchema.SL_Parent, org.PK);
			eventCodeQuery2.AddToFilter(StmALogSchema.SL_Reference, "Opportunity " + opportunity.P8_OpportunityID + " moved to " + org2.OH_Code);

			auditedLogs1 = org.Logs.Find(eventCodeQuery2);
			AssertEquals(1, auditedLogs1.Length);
			AssertEquals("Opportunity " + opportunity.P8_OpportunityID + " moved to " + org2.OH_Code, auditedLogs1[0].SL_Reference);

			ZQuery eventCodeQuery3 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Parent, org2.PK);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Reference, "Opportunity " + opportunity.P8_OpportunityID + " attached from " + org.OH_Code);

			StmALog[] auditedLogs2 = org2.Logs.Find(eventCodeQuery3);
			AssertEquals(1, auditedLogs2.Length);
			AssertEquals("Opportunity " + opportunity.P8_OpportunityID + " attached from " + org.OH_Code, auditedLogs2[0].SL_Reference);
		}

		public void TestRemoveOpportunityLog()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_Code = "Code333";
			OrgOpportunity opportunity = CreateOpportunity(true);
			opportunity.P8_OH = org.PK;
			Factory.Save();

			ZQuery eventCodeQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Parent, org.PK);
			eventCodeQuery.AddToFilter(StmALogSchema.SL_Reference, "Opportunity " + opportunity.P8_OpportunityID + " attached");

			StmALog[] auditedLogs1 = org.Logs.Find(eventCodeQuery);
			AssertEquals(1, auditedLogs1.Length);
			AssertEquals("Opportunity " + opportunity.P8_OpportunityID + " attached", auditedLogs1[0].SL_Reference);
			string opportunityID = opportunity.P8_OpportunityID;
			opportunity.Delete();
			Factory.Save();

			ZQuery eventCodeQuery2 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			eventCodeQuery2.AddToFilter(StmALogSchema.SL_Parent, org.PK);
			eventCodeQuery2.AddToFilter(StmALogSchema.SL_Reference, "Opportunity " + opportunityID + " deleted");

			auditedLogs1 = org.Logs.Find(eventCodeQuery2);
			AssertEquals(1, auditedLogs1.Length);
			AssertEquals("Opportunity " + opportunityID + " deleted", auditedLogs1[0].SL_Reference);
		}

		[TestUtcOffset(5, 30, 0)]
		public void TestCalendarReminder()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Zubin Appoo";
			staff.GS_EmailAddress = "";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "My Test Org";
			org.MainAddress.OA_Address1 = "Some Street";
			org.OH_RL_NKClosestPort = "INBOM";
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_OpportunityID = "9876";
			opportunity.P8_OA = org.MainAddress.PK;
			Factory.Save();

			opportunity.P8_RecallDate = new ZDateTime(2006, 5, 5, 9, 0, 0);

			Factory.Save();

			opportunity.P8_GS_NKPrimarySalesPerson = staff.GS_Code;
			Factory.Save();

			staff.GS_EmailAddress = "zubin.appoo@cargowise.com";
			opportunity.P8_RecallDateLocal = new ZDateTime(2006, 5, 5, 9, 10, 0);
			AssertEquals(true, opportunity.ShouldCreateRecallReminder);
			Factory.Save();

			AssertEquals(false, opportunity.ShouldCreateRecallReminder);
			var recallReminder = opportunity.GetNewRecallReminder(ZDateTime.Empty, opportunity.P8_RecallDate);
			AssertEquals("Recall due for Opportunity " + opportunity.P8_OpportunityID + " - My Test Org", recallReminder.Subject);
			AssertEquals("Recall due for Opportunity " + opportunity.P8_OpportunityID + " - My Test Org", recallReminder.Body);
			var expectedUrl = $"edient:Command=ShowEditForm&ControllerID=Opportunity&BusinessEntityPK={opportunity.PK}";
			AssertStartsWith("HTML body should contain valid hyperlink", $"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>Recall due for Opportunity <a href='{expectedUrl}", recallReminder.HtmlBody);
			AssertEndsWith("HTML body should contain hyperlink text", $"'>{opportunity.P8_OpportunityID} - My Test Org</a></BODY></HTML>", recallReminder.HtmlBody);
			AssertEquals("zubin.appoo@cargowise.com", recallReminder.Recipients[0].Email);
			AssertEquals("Zubin Appoo", recallReminder.Recipients[0].Name);
			AssertEquals("SOME STREET MH INDIA", recallReminder.Location);
			AssertEquals(new ZDateTime(2006, 5, 5, 3, 40, 0), recallReminder.UTCDateFrom);

			//test cancellation
			opportunity.P8_RecallDateLocal = ZDate.Empty;
			AssertEquals(true, opportunity.ShouldCreateRecallReminder);
			Factory.Save();

			AssertEquals(false, opportunity.ShouldCreateRecallReminder);
			recallReminder = opportunity.GetNewRecallReminder(new ZDateTime(2006, 5, 5, 3, 40, 0), opportunity.P8_RecallDate);
			AssertEquals("Recall has been canceled for Opportunity " + opportunity.P8_OpportunityID + " - My Test Org", recallReminder.Subject);
			AssertEquals("Recall has been canceled for Opportunity " + opportunity.P8_OpportunityID + " - My Test Org", recallReminder.Body);
			AssertStartsWith("HTML body should contain valid hyperlink", $"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>Recall has been canceled for Opportunity <a href='{expectedUrl}", recallReminder.HtmlBody);
			AssertEndsWith("HTML body should contain hyperlink text", $"'>{opportunity.P8_OpportunityID} - My Test Org</a></BODY></HTML>", recallReminder.HtmlBody);
			AssertEquals(CargoWise.Services.Calendar.ReminderType.Cancellation, recallReminder.ReminderType);
		}

		public virtual void TestSetMultiplierRecalculatesEstimatedValue()
		{
			OrgOpportunity opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			OrgOpportunityValue value = opportunity.ValueItems.AddNew();
			value.PV_Value = 10;
			value = opportunity.ValueItems.AddNew();
			value.PV_Value = 20;
			opportunity.P8_RentalMultiplier = 12;
			AssertEquals("Incorrect value", ZDecimal.Parse("360"), opportunity.P8_EstimatedValue);
			opportunity.P8_RentalMultiplier = 25;
			value.PV_Value = 35;
			opportunity.P8_RentalMultiplier = 30;
			AssertEquals("Incorrect value", ZDecimal.Parse("540"), opportunity.P8_EstimatedValue);
		}

		public virtual void TestNoteTypes()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			AssertEquals(1, opportunity.NoteTypes.Count);
			AssertEquals(PredefinedNoteTypes.Instance.OpportunityFollowUpNote, ((IList)opportunity.NoteTypes)[0]);
		}

		public void TestExtraCategoryLabel()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Product Type", opportunity.ExtraCategoryDescription);
			OrganisationsDataRegistry.Instance.ProductTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Mary");
			AssertEquals("Mary", opportunity.ExtraCategoryDescription);
		}

		public virtual void TestRentalMultiplierDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Potential", opportunity.RentalMultiplierDescription);
			OrganisationsDataRegistry.Instance.PotentialLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Mary");
			AssertEquals("Mary", opportunity.RentalMultiplierDescription);
		}

		public virtual void TestTotalDiscountDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();

			AssertEquals("Current", opportunity.TotalDiscountDescription);
			OrganisationsDataRegistry.Instance.CurrentLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Sam");
			AssertEquals("Sam", opportunity.TotalDiscountDescription);
		}

		[TestDate(2014, 1, 31)]
		public void TestStatusAutoCloseOpportunity()
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
			org.OH_FullName = "Strawberry Fields Co";
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();

			Assert(opportunity.P8_ClosedDate.IsEmpty);

			opportunity.P8_Status = "ST1";
			Assert(opportunity.P8_ClosedDate.IsEmpty);

			opportunity.P8_Status = "ST2";
			AssertEquals(new ZDateTime(2014, 1, 31), opportunity.P8_ClosedDate);
		}

		public void TestAssignedOrgPK()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			Factory.Save();

			AssertEquals(false, opportunity.HasChanges);
			OrgHeader assignedOrg = Factory.NewWithValidTestData<OrgHeader>();
			opportunity.AssignedOrgPK = assignedOrg.PK;
			AssertEquals(true, opportunity.HasChanges);
		}

		public void TestAssignedOrgPKValidation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();

			AssertNoErrors(opportunity.AssignedOrgPKInfo);

			opportunity.AssignedOrgPK = ZGuid.NewZGuid();
			AssertHasErrors(opportunity.AssignedOrgPKInfo);

			opportunity.AssignedOrgPK = org1.PK;
			AssertNoErrors(opportunity.AssignedOrgPKInfo);
		}

		public void TestAssignedOrgPKResetCache()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			AssertEquals(false, opportunity.HasChanges);
			var assignedOrg = Factory.NewWithValidTestData<OrgHeader>();

			opportunity.AssignedOrgPK = assignedOrg.PK;
			opportunity.ResetAssignedOrgPKCache();
			AssertEquals(opportunity.AssignedOffice.OA_OH, opportunity.AssignedOrgPK);

			opportunity.AssignedOrgPK = Guid.Empty;
			opportunity.ResetAssignedOrgPKCache();
			AssertEquals(Guid.Empty, opportunity.AssignedOrgPK);
		}

		public void TestAssignedContactAndAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Zubin";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "ABC";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			AssertEquals("no org, so contact readonly", true, opportunity.P8_OC_AssignedOfficeContactInfo.ReadOnly);

			opportunity.AssignedOrgPK = org1.PK;
			Factory.Save();

			opportunity.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("Assigned office is defaulted to main address, so contact not readonly", false, opportunity.P8_OC_AssignedOfficeContactInfo.ReadOnly);
			AssertEquals("There are contacts because office is defaulted to main address", 1, opportunity.Lookups.AssignedOfficeContacts.Count);

			opportunity.AssignedOrgPK = org2.PK;
			opportunity.P8_OA_AssignedOffice = org2.Addresses[0].PK;
			Factory.Save();

			opportunity.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("1 contact - ABC", "ABC", opportunity.Lookups.AssignedOfficeContacts[0].OC_ContactName);
			AssertEquals("1 office", 1, opportunity.Lookups.AssignedOffices.Count);
			opportunity.P8_OC_AssignedOfficeContact = opportunity.Lookups.AssignedOfficeContacts[0].PK;
			Factory.Save();

			opportunity.Lookups.AssignedOfficeContacts.Load();
			AssertEquals("Contact set", opportunity.Lookups.AssignedOfficeContacts[0].PK, opportunity.P8_OC_AssignedOfficeContact);

			opportunity.AssignedOrgPK = ZGuid.Empty;
			Factory.Save();

			opportunity.Lookups.AssignedOfficeContacts.Load();
			Assert("no org, so office blanked out", opportunity.P8_OA_AssignedOffice.IsEmpty);
			AssertEquals("no offices in list", 0, opportunity.Lookups.AssignedOffices.Count);
			Assert("no org, so contact blanked out", opportunity.P8_OC_AssignedOfficeContact.IsEmpty);
			AssertEquals("no contacts in list", 0, opportunity.Lookups.AssignedOfficeContacts.Count);
		}

		public void TestAssignedContactIsMaintainedAfterLoad()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			var address1 = org1.Addresses[0];
			contact1.OC_ContactName = "Zubin";

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.AssignedOrgPK = org1.PK;
			opportunity.P8_OA_AssignedOffice = address1.PK;
			Factory.Save();

			opportunity.Lookups.AssignedOfficeContacts.Load();

			opportunity.P8_OC_AssignedOfficeContact = opportunity.Lookups.AssignedOfficeContacts[0].PK;
			Factory.Save();

			opportunity.Lookups.AssignedOfficeContacts.Load();

			AssertEquals("Contact set", opportunity.Lookups.AssignedOfficeContacts[0].PK, opportunity.P8_OC_AssignedOfficeContact);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var reloadedOpp = newFactory.Load<OrgOpportunity>(opportunity.PK);
			AssertEquals("Reloaded opp has correct assigned org", org1.PK, reloadedOpp.AssignedOrgPK);
			AssertEquals("Reloaded opp has correct assigned contact", "Zubin", reloadedOpp.AssignedOfficeContact.OC_ContactName);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity1 = newOrg.SalesOpportunities.AddNew();
			opportunity1.AssignedOrgPK = org1.PK;
			opportunity1.P8_OA_AssignedOffice = ZGuid.Empty;

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			reloadedOpp = newFactory.Load<OrgOpportunity>(opportunity1.PK);
			AssertEquals("Because the Assigned Office was not set", ZGuid.Empty, reloadedOpp.AssignedOrgPK);
			AssertEquals("Reloaded opp does not have an assigned office contact", null, reloadedOpp.AssignedOfficeContact);
		}

		public void TestAssignedOfficeAndAssignedOrgPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			var address1 = org1.Addresses[0];
			contact1.OC_ContactName = "Zubin";

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.AssignedOrgPK = org1.PK;
			opportunity.P8_OA_AssignedOffice = address1.PK;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var reloadedOpp = newFactory.Load<OrgOpportunity>(opportunity.PK);
			AssertEquals("Reloaded opp has correct assigned org", org1.PK, reloadedOpp.AssignedOrgPK);
			AssertEquals("Reloaded opp has correct assigned office", address1.PK, reloadedOpp.P8_OA_AssignedOffice);
		}

		public void TestDefaultAssignedOffice()
		{
			OrgHeader assignedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = assignedOrg.Addresses[0];
			address1.OA_Address1 = "Address One";
			var address2 = assignedOrg.Addresses.AddNew();
			address2.OA_Address2 = "Address Two";

			AssertEquals(address1.PK, assignedOrg.MainAddress.PK);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.AssignedOrgPK = assignedOrg.PK;
			AssertEquals(address1.PK, opportunity.P8_OA_AssignedOffice);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			AssertEquals("Should be 0 business objects with related logs", 0, Opportunity.BusinessObjectsWithRelatedEvents.Length);

			Opportunity.WorkflowItems.AddNew();
			AssertEquals("Business objects with related logs", 1, Opportunity.BusinessObjectsWithRelatedEvents.Length);

			Opportunity.WorkflowItems.AddNew();
			AssertEquals("Business objects with related logs", 2, Opportunity.BusinessObjectsWithRelatedEvents.Length);

			Opportunity.ValueItems.AddNew();
			AssertEquals("Business objects with related logs", 3, Opportunity.BusinessObjectsWithRelatedEvents.Length);

			var agreement = Opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.ProductItems.DeleteAll();
			agreement.Recipients.DeleteAll();
			AssertEquals("Business objects with related logs", 4, Opportunity.BusinessObjectsWithRelatedEvents.Length);

			agreement.Recipients.AddNew();
			AssertEquals("Business objects with related logs", 5, Opportunity.BusinessObjectsWithRelatedEvents.Length);

			var newDraftAgreement = Opportunity.CommissionAgreements.AddNew();
			newDraftAgreement.ProductItems.DeleteAll();
			newDraftAgreement.Recipients.DeleteAll();
			AssertEquals("Business objects with related logs", 6, Opportunity.BusinessObjectsWithRelatedEvents.Length);

			OrgCommissionAgreementTestHelper.AddExclusionCommissionAgreementOverallItem(newDraftAgreement, "XXX");
			AssertEquals("Business objects with related logs", 7, Opportunity.BusinessObjectsWithRelatedEvents.Length);
		}

		public void TestTypeDescription()
		{
			Opportunity.P8_OpportunityType = Opportunity.Lookups.Types[0].Code;
			AssertEquals("Description shown", Opportunity.Lookups.Types[0].Description, Opportunity.TypeDescription);

			Opportunity.P8_OpportunityType = "";
			AssertEquals("Description blank", "", Opportunity.TypeDescription);
		}

		public void TestOpportunityIDReadOnly()
		{
			Assert("Readonly ID", Opportunity.P8_OpportunityIDInfo.ReadOnly);
		}

		public void TestOpportunityIDSetOnSaving()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			AssertEquals("No ID", ZString.Empty, opportunity.P8_OpportunityID);

			string expectedID = Env.NumberFountains.SalesOpportunityID.PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("ID set", expectedID, opportunity.P8_OpportunityID);

			opportunity.P8_OpportunityDescription = "Test";
			Factory.Save();
			AssertEquals("ID not changed", expectedID, opportunity.P8_OpportunityID);
		}

		[UseSnapshotProtection]
		public void TestOpportunityID_IsSetAgainAfterFirstSaveFails()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAA";
			Factory.Save();

			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_GC = ZGuid.NewZGuid();

			Env.NumberFountains.SalesOpportunityID.SetNext(Db.Connection, 5005);
			AssertExceptionThrown("Should not save succesfully because of invalid company", typeof(ZSaveException), () =>
			{
				Factory.Save();
			});
			AssertEquals(false, opportunity.IsInDatabase);
			AssertEquals("Should be reset to empty after failed save", ZString.Empty, opportunity.P8_OpportunityID);

			Env.NumberFountains.SalesOpportunityID.SetNext(Db.Connection, 6001);

			opportunity.P8_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertNotEquals("ID should have been updated", "O0006001", opportunity.P8_OpportunityID);
		}

		public void TestUpdateProcessTasksOrgs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			OrgContact contact2 = org1.Contacts.AddNew();
			OrgAddress mainAddress1 = org1.MainAddress;
			OrgAddress otherAddress1 = org1.Addresses.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact3 = org2.Contacts.AddNew();
			OrgAddress mainAddress2 = org2.MainAddress;

			OrgOpportunity opportunity = org1.SalesOpportunities.AddNew();
			opportunity.P8_OC = contact1.PK;

			ProcessTask task1 = opportunity.WorkflowItems.AddNew();
			ProcessTask task2 = opportunity.WorkflowItems.AddNew();
			ProcessTask task3 = opportunity.WorkflowItems.AddNew();

			Assert("Pre-condition", task1.OrganisationPK == task2.OrganisationPK &&
				task2.OrganisationPK == task3.OrganisationPK && task3.OrganisationPK == org1.PK);
			Assert("Pre-condition", task1.P9_OC == task2.P9_OC && task2.P9_OC == task3.P9_OC && task3.P9_OC == contact1.PK);
			Assert("Pre-condition", task1.P9_OA == task2.P9_OA && task2.P9_OA == task3.P9_OA && task3.P9_OA == mainAddress1.PK);

			opportunity.P8_OA = otherAddress1.PK;
			Assert("The changing Address on an Opportunity Header should update on all attached workflow tasks",
				task1.P9_OA == task2.P9_OA && task2.P9_OA == task3.P9_OA && task3.P9_OA == otherAddress1.PK);
			Assert(task1.OrganisationPK == task2.OrganisationPK && task2.OrganisationPK == task3.OrganisationPK && task3.OrganisationPK == org1.PK);
			Assert(task1.P9_OC == task2.P9_OC && task2.P9_OC == task3.P9_OC && task3.P9_OC == contact1.PK);

			opportunity.P8_OC = contact2.PK;
			Assert("The changing Contact on an Opportunity Header should update on all attached workflow tasks",
				task1.P9_OC == task2.P9_OC && task2.P9_OC == task3.P9_OC && task3.P9_OC == contact2.PK);
			Assert(task1.P9_OA == task2.P9_OA && task2.P9_OA == task3.P9_OA && task3.P9_OA == otherAddress1.PK);
			Assert(task1.OrganisationPK == task2.OrganisationPK && task2.OrganisationPK == task3.OrganisationPK && task3.OrganisationPK == org1.PK);

			opportunity.P8_OH = org2.PK;
			Assert("The changing Org on an Opportunity Header should update on all attached workflow tasks (Contact == Empty and Address == new org main address)",
				task1.OrganisationPK == task2.OrganisationPK && task2.OrganisationPK == task3.OrganisationPK && task3.OrganisationPK == org2.PK);
			Assert("The changing Org on an Opportunity Header should update on all attached workflow tasks (Contact == Empty and Address == new org main address)",
				task1.P9_OA == task2.P9_OA && task2.P9_OA == task3.P9_OA && task3.P9_OA == mainAddress2.PK);
			Assert("The changing Org on an Opportunity Header should update on all attached workflow tasks (Contact == Empty and Address == new org main address)",
				task1.P9_OC == task2.P9_OC && task2.P9_OC == task3.P9_OC && task3.P9_OC == ZGuid.Empty);
		}

		public void TestSetOpportunityOrgUpdatesAddressAndContact()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org1.Contacts.AddNew();
			OrgAddress mainAddress1 = org1.MainAddress;
			OrgAddress otherAddress1 = org1.Addresses.AddNew();

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact2 = org2.Contacts.AddNew();
			OrgAddress mainAddress2 = org2.MainAddress;
			OrgAddress otherAddress2 = org2.Addresses.AddNew();

			OrgOpportunity opportunity = org1.SalesOpportunities.AddNew();
			opportunity.P8_OC = contact1.PK;
			opportunity.P8_OA = otherAddress1.PK;

			opportunity.P8_OH = org2.PK;
			AssertEquals("If organization changed the address be reset to new org main address", mainAddress2.PK, opportunity.P8_OA);
			AssertEquals("If organization changed the contact be reset to empty", ZGuid.Empty, opportunity.P8_OC);

			opportunity.P8_OC = contact2.PK;
			opportunity.P8_OH = org1.PK;
			AssertEquals("If organization changed the address be reset to new org main address", mainAddress1.PK, opportunity.P8_OA);
			AssertEquals("If organization changed the contact be reset to empty", ZGuid.Empty, opportunity.P8_OC);
		}

		public void TestCloseReasonDescription()
		{
			var collection = new OpportunityClosedReasonsCollection();
			collection.Add("MEH", (NoResString)"Too complicated", true);
			collection.Add("INC", (NoResString)"Too expensive", true);

			OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var opportunity = CreateOpportunity(false);
			opportunity.P8_LostReason = "MEH";
			AssertEquals("Too complicated", opportunity.CloseReasonDescription);
			opportunity.P8_LostReason = "INC";
			AssertEquals("Too expensive", opportunity.CloseReasonDescription);
		}

		public void TestProductTypeDescription()
		{
			var productTypes = new CodeDescriptionBoolCollection();
			productTypes.Add("AAA", (NoResString)"Type A");
			productTypes.Add("BBB", (NoResString)"Type B", false);

			OrganisationsDataRegistry.Instance.ProductTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productTypes);
			OrgOpportunity opp = CreateOpportunity(false);
			AssertEquals(string.Empty, opp.ProductTypeDescription);
			opp.P8_PackageType = "AAA";
			AssertEquals("Type A", opp.ProductTypeDescription);
			opp.P8_PackageType = "BBB";
			AssertEquals("Type B", opp.ProductTypeDescription);
		}

		public void TestValueTypes()
		{
			OrgOpportunity opp = CreateOpportunity(false);
			AssertEquals(string.Empty, opp.ValueTypes);

			OrgOpportunityValue value1 = opp.ValueItems.AddNew();
			value1.PV_RevenueType = "A10";
			AssertEquals("A10", opp.ValueTypes);

			OrgOpportunityValue value2 = opp.ValueItems.AddNew();
			value2.PV_RevenueType = "BID";
			AssertEquals("A10, BID", opp.ValueTypes);
		}

		public void TestOpportunityDetails()
		{
			OrgOpportunity opp = CreateOpportunity(false);
			AssertEquals(string.Empty, opp.OpportunityDetails);

			string details =
@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue0;}
\viewkind4\uc1\pard\cf1\f0\fs20Cool test RTF formatted text.\par It is really very very cool.\cf0\par
}";

			opp.P8_OpportunityNotes = ZBlob.FromUTF8(details);
			AssertEquals("Cool test RTF formatted text. It is really very very cool.", opp.OpportunityDetails);
		}

		public void TestCloseCertaintyAsPercentageString()
		{
			OrgOpportunity opportunity = CreateOpportunity(false);
			opportunity.P8_CloseCertainty = 45;
			AssertEquals("45%", opportunity.CloseCertaintyAsPercentageString);

			opportunity.P8_CloseCertainty = 100;
			AssertEquals("100%", opportunity.CloseCertaintyAsPercentageString);

			opportunity.P8_CloseCertainty = 63;
			AssertEquals("63%", opportunity.CloseCertaintyAsPercentageString);

			OrgOpportunity newOpportunity = CreateOpportunity(false);
			AssertEquals("0%", newOpportunity.CloseCertaintyAsPercentageString);
		}

		public void TestSourceDetailsInfoMaxLength()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("MRK", (NoResString)"Marketing Campaign", true, "CL2");
			collection.Add("OTH", (NoResString)"Other Campaign", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var collection2 = new CodeDescriptionBoolCollection();
			collection2.Add("AB", (NoResString)"code of length 2", true);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);

			OrgOpportunity opp = CreateOpportunity(false);
			opp.P8_Source = "MRK";
			AssertEquals(2, opp.SourceDetailsInfo.MaxLength);

			collection2.Add("ABC", (NoResString)"code of length 3", true);
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);
			AssertEquals(3, opp.SourceDetailsInfo.MaxLength);

			opp.P8_Source = "OTH";
			AssertEquals(OrgOpportunity.Schema.P8_SourceDetailsMaxLength, opp.SourceDetailsInfo.MaxLength);
		}

		public void TestSourceDetailsDataFieldType()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("MRK", (NoResString)"Marketing Campaign", true, "CL2");
			collection.Add("OTH", (NoResString)"Other Campaign", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var opp = CreateOpportunity(false);

			opp.P8_Source = "";
			AssertEquals(nameof(FieldType.Text), opp.SourceDetailsDataFieldType);

			opp.P8_Source = "MRK";
			AssertEquals(nameof(FieldType.TextDropEdit), opp.SourceDetailsDataFieldType);

			opp.P8_Source = "OTH";
			AssertEquals(nameof(FieldType.Text), opp.SourceDetailsDataFieldType);
		}

		public void TestSourceValidation()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("MRK", (NoResString)"Marketing Campaign", true, "CL2");
			collection.Add("OTH", (NoResString)"Other Campaign", true);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var opp = CreateOpportunity(true);
			opp.SourceDetails = "TEST";
			opp.P8_Source = "";
			AssertNoErrors(opp);

			opp.P8_Source = "MRK";
			AssertHasError("Should have error when setting source as TEST is not included in MRK source details list", opp.SourceDetailsInfo, "Enter a valid Source Details.");

			opp.P8_Source = "OTH";
			AssertNoErrors("Should clear error when setting source as OTH source does not have associated details list", opp);
		}

		#region Associated Trade Lanes

		OrgSales CreateNewProspectiveSales(OrgHeader header, ZString originCode, ZString destinationCode)
		{
			var origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, originCode);
			var destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, destinationCode);

			OrgSales tradeLane = header.SalesCollection.AddNew();
			if (origin != null)
			{
				tradeLane.OW_OriginID = origin.PK;
			}
			if (destination != null)
			{
				tradeLane.OW_DestinationID = destination.PK;
			}

			return tradeLane;
		}

		public void TestReferringContactNameReadOnlyAndGetsReset()
		{
			var referringOrg = Factory.New<OrgHeader>();
			var opportunity = CreateOpportunity(false);
			Assert("Should be read-only when referring org has not been set", opportunity.P8_OC_ReferringContactInfo.ReadOnly);

			opportunity.P8_OH_ReferringOrganisation = ZGuid.Invalid;
			Assert("Should be read-only when referring org is invalid", opportunity.P8_OC_ReferringContactInfo.ReadOnly);

			opportunity.P8_OH_ReferringOrganisation = referringOrg.PK;
			Assert("Should be writeable when referring org is valid", !opportunity.P8_OC_ReferringContactInfo.ReadOnly);

			opportunity.P8_OC_ReferringContact = referringOrg.Contacts.AddNew().PK;
			opportunity.P8_OH_ReferringOrganisation = ZGuid.Empty;

			Assert("P8_OC_ReferringContact should be reset when referring org is erased", opportunity.P8_OC_ReferringContact.IsEmpty);
		}

		public void TestAssociatedTradeLanesPivots()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = header.SalesOpportunities.AddNew();
			OrgOpportunity opportunity2 = header.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();
			opportunity2.FillWithValidTestData();

			AssertEquals("No TradeLanes on Header", 0, header.SalesCollection.Count);
			OrgSales tradeLane1 = CreateNewProspectiveSales(header, "USLAX", "AUSYD");
			OrgSales tradeLane2 = CreateNewProspectiveSales(header, "PAACU", "GAAKE");
			OrgSales tradeLane3 = CreateNewProspectiveSales(header, "IDABU", "OMBYB");
			OrgSales tradeLane4 = CreateNewProspectiveSales(header, "LAAOU", "SAAAK");
			AssertEquals("TradeLanes added to Header", 4, header.SalesCollection.Count);

			AssertEquals("There should be no tradelanes for opportunity", 0, opportunity.AssociatedTradeLanesPivots.Count);
			AssertEquals("There should be no tradelanes for opportunity2", 0, opportunity2.AssociatedTradeLanesPivots.Count);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
			opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane3);
			opportunity2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane4);

			AssertEquals("There should be 2 tradelanes for opportunity", 2, opportunity.AssociatedTradeLanesPivots.Count);
			AssertEquals("There should be 2 tradelanes for opportunity2", 2, opportunity2.AssociatedTradeLanesPivots.Count);

			opportunity.AssociatedTradeLanesPivots.DeletePivotFor(tradeLane1);
			Assert("There should be 1 tradelane for opportunity", opportunity.AssociatedTradeLanesPivots.Contains(tradeLane2));
			Assert("Organisation still contains TradeLane1", header.SalesCollection.Contains(tradeLane1));

			opportunity.AssociatedTradeLanesPivots.DeletePivotFor(tradeLane2);
			AssertEquals("There should be 0 tradelanes for opportunity", 0, opportunity.AssociatedTradeLanesPivots.Count);
			Assert("Organisation still contains TradeLane2", header.SalesCollection.Contains(tradeLane2));
		}

		public void TestDeletingLinksDoesNotRemoveTheTradeLane()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = header.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();

			AssertEquals("No TradeLanes on Header", 0, header.SalesCollection.Count);
			OrgSales tradeLane1 = CreateNewProspectiveSales(header, "USLAX", "AUSYD");
			AssertEquals("TradeLanes added to Header", 1, header.SalesCollection.Count);

			AssertEquals("There should be no tradelanes for opportunity", 0, opportunity.AssociatedTradeLanesPivots.Count);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			AssertEquals("There should be 1 tradelane for opportunity", 1, opportunity.AssociatedTradeLanesPivots.Count);

			Factory.Save();

			opportunity.Delete();
			Assert("Organisation should still contain TradeLane1", header.SalesCollection.Contains(tradeLane1));
		}

		#endregion

		#region Sales Team

		public void TestSalesTeam()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_RL_NKClosestPort = "AUSYD";

			GlbStaff staffA = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staffB = Factory.NewWithValidTestData<GlbStaff>();

			staffA.GS_Code = "AAA";
			staffB.GS_Code = "BBB";

			var teamX = staffA.SalesTeams.AddNew();
			teamX.GG_Code = "XXX";
			teamX.GG_IsSales = true;
			teamX.GG_Desc = "Sales team X";
			teamX.CoveredCountries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")));

			var teamY = staffB.SalesTeams.AddNew();
			teamY.GG_Code = "YYY";
			teamY.GG_IsSales = true;
			teamY.GG_Desc = "Sales team Y";
			teamY.CoveredCountries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")));

			OrgOpportunity opportunity1 = CreateOpportunity(true);
			opportunity1.P8_OH = org1.PK;
			opportunity1.P8_GS_NKPrimarySalesPerson = staffA.GS_Code;

			OrgOpportunity opportunity2 = CreateOpportunity(true);
			opportunity2.P8_OH = org1.PK;
			opportunity2.P8_GS_NKPrimarySalesPerson = staffB.GS_Code;

			OrgOpportunity opportunity3 = CreateOpportunity(true);
			opportunity3.P8_OH = org1.PK;
			opportunity3.P8_GS_NKPrimarySalesPerson = ZString.Empty;

			Factory.Save();
			AssertEquals(1, opportunity1.PrimarySalesPerson.SalesTeams.Count);
			AssertEquals(teamX, opportunity1.SalesTeam);

			AssertEquals(1, opportunity2.PrimarySalesPerson.SalesTeams.Count);
			AssertEquals(teamY, opportunity2.SalesTeam);

			AssertEquals(null, opportunity3.SalesTeam);
		}

		#endregion

		#region Created From Inquiry

		public void TestCreatedFromInquiry()
		{
			OrgOpportunity opp = CreateOpportunity(false);
			AssertNull(opp.CreatedFromInquiry);

			SalesEnquiry inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_LeadUniqueReference = "I00002823";
			opp.P8_O1_Enquiry = inquiry.PK;
			AssertEquals("I00002823", opp.CreatedFromInquiry.O1_LeadUniqueReference);
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		public void TestImportParentRelatedActivityInfoOnNew_Communication()
		{
			var header = Factory.New<OrgHeader>();
			var primaryContact = Factory.New<OrgContact>();
			var communication = Factory.New<OrgSalesCall>();
			communication.OQ_OH = header.PK;
			communication.OQ_OC = primaryContact.PK;
			communication.OQ_GS_NKSalesRep = "ADL";
			communication.OQ_SalesCallNotes = ZBlob.FromUTF8("Hello");
			communication.OQ_CallSummary = "Call him to say hello";

			var opportunity = CreateOpportunity(false);
			((IImportParentRelatedActivityInfoOnNew)opportunity).ImportParentInfo(communication, new ImportRelatedActivityNoDecisionFactory());

			CombineAssertions(() =>
			{
				AssertEquals("P8_OH", header.PK, opportunity.P8_OH);
				AssertEquals("P8_OC", primaryContact.PK, opportunity.P8_OC);
				AssertEquals("P8_GS_NKPrimarySalesPerson", "ADL", opportunity.P8_GS_NKPrimarySalesPerson);
				AssertEquals("P8_OpportunityNotes", "Hello", opportunity.P8_OpportunityNotes.ToUTF8());
				AssertEquals("P8_OpportunityDescription", "Call him to say hello", opportunity.P8_OpportunityDescription);
			});
		}

		public void TestImportParentRelatedActivityInfoOnNew_Inquiry()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "REP";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("SalesOpportunities.Count", 0, org.SalesOpportunities.Count);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			contact.OC_Email = "Contact@test.com";
			contact.OC_Phone = "12345678";
			contact.OC_Mobile = "0012345678";
			contact.OC_JobCategory = "Some Role";

			var referringOrg = Factory.New<OrgHeader>();
			referringOrg.OH_FullName = "Some Agent Org";
			referringOrg.MainAddress.OA_Address1 = "1 Road";
			referringOrg.OH_RL_NKClosestPort = "AUSYD";

			var referringContact = referringOrg.Contacts.AddNew();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			enquiry.O1_OC_LinkedContact = contact.PK;
			enquiry.O1_GS_NKRepAssigned = staff.GS_Code;
			enquiry.EnquiryNotesContent = ZBlob.FromAscii("test note");
			enquiry.O1_LeadSource = "WEB";
			enquiry.O1_OpportunitySourceDetails = "Web campaign";
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.O1_OC_ReferringContact = referringContact.PK;

			Factory.Save();

			var opp = org.SalesOpportunities.AddNew();

			var decisionFactory = new ImportRelatedActivityNoDecisionFactory();
			var mockYesNoDecider = new Mock<IImportRelatedActivityYesNoDecider>();
			mockYesNoDecider
				.Setup(m => m.GetDecision($"Convert {enquiry.HumanReadableName} to new Opportunity?"))
				.Returns(true);
			decisionFactory.AddDecider(typeof(IImportRelatedActivityYesNoDecider), mockYesNoDecider.Object);

			((IImportParentRelatedActivityInfoOnNew)opp).ImportParentInfo(enquiry, decisionFactory);

			AssertEquals("SalesOpportunities.Count", 1, org.SalesOpportunities.Count);
			AssertEquals("P8_O1_Enquiry", enquiry.PK, opp.P8_O1_Enquiry);
			AssertEquals("P8_Source", "WEB", opp.P8_Source);
			AssertEquals("P8_SourceDetails", "Web campaign", opp.P8_SourceDetails);
			AssertEquals("P8_OH_ReferringOrganisation", referringOrg.PK, opp.P8_OH_ReferringOrganisation);
			AssertEquals("P8_OC_ReferringContact", referringContact.PK, opp.P8_OC_ReferringContact);
			AssertEquals("P8_GS_NKPrimarySalesPerson", staff.GS_Code, opp.P8_GS_NKPrimarySalesPerson);
			AssertEquals("P8_OpportunityNotes", enquiry.EnquiryNotesContent.ToAscii(), opp.P8_OpportunityNotes.ToAscii());
			AssertEquals("P8_OH", org.PK, opp.P8_OH);
			AssertEquals("P8_OC", contact.PK, opp.P8_OC);
		}

		public void TestImportParentRelatedActivityInfoOnNew_CreateSalesOpportunityFromInquiry()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Test Organization";
			orgHeader.OH_Code = "TESTORG";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Contact";

			var referringOrg = Factory.New<OrgHeader>();
			referringOrg.OH_FullName = "Referring Organization";
			referringOrg.OH_Code = "REFORG";

			var referringContact = referringOrg.Contacts.AddNew();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = orgHeader.PK;
			enquiry.O1_OC_ReferringContact = referringContact.PK;
			enquiry.O1_GS_NKRepAssigned = staff.GS_Code;
			enquiry.O1_LeadSource = "WOM";
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.O1_OH_ConvertedToQualifiedLead = Guid.Empty;
			enquiry.O1_OC_LinkedContact = contact.PK;
			enquiry.O1_OpportunitySourceDetails = "Testing Opportunity Source";
			enquiry.EnquiryNotesContent = ZBlob.FromAscii("Testing Notes");

			Factory.Save();

			var opp = orgHeader.SalesOpportunities.AddNew();

			var decisionFactory = new ImportRelatedActivityNoDecisionFactory();

			var mockLinkInquiryToOrganisationDecider = new Mock<IImportRelatedActivityLinkInquiryToOrganisationDecider>();
			mockLinkInquiryToOrganisationDecider.Setup(m => m.GetDecision(enquiry)).Returns(true);

			var mockYesNoDecider = new Mock<IImportRelatedActivityYesNoDecider>();
			mockYesNoDecider.Setup(m => m.GetDecision($"Convert {enquiry.HumanReadableName} to new Opportunity?")).Returns(true);

			decisionFactory.AddDecider(typeof(IImportRelatedActivityLinkInquiryToOrganisationDecider), mockLinkInquiryToOrganisationDecider.Object);
			decisionFactory.AddDecider(typeof(IImportRelatedActivityYesNoDecider), mockYesNoDecider.Object);
			((IImportParentRelatedActivityInfoOnNew)opp).ImportParentInfo(enquiry, decisionFactory);

			AssertEquals("SalesOpportunities.Count", 1, orgHeader.SalesOpportunities.Count);
			AssertEquals("P8_Source", enquiry.O1_LeadSource, opp.P8_Source);
			AssertEquals("P8_GS_NKPrimarySalesPerson", staff.GS_Code, opp.P8_GS_NKPrimarySalesPerson);
			AssertEquals("P8_OpportunityNotes", enquiry.EnquiryNotesContent.ToAscii(), opp.P8_OpportunityNotes.ToAscii());
			AssertEquals("P8_SourceDetails", enquiry.O1_OpportunitySourceDetails, opp.P8_SourceDetails);
			AssertEquals("P8_OC", contact.PK, opp.P8_OC);
			AssertEquals("P8_OH", orgHeader.PK, opp.P8_OH);
			AssertEquals("P8_O1_Enquiry", enquiry.PK, opp.P8_O1_Enquiry);
			AssertEquals("P8_OH_ReferringOrganisation", referringOrg.PK, opp.P8_OH_ReferringOrganisation);
			AssertEquals("P8_OC_ReferringContact", referringContact.PK, opp.P8_OC_ReferringContact);
		}

		public void TestImportParentRelatedActivityInfoOnNew_CreateSalesOpportunityFromInquiry_Cancel()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Test Organization";
			orgHeader.OH_Code = "TESTORG";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Contact";

			var referringOrg = Factory.New<OrgHeader>();
			referringOrg.OH_FullName = "Referring Organization";
			referringOrg.OH_Code = "REFORG";

			var referringContact = referringOrg.Contacts.AddNew();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = orgHeader.PK;
			enquiry.O1_OC_ReferringContact = referringContact.PK;
			enquiry.O1_GS_NKRepAssigned = staff.GS_Code;
			enquiry.O1_LeadSource = "WOM";
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.O1_OH_ConvertedToQualifiedLead = Guid.Empty;
			enquiry.O1_OC_LinkedContact = contact.PK;
			enquiry.O1_OpportunitySourceDetails = "Testing Opportunity Source";
			enquiry.EnquiryNotesContent = ZBlob.FromAscii("Testing Notes");

			Factory.Save();

			var opp = orgHeader.SalesOpportunities.AddNew();

			var decisionFactory = new ImportRelatedActivityNoDecisionFactoryTestTrackCalls();

			var mockLinkInquiryToOrganisationDecider = new Mock<IImportRelatedActivityLinkInquiryToOrganisationDecider>();
			mockLinkInquiryToOrganisationDecider.Setup(m => m.GetDecision(enquiry)).Returns(false);

			decisionFactory.AddDecider(typeof(IImportRelatedActivityLinkInquiryToOrganisationDecider), mockLinkInquiryToOrganisationDecider.Object);
			bool result = ((IImportParentRelatedActivityInfoOnNew)opp).ImportParentInfo(enquiry, decisionFactory);

			AssertEquals("Yes No Decider called", false, decisionFactory.GetIfAvailableCoreWasCalled);
			AssertEquals("ImportParentInfo return", false, result);
		}

		public void TestImportParentRelatedActivityInfoOnNew_CampaignWithParentInquiry()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "REP";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("SalesOpportunities.Count", 0, org.SalesOpportunities.Count);

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			contact.OC_Email = "Contact@test.com";
			contact.OC_Phone = "12345678";
			contact.OC_Mobile = "0012345678";
			contact.OC_JobCategory = "Some Role";

			var referringOrg = Factory.New<OrgHeader>();
			referringOrg.OH_FullName = "Some Agent Org";
			referringOrg.MainAddress.OA_Address1 = "1 Road";
			referringOrg.OH_RL_NKClosestPort = "AUSYD";

			var referringContact = referringOrg.Contacts.AddNew();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			enquiry.O1_OC_LinkedContact = contact.PK;
			enquiry.O1_GS_NKRepAssigned = staff.GS_Code;
			enquiry.EnquiryNotesContent = ZBlob.FromAscii("test note");
			enquiry.O1_LeadSource = "WEB";
			enquiry.O1_OpportunitySourceDetails = "Web campaign";
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.O1_OC_ReferringContact = referringContact.PK;
			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			var campaignItem = (IRelatableActivity)Factory.New<IGlbCompanyCampaignItem>();
			((BusinessObject)campaignItem)[GlbCompanyCampaignItemSchema.Constants.G8_G0] = campaign.PK;
			((BusinessObject)campaignItem)[GlbCompanyCampaignItemSchema.Constants.G8_RecipientTableCode] = OrgContactSchema.Constants.Prefix;
			((BusinessObject)campaignItem)[GlbCompanyCampaignItemSchema.Constants.G8_RecipientID] = ZGuid.NewZGuid();
			campaignItem.RelatedParentActivityPivotCollection.AddNewPivot(enquiry);

			Factory.Save();

			var opp = org.SalesOpportunities.AddNew();

			var decisionFactory = new ImportRelatedActivityNoDecisionFactory();
			var mockYesNoDecider = new Mock<IImportRelatedActivityYesNoDecider>();
			mockYesNoDecider
				.Setup(m => m.GetDecision(string.Format("Convert {0} to new Opportunity?", enquiry.HumanReadableName)))
				.Returns(true);
			decisionFactory.AddDecider(typeof(IImportRelatedActivityYesNoDecider), mockYesNoDecider.Object);

			((IImportParentRelatedActivityInfoOnNew)opp).ImportParentInfo(campaignItem, decisionFactory);

			AssertEquals("SalesOpportunities.Count", 1, org.SalesOpportunities.Count);
			AssertEquals("P8_O1_Enquiry", enquiry.PK, opp.P8_O1_Enquiry);
			AssertEquals("P8_Source", "WEB", opp.P8_Source);
			AssertEquals("P8_SourceDetails", "Web campaign", opp.P8_SourceDetails);
			AssertEquals("P8_OH_ReferringOrganisation", referringOrg.PK, opp.P8_OH_ReferringOrganisation);
			AssertEquals("P8_OC_ReferringContact", referringContact.PK, opp.P8_OC_ReferringContact);
			AssertEquals("P8_GS_NKPrimarySalesPerson", staff.GS_Code, opp.P8_GS_NKPrimarySalesPerson);
			AssertEquals("P8_OpportunityNotes", enquiry.EnquiryNotesContent.ToAscii(), opp.P8_OpportunityNotes.ToAscii());
			AssertEquals("P8_OH", org.PK, opp.P8_OH);
			AssertEquals("P8_OC", contact.PK, opp.P8_OC);
		}

		public void TestImportParentRelatedActivityInfoOnNew_Campaign()
		{
			var campaign = Factory.New<IGlbCompanyCampaign>();
			var orgOpportunity = CreateOpportunity(false);

			AssertEquals(ZGuid.Empty, orgOpportunity.P8_G0);

			((IImportParentRelatedActivityInfoOnNew)orgOpportunity).ImportParentInfo((IRelatableActivity)campaign, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals(campaign.PK, orgOpportunity.P8_G0);
		}

		public void TestImportParentRelatedActivityInfoOnNew_CampaignItem()
		{
			var campaign = Factory.New<IGlbCompanyCampaign>();
			var campaignItem = Factory.New<IGlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;

			var orgOpportunity = CreateOpportunity(false);

			AssertEquals(ZGuid.Empty, orgOpportunity.P8_G0);

			((IImportParentRelatedActivityInfoOnNew)orgOpportunity).ImportParentInfo((IRelatableActivity)campaignItem, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals(campaign.PK, orgOpportunity.P8_G0);
		}

		#endregion

		public virtual void TestDefaultEstimatedValueCurrency()
		{
			RefCountry branchCountry = Factory.NewWithValidTestData<RefCountry>();
			branchCountry.RN_Code = "Z1";
			branchCountry.RN_RX_NKLocalCurrency = "AAA";

			RefUNLOCO port1 = Factory.NewWithValidTestData<RefUNLOCO>();
			port1.RL_RN_NKCountryCode = branchCountry.RN_Code;

			RefCountry companyCountry = Factory.NewWithValidTestData<RefCountry>();
			companyCountry.RN_Code = "Z2";
			companyCountry.RN_RX_NKLocalCurrency = "BBB";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = companyCountry.RN_Code;
			company.GC_RX_NKLocalCurrency = companyCountry.RN_RX_NKLocalCurrency;

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = port1.Code;
			branch.GB_PostCode = "555555";

			Factory.Save();

			AssertEquals(branchCountry.RN_Code, branch.Country.RN_Code);
			AssertEquals("AAA", branch.Country.RN_RX_NKLocalCurrency);

			AssertNotEquals(branch.Country.RN_Code, company.GC_RN_NKCountryCode);

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";

			GlbStaff currentUser = Factory.NewWithValidTestData<GlbStaff>();
			currentUser.GS_Code = "KKK";
			currentUser.GS_LoginName = "kkkkk";
			currentUser.GS_GB_HomeBranch = branch.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, branch.PK.ToGuid(), department1.PK.ToGuid()))
			{
				OrgHeader orgHeader = Factory.New<OrgHeader>();
				OrgOpportunity orgOpportunity = orgHeader.SalesOpportunities.AddNew();

				OrgOpportunityValue opportunityValue1 = orgOpportunity.ValueItems.AddNew();
				opportunityValue1.PV_Value = 100m;
				AssertEquals("BBB", orgOpportunity.P8_RX_NKEstimatedValueCurrency);
				AssertEquals("AAA", branch.Country.RN_RX_NKLocalCurrency);
				opportunityValue1.PV_Value = 0m;

				OrgOpportunity orgOpportunity2 = orgHeader.SalesOpportunities.AddNew();
				OrgOpportunityValue opportunityValue2 = orgOpportunity.ValueItems.AddNew();
				orgOpportunity.P8_RX_NKEstimatedValueCurrency = "CAD";
				opportunityValue2.PV_Value = 100m;
				AssertEquals("CAD", orgOpportunity.P8_RX_NKEstimatedValueCurrency);
			}
		}

		public void TestOnSaving_UpdateProspectiveSalesIfNeeded()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var opp1 = org1.SalesOpportunities.AddNew();
			var opp2 = org1.SalesOpportunities.AddNew();

			Factory.Save();

			var product = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var salesOpp1Only = Factory.New<OrgSales>();
			salesOpp1Only.OW_OH_Primary = org1.PK;
			salesOpp1Only.OW_MP_Product = product.Identifier;
			var tradeDetail1 = salesOpp1Only.TradeDetails.AddNew();
			opp1.AssociatedTradeLanesPivots.AddPivotFor(salesOpp1Only);

			var salesBothOpp = Factory.New<OrgSales>();
			salesBothOpp.OW_OH_Primary = org1.PK;
			salesBothOpp.OW_MP_Product = product.Identifier;
			var tradeDetail2 = salesBothOpp.TradeDetails.AddNew();
			opp1.AssociatedTradeLanesPivots.AddPivotFor(salesBothOpp);
			opp2.AssociatedTradeLanesPivots.AddPivotFor(salesBothOpp);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var opp1Reloaded = factory2.Load<OrgOpportunity>(opp1.PK);
			opp1Reloaded.P8_OH = org2.PK;
			factory2.Save();

			// pivot from opp1 to salesOpp1Only updated
			// pivot from opp1 to salesBothOpp removed
			AssertEquals(1, opp1Reloaded.AssociatedTradeLanesPivots.Count);
			var pivot = opp1Reloaded.AssociatedTradeLanesPivots[0];
			AssertEquals(opp1Reloaded.PK, pivot.SVP_ActivityId);
			AssertEquals(salesOpp1Only.PK, pivot.SVP_TradeId);
		}

		public void TestDuplicateSharedProspectiveSales()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer3 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer4 = Factory.NewWithValidTestData<OrgHeader>();

			var opp1 = org1.SalesOpportunities.AddNew();

			var productWithSalesOnlyAssociations = Factory.New<IOrgSalesProduct>();
			productWithSalesOnlyAssociations.MP_Code = "Foo";
			productWithSalesOnlyAssociations.MP_Name = "Foo";
			var productWithSalesAndDetailsAssociations = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);

			var sales1NotShared = Factory.New<OrgSales>();
			sales1NotShared.OW_OH_Primary = org1.PK;
			sales1NotShared.OW_OH_Buyer = buyer1.PK;
			sales1NotShared.OW_MP_Product = productWithSalesOnlyAssociations.Identifier;
			sales1NotShared.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			var tradeDetail1 = sales1NotShared.TradeDetails.AddNew();
			tradeDetail1.CurrentProspectPeriod.PAS_EstimatedProfit = 11m;

			var sales2NotShared = Factory.New<OrgSales>();
			sales2NotShared.OW_OH_Primary = org1.PK;
			sales2NotShared.OW_OH_Buyer = buyer2.PK;
			sales2NotShared.OW_MP_Product = productWithSalesAndDetailsAssociations.Identifier;
			sales2NotShared.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			var tradeDetail2 = sales2NotShared.TradeDetails.AddNew();
			tradeDetail2.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			tradeDetail2.CurrentProspectPeriod.PAS_EstimatedProfit = 22;

			var sales3Shared = Factory.New<OrgSales>();
			sales3Shared.OW_OH_Primary = org1.PK;
			sales3Shared.OW_OH_Buyer = buyer3.PK;
			sales3Shared.OW_MP_Product = productWithSalesOnlyAssociations.Identifier;
			sales3Shared.SalesAssociationPivotCollectionGlobal.AddNew(org1);
			sales3Shared.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			var tradeDetail31 = sales3Shared.TradeDetails.AddNew();
			tradeDetail31.CurrentProspectPeriod.PAS_EstimatedProfit = 31m;
			tradeDetail31.SalesAssociationPivotCollectionGlobal.AddNew(org1);
			var tradeDetail32 = sales3Shared.TradeDetails.AddNew();
			tradeDetail32.SalesAssociationPivotCollectionGlobal.AddNew(org1);
			tradeDetail32.CurrentProspectPeriod.PAS_EstimatedProfit = 32m;

			var sales4Shared = Factory.New<OrgSales>();
			sales4Shared.OW_OH_Primary = org1.PK;
			sales4Shared.OW_OH_Buyer = buyer4.PK;
			sales4Shared.OW_MP_Product = productWithSalesAndDetailsAssociations.Identifier;
			sales4Shared.SalesAssociationPivotCollectionGlobal.AddNew(org1);
			sales4Shared.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			var tradeDetail41 = sales4Shared.TradeDetails.AddNew();
			tradeDetail41.CurrentProspectPeriod.PAS_EstimatedProfit = 41m;
			tradeDetail41.SalesAssociationPivotCollectionGlobal.AddNew(org1);
			tradeDetail41.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			var tradeDetail42 = sales4Shared.TradeDetails.AddNew();
			tradeDetail42.SalesAssociationPivotCollectionGlobal.AddNew(org1);
			tradeDetail42.CurrentProspectPeriod.PAS_EstimatedProfit = 42m;
			var tradeDetail43 = sales4Shared.TradeDetails.AddNew();
			tradeDetail43.SalesAssociationPivotCollectionGlobal.AddNew(opp1);
			tradeDetail43.CurrentProspectPeriod.PAS_EstimatedProfit = 43m;

			Factory.Save();

			AssertEquals("PRE", 7, opp1.AssociatedTradeLanesPivots.Count);

			opp1.DuplicateSharedProspectiveSales();

			AssertEquals(7, opp1.AssociatedTradeLanesPivots.Count);
			var finalSales1NotShared = opp1.AssociatedTradeLanesPivots.Select(x => x.SalesValue as OrgSales).Where(x => x != null).Single(x => x.OW_OH_Buyer == buyer1.PK);
			var finalSales2NotShared = opp1.AssociatedTradeLanesPivots.Select(x => x.SalesValue as OrgSales).Where(x => x != null).Single(x => x.OW_OH_Buyer == buyer2.PK);
			var finalSales3Shared = opp1.AssociatedTradeLanesPivots.Select(x => x.SalesValue as OrgSales).Where(x => x != null).Single(x => x.OW_OH_Buyer == buyer3.PK);
			var finalSales4Shared = opp1.AssociatedTradeLanesPivots.Select(x => x.SalesValue as OrgSales).Where(x => x != null).Single(x => x.OW_OH_Buyer == buyer4.PK);
			AssertEquals("non-shared sales are not cloned", sales1NotShared.PK, finalSales1NotShared.PK);
			AssertEquals("non-shared sales are not cloned", sales2NotShared.PK, finalSales2NotShared.PK);
			AssertNotEquals("shared sales are cloned", sales3Shared.PK, finalSales3Shared.PK);
			AssertNotEquals("shared sales are cloned", sales4Shared.PK, finalSales4Shared.PK);

			AssertEquals("trade details count", 1, finalSales1NotShared.TradeDetails.Count);
			AssertEquals("trade details not cloned if not shared", tradeDetail1.PK, finalSales1NotShared.TradeDetails[0].PK);
			AssertEquals("trade details count", 1, finalSales2NotShared.TradeDetails.Count);
			AssertEquals("trade details not cloned if not shared", tradeDetail2.PK, finalSales2NotShared.TradeDetails[0].PK);

			AssertEquals("trade details cloned if shared", 2, finalSales3Shared.TradeDetails.Count);
			var clonedDetail31 = finalSales3Shared.TradeDetails.Cast<OrgTradeDetail>().Single(x => x.CurrentProspectPeriod.PAS_EstimatedProfit == tradeDetail31.CurrentProspectPeriod.PAS_EstimatedProfit);
			AssertNotEquals("tradeDetail is cloned", tradeDetail31.PK, clonedDetail31.PK);
			var clonedDetail32 = finalSales3Shared.TradeDetails.Cast<OrgTradeDetail>().Single(x => x.CurrentProspectPeriod.PAS_EstimatedProfit == tradeDetail32.CurrentProspectPeriod.PAS_EstimatedProfit);
			AssertNotEquals("tradeDetail is cloned", tradeDetail32.PK, clonedDetail32.PK);
			AssertEquals("product does not support associations for details", 0, clonedDetail31.SalesAssociationPivotCollectionGlobal.Count);
			AssertEquals("product does not support associations for details", 0, clonedDetail32.SalesAssociationPivotCollectionGlobal.Count);

			AssertEquals("trade details cloned if shared", 2, finalSales4Shared.TradeDetails.Count);
			var clonedDetail41 = finalSales4Shared.TradeDetails.Cast<OrgTradeDetail>().Single(x => x.CurrentProspectPeriod.PAS_EstimatedProfit == tradeDetail41.CurrentProspectPeriod.PAS_EstimatedProfit);
			var clonedDetail43 = finalSales4Shared.TradeDetails.Cast<OrgTradeDetail>().Single(x => x.CurrentProspectPeriod.PAS_EstimatedProfit == tradeDetail43.CurrentProspectPeriod.PAS_EstimatedProfit);
			AssertNotEquals("tradeDetail is cloned", tradeDetail41.PK, clonedDetail41.PK);
			AssertNotEquals("tradeDetail is cloned", tradeDetail43.PK, clonedDetail43.PK);

			AssertEquals("clone detail has pivot to opportunity", 1, clonedDetail41.SalesAssociationPivotCollectionGlobal.Count);
			AssertEquals(opp1.PK, clonedDetail41.SalesAssociationPivotCollectionGlobal[0].SVP_ActivityId);
			AssertEquals("clone detail has pivot to opportunity", 1, clonedDetail43.SalesAssociationPivotCollectionGlobal.Count);
			AssertEquals(opp1.PK, clonedDetail41.SalesAssociationPivotCollectionGlobal[0].SVP_ActivityId);
		}

		public void TestDuplicateSharedProspectiveSales_NoExceptionThrown()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();

			var sales = Factory.New<OrgSales>();
			sales.OW_IsTraded = false;
			sales.OW_OH_Primary = org1.PK;
			sales.OW_OH_Buyer = buyer1.PK;

			var opp1 = org1.SalesOpportunities.AddNew();

			var productWithSalesOnlyAssociations = Factory.New<IOrgSalesProduct>();
			productWithSalesOnlyAssociations.MP_Code = "Foo";
			productWithSalesOnlyAssociations.MP_Name = "Foo";
			var productWithSalesAndDetailsAssociations = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);

			sales.OW_MP_Product = productWithSalesOnlyAssociations.Identifier;
			sales.SalesAssociationPivotCollectionGlobal.AddNew(opp1);

			var detail = sales.TradeDetails.AddNew();
			AssertEquals(0, detail.ProspectPeriods.Count);

			detail.ProspectPeriodStart = new ZDate(2018, 1, 1);
			detail.ProspectPeriodEnd = new ZDate(2018, 3, 1);
			AssertEquals(12, detail.ProspectPeriods.Count);
			AssertEquals(12, detail.ProspectPeriods.Count(x => x.PAS_OH_Client == org1.PK));

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var sourceOpportunity = factory.Load<OrgOpportunity>(opp1.PK);
			var targetOpportunity = CreateOpportunity(false);

			targetOpportunity.P8_OpportunityDescription = sourceOpportunity.P8_OpportunityDescription;
			targetOpportunity.P8_PackageType = sourceOpportunity.P8_PackageType;
			targetOpportunity.P8_OpportunityType = sourceOpportunity.P8_OpportunityType;
			targetOpportunity.P8_DiscountAmount = sourceOpportunity.P8_DiscountAmount;
			targetOpportunity.P8_RentalMultiplier = sourceOpportunity.P8_RentalMultiplier;
			targetOpportunity.P8_OH = sourceOpportunity.P8_OH;
			targetOpportunity.P8_OC = sourceOpportunity.P8_OC;
			targetOpportunity.P8_GS_NKPrimarySalesPerson = sourceOpportunity.P8_GS_NKPrimarySalesPerson;
			targetOpportunity.P8_OA_AssignedOffice = sourceOpportunity.P8_OA_AssignedOffice;
			targetOpportunity.P8_OC_AssignedOfficeContact = sourceOpportunity.P8_OC_AssignedOfficeContact;
			targetOpportunity.P8_Source = sourceOpportunity.P8_Source;
			targetOpportunity.P8_SourceDetails = sourceOpportunity.P8_SourceDetails;
			targetOpportunity.P8_OH_ReferringOrganisation = sourceOpportunity.P8_OH_ReferringOrganisation;
			targetOpportunity.P8_OC_ReferringContact = sourceOpportunity.P8_OC_ReferringContact;

			ObjectFactory.Get<ITradeDetailClonerGUIManager>().CloneAllForTest(sourceOpportunity, targetOpportunity);
			factory.Save();

			targetOpportunity.P8_OH = org2.PK;
			targetOpportunity.AssociatedTradeLanesPivots.AddPivotFor(sales);
			targetOpportunity.DuplicateSharedProspectiveSales();
			AssertNoExceptionThrown(() => factory.Save());
		}

		public void TestHasUnsavedCopiedOrgTradePeriods()
		{
			var sourceOpportunity = CreateOpportunity(true);
			Factory.Save();

			var targetOpportunity = CreateOpportunity(false);
			targetOpportunity.OnCopyOrgOpportunity(sourceOpportunity);

			AssertEquals(sourceOpportunity.P8_OH, targetOpportunity.SourceOpportunityOrgPK);
			AssertEquals(false, targetOpportunity.HasUnsavedCopiedOrgTradePeriods);

			targetOpportunity.OnCopyOrgTradePeriod(Factory.New<OrgTradePeriod>());
			AssertEquals(true, targetOpportunity.HasUnsavedCopiedOrgTradePeriods);
		}

		public void TestDeleteOrphanedRecords()
		{
			var sourceOpportunity = CreateOpportunity(true);
			Factory.Save();

			var targetOpportunity = CreateOpportunity(false);
			targetOpportunity.OnCopyOrgOpportunity(sourceOpportunity);

			var orphanRecord = Factory.New<OrgTradeDetail>();
			targetOpportunity.OnCopyOrgTradeDetail(orphanRecord);

			targetOpportunity.DuplicateSharedProspectiveSales();
			AssertEquals(true, orphanRecord.IsDeleted);
		}

		#region Custom Fields

		[TestedType(typeof(OrgOpportunity))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#endregion

		#region Business Object Overrides

		public void TestHumanReadableName()
		{
			OrgOpportunity opportunity = CreateOpportunity(false);
			opportunity.P8_OpportunityID = "OpportunityID";
			AssertEquals("Opportunity (OpportunityID)", opportunity.HumanReadableName);
			opportunity.P8_OpportunityID = "";
			AssertEquals("Opportunity", opportunity.HumanReadableName);
		}

		public void TestHumanReadableShortcutName()
		{
			var opportunity = CreateOpportunity(false);

			opportunity.P8_OpportunityID = "blah";
			AssertEquals("blah", opportunity.HumanReadableShortcutName);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CODE";
			opportunity.P8_OH = org.PK;
			AssertEquals("blah - CODE", opportunity.HumanReadableShortcutName);

			opportunity.P8_OpportunityDescription = "description";
			AssertEquals("blah - CODE - description", opportunity.HumanReadableShortcutName);

			opportunity.P8_OH = ZGuid.Empty;
			AssertEquals("blah - description", opportunity.HumanReadableShortcutName);
		}

		#region Delete

		public void TestDelete()
		{
			OpportunityProcessTasks task = (OpportunityProcessTasks)Opportunity.WorkflowItems.AddNew();
			OrgOpportunityValue valueItem = Opportunity.ValueItems.AddNew();
			Opportunity.Delete();
			AssertEquals("No tasks on Opportunity", 0, Opportunity.WorkflowItems.Count);
			AssertEquals("No value items on Opportunity", 0, Opportunity.ValueItems.Count);
			AssertEquals("Opp Deleted", true, Opportunity.IsDeleted);
			AssertEquals("Dependent Task Deleted", true, task.IsDeleted);
			AssertEquals("Dependent Value Item Deleted", true, valueItem.IsDeleted);
		}

		public void TestCanDelete()
		{
			var opportunity = CreateOpportunity(true);
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			Factory.Save();

			AssertEquals(true, opportunity.CanDelete);

			agreement.CA0_FirstUsageDateUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();

			var reloadedOpportunity = new BusinessObjectFactory().Load<OrgOpportunity>(opportunity.PK);
			AssertEquals(false, reloadedOpportunity.CanDelete);
			AssertEquals("This opportunity's commission agreement has already been used for a commission pay out.", reloadedOpportunity.ReasonForNotAbleToDelete);
		}

		#endregion

		public void TestLogging()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			Factory.Save();

			AssertEquals("Autolog event reference description", "Opportunity " + opportunity.P8_OpportunityID, opportunity.Logs.AutoCreatedLog.SL_Reference);
		}

		#endregion

		public void TestAssignedOrgPKIsCalculated()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = CreateOpportunity(true);
			opp.P8_OA_AssignedOffice = org.MainAddress.PK;
			AssertEquals(org.PK, opp.AssignedOrgPK);
		}

		public void TestUniversalCopy()
		{
			var extendedEntitiesAttribute = (UniversalCopyWithExtendedEntitiesAttribute[])(typeof(OrgOpportunity).GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), false));
			AssertEquals(1, extendedEntitiesAttribute.Length);

			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])(typeof(OrgOpportunity).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false));
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertCollectionContains(OrgOpportunity.Schema.P8_OpportunityID, ignoreElementAttributes[0].ElementNames);
		}

		[TestDate(2020, 5, 10, 12, 0, 0)]
		public void TestStageProgress()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("TS1", (NoResString)"Test Stage 1", true);
			list.Add("TS2", (NoResString)"Test Stage 2", true);
			list.Add("TS3", (NoResString)"Test Stage 3 A very very very very very long stage description", true);
			list.Add("TS4", (NoResString)"", true);
			OrganisationsDataRegistry.Instance.OpportunityStages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var statusCollection = new OpportunityStatusCollection();
			statusCollection.Add("CS1", (NoResString)"Lost", effectiveAgreement: false, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
			statusCollection.Add("CS2", (NoResString)"Abandoned", effectiveAgreement: false, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Unsuccessful);
			statusCollection.Add("OP1", (NoResString)"Current", effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
			statusCollection.Add("OP2", (NoResString)"Suspended", effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statusCollection);

			var user1 = Factory.NewWithValidTestData<GlbStaff>();
			user1.GS_Code = "TS1";

			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			user2.GS_Code = "TS2";

			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = CreateOpportunity(true);
			opportunity.P8_OH = org1.PK;
			opportunity.P8_Status = "OP1";

			using (Env.SetTemporaryUserContext(user1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				opportunity.P8_Stage = "TS1";

				Factory.Save();

				opportunity.P8_OpportunityDescription = "A change to the opportunity without changing stage";

				Factory.Save();
			}

			AssertEquals(new ZDateTime(2020, 5, 10, 12, 0, 0), opportunity.P8_SystemCreateTimeUtc);
			AssertEquals(user1.GS_Code, opportunity.P8_SystemCreateUser);

			AssertEquals(1, opportunity.StageProgressCollection.Count);
			{
				var stageProgress = opportunity.StageProgressCollection.Last();
				AssertEquals("TS1", stageProgress.OSP_Stage);
				AssertEquals("Test Stage 1", stageProgress.OSP_StageDescription);
				AssertEquals(new ZDateTimeOffset(new ZDateTime(2020, 5, 10, 12, 0, 0)), stageProgress.OSP_DateStarted);
				AssertEquals(0, stageProgress.DaysInStage);
				AssertEquals(user1.GS_Code, stageProgress.OSP_SystemLastEditUser);
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			using (Env.SetTemporaryUserContext(user2.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				opportunity.P8_Stage = "TS2";

				Factory.Save();
			}

			AssertEquals(new ZDateTime(2020, 5, 11, 12, 0, 0), opportunity.P8_SystemLastEditTimeUtc);
			AssertEquals(user2.GS_Code, opportunity.P8_SystemLastEditUser);

			AssertEquals(2, opportunity.StageProgressCollection.Count);
			{
				var stageProgressFirst = opportunity.StageProgressCollection.First();
				AssertEquals(new ZDateTimeOffset(new ZDateTime(2020, 5, 11, 12, 0, 0)), stageProgressFirst.OSP_DateCompleted);
				var stageProgress = opportunity.StageProgressCollection.Last();
				AssertEquals("TS2", stageProgress.OSP_Stage);
				AssertEquals("Test Stage 2", stageProgress.OSP_StageDescription);
				AssertEquals(new ZDateTimeOffset(new ZDateTime(2020, 5, 11, 12, 0, 0)), stageProgress.OSP_DateStarted);
				AssertEquals(1, stageProgressFirst.DaysInStage);
				AssertEquals(user2.GS_Code, stageProgress.OSP_SystemLastEditUser);
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			opportunity.P8_Stage = "TS3";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(5);

			AssertEquals(3, opportunity.StageProgressCollection.Count);
			{
				var stageProgress = opportunity.StageProgressCollection.Last();
				AssertEquals(stageProgress.OSP_StageDescriptionInfo.MaxLength, stageProgress.OSP_StageDescription.Length);
				AssertStartsWith("Stage Description Starts With", "Test Stage 3", stageProgress.OSP_StageDescription);
				AssertEquals(5, stageProgress.DaysInStage);
			}

			opportunity.P8_Stage = "TS4";
			Factory.Save();

			AssertEquals(3, opportunity.StageProgressCollection.Count);
			{
				var stageProgress = opportunity.StageProgressCollection.Last();
				AssertEquals(new ZDateTimeOffset(new ZDateTime(2020, 5, 17, 12, 0, 0)), stageProgress.OSP_DateCompleted);
			}

			opportunity.P8_OpportunityDescription = "Change the Description only";
			Factory.Save();

			AssertEquals(3, opportunity.StageProgressCollection.Count);

			opportunity.P8_Stage = "TS1";
			Factory.Save();

			AssertEquals("Precondition: A new stage progress is added", 4, opportunity.StageProgressCollection.Count);
			Assert("Precondition: Date Completed is empty for the last stage progress", opportunity.StageProgressCollection.Last().OSP_DateCompleted.IsEmpty);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			opportunity.P8_Status = "OP2";
			Factory.Save();

			AssertEquals("No new stage progress is added", 4, opportunity.StageProgressCollection.Count);
			Assert("Date Completed does not change when status changes from open to open.", opportunity.StageProgressCollection.Last().OSP_DateCompleted.IsEmpty);

			opportunity.P8_Status = "CS1";
			Factory.Save();

			AssertEquals("No new stage progress is added", 4, opportunity.StageProgressCollection.Count);
			AssertEquals("Date Completed is filled for the last stage progress when status changes from open to closed", new ZDateTimeOffset(new ZDateTime(2020, 5, 18, 12, 0, 0)), opportunity.StageProgressCollection.Last().OSP_DateCompleted);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			opportunity.P8_Status = "CS2";
			Factory.Save();

			AssertEquals("No new stage progress is added", 4, opportunity.StageProgressCollection.Count);
			AssertEquals("Date Completed does not change when status changes from closed to closed", new ZDateTimeOffset(new ZDateTime(2020, 5, 18, 12, 0, 0)), opportunity.StageProgressCollection.Last().OSP_DateCompleted);

			opportunity.P8_Status = "OP1";
			Factory.Save();

			AssertEquals("No new stage progress is added", 4, opportunity.StageProgressCollection.Count);
			AssertEquals("Date Completed does not change when status changes from closed to open", new ZDateTimeOffset(new ZDateTime(2020, 5, 18, 12, 0, 0)), opportunity.StageProgressCollection.Last().OSP_DateCompleted);

			opportunity.P8_Stage = "TS2";
			Factory.Save();

			AssertEquals("Precondition: A new stage progress is added", 5, opportunity.StageProgressCollection.Count);
			Assert("Precondition: Date Completed is empty for the last stage progress", opportunity.StageProgressCollection.Last().OSP_DateCompleted.IsEmpty);

			opportunity.P8_Stage = "TS1";
			opportunity.P8_Status = "CS1";
			Factory.Save();

			AssertEquals("A new stage is added when stage and status (Open to Closed) change at the same time.", 6, opportunity.StageProgressCollection.Count);
			AssertEquals("Date Completed updated for the existing stage progress", new ZDateTimeOffset(new ZDateTime(2020, 5, 19, 12, 0, 0)), opportunity.StageProgressCollection[4].OSP_DateCompleted);
			AssertEquals("Date Completed updated for the new stage progress", new ZDateTimeOffset(new ZDateTime(2020, 5, 19, 12, 0, 0)), opportunity.StageProgressCollection.Last().OSP_DateCompleted);
		}

		public void TestIsEditCommissionAgreementsAllowed()
		{
			var opp = CreateOpportunity(false);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = true;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = false;
			AssertEquals(true, opp.CreateCommissionAgreementsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = true;
			AssertEquals(true, opp.CreateCommissionAgreementsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = true;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = true;
			AssertEquals(true, opp.CreateCommissionAgreementsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = false;
			AssertEquals(false, opp.CreateCommissionAgreementsAllowed);
		}

		public void TestUpdateIDWhenAttachingRelatedProject()
		{
			var opportunity = CreateOpportunity(false);
			var otherOpportunity = CreateOpportunity(false);
			opportunity.P8_OpportunityID = "O00010011";
			otherOpportunity.P8_OpportunityID = "O00010022";

			var project = Factory.New<IProject>();
			project.WKP_ProjectNumber = "P00010011";

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			var importDeciderFactory = new ImportRelatedActivityNoDecisionFactory();

			var importer = opportunity as IImportChildRelatedActivityInfoOnAttach;

			project.WKP_P8_Opportunity = otherOpportunity.PK;
			AssertEquals("Opportunity on Project should be changed", true, importer.ImportChildInfo((IRelatableActivity)project, importDeciderFactory));
			AssertEquals(opportunity.PK, project.WKP_P8_Opportunity);

			AssertEquals("Sales Enquiry should not cause exception", true, importer.ImportChildInfo(enquiry, importDeciderFactory));
		}

		public void TestUpdateIDWhenDetachingRelatedProject()
		{
			var opportunity = CreateOpportunity(false);
			var otherOpportunity = CreateOpportunity(false);
			opportunity.P8_OpportunityID = "O00010011";
			otherOpportunity.P8_OpportunityID = "O00010022";

			var project = Factory.New<IProject>();
			project.WKP_ProjectNumber = "P00010011";

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			var importDeciderFactory = new ImportRelatedActivityNoDecisionFactory();

			var importer = opportunity as IImportChildRelatedActivityInfoOnDetach;

			project.WKP_P8_Opportunity = otherOpportunity.PK;
			AssertEquals("Opportunity on Project should be cleared", true, importer.ImportChildInfo((IRelatableActivity)project, importDeciderFactory));
			AssertEquals(ZGuid.Empty, project.WKP_P8_Opportunity);

			AssertEquals("Sales Enquiry should not cause exception", true, importer.ImportChildInfo(enquiry, importDeciderFactory));
		}

		public void TestHtmlProperty()
		{
			var orgOpportunity = (OrgOpportunity)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, orgOpportunity.P8_OpportunityNotes);
			AssertEquals(ZBlob.Empty, orgOpportunity.P8_OpportunityNotes_HTML);

			orgOpportunity.P8_OpportunityNotes_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(orgOpportunity.P8_OpportunityNotes.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", orgOpportunity.P8_OpportunityNotes_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var orgOpportunity = (OrgOpportunity)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, orgOpportunity.P8_OpportunityNotes);
			AssertEquals(ZBlob.Empty, orgOpportunity.P8_OpportunityNotes_HTML);

			orgOpportunity.P8_OpportunityNotes = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", orgOpportunity.P8_OpportunityNotes_HTML.ToUTF8());

			orgOpportunity.P8_OpportunityNotes = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", orgOpportunity.P8_OpportunityNotes_HTML.ToUTF8());
		}

		#region Implementation

		OrgOpportunity Opportunity
		{
			get
			{
				if (opportunity == null)
				{
					opportunity = CreateOpportunity(false);
				}
				return opportunity;
			}
		}
		OrgOpportunity opportunity;

		protected virtual OrgOpportunity CreateOpportunity(bool withValidTestData)
		{
			return withValidTestData ? Factory.NewWithValidTestData<OrgOpportunity>() : Factory.New<OrgOpportunity>();
		}

		GlbStaff AddNewSalesStaff(OrgHeader organisation, ZGuid companyPk)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffAssignment = organisation.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssignment.O8_Role = "SAL";
			staffAssignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssignment.O8_GC = companyPk;

			return staff;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var opp = (OrgOpportunity)base.GetBusinessObjectForFetchForLoad();
			AddPivotsToOpportunity(opp);
			return opp;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var opp = (OrgOpportunity)base.GetNewBusinessObjectForDeleteTest(factory);
			AddPivotsToOpportunity(opp);
			return opp;
		}

		void AddPivotsToOpportunity(OrgOpportunity opp)
		{
			foreach (var pivot in opp.AssociatedTradeLanesPivots)
			{
				var sales = Factory.NewWithValidTestData<OrgSales>();
				pivot.SVP_ActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
				pivot.SVP_ActivityId = opp.PK;
				pivot.SVP_TradeTableCode = OrgSalesSchema.Constants.Prefix;
				pivot.SVP_TradeId = sales.PK;
			}
		}

		#endregion
	}
}
