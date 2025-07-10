using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbPortDeliveryTimeForm : ZTemplateForm
	{
		Enterprise.ZArchitecture.ZCalcEdit G1_DaysDelayFromArrivalCalcEdit;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox DischargePortCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationPortCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit FreightModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit JobModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ClientGuidFindBox;
		Enterprise.ZArchitecture.ZCalcEdit G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit;

		new void InitializeComponent()
		{
			this.JobModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreightModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.G1_DaysDelayFromArrivalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DestinationPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ClientGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 323, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit);
			this.MainTabPage.Controls.Add(this.ClientGuidFindBox);
			this.MainTabPage.Controls.Add(this.G1_DaysDelayFromArrivalCalcEdit);
			this.MainTabPage.Controls.Add(this.JobModeDropEdit);
			this.MainTabPage.Controls.Add(this.FreightModeDropEdit);
			this.MainTabPage.Controls.Add(this.DischargePortCodeFindBox);
			this.MainTabPage.Controls.Add(this.DestinationPortCodeFindBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 296, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(416);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(416);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPortDeliveryTime);
			// 
			// JobModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JobModeDropEdit, "G1_JobMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPortDeliveryTime)(null)).G1_JobMode)));
			this.JobModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 40, true);
			this.JobModeDropEdit.Name = "JobModeDropEdit";
			this.JobModeDropEdit.PreBoundMaxLength = 3;
			this.JobModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.JobModeDropEdit.TabIndex = 2;
			// 
			// FreightModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.FreightModeDropEdit, "G1_FreightMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPortDeliveryTime)(null)).G1_FreightMode)));
			this.FreightModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 16, true);
			this.FreightModeDropEdit.Name = "FreightModeDropEdit";
			this.FreightModeDropEdit.PreBoundMaxLength = 3;
			this.FreightModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.FreightModeDropEdit.TabIndex = 1;
			// 
			// DischargePortCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.DischargePortCodeFindBox, "G1_RL_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPortDeliveryTime)(null)).G1_RL_NKDischargePort)));
			this.DischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 64, true);
			this.DischargePortCodeFindBox.Name = "DischargePortCodeFindBox";
			this.DischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DischargePortCodeFindBox.TabIndex = 3;
			// 
			// G1_DaysDelayFromArrivalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.G1_DaysDelayFromArrivalCalcEdit, "G1_DaysDelayFromArrivalToDeliver");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.GlbPortDeliveryTime)(null)).G1_DaysDelayFromArrivalToDeliver)));
			this.G1_DaysDelayFromArrivalCalcEdit.Decimals = 0;
			this.G1_DaysDelayFromArrivalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 112, true);
			this.G1_DaysDelayFromArrivalCalcEdit.Name = "G1_DaysDelayFromArrivalCalcEdit";
			this.G1_DaysDelayFromArrivalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.G1_DaysDelayFromArrivalCalcEdit.TabIndex = 5;
			this.G1_DaysDelayFromArrivalCalcEdit.Text = "0";
			this.G1_DaysDelayFromArrivalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DestinationPortCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationPortCodeFindBox, "G1_RL_NKDestinationPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPortDeliveryTime)(null)).G1_RL_NKDestinationPort)));
			this.DestinationPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 88, true);
			this.DestinationPortCodeFindBox.Name = "DestinationPortCodeFindBox";
			this.DestinationPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DestinationPortCodeFindBox.TabIndex = 4;
			// 
			// ClientGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.ClientGuidFindBox, "G1_OH_ClientOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbPortDeliveryTime)(null)).G1_OH_ClientOverride)));
			this.ClientGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 136, true);
			this.ClientGuidFindBox.Name = "ClientGuidFindBox";
			this.ClientGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ClientGuidFindBox.TabIndex = 6;
			// 
			// G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit, "G1_DaysFromDestinationArrivalToClientDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.GlbPortDeliveryTime)(null)).G1_DaysFromDestinationArrivalToClientDelivery)));
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit.Decimals = 0;
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 160, true);
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit.Name = "G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit";
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit.TabIndex = 10;
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit.Text = "0";
			this.G1_DaysFromDestinationArrivalToClientDeliveryCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GlbPortDeliveryTimeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 379, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPortDeliveryTime);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 310, true);
			this.Name = "GlbPortDeliveryTimeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
