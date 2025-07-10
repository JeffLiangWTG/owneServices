using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickHeaderReleaseUserControl
	{
		ZGroupBox PickInfoGroupBox;
		ZTextBox PickNoTextBox;
		ZTextBox PickStatusTextBox;
		ZGuidFindBox WarehouseGuidFindBox;
		ZCheckBox IsAwaitingReplenishmentCheckBox;

		ZCalcEdit PickPriorityCalcEdit;
		ZGuidFindBox DockDoorGuidFindBox;
		ZGuidFindBox PackingStationGuidFindBox;
		ZGuidFindBox PickAreaOverride;
		protected ZButton DisplayAwaitingReplenishmentPicksButton;

		void InitializeComponent()
		{
			this.PickInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PickAreaOverride = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DockDoorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PackingStationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PickPriorityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WarehouseGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PickStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PickNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsAwaitingReplenishmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DisplayAwaitingReplenishmentPicksButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickInfoGroupBox.SuspendLayout();
			this.PickAreaOverride.SuspendLayout();
			this.DockDoorGuidFindBox.SuspendLayout();
			this.PackingStationGuidFindBox.SuspendLayout();
			this.WarehouseGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsPick);
			// 
			// PickInfoGroupBox
			//
			this.PickInfoGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickHeaderUserControl|908df9d1-153f-4a84-a1cf-d97a10c51d61", "Pick Details");
			this.PickInfoGroupBox.Controls.Add(this.PickAreaOverride);
			this.PickInfoGroupBox.Controls.Add(this.DockDoorGuidFindBox);
			this.PickInfoGroupBox.Controls.Add(this.PackingStationGuidFindBox);
			this.PickInfoGroupBox.Controls.Add(this.PickPriorityCalcEdit);
			this.PickInfoGroupBox.Controls.Add(this.WarehouseGuidFindBox);
			this.PickInfoGroupBox.Controls.Add(this.PickStatusTextBox);
			this.PickInfoGroupBox.Controls.Add(this.PickNoTextBox);
			this.PickInfoGroupBox.Controls.Add(this.IsAwaitingReplenishmentCheckBox);
			this.PickInfoGroupBox.Controls.Add(this.DisplayAwaitingReplenishmentPicksButton);
			this.PickInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickInfoGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 72, true);
			this.PickInfoGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 72, true);
			this.PickInfoGroupBox.Name = "PickInfoGroupBox";
			this.PickInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 72, true);
			this.PickInfoGroupBox.TabIndex = 0;
			this.PickInfoGroupBox.TabStop = false;
			// 
			// PickAreaOverride
			// 
			this.PickAreaOverride.AllowDrop = true;
			this.PickAreaOverride.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.PickAreaOverride, "WP_WA_DynamicPickAreaOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).WP_WA_DynamicPickAreaOverride)));
			this.PickAreaOverride.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 42, true);
			this.PickAreaOverride.Name = "PickAreaOverride";
			this.PickAreaOverride.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PickAreaOverride.ParentType = null;
			this.PickAreaOverride.PreBoundMaxLength = 23;
			this.PickAreaOverride.ShowDescriptionBox = false;
			this.PickAreaOverride.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.PickAreaOverride.TabIndex = 7;
			// 
			// DockDoorGuidFindBox
			// 
			this.DockDoorGuidFindBox.AllowDrop = true;
			this.DockDoorGuidFindBox.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.DockDoorGuidFindBox, "DockDoorPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DockDoorPK)));
			this.DockDoorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(736, 16, true);
			this.DockDoorGuidFindBox.Name = "DockDoorGuidFindBox";
			this.DockDoorGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DockDoorGuidFindBox.ParentType = null;
			this.DockDoorGuidFindBox.PreBoundMaxLength = 23;
			this.DockDoorGuidFindBox.ShowDescriptionBox = false;
			this.DockDoorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.DockDoorGuidFindBox.TabIndex = 8;
			// 
			// PackingStationGuidFindBox
			// 
			this.PackingStationGuidFindBox.AllowDrop = true;
			this.PackingStationGuidFindBox.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.PackingStationGuidFindBox, "WP_WL_PackingStation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).WP_WL_PackingStation)));
			this.PackingStationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(736, 42, true);
			this.PackingStationGuidFindBox.Name = "PackingStationGuidFindBox";
			this.PackingStationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PackingStationGuidFindBox.ParentType = null;
			this.PackingStationGuidFindBox.PreBoundMaxLength = 23;
			this.PackingStationGuidFindBox.ShowDescriptionBox = false;
			this.PackingStationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.PackingStationGuidFindBox.TabIndex = 9;
			// 
			// PickPriorityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PickPriorityCalcEdit, "PickPriority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).PickPriority)));
			this.PickPriorityCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|6F1087A6-4A0A-4BAF-998F-A0F01DEEC504", "Pick Priority");
			this.PickPriorityCalcEdit.DecimalPlaces = 0;
			this.PickPriorityCalcEdit.Decimals = 0;
			this.PickPriorityCalcEdit.IsCalculatorEnabled = false;
			this.PickPriorityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 42, true);
			this.PickPriorityCalcEdit.Name = "PickPriorityCalcEdit";
			this.PickPriorityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.PickPriorityCalcEdit.TabIndex = 5;
			this.PickPriorityCalcEdit.Text = "0";
			this.PickPriorityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PickPriorityCalcEdit.TrackDisposedAccess = true;
			this.PickPriorityCalcEdit.WordWrap = false;
			// 
			// WarehouseGuidFindBox
			// 
			this.WarehouseGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseGuidFindBox, "WP_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).WP_WW_Whs)));
			this.WarehouseGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 16, true);
			this.WarehouseGuidFindBox.Name = "WarehouseGuidFindBox";
			this.WarehouseGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WarehouseGuidFindBox.ParentType = null;
			this.WarehouseGuidFindBox.PreBoundMaxLength = 3;
			this.WarehouseGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.WarehouseGuidFindBox.TabIndex = 6;
			// 
			// PickStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.PickStatusTextBox, "StatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).StatusDesc)));
			this.PickStatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickHeaderUserControl|751bc522-0717-4da6-90b8-e8081228aa60", "Status");
			this.PickStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 16, true);
			this.PickStatusTextBox.Name = "PickStatusTextBox";
			this.PickStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.PickStatusTextBox.TabIndex = 4;
			// 
			// PickNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PickNoTextBox, "WP_PickNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).WP_PickNo)));
			this.PickNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 16, true);
			this.PickNoTextBox.Name = "PickNoTextBox";
			this.PickNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.PickNoTextBox.TabIndex = 1;
			// 
			// IsAwaitingReplenishmentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsAwaitingReplenishmentCheckBox, "WP_IsAwaitingReplenishment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).WP_IsAwaitingReplenishment)));
			this.IsAwaitingReplenishmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.IsAwaitingReplenishmentCheckBox.Name = "IsAwaitingReplenishmentCheckBox";
			this.IsAwaitingReplenishmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 16, true);
			this.IsAwaitingReplenishmentCheckBox.TabIndex = 2;
			this.IsAwaitingReplenishmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// DisplayAwaitingReplenishmentPicksButton
			// 
			this.DisplayAwaitingReplenishmentPicksButton.IsCaptionOverridden = true;
			this.DisplayAwaitingReplenishmentPicksButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 42, true);
			this.DisplayAwaitingReplenishmentPicksButton.Name = "DisplayAwaitingReplenishmentPicksButton";
			this.DisplayAwaitingReplenishmentPicksButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.DisplayAwaitingReplenishmentPicksButton.TabIndex = 3;
			this.DisplayAwaitingReplenishmentPicksButton.Text = "...";
			this.DisplayAwaitingReplenishmentPicksButton.ToolTipCaption = null;
			this.DisplayAwaitingReplenishmentPicksButton.Click += new System.EventHandler(this.DisplayAwaitingReplenishmentPicksButton_Click);
			// 
			// PickHeaderReleaseUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickInfoGroupBox);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 72, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 72, true);
			this.Name = "PickHeaderReleaseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 72, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickInfoGroupBox.ResumeLayout(false);
			this.PickInfoGroupBox.PerformLayout();
			this.PickAreaOverride.ResumeLayout(true);
			this.PickAreaOverride.PerformLayout();
			this.DockDoorGuidFindBox.ResumeLayout(true);
			this.DockDoorGuidFindBox.PerformLayout();
			this.PackingStationGuidFindBox.ResumeLayout(true);
			this.PackingStationGuidFindBox.PerformLayout();
			this.WarehouseGuidFindBox.ResumeLayout(true);
			this.WarehouseGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
