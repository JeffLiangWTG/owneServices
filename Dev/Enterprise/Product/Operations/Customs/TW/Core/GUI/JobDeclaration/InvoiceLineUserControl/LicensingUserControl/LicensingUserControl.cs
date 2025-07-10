using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class LicensingUserControl : ZUserControl
	{
		public LicensingUserControl()
		{
			InitializeComponent();
		}

		public void SetControllingMsgTabPagesVisibilityForCAHeader(JobComInvoiceLine invoiceLine)
		{
			CommonTabPage.TabVisible = invoiceLine.HasLinkedCMHeader;
			AnimalAndPlantTabPage.TabVisible = invoiceLine.AnimalAndPlantSupported;
			CertificateOfOriginTabPage.TabVisible = invoiceLine.CertificateOfOriginSupported;
			AlcoholTabPage.TabVisible = invoiceLine.AlcoholSupported;
			TypeApprovalTabPage.TabVisible = invoiceLine.TypeApprovalSupported;
			FoodAndDrugTabPage.TabVisible = invoiceLine.FoodAndDrugSupported;
		}
	}
}
