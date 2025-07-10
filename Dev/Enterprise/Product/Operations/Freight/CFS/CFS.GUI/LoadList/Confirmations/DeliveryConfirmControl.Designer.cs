namespace Enterprise.Freight.CFS.GUI
{
	partial class DeliveryConfirmControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainerPickupDeliveryConfirmPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DepartureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DestinationCFSDepartureConfirmDetails = new Enterprise.Freight.CFS.GUI.ConfirmDetailsControl();
			this.ArrivalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersDestinationCFSArrivalConfirmDetails = new Enterprise.Freight.CFS.GUI.ConfirmDetailsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainerPickupDeliveryConfirmPanel.SuspendLayout();
			this.DepartureGroupBox.SuspendLayout();
			this.ArrivalGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.CFS.Business.CFSLoadListConsol);
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_RC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ArrivalSlotDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ArrivalSlotReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ReleaseNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_TotalLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_TotalWidth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_TotalHeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_TareWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_GrossWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_EmptyReturnedBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerYardEmptyReturnGateIn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_EmptyRequired)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_DepartureEstimatedPickup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_LCLUnpack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_LCLAvailable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_LCLStorageCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_AdditionalSealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_PackDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_Calc_ConsolID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerStorageLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerQuality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ContainerRating)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_ExportDepotCustomsReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_TrainWagonNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).JC_TempRecorderSerialNo)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JC_ContainerNum";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JC_RC";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.ColumnName = "JC_ContainerMode";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDateEditColumnStyleInfo1.ColumnName = "JC_ArrivalSlotDateTime";
			zDateEditColumnStyleInfo1.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("DeliveryConfirmControl|fcabdba6-b187-488f-a9dc-c04966a2ebd5", "Arrival Slot");
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JC_ArrivalSlotReference";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("DeliveryConfirmControl|fcabdba6-b187-488f-a9dc-c04966a2ebd5", "Arrival Slot");
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ColumnName = "JC_ReleaseNum";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JC_TotalLength";
			zCalcEditColumnStyleInfo1.Decimals = 3;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JC_TotalWidth";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JC_TotalHeight";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JC_TareWeight";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JC_GrossWeight";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("DeliveryConfirmControl|2afe1cb3-b8b7-48f8-9e1b-eb48496381be", "Gross Weight");
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zDropEditColumnStyleInfo2.ColumnName = "JC_GrossWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("DeliveryConfirmControl|2afe1cb3-b8b7-48f8-9e1b-eb48496381be", "Gross Weight");
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDateEditColumnStyleInfo2.ColumnName = "JC_EmptyReturnedBy";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "JC_ContainerMode";
			zDropEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "JC_ContainerYardEmptyReturnGateIn";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo4.ColumnName = "JC_EmptyRequired";
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo5.ColumnName = "JC_DepartureEstimatedPickup";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo6.ColumnName = "JC_LCLUnpack";
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo7.ColumnName = "JC_LCLAvailable";
			zDateEditColumnStyleInfo7.IsReadOnly = true;
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo8.ColumnName = "JC_LCLStorageCommences";
			zDateEditColumnStyleInfo8.IsReadOnly = true;
			zDateEditColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "JC_SealNum";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "JC_AdditionalSealNum";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo9.ColumnName = "JC_PackDate";
			zDateEditColumnStyleInfo9.IsReadOnly = true;
			zDateEditColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "JC_Calc_ConsolID";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "JC_ContainerStorageLocation";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "JC_ContainerStatus";
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo5.ColumnName = "JC_ContainerQuality";
			zDropEditColumnStyleInfo5.IsReadOnly = true;
			zDropEditColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "JC_ContainerRating";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.ColumnName = "JC_ExportDepotCustomsReference";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.ColumnName = "JC_TrainWagonNumber";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "JC_TempRecorderSerialNo";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ContainersGrid.GridId = "44440e3d-e085-4295-b623-dd61f8294c37";
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "zGrid2";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.ReadOnly = true;
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 250, true);
			this.ContainersGrid.TabIndex = 5;
			// 
			// ContainerPickupDeliveryConfirmPanel
			// 
			this.ContainerPickupDeliveryConfirmPanel.Controls.Add(this.DepartureGroupBox);
			this.ContainerPickupDeliveryConfirmPanel.Controls.Add(this.ArrivalGroupBox);
			this.ContainerPickupDeliveryConfirmPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ContainerPickupDeliveryConfirmPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 250, true);
			this.ContainerPickupDeliveryConfirmPanel.Name = "ContainerPickupDeliveryConfirmPanel";
			this.ContainerPickupDeliveryConfirmPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 195, true);
			this.ContainerPickupDeliveryConfirmPanel.TabIndex = 4;
			// 
			// DepartureGroupBox
			// 
			this.DepartureGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("DeliveryConfirmControl|964977d2-6190-49c2-91ed-371f42bc9599", "Departure From CFS");
			this.DepartureGroupBox.Controls.Add(this.DestinationCFSDepartureConfirmDetails);
			this.DepartureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 0, true);
			this.DepartureGroupBox.Name = "DepartureGroupBox";
			this.DepartureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 192, true);
			this.DepartureGroupBox.TabIndex = 44;
			this.DepartureGroupBox.TabStop = false;
			// 
			// DestinationCFSDepartureConfirmDetails
			// 
			this.BindingSource.SetBindingMember(this.DestinationCFSDepartureConfirmDetails, "Containers.DestinationCFSDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).DestinationCFSDeparture)));
			this.DestinationCFSDepartureConfirmDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DestinationCFSDepartureConfirmDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DestinationCFSDepartureConfirmDetails.Name = "DestinationCFSDepartureConfirmDetails";
			this.DestinationCFSDepartureConfirmDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 173, true);
			this.DestinationCFSDepartureConfirmDetails.TabIndex = 0;
			// 
			// ArrivalGroupBox
			// 
			this.ArrivalGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("DeliveryConfirmControl|ac3dd5b7-05d3-49b8-bcde-4e9292c9247e", "Arrival At CFS");
			this.ArrivalGroupBox.Controls.Add(this.ContainersDestinationCFSArrivalConfirmDetails);
			this.ArrivalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ArrivalGroupBox.Name = "ArrivalGroupBox";
			this.ArrivalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 192, true);
			this.ArrivalGroupBox.TabIndex = 43;
			this.ArrivalGroupBox.TabStop = false;
			// 
			// ContainersDestinationCFSArrivalConfirmDetails
			// 
			this.BindingSource.SetBindingMember(this.ContainersDestinationCFSArrivalConfirmDetails, "Containers.DestinationCFSArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(((Enterprise.Freight.CFS.Business.CFSContainer)(((System.Collections.IList)(((Enterprise.Freight.CFS.Business.CFSLoadListConsol)(null)).Containers)).SyncRoot)).DestinationCFSArrival)));
			this.ContainersDestinationCFSArrivalConfirmDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersDestinationCFSArrivalConfirmDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainersDestinationCFSArrivalConfirmDetails.Name = "ContainersDestinationCFSArrivalConfirmDetails";
			this.ContainersDestinationCFSArrivalConfirmDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 173, true);
			this.ContainersDestinationCFSArrivalConfirmDetails.TabIndex = 0;
			// 
			// DeliveryConfirmControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainersGrid);
			this.Controls.Add(this.ContainerPickupDeliveryConfirmPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 300, true);
			this.Name = "DeliveryConfirmControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 445, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainerPickupDeliveryConfirmPanel.ResumeLayout(false);
			this.DepartureGroupBox.ResumeLayout(false);
			this.ArrivalGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ContainersGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel ContainerPickupDeliveryConfirmPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DepartureGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ArrivalGroupBox;
		private ConfirmDetailsControl DestinationCFSDepartureConfirmDetails;
		private ConfirmDetailsControl ContainersDestinationCFSArrivalConfirmDetails;
	}
}
