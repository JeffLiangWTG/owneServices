using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class QuotationDateAndNumberControl
	{
		ZDateEdit QuotationEndDateEdit;
		ZDateEdit QuotationStartDateEdit;
		ZDropEdit CancellationReasonDropEdit;

		private void InitializeComponent()
		{
			this.QuotationEndDateEdit = new ZDateEdit();
			this.QuotationStartDateEdit = new ZDateEdit();
			this.CancellationReasonDropEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.QuotationEndDateEdit.SuspendLayout();
			this.QuotationStartDateEdit.SuspendLayout();
			this.CancellationReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Quote);
			// 
			// QuotationEndDateEdit
			// 
			this.QuotationEndDateEdit.AllowDrop = true;
			this.QuotationEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.QuotationEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QuotationEndDateEdit, "TH_QuoteEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.Quote)(null)).TH_QuoteEndDate);
			this.QuotationEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 0, true);
			this.QuotationEndDateEdit.Name = "QuotationEndDateEdit";
			this.QuotationEndDateEdit.TabIndex = 3;
			// 
			// QuotationStartDateEdit
			// 
			this.QuotationStartDateEdit.AllowDrop = true;
			this.QuotationStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.QuotationStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QuotationStartDateEdit, "TH_QuoteDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.Quote)(null)).TH_QuoteDate);
			this.QuotationStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.QuotationStartDateEdit.Name = "QuotationStartDateEdit";
			this.QuotationStartDateEdit.TabIndex = 1;
			// 
			// CancellationReasonDropEdit
			// 
			this.CancellationReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CancellationReasonDropEdit, "TH_QuoteCancellationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.Quote)(null)).TH_QuoteCancellationReason);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.Quote)(null)).QuoteCancellationReasonDescription);
			this.CancellationReasonDropEdit.BindToForDescription = "QuoteCancellationReasonDescription";
			this.CancellationReasonDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("149052bb-47ff-4df2-b829-5a766de329f0", "Cancellation Reason");
			this.CancellationReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.CancellationReasonDropEdit.Name = "CancellationReasonDropEdit";
			this.CancellationReasonDropEdit.PreBoundMaxLength = 3;
			this.CancellationReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.CancellationReasonDropEdit.TabIndex = 6;
			// 
			// QuotationDateAndNumberControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.QuotationEndDateEdit);
			this.Controls.Add(this.QuotationStartDateEdit);
			this.Controls.Add(this.CancellationReasonDropEdit);
			this.Name = "QuotationDateAndNumberControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.QuotationEndDateEdit.ResumeLayout(true);
			this.QuotationEndDateEdit.PerformLayout();
			this.QuotationStartDateEdit.ResumeLayout(true);
			this.QuotationStartDateEdit.PerformLayout();
			this.CancellationReasonDropEdit.ResumeLayout(true);
			this.CancellationReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
