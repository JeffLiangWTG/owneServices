using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration.CreditControl;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocumentDeliveryCreditControlHelperTest : TestCaseWithFactory
	{
		public void TestHasOutstandingCashAdvanceRequests()
		{
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			var businessObject = Factory.New<CreditControlledBizo>();

			AssertHasUnpaidCashAdvanceRequests(false);
			AssertHasUnpaidCashAdvanceRequests(true);

			void AssertHasUnpaidCashAdvanceRequests(bool hasOutstandingCashAdvanceRequests)
			{
				var expectedValues = new HelperValues();

				if (hasOutstandingCashAdvanceRequests)
				{
					var expectedMessageToShowWhenNotAllowed =
		@"Delivery of this document is restricted because:
       The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.";

					var expectedLoginPromptMessage = $@"{expectedMessageToShowWhenNotAllowed}

Do you wish to override it and deliver this document?";

					expectedValues.argsIsAccountingRestricted = true;
					expectedValues.GetBreachReasonsForOrganisation = "Unpaid Advance Payment Request";
					expectedValues.GetEventReferences.Add("Unpaid Advance Payment Request");
					expectedValues.IsRestricted = true;
					expectedValues.MessageToShowWhenNotAllowed = expectedMessageToShowWhenNotAllowed;
					expectedValues.LoginPromptMessage = expectedLoginPromptMessage;
				}
				else
				{
					expectedValues.argsIsAccountingRestricted = false;
					expectedValues.GetBreachReasonsForOrganisation = "";
					expectedValues.GetEventReferences.Clear();
					expectedValues.IsRestricted = false;
					expectedValues.MessageToShowWhenNotAllowed = "";
					expectedValues.LoginPromptMessage = "";
				}

				var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, hasOutstandingCashAdvanceRequests, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK);
				AssertHelper(expectedValues, helper);
			}
		}

		#region Test Order

		[TestDate]
		public void TestOrderOfDocumentRestrictionAndExtraRestrictions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var businessObject = Factory.New<CreditControlledBizo>();

				var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
				authorizingUser.GS_LoginName = "ABC";
				authorizingUser.GS_FullName = "Adam Brian Carlson";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "TMPORG";
				org.OH_IsDebtor = true;

				Factory.Save();

				AssertEquals("Precondition", false, org.CompanyData.OB_AROnCreditHold);

				OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, 1);

				var helper = new DocumentDeliveryCreditControlHelper(new DocumentDeliveryExtraRestrictions(true, true, typeof(IForwardingConsol)), false, false, "document", "org", null, new[] { org }, businessObject, MenutItem.PK);

				var expectedValues = new HelperValues();

				expectedValues.GetBreachReasonsForOrganisation = "Credit On Hold";
				expectedValues.GetEventReferences.Add("Credit Hold Status Overridden");
				expectedValues.GetEventReferences.Add("Denied Party Hold Status Overridden|Test Document Name");
				expectedValues.GetEventReferences.Add("Aviation Security Hold Status Overridden");
				expectedValues.argsSecurityCheckPoints.Add(Env.Security.OnCreditHoldControllerFirstLevel);
				expectedValues.argsSecurityCheckPoints.Add(Env.Security.OnCreditHoldControllerSecondLevel);
				expectedValues.argsSecurityCheckPoints.Add(Env.Security.OnCreditHoldControllerThirdLevel);
				expectedValues.argsSecurityCheckPoints.Add(Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr);
				expectedValues.argsSecurityCheckPoints.Add(Env.Security.OverrideRestrictionOfAviationSecurityFreightMovementRestricted);
				expectedValues.argsAuthorizationLevel.AddRange(new[] { 1, 2, 3 });
				expectedValues.IsRestricted = true;
				expectedValues.argsIsAccountingRestricted = true;
				expectedValues.argsIsDPSFreightMovementRestricted = true;
				expectedValues.argsIsAviationSecurityFreightMovementRestricted = true;

				expectedValues.MessageToShowWhenNotAllowed = @"Delivery of this document is restricted because:
       1 ) The org
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.
AND
       2 ) Screening Status is not Clear.
AND
       3 ) Only users with valid BKG and DTA certificate types saved in their Staff Profile may issue documents for air export jobs from the EU, Iceland, Switzerland, Norway or Liechtenstein.";

				expectedValues.LoginPromptMessage = $@"{expectedValues.MessageToShowWhenNotAllowed}

Do you wish to override it and deliver this document?";

				AssertHelper(expectedValues, helper);
			}
		}

		[TestDate]
		public void TestOrderOfExternalRestrictionAndExtraRestrictions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var businessObject = Factory.New<CreditControlledBizo>();

				var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
				authorizingUser.GS_LoginName = "ABC";
				authorizingUser.GS_FullName = "Adam Brian Carlson";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "TMPORG";
				org.OH_IsDebtor = true;

				Factory.Save();

				AssertEquals("Precondition", false, org.CompanyData.OB_AROnCreditHold);

				AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code);

				var helper = new DocumentDeliveryCreditControlHelper(new DocumentDeliveryExtraRestrictions(true, true, typeof(IForwardingConsol)), false, false, "document", "org", null, new[] { org }, businessObject, MenutItem.PK);

				var expectedValues = new HelperValues();

				expectedValues.GetBreachReasonsForOrganisation = "External system request is required to evaluate credit status";
				expectedValues.GetEventReferences.Add("External System Credit Status Overridden");
				expectedValues.GetEventReferences.Add("Denied Party Hold Status Overridden|Test Document Name");
				expectedValues.GetEventReferences.Add("Aviation Security Hold Status Overridden");
				expectedValues.argsSecurityCheckPoints.Add(Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr);
				expectedValues.argsSecurityCheckPoints.Add(Env.Security.OverrideRestrictionOfAviationSecurityFreightMovementRestricted);
				expectedValues.argsAuthorizationLevel.AddRange(new[] { 3 });
				expectedValues.IsRestricted = true;
				expectedValues.argsIsAccountingRestricted = true;
				expectedValues.argsIsExternalAccountingSystemUsed = true;
				expectedValues.argsIsDPSFreightMovementRestricted = true;
				expectedValues.argsIsAviationSecurityFreightMovementRestricted = true;

				expectedValues.MessageToShowWhenNotAllowed = @"Delivery of this document is restricted because:
       1 ) External system request is required to evaluate credit status.
AND
       2 ) Screening Status is not Clear.
AND
       3 ) Only users with valid BKG and DTA certificate types saved in their Staff Profile may issue documents for air export jobs from the EU, Iceland, Switzerland, Norway or Liechtenstein.";

				expectedValues.LoginPromptMessage = $@"{expectedValues.MessageToShowWhenNotAllowed}

Do you wish to override it and deliver this document?";

				AssertHelper(expectedValues, helper);
			}
		}

		[TestDate]
		public void TestOrderOfRestrictionsWithoutComplianceAssessment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
				{
					var businessObject = Factory.New<CreditControlledBizo>();

					var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
					authorizingUser.GS_LoginName = "ABC";
					authorizingUser.GS_FullName = "Adam Brian Carlson";

					var org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_Code = "TMPORG";
					org.OH_IsDebtor = true;

					Factory.Save();

					AssertEquals("Precondition", false, org.CompanyData.OB_AROnCreditHold);

					OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, 1);

					var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(true, true, typeof(IForwardingConsol)), false, false, "document", "org", null, new[] { org }, businessObject, MenutItem.PK);

					var expectedValues = new HelperValues();

					expectedValues.GetBreachReasonsForOrganisation = "Credit On Hold";
					expectedValues.GetEventReferences.Add("Credit Hold Status Overridden");
					expectedValues.GetEventReferences.Add("Denied Party Hold Status Overridden|Test Document Name");
					expectedValues.GetEventReferences.Add("Aviation Security Hold Status Overridden");
					expectedValues.argsSecurityCheckPoints.Add(Env.Security.OnCreditHoldControllerFirstLevel);
					expectedValues.argsSecurityCheckPoints.Add(Env.Security.OnCreditHoldControllerSecondLevel);
					expectedValues.argsSecurityCheckPoints.Add(Env.Security.OnCreditHoldControllerThirdLevel);
					expectedValues.argsSecurityCheckPoints.Add(Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr);
					expectedValues.argsSecurityCheckPoints.Add(Env.Security.OverrideRestrictionOfAviationSecurityFreightMovementRestricted);
					expectedValues.argsAuthorizationLevel.AddRange(new[] { 1, 2, 3 });
					expectedValues.IsRestricted = true;
					expectedValues.argsIsAccountingRestricted = true;
					expectedValues.argsIsDPSFreightMovementRestricted = true;
					expectedValues.argsIsAviationSecurityFreightMovementRestricted = true;

					expectedValues.MessageToShowWhenNotAllowed = @"Delivery of this document is restricted because:
       1 ) The org
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.
AND
       2 ) Screening Status is not Clear.
AND
       3 ) Only users with valid BKG and DTA certificate types saved in their Staff Profile may issue documents for air export jobs from the EU, Iceland, Switzerland, Norway or Liechtenstein.";

					expectedValues.LoginPromptMessage = $@"{expectedValues.MessageToShowWhenNotAllowed}

Do you wish to override it and deliver this document?";

					AssertHelper(expectedValues, helper);
				}
		}

		#endregion Test Order

		public void TestIsRestricted()
		{
			var businessObject = Factory.New<CreditControlledBizo>();
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			var emptyExtraRestrictions = GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol));

			AssertEquals("Not Restricted", false, new DocumentDeliveryCreditControlHelper(emptyExtraRestrictions, false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());

			CombineAssertions("Extra Restrictions", () =>
			{
				Assert(new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(true, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());
				Assert(new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, true, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());
			});

			using (new DisposableAction(
				() => CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true),
				() => CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, false)))
			{
				Assert(new DocumentDeliveryCreditControlHelper(emptyExtraRestrictions, false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());
			}

			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			CreditControlledBizo.SetOrganizationCredit(orgHeader, 2, 1);
			Assert(new DocumentDeliveryCreditControlHelper(emptyExtraRestrictions, false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());

			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired);
			Assert(new DocumentDeliveryCreditControlHelper(emptyExtraRestrictions, true, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());
		}

		#region Test Extra Restrictions

		public void TestIsDPSFreightMovementRestricted()
		{
			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IForwardingConsol)),
				"document",
				null,
				MenutItem.PK), typeof(IForwardingConsol), "Denied Party Hold Status Overridden|");
		}

		public void TestIsDPSFreightMovementRestricted_WhenEnableComplianceRiskOnWarehouseOrderAndReceive()
		{
			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IWhsOrder)),
				"document",
				null,
				MenutItem.PK), typeof(IWhsOrder), "Denied Party Hold Status Overridden|");

			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IWhsReceive)),
				"document",
				null,
				MenutItem.PK), typeof(IWhsReceive), "Denied Party Hold Status Overridden|");
		}

		public void TestIsDPSFreightMovementRestricted_WhenDisableComplianceRisk()
		{
			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IBaseJobDeclaration)),
				"document",
				null,
				MenutItem.PK), typeof(IBaseJobDeclaration), "Denied Party Hold Status Overridden|");

			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IForwardingConsol)),
				"document",
				null,
				MenutItem.PK), typeof(IForwardingConsol), "Denied Party Hold Status Overridden|");

			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IForwardingShipment)),
				"document",
				null,
				MenutItem.PK), typeof(IForwardingShipment), "Denied Party Hold Status Overridden|");

			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IQuotedBooking)),
				"document",
				null,
				MenutItem.PK), typeof(IQuotedBooking), "Denied Party Hold Status Overridden|");
		}

		public void TestIsDPSFreightMovementRestricted_WhenEnableComplianceRisk()
		{
			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (ObjectFactory.Substitute(featureControlMock.Object))
				{
					AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
						GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IBaseJobDeclaration)),
						"document",
						null,
						MenutItem.PK), typeof(IBaseJobDeclaration), "|MST=Compliance Risk|Document Hold Status Overridden|");
				}

				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
				{
					AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
						GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IForwardingConsol)),
						"document",
						null,
						MenutItem.PK), typeof(IForwardingConsol), "|MST=Compliance Risk|Document Hold Status Overridden|");

					AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
						GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IForwardingShipment)),
						"document",
						null,
						MenutItem.PK), typeof(IForwardingShipment), "|MST=Compliance Risk|Document Hold Status Overridden|");

					AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
						GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IQuotedBooking)),
						"document",
						null,
						MenutItem.PK), typeof(IQuotedBooking), "|MST=Compliance Risk|Document Hold Status Overridden|");
				}
			}
		}

		public void TestRestrictedMessageOnBaseJobDeclaration()
		{
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_MessageType = "EXP";

			var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(featureControlMock.Object))
			{
				var helper = new DocumentDeliveryCreditControlHelper(
					GetDocumentDeliveryExtraRestrictions(
						isDPSFreightMovementRestricted: true,
						false,
						typeof(IBaseJobDeclaration)),
					"document",
					(BusinessObject)declaration,
					MenutItem.PK);

				var restrictedMessage = helper.GetDpsOrComplianceFreightMovementRestrictedMessage();
				AssertEquals("The Job Compliance Status is not Clear", "4C841FDB-C41E-4787-99B0-F1A2887ED3FA", restrictedMessage.ResourceKey);
			}
		}

		public void TestRestrictedMessageOnTransportBookingWhenParentJobIsCustomDeclaration()
		{
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var bookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = declaration.PK;
			bookingConsolidation.KB_ParentTableCode = "JE";
			bookingConsolidation.KB_JobDirection = "DLV";

			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = bookingConsolidation.PK;

			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var helper = new DocumentDeliveryCreditControlHelper(
					GetDocumentDeliveryExtraRestrictions(
						isDPSFreightMovementRestricted: true,
						false,
						typeof(IDtbBooking)),
					"document",
					(BusinessObject)booking,
					MenutItem.PK);

				var restrictedMessage = helper.GetDpsOrComplianceFreightMovementRestrictedMessage();
				AssertEquals("Screening Status is not Clear", "79596428-06b8-4600-a339-5e6f82bffbc8", restrictedMessage.ResourceKey);
			}
		}

		public void TestRestrictedMessageOnTransportBookingWhenParentJobIsShipment()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			Factory.Save();

			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var helper = new DocumentDeliveryCreditControlHelper(
					GetDocumentDeliveryExtraRestrictions(
						isDPSFreightMovementRestricted: true,
						false,
						typeof(IDtbBooking)),
					"document",
					(BusinessObject)booking,
					MenutItem.PK);

				var restrictedMessage = helper.GetDpsOrComplianceFreightMovementRestrictedMessage();
				AssertEquals("The Job Compliance Status of a related job is not Clear", "A88B0007-C61C-440A-8CCC-853954D91C77", restrictedMessage.ResourceKey);
			}
		}

		public void TestIsAviationSecurityFreightMovementRestricted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, isAviationSecurityFreightMovementRestricted: true, typeof(IForwardingConsol)), "document", null, MenutItem.PK);
				var args = helper.GetSecurityLoginEventArgs();
				AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

				var expectedLoginPromptMessage =
	@"Delivery of this document is restricted because:
       Only users with valid BKG and DTA certificate types saved in their Staff Profile may issue documents for air export jobs from the EU, Iceland, Switzerland, Norway or Liechtenstein.

Do you wish to override it and deliver this document?
";
				AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

				var expectedMessageToShowWhenNotAllowed =
	@"Delivery of this document is restricted because:
       Only users with valid BKG and DTA certificate types saved in their Staff Profile may issue documents for air export jobs from the EU, Iceland, Switzerland, Norway or Liechtenstein.";
				AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

				AssertEquals("securityCheckPoints", Env.Security.OverrideRestrictionOfAviationSecurityFreightMovementRestricted, args.SecurityCheckPoints.Single().Invoke(Env.Security));

				AssertEquals("eventReferences", "Aviation Security Hold Status Overridden", helper.GetEventReferences().Single());
				Assert("Approval Button should be hidden", args.HideApprovalRequestButton);
			}
		}

		void AssertFreightMovementResctrictionsSecurityCheckPoints(Type bizoType, SecurityLoginEventArgsForDocumentApproval args)
		{
			if (typeof(IForwardingConsol) == bizoType && ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				AssertEquals("securityCheckPoints", Env.Security.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions, args.SecurityCheckPoints.Single().Invoke(Env.Security));
			}
			else if (typeof(IForwardingShipment) == bizoType && ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				AssertEquals("securityCheckPoints", Env.Security.ShipmentsComplianceAllowOverrideFreightMovementRestrictions, args.SecurityCheckPoints.Single().Invoke(Env.Security));
			}
			else if (typeof(IQuotedBooking) == bizoType && ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				AssertEquals("securityCheckPoints", Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions, args.SecurityCheckPoints.Single().Invoke(Env.Security));
			}
			else if (typeof(IBaseJobDeclaration) == bizoType && ComplianceRiskHelper.IsCustomsEnabledComplianceWise)
			{
				AssertEquals("securityCheckPoints", Env.Security.CustomsComplianceAllowOverrideFreightMovementRestrictions, args.SecurityCheckPoints.Single().Invoke(Env.Security));
			}
			else
			{
				AssertEquals("securityCheckPoints", Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr, args.SecurityCheckPoints.Single().Invoke(Env.Security));
			}
		}

		void AssertIsDPSFreightMovementRestricted(DocumentDeliveryCreditControlHelper helper, Type bizoType, string expectedEventReference)
		{
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage =
$@"Delivery of this document is restricted because:
       {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}

Do you wish to override it and deliver this document?
";
			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed =
$@"Delivery of this document is restricted because:
       {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertFreightMovementResctrictionsSecurityCheckPoints(bizoType, args);

			AssertEquals("eventReference", expectedEventReference, helper.GetEventReferences().Single());
		}

		#endregion Test Extra Restrictions

		#region Test Extra Restriction with Credit Enabled

		public void TestIsDPSFreightMovementRestricted_CreditCheckEnabled()
		{
			AssertIsDPSFreightMovementRestricted(new DocumentDeliveryCreditControlHelper(
				GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted: true, false, typeof(IForwardingConsol)),
				false,
				false,
				"document",
				"organisation",
				null,
				Array.Empty<OrgHeader>(),
				null,
				MenutItem.PK), typeof(IForwardingConsol), "Denied Party Hold Status Overridden|");
		}

		#endregion Test Extra Restriction with Credit Enabled

		#region Test IsCreditOnHold

		[TestDate]
		public void TestIsCreditOnHoldWhenNotOnlyCreditOnHoldOrgsPassedForCreditChecks()
		{
			var nonDebtorOrg = Factory.New<OrgHeader>();
			nonDebtorOrg.OH_Code = "TMPORG";
			var nonCreditOnHoldOrg = Factory.New<OrgHeader>();
			nonCreditOnHoldOrg.OH_Code = "DEBTORORG";
			nonCreditOnHoldOrg.OH_IsDebtor = true;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DEBTORONHOLD";
			org.OH_IsDebtor = true;
			int[] expectedAuthorizationRequired = { 2, 3 };

			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			var actualCreditOnHoldStatus = org.CompanyData.OB_AROnCreditHold;
			AssertEquals("Precondition", false, org.CompanyData.OB_AROnCreditHold);

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, expectedAuthorizationRequired[0]);

			actualCreditOnHoldStatus = org.CompanyData.OB_AROnCreditHold;
			AssertEquals(true, actualCreditOnHoldStatus);

			var businessObject = Factory.New<CreditControlledBizo>();
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { nonDebtorOrg, nonCreditOnHoldOrg, org }, businessObject, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			AssertEquals("securityCheckPoints.Length", 2, args.SecurityCheckPoints.Count);

			var actualAuthotizationRequired = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertEquals(2, actualAuthotizationRequired.Count);
			AssertEquals("Level 2 authorization required", expectedAuthorizationRequired[0], actualAuthotizationRequired[0]);
			AssertEquals("Level 3 authorization required", expectedAuthorizationRequired[1], actualAuthotizationRequired[1]);

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 1, eventReferences.Length);
			AssertEquals("eventReferences[0]", "Credit Hold Status Overridden", eventReferences[0]);
		}

		[TestDate]
		public void TestCreditOnHold_AuthorisationLevel_ShouldReturn_HigherCreditOnHoldLevel()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TMPORG";
			org.OH_IsDebtor = true;
			int[] expectedAuthorizationRequired = { 2, 3 };

			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";

			var anotherAuthorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			anotherAuthorizingUser.GS_LoginName = "Tester";
			anotherAuthorizingUser.GS_FullName = "Another Tester Foo";
			Factory.Save();

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, expectedAuthorizationRequired[0]);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.MiscServ, anotherAuthorizingUser, expectedAuthorizationRequired[1], true);

			Assert_Return_HigherCreditOnHold("Level 3 Global authorization required", org, 1, new int[] { expectedAuthorizationRequired[1] });

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.MiscServ, anotherAuthorizingUser, expectedAuthorizationRequired[0], true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, expectedAuthorizationRequired[1]);

			Assert_Return_HigherCreditOnHold("Level 3 Local authorization required", org, 1, new int[] { expectedAuthorizationRequired[1] });

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.MiscServ, anotherAuthorizingUser, expectedAuthorizationRequired[0], true);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, expectedAuthorizationRequired[0]);

			Assert_Return_HigherCreditOnHold("Level 2, 3 authorization required", org, 2, expectedAuthorizationRequired);
		}

		void Assert_Return_HigherCreditOnHold(string expectedComment, OrgHeader org, int expectedAuthorizationLength, int[] expectedAuthorizationRequired)
		{
			var businessObject = Factory.New<CreditControlledBizo>();
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { org }, businessObject, MenutItem.PK);
			var actualAuthotizationRequired = helper.GetSecurityLoginEventArgs().AuthorizationLevel;

			AssertEquals(expectedAuthorizationLength, actualAuthotizationRequired.Count);
			AssertEquals(expectedComment, true, expectedAuthorizationRequired.SequenceEqual(actualAuthotizationRequired));
		}

		[TestDate]
		public void TestIsCreditOnHold()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TMPORG";
			org.OH_IsDebtor = true;
			int[] expectedAuthorizationRequired = { 2, 3 };

			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			var actualCreditOnHoldStatus = org.CompanyData.OB_AROnCreditHold;
			AssertEquals("Precondition", false, org.CompanyData.OB_AROnCreditHold);

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, expectedAuthorizationRequired[0]);

			actualCreditOnHoldStatus = org.CompanyData.OB_AROnCreditHold;
			AssertEquals(true, actualCreditOnHoldStatus);

			var businessObject = Factory.New<CreditControlledBizo>();
			businessObject.UseICreditControlledNotificationTextProvider = true;
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { org }, businessObject, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage =
@"Test Start Words
       The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Test Contact Message
";
			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed =
@"Test Start Words
       The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertEquals("securityCheckPoints.Length", 2, args.SecurityCheckPoints.Count);

			var actualAuthotizationRequired = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertEquals(2, actualAuthotizationRequired.Count);
			AssertEquals("Level 2 authorization required", expectedAuthorizationRequired[0], actualAuthotizationRequired[0]);
			AssertEquals("Level 3 authorization required", expectedAuthorizationRequired[1], actualAuthotizationRequired[1]);

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 1, eventReferences.Length);
			AssertEquals("eventReferences[0]", "Credit Hold Status Overridden", eventReferences[0]);
		}

		public void TestIsCreditOnHold_WhenWebServiceError()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TMPORG";
			org.OH_IsDebtor = true;
			org.CompanyData.OB_AROnCreditHold = true;
			int[] expectedAuthorizationRequired = { 2, 3 };

			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			using (ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForOutstandingBalance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckWebServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ErrorUrl.com"))
			{
				AssertPromptMessage();
				AssertNoExceptionThrown(
					"Make sure WebService Error will not be cached on second call, causing the error message to disappear",
					() => AssertPromptMessage()
				);
			}

			void AssertPromptMessage()
			{
				var businessObject = Factory.New<CreditControlledBizo>();
				var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { org }, businessObject, MenutItem.PK);
				var args = helper.GetSecurityLoginEventArgs();
				AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

				var expectedLoginPromptMessage =
	@"Delivery of this document is restricted because:
       The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this document?
";
				AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

				var expectedMessageToShowWhenNotAllowed =
	@"Delivery of this document is restricted because:
       The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.";
				AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);
			}
		}

		#endregion Test IsCreditOnHold

		#region Test IsAtOrOverCreditLimit

		public void TestIsAtOrOverCreditLimit()
		{
			var businessObject = Factory.New<CreditControlledBizo>();
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);
			CreditControlledBizo.SetOrganizationCredit(orgHeader, 2, 1);
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage =
@"Delivery of this document is restricted because:
       The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this document?
";

			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed =
@"Delivery of this document is restricted because:
       The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertEquals("securityCheckPoints.Length", 1, args.SecurityCheckPoints.Count);

			var actualAuthotizationRequired = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertEquals("Level 3 authorization required", 3, actualAuthotizationRequired[0]);

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 1, eventReferences.Length);
			AssertEquals("eventReferences[0]", "At or Over Credit Limit", eventReferences[0]);
		}

		#endregion Test IsAtOrOverCreditLimit

		#region Test HasUnpaidPIAInvoices

		public void TestHasUnpaidPIAInvoices()
		{
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			var businessObject = Factory.New<CreditControlledBizo>();
			var hasUnpaidPIAInvoices = true;
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), hasUnpaidPIAInvoices, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage =
@"Delivery of this document is restricted because:
       The organisation
	  a) Has unpaid PIA (Payment In Advance) invoices posted on this shipment, OR
	  b) Has no invoices posted on this shipment and has credit terms PIA (Payment In Advance).

Do you wish to override it and deliver this document?
";
			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed =
@"Delivery of this document is restricted because:
       The organisation
	  a) Has unpaid PIA (Payment In Advance) invoices posted on this shipment, OR
	  b) Has no invoices posted on this shipment and has credit terms PIA (Payment In Advance).";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertEquals("securityCheckPoints.Length", 1, args.SecurityCheckPoints.Count);

			var actualAuthotizationRequired = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertEquals("Level 3 authorization required", 3, actualAuthotizationRequired[0]); //for jobs having PIA invoice at the moment fixed level 1 authorization is required

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 1, eventReferences.Length);
			AssertEquals("eventReferences[0]", "Unpaid Payment in Advance Transactions", eventReferences[0]);
		}

		public void TestHasUnpaidPIAInvoicesButApproved()
		{
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			var businessObject = Factory.New<CreditControlledBizo>();
			var hasUnpaidPIAInvoices = true;

			Func<DocumentDeliveryCreditControlHelper> createHelper = () => new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), hasUnpaidPIAInvoices, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK);
			var helper = createHelper();
			AssertEquals("Must be restricted", true, helper.IsRestricted());
			var actualAuthotizationRequired = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertEquals("Must have 1 security-check", 1, actualAuthotizationRequired.Count);

			var approval = Factory.New<ICreditControlledDocumentsApproval>();
			approval.Initialize(businessObject, MenutItem.PK, new int[] { 3 });
			Factory.Save();
			approval.SetStatus(Constants.GenApprovalRequestApprovalStatus.Approved, false);
			Factory.Save();
			helper = createHelper();
			AssertEquals("Must not be restricted", false, helper.IsRestricted());

			actualAuthotizationRequired = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertEquals("Must have 0 security-check", 0, actualAuthotizationRequired.Count);
		}

		#endregion Test HasUnpaidPIAInvoices

		#region Test DocumentRestricted Plus Extra Restriction

		[TestDate]
		public void TestIsCreditOnHoldPlusIsDPSFreightMovementRestricted()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TMPORG";
			org.OH_IsDebtor = true;

			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			var actualCreditOnHoldStatus = org.CompanyData.OB_AROnCreditHold;
			AssertEquals("Precondition", false, org.CompanyData.OB_AROnCreditHold);

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, 2);

			actualCreditOnHoldStatus = org.CompanyData.OB_AROnCreditHold;
			AssertEquals(true, actualCreditOnHoldStatus);

			var businessObject = Factory.New<CreditControlledBizo>();
			var isDPSFreightMovementRestricted = true;
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { org }, businessObject, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage =
$@"Delivery of this document is restricted because:
       1 ) The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.
AND
       2 ) {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}

Do you wish to override it and deliver this document?
";
			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed =
$@"Delivery of this document is restricted because:
       1 ) The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.
AND
       2 ) {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertEquals("securityCheckPoints.Length", 3, args.SecurityCheckPoints.Count);

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 2, eventReferences.Length);
			AssertEquals("eventReferences[0]", "Credit Hold Status Overridden", eventReferences[0]);
			AssertEquals("eventReferences[1]", "Denied Party Hold Status Overridden|Test Document Name", eventReferences[1]);
		}

		public void TestIsAtOrOverCreditLimitPlusIsDPSFreightMovementRestricted()
		{
			var businessObject = Factory.New<CreditControlledBizo>();
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetupCreditControllerOverrideThreshold(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			CreditControlledBizo.SetOrganizationCredit(orgHeader, 2, 1);
			var isDPSFreightMovementRestricted = true;
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage =
$@"Delivery of this document is restricted because:
       1 ) The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.
AND
       2 ) {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}

Do you wish to override it and deliver this document?
";
			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed =
$@"Delivery of this document is restricted because:
       1 ) The organisation
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.
AND
       2 ) {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertEquals("securityCheckPoints.Length", 2, args.SecurityCheckPoints.Count);

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 2, eventReferences.Length);
			AssertEquals("eventReferences[0]", "At or Over Credit Limit", eventReferences[0]);
			AssertEquals("eventReferences[1]", "Denied Party Hold Status Overridden|Test Document Name", eventReferences[1]);
		}

		public void TestHasUnpaidPIAInvoicesPlusIsDPSFreightMovementRestricted()
		{
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			var businessObject = Factory.New<CreditControlledBizo>();
			var isDPSFreightMovementRestricted = true;
			var hasUnpaidPIAInvoices = true;
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted, false, typeof(IForwardingConsol)), hasUnpaidPIAInvoices, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage =
$@"Delivery of this document is restricted because:
       1 ) The organisation
	  a) Has unpaid PIA (Payment In Advance) invoices posted on this shipment, OR
	  b) Has no invoices posted on this shipment and has credit terms PIA (Payment In Advance).
AND
       2 ) {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}

Do you wish to override it and deliver this document?
";
			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, (string)args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed =
$@"Delivery of this document is restricted because:
       1 ) The organisation
	  a) Has unpaid PIA (Payment In Advance) invoices posted on this shipment, OR
	  b) Has no invoices posted on this shipment and has credit terms PIA (Payment In Advance).
AND
       2 ) {helper.GetDpsOrComplianceFreightMovementRestrictedMessage()}";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertEquals("securityCheckPoints.Length", 2, args.SecurityCheckPoints.Count);

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 2, eventReferences.Length);
			AssertEquals("eventReferences[0]", "Unpaid Payment in Advance Transactions", eventReferences[0]);
			AssertEquals("eventReferences[1]", "Denied Party Hold Status Overridden|Test Document Name", eventReferences[1]);
		}

		#endregion Test DocumentRestricted Plus Extra Restriction

		public void TestNotRestricted()
		{
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, Array.Empty<OrgHeader>(), null, MenutItem.PK);
			var args = helper.GetSecurityLoginEventArgs();
			AssertEquals("args.GetType()", typeof(SecurityLoginEventArgsForDocumentApproval), args.GetType());

			var expectedLoginPromptMessage = @"";
			AssertEquals("loginPromptMessage", expectedLoginPromptMessage, args.LoginPromptMessage);

			var expectedMessageToShowWhenNotAllowed = @"";
			AssertEquals("MessageToShowWhenNotAllowed", expectedMessageToShowWhenNotAllowed, args.Message);

			AssertEquals("securityCheckPoints.Length", 0, args.SecurityCheckPoints.Count);

			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 0, eventReferences.Length);
		}

		public void TestRestrictionLiftedOnApproval()
		{
			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuPath = "menu/path2";
			menuItem2.SU_MenuName = "name";
			Factory.Save();

			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);

			var businessObject = Factory.New<CreditControlledBizo>();
			AssertEquals("no request so restricted", true, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());

			var approval = Factory.New<ICreditControlledDocumentsApproval>();
			approval.Initialize(businessObject, MenutItem.PK, new[] { 3 });
			Factory.Save();

			AssertEquals("request not actioned, so restricted", true, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());

			approval.SetStatus(Constants.GenApprovalRequestApprovalStatus.Approved, false);
			Factory.Save();

			AssertEquals("request appoved actioned, so not restricted", false, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());
			AssertEquals("request appoved actioned but for another doc, so restricted", true, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, menuItem2.PK).IsRestricted());

			approval.SetStatus(Constants.GenApprovalRequestApprovalStatus.Approved, true);
			Factory.Save();

			AssertEquals("request appoved actioned, so not restricted", false, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());
			AssertEquals("request appoved actioned for all docs, so not restricted", false, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, menuItem2.PK).IsRestricted());

			approval.SetStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled, false);
			Factory.Save();

			AssertEquals("request cancelled, so restricted", true, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());

			approval.SetStatus(Constants.GenApprovalRequestApprovalStatus.Rejected, false);
			Factory.Save();

			AssertEquals("request rejected, so restricted", true, new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { orgHeader }, businessObject, MenutItem.PK).IsRestricted());
		}

		public void TestGetSecurityLoginEventArgsWithDefaultRequestReason()
		{
			var testGuid = ZGuid.NewZGuid();
			var businessObject = Factory.New<CreditControlledBizo>();
			var testHelper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, Array.Empty<OrgHeader>(), businessObject, testGuid);
			var tester = testHelper.GetSecurityLoginEventArgs();
			AssertEquals(ZString.Empty, tester.DefaultApprovalRequestReason);
			tester = testHelper.GetSecurityLoginEventArgs("");
			AssertEquals(ZString.Empty, tester.DefaultApprovalRequestReason);
			tester = testHelper.GetSecurityLoginEventArgs(null);
			AssertEquals(ZString.Empty, tester.DefaultApprovalRequestReason);
			tester = testHelper.GetSecurityLoginEventArgs("ABC");
			AssertEquals("ABC", tester.DefaultApprovalRequestReason);
		}

		[TestDate]
		public void TestGetMaxAuthorizationRequiredWhenUseWebServiceForCreditLimit()
		{
			var org = OrgHeader.LoadFromCode(Factory, "AALSHI");
			org.OH_IsDebtor = true;

			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			Assert("Precondition: credit on hold is not set initially", !org.CompanyData.OB_AROnCreditHold);
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(org.CompanyData, authorizingUser, 1);
			Assert("Precondition: credit on hold is set", org.CompanyData.OB_AROnCreditHold);

			var businessObject = Factory.New<CreditControlledBizo>();
			ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { org }, businessObject, MenutItem.PK);
			var authorizationLevel = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertArrayEqualsByElements(nameof(authorizationLevel), new int[] { 1, 2, 3 }, authorizationLevel.ToArray());

			ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://syd-wdpa-1/AccountingWebService/CreditLimitService.asmx");
			helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(false, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { org }, businessObject, MenutItem.PK);
			authorizationLevel = helper.GetSecurityLoginEventArgs().AuthorizationLevel;
			AssertArrayEqualsByElements(nameof(authorizationLevel) + " - web service ", new int[] { 3 }, authorizationLevel.ToArray());
		}

		public void TestDocumentDeliveryCreditControlHelperWithNullOrg()
		{
			var businessObject = Factory.New<CreditControlledBizo>();
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			Assert(new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(true, false, typeof(IForwardingConsol)), false, false, "document", "organisation", null, new OrgHeader[] { null, orgHeader }, businessObject, MenutItem.PK).IsRestricted());
		}

		public void TestGetEventReferencesJobShipmentReferenceWithDocumentName_ForDPS()
		{
			var businessObject = Factory.New<CreditControlledBizo>();
			var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(true, false, typeof(IForwardingConsol)), "document", businessObject, MenutItem.PK);
			var eventReferences = helper.GetEventReferences();
			AssertEquals("eventReferences.Length", 1, eventReferences.Length);
			AssertEquals("eventReferences[0]", "Denied Party Hold Status Overridden|Test Document Name", eventReferences[0]);
		}

		public void TestGetEventReferencesJobShipmentReferenceWithDocumentName_ForCPW()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var businessObject = Factory.New<CreditControlledBizo>();
				var helper = new DocumentDeliveryCreditControlHelper(GetDocumentDeliveryExtraRestrictions(true, false, typeof(IForwardingConsol)), "document", businessObject, MenutItem.PK);
				var eventReferences = helper.GetEventReferences();
				AssertEquals("eventReferences.Length", 1, eventReferences.Length);
				AssertEquals("eventReferences[0]", "|MST=Compliance Risk|Document Hold Status Overridden|Test Document Name", eventReferences[0]);
			}
		}

		#region Implementation

		StmMenuItem MenutItem;

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			MenutItem = Factory.New<StmMenuItem>();
			MenutItem.SU_MenuPath = "menu/path";
			MenutItem.SU_MenuName = "Test Document Name";

			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		static void AssertHelper(HelperValues expectedValues, DocumentDeliveryCreditControlHelper helper)
		{
			CombineAssertions(() =>
			{
				AssertEquals("GetBreachReasonsForOrganisation", expectedValues.GetBreachReasonsForOrganisation, helper.GetBreachReasonsForOrganisation());
				AssertArrayEqualsByElements("GetEventReferences", expectedValues.GetEventReferences.ToArray(), helper.GetEventReferences());
				var args = helper.GetSecurityLoginEventArgs();
				AssertArrayEqualsByElements("args.SecurityCheckPoints", expectedValues.argsSecurityCheckPoints.ToArray(), args.SecurityCheckPoints.Select(x => x(Env.Security)).ToArray());
				AssertArrayEqualsByElements("args.AuthorizationLevel", expectedValues.argsAuthorizationLevel.ToArray(), args.AuthorizationLevel.ToArray());
				AssertEquals("args.IsAviationSecurityFreightMovementRestricted", expectedValues.argsIsAviationSecurityFreightMovementRestricted, args.IsAviationSecurityFreightMovementRestricted);
				AssertEquals("IsMultiStep", expectedValues.argsIsDPSFreightMovementRestricted, args.IsDPSFreightMovementRestricted);
				AssertEquals("IsMultiStep", expectedValues.argsIsAccountingRestricted, args.IsAccountingRestricted);
				AssertEquals("IsMultiStep", expectedValues.argsIsExternalAccountingSystemUsed, args.IsExternalAccountingSystemUsed);
				AssertEquals("IsRestricted", expectedValues.IsRestricted, helper.IsRestricted());
				AssertMultilineASCIIEquals("MessageToShowWhenNotAllowed", expectedValues.MessageToShowWhenNotAllowed, helper.MessageToShowWhenNotAllowed);
				AssertMultilineASCIIEquals("LoginPromptMessage", expectedValues.LoginPromptMessage, helper.LoginPromptMessage);
			});
		}

		class HelperValues
		{
			public ZString GetBreachReasonsForOrganisation;
			public List<string> GetEventReferences = new List<string>();
			public List<SecurityCheckpoint> argsSecurityCheckPoints = new List<SecurityCheckpoint>();
			public List<int> argsAuthorizationLevel = new List<int>();
			public bool argsIsDPSFreightMovementRestricted;
			public bool argsIsAviationSecurityFreightMovementRestricted;
			public bool argsIsAccountingRestricted;
			public bool argsIsExternalAccountingSystemUsed;
			public bool IsRestricted;
			public ZString LoginPromptMessage;
			public ZString MessageToShowWhenNotAllowed;
		}

		static DocumentDeliveryExtraRestrictions GetDocumentDeliveryExtraRestrictions(bool isDPSFreightMovementRestricted, bool isAviationSecurityFreightMovementRestricted, Type bizoType)
		{
			return new DocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted, isAviationSecurityFreightMovementRestricted, bizoType);
		}

		#endregion Implementation
	}
}
