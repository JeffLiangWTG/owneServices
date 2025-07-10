using Enterprise.ZArchitecture.GUI;
namespace Enterprise.MasterFiles.GUI
{
	partial class QuickAddressForm : ZChildForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		private CargoWise.Windows.UI.KFlowLayoutPanel FlowLayoutPanel;
		private Enterprise.ZArchitecture.GUI.ZButton OkButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelButtonX;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl NewAddressDocAddressControl;
		private ZDropEdit CartageAddressTypeDropEdit;

		new void InitializeComponent()
		{
			this.FlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewAddressDocAddressControl = new Enterprise.MasterFiles.GUI.QuickAddressFormZDocAddressControl();
			this.CartageAddressTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 248, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 0, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DocAddressCreatorHost);
			// 
			// FlowLayoutPanel
			// 
			this.FlowLayoutPanel.AutoSize = true;
			this.FlowLayoutPanel.Controls.Add(this.CancelButtonX);
			this.FlowLayoutPanel.Controls.Add(this.OkButton);
			this.FlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.FlowLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 8, 4);
			this.FlowLayoutPanel.Name = "FlowLayoutPanel";
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("QuickAddressForm|ac709abb-e98a-40c9-9664-8d9dbf31aacb", "OK");
			this.OkButton.Name = "OkButton";
			this.OkButton.AutoSize = true;
			this.OkButton.TabIndex = 2;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("QuickAddressForm|e35fbd17-a6ed-404f-82f1-3b056fb7cc88", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.AutoSize = true;
			this.CancelButtonX.TabIndex = 3;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// NewAddressDocAddressControl
			// 
			this.NewAddressDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewAddressDocAddressControl, "DocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.MasterFiles.Business.DocAddressCreatorHost)(null)).DocAddress)));
			this.NewAddressDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.NewAddressDocAddressControl.CaptionResourceString = null;
			this.NewAddressDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 29, true);
			this.NewAddressDocAddressControl.Name = "NewAddressDocAddressControl";
			this.NewAddressDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.NewAddressDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.NewAddressDocAddressControl.TabIndex = 1;
			// 
			// CartageAddressTypeDropEdit
			// 
			this.CartageAddressTypeDropEdit.AllowDrop = true;
			this.CartageAddressTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CartageAddressTypeDropEdit, "AddressTypeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.DocAddressCreatorHost)(null)).AddressTypeCode)));
			this.CartageAddressTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 8, true);
			this.CartageAddressTypeDropEdit.Name = "CartageAddressTypeDropEdit";
			this.CartageAddressTypeDropEdit.PreBoundMaxLength = 3;
			this.CartageAddressTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.CartageAddressTypeDropEdit.TabIndex = 0;
			// 
			// QuickAddressForm
			// 
			this.AcceptButton = this.OkButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 248, true);
			this.ControlBox = false;
			this.Controls.Add(this.CartageAddressTypeDropEdit);
			this.Controls.Add(this.NewAddressDocAddressControl);
			this.Controls.Add(this.FlowLayoutPanel);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.DocAddressCreatorHost);
			this.DataSourceTypeName = "Enterprise.Rating.Business.CustomConversionFactor";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 250, true);
			this.Name = "QuickAddressForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FlowLayoutPanel, 0);
			this.Controls.SetChildIndex(this.NewAddressDocAddressControl, 0);
			this.Controls.SetChildIndex(this.CartageAddressTypeDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
