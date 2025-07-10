using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class ConsolidatedDeclarationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConsolidatedDeclarationForm));
			this.HeaderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.LinkedDeclarationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobDeclarationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LeadDeclarationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LeadDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OpenJobDeclarationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderDetailsGroupBox.SuspendLayout();
			this.LinkedDeclarationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobDeclarationsGrid)).BeginInit();
			this.JobDeclarationsGrid.SuspendLayout();
			this.LeadDeclarationGroupBox.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 631, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.LinkedDeclarationsGroupBox);
			this.MainTabPage.Controls.Add(this.LeadDeclarationGroupBox);
			this.MainTabPage.Controls.Add(this.HeaderDetailsGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 596, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 596, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 609, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 631, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.ConsolidatedDeclaration);
			// 
			// HeaderDetailsGroupBox
			// 
			this.HeaderDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1309325D-13CE-4904-8F8E-D4857B87365A", "Header Details");
			this.HeaderDetailsGroupBox.Controls.Add(this.DynamicPanel);
			this.HeaderDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDetailsGroupBox.Name = "HeaderDetailsGroupBox";
			this.HeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 400, true);
			this.HeaderDetailsGroupBox.TabIndex = 0;
			this.HeaderDetailsGroupBox.TabStop = false;
			// 
			// DynamicPanel
			// 
			this.DynamicPanel.AllowDrop = true;
			this.DynamicPanel.AutoScroll = true;
			this.DynamicPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DynamicPanel.Name = "DynamicPanel";
			this.DynamicPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 383, true);
			this.DynamicPanel.TabIndex = 1;
			// 
			// LinkedDeclarationsGroupBox
			// 
			this.LinkedDeclarationsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("f6918fe9-ea13-4f83-88aa-0b35a71ef5b0", "Linked Declarations");
			this.LinkedDeclarationsGroupBox.Controls.Add(this.JobDeclarationsGrid);
			this.LinkedDeclarationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinkedDeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 460, true);
			this.LinkedDeclarationsGroupBox.Name = "LinkedDeclarationsGroupBox";
			this.LinkedDeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 152, true);
			this.LinkedDeclarationsGroupBox.TabIndex = 0;
			this.LinkedDeclarationsGroupBox.TabStop = false;
			// 
			// JobDeclarationsGrid
			// 
			this.JobDeclarationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobDeclarationsGrid, "JobDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).JobDeclarations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).JobDeclarations)).SyncRoot)).JE_DeclarationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).JobDeclarations)).SyncRoot)).JE_MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).JobDeclarations)).SyncRoot)).JE_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).JobDeclarations)).SyncRoot)).JE_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).JobDeclarations)).SyncRoot)).Lookups.SuppliersList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(((System.Collections.IList)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).JobDeclarations)).SyncRoot)).JE_GoodsDescription)));
			this.JobDeclarationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JE_DeclarationReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("39B3A70A-7DD9-41CD-8A52-38EA59564789", "Master Bill");
			zTextBoxColumnStyleInfo2.ColumnName = "JE_MasterBill";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1DCC6917-C58A-4C40-9BC4-91EC0EB6CEF7", "House Bill");
			zTextBoxColumnStyleInfo3.ColumnName = "JE_HouseBill";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.SuppliersList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("E9A6D606-F19D-4D25-81DB-B1ACF5253779", "Supplier");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JE_OH_Supplier";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("A4F1B8BE-4BCA-403A-8B21-4DD0FF7D439D", "Goods Description");
			zTextBoxColumnStyleInfo4.ColumnName = "JE_GoodsDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.JobDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.JobDeclarationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.JobDeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.JobDeclarationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobDeclarationsGrid.GridId = "5806dce0-5553-480b-881c-741f5c1c6475";
			this.JobDeclarationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobDeclarationsGrid.IsWholeRowSelectedOnClick = true;
			this.JobDeclarationsGrid.LayoutKey = "JobDeclarationsGrid";
			this.JobDeclarationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.JobDeclarationsGrid.Name = "JobDeclarationsGrid";
			this.JobDeclarationsGrid.ReadOnly = true;
			this.JobDeclarationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(975, 119, true);
			this.JobDeclarationsGrid.TabIndex = 2;
			this.JobDeclarationsGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.JobDeclarationsGrid_MouseDown);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5F429687-2295-4BB3-9315-C7704347C022", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 596, true);
			this.MessagesTabPage.TabIndex = 1;
			// 
			// LeadDeclarationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LeadDeclarationNumberTextBox, "LeadDeclaration+JE_DeclarationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.ConsolidatedDeclaration)(null)).LeadDeclaration.JE_DeclarationReference)));
			this.LeadDeclarationNumberTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("LeadDeclarationNumberTextBox.CaptionResourceString")));
			this.LeadDeclarationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 25, true);
			this.LeadDeclarationNumberTextBox.Name = "LeadDeclarationNumberTextBox";
			this.LeadDeclarationNumberTextBox.ReadOnly = true;
			this.LeadDeclarationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.LeadDeclarationNumberTextBox.TabIndex = 0;
			// 
			// LeadDeclarationGroupBox
			// 
			this.LeadDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8A33A2DC-79BE-4699-832B-33118DB47044", "Lead Declaration");
			this.LeadDeclarationGroupBox.Controls.Add(this.OpenJobDeclarationButton);
			this.LeadDeclarationGroupBox.Controls.Add(this.LeadDeclarationNumberTextBox);
			this.LeadDeclarationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LeadDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 400, true);
			this.LeadDeclarationGroupBox.Name = "LeadDeclarationGroupBox";
			this.LeadDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 60, true);
			this.LeadDeclarationGroupBox.TabIndex = 1;
			this.LeadDeclarationGroupBox.TabStop = false;
			// 
			// OpenJobDeclarationButton
			// 
			this.OpenJobDeclarationButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F3AE9650-5689-4727-BF9F-A6CC8C1EBF3D", "Open");
			this.OpenJobDeclarationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 24, true);
			this.OpenJobDeclarationButton.Name = "OpenJobDeclarationButton";
			this.OpenJobDeclarationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 22, true);
			this.OpenJobDeclarationButton.TabIndex = 1;
			this.OpenJobDeclarationButton.ToolTipCaption = null;
			this.OpenJobDeclarationButton.UseVisualStyleBackColor = true;
			this.OpenJobDeclarationButton.Click += new System.EventHandler(this.OpenJobDeclarationButton_Click);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 553, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// ConsolidatedDeclarationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 687, true);
			this.DataSourceType = typeof(Enterprise.Customs.Business.ConsolidatedDeclaration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 725, true);
			this.Name = "ConsolidatedDeclarationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderDetailsGroupBox.ResumeLayout(false);
			this.HeaderDetailsGroupBox.PerformLayout();
			this.LinkedDeclarationsGroupBox.ResumeLayout(false);
			this.LinkedDeclarationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobDeclarationsGrid)).EndInit();
			this.JobDeclarationsGrid.ResumeLayout(false);
			this.JobDeclarationsGrid.PerformLayout();
			this.LeadDeclarationGroupBox.ResumeLayout(false);
			this.LeadDeclarationGroupBox.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		protected internal Enterprise.ZArchitecture.ZGrid JobDeclarationsGrid;
		DynamicLayoutPanel DynamicPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox LeadDeclarationGroupBox;
		Enterprise.ZArchitecture.ZTextBox LeadDeclarationNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZButton OpenJobDeclarationButton;
		Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox HeaderDetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox LinkedDeclarationsGroupBox;
		Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
