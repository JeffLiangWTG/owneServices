using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class CommonPickupDeliveryConfirmDocumentSupporter : DocumentSupporter
	{
		public CommonPickupDeliveryConfirmDocumentSupporter(CommonPickupDeliveryConfirm confirm)
			: base(confirm)
		{
		}

		protected CommonPickupDeliveryConfirm Confirm
		{
			get { return (CommonPickupDeliveryConfirm)BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
					{
						Constants.DataContext.PickupDeliveryConfirm,
						Constants.DataContext.CommonCartageLeg,
						Constants.DataContext.ContainerLeg,
						Constants.DataContext.CartageAdvice,
						Constants.DataContext.Shipment
					};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.PickupDeliverConfirm; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Constants.DataContext.PickupDeliveryConfirm:
				case Constants.DataContext.CommonCartageLeg:
				case Constants.DataContext.ContainerLeg:
				case Constants.DataContext.CartageAdvice:
					{
						DocumentWrapper cartageLegWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.PickupDeliveryConfirm, Confirm);
						return new DocumentWrapper[] { cartageLegWrapper };
					}

				case Constants.DataContext.Shipment:
					List<DocumentWrapper> shipmentWrappers = new List<DocumentWrapper>();
					foreach (CommonShipment shipment in Confirm.Shipments)
					{
						DocumentShipment documentShipment = new DocumentShipment(shipment, Core.Constants.DataContext.Shipment);
						documentShipment.ConfirmationToPrint = Confirm;

						DocumentWrapper shipmentWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, documentShipment);
						shipmentWrappers.Add(shipmentWrapper);
					}

					return shipmentWrappers.ToArray();

				default:
					throw new ArgumentException("Invalid DataContext: " + dataContext);
			}
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.LocalTransport)
			{
				return Confirm.TransportProvider;
			}
			else
			{
				return base.GetOverriddenDeliveryDetails(menuName, contact, direction);
			}
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.LocalTransport)
			{
				if (Confirm.TransportProvider != null)
				{
					result = new OrgHeaderContact(Confirm.TransportProvider.Header, Confirm.TransportProvider);
				}
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}

			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == Constants.DataContext.Shipment)
			{
				if (!Confirm.Shipments.Any())
				{
					return Res.GetString("5a01c3c4-913f-4deb-b869-9c3e47db06c7", "This confirm is not associated with a shipment.", Confirm.HumanReadableShortcutName);
				}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Constants.DataContext.PickupDeliveryConfirm
				&& dataContext != Constants.DataContext.CommonCartageLeg
				&& dataContext != Constants.DataContext.ContainerLeg
				&& dataContext != Constants.DataContext.CartageAdvice
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion
	}
}
