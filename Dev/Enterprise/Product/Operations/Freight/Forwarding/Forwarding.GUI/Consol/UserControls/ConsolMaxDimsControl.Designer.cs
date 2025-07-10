
namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ConsolMaxDimsControl
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
			this.MaxPackageWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxPackageLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxPackageHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxPackageUnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MaxDimsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WidthLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HeightLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MaxPackageUnitDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingConsol);
			// 
			// MaxPackageWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxPackageWidthCalcEdit, "JK_MaximumAllowablePackageWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MaximumAllowablePackageWidth)));
			this.MaxPackageWidthCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolMaxDimsControl|8b8eb18d-eda5-4760-980d-30890afba458", "Maximum Allowable Package Width");
			this.MaxPackageWidthCalcEdit.Decimals = 3;
			this.MaxPackageWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 2, true);
			this.MaxPackageWidthCalcEdit.Name = "MaxPackageWidthCalcEdit";
			this.MaxPackageWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.MaxPackageWidthCalcEdit.TabIndex = 2;
			this.MaxPackageWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxPackageLengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxPackageLengthCalcEdit, "JK_MaximumAllowablePackageLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MaximumAllowablePackageLength)));
			this.MaxPackageLengthCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolMaxDimsControl|db68c37e-1774-4496-9c90-879c1b1f9e52", "Maximum Allowable Package Length");
			this.MaxPackageLengthCalcEdit.Decimals = 3;
			this.MaxPackageLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 2, true);
			this.MaxPackageLengthCalcEdit.Name = "MaxPackageLengthCalcEdit";
			this.MaxPackageLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.MaxPackageLengthCalcEdit.TabIndex = 1;
			this.MaxPackageLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxPackageHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxPackageHeightCalcEdit, "JK_MaximumAllowablePackageHeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MaximumAllowablePackageHeight)));
			this.MaxPackageHeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolMaxDimsControl|d87fc287-e8dc-4849-bf0d-5651a4f0e17d", "Maximum Allowable Package Height");
			this.MaxPackageHeightCalcEdit.Decimals = 3;
			this.MaxPackageHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 2, true);
			this.MaxPackageHeightCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MaxPackageHeightCalcEdit.Name = "MaxPackageHeightCalcEdit";
			this.MaxPackageHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.MaxPackageHeightCalcEdit.TabIndex = 3;
			this.MaxPackageHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxPackageUnitDropEdit
			// 
			this.MaxPackageUnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MaxPackageUnitDropEdit, "JK_MaximumAllowablePackageUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).JK_MaximumAllowablePackageUnit)));
			this.MaxPackageUnitDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolMaxDimsControl|32d05813-de71-47c0-9735-98c42f23495f", "Maximum Allowable Package Unit");
			this.MaxPackageUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 2, true);
			this.MaxPackageUnitDropEdit.Name = "MaxPackageUnitDropEdit";
			this.MaxPackageUnitDropEdit.PreBoundMaxLength = 3;
			this.MaxPackageUnitDropEdit.ShouldResizeByMaxLength = true;
			this.MaxPackageUnitDropEdit.ShowDescriptionBox = false;
			this.MaxPackageUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.MaxPackageUnitDropEdit.TabIndex = 4;
			// 
			// MaxDimsLabel
			// 
			this.MaxDimsLabel.AutoSize = true;
			this.MaxDimsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolMaxDimsControl|ac0cf5ad-1e0a-47b4-86e5-6954088815d7", "Max Dims:", "Maximum allowable dimensions for this consol.");
			this.MaxDimsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MaxDimsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MaxDimsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.MaxDimsLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MaxDimsLabel.Name = "MaxDimsLabel";
			this.MaxDimsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.MaxDimsLabel.TabIndex = 15;
			// 
			// WidthLabel
			// 
			this.WidthLabel.AutoSize = true;
			this.WidthLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolMaxDimsControl|b0242acb-07e8-46ae-a3d5-c9f583753fe9", "x");
			this.WidthLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WidthLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 4, true);
			this.WidthLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.WidthLabel.Name = "WidthLabel";
			this.WidthLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(11, 13, true);
			this.WidthLabel.TabIndex = 16;
			// 
			// HeightLabel
			// 
			this.HeightLabel.AutoSize = true;
			this.HeightLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolMaxDimsControl|b0242acb-07e8-46ae-a3d5-c9f583753fe9", "x");
			this.HeightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 4, true);
			this.HeightLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeightLabel.Name = "HeightLabel";
			this.HeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(11, 13, true);
			this.HeightLabel.TabIndex = 17;
			// 
			// ConsolMaxDimsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MaxPackageWidthCalcEdit);
			this.Controls.Add(this.MaxPackageLengthCalcEdit);
			this.Controls.Add(this.MaxPackageHeightCalcEdit);
			this.Controls.Add(this.MaxPackageUnitDropEdit);
			this.Controls.Add(this.MaxDimsLabel);
			this.Controls.Add(this.WidthLabel);
			this.Controls.Add(this.HeightLabel);
			this.Name = "ConsolMaxDimsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MaxPackageUnitDropEdit.ResumeLayout(true);
			this.MaxPackageUnitDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit MaxPackageWidthCalcEdit;
		private ZArchitecture.ZCalcEdit MaxPackageLengthCalcEdit;
		private ZArchitecture.ZCalcEdit MaxPackageHeightCalcEdit;
		private ZArchitecture.GUI.ZDropEdit MaxPackageUnitDropEdit;
		private ZArchitecture.ZLabel MaxDimsLabel;
		private ZArchitecture.ZLabel WidthLabel;
		private ZArchitecture.ZLabel HeightLabel;
	}
}
