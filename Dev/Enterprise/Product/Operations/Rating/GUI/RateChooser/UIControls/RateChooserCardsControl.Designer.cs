namespace Enterprise.Rating.GUI.RateChooser
{
	partial class RateChooserCardsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblNoRatesFound = new Enterprise.ZArchitecture.ZLabel();
            this.pnlItemsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.ChooserContainerCommodityViewModel);
            // 
            // lblNoRatesFound
            // 
            this.BindingSource.SetBindingMember(this.lblNoRatesFound, "NoRatesFound");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserContainerCommodityViewModel)(null)).NoRatesFound)));
            this.lblNoRatesFound.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNoRatesFound.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblNoRatesFound.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblNoRatesFound.Name = "lblNoRatesFound";
            this.lblNoRatesFound.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 23, true);
            this.lblNoRatesFound.TabIndex = 0;
            this.lblNoRatesFound.Text = "NoRatesFound";
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.AutoScroll = true;
            this.pnlItemsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 174, true);
            this.pnlItemsContainer.TabIndex = 1;
            // 
            // RateChooserCardsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlItemsContainer);
            this.Controls.Add(this.lblNoRatesFound);
            this.Name = "RateChooserCardsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 197, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel lblNoRatesFound;
		private ZArchitecture.GUI.ZPanel pnlItemsContainer;
	}
}
