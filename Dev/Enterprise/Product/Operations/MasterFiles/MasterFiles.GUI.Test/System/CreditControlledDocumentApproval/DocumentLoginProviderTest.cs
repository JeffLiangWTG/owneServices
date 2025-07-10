using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class DocumentLoginProviderTest : TestCaseWithFactory
	{
		public void TestHideApprovalRequest_Hide()
		{
			TestHideApprovalRequest(true);
		}

		public void TestHideApprovalRequest_Show()
		{
			TestHideApprovalRequest(false);
		}

		void TestHideApprovalRequest(bool hideApproval)
		{
			SecurityLoginEventArgs loginEventArgsHideApproval = new SecurityLoginEventArgs((NoResString)"Doc Login Test Message.", (NoResString)"Doc Error Message.", (s) => s.ReceivablesOnCreditHoldController)
			{
				HideApprovalRequestButton = hideApproval
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
			{
				var loginForm = (DocumentLoginForm)form;

				loginForm.Shown += delegate
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					AssertEquals(hideApproval, ((SecurityLogin)loginForm.BusinessEntity).HideApprovalRequestButton);
					loginForm.CancelPrintButton.PerformClick();

					ZFormModaliser.ResultToReturnFromShowDialog = loginForm.DialogResult; // ZFormModaliser overrides our button click dialog results
				};
			});
			TestObject.ShowDocumentLoginForDocuments(loginEventArgsHideApproval);
		}

		#region ShowDocumentLoginForDocuments

		public void TestShowDocumentLoginForDocuments()
		{
			SecurityLoginEventArgs loginEventArgs = new SecurityLoginEventArgs((NoResString)"Doc Login Test Message.", (NoResString)"Doc Error Message.", (s) => s.ReceivablesOnCreditHoldController);
			CheckShowDocumentLoginForDocuments(loginEventArgs);

			var businessObject = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			loginEventArgs =
				new SecurityLoginEventArgsForDocumentApproval(
					(NoResString)"Doc Login Test Message.",
					(NoResString)"Doc Error Message.",
					(s) => s.ReceivablesOnCreditHoldController,
					null, businessObject, ZGuid.Empty, new List<int>(),
					"", false, false, false, true);
			CheckShowDocumentLoginForDocuments(loginEventArgs);

			loginEventArgs =
				new SecurityLoginEventArgsForDocumentApproval(
					(NoResString)"Doc Login Test Message.",
					(NoResString)"Doc Error Message.",
					(s) => s.ReceivablesOnCreditHoldController,
					MessageBoxCallback, businessObject, ZGuid.Empty,
					new List<int>(), "", false, false, false, true);
			CheckShowDocumentLoginForDocuments(loginEventArgs);
		}

		public void TestShowDocumentLoginMultiStep()
		{
			SecurityLoginEventArgs loginEventArgs =
				new SecurityLoginEventArgsForDocumentApproval(
					(NoResString)"Doc Login Test Message.",
					(NoResString)"Doc Error Message.",
					(s) => s.ReceivablesOnCreditHoldController,
					null, null, ZGuid.Empty, new List<int>(), "", true, false, false, true);

			loginEventArgs.SecurityCheckPoints.Add((s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr);
			CheckShowDocumentLoginForMultiStep(loginEventArgs);
		}

		public void TestShowCreditControlledDocumentsApprovalForm()
		{
			AssertShowCreditControlledDocumentsApprovalForm(false);
		}

		public void TestDoNotShowCreditControlledDocumentsApprovalForm_WhenApprovalRequestApprovedExternally()
		{
			AssertShowCreditControlledDocumentsApprovalForm(true);
		}

		void AssertShowCreditControlledDocumentsApprovalForm(bool isExternalSystem)
		{
			CreditControlledDocumentsApproval approvalRequest = null;

			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path";
			menutItem.SU_MenuName = "name";

			var businessObject = Factory.NewWithValidTestData<CreditControlledBizo>();
			var loginEventArgs =
				new SecurityLoginEventArgsForDocumentApproval(
					(NoResString)"Doc Login Test Message.",
					(NoResString)"Doc Error Message.",
					(s) => s.ReceivablesOnCreditHoldController,
					null, businessObject, menutItem.PK, new List<int>(),
					"DefaultApprovalRequestReasonDescription",
					false, false, isExternalSystem, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; // Ignore is the action wired to requesting approval
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
			{
				var approvalForm = popupForm as CreditControlledDocumentsApprovalForm;
				if (approvalForm != null)
				{
					approvalRequest = ((CreditControlledDocumentsApprovalBulk)approvalForm.BusinessEntity).CreditControlledDocumentsApprovalsAllowedToProcess.Single();
					AssertEquals("DefaultApprovalRequestReasonDescription", approvalRequest.XP_ReasonDescription);
					approvalForm.FireSaveButton();
				}
			});
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			if (isExternalSystem)
			{
				AssertNull("Credit controlled doc approval form is not shown", ZFormModaliser.LastFormShownDialogForTest);
				var factory2 = new BusinessObjectFactory();
				var approvalRequests = factory2.Load<CreditControlledDocumentsApproval>(new ZQuery());
				AssertEquals("1 approval request should be created", 1, approvalRequests.Length);
				approvalRequest = approvalRequests[0];
			}
			else
			{
				AssertEquals("Credit controlled doc approval form is shown", typeof(CreditControlledDocumentsApprovalForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; // Ignore is the action wired to requesting approval
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
			{
				var approvalForm = popupForm as CreditControlledDocumentsApprovalForm;
				if (approvalForm != null)
				{
					Fail("If a request is already made the approval form should not be shown.");
				}
			});
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			var expectedMessage = isExternalSystem ? @"A request has already been made to print this document for this job.
Please repeat the same operation in a short while to check if the request is approved." :
"A request has already been made to print this document for this job.";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			approvalRequest.SetStatus(Constants.GenApprovalRequestApprovalStatus.Rejected, false);
			approvalRequest.RejectionReason = "Debtor is bankrupt";
			approvalRequest.Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; // Ignore is the action wired to requesting approval
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);

			expectedMessage = isExternalSystem ?
				@"Credit Control Approval Request created. Reference No: '00001001'
The operation you want to perform requires this request to be approved before you can proceed.
Please repeat the same operation in a short while to check if the request is approved." :
				"A delivery request for this document has previously been rejected.\r\nReason for rejection: 'Debtor is bankrupt'\r\nDo you want to re-submit the request?";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			if (isExternalSystem)
			{
				AssertNull("Credit controlled doc approval form is not shown", ZFormModaliser.LastFormShownDialogForTest);
				var factory2 = new BusinessObjectFactory();
				var approvalRequests = factory2.Load<CreditControlledDocumentsApproval>(new ZQuery());
				AssertEquals("2 approval request should be created", 2, approvalRequests.Length);
			}
			else
			{
				AssertEquals("Credit controlled doc approval form is shown", typeof(CreditControlledDocumentsApprovalForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestDoNotShowLoginFormWhenInDbTransaction()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			Factory.Save();

			var dbConnection = ((CargoWise.Data.IDbConnected)Factory).Connection;
			try
			{
				dbConnection.BeginTransaction();

				var loginEventArgs =
					new SecurityLoginEventArgsForDocumentApproval(
						(NoResString)"Doc Login Test Message.",
						(NoResString)"Doc Error Message.",
						(s) => s.ReceivablesOnCreditHoldController,
						null, shipment, ZGuid.Empty,
						new List<int>(), string.Empty,
						false, false, false, true);
				TestObject.ShowDocumentLoginForDocuments(loginEventArgs);

				AssertEquals("Should not pop up message form", null, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(false, loginEventArgs.IsAllowedToProceed);
				AssertEquals("Cannot perform document login during data save process. Automatically rejected the login request.", loginEventArgs.Message);
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}
		}

		public void TestErrorMessageIsShownInsteadOfCCDApprovalFormWhenDPSSatusIsNotClear()
		{
			AssertCreditControlDocumentApprovalMessagesWhenDPSSatusIsNotClear(false, false);
		}

		public void TestErrorMessageIsShownInsteadOfCCDApprovalFormWhenDPSSatusIsNotClear_CustomsSubmission()
		{
			AssertCreditControlDocumentApprovalMessagesWhenDPSSatusIsNotClear(false, true);
		}

		public void TestErrorMessageIsShownInsteadOfCreatingApprovalRequestAutomaticallyFormWhenDPSSatusIsNotClear()
		{
			AssertCreditControlDocumentApprovalMessagesWhenDPSSatusIsNotClear(true, false);
		}

		public void TestErrorMessageIsShownInsteadOfCreatingApprovalRequestAutomaticallyWhenDPSSatusIsNotClear_CustomsSubmission()
		{
			AssertCreditControlDocumentApprovalMessagesWhenDPSSatusIsNotClear(true, true);
		}

		void AssertCreditControlDocumentApprovalMessagesWhenDPSSatusIsNotClear(bool isExternalSystem, bool isCustomsSubmission)
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var businessObject = Factory.NewWithValidTestData<CreditControlledBizo>();
			if (isCustomsSubmission)
			{
				OrgHeader party1Org = Factory.NewWithValidTestData<OrgHeader>();
				party1Org.OH_FullName = "Dodgy Company";
				party1Org.OH_Code = "DGC";
				party1Org.OH_ScreeningStatus = "MAT";
				var party1 = new ScreeningParty(businessObject, "Party 1", party1Org);
				OrgHeader party2Org = Factory.NewWithValidTestData<OrgHeader>();
				party2Org.OH_FullName = "Unknown Company";
				party2Org.OH_Code = "UNC";
				party2Org.OH_ScreeningStatus = "UNK";
				var party2 = new ScreeningParty(businessObject, "Party 2", party2Org);
				OrgHeader party3Org = Factory.NewWithValidTestData<OrgHeader>();
				party3Org.OH_FullName = "Clean Company";
				party3Org.OH_Code = "CLC";
				party3Org.OH_ScreeningStatus = "CLR";
				var party3 = new ScreeningParty(businessObject, "Party 3", party3Org);
				businessObject.DPSPartiesForTest = new ScreeningParty[] { party1, party2, party3 };
			}

			SecurityLoginEventArgsForDocumentApproval loginEventArgs;

			#region Single Step

			AssertWhenDPSStatusIsClear();
			AssertWhenDPSStatusIsNotClear(false);

			#endregion

			#region Multi Step

			AssertWhenDPSStatusIsNotClear(true);

			#endregion

			void AssertWhenDPSStatusIsClear()
			{
				Func<SecurityCore, SecurityCheckpoint> getSecurityCheckpoint = (s) => s.ReceivablesOnCreditHoldController;
				loginEventArgs =
					new SecurityLoginEventArgsForDocumentApproval(
						(NoResString)"Doc Login Test Message.",
						(NoResString)"Doc Error Message.",
						isExternalSystem ? null : getSecurityCheckpoint,
						null, businessObject, menuItem.PK, new List<int>(),
						"", false, false, isExternalSystem, true)
					{
						IsCustomsSubmission = isCustomsSubmission
					};
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; // Ignore is the action wired to requesting approval
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
				{
					if (popupForm is CreditControlledDocumentsApprovalForm approvalForm)
					{
						approvalForm.FireSaveButton();
					}
				});
				TestObject.ShowDocumentLoginForDocuments(loginEventArgs);

				AssertDialogsAndRequest(true, false);

				var expectedMessages = new List<string>();
				if (isExternalSystem)
				{
					expectedMessages.Add(@"Credit Control Approval Request created. Reference No: '00001000'
The operation you want to perform requires this request to be approved before you can proceed.
Please repeat the same operation in a short while to check if the request is approved.");
				}

				if (!(isCustomsSubmission && isExternalSystem))
				{
					expectedMessages.Add("Doc Login Test Message.");
				}
				AssertArrayEqualsByElements("PreviousMessages", expectedMessages.ToArray(), UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Where(x => x != null).ToArray());
			}

			void AssertWhenDPSStatusIsNotClear(bool isMultiStep)
			{
				if (isExternalSystem && !isMultiStep) //For single step, it is not possible to have both DPS and external system set to true
				{
					return;
				}

				loginEventArgs =
					new SecurityLoginEventArgsForDocumentApproval(
						(NoResString)"Doc Login Test Message.",
						(NoResString)"Doc Error Message.",
						(s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr,
						null, businessObject, menuItem.PK, new List<int>(),
						"", true, false, isExternalSystem, isMultiStep)
					{
						IsCustomsSubmission = isCustomsSubmission
					};
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				if (isCustomsSubmission)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Denied Party Screening prompt
				}
				if (isMultiStep)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				}

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
				{
					if (popupForm is DocumentLoginForm loginForm)
					{
						if (ZFormModaliser.ResultToReturnFromShowDialog == DialogResult.No)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
						}
						else
						{
							if (isMultiStep)
							{
								((SecurityLogin)loginForm.BusinessEntity).SecurityCheckpoints.Clear();
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
							}
							else
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
							}
						}
					}
				});
				TestObject.ShowDocumentLoginForDocuments(loginEventArgs);

				AssertDialogsAndRequest(false, isMultiStep);

				var expectedMessages = new List<string>();
				if (isExternalSystem)
				{
					expectedMessages.Add("You cannot proceed with printing as according to your configuration only external system can evaluate a credit status for this operation. Do you want to request approval?");
				}
				if (!isCustomsSubmission || isMultiStep)
				{
					expectedMessages.Add(ComplianceRiskApprovalRequestHelper.GetRestrictedDocumentErrorMessageFor_DeniedPartyOrComplianceWise(loginEventArgs));
				}
				if (isCustomsSubmission)
				{
					expectedMessages.Add(ComplianceRiskApprovalRequestHelper.GetRestrictedDocumentWarningMessageFor_DeniedPartyOrComplianceWise(loginEventArgs) + @"

Denied Parties:
Matched : DGC, Dodgy Company (Party 1)
Unknown : UNC, Unknown Company (Party 2)
");
				}
				if (!isCustomsSubmission || isMultiStep)
				{
					expectedMessages.Add("Doc Login Test Message." + (!isMultiStep ? "" : @"Note: The staff user that may override this must have security rights to override the following restrictions: 
       - Override Freight Movement Restrictions
"));
				}
				AssertArrayEqualsByElements("PreviousMessages", expectedMessages.ToArray(), UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Where(x => x != null).ToArray());
			}

			void AssertDialogsAndRequest(bool hasNewRequestBeenCreated, bool isMultiStep)
			{
				if (isExternalSystem && !isMultiStep)
				{
					AssertNull("Credit controlled doc approval form is not shown", ZFormModaliser.LastFormShownDialogForTest);
					var factory2 = new BusinessObjectFactory();
					var approvalRequests = factory2.Load<CreditControlledDocumentsApproval>(new ZQuery());
					AssertEquals("approval request count", 1, approvalRequests.Length);
				}
				else if (isCustomsSubmission && !hasNewRequestBeenCreated && (!loginEventArgs.IsDPSFreightMovementRestricted || !loginEventArgs.IsAccountingRestricted))
				{
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
				}
				else
				{
					AssertEquals("LastFormShownDialogForTest", hasNewRequestBeenCreated ? typeof(CreditControlledDocumentsApprovalForm) : typeof(DocumentLoginForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
				}
			}
		}

		public void TestErrorMessageIsShownInsteadOfCCDApprovalFormWhenCPWSatusIsNotClear()
		{
			AssertCreditControlDocumentApprovalMessagesWhenCPWSatusIsNotClear(false, false);
		}

		public void TestErrorMessageIsShownInsteadOfCCDApprovalFormWhenCPWSatusIsNotClear_CustomsSubmission()
		{
			AssertCreditControlDocumentApprovalMessagesWhenCPWSatusIsNotClear(false, true);
		}

		public void TestErrorMessageIsShownInsteadOfCreatingApprovalRequestAutomaticallyFormWhenCPWSatusIsNotClear()
		{
			AssertCreditControlDocumentApprovalMessagesWhenCPWSatusIsNotClear(true, false);
		}

		public void TestErrorMessageIsShownInsteadOfCreatingApprovalRequestAutomaticallyWhenCPWSatusIsNotClear_CustomsSubmission()
		{
			AssertCreditControlDocumentApprovalMessagesWhenCPWSatusIsNotClear(true, true);
		}

		void AssertCreditControlDocumentApprovalMessagesWhenCPWSatusIsNotClear(bool isExternalSystem, bool isCustomsSubmission)
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var businessObject = Factory.New<IBaseJobDeclaration>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			businessObject.JE_JS = shipment.PK;

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;

			SecurityLoginEventArgsForDocumentApproval loginEventArgs;

			#region Single Step

			AssertWhenDPSStatusIsClear();
			AssertWhenDPSStatusIsNotClear(false);

			#endregion

			#region Multi Step

			AssertWhenDPSStatusIsNotClear(true);

			#endregion

			void AssertWhenDPSStatusIsClear()
			{
				Func<SecurityCore, SecurityCheckpoint> getSecurityCheckpoint = (s) => s.ReceivablesOnCreditHoldController;
				loginEventArgs =
					new SecurityLoginEventArgsForDocumentApproval(
						(NoResString)"Doc Login Test Message.",
						(NoResString)"Doc Error Message.",
						isExternalSystem ? null : getSecurityCheckpoint,
						null, businessObject as BusinessObject, menuItem.PK, new List<int>(),
						"", false, false, isExternalSystem, true)
					{
						IsCustomsSubmission = isCustomsSubmission
					};
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; // Ignore is the action wired to requesting approval
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
				{
					if (popupForm is CreditControlledDocumentsApprovalForm approvalForm)
					{
						approvalForm.FireSaveButton();
					}
				});
				TestObject.ShowDocumentLoginForDocuments(loginEventArgs);

				AssertDialogsAndRequest(true, false);

				var expectedMessages = new List<string>();
				if (isExternalSystem)
				{
					expectedMessages.Add(@"Credit Control Approval Request created. Reference No: '00001000'
The operation you want to perform requires this request to be approved before you can proceed.
Please repeat the same operation in a short while to check if the request is approved.");
				}

				if (!(isCustomsSubmission && isExternalSystem))
				{
					expectedMessages.Add("Doc Login Test Message.");
				}
				AssertArrayEqualsByElements("PreviousMessages", expectedMessages.ToArray(), UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Where(x => x != null).ToArray());
			}

			void AssertWhenDPSStatusIsNotClear(bool isMultiStep)
			{
				if (isExternalSystem && !isMultiStep) //For single step, it is not possible to have both DPS and external system set to true
				{
					return;
				}

				loginEventArgs =
					new SecurityLoginEventArgsForDocumentApproval(
						(NoResString)"Doc Login Test Message.",
						(NoResString)"Doc Error Message.",
						(s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr,
						null, businessObject as BusinessObject, menuItem.PK, new List<int>(),
						"", true, false, isExternalSystem, isMultiStep)
					{
						IsCustomsSubmission = isCustomsSubmission
					};
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				if (isCustomsSubmission)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Denied Party Screening prompt
				}
				if (isMultiStep)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				}

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
				{
					if (popupForm is DocumentLoginForm loginForm)
					{
						if (ZFormModaliser.ResultToReturnFromShowDialog == DialogResult.No)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
						}
						else
						{
							if (isMultiStep)
							{
								((SecurityLogin)loginForm.BusinessEntity).SecurityCheckpoints.Clear();
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
							}
							else
							{
								ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
							}
						}
					}
				});
				TestObject.ShowDocumentLoginForDocuments(loginEventArgs);

				AssertDialogsAndRequest(false, isMultiStep);

				var expectedMessages = new List<string>();
				if (isExternalSystem)
				{
					expectedMessages.Add("You cannot proceed with printing as according to your configuration only external system can evaluate a credit status for this operation. Do you want to request approval?");
				}
				if (!isCustomsSubmission || isMultiStep)
				{
					expectedMessages.Add(ComplianceRiskApprovalRequestHelper.GetRestrictedDocumentErrorMessageFor_DeniedPartyOrComplianceWise(loginEventArgs));
				}
				if (isCustomsSubmission)
				{
					expectedMessages.Add(ComplianceRiskApprovalRequestHelper.GetRestrictedDocumentWarningMessageFor_DeniedPartyOrComplianceWise(loginEventArgs));
				}
				if (!isCustomsSubmission || isMultiStep)
				{
					expectedMessages.Add("Doc Login Test Message." + (!isMultiStep ? "" : @"Note: The staff user that may override this must have security rights to override the following restrictions: 
       - Override Freight Movement Restrictions
"));
				}
				AssertArrayEqualsByElements("PreviousMessages", expectedMessages.ToArray(), UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Where(x => x != null).ToArray());
			}

			void AssertDialogsAndRequest(bool hasNewRequestBeenCreated, bool isMultiStep)
			{
				if (isExternalSystem && !isMultiStep)
				{
					AssertNull("Credit controlled doc approval form is not shown", ZFormModaliser.LastFormShownDialogForTest);
					var factory2 = new BusinessObjectFactory();
					var approvalRequests = factory2.Load<CreditControlledDocumentsApproval>(new ZQuery());
					AssertEquals("approval request count", 1, approvalRequests.Length);
				}
				else if (isCustomsSubmission && !hasNewRequestBeenCreated && (!loginEventArgs.IsDPSFreightMovementRestricted || !loginEventArgs.IsAccountingRestricted))
				{
					AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
				}
				else
				{
					AssertEquals("LastFormShownDialogForTest", hasNewRequestBeenCreated ? typeof(CreditControlledDocumentsApprovalForm) : typeof(DocumentLoginForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
				}
			}
		}

		public void TestAviationSecurityFreightMovementIsRestricted()
		{
			var errorMessageForAviationSecurityRestrictedDocuments =
			@"Only users with valid BKG and DTA certificate types saved in their Staff Profile may issue documents for air export jobs from the EU, Iceland, Switzerland, Norway or Liechtenstein.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AssertAviationSecurityIsRestricted(DialogResult.No, ZString.Empty, ZString.Empty, typeof(DocumentLoginForm), "Document delivery canceled.");

				AssertAviationSecurityIsRestricted(DialogResult.Yes, ZString.Empty, ZString.Empty, typeof(DocumentLoginForm), errorMessageForAviationSecurityRestrictedDocuments);

				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_LoginName = "MM7";
				Factory.Save();
				AssertAviationSecurityIsRestricted(DialogResult.Yes, staff1.GS_LoginName, staff1.StaffPlainTextPassword, typeof(DocumentLoginForm), errorMessageForAviationSecurityRestrictedDocuments);

				var bkgCertificate = staff1.Certificates.AddNew();
				bkgCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;
				AssertAviationSecurityIsRestricted(DialogResult.Yes, staff1.GS_LoginName, staff1.StaffPlainTextPassword, typeof(DocumentLoginForm), errorMessageForAviationSecurityRestrictedDocuments);

				var dtaCertificate = staff1.Certificates.AddNew();
				dtaCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.DTA;
				Factory.Save();
				AssertAviationSecurityIsRestricted(DialogResult.Yes, staff1.GS_LoginName, staff1.StaffPlainTextPassword, typeof(DocumentLoginForm), ZString.Empty);
			}
		}

		void AssertAviationSecurityIsRestricted(DialogResult selectedDialogResult, ZString username, ZString password, Type lastShownFormType, ZString expectedMessage)
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var businessObject = Factory.NewWithValidTestData<CreditControlledBizo>();

			SecurityLoginEventArgsForDocumentApproval loginEventArgs;
			loginEventArgs =
					new SecurityLoginEventArgsForDocumentApproval(
						(NoResString)"Doc Login Test Message.",
						(NoResString)"",
						(s) => s.OverrideRestrictionOfAviationSecurityFreightMovementRestricted,
						null, businessObject, menuItem.PK, new List<int>(),
						"", false, true, false, false)
					{
						IsCustomsSubmission = false
					};
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
			{
				if (popupForm is DocumentLoginForm loginForm)
				{
					ZFormModaliser.ResultToReturnFromShowDialog = selectedDialogResult;
					var userLogin = loginForm.BusinessEntity as SecurityLogin;
					userLogin.Login = username;
					userLogin.Password = password;
				}
				if (popupForm is CreditControlledDocumentsApprovalForm approvalForm)
				{
					approvalForm.FireSaveButton();
				}
			});
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			AssertEquals("LastFormShownDialogForTest", lastShownFormType, ZFormModaliser.LastFormShownDialogForTest?.GetType());
			AssertEquals("Expected Message", expectedMessage, loginEventArgs.MessageToShowWhenNotAllowed.ToString());
		}

		public void TestCreditControlDocumentApprovalMessagesWhenDPSSatusIsNotClearAndExternalSystemUsed()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			var businessObject = Factory.NewWithValidTestData<CreditControlledBizo>();

			var loginEventArgs =
				new SecurityLoginEventArgsForDocumentApproval(
					(NoResString)"Doc Login Test Message.",
					(NoResString)"Doc Error Message.",
					(s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr,
					null, businessObject, menuItem.PK, new List<int>(),
					"", true, false, true, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
			{
				if (popupForm is DocumentLoginForm loginForm)
				{
					if (ZFormModaliser.ResultToReturnFromShowDialog == DialogResult.No)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					}
					else
					{
						((SecurityLogin)loginForm.BusinessEntity).SecurityCheckpoints.Clear();
					}
				}
			});

			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);

			AssertEquals("LastFormShownDialogForTest", typeof(DocumentLoginForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			var factory2 = new BusinessObjectFactory();
			var approvalRequests = factory2.Load<CreditControlledDocumentsApproval>(new ZQuery());
			AssertEquals("approval request count", 1, approvalRequests.Length);

			var expectedMessages = new List<string>();
			expectedMessages.Add(@"Credit Control Approval Request created. Reference No: '00001000'
The operation you want to perform requires this request to be approved before you can proceed.
Please repeat the same operation in a short while to check if the request is approved.");
			expectedMessages.Add("You cannot proceed with printing as according to your configuration only external system can evaluate a credit status for this operation. Do you want to request approval?");
			expectedMessages.Add(@"Doc Login Test Message.Note: The staff user that may override this must have security rights to override the following restrictions: 
       - Override Freight Movement Restrictions
");
			expectedMessages.Add(@"Doc Login Test Message.Note: The staff user that may override this must have security rights to override the following restrictions: 
       - Override Freight Movement Restrictions
");
			AssertArrayEqualsByElements("PreviousMessages", expectedMessages.ToArray(), UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Where(x => x != null).ToArray());
		}

		public void TestShowDocumentLoginMultiStep_ApprovedDPSButRequestForApproval()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "mickey.mouse";
			staff.GS_Code = "MM";
			staff.StaffPlainTextPassword = "bigears";
			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();

			var businessObject = Factory.NewWithValidTestData<CreditControlledBizo>();
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
			businessObject.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			var loginEventArgs =
				new SecurityLoginEventArgsForDocumentApproval(
					(NoResString)"Doc Login Test Message.",
					(NoResString)"Doc Error Message.",
					(s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr,
					null, businessObject, ZGuid.Empty, new List<int>(),
					"", true, false, false, true);
			loginEventArgs.SecurityCheckPoints.Add((s) => s.CreditLimitAdjustmentThirdLevel);

			//DPS status is not clear but user has been granted permission (AllowedSecurity)
			CreditControlledDocumentsApproval approvalRequest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; // Ignore is the action wired to requesting approval

			TestObject.loginBisObject = new SecurityLogin(loginEventArgs.SecurityCheckPoints.Where(x => x(Env.Security) != Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr).Select(x => x).ToList());
			TestObject.loginBisObject.Login = staff.GS_LoginName;
			TestObject.loginBisObject.Password = staff.StaffPlainTextPassword;
			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			Assert("Login should be correct", loginController.ValidateUserLoginAndPassword(TestObject.loginBisObject.Login, TestObject.loginBisObject.Password).LoginValidated);

			TestObject.loginBisObject.CheckIfValidLoginForDocumentPrinting();

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
			{
				var approvalForm = popupForm as CreditControlledDocumentsApprovalForm;
				if (approvalForm != null)
				{
					approvalRequest = ((CreditControlledDocumentsApprovalBulk)approvalForm.BusinessEntity).CreditControlledDocumentsApprovalsAllowedToProcess.Single();
					approvalForm.FireSaveButton();
				}
			});
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			AssertEquals("Credit controlled doc approval form must be shown", typeof(CreditControlledDocumentsApprovalForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			var previousMessages = UnitTestUserNotification.Instance.PreviousMessages;
			AssertEquals("Previous Messages count must be 2", previousMessages.Length, 2);
			Assert("Message #1 must be about third-level-approval", previousMessages[0].Text.Contains("Third Level Approval"));
			AssertEquals("Message #2 must be NULL", null, previousMessages[1].Text);
		}

		public void TestShowDocumentLoginMultiStep_ClearUsernamePassword()
		{
			var loginEventArgs =
				new SecurityLoginEventArgsForDocumentApproval(
					(NoResString)"Doc Login Test Message.",
					(NoResString)"Doc Error Message.",
					(s) => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr,
					null, null, ZGuid.Empty, new List<int>(),
					"", true, false, false, true);
			loginEventArgs.SecurityCheckPoints.Add((s) => s.CreditLimitAdjustmentThirdLevel);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestObject.loginBisObject = new SecurityLogin(loginEventArgs.SecurityCheckPoints);
			TestObject.loginBisObject.LoginInfo.SetValueFromString("login"); // previous-set login
			TestObject.loginBisObject.PasswordInfo.SetValueFromString("password"); // previous-set password
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			AssertEquals("Document Login Form is shown", typeof(DocumentLoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			var documentLoginForm = (DocumentLoginForm)ZFormModaliser.LastFormShownDialogForTest;
			var securityLogin = (SecurityLogin)documentLoginForm.LastDataSourceForTest;
			AssertEquals("Username must be blank", ZString.Empty, securityLogin.LoginInfo.Value);
			AssertEquals("Password  must be blank", ZString.Empty, securityLogin.PasswordInfo.Value);
		}

		void CheckShowDocumentLoginForMultiStep(SecurityLoginEventArgs loginEventArgs)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestObject.loginBisObject = new SecurityLogin(loginEventArgs.SecurityCheckPoints);
			TestObject.loginBisObject.SecurityCheckpoints.Clear();
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Override successful", loginEventArgs.IsAllowedToProceed);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestObject.loginBisObject = new SecurityLogin(loginEventArgs.SecurityCheckPoints.Where(x => x(Env.Security) != Env.Security.ReceivablesOnCreditHoldController).Select(x => x).ToList());
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Override unsuccessful - second user did not have sufficient authorisation", !loginEventArgs.IsAllowedToProceed);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestObject.loginBisObject = new SecurityLogin(loginEventArgs.SecurityCheckPoints.Where(x => x(Env.Security) != Env.Security.ReceivablesOnCreditHoldController).Select(x => x).ToList());
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Override unsuccessful - second login cancelled or invalid", !loginEventArgs.IsAllowedToProceed);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			TestObject.loginBisObject = new SecurityLogin(loginEventArgs.SecurityCheckPoints);
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Override unsuccessful - override cancelled before starting", !loginEventArgs.IsAllowedToProceed);
		}

		void CheckShowDocumentLoginForDocuments(SecurityLoginEventArgs loginEventArgs)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Print has not canceled.", loginEventArgs.IsAllowedToProceed);
			AssertEquals("Document cancelled message.", ZString.Empty, loginEventArgs.Message);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Print has canceled.", !loginEventArgs.IsAllowedToProceed);
			AssertEquals("Document canceled message.", "Document delivery canceled.", loginEventArgs.Message);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Print has canceled.", !loginEventArgs.IsAllowedToProceed);
			AssertEquals("Document canceled message.", "Document delivery canceled.", loginEventArgs.Message);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Print has canceled.", !loginEventArgs.IsAllowedToProceed);
			AssertEquals("Document canceled message.", "Document delivery canceled.", loginEventArgs.Message);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; // Ignore is the action wired to requesting approval
			TestObject.ShowDocumentLoginForDocuments(loginEventArgs);
			Assert("Print has canceled.", !loginEventArgs.IsAllowedToProceed);
			AssertEquals("Document canceled message.", "Document delivery canceled.", loginEventArgs.Message);
		}

		static ZDialogResult MessageBoxCallback(ZString message, ZString caption)
		{
			return Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Exclamation);
		}

		public void TestFormDialogResultMapping()
		{
			AssertEquals(LoginDialogResult.None, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.None));
			AssertEquals(LoginDialogResult.OK, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.OK));
			AssertEquals(LoginDialogResult.Cancel, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.Cancel));
			AssertEquals(LoginDialogResult.Abort, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.Abort));
			AssertEquals(LoginDialogResult.Retry, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.Retry));
			AssertEquals(LoginDialogResult.Ignore, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.Ignore));
			AssertEquals(LoginDialogResult.Yes, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.Yes));
			AssertEquals(LoginDialogResult.No, SecurityLoginProvider.MapDialogResultToLoginDialogResult(DialogResult.No));

			AssertEquals(DialogResult.None, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.None));
			AssertEquals(DialogResult.OK, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.OK));
			AssertEquals(DialogResult.Cancel, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.Cancel));
			AssertEquals(DialogResult.Abort, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.Abort));
			AssertEquals(DialogResult.Retry, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.Retry));
			AssertEquals(DialogResult.Ignore, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.Ignore));
			AssertEquals(DialogResult.Yes, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.Yes));
			AssertEquals(DialogResult.No, SecurityLoginProvider.MapLoginDialogResultToDialogResult(LoginDialogResult.No));

			const int invalidValue = 999;
			AssertEquals(LoginDialogResult.None, SecurityLoginProvider.MapDialogResultToLoginDialogResult((DialogResult)invalidValue));
			AssertEquals(DialogResult.None, SecurityLoginProvider.MapLoginDialogResultToDialogResult((LoginDialogResult)invalidValue));
		}

		#endregion

		#region Implementation

		SecurityLoginProvider TestObject;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			TestObject = new SecurityLoginProvider("Document delivery");
		}

		#endregion
	}
}

