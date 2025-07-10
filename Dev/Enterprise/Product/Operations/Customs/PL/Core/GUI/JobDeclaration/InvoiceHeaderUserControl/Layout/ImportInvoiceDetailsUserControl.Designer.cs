namespace Enterprise.Customs.PL.GUI
{
	partial class ImportInvoiceDetailsUserControl
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
		private void InitializeComponent()
		{
			this.TranCircumstanceUserControl = new Enterprise.Customs.PL.GUI.TranCircumstancesUserControl();
			this.ValuationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TranCircumstanceUserControl.SuspendLayout();
			this.ValuationMethodDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader);
			// 
			// TranCircumstanceUserControl
			// 
			this.TranCircumstanceUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TranCircumstanceUserControl, ".");
			this.TranCircumstanceUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 3, true);
			this.TranCircumstanceUserControl.Name = "TranCircumstanceUserControl";
			this.TranCircumstanceUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 22, true);
			this.TranCircumstanceUserControl.TabIndex = 0;
			// 
			// ValuationMethodDropEdit
			// 
			this.ValuationMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationMethodDropEdit, "ZG_ValuationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(null)).ZG_ValuationMethod)));
			this.ValuationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 30, true);
			this.ValuationMethodDropEdit.Name = "ValuationMethodDropEdit";
			this.ValuationMethodDropEdit.ShouldResizeByMaxLength = false;
			this.ValuationMethodDropEdit.PreBoundMaxLength = 5;
			this.ValuationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 18, true);
			this.ValuationMethodDropEdit.TabIndex = 2;
			// 
			// ImportInvoiceDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TranCircumstanceUserControl);
			this.Controls.Add(this.ValuationMethodDropEdit);
			this.Name = "ImportInvoiceDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TranCircumstanceUserControl.ResumeLayout(true);
			this.TranCircumstanceUserControl.PerformLayout();
			this.ValuationMethodDropEdit.ResumeLayout(true);
			this.ValuationMethodDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal PL.GUI.TranCircumstancesUserControl TranCircumstanceUserControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ValuationMethodDropEdit;
	}
}
