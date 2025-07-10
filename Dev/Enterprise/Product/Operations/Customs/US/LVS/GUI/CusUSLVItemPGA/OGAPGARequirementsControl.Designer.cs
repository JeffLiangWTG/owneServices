namespace Enterprise.Customs.US.LVS.GUI
{
	partial class OGAPGARequirementsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.pgaRequirementsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.pgaRequirementsTabPage = new Enterprise.Customs.GUI.BaseDeclarationTabPage();
			this.OGAPGARequirementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.pgaRequirementsTabControl.SuspendLayout();
			this.pgaRequirementsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OGAPGARequirementsGrid)).BeginInit();
			this.OGAPGARequirementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVItem);
			// 
			// pgaRequirementsTabControl
			// 
			this.pgaRequirementsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.pgaRequirementsTabControl.Controls.Add(this.pgaRequirementsTabPage);
			this.pgaRequirementsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pgaRequirementsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.pgaRequirementsTabControl.Name = "pgaRequirementsTabControl";
			this.pgaRequirementsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 450, true);
			this.pgaRequirementsTabControl.TabIndex = 0;
			this.pgaRequirementsTabControl.TabStop = false;
			// 
			// pgaRequirementsTabPage
			// 
			this.pgaRequirementsTabPage.Controls.Add(this.OGAPGARequirementsGrid);
			this.pgaRequirementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.pgaRequirementsTabPage.Name = "pgaRequirementsTabPage";
			this.pgaRequirementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 426, true);
			this.pgaRequirementsTabPage.TabIndex = 0;
			this.pgaRequirementsTabPage.Text = "PGA Requirements";
			// 
			// OGAPGARequirementsGrid
			// 
			this.OGAPGARequirementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OGAPGARequirementsGrid, "ItemPGAWrapperCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(null)).ItemPGAWrapperCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItemPGAWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(null)).ItemPGAWrapperCollection)).SyncRoot)).AgencyCodeWithDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItemPGAWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(null)).ItemPGAWrapperCollection)).SyncRoot)).Requirement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItemPGAWrapper)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(null)).ItemPGAWrapperCollection)).SyncRoot)).DisclaimReason)));
			this.OGAPGARequirementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("6e6f14f8-5994-4f41-8f71-7be1083c8ffc", "Agency");
			zTextBoxColumnStyleInfo1.ColumnName = "AgencyCodeWithDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(446);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e07f3c08-aac0-4f06-b0ad-94993f771f72", "Requirement");
			zTextBoxColumnStyleInfo2.ColumnName = "Requirement";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(431);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("0643c1e9-ee60-44f5-b4a7-2bd8d46a0aac", "Disclaim Reason");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "DisclaimReason";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			this.OGAPGARequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OGAPGARequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OGAPGARequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OGAPGARequirementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OGAPGARequirementsGrid.GridId = "15bbf0a7-6ec0-41fb-b3b0-09bcd0210243";
			this.OGAPGARequirementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OGAPGARequirementsGrid.LayoutKey = "OGAPGARequirementsGrid";
			this.OGAPGARequirementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OGAPGARequirementsGrid.Name = "OGAPGARequirementsGrid";
			this.OGAPGARequirementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 426, true);
			this.OGAPGARequirementsGrid.TabIndex = 0;
			// 
			// OGAPGARequirementsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.pgaRequirementsTabControl);
			this.Name = "OGAPGARequirementsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 450, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.pgaRequirementsTabControl.ResumeLayout(false);
			this.pgaRequirementsTabControl.PerformLayout();
			this.pgaRequirementsTabPage.ResumeLayout(false);
			this.pgaRequirementsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OGAPGARequirementsGrid)).EndInit();
			this.OGAPGARequirementsGrid.ResumeLayout(false);
			this.OGAPGARequirementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTemplateTabControl pgaRequirementsTabControl;
		private Enterprise.Customs.GUI.BaseDeclarationTabPage pgaRequirementsTabPage;
		private ZArchitecture.ZGrid OGAPGARequirementsGrid;
	}
}
