namespace Enterprise.Customs.GUI
{
	public partial class PickupPartiesUserControl
	{
		private void InitializeComponent()
		{
			this.PickupCartageCoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeliveryOrPickupCartageCoAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PickupDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickupCartageCoGroupBox.SuspendLayout();
			this.DeliveryOrPickupCartageCoAddressControl.SuspendLayout();
			this.PickupDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PickupCartageCoGroupBox
			// 
			this.PickupCartageCoGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("719044bf-4ac0-48ff-8287-396f126cd4ec", "Pickup Transport Company");
			this.PickupCartageCoGroupBox.Controls.Add(this.DeliveryOrPickupCartageCoAddressControl);
			this.PickupCartageCoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PickupCartageCoGroupBox.Name = "PickupCartageCoGroupBox";
			this.PickupCartageCoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 61, true);
			this.PickupCartageCoGroupBox.TabIndex = 0;
			this.PickupCartageCoGroupBox.TabStop = false;
			this.PickupCartageCoGroupBox.UseCompatibleTextRendering = true;
			// 
			// DeliveryOrPickupCartageCoAddressControl
			// 
			this.DeliveryOrPickupCartageCoAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryOrPickupCartageCoAddressControl, "JE_OA_DeliveryOrPickupCartageCoAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_DeliveryOrPickupCartageCoAddr)));
			this.DeliveryOrPickupCartageCoAddressControl.BindToOrgList = "Lookups+CartageList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryOrPickupCartageCoAddressControl, false);
			this.DeliveryOrPickupCartageCoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 14, true);
			this.DeliveryOrPickupCartageCoAddressControl.Name = "DeliveryOrPickupCartageCoAddressControl";
			this.DeliveryOrPickupCartageCoAddressControl.PopupCaption = "";
			this.DeliveryOrPickupCartageCoAddressControl.ReadOnly = false;
			this.DeliveryOrPickupCartageCoAddressControl.ShowAddress = false;
			this.DeliveryOrPickupCartageCoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 41, true);
			this.DeliveryOrPickupCartageCoAddressControl.StackControls = true;
			this.DeliveryOrPickupCartageCoAddressControl.TabIndex = 0;
			// 
			// PickupDocAddressControl
			// 
			this.PickupDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickupDocAddressControl, "SupplierPickupAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).SupplierPickupAddress)));
			this.PickupDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.PickupDocAddressControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8317b9ce-2b09-4272-8bac-7acbeb2d04e5", "Pickup From");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PickupDocAddressControl, false);
			this.PickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 70, true);
			this.PickupDocAddressControl.Name = "PickupDocAddressControl";
			this.PickupDocAddressControl.ReadOnly = false;
			this.PickupDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.PickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.PickupDocAddressControl.TabIndex = 1;
			this.PickupDocAddressControl.ValidationJustForced = false;
			// 
			// PickupPartiesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickupDocAddressControl);
			this.Controls.Add(this.PickupCartageCoGroupBox);
			this.Name = "PickupPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickupCartageCoGroupBox.ResumeLayout(false);
			this.PickupCartageCoGroupBox.PerformLayout();
			this.DeliveryOrPickupCartageCoAddressControl.ResumeLayout(true);
			this.DeliveryOrPickupCartageCoAddressControl.PerformLayout();
			this.PickupDocAddressControl.ResumeLayout(true);
			this.PickupDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.GUI.ZGroupBox PickupCartageCoGroupBox;
		ZArchitecture.GUI.ZAddressControl DeliveryOrPickupCartageCoAddressControl;
		MasterFiles.GUI.ZDocAddressControl PickupDocAddressControl;
	}
}
