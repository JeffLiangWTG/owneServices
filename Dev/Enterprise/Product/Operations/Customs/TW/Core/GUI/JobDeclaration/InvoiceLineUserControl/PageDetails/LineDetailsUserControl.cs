using System;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class LineDetailsUserControl : ZUserControl
	{
		public LineDetailsUserControl()
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.TW_CompositionsLongTextControl, "FilteredInvoiceLines.JI_Compositions");
			this.BindingSource.SetBindingMember(this.JI_NDescriptionLongTextControl, "FilteredInvoiceLines.JI_NDescription");
			this.BindingSource.SetBindingMember(this.TWGroupLongTextControl, "FilteredInvoiceLines.JI_Group");
			this.BindingSource.SetBindingMember(this.JI_DescriptionLongTextControl, "FilteredInvoiceLines.JI_Description");

			LabelCaptionRenderProvider.SetLabelCaptionVisible(JI_TariffDescriptionTextBox, false);
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

		Customs.Business.IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

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

		protected virtual void UpdateControlsVisiblity()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				if (declaration.IsAir)
				{
					UNDGCodeFindBox.CaptionResourceString = Res.GetData("6C116A25-B6D2-494C-843C-112101353D77", "IATA DG Code", "The standard classification code for dangerous goods.");
				}
				else
				{
					UNDGCodeFindBox.CaptionResourceString = Res.GetData("8B06C8B1-0F3A-4AD6-ADCC-F5654987D8AC", "UN DG Code", "The standard classification code for dangerous goods.");
				}
			}
		}
	}
}
