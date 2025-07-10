namespace Enterprise.Rating.GUI
{
	partial class CompanyTariffOrCostBasedCalculatorPanel1
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
			this.MessageTypeSubtypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageSubtypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.EquipmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.PercentLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.DollarLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.DollarLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.UnitPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitChangeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CalculationOrderDropDown = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.CalculationOrderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentageChangeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BasePriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BaseChangeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DollarLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MinimumChangeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MinimumCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitPercentageChangePercentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnitPercentageChangeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitPercentageChangeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTypeSubtypeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessageTypeSubtypeGroupBox
			// 
			this.MessageTypeSubtypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageTypeSubtypeGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel1|614afd24-4008-4176-8261-ee7e6d17196f", "Message Type/Style");
			this.MessageTypeSubtypeGroupBox.Controls.Add(this.MessageSubtypeDropEdit);
			this.MessageTypeSubtypeGroupBox.Controls.Add(this.MessageTypeDropEdit);
			this.MessageTypeSubtypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 1, true);
			this.MessageTypeSubtypeGroupBox.Name = "MessageTypeSubtypeGroupBox";
			this.MessageTypeSubtypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 72, true);
			this.MessageTypeSubtypeGroupBox.TabIndex = 31;
			this.MessageTypeSubtypeGroupBox.TabStop = false;
			// 
			// MessageSubtypeDropEdit
			// 
			this.MessageSubtypeDropEdit.AllowDrop = true;
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
			this.MessageTypeDropEdit.AllowDrop = true;
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
			this.EquipmentDropEdit.AllowDrop = true;
			this.EquipmentDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.EquipmentDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("33A5041E-3BF5-4E51-B557-763F0296284C", "Drop Mode");
			this.EquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 1, true);
			this.EquipmentDropEdit.Name = "EquipmentDropEdit";
			this.EquipmentDropEdit.PreBoundMaxLength = 3;
			this.EquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.EquipmentDropEdit.TabIndex = 0;
			// 
			// PercentLabel1
			// 
			this.PercentLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 1, true);
			this.PercentLabel1.Name = "PercentLabel1";
			this.PercentLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.PercentLabel1.Text = "%";
			// 
			// DollarLabel3
			// 
			this.DollarLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 73, true);
			this.DollarLabel3.Name = "DollarLabel3";
			this.DollarLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 23, true);
			this.DollarLabel3.Text = "$";
			// 
			// DollarLabel2
			// 
			this.DollarLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 25, true);
			this.DollarLabel2.Name = "DollarLabel2";
			this.DollarLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 23, true);
			this.DollarLabel2.Text = "$";
			// 
			// UnitPriceCalcEdit
			// 
			this.UnitPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 73, true);
			this.UnitPriceCalcEdit.Name = "UnitPriceCalcEdit";
			this.UnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.UnitPriceCalcEdit.TabIndex = 24;
			this.UnitPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitChangeLabel
			// 
			this.UnitChangeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel1|7b665c15-4f35-43db-9da0-1ca037438375", "Unit Price Change");
			this.UnitChangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 73, true);
			this.UnitChangeLabel.Name = "UnitChangeLabel";
			this.UnitChangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			// 
			// CalculationOrderDropDown
			// 
			this.CalculationOrderDropDown.AllowDrop = true;
			this.CalculationOrderDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 119, true);
			this.CalculationOrderDropDown.Name = "CalculationOrderDropDown";
			this.CalculationOrderDropDown.PreBoundMaxLength = 3;
			this.CalculationOrderDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CalculationOrderDropDown.TabIndex = 29;
			// 
			// CalculationOrderLabel
			// 
			this.CalculationOrderLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel1|99f16377-6859-454e-a34f-549aca6e0d43", "Calculation Order");
			this.CalculationOrderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 119, true);
			this.CalculationOrderLabel.Name = "CalculationOrderLabel";
			this.CalculationOrderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			// 
			// PercentageCalcEdit
			// 
			this.PercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 1, true);
			this.PercentageCalcEdit.Name = "PercentageCalcEdit";
			this.PercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.PercentageCalcEdit.TabIndex = 17;
			this.PercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PercentageLabel
			// 
			this.PercentageChangeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel1|000ffe19-1a14-460a-930f-b7860d26a7ac", "Percentage Change");
			this.PercentageChangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1, true);
			this.PercentageChangeLabel.Name = "PercentageChangeLabel";
			this.PercentageChangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			// 
			// BasePriceCalcEdit
			// 
			this.BasePriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 25, true);
			this.BasePriceCalcEdit.Name = "BasePriceCalcEdit";
			this.BasePriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.BasePriceCalcEdit.TabIndex = 21;
			this.BasePriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BaseChangeLabel
			// 
			this.BaseChangeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel1|3b0f6f38-6485-4c72-9019-3e35371ee66e", "Base Price Change");
			this.BaseChangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.BaseChangeLabel.Name = "BaseChangeLabel";
			this.BaseChangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			// 
			// DollarLabel1
			// 
			this.DollarLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 96, true);
			this.DollarLabel1.Name = "DollarLabel1";
			this.DollarLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 23, true);
			this.DollarLabel1.Text = "$";
			// 
			// MinimumChangeLabel
			// 
			this.MinimumChangeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel1|c88f6d8f-4f56-42c7-b705-445fd27c15c2", "Minimum Change");
			this.MinimumChangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 96, true);
			this.MinimumChangeLabel.Name = "MinimumChangeLabel";
			this.MinimumChangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			// 
			// MinimumCalcEdit
			// 
			this.MinimumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 96, true);
			this.MinimumCalcEdit.Name = "MinimumCalcEdit";
			this.MinimumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.MinimumCalcEdit.TabIndex = 27;
			this.MinimumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//// 
			//// UnitPercentageChangeCalcEdit
			//// 
			this.UnitPercentageChangeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 49, true);
			this.UnitPercentageChangeCalcEdit.Name = "UnitPercentageChangeCalcEdit";
			this.UnitPercentageChangeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.UnitPercentageChangeCalcEdit.TabIndex = 23;
			// 
			// UnitPercentageChangeLabel
			// 
			this.UnitPercentageChangeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CompanyTariffOrCostBasedCalculatorPanel1|47952B33-253A-49FA-9DA7-73E8A5C3D04F", "Unit Percentage Change");
			this.UnitPercentageChangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 49, true);
			this.UnitPercentageChangeLabel.Name = "UnitPercentageChangeLabel";
			this.UnitPercentageChangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			// 
			// UnitPercentageChangePercentLabel
			// 
			this.UnitPercentageChangePercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 49, true);
			this.UnitPercentageChangePercentLabel.Name = "UnitPercentageChangePercentLabel";
			this.UnitPercentageChangePercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.UnitPercentageChangePercentLabel.Text = "%";
			// 
			// CompanyTariffOrCostBasedCalculatorPanel1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MessageTypeSubtypeGroupBox);
			this.Controls.Add(this.EquipmentDropEdit);
			this.Controls.Add(this.PercentLabel1);
			this.Controls.Add(this.DollarLabel3);
			this.Controls.Add(this.DollarLabel2);
			this.Controls.Add(this.UnitPriceCalcEdit);
			this.Controls.Add(this.UnitChangeLabel);
			this.Controls.Add(this.CalculationOrderDropDown);
			this.Controls.Add(this.CalculationOrderLabel);
			this.Controls.Add(this.PercentageCalcEdit);
			this.Controls.Add(this.PercentageChangeLabel);
			this.Controls.Add(this.BasePriceCalcEdit);
			this.Controls.Add(this.BaseChangeLabel);
			this.Controls.Add(this.DollarLabel1);
			this.Controls.Add(this.MinimumChangeLabel);
			this.Controls.Add(this.MinimumCalcEdit);
			this.Controls.Add(this.UnitPercentageChangeCalcEdit);
			this.Controls.Add(this.UnitPercentageChangeLabel);
			this.Controls.Add(this.UnitPercentageChangePercentLabel);
			this.Name = "CompanyTariffOrCostBasedCalculatorPanel1";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTypeSubtypeGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox MessageTypeSubtypeGroupBox;
		private Enterprise.ZArchitecture.ZLabel PercentLabel1;
		private Enterprise.ZArchitecture.ZLabel DollarLabel3;
		private Enterprise.ZArchitecture.ZLabel DollarLabel2;
		protected Enterprise.ZArchitecture.ZCalcEdit UnitPriceCalcEdit;
		private Enterprise.ZArchitecture.ZLabel UnitChangeLabel;
		protected Enterprise.ZArchitecture.ZCalcEdit PercentageCalcEdit;
		private Enterprise.ZArchitecture.ZLabel PercentageChangeLabel;
		protected Enterprise.ZArchitecture.ZCalcEdit BasePriceCalcEdit;
		private Enterprise.ZArchitecture.ZLabel BaseChangeLabel;
		private Enterprise.ZArchitecture.ZLabel DollarLabel1;
		protected Enterprise.ZArchitecture.ZLabel MinimumChangeLabel;
		protected Enterprise.ZArchitecture.ZCalcEdit MinimumCalcEdit;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth MessageSubtypeDropEdit;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth MessageTypeDropEdit;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth EquipmentDropEdit;
		protected ZArchitecture.GUI.ZDropEditWithFixedWidth CalculationOrderDropDown;
		private Enterprise.ZArchitecture.ZLabel CalculationOrderLabel;
		protected Enterprise.ZArchitecture.ZCalcEdit UnitPercentageChangeCalcEdit;
		private Enterprise.ZArchitecture.ZLabel UnitPercentageChangeLabel;
		private Enterprise.ZArchitecture.ZLabel UnitPercentageChangePercentLabel;
	}
}
