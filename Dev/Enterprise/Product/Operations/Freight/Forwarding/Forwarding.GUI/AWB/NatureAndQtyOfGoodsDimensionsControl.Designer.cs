using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsDimensionsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.lengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.widthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.heightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.unitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.countCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// lengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.lengthCalcEdit, "NatureAndQtyOfGoodsDimensions.Length");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsDimensions.Length)));
			this.lengthCalcEdit.CaptionResourceString = Res.GetData("49fa23a0-f235-450f-ad41-569f8ba67394", "Length");
			this.lengthCalcEdit.DecimalPlaces = 0;
			this.lengthCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.lengthCalcEdit, false);
			this.lengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.lengthCalcEdit.Name = "lengthCalcEdit";
			this.lengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.lengthCalcEdit.TabIndex = 0;
			this.lengthCalcEdit.Text = "0";
			this.lengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// widthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.widthCalcEdit, "NatureAndQtyOfGoodsDimensions.Width");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsDimensions.Width)));
			this.widthCalcEdit.CaptionResourceString = Res.GetData("400cb459-bdb0-4bc0-9a63-3f21817171b8", "Width");
			this.widthCalcEdit.DecimalPlaces = 0;
			this.widthCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.widthCalcEdit, false);
			this.widthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 0, true);
			this.widthCalcEdit.Name = "widthCalcEdit";
			this.widthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.widthCalcEdit.TabIndex = 1;
			this.widthCalcEdit.Text = "0";
			this.widthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// heightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.heightCalcEdit, "NatureAndQtyOfGoodsDimensions.Height");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsDimensions.Height)));
			this.heightCalcEdit.CaptionResourceString = Res.GetData("060a0532-3295-42ac-ab4d-ccdb3d7e6bb2", "Height");
			this.heightCalcEdit.DecimalPlaces = 0;
			this.heightCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.heightCalcEdit, false);
			this.heightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 0, true);
			this.heightCalcEdit.Name = "heightCalcEdit";
			this.heightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.heightCalcEdit.TabIndex = 3;
			this.heightCalcEdit.Text = "0";
			this.heightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// unitDropEdit
			// 
			this.unitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.unitDropEdit, "NatureAndQtyOfGoodsDimensions.Unit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsDimensions.Unit)));
			this.unitDropEdit.CaptionResourceString = Res.GetData("25d01397-a125-4947-afbe-9a9615419a22", "Unit");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.unitDropEdit, false);
			this.unitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 0, true);
			this.unitDropEdit.Name = "unitDropEdit";
			this.unitDropEdit.PreBoundMaxLength = 2;
			this.unitDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.unitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.unitDropEdit.TabIndex = 4;
			// 
			// countCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.countCalcEdit, "NatureAndQtyOfGoodsDimensions.Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsDimensions.Count)));
			this.countCalcEdit.CaptionResourceString = Res.GetData("c7758eca-1520-47db-970d-32ac5e0d7701", "Count");
			this.countCalcEdit.DecimalPlaces = 0;
			this.countCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.countCalcEdit, false);
			this.countCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 0, true);
			this.countCalcEdit.Name = "countCalcEdit";
			this.countCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.countCalcEdit.TabIndex = 5;
			this.countCalcEdit.Text = "0";
			this.countCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NatureAndQtyOfGoodsDimensionsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.countCalcEdit);
			this.Controls.Add(this.unitDropEdit);
			this.Controls.Add(this.heightCalcEdit);
			this.Controls.Add(this.widthCalcEdit);
			this.Controls.Add(this.lengthCalcEdit);
			this.Name = "NatureAndQtyOfGoodsDimensionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZCalcEdit lengthCalcEdit;
		private ZArchitecture.ZCalcEdit widthCalcEdit;
		private ZArchitecture.ZCalcEdit heightCalcEdit;
		private ZArchitecture.GUI.ZDropEdit unitDropEdit;
		private ZArchitecture.ZCalcEdit countCalcEdit;
	}
}
