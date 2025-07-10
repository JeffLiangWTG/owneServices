using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class InnerPackLinesOfOuterPackLineForm : ZChildForm
	{
		InnerPackLinesGrid InnerPackLinesGrid;
		ZGroupBox PackageDetailsGroupBox;
		ZArchitecture.ZCalcEdit TotalInnerPacksCalcEdit;
		ZArchitecture.ZCalcEdit TotalPackLineWeightCalcEdit;
		ZArchitecture.ZCalcEdit TotalPackLineVolumeCalcEdit;
		ZArchitecture.ZLabel TotalWeightUnitLabel;
		ZArchitecture.ZLabel TotalVolumeUnitLabel;
		ZArchitecture.ZLabel PackageTypeLabel;
		ZButton OKButton;

		new void InitializeComponent()
		{
			this.InnerPackLinesGrid = new InnerPackLinesGrid();
			this.PackageDetailsGroupBox = new ZGroupBox();
			this.TotalInnerPacksCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TotalPackLineWeightCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TotalPackLineVolumeCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TotalWeightUnitLabel = new ZArchitecture.ZLabel();
			this.TotalVolumeUnitLabel = new ZArchitecture.ZLabel();
			this.PackageTypeLabel = new ZArchitecture.ZLabel();
			this.OKButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.InnerPackLinesGrid)).BeginInit();
			this.PackageDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 24, true);
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
			this.BindingSource.DataSourceType = typeof(ForwardingPackLine);
			// 
			// PackLinesGrid
			// 
			this.InnerPackLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InnerPackLinesGrid, "InnerPackLines");
			this.InnerPackLinesGrid.GridId = "19c2c908-a934-458a-4065-ac0619a1a423";
			this.InnerPackLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InnerPackLinesGrid.LayoutKey = "InnerPackLinesGrid";
			this.InnerPackLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.InnerPackLinesGrid.Name = "InnerPackLinesGrid";
			this.InnerPackLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 164, true);
			this.InnerPackLinesGrid.TabIndex = 6;
			// 
			// PackageDetailsGroupBox
			// 
			this.PackageDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("InnerPackLinesOfOuterPackLineForm|3ec94e02-2347-c49c-47d6-d782b05f3113", "Packaging Details");
			this.PackageDetailsGroupBox.Controls.Add(this.TotalInnerPacksCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalPackLineWeightCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalPackLineVolumeCalcEdit);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalWeightUnitLabel);
			this.PackageDetailsGroupBox.Controls.Add(this.TotalVolumeUnitLabel);
			this.PackageDetailsGroupBox.Controls.Add(this.PackageTypeLabel);
			this.PackageDetailsGroupBox.Controls.Add(this.InnerPackLinesGrid);
			this.PackageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.PackageDetailsGroupBox.Name = "PackageDetailsGroupBox";
			this.PackageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 245, true);
			this.PackageDetailsGroupBox.TabIndex = 7;
			this.PackageDetailsGroupBox.TabStop = false;
			// 
			// TotalWeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TotalPackLineWeightCalcEdit, "InnerPackTotalWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((ForwardingPackLine)(null)).InnerPackTotalWeight)));
			this.TotalPackLineWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("InnerPackLinesOfOuterPackLineForm|267f32fd-e269-aeaf-499d-ac044855512b", "Total Weight");
			this.TotalPackLineWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 193, true);
			this.TotalPackLineWeightCalcEdit.Name = "TotalPackLineWeightCalcEdit";
			this.TotalPackLineWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalPackLineWeightCalcEdit.TabIndex = 12;
			this.TotalPackLineWeightCalcEdit.Text = "0.000";
			this.TotalPackLineWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalWeightUnitLabel, "JL_ActualWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingPackLine)(null)).JL_ActualWeightUQ)));
			this.TotalWeightUnitLabel.CaptionResourceString = null;
			this.TotalWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 194, true);
			this.TotalWeightUnitLabel.Name = "TotalWeightUnitLabel";
			this.TotalWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.TotalWeightUnitLabel.TabIndex = 27;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalWeightUnitLabel, false);

			// 
			// TotalVolumeUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalVolumeUnitLabel, "JL_ActualVolumeUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingPackLine)(null)).JL_ActualVolumeUQ)));
			this.TotalVolumeUnitLabel.CaptionResourceString = null;
			this.TotalVolumeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 194, true);
			this.TotalVolumeUnitLabel.Name = "TotalVolumeUnitLabel";
			this.TotalVolumeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.TotalVolumeUnitLabel.TabIndex = 25;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalVolumeUnitLabel, false);
			// 
			// PackageTypeLabel
			// 
			this.BindingSource.SetBindingMember(this.PackageTypeLabel, "InnerPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingPackLine)(null)).JL_ActualWeightUQ)));
			this.PackageTypeLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("InnerPackLinesOfOuterPackLineForm|8aaecdce-4e72-6e83-493f-aa65fcfa3c5d", "PKG");
			this.PackageTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 194, true);
			this.PackageTypeLabel.Name = "PackageTypeLabel";
			this.PackageTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.PackageTypeLabel.TabIndex = 25;
			// 
			// TotalPackLineVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPackLineVolumeCalcEdit, "InnerPackTotalVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((ForwardingPackLine)(null)).InnerPackTotalVolume)));
			this.TotalPackLineVolumeCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("InnerPackLinesOfOuterPackLineForm|4650152f-ff81-218d-4837-77577d509efb", "Total Volume");
			this.TotalPackLineVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 193, true);
			this.TotalPackLineVolumeCalcEdit.Name = "TotalPackLineVolumeCalcEdit";
			this.TotalPackLineVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalPackLineVolumeCalcEdit.TabIndex = 13;
			this.TotalPackLineVolumeCalcEdit.Text = "0.000";
			this.TotalPackLineVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalInnerPacksCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalInnerPacksCalcEdit, "InnerPackCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((ForwardingPackLine)(null)).InnerPackCount)));
			this.TotalInnerPacksCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("InnerPackLinesOfOuterPackLineForm|281536a1-9ed3-8188-4a03-93f6ea17c237", "Total Inner Packs");
			this.TotalInnerPacksCalcEdit.Decimals = 0;
			this.TotalInnerPacksCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 193, true);
			this.TotalInnerPacksCalcEdit.Name = "TotalInnerPacksCalcEdit";
			this.TotalInnerPacksCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalInnerPacksCalcEdit.TabIndex = 8;
			this.TotalInnerPacksCalcEdit.Text = "0";
			this.TotalInnerPacksCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("InnerPackLinesOfOuterPackLineForm|fa071b70-ed6f-7b96-48b1-acd0030d6d0c", "OK", "OK", "OK", "Accept Changes.");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 314, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 11;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// InnerPackLinesOfOuterPackLineForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 363, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.PackageDetailsGroupBox);
			this.Name = "InnerPackLinesOfOuterPackLineForm";
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(ForwardingPackLine);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ForwardingPackLine";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.InnerPackLinesGrid)).EndInit();
			this.PackageDetailsGroupBox.ResumeLayout(false);
			this.PackageDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
