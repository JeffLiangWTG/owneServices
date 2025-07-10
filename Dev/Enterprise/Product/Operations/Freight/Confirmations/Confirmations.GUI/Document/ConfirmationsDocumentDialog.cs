using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI
{
	public class ConfirmationsDocumentDialog : IConfirmationsDocumentDialog
	{
		public bool ShowDialogDisposeAndContinue(BusinessObjectCollection confirmsCollection, BusinessObject commonShipment)
		{
			if (!typeof(DocumentPickupDeliveryConfirmCollection).IsAssignableFrom(confirmsCollection.GetType()))
			{
				throw new ArgumentException($"{nameof(confirmsCollection)} needs to be of type DocumentPickupDeliveryConfirmCollection"); //temp
			}

			if (!typeof(CommonShipment).IsAssignableFrom(commonShipment.GetType()))
			{
				throw new ArgumentException($"{nameof(commonShipment)} needs to be of type CommonShipment"); //temp
			}

			return ZFormModaliser.ShowDialogAndDispose(new DocumentPickupDeliveryConfirmForm((DocumentPickupDeliveryConfirmCollection)confirmsCollection, (CommonShipment)commonShipment)) == DialogResult.Yes;
		}
	}
}
