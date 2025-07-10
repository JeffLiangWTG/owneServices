using System;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingAdditionalDetailsPage : ZUserControl
	{
		public BillOfLadingAdditionalDetailsPage()
		{
			InitializeComponent();
		}

		BillOfLading Shipment
		{
			get { return CurrentDataItem as BillOfLading; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			customsDetailPanel.Visible = (Shipment != null && Shipment.ShouldShowCustomsDetail);
		}

		void OrderReferencesButton_Click(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				OrderItemCollectionForm.ShowDialog(Shipment.DocsAndCartage.OrderItems);
			}
		}
	}
}


