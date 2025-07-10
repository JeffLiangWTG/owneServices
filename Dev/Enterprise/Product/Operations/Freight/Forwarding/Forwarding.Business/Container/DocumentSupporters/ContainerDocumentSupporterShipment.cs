using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ContainerDocumentSupporterShipment : ContainerDocumentSupporter
	{
		public ContainerDocumentSupporterShipment(ForwardingContainer container)
			: base(container)
		{
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return Container.LinkedShipment != null ? new DocumentWrapper[] { DocumentWrapperFactory.CreateContainerWrapperWithShipment(Container, Container.LinkedShipment) } : new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Container, Container) };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.LocalTransport)
			{
				if (Container.LinkedShipment != null)
				{
					if (direction == DocumentDirection.ARV)
					{
						result = new OrgHeaderContact(Container.LinkedShipment.DocsAndCartage.DeliveryCartageCo, Container.LinkedShipment.DocsAndCartage.DeliveryCartageCoAddr);
					}
					else if (direction == DocumentDirection.DEP)
					{
						result = new OrgHeaderContact(Container.LinkedShipment.DocsAndCartage.PickupCartageCo, Container.LinkedShipment.DocsAndCartage.PickupCartageCoAddr);
					}
				}
			}
			return result;
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.LocalTransport && Container.LinkedShipment != null)
			{
				if (direction == DocumentDirection.ARV)
				{
					return Container.LinkedShipment.DocsAndCartage.DeliveryCartageCoAddr;
				}
				else
				{
					return Container.LinkedShipment.DocsAndCartage.PickupCartageCoAddr;
				}
			}
			else
			{
				return base.GetOverriddenDeliveryDetails(menuName, contact, direction);
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ForwardingContainer; }
		}

		public override string LocalPort(IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.LocalTransport)
			{
				if (direction == DocumentDirection.ARV)
				{
					return Container.LinkedShipment != null ? Container.LinkedShipment.JS_RL_NKDestination : ZString.Empty;
				}
				else if (direction == DocumentDirection.DEP)
				{
					return Container.LinkedShipment != null ? Container.LinkedShipment.JS_RL_NKOrigin : ZString.Empty;
				}
			}
			return base.LocalPort(contact, direction);
		}

		public override string ForeignPort(IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.LocalTransport)
			{
				if (direction == DocumentDirection.DEP)
				{
					return Container.LinkedShipment != null ? Container.LinkedShipment.JS_RL_NKDestination : ZString.Empty;
				}
				else if (direction == DocumentDirection.ARV)
				{
					return Container.LinkedShipment != null ? Container.LinkedShipment.JS_RL_NKOrigin : ZString.Empty;
				}
			}
			return base.ForeignPort(contact, direction);
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.CartageAdvice
				&& dataContext != Core.Constants.DataContext.Service
				&& dataContext != Core.Constants.DataContext.GenericFreightJob
				&& dataContext != Core.Constants.DataContext.GenericFreightJobServices
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#region Branding

		public override IOrgHeader GetBrandedOrganisation(IContactType contactType, DocumentDirection direction)
		{
			IOrgHeader result = null;
			ContactType concreteContactType = contactType as ContactType;

			if (Container.LinkedShipment != null && concreteContactType != null && concreteContactType.BrandingType == ContactBrandingType.Client)
			{
				result = (direction == DocumentDirection.DEP) ? Container.LinkedShipment.Consignor : Container.LinkedShipment.Consignee;
			}

			return result;
		}

		#endregion
	}
}
