namespace Enterprise.Customs.NZ.Module
{
	partial class CusClassificationFilterControl
	{
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("3bbdd354-950d-4759-a3e5-ccb91bb95c54", "Other Info Codes");
			zTextBoxColumnStyleInfo1.ColumnName = "CC_AggregatedOtherInfoCodes";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("0831d161-8ce4-4797-8c0f-6ce350bd37a8", "Permit Codes");
			zTextBoxColumnStyleInfo2.ColumnName = "CC_AggregatedPermitCodes";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("3ebe8ce3-030f-49fc-aee3-e581dc73d57d", "Prohibited Codes");
			zTextBoxColumnStyleInfo3.ColumnName = "CC_AggregatedProhibitedCodes";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("d680e6dd-530c-4b19-acfe-b24049bf8cee", "Concession Code");
			zTextBoxColumnStyleInfo4.ColumnName = "CC_ConcessionCode";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("f6e47c13-ca0b-474e-92e9-ad2fb56ea031", "Parts Of Classification");
			zTextBoxColumnStyleInfo5.ColumnName = "CC_PartsOfClassification";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 137, true);
			this.FilteredGrid.TabIndex = 12;
			// 
			// CusClassificationFilterControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.MasterFiles.CusClassification";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 310, true);
			this.Name = "CusClassificationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 310, true);
			this.Controls.SetChildIndex(this.FilteredGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.ComponentModel.Container components;
	}
}
