using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class RunSheetSecurityGUIProviderTest : TestCaseWithFactory
	{
		public void TestRegister()
		{
			var provider = RunSheetSecurityProvider.GetProvider(Factory);
			AssertNotNull(provider);
			AssertEquals(typeof(RunSheetSecurityProvider), provider.GetType());
			RunSheetSecurityGUIProvider.Register(Factory);
			var guiProvider = RunSheetSecurityProvider.GetProvider(Factory);
			AssertNotNull(guiProvider);
			AssertEquals(typeof(RunSheetSecurityGUIProvider), guiProvider.GetType());
			AssertNotEquals(provider, guiProvider);
		}

		public void TestUnregister()
		{
			var provider = RunSheetSecurityProvider.GetProvider(Factory);
			AssertNotNull(provider);
			AssertEquals(typeof(RunSheetSecurityProvider), provider.GetType());
			RunSheetSecurityGUIProvider.Register(Factory);
			var guiProvider = RunSheetSecurityProvider.GetProvider(Factory);
			AssertNotNull(guiProvider);
			AssertEquals(typeof(RunSheetSecurityGUIProvider), guiProvider.GetType());
			AssertNotEquals(provider, guiProvider);
			RunSheetSecurityGUIProvider.Unregister(Factory);
			guiProvider = RunSheetSecurityProvider.GetProvider(Factory);
			AssertNotNull(guiProvider);
			AssertEquals(typeof(RunSheetSecurityProvider), guiProvider.GetType());
		}

		public void TestIRunSheetSecurityQueryProvider_IsOKToAttachLeg_DontOverride()
		{
			Env.Security.LocalTransportRunSheetCustomsClearanceControl.IsAllowed = false;
			RunSheetSecurityGUIProvider.Register(Factory);
			var provider = RunSheetSecurityProvider.GetProvider(Factory);
			provider.TryAuthorise(null);
			var leg = Factory.New<CommonCartageLeg>();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			provider.TryAuthorise(leg);
			AssertEquals("No parent, so allow.", true, leg.IsRunSheetAuthorised);
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
			var move = cartage.LooseBookedMoves.AddNew();
			leg = move.CartageLegs.AddNew();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			provider.TryAuthorise(leg);
			AssertEquals("Has Parent, but no Custom Cleared event, so allow.", true, leg.IsRunSheetAuthorised);
			var iShipment = (IWorkflowProvider)shipment;
			var customCleared = iShipment.WorkflowItems.Milestones.AddNew();
			customCleared.TriggerConditions.TriggerEventCode = Events.CustomsCleared.Code;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			leg = move.CartageLegs.AddNew();
			provider.TryAuthorise(leg);
			AssertEquals("Parent has custom cleared event and it is NOT cleared, so do NOT allow.", false, leg.IsRunSheetAuthorised);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirExport;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			provider.TryAuthorise(leg);
			AssertEquals("Parent has custom cleared event and it is NOT cleared, but the cartage is for export, so do allow.", true, leg.IsRunSheetAuthorised);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
			Env.Security.LocalTransportRunSheetCustomsClearanceControl.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			leg = move.CartageLegs.AddNew();
			provider.TryAuthorise(leg);
			AssertEquals("Parent has custom cleared event and it is NOT cleared, but the user has security, so do allow.", true, leg.IsRunSheetAuthorised);
			Env.Security.LocalTransportRunSheetCustomsClearanceControl.IsAllowed = false;
			customCleared.SetMilestoneActualDateForTest(ZDateTime.Now);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			leg = move.CartageLegs.AddNew();
			provider.TryAuthorise(leg);
			AssertEquals("Parent has custom cleared event and it is cleared, so allow.", true, leg.IsRunSheetAuthorised);
		}

		public void TestIRunSheetSecurityQueryProvider_IsOKToAttachLeg_Override()
		{
			Env.Security.LocalTransportRunSheetCustomsClearanceControl.IsAllowed = false;
			RunSheetSecurityGUIProvider.Register(Factory);
			var provider = RunSheetSecurityProvider.GetProvider(Factory);
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var iShipment = (IWorkflowProvider)shipment;
			var customCleared = iShipment.WorkflowItems.Milestones.AddNew();
			customCleared.TriggerConditions.TriggerEventCode = Events.CustomsCleared.Code;
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			provider.TryAuthorise(leg);
			AssertEquals("Cancel on msg Dialog.", false, leg.IsRunSheetAuthorised);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			provider.TryAuthorise(leg);
			AssertEquals("Overriden, but cancel on Login Dialog.", false, leg.IsRunSheetAuthorised);
			// Override
			// Attempt to Login, but fail because no login or password entered.
			// After each login attempt, an error dialog will show.
			// Repeat 3 times, then cancel.
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;
			int i = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
			{
				var loginForm = (DocumentLoginForm)form;
				loginForm.Shown += delegate
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					if (i == 3)
					{
						loginForm.CancelPrintButton.PerformClick();
					}
					else
					{
						loginForm.PrintButton.PerformClick();
						i++;
					}

					ZFormModaliser.ResultToReturnFromShowDialog = loginForm.DialogResult; // ZFormModaliser overrides our button click dialog results
				};
			});
			provider.TryAuthorise(leg);
			AssertEquals("Should eventually cancel login.", false, leg.IsRunSheetAuthorised);
			AssertEquals("Should continue to be called if login incorrect, until Login Cancelled.", 3, i);
			// Override
			// Attempt to Login, but fail because no login or password entered.
			// After each login attempt, an error dialog will show.
			// Repeat 3 times, then authorise!
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;
			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			AssertEquals(true, loginController.ValidateUserLoginAndPassword(User.SupportUserName, User.MasterPassword).LoginValidated);
			i = 0;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
			{
				var loginForm = (DocumentLoginForm)form;
				loginForm.Shown += delegate
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					if (i == 3)
					{
						var loginBO = (SecurityLogin)loginForm.BusinessEntity;
						loginBO.Login = User.SupportUserName;
						loginBO.Password = User.MasterPassword;
						loginForm.PrintButton.PerformClick();
					}
					else
					{
						loginForm.PrintButton.PerformClick();
						i++;
					}

					ZFormModaliser.ResultToReturnFromShowDialog = loginForm.DialogResult; // ZFormModaliser overrides our button click dialog results
				};
			});
			provider.TryAuthorise(leg);
			AssertEquals("Should eventually authorise login.", true, leg.IsRunSheetAuthorised);
			AssertEquals("Should continue to be called if login incorrect, until Login Authorised.", 3, i);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
