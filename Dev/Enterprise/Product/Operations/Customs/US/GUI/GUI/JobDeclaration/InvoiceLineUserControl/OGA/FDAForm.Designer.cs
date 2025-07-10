namespace Enterprise.Customs.US.GUI
{
	partial class FDAForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.FDAQuantitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FDAQtyRunningTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.US_FDAQty6CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.US_FDAQty5CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.US_FDAQty4CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.US_FDAQty3CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.US_FDAQty2CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.US_FDAQty1CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ContainerDimensionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DimUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_ContainerDim3CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.US_ContainerDim2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.US_ContainerDim1CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.US_FDAContainerDimTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AffirmationCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AffirmationCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.US_UC_NKFDAProductionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.US_FDACommercialDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_TradeBrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_FDAProductCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.US_FDACargoStorageCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FDAValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.USDCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FDACurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvCurrFDAValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShipperAddressAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FEINoAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ManufacturerAddressAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PriorNoticeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OtherLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PFRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufacturerProducerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExemptDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ForcePNCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ContainersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PFTDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_SFRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSHCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OFTDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FDAQuantitiesGroupBox.SuspendLayout();
			this.ContainerDimensionsGroupBox.SuspendLayout();
			this.AffirmationCodeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AffirmationCodesGrid)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.PriorNoticeGroupBox.SuspendLayout();
			this.BillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).BeginInit();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 645, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.FDA);
			// 
			// FDAQuantitiesGroupBox
			// 
			this.FDAQuantitiesGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f14b2c19-1e63-479b-9028-22eb63761afe", "FDA Quantities");
			this.FDAQuantitiesGroupBox.Controls.Add(this.FDAQtyRunningTotalLabel);
			this.FDAQuantitiesGroupBox.Controls.Add(this.US_FDAQty6CalcDropEdit);
			this.FDAQuantitiesGroupBox.Controls.Add(this.US_FDAQty5CalcDropEdit);
			this.FDAQuantitiesGroupBox.Controls.Add(this.US_FDAQty4CalcDropEdit);
			this.FDAQuantitiesGroupBox.Controls.Add(this.US_FDAQty3CalcDropEdit);
			this.FDAQuantitiesGroupBox.Controls.Add(this.US_FDAQty2CalcDropEdit);
			this.FDAQuantitiesGroupBox.Controls.Add(this.US_FDAQty1CalcDropEdit);
			this.FDAQuantitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 253, true);
			this.FDAQuantitiesGroupBox.Name = "FDAQuantitiesGroupBox";
			this.FDAQuantitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 167, true);
			this.FDAQuantitiesGroupBox.TabIndex = 1;
			this.FDAQuantitiesGroupBox.TabStop = false;
			// 
			// FDAQtyRunningTotalLabel
			// 
			this.BindingSource.SetBindingMember(this.FDAQtyRunningTotalLabel, "FDAQtyRunningTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).FDAQtyRunningTotal)));
			this.FDAQtyRunningTotalLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FDAQtyRunningTotalLabel, false);
			this.FDAQtyRunningTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 28, true);
			this.FDAQtyRunningTotalLabel.Name = "FDAQtyRunningTotalLabel";
			this.FDAQtyRunningTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 13, true);
			this.FDAQtyRunningTotalLabel.TabIndex = 6;
			this.FDAQtyRunningTotalLabel.Text = "Running Total";
			this.FDAQtyRunningTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// US_FDAQty6CalcDropEdit
			// 
			this.US_FDAQty6CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAQty6CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAQty6)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAMeasure6)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.FDAUQs)));
			this.US_FDAQty6CalcDropEdit.BindToAmount = "US_FDAQty6";
			this.US_FDAQty6CalcDropEdit.BindToList = "AddInfoLookups+FDAUQs";
			this.US_FDAQty6CalcDropEdit.BindToUnit = "US_FDAMeasure6";
			this.US_FDAQty6CalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("48dc3435-eaa9-4e51-965f-f28673701013", "Next Qty");
			this.US_FDAQty6CalcDropEdit.Decimals = 2;
			this.US_FDAQty6CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 139, true);
			this.US_FDAQty6CalcDropEdit.Name = "US_FDAQty6CalcDropEdit";
			this.US_FDAQty6CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.US_FDAQty6CalcDropEdit.TabIndex = 5;
			this.US_FDAQty6CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// US_FDAQty5CalcDropEdit
			// 
			this.US_FDAQty5CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAQty5CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAQty5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAMeasure5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.FDAUQs)));
			this.US_FDAQty5CalcDropEdit.BindToAmount = "US_FDAQty5";
			this.US_FDAQty5CalcDropEdit.BindToList = "AddInfoLookups+FDAUQs";
			this.US_FDAQty5CalcDropEdit.BindToUnit = "US_FDAMeasure5";
			this.US_FDAQty5CalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("28264508-273a-4304-a9ad-e389b2376ecb", "Next Qty");
			this.US_FDAQty5CalcDropEdit.Decimals = 2;
			this.US_FDAQty5CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 116, true);
			this.US_FDAQty5CalcDropEdit.Name = "US_FDAQty5CalcDropEdit";
			this.US_FDAQty5CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.US_FDAQty5CalcDropEdit.TabIndex = 4;
			this.US_FDAQty5CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// US_FDAQty4CalcDropEdit
			// 
			this.US_FDAQty4CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAQty4CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAQty4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAMeasure4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.FDAUQs)));
			this.US_FDAQty4CalcDropEdit.BindToAmount = "US_FDAQty4";
			this.US_FDAQty4CalcDropEdit.BindToList = "AddInfoLookups+FDAUQs";
			this.US_FDAQty4CalcDropEdit.BindToUnit = "US_FDAMeasure4";
			this.US_FDAQty4CalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b68eca1a-8e93-4475-a0f4-7281e617598c", "Next Qty");
			this.US_FDAQty4CalcDropEdit.Decimals = 2;
			this.US_FDAQty4CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 93, true);
			this.US_FDAQty4CalcDropEdit.Name = "US_FDAQty4CalcDropEdit";
			this.US_FDAQty4CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.US_FDAQty4CalcDropEdit.TabIndex = 3;
			this.US_FDAQty4CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// US_FDAQty3CalcDropEdit
			// 
			this.US_FDAQty3CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAQty3CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAQty3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAMeasure3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.FDAUQs)));
			this.US_FDAQty3CalcDropEdit.BindToAmount = "US_FDAQty3";
			this.US_FDAQty3CalcDropEdit.BindToList = "AddInfoLookups+FDAUQs";
			this.US_FDAQty3CalcDropEdit.BindToUnit = "US_FDAMeasure3";
			this.US_FDAQty3CalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bd0d9bba-192c-4578-997f-d02e6d8dbf0a", "Next Qty");
			this.US_FDAQty3CalcDropEdit.Decimals = 2;
			this.US_FDAQty3CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 70, true);
			this.US_FDAQty3CalcDropEdit.Name = "US_FDAQty3CalcDropEdit";
			this.US_FDAQty3CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.US_FDAQty3CalcDropEdit.TabIndex = 2;
			this.US_FDAQty3CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// US_FDAQty2CalcDropEdit
			// 
			this.US_FDAQty2CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAQty2CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAQty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAMeasure2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.FDAUQs)));
			this.US_FDAQty2CalcDropEdit.BindToAmount = "US_FDAQty2";
			this.US_FDAQty2CalcDropEdit.BindToList = "AddInfoLookups+FDAUQs";
			this.US_FDAQty2CalcDropEdit.BindToUnit = "US_FDAMeasure2";
			this.US_FDAQty2CalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ede90ba0-14ae-49ae-b04b-990a8840d408", "Next Qty");
			this.US_FDAQty2CalcDropEdit.Decimals = 2;
			this.US_FDAQty2CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 47, true);
			this.US_FDAQty2CalcDropEdit.Name = "US_FDAQty2CalcDropEdit";
			this.US_FDAQty2CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.US_FDAQty2CalcDropEdit.TabIndex = 1;
			this.US_FDAQty2CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// US_FDAQty1CalcDropEdit
			// 
			this.US_FDAQty1CalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAQty1CalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAQty1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAMeasure1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.FDABaseUQs)));
			this.US_FDAQty1CalcDropEdit.BindToAmount = "US_FDAQty1";
			this.US_FDAQty1CalcDropEdit.BindToList = "AddInfoLookups+FDABaseUQs";
			this.US_FDAQty1CalcDropEdit.BindToUnit = "US_FDAMeasure1";
			this.US_FDAQty1CalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a08c7485-7b20-4caf-bc89-9554c6cc4792", "Base Qty");
			this.US_FDAQty1CalcDropEdit.Decimals = 2;
			this.US_FDAQty1CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 24, true);
			this.US_FDAQty1CalcDropEdit.Name = "US_FDAQty1CalcDropEdit";
			this.US_FDAQty1CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.US_FDAQty1CalcDropEdit.TabIndex = 0;
			this.US_FDAQty1CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ContainerDimensionsGroupBox
			// 
			this.ContainerDimensionsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c1fb6842-8d5a-438e-8bc7-3df999586151", "Container Dimensions");
			this.ContainerDimensionsGroupBox.Controls.Add(this.DimUQDropEdit);
			this.ContainerDimensionsGroupBox.Controls.Add(this.US_ContainerDim3CalcEdit);
			this.ContainerDimensionsGroupBox.Controls.Add(this.US_ContainerDim2CalcEdit);
			this.ContainerDimensionsGroupBox.Controls.Add(this.US_ContainerDim1CalcEdit);
			this.ContainerDimensionsGroupBox.Controls.Add(this.US_FDAContainerDimTypeDropEdit);
			this.ContainerDimensionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 426, true);
			this.ContainerDimensionsGroupBox.Name = "ContainerDimensionsGroupBox";
			this.ContainerDimensionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 84, true);
			this.ContainerDimensionsGroupBox.TabIndex = 2;
			this.ContainerDimensionsGroupBox.TabStop = false;
			// 
			// DimUQDropEdit
			// 
			this.DimUQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DimUQDropEdit, "US_DimUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FDA)(null)).US_DimUQ)));
			this.DimUQDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9c1379b9-3730-4c39-bed8-658e0b551a5c", "UQ");
			this.DimUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 25, true);
			this.DimUQDropEdit.Name = "DimUQDropEdit";
			this.DimUQDropEdit.PreBoundMaxLength = 1;
			this.DimUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
			this.DimUQDropEdit.TabIndex = 1;
			// 
			// US_ContainerDim3CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.US_ContainerDim3CalcEdit, "US_ContainerDim3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_ContainerDim3)));
			this.US_ContainerDim3CalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7177819c-9422-4c5d-b370-3b744e4c602b", "Length");
			this.US_ContainerDim3CalcEdit.DecimalPlaces = 4;
			this.US_ContainerDim3CalcEdit.Decimals = 4;
			this.US_ContainerDim3CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 51, true);
			this.US_ContainerDim3CalcEdit.Name = "US_ContainerDim3CalcEdit";
			this.US_ContainerDim3CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.US_ContainerDim3CalcEdit.TabIndex = 4;
			this.US_ContainerDim3CalcEdit.Text = "0.0000";
			this.US_ContainerDim3CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// US_ContainerDim2CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.US_ContainerDim2CalcEdit, "US_ContainerDim2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_ContainerDim2)));
			this.US_ContainerDim2CalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c879df54-2c54-42ee-8273-e8dcaed7f5e2", "Height");
			this.US_ContainerDim2CalcEdit.DecimalPlaces = 4;
			this.US_ContainerDim2CalcEdit.Decimals = 4;
			this.US_ContainerDim2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 51, true);
			this.US_ContainerDim2CalcEdit.Name = "US_ContainerDim2CalcEdit";
			this.US_ContainerDim2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.US_ContainerDim2CalcEdit.TabIndex = 3;
			this.US_ContainerDim2CalcEdit.Text = "0.0000";
			this.US_ContainerDim2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// US_ContainerDim1CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.US_ContainerDim1CalcEdit, "US_ContainerDim1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_ContainerDim1)));
			this.US_ContainerDim1CalcEdit.DecimalPlaces = 4;
			this.US_ContainerDim1CalcEdit.Decimals = 4;
			this.US_ContainerDim1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 51, true);
			this.US_ContainerDim1CalcEdit.Name = "US_ContainerDim1CalcEdit";
			this.US_ContainerDim1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.US_ContainerDim1CalcEdit.TabIndex = 2;
			this.US_ContainerDim1CalcEdit.Text = "0.0000";
			this.US_ContainerDim1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// US_FDAContainerDimTypeDropEdit
			// 
			this.US_FDAContainerDimTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAContainerDimTypeDropEdit, "US_FDAContainerDimType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAContainerDimType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.US_CylindricalRectangularList)));
			this.US_FDAContainerDimTypeDropEdit.BindToList = "AddInfoLookups+US_CylindricalRectangularList";
			this.US_FDAContainerDimTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("37e52564-2827-4a22-9be3-970e9196e38f", "Shape");
			this.US_FDAContainerDimTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 25, true);
			this.US_FDAContainerDimTypeDropEdit.Name = "US_FDAContainerDimTypeDropEdit";
			this.US_FDAContainerDimTypeDropEdit.PreBoundMaxLength = 1;
			this.US_FDAContainerDimTypeDropEdit.ShowDescriptionBox = false;
			this.US_FDAContainerDimTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.US_FDAContainerDimTypeDropEdit.TabIndex = 0;
			// 
			// AffirmationCodeGroupBox
			// 
			this.AffirmationCodeGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("71074c62-0946-4b37-954a-023f9d0edfa3", "Affirmation Codes");
			this.AffirmationCodeGroupBox.Controls.Add(this.AffirmationCodesGrid);
			this.AffirmationCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 516, true);
			this.AffirmationCodeGroupBox.Name = "AffirmationCodeGroupBox";
			this.AffirmationCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 116, true);
			this.AffirmationCodeGroupBox.TabIndex = 3;
			this.AffirmationCodeGroupBox.TabStop = false;
			// 
			// AffirmationCodesGrid
			// 
			this.AffirmationCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AffirmationCodesGrid, "AffirmationCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AffirmationCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AffirmationCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AffirmationCodes)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AffirmationCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AffirmationCodes)).SyncRoot)).AffirmationCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AffirmationCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AffirmationCodes)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AffirmationCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AffirmationCodes)).SyncRoot)).CY_Data)));
			this.AffirmationCodesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "AffirmationCodeList";
			zCodeFindBoxColumnStyleInfo1.Caption = "Code";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.AffirmationOfCompliance;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo1.Caption = "Description";
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(175);
			zTextBoxColumnStyleInfo2.Caption = "Value";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			this.AffirmationCodesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AffirmationCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AffirmationCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AffirmationCodesGrid.CopySelectedRowsAllowed = true;
			this.AffirmationCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AffirmationCodesGrid.GridId = "fa294691-43c1-426a-919a-c5a36909e9df";
			this.AffirmationCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AffirmationCodesGrid.LayoutKey = "AffirmationCodesGrid";
			this.AffirmationCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AffirmationCodesGrid.Name = "AffirmationCodesGrid";
			this.AffirmationCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 97, true);
			this.AffirmationCodesGrid.TabIndex = 0;
			// 
			// US_UC_NKFDAProductionCodeFindBox
			// 
			this.US_UC_NKFDAProductionCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_UC_NKFDAProductionCodeFindBox, "US_UC_NKFDAProduction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_UC_NKFDAProduction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.USCCountryList)));
			this.US_UC_NKFDAProductionCodeFindBox.BindToList = "AddInfoLookups+USCCountryList";
			this.US_UC_NKFDAProductionCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("019d3b58-d9c0-44f9-b536-b4d7babb55af", "Prod. Ctry/Rgn.");
			this.US_UC_NKFDAProductionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 86, true);
			this.US_UC_NKFDAProductionCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Country;
			this.US_UC_NKFDAProductionCodeFindBox.Name = "US_UC_NKFDAProductionCodeFindBox";
			this.US_UC_NKFDAProductionCodeFindBox.PopupCaption = null;
			this.US_UC_NKFDAProductionCodeFindBox.PreBoundMaxLength = 2;
			this.US_UC_NKFDAProductionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.US_UC_NKFDAProductionCodeFindBox.TabIndex = 3;
			// 
			// US_FDACommercialDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_FDACommercialDescTextBox, "US_FDACommercialDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDACommercialDesc)));
			this.US_FDACommercialDescTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("31596914-10dc-4c0a-a8d5-53ca9137123f", "Description");
			this.US_FDACommercialDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 109, true);
			this.US_FDACommercialDescTextBox.Multiline = true;
			this.US_FDACommercialDescTextBox.Name = "US_FDACommercialDescTextBox";
			this.US_FDACommercialDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 37, true);
			this.US_FDACommercialDescTextBox.TabIndex = 4;
			// 
			// US_TradeBrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_TradeBrandNameTextBox, "US_TradeBrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_TradeBrandName)));
			this.US_TradeBrandNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6e7033a3-f972-40a5-b2c0-d621ebc64f43", "Brand Name");
			this.US_TradeBrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 218, true);
			this.US_TradeBrandNameTextBox.Name = "US_TradeBrandNameTextBox";
			this.US_TradeBrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.US_TradeBrandNameTextBox.TabIndex = 11;
			// 
			// US_FDAProductCodeCodeFindBox
			// 
			this.US_FDAProductCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDAProductCodeCodeFindBox, "US_FDAProductCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.US_FDAProductNumberList)));
			this.US_FDAProductCodeCodeFindBox.BindToList = "AddInfoLookups+US_FDAProductNumberList";
			this.US_FDAProductCodeCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("207a4540-49b3-493c-ae03-36469a4b9885", "FDA Product Code");
			this.US_FDAProductCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 149, true);
			this.US_FDAProductCodeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.US_FDAProductCodeCodeFindBox.Name = "US_FDAProductCodeCodeFindBox";
			this.US_FDAProductCodeCodeFindBox.PopupCaption = null;
			this.US_FDAProductCodeCodeFindBox.PreBoundMaxLength = 7;
			this.US_FDAProductCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.US_FDAProductCodeCodeFindBox.TabIndex = 5;
			// 
			// US_FDACargoStorageCodeDropEdit
			// 
			this.US_FDACargoStorageCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FDACargoStorageCodeDropEdit, "US_FDACargoStorageCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDACargoStorageCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.US_CargoStorageCodeList)));
			this.US_FDACargoStorageCodeDropEdit.BindToList = "AddInfoLookups+US_CargoStorageCodeList";
			this.US_FDACargoStorageCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("54d52bb6-716d-46f1-bfe1-3aebcdb1c83c", "Cargo Storage");
			this.US_FDACargoStorageCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 172, true);
			this.US_FDACargoStorageCodeDropEdit.Name = "US_FDACargoStorageCodeDropEdit";
			this.US_FDACargoStorageCodeDropEdit.PreBoundMaxLength = 1;
			this.US_FDACargoStorageCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.US_FDACargoStorageCodeDropEdit.TabIndex = 6;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2cca6f2a-19a2-4399-8750-588c73e15281", "Details");
			this.DetailsGroupBox.Controls.Add(this.FDAValueTextBox);
			this.DetailsGroupBox.Controls.Add(this.USDCurrencyTextBox);
			this.DetailsGroupBox.Controls.Add(this.FDACurrencyTextBox);
			this.DetailsGroupBox.Controls.Add(this.InvCurrFDAValueCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.ShipperAddressAddressControl);
			this.DetailsGroupBox.Controls.Add(this.FEINoAddressControl);
			this.DetailsGroupBox.Controls.Add(this.ManufacturerAddressAddressControl);
			this.DetailsGroupBox.Controls.Add(this.US_UC_NKFDAProductionCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.US_FDACommercialDescTextBox);
			this.DetailsGroupBox.Controls.Add(this.US_FDACargoStorageCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.US_TradeBrandNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.US_FDAProductCodeCodeFindBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 247, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// FDAValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.FDAValueTextBox, "FDAValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).FDAValue)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FDAValueTextBox, false);
			this.FDAValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 196, true);
			this.FDAValueTextBox.Name = "FDAValueTextBox";
			this.FDAValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.FDAValueTextBox.TabIndex = 9;
			// 
			// USDCurrencyTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.USDCurrencyTextBox, false);
			this.USDCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 196, true);
			this.USDCurrencyTextBox.Name = "USDCurrencyTextBox";
			this.USDCurrencyTextBox.ReadOnly = true;
			this.USDCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.USDCurrencyTextBox.TabIndex = 10;
			this.USDCurrencyTextBox.Text = "USD";
			// 
			// FDACurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.FDACurrencyTextBox, "InvCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).InvCurrencyCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FDACurrencyTextBox, false);
			this.FDACurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 196, true);
			this.FDACurrencyTextBox.Name = "FDACurrencyTextBox";
			this.FDACurrencyTextBox.ReadOnly = true;
			this.FDACurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.FDACurrencyTextBox.TabIndex = 8;
			// 
			// InvCurrFDAValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvCurrFDAValueCalcEdit, "US_InvCurrFDAValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FDA)(null)).US_InvCurrFDAValue)));
			this.InvCurrFDAValueCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5277f660-2319-404d-8905-1d3d31deadf1", "FDA Value");
			this.InvCurrFDAValueCalcEdit.DecimalPlaces = 2;
			this.InvCurrFDAValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 196, true);
			this.InvCurrFDAValueCalcEdit.Name = "InvCurrFDAValueCalcEdit";
			this.InvCurrFDAValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.InvCurrFDAValueCalcEdit.TabIndex = 7;
			this.InvCurrFDAValueCalcEdit.Text = "0";
			this.InvCurrFDAValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipperAddressAddressControl
			// 
			this.ShipperAddressAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressAddressControl, "US_FDAShipperAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAShipperAddress)));
			this.ShipperAddressAddressControl.BindToOrgList = "AddInfoLookups+Consignors";
			this.ShipperAddressAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("92e3266c-dbb7-4c0d-8aef-bda94ed3fe97", "Shipper");
			this.ShipperAddressAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 15, true);
			this.ShipperAddressAddressControl.Name = "ShipperAddressAddressControl";
			this.ShipperAddressAddressControl.PopupCaption = "Manufacturer";
			this.ShipperAddressAddressControl.ShowAddress = false;
			this.ShipperAddressAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.ShipperAddressAddressControl.TabIndex = 0;
			// 
			// FEINoAddressControl
			// 
			this.FEINoAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FEINoAddressControl, "US_OA_FDAFEI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.FDA)(null)).US_OA_FDAFEI)));
			this.FEINoAddressControl.BindToOrgList = "AddInfoLookups+Consignees";
			this.FEINoAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c3cf49b5-d9ba-4694-bafd-630b9f23c1d9", "FEI No.");
			this.FEINoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 63, true);
			this.FEINoAddressControl.Name = "FEINoAddressControl";
			this.FEINoAddressControl.PopupCaption = "FEI";
			this.FEINoAddressControl.ShowAddress = false;
			this.FEINoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.FEINoAddressControl.TabIndex = 2;
			// 
			// ManufacturerAddressAddressControl
			// 
			this.ManufacturerAddressAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressAddressControl, "US_FDAManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAManufacturerAddress)));
			this.ManufacturerAddressAddressControl.BindToOrgList = "AddInfoLookups+Consignors";
			this.ManufacturerAddressAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("531211c2-1b27-4a14-9562-80370e23d559", "Manufacturer No.");
			this.ManufacturerAddressAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 39, true);
			this.ManufacturerAddressAddressControl.Name = "ManufacturerAddressAddressControl";
			this.ManufacturerAddressAddressControl.PopupCaption = "Manufacturer";
			this.ManufacturerAddressAddressControl.ShowAddress = false;
			this.ManufacturerAddressAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.ManufacturerAddressAddressControl.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 616, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 5;
			this.OKButton.Text = "&OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// PriorNoticeGroupBox
			// 
			this.PriorNoticeGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("876f9881-64d8-4ef9-b02c-17874bc4e4e1", "Prior Notice");
			this.PriorNoticeGroupBox.Controls.Add(this.OtherLabel);
			this.PriorNoticeGroupBox.Controls.Add(this.BillsGroupBox);
			this.PriorNoticeGroupBox.Controls.Add(this.PFRTextBox);
			this.PriorNoticeGroupBox.Controls.Add(this.ManufacturerProducerLabel);
			this.PriorNoticeGroupBox.Controls.Add(this.ExemptDropEdit);
			this.PriorNoticeGroupBox.Controls.Add(this.ForcePNCheckBox);
			this.PriorNoticeGroupBox.Controls.Add(this.ContainersGroupBox);
			this.PriorNoticeGroupBox.Controls.Add(this.PFTDropEdit);
			this.PriorNoticeGroupBox.Controls.Add(this.US_SFRTextBox);
			this.PriorNoticeGroupBox.Controls.Add(this.CSHCodeFindBox);
			this.PriorNoticeGroupBox.Controls.Add(this.OFTDropEdit);
			this.PriorNoticeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 0, true);
			this.PriorNoticeGroupBox.Name = "PriorNoticeGroupBox";
			this.PriorNoticeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 609, true);
			this.PriorNoticeGroupBox.TabIndex = 4;
			this.PriorNoticeGroupBox.TabStop = false;
			// 
			// OtherLabel
			// 
			this.OtherLabel.AutoSize = true;
			this.OtherLabel.IsFontBold = true;
			this.OtherLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 185, true);
			this.OtherLabel.Name = "OtherLabel";
			this.OtherLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.OtherLabel.TabIndex = 6;
			this.OtherLabel.Text = "Other";
			// 
			// BillsGroupBox
			// 
			this.BillsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cee7fcdb-f406-4ff6-9914-9a3ecfb13d6a", "Related Bills");
			this.BillsGroupBox.Controls.Add(this.BillsGrid);
			this.BillsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 301, true);
			this.BillsGroupBox.Name = "BillsGroupBox";
			this.BillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 152, true);
			this.BillsGroupBox.TabIndex = 9;
			this.BillsGroupBox.TabStop = false;
			// 
			// BillsGrid
			// 
			this.BillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillsGrid, "BillsAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).BillsAvailable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDARelatedBill)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).BillsAvailable)).SyncRoot)).BillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FDARelatedBill)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).BillsAvailable)).SyncRoot)).IsForFDALine)));
			this.BillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "BillNumber";
			zTextBoxColumnStyleInfo3.ColumnName = "BillNumber";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.Caption = "Related?";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsForFDALine";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BillsGrid.CopySelectedRowsAllowed = true;
			this.BillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGrid.GridId = "66cfad5c-50be-4058-af6e-7d56f1d2f56d";
			this.BillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillsGrid.LayoutKey = "zGrid1";
			this.BillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BillsGrid.Name = "BillsGrid";
			this.BillsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.BillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 133, true);
			this.BillsGrid.TabIndex = 0;
			// 
			// PFRTextBox
			// 
			this.PFRTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PFRTextBox, "US_PFR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_PFR)));
			this.PFRTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8f0ee0f8-79de-4c73-9789-0dcf9958d441", "Food Facility Reg. No.");
			this.PFRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 101, true);
			this.PFRTextBox.Name = "PFRTextBox";
			this.PFRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.PFRTextBox.TabIndex = 3;
			// 
			// ManufacturerProducerLabel
			// 
			this.ManufacturerProducerLabel.AutoSize = true;
			this.ManufacturerProducerLabel.IsFontBold = true;
			this.ManufacturerProducerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.ManufacturerProducerLabel.Name = "ManufacturerProducerLabel";
			this.ManufacturerProducerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 13, true);
			this.ManufacturerProducerLabel.TabIndex = 2;
			this.ManufacturerProducerLabel.Text = "Manufacturer/Producer";
			// 
			// ExemptDropEdit
			// 
			this.ExemptDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExemptDropEdit, "US_FME");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FDA)(null)).US_FME)));
			this.ExemptDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("021f0d8a-3dac-42b5-abe1-17112db5cf99", "Food Facility Reg. Exempt.");
			this.ExemptDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 149, true);
			this.ExemptDropEdit.Name = "ExemptDropEdit";
			this.ExemptDropEdit.PreBoundMaxLength = 1;
			this.ExemptDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.ExemptDropEdit.TabIndex = 5;
			// 
			// ForcePNCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ForcePNCheckBox, "US_FDAForcePN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FDA)(null)).US_FDAForcePN)));
			this.ForcePNCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ForcePNCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForcePNCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 240, true);
			this.ForcePNCheckBox.Name = "ForcePNCheckBox";
			this.ForcePNCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 55, true);
			this.ForcePNCheckBox.TabIndex = 8;
			this.ForcePNCheckBox.Text = "Force Prior Notice. When PN is not normally required (non FD3/FD4 Tariffs), check" +
    " this to force the sending of prior notice information for this FDA line.";
			this.ForcePNCheckBox.UseVisualStyleBackColor = true;
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ca2ca1fb-6721-4a9b-ab6f-1b9d7ae781c3", "In Container / Rail Car");
			this.ContainersGroupBox.Controls.Add(this.ContainersGrid);
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 453, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 153, true);
			this.ContainersGroupBox.TabIndex = 10;
			this.ContainersGroupBox.TabStop = false;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "ContainersForInvoiceLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).ContainersForInvoiceLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDARelatedContainer)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).ContainersForInvoiceLine)).SyncRoot)).ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FDARelatedContainer)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).ContainersForInvoiceLine)).SyncRoot)).IsForFDALine)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "CNO / RNO";
			zTextBoxColumnStyleInfo4.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Caption = "Related?";
			zCheckBoxColumnStyleInfo2.ColumnName = "IsForFDALine";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ContainersGrid.CopySelectedRowsAllowed = true;
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "151b8977-e2a7-48ab-9903-5615779a7535";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "zGrid1";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 134, true);
			this.ContainersGrid.TabIndex = 0;
			// 
			// PFTDropEdit
			// 
			this.PFTDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PFTDropEdit, "US_PFT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FDA)(null)).US_PFT)));
			this.PFTDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fe3f6187-f1ed-43b4-8b6a-7f303f8c1a09", "Producer Firm Type");
			this.PFTDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 125, true);
			this.PFTDropEdit.Name = "PFTDropEdit";
			this.PFTDropEdit.PreBoundMaxLength = 2;
			this.PFTDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.PFTDropEdit.TabIndex = 4;
			// 
			// US_SFRTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_SFRTextBox, "US_SFR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_SFR)));
			this.US_SFRTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6dc2ee87-4805-404f-987f-f1d9e2ef9107", "Shipper Reg. No.");
			this.US_SFRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 208, true);
			this.US_SFRTextBox.Name = "US_SFRTextBox";
			this.US_SFRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.US_SFRTextBox.TabIndex = 7;
			// 
			// CSHCodeFindBox
			// 
			this.CSHCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSHCodeFindBox, "US_CSH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FDA)(null)).US_CSH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FDA)(null)).AddInfoLookups.USCCountryList)));
			this.CSHCodeFindBox.BindToList = "AddInfoLookups+USCCountryList";
			this.CSHCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b22dd05f-0ab5-4526-a2c5-725a320b14a9", "Country/Region Of Shipping");
			this.CSHCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 48, true);
			this.CSHCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Country;
			this.CSHCodeFindBox.Name = "CSHCodeFindBox";
			this.CSHCodeFindBox.PopupCaption = null;
			this.CSHCodeFindBox.PreBoundMaxLength = 2;
			this.CSHCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.CSHCodeFindBox.TabIndex = 1;
			// 
			// OFTDropEdit
			// 
			this.OFTDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OFTDropEdit, "US_OFT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FDA)(null)).US_OFT)));
			this.OFTDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9bd8cc46-9870-4d83-920c-d3102ba37847", "Owner Firm Type");
			this.OFTDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 24, true);
			this.OFTDropEdit.Name = "OFTDropEdit";
			this.OFTDropEdit.PreBoundMaxLength = 2;
			this.OFTDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.OFTDropEdit.TabIndex = 0;
			// 
			// FDAForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 669, true);
			this.Controls.Add(this.PriorNoticeGroupBox);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.AffirmationCodeGroupBox);
			this.Controls.Add(this.FDAQuantitiesGroupBox);
			this.Controls.Add(this.ContainerDimensionsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.FDA);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FDAForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "FDAForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ContainerDimensionsGroupBox, 0);
			this.Controls.SetChildIndex(this.FDAQuantitiesGroupBox, 0);
			this.Controls.SetChildIndex(this.AffirmationCodeGroupBox, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.PriorNoticeGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FDAQuantitiesGroupBox.ResumeLayout(false);
			this.ContainerDimensionsGroupBox.ResumeLayout(false);
			this.ContainerDimensionsGroupBox.PerformLayout();
			this.AffirmationCodeGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AffirmationCodesGrid)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.PriorNoticeGroupBox.ResumeLayout(false);
			this.PriorNoticeGroupBox.PerformLayout();
			this.BillsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).EndInit();
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox FDAQuantitiesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit US_FDAQty1CalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ContainerDimensionsGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit US_ContainerDim3CalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit US_ContainerDim2CalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit US_ContainerDim1CalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit US_FDAContainerDimTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit US_FDAQty5CalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit US_FDAQty4CalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit US_FDAQty3CalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit US_FDAQty2CalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AffirmationCodeGroupBox;
		private Enterprise.ZArchitecture.ZGrid AffirmationCodesGrid;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit US_FDAQty6CalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox US_UC_NKFDAProductionCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox US_FDACommercialDescTextBox;
		private Enterprise.ZArchitecture.ZTextBox US_TradeBrandNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox US_FDAProductCodeCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit US_FDACargoStorageCodeDropEdit;
		public Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ManufacturerAddressAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PriorNoticeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OFTDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox PFRTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ExemptDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PFTDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CSHCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox US_SFRTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ContainersGroupBox;
		protected internal Enterprise.ZArchitecture.ZGrid ContainersGrid;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ForcePNCheckBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl FEINoAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BillsGroupBox;
		protected internal Enterprise.ZArchitecture.ZGrid BillsGrid;
		private Enterprise.ZArchitecture.ZLabel ManufacturerProducerLabel;
		private Enterprise.ZArchitecture.ZLabel OtherLabel;
		private Enterprise.ZArchitecture.ZLabel FDAQtyRunningTotalLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DimUQDropEdit;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ShipperAddressAddressControl;
		private Enterprise.ZArchitecture.ZCalcEdit InvCurrFDAValueCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox FDACurrencyTextBox;
		private Enterprise.ZArchitecture.ZTextBox USDCurrencyTextBox;
		internal ZArchitecture.ZTextBox FDAValueTextBox;
	}
}
