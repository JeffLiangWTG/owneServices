namespace Enterprise.Customs.NO.GUI;

partial class CusTransportMeansUserControl
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
            this.NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.NationalityCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.CusTransportMeans);
            // 
            // NationalityCodeFindBox
            // 
            this.NationalityCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.NationalityCodeFindBox, "TPM_RN_NKTransportNationality");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusTransportMeans)(null)).TPM_RN_NKTransportNationality)));
            this.NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 1, true);
            this.NationalityCodeFindBox.Name = "NationalityCodeFindBox";
            this.NationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.NationalityCodeFindBox.ParentType = null;
            this.NationalityCodeFindBox.PreBoundMaxLength = 2;
            this.NationalityCodeFindBox.ShowDescriptionBox = false;
            this.NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 15, true);
            this.NationalityCodeFindBox.TabIndex = 1;
            // 
            // TransportIDTextBox
            // 
            this.BindingSource.SetBindingMember(this.TransportIDTextBox, "TPM_IdentificationNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusTransportMeans)(null)).TPM_IdentificationNumber)));
            this.TransportIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
            this.TransportIDTextBox.Name = "TransportIDTextBox";
            this.TransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
            this.TransportIDTextBox.TabIndex = 0;
            // 
            // CusTransportMeansUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TransportIDTextBox);
            this.Controls.Add(this.NationalityCodeFindBox);
            this.Name = "CusTransportMeansUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 21, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.NationalityCodeFindBox.ResumeLayout(true);
            this.NationalityCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

	}

	#endregion

	ZArchitecture.GUI.ZCodeFindBox NationalityCodeFindBox;
	ZArchitecture.ZTextBox TransportIDTextBox;
}
