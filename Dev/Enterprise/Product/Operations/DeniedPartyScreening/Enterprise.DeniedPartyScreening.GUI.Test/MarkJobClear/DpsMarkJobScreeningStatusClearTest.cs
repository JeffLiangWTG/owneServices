using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DpsMarkJobScreeningStatusClearTest : TestCaseWithFactory
	{
		public void TestDeniedPartyProviderGetShipmentReferenceId()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00002373";

			AssertEquals("S00002373", (shipment as IDeniedPartyProvider).ReferenceId);
		}

		public void TestDeniedPartyProviderGetDeclaratiobReferenceId()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00002373";

			AssertEquals("B00002373", (declaration as IDeniedPartyProvider).ReferenceId);
		}

		public void TestDeniedPartyProviderGetConsolidationReferenceId()
		{
			var declaration = Factory.New<IForwardingConsol>();
			declaration.JK_UniqueConsignRef = "C00002373";

			AssertEquals("C00002373", (declaration as IDeniedPartyProvider).ReferenceId);
		}

		public void TestDeniedPartyProviderGetWhsOrderReferenceId()
		{
			var order = Factory.New<IWhsOrder>();
			order.WD_DocketID = "ORD2373";

			AssertEquals("ORD2373", (order as IDeniedPartyProvider).ReferenceId);
		}

		public void TestDeniedPartyProviderGetWhsReceiveReferenceId()
		{
			var receive = Factory.New<IWhsReceive>();
			receive.WD_DocketID = "RCV2373";

			AssertEquals("RCV2373", (receive as IDeniedPartyProvider).ReferenceId);
		}

		public void TestShouldThrowArgumentNullException()
		{
			var shipment = Factory.New<IForwardingShipment>();
			using (var form = new ZForm(shipment))
			{
				var exception = AssertExceptionThrown<Exception>(() => new DeniedPartyScreeningActionsProvider(form).AddJobsMenuItem());
				AssertEquals(typeof(ArgumentNullException), exception.GetType());
				AssertContains("Should have valid business entity", exception.Message);
			}
		}

		public void TestValidationWhenChangesOrNotInDatabase_ShouldShowMessage()
		{
			var shipment = Factory.New<IForwardingShipment>();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				new DeniedPartyScreeningActionsProvider(form, (IBusiness)shipment).AddJobsMenuItem();

				PerformMenuItemClick(form);
				AssertEquals(ExpectedMessageSaveBeforeMarking, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestValidationWhenStatusIsClearOrJobClear_ShouldShowMessage()
		{
			var shipment = Factory.New<IForwardingShipment>();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				new DeniedPartyScreeningActionsProvider(form, (IBusiness)shipment).AddJobsMenuItem();

				PerformMenuItemClick(form);
				AssertEquals(ExpectedMessageAlreadyClearOrJobClear, UnitTestUserNotification.Instance.LastMessage.Text);

				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				Factory.Save();

				PerformMenuItemClick(form);
				AssertEquals(ExpectedMessageAlreadyClearOrJobClear, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestValidationWhenSecurityRightsNotGranted_ShouldShowMessage()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var security = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			security.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;

			using (var form = new ZForm(shipment))
			using (Env.SetTemporarySecurityInstanceForTest(security))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();

				new DeniedPartyScreeningActionsProvider(form, (IBusiness)shipment).AddJobsMenuItem();

				PerformMenuItemClick(form);
				AssertEquals(ExpectedMessageSecurityRights(security), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetDefaultJobClearedReasonWithCreateDPSLogStatusJCL()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<IForwardingShipment>();
				using (var form = new ZForm(shipment))
				using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					Factory.Save();

					new DeniedPartyScreeningActionsProvider(form, (IBusiness)shipment).AddJobsMenuItem();

					PerformMenuItemClick(form);

					var confirmationForm = (DpsMarkJobClearConfirmationForm)ZFormModaliser.LastFormShownDialogForTest;
					var confirmationModel = (DpsMarkJobClearConfirmationModel)confirmationForm.LastDataSourceForTest;
					var latestLog = (shipment as BusinessObject).GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);

					AssertEquals("DEF", confirmationModel.Code);
					AssertEquals("Default", confirmationModel.JobClearingReasonList[confirmationModel.Code]?.Description);
					AssertContains("|NEW=JCL|OLD=NOT|TYP=MAN", latestLog.SL_Reference);
					AssertEquals(ScreeningStatusesList.Codes.JobCleared, shipment.JS_ScreeningStatus);
				}
			}
		}

		public void TestGetShipmentRelatedJobIdsWhenStatusNotJCLorCLR()
		{
			var masterShipment = Factory.New<IForwardingShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var coloadShipment1 = Factory.New<IForwardingShipment>();
			coloadShipment1.JS_UniqueConsignRef = "dummy1";
			coloadShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var coloadShipment2 = Factory.New<IForwardingShipment>();
			coloadShipment2.JS_UniqueConsignRef = "dummy2";
			coloadShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();

			using (var form = new ZForm(masterShipment))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				masterShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				Factory.Save();

				new DeniedPartyScreeningActionsProvider(form, (IBusiness)masterShipment).AddJobsMenuItem();

				PerformMenuItemClick(form);

				var confirmationForm = (DpsMarkJobClearConfirmationForm)ZFormModaliser.LastFormShownDialogForTest;
				var confirmationModel = (DpsMarkJobClearConfirmationModel)confirmationForm.LastDataSourceForTest;

				CombineAssertions("Assert ID of master shipment related jobs that are not JCL or CLR", () =>
				{
					AssertType(typeof(DpsMarkJobClearConfirmationForm), confirmationForm);
					AssertType(typeof(DpsMarkJobClearConfirmationModel), confirmationModel);
					AssertEquals(masterShipment.JS_UniqueConsignRef, confirmationModel.JobID);
					AssertContainsExactElementsInAnyOrder(new[] { "dummy1", "dummy2" }, confirmationModel.RelatedJobsIDNotJCLOrCLR);
				});
			}
		}

		public void TestGetConsolidationRelatedJobIdsWhenStatusNotJCLorCLR()
		{
			var masterShipment = Factory.New<IForwardingShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_UniqueConsignRef = "dummy master";
			masterShipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var coloadShipment1 = Factory.New<IForwardingShipment>();
			coloadShipment1.JS_UniqueConsignRef = "dummy coload 1";
			coloadShipment1.JS_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			coloadShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var coloadShipment2 = Factory.New<IForwardingShipment>();
			coloadShipment2.JS_UniqueConsignRef = "dummy coload 2";
			coloadShipment2.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			coloadShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var coloadShipment3 = Factory.New<IForwardingShipment>();
			coloadShipment3.JS_UniqueConsignRef = "dummy coload 3";
			coloadShipment3.JS_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			coloadShipment3.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = Factory.New<IForwardingConsol>();
			consol.AddShipment(masterShipment);

			Factory.Save();

			using (var form = new ZForm(consol))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				Factory.Save();

				new DeniedPartyScreeningActionsProvider(form, (IBusiness)consol).AddJobsMenuItem();

				PerformMenuItemClick(form);

				var confirmationForm = (DpsMarkJobClearConfirmationForm)ZFormModaliser.LastFormShownDialogForTest;
				var confirmationModel = (DpsMarkJobClearConfirmationModel)confirmationForm.LastDataSourceForTest;

				CombineAssertions("Assert ID of master shipment related jobs that are not JCL or CLR", () =>
				{
					AssertType(typeof(DpsMarkJobClearConfirmationForm), confirmationForm);
					AssertType(typeof(DpsMarkJobClearConfirmationModel), confirmationModel);
					AssertEquals(consol.JK_UniqueConsignRef, confirmationModel.JobID);
					AssertContainsExactElementsInAnyOrder(new[] { "dummy coload 1", "dummy coload 3" }, confirmationModel.RelatedJobsIDNotJCLOrCLR);
				});
			}
		}

		public void TestMarkJobClearMenuItemWhenRegistryIsDisabled_ShouldNotCreateMenu()
		{
			var dummyBizO = Factory.New<IForwardingShipment>();
			using (var form = new ZForm(dummyBizO))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				new DeniedPartyScreeningActionsProvider(form, (IBusiness)dummyBizO).AddJobsMenuItem();

				AssertNull("Should not create Mark as Job Clear menu item", GetActionsMenuItems(form));
			}
		}

		#region Common Shared Assertions

		public void AssertSecurityRightsAccessibilityCheckpoint(ZForm formProvider, SecurityCore security)
		{
			PerformMenuItemClick(formProvider);
			AssertEquals(ExpectedMessageSecurityRights(security), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void AssertMenuItemAccessibilityCheckpoint(ZForm formProvider, bool registrySetting)
		{
			if (registrySetting)
			{
				AssertNotNull("Should add menu when registry is enabled", GetActionsMenuItems(formProvider));
			}
			else
			{
				AssertNull("Should not add menu when registry is disabled", GetActionsMenuItems(formProvider));
			}
		}

		public void AssertScreeenigStatusToJCL(ZForm formProvider, BusinessObject bizO)
		{
			PerformMenuItemClick(formProvider);
			AssertEquals(ScreeningStatusesList.Codes.JobCleared, (bizO as IScreeningStatusProvider).ScreeningStatus);
		}

		public void AssertSaveBeforeMarkingClear(ZForm formProvider)
		{
			PerformMenuItemClick(formProvider);
			AssertEquals(ExpectedMessageSaveBeforeMarking, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void AssertStatusAlreadyClearOrJobClear(ZForm formProvider)
		{
			PerformMenuItemClick(formProvider);
			AssertEquals(ExpectedMessageAlreadyClearOrJobClear, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		const string ExpectedMessageAlreadyClearOrJobClear = "There is no need to Mark as Job Clear when screening status is \"CLR\" or \"JCL\".";

		const string ExpectedMessageSaveBeforeMarking = "Please save the form before marking as job clear.";

		string ExpectedMessageSecurityRights(SecurityCore security) => string.Format(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", security.OrgDeniedPartyScreeningAllowJobLevelClear.DisplayTextPathToSecurityRight);

		#endregion

		void PerformMenuItemClick(ZForm formProvider)
		{
			UnitTestUserNotification.Instance.ClearMessages();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var menuItem = GetActionsMenuItems(formProvider);
			AssertNotNull("Precondition Mark as Job Clear menu item exists", menuItem);
			menuItem.PerformClick();
		}

		MenuItem GetActionsMenuItems(ZForm form)
		{
			return form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Mark as Job Clear", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			UnitTestUserNotification.Instance.ClearMessages();
		}
	}
}
