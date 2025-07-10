namespace Enterprise.MasterFiles.Module
{
	partial class OrgCreditScoresDnBRatingFilterControl
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
			//components = new System.ComponentModel.Container();
			//this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

			this.financialStrengthDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.creditAppraisalDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.financialStrengthDropEdit.SuspendLayout();
			this.creditAppraisalDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgCreditScoresDnBRatingModuleFilter);
			// 
			// financialStrengthDropEdit
			// 
			this.financialStrengthDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.financialStrengthDropEdit, "FinancialStrength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgCreditScoresDnBRatingModuleFilter)(null)).FinancialStrength)));
			this.financialStrengthDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("6dfa44d1-44c9-46ec-bbff-3f474db8e473", "Financial Strength");
			this.financialStrengthDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 1, true);
			this.financialStrengthDropEdit.Name = "FinancialStrengthDropEdit";
			this.financialStrengthDropEdit.ShowDescriptionBox = false;
			this.financialStrengthDropEdit.MaxLength = 3;
			this.financialStrengthDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.financialStrengthDropEdit.TabIndex = 2;
			// 
			// creditAppraisalDropEdit
			// 
			this.creditAppraisalDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.creditAppraisalDropEdit, "CreditAppraisal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgCreditScoresDnBRatingModuleFilter)(null)).CreditAppraisal)));
			this.creditAppraisalDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("c649d910-02f4-497a-8514-ddb6ad444e33", "Credit Appraisal");
			this.creditAppraisalDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 1, true);
			this.creditAppraisalDropEdit.Name = "CreditAppraisalDropEdit";
			this.creditAppraisalDropEdit.ShowDescriptionBox = false;
			this.creditAppraisalDropEdit.MaxLength = 3;
			this.creditAppraisalDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.creditAppraisalDropEdit.TabIndex = 3;
			this.creditAppraisalDropEdit.BindToList = "CreditAppraisalList";
			// 
			// OrgCodeMappingForeignFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.financialStrengthDropEdit);
			this.Controls.Add(this.creditAppraisalDropEdit);
			this.Name = "OrgCreditScoresDnBRatingModuleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.financialStrengthDropEdit.ResumeLayout(true);
			this.financialStrengthDropEdit.PerformLayout();
			this.creditAppraisalDropEdit.ResumeLayout(true);
			this.creditAppraisalDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit financialStrengthDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit creditAppraisalDropEdit;
	}
}
