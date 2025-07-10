using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class OtherDetailsUserControl : ZUserControl
	{
		public OtherDetailsUserControl()
		{
			InitializeComponent();
		}

		public new Customs.Business.IInvoicesProvider CurrentDataItem => base.CurrentDataItem as Customs.Business.IInvoicesProvider;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			declarationValueChangedAnnouncer_OnValueChanged(this, null);
			declarationValueChangedAnnouncer = CurrentDataItem?.GetValueChangedAnnouncer();
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged += declarationValueChangedAnnouncer_OnValueChanged;
			}
		}
		IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged -= declarationValueChangedAnnouncer_OnValueChanged;
				declarationValueChangedAnnouncer.Dispose();
			}
		}

		void declarationValueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
		{
			UpdateControlsVisiblity();
		}

		void UpdateControlsVisiblity()
		{
			if (CurrentDataItem is JobDeclaration declaration)
			{
				var isExport = declaration.IsExport;
				EnvironmentalProtectionTariffGroupBox.Visible = !isExport;
				TW_TpfPymntMthdDropEdit.Visible = isExport;
				SetManufacturerAddressAddressControlCaption(isExport);
			}
		}

		void SetManufacturerAddressAddressControlCaption(bool isExport)
		{
			if (isExport)
			{
				ManuFacturerAddressControl.CaptionResourceString = Res.GetData("dd0703e7-5ceb-487d-ad5c-3f2e6b15dad7", "Manuf.", "Manufacturer", "The name and VAT number of the manufacturer.");
			}
			else
			{
				ManuFacturerAddressControl.CaptionResourceString = Res.GetData("28b340c8-5afd-474c-bad5-f422e9143086", "Foreign Manuf.", "Foreign Manufacturer", "The code of foreign manufacturer issued by the foreign authority.");
			}
		}
	}
}
