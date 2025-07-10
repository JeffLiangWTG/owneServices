using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class DPSSecurityProviderTest : TestCaseWithFactory
	{
		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		public void TestDPSValidation_CanFinalise_WhenDPSNotMatched_Clear() => TestDPSValidation_CanFinalise_WhenDPSNotMatched(ScreeningStatusesList.Codes.Clear);
		public void TestDPSValidation_CanFinalise_WhenDPSNotMatched_JobCleared() => TestDPSValidation_CanFinalise_WhenDPSNotMatched(ScreeningStatusesList.Codes.JobCleared);

		void TestDPSValidation_CanFinalise_WhenDPSNotMatched(string status)
		{
			var dpsValidation = new DPSSecurityProvider(new NotificationManager().Peek);

			var docket = Factory.New<WhsOrder>();
			docket.WD_ScreeningStatus = status;
			docket.WD_DocketSubType = OrderType.Codes.Customs;

			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				AssertEquals("Finalise allowed as DPS not matched", true, dpsValidation.ValidateDPS(docket));
			}
		}

		public void TestDPSValidation_CannotFinalise_WhenDPSMatched_Matched() => TestDPSValidation_CannotFinalise_WhenDPSMatched(ScreeningStatusesList.Codes.Matched);
		public void TestDPSValidation_CannotFinalise_WhenDPSMatched_Unknown() => TestDPSValidation_CannotFinalise_WhenDPSMatched(ScreeningStatusesList.Codes.Unknown);
		public void TestDPSValidation_CannotFinalise_WhenDPSMatched_NeedsScreening() => TestDPSValidation_CannotFinalise_WhenDPSMatched(ScreeningStatusesList.Codes.NeedsScreening);

		void TestDPSValidation_CannotFinalise_WhenDPSMatched(string status)
		{
			var dpsValidation = new DPSSecurityProvider(new NotificationManager().Peek);

			var docket = Factory.New<WhsOrder>();
			docket.WD_ScreeningStatus = status;
			docket.WD_DocketSubType = OrderType.Codes.Customs;

			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				AssertEquals("Finalise not allowed as DPS is matched", false, dpsValidation.ValidateDPS(docket));
			}
		}

		public void TestDPSValidation_CanFinalise_WhenDPSMatched_RegistryItemDisabled_Order_Matched() => TestDPSValidation_CanFinalise_WhenDPSMatched_Order_RegistryItemDisabled(ScreeningStatusesList.Codes.Matched);
		public void TestDPSValidation_CanFinalise_WhenDPSMatched_RegistryItemDisabled_Order_Unknown() => TestDPSValidation_CanFinalise_WhenDPSMatched_Order_RegistryItemDisabled(ScreeningStatusesList.Codes.Unknown);
		public void TestDPSValidation_CanFinalise_WhenDPSMatched_RegistryItemDisabled_Order_NeedsScreening() => TestDPSValidation_CanFinalise_WhenDPSMatched_Order_RegistryItemDisabled(ScreeningStatusesList.Codes.NeedsScreening);

		void TestDPSValidation_CanFinalise_WhenDPSMatched_Order_RegistryItemDisabled(string status)
		{
			var dpsValidation = new DPSSecurityProvider(new NotificationManager().Peek);

			var docket = Factory.New<WhsOrder>();
			docket.WD_ScreeningStatus = status;
			docket.WD_DocketSubType = OrderType.Codes.Customs;

			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				AssertEquals("Precondition: Finalise not allowed as DPS is matched", false, dpsValidation.ValidateDPS(docket));

				using (WarehouseDataRegistry.Instance.ValidateDPSRestrictionsOnFinalisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Finalise allowed as registry item is disabled.", true, dpsValidation.ValidateDPS(docket));
				}
			}
		}

		public void TestDPSValidation_CanFinalise_WhenDPSMatched_RegistryItemDisabled_Receive_Matched() => TestDPSValidation_CanFinalise_WhenDPSMatched_Receive_RegistryItemDisabled(ScreeningStatusesList.Codes.Matched);
		public void TestDPSValidation_CanFinalise_WhenDPSMatched_RegistryItemDisabled_Receive_Unknown() => TestDPSValidation_CanFinalise_WhenDPSMatched_Receive_RegistryItemDisabled(ScreeningStatusesList.Codes.Unknown);
		public void TestDPSValidation_CanFinalise_WhenDPSMatched_RegistryItemDisabled_Receive_NeedsScreening() => TestDPSValidation_CanFinalise_WhenDPSMatched_Receive_RegistryItemDisabled(ScreeningStatusesList.Codes.NeedsScreening);

		void TestDPSValidation_CanFinalise_WhenDPSMatched_Receive_RegistryItemDisabled(string status)
		{
			var dpsValidation = new DPSSecurityProvider(new NotificationManager().Peek);

			var docket = Factory.New<WhsReceive>();
			docket.WD_ScreeningStatus = status;
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;

			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				AssertEquals("Precondition: Finalise not allowed as DPS is matched", false, dpsValidation.ValidateDPS(docket));

				using (WarehouseDataRegistry.Instance.ValidateDPSRestrictionsOnFinalisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Finalise allowed as registry item is disabled.", true, dpsValidation.ValidateDPS(docket));
				}
			}
		}

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				var dpsValidation = new DPSSecurityProvider(new NotificationManager().Peek);

				var docket = Factory.New<WhsOrder>();
				docket.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				docket.WD_DocketSubType = OrderType.Codes.Customs;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("Cancel on msg Dialog.", false, dpsValidation.ValidateDPS(docket));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				AssertEquals("Overriden, but cancel on Login Dialog.", false, dpsValidation.ValidateDPS(docket));

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

				AssertEquals("Should eventually cancel login.", false, dpsValidation.ValidateDPS(docket));
				AssertEquals("Should continue to be called if login incorrect, until Login Cancelled.", 3, i);
			}
		}

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive_Matched()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Descriptions.Matched);

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive_Unknown()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Descriptions.Unknown);

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive_NeedsScreening()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive(ScreeningStatusesList.Codes.NeedsScreening, ScreeningStatusesList.Descriptions.NeedsScreening);

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive_InvalidCode()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive("LOL", "LOL");

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive(string statusCode, string statusDesc)
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50);
				receive.AllocateLocationsWithMock();
				Factory.Save();

				receive.WD_ScreeningStatus = statusCode;
				receive.FinaliseDocket();

				var expectedMessageString = $@"This Receive cannot be finalized due to a Screening Status of '{statusDesc}'.

Contact the supervisor for further instructions or escalate to a user who has security rights to override this restriction.

Do you wish to override and allow finalization?";

				AssertEquals(expectedMessageString, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("Cancel on msg Dialog.", false, receive.IsFinalised);

				receive.FinaliseDocket();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				AssertEquals("Overriden, but cancel on Login Dialog.", false, receive.IsFinalised);

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

				receive.FinaliseDocket();
				AssertEquals("Should eventually cancel login.", false, receive.IsFinalised);
				AssertEquals("Should continue to be called if login incorrect, until Login Cancelled.", 3, i);
			}
		}

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order_Matched()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Descriptions.Matched);

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order_Unknown()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order(ScreeningStatusesList.Codes.Unknown, ScreeningStatusesList.Descriptions.Unknown);

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order_NeedsScreening()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order(ScreeningStatusesList.Codes.NeedsScreening, ScreeningStatusesList.Descriptions.NeedsScreening);

		[GuiTest]
		public void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order_InvalidCode()
			=> TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order("LOL", "LOL");

		void TestDPSValidation_DenyOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order(string statusCode, string statusDesc)
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				Helper.CreateWhsOrderLine(order, data.Part1, 50);
				Factory.Save();

				order.WD_ScreeningStatus = statusCode;
				Helper.CreatePickNew(order);
				order.FinaliseDocket();

				var expectedMessageString = $@"This Order cannot be finalized due to a Screening Status of '{statusDesc}'.

Contact the supervisor for further instructions or escalate to a user who has security rights to override this restriction.

Do you wish to override and allow finalization?";

				AssertEquals(expectedMessageString, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("Cancel on msg Dialog.", false, order.IsFinalised);

				order.FinaliseDocket();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				AssertEquals("Overriden, but cancel on Login Dialog.", false, order.IsFinalised);

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

				order.FinaliseDocket();
				AssertEquals("Should eventually cancel login.", false, order.IsFinalised);
				AssertEquals("Should continue to be called if login incorrect, until Login Cancelled.", 3, i);
			}
		}

		[GuiTest]
		public void TestDPSValidation_AllowOverride_WhenUserHasPermission()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				var dpsValidation = new DPSSecurityProvider(new NotificationManager().Peek);

				var docket = Factory.New<WhsOrder>();
				docket.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				docket.WD_DocketSubType = OrderType.Codes.Customs;

				// Override
				// Attempt to Login, but fail because no login or password entered.
				// After each login attempt, an error dialog will show.
				// Repeat 3 times, then authorise
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;

				var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
				AssertEquals(true, loginController.ValidateUserLoginAndPassword(User.SupportUserName, User.MasterPassword).LoginValidated);

				var i = 0;
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
							Assert(loginBO.HideApprovalRequestButton);

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

				AssertEquals("Should eventually authorise login.", true, dpsValidation.ValidateDPS(docket));
				AssertEquals("Should continue to be called if login incorrect, until Login Authorised.", 3, i);
			}
		}

		[GuiTest]
		public void TestDPSValidation_AllowOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Receive()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50);
				receive.AllocateLocationsWithMock();
				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				receive.FinaliseDocket();
				AssertEquals("Precondition - ensure Receive is not finalised.", false, receive.IsFinalised);

				SetupPassDialog();

				receive.FinaliseDocket();
				AssertEquals("Should eventually authorise login.", true, receive.IsFinalised);
			}
		}

		[GuiTest]
		public void TestDPSValidation_AllowOverride_WhenUserDoesNotHavePermission_FromFinaliseDocket_Order()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				Helper.CreateWhsOrderLine(order, data.Part1, 50);
				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Helper.CreatePickNew(order);
				order.FinaliseDocket();
				AssertEquals("Precondition - ensure Receive is not finalised.", false, order.IsFinalised);

				SetupPassDialog();

				order.FinaliseDocket();
				AssertEquals("Should eventually authorise login.", true, order.IsFinalised);
			}
		}

		public void TestDPSValidation_FreightMovementUnrestricted()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "NO"))
			{
				var dpsValidation = new DPSSecurityProvider(new NotificationManager().Peek);

				var docket = Factory.New<WhsOrder>();
				docket.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				docket.WD_DocketSubType = OrderType.Codes.Customs;

				AssertEquals("Should not have shown any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have successfully validated DPS.", true, dpsValidation.ValidateDPS(docket));
			}
		}

		public void TestDPSValidation_FreightMovementUnrestricted_FromFinaliseDocket_Receive()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "NO"))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50);
				receive.AllocateLocationsWithMock();
				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

				receive.FinaliseDocket();
				AssertEquals("Should not have shown any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have finalised the docket.", true, receive.IsFinalised);
			}
		}

		public void TestDPSValidation_FreightMovementUnrestricted_FromFinaliseDocket_Order()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "NO"))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				Helper.CreateWhsOrderLine(order, data.Part1, 50);
				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Helper.CreatePickNew(order);

				order.FinaliseDocket();
				AssertEquals("Should not have shown any messages.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have finalised the docket.", true, order.IsFinalised);
			}
		}

		void SetupPassDialog()
		{
			// Override
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;

			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			AssertEquals(true, loginController.ValidateUserLoginAndPassword(User.SupportUserName, User.MasterPassword).LoginValidated);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
			{
				var loginForm = (DocumentLoginForm)form;

				loginForm.Shown += delegate
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					var loginBO = (SecurityLogin)loginForm.BusinessEntity;
					Assert(loginBO.HideApprovalRequestButton);

					loginBO.Login = User.SupportUserName;
					loginBO.Password = User.MasterPassword;
					loginForm.PrintButton.PerformClick();

					ZFormModaliser.ResultToReturnFromShowDialog = loginForm.DialogResult; // ZFormModaliser overrides our button click dialog results
				};
			});
		}
	}
}
