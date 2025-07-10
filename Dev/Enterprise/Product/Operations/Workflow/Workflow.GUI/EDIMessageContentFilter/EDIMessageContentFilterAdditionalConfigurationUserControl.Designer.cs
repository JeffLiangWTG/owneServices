
namespace Enterprise.Workflow.GUI
{
	partial class EDIMessageContentFilterAdditionalConfigurationUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zTextBox2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTextBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.EDIMessageContentFilterAdditionalConfiguration);
			//// 
			//// zTextBox2
			//// 
			this.zTextBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zTextBox2, "AdditionalConfiguration.PrimaryDataSource");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.EDIMessageContentFilterAdditionalConfiguration)(null)).PrimaryDataSource)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.ShouldResizeByMaxLength = true;
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zTextBox2.TabIndex = 3;
			// 
			// EDIMessageContentFilterLineUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zTextBox2);
			this.Name = "EDIMessageContentFilterAdditionalConfigurationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 325, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTextBox2.ResumeLayout(true);
			this.zTextBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.GUI.ZDropEdit zTextBox2;
	}
}
