using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgSupplierBuyersLinksFormForTest : OrgSupplierBuyerLinksForm
	{
		public OrgSupplierBuyersLinksFormForTest(OrgSupplierBuyerLinkCollectionReadOnlyView buyersSuppliers, OrgHeaderDocumentSupporter.SuppliersBuyersType orgType)
			: base(buyersSuppliers, orgType)
		{
		}

		public ZButton OKButton_Exposed
		{
			get { return base.OKButton; }
		}

		public ZButton CancelButton_Exposed
		{
			get { return base.CancelPrintButton; }
		}

		public ZDropEdit MonthsDropEdit_Exposed
		{
			get { return base.MonthsDropEdit; }
		}
	}
}
