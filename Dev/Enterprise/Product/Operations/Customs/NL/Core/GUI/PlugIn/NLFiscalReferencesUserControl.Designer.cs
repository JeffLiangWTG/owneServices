namespace Enterprise.Customs.NL.GUI
{
	partial class NLFiscalReferencesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.FiscalReferencesCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FiscalReferencesReferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FiscalReferencesCusEntryInstructionDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.FiscalReferencesSplitter = new CargoWise.Windows.UI.KSplitter();
			this.FiscalReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FiscalReferencesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FiscalReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FiscalReferencesCodeDropEdit.SuspendLayout();
			this.FiscalReferencesReferenceDropEdit.SuspendLayout();
			this.FiscalReferencesCusEntryInstructionDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FiscalReferencesGrid)).BeginInit();
			this.FiscalReferencesGrid.SuspendLayout();
			this.FiscalReferencesPanel.SuspendLayout();
			this.FiscalReferencesGroupBox.SuspendLayout();
			this.SuspendLayout();
			this.CaptionRenderingEnabled = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// FiscalReferencesCodeDropEdit
			// 
			this.FiscalReferencesCodeDropEdit.AllowDrop = true;
			this.FiscalReferencesCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FiscalReferencesCodeDropEdit, "FiscalReferences.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.FiscalReferencesCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 18, true);
			this.FiscalReferencesCodeDropEdit.Name = "FiscalReferencesCodeDropEdit";
			this.FiscalReferencesCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 18, true);
			this.FiscalReferencesCodeDropEdit.TabIndex = 3;
			// 
			// FiscalReferencesReferenceDropEdit
			// 
			this.FiscalReferencesReferenceDropEdit.AllowDrop = true;
			this.FiscalReferencesReferenceDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FiscalReferencesReferenceDropEdit, "FiscalReferences.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.FiscalReferencesReferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 42, true);
			this.FiscalReferencesReferenceDropEdit.Name = "FiscalReferencesReferenceDropEdit";
			this.FiscalReferencesReferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 18, true);
			this.FiscalReferencesReferenceDropEdit.TabIndex = 4;
			// 
			// FiscalReferencesCusEntryInstructionDropEdit
			// 
			this.FiscalReferencesCusEntryInstructionDropEdit.AllowDrop = true;
			this.FiscalReferencesCusEntryInstructionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FiscalReferencesCusEntryInstructionDropEdit, "FiscalReferences.EntryInstructionID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.FiscalReferencesCusEntryInstructionDropEdit.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("C61E30B2-4D8B-4607-B2C6-A17855345306", "Entry Instruction");
			this.FiscalReferencesCusEntryInstructionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 66, true);
			this.FiscalReferencesCusEntryInstructionDropEdit.Name = "FiscalReferencesCusEntryInstructionDropEdit";
			this.FiscalReferencesCusEntryInstructionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 18, true);
			this.FiscalReferencesCusEntryInstructionDropEdit.TabIndex = 5;
			this.FiscalReferencesCusEntryInstructionDropEdit.Visible = false;
			// 
			// FiscalReferencesSplitter
			// 
			this.FiscalReferencesSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FiscalReferencesSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 150, true);
			this.FiscalReferencesSplitter.Name = "FiscalReferencesSplitter";
			this.FiscalReferencesSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 3, true);
			this.FiscalReferencesSplitter.TabIndex = 7;
			this.FiscalReferencesSplitter.TabStop = false;
			// 
			// FiscalReferencesGrid
			// 
			this.FiscalReferencesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FiscalReferencesGrid, "FiscalReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.FiscalReferencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("FD548239-4962-4B83-8C98-060F18284FCE", "Entry Instruction");
			zGuidDropEditColumnStyleInfo1.ColumnName = "EntryInstructionID";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo1.IsVisible = false;
			this.FiscalReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FiscalReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.FiscalReferencesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.FiscalReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiscalReferencesGrid.GridId = "7f9108b9-5689-4cdc-8d2e-297cb53b86fe";
			this.FiscalReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FiscalReferencesGrid.LayoutKey = "FiscalReferencesGrid";
			this.FiscalReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FiscalReferencesGrid.Name = "FiscalReferencesGrid";
			this.FiscalReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 150, true);
			this.FiscalReferencesGrid.TabIndex = 0;
			// 
			// FiscalReferencesPanel
			// 
			this.FiscalReferencesPanel.Controls.Add(this.FiscalReferencesGroupBox);
			this.FiscalReferencesPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FiscalReferencesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 154, true);
			this.FiscalReferencesPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 90, true);
			this.FiscalReferencesPanel.Name = "FiscalReferencesPanel";
			this.FiscalReferencesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 120, true);
			this.FiscalReferencesPanel.TabIndex = 1;
			// 
			// FiscalReferencesGroupBox
			// 
			this.FiscalReferencesGroupBox.Controls.Add(this.FiscalReferencesCodeDropEdit);
			this.FiscalReferencesGroupBox.Controls.Add(this.FiscalReferencesReferenceDropEdit);
			this.FiscalReferencesGroupBox.Controls.Add(this.FiscalReferencesCusEntryInstructionDropEdit);
			this.FiscalReferencesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiscalReferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FiscalReferencesGroupBox.Name = "FiscalReferencesGroupBox";
			this.FiscalReferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 120, true);
			this.FiscalReferencesGroupBox.TabIndex = 0;
			this.FiscalReferencesGroupBox.TabStop = false;
			this.FiscalReferencesGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("F9613661-B91F-4596-8D89-0FD722CB4748", "Fiscal References");
			// 
			// NLFiscalReferencesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.FiscalReferencesGrid);
			this.Controls.Add(this.FiscalReferencesSplitter);
			this.Controls.Add(this.FiscalReferencesPanel);
			this.Name = "NLFiscalReferencesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FiscalReferencesCodeDropEdit.ResumeLayout(true);
			this.FiscalReferencesCodeDropEdit.PerformLayout();
			this.FiscalReferencesReferenceDropEdit.ResumeLayout(true);
			this.FiscalReferencesReferenceDropEdit.PerformLayout();
			this.FiscalReferencesCusEntryInstructionDropEdit.ResumeLayout(true);
			this.FiscalReferencesCusEntryInstructionDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FiscalReferencesGrid)).EndInit();
			this.FiscalReferencesGrid.ResumeLayout(false);
			this.FiscalReferencesGrid.PerformLayout();
			this.FiscalReferencesPanel.ResumeLayout(false);
			this.FiscalReferencesPanel.PerformLayout();
			this.FiscalReferencesGroupBox.ResumeLayout(false);
			this.FiscalReferencesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		#endregion

		private Enterprise.ZArchitecture.ZGrid FiscalReferencesGrid;
		protected CargoWise.Windows.UI.KSplitter FiscalReferencesSplitter;
		private Enterprise.ZArchitecture.GUI.ZPanel FiscalReferencesPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox FiscalReferencesGroupBox;

		protected Enterprise.ZArchitecture.GUI.ZDropEdit FiscalReferencesCodeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit FiscalReferencesReferenceDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGuidDropEdit FiscalReferencesCusEntryInstructionDropEdit;
	}
}
