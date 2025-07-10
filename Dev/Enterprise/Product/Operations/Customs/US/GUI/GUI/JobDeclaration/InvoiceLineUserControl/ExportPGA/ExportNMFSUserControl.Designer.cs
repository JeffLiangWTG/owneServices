namespace Enterprise.Customs.US.GUI
{
	partial class ExportNMFSUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.NMFSGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NMFSHeaderGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NMFSGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NMFSHeaderGrid)).BeginInit();
			this.NMFSHeaderGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.NMFSLineCollection);
			// 
			// NMFSGroupBox
			// 
			this.NMFSGroupBox.Controls.Add(this.NMFSHeaderGrid);
			this.NMFSGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NMFSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NMFSGroupBox.Name = "NMFSGroupBox";
			this.NMFSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 153, true);
			this.NMFSGroupBox.TabIndex = 1;
			this.NMFSGroupBox.TabStop = false;
			this.NMFSGroupBox.Text = "NMFS - National Oceanic and Atmospheric Administration, National Marine Fisheries" +
    "";
			// 
			// NMFSHeaderGrid
			// 
			this.NMFSHeaderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NMFSHeaderGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.NMFSLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_ProgramType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_ProcessingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_DocumentTypeDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_IFTPPermitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_EBCDNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_VesselCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_HarvestedCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_GeographicLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_UnitOfMeasure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_CatchDocument)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_ReExportNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_DISDocumentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.NMFSLine)(null)).US_DISDocumentIDDesc)));
			this.NMFSHeaderGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_ProgramType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_ProcessingType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "US_DocumentType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_DocumentTypeDesc";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_IFTPPermitNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_EBCDNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_VesselCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "US_HarvestedCountry";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "US_GeographicLocation";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "US_Quantity";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("289586f5-c335-4c2d-8d1e-f4fce172ea5e", "Total Weight");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "US_UnitOfMeasure";
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.US.GUI.Res.GetData("289586f5-c335-4c2d-8d1e-f4fce172ea5e", "Total Weight");
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "US_CatchDocument";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "US_ReExportNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("23d0089b-14a7-4026-a8fa-8c612ed0cb9f", "DIS Document ID");
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "US_DISDocumentID";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("da29c812-a197-482c-914a-c2b7a1abf9a1", "DIS Doc. Type Desc.");
			zTextBoxColumnStyleInfo6.ColumnName = "US_DISDocumentIDDesc";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.NMFSHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NMFSHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.NMFSHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.NMFSHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NMFSHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NMFSHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NMFSHeaderGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.NMFSHeaderGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.NMFSHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.NMFSHeaderGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.NMFSHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.NMFSHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.NMFSHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.NMFSHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.NMFSHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.NMFSHeaderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NMFSHeaderGrid.GridId = "e6136d86-a462-4758-a4a0-973dd57f1c09";
			this.NMFSHeaderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NMFSHeaderGrid.LayoutKey = "NMFSHeaderGrid";
			this.NMFSHeaderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.NMFSHeaderGrid.Name = "NMFSHeaderGrid";
			this.NMFSHeaderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 136, true);
			this.NMFSHeaderGrid.TabIndex = 0;
			// 
			// ExportNMFSUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NMFSGroupBox);
			this.Name = "ExportNMFSUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 153, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NMFSGroupBox.ResumeLayout(false);
			this.NMFSGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NMFSHeaderGrid)).EndInit();
			this.NMFSHeaderGrid.ResumeLayout(false);
			this.NMFSHeaderGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox NMFSGroupBox;
		private ZArchitecture.ZGrid NMFSHeaderGrid;
	}
}
