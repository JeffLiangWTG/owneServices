
namespace Enterprise.Workflow.GUI
{
	partial class EDIMessageContentFilterSharedAdditionalConfigurationUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.EDIMessageContentFilterSharedAdditionalConfiguration);
			//// 
			//// zCheckBox
			//// 
			this.zCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox, "ExcludeEmptyElements");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.EDIMessageContentFilterSharedAdditionalConfiguration)(null)).ExcludeEmptyElements)));
			this.zCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 10, true);
			this.zCheckBox.Name = "zCheckBox";
			this.zCheckBox.Text = Res.GetString("00A2D3A7-BDCB-4C6A-A49B-336358120BA3", "Exclude Empty Collections and Elements");
			this.zCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zCheckBox.TabIndex = 3;
			// 
			// EDIMessageContentFilterLineUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zCheckBox);
			this.Name = "EDIMessageContentFilterSharedAdditionalConfigurationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 325, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zCheckBox.ResumeLayout(true);
			this.zCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.GUI.ZCheckBox zCheckBox;
	}
}
