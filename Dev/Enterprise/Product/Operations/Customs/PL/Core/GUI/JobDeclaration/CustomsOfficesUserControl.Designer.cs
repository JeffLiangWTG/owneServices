namespace Enterprise.Customs.PL.GUI
{
	public partial class PLCustomsOfficesUserControl : EU.GUI.CustomsOfficesUserControl
	{
		private System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
            this.AdditionalRequiredOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PresentationStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.OfficesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).BeginInit();
            this.CustomsOfficesGrid.SuspendLayout();
            this.CustomsOfficeFindBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AdditionalRequiredOfficeFindBox.SuspendLayout();
            this.PresentationStartDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // OfficesGroupBox
            // 
            this.OfficesGroupBox.Controls.Add(this.PresentationStartDateEdit);
            this.OfficesGroupBox.Controls.Add(this.AdditionalRequiredOfficeFindBox);
            this.OfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 166, true);
            this.OfficesGroupBox.Controls.SetChildIndex(this.AdditionalRequiredOfficeFindBox, 0);
            this.OfficesGroupBox.Controls.SetChildIndex(this.CustomsOfficesGrid, 0);
            this.OfficesGroupBox.Controls.SetChildIndex(this.CustomsOfficeFindBox, 0);
            this.OfficesGroupBox.Controls.SetChildIndex(this.PresentationStartDateEdit, 0);
            // 
            // CustomsOfficesGrid
            // 
            this.CustomsOfficesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CustomsOfficesGrid, "CustomsOfficesForBinding");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsOfficesForBinding)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.OfficeCode)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsOfficesForBinding)).SyncRoot)).CY_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.OfficeCode)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsOfficesForBinding)).SyncRoot)).CY_Data)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.OfficeCode)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsOfficesForBinding)).SyncRoot)).CY_OfficeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.PL.Business.Declaration.OfficeCode)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsOfficesForBinding)).SyncRoot)).CY_Date)));
            this.CustomsOfficesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 71, true);
            this.CustomsOfficesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 61, true);
            this.CustomsOfficesGrid.TabIndex = 3;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
            // 
            // AdditionalRequiredOfficeFindBox
            // 
            this.AdditionalRequiredOfficeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AdditionalRequiredOfficeFindBox, "JE_OfficeOfEntryExit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).JE_OfficeOfEntryExit)));
            this.AdditionalRequiredOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 45, true);
            this.AdditionalRequiredOfficeFindBox.Name = "AdditionalRequiredOfficeFindBox";
            this.AdditionalRequiredOfficeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.AdditionalRequiredOfficeFindBox.ParentType = null;
            this.AdditionalRequiredOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 15, true);
            this.AdditionalRequiredOfficeFindBox.TabIndex = 2;
            // 
            // PresentationStartDateEdit
            // 
            this.PresentationStartDateEdit.AllowDrop = true;
            this.PresentationStartDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.PresentationStartDateEdit, "ZG_PresentationStartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).ZG_PresentationStartDate)));
            this.PresentationStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 140, true);
            this.PresentationStartDateEdit.Name = "PresentationStartDateEdit";
            this.PresentationStartDateEdit.TabIndex = 4;
            // 
            // PLCustomsOfficesUserControl
            // 
            this.Name = "PLCustomsOfficesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 166, true);
            this.OfficesGroupBox.ResumeLayout(false);
            this.OfficesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).EndInit();
            this.CustomsOfficesGrid.ResumeLayout(false);
            this.CustomsOfficesGrid.PerformLayout();
            this.CustomsOfficeFindBox.ResumeLayout(true);
            this.CustomsOfficeFindBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AdditionalRequiredOfficeFindBox.ResumeLayout(true);
            this.AdditionalRequiredOfficeFindBox.PerformLayout();
            this.PresentationStartDateEdit.ResumeLayout(true);
            this.PresentationStartDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZCodeFindBox AdditionalRequiredOfficeFindBox;
		protected ZArchitecture.GUI.ZDateEdit PresentationStartDateEdit;
	}
}
