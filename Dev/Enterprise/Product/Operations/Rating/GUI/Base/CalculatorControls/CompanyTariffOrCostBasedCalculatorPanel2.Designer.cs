namespace Enterprise.Rating.GUI
{
	partial class CompanyTariffOrCostBasedCalculatorPanel2
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.MessageTypeSubtypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageSubtypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.EquipmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.DollarLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.BasePriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BaseChangeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DollarLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MinimumChangeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MinimumCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateLineItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTypeSubtypeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MessageTypeSubtypeGroupBox
			// 
			this.MessageTypeSubtypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MessageTypeSubtypeGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel2|42e2f99c-27c2-4831-af9e-f22197a61f80", "Message Type/Style");
			this.MessageTypeSubtypeGroupBox.Controls.Add(this.MessageSubtypeDropEdit);
			this.MessageTypeSubtypeGroupBox.Controls.Add(this.MessageTypeDropEdit);
			this.MessageTypeSubtypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 58, true);
			this.MessageTypeSubtypeGroupBox.Name = "MessageTypeSubtypeGroupBox";
			this.MessageTypeSubtypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 72, true);
			this.MessageTypeSubtypeGroupBox.TabIndex = 31;
			this.MessageTypeSubtypeGroupBox.TabStop = false;
			// 
			// MessageSubtypeDropEdit
			// 
			this.MessageSubtypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageSubtypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 43, true);
			this.MessageSubtypeDropEdit.Name = "MessageSubtypeDropEdit";
			this.MessageSubtypeDropEdit.PreBoundMaxLength = 3;
			this.MessageSubtypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.MessageSubtypeDropEdit.TabIndex = 1;
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 17, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 3;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.MessageTypeDropEdit.TabIndex = 0;
			// 
			// EquipmentDropEdit
			// 
			this.EquipmentDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EquipmentDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("A9FE2B07-100F-48DF-B2A9-5EA7EDEDEC56", "Drop Mode");
			this.EquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 58, true);
			this.EquipmentDropEdit.Name = "EquipmentDropEdit";
			this.EquipmentDropEdit.PreBoundMaxLength = 3;
			this.EquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.EquipmentDropEdit.TabIndex = 0;
			// 
			// DollarLabel2
			// 
			this.DollarLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DollarLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 9, true);
			this.DollarLabel2.Name = "DollarLabel2";
			this.DollarLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 23, true);
			this.DollarLabel2.TabIndex = 2;
			this.DollarLabel2.Text = "$";
			// 
			// BasePriceCalcEdit
			// 
			this.BasePriceCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BasePriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 9, true);
			this.BasePriceCalcEdit.Name = "BasePriceCalcEdit";
			this.BasePriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.BasePriceCalcEdit.TabIndex = 3;
			this.BasePriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BaseChangeLabel
			// 
			this.BaseChangeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BaseChangeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel2|661e63d3-ced7-4491-acc4-a018c14defa5", "Base Price Change");
			this.BaseChangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 9, true);
			this.BaseChangeLabel.Name = "BaseChangeLabel";
			this.BaseChangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.BaseChangeLabel.TabIndex = 1;
			// 
			// DollarLabel1
			// 
			this.DollarLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DollarLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 32, true);
			this.DollarLabel1.Name = "DollarLabel1";
			this.DollarLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 23, true);
			this.DollarLabel1.TabIndex = 5;
			this.DollarLabel1.Text = "$";
			// 
			// MinimumChangeLabel
			// 
			this.MinimumChangeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MinimumChangeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel2|4bb02a66-0d4e-4427-a73c-55944c073462", "Minimum Change");
			this.MinimumChangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 32, true);
			this.MinimumChangeLabel.Name = "MinimumChangeLabel";
			this.MinimumChangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.MinimumChangeLabel.TabIndex = 4;
			// 
			// MinimumCalcEdit
			// 
			this.MinimumCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MinimumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 32, true);
			this.MinimumCalcEdit.Name = "MinimumCalcEdit";
			this.MinimumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.MinimumCalcEdit.TabIndex = 6;
			this.MinimumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateLineItemsGrid
			// 
			this.RateLineItemsGrid.AllowNavigation = false;
			this.RateLineItemsGrid.AllowSorting = false;
			this.RateLineItemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TM_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TM_Break";
			zCalcEditColumnStyleInfo1.Decimals = 1;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel2|3a3bc6be-f7e9-4d41-b8fc-49403a28ffbe", "Per Unit Change");
			zCalcEditColumnStyleInfo2.ColumnName = "TM_RelevantValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|7cba3a1c-04d3-40bf-8aa4-fc28b2654700", "Reason");
			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RateLineItemsGrid.GridId = "e33464d5-3541-4dbd-aad6-f95228c49934";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridCartageZone";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 160, true);
			this.RateLineItemsGrid.TabIndex = 0;
			// 
			// CompanyTariffOrCostBasedCalculatorPanel2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RateLineItemsGrid);
			this.Controls.Add(this.MessageTypeSubtypeGroupBox);
			this.Controls.Add(this.EquipmentDropEdit);
			this.Controls.Add(this.DollarLabel2);
			this.Controls.Add(this.BasePriceCalcEdit);
			this.Controls.Add(this.BaseChangeLabel);
			this.Controls.Add(this.DollarLabel1);
			this.Controls.Add(this.MinimumChangeLabel);
			this.Controls.Add(this.MinimumCalcEdit);
			this.Name = "CompanyTariffOrCostBasedCalculatorPanel2";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTypeSubtypeGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox MessageTypeSubtypeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth MessageSubtypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth MessageTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth EquipmentDropEdit;
		private Enterprise.ZArchitecture.ZLabel DollarLabel2;
		private Enterprise.ZArchitecture.ZCalcEdit BasePriceCalcEdit;
		private Enterprise.ZArchitecture.ZLabel BaseChangeLabel;
		private Enterprise.ZArchitecture.ZLabel DollarLabel1;
		private Enterprise.ZArchitecture.ZLabel MinimumChangeLabel;
		private Enterprise.ZArchitecture.ZCalcEdit MinimumCalcEdit;
		public Enterprise.ZArchitecture.ZGrid RateLineItemsGrid;
	}
}
