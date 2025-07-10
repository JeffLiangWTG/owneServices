namespace Enterprise.Freight.Forwarding.GUI.Registry
{
	partial class PreAllocationCheckRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ChecksGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChecksGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Registry.PreAllocationCheckCollection);
			// 
			// ChecksGrid
			// 
			this.ChecksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChecksGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PreAllocationCheck)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.PreAllocationCheck)(null)).Measure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.PreAllocationCheck)(null)).Action)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Registry.PreAllocationCheck)(null)).Percentage)));
			this.ChecksGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("767bf0f7-c124-49e6-9269-1b69146087d9", "Measure");
			zTextBoxColumnStyleInfo1.ColumnName = "MeasureMultilingualString";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5e0700d9-4304-4482-ae23-4f2bc84edbf7", "Action");
			zDropEditColumnStyleInfo1.ColumnName = "Action";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("58585417-cdd5-4df6-b283-3b2617046ccc", "Percentage");
			zCalcEditColumnStyleInfo1.ColumnName = "Percentage";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			this.ChecksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChecksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChecksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChecksGrid.CopySelectedRowsAllowed = true;
			this.ChecksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChecksGrid.GridId = "5c5f4987-182f-4578-988e-f4465b7b0a20";
			this.ChecksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChecksGrid.LayoutKey = "ChecksGrid";
			this.ChecksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChecksGrid.Name = "ChecksGrid";
			this.ChecksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.ChecksGrid.TabIndex = 0;
			// 
			// PreAllocationCheckRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChecksGrid);
			this.Name = "PreAllocationCheckRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChecksGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid ChecksGrid;
	}
}
