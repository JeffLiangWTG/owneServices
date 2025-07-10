using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class RateSelectorControl
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
            this.pnlTop = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.kTableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.zSortOptionsControl1 = new Enterprise.Rating.GUI.RateSelector.UIControls.SortOptionsControl();
            this.lblStatusText = new Enterprise.ZArchitecture.ZLabel();
            this.btnCardView = new Enterprise.ZArchitecture.GUI.ZButton();
            this.btnWarnings = new Enterprise.ZArchitecture.GUI.ZButton();
            this.pnlCardsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.tbLogs = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.kTableLayoutPanel1.SuspendLayout();
            this.zSortOptionsControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.SortableRatesViewModel);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.Black;
            this.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTop.Controls.Add(this.kTableLayoutPanel1);
            this.pnlTop.Controls.Add(this.btnCardView);
            this.pnlTop.Controls.Add(this.btnWarnings);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 36, true);
            this.pnlTop.TabIndex = 0;
            // 
            // kTableLayoutPanel1
            // 
            this.kTableLayoutPanel1.AutoSize = true;
            this.kTableLayoutPanel1.ColumnCount = 2;
            this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.kTableLayoutPanel1.Controls.Add(this.zSortOptionsControl1, 1, 0);
            this.kTableLayoutPanel1.Controls.Add(this.lblStatusText, 0, 0);
            this.kTableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.kTableLayoutPanel1.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 36, true);
            this.kTableLayoutPanel1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 36, true);
            this.kTableLayoutPanel1.Name = "kTableLayoutPanel1";
            this.kTableLayoutPanel1.RowCount = 1;
            this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.kTableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 36, true);
            this.kTableLayoutPanel1.TabIndex = 5;
            // 
            // zSortOptionsControl1
            // 
            this.zSortOptionsControl1.AllowDrop = true;
            this.zSortOptionsControl1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.zSortOptionsControl1.AutoSize = true;
            this.BindingSource.SetBindingMember(this.zSortOptionsControl1, ".");
            this.zSortOptionsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 3, true);
            this.zSortOptionsControl1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.zSortOptionsControl1.Name = "zSortOptionsControl1";
            this.zSortOptionsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 29, true);
            this.zSortOptionsControl1.TabIndex = 3;
            // 
            // lblStatusText
            // 
            this.lblStatusText.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatusText.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblStatusText, "StatusText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.SortableRatesViewModel)(null)).StatusText)));
            this.lblStatusText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblStatusText.ForeColor = System.Drawing.Color.White;
            this.lblStatusText.IsFontBold = true;
            this.lblStatusText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 11, true);
            this.lblStatusText.Name = "lblStatusText";
            this.lblStatusText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
            this.lblStatusText.TabIndex = 2;
            this.lblStatusText.Text = "StatusText";
            this.lblStatusText.UseMnemonic = false;
            // 
            // btnCardView
            // 
            this.btnCardView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.btnCardView, "CardViewText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.SortableRatesViewModel)(null)).CardViewText)));
            this.btnCardView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 6, true);
            this.btnCardView.Name = "btnCardView";
            this.btnCardView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
            this.btnCardView.TabIndex = 0;
            this.btnCardView.ToolTipCaption = null;
            this.btnCardView.UseVisualStyleBackColor = true;
            this.btnCardView.Click += new System.EventHandler(this.btnCardView_Click);
            // 
            // btnWarnings
            // 
            this.btnWarnings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.btnWarnings, "WarningsCountText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.SortableRatesViewModel)(null)).WarningsCountText)));
            this.btnWarnings.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1005, 6, true);
            this.btnWarnings.Name = "btnWarnings";
            this.btnWarnings.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 23, true);
            this.btnWarnings.TabIndex = 1;
            this.btnWarnings.ToolTipCaption = null;
            this.btnWarnings.UseVisualStyleBackColor = false;
            this.btnWarnings.Click += new System.EventHandler(this.btnWarnings_Click);
            // 
            // pnlCardsContainer
            // 
            this.pnlCardsContainer.AutoScroll = true;
            this.pnlCardsContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCardsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
            this.pnlCardsContainer.Name = "pnlCardsContainer";
            this.pnlCardsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 226, true);
            this.pnlCardsContainer.TabIndex = 2;
            // 
            // tbLogs
            // 
            this.tbLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLogs.ForeColor = System.Drawing.Color.Gainsboro;
            this.tbLogs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 262, true);
            this.tbLogs.Multiline = true;
            this.tbLogs.Name = "tbLogs";
            this.tbLogs.ReadOnly = true;
            this.tbLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbLogs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 298, true);
            this.tbLogs.TabIndex = 3;
            this.tbLogs.TabStop = false;
            // 
            // RateSelectorControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.tbLogs);
            this.Controls.Add(this.pnlCardsContainer);
            this.Controls.Add(this.pnlTop);
            this.Name = "RateSelectorControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1146, 560, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.kTableLayoutPanel1.ResumeLayout(false);
            this.kTableLayoutPanel1.PerformLayout();
            this.zSortOptionsControl1.ResumeLayout(true);
            this.zSortOptionsControl1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.GUI.ZButton btnWarnings;
		private ZArchitecture.GUI.ZButton btnCardView;
		private ZArchitecture.ZLabel lblStatusText;
		private ZArchitecture.GUI.ZPanel pnlCardsContainer;
		private ZArchitecture.ZTextBox tbLogs;
		private Enterprise.Rating.GUI.RateSelector.UIControls.SortOptionsControl zSortOptionsControl1;
		private CargoWise.Windows.UI.KTableLayoutPanel kTableLayoutPanel1;
	}
}
