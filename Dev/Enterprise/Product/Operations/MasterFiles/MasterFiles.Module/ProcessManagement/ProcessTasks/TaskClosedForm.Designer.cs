using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Module
{
	public partial class TaskClosedForm
	{

		internal ZButton OKButton;
		internal ZButton NoButton;
		ZTimeEdit zTimeEdit1;
		ZTimeEdit zTimeEdit2;
		ZLabel label1;

		new void InitializeComponent()
		{
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zTimeEdit1 = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.zTimeEdit2 = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// label1
			// 
			this.label1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("TaskClosedForm|24901996-9d21-4b69-8131-e011dfca4dfc", "", "This task is currently working. Do you want to Close the task?\r\n\r\nThe system has calculated that you have taken the following amount of time to complete this task. Please confirm the duration, and if necessary enter an updated duration, and then press Yes.");
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 67, true);
			this.label1.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("TaskClosedForm|663ebc08-3209-4d62-bbc1-f56b0c422fbf", "Yes");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 129, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 6;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// NoButton
			// 
			this.NoButton.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("TaskClosedForm|6f69007b-026c-4c8a-b35a-d275b25f66f8", "No");
			this.NoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 129, true);
			this.NoButton.Name = "NoButton";
			this.NoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NoButton.TabIndex = 7;
			this.NoButton.UseVisualStyleBackColor = true;
			this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// zTimeEdit1
			// 
			this.zTimeEdit1.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTimeEdit1, "ElapsedDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ElapsedDuration)));
			this.zTimeEdit1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("TaskClosedForm|5babf232-9333-489d-b0ad-fc001470323e", "Calculated Elapsed Duration");
			this.zTimeEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 88, true);
			this.zTimeEdit1.Name = "zTimeEdit1";
			this.zTimeEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.zTimeEdit1.TabIndex = 3;
			// 
			// zTimeEdit2
			// 
			this.zTimeEdit2.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTimeEdit2, "OverrideActualDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).OverrideActualDuration)));
			this.zTimeEdit2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("TaskClosedForm|03ee989d-629c-40b8-8158-091141809ef7", "Actual Duration");
			this.zTimeEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 88, true);
			this.zTimeEdit2.Name = "zTimeEdit2";
			this.zTimeEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.zTimeEdit2.TabIndex = 5;
			// 
			// TaskClosedForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 182, true);
			this.Controls.Add(this.zTimeEdit2);
			this.Controls.Add(this.zTimeEdit1);
			this.Controls.Add(this.NoButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.label1);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.ProcessTask";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "TaskClosedForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.NoButton, 0);
			this.Controls.SetChildIndex(this.zTimeEdit1, 0);
			this.Controls.SetChildIndex(this.zTimeEdit2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
