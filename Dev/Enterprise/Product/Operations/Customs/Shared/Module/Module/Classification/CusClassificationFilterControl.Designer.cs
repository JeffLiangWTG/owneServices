namespace Enterprise.Customs.Module
{
	partial class CusClassificationFilterControl<T>
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CC_LookupCode";
			zTextBoxColumnStyleInfo2.ColumnName = "CC_TariffNum";
			zTextBoxColumnStyleInfo3.ColumnName = "CC_Description";
			zCheckBoxColumnStyleInfo1.ColumnName = "CC_IsActive";
			zTextBoxColumnStyleInfo4.ColumnName = "CC_LastAuditedUser";
			zTextBoxColumnStyleInfo5.ColumnName = "CC_ClassificationType";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("992FAEB7-4609-44D4-91D7-749EBADE29B6", "Classification Type");
			zDateEditColumnStyleInfo1.ColumnName = "CC_LastAuditedDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 174, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 277, true);
			this.FilteredGrid.TabIndex = 8;
			// 
			// CusClassificationFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.BaseCusClassification";
			this.Name = "CusClassificationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 451, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
