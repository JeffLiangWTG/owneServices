using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDConsolWrapper : CMDWrapperBase
	{
		public CMDConsolWrapper(ForwardingConsol consol)
			: base(consol.Factory)
		{
			this.Consol = consol;
			HookEvents();
		}

		public override CMDShipmentWrapper[] CMDShipments
		{
			get
			{
				if (fCMDShipments == null)
				{
					List<CMDShipmentWrapper> list = new List<CMDShipmentWrapper>(Consol.Shipments.Count);
					if (Consol.IsExport() || Consol.IsImport())
					{
						foreach (ForwardingShipment shipment in Consol.Shipments)
						{
							if (shipment.IsAir)
							{
								list.Add(new CMDShipmentWrapper(shipment));
							}
						}
					}
					fCMDShipments = list.ToArray();
				}

				return fCMDShipments;
			}
		}

		#region Validation

		public override void RunPreSendValidation(INotifications notifications)
		{
			ValidateShipments(notifications);
			foreach (CMDShipmentWrapper shipmentWrapper in CMDShipments)
			{
				shipmentWrapper.RunPreSendValidation(notifications);
			}
		}

		public override void RunPreDeleteValidation(INotifications notifications)
		{
			ValidateShipments(notifications);
			foreach (CMDShipmentWrapper shipmentWrapper in CMDShipments)
			{
				shipmentWrapper.RunPreDeleteValidation(notifications);
			}
		}

		void ValidateShipments(INotifications notifications)
		{
			if (CMDShipments.Length < 1)
			{
				NotifyCMDError(notifications, "There is no Shipment to send/delete the CMD Messages for", Consol);
			}
		}

		#endregion

		#region Send / Delete CMD

		public override void SendMessage(INotifications notifications)
		{
			foreach (CMDShipmentWrapper shipmentWrapper in CMDShipments)
			{
				shipmentWrapper.SendMessage(notifications);
			}
		}

		public override void SendMessage(string recipient, INotifications notifications)
		{
			foreach (CMDShipmentWrapper shipmentWrapper in CMDShipments)
			{
				shipmentWrapper.SendMessage(recipient, notifications);
			}
		}

		public override void DeleteExistingCMDMessages(INotifications notifications)
		{
			foreach (CMDShipmentWrapper shipmentWrapper in CMDShipments)
			{
				shipmentWrapper.DeleteExistingCMDMessages(notifications);
			}
		}

		#endregion

		#region Implementation

		void HookEvents()
		{
			Consol.Shipments.CountChanged += Shipments_CountChanged;
		}

		void Shipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			RebuildShipmentWrapperArray();
		}

		void RebuildShipmentWrapperArray()
		{
			fCMDShipments = null;
		}

		CMDShipmentWrapper[] fCMDShipments;
		public readonly ForwardingConsol Consol;

		#endregion
	}
}
