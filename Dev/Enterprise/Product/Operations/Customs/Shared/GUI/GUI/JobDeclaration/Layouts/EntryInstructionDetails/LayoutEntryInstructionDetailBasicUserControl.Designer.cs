using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class LayoutEntryInstructionDetailBasicUserControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.dynamicDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// dynamicDetailsPanel
			//
			this.BindingSource.SetBindingMember(this.dynamicDetailsPanel, nameof(BaseJobDeclaration.CustomsEntryInstructions));
			this.dynamicDetailsPanel.AllowDrop = true;
			this.dynamicDetailsPanel.AutoScroll = true;
			this.dynamicDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dynamicDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.dynamicDetailsPanel.Name = "dynamicDetailsPanel";
			this.dynamicDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.dynamicDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 475, true);
			this.dynamicDetailsPanel.TabIndex = 1;
			//
			// LayoutEntryInstructionDetailBasicUserControl
			//
			this.AutoScroll = true;
			this.Controls.Add(this.dynamicDetailsPanel);
			this.CaptionRenderingEnabled = true;
			this.Name = "LayoutEntryInstructionDetailBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 475, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected DynamicLayoutPanel dynamicDetailsPanel;
	}
}
