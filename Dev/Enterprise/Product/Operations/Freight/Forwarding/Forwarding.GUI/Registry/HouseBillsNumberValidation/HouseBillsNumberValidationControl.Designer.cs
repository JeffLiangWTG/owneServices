namespace Enterprise.Freight.Forwarding.GUI
{
	partial class HouseBillsNumberValidationControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.HouseBillsNumberValidationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsNumberValidationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidationCollection);
			// 
			// HouseBillsNumberValidationGrid
			// 
			this.HouseBillsNumberValidationGrid.AllowNavigation = false;
			this.HouseBillsNumberValidationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HouseBillsNumberValidationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).HBLPrefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).IncludeHBLPrefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).HBLSuffix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).HBLLengthFormatted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.HouseBillsNumberValidation)(null)).CheckDigitAlgorithm)));
			this.HouseBillsNumberValidationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7ba4fbaf-8e90-4b64-b403-9f40e5dd7898", "Mode", "Transport Mode", "");
			zDropEditColumnStyleInfo1.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3862df95-b9a5-4359-8d2a-b4a58cfc52f4", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Origin";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("989f1212-2008-43e1-ae31-c8bdc3fa3a09", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Destination";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2c9ca841-2f40-4a56-b3cf-05768f3fc6d9", "HBL Prefix");
			zTextBoxColumnStyleInfo1.ColumnName = "HBLPrefix";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("975e5715-39ac-406a-b6dc-5df9ff93d012", "Include Prefix", "Include Prefix in check digit algorithm", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeHBLPrefix";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("306d7b28-1c02-4821-8fd8-cc9ac855b95d", "HBL Suffix");
			zTextBoxColumnStyleInfo2.ColumnName = "HBLSuffix";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d5009cdd-cc70-44b1-b220-3491490fb2fd", "HBL Length", "Length of HBL number (excluding prefix and suffix)", "");
			zTextBoxColumnStyleInfo3.ColumnName = "HBLLengthFormatted";
			zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b62f7639-519b-411a-a532-9b4f763b519b", "Algorithm", "Check Digit Algorithm", "");
			zDropEditColumnStyleInfo2.ColumnName = "CheckDigitAlgorithm";
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.HouseBillsNumberValidationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.HouseBillsNumberValidationGrid.CopySelectedRowsAllowed = true;
			this.HouseBillsNumberValidationGrid.GridId = "0628b79a-aa94-493a-9668-276611257a9c";
			this.HouseBillsNumberValidationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseBillsNumberValidationGrid.LayoutKey = "HouseBillsNumberValidationGrid";
			this.HouseBillsNumberValidationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillsNumberValidationGrid.Name = "HouseBillsNumberValidationGrid";
			this.HouseBillsNumberValidationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 218, true);
			this.HouseBillsNumberValidationGrid.TabIndex = 0;
			// 
			// HouseBillsNumberValidationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.HouseBillsNumberValidationGrid);
			this.Name = "HouseBillsNumberValidationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 221, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsNumberValidationGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid HouseBillsNumberValidationGrid;
	}
}
