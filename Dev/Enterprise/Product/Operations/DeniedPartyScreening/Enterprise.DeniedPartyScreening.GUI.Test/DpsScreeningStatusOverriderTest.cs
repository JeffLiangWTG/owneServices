using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DpsScreeningStatusOverriderTest : TestCaseWithFactory
	{
		public void TestOverrideScreeningStatusWhenPartiesNotSave_ShouldShowMessage()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(organization))
			{
				AssertSaveBeforeOverride(form);
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			using (var form = new ZForm(vessel))
			{
				AssertSaveBeforeOverride(form);
			}

			void AssertSaveBeforeOverride(ZForm form)
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Update Status to Permanent Clear", true);
				overrideMenu.PerformClick();

				AssertEquals("Please save the form before overriding the screening status", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenPartiesInactive_ShouldShowMessage()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_IsActive = false;
			Factory.Save();

			using (var form = new ZForm(organization))
			{
				AssertInactiveParties(form, organization.HumanReadableName);
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_IsActive = false;
			Factory.Save();

			using (var form = new ZForm(vessel))
			{
				AssertInactiveParties(form, vessel.HumanReadableName);
			}

			void AssertInactiveParties(ZForm form, string partyname)
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Update Status to Permanent Clear", true);
				overrideMenu.PerformClick();

				AssertEquals(string.Format(CultureInfo.InvariantCulture, "The selected {0} is inactive and its screening status cannot be modified.", partyname), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenPartiesStatusAlreadyExists_ShouldShowMessage()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			using (var form = new ZForm(organization))
			{
				AssertStatusAlreadyExist(form, "Update Status to Permanent Clear", ScreeningStatusesList.Codes.PermanentClear);
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();
			using (var form = new ZForm(vessel))
			{
				AssertStatusAlreadyExist(form, "Reset Status to Unknown", ScreeningStatusesList.Codes.Unknown);
			}

			void AssertStatusAlreadyExist(ZForm form, string menutItem, string statusCode)
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText(menutItem, true);
				overrideMenu.PerformClick();

				var parentBizO = (form.BusinessEntity as BusinessObject);
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "The {0} screening status is already ({1})", parentBizO.HumanReadableName, statusCode), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenPartiesStatusNotClearResetToUnknown_ShouldShowMessage()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			using (var form = new ZForm(organization))
			{
				AssertResetStatusUnknown(form);
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			using (var form = new ZForm(vessel))
			{
				AssertResetStatusUnknown(form);
			}

			void AssertResetStatusUnknown(ZForm form)
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Reset Status to Unknown", true);
				overrideMenu.PerformClick();

				AssertEquals("Cannot reset screening status to Unknown. The screening status must be override Clear", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenPartiesAcknowledgedComplianceRisk_UpdateStatusToClear_ShouldShowMessage()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			using (var form = new ZForm(organization))
			{
				AssertResetStatusUnknown(form);
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			using (var form = new ZForm(vessel))
			{
				AssertResetStatusUnknown(form);
			}

			void AssertResetStatusUnknown(ZForm form)
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Update Status to Permanent Clear", true);
				overrideMenu.PerformClick();

				var expectedMessage = string.Format(@"You are about to change the screening status of {0} to Override Permanent Clear.

Doing so will exclude this record from Denied Party Screening processes. This records status will not be reset when data on the record is updated or when denied party lists are amended.

This operation must only be done by staff that understand the impacts it will have on their compliance process", (form.BusinessEntity as BusinessObject).HumanReadableName);

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenPartiesAcknowledgedComplianceRisk_ResetStatusToUnknown_ShouldShowMessage()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			using (var form = new ZForm(organization))
			{
				AssertResetStatusUnknown(form);
			}

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			using (var form = new ZForm(vessel))
			{
				AssertResetStatusUnknown(form);
			}

			void AssertResetStatusUnknown(ZForm form)
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Reset Status to Unknown", true);
				overrideMenu.PerformClick();

				var expectedMessage = string.Format(@"You are about to reset the {0} status to Unknown.

Doing so will reset the overridden screening status

This operation must only be done by staff that understand the impacts it will have on their compliance process", (form.BusinessEntity as BusinessObject).HumanReadableName);

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenInvalidSecurityRights_ShowMessage()
		{
			var securityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			securityCore.DpsAllowOverrideScreeningStatus.IsAllowed = false;
			securityCore.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed = false;

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ZForm(organization))
			{
				AssertInvalidSecurityRights(form, "Update Status to Permanent Clear", ScreeningStatusesList.Codes.PermanentClear, securityCore.DpsAllowOverrideScreeningStatusUpdateToClear.DisplayTextPathToSecurityRight);

				organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				Factory.Save();

				AssertInvalidSecurityRights(form, "Reset Status to Unknown", ScreeningStatusesList.Codes.Unknown, securityCore.DpsAllowOverrideScreeningStatus.DisplayTextPathToSecurityRight);
			}

			void AssertInvalidSecurityRights(ZForm form, string menuItem, string overrideStatus, string securityPath)
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText(menuItem, true);
				overrideMenu.PerformClick();

				var statusDescription = new ScreeningStatusesList().GetDescriptionFromCode(overrideStatus);
				var expectedMessage = string.Format(@"You do not have the security right to override screening status to '{0}'. The security right you require is '{1}'.

Contact the supervisor for further instructions or escalate the issue to a user who has security rights to override this restriction.

Do you wish to override this restriction by logging in as a user with this security right?", statusDescription, securityPath);

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenInvalidSecurityRights_WithCancelComplianceLogin()
		{
			var securityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			securityCore.DpsAllowOverrideScreeningStatus.IsAllowed = false;
			securityCore.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed = false;

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ZForm(organization))
			{
				AssertInvalidSecurityRights(form, "Update Status to Permanent Clear", ScreeningStatusesList.Codes.PermanentClear, securityCore.DpsAllowOverrideScreeningStatusUpdateToClear.DisplayTextPathToSecurityRight);
				AssertEquals(ScreeningStatusesList.Codes.NotScreened, organization.OH_ScreeningStatus);

				organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				Factory.Save();

				AssertInvalidSecurityRights(form, "Reset Status to Unknown", ScreeningStatusesList.Codes.Unknown, securityCore.DpsAllowOverrideScreeningStatus.DisplayTextPathToSecurityRight);
				AssertEquals(ScreeningStatusesList.Codes.PermanentClear, organization.OH_ScreeningStatus);
			}

			void AssertInvalidSecurityRights(ZForm form, string menuItem, string overrideStatus, string securityPath)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText(menuItem, true);
				overrideMenu.PerformClick();

				var statusDescription = new ScreeningStatusesList().GetDescriptionFromCode(overrideStatus);
				var expectedMessage = string.Format(@"You do not have the security right to override screening status to '{0}'. The security right you require is '{1}'.

Contact the supervisor for further instructions or escalate the issue to a user who has security rights to override this restriction.

Do you wish to override this restriction by logging in as a user with this security right?", statusDescription, securityPath);

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOverrideScreeningStatusWhenOrganizationUpdateStatusToClear_ShouldChangeStatusWithCreatedLogs()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZForm(organization))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Update Status to Permanent Clear", true);
				overrideMenu.PerformClick();

				AssertOverrideStatusWithCreatedLogs(organization, DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear, "NEW=CLP|OLD=NOT");
				AssertEquals(ScreeningStatusesList.Codes.PermanentClear, organization.OH_ScreeningStatus);
			}
		}

		public void TestOverrideScreeningStatusWhenVesselUpdateStatusToClear_ShouldChangeStatusWithCreatedLogs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();

			using (var form = new ZForm(vessel))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Update Status to Permanent Clear", true);
				overrideMenu.PerformClick();

				AssertOverrideStatusWithCreatedLogs(vessel, DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear, "NEW=CLP|OLD=NOT");
				AssertEquals(ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);
			}
		}

		public void TestOverrideScreeningStatusWhenOrganizationResetStatusToUnknown_ShouldChangeStatusWithCreatedLogs()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			using (var form = new ZForm(organization))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Reset Status to Unknown", true);
				overrideMenu.PerformClick();

				AssertOverrideStatusWithCreatedLogs(organization, DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, "NEW=UNK|OLD=CLP");
				AssertEquals(ScreeningStatusesList.Codes.Unknown, organization.OH_ScreeningStatus);
			}
		}

		public void TestOverrideScreeningStatusWhenVesselResetStatusToUnknown_ShouldChangeStatusWithCreatedLogs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			using (var form = new ZForm(vessel))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Reset Status to Unknown", true);
				overrideMenu.PerformClick();

				AssertOverrideStatusWithCreatedLogs(vessel, DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges, "NEW=UNK|OLD=CLP");
				AssertEquals(ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
			}
		}

		public void TestCreateOverrideScreeningStatusMenuItems_ShouldNotExistOnGenericForm()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummyBizO))
			using (var module = new DummyPartiesModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var menu = module.FormActionMenu.FindByText("Override Screening Status", true);

				AssertNull("Override Screening Status should not exist", menu);
			}
		}

		public void TestCreateOverrideScreeningStatusMenuItems_ShouldExistOnVesselsForm()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();

			using (var form = new RefVesselForm(vessel))
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideScreeningItem = FindOverrideScreeningMenuItem(form);

				AssertOverrideScreeningStatusMenuItemsExist(overrideScreeningItem);
			}
		}

		public void TestCreateOverrideScreeningStatusMenuItems_ShouldExistOnOrganizationsForm()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new BaseOrganisationsForm(organization))
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideScreeningItem = FindOverrideScreeningMenuItem(form);

				AssertOverrideScreeningStatusMenuItemsExist(overrideScreeningItem);
			}
		}

		public void TestCreateOverrideScreeningStatusMenuItems_ShouldNotExistOnPartiesModule()
		{
			using (var form = new Form())
			using (var module = new DummyPartiesModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var menu = module.FormActionMenu.FindByText("Override Screening Status", true);

				AssertNull("Override Screening Status should not exist", menu);
			}
		}

		public void TestSystemDefinedUnmatchedOrganisation_ShouldShowMessage()
		{
			var header = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			AssertEquals("Precondition : System Defined UNMATCHED Organization", "UNMATCHED", header.OH_Code);
			Assert("Precondition : UNMATCHED is system Defined", header.IsSystemDefinedOrganisation);

			using (var form = new ZForm(header))
			{
				var provider = new DeniedPartyScreeningActionsProvider(form);
				DpsScreeningStatusOverrider.AddActionsMenuItem(provider);
				MenuItem overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Update Status to Permanent Clear", true);
				overrideMenu.PerformClick();

				var expectedMessage = "The selected organization is a system defined organization and its screening status cannot be modified.";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				overrideMenu = FindOverrideScreeningMenuItem(form).MenuItems.FindByText("Reset Status to Unknown", true);
				overrideMenu.PerformClick();

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		MenuItem FindOverrideScreeningMenuItem(ZForm form)
		{
			return form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Override Screening Status");
		}

		void AssertOverrideScreeningStatusMenuItemsExist(MenuItem actionsMenu)
		{
			AssertNotNull(actionsMenu);
			AssertEquals(2, actionsMenu.MenuItems.Count);
			AssertEquals("Override Screening Status", actionsMenu.Text);
			AssertEquals("Update Status to Permanent Clear", actionsMenu.MenuItems[0].Text);
			AssertEquals("Reset Status to Unknown", actionsMenu.MenuItems[1].Text);
		}

		void AssertOverrideStatusWithCreatedLogs(BusinessObject bizO, string dpsLogStatus, string eventReference)
		{
			//DPS Log
			var dpsLog = new StmEntityScreeningLogCollection(bizO);
			AssertEquals(1, dpsLog.Count);
			AssertEquals("Override Screening Status", dpsLog[0].PJ_ClearedReason);
			AssertEquals(dpsLogStatus, dpsLog[0].PJ_Status);
			AssertEquals(bizO.PK, dpsLog[0].PJ_ParentID);
			AssertEquals(bizO.TablePrefix, dpsLog[0].PJ_ParentTableCode);
			AssertEquals(bizO.PK, dpsLog[0].PJ_SourceID);
			AssertEquals(bizO.TablePrefix, dpsLog[0].PJ_SourceTableCode);

			//Workflow Tracking Event
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeniedPartyStatusUpdatedCode);
			var eventLog = bizO.GetLogs().Find(query).First();
			AssertContains(eventReference, eventLog.SL_Reference);

			//EDI Message
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.JDC);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ScreeningRequest);

			var ediMessages = Factory.Load<DpsEDIMessage>(filter);
			AssertEquals("One DPS EDI message should have been created", 1, ediMessages.Length);

			var content = JsonConvert.DeserializeObject<DpsEDIMessageCandidatesContent>(ediMessages[0].EM_MessageData.ToUTF8());
			AssertNotNull(content);
			AssertEquals("ID is correct for the created EDI message", bizO.PK.ToGuid(), content.ClientSpecifiedIdentifier);
			AssertEquals("Type is correct for the created EDI message", bizO.TablePrefix, content.EntityType);
			AssertEquals("Status is correct for the created EDI message", ((IScreeningStatusProvider)bizO).ScreeningStatus, content.PersistentStatus);
			AssertNotNull("Dps Name Candidates should not be null for the created EDI message", content.DpsNameCandidates);
		}

		protected override void SetUp()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			base.SetUp();
		}

		class DummyPartiesModule : DummyFilterGridModule
		{
			protected override MenuItem[] GetNewActionMenuItems()
			{
				List<MenuItem> results = new List<MenuItem>(base.GetNewActionMenuItems());

				var presentationManager = new DeniedPartyScreeningPresentationManager();
				presentationManager.CreateModuleMenusForScreeningEntity(this, results);

				return results.ToArray();
			}
		}
	}
}
