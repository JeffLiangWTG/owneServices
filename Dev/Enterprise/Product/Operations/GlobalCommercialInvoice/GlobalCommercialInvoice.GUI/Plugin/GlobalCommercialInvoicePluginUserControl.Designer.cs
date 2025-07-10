using CargoWise.Windows.UI;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	partial class GlobalCommercialInvoicePluginUserControl
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
			this.components = new System.ComponentModel.Container();
			this.InvoiceMainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.InvoiceHeaderTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceHeaderSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.InvoiceLineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceLineSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.InvoiceLineUserControlsVisibilityLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceMainTabControl.SuspendLayout();
			this.InvoiceHeaderTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceHeaderSplitContainer)).BeginInit();
			this.InvoiceHeaderSplitContainer.SuspendLayout();
			this.InvoiceLineTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLineSplitContainer)).BeginInit();
			this.InvoiceLineSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceMainTabControl
			// 
			this.InvoiceMainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InvoiceMainTabControl.Controls.Add(this.InvoiceHeaderTabPage);
			this.InvoiceMainTabControl.Controls.Add(this.InvoiceLineTabPage);
			this.InvoiceMainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceMainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceMainTabControl.Name = "InvoiceMainTabControl";
			this.InvoiceMainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 667, true);
			this.InvoiceMainTabControl.TabIndex = 1;
			// 
			// InvoiceHeaderTabPage
			// 
			this.InvoiceHeaderTabPage.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("19C4BB38-CBCF-4BFC-802A-5C481FAB524E", "Inv. Headers");
			this.InvoiceHeaderTabPage.Controls.Add(this.InvoiceHeaderSplitContainer);
			this.InvoiceHeaderTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceHeaderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.InvoiceHeaderTabPage.Name = "InvoiceHeaderTabPage";
			this.InvoiceHeaderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 645, true);
			this.InvoiceHeaderTabPage.TabIndex = 0;
			// 
			// InvoiceHeaderSplitContainer
			// 
			this.InvoiceHeaderSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceHeaderSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceHeaderSplitContainer.Name = "InvoiceHeaderSplitContainer";
			this.InvoiceHeaderSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.InvoiceHeaderSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 645, true);
			this.InvoiceHeaderSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			// 
			// InvoiceHeaderSplitContainer.Panel2
			// 
			this.InvoiceHeaderSplitContainer.Panel2.AutoScroll = false;
			this.InvoiceHeaderSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(167);
			this.InvoiceHeaderSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(269);
			this.InvoiceHeaderSplitContainer.SplitterWidth = 9;
			this.InvoiceHeaderSplitContainer.TabIndex = 0;
			// 
			// InvoiceLineTabPage
			// 
			this.InvoiceLineTabPage.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("C748F9AD-7C45-4E43-B490-B7F12079A29F", "Inv. Lines");
			this.InvoiceLineTabPage.Controls.Add(this.InvoiceLineUserControlsVisibilityLabel);
			this.InvoiceLineTabPage.Controls.Add(this.InvoiceLineSplitContainer);
			this.InvoiceLineTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.InvoiceLineTabPage.Name = "InvoiceLineTabPage";
			this.InvoiceLineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 645, true);
			this.InvoiceLineTabPage.TabIndex = 0;
			// 
			// InvoiceLineSplitContainer
			// 
			this.InvoiceLineSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineSplitContainer.Name = "InvoiceLineSplitContainer";
			this.InvoiceLineSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// InvoiceLineSplitContainer.Panel1
			// 
			this.InvoiceLineSplitContainer.Panel1.AutoScroll = true;
			this.InvoiceLineSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 645, true);
			this.InvoiceLineSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			// 
			// InvoiceLineSplitContainer.Panel2
			// 
			this.InvoiceLineSplitContainer.Panel2.AutoScroll = false;
			this.InvoiceLineSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(167);
			this.InvoiceLineSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(269);
			this.InvoiceLineSplitContainer.SplitterWidth = 9;
			this.InvoiceLineSplitContainer.TabIndex = 0;
			// 
			// InvoiceLineHideUserControlsLabel
			// 
			this.InvoiceLineUserControlsVisibilityLabel.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("4607c2d7-93ea-4a49-91f1-3d13821b081e", "You must enter at least one Invoice Header before you can create Invoice Lines.");
			this.InvoiceLineUserControlsVisibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineUserControlsVisibilityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InvoiceLineUserControlsVisibilityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineUserControlsVisibilityLabel.Name = "InvoiceLineUserControlsVisibilityLabel";
			this.InvoiceLineUserControlsVisibilityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 364, true);
			this.InvoiceLineUserControlsVisibilityLabel.TabIndex = 0;
			this.InvoiceLineUserControlsVisibilityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.InvoiceLineUserControlsVisibilityLabel.UseMnemonic = false;
			// 
			// GlobalCommercialInvoicePluginUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceMainTabControl);
			this.Name = "GlobalCommercialInvoicePluginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 667, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceMainTabControl.ResumeLayout(false);
			this.InvoiceMainTabControl.PerformLayout();
			this.InvoiceHeaderTabPage.ResumeLayout(false);
			this.InvoiceHeaderTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceHeaderSplitContainer)).EndInit();
			this.InvoiceHeaderSplitContainer.ResumeLayout(false);
			this.InvoiceHeaderSplitContainer.PerformLayout();
			this.InvoiceLineTabPage.ResumeLayout(false);
			this.InvoiceLineTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLineSplitContainer)).EndInit();
			this.InvoiceLineSplitContainer.ResumeLayout(false);
			this.InvoiceLineSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl InvoiceMainTabControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage InvoiceHeaderTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage InvoiceLineTabPage;
		public KSplitContainer InvoiceHeaderSplitContainer;
		public KSplitContainer InvoiceLineSplitContainer;
		internal ZArchitecture.ZLabel InvoiceLineUserControlsVisibilityLabel;
	}
}
