namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class WhsReceiveUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PutawayGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductionRulesEngineLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ReceiveParamsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PutawayGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReceiveParamsGrid)).BeginInit();
			this.ReceiveParamsGrid.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsClientParams);
			// 
			// PutawayGroupBox
			// 
			this.PutawayGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsReceiveUserControl|c4e1d866-91f4-4a8c-8f80-422fbe372b73", "Putaway");
			this.PutawayGroupBox.Controls.Add(this.ProductionRulesEngineLinkLabel);
			this.PutawayGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.PutawayGroupBox.Name = "PutawayGroupBox";
			this.PutawayGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 46, true);
			this.PutawayGroupBox.TabIndex = 2;
			this.PutawayGroupBox.TabStop = false;
			// 
			// ProductionRulesEngineLinkLabel
			// 
			this.ProductionRulesEngineLinkLabel.AutoSize = true;
			this.ProductionRulesEngineLinkLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsReceiveUserControl|98edce02-a7e9-41e2-bd78-fef5254be4c2", "Setup Putaway Rules");
			this.ProductionRulesEngineLinkLabel.IsFontBold = false;
			this.ProductionRulesEngineLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 21, true);
			this.ProductionRulesEngineLinkLabel.Name = "ProductionRulesEngineLinkLabel";
			this.ProductionRulesEngineLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 13, true);
			this.ProductionRulesEngineLinkLabel.TabIndex = 0;
			this.ProductionRulesEngineLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ProductionRulesEngineLinkLabel_LinkClicked);
			// 
			// ReceiveParamsGrid
			// 
			this.ReceiveParamsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReceiveParamsGrid, "ClientParametersByWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_WW_Whs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_ReceiveCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_PrintPalletIDDuringUnload)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_EnforcePalletIDEntry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_PreventReceivingOvers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_ReceiveOverageTolerancePercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_CycleCountOnAlternatePutaway)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientParameterByWarehouse)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).ClientParametersByWarehouse)).SyncRoot)).WY_ValidatePalletIDAsSSCCOnUnload)));
			this.ReceiveParamsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WY_WW_Whs";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "WY_ReceiveCategory";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo5.ColumnName = "WY_PrintPalletIDDuringUnload";
			zCheckBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.ColumnName = "WY_EnforcePalletIDEntry";
			zCheckBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo7.ColumnName = "WY_PreventReceivingOvers";
			zCheckBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "WY_ReceiveOverageTolerancePercent";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo8.ColumnName = "WY_CycleCountOnAlternatePutaway";
			zCheckBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127);
			zCheckBoxColumnStyleInfo9.ColumnName = "WY_ValidatePalletIDAsSSCCOnUnload";
			zCheckBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127);
			this.ReceiveParamsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ReceiveParamsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ReceiveParamsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.ReceiveParamsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.ReceiveParamsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.ReceiveParamsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ReceiveParamsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.ReceiveParamsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.ReceiveParamsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiveParamsGrid.GridId = "df7f9023-4a87-47c6-8db2-75c7c0120287";
			this.ReceiveParamsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReceiveParamsGrid.LayoutKey = "zGrid1";
			this.ReceiveParamsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.ReceiveParamsGrid.Name = "ReceiveParamsGrid";
			this.ReceiveParamsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 176, true);
			this.ReceiveParamsGrid.TabIndex = 3;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsReceiveUserControl|a89c0d98-cbef-4354-af4e-9d121080b2f8", "Receive Params. by Warehouse", "Receive Parameters by Warehouse");
			this.zGroupBox1.Controls.Add(this.ReceiveParamsGrid);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 133, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 213, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsReceiveUserControl|bbd4b48a-c9aa-45b9-aee0-58e2be958b0a", "Receive Params.", "Receive Parameters");
			this.zGroupBox2.Controls.Add(this.zCheckBox1);
			this.zGroupBox2.Controls.Add(this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 63, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 62, true);
			this.zGroupBox2.TabIndex = 5;
			this.zGroupBox2.TabStop = false;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.zCheckBox1, "Client.MiscServ.OM_WhsGenerateSSCCOnInbound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).Client.MiscServ.OM_WhsGenerateSSCCOnInbound)));
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 24, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 24, true);
			this.zCheckBox1.TabIndex = 0;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// CheckReceiveOfProductWithoutWeightAndDimsDropEdit
			//
			this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit, "Client.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Business.WhsClientParams)(null)).Client.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive)));
			this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 28, true);
			this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit.Name = "CheckReceiveOfProductWithoutWeightAndDimsDropEdit";
			this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 24, true);
			this.CheckReceiveOfProductWithoutWeightAndDimsDropEdit.TabIndex = 1;
			// 
			// WhsReceiveUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.PutawayGroupBox);
			this.Name = "WhsReceiveUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 366, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PutawayGroupBox.ResumeLayout(false);
			this.PutawayGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReceiveParamsGrid)).EndInit();
			this.ReceiveParamsGrid.ResumeLayout(false);
			this.ReceiveParamsGrid.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZGroupBox PutawayGroupBox;
		private Enterprise.ZArchitecture.ZGrid ReceiveParamsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel ProductionRulesEngineLinkLabel;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CheckReceiveOfProductWithoutWeightAndDimsDropEdit;
	}
}
