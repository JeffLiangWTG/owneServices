namespace Enterprise.Customs.US.GUI
{
	partial class EG1UserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.EG1GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EG1Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EG1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EG1Grid)).BeginInit();
			this.EG1Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMSLineCollection);
			// 
			// EG1GroupBox
			// 
			this.EG1GroupBox.Controls.Add(this.EG1Grid);
			this.EG1GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EG1GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EG1GroupBox.Name = "EG1GroupBox";
			this.EG1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			this.EG1GroupBox.TabIndex = 0;
			this.EG1GroupBox.TabStop = false;
			this.EG1GroupBox.Text = "EG1 Details";
			// 
			// EG1Grid
			// 
			this.EG1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EG1Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_ProductNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).ApplicantOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OA_Applicant)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).GoodsLocationOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OA_GoodsLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OuterPackage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OuterPackageUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InnerPackage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InnerPackageUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InnerAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InnerAmountUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InnerWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InnerWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_TotalWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_TotalQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_TotalQuantityUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InspecDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_InspecRemarks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_IsDocSubmitted)));
			this.EG1Grid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("76412148-3627-4e50-9cba-fec40389c281", "Product Number");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_ProductNumber";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f23d1522-688a-496e-8836-2c7682b4bfc0", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ApplicantOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("27222b60-8bd5-43fa-9247-fcdc7c2ea6f3", "Applicant");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7be98d52-777a-44a7-b146-344494ce6222", "Address");
			zAddressDropEditColumnStyleInfo1.ColumnName = "US_OA_Applicant";
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("27222b60-8bd5-43fa-9247-fcdc7c2ea6f3", "Applicant");
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("55a671c9-c18f-4c5d-835d-ee49b08f2828", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "GoodsLocationOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("45959340-af00-4e69-ac8f-6a901bf5a794", "Goods Location");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8df8eb00-c681-4b9c-b95b-1805b87c8e06", "Address");
			zAddressDropEditColumnStyleInfo2.ColumnName = "US_OA_GoodsLocation";
			zAddressDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("45959340-af00-4e69-ac8f-6a901bf5a794", "Goods Location");
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("83f8586a-36ab-4fb6-af0e-7292c577e63f", "Outer Package");
			zCalcEditColumnStyleInfo1.ColumnName = "US_OuterPackage";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("92d0f3fc-a2ac-4ae9-b0d9-0621a7aed4e6", "Outer Package");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0be1765a-07d3-4ca3-96f0-ce3d4845ddb3", "UQ");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_OuterPackageUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("92d0f3fc-a2ac-4ae9-b0d9-0621a7aed4e6", "Outer Package");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5459c66c-82df-433d-9f47-6f83eef03431", "Inner Package");
			zCalcEditColumnStyleInfo2.ColumnName = "US_InnerPackage";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("cad1374b-e8c2-4d84-a87f-a3abbe934b8f", "Inner Package");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ac07b270-603e-45e2-be47-62ee99b32a4c", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "US_InnerPackageUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("cad1374b-e8c2-4d84-a87f-a3abbe934b8f", "Inner Package");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d1db542a-fbee-42dc-957b-adcf51d2535b", "Amount Of Product In Inner Package");
			zCalcEditColumnStyleInfo3.ColumnName = "US_InnerAmount";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("697a8b3b-262b-401f-bbb7-e4c9948a52d2", "Amount Of Product In Inner Package");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cfd07975-63c0-4002-8b96-e8bf6e7e2345", "UQ");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "US_InnerAmountUQ";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.GUI.Res.GetData("697a8b3b-262b-401f-bbb7-e4c9948a52d2", "Amount Of Product In Inner Package");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6200d5a5-423d-4f64-b4fe-8b0a5d9980eb", "Inner Package Weight");
			zCalcEditColumnStyleInfo4.ColumnName = "US_InnerWeight";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.GUI.Res.GetData("7dc6d9ab-fead-492e-922e-d51b07941221", "Inner Package Weight");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("094ba5ae-24df-4a91-9a1b-d768549713cc", "UQ");
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "US_InnerWeightUQ";
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.US.GUI.Res.GetData("7dc6d9ab-fead-492e-922e-d51b07941221", "Inner Package Weight");
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("72e7e675-ad60-4af6-8894-85b41eac61ed", "Total Weight");
			zCalcEditColumnStyleInfo5.ColumnName = "US_TotalWeight";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6369634f-58c1-4407-bc0b-6edf53c06399", "Total Weight");
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("65dc5d39-e967-47d0-99cb-1fd0ca654ac1", "UQ");
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "US_TotalWeightUQ";
			zDropEditColumnStyleInfo6.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6369634f-58c1-4407-bc0b-6edf53c06399", "Total Weight");
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cec82941-b19c-4c71-9b32-28c2e8c23ab8", "Total Quantity");
			zCalcEditColumnStyleInfo6.ColumnName = "US_TotalQuantity";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.US.GUI.Res.GetData("5caac116-b93f-4b35-9d2b-d78c9a9e37d7", "Total Quantity");
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2e09a3cf-57b6-4f1a-8c41-40932b6362db", "UQ");
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "US_TotalQuantityUQ";
			zDropEditColumnStyleInfo7.GroupName = Enterprise.Customs.US.GUI.Res.GetData("5caac116-b93f-4b35-9d2b-d78c9a9e37d7", "Total Quantity");
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7c530b99-a7e8-4ea2-9e75-7591e5e660e0", "Inspect Date/Time");
			zDateEditColumnStyleInfo1.ColumnName = "US_InspecDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6ee45a1c-6185-4d55-b22c-53c67cc704fa", "Remarks");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_InspecRemarks";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.Caption = "Doc. Submitted";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5941a333-c091-4183-8806-933ff224613a", "Document Submitted");
			zCheckBoxColumnStyleInfo1.ColumnName = "US_IsDocSubmitted";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EG1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EG1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.EG1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.EG1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.EG1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.EG1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EG1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EG1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EG1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EG1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EG1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EG1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EG1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.EG1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.EG1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.EG1Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.EG1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.EG1Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EG1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EG1Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EG1Grid.CopySelectedRowsAllowed = true;
			this.EG1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EG1Grid.GridId = "98c4e8eb-5325-45b8-a3dc-96f0d62b7663";
			this.EG1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EG1Grid.LayoutKey = "EG1Grid";
			this.EG1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EG1Grid.Name = "EG1Grid";
			this.EG1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 267, true);
			this.EG1Grid.TabIndex = 0;
			// 
			// EG1UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.EG1GroupBox);
			this.Name = "EG1UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EG1GroupBox.ResumeLayout(false);
			this.EG1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EG1Grid)).EndInit();
			this.EG1Grid.ResumeLayout(false);
			this.EG1Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox EG1GroupBox;
		public ZArchitecture.ZGrid EG1Grid;
	}
}
