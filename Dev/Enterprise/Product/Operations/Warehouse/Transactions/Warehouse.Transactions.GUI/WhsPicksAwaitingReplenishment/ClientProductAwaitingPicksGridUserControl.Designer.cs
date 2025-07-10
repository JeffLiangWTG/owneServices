namespace Enterprise.Warehouse.Environment.GUI.PickFaces
{
	partial class ClientProductAwaitingPicksGridUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.PicksAwaitingGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.PicksAwaitingGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ClientProductGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ClientProductGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PicksAwaitingGridGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicksAwaitingGrid)).BeginInit();
            this.PicksAwaitingGrid.SuspendLayout();
            this.ClientProductGridGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClientProductGrid)).BeginInit();
            this.ClientProductGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsPick);
            // 
            // PicksAwaitingGridGroupBox
            // 
            this.PicksAwaitingGridGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ClientProductAwaitingPicksGridUserControl|1304D11E-D58D-4DF6-ABC4-E20F37657A2D", "Picks Awaiting Replenishment");
            this.PicksAwaitingGridGroupBox.Controls.Add(this.PicksAwaitingGrid);
            this.PicksAwaitingGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicksAwaitingGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 127, true);
            this.PicksAwaitingGridGroupBox.Name = "PicksAwaitingGridGroupBox";
            this.PicksAwaitingGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 281, true);
            this.PicksAwaitingGridGroupBox.TabIndex = 1;
            this.PicksAwaitingGridGroupBox.TabStop = false;
            // 
            // PicksAwaitingGrid
            // 
            this.PicksAwaitingGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.PicksAwaitingGrid, "DistinctClientProductPickFacesInWhsAwaitingReplenishment.PickFacesAwaitingRepleni" +
        "shmentWithSameWhsClientProduct");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.PickFaceProductsAwaitingReplenishment)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DistinctClientProductPickFacesInWhsAwaitingReplenishment)).SyncRoot)).PickFacesAwaitingReplenishmentWithSameWhsClientProduct)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceAwaitingReplenishmentView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.PickFaceProductsAwaitingReplenishment)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DistinctClientProductPickFacesInWhsAwaitingReplenishment)).SyncRoot)).PickFacesAwaitingReplenishmentWithSameWhsClientProduct)).SyncRoot)).WWP_PickNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceAwaitingReplenishmentView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.PickFaceProductsAwaitingReplenishment)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DistinctClientProductPickFacesInWhsAwaitingReplenishment)).SyncRoot)).PickFacesAwaitingReplenishmentWithSameWhsClientProduct)).SyncRoot)).WWP_PickPriority)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsPickFaceAwaitingReplenishmentView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.PickFaceProductsAwaitingReplenishment)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DistinctClientProductPickFacesInWhsAwaitingReplenishment)).SyncRoot)).PickFacesAwaitingReplenishmentWithSameWhsClientProduct)).SyncRoot)).WWP_QuantityRequired)));
            this.PicksAwaitingGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ClientProductAwaitingPicksGridUserControl|54989022-8024-4781-90EC-B9A205006E69", "Pick No");
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "WWP_PickNo";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.ToolTip = "Pick No";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ClientProductAwaitingPicksGridUserControl|79F784E6-3452-477E-ABD6-1A7C716DA3BA", "Pick Priority");
            zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCalcEditColumnStyleInfo1.ColumnName = "WWP_PickPriority";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.ToolTip = "Pick Priority";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ClientProductAwaitingPicksGridUserControl|EAC6EA5C-E21A-48D6-88F0-C55CBE446723", "Quantity Required");
            zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCalcEditColumnStyleInfo2.ColumnName = "WWP_QuantityRequired";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.ToolTip = "Quantity Required";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.PicksAwaitingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.PicksAwaitingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.PicksAwaitingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.PicksAwaitingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicksAwaitingGrid.GridId = "DB564F71-D4DD-48C5-9767-526EE5B94D5E";
            this.PicksAwaitingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.PicksAwaitingGrid.LayoutKey = "PicksAwaitingGrid";
            this.PicksAwaitingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
            this.PicksAwaitingGrid.Name = "PicksAwaitingGrid";
            this.PicksAwaitingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 244, true);
            this.PicksAwaitingGrid.TabIndex = 3;
            this.PicksAwaitingGrid.ReadOnly = true;
            // 
            // ClientProductGridGroupBox
            // 
            this.ClientProductGridGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ClientProductAwaitingPicksGridUserControl|737B1F78-C9F9-404E-92EE-147A0946C589", "Pick Face Products Requiring Replenishment");
            this.ClientProductGridGroupBox.Controls.Add(this.ClientProductGrid);
            this.ClientProductGridGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ClientProductGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ClientProductGridGroupBox.Name = "ClientProductGridGroupBox";
            this.ClientProductGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 127, true);
            this.ClientProductGridGroupBox.TabIndex = 2;
            this.ClientProductGridGroupBox.TabStop = false;
            // 
            // ClientProductGrid
            // 
            this.ClientProductGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ClientProductGrid, "DistinctClientProductPickFacesInWhsAwaitingReplenishment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DistinctClientProductPickFacesInWhsAwaitingReplenishment)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.PickFaceProductsAwaitingReplenishment)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DistinctClientProductPickFacesInWhsAwaitingReplenishment)).SyncRoot)).ClientCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.PickFaceProductsAwaitingReplenishment)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsPick)(null)).DistinctClientProductPickFacesInWhsAwaitingReplenishment)).SyncRoot)).ProductCode)));
            this.ClientProductGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ClientProductAwaitingPicksGridUserControl|6857E585-A9F2-4826-BDE3-0F986B989412", "Client");
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "ClientCode";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.ToolTip = "Client";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ClientProductAwaitingPicksGridUserControl|1674166E-1472-4570-A880-8F01AA568628", "Product");
            zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo3.ColumnName = "ProductCode";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.ToolTip = "Product";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.ClientProductGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ClientProductGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ClientProductGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClientProductGrid.GridId = "1674166E-1472-4570-A880-8F01AA568628";
            this.ClientProductGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ClientProductGrid.LayoutKey = "ClientProductGrid";
            this.ClientProductGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
            this.ClientProductGrid.Name = "ClientProductGrid";
            this.ClientProductGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 90, true);
            this.ClientProductGrid.TabIndex = 1;
            this.ClientProductGrid.ReadOnly = true;
            // 
            // ClientProductAwaitingPicksGridUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PicksAwaitingGridGroupBox);
            this.Controls.Add(this.ClientProductGridGroupBox);
            this.Name = "ClientProductAwaitingPicksGridUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 408, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PicksAwaitingGridGroupBox.ResumeLayout(false);
            this.PicksAwaitingGridGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicksAwaitingGrid)).EndInit();
            this.PicksAwaitingGrid.ResumeLayout(false);
            this.PicksAwaitingGrid.PerformLayout();
            this.ClientProductGridGroupBox.ResumeLayout(false);
            this.ClientProductGridGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClientProductGrid)).EndInit();
            this.ClientProductGrid.ResumeLayout(false);
            this.ClientProductGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid PicksAwaitingGrid;
		public ZArchitecture.GUI.ZGroupBox ClientProductGridGroupBox;
		public ZArchitecture.ZGrid ClientProductGrid;
	}
}
