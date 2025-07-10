namespace Enterprise.Customs.GUI
{
	partial class CusUnderbondUserControl
	{
		#region Component Designer generated code

		Enterprise.ZArchitecture.GUI.ZButton CreateNewUnderbondButton;
		internal Enterprise.Customs.GUI.CusUnderbondDetailsUserControl DetailsUserControl;
		public Enterprise.ZArchitecture.ZGrid UnderbondsGrid;
		CargoWise.Windows.UI.KSplitContainer OutturnSplitContainer;
		Enterprise.ZArchitecture.GUI.ZPanel UnderbondDetailsPanel;
		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.UnderbondsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UnderbondDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OutturnSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CreateNewUnderbondButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UnderbondsGrid)).BeginInit();
			this.UnderbondDetailsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OutturnSplitContainer)).BeginInit();
			this.OutturnSplitContainer.Panel1.SuspendLayout();
			this.OutturnSplitContainer.Panel2.SuspendLayout();
			this.OutturnSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusUnderbondUnionCollectionParentCollection);
			// 
			// UnderbondsGrid
			// 
			this.UnderbondsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnderbondsGrid, "AllUnderbonds");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_SendersMessageReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_MovementReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).Lookups.RequestReasonList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).LinkedObjectStringRepresentation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).Lookups.UnderbondForList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_ModeOfMovement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).Lookups.ModeOfTransportList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_IsMoveFromDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_OriginPremiseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_DestinationPremiseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_ResponsiblePartyID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_DateOfArrivalIntoDestinationPremise)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_FlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_ArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_PiecesManifested)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).UnderbondStatus.Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).ApprovalStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_UnderbondBySeaLloydsIMONum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_UnderbondBySeaVoyage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).C4_UnderbondBySeaVessel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(((System.Collections.IList)(((Enterprise.Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent)(null)).AllUnderbonds)).SyncRoot)).Lookups.UnderbondBySeaVessels)));
			this.UnderbondsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Reference";
			zTextBoxColumnStyleInfo1.ColumnName = "C4_SendersMessageReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+RequestReasonList";
			zDropEditColumnStyleInfo1.Caption = "Movement Reason";
			zDropEditColumnStyleInfo1.ColumnName = "C4_MovementReason";
			zDropEditColumnStyleInfo1.ToolTip = "Movement Reason";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.UnderbondForList";
			zDropEditColumnStyleInfo2.Caption = "Underbond For";
			zDropEditColumnStyleInfo2.ColumnName = "LinkedObjectStringRepresentation";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.ModeOfTransportList";
			zDropEditColumnStyleInfo3.Caption = "Mode Of Movement";
			zDropEditColumnStyleInfo3.ColumnName = "C4_ModeOfMovement";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.Caption = "Discharge";
			zCheckBoxColumnStyleInfo1.ColumnName = "C4_IsMoveFromDischarge";
			zCheckBoxColumnStyleInfo1.ToolTip = "Is Move From Discharge";
			zTextBoxColumnStyleInfo2.Caption = "Origin ID";
			zTextBoxColumnStyleInfo2.ColumnName = "C4_OriginPremiseID";
			zTextBoxColumnStyleInfo2.ToolTip = "Origin Premise ID";
			zTextBoxColumnStyleInfo3.Caption = "Destination ID";
			zTextBoxColumnStyleInfo3.ColumnName = "C4_DestinationPremiseID";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "Responsible Party ID";
			zTextBoxColumnStyleInfo4.ColumnName = "C4_ResponsiblePartyID";
			zTextBoxColumnStyleInfo4.ToolTip = "Responsible Party ID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo1.Caption = "Arrival Date";
			zDateEditColumnStyleInfo1.ColumnName = "C4_DateOfArrivalIntoDestinationPremise";
			zDateEditColumnStyleInfo1.ToolTip = "Arrival Date Into Destination Premise";
			zTextBoxColumnStyleInfo5.Caption = "Flight No";
			zTextBoxColumnStyleInfo5.ColumnName = "C4_FlightNo";
			zDateEditColumnStyleInfo2.Caption = "Arrival Date";
			zDateEditColumnStyleInfo2.ColumnName = "C4_ArrivalDate";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Pieces Manifested";
			zCalcEditColumnStyleInfo1.ColumnName = "C4_PiecesManifested";
			zTextBoxColumnStyleInfo6.Caption = "Package Type";
			zTextBoxColumnStyleInfo6.ColumnName = "C4_PackageType";
			zTextBoxColumnStyleInfo7.Caption = "Status";
			zTextBoxColumnStyleInfo7.ColumnName = "UnderbondStatus+Description";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zTextBoxColumnStyleInfo8.Caption = "Status";
			zTextBoxColumnStyleInfo8.ColumnName = "ApprovalStatus";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo9.Caption = "Lloyds";
			zTextBoxColumnStyleInfo9.ColumnName = "C4_UnderbondBySeaLloydsIMONum";
			zTextBoxColumnStyleInfo10.Caption = "UBS Voyage";
			zTextBoxColumnStyleInfo10.ColumnName = "C4_UnderbondBySeaVoyage";
			zTextBoxColumnStyleInfo10.ToolTip = "Underbond By Sea Voyage";
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+UnderbondBySeaVessels";
			zCodeFindBoxColumnStyleInfo1.Caption = "UBS Vessel";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "C4_UnderbondBySeaVessel";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Underbond By Sea Vessel";
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UnderbondsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.UnderbondsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.UnderbondsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.UnderbondsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UnderbondsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.UnderbondsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.UnderbondsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.UnderbondsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.UnderbondsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.UnderbondsGrid.CopySelectedRowsAllowed = true;
			this.UnderbondsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondsGrid.GridId = "aadf91f5-ee83-41f8-850d-5c873f7fcc6a";
			this.UnderbondsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnderbondsGrid.LayoutKey = "zGrid1";
			this.UnderbondsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnderbondsGrid.Name = "UnderbondsGrid";
			this.UnderbondsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 67, true);
			this.UnderbondsGrid.TabIndex = 0;
			this.UnderbondsGrid.CurrentCellChanged += new System.EventHandler(this.UnderbondsGrid_CurrentCellChanged);
			// 
			// UnderbondDetailsPanel
			// 
			this.UnderbondDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnderbondDetailsPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 184, true);
			this.UnderbondDetailsPanel.Name = "UnderbondDetailsPanel";
			this.UnderbondDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 190, true);
			this.UnderbondDetailsPanel.TabIndex = 4;
			// 
			// OutturnSplitContainer
			// 
			this.OutturnSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutturnSplitContainer.Font = new System.Drawing.Font("Tahoma", 8F);
			this.OutturnSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OutturnSplitContainer.Name = "OutturnSplitContainer";
			this.OutturnSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// OutturnSplitContainer.Panel1
			// 
			this.OutturnSplitContainer.Panel1.Controls.Add(this.UnderbondsGrid);
			this.OutturnSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40);
			// 
			// OutturnSplitContainer.Panel2
			// 
			this.OutturnSplitContainer.Panel2.Controls.Add(this.CreateNewUnderbondButton);
			this.OutturnSplitContainer.Panel2.Controls.Add(this.UnderbondDetailsPanel);
			this.OutturnSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(184);
			this.OutturnSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 261, true);
			this.OutturnSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			this.OutturnSplitContainer.TabIndex = 7;
			this.OutturnSplitContainer.TabStop = false;
			// 
			// CreateNewUnderbondButton
			// 
			this.CreateNewUnderbondButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateNewUnderbondButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(835, 0, true);
			this.CreateNewUnderbondButton.Name = "CreateNewUnderbondButton";
			this.CreateNewUnderbondButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CreateNewUnderbondButton.TabIndex = 11;
			this.CreateNewUnderbondButton.Text = "Create Underbond";
			this.CreateNewUnderbondButton.Click += new System.EventHandler(this.CreateNewUnderbondButton_Click);
			// 
			// CusUnderbondUserControl
			// 
			this.Controls.Add(this.OutturnSplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 261, true);
			this.Name = "CusUnderbondUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 261, true);
			this.Load += new System.EventHandler(this.CusUnderbondUserControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UnderbondsGrid)).EndInit();
			this.UnderbondDetailsPanel.ResumeLayout(false);
			this.OutturnSplitContainer.Panel1.ResumeLayout(false);
			this.OutturnSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OutturnSplitContainer)).EndInit();
			this.OutturnSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
