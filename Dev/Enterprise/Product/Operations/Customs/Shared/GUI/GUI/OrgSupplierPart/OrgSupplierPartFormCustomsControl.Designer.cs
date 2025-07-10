using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GUI
{
	partial class OrgSupplierPartFormCustomsControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected ZGrid PivotGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PivotGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			SuspendLayout();
			// 
			// BindingSource
			// 
			BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierPart);
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			// 
			// PivotGrid
			// 
			this.PivotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PivotGrid, "PivotsForBinding");
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|783E9A22-D42C-47D8-9EE0-8C3B20FC620D", "Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CI_ChildType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.PivotGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PivotGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.PivotGrid.GridId = "3FBB7DD1-2DDA-48E7-B957-614EBB9F2F08";
			this.PivotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PivotGrid.LayoutKey = "PivotGrid";
			this.PivotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PivotGrid.Name = "PivotGrid";
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 130, true);
			this.PivotGrid.TabIndex = 9;
			// 
			// OrgSupplierPartFormCustomsControl
			// 
			CaptionRenderingEnabled = true;
			this.Controls.Add(this.PivotGrid);
			Name = "OrgSupplierPartFormCustomsControl";
			Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 167, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
