using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ConsolDashboardFormTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestBusyIndicatorProviderIsRegistered()
		{
			var dashboard = new ConsolDashboard(Factory);
			using (var form = new ConsolDashboardForm(dashboard))
			{
				AssertEquals(true, Factory.GetValue<IBusyIndicatorProvider>() is BusyIndicatorProvider);
			}
		}

		public void TestModuleButtonGrids_AttachDetachAreEnabled_NewEditAreDisabled()
		{
			var dashboard = new ConsolDashboard(Factory);
			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentModuleGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				AssertEquals("New MUST BE disabled", false, shipmentModuleGrid.ShowNewButton);
				AssertEquals("Edit MUST BE disabled", false, shipmentModuleGrid.ShowEditButton);
				AssertEquals("Attach should be enabled", true, shipmentModuleGrid.ShowAttachButton);
				AssertEquals("Detach should be enabled", true, shipmentModuleGrid.ShowDetachButton);

				var consolModuleGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				AssertEquals("New MUST BE disabled", false, consolModuleGrid.ShowNewButton);
				AssertEquals("Edit MUST BE disabled", false, consolModuleGrid.ShowEditButton);
				AssertEquals("Attach should be enabled", true, consolModuleGrid.ShowAttachButton);
				AssertEquals("Detach should be enabled", true, consolModuleGrid.ShowDetachButton);
			}
		}

		[RequiresSTA]
		public void TestCurrentConsolShipmentModuleButtonGrid_AllButtonsAreDisabled()
		{
			var dashboard = new ConsolDashboard(Factory);
			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var moduleGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				AssertEquals("New should be disabled", false, moduleGrid.ShowNewButton);
				AssertEquals("Edit should be disabled", false, moduleGrid.ShowEditButton);
				AssertEquals("Attach should be disabled", false, moduleGrid.ShowAttachButton);
				AssertEquals("Detach should be disabled", false, moduleGrid.ShowDetachButton);
			}
		}

		public void TestSaveButtonsAreDisabledByDefault()
		{
			var dashboard = new ConsolDashboard(Factory);
			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var saveButton = FindControl<ZButton>(form, "SaveButton");
				AssertEquals("Save button should be disabled", false, saveButton.Enabled);

				var saveAndCloseButton = FindControl<ZButton>(form, "SaveAndCloseButton");
				AssertEquals("Save And Close button should be disabled", false, saveAndCloseButton.Enabled);
			}
		}

		public void TestShowSubsCheckBox()
		{
			var masterShipment = CreateShipment("Master shipment");
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var subShipment1 = CreateShipment("Sub-shipment1");
			var subShipment2 = CreateShipment("Sub-shipment2");

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = CreateConsol("consol1");
			consol.Shipments.Add(masterShipment);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var showSubsCheckBox = FindControl<ZCheckBox>(form, "ShowSubsCheckBox");
				AssertEquals("ShowSubs checkbox should be enabled", true, showSubsCheckBox.Enabled);
				AssertEquals("ShowSubs checkbox should be visible", true, showSubsCheckBox.Visible);
				AssertEquals("ShowSubs checkbox should be readonly by default", true, showSubsCheckBox.ReadOnly);

				dashboard.Consols.Add(consol);
				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				AssertEquals("ShowSubs checkbox should not be readonly when consol is selected", false, showSubsCheckBox.ReadOnly);

				var consolShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				showSubsCheckBox.Checked = false;
				AssertEquals("Sub-shipments are not shown in the grid", 1, consolShipmentsGrid.InnerGrid.VisibleRowCount);
				showSubsCheckBox.Checked = true;
				AssertEquals("Sub-shipments are shown in the grid", 3, consolShipmentsGrid.InnerGrid.VisibleRowCount);
			}
		}

		[RequiresSTA]
		public void TestRemoveButtonsAreDisabledWhenDashboardHasUnsavedChanges()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			var consol1 = CreateConsol("consol1");
			var consol2 = CreateConsol("consol2");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { shipment1 });
			dashboard.Consols.AddRange(new[] { consol1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				ToolStripItem findModuleGridButton(ZModuleButtonGrid moduleGrid)
				{
					var toolStrip = moduleGrid.Controls.Find("toolStrip", true).FirstOrDefault() as ZToolStrip;
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).FirstOrDefault();
				}

				AssertEquals("Prerequisite: Planning Board has unsaved changes", true, dashboard.HasUnsavedChanges);

				var shipmentsRemoveButton = findModuleGridButton(shipmentsGrid);
				AssertEquals("Remove button is disabled", false, shipmentsRemoveButton.Enabled);

				var consolsRemoveButton = findModuleGridButton(consolsGrid);
				AssertEquals("Remove button is disabled", false, consolsRemoveButton.Enabled);

				dashboard.Shipments.AddRange(new[] { shipment2 });
				AssertEquals("Remove button is still disabled", false, shipmentsRemoveButton.Enabled);
				AssertEquals("Remove button is still disabled", false, consolsRemoveButton.Enabled);

				dashboard.Shipments.AddRange(new[] { shipment2 });
				AssertEquals("Remove button is still disabled", false, shipmentsRemoveButton.Enabled);
				AssertEquals("Remove button is still disabled", false, consolsRemoveButton.Enabled);

				UnitTestUserNotification.Instance.AddOKAnswer();

				var saveButton = FindControl<ZButton>(form, "SaveButton");
				saveButton.PerformClick();

				AssertEquals("Prerequisite: Planning Board has no unsaved changes", false, dashboard.HasUnsavedChanges);
				AssertEquals("Remove button is re-enabled", true, shipmentsRemoveButton.Enabled);
				AssertEquals("Remove button is re-enabled", true, consolsRemoveButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestEditForms()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			var consol1 = CreateConsol("consol1");
			consol1.Shipments.AddRange(new[] { shipment1 });

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });
			dashboard.Shipments.AddRange(new[] { shipment2 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ShipmentModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var consolShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");

				void assertEditFormWasShown(ZModuleButtonGrid moduleGrid, Type expectedFormType)
				{
					moduleGrid.InnerGrid.Select(0);
					moduleGrid.InnerGrid.PerformDoubleClickForTest();

					AssertEquals("Correct form was shown", expectedFormType, ZFormModaliser.LastFormShownDialogForTest.GetType());
				}

				assertEditFormWasShown(shipmentsGrid, typeof(ShipmentForm));
				assertEditFormWasShown(consolsGrid, typeof(ConsolForm));
				assertEditFormWasShown(shipmentsGrid, typeof(ShipmentForm));

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				UnitTestUserNotification.Instance.AddOKAnswer();
				shipmentsGrid.InnerGrid.PerformDoubleClickForTest();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Please save Planning Board to be able to open Edit form", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateNewConsol()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var createNewConsolButton = FindControl<ZButton>(form, "CreateNewConsolButton");

				UnitTestUserNotification.Instance.AddOKAnswer();
				createNewConsolButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Please select shipment(s) first", UnitTestUserNotification.Instance.LastMessage.Text);

				var shipmentsGrid = FindControl<ShipmentModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				createNewConsolButton.PerformClick();
				AssertEquals("Consol form was shown", true, ZFormModaliser.LastFormShownDialogForTest is ConsolForm);
			}
		}

		public void TestAttach()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");
			var shipment3 = CreateShipment("s3");

			var consol1 = CreateConsol("consol1");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2, shipment3 });
			dashboard.Consols.AddRange(new[] { consol1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);
				shipmentsGrid.InnerGrid.Select(1);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Shipments have been attached",
					new[] { shipment1.PK, shipment2.PK },
					consol1.Shipments.Select(shipment => shipment.PK));

				var saveButton = FindControl<ZButton>(form, "SaveButton");
				AssertEquals("Save button should be new enabled", true, saveButton.Enabled);
			}
		}

		public void TestAttach_CheckConsolWithContainerWithOverrideGrossWeight_ShowWarning_AnswerNo()
		{
			var shipment1 = CreateShipment("s1");
			var consol1 = CreateConsol("consol1");

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "con1";
			container.JC_IsGrossWeightOverridden = true;

			var outerpacklines = shipment1.OuterPackLines.AddNew();
			outerpacklines.JL_PackageCount = 5;

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { shipment1 });
			dashboard.Consols.AddRange(new[] { consol1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				attachButton.PerformClick();
				AssertStartsWith("Warning prompt for weight override", "Attaching Shipment s1. Gross Weight of Container", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, container.JC_IsGrossWeightOverridden);
			}
		}

		public void TestAttach_CheckConsolWithContainerWithOverrideGrossWeight_ShowWarning_AnswerYes()
		{
			var shipment1 = CreateShipment("s1");
			var consol1 = CreateConsol("consol1");

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "con1";
			container.JC_IsGrossWeightOverridden = true;

			var outerpacklines = shipment1.OuterPackLines.AddNew();
			outerpacklines.JL_PackageCount = 5;

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { shipment1 });
			dashboard.Consols.AddRange(new[] { consol1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				attachButton.PerformClick();
				AssertStartsWith("Warning prompt for weight override", "Attaching Shipment s1. Gross Weight of Container", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(true, container.JC_IsGrossWeightOverridden);
			}
		}

		public void TestAttach_ChecksComplianceRisk_WhenConsolIsRisky()
		{
			AssertCheckComplianceRiskWhenConsolIsRisky(ComplianceRiskStatusCodeList.Codes.Blocked, ComplianceRiskStatusCodeList.Codes.Clear);
			AssertCheckComplianceRiskWhenConsolIsRisky(ComplianceRiskStatusCodeList.Codes.OverrideClear, ComplianceRiskStatusCodeList.Codes.Clear);
		}

		public void TestAttach_ChecksComplianceRisk_WhenShipmentIsRisky()
		{
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.OverrideClear, ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertCheckComplianceRiskWhenShipmentIsRisky(ComplianceRiskStatusCodeList.Codes.Clear, ComplianceRiskStatusCodeList.Codes.OverrideClear);
		}

		public void TestDetachButton_Click_After_AttachButton_Click()
		{
			var shipment1 = CreateShipment("s1");
			var packline = shipment1.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;

			var consol1 = CreateConsol("consol1");
			var oldShipmentETD = shipment1.JS_E_DEP;
			AssertNotEquals("Precondition: shipment ETD should be different to consol to test with updated ETD", shipment1.JS_E_DEP, consol1.Transports[0].JW_ETD);

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "ASDF1231231";

			var jobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader1.JH_JobNum = "Job1";
			jobHeader1.JH_ParentTableCode = "JS";
			jobHeader1.JH_ParentID = shipment1.PK;

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.Add(shipment1);
			dashboard.Consols.Add(consol1);

			AssertEquals("Precondition: Shipment.HasChanges should be false", false, shipment1.HasChanges);

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var initialLogs = new List<StmALog>();
				shipment1.Logs.GetAllLogs().CopyToList(initialLogs);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();
				AssertEquals("Shipment has been attached", shipment1.PK, consol1.Shipments.FirstOrDefault()?.PK);

				var consolShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				consolShipmentsGrid.InnerGrid.SelectAllElements();

				var detachButton = FindControl<ZButton>(form, "DetachButton");
				detachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Shipments have been detached", false, consol1.Shipments.Any());

				shipmentsGrid.InnerGrid.Select(0);
				attachButton.PerformClick();
				AssertEquals("Shipment has been attached", shipment1.PK, consol1.Shipments.FirstOrDefault()?.PK);

				var saveButton = FindControl<ZButton>(form, "SaveButton");
				saveButton.PerformClick();
				UnitTestUserNotification.Instance.ClearMessages();

				consolShipmentsGrid.InnerGrid.SelectAllElements();
				detachButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Shipments have been detached", false, consol1.Shipments.Any());

				shipmentsGrid.InnerGrid.Select(0);
				attachButton.PerformClick();
				AssertEquals("Shipment has been attached", shipment1.PK, consol1.Shipments.FirstOrDefault()?.PK);

				saveButton.PerformClick();

				var otherFactory = new BusinessObjectFactory();
				var shipmentInOtherFactory = otherFactory.Load<CommonShipment>(shipment1.PK);

				AssertEquals("Shipment ETD should not have changed", oldShipmentETD, shipment1.JS_E_DEP);
				AssertEquals("There should be no detached logs saved", 0, shipmentInOtherFactory.Logs.Find(x => x.SL_SE_NKEvent == Events.DetachedCode).Count());
				AssertEquals("There should be no attached log saved", 0, shipmentInOtherFactory.Logs.Find(x => x.SL_SE_NKEvent == Events.AttachedCode).Count());
			}
		}

		public void TestVolumeAndWeightPercentageBarAfterAttaching()
		{
			var shipment1 = CreateShipment("shipment1");
			shipment1.JS_ActualVolume = 50m;
			shipment1.JS_ActualWeight = 100m;

			var shipment2 = CreateShipment("shipment2");
			shipment2.JS_ActualVolume = 100m;
			shipment2.JS_ActualWeight = 50m;

			var consol1 = CreateConsol("consol1");
			consol1.JK_TotalShipmentActVolumeCheck = 200m;
			consol1.JK_TotalShipmentActWeightCheck = 200m;
			consol1.Shipments.Add(shipment1);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				dashboard.Shipments.Add(shipment2);
				dashboard.Consols.Add(consol1);

				var weightUtilisationPercentageBar = FindControl<LabelledPercentageBar>(form, "WeightUtilisationPercentageBar");
				var volumeUtilisationPercentageBar = FindControl<LabelledPercentageBar>(form, "VolumeUtilisationPercentageBar");
				var progressBarForWeight = FindControl<KProgressBar>(weightUtilisationPercentageBar, "ProgressBar");
				var progressBarForVolume = FindControl<KProgressBar>(volumeUtilisationPercentageBar, "ProgressBar");

				AssertEquals("The volume percentage bar should be 25% before attaching", 25m, consol1.Density.VolumeUtilisationPercentage);
				AssertEquals("The weight percentage bar should be 50% before attaching", 50m, consol1.Density.WeightUtilisationPercentage);
				AssertEquals(50, progressBarForWeight.Value);
				AssertEquals(25, progressBarForVolume.Value);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);
				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Shipments have been attached",
					new[] { shipment1.PK, shipment2.PK },
					consol1.Shipments.Select(shipment => shipment.PK));

				AssertEquals("The volume percentage bar should be 75% after attaching shipment", 75m, consol1.Density.VolumeUtilisationPercentage);
				AssertEquals("The weight percentage bar should be 75% after attaching shipment", 75m, consol1.Density.WeightUtilisationPercentage);
				AssertEquals(75, progressBarForWeight.Value);
				AssertEquals(75, progressBarForVolume.Value);
			}
		}

		[RequiresSTA]
		public void TestDetach()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			var consol1 = CreateConsol("consol1");
			consol1.Shipments.AddRange(new[] { shipment1, shipment2 });

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var consolShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				consolShipmentsGrid.InnerGrid.SelectAllElements();

				var detachButton = FindControl<ZButton>(form, "DetachButton");
				detachButton.PerformClick();

				AssertEquals("Shipments have been detached", false, consol1.Shipments.Any());

				CombineAssertions("Save buttons should now be enabled; Close button should become Cancel", () =>
				{
					AssertEquals("Save", true, FindControl<ZButton>(form, "SaveButton").Enabled);
					AssertEquals("Save and Close", true, FindControl<ZButton>(form, "SaveAndCloseButton").Enabled);
					AssertEquals("Cancel", "Cancel", FindControl<ZButton>(form, "CancelAndCloseButton").CaptionResourceString.Caption);
				});
			}
		}

		public void TestVolumeAndWeightPercentageBarAfterDetaching()
		{
			var shipment1 = CreateShipment("shipment1");
			shipment1.JS_ActualVolume = 50m;
			shipment1.JS_ActualWeight = 100m;
			var shipment2 = CreateShipment("shipment2");
			shipment2.JS_ActualVolume = 100m;
			shipment2.JS_ActualWeight = 50m;

			var consol1 = CreateConsol("consol1");
			consol1.JK_TotalShipmentActVolumeCheck = 200m;
			consol1.JK_TotalShipmentActWeightCheck = 200m;
			consol1.Shipments.AddRange(new[] { shipment1, shipment2 });

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				dashboard.Consols.Add(consol1);

				var weightUtilisationPercentageBar = FindControl<LabelledPercentageBar>(form, "WeightUtilisationPercentageBar");
				var volumeUtilisationPercentageBar = FindControl<LabelledPercentageBar>(form, "VolumeUtilisationPercentageBar");
				var progressBarForWeight = FindControl<KProgressBar>(weightUtilisationPercentageBar, "ProgressBar");
				var progressBarForVolume = FindControl<KProgressBar>(volumeUtilisationPercentageBar, "ProgressBar");

				AssertEquals("The volume percentage bar should be 75% before detaching", 75m, consol1.Density.VolumeUtilisationPercentage);
				AssertEquals("The weight percentage bar should be 75% before detaching", 75m, consol1.Density.WeightUtilisationPercentage);
				AssertEquals(75, progressBarForWeight.Value);
				AssertEquals(75, progressBarForVolume.Value);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var consolShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				consolShipmentsGrid.InnerGrid.Select(0);

				var detachButton = FindControl<ZButton>(form, "DetachButton");
				detachButton.PerformClick();

				AssertEquals("Shipment1 has been detached", false, consol1.Shipments.Contains(shipment1));
				AssertEquals("The volume percentage bar should be 50% after detaching shipment", 50m, consol1.Density.VolumeUtilisationPercentage);
				AssertEquals("The weight percentage bar should be 25% after detaching shipment", 25m, consol1.Density.WeightUtilisationPercentage);
				AssertEquals(25, progressBarForWeight.Value);
				AssertEquals(50, progressBarForVolume.Value);
			}
		}

		[RequiresSTA]
		public void TestCancelAndClose()
		{
			var shipment1 = CreateShipment("s1");
			var consol1 = CreateConsol("consol1");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });
			dashboard.Shipments.AddRange(new[] { shipment1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				form.Close();
				AssertEquals("Should close form without changes - no questions asked", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				bool formWasClosed = false;
				form.FormClosed += (s, e) => { formWasClosed = true; };

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.Close();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Planning Board has unsaved changes. Do you want to cancel them and close the form?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form was not closed", false, formWasClosed);

				var cancelButton = FindControl<ZButton>(form, "CancelAndCloseButton");
				cancelButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				cancelButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Planning Board has unsaved changes. Do you want to cancel them and close the form?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form was not closed", false, formWasClosed);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				cancelButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Planning Board has unsaved changes. Do you want to cancel them and close the form?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form was closed", true, formWasClosed);
			}
		}

		public void TestSave()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");
			var shipment3 = CreateShipment("s3");

			var consol1 = CreateConsol("consol1");
			var consol2 = CreateConsol("consol2");
			var consol3 = CreateConsol("consol3");

			consol3.Shipments.Add(shipment3);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1, consol2, consol3 });
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var attachButton = FindControl<ZButton>(form, "AttachButton");

				CombineAssertions("Attach shipment1 => consol1", () =>
				{
					consolsGrid.InnerGrid.Select(0);
					shipmentsGrid.InnerGrid.Select(0);
					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, consol1.Shipments);
				});

				var saveButton = FindControl<ZButton>(form, "SaveButton");

				AssertEquals("Prerequisite: Planning Board has unsaved changes", true, dashboard.HasUnsavedChanges);
				AssertEquals("Prerequisite: save button is enabled", true, saveButton.Enabled);

				UnitTestUserNotification.Instance.AddOKAnswer();
				saveButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				var expectedMessage =
@"The following consol(s) have been successfully saved:

Consol consol1 (Master Bill='consol1')";

				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd());

				AssertEquals("Planning Board should not have unsaved changes", false, dashboard.HasUnsavedChanges);
				AssertEquals("Save button should be disabled", false, saveButton.Enabled);

				CombineAssertions("Attach shipment2 => consol2", () =>
				{
					consolsGrid.InnerGrid.UnSelectAll();
					consolsGrid.InnerGrid.Select(1);

					shipmentsGrid.InnerGrid.UnSelectAll();
					shipmentsGrid.InnerGrid.Select(0);

					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, consol2.Shipments);
				});

				AssertEquals("Prerequisite: Planning Board has unsaved changes", true, dashboard.HasUnsavedChanges);
				AssertEquals("Prerequisite: save button is enabled", true, saveButton.Enabled);

				UnitTestUserNotification.Instance.AddOKAnswer();
				saveButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				expectedMessage =
@"The following consol(s) have been successfully saved:

Consol consol2 (Master Bill='consol2')";

				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd());

				AssertEquals("Planning Board should not have unsaved changes", false, dashboard.HasUnsavedChanges);
				AssertEquals("Save button should be disabled", false, saveButton.Enabled);

				AssertContainsExactElementsInAnyOrder("Consol have been saved",
					new[] { shipment1.PK },
					new BusinessObjectFactory().Load<ForwardingConsol>(consol1.PK).Shipments.GetPKs());

				AssertContainsExactElementsInAnyOrder("Consol have been saved",
					new[] { shipment2.PK },
					new BusinessObjectFactory().Load<ForwardingConsol>(consol2.PK).Shipments.GetPKs());

				AssertContainsExactElementsInAnyOrder("Consol should remain unchanged",
					new[] { shipment3.PK },
					new BusinessObjectFactory().Load<ForwardingConsol>(consol3.PK).Shipments.GetPKs());
			}
		}

		public void TestSaveAndClose_AllSaved()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");
			var shipment3 = CreateShipment("s3");

			var consol1 = CreateConsol("consol1");
			var consol2 = CreateConsol("consol2");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1, consol2 });
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2, shipment3 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var attachButton = FindControl<ZButton>(form, "AttachButton");

				CombineAssertions("Attach shipment1 => consol1", () =>
				{
					consolsGrid.InnerGrid.Select(0);
					shipmentsGrid.InnerGrid.Select(0);
					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, consol1.Shipments);
				});

				CombineAssertions("Attach shipment2, shipment3 => consol2", () =>
				{
					consolsGrid.InnerGrid.UnSelectAll();
					consolsGrid.InnerGrid.Select(1);

					shipmentsGrid.InnerGrid.UnSelectAll();
					shipmentsGrid.InnerGrid.Select(0);
					shipmentsGrid.InnerGrid.Select(1);

					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, consol2.Shipments);
				});

				var saveAndCloseButton = FindControl<ZButton>(form, "SaveAndCloseButton");

				AssertEquals("Prerequisite: Planning Board has unsaved changes", true, dashboard.HasUnsavedChanges);
				AssertEquals("Prerequisite: save and close button is enabled", true, saveAndCloseButton.Enabled);

				UnitTestUserNotification.Instance.AddOKAnswer();
				saveAndCloseButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				var expectedMessage =
@"The following consol(s) have been successfully saved:

Consol consol1 (Master Bill='consol1')
Consol consol2 (Master Bill='consol2')";

				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd());

				AssertContainsExactElementsInAnyOrder("Consol have been saved",
					new[] { shipment1.PK },
					new BusinessObjectFactory().Load<ForwardingConsol>(consol1.PK).Shipments.GetPKs());

				AssertContainsExactElementsInAnyOrder("Consol have been saved",
					new[] { shipment2.PK, shipment3.PK },
					new BusinessObjectFactory().Load<ForwardingConsol>(consol2.PK).Shipments.GetPKs());

				AssertEquals("Planning Board should not have unsaved changes", false, dashboard.HasUnsavedChanges);
				AssertEquals("Save and Close button should be disabled", false, saveAndCloseButton.Enabled);
			}
		}

		public void TestSave_ValidationErrors_UserHaveOptionToCorrectThemManually()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			shipment2.JS_GoodsDescription = "Injecting a blatantly invalid value";
			shipment2.JS_RL_NKOrigin = "#FUUU";

			var consol1 = CreateConsol("consol1");
			var consol2 = CreateConsol("consol2");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1, consol2 });
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var attachButton = FindControl<ZButton>(form, "AttachButton");

				CombineAssertions("Attach shipment1 => consol1", () =>
				{
					consolsGrid.InnerGrid.Select(0);
					shipmentsGrid.InnerGrid.Select(0);
					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, consol1.Shipments);
				});

				CombineAssertions("Attach shipment2, shipment3 => consol2", () =>
				{
					consolsGrid.InnerGrid.UnSelectAll();
					consolsGrid.InnerGrid.Select(1);

					shipmentsGrid.InnerGrid.UnSelectAll();
					shipmentsGrid.InnerGrid.Select(0);

					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, consol2.Shipments);
				});

				var saveButton = FindControl<ZButton>(form, "SaveButton");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				saveButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				var expectedMessage =
@"The following consol(s) have been successfully saved:

Consol consol1 (Master Bill='consol1')

The following consol(s) have not been saved due to validation/concurrency errors:

Consol consol2 (Master Bill='consol2') - Error - JS_RL_NKOrigin: Enter a valid Origin.

Do you want the system to open unsaved consol(s) one by one so you can correct the errors manually?

Select 'Yes' if you want the system to open unsaved consol(s) one by one
Select 'No'  if you want to leave it 'as is'; consol(s) in question would not be saved";

				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Consol form was shown", true, ZFormModaliser.LastFormShownDialogForTest is ConsolForm);
			}
		}

		public void TestSave_ValidationErrors_UserHaveOptionToCorrectThemManuallyWithMultiErrors()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			shipment2.JS_GoodsDescription = "Injecting a blatantly invalid value";
			shipment2.JS_RL_NKOrigin = "#FUUU";
			shipment2.JS_RL_NKDestination = "#FUUU";

			var consol1 = CreateConsol("consol1");
			var consol2 = CreateConsol("consol2");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1, consol2 });
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var attachButton = FindControl<ZButton>(form, "AttachButton");

				CombineAssertions("Attach shipment1 => consol1", () =>
				{
					consolsGrid.InnerGrid.Select(0);
					shipmentsGrid.InnerGrid.Select(0);
					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, consol1.Shipments);
				});

				CombineAssertions("Attach shipment2, shipment3 => consol2", () =>
				{
					consolsGrid.InnerGrid.UnSelectAll();
					consolsGrid.InnerGrid.Select(1);

					shipmentsGrid.InnerGrid.UnSelectAll();
					shipmentsGrid.InnerGrid.Select(0);

					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, consol2.Shipments);
				});

				var saveButton = FindControl<ZButton>(form, "SaveButton");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				saveButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				var expectedMessage =
@"The following consol(s) have been successfully saved:

Consol consol1 (Master Bill='consol1')

The following consol(s) have not been saved due to validation/concurrency errors:

Consol consol2 (Master Bill='consol2')
 - Error - JS_RL_NKDestination: Enter a valid Destination.
 - Error - JS_RL_NKOrigin: Enter a valid Origin.

Do you want the system to open unsaved consol(s) one by one so you can correct the errors manually?

Select 'Yes' if you want the system to open unsaved consol(s) one by one
Select 'No'  if you want to leave it 'as is'; consol(s) in question would not be saved";

				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Consol form was shown", true, ZFormModaliser.LastFormShownDialogForTest is ConsolForm);
			}
		}

		public void TestSave_ValidationErrors_PreAllocatedValues()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Weight.Action = PreAllocationCheck.Actions.Restriction;
			checks.Weight.Percentage = 90m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			var shipment1 = CreateShipment("s1");
			shipment1.JS_ActualWeight = 2000m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var consol1 = CreateConsol("Consol1");
			consol1.JK_TotalShipmentActWeightCheck = 2000m;
			consol1.WeightVerificationUnit = Core.Constants.Weight.Kilograms;

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });
			dashboard.Shipments.AddRange(new[] { shipment1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var attachButton = FindControl<ZButton>(form, "AttachButton");

				CombineAssertions("Attach shipment1 => consol1", () =>
				{
					consolsGrid.InnerGrid.Select(0);
					shipmentsGrid.InnerGrid.Select(0);
					attachButton.PerformClick();

					AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, consol1.Shipments);
				});

				var saveButton = FindControl<ZButton>(form, "SaveButton");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				saveButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				var expectedMessage =
@"The following consol(s) have not been saved due to validation/concurrency errors:

Consol Consol1 (Master Bill='Consol1') - Pre-allocated values exceed the registry specified percentage.

Do you want the system to open unsaved consol(s) one by one so you can correct the errors manually?

Select 'Yes' if you want the system to open unsaved consol(s) one by one
Select 'No'  if you want to leave it 'as is'; consol(s) in question would not be saved";

				AssertMultilineASCIIEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Consol form was shown", true, ZFormModaliser.LastFormShownDialogForTest is ConsolForm);
			}
		}

		public void TestSave_ConcurrentChanges_ConflictsAreShownInDialog_And_UserHaveOptionToCorrectThemManually()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			var consol1 = CreateConsol("consol1");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });
			dashboard.Shipments.AddRange(new[] { shipment1 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				var consolReloadedInAnotherFactory = anotherFactory.Load<ForwardingConsol>(consol1.PK);
				consolReloadedInAnotherFactory.Shipments.AddFromDatabase(shipment2.PK);

				anotherFactory.Save();

				var saveButton = FindControl<ZButton>(form, "SaveButton");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				saveButton.PerformClick();

				var firstDialogMessage = UnitTestUserNotification.Instance.PreviousMessages[1];
				AssertEquals(true, firstDialogMessage.WasQuestion);

				var expectedMessage =
@"The following consol(s) have not been saved due to validation/concurrency errors:

Consol consol1 (Master Bill='consol1')

Do you want the system to open unsaved consol(s) one by one so you can correct the errors manually?

Select 'Yes' if you want the system to open unsaved consol(s) one by one
Select 'No'  if you want to leave it 'as is'; consol(s) in question would not be saved";

				AssertMultilineASCIIEquals(expectedMessage, firstDialogMessage.Text);

				var lastDialogMessage = UnitTestUserNotification.Instance.PreviousMessages[0];
				var expectedErrorMessage =
@"Please resolve 2 Shipments with conflicts for Consol consol1 (Master Bill='consol1'):
Shipment s1 - Attached by current user
Shipment s2 - Attached by another user";

				AssertMultilineASCIIEquals(expectedErrorMessage, lastDialogMessage.Text);

				AssertEquals("Consol form was shown", true, ZFormModaliser.LastFormShownDialogForTest is ConsolForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		#region View Measurements

		public void TestViewMeasurementMenuItemExists()
		{
			var dashboard = new ConsolDashboard(Factory);
			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var consolsShipmentGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");

				AssertNotNull(shipmentsGrid.InnerGrid.ContextMenu.MenuItems.FindByText("View Measurements"));
				AssertNotNull(consolsShipmentGrid.InnerGrid.ContextMenu.MenuItems.FindByText("View Measurements"));
			}
		}

		public void TestViewMeasurementForm()
		{
			var shipment1 = CreateShipment("s1");
			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = "CTN";
			packline1.JL_ActualWeight = 500.1;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 6;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1;
			packline1.JL_Width = 2;
			packline1.JL_Height = 3;
			packline1.JL_UnitOfDimension = "M";

			var shipment2 = CreateShipment("s2");
			var packline2 = shipment2.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 2;
			packline2.JL_F3_NKPackType = "CTN";
			packline2.JL_ActualWeight = 300.3;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 420;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 5;
			packline2.JL_Width = 6;
			packline2.JL_Height = 7;
			packline2.JL_UnitOfDimension = "M";

			var consol1 = CreateConsol("consol1");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });
			dashboard.Shipments.AddRange(new[] { shipment1, shipment2 });

			using (var form = new ConsolDashboardForm(dashboard))
			using (ZFormModaliser.SuspendDispose())
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var consolsShipmentGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");

				var viewMeasurementMenuItem = shipmentsGrid.InnerGrid.ContextMenu.MenuItems.FindByText("View Measurements");
				var viewConsolsShipmentMeasurementMenuItem = consolsShipmentGrid.InnerGrid.ContextMenu.MenuItems.FindByText("View Measurements");

				shipmentsGrid.InnerGrid.Select(0);
				consolsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewMeasurementMenuItem.PerformClick();
				AssertEquals("Please select a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewConsolsShipmentMeasurementMenuItem.PerformClick();
				AssertEquals("Please select a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				shipmentsGrid.InnerGrid.Select(0);
				viewMeasurementMenuItem.PerformClick();

				var lastShipmentForm = ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(typeof(ShipmentViewMeasurementsForm), lastShipmentForm?.GetType());

				var shipmentPopup = lastShipmentForm.GetField("TextBox") as RichTextBox;
				var expectedMessage = @"s2
     2 CTN                       5.00 x 6.00 x 7.00 M     300.30 KG     420.000 M3
Totals: 2 CTN                                              300.30 KG     420.000 M3";
				AssertMultilineASCIIEquals(expectedMessage, shipmentPopup?.Text ?? string.Empty);
				var shipmentCloseButton = lastShipmentForm.GetField("CloseButton") as ZButton;
				shipmentCloseButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consolsShipmentGrid.InnerGrid.Select(0);
				viewConsolsShipmentMeasurementMenuItem.PerformClick();

				var lastConsolForm = ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(typeof(ShipmentViewMeasurementsForm), lastConsolForm?.GetType());

				var consolShipmentPopup = lastConsolForm.GetField("TextBox") as RichTextBox;
				var expectedConsolShipmentsMessage = @"s1
     1 CTN                       1.00 x 2.00 x 3.00 M     500.10 KG       6.000 M3
Totals: 1 CTN                                              500.10 KG       6.000 M3";
				AssertMultilineASCIIEquals(expectedConsolShipmentsMessage, consolShipmentPopup?.Text ?? string.Empty);
			}
		}

		#endregion

		[RequiresSTA]
		public void TestDatesOutsideRange()
		{
			var today = ZDateTime.Today;

			var shipment = CreateShipment("shipment1");
			shipment.JS_E_ARV = today;

			var consol = CreateConsol("consol1");
			consol.Transports.MostInterestingTransport.JW_ETA = today.AddDays(10);

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol });
			dashboard.Shipments.AddRange(new[] { shipment });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				var question = @"None There's inconsistency between ETD/ETA of Shipments and Consols you are trying to link.
See details below:
Shipment shipment1 has estimated arrival date before Consol consol1 ETA.

How would you like to proceed?

Press [Yes] to attach and update the Shipments dates to match the Consols dates.
Press [No] to attach Shipments but do not update Shipments dates.
Press [Cancel] to cancel operation.";
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[RequiresSTA]
		public void TestAllowDoubleClickOverrides()
		{
			var shipment1 = CreateShipment("s1");
			var shipment2 = CreateShipment("s2");

			var consol1 = CreateConsol("consol1");
			consol1.Shipments.AddRange(new[] { shipment1 });

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });
			dashboard.Shipments.AddRange(new[] { shipment2 });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ShipmentModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				var consolShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");

				void assertCoreDoubleClickDoesNothing(ZModuleButtonGrid moduleGrid)
				{
					moduleGrid.InnerGrid.Select(0);
					moduleGrid.InnerGrid.GetSelectedElements<BusinessObject>().ForEach(t => t.HasChanges = true);
					moduleGrid.InnerGrid.PerformMouseDownForTest(0, 2);
					AssertNull("Core HasChanges error message not shown", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				assertCoreDoubleClickDoesNothing(shipmentsGrid);
				assertCoreDoubleClickDoesNothing(consolsGrid);
				assertCoreDoubleClickDoesNothing(consolShipmentsGrid);
			}
		}

		#region Attach/Detach Master/Lead Sub

		public void TestAttach_MasterShipment()
		{
			var masterShipment = CreateShipment("Master shipment");
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var subShipment1 = CreateShipment("Sub-shipment 1");
			var subShipment2 = CreateShipment("Sub-shipment 2");
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = CreateConsol("consol1");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { masterShipment });
			dashboard.Consols.AddRange(new[] { consol });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Shipments have been attached",
					new[] { masterShipment.PK, subShipment1.PK, subShipment2.PK },
					consol.Shipments.Select(shipment => shipment.PK));

				var saveButton = FindControl<ZButton>(form, "SaveButton");
				AssertEquals("Save button should be new enabled", true, saveButton.Enabled);

				saveButton.PerformClick();

				var otherFactory = new BusinessObjectFactory();
				var consolInOtherFactory = otherFactory.Load<ForwardingConsol>(consol.PK);
				AssertContainsExactElementsInAnyOrder("Attached shipments have been saved",
					new[] { masterShipment.PK, subShipment1.PK, subShipment2.PK },
					consolInOtherFactory.Shipments.Select(shipment => shipment.PK));
			}
		}

		public void TestAttach_SubShipment()
		{
			var masterShipment = CreateShipment("Master shipment");
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var subShipment1 = CreateShipment("Sub-shipment1");
			var subShipment2 = CreateShipment("Sub-shipment2");
			var standaloneShipment = CreateShipment("Standalone shipment");

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = CreateConsol("consol1");

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Shipments.AddRange(new[] { subShipment1, subShipment2, standaloneShipment });
			dashboard.Consols.AddRange(new[] { consol });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);
				shipmentsGrid.InnerGrid.Select(1);
				shipmentsGrid.InnerGrid.Select(2);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				AssertMultilineASCIIEquals(@"The shipments that you are trying to attach are sub-shipments:
Sub-shipment1 is a sub-shipment of master/lead Master shipment
Sub-shipment2 is a sub-shipment of master/lead Master shipment

Only these shipments, without their masters, will be attached to consol1 consol.

If you would like to attach these shipments, their masters/leads and all sub-shipments of their masters/leads to this consol, you need to attach the master/lead shipments to this consol instead.

Press [Yes] if you would like to continue.
Press [No] if you would like to skip the above listed shipments and attach all other selected shipments.
Press [Cancel] to cancel operation.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertContainsExactElementsInAnyOrder("Only standalone shipment has been attached",
					new[] { standaloneShipment.PK },
					consol.Shipments.Select(shipment => shipment.PK));

				shipmentsGrid.InnerGrid.Select(0);
				shipmentsGrid.InnerGrid.Select(1);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				attachButton.PerformClick();
				AssertContainsExactElementsInAnyOrder("Sub-shipments have been attached",
					new[] { subShipment1.PK, subShipment2.PK, standaloneShipment.PK },
					consol.Shipments.Select(shipment => shipment.PK));
			}
		}

		public void TestDetach_SubShipment()
		{
			var masterShipment = CreateShipment("Master shipment");
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var subShipment = CreateShipment("Sub-shipment");
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = CreateConsol("consol1");
			consol.Shipments.Add(masterShipment);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var dashboard = new ConsolDashboard(otherFactory);
			var consolInOtherFactory = otherFactory.Load<ForwardingConsol>(consol.PK);
			dashboard.Consols.AddRange(new[] { consolInOtherFactory });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var showSubsCheckBox = FindControl<ZCheckBox>(form, "ShowSubsCheckBox");
				showSubsCheckBox.Checked = true;

				var currentShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				currentShipmentsGrid.InnerGrid.Select(1);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var detachButton = FindControl<ZButton>(form, "DetachButton");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				detachButton.PerformClick();

				AssertEquals("Cannot detach the shipment Sub-shipment from the consol consol1 as the shipment is a sub-shipment of the master-shipment Master shipment that also belongs to the consol. If you would like to detach Sub-shipment, then you must detach the shipment from the master-shipment first.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertContainsExactElementsInAnyOrder("Sub-shipments have not been detached",
					new[] { masterShipment.PK, subShipment.PK },
					consol.Shipments.Select(shipment => shipment.PK));
			}
		}

		public void TestDetach_MasterShipment_WithSubShipments()
		{
			var masterShipment = CreateShipment("Master shipment");
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var subShipment1 = CreateShipment("Sub-shipment1");
			var subShipment2 = CreateShipment("Sub-shipment2");

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = CreateConsol("consol1");
			consol.Shipments.Add(masterShipment);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var currentShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				currentShipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var detachButton = FindControl<ZButton>(form, "DetachButton");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				detachButton.PerformClick();

				var warningDialog = UnitTestUserNotification.Instance.PreviousMessages[1];

				AssertMultilineASCIIEquals(@"The shipment Master shipment is a master / lead shipment of Sub-shipment1, Sub-shipment2.

Do you also want to detach the sub-shipments from the consol consol1?

Press [Yes] to detach the master / lead shipment and sub-shipments from the consol.
Press [No] to detach the master / lead shipment from the consol, but keep the sub-shipments attached to the consol.
Press [Cancel] to cancel the operation.", warningDialog.Text);

				var questionDialog = UnitTestUserNotification.Instance.PreviousMessages[0];
				AssertEquals("Do you want to show detached sub-shipments in the top left shipments grid?", questionDialog.Text);

				AssertEquals("Master shipment and its sub-shipments have been detached", 0, consol.Shipments.Count);
				AssertContainsExactElementsInAnyOrder("Only master shipment has been returned back to the available shipments grid",
					new[] { masterShipment.PK },
					dashboard.Shipments.Select(shipment => shipment.PK));

				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();

				currentShipmentsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				detachButton.PerformClick();

				AssertEquals("Master shipment and its sub-shipments have been detached", 0, consol.Shipments.Count);
				AssertContainsExactElementsInAnyOrder("Master shipment and its sub-shipments have been returned back to the available shipments grid",
					new[] { masterShipment.PK, subShipment1.PK, subShipment2.PK },
					dashboard.Shipments.Select(shipment => shipment.PK));
			}
		}

		[RequiresSTA]
		public void TestDetach_MasterShipment_WithoutSubShipments()
		{
			var masterShipment = CreateShipment("Master shipment");
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			var subShipment1 = CreateShipment("Sub-shipment1");
			var subShipment2 = CreateShipment("Sub-shipment2");

			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = CreateConsol("consol1");
			consol.Shipments.Add(masterShipment);

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var currentShipmentsGrid = FindControl<ZModuleButtonGrid>(form, "CurrentConsolShipmentModuleButtonGrid");
				currentShipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				var detachButton = FindControl<ZButton>(form, "DetachButton");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				detachButton.PerformClick();

				AssertMultilineASCIIEquals(@"The shipment Master shipment is a master / lead shipment of Sub-shipment1, Sub-shipment2.

Do you also want to detach the sub-shipments from the consol consol1?

Press [Yes] to detach the master / lead shipment and sub-shipments from the consol.
Press [No] to detach the master / lead shipment from the consol, but keep the sub-shipments attached to the consol.
Press [Cancel] to cancel the operation.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertContainsExactElementsInAnyOrder("Sub-shipments are kept attached to the consol",
					new[] { subShipment1.PK, subShipment2.PK },
					consol.Shipments.Select(shipment => shipment.PK));
				AssertContainsExactElementsInAnyOrder("Master shipment has been returned back to the available shipments grid",
					new[] { masterShipment.PK },
					dashboard.Shipments.Select(shipment => shipment.PK));
			}
		}

		#endregion

		ForwardingShipment CreateShipmentForTestUpdateCalculation(string uniqueConsignRef)
		{
			var shipment = CreateShipment(uniqueConsignRef);
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "CTN";
			packLine.JL_ActualWeight = 3;
			packLine.JL_ActualWeightUQ = "KG";
			packLine.JL_Width = 4;
			packLine.JL_Length = 5;
			packLine.JL_Height = 6;
			packLine.JL_UnitOfDimension = "M";
			shipment.UpdateShipmentFromOuterPackLines();
			shipment.JS_ActualChargeable = 360;

			return shipment;
		}

		[RequiresSTA]
		public void TestUpdateCalculation()
		{
			var shipments = Enumerable.Range(1, 200).Select(x => CreateShipmentForTestUpdateCalculation("s" + x)).ToArray();

			var consol1 = CreateConsol("consol1");
			consol1.CalculationWrapper.MarkAsBound();

			Factory.Save();

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol1 });
			dashboard.Shipments.AddRange(shipments);

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();

				var mock = new Moq.Mock<ForwardingConsol>(Factory, ((IBusinessObjectFactoryInternals)Factory).RowFactory.LoadFromPK("JobConsol", consol1.PK));
				mock.CallBase = true;

				var mockConsol = mock.Object;
				dashboard.Shipments.AddRange(shipments);
				dashboard.Consols.Add(mockConsol);
				mock.Invocations.Clear();
				mock.VerifyGet(s => s.JK_CorrectedConsolVolume, Moq.Times.Exactly(0));

				dashboard.TryAttach(mockConsol, shipments.Take(30));

				mock.VerifyGet(s => s.JK_CorrectedConsolVolume, Moq.Times.Exactly(6));

				mock.Invocations.Clear();
				mock.VerifyGet(s => s.JK_CorrectedConsolVolume, Moq.Times.Exactly(0));

				dashboard.TryAttach(mockConsol, shipments.Skip(30).Take(60));

				mock.VerifyGet(s => s.JK_CorrectedConsolVolume, Moq.Times.Exactly(4));

				mock.Invocations.Clear();
				mock.VerifyGet(s => s.JK_CorrectedConsolVolume, Moq.Times.Exactly(0));

				dashboard.TryAttach(mockConsol, shipments.Skip(90));

				mock.VerifyGet(s => s.JK_CorrectedConsolVolume, Moq.Times.Exactly(4));
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		void AssertCheckComplianceRiskWhenShipmentIsRisky(ZString consolComplianceRiskType, ZString shipmentComplianceRiskType)
		{
			var consol = CreateConsol("consol1");
			var consolComplianceRisk = new ComplianceRiskPlugInBusinessObject(consol);
			consolComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = consolComplianceRiskType;

			var shipment = CreateShipment("shipment1");
			var shipmentComplianceRisk = new ComplianceRiskPlugInBusinessObject(shipment);
			shipmentComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = shipmentComplianceRiskType;

			var question = @"None Warning - Shipment (shipment1) has a Job Compliance Status of ‘Risk’ or ‘Override Clear’ within their Compliance Risk tab.
Attaching Shipment(s) with that Risk Status to a Consolidation may put other Shipments linked to the Consolidation at risk.

Are you sure you want to proceed?
Click No – to cancel the operation and review the shipment.
Click Yes – to attach the shipment and flag the Consol as ‘Risk’.";

			AssertAttachRaisesWarningMessage(shipment, consol, question);
		}

		void AssertCheckComplianceRiskWhenConsolIsRisky(ZString consolComplianceRiskType, ZString shipmentComplianceRiskType)
		{
			var shipment = CreateShipment("shipment1");
			var shipmentComplianceRisk = new ComplianceRiskPlugInBusinessObject(shipment);
			shipmentComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = shipmentComplianceRiskType;

			var consol = CreateConsol("consol1");
			var consolComplianceRisk = new ComplianceRiskPlugInBusinessObject(consol);
			consolComplianceRisk.ComplianceRiskStatus.COR_OverallRisk = consolComplianceRiskType;

			var question = @"None Warning - Consol consol1 has a Job Compliance Status of ‘Risk’ or ‘Override Clear’ within their Compliance Risk tab which may cause delays to the Shipment shipment1.

Are you sure you want to proceed?

Click No – to cancel the operation.
Click Yes – to attach the shipment.";

			AssertAttachRaisesWarningMessage(shipment, consol, question);
		}

		void AssertAttachRaisesWarningMessage(ForwardingShipment shipment, ForwardingConsol consol, string question)
		{
			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol });
			dashboard.Shipments.AddRange(new[] { shipment });

			using (var form = new ConsolDashboardForm(dashboard))
			{
				form.Show();
				var shipmentsGrid = FindControl<ZModuleButtonGrid>(form, "ShipmentModuleButtonGrid");
				shipmentsGrid.InnerGrid.Select(0);

				var consolsGrid = FindControl<ZModuleButtonGrid>(form, "ConsolModuleButtonGrid");
				consolsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var attachButton = FindControl<ZButton>(form, "AttachButton");
				attachButton.PerformClick();
				AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		TControl FindControl<TControl>(Control parentControl, string controlName) where TControl : Control
		{
			return parentControl.Controls.Find(controlName, true).OfType<TControl>().FirstOrDefault();
		}

		ForwardingShipment CreateShipment(string uniqueConsignRef = null)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			shipment.ConsignorPK = Consignor.PK;
			shipment.ConsigneePK = Consignee.PK;

			shipment.JS_UniqueConsignRef = uniqueConsignRef;

			shipment.RunPreSaveValidation();

			AssertEquals("Prerequisite: shipment should have no errors; please adjust setup if this test fails",
				"",
				shipment.GetErrors().ToUniqueMessageListString());

			return shipment;
		}

		ForwardingConsol CreateConsol(string uniqueConsignRef = null)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;

			consol.Transports[0].JW_VoyageFlight = "QF512";
			consol.Transports[0].JW_ETD = ZDateTime.Today;

			consol.JK_UniqueConsignRef = uniqueConsignRef;
			consol.JK_MasterBillNum = uniqueConsignRef;

			consol.RunPreSaveValidation();

			AssertEquals("Prerequisite: consol should have no errors; please adjust setup if this test fails",
				"",
				consol.GetErrors().ToUniqueMessageListString());

			return consol;
		}

		OrgHeader Consignor
		{
			get
			{
				if (consignor == null)
				{
					consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_FullName = "CONSIGNOR";
					consignor.MainAddress.OA_Address1 = "Consignor Address";
					consignor.OH_IsConsignor = true;
				}

				return consignor;
			}
		}
		OrgHeader consignor;

		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_FullName = "CONSIGNEE";
					consignee.MainAddress.OA_Address1 = "Consignee Address";
					consignee.OH_IsConsignee = true;
				}

				return consignee;
			}
		}
		OrgHeader consignee;

		#endregion
	}

	[TestedType(typeof(ConsolDashboardForm))]
	public class ConsolDashboardFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var dashboard = new ConsolDashboard(Factory);
			return new ConsolDashboardForm(dashboard);
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); // override to allow 1080p
		}

		#endregion
	}
}
