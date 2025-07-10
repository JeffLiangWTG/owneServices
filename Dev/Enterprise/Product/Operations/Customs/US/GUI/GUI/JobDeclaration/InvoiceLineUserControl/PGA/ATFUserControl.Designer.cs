namespace Enterprise.Customs.US.GUI
{
	partial class ATFUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.ATFGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ATFGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ATFGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ATFGrid)).BeginInit();
			this.ATFGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ATFCollection);
			// 
			// ATFGroupBox
			// 
			this.ATFGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c0cf48a6-728c-47d5-96c5-4acfe5511e85", "Bureau of Alcohol, Tobacco, Firearms and Explosives - ATF");
			this.ATFGroupBox.Controls.Add(this.ATFGrid);
			this.ATFGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ATFGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ATFGroupBox.Name = "ATFGroupBox";
			this.ATFGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			this.ATFGroupBox.TabIndex = 0;
			this.ATFGroupBox.TabStop = false;
			this.ATFGroupBox.Text = "Bureau of Alcohol, Tobacco, Firearms and Explosives - ATF";
			// 
			// ATFGrid
			// 
			this.ATFGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ATFGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ATF)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ATF)(null)).US_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_CategoryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ATF)(null)).US_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_FFLNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_FFLExemptionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_FELNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_FELExemptionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_PermitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_PermitExemptionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_AECANumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_AECAExemptionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_Model)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_CaliberGaugeSize)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ATF)(null)).US_BarrelLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ATF)(null)).US_OverallLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_ExtendedDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_MunitionsListCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ATF)(null)).US_FFLExpirationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ATF)(null)).US_AECAExpirationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_TrackingStatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).StatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ATF)(null)).StatusDate)));
			this.ATFGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("043f1fea-0ea3-428e-a1d5-276283e0f531", "Line No.");
			zCalcEditColumnStyleInfo1.ColumnName = "US_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("36fa957f-2642-4206-8e9e-df7a15030773", "Category");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_CategoryCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e99d21fc-936f-4e12-a2f7-7fe074a31109", "Quantity");
			zCalcEditColumnStyleInfo2.ColumnName = "US_Quantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6bcebb64-5aac-460c-879c-36ec7415fa86", "FFL Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_FFLNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3021eea8-b7fd-4c40-befd-2145705d3a1c", "FFL Exempt.");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "US_FFLExemptionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3547e833-d756-4329-91ba-0af0b973a934", "FEL Number");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_FELNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fc3d3fe6-89c2-48f3-bed3-2675c91a90e0", "FEL Exempt.");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "US_FELExemptionCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a06da757-fd8d-47c0-a128-bab596f28aa3", "Permit Number");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_PermitNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("dd9bfa20-3bd9-4d5a-80cd-b7bad42aecd3", "Permit Exempt.");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "US_PermitExemptionCode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo4.Caption = "AECA Number";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "US_AECANumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.Caption = "";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0a312c53-edce-4f33-834e-f2a86f0ee9c0", "AECA Exempt.");
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "US_AECAExemptionCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1c4522c2-9367-4da0-b19b-a1e5fd0a6245", "Model");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "US_Model";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.Caption = "Caliber Gauge Or Size";
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "US_CaliberGaugeSize";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a4b9ddc1-c50d-44e9-80ca-629fc92d3b6b", "Barrel (in.)");
			zCalcEditColumnStyleInfo3.ColumnName = "US_BarrelLength";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.US.GUI.Res.GetData("2fb938e1-b0eb-4840-86bc-b02d10184ca7", "Barrel Length");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("dee9d8bc-cb62-460f-8782-8d9addb3a70b", "Overall (in.)");
			zCalcEditColumnStyleInfo4.ColumnName = "US_OverallLength";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.US.GUI.Res.GetData("453dcf2c-698a-415a-8676-0e7b852d5a88", "Overall Length");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eb0d5568-d435-4b08-a400-d53c4e9e2cb7", "Extended Description");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "US_ExtendedDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("45425d10-612b-4b01-aae6-646d6349087b", "Munitions List Category");
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo6.ColumnName = "US_MunitionsListCategory";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b1fc10d3-1edf-48d8-981b-152f0c291c66", "FFL Expiration Date");
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDateEditColumnStyleInfo1.ColumnName = "US_FFLExpirationDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0b467212-faf7-4d68-8af8-46034d8392db", "AECA Expiration Date");
			zDateEditColumnStyleInfo2.ColumnName = "US_AECAExpirationDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ec4f3d60-74fd-4379-a3a3-a7194bfff2b8", "Message Status");
			zTextBoxColumnStyleInfo8.ColumnName = "US_TrackingStatusDesc";
			zTextBoxColumnStyleInfo8.IsMandatory = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("da123a45-3e3a-4ff7-ba5e-6c7dffdafcc9", "PGA Line Status");
			zTextBoxColumnStyleInfo9.ColumnName = "Status";
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.US.GUI.Res.GetData("2c6bbcf6-ae81-41bd-9b2e-5fedb985e339", "PGA Line Status");
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("24913121-6d45-44fd-b515-8a8ffae24bef", "PGA Line Status Desc.");
			zTextBoxColumnStyleInfo10.ColumnName = "StatusDesc";
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.US.GUI.Res.GetData("2c6bbcf6-ae81-41bd-9b2e-5fedb985e339", "PGA Line Status");
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4901b0d4-3952-4951-bd7c-e9ce7cf9e264", "PGA Line Status Date");
			zDateEditColumnStyleInfo3.ColumnName = "StatusDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.ATFGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ATFGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ATFGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ATFGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ATFGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ATFGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ATFGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ATFGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ATFGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ATFGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ATFGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ATFGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ATFGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ATFGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ATFGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ATFGrid.GridId = "4f9fa729-2a55-4735-a14c-85ec92e0c27d";
			this.ATFGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ATFGrid.LayoutKey = "ATFGrid";
			this.ATFGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ATFGrid.Name = "ATFGrid";
			this.ATFGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 291, true);
			this.ATFGrid.TabIndex = 0;
			// 
			// ATFUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ATFGroupBox);
			this.Name = "ATFUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ATFGroupBox.ResumeLayout(false);
			this.ATFGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ATFGrid)).EndInit();
			this.ATFGrid.ResumeLayout(false);
			this.ATFGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ATFGroupBox;
		internal ZArchitecture.ZGrid ATFGrid;
	}
}
