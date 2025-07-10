namespace Enterprise.MasterFiles.Module
{
	partial class OrgHasMainCompetitorFilterControl
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
            this.HasMainCompetitorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgHasMainCompetitorModuleFilter);
            // 
            // CompetitorTypeDropEdit
            // 
            this.BindingSource.SetBindingMember(this.CompetitorTypeDropEdit, "CompetitorType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgHasMainCompetitorModuleFilter)(null)).CompetitorType)));
            this.CompetitorTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 1, true);
            this.CompetitorTypeDropEdit.Name = "CompetitorTypeDropEdit";
            this.CompetitorTypeDropEdit.PreBoundMaxLength = 3;
            this.CompetitorTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 16, true);
            this.CompetitorTypeDropEdit.TabIndex = 1;
            // 
            // HasMainCompetitorCheckBox
            // 
            this.BindingSource.SetBindingMember(this.HasMainCompetitorCheckBox, "HasMainCompetitor");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgHasMainCompetitorModuleFilter)(null)).HasMainCompetitor)));
            this.HasMainCompetitorCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgHasMainCompetitorModuleFilter|DDD3EE09-E7EE-41F9-B62B-22E46084B0D0", "Has Main Competitor");
            this.HasMainCompetitorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 2, true);
            this.HasMainCompetitorCheckBox.Name = "HasMainCompetitorCheckBox";
            this.HasMainCompetitorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 16, true);
            this.HasMainCompetitorCheckBox.TabIndex = 2;
            this.HasMainCompetitorCheckBox.UseVisualStyleBackColor = true;
            // 
            // OrgHasMainCompetitorFilterControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.HasMainCompetitorCheckBox);
            this.Controls.Add(this.CompetitorTypeDropEdit);
            this.Name = "OrgHasMainCompetitorFilterControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit CompetitorTypeDropEdit;
		private ZArchitecture.GUI.ZCheckBox HasMainCompetitorCheckBox;
	}
}
