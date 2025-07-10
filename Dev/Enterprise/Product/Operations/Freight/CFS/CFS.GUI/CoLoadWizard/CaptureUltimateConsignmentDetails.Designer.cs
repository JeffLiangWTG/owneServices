using System.Windows.Forms;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	/// <summary>
	/// Summary description for UserControl1.
	/// </summary>
	public partial class CaptureUltimateConsignmentDetails : WizardPage
	{
		private ZArchitecture.ZTextBox ShipmentHouseBillTextBox;
		private MasterFiles.GUI.ZOrganisationFindBox zOrganisationFindBox1;
		private ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		private ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		private ZArchitecture.ZTextBox MarksAndNumbersTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit NumberOfPackagesCalcDropEdit;
		private ZArchitecture.GUI.ZCheckBox EnterConsignorDetailsCheckBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ShipmentHouseBillTextBox = new ZArchitecture.ZTextBox();
			this.zOrganisationFindBox1 = new MasterFiles.GUI.ZOrganisationFindBox();
			this.GoodsDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.MarksAndNumbersTextBox = new ZArchitecture.ZTextBox();
			this.VolumeCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.NumberOfPackagesCalcDropEdit = new ZArchitecture.GUI.ZCalcDropEdit();
			this.EnterConsignorDetailsCheckBox = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CoLoadWizardShipment);
			// 
			// ShipmentHouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentHouseBillTextBox, "CW_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_HouseBill)));
			this.ShipmentHouseBillTextBox.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|bd6300b4-c30d-4623-8308-d436efc0dfb6", "House bill");
			this.ShipmentHouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 56, true);
			this.ShipmentHouseBillTextBox.Name = "ShipmentHouseBillTextBox";
			this.ShipmentHouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ShipmentHouseBillTextBox.TabIndex = 3;
			// 
			// zOrganisationFindBox1
			// 
			this.BindingSource.SetBindingMember(this.zOrganisationFindBox1, "CW_OH_CoLoadForwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CoLoadWizardShipment)(null)).CW_OH_CoLoadForwarder)));
			this.zOrganisationFindBox1.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|41951001-da4f-4824-b88d-2b051054f6c7", "Co-Loader");
			this.zOrganisationFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 8, true);
			this.zOrganisationFindBox1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(ZArchitecture.Modules.ModuleIDs.Organisation));
			this.zOrganisationFindBox1.Name = "zOrganisationFindBox1";
			this.zOrganisationFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zOrganisationFindBox1.TabIndex = 1;
			// 
			// GoodsDescriptionTextBox
			// 
			this.GoodsDescriptionTextBox.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CW_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|68fb7c39-3887-4643-9512-f75df47fce1b", "Goods Description");
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 184, true);
			this.GoodsDescriptionTextBox.Multiline = true;
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.ScrollBars = ScrollBars.Vertical;
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 64, true);
			this.GoodsDescriptionTextBox.TabIndex = 11;
			// 
			// MarksAndNumbersTextBox
			// 
			this.MarksAndNumbersTextBox.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "CW_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|9007d0a4-9590-452b-a154-3cf63221522c", "Marks & Numbers");
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 256, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.ScrollBars = ScrollBars.Vertical;
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 72, true);
			this.MarksAndNumbersTextBox.TabIndex = 13;
			// 
			// VolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CoLoadWizardShipment)(null)).CW_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_VolumeUQ)));
			this.VolumeCalcDropEdit.BindToAmount = "CW_Volume";
			this.VolumeCalcDropEdit.BindToUnit = "CW_VolumeUQ";
			this.VolumeCalcDropEdit.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|f91cb2fc-73ce-4965-b30e-b4316ed20a3b", "Volume");
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 152, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 9;
			// 
			// WeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CoLoadWizardShipment)(null)).CW_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_WeightUQ)));
			this.WeightCalcDropEdit.BindToAmount = "CW_Weight";
			this.WeightCalcDropEdit.BindToUnit = "CW_WeightUQ";
			this.WeightCalcDropEdit.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|b1273383-485d-4e46-adf4-ad19b5f490da", "Weight");
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 120, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.WeightCalcDropEdit.TabIndex = 7;
			// 
			// NumberOfPackagesCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfPackagesCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CoLoadWizardShipment)(null)).CW_NumberOfPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CoLoadWizardShipment)(null)).CW_NumberOfPackagesUQ)));
			this.NumberOfPackagesCalcDropEdit.BindToAmount = "CW_NumberOfPackages";
			this.NumberOfPackagesCalcDropEdit.BindToUnit = "CW_NumberOfPackagesUQ";
			this.NumberOfPackagesCalcDropEdit.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|6d96f1f7-1421-4d32-a241-978cfcf73ea6", "No. Packages");
			this.NumberOfPackagesCalcDropEdit.Decimals = 0;
			this.NumberOfPackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 88, true);
			this.NumberOfPackagesCalcDropEdit.Name = "NumberOfPackagesCalcDropEdit";
			this.NumberOfPackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.NumberOfPackagesCalcDropEdit.TabIndex = 5;
			// 
			// EnterConsignorDetailsCheckBox
			// 
			this.EnterConsignorDetailsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnterConsignorDetailsCheckBox, "CW_EnterConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.CoLoadWizardShipment)(null)).CW_EnterConsignor)));
			this.EnterConsignorDetailsCheckBox.CaptionResourceString = Res.GetData("CaptureUltimateConsignmentDetails|34132d0a-bbd3-4a2b-bd48-ba01349a0024", "Enter Consignor Details");
			this.EnterConsignorDetailsCheckBox.FlatStyle = FlatStyle.System;
			this.EnterConsignorDetailsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 336, true);
			this.EnterConsignorDetailsCheckBox.Name = "EnterConsignorDetailsCheckBox";
			this.EnterConsignorDetailsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.EnterConsignorDetailsCheckBox.TabIndex = 14;
			// 
			// CaptureUltimateConsignmentDetails
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EnterConsignorDetailsCheckBox);
			this.Controls.Add(this.NumberOfPackagesCalcDropEdit);
			this.Controls.Add(this.WeightCalcDropEdit);
			this.Controls.Add(this.VolumeCalcDropEdit);
			this.Controls.Add(this.MarksAndNumbersTextBox);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.zOrganisationFindBox1);
			this.Controls.Add(this.ShipmentHouseBillTextBox);
			this.Name = "CaptureUltimateConsignmentDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 360, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
