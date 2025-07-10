using System;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusRefTariffVersionForm : ZTemplateForm
	{
		public CusRefTariffVersionForm(CusRefTariffVersion businessEntity)
			: base(businessEntity)
		{
		}

		public CusRefTariffVersionForm()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object.");
			}
		}

		public new CusRefTariffVersion BusinessEntity => (CusRefTariffVersion)base.BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption => Res.GetString("CusRefTariffVersionForm|FormCaption", "Tariff Version Form");

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;
	}
}
