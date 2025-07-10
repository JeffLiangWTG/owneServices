using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.Module
{
	public partial class PickupDeliveryConfirmFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).PickupDeliveryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).ConsolidatedTransportBooking.D1_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).ConsolidatedTransportBooking.D1_BookingReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PlannedPickupDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_RequestedPickupDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DriversLicence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DriversName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DropMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).UniqueID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_GoodsSignForBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PickupDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_VehicleRegistration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_TransportCoName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PickupDeliveryInstruction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).Container.JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_OA_TransportProvider_ZAddress.OrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).FirstShipment.JS_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).PickupFrom.OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).BindToLists.Organisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).DeliverTo.OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Freight.Business.CommonPickupDeliveryConfirm)(null)).BindToLists.Organisations)));
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.Module.Res.GetData("PickupDeliveryConfirmFilterControl|f14af975-fbf0-4e01-9a72-d87599b66a5f", "Confirmation Type", "Confirmation Type", "");
			zTextBoxColumnStyleInfo1.ColumnName = "PickupDeliveryType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "ConsolidatedTransportBooking+D1_UniqueConsignRef";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "ConsolidatedTransportBooking+D1_BookingReference";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = null;
			zDateEditColumnStyleInfo1.ColumnName = "EU_PlannedPickupDeliveryTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = null;
			zDateEditColumnStyleInfo2.ColumnName = "EU_RequestedPickupDeliveryTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = null;
			zTextBoxColumnStyleInfo4.ColumnName = "EU_DriversLicence";
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = null;
			zTextBoxColumnStyleInfo5.ColumnName = "EU_DriversName";
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = null;
			zTextBoxColumnStyleInfo6.ColumnName = "EU_DropMode";
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Confirmations.Module.Res.GetData("PickupDeliveryConfirmFilterControl|a197a4c3-0e27-48b8-80c0-dce2f187b36d", "ID", "Unique ID", "");
			zTextBoxColumnStyleInfo7.ColumnName = "UniqueID";
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = null;
			zTextBoxColumnStyleInfo8.ColumnName = "EU_GoodsSignForBy";
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = null;
			zDateEditColumnStyleInfo3.ColumnName = "EU_PickupDeliveryTime";
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = null;
			zTextBoxColumnStyleInfo9.ColumnName = "EU_VehicleRegistration";
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = null;
			zTextBoxColumnStyleInfo10.ColumnName = "EU_TransportCoName";
			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = null;
			zTextBoxColumnStyleInfo11.ColumnName = "EU_PickupDeliveryInstruction";
			zTextBoxColumnStyleInfo12.Caption = null;
			zTextBoxColumnStyleInfo12.CaptionResourceString = null;
			zTextBoxColumnStyleInfo12.ColumnName = "Container+JC_ContainerNum";
			zOrganisationFindBoxColumnStyleInfo1.Caption = null;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.Module.Res.GetData("PickupDeliveryConfirmFilterControl|0b4aed4a-07dd-4ef3-9b7b-ba29c33980b0", "Transport Co", "Transport Company", "");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "EU_OA_TransportProvider_ZAddress+OrgPK";
			zTextBoxColumnStyleInfo13.Caption = null;
			zTextBoxColumnStyleInfo13.CaptionResourceString = null;
			zTextBoxColumnStyleInfo13.ColumnName = "FirstShipment+JS_UniqueConsignRef";
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "BindToLists+Organisations";
			zOrganisationFindBoxColumnStyleInfo2.Caption = null;
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Confirmations.Module.Res.GetData("PickupDeliveryConfirmFilterControl|d816d4ec-5f0d-4924-b139-3f95e360e989", "Pickup", "Pickup", "");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "PickupFrom+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo2.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo3.BindToList = "BindToLists+Organisations";
			zOrganisationFindBoxColumnStyleInfo3.Caption = null;
			zOrganisationFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Confirmations.Module.Res.GetData("PickupDeliveryConfirmFilterControl|20e6c823-9d26-4ca9-9439-10165b2456a4", "Delivery", "Delivery", "");
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "DeliverTo+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo3.IsReadOnly = true;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Freight.Business.CommonPickupDeliveryConfirm);
			// 
			// PickupDeliveryConfirmFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "PickupDeliveryConfirmFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}