namespace Enterprise.Customs.NL.NCTS.GUI
{
	partial class DateLimitAndCalculationUserControl
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
			this.DateLimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CalCalculationMethodDropEdit.SuspendLayout();
			this.DateLimitDateEdit.SuspendLayout();
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
			this.CalCalculationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 0, true);
			this.CalCalculationMethodDropEdit.Name = "CalCalculationMethodDropEdit";
			this.CalCalculationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 15, true);
			this.CalCalculationMethodDropEdit.TabIndex = 1;
			// 
			// DateLimitDateEdit
			// 
			this.DateLimitDateEdit.AllowDrop = true;
			this.DateLimitDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateLimitDateEdit, "MovementHeader.BM_ExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_ExportDate)));
			this.DateLimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 0, true);
			this.DateLimitDateEdit.Name = "DateLimitDateEdit";
			this.DateLimitDateEdit.TabIndex = 12;
			// 
			// DateLimitAndCalculationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DateLimitDateEdit);
			this.Controls.Add(this.CalCalculationMethodDropEdit);
			this.Name = "DateLimitAndCalculationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 18, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CalCalculationMethodDropEdit.ResumeLayout(true);
			this.CalCalculationMethodDropEdit.PerformLayout();
			this.DateLimitDateEdit.ResumeLayout(true);
			this.DateLimitDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CalCalculationMethodDropEdit;
		internal ZArchitecture.GUI.ZDateEdit DateLimitDateEdit;
	}
}
