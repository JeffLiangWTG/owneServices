using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(BookingContentTabControlTestingForm))]
	internal class BookingContentTabControlBasher : ZFormBasherTest
	{
		public void TestTabsVisibility()
		{
			var shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			using (var form = new BookingContentTabControlTestingForm(shipment))
			{
				form.Show();
				Action<ZTabPage[]> assertTabsVisibility = (expectedVisibleTabs) =>
				{
					var allTabPages = new[] { form.ContainersTab, form.PackLinesTab, form.TopLevelPacksTab, form.VehiclesTab };
					AssertEquals("All expected tabs are visible", true, expectedVisibleTabs.All(tab => tab.TabVisible));
					AssertEquals("Other tabs should be hidden", true, allTabPages.Except(expectedVisibleTabs).All(tab => !tab.TabVisible));
					AssertEquals("The first tab should be selected", 0, form.Control.SelectedIndex);
				};
				assertTabsVisibility(new[] { form.ContainersTab, form.PackLinesTab });
				shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
				assertTabsVisibility(new[] { form.TopLevelPacksTab });
				shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
				assertTabsVisibility(new[] { form.TopLevelPacksTab });
				shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
				assertTabsVisibility(new[] { form.VehiclesTab });
				shipment.JS_PackingMode = Constants.ContainerModes.FCL;
				assertTabsVisibility(new[] { form.ContainersTab, form.PackLinesTab });
			}
		}

		public void TestCustomFieldsTabVisible()
		{
			var shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			using (var form = new BookingContentTabControlTestingForm(shipment))
			{
				form.Show();
				var customfieldsTab = form.CustomFieldsTab;
				AssertNotNull(customfieldsTab);
				AssertEquals("should be shown", true, customfieldsTab.TabVisible);

				var customFieldsControl = form.GetControl<ProcessTemplateCustomFieldsControl>("CustomFieldsControl");
				AssertNotNull(customFieldsControl);
			}
		}

		public void TestTabPageBindings()
		{
			var shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			using (var form = new BookingContentTabControlTestingForm(shipment))
			{
				form.Show();
				Action<ZTabPage, bool> assertIsBound = (tabPage, shouldBeBound) =>
				{
					string message = string.Format("{0} {1} be bound", tabPage.Name, shouldBeBound ? "should" : "should not yet");
					AssertEquals(message, shouldBeBound, ((IDataBoundControl)tabPage.Controls[0]).DataSource != null);
				};
				assertIsBound(form.ContainersTab, true);
				assertIsBound(form.PackLinesTab, false);
				assertIsBound(form.TopLevelPacksTab, false);
				assertIsBound(form.VehiclesTab, false);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
				assertIsBound(form.TopLevelPacksTab, true);
				assertIsBound(form.VehiclesTab, false);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
				assertIsBound(form.VehiclesTab, true);
			}
		}

		public void TestTopLevelPacksGridColumns()
		{
			var expectedContainerColumns = new[] { JobContainerSchema.Constants.JC_ContainerNum, JobContainerSchema.Constants.JC_ContainerCount, JobContainerSchema.Constants.JC_F3_NKPackType, JobContainerSchema.Constants.JC_TotalUnitOfMeasure, JobContainerSchema.Constants.JC_GrossWeight, JobContainerSchema.Constants.JC_GrossWeightUQ, JobContainerSchema.Constants.JC_GrossVolume, JobContainerSchema.Constants.JC_GrossVolumeUQ, JobContainerSchema.Constants.JC_RH_NKContainerCommodityCode, JobContainerSchema.Constants.JC_Description, JobContainerSchema.Constants.JC_TotalLength, JobContainerSchema.Constants.JC_TotalWidth, JobContainerSchema.Constants.JC_TotalHeight, JobContainerSchema.Constants.JC_MarksAndNumbers, JobContainerSchema.Constants.JC_GoodsValue, JobContainerSchema.Constants.JC_RX_NKGoodsCurrency, JobContainerSchema.Constants.JC_HarmonisedCode };
			var shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			using (var form = new BookingContentTabControlTestingForm(shipment))
			{
				form.Show();
				Application.DoEvents();
				var topLevelPacksGrid = (ZGrid)form.TopLevelPacksTab.Controls.Find("TopLevelPacksGrid", true)[0];
				var actualContainerColumns = new List<string>();
				foreach (var column in topLevelPacksGrid.Columns.Where(c => !c.ColumnName.StartsWith("UNDGs+")))
				{
					actualContainerColumns.Add(column.ColumnName);
				}

				AssertContainsExactElementsInAnyOrder("Container columns expected", expectedContainerColumns, actualContainerColumns);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			return new BookingContentTabControlTestingForm(shipment);
		}

		protected override void BashControl(Control controlToBash)
		{
			BookingContentTabControl control = controlToBash as BookingContentTabControl;
			if (control != null)
			{
				var form = controlToBash.Parent as BookingContentTabControlTestingForm;
				control.UpdateVisibleTabsForPackingMode(Constants.ContainerModes.FCL);
				BashControl(form.ContainersTab);
				BashControl(form.PackLinesTab);
				control.UpdateVisibleTabsForPackingMode(Constants.ContainerModes.FCL);
				BashControl(form.VehiclesTab);
				control.UpdateVisibleTabsForPackingMode(Constants.ContainerModes.BreakBulk);
				BashControl(form.TopLevelPacksTab);
			}

			base.BashControl(controlToBash);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		#region BookingContentTabControlTestingForm

		class BookingContentTabControlTestingForm : ZForm
		{
			public BookingContentTabControlTestingForm(AgencyBooking shipment) : base(shipment)
			{
				ControllerID = ControllerIDs.AgencyBooking;
				shipment.JS_PackingModeInfo.ValueChanged += new EventHandler(JS_PackingModeInfo_ValueChanged);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					Shipment.JS_PackingModeInfo.ValueChanged -= new EventHandler(JS_PackingModeInfo_ValueChanged);
				}

				base.Dispose(disposing);
			}

			void JS_PackingModeInfo_ValueChanged(object sender, EventArgs e)
			{
				Control.UpdateVisibleTabsForPackingMode(Shipment.JS_PackingMode);
			}

			protected override void InitializeComponent()
			{
				Control = new BookingContentTabControl();
				Control.Name = "Control";
				Control.Dock = DockStyle.Fill;
				Controls.Add(Control);
				Size = ControlDpiScalingHelper.NewScaledSize(1000, 400, true);
				Control.UpdateVisibleTabsForPackingMode(Shipment.JS_PackingMode);
				ContainersTab = FindControl<ZTabPage>("ContainersTab");
				PackLinesTab = FindControl<ZTabPage>("PackLinesTab");
				TopLevelPacksTab = FindControl<ZTabPage>("TopLevelPacksTab");
				VehiclesTab = FindControl<ZTabPage>("VehiclesTab");
				CustomFieldsTab = FindControl<ZTabPage>("CustomFieldsTab");
				this.CaptionRenderingEnabled = true;
			}

			T FindControl<T>(string controlName)
			{
				return (T)typeof(BookingContentTabControl).GetField(controlName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Control);
			}

			public AgencyBooking Shipment
			{
				get
				{
					return (AgencyBooking)BusinessEntity;
				}
			}

			public BookingContentTabControl Control;
			public ZTabPage ContainersTab;
			public ZTabPage PackLinesTab;
			public ZTabPage TopLevelPacksTab;
			public ZTabPage VehiclesTab;
			public ZTabPage CustomFieldsTab;
		}

		#endregion

		#endregion
	}
}
