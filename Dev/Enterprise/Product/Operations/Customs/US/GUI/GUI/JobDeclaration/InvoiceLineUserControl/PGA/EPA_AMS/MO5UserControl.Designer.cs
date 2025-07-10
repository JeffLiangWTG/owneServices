namespace Enterprise.Customs.US.GUI
{
	partial class MO5UserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MO5Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO5Grid)).BeginInit();
			this.MO5Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMSLineCollection);
			// 
			// LinesGroupBox
			// 
			this.LinesGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c68ab556-8ddf-4576-a758-b18eba9b5c53", "MO5 Details");
			this.LinesGroupBox.Controls.Add(this.MO5Grid);
			this.LinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGroupBox.Name = "LinesGroupBox";
			this.LinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			this.LinesGroupBox.TabIndex = 0;
			this.LinesGroupBox.TabStop = false;
			// 
			// MO5Grid
			// 
			this.MO5Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MO5Grid, ".");
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
			this.MO5Grid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "AddInfoLookups.ProductNumberCodes";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("40a1bce2-9d55-49b7-9bfe-b8262a4899e2", "Product Number");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_ProductNumber";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0270b9be-2306-46ca-be74-730582d4deb4", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ApplicantOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("44347c34-1f5f-44c4-8ff7-5ff5de37f93f", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("50755145-45aa-47aa-a8b1-f3d7987b3efa", "Address");
			zAddressDropEditColumnStyleInfo1.ColumnName = "US_OA_Applicant";
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("44347c34-1f5f-44c4-8ff7-5ff5de37f93f", "Applicant");
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9e0a2248-110b-4a5d-9161-a564d7540a1d", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "GoodsLocationOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6b68449a-e362-4a98-938f-53c51cec8cc2", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0ed24e09-a10d-4aba-9d7a-5f18a3c979b6", "Address");
			zAddressDropEditColumnStyleInfo2.ColumnName = "US_OA_GoodsLocation";
			zAddressDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6b68449a-e362-4a98-938f-53c51cec8cc2", "Goods Location");
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Package";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8bb271ae-983d-44e1-9d25-39f4e771c2e5", "Package");
			zCalcEditColumnStyleInfo1.ColumnName = "US_Packages";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("688ce0ed-5beb-469a-ba7e-6a5f8d8ad33b", "Package");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.Caption = "UQ";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_PackagesUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("688ce0ed-5beb-469a-ba7e-6a5f8d8ad33b", "Package");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Weight Per Package";
			zCalcEditColumnStyleInfo2.ColumnName = "US_PackageWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("29932245-64e4-49ad-8ee3-bb4124556868", "Package Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f5eb60ec-21e7-419f-a063-d049a2f04fd2", "UQ");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_PackageWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("29932245-64e4-49ad-8ee3-bb4124556868", "Package Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Net Weight";
			zCalcEditColumnStyleInfo3.ColumnName = "US_NetWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ab30c863-ddd6-4bdd-a1ea-02e5c2c758df", "Net Weight");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("38538f6f-4026-4d01-b160-3202c4c72279", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "US_NetWeightUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ab30c863-ddd6-4bdd-a1ea-02e5c2c758df", "Net Weight");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.Caption = "Inspec. Date/Time";
			zDateEditColumnStyleInfo1.ColumnName = "US_InspecDateTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo1.Caption = "Inspec. Remarks";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_InspecRemarks";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MO5Grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MO5Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.MO5Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.MO5Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.MO5Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.MO5Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MO5Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MO5Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MO5Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MO5Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MO5Grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.MO5Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MO5Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MO5Grid.CopySelectedRowsAllowed = true;
			this.MO5Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MO5Grid.GridId = "b7e01fa2-2bf6-4cbd-806c-2e31b6f96189";
			this.MO5Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MO5Grid.LayoutKey = "MO5Grid";
			this.MO5Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MO5Grid.Name = "MO5Grid";
			this.MO5Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 267, true);
			this.MO5Grid.TabIndex = 0;
			// 
			// MO5UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LinesGroupBox);
			this.Name = "MO5UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LinesGroupBox.ResumeLayout(false);
			this.LinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO5Grid)).EndInit();
			this.MO5Grid.ResumeLayout(false);
			this.MO5Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox LinesGroupBox;
		public ZArchitecture.ZGrid MO5Grid;
	}
}
