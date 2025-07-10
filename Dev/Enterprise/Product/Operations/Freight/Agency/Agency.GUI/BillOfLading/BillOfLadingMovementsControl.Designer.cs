using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.GUI
{
	partial class BillOfLadingMovementsControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			movementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(movementsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLading);
			// 
			// movementsGrid
			// 
			movementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(movementsGrid, "FCLContainers.Movements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_MovementDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_MovementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_ContainerCondition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_ContainerQuality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_OA_Depot_ZAddress.OrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).Lookups.DepotOrgList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_OA_Depot)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_OA_Depot_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_OtherLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_DetentionDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_NC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).DepotPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).VoyageNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).E9_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Agency.Business.ContainerMovement)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).RealContainers)).SyncRoot)).Movements)).SyncRoot)).CreatedTimeLocal)));
			movementsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "E9_MovementDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "E9_MovementType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "E9_ContainerCondition";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "E9_ContainerQuality";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+DepotOrgList";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMovementsControl|d30085e2-eb5d-4b2e-8937-42b2a9408207", "Depot");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "E9_OA_Depot_ZAddress+OrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMovementsControl|8cb19646-a278-4be3-8c2a-88034c8d5cd4", "Depot Address");
			zGuidDropEditColumnStyleInfo1.BindToList = "E9_OA_Depot_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo1.ColumnName = "E9_OA_Depot";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMovementsControl|8cb19646-a278-4be3-8c2a-88034c8d5cd4", "Depot Address");
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "E9_OtherLocation";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "E9_DetentionDays";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "E9_NC";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "DepotPort";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "VesselName";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "VoyageNo";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo2.ColumnName = "E9_SystemCreateTimeUtc";			
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.ColumnName = "CreatedTimeLocal";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "E9_SystemCreateUser";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			movementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			movementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			movementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			movementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			movementsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			movementsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			movementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			movementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			movementsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			movementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			movementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			movementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			movementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			movementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			movementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			movementsGrid.GridId = "fdd055e3-aa0d-4054-99e3-3b9d6ca8536e";
			movementsGrid.CopySelectedRowsAllowed = true;
			movementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			movementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			movementsGrid.LayoutKey = "movementsGrid";
			movementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			movementsGrid.Name = "movementsGrid";
			movementsGrid.ReadOnly = true;
			movementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 213, true);
			movementsGrid.TabIndex = 0;
			// 
			// BillOfLadingMovementsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(movementsGrid);
			this.Name = "BillOfLadingMovementsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 213, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(movementsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		Enterprise.ZArchitecture.ZGrid movementsGrid;
	}
}
