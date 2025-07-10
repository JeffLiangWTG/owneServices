using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ConfirmCreateSubHouseWizardPage : WizardPage
	{
		private MasterFiles.GUI.ZOrganisationFindBox zOrganisationFindBox2;
		private ZArchitecture.ZTextBox ShipmentHouseBillTextBox;
		private ZArchitecture.ZLabel InformationLabel;
		private ZCalcDropEdit WeightCalcDropEdit;
		private ZCalcDropEdit VolumeCalcDropEdit;
		private ZArchitecture.ZTextBox MarksAndNumbersTextBox;
		private ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		private MasterFiles.GUI.ZOrganisationFindBox ConsigneeOrganisationFindBox;
		private MasterFiles.GUI.ZOrganisationFindBox CoLoadForwarderOrganisationFindBox;
		private ZCalcDropEdit NumberOfPackagesCalcDropEdit;
		private Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ConsigneeOrganisationFindBox = new MasterFiles.GUI.ZOrganisationFindBox();
			this.zOrganisationFindBox2 = new MasterFiles.GUI.ZOrganisationFindBox();
			this.CoLoadForwarderOrganisationFindBox = new MasterFiles.GUI.ZOrganisationFindBox();
			this.ShipmentHouseBillTextBox = new ZArchitecture.ZTextBox();
			this.InformationLabel = new ZArchitecture.ZLabel();
			this.WeightCalcDropEdit = new ZCalcDropEdit();
			this.VolumeCalcDropEdit = new ZCalcDropEdit();
			this.MarksAndNumbersTextBox = new ZArchitecture.ZTextBox();
			this.GoodsDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.NumberOfPackagesCalcDropEdit = new ZCalcDropEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CoLoadWizardShipment);
			// 
			// ConsigneeOrganisationFindBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeOrganisationFindBox, "CW_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CoLoadWizardShipment)(null)).CW_OH_Consignee)));
			this.ConsigneeOrganisationFindBox.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|50e60501-ba74-4505-9bd0-f7a1bc276f93", "Consignee");
			this.ConsigneeOrganisationFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(152, 120, true);
			this.ConsigneeOrganisationFindBox.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ConsigneeOrganisationFindBox.Name = "ConsigneeOrganisationFindBox";
			this.ConsigneeOrganisationFindBox.Size = ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ConsigneeOrganisationFindBox.TabIndex = 1;
			// 
			// zOrganisationFindBox2
			// 
			this.BindingSource.SetBindingMember(this.zOrganisationFindBox2, "CW_OH_Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CoLoadWizardShipment)(null)).CW_OH_Consignor)));
			this.zOrganisationFindBox2.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|19cb4fe5-0d6d-44ee-9f5d-4fc1dc6beb64", "Consignor");
			this.zOrganisationFindBox2.Location = ControlDpiScalingHelper.NewScaledPoint(152, 144, true);
			this.zOrganisationFindBox2.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(ZArchitecture.Modules.ModuleIDs.Organisation));
			this.zOrganisationFindBox2.Name = "zOrganisationFindBox2";
			this.zOrganisationFindBox2.Size = ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zOrganisationFindBox2.TabIndex = 2;
			// 
			// CoLoadForwarderOrganisationFindBox
			// 
			this.BindingSource.SetBindingMember(this.CoLoadForwarderOrganisationFindBox, "CW_OH_CoLoadForwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CoLoadWizardShipment)(null)).CW_OH_CoLoadForwarder)));
			this.CoLoadForwarderOrganisationFindBox.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|c325bd33-f100-42d9-8fc9-725cbdae7df7", "Co-Loader");
			this.CoLoadForwarderOrganisationFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(152, 80, true);
			this.CoLoadForwarderOrganisationFindBox.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CoLoadForwarderOrganisationFindBox.Name = "CoLoadForwarderOrganisationFindBox";
			this.CoLoadForwarderOrganisationFindBox.Size = ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CoLoadForwarderOrganisationFindBox.TabIndex = 0;
			// 
			// ShipmentHouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentHouseBillTextBox, "CW_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_HouseBill)));
			this.ShipmentHouseBillTextBox.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|eb001f74-b872-4da1-9e4f-3f1a79e48240", "House bill");
			this.ShipmentHouseBillTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(152, 168, true);
			this.ShipmentHouseBillTextBox.Name = "ShipmentHouseBillTextBox";
			this.ShipmentHouseBillTextBox.Size = ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ShipmentHouseBillTextBox.TabIndex = 3;
			// 
			// InformationLabel
			// 
			this.InformationLabel.Location = ControlDpiScalingHelper.NewScaledPoint(24, 8, true);
			this.InformationLabel.Name = "InformationLabel";
			this.InformationLabel.Size = ControlDpiScalingHelper.NewScaledSize(376, 64, true);
			this.InformationLabel.TabIndex = 14;
			this.InformationLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// WeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CoLoadWizardShipment)(null)).CW_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_WeightUQ)));
			this.WeightCalcDropEdit.BindToAmount = "CW_Weight";
			this.WeightCalcDropEdit.BindToUnit = "CW_WeightUQ";
			this.WeightCalcDropEdit.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|5002ac5a-7d2b-4879-9dfb-30879097fdc2", "Weight");
			this.WeightCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(152, 216, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.WeightCalcDropEdit.TabIndex = 5;
			// 
			// VolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CoLoadWizardShipment)(null)).CW_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_VolumeUQ)));
			this.VolumeCalcDropEdit.BindToAmount = "CW_Volume";
			this.VolumeCalcDropEdit.BindToUnit = "CW_VolumeUQ";
			this.VolumeCalcDropEdit.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|24533bd0-6188-462e-8795-6f9af8c1aea9", "Volume");
			this.VolumeCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(152, 240, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 6;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "CW_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|992407fa-6021-417c-9a55-ea0188d23409", "Marks & Numbers");
			this.MarksAndNumbersTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(152, 288, true);
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.MarksAndNumbersTextBox.TabIndex = 8;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CW_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|70504f92-8a65-48c0-adb0-1e7cdf083e21", "Goods Description");
			this.GoodsDescriptionTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(152, 264, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 7;
			// 
			// NumberOfPackagesCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfPackagesCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CoLoadWizardShipment)(null)).CW_NumberOfPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_NumberOfPackagesUQ)));
			this.NumberOfPackagesCalcDropEdit.BindToAmount = "CW_NumberOfPackages";
			this.NumberOfPackagesCalcDropEdit.BindToUnit = "CW_NumberOfPackagesUQ";
			this.NumberOfPackagesCalcDropEdit.CaptionResourceString = Res.GetData("ConfirmCreateSubHouseWizardPage|74791794-b7dd-4129-a9ae-0f89926e37dc", "No. Packages");
			this.NumberOfPackagesCalcDropEdit.Decimals = 0;
			this.NumberOfPackagesCalcDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(152, 192, true);
			this.NumberOfPackagesCalcDropEdit.Name = "NumberOfPackagesCalcDropEdit";
			this.NumberOfPackagesCalcDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.NumberOfPackagesCalcDropEdit.TabIndex = 4;
			// 
			// ConfirmCreateSubHouseWizardPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberOfPackagesCalcDropEdit);
			this.Controls.Add(this.MarksAndNumbersTextBox);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.WeightCalcDropEdit);
			this.Controls.Add(this.VolumeCalcDropEdit);
			this.Controls.Add(this.InformationLabel);
			this.Controls.Add(this.CoLoadForwarderOrganisationFindBox);
			this.Controls.Add(this.ShipmentHouseBillTextBox);
			this.Controls.Add(this.zOrganisationFindBox2);
			this.Controls.Add(this.ConsigneeOrganisationFindBox);
			this.Name = "ConfirmCreateSubHouseWizardPage";
			this.Size = ControlDpiScalingHelper.NewScaledSize(424, 360, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
