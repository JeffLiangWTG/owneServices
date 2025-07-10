namespace Enterprise.Customs.US.GUI
{
	partial class MO1UserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MO1GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MO1Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MO1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO1Grid)).BeginInit();
			this.MO1Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMSLineCollection);
			// 
			// MO1GroupBox
			// 
			this.MO1GroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d17e71eb-6e3f-4463-ac34-44bc62ae57bb", "MO1 Details");
			this.MO1GroupBox.Controls.Add(this.MO1Grid);
			this.MO1GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MO1GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MO1GroupBox.Name = "MO1GroupBox";
			this.MO1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			this.MO1GroupBox.TabIndex = 0;
			this.MO1GroupBox.TabStop = false;
			this.MO1GroupBox.Text = "MO1 Details";
			// 
			// MO1Grid
			// 
			this.MO1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MO1Grid, ".");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_QtyPerPackage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_QtyPerPackageUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InspecDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InspecRemarks)));
			this.MO1Grid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "AddInfoLookups.ProductNumberCodes";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("be4f8073-c3b8-4fb7-bca3-5c8bc8969a0a", "Product Number");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_ProductNumber";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3ddfdfc6-0bd3-4ab3-a02e-4e221267cb5c", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ApplicantOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("56e22e0c-2e51-46ed-8992-e3a7e642380a", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7986e44c-fe0b-40b4-abc5-df37b3feec36", "Address");
			zAddressDropEditColumnStyleInfo1.ColumnName = "US_OA_Applicant";
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("56e22e0c-2e51-46ed-8992-e3a7e642380a", "Applicant");
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1548e66e-00ff-435a-8c67-cf3e2975df48", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "GoodsLocationOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ead5ce49-835c-4af1-9f82-00e44b17b7b6", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1b622216-25b3-47f3-86df-c788aaa30726", "Address");
			zAddressDropEditColumnStyleInfo2.ColumnName = "US_OA_GoodsLocation";
			zAddressDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ead5ce49-835c-4af1-9f82-00e44b17b7b6", "Goods Location");
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("568b4f48-9cc1-4ece-a0fd-b8363b76924e", "No of Packages");
			zCalcEditColumnStyleInfo1.ColumnName = "US_Packages";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("a2be305b-e95d-4270-a39e-93b5d4ae6900", "No of Packages");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a7d77a60-f271-418c-a104-eff8c2548eea", "UQ");
			zDropEditColumnStyleInfo1.ColumnName = "US_PackagesUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("a2be305b-e95d-4270-a39e-93b5d4ae6900", "No of Packages");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("86a9f885-ffa0-4981-8f3b-8a21690eca61", "Weight Per Package");
			zCalcEditColumnStyleInfo2.ColumnName = "US_PackageWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("26247fdd-409b-448e-b9de-368e71918880", "Weight Per Package");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6fae4cb2-8a08-4e0c-af0f-39392b88253e", "UQ");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_PackageWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("26247fdd-409b-448e-b9de-368e71918880", "Weight Per Package");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9143fe30-0939-489f-a8b8-1dd76fc02505", "Qty. Per Package");
			zCalcEditColumnStyleInfo3.ColumnName = "US_QtyPerPackage";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("178ef128-d016-4a01-abe2-7a10f18cf6e6", "Qty. Per Package");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6682d0bc-01ac-41fb-9325-b9b8b9a89dcb", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "US_QtyPerPackageUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("178ef128-d016-4a01-abe2-7a10f18cf6e6", "Qty. Per Package");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Net Weight";
			zCalcEditColumnStyleInfo4.ColumnName = "US_NetWeight";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.GUI.Res.GetData("db8ae951-1842-475b-bda4-603913fa5f86", "Net Weight");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.Caption = "UQ";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "US_NetWeightUQ";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.GUI.Res.GetData("db8ae951-1842-475b-bda4-603913fa5f86", "Net Weight");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ccbd184c-6e07-4eb6-978c-f1088a8c9276", "Date/Time of Inspection");
			zDateEditColumnStyleInfo1.ColumnName = "US_InspecDateTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.Caption = "Remarks";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_InspecRemarks";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MO1Grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MO1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.MO1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.MO1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.MO1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.MO1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MO1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MO1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MO1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MO1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MO1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.MO1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.MO1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.MO1Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MO1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MO1Grid.CopySelectedRowsAllowed = true;
			this.MO1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MO1Grid.GridId = "ee4496a2-0476-4d40-aaf7-7d92f21aa829";
			this.MO1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MO1Grid.LayoutKey = "MO1Grid";
			this.MO1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MO1Grid.Name = "MO1Grid";
			this.MO1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 267, true);
			this.MO1Grid.TabIndex = 0;
			// 
			// MO1UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MO1GroupBox);
			this.Name = "MO1UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MO1GroupBox.ResumeLayout(false);
			this.MO1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO1Grid)).EndInit();
			this.MO1Grid.ResumeLayout(false);
			this.MO1Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MO1GroupBox;
		public ZArchitecture.ZGrid MO1Grid;
	}
}
