namespace Enterprise.Customs.NL.NCTS.GUI
{
	partial class Phase5DepartureDetailsUserControl
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
			this.CalCalculationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DateLimitAndCalculationUserControl = new Enterprise.Customs.NL.NCTS.GUI.DateLimitAndCalculationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CalCalculationMethodDropEdit.SuspendLayout();
			this.DateLimitAndCalculationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.NCTS.Business.NctsHeader);
			// 
			// CalCalculationMethodDropEdit
			// 
			this.CalCalculationMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CalCalculationMethodDropEdit, "CALCalculationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).CALCalculationMethod)));
			this.CalCalculationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 9, true);
			this.CalCalculationMethodDropEdit.Name = "CalCalculationMethodDropEdit";
			this.CalCalculationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
			this.CalCalculationMethodDropEdit.TabIndex = 0;
			// 
			// DateLimitAndCalculationUserControl
			// 
			this.DateLimitAndCalculationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DateLimitAndCalculationUserControl, ".");
			this.DateLimitAndCalculationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 32, true);
			this.DateLimitAndCalculationUserControl.Name = "DateLimitAndCalculationUserControl";
			this.DateLimitAndCalculationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 18, true);
			this.DateLimitAndCalculationUserControl.TabIndex = 1;
			// 
			// Phase5DepartureDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DateLimitAndCalculationUserControl);
			this.Controls.Add(this.CalCalculationMethodDropEdit);
			this.Name = "Phase5DepartureDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 65, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CalCalculationMethodDropEdit.ResumeLayout(true);
			this.CalCalculationMethodDropEdit.PerformLayout();
			this.DateLimitAndCalculationUserControl.ResumeLayout(true);
			this.DateLimitAndCalculationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CalCalculationMethodDropEdit;
		internal DateLimitAndCalculationUserControl DateLimitAndCalculationUserControl;
	}
}
