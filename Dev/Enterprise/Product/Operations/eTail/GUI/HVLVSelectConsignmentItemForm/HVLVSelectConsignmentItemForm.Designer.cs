namespace Enterprise.eTail.GUI
{
	abstract partial class HVLVSelectConsignmentItemForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.ButtonOk = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSelectAll = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonDeselectAll = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConsignmentTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.SuspendLayout();
			// 
			// ButtonOk
			//
			this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 390, true);
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonOk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.ButtonOk.TabIndex = 4;
			this.ButtonOk.Text = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("9b67f6c4-2370-4ead-ad10-522d534e2ca0", "OK");
			this.ButtonOk.ToolTipCaption = null;
			this.ButtonOk.UseVisualStyleBackColor = true;
			this.ButtonOk.Click += new System.EventHandler(this.ButtonClick_OK);
			// 
			// ButtonSelectAll
			//
			this.ButtonSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonSelectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 390, true);
			this.ButtonSelectAll.Name = "ButtonSelectAll";
			this.ButtonSelectAll.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSelectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.ButtonSelectAll.TabIndex = 2;
			this.ButtonSelectAll.Text = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("d0453d5a-45a5-42f2-89a6-10cb4bf0542a", "Select All");
			this.ButtonSelectAll.ToolTipCaption = null;
			this.ButtonSelectAll.UseVisualStyleBackColor = true;
			this.ButtonSelectAll.Click += new System.EventHandler(this.ButtonClick_SelectAll);
			// 
			// ButtonDeselectAll
			//
			this.ButtonDeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonDeselectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 390, true);
			this.ButtonDeselectAll.Name = "ButtonDeselectAll";
			this.ButtonDeselectAll.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonDeselectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.ButtonDeselectAll.TabIndex = 3;
			this.ButtonDeselectAll.Text = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("14e45c67-9952-457c-9591-e018aa90ac5c", "Deselect All");
			this.ButtonDeselectAll.ToolTipCaption = null;
			this.ButtonDeselectAll.UseVisualStyleBackColor = true;
			this.ButtonDeselectAll.Click += new System.EventHandler(this.ButtonClick_DeselectAll);
			// 
			// ConsignmentTreeView
			// 
			this.ConsignmentTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsignmentTreeView.CheckBoxes = true;
			this.ConsignmentTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.ConsignmentTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentTreeView.Name = "ConsignmentItemTreeView";
			this.ConsignmentTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 375, true);
			this.ConsignmentTreeView.TabIndex = 0;

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 450, true);
			this.Controls.Add(this.ConsignmentTreeView);
			this.Controls.Add(this.ButtonDeselectAll);
			this.Controls.Add(this.ButtonSelectAll);
			this.Controls.Add(this.ButtonOk);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Name = "HVLVSelectConsignmentItemForm";
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("ab5ff0c6-49e3-4518-aef6-97c2883596b4", "Select HVLV Items");
			this.Controls.SetChildIndex(this.ConsignmentTreeView, 0);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton ButtonOk;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonSelectAll;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonDeselectAll;
		private Enterprise.ZArchitecture.GUI.ZTreeView ConsignmentTreeView;
	}
}
