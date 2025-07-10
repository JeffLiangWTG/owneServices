using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class RatesCardControl
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
			if (disposing && components != null)
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
            this.pnlPageNavigation = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.pageNavigator = new Enterprise.Rating.GUI.RateSelection.BaseControls.PageNavigator();
            this.pnlItemsContainer = new CargoWise.Windows.UI.KTableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlPageNavigation.SuspendLayout();
            this.pageNavigator.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.SortableRatesViewModel);
            // 
            // pnlPageNavigation
            // 
            this.pnlPageNavigation.AutoScroll = true;
            this.pnlPageNavigation.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1650, 0, true);
            this.pnlPageNavigation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(91)))), ((int)(((byte)(91)))));
            this.pnlPageNavigation.ColumnCount = 3;
            this.pnlPageNavigation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlPageNavigation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlPageNavigation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlPageNavigation.Controls.Add(this.pageNavigator, 1, 0);
            this.pnlPageNavigation.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlPageNavigation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 146, true);
            this.pnlPageNavigation.Name = "pnlPageNavigation";
            this.pnlPageNavigation.RowCount = 1;
            this.pnlPageNavigation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlPageNavigation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 50, true);
            this.pnlPageNavigation.TabIndex = 1;
            // 
            // pageNavigator
            // 
            this.pageNavigator.AllowDrop = true;
            this.pageNavigator.AutoSize = true;
            this.pageNavigator.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.pageNavigator, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IEnumerable)(((Enterprise.Rating.GUI.RateSelector.Models.SortableRatesViewModel)(null)))));
            this.pageNavigator.ForeColor = System.Drawing.Color.White;
            this.pageNavigator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 3, true);
            this.pageNavigator.MaxPageSize = 20;
            this.pageNavigator.Name = "pageNavigator";
            this.pageNavigator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 28, true);
            this.pageNavigator.TabIndex = 0;
            this.pageNavigator.PageChanged += new System.EventHandler<Enterprise.Rating.GUI.RateSelection.BaseControls.PageChangedEventArgs>(this.pageNavigator_PageChanged);
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.AutoScroll = true;
            this.pnlItemsContainer.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1650, 0, true);
            this.pnlItemsContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.pnlItemsContainer.ColumnCount = 1;
            this.pnlItemsContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlItemsContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
            this.pnlItemsContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
            this.pnlItemsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.RowCount = 1;
            this.pnlItemsContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 146, true);
            this.pnlItemsContainer.TabIndex = 2;
            // 
            // RatesCardControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlItemsContainer);
            this.Controls.Add(this.pnlPageNavigation);
            this.Name = "RatesCardControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 180, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlPageNavigation.ResumeLayout(false);
            this.pnlPageNavigation.PerformLayout();
            this.pageNavigator.ResumeLayout(true);
            this.pageNavigator.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private KTableLayoutPanel pnlPageNavigation;
		private KTableLayoutPanel pnlItemsContainer;
		private RateSelection.BaseControls.PageNavigator pageNavigator;
	}
}
