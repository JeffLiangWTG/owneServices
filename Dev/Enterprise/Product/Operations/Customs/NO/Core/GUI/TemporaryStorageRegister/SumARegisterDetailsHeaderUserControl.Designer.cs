namespace Enterprise.Customs.NO.GUI;

partial class SumARegisterDetailsHeaderUserControl
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
        this.GoodsNumberUserControl = new Enterprise.Customs.NO.GUI.GoodsRegistrationNumberUserControl();
        this.TrasportMeansUserControl = new Enterprise.Customs.NO.GUI.CusTransportMeansUserControl();
        this.UnloadingRemarksLabel = new Enterprise.ZArchitecture.ZLabel();
        this.UnloadingRemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.GoodsNumberUserControl.SuspendLayout();
        this.TrasportMeansUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.CusTempStorageRegHeader);
		// 
		// GoodsNumberUserControl
		// 
		this.GoodsNumberUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.GoodsNumberUserControl, ".");
        this.GoodsNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 137, true);
        this.GoodsNumberUserControl.Name = "GoodsNumberUserControl";
        this.GoodsNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 27, true);
        this.GoodsNumberUserControl.TabIndex = 0;
        // 
        // TrasportMeansUserControl
        // 
        this.TrasportMeansUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TrasportMeansUserControl, "TransportMeans");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.NO.Business.CusTransportMeans)(((Enterprise.Customs.NO.Business.CusTempStorageRegHeader)(null)).TransportMeans)));
        this.TrasportMeansUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
        this.TrasportMeansUserControl.Name = "TrasportMeansUserControl";
        this.TrasportMeansUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 27, true);
        this.TrasportMeansUserControl.TabIndex = 1;
		// 
		// UnloadingRemarksLabel
		//
		this.UnloadingRemarksLabel.CaptionResourceString = Res.GetData("3D27A039-5CBE-4B2F-B297-CC1B95571B72", "Unloading Remarks");
		this.UnloadingRemarksLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
        this.UnloadingRemarksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
        this.UnloadingRemarksLabel.Name = "UnloadingRemarksLabel";
        this.UnloadingRemarksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 13, true);
        this.UnloadingRemarksLabel.TabIndex = 2;
        this.UnloadingRemarksLabel.UseMnemonic = false;
        // 
        // UnloadingRemarksTextBox
        // 
        this.BindingSource.SetBindingMember(this.UnloadingRemarksTextBox, "UnloadingRemarks");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusTempStorageRegHeader)(null)).UnloadingRemarks)));
        this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.UnloadingRemarksTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnloadingRemarksTextBox, false);
        this.UnloadingRemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 43, true);
        this.UnloadingRemarksTextBox.Multiline = true;
        this.UnloadingRemarksTextBox.Name = "UnloadingRemarksTextBox";
        this.UnloadingRemarksTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.UnloadingRemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 86, true);
        this.UnloadingRemarksTextBox.TabIndex = 3;
        // 
        // SumARegisterDetailsHeaderUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.GoodsNumberUserControl);
        this.Controls.Add(this.TrasportMeansUserControl);
        this.Controls.Add(this.UnloadingRemarksLabel);
        this.Controls.Add(this.UnloadingRemarksTextBox);
        this.Name = "SumARegisterDetailsHeaderUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 260, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.GoodsNumberUserControl.ResumeLayout(true);
        this.GoodsNumberUserControl.PerformLayout();
        this.TrasportMeansUserControl.ResumeLayout(true);
        this.TrasportMeansUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	internal GoodsRegistrationNumberUserControl GoodsNumberUserControl;
	internal CusTransportMeansUserControl TrasportMeansUserControl;
	internal Enterprise.ZArchitecture.ZTextBox UnloadingRemarksTextBox;
	internal Enterprise.ZArchitecture.ZLabel UnloadingRemarksLabel;
}
