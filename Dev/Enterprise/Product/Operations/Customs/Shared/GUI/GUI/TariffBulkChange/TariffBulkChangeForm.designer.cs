namespace Enterprise.Customs.GUI
{
	partial class TariffBulkChangeForm
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NewTariffsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PartsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NewClassificationsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OldClassificationsZGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OldTariffsZGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 621, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 25, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.TariffBulkChange);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.label1.TabIndex = 30;
			this.label1.Text = Enterprise.Customs.GUI.Res.GetString("AF245143-3C11-4F48-BF44-D918C4CFE467", "Old Tariffs");
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 150, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.label2.TabIndex = 31;
			this.label2.Text = Enterprise.Customs.GUI.Res.GetString("94D83562-3955-431A-B2F6-4C715FF2676D", "New Tariffs");
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 9, true);
			this.label3.Name = "label3";
			this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 13, true);
			this.label3.TabIndex = 32;
			this.label3.Text = Enterprise.Customs.GUI.Res.GetString("EC5FB1C8-009C-4DB7-A96E-04A7E9314202", "Original Lookups (Classifications)");
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 334, true);
			this.label4.Name = "label4";
			this.label4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 13, true);
			this.label4.TabIndex = 34;
			this.label4.Text = Enterprise.Customs.GUI.Res.GetString("86DC04F5-7EEE-4A07-980A-6E978949AC69", "Products for selected Old Tariff");
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 150, true);
			this.label5.Name = "label5";
			this.label5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 13, true);
			this.label5.TabIndex = 8;
			this.label5.Text = Enterprise.Customs.GUI.Res.GetString("A3457F54-0F21-476C-A9A7-A13A78D93956", "New Lookups (Classifications)");
			// 
			// MakeNewClassZButton
			// 
			this.MakeNewClassZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 290, true);
			this.MakeNewClassZButton.Name = "MakeNewClassZButton";
			this.MakeNewClassZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.MakeNewClassZButton.TabIndex = 6;
			this.MakeNewClassZButton.Text = Enterprise.Customs.GUI.Res.GetString("766DF618-7EF2-41D3-960A-6ABB9409060B", "Make New Lookups From New Tariffs");
			this.MakeNewClassZButton.UseVisualStyleBackColor = true;
			this.MakeNewClassZButton.Click += new System.EventHandler(this.MakeNewClassZButton_Click);
			// 
			// UpdateProductsZButton
			// 
			this.UpdateProductsZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 290, true);
			this.UpdateProductsZButton.Name = "UpdateProductsZButton";
			this.UpdateProductsZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.UpdateProductsZButton.TabIndex = 7;
			this.UpdateProductsZButton.Text = Enterprise.Customs.GUI.Res.GetString("23190C95-1C18-450A-992D-891EE4F982C5", "Update Products with Selected New Lookup");
			this.UpdateProductsZButton.UseVisualStyleBackColor = true;
			this.UpdateProductsZButton.Click += new System.EventHandler(this.UpdateProductsZButton_Click);
			// 
			// NewTariffsZGrid
			// 
			this.NewTariffsZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NewTariffsZGrid, "TariffBulkChangeOldTariffs.TariffBulkChangeNewTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffBulkChangeNewTariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeNewTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffBulkChangeNewTariffs)).SyncRoot)).NewTariffNum)));
			this.NewTariffsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B81E47B6-A75E-482B-AD2B-FBC1DAA12B2E", "New Tariff Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.NewTariffsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NewTariffsZGrid.CopySelectedRowsAllowed = true;
			this.NewTariffsZGrid.GridId = "b7cfdd91-5426-4d0d-9e82-d84e9a949dab";
			this.NewTariffsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewTariffsZGrid.LayoutKey = "NewTariffsZGrid";
			this.NewTariffsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 169, true);
			this.NewTariffsZGrid.Name = "NewTariffsZGrid";
			this.NewTariffsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.NewTariffsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 115, true);
			this.NewTariffsZGrid.TabIndex = 3;
			// 
			// PartsZGrid
			// 
			this.PartsZGrid.AllowNavigation = false;
			this.PartsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PartsZGrid, "TariffBulkChangeOldTariffs.TariffItemPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.OP_PartNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.OP_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.AllOwners)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.AllSuppliers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).OldClassificationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).OldTariffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).NewLookUpCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).NewTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).TariffItemPivots)).SyncRoot)).Part.OP_StockKeepingUnit)));
			this.PartsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("581AA581-8FB2-484E-8C29-F39C7FD0CB4B", "Part Number");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "Part+OP_PartNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DC6EF280-05BE-4E27-9406-C73679338A0B", "Product Description");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Part+OP_Desc";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("443FDCBA-5BF4-4384-9F20-A5AEA28FE482", "Importers");
			zTextBoxColumnStyleInfo4.ColumnName = "Part+AllOwners";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("14146FE9-078C-4CBA-9149-A2457D100CDB", "Suppliers");
			zTextBoxColumnStyleInfo5.ColumnName = "Part+AllSuppliers";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("95ED9EDA-BB18-4F83-A8F9-C91CE912AFFE", "Orig. Lookup");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "OldClassificationCode";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F6F2BC61-A75C-4058-9BFF-1F113B58B92F", "Orig. Tariff");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "OldTariffCode";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("38D371A5-3C93-4840-83A9-659A50287AD5", "New Lookup");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "NewLookUpCode";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("35901C96-D088-470B-87BE-37B52EB2140D", "New Tariff");
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3BF621A1-3A0B-470E-AE13-1C792AD55A6E", "Stock Keeping Unit");
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
			this.PartsZGrid.CopySelectedRowsAllowed = true;
			this.PartsZGrid.GridId = "ef07d1e2-f6cd-400c-bdbc-509ac23772ef";
			this.PartsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartsZGrid.LayoutKey = "PartsZGrid";
			this.PartsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 350, true);
			this.PartsZGrid.Name = "PartsZGrid";
			this.PartsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PartsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 235, true);
			this.PartsZGrid.TabIndex = 8;
			// 
			// NewClassificationsZGrid
			// 
			this.NewClassificationsZGrid.AllowNavigation = false;
			this.NewClassificationsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NewClassificationsZGrid, "TariffBulkChangeOldTariffs.NewClassifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_LookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).NewLookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_TariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).NewTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_ClassificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).NewClassifications)).SyncRoot)).CC_AddInfo)));
			this.NewClassificationsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("95ED9EDA-BB18-4F83-A8F9-C91CE912AFFE", "Orig. Lookup");
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "CC_LookupCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("38D371A5-3C93-4840-83A9-659A50287AD5", "New Lookup");
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "NewLookupCode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F6F2BC61-A75C-4058-9BFF-1F113B58B92F", "Orig. Tariff");
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "CC_TariffNum";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("35901C96-D088-470B-87BE-37B52EB2140D", "New Tariff");
			zTextBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5BC2BD4B-7DC8-4841-97E4-8F321A8ED3FA", "Lookup Description");
			zTextBoxColumnStyleInfo15.ColumnName = "CC_Description";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4C6FCE68-C345-48AC-936B-C2995625CC36", "Type");
			zTextBoxColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo16.ColumnName = "CC_ClassificationType";
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6C12225D-4DFE-4294-BA3E-72DA06CEE227", "Add Info");
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
			this.NewClassificationsZGrid.CopySelectedRowsAllowed = true;
			this.NewClassificationsZGrid.GridId = "20d41071-7279-4b8b-bfe7-61d647780589";
			this.NewClassificationsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NewClassificationsZGrid.LayoutKey = "zGrid1";
			this.NewClassificationsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 169, true);
			this.NewClassificationsZGrid.Name = "NewClassificationsZGrid";
			this.NewClassificationsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.NewClassificationsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 115, true);
			this.NewClassificationsZGrid.TabIndex = 4;
			// 
			// OldClassificationsZGrid
			// 
			this.OldClassificationsZGrid.AllowNavigation = false;
			this.OldClassificationsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OldClassificationsZGrid, "TariffBulkChangeOldTariffs.OriginalClassifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_LookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).NewLookupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_TariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).NewTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_ClassificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TBCClassification)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OriginalClassifications)).SyncRoot)).CC_AddInfo)));
			this.OldClassificationsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("95ED9EDA-BB18-4F83-A8F9-C91CE912AFFE", "Orig. Lookup");
			zTextBoxColumnStyleInfo18.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo18.ColumnName = "CC_LookupCode";
			zTextBoxColumnStyleInfo18.IsReadOnly = true;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("38D371A5-3C93-4840-83A9-659A50287AD5", "New Lookup");
			zTextBoxColumnStyleInfo19.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo19.ColumnName = "NewLookupCode";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F6F2BC61-A75C-4058-9BFF-1F113B58B92F", "Orig. Tariff");
			zTextBoxColumnStyleInfo20.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo20.ColumnName = "CC_TariffNum";
			zTextBoxColumnStyleInfo20.IsReadOnly = true;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("35901C96-D088-470B-87BE-37B52EB2140D", "New Tariff");
			zTextBoxColumnStyleInfo21.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo21.ColumnName = "NewTariffNum";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5BC2BD4B-7DC8-4841-97E4-8F321A8ED3FA", "Lookup Description");
			zTextBoxColumnStyleInfo22.ColumnName = "CC_Description";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4C6FCE68-C345-48AC-936B-C2995625CC36", "Type");
			zTextBoxColumnStyleInfo23.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo23.ColumnName = "CC_ClassificationType";
			zTextBoxColumnStyleInfo23.IsReadOnly = true;
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6C12225D-4DFE-4294-BA3E-72DA06CEE227", "Add Info");
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
			this.OldClassificationsZGrid.CopySelectedRowsAllowed = true;
			this.OldClassificationsZGrid.GridId = "77934bb4-2ef0-4cbe-a79f-945e1bd8fbc1";
			this.OldClassificationsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OldClassificationsZGrid.LayoutKey = "OldClassificationsZGrid";
			this.OldClassificationsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 28, true);
			this.OldClassificationsZGrid.Name = "OldClassificationsZGrid";
			this.OldClassificationsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OldClassificationsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 116, true);
			this.OldClassificationsZGrid.TabIndex = 2;
			// 
			// UpdateOriginalLookupsZButton
			// 
			this.UpdateOriginalLookupsZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 290, true);
			this.UpdateOriginalLookupsZButton.Name = "UpdateOriginalLookupsZButton";
			this.UpdateOriginalLookupsZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 38, true);
			this.UpdateOriginalLookupsZButton.TabIndex = 5;
			this.UpdateOriginalLookupsZButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("97DC9C33-8188-4637-AB6B-5363B6A66173", "Update Original Lookups with Selected New Tariff");
			this.UpdateOriginalLookupsZButton.UseVisualStyleBackColor = true;
			this.UpdateOriginalLookupsZButton.Click += new System.EventHandler(this.UpdateOriginalLookups);
			// 
			// OldTariffsZGrid
			// 
			this.OldTariffsZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OldTariffsZGrid, "TariffBulkChangeOldTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.TariffBulkChange.TariffBulkChangeOldTariff)(((System.Collections.IList)(((Enterprise.Customs.Business.TariffBulkChange)(null)).TariffBulkChangeOldTariffs)).SyncRoot)).OldTariffNum)));
			this.OldTariffsZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo25.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2046545C-E944-4763-B5CB-7C514A396417", "Old Tariff Number");
			zTextBoxColumnStyleInfo25.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo25.ColumnName = "OldTariffNum";
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OldTariffsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.OldTariffsZGrid.CopySelectedRowsAllowed = true;
			this.OldTariffsZGrid.GridId = "80ecacda-8f87-4bbf-9a30-c270a54fc795";
			this.OldTariffsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OldTariffsZGrid.LayoutKey = "OldTariffsZGrid";
			this.OldTariffsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 28, true);
			this.OldTariffsZGrid.Name = "OldTariffsZGrid";
			this.OldTariffsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OldTariffsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 116, true);
			this.OldTariffsZGrid.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(770, 591, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 9;
			// 
			// TariffBulkChangeForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
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
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.TariffBulkChange);
			this.DataSourceTypeName = "Enterprise.Customs.Business.TariffBulkChange";
			this.Name = "TariffBulkChangeForm";
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("45567174-6884-46D9-9D34-E53876DADB23", "Tariff Bulk Change");
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
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

