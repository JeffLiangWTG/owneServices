namespace Enterprise.Customs.TW.GUI
{
	partial class MiscOptionsUserControl
	{

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReservedFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReservedFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.JE_RS_NKServiceLevelBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JE_DeclDocTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ItineraryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItineraryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.PaidByDropEdit.SuspendLayout();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReservedFieldsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReservedFieldsGrid)).BeginInit();
			this.ReservedFieldsGrid.SuspendLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.SuspendLayout();
			this.JE_DeclDocTypeDropEdit.SuspendLayout();
			this.ItineraryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItineraryGrid)).BeginInit();
			this.ItineraryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 73, true);
			this.PaymentPartyDropEdit.Visible = false;
			// 
			// PaidByDropEdit
			// 
			this.PaidByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 119, true);
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Controls.Add(this.JE_DeclDocTypeDropEdit);
			this.MiscOptionsGroupBox.Controls.Add(this.JE_RS_NKServiceLevelBoundFindBox);
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 158, true);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaidByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.MergeByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BrokerCodeFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.JE_RS_NKServiceLevelBoundFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.JE_DeclDocTypeDropEdit, 0);
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// ReservedFieldsGroupBox
			// 
			this.ReservedFieldsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("ca592b27-746d-437a-b788-d579e5771514", "Reserved Fields");
			this.ReservedFieldsGroupBox.Controls.Add(this.ReservedFieldsGrid);
			this.ReservedFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 177, true);
			this.ReservedFieldsGroupBox.Name = "ReservedFieldsGroupBox";
			this.ReservedFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 100, true);
			this.ReservedFieldsGroupBox.TabIndex = 1;
			this.ReservedFieldsGroupBox.TabStop = false;
			// 
			// ReservedFieldsGrid
			// 
			this.ReservedFieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReservedFieldsGrid, "ReservedFields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).ReservedFields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclarationReservedField)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).ReservedFields)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclarationReservedField)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).ReservedFields)).SyncRoot)).CY_Data)));
			this.ReservedFieldsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ReservedFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReservedFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReservedFieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReservedFieldsGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.ReservedFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReservedFieldsGrid.LayoutKey = "ReservedFieldsGrid";
			this.ReservedFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReservedFieldsGrid.MaximumRows = 10;
			this.ReservedFieldsGrid.Name = "ReservedFieldsGrid";
			this.ReservedFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 81, true);
			this.ReservedFieldsGrid.TabIndex = 0;
			// 
			// JE_RS_NKServiceLevelBoundFindBox
			// 
			this.JE_RS_NKServiceLevelBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_RS_NKServiceLevelBoundFindBox, "JE_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).JE_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Lookups.ServiceLevels)));
			this.JE_RS_NKServiceLevelBoundFindBox.BindToList = "Lookups.ServiceLevels";
			this.JE_RS_NKServiceLevelBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 49, true);
			this.JE_RS_NKServiceLevelBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.JE_RS_NKServiceLevelBoundFindBox.Name = "JE_RS_NKServiceLevelBoundFindBox";
			this.JE_RS_NKServiceLevelBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JE_RS_NKServiceLevelBoundFindBox.ParentType = null;
			this.JE_RS_NKServiceLevelBoundFindBox.PreBoundMaxLength = 3;
			this.JE_RS_NKServiceLevelBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.JE_RS_NKServiceLevelBoundFindBox.TabIndex = 4;
			// 
			// JE_DeclDocTypeDropEdit
			// 
			this.JE_DeclDocTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_DeclDocTypeDropEdit, "JE_DeclDocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).JE_DeclDocType)));
			this.JE_DeclDocTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 96, true);
			this.JE_DeclDocTypeDropEdit.Name = "JE_DeclDocTypeDropEdit";
			this.JE_DeclDocTypeDropEdit.PreBoundMaxLength = 3;
			this.JE_DeclDocTypeDropEdit.ShouldResizeByMaxLength = false;
			this.JE_DeclDocTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.JE_DeclDocTypeDropEdit.TabIndex = 8;
			// 
			// ItineraryGroupBox
			// 
			this.ItineraryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.ItineraryGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("d8584f7d-181f-4ae0-80f1-e4cfb8c5cfd3", "Itinerary");
			this.ItineraryGroupBox.Controls.Add(this.ItineraryGrid);
			this.ItineraryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 292, true);
			this.ItineraryGroupBox.Name = "ItineraryGroupBox";
			this.ItineraryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 147, true);
			this.ItineraryGroupBox.TabIndex = 2;
			this.ItineraryGroupBox.TabStop = false;
			// 
			// ItineraryGrid
			// 
			this.ItineraryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItineraryGrid, "Itineraries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Itineraries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.ItineraryData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Itineraries)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.ItineraryData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).Itineraries)).SyncRoot)).Description)));
			this.ItineraryGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ItineraryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ItineraryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ItineraryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItineraryGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.ItineraryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItineraryGrid.LayoutKey = "ItineraryGrid";
			this.ItineraryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ItineraryGrid.Name = "ItineraryGrid";
			this.ItineraryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 128, true);
			this.ItineraryGrid.TabIndex = 0;
			// 
			// MiscOptionsUserControl
			// 
			this.Controls.Add(this.ReservedFieldsGroupBox);
			this.Controls.Add(this.ItineraryGroupBox);
			this.Name = "MiscOptionsUserControl";
			this.Controls.SetChildIndex(this.ItineraryGroupBox, 0);
			this.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.ReservedFieldsGroupBox, 0);
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.PaidByDropEdit.ResumeLayout(true);
			this.PaidByDropEdit.PerformLayout();
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.MergeByDropEdit.ResumeLayout(true);
			this.MergeByDropEdit.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReservedFieldsGroupBox.ResumeLayout(false);
			this.ReservedFieldsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReservedFieldsGrid)).EndInit();
			this.ReservedFieldsGrid.ResumeLayout(false);
			this.ReservedFieldsGrid.PerformLayout();
			this.JE_RS_NKServiceLevelBoundFindBox.ResumeLayout(true);
			this.JE_RS_NKServiceLevelBoundFindBox.PerformLayout();
			this.JE_DeclDocTypeDropEdit.ResumeLayout(true);
			this.JE_DeclDocTypeDropEdit.PerformLayout();
			this.ItineraryGroupBox.ResumeLayout(false);
			this.ItineraryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItineraryGrid)).EndInit();
			this.ItineraryGrid.ResumeLayout(false);
			this.ItineraryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZArchitecture.GUI.ZGroupBox ReservedFieldsGroupBox;
		private ZArchitecture.ZGrid ReservedFieldsGrid;
		private ZArchitecture.GUI.ZCodeFindBox JE_RS_NKServiceLevelBoundFindBox;
		private ZArchitecture.GUI.ZDropEdit JE_DeclDocTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox ItineraryGroupBox;
		private ZArchitecture.ZGrid ItineraryGrid;
	}
}
