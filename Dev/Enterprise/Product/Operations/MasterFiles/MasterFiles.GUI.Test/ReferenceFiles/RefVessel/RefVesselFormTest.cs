using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefVesselForm))]
	sealed class RefVesselFormTest : ZFormBasherTest
	{
		public void TestCustomFieldsWhenNoneAreSetup()
		{
			FreightDataRegistry.Instance.VesselCustomAttribute1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			FreightDataRegistry.Instance.VesselCustomAttribute2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			FreightDataRegistry.Instance.VesselCustomAttribute3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			FreightDataRegistry.Instance.VesselCustomDecimal1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			FreightDataRegistry.Instance.VesselCustomFlag1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			RefVessel vessel = Factory.New<RefVessel>();
			using (RefVesselTestForm testForm = new RefVesselTestForm(vessel))
			{
				testForm.Show();

				AssertEquals("CustomAttribute1TextBox.Visible", false, testForm.GetCustomAttribute1TextBox().Visible);
				AssertEquals("CustomAttribute2TextBox.Visible", false, testForm.GetCustomAttribute2TextBox().Visible);
				AssertEquals("CustomAttribute3TextBox.Visible", false, testForm.GetCustomAttribute3TextBox().Visible);
				AssertEquals("CustomFlag1CheckBox.Visible", false, testForm.GetCustomFlag1CheckBox().Visible);
				AssertEquals("CustomDecimal1CalcEdit.Visible", false, testForm.GetCustomDecimal1CalcEdit().Visible);
			}
		}

		public void TestCustomFieldsWhenAllAreSetup()
		{
			FreightDataRegistry.Instance.VesselCustomAttribute1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Attribute1Caption");
			FreightDataRegistry.Instance.VesselCustomAttribute2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Attribute2Caption");
			FreightDataRegistry.Instance.VesselCustomAttribute3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Attribute3Caption");
			FreightDataRegistry.Instance.VesselCustomDecimal1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Decimal1Caption");
			FreightDataRegistry.Instance.VesselCustomFlag1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Flag1Caption");

			RefVessel vessel = Factory.New<RefVessel>();
			using (RefVesselTestForm testForm = new RefVesselTestForm(vessel))
			{
				testForm.Show();

				AssertEquals("CustomAttribute1TextBox.Visible", true, testForm.GetCustomAttribute1TextBox().Visible);
				AssertEquals("CustomAttribute2TextBox.Visible", true, testForm.GetCustomAttribute2TextBox().Visible);
				AssertEquals("CustomAttribute3TextBox.Visible", true, testForm.GetCustomAttribute3TextBox().Visible);
				AssertEquals("CustomFlag1CheckBox.Visible", true, testForm.GetCustomFlag1CheckBox().Visible);
				AssertEquals("CustomDecimal1CalcEdit.Visible", true, testForm.GetCustomDecimal1CalcEdit().Visible);
			}
		}

		public void TestCustomFieldsWhenSomeAreSetup()
		{
			FreightDataRegistry.Instance.VesselCustomAttribute1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			FreightDataRegistry.Instance.VesselCustomAttribute2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			FreightDataRegistry.Instance.VesselCustomAttribute3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Attribute3Caption");
			FreightDataRegistry.Instance.VesselCustomDecimal1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Decimal1Caption");
			FreightDataRegistry.Instance.VesselCustomFlag1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			RefVessel vessel = Factory.New<RefVessel>();
			using (RefVesselTestForm testForm = new RefVesselTestForm(vessel))
			{
				testForm.Show();

				AssertEquals("CustomAttribute1TextBox.Visible", false, testForm.GetCustomAttribute1TextBox().Visible);
				AssertEquals("CustomAttribute2TextBox.Visible", false, testForm.GetCustomAttribute2TextBox().Visible);
				AssertEquals("CustomAttribute3TextBox.Visible", true, testForm.GetCustomAttribute3TextBox().Visible);
				AssertEquals("CustomFlag1CheckBox.Visible", false, testForm.GetCustomFlag1CheckBox().Visible);
				AssertEquals("CustomDecimal1CalcEdit.Visible", true, testForm.GetCustomDecimal1CalcEdit().Visible);
			}
		}

		public void TestScreeningLogsTabPage()
		{
			DataRegistry.Instance.UserEventTrackingEnterprise = true;

			RefVessel vessel = Factory.New<RefVessel>();
			using (RefVesselTestForm form = new RefVesselTestForm(vessel))
			{
				form.Show();
				ZTabControl tabControl = (ZTabControl)form.LogsTabPage_Exposed.Controls[0].Controls[0];
				AssertEquals(3, tabControl.TabPages.Count);
				AssertEquals("Denied Party Screening Logs", tabControl.TabPages[2].Text);
				AssertEquals(typeof(RelatedDeniedPartyScreeningStatusControl), tabControl.TabPages[2].Controls[0].GetType());
			}
		}

		public void TestSwitchToDpsLogsTab()
		{
			var vessel = Factory.New<RefVessel>();
			using (var form = new RefVesselTestForm(vessel))
			{
				form.Show();
				AssertNotEquals("Logs", form.MainTabControl_Exposed.SelectedTab.Text);

				form.SwitchToDpsLogsTab();
				AssertEquals("Logs", form.MainTabControl_Exposed.SelectedTab.Text);
				AssertEquals("Denied Party Screening Logs", ((ZTabControl)form.LogsTabPage_Exposed.Controls[0].Controls[0]).SelectedTab.Text);
			}
		}

		[RequiresSTA]
		public void TestRadioCallSignOverrideLabelVisibility()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			RefVessel customVessel = Factory.New<RefVessel>();
			var overriddenVessel = Factory.NewWithValidTestData<RefVesselZZ>();
			overriddenVessel.ZZO_RadioCallSign = "1234";
			Factory.Save();

			RefVessel overrideVessel = Factory.NewWithValidTestData<RefVessel>();
			overrideVessel.RV_Code = overriddenVessel.ZZO_Code;
			overrideVessel.RV_RadioCallSign = "1234";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				using (RefVesselTestForm form = new RefVesselTestForm(customVessel))
				{
					form.Show();
					AssertEquals("RadioCallSignOverrideLabel.Visible", false, form.GetRadioCallSignOverrideLabel().Visible);
				}
				using (RefVesselTestForm form = new RefVesselTestForm(overrideVessel))
				{
					form.Show();
					AssertEquals("RadioCallSignOverrideLabel.Visible", false, form.GetRadioCallSignOverrideLabel().Visible);
				}

				overrideVessel.RV_RadioCallSign = "4321";
				using (RefVesselTestForm form = new RefVesselTestForm(overrideVessel))
				{
					form.Show();
					AssertEquals("RadioCallSignOverrideLabel.Visible", true, form.GetRadioCallSignOverrideLabel().Visible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (RefVesselTestForm form = new RefVesselTestForm(customVessel))
				{
					form.Show();
					AssertEquals("RadioCallSignOverrideLabel.Visible", false, form.GetRadioCallSignOverrideLabel().Visible);
				}
				using (RefVesselTestForm form = new RefVesselTestForm(overrideVessel))
				{
					form.Show();
					AssertEquals("RadioCallSignOverrideLabel.Visible", false, form.GetRadioCallSignOverrideLabel().Visible);
				}
			}
		}

		public void TestScreeningStatus_ColorChanges_BasedOnStatus()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			using (var form = new RefVesselTestForm(vessel))
			{
				form.Show();

				AssertCodeBoxAndDescriptionAreThisColor("Pre-condition", form.GetScreeningStatusForTest(), DeniedPartyConstants.GridColor.NotScreened);

				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertCodeBoxAndDescriptionAreThisColor("Should be cleared (green)", form.GetScreeningStatusForTest(), DeniedPartyConstants.GridColor.Clear);

				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertCodeBoxAndDescriptionAreThisColor("Should be unknown (orange)", form.GetScreeningStatusForTest(), DeniedPartyConstants.GridColor.Unknown);

				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertCodeBoxAndDescriptionAreThisColor("Should be matched (red)", form.GetScreeningStatusForTest(), DeniedPartyConstants.GridColor.Matched);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefVesselForm)GetFormToBashCore())
			{
				AssertNotNull("RefVesselForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		#region Implementation

		void AssertCodeBoxAndDescriptionAreThisColor(string message, ZDropEdit status, Color color)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Description box color should be:", color, status.DescriptionBox.BackColor);
				AssertEquals("Code box color should be:", color, status.DescriptionBox.BackColor);
			});
		}

		protected override Form GetFormToBashCore()
		{
			return new RefVesselForm(Factory.New<RefVessel>());
		}

		class RefVesselTestForm : RefVesselForm
		{
			public RefVesselTestForm(RefVessel bO)
				: base(bO)
			{
			}

			public ZArchitecture.ZTextBox GetCustomAttribute2TextBox()
			{
				return CustomAttribute2TextBox;
			}

			public ZArchitecture.ZTextBox GetCustomAttribute3TextBox()
			{
				return CustomAttribute3TextBox;
			}

			public ZArchitecture.ZTextBox GetCustomAttribute1TextBox()
			{
				return CustomAttribute1TextBox;
			}

			public ZArchitecture.ZCalcEdit GetCustomDecimal1CalcEdit()
			{
				return CustomDecimal1CalcEdit;
			}

			public ZCheckBox GetCustomFlag1CheckBox()
			{
				return CustomFlag1CheckBox;
			}

			public ZArchitecture.ZLabel GetRadioCallSignOverrideLabel()
			{
				return RadioCallSignOverrideLabel;
			}

			public ZDropEdit GetScreeningStatusForTest()
			{
				return ScreeningStatus;
			}

			public new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}

			public ZLogsTabPage LogsTabPage_Exposed
			{
				get { return LogsTabPage; }
			}

			public ZTemplateTabControl MainTabControl_Exposed
			{
				get { return MainTabControl; }
			}
		}

		#endregion
	}
}
