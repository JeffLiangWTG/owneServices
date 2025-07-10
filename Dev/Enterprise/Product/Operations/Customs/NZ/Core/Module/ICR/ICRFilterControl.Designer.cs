namespace Enterprise.Customs.NZ.Module
{
	partial class ICRFilterControl
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
			this.SuspendLayout();
			// 
			// ICRFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ICRFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 202, true);
			this.ResumeLayout(false);

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("3ce9a0bc-e9b7-4c75-9833-7035c180b7b6", "Consol ID");
			zTextBoxColumnStyleInfo1.ColumnName = "CE_ConsolID";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("fa669b20-70b0-454d-a0ed-025b8b99f2bd", "MAWB");
			zTextBoxColumnStyleInfo2.ColumnName = "CE_MasterBillNumber";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("6286edb5-548e-409d-a685-f3d36a8bb390", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "CE_EntryStatus";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("501f1c53-db54-4bb6-9d65-a2838479becb", "Desc.");
			zTextBoxColumnStyleInfo4.ColumnName = "CE_StatusDescription";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("18f1d069-9db5-4b90-8ada-eea9cf581fc7", "Clearance No");
			zTextBoxColumnStyleInfo5.ColumnName = "CE_EntryNum";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 413, true);
			this.FilteredGrid.TabIndex = 5;
			// 
			// OutwardReportFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.TradeSingleWindow.InwardCargoReport.CusEntryNumber";
			this.Name = "ICRFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
