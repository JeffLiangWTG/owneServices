using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;

namespace Enterprise.Customs.NZ.GUI
{
	partial class NZTariffBulkChangeForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.label3 = new CargoWise.Windows.UI.KLabel();
			this.label4 = new CargoWise.Windows.UI.KLabel();
			this.label5 = new CargoWise.Windows.UI.KLabel();
			this.MakeNewClassZButton = new CargoWise.Windows.UI.KButton();
			this.UpdateProductsZButton = new CargoWise.Windows.UI.KButton();
			this.NewTariffsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PartsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewClassificationsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OldClassificationsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpdateOriginalLookupsZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OldTariffsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NewTariffsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PartsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NewClassificationsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OldClassificationsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OldTariffsZGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 622, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.label1.TabIndex = 30;
			this.label1.Text = "Old Tariffs";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 150, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.label2.TabIndex = 31;
			this.label2.Text = "New Tariffs";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 9, true);
			this.label3.Name = "label3";
			this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 13, true);
			this.label3.TabIndex = 32;
			this.label3.Text = "Original Lookups (Classifications)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 334, true);
			this.label4.Name = "label4";
			this.label4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 13, true);
			this.label4.TabIndex = 34;
			this.label4.Text = "Products for selected Old Tariff";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 150, true);
			this.label5.Name = "label5";
			this.label5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 13, true);
			this.label5.TabIndex = 8;
			this.label5.Text = "New Lookups (Classifications)";
			// 
			// MakeNewClassZButton
			// 
			this.MakeNewClassZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 290, true);
			this.MakeNewClassZButton.Name = "MakeNewClassZButton";
			this.MakeNewClassZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.MakeNewClassZButton.TabIndex = 6;
			this.MakeNewClassZButton.Text = "Make New Lookups From New Tariffs";
			this.MakeNewClassZButton.UseVisualStyleBackColor = true;
			this.MakeNewClassZButton.Click += new System.EventHandler(this.MakeNewClassZButton_Click);
			// 
			// UpdateProductsZButton
			// 
			this.UpdateProductsZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 290, true);
			this.UpdateProductsZButton.Name = "UpdateProductsZButton";
			this.UpdateProductsZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.UpdateProductsZButton.TabIndex = 7;
			this.UpdateProductsZButton.Text = "Update Products with Selected New Lookup";
			this.UpdateProductsZButton.UseVisualStyleBackColor = true;
			this.UpdateProductsZButton.Click += new System.EventHandler(this.UpdateProductsZButton_Click);
			// 
			// NewTariffsZGrid
			// 
			this.NewTariffsZGrid.AllowNavigation = false;
			this.NewTariffsZGrid.BindTo = "TariffBulkChangeOldTariffs.TariffBulkChangeNewTariffs";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffBulkChangeNewTariffs)));
			this.NewTariffsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("2B62AC11-7ACD-4A6B-BFAD-4A0208A81924", "New Tariff Num");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.NewTariffsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NewTariffsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewTariffsZGrid.LayoutKey = "NewTariffsZGrid";
			this.NewTariffsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 169, true);
			this.NewTariffsZGrid.Name = "NewTariffsZGrid";
			this.NewTariffsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.NewTariffsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 115, true);
			this.NewTariffsZGrid.TabIndex = 3;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TariffBulkChangeNewTariff)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffBulkChangeNewTariffs)))).NewTariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TariffBulkChangeNewTariff)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffBulkChangeNewTariffs)))).NewTariffNum)));
			// 
			// PartsZGrid
			// 
			this.PartsZGrid.AllowNavigation = false;
			this.PartsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PartsZGrid.BindTo = "TariffBulkChangeOldTariffs.TariffItemPivots";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)));
			this.PartsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("325B98BE-0E53-4987-8E40-FE4FEA145089", "Part Num");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "Part+OP_PartNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("88AD1C61-DD46-40E4-8564-BBB1F9ED1F10", "Product Description");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Part+OP_Desc";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6807EBB0-245E-433E-A907-DAFBB92EBA7B", "Importers");
			zTextBoxColumnStyleInfo4.ColumnName = "Part+AllOwners";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("BCF6203A-59AC-4299-8B15-D3D0F7E0F08F", "Suppliers");
			zTextBoxColumnStyleInfo5.ColumnName = "Part+AllSuppliers";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7F9DD0A1-2C32-4AA8-B235-70C40BB1D7C7", "Orig. Lookup");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "OldClassificationCode";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("25E252DE-640C-4C37-8E3A-2D898F091232", "Orig. Tariff");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "OldTariffCode";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8A02CDB4-D0DD-464E-BA2A-AD5F862CC44A", "New Lookup");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "NewLookUpCode";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6FBFFAEF-EA30-4194-8E32-74B860BA547D", "New Tariff");
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("14BFB887-E832-4AD1-8933-97953D6B0E92", "Stock Keeping Unit");
			zTextBoxColumnStyleInfo10.ColumnName = "Part+OP_StockKeepingUnit";
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.PartsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.PartsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartsZGrid.LayoutKey = "PartsZGrid";
			this.PartsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 350, true);
			this.PartsZGrid.Name = "PartsZGrid";
			this.PartsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PartsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 232, true);
			this.PartsZGrid.TabIndex = 8;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.OP_PartNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.OP_PartNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.OP_DescInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.OP_Desc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.AllOwnersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.AllOwners)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.AllSuppliersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.AllSuppliers)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).OldClassificationCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).OldClassificationCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).OldTariffCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).OldTariffCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).NewLookUpCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).NewLookUpCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).NewTariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).NewTariffNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.OP_StockKeepingUnitInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CusClassPartPivot)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).TariffItemPivots)))).Part.OP_StockKeepingUnit)));
			// 
			// NewClassificationsZGrid
			// 
			this.NewClassificationsZGrid.AllowNavigation = false;
			this.NewClassificationsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.NewClassificationsZGrid.BindTo = "TariffBulkChangeOldTariffs.NewClassifications";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)));
			this.NewClassificationsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6D73A237-80EF-45C7-B4A9-4E3134BBF9B0", "Orig. Lookup");
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "CC_LookupCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("39F4FF2E-DA42-4623-A33F-8F5B940B7007", "New Lookup");
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "NewLookupCode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("736E1B08-5369-48E2-9A72-3B5E383DD961", "Orig. Tariff");
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "CC_TariffNum";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("AABB8C5C-0361-4B7E-908F-DCF134884AFE", "New Tariff");
			zTextBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("1770952C-F492-4B66-9238-B669896EE6AD", "Lookup Description");
			zTextBoxColumnStyleInfo15.ColumnName = "CC_Description";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("2F0DFB1E-AE89-4783-839B-E3579C420B42", "Type");
			zTextBoxColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo16.ColumnName = "CC_ClassificationType";
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("E75B923B-8419-45E4-8ECB-71202C034167", "Add Info");
			zTextBoxColumnStyleInfo17.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo17.ColumnName = "CC_AddInfo";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.NewClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.NewClassificationsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewClassificationsZGrid.LayoutKey = "zGrid1";
			this.NewClassificationsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 169, true);
			this.NewClassificationsZGrid.Name = "NewClassificationsZGrid";
			this.NewClassificationsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.NewClassificationsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 115, true);
			this.NewClassificationsZGrid.TabIndex = 4;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_LookupCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_LookupCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).NewLookupCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).NewLookupCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_TariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_TariffNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).NewTariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).NewTariffNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_Description)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_ClassificationTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_ClassificationType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_AddInfoInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).NewClassifications)))).CC_AddInfo)));
			// 
			// OldClassificationsZGrid
			// 
			this.OldClassificationsZGrid.AllowNavigation = false;
			this.OldClassificationsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OldClassificationsZGrid.BindTo = "TariffBulkChangeOldTariffs.OriginalClassifications";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)));
			this.OldClassificationsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("9D01530E-F3E1-488C-8001-46A81F8BBF18", "Orig. Lookup");
			zTextBoxColumnStyleInfo18.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo18.ColumnName = "CC_LookupCode";
			zTextBoxColumnStyleInfo18.IsReadOnly = true;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("024C9FEC-54D8-4205-A27D-811211DF74DF", "New Lookup");
			zTextBoxColumnStyleInfo19.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo19.ColumnName = "NewLookupCode";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8CFD760F-28ED-44E9-8EB4-545C259A6CAA", "Orig. Tariff");
			zTextBoxColumnStyleInfo20.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo20.ColumnName = "CC_TariffNum";
			zTextBoxColumnStyleInfo20.IsReadOnly = true;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("F0768DFD-161E-4D24-95B0-35F2F7469130", "New Tariff");
			zTextBoxColumnStyleInfo21.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo21.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("A4F21F92-37A5-4924-B2DA-2789523ADF9A", "Lookup Description");
			zTextBoxColumnStyleInfo22.ColumnName = "CC_Description";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("31DF3CE8-47D9-4680-B972-8D869DA2A1F6", "Type");
			zTextBoxColumnStyleInfo23.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo23.ColumnName = "CC_ClassificationType";
			zTextBoxColumnStyleInfo23.IsReadOnly = true;
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CBAA25C4-FDE7-4818-A8CB-C16E5B3B8730", "Add Info");
			zTextBoxColumnStyleInfo24.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo24.ColumnName = "CC_AddInfo";
			zTextBoxColumnStyleInfo24.IsVisible = false;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.OldClassificationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.OldClassificationsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OldClassificationsZGrid.LayoutKey = "OldClassificationsZGrid";
			this.OldClassificationsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 28, true);
			this.OldClassificationsZGrid.Name = "OldClassificationsZGrid";
			this.OldClassificationsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OldClassificationsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 116, true);
			this.OldClassificationsZGrid.TabIndex = 2;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_LookupCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_LookupCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).NewLookupCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).NewLookupCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_TariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_TariffNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).NewTariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).NewTariffNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_Description)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_ClassificationTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_ClassificationType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_AddInfoInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TBCClassification)(((object)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OriginalClassifications)))).CC_AddInfo)));
			// 
			// UpdateOriginalLookupsZButton
			// 
			this.UpdateOriginalLookupsZButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("E643C229-615A-4B1A-A685-E388CB5E09C8", "Update Original Lookups with Selected New Tariff");
			this.UpdateOriginalLookupsZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 290, true);
			this.UpdateOriginalLookupsZButton.Name = "UpdateOriginalLookupsZButton";
			this.UpdateOriginalLookupsZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.UpdateOriginalLookupsZButton.TabIndex = 5;
			this.UpdateOriginalLookupsZButton.UseVisualStyleBackColor = true;
			this.UpdateOriginalLookupsZButton.Click += new System.EventHandler(this.UpdateOriginalLookups);
			// 
			// OldTariffsZGrid
			// 
			this.OldTariffsZGrid.AllowNavigation = false;
			this.OldTariffsZGrid.BindTo = "TariffBulkChangeOldTariffs";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)));
			this.OldTariffsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo25.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("9D93C905-B278-4217-96B9-EB980D23DAE2", "Old Tariff Num");
			zTextBoxColumnStyleInfo25.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo25.ColumnName = "OldTariffNum";
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OldTariffsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.OldTariffsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OldTariffsZGrid.LayoutKey = "OldTariffsZGrid";
			this.OldTariffsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 28, true);
			this.OldTariffsZGrid.Name = "OldTariffsZGrid";
			this.OldTariffsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OldTariffsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 116, true);
			this.OldTariffsZGrid.TabIndex = 1;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OldTariffNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((NZTariffBulkChange.TariffBulkChangeOldTariff)(((object)(((NZTariffBulkChange)(null)).TariffBulkChangeOldTariffs)))).OldTariffNum)));
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 589, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 9;
			// 
			// NZTariffBulkChangeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 646, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OldTariffsZGrid);
			this.Controls.Add(this.UpdateOriginalLookupsZButton);
			this.Controls.Add(this.NewClassificationsZGrid);
			this.Controls.Add(this.OldClassificationsZGrid);
			this.Controls.Add(this.PartsZGrid);
			this.Controls.Add(this.NewTariffsZGrid);
			this.Controls.Add(this.UpdateProductsZButton);
			this.Controls.Add(this.MakeNewClassZButton);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceTypeName = "NZTariffBulkChange";
			this.Name = "NZTariffBulkChangeForm";
			this.Text = "Tariff Bulk Change";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.label5, 0);
			this.Controls.SetChildIndex(this.MakeNewClassZButton, 0);
			this.Controls.SetChildIndex(this.UpdateProductsZButton, 0);
			this.Controls.SetChildIndex(this.NewTariffsZGrid, 0);
			this.Controls.SetChildIndex(this.PartsZGrid, 0);
			this.Controls.SetChildIndex(this.OldClassificationsZGrid, 0);
			this.Controls.SetChildIndex(this.NewClassificationsZGrid, 0);
			this.Controls.SetChildIndex(this.UpdateOriginalLookupsZButton, 0);
			this.Controls.SetChildIndex(this.OldTariffsZGrid, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NewTariffsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PartsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NewClassificationsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OldClassificationsZGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OldTariffsZGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KLabel label1;
		private CargoWise.Windows.UI.KLabel label2;
		private CargoWise.Windows.UI.KLabel label3;
		private CargoWise.Windows.UI.KLabel label4;
		private CargoWise.Windows.UI.KLabel label5;
		private CargoWise.Windows.UI.KButton MakeNewClassZButton;
		private CargoWise.Windows.UI.KButton UpdateProductsZButton;
		private Enterprise.ZArchitecture.ZGrid NewTariffsZGrid;
		private Enterprise.ZArchitecture.ZGrid PartsZGrid;
		private Enterprise.ZArchitecture.ZGrid NewClassificationsZGrid;
		private Enterprise.ZArchitecture.ZGrid OldClassificationsZGrid;
		private Enterprise.ZArchitecture.GUI.ZButton UpdateOriginalLookupsZButton;
		private Enterprise.ZArchitecture.ZGrid OldTariffsZGrid;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
	}
}

