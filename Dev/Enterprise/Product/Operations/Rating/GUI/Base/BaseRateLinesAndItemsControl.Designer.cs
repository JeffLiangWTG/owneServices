using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class BaseRateLinesAndItemsControl
	{
		protected KSplitContainer splitContainer;
		protected ZPanel zPanel1;
		protected CalculatorPanel calculatorPanel1;
		internal ZGrid RateLinesGrid;
		Container components = null;

		void InitializeComponent()
		{
			this.splitContainer = new KSplitContainer();
			this.zPanel1 = new ZPanel();
			this.RateLinesGrid = new ZGrid();
			this.calculatorPanel1 = new CalculatorPanel();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.zPanel1.SuspendLayout();
			((ISupportInitialize)(this.RateLinesGrid)).BeginInit();
			this.RateLinesGrid.SuspendLayout();
			this.calculatorPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.zPanel1);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.calculatorPanel1);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 160, true);
			this.splitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(456);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(492);
			this.splitContainer.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.RateLinesGrid);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 160, true);
			this.zPanel1.TabIndex = 10;
			// 
			// RateLinesGrid
			// 
			this.RateLinesGrid.AllowDrop = true;
			this.RateLinesGrid.AllowNavigation = false;
			this.RateLinesGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
				| System.Windows.Forms.AnchorStyles.Left
				| System.Windows.Forms.AnchorStyles.Right;
			this.RateLinesGrid.CaptionVisible = false;
			this.RateLinesGrid.GridId = "14a45462-e42d-49a7-b036-cca012f20bf5";
			this.RateLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLinesGrid.LayoutKey = "RateLinesGrid";
			this.RateLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLinesGrid.Name = "RateLinesGrid";
			this.RateLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 160, true);
			this.RateLinesGrid.TabIndex = 4;
			this.RateLinesGrid.DragDrop += new DragEventHandler(this.RateLinesGrid_DragDrop);
			this.RateLinesGrid.DragEnter += new DragEventHandler(this.RateLinesGrid_DragEnter);
			// 
			// calculatorPanel1
			// 
			this.calculatorPanel1.AgentRatesCheckBoxVisible = true;
			this.calculatorPanel1.AllowDrop = true;
			this.calculatorPanel1.BindingMember = null;
			this.calculatorPanel1.CalculatorDropEditVisible = true;
			this.calculatorPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.calculatorPanel1.IsBreakWeightVolumeAvailable = true;
			this.calculatorPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.calculatorPanel1.Name = "calculatorPanel1";
			this.calculatorPanel1.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.calculatorPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 160, true);
			this.calculatorPanel1.TabIndex = 9;
			this.calculatorPanel1.ViewResultsVisible = false;
			// 
			// BaseRateLinesAndItemsControl
			// 
			this.Controls.Add(this.splitContainer);
			this.Name = "BaseRateLinesAndItemsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 160, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			((ISupportInitialize)(this.RateLinesGrid)).EndInit();
			this.RateLinesGrid.ResumeLayout(false);
			this.RateLinesGrid.PerformLayout();
			this.calculatorPanel1.ResumeLayout(true);
			this.calculatorPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
