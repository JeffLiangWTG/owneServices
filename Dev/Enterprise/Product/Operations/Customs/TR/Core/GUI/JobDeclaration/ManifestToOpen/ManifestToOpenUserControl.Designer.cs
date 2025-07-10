using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.GUI
{
	partial class ManifestToOpenUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.HeaderGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BillGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ManifestToOpenGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderGrid)).BeginInit();
			this.HeaderGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillGrid)).BeginInit();
			this.BillGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackGrid)).BeginInit();
			this.PackGrid.SuspendLayout();
			this.ManifestToOpenGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider);
			// 
			// HeaderGrid
			// 
			this.HeaderGrid.AllowNavigation = false;
			this.HeaderGrid.AllowSorting = false;
			this.HeaderGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HeaderGrid, "ManifestToOpenHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).CE_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).CE_ExpiryDate)));
			this.HeaderGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HeaderGrid.GridId = "69f97afa-c479-45ee-8a4f-930bc139d976";
			this.HeaderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HeaderGrid.LayoutKey = "HeaderGrid";
			this.HeaderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.HeaderGrid.Name = "HeaderGrid";
			this.HeaderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 108, true);
			this.HeaderGrid.TabIndex = 0;
			// 
			// BillGrid
			// 
			this.BillGrid.AllowNavigation = false;
			this.BillGrid.AllowSorting = false;
			this.BillGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BillGrid, "ManifestToOpenHeaders.Bills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).TPD_DocumentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).TPD_IncludeAllItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).TPD_IsInWarehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).TPD_IsOtherProcedure)));
			this.BillGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "TPD_DocumentNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "TPD_IncludeAllItems";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "TPD_IsInWarehouse";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "TPD_IsOtherProcedure";
			zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BillGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BillGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BillGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.BillGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.BillGrid.GridId = "69f97afa-c479-45ee-8a4f-930bc139d976";
			this.BillGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillGrid.LayoutKey = "BillGrid";
			this.BillGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 133, true);
			this.BillGrid.Name = "BillGrid";
			this.BillGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 108, true);
			this.BillGrid.TabIndex = 1;
			// 
			// PackGrid
			// 
			this.PackGrid.AllowNavigation = false;
			this.PackGrid.AllowSorting = false;
			this.PackGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PackGrid, "ManifestToOpenHeaders.Bills.Packs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).Packs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenPack)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).Packs)).SyncRoot)).TPI_LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenPack)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).Packs)).SyncRoot)).TPI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenPack)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenBill)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.ManifestToOpenHeader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.IManifestToOpenProvider)(null)).ManifestToOpenHeaders)).SyncRoot)).Bills)).SyncRoot)).Packs)).SyncRoot)).TPI_WarehouseCode)));
			this.PackGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TPI_LineNumber";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TPI_Quantity";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TPI_WarehouseCode";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.PackGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PackGrid.GridId = "69f97afa-c479-45ee-8a4f-930bc139d976";
			this.PackGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackGrid.LayoutKey = "PackGrid";
			this.PackGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 247, true);
			this.PackGrid.Name = "PackGrid";
			this.PackGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 115, true);
			this.PackGrid.TabIndex = 2;
			// 
			// ManifestToOpenGroupBox
			// 
			this.ManifestToOpenGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ManifestToOpenGroupBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("472752e4-2792-42e5-b916-f1f77d7dc159", "Manifest to Open");
			this.ManifestToOpenGroupBox.Controls.Add(this.HeaderGrid);
			this.ManifestToOpenGroupBox.Controls.Add(this.PackGrid);
			this.ManifestToOpenGroupBox.Controls.Add(this.BillGrid);
			this.ManifestToOpenGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ManifestToOpenGroupBox.Name = "ManifestToOpenGroupBox";
			this.ManifestToOpenGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 368, true);
			this.ManifestToOpenGroupBox.TabIndex = 0;
			this.ManifestToOpenGroupBox.TabStop = false;
			// 
			// ManifestToOpenUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManifestToOpenGroupBox);
			this.Name = "ManifestToOpenUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 374, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderGrid)).EndInit();
			this.HeaderGrid.ResumeLayout(false);
			this.HeaderGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillGrid)).EndInit();
			this.BillGrid.ResumeLayout(false);
			this.BillGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackGrid)).EndInit();
			this.PackGrid.ResumeLayout(false);
			this.PackGrid.PerformLayout();
			this.ManifestToOpenGroupBox.ResumeLayout(false);
			this.ManifestToOpenGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZArchitecture.ZGrid HeaderGrid;
		private ZArchitecture.ZGrid BillGrid;
		private ZArchitecture.ZGrid PackGrid;
		private ZArchitecture.GUI.ZGroupBox ManifestToOpenGroupBox;
	}
}
