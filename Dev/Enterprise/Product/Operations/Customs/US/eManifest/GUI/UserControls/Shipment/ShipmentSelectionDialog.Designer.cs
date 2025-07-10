using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public class ZTreeViewNoDoubleClick : ZTreeView
	{
		#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			if (m.Msg != 0x203)
			{
				base.WndProc(ref m);
			}
		}

		#endif
	}

	partial class ShipmentItemSelectionDialog
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.BottomButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectedItemCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShipmentTreeView = new Enterprise.Customs.US.eManifest.GUI.ZTreeViewNoDoubleClick();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.SelectedItemCountLabel);
			this.BottomButtonPanel.Controls.Add(this.ButtonCancel);
			this.BottomButtonPanel.Controls.Add(this.ButtonOK);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 37, true);
			this.BottomButtonPanel.TabIndex = 1;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 2, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 27, true);
			this.ButtonCancel.TabIndex = 3;
			this.ButtonCancel.Text = "Cancel";
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 2, true);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 27, true);
			this.ButtonOK.TabIndex = 2;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.ToolTipCaption = null;
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// SelectedItemCountLabel
			// 
			this.SelectedItemCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectedItemCountLabel.AutoSize = true;
			this.SelectedItemCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SelectedItemCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 10, true);
			this.SelectedItemCountLabel.Name = "SelectedItemCountLabel";
			this.SelectedItemCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.SelectedItemCountLabel.TabIndex = 2;
			// 
			// ShipmentTreeView
			// 
			this.ShipmentTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ShipmentTreeView.CheckBoxes = true;
			this.ShipmentTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.ShipmentTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentTreeView.Name = "ShipmentTreeView";
			this.ShipmentTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 228, true);
			this.ShipmentTreeView.TabIndex = 0;
			// 
			// ShipmentItemSelectionDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 267, true);
			this.Controls.Add(this.BottomButtonPanel);
			this.Controls.Add(this.ShipmentTreeView);
			this.Name = "ShipmentItemSelectionDialog";
			this.Controls.SetChildIndex(this.ShipmentTreeView, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZTreeViewNoDoubleClick ShipmentTreeView;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomButtonPanel;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonOK;
		private Enterprise.ZArchitecture.ZLabel SelectedItemCountLabel;
	}
}
