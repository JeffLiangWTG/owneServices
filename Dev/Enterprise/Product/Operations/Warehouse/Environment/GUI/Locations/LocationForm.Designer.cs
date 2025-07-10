using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI
{
	public partial class LocationForm
	{
		ZPanel oPanel1;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		LocationsEditControlSwitcher whsRowLocationsEditControl1;
		ZButton SortButton;

		protected override void InitializeComponent()
		{
			this.oPanel1 = new ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.SortButton = new ZButton();
			this.whsRowLocationsEditControl1 = new LocationsEditControlSwitcher();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.oPanel1.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.whsRowLocationsEditControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 364, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(723);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsRow);
			// 
			// oPanel1
			// 
			this.oPanel1.Controls.Add(this.PostingButtonsUserControl);
			this.oPanel1.Controls.Add(this.SortButton);
			this.oPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.oPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 335, true);
			this.oPanel1.Name = "oPanel1";
			this.oPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 29, true);
			this.oPanel1.TabIndex = 8;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 0, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 3;
			// 
			// SortButton
			// 
			this.SortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SortButton.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationForm|aed3537b-b793-47ee-a039-cebbe4c5ec31", "Sort by Location");
			this.SortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 5, true);
			this.SortButton.Name = "SortButton";
			this.SortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 21, true);
			this.SortButton.TabIndex = 2;
			this.SortButton.Click += new System.EventHandler(this.SortButton_Click);
			// 
			// whsRowLocationsEditControl1
			// 
			this.whsRowLocationsEditControl1.AllowDrop = true;
			this.whsRowLocationsEditControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.whsRowLocationsEditControl1, ".");
			this.whsRowLocationsEditControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.whsRowLocationsEditControl1.Name = "whsRowLocationsEditControl1";
			this.whsRowLocationsEditControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 324, true);
			this.whsRowLocationsEditControl1.TabIndex = 9;
			// 
			// LocationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("LocationForm|a26c81c6-ba3c-402f-9882-7caf49eb365d", "Locations");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 386, true);
			this.Controls.Add(this.whsRowLocationsEditControl1);
			this.Controls.Add(this.oPanel1);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Environment.Business";
			this.DataSourceType = typeof(WhsRow);
			this.DataSourceTypeName = "Enterprise.Warehouse.Environment.Business.WhsRow";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 316, true);
			this.Name = "LocationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.oPanel1, 0);
			this.Controls.SetChildIndex(this.whsRowLocationsEditControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.oPanel1.ResumeLayout(false);
			this.oPanel1.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.whsRowLocationsEditControl1.ResumeLayout(true);
			this.whsRowLocationsEditControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
