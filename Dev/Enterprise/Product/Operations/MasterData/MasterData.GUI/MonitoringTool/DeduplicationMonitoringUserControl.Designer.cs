using System.ComponentModel;

namespace Enterprise.MasterData.GUI
{
	partial class DeduplicationMonitoringUserControl
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
			name.EditorShowing -= new CancelEventHandler(Name_EditorShowing);
			model = null;
			treeView.Dispose();
			treeView = null;

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1094:ToolTipTextShouldBeSetWithResGetString", Justification = "Generated code")]
		void InitializeComponent()
		{
			this.treeColumn1 = new Aga.Controls.Tree.TreeColumn();
			this.treeColumn2 = new Aga.Controls.Tree.TreeColumn();
			this.treeColumn3 = new Aga.Controls.Tree.TreeColumn();
			this.treeView = new Aga.Controls.Tree.TreeViewAdv();
			this.icon = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
			this.name = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.type = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.dValue = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.queryColumn = new Aga.Controls.Tree.TreeColumn();
			this.queries = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// treeColumn1
			// 
			this.treeColumn1.Header = "Execution Path";
			this.treeColumn1.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn1.TooltipText = "Execution Path";
			this.treeColumn1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			// 
			// treeColumn2
			// 
			this.treeColumn2.Header = "Hierarchy";
			this.treeColumn2.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.treeColumn2.TooltipText = "Hierarchy";
			this.treeColumn2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// treeColumn3
			// 
			this.treeColumn3.Header = "Property Value";
			this.treeColumn3.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.treeColumn3.TooltipText = "Property raw value";
			this.treeColumn3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			// 
			// treeView
			// 
			this.treeView.AllowColumnReorder = true;
			this.treeView.AutoRowHeight = true;
			this.treeView.BackColor = System.Drawing.SystemColors.Window;
			this.treeView.Columns.Add(this.treeColumn1);
			this.treeView.Columns.Add(this.treeColumn2);
			this.treeView.Columns.Add(this.treeColumn3);
			this.treeView.Columns.Add(this.queryColumn);
			this.treeView.Cursor = System.Windows.Forms.Cursors.Default;
			this.treeView.DefaultToolTipProvider = null;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.DragDropMarkColor = System.Drawing.Color.Black;
			this.treeView.FullRowSelect = true;
			this.treeView.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle)((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this.treeView.LineColor = System.Drawing.SystemColors.ControlDark;
			this.treeView.LineDashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
			this.treeView.LoadOnDemand = true;
			this.treeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.treeView.Model = null;
			this.treeView.Name = "treeView";
			this.treeView.NodeControls.Add(this.icon);
			this.treeView.NodeControls.Add(this.name);
			this.treeView.NodeControls.Add(this.type);
			this.treeView.NodeControls.Add(this.dValue);
			this.treeView.NodeControls.Add(this.queries);
			this.treeView.SelectedNode = null;
			this.treeView.ShowNodeToolTips = true;
			this.treeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 282, true);
			this.treeView.TabIndex = 0;
			this.treeView.UseColumns = true;
			this.treeView.NodeMouseDoubleClick += TreeView_NodeMouseDoubleClick;
			this.treeView.NodeMouseClick += TreeView_NodeMouseClick;
			// 
			// icon
			// 
			this.icon.DataPropertyName = "Icon";
			this.icon.LeftMargin = 1;
			this.icon.ParentColumn = this.treeColumn1;
			this.icon.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
			// 
			// name
			// 
			this.name.DataPropertyName = "Name";
			this.name.IncrementalSearchEnabled = true;
			this.name.LeftMargin = 3;
			this.name.ParentColumn = this.treeColumn1;
			this.name.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			this.name.UseCompatibleTextRendering = true;
			// 
			// type
			// 
			this.type.DataPropertyName = "DataType";
			this.type.IncrementalSearchEnabled = true;
			this.type.LeftMargin = 3;
			this.type.ParentColumn = this.treeColumn2;
			this.type.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// dValue
			// 
			this.dValue.DataPropertyName = "DataValue";
			this.dValue.IncrementalSearchEnabled = true;
			this.dValue.LeftMargin = 3;
			this.dValue.ParentColumn = this.treeColumn3;
			this.dValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// treeColumn4
			// 
			this.queryColumn.Header = "Queries";
			this.queryColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.queryColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			// 
			// queries
			// 
			this.queries.DataPropertyName = "QueryTitle";
			this.queries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.queries.IncrementalSearchEnabled = true;
			this.queries.LeftMargin = 3;
			this.queries.ParentColumn = this.queryColumn;
			// 
			// DeduplicationMonitoringUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.treeView);
			this.Name = "DeduplicationMonitoringUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 282, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Aga.Controls.Tree.TreeViewAdv treeView;
		private Aga.Controls.Tree.NodeControls.NodeStateIcon icon;
		private Aga.Controls.Tree.NodeControls.NodeTextBox name;
		private Aga.Controls.Tree.NodeControls.NodeTextBox type;
		private Aga.Controls.Tree.NodeControls.NodeTextBox dValue;
		private Aga.Controls.Tree.TreeColumn treeColumn1;
		private Aga.Controls.Tree.TreeColumn treeColumn2;
		private Aga.Controls.Tree.TreeColumn treeColumn3;
		private Aga.Controls.Tree.TreeColumn queryColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox queries;
	}
}
