using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	partial class OverhangControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OnfileLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ActualLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverhangLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StandardDataHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AddDataHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StandardDataLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AddDataLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangFront = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangBack = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StandardWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AddDataWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangLeft = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangRight = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonContainer);
			// 
			// OnfileLabel
			// 
			this.OnfileLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("56b53ced-83ab-4491-be25-1cbb8a7907db", "On File");
			this.OnfileLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 13, true);
			this.OnfileLabel.Name = "OnfileLabel";
			this.OnfileLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 15, true);
			this.OnfileLabel.TabIndex = 0;
			this.OnfileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ActualLabel
			// 
			this.ActualLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("d7edd828-0431-4a30-aed6-3f8f4255d7b9", "Actual");
			this.ActualLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 13, true);
			this.ActualLabel.Name = "ActualLabel";
			this.ActualLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 15, true);
			this.ActualLabel.TabIndex = 1;
			this.ActualLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OverhangLabel
			// 
			this.OverhangLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("9c0b3b05-643d-408e-a96c-4a25bbe20e5f", "Overhang");
			this.OverhangLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 13, true);
			this.OverhangLabel.Name = "OverhangLabel";
			this.OverhangLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 15, true);
			this.OverhangLabel.TabIndex = 2;
			this.OverhangLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// StandardDataHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StandardDataHeightCalcEdit, "JobContainer+JC_Calc_Height");
			this.StandardDataHeightCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("408cfbeb-a2f6-4c8a-b212-3032b6bc498f", "H. (ft.)", "Height (ft.)");
			this.StandardDataHeightCalcEdit.DecimalPlaces = 2;
			this.StandardDataHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 31, true);
			this.StandardDataHeightCalcEdit.Name = "StandardDataHeightCalcEdit";
			this.StandardDataHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.StandardDataHeightCalcEdit.TabIndex = 3;
			this.StandardDataHeightCalcEdit.Text = "0.000";
			this.StandardDataHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AddDataHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AddDataHeightCalcEdit, "JobContainer+JC_TotalHeight");
			this.AddDataHeightCalcEdit.DecimalPlaces = 3;
			this.AddDataHeightCalcEdit.Decimals = 3;
			this.AddDataHeightCalcEdit.MaxValue = 100000;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddDataHeightCalcEdit, false);
			this.AddDataHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 31, true);
			this.AddDataHeightCalcEdit.Name = "AddDataHeightCalcEdit";
			this.AddDataHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.AddDataHeightCalcEdit.TabIndex = 4;
			this.AddDataHeightCalcEdit.Text = "0.000";
			this.AddDataHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangHeightCalcEdit, "JobContainer+JC_Calc_OverhangHeight");
			this.OverhangHeightCalcEdit.DecimalPlaces = 3;
			this.OverhangHeightCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverhangHeightCalcEdit, false);
			this.OverhangHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 31, true);
			this.OverhangHeightCalcEdit.Name = "OverhangHeightCalcEdit";
			this.OverhangHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OverhangHeightCalcEdit.TabIndex = 5;
			this.OverhangHeightCalcEdit.Text = "0.000";
			this.OverhangHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardDataLengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StandardDataLengthCalcEdit, "JobContainer+JC_Calc_Length");
			this.StandardDataLengthCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("4ac4f4ff-016b-406a-b365-43f500fea91c", "L. (ft.)", "Length (ft.)");
			this.StandardDataLengthCalcEdit.DecimalPlaces = 2;
			this.StandardDataLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 57, true);
			this.StandardDataLengthCalcEdit.Name = "StandardDataLengthCalcEdit";
			this.StandardDataLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.StandardDataLengthCalcEdit.TabIndex = 6;
			this.StandardDataLengthCalcEdit.Text = "0.000";
			this.StandardDataLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AddDataLengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AddDataLengthCalcEdit, "JobContainer+JC_TotalLength");
			this.AddDataLengthCalcEdit.DecimalPlaces = 3;
			this.AddDataLengthCalcEdit.Decimals = 3;
			this.AddDataLengthCalcEdit.MaxValue = 100000;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddDataLengthCalcEdit, false);
			this.AddDataLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 57, true);
			this.AddDataLengthCalcEdit.Name = "AddDataLengthCalcEdit";
			this.AddDataLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.AddDataLengthCalcEdit.TabIndex = 7;
			this.AddDataLengthCalcEdit.Text = "0.000";
			this.AddDataLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangLengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangLengthCalcEdit, "JobContainer+JC_Calc_OverhangLength");
			this.OverhangLengthCalcEdit.DecimalPlaces = 3;
			this.OverhangLengthCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverhangLengthCalcEdit, false);
			this.OverhangLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 57, true);
			this.OverhangLengthCalcEdit.Name = "OverhangLengthCalcEdit";
			this.OverhangLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OverhangLengthCalcEdit.TabIndex = 8;
			this.OverhangLengthCalcEdit.Text = "0.000";
			this.OverhangLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangFront
			// 
			this.BindingSource.SetBindingMember(this.OverhangFront, "JobContainer+JC_Calc_OverhangFront");
			this.OverhangFront.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("4132e8dd-3d1c-41dd-9f5f-af22b864455b", "Front", "Overhang Front");
			this.OverhangFront.DecimalPlaces = 3;
			this.OverhangFront.Decimals = 3;
			this.OverhangFront.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 83, true);
			this.OverhangFront.Name = "OverhangFront";
			this.OverhangFront.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OverhangFront.TabIndex = 9;
			this.OverhangFront.Text = "0.000";
			this.OverhangFront.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangBack
			// 
			this.BindingSource.SetBindingMember(this.OverhangBack, "JobContainer+JC_OverhangBack");
			this.OverhangBack.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("bbe3c9c9-267e-4c10-950c-fb14cdc6cfed", "Back", "Overhang Back");
			this.OverhangBack.MaxValue = 100000;
			this.OverhangBack.DecimalPlaces = 3;
			this.OverhangBack.Decimals = 3;
			this.OverhangBack.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 83, true);
			this.OverhangBack.Name = "OverhangBack";
			this.OverhangBack.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OverhangBack.TabIndex = 10;
			this.OverhangBack.Text = "0.000";
			this.OverhangBack.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StandardWidthCalcEdit, "JobContainer+JC_Calc_Width");
			this.StandardWidthCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("796eae13-258c-4296-905b-b1aa4a9b25ec", "W. (ft.)", "Width (ft.)");
			this.StandardWidthCalcEdit.DecimalPlaces = 2;
			this.StandardWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 109, true);
			this.StandardWidthCalcEdit.Name = "StandardWidthCalcEdit";
			this.StandardWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.StandardWidthCalcEdit.TabIndex = 11;
			this.StandardWidthCalcEdit.Text = "0.000";
			this.StandardWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AddDataWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AddDataWidthCalcEdit, "JobContainer+JC_TotalWidth");
			this.AddDataWidthCalcEdit.DecimalPlaces = 3;
			this.AddDataWidthCalcEdit.Decimals = 3;
			this.AddDataWidthCalcEdit.MaxValue = 100000;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddDataWidthCalcEdit, false);
			this.AddDataWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 109, true);
			this.AddDataWidthCalcEdit.Name = "AddDataWidthCalcEdit";
			this.AddDataWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.AddDataWidthCalcEdit.TabIndex = 12;
			this.AddDataWidthCalcEdit.Text = "0.000";
			this.AddDataWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangWidthCalcEdit, "JobContainer+JC_Calc_OverhangWidth");
			this.OverhangWidthCalcEdit.DecimalPlaces = 3;
			this.OverhangWidthCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverhangWidthCalcEdit, false);
			this.OverhangWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 109, true);
			this.OverhangWidthCalcEdit.Name = "OverhangWidthCalcEdit";
			this.OverhangWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OverhangWidthCalcEdit.TabIndex = 13;
			this.OverhangWidthCalcEdit.Text = "0.000";
			this.OverhangWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangLeft
			// 
			this.BindingSource.SetBindingMember(this.OverhangLeft, "JobContainer+JC_Calc_OverhangLeft");
			this.OverhangLeft.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("b373017e-d2b2-4372-9b9c-ec6bbdfe85d2", "Left", "Overhang Left");
			this.OverhangLeft.DecimalPlaces = 3;
			this.OverhangLeft.Decimals = 3;
			this.OverhangLeft.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 135, true);
			this.OverhangLeft.Name = "OverhangLeft";
			this.OverhangLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OverhangLeft.TabIndex = 14;
			this.OverhangLeft.Text = "0.000";
			this.OverhangLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangRight
			// 
			this.BindingSource.SetBindingMember(this.OverhangRight, "JobContainer+JC_OverhangRight");
			this.OverhangRight.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("d98c516f-f7d2-4e2d-b49e-c32bce53b349", "Right", "Overhang Right");
			this.OverhangRight.MaxValue = 100000;
			this.OverhangRight.DecimalPlaces = 3;
			this.OverhangRight.Decimals = 3;
			this.OverhangRight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 135, true);
			this.OverhangRight.Name = "OverhangRight";
			this.OverhangRight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.OverhangRight.TabIndex = 15;
			this.OverhangRight.Text = "0.000";
			this.OverhangRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OnfileLabel);
			this.Controls.Add(this.ActualLabel);
			this.Controls.Add(this.OverhangLabel);
			this.Controls.Add(this.StandardDataHeightCalcEdit);
			this.Controls.Add(this.AddDataHeightCalcEdit);
			this.Controls.Add(this.OverhangHeightCalcEdit);
			this.Controls.Add(this.StandardDataLengthCalcEdit);
			this.Controls.Add(this.AddDataLengthCalcEdit);
			this.Controls.Add(this.OverhangLengthCalcEdit);
			this.Controls.Add(this.OverhangFront);
			this.Controls.Add(this.OverhangBack);
			this.Controls.Add(this.StandardWidthCalcEdit);
			this.Controls.Add(this.AddDataWidthCalcEdit);
			this.Controls.Add(this.OverhangWidthCalcEdit);
			this.Controls.Add(this.OverhangLeft);
			this.Controls.Add(this.OverhangRight);
			this.Name = "OverhangControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 157, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit OverhangLeft;
		private ZArchitecture.ZCalcEdit OverhangFront;
		private ZArchitecture.ZCalcEdit AddDataLengthCalcEdit;
		private ZArchitecture.ZCalcEdit AddDataHeightCalcEdit;
		private ZArchitecture.ZCalcEdit AddDataWidthCalcEdit;
		private ZArchitecture.ZCalcEdit OverhangRight;
		private ZArchitecture.ZCalcEdit OverhangBack;
		private ZArchitecture.ZCalcEdit OverhangHeightCalcEdit;
		private ZArchitecture.ZCalcEdit OverhangLengthCalcEdit;
		private ZArchitecture.ZCalcEdit OverhangWidthCalcEdit;
		private ZArchitecture.ZLabel OverhangLabel;
		private ZArchitecture.ZLabel ActualLabel;
		private ZArchitecture.ZLabel OnfileLabel;
		private ZArchitecture.ZCalcEdit StandardWidthCalcEdit;
		private ZArchitecture.ZCalcEdit StandardDataHeightCalcEdit;
		private ZArchitecture.ZCalcEdit StandardDataLengthCalcEdit;

	}
}
