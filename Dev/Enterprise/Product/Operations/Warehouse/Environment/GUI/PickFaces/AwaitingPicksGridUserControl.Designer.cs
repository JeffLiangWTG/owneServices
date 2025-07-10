using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.GUI.PickFaces
{
	partial class AwaitingPicksGridUserControl
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
		this.AwaitingPicksGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AwaitingPicksGrid)).BeginInit();
            this.AwaitingPicksGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsPickFace);
            // 
            // AwaitingPicksGrid
            // 
            this.AwaitingPicksGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AwaitingPicksGrid, "AwaitingPicks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).AwaitingPicks)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceAwaitingReplenishmentView)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).AwaitingPicks)).SyncRoot)).WWP_PickNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceAwaitingReplenishmentView)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).AwaitingPicks)).SyncRoot)).WWP_PickPriority)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceAwaitingReplenishmentView)(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).AwaitingPicks)).SyncRoot)).WWP_QuantityRequired)));
		this.AwaitingPicksGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("AwaitingPicksGridUserControl|032D763C-BDEC-48CE-8189-823739915077", "Pick No");
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "WWP_PickNo";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.ToolTip = "Pick No";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("AwaitingPicksGridUserControl|9E5DFFB1-C524-4B36-B5E5-EFDA0533FE6B", "Pick Priority");
            zCalcEditColumnStyleInfo1.ColumnName = "WWP_PickPriority";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.ToolTip = "Pick Priority";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Environment.GUI.Res.GetData("AwaitingPicksGridUserControl|D5DD5E57-7321-484E-AB56-F2FC80064165", "Quantity Required");
            zCalcEditColumnStyleInfo2.ColumnName = "WWP_QuantityRequired";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.ToolTip = "Quantity Required";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.AwaitingPicksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.AwaitingPicksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.AwaitingPicksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.AwaitingPicksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AwaitingPicksGrid.GridId = "52FC51BD-2177-4E5F-A95E-3CEEA810080A";
            this.AwaitingPicksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AwaitingPicksGrid.LayoutKey = "AwaitingPicksGrid";
            this.AwaitingPicksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AwaitingPicksGrid.Name = "AwaitingPicksGrid";
            this.AwaitingPicksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1044, 240, true);
            this.AwaitingPicksGrid.TabIndex = 1;
            this.AwaitingPicksGrid.ReadOnly = true;
            // 
            // AwaitingPicksGridUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.AwaitingPicksGrid);
            this.Name = "AwaitingPicksGridUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 160, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AwaitingPicksGrid)).EndInit();
            this.AwaitingPicksGrid.ResumeLayout(false);
            this.AwaitingPicksGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		#endregion
	}
}
