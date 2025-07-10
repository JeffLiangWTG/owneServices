namespace Enterprise.Freight.Agency.GUI
{
	partial class SundryChargesForm
	{
		protected new void InitializeComponent()
		{
			this.postingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.mainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			topPanel = new CargoWise.Windows.UI.KPanel();
			descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			detailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			modeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			activityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			toDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			fromDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			billToParty = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			notesTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			logsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.postingButtons.SuspendLayout();
			topPanel.SuspendLayout();
			bottomPanel.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 666, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.SundryCharges);
			// 
			// topPanel
			// 
			topPanel.Controls.Add(descriptionTextBox);
			topPanel.Controls.Add(detailsTextBox);
			topPanel.Controls.Add(modeDropEdit);
			topPanel.Controls.Add(activityDropEdit);
			topPanel.Controls.Add(typeDropEdit);
			topPanel.Controls.Add(toDateDateEdit);
			topPanel.Controls.Add(fromDateDateEdit);
			topPanel.Controls.Add(billToParty);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 56, true);
			topPanel.TabIndex = 0;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(descriptionTextBox, "D4_SundriesDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_SundriesDescription)));
			descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 32, true);
			descriptionTextBox.Name = "descriptionTextBox";
			descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			descriptionTextBox.TabIndex = 6;
			// 
			// detailsTextBox
			// 
			this.BindingSource.SetBindingMember(detailsTextBox, "D4_AdditionalDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_AdditionalDetails)));
			detailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(808, 32, true);
			detailsTextBox.Name = "detailsTextBox";
			detailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			detailsTextBox.TabIndex = 7;
			// 
			// modeDropEdit
			// 
			this.BindingSource.SetBindingMember(modeDropEdit, "D4_SundryJobMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_SundryJobMode)));
			modeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 8, true);
			modeDropEdit.Name = "modeDropEdit";
			modeDropEdit.PreBoundMaxLength = 3;
			modeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			modeDropEdit.TabIndex = 2;
			// 
			// activityDropEdit
			// 
			this.BindingSource.SetBindingMember(activityDropEdit, "D4_SundryJobActivity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_SundryJobActivity)));
			activityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(808, 8, true);
			activityDropEdit.Name = "activityDropEdit";
			activityDropEdit.PreBoundMaxLength = 3;
			activityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			activityDropEdit.TabIndex = 3;
			// 
			// typeDropEdit
			// 
			this.BindingSource.SetBindingMember(typeDropEdit, "D4_SundriesJobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_SundriesJobType)));
			typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 8, true);
			typeDropEdit.Name = "typeDropEdit";
			typeDropEdit.PreBoundMaxLength = 3;
			typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			typeDropEdit.TabIndex = 1;
			// 
			// toDateDateEdit
			// 
			toDateDateEdit.AutoCompleteMonthThreshold = 1;
			toDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(toDateDateEdit, "D4_ToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_ToDate)));
			toDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 32, true);
			toDateDateEdit.Name = "toDateDateEdit";
			toDateDateEdit.TabIndex = 5;
			// 
			// fromDateDateEdit
			// 
			fromDateDateEdit.AutoCompleteMonthThreshold = 1;
			fromDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(fromDateDateEdit, "D4_FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_FromDate)));
			fromDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			fromDateDateEdit.Name = "fromDateDateEdit";
			fromDateDateEdit.TabIndex = 4;
			// 
			// billToParty
			// 
			this.BindingSource.SetBindingMember(billToParty, "D4_OH_BillToParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.SundryCharges)(null)).D4_OH_BillToParty)));
			billToParty.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			billToParty.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			billToParty.Name = "billToParty";
			billToParty.PreBoundMaxLength = 12;
			billToParty.ShowDescriptionBox = false;
			billToParty.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			billToParty.TabIndex = 0;
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(this.postingButtons);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 632, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 34, true);
			bottomPanel.TabIndex = 2;
			// 
			// postingButtons
			// 
			this.postingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(735, 6, true);
			this.postingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtons.Name = "postingButtons";
			this.postingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 25, true);
			this.postingButtons.TabIndex = 0;
			// 
			// notesTabPage
			// 
			notesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			notesTabPage.Name = "notesTabPage";
			notesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 549, true);
			notesTabPage.TabIndex = 0;
			// 
			// logsTabPage
			// 
			logsTabPage.ExcludeFromBindingOnSave = true;
			logsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			logsTabPage.Name = "logsTabPage";
			logsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 549, true);
			logsTabPage.TabIndex = 1;
			// 
			// mainTabControl
			// 
			this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.mainTabControl.Controls.Add(notesTabPage);
			this.mainTabControl.Controls.Add(logsTabPage);
			this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 56, true);
			this.mainTabControl.Name = "mainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 576, true);
			this.mainTabControl.TabIndex = 1;
			// 
			// SundryChargesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 690, true);
			this.Controls.Add(this.mainTabControl);
			this.Controls.Add(bottomPanel);
			this.Controls.Add(topPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.SundryCharges);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 717, true);
			this.Name = "SundryChargesForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(topPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(this.mainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.postingButtons.ResumeLayout(true);
			this.postingButtons.PerformLayout();
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			bottomPanel.ResumeLayout(false);
			this.mainTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZTemplateTabControl mainTabControl;
		Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtons;
		CargoWise.Windows.UI.KPanel topPanel;
		Enterprise.ZArchitecture.ZTextBox descriptionTextBox;
		Enterprise.ZArchitecture.ZTextBox detailsTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit modeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit activityDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit typeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit toDateDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit fromDateDateEdit;
		Enterprise.MasterFiles.GUI.ZOrganisationFindBox billToParty;
		CargoWise.Windows.UI.KPanel bottomPanel;
		Enterprise.ZArchitecture.GUI.ZStmNoteTabPage notesTabPage;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage logsTabPage;
	}
}
