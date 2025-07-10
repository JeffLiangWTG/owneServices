namespace Enterprise.MasterFiles.GUI
{
	partial class CommunicationGrid
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommunicationGrid));
			this.ShowNotesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.moduleButtonGrid = new Enterprise.MasterFiles.GUI.CommunicationGrid.ModuleGrid();
			this.NotesTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.editButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.moduleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSalesCallCollection);
			// 
			// moduleButtonGrid
			// 
			this.moduleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.moduleButtonGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)))));
			this.moduleButtonGrid.BindToFindBoxList = ".";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9c34c9ed-3a92-4961-bb35-d8f7edeb0406", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "DateLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ced45aff-4e18-455b-b878-321ba227dba5", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "OQ_TypeOfCall";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4e80f9fe-d64e-4b3e-a43c-426a74047c97", "Contact");
			zTextBoxColumnStyleInfo2.ColumnName = "ContactName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c46e43ab-9594-4ebd-b304-2be2151e5314", "Subject");
			zTextBoxColumnStyleInfo3.ColumnName = "OQ_CallSummary";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1d80a08e-50b6-4fbd-8604-1a96e5db88fb", "Created Time");
			zDateEditColumnStyleInfo2.ColumnName = "OQ_SystemCreateTimeLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a972dff3-800e-4c3a-91df-e7849d9574c2", "ID");
			zTextBoxColumnStyleInfo4.ColumnName = "OQ_CommunicationID";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dd3711bc-0c61-4ff6-b432-eda0592ab9f1", "Client");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OQ_OH";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("115ac214-4052-488f-bcc8-e4d05380f880", "Overall Disposition");
			zTextBoxColumnStyleInfo5.ColumnName = "OverallDispositionDescription";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			this.moduleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.moduleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.moduleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.moduleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.moduleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.moduleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.moduleButtonGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.moduleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.moduleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.moduleButtonGrid.GridId = "be184b07-e9d8-42b4-9c6e-7ee7820edfca";
			this.moduleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.moduleButtonGrid.InnerGrid.GridId = null;
			this.moduleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.moduleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.moduleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.moduleButtonGrid.InnerGrid.Name = "Grid";
			this.moduleButtonGrid.InnerGrid.ReadOnly = true;
			this.moduleButtonGrid.InnerGrid.TabIndex = 0;
			this.moduleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.moduleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Communication;
			this.moduleButtonGrid.Name = "moduleButtonGrid";
			this.moduleButtonGrid.ReadOnly = true;
			this.moduleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 304, true);
			this.moduleButtonGrid.TabIndex = 0;
			// 
			// ShowNotesCheckBox
			// 
			this.ShowNotesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowNotesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7163672a-7ae5-4d51-83fb-441428f65d75", "Show Notes");
			this.ShowNotesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowNotesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 448, true);
			this.ShowNotesCheckBox.Name = "ShowNotesCheckBox";
			this.ShowNotesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ShowNotesCheckBox.TabIndex = 1;
			this.ShowNotesCheckBox.CheckedChanged += new System.EventHandler(this.showNotesCheckBox_CheckedChanged);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.moduleButtonGrid);
			this.MainSplitContainer.Panel1MinSize = 50;
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.NotesTextBox);
			this.MainSplitContainer.Panel2MinSize = 50;
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 445, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(304);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// NotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesTextBox, "OQ_SalesCallNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).OQ_SalesCallNotes)));
			this.NotesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NotesTextBox.IsToolBarVisible = false;
			this.NotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotesTextBox.MaxLength = 10000000;
			this.NotesTextBox.Name = "NotesTextBox";
			this.NotesTextBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 0, true);
			this.NotesTextBox.ReadOnly = true;
			this.NotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 137, true);
			this.NotesTextBox.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewButton,
            this.editButton});
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 448, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 25, true);
			this.toolStrip.TabIndex = 4;
			this.toolStrip.Text = "zToolStrip1";
			// 
			// newButton
			// 
			this.NewButton.BackColor = System.Drawing.Color.Transparent;
			this.NewButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aa8276dd-ba05-4217-a71b-a5fc466e4869", "New");
			this.NewButton.Name = "newButton";
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 22, true);
			// 
			// editButton
			// 
			this.editButton.BackColor = System.Drawing.Color.Transparent;
			this.editButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e7902a6f-b99d-4452-9939-22a50700e810", "Edit");
			this.editButton.Name = "editButton";
			this.editButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0, true);
			this.editButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 22, true);
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// CommunicationGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.MainSplitContainer);
			this.Controls.Add(this.ShowNotesCheckBox);
			this.Name = "CommunicationGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 474, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.moduleButtonGrid.InnerGrid)).EndInit();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox ShowNotesCheckBox;
		internal CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		public ZArchitecture.GUI.ZRichTextBox NotesTextBox;
		private Enterprise.MasterFiles.GUI.CommunicationGrid.ModuleGrid moduleButtonGrid;
		private ZArchitecture.GUI.ZToolStrip toolStrip;
		internal ZArchitecture.GUI.ZToolStripDropDownButton NewButton;
		internal ZArchitecture.GUI.ZToolStripButton editButton;
	}
}
