using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class PenaltiesControl
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
            this.pnlTop = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblTitle = new Enterprise.ZArchitecture.ZLabel();
            this.pnlColumnTitles = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblCurrency = new Enterprise.ZArchitecture.ZLabel();
            this.lblCostPUnit = new Enterprise.ZArchitecture.ZLabel();
            this.lblUnit = new Enterprise.ZArchitecture.ZLabel();
            this.lblFreeTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblName = new Enterprise.ZArchitecture.ZLabel();
            this.lblType = new Enterprise.ZArchitecture.ZLabel();
            this.lblProcess = new Enterprise.ZArchitecture.ZLabel();
            this.pnlItemsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.pnlColumnTitles.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.BookingInfoViewModel);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTop.Controls.Add(this.lblTitle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 22, true);
            this.pnlTop.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitle.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblTitle.IsFontBold = true;
            this.lblTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 22, true);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Penalty Type Free Time";
            // 
            // pnlColumnTitles
            // 
            this.pnlColumnTitles.Controls.Add(this.lblCurrency);
            this.pnlColumnTitles.Controls.Add(this.lblCostPUnit);
            this.pnlColumnTitles.Controls.Add(this.lblUnit);
            this.pnlColumnTitles.Controls.Add(this.lblFreeTime);
            this.pnlColumnTitles.Controls.Add(this.lblName);
            this.pnlColumnTitles.Controls.Add(this.lblType);
            this.pnlColumnTitles.Controls.Add(this.lblProcess);
            this.pnlColumnTitles.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlColumnTitles.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
            this.pnlColumnTitles.Name = "pnlColumnTitles";
            this.pnlColumnTitles.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 22, true);
            this.pnlColumnTitles.TabIndex = 1;
            // 
            // lblCurrency
            // 
            this.lblCurrency.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrency.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCurrency.IsFontBold = true;
            this.lblCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 0, true);
            this.lblCurrency.Name = "lblCurrency";
            this.lblCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 22, true);
            this.lblCurrency.TabIndex = 7;
            this.lblCurrency.Text = "Currency";
            // 
            // lblCostPUnit
            // 
            this.lblCostPUnit.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCostPUnit.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCostPUnit.IsFontBold = true;
            this.lblCostPUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 0, true);
            this.lblCostPUnit.Name = "lblCostPUnit";
            this.lblCostPUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 22, true);
            this.lblCostPUnit.TabIndex = 6;
            this.lblCostPUnit.Text = "Cost P/Unit";
            // 
            // lblUnit
            // 
            this.lblUnit.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblUnit.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblUnit.IsFontBold = true;
            this.lblUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 0, true);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 22, true);
            this.lblUnit.TabIndex = 5;
            this.lblUnit.Text = "Unit";
            // 
            // lblFreeTime
            // 
            this.lblFreeTime.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFreeTime.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblFreeTime.IsFontBold = true;
            this.lblFreeTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 0, true);
            this.lblFreeTime.Name = "lblFreeTime";
            this.lblFreeTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 22, true);
            this.lblFreeTime.TabIndex = 4;
            this.lblFreeTime.Text = "Free Time";
            // 
            // lblName
            // 
            this.lblName.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblName.IsFontBold = true;
            this.lblName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 0, true);
            this.lblName.Name = "lblName";
            this.lblName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 22, true);
            this.lblName.TabIndex = 3;
            this.lblName.Text = "Name";
            // 
            // lblType
            // 
            this.lblType.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblType.IsFontBold = true;
            this.lblType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 0, true);
            this.lblType.Name = "lblType";
            this.lblType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 22, true);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "Type";
            // 
            // lblProcess
            // 
            this.lblProcess.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblProcess.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblProcess.IsFontBold = true;
            this.lblProcess.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblProcess.Name = "lblProcess";
            this.lblProcess.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 22, true);
            this.lblProcess.TabIndex = 1;
            this.lblProcess.Text = "Process";
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 44, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 140, true);
            this.pnlItemsContainer.TabIndex = 2;
            // 
            // PenaltiesControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlItemsContainer);
            this.Controls.Add(this.pnlColumnTitles);
            this.Controls.Add(this.pnlTop);
            this.Name = "PenaltiesControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 184, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlColumnTitles.ResumeLayout(false);
            this.pnlColumnTitles.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.GUI.ZPanel pnlColumnTitles;
		private ZArchitecture.GUI.ZPanel pnlItemsContainer;
		private ZArchitecture.ZLabel lblTitle;
		private ZArchitecture.ZLabel lblProcess;
		private ZArchitecture.ZLabel lblType;
		private ZArchitecture.ZLabel lblName;
		private ZArchitecture.ZLabel lblFreeTime;
		private ZArchitecture.ZLabel lblUnit;
		private ZArchitecture.ZLabel lblCostPUnit;
		private ZArchitecture.ZLabel lblCurrency;
	}
}
