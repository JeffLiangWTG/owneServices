using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NO.GUI;

partial class FTPSettingsCustomsUserControl
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
		this.SendToCustomFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ReceiveFromCustomFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SendToCustomFolderTextBox.SuspendLayout();
		this.ReceiveFromCustomFolderTextBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry);
		// 
		// SendToCustomFolderTextBox
		// 
		this.BindingSource.SetBindingMember(this.SendToCustomFolderTextBox, "SendToCustomFolder");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).SendToCustomFolder)));
		this.SendToCustomFolderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.SendToCustomFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 18, true);
		this.SendToCustomFolderTextBox.Name = "SendToCustomFolderTextBox";
		this.SendToCustomFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
		this.SendToCustomFolderTextBox.TabIndex = 6;
		// 
		// ReceiveFromCustomFolderTextBox
		// 
		this.BindingSource.SetBindingMember(this.ReceiveFromCustomFolderTextBox, "ReceiveFromCustomFolder");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).ReceiveFromCustomFolder)));
		this.ReceiveFromCustomFolderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.ReceiveFromCustomFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 39, true);
		this.ReceiveFromCustomFolderTextBox.Name = "ReceiveFromCustomFolderTextBox";
		this.ReceiveFromCustomFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
		this.ReceiveFromCustomFolderTextBox.TabIndex = 7;
		// 
		// FTPSettingsCustomsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.SendToCustomFolderTextBox);
		this.Controls.Add(this.ReceiveFromCustomFolderTextBox);
		this.Name = "FTPSettingsCustomsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 202, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.SendToCustomFolderTextBox.ResumeLayout(false);
		this.SendToCustomFolderTextBox.PerformLayout();
		this.ReceiveFromCustomFolderTextBox.ResumeLayout(false);
		this.ReceiveFromCustomFolderTextBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZTextBox SendToCustomFolderTextBox;
	internal ZTextBox ReceiveFromCustomFolderTextBox;
}
