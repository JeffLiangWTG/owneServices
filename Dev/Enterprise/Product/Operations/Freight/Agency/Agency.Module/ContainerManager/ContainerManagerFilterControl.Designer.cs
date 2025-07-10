namespace Enterprise.Freight.Agency.Module
{
	public partial class ContainerManagerFilterControl
	{
		protected override void Dispose(bool IsNotFinalizing)
		{
			if (IsNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(IsNotFinalizing);
		}

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "R6_ContainerNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "R6_RC";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "R6_RC_ISOType";
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "R6_OwnerType";
			zOrganisationFindBoxColumnStyleInfo1.Caption = null;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "R6_OH_Owner";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|c7dceb34-2335-419e-a6e6-ba8f58d46c7f", "Last Date", "Last Movement Date", "The date of the last container movement.");
			zDateEditColumnStyleInfo1.ColumnName = "LastMovement+E9_MovementDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|d176ecfa-7318-4460-8220-dd57915f046d", "Last Type", "Last Movement Type", "The type of the last container movement.");
			zTextBoxColumnStyleInfo4.ColumnName = "LastMovement+E9_MovementType";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|21f4cfdd-35ff-4467-9b81-0b3aa6181251", "Last Moved Empty", "Was Last Moved Empty", "Was the container empty at the time it was last received/released.");
			zCheckBoxColumnStyleInfo1.ColumnName = "LastMovement+E9_ContainerIsEmpty";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|2dc19642-453d-45bf-ae6d-07ca84b280ee", "Last Cond.", "Last Condition", "Last Container Condition", "The condition in which the container was last received/released by the depot.");
			zTextBoxColumnStyleInfo5.ColumnName = "LastMovement+E9_ContainerCondition";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|77a663cf-640d-4038-9019-56295e6aca1f", "Last Qlty.", "Last Container Quality", "The quality of the container at the time it was last received/released by the depot.");
			zTextBoxColumnStyleInfo6.ColumnName = "LastMovement+E9_ContainerQuality";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "LastMovement+Lookups+DepotOrgList";
			zOrganisationFindBoxColumnStyleInfo2.Caption = null;
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|ac7e0f22-6a71-4322-ad1a-e05b8b4ccec0", "Depot", "The depot/wharf/yard that announced the last movement.");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "LastMovement+E9_OA_Depot_ZAddress+OrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|3ac34d35-77bc-430d-ab24-8556e8c38f4d", "Last Depot");
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidDropEditColumnStyleInfo1.Caption = null;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|af8b506f-a719-4688-ab6c-f3d3492ad06c", "Address", "The address of the depot/wharf/yard that announced the last movement.");
			zGuidDropEditColumnStyleInfo1.ColumnName = "LastMovement+E9_OA_Depot";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|3ac34d35-77bc-430d-ab24-8556e8c38f4d", "Last Depot");
			zGuidDropEditColumnStyleInfo1.IsVisible = false;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.Caption = null;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|95958a0e-2be7-4bde-9b75-0bc906d804dc", "Last Depot Port", "The location where the last movement took place.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "LastMovement+DepotPort";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|95462150-13d4-4f84-b629-3f09d35603d7", "Last LC", "Last Loc. Cat.", "Last Location Category", "");
			zDropEditColumnStyleInfo1.ColumnName = "LastMovement+ToLocationCategory";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|31c4f523-1b23-4cbf-9bc0-2ef45a487327", "Last Vessel", "The vessel associated with the last movement.");
			zTextBoxColumnStyleInfo7.ColumnName = "LastMovement+Voyage+JV_RV_NKVessel";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|855136d7-e835-4eb4-9811-224491137b67", "Last Voyage", "The voyage number associated with the last movement.");
			zTextBoxColumnStyleInfo8.ColumnName = "LastMovement+Voyage+JV_VoyageFlight";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|ac3b9cd2-3ba0-44b4-9067-5d5dbd6762d4", "Last Shipment Numbs", "Last Shipment Numbers", "The shipment numbers associated with the last movement.");
			zTextBoxColumnStyleInfo9.ColumnName = "LastMovement+RelatedInfo+ShipmentNumbers";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|37719682-b5bc-4c11-829c-97022ad644b6", "Last OBLs", "Last Ocean Bills of Lading", "The ocean bills of lading associated with the last movement.");
			zTextBoxColumnStyleInfo10.ColumnName = "LastMovement+RelatedInfo+BillsOfLading";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|3f52f799-1faf-4ab6-8049-3524e5ca9dee", "Last Principal", "The principal associated with the last movement.");
			zTextBoxColumnStyleInfo11.ColumnName = "LastMovement+RelatedInfo+Principal";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo12.Caption = null;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|7f43fc02-7e51-4f48-bacb-aece95a11e23", "Last Rsp. Parties", "Last Responsible Parties", "The party responsible for the container at the time of its last movement.");
			zTextBoxColumnStyleInfo12.ColumnName = "LastMovement+RelatedInfo+LocalClient";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo13.Caption = null;
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|0e6f4d28-285f-4ea8-bb7d-6b98d910c22f", "Last Booking#", "Last Booking Numbers", "The booking numbers from any jobs associated with the last movement.");
			zTextBoxColumnStyleInfo13.ColumnName = "LastMovement+RelatedInfo+BookingNumbers";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo14.Caption = null;
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|0348ed1f-42df-4e19-8e52-3b3fbac5446d", "Last Lease Contract#", "Last Lease Contract No", "The Lease Contract No from any jobs associated with the last movement.");
			zTextBoxColumnStyleInfo14.ColumnName = "LastMovement+E9_LeaseNumber";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|74a2b852-f7b9-4ed4-9992-ba59bea9dfbb", "Last Movement Origin Port", "Last Movement Origin Port", "The origin port(s) associated with the last movement.");
			zTextBoxColumnStyleInfo15.ColumnName = "LastMovement+RelatedInfo+Origin";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|ee2e9260-8a18-4a54-baa4-b5ded2e6c833", "Last Movement Destination Port", "Last Movement Destination Port", "The destination port(s) associated with the last movement.");
			zTextBoxColumnStyleInfo16.ColumnName = "LastMovement+RelatedInfo+Destination";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|22a17e42-88bb-4d48-9967-3f37cde57b7a", "Last Movement Load Port", "Last Movement Load Port", "The load port(s) associated with the last movement.");
			zTextBoxColumnStyleInfo17.ColumnName = "LastMovement+RelatedInfo+LoadPort";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Freight.Agency.Module.Res.GetData("ContainerManagerFilterControl|94fd2bff-4df5-410e-b58c-5a67869bd074", "Last Movement Discharge Port", "Last Movement Discharge Port", "The discharge port(s) associated with the last movement.");
			zTextBoxColumnStyleInfo18.ColumnName = "LastMovement+RelatedInfo+DischargePort";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo18.IsVisible = false;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 135, true);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			// 
			// ContainerManagerFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ContainerManagerFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 287, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private System.ComponentModel.Container components = null;
	}
}
