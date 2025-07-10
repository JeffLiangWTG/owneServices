namespace Enterprise.Customs.PL.GUI
{
	partial class CPCUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RequestedCustomsProcedureCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreviousCustomsProcedureCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RequestedCustomsProcedureCodeDropEdit.SuspendLayout();
			this.PreviousCustomsProcedureCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine);
			// 
			// RequestedCustomsProcedureCodeDropEdit
			// 
			this.RequestedCustomsProcedureCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestedCustomsProcedureCodeDropEdit, "ProcedureCodeBase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).ProcedureCodeBase)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RequestedCustomsProcedureCodeDropEdit, false);
			this.RequestedCustomsProcedureCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequestedCustomsProcedureCodeDropEdit.Name = "RequestedCustomsProcedureCodeDropEdit";
			this.RequestedCustomsProcedureCodeDropEdit.PreBoundMaxLength = 2;
			this.RequestedCustomsProcedureCodeDropEdit.ShowDescriptionBox = false;
			this.RequestedCustomsProcedureCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 18, true);
			this.RequestedCustomsProcedureCodeDropEdit.TabIndex = 118;
			// 
			// PreviousCustomsProcedureCodeDropEdit
			// 
			this.PreviousCustomsProcedureCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreviousCustomsProcedureCodeDropEdit, "PreviousProcedureCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).PreviousProcedureCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PreviousCustomsProcedureCodeDropEdit, false);
			this.PreviousCustomsProcedureCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 0, true);
			this.PreviousCustomsProcedureCodeDropEdit.Name = "PreviousCustomsProcedureCodeDropEdit";
			this.PreviousCustomsProcedureCodeDropEdit.PreBoundMaxLength = 2;
			this.PreviousCustomsProcedureCodeDropEdit.ShowDescriptionBox = false;
			this.PreviousCustomsProcedureCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 18, true);
			this.PreviousCustomsProcedureCodeDropEdit.TabIndex = 119;
			// 
			// CPCUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreviousCustomsProcedureCodeDropEdit);
			this.Controls.Add(this.RequestedCustomsProcedureCodeDropEdit);
			this.Name = "CPCUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RequestedCustomsProcedureCodeDropEdit.ResumeLayout(true);
			this.RequestedCustomsProcedureCodeDropEdit.PerformLayout();
			this.PreviousCustomsProcedureCodeDropEdit.ResumeLayout(true);
			this.PreviousCustomsProcedureCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.GUI.ZDropEdit RequestedCustomsProcedureCodeDropEdit;
		ZArchitecture.GUI.ZDropEdit PreviousCustomsProcedureCodeDropEdit;
	}
}
