using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

partial class SpecificDataUserControl
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
		this.DynamicSpecificDataPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent);
		// 
		// DynamicSpecificDataPanel
		// 
		this.DynamicSpecificDataPanel.AllowDrop = true;
		this.DynamicSpecificDataPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DynamicSpecificDataPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.DynamicSpecificDataPanel.Name = "DynamicSpecificDataPanel";
		this.DynamicSpecificDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 128, true);
		this.DynamicSpecificDataPanel.TabIndex = 1;
		// 
		// SpecificDataUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.DynamicSpecificDataPanel);
		this.Name = "SpecificDataUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 128, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion
	internal DynamicLayoutPanel DynamicSpecificDataPanel;
}
