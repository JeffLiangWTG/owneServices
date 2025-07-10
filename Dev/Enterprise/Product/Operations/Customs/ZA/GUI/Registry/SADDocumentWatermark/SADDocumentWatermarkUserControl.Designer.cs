namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	partial class SADDocumentWatermarkUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            this.MainGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainGrid)).BeginInit();
            this.MainGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.DataRegistry.Business.SADDocumentWatermarkCollection);
            // 
            // MainGrid
            // 
            this.MainGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.MainGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.DataRegistry.Business.SADDocumentWatermark)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.DataRegistry.Business.SADDocumentWatermark)(null)).EntryStatusCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.DataRegistry.Business.SADDocumentWatermark)(null)).EntryStatusDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.DataRegistry.Business.SADDocumentWatermark)(null)).WatermarkText)));
            this.MainGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.Caption = "";
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("b6961490-dd2b-430b-bea1-9aba0a4d939f", "Entry Status Code");
            zDropEditColumnStyleInfo1.ColumnName = "EntryStatusCode";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
            zTextBoxColumnStyleInfo1.Caption = "";
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("1bee4c27-5c99-42d7-9aff-9f9de9eaa00d", "Entry Status Description");
            zTextBoxColumnStyleInfo1.ColumnName = "EntryStatusDescription";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(312);
            zMultiLineTextBoxColumnInfo1.Caption = "";
            zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("b6d5f607-da67-4ffa-8f02-0b9425175880", "Watermark Text");
            zMultiLineTextBoxColumnInfo1.ColumnName = "WatermarkText";
            zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
            zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
            this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.MainGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
            this.MainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainGrid.GridId = "8053A9E0-DCFF-4230-A627-F310D681A0F9";
            this.MainGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.MainGrid.LayoutKey = "MainGrid";
            this.MainGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainGrid.Name = "MainGrid";
            this.MainGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 313, true);
            this.MainGrid.TabIndex = 0;
            // 
            // SADDocumentWatermarkUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.MainGrid);
            this.Name = "SADDocumentWatermarkUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 313, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainGrid)).EndInit();
            this.MainGrid.ResumeLayout(false);
            this.MainGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid MainGrid;
	}
}
