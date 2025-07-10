using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MockZForm))]
	sealed class ChangeOthersSecurityControlTest : ZFormBasherTest
	{
		T GetFieldValue<T>(object fieldContainer, string fieldName)
		{
			FieldInfo info = fieldContainer.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			return (T)info.GetValue(fieldContainer);
		}

		T GetPropertyValue<T>(object propertyContainer, string propertyName)
		{
			PropertyInfo info = propertyContainer.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			return (T)info.GetValue(propertyContainer, null);
		}

		new MockZForm GetFormToBash()
		{
			return (MockZForm)base.GetFormToBash();
		}

		protected override Form GetFormToBashCore()
		{
			return new MockZForm(Factory.New<GlbGroup>());
		}

		void TestAdd(
			MockZForm form,
			ZButton addButton,
			BusinessObject objectBeingChanged,
			ZString codeForObjectBeingChanged,
			IFindBoxListProvider expectedListProvider,
			ModuleIdentifier expectedModuleID)
		{
			AssertEquals("SecurityChangeOthersView.Count", 0, form.BusinessEntity.SecurityChangeOthersView.Count);

			form.Show();
			addButton.PerformClick();

			using (EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm)
			{
				IFindBox findBox = GetPropertyValue<IFindBox>(popup, "FindBox");
				AssertEquals("FindBox.ListProvider", expectedListProvider, findBox.ListProvider);

				ZFilterModule module = popup.Module_ForTest;
				AssertEquals("Module.ID", expectedModuleID, module.ID);

				var decisionProvider = module.ModuleDecisionProvider;
				AssertEquals("Module.ModuleDecisionProvider.FindBox", findBox, GetFieldValue<IFindBox>(decisionProvider, "FindBox"));

				findBox.Code = codeForObjectBeingChanged;
				var collection = form.securityControl.mode == GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator ? form.BusinessEntity.SecurityChangeOthersView : form.BusinessEntity.GroupOwnersForGroupView;
				AssertEquals("SecurityChangeOthersView.Count", 1, collection.Count);
				var pk = form.securityControl.mode == GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator ? collection[0].GU_ItemGUID : (!collection[0].GU_GG.IsEmpty ? collection[0].GU_GG : collection[0].GU_GS);
				AssertEquals("SecurityChangeOthersView[0].GU_ItemGuid", objectBeingChanged.PK, pk);

				ZForm lastShownForm = (ZForm)form.SecurityControl.ShowForm(collection[0]);
				AssertEquals("ShowForm().BusinessEntity.PK", objectBeingChanged.PK, ((BusinessObject)lastShownForm.BusinessEntity).PK);
			}
		}

		public void TestAddGroup()
		{
			var groupBeingChanged = Factory.NewWithValidTestData<GlbGroup>();
			var controllerUser = Factory.NewWithValidTestData<GlbStaff>();
			controllerUser.GS_IsController = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(controllerUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (MockZForm form = GetFormToBash())
			{
				TestAdd(
					form,
					form.SecurityControl.addGroupButton,
					groupBeingChanged,
					groupBeingChanged.GG_Code,
					form.BusinessEntity.Lookups.CompleteGroupList,
					ModuleIDs.GlbGroup);
			}
		}

		public void TestAddGroup_GroupOwnersMode()
		{
			var groupBeingChanged = Factory.NewWithValidTestData<GlbGroup>();
			var controllerUser = Factory.NewWithValidTestData<GlbStaff>();
			controllerUser.GS_IsController = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(controllerUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (MockZForm form = GetFormToBash())
			{
				form.securityControl.mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup;
				TestAdd(
					form,
					form.SecurityControl.addGroupButton,
					groupBeingChanged,
					groupBeingChanged.GG_Code,
					form.BusinessEntity.Lookups.CompleteGroupList,
					ModuleIDs.GlbGroup);
			}
		}

		public void TestAddMultiGroup()
		{
			var firstGroupBeingSelected = Factory.NewWithValidTestData<GlbGroup>();
			var secondGroupBeingSelected = Factory.NewWithValidTestData<GlbGroup>();

			var controllerUser = Factory.NewWithValidTestData<GlbStaff>();
			controllerUser.GS_IsController = true;

			Factory.Save();

			using (MockZForm form = GetFormToBash())
			{
				form.Show();
				form.SecurityControl.addGroupButton.PerformClick();

				using (EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm)
				{
					var module = popup.Module_ForTest;
					var decisionProvider = module.ModuleDecisionProvider;
					decisionProvider.HandleDefaultAction(new BusinessObject[] { firstGroupBeingSelected, secondGroupBeingSelected });
					form.FireSaveButton();
					AssertEquals("SecurityChangeOthersView.Count", 2, form.BusinessEntity.SecurityChangeOthersView.Count);
				}
			}
		}
		public void TestAddStaff()
		{
			var staffBeingChanged = Factory.NewWithValidTestData<GlbStaff>();
			var controllerUser = Factory.NewWithValidTestData<GlbStaff>();
			controllerUser.GS_IsController = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(controllerUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (MockZForm form = GetFormToBash())
			{
				TestAdd(
					form,
					form.SecurityControl.addStaffButton,
					staffBeingChanged,
					staffBeingChanged.GS_Code,
					form.BusinessEntity.Lookups.CompleteStaffList,
					ModuleIDs.GlbStaff);
			}
		}

		[RequiresSTA]
		public void TestAddStaff_GroupOwnersMode()
		{
			var staffBeingChanged = Factory.NewWithValidTestData<GlbStaff>();
			var controllerUser = Factory.NewWithValidTestData<GlbStaff>();
			controllerUser.GS_IsController = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(controllerUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (MockZForm form = GetFormToBash())
			{
				form.securityControl.mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup;
				TestAdd(
					form,
					form.SecurityControl.addStaffButton,
					staffBeingChanged,
					staffBeingChanged.GS_Code,
					form.BusinessEntity.Lookups.CompleteStaffList,
					ModuleIDs.GlbStaff);
			}
		}

		public void TestAddMultiStaff()
		{
			var firstStaffBeingSelected = Factory.NewWithValidTestData<GlbStaff>();
			var secondStaffBeingSelected = Factory.NewWithValidTestData<GlbStaff>();

			var controllerUser = Factory.NewWithValidTestData<GlbStaff>();
			controllerUser.GS_IsController = true;

			Factory.Save();

			using (MockZForm form = GetFormToBash())
			{
				form.Show();
				form.SecurityControl.addStaffButton.PerformClick();

				using (EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm)
				{
					var module = popup.Module_ForTest;
					var decisionProvider = module.ModuleDecisionProvider;
					decisionProvider.HandleDefaultAction(new BusinessObject[] { firstStaffBeingSelected, secondStaffBeingSelected });

					form.FireSaveButton();
					AssertEquals("SecurityChangeOthersView.Count", 2, form.BusinessEntity.SecurityChangeOthersView.Count);
				}
			}
		}

		public void TestDeletionIsEnabledThenDelete()
		{
			var staffBeingChanged = Factory.NewWithValidTestData<GlbStaff>();
			var controllerUser = Factory.NewWithValidTestData<GlbStaff>();
			controllerUser.GS_IsController = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(controllerUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (MockZForm form = GetFormToBash())
			{
				Assert(form.SecurityControl.changeOthersSecurityGrid.AllowReadOnlyRowsToBeDeleted);
				TestAdd(
					form,
					form.SecurityControl.addStaffButton,
					staffBeingChanged,
					staffBeingChanged.GS_Code,
					form.BusinessEntity.Lookups.CompleteStaffList,
					ModuleIDs.GlbStaff);
				AssertEquals(1, form.SecurityControl.changeOthersSecurityGrid.VisibleRowCount);
				form.SecurityControl.changeOthersSecurityGrid.Select(0);
				Assert(form.SecurityControl.changeOthersSecurityGrid.DeleteMenuItem.Enabled);
				form.SecurityControl.deleteStaffOrGroupButton.PerformClick();
				form.FireSaveButton();
				AssertEquals(0, form.SecurityControl.changeOthersSecurityGrid.VisibleRowCount);
			}
		}

		public void TestEnabledState()
		{
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(true))
			using (ChangeOthersSecurityControl control = new ChangeOthersSecurityControl())
			{
				AssertEquals("Enabled", true, control.Enabled);

				control.Mode = control.Mode;
				AssertEquals("Enabled", true, control.Enabled);

				control.Enabled = false;
				AssertEquals("Enabled", true, control.Enabled);

				control.Enabled = false;
				AssertEquals("Enabled", true, control.Enabled);
			}

			using (Env.CurrentUser.SetIsControllerOverrideForTesting(false))
			using (ChangeOthersSecurityControl control = new ChangeOthersSecurityControl())
			{
				AssertEquals("Enabled", true, control.Enabled);

				control.Mode = control.Mode;
				AssertEquals("Enabled", false, control.Enabled);

				control.Enabled = true;
				AssertEquals("Enabled", false, control.Enabled);

				control.Enabled = true;
				AssertEquals("Enabled", false, control.Enabled);
			}
		}

		public void TestMode()
		{
			using (ChangeOthersSecurityControl control = new ChangeOthersSecurityControl())
			{
				control.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
				Assert(!control.addStaffButton.Visible);
				control.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
				Assert(control.addStaffButton.Visible);
			}
		}
		#region class MockZForm

		class MockZForm : ZForm
		{
			public ChangeOthersSecurityControl securityControl;

			public MockZForm(GlbGroup businessEntity)
				: base(businessEntity)
			{
				SecurityControl.ChangeOthersSecurity = businessEntity;
				this.CaptionRenderingEnabled = true;
			}

			public new GlbGroup BusinessEntity
			{
				get { return (GlbGroup)base.BusinessEntity; }
			}

			public ChangeOthersSecurityControl SecurityControl
			{
				get { return securityControl; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				securityControl = new ChangeOthersSecurityControl();
				Controls.Add(securityControl);
				securityControl.Dock = DockStyle.Fill;
				Size = new Size(800, 600);
			}
		}

		#endregion
	}
}
