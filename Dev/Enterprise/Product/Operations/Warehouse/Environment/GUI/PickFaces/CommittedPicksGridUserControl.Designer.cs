using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.GUI.PickFaces
{
	partial class CommittedPicksGridUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
		this.CommittedPicksGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CommittedPicksGrid)).BeginInit();
            this.CommittedPicksGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsPickFace);
            // 
            // CommittedPicksGrid
            // 
            this.CommittedPicksGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.CommittedPicksGrid, "CommittedPicks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).CommittedPicks)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceCommittedStockView)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).CommittedPicks)).SyncRoot)).WCP_PickNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceCommittedStockView)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).CommittedPicks)).SyncRoot)).WCP_PickPriority)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceCommittedStockView)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).CommittedPicks)).SyncRoot)).WCP_QuantityCommitted)));
		this.CommittedPicksGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("CommittedPicksGridUserControl|f2ee0cce-ee81-403c-b82f-0f901c74b000", "Pick No");
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "WCP_PickNo";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.ToolTip = "Pick No";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("CommittedPicksGridUserControl|D5FF582C-80D4-4786-8721-EDC9370DCECB", "Pick Priority");
            zCalcEditColumnStyleInfo1.ColumnName = "WCP_PickPriority";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.ToolTip = "Pick Priority";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("CommittedPicksGridUserControl|2E1E27DF-26F9-4792-A40F-8CAE2DFC90F5", "Quantity Committed");
            zCalcEditColumnStyleInfo2.ColumnName = "WCP_QuantityCommitted";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.ToolTip = "Quantity Committed";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.CommittedPicksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.CommittedPicksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.CommittedPicksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.CommittedPicksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommittedPicksGrid.GridId = "4f231c15-7d91-4e6e-ae26-d8e6d8384cbe";
            this.CommittedPicksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CommittedPicksGrid.LayoutKey = "CommittedPicksGrid";
            this.CommittedPicksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CommittedPicksGrid.Name = "CommittedPicksGrid";
            this.CommittedPicksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 226, true);
            this.CommittedPicksGrid.TabIndex = 1;
            this.CommittedPicksGrid.ReadOnly = true;
            // 
            // CommittedPicksGridUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.CommittedPicksGrid);
            this.Name = "CommittedPicksGridUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 151, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CommittedPicksGrid)).EndInit();
            this.CommittedPicksGrid.ResumeLayout(false);
            this.CommittedPicksGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}
