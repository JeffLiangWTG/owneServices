namespace Enterprise.eTail.GUI
{
	partial class HVLVConsignmentACASUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ACASGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEditACASStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zSendACASAmendmentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ACASGroupBox.SuspendLayout();
			this.zDropEditACASStatus.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVConsignment);
			// 
			// ACASGroupBox
			// 
			this.ACASGroupBox.Controls.Add(this.zDropEditACASStatus);
			this.ACASGroupBox.Controls.Add(this.zSendACASAmendmentButton);
			this.ACASGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ACASGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ACASGroupBox.Name = "ACASGroupBox";
			this.ACASGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1634, 215, true);
			this.ACASGroupBox.TabIndex = 0;
			this.ACASGroupBox.TabStop = false;
			// 
			// zDropEditACASStatus
			// 
			this.zDropEditACASStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditACASStatus, "HVC_ACASStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVConsignment)(null)).HVC_ACASStatus)));
			this.zDropEditACASStatus.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("a32c20d2-4b9f-4301-bb47-e54852ade9d1", "ACAS Status");
			this.zDropEditACASStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 15, true);
			this.zDropEditACASStatus.Name = "zDropEditACASStatus";
			this.zDropEditACASStatus.PreBoundMaxLength = 2;
			this.zDropEditACASStatus.ShouldResizeByMaxLength = true;
			this.zDropEditACASStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.zDropEditACASStatus.TabIndex = 0;
			// 
			// zSendACASAmendmentButton
			// 
			this.zSendACASAmendmentButton.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("ea4c20af-66bf-4b62-bebb-6e8c2a326b70", "Send ACAS Amendment");
			this.zSendACASAmendmentButton.IsCaptionOverridden = false;
			this.zSendACASAmendmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 15, true);
			this.zSendACASAmendmentButton.Name = "zSendACASAmendmentButton";
			this.zSendACASAmendmentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zSendACASAmendmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.zSendACASAmendmentButton.TabIndex = 0;
			this.zSendACASAmendmentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zSendACASAmendmentButton.ToolTipCaption = null;
			this.zSendACASAmendmentButton.Click += new System.EventHandler(this.SendACASAmendment);
			// 
			// HVLVConsignmentACASUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ACASGroupBox);
			this.Name = "HVLVConsignmentACASUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1634, 215, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ACASGroupBox.ResumeLayout(false);
			this.ACASGroupBox.PerformLayout();
			this.zDropEditACASStatus.ResumeLayout(true);
			this.zDropEditACASStatus.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ACASGroupBox;
		private ZArchitecture.GUI.ZDropEdit zDropEditACASStatus;
		private ZArchitecture.GUI.ZButton zSendACASAmendmentButton;
	}
}
