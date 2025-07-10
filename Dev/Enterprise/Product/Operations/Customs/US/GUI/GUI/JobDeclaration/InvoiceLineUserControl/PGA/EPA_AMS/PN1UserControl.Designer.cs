namespace Enterprise.Customs.US.GUI
{
	partial class PN1UserControl
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
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PN1Grid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.LotCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LotCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PN1Grid)).BeginInit();
			this.PN1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.LotCodeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LotCodesGrid)).BeginInit();
			this.LotCodesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMSLineCollection);
			// 
			// LinesGroupBox
			// 
			this.LinesGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3ee0041d-fbb7-45d2-a2f7-37653f905fa2", "PN1 Details");
			this.LinesGroupBox.Controls.Add(this.PN1Grid);
			this.LinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGroupBox.Name = "LinesGroupBox";
			this.LinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 219, true);
			this.LinesGroupBox.TabIndex = 1;
			this.LinesGroupBox.TabStop = false;
			this.LinesGroupBox.Text = "PN1 Details";
			// 
			// PN1Grid
			// 
			this.PN1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PN1Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_ProductNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)).AddInfoLookups.ProductNumberCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).ApplicantOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OA_Applicant)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).GoodsLocationOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OA_GoodsLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_PackagesUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_PackageWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_PackageWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InspecDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InspecRemarks)));
			this.PN1Grid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("782c65e1-c65b-4d07-a11d-fcd78450da24", "Product Number");
			zDropEditColumnStyleInfo1.BindToList = "AddInfoLookups.ProductNumberCodes";
			zDropEditColumnStyleInfo1.ColumnName = "US_ProductNumber";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("593eb891-b9e4-4bf2-8cf3-c1b5dbe5917b", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ApplicantOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("9fc9aa5a-82c5-4667-b8a2-2b23fecd7636", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4d7c86b0-ee72-4a75-a9d1-457d5a00b8e9", "Address");
			zAddressDropEditColumnStyleInfo1.ColumnName = "US_OA_Applicant";
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("9fc9aa5a-82c5-4667-b8a2-2b23fecd7636", "Applicant");
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6abc32e2-e708-44c9-828b-7b34f4fa3bc2", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "GoodsLocationOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6731afd9-dfde-4643-bb0b-4a7de06cce76", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("30ba836b-16a4-447a-bcf4-8dc79333cce5", "Address");
			zAddressDropEditColumnStyleInfo2.ColumnName = "US_OA_GoodsLocation";
			zAddressDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6731afd9-dfde-4643-bb0b-4a7de06cce76", "Goods Location");
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Package";
			zCalcEditColumnStyleInfo1.ColumnName = "US_Packages";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("26de3c55-73b9-484c-85ab-6240e5e35185", "Package");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.Caption = "";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eaeb4fd7-0d94-494b-8af0-8c8a8effafdb", "UQ");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_PackagesUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("26de3c55-73b9-484c-85ab-6240e5e35185", "Package");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e3e6e138-1ef3-4569-99ba-1ad6aa0dd10e", "Weight Per Package");
			zCalcEditColumnStyleInfo2.ColumnName = "US_PackageWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("08f45973-99e3-4dda-9c20-2e712f1d1d74", "Weight Per Package");
			zCalcEditColumnStyleInfo2.IsCustomColumn = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f2082a50-b48c-40ed-91f4-5627d659ba1c", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "US_PackageWeightUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("08f45973-99e3-4dda-9c20-2e712f1d1d74", "Weight Per Package");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "";
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("220c52a7-3828-4105-a34a-6a84f74cdb6f", "Net Weight");
			zCalcEditColumnStyleInfo3.ColumnName = "US_NetWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("9562e37b-7c03-42da-8dc6-d7e27f46adb3", "Net Weight");
			zCalcEditColumnStyleInfo3.IsCustomColumn = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2ede07df-a489-4bfc-b443-8e758f68feba", "UQ");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "US_NetWeightUQ";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.GUI.Res.GetData("9562e37b-7c03-42da-8dc6-d7e27f46adb3", "Net Weight");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.Caption = "";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("10d4269c-9ea4-4a9d-ada7-b414a549be34", "Inspect Date");
			zDateEditColumnStyleInfo1.ColumnName = "US_InspecDateTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c86361f0-1829-4846-a743-03d53a955146", "Inspect Remarks");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_InspecRemarks";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.PN1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PN1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.PN1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.PN1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.PN1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.PN1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PN1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PN1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PN1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PN1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PN1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PN1Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PN1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PN1Grid.CopySelectedRowsAllowed = true;
			this.PN1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PN1Grid.GridId = "b7e01fa2-2bf6-4cbd-806c-2e31b6f96189";
			this.PN1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PN1Grid.LayoutKey = "MO5Grid";
			this.PN1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PN1Grid.Name = "PN1Grid";
			this.PN1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(638, 200, true);
			this.PN1Grid.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.LinesGroupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.LotCodeGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 219, true);
			this.splitContainer1.SplitterDistance = 644;
			this.splitContainer1.TabIndex = 2;
			// 
			// LotCodeGroupBox
			// 
			this.LotCodeGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("97173f5c-08cc-4792-9451-e40448ee57d8", "Lot Details");
			this.LotCodeGroupBox.Controls.Add(this.LotCodesGrid);
			this.LotCodeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LotCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LotCodeGroupBox.Name = "LotCodeGroupBox";
			this.LotCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 219, true);
			this.LotCodeGroupBox.TabIndex = 0;
			this.LotCodeGroupBox.TabStop = false;
			this.LotCodeGroupBox.Text = "Lot Details";
			// 
			// LotCodesGrid
			// 
			this.LotCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LotCodesGrid, "LotCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)).LotCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLotCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)).LotCodes)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLotCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)).LotCodes)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLotCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)).LotCodes)).SyncRoot)).CY_Data)));
			this.LotCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6cc88158-fb8b-4501-b455-881214687c5d", "Lot Number Qualifier");
			zDropEditColumnStyleInfo5.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("734d2256-9363-48ec-89bb-db5f08534e1b", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d028af3c-cc7a-40d9-b92e-ea8259d1c6bc", "Lot Number");
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo3.IsCustomColumn = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LotCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.LotCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LotCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LotCodesGrid.CopySelectedRowsAllowed = true;
			this.LotCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LotCodesGrid.GridId = "b7e01fa2-2bf6-4cbd-806c-2e31b6f96189";
			this.LotCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LotCodesGrid.LayoutKey = "MO5Grid";
			this.LotCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LotCodesGrid.Name = "LotCodesGrid";
			this.LotCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 200, true);
			this.LotCodesGrid.TabIndex = 1;
			// 
			// PN1UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.splitContainer1);
			this.Name = "PN1UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 219, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LinesGroupBox.ResumeLayout(false);
			this.LinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PN1Grid)).EndInit();
			this.PN1Grid.ResumeLayout(false);
			this.PN1Grid.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.LotCodeGroupBox.ResumeLayout(false);
			this.LotCodeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LotCodesGrid)).EndInit();
			this.LotCodesGrid.ResumeLayout(false);
			this.LotCodesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox LinesGroupBox;
		internal System.Windows.Forms.SplitContainer splitContainer1;
		internal ZArchitecture.GUI.ZGroupBox LotCodeGroupBox;
		public ZArchitecture.ZGrid PN1Grid;
		internal ZArchitecture.ZGrid LotCodesGrid;
	}
}
