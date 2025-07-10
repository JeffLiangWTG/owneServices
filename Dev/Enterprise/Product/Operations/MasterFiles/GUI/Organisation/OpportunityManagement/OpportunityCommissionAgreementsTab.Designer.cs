namespace Enterprise.MasterFiles.GUI
{
	partial class OpportunityCommissionAgreementsTab
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AgreementsGridToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.NewAgreementButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.AgreementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.AgreementsGridToolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AgreementsGrid)).BeginInit();
			this.AgreementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgOpportunity);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.IsSplitterFixed = true;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.AgreementsGridToolStrip);
			this.mainSplitContainer.Panel1.Controls.Add(this.AgreementsGrid);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 250, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.mainSplitContainer.TabIndex = 0;
			// 
			// AgreementsGridToolStrip
			// 
			this.AgreementsGridToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.AgreementsGridToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.AgreementsGridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewAgreementButton});
			this.AgreementsGridToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 144, true);
			this.AgreementsGridToolStrip.Name = "AgreementsGridToolStrip";
			this.AgreementsGridToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.AgreementsGridToolStrip.TabIndex = 1;
			// 
			// NewAgreementButton
			// 
			this.NewAgreementButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.NewAgreementButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9d4ad116-9a18-4ec6-ad75-f1017d159850", "New");
			this.NewAgreementButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.NewAgreementButton.Name = "NewAgreementButton";
			this.NewAgreementButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 36, true);
			this.NewAgreementButton.Click += new System.EventHandler(this.NewAgreementButton_Click);
			// 
			// AgreementsGrid
			// 
			this.AgreementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AgreementsGrid, "CommissionAgreementsForEdit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).CommissionAgreementsForEdit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).CommissionAgreementsForEdit)).SyncRoot)).CA0_Name)));
			this.AgreementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CA0_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.AgreementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AgreementsGrid.CopySelectedRowsAllowed = true;
			this.AgreementsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.AgreementsGrid.GridId = "c708c0c6-fe28-4c49-a360-dbe5cb7c9093";
			this.AgreementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AgreementsGrid.LayoutKey = "AgreementsGrid";
			this.AgreementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AgreementsGrid.Name = "AgreementsGrid";
			this.AgreementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 144, true);
			this.AgreementsGrid.TabIndex = 0;
			// 
			// OpportunityCommissionAgreementsTab
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "OpportunityCommissionAgreementsTab";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel1.PerformLayout();
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.AgreementsGridToolStrip.ResumeLayout(false);
			this.AgreementsGridToolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AgreementsGrid)).EndInit();
			this.AgreementsGrid.ResumeLayout(false);
			this.AgreementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		protected internal ZArchitecture.ZGrid AgreementsGrid;
		protected ZArchitecture.GUI.ZToolStripButton NewAgreementButton;
		private ZArchitecture.GUI.ZToolStrip AgreementsGridToolStrip;

	}
}
