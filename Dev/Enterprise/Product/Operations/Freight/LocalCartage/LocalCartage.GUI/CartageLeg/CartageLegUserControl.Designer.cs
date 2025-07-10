using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageLegUserControl
	{
		private BookedMoveControl CartageBookedMovePanel;
		private ZGroupBox zGroupBox1;
		private ZGroupBox zGroupBox2;
		private ZPanel zPanel1;
		private ZGroupBox LooseDetailsGroupBox;
		internal ZLinkLabel DGLinkLabel;
		internal ZGuidFindBox DGSubstanceGuidFindBox;
		private ZLabel zLabel9;
		private ZLabel zLabel12;
		private ZCalcEdit LooseEW_BookedHeightCalcEdit;
		private ZCalcEdit LooseEW_BookedWidthCalcEdit;
		internal ZCalcEdit FlashPointCalcEdit;
		internal ZGuidFindBox DGContactGuidFindBox;
		private ZCalcDropEdit LooseEW_BookedLengthCalcDropEdit;
		private ZCalcDropEdit EW_BookedPackCountCalcDropEdit;
		private ZCalcDropEdit LooseEW_BookedWeightCalcDropEdit;
		private ZCalcDropEdit LooseEW_BookedVolumeCalcDropEdit;
		private ZGroupBox RefrigerationGroupBox;
		private ZCheckBox ExportIsControlledAtmosphereCheckBox;
		private ZCheckBox ExportIsFrozenCheckBox;
		private ZCheckBox ExportIsChillerCheckBox;
		private ZTextBox TempRecorderSerialNoTextBox;
		private ZCalcDropEdit JY_AirVentFlowRateCalcDropEdit;
		private ZCalcDropEdit JY_SetPointTempCalcDropEdit;
		private ZTextBox ReeferGeneratorTextBox;
		private ZCalcEdit JC_HumidityPercentCalcEdit;
		private ZGroupBox ContainerGroupBox;
		private ZDropEdit zDropEdit3;
		private ZCalcEdit zCalcEdit1;
		private ZGuidFindBox zGuidFindBox8;
		private ZTextBox zTextBox6;
		private ZTextBox ContainerTextBox;
		private ZPanel zPanel2;
		private ZGuidFindBox zGuidFindBox1;
		private ZButton OpenLocalTransportButton;
		private ProcessTemplateCustomFieldsControl customFieldsDisplayControl1;
		private ZGroupBox CustomFieldsGroupBox;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel1;
		internal CartageLegControl CartageLegPanel;

		private void InitializeComponent()
		{
			this.CartageBookedMovePanel = new Enterprise.Freight.LocalCartage.GUI.BookedMoveControl();
			this.CartageLegPanel = new Enterprise.Freight.LocalCartage.GUI.CartageLegControl();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OpenLocalTransportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.customFieldsDisplayControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.LooseDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DGLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.DGSubstanceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.LooseEW_BookedHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LooseEW_BookedWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FlashPointCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DGContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LooseEW_BookedLengthCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.EW_BookedPackCountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LooseEW_BookedWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LooseEW_BookedVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.RefrigerationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ExportIsControlledAtmosphereCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportIsFrozenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportIsChillerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TempRecorderSerialNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JY_AirVentFlowRateCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JY_SetPointTempCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ReeferGeneratorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JC_HumidityPercentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContainerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zGuidFindBox8 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.CustomFieldsGroupBox.SuspendLayout();
			this.LooseDetailsGroupBox.SuspendLayout();
			this.RefrigerationGroupBox.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.ContainerGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartageLeg);
			// 
			// CartageBookedMovePanel
			// 
			this.CartageBookedMovePanel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CartageBookedMovePanel, "BookedCtgMove");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove)));
			this.CartageBookedMovePanel.BookedMoveLayout = Enterprise.Freight.LocalCartage.GUI.BookedMoveControl.BookedMovesLayout.CartageLeg;
			this.CartageBookedMovePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CartageBookedMovePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CartageBookedMovePanel.Name = "CartageBookedMovePanel";
			this.CartageBookedMovePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 133, true);
			this.CartageBookedMovePanel.TabIndex = 4;
			// 
			// CartageLegPanel
			// 
			this.CartageLegPanel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CartageLegPanel, ".");
			this.CartageLegPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CartageLegPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 36, true);
			this.CartageLegPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.CartageLegPanel.Name = "CartageLegPanel";
			this.CartageLegPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 181, true);
			this.CartageLegPanel.TabIndex = 3;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegUserControl|4573c945-414a-4f3a-9416-5c28d7658122", "Booking Details");
			this.zGroupBox1.Controls.Add(this.CartageBookedMovePanel);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 220, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 152, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegUserControl|8e3409b1-cabd-4cc9-bc5a-e56f37cc1c7d", "Leg Details");
			this.zGroupBox2.Controls.Add(this.CartageLegPanel);
			this.zGroupBox2.Controls.Add(this.zPanel2);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 220, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.OpenLocalTransportButton);
			this.zPanel2.Controls.Add(this.zGuidFindBox1);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 20, true);
			this.zPanel2.TabIndex = 4;
			// 
			// OpenLocalTransportButton
			// 
			this.OpenLocalTransportButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegUserControl|c75cd3b4-81da-43a9-9a94-b9f20e88a4b0", "Open Port Transport Job");
			this.OpenLocalTransportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 0, true);
			this.OpenLocalTransportButton.Name = "OpenLocalTransportButton";
			this.OpenLocalTransportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.OpenLocalTransportButton.TabIndex = 1;
			this.OpenLocalTransportButton.UseVisualStyleBackColor = true;
			this.OpenLocalTransportButton.Click += new System.EventHandler(this.OpenLocalTransportButton_Click);
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "BookedCtgMove+EW_JJ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_JJ)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 0, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.ShowDescriptionBox = false;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zGuidFindBox1.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.CustomFieldsGroupBox);
			this.zPanel1.Controls.Add(this.LooseDetailsGroupBox);
			this.zPanel1.Controls.Add(this.RefrigerationGroupBox);
			this.zPanel1.Controls.Add(this.ContainerGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 372, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 151, true);
			this.zPanel1.TabIndex = 8;
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegUserControl|40e8fec5-7f27-4a32-bf93-6343a1fe76a0", "Workflow Custom Fields");
			this.CustomFieldsGroupBox.Controls.Add(this.customFieldsDisplayControl1);
			this.CustomFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(867, 0, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 3, 8, 8, true);
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 151, true);
			this.CustomFieldsGroupBox.TabIndex = 10;
			this.CustomFieldsGroupBox.TabStop = false;
			// 
			// customFieldsDisplayControl1
			// 
			this.customFieldsDisplayControl1.AllowDrop = true;
			this.customFieldsDisplayControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customFieldsDisplayControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.customFieldsDisplayControl1.Name = "customFieldsDisplayControl1";
			this.customFieldsDisplayControl1.NothingSetupMessageLabelText = "";
			this.customFieldsDisplayControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 127, true);
			this.customFieldsDisplayControl1.TabIndex = 9;
			// 
			// LooseDetailsGroupBox
			// 
			this.LooseDetailsGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegUserControl|78d53505-2402-4e78-a6cb-0315964401a1", "Loose Details");
			this.LooseDetailsGroupBox.Controls.Add(this.DGLinkLabel);
			this.LooseDetailsGroupBox.Controls.Add(this.DGSubstanceGuidFindBox);
			this.LooseDetailsGroupBox.Controls.Add(this.zLabel9);
			this.LooseDetailsGroupBox.Controls.Add(this.zLabel12);
			this.LooseDetailsGroupBox.Controls.Add(this.LooseEW_BookedHeightCalcEdit);
			this.LooseDetailsGroupBox.Controls.Add(this.LooseEW_BookedWidthCalcEdit);
			this.LooseDetailsGroupBox.Controls.Add(this.FlashPointCalcEdit);
			this.LooseDetailsGroupBox.Controls.Add(this.DGContactGuidFindBox);
			this.LooseDetailsGroupBox.Controls.Add(this.LooseEW_BookedLengthCalcDropEdit);
			this.LooseDetailsGroupBox.Controls.Add(this.EW_BookedPackCountCalcDropEdit);
			this.LooseDetailsGroupBox.Controls.Add(this.LooseEW_BookedWeightCalcDropEdit);
			this.LooseDetailsGroupBox.Controls.Add(this.LooseEW_BookedVolumeCalcDropEdit);
			this.LooseDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.LooseDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 0, true);
			this.LooseDetailsGroupBox.Name = "LooseDetailsGroupBox";
			this.LooseDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 151, true);
			this.LooseDetailsGroupBox.TabIndex = 2;
			this.LooseDetailsGroupBox.TabStop = false;
			// 
			// DGLinkLabel
			// 
			this.DGLinkLabel.AutoSize = true;
			this.DGLinkLabel.IsFontBold = false;
			this.DGLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 39, true);
			this.DGLinkLabel.Name = "DGLinkLabel";
			this.DGLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.DGLinkLabel.TabIndex = 9;
			this.DGLinkLabel.TabStop = false;
			// 
			// DGSubstanceCodeFindBox
			// 
			this.DGSubstanceGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGSubstanceGuidFindBox, "BookedCtgMove+UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.DGSubstanceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 13, true);
			this.DGSubstanceGuidFindBox.Name = "DGSubstanceGuidFindBox";
			this.DGSubstanceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.DGSubstanceGuidFindBox.TabIndex = 8;
			// 
			// zLabel9
			// 
			this.BindingSource.SetBindingMember(this.zLabel9, "BookedCtgMove+EW_DimUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_DimUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel9, false);
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 122, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.zLabel9.TabIndex = 7;
			// 
			// zLabel12
			// 
			this.BindingSource.SetBindingMember(this.zLabel12, "BookedCtgMove+EW_DimUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_DimUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel12, false);
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 100, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.zLabel12.TabIndex = 5;
			// 
			// LooseEW_BookedHeightCalcEdit
			// 
			this.LooseEW_BookedHeightCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LooseEW_BookedHeightCalcEdit, "BookedCtgMove+EW_BookedHeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_BookedHeight)));
			this.LooseEW_BookedHeightCalcEdit.DecimalPlaces = 2;
			this.LooseEW_BookedHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 123, true);
			this.LooseEW_BookedHeightCalcEdit.Name = "LooseEW_BookedHeightCalcEdit";
			this.LooseEW_BookedHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.LooseEW_BookedHeightCalcEdit.TabIndex = 6;
			this.LooseEW_BookedHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LooseEW_BookedWidthCalcEdit
			// 
			this.LooseEW_BookedWidthCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LooseEW_BookedWidthCalcEdit, "BookedCtgMove+EW_BookedWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_BookedWidth)));
			this.LooseEW_BookedWidthCalcEdit.DecimalPlaces = 2;
			this.LooseEW_BookedWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 101, true);
			this.LooseEW_BookedWidthCalcEdit.Name = "LooseEW_BookedWidthCalcEdit";
			this.LooseEW_BookedWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.LooseEW_BookedWidthCalcEdit.TabIndex = 4;
			this.LooseEW_BookedWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FlashPointCalcEdit
			// 
			this.FlashPointCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FlashPointCalcEdit, "BookedCtgMove+UNDGs+FirstItemForBinding.DI_DGFlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.UNDGs.FirstItemForBinding)).SyncRoot)).DI_DGFlashPoint)));
			this.FlashPointCalcEdit.DecimalPlaces = 1;
			this.FlashPointCalcEdit.Decimals = 1;
			this.FlashPointCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 35, true);
			this.FlashPointCalcEdit.Name = "FlashPointCalcEdit";
			this.FlashPointCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.FlashPointCalcEdit.TabIndex = 10;
			this.FlashPointCalcEdit.Text = "0.0";
			this.FlashPointCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DGContactGuidFindBox
			// 
			this.DGContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGContactGuidFindBox, "BookedCtgMove+UNDGs+FirstItemForBinding.DI_OC_DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.UNDGs.FirstItemForBinding)).SyncRoot)).DI_OC_DGContact)));
			this.DGContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 57, true);
			this.DGContactGuidFindBox.Name = "DGContactGuidFindBox";
			this.DGContactGuidFindBox.PopupCaption = null;
			this.DGContactGuidFindBox.PreBoundMaxLength = 15;
			this.DGContactGuidFindBox.ShowDescriptionBox = false;
			this.DGContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.DGContactGuidFindBox.TabIndex = 11;
			// 
			// LooseEW_BookedLengthCalcDropEdit
			// 
			this.LooseEW_BookedLengthCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LooseEW_BookedLengthCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_BookedLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_DimUnit)));
			this.LooseEW_BookedLengthCalcDropEdit.BindToAmount = "BookedCtgMove+EW_BookedLength";
			this.LooseEW_BookedLengthCalcDropEdit.BindToUnit = "BookedCtgMove+EW_DimUnit";
			this.LooseEW_BookedLengthCalcDropEdit.Decimals = 3;
			this.LooseEW_BookedLengthCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 79, true);
			this.LooseEW_BookedLengthCalcDropEdit.Name = "LooseEW_BookedLengthCalcDropEdit";
			this.LooseEW_BookedLengthCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.LooseEW_BookedLengthCalcDropEdit.TabIndex = 3;
			this.LooseEW_BookedLengthCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// EW_BookedPackCountCalcDropEdit
			// 
			this.EW_BookedPackCountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EW_BookedPackCountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_BookedPackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_F3_NKPackType)));
			this.EW_BookedPackCountCalcDropEdit.BindToAmount = "BookedCtgMove+EW_BookedPackCount";
			this.EW_BookedPackCountCalcDropEdit.BindToUnit = "BookedCtgMove+EW_F3_NKPackType";
			this.EW_BookedPackCountCalcDropEdit.Decimals = 0;
			this.EW_BookedPackCountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 13, true);
			this.EW_BookedPackCountCalcDropEdit.Name = "EW_BookedPackCountCalcDropEdit";
			this.EW_BookedPackCountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.EW_BookedPackCountCalcDropEdit.TabIndex = 0;
			this.EW_BookedPackCountCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// LooseEW_BookedWeightCalcDropEdit
			// 
			this.LooseEW_BookedWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LooseEW_BookedWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_BookedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_WeightUQ)));
			this.LooseEW_BookedWeightCalcDropEdit.BindToAmount = "BookedCtgMove+EW_BookedWeight";
			this.LooseEW_BookedWeightCalcDropEdit.BindToUnit = "BookedCtgMove+EW_WeightUQ";
			this.LooseEW_BookedWeightCalcDropEdit.Decimals = 3;
			this.LooseEW_BookedWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 35, true);
			this.LooseEW_BookedWeightCalcDropEdit.Name = "LooseEW_BookedWeightCalcDropEdit";
			this.LooseEW_BookedWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.LooseEW_BookedWeightCalcDropEdit.TabIndex = 1;
			this.LooseEW_BookedWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// LooseEW_BookedVolumeCalcDropEdit
			// 
			this.LooseEW_BookedVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LooseEW_BookedVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_BookedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_VolumeUQ)));
			this.LooseEW_BookedVolumeCalcDropEdit.BindToAmount = "BookedCtgMove+EW_BookedVolume";
			this.LooseEW_BookedVolumeCalcDropEdit.BindToUnit = "BookedCtgMove+EW_VolumeUQ";
			this.LooseEW_BookedVolumeCalcDropEdit.Decimals = 3;
			this.LooseEW_BookedVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 57, true);
			this.LooseEW_BookedVolumeCalcDropEdit.Name = "LooseEW_BookedVolumeCalcDropEdit";
			this.LooseEW_BookedVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.LooseEW_BookedVolumeCalcDropEdit.TabIndex = 2;
			this.LooseEW_BookedVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// RefrigerationGroupBox
			// 
			this.RefrigerationGroupBox.AutoSize = true;
			this.RefrigerationGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegUserControl|3cc0d3fa-3592-4029-9884-e69c4671ac83", "Refrigeration");
			this.RefrigerationGroupBox.Controls.Add(this.tableLayoutPanel1);
			this.RefrigerationGroupBox.Controls.Add(this.TempRecorderSerialNoTextBox);
			this.RefrigerationGroupBox.Controls.Add(this.JY_AirVentFlowRateCalcDropEdit);
			this.RefrigerationGroupBox.Controls.Add(this.JY_SetPointTempCalcDropEdit);
			this.RefrigerationGroupBox.Controls.Add(this.ReeferGeneratorTextBox);
			this.RefrigerationGroupBox.Controls.Add(this.JC_HumidityPercentCalcEdit);
			this.RefrigerationGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.RefrigerationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 0, true);
			this.RefrigerationGroupBox.Name = "RefrigerationGroupBox";
			this.RefrigerationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 151, true);
			this.RefrigerationGroupBox.TabIndex = 1;
			this.RefrigerationGroupBox.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.ExportIsControlledAtmosphereCheckBox, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.ExportIsFrozenCheckBox, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.ExportIsChillerCheckBox, 1, 0);
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 13, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// ExportIsControlledAtmosphereCheckBox
			// 
			this.ExportIsControlledAtmosphereCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsControlledAtmosphereCheckBox, "BookedCtgMove+Container+JC_IsControlledAtmosphere");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_IsControlledAtmosphere)));
			this.ExportIsControlledAtmosphereCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsControlledAtmosphereCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsControlledAtmosphereCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ExportIsControlledAtmosphereCheckBox.Name = "ExportIsControlledAtmosphereCheckBox";
			this.ExportIsControlledAtmosphereCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportIsControlledAtmosphereCheckBox.TabIndex = 0;
			this.ExportIsControlledAtmosphereCheckBox.UseVisualStyleBackColor = false;
			// 
			// ExportIsFrozenCheckBox
			// 
			this.ExportIsFrozenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsFrozenCheckBox, "BookedCtgMove+Container+IsFreezer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.IsFreezer)));
			this.ExportIsFrozenCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsFrozenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsFrozenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 3, true);
			this.ExportIsFrozenCheckBox.Name = "ExportIsFrozenCheckBox";
			this.ExportIsFrozenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportIsFrozenCheckBox.TabIndex = 2;
			// 
			// ExportIsChillerCheckBox
			// 
			this.ExportIsChillerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportIsChillerCheckBox, "BookedCtgMove+Container+IsChiller");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.IsChiller)));
			this.ExportIsChillerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsChillerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsChillerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 3, true);
			this.ExportIsChillerCheckBox.Name = "ExportIsChillerCheckBox";
			this.ExportIsChillerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportIsChillerCheckBox.TabIndex = 1;
			// 
			// TempRecorderSerialNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.TempRecorderSerialNoTextBox, "BookedCtgMove+Container+JC_TempRecorderSerialNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_TempRecorderSerialNo)));
			this.TempRecorderSerialNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 80, true);
			this.TempRecorderSerialNoTextBox.Name = "TempRecorderSerialNoTextBox";
			this.TempRecorderSerialNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.TempRecorderSerialNoTextBox.TabIndex = 6;
			// 
			// JY_AirVentFlowRateCalcDropEdit
			// 
			this.JY_AirVentFlowRateCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JY_AirVentFlowRateCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_AirVentFlow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_AirVentFlowRateUnit)));
			this.JY_AirVentFlowRateCalcDropEdit.BindToAmount = "BookedCtgMove+Container+JC_AirVentFlow";
			this.JY_AirVentFlowRateCalcDropEdit.BindToUnit = "BookedCtgMove+Container+JC_AirVentFlowRateUnit";
			this.JY_AirVentFlowRateCalcDropEdit.Decimals = 0;
			this.JY_AirVentFlowRateCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 102, true);
			this.JY_AirVentFlowRateCalcDropEdit.Name = "JY_AirVentFlowRateCalcDropEdit";
			this.JY_AirVentFlowRateCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.JY_AirVentFlowRateCalcDropEdit.TabIndex = 7;
			this.JY_AirVentFlowRateCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JY_SetPointTempCalcDropEdit
			// 
			this.JY_SetPointTempCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JY_SetPointTempCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_SetPointTemp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_SetPointTempUnit)));
			this.JY_SetPointTempCalcDropEdit.BindToAmount = "BookedCtgMove+Container+JC_SetPointTemp";
			this.JY_SetPointTempCalcDropEdit.BindToUnit = "BookedCtgMove+Container+JC_SetPointTempUnit";
			this.JY_SetPointTempCalcDropEdit.Decimals = 1;
			this.JY_SetPointTempCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 36, true);
			this.JY_SetPointTempCalcDropEdit.Name = "JY_SetPointTempCalcDropEdit";
			this.JY_SetPointTempCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JY_SetPointTempCalcDropEdit.TabIndex = 4;
			this.JY_SetPointTempCalcDropEdit.UnitPreBoundMaxLength = 1;
			// 
			// ReeferGeneratorTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReeferGeneratorTextBox, "BookedCtgMove+Container+JC_RefrigGeneratorID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_RefrigGeneratorID)));
			this.ReeferGeneratorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 124, true);
			this.ReeferGeneratorTextBox.Name = "ReeferGeneratorTextBox";
			this.ReeferGeneratorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.ReeferGeneratorTextBox.TabIndex = 8;
			// 
			// JC_HumidityPercentCalcEdit
			// 
			this.JC_HumidityPercentCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JC_HumidityPercentCalcEdit, "BookedCtgMove+Container+JC_HumidityPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_HumidityPercent)));
			this.JC_HumidityPercentCalcEdit.DecimalPlaces = 0;
			this.JC_HumidityPercentCalcEdit.Decimals = 0;
			this.JC_HumidityPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 58, true);
			this.JC_HumidityPercentCalcEdit.Name = "JC_HumidityPercentCalcEdit";
			this.JC_HumidityPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.JC_HumidityPercentCalcEdit.TabIndex = 5;
			this.JC_HumidityPercentCalcEdit.Text = "0";
			this.JC_HumidityPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ContainerGroupBox
			// 
			this.ContainerGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegUserControl|c21b18d6-5d7a-4b14-a396-440511deb39a", "Container Details");
			this.ContainerGroupBox.Controls.Add(this.zDropEdit3);
			this.ContainerGroupBox.Controls.Add(this.zCalcEdit1);
			this.ContainerGroupBox.Controls.Add(this.zGuidFindBox8);
			this.ContainerGroupBox.Controls.Add(this.zTextBox6);
			this.ContainerGroupBox.Controls.Add(this.ContainerTextBox);
			this.ContainerGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ContainerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerGroupBox.Name = "ContainerGroupBox";
			this.ContainerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 151, true);
			this.ContainerGroupBox.TabIndex = 0;
			this.ContainerGroupBox.TabStop = false;
			// 
			// zDropEdit3
			// 
			this.zDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit3, "BookedCtgMove+Container+JC_GrossWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_GrossWeightUQ)));
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.PreBoundMaxLength = 3;
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zDropEdit3.TabIndex = 4;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "BookedCtgMove+Container+JC_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_GrossWeight)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 81, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zCalcEdit1.TabIndex = 3;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGuidFindBox8
			// 
			this.zGuidFindBox8.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox8, "BookedCtgMove+Container+JC_RC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_RC)));
			this.zGuidFindBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 36, true);
			this.zGuidFindBox8.Name = "zGuidFindBox8";
			this.zGuidFindBox8.PopupCaption = null;
			this.zGuidFindBox8.PreBoundMaxLength = 4;
			this.zGuidFindBox8.ShowDescriptionBox = false;
			this.zGuidFindBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.zGuidFindBox8.TabIndex = 1;
			// 
			// zTextBox6
			// 
			this.BindingSource.SetBindingMember(this.zTextBox6, "BookedCtgMove+Container+JC_SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_SealNum)));
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 58, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.zTextBox6.TabIndex = 2;
			// 
			// ContainerTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContainerTextBox, "BookedCtgMove+Container+JC_ContainerNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_ContainerNum)));
			this.ContainerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 14, true);
			this.ContainerTextBox.Name = "ContainerTextBox";
			this.ContainerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.ContainerTextBox.TabIndex = 0;
			// 
			// CartageLegUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.zGroupBox2);
			this.Name = "CartageLegUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 523, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox2.ResumeLayout(false);
			this.zPanel2.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.CustomFieldsGroupBox.ResumeLayout(false);
			this.LooseDetailsGroupBox.ResumeLayout(false);
			this.LooseDetailsGroupBox.PerformLayout();
			this.RefrigerationGroupBox.ResumeLayout(false);
			this.RefrigerationGroupBox.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ContainerGroupBox.ResumeLayout(false);
			this.ContainerGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
