namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class LiquidationGroupNotificationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.SuppressNoChangeLiquidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LiquidationGroupNotificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendGroupGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SendModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LiquidationGroupNotificationGroupBox.SuspendLayout();
			this.SendGroupGuidFindBox.SuspendLayout();
			this.SendModeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.LiquidationGroupNotification);
			// 
			// SuppressNoChangeLiquidationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SuppressNoChangeLiquidationCheckBox, "SuppressNoChangeLiquidations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.DataRegistry.Business.LiquidationGroupNotification)(null)).SuppressNoChangeLiquidations)));
			this.SuppressNoChangeLiquidationCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("51c22d89-46b3-4f59-adc4-835783aea372", "Suppress No Change Liquidation");
			this.SuppressNoChangeLiquidationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SuppressNoChangeLiquidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 70, true);
			this.SuppressNoChangeLiquidationCheckBox.Name = "SuppressNoChangeLiquidationCheckBox";
			this.SuppressNoChangeLiquidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 24, true);
			this.SuppressNoChangeLiquidationCheckBox.TabIndex = 3;
			this.SuppressNoChangeLiquidationCheckBox.UseVisualStyleBackColor = true;
			// 
			// LiquidationGroupNotificationGroupBox
			// 
			this.LiquidationGroupNotificationGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("21fe20fe-b0f3-4db0-879e-0c1e4657ed0c", "Send Notifications");
			this.LiquidationGroupNotificationGroupBox.Controls.Add(this.SendGroupGuidFindBox);
			this.LiquidationGroupNotificationGroupBox.Controls.Add(this.SendModeDropEdit);
			this.LiquidationGroupNotificationGroupBox.Controls.Add(this.SuppressNoChangeLiquidationCheckBox);
			this.LiquidationGroupNotificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LiquidationGroupNotificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LiquidationGroupNotificationGroupBox.Name = "LiquidationGroupNotificationGroupBox";
			this.LiquidationGroupNotificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 100, true);
			this.LiquidationGroupNotificationGroupBox.TabIndex = 0;
			this.LiquidationGroupNotificationGroupBox.TabStop = false;
			// 
			// SendGroupGuidFindBox
			// 
			this.SendGroupGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendGroupGuidFindBox, "SendGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.DataRegistry.Business.LiquidationGroupNotification)(null)).SendGroupPK)));
			this.SendGroupGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c962948e-dba8-41f2-9b40-a10c8eacae71", "Send Group");
			this.SendGroupGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 45, true);
			this.SendGroupGuidFindBox.Name = "SendGroupGuidFindBox";
			this.SendGroupGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SendGroupGuidFindBox.ParentType = null;
			this.SendGroupGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.SendGroupGuidFindBox.TabIndex = 2;
			// 
			// SendModeDropEdit
			// 
			this.SendModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SendModeDropEdit, "SendMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.DataRegistry.Business.LiquidationGroupNotification)(null)).SendMode)));
			this.SendModeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4b127996-7546-43bc-b42e-a3fc6b191ecb", "Send Mode");
			this.SendModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 19, true);
			this.SendModeDropEdit.Name = "SendModeDropEdit";
			this.SendModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.SendModeDropEdit.TabIndex = 1;
			// 
			// LiquidationGroupNotificationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LiquidationGroupNotificationGroupBox);
			this.Name = "LiquidationGroupNotificationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LiquidationGroupNotificationGroupBox.ResumeLayout(false);
			this.LiquidationGroupNotificationGroupBox.PerformLayout();
			this.SendGroupGuidFindBox.ResumeLayout(true);
			this.SendGroupGuidFindBox.PerformLayout();
			this.SendModeDropEdit.ResumeLayout(true);
			this.SendModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox SuppressNoChangeLiquidationCheckBox;
		private ZArchitecture.GUI.ZGroupBox LiquidationGroupNotificationGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox SendGroupGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit SendModeDropEdit;
	}
}
