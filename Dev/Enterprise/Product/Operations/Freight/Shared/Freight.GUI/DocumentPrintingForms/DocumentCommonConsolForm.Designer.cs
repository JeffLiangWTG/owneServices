namespace Enterprise.Freight.GUI
{
	public partial class DocumentCommonConsolForm
	{

		#region Component Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		Enterprise.ZArchitecture.GUI.ZRadioButton AllShipmentsRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton UnpackedRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton PackedRadioButton;
		Enterprise.ZArchitecture.GUI.ZCheckBox IncludeCustomsBrokerCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IncludeConsignorCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IncludeConsigneeCheckBox;
		System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.AllShipmentsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.UnpackedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PackedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.IncludeCustomsBrokerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncludeConsignorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncludeConsigneeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OptionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Controls.Add(this.AllShipmentsRadioButton);
			this.OptionsGroupBox.Controls.Add(this.UnpackedRadioButton);
			this.OptionsGroupBox.Controls.Add(this.PackedRadioButton);
			this.OptionsGroupBox.Controls.Add(this.IncludeCustomsBrokerCheckBox);
			this.OptionsGroupBox.Controls.Add(this.IncludeConsignorCheckBox);
			this.OptionsGroupBox.Controls.Add(this.IncludeConsigneeCheckBox);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 344, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.DocumentCommonConsol);
			// 
			// AllShipmentsRadioButton
			// 
			this.AllShipmentsRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.AllShipmentsRadioButton, "IncludeAllShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentCommonConsol)(null)).IncludeAllShipments)));
			this.AllShipmentsRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentCommonConsolForm|7789d54e-a220-404b-add6-cbb5a17798b5", "All Shipments");
			this.AllShipmentsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllShipmentsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 96, true);
			this.AllShipmentsRadioButton.Name = "AllShipmentsRadioButton";
			this.AllShipmentsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 26, true);
			this.AllShipmentsRadioButton.TabIndex = 12;
			this.AllShipmentsRadioButton.Visible = false;
			// 
			// UnpackedRadioButton
			// 
			this.UnpackedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.UnpackedRadioButton, "IncludeUnPacked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentCommonConsol)(null)).IncludeUnPacked)));
			this.UnpackedRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentCommonConsolForm|055ec083-52f7-461e-9eb2-b266083a70ea", "Only Unpacked");
			this.UnpackedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnpackedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 144, true);
			this.UnpackedRadioButton.Name = "UnpackedRadioButton";
			this.UnpackedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 26, true);
			this.UnpackedRadioButton.TabIndex = 11;
			this.UnpackedRadioButton.Visible = false;
			// 
			// PackedRadioButton
			// 
			this.PackedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.PackedRadioButton, "IncludePacked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentCommonConsol)(null)).IncludePacked)));
			this.PackedRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentCommonConsolForm|ee76743a-f816-4e9a-9587-d87fca377112", "Only Packed");
			this.PackedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PackedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 120, true);
			this.PackedRadioButton.Name = "PackedRadioButton";
			this.PackedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 26, true);
			this.PackedRadioButton.TabIndex = 10;
			this.PackedRadioButton.Visible = false;
			// 
			// IncludeCustomsBrokerCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludeCustomsBrokerCheckBox, "IncludeCustomsBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentCommonConsol)(null)).IncludeCustomsBroker)));
			this.IncludeCustomsBrokerCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentCommonConsolForm|dc57e6fa-e939-466d-b1be-7cdbb46033ed", "Customs Broker");
			this.IncludeCustomsBrokerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeCustomsBrokerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 72, true);
			this.IncludeCustomsBrokerCheckBox.Name = "IncludeCustomsBrokerCheckBox";
			this.IncludeCustomsBrokerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 24, true);
			this.IncludeCustomsBrokerCheckBox.TabIndex = 9;
			this.IncludeCustomsBrokerCheckBox.Visible = false;
			// 
			// IncludeConsignorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludeConsignorCheckBox, "IncludeConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentCommonConsol)(null)).IncludeConsignor)));
			this.IncludeConsignorCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentCommonConsolForm|2c10c586-a573-4378-b73a-0357a68d7cb9", "Consignor");
			this.IncludeConsignorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeConsignorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.IncludeConsignorCheckBox.Name = "IncludeConsignorCheckBox";
			this.IncludeConsignorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 24, true);
			this.IncludeConsignorCheckBox.TabIndex = 7;
			this.IncludeConsignorCheckBox.Visible = false;
			// 
			// IncludeConsigneeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludeConsigneeCheckBox, "IncludeConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentCommonConsol)(null)).IncludeConsignee)));
			this.IncludeConsigneeCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentCommonConsolForm|588d2fa1-2ffa-4527-988c-1f52843c65ca", "Consignee");
			this.IncludeConsigneeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.IncludeConsigneeCheckBox.Name = "IncludeConsigneeCheckBox";
			this.IncludeConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 24, true);
			this.IncludeConsigneeCheckBox.TabIndex = 8;
			this.IncludeConsigneeCheckBox.Visible = false;
			// 
			// DocumentCommonConsolForm
			// 
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentCommonConsolForm|9bead88e-c48d-4ed5-b2dd-878b870013f9", "Include in Document");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 368, true);
			this.DataSourceType = typeof(Enterprise.Freight.Business.DocumentCommonConsol);
			this.Name = "DocumentCommonConsolForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			this.OptionsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
