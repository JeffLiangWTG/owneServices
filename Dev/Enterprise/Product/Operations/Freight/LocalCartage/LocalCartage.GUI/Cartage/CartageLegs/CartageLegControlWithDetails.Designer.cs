namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageLegControlWithDetails
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
			this.RunSheetDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CartageLegDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CartageLegPanel = new Enterprise.Freight.LocalCartage.GUI.CartageLegControl();
			this.BookedMovementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CartageBookedMovePanel = new Enterprise.Freight.LocalCartage.GUI.BookedMoveControl();
			this.ContainerOrLooseDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
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
			this.GPSMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.gpsMessagesControl1 = new Enterprise.Freight.LocalCartage.GUI.CartageLeg.GPS.GPSMessagesControl();
			this.PacksDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RunSheetDetailsTabControl.SuspendLayout();
			this.CartageLegDetailsTabPage.SuspendLayout();
			this.BookedMovementTabPage.SuspendLayout();
			this.ContainerOrLooseDetailsTabPage.SuspendLayout();
			this.LooseDetailsGroupBox.SuspendLayout();
			this.RefrigerationGroupBox.SuspendLayout();
			this.ContainerGroupBox.SuspendLayout();
			this.GPSMessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartageLeg);
			// 
			// RunSheetDetailsTabControl
			// 
			this.RunSheetDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.RunSheetDetailsTabControl.Controls.Add(this.CartageLegDetailsTabPage);
			this.RunSheetDetailsTabControl.Controls.Add(this.BookedMovementTabPage);
			this.RunSheetDetailsTabControl.Controls.Add(this.ContainerOrLooseDetailsTabPage);
			this.RunSheetDetailsTabControl.Controls.Add(this.GPSMessagesTabPage);
			this.RunSheetDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RunSheetDetailsTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 199, true);
			this.RunSheetDetailsTabControl.Name = "RunSheetDetailsTabControl";
			this.RunSheetDetailsTabControl.SelectedIndex = 0;
			this.RunSheetDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 199, true);
			this.RunSheetDetailsTabControl.TabIndex = 94;
			// 
			// CartageLegDetailsTabPage
			// 
			this.CartageLegDetailsTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControlWithDetails|c3fb29c4-5e72-425a-b7c3-1b049b535feb", "Leg Details", "Port Transport Leg Details.");
			this.CartageLegDetailsTabPage.Controls.Add(this.CartageLegPanel);
			this.CartageLegDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CartageLegDetailsTabPage.Name = "CartageLegDetailsTabPage";
			this.CartageLegDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.CartageLegDetailsTabPage.TabIndex = 0;
			// 
			// CartageLegPanel
			// 
			this.BindingSource.SetBindingMember(this.CartageLegPanel, ".");
			this.CartageLegPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CartageLegPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.CartageLegPanel.Name = "CartageLegPanel";
			this.CartageLegPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.CartageLegPanel.TabIndex = 0;
			// 
			// BookedMovementTabPage
			// 
			this.BookedMovementTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControlWithDetails|1f5ee807-0192-4ca9-8243-82f0fdc4bc7c", "Booked Movement");
			this.BookedMovementTabPage.Controls.Add(this.CartageBookedMovePanel);
			this.BookedMovementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BookedMovementTabPage.Name = "BookedMovementTabPage";
			this.BookedMovementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.BookedMovementTabPage.TabIndex = 6;
			// 
			// CartageBookedMovePanel
			// 
			this.BindingSource.SetBindingMember(this.CartageBookedMovePanel, "BookedCtgMove");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove)));
			this.CartageBookedMovePanel.BookedMoveLayout = Enterprise.Freight.LocalCartage.GUI.BookedMoveControl.BookedMovesLayout.CartageLeg;
			this.CartageBookedMovePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CartageBookedMovePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CartageBookedMovePanel.Name = "CartageBookedMovePanel";
			this.CartageBookedMovePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.CartageBookedMovePanel.TabIndex = 0;
			// 
			// ContainerOrLooseDetailsTabPage
			// 
			this.ContainerOrLooseDetailsTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControlWithDetails|60cb9a38-97e9-45cb-91fc-49b63e6d487f", "Details", "Container Details", "");
			this.ContainerOrLooseDetailsTabPage.Controls.Add(this.LooseDetailsGroupBox);
			this.ContainerOrLooseDetailsTabPage.Controls.Add(this.RefrigerationGroupBox);
			this.ContainerOrLooseDetailsTabPage.Controls.Add(this.ContainerGroupBox);
			this.ContainerOrLooseDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerOrLooseDetailsTabPage.Name = "ContainerOrLooseDetailsTabPage";
			this.ContainerOrLooseDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.ContainerOrLooseDetailsTabPage.TabIndex = 5;
			// 
			// LooseDetailsGroupBox
			// 
			this.LooseDetailsGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControlWithDetails|78d53505-2402-4e78-a6cb-0315964401a1", "Loose");
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
			this.LooseDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LooseDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 0, true);
			this.LooseDetailsGroupBox.Name = "LooseDetailsGroupBox";
			this.LooseDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 172, true);
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
			this.BindingSource.SetBindingMember(this.DGSubstanceGuidFindBox, "BookedCtgMove+UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.DGSubstanceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 13, true);
			this.DGSubstanceGuidFindBox.Name = "DGSubstanceGuidFindBox";
			this.DGSubstanceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.DGSubstanceGuidFindBox.TabIndex = 8;
			// 
			// zLabel9
			// 
			this.BindingSource.SetBindingMember(this.zLabel9, "BookedCtgMove+EW_DimUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_DimUnit)));
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
			this.FlashPointCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 35, true);
			this.FlashPointCalcEdit.Name = "FlashPointCalcEdit";
			this.FlashPointCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.FlashPointCalcEdit.TabIndex = 10;
			this.FlashPointCalcEdit.Text = "0.0";
			this.FlashPointCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DGContactGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.DGContactGuidFindBox, "BookedCtgMove+UNDGs+FirstItemForBinding.DI_OC_DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.UNDGs.FirstItemForBinding)).SyncRoot)).DI_OC_DGContact)));
			this.DGContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 57, true);
			this.DGContactGuidFindBox.Name = "DGContactGuidFindBox";
			this.DGContactGuidFindBox.PopupCaption = null;
			this.DGContactGuidFindBox.PreBoundMaxLength = 15;
			this.DGContactGuidFindBox.ShowDescriptionBox = false;
			this.DGContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.DGContactGuidFindBox.TabIndex = 11;
			// 
			// LooseEW_BookedLengthCalcDropEdit
			// 
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
			this.RefrigerationGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControlWithDetails|3cc0d3fa-3592-4029-9884-e69c4671ac83", "Refrigeration");
			this.RefrigerationGroupBox.Controls.Add(this.ExportIsControlledAtmosphereCheckBox);
			this.RefrigerationGroupBox.Controls.Add(this.ExportIsFrozenCheckBox);
			this.RefrigerationGroupBox.Controls.Add(this.ExportIsChillerCheckBox);
			this.RefrigerationGroupBox.Controls.Add(this.TempRecorderSerialNoTextBox);
			this.RefrigerationGroupBox.Controls.Add(this.JY_AirVentFlowRateCalcDropEdit);
			this.RefrigerationGroupBox.Controls.Add(this.JY_SetPointTempCalcDropEdit);
			this.RefrigerationGroupBox.Controls.Add(this.ReeferGeneratorTextBox);
			this.RefrigerationGroupBox.Controls.Add(this.JC_HumidityPercentCalcEdit);
			this.RefrigerationGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.RefrigerationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 0, true);
			this.RefrigerationGroupBox.Name = "RefrigerationGroupBox";
			this.RefrigerationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 172, true);
			this.RefrigerationGroupBox.TabIndex = 1;
			this.RefrigerationGroupBox.TabStop = false;
			// 
			// ExportIsControlledAtmosphereCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExportIsControlledAtmosphereCheckBox, "BookedCtgMove+Container+JC_IsControlledAtmosphere");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_IsControlledAtmosphere)));
			this.ExportIsControlledAtmosphereCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsControlledAtmosphereCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsControlledAtmosphereCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.ExportIsControlledAtmosphereCheckBox.Name = "ExportIsControlledAtmosphereCheckBox";
			this.ExportIsControlledAtmosphereCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 18, true);
			this.ExportIsControlledAtmosphereCheckBox.TabIndex = 0;
			this.ExportIsControlledAtmosphereCheckBox.UseVisualStyleBackColor = false;
			// 
			// ExportIsFrozenCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExportIsFrozenCheckBox, "BookedCtgMove+Container+IsFreezer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.IsFreezer)));
			this.ExportIsFrozenCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsFrozenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsFrozenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 16, true);
			this.ExportIsFrozenCheckBox.Name = "ExportIsFrozenCheckBox";
			this.ExportIsFrozenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 18, true);
			this.ExportIsFrozenCheckBox.TabIndex = 2;
			// 
			// ExportIsChillerCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExportIsChillerCheckBox, "BookedCtgMove+Container+IsChiller");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.IsChiller)));
			this.ExportIsChillerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsChillerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsChillerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 16, true);
			this.ExportIsChillerCheckBox.Name = "ExportIsChillerCheckBox";
			this.ExportIsChillerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 18, true);
			this.ExportIsChillerCheckBox.TabIndex = 1;
			// 
			// TempRecorderSerialNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.TempRecorderSerialNoTextBox, "BookedCtgMove+Container+JC_TempRecorderSerialNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_TempRecorderSerialNo)));
			this.TempRecorderSerialNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 80, true);
			this.TempRecorderSerialNoTextBox.Name = "TempRecorderSerialNoTextBox";
			this.TempRecorderSerialNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.TempRecorderSerialNoTextBox.TabIndex = 5;
			// 
			// JY_AirVentFlowRateCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JY_AirVentFlowRateCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_AirVentFlow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_AirVentFlowRateUnit)));
			this.JY_AirVentFlowRateCalcDropEdit.BindToAmount = "BookedCtgMove+Container+JC_AirVentFlow";
			this.JY_AirVentFlowRateCalcDropEdit.BindToUnit = "BookedCtgMove+Container+JC_AirVentFlowRateUnit";
			this.JY_AirVentFlowRateCalcDropEdit.Decimals = 0;
			this.JY_AirVentFlowRateCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 102, true);
			this.JY_AirVentFlowRateCalcDropEdit.Name = "JY_AirVentFlowRateCalcDropEdit";
			this.JY_AirVentFlowRateCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.JY_AirVentFlowRateCalcDropEdit.TabIndex = 6;
			this.JY_AirVentFlowRateCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JY_SetPointTempCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JY_SetPointTempCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_SetPointTemp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_SetPointTempUnit)));
			this.JY_SetPointTempCalcDropEdit.BindToAmount = "BookedCtgMove+Container+JC_SetPointTemp";
			this.JY_SetPointTempCalcDropEdit.BindToUnit = "BookedCtgMove+Container+JC_SetPointTempUnit";
			this.JY_SetPointTempCalcDropEdit.Decimals = 1;
			this.JY_SetPointTempCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 36, true);
			this.JY_SetPointTempCalcDropEdit.Name = "JY_SetPointTempCalcDropEdit";
			this.JY_SetPointTempCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JY_SetPointTempCalcDropEdit.TabIndex = 3;
			this.JY_SetPointTempCalcDropEdit.UnitPreBoundMaxLength = 1;
			// 
			// ReeferGeneratorTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReeferGeneratorTextBox, "BookedCtgMove+Container+JC_RefrigGeneratorID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_RefrigGeneratorID)));
			this.ReeferGeneratorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 124, true);
			this.ReeferGeneratorTextBox.Name = "ReeferGeneratorTextBox";
			this.ReeferGeneratorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.ReeferGeneratorTextBox.TabIndex = 7;
			// 
			// JC_HumidityPercentCalcEdit
			// 
			this.JC_HumidityPercentCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JC_HumidityPercentCalcEdit, "BookedCtgMove+Container+JC_HumidityPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_HumidityPercent)));
			this.JC_HumidityPercentCalcEdit.DecimalPlaces = 0;
			this.JC_HumidityPercentCalcEdit.Decimals = 0;
			this.JC_HumidityPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 58, true);
			this.JC_HumidityPercentCalcEdit.Name = "JC_HumidityPercentCalcEdit";
			this.JC_HumidityPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.JC_HumidityPercentCalcEdit.TabIndex = 4;
			this.JC_HumidityPercentCalcEdit.Text = "0";
			this.JC_HumidityPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ContainerGroupBox
			// 
			this.ContainerGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControlWithDetails|c21b18d6-5d7a-4b14-a396-440511deb39a", "Container");
			this.ContainerGroupBox.Controls.Add(this.PacksDropEdit);
			this.ContainerGroupBox.Controls.Add(this.zDropEdit3);
			this.ContainerGroupBox.Controls.Add(this.zCalcEdit1);
			this.ContainerGroupBox.Controls.Add(this.zGuidFindBox8);
			this.ContainerGroupBox.Controls.Add(this.zTextBox6);
			this.ContainerGroupBox.Controls.Add(this.ContainerTextBox);
			this.ContainerGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ContainerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerGroupBox.Name = "ContainerGroupBox";
			this.ContainerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 172, true);
			this.ContainerGroupBox.TabIndex = 0;
			this.ContainerGroupBox.TabStop = false;
			// 
			// zDropEdit3
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit3, "BookedCtgMove+Container+JC_GrossWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_GrossWeightUQ)));
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 125, true);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.PreBoundMaxLength = 3;
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zDropEdit3.TabIndex = 5;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "BookedCtgMove+Container+JC_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.Container.JC_GrossWeight)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 102, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.zCalcEdit1.TabIndex = 4;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGuidFindBox8
			// 
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
			this.ContainerTextBox.ReadOnly = true;
			this.ContainerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.ContainerTextBox.TabIndex = 0;
			// 
			// GPSMessagesTabPage
			// 
			this.GPSMessagesTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CartageLegControlWithDetails|23c19240-be24-44a9-a103-7cd0f4522bde", "GPS Messages");
			this.GPSMessagesTabPage.Controls.Add(this.gpsMessagesControl1);
			this.GPSMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GPSMessagesTabPage.Name = "GPSMessagesTabPage";
			this.GPSMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.GPSMessagesTabPage.TabIndex = 8;
			// 
			// gpsMessagesControl1
			// 
			this.BindingSource.SetBindingMember(this.gpsMessagesControl1, ".");
			this.gpsMessagesControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gpsMessagesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gpsMessagesControl1.Name = "gpsMessagesControl1";
			this.gpsMessagesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 172, true);
			this.gpsMessagesControl1.TabIndex = 0;
			// 
			// PacksDropEdit
			// 
			this.PacksDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PacksDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_BookedPackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).BookedCtgMove.EW_F3_NKPackType)));
			this.PacksDropEdit.BindToAmount = "BookedCtgMove+EW_BookedPackCount";
			this.PacksDropEdit.BindToUnit = "BookedCtgMove+EW_F3_NKPackType";
			this.PacksDropEdit.Decimals = 2;
			this.PacksDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 80, true);
			this.PacksDropEdit.Name = "PacksDropEdit";
			this.PacksDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.PacksDropEdit.TabIndex = 3;
			this.PacksDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CartageLegControlWithDetails
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RunSheetDetailsTabControl);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 200, true);
			this.Name = "CartageLegControlWithDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RunSheetDetailsTabControl.ResumeLayout(false);
			this.CartageLegDetailsTabPage.ResumeLayout(false);
			this.BookedMovementTabPage.ResumeLayout(false);
			this.ContainerOrLooseDetailsTabPage.ResumeLayout(false);
			this.LooseDetailsGroupBox.ResumeLayout(false);
			this.LooseDetailsGroupBox.PerformLayout();
			this.RefrigerationGroupBox.ResumeLayout(false);
			this.RefrigerationGroupBox.PerformLayout();
			this.ContainerGroupBox.ResumeLayout(false);
			this.ContainerGroupBox.PerformLayout();
			this.GPSMessagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZTabControl RunSheetDetailsTabControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage CartageLegDetailsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage BookedMovementTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage ContainerOrLooseDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LooseDetailsGroupBox;
		private Enterprise.ZArchitecture.ZLabel zLabel9;
		private Enterprise.ZArchitecture.ZLabel zLabel12;
		private Enterprise.ZArchitecture.ZCalcEdit LooseEW_BookedHeightCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit LooseEW_BookedWidthCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit FlashPointCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox DGContactGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit LooseEW_BookedLengthCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit EW_BookedPackCountCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit LooseEW_BookedWeightCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit LooseEW_BookedVolumeCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RefrigerationGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ExportIsControlledAtmosphereCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ExportIsFrozenCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ExportIsChillerCheckBox;
		private Enterprise.ZArchitecture.ZTextBox TempRecorderSerialNoTextBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JY_AirVentFlowRateCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JY_SetPointTempCalcDropEdit;
		private Enterprise.ZArchitecture.ZTextBox ReeferGeneratorTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit JC_HumidityPercentCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ContainerGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit3;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox8;
		private Enterprise.ZArchitecture.ZTextBox zTextBox6;
		private Enterprise.ZArchitecture.ZTextBox ContainerTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage GPSMessagesTabPage;
		private Enterprise.Freight.LocalCartage.GUI.CartageLeg.GPS.GPSMessagesControl gpsMessagesControl1;
		internal CartageLegControl CartageLegPanel;
		private BookedMoveControl CartageBookedMovePanel;
		internal Enterprise.ZArchitecture.GUI.ZLinkLabel DGLinkLabel;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox DGSubstanceGuidFindBox;
		private ZArchitecture.GUI.ZCalcDropEdit PacksDropEdit;
	}
}
