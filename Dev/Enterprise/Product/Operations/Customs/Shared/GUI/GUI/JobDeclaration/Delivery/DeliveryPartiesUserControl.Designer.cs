namespace Enterprise.Customs.GUI
{
	public partial class DeliveryPartiesUserControl
	{
		void InitializeComponent()
		{
			this.DeliveryCartageCoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeliveryOrPickupCartageCoAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DeliveryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryCartageCoGroupBox.SuspendLayout();
			this.DeliveryOrPickupCartageCoAddressControl.SuspendLayout();
			this.DeliveryDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// DeliveryCartageCoGroupBox
			// 
			this.DeliveryCartageCoGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fc3bd4ea-7e29-44fa-82cc-f0677c2e568c", "Delivery Transport Company");
			this.DeliveryCartageCoGroupBox.Controls.Add(this.DeliveryOrPickupCartageCoAddressControl);
			this.DeliveryCartageCoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DeliveryCartageCoGroupBox.Name = "DeliveryCartageCoGroupBox";
			this.DeliveryCartageCoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 61, true);
			this.DeliveryCartageCoGroupBox.TabIndex = 0;
			this.DeliveryCartageCoGroupBox.TabStop = false;
			// 
			// DeliveryOrPickupCartageCoAddressControl
			// 
			this.DeliveryOrPickupCartageCoAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryOrPickupCartageCoAddressControl, "JE_OA_DeliveryOrPickupCartageCoAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OA_DeliveryOrPickupCartageCoAddr)));
			this.DeliveryOrPickupCartageCoAddressControl.BindToOrgList = "Lookups+CartageList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryOrPickupCartageCoAddressControl, false);
			this.DeliveryOrPickupCartageCoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
			this.DeliveryOrPickupCartageCoAddressControl.Name = "DeliveryOrPickupCartageCoAddressControl";
			this.DeliveryOrPickupCartageCoAddressControl.PopupCaption = "";
			this.DeliveryOrPickupCartageCoAddressControl.ReadOnly = false;
			this.DeliveryOrPickupCartageCoAddressControl.ShowAddress = false;
			this.DeliveryOrPickupCartageCoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 41, true);
			this.DeliveryOrPickupCartageCoAddressControl.StackControls = true;
			this.DeliveryOrPickupCartageCoAddressControl.TabIndex = 0;
			// 
			// DeliveryDocAddressControl
			// 
			this.DeliveryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryDocAddressControl, "ImporterDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ImporterDeliveryAddress)));
			this.DeliveryDocAddressControl.BindToOrganisations = "Lookups+ImportersList";
			this.DeliveryDocAddressControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fdd14a4b-6aaf-4d86-8b56-0268b6705601", "Delivery Address");
			this.DeliveryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 67, true);
			this.DeliveryDocAddressControl.Name = "DeliveryDocAddressControl";
			this.DeliveryDocAddressControl.ReadOnly = false;
			this.DeliveryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DeliveryDocAddressControl.TabIndex = 1;
			this.DeliveryDocAddressControl.ValidationJustForced = false;
			// 
			// DeliveryPartiesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryDocAddressControl);
			this.Controls.Add(this.DeliveryCartageCoGroupBox);
			this.Name = "DeliveryPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 260, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryCartageCoGroupBox.ResumeLayout(false);
			this.DeliveryCartageCoGroupBox.PerformLayout();
			this.DeliveryOrPickupCartageCoAddressControl.ResumeLayout(true);
			this.DeliveryOrPickupCartageCoAddressControl.PerformLayout();
			this.DeliveryDocAddressControl.ResumeLayout(true);
			this.DeliveryDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.GUI.ZGroupBox DeliveryCartageCoGroupBox;
		ZArchitecture.GUI.ZAddressControl DeliveryOrPickupCartageCoAddressControl;
		MasterFiles.GUI.ZDocAddressControl DeliveryDocAddressControl;
	}
}
