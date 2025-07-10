using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.AMS.Module
{
	partial class USAMSFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_JobReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_CarrierSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_ImportTransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_RL_NKPortUnlading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_FIRMS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_ImportConveyanceCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_ImportConveyanceName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_VoyageNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MovementHeader.BM_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).MovementHeader.BM_CustomsStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).ActualArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_FirstExportDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_RL_NKImportLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_LatestDispositionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_LatestDispositionCodeDescription)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("337a5c27-9802-4c82-bb43-807872724291", "Job Reference");
			zTextBoxColumnStyleInfo1.ColumnName = "BH_JobReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("a0c5513d-15a0-48c7-9965-3b2d80b5369f", "Carrier Code");
			zTextBoxColumnStyleInfo2.ColumnName = "BH_CarrierSCAC";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("eb22398b-2bd8-4c5e-acde-b4ae8f97f21e", "Transport Mode");
			zTextBoxColumnStyleInfo3.ColumnName = "BH_ImportTransportMode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("9700e892-d6f0-405c-98fc-c82f6949f145", "Port of Unlading");
			zTextBoxColumnStyleInfo4.ColumnName = "BH_RL_NKPortUnlading";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("e804c3c5-90c5-422e-a411-c4b8d122c941", "Org. Est.", "Original Estimated Time", "");
			zDateEditColumnStyleInfo1.ColumnName = "BH_ETA";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("455d3667-edf0-40a0-a455-6a8e556111ea", "FIRMS");
			zTextBoxColumnStyleInfo5.ColumnName = "BH_FIRMS";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("adc28fc4-d3d2-40a3-9dab-2b77380ac3c2", "Conveyance Country");
			zTextBoxColumnStyleInfo6.ColumnName = "BH_ImportConveyanceCountry";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("f3b9b071-bec1-4c77-a5e7-d93edec49cd8", "Conveyance Name");
			zTextBoxColumnStyleInfo7.ColumnName = "BH_ImportConveyanceName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("a2adca9c-7ec4-4c45-b379-0c20a47b3ff5", "Voyage Number");
			zTextBoxColumnStyleInfo8.ColumnName = "BH_VoyageNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("f483211e-938e-4edd-a371-96980ec2d162", "Con. Msg. Stat.", "Con. Message Status", "Conveyance Message Status", "");
			zTextBoxColumnStyleInfo9.ColumnName = "MovementHeader+BM_CustomsStatus";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("e086f293-3193-4fa9-ab2d-7d9c79fb0396", "Con. Msg. Stat. Desc.", "Con. Message Status Desc.", "Conveyance Message Status Description", "");
			zTextBoxColumnStyleInfo10.ColumnName = "MovementHeader+BM_CustomsStatusDescription";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(159);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("ddc8f893-832a-4d86-aa9a-b87ff422573b", "Actual Arrival Date");
			zTextBoxColumnStyleInfo11.ColumnName = "ActualArrivalDate";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("793d2703-f52d-49e9-bd22-a8fbc1dd1c6a", "ETD", "Estimated Date of Departure");
			zDateEditColumnStyleInfo2.ColumnName = "BH_FirstExportDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.US.AMS.Module.Res.GetData("f558e439-3135-43f4-bae7-c4c0fa28cbb6", "Load Port", "Load Port (UNLOCO)");
			zTextBoxColumnStyleInfo12.ColumnName = "BH_RL_NKImportLoadPort";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "BH_LatestDispositionCode";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "BH_LatestDispositionCodeDescription";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeader);
			// 
			// USAMSFilterStripControl
			// 
			this.Name = "USAMSFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
