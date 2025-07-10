namespace Enterprise.Customs.NO.GUI
{
	partial class InvoiceLayoutsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InvoiceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ValuationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExchangeRatePlusFixedRateUserControl = new Enterprise.Customs.NO.GUI.ExchangeRatePlusFixedRateUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.ValuationMethodDropEdit.SuspendLayout();
			this.ExchangeRatePlusFixedRateUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobComInvoiceHeader);
			// 
			// InvoiceDateDateEdit
			// 
			this.InvoiceDateDateEdit.AllowDrop = true;
			this.InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InvoiceDateDateEdit, "JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceHeader)(null)).JZ_InvoiceDate)));
			this.InvoiceDateDateEdit.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("A30765FD-9CE6-434B-9A65-242188EA14A1", "Invoice Date");
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDateDateEdit.Name = "InvoiceDateDateEdit";
			this.InvoiceDateDateEdit.TabIndex = 0;
			// 
			// ValuationMethodDropEdit
			// 
			this.ValuationMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationMethodDropEdit, "JZ_ValuationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceHeader)(null)).JZ_ValuationMethod)));
			this.ValuationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValuationMethodDropEdit.Name = "ValuationMethodDropEdit";
			this.ValuationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.ValuationMethodDropEdit.TabIndex = 2;
			// 
			// ExchangeRatePlusFixedRateUserControl
			// 
			this.ExchangeRatePlusFixedRateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRatePlusFixedRateUserControl, ".");
			this.ExchangeRatePlusFixedRateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExchangeRatePlusFixedRateUserControl.Name = "ExchangeRatePlusFixedRateUserControl";
			this.ExchangeRatePlusFixedRateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.ExchangeRatePlusFixedRateUserControl.TabIndex = 1;
			// 
			// InvoiceLayoutsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceDateDateEdit);
			this.Controls.Add(this.ValuationMethodDropEdit);
			this.Controls.Add(this.ExchangeRatePlusFixedRateUserControl);
			this.Name = "InvoiceLayoutsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.ValuationMethodDropEdit.ResumeLayout(true);
			this.ValuationMethodDropEdit.PerformLayout();
			this.ExchangeRatePlusFixedRateUserControl.ResumeLayout(true);
			this.ExchangeRatePlusFixedRateUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDateEdit InvoiceDateDateEdit;
		internal ZArchitecture.GUI.ZDropEdit ValuationMethodDropEdit;
		internal ExchangeRatePlusFixedRateUserControl ExchangeRatePlusFixedRateUserControl;
	}
}
