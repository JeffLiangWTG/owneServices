namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	partial class OrderPlanningVesselVoyageAndDatesControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.JD_E_ARVBoundDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JD_E_DEP_3BoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JD_E_ARV_2ndIntermediateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JD_E_DEP_2BoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExpectedETALabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.JD_E_ARV_1stIntermediateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JD_ArrivalVoyageBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_IntermediateVoyageBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_DepartureVoyageBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JD_E_DEPBoundDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VesselLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JD_RV_NKIntermediateVesselBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JD_RV_NKArrivalVesselBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JD_RV_NKDepartureVesselBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.Order);
			// 
			// zLabel4
			// 
			this.zLabel4.AutoSize = true;
			this.zLabel4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|96f49ee5-b95c-4189-9c4a-3d1b5e2a2087", "Voyage/Flight");
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 1, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.zLabel4.TabIndex = 107;
			// 
			// JD_E_ARVBoundDateEdit2
			// 
			this.JD_E_ARVBoundDateEdit2.AutoCompleteMonthThreshold = 1;
			this.JD_E_ARVBoundDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_ARVBoundDateEdit2, "JD_Milestone_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Milestone_E_ARV)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_E_ARVBoundDateEdit2, false);
			this.JD_E_ARVBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 64, true);
			this.JD_E_ARVBoundDateEdit2.Name = "JD_E_ARVBoundDateEdit2";
			this.JD_E_ARVBoundDateEdit2.TabIndex = 16;
			// 
			// JD_E_DEP_3BoundDateEdit
			// 
			this.JD_E_DEP_3BoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_DEP_3BoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_DEP_3BoundDateEdit, "JD_E_DEP_3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_E_DEP_3)));
			this.JD_E_DEP_3BoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 64, true);
			this.JD_E_DEP_3BoundDateEdit.Name = "JD_E_DEP_3BoundDateEdit";
			this.JD_E_DEP_3BoundDateEdit.TabIndex = 15;
			// 
			// JD_E_ARV_2ndIntermediateBoundDateEdit
			// 
			this.JD_E_ARV_2ndIntermediateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_ARV_2ndIntermediateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_ARV_2ndIntermediateBoundDateEdit, "JD_E_ARV_2ndIntermediate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_E_ARV_2ndIntermediate)));
			this.JD_E_ARV_2ndIntermediateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 40, true);
			this.JD_E_ARV_2ndIntermediateBoundDateEdit.Name = "JD_E_ARV_2ndIntermediateBoundDateEdit";
			this.JD_E_ARV_2ndIntermediateBoundDateEdit.TabIndex = 12;
			// 
			// JD_E_DEP_2BoundDateEdit
			// 
			this.JD_E_DEP_2BoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_DEP_2BoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_DEP_2BoundDateEdit, "JD_E_DEP_2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_E_DEP_2)));
			this.JD_E_DEP_2BoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 40, true);
			this.JD_E_DEP_2BoundDateEdit.Name = "JD_E_DEP_2BoundDateEdit";
			this.JD_E_DEP_2BoundDateEdit.TabIndex = 11;
			// 
			// ExpectedETALabel
			// 
			this.ExpectedETALabel.AutoSize = true;
			this.ExpectedETALabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|eadb5b73-390b-413c-84f1-d99998d91e53", "Estimated Arrive");
			this.ExpectedETALabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 1, true);
			this.ExpectedETALabel.Name = "ExpectedETALabel";
			this.ExpectedETALabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.ExpectedETALabel.TabIndex = 106;
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|d6515307-a98e-459f-8871-e73811ce2f24", "Estimated Depart");
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(476, 1, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.zLabel6.TabIndex = 105;
			// 
			// JD_E_ARV_1stIntermediateBoundDateEdit
			// 
			this.JD_E_ARV_1stIntermediateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JD_E_ARV_1stIntermediateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_ARV_1stIntermediateBoundDateEdit, "JD_E_ARV_1stIntermediate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_E_ARV_1stIntermediate)));
			this.JD_E_ARV_1stIntermediateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 16, true);
			this.JD_E_ARV_1stIntermediateBoundDateEdit.Name = "JD_E_ARV_1stIntermediateBoundDateEdit";
			this.JD_E_ARV_1stIntermediateBoundDateEdit.TabIndex = 8;
			// 
			// JD_ArrivalVoyageBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_ArrivalVoyageBoundTextBox, "JD_ArrivalVoyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_ArrivalVoyage)));
			this.JD_ArrivalVoyageBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|d52592f5-4972-4824-8d54-8f24f7efb37d", "Arrival");
			this.JD_ArrivalVoyageBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 64, true);
			this.JD_ArrivalVoyageBoundTextBox.Name = "JD_ArrivalVoyageBoundTextBox";
			this.JD_ArrivalVoyageBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JD_ArrivalVoyageBoundTextBox.TabIndex = 14;
			// 
			// JD_IntermediateVoyageBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_IntermediateVoyageBoundTextBox, "JD_IntermediateVoyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_IntermediateVoyage)));
			this.JD_IntermediateVoyageBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|d85d4a37-111c-45a2-9ba5-f37f5b508ac1", "Intermediate");
			this.JD_IntermediateVoyageBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 40, true);
			this.JD_IntermediateVoyageBoundTextBox.Name = "JD_IntermediateVoyageBoundTextBox";
			this.JD_IntermediateVoyageBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JD_IntermediateVoyageBoundTextBox.TabIndex = 10;
			// 
			// JD_DepartureVoyageBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JD_DepartureVoyageBoundTextBox, "JD_DepartureVoyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_DepartureVoyage)));
			this.JD_DepartureVoyageBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|3f7a2044-e513-4d18-9d56-f7c61bd89ee8", "Departure");
			this.JD_DepartureVoyageBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 16, true);
			this.JD_DepartureVoyageBoundTextBox.Name = "JD_DepartureVoyageBoundTextBox";
			this.JD_DepartureVoyageBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.JD_DepartureVoyageBoundTextBox.TabIndex = 6;
			// 
			// JD_E_DEPBoundDateEdit2
			// 
			this.JD_E_DEPBoundDateEdit2.AutoCompleteMonthThreshold = 1;
			this.JD_E_DEPBoundDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JD_E_DEPBoundDateEdit2, "JD_Milestone_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_Milestone_E_DEP)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JD_E_DEPBoundDateEdit2, false);
			this.JD_E_DEPBoundDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 16, true);
			this.JD_E_DEPBoundDateEdit2.Name = "JD_E_DEPBoundDateEdit2";
			this.JD_E_DEPBoundDateEdit2.TabIndex = 7;
			// 
			// VesselLabel
			// 
			this.VesselLabel.AutoSize = true;
			this.VesselLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|8a80b7bb-2b3c-419a-9d48-6458792a4ffc", "Vessel");
			this.VesselLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 0, true);
			this.VesselLabel.Name = "VesselLabel";
			this.VesselLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 13, true);
			this.VesselLabel.TabIndex = 106;
			// 
			// JD_RV_NKIntermediateVesselBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.JD_RV_NKIntermediateVesselBoundFindBox, "JD_RV_NKIntermediateVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_RV_NKIntermediateVessel)));
			this.JD_RV_NKIntermediateVesselBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|cb386893-6803-4d92-9e7f-c41aa217564a", "Intermediate");
			this.JD_RV_NKIntermediateVesselBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 40, true);
			this.JD_RV_NKIntermediateVesselBoundFindBox.Name = "JD_RV_NKIntermediateVesselBoundFindBox";
			this.JD_RV_NKIntermediateVesselBoundFindBox.PreBoundMaxLength = 35;
			this.JD_RV_NKIntermediateVesselBoundFindBox.ShowDescriptionBox = false;
			this.JD_RV_NKIntermediateVesselBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.JD_RV_NKIntermediateVesselBoundFindBox.TabIndex = 9;
			// 
			// JD_RV_NKArrivalVesselBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.JD_RV_NKArrivalVesselBoundFindBox, "JD_RV_NKArrivalVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_RV_NKArrivalVessel)));
			this.JD_RV_NKArrivalVesselBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|61a6835a-70e3-41b9-ac31-79591a99d112", "Arrival");
			this.JD_RV_NKArrivalVesselBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 64, true);
			this.JD_RV_NKArrivalVesselBoundFindBox.Name = "JD_RV_NKArrivalVesselBoundFindBox";
			this.JD_RV_NKArrivalVesselBoundFindBox.PreBoundMaxLength = 35;
			this.JD_RV_NKArrivalVesselBoundFindBox.ShowDescriptionBox = false;
			this.JD_RV_NKArrivalVesselBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.JD_RV_NKArrivalVesselBoundFindBox.TabIndex = 13;
			// 
			// JD_RV_NKDepartureVesselBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.JD_RV_NKDepartureVesselBoundFindBox, "JD_RV_NKDepartureVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.Order)(null)).JD_RV_NKDepartureVessel)));
			this.JD_RV_NKDepartureVesselBoundFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderPlanningVesselVoyageAndDatesControl|860598d4-407b-4c76-92b1-0e9f6b6d026c", "Departure");
			this.JD_RV_NKDepartureVesselBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 16, true);
			this.JD_RV_NKDepartureVesselBoundFindBox.Name = "JD_RV_NKDepartureVesselBoundFindBox";
			this.JD_RV_NKDepartureVesselBoundFindBox.PreBoundMaxLength = 35;
			this.JD_RV_NKDepartureVesselBoundFindBox.ShowDescriptionBox = false;
			this.JD_RV_NKDepartureVesselBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.JD_RV_NKDepartureVesselBoundFindBox.TabIndex = 5;
			// 
			// OrderPlanningVesselVoyageAndDatesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.JD_E_ARVBoundDateEdit2);
			this.Controls.Add(this.JD_E_DEP_3BoundDateEdit);
			this.Controls.Add(this.JD_E_ARV_2ndIntermediateBoundDateEdit);
			this.Controls.Add(this.JD_E_DEP_2BoundDateEdit);
			this.Controls.Add(this.ExpectedETALabel);
			this.Controls.Add(this.zLabel6);
			this.Controls.Add(this.JD_E_ARV_1stIntermediateBoundDateEdit);
			this.Controls.Add(this.JD_ArrivalVoyageBoundTextBox);
			this.Controls.Add(this.JD_IntermediateVoyageBoundTextBox);
			this.Controls.Add(this.JD_DepartureVoyageBoundTextBox);
			this.Controls.Add(this.JD_E_DEPBoundDateEdit2);
			this.Controls.Add(this.VesselLabel);
			this.Controls.Add(this.JD_RV_NKIntermediateVesselBoundFindBox);
			this.Controls.Add(this.JD_RV_NKArrivalVesselBoundFindBox);
			this.Controls.Add(this.JD_RV_NKDepartureVesselBoundFindBox);
			this.Name = "OrderPlanningVesselVoyageAndDatesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 88, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Enterprise.ZArchitecture.ZLabel zLabel4;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JD_E_ARVBoundDateEdit2;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JD_E_DEP_3BoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JD_E_ARV_2ndIntermediateBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JD_E_DEP_2BoundDateEdit;
		private Enterprise.ZArchitecture.ZLabel ExpectedETALabel;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JD_E_ARV_1stIntermediateBoundDateEdit;
		private Enterprise.ZArchitecture.ZTextBox JD_ArrivalVoyageBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox JD_IntermediateVoyageBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox JD_DepartureVoyageBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JD_E_DEPBoundDateEdit2;
		protected Enterprise.ZArchitecture.ZLabel VesselLabel;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox JD_RV_NKIntermediateVesselBoundFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox JD_RV_NKArrivalVesselBoundFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox JD_RV_NKDepartureVesselBoundFindBox;
	}
}
