namespace Enterprise.Customs.NO.GUI;

partial class GoodsRegistrationNumberUserControl
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
        this.GoodsNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.GenerateGoodsNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.CusTempStorageRegHeader);
		// 
		// GoodsNumberTextBox
		// 
		this.BindingSource.SetBindingMember(this.GoodsNumberTextBox, "SRH_Reference");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusTempStorageRegHeader)(null)).SRH_Reference)));
        this.GoodsNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
        this.GoodsNumberTextBox.Name = "GoodsNumberTextBox";
        this.GoodsNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 15, true);
        this.GoodsNumberTextBox.TabIndex = 0;
        // 
        // GenerateGoodsNumberButton
        // 
        this.GenerateGoodsNumberButton.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("9E223AFB-2EA2-48BF-93A8-515F47DE4389", "Select goods number");
        this.GenerateGoodsNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 1, true);
        this.GenerateGoodsNumberButton.Name = "GenerateGoodsNumberButton";
        this.GenerateGoodsNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
        this.GenerateGoodsNumberButton.TabIndex = 1;
        this.GenerateGoodsNumberButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.GenerateGoodsNumberButton.ToolTipCaption = null;
		this.GenerateGoodsNumberButton.Click += new System.EventHandler(this.GenerateGoodsNumberButton_Click);
		// 
		// GoodsNumberUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.GoodsNumberTextBox);
        this.Controls.Add(this.GenerateGoodsNumberButton);
        this.Name = "GoodsNumberUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 24, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	ZArchitecture.ZTextBox GoodsNumberTextBox;
	ZArchitecture.GUI.ZButton GenerateGoodsNumberButton;
}
