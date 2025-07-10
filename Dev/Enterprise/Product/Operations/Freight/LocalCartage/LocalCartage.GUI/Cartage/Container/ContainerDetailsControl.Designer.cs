using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class ContainerDetailsControl
	{
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		ZTabControl ContainerTabControl;
		ZTabPage ContainerMovementsTabPage;
		ZTabPage ContainerServicesTabPage;
		ZTabPage ContainerDetailsTabPage;
		ZGroupBox RefrigerationGroupBox;
		ZCheckBox ExportIsControlledAtmosphereCheckBox;
		ZCheckBox ExportIsFrozenCheckBox;
		ZCheckBox ExportIsChillerCheckBox;
		ZTextBox TempRecorderSerialNoTextBox;
		ZCalcDropEdit JY_SetPointTempCalcDropEdit;
		ZTextBox ReeferGeneratorTextBox;
		ZCalcEdit JC_HumidityPercentCalcEdit;
		ZGroupBox ContainerGroupBox;
		ZGuidFindBox zGuidFindBox8;
		ZTextBox sealTextBox;
		ZTextBox ContainerTextBox;
		internal ContainerMovesControl ContainerMovesControl;
		ZCalcEdit GrossWeightCalcEdit;
		ZDropEdit zDropEdit1;
		ZCalcDropEdit zCalcDropEdit2;
		MasterFiles.GUI.ServicesControl servicesControl1;
		internal ZGrid ContainersGrid;
		ZTextBox secondSealTextBox;
		ZDropEdit sealedByZDropEdit;
		ZDropEdit secondSealedByZDropEdit;
		ZTextBox thirdSealTextBox;
		ZDropEdit thirdSealedByZDropEdit;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoThirdSealNum = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoThirdSealParty = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainerTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainerMovementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerMovesControl = new Enterprise.Freight.LocalCartage.GUI.ContainerMovesControl();
			this.ContainerDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RefrigerationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExportIsControlledAtmosphereCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportIsFrozenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportIsChillerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TempRecorderSerialNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JY_SetPointTempCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ReeferGeneratorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JC_HumidityPercentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContainerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCalcDropEdit2 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zGuidFindBox8 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.sealTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerServicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.servicesControl1 = new Enterprise.MasterFiles.GUI.ServicesControl();
			this.sealedByZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.secondSealTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.secondSealedByZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.thirdSealTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.thirdSealedByZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainerTabControl.SuspendLayout();
			this.ContainerMovementsTabPage.SuspendLayout();
			this.ContainerDetailsTabPage.SuspendLayout();
			this.RefrigerationGroupBox.SuspendLayout();
			this.ContainerGroupBox.SuspendLayout();
			this.ContainerServicesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartage);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 548, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ContainersGrid);
			this.splitContainer1.Panel1MinSize = 70;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ContainerTabControl);
			this.splitContainer1.Panel2MinSize = 450;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 548, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			this.splitContainer1.TabIndex = 4;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "ContainerBookedMoves");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_RC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).ContainerModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ArrivalSlotDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ArrivalSlotReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_DepartureSlotDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_DepartureSlotReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ReleaseNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_TareWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_Calc_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_GrossWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_EmptyReturnedBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerYardEmptyReturnGateIn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_EmptyRequired)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_TotalLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_TotalWidth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_TotalHeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_DepartureEstimatedPickup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_LCLUnpack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_LCLAvailable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_LCLStorageCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_AdditionalSealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_Additional2SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_PackDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerStorageLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerQuality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerRating)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ExportDepotCustomsReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_TrainWagonNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_TempRecorderSerialNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).EW_BookedPackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).EW_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_SealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_AdditionalSealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_Additional2SealParty)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Container+JC_ContainerNum";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Container+JC_RC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "Container+JC_SealNum";
			zDropEditColumnStyleInfo1.BindToList = "ContainerModes";
			zDropEditColumnStyleInfo1.ColumnName = "Container+JC_ContainerMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|2e321a3a-9d96-46f6-bba0-2a408f9e255f", "Arv. Slot Date");
			zDateEditColumnStyleInfo1.ColumnName = "Container+JC_ArrivalSlotDateTime";
			zDateEditColumnStyleInfo1.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|ab776354-95b1-4987-802a-b9a18505ea10", "Arrival Slot");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|f0091224-1e24-42da-adfd-91940e18c049", "Arv. Slot Ref");
			zTextBoxColumnStyleInfo3.ColumnName = "Container+JC_ArrivalSlotReference";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|ab776354-95b1-4987-802a-b9a18505ea10", "Arrival Slot");
			zDateEditColumnStyleInfo2.ColumnName = "Container+JC_DepartureSlotDateTime";
			zDateEditColumnStyleInfo2.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|f5c339bc-677f-4d4c-9329-6ec4465cde3c", "Departure Slot");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|127fdf9e-ebf6-404b-8b8a-9514324d8d8a", "Dep. Slot Ref");
			zTextBoxColumnStyleInfo4.ColumnName = "Container+JC_DepartureSlotReference";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|f5c339bc-677f-4d4c-9329-6ec4465cde3c", "Departure Slot");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "Container+JC_ReleaseNum";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Container+JC_TareWeight";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Container+JC_Calc_NetWeight";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Container+JC_GrossWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|87ffa1fb-7224-4aa6-aae7-1469a889d6aa", "Gross Weight");
			zDropEditColumnStyleInfo2.ColumnName = "Container+JC_GrossWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|87ffa1fb-7224-4aa6-aae7-1469a889d6aa", "Gross Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo3.ColumnName = "Container+JC_EmptyReturnedBy";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo4.ColumnName = "Container+JC_ContainerYardEmptyReturnGateIn";
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo5.ColumnName = "Container+JC_EmptyRequired";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "Container+JC_TotalLength";
			zCalcEditColumnStyleInfo4.Decimals = 3;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "Container+JC_TotalWidth";
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "Container+JC_TotalHeight";
			zCalcEditColumnStyleInfo6.Decimals = 3;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo6.ColumnName = "Container+JC_DepartureEstimatedPickup";
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo7.ColumnName = "Container+JC_LCLUnpack";
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo8.ColumnName = "Container+JC_LCLAvailable";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo9.ColumnName = "Container+JC_LCLStorageCommences";
			zDateEditColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "Container+JC_AdditionalSealNum";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfoThirdSealNum.ColumnName = "Container+JC_Additional2SealNum";
			zTextBoxColumnStyleInfoThirdSealNum.IsVisible = false;
			zDateEditColumnStyleInfo10.ColumnName = "Container+JC_PackDate";
			zDateEditColumnStyleInfo10.IsReadOnly = true;
			zDateEditColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "Container+JC_ContainerStorageLocation";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "Container+JC_ContainerStatus";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "Container+JC_ContainerQuality";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "Container+JC_ContainerRating";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.ColumnName = "Container+JC_ExportDepotCustomsReference";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.ColumnName = "Container+JC_TrainWagonNumber";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "Container+JC_TempRecorderSerialNo";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "EW_BookedPackCount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo5.ColumnName = "EW_F3_NKPackType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo6.ColumnName = "Container+JC_SealParty";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo7.ColumnName = "Container+JC_AdditionalSealParty";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfoThirdSealParty.ColumnName = "Container+JC_Additional2SealParty";
			zDropEditColumnStyleInfoThirdSealParty.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfoThirdSealParty.IsVisible = false;
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoThirdSealNum);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfoThirdSealParty);
			this.ContainersGrid.GridId = "490fb5f0-fae5-49bc-b06c-908ef86fd5ef";
			this.ContainersGrid.CopySelectedRowsAllowed = true;
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 87, true);
			this.ContainersGrid.TabIndex = 3;
			// 
			// ContainerTabControl
			// 
			this.ContainerTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ContainerTabControl.Controls.Add(this.ContainerMovementsTabPage);
			this.ContainerTabControl.Controls.Add(this.ContainerDetailsTabPage);
			this.ContainerTabControl.Controls.Add(this.ContainerServicesTabPage);
			this.ContainerTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerTabControl.Name = "ContainerTabControl";
			this.ContainerTabControl.SelectedIndex = 0;
			this.ContainerTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 457, true);
			this.ContainerTabControl.TabIndex = 0;
			// 
			// ContainerMovementsTabPage
			// 
			this.ContainerMovementsTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|6f2c1f09-59f7-4967-923c-07474939a1a5", "Movements");
			this.ContainerMovementsTabPage.Controls.Add(this.ContainerMovesControl);
			this.ContainerMovementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerMovementsTabPage.Name = "ContainerMovementsTabPage";
			this.ContainerMovementsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainerMovementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 430, true);
			this.ContainerMovementsTabPage.TabIndex = 0;
			// 
			// ContainerMovesControl
			// 
			this.ContainerMovesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerMovesControl, "ContainerBookedMoves");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMoveCollection)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)));
			this.ContainerMovesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerMovesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContainerMovesControl.Name = "ContainerMovesControl";
			this.ContainerMovesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 424, true);
			this.ContainerMovesControl.TabIndex = 0;
			// 
			// ContainerDetailsTabPage
			// 
			this.ContainerDetailsTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|3fc6c752-e94b-486b-999f-ce5ec3e4c8a4", "Container Details");
			this.ContainerDetailsTabPage.Controls.Add(this.RefrigerationGroupBox);
			this.ContainerDetailsTabPage.Controls.Add(this.ContainerGroupBox);
			this.ContainerDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerDetailsTabPage.Name = "ContainerDetailsTabPage";
			this.ContainerDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainerDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 430, true);
			this.ContainerDetailsTabPage.TabIndex = 2;
			// 
			// RefrigerationGroupBox
			// 
			this.RefrigerationGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|6ab221fa-9139-4683-bca7-9e55a91ff677", "Refrigeration");
			this.RefrigerationGroupBox.Controls.Add(this.ExportIsControlledAtmosphereCheckBox);
			this.RefrigerationGroupBox.Controls.Add(this.ExportIsFrozenCheckBox);
			this.RefrigerationGroupBox.Controls.Add(this.ExportIsChillerCheckBox);
			this.RefrigerationGroupBox.Controls.Add(this.TempRecorderSerialNoTextBox);
			this.RefrigerationGroupBox.Controls.Add(this.JY_SetPointTempCalcDropEdit);
			this.RefrigerationGroupBox.Controls.Add(this.ReeferGeneratorTextBox);
			this.RefrigerationGroupBox.Controls.Add(this.JC_HumidityPercentCalcEdit);
			this.RefrigerationGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.RefrigerationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 3, true);
			this.RefrigerationGroupBox.Name = "RefrigerationGroupBox";
			this.RefrigerationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 424, true);
			this.RefrigerationGroupBox.TabIndex = 1;
			this.RefrigerationGroupBox.TabStop = false;
			// 
			// ExportIsControlledAtmosphereCheckBox
			// 
			this.ExportIsControlledAtmosphereCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ExportIsControlledAtmosphereCheckBox, "ContainerBookedMoves.Container+JC_IsControlledAtmosphere");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_IsControlledAtmosphere)));
			this.ExportIsControlledAtmosphereCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsControlledAtmosphereCheckBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|e4d45409-e4d6-445b-aa92-35651a866ff6", "Temp. Controlled");
			this.ExportIsControlledAtmosphereCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsControlledAtmosphereCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.ExportIsControlledAtmosphereCheckBox.Name = "ExportIsControlledAtmosphereCheckBox";
			this.ExportIsControlledAtmosphereCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 18, true);
			this.ExportIsControlledAtmosphereCheckBox.TabIndex = 0;
			this.ExportIsControlledAtmosphereCheckBox.UseVisualStyleBackColor = false;
			// 
			// ExportIsFrozenCheckBox
			// 
			this.ExportIsFrozenCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ExportIsFrozenCheckBox, "ContainerBookedMoves.Container+IsFreezer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.IsFreezer)));
			this.ExportIsFrozenCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsFrozenCheckBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|4e2b3dbe-72c8-425d-963d-f12adffcf103", "Frozen");
			this.ExportIsFrozenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsFrozenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 16, true);
			this.ExportIsFrozenCheckBox.Name = "ExportIsFrozenCheckBox";
			this.ExportIsFrozenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 18, true);
			this.ExportIsFrozenCheckBox.TabIndex = 2;
			this.ExportIsFrozenCheckBox.UseVisualStyleBackColor = false;
			// 
			// ExportIsChillerCheckBox
			// 
			this.ExportIsChillerCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.ExportIsChillerCheckBox, "ContainerBookedMoves.Container+IsChiller");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.IsChiller)));
			this.ExportIsChillerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExportIsChillerCheckBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|bb28f35a-b4ab-4003-891a-4e25e49774e7", "Chiller");
			this.ExportIsChillerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportIsChillerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 16, true);
			this.ExportIsChillerCheckBox.Name = "ExportIsChillerCheckBox";
			this.ExportIsChillerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 18, true);
			this.ExportIsChillerCheckBox.TabIndex = 1;
			this.ExportIsChillerCheckBox.UseVisualStyleBackColor = false;
			// 
			// TempRecorderSerialNoTextBox
			// 
			this.TempRecorderSerialNoTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TempRecorderSerialNoTextBox, "ContainerBookedMoves.Container+JC_TempRecorderSerialNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_TempRecorderSerialNo)));
			this.TempRecorderSerialNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 80, true);
			this.TempRecorderSerialNoTextBox.Name = "TempRecorderSerialNoTextBox";
			this.TempRecorderSerialNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.TempRecorderSerialNoTextBox.TabIndex = 5;
			// 
			// JY_SetPointTempCalcDropEdit
			// 
			this.JY_SetPointTempCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JY_SetPointTempCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_SetPointTemp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_SetPointTempUnit)));
			this.JY_SetPointTempCalcDropEdit.BindToAmount = "ContainerBookedMoves.Container+JC_SetPointTemp";
			this.JY_SetPointTempCalcDropEdit.BindToUnit = "ContainerBookedMoves.Container+JC_SetPointTempUnit";
			this.JY_SetPointTempCalcDropEdit.Decimals = 1;
			this.JY_SetPointTempCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 36, true);
			this.JY_SetPointTempCalcDropEdit.Name = "JY_SetPointTempCalcDropEdit";
			this.JY_SetPointTempCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JY_SetPointTempCalcDropEdit.TabIndex = 3;
			this.JY_SetPointTempCalcDropEdit.UnitPreBoundMaxLength = 1;
			// 
			// ReeferGeneratorTextBox
			// 
			this.ReeferGeneratorTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReeferGeneratorTextBox, "ContainerBookedMoves.Container+JC_RefrigGeneratorID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_RefrigGeneratorID)));
			this.ReeferGeneratorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 103, true);
			this.ReeferGeneratorTextBox.Name = "ReeferGeneratorTextBox";
			this.ReeferGeneratorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.ReeferGeneratorTextBox.TabIndex = 7;
			// 
			// JC_HumidityPercentCalcEdit
			// 
			this.JC_HumidityPercentCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JC_HumidityPercentCalcEdit, "ContainerBookedMoves.Container+JC_HumidityPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_HumidityPercent)));
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
			this.ContainerGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|2971cad0-0fd6-45dd-a701-4bffe5f71c06", "Container");
			this.ContainerGroupBox.Controls.Add(this.sealedByZDropEdit);
			this.ContainerGroupBox.Controls.Add(this.secondSealedByZDropEdit);
			this.ContainerGroupBox.Controls.Add(this.thirdSealedByZDropEdit);
			this.ContainerGroupBox.Controls.Add(this.zCalcDropEdit2);
			this.ContainerGroupBox.Controls.Add(this.zDropEdit1);
			this.ContainerGroupBox.Controls.Add(this.GrossWeightCalcEdit);
			this.ContainerGroupBox.Controls.Add(this.zGuidFindBox8);
			this.ContainerGroupBox.Controls.Add(this.sealTextBox);
			this.ContainerGroupBox.Controls.Add(this.secondSealTextBox);
			this.ContainerGroupBox.Controls.Add(this.thirdSealTextBox);
			this.ContainerGroupBox.Controls.Add(this.ContainerTextBox);
			this.ContainerGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ContainerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContainerGroupBox.Name = "ContainerGroupBox";
			this.ContainerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 424, true);
			this.ContainerGroupBox.TabIndex = 0;
			this.ContainerGroupBox.TabStop = false;
			// 
			// zCalcDropEdit2
			// 
			this.zCalcDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).EW_BookedPackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).EW_F3_NKPackType)));
			this.zCalcDropEdit2.BindToAmount = "ContainerBookedMoves.EW_BookedPackCount";
			this.zCalcDropEdit2.BindToUnit = "ContainerBookedMoves.EW_F3_NKPackType";
			this.zCalcDropEdit2.Decimals = 2;
			this.zCalcDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 190, true);
			this.zCalcDropEdit2.Name = "zCalcDropEdit2";
			this.zCalcDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zCalcDropEdit2.TabIndex = 8;
			this.zCalcDropEdit2.UnitPreBoundMaxLength = 3;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "ContainerBookedMoves.Container+JC_GrossWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_GrossWeightUQ)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 234, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.zDropEdit1.TabIndex = 10;
			// 
			// GrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit, "ContainerBookedMoves.Container+JC_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_GrossWeight)));
			this.GrossWeightCalcEdit.DecimalPlaces = 2;
			this.GrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 212, true);
			this.GrossWeightCalcEdit.Name = "GrossWeightCalcEdit";
			this.GrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.GrossWeightCalcEdit.TabIndex = 9;
			this.GrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGuidFindBox8
			// 
			this.zGuidFindBox8.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox8, "ContainerBookedMoves.Container+JC_RC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_RC)));
			this.zGuidFindBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 36, true);
			this.zGuidFindBox8.Name = "zGuidFindBox8";
			this.zGuidFindBox8.PopupCaption = null;
			this.zGuidFindBox8.PreBoundMaxLength = 4;
			this.zGuidFindBox8.ShowDescriptionBox = false;
			this.zGuidFindBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.zGuidFindBox8.TabIndex = 1;
			// 
			// sealTextBox
			// 
			this.sealTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.sealTextBox, "ContainerBookedMoves.Container+JC_SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_SealNum)));
			this.sealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 58, true);
			this.sealTextBox.Name = "sealTextBox";
			this.sealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.sealTextBox.TabIndex = 2;
			// 
			// secondSealTextBox
			// 
			this.secondSealTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.secondSealTextBox, "ContainerBookedMoves.Container+JC_AdditionalSealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_AdditionalSealNum)));
			this.secondSealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 102, true);
			this.secondSealTextBox.Name = "secondSealTextBox";
			this.secondSealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.secondSealTextBox.TabIndex = 4;
			//
			// thirdSealTextBox
			//
			this.thirdSealTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.thirdSealTextBox, "ContainerBookedMoves.Container+JC_Additional2SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_Additional2SealNum)));
			this.thirdSealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 146, true);
			this.thirdSealTextBox.Name = "thirdSealTextBox";
			this.thirdSealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.thirdSealTextBox.TabIndex = 6;
			// 
			// ContainerTextBox
			// 
			this.ContainerTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ContainerTextBox, "ContainerBookedMoves.Container+JC_ContainerNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_ContainerNum)));
			this.ContainerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 14, true);
			this.ContainerTextBox.Name = "ContainerTextBox";
			this.ContainerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.ContainerTextBox.TabIndex = 0;
			// 
			// sealedByZDropEdit
			// 
			this.sealedByZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sealedByZDropEdit, "ContainerBookedMoves.Container+JC_SealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_SealParty)));
			this.sealedByZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 80, true);
			this.sealedByZDropEdit.Name = "sealedByZDropEdit";
			this.sealedByZDropEdit.PreBoundMaxLength = 3;
			this.sealedByZDropEdit.ShowDescriptionBox = false;
			this.sealedByZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.sealedByZDropEdit.TabIndex = 3;
			// 
			// secondSealedByZDropEdit
			// 
			this.secondSealedByZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.secondSealedByZDropEdit, "ContainerBookedMoves.Container+JC_AdditionalSealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_AdditionalSealParty)));
			this.secondSealedByZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 124, true);
			this.secondSealedByZDropEdit.Name = "secondSealedByZDropEdit";
			this.secondSealedByZDropEdit.PreBoundMaxLength = 3;
			this.secondSealedByZDropEdit.ShowDescriptionBox = false;
			this.secondSealedByZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.secondSealedByZDropEdit.TabIndex = 5;
			//
			// thirdSealedByZDropEdit
			//
			this.thirdSealedByZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.thirdSealedByZDropEdit, "ContainerBookedMoves.Container+JC_Additional2SealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonCartage)(null)).ContainerBookedMoves)).SyncRoot)).Container.JC_AdditionalSealParty)));
			this.thirdSealedByZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 168, true);
			this.thirdSealedByZDropEdit.Name = "thirdSealedByZDropEdit";
			this.thirdSealedByZDropEdit.PreBoundMaxLength = 3;
			this.thirdSealedByZDropEdit.ShowDescriptionBox = false;
			this.thirdSealedByZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.thirdSealedByZDropEdit.TabIndex = 7;
			// 
			// ContainerServicesTabPage
			// 
			this.ContainerServicesTabPage.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("ContainerDetailsControl|31bbc6e5-0708-45fa-ba00-c17217b86cb6", "Services");
			this.ContainerServicesTabPage.Controls.Add(this.servicesControl1);
			this.ContainerServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerServicesTabPage.Name = "ContainerServicesTabPage";
			this.ContainerServicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainerServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 430, true);
			this.ContainerServicesTabPage.TabIndex = 1;
			// 
			// servicesControl1
			// 
			this.servicesControl1.AllowDrop = true;
			this.servicesControl1.BindToServices = "ContainerBookedMoves.Container.Services";
			this.servicesControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.servicesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.servicesControl1.Name = "servicesControl1";
			this.servicesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 424, true);
			this.servicesControl1.TabIndex = 1;
			// 
			// ContainerDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ContainerDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 548, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainerTabControl.ResumeLayout(false);
			this.ContainerMovementsTabPage.ResumeLayout(false);
			this.ContainerDetailsTabPage.ResumeLayout(false);
			this.RefrigerationGroupBox.ResumeLayout(false);
			this.RefrigerationGroupBox.PerformLayout();
			this.ContainerGroupBox.ResumeLayout(false);
			this.ContainerGroupBox.PerformLayout();
			this.ContainerServicesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
