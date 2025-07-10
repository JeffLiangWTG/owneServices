namespace Enterprise.eTail.GUI
{
	partial class HVLVDpsClearingReasonForm
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
		private new void InitializeComponent()
		{
			this.clearingReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.clearingReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.saveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.clearingReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 164, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason);
			// 
			// clearingReasonDropEdit
			// 
			this.clearingReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.clearingReasonDropEdit, "ClearingReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason)(null)).ClearingReason)));
			this.clearingReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 25, true);
			this.clearingReasonDropEdit.Name = "clearingReasonDropEdit";
			this.clearingReasonDropEdit.PreBoundMaxLength = 3;
			this.clearingReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.clearingReasonDropEdit.TabIndex = 2;
			// 
			// clearingReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.clearingReasonTextBox, "ClearingReasonText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason)(null)).ClearingReasonText)));
			this.clearingReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.clearingReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 53, true);
			this.clearingReasonTextBox.Multiline = true;
			this.clearingReasonTextBox.Name = "clearingReasonTextBox";
			this.clearingReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.clearingReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 62, true);
			this.clearingReasonTextBox.TabIndex = 3;
			// 
			// saveButton
			// 
			this.saveButton.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("cffa0f64-6cd9-4033-844a-2111db5a60db", "&Save");
			this.saveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 126, true);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.saveButton.TabIndex = 4;
			this.saveButton.ToolTipCaption = null;
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("6f178886-fb23-494b-a87c-afd6df1bc0ba", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 126, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// HVLVDpsClearingReasonForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("efdd8c20-1876-4429-850a-9108db743fb5", "Clearing Reason");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 188, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.clearingReasonDropEdit);
			this.Controls.Add(this.clearingReasonTextBox);
			this.DataSourceType = typeof(Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "HVLVDpsClearingReasonForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.clearingReasonTextBox, 0);
			this.Controls.SetChildIndex(this.clearingReasonDropEdit, 0);
			this.Controls.SetChildIndex(this.saveButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.clearingReasonDropEdit.ResumeLayout(true);
			this.clearingReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit clearingReasonDropEdit;
		private ZArchitecture.ZTextBox clearingReasonTextBox;
		private ZArchitecture.GUI.ZButton saveButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
