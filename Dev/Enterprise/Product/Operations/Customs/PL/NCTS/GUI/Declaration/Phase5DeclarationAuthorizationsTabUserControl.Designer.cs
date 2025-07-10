namespace Enterprise.Customs.PL.NCTS.GUI
{
	partial class Phase5DeclarationAuthorizationsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.AuthorisationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).BeginInit();
			this.AuthorisationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.NCTS.Business.NctsHeader);
			// 
			// AuthorisationsGrid
			// 
			this.AuthorisationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AuthorisationsGrid, "MovementHeader.CusAuthorizationUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.NCTS.Business.NctsHeader)(null)).CusAuthorizationUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.NCTS.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.PL.NCTS.Business.NctsHeader)(null)).CusAuthorizationUsages)).SyncRoot)).AGC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.NCTS.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.PL.NCTS.Business.NctsHeader)(null)).CusAuthorizationUsages)).SyncRoot)).AGC_Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.PL.NCTS.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.PL.NCTS.Business.NctsHeader)(null)).CusAuthorizationUsages)).SyncRoot)).AGC_OH_Owner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.NCTS.Business.CusAuthorizationUsage)(((System.Collections.IList)(((Enterprise.Customs.PL.NCTS.Business.NctsHeader)(null)).CusAuthorizationUsages)).SyncRoot)).AGC_Location)));
			this.AuthorisationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AGC_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AGC_Number";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CusAuthorisations;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AGC_OH_Owner";
			zOrganisationFindBoxColumnStyleInfo1.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "AGC_Location";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.PL.AuthorisationRule;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.AuthorisationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.AuthorisationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.AuthorisationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationsGrid.GridId = "0D5091BE-4932-47AC-9912-195F23FC0D8F";
			this.AuthorisationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorisationsGrid.LayoutKey = "AuthorisationsGrid";
			this.AuthorisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorisationsGrid.Name = "AuthorisationsGrid";
			this.AuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			this.AuthorisationsGrid.TabIndex = 0;
			// 
			// Phase5DeclarationAuthorizationsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AuthorisationsGrid);
			this.Name = "Phase5DeclarationAuthorizationsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).EndInit();
			this.AuthorisationsGrid.ResumeLayout(false);
			this.AuthorisationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid AuthorisationsGrid;
	}
}
