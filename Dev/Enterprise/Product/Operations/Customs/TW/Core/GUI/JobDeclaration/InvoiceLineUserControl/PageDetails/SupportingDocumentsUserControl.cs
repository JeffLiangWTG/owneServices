using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class SupportingDocumentsUserControl : ZUserControl
	{
		public SupportingDocumentsUserControl()
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
				var isImport = declaration.IsImport;
				QuotaPermitNumberTextBox.Visible = isImport;
				QuotaPermitNumberItemNumberCalcEdit.Visible = isImport;
				HighTechLicenseTextBox.Visible = isImport;
				CitesPermitTextBox.Visible = isImport;
				ImportExportRegulationsGroupBox.CaptionResourceString = isImport ? Res.GetData("d0b10402-b6de-453a-aca8-4d536cff56f7", "Import Regulations") : Res.GetData("04aea241-4fd6-4d16-acc5-d1e78bb98a2a", "Export Regulations");
			}
		}
	}
}
