using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSupplierBuyerLinksForm : ZChildForm
	{
		public OrgSupplierBuyerLinksForm()
			: base()
		{
			DialogResult = DialogResult.Cancel;
		}

		public OrgSupplierBuyerLinksForm(OrgSupplierBuyerLinkCollectionReadOnlyView buyersSuppliers, OrgHeaderDocumentSupporter.SuppliersBuyersType orgType)
			: base(buyersSuppliers)
		{
			DialogResult = DialogResult.Cancel;
			this.OrgType = orgType;
			ShowHideColumns();
		}

		void ShowHideColumns()
		{
			BuyerSupplierGrid.GetColumnStyle(OrgSupplierBuyerLink.Schema.OL_OH_Supplier).IsVisible = OrgType == OrgHeaderDocumentSupporter.SuppliersBuyersType.Suppliers;
			BuyerSupplierGrid.GetColumnStyle(OrgSupplierBuyerLink.Schema.OL_OH_Buyer).IsVisible = OrgType == OrgHeaderDocumentSupporter.SuppliersBuyersType.Buyers;
			BuyerSupplierGrid.RefreshTableStyles();
		}

		readonly OrgHeaderDocumentSupporter.SuppliersBuyersType OrgType;

		public override string FormCaption
		{
			get { return OrgType == OrgHeaderDocumentSupporter.SuppliersBuyersType.Suppliers ? Res.GetString("OrgSupplierBuyerLinksForm|SupplierLinks", "Supplier Links") : Res.GetString("OrgSupplierBuyerLinksForm|BuyerLinks", "Buyer Links"); }
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			OrgSupplierBuyerLinkDependentCollection relationships = ((OrgSupplierBuyerLinkCollectionReadOnlyView)BusinessEntity).SupplierBuyerCollection;
			OrgSupplierBuyerLink firstRelationshipWithError = relationships.FindFirstRelationshipWithError();

			if (firstRelationshipWithError != null)
			{
				var msgBox = new ZErrorMessageBox(firstRelationshipWithError);
				msgBox.Text = Res.GetString("52335d53-0aaf-4035-a5b5-903a91e3461e", "Cannot Print Document...");
				msgBox.Message = Res.GetString("7827ec6a-008d-47e0-bde9-0c859e32785e", "There are errors that need to be corrected before the Routing Order can be printed.");
				ZFormModaliser.ShowDialogAndDispose(msgBox);
			}
			else
			{
				if (SetShipmentDatesCheckBox.Checked)
				{
					relationships.SetNewDateOnFirstExpectedShipment();
				}
				relationships.ResetExpectedShipmentMonths();

				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelOrderButton_Click(object sender, System.EventArgs e)
		{
			OrgSupplierBuyerLinkDependentCollection relationships = ((OrgSupplierBuyerLinkCollectionReadOnlyView)BusinessEntity).SupplierBuyerCollection;
			relationships.ResetExpectedShipmentMonths();
		}
	}
}
