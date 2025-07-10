using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentPiecesDetailForm : ZChildForm
	{
		readonly CommonShipment Shipment;

		InnerPackLinesGrid PackLinesGrid;
		ZGroupBox PackageDetailsGroupBox;
		ZButton SaveButton;
		ZArchitecture.ZCalcEdit TotalPackLinesPackagesCalcEdit;
		ZArchitecture.ZCalcEdit TotalPackLineWeightCalcEdit;
		ZArchitecture.ZCalcEdit TotalPackLineVolumeCalcEdit;
		ZArchitecture.ZCalcEdit JS_TotalPackageCountBoundCalcEdit;
		ZCalcDropEdit JS_TotalPackageCountBoundCalcDropEdit;
		ZArchitecture.ZCalcEdit JS_ActualVolumeCalcEdit;
		ZArchitecture.ZCalcEdit JS_ActualWeightCalcEdit;
		ZArchitecture.ZLabel TotalVolumeUnitLabel;
		ZArchitecture.ZLabel ShipmentVolumeUnitLabel;
		ZArchitecture.ZLabel ShipmentWeightUnitLabel;
		ZArchitecture.ZLabel TotalWeightUnitLabel;

		protected new void InitializeComponent()
		{
			this.PackLinesGrid = new InnerPackLinesGrid();
			this.PackageDetailsGroupBox = new ZGroupBox();
			this.ShipmentWeightUnitLabel = new ZArchitecture.ZLabel();
			this.TotalWeightUnitLabel = new ZArchitecture.ZLabel();
			this.ShipmentVolumeUnitLabel = new ZArchitecture.ZLabel();
			this.TotalVolumeUnitLabel = new ZArchitecture.ZLabel();
			this.JS_ActualVolumeCalcEdit = new ZArchitecture.ZCalcEdit();
			this.JS_ActualWeightCalcEdit = new ZArchitecture.ZCalcEdit();
			this.JS_TotalPackageCountBoundCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TotalPackLineVolumeCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TotalPackLineWeightCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TotalPackLinesPackagesCalcEdit = new ZArchitecture.ZCalcEdit();
			this.SaveButton = new ZButton();
			this.JS_TotalPackageCountBoundCalcDropEdit = new ZCalcDropEdit();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.PackLinesGrid)).BeginInit();
			this.PackageDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 341, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ForwardingShipment);
			// 
			// PackLinesGrid
			// 
			this.PackLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackLinesGrid, "InnerPackLines");
			this.PackLinesGrid.GridId = "98e1af6c-bd01-4114-ba3b-ad0bc4dfd39f";
			this.PackLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackLinesGrid.LayoutKey = "PackLinesGrid";
			this.PackLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.PackLinesGrid.Name = "PackLinesGrid";
			this.PackLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 164, true);
			this.PackLinesGrid.TabIndex = 6;
			// 
			// PackageDetailsGroupBox
			// 
			this.PackageDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|f693831a-3ca4-4d29-8a1e-19cb24a57f44", "Packaging Details");
			this.PackageDetailsGroupBox.Controls.Add(this.ShipmentWeightUnitLabel);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalWeightUnitLabel);
			this.PackageDetailsGroupBox.Controls.Add(this.ShipmentVolumeUnitLabel);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalVolumeUnitLabel);
			this.PackageDetailsGroupBox.Controls.Add(this.JS_ActualVolumeCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.JS_ActualWeightCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.JS_TotalPackageCountBoundCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalPackLineVolumeCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalPackLineWeightCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalPackLinesPackagesCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.PackLinesGrid);
			this.PackageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.PackageDetailsGroupBox.Name = "PackageDetailsGroupBox";
			this.PackageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 245, true);
			this.PackageDetailsGroupBox.TabIndex = 7;
			this.PackageDetailsGroupBox.TabStop = false;
			// 
			// ShipmentWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.ShipmentWeightUnitLabel, "TotalPackLineWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ForwardingShipment)(null)).TotalPackLineWeightUnit)));
			this.ShipmentWeightUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|cda3fb5d-a926-4cbe-b051-daa347a1560b", "KG");
			this.ShipmentWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 218, true);
			this.ShipmentWeightUnitLabel.Name = "ShipmentWeightUnitLabel";
			this.ShipmentWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.ShipmentWeightUnitLabel.TabIndex = 28;
			// 
			// TotalWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalWeightUnitLabel, "TotalPackLineWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ForwardingShipment)(null)).TotalPackLineWeightUnit)));
			this.TotalWeightUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|caac3a94-fb7a-4d85-8cdd-9b73e743c806", "KG");
			this.TotalWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 196, true);
			this.TotalWeightUnitLabel.Name = "TotalWeightUnitLabel";
			this.TotalWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.TotalWeightUnitLabel.TabIndex = 27;
			// 
			// ShipmentVolumeUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.ShipmentVolumeUnitLabel, "TotalPackLineVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ForwardingShipment)(null)).TotalPackLineVolumeUnit)));
			this.ShipmentVolumeUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|859e43de-7d94-470e-bf9e-532e007c1360", "M3");
			this.ShipmentVolumeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 218, true);
			this.ShipmentVolumeUnitLabel.Name = "ShipmentVolumeUnitLabel";
			this.ShipmentVolumeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.ShipmentVolumeUnitLabel.TabIndex = 26;
			// 
			// TotalVolumeUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalVolumeUnitLabel, "TotalPackLineVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ForwardingShipment)(null)).TotalPackLineVolumeUnit)));
			this.TotalVolumeUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|a9f14caf-310a-46e0-a1c6-6f7ec06df81e", "M3");
			this.TotalVolumeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 196, true);
			this.TotalVolumeUnitLabel.Name = "TotalVolumeUnitLabel";
			this.TotalVolumeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.TotalVolumeUnitLabel.TabIndex = 25;
			// 
			// JS_ActualVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_ActualVolumeCalcEdit, "JS_ActualVolumeReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ForwardingShipment)(null)).JS_ActualVolumeReadOnly)));
			this.JS_ActualVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|205c13f8-7626-4d07-8d1a-4dd99672e28c", "Shipment Volume");
			this.JS_ActualVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 215, true);
			this.JS_ActualVolumeCalcEdit.Name = "JS_ActualVolumeCalcEdit";
			this.JS_ActualVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.JS_ActualVolumeCalcEdit.TabIndex = 22;
			this.JS_ActualVolumeCalcEdit.Text = "0.000";
			this.JS_ActualVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JS_ActualWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_ActualWeightCalcEdit, "JS_ActualWeightReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ForwardingShipment)(null)).JS_ActualWeightReadOnly)));
			this.JS_ActualWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|5048ac5c-8c91-4837-a2f1-151bc46647a9", "Shipment Weight");
			this.JS_ActualWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 215, true);
			this.JS_ActualWeightCalcEdit.Name = "JS_ActualWeightCalcEdit";
			this.JS_ActualWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.JS_ActualWeightCalcEdit.TabIndex = 21;
			this.JS_ActualWeightCalcEdit.Text = "0.000";
			this.JS_ActualWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JS_TotalPackageCountBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_TotalPackageCountBoundCalcEdit, "JS_InnerPacks_ReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ForwardingShipment)(null)).JS_InnerPacks_ReadOnly)));
			this.JS_TotalPackageCountBoundCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|e9a2627d-da62-46b8-a705-b27aef4fe2d1", "Shipment Inner Packs");
			this.JS_TotalPackageCountBoundCalcEdit.Decimals = 0;
			this.JS_TotalPackageCountBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 215, true);
			this.JS_TotalPackageCountBoundCalcEdit.Name = "JS_TotalPackageCountBoundCalcEdit";
			this.JS_TotalPackageCountBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.JS_TotalPackageCountBoundCalcEdit.TabIndex = 18;
			this.JS_TotalPackageCountBoundCalcEdit.Text = "0";
			this.JS_TotalPackageCountBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPackLineVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackLineVolumeCalcEdit, "TotalInnerPackLineVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ForwardingShipment)(null)).TotalInnerPackLineVolume)));
			this.TotalPackLineVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|aa000988-0595-429c-8f68-c7bd7db3b3d9", "Total Volume");
			this.TotalPackLineVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 193, true);
			this.TotalPackLineVolumeCalcEdit.Name = "TotalPackLineVolumeCalcEdit";
			this.TotalPackLineVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalPackLineVolumeCalcEdit.TabIndex = 13;
			this.TotalPackLineVolumeCalcEdit.Text = "0.000";
			this.TotalPackLineVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPackLineWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackLineWeightCalcEdit, "TotalInnerPackLineWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ForwardingShipment)(null)).TotalInnerPackLineWeight)));
			this.TotalPackLineWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|6186d5f2-6e76-4069-bc7d-92b34db4b84b", "Total Weight");
			this.TotalPackLineWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 193, true);
			this.TotalPackLineWeightCalcEdit.Name = "TotalPackLineWeightCalcEdit";
			this.TotalPackLineWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalPackLineWeightCalcEdit.TabIndex = 12;
			this.TotalPackLineWeightCalcEdit.Text = "0.000";
			this.TotalPackLineWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPackLinesPackagesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackLinesPackagesCalcEdit, "TotalInnerPackLinePackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ForwardingShipment)(null)).TotalInnerPackLinePackages)));
			this.TotalPackLinesPackagesCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|08eef136-bdbc-4cc3-9e1c-a7020c4a2dc9", "Total Inner Packs");
			this.TotalPackLinesPackagesCalcEdit.Decimals = 0;
			this.TotalPackLinesPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 193, true);
			this.TotalPackLinesPackagesCalcEdit.Name = "TotalPackLinesPackagesCalcEdit";
			this.TotalPackLinesPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalPackLinesPackagesCalcEdit.TabIndex = 8;
			this.TotalPackLinesPackagesCalcEdit.Text = "0";
			this.TotalPackLinesPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentPiecesDetailForm|cd1cdcef-cfa8-4a74-81c3-e98ce1af2a53", "OK", "OK", "OK", "Accept Changes.");
			this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 314, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 11;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// JS_TotalPackageCountBoundCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_TotalPackageCountBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ForwardingShipment)(null)).JS_TotalPackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ForwardingShipment)(null)).JS_F3_NKTotalCountPackType)));
			this.JS_TotalPackageCountBoundCalcDropEdit.BindToAmount = "JS_TotalPackageCount";
			this.JS_TotalPackageCountBoundCalcDropEdit.BindToUnit = "JS_F3_NKTotalCountPackType";
			this.JS_TotalPackageCountBoundCalcDropEdit.Decimals = 0;
			this.JS_TotalPackageCountBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 7, true);
			this.JS_TotalPackageCountBoundCalcDropEdit.Name = "JS_TotalPackageCountBoundCalcDropEdit";
			this.JS_TotalPackageCountBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JS_TotalPackageCountBoundCalcDropEdit.TabIndex = 4;
			this.JS_TotalPackageCountBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ShipmentPiecesDetailForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 363, true);
			this.Controls.Add(this.JS_TotalPackageCountBoundCalcDropEdit);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.PackageDetailsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Business.ForwardingShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ForwardingShipment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ShipmentPiecesDetailForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PackageDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.JS_TotalPackageCountBoundCalcDropEdit, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.PackLinesGrid)).EndInit();
			this.PackageDetailsGroupBox.ResumeLayout(false);
			this.PackageDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
