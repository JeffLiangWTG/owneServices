namespace Enterprise.Customs.US.GUI
{
	partial class LiquidationsUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.LiquidationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LiquidationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LiquidationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LiquidationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusLiquidationCollection);
			// 
			// LiquidationsGroupBox
			// 
			this.LiquidationsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bca3b4bc-5f96-4b89-8ad1-242d5ca61529", "Liquidations");
			this.LiquidationsGroupBox.Controls.Add(this.LiquidationsGrid);
			this.LiquidationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LiquidationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LiquidationsGroupBox.Name = "LiquidationsGroupBox";
			this.LiquidationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 265, true);
			this.LiquidationsGroupBox.TabIndex = 1;
			this.LiquidationsGroupBox.TabStop = false;
			// 
			// LiquidationsGrid
			// 
			this.LiquidationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LiquidationsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.CusLiquidation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_EntryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).LiquidationTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ChangeLiquidationReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).ChangeLiquidationReasonCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).ImporterOfRecordNumberForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_CustomsDocumentFilingLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ExtensionSuspensionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).ExtensionSuspensionCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_ExtensionSuspensionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_NoOfSuspensions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_InterestAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_DutyPaid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidatedDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_LiquidatedTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TaxPaid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalLiquidatedFees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalPaidFees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalLiquidatedAntiDumpingDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalLiquidatedCounterVailingDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalPaidAntiDumpingDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusLiquidation)(null)).B8_TotalPaidCounterVailingDuty)));
			this.LiquidationsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d568f348-58e5-411e-919c-e365637769ae", "Entry Date");
			zDateEditColumnStyleInfo1.ColumnName = "B8_EntryDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.GroupName = null;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cea5f56a-259b-4e68-9d13-111afd0692e2", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "B8_LiquidationType";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("72019042-ff78-4648-9ae4-981efb7dba18", "Type Description");
			zTextBoxColumnStyleInfo2.ColumnName = "LiquidationTypeDescription";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("22c4912f-aca1-4e4d-ac03-6921bf387b80", "Date");
			zDateEditColumnStyleInfo2.ColumnName = "B8_LiquidationDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.GroupName = null;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6a345ee0-b65e-43fc-bc2c-f0d67d3df030", "Change Reason Code");
			zTextBoxColumnStyleInfo3.ColumnName = "B8_ChangeLiquidationReasonCode";
			zTextBoxColumnStyleInfo3.GroupName = null;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fb5b8d96-c7a6-4610-b0f9-dc22afe363fa", "Change Reason Code Description");
			zTextBoxColumnStyleInfo4.ColumnName = "ChangeLiquidationReasonCodeDescription";
			zTextBoxColumnStyleInfo4.GroupName = null;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6590ff22-0a79-4d5f-8dbe-39ba01221eb4", "Importer Of Record No");
			zTextBoxColumnStyleInfo5.ColumnName = "ImporterOfRecordNumberForDisplay";
			zTextBoxColumnStyleInfo5.GroupName = null;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("daba56c6-57c2-405d-958c-0df959e0602f", "Document Filing Location");
			zTextBoxColumnStyleInfo6.ColumnName = "B8_CustomsDocumentFilingLocation";
			zTextBoxColumnStyleInfo6.GroupName = null;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0988ab97-6da9-4193-a41b-2aa60c01ba06", "Sus Code", "Suspension Code", "");
			zTextBoxColumnStyleInfo7.ColumnName = "B8_ExtensionSuspensionCode";
			zTextBoxColumnStyleInfo7.GroupName = null;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bd5b1835-8569-4154-9474-571ceb0b3059", "Sus Code Description", "Suspension Code Description", "");
			zTextBoxColumnStyleInfo8.ColumnName = "ExtensionSuspensionCodeDescription";
			zTextBoxColumnStyleInfo8.GroupName = null;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("064e1c85-6575-41fe-8fe2-0fb8b91e0218", "Sus Date", "Suspension Date", "");
			zDateEditColumnStyleInfo3.ColumnName = "B8_ExtensionSuspensionDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.GroupName = null;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("abb42f1f-2c4e-4998-a930-5f9e59d5e1df", "No Of Sus", "Number Of Suspensions", "");
			zCalcEditColumnStyleInfo1.ColumnName = "B8_NoOfSuspensions";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = null;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4c99605b-684c-4303-a765-9714f0bebaa7", "Interest Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "B8_InterestAmount";
			zCalcEditColumnStyleInfo2.GroupName = null;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("13b901dc-281c-4f9b-963d-8f4ba14e212a", "Duty Paid");
			zCalcEditColumnStyleInfo3.ColumnName = "B8_DutyPaid";
			zCalcEditColumnStyleInfo3.GroupName = null;
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b68bd202-baf9-48a9-b838-0e7ef6c9b8a0", "Liquidated Duty");
			zCalcEditColumnStyleInfo4.ColumnName = "B8_LiquidatedDuty";
			zCalcEditColumnStyleInfo4.GroupName = null;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2f00b42d-8eb5-4f3d-a829-707359178bff", "Liquidated Tax");
			zCalcEditColumnStyleInfo5.ColumnName = "B8_LiquidatedTax";
			zCalcEditColumnStyleInfo5.GroupName = null;
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("21fecc79-12cc-456f-be6a-c6fe50500122", "Tax Paid");
			zCalcEditColumnStyleInfo6.ColumnName = "B8_TaxPaid";
			zCalcEditColumnStyleInfo6.GroupName = null;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.Caption = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("005d9c54-5a67-40bc-98a9-8aa5f0dfc911", "Total Liquidated Fees");
			zCalcEditColumnStyleInfo7.ColumnName = "B8_TotalLiquidatedFees";
			zCalcEditColumnStyleInfo7.GroupName = null;
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.Caption = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d81cf9d1-d292-4f35-8a82-88027e051f05", "Total Paid Fees");
			zCalcEditColumnStyleInfo8.ColumnName = "B8_TotalPaidFees";
			zCalcEditColumnStyleInfo8.GroupName = null;
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.Caption = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d652e56e-31d8-4350-97a9-3ffb2f9c2e78", "Total Liquidated AD Duty", "Total Liquidated Anti Dumping Duty", "");
			zCalcEditColumnStyleInfo9.ColumnName = "B8_TotalLiquidatedAntiDumpingDuty";
			zCalcEditColumnStyleInfo9.GroupName = null;
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.Caption = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fa7f9525-4012-46cd-8b55-bc2e9ddf4550", "Total Liquidated CV Duty", "Total Liquidated Countervailing Duty", "");
			zCalcEditColumnStyleInfo10.ColumnName = "B8_TotalLiquidatedCounterVailingDuty";
			zCalcEditColumnStyleInfo10.GroupName = null;
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.Caption = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c09008c9-f598-43f8-83f7-013f3ff56a32", "Total Paid AD Duty", "Total Paid Anti Dumping Duty", "");
			zCalcEditColumnStyleInfo11.ColumnName = "B8_TotalPaidAntiDumpingDuty";
			zCalcEditColumnStyleInfo11.GroupName = null;
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.Caption = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("07850337-c01e-4f06-84e4-11b6e305a82d", "Total Paid CV Duty", "Total Paid Countervailing Duty", "");
			zCalcEditColumnStyleInfo12.ColumnName = "B8_TotalPaidCounterVailingDuty";
			zCalcEditColumnStyleInfo12.GroupName = null;
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			this.LiquidationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LiquidationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LiquidationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LiquidationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.LiquidationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.LiquidationsGrid.CopySelectedRowsAllowed = true;
			this.LiquidationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LiquidationsGrid.GridId = "da40e247-f1dc-4890-a8c0-05280e956156";
			this.LiquidationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LiquidationsGrid.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.LiquidationsGrid.LayoutKey = "LiquidationsGrid";
			this.LiquidationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LiquidationsGrid.Name = "LiquidationsGrid";
			this.LiquidationsGrid.ReadOnly = true;
			this.LiquidationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 246, true);
			this.LiquidationsGrid.TabIndex = 1;
			// 
			// LiquidationsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LiquidationsGroupBox);
			this.Name = "LiquidationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LiquidationsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LiquidationsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox LiquidationsGroupBox;
		private ZArchitecture.ZGrid LiquidationsGrid;

	}
}
