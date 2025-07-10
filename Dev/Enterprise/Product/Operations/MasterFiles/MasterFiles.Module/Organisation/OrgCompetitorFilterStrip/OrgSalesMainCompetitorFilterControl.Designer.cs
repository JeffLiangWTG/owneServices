
namespace Enterprise.MasterFiles.Module
{
	partial class OrgSalesMainCompetitorFilterControl
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
            this.CompetitorTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CompetitorOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgSalesMainCompetitorModuleFilter);
            // 
            // CompetitorTypeDropEdit
            // 
            this.BindingSource.SetBindingMember(this.CompetitorTypeDropEdit, "CompetitorType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgSalesMainCompetitorModuleFilter)(null)).CompetitorType)));
            this.CompetitorTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 1, true);
            this.CompetitorTypeDropEdit.Name = "CompetitorTypeDropEdit";
            this.CompetitorTypeDropEdit.PreBoundMaxLength = 3;
            this.CompetitorTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 15, true);
            this.CompetitorTypeDropEdit.TabIndex = 1;
            // 
            // CompetitorOrganisationFindBox
            // 
            this.BindingSource.SetBindingMember(this.CompetitorOrganisationFindBox, "Competitor");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.OrgSalesMainCompetitorModuleFilter)(null)).Competitor)));
            this.CompetitorOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 1, true);
            this.CompetitorOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.CompetitorOrganisationFindBox.Name = "CompetitorOrganisationFindBox";
            this.CompetitorOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 15, true);
            this.CompetitorOrganisationFindBox.TabIndex = 2;
            // 
            // OrgSalesMainCompetitorFilterControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.CompetitorOrganisationFindBox);
            this.Controls.Add(this.CompetitorTypeDropEdit);
            this.Name = "OrgSalesMainCompetitorFilterControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit CompetitorTypeDropEdit;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox CompetitorOrganisationFindBox;
	}
}
