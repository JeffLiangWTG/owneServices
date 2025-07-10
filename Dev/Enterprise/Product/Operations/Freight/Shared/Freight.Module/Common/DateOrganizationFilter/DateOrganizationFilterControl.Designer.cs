using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Module
{
	partial class DateOrganizationFilterControl : ZDateRangeControl
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
			this.OrganizationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.OrganizationComparisonOperator = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PropertySearchDropEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			this.FilterOptionDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganizationFindBox.SuspendLayout();
			this.OrganizationComparisonOperator.SuspendLayout();
			this.SuspendLayout();
			// 
			// PropertySearchDropEdit
			// 
			this.PropertySearchDropEdit.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("15a70d79-6d7b-4032-963c-5450103547fc", "Date");
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Module.DateOrganizationFilter);
			// 
			// OrganizationFindBox
			// 
			this.OrganizationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationFindBox, "OrganizationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Module.DateOrganizationFilter)(null)).OrganizationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Module.DateOrganizationFilter)(null)).OrganizationList)));
			this.OrganizationFindBox.BindToList = "OrganizationList";
			this.OrganizationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 25, true);
			this.OrganizationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganizationFindBox.Name = "OrganizationFindBox";
			this.OrganizationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.OrganizationFindBox.TabIndex = 7;
			// 
			// OrganizationComparisonOperator
			// 
			this.OrganizationComparisonOperator.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationComparisonOperator, "OrganizationComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Module.DateOrganizationFilter)(null)).OrganizationComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Module.DateOrganizationFilter)(null)).ComparisonOperator_List)));
			this.OrganizationComparisonOperator.BindToList = "ComparisonOperator_List";
			this.OrganizationComparisonOperator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 25, true);
			this.OrganizationComparisonOperator.Name = "OrganizationComparisonOperator";
			this.OrganizationComparisonOperator.ShouldResizeByMaxLength = true;
			this.OrganizationComparisonOperator.ShowDescriptionBox = false;
			this.OrganizationComparisonOperator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.OrganizationComparisonOperator.TabIndex = 6;
			// 
			// DateOrganizationFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OrganizationComparisonOperator);
			this.Controls.Add(this.OrganizationFindBox);
			this.Name = "DateOrganizationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 48, true);
			this.Controls.SetChildIndex(this.FilterOptionDropEdit, 0);
			this.Controls.SetChildIndex(this.OrganizationFindBox, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.PropertySearchDropEdit, 0);
			this.Controls.SetChildIndex(this.OrganizationComparisonOperator, 0);
			this.PropertySearchDropEdit.ResumeLayout(true);
			this.PropertySearchDropEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.FilterOptionDropEdit.ResumeLayout(true);
			this.FilterOptionDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganizationFindBox.ResumeLayout(true);
			this.OrganizationFindBox.PerformLayout();
			this.OrganizationComparisonOperator.ResumeLayout(true);
			this.OrganizationComparisonOperator.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MasterFiles.GUI.ZOrganisationFindBox OrganizationFindBox;
		private ZArchitecture.GUI.ZDropEdit OrganizationComparisonOperator;
	}
}
