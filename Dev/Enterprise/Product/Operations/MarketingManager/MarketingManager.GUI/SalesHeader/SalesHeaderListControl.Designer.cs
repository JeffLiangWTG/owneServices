namespace Enterprise.MarketingManager.GUI
{
	partial class SalesHeaderListControl
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
			this.mainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.DateForExchangeRate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TopToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.AddToolStripDropDownButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.CreateQuotationToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.stripsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainTableLayoutPanel.SuspendLayout();
			this.DateForExchangeRate.SuspendLayout();
			this.TopToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ISalesValueAssociatedEntity);
			// 
			// mainTableLayoutPanel
			// 
			this.mainTableLayoutPanel.ColumnCount = 2;
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 330F));
			this.mainTableLayoutPanel.Controls.Add(this.DateForExchangeRate, 1, 0);
			this.mainTableLayoutPanel.Controls.Add(this.TopToolStrip, 0, 0);
			this.mainTableLayoutPanel.Controls.Add(this.stripsPanel, 0, 1);
			this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
			this.mainTableLayoutPanel.RowCount = 2;
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 100, true);
			this.mainTableLayoutPanel.TabIndex = 0;
			#if !WINZOR
			this.mainTableLayoutPanel.CellPaint += new System.Windows.Forms.TableLayoutCellPaintEventHandler(this.MainTableLayoutPanel_CellPaint);
			#endif
			// 
			// DateForExchangeRate
			// 
			this.DateForExchangeRate.AllowDrop = true;
			this.DateForExchangeRate.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.DateForExchangeRate.AutoCompleteMonthThreshold = 1;
			this.DateForExchangeRate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateForExchangeRate, "DateForExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ISalesValueAssociatedEntity)(null)).DateForExchangeRate)));
			this.DateForExchangeRate.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9e857b8b-c9ba-4f99-9626-c26e5e6e7db5", "Exchange Rate Date");
			this.DateForExchangeRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 6, true);
			this.DateForExchangeRate.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 4, 6, 2, true);
			this.DateForExchangeRate.Name = "DateForExchangeRate";
			this.DateForExchangeRate.TabIndex = 1;
			// 
			// TopToolStrip
			// 
			this.TopToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.TopToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddToolStripDropDownButton,
            this.CreateQuotationToolStripButton});
			this.TopToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.TopToolStrip.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 0, 0, true);
			this.TopToolStrip.Name = "TopToolStrip";
			this.TopToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TopToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 30, true);
			this.TopToolStrip.TabIndex = 0;
			this.TopToolStrip.Text = "zToolStrip1";
			// 
			// AddToolStripDropDownButton
			// 
			this.AddToolStripDropDownButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f622db42-1aac-4f9c-b9b7-ef7752ab75f1", "Add Estimate Value");
			this.AddToolStripDropDownButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AddToolStripDropDownButton.Name = "AddToolStripDropDownButton";
			this.AddToolStripDropDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 27, true);
			this.AddToolStripDropDownButton.DropDownOpening += new System.EventHandler(this.AddToolStripDropDownButton_DropDownOpening);
			// 
			// CreateQuotationToolStripButton
			// 
			this.CreateQuotationToolStripButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5c6b3d96-86d4-4841-8543-6fa975d7b05a", "Create Quotation");
			this.CreateQuotationToolStripButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.CreateQuotationToolStripButton.Name = "CreateQuotationToolStripButton";
			this.CreateQuotationToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 27, true);
			this.CreateQuotationToolStripButton.Click += new System.EventHandler(this.CreateQuotationToolStripButton_Click);
			// 
			// stripsPanel
			// 
			this.stripsPanel.AutoScroll = true;
			this.mainTableLayoutPanel.SetColumnSpan(this.stripsPanel, 2);
			this.stripsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stripsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 33, true);
			this.stripsPanel.Name = "stripsPanel";
			this.stripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 64, true);
			this.stripsPanel.TabIndex = 2;
			this.stripsPanel.Click += new System.EventHandler(this.StripsPanel_Click);
			// 
			// SalesHeaderListControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "SalesHeaderListControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 100, true);
			this.Click += new System.EventHandler(this.SalesHeaderListControl_Click);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainTableLayoutPanel.ResumeLayout(false);
			this.mainTableLayoutPanel.PerformLayout();
			this.DateForExchangeRate.ResumeLayout(true);
			this.DateForExchangeRate.PerformLayout();
			this.TopToolStrip.ResumeLayout(false);
			this.TopToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel mainTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel stripsPanel;
		protected ZArchitecture.GUI.ZToolStrip TopToolStrip;
		protected ZArchitecture.GUI.ZToolStripDropDownButton AddToolStripDropDownButton;
		protected ZArchitecture.GUI.ZToolStripButton CreateQuotationToolStripButton;
		private ZArchitecture.GUI.ZDateEdit DateForExchangeRate;
	}
}
