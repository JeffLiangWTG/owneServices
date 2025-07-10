using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.Module
{
	public partial class ConsolidatedTransportBookingFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Freight.Business.CommonConsolidatedTransportBooking)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonConsolidatedTransportBooking)(null)).D1_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonConsolidatedTransportBooking)(null)).D1_BookingReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Freight.Business.CommonConsolidatedTransportBooking)(null)).D1_BookingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Freight.Business.CommonConsolidatedTransportBooking)(null)).D1_TransportCoBookingContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Freight.Business.CommonConsolidatedTransportBooking)(null)).D1_OA_TransportCo_ZAddress.OrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Freight.Business.CommonConsolidatedTransportBooking)(null)).D1_OA_Customer_ZAddress.OrgPK)));
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "D1_UniqueConsignRef";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "D1_BookingReference";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = null;
			zDateEditColumnStyleInfo1.ColumnName = "D1_BookingDate";
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "D1_TransportCoBookingContact";
			zOrganisationFindBoxColumnStyleInfo1.Caption = null;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.Module.Res.GetData("ConsolidatedTransportBookingFilterControl|b92d300b-0783-48b3-a873-6b2aa5d65c4e", "Transport Co", "Transport Company", "");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "D1_OA_TransportCo_ZAddress+OrgPK";
			zOrganisationFindBoxColumnStyleInfo2.Caption = null;
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Confirmations.Module.Res.GetData("ConsolidatedTransportBookingFilterControl|c177a7ef-5c95-4a25-b780-fdf066d69f7f", "Customer", "Customer", "");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "D1_OA_Customer_ZAddress+OrgPK";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Freight.Business.CommonConsolidatedTransportBooking);
			// 
			// ConsolidatedTransportBookingFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ConsolidatedTransportBookingFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}