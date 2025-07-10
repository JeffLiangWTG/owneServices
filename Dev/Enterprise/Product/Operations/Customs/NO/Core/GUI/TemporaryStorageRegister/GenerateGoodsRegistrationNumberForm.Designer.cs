using CargoWise.Types;

namespace Enterprise.Customs.NO.GUI;

partial class GenerateGoodsRegistrationNumberForm
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

	#region Windows Form Designer generated code

	new void InitializeComponent()
	{
        this.CancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.CreateNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.GoodsRegistrationZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        this.WarehouseAuthorizationZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.GoodsRegistrationZDateEdit.SuspendLayout();
        this.WarehouseAuthorizationZDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // MainStatusBar
        // 
        this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 143, true);
        this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 24, true);
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.GoodsRegistrationNumberGeneratorObject);
        // 
        // CancelFormButton
        // 
        this.CancelFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.CancelFormButton.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("FB85C044-46DF-4257-8200-B31FED3F8C3E", "Cancel");
        this.CancelFormButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(566, 110, true);
        this.CancelFormButton.Name = "CancelFormButton";
        this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.CancelFormButton.TabIndex = 3;
        this.CancelFormButton.ToolTipCaption = null;
        this.CancelFormButton.Click += new System.EventHandler(this.CancelButton_Click);
        // 
        // CreateNextButton
        // 
        this.CreateNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.CreateNextButton.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("EED2764A-83D4-4BD8-BB53-78B80DD51A27", "Create Next");
        this.CreateNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 110, true);
        this.CreateNextButton.Name = "CreateNextButton";
        this.CreateNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.CreateNextButton.TabIndex = 2;
        this.CreateNextButton.ToolTipCaption = null;
        this.CreateNextButton.Click += new System.EventHandler(this.CreateNextButton_Click);
        // 
        // GoodsRegistrationZDateEdit
        // 
        this.GoodsRegistrationZDateEdit.AllowDrop = true;
        this.GoodsRegistrationZDateEdit.AutoCompleteMonthThreshold = 1;
        this.BindingSource.SetBindingMember(this.GoodsRegistrationZDateEdit, "GoodsRegistrationDate");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.GoodsRegistrationNumberGeneratorObject)(null)).GoodsRegistrationDate)));
        this.GoodsRegistrationZDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
        this.GoodsRegistrationZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 40, true);
        this.GoodsRegistrationZDateEdit.Name = "GoodsRegistrationZDateEdit";
        this.GoodsRegistrationZDateEdit.TabIndex = 0;
        // 
        // WarehouseAuthorizationZDropEdit
        // 
        this.WarehouseAuthorizationZDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.WarehouseAuthorizationZDropEdit, "WarehouseAuthorisationId");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.GoodsRegistrationNumberGeneratorObject)(null)).WarehouseAuthorisationId)));
        this.WarehouseAuthorizationZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 74, true);
        this.WarehouseAuthorizationZDropEdit.Name = "WarehouseAuthorizationZDropEdit";
        this.WarehouseAuthorizationZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 15, true);
        this.WarehouseAuthorizationZDropEdit.TabIndex = 1;
        // 
        // GenerateGoodsNumberForm
        // 
        this.AcceptButton = this.CreateNextButton;
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CancelButton = this.CancelFormButton;
        this.CaptionRenderingEnabled = true;
        this.CaptionResourceString = Res.GetData("9AF767EB-3320-4507-8730-BDA7084246A2", "Create next Goods Number");
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 167, true);
        this.Controls.Add(this.CancelFormButton);
        this.Controls.Add(this.CreateNextButton);
        this.Controls.Add(this.WarehouseAuthorizationZDropEdit);
        this.Controls.Add(this.GoodsRegistrationZDateEdit);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Name = "GenerateGoodsNumberForm";
        this.Controls.SetChildIndex(this.GoodsRegistrationZDateEdit, 0);
        this.Controls.SetChildIndex(this.WarehouseAuthorizationZDropEdit, 0);
        this.Controls.SetChildIndex(this.MainStatusBar, 0);
        this.Controls.SetChildIndex(this.CreateNextButton, 0);
        this.Controls.SetChildIndex(this.CancelFormButton, 0);
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.GoodsRegistrationZDateEdit.ResumeLayout(true);
        this.GoodsRegistrationZDateEdit.PerformLayout();
        this.WarehouseAuthorizationZDropEdit.ResumeLayout(true);
        this.WarehouseAuthorizationZDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZDateEdit GoodsRegistrationZDateEdit;
	internal ZArchitecture.GUI.ZDropEdit WarehouseAuthorizationZDropEdit;
	internal ZArchitecture.GUI.ZButton CancelFormButton;
	internal ZArchitecture.GUI.ZButton CreateNextButton;
}
