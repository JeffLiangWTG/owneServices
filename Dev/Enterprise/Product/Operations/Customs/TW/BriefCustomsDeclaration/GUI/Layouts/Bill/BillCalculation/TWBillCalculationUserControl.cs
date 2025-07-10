using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public partial class TWBillCalculationUserControl : ZUserControl, IAdditionalTabPage, ISupportMultipleResourceStringDataSupporter
	{
		public TWBillCalculationUserControl()
		{
			InitializeComponent();
			GoodsValueConvertToLocalCurrencyControl.SetCurrencyCodeFindBoxReadOnly(true);
		}

		#region IAdditionalTabPage
		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("9D9B783A-AF9B-4C47-BDC7-5AAAF1FC96C6", "Calculation");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 2;
		#endregion

		bool IsImport => DataSource is AsycudaManifestHeader header && header.IsImport;

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => (AsycudaBill)CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is AsycudaManifestHeader header)
			{
				header.AMA_NatureInfo.ValueChanged -= AMA_NatureInfo_ValueChanged;
				header.AMA_NatureInfo.ValueChanged += AMA_NatureInfo_ValueChanged;
				AMA_NatureInfo_ValueChanged(null, null);
			}
		}

		void AMA_NatureInfo_ValueChanged(object sender, EventArgs e)
		{
			DutiesTaxesAndFeesGroupBox.Visible = IsImport;
			CustomsValueConvertToLocalCurrencyControl.UpdateCaption();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			if (DataSource is AsycudaManifestHeader header)
			{
				header.AMA_NatureInfo.ValueChanged -= AMA_NatureInfo_ValueChanged;
			}
			base.Dispose(disposing);
		}
	}
}
