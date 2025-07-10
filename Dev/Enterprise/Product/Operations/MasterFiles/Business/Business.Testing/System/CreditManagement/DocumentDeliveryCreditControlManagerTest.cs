using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Organisation.Registry;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocumentDeliveryCreditControlManagerTest : TestCaseWithFactory
	{
		public void TestWarningMessageShouldOnlyMentionOutstandingCashAdvanceRequestWhenIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryIsEnabled()
		{
			var consignor = CreditControlledBizo.CreateOrganization(Factory, "ABC");
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000011";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_OH_SellAccount = consignor.PK;
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.Australia;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();

			CreditControlledBizo.SetOrganizationCreditOnHold(consignor, true); //consignor on credit hold

			AssertWarningMessage(false);
			AssertWarningMessage(true);

			void AssertWarningMessage(bool isRegistryEnabled)
			{
				consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				using (ObjectFactory.Get<IAccounting>().SetupIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation(Guid.Empty, isRegistryEnabled))
				{
					var status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement((BusinessObject)shipment, "document", menuItem.PK);
					if (isRegistryEnabled)
					{
						var expectedStatus =
@"Delivery of this document is restricted because:
       The Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold, OR
	  d) Has an outstanding Advance Payment Request.";
						AssertContains(expectedStatus, status);
					}
					else
					{
						var expectedStatus =
@"Delivery of this document is restricted because:
       The Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.";
						AssertContains(expectedStatus, status);
					}
				}
			}
		}

		public void TestCheckOutstandingCashAdvanceRequests()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGAA1";

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000011";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_OH_SellAccount = org.PK;
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.Australia;
			charge.JR_OSSellAmt = 100m;

			AssertCheckOutstandingCashAdvanceRequests(false, false);
			AssertCheckOutstandingCashAdvanceRequests(false, true);
			AssertCheckOutstandingCashAdvanceRequests(true, false);
			AssertCheckOutstandingCashAdvanceRequests(true, true);

			void AssertCheckOutstandingCashAdvanceRequests(bool isRegistryEnabled, bool hasOutstandingCashAdvance)
			{
				charge.JR_IsARCashAdvance = hasOutstandingCashAdvance;
				Factory.Save();

				using (ObjectFactory.Get<IAccounting>().SetupIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation(Guid.Empty, isRegistryEnabled))
				{
					var status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement((BusinessObject)shipment, "document", menuItem.PK);
					if (isRegistryEnabled && hasOutstandingCashAdvance)
					{
						var expectedStatus =
@"Delivery of this document is restricted because:
       The Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold, OR
	  d) Has an outstanding Advance Payment Request.";
						AssertContains(expectedStatus, status);
					}
					else
					{
						AssertEquals("", status);
					}
				}
			}
		}

		public void TestGetOrganisationsInBreachAndTheirBreachReasons()
		{
			var localClient = CreditControlledBizo.CreateOrganization(Factory, "ABC");
			var consignor = CreditControlledBizo.CreateOrganization(Factory, "PQR");
			var consignee = CreditControlledBizo.CreateOrganization(Factory, "XYZ");

			Action clearCreditCheckerCacheForTest = () =>
			{
				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				consignee.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			};

			var shipment = Factory.New<IForwardingShipment>();
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = localClient.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			Factory.Save();

			clearCreditCheckerCacheForTest();
			var result = new DocumentDeliveryCreditControlManager().GetOrganisationsInBreachAndTheirBreachReasons((BusinessObject)shipment);
			AssertEquals("There should be no organisations in breach", 0, result.Count);

			CreditControlledBizo.SetOrganizationCreditOnHold(localClient, true); //local client on credit hold
			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			CreditControlledBizo.SetOrganizationCredit(consignor, 100m, 50m); // consignor is over credit limit

			clearCreditCheckerCacheForTest();
			result = new DocumentDeliveryCreditControlManager().GetOrganisationsInBreachAndTheirBreachReasons((BusinessObject)shipment);
			AssertEquals("There should be only two organisations in breach", 2, result.Count);
			AssertContainsExactElementsInAnyOrder(new[] { localClient, consignor }, result.Keys);
			AssertEquals("Credit On Hold", result[localClient]);
			AssertEquals("Over Credit Limit", result[consignor]);

			var unpaidPIAInvoiceForConsignee = GetInvoiceForTest(GlbBranch.CurrentBranch, shipment.JS_UniqueConsignRef, false);
			unpaidPIAInvoiceForConsignee.AH_OH = consignee.PK;

			var mockCreditControlledDocumentsCheckConfiguration = new Mock<ICreditControlledDocumentsCheckConfiguration>();
			mockCreditControlledDocumentsCheckConfiguration.Setup(m => m.InvoiceType).Returns(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code);
			mockCreditControlledDocumentsCheckConfiguration.Setup(m => m.Amount).Returns(1m);
			mockCreditControlledDocumentsCheckConfiguration.Setup(m => m.Range).Returns(AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above);
			mockCreditControlledDocumentsCheckConfiguration.Setup(m => m.AuthorisationRequirement).Returns(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			mockCreditControlledDocumentsCheckConfiguration.Setup(m => m.NumberOfDaysOverdue).Returns(1);

			var mockRegistry = new Mock<IRegistryItem>();
			mockRegistry.Setup(r => r.Value).Returns(true);

			var autoJRJHelperMock = new Mock<IAutoJobRevenueJournalHelper>();
			autoJRJHelperMock.Setup(x => x.IsAutoJRJEnabled).Returns(true);

			var accountingMock = new Mock<IAccounting>();
			accountingMock.Setup(m => m.GetCreditControlledDocumentsCheckConfiguration()).Returns(new ICreditControlledDocumentsCheckConfiguration[] { mockCreditControlledDocumentsCheckConfiguration.Object });
			accountingMock.Setup(m => m.IncludeUnpostedRevenueInCreditLimitCalculation).Returns(mockRegistry.Object);
			accountingMock.Setup(m => m.IncludeUnpostedRevenueInGlobalCreditLimitCalculation).Returns(mockRegistry.Object);
			using (ObjectFactory.Substitute(accountingMock.Object))
			using (ObjectFactory.Substitute(autoJRJHelperMock.Object))
			{
				var overdueInvoiceForConsignee = GetInvoiceForTest(GlbBranch.CurrentBranch, "INV123", false);
				overdueInvoiceForConsignee.AH_OH = consignee.PK;
				overdueInvoiceForConsignee.AH_InvoiceAmount = overdueInvoiceForConsignee.AH_OutstandingAmount = 10m;
				overdueInvoiceForConsignee.AH_PostDate = ZDateTime.Today.AddDays(-10);
				overdueInvoiceForConsignee.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
				overdueInvoiceForConsignee.AH_DueDate = ZDateTime.Today.AddDays(-5);
				overdueInvoiceForConsignee.AH_InvoiceTerm = Core.Constants.InvoiceTerms.CashOnDelivery;

				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				Factory.Save();

				clearCreditCheckerCacheForTest();
				result = new DocumentDeliveryCreditControlManager().GetOrganisationsInBreachAndTheirBreachReasons((BusinessObject)shipment);
				AssertEquals("There should be only three organisations in breach", 3, result.Count);
				AssertContainsExactElementsInAnyOrder(new[] { localClient, consignor, consignee }, result.Keys);
				AssertEquals("Credit On Hold", result[localClient]);
				AssertEquals("Over Credit Limit", result[consignor]);
				AssertEquals("Overdue Transactions,Credit Term PIA", result[consignee]);

				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = job.PK;
				charge.JR_OH_SellAccount = consignor.PK;
				charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
				charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.Australia;
				charge.JR_OSSellAmt = 100m;
				charge.JR_IsARCashAdvance = true;
				Factory.Save();

				clearCreditCheckerCacheForTest();
				result = new DocumentDeliveryCreditControlManager().GetOrganisationsInBreachAndTheirBreachReasons((BusinessObject)shipment);
				AssertEquals("There should be only three organisations in breach", 3, result.Count);
				AssertContainsExactElementsInAnyOrder(new[] { localClient, consignor, consignee }, result.Keys);
				AssertEquals("Credit On Hold", result[localClient]);
				AssertEquals("Over Credit Limit,Unpaid Advance Payment Request", result[consignor]);
				AssertEquals("Overdue Transactions,Credit Term PIA", result[consignee]);

				accountingMock.VerifyAll();
				mockRegistry.VerifyAll();
				autoJRJHelperMock.VerifyAll();
			}
		}

		public void TestGetSecurityLoginEventArgs()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			CreditControlledBizo bizo = Factory.New<CreditControlledBizo>();
			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);
			var manager = new DocumentDeliveryCreditControlManager();
			manager.GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			var tester = manager.GetSecurityLoginEventArgs();
			AssertEquals(menuItem.PK, tester.MenuItemPK);
			AssertEquals(bizo.PK, tester.ParentBusinessObject.PK);
			AssertEquals(ZString.Empty, tester.DefaultApprovalRequestReason);
			tester = manager.GetSecurityLoginEventArgs(null);
			AssertEquals(menuItem.PK, tester.MenuItemPK);
			AssertEquals(bizo.PK, tester.ParentBusinessObject.PK);
			AssertEquals(ZString.Empty, tester.DefaultApprovalRequestReason);
			tester = manager.GetSecurityLoginEventArgs("");
			AssertEquals(menuItem.PK, tester.MenuItemPK);
			AssertEquals(bizo.PK, tester.ParentBusinessObject.PK);
			AssertEquals(ZString.Empty, tester.DefaultApprovalRequestReason);
			tester = manager.GetSecurityLoginEventArgs("ABC");
			AssertEquals(menuItem.PK, tester.MenuItemPK);
			AssertEquals(bizo.PK, tester.ParentBusinessObject.PK);
			AssertEquals("ABC", tester.DefaultApprovalRequestReason);
		}

		public void TestShouldStopDelivery()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "Document Name";

			var bizo = Factory.New<IForwardingShipment>();
			var manager = new DocumentDeliveryCreditControlManager();

			((IComplianceItemRiskStatusProvider)bizo).InitializeComplianceWorkflowPopupIfNeeded = () => { return DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery; };
			AssertEquals(false, manager.ShouldStopDelivery(bizo as BusinessObject));

			((IComplianceItemRiskStatusProvider)bizo).InitializeComplianceWorkflowPopupIfNeeded = () => { return DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery; };
			AssertEquals(true, manager.ShouldStopDelivery(bizo as BusinessObject));
		}

		[TestDate]
		public void TestGetDocumentDeliveryStatusForCreditManagement_AuthorizationLevelWhenCreditOnHoldWithOverCreditAndNoUserRights()
		{
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);

			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly);
			var creditControllerOverrideThresholdLevel = 2;

			CreditControlledBizo.SetOrganizationCredit(orgHeader, 10, 1);

			var shipment = Factory.New<IForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = orgHeader.MainAddress.PK;

			Factory.Save();

			var expectedAuthorizationLevel = Array.Empty<int>();
			var getDocumentLoginCallCounter = 0;
			var expectedGetDocumentLoginCallCounter = 0;
			((ICreditControlledDocumentDelivery)shipment).GetDocumentLogin += (object sender, SecurityLoginEventArgs e) =>
			{
				var eventWithApproval = (SecurityLoginEventArgsForDocumentApproval)e;
				AssertContainsExactElementsInAnyOrder("AuthorizationLevel", expectedAuthorizationLevel, eventWithApproval.AuthorizationLevel);
				getDocumentLoginCallCounter++;
			};

			Action assertAuthorizationLevel = () =>
			{
				orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement((BusinessObject)shipment, "document", menuItem.PK);
				AssertEquals(nameof(getDocumentLoginCallCounter), ++expectedGetDocumentLoginCallCounter, getDocumentLoginCallCounter);
			};

			var defaultLevelForCreditOnHold = 3;
			var userLevel = 0; //no rights
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(orgHeader.CompanyData, authorizingUser, userLevel);
			expectedAuthorizationLevel = new[] { defaultLevelForCreditOnHold };
			assertAuthorizationLevel();

			userLevel = 1;
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(orgHeader.CompanyData, authorizingUser, userLevel);
			expectedAuthorizationLevel = new[] { creditControllerOverrideThresholdLevel };
			assertAuthorizationLevel();

			var creditControlsModifiedEvents = orgHeader.CompanyData.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));
			foreach (var creditControlsModifiedEvent in creditControlsModifiedEvents)
			{
				using (creditControlsModifiedEvent.LockForUpdatingKeyFieldsForTesting())
				{
					creditControlsModifiedEvent.SL_Reference = ZString.Empty;
				}
			}
			Factory.Save();

			expectedAuthorizationLevel = new[] { defaultLevelForCreditOnHold };
			assertAuthorizationLevel();

			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, false);
			expectedAuthorizationLevel = new[] { creditControllerOverrideThresholdLevel };
			assertAuthorizationLevel();

			userLevel = 3;
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(orgHeader.CompanyData, authorizingUser, userLevel);
			expectedAuthorizationLevel = new[] { userLevel };
			assertAuthorizationLevel();

			userLevel = 2;
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(orgHeader.CompanyData, authorizingUser, userLevel);
			AssertEquals("Levels are equal so doesn't matter which one be set as expectedAuthorizationLevel.", userLevel, creditControllerOverrideThresholdLevel);
			expectedAuthorizationLevel = new[] { userLevel, 3 };
			assertAuthorizationLevel();

			CreditControlledBizo.ClearCreditControllerOverrideThreshold();
			creditControllerOverrideThresholdLevel = 0;
			userLevel = 1;
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(orgHeader.CompanyData, authorizingUser, userLevel);
			expectedAuthorizationLevel = new[] { userLevel, 2, 3 };
			assertAuthorizationLevel();

			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, false);
			expectedGetDocumentLoginCallCounter--; //as GetDocumentLogin should not be called at all
			expectedAuthorizationLevel = Array.Empty<int>();
			assertAuthorizationLevel();
		}

		public void TestGetDocumentDeliveryStatusForCreditManagement()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "Document Name";

			CreditControlledBizo bizo = Factory.New<CreditControlledBizo>();
			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);
			string status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("State is valid", "", status);

			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			bizo.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			ContinueToPrint = true;
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
			status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("Credit On Hold but Continuing - so no error", "", status);
			AssertContains("on Credit Hold", LastLoginMessage);
			AssertEquals("Credit Hold Status Overridden", bizo.Logs.MostRecentLog.SL_Reference);

			LastLoginMessage = "";
			ContinueToPrint = false;
			status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("Credit On Hold and NOT Continuing", "Cancelled", status);
			AssertContains("on Credit Hold", LastLoginMessage);
			AssertEquals("Credit Hold Status Overridden", bizo.Logs.MostRecentLog.SL_Reference);

			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, false);
			orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("State is valid", "", status);

			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			CreditControlledBizo.SetOrganizationCredit(orgHeader, 2, 1);

			status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("Over Limit and NOT Continuing - so error", "Cancelled", status);
			AssertContains("above their Credit Limit", LastLoginMessage);
			AssertEquals("Credit Hold Status Overridden", bizo.Logs.MostRecentLog.SL_Reference);

			ContinueToPrint = true;
			status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("Over Limit but Continuing - so no error", "", status);
			AssertContains("above their Credit Limit", LastLoginMessage);
			AssertEquals("At or Over Credit Limit", bizo.Logs.MostRecentLog.SL_Reference);
			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired);

			GetInvoiceForTest(GlbCompany.CurrentCompany.Branches[0], "123");

			ContinueToPrint = false;
			status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("PIA and not continuing", "Cancelled", status);
			AssertContains("Has unpaid PIA", LastLoginMessage);
			AssertEquals("At or Over Credit Limit", bizo.Logs.MostRecentLog.SL_Reference);

			ContinueToPrint = true;
			status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			AssertEquals("PIA but Continuing - so no error", "", status);
			AssertContains("Has unpaid PIA", LastLoginMessage);
			AssertEquals("Unpaid Payment in Advance Transactions", bizo.Logs.MostRecentLog.SL_Reference);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var prevoiusEventTime = bizo.Logs.MostRecentLog.SL_EventTime;
				Thread.Sleep(1);
				bizo.IsDPSFreightMovementRestrictedForTest = true;
				ContinueToPrint = true;
				status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
				AssertEquals("Screening Status is not Clear but continuing- so no error", "", status);
				AssertContains("Screening Status is not Clear", LastLoginMessage);
				var newLogs = bizo.Logs.Find(new ZQuery(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThan, prevoiusEventTime));
				AssertEquals(2, newLogs.Length);
				AssertEquals("Denied Party Hold Status Overridden|Document Name", newLogs.OrderBy(log => log.SL_Reference).First().SL_Reference);

				bizo.IsDPSFreightMovementRestrictedForTest = true;
				ContinueToPrint = false;
				status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
				AssertEquals("Screening Status is not Clear and not continuing", "Cancelled", status);
				AssertContains("Screening Status is not Clear", LastLoginMessage);
			}
		}

		public void TestGetDocumentDeliveryStatusForCreditManagement_InitializeComplianceWorkflowPopupIfNeeded()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "Document Name";

			var manager = new DocumentDeliveryCreditControlManager();
			var bizo = Factory.New<IForwardingShipment>();
			var popupCount = 0;
			((IComplianceItemRiskStatusProvider)bizo).InitializeComplianceWorkflowPopupIfNeeded = () =>
			{
				popupCount++;
				return DocumentDeliveryResultForComplianceWorkflow.ContinueDocumentDelivery;
			};
			var status = manager.GetDocumentDeliveryStatusForCreditManagement((BusinessObject)bizo, "document", menuItem.PK);
			AssertEquals("State is valid", string.Empty, status);
			AssertEquals("Compliance Initialization form not pops up.", 0, popupCount);

			var complianceRiskStatusSupporterMock = new Mock<IComplianceRiskStatusSupporter>();
			complianceRiskStatusSupporterMock.Setup(m => m.IsDPSFreightMovementRestricted(It.IsAny<ZString>(), It.IsAny<IComplianceJobDirectionProvider>(), It.IsAny<BusinessObject>())).Returns(() => true);

			using (ObjectFactory.Substitute(complianceRiskStatusSupporterMock.Object))
			{
				status = manager.GetDocumentDeliveryStatusForCreditManagement((BusinessObject)bizo, "document", menuItem.PK);
				AssertContains("Delivery is restricted", "Delivery of this document is restricted", status);
				AssertEquals("Compliance Initialization form pops up.", 1, popupCount);

				((IComplianceItemRiskStatusProvider)bizo).InitializeComplianceWorkflowPopupIfNeeded = () =>
				{
					popupCount++;
					return DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery;
				};
				status = manager.GetDocumentDeliveryStatusForCreditManagement((BusinessObject)bizo, "document", menuItem.PK);
				AssertEquals("Cancel delivery", "Document Delivery canceled.", status);
				AssertEquals("Compliance Initialization form pops up.", 2, popupCount);
			}
		}

		public void TestConcurrencyErrorOnCreditControlManagement()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var bizo = Factory.New<CreditControlledBizo>();
			GetInvoiceForTest(GlbCompany.CurrentCompany.Branches[0], "123");

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var bizO2 = factory2.Load<CreditControlledBizo>(bizo.PK);
			((ICreditControlledDocumentDelivery)bizO2).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);

			bizo.Z0_Short = 1;
			bizO2.Z0_Short = 2;
			ConcurrencyInfo.SetConcurrencyPolicy(bizO2, nameof(CreditControlledBizo.Z0_Short), ConcurrencyPolicy.Observe);

			bizO2.IsDPSFreightMovementRestrictedForTest = true;
			ContinueToPrint = true;
			Factory.Save();

			AssertNoExceptionThrown(delegate
			{ new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizO2, "document", menuItem.PK); });
		}

		public void TestConcurrencyErrorWithStrictPolicy()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var bizo = Factory.New<CreditControlledBizo>();
			Factory.Save();

			GlbStaff serviceUser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, User.ServiceUserName));
			using (Env.SetTemporaryUserContext(serviceUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var bizO2 = factory2.Load<CreditControlledBizo>(bizo.PK);

				bizO2.Z0_Short = 2;
				factory2.Save();
			}

			bizo.Z0_Short = 2;
			ConcurrencyInfo.SetConcurrencyPolicy(bizo, nameof(CreditControlledBizo.Z0_Short), ConcurrencyPolicy.Strict);
			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);
			bizo.IsDPSFreightMovementRestrictedForTest = true;
			ContinueToPrint = true;

			AssertNoExceptionThrown(delegate
			{ new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK); });
		}

		public void TestGetDocumentDeliveryStatusForCreditManagementWithUnclearScreeningStatus()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			const int EXPECTED_LOGIN_MESSAGES = 1;
			const string CREDIT_ON_HOLD = "on Credit Hold";
			const string SCREENING_NOT_CLEAR = "Screening Status is not Clear";

			LoginMessages.Clear();

			CreditControlledBizo bizo = Factory.New<CreditControlledBizo>();
			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);

			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			bizo.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			ContinueToPrint = true;
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
			bizo.IsDPSFreightMovementRestrictedForTest = true;
			new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK, false);

			AssertEquals("Should only have 1 login message", EXPECTED_LOGIN_MESSAGES, LoginMessages.Count);
			AssertNotContains("Login Message should not contain " + CREDIT_ON_HOLD, CREDIT_ON_HOLD, LastLoginMessage);
			AssertContains("Login Message should contain " + SCREENING_NOT_CLEAR, SCREENING_NOT_CLEAR, LastLoginMessage);
		}

		public void TestGetDocumentDeliveryStatusForCreditManagementWithBothCreditOnHoldAndUnclearScreeningStatus()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			const int EXPECTED_LOGIN_MESSAGES = 1;
			const string CREDIT_ON_HOLD = "on Credit Hold";
			const string SCREENING_NOT_CLEAR = "Screening Status is not Clear";

			LoginMessages.Clear();

			CreditControlledBizo bizo = Factory.New<CreditControlledBizo>();
			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);

			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			bizo.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			ContinueToPrint = true;
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
			bizo.IsDPSFreightMovementRestrictedForTest = true;
			new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);

			AssertEquals("Should only have 1 login message", EXPECTED_LOGIN_MESSAGES, LoginMessages.Count);
			AssertContains("Login Message should contain " + CREDIT_ON_HOLD, CREDIT_ON_HOLD, LastLoginMessage);
			AssertContains("Login Message should contain " + SCREENING_NOT_CLEAR, SCREENING_NOT_CLEAR, LastLoginMessage);
		}

		[ExpectNoExceptions]
		public void TestGetDocumentDeliveryStatusFromBOThatDoesNotImplementICreditControlledDocumentDeliveryDoesNotBlowUp()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var bo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement(bo, "document", menuItem.PK);
			AssertEquals(string.Empty, status);
		}

		public void TestContentOfTracerInfo_WhenOn()
		{
			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var bizo = Factory.New<CreditControlledBizo>();
			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);
			var manager = new DocumentDeliveryCreditControlManager();
			manager.GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);

			var actualTraceMessage = string.Join("", dummyTracer.Traces);

			AssertContains("Error Key", "CreditCheckDiagnosticInfo_DocumentDelivery_2", actualTraceMessage);
			AssertContains("DocumentDeliveryCreditControlManager", actualTraceMessage);
			AssertContains("IsDPSFreightMovementRestricted", actualTraceMessage);
			AssertContains("IsAviationSecurityFreightMovementRestricted", actualTraceMessage);
			AssertContains("creditCheckEnabled", actualTraceMessage);
			AssertContains("IsRestricted", actualTraceMessage);
			AssertContains("IsAllowedToProceed", actualTraceMessage);

			AssertContains("DocumentDeliveryCreditControlHelper", actualTraceMessage);
			AssertContains("IsDPSFreightMovementRestricted", actualTraceMessage);
			AssertContains("HasUnpaidPIAInvoices", actualTraceMessage);
			AssertContains("OrgsForCreditChecks", actualTraceMessage);
			AssertContains("IsOverGlobalCreditLimit", actualTraceMessage);
			AssertContains("AuthorizationLevelRequiredDueToCreditLimitExceeding", actualTraceMessage);
			AssertContains("AuthorizationLevelRequiredDueToCreditCheck", actualTraceMessage);
			AssertContains("HasCreditControlledDocumentsApproval", actualTraceMessage);

			dummyTracer.Traces.Clear();
		}

		public void TestContentOfTracerInfo_WhenOff()
		{
			var dummyTracer = new DummyTracer(["none"]);
			ObjectFactory.Substitute<ITracer>(dummyTracer);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var bizo = Factory.New<CreditControlledBizo>();
			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);
			var manager = new DocumentDeliveryCreditControlManager();
			manager.GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);

			var actualTraceMessage = string.Join("", dummyTracer.Traces);

			AssertEquals("Error Key & Message", string.Empty, actualTraceMessage);

			dummyTracer.Traces.Clear();
		}

		public void TestExtraAviationSecurityFreightMovementRestrictedChangedbyDocumentDirection()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			CreditControlledBizo bizo = Factory.New<CreditControlledBizo>();
			bizo.IsAviationSecurityFreightMovementRestrictedTest = true;

			((ICreditControlledDocumentDelivery)bizo).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);

			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);
			var manager = new DocumentDeliveryCreditControlManager();
			manager.GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK);
			manager.GetDocumentDeliveryStatusForCreditManagement(bizo, "document", menuItem.PK, true, nameof(DocumentDirection.ARV));

			var actualTraceMessage = string.Join("", dummyTracer.Traces);
			AssertContains("IsAviationSecurityFreightMovementRestricted: True", actualTraceMessage);
			AssertContains("IsAviationSecurityFreightMovementRestricted: False", actualTraceMessage);
			dummyTracer.Traces.Clear();
		}

		public void TestDocumentHoldStatusOverridden_ShouldCreateStmAlogAndStmComplianceEvent()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "Document Name";

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.ComplianceRiskStatus
(
	COR_PK, COR_ParentTableCode, COR_ParentID, COR_PartyRisk, COR_LocationRisk, COR_OverallRisk, COR_CommodityRisk,
	COR_SystemCreateTimeUtc, COR_SystemLastEditTimeUtc, COR_SystemCreateUser, COR_SystemLastEditUser
)
VALUES
(
	NEWID(), 'JS', '{shipment.PK}', 'PSK', 'PSK', 'PSK', 'PSK',
	GETUTCDATE(), GETUTCDATE(), '~BP', '~BP'
)");

			((ICreditControlledDocumentDelivery)(BusinessObject)shipment).GetDocumentLogin += new EventHandler<SecurityLoginEventArgs>(CreditControlManagerTest_GetDocumentLogin);
			ContinueToPrint = true;

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.ComplianceRiskFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var status = new DocumentDeliveryCreditControlManager().GetDocumentDeliveryStatusForCreditManagement((BusinessObject)shipment, "document", menuItem.PK);
				AssertEquals("State is valid", string.Empty, status);
				AssertEquals("StmComplianceEvent Log:", "HSO|OVR|Document Hold Status Overridden|Document Name", TestConnection.ExecuteScalar<string>($@"
SELECT CONCAT(SCE_EventType, '|', SCE_EventSubType, '|', TRIM(SCE_EventReference))
FROM dbo.StmComplianceEvent
WHERE SCE_ParentID = '{shipment.PK}'"));

				var query = new ZQuery(StmALogSchema.SL_Parent, shipment.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "HSO");
				AssertEquals("StmALog Log:", "|MST=Compliance Risk|Document Hold Status Overridden|Document Name", Factory.Load<StmALog>(query).LastOrDefault().SL_Reference);
			}
		}

		#region Implementation

		void CreditControlManagerTest_GetDocumentLogin(object sender, SecurityLoginEventArgs e)
		{
			e.IsAllowedToProceed = ContinueToPrint;
			LastLoginMessage = e.LoginPromptMessage;
			LoginMessages.Add(e.LoginPromptMessage);
			e.MessageToShowWhenNotAllowed = (NoResString)"Cancelled";
		}

		readonly List<string> LoginMessages = new List<string>();
		string LastLoginMessage;
		bool ContinueToPrint;

		IAccTransactionHeader GetInvoiceForTest(IGlbBranch branch, ZString invoiceNumber, bool shouldSave = true)
		{
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = invoiceNumber;
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = branch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = invoiceNumber;
			newInvoice.AH_InvoiceTerm = "PIA";

			if (shouldSave)
			{
				Factory.Save();
			}

			return newInvoice;
		}

		public class CreditControlledBizo : DummyEnterpriseBusinessObject, ICreditControlledDocumentDelivery, ICreditControlledNotificationTextProvider
		{
			public CreditControlledBizo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public OrgHeader[] OrganisationsForCreditChecksForTest = Array.Empty<OrgHeader>();

			OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
			{
				get { return OrganisationsForCreditChecksForTest; }
			}

			public bool IsDPSFreightMovementRestrictedForTest;

			bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
			{
				get { return IsDPSFreightMovementRestrictedForTest; }
			}

			public bool IsAviationSecurityFreightMovementRestrictedTest;

			bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => IsAviationSecurityFreightMovementRestrictedTest;

			public ScreeningParty[] GetScreeningParties()
			{
				return DPSPartiesForTest;
			}

			public ScreeningParty[] DPSPartiesForTest = Array.Empty<ScreeningParty>();

			void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
			{
				if (getDocumentLogin != null)
				{
					getDocumentLogin(this, e);
				}
			}

			event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
			{
				add { getDocumentLogin += value; }
				remove { getDocumentLogin -= value; }
			}
			event EventHandler<SecurityLoginEventArgs> getDocumentLogin;

			CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback
			{
				get
				{
					return null;
				}
				set
				{
				}
			}

			string[] IRelatedJobNumber.JobNumber
			{
				get { return new string[] { "123" }; }
			}

			string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
			{
				get { return "Consignee, Consignor or Local Client for Billing"; }
			}

			#region Test helper methods

			public static void SetupCreditControllerOverrideThreshold(ZString authorisationRequirement)
			{
				var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
				collection.Add(new AmountOrPercentageBasedThreeLevelAuthorisationRequirement() { Amount = 0, Percentage = 0, Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AuthorisationRequirement = authorisationRequirement });
				AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			}

			public static void ClearCreditControllerOverrideThreshold()
			{
				var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
				AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			}

			public static OrgHeader CreateOrganization(BusinessObjectFactory factory, string orgCode = null)
			{
				var newFactory = new BusinessObjectFactory();
				var organisation = newFactory.NewWithValidTestData<OrgHeader>();
				organisation.OH_IsDebtor = true;
				if (orgCode != null)
				{
					organisation.OH_Code = orgCode;
				}

				newFactory.Save();

				return factory.Load<OrgHeader>(organisation.PK);
			}

			public static void SetOrganizationCreditOnHold(OrgHeader organisation, ZBool isCreditOnHold)
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.Load<OrgHeader>(organisation.PK).CompanyData.OB_AROnCreditHold = isCreditOnHold;

				newFactory.Save();

				organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			}

			public static void SetOrganizationCredit(OrgHeader organisation, ZDecimal creditAmount, decimal creditLimit = -1)
			{
				var newFactory = new BusinessObjectFactory();
				if (creditLimit != -1)
				{
					newFactory.Load<OrgHeader>(organisation.PK).CompanyData.OB_ARCreditLimit = creditLimit;
				}

				var transaction = newFactory.New<AccTransactionHeader>();
				transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
				transaction.AH_TransactionType = TransactionTypes.Payment;
				transaction.AH_InvoiceDate = ZDateTime.Today;
				transaction.AH_InvoiceAmount = transaction.AH_OutstandingAmount = creditAmount;
				transaction.AH_GC = GlbCompany.CurrentCompany.PK;
				transaction.AH_GB = GlbBranch.CurrentBranch.PK;
				transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
				transaction.AH_OH = organisation.PK;
				transaction.AH_TransactionNum = "VALUEFORTEST";

				newFactory.Save();

				organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			}

			#endregion

			#region ICreditControlledNotificationTextProvider Members

			public bool UseICreditControlledNotificationTextProvider;

			MultilingualString ICreditControlledNotificationTextProvider.NotificationHeaderText
			{
				get
				{
					if (UseICreditControlledNotificationTextProvider)
					{
						return (NoResString)"Test Start Words";
					}
					return null;
				}
			}

			MultilingualString ICreditControlledNotificationTextProvider.NotificationConfirmationText
			{
				get
				{
					if (UseICreditControlledNotificationTextProvider)
					{
						return (NoResString)"Test Contact Message";
					}
					return null;
				}
			}

			#endregion
		}

		#endregion
	}
}

