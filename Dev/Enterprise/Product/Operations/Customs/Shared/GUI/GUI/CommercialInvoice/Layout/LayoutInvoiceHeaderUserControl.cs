namespace Enterprise.Customs.GUI.CommercialInvoice
{
	public partial class LayoutInvoiceHeaderUserControl : CommonInvoiceHeaderUserControl
	{
		public LayoutInvoiceHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override void ChangeControlsVisibilityWhenMessageTypeChanges()
		{
			base.ChangeControlsVisibilityWhenMessageTypeChanges();
			SetInvoiceFormLayout();
		}

		void SetInvoiceFormLayout()
		{
			if (CommercialInvoiceFormLayoutProvider != null)
			{
				SetHeaderDetailsLayout();
			}
		}

		void SetHeaderDetailsLayout()
		{
			var newHeaderDetailLayout = CommercialInvoiceFormLayoutProvider?.GetInvoiceHeaderDetailsLayout(Invoice);
			if (newHeaderDetailLayout != null)
			{
				HeaderDetailsUserControl.SetInvoiceHeaderDetailsLayout(newHeaderDetailLayout);
			}
		}

		ICommercialInvoiceFormLayoutProvider CommercialInvoiceFormLayoutProvider
		{
			get
			{
				if (commercialInvoiceFormLayoutProvider == null)
				{
					commercialInvoiceFormLayoutProvider = GUI.CommercialInvoiceFormLayoutProvider.GetLayoutProvider(Invoice);
				}
				return commercialInvoiceFormLayoutProvider;
			}
		}
		ICommercialInvoiceFormLayoutProvider commercialInvoiceFormLayoutProvider;
	}
}
