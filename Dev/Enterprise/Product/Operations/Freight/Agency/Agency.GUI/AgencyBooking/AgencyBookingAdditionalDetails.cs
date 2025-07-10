using System;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class AgencyBookingAdditionalDetails : ZUserControl
	{
		public AgencyBookingAdditionalDetails()
		{
			InitializeComponent();
		}

		void ConsignorDocumentaryDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (Shipment != null && !Shipment.ConsignorDocumentaryAddress.ReadOnly && Shipment.BuyerSupplierLinksHelper.ShouldShowRelatedConsignors)
			{
				ConsignorDocumentaryDocAddressControl.SelectFromPopupForm();
			}
		}

		void ConsigneeDocumentaryDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (Shipment != null && !Shipment.ConsigneeDocumentaryAddress.ReadOnly && Shipment.BuyerSupplierLinksHelper.ShouldShowRelatedConsignees)
			{
				ConsigneeDocumentaryDocAddressControl.SelectFromPopupForm();
			}
		}

		AgencyShipment Shipment
		{
			get { return (AgencyShipment)CurrentDataItem; }
		}
	}
}
