using System;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	partial class ContainersUserControl
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
			this.components = new System.ComponentModel.Container();
			this.edContainerNum = new Enterprise.ZArchitecture.ZTextBox();
			this.edSealNum = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TareOnFileCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActualTareWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ActualNetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ActualDunnageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ActualWeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalGrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OnFileWeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MaxGrossWeightOnFileCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExportContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExportContainerTypeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImportTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ImportIsSealOkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JC_IsShipperOwnedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainerQualityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommodityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ContainerStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExportIsDamagedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportIsEmptyContainerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DetailTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ReeferTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MeasuresTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OutturnTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.VGMTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ServicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FreightRatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WeightsGroupBox.SuspendLayout();
			this.GrossWeightUQDropEdit.SuspendLayout();
			this.ExportContainerModeDropEdit.SuspendLayout();
			this.DeliveryModeDropEdit.SuspendLayout();
			this.ExportContainerTypeGuidFindBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ContainerQualityDropEdit.SuspendLayout();
			this.CommodityCodeFindBox.SuspendLayout();
			this.ContainerStatusDropEdit.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonContainer);
			// 
			// edContainerNum
			// 
			this.BindingSource.SetBindingMember(this.edContainerNum, "ContainerNumberForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).ContainerNumberForBinding)));
			this.edContainerNum.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|1f00f667-a1c8-48fe-9f4b-66101d319d1d", "Container", "Container Number.");
			this.edContainerNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 4, true);
			this.edContainerNum.Name = "edContainerNum";
			this.edContainerNum.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.edContainerNum.TabIndex = 0;
			// 
			// edSealNum
			// 
			this.BindingSource.SetBindingMember(this.edSealNum, "SealNumberForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).SealNumberForBinding)));
			this.edSealNum.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|032e5f98-98a0-45bb-8fc3-41372bc45d89", "Seal", "Seal Number.");
			this.edSealNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 25, true);
			this.edSealNum.Name = "edSealNum";
			this.edSealNum.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.edSealNum.TabIndex = 2;
			// 
			// WeightsGroupBox
			// 
			this.WeightsGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|72909908-386a-4f2d-92a5-c847e1b46348", "Weights");
			this.WeightsGroupBox.Controls.Add(this.TareOnFileCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.GrossWeightUQDropEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualTareWeightCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualNetWeightCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualDunnageCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualWeightLabel);
			this.WeightsGroupBox.Controls.Add(this.TotalGrossWeightCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.OnFileWeightLabel);
			this.WeightsGroupBox.Controls.Add(this.MaxGrossWeightOnFileCalcEdit);
			this.WeightsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 74, true);
			this.WeightsGroupBox.Name = "WeightsGroupBox";
			this.WeightsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 132, true);
			this.WeightsGroupBox.TabIndex = 5;
			this.WeightsGroupBox.TabStop = false;
			// 
			// TareOnFileCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TareOnFileCalcEdit, "JobContainer+JC_Calc_TareWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_Calc_TareWeight)));
			this.TareOnFileCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|8feb378b-93b1-4884-a6e3-3130f94bfa8c", "Tare");
			this.TareOnFileCalcEdit.DecimalPlaces = 2;
			this.TareOnFileCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 23, true);
			this.TareOnFileCalcEdit.Name = "TareOnFileCalcEdit";
			this.TareOnFileCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.TareOnFileCalcEdit.TabIndex = 1;
			this.TareOnFileCalcEdit.Text = "0.000";
			this.TareOnFileCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightUQDropEdit
			// 
			this.GrossWeightUQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightUQDropEdit, "WeightUnitForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).WeightUnitForBinding)));
			this.GrossWeightUQDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|ff30d61d-805d-4add-a5b4-8143be736e2c", "Weight UQ");
			this.GrossWeightUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 107, true);
			this.GrossWeightUQDropEdit.Name = "GrossWeightUQDropEdit";
			this.GrossWeightUQDropEdit.PreBoundMaxLength = 2;
			this.GrossWeightUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.GrossWeightUQDropEdit.TabIndex = 8;
			// 
			// ActualTareWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualTareWeightCalcEdit, "JobContainer+JC_TareWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_TareWeight)));
			this.ActualTareWeightCalcEdit.DecimalPlaces = 2;
			this.ActualTareWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 23, true);
			this.ActualTareWeightCalcEdit.Name = "ActualTareWeightCalcEdit";
			this.ActualTareWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ActualTareWeightCalcEdit.TabIndex = 0;
			this.ActualTareWeightCalcEdit.Text = "0.000";
			this.ActualTareWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActualNetWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualNetWeightCalcEdit, "GoodsWeightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).GoodsWeightForBinding)));
			this.ActualNetWeightCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|793d2b87-ab4e-49ac-953d-8632ceb06046", "Goods Wgt.");
			this.ActualNetWeightCalcEdit.DecimalPlaces = 2;
			this.ActualNetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 44, true);
			this.ActualNetWeightCalcEdit.Name = "ActualNetWeightCalcEdit";
			this.ActualNetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ActualNetWeightCalcEdit.TabIndex = 3;
			this.ActualNetWeightCalcEdit.Text = "0.000";
			this.ActualNetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActualDunnageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualDunnageCalcEdit, "JobContainer+JC_DunnageWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DunnageWeight)));
			this.ActualDunnageCalcEdit.DecimalPlaces = 2;
			this.ActualDunnageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 65, true);
			this.ActualDunnageCalcEdit.Name = "ActualDunnageCalcEdit";
			this.ActualDunnageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ActualDunnageCalcEdit.TabIndex = 4;
			this.ActualDunnageCalcEdit.Text = "0.000";
			this.ActualDunnageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActualWeightLabel
			// 
			this.BindingSource.SetBindingMember(this.ActualWeightLabel, "JobContainer+ActualWeightUnitText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.ActualWeightUnitText)));
			this.ActualWeightLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|2492c415-2b61-4557-96b8-f8fcc6bdd04d", "Actual (kg)");
			this.ActualWeightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ActualWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 8, true);
			this.ActualWeightLabel.Name = "ActualWeightLabel";
			this.ActualWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 13, true);
			this.ActualWeightLabel.TabIndex = 1;
			this.ActualWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TotalGrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossWeightCalcEdit, "GrossWeightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).GrossWeightForBinding)));
			this.TotalGrossWeightCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("FreightContainersUserControl|a671ec23-048e-40e1-922b-376d5f02b236", "Gross Wgt.");
			this.TotalGrossWeightCalcEdit.DecimalPlaces = 2;
			this.TotalGrossWeightCalcEdit.IsCalculatorEnabled = false;
			this.TotalGrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 86, true);
			this.TotalGrossWeightCalcEdit.Name = "TotalGrossWeightCalcEdit";
			this.TotalGrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TotalGrossWeightCalcEdit.TabIndex = 5;
			this.TotalGrossWeightCalcEdit.Text = "0.000";
			this.TotalGrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OnFileWeightLabel
			// 
			this.BindingSource.SetBindingMember(this.OnFileWeightLabel, "JobContainer+OnFileWeightUnitText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.OnFileWeightUnitText)));
			this.OnFileWeightLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|1c79c741-2cb7-48f2-a8ea-50e67498512e", "On File (kg)");
			this.OnFileWeightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OnFileWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 8, true);
			this.OnFileWeightLabel.Name = "OnFileWeightLabel";
			this.OnFileWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 13, true);
			this.OnFileWeightLabel.TabIndex = 0;
			this.OnFileWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MaxGrossWeightOnFileCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxGrossWeightOnFileCalcEdit, "JobContainer+JC_Calc_MaxGrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_Calc_MaxGrossWeight)));
			this.MaxGrossWeightOnFileCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("FreightContainersUserControl|8fa7b57d-1ef6-4268-9ad5-09be98263179", "Max");
			this.MaxGrossWeightOnFileCalcEdit.DecimalPlaces = 2;
			this.MaxGrossWeightOnFileCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 86, true);
			this.MaxGrossWeightOnFileCalcEdit.Name = "MaxGrossWeightOnFileCalcEdit";
			this.MaxGrossWeightOnFileCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.MaxGrossWeightOnFileCalcEdit.TabIndex = 7;
			this.MaxGrossWeightOnFileCalcEdit.Text = "0.000";
			this.MaxGrossWeightOnFileCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExportContainerModeDropEdit
			// 
			this.ExportContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportContainerModeDropEdit, "ContainerModeForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).ContainerModeForBinding)));
			this.ExportContainerModeDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|268c6cb2-4f26-4a70-bda7-3c44925fddb6", "Mode", "Container Mode.");
			this.ExportContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 4, true);
			this.ExportContainerModeDropEdit.Name = "ExportContainerModeDropEdit";
			this.ExportContainerModeDropEdit.PreBoundMaxLength = 3;
			this.ExportContainerModeDropEdit.ShowDescriptionBox = false;
			this.ExportContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.ExportContainerModeDropEdit.TabIndex = 1;
			// 
			// DeliveryModeDropEdit
			// 
			this.DeliveryModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryModeDropEdit, "DeliveryModeForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).DeliveryModeForBinding)));
			this.DeliveryModeDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|6c912018-d2fa-4d17-913b-0d7c0859099b", "Deliv. Mode", "Delivery Mode");
			this.DeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 48, true);
			this.DeliveryModeDropEdit.Name = "DeliveryModeDropEdit";
			this.DeliveryModeDropEdit.PreBoundMaxLength = 3;
			this.DeliveryModeDropEdit.ShowDescriptionBox = false;
			this.DeliveryModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.DeliveryModeDropEdit.TabIndex = 4;
			// 
			// ExportContainerTypeGuidFindBox
			// 
			this.ExportContainerTypeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportContainerTypeGuidFindBox, "RCForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonContainer)(null)).RCForBinding)));
			this.ExportContainerTypeGuidFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|cdba13ca-8644-4801-b2bb-e8d725e6a043", "Type", "Container Type.");
			this.ExportContainerTypeGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ExportContainerTypeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 25, true);
			this.ExportContainerTypeGuidFindBox.Name = "ExportContainerTypeGuidFindBox";
			this.ExportContainerTypeGuidFindBox.PreBoundMaxLength = 4;
			this.ExportContainerTypeGuidFindBox.ShouldResize = true;
			this.ExportContainerTypeGuidFindBox.ShowDescriptionBox = false;
			this.ExportContainerTypeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.ExportContainerTypeGuidFindBox.TabIndex = 3;
			// 
			// ImportTabPage
			// 
			this.ImportTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|50468914-127b-40b3-88b9-7d20be0db257", "Import Process");
			this.ImportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ImportTabPage.Name = "ImportTabPage";
			this.ImportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.ImportTabPage.TabIndex = 1;
			this.ImportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ImportTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_FCLHeldInTransitStaging)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_FCLAvailable)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalCTOStorageStartDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_OverrideLCLAvailableStorage)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_LCLAvailable)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_LCLStorageCommences)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_EmptyReadyForReturn)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_EmptyReturnedBy)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_FCLWharfGateOut)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_FCLUnloadFromVessel)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalEstimatedDelivery)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalSlotReference)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalSlotDateTime)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalCartageComplete)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalCartageRef)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalCartageAdvised)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ArrivalPickupByRail)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ContainerYardEmptyReturnGateIn)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_OA_ArrivalContainerYardAddress)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_OverrideFCLAvailableStorage)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ContainerImportDORelease)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_PenaltyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_CreditorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_RL_NKLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_FreeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_PerUnitCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_TotalCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ImportPenalties)).SyncRoot)).CPY_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_PenaltyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_CreditorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_RL_NKLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_FreeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_TimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_PerUnitCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_TotalCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.ContainerPenalty)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).ExportPenalties)).SyncRoot)).CPY_RX_NKCurrency)));
			// 
			// ImportIsSealOkCheckBox
			// 
			this.ImportIsSealOkCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ImportIsSealOkCheckBox, "JobContainer+JC_IsSealOk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_IsSealOk)));
			this.ImportIsSealOkCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ImportIsSealOkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ImportIsSealOkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 10, true);
			this.ImportIsSealOkCheckBox.Name = "ImportIsSealOkCheckBox";
			this.ImportIsSealOkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ImportIsSealOkCheckBox.TabIndex = 0;
			// 
			// ExportTabPage
			// 
			this.ExportTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|5374be10-7168-4872-ba16-18b325faa100", "Export Process");
			this.ExportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ExportTabPage.Name = "ExportTabPage";
			this.ExportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.ExportTabPage.TabIndex = 0;
			this.ExportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ExportTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DepartureDeliveryByRail)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ContainerYardEmptyPickupGateOut)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_FCLOnBoardVessel)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DepartureCartageComplete)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DepartureCartageRef)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DepartureCartageAdvised)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_FCLWharfGateIn)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DepartureSlotDateTime)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DepartureSlotReference)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_DepartureEstimatedPickup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_EmptyRequired)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_OA_DepartureContainerYardAddress)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ReleaseNum)));
			// 
			// JC_IsShipperOwnedCheckBox
			// 
			this.JC_IsShipperOwnedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JC_IsShipperOwnedCheckBox, "JobContainer+JC_IsShipperOwned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_IsShipperOwned)));
			this.JC_IsShipperOwnedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.JC_IsShipperOwnedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JC_IsShipperOwnedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 10, true);
			this.JC_IsShipperOwnedCheckBox.Name = "JC_IsShipperOwnedCheckBox";
			this.JC_IsShipperOwnedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.JC_IsShipperOwnedCheckBox.TabIndex = 6;
			this.JC_IsShipperOwnedCheckBox.UseVisualStyleBackColor = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.JC_IsShipperOwnedCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ContainerQualityDropEdit);
			this.DetailsGroupBox.Controls.Add(this.CommodityCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.ContainerStatusDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ExportIsDamagedCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ExportIsEmptyContainerCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ImportIsSealOkCheckBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsGroupBox, false);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 212, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 125, true);
			this.DetailsGroupBox.TabIndex = 6;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ContainerQualityDropEdit
			// 
			this.ContainerQualityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerQualityDropEdit, "JobContainer+JC_ContainerQuality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ContainerQuality)));
			this.ContainerQualityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 81, true);
			this.ContainerQualityDropEdit.Name = "ContainerQualityDropEdit";
			this.ContainerQualityDropEdit.PreBoundMaxLength = 3;
			this.ContainerQualityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.ContainerQualityDropEdit.TabIndex = 5;
			// 
			// CommodityCodeFindBox
			// 
			this.CommodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeFindBox, "JobContainer+JC_RH_NKContainerCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_RH_NKContainerCommodityCode)));
			this.CommodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 29, true);
			this.CommodityCodeFindBox.Name = "CommodityCodeFindBox";
			this.CommodityCodeFindBox.ShouldResize = true;
			this.CommodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.CommodityCodeFindBox.TabIndex = 3;
			// 
			// ContainerStatusDropEdit
			// 
			this.ContainerStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerStatusDropEdit, "JobContainer+JC_ContainerStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_ContainerStatus)));
			this.ContainerStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 55, true);
			this.ContainerStatusDropEdit.Name = "ContainerStatusDropEdit";
			this.ContainerStatusDropEdit.PreBoundMaxLength = 3;
			this.ContainerStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.ContainerStatusDropEdit.TabIndex = 4;
			// 
			// ExportIsDamagedCheckBox
			// 
			this.ExportIsDamagedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsDamagedCheckBox, "JobContainer+JC_IsDamaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_IsDamaged)));
			this.ExportIsDamagedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsDamagedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsDamagedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 10, true);
			this.ExportIsDamagedCheckBox.Name = "ExportIsDamagedCheckBox";
			this.ExportIsDamagedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportIsDamagedCheckBox.TabIndex = 2;
			// 
			// ExportIsEmptyContainerCheckBox
			// 
			this.ExportIsEmptyContainerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsEmptyContainerCheckBox, "JobContainer+JC_IsEmptyContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_IsEmptyContainer)));
			this.ExportIsEmptyContainerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsEmptyContainerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsEmptyContainerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 10, true);
			this.ExportIsEmptyContainerCheckBox.Name = "ExportIsEmptyContainerCheckBox";
			this.ExportIsEmptyContainerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportIsEmptyContainerCheckBox.TabIndex = 1;
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Controls.Add(this.ReeferTabPage);
			this.DetailTabControl.Controls.Add(this.MeasuresTabPage);
			this.DetailTabControl.Controls.Add(this.ExportTabPage);
			this.DetailTabControl.Controls.Add(this.ImportTabPage);
			this.DetailTabControl.Controls.Add(this.OutturnTabPage);
			this.DetailTabControl.Controls.Add(this.VGMTabPage);
			this.DetailTabControl.Controls.Add(this.ServicesTabPage);
			this.DetailTabControl.Controls.Add(this.FreightRatesTabPage);
			this.DetailTabControl.Controls.Add(this.NumbersTabPage);
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 2, true);
			this.DetailTabControl.Name = "DetailTabControl";
			this.DetailTabControl.SelectedIndex = 0;
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 370, true);
			this.DetailTabControl.TabIndex = 7;
			// 
			// ReeferTabPage
			// 
			this.ReeferTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|7ee6aac5-144a-4cec-93a0-0e17b1ccdb0a", "Refrigeration");
			this.ReeferTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ReeferTabPage.Name = "ReeferTabPage";
			this.ReeferTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ReeferTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.ReeferTabPage.TabIndex = 2;
			this.ReeferTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ReeferTabPage_InitializeTab));
			// 
			// MeasuresTabPage
			// 
			this.MeasuresTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("9b175035-9d11-401d-8045-37cb0851bd4b", "Measures");
			this.MeasuresTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MeasuresTabPage.Name = "MeasuresTabPage";
			this.MeasuresTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MeasuresTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.MeasuresTabPage.TabIndex = 5;
			this.MeasuresTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MeasuresTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_Calc_ContainerCapacity)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_Calc_ActualCapacity)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_Calc_TotalVolumeInM3)));
			// 
			// OutturnTabPage
			// 
			this.OutturnTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|3b5751c1-d832-4f52-98a1-5890b7170eb0", "Outturn");
			this.OutturnTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OutturnTabPage.Name = "OutturnTabPage";
			this.OutturnTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OutturnTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.OutturnTabPage.TabIndex = 4;
			this.OutturnTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.OutturnTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Calc_JS_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Outturn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Pillaged)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Damaged)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_ActualWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_UnitOfDimension)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Calc_Shortlanded)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Calc_Surplus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnedLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnedWidth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnedHeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnUD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Calc_ConsignorPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_Calc_ConsigneePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomAttrib1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomAttrib2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomAttrib3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomAttrib4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomDate1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomDate2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomDecimal1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomDecimal2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomFlag1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_CustomFlag2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnedWeight)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnComment)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_MarksAndNumbers)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.PackLine)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.PackLines)).SyncRoot)).JL_OutturnedVolume)));
			// 
			// VGMTabPage
			// 
			this.VGMTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("1240db36-7f50-4a48-9c70-811b7eb38dc5", "VGM");
			this.VGMTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.VGMTabPage.Name = "VGMTabPage";
			this.VGMTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.VGMTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.VGMTabPage.TabIndex = 6;
			this.VGMTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.VGMTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_GrossWeightVerificationType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_GrossWeightVerificationDateTime)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.GrossWeightVerifiedByAddress)));
			// 
			// ServicesTabPage
			// 
			this.ServicesTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|4140c536-3d8c-46c5-81b4-c448e06eb5d9", "Services");
			this.ServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ServicesTabPage.Name = "ServicesTabPage";
			this.ServicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.ServicesTabPage.TabIndex = 3;
			this.ServicesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ServicesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_OA_Location)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Duration)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Completed)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_References)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Booked)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Calc_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Booked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_References)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Completed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ServiceProviderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_Calc_LocationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ServiceId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_ExternalServiceId)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)).Services)).SyncRoot)).ES_MeasurementBasis)));
			// 
			// FreightRatesTabPage
			// 
			this.FreightRatesTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("f8e0fb8b-ade6-47c9-8354-39df90124c47", "Freight Rates");
			this.FreightRatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.FreightRatesTabPage.Name = "FreightRatesTabPage";
			this.FreightRatesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FreightRatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.FreightRatesTabPage.TabIndex = 7;
			this.FreightRatesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.FreightRatesTabPage_InitializeTab));
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("88824c50-0b8b-4095-9dae-dc5efca4cae6", "", "Numbers", "", "");
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NumbersTabPage.Name = "NumbersTabPage";
			this.NumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 348, true);
			this.NumbersTabPage.TabIndex = 8;
			this.NumbersTabPage.UseVisualStyleBackColor = true;
			this.NumbersTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NumbersTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.AdditionalReferenceNumbers)));
			// 
			// ContainersUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryModeDropEdit);
			this.Controls.Add(this.ExportContainerModeDropEdit);
			this.Controls.Add(this.ExportContainerTypeGuidFindBox);
			this.Controls.Add(this.edSealNum);
			this.Controls.Add(this.edContainerNum);
			this.Controls.Add(this.DetailTabControl);
			this.Controls.Add(this.WeightsGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "ContainersUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 379, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WeightsGroupBox.ResumeLayout(false);
			this.WeightsGroupBox.PerformLayout();
			this.GrossWeightUQDropEdit.ResumeLayout(true);
			this.GrossWeightUQDropEdit.PerformLayout();
			this.ExportContainerModeDropEdit.ResumeLayout(true);
			this.ExportContainerModeDropEdit.PerformLayout();
			this.DeliveryModeDropEdit.ResumeLayout(true);
			this.DeliveryModeDropEdit.PerformLayout();
			this.ExportContainerTypeGuidFindBox.ResumeLayout(true);
			this.ExportContainerTypeGuidFindBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ContainerQualityDropEdit.ResumeLayout(true);
			this.ContainerQualityDropEdit.PerformLayout();
			this.CommodityCodeFindBox.ResumeLayout(true);
			this.CommodityCodeFindBox.PerformLayout();
			this.ContainerStatusDropEdit.ResumeLayout(true);
			this.ContainerStatusDropEdit.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void OutturnTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.outturnSubTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.OutturnGrid = new Enterprise.ZArchitecture.ZGrid();
			this.packlineDetailsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.packLocationTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OutturnTabPage.SuspendLayout();
			this.outturnSubTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OutturnGrid)).BeginInit();
			this.OutturnGrid.SuspendLayout();
			this.OutturnTabPage.Controls.Add(this.OutturnGrid);
			this.OutturnTabPage.Controls.Add(this.outturnSubTabControl);
			// 
			// outturnSubTabControl
			// 
			this.outturnSubTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.outturnSubTabControl.Controls.Add(this.packlineDetailsTab);
			this.outturnSubTabControl.Controls.Add(this.packLocationTab);
			this.outturnSubTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.outturnSubTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 158, true);
			this.outturnSubTabControl.Name = "outturnSubTabControl";
			this.outturnSubTabControl.SelectedIndex = 0;
			this.outturnSubTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 182, true);
			this.outturnSubTabControl.TabIndex = 21;
			// 
			// OutturnGrid
			// 
			this.OutturnGrid.AllowDrop = true;
			this.OutturnGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OutturnGrid, "JobContainer+PackLines");
			this.OutturnGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|a8ae07cb-cac3-40d4-a261-84a49fde4b5c", "Shipment ID");
			zTextBoxColumnStyleInfo1.ColumnName = "JL_Calc_JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|6f296095-27bb-4ba7-a409-9573a3c4e662", "Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "JL_PackageCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.ColumnName = "JL_F3_NKPackType";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|6ba7dd55-2171-48fc-a076-5b59be02a9e7", "Outturn");
			zCalcEditColumnStyleInfo2.ColumnName = "JL_Outturn";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|1b1f82a1-70d2-48db-b1b7-c2a15c583455", "Outturn");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|1b0dde77-1a93-4863-981e-652602370d64", "Pillaged");
			zCalcEditColumnStyleInfo3.ColumnName = "JL_Pillaged";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|1b1f82a1-70d2-48db-b1b7-c2a15c583455", "Outturn");
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|534cd674-ede7-4b3c-9be1-1cf9a77b9b80", "Damaged");
			zCalcEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zCalcEditColumnStyleInfo4.ColumnName = "JL_Damaged";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|1b1f82a1-70d2-48db-b1b7-c2a15c583455", "Outturn");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JL_ActualWeight";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|fb43dacf-e0ed-4d8e-a60a-509703961953", "Manifested Weight");
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|b939ee7b-7f0f-461a-a611-efd9c3e88f27", "UQ");
			zDropEditColumnStyleInfo2.ColumnName = "JL_ActualWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|fb43dacf-e0ed-4d8e-a60a-509703961953", "Manifested Weight");
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JL_ActualVolume";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|3695f53f-84b9-43e8-b394-326c3ea5a422", "Manifested Volume");
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|9cf2206e-2c39-4302-a4ba-590ff694f99e", "UQ");
			zDropEditColumnStyleInfo3.ColumnName = "JL_ActualVolumeUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|3695f53f-84b9-43e8-b394-326c3ea5a422", "Manifested Volume");
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "JL_Length";
			zCalcEditColumnStyleInfo7.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|c4a10520-fbdb-4774-8808-964e2206f018", "Manifested Dimensions");
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "JL_Width";
			zCalcEditColumnStyleInfo8.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|c4a10520-fbdb-4774-8808-964e2206f018", "Manifested Dimensions");
			zCalcEditColumnStyleInfo8.IsReadOnly = true;
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "JL_Height";
			zCalcEditColumnStyleInfo9.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|c4a10520-fbdb-4774-8808-964e2206f018", "Manifested Dimensions");
			zCalcEditColumnStyleInfo9.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|a74281a5-4b35-40f2-9fd5-0922ec7d8a6c", "UD");
			zDropEditColumnStyleInfo4.ColumnName = "JL_UnitOfDimension";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|c4a10520-fbdb-4774-8808-964e2206f018", "Manifested Dimensions");
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|ab350719-962f-4e9d-bb4a-d7851ad5b90a", "Cmdty. Code", "Commodity Code");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JL_RH_NKCommodityCode";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|50f34bc2-3fca-4e4c-8695-cfbf3c49c330", "Short");
			zCalcEditColumnStyleInfo10.ColumnName = "JL_Calc_Shortlanded";
			zCalcEditColumnStyleInfo10.Decimals = 0;
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|0bcdf4a1-04aa-408e-ae65-04dd57822768", "Surplus");
			zCalcEditColumnStyleInfo11.ColumnName = "JL_Calc_Surplus";
			zCalcEditColumnStyleInfo11.Decimals = 0;
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|e611ec80-3526-4b78-a1d3-5a03d0a858de", "OT Weight");
			zCalcEditColumnStyleInfo12.ColumnName = "JL_OutturnedWeight";
			zCalcEditColumnStyleInfo12.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|b355bb4f-f16d-4a3f-9ce2-e5f3aa86b3ea", "Outturned Weight");
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|483f07e5-b983-401e-b798-8d36a3c7e19d", "UW");
			zTextBoxColumnStyleInfo2.ColumnName = "JL_OutturnWeightUQ";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|b355bb4f-f16d-4a3f-9ce2-e5f3aa86b3ea", "Outturned Weight");
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|35556b14-15dc-4e2f-9faa-fdeb272d6948", "OT Volume");
			zCalcEditColumnStyleInfo13.ColumnName = "JL_OutturnedVolume";
			zCalcEditColumnStyleInfo13.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|b6976a24-6c39-484e-87b2-37d0bf8f230f", "Outturned Volume");
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|94996978-39fa-4d37-b6a9-3c5eb623ea6b", "UV");
			zTextBoxColumnStyleInfo3.ColumnName = "JL_OutturnVolumeUQ";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|b6976a24-6c39-484e-87b2-37d0bf8f230f", "Outturned Volume");
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|7573e873-a1a3-422f-8a5d-2197b48df0df", "OT Length");
			zCalcEditColumnStyleInfo14.ColumnName = "JL_OutturnedLength";
			zCalcEditColumnStyleInfo14.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|abef9f25-0994-43da-b956-8e0ddbce7820", "Outturned Dimensions");
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|8eb458e7-14b4-4851-bc4f-e7b4016c4878", "OT Width");
			zCalcEditColumnStyleInfo15.ColumnName = "JL_OutturnedWidth";
			zCalcEditColumnStyleInfo15.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|abef9f25-0994-43da-b956-8e0ddbce7820", "Outturned Dimensions");
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|fd1a2d3c-8621-4f20-9c14-43e9fb5b2fe9", "OT Height");
			zCalcEditColumnStyleInfo16.ColumnName = "JL_OutturnedHeight";
			zCalcEditColumnStyleInfo16.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|abef9f25-0994-43da-b956-8e0ddbce7820", "Outturned Dimensions");
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|993455ef-99ee-40aa-871b-ad38ad520d4c", "UD");
			zDropEditColumnStyleInfo5.ColumnName = "JL_OutturnUD";
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|abef9f25-0994-43da-b956-8e0ddbce7820", "Outturned Dimensions");
			zDropEditColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo4.ColumnName = "JL_OutturnComment";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|6b9d4d5d-61d2-4551-a44c-6c0d7836b608", "Description");
			zTextBoxColumnStyleInfo5.ColumnName = "JL_Description";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|2c9096b4-8cf2-4a25-ab17-2a7b02889381", "Consignor");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JL_Calc_ConsignorPK";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|e3cd795d-875c-44a1-b419-35fc921264ee", "Consignee");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JL_Calc_ConsigneePK";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "JL_CustomAttrib1";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "JL_CustomAttrib2";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "JL_CustomAttrib3";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "JL_CustomAttrib4";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "JL_CustomDate1";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "JL_CustomDate2";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.ColumnName = "JL_CustomDecimal1";
			zCalcEditColumnStyleInfo17.IsVisible = false;
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.ColumnName = "JL_CustomDecimal2";
			zCalcEditColumnStyleInfo18.IsVisible = false;
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "JL_CustomFlag1";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "JL_CustomFlag2";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OutturnGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.OutturnGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.OutturnGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.OutturnGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OutturnGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.OutturnGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OutturnGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OutturnGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OutturnGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OutturnGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OutturnGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.OutturnGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.OutturnGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OutturnGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.OutturnGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutturnGrid.GridId = "498d4ad4-f2ed-42ba-9e69-9266ae6da4cc";
			this.OutturnGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OutturnGrid.LayoutKey = "JobContainerBoundGrid";
			this.OutturnGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OutturnGrid.Name = "OutturnGrid";
			this.OutturnGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 155, true);
			this.OutturnGrid.TabIndex = 0;
			// 
			// packlineDetailsTab
			// 
			this.packlineDetailsTab.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|d629d30d-6a0e-4c24-9327-a9fa36f02ffc", "Details");
			this.packlineDetailsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.packlineDetailsTab.Name = "packlineDetailsTab";
			this.packlineDetailsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.packlineDetailsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 160, true);
			this.packlineDetailsTab.TabIndex = 0;
			this.packlineDetailsTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.packlineDetailsTab_InitializeTab));
			// 
			// packLocationTab
			// 
			this.packLocationTab.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|0b754398-9c0e-4d1e-aeae-8bebeacd64ac", "Location");
			this.packLocationTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.packLocationTab.Name = "packLocationTab";
			this.packLocationTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.packLocationTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 160, true);
			this.packLocationTab.TabIndex = 1;
			this.packLocationTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.packLocationTab_InitializeTab));
			this.OutturnTabPage.PerformLayout();
			this.outturnSubTabControl.ResumeLayout(false);
			this.outturnSubTabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OutturnGrid)).EndInit();
			this.OutturnGrid.ResumeLayout(false);
			this.OutturnGrid.PerformLayout();
			this.OutturnTabPage.ResumeLayout(true);

		}

		private void ServicesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo19 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.NotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ServiceLocationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ServiceCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ServiceDurationTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.ServiceCompletedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ES_ReferTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ES_BookedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ServiceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.contractorFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.rateAndCurrencyCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.measurementBasisDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServicesTabPage.SuspendLayout();
			this.ServiceLocationAddressControl.SuspendLayout();
			this.ServiceCompletedDateEdit.SuspendLayout();
			this.ES_BookedDateEdit.SuspendLayout();
			this.ServiceCodeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.ServicesGrid.SuspendLayout();
			this.contractorFindBox.SuspendLayout();
			this.rateAndCurrencyCalcFindBox.SuspendLayout();
			this.measurementBasisDropEdit.SuspendLayout();
			this.ServicesTabPage.Controls.Add(this.measurementBasisDropEdit);
			this.ServicesTabPage.Controls.Add(this.rateAndCurrencyCalcFindBox);
			this.ServicesTabPage.Controls.Add(this.NotesTextBox);
			this.ServicesTabPage.Controls.Add(this.ServiceLocationAddressControl);
			this.ServicesTabPage.Controls.Add(this.contractorFindBox);
			this.ServicesTabPage.Controls.Add(this.ServiceCountCalcEdit);
			this.ServicesTabPage.Controls.Add(this.ServiceDurationTimeEdit);
			this.ServicesTabPage.Controls.Add(this.ServiceCompletedDateEdit);
			this.ServicesTabPage.Controls.Add(this.ES_ReferTextBox);
			this.ServicesTabPage.Controls.Add(this.ES_BookedDateEdit);
			this.ServicesTabPage.Controls.Add(this.ServiceCodeDropEdit);
			this.ServicesTabPage.Controls.Add(this.ServicesGrid);
			// 
			// NotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesTextBox, "Services.ES_ServiceNote");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.NotesTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.NotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 185, true);
			this.NotesTextBox.Multiline = true;
			this.NotesTextBox.Name = "NotesTextBox";
			this.NotesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.NotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 149, true);
			this.NotesTextBox.TabIndex = 19;
			// 
			// ServiceLocationAddressControl
			// 
			this.ServiceLocationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLocationAddressControl, "Services.ES_OA_Location");
			this.ServiceLocationAddressControl.BindToOrgList = "Services.Lookups.ServiceProvider";
			this.ServiceLocationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 275, true);
			this.ServiceLocationAddressControl.Name = "ServiceLocationAddressControl";
			this.ServiceLocationAddressControl.PopupCaption = null;
			this.ServiceLocationAddressControl.ReadOnly = false;
			this.ServiceLocationAddressControl.ShowAddress = false;
			this.ServiceLocationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
			this.ServiceLocationAddressControl.TabIndex = 8;
			// 
			// ServiceCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ServiceCountCalcEdit, "Services.ES_ServiceCount");
			this.ServiceCountCalcEdit.DecimalPlaces = 0;
			this.ServiceCountCalcEdit.Decimals = 0;
			this.ServiceCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 191, true);
			this.ServiceCountCalcEdit.Name = "ServiceCountCalcEdit";
			this.ServiceCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 17, true);
			this.ServiceCountCalcEdit.TabIndex = 5;
			this.ServiceCountCalcEdit.Text = "0";
			this.ServiceCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ServiceDurationTimeEdit
			// 
			this.ServiceDurationTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ServiceDurationTimeEdit, "Services.ES_Duration");
			this.ServiceDurationTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 212, true);
			this.ServiceDurationTimeEdit.Name = "ServiceDurationTimeEdit";
			this.ServiceDurationTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.ServiceDurationTimeEdit.TabIndex = 6;
			// 
			// ServiceCompletedDateEdit
			// 
			this.ServiceCompletedDateEdit.AllowDrop = true;
			this.ServiceCompletedDateEdit.AutoCompleteMonthThreshold = 1;
			this.ServiceCompletedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ServiceCompletedDateEdit, "Services.ES_Completed");
			this.ServiceCompletedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ServiceCompletedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 233, true);
			this.ServiceCompletedDateEdit.Name = "ServiceCompletedDateEdit";
			this.ServiceCompletedDateEdit.TabIndex = 4;
			// 
			// ES_ReferTextBox
			// 
			this.BindingSource.SetBindingMember(this.ES_ReferTextBox, "Services.ES_References");
			this.ES_ReferTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 212, true);
			this.ES_ReferTextBox.Name = "ES_ReferTextBox";
			this.ES_ReferTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ES_ReferTextBox.TabIndex = 3;
			// 
			// ES_BookedDateEdit
			// 
			this.ES_BookedDateEdit.AllowDrop = true;
			this.ES_BookedDateEdit.AutoCompleteMonthThreshold = 1;
			this.ES_BookedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ES_BookedDateEdit, "Services.ES_Booked");
			this.ES_BookedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ES_BookedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 191, true);
			this.ES_BookedDateEdit.Name = "ES_BookedDateEdit";
			this.ES_BookedDateEdit.TabIndex = 2;
			// 
			// ServiceCodeDropEdit
			// 
			this.ServiceCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceCodeDropEdit, "Services.ES_ServiceCode");
			this.ServiceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 170, true);
			this.ServiceCodeDropEdit.Name = "ServiceCodeDropEdit";
			this.ServiceCodeDropEdit.PreBoundMaxLength = 3;
			this.ServiceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 17, true);
			this.ServiceCodeDropEdit.TabIndex = 1;
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServicesGrid, "Services");
			this.ServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.ColumnName = "ES_ServiceCode";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo10.ColumnName = "ES_Calc_Description";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "ES_Booked";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "ES_References";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo4.ColumnName = "ES_Completed";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo19.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo19.ColumnName = "ES_ServiceCount";
			zCalcEditColumnStyleInfo19.Decimals = 0;
			zCalcEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "ES_Duration";
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "ES_OH_Contractor";
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|86874f5f-e340-47b0-acc3-2b19c2ce30c0", "Provider", "Service Provider", "Service Location Provider", "");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "ServiceProviderPK";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|29f85f64-c9d9-4695-975c-c76bcad250d5", "Srv. Location", "Service Location", "Service Location Address", "");
			zDropEditColumnStyleInfo7.ColumnName = "ES_Calc_LocationCode";
			zDropEditColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "ES_ServiceNote";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "ES_ServiceId";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo14.ColumnName = "ES_ExternalServiceId";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			this.ServicesGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ServicesGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.ServicesGrid.GridId = "3c3bd864-72ed-4091-aa44-392bcfb774d7";
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "ServicesGrid";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 161, true);
			this.ServicesGrid.TabIndex = 0;
			// 
			// contractorFindBox
			// 
			this.contractorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contractorFindBox, "Services.ES_OH_Contractor");
			this.contractorFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.contractorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 254, true);
			this.contractorFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.contractorFindBox.Name = "contractorFindBox";
			this.contractorFindBox.ShouldResize = true;
			this.contractorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 17, true);
			this.contractorFindBox.TabIndex = 7;
			// 
			// rateAndCurrencyCalcFindBox
			// 
			this.rateAndCurrencyCalcFindBox.AllowDrop = true;
			this.rateAndCurrencyCalcFindBox.BindToAmount = "Services.ES_ServiceRate";
			this.rateAndCurrencyCalcFindBox.BindToUnit = "Services.ES_RX_NKServiceRateCurrency";
			this.rateAndCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.rateAndCurrencyCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 296, true);
			this.rateAndCurrencyCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.rateAndCurrencyCalcFindBox.Name = "rateAndCurrencyCalcFindBox";
			this.rateAndCurrencyCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			this.rateAndCurrencyCalcFindBox.TabIndex = 12;
			// 
			// measurementBasisDropEdit
			// 
			this.measurementBasisDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.measurementBasisDropEdit, "Services.ES_MeasurementBasis");
			this.measurementBasisDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.measurementBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 296, true);
			this.measurementBasisDropEdit.Name = "measurementBasisDropEdit";
			this.measurementBasisDropEdit.PreBoundMaxLength = 11;
			this.measurementBasisDropEdit.ShowDescriptionBox = false;
			this.measurementBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 17, true);
			this.measurementBasisDropEdit.TabIndex = 13;
			this.ServicesTabPage.PerformLayout();
			this.ServiceLocationAddressControl.ResumeLayout(true);
			this.ServiceLocationAddressControl.PerformLayout();
			this.ServiceCompletedDateEdit.ResumeLayout(true);
			this.ServiceCompletedDateEdit.PerformLayout();
			this.ES_BookedDateEdit.ResumeLayout(true);
			this.ES_BookedDateEdit.PerformLayout();
			this.ServiceCodeDropEdit.ResumeLayout(true);
			this.ServiceCodeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.ServicesGrid.ResumeLayout(false);
			this.ServicesGrid.PerformLayout();
			this.contractorFindBox.ResumeLayout(true);
			this.contractorFindBox.PerformLayout();
			this.rateAndCurrencyCalcFindBox.ResumeLayout(true);
			this.rateAndCurrencyCalcFindBox.PerformLayout();
			this.measurementBasisDropEdit.ResumeLayout(true);
			this.measurementBasisDropEdit.PerformLayout();
			this.ServicesTabPage.ResumeLayout(true);

		}

		protected virtual void ExportTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo penaltyTypeDescriptionColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo exportFreeDaysExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo exportDurationExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.IsArrivingAtCTOByRailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ContainerYardEmptyPickupGateOutDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EmptyPickupByTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FCLOnBoardVesselDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureCartageCompleteDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureCartageRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureCartageAdvisedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FCLWharfGateInDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureSlotDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureSlotReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportFullPickupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportEmptyReqByDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportPickupEmptyFromAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ExportReleaseNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportDepotCustomsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportPenaltiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SupplierBookingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RelatedContainerLoadListFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SupplierBookingCodeFindBox.AllowOverlap(RelatedContainerLoadListFindBox);
			this.RelatedContainerLoadPlanFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExportTabPage.SuspendLayout();
			this.ContainerYardEmptyPickupGateOutDateEdit.SuspendLayout();
			this.EmptyPickupByTransportModeDropEdit.SuspendLayout();
			this.FCLOnBoardVesselDateEdit.SuspendLayout();
			this.DepartureCartageCompleteDateEdit.SuspendLayout();
			this.DepartureCartageAdvisedDateEdit.SuspendLayout();
			this.FCLWharfGateInDateEdit.SuspendLayout();
			this.DepartureSlotDateEdit.SuspendLayout();
			this.ExportFullPickupDateEdit.SuspendLayout();
			this.ExportEmptyReqByDateEdit.SuspendLayout();
			this.ExportPickupEmptyFromAddressControl.SuspendLayout();
			this.SupplierBookingCodeFindBox.SuspendLayout();
			this.RelatedContainerLoadListFindBox.SuspendLayout();
			this.RelatedContainerLoadPlanFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExportPenaltiesGrid)).BeginInit();
			this.ExportPenaltiesGrid.SuspendLayout();
			this.ExportTabPage.Controls.Add(this.ExportPenaltiesGrid);
			this.ExportTabPage.Controls.Add(this.IsArrivingAtCTOByRailCheckBox);
			this.ExportTabPage.Controls.Add(this.ContainerYardEmptyPickupGateOutDateEdit);
			this.ExportTabPage.Controls.Add(this.FCLOnBoardVesselDateEdit);
			this.ExportTabPage.Controls.Add(this.DepartureCartageCompleteDateEdit);
			this.ExportTabPage.Controls.Add(this.DepartureCartageRefTextBox);
			this.ExportTabPage.Controls.Add(this.DepartureCartageAdvisedDateEdit);
			this.ExportTabPage.Controls.Add(this.FCLWharfGateInDateEdit);
			this.ExportTabPage.Controls.Add(this.DepartureSlotDateEdit);
			this.ExportTabPage.Controls.Add(this.DepartureSlotReferenceTextBox);
			this.ExportTabPage.Controls.Add(this.ExportFullPickupDateEdit);
			this.ExportTabPage.Controls.Add(this.ExportEmptyReqByDateEdit);
			this.ExportTabPage.Controls.Add(this.ExportPickupEmptyFromAddressControl);
			this.ExportTabPage.Controls.Add(this.ExportReleaseNumTextBox);
			this.ExportTabPage.Controls.Add(this.ExportDepotCustomsReferenceTextBox);
			this.ExportTabPage.Controls.Add(this.SupplierBookingCodeFindBox);
			this.ExportTabPage.Controls.Add(this.RelatedContainerLoadListFindBox);
			this.ExportTabPage.Controls.Add(this.RelatedContainerLoadPlanFindBox);
			this.ExportTabPage.Controls.Add(this.EmptyPickupByTransportModeDropEdit);
			// 
			// EmptyPickupByTransportModeDropEdit
			// 
			this.EmptyPickupByTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EmptyPickupByTransportModeDropEdit, "JobContainer+EmptyPickupByTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).EmptyPickupByTransportMode)));
			this.EmptyPickupByTransportModeDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|5CB8155C-1990-427D-9A1F-CB59B3D22FA9", "By", "Transport Mode.");
			this.EmptyPickupByTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 27, true);
			this.EmptyPickupByTransportModeDropEdit.Name = "EmptyPickupByTransportModeDropEdit";
			this.EmptyPickupByTransportModeDropEdit.PreBoundMaxLength = 3;
			this.EmptyPickupByTransportModeDropEdit.ShowDescriptionBox = true;
			this.EmptyPickupByTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.EmptyPickupByTransportModeDropEdit.TabIndex = 1;
			// 
			// SupplierBookingFindBox
			// 
			this.SupplierBookingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierBookingCodeFindBox, "JobContainer+JC_JSB_SupplierBooking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.JC_JSB_SupplierBooking)));
			this.SupplierBookingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 69, true);
			this.SupplierBookingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SupplierBooking;
			this.SupplierBookingCodeFindBox.Name = "SupplierBookingCodeFindBox";
			this.SupplierBookingCodeFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainer|ad2462d3-2e0c-4382-bfb4-5044e690cb37", "Supplier Booking Ref");
			this.SupplierBookingCodeFindBox.ShouldResize = false;
			this.SupplierBookingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.SupplierBookingCodeFindBox.ShowDescriptionBox = false;
			this.SupplierBookingCodeFindBox.AllowNewForm = false;
			this.SupplierBookingCodeFindBox.TabIndex = 12;
			// 
			// RelatedContainerLoadListFindBox
			//
			this.RelatedContainerLoadListFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedContainerLoadListFindBox, "JobContainer+RelatedContainerLoadListPK");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonContainer)(null)).JobContainer.RelatedContainerLoadListPK)));
			this.RelatedContainerLoadListFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 69, true);
			this.RelatedContainerLoadListFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ContainerLoadList;
			this.RelatedContainerLoadListFindBox.Name = "RelatedContainerLoadListFindBox";
			this.RelatedContainerLoadListFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainer|06FBEE15-B937-4E88-B310-65C477D2B5A6", "CY Load ID");
			this.RelatedContainerLoadListFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.RelatedContainerLoadListFindBox.ShouldResize = false;
			this.RelatedContainerLoadListFindBox.AllowNewForm = false;
			this.RelatedContainerLoadListFindBox.ShowDescriptionBox = false;
			this.RelatedContainerLoadListFindBox.TabIndex = 13;
			// 
			// RelatedContainerLoadPlanFindBox
			// 
			this.RelatedContainerLoadPlanFindBox.AllowDrop = true;
			this.RelatedContainerLoadPlanFindBox.AllowNewForm = false;
			this.BindingSource.SetBindingMember(this.RelatedContainerLoadPlanFindBox, "JobContainer+JC_CLH_LoadListPlan");
			this.RelatedContainerLoadPlanFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainer|17db6cbb-246e-4557-b5bb-12badbd423ef", "Load Plan ID");
			this.RelatedContainerLoadPlanFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 90, true);
			this.RelatedContainerLoadPlanFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ContainerLoadPlan;
			this.RelatedContainerLoadPlanFindBox.Name = "RelatedContainerLoadPlanFindBox";
			this.RelatedContainerLoadPlanFindBox.ParentType = null;
			this.RelatedContainerLoadPlanFindBox.ShouldResize = false;
			this.RelatedContainerLoadPlanFindBox.ShowDescriptionBox = false;
			this.RelatedContainerLoadPlanFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.RelatedContainerLoadPlanFindBox.TabIndex = 16;
			// 
			// IsArrivingAtCTOByRailCheckBox
			// 
			this.IsArrivingAtCTOByRailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsArrivingAtCTOByRailCheckBox, "JobContainer+JC_DepartureDeliveryByRail");
			this.IsArrivingAtCTOByRailCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsArrivingAtCTOByRailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsArrivingAtCTOByRailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 195, true);
			this.IsArrivingAtCTOByRailCheckBox.Name = "IsArrivingAtCTOByRailCheckBox";
			this.IsArrivingAtCTOByRailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsArrivingAtCTOByRailCheckBox.TabIndex = 13;
			this.IsArrivingAtCTOByRailCheckBox.UseVisualStyleBackColor = false;
			// 
			// ContainerYardEmptyPickupGateOutDateEdit
			// 
			this.ContainerYardEmptyPickupGateOutDateEdit.AllowDrop = true;
			this.ContainerYardEmptyPickupGateOutDateEdit.AutoCompleteMonthThreshold = 1;
			this.ContainerYardEmptyPickupGateOutDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ContainerYardEmptyPickupGateOutDateEdit, "JobContainer+JC_ContainerYardEmptyPickupGateOut");
			this.ContainerYardEmptyPickupGateOutDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ContainerYardEmptyPickupGateOutDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 69, true);
			this.ContainerYardEmptyPickupGateOutDateEdit.Name = "ContainerYardEmptyPickupGateOutDateEdit";
			this.ContainerYardEmptyPickupGateOutDateEdit.TabIndex = 3;
			// 
			// FCLOnBoardVesselDateEdit
			// 
			this.FCLOnBoardVesselDateEdit.AllowDrop = true;
			this.FCLOnBoardVesselDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCLOnBoardVesselDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCLOnBoardVesselDateEdit, "JobContainer+JC_FCLOnBoardVessel");
			this.FCLOnBoardVesselDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCLOnBoardVesselDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 237, true);
			this.FCLOnBoardVesselDateEdit.Name = "FCLOnBoardVesselDateEdit";
			this.FCLOnBoardVesselDateEdit.TabIndex = 14;
			// 
			this.FCLOnBoardVesselDateEdit.TabIndex = 15;
			// DepartureCartageCompleteDateEdit
			// 
			this.DepartureCartageCompleteDateEdit.AllowDrop = true;
			this.DepartureCartageCompleteDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureCartageCompleteDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureCartageCompleteDateEdit, "JobContainer+JC_DepartureCartageComplete");
			this.DepartureCartageCompleteDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DepartureCartageCompleteDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 153, true);
			this.DepartureCartageCompleteDateEdit.Name = "DepartureCartageCompleteDateEdit";
			this.DepartureCartageCompleteDateEdit.TabIndex = 9;
			// 
			// DepartureCartageRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureCartageRefTextBox, "JobContainer+JC_DepartureCartageRef");
			this.DepartureCartageRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 111, true);
			this.DepartureCartageRefTextBox.Name = "DepartureCartageRefTextBox";
			this.DepartureCartageRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.DepartureCartageRefTextBox.TabIndex = 7;
			// 
			// DepartureCartageAdvisedDateEdit
			// 
			this.DepartureCartageAdvisedDateEdit.AllowDrop = true;
			this.DepartureCartageAdvisedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureCartageAdvisedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureCartageAdvisedDateEdit, "JobContainer+JC_DepartureCartageAdvised");
			this.DepartureCartageAdvisedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DepartureCartageAdvisedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 90, true);
			this.DepartureCartageAdvisedDateEdit.Name = "DepartureCartageAdvisedDateEdit";
			this.DepartureCartageAdvisedDateEdit.TabIndex = 4;
			// 
			// FCLWharfGateInDateEdit
			// 
			this.FCLWharfGateInDateEdit.AllowDrop = true;
			this.FCLWharfGateInDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCLWharfGateInDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCLWharfGateInDateEdit, "JobContainer+JC_FCLWharfGateIn");
			this.FCLWharfGateInDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCLWharfGateInDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 216, true);
			this.FCLWharfGateInDateEdit.Name = "FCLWharfGateInDateEdit";
			this.FCLWharfGateInDateEdit.TabIndex = 13;
			// 
			this.FCLWharfGateInDateEdit.TabIndex = 14;
			// DepartureSlotDateEdit
			// 
			this.DepartureSlotDateEdit.AllowDrop = true;
			this.DepartureSlotDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureSlotDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureSlotDateEdit, "JobContainer+JC_DepartureSlotDateTime");
			this.DepartureSlotDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DepartureSlotDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 174, true);
			this.DepartureSlotDateEdit.Name = "DepartureSlotDateEdit";
			this.DepartureSlotDateEdit.TabIndex = 10;
			// 
			// DepartureSlotReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureSlotReferenceTextBox, "JobContainer+JC_DepartureSlotReference");
			this.DepartureSlotReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 195, true);
			this.DepartureSlotReferenceTextBox.Name = "DepartureSlotReferenceTextBox";
			this.DepartureSlotReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.DepartureSlotReferenceTextBox.TabIndex = 11;
			// 
			this.DepartureSlotReferenceTextBox.TabIndex = 12;
			// ExportFullPickupDateEdit
			// 
			this.ExportFullPickupDateEdit.AllowDrop = true;
			this.ExportFullPickupDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExportFullPickupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExportFullPickupDateEdit, "JobContainer+JC_DepartureEstimatedPickup");
			this.ExportFullPickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ExportFullPickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 132, true);
			this.ExportFullPickupDateEdit.Name = "ExportFullPickupDateEdit";
			this.ExportFullPickupDateEdit.TabIndex = 8;
			// 
			// ExportEmptyReqByDateEdit
			// 
			this.ExportEmptyReqByDateEdit.AllowDrop = true;
			this.ExportEmptyReqByDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExportEmptyReqByDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExportEmptyReqByDateEdit, "JobContainer+JC_EmptyRequired");
			this.ExportEmptyReqByDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ExportEmptyReqByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 6, true);
			this.ExportEmptyReqByDateEdit.Name = "ExportEmptyReqByDateEdit";
			this.ExportEmptyReqByDateEdit.TabIndex = 0;
			// 
			// ExportPickupEmptyFromAddressControl
			// 
			this.ExportPickupEmptyFromAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportPickupEmptyFromAddressControl, "JobContainer+JC_OA_DepartureContainerYardAddress");
			this.ExportPickupEmptyFromAddressControl.BindToOrgList = "JobContainer+Lookups+ContainerYard_List";
			this.ExportPickupEmptyFromAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 27, true);
			this.ExportPickupEmptyFromAddressControl.Name = "ExportPickupEmptyFromAddressControl";
			this.ExportPickupEmptyFromAddressControl.PopupCaption = null;
			this.ExportPickupEmptyFromAddressControl.ReadOnly = false;
			this.ExportPickupEmptyFromAddressControl.ShowAddress = false;
			this.ExportPickupEmptyFromAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
			this.ExportPickupEmptyFromAddressControl.TabIndex = 1;
			// 
			// ExportReleaseNumTextBox
			// 
			this.ExportReleaseNumTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExportReleaseNumTextBox, "JobContainer+JC_ReleaseNum");
			this.ExportReleaseNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 48, true);
			this.ExportReleaseNumTextBox.Name = "ExportReleaseNumTextBox";
			this.ExportReleaseNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ExportReleaseNumTextBox.TabIndex = 2;
			// 
			// ExportDepotCustomsReferenceTextBox
			// 
			this.ExportDepotCustomsReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExportDepotCustomsReferenceTextBox, "JobContainer+JC_ExportDepotCustomsReference");
			this.ExportDepotCustomsReferenceTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainer|655fa472-7a65-79b0-449e-393b1b1b67b5", "Export Port/Cus. Ref");
			this.ExportDepotCustomsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 48, true);
			this.ExportDepotCustomsReferenceTextBox.Name = "ExportDepotCustomsReferenceTextBox";
			this.ExportDepotCustomsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ExportDepotCustomsReferenceTextBox.TabIndex = 5;
			// 
			// ExportPenaltiesGrid
			// 
			this.ExportPenaltiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExportPenaltiesGrid, "ExportPenalties");
			this.ExportPenaltiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "CPY_PenaltyType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			penaltyTypeDescriptionColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainerPenalty|af99a483-59e0-4713-9e02-5f616d0691c6", "Penalty Desc.", "Penalty Description", "");
			penaltyTypeDescriptionColumnStyleInfo.ColumnName = "PenaltyTypeDescription";
			penaltyTypeDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo5.ColumnName = "CPY_CreditorType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CPY_OH_Creditor";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainerPenalty|9f503ac4-f2ae-47c2-944f-6e62519c7b2b", "Creditor Org.");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CPY_RL_NKLocation";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDayAndTimeEditColumnStyleInfo3.AllowNegative = false;
			zDayAndTimeEditColumnStyleInfo3.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo3.ColumnName = "CPY_FreeTime";
			zDayAndTimeEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zDayAndTimeEditColumnStyleInfo4.AllowNegative = false;
			zDayAndTimeEditColumnStyleInfo4.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo4.ColumnName = "CPY_Duration";
			zDayAndTimeEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo6.ColumnName = "CPY_TimeUnit";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CPY_PerUnitCost";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CPY_TotalCost";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CPY_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDateEditColumnStyleInfo1.ColumnName = "FirstFreeDay";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "LastFreeDay";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDayAndTimeEditColumnStyleInfo5.ColumnName = "ElapsedFreeTime";
			zDayAndTimeEditColumnStyleInfo5.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDayAndTimeEditColumnStyleInfo6.ColumnName = "ElapsedDuration";
			zDayAndTimeEditColumnStyleInfo6.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			exportFreeDaysExclusionsColumnStyleInfo.ColumnName = "FormattedFreeDayExclusions";
			exportFreeDaysExclusionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			exportFreeDaysExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("de72ba88-04b2-86ba-4e7c-2b8479282954", "Free Day Excl.", "Free Day Exclusions", "The days excluded when calculating Free Days.");
			exportDurationExclusionsColumnStyleInfo.ColumnName = "FormattedDurationExclusions";
			exportDurationExclusionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			exportDurationExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("1bc2a996-a961-7586-4a3b-08553db49be7", "Duration Excl.", "Duration Exclusions", "The days excluded when calculating Duration.");
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ExportPenaltiesGrid.ColumnStyles.Add(penaltyTypeDescriptionColumnStyleInfo);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo3);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo4);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo5);
			this.ExportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo6);
			this.ExportPenaltiesGrid.ColumnStyles.Add(exportFreeDaysExclusionsColumnStyleInfo);
			this.ExportPenaltiesGrid.ColumnStyles.Add(exportDurationExclusionsColumnStyleInfo);

			this.ExportPenaltiesGrid.GridId = "8bf4d8d5-e46c-4a06-935e-252c5e718aa2";
			this.ExportPenaltiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExportPenaltiesGrid.LayoutKey = "PenaltiesGrid";
			this.ExportPenaltiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 260, true);
			this.ExportPenaltiesGrid.Name = "ExportPenaltiesGrid";
			this.ExportPenaltiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 106, true);
			this.ExportPenaltiesGrid.TabIndex = 17;

			this.ExportTabPage.PerformLayout();
			this.ContainerYardEmptyPickupGateOutDateEdit.ResumeLayout(true);
			this.ContainerYardEmptyPickupGateOutDateEdit.PerformLayout();
			this.FCLOnBoardVesselDateEdit.ResumeLayout(true);
			this.FCLOnBoardVesselDateEdit.PerformLayout();
			this.DepartureCartageCompleteDateEdit.ResumeLayout(true);
			this.DepartureCartageCompleteDateEdit.PerformLayout();
			this.DepartureCartageAdvisedDateEdit.ResumeLayout(true);
			this.DepartureCartageAdvisedDateEdit.PerformLayout();
			this.FCLWharfGateInDateEdit.ResumeLayout(true);
			this.FCLWharfGateInDateEdit.PerformLayout();
			this.DepartureSlotDateEdit.ResumeLayout(true);
			this.DepartureSlotDateEdit.PerformLayout();
			this.ExportFullPickupDateEdit.ResumeLayout(true);
			this.ExportFullPickupDateEdit.PerformLayout();
			this.EmptyPickupByTransportModeDropEdit.ResumeLayout(true);
			this.EmptyPickupByTransportModeDropEdit.PerformLayout();
			this.ExportEmptyReqByDateEdit.ResumeLayout(true);
			this.ExportEmptyReqByDateEdit.PerformLayout();
			this.ExportPickupEmptyFromAddressControl.ResumeLayout(true);
			this.ExportPickupEmptyFromAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExportPenaltiesGrid)).EndInit();
			this.ExportPenaltiesGrid.ResumeLayout(false);
			this.ExportPenaltiesGrid.PerformLayout();
			this.SupplierBookingCodeFindBox.ResumeLayout(false);
			this.SupplierBookingCodeFindBox.PerformLayout();
			this.RelatedContainerLoadListFindBox.ResumeLayout(false);
			this.RelatedContainerLoadListFindBox.PerformLayout();
			this.RelatedContainerLoadPlanFindBox.ResumeLayout(false);
			this.RelatedContainerLoadPlanFindBox.PerformLayout();
			this.ExportTabPage.ResumeLayout(true);

		}

		protected virtual void ImportTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo penaltyTypeDescriptionColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo zDayAndTimeEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.Internal.ZDayAndTimeEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo20 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo21 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo importFreeDaysExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo importDurationExclusionsColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.EmptyReturnToTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FCLHeldForTransitStagingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FCLDatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportFclAvailableDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImportFclTimeupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LCLDatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LCLDatesOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImportLclAvailableDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImportLclTimeupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EpmtyReturnedByDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EmptyReturnByDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FCLWharfGateOutDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FCLUnloadFromVesselDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImportEstDeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImportSlotReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportSlotDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalCartageCompleteDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalCartageRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalCartageAdvisedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalPickupByRailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImportEmptyWasReturnedOnDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImportEmptyReturnReference = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportDeliverEmptyToAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FCLDatesOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImportReleaseNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportDepotCustomsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportPenaltiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EmptyReturnToTransportModeDropEdit.SuspendLayout();
			this.ImportTabPage.SuspendLayout();
			this.FCLDatesGroupBox.SuspendLayout();
			this.ImportFclAvailableDateEdit.SuspendLayout();
			this.ImportFclTimeupDateEdit.SuspendLayout();
			this.LCLDatesGroupBox.SuspendLayout();
			this.ImportLclAvailableDateEdit.SuspendLayout();
			this.ImportLclTimeupDateEdit.SuspendLayout();
			this.EpmtyReturnedByDateEdit.SuspendLayout();
			this.EmptyReturnByDateEdit.SuspendLayout();
			this.FCLWharfGateOutDateEdit.SuspendLayout();
			this.FCLUnloadFromVesselDateEdit.SuspendLayout();
			this.ImportEstDeliveryDateEdit.SuspendLayout();
			this.ImportSlotDateDateEdit.SuspendLayout();
			this.ArrivalCartageCompleteDateEdit.SuspendLayout();
			this.ArrivalCartageAdvisedDateEdit.SuspendLayout();
			this.ImportEmptyWasReturnedOnDateEdit.SuspendLayout();
			this.ImportEmptyReturnReference.SuspendLayout();
			this.ImportDeliverEmptyToAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportPenaltiesGrid)).BeginInit();
			this.ImportPenaltiesGrid.SuspendLayout();
			this.ImportTabPage.Controls.Add(this.EmptyReturnToTransportModeDropEdit);
			this.ImportTabPage.Controls.Add(this.ImportReleaseNumberTextBox);
			this.ImportTabPage.Controls.Add(this.ImportDepotCustomsReferenceTextBox);
			this.ImportTabPage.Controls.Add(this.FCLHeldForTransitStagingCheckBox);
			this.ImportTabPage.Controls.Add(this.FCLDatesGroupBox);
			this.ImportTabPage.Controls.Add(this.LCLDatesGroupBox);
			this.ImportTabPage.Controls.Add(this.EpmtyReturnedByDateEdit);
			this.ImportTabPage.Controls.Add(this.EmptyReturnByDateEdit);
			this.ImportTabPage.Controls.Add(this.FCLWharfGateOutDateEdit);
			this.ImportTabPage.Controls.Add(this.FCLUnloadFromVesselDateEdit);
			this.ImportTabPage.Controls.Add(this.ImportEstDeliveryDateEdit);
			this.ImportTabPage.Controls.Add(this.ImportSlotReferenceTextBox);
			this.ImportTabPage.Controls.Add(this.ImportSlotDateDateEdit);
			this.ImportTabPage.Controls.Add(this.ArrivalCartageCompleteDateEdit);
			this.ImportTabPage.Controls.Add(this.ArrivalCartageRefTextBox);
			this.ImportTabPage.Controls.Add(this.ArrivalCartageAdvisedDateEdit);
			this.ImportTabPage.Controls.Add(this.ArrivalPickupByRailCheckBox);
			this.ImportTabPage.Controls.Add(this.ImportEmptyWasReturnedOnDateEdit);
			this.ImportTabPage.Controls.Add(this.ImportEmptyReturnReference);
			this.ImportTabPage.Controls.Add(this.ImportDeliverEmptyToAddressControl);
			this.ImportTabPage.Controls.Add(this.ImportPenaltiesGrid);
			// 
			// EmptyReturnToTransportModeDropEdit
			// 
			this.EmptyReturnToTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EmptyReturnToTransportModeDropEdit, "JobContainer+EmptyReturnToTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).EmptyReturnToTransportMode)));
			this.EmptyReturnToTransportModeDropEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|5CB8155C-1990-427D-9A1F-CB59B3D22FA9", "By", "Transport Mode.");
			this.EmptyReturnToTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 195, true);
			this.EmptyReturnToTransportModeDropEdit.Name = "EmptyReturnToTransportModeDropEdit";
			this.EmptyReturnToTransportModeDropEdit.PreBoundMaxLength = 3;
			this.EmptyReturnToTransportModeDropEdit.ShowDescriptionBox = true;
			this.EmptyReturnToTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.EmptyReturnToTransportModeDropEdit.TabIndex = 16;
			//
			// FCLHeldForTransitStagingCheckBox
			// 
			this.FCLHeldForTransitStagingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FCLHeldForTransitStagingCheckBox, "JobContainer+JC_FCLHeldInTransitStaging");
			this.FCLHeldForTransitStagingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FCLHeldForTransitStagingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FCLHeldForTransitStagingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 153, true);
			this.FCLHeldForTransitStagingCheckBox.Name = "FCLHeldForTransitStagingCheckBox";
			this.FCLHeldForTransitStagingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.FCLHeldForTransitStagingCheckBox.TabIndex = 11;
			// 
			// FCLDatesGroupBox
			// 
			this.FCLDatesGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|de886c5b-3967-44bd-bdb7-5746bba326a1", "CTO Dates");
			this.FCLDatesGroupBox.Controls.Add(this.FCLDatesOverrideCheckBox);
			this.FCLDatesGroupBox.Controls.Add(this.ImportFclAvailableDateEdit);
			this.FCLDatesGroupBox.Controls.Add(this.ImportFclTimeupDateEdit);
			this.FCLDatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.FCLDatesGroupBox.Name = "FCLDatesGroupBox";
			this.FCLDatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 62, true);
			this.FCLDatesGroupBox.TabIndex = 0;
			this.FCLDatesGroupBox.TabStop = false;
			// 
			// ImportFclAvailableDateEdit
			// 
			this.ImportFclAvailableDateEdit.AllowDrop = true;
			this.ImportFclAvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportFclAvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportFclAvailableDateEdit, "JobContainer+JC_FCLAvailable");
			this.ImportFclAvailableDateEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|84528330-c434-424d-a92e-f4742321aeb4", "CTO Available");
			this.ImportFclAvailableDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ImportFclAvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 15, true);
			this.ImportFclAvailableDateEdit.Name = "ImportFclAvailableDateEdit";
			this.ImportFclAvailableDateEdit.TabIndex = 1;
			// 
			// ImportFclTimeupDateEdit
			// 
			this.ImportFclTimeupDateEdit.AllowDrop = true;
			this.ImportFclTimeupDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportFclTimeupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportFclTimeupDateEdit, "JobContainer+JC_ArrivalCTOStorageStartDate");
			this.ImportFclTimeupDateEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|0af0099f-5b69-436c-b9d1-b0fce6ad4efe", "CTO Storage Start");
			this.ImportFclTimeupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ImportFclTimeupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 36, true);
			this.ImportFclTimeupDateEdit.Name = "ImportFclTimeupDateEdit";
			this.ImportFclTimeupDateEdit.TabIndex = 2;
			// 
			// LCLDatesGroupBox
			// 
			this.LCLDatesGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersUserControl|ee2f2012-f410-45a2-9518-101e12401c2a", "CFS Dates");
			this.LCLDatesGroupBox.Controls.Add(this.LCLDatesOverrideCheckBox);
			this.LCLDatesGroupBox.Controls.Add(this.ImportLclAvailableDateEdit);
			this.LCLDatesGroupBox.Controls.Add(this.ImportLclTimeupDateEdit);
			this.LCLDatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 5, true);
			this.LCLDatesGroupBox.Name = "LCLDatesGroupBox";
			this.LCLDatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 62, true);
			this.LCLDatesGroupBox.TabIndex = 1;
			this.LCLDatesGroupBox.TabStop = false;
			// 
			// LCLDatesOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.LCLDatesOverrideCheckBox, "JobContainer+JC_OverrideLCLAvailableStorage");
			this.LCLDatesOverrideCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LCLDatesOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LCLDatesOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, -1, true);
			this.LCLDatesOverrideCheckBox.Name = "LCLDatesOverrideCheckBox";
			this.LCLDatesOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.LCLDatesOverrideCheckBox.TabIndex = 0;
			// 
			// ImportLclAvailableDateEdit
			// 
			this.ImportLclAvailableDateEdit.AllowDrop = true;
			this.ImportLclAvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportLclAvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportLclAvailableDateEdit, "JobContainer+JC_LCLAvailable");
			this.ImportLclAvailableDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ImportLclAvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 16, true);
			this.ImportLclAvailableDateEdit.Name = "ImportLclAvailableDateEdit";
			this.ImportLclAvailableDateEdit.TabIndex = 1;
			// 
			// ImportLclTimeupDateEdit
			// 
			this.ImportLclTimeupDateEdit.AllowDrop = true;
			this.ImportLclTimeupDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportLclTimeupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportLclTimeupDateEdit, "JobContainer+JC_LCLStorageCommences");
			this.ImportLclTimeupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ImportLclTimeupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 37, true);
			this.ImportLclTimeupDateEdit.Name = "ImportLclTimeupDateEdit";
			this.ImportLclTimeupDateEdit.TabIndex = 2;
			// 
			// EpmtyReturnedByDateEdit
			// 
			this.EpmtyReturnedByDateEdit.AllowDrop = true;
			this.EpmtyReturnedByDateEdit.AutoCompleteMonthThreshold = 1;
			this.EpmtyReturnedByDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EpmtyReturnedByDateEdit, "JobContainer+JC_EmptyReadyForReturn");
			this.EpmtyReturnedByDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EpmtyReturnedByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 237, true);
			this.EpmtyReturnedByDateEdit.Name = "EpmtyReturnedByDateEdit";
			this.EpmtyReturnedByDateEdit.TabIndex = 19;
			//
			// EmptyReturnByDateEdit
			// 
			this.EmptyReturnByDateEdit.AllowDrop = true;
			this.EmptyReturnByDateEdit.AutoCompleteMonthThreshold = 1;
			this.EmptyReturnByDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EmptyReturnByDateEdit, "JobContainer+JC_EmptyReturnedBy");
			this.EmptyReturnByDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EmptyReturnByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 216, true);
			this.EmptyReturnByDateEdit.Name = "EmptyReturnByDateEdit";
			this.EmptyReturnByDateEdit.TabIndex = 17;
			//
			// FCLWharfGateOutDateEdit
			// 
			this.FCLWharfGateOutDateEdit.AllowDrop = true;
			this.FCLWharfGateOutDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCLWharfGateOutDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCLWharfGateOutDateEdit, "JobContainer+JC_FCLWharfGateOut");
			this.FCLWharfGateOutDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCLWharfGateOutDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 132, true);
			this.FCLWharfGateOutDateEdit.Name = "FCLWharfGateOutDateEdit";
			this.FCLWharfGateOutDateEdit.TabIndex = 8;
			// 
			// FCLUnloadFromVesselDateEdit
			// 
			this.FCLUnloadFromVesselDateEdit.AllowDrop = true;
			this.FCLUnloadFromVesselDateEdit.AutoCompleteMonthThreshold = 1;
			this.FCLUnloadFromVesselDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FCLUnloadFromVesselDateEdit, "JobContainer+JC_FCLUnloadFromVessel");
			this.FCLUnloadFromVesselDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FCLUnloadFromVesselDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 69, true);
			this.FCLUnloadFromVesselDateEdit.Name = "FCLUnloadFromVesselDateEdit";
			this.FCLUnloadFromVesselDateEdit.TabIndex = 2;
			// 
			// ImportEstDeliveryDateEdit
			// 
			this.ImportEstDeliveryDateEdit.AllowDrop = true;
			this.ImportEstDeliveryDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportEstDeliveryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportEstDeliveryDateEdit, "JobContainer+JC_ArrivalEstimatedDelivery");
			this.ImportEstDeliveryDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ImportEstDeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 153, true);
			this.ImportEstDeliveryDateEdit.Name = "ImportEstDeliveryDateEdit";
			this.ImportEstDeliveryDateEdit.TabIndex = 10;
			// 
			// ImportSlotReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportSlotReferenceTextBox, "JobContainer+JC_ArrivalSlotReference");
			this.ImportSlotReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 111, true);
			this.ImportSlotReferenceTextBox.Name = "ImportSlotReferenceTextBox";
			this.ImportSlotReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ImportSlotReferenceTextBox.TabIndex = 7;
			// 
			// ImportSlotDateDateEdit
			// 
			this.ImportSlotDateDateEdit.AllowDrop = true;
			this.ImportSlotDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportSlotDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportSlotDateDateEdit, "JobContainer+JC_ArrivalSlotDateTime");
			this.ImportSlotDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ImportSlotDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 111, true);
			this.ImportSlotDateDateEdit.Name = "ImportSlotDateDateEdit";
			this.ImportSlotDateDateEdit.TabIndex = 6;
			// 
			// ArrivalCartageCompleteDateEdit
			// 
			this.ArrivalCartageCompleteDateEdit.AllowDrop = true;
			this.ArrivalCartageCompleteDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalCartageCompleteDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalCartageCompleteDateEdit, "JobContainer+JC_ArrivalCartageComplete");
			this.ArrivalCartageCompleteDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ArrivalCartageCompleteDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 174, true);
			this.ArrivalCartageCompleteDateEdit.Name = "ArrivalCartageCompleteDateEdit";
			this.ArrivalCartageCompleteDateEdit.TabIndex = 12;
			// 
			this.ArrivalCartageCompleteDateEdit.TabIndex = 13;
			// ArrivalCartageRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalCartageRefTextBox, "JobContainer+JC_ArrivalCartageRef");
			this.ArrivalCartageRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 90, true);
			this.ArrivalCartageRefTextBox.Name = "ArrivalCartageRefTextBox";
			this.ArrivalCartageRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ArrivalCartageRefTextBox.TabIndex = 5;
			// 
			// ArrivalCartageAdvisedDateEdit
			// 
			this.ArrivalCartageAdvisedDateEdit.AllowDrop = true;
			this.ArrivalCartageAdvisedDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalCartageAdvisedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalCartageAdvisedDateEdit, "JobContainer+JC_ArrivalCartageAdvised");
			this.ArrivalCartageAdvisedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ArrivalCartageAdvisedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 90, true);
			this.ArrivalCartageAdvisedDateEdit.Name = "ArrivalCartageAdvisedDateEdit";
			this.ArrivalCartageAdvisedDateEdit.TabIndex = 4;
			// 
			// ArrivalPickupByRailCheckBox
			// 
			this.ArrivalPickupByRailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ArrivalPickupByRailCheckBox, "JobContainer+JC_ArrivalPickupByRail");
			this.ArrivalPickupByRailCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ArrivalPickupByRailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ArrivalPickupByRailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 153, true);
			this.ArrivalPickupByRailCheckBox.Name = "ArrivalPickupByRailCheckBox";
			this.ArrivalPickupByRailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ArrivalPickupByRailCheckBox.TabIndex = 13;
			// 
			this.ArrivalPickupByRailCheckBox.TabIndex = 12;
			// ImportEmptyWasReturnedOnDateEdit
			// 
			this.ImportEmptyWasReturnedOnDateEdit.AllowDrop = true;
			this.ImportEmptyWasReturnedOnDateEdit.AutoCompleteMonthThreshold = 1;
			this.ImportEmptyWasReturnedOnDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ImportEmptyWasReturnedOnDateEdit, "JobContainer+JC_ContainerYardEmptyReturnGateIn");
			this.ImportEmptyWasReturnedOnDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ImportEmptyWasReturnedOnDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 216, true);
			this.ImportEmptyWasReturnedOnDateEdit.Name = "ImportEmptyWasReturnedOnDateEdit";
			this.ImportEmptyWasReturnedOnDateEdit.TabIndex = 19;
			//
			// ImportEmptyReturnReference
			// 
			this.BindingSource.SetBindingMember(this.ImportEmptyReturnReference, "JobContainer+JC_EmptyReturnReference");
			this.ImportEmptyReturnReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 237, true);
			this.ImportEmptyReturnReference.Name = "ImportEmptyReturnReference";
			this.ImportEmptyReturnReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ImportEmptyReturnReference.TabIndex = 19;
			//
			// ImportDeliverEmptyToAddressControl
			// 
			this.ImportDeliverEmptyToAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportDeliverEmptyToAddressControl, "JobContainer+JC_OA_ArrivalContainerYardAddress");
			this.ImportDeliverEmptyToAddressControl.BindToOrgList = "JobContainer+Lookups+ContainerYard_List";
			this.ImportDeliverEmptyToAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 195, true);
			this.ImportDeliverEmptyToAddressControl.Name = "ImportDeliverEmptyToAddressControl";
			this.ImportDeliverEmptyToAddressControl.PopupCaption = null;
			this.ImportDeliverEmptyToAddressControl.ReadOnly = false;
			this.ImportDeliverEmptyToAddressControl.ShowAddress = false;
			this.ImportDeliverEmptyToAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
			this.ImportDeliverEmptyToAddressControl.TabIndex = 14;
			// 
			this.ImportDeliverEmptyToAddressControl.TabIndex = 15;
			// FCLDatesOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FCLDatesOverrideCheckBox, "JobContainer+JC_OverrideFCLAvailableStorage");
			this.FCLDatesOverrideCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FCLDatesOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FCLDatesOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, -1, true);
			this.FCLDatesOverrideCheckBox.Name = "FCLDatesOverrideCheckBox";
			this.FCLDatesOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.FCLDatesOverrideCheckBox.TabIndex = 0;
			// 
			// ImportReleaseNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportReleaseNumberTextBox, "JobContainer+JC_ContainerImportDORelease");
			this.ImportReleaseNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 132, true);
			this.ImportReleaseNumberTextBox.Name = "ImportReleaseNumberTextBox";
			this.ImportReleaseNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ImportReleaseNumberTextBox.TabIndex = 9;
			// 
			// ImportDepotCustomsReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportDepotCustomsReferenceTextBox, "JobContainer+JC_ImportDepotCustomsReference");
			this.ImportDepotCustomsReferenceTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainer|4e1a4e8a-e827-60a9-4d3f-8de8aa20a120", "Port/Customs Ref");
			this.ImportDepotCustomsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 69, true);
			this.ImportDepotCustomsReferenceTextBox.Name = "ImportDepotCustomsReferenceTextBox";
			this.ImportDepotCustomsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ImportDepotCustomsReferenceTextBox.TabIndex = 3;
			// 
			// PenaltiesGrid
			// 
			this.ImportPenaltiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ImportPenaltiesGrid, "ImportPenalties");
			this.ImportPenaltiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo8.ColumnName = "CPY_PenaltyType";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			penaltyTypeDescriptionColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainerPenalty|af99a483-59e0-4713-9e02-5f616d0691c6", "Penalty Desc.", "Penalty Description", "");
			penaltyTypeDescriptionColumnStyleInfo.ColumnName = "PenaltyTypeDescription";
			penaltyTypeDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo9.ColumnName = "CPY_CreditorType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zDropEditColumnStyleInfo10.ColumnName = "CPY_TimeUnit";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDayAndTimeEditColumnStyleInfo2.AllowNegative = false;
			zDayAndTimeEditColumnStyleInfo2.ColumnName = "CPY_FreeTime";
			zDayAndTimeEditColumnStyleInfo2.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zDayAndTimeEditColumnStyleInfo3.AllowNegative = false;
			zDayAndTimeEditColumnStyleInfo3.ColumnName = "CPY_Duration";
			zDayAndTimeEditColumnStyleInfo3.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo20.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo20.ColumnName = "CPY_PerUnitCost";
			zCalcEditColumnStyleInfo20.Decimals = 2;
			zCalcEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo21.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo21.ColumnName = "CPY_TotalCost";
			zCalcEditColumnStyleInfo21.Decimals = 2;
			zCalcEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CPY_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CPY_RL_NKLocation";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CPY_OH_Creditor";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("JobContainerPenalty|fc34c008-a6cd-427d-8d5e-a94bcefbcda8", "Creditor Org.");
			zDateEditColumnStyleInfo1.ColumnName = "FirstFreeDay";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "LastFreeDay";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDayAndTimeEditColumnStyleInfo4.ColumnName = "ElapsedFreeTime";
			zDayAndTimeEditColumnStyleInfo4.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDayAndTimeEditColumnStyleInfo5.ColumnName = "ElapsedDuration";
			zDayAndTimeEditColumnStyleInfo5.BindToTimeUnit = "CPY_TimeUnit";
			zDayAndTimeEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			importFreeDaysExclusionsColumnStyleInfo.ColumnName = "FormattedFreeDayExclusions";
			importFreeDaysExclusionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			importFreeDaysExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("bcdd6691-f0d0-68be-493a-55a8ad0871f6", "Free Day Excl.", "Free Day Exclusion");
			importDurationExclusionsColumnStyleInfo.ColumnName = "FormattedDurationExclusions";
			importDurationExclusionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			importDurationExclusionsColumnStyleInfo.CaptionResourceString = Res.GetData("ae0d9715-9906-b0ab-45ff-88e30f9cdc41", "Duration Excl.", "Duration Exclusion");
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ImportPenaltiesGrid.ColumnStyles.Add(penaltyTypeDescriptionColumnStyleInfo);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo2);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo3);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo20);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo4);
			this.ImportPenaltiesGrid.ColumnStyles.Add(zDayAndTimeEditColumnStyleInfo5);
			this.ImportPenaltiesGrid.ColumnStyles.Add(importFreeDaysExclusionsColumnStyleInfo);
			this.ImportPenaltiesGrid.ColumnStyles.Add(importDurationExclusionsColumnStyleInfo);

			this.ImportPenaltiesGrid.GridId = "8bf4d8d5-e46c-4a06-935e-252c5e718aa2";
			this.ImportPenaltiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportPenaltiesGrid.LayoutKey = "PenaltiesGrid";
			this.ImportPenaltiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 260, true);
			this.ImportPenaltiesGrid.Name = "PenaltiesGrid";
			this.ImportPenaltiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 106, true);
			this.ImportPenaltiesGrid.TabIndex = 20;
			this.ImportTabPage.PerformLayout();
			this.EmptyReturnToTransportModeDropEdit.ResumeLayout(true);
			this.EmptyReturnToTransportModeDropEdit.PerformLayout();
			this.FCLDatesGroupBox.ResumeLayout(false);
			this.FCLDatesGroupBox.PerformLayout();
			this.ImportFclAvailableDateEdit.ResumeLayout(true);
			this.ImportFclAvailableDateEdit.PerformLayout();
			this.ImportFclTimeupDateEdit.ResumeLayout(true);
			this.ImportFclTimeupDateEdit.PerformLayout();
			this.LCLDatesGroupBox.ResumeLayout(false);
			this.LCLDatesGroupBox.PerformLayout();
			this.ImportLclAvailableDateEdit.ResumeLayout(true);
			this.ImportLclAvailableDateEdit.PerformLayout();
			this.ImportLclTimeupDateEdit.ResumeLayout(true);
			this.ImportLclTimeupDateEdit.PerformLayout();
			this.EpmtyReturnedByDateEdit.ResumeLayout(true);
			this.EpmtyReturnedByDateEdit.PerformLayout();
			this.EmptyReturnByDateEdit.ResumeLayout(true);
			this.EmptyReturnByDateEdit.PerformLayout();
			this.FCLWharfGateOutDateEdit.ResumeLayout(true);
			this.FCLWharfGateOutDateEdit.PerformLayout();
			this.FCLUnloadFromVesselDateEdit.ResumeLayout(true);
			this.FCLUnloadFromVesselDateEdit.PerformLayout();
			this.ImportEstDeliveryDateEdit.ResumeLayout(true);
			this.ImportEstDeliveryDateEdit.PerformLayout();
			this.ImportSlotDateDateEdit.ResumeLayout(true);
			this.ImportSlotDateDateEdit.PerformLayout();
			this.ArrivalCartageCompleteDateEdit.ResumeLayout(true);
			this.ArrivalCartageCompleteDateEdit.PerformLayout();
			this.ArrivalCartageAdvisedDateEdit.ResumeLayout(true);
			this.ArrivalCartageAdvisedDateEdit.PerformLayout();
			this.ImportEmptyWasReturnedOnDateEdit.ResumeLayout(true);
			this.ImportEmptyWasReturnedOnDateEdit.PerformLayout();
			this.ImportEmptyReturnReference.ResumeLayout(true);
			this.ImportEmptyReturnReference.PerformLayout();
			this.ImportDeliverEmptyToAddressControl.ResumeLayout(true);
			this.ImportDeliverEmptyToAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportPenaltiesGrid)).EndInit();
			this.ImportPenaltiesGrid.ResumeLayout(false);
			this.ImportPenaltiesGrid.PerformLayout();
			this.ImportTabPage.ResumeLayout(true);

		}

		#endregion

		private ZLabel MeasuresLowerOnfileLabel;
		private ZLabel MeasuresCalculatedLabel;
		private ZLabel ShipmentTotalLabel;
		private ZCalcEdit CalculatedVolumeOnFileCalcEdit;
		private ZCalcEdit MaxVolumeOnFileCalcEdit;
		private ZCalcEdit ShipmentTotalVolumeCalcEdit;
		protected ZTextBox edContainerNum;
		protected ZTextBox edSealNum;
		protected ZGroupBox WeightsGroupBox;
		private ZDropEdit GrossWeightUQDropEdit;
		private ZCalcEdit ActualTareWeightCalcEdit;
		private ZCalcEdit ActualNetWeightCalcEdit;
		private ZCalcEdit ActualDunnageCalcEdit;
		private ZLabel ActualWeightLabel;
		private ZLabel OnFileWeightLabel;
		private ZCalcEdit TotalGrossWeightCalcEdit;
		private ZCalcEdit MaxGrossWeightOnFileCalcEdit;
		protected ZDropEdit ExportContainerModeDropEdit;
		protected ZGuidFindBox ExportContainerTypeGuidFindBox;
		protected ZTabPage ImportTabPage;
		protected ZTabPage ExportTabPage;
		private ZDateEdit DepartureSlotDateEdit;
		protected ZGroupBox DetailsGroupBox;
		private ZCheckBox IsArrivingAtCTOByRailCheckBox;
		private ZCodeFindBox CommodityCodeFindBox;
		protected ZCheckBox ExportIsDamagedCheckBox;
		protected ZCheckBox ExportIsEmptyContainerCheckBox;
		private ZDateEdit ExportFullPickupDateEdit;
		protected ZDateEdit ExportEmptyReqByDateEdit;
		private ZTextBox ExportReleaseNumTextBox;
		private ZTextBox ExportDepotCustomsReferenceTextBox;
		public ZTemplateTabControl DetailTabControl;
		private ZDateEdit ImportLclTimeupDateEdit;
		private ZDateEdit ImportEmptyWasReturnedOnDateEdit;
		private ZTextBox ImportEmptyReturnReference;
		private ZDateEdit ImportEstDeliveryDateEdit;
		private ZDateEdit ImportLclAvailableDateEdit;
		protected ZCheckBox ImportIsSealOkCheckBox;
		private ZDateEdit ImportFclTimeupDateEdit;
		private ZDateEdit ImportFclAvailableDateEdit;
		private ZDateEdit ImportSlotDateDateEdit;
		private ZDateEdit FCLWharfGateInDateEdit;
		private ZDateEdit DepartureCartageAdvisedDateEdit;
		private ZTextBox DepartureCartageRefTextBox;
		private ZDateEdit DepartureCartageCompleteDateEdit;
		private ZDateEdit FCLOnBoardVesselDateEdit;
		private ZCheckBox ArrivalPickupByRailCheckBox;
		private ZDateEdit ArrivalCartageCompleteDateEdit;
		private ZTextBox ArrivalCartageRefTextBox;
		private ZDateEdit ArrivalCartageAdvisedDateEdit;
		private ZDateEdit FCLUnloadFromVesselDateEdit;
		private ZDateEdit FCLWharfGateOutDateEdit;
		private ZDateEdit EmptyReturnByDateEdit;
		private ZDropEdit EmptyReturnToTransportModeDropEdit;
		private ZDropEdit EmptyPickupByTransportModeDropEdit;
		private ZDateEdit EpmtyReturnedByDateEdit;
		private ZDateEdit ContainerYardEmptyPickupGateOutDateEdit;
		private ZCalcEdit TareOnFileCalcEdit;
		private ZTabPage ReeferTabPage;
		private ZTabPage ServicesTabPage;
		private ZDropEdit ServiceCodeDropEdit;
		private ZGrid ServicesGrid;
		private ZButton DefaultManifestedValuesToOutturnButton;
		private ZGroupBox DefaultManifestedValuesToOutturnGroupBox;
		private ZButton DefaultManifestedValuesToOutturnButtonForAllContainers;
		private ZTextBox ES_ReferTextBox;
		private ZDateEdit ES_BookedDateEdit;
		private ZDateEdit ServiceCompletedDateEdit;
		private ZTimeEdit ServiceDurationTimeEdit;
		private ZCalcEdit ServiceCountCalcEdit;
		private ZAddressControl ServiceLocationAddressControl;
		private ZCalcFindBox rateAndCurrencyCalcFindBox;
		private ZDropEdit measurementBasisDropEdit;
		private ZTextBox NotesTextBox;
		private ZTextBox MarksAndNoTextBox;
		private ZTextBox OutturnCommentTextBox;
		private ZCalcEdit OutturnWeightCalcEdit;
		private ZGrid OutturnGrid;
		private ZGroupBox LCLDatesGroupBox;
		private ZCheckBox LCLDatesOverrideCheckBox;
		private ZGroupBox FCLDatesGroupBox;
		private ZCheckBox FCLHeldForTransitStagingCheckBox;
		private ZTabControl outturnSubTabControl;
		private ZTabPage packlineDetailsTab;
		private ZTabPage packLocationTab;
		public ZTabPage OutturnTabPage;
		private ZDropEdit ContainerStatusDropEdit;
		private ZDropEdit ContainerQualityDropEdit;
		private ZCalcEdit OuuternedVolumeCalcEdit;
		private ZCheckBox FCLDatesOverrideCheckBox;
		protected ZCheckBox JC_IsShipperOwnedCheckBox;
		private ZGuidFindBox contractorFindBox;
		private ZTabPage MeasuresTabPage;
		private PackLineLocationsControl locationsControl;
		private OverhangControl overhangControl;
		private ReeferUserControl reeferUserControl;
		protected ZDropEdit DeliveryModeDropEdit;
		public ZTabPage VGMTabPage;
		public ZTabPage FreightRatesTabPage;
		private ZGroupBox VerifiedWeightGroupBox;
		private ZDropEdit VerifiedMethodDropEdit;
		private ZDropEdit VGMStatusDropEdit;
		private ZDateEdit VerifiedDateEdit;
		private MasterFiles.GUI.ZDocAddressControl GrossWeightVerifiedByDocAddressControl;
		FreightRateControl SellSpotRate;
		FreightRateControl CostSpotRate;
		FreightRateControl GatewaySellSpotRate;
		ZCodeFindBox RatingCommodity;
		ZLabel MeasureLabel;
		ZCalcEdit PivotBreakCalcEdit;
		private ZTabPage NumbersTabPage;
		private MasterFiles.GUI.NumbersControl numbersControl1;
		protected ZTextBox DepartureSlotReferenceTextBox;
		protected ZAddressControl ExportPickupEmptyFromAddressControl;
		protected ZTextBox ImportSlotReferenceTextBox;
		protected ZAddressControl ImportDeliverEmptyToAddressControl;
		public ZTextBox ImportReleaseNumberTextBox;
		public ZTextBox ImportDepotCustomsReferenceTextBox;
		private ZGrid ImportPenaltiesGrid;
		private ZGrid ExportPenaltiesGrid;
		private ZGuidFindBox SupplierBookingCodeFindBox;
		public ZGuidFindBox RelatedContainerLoadListFindBox;
		private ZGuidFindBox RelatedContainerLoadPlanFindBox;
	}
}
