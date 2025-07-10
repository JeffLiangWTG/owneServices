using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public class MainFormForwardingConsolCollection : MainFormConsolCollection, IFilterModuleExtraNotificationProvider, IUseParentGridContext
	{
		public MainFormForwardingConsolCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MainFormForwardingConsolCollection(BusinessObjectFactory factory, ForwardingShipment shipment)
			: base(factory)
		{
			ParentShipment = shipment;
		}

		#region Properties

		public new ForwardingConsol this[int index]
		{
			get { return (ForwardingConsol)Elements[index]; }
		}

		#endregion

		#region Implementation

		public new ForwardingConsol AddNew()
		{
			return (ForwardingConsol)base.AddNew();
		}

		#endregion

		#region IFilterModuleExtraNotificationProvider Members

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			ForwardingConsol consol = businessObject as ForwardingConsol;
			if (consol != null && ParentShipment != null)
			{
				using
				(
					new DisposableAction(
						() => FreightShipmentVsConsolMessageHelper.Instance.IsGatewayServiceLevelCheckSuspended = true,
						() => FreightShipmentVsConsolMessageHelper.Instance.IsGatewayServiceLevelCheckSuspended = false)
				)
				{
					IShipmentConsolAttachRequest attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachConsol(ParentShipment, consol);
					if (!attachRequest.Errors.IsEmpty)
					{
						return new Notification(CargoWise.ComponentModel.NotificationType.Error, attachRequest.Errors);
					}
					else if (!attachRequest.Warnings.IsEmpty)
					{
						return new Notification(CargoWise.ComponentModel.NotificationType.Warning, attachRequest.Warnings);
					}
				}
			}

			return null;
		}

		#endregion

		#region IUseParentGridContext

		public Type ParentType => typeof(ForwardingModuleConsol);

		#endregion
	}
}
